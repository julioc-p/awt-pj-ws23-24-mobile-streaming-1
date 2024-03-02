# awt-pj-ws23-24-mobile-streaming-1

## Current Progress on Demo

- [ ] Tested on macOS using Apple Silicon Mac
- [ ] Tested on macOS using Intel Mac
- [x] Implemented for Windows devices (Intel)
- [ ] Implemented for Windows devices (AMD)
- [x] Tested on Windows

## Build

### Preparation:

1. Install [dotnet 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0).
2. Make sure Intel Power Gadget 3.6 (Windows/Intel) is installed.
3. Make sure [libman](https://learn.microsoft.com/en-us/aspnet/core/client-side/libman/libman-cli?view=aspnetcore-7.0) is installed.
4. Run `libman restore` to install missing packages.

### Windows (Intel)

1. `dotnet restore` in the project directory.
2. `dotnet run` in the project directory.
3. Press on the cog button on the analytics section to save URLs or internal paths, and press the "Start Analytics" button to start the analysis, or click on the button to analyze with the default video bbb_30fps.
4. Click on "Stop Analytics" or wait until the analysis is complete. Once finished, you can view the graphs corresponding to the data in the folder `Measurements/analytics_results`.
