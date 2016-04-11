@ECHO OFF

@ECHO deployUpdateVersion path version
@ECHO Ex: deployUpdateVersion myPath 9.99.99.99
@ECHO COPYING APPLICATION
rd /s deploy-%1\%2
mkdir deploy-%1\%2
mkdir "deploy-%1\%2\Application Files"
mkdir "deploy-%1\%2\Application Files\pt-br"
mkdir "deploy-%1\%2\Application Files\SpellDics"
copy ..\bin\*.dll "deploy-%1\%2\Application Files"

copy ..\bin\ScrumFactory.Windows.exe "deploy-%1\%2\Application Files"
copy ..\bin\ScrumFactory.Windows.exe.config "deploy-%1\%2\Application Files"
copy ..\bin\pt-br\*.dll "deploy-%1\%2\Application Files\pt-br"
copy .\SpellDics\*.* "deploy-%1\%2\Application Files\SpellDics"

copy factory.ico "deploy-%1\%2\Application Files"

ECHO CREATING APP MANIFEST
mage -New Application -Processor x86 -ToFile "deploy-%1\%2\Application Files\ScrumFactory.exe.manifest" -name "Scrum Factory" -Version %2 -FromDirectory "deploy-%1\%2\Application Files" -if factory.ico

ECHO SIGNING APP MANIFEST
mage -Sign "deploy-%1\%2\Application Files\ScrumFactory.exe.manifest" -CertFile ScrumFactory.Windows_TemporaryKey.pfx

ECHO CREATING DEPLOY MANIFEST
mage -Update "deploy-%1\ScrumFactory.application" -Version %2 -Publisher "Bad Habit" -AppManifest "deploy-%1\%2\Application Files\ScrumFactory.exe.manifest"

ECHO PLEASE REMOVE SUBSCRIPTION NODE BEFORE SIGNING

pause

ECHO SIGNING DEPLOY MANIFEST
mage -Sign "deploy-%1\ScrumFactory.application" -CertFile ScrumFactory.Windows_TemporaryKey.pfx

ECHO ADDING .deploy extension

ECHO COPY SHARP DLL NOW...it must be done after the manifest is signed
copy ..\bin\SharpPlink-Win32.svnExe "deploy-%1\%2\Application Files"

rename "deploy-%1\%2\Application Files\*.*" *.*.deploy
rename "deploy-%1\%2\Application Files\ScrumFactory.exe.manifest.deploy" ScrumFactory.exe.manifest
rename "deploy-%1\%2\Application Files\pt-br\*.*" *.*.deploy
rename "deploy-%1\%2\Application Files\SpellDics\*.*" *.*.deploy