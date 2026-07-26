using Common.Application.Extensions;
using Common.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Common.Application.Tests.Extensions;

public class ModuleDiscoveryExtensionsTests
{
    public interface IAlphaMarker;
    public interface IBetaMarker;

    public sealed class AlphaModule : IServiceModule
    {
        public void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(Mock.Of<IAlphaMarker>());
            services.AddSingleton(configuration);
        }
    }

    public sealed class BetaModule : IServiceModule
    {
        public void Register(IServiceCollection services, IConfiguration configuration)
            => services.AddSingleton(Mock.Of<IBetaMarker>());
    }

    // Must be skipped by discovery: abstract implementations are not instantiable modules.
    public abstract class AbstractModule : IServiceModule
    {
        public abstract void Register(IServiceCollection services, IConfiguration configuration);
    }

    [Fact]
    public void AddDiscoveredModules_RegistersEveryConcreteModuleInTheAssembly()
    {
        var services = new ServiceCollection();
        var configuration = Mock.Of<IConfiguration>();

        services.AddDiscoveredModules(typeof(ModuleDiscoveryExtensionsTests).Assembly, configuration);

        services.Should().Contain(d => d.ServiceType == typeof(IAlphaMarker));
        services.Should().Contain(d => d.ServiceType == typeof(IBetaMarker));
    }

    [Fact]
    public void AddDiscoveredModules_PassesTheConfigurationThrough()
    {
        var services = new ServiceCollection();
        var configuration = Mock.Of<IConfiguration>();

        services.AddDiscoveredModules(typeof(ModuleDiscoveryExtensionsTests).Assembly, configuration);

        services.Single(d => d.ServiceType == typeof(IConfiguration))
            .ImplementationInstance.Should().BeSameAs(configuration);
    }

    [Fact]
    public void AddDiscoveredModules_AssemblyWithoutModules_RegistersNothing()
    {
        var services = new ServiceCollection();

        services.AddDiscoveredModules(typeof(string).Assembly, Mock.Of<IConfiguration>());

        services.Should().BeEmpty();
    }
}
