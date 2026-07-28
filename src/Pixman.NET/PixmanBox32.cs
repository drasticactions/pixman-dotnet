namespace Pixman;

/// <summary>A 32-bit box given by its corners.</summary>
public struct PixmanBox32
{
    /// <summary>The left edge.</summary>
    public int X1;

    /// <summary>The top edge.</summary>
    public int Y1;

    /// <summary>The right edge (exclusive).</summary>
    public int X2;

    /// <summary>The bottom edge (exclusive).</summary>
    public int Y2;

    /// <summary>Initializes a new instance of the <see cref="PixmanBox32"/> struct.</summary>
    /// <param name="x1">The left edge.</param>
    /// <param name="y1">The top edge.</param>
    /// <param name="x2">The right edge (exclusive).</param>
    /// <param name="y2">The bottom edge (exclusive).</param>
    public PixmanBox32(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
    }

    /// <summary>The width of the box.</summary>
    public readonly int Width => X2 - X1;

    /// <summary>The height of the box.</summary>
    public readonly int Height => Y2 - Y1;
}
