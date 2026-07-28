using Pixman.Native;

namespace Pixman;

/// <summary>One polygon edge stepped a scanline at a time.</summary>
/// <remarks>
/// This is the low-level scanline rasterizer state consumed by
/// <c>PixmanImage.RasterizeEdges</c>: initialize a left and a right edge, then rasterize the
/// spans between them.
/// </remarks>
public unsafe struct PixmanEdge
{
    /// <summary>The embedded native edge.</summary>
    internal pixman_edge Native;

    /// <summary>Gets the current x position of the edge.</summary>
    public PixmanFixed X => PixmanFixed.FromRaw(Native.x);

    /// <summary>Gets the current error term.</summary>
    public PixmanFixed E => PixmanFixed.FromRaw(Native.e);

    /// <summary>Gets the x step per scanline.</summary>
    public PixmanFixed StepX => PixmanFixed.FromRaw(Native.stepx);

    /// <summary>Gets the sign of the x direction.</summary>
    public PixmanFixed SignDx => PixmanFixed.FromRaw(Native.signdx);

    /// <summary>Gets the y extent of the edge.</summary>
    public PixmanFixed Dy => PixmanFixed.FromRaw(Native.dy);

    /// <summary>Gets the x extent of the edge.</summary>
    public PixmanFixed Dx => PixmanFixed.FromRaw(Native.dx);

    /// <summary>Gets the x step for a small (sub-scanline) sample step.</summary>
    public PixmanFixed StepXSmall => PixmanFixed.FromRaw(Native.stepx_small);

    /// <summary>Gets the x step for a big (full-scanline) sample step.</summary>
    public PixmanFixed StepXBig => PixmanFixed.FromRaw(Native.stepx_big);

    /// <summary>Gets the error increment for a small sample step.</summary>
    public PixmanFixed DxSmall => PixmanFixed.FromRaw(Native.dx_small);

    /// <summary>Gets the error increment for a big sample step.</summary>
    public PixmanFixed DxBig => PixmanFixed.FromRaw(Native.dx_big);

    /// <summary>Initializes an edge from its top and bottom points.</summary>
    /// <param name="bpp">The bits per pixel of the target alpha image (1, 4 or 8).</param>
    /// <param name="yStart">The y coordinate of the first sample row.</param>
    /// <param name="xTop">The x coordinate of the top point.</param>
    /// <param name="yTop">The y coordinate of the top point.</param>
    /// <param name="xBot">The x coordinate of the bottom point.</param>
    /// <param name="yBot">The y coordinate of the bottom point.</param>
    public static PixmanEdge Init(int bpp, PixmanFixed yStart, PixmanFixed xTop, PixmanFixed yTop, PixmanFixed xBot, PixmanFixed yBot)
    {
        PixmanEdge edge = default;
        Libpixman.pixman_edge_init(&edge.Native, bpp, yStart.Raw, xTop.Raw, yTop.Raw, xBot.Raw, yBot.Raw);
        return edge;
    }

    /// <summary>Initializes an edge from a fixed-point line.</summary>
    /// <param name="bpp">The bits per pixel of the target alpha image (1, 4 or 8).</param>
    /// <param name="y">The y coordinate of the first sample row.</param>
    /// <param name="line">The line to walk.</param>
    /// <param name="xOff">The x offset added to the line.</param>
    /// <param name="yOff">The y offset added to the line.</param>
    public static PixmanEdge FromLine(int bpp, PixmanFixed y, in PixmanLineFixed line, int xOff, int yOff)
    {
        var l = line;
        PixmanEdge edge = default;
        Libpixman.pixman_line_fixed_edge_init(&edge.Native, bpp, y.Raw, (pixman_line_fixed*)&l, xOff, yOff);
        return edge;
    }

    /// <summary>Advances the edge by <paramref name="n"/> sample rows.</summary>
    /// <param name="n">The number of sample rows to step.</param>
    public void Step(int n)
    {
        fixed (pixman_edge* e = &Native)
        {
            Libpixman.pixman_edge_step(e, n);
        }
    }
}
