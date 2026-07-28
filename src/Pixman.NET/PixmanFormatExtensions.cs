using Pixman.Native;

namespace Pixman;

/// <summary>Introspection helpers for <see cref="PixmanFormat"/>, translating the <c>PIXMAN_FORMAT_*</c> macros.</summary>
public static class PixmanFormatExtensions
{
    // PIXMAN_FORMAT_RESHIFT(val, ofs, num): the nibble is scaled by the shift
    // stored in bits [23:22], used by the wide (>= 64bpp) byte-coded formats.
    private static int Reshift(uint val, int ofs, int num)
        => (int)(((val >> ofs) & ((1u << num) - 1)) << (int)((val >> 22) & 3));

    /// <summary>Bits per pixel (<c>PIXMAN_FORMAT_BPP</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int Bpp(this PixmanFormat format) => Reshift((uint)format, 24, 8);

    /// <summary>Significant bits per pixel, the sum of the channel widths (<c>PIXMAN_FORMAT_DEPTH</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int Depth(this PixmanFormat format)
        => format.AlphaBits() + format.RedBits() + format.GreenBits() + format.BlueBits();

    /// <summary>The format type, one of the <c>PIXMAN_TYPE_*</c> constants (<c>PIXMAN_FORMAT_TYPE</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int Type(this PixmanFormat format) => (int)(((uint)format >> 16) & 0x3f);

    /// <summary>The width of the alpha channel in bits (<c>PIXMAN_FORMAT_A</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int AlphaBits(this PixmanFormat format) => Reshift((uint)format, 12, 4);

    /// <summary>The width of the red channel in bits (<c>PIXMAN_FORMAT_R</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int RedBits(this PixmanFormat format) => Reshift((uint)format, 8, 4);

    /// <summary>The width of the green channel in bits (<c>PIXMAN_FORMAT_G</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int GreenBits(this PixmanFormat format) => Reshift((uint)format, 4, 4);

    /// <summary>The width of the blue channel in bits (<c>PIXMAN_FORMAT_B</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int BlueBits(this PixmanFormat format) => Reshift((uint)format, 0, 4);

    /// <summary>Whether the format has color channels (<c>PIXMAN_FORMAT_RGB</c> is nonzero).</summary>
    /// <param name="format">The format to inspect.</param>
    public static bool HasRgb(this PixmanFormat format) => ((uint)format & 0xfff) != 0;

    /// <summary>The channel-width bits of the format code (<c>PIXMAN_FORMAT_VIS</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static int Vis(this PixmanFormat format) => (int)((uint)format & 0xffff);

    /// <summary>Whether the format is a true-color type (<c>PIXMAN_FORMAT_COLOR</c>: ARGB, ABGR, BGRA, RGBA or RGBA-float).</summary>
    /// <param name="format">The format to inspect.</param>
    public static bool IsColor(this PixmanFormat format)
        => format.Type() is Libpixman.PIXMAN_TYPE_ARGB
            or Libpixman.PIXMAN_TYPE_ABGR
            or Libpixman.PIXMAN_TYPE_BGRA
            or Libpixman.PIXMAN_TYPE_RGBA
            or Libpixman.PIXMAN_TYPE_RGBA_FLOAT;

    /// <summary>Whether pixman supports the format as a composite source (<c>pixman_format_supported_source</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static bool IsSupportedSource(this PixmanFormat format)
        => Libpixman.pixman_format_supported_source((pixman_format_code_t)format) != 0;

    /// <summary>Whether pixman supports the format as a composite destination (<c>pixman_format_supported_destination</c>).</summary>
    /// <param name="format">The format to inspect.</param>
    public static bool IsSupportedDestination(this PixmanFormat format)
        => Libpixman.pixman_format_supported_destination((pixman_format_code_t)format) != 0;
}
