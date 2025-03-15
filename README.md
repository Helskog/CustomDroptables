<p align="center"><img src="https://i.imgur.com/Xwt45xO.jpeg"/></p>

# CustomDroptables
A V-Rising server plugin allowing more control over loot spawns in containers. Customize your looting experience by adding items to or entirely replace existing droptables in V-Rising. Applicable for chests, drawers and other containers that spawn in the world with randomized loot.

## Features

- Extensive modification of droptables via configuration file.
- Adjust spawn chance of individual items from below 1% up to 100%
- Add multiple items to any droptable.
- Replace the droptable entirely, leaving you in control of all items spawned.
## Limitations

- Most droptables are not unique, meaning multiple containers in the world share the same droptable. However there are some exceptions, check the Google Sheets link below for an overview.
- You can only set fixed quantities for individual items (for now). You can theoretically bypass this by adding multiple items to the same droptable with different spawn chances.
- If you add too many items or the containers doesnt fit, the rest will be dropped on the floor next to it. Due to this you should keep in mind order of item-objects in the config, first one will spawn in the next available slot and so on until the next one cant fit.
## Configuration
The configuration file is automatically created in `\%SERVER_FOLDER%\BepInEx\config\CustomDropTables\` as `Configuration.json` and will contain some boilerplate to get you going.
```json
{
  "1049541583": {             <       Drop table GUID value.
    "replace_all": false,     <       False to add on items to existing droptable, true to replace all items.
    "add_items": {            <       List of items to add
      "-257494203": {         <       Item group, specify the Prefab GUID you want to spawn as the "Key" value of the group.
        "quantity": 100,      <       Define amount of items to spawn
        "spawnchance": 1.0    <       Define chance of spawning. 1.0 is 100%, 0.5 is 50%, 0.25 is 25% and so on.
      },                               
      "-1913156733": {        <       Easily repeat the pattern for more items.
        "quantity": 15,
        "spawnchance": 0.5
      },
      "-850142339": {
        "quantity": 1,
        "spawnchance": 0.25
      }
    }
  },
  "-600923884": {             <       Add more drop tables with different items.
    "replace_all": true,
    "add_items": {
      "-257494203": {
        "quantity": 10,
        "spawnchance": 0.44
      },
      "-1913156733": {
        "quantity": 100,
        "spawnchance": 0.01
      },
      "-850142339": {
        "quantity": 5,
        "spawnchance": 0.05
      }
    }
  }
}
```

## Information about Prefab & Droptable GUID values.
More information about GUID values can be obtained on the [V-Rising Mod Wiki.](https://wiki.vrisingmods.com/prefabs/All)
You can also check my [google-sheets](https://docs.google.com/spreadsheets/d/1rnUhI94OsQjDguQZr2sxKGTnhVt1W-emFUXpQFfnAKA/edit?usp=sharing) document for more information about the droptables.

## Disclaimer
This plugin is not tested in a live-enviroment with lots of players so beware bugs or other issues that could arise, to report them you can raise an issue here on github or join the [V-Rising modding community discord](https://discord.com/invite/QG2FmueAG9) to speak with me directly.

### Dependencies
- [Bloodstone](https://thunderstore.io/c/v-rising/p/deca/Bloodstone/) by Deca.

### Credits
- [Mitch (zfolmt)](https://github.com/mfoltz) for general questions / help.
- [Odjit](https://github.com/Odjit) for initialization patch / core structure.

- [V Rising Modding Community](https://vrisingmods.com)

### License

This project is licensed under the GPL-3.0 license.
