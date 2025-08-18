<div align="center">
  <h1>Audibility2D • <a href="https://github.com/H1M4W4R1/AudibilitySystem-Unity3D/wiki">GitHub Wiki</a></h1>
  <img src="https://github.com/H1M4W4R1/AudibilitySystem-Unity3D/blob/master/Images/screenshot.png" alt="Preview screenshot"/>
</div>

# About
**Audibility2D** is a Unity3D package designed to detect audibility levels of multiple audio sources in 2D space. Its primary purpose is to enable efficient enemy-hearing mechanics, with walls and other obstacles that dampen sound, creating more engaging gameplay.

# Implementation
To set up 2D mode:

1. Create a basic Unity project with a standard `Tileset` (2D Tilemap Editor package)
2. Add a secondary `Tileset` for audio tiles (these can be created from the Asset menu).  
   Each **Audio Tile** can have an optional sound-dampening material attached, which defines how effectively sound propagates through it.

The system calculates dampening by subtracting the muffling strength of the current tile from the loudness of previously visited tiles, while also factoring in distance to the audio source. For visualization, you can use `MufflingLevelAnalysisDrawer` to display current tile muffling levels.

To define sound sources, attach the `AudibleSound` component to desired GameObjects. This component specifies the source’s loudness. Once added, you can preview audibility levels using the `AudibilitySampler`. Beware that only Audibile Sounds that are located within tileset area will be considered for calculations.

You can also base on the `OnDrawGizmos` method in `AudibilitySampler` to implement custom mechanics or visualization tools.

# Known Limitations
- Full-world computations are efficient, but rendering debug gizmos for all tiles can significantly impact performance. It is recommended to visualize only selected nodes.  
- You can improve performance by processing only a limited section of the tileset around your entity. However, this requires using low-level API.
- The system is primarily tested on XY tilemaps; other layouts (e.g., XZ) may behave unpredictably.

# Warranty
Audibility2D is experimental for non-standard use cases. Use at your own risk.
