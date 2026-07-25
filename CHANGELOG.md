# Changelog

## 1.3.3.0 - 2026-06-07
- Added compatibility for The Old Realms (TOR) bandit parties.
- Improved bandit classification to detect category from clan id, culture id, and default party template id.
- Added TOR greenskin bandit handling (`greenskin_bandits` / `greenskin_bandit`) so party scaling and limits apply correctly.
- Kept existing Adjustable Bandits settings behavior intact while extending support to TOR-specific bandit data.

## 1.3.2.0 - 2026-02-14
- Split hideout player troop cap into separate settings for Sneak-In and Assault missions.
- Updated hideout mission limits to enforce mission-specific minimums (Sneak-In: 20, Assault: 8) with max 100.
- Added backward-compatible defaults loading so legacy single hideout troop cap values migrate into both new settings.
- Completed MCM localization key coverage and cleanup for English and German, including key ordering to match MCM display sequence.
- Added Simplified Chinese localization under `ModuleData/Languages/CN` with language metadata and key parity.

## 1.3.1.0 - 2026-02-13
- Added grouped JSON defaults support (`ModuleData/adjustablebandits.defaults.json`) and MCM "Reset from JSON" action.
- Added guarded first-run auto-migration: when settings match legacy built-in defaults (e.g., minimum party counts of 150), JSON defaults are auto-applied.
- Reset applies defaults only when the button is clicked and saves immediately.
- Added guidance to reopen MCM after reset to view refreshed values.
- Added minimum party enforcement behavior and expanded separate pirate controls (Sea Raider, Northern Pirate, Southern Pirate).
- Improved bandit spawn/party handling and patch robustness.
- Cleaned unused/orphan files and reorganized source into simple folders (`Core`, `Config`, `Models`, `Behaviors`, `Utilities`).

## 1.2.8.3 - 2026-02-11
- Logging is now opt-in via MCM (default off).
