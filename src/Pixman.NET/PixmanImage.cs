using Pixman.Native;

namespace Pixman;

/// <summary>A pixman image: a bits surface, a solid fill, or a gradient, usable as source, mask, or destination of compositing operations.</summary>
/// <remarks>
/// Wraps <c>pixman_image_t</c>. The raw pointer is exposed via <see cref="Handle"/> so consumers can
/// drop down to <see cref="Libpixman"/> and mix layers freely. APIs without a safe wrapper remain
/// available on the raw layer only: <c>pixman_image_set_accessors</c>,
/// <c>pixman_image_set_destroy_function</c>, <c>pixman_image_set_indexed</c>, manual ref-counting
/// (<c>pixman_image_ref</c>), and the float variants <c>pixman_image_set_clip_region64f</c> and
/// <c>pixman_image_composite64f</c>.
/// </remarks>
public sealed unsafe class PixmanImage : IDisposable
{
    private pixman_image* _image;

    private PixmanImage(pixman_image* image) => _image = image;

    /// <summary>Gets the raw <c>pixman_image_t*</c> for use with the <see cref="Libpixman"/> layer.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public IntPtr Handle => (IntPtr)NativePtr;

    /// <summary>Gets a value indicating whether the image has been disposed.</summary>
    public bool IsDisposed => _image is null;

    internal pixman_image* NativePtr
    {
        get
        {
            ObjectDisposedException.ThrowIf(_image is null, this);
            return _image;
        }
    }

    /// <summary>Creates a bits image whose pixel memory is allocated (and zeroed) by pixman.</summary>
    /// <param name="format">The pixel format.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="PixmanException">Thrown when the image cannot be created.</exception>
    public static PixmanImage CreateBits(PixmanFormat format, int width, int height)
    {
        var image = Libpixman.pixman_image_create_bits((pixman_format_code_t)format, width, height, null, 0);
        PixmanException.ThrowIfNull(image, $"pixman_image_create_bits({format}, {width}x{height}) failed");
        return new PixmanImage(image);
    }

    /// <summary>Creates a bits image whose pixel memory is allocated by pixman and left uninitialized.</summary>
    /// <param name="format">The pixel format.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="PixmanException">Thrown when the image cannot be created.</exception>
    public static PixmanImage CreateBitsNoClear(PixmanFormat format, int width, int height)
    {
        var image = Libpixman.pixman_image_create_bits_no_clear((pixman_format_code_t)format, width, height, null, 0);
        PixmanException.ThrowIfNull(image, $"pixman_image_create_bits_no_clear({format}, {width}x{height}) failed");
        return new PixmanImage(image);
    }

    /// <summary>Creates a bits image over caller-owned pixel memory.</summary>
    /// <param name="format">The pixel format.</param>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="bits">The pixel memory; the caller owns it and it must stay valid for the image's lifetime.</param>
    /// <param name="strideBytes">The row stride in bytes; must be a multiple of <c>sizeof(uint)</c>.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="PixmanException">Thrown when the image cannot be created.</exception>
    public static PixmanImage CreateBits(PixmanFormat format, int width, int height, IntPtr bits, int strideBytes)
    {
        var image = Libpixman.pixman_image_create_bits((pixman_format_code_t)format, width, height, (uint*)bits, strideBytes);
        PixmanException.ThrowIfNull(image, $"pixman_image_create_bits({format}, {width}x{height}, stride {strideBytes}) failed");
        return new PixmanImage(image);
    }

    /// <summary>Creates a solid-fill image of the given color.</summary>
    /// <param name="color">The fill color.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="PixmanException">Thrown when the image cannot be created.</exception>
    public static PixmanImage CreateSolidFill(PixmanColor color)
    {
        var image = Libpixman.pixman_image_create_solid_fill((pixman_color*)&color);
        PixmanException.ThrowIfNull(image, "pixman_image_create_solid_fill failed");
        return new PixmanImage(image);
    }

