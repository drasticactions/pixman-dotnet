using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Pixman.Native;

public static unsafe partial class Libpixman
{
    // A module initializer is normally discouraged in libraries (CA2255), but a
    // DllImportResolver must be registered before the first generated DllImport
    // fires, and there is no hook a consumer could call earlier.
#pragma warning disable CA2255
    /// <summary>Registers the <see cref="DllImportResolver"/> that maps <see cref="LibraryName"/> to the platform's pixman binary.</summary>
    [ModuleInitializer]
    internal static void Initialize()
    {
        // Statically linked platforms: "__Internal" on the Apple mobile TFMs, and the
        // pinvoke table on browser-wasm. The runtime resolves both without help.
        if (LibraryName == "__Internal" || OperatingSystem.IsBrowser())
        {
            return;
        }

        NativeLibrary.SetDllImportResolver(typeof(Libpixman).Assembly, Resolve);
    }
#pragma warning restore CA2255

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
        {
            return IntPtr.Zero;
        }

        foreach (var candidate in Candidates)
        {
            if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out var handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// Library names probed in order. The packaged binary is listed first so it wins over a
    /// system copy; the DllImport name "pixman-1" only resolves as-is on Windows (pixman-1.dll),
    /// elsewhere the versioned soname/dylib must be probed explicitly.
    /// </summary>
    /// <remarks>
    /// msys2/mingw names its binary libpixman-1-0.dll. The absolute macOS paths cover Homebrew
    /// (arm64 and x64) and MacPorts, none of which are on dlopen's default search path.
    /// The package ships an unversioned libpixman-1.so for Linux and Android; libpixman-1.so.0
    /// is the distro's soname. <c>TestHelpers.Probe</c> in the tests duplicates this list.
    /// </remarks>
    internal static string[] Candidates { get; } =
            OperatingSystem.IsWindows()
            ? ["pixman-1.dll", "libpixman-1-0.dll", "libpixman-1.dll"]
            : OperatingSystem.IsMacOS()
                ?
                [
                    "libpixman-1.0.dylib", "libpixman-1.dylib",
                    "/opt/homebrew/lib/libpixman-1.0.dylib",
                    "/usr/local/lib/libpixman-1.0.dylib",
                    "/opt/local/lib/libpixman-1.0.dylib",
                ]
                : OperatingSystem.IsAndroid()
                    ? ["libpixman-1.so"]
                    : ["libpixman-1.so", "libpixman-1.so.0", "libpixman-1"];
}
