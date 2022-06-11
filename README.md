## INFO ##
- Endless Ocean 1 & 2 Packer
- Author: NiV, MDB
- Special thanks to Taylor
- Current version: 1.0
- *If you have any issues, join this discord server and contact NiV-L-A: https://discord.gg/4hmcsmPMDG - Endless Ocean Wiki Server

## Changes ##
- Initial Commit

## Description ##
- Straight-forward C# code that has the ability to build the GAME.DAT/INFO.DAT pair from feeding the INFO.DAT and the content folder.
- NOTE: It deletes GAME.DAT and creates a new one, it modifies INFO.DAT accordingly.
- In case of EO1, it decrypts INFO.DAT and encrypts it back when GAME.DAT has been built sucessfully.
- Supported game versions:
- EO1
	- NTSC RFBE01
	- PAL RFBP01
	- JAP 1.0 RFBJ01
	- JAP 1.1 RFBJ01
- EO2
	- NTSC R4EE01
	- PAL R4EP01
	- JAP R4EJ01
	
## How to run ##
- NOTE: You need .NET Desktop Runtime 5.0 to run this program.
	- https://dotnet.microsoft.com/en-us/download/dotnet/5.0
- To use this tool, create a file in the same directory as the .exe called "EndlessOceanPackerSettings.txt", 
- Make sure you have the .txt in this format:
	- #Argument 1, INFO.DAT & GAME.DAT folder path (input & output folder)
	- InfoFolder=
	- #Argument 2, content folder
	- ContentFolder=
- Lines that start with '#' will be ignored
- You can have a third argument, '-log'. It will create another .txt file where some log information will be stored.
- Example:
	- #Argument 1, INFO.DAT & GAME.DAT folder path (input & output folder)
	- InfoFolder=C:\Users\Roberto\Desktop\EOModding\Wii\Endless Ocean Blue World [R4EE01]\files
	- #Argument 2, content folder
	- ContentFolder=C:\Users\Roberto\Desktop\EOModding\Wii\Endless Ocean Blue World [R4EE01]\files\QuickBMSOutput
	- -log

https://user-images.githubusercontent.com/44531714/173192058-35478e4e-8028-45ec-b914-ed3ecd148a9b.mp4
