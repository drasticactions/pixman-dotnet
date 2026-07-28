namespace Pixman.Native;

/// <summary>Hand-written members of the <see cref="Libpixman"/> interop class.</summary>
public static unsafe partial class Libpixman
{
    /// <summary>The library name used by the generated <c>DllImport</c> attributes.</summary>
    public const string LibraryName = "pixman-1";

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
