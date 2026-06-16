@echo off
cd /d C:\Apps\fatimatbm
echo Checking docx package...
node -e "require('docx')" 2>nul
if errorlevel 1 (
  echo Installing docx globally...
  npm install -g docx
)
echo Generating README.docx...
node create-readme.js
echo.
if exist README.docx (
  echo SUCCESS: README.docx created at C:\Apps\fatimatbm\README.docx
) else (
  echo ERROR: README.docx was not created. Check output above.
)
pause
