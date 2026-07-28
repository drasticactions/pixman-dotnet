using Pixman.Native;

namespace Pixman;

/// <summary>Pixel formats.</summary>
public enum PixmanFormat : uint
{
    /// <summary><c>PIXMAN_rgba_float</c>: 128bpp RGBA, 32-bit float per channel.</summary>
    RgbaFloat = (uint)pixman_format_code_t.PIXMAN_rgba_float,

    /// <summary><c>PIXMAN_rgb_float</c>: 96bpp RGB, 32-bit float per channel.</summary>
    RgbFloat = (uint)pixman_format_code_t.PIXMAN_rgb_float,

    /// <summary><c>PIXMAN_a16b16g16r16</c>.</summary>
    A16B16G16R16 = (uint)pixman_format_code_t.PIXMAN_a16b16g16r16,

    /// <summary><c>PIXMAN_a8r8g8b8</c>.</summary>
    A8R8G8B8 = (uint)pixman_format_code_t.PIXMAN_a8r8g8b8,

    /// <summary><c>PIXMAN_x8r8g8b8</c>.</summary>
    X8R8G8B8 = (uint)pixman_format_code_t.PIXMAN_x8r8g8b8,

    /// <summary><c>PIXMAN_a8b8g8r8</c>.</summary>
    A8B8G8R8 = (uint)pixman_format_code_t.PIXMAN_a8b8g8r8,

    /// <summary><c>PIXMAN_x8b8g8r8</c>.</summary>
    X8B8G8R8 = (uint)pixman_format_code_t.PIXMAN_x8b8g8r8,

    /// <summary><c>PIXMAN_b8g8r8a8</c>.</summary>
    B8G8R8A8 = (uint)pixman_format_code_t.PIXMAN_b8g8r8a8,

    /// <summary><c>PIXMAN_b8g8r8x8</c>.</summary>
    B8G8R8X8 = (uint)pixman_format_code_t.PIXMAN_b8g8r8x8,

    /// <summary><c>PIXMAN_r8g8b8a8</c>.</summary>
    R8G8B8A8 = (uint)pixman_format_code_t.PIXMAN_r8g8b8a8,

    /// <summary><c>PIXMAN_r8g8b8x8</c>.</summary>
    R8G8B8X8 = (uint)pixman_format_code_t.PIXMAN_r8g8b8x8,

    /// <summary><c>PIXMAN_x14r6g6b6</c>.</summary>
    X14R6G6B6 = (uint)pixman_format_code_t.PIXMAN_x14r6g6b6,

    /// <summary><c>PIXMAN_x2r10g10b10</c>.</summary>
    X2R10G10B10 = (uint)pixman_format_code_t.PIXMAN_x2r10g10b10,

    /// <summary><c>PIXMAN_a2r10g10b10</c>.</summary>
    A2R10G10B10 = (uint)pixman_format_code_t.PIXMAN_a2r10g10b10,

    /// <summary><c>PIXMAN_x2b10g10r10</c>.</summary>
    X2B10G10R10 = (uint)pixman_format_code_t.PIXMAN_x2b10g10r10,

    /// <summary><c>PIXMAN_a2b10g10r10</c>.</summary>
    A2B10G10R10 = (uint)pixman_format_code_t.PIXMAN_a2b10g10r10,

    /// <summary><c>PIXMAN_a8r8g8b8_sRGB</c>.</summary>
    A8R8G8B8Srgb = (uint)pixman_format_code_t.PIXMAN_a8r8g8b8_sRGB,

    /// <summary><c>PIXMAN_r8g8b8_sRGB</c>.</summary>
    R8G8B8Srgb = (uint)pixman_format_code_t.PIXMAN_r8g8b8_sRGB,

    /// <summary><c>PIXMAN_r8g8b8</c>.</summary>
    R8G8B8 = (uint)pixman_format_code_t.PIXMAN_r8g8b8,

    /// <summary><c>PIXMAN_b8g8r8</c>.</summary>
    B8G8R8 = (uint)pixman_format_code_t.PIXMAN_b8g8r8,

    /// <summary><c>PIXMAN_r5g6b5</c>.</summary>
    R5G6B5 = (uint)pixman_format_code_t.PIXMAN_r5g6b5,

    /// <summary><c>PIXMAN_b5g6r5</c>.</summary>
    B5G6R5 = (uint)pixman_format_code_t.PIXMAN_b5g6r5,

    /// <summary><c>PIXMAN_a1r5g5b5</c>.</summary>
    A1R5G5B5 = (uint)pixman_format_code_t.PIXMAN_a1r5g5b5,

    /// <summary><c>PIXMAN_x1r5g5b5</c>.</summary>
    X1R5G5B5 = (uint)pixman_format_code_t.PIXMAN_x1r5g5b5,

