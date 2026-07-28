namespace Pixman;

/// <summary>A trapezoid given by its top and bottom spans.</summary>
public struct PixmanTrap
{
    /// <summary>The top span (native field <c>top</c>).</summary>
    public PixmanSpanFix Top;

    /// <summary>The bottom span (native field <c>bot</c>).</summary>
    public PixmanSpanFix Bottom;

    /// <summary>Initializes a new instance of the <see cref="PixmanTrap"/> struct.</summary>
    /// <param name="top">The top span.</param>
    /// <param name="bottom">The bottom span.</param>
    public PixmanTrap(PixmanSpanFix top, PixmanSpanFix bottom)
    {
        Top = top;
        Bottom = bottom;
    }
}
