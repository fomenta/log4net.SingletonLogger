@ECHO OFF
SETLOCAL

@REM ************************************************************************************************
REM Verificar parametros de entrada
@REM ************************************************************************************************
SET MYSCRIPT=%~n0

REM se ubica el folder del bat como directorio actual
PUSHD "%~dp0"
@REM ------------------------------------------------------------------------------------------------



@REM ************************************************************************************************
REM Se asegura que se ejecute como administrador
REM http://goo.gl/TxrpsK
@REM ************************************************************************************************
NET SESSION >nul 2>&1
IF ERRORLEVEL 1 (
   ECHO ######## ########  ########   #######  ########  
   ECHO ##       ##     ## ##     ## ##     ## ##     ## 
   ECHO ##       ##     ## ##     ## ##     ## ##     ## 
   ECHO ######   ########  ########  ##     ## ########  
   ECHO ##       ##   ##   ##   ##   ##     ## ##   ##   
   ECHO ##       ##    ##  ##    ##  ##     ## ##    ##  
   ECHO ######## ##     ## ##     ##  #######  ##     ## 
   ECHO.
   ECHO.
   ECHO ####### ERROR: ADMINISTRATOR PRIVILEGES REQUIRED #########
   ECHO This script must be run as administrator to work properly!  
   ECHO If you're seeing this after clicking on a start menu icon, then right click on the shortcut and select "Run As Administrator".
   ECHO ##########################################################
   ECHO.
   GOTO ERROR
)
@REM ------------------------------------------------------------------------------------------------



@REM ************************************************************************************************
REM ejecutando script
@REM ************************************************************************************************
PowerShell -NoProfile -ExecutionPolicy Bypass -File "%MYSCRIPT%.ps1"
IF ERRORLEVEL 1 GOTO ERROR
@REM ------------------------------------------------------------------------------------------------



GOTO :EOF
:ERROR
PAUSE

ENDLOCAL
