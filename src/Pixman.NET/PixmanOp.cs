using Pixman.Native;

namespace Pixman;

/// <summary>A compositing operator.</summary>
public enum PixmanOp : uint
{
    /// <summary><c>PIXMAN_OP_CLEAR</c>.</summary>
    Clear = (uint)pixman_op_t.PIXMAN_OP_CLEAR,

    /// <summary><c>PIXMAN_OP_SRC</c>.</summary>
    Src = (uint)pixman_op_t.PIXMAN_OP_SRC,

    /// <summary><c>PIXMAN_OP_DST</c>.</summary>
    Dst = (uint)pixman_op_t.PIXMAN_OP_DST,

    /// <summary><c>PIXMAN_OP_OVER</c>.</summary>
    Over = (uint)pixman_op_t.PIXMAN_OP_OVER,

    /// <summary><c>PIXMAN_OP_OVER_REVERSE</c>.</summary>
    OverReverse = (uint)pixman_op_t.PIXMAN_OP_OVER_REVERSE,

    /// <summary><c>PIXMAN_OP_IN</c>.</summary>
    In = (uint)pixman_op_t.PIXMAN_OP_IN,

    /// <summary><c>PIXMAN_OP_IN_REVERSE</c>.</summary>
    InReverse = (uint)pixman_op_t.PIXMAN_OP_IN_REVERSE,

    /// <summary><c>PIXMAN_OP_OUT</c>.</summary>
    Out = (uint)pixman_op_t.PIXMAN_OP_OUT,

    /// <summary><c>PIXMAN_OP_OUT_REVERSE</c>.</summary>
    OutReverse = (uint)pixman_op_t.PIXMAN_OP_OUT_REVERSE,

    /// <summary><c>PIXMAN_OP_ATOP</c>.</summary>
    Atop = (uint)pixman_op_t.PIXMAN_OP_ATOP,

    /// <summary><c>PIXMAN_OP_ATOP_REVERSE</c>.</summary>
    AtopReverse = (uint)pixman_op_t.PIXMAN_OP_ATOP_REVERSE,

    /// <summary><c>PIXMAN_OP_XOR</c>.</summary>
    Xor = (uint)pixman_op_t.PIXMAN_OP_XOR,

    /// <summary><c>PIXMAN_OP_ADD</c>.</summary>
    Add = (uint)pixman_op_t.PIXMAN_OP_ADD,

    /// <summary><c>PIXMAN_OP_SATURATE</c>.</summary>
    Saturate = (uint)pixman_op_t.PIXMAN_OP_SATURATE,

    /// <summary><c>PIXMAN_OP_DISJOINT_CLEAR</c>.</summary>
    DisjointClear = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_CLEAR,

    /// <summary><c>PIXMAN_OP_DISJOINT_SRC</c>.</summary>
    DisjointSrc = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_SRC,

    /// <summary><c>PIXMAN_OP_DISJOINT_DST</c>.</summary>
    DisjointDst = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_DST,

    /// <summary><c>PIXMAN_OP_DISJOINT_OVER</c>.</summary>
    DisjointOver = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_OVER,

    /// <summary><c>PIXMAN_OP_DISJOINT_OVER_REVERSE</c>.</summary>
    DisjointOverReverse = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_OVER_REVERSE,

    /// <summary><c>PIXMAN_OP_DISJOINT_IN</c>.</summary>
    DisjointIn = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_IN,

    /// <summary><c>PIXMAN_OP_DISJOINT_IN_REVERSE</c>.</summary>
    DisjointInReverse = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_IN_REVERSE,

    /// <summary><c>PIXMAN_OP_DISJOINT_OUT</c>.</summary>
    DisjointOut = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_OUT,

    /// <summary><c>PIXMAN_OP_DISJOINT_OUT_REVERSE</c>.</summary>
    DisjointOutReverse = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_OUT_REVERSE,

    /// <summary><c>PIXMAN_OP_DISJOINT_ATOP</c>.</summary>
    DisjointAtop = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_ATOP,

    /// <summary><c>PIXMAN_OP_DISJOINT_ATOP_REVERSE</c>.</summary>
    DisjointAtopReverse = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_ATOP_REVERSE,

    /// <summary><c>PIXMAN_OP_DISJOINT_XOR</c>.</summary>
    DisjointXor = (uint)pixman_op_t.PIXMAN_OP_DISJOINT_XOR,

    /// <summary><c>PIXMAN_OP_CONJOINT_CLEAR</c>.</summary>
    ConjointClear = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_CLEAR,

