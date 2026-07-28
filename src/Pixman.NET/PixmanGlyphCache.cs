using Pixman.Native;

namespace Pixman;

/// <summary>A cache of rasterized glyph images for use with <see cref="PixmanImage.CompositeGlyphs"/>.</summary>
/// <remarks>
/// Wraps <c>pixman_glyph_cache_t</c>. Glyphs are keyed by an opaque (font key, glyph key) pointer
/// pair; pixman only compares the pointers, so any stable unique values work.
/// </remarks>
public sealed unsafe class PixmanGlyphCache : IDisposable
{
    private pixman_glyph_cache_t* _cache;

    private PixmanGlyphCache(pixman_glyph_cache_t* cache) => _cache = cache;

    /// <summary>Creates an empty glyph cache.</summary>
    /// <returns>The new cache.</returns>
    /// <exception cref="PixmanException">Thrown when the cache cannot be created.</exception>
    public static PixmanGlyphCache Create()
    {
        var cache = Libpixman.pixman_glyph_cache_create();
        PixmanException.ThrowIfNull(cache, "pixman_glyph_cache_create failed");
        return new PixmanGlyphCache(cache);
    }

    /// <summary>Gets the raw <c>pixman_glyph_cache_t*</c> for use with the <see cref="Libpixman"/> layer.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the cache has been disposed.</exception>
    public IntPtr Handle => (IntPtr)NativePtr;

    /// <summary>Gets a value indicating whether the cache has been disposed.</summary>
    public bool IsDisposed => _cache is null;

    internal pixman_glyph_cache_t* NativePtr
    {
        get
        {
            ObjectDisposedException.ThrowIf(_cache is null, this);
            return _cache;
        }
    }

    /// <summary>Freezes the cache. Glyphs may only be inserted or removed while the cache is frozen.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the cache has been disposed.</exception>
    public void Freeze() => Libpixman.pixman_glyph_cache_freeze(NativePtr);

    /// <summary>Thaws the cache. If the cache grew too large while frozen, pixman may evict glyphs now.</summary>
    /// <exception cref="ObjectDisposedException">Thrown when the cache has been disposed.</exception>
    public void Thaw() => Libpixman.pixman_glyph_cache_thaw(NativePtr);

    /// <summary>Inserts a glyph image into the cache. The cache must be frozen.</summary>
    /// <param name="fontKey">The opaque font key.</param>
    /// <param name="glyphKey">The opaque glyph key.</param>
    /// <param name="originX">The x coordinate of the glyph origin within <paramref name="glyphImage"/>.</param>
    /// <param name="originY">The y coordinate of the glyph origin within <paramref name="glyphImage"/>.</param>
    /// <param name="glyphImage">The rasterized glyph; pixman copies its pixels, so it may be disposed afterwards.</param>
    /// <returns>The cached glyph pointer to store in <see cref="PixmanGlyph.Glyph"/>; valid until the glyph is removed or the cache is destroyed.</returns>
    /// <exception cref="PixmanException">Thrown when the glyph cannot be inserted.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the cache or <paramref name="glyphImage"/> has been disposed.</exception>
    public IntPtr Insert(IntPtr fontKey, IntPtr glyphKey, int originX, int originY, PixmanImage glyphImage)
    {
        var glyph = Libpixman.pixman_glyph_cache_insert(NativePtr, (void*)fontKey, (void*)glyphKey, originX, originY, glyphImage.NativePtr);
        PixmanException.ThrowIfNull(glyph, "pixman_glyph_cache_insert failed");
        return (IntPtr)glyph;
    }

    /// <summary>Looks up a cached glyph.</summary>
    /// <param name="fontKey">The opaque font key.</param>
    /// <param name="glyphKey">The opaque glyph key.</param>
    /// <returns>The cached glyph pointer, or <see cref="IntPtr.Zero"/> when the glyph is not cached.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the cache has been disposed.</exception>
    public IntPtr Lookup(IntPtr fontKey, IntPtr glyphKey)
        => (IntPtr)Libpixman.pixman_glyph_cache_lookup(NativePtr, (void*)fontKey, (void*)glyphKey);

    /// <summary>Removes a glyph from the cache. The cache must be frozen.</summary>
    /// <param name="fontKey">The opaque font key.</param>
    /// <param name="glyphKey">The opaque glyph key.</param>
    /// <exception cref="ObjectDisposedException">Thrown when the cache has been disposed.</exception>
    public void Remove(IntPtr fontKey, IntPtr glyphKey)
        => Libpixman.pixman_glyph_cache_remove(NativePtr, (void*)fontKey, (void*)glyphKey);

    /// <summary>Computes the bounding box of positioned glyphs.</summary>
    /// <param name="glyphs">The positioned glyphs, with <see cref="PixmanGlyph.Glyph"/> pointers from this cache.</param>
    /// <returns>The bounding box.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the cache has been disposed.</exception>
    public PixmanBox32 GetExtents(ReadOnlySpan<PixmanGlyph> glyphs)
    {
        var extents = default(pixman_box32);
        fixed (PixmanGlyph* glyphsPtr = glyphs)
        {
            Libpixman.pixman_glyph_get_extents(NativePtr, glyphs.Length, (pixman_glyph_t*)glyphsPtr, &extents);
        }

        return *(PixmanBox32*)&extents;
    }

    /// <summary>Determines the narrowest mask format that can hold all the given glyphs.</summary>
    /// <param name="glyphs">The glyphs, with <see cref="PixmanGlyph.Glyph"/> pointers from this cache.</param>
    /// <returns>The mask format.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the cache has been disposed.</exception>
    public PixmanFormat GetMaskFormat(ReadOnlySpan<PixmanGlyph> glyphs)
    {
        fixed (PixmanGlyph* glyphsPtr = glyphs)
        {
            return (PixmanFormat)Libpixman.pixman_glyph_get_mask_format(NativePtr, glyphs.Length, (pixman_glyph_t*)glyphsPtr);
        }
    }

    /// <summary>Destroys the cache and all cached glyphs.</summary>
    public void Dispose()
    {
        if (_cache is not null)
        {
            Libpixman.pixman_glyph_cache_destroy(_cache);
            _cache = null;
        }
    }
}
