using System.Runtime.InteropServices;

namespace Pixman.Tests;

internal static class TestHelpers
{
    /// <summary>Whether a system pixman library is present, probed with the same candidate
    /// names as the assembly's DllImportResolver.</summary>
    internal static bool PixmanAvailable { get; } = Probe();

    private static bool Probe()
    {
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
            if (NativeLibrary.TryLoad(candidate, out var handle))
            {
                NativeLibrary.Free(handle);
                return true;
            }
        }

        return false;
    }
}
