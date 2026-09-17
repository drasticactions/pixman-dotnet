#!/bin/sh
# Regenerates src/Pixman.NET/Native/Generated from the external/pixman
# submodule headers via ClangSharpPInvokeGenerator (a local dotnet tool, see
# .config/dotnet-tools.json). Run from the repository root:
#
#   dotnet tool restore
#   sh eng/generate.sh
set -eu
cd "$(dirname "$0")/.."

rm -rf src/Pixman.NET/Native/Generated
mkdir -p src/Pixman.NET/Native/Generated

# pixman.h includes pixman-version.h, which meson generates from
# pixman-version.h.in; substitute the submodule's pinned version here instead
# of requiring a meson build. Keep in sync with <Version> in
# Directory.Build.props.
PIXMAN_MAJOR=0
PIXMAN_MINOR=46
PIXMAN_MICRO=4
mkdir -p eng/include
sed -e "s/@PIXMAN_VERSION_MAJOR@/$PIXMAN_MAJOR/g" \
    -e "s/@PIXMAN_VERSION_MINOR@/$PIXMAN_MINOR/g" \
    -e "s/@PIXMAN_VERSION_MICRO@/$PIXMAN_MICRO/g" \
    external/pixman/pixman/pixman-version.h.in > eng/include/pixman-version.h

# The bundled libclang needs clang's builtin headers (stddef.h etc.); use the
# system clang's resource directory.
RESOURCE_DIR="$(clang -print-resource-dir)"

# The generator's exit code is its diagnostic (warning) count, so a successful
# run with warnings is nonzero; presence of the output files is checked instead.
dotnet tool run ClangSharpPInvokeGenerator -- --resource-directory "$RESOURCE_DIR" "@eng/pixman.rsp" || true

test -f src/Pixman.NET/Native/Generated/pixman/Libpixman.cs

# ClangSharp writes the library name as a string literal. Route it through the
# LibraryName constant instead so the iOS/tvOS/Catalyst builds, where pixman is
# linked statically, can switch it to "__Internal" (see Libpixman.Manual.cs).
find src/Pixman.NET/Native/Generated -name '*.cs' -exec sed -i.bak 's/\[DllImport("pixman-1",/[DllImport(LibraryName,/' {} \; -exec rm {}.bak \;
grep -rq 'DllImport(LibraryName,' src/Pixman.NET/Native/Generated/pixman/Libpixman.cs
