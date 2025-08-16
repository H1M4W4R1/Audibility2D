# About

This is a Unity3D package used to detect audibility levels of multiple audio sources in 2D space.
Main intent was to create efficient detection of enemy hearing mechanics with walls that dampen sound to make gameplay
more engaging.

![Showcase Image](https://github.com/H1M4W4R1/AudibilitySystem-Unity3D/blob/master/Images/screenshot.png)

# Current capabilities

* Audibility2D - Unity Tileset-based system to detect audio propagation including tiles that may muffle sound.

# Audibility2D
To implement 2D mode you need to create basic project including Unity Tileset and add another Tileset for audio tiles (
you can create them from asset menu). Audio Tiles can have sound dampening material attached - this material
describes how efficiently (or inefficiently) sound transmission to tile is.

System calculates dampening effects when audio enters tile by subtraction of muffling strength from previously-visited
tile loudness. Also takes distance to audio source into account.
You can use `MufflingLevelAnalysisDrawer` to draw current tile muffling levels.

To enable computation you need also to add `AudibleSound` to desired GameObjects. This component defines how loud
audio is and serves as intermediate layer for defining sound source properties.
If you've added your `AudibleSound`(s) you should be able to preview loudness using `AudioSampler`.

You can implement your own mechanics based on `AudioSampler` script `OnDrawGizmos` method as your reference.

# Warranty
This thing is mostly untested for non-common scenarios (e.g. XZ tilemap instead of XY one). Use it at your own risk.
