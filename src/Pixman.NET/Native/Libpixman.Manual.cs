using System.Runtime.InteropServices;

namespace Pixman.Native;

#if !(IOS || TVOS || MACCATALYST)
/// <summary>
/// Arguments of <see cref="Libpixman.pixman_dotnet_composite_glyphs"/>, the browser-wasm shim
/// around <see cref="Libpixman.pixman_composite_glyphs"/>. Layout matches
/// <c>eng/wasm/pixman-dotnet-shim.c</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct pixman_dotnet_composite_glyphs_args
{
#pragma warning disable CS1591 // mirrors the pixman_composite_glyphs parameters
    public pixman_op_t op;
    public pixman_image* src;
    public pixman_image* dest;
    public pixman_format_code_t mask_format;
    public int src_x;
    public int src_y;
    public int mask_x;
    public int mask_y;
    public int dest_x;
    public int dest_y;
    public int width;
    public int height;
    public pixman_glyph_cache_t* cache;
    public int n_glyphs;
    public pixman_glyph_t* glyphs;
#pragma warning restore CS1591
}

#endif

/// <summary>Hand-written members of the <see cref="Libpixman"/> interop class.</summary>
public static unsafe partial class Libpixman
{
#if !(IOS || TVOS || MACCATALYST)
    /// <summary>
    /// Browser-wasm only: calls <see cref="pixman_composite_glyphs"/> through a one-argument shim
    /// compiled into the package's <c>pixman-1.a</c>. The Mono interpreter that runs browser apps
    /// cannot invoke a P/Invoke with more than 12 integer arguments, and
    /// <see cref="pixman_composite_glyphs"/> has 15.
    /// </summary>
    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    public static extern void pixman_dotnet_composite_glyphs(pixman_dotnet_composite_glyphs_args* args);
#endif

    /// <summary>The library name used by the generated <c>DllImport</c> attributes.</summary>
    /// <remarks>
    /// <c>"pixman-1"</c> everywhere pixman is a shared library (desktop, Android) or resolved
    /// from the pinvoke table (browser-wasm). The iOS, tvOS and Mac Catalyst builds of this
    /// assembly link pixman statically into the app, so they use <c>"__Internal"</c> instead.
    /// </remarks>
#if IOS || TVOS || MACCATALYST
    public const string LibraryName = "__Internal";
#else
    public const string LibraryName = "pixman-1";
#endif

    /// <summary>Major component of the pixman version the bindings were generated from.</summary>
    /// <remarks>
    /// <c>PIXMAN_VERSION_MAJOR</c> lives in the meson-generated <c>pixman-version.h</c>, which is
    /// outside the generator's traversal; these constants are hand-evaluated and guarded by tests
    /// against <see cref="pixman_version"/>. Keep in sync with the <c>external/pixman</c> submodule.
    /// </remarks>
    public const int PIXMAN_VERSION_MAJOR = 0;

    /// <summary>Minor component of the pixman version the bindings were generated from.</summary>
    public const int PIXMAN_VERSION_MINOR = 46;

    /// <summary>Micro component of the pixman version the bindings were generated from.</summary>
    public const int PIXMAN_VERSION_MICRO = 4;

    /// <summary>Encoded pixman version, <c>major * 10000 + minor * 100 + micro</c>.</summary>
    public const int PIXMAN_VERSION = PIXMAN_VERSION_MAJOR * 10000 + PIXMAN_VERSION_MINOR * 100 + PIXMAN_VERSION_MICRO;

    /// <summary>String form of the pixman version the bindings were generated from.</summary>
    public const string PIXMAN_VERSION_STRING = "0.46.4";
}
