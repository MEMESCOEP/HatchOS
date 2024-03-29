# [ I have stopped working on this project! ]
# HatchOS
HatchOS is an experimental, [Cosmos](https://www.github.com/CosmosOS/Cosmos) based operating system designed for x86_64 computers.
<hr/>
<br/>

## Features
Here is a list of features that are currently implemented:
* Basic GUI
* Basic serial console
* Basic window manager
* Power functionality (Shutdown, Reboot, WIP ACPI)
* Demo 3D Rasterizer (Implemented in PrismAPI; Currently broken due to funky cosmos things)
* [WIP] AC97 Audio system and driver
<hr/>
<br/>

## Resources used
The following resources have been used during development:
* [Cosmos OS Development Kit](https://www.github.com/CosmosOS/Cosmos) (Ver. DEVKit_2023604)
* [PrismAPI](https://github.com/Project-Prism/Prism-OS/tree/main/PrismAPI) (Ver. 1.1)
<hr/>
<br/>

## Future Plans
I plan to add more functionality and features to HatchOS. Here's what I'd like to do:
* Add a file manager
* Add networking (HTTP, FTP, etc.)
* Add syscalls
* Add ELF executable support
* Add an internal API for system functions
* Add USB/XHCI support
* Add floppy disk support
* Improve hardware compatibility
* Improve performance and code quality
<hr/>
<br/>

## Compilation
Before compiling HatchOS, make sure you've installed the following:
* [Cosmos OS Development Kit](https://www.github.com/CosmosOS/Cosmos)
* [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

Once you've installed those, simply run `dotnet build` or `build.py` to compile.
>Note: `build.py` internally calls `dotnet build`. It's in a python script because it allows compilation without requiring the user to type in a command every time.
<hr/>
<br/>

## Media
If you would like to see what HatchOS looks and/or sounds like, [click here](https://www.github.com/memescoep/HatchOS/tree/main/media/media.md).
<hr/>
<br/>

## License
This project is licensed under the MIT license. You can view the license [here](https://github.com/MEMESCOEP/HatchOS/blob/main/LICENSE).
