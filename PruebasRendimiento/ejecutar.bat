@echo off
REM ==========================================================
REM  BilleTico - Prueba de rendimiento en modo consola (sin GUI)
REM  Uso:  ejecutar.bat [usuarios] [duracion_seg] [rampup_seg] [pausa_min_ms] [pausa_rango_ms]
REM  Estres sin pausas:  ejecutar.bat 100 120 20 0 0
REM  Ej.:  ejecutar.bat 25 120 20
REM ==========================================================
cd /d "%~dp0"

REM Cambie esta ruta si descomprimio JMeter en otro lugar
if not defined JMETER_HOME set JMETER_HOME=C:\Users\josed\Desktop\apache-jmeter-5.6.3

set USUARIOS=%1
if "%USUARIOS%"=="" set USUARIOS=10
set DURACION=%2
if "%DURACION%"=="" set DURACION=60
set RAMPUP=%3
if "%RAMPUP%"=="" set RAMPUP=10
set THINKMIN=%4
if "%THINKMIN%"=="" set THINKMIN=500
set THINKRANGO=%5
if "%THINKRANGO%"=="" set THINKRANGO=1000

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set TS=%%i
set OUT=resultados\%TS%_%USUARIOS%usuarios
mkdir "%OUT%"

call "%JMETER_HOME%\bin\jmeter.bat" -n -t BilleTico_Rendimiento.jmx ^
  -Jusuarios=%USUARIOS% -Jduracion=%DURACION% -Jrampup=%RAMPUP% -JthinkMin=%THINKMIN% -JthinkRango=%THINKRANGO% ^
  -l "%OUT%\resultados.jtl" -j "%OUT%\jmeter.log" ^
  -e -o "%OUT%\reporte"

echo.
echo Reporte generado en %OUT%\reporte\index.html
start "" "%OUT%\reporte\index.html"
