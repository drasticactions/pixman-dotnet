#!/bin/sh
# Builds the pixman payloads that ship in the package. Windows is covered by
# eng/build-natives.ps1. Outputs land in runtimes/<rid>/native (shared
# libraries and the browser-wasm archive) and xcframeworks/ (Apple static
# slices); both directories are gitignored and merged into the nupkg by pack.
set -eu

usage() {
    cat >&2 <<'EOF'
usage: eng/build-natives.sh <target>

  osx                  universal arm64 + x86_64 dylib          -> runtimes/osx/native
  linux                shared library for this machine's arch  -> runtimes/linux-{x64,arm64}/native
  linux-docker <arch>  the linux target inside manylinux_2_28 (glibc 2.28); arch is x64 or arm64
  android [<arch>]     arm64, x64 or both (default)             -> runtimes/android-{arm64,x64}/native
  apple                iOS, tvOS and Mac Catalyst static slices -> xcframeworks/pixman.xcframework
  wasm                 browser-wasm static archive              -> runtimes/browser-wasm/native/pixman-1.a

Requirements: meson >= 1.3 and ninja for every target; Xcode for osx and apple;
an Android NDK (ANDROID_NDK_HOME, ANDROID_NDK_ROOT or ANDROID_HOME/ndk) for
android; docker for linux-docker; Emscripten $PIXMAN_WASM_EMSCRIPTEN for wasm
(EMSDK, PATH, or the .NET wasm-tools workload pack under DOTNET_ROOT).
EOF
    exit 2
}

# The Emscripten release the .NET 10 wasm-tools workload links apps with. The
# archive must be produced by the same release or the final link can fail.
PIXMAN_WASM_EMSCRIPTEN=3.1.56

# Deployment targets, matching the .NET 10 minimums for each Apple platform.
IOS_MIN=15.0
TVOS_MIN=15.0
CATALYST_MIN=15.0

[ $# -ge 1 ] || usage
target=$1
shift
root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
src="$root/external/pixman"

[ -f "$src/meson.build" ] || {
    echo "external/pixman is empty: run git submodule update --init." >&2
    exit 1
}

common_args="--buildtype=release -Dtests=disabled -Ddemos=disabled -Dgtk=disabled -Dlibpng=disabled -Dopenmp=disabled"

meson_setup() {
    # meson_setup <builddir> <shared|static> [extra meson args...]
    build=$1
    kind=$2
    shift 2
    rm -rf "$build"
    # shellcheck disable=SC2086
    meson setup "$build" "$src" --default-library="$kind" $common_args "$@"
    ninja -C "$build"
}

find_built() {
    # find_built <builddir> <glob> : the real (non-symlink) library file
    found=$(find "$1" -name "$2" -type f | head -1)
    [ -n "$found" ] || {
        echo "meson built no $2 in $1." >&2
        exit 1
    }
    echo "$found"
}

cpu_family() {
    case "$1" in
        arm64 | aarch64) echo aarch64 ;;
        x86_64 | x64) echo x86_64 ;;
        *) echo "$1" ;;
    esac
}

write_cross_file() {
    # write_cross_file <file> <system> <subsystem> <cpu_family> <cpu> <c-array> <ar> <strip> <c_args-array> [link_args-array]
    file=$1
    mkdir -p "$(dirname "$file")"
    cat > "$file" <<EOF
[binaries]
c = $6
ar = '$7'
strip = '$8'

[properties]
needs_exe_wrapper = true

[built-in options]
c_args = $9
c_link_args = ${10:-$9}

[host_machine]
system = '$2'
subsystem = '$3'
kernel = '${11:-linux}'
cpu_family = '$4'
cpu = '$5'
endian = 'little'
EOF
}

