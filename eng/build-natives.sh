#!/bin/sh
set -eu

usage() {
    echo "usage: eng/build-natives.sh osx" >&2
    echo "  Builds a universal arm64 + x86_64 pixman into runtimes/osx/native." >&2
    echo "  Use eng/build-natives.ps1 for win-x64." >&2
    exit 2
}

[ $# -eq 1 ] || usage
[ "$1" = "osx" ] || usage
root=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)

if [ "$(uname -s)" != "Darwin" ]; then
    echo "build-natives.sh builds the macOS runtime; use build-natives.ps1 for win-x64." >&2
    exit 1
fi

[ -f "$root/external/pixman/meson.build" ] || {
    echo "external/pixman is empty: run git submodule update --init." >&2
    exit 1
}

out="$root/runtimes/osx/native"
mkdir -p "$out"
rm -f "$out"/*.dylib

cpu_family() {
    case "$1" in
        arm64) echo aarch64 ;;
        *) echo "$1" ;;
    esac
}

# Both slices are cross builds, including the one for this host's own
# architecture. A native build can run its own compile checks and a cross build
# cannot, so mixing the two would lipo together halves configured differently.
write_cross_file() {
    arch=$1
    file=$2
    mkdir -p "$(dirname "$file")"
    cat > "$file" <<EOF
[binaries]
c = ['clang', '-arch', '$arch']
cpp = ['clang++', '-arch', '$arch']
strip = 'strip'

[properties]
needs_exe_wrapper = true

[built-in options]
c_args = ['-arch', '$arch']
c_link_args = ['-arch', '$arch']

[host_machine]
system = 'darwin'
subsystem = 'macos'
kernel = 'xnu'
cpu_family = '$(cpu_family "$arch")'
cpu = '$arch'
endian = 'little'
EOF
}

build_arch() {
    arch=$1
    build="$root/artifacts/natives/osx-$arch"
    cross="$root/artifacts/natives/osx-$arch.ini"
    rm -rf "$build"
    write_cross_file "$arch" "$cross"

    meson setup "$build" "$root/external/pixman" \
        --cross-file "$cross" \
        --buildtype=release \
        --default-library=shared \
        -Dtests=disabled \
        -Ddemos=disabled \
        -Dgtk=disabled \
        -Dlibpng=disabled \
        -Dopenmp=disabled
    ninja -C "$build"

    found=$(find "$build" -name 'libpixman-1*.dylib' -type f | head -1)
    [ -n "$found" ] || {
        echo "meson built no libpixman-1 dylib for $arch in $build." >&2
        exit 1
    }

    cp "$found" "$out/libpixman-1.0.$arch.dylib"
}

build_arch arm64
build_arch x86_64

lipo -create "$out/libpixman-1.0.arm64.dylib" "$out/libpixman-1.0.x86_64.dylib" \
    -output "$out/libpixman-1.0.dylib"
rm -f "$out/libpixman-1.0.arm64.dylib" "$out/libpixman-1.0.x86_64.dylib"
install_name_tool -id "@rpath/libpixman-1.0.dylib" "$out/libpixman-1.0.dylib"

lipo -info "$out/libpixman-1.0.dylib"
ls -l "$out"
