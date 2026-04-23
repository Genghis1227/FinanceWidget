Add-Type -AssemblyName System.Drawing
$bmp = New-Object System.Drawing.Bitmap("c:\backup\Tony\Source\Repos\FinanceWidget\icon.png")
$resized = New-Object System.Drawing.Bitmap($bmp, 64, 64)
$tempPng = "c:\backup\Tony\Source\Repos\FinanceWidget\temp.png"
$resized.Save($tempPng, [System.Drawing.Imaging.ImageFormat]::Png)
$pngBytes = [System.IO.File]::ReadAllBytes($tempPng)
Remove-Item $tempPng

$ms = New-Object System.IO.MemoryStream
$bw = New-Object System.IO.BinaryWriter($ms)

# ICO header
$bw.Write([uint16]0) # Reserved
$bw.Write([uint16]1) # Type: 1 for icon
$bw.Write([uint16]1) # Count: 1 image

# Directory Entry
$bw.Write([byte]64)  # Width
$bw.Write([byte]64)  # Height
$bw.Write([byte]0)   # Color count
$bw.Write([byte]0)   # Reserved
$bw.Write([uint16]1) # Planes
$bw.Write([uint16]32) # BPP
$bw.Write([uint32]$pngBytes.Length) # Size
$bw.Write([uint32]22) # Offset

# Image Data
$bw.Write($pngBytes)

[System.IO.File]::WriteAllBytes("c:\backup\Tony\Source\Repos\FinanceWidget\icon.ico", $ms.ToArray())
$bw.Close()
$resized.Dispose()
$bmp.Dispose()