# ---------------------------------------------------------------------------
# macOS: two cross builds lipo'd together. Both slices are cross builds,
# including the host's own architecture, so they are configured identically.
build_osx() {
    [ "$(uname -s)" = "Darwin" ] || { echo "osx builds need macOS." >&2; exit 1; }
    out="$root/runtimes/osx/native"
    mkdir -p "$out"
    rm -f "$out"/*.dylib

    for arch in arm64 x86_64; do
        build="$root/artifacts/natives/osx-$arch"
        cross="$build.ini"
        write_cross_file "$cross" darwin macos "$(cpu_family "$arch")" "$arch" \
            "['clang', '-arch', '$arch']" ar strip "['-arch', '$arch']" "['-arch', '$arch']" xnu
        meson_setup "$build" shared --cross-file "$cross"
        cp "$(find_built "$build" 'libpixman-1*.dylib')" "$out/libpixman-1.0.$arch.dylib"
    done

    lipo -create "$out/libpixman-1.0.arm64.dylib" "$out/libpixman-1.0.x86_64.dylib" \
        -output "$out/libpixman-1.0.dylib"
    rm -f "$out/libpixman-1.0.arm64.dylib" "$out/libpixman-1.0.x86_64.dylib"
    install_name_tool -id "@rpath/libpixman-1.0.dylib" "$out/libpixman-1.0.dylib"
    lipo -info "$out/libpixman-1.0.dylib"
    ls -l "$out"
}

# ---------------------------------------------------------------------------
# Linux: a native build for the machine (or container) this runs on. The
# shipped file is unversioned so the "pixman-1" DllImport finds it directly.
build_linux() {
    [ "$(uname -s)" = "Linux" ] || { echo "linux builds need Linux; use linux-docker elsewhere." >&2; exit 1; }
    case "$(uname -m)" in
        x86_64) rid=linux-x64 ;;
        aarch64) rid=linux-arm64 ;;
        *) echo "unsupported Linux architecture $(uname -m)." >&2; exit 1 ;;
    esac
    out="$root/runtimes/$rid/native"
    build="$root/artifacts/natives/$rid"
    mkdir -p "$out"
    rm -f "$out"/*.so*

    meson_setup "$build" shared
    cp "$(find_built "$build" 'libpixman-1.so.*')" "$out/libpixman-1.so"
    strip --strip-unneeded "$out/libpixman-1.so"
    ls -l "$out"
}

build_linux_docker() {
    arch=${1:-}
    case "$arch" in
        x64) platform=linux/amd64; image=quay.io/pypa/manylinux_2_28_x86_64 ;;
        arm64) platform=linux/arm64; image=quay.io/pypa/manylinux_2_28_aarch64 ;;
        *) echo "linux-docker needs x64 or arm64." >&2; usage ;;
    esac
    # manylinux_2_28 (AlmaLinux 8) pins glibc 2.28, so the library runs on any
    # distro from that era on (Ubuntu 20.04, Debian 10, RHEL 8). The image has
    # no meson, so it comes from pip. The repo is mounted so outputs land in place.
    docker run --rm --platform "$platform" \
        -v "$root:/work" -w /work \
        "$image" sh -euc '
            py=$(ls -d /opt/python/cp312-cp312 /opt/python/cp311-cp311 2>/dev/null | head -1)
            "$py/bin/pip" install -q "meson>=1.3" ninja
            PATH="$py/bin:$PATH" sh eng/build-natives.sh linux
        '
}

# ---------------------------------------------------------------------------
# Android: NDK clang per ABI, API level 21. arm64 has NEON unconditionally and
# x86_64 detects SSE at runtime, so no cpu-features shim is needed.
find_ndk() {
    for candidate in "${ANDROID_NDK_HOME:-}" "${ANDROID_NDK_ROOT:-}" "${ANDROID_NDK:-}"; do
        [ -n "$candidate" ] && [ -d "$candidate/toolchains/llvm" ] && { echo "$candidate"; return; }
    done
    for sdk in "${ANDROID_HOME:-}" "${ANDROID_SDK_ROOT:-}" "$HOME/Library/Android/sdk" "$HOME/Android/Sdk"; do
        [ -n "$sdk" ] && [ -d "$sdk/ndk" ] || continue
        latest=$(ls "$sdk/ndk" | sort -V | tail -1)
        [ -n "$latest" ] && { echo "$sdk/ndk/$latest"; return; }
    done
    echo "no Android NDK found: set ANDROID_NDK_HOME." >&2
    exit 1
}

build_android_arch() {
    arch=$1
    ndk=$2
    api=21
    case "$arch" in
        arm64) rid=android-arm64; triple=aarch64-linux-android ;;
        x64) rid=android-x64; triple=x86_64-linux-android ;;
        *) echo "android needs arm64 or x64." >&2; usage ;;
    esac
    host=$(ls "$ndk/toolchains/llvm/prebuilt" | head -1)
    bin="$ndk/toolchains/llvm/prebuilt/$host/bin"
    [ -x "$bin/$triple$api-clang" ] || { echo "$bin/$triple$api-clang not found." >&2; exit 1; }

    out="$root/runtimes/$rid/native"
    build="$root/artifacts/natives/$rid"
    cross="$build.ini"
    mkdir -p "$out"
    rm -f "$out"/*.so

    # 16 KB page alignment is mandatory for Android 15+ on arm64.
    write_cross_file "$cross" android android "$(cpu_family "$arch")" "$(cpu_family "$arch")" \
        "['$bin/$triple$api-clang']" "$bin/llvm-ar" "$bin/llvm-strip" "[]" "['-Wl,-z,max-page-size=16384']"
    meson_setup "$build" shared --cross-file "$cross"
    cp "$(find_built "$build" 'libpixman-1.so*')" "$out/libpixman-1.so"
    "$bin/llvm-strip" --strip-unneeded "$out/libpixman-1.so"
    "$bin/llvm-readelf" -d "$out/libpixman-1.so" | grep -E 'SONAME|NEEDED' || true
    ls -l "$out"
}

build_android() {
    ndk=$(find_ndk)
    echo "Using NDK $ndk"
    if [ $# -eq 0 ]; then
        build_android_arch arm64 "$ndk"
        build_android_arch x64 "$ndk"
    else
        build_android_arch "$1" "$ndk"
    fi
}

# ---------------------------------------------------------------------------
# Apple: static slices per (platform, arch), fat per platform, then one
# xcframework. Consumers link it through the buildTransitive targets.
build_apple_slice() {
    # build_apple_slice <slice> <arch> <sdk> <subsystem> <target-flag...>
    slice=$1
    arch=$2
    sdk=$3
    subsystem=$4
    shift 4
    sysroot=$(xcrun --sdk "$sdk" --show-sdk-path)
    build="$root/artifacts/natives/apple/$slice-$arch"
    cross="$build.ini"

    flags="'-arch', '$arch', '-isysroot', '$sysroot'"
    for f in "$@"; do flags="$flags, '$f'"; done
    write_cross_file "$cross" darwin "$subsystem" "$(cpu_family "$arch")" "$arch" \
        "['clang', $flags]" ar strip "[$flags]" "[$flags]" xnu
    meson_setup "$build" static --cross-file "$cross"
    cp "$(find_built "$build" 'libpixman-1.a')" "$build.a"
}

build_apple_platform() {
    # build_apple_platform <slice> <sdk> <subsystem> <archs> <target-flag...>
    slice=$1
    sdk=$2
    subsystem=$3
    archs=$4
    shift 4
    libs=""
    for arch in $archs; do
        build_apple_slice "$slice" "$arch" "$sdk" "$subsystem" "$@"
        libs="$libs $root/artifacts/natives/apple/$slice-$arch.a"
    done
    # shellcheck disable=SC2086
    lipo -create $libs -output "$root/artifacts/natives/apple/$slice/libpixman-1.a" 2>/dev/null || {
        mkdir -p "$root/artifacts/natives/apple/$slice"
        # shellcheck disable=SC2086
        lipo -create $libs -output "$root/artifacts/natives/apple/$slice/libpixman-1.a"
    }
    lipo -info "$root/artifacts/natives/apple/$slice/libpixman-1.a"
}

build_apple() {
    [ "$(uname -s)" = "Darwin" ] || { echo "apple builds need macOS." >&2; exit 1; }
    stage="$root/artifacts/natives/apple"
    rm -rf "$stage"
    mkdir -p "$stage"

    build_apple_platform ios iphoneos ios "arm64" "-miphoneos-version-min=$IOS_MIN"
    build_apple_platform iossimulator iphonesimulator ios-simulator "arm64 x86_64" "-mios-simulator-version-min=$IOS_MIN"
    build_apple_platform tvos appletvos tvos "arm64" "-mtvos-version-min=$TVOS_MIN"
    build_apple_platform tvossimulator appletvsimulator tvos-simulator "arm64 x86_64" "-mtvos-simulator-version-min=$TVOS_MIN"
    # Catalyst is the macOS SDK with an iOS-macabi target; the arch goes in the target triple.
    build_apple_slice maccatalyst arm64 macosx macos "-target" "arm64-apple-ios$CATALYST_MIN-macabi"
    build_apple_slice maccatalyst x86_64 macosx macos "-target" "x86_64-apple-ios$CATALYST_MIN-macabi"
    mkdir -p "$stage/maccatalyst"
    lipo -create "$stage/maccatalyst-arm64.a" "$stage/maccatalyst-x86_64.a" -output "$stage/maccatalyst/libpixman-1.a"
    lipo -info "$stage/maccatalyst/libpixman-1.a"

    # Headers are informational (the bindings need none), taken from the device build.
    headers="$stage/include"
    mkdir -p "$headers"
    cp "$src/pixman/pixman.h" "$root/artifacts/natives/apple/ios-arm64/pixman/pixman-version.h" "$headers/"

    out="$root/xcframeworks/pixman.xcframework"
    rm -rf "$out"
    mkdir -p "$root/xcframeworks"
    xcodebuild -create-xcframework \
        -library "$stage/ios/libpixman-1.a" -headers "$headers" \
        -library "$stage/iossimulator/libpixman-1.a" -headers "$headers" \
        -library "$stage/tvos/libpixman-1.a" -headers "$headers" \
        -library "$stage/tvossimulator/libpixman-1.a" -headers "$headers" \
        -library "$stage/maccatalyst/libpixman-1.a" -headers "$headers" \
        -output "$out"
    ls "$out"
}

# ---------------------------------------------------------------------------
# Browser WebAssembly: a static archive the wasm-tools workload links into the
# app. The file name doubles as the pinvoke module name, hence pixman-1.a.
find_emcc() {
    # Sets emdir (and, for the workload packs, the environment emcc needs).
    if [ -n "${EMSDK:-}" ] && [ -x "$EMSDK/upstream/emscripten/emcc" ]; then
        emdir="$EMSDK/upstream/emscripten"
        return
    fi
    if command -v emcc >/dev/null 2>&1; then
        emdir=$(dirname "$(command -v emcc)")
        return
    fi
    # The wasm-tools workload ships Emscripten as packs: Sdk (emcc + llvm),
    # Node and Cache. Their .emscripten config reads these DOTNET_EMSCRIPTEN_*
    # variables instead of hard-coding paths. Stable pack versions win over previews.
    for dotnet_root in "${DOTNET_ROOT:-}" "$HOME/.dotnet" /usr/share/dotnet /usr/local/share/dotnet /usr/lib/dotnet; do
        [ -n "$dotnet_root" ] && [ -d "$dotnet_root/packs" ] || continue
        for pack in "$dotnet_root/packs/Microsoft.NET.Runtime.Emscripten.$PIXMAN_WASM_EMSCRIPTEN.Sdk."*; do
            [ -d "$pack" ] || continue
            ver=$(ls "$pack" | grep -v -- '-' | sort -V | tail -1)
            [ -n "$ver" ] || ver=$(ls "$pack" | sort -V | tail -1)
            tools="$pack/$ver/tools"
            [ -x "$tools/emscripten/emcc" ] || continue
            host_rid=${pack##*.Sdk.}
            node_pack="$dotnet_root/packs/Microsoft.NET.Runtime.Emscripten.$PIXMAN_WASM_EMSCRIPTEN.Node.$host_rid/$ver"
            cache_pack="$dotnet_root/packs/Microsoft.NET.Runtime.Emscripten.$PIXMAN_WASM_EMSCRIPTEN.Cache.$host_rid/$ver"
            export EM_CONFIG="$tools/emscripten/.emscripten"
            export DOTNET_EMSCRIPTEN_LLVM_ROOT="$tools/bin"
            export DOTNET_EMSCRIPTEN_BINARYEN_ROOT="$tools"
            export DOTNET_EMSCRIPTEN_NODE_JS="$node_pack/tools/bin/node"
            [ -d "$cache_pack/tools/emscripten/cache" ] && export EM_CACHE="$cache_pack/tools/emscripten/cache" EM_FROZEN_CACHE=1
            emdir="$tools/emscripten"
            return
        done
    done
    echo "no emcc found: install emsdk $PIXMAN_WASM_EMSCRIPTEN, or the .NET wasm-tools workload." >&2
    exit 1
}

build_wasm() {
    find_emcc
    version=$("$emdir/emcc" --version | head -1 | sed -E 's/.* ([0-9]+\.[0-9]+\.[0-9]+).*/\1/')
    echo "Using emcc $version from $emdir"
    if [ "$version" != "$PIXMAN_WASM_EMSCRIPTEN" ] && [ "${PIXMAN_ALLOW_EMSCRIPTEN_MISMATCH:-}" != "1" ]; then
        echo "emcc $version does not match the pinned $PIXMAN_WASM_EMSCRIPTEN; set PIXMAN_ALLOW_EMSCRIPTEN_MISMATCH=1 to build anyway." >&2
        exit 1
    fi

    out="$root/runtimes/browser-wasm/native"
    build="$root/artifacts/natives/browser-wasm"
    cross="$build.ini"
    mkdir -p "$out"
    rm -f "$out"/*.a

    write_cross_file "$cross" emscripten emscripten wasm32 wasm32 \
        "['$emdir/emcc']" "$emdir/emar" "$emdir/emstrip" "[]" "[]" emscripten
    # tls=disabled: the .NET runtime links single-threaded and never initialises
    # the wasm TLS base, so a __thread variable (pixman's fast-path cache) would
    # land at offset zero, on top of the string literals. The pthread-key
    # fallback works with Emscripten's single-thread stubs.
    meson_setup "$build" static --cross-file "$cross" -Dtls=disabled
    cp "$(find_built "$build" 'libpixman-1.a')" "$out/pixman-1.a"
    # The interpreter cannot call pixman_composite_glyphs directly (see the shim).
    "$emdir/emcc" -O2 -c "$root/eng/wasm/pixman-dotnet-shim.c" \
        -I"$src/pixman" -I"$build/pixman" -o "$build/pixman-dotnet-shim.o"
    "$emdir/emar" r "$out/pixman-1.a" "$build/pixman-dotnet-shim.o"
    ls -l "$out"
}

case "$target" in
    osx) build_osx ;;
    linux) build_linux ;;
    linux-docker) build_linux_docker "$@" ;;
    android) build_android "$@" ;;
    apple) build_apple ;;
    wasm) build_wasm ;;
    *) usage ;;
esac
