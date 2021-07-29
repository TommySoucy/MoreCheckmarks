# Hideout Requirement Indicator
A mod for JustEmuTarkov that show a colored checkmark on items needed to upgrade hideout modules.

## Installation

1. Download latest from [releases](https://github.com/TommySoucy/HideoutRequirementIndicator/releases)
2. Download the latest MelonLoader installer from [here](https://github.com/LavaGang/MelonLoader/releases) and install version **0.3.0** into your JET installation
3. Put HideoutRequirementIndicator.dll in the Mods folder created by MelonLoader

## Building

1. Clone repo
2. Open solution
3. Ensure all references are there
4. Build
5. Find built dll and open it in dnSpy
6. Right click and "Edit IL Code" on lines with "__instance.ShowGameObject(false);"
7. On the left of these line should be "callvirt", click on that and change it to "call"
8. Save module. DLL is now ready for install as explained in **Installation** section
