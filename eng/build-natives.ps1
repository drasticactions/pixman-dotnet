#Requires -Version 7
[CmdletBinding()]
param(
    [ValidateSet('win-x64')]
    [string]$Rid = 'win-x64'
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot

if (-not (Test-Path "$root/external/pixman/meson.build")) {
    throw 'external/pixman is empty: run git submodule update --init.'
}

if (-not (Get-Command cl.exe -ErrorAction SilentlyContinue)) {
    $vswhere = "${env:ProgramFiles(x86)}/Microsoft Visual Studio/Installer/vswhere.exe"
    if (-not (Test-Path $vswhere)) { throw 'No cl.exe on PATH and no vswhere.exe to find Visual Studio.' }
    $vs = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath
    if (-not $vs) { throw 'No Visual Studio with the C++ x64 toolset is installed.' }
    Import-Module (Join-Path $vs 'Common7/Tools/Microsoft.VisualStudio.DevShell.dll')
    Enter-VsDevShell -VsInstallPath $vs -SkipAutomaticLocation -DevCmdArguments '-arch=x64 -host_arch=x64' | Out-Null
    if (-not (Get-Command cl.exe -ErrorAction SilentlyContinue)) { throw 'Enter-VsDevShell did not put cl.exe on PATH.' }
}

$build = Join-Path $root "artifacts/natives/$Rid"
$out = Join-Path $root "runtimes/$Rid/native"
if (Test-Path $build) { Remove-Item -Recurse -Force $build }

meson setup $build "$root/external/pixman" `
    --buildtype=release `
    --default-library=shared `
    -Dtests=disabled `
    -Ddemos=disabled `
    -Dgtk=disabled `
    -Dlibpng=disabled `
    -Dopenmp=disabled
if ($LASTEXITCODE -ne 0) { throw 'meson setup failed' }

ninja -C $build
if ($LASTEXITCODE -ne 0) { throw 'ninja failed' }

New-Item -ItemType Directory -Force -Path $out | Out-Null
Get-ChildItem -Path $out -Filter *.dll -ErrorAction SilentlyContinue | Remove-Item -Force

$dll = Get-ChildItem -Path $build -Recurse -Filter 'pixman-1*.dll' | Select-Object -First 1
if (-not $dll) { throw "meson built no pixman-1 dll in $build." }

Copy-Item $dll.FullName (Join-Path $out 'pixman-1.dll')
Get-ChildItem $out
