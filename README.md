# Soulash2-SaveSync

Built this project to enable cloud saving since I play between devices. Its quite rudimentary currently, but this should be fairly easy to modify in the future to support any game.

Current cloud saving integrations are here: https://github.com/CurtisDH/Soulash2-SaveSync/tree/master/Soulash2-SaveSync/Integrations
at the time of writing I've only done a quick and dirty dropbox implementation

## How to use
- Download release, put the contents in the directory of Soulash2.exe, then run the SaveSync.exe, select the integration and follow the steps to set it up, once its done it'll launch the game, once the game closes it'll upload the save file. Every time you run the SaveSync.exe it'll download the contents and prompt you to replace the files (see the config file section if you want to disable the replacement prompt)

## Config files
Planning to eventually clean this up and put it all into one file but currently there is the following:
- Dropbox integration config, no need to mess with this it just allows for api access to upload 
- integration_config.json -> shows which Integration is selected, and if you want to remove the tedious process of always saying yes to replacement you can set the "ReplacSaveWithoutAsking":true -- beware if the save corrupts there's no local backup system in place right now
- launch_cfg.json -> allows for the exe path to be changed, if you still want to launch through steam and enable save sync, rename the original exe, and update this path, then change the SaveSync.exe to the orignal game name
- i.e. rename "Soulash 2.exe" -> "Original Soulash 2.exe" | rename "SaveSync.exe" -> "Soulash 2.exe" | Update the config ExePath to "Original Soulash 2.exe", then launch through steam as normal
![image](https://github.com/user-attachments/assets/91b45022-896d-4e88-aad3-b12bbc85dadf)




## Development TODOs
- Create a backup before replacing files
  
  Currently there is no backup system in place, if for some reason the upload gets corrupted then it could spread pretty quickly and override save files

- BaseIntegration

  I'll add a settings config here for each integration, currently that has to be handled on a per implementation process

- Save path modification

  I'll add the save path to the launch config so this can theorhetically be expanded to any game
