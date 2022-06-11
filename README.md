## INFO ##
- Endless Ocean 1 & 2 Packer
- Author: NiV, MDB
- Special thanks to Taylor
- Current version: 1.0
- *If you have any issues, join this discord server and contact NiV-L-A: https://discord.gg/4hmcsmPMDG - Endless Ocean Wiki Server

## Changes ##
- Initial Commit

## Description ##
- Straight-forward C# code that has the ability to create the GAME.DAT/INFO.DAT pair from feeding the INFO.DAT and the content folder.
- Supported game versions:
- EO1 (decrypts INFO.DAT on the fly, then encrypts it back)
	- NTSC RFBE01
	- PAL RFBP01
	- JAP 1.0 RFBJ01
	- JAP 1.1 RFBJ01
- EO2
	- NTSC R4EE01
	- PAL R4EP01
	- JAP R4EJ01
	
## How to run ##
- To use this tool, create a file in the same directory as the .exe called "EndlessOceanPackerSettings.txt", 
- Make sure you have the .txt in this format:
	- #Argument 1, INFO.DAT & GAME.DAT folder path (input & output folder)
	- InfoFolder=
	- #Argument 2, content folder
	- ContentFolder=
- Lines that start with '#' will be ignored