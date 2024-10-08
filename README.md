# Soulash2-SaveSync

Built this project to enable cloud saving since I play between devices. Its quite rudimentary currently, but this should be fairly easy to modify in the future to support any game.

Current cloud saving integrations are here: https://github.com/CurtisDH/Soulash2-SaveSync/tree/master/Soulash2-SaveSync/Integrations
at the time of writing I've only done a quick and dirty dropbox implementation

## How to use
- Download release, put the contents in the directory of Soulash2.exe, then run the SaveSync.exe, select the integration and follow the steps to set it up, once its done it'll launch the game, once the game closes it'll upload the save file. Every time you run the SaveSync.exe it'll download the contents and prompt you to replace the files (see the config file section if you want to disable the replacement prompt)

## Config files

---

#### savesync_config.json
Here you can modify the following fields to theorhetically work with any game, the defaults are configured for Soulash2.
- **`BackupDirectory`** - **_Default_**: `SaveSyncBackups` This can be changed to a path, but if it's just the name it'll be created in the same directory as the .exe
- **`SelectedIntegrationName`** -  This updates based on the first time running selection. If there is an issue an issue with the integration or you want to change, just delete the config and rerun the exe
- **`SaveLocation`** - _**Default**_: `AppData\\Roaming\\WizardsOfTheCode\\Soulash2\\saves` Defaults to appdata Soulash2, if you want to customise this for any game point this to the save directory of the game you want to support
- **`ReplaceSaveWithoutAsking`** - **_Default_**: `false`, change this to true if you want to remove the replace prompt from the console window.
- **`ExePath`** - **_Default_**: 
`"C:\\Program Files (x86)\\Steam\\steamapps\\common\\Soulash 2\\Soulash 2.exe"`,
allows for the exe path to be changed this can be modified to support any other game too. If you still want to launch through steam and enable save sync, rename the original exe, and update this path, then change the `SaveSync.exe` to the orignal game name e.g. rename `Soulash 2.exe` -> `Original Soulash 2.exe` | rename `SaveSync.exe` -> `Soulash 2.exe` | Update the config ExePath to `Original Soulash 2.exe`, then launch through steam as normal
  ![image](https://github.com/user-attachments/assets/91b45022-896d-4e88-aad3-b12bbc85dadf)

Example full config with Dropbox Integration selected
```json 
{
"BackupDirectory": "SaveSyncBackups",
"SelectedIntegrationName": "Dropbox",
"ReplaceSaveWithoutAsking": true,
"SaveLocation": "C:\\Users\\YOURUSERNAME\\AppData\\Roaming\\WizardsOfTheCode\\Soulash2\\saves",
"LaunchConfig": {
"ExePath": "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Soulash 2\\Soulash 2.exe"
},
"DropboxConfigModel": {
"AccessToken": "REDACTED",
"Uid": "REDACTED",
"RefreshToken": "REDACTED"
}
}
```
  