# Iconic Framework

Framework for adding shortcut icons to vanilla and mod functions.

- [Iconic Framework](#iconic-framework)
  - [API](#api)
  - [Assets](#assets)
    - [Menu](#menu)
    - [Method](#method)
    - [Keybind](#keybind)
  - [Texture Overrides](#texture-overrides)
    - [Example](#example)
  - [Translations](#translations)
  - [Credits](#credits)

## API

Direct integration using
the [Iconic Framework
API](https://github.com/LeFauxMatt/FauxCore/blob/develop/FauxCommon/Integrations/IconicFramework/IIconicFrameworkApi.cs)
is preferred.

## Assets

External integration is possible using data paths with
[SMAPI](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Content#Edit_a_game_asset)
or
[Content Patcher](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md).

You can find examples in the [\[CP\] Toolbar Icons](<IconicFramework/[CP] Toolbar Icons/>) folder.

The data model is as follows:

| Entry       | Description                                                 |
| :---------- | :---------------------------------------------------------- |
| ModId       | The mod that the icon is for.                               |
| Type        | Menu, Method, or Keybind                                    |
| ExtraData   | Additional data depending on integration type (see below)   |
| Title       | The title text to display when hovering over an icon.       |
| HoverText   | The description text to display when hovering over an icon. |
| TexturePath | The path to the icon's texture.                             |
| SourceRect  | The source area of the icon's texture.                      |

### Menu

ExtraData requires the fully qualified name of the Menu class. It must have a parameterless constructor.

### Method

ExtraData requires the fully qualified name of the Method. It must be a
parameterless method.

### Keybind

ExtraData requires the keybind.

## Texture Overrides

A content pack can provide custom texture for any icon. You need the icon's
unique identifier which you can find in your config.json file.

### Example

```json
{
  "Format": "2.4.0",
  "Changes": [
    {
      "LogName": "Edit the icon texture",
      "Action": "EditData",
      "Target": "furyx639.ToolbarIcons/TextureOverrides",
      "Entries": {
        "Pathoschild.HorseFluteAnywhere": {
          "Texture": "{{InternalAssetKey: assets/HorseFluteIcon.png}}",
          "SourceRect": {
            "X": 0,
            "Y": 0,
            "Width": 16,
            "Height": 16
          }
        }
      }
    }
  ]
}
```

## Translations

❌️ = Not Translated, ❔ = Incomplete, ✔️ = Complete

|            |         Iconic Framework          |                   [CP] Toolbar Icons                   |
| :--------- | :-------------------------------: | :----------------------------------------------------: |
| Chinese    | [❌️](IconicFramework/i18n/zh.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/zh.json>) |
| French     | [❌️](IconicFramework/i18n/fr.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/fr.json>  |
| German     | [❌️](IconicFramework/i18n/de.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/de.json>  |
| Hungarian  | [❌️](IconicFramework/i18n/hu.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/hu.json>  |
| Italian    | [❌️](IconicFramework/i18n/it.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/it.json>  |
| Japanese   | [❌️](IconicFramework/i18n/ja.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/ja.json>  |
| Korean     | [❌️](IconicFramework/i18n/ko.json) | [❔](<IconicFramework/[CP] Toolbar Icons/i18n/ko.json>) |
| Portuguese | [❌️](IconicFramework/i18n/pt.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/pt.json>  |
| Russian    | [❌️](IconicFramework/i18n/ru.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/ru.json>  |
| Spanish    | [❌️](IconicFramework/i18n/es.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/es.json>  |
| Turkish    | [❌️](IconicFramework/i18n/tr.json) | [❌️](<IconicFramework/[CP] Toolbar Icons/i18n/tr.json>  |

## Credits

Icons created by [Tai](https://www.nexusmods.com/stardewvalley/users/92060238):

* Always Scroll Map
* To-Dew
* Daily Quests
* Special Orders
