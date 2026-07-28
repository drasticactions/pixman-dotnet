using Xunit;

namespace Pixman.Tests;

public class RegionTests
{
    [Fact]
    public void EmptyRegion32_IsEmpty()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var region = new PixmanRegion32();
        Assert.True(region.IsEmpty);
        Assert.Equal(0, region.RectangleCount);
        Assert.Empty(region.Rectangles());
    }

    [Fact]
    public void RectCtor32_SetsExtents()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var region = new PixmanRegion32(10, 20, 30, 40);
        Assert.False(region.IsEmpty);
        Assert.Equal(1, region.RectangleCount);
        var extents = region.Extents;
        Assert.Equal(10, extents.X1);
        Assert.Equal(20, extents.Y1);
        Assert.Equal(40, extents.X2);
        Assert.Equal(60, extents.Y2);
    }

    [Fact]
    public void Intersect32_OverlappingRects_YieldsIntersection()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var a = new PixmanRegion32(0, 0, 100, 100);
        using var b = new PixmanRegion32(50, 50, 100, 100);
        using var result = new PixmanRegion32();
        result.Intersect(a, b);
        var extents = result.Extents;
        Assert.Equal(50, extents.X1);
        Assert.Equal(50, extents.Y1);
        Assert.Equal(100, extents.X2);
        Assert.Equal(100, extents.Y2);
    }

    [Fact]
    public void Union32_DisjointRects_KeepsBoth()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var a = new PixmanRegion32(0, 0, 10, 10);
        using var b = new PixmanRegion32(20, 20, 10, 10);
        using var result = new PixmanRegion32();
        result.Union(a, b);
        Assert.Equal(2, result.RectangleCount);
        var rects = result.Rectangles();
        Assert.Contains(rects, r => r is { X1: 0, Y1: 0, X2: 10, Y2: 10 });
        Assert.Contains(rects, r => r is { X1: 20, Y1: 20, X2: 30, Y2: 30 });
    }

    [Fact]
    public void Subtract32_RemovesOverlap()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var minuend = new PixmanRegion32(0, 0, 100, 10);
        using var subtrahend = new PixmanRegion32(50, 0, 50, 10);
        using var result = new PixmanRegion32();
        result.Subtract(minuend, subtrahend);
        var extents = result.Extents;
        Assert.Equal(0, extents.X1);
        Assert.Equal(50, extents.X2);
    }

    [Fact]
    public void InPlaceConveniences32_MutateThis()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var region = new PixmanRegion32(0, 0, 100, 100);
        using var other = new PixmanRegion32(50, 50, 100, 100);
        region.IntersectWith(other);
        Assert.Equal(50, region.Extents.X1);
        Assert.Equal(100, region.Extents.X2);

        region.UnionWith(other);
        Assert.Equal(150, region.Extents.X2);

        region.SubtractWith(other);
        Assert.True(region.IsEmpty);
    }

    [Fact]
    public void Contains32_PointAndBox()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var region = new PixmanRegion32(0, 0, 10, 10);
        Assert.True(region.Contains(5, 5));
        Assert.False(region.Contains(15, 15));

        Assert.True(region.Contains(3, 3, out var containing));
        Assert.Equal(0, containing.X1);
        Assert.Equal(10, containing.X2);

        Assert.Equal(PixmanRegionOverlap.In, region.Contains(new PixmanBox32(1, 1, 9, 9)));
        Assert.Equal(PixmanRegionOverlap.Out, region.Contains(new PixmanBox32(20, 20, 30, 30)));
        Assert.Equal(PixmanRegionOverlap.Part, region.Contains(new PixmanBox32(5, 5, 15, 15)));
    }

    [Fact]
    public void Translate32_MovesExtents()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var region = new PixmanRegion32(0, 0, 10, 10);
        region.Translate(5, -3);
        var extents = region.Extents;
        Assert.Equal(5, extents.X1);
        Assert.Equal(-3, extents.Y1);
        Assert.Equal(15, extents.X2);
        Assert.Equal(7, extents.Y2);
    }

    [Fact]
    public void FromRectangles32_BuildsRegion()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var region = PixmanRegion32.FromRectangles(
        [
            new PixmanBox32(0, 0, 10, 10),
            new PixmanBox32(10, 0, 20, 10),
        ]);
        // Adjacent boxes coalesce into one band.
        Assert.Equal(1, region.RectangleCount);
        Assert.Equal(20, region.Extents.X2);
    }

    [Fact]
    public void CopyAndEquals32_SetEquality()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var a = new PixmanRegion32(0, 0, 10, 10);
        using var b = new PixmanRegion32();
        b.Copy(a);
        Assert.True(a.Equals(b));

        // The same set built a different way is still equal.
        using var c = new PixmanRegion32();
        using var left = new PixmanRegion32(0, 0, 5, 10);
        using var right = new PixmanRegion32(5, 0, 5, 10);
        c.Union(left, right);
        Assert.True(a.Equals(c));

        using var different = new PixmanRegion32(1, 0, 10, 10);
        Assert.False(a.Equals(different));
    }

    [Fact]
    public void ClearAndReset32_Work()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var region = new PixmanRegion32(0, 0, 10, 10);
        region.Clear();
        Assert.True(region.IsEmpty);

        region.Reset(new PixmanBox32(1, 2, 3, 4));
        Assert.False(region.IsEmpty);
        Assert.Equal(1, region.Extents.X1);
        Assert.True(region.SelfCheck());
    }

    [Fact]
    public void Disposed32_Throws()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var region = new PixmanRegion32(0, 0, 10, 10);
        region.Dispose();
        region.Dispose(); // double dispose is a no-op
        Assert.True(region.IsDisposed);
        Assert.Throws<ObjectDisposedException>(() => region.IsEmpty);
    }

    [Fact]
    public void Region16_BasicOperations()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var a = new PixmanRegion16(0, 0, 100, 100);
        using var b = new PixmanRegion16(50, 50, 100, 100);
        using var result = new PixmanRegion16();

        result.Intersect(a, b);
        var extents = result.Extents;
        Assert.Equal(50, extents.X1);
        Assert.Equal(100, extents.X2);

        result.Union(a, b);
        Assert.Equal(150, result.Extents.X2);

        Assert.True(a.Contains(5, 5));
        Assert.False(a.Contains(150, 150));
        Assert.Equal(PixmanRegionOverlap.Part, a.Contains(new PixmanBox16(50, 50, 150, 150)));

        a.Translate(10, 0);
        Assert.Equal(10, a.Extents.X1);

        using var copy = new PixmanRegion16();
        copy.Copy(a);
        Assert.True(copy.Equals(a));

        a.Clear();
        Assert.True(a.IsEmpty);
    }

    [Fact]
    public void Region16_Disposed_Throws()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var region = new PixmanRegion16();
        region.Dispose();
        Assert.Throws<ObjectDisposedException>(() => region.RectangleCount);
    }

    [Fact]
    public void FromImage32_ReadsA1Bits()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var empty = PixmanImage.CreateBits(PixmanFormat.A1, 8, 8);
        using var emptyRegion = PixmanRegion32.FromImage(empty);
        Assert.True(emptyRegion.IsEmpty);

        using var full = PixmanImage.CreateBits(PixmanFormat.A1, 8, 8);
        full.DataSpan.Fill(0xff);
        using var fullRegion = PixmanRegion32.FromImage(full);
        Assert.False(fullRegion.IsEmpty);
        var extents = fullRegion.Extents;
        Assert.Equal(0, extents.X1);
        Assert.Equal(0, extents.Y1);
        Assert.Equal(8, extents.X2);
        Assert.Equal(8, extents.Y2);
    }

    [Fact]
    public void ComputeCompositeRegion16_CoversDestination()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        using var source = PixmanImage.CreateSolidFill(PixmanColor.FromRgba(0xff, 0, 0, 0xff));
        using var dest = PixmanImage.CreateBits(PixmanFormat.A8R8G8B8, 8, 8);
        using var result = new PixmanRegion16();

        Assert.True(PixmanRegion16.ComputeCompositeRegion(result, source, null, dest, 0, 0, 0, 0, 0, 0, 8, 8));
        var extents = result.Extents;
        Assert.Equal(0, extents.X1);
        Assert.Equal(8, extents.X2);
        Assert.Equal(8, extents.Y2);
    }
}
