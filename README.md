# What is the Multi-Surface Environment API?

An API for creating Multi-Surface Environments

# Getting Started with MSE API

1.  Install [Kinect SDK 1.5](http://www.microsoft.com/en-us/kinectforwindows/develop/developer-downloads.aspx)
2.  Install [Bonjour SDK] (https://developer.apple.com/bonjour/) (Must have Apple Developer Account)
3.  Install [NuGet] (http://nuget.codeplex.com/)
4.  Clone MSE API repository using `git clone git@github.com:ase-lab/MSEAPI.git --recursive`

    The `--recursive` ensures that all submodules get downloaded. If you forget, you can use `git submodule update --recursive --init` later.
5. Open `MSEAPI.sln` in Visual Studio and ensure it builds correctly `F6`

# Jenkins

This repository is set up with Jenkins, so whenever something is pushed to GitHub, then Jenkins runs the build script
