@echo off
setlocal enabledelayedexpansion

:: ■■■ 設定 ■■■
:: 処理対象のフォルダ名
set "INPUT_DIR=Textures"
:: 出力先のフォルダ名
set "OUTPUT_DIR=Textures"
:: ■■■ 設定ここまで ■■■

echo.
echo =======================================================
echo. ImageMagick アルファチャンネル前処理（透明部分を黒に）
echo.
echo. 処理対象フォルダ: %INPUT_DIR%
echo. 出力先フォルダ:   %OUTPUT_DIR%
echo =======================================================
echo.

:: バッチファイルがあるフォルダのパスを取得
set "BATCH_DIR=%~dp0"
:: INPUT_DIRのフルパスを取得しておく
set "INPUT_FULL_PATH=%BATCH_DIR%%INPUT_DIR%"
:: _outputフォルダのフルパスを作成
set "OUTPUT_FULL_PATH=%BATCH_DIR%%OUTPUT_DIR%"

:: _outputフォルダが存在しない場合は作成
if not exist "%OUTPUT_FULL_PATH%" (
    mkdir "%OUTPUT_FULL_PATH%"
    echo. 出力フォルダを作成しました: "%OUTPUT_FULL_PATH%"
)

:: /R オプションで INPUT_DIR 以下の全てのPNGファイルを再帰的に処理
for /R "%INPUT_FULL_PATH%" %%i in (*.png) do (

    :: 入力ファイルの相対パスを取得（フルパスから入力フォルダパスを除く）
    set "REL_PATH=%%i"
    set "REL_PATH=!REL_PATH:%INPUT_FULL_PATH%\=!"

    :: 出力ファイルのフルパスを組み立て
    set "OUT_PATH=%OUTPUT_FULL_PATH%\!REL_PATH!"

    :: 出力先ディレクトリがなければ作成
    for %%d in ("!OUT_PATH!") do (
        if not exist "%%~dpd" (
            mkdir "%%~dpd"
        )
    )

    echo 変換中: "!REL_PATH!"
    magick "%%i" ^( +clone -alpha extract -threshold 0 -negate -fill black -colorize 100% ^) -compose Dst_In -composite "!OUT_PATH!"


    if errorlevel 1 (
        echo エラー: %%~nxi
    )
)

echo.
echo =======================================================
echo. 全ての処理が完了しました。
echo =======================================================
pause
endlocal