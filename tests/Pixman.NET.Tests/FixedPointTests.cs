using Xunit;

namespace Pixman.Tests;

public class FixedPointTests
{
    [Fact]
    public void Constants_HaveExpectedRawValues()
    {
        Assert.Equal(65536, PixmanFixed.One.Raw);
        Assert.Equal(1, PixmanFixed.E.Raw);
        Assert.Equal(65535, PixmanFixed.OneMinusE.Raw);
        Assert.Equal(-65536, PixmanFixed.MinusOne.Raw);
        Assert.Equal(0, PixmanFixed.Zero.Raw);
    }

    [Fact]
    public void FromInt_ToInt_RoundTrips()
    {
        Assert.Equal(42, PixmanFixed.FromInt(42).ToInt());
        Assert.Equal(-7, PixmanFixed.FromInt(-7).ToInt());
        Assert.Equal(42 << 16, PixmanFixed.FromInt(42).Raw);
    }

    [Fact]
    public void FromDouble_ToDouble_RoundTrips()
    {
        Assert.Equal(1.5, PixmanFixed.FromDouble(1.5).ToDouble());
        Assert.Equal(-0.25, PixmanFixed.FromDouble(-0.25).ToDouble());
        Assert.Equal(98304, PixmanFixed.FromDouble(1.5).Raw);
    }

    [Fact]
    public void ToInt_TruncatesTowardNegativeInfinity()
    {
        // pixman_fixed_to_int is an arithmetic >> 16, i.e. floor semantics.
        Assert.Equal(1, PixmanFixed.FromDouble(1.75).ToInt());
        Assert.Equal(-2, PixmanFixed.FromDouble(-1.25).ToInt());
    }

    [Fact]
    public void FracFloorCeil_MatchMacroSemantics()
    {
        var positive = PixmanFixed.FromDouble(1.25);
        Assert.Equal(0.25, positive.Frac.ToDouble());
        Assert.Equal(1.0, positive.Floor.ToDouble());
        Assert.Equal(2.0, positive.Ceil.ToDouble());

        // C macro semantics on negative values: frac/floor come from the two's
        // complement bit pattern, so frac(-0.25) is +0.75 and floor(-0.25) is -1.
        var negative = PixmanFixed.FromDouble(-0.25);
        Assert.Equal(0.75, negative.Frac.ToDouble());
        Assert.Equal(-1.0, negative.Floor.ToDouble());
        Assert.Equal(0.0, negative.Ceil.ToDouble());

        var exact = PixmanFixed.FromInt(3);
        Assert.Equal(0.0, exact.Frac.ToDouble());
        Assert.Equal(3.0, exact.Floor.ToDouble());
        Assert.Equal(3.0, exact.Ceil.ToDouble());
    }

    [Fact]
    public void Operators_Work()
    {
        var one = PixmanFixed.One;
        var half = PixmanFixed.FromDouble(0.5);

        Assert.Equal(1.5, (one + half).ToDouble());
        Assert.Equal(0.5, (one - half).ToDouble());
        Assert.Equal(-1.0, (-one).ToDouble());
        Assert.True(half < one);
        Assert.True(one > half);
        Assert.True(half <= one);
        Assert.True(one >= half);
        Assert.True(half <= PixmanFixed.FromDouble(0.5));
        Assert.True(half >= PixmanFixed.FromDouble(0.5));
        Assert.True(half == PixmanFixed.FromRaw(32768));
        Assert.True(half != one);
        Assert.Equal(-1, half.CompareTo(one));
    }

    [Fact]
    public void ImplicitConversions_Work()
    {
        PixmanFixed fromInt = 3;
        Assert.Equal(3 << 16, fromInt.Raw);

        PixmanFixed fromDouble = 0.5;
        Assert.Equal(32768, fromDouble.Raw);

        double back = fromDouble;
        Assert.Equal(0.5, back);
    }

    [Fact]
    public void ToString_IsInvariant()
    {
        Assert.Equal("1.5", PixmanFixed.FromDouble(1.5).ToString());
        Assert.Equal("-0.25", PixmanFixed.FromDouble(-0.25).ToString());
    }

    [Fact]
    public void FormatExtensions_ExtractChannelWidths()
    {
        Assert.Equal(32, PixmanFormat.A8R8G8B8.Bpp());
        Assert.Equal(32, PixmanFormat.A8R8G8B8.Depth());
        Assert.Equal(8, PixmanFormat.A8R8G8B8.AlphaBits());
        Assert.True(PixmanFormat.A8R8G8B8.IsColor());

        Assert.Equal(24, PixmanFormat.X8R8G8B8.Depth());
        Assert.Equal(0, PixmanFormat.X8R8G8B8.AlphaBits());

        Assert.Equal(16, PixmanFormat.R5G6B5.Bpp());
        Assert.Equal(16, PixmanFormat.R5G6B5.Depth());
        Assert.Equal(5, PixmanFormat.R5G6B5.RedBits());
        Assert.Equal(6, PixmanFormat.R5G6B5.GreenBits());
        Assert.Equal(5, PixmanFormat.R5G6B5.BlueBits());

        Assert.Equal(8, PixmanFormat.A8.Bpp());
        Assert.Equal(8, PixmanFormat.A8.Depth());
        Assert.Equal(8, PixmanFormat.A8.AlphaBits());
        Assert.False(PixmanFormat.A8.IsColor());

        Assert.Equal(1, PixmanFormat.A1.Bpp());

        // Wide formats store bpp / 8 with a reshift nibble; exercise that path.
        Assert.Equal(128, PixmanFormat.RgbaFloat.Bpp());
        Assert.Equal(96, PixmanFormat.RgbFloat.Bpp());
        Assert.Equal(32, PixmanFormat.RgbaFloat.AlphaBits());
    }

    [Fact]
    public void FormatSupport_QueriesNativeLibrary()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        Assert.True(PixmanFormat.A8R8G8B8.IsSupportedSource());
        Assert.True(PixmanFormat.A8R8G8B8.IsSupportedDestination());
    }
}
