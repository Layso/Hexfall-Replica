Project: Hexfall game replica for Vertigo Games job application
Start Date: 29/03/2018
Due Date: 2/04/2018
Author: Layso



29/03/2018 - Thursday
---------------------
- Project plan has drawn as TODO list (to my whiteboard)
- Unity and Visual Studio updated
- Android SDK tools downgraded due to Unity Android build error
- Git installed for version control
- Project created on both Github and Unity
- Temporary Main Menu created to switch to game screen
- Game screen can initialize a grid with a changable size  



30/03/2018 - Friday
-------------------
- Hex selection mechanic implemented
- Each touch on same hexagon will make it select its next 2 neighboor (towards clockwise direction)
- A touch on a different hexagon will reset direction counter which selects first set of hexagons (pivot, up, up-left)
- A function implemented to specify which hexagons are selected currently by outlining them
- Performance improvements can be done due to massive object creation and component creation process
- Menu manager script created to switch from main menu to game screen
- First demo APK created to test the touch and select mechanism, seems fine



31/03/2018 - Saturday
---------------------
- Some hexagon specific functions taken from GridManager class to Hexagon itself for logical reasons
- First rotation attempt seems successfull. Still needs some adjustments for neighbour hexagons
- Rotation fully implemented through out the day
- InputManager class created to seperate input prcoess from input checks, GridManager now handles inputs according to InputManager's directions
