# awt-pj-ws23-24-mobile-streaming-1

## current progress on demo

- [x] tested on macOS using Apple Silicon Mac
- [x] tested on macOS using Intel Mac
- [x] implemented for Windows devices (Intel)
- [ ] implemented for Windows devices (AMD)
- [x] tested on Windows

## Build

Preparation:

1. install dotnet 7
2. make sure powermetrics (macOS) or Intel Power Gadget 3.6 (Windows/Intel) is installed
3. make sure [libman](<https://learn.microsoft.com/en-us/aspnet/core/client-side/libman/libman-cli?view=aspnetcore-7.0>) is installed
4. run `libman restore` to install missing packages

MacOs

1. `dotnet run` in project directory

Windows (Intel)

1. `dotnet restore` in project directory
2. `dotnet run` in project directory