    /// <summary>Creates a linear gradient image along the line from <paramref name="p1"/> to <paramref name="p2"/>.</summary>
    /// <param name="p1">The gradient start point.</param>
    /// <param name="p2">The gradient end point.</param>
    /// <param name="stops">The gradient stops.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="PixmanException">Thrown when the image cannot be created.</exception>
    public static PixmanImage CreateLinearGradient(PixmanPointFixed p1, PixmanPointFixed p2, ReadOnlySpan<PixmanGradientStop> stops)
    {
        fixed (PixmanGradientStop* stopsPtr = stops)
        {
            var image = Libpixman.pixman_image_create_linear_gradient(
                (pixman_point_fixed*)&p1, (pixman_point_fixed*)&p2, (pixman_gradient_stop*)stopsPtr, stops.Length);
            PixmanException.ThrowIfNull(image, "pixman_image_create_linear_gradient failed");
            return new PixmanImage(image);
        }
    }

    /// <summary>Creates a radial gradient image between two circles.</summary>
    /// <param name="inner">The center of the inner circle.</param>
    /// <param name="outer">The center of the outer circle.</param>
    /// <param name="innerRadius">The radius of the inner circle.</param>
    /// <param name="outerRadius">The radius of the outer circle.</param>
    /// <param name="stops">The gradient stops.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="PixmanException">Thrown when the image cannot be created.</exception>
    public static PixmanImage CreateRadialGradient(PixmanPointFixed inner, PixmanPointFixed outer, PixmanFixed innerRadius, PixmanFixed outerRadius, ReadOnlySpan<PixmanGradientStop> stops)
    {
        fixed (PixmanGradientStop* stopsPtr = stops)
        {
            var image = Libpixman.pixman_image_create_radial_gradient(
                (pixman_point_fixed*)&inner, (pixman_point_fixed*)&outer, innerRadius.Raw, outerRadius.Raw, (pixman_gradient_stop*)stopsPtr, stops.Length);
            PixmanException.ThrowIfNull(image, "pixman_image_create_radial_gradient failed");
            return new PixmanImage(image);
        }
    }

    /// <summary>Creates a conical gradient image around <paramref name="center"/>.</summary>
    /// <param name="center">The gradient center.</param>
    /// <param name="angle">The start angle in degrees, as a fixed-point value.</param>
    /// <param name="stops">The gradient stops.</param>
    /// <returns>The new image.</returns>
    /// <exception cref="PixmanException">Thrown when the image cannot be created.</exception>
    public static PixmanImage CreateConicalGradient(PixmanPointFixed center, PixmanFixed angle, ReadOnlySpan<PixmanGradientStop> stops)
    {
        fixed (PixmanGradientStop* stopsPtr = stops)
        {
            var image = Libpixman.pixman_image_create_conical_gradient(
                (pixman_point_fixed*)&center, angle.Raw, (pixman_gradient_stop*)stopsPtr, stops.Length);
            PixmanException.ThrowIfNull(image, "pixman_image_create_conical_gradient failed");
            return new PixmanImage(image);
        }
    }

    /// <summary>Gets the width in pixels, or 0 for a non-bits image.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public int Width => Libpixman.pixman_image_get_width(NativePtr);

    /// <summary>Gets the height in pixels, or 0 for a non-bits image.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public int Height => Libpixman.pixman_image_get_height(NativePtr);

    /// <summary>Gets the row stride in bytes, or 0 for a non-bits image.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public int Stride => Libpixman.pixman_image_get_stride(NativePtr);

    /// <summary>Gets the color depth in bits, or 0 for a non-bits image.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public int Depth => Libpixman.pixman_image_get_depth(NativePtr);

    /// <summary>Gets the pixel format, or 0 for a non-bits image.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public PixmanFormat Format => (PixmanFormat)Libpixman.pixman_image_get_format(NativePtr);

    /// <summary>Gets a pointer to the pixel memory, or <see cref="IntPtr.Zero"/> for a non-bits image.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public IntPtr Data => (IntPtr)Libpixman.pixman_image_get_data(NativePtr);

    /// <summary>Gets the pixel memory as a span of <c>Stride * Height</c> bytes, or an empty span for a non-bits image.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public Span<byte> DataSpan
    {
        get
        {
            var data = Libpixman.pixman_image_get_data(NativePtr);
            return data is null ? [] : new Span<byte>(data, Stride * Height);
        }
    }

    /// <summary>Gets or sets whether the image's color channels are treated as separate alpha-weighted components when used as a mask.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public bool ComponentAlpha
    {
        get => Libpixman.pixman_image_get_component_alpha(NativePtr) != 0;
        set => Libpixman.pixman_image_set_component_alpha(NativePtr, value ? 1 : 0);
    }

