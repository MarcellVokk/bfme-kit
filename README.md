# RegistryKit by Bfme Foundation
#### This project is part of the Bfme Foundation Project!
<a href="https://github.com/MarcellVokk/bfme-foundation-project">
    <img src="https://img.shields.io/badge/GitHub-Foundation Project-lime"/>
</a>

## Welcome
Welcome to the official github repository of RegistryKit!
This library allows you to easily modify the registry entries used by the BFME games. It can also create installation registries from scratch.

## Get on NuGet
<a href="https://www.nuget.org/packages/BfmeFoundationProject.RegistryKit">
   <img src="https://img.shields.io/nuget/v/BfmeFoundationProject.RegistryKit"/>
</a>

## Usage
- `BfmeRegistryManager.CreateNewInstallRegistry()` - Create new installation registry (install the game)
- `BfmeRegistryManager.GetKeyValue()` - Get a value from the game registry (get game language, installation directory, user data folder, etc...)
- `BfmeRegistryManager.SetKeyValue()` - Set a value in the games registry
- `BfmeRegistryManager.EnsureDefaults()` - Ensures that all necesary default values exist for the game to start without errors
- `BfmeRegistryManager.IsInstalled()` - Returns a bool indicating wether the game is installed or not.
- `BfmeRegistryManager.EnsureFixedRegistry()` - Manualy fixes the registry if it has been corrupted. Called every time when calling `BfmeRegistryManager.GetKeyValue()`.

###### Developed by The Online Battle Arena Team (Beterwel, MarcellVokk)
