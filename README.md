# Nemetschek.AutoCad.LayersConvertor

LayersConvertor is a study example library implemented as AutoDesk-AutoCad plugin for AutoCad drawing layers conversion.

## LayersConvertor Instalation
* Must have AutoCad installed.
* Add AutoCad dll project reference to - accore.dll, acdbmgd.dll, acmgd.dll
* Call in AutoCad prompt - netload command and set reference to Nemetschek.AutoCad.LayersConvertor.dll
* In AutoCad prompt call "LCU" CommandMethod in order to start it.
* For debug -  Add process startup reference to acad.exe (AutoCad executable)
