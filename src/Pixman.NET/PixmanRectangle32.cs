namespace Pixman;

/// <summary>A 32-bit rectangle given by origin and size.</summary>
public struct PixmanRectangle32
{
    /// <summary>The x coordinate of the top-left corner.</summary>
    public int X;

    /// <summary>The y coordinate of the top-left corner.</summary>
    public int Y;

    /// <summary>The width of the rectangle.</summary>
    public uint Width;

    /// <summary>The height of the rectangle.</summary>
    public uint Height;

    /// <summary>Initializes a new instance of the <see cref="PixmanRectangle32"/> struct.</summary>
    /// <param name="x">The x coordinate of the top-left corner.</param>
    /// <param name="y">The y coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public PixmanRectangle32(int x, int y, uint width, uint height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