    /// <summary><c>PIXMAN_a1b5g5r5</c>.</summary>
    A1B5G5R5 = (uint)pixman_format_code_t.PIXMAN_a1b5g5r5,

    /// <summary><c>PIXMAN_x1b5g5r5</c>.</summary>
    X1B5G5R5 = (uint)pixman_format_code_t.PIXMAN_x1b5g5r5,

    /// <summary><c>PIXMAN_a4r4g4b4</c>.</summary>
    A4R4G4B4 = (uint)pixman_format_code_t.PIXMAN_a4r4g4b4,

    /// <summary><c>PIXMAN_x4r4g4b4</c>.</summary>
    X4R4G4B4 = (uint)pixman_format_code_t.PIXMAN_x4r4g4b4,

    /// <summary><c>PIXMAN_a4b4g4r4</c>.</summary>
    A4B4G4R4 = (uint)pixman_format_code_t.PIXMAN_a4b4g4r4,

    /// <summary><c>PIXMAN_x4b4g4r4</c>.</summary>
    X4B4G4R4 = (uint)pixman_format_code_t.PIXMAN_x4b4g4r4,

    /// <summary><c>PIXMAN_a8</c>.</summary>
    A8 = (uint)pixman_format_code_t.PIXMAN_a8,

    /// <summary><c>PIXMAN_r3g3b2</c>.</summary>
    R3G3B2 = (uint)pixman_format_code_t.PIXMAN_r3g3b2,

    /// <summary><c>PIXMAN_b2g3r3</c>.</summary>
    B2G3R3 = (uint)pixman_format_code_t.PIXMAN_b2g3r3,

    /// <summary><c>PIXMAN_a2r2g2b2</c>.</summary>
    A2R2G2B2 = (uint)pixman_format_code_t.PIXMAN_a2r2g2b2,

    /// <summary><c>PIXMAN_a2b2g2r2</c>.</summary>
    A2B2G2R2 = (uint)pixman_format_code_t.PIXMAN_a2b2g2r2,

    /// <summary><c>PIXMAN_c8</c>: 8-bit color-indexed.</summary>
    C8 = (uint)pixman_format_code_t.PIXMAN_c8,

    /// <summary><c>PIXMAN_g8</c>: 8-bit grayscale.</summary>
    G8 = (uint)pixman_format_code_t.PIXMAN_g8,

    /// <summary><c>PIXMAN_x4a4</c>.</summary>
    X4A4 = (uint)pixman_format_code_t.PIXMAN_x4a4,

    /// <summary><c>PIXMAN_x4c4</c>.</summary>
    X4C4 = (uint)pixman_format_code_t.PIXMAN_x4c4,

    /// <summary><c>PIXMAN_x4g4</c>.</summary>
    X4G4 = (uint)pixman_format_code_t.PIXMAN_x4g4,

    /// <summary><c>PIXMAN_a4</c>.</summary>
    A4 = (uint)pixman_format_code_t.PIXMAN_a4,

    /// <summary><c>PIXMAN_r1g2b1</c>.</summary>
    R1G2B1 = (uint)pixman_format_code_t.PIXMAN_r1g2b1,

    /// <summary><c>PIXMAN_b1g2r1</c>.</summary>
    B1G2R1 = (uint)pixman_format_code_t.PIXMAN_b1g2r1,

    /// <summary><c>PIXMAN_a1r1g1b1</c>.</summary>
    A1R1G1B1 = (uint)pixman_format_code_t.PIXMAN_a1r1g1b1,

    /// <summary><c>PIXMAN_a1b1g1r1</c>.</summary>
    A1B1G1R1 = (uint)pixman_format_code_t.PIXMAN_a1b1g1r1,

    /// <summary><c>PIXMAN_c4</c>: 4-bit color-indexed.</summary>
    C4 = (uint)pixman_format_code_t.PIXMAN_c4,

    /// <summary><c>PIXMAN_g4</c>: 4-bit grayscale.</summary>
    G4 = (uint)pixman_format_code_t.PIXMAN_g4,

    /// <summary><c>PIXMAN_a1</c>.</summary>
    A1 = (uint)pixman_format_code_t.PIXMAN_a1,

    /// <summary><c>PIXMAN_g1</c>: 1-bit grayscale.</summary>
    G1 = (uint)pixman_format_code_t.PIXMAN_g1,

    /// <summary><c>PIXMAN_yuy2</c>.</summary>
    Yuy2 = (uint)pixman_format_code_t.PIXMAN_yuy2,

    /// <summary><c>PIXMAN_yv12</c>.</summary>
    Yv12 = (uint)pixman_format_code_t.PIXMAN_yv12,
}
