namespace Pixman;

/// <summary>A 16-bit-per-channel color. Channels are not premultiplied by alpha.</summary>
public readonly struct PixmanColor
{
    /// <summary>The red channel.</summary>
    public readonly ushort Red;

    /// <summary>The green channel.</summary>
    public readonly ushort Green;

    /// <summary>The blue channel.</summary>
    public readonly ushort Blue;

    /// <summary>The alpha channel.</summary>
    public readonly ushort Alpha;

    /// <summary>Initializes a new instance of the <see cref="PixmanColor"/> struct from 16-bit channels.</summary>
    /// <param name="red">The red channel.</param>
    /// <param name="green">The green channel.</param>
    /// <param name="blue">The blue channel.</param>
    /// <param name="alpha">The alpha channel.</param>
    public PixmanColor(ushort red, ushort green, ushort blue, ushort alpha)
    {
        Red = red;
        Green = green;
        Blue = blue;
        Alpha = alpha;
    }

    /// <summary>Creates a color from 8-bit channels, widening each to 16 bits.</summary>
    /// <param name="red">The red channel.</param>
    /// <param name="green">The green channel.</param>
    /// <param name="blue">The blue channel.</param>
    /// <param name="alpha">The alpha channel.</param>
    public static PixmanColor FromRgba(byte red, byte green, byte blue, byte alpha)
        => new((ushort)(red * 0x0101), (ushort)(green * 0x0101), (ushort)(blue * 0x0101), (ushort)(alpha * 0x0101));
}
