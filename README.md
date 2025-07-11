﻿# The Other Them

This fork is based on TORGM `v3.5.4`, and it supports Among Us `v16.1.0` (also known as `v2025.6.10`).

[简体中文自述文件](README-CN.md)

## Disclaimer

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

## Roles

| Crewmates | Neutral | Impostors | Modifier | 
|----------|-------------|-----------------|----------------|
| [Pacifist](#pacifist) | [Idealist](#idealist) | [Innersloth](#innersloth) | Coming soon... |
| Coming soon... | [Phoenix](#phoenix) | Coming soon... | Coming soon... |

### Crewmates

#### Pacifist
The Pacifist can reset the kill cooldown of bad roles, but the ability has use limit.

- Ability icon by [JieGeLovesDengDuaLang](https://github.com/JieGeLovesDengDuaLang)

- Role idea by [YZ-华酱](https://space.bilibili.com/519835400)

##### Game Options
| Name | Description |
|----------|:-------------:|
| Pacifist Spawn Chance | -
| Ability Cooldown | -
| Ability Use Limit | -

### Neutral

#### Idealist
The Idealist can select a target to guess the target will be killed within certain time, or the Idealist will suicide. When the amount of guessed and killed players equals to the winning amount set by the host, the Idealist wins.

- Ability icon by [JieGeLovesDengDuaLang](https://github.com/JieGeLovesDengDuaLang)

- Role idea by [JieGeLovesDengDuaLang](https://github.com/JieGeLovesDengDuaLang)

##### Game Options
| Name | Description |
|----------|:-------------:|
| Idealist Spawn Chance | -
| Guessing Ability Cooldown | -
| Winning Guessed & Dead Target Count | The amount of players that the Idealist should guess correctly to win
| Suicide Countdown | The time that the Idealist's guessing target should be killed within, or the Idealist will suicide when time's up

#### Phoenix
The Phoenix needs to kill players. When the amount of killed players reaches the number set by host, the Phoenix wins. The Phoneix can possess a corpse to revive, and can only kill for 1 time after each revival.

- Ability icon by [JieGeLovesDengDuaLang](https://github.com/JieGeLovesDengDuaLang)

- Role idea by The spy

##### Game Options
| Name | Description |
|----------|:-------------:|
| Phoenix Spawn Chance | -
| Winning Killed Player Count | The amount of players that the Phoenix should guess correctly to win
| Possession to Revive Maximum Times | The maximum times that the Phoenix can revive by possessing after death
| Has Arrow to Corpses for Possession | Whether the Phoenix has arrows towards corpses for possessing to revive

### Impostors

#### Innersloth
The Innersloth can use the ability to sabotage, but the sabotage can make everyone lag while walking.

- Ability icon by [ADeerWhoLovesEveryone](https://github.com/ADeerWhoLovesEveryone)

- Role idea by [JieGeLovesDengDuaLang](https://github.com/JieGeLovesDengDuaLang)

##### Game Options
| Name | Description |
|----------|:-------------:|
| Idealist Spawn Chance | -
| Lagging Ability Cooldown | -

-------------------

**ORIGINAL README OF THIS FORK'S BASE REPOS BELOW**

-------------------

# The Other Roles: GM Edition 

This fork introduces a number of changes to [The Other Roles](https://github.com/Eisbison/TheOtherRoles).

* New Roles
  * [GM](#gm) by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)
  * [Madmate](#madmate) by [tomarai](https://github.com/tomarai)
  * [Opportunist](#opportunist) by [libhalt](https://twitter.com/libhalt)
  * [Ninja](#ninja) by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)
  * [Evil Swapper](#swapper) by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)
  * [Chain-Shifter](#shifter) by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)
  * [Plague Doctor](#plague-doctor) by [haoming37](https://github.com/haoming37)
  * [Serial Killer](#serial-killer) by [haoming37](https://github.com/haoming37)
  * [Neko-Kabocha](#neko-kabocha) by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)
  * [Fox & Immoralist](#fox) by [haoming37](https://github.com/haoming37)
  * [Fortune Teller](#serial-killer) by [haoming37](https://github.com/haoming37)
  * [Watcher](#watcher) by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)
* Pluralized Roles
  * Lovers (up to 7 couples)
  * Sheriff
  * And more...
* [Translation](#translation) into any language supported by Among Us
* Custom Hats
  * Almost 90 new hats drawn by members of the Japanese Among Us community

This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC. Portions of the materials contained herein are property of Innersloth LLC. © Innersloth LLC.

# The Other Roles

The **The Other Roles**, is a mod for [Among Us](https://store.steampowered.com/app/945360/Among_Us) which adds many new roles, new [Settings](#settings), and new [Custom Hats](#custom-hats) to the game.
Even more roles are coming soon :)

| Impostors | Crewmates | Neutral | Other | 
|----------|-------------|-----------------|----------------|
| [Evil Mini](#mini) | [Nice Mini](#mini) | [Arsonist](#arsonist) | [GM](#gm) |
| [Evil Guesser](#guesser) | [Nice Guesser](#guesser) | [Jester](#jester) |  
| [Bounty Hunter](#bounty-hunter) | [Detective](#detective) | [Jackal](#jackal) |  |
| [Camouflager](#camouflager) | [Engineer](#engineer) | [Sidekick](#sidekick) |  |
| [Cleaner](#cleaner) | [Hacker](#hacker) | [Lover](#lovers) |  |
| [Eraser](#eraser) | [Lighter](#lighter) | [Opportunist](#opportunist) |  |
| [Godfather (Mafia)](#mafia) | [Mayor](#mayor) | [Vulture](#vulture)  |  |
| [Mafioso (Mafia)](#mafia) | [Medic](#medic) | [Lawyer](#lawyer) |  |
| [Janitor (Mafia)](#mafia)  | [Security Guard](#security-guard) | [Chain-Shifter](#shifter) |  |
| [Morphling](#morphling) | [Seer](#seer) | [Plague Doctor](#plague-doctor) |  |
| [Trickster](#trickster) | [Sheriff](#sheriff) | [Fox & Immoralist](#fox) |  |
| [Vampire](#vampire) | [Shifter](#shifter) |  |  |
| [Warlock](#warlock) | [Snitch](#snitch) |  |  |
| [Witch](#witch) | [Spy](#spy) |  |  |
| [Ninja](#ninja) | [Nice Swapper](#swapper) |  |  |
| [Evil Swapper](#swapper) | [Time Master](#time-master) |  |  |
| [Serial Killer](#serial-killer) |  [Tracker](#tracker) |  |  |
| [Neko-Kabocha](#neko-kabocha) |  [Bait](#bait) |  |  |
| [Evil Watcher](#Watcher) |  [Madmate](#madmate)  |  |  |
|  |  [Medium](#medium) |  |  |
|  |  [Fortune Teller](#fortune-teller) |  |  |
|  |  [Nice Watcher](#Watcher) |  |  |

The [Role Assignment](#role-assignment) sections explains how the roles are being distributed among the players.

# Releases
| Among Us - Version | Mod Version | Link |
|----------|-------------|-----------------|
| 2022.2.24s | v3.5.4 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.5.4/TheOtherRoles-GM.v3.5.4.zip)
| 2022.2.24s | v3.5.3 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.5.3/TheOtherRoles-GM.v3.5.3.zip)
| 2022.2.8s | v3.5.2 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.5.2/TheOtherRoles-GM.v3.5.2.zip)
| 2021.12.15s (build num: 1421) | v3.5.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.5.1/TheOtherRoles-GM.v3.5.1.zip)
| 2021.12.15s (build num: 1421) | v3.5.0 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.5.0/TheOtherRoles-GM.v3.5.0.zip)
| 2021.12.15s (build num: 1421) | v3.4.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.4.1/TheOtherRoles-GM.v3.4.1.zip)
| 2021.12.15s (build num: 1421) | v3.4.0 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.4.0/TheOtherRoles-GM.v3.4.0.zip)
| 2021.12.15s (build num: 1421) | v3.3.3.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.3.3.1/TheOtherRoles-GM.v3.3.3.1.zip)
| 2021.12.15s (build num: 1421) | v3.3.2 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.3.2/TheOtherRoles-GM.v3.3.2.zip)
| 2021.12.15s (build num: 1421) | v3.3.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.3.1/TheOtherRoles-GM.v3.3.1.zip)
| 2021.12.15s (build num: 1421) | v3.3.0 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.3.0/TheOtherRoles-GM.v3.3.0.zip)
| 2021.12.15s (build num: 1421) | v3.2.6 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.2.6/TheOtherRoles-GM.v3.2.6.zip)
| 2021.12.14s (build num: 1402) | v3.2.5.3 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.2.5.3/TheOtherRoles-GM.v3.2.5.3.zip)
| 2021.11.9.5s | v3.2.5.2 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.2.5.2/TheOtherRoles-GM.v3.2.5.2.zip)
| 2021.11.9.5s | v3.2.5.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.2.5.1/TheOtherRoles-GM.v3.2.5.1.zip)
| 2021.11.9.5s | v3.2.5 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.2.5/TheOtherRoles-GM.v3.2.5.zip)
| 2021.11.9.5s | v3.2.4 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.2.4/TheOtherRoles-GM.v3.2.4.zip)
| 2021.11.9.5s | v3.2.3 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.2.3/TheOtherRolesGM.dll)
| 2021.11.9.5s | v3.2.2 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/3.2.2/TheOtherRoles-GM.v3.2.2.zip)
| 2021.11.9.5s | v3.1.2.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.1.2.1/TheOtherRoles-GM.v3.1.2.1.zip)
| 2021.11.9.5s | v3.1.2 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v3.1.2/TheOtherRoles-GM.v3.1.2.zip)
| 2021.6.30s | v2.9.2.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v2.9.2.1/TheOtherRoles-GM.v2.9.2.1.zip)
| 2021.6.30s | v2.9.2 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v2.9.2/TheOtherRoles-GM.v2.9.2.zip)
| 2021.6.30s | v2.9.0.1 GM | [Download](https://github.com/yukinogatari/TheOtherRoles-GM/releases/download/v2.9.0/TheOtherRoles-GM.v2.9.0.1.zip)

# Changelog
<details>
  <summary>Click to show the Changelog</summary>

**Version 3.4.3**
- Fixed a bug where "Guesser Is Impostor Chance" crashed the role system
- Fixed a bug where a sidekicked Hacker was stuck
- Fixed a bug where a sidekicked Security Guard was stuck
- Fixed a bug where a disabled Report Button triggered handcuffs
- Fixed a bug where the Evil Guesser spawn rate was not correct
- Changed that Cleaner & Vulture exclude each other
- Changed that the lighter/darker color indicator can be displayed as dead

**Version 3.4.2**
- Fixed a game breaking bug
  
**Version 3.4.2**
- Fixed a game breaking bug
  
**Version 3.4.1**
- Added a new mod option "Show Lighter/Darker" for meetings
- Added options for choosing which maps are enabled for random maps thanks [EvilScum](https://github.com/JustASysAdmin)
- Added Jester option "Jester Has Impostor Vision" thanks [EvilScum](https://github.com/JustASysAdmin)
- Fixed a bug where the Bounty Hunter had no bounty
- Fixed a bug where the Guesser & Sheriff were not assigned properly (fingers crossed)
- Fixed a bug where Hacker buttons didn't work as intended with "random map" option
- Fixed a bug where the Security Guard could not access cams on Skeld, dlekS & Airship
- Changed Tracker update intervall to a minimum of 1 thanks [LaicosVK](https://github.com/LaicosVK)

**Version 3.4.0**
- Added new Role [Deputy](#deputy) thanks [gendelo3](https://github.com/gendelo3)
- Added Hacker option "Cant Move During Mobile Gadget Duration"
- Added Security Guard mobile cams after placing all screws
- Added Lover option "Enable Lover Chat"
- Added return votes in meetings: You'll now get your votes back if your target got shot by the Guesser
- Added New Option for Guesser: Guesser can't guess Snitch if they has done all tasks (created by [MaximeGillot](https://github.com/MaximeGillot))
- Added The Other Roles changelog announcement popup
- Changed that the Bounty Hunter exclude their Lover
- Changed the position of the Witch icon in meetings for better visibility
- Fixed a bug where the spy had a white name for Impostors in chat
- Fixed a bug where the Guesser and Swapper UI in meetings was behind the visor cosmetics

**Version 3.3.3**
- Fixed a bug where a guessed Guesser could guess
- Fixed a bug where buttons were visible during the meeting
- Removed Hacker vitals for Skeld & dlekS
- Changed the Guesser option "Other Guesser Spawn Rate" to "Both Guesser Spawn Rate" (now only take effect when the chance for the first guesser was successful)
- Changed Hacker vitals to doorlog for MIRA HQ

**Version 3.3.2**
- Fixed a bug where you can't create a lobby on Among Us 2021.12.15

**Version 3.3.1**
- Fixed a bug where sometimes the Evil Guesser could not guess. Thanks @tomarai

**Version 3.3.0**
- Update to Among Us version 2021.12.14s
- Fixed a bug where the Pursuer won if the Pursuer was the last killed or voted player
- Fixed a bug where the option "Enable Mod Roles And Block Vanilla Roles" was not set correctly
- New option for the Guesser "Evil Guesser can guess spy"
- New option for the Guesser "Other Guesser Spawn Rate"
- New ability for the Hacker "Mobile Gadgets" (including vitals & admin table)
- New option for the Hacker "Max Mobile Gadget Charges"
- New option for the Hacker "Number Of Tasks Needed For Recharging"
- Fixed some UI bugs during the meeting  
  
**Version 3.2.5 (GM)**
- Allow hiding nameplates on an individual basis
- A failed Shifter is marked as a suicide now
- Fix for the Lawyer stealing a win when they were ejected
- Fix for players sometimes having their clothes/names swapped around
- Hide the kill button on the meeting screen
- Fix for non-Vampire/Warlock kills sometimes happening without a warp

**Version 3.2.4**
- Fixed a bug where the Vampire teleported when the bitten player died
- The settings UI has been improved by [Amsyar Rasyiq](https://github.com/amsyarasyiq)
- New option to the Bait "Warn The Killer With A Flash", created by [gendelo3](https://github.com/gendelo3)

**Version 3.2.3**
- Fixed a bug where the role of a dead client was visible to the Pursuer
- Fixed a bug where the Morphling changed their color when killing players
- Fixed a bug where voting the Lover partner of a Lover Witch did not safe the spellbound players
- When the Lawyer dies, the client doesn't have the client mark (§) anymore, making the client aware of the fact that the Lawyer can't steal the win anymore (only relevant if the "Client Knows" option is on)

**Version 3.2.2**
- Add new option "Play On A Random Map" created by [Alex2911](https://github.com/Alex2911)
- Add Witch option "Voting The Witch Saves All The Targets"
- Add Lawyer option "Lawyer Knows Target Role"
- We changed the win conditions of the [Lawyer](#lawyer), to make it more viable
- Bug fix: The Medium now shows the roles of players in the right format
- The name and the role of all winners is now being displayed on the end screen
- We changed the way settings are being shared among the players (which caused some people to be unable to join the lobby). This might resolve the problem or make it even worse... we'll see.

**Version 3.2.1**
- Hotfix for 3.2.0
- Bug fix: The Warlock is again able to kill with the curse abilty

**Version 3.2.0**
- **New Role:** [Witch](#witch) created by [Alex2911](https://github.com/Alex2911)
- **New Role:** [Lawyer](#lawyer)
- Bug fix: Choosing an Impostor as a Sidekick won't resulted in an Impostor/Sidekick mix anymore.
- Bug fix: The Guesser info now shows the right information, when the Guesser guesses the wrong role and kills himself.
- Bug fix: Hats are being displayed in alphabetic order. Hats demo in freeplay is working again. Fixed a bug where hats would not load when accessed from the main menu.
- Bug fix: The Detective now shows the name of the players in any case.

**Hotfix 3.1.2**
- Don't ask, just update. I messed up.

**Hotfix 3.1.1**
- Bug fix: You're again able to connect to custom servers
- Bug fix: The option "Guesses Visible In Ghost Chat" doesn't result in a ban of the Guesser anymore
- Bug fix: The position of the Spy on the intro screen is again random
- Bug fix: Re-added some venting rules that were lost (Spy can't move between vents, only Trickster can use boxes, ...)

**Version 3.1.0**
- Hopefully temporary fixing the issue of being kicked by Innersloth servers for regular kills, until Innersloth fixes it on their side.
- **NOTE:** Do not combine modded and unmodded versions of the game (even if you don't activate anything). Because of the kicking fix, your kills won't be performed for players that do not share the exact same modded version. Due to this you now can't start a game as the host, if not everyone in the lobby has the same version of the mod. Additionally you'll be kicked out of a lobby after 10 seconds, if the host doesn't have the mod installed (or the same mod version).
- **Tracker:** The Tracker has been reworked by [Alex2911](https://github.com/Alex2911). The Tracker now has an additional optional ability that tracks all corpses on the map for a few seconds.
- Add new option: Allow Parallel MedBay Scans
- Add new [Guesser](#guesser) option: "Guesses Visible In Ghost Chat"
- Add new [Guesser](#guesser) option: "Guesses Ignore The Medic Shield". If this option is set to false, no matter what the Guesser guessed, no one will die and the shielded player/Medic might be notified
- Add new [Medic](#medic) option: "Medic Sees Murder Attempt On Shielded Player". This includes attempts from any kind of killer (Sheriff, Jackal, Guesser if the shield is not being ignored, ...)
- During meetings the [Detective](#detective), [Hacker](#hacker) and [Medium](#medium) now display, whether a player wears a darker or lighter color
- Bug fix: Bounty Hunter, Mini and Engineer in vent kills do not result in players being kicked anymore
- Bug fix: The Trickster vent button now doesn't show the text "vent" twice anymore
- Bug fix: Fixed the visual bug where both Lovers always showed dead during the meeting after a correct guess of one of them even if the option "Both Lovers Die" was disabled

**Version 3.0.0**
- Updated to Among Us version v2021.11.9.5s
- **Note:** We wanted to update as fast as possible, that's why you can't use both the Innersloth and mod roles at the same time. We'll make that possible in the future, but there are various things that need to be modified (e.g. Shifter, Guesser, ...) to make that work, so that'll take a little longer. Also, be aware that this version might contain more bugs than usual because Innersloth changed a lot of things and we might have missed some of them.
- Ability buttons are now bind to the Q key (if it's a killing ability) or to the F key (otherwise). We'll make the binds adaptable in the future.
- For now we removed the option "Jester Can Sabotage"
- The Sheriff now always dies, when they try to kill a not fully grown Mini

**Hotfix 2.9.2.1**

- Fix for Morphing/Camoflauger's powers not properly ending when the timer ran out.

**Version 2.9.2**

- Merged from upstream
- **New Role:** [Medium](#medium)
- **New Role:** [Vulture](#vulture)
- Added Jackal Option: "Jackal Can See If Engineer Is In A Vent"
- Added Guesser Option: "Guesser Can Shoot Multiple Times Per Meeting"
- Fixed a bug that occured when the Shifter shifted the Bait
- Fixed a bug where [Medium](#medium) did not exlude the Evil [Mini](#mini)
- [Vulture](#vulture) "Number Of Corpses Needed To Be Eaten" max value extended to 12
- Added Vulture Option: "Show Arrows Pointing Towards The Corpses"

## v2.9.1

**New Features:**

  * Added an overlay to display current settings/role summary in-game (press H key to display)
  Japanese summaries by [Ao](https://twitter.com/aokunnnnn2525)
  * New Camouflager Option: Randomize Colors
  * New Guesser Option: Only Show Available Roles
  * New Sheriff Option: Misfire Kills Target
  * New GM Edition exclusive hats, 30 designs based on Japanese Among Us streamers and drawn by [Unohana Shiune](https://twitter.com/konken5)

**Major Changes:**

  * Opportunist is now handled as a Neutral role instead of a Crew role
    As a result, the Opportunist can now be killed by the Sheriff
  * Shifter now dies when attempting to steal Opportunist or Madmate (Opportunist is a Neutral role, and Madmate is technically a Crew role but effectively an Impostor role)
  * An erased Neutral role no longer results in the player having to do tasks
  * Adjusted Lovers quite a bit
    * Option "Can Win With Crew" replaced with "Counts as Separate Team"
    * When off, Lovers now properly behaves like old TOR (some fixes from the previous update left us inconsistent there)
  * Hide number of tasks completed during a Comms sabotage
  * Expanded the Impostors setting from 1-3 to 0-15
  * Extended all Kill Cooldowns to go down as far as 2.5s
  * More granular info on the results screen, such as Lovers who committed suicide or players who were torched by the Arsonist
  * Replace the On/Off special device restriction with a more granular time-based system (idea by [tomarai](https://github.com/tomarai))
    * Time limits are shared across the entire crew, and can be reset after each round if desired
    * Setting the time limit to 0s is the same as disabling the device entirely

**Bug Fixes:**

  * Briefly display a black screen in the period between the report animation and the meeting starting
  * Fix a bug with the original game where a kill happening *after* a meeting begins results in a corpse being left behind for some players
  * Fix a bug with the original game where the arrow for a task completed during a Comms sabotage stays behind
  * Don't highlight potential targets while inside a vent
  * Fix issues with the Snitch arrows sometimes showing the wrong color
  * The Security Guard's cameras now display their room name properly on Polus and Airship
  * Fix Airship showing up as Dleks on the options menu after every game
  * Results screen didn't display properly if the game ended during a Camouflage or Morph
  * Fix the UI getting lost if you open and then close the map while zoomed out
  * A task win cannot occur if there are zero players with tasks (ie. everyone is an Impostor or Neutral role)

**Hotfix 2.9.0.1**
- **New Role:** [Opportunist](#opportunist) (created by [libhalt](https://twitter.com/libhalt))
- Fixed a bug causing the Guesser to be unable to shoot
- Added support for a second repository for custom hats

**Version 2.9.0**
- **New Role:** [Madmate](#madmate) (created by [tomarai](https://github.com/tomarai))
- **New Role:** [GM](#gm) (created by [Virtual_Dusk](https://twitter.com/Virtual_Dusk))
- Added Lovers Options: "Lovers Can Win With Crew", "Lovers Tasks Are Counted"
- Added Sheriff Options: "Number of Shots"
- Changed [Mafia](#mafia) so that if only the Janitor and crew remain, the game automatically ends, as the Janitor is unable to kill.
- Completely overhauled how options are displayed on the lobby screen
- Post-game results now displays in a more legible table format
- Improved performance of Morphling/Camouflager abilities
- Options to disable admin, security, vitals, and vents
- Added Japanese translation (by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)) and support for adding [other languages](#translation).
- Numerous miscellaneous changes and bugfixes

**Hotfix 2.8.1**
- Fixed a game breaking bug where killing the Bait resulted in a ban of the Bait

**Version 2.8.0**
- **New Role:** [Bait](#bait)
- Added Tracker Option: "Tracker Reset Target After Meeting" (feature created by [MaximeGillot](https://github.com/MaximeGillot))
- Added Snitch Options: "Include Team Jackal" and "Use Different Arrow Color For Team Jackal"
- Added Medic Option: "Shield Will Be Set After Next Meeting"

**Version 2.7.3**
- Updated to Among Us v2021.6.30
- Updated BepInEx version
- Updated Credentials
- Fixed some Colors being considered darker, when they should be lighter
- Added /size command for Lobby
- Added /color and /murder command to Freeplay (for the Hat Designers)

**Version 2.7.1**
- Fixed a bug where [swapped](#swapper) votes were sometimes counted wrongly
- Fixed the positioning of the player name while [morphed](#morphling)
- Fixed a bug where the window of the [Guesser](#guesser) sometimes showed no "close button"
- Fixed a bug where the [garlics](#vampire) were not displayed properly

**Version 2.7.0**
- **New Role:** [Bounty Hunter](#bounty-hunter)
- Added more new [colors](#colors) (Thanks to [Drakoni](https://twitter.com/Drakoni13) for sorting them)
- Added a setting to the [Shifter](#shifter), that will prevent [Medic Shield](#medic) & [Lover](#lovers) Roles to be shifted
- Changed [Jackal](#jackal) & [Sidekick](#sidekick) to always be killable by [Sheriff](#sheriff)
- Changed [Jackal](#jackal) & [Sidekick](#sidekick) to not be [erasable](#eraser) anymore
- Changed [Role Assignment](#role-assignment) slightly to make chances more consistent
- Fixed a bug where votes would still count after the [Guesser](#guesser) or it's target died
- Fixed a bug where a [lover partner](#lovers) would not be shown as dead when killed by the [Guesser](#guesser)
- Fixed a bug on the Airship, where the [Jester](#jester) win was not triggered in some cases

**Version 2.6.7**
- **New Role:** [Guesser](#guesser)
- We changed the colors of some of our roles
- We renamed the Child to Mini
- Fixed a bug where a Jester win was triggered, when the partner of a Jester Lover was voted out
- Fixed a bug where a Mini lose was triggered, when the partner of a Crew Mini Lover was voted out

**Version 2.6.6**
- Fixed a bug introduced in v2.6.5 that caused all player to be able to use vents when the new option for spy was enabled

**Version 2.6.5**
- Added the ability to increase the number of tasks assigned to crewmates
- New option: A role summary in the end screen (Client option)
- **[Spy](#spy):** New option for spy to have the same vision as impostors
- **[Spy](#spy):** New option for spy to be able to jump into vents (but they can't move between them)
- Fixed a bug causing a crewmate task win when lovers were in the game even when not all crewmates had completed all their tasks
- Restored the original Among Us color for crewmates in the intro cutscene

**Version 2.6.4**
- **[Lovers](#lovers):** You can now select that Lovers may have a second role (could be a Crewmate, Neutral or Impostor role)
- **[Seer](#seer):** Fixed souls and flash sometimes not being visible (Thanks to [orangeNKeks](https://github.com/orangeNKeks))
- New option: [Swapper](#swapper) can only swap others
- New option: Ghosts can see votes
- New option: [Jackal](#jackal) and [Sidekick](#sidekick) have Impostor vision
- New option: [Jester](#jester) can sabotage
- Changed Freeplay mode to not assign custom roles anymore
- Fixed a bug with directional hats not using their flip image after a while

**Version 2.6.3**
- Changed the role limits options to allow for minimum and maximum bounds
- Changed the role assignment to be more random when assigning roles (previously assigned the neutral roles before assigning the crewmate roles)
- Added new `flip` option to [Custom Hats](#custom-hats)

**Version 2.6.2**
- The Other Roles now supports the new Among Us version **2021.5.10s**
- Added a chat command to kick players as the host of a lobby (`/kick playerName`)

**Version 2.6.1**
- Fixed a bug where the Sheriff was unable to kill the Arsonist
- Fixed a bug in the role assignment system
- Added the option to select the Dleks map
- Improved the overlay of the Arsonist

**Version 2.6.0**
- **New Role:** [Arsonist](#arsonist)
- Added an In-Game Updater, to make it easier to update the Mod
- Added synchronization for Airship toilet doors. Doors now open/close for everyone
- Changed Shifter to also die when shifting a neutral role (Jester, Arsonist, Jackal, ...)
- Changed the option "Jester Can Die To Sheriff" to "Neutrals Can Die To Sheriff"
- Changed the role assignment system. You can now set how many neutral roles you want in your game
- Changed Hacker to see colors more clearly on Admin Table
- Changed version handshake to give more clear info
- Fixed a problem with the Hat Tab leaving too much space between categories
- Fixed an Among Us bug, which made the selected region always show "North America"
- Fixed an Among Us bug, which made the disconnect info be off-screen. (hopefully)

**Version 2.5.1**
- **New Hats:** We added the support for custom hats and there are already a few hats inside the game. We can add new hats without updating the mod and we're awaiting your hat designs on our discord server.
- Changed Lovers to ignore Lover's Tasks for task win, while an ImpLover is alive
- Fixed a bug where garlic was not visible in some places
- The Security Guard can't place cameras on MiraHQ anymore
- Fixed a bug on the Airship, where the view of the cameras that the Security Guard placed wasn't centered on the camera.

**Version 2.5.0**
- **New Role:** [Security Guard](#security-guard)
- Fixed a bug where the game would stop after the first meeting
- Fixed a bug where killing with the hotkey Q ignored shields

**Version 2.4.0**
- **New Role:** [Warlock](#warlock)
- Added an option that allows ghosts to see the roles and remaining tasks of other players
- Added options to configure Morph & Camo duration
- Added hotkeys to the custom buttons (**Q** for the buttons that are on the same place as the kill button, **F** for the buttons that are above the kill button)
- Fixed an oversight which made StreamerMode only work as host
- Fixed an oversight which required Jackals to finish Tasks, after Sidekick was promoted
- Fixed an oversight which made Sidekicks not promote, if the Jackal disconnected
- Fixed a bug where the Trickster box was invisible
- Fixed a bug where changes to the server ip and port would only be applied if the game was restarted
- Added a way to get the 2 Hidden [Colors](#colors)

**Version 2.3.0**
- **New Role:** [Cleaner](#cleaner)
- Added 12 new [Colors](#colors)
- We added support for creating [Custom Hats](#custom-hats). New hats are coming with the next version, but you can already create and submit your own hats on [Discord](https://discord.gg/77RkMJHWsM).
- Added the option to hide the name of players with an unknown role
- Added Trickster Box vent animation. Thanks to [Drakoni](https://twitter.com/Drakoni13)
- You can now change the custom server ip/port right inside the game
- The Jackal, the Sidekick and the Jester now have fake tasks
- Added outlines, to show who you're targeting with your ability. Thanks to [Sihaack](https://github.com/sihaack) for part of the code.
- Added a streamer mode to Among Us, which hides lobby codes, the ip of your custom server and the port of your custom server. You can also modify the text that replaces the lobby code, check [Settings](#settings) for more details.
- Changed Meeting HUD Layout when playing with more than 10 players
- Fixed a bug where **ImpLovers** would hardly spawn
- Fixed a bug where players could get stuck on ladders/platforms when being rewound
- Fixed a bug where players could only use quickchat
- Fixed a bug which prevented to play in Freeplay mode
- Fixed a bug which moved the Ping info off-screen

**Version 2.2.2**
- Among Us version 2021.4.14s compatibility
- Improved the block votes on emergency meeting option

**Version 2.2.1**
- Trickster: The vent button now has a custom texture. Fixed a bug where the Trickster could clip out of bounds when their box was close to a wall.
- Fixed a bug where the Bad Mini's kill button went on cooldown when someone else performed a kill
- Fixed a few bugs with footprints, Seer souls and the Vampire delayed kill
- Fixed a bug where the Mini was banned for hacking (because of its reduced kill cooldown)
- Improved the version handshake

**Version 2.2.0**
- **Works with the latest Among Us version (2021.4.12s)**
- **Added support for 10+ player lobbies on custom servers:** Check the [Custom Servers and 10+ Players](#Custom-Servers-and-10+-Players) section. During meetings use the up/down keys, on vitals use the left/right keys.
- **Added a new Impostor role: The Trickster** check the [Trickster](#trickster) section for more info
- You can now set how long the Time Master shield lasts
- The host now sees for how long the lobby will remain open
- We changed the look/layout of the settings
- Added a new option that deactivates skipping in meetings (if the player does not vote, they vote themself)
- You can now choose whether the Eraser is able to erase the Spy/Impostors or not
- Fixed a bug where a Lovers win wasn't displayed properly
- Fixed the Among Us bug where people were unable to move after meetings
- We added a version checking system: The host can only start the game if everyone in their lobby has the same version of the mod installed (they will see, who is using a wrong version). This prevents hacking in public lobbies and bugs because of version mismatches.
- Fixed a bug where the Mini Impostor had the same cooldowns as normal Impostors
- Fixed a bug where the Vampire/Janitor/Mafioso would lose their kill button after being erased
- The Mini is now able to use ladders and it can do all the tasks right away

**Version 2.1.0**
- **New Role:** [Spy](#spy)
- **Eraser:** The Eraser can now also remove the role of other Impostors. This enables them to reveal the Spy, but might result in removing the special ability of their partner.
- **Camouflager:** The Mini age/size will now also be hidden, to allow the Mini Impostor to kill during camouflage

**Hotfix 2.0.1**
- Fixed a bug where camouflaged players would get stuck on ladders/platforms on the airship
- Introduced a one-second cooldown after the Morphling sampled another player
- The Mini can now always reach all usables (ladders, tasks, ...)
- We removed a bug, where some footprints remained on the ground forever
- We removed a bug, where the Detective didn't see the right color type when reporting a player
- We changed the Jester win and Mini lose conditions, they're not being affected by server delays anymore

**Changes in 2.0.0**
- **New button art** created by **Bavari**
- **New mod updater/installer tool** created by [Narua](https://github.com/Narua2010) and [Jolle](https://github.com/joelweih). Check the [Installation](#installation) section for more details.
- **Custom options:** Introduced customizable presets. Starting with 2.0.0, settings can be copied and used with higher versions (2.0.0).
- **Time Master rework:** Check [Time Master](#time-master) for more information
- **Medic:** The Medic report changed, it only shows the time since death (see Detective)
- **Detective:** The Detective now sees the name/color type of the killer when they report a dead body (ability moved from the Medic to the Detective)
- **Lighter:** We changed and tried to nerf the Lighter, see the [Lighter](#lighter) section for more details.
- **Seer:** As the role didn't work the way it was, we completely changed it. We're still working on the role, for now we're trying a few things. Check the [Seer](#seer) section to get more details about the new Seer.
- **Shifter:** We reworked the Shifter, they are now part of the crew. Check out the [Shifter](#shifter) sections for more details.
- **Hacker:** The Hacker is basically the old Spy. We added the option to only show the color type instead of the color on the admin table.
- **Camouflager:** Now also overrides the information of other roles, check the [Camouflager](#camouflager) section for more details.
- **Morphling:** Now also overrides the information of other roles, check the [Morphling](#morphling) section for more details
- **Mini:** The Mini can now be a Crewmate Mini or an Impostor Mini, check the [Mini](#mini) section for more details
- **Eraser:** The Eraser, a new Impostor role, is now part of the mod. Check the [Eraser](#eraser) section for more details
- **New options:**
  - You can now set the maximum number of meetings in a game: Every player still only has one meeting. The Mayor can always use their meeting (even if the maximum number of meetings was reached). Impostor/Jackal meetings also count.

**Hotfix 1.8.2**
- Add map and Impostor count to lobby settings.
- Fixed bugs where changing players to be the Sidekick didn't reset all the effects of their previous role.

**Hotfix 1.8.1** Resolves bugs that occurred when the Jackal recruited a Medic, Swapper and Tracker\
\
**Changes in v1.8:**
- **New Roles:** Added the Jackal and Sidekick roles
- Vampire: Medic report shows the right info now. A bitten Swapper is not able to swap if they die at the beginning of a meeting. One can now set the cooldown and whether a normal kill is possible when a target is next to a garlic or not.
- Lover: New option that sets how often an ImpLover appears. If a Lover is exiled, their partner doesn't spawn a dead body anymore.
- Cooldowns now stop cooling down, if a player sits inside a vent.
- Fixed a bug that prevented the game from continuing after a meeting (for an edge case with Lovers)
- If two players try to kill each other at the same time both should die (e.g. Sheriff vs Impostor)
- We added a description for your current role right above the task list
- Added a description for the [Role Assignment System](#role-assignment)

\
**Changes in v1.7:**
- **New Roles:** The Vampire, the Tracker and the Snitch are now in the game
- The role assignment system has been changed
- Impostors now see a blue outline around all vents of the map, if the Engineer sits inside one of them

\
**Changes in v1.6:**
- This update is a small hotfix, fixing the bug where some people were unable to join lobbies.
- The Mini can't be voted out anymore before it turns 18, hence games can't end anymore because the Mini died.
- Footprints are no longer visible to the Detective, if players are inside vents.

\
**Changes in v1.5:**
- Time Master - Buff: They are not affected by their rewind anymore, which gives them more utility. Players will now be rewound out of vents.
- Mini - Nerf: The Mini now grows up (see [Mini](#mini)) and becomes a normal Crewmate at some point. A growing Mini is not killable anymore. Some tasks are still not doable for the small Mini, we are working on that. But eventually when growing up it can do all the tasks as it's size increases.
- Seer - Nerf: Added an option that sets how often the Seer mistakes the player for another.
- Hacker - Nerf: The Hacker now only sees the additional information when they activate their "Hacker mode". That should stop the Hacker from camping the admin table/vitals.
- Other: Camouflager/Morphling cooldowns were fixed. Custom regions code was removed to enable 3rd party tools. Some minor bugfixes.

**Changes in v1.4:**
- Fixing a Camouflager/Morphling animation bug
- Fixing a bug where the Swapper could swap votes even if they are dead
- The custom cooldown buttons now render the cooldown progress (the grey overlay) in the right way (v1.3 introduced the bug)
- Players in vents are not targetable anymore by the role actions, the button does not activate (e.g. Seer revealing, Morphling sample). Exception: Impostor killing an Engineer in a vent

**Changes in v1.3:**
- Adds support for the Among Us version **2021.3.5s**
- Fixes a bug where an edge case caused all players to start the game with the camouflaged look
- There might be a few bugs, since I focused on getting the update out fast. A new version resolving the bugs will be published tomorrow.

**Changes in v1.1:**
- Morphling: The color of pet now also morphs. The skin animation now starts at the right point.
- The game over screen now shows if the Jester/Mini/Lovers won.
- A bug was removed where the Jester won together with the Crewmates.
- A bug was removed where the game of the Lovers crashed if they were the last players killed by the host of the lobby.
</details>



# Credits & Resources
[OxygenFilter](https://github.com/NuclearPowered/Reactor.OxygenFilter) - For all the version v2.3.0 to v2.6.1, we were using the OxygenFilter for automatic deobfuscation\
[Reactor](https://github.com/NuclearPowered/Reactor) - The framework used for all version before v2.0.0\
[BepInEx](https://github.com/BepInEx) - Used to hook game functions\
[Essentials](https://github.com/DorCoMaNdO/Reactor-Essentials) - Custom game options by **DorCoMaNdO**:
- Before v1.6: We used the default Essentials release
- v1.6-v1.8: We slightly changed the default Essentials. The changes can be found on this [branch](https://github.com/Eisbison/Reactor-Essentials/tree/feature/TheOtherRoles-Adaption) of our fork.
- v2.0.0 and later: As we're not using Reactor anymore, we are using our own implementation, inspired by the one from **DorCoMaNdO**

[Jackal and Sidekick](https://www.twitch.tv/dhalucard) - Original idea for the Jackal and Sidekick comes from **Dhalucard**\
[Among-Us-Love-Couple-Mod](https://github.com/Woodi-dev/Among-Us-Love-Couple-Mod) - Idea for the Lovers role comes from **Woodi-dev**\
[Jester](https://github.com/Maartii/Jester) - Idea for the Jester role comes from **Maartii**\
[ExtraRolesAmongUs](https://github.com/NotHunter101/ExtraRolesAmongUs) - Idea for the Engineer and Medic role comes from **NotHunter101**. Also some code snippets come of the implementation were used.\
[Among-Us-Sheriff-Mod](https://github.com/Woodi-dev/Among-Us-Sheriff-Mod) - Idea for the Sheriff role comes from **Woodi-dev**\
[TooManyRolesMods](https://github.com/Hardel-DW/TooManyRolesMods) - Idea for the Detective and Time Master roles comes from **Hardel-DW**. Also some code snippets of the implementation were used.\
[TownOfUs](https://github.com/slushiegoose/Town-Of-Us) - Idea for the Swapper, Shifter, Arsonist and a similar Mayor role come from **Slushiegoose**\
[Ottomated](https://twitter.com/ottomated_) - Idea for the Morphling, Snitch and Camouflager role come from **Ottomated**\
[Crowded-Mod](https://github.com/CrowdedMods/CrowdedMod) - Our implementation for 10+ player lobbies is inspired by the one from the **Crowded Mod Team**\
[Goose-Goose-Duck](https://store.steampowered.com/app/1568590/Goose_Goose_Duck) - Idea for the Vulture role come from **Slushygoose**

# Settings
The mod adds a few new settings to Among Us (in addition to the role settings):
- **Streamer Mode:** You can activate the streamer mode in the Among Us settings. It hides the lobby code, the custom server ip and the custom server port. You can set a custom lobby code replacement text, by changing the *Streamer Mode Replacement Text* in the `BepInEx\config\me.eisbison.theotherroles.cfg` file.
- **Number of Impostors:** The number of Impostor count be set inside a lobby
- **Map:** The map can be changed inside a lobby
- **Maximum Number Of Meetings:** You can set the maximum number of meetings that can be called in total (Every player still has personal maximum of buttons, but if the maximum number of meetings is reached you can't use your meetings even if you have some left. Impostor and Jackal meetings also count)
- **Allow Skips On Emergency Meetings:** If set to false, there will not be a skip button in emergency meetings. If a player does not vote, they'll vote themself.
- **Hide Player Names:** Hides the names of all players that have role which is unknown to you. Team Lovers/Impostors/Jackal still see the names of their teammates. Impostors can also see the name of the Spy and everyone can still see the age of the mini.
- **Allow Parallel MedBay Scans:** Allows players to perform their MedBay scans at the same time
- **Ghosts Can See Roles**
- **Ghosts Can See Votes**
- **Ghosts Can See The Number Of Remaining Tasks**
- **Dleks:** You are now able to select the Dleks map.
- **Task Counts:** You are now able to select more tasks.
- **Role Summary:** When a game ends there will be a list of all players and their roles and their task progress
- **Darker/Lighter:** Displays color type of each player in meetings

### Task Count Limits per map
You can configure:
- Up to 4 common tasks
- Up to 23 short tasks
- Up to 15 long tasks

Please note, that if the configured option exceeds the available number of tasks of a map, the tasks will be limited to that number of tasks. \
Example: If you configure 4 common tasks on Airship crewmates will only receive 2 common tasks, as airship doesn't offer more than 2 common tasks.

| Map | Common Tasks | Short Tasks | Long Tasks |
|----------|:-------------:|:-------------:|:-------------:|
| Skeld / Dleks | 2 | 19 | 8
| Mira HQ | 2 | 13 | 11
| Polus | 4 | 14 | 15
| Airship | 2 | 23 | 15
-----------------------


# Custom Hats
## Create and submit new hat designs
We're awaiting your creative hat designs and we'll integrate all the good ones in our mod.
Here are a few instructions, on how to create a custom hat:

- **Creation:** A hat consists of up to three textures. The aspect ratio of the textures has to be `4:5`, we recommend `300px:375px`:
  - `Main texture (required)`:
    - This is the main texture of your hat. It will usually be rendered in front of the player, if you set the `behind` parameter it will be rendered behind the player.
    - The name of the texture needs to follow the pattern *hatname.png*, but you can also set some additional parameters in the file name by adding `_parametername` to the file name (before the *.png*).
    - Parameter `bounce`: This parameter determines whether the hat will bounce while you're walking or not.
    - Parameter `adaptive`: If this parameter is set, the Among Us coloring shader will be applied (the shader that replaces some colors with the colors that your character is wearing in the game). The color red (#ff0000) will be replaced with the primary color of your player and the color blue (#0000ff) with the secondary color. Also other colors will be affected and changed, you can have a look at the texture of the [Crewmate Hat](https://static.wikia.nocookie.net/among-us-wiki/images/e/e0/Crewmate_hat.png) to see how this feature should be used.
    - Parameter `behind`: If this parameter is set, the main texture will be rendered behind the player.
  - `Flipped texture (optional)`:
    - This texture will be rendered instead of the Main texture, when facing the left.
    - The name of the texture needs to follow the pattern `hatname_flip.png`.
  - `Back texture (optional)`:
    - This texture will be rendered behind the player.
    - The name of the texture needs to follow the pattern `hatname_back.png`.
  - `Flipped Back texture (optional)`:
    - This texture will be rendered instead of the Back texture, when facing the left.
    - The name of the texture needs to follow the pattern `hatname_back_flip.png`.
  - `Climb texture (optional)`:
    - This texture will be rendered in front of the player, when they're climbing.
    - The name of the texture needs to follow the pattern `hatname_climb.png`.
- **Testing:** You can test your hat design by putting all the files in the `\TheOtherHats\Test` subfolder of your mod folder. Then whenever you start a Freeplay game, you and all the dummies will be wearing the new hat. You don't need to restart Among Us if you change the hat files, just exit and reenter the Freeplay mode.

- **Submission:** If you got a hat design, you can submit it on our [Discord server](https://discord.gg/77RkMJHWsM). We'll look at all the hats and add all the good ones to the game.

# Colors
![TOR Colors](./Images/TOR_colors.jpg)

# Roles

## Role Assignment
We are still improving the role assignment system. It's not that intuitive right now, but it's more flexible than the older one
if you're using it right.

First you need to choose how many special roles of each kind (Impostor/Neutral/Crewmate) you want in the game.
The count you set will only be reached, if there are enough Crewmates/Impostors in the game and if enough roles are set to be in the game (i.e. they are set to > 0%). The roles are then being distributed as follows:
- First all roles that are set to 100% are being assigned to arbitrary players.
- After that each role that has 10%-90% selected adds 1-9 tickets to a ticket pool (there exists a ticket pool for Crewmates, Neutrals and Impostors). Then the roles will be selected randomly from the pools as long it's possible (until the selected number is reached, until there are no more Crewmates/Impostors or until there are no more tickets). If a role is selected from the pool, obviously all the tickets of that role are being removed.
- The Mafia, Lovers and Mini are being selected independently (without using the ticket system) according to the spawn chance you selected. After that the Crewmate, Neutral and Impostor roles are selected and assigned in a random order.

**Example:**\
Settings: 2 special Crewmate roles, Snitch: 100%, Hacker: 10%, Tracker: 30%\
Result: Snitch is assigned, then one role out of the pool [Hacker, Tracker, Tracker, Tracker] is being selected\
Note: Changing the settings to Hacker: 20%, Tracker: 60% would statistically result in the same outcome.


## Mafia
### **Team: Impostors**
The Mafia are a group of three Impostors.\
The Godfather works like a normal Impostor.\
The Mafioso is an Impostor who cannot kill until the Godfather is dead.\
The Janitor is an Impostor who cannot kill, but they can hide dead bodies instead.\
\
**NOTE:**
- There have to be 3 Impostors activated for the mafia to spawn.
- If only the Janitor is left alive, the Mafia loses as there's no one left able to kill.

### Game Options
| Name | Description |
|----------|:-------------:|
| Mafia Spawn Chance | -
| Janitor Cooldown | -
-----------------------

## Morphling
### **Team: Impostors**
The Morphling is an Impostor which can additionally scan the appearance of a player. After an arbitrary time they can take on that appearance for 10s.
\
**NOTE:**
- They shrink to the size of the Mini when they copy its look.
- The Hacker sees the new color on the admin table.
- The color of the footprints changes accordingly (also the ones that were already on the ground).
- The other Impostor still sees that they are an Impostor (the name remains red).
- The shield indicator changes accordingly (the Morphling gains or loses the shield indicator).
- Tracker and Snitch arrows keep working.

### Game Options
| Name | Description |
|----------|:-------------:|
| Morphling Spawn Chance | -
| Morphling Cooldown | -
| Morph Duration | Time the Morphling stays morphed
-----------------------

## Camouflager
### **Team: Impostors**
The Camouflager is an Impostor which can additionally activate a camouflage mode.
The camouflage mode lasts for 10s and while it is active, all player names/pets/hats
are hidden and all players have the same color.\
\
**NOTE:**
- The Mini will look like all the other players
- The color of the footprints turns gray (also the ones that were already on the ground).
- The Hacker sees gray icons on the admin table
- The shield is not visible anymore
- Tracker and Snitch arrows keep working

### Game Options
| Name | Description |
|----------|:-------------:|
| Camouflager Spawn Chance | -
| Camouflager Cooldown | -
| Camo Duration | Time players stay camouflaged
-----------------------

## Vampire
### **Team: Impostors**
The Vampire is an Impostor, that can bite other player. Bitten players die after a configurable amount of time.\
If the Vampire spawn chance is greater 0 (even if there is no Vampire in the game), all players can place one garlic.\
If a victim is near a garlic, the "Bite Button" turns into the default "Kill Button" and the Vampire can only perform a normal kill.\
\
**NOTE:**
- If a bitten player is still alive when a meeting is being called, they die at the start of the meeting.
- The cooldown is the same as the default kill cooldown (+ the kill delay if the Vampire bites the target).
- If there is a Vampire in the game, there can't be a Warlock.

### Game Options
| Name | Description |
|----------|:-------------:|
| Vampire Spawn Chance | -
| Vampire Kill Delay | -
| Vampire Cooldown | Sets the kill/bite cooldown
| Vampire Can Kill Near Garlics | The Vampire can never bite when their victim is near a garlic. If this option is set to true, they can still perform a normal kill there.
-----------------------

## Eraser
### **Team: Impostors**
The Eraser is an Impostor that can erase the role of every player.\
The targeted players will lose their role after the meeting right before a player is exiled.\
After every erase, the cooldown increases by 10 seconds.\
The erase will be performed, even if the Eraser or their target die before the next meeting.\
By default the Eraser can erase everyone but the Spy and other Impostors. Depending on the options
they can also erase them (Impostors will lose their special Impostor ability).
\
**NOTE:**
- The Shifter shift will always be triggered before the Erase (hence either the new role of the Shifter will be erased or the Shifter saves the role of their target, depending on whom the Eraser erased)
- Erasing a Lover automatically erases the other Lover as well (if the second Lover is an ImpLover, they will turn into an Impostor)
- Erasing a Jackal that has a Sidekick, triggers the Sidekick promotion if it's activated in the settings
- As the erasing is being triggered before the eject of a player, erasing and voting out a Lover in the same round, would result in the
ex Lover surviving as the partnership was erased before. Also a Jester win would not happen, as the erase will be triggered before.

### Game Options
| Name | Description |
|----------|:-------------:|
| Eraser Spawn Chance | -
| Eraser Cooldown | The Eraser's cooldown will increase by 10 seconds after every erase.
| Eraser Can Erase Anyone | If set to false, they can't erase the Spy and other Impostors
-----------------------

## Trickster
### **Team: Impostors**
The Trickster is an Impostor that can place 3 jack-in-the-boxes that are invisible at first to other players. \
If the Trickster has placed all of their boxes they will be converted into a vent network usable only by the Trickster themself, but the boxes are revealed to the others. \
If the boxes are converted to a vent network, the Trickster gains a new ability "Lights out" to limit the visibility of Non-Impostors, that cannot be fixed by other players. Lights are automatically restored after a while.\

\
**NOTE:**
- Impostors will get a text indicator at the bottom of the screen to notify them if the lights are out due to the Trickster ability, as there is no sabotage arrows or task to sabotage text to otherwise notify them about it.

### Game Options
| Name | Description |
|----------|:-------------:|
| Trickster Spawn Chance | -
| Trickster Box Cooldown | Cooldown for placing jack-in-the-boxes
| Trickster Lights Out Cooldown | Cooldown for their "lights out" ability
| Trickster Lights Out Duration | Duration after which the light is automatically restored
-----------------------

## Cleaner
### **Team: Impostors**
The Cleaner is an Impostor who has the ability to clean up dead bodies.\

\
**NOTE:**
- The Kill and Clean cooldown are shared, preventing them from immediately cleaning their own kills.
- If there is a Cleaner in the game, there can't be a Vulture.

### Game Options
| Name | Description |
|----------|:-------------:|
| Cleaner Spawn Chance | -
| Cleaner Cooldown | Cooldown for cleaning dead bodies
-----------------------


## Warlock
### **Team: Impostors**
The Warlock is an Impostor, that can curse another player (the cursed player doesn't get notified).\
If the cursed person stands next to another player, the Warlock is able to kill that player (no matter how far away they are).\
Performing a kill with the help of a cursed player, will lift the curse and it will result in the Warlock being unable to move for a configurable amount of time.\
The Warlock can still perform normal kills, but the two buttons share the same cooldown.

\
**NOTE:**
- The Warlock can always kill their Impostor mates (and even themself) using the "cursed kill"
- If there is a Warlock in the game, there can't be a Vampire
- Performing a normal kill, doesn't lift the curse

### Game Options
| Name | Description |
|----------|:-------------:|
| Warlock Spawn Chance | -
| Warlock Cooldown | Cooldown for using the Curse and curse Kill
| Warlock Root Time | Time the Warlock is rooted in place after killing using the curse
-----------------------


## Bounty Hunter
### **Team: Impostors**
\
The Bounty Hunter is an Impostor, that continuously get bounties (the targeted player doesn't get notified).\
The target of the Bounty Hunter swaps after every meeting and after a configurable amount of time.\
If the Bounty Hunter kills their target, their kill cooldown will be a lot less than usual.\
Killing a player that's not their current target results in an increased kill cooldown.\
Depending on the options, there'll be an arrow pointing towards the current target.\
\
**NOTE:**
- The target won't be an Impostor, a Spy or the Bounty Hunter's Lover.
- Killing the target resets the timer and a new target will be selected.

### Game Options
| Name | Description |
|----------|:-------------:|
| Bounty Hunter Spawn Chance | -
| Duration After Which Bounty Changes | -
| Cooldown After Killing Bounty | -
| Additional Cooldown After Killing Others | Time will be added to the normal impostor cooldown if the Bounty Hunter kills a not-bounty player
| Show Arrow Pointing Towards The Bounty | If set to true an arrow will appear (only visiable for the Bounty Hunter)
| Bounty Hunter Arrow Update Interval | Sets how often the position is being updated

-----------------------


## Madmate
### **Team: Impostor**
TheOtherRoles implementation by [tomarai](https://github.com/tomarai)

The Madmate is a crewmate that works to support the impostors.
The concepted originally comes from Are You a Werewolf?, and the name for this role is taken from [au.libhalt.net's mod](https://au.libhalt.net/#madmate) (Japanese only).

1. Functionally, the Madmate is a crewmate role.
2. If the Impostors win, the Madmate wins as well.
3. The Madmate doesn't know who the Impostors are, and vice versa.
4. The Madmate doesn't have tasks.
5. The Madmate cannot fix lights.

### Game Options
| Name | Description
|----------|:-------------:|
| Madmate Spawn Chance | - |
| Madmate Can Die To Sheriff | Allows the Sheriff to kill the Madmate
| Madmate Can Enter Vents | Allow the Madmate to enter/exit vents
| Madmate Has Impostor Vision | Give the Madmate the same vision as the Impostors have
| Madmate Can Sabotage | Allow the Madmate to sabotage
| Madmate Can Fix Comm | Allow the Madmate to fix comms
-----------------------

## Witch
### **Team: Impostors**
The Witch is an Impostor who has the ability to cast a spell on other players.\
During the next meeting, the spellbound player will be highlighted and they'll die right after the meeting.\
There are multiple options listed down below with which you can configure to fit your taste.\
Similar to the Vampire, shields and blanks will be checked twice (at the end of casting the spell on the player and at the end of the meeting, when the spell will be activated).\
This can result in players being marked as spelled during the meeting, but not dying in the end (when they get a shield or the Witch gets blanked after they were spelled by the Witch).\
If the Witch dies before the meeting starts or if the Witch is being guessed during the meeting, the spellbound players will be highlighted but they'll survive in any case.\
Depending on the options you can choose whether voting the Witch out will save all the spellbound players or not.\

\
**NOTE:**
- The spellbound players will die before the voted player dies (which might trigger e.g. trigger an Impostor win condition, even if the Witch is the one being voted)


### Game Options
| Name | Description |
|----------|:-------------:|
| Witch Spawn Chance | -
| Witch Spell Casting Cooldown | -
| Witch Additional Cooldown | The spell casting cooldown will be increased by the amount you set here after each spell
| Witch Can Spell Everyone | If set to false, the witch can't spell the Spy and other Impostors
| Witch Spell Casting Duration | The time that you need to stay next to the target in order to cast a spell on it
| Trigger Both Cooldowns | If set to true, casting a spell will also trigger cooldown of the kill button and vice versa (but the two cooldowns may vary)
| Voting The Witch Saves All The Targets | If set to true, all the cursed targets will survive at the end of the meeting
-----------------------

## Ninja
### **Team: Impostors**
Created by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)
Original Idea by [うるさくてすみま船](https://twitter.com/nakanocchi2)

The Ninja is an Impostor that can turn invisible. While stealthed, the Ninja moves faster than a normal Crewmate, and kills don't cause them to warp. However, using their stealth ability increases their kill cooldown--a penalty for killing while invisible, and a short penalty applied after unstealthing.

## Serial Killer
### **Team: Impostors**
Created by [haoming37](https://github.com/haoming37)

The Serial Killer is an Impostor that has a reduced kill cooldown at the cost of their own life. Once the Serial Killer has their first taste of blood, they must kill again within a set time or be driven crazy by bloodlust and commit suicide.

## Neko-Kabocha
### **Team: Impostors**
Created by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)

The Neko-Kabocha is an Impostor capable of taking revenge on their killer. If killed by a Sheriff, Jackal, or other player, the killer will die alongside the Neko-Kabocha.

## Guesser
### **Team: Crewmates or Impostors**
The Guesser can be a Crewmate or an Impostor (depending on the settings).\
The Guesser can shoot players during the meeting, by guessing its role. If the guess is wrong, the Guesser dies instead.\
You can select how many players can be shot per game and if multiple players can be shot during a single meeting.\
The guesses Impostor and Crewmate are only right, if the player is part of the corresponding team and has no special role.\
You can only shoot during the voting time.\
Depending on the options, the Guesser can't guess the shielded player and depending on the Medic options the Medic/shielded player might be notified (no one will die, independently of what the Guesser guessed).\
\
**NOTE:**
- If a player gets shot, you'll get back your votes
- You can't guess the role **Nice Mini** for obvious reasons
- You can't guess the role **Lover**, you'll have to guess the primary role of one of the Lovers, to kill both of them
- Jester wins won't be triggered, if the Guesser shoots the Jester before the Jester gets voted out

### Game Options
| Name | Description |
|----------|:-------------:|
| Guesser Spawn Chance | -
| Chance That The Guesser Is An Impostor | -
| Guesser Number Of Shots Per Game | -
| Guesser Can Shoot Multiple Times Per Meeting |  -
| Guesses Visible In Ghost Chat | -
| Guesses Ignore The Medic Shield | -
| Evil Guesser Can Guess The Spy | -
| Both Guesser Spawn Rate | -
| Guesser Can't Guess Snitch When Tasks Completed | -
-----------------------

## Lovers
### **Team: Lovers (and secondary team)**
There are always two Lovers which are linked together.\
Their primary goal is it to stay alive together until the end of the game.\
If one Lover dies (and the option is activated), the other Lover suicides.\
You can select if Lovers are able to have a second role (could be a Neutral, Crewmate or Impostor Role)\
You can specify the chance of one Lover being an Impostor.\
The Lovers never know the role of their partner, they only see who their partner is.\
The Lovers win if they are both alive when the game ends. \
If there is no killer among the Lovers (e.g. an Arsonist Lover + Crewmate Lover) and they are both alive when the game ends, they can win together with the Crewmates.\
If there's a team Impostor/Jackal Lover in the game, the tasks of a Crewmate Lover won't be counted (for a task win) as long as they're alive. If the Lover dies, their tasks will also be counted.\
You can enable an exclusive chat only for Lovers\
\
**NOTE:**
- In a 2 Cremates vs 2 Impostors (or 2 members of team Jackal) and the Lovers are not in the same team, the game is not automatically over since the Lovers can still achieve a solo win. E.g. if there are the following roles Impostor + ImpLover + Lover + Crewmate left, the game will not end and the next kill will decide if the Impostors or Lovers win.
- The Lovers can change if the Shifter takes the role of a Lovers

### Game Options
| Name | Description |
|----------|:-------------:|
| Lovers Spawn Chance | -
| Chance That One Lover Is Impostor | -
| Both Lovers Die | Whether the second Lover suicides when the first one dies
| Lovers Can Have Another Role | If set to true, the Lovers can have a second role
| Lovers Can Win With Crew | When false, the Lovers are treated as a separate team. (True: original TheOtherRoles behavior.)
| Lovers Tasks Are Counted | Whether the Lovers' tasks count toward overall task completion.
| Enable Lover Chat | -
-----------------------


## Sheriff
### **Team: Crewmates**
The Sheriff has the ability to kill Impostors.
If they try to kill a Crewmate, they die instead.

**NOTE:**
- If the Sheriff shoots the person the Medic shielded, the Sheriff and the shielded person **both remain unharmed**.
- If the Sheriff shoots a Mini Impostor, the Sheriff dies if the Mini is still growing up. If it's 18, the Mini Impostor dies.

### Game Options
| Name | Description |
|----------|:-------------:|
| Sheriff Spawn Chance | -
| Sheriff Number of Shots | The number of times the Sheriff is able to kill
| Sheriff Cooldown | -
| Neutrals Can Die To Sheriff | -
-----------------------

## Jester
### **Team: Neutral**
The Jester does not have any tasks. They win the game as a solo, if they get voted out during a meeting.

### Game Options
| Name | Description |
|----------|:-------------:|
| Jester Spawn Chance | -
| Jester Can Call Emergency Meeting | Option to disable the emergency button for the Jester
-----------------------

## Arsonist
### **Team: Neutral**
The Arsonist does not have any tasks, they have to win the game as a solo.\
The Arsonist can douse other players by pressing the douse button and remaining next to the player for a few seconds.\
If the player that the Arsonist douses walks out of range, the cooldown will reset to 0.\
After dousing everyone alive the Arsonist can ignite all the players which results in an Arsonist win.

### Game Options
| Name | Description |
|----------|:-------------:|
| Arsonist Spawn Chance | -
| Arsonist Countdown | -
| Arsonist Douse Duration | The time it takes to douse a player
-----------------------

## Seer
### **Team: Crewmates**
The Seer has two abilities (one can activate one of them or both in the options).
The Seer sees the souls of players that died a round earlier, the souls slowly fade away.
The Seer gets a blue flash on their screen, if a player dies somewhere on the map.

### Game Options
| Name | Description |
|----------|:-------------:|
| Seer Spawn Chance | -
| Seer Mode | Options: Show death flash and souls, show death flash, show souls
| Seer Limit Soul Duration | Toggle if souls should turn invisible after a while
| Seer Soul Duration | Sets how long it will take the souls to turn invisible after a meeting
-----------------------

## Engineer
### **Team: Crewmates**
The Engineer (if alive) can fix a certain amount of sabotages per game from anywhere on the map.\
The Engineer can use vents.\
If the Engineer is inside a vent, depending on the options the members of the team Jackal/Impostors will see a blue outline around all vents on the map (in order to warn them).
Because of the vents the Engineer might not be able to start some tasks using the "Use" button, you can double-click on the tasks instead.\
\
**NOTE:**
- The kill button of Impostors activates if they stand next to a vent where the Engineer is. They can also kill them there. No other action (e.g. Morphling sample, Shifter shift, ...) can affect players inside vents.

### Game Options
| Name | Description |
|----------|:-------------:|
| Engineer Spawn Chance | -
| Number Of Sabotage Fixes| -
| Impostors See Vents Highlighted | -
| Jackal and Sidekick See Vents Highlighted | -
-----------------------

## Detective
### **Team: Crewmates**
The Detective can see footprints that other players leave behind.
The Detective's other feature shows when they report a corpse: they receive clues about the killer's identity. The type of information they get is based on the time it took them to find the corpse.
\
**NOTE:**
- When people change their colors (because of a morph or camouflage), all the footprints also change their colors (also the ones that were already on the ground). If the effects are over, all footprints switch back to the original color.
- The Detective does not see footprints of players that sit in vents
- More information about the [colors](#colors)
- During the meetings you can see, whether a player wears a darker or a lighter color, represented by (D) or (L) in the names.

### Game Options
| Name | Description |
|----------|:-------------:|
| Detective Spawn Chance | -
| Anonymous Footprints | If set to true, all footprints will have the same color. Otherwise they will have the color of the respective player.
| Footprint Interval | The interval between two footprints
| Footprint Duration | Sets how long the footprints remain visible.
| Time Where Detective Reports Will Have Name | The amount of time that the Detective will have to report the body since death to get the killer's name.  |
| Time Where Detective Reports Will Have Color Type| The amount of time that the Detective will have to report the body since death to get the killer's color type. |
-----------------------

## Lighter
### **Team: Crewmates**
The Lighter can turn on their Lighter every now and then, which increases their vision by a customizable amount.

### Game Options
| Name | Description |
|----------|:-------------:|
| Lighter Spawn Chance | -
| Lighter Mode Vision On Lights On | The vision the Lighter has when the lights are on and the Lighter mode is on
| Lighter Mode Vision On Lights Off | The vision the Lighter has when the lights are down and the Lighter mode is on
| Lighter Cooldown | -
| Lighter Duration | -
-----------------------

## Mini
### **Team: Crewmates or Impostors**
The Mini can be a Crewmate (67% chance) or an Impostor (33% chance).\
The Mini's character is smaller and hence visible to everyone in the game.\
The Mini cannot be killed until it turns 18 years old, however it can be voted out.\
**Impostor Mini:**
  - While growing up the kill cooldown is doubled. When it's fully grown up its kill cooldown is 2/3 of the default one.
  - If it gets thrown out of the ship, everything is fine.

**Crewmate Mini:**
  - The Crewmate Mini aims to play out the strength its invincibility in the early game.
  - If it gets thrown out of the ship before it turns 18, everyone loses. So think twice before you vote out a Mini.

**NOTE:**
- If the Sheriff tries to kill the Mini before it's fully grown, they die, no matter if the Mini is a Crewmate or Impostor
- The Sheriff can kill the Impostor Mini, but only if it's fully grown up

### Game Options
| Name | Description |
|----------|:-------------:|
| Mini Spawn Chance | -
| Mini  | Mini Growing Up Duration
-----------------------

## Medic
### **Team: Crewmates**
The Medic can shield (highlighted by an outline around the player) one player per game, which makes the player unkillable.\
The shielded player can still be voted out and might also be an Impostor.\
If set in the options, the shielded player and/or the Medic will get a red flash on their screen if someone (Impostor, Sheriff, ...) tried to murder them.
If the Medic dies, the shield disappears with them.\
The Sheriff will not die if they try to kill a shielded Crewmate and won't perform a kill if they try to kill a shielded Impostor.\
Depending on the options, guesses from the Guesser will be blocked by the shield and the shielded player/medic might be notified.\
The Medic's other feature shows when they report a corpse: they will see how long ago the player died.
\
**NOTE:**
- If the shielded player is a Lover and the other Lover dies, they nevertheless kill themselves.
- If the Shifter has a shield or their target has a Shield, the shielded player switches.
- Shields set after the next meeting, will be set before a possible shift is being performed.


### Game Options
| Name | Description | Options |
|----------|:-------------:|:-------------:|
| Medic Spawn Chance | - | -
| Show Shielded Player | Sets who sees if a player has a shield | "Everyone", "Shielded + Medic", "Medic"
| Shielded Player Sees Murder Attempt| Whether a shielded player sees if someone tries to kill them | True/false |
| Shield Will Be Set After Next Meeting | - | True/false
| Medic Sees Murder Attempt On Shielded Player | - | If anyone tries to harm the shielded player (Impostor, Sheriff, Guesser, ...), the Medic will see a red flash
-----------------------

## Mayor
### **Team: Crewmates**
The Mayor leads the Crewmates by having a vote that counts twice.\
The Mayor can always use their meeting, even if the maximum number of meetings was reached.

### Game Options
| Name | Description |
|----------|:-------------:|
| Mayor Spawn Chance | -
-----------------------

## Hacker
### **Team: Crewmates**
If the Hacker activates the "Hacker mode", the Hacker gets more information than others from the admin table and vitals for a set duration.\
Otherwise they see the same information as everyone else.
**Admin table:** The Hacker can see the colors (or color types) of the players on the table.\
**Vitals**: The Hacker can see how long dead players have been dead for.\
The Hacker can access his mobile gadgets (vitals & admin table), with a maximum of charges (uses) and a configurable amount of tasks needed to recharge.\
While accessing those mobile gadgets, the Hacker is not able to move.\
\
**NOTE:**
- If the Morphling morphs or the Camouflager camouflages, the colors on the admin table change accordingly
- More information about the [colors](#colors)
- During the meetings you can see, whether a player wears a darker or a lighter color, represented by (D) or (L) in the names.

### Game Options
| Name | Description |
|----------|:-------------:|
| Hacker Spawn Chance | -
| Hacker Cooldown | -
| Hacker Duration | Sets how long the "Hacker mode" remains active
| Hacker Only Sees Color Type | Sets if the Hacker sees the player colors on the admin table or only white/gray (for Lighter and darker colors)
| Max Mobile Gadget Charges | -
| Number Of Tasks Needed For Recharging | Number of tasks to get a charge
| Can't Move During Cam Duration | -
-----------------------


## Shifter
### **Team: Crewmates or Neutral**
The Shifter can take over the role of another Crewmate, the other player will transform into a Crewmate.\
The Shift will always be performed at the end of the next meeting right before a player is exiled. The target needs to be chosen during the round.\
Even if the Shifter or the target dies before the meeting, the Shift will still be performed.\
Swapping roles with an Impostor or Neutral fails and the Shifter commits suicide after the next meeting (there won't be any body).\
The Shifter aims to save roles from leaving the game, by e.g. taking over a Sheriff or Medic that is known to the Impostors.\
This works especially well against the Eraser, but also gives the Eraser the possibility to act like a Shifter.\
The **special interactions** with the Shifter are noted in the chapters of the respective roles.\

A Neutral version of the Shifter, known as the Chain-Shifter, is able to steal roles from any player, regardless of their team. The shifted player then inherits the role of Chain-Shifter, and must steal another player's role. The player who retains the Chain-Shifter role at the end of the game automatically loses.

**NOTE:**
- The Shifter shift will always be triggered before the Erase (hence either the new role of the Shifter will be erased or the Shifter saves the role of their target, depending on whom the Eraser erased)
- If the Shifter takes over a role, their new cooldowns will start at the maximum cooldown of the ability
- One time use abilities (e.g. shielding a player or Engineer sabotage fix) can only used by one player in the game (i.e. the Shifter
can only use them, if the previous player did not use them before)

### Game Options
| Name | Description
|----------|:-------------:|
| Shifter Spawn Chance | -
| Shifter Shifts Modifiers | Sets if Lovers and/or Medic Shield will be shifted
-----------------------

## Time Master
### **Team: Crewmates**
The Time Master has a time shield which they can activate. The time shield remains active for a configurable amount of time.\
If a player tries to kill the Time Master while the time shield is active, the kill won't happen and the
time will rewind for a set amount of time.\
The kill cooldown of the killer won't be reset, so the Time Master
has to make sure that the game won't result in the same situation.\
The Time Master won't be affected by the rewind.\
\
**NOTE:**
- Only the movement is affected by the rewind.
- A Vampire bite will trigger the rewind. If the Time Master misses shielding the bite, they can still shield the kill which happens a few seconds later.
- If the Time Master was bitten and has their shield active before when a meeting is called, they survive but the time won't be rewound.
- If the Time Master has a Medic shield, they won't rewind.
- The shield itself ends immediately when triggered. So the Time Master can be attacked again as soon as the rewind ends.

### Game Options
| Name | Description |
|----------|:-------------:|
| Time Master Spawn Chance | - |
| Time Master Cooldown | - |
| Rewind Duration | How much time to rewind |
| Time Master Shield Duration |
-----------------------

## Swapper
### **Team: Crewmates or Impostor**
During meetings the Swapper can exchange votes that two people get (i.e. all votes
that player A got will be given to player B and vice versa).\
Because of the Swapper's strength in meetings, they might not start emergency meetings
and can't fix lights and comms.

### Game Options
| Name | Description
|----------|:-------------:|
| Swapper Spawn Chance | -
| Swapper can call emergency meeting | Option to disable the emergency button for the Swapper
| Swapper can only swap others | Sets whether the Swapper can swap themself or not
-----------------------

## Tracker
### **Team: Crewmates**
The Tracker can select one player to track. Depending on the options the Tracker can track a different person after each meeting or the Tracker tracks the same person for the whole game.
An arrow points to the last tracked position of the player.
The arrow updates its position every few seconds (configurable).
Depending on the options, the Tracker has another ability: They can track all corpses on the map for a set amount of time. They will keep tracking corpses, even if they were cleaned or eaten by the Vulture.

### Game Options
| Name | Description
|----------|:-------------:|
| Tracker Spawn Chance | -
| Tracker Update Interval | Sets how often the position is being updated
| Tracker Reset Target After Meeting | -
| Tracker Can Track Corpses | -
| Corpses Tracking Cooldown | -
| Corpses Tracking Duration | -
-----------------------

## Snitch
### **Team: Crewmates**
When the Snitch finishes all the tasks, arrows will appear (only visible to the Snitch) that point to the Impostors (depending on the options also to members of team Jackal).
When the Snitch has one task left (configurable) the Snitch will be revealed to the Impostors (depending on the options also to members of team Jackal) with an arrow pointing to the Snitch.


### Game Options
| Name | Description
|----------|:-------------:|
| Snitch Spawn Chance | -
| Task Count Where The Snitch Will Be Revealed | -
| Include Team Jackal | -
| Use Different Arrow Color For Team Jackal | -
| Snitch can't be guessed after finishing all their tasks | -
-----------------------

## Jackal
### **Team: Jackal**
The Jackal is part of an extra team, that tries to eliminate all the other players.\
The Jackal has no tasks and can kill Impostors, Crewmates and Neutrals.\
The Jackal (if allowed by the options) can select another player to be their Sidekick.
Creating a Sidekick removes all tasks of the Sidekick and adds them to the team Jackal. The Sidekick loses their current role (except if they're a Lover, then they play in two teams).
The "Create Sidekick Action" may only be used once per Jackal or once per game (depending on the options).
The Jackal can also promote Impostors to be their Sidekick but, depending on the options the Impostor will either really turn into the Sidekick and leave the team Impostors or they will just look like the Sidekick to the Jackal and remain as they were.\
\
The team Jackal enables multiple new outcomes of the game, listing some examples here:
- The Impostors could be eliminated and then the crew plays against the team Jackal.
- The Crew could be eliminated, then the Team Jackal fight against the Impostors (The Crew can still make a task win in this scenario)

The priority of the win conditions is the following:
1. Crewmate Mini lose by vote
2. Jester wins by vote
3. Arsonist win
4. Team Impostor wins by sabotage
5. Team Crew wins by tasks (also possible if the whole Crew is dead)
6. Lovers among the last three players win
7. Team Jackal wins by outnumbering (When the team Jackal contains an equal or greater amount of players than the Crew and there are 0 Impostors left and team Jackal contains no Lover)
8. Team Impostor wins by outnumbering (When the team Impostors contains an equal or greater amount of players than the Crew and there are 0 players of the team Jackal left and team Impostors contains no Lover)
9. Team Crew wins by outnumbering (When there is no player of the team Jackal and the team Impostors left)

**NOTE:**
- The Jackal (and their Sidekick) may be killed by a Sheriff.
- A Jackal cannot target the Mini, while it's growing up. After that they can kill it or select it as its Sidekick.
- The Crew can still win, even if all of their members are dead, if they finish their tasks fast enough (that's why converting the last Crewmate with tasks left into a Sidekick results in a task win for the crew)

If both Impostors and Jackals are in the game the game continues even if all Crewmates are dead. Crewmates may still win in this case by completing their tasks. Jackal and Impostor have to kill each other.



### Game Options
| Name | Description
|----------|:-------------:|
| Jackal Spawn Chance | - |
| Jackal/Sidekick Kill Cooldown | Kill cooldown |
| Jackal Create Sidekick Cooldown | Cooldown before a Sidekick can be created |
| Jackal can use vents | Yes/No |
| Jackal can create a Sidekick | Yes/No |
| Jackals promoted from Sidekick can create a Sidekick | Yes/No (to prevent the Jackal team from growing) |
| Jackals can make an Impostor to their Sidekick | Yes/No (to prevent a Jackal from turning an Impostor into a Sidekick, if they use the ability on an Impostor they see the Impostor as Sidekick, but the Impostor isn't converted to Sidekick. If this option is set to "No" Jackal and Sidekick can kill each other.) |
| Jackal and Sidekick have Impostor vision | - |
-----------------------

## Sidekick
### **Team: Jackal**
Gets assigned to a player during the game by the "Create Sidekick Action" of the Jackal and joins the Jackal in their quest to eliminate all other players.\
Upon the death of the Jackal (depending on the options), they might get promoted to Jackal themself and potentially even assign a Sidekick of their own.
\
**NOTE:**
- A player that converts into a Sidekick loses their previous role and tasks (if they had one), except the Lover role.
- The Sidekick may be killed by a Sheriff.
- The Sidekick cannot target the Mini, while it's growing up.

### Game Options
| Name | Description
|----------|:-------------:|
| Jackal/Sidekick Kill Cooldown | Uses the same kill cooldown setting as the Jackal |
| Sidekick gets promoted to Jackal on Jackal death |  Yes/No |
| Sidekick can kill | Yes/No |
| Sidekick can use vents | Yes/No |
-----------------------

## Spy
### **Team: Crewmates**
The Spy is a Crewmate, which has no special abilities.\
The Spy looks like an additional Impostor to the Impostors, they can't tell the difference.\
There are two possibilities (depending on the set options):
- The Impostors can't kill the Spy (because otherwise their kill button would reveal, who the Spy is)
- The Impostors can kill the Spy but they can also kill their Impostor partner (if they mistake another Impostor for the Spy)
You can set whether the Sheriff can kill the Spy or not (in order to keep the lie alive).

### Game Options
| Name | Description
|----------|:-------------:|
| Spy Spawn Chance |
| Spy Can Die To Sheriff |
| Impostors Can Kill Anyone If There Is A Spy | This allows the Impostors to kill both the Spy and their Impostor partners
| Spy Can Enter Vents | Allow the Spy to enter/exit vents (but not actually move to connected vents)
| Spy Has Impostor Vision | Give the Spy the same vision as the Impostors have
-----------------------

## Security Guard
### **Team: Crewmates**
The Security Guard is a Crewmate that has a certain number of screws that they can use for either sealing vents or for placing new cameras.\
Placing a new camera and sealing vents takes a configurable amount of screws. The total number of screws that a Security Guard has can also be configured.\
The new camera will be visible after the next meeting and accessible by everyone.\
The vents will be sealed after the next meeting, players can't enter or exit sealed vents, but they can still "move to them" underground.\
\
**NOTE:**

- Trickster boxes can't be sealed
- The Security Guard can't place cameras on MiraHQ
- The remaining number of screws can be seen above their special button.
- On Skeld the four cameras will be replaced every 3 seconds (with the next four cameras). You can also navigate manually using the arrow keys
- Security Guard can access mobile cameras after placing all screws
- While accessing the mobile cameras, the Security Guard is not able to move

### Game Options
| Name | Description
|----------|:-------------:|
| Security Guard Spawn Chance |
| Security Guard Cooldown |
| Security Guard Number Of Screws | The number of screws that a Security Guard can use in a game
| Number Of Screws Per Cam | The number of screws it takes to place a camera
| Number Of Screws Per Vent | The number of screws it takes to seal a vent
| Security Guard Duration | -
| Gadget Max Charges | -
| Number Of Tasks Needed For Recharging | -
| Can't Move During Cam Duration | -
-----------------------

## Bait
### **Team: Crewmates**

The Bait is a Crewmate that if killed, forces the killer to self report the body (you can configure a delay in the options).
Additionally, the Bait can see if someone is inside a vent (depending on the options the exact vent gets
an outline or all vents do).

### Game Options
| Name | Description
|----------|:-------------:|
| Bait Spawn Chance | -
| Bait Highlight All Vents | If set to true, all vents will be highlighted if a player is inside of one of them. If set to false, only the vents where players are siting in will be highlighted.
| Bait Report Delay | -
| Warn The Killer With A Flash | -

## Opportunist
### **Team: N/A**
Created by [libhalt](https://twitter.com/libhalt)

The Opportunist is a outsider role. So long as they are alive at the end of the game, they win alongside the victorious team. They can choose to side with any team they wish to ensure their own survival.

## GM
### **Team: N/A**
Created by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)

The GM (Game Master) is an observer role. Their presence has no effect on the game itself, and all players know who the GM is at all times. The GM cannot be targeted by other players, cannot fix sabotages, and cannot vote or be voted for. They are a completely external presence, provided with a range of tools at their disposal to efficiently observe the flow of the game.

The GM role is intended to allow groups to play with a wide variety of rules not supported by Among Us natively. 

### Game Options
| Name | Description
|----------|:-------------:|
| GM is always the host | Always assign the GM role to the lobby's host
| GM can warp to other players | Allow the GM to teleport to other players
| GM can kill/revive players | Allow the GM to indiscriminately murder or revive players
| Hide settings from other players | Hides mod-related settings from everyone except the lobby host
| GM dies at start of game | The GM begins the game dead

## Medium
### **Team: Crewmates**

The medium is a crewmate who can ask the souls of dead players for information. Like the Seer, it sees the places where the players have died (after the next meeting) and can question them. It then gets random information about the soul or the killer in the chat. The souls only stay for one round, i.e. until the next meeting. Depending on the options, the souls can only be questioned once and then disappear.
During the meetings you can see, whether a player wears a darker or a lighter color, represented by (D) or (L) in the names.

Questions:
What is your Role?
What is your killer's color type?
When did you die?
What is your killers role? (mini exluded)

### Game Options
| Name | Description
|----------|:-------------:|
| Medium Spawn Chance | -
| Medium Cooldown | -
| Medium Duration | The time it takes to question a soul
| Medium Each Soul Can Only Be Questioned Once | If set to true, souls can only be questioned once and then disappear
-----------------------

## Vulture
### **Team: Neutral**

The Vulture does not have any tasks, they have to win the game as a solo.\
The Vulture is a neutral role that must eat a specified number of corpses (depending on the options) in order to win.\
Depending on the options, when a player dies, the Vulture gets an arrow pointing to the corpse.
If there is a Vulture in the game, there can't be a Cleaner.

### Game Options
| Name | Description |
|----------|:-------------:|
| Vulture Spawn Chance | -
| Vulture Countdown | -
| Number Of Corpses Needed To Be Eaten | Corpses needed to be eaten to win the game
| Vulture Can Use Vents | -
| Show Arrows Pointing Towards The Corpses | -
-----------------------

## Lawyer
### **Team: Neutral**
The Lawyer is a neutral role that has a client.
The client might be an Impostor or Jackal which is no Lover.
The Lawyer needs their client to win in order to win the game.
If their client dies or gets voted out, the Lawyer changes their role and becomes the [Pursuer](#pursuer), which has a different goal to win the game.
The main goal of the Lawyer is to win as Lawyer, as it is not allowed to betray their client.

The Lawyer can win in multiple ways:
- Lawyer dead, client alive and client team won: The Lawyer wins together with the team of the client
- Lawyer and client alive and client team won: The Lawyer wins with the team of the client. The client **doesn't** win (even if their Impostor/Team Jackal mate wins), the Lawyer steals their win. Hence the client should keep the Lawyer alive for some time, to get some help during the meetings, but has to eliminate them soon enough to not get their win stolen.

**NOTE:**
- If the client disconnects, the Lawyer will also turn into the Pursuer
- If "Lawyer Target Knows" is set to true, the client will know that someone is their Lawyer, but won't know who.

### Game Options
| Name | Description |
|----------|:-------------:|
| Lawyer Target Knows | The target knows that it is the target (marked with "§", if the Lawyer dies, the mark will disappear)
| Lawyer Wins After Meetings | If set to true, the Lawyer wins after a configurable amount of meetings (can't start meetings himself)
| Lawyer Needed Meetings To Win | -
| Lawyer Vision | Pursuer has normal vision
| Lawyer Knows Target Role | -
| Pursuer Blank Cooldown | -
| Pursuer Number Of Blanks | -
-----------------------

## Pursuer
### **Team: Neutral**
The Pursuer is still a neutral role, but has a different goal to win the game; they have to be alive when the game ends (no matter who causes the win).
In order to achieve this goal, the Pursuer has an ability called "Blank", where they can fill a killers (this also includes the Sheriff) weapon with a blank. So, if the killer attempts to kill someone, the killer will miss their target, and their cooldowns will be triggered as usual.
If the killer fires the "Blank", shields (e.g. Medic shield or Time Master shield) will not be triggered.
The Pursuer has tasks (which can already be done while being a Lawyer), that count towards the task win for the Crewmates. If the Pursuer dies, their tasks won't be counted anymore.

## Plague Doctor
### **Team: Neutral**

Created by [haoming37](https://github.com/haoming37)

The Plague Doctor is a neutral role whose goal is to infect every living player. They start by choosing one player to infect, after which anyone who spends a set amount of time in range of the infected player becomes infected themselves. Infection progress is cumulative, and does not reset with distance or after meetings.

The Plague Doctor is still able to win even if dead. Furthermore, if killed, their killer is automatically infected.

A set period of time after each meeting, players are immune from infection, letting them get safely away from potentially infected players.

## Fox
### **Team: Neutral**

Created by [haoming37](https://github.com/haoming37)

The Fox is a Neutral role whose goal is to keep themselves hidden from both the Crew and Impostors while completing their own tasks. The Fox is always aware of where any threats are, and has numerous abilities to protect themselves.

## Immoralist
### **Team: Neutral**

Created by [haoming37](https://github.com/haoming37)

The Immoralist's objective is to support the Fox in any way they can, even if it means sacrificing their own life.

## Fortune Teller
### **Team: Crewmate**

Created by [haoming37](https://github.com/haoming37)

The Fortune Teller is a Crewmate with the power to divine a single player's role. They start out believing themselves to be an ordinary Crewmate, only awakening to their powers after completing a designated number of tasks.

## Watcher
### **Team: Crewmate or Impostors**
Created by [Virtual_Dusk](https://twitter.com/Virtual_Dusk)

The Watcher is a player capable of seeing everyone's votes during meetings.

# Source code
It's bad I know, this is a side project and my second week of modding. So there are no best practices around here.
You can use parts of the code but don't copy paste the whole thing. Make sure you give credits to the other developers, because some parts of the code are based on theirs.

# Translation
As of v2.9.0, The Other Roles GM now supports translation into other languages. To add a new language, add translations to [Strings.xlsx](/Strings.xlsx) and submit a pull request.
