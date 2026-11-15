using System.Net.Http.Headers;
using System.Text;

namespace TrackHub.Router.Domain.Helpers;

public static class BasicAuthentication
{
    public static AuthenticationHeaderValue Header(string username, string password)
        => new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
}
