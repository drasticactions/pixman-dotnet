using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Pixman.Native;
using Xunit;

namespace Pixman.Tests;

public class NativeBindingTests
{
    [Fact]
    public void GeneratedFormatConstants_HaveExpectedValues()
    {
        // Hand-derived from PIXMAN_FORMAT(bpp, type, a, r, g, b) =
        // (bpp << 24) | (type << 16) | (a << 12) | (r << 8) | (g << 4) | b.
        Assert.Equal(0x20028888u, (uint)pixman_format_code_t.PIXMAN_a8r8g8b8);
        Assert.Equal(0x20020888u, (uint)pixman_format_code_t.PIXMAN_x8r8g8b8);
        Assert.Equal(0x10020565u, (uint)pixman_format_code_t.PIXMAN_r5g6b5);
        Assert.Equal(0x08018000u, (uint)pixman_format_code_t.PIXMAN_a8);
        Assert.Equal(0x01011000u, (uint)pixman_format_code_t.PIXMAN_a1);
    }

    [Fact]
    public void GeneratedFixedPointConstants_HaveExpectedValues()
    {
        Assert.Equal(65536, Libpixman.pixman_fixed_1);
        Assert.Equal(1, Libpixman.pixman_fixed_e);
        Assert.Equal(65535, Libpixman.pixman_fixed_1_minus_e);
        Assert.Equal(2, Libpixman.PIXMAN_TYPE_ARGB);
    }

    [Fact]
    public void HandWrittenVersionConstants_AreSelfConsistent()
    {
        // Guards the hand-evaluated constants in Libpixman.Manual.cs, which cannot be
        // generated because pixman-version.h is produced by meson.
        Assert.Equal(
            Libpixman.PIXMAN_VERSION_MAJOR * 10000 + Libpixman.PIXMAN_VERSION_MINOR * 100 + Libpixman.PIXMAN_VERSION_MICRO,
            Libpixman.PIXMAN_VERSION);
        Assert.Equal(
            Libpixman.PIXMAN_VERSION_STRING,
            $"{Libpixman.PIXMAN_VERSION_MAJOR}.{Libpixman.PIXMAN_VERSION_MINOR}.{Libpixman.PIXMAN_VERSION_MICRO}");
    }

    [Fact]
    public unsafe void WrapperStructs_MatchNativeLayout()
    {
        Assert.Equal(sizeof(pixman_color), Unsafe.SizeOf<PixmanColor>());
        Assert.Equal(sizeof(pixman_point_fixed), Unsafe.SizeOf<PixmanPointFixed>());
        Assert.Equal(sizeof(pixman_line_fixed), Unsafe.SizeOf<PixmanLineFixed>());
        Assert.Equal(sizeof(pixman_box16), Unsafe.SizeOf<PixmanBox16>());
        Assert.Equal(sizeof(pixman_box32), Unsafe.SizeOf<PixmanBox32>());
        Assert.Equal(sizeof(pixman_rectangle16), Unsafe.SizeOf<PixmanRectangle16>());
        Assert.Equal(sizeof(pixman_rectangle32), Unsafe.SizeOf<PixmanRectangle32>());
        Assert.Equal(sizeof(pixman_trapezoid), Unsafe.SizeOf<PixmanTrapezoid>());
        Assert.Equal(sizeof(pixman_triangle), Unsafe.SizeOf<PixmanTriangle>());
        Assert.Equal(sizeof(pixman_trap), Unsafe.SizeOf<PixmanTrap>());
        Assert.Equal(sizeof(pixman_span_fix), Unsafe.SizeOf<PixmanSpanFix>());
        Assert.Equal(sizeof(pixman_gradient_stop), Unsafe.SizeOf<PixmanGradientStop>());
        Assert.Equal(sizeof(pixman_glyph_t), Unsafe.SizeOf<PixmanGlyph>());
        Assert.Equal(sizeof(pixman_vector), Unsafe.SizeOf<PixmanVector>());
        Assert.Equal(sizeof(pixman_f_vector), Unsafe.SizeOf<PixmanFVector>());
        Assert.Equal(sizeof(pixman_transform), Unsafe.SizeOf<PixmanTransform>());
        Assert.Equal(sizeof(pixman_f_transform), Unsafe.SizeOf<PixmanFTransform>());
        Assert.Equal(sizeof(pixman_edge), Unsafe.SizeOf<PixmanEdge>());
        Assert.Equal(4, Unsafe.SizeOf<PixmanFixed>());
    }

    [Fact]
    public void WrapperEnums_MatchNativeValues()
    {
        Assert.Equal((uint)pixman_op_t.PIXMAN_OP_OVER, (uint)PixmanOp.Over);
        Assert.Equal((uint)pixman_op_t.PIXMAN_OP_HSL_LUMINOSITY, (uint)PixmanOp.HslLuminosity);
        Assert.Equal((uint)pixman_format_code_t.PIXMAN_a8r8g8b8, (uint)PixmanFormat.A8R8G8B8);
        Assert.Equal((uint)pixman_format_code_t.PIXMAN_rgba_float, (uint)PixmanFormat.RgbaFloat);
        Assert.Equal((uint)pixman_repeat_t.PIXMAN_REPEAT_REFLECT, (uint)PixmanRepeat.Reflect);
        Assert.Equal((uint)pixman_filter_t.PIXMAN_FILTER_BILINEAR, (uint)PixmanFilter.Bilinear);
        Assert.Equal((uint)pixman_region_overlap_t.PIXMAN_REGION_PART, (uint)PixmanRegionOverlap.Part);
        Assert.Equal((uint)pixman_dither_t.PIXMAN_DITHER_BEST, (uint)PixmanDither.Best);
        Assert.Equal((uint)pixman_kernel_t.PIXMAN_KERNEL_LANCZOS3, (uint)PixmanKernel.Lanczos3);
    }

    [Fact]
    public unsafe void Resolver_LoadsSystemPixman()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        Assert.True(Libpixman.pixman_version() > 0);
        Assert.NotNull(Marshal.PtrToStringUTF8((IntPtr)Libpixman.pixman_version_string()));
    }

    [Fact]
    public void PixmanLibrary_VersionEncodesVersionString()
    {
        Assert.SkipUnless(TestHelpers.PixmanAvailable, "pixman is not installed");

        var parts = PixmanLibrary.VersionString.Split('.');
        Assert.Equal(3, parts.Length);
        var encoded = int.Parse(parts[0]) * 10000 + int.Parse(parts[1]) * 100 + int.Parse(parts[2]);
        Assert.Equal(encoded, PixmanLibrary.Version);
    }
}
