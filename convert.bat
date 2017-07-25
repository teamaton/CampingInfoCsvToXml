@ECHO OFF

SET XML_FILE=%1
SET DATA_FILE=%~n2

IF NOT DEFINED XML_FILE (
    ECHO Bitte als erstes Argument die XML-Vorlage angeben.
)

IF NOT DEFINED DATA_FILE (
    ECHO Bitte als zweites Argument die CSV-Datei mit den Daten angeben.
    GOTO :EOF
)

CampingInfoCsvToXml.exe %XML_FILE% %DATA_FILE%.csv > convert.log

ECHO.
ECHO Erstellung der XML-Dateien ist abgeschlossen.
ECHO.

pause

@ECHO ON