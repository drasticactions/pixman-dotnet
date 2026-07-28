using Pixman.Native;

namespace Pixman;

/// <summary>A 3×3 transformation matrix in 16.16 fixed-point.</summary>
public unsafe struct PixmanTransform
{
    /// <summary>The embedded native transform.</summary>
    internal pixman_transform Native;

    /// <summary>Gets or sets the matrix element at the given position.</summary>
    /// <param name="row">The row index, 0–2.</param>
    /// <param name="column">The column index, 0–2.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an index is outside 0–2.</exception>
    public PixmanFixed this[int row, int column]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)row, 2u, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)column, 2u, nameof(column));
            return PixmanFixed.FromRaw(Native.matrix[(row * 3) + column]);
        }

        set
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)row, 2u, nameof(row));
            ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)column, 2u, nameof(column));
            Native.matrix[(row * 3) + column] = value.Raw;
        }
    }

    /// <summary>Gets the identity transform.</summary>
    public static PixmanTransform Identity
    {
        get
        {
            PixmanTransform t = default;
            Libpixman.pixman_transform_init_identity(&t.Native);
            return t;
        }
    }

    /// <summary>Creates a scaling transform.</summary>
    /// <param name="sx">The horizontal scale factor.</param>
    /// <param name="sy">The vertical scale factor.</param>
    public static PixmanTransform CreateScale(PixmanFixed sx, PixmanFixed sy)
    {
        PixmanTransform t = default;
        Libpixman.pixman_transform_init_scale(&t.Native, sx.Raw, sy.Raw);
        return t;
    }

    /// <summary>Creates a rotation transform from the cosine and sine of the angle.</summary>
    /// <param name="cos">The cosine of the rotation angle.</param>
    /// <param name="sin">The sine of the rotation angle.</param>
    public static PixmanTransform CreateRotate(PixmanFixed cos, PixmanFixed sin)
    {
        PixmanTransform t = default;
        Libpixman.pixman_transform_init_rotate(&t.Native, cos.Raw, sin.Raw);
        return t;
    }

    /// <summary>Creates a translation transform.</summary>
    /// <param name="tx">The horizontal offset.</param>
    /// <param name="ty">The vertical offset.</param>
    public static PixmanTransform CreateTranslate(PixmanFixed tx, PixmanFixed ty)
    {
        PixmanTransform t = default;
        Libpixman.pixman_transform_init_translate(&t.Native, tx.Raw, ty.Raw);
        return t;
    }

    /// <summary>Multiplies two transforms, returning <c>l · r</c>.</summary>
    /// <param name="l">The left operand.</param>
    /// <param name="r">The right operand.</param>
    /// <exception cref="PixmanException">Thrown when the product overflows the fixed-point range.</exception>
    public static PixmanTransform Multiply(in PixmanTransform l, in PixmanTransform r)
    {
        var left = l;
        var right = r;
        PixmanTransform dst = default;
        PixmanException.ThrowIfFalse(
            Libpixman.pixman_transform_multiply(&dst.Native, &left.Native, &right.Native),
            "pixman_transform_multiply overflowed");
        return dst;
    }

    /// <summary>Converts a floating-point transform to fixed-point.</summary>
    /// <param name="ft">The floating-point transform to convert.</param>
    /// <exception cref="PixmanException">Thrown when a coefficient is outside the fixed-point range.</exception>
    public static PixmanTransform FromFTransform(in PixmanFTransform ft)
    {
        var source = ft;
        PixmanTransform t = default;
        PixmanException.ThrowIfFalse(
            Libpixman.pixman_transform_from_pixman_f_transform(&t.Native, &source.Native),
            "pixman_transform_from_pixman_f_transform overflowed");
        return t;
    }

    /// <summary>Multiplies an additional scale onto this transform.</summary>
    /// <param name="sx">The horizontal scale factor.</param>
    /// <param name="sy">The vertical scale factor.</param>
    /// <exception cref="PixmanException">Thrown when a factor is zero or the product overflows.</exception>
    public void Scale(PixmanFixed sx, PixmanFixed sy)
    {
        fixed (pixman_transform* t = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_transform_scale(t, null, sx.Raw, sy.Raw),
                "pixman_transform_scale failed");
        }
    }

    /// <summary>Multiplies an additional rotation onto this transform.</summary>
    /// <param name="cos">The cosine of the rotation angle.</param>
    /// <param name="sin">The sine of the rotation angle.</param>
    /// <exception cref="PixmanException">Thrown when the product overflows.</exception>
    public void Rotate(PixmanFixed cos, PixmanFixed sin)
    {
        fixed (pixman_transform* t = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_transform_rotate(t, null, cos.Raw, sin.Raw),
                "pixman_transform_rotate failed");
        }
    }

    /// <summary>Multiplies an additional translation onto this transform.</summary>
    /// <param name="tx">The horizontal offset.</param>
    /// <param name="ty">The vertical offset.</param>
    /// <exception cref="PixmanException">Thrown when the product overflows.</exception>
    public void Translate(PixmanFixed tx, PixmanFixed ty)
    {
        fixed (pixman_transform* t = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_transform_translate(t, null, tx.Raw, ty.Raw),
                "pixman_transform_translate failed");
        }
    }

    /// <summary>Computes the inverse of this transform.</summary>
    /// <exception cref="PixmanException">Thrown when the matrix is singular.</exception>
    public PixmanTransform Invert()
    {
        PixmanTransform dst = default;
        fixed (pixman_transform* src = &Native)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_transform_invert(&dst.Native, src),
                "pixman_transform_invert failed: matrix is singular");
        }

        return dst;
    }

    /// <summary>Transforms a homogeneous vector and divides through by the resulting w.</summary>
    /// <param name="vector">The vector to transform in place.</param>
    /// <exception cref="PixmanException">Thrown when the computation overflows.</exception>
    public void TransformPoint(ref PixmanVector vector)
    {
        fixed (pixman_transform* t = &Native)
        fixed (PixmanVector* v = &vector)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_transform_point(t, (pixman_vector*)v),
                "pixman_transform_point overflowed");
        }
    }

    /// <summary>Transforms a homogeneous vector without the perspective divide.</summary>
    /// <param name="vector">The vector to transform in place.</param>
    /// <exception cref="PixmanException">Thrown when the computation overflows.</exception>
    public void TransformPoint3D(ref PixmanVector vector)
    {
        fixed (pixman_transform* t = &Native)
        fixed (PixmanVector* v = &vector)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_transform_point_3d(t, (pixman_vector*)v),
                "pixman_transform_point_3d overflowed");
        }
    }

    /// <summary>Replaces a box with the bounding box of its transformed corners.</summary>
    /// <param name="box">The box to transform in place.</param>
    /// <exception cref="PixmanException">Thrown when the computation overflows.</exception>
    public void Bounds(ref PixmanBox16 box)
    {
        fixed (pixman_transform* t = &Native)
        fixed (PixmanBox16* b = &box)
        {
            PixmanException.ThrowIfFalse(
                Libpixman.pixman_transform_bounds(t, (pixman_box16*)b),
                "pixman_transform_bounds overflowed");
        }
    }

    /// <summary>Gets a value indicating whether this transform is the identity.</summary>
    public bool IsIdentity
    {
        get
        {
            fixed (pixman_transform* t = &Native)
            {
                return Libpixman.pixman_transform_is_identity(t) != 0;
            }
        }
    }

    /// <summary>Gets a value indicating whether this transform is a pure scale.</summary>
    public bool IsScale
    {
        get
        {
            fixed (pixman_transform* t = &Native)
            {
                return Libpixman.pixman_transform_is_scale(t) != 0;
            }
        }
    }

    /// <summary>Gets a value indicating whether this transform is an integer translation.</summary>
    public bool IsIntTranslate
    {
        get
        {
            fixed (pixman_transform* t = &Native)
            {
                return Libpixman.pixman_transform_is_int_translate(t) != 0;
            }
        }
    }

    /// <summary>Checks whether this transform and <paramref name="other"/> are inverses of each other.</summary>
    /// <param name="other">The transform to compare against.</param>
    /// <returns><see langword="true"/> when the two transforms are inverses.</returns>
    public bool IsInverseOf(in PixmanTransform other)
    {
        var b = other;
        fixed (pixman_transform* a = &Native)
        {
            return Libpixman.pixman_transform_is_inverse(a, &b.Native) != 0;
        }
    }

    /// <summary>Converts this transform to its floating-point equivalent.</summary>
    public PixmanFTransform ToFTransform()
    {
        PixmanFTransform ft = default;
        fixed (pixman_transform* t = &Native)
        {
            Libpixman.pixman_f_transform_from_pixman_transform(&ft.Native, t);
        }

        return ft;
    }
}
