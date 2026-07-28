namespace Pixman;

/// <summary>A trapezoid bounded by two edges and two horizontal lines.</summary>
public struct PixmanTrapezoid
{
    /// <summary>The y coordinate of the top bound.</summary>
    public PixmanFixed Top;

    /// <summary>The y coordinate of the bottom bound.</summary>
    public PixmanFixed Bottom;

    /// <summary>The left edge.</summary>
    public PixmanLineFixed Left;

    /// <summary>The right edge.</summary>
    public PixmanLineFixed Right;

    /// <summary>Initializes a new instance of the <see cref="PixmanTrapezoid"/> struct.</summary>
    /// <param name="top">The y coordinate of the top bound.</param>
    /// <param name="bottom">The y coordinate of the bottom bound.</param>
    /// <param name="left">The left edge.</param>
    /// <param name="right">The right edge.</param>
    public PixmanTrapezoid(PixmanFixed top, PixmanFixed bottom, PixmanLineFixed left, PixmanLineFixed right)
    {
        Top = top;
        Bottom = bottom;
        Left = left;
        Right = right;
    }

    /// <summary>Whether the trapezoid is renderable (<c>pixman_trapezoid_valid</c>): neither edge is horizontal and the bottom is below the top.</summary>
    public readonly bool IsValid
        => Left.P1.Y != Left.P2.Y && Right.P1.Y != Right.P2.Y && Bottom > Top;
}
