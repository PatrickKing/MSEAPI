# What is the Multi-Surface Environment API?

An API for creating Multi-Surface Environments. This repository is used as server for location based calls. The RoomVisualizer project is used as a centralized place that displays all information that is currently known to the API

# Getting Started with MSE API

1.  Install [Kinect SDK 1.6](http://go.microsoft.com/fwlink/?LinkID=262831) (The API currently supports only version 1.6 only)
2.  Install [Bonjour SDK] (https://developer.apple.com/bonjour/) (Must have Apple Developer Account)
3.  Install [NuGet] (http://nuget.codeplex.com/)
4.  Clone MSE API repository using `git clone git@github.com:ase-lab/MSEAPI.git --recursive`

    The `--recursive` ensures that all submodules get downloaded. If you forget, you can use `git submodule update --recursive --init` later.
5. Open `MSEAPI.sln` in Visual Studio and ensure it builds correctly `F6`
