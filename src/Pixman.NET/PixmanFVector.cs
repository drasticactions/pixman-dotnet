namespace Pixman;

/// <summary>A double-precision homogeneous vector.</summary>
public struct PixmanFVector
{
    /// <summary>The x component.</summary>
    public double X;

    /// <summary>The y component.</summary>
    public double Y;

    /// <summary>The z (homogeneous) component.</summary>
    public double Z;

    /// <summary>Initializes a new instance of the <see cref="PixmanFVector"/> struct.</summary>
    /// <param name="x">The x component.</param>
    /// <param name="y">The y component.</param>
    /// <param name="z">The z (homogeneous) component.</param>
    public PixmanFVector(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}
