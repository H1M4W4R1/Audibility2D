# About

This is a Unity3D package used to detect audibility levels of multiple audio sources in 2D space.
Main intent was to create efficient detection of enemy hearing mechanics with walls that dampen sound to make gameplay
more engaging.

![Showcase Image](https://github.com/H1M4W4R1/AudibilitySystem-Unity3D/blob/master/Images/screenshot.png)

# Implementation
To implement 2D mode you need to create basic project including Unity `Tileset` and add another `Tileset` for audio tiles (
you can create them from asset menu). Audio Tiles can have sound dampening material attached - this material
describes how efficiently (or inefficiently) sound transmission to tile is.

System calculates dampening effects when audio enters tile by subtraction of muffling strength from previously-visited
tile loudness. Also takes distance to audio source into account.
You can use `MufflingLevelAnalysisDrawer` to draw current tile muffling levels.

To enable computation you need also to add `AudibleSound` to desired GameObjects. This component defines how loud
audio is and serves as intermediate layer for defining sound source properties.
If you've added your `AudibleSound`(s) you should be able to preview loudness using `AudibilitySampler`.

You can implement your own mechanics based on `AudibilitySampler` script `OnDrawGizmos` method as your reference.

# Known limitations
Even if computation of entire world is quite efficient you can lose a lot of performance when trying to access whole data to render
e.g. debug gizmos. It is heavily recommended to use only selected nodes instead of all of them as scale matters.

You can also use low-level method to access limited tileset section near your entity to improve performance as any sources or audio tiles
outside of bounds won't be taken into account. Unfortunately that requires creating a few helper methods to compute all necessary data
as currently implemented ones allow only to process all nodes in tileset at once.

# Warranty
This thing is mostly untested for non-common scenarios (e.g. XZ tilemap instead of XY one). Use it at your own risk.
