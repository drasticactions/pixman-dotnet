using System.Runtime.InteropServices;
using System.Text;
using Pixman;

string outputPath = args.Length > 0 ? args[0] : "sample.ppm";
const int Size = 512;

Console.WriteLine($"pixman {PixmanLibrary.VersionString} (bindings {PixmanLibrary.BindingsVersionString})");

using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, Size, Size);

using (var gradient = PixmanImage.CreateLinearGradient(
    new PixmanPointFixed(0, 0), new PixmanPointFixed(Size, Size),
    [
        new PixmanGradientStop(0.0, PixmanColor.FromRgba(0x1e, 0x29, 0x4b, 0xff)),
        new PixmanGradientStop(1.0, PixmanColor.FromRgba(0xff, 0x5d, 0x8f, 0xff)),
    ]))
{
    canvas.Composite(PixmanOp.Src, gradient, null, 0, 0, 0, 0, 0, 0, Size, Size);
}

using (var disc = PixmanImage.CreateRadialGradient(
    new PixmanPointFixed(340, 180), new PixmanPointFixed(340, 180), 0, 130,
    [
        new PixmanGradientStop(0.0, PixmanColor.FromRgba(0xff, 0xd7, 0x00, 0xff)),
        new PixmanGradientStop(0.7, PixmanColor.FromRgba(0xff, 0x8c, 0x00, 0xc0)),
        new PixmanGradientStop(1.0, PixmanColor.FromRgba(0xff, 0x8c, 0x00, 0x00)),
    ]))
{
    canvas.Composite(PixmanOp.Over, disc, null, 0, 0, 0, 0, 0, 0, Size, Size);
}

using (var checker = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 2, 2))
{
    var texels = MemoryMarshal.Cast<byte, uint>(checker.DataSpan);
    texels[0] = 0xFFF0F0F0; texels[1] = 0xFF303030;
    texels[2] = 0xFF303030; texels[3] = 0xFFF0F0F0;

    checker.SetTransform(PixmanTransform.CreateScale(1.0 / 16.0, 1.0 / 16.0));
    checker.SetRepeat(PixmanRepeat.Normal);
    checker.SetFilter(PixmanFilter.Nearest);
    canvas.Composite(PixmanOp.Over, checker, null, 0, 0, 0, 0, 24, 328, 160, 160);
}

using (var ink = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0x00, 0xe6, 0xa8, 0xff)))
{
    var apex = new PixmanPointFixed(110, 56);
    var triangle = new PixmanTrapezoid(
        56, 190,
        new PixmanLineFixed(apex, new PixmanPointFixed(40, 190)),
        new PixmanLineFixed(apex, new PixmanPointFixed(180, 190)));
    canvas.CompositeTrapezoids(PixmanOp.Over, ink, PixmanFormat.A8, 0, 0, 0, 0, [triangle]);
}

var pixels = MemoryMarshal.Cast<byte, uint>(canvas.DataSpan);
int strideUints = canvas.Stride / sizeof(uint);
using (var output = File.Create(outputPath))
{
    output.Write(Encoding.ASCII.GetBytes($"P6\n{Size} {Size}\n255\n"));
    var row = new byte[Size * 3];
    for (int y = 0; y < Size; y++)
    {
        var scanline = pixels.Slice(y * strideUints, Size);
        for (int x = 0; x < Size; x++)
        {
            uint argb = scanline[x];
            row[(x * 3) + 0] = (byte)(argb >> 16);
            row[(x * 3) + 1] = (byte)(argb >> 8);
            row[(x * 3) + 2] = (byte)argb;
        }

        output.Write(row);
    }
}

Console.WriteLine($"Rendered {Size}x{Size} canvas to {outputPath}");
