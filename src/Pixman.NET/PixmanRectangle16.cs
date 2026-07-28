namespace Pixman;

/// <summary>A 16-bit rectangle given by origin and size.</summary>
public struct PixmanRectangle16
{
    /// <summary>The x coordinate of the top-left corner.</summary>
    public short X;

    /// <summary>The y coordinate of the top-left corner.</summary>
    public short Y;

    /// <summary>The width of the rectangle.</summary>
    public ushort Width;

    /// <summary>The height of the rectangle.</summary>
    public ushort Height;

    /// <summary>Initializes a new instance of the <see cref="PixmanRectangle16"/> struct.</summary>
    /// <param name="x">The x coordinate of the top-left corner.</param>
    /// <param name="y">The y coordinate of the top-left corner.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    public PixmanRectangle16(short x, short y, ushort width, ushort height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}
