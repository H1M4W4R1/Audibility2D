# About
This is a Unity3D package used to detect audibility levels of multiple audio sources in 2D (and possibly 3D) space.
Main intent was to create efficient detection of enemy hearing mechanics with walls that dampen sound to make gameplay more engaging.

![Showcase Image](https://github.com/H1M4W4R1/AudibilitySystem-Unity3D/blob/master/Images/screenshot.png)

# Current capabilities
* Audibility2D - Unity Tileset-based system to detect audio propagation including tiles that may muffle sound.
* Audibility3D - Proof of concept of RT-based audio calculation, very inefficient

# Audibility2D
To implement 2D mode you need to create basic project including Unity Tileset and add another Tileset for audio tiles (you can create them from asset menu).
Audio Tiles can have sound dampening material attached - this material describes how efficiently (or inefficiently) sound transission to tile is.

System calculates dampening effects when audio enters tile by subtraction of muffling strength from previously-visited tile loudness. Also takes distance to audio source into account.
You can use `MufflingLevelAnalysisDrawer2D` to draw current tile muffling levels.

To enable computation you need also to add `AudibleSound` to desired GameObjects. This component defines how loud 
audio is and serves as intermediate layer for defining sound source properties.
If you've added your `AudibleSound`(s) you should be able to preview loudness using `AudioSampler2D`.

# Audibility3D
Do not use. Really. Do it on your own risk.

Okay... This thing does not require pretty much anything as it's raycast-based. Unfortunately at this moment audio materials are not supported as they were very expensive to compute.
You can simply add `AudibleSound`(s) to your world and use `AudioSampler3D` to draw loudness levels. This sampler uses 2D grid in XZ-space, however you can move it's Y axis to check audio levels at different height.

Any object that is in raycast mask for audio sources will muffle sound by specified amount (concrete values are used by default and must be changed in script).

# Warranty
This thing is mostly untested for non-common scenarios (e.g. XZ tilemap instead of XY one). Use it at your own risk.
