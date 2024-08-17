# TMS Mod

This is a mod based on [BepInEx](https://github.com/BepInEx/BepInEx) for the steam game 
[Tavern Manager Simulator](https://store.steampowered.com/app/2756770/Tavern_Manager_Simulator/). Its features are more
what most people would consider cheats.

## Features
- Fires always have wood, they never stop burning.
- No resources needed to cook. No more buying supplies every few minutes.
- The washer station always has a full bucket of water.
- The fairy house automatically feeds the fairies. No reason to pay them in food anymore.
- The woodpile has an infinite supply of chopped wood.
- Horse water troughs and food troughs have infinite supplies.
- Thieves are very bad in their occupation and take 10 minutes instead of 25 seconds to rob you.
- Thieves are more talkative and have a speech bubble to draw attention.
- Any income from your customers is doubled.

### """Planned""" Features
Not sure if this will ever receive an update. But if it should, these would be future features:
- Automatically clean dirt.
- Display an incident at the top left for thieves like for other incidents
- Speed up fairies (including preventing them from stopping to eat, lazy fucks)

## Known Bugs
- Some quests might require you to fill something with some resource which is impossible if it has infinite supplies.

## Installation
1. Install the latest [BepInEx BleedingEdge build](https://builds.bepinex.dev/projects/bepinex_be).
   1. Download BepInEx-Unity.IL2CPP-*.zip for your operating system.
   2. Extract everything from that zip file into the game folder (`.../steamapps/common/TavernManagerSimlator/`).
2. Download the [latest release](https://github.com/Kraisie/TMS_Mods/releases) from this repo. 
3. Copy `Plugin.dll` to `BepinEx/plugins/` in the game folder. 
4. Start the game as normal.

## Compiling
Download the `Plugin.cs` file, open it in some IDE and let it create a project with a `.csproj` file. After running 
the game once with BepInEx installed import all `.dll` files from `BepinEx/interop` as libraries for the project. 
Compile with your IDE or run `dotnet build`.