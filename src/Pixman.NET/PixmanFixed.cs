using System.Globalization;

namespace Pixman;

/// <summary>A 16.16 fixed-point number, the <c>pixman_fixed_t</c> type used throughout pixman.</summary>
public readonly struct PixmanFixed : IEquatable<PixmanFixed>, IComparable<PixmanFixed>
{
    private readonly int _raw;

    private PixmanFixed(int raw) => _raw = raw;

    /// <summary>The raw 16.16 fixed-point bits.</summary>
    public int Raw => _raw;

    /// <summary>The smallest positive fixed-point value (<c>pixman_fixed_e</c>).</summary>
    public static PixmanFixed E => new(1);

    /// <summary>The fixed-point value 1 (<c>pixman_fixed_1</c>).</summary>
    public static PixmanFixed One => new(1 << 16);

    /// <summary>The fixed-point value 1 - e (<c>pixman_fixed_1_minus_e</c>).</summary>
    public static PixmanFixed OneMinusE => new((1 << 16) - 1);

    /// <summary>The fixed-point value -1 (<c>pixman_fixed_minus_1</c>).</summary>
    public static PixmanFixed MinusOne => new(-1 << 16);

    /// <summary>The fixed-point value 0.</summary>
    public static PixmanFixed Zero => new(0);

    /// <summary>The fractional part (<c>pixman_fixed_frac</c>).</summary>
    public PixmanFixed Frac => new(_raw & 0xffff);

    /// <summary>The value rounded down to an integer (<c>pixman_fixed_floor</c>).</summary>
    public PixmanFixed Floor => new(_raw & ~0xffff);

    /// <summary>The value rounded up to an integer (<c>pixman_fixed_ceil</c>).</summary>
    public PixmanFixed Ceil => new((_raw + 0xffff) & ~0xffff);

    /// <summary>Wraps raw 16.16 fixed-point bits.</summary>
    /// <param name="raw">The raw fixed-point bits.</param>
    public static PixmanFixed FromRaw(int raw) => new(raw);

    /// <summary>Converts an integer to fixed point (<c>pixman_int_to_fixed</c>).</summary>
    /// <param name="value">The integer value.</param>
    public static PixmanFixed FromInt(int value) => new((int)((uint)value << 16));

    /// <summary>Converts a double to fixed point (<c>pixman_double_to_fixed</c>).</summary>
    /// <param name="value">The floating-point value.</param>
    public static PixmanFixed FromDouble(double value) => new((int)(value * 65536.0));

    /// <summary>Converts to an integer, truncating the fraction (<c>pixman_fixed_to_int</c>).</summary>
    public int ToInt() => _raw >> 16;

    /// <summary>Converts to a double (<c>pixman_fixed_to_double</c>).</summary>
    public double ToDouble() => _raw / 65536.0;

    /// <summary>Converts an integer to fixed point.</summary>
    /// <param name="value">The integer value.</param>
    public static implicit operator PixmanFixed(int value) => FromInt(value);

    /// <summary>Converts a double to fixed point.</summary>
    /// <param name="value">The floating-point value.</param>
    public static implicit operator PixmanFixed(double value) => FromDouble(value);

    /// <summary>Converts a fixed-point value to a double.</summary>
    /// <param name="value">The fixed-point value.</param>
    public static implicit operator double(PixmanFixed value) => value.ToDouble();

    /// <summary>Adds two fixed-point values.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static PixmanFixed operator +(PixmanFixed left, PixmanFixed right) => new(left._raw + right._raw);

    /// <summary>Subtracts one fixed-point value from another.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static PixmanFixed operator -(PixmanFixed left, PixmanFixed right) => new(left._raw - right._raw);

    /// <summary>Negates a fixed-point value.</summary>
    /// <param name="value">The operand.</param>
    public static PixmanFixed operator -(PixmanFixed value) => new(-value._raw);

    /// <summary>Tests two fixed-point values for equality.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static bool operator ==(PixmanFixed left, PixmanFixed right) => left._raw == right._raw;

    /// <summary>Tests two fixed-point values for inequality.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static bool operator !=(PixmanFixed left, PixmanFixed right) => left._raw != right._raw;

    /// <summary>Tests whether one fixed-point value is less than another.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static bool operator <(PixmanFixed left, PixmanFixed right) => left._raw < right._raw;

    /// <summary>Tests whether one fixed-point value is less than or equal to another.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static bool operator <=(PixmanFixed left, PixmanFixed right) => left._raw <= right._raw;

    /// <summary>Tests whether one fixed-point value is greater than another.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static bool operator >(PixmanFixed left, PixmanFixed right) => left._raw > right._raw;

    /// <summary>Tests whether one fixed-point value is greater than or equal to another.</summary>
    /// <param name="left">The first operand.</param>
    /// <param name="right">The second operand.</param>
    public static bool operator >=(PixmanFixed left, PixmanFixed right) => left._raw >= right._raw;

    /// <inheritdoc/>
    public bool Equals(PixmanFixed other) => _raw == other._raw;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is PixmanFixed other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => _raw;

    /// <inheritdoc/>
    public int CompareTo(PixmanFixed other) => _raw.CompareTo(other._raw);

    /// <inheritdoc/>
    public override string ToString() => ToDouble().ToString(CultureInfo.InvariantCulture);
}
