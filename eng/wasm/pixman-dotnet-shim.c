/*
 * Compiled into the browser-wasm pixman-1.a only. The Mono interpreter, which
 * runs browser apps unless they are AOT compiled, cannot call a P/Invoke with
 * more than 12 integer arguments (INTERP_ICALL_TRAMP_IARGS); pixman_composite_glyphs
 * takes 15. This packs them into one struct. Keep the layout in sync with
 * pixman_dotnet_composite_glyphs_args in Libpixman.Manual.cs.
 */
#include <stdint.h>
#include <pixman.h>

typedef struct pixman_dotnet_composite_glyphs_args
{
    uint32_t op;
    pixman_image_t *src;
    pixman_image_t *dest;
    uint32_t mask_format;
    int32_t src_x;
    int32_t src_y;
    int32_t mask_x;
    int32_t mask_y;
    int32_t dest_x;
    int32_t dest_y;
    int32_t width;
    int32_t height;
    pixman_glyph_cache_t *cache;
    int32_t n_glyphs;
    const pixman_glyph_t *glyphs;
} pixman_dotnet_composite_glyphs_args;

__attribute__((visibility("default")))
void pixman_dotnet_composite_glyphs(const pixman_dotnet_composite_glyphs_args *a)
{
    pixman_composite_glyphs((pixman_op_t)a->op, a->src, a->dest, (pixman_format_code_t)a->mask_format,
                            a->src_x, a->src_y, a->mask_x, a->mask_y, a->dest_x, a->dest_y,
                            a->width, a->height, a->cache, a->n_glyphs, a->glyphs);
}