    /// <summary><c>PIXMAN_OP_CONJOINT_SRC</c>.</summary>
    ConjointSrc = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_SRC,

    /// <summary><c>PIXMAN_OP_CONJOINT_DST</c>.</summary>
    ConjointDst = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_DST,

    /// <summary><c>PIXMAN_OP_CONJOINT_OVER</c>.</summary>
    ConjointOver = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_OVER,

    /// <summary><c>PIXMAN_OP_CONJOINT_OVER_REVERSE</c>.</summary>
    ConjointOverReverse = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_OVER_REVERSE,

    /// <summary><c>PIXMAN_OP_CONJOINT_IN</c>.</summary>
    ConjointIn = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_IN,

    /// <summary><c>PIXMAN_OP_CONJOINT_IN_REVERSE</c>.</summary>
    ConjointInReverse = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_IN_REVERSE,

    /// <summary><c>PIXMAN_OP_CONJOINT_OUT</c>.</summary>
    ConjointOut = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_OUT,

    /// <summary><c>PIXMAN_OP_CONJOINT_OUT_REVERSE</c>.</summary>
    ConjointOutReverse = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_OUT_REVERSE,

    /// <summary><c>PIXMAN_OP_CONJOINT_ATOP</c>.</summary>
    ConjointAtop = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_ATOP,

    /// <summary><c>PIXMAN_OP_CONJOINT_ATOP_REVERSE</c>.</summary>
    ConjointAtopReverse = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_ATOP_REVERSE,

    /// <summary><c>PIXMAN_OP_CONJOINT_XOR</c>.</summary>
    ConjointXor = (uint)pixman_op_t.PIXMAN_OP_CONJOINT_XOR,

    /// <summary><c>PIXMAN_OP_MULTIPLY</c>.</summary>
    Multiply = (uint)pixman_op_t.PIXMAN_OP_MULTIPLY,

    /// <summary><c>PIXMAN_OP_SCREEN</c>.</summary>
    Screen = (uint)pixman_op_t.PIXMAN_OP_SCREEN,

    /// <summary><c>PIXMAN_OP_OVERLAY</c>.</summary>
    Overlay = (uint)pixman_op_t.PIXMAN_OP_OVERLAY,

    /// <summary><c>PIXMAN_OP_DARKEN</c>.</summary>
    Darken = (uint)pixman_op_t.PIXMAN_OP_DARKEN,

    /// <summary><c>PIXMAN_OP_LIGHTEN</c>.</summary>
    Lighten = (uint)pixman_op_t.PIXMAN_OP_LIGHTEN,

    /// <summary><c>PIXMAN_OP_COLOR_DODGE</c>.</summary>
    ColorDodge = (uint)pixman_op_t.PIXMAN_OP_COLOR_DODGE,

    /// <summary><c>PIXMAN_OP_COLOR_BURN</c>.</summary>
    ColorBurn = (uint)pixman_op_t.PIXMAN_OP_COLOR_BURN,

    /// <summary><c>PIXMAN_OP_HARD_LIGHT</c>.</summary>
    HardLight = (uint)pixman_op_t.PIXMAN_OP_HARD_LIGHT,

    /// <summary><c>PIXMAN_OP_SOFT_LIGHT</c>.</summary>
    SoftLight = (uint)pixman_op_t.PIXMAN_OP_SOFT_LIGHT,

    /// <summary><c>PIXMAN_OP_DIFFERENCE</c>.</summary>
    Difference = (uint)pixman_op_t.PIXMAN_OP_DIFFERENCE,

    /// <summary><c>PIXMAN_OP_EXCLUSION</c>.</summary>
    Exclusion = (uint)pixman_op_t.PIXMAN_OP_EXCLUSION,

    /// <summary><c>PIXMAN_OP_HSL_HUE</c>.</summary>
    HslHue = (uint)pixman_op_t.PIXMAN_OP_HSL_HUE,

    /// <summary><c>PIXMAN_OP_HSL_SATURATION</c>.</summary>
    HslSaturation = (uint)pixman_op_t.PIXMAN_OP_HSL_SATURATION,

    /// <summary><c>PIXMAN_OP_HSL_COLOR</c>.</summary>
    HslColor = (uint)pixman_op_t.PIXMAN_OP_HSL_COLOR,

    /// <summary><c>PIXMAN_OP_HSL_LUMINOSITY</c>.</summary>
    HslLuminosity = (uint)pixman_op_t.PIXMAN_OP_HSL_LUMINOSITY,
}
