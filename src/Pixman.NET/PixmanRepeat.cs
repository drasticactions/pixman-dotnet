using Pixman.Native;

namespace Pixman;

/// <summary>How a source image repeats outside its bounds.</summary>
public enum PixmanRepeat : uint
{
    /// <summary><c>PIXMAN_REPEAT_NONE</c>: pixels outside the image are transparent.</summary>
    None = (uint)pixman_repeat_t.PIXMAN_REPEAT_NONE,

    /// <summary><c>PIXMAN_REPEAT_NORMAL</c>: the image tiles.</summary>
    Normal = (uint)pixman_repeat_t.PIXMAN_REPEAT_NORMAL,

    /// <summary><c>PIXMAN_REPEAT_PAD</c>: edge pixels extend outward.</summary>
    Pad = (uint)pixman_repeat_t.PIXMAN_REPEAT_PAD,

    /// <summary><c>PIXMAN_REPEAT_REFLECT</c>: the image tiles, mirrored on alternate tiles.</summary>
    Reflect = (uint)pixman_repeat_t.PIXMAN_REPEAT_REFLECT,
}
