# Hideout Requirement Indicator

A mod for JustEmuTarkov that shows a colored checkmark on items needed to upgrade hideout modules.
Also shows a tooltip with the list of modules the item is needed for when cursor hovers over the checkmark.
If the item is also needed for a quest or is found in raid (so should already have a checkmark) it will have the different color but will still show found in raid or the quest in the tooltip.

- A **_GREEN_** checkmark means the item is needed in hideout but more is needed to fulfill the requirement of at least one of the modules you need it for.
- A **_BLUE_** checkmark means the item is needed in hideout and fulfills the required amount of all the modules that need it. Note that this does not mean you have enough to upgrade all of the modules that need it. Say if you need 2 wires to upgrade a module and 3 wires to upgrade another, and you have 3 wires in total inside your stash, the checkmark will be blue but if you upgrade one of the modules, you wont have enough to upgrade the other and the checkmark will go back to green. This though, can now be changed in the config using the blueAnyCanBeUpgraded setting. See config section below.
- The tooltip will show individual color for each module, blue or green, depending on whether you have enough of the item for the specific module.
- The colors can now be changed in the config. The ones above are default.

![alt text](https://github.com/TommySoucy/HideoutRequirementIndicator/blob/main/hub/example0.png "Green example")
![alt text](https://github.com/TommySoucy/HideoutRequirementIndicator/blob/main/hub/example1.png "Quest example")
![alt text](https://github.com/TommySoucy/HideoutRequirementIndicator/blob/main/hub/example2.png "Blue example")

This mod works great with [this one](https://github.com/JakeLoustone/HideoutShoppingList) which will let you see the exact count you need for each module.

## Installation

1. Download latest from [releases](https://github.com/TommySoucy/HideoutRequirementIndicator/releases)
2. Download the latest MelonLoader installer from [here](https://github.com/LavaGang/MelonLoader/releases) and install version **0.3.0** into your JET installation
3. Put HideoutRequirementIndicator.dll and the .txt file of the same name in the Mods folder created by MelonLoader

## Config

There are some settings available in the provided config file described below:

- **_blueAnyCanBeUpgraded_**: This setting will decide when to display a blue checkmark for a hideout required item. 
      **true** means to display blue when AT LEAST ONE of the hideout modules requiring this item can be upgraded with the amount of the item there is in stash
      **false** means to display blue when ALL of the hideout modules requiring this item can be upgraded
      
- **_prioritizeQuest_**: This setting will decide which color (yellow for quest or green/blue for hideout required) to display if the item is required for both a quest and a hideout upgrade
      **true** will prioritize quests, so if the item is needed for quests and hideout upgrade, the checkmark will be yellow
      **false** is opposite
      
- **_showLockedModules_**: This setting will decide whether to show a green/blue checkmark on an item required for a hideout module that is still locked for construction. This is an option because some modules like the bitcoin farm require a lot of items but are locked for a long time and maybe you just don't want to have green/blue checkmark on a bunch of random stuff you won't need for a while

- **_needMoreColor_** and **_fulfilledColor_**: These settings are used to changed the colors of the different checkmarks. needMoreColor is a light Green by default, appears when you still need more of that item. fulfilledColor is a light Blue by default, appears when you fulfilled the requirements of all modules that require it depending on blueAnyCanBeUpgraded setting. Please keep the format (value,value,value) (note the lack of spaces) since that is the one that the values will be parsed in. Otherwise the settings won't work. The values should always be in RGB in range [0 - 1]

## Building

1. Clone repo
2. Open solution
3. Ensure all references are there
4. Build
5. Find built dll and open it in dnSpy
6. Right click and "Edit IL Code" on the lines with "__instance.ShowGameObject(false);"
7. On the left of this line should be "callvirt", click on that and change it to "call"
8. Save module. DLL is now ready for install as explained in **Installation** section

## Used libraries

- Harmony
- MelonLoader
