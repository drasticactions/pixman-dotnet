using System.Runtime.InteropServices;
using Pixman.Native;

namespace Pixman;

/// <summary>Library-level pixman entry points that are not tied to an image or region.</summary>
public static unsafe class PixmanLibrary
{
    /// <summary>Gets the version of the loaded pixman library, encoded as <c>major * 10000 + minor * 100 + micro</c>.</summary>
    public static int Version => Libpixman.pixman_version();

    /// <summary>Gets the version string of the loaded pixman library.</summary>
    public static string VersionString => Marshal.PtrToStringUTF8((IntPtr)Libpixman.pixman_version_string()) ?? string.Empty;

    /// <summary>Gets the encoded pixman version the bindings were generated from.</summary>
    public static int BindingsVersion => Libpixman.PIXMAN_VERSION;

    /// <summary>Gets the version string of the pixman release the bindings were generated from.</summary>
    public static string BindingsVersionString => Libpixman.PIXMAN_VERSION_STRING;

    /// <summary>Fills a rectangle of raw pixel memory with a value.</summary>
    /// <param name="bits">The pixel buffer.</param>
    /// <param name="stride">The buffer stride in <c>uint32</c> units, matching the C API.</param>
    /// <param name="bpp">The bits per pixel of the buffer.</param>
    /// <param name="x">The left edge of the rectangle in pixels.</param>
    /// <param name="y">The top edge of the rectangle in pixels.</param>
    /// <param name="width">The rectangle width in pixels.</param>
    /// <param name="height">The rectangle height in pixels.</param>
    /// <param name="filler">The value written to every pixel.</param>
    /// <returns><see langword="false"/> when the buffer's <paramref name="bpp"/> is not supported and nothing was filled.</returns>
    public static bool Fill(IntPtr bits, int stride, int bpp, int x, int y, int width, int height, uint filler)
        => Libpixman.pixman_fill((uint*)bits, stride, bpp, x, y, width, height, filler) != 0;

    /// <summary>Copies a rectangle of raw pixel memory between two buffers of equal depth.</summary>
    /// <param name="srcBits">The source pixel buffer.</param>
    /// <param name="dstBits">The destination pixel buffer.</param>
    /// <param name="srcStride">The source stride in <c>uint32</c> units, matching the C API.</param>
    /// <param name="dstStride">The destination stride in <c>uint32</c> units, matching the C API.</param>
    /// <param name="srcBpp">The source bits per pixel.</param>
    /// <param name="dstBpp">The destination bits per pixel; must equal <paramref name="srcBpp"/>.</param>
    /// <param name="srcX">The left edge of the source rectangle in pixels.</param>
    /// <param name="srcY">The top edge of the source rectangle in pixels.</param>
    /// <param name="destX">The left edge of the destination rectangle in pixels.</param>
    /// <param name="destY">The top edge of the destination rectangle in pixels.</param>
    /// <param name="width">The rectangle width in pixels.</param>
    /// <param name="height">The rectangle height in pixels.</param>
    /// <returns><see langword="false"/> when the depths are unsupported and nothing was copied.</returns>
    public static bool Blt(IntPtr srcBits, IntPtr dstBits, int srcStride, int dstStride, int srcBpp, int dstBpp, int srcX, int srcY, int destX, int destY, int width, int height)
        => Libpixman.pixman_blt((uint*)srcBits, (uint*)dstBits, srcStride, dstStride, srcBpp, dstBpp, srcX, srcY, destX, destY, width, height) != 0;

    /// <summary>Disables the workaround for X servers older than 1.7 that passed out-of-bounds drawables.</summary>
    public static void DisableOutOfBoundsWorkaround() => Libpixman.pixman_disable_out_of_bounds_workaround();

    /// <summary>Rounds y up to the next polygon sample row for the given alpha depth.</summary>
    /// <param name="y">The y coordinate to round.</param>
    /// <param name="bpp">The bits per pixel of the target alpha image (1, 4 or 8).</param>
    public static PixmanFixed SampleCeilY(PixmanFixed y, int bpp)
        => PixmanFixed.FromRaw(Libpixman.pixman_sample_ceil_y(y.Raw, bpp));

    /// <summary>Rounds y down to the previous polygon sample row for the given alpha depth.</summary>
    /// <param name="y">The y coordinate to round.</param>
    /// <param name="bpp">The bits per pixel of the target alpha image (1, 4 or 8).</param>
    public static PixmanFixed SampleFloorY(PixmanFixed y, int bpp)
        => PixmanFixed.FromRaw(Libpixman.pixman_sample_floor_y(y.Raw, bpp));
}
