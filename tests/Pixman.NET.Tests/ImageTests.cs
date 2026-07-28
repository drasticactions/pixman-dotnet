using System.Runtime.InteropServices;
using Xunit;

namespace Pixman.Tests;

public class ImageTests
{
    private static Span<uint> Pixels(PixmanImage image) => MemoryMarshal.Cast<byte, uint>(image.DataSpan);

    [Fact]
    public void CreateBits_HasExpectedProperties()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var image = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 16, 8);
        Assert.Equal(16, image.Width);
        Assert.Equal(8, image.Height);
        Assert.Equal(PixmanFormat.A8R8G8B8, image.Format);
        Assert.Equal(32, image.Depth);
        Assert.Equal(64, image.Stride);
        Assert.NotEqual(IntPtr.Zero, image.Data);
        Assert.Equal(image.Stride * image.Height, image.DataSpan.Length);

        // pixman_image_create_bits zeroes newly allocated bits.
        foreach (var b in image.DataSpan)
        {
            Assert.Equal(0, b);
        }
    }

    [Fact]
    public unsafe void CreateBits_ExternalMemory_WritesThrough()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        const int width = 4, height = 4, stride = width * 4;
        void* bits = NativeMemory.AllocZeroed(height * stride);
        try
        {
            var red = PixmanColor.FromRgba(0xff, 0x00, 0x00, 0xff);
            using (var image = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, width, height, (IntPtr)bits, stride))
            {
                image.Fill(PixmanOp.Src, red, [new PixmanRectangle16(0, 0, width, height)]);
            }

            // pixman rendered directly into the caller-owned buffer.
            Assert.Equal(0xffff0000u, ((uint*)bits)[0]);
            Assert.Equal(0xffff0000u, ((uint*)bits)[width * height - 1]);
        }
        finally
        {
            NativeMemory.Free(bits);
        }
    }

    [Fact]
    public void Fill_Rectangles_WritesExactPixels()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var image = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        image.Fill(PixmanOp.Src, PixmanColor.FromRgba(0xff, 0x00, 0x00, 0xff), [new PixmanRectangle16(2, 2, 4, 4)]);

        var pixels = Pixels(image);
        Assert.Equal(0xffff0000u, pixels[3 * 8 + 3]); // inside
        Assert.Equal(0u, pixels[0]);                  // outside
        Assert.Equal(0u, pixels[7 * 8 + 7]);
    }

    [Fact]
    public void Fill_Boxes_WritesExactPixels()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var image = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        image.Fill(PixmanOp.Src, PixmanColor.FromRgba(0x00, 0xff, 0x00, 0xff), [new PixmanBox32(0, 0, 8, 4)]);

        var pixels = Pixels(image);
        Assert.Equal(0xff00ff00u, pixels[0]);
        Assert.Equal(0u, pixels[5 * 8]);
    }

    [Fact]
    public void Composite_SrcSolid_WritesExactPixels()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0x00, 0x00, 0xff, 0xff));
        canvas.Composite(PixmanOp.Src, solid, null, 0, 0, 0, 0, 2, 2, 4, 4);

        var pixels = Pixels(canvas);
        Assert.Equal(0xff0000ffu, pixels[2 * 8 + 2]);
        Assert.Equal(0xff0000ffu, pixels[5 * 8 + 5]);
        Assert.Equal(0u, pixels[0]);
        Assert.Equal(0u, pixels[6 * 8 + 6]);
    }

    [Fact]
    public void Composite_OverOpaque_ReplacesDestination()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 4, 4);
        canvas.Fill(PixmanOp.Src, PixmanColor.FromRgba(0xff, 0x00, 0x00, 0xff), [new PixmanRectangle16(0, 0, 4, 4)]);

        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0x00, 0xff, 0x00, 0xff));
        canvas.Composite(PixmanOp.Over, solid, null, 0, 0, 0, 0, 0, 0, 4, 4);

        Assert.Equal(0xff00ff00u, Pixels(canvas)[0]);
    }

    [Fact]
    public void Composite_LinearGradient_MatchesEndpoints()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 4, 16);
        using var gradient = PixmanImage.CreateLinearGradient(
            new PixmanPointFixed(0, 0),
            new PixmanPointFixed(0, 16),
            [
                new PixmanGradientStop(0.0, PixmanColor.FromRgba(0xff, 0x00, 0x00, 0xff)),
                new PixmanGradientStop(1.0, PixmanColor.FromRgba(0x00, 0x00, 0xff, 0xff)),
            ]);
        canvas.Composite(PixmanOp.Src, gradient, null, 0, 0, 0, 0, 0, 0, 4, 16);

        var pixels = Pixels(canvas);
        uint top = pixels[0];
        uint bottom = pixels[15 * 4];
        Assert.NotEqual(top, bottom);

        // Top row is dominated by the red endpoint, bottom row by the blue one.
        Assert.True(((top >> 16) & 0xff) > 0xc0, $"top red channel was {(top >> 16) & 0xff:x2}");
        Assert.True((top & 0xff) < 0x40, $"top blue channel was {top & 0xff:x2}");
        Assert.True((bottom & 0xff) > 0xc0, $"bottom blue channel was {bottom & 0xff:x2}");
        Assert.True(((bottom >> 16) & 0xff) < 0x40, $"bottom red channel was {(bottom >> 16) & 0xff:x2}");
    }

    [Fact]
    public void Composite_ScaledSource_UsesTransform()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        // 2x2 source: red | green / blue | white.
        using var source = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 2, 2);
        var sourcePixels = Pixels(source);
        sourcePixels[0] = 0xffff0000;
        sourcePixels[1] = 0xff00ff00;
        sourcePixels[2] = 0xff0000ff;
        sourcePixels[3] = 0xffffffff;

        // The transform maps destination coordinates to source coordinates, so a
        // 0.5 scale magnifies the source 2x.
        source.SetFilter(PixmanFilter.Nearest);
        source.SetRepeat(PixmanRepeat.Pad);
        source.SetTransform(PixmanTransform.CreateScale(0.5, 0.5));

        using var dest = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 4, 4);
        dest.Composite(PixmanOp.Src, source, null, 0, 0, 0, 0, 0, 0, 4, 4);

        var pixels = Pixels(dest);
        Assert.Equal(0xffff0000u, pixels[0]);          // top-left block
        Assert.Equal(0xff00ff00u, pixels[3]);          // top-right block
        Assert.Equal(0xff0000ffu, pixels[3 * 4]);      // bottom-left block
        Assert.Equal(0xffffffffu, pixels[3 * 4 + 3]);  // bottom-right block
    }

    [Fact]
    public void SetClipRegion_RestrictsComposite()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0xff, 0xff, 0xff, 0xff));

        using var clip = new PixmanRegion32(0, 0, 4, 4);
        canvas.SetClipRegion(clip);
        canvas.Composite(PixmanOp.Src, solid, null, 0, 0, 0, 0, 0, 0, 8, 8);

        var pixels = Pixels(canvas);
        Assert.Equal(0xffffffffu, pixels[0]);        // inside clip
        Assert.Equal(0u, pixels[5 * 8 + 5]);          // outside clip

        canvas.ClearClipRegion();
        canvas.Composite(PixmanOp.Src, solid, null, 0, 0, 0, 0, 0, 0, 8, 8);
        Assert.Equal(0xffffffffu, Pixels(canvas)[5 * 8 + 5]);
    }

    [Fact]
    public void SetClipRegion16_AlsoRestricts()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0xff, 0xff, 0xff, 0xff));

        using var clip = new PixmanRegion16(0, 0, 2, 2);
        canvas.SetClipRegion(clip);
        canvas.Composite(PixmanOp.Src, solid, null, 0, 0, 0, 0, 0, 0, 8, 8);

        var pixels = Pixels(canvas);
        Assert.Equal(0xffffffffu, pixels[0]);
        Assert.Equal(0u, pixels[3 * 8 + 3]);
    }

    [Fact]
    public void SetFilter_WithConvolutionParameters_DoesNotCrash()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var source = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 2, 2);
        // A 2x2 convolution kernel: width, height, then width*height weights.
        var quarter = PixmanFixed.FromDouble(0.25);
        source.SetFilter(PixmanFilter.Convolution, [2, 2, quarter, quarter, quarter, quarter]);

        using var dest = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 2, 2);
        dest.Composite(PixmanOp.Src, source, null, 0, 0, 0, 0, 0, 0, 2, 2);
    }

    [Fact]
    public void SolidAndGradientImages_ReportZeroDimensions()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        // Non-bits images have no bit geometry: pixman reports 0 width/height
        // and no data pointer.
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0xff, 0, 0, 0xff));
        Assert.Equal(0, solid.Width);
        Assert.Equal(0, solid.Height);
        Assert.Equal(IntPtr.Zero, solid.Data);
        Assert.True(solid.DataSpan.IsEmpty);

        using var gradient = PixmanImage.CreateConicalGradient(
            new PixmanPointFixed(0, 0),
            PixmanFixed.Zero,
            [
                new PixmanGradientStop(0.0, PixmanColor.FromRgba(0xff, 0, 0, 0xff)),
                new PixmanGradientStop(1.0, PixmanColor.FromRgba(0, 0xff, 0, 0xff)),
            ]);
        Assert.Equal(0, gradient.Width);
    }

    [Fact]
    public void RadialGradient_Composites()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var gradient = PixmanImage.CreateRadialGradient(
            new PixmanPointFixed(4, 4),
            new PixmanPointFixed(4, 4),
            PixmanFixed.Zero,
            8,
            [
                new PixmanGradientStop(0.0, PixmanColor.FromRgba(0xff, 0xff, 0xff, 0xff)),
                new PixmanGradientStop(1.0, PixmanColor.FromRgba(0x00, 0x00, 0x00, 0xff)),
            ]);
        canvas.Composite(PixmanOp.Src, gradient, null, 0, 0, 0, 0, 0, 0, 8, 8);

        var pixels = Pixels(canvas);
        // The center is brighter than the corner.
        Assert.True((pixels[4 * 8 + 4] & 0xff) > (pixels[0] & 0xff));
    }

    [Fact]
    public void RasterizeTrapezoid_CoversInterior()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var mask = PixmanImage.CreateBits(PixmanFormat.A8, 8, 8);
        var trap = new PixmanTrapezoid(
            0,
            8,
            new PixmanLineFixed(new PixmanPointFixed(2, 0), new PixmanPointFixed(2, 8)),
            new PixmanLineFixed(new PixmanPointFixed(6, 0), new PixmanPointFixed(6, 8)));
        Assert.True(trap.IsValid);

        mask.RasterizeTrapezoid(trap, 0, 0);

        var data = mask.DataSpan;
        Assert.Equal(0xff, data[4 * mask.Stride + 4]); // fully covered interior
        Assert.Equal(0, data[4 * mask.Stride]);        // left of the trapezoid
        Assert.Equal(0, data[4 * mask.Stride + 7]);    // right of the trapezoid
    }

    [Fact]
    public void CompositeTrapezoids_RendersThroughMask()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0xff, 0x00, 0x00, 0xff));
        var trap = new PixmanTrapezoid(
            0,
            8,
            new PixmanLineFixed(new PixmanPointFixed(2, 0), new PixmanPointFixed(2, 8)),
            new PixmanLineFixed(new PixmanPointFixed(6, 0), new PixmanPointFixed(6, 8)));

        canvas.CompositeTrapezoids(PixmanOp.Over, solid, PixmanFormat.A8, 0, 0, 0, 0, [trap]);

        var pixels = Pixels(canvas);
        Assert.Equal(0xffff0000u, pixels[4 * 8 + 4]);
        Assert.Equal(0u, pixels[4 * 8]);
    }

    [Fact]
    public void CompositeTriangles_RendersThroughMask()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0x00, 0xff, 0x00, 0xff));
        var triangle = new PixmanTriangle(
            new PixmanPointFixed(0, 0),
            new PixmanPointFixed(8, 0),
            new PixmanPointFixed(0, 8));

        canvas.CompositeTriangles(PixmanOp.Over, solid, PixmanFormat.A8, 0, 0, 0, 0, [triangle]);

        var pixels = Pixels(canvas);
        Assert.Equal(0xff00ff00u, pixels[1 * 8 + 1]); // deep inside the triangle
        Assert.Equal(0u, pixels[7 * 8 + 7]);          // opposite corner, outside
    }

    [Fact]
    public void ComponentAlpha_RoundTrips()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var image = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 2, 2);
        Assert.False(image.ComponentAlpha);
        image.ComponentAlpha = true;
        Assert.True(image.ComponentAlpha);
    }

    [Fact]
    public void Dispose_MakesMembersThrow()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var image = PixmanImage.CreateBits(PixmanFormat.A8, 2, 2);
        image.Dispose();
        image.Dispose(); // double dispose is a no-op
        Assert.True(image.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => image.Width);
    }
}
