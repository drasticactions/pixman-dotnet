# pixman-dotnet

pixman-dotnet are .NET bindings for [pixman](https://gitlab.freedesktop.org/pixman/pixman), the pixel-manipulation and compositing library used by cairo and the X server.

## Usage

```csharp
using Pixman;

using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 256, 256);

// Fill with a linear gradient…
using var gradient = PixmanImage.CreateLinearGradient(
    new PixmanPointFixed(0, 0), new PixmanPointFixed(256, 256),
    [
        new PixmanGradientStop(0.0, PixmanColor.FromRgba(0x33, 0x66, 0xff, 0xff)),
        new PixmanGradientStop(1.0, PixmanColor.FromRgba(0xff, 0x33, 0x99, 0xff)),
    ]);
canvas.Composite(PixmanOp.Src, gradient, mask: null, 0, 0, 0, 0, 0, 0, 256, 256);

// …composite a translucent solid square over it…
using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0xff, 0xff, 0xff, 0x80));
canvas.Composite(PixmanOp.Over, solid, mask: null, 0, 0, 0, 0, 64, 64, 128, 128);

// …and read the pixels back.
Span<byte> pixels = canvas.DataSpan;
```

```csharp
using Pixman;

using var region = new PixmanRegion32(0, 0, 100, 100);
using var other = new PixmanRegion32(50, 50, 100, 100);
region.IntersectWith(other);
PixmanBox32 extents = region.Extents; // 50,50 → 100,100
```

The raw layer is always available and interoperates with the wrapper via `Handle`:

```csharp
using Pixman.Native;

unsafe
{
    Console.WriteLine(Libpixman.pixman_version());
    pixman_image* img = Libpixman.pixman_image_create_bits(
        pixman_format_code_t.PIXMAN_a8r8g8b8, 64, 64, null, 0);
    Libpixman.pixman_image_unref(img);
}
```

## Testing

```sh
dotnet test
```

## Samples

```sh
# Renders gradients/trapezoids and writes sample.ppm to the working directory.
dotnet run --project samples/Pixman.NET.Sample

# Renders with pixman into a DRM dumb buffer and displays it (Linux, needs DRM master).
dotnet run --project samples/Pixman.NET.DrmSample -- /dev/dri/card0 5
```

## Regenerating the bindings

```sh
git submodule update --init
dotnet tool restore
sh eng/generate.sh
```

Requires a system clang for its builtin-header resource directory.
