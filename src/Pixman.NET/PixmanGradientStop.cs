namespace Pixman;

/// <summary>A gradient color stop.</summary>
public struct PixmanGradientStop
{
    /// <summary>The position of the stop, normally in [0, 1].</summary>
    public PixmanFixed X;

    /// <summary>The color at the stop.</summary>
    public PixmanColor Color;

    /// <summary>Initializes a new instance of the <see cref="PixmanGradientStop"/> struct.</summary>
    /// <param name="x">The position of the stop, normally in [0, 1].</param>
    /// <param name="color">The color at the stop.</param>
    public PixmanGradientStop(PixmanFixed x, PixmanColor color)
    {
        X = x;
        Color = color;
    }
}
