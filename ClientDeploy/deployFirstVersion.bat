@ECHO OFF

ECHO COPYING APPLICATION

rd /s deploy-%1\1.0.0.0
mkdir deploy-%1\1.0.0.0
mkdir "deploy-%1\1.0.0.0\Application Files"
mkdir "deploy-%1\1.0.0.0\Application Files\pt-br"
copy ..\bin\*.dll "deploy-%1\1.0.0.0\Application Files"
copy ..\bin\ScrumFactory.Windows.exe "deploy-%1\1.0.0.0\Application Files"
copy ..\bin\ScrumFactory.Windows.exe.config "deploy-%1\1.0.0.0\Application Files"
copy ..\bin\pt-br\*.dll "deploy-%1\1.0.0.0\Application Files\pt-br"
copy ..\bin\SpellDics\*.* "deploy-%1\%2\Application Files\SpellDics"

copy factory.ico "deploy-%1\1.0.0.0\Application Files"

ECHO CREATING APP MANIFEST
mage -New Application -Processor x86 -ToFile "deploy-%1\1.0.0.0\Application Files\ScrumFactory.exe.manifest" -name "Scrum Factory" -Version 1.0.0.0 -FromDirectory "deploy-%1\1.0.0.0\Application Files" -if factory.ico

ECHO SIGNING APP MANIFEST
mage -Sign "deploy-%1\1.0.0.0\Application Files\ScrumFactory.exe.manifest" -CertFile ScrumFactory.Windows_TemporaryKey.pfx

ECHO CREATING DEPLOY MANIFEST
mage -New Deployment -Processor x86 -Install true -Publisher "Bad Habit" -ProviderUrl "http://%1/SFClient2012/ScrumFactory.application" -AppManifest "deploy-%1\1.0.0.0\Application Files\ScrumFactory.exe.manifest" -ToFile deploy-%1\ScrumFactory.application -name "Scrum Factory"


ECHO PLEASE ADD mapFileExtensions="true" trustURLParameters="true" co.v1:createDesktopShortcut="true" AT deployment TAG

pause

ECHO SIGNING DEPLOY MANIFEST
mage -Sign "deploy-%1\ScrumFactory.application" -CertFile ScrumFactory.Windows_TemporaryKey.pfx

ECHO ADDING .deploy extension
rename "deploy-%1\1.0.0.0\Application Files\*.*" *.*.deploy
rename "deploy-%1\1.0.0.0\Application Files\ScrumFactory.exe.manifest.deploy" ScrumFactory.exe.manifest
rename "deploy-%1\1.0.0.0\Application Files\pt-br\*.*" *.*.deploy
rename "deploy-%1\%2\Application Files\SpellDics\*.*" *.*.deploy