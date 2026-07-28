using Pixman.Native;

namespace Pixman;

/// <summary>A dithering method.</summary>
public enum PixmanDither : uint
{
    /// <summary><c>PIXMAN_DITHER_NONE</c>.</summary>
    None = (uint)pixman_dither_t.PIXMAN_DITHER_NONE,

    /// <summary><c>PIXMAN_DITHER_FAST</c>.</summary>
    Fast = (uint)pixman_dither_t.PIXMAN_DITHER_FAST,

    /// <summary><c>PIXMAN_DITHER_GOOD</c>.</summary>
    Good = (uint)pixman_dither_t.PIXMAN_DITHER_GOOD,

    /// <summary><c>PIXMAN_DITHER_BEST</c>.</summary>
    Best = (uint)pixman_dither_t.PIXMAN_DITHER_BEST,

    /// <summary><c>PIXMAN_DITHER_ORDERED_BAYER_8</c>.</summary>
    OrderedBayer8 = (uint)pixman_dither_t.PIXMAN_DITHER_ORDERED_BAYER_8,

    /// <summary><c>PIXMAN_DITHER_ORDERED_BLUE_NOISE_64</c>.</summary>
    OrderedBlueNoise64 = (uint)pixman_dither_t.PIXMAN_DITHER_ORDERED_BLUE_NOISE_64,
}
