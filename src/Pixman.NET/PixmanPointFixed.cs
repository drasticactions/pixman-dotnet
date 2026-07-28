namespace Pixman;

/// <summary>A fixed-point point.</summary>
public struct PixmanPointFixed
{
    /// <summary>The x coordinate.</summary>
    public PixmanFixed X;

    /// <summary>The y coordinate.</summary>
    public PixmanFixed Y;

    /// <summary>Initializes a new instance of the <see cref="PixmanPointFixed"/> struct.</summary>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public PixmanPointFixed(PixmanFixed x, PixmanFixed y)
    {
        X = x;
        Y = y;
    }
}
