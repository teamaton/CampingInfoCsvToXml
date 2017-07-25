@ECHO OFF

SET XML_FILE=%1
SET DATA_FILE=%~n2

IF NOT DEFINED XML_FILE (
    ECHO Bitte als erstes Argument die XML-Vorlage angeben.
)

IF NOT DEFINED DATA_FILE (
    ECHO Bitte als zweites Argument die Excel-Datei mit den Daten angeben.
    GOTO :EOF
)

"C:\Program Files\LibreOffice 5\program\soffice.exe" --convert-to "csv:Text - txt - csv (StarCalc):59,,76,1" "%DATA_FILE%.xlsx"

copy CampingInfoCsvToXml\CampingInfoCsvToXml\bin\Debug\CampingInfoCsvToXml.exe .

echo on
CampingInfoCsvToXml.exe %XML_FILE% "%DATA_FILE%.csv" > convert.log
echo off

ECHO.
ECHO Erstellung der XML-Dateien ist abgeschlossen.
ECHO.

pause

start convert.log

@ECHO ON