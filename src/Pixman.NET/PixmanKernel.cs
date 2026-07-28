using Pixman.Native;

namespace Pixman;

/// <summary>A resampling kernel for separable convolution filters.</summary>
public enum PixmanKernel : uint
{
    /// <summary><c>PIXMAN_KERNEL_IMPULSE</c>.</summary>
    Impulse = (uint)pixman_kernel_t.PIXMAN_KERNEL_IMPULSE,

    /// <summary><c>PIXMAN_KERNEL_BOX</c>.</summary>
    Box = (uint)pixman_kernel_t.PIXMAN_KERNEL_BOX,

    /// <summary><c>PIXMAN_KERNEL_LINEAR</c>.</summary>
    Linear = (uint)pixman_kernel_t.PIXMAN_KERNEL_LINEAR,

    /// <summary><c>PIXMAN_KERNEL_CUBIC</c>.</summary>
    Cubic = (uint)pixman_kernel_t.PIXMAN_KERNEL_CUBIC,

    /// <summary><c>PIXMAN_KERNEL_GAUSSIAN</c>.</summary>
    Gaussian = (uint)pixman_kernel_t.PIXMAN_KERNEL_GAUSSIAN,

    /// <summary><c>PIXMAN_KERNEL_LANCZOS2</c>.</summary>
    Lanczos2 = (uint)pixman_kernel_t.PIXMAN_KERNEL_LANCZOS2,

    /// <summary><c>PIXMAN_KERNEL_LANCZOS3</c>.</summary>
    Lanczos3 = (uint)pixman_kernel_t.PIXMAN_KERNEL_LANCZOS3,

    /// <summary><c>PIXMAN_KERNEL_LANCZOS3_STRETCHED</c>.</summary>
    Lanczos3Stretched = (uint)pixman_kernel_t.PIXMAN_KERNEL_LANCZOS3_STRETCHED,
}
