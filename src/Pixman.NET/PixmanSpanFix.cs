namespace Pixman;

/// <summary>A fixed-point horizontal span.</summary>
public struct PixmanSpanFix
{
    /// <summary>The left edge of the span.</summary>
    public PixmanFixed L;

    /// <summary>The right edge of the span.</summary>
    public PixmanFixed R;

    /// <summary>The y coordinate of the span.</summary>
    public PixmanFixed Y;

    /// <summary>Initializes a new instance of the <see cref="PixmanSpanFix"/> struct.</summary>
    /// <param name="l">The left edge of the span.</param>
    /// <param name="r">The right edge of the span.</param>
    /// <param name="y">The y coordinate of the span.</param>
    public PixmanSpanFix(PixmanFixed l, PixmanFixed r, PixmanFixed y)
    {
        L = l;
        R = r;
        Y = y;
    }
}