    /// <summary>Sets how the image repeats when sampled outside its bounds.</summary>
    /// <param name="repeat">The repeat mode.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetRepeat(PixmanRepeat repeat) => Libpixman.pixman_image_set_repeat(NativePtr, (pixman_repeat_t)repeat);

    /// <summary>Sets the sampling filter used when the image is transformed.</summary>
    /// <param name="filter">The filter.</param>
    /// <exception cref="PixmanException">Thrown when the filter cannot be set.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetFilter(PixmanFilter filter)
        => PixmanException.ThrowIfFalse(
            Libpixman.pixman_image_set_filter(NativePtr, (pixman_filter_t)filter, null, 0),
            "pixman_image_set_filter failed");

    /// <summary>Sets the sampling filter with parameters (for the convolution filters).</summary>
    /// <param name="filter">The filter.</param>
    /// <param name="parameters">The filter parameters; for <see cref="PixmanFilter.Convolution"/> the first two values are the kernel width and height.</param>
    /// <exception cref="PixmanException">Thrown when the filter cannot be set.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetFilter(PixmanFilter filter, ReadOnlySpan<PixmanFixed> parameters)
    {
        fixed (PixmanFixed* parametersPtr = parameters)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_image_set_filter(NativePtr, (pixman_filter_t)filter, (int*)parametersPtr, parameters.Length),
                "pixman_image_set_filter failed");
        }
    }

    /// <summary>Sets the transformation applied when the image is used as a source.</summary>
    /// <param name="transform">The transformation matrix; pixman copies it.</param>
    /// <exception cref="PixmanException">Thrown when the transform cannot be set.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetTransform(in PixmanTransform transform)
    {
        var native = transform.Native;
        PixmanException.ThrowIfFalse(
            Libpixman.pixman_image_set_transform(NativePtr, &native),
            "pixman_image_set_transform failed");
    }

    /// <summary>Removes the image's transformation, restoring the identity transform.</summary>
    /// <exception cref="PixmanException">Thrown when the transform cannot be cleared.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void ClearTransform()
        => PixmanException.ThrowIfFalse(
            Libpixman.pixman_image_set_transform(NativePtr, null),
            "pixman_image_set_transform(NULL) failed");

    /// <summary>Sets the clip region; pixman copies it.</summary>
    /// <param name="region">The clip region.</param>
    /// <exception cref="PixmanException">Thrown when the clip region cannot be set.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image or the region has been disposed.</exception>
    public void SetClipRegion(PixmanRegion16 region)
    {
        fixed (pixman_region16* regionPtr = &region.Region)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_image_set_clip_region(NativePtr, regionPtr),
                "pixman_image_set_clip_region failed");
        }
    }

    /// <summary>Sets the clip region; pixman copies it.</summary>
    /// <param name="region">The clip region.</param>
    /// <exception cref="PixmanException">Thrown when the clip region cannot be set.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image or the region has been disposed.</exception>
    public void SetClipRegion(PixmanRegion32 region)
    {
        fixed (pixman_region32* regionPtr = &region.Region)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_image_set_clip_region32(NativePtr, regionPtr),
                "pixman_image_set_clip_region32 failed");
        }
    }

    /// <summary>Removes the clip region.</summary>
    /// <exception cref="PixmanException">Thrown when the clip region cannot be cleared.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void ClearClipRegion()
        => PixmanException.ThrowIfFalse(
            Libpixman.pixman_image_set_clip_region32(NativePtr, null),
            "pixman_image_set_clip_region32(NULL) failed");

    /// <summary>Sets or clears the image's separate alpha map.</summary>
    /// <param name="alphaMap">The alpha map image, or <see langword="null"/> to clear it. The image keeps its own reference.</param>
    /// <param name="originX">The x origin of the alpha map relative to the image.</param>
    /// <param name="originY">The y origin of the alpha map relative to the image.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image (or a non-null <paramref name="alphaMap"/>) has been disposed.</exception>
    public void SetAlphaMap(PixmanImage? alphaMap, short originX, short originY)
        => Libpixman.pixman_image_set_alpha_map(NativePtr, alphaMap is null ? null : alphaMap.NativePtr, originX, originY);

    /// <summary>Sets whether the clip region also restricts the image when used as a source.</summary>
    /// <param name="sourceClipping">Whether source clipping is enabled.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetSourceClipping(bool sourceClipping)
        => Libpixman.pixman_image_set_source_clipping(NativePtr, sourceClipping ? 1 : 0);

    /// <summary>Sets whether the clip region was supplied by the client rather than computed by pixman.</summary>
    /// <param name="hasClientClip">Whether the clip is client-provided.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetHasClientClip(bool hasClientClip)
        => Libpixman.pixman_image_set_has_client_clip(NativePtr, hasClientClip ? 1 : 0);

    /// <summary>Sets the dithering applied when compositing into this destination.</summary>
    /// <param name="dither">The dither mode.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetDither(PixmanDither dither) => Libpixman.pixman_image_set_dither(NativePtr, (pixman_dither_t)dither);

    /// <summary>Sets the origin offset of the dither matrix.</summary>
    /// <param name="x">The x offset.</param>
    /// <param name="y">The y offset.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void SetDitherOffset(int x, int y) => Libpixman.pixman_image_set_dither_offset(NativePtr, x, y);

    /// <summary>Fills rectangles of this image with a color using the given operator.</summary>
    /// <param name="op">The compositing operator.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="rects">The rectangles to fill.</param>
    /// <exception cref="PixmanException">Thrown when the fill fails.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void Fill(PixmanOp op, PixmanColor color, ReadOnlySpan<PixmanRectangle16> rects)
    {
        fixed (PixmanRectangle16* rectsPtr = rects)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_image_fill_rectangles((pixman_op_t)op, NativePtr, (pixman_color*)&color, rects.Length, (pixman_rectangle16*)rectsPtr),
                "pixman_image_fill_rectangles failed");
        }
    }

    /// <summary>Fills boxes of this image with a color using the given operator.</summary>
    /// <param name="op">The compositing operator.</param>
    /// <param name="color">The fill color.</param>
    /// <param name="boxes">The boxes to fill.</param>
    /// <exception cref="PixmanException">Thrown when the fill fails.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void Fill(PixmanOp op, PixmanColor color, ReadOnlySpan<PixmanBox32> boxes)
    {
        fixed (PixmanBox32* boxesPtr = boxes)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_image_fill_boxes((pixman_op_t)op, NativePtr, (pixman_color*)&color, boxes.Length, (pixman_box32*)boxesPtr),
                "pixman_image_fill_boxes failed");
        }
    }

    /// <summary>Composites a source (optionally through a mask) into this image, which is the destination.</summary>
    /// <param name="op">The compositing operator.</param>
    /// <param name="source">The source image.</param>
    /// <param name="mask">The mask image, or <see langword="null"/> for none.</param>
    /// <param name="sourceX">The x coordinate of the source rectangle's origin.</param>
    /// <param name="sourceY">The y coordinate of the source rectangle's origin.</param>
    /// <param name="maskX">The x coordinate of the mask rectangle's origin.</param>
    /// <param name="maskY">The y coordinate of the mask rectangle's origin.</param>
    /// <param name="destX">The x coordinate of the destination rectangle's origin in this image.</param>
    /// <param name="destY">The y coordinate of the destination rectangle's origin in this image.</param>
    /// <param name="width">The width of the composited rectangle.</param>
    /// <param name="height">The height of the composited rectangle.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this image, <paramref name="source"/>, or a non-null <paramref name="mask"/> has been disposed.</exception>
    public void Composite(PixmanOp op, PixmanImage source, PixmanImage? mask, int sourceX, int sourceY, int maskX, int maskY, int destX, int destY, int width, int height)
        => Libpixman.pixman_image_composite32(
            (pixman_op_t)op, source.NativePtr, mask is null ? null : mask.NativePtr, NativePtr,
            sourceX, sourceY, maskX, maskY, destX, destY, width, height);

    /// <summary>Composites trapezoids from a source into this image through an implicit mask of the given format.</summary>
    /// <param name="op">The compositing operator.</param>
    /// <param name="source">The source image.</param>
    /// <param name="maskFormat">The format of the intermediate mask (typically <see cref="PixmanFormat.A8"/>).</param>
    /// <param name="sourceX">The source x origin.</param>
    /// <param name="sourceY">The source y origin.</param>
    /// <param name="destX">The destination x origin.</param>
    /// <param name="destY">The destination y origin.</param>
    /// <param name="traps">The trapezoids.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this image or <paramref name="source"/> has been disposed.</exception>
    public void CompositeTrapezoids(PixmanOp op, PixmanImage source, PixmanFormat maskFormat, int sourceX, int sourceY, int destX, int destY, ReadOnlySpan<PixmanTrapezoid> traps)
    {
        fixed (PixmanTrapezoid* trapsPtr = traps)
        {
            Libpixman.pixman_composite_trapezoids(
                (pixman_op_t)op, source.NativePtr, NativePtr, (pixman_format_code_t)maskFormat,
                sourceX, sourceY, destX, destY, traps.Length, (pixman_trapezoid*)trapsPtr);
        }
    }

    /// <summary>Composites triangles from a source into this image through an implicit mask of the given format.</summary>
    /// <param name="op">The compositing operator.</param>
    /// <param name="source">The source image.</param>
    /// <param name="maskFormat">The format of the intermediate mask (typically <see cref="PixmanFormat.A8"/>).</param>
    /// <param name="sourceX">The source x origin.</param>
    /// <param name="sourceY">The source y origin.</param>
    /// <param name="destX">The destination x origin.</param>
    /// <param name="destY">The destination y origin.</param>
    /// <param name="triangles">The triangles.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this image or <paramref name="source"/> has been disposed.</exception>
    public void CompositeTriangles(PixmanOp op, PixmanImage source, PixmanFormat maskFormat, int sourceX, int sourceY, int destX, int destY, ReadOnlySpan<PixmanTriangle> triangles)
    {
        fixed (PixmanTriangle* trianglesPtr = triangles)
        {
            Libpixman.pixman_composite_triangles(
                (pixman_op_t)op, source.NativePtr, NativePtr, (pixman_format_code_t)maskFormat,
                sourceX, sourceY, destX, destY, triangles.Length, (pixman_triangle*)trianglesPtr);
        }
    }

    /// <summary>Rasterizes triangles additively into this image (typically an alpha-only mask).</summary>
    /// <param name="offsetX">The x offset applied to every triangle.</param>
    /// <param name="offsetY">The y offset applied to every triangle.</param>
    /// <param name="triangles">The triangles.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void AddTriangles(int offsetX, int offsetY, ReadOnlySpan<PixmanTriangle> triangles)
    {
        fixed (PixmanTriangle* trianglesPtr = triangles)
        {
            Libpixman.pixman_add_triangles(NativePtr, offsetX, offsetY, triangles.Length, (pixman_triangle*)trianglesPtr);
        }
    }

    /// <summary>Rasterizes trapezoids additively into this image (typically an alpha-only mask).</summary>
    /// <param name="offsetX">The x offset applied to every trapezoid.</param>
    /// <param name="offsetY">The y offset applied to every trapezoid.</param>
    /// <param name="traps">The trapezoids.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void AddTrapezoids(short offsetX, int offsetY, ReadOnlySpan<PixmanTrapezoid> traps)
    {
        fixed (PixmanTrapezoid* trapsPtr = traps)
        {
            Libpixman.pixman_add_trapezoids(NativePtr, offsetX, offsetY, traps.Length, (pixman_trapezoid*)trapsPtr);
        }
    }

    /// <summary>Rasterizes pre-sampled traps additively into this image (typically an alpha-only mask).</summary>
    /// <param name="offsetX">The x offset applied to every trap.</param>
    /// <param name="offsetY">The y offset applied to every trap.</param>
    /// <param name="traps">The traps.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void AddTraps(short offsetX, short offsetY, ReadOnlySpan<PixmanTrap> traps)
    {
        fixed (PixmanTrap* trapsPtr = traps)
        {
            Libpixman.pixman_add_traps(NativePtr, offsetX, offsetY, traps.Length, (pixman_trap*)trapsPtr);
        }
    }

    /// <summary>Rasterizes a single trapezoid into this alpha image.</summary>
    /// <param name="trap">The trapezoid; must be valid (top &lt; bottom).</param>
    /// <param name="offsetX">The x offset applied to the trapezoid.</param>
    /// <param name="offsetY">The y offset applied to the trapezoid.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void RasterizeTrapezoid(PixmanTrapezoid trap, int offsetX, int offsetY)
        => Libpixman.pixman_rasterize_trapezoid(NativePtr, (pixman_trapezoid*)&trap, offsetX, offsetY);

    /// <summary>Rasterizes the span between two edges into this alpha image, stepping both edges.</summary>
    /// <param name="left">The left edge; its position is advanced by the rasterization.</param>
    /// <param name="right">The right edge; its position is advanced by the rasterization.</param>
    /// <param name="top">The top y coordinate.</param>
    /// <param name="bottom">The bottom y coordinate.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the image has been disposed.</exception>
    public void RasterizeEdges(ref PixmanEdge left, ref PixmanEdge right, PixmanFixed top, PixmanFixed bottom)
    {
        fixed (pixman_edge* leftPtr = &left.Native)
        fixed (pixman_edge* rightPtr = &right.Native)
        {
            Libpixman.pixman_rasterize_edges(NativePtr, leftPtr, rightPtr, top.Raw, bottom.Raw);
        }
    }

    /// <summary>Composites cached glyphs from a source into this image through an intermediate mask.</summary>
    /// <param name="op">The compositing operator.</param>
    /// <param name="source">The source image.</param>
    /// <param name="maskFormat">The format of the intermediate mask.</param>
    /// <param name="sourceX">The source x origin.</param>
    /// <param name="sourceY">The source y origin.</param>
    /// <param name="maskX">The mask x origin.</param>
    /// <param name="maskY">The mask y origin.</param>
    /// <param name="destX">The destination x origin.</param>
    /// <param name="destY">The destination y origin.</param>
    /// <param name="width">The width of the composited rectangle.</param>
    /// <param name="height">The height of the composited rectangle.</param>
    /// <param name="cache">The glyph cache holding the glyphs.</param>
    /// <param name="glyphs">The positioned glyphs, with <see cref="PixmanGlyph.Glyph"/> pointers from <paramref name="cache"/>.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this image, <paramref name="source"/>, or <paramref name="cache"/> has been disposed.</exception>
    public void CompositeGlyphs(PixmanOp op, PixmanImage source, PixmanFormat maskFormat, int sourceX, int sourceY, int maskX, int maskY, int destX, int destY, int width, int height, PixmanGlyphCache cache, ReadOnlySpan<PixmanGlyph> glyphs)
    {
        fixed (PixmanGlyph* glyphsPtr = glyphs)
        {
            Libpixman.pixman_composite_glyphs(
                (pixman_op_t)op, source.NativePtr, NativePtr, (pixman_format_code_t)maskFormat,
                sourceX, sourceY, maskX, maskY, destX, destY, width, height,
                cache.NativePtr, glyphs.Length, (pixman_glyph_t*)glyphsPtr);
        }
    }

    /// <summary>Composites cached glyphs from a source directly into this image, without an intermediate mask.</summary>
    /// <param name="op">The compositing operator.</param>
    /// <param name="source">The source image.</param>
    /// <param name="sourceX">The source x origin.</param>
    /// <param name="sourceY">The source y origin.</param>
    /// <param name="destX">The destination x origin.</param>
    /// <param name="destY">The destination y origin.</param>
    /// <param name="cache">The glyph cache holding the glyphs.</param>
    /// <param name="glyphs">The positioned glyphs, with <see cref="PixmanGlyph.Glyph"/> pointers from <paramref name="cache"/>.</param>
    /// <exception cref="ObjectDisposedException">Thrown when this image, <paramref name="source"/>, or <paramref name="cache"/> has been disposed.</exception>
    public void CompositeGlyphsNoMask(PixmanOp op, PixmanImage source, int sourceX, int sourceY, int destX, int destY, PixmanGlyphCache cache, ReadOnlySpan<PixmanGlyph> glyphs)
    {
        fixed (PixmanGlyph* glyphsPtr = glyphs)
        {
            Libpixman.pixman_composite_glyphs_no_mask(
                (pixman_op_t)op, source.NativePtr, NativePtr,
                sourceX, sourceY, destX, destY,
                cache.NativePtr, glyphs.Length, (pixman_glyph_t*)glyphsPtr);
        }
    }

    /// <summary>Releases the image's reference; pixel memory owned by pixman is freed with the last reference.</summary>
    public void Dispose()
    {
        if (_image is not null)
        {
            Libpixman.pixman_image_unref(_image);
            _image = null;
        }
    }
}
