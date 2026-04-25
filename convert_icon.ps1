Add-Type -AssemblyName System.Drawing
$iconPath = "c:\backup\Tony\Source\Repos\FinanceWidget\icon.png"
$bmp = New-Object System.Drawing.Bitmap($iconPath)

Write-Host "Analyzing $iconPath ($($bmp.Width)x$($bmp.Height))..."

# Find the bounding box with a VERY high threshold to ignore glow
$left = $bmp.Width
$top = $bmp.Height
$right = 0
$bottom = 0
$found = $false

for ($y = 0; $y -lt $bmp.Height; $y += 4) { # Faster scan
    for ($x = 0; $x -lt $bmp.Width; $x += 4) {
        $pixel = $bmp.GetPixel($x, $y)
        # High threshold (150) to focus on the bright part of the logo only
        if ($pixel.R -gt 150 -or $pixel.G -gt 150 -or $pixel.B -gt 150) {
            if ($x -lt $left) { $left = $x }
            if ($x -gt $right) { $right = $x }
            if ($y -lt $top) { $top = $y }
            if ($y -gt $bottom) { $bottom = $y }
            $found = $true
        }
    }
}

if (-not $found) {
    Write-Host "No bright content found! Lowering threshold to 100..."
    for ($y = 0; $y -lt $bmp.Height; $y += 4) {
        for ($x = 0; $x -lt $bmp.Width; $x += 4) {
            $pixel = $bmp.GetPixel($x, $y)
            if ($pixel.R -gt 100 -or $pixel.G -gt 100 -or $pixel.B -gt 100) {
                if ($x -lt $left) { $left = $x }
                if ($x -gt $right) { $right = $x }
                if ($y -lt $top) { $top = $y }
                if ($y -gt $bottom) { $bottom = $y }
                $found = $true
            }
        }
    }
}

Write-Host "Detected bounds: L:$left T:$top R:$right B:$bottom"

# ZERO padding for maximum size
$cropWidth = $right - $left + 1
$cropHeight = $bottom - $top + 1

# Create a square crop area to maintain aspect ratio
$size = [Math]::Max($cropWidth, $cropHeight)
$centerX = $left + ($cropWidth / 2)
$centerY = $top + ($cropHeight / 2)
$cropLeft = [Math]::Max(0, [int]($centerX - ($size / 2)))
$cropTop = [Math]::Max(0, [int]($centerY - ($size / 2)))

# Adjust if crop area goes out of bounds
if ($cropLeft + $size -gt $bmp.Width) { $cropLeft = $bmp.Width - $size }
if ($cropTop + $size -gt $bmp.Height) { $cropTop = $bmp.Height - $size }

Write-Host "Cropping to: $cropLeft, $cropTop, $size, $size"

# Crop and Resize
$cropRect = New-Object System.Drawing.Rectangle($cropLeft, $cropTop, $size, $size)
$cropped = $bmp.Clone($cropRect, $bmp.PixelFormat)
$bmp.Dispose() # Free the original file

# Save the cropped PNG back to icon.png
$cropped.Save("c:\backup\Tony\Source\Repos\FinanceWidget\icon_temp.png", [System.Drawing.Imaging.ImageFormat]::Png)
Move-Item "c:\backup\Tony\Source\Repos\FinanceWidget\icon_temp.png" $iconPath -Force

# Now create the ICO from the cropped version
$resized = New-Object System.Drawing.Bitmap($cropped, 64, 64)
$tempPng = "c:\backup\Tony\Source\Repos\FinanceWidget\temp_ico.png"
$resized.Save($tempPng, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = [System.IO.File]::ReadAllBytes($tempPng)
Remove-Item $tempPng

$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($ms)
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]1) # Header
$bw.Write([byte]64); $bw.Write([byte]64); $bw.Write([byte]0); $bw.Write([byte]0); $bw.Write([uint16]1); $bw.Write([uint16]32) # Entry
$bw.Write([uint32]$pngBytes.Length); $bw.Write([uint32]22) # Size/Offset
$bw.Write($pngBytes)

[System.IO.File]::WriteAllBytes("c:\backup\Tony\Source\Repos\FinanceWidget\icon.ico", $ms.ToArray())
$bw.Close()
$resized.Dispose()
$cropped.Dispose()
Write-Host "Update complete!"

