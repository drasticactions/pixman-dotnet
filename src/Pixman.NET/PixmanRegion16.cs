using Pixman.Native;

namespace Pixman;

/// <summary>A set of non-overlapping rectangles with 16-bit coordinates, wrapping <c>pixman_region16_t</c>.</summary>
/// <remarks>
/// Ternary set operations follow pixman's destination-style convention: the instance the method is
/// called on receives the result and may alias either operand. The float region API
/// (<c>pixman_region64f_*</c>) has no wrapper; use <see cref="Native.Libpixman"/> directly.
/// A region that is never disposed leaks its native rectangle array.
/// </remarks>
public sealed unsafe class PixmanRegion16 : IDisposable
{
    /// <summary>The native region, pinned via <c>fixed</c> for each call.</summary>
    internal pixman_region16 Region;

    private bool _disposed;

    /// <summary>Initializes a new empty region.</summary>
    public PixmanRegion16()
    {
        fixed (pixman_region16* r = &Region)
        {
            Libpixman.pixman_region_init(r);
        }
    }

    /// <summary>Initializes a new region covering a single rectangle.</summary>
    /// <param name="x">Left edge of the rectangle.</param>
    /// <param name="y">Top edge of the rectangle.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    public PixmanRegion16(int x, int y, uint width, uint height)
    {
        fixed (pixman_region16* r = &Region)
        {
            Libpixman.pixman_region_init_rect(r, x, y, width, height);
        }
    }

    /// <summary>Initializes a new region covering a single box.</summary>
    /// <param name="extents">The box the region covers.</param>
    public PixmanRegion16(PixmanBox16 extents)
    {
        fixed (pixman_region16* r = &Region)
        {
            Libpixman.pixman_region_init_with_extents(r, (pixman_box16*)&extents);
        }
    }

    /// <summary>Whether <see cref="Dispose"/> has been called.</summary>
    public bool IsDisposed => _disposed;

