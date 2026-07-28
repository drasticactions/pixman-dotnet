using Pixman.Native;

namespace Pixman;

/// <summary>The exception thrown when a pixman operation fails.</summary>
/// <remarks>
/// pixman has no errno or error-code convention; failures surface as <c>NULL</c> returns
/// (allocation failure) or a <c>FALSE</c> <c>pixman_bool_t</c> (invalid input, allocation
/// failure inside region code).
/// </remarks>
public sealed class PixmanException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="PixmanException"/> class.</summary>
    /// <param name="message">The message that describes the error.</param>
    public PixmanException(string message)
        : base(message)
    {
    }

    /// <summary>Throws when a pixman call returned a <c>NULL</c> pointer.</summary>
    /// <param name="pointer">The pointer returned by the native call.</param>
    /// <param name="message">The message describing the failed operation.</param>
    /// <exception cref="PixmanException">Thrown when <paramref name="pointer"/> is <c>NULL</c>.</exception>
    internal static unsafe void ThrowIfNull(void* pointer, string message)
    {
        if (pointer is null)
        {
            throw new PixmanException(message);
        }
    }

    /// <summary>Throws when a pixman call returned <c>FALSE</c>.</summary>
    /// <param name="result">The <c>pixman_bool_t</c> returned by the native call.</param>
    /// <param name="message">The message describing the failed operation.</param>
    /// <exception cref="PixmanException">Thrown when <paramref name="result"/> is zero.</exception>
    internal static void ThrowIfFalse(int result, string message)
    {
        if (result == 0)
        {
            throw new PixmanException(message);
        }
    }
}
