REM -------========== MqttSnifferAndRelay.Core ==========-------

set folder=".\MqttSnifferAndRelay.Core\bin"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

set folder="..\obj"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

REM  -------========== MqttSnifferAndRelay.Main ==========-------

set folder="..\..\MqttSnifferAndRelay.Main\bin"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

set folder="..\obj"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

REM  -------========== MqttSnifferAndRelay.UI ==========-------

set folder="..\..\MqttSnifferAndRelay.UI\bin"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

set folder="..\obj"

cd /d %folder%

for /F "delims=" %%i in ('dir /b') do (rmdir "%%i" /s/q 2>NUL || del "%%i" /s/q >NUL )

pause
