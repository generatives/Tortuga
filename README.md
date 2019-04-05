# Tortuga
A simple, cross-platform 2D game development framework for C#

My goal with this project is to create a simple framework for game development across Windows, Linux, OSX, Android, and iOS. It will provide 2D rendering, audio playback, resource management, and other basic requirements for development. It will follow a code first approach and is intended for developers with some programming experience.

The project is in the very early stages and is completely undocumented. Currently it provides a draw batching system and audio playback for Windows and Android. It should work on Linux and OSX as well but has not been tested.

The repository also contains the start of a Scene-GameObject-Component system. This has been set aside for now while the core functionality is developed.

The font loader and rendering code is ported from https://github.com/cyotek/Cyotek.Drawing.BitmapFont