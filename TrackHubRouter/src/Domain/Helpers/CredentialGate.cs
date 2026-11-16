using System.Collections.Concurrent;

namespace TrackHub.Router.Domain.Helpers;

// Serialises work per credential across the transient readers of one process. The scope keeps unrelated
// uses of the same credential (its token, its requests) from sharing a gate.
public static class CredentialGate
{
    private static readonly ConcurrentDictionary<(string Scope, Guid CredentialId), SemaphoreSlim> Gates = new();

    public static async Task<T> RunAsync<T>(string scope, Guid credentialId, Func<Task<T>> work, CancellationToken cancellationToken)
    {
        var gate = Gates.GetOrAdd((scope, credentialId), _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            return await work();
        }
        finally
        {
            gate.Release();
        }
    }
}
