#Requires -Version 7
<#
.SYNOPSIS
Builds pixman-1.dll for win-x64 or win-arm64 into runtimes/<rid>/native.

.DESCRIPTION
Uses MSVC via the Visual Studio developer shell. win-arm64 is a cross build
from an x64 host (the arm64 tools ship with the x64 toolset), configured
through a meson cross file. pixman's SIMD paths are GNU-assembler based on
ARM, so the arm64 build uses the portable C paths; x64 gets SSE2/SSSE3 with
runtime detection.
#>
[CmdletBinding()]
param(
    [ValidateSet('win-x64', 'win-arm64')]
    [string]$Rid = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if (-not (Test-Path "$root/external/pixman/meson.build")) {
    throw 'external/pixman is empty: run git submodule update --init.'
}

$targetArch = if ($Rid -eq 'win-arm64') { 'arm64' } else { 'x64' }
$hostArch = if ($env:PROCESSOR_ARCHITECTURE -eq 'ARM64') { 'arm64' } else { 'x64' }

# Always enter a dev shell for the requested target, even if cl.exe is already
# on PATH: a shell set up for x64 would otherwise silently produce x64 objects
# for the arm64 build.
$vswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
if (-not (Test-Path $vswhere)) { throw 'No vswhere.exe to find Visual Studio.' }
$component = if ($targetArch -eq 'arm64') { 'Microsoft.VisualStudio.Component.VC.Tools.ARM64' } else { 'Microsoft.VisualStudio.Component.VC.Tools.x86.x64' }
$vs = & $vswhere -latest -products * -requires $component -property installationPath
if (-not $vs) { throw "No Visual Studio with the C++ $targetArch toolset ($component) is installed." }
Import-Module (Join-Path $vs 'Common7/Tools/Microsoft.VisualStudio.DevShell.dll')
Enter-VsDevShell -VsInstallPath $vs -SkipAutomaticLocation -DevCmdArguments "-arch=$targetArch -host_arch=$hostArch" | Out-Null
if (-not (Get-Command cl.exe -ErrorAction SilentlyContinue)) { throw 'Enter-VsDevShell did not put cl.exe on PATH.' }

$build = Join-Path $root "artifacts/natives/$Rid"
$out = Join-Path $root "runtimes/$Rid/native"
if (Test-Path $build) { Remove-Item -Recurse -Force $build }
New-Item -ItemType Directory -Force -Path (Split-Path $build) | Out-Null

$mesonArgs = @(
    'setup', $build, "$root/external/pixman",
    '--buildtype=release',
    '--default-library=shared',
    '-Dtests=disabled',
    '-Ddemos=disabled',
    '-Dgtk=disabled',
    '-Dlibpng=disabled',
    '-Dopenmp=disabled'
)

if ($targetArch -eq 'arm64') {
    # pixman's meson enables the x86 MMX/SSE paths for any MSVC compiler, and the
    # arm64 cl rejects <mmintrin.h>; its A64 NEON path is GNU assembler only.
    $mesonArgs += @('-Dmmx=disabled', '-Dsse2=disabled', '-Dssse3=disabled', '-Da64-neon=disabled')
}

if ($targetArch -ne $hostArch) {
    $cross = "$build.ini"
    @"
[binaries]
c = 'cl'
cpp = 'cl'
ar = 'lib'
windres = 'rc'
strip = 'echo'

[properties]
needs_exe_wrapper = true

[host_machine]
system = 'windows'
cpu_family = 'aarch64'
cpu = 'arm64'
endian = 'little'
"@ | Set-Content -Path $cross -Encoding ascii
    $mesonArgs += @('--cross-file', $cross)
}

meson @mesonArgs
if ($LASTEXITCODE -ne 0) { throw 'meson setup failed' }

ninja -C $build
if ($LASTEXITCODE -ne 0) { throw 'ninja failed' }

New-Item -ItemType Directory -Force -Path $out | Out-Null
Get-ChildItem -Path $out -Filter *.dll -ErrorAction SilentlyContinue | Remove-Item -Force

$dll = Get-ChildItem -Path $build -Recurse -Filter 'pixman-1*.dll' | Select-Object -First 1
if (-not $dll) { throw "meson built no pixman-1 dll in $build." }

Copy-Item $dll.FullName (Join-Path $out 'pixman-1.dll')

# Confirm the PE machine type matches the requested RID.
$bytes = [System.IO.File]::ReadAllBytes((Join-Path $out 'pixman-1.dll'))
$peOffset = [BitConverter]::ToInt32($bytes, 0x3C)
$machine = [BitConverter]::ToUInt16($bytes, $peOffset + 4)
$expected = if ($targetArch -eq 'arm64') { 0xAA64 } else { 0x8664 }
if ($machine -ne $expected) { throw ("pixman-1.dll machine type is 0x{0:X4}, expected 0x{1:X4} for {2}." -f $machine, $expected, $Rid) }

Get-ChildItem $out
