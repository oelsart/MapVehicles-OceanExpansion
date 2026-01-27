function Process-PngFile {
    param(
        [Parameter(ValueFromPipeline=$true)]
        [string]$filePath
    )
    Process{
		$fileNameWithoutExt = [System.IO.Path]::GetFileNameWithoutExtension($filePath)
		$directory = [System.IO.Path]::GetDirectoryName($filePath)

		# アルファチャンネル存在判定
		if ($(magick identify -format "%[channels]" $filePath) -match "a") {
			Write-Host "変換中: $($filePath | Split-Path -Leaf)" -ForegroundColor Green
			magick "$filePath" -background black -alpha background -channel RGB -fx "u*a" "$filePath"
		} else {
			Write-Host "スキップ: $($filePath | Split-Path -Leaf)"
		}
		
		if ($LASTEXITCODE -ne 0) {
			Write-Host "エラーが発生しました: $($filePath | Split-Path -Leaf)" -ForegroundColor Red
		}
	}
}

Write-Host "======================================================="
Write-Host "       ImageMagickで透明部分を黒にする"
Write-Host "======================================================="
Write-Host ""

### デフォルトでスクリプトと同じフォルダのTexturesフォルダの中身を探すことになっています。この機能が要らない場合次のif文を削除してください。
if ($args.Count -eq 0) {
    $DefaultPath = Join-Path -Path $PSScriptRoot -ChildPath "Textures"
    $args = @($DefaultPath)
}
### ここまで

foreach ($item in $args) {
    if (-not (Test-Path $item)) {
        Write-Host "エラー: パスが見つかりません: $item" -ForegroundColor Yellow
        continue
    }

    if (Test-Path $item -PathType Container) {
        Get-ChildItem -Path $item -Filter "*.png" -Recurse | Select-Object -ExpandProperty FullName | Process-PngFile
    }
    elseif (Test-Path $item -PathType Leaf) {
        $extension = [System.IO.Path]::GetExtension($item).ToLower()
        
        if ($extension -eq ".png") {
            Process-PngFile $item
        } else {
            Write-Host "スキップ: PNGファイルではありません: $item" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "======================================================="
Write-Host "       全ての処理が完了しました。"
Write-Host "======================================================="
Read-Host