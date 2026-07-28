using System.Diagnostics;
using System.Runtime.Versioning;
using Drm;
using Pixman;

[assembly: SupportedOSPlatform("linux")]
[assembly: SupportedOSPlatform("freebsd")]

string? path = args.Length > 0 ? args[0] : null;
int seconds = args.Length > 1 ? int.Parse(args[1]) : 5;

if (path is null)
{
    foreach (var info in DrmDevice.EnumerateDevices())
    {
        Console.WriteLine($"Found device: {info} (bus: {info.BusType})");
        path ??= info.PrimaryNodePath;
    }
}

if (path is null)
{
    Console.Error.WriteLine("No DRM devices found.");
    return 1;
}

using var device = DrmDevice.Open(path);
Console.WriteLine($"Opened {path}: {device.GetVersion()}, pixman {PixmanLibrary.VersionString}");

if (device.GetCapability(DrmCapability.DumbBuffer) == 0)
{
    Console.Error.WriteLine("Device does not support dumb buffers.");
    return 1;
}

var resources = device.GetResources();

// Pick the first connected connector that has modes.
DrmConnector? connector = null;
foreach (uint connectorId in resources.ConnectorIds)
{
    var candidate = device.GetConnector(connectorId);
    if (candidate.Status == DrmConnectionStatus.Connected && candidate.Modes.Count > 0)
    {
        connector ??= candidate;
    }
}

if (connector is null)
{
    Console.Error.WriteLine("No connected connector with modes found.");
    return 1;
}

var mode = connector.Modes.FirstOrDefault(m => m.IsPreferred, connector.Modes[0]);
Console.WriteLine($"Using {connector.Name} with mode {mode}");

// Find a CRTC for the connector: the one already driving it if possible,
// otherwise the first CRTC an encoder of the connector can use.
uint crtcId = 0;
if (connector.CurrentEncoderId != 0)
{
    crtcId = device.GetEncoder(connector.CurrentEncoderId).CrtcId;
}

if (crtcId == 0)
{
    foreach (uint encoderId in connector.EncoderIds)
    {
        var encoder = device.GetEncoder(encoderId);
        for (int i = 0; i < resources.CrtcIds.Count && crtcId == 0; i++)
        {
            if ((encoder.PossibleCrtcs & (1u << i)) != 0)
            {
                crtcId = resources.CrtcIds[i];
            }
        }

        if (crtcId != 0)
        {
            break;
        }
    }
}

if (crtcId == 0)
{
    Console.Error.WriteLine("No usable CRTC found.");
    return 1;
}

// Save the current CRTC state so it can be restored on exit.
var previous = device.GetCrtc(crtcId);

using var buffer = device.CreateDumbBuffer(mode.HorizontalDisplay, mode.VerticalDisplay);
using var framebuffer = device.AddFramebuffer(buffer);

IntPtr scanout;
unsafe
{
    fixed (byte* mapped = buffer.AsSpan())
    {
        scanout = (IntPtr)mapped;
    }
}

int width = (int)buffer.Width;
int height = (int)buffer.Height;
using var canvas = PixmanImage.CreateBits(PixmanFormat.X8R8G8B8, width, height, scanout, (int)buffer.Pitch);

PixmanGradientStop[] backgroundStops =
[
    new(0.0, PixmanColor.FromRgba(0x0f, 0x1e, 0x3c, 0xff)),
    new(0.5, PixmanColor.FromRgba(0x8a, 0x2e, 0x60, 0xff)),
    new(1.0, PixmanColor.FromRgba(0xff, 0xb3, 0x47, 0xff)),
];
PixmanGradientStop[] discStops =
[
    new(0.0, PixmanColor.FromRgba(0xff, 0xff, 0xff, 0xff)),
    new(0.6, PixmanColor.FromRgba(0x66, 0xd9, 0xff, 0xa0)),
    new(1.0, PixmanColor.FromRgba(0x66, 0xd9, 0xff, 0x00)),
];

Console.WriteLine($"Modesetting CRTC {crtcId}, animating for {seconds}s...");
try
{
    device.SetCrtc(crtcId, framebuffer.Id, 0, 0, [connector.ConnectorId], mode);

    double cx = width / 2.0, cy = height / 2.0;
    double discRadius = Math.Min(width, height) * 0.18;
    var clock = Stopwatch.StartNew();
    int frames = 0;
    while (clock.Elapsed < TimeSpan.FromSeconds(seconds))
    {
        double t = clock.Elapsed.TotalSeconds;

        double angle = t * 0.6;
        double dx = Math.Cos(angle) * cx, dy = Math.Sin(angle) * cy;
        using (var background = PixmanImage.CreateLinearGradient(
            new PixmanPointFixed(cx - dx, cy - dy), new PixmanPointFixed(cx + dx, cy + dy), backgroundStops))
        {
            background.SetRepeat(PixmanRepeat.Pad);
            canvas.Composite(PixmanOp.Src, background, null, 0, 0, 0, 0, 0, 0, width, height);
        }

        double ox = cx + Math.Cos(t * 1.7) * width * 0.28;
        double oy = cy + Math.Sin(t * 1.3) * height * 0.28;
        var center = new PixmanPointFixed(ox, oy);
        using (var disc = PixmanImage.CreateRadialGradient(center, center, 0, discRadius, discStops))
        {
            canvas.Composite(PixmanOp.Over, disc, null, 0, 0, 0, 0, 0, 0, width, height);
        }

        frames++;
    }

    Console.WriteLine($"Rendered {frames} frames ({frames / (double)seconds:F1} fps, single-buffered).");
}
finally
{
    device.SetCrtc(crtcId, previous.BufferId, previous.X, previous.Y, [connector.ConnectorId], previous.Mode);
}

Console.WriteLine("Done.");
return 0;
