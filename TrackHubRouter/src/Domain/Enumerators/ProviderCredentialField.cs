namespace TrackHub.Router.Domain.Enumerators;

// Which credential fields a provider reads besides the base URL, so a client can ask only for those.
[Flags]
public enum ProviderCredentialField
{
    None = 0,
    Username = 1,
    Password = 2,
    Key = 4,
    Key2 = 8,
    All = Username | Password | Key | Key2,
}
