# Adjustable Bandits

Adjusts bandit, looter, deserter, sea raider, and corsair party sizes and counts, plus hideout and mission limits. Supports Bannerlord 1.3.14 and War Sails 1.1.2 (optional).

## Requirements
- Harmony
- MCM (Mod Configuration Menu)
- War Sails: optional (only affects pirate clans; the mod runs fine without it)

## Logging
- Logging is opt-in (default off as of 1.2.8.3).
- Enable logging in MCM: Logs -> Enable Logging.
- Log file location:
  - %USERPROFILE%\Documents\Mount and Blade II Bannerlord\Configs\ModLogs\AdjustableBandits.log

## MCM Defaults

### Looters
- Looter Party Size Multiplier: 2.00
- Maximum Number of Looter Parties: 200

### Bandits
- Desert Bandit Party Size Multiplier: 0.60
- Maximum Number of Desert Bandit Parties: 5
- Steppe Bandit Party Size Multiplier: 0.60
- Maximum Number of Steppe Bandit Parties: 5
- Forest Bandit Party Size Multiplier: 0.60
- Maximum Number of Forest Bandit Parties: 5
- Bandit Party Size Multiplier (other bandits): 1.00
- Maximum Number of Bandit Parties (other bandits): 5
- Bandit Party Size Limit (affects Movement Speed): 20

### Deserters
- Deserter Party Size Multiplier: 1.00
- Maximum Number of Deserter Parties: 30

### Sea Raiders & Corsairs
- Sea Raider Party Size Multiplier: 0.60
- Maximum Number of Sea Raider Parties: 5
- Corsair Party Size Multiplier: 0.60
- Maximum Number of Corsair Parties: 5

### Hideout & Mission Settings
- Initial Hideouts per Faction: 3
- Maximum Hideouts per Faction: 10
- Minimum Parties to Infest Hideout: 2
- Maximum Parties around Hideout: 8
- Maximum Parties in Hideout: 4
- Maximum Troop Count in First Fight - Factor: 1.0
- Maximum Troop Count in Boss Fight - Factor: 1.0
- Spawn Percentage in First Fight: 75%
- Minimum Bandit Troops in Hideout Mission: 10
- Maximum Player Troops in Hideout Mission: 10

## Build and Deploy (VS Code)
- Debug task: .vscode/build-debug.ps1 (MSBuild restore + build).
- Release task: .vscode/build-release.ps1 (MSBuild restore + build, then deploys to your Bannerlord Modules folder).
- Optional env vars in .env:
  - MSBUILD_PATH: override MSBuild path.
  - MOD_ROOT or GAME_MODULE_DIR: override the deploy path for the module.

## Steam Workshop
- Update item: _Workshop/WorkshopUpdate.bat
- XML files:
  - _Workshop/WorkshopUpdate.xml

## Notes
- Workshop-installed mods are stored in steamapps/workshop/content/261550/<itemId>, not in the game Modules folder.
- If testing the Workshop version, temporarily move your local module out of Bannerlord/Modules.
