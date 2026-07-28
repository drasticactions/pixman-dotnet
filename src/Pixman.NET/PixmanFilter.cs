using Pixman.Native;

namespace Pixman;

/// <summary>A sampling filter.</summary>
public enum PixmanFilter : uint
{
    /// <summary><c>PIXMAN_FILTER_FAST</c>.</summary>
    Fast = (uint)pixman_filter_t.PIXMAN_FILTER_FAST,

    /// <summary><c>PIXMAN_FILTER_GOOD</c>.</summary>
    Good = (uint)pixman_filter_t.PIXMAN_FILTER_GOOD,

    /// <summary><c>PIXMAN_FILTER_BEST</c>.</summary>
    Best = (uint)pixman_filter_t.PIXMAN_FILTER_BEST,

    /// <summary><c>PIXMAN_FILTER_NEAREST</c>.</summary>
    Nearest = (uint)pixman_filter_t.PIXMAN_FILTER_NEAREST,

    /// <summary><c>PIXMAN_FILTER_BILINEAR</c>.</summary>
    Bilinear = (uint)pixman_filter_t.PIXMAN_FILTER_BILINEAR,

    /// <summary><c>PIXMAN_FILTER_CONVOLUTION</c>.</summary>
    Convolution = (uint)pixman_filter_t.PIXMAN_FILTER_CONVOLUTION,

    /// <summary><c>PIXMAN_FILTER_SEPARABLE_CONVOLUTION</c>.</summary>
    SeparableConvolution = (uint)pixman_filter_t.PIXMAN_FILTER_SEPARABLE_CONVOLUTION,
}
