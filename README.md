# What is the Multi-Surface Environment API?

An API for creating Multi-Surface Environments. This repository is used as server for location based calls. The RoomVisualizer project is used as a centralized place that displays all information that is currently known to the API

# Getting Started with MSE API

1. Install [Kinect SDK 1.7](http://go.microsoft.com/fwlink/?LinkId=275588)
2. Install [Bonjour SDK] (https://developer.apple.com/bonjour/) (Must have Apple Developer Account)
3. Install [NuGet] (http://nuget.codeplex.com/)
4. Clone MSE API repository using `git clone git@github.com:ase-lab/MSEAPI.git --recursive`

    The `--recursive` ensures that all submodules get downloaded. If you forget, you can use `git submodule update --recursive --init` later.
	
5. Open `MSEAPI.sln` in Visual Studio
6. Set the `RoomVisualizer` project as the startup project
7. Ensure it builds correctly `F6`

# Setting up Kinects with MSE API

1. Clone the [KinectClient](https://github.com/ase-lab/KinectClient) repository.
2. Make sure that the computer that is running the KinectClient is on the same network as the computer running the MSE API.(Note: You can run the MSE API and the KinectClient on the same computer.)
3. Run the KinectClient and then it should show up on the MSE API visualizer like below.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot1.png)

4. Drag and drop each Kinect on the visualizer canvas to match the location and orientation of the Kinects in the room.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot2.png)
	
# Calibrating the Kinects (for multiple kinects)

If more than one Kinect is being used then they have to be calibrated. Calibration makes sure that the relative location of each Kinect in the visualizer matches the relative location of each Kinect in the room. It is recommended to have the the kinects' orientation at 0°, 90° , 180° or 270° since this allows for easier calibration and better tracking. Here is how to calibrate Kinects in MSE API:

1. One person should stand in the overlapping area between the Kinects that are to be calibrated like below. Since these Kinects are not yet calibrated, that person will appear twice (or more, depending on the number of Kinects being used) in the visualizer.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot4.png)
	
2. Click on those two persons in the visualizer and they will turn blue.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot3.png)
	
3. Simply click on the calibrate button on the sidebar. The person should leave the room and reenter and the Kinects will be calibrated like below.
	
	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot5.png)

# What is the best way to set up Kinects in the room ?

There are essentially two ways to set up Kinects in a room:

1. One way is to have multiple kinects point towards the same area. This will allow for better tracking of the people in that small area.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot6.png)

2. Another way is to have each kinect point at a different area to increase the area of tracking. However, it is really important to have enough overlapping coverage between Kinects so that there is always at least one kinect tracking people who are moving across.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot7.png)
	
# How to pair devices with people

MSE API tracks the location of devices by tracking the people who are holding these devices. To do that, devices need to be "paired" with the person who is holding that device. Here is how to pair a device with a person:

1. Whenever an application that is running any of the MSE API client libraries is turned on, it will show up on the sidebar of the visualizer like below.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot9.png)
	
2. Simply drag and drop that device on a person in the visualiser to pair them.

	![ScreenShot](https://raw.github.com/ase-lab/MSEAPI/Warnings/Screenshots/Screenshot8.png)
	
3. If a device is not a hand held device (i.e a tabletop or a display), it can be dragged and dropped anywhere on the visualizer canvas.
	
