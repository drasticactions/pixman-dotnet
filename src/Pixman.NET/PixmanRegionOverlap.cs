using Pixman.Native;

namespace Pixman;

/// <summary>The result of a region containment test.</summary>
public enum PixmanRegionOverlap : uint
{
    /// <summary><c>PIXMAN_REGION_OUT</c>: entirely outside the region.</summary>
    Out = (uint)pixman_region_overlap_t.PIXMAN_REGION_OUT,

    /// <summary><c>PIXMAN_REGION_IN</c>: entirely inside the region.</summary>
    In = (uint)pixman_region_overlap_t.PIXMAN_REGION_IN,

    /// <summary><c>PIXMAN_REGION_PART</c>: partially inside the region.</summary>
    Part = (uint)pixman_region_overlap_t.PIXMAN_REGION_PART,
}
