namespace TrackHub.Router.Domain.Helpers;

public static class GpsFix
{
    // Providers report a device without a fix as missing coordinates or as 0,0.
    public static bool IsValid(double? latitude, double? longitude)
        => latitude is { } lat && longitude is { } lng && (lat != 0 || lng != 0);
}
