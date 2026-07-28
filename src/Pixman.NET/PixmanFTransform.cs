using Pixman.Native;

namespace Pixman;

/// <summary>A 3×3 double-precision transformation matrix.</summary>
public unsafe struct PixmanFTransform
{
    /// <summary>The embedded native transform.</summary>
    internal pixman_f_transform Native;

    /// <summary>Gets or sets the matrix element at the given position.</summary>
    /// <param name="row">The row index, 0–2.</param>
    /// <param name="column">The column index, 0–2.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an index is outside 0–2.</exception>
    public double this[int row, int column]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)row, 2u, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)column, 2u, nameof(column));
            return Native.m[(row * 3) + column];
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)row, 2u, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)column, 2u, nameof(column));
            Native.m[(row * 3) + column] = value;
        }
    }

    /// <summary>Gets the identity transform.</summary>
    public static PixmanFTransform Identity
    {
        get
        {
            PixmanFTransform t = default;
            Libpixman.pixman_f_transform_init_identity(&t.Native);
            return t;
        }
    }

    /// <summary>Creates a scaling transform.</summary>
    /// <param name="sx">The horizontal scale factor.</param>
    /// <param name="sy">The vertical scale factor.</param>
    public static PixmanFTransform CreateScale(double sx, double sy)
    {
        PixmanFTransform t = default;
        Libpixman.pixman_f_transform_init_scale(&t.Native, sx, sy);
        return t;
    }

    /// <summary>Creates a rotation transform from the cosine and sine of the angle.</summary>
    /// <param name="cos">The cosine of the rotation angle.</param>
    /// <param name="sin">The sine of the rotation angle.</param>
    public static PixmanFTransform CreateRotate(double cos, double sin)
    {
        PixmanFTransform t = default;
        Libpixman.pixman_f_transform_init_rotate(&t.Native, cos, sin);
        return t;
    }

    /// <summary>Creates a translation transform.</summary>
    /// <param name="tx">The horizontal offset.</param>
    /// <param name="ty">The vertical offset.</param>
    public static PixmanFTransform CreateTranslate(double tx, double ty)
    {
        PixmanFTransform t = default;
        Libpixman.pixman_f_transform_init_translate(&t.Native, tx, ty);
        return t;
    }

    /// <summary>Multiplies two transforms, returning <c>l · r</c>.</summary>
    /// <param name="l">The left operand.</param>
    /// <param name="r">The right operand.</param>
    public static PixmanFTransform Multiply(in PixmanFTransform l, in PixmanFTransform r)
    {
        var left = l;
        var right = r;
        PixmanFTransform dst = default;
        Libpixman.pixman_f_transform_multiply(&dst.Native, &left.Native, &right.Native);
        return dst;
    }

    /// <summary>Multiplies an additional scale onto this transform.</summary>
    /// <param name="sx">The horizontal scale factor.</param>
    /// <param name="sy">The vertical scale factor.</param>
    /// <exception cref="PixmanException">Thrown when a factor is zero.</exception>
    public void Scale(double sx, double sy)
    {
        fixed (pixman_f_transform* t = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_f_transform_scale(t, null, sx, sy),
                "pixman_f_transform_scale failed");
        }
    }

    /// <summary>Multiplies an additional rotation onto this transform.</summary>
    /// <param name="cos">The cosine of the rotation angle.</param>
    /// <param name="sin">The sine of the rotation angle.</param>
    /// <exception cref="PixmanException">Thrown when the native call reports failure.</exception>
    public void Rotate(double cos, double sin)
    {
        fixed (pixman_f_transform* t = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_f_transform_rotate(t, null, cos, sin),
                "pixman_f_transform_rotate failed");
        }
    }

    /// <summary>Multiplies an additional translation onto this transform.</summary>
    /// <param name="tx">The horizontal offset.</param>
    /// <param name="ty">The vertical offset.</param>
    /// <exception cref="PixmanException">Thrown when the native call reports failure.</exception>
    public void Translate(double tx, double ty)
    {
        fixed (pixman_f_transform* t = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_f_transform_translate(t, null, tx, ty),
                "pixman_f_transform_translate failed");
        }
    }

    /// <summary>Computes the inverse of this transform.</summary>
    /// <exception cref="PixmanException">Thrown when the matrix is singular.</exception>
    public PixmanFTransform Invert()
    {
        PixmanFTransform dst = default;
        fixed (pixman_f_transform* src = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_f_transform_invert(&dst.Native, src),
                "pixman_f_transform_invert failed: matrix is singular");
        }

        return dst;
    }

    /// <summary>Transforms a homogeneous vector and divides through by the resulting w.</summary>
    /// <param name="vector">The vector to transform in place.</param>
    /// <exception cref="PixmanException">Thrown when the resulting w is zero.</exception>
    public void TransformPoint(ref PixmanFVector vector)
    {
        fixed (pixman_f_transform* t = &Native)
        fixed (PixmanFVector* v = &vector)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_f_transform_point(t, (pixman_f_vector*)v),
                "pixman_f_transform_point failed");
        }
    }

    /// <summary>Transforms a homogeneous vector without the perspective divide.</summary>
    /// <param name="vector">The vector to transform in place.</param>
    public void TransformPoint3D(ref PixmanFVector vector)
    {
        fixed (pixman_f_transform* t = &Native)
        fixed (PixmanFVector* v = &vector)
        {
            Libpixman.pixman_f_transform_point_3d(t, (pixman_f_vector*)v);
        }
    }

    /// <summary>Replaces a box with the bounding box of its transformed corners.</summary>
    /// <param name="box">The box to transform in place.</param>
    /// <exception cref="PixmanException">Thrown when the computation fails.</exception>
    public void Bounds(ref PixmanBox16 box)
    {
        fixed (pixman_f_transform* t = &Native)
        fixed (PixmanBox16* b = &box)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_f_transform_bounds(t, (pixman_box16*)b),
                "pixman_f_transform_bounds failed");
        }
    }
}
