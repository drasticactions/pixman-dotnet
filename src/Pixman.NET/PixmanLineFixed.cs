namespace Pixman;

/// <summary>A fixed-point line segment.</summary>
public struct PixmanLineFixed
{
    /// <summary>The first endpoint.</summary>
    public PixmanPointFixed P1;

    /// <summary>The second endpoint.</summary>
    public PixmanPointFixed P2;

    /// <summary>Initializes a new instance of the <see cref="PixmanLineFixed"/> struct.</summary>
    /// <param name="p1">The first endpoint.</param>
    /// <param name="p2">The second endpoint.</param>
    public PixmanLineFixed(PixmanPointFixed p1, PixmanPointFixed p2)
    {
        P1 = p1;
        P2 = p2;
    }
}
