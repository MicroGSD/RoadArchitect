
# Warning
Any code relating to the traffic system is in early alpha and not ready for production. Use at own risk.

## Directory
This directory contains experimental traffic implementations not ready for production and contains the code for the traffic project. This will be contained within the Road Architect project due to its heavy integration with the Road Architect system, but should be able to be used on other implementations.

https://github.com/MicroGSD/RoadArchitect/projects/3

## State summary
The code is 4 years old and needs work before including in the repo.

## Features of current implementation
- Hundreds of cars at once across many square km
- Uses forward cached bucket lookups of spline system to facilitate tracing. 
- Except for the forward lookups on the spline, it works very similar to real-life lidar systems.
- Each agent (car) is capable of the following:
  - Traveling from one node to another node.
  - Evasion of objects in the road.
  - Evasion of objects in non-road parts (shoulders) when already evading other objects.
  - Stopping at intersections gracefully with other vehicles.
  - Communicating with other agents on who should go first in situations (object in road).
  - LOD like system which degrades CPU requirements per agent the further they are away from player (single player implementation only).
  -  Pull over to shoulder for arbitrary reasons (emergency etc).

## Technical details
- Each car is their own agent, with vision cones/cylinders via ray casting ( https://docs.unity3d.com/ScriptReference/Physics.Raycast.html )
- This system gracefully degrades if user is further away from vehicles. e.g. they go on "rails" without collision.
- There is caching on spline lookups on Road Architect explicitly  written for the traffic system. "Buckets" are established to facilitate faster lookups. Line ~428 at https://github.com/MicroGSD/RoadArchitect/blob/master/Spline/GSDSplineC.cs#L428 . This caching is handled internally by RA.

## Active Issues
- System was written in early development of RA and needs updated heavily. Organizing and pruning old code is necessary before inclusion of the code files.
- System needs adapted to whatever best vehicle solution (preferably open-source).