namespace TrackHub.Router.Infrastructure.ManagerApi;

public class DeviceSyncWriter(IGraphQLClientFactory graphQLClient)
    : GraphQLService(graphQLClient.CreateClient(Clients.Manager)), IDeviceSyncWriter
{
    internal const string WipeDevicesMutation = @"
                mutation($operatorId: UUID!) {
                    wipeDevices(operatorId: $operatorId)
                }";

    internal const string SynchronizeOperatorDevicesMutation = @"
                mutation($command: SynchronizeOperatorDevicesCommandInput!) {
                    synchronizeOperatorDevices(command: $command) {
                        devicesSeen
                        devicesAdded
                        devicesUpdated
                        devicesRemoved
                        devicesIgnored
                    }
                }";

    public async Task ResetAsync(Guid accountId, Guid operatorId, CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = WipeDevicesMutation,
            Variables = new { operatorId }
        };
        await MutationAsync<object>(request, cancellationToken);
    }

    public async Task<DeviceSyncCountsVm> SynchronizeAsync(
        Guid accountId,
        Guid operatorId,
        IEnumerable<SynchronizedDeviceDto> devices,
        string correlationId,
        string triggerType,
        bool autoAssignNewDevices,
        CancellationToken cancellationToken)
    {
        var request = new GraphQLRequest
        {
            Query = SynchronizeOperatorDevicesMutation,
            Variables = new
            {
                command = new
                {
                    accountId,
                    operatorId,
                    correlationId,
                    triggerType,
                    autoAssignNewDevices,
                    devices = devices.Select(d => new
                    {
                        accountId = d.AccountId,
                        operatorId = d.OperatorId,
                        serial = d.Serial,
                        name = d.Name,
                        identifier = d.Identifier,
                        providerDisplayName = d.ProviderDisplayName,
                        deviceTypeId = d.DeviceTypeId,
                        description = d.Description,
                        providerMetadataHash = d.ProviderMetadataHash,
                        providerStatus = d.ProviderStatus
                    }).ToArray()
                }
            }
        };
        return await MutationAsync<DeviceSyncCountsVm>(request, cancellationToken);
    }
}
