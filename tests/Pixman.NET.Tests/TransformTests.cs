using Xunit;

namespace Pixman.Tests;

public class TransformTests
{
    [Fact]
    public void Identity_IsIdentity()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var identity = PixmanTransform.Identity;
        Assert.True(identity.IsIdentity);
        Assert.Equal(PixmanFixed.One, identity[0, 0]);
        Assert.Equal(PixmanFixed.Zero, identity[0, 1]);
        Assert.Equal(PixmanFixed.One, identity[2, 2]);
    }

    [Fact]
    public void CreateScale_TransformsPoint()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var scale = PixmanTransform.CreateScale(2, 3);
        Assert.True(scale.IsScale);

        var vector = new PixmanVector(1, 1, PixmanFixed.One);
        scale.TransformPoint(ref vector);
        Assert.Equal(2.0, vector.X.ToDouble());
        Assert.Equal(3.0, vector.Y.ToDouble());
    }

    [Fact]
    public void CreateTranslate_TransformsPoint()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var translate = PixmanTransform.CreateTranslate(5, -2);
        Assert.True(translate.IsIntTranslate);

        var vector = new PixmanVector(1, 1, PixmanFixed.One);
        translate.TransformPoint(ref vector);
        Assert.Equal(6.0, vector.X.ToDouble());
        Assert.Equal(-1.0, vector.Y.ToDouble());
    }

    [Fact]
    public void Multiply_AppliesRightOperandFirst()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        // Multiply(l, r) composes so that r is applied to the point first:
        // (l×r)·v = l·(r·v).
        var translate = PixmanTransform.CreateTranslate(1, 0);
        var scale = PixmanTransform.CreateScale(2, 2);

        var composed = PixmanTransform.Multiply(translate, scale);
        var vector = new PixmanVector(1, 1, PixmanFixed.One);
        composed.TransformPoint(ref vector);
        Assert.Equal(3.0, vector.X.ToDouble()); // scale → 2, translate → 3
        Assert.Equal(2.0, vector.Y.ToDouble());

        var reversed = PixmanTransform.Multiply(scale, translate);
        vector = new PixmanVector(1, 1, PixmanFixed.One);
        reversed.TransformPoint(ref vector);
        Assert.Equal(4.0, vector.X.ToDouble()); // translate → 2, scale → 4
    }

    [Fact]
    public void Invert_RoundTrips()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var scale = PixmanTransform.CreateScale(2, 4);
        var inverse = scale.Invert();
        Assert.True(scale.IsInverseOf(inverse));

        var vector = new PixmanVector(2, 4, PixmanFixed.One);
        inverse.TransformPoint(ref vector);
        Assert.Equal(1.0, vector.X.ToDouble());
        Assert.Equal(1.0, vector.Y.ToDouble());
    }

    [Fact]
    public void InstanceMutators_ComposeInPlace()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var transform = PixmanTransform.Identity;
        transform.Scale(2, 2);
        transform.Translate(3, 0);

        var vector = new PixmanVector(1, 0, PixmanFixed.One);
        transform.TransformPoint(ref vector);
        // pixman composes each mutator on the left (applied after the existing
        // map): scale first takes (1,0) to (2,0), then the translate to (5,0).
        Assert.Equal(5.0, vector.X.ToDouble());
        Assert.Equal(0.0, vector.Y.ToDouble());
    }

    [Fact]
    public void Bounds_MapsBox()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var scale = PixmanTransform.CreateScale(2, 2);
        var box = new PixmanBox16(0, 0, 10, 10);
        scale.Bounds(ref box);
        Assert.Equal(0, box.X1);
        Assert.Equal(20, box.X2);
        Assert.Equal(20, box.Y2);
    }

    [Fact]
    public void FTransform_ScaleAndInvert()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var scale = PixmanFTransform.CreateScale(2.0, 4.0);
        Assert.Equal(2.0, scale[0, 0]);
        Assert.Equal(4.0, scale[1, 1]);

        var vector = new PixmanFVector(1.0, 1.0, 1.0);
        scale.TransformPoint(ref vector);
        Assert.Equal(2.0, vector.X, 10);
        Assert.Equal(4.0, vector.Y, 10);

        var inverse = scale.Invert();
        var back = new PixmanFVector(2.0, 4.0, 1.0);
        inverse.TransformPoint(ref back);
        Assert.Equal(1.0, back.X, 10);
        Assert.Equal(1.0, back.Y, 10);
    }

    [Fact]
    public void FixedAndFloatTransforms_ConvertBothWays()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var fixedScale = PixmanTransform.CreateScale(2, 3);
        var floatScale = fixedScale.ToFTransform();
        Assert.Equal(2.0, floatScale[0, 0], 10);
        Assert.Equal(3.0, floatScale[1, 1], 10);

        var roundTripped = PixmanTransform.FromFTransform(floatScale);
        Assert.Equal(fixedScale[0, 0], roundTripped[0, 0]);
        Assert.Equal(fixedScale[1, 1], roundTripped[1, 1]);
    }
}
