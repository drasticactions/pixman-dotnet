namespace Pixman;

/// <summary>A fixed-point triangle.</summary>
public struct PixmanTriangle
{
    /// <summary>The first vertex.</summary>
    public PixmanPointFixed P1;

    /// <summary>The second vertex.</summary>
    public PixmanPointFixed P2;

    /// <summary>The third vertex.</summary>
    public PixmanPointFixed P3;

    /// <summary>Initializes a new instance of the <see cref="PixmanTriangle"/> struct.</summary>
    /// <param name="p1">The first vertex.</param>
    /// <param name="p2">The second vertex.</param>
    /// <param name="p3">The third vertex.</param>
    public PixmanTriangle(PixmanPointFixed p1, PixmanPointFixed p2, PixmanPointFixed p3)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
    }
}
