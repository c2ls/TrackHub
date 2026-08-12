// Copyright (c) 2026 Sergio Hernandez. All rights reserved.
//
//  Licensed under the Apache License, Version 2.0 (the "License").
//  You may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//

using System.Reflection;
using Common.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using TrackHub.Router.Domain.Interfaces.Operator;

namespace TrackHub.Router.Infrastructure.Common.Helpers;

// Registers each provider's reader/tester types KEYED by their ProtocolType, so the registries
// resolve exactly one implementation per protocol from the caller's own scope — no O(N)
// resolve-all-and-filter, no disposed-scope hand-off (router-audit A-07). All providers register
// through one uniform path (the fake-async adapters were removed — provider Init is a real Task,
// router-audit A-18).
public static class ProtocolRegistrationExtensions
{
    public static IServiceCollection RegisterProtocol(
        this IServiceCollection services,
        string protocolName)
    {
        if (!Enum.TryParse<ProtocolType>(protocolName, ignoreCase: true, out var protocol))
        {
            throw new InvalidOperationException(
                $"Configured protocol '{protocolName}' (AppSettings:Protocols) is not a known "
                + $"{nameof(ProtocolType)} value.");
        }

        var protocolAssembly = GetProtocolAssembly(protocolName);
        if (protocolAssembly is null)
        {
            // Fail fast: a configured protocol whose provider assembly cannot be found is a
            // deployment/config error, not something to skip silently (a silent skip left the
            // protocol with no reader and produced a masked "Unexpected Execution Error" at the
            // first sync — the class of bug that hid Geotab, see router-audit A-01/A-06).
            throw new InvalidOperationException(
                $"Configured protocol '{protocolName}' (AppSettings:Protocols) has no provider "
                + $"assembly 'TrackHub.Router.Infrastructure.{protocolName}'. Build the provider "
                + "project or remove the protocol from configuration.");
        }

        // The enum/config spelling and the assembly namespace can differ in casing
        // (e.g. config "GeoTab" vs namespace "Geotab"); resolve the assembly's actual root
        // namespace and match types case-insensitively so casing drift never silently unregisters
        // a provider.
        var protocolNamespace = ResolveProtocolNamespace(protocolAssembly, protocolName);

        var registered = RegisterReaders(services, protocolAssembly, protocolNamespace, protocol);

        if (!registered)
        {
            throw new InvalidOperationException(
                $"Configured protocol '{protocolName}' resolved assembly "
                + $"'{protocolAssembly.GetName().Name}' but no DeviceReader/PositionReader/"
                + "ConnectivityTester implementing the protocol interfaces was found. Check the "
                + "provider's type names and namespace.");
        }

        return services;
    }

    private static Type? GetProtocolType(Assembly assembly, string protocolNamespace, string typeName)
        // ignoreCase: true — the config/enum spelling may differ in casing from the namespace.
        => assembly.GetType($"{protocolNamespace}.{typeName}", throwOnError: false, ignoreCase: true);

    private static bool RegisterReaders(
        IServiceCollection services,
        Assembly assembly,
        string protocolNamespace,
        ProtocolType protocol)
    {
        var deviceReaderType = GetProtocolType(assembly, protocolNamespace, "DeviceReader");
        var positionReaderType = GetProtocolType(assembly, protocolNamespace, "PositionReader");
        var connectivityTesterType = GetProtocolType(assembly, protocolNamespace, "ConnectivityTester");
        var registered = false;

        if (deviceReaderType is not null && typeof(IExternalDeviceReader).IsAssignableFrom(deviceReaderType))
        {
            services.AddKeyedTransient(typeof(IExternalDeviceReader), protocol, deviceReaderType);
            registered = true;
        }

        if (positionReaderType is not null && typeof(IPositionReader).IsAssignableFrom(positionReaderType))
        {
            services.AddKeyedTransient(typeof(IPositionReader), protocol, positionReaderType);
            registered = true;
        }

        if (connectivityTesterType is not null && typeof(IConnectivityTester).IsAssignableFrom(connectivityTesterType))
        {
            services.AddKeyedTransient(typeof(IConnectivityTester), protocol, connectivityTesterType);
            registered = true;
        }

        return registered;
    }

    // The assembly's real root namespace for the provider types, matched case-insensitively so a
    // config/enum spelling (e.g. "GeoTab") resolves the actual namespace (e.g. "Geotab").
    private static string ResolveProtocolNamespace(Assembly assembly, string protocolName)
    {
        var expected = $"TrackHub.Router.Infrastructure.{protocolName}";
        var actual = SafeGetTypes(assembly)
            .Select(t => t.Namespace)
            .FirstOrDefault(ns => ns is not null
                && ns.Equals(expected, StringComparison.OrdinalIgnoreCase));
        return actual ?? expected;
    }

    private static Assembly? GetProtocolAssembly(string protocolName)
    {
        var expectedNamespace = $"TrackHub.Router.Infrastructure.{protocolName}";

        // First, check if the assembly is already loaded by looking for types in the expected
        // namespace (case-insensitive) that implement our protocol interfaces.
        var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => !a.IsDynamic &&
                SafeGetTypes(a).Any(t =>
                    t.Namespace?.StartsWith(expectedNamespace, StringComparison.OrdinalIgnoreCase) == true &&
                    (typeof(IExternalDeviceReader).IsAssignableFrom(t) ||
                     typeof(IPositionReader).IsAssignableFrom(t) ||
                     typeof(IConnectivityTester).IsAssignableFrom(t))));

        if (loadedAssembly is not null)
        {
            return loadedAssembly;
        }

        // If not loaded, try to load by expected assembly name (simple names are case-insensitive).
        try
        {
            return Assembly.Load($"TrackHub.Router.Infrastructure.{protocolName}");
        }
        catch
        {
            return null;
        }
    }

    // A partially-loadable assembly can throw ReflectionTypeLoadException from GetTypes(); recover
    // the types that did load rather than letting one bad assembly abort protocol discovery.
    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t is not null)!;
        }
    }
}
