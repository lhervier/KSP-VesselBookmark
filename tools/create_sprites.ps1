# Generates the footer sprite PNGs from Unicode glyphs.
#
# Each icon is rendered as a white glyph on a transparent background,
# 32x32 px, into GameData/VesselBookmarkMod/Textures/. See the README
# in that folder for the meaning of each icon.
#
# Run from anywhere: powershell -ExecutionPolicy Bypass -File tools\create_sprites.ps1

Add-Type -AssemblyName System.Drawing

# Output size (px). The README asks for square ~32-64 px sprites; 32 is enough
# since they are displayed inline at line height.
$size = 32

# filename -> glyph to render (see Textures/README.txt)
$icons = [ordered]@{
    'edit.png'   = [char]0x270E  # pencil   -> "Edit comment"
    'goto.png'   = [char]0x27A4  # arrow    -> "Go to vessel"
    'target.png' = [char]0x25CE  # bullseye -> "Set as target"
}

# Resolve the Textures directory relative to this script, independent of the CWD.
$texturesDir = Join-Path $PSScriptRoot '..\GameData\VesselBookmarkMod\Textures'
$texturesDir = [System.IO.Path]::GetFullPath($texturesDir)

# This font ships with Windows and covers the symbol glyphs used above.
$fontFamily = 'Segoe UI Symbol'
# Leave a small margin so glyphs are not clipped at the edges.
$font = New-Object System.Drawing.Font($fontFamily, ($size * 0.6), [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)

$format = New-Object System.Drawing.StringFormat
$format.Alignment = [System.Drawing.StringAlignment]::Center
$format.LineAlignment = [System.Drawing.StringAlignment]::Center

$brush = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::White)
$rect = New-Object System.Drawing.RectangleF(0, 0, $size, $size)

foreach ($name in $icons.Keys) {
    $glyph = $icons[$name]

    $bmp = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    try {
        # Transparent background + smooth glyph edges.
        $g.Clear([System.Drawing.Color]::Transparent)
        $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit

        $g.DrawString([string]$glyph, $font, $brush, $rect, $format)

        $path = Join-Path $texturesDir $name
        $bmp.Save($path, [System.Drawing.Imaging.ImageFormat]::Png)
        Write-Host "Wrote $path"
    } finally {
        $g.Dispose()
        $bmp.Dispose()
    }
}

$brush.Dispose()
$font.Dispose()
$format.Dispose()