    /// <summary>Whether the region contains no pixels.</summary>
    public bool IsEmpty
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            fixed (pixman_region16* r = &Region)
            {
                return Libpixman.pixman_region_not_empty(r) == 0;
            }
        }
    }

    /// <summary>The number of rectangles the region is composed of.</summary>
    public int RectangleCount
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            fixed (pixman_region16* r = &Region)
            {
                return Libpixman.pixman_region_n_rects(r);
            }
        }
    }

    /// <summary>The bounding box of the region.</summary>
    public PixmanBox16 Extents
    {
        get
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            fixed (pixman_region16* r = &Region)
            {
                return *(PixmanBox16*)Libpixman.pixman_region_extents(r);
            }
        }
    }

    /// <summary>Creates a region from a set of boxes.</summary>
    /// <param name="boxes">The boxes the region is the union of; they may overlap.</param>
    /// <returns>The new region.</returns>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public static PixmanRegion16 FromRectangles(ReadOnlySpan<PixmanBox16> boxes)
    {
        var region = new PixmanRegion16();
        fixed (pixman_region16* r = &region.Region)
        fixed (PixmanBox16* b = boxes)
        {
            Libpixman.pixman_region_fini(r);
            var result = Libpixman.pixman_region_init_rects(r, (pixman_box16*)b, boxes.Length);
            if (result == 0)
            {
                Libpixman.pixman_region_init(r);
                region.Dispose();
                throw new PixmanException("pixman_region_init_rects failed");
            }
        }

        return region;
    }

    /// <summary>Creates a region covering the pixels of an a1 (1-bit alpha) image that are set.</summary>
    /// <param name="image">The image to trace.</param>
    /// <returns>The new region.</returns>
    public static PixmanRegion16 FromImage(PixmanImage image)
    {
        var region = new PixmanRegion16();
        fixed (pixman_region16* r = &region.Region)
        {
            Libpixman.pixman_region_fini(r);
            Libpixman.pixman_region_init_from_image(r, image.NativePtr);
        }

        return region;
    }

    /// <summary>Computes the clipped area a composite operation with the given geometry would touch.</summary>
    /// <param name="result">The region receiving the composite area.</param>
    /// <param name="source">The source image.</param>
    /// <param name="mask">The mask image, or <see langword="null"/> for none.</param>
    /// <param name="destination">The destination image.</param>
    /// <param name="sourceX">X coordinate of the source origin.</param>
    /// <param name="sourceY">Y coordinate of the source origin.</param>
    /// <param name="maskX">X coordinate of the mask origin.</param>
    /// <param name="maskY">Y coordinate of the mask origin.</param>
    /// <param name="destX">X coordinate of the destination origin.</param>
    /// <param name="destY">Y coordinate of the destination origin.</param>
    /// <param name="width">Width of the composite area.</param>
    /// <param name="height">Height of the composite area.</param>
    /// <returns><see langword="false"/> when the computed region is empty.</returns>
    public static bool ComputeCompositeRegion(
        PixmanRegion16 result,
        PixmanImage source,
        PixmanImage? mask,
        PixmanImage destination,
        short sourceX,
        short sourceY,
        short maskX,
        short maskY,
        short destX,
        short destY,
        ushort width,
        ushort height)
    {
        ObjectDisposedException.ThrowIf(result._disposed, result);
        fixed (pixman_region16* r = &result.Region)
        {
            return Libpixman.pixman_compute_composite_region(
                r,
                source.NativePtr,
                mask is null ? null : mask.NativePtr,
                destination.NativePtr,
                sourceX,
                sourceY,
                maskX,
                maskY,
                destX,
                destY,
                width,
                height) != 0;
        }
    }

    /// <summary>Replaces this region with a copy of <paramref name="source"/>.</summary>
    /// <param name="source">The region to copy.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void Copy(PixmanRegion16 source)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(source._disposed, source);
        fixed (pixman_region16* d = &Region)
        fixed (pixman_region16* s = &source.Region)
        {
            PixmanException.ThrowIfFalse(Libpixman.pixman_region_copy(d, s), "pixman_region_copy failed");
        }
    }

    /// <summary>Replaces this region with the intersection of <paramref name="a"/> and <paramref name="b"/>; this region may alias an operand.</summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void Intersect(PixmanRegion16 a, PixmanRegion16 b)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(a._disposed, a);
        ObjectDisposedException.ThrowIf(b._disposed, b);
        fixed (pixman_region16* d = &Region)
        fixed (pixman_region16* ra = &a.Region)
        fixed (pixman_region16* rb = &b.Region)
        {
            PixmanException.ThrowIfFalse(Libpixman.pixman_region_intersect(d, ra, rb), "pixman_region_intersect failed");
        }
    }

    /// <summary>Replaces this region with the union of <paramref name="a"/> and <paramref name="b"/>; this region may alias an operand.</summary>
    /// <param name="a">The first operand.</param>
    /// <param name="b">The second operand.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void Union(PixmanRegion16 a, PixmanRegion16 b)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(a._disposed, a);
        ObjectDisposedException.ThrowIf(b._disposed, b);
        fixed (pixman_region16* d = &Region)
        fixed (pixman_region16* ra = &a.Region)
        fixed (pixman_region16* rb = &b.Region)
        {
            PixmanException.ThrowIfFalse(Libpixman.pixman_region_union(d, ra, rb), "pixman_region_union failed");
        }
    }

    /// <summary>Replaces this region with <paramref name="minuend"/> minus <paramref name="subtrahend"/>; this region may alias an operand.</summary>
    /// <param name="minuend">The region subtracted from.</param>
    /// <param name="subtrahend">The region to subtract.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void Subtract(PixmanRegion16 minuend, PixmanRegion16 subtrahend)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(minuend._disposed, minuend);
        ObjectDisposedException.ThrowIf(subtrahend._disposed, subtrahend);
        fixed (pixman_region16* d = &Region)
        fixed (pixman_region16* m = &minuend.Region)
        fixed (pixman_region16* s = &subtrahend.Region)
        {
            PixmanException.ThrowIfFalse(Libpixman.pixman_region_subtract(d, m, s), "pixman_region_subtract failed");
        }
    }

    /// <summary>Replaces this region with the part of <paramref name="invertBounds"/> not covered by <paramref name="source"/>.</summary>
    /// <param name="source">The region to invert.</param>
    /// <param name="invertBounds">The box the inversion is computed within.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void Inverse(PixmanRegion16 source, PixmanBox16 invertBounds)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(source._disposed, source);
        fixed (pixman_region16* d = &Region)
        fixed (pixman_region16* s = &source.Region)
        {
            PixmanException.ThrowIfFalse(Libpixman.pixman_region_inverse(d, s, (pixman_box16*)&invertBounds), "pixman_region_inverse failed");
        }
    }

    /// <summary>Replaces this region with the union of <paramref name="source"/> and a rectangle; this region may alias the operand.</summary>
    /// <param name="source">The region operand.</param>
    /// <param name="x">Left edge of the rectangle.</param>
    /// <param name="y">Top edge of the rectangle.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void UnionRect(PixmanRegion16 source, int x, int y, uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(source._disposed, source);
        fixed (pixman_region16* d = &Region)
        fixed (pixman_region16* s = &source.Region)
        {
            PixmanException.ThrowIfFalse(Libpixman.pixman_region_union_rect(d, s, x, y, width, height), "pixman_region_union_rect failed");
        }
    }

    /// <summary>Replaces this region with the intersection of <paramref name="source"/> and a rectangle; this region may alias the operand.</summary>
    /// <param name="source">The region operand.</param>
    /// <param name="x">Left edge of the rectangle.</param>
    /// <param name="y">Top edge of the rectangle.</param>
    /// <param name="width">Width of the rectangle.</param>
    /// <param name="height">Height of the rectangle.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void IntersectRect(PixmanRegion16 source, int x, int y, uint width, uint height)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(source._disposed, source);
        fixed (pixman_region16* d = &Region)
        fixed (pixman_region16* s = &source.Region)
        {
            PixmanException.ThrowIfFalse(Libpixman.pixman_region_intersect_rect(d, s, x, y, width, height), "pixman_region_intersect_rect failed");
        }
    }

    /// <summary>Intersects this region with <paramref name="other"/> in place.</summary>
    /// <param name="other">The region to intersect with.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void IntersectWith(PixmanRegion16 other) => Intersect(this, other);

    /// <summary>Unions this region with <paramref name="other"/> in place.</summary>
    /// <param name="other">The region to union with.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void UnionWith(PixmanRegion16 other) => Union(this, other);

    /// <summary>Subtracts <paramref name="other"/> from this region in place.</summary>
    /// <param name="other">The region to subtract.</param>
    /// <exception cref="PixmanException">Thrown when native memory allocation fails.</exception>
    public void SubtractWith(PixmanRegion16 other) => Subtract(this, other);

    /// <summary>Whether the region contains the given point.</summary>
    /// <param name="x">X coordinate of the point.</param>
    /// <param name="y">Y coordinate of the point.</param>
    /// <returns><see langword="true"/> when the point is inside the region.</returns>
    public bool Contains(int x, int y)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            return Libpixman.pixman_region_contains_point(r, x, y, null) != 0;
        }
    }

    /// <summary>Whether the region contains the given point, returning the rectangle that contains it.</summary>
    /// <param name="x">X coordinate of the point.</param>
    /// <param name="y">Y coordinate of the point.</param>
    /// <param name="containingBox">The region rectangle containing the point, or default when outside.</param>
    /// <returns><see langword="true"/> when the point is inside the region.</returns>
    public bool Contains(int x, int y, out PixmanBox16 containingBox)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            PixmanBox16 box = default;
            var result = Libpixman.pixman_region_contains_point(r, x, y, (pixman_box16*)&box) != 0;
            containingBox = box;
            return result;
        }
    }

    /// <summary>How the given box relates to the region.</summary>
    /// <param name="box">The box to test.</param>
    /// <returns>Whether the box is inside, outside, or partially covered by the region.</returns>
    public PixmanRegionOverlap Contains(PixmanBox16 box)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            return (PixmanRegionOverlap)Libpixman.pixman_region_contains_rectangle(r, (pixman_box16*)&box);
        }
    }

    /// <summary>Copies the rectangles composing the region.</summary>
    /// <returns>The rectangles, or an empty array when the region is empty.</returns>
    public PixmanBox16[] Rectangles()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            int count;
            var boxes = Libpixman.pixman_region_rectangles(r, &count);
            if (count == 0)
            {
                return [];
            }

            return new ReadOnlySpan<PixmanBox16>(boxes, count).ToArray();
        }
    }

    /// <summary>Whether this region covers exactly the same pixels as <paramref name="other"/>.</summary>
    /// <param name="other">The region to compare against.</param>
    /// <returns><see langword="true"/> when the regions are set-equal.</returns>
    /// <remarks>This is set equality; <see cref="object.Equals(object)"/> is not overridden and keeps reference semantics.</remarks>
    public bool Equals(PixmanRegion16 other)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ObjectDisposedException.ThrowIf(other._disposed, other);
        fixed (pixman_region16* a = &Region)
        fixed (pixman_region16* b = &other.Region)
        {
            return Libpixman.pixman_region_equal(a, b) != 0;
        }
    }

    /// <summary>Verifies the region's internal invariants; intended for debugging.</summary>
    /// <returns><see langword="true"/> when the region data structure is consistent.</returns>
    public bool SelfCheck()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            return Libpixman.pixman_region_selfcheck(r) != 0;
        }
    }

    /// <summary>Moves the region by the given offsets.</summary>
    /// <param name="dx">Horizontal offset.</param>
    /// <param name="dy">Vertical offset.</param>
    public void Translate(int dx, int dy)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            Libpixman.pixman_region_translate(r, dx, dy);
        }
    }

    /// <summary>Empties the region.</summary>
    public void Clear()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            Libpixman.pixman_region_clear(r);
        }
    }

    /// <summary>Replaces the region with a single box.</summary>
    /// <param name="box">The box the region covers afterwards.</param>
    public void Reset(PixmanBox16 box)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        fixed (pixman_region16* r = &Region)
        {
            Libpixman.pixman_region_reset(r, (pixman_box16*)&box);
        }
    }

    /// <summary>Releases the native rectangle storage.</summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            fixed (pixman_region16* r = &Region)
            {
                Libpixman.pixman_region_fini(r);
            }

            _disposed = true;
        }
    }
}
