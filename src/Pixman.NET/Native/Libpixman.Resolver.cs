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
        => NativeLibrary.SetDllImportResolver(typeof(Libpixman).Assembly, Resolve);
#pragma warning restore CA2255

    private static IntPtr Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        if (libraryName != LibraryName)
        {
            return IntPtr.Zero;
        }

        // The DllImport name "pixman-1" only resolves as-is on Windows
        // (pixman-1.dll); elsewhere the versioned soname/dylib must be probed
        // explicitly. msys2/mingw names its binary libpixman-1-0.dll. The
        // absolute macOS paths cover Homebrew (arm64 and x64) and MacPorts,
        // none of which are on dlopen's default search path.
        ReadOnlySpan<string> candidates = OperatingSystem.IsWindows()
            ? ["pixman-1.dll", "libpixman-1-0.dll", "libpixman-1.dll"]
            : OperatingSystem.IsMacOS()
                ?
                [
                    "libpixman-1.0.dylib", "libpixman-1.dylib",
                    "/opt/homebrew/lib/libpixman-1.0.dylib",
                    "/usr/local/lib/libpixman-1.0.dylib",
                    "/opt/local/lib/libpixman-1.0.dylib",
                ]
                : ["libpixman-1.so.0", "libpixman-1.so", "libpixman-1"];

        foreach (var candidate in candidates)
        {
            if (NativeLibrary.TryLoad(candidate, assembly, searchPath, out var handle))
            {
                return handle;
            }
        }

        return IntPtr.Zero;
    }
}
