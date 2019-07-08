Get-ChildItem . -Filter *.xml | 
Foreach-Object {
    $name = $_.BaseName
    echo ".\konverter.exe -t $name.xml -d $name.csv -i file:///X:/Thematica/kinderhotel.info/Hotelguide"
    & "$PSScriptRoot\konverter.exe" @("-t", "$name.xml", "-d", "$name.csv", "-i", "file:///X:/Thematica/kinderhotel.info/Hotelguide")
}