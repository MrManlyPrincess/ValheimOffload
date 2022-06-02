# Valheim Offload

## Features
Allows you to offload your inventory into nearby chests at the press of a button.

Ignores containers that are part of the world generation so that you don't accidentally lose items,
and allows you to designate slots/items to not offload.

## Installation (manual)
### Manual Install
Extract ValheimOffload.dll into the BepinEx/plugins folder.

## Config
### Default Config File

```
## Settings file was created by plugin Valheim Offload v0.0.3
## Plugin GUID: com.mrmanlyprincess.ValheimOffload

[General]

## Enable this mod
# Setting type: Boolean
# Default value: true
Enabled = true

## Radius in which to search for containers
# Setting type: Single
# Default value: 15
Offload Radius = 15

[Ignore Options]

## If true, consumable items will not be offloaded
# Setting type: Boolean
# Default value: true
Ignore Consumables = true

## If true, unequipped ammo will not be offloaded
# Setting type: Boolean
# Default value: true
Ignore Unequipped Ammo = true

## A pipe-delimited list of item slots to ignore. Each slot is referenced by a zero-based Vector2. First slot is '0,0', last slot is '3,7'.If a value in the Vector2 is left blank, the entire column/row will be affected.
# Setting type: String
# Default value: 0,2|1,2|2,2|0,3|1,3|2,3|
Item slots to ignore = 0,2|1,2|2,2|0,3|1,3|2,3|

## A comma-delimited list of item names to ignore. Examples: '$item_hammer, $item_hoe, $item_leatherscraps'
# Setting type: String
# Default value:
Items to ignore =

[Keybindings]

## KeyboardShortcut for OffloadButton keybinding. Modifiers are ignored.
# Setting type: KeyboardShortcut
# Default value: Tilde
Offload Button (Keyboard) = Tilde
```

## Changelog
- **v0.0.6 -** Minor optimization while inventory is open
- **v0.0.5 -** Initial version

## Known issues
- Modifier keys don't work on the Shortcut key. This is due to an issue with BepinEx's ShortcutKey that I had to work around.
