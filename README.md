# Guidelines for setup of Drone Project
# Unity Editor version 2022.3.10
# Last updated 23rd April 2024

1. ## Installation of required software:
**Unity Hub and Unity Editor**
 - Download [Unity Hub] (https://unity.com/download) from the official Unity website.
 - Install Unity Hub and use it to install Unity Editor version 2022.3.10f1.
**Blender**
 - Download and install [Blender] (https://www.blender.org/download/).
 -  *Note* Blender is used for rendering the meshes. If the meshes are not displaying correctly, right-click on the project folder within the open Unity project and select 'Re-import'.
 - *Note* There is a dependency between Unity and objects imported from Blender, so having it installed is necessary.  
** Oculus application**
- Download and install [Oculus Setup] (https://www.meta.com/dk/en/quest/setup/).
- *Note* This software is necessary for running the Drone Project application directly from Unity, using Link Cable or Air Link.
- After the installation a Meta account is needed to connect with the headset. Follow instructions in the software.
**Clone the Repository**
- Go to the [Drone Project repository] (https://github.com/Capt-J-Kirk/DroneProject) (You might be there :-)) on GitHub and clone the project to your local machine.
**Add the Project to Unity Hub**
- Open Unity Hub, navigate to the 'Projects' tab.
- Click on the drop-down menu next to the 'ADD' button at the top right and select 'Add project from disk'.
- Locate the folder where you cloned the Drone Project and select it.
- Load the project by clicking on the new entry in Unity Hub.

2. ## VR Setup:
 a) **Prepare the VR Equipment**
- Start the Oculus application on your PC.
- Connect your VR headset to the PC (if using cable). Ensure it is listed as active in the Oculus application.
 b) **Notes on Getting the application to run in VR, when using Link Cable**
- Link Cable was used to run the application directly from Unity and displaying it in the VR-headset.
- It was experienced that sometimes the headset would not run the application when pressing play in Unity.
- Often the issue was resolved by rebooting the headset and pressing play again in Unity.
- If using the wireless Air Link connection, this issue might not be present.

3. ## Scenes / GitHub repository branches.
 - At the time of writing the main branch of the repository has 2 scenes.
 a) ** Scene 1**
 - Has a Drone Controller based on Unity's built in methods of rotation and movement.
 - Has a performance measure based on the textures (visible ‘dirt’) of different parts of a wind turbine.
 - Has only a control scheme where 2 operators each control one drone - a Washing drone, and an Observing Drone.
b) ** Scene 2**
 - Has different control schemes where the VR-player has two monitors attached to the hand, and control both drones. The secondary drone positions itself relative to the main drone.
 - This scene uses PID controllers for the drones.
 c) Repository branch ‘Jannich’ is the other option, in case Scene 2 on main is not fully functional.

4. ## Launching the project:
 - Within Unity, find the desired scene (in scenes folder).
 - Open the scene with double click.
 - Launce the Oculus link connection from the headset.
 - Click on the play/run game icon located at the top of the Unity window.
 ** You should now be ready to test and interact with the Drone Project in a VR environment **
