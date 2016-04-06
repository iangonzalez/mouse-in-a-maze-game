#Mouse in a maze

This is a game being developed with Unity and C# as a senior project in the Yale Computer Science Department (CPSC 490).

Advisor: Holly Rushmeier.

##Game Overview

- My vision for the game is a maze-based world that the player will have to explore in first person, all the while contending with the demands and map alterations of an unfriendly, powerful being (the game AI) in control of the situation.
- Communication with the AI will occur via a text interface (allowing me to avoid the need for voice actors). 
- The player starts the game in an unfamiliar grey room with 1-4 doors leading out to hallways.
- The hallways lead to identical rooms � these hallways and doors constitute the maze, and will be designed with a scientific simplicity and blandness in mind.
- As the game progresses, the player will have to decide whether or not to comply with the increasingly strange demands issued by a being that seems to be in control of this place. If the player complies, the game will become noticeably easier, with the being providing helpful hints and shortcuts.
- If the player does not comply, the being will become increasingly hostile, thwarting the player at every turn: locking the player in a room for a few minutes, spinning the maze to confuse orientation, moving hallways in the maze if the player is on the right path, and lengthening hallways.
- The game should leave the player with the sense that they must choose between their own agency and winning the game � they can comply in order to win, with the downside that the being will mock them as weak for doing so (and the ending will be less satisfying).

##Technical Challenges

- Graph programming: I will be using my own graph library to create the maze, so I'll have to implement efficient spanning tree and shortest path algorithms.
- Procedural map generation
- Game AI: The AI will be based on some kind of FSM, with decisions based on a series of probabilistic rules associated with each state.

##Guide to the code

- **Assets/Resources** folder. This contains Unity scripts, prefabs, and scenes needed to run the game.
 - **_scripts**: Contains code for creating the maze, the player, the AI, and other Unity specific code.
 - **prefabs**: Contains the prefabs for all game objects.
 - **materials**: Contains game materials.
 - **scenes**: Contains the scene that, when run, starts the game.
 
- **Graph**. This VS solution contains my implementation of a graph class.

- **IntVector2**. This VS solution contains code for a useful struct containing 2 ints, since Unity doesnt currently support C# tuples.

Almost everything else is files that are auto-generated by Unity or Visual Studio.

##Licensing

This is totally open source: Use whatever you want, however you want!

Note that I did not include some of the textures and models needed to run the game. The two asset packages I used from the asset store were Free Steel Ladder Pack and Modular Abandoned Slaughterhouse (both are free in the store, and it shouldnt be too ahrd to get the lamp/ladder models and other textures from there).

##Acknowledgments

A huge thank you to Professor Holly Rushmeier for advising this project.


In addition, thanks to Jasper Flick's Unity Maze tutorial (http://catlikecoding.com/unity/tutorials/maze/); I used this code to get started on this project.