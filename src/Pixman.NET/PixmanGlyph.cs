namespace Pixman;

/// <summary>A positioned glyph reference for batch compositing.</summary>
public struct PixmanGlyph
{
    /// <summary>The x coordinate at which to draw the glyph.</summary>
    public int X;

    /// <summary>The y coordinate at which to draw the glyph.</summary>
    public int Y;

    /// <summary>The cache handle returned by <c>PixmanGlyphCache</c> insert or lookup.</summary>
    public IntPtr Glyph;

    /// <summary>Initializes a new instance of the <see cref="PixmanGlyph"/> struct.</summary>
    /// <param name="x">The x coordinate at which to draw the glyph.</param>
    /// <param name="y">The y coordinate at which to draw the glyph.</param>
    /// <param name="glyph">The cache handle returned by <c>PixmanGlyphCache</c> insert or lookup.</param>
    public PixmanGlyph(int x, int y, IntPtr glyph)
    {
        X = x;
        Y = y;
        Glyph = glyph;
    }
}
