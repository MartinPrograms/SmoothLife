# SmoothLife
A gpu-accelerated implementation of SmoothLife, a continuous version of Conway's Game of Life.  
All in C# and OpenGl.  
The values have yet to be tweaked, so it might not look as good as it could, but it works.

## What is SmoothLife?
SmoothLife is a continuous version of Conway's Game of Life.
https://conwaylife.com/wiki/OCA:SmoothLife  
https://www.youtube.com/watch?v=KJe9H6qS82I

## How to use
Clone this repo, open the solution in Visual Studio, and run the project.  
It does NOT work on Mac, as it does not support OpenGL 4.3 required for compute shaders.  

## Controls
- Right click to pan
- Scroll to zoom
- Space to pause
- R to randomize
- C to clear
- S to step once
- Click to add cells
- Shift-click to remove cells (you can change the radius in the controls window)
- Escape to close the program  

Pressing export, will save every next step's result to an image.