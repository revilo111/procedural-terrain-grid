My program created is a expanable grid system with implemented terrain generation. I have created a video demonstrating its use briefly[1] with most of the created features. 
The procedural generation for the planes (floors and walls) were created by following the playlist[2], however all other features created were implemented and thought by me with help from the unity handbook[3].

The program works by splitting the x,z plane into squares in a grid with every coordinate corresponding to a certain grid square.
For each square, it is either occupied or not, displayed when all vertices of that square are joined creating a plane, and the square becoming a chunk. For each chunk, there is always a floor and 0 to 4 walls surrounding it.
All walls are attached to only one floor, however one floor can have multiple walls. If there are 2 chunks next to one another, there cannot be a wall between them (notably this program ensures an open layout).
Every wall can be destroyed, creating another chunk in the opposite direction to the chunk that the wall belongs to. For each chunk, there are 12 different generated floors all dependent on the amount of walls of neighbouring chunks.
For each chunk, vector 4s are stored of the chunks walls, in the form [0,0,0,0] where each element represents a wall on the north, east, south, west edge respectively, with 1 being a wall in that direction.
Also, the neighbouring chunks' walls are stored in the current chunk so the terrain generation can be calculated. For the floor generation, each floor plane is split into 4 sections and a specific exponental function is applied using the
distance that the x,z coordinate is away from the centre of the chunk or another coordinate set.

Thank you.

[1] - https://www.youtube.com/watch?v=TPfS-HjPlRs
[2] - https://www.youtube.com/watch?v=wbpMiKiSKm8&list=PLFt_AvWsXl0eBW2EiBtl_sxmDtSgZBxB3
[3] - https://docs.unity3d.com/Manual/UnityManual.html