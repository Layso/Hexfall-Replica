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



01/04/2018 - Sunday
-------------------
- Whole day spent to implement hexagon explosion mechanic, just to find out at the end of day that there was an indexing problem
- Exploded hexagon will be counted by UserInterfaceManager to show score



02/04/2018 - Monday
-------------------
- To avoid sprite tweaking and bouncing 2D physics removed from game
- All movement functionalities will be done by using LERP
- Bomb hexagon implemented to appear on each 1000 score to end game if not exploded in 6 turns
- Menu and UI design completed, not beautiful but serves the purpose
- Thanks to changed physics, movement is more smooth and explosion mechanic works fine

As today is the due day it will be end of development for a while, until I find time again to improve and finish the whole project.
This was a hard challenge for me to achieve in 5 days thanks to some other problems. But I'll be continuing as I said above. It will
be independent from Vertigo Games, just to satisfy myself. Here is a missing/TODO list:
- Game is fixed to 5 colors, preparation screen doesn't affect it even with sliders
- Game end check is missing, there is nothing that controls if there is still moves to play
- UI could be more attractive
- Code readability is a little messed on last day due to assignment stress
