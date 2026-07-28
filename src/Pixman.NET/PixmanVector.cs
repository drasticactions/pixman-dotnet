namespace Pixman;

/// <summary>A fixed-point homogeneous vector.</summary>
public struct PixmanVector
{
    /// <summary>The x component.</summary>
    public PixmanFixed X;

    /// <summary>The y component.</summary>
    public PixmanFixed Y;

    /// <summary>The z (homogeneous) component.</summary>
    public PixmanFixed Z;

    /// <summary>Initializes a new instance of the <see cref="PixmanVector"/> struct.</summary>
    /// <param name="x">The x component.</param>
    /// <param name="y">The y component.</param>
    /// <param name="z">The z (homogeneous) component.</param>
    public PixmanVector(PixmanFixed x, PixmanFixed y, PixmanFixed z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
