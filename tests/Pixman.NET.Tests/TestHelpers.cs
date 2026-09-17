using System.Runtime.InteropServices;

namespace Pixman.Tests;

internal static class TestHelpers
{
    /// <summary>Whether a pixman library is present, probed with the same candidate
    /// names as the assembly's DllImportResolver.</summary>
    internal static bool PixmanAvailable { get; } = Probe();

    private static bool Probe()
    {
        // Statically linked platforms cannot be probed by name; the tests run inside
        // an app that already links pixman.
        if (OperatingSystem.IsIOS() || OperatingSystem.IsTvOS() || OperatingSystem.IsMacCatalyst() || OperatingSystem.IsBrowser())
        {
            return true;
        }

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
                : OperatingSystem.IsAndroid()
                    ? ["libpixman-1.so"]
                    : ["libpixman-1.so", "libpixman-1.so.0", "libpixman-1"];

        foreach (var candidate in candidates)
        {
            if (NativeLibrary.TryLoad(candidate, out var handle))
            {
                NativeLibrary.Free(handle);
                return true;
            }
        }

        return false;
    }
}
