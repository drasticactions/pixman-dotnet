using System.Runtime.InteropServices;
using Xunit;

namespace Pixman.Tests;

public class GlyphCacheTests
{
    private static readonly IntPtr FontKey = 1;
    private static readonly IntPtr GlyphKey = 2;

    private static PixmanImage CreateGlyphImage()
    {
        // A fully-covered 4x4 A8 glyph.
        var image = PixmanImage.CreateBits(PixmanFormat.A8, 4, 4);
        image.DataSpan.Fill(0xff);
        return image;
    }

    [Fact]
    public void CreateAndDispose_Works()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var cache = PixmanGlyphCache.Create();
        Assert.NotEqual(IntPtr.Zero, cache.Handle);
        cache.Dispose();
        cache.Dispose(); // double dispose is a no-op
        Assert.True(cache.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => cache.Freeze());
    }

    [Fact]
    public void InsertLookupRemove_RoundTrips()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var cache = PixmanGlyphCache.Create();
        using var glyphImage = CreateGlyphImage();

        cache.Freeze();
        var handle = cache.Insert(FontKey, GlyphKey, 0, 0, glyphImage);
        Assert.NotEqual(IntPtr.Zero, handle);
        Assert.Equal(handle, cache.Lookup(FontKey, GlyphKey));

        // A different key finds nothing.
        Assert.Equal(IntPtr.Zero, cache.Lookup(FontKey, (IntPtr)99));

        cache.Remove(FontKey, GlyphKey);
        Assert.Equal(IntPtr.Zero, cache.Lookup(FontKey, GlyphKey));
        cache.Thaw();
    }

    [Fact]
    public void GetExtentsAndMaskFormat_DescribeGlyphs()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var cache = PixmanGlyphCache.Create();
        using var glyphImage = CreateGlyphImage();

        cache.Freeze();
        var handle = cache.Insert(FontKey, GlyphKey, 0, 0, glyphImage);
        ReadOnlySpan<PixmanGlyph> glyphs = [new PixmanGlyph(2, 3, handle)];

        var extents = cache.GetExtents(glyphs);
        Assert.Equal(2, extents.X1);
        Assert.Equal(3, extents.Y1);
        Assert.Equal(6, extents.X2);
        Assert.Equal(7, extents.Y2);

        Assert.Equal(PixmanFormat.A8, cache.GetMaskFormat(glyphs));
        cache.Thaw();
    }

    [Fact]
    public void CompositeGlyphs_PlacesGlyph()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var cache = PixmanGlyphCache.Create();
        using var glyphImage = CreateGlyphImage();
        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0xff, 0x00, 0x00, 0xff));

        cache.Freeze();
        var handle = cache.Insert(FontKey, GlyphKey, 0, 0, glyphImage);
        ReadOnlySpan<PixmanGlyph> glyphs = [new PixmanGlyph(2, 2, handle)];

        canvas.CompositeGlyphs(PixmanOp.Over, solid, PixmanFormat.A8, 0, 0, 0, 0, 0, 0, 8, 8, cache, glyphs);
        cache.Thaw();

        var pixels = MemoryMarshal.Cast<byte, uint>(canvas.DataSpan);
        Assert.Equal(0xffff0000u, pixels[3 * 8 + 3]); // inside the glyph
        Assert.Equal(0u, pixels[0]);                  // outside the glyph
    }

    [Fact]
    public void CompositeGlyphsNoMask_PlacesGlyph()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var cache = PixmanGlyphCache.Create();
        using var glyphImage = CreateGlyphImage();
        using var canvas = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var solid = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0x00, 0xff, 0x00, 0xff));

        cache.Freeze();
        var handle = cache.Insert(FontKey, GlyphKey, 0, 0, glyphImage);
        ReadOnlySpan<PixmanGlyph> glyphs = [new PixmanGlyph(1, 1, handle)];

        canvas.CompositeGlyphsNoMask(PixmanOp.Over, solid, 0, 0, 0, 0, cache, glyphs);
        cache.Thaw();

        var pixels = MemoryMarshal.Cast<byte, uint>(canvas.DataSpan);
        Assert.Equal(0xff00ff00u, pixels[2 * 8 + 2]);
        Assert.Equal(0u, pixels[7 * 8 + 7]);
    }
}
