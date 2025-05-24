@echo off
setlocal

rem Путь к OpenCover
set OPENCOVER=C:\Users\alisa\source\repos\UchetRashodov\OpenCover\OpenCover.Console.exe

rem Путь к ReportGenerator
set REPORTGEN=C:\Users\alisa\source\repos\UchetRashodov\ReportGenerator\net47\ReportGenerator.exe

rem Путь к тестовому DLL
set TESTDLL=C:\Users\alisa\source\repos\UchetRashodov\UnitTestProject\bin\Debug\UnitTestProject.dll

rem Путь к результату покрытия
set COVERAGE_XML=coverage.xml

rem Путь к MSTest (Visual Studio 2022)
set VSTEST="C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe"

%OPENCOVER% -target:%VSTEST% ^
    -targetargs:"%TESTDLL%" ^
    -register:user ^
    -output:%COVERAGE_XML% ^
    -filter:"+[BusinessLogic*]* -[UnitTestProject*]*"

rem Генерация HTML-отчёта
"%REPORTGEN%" -reports:%COVERAGE_XML% -targetdir:CoverageReport

echo.
echo Отчёт создан в папке CoverageReport
pause
