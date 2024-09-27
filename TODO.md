* Move Player configuration from Game.xml to Player.xml
* Add description field to sprites, animations, etc.
* Animation frame definition should point to a sprite, rather than part of a texture.
* Weapon collision box should be set in the weapon definition, not for each level.
* Move animation definitions from the general library, into the entity. E.g. the frames of the minigun animations should be defined together with the rest of the weapon definition. (Might remove the need for explicit animation ids. An actor e.g. only cares about which state each animation definition corresponds to).
* Do not require all guns to have a warmup and fire animation.
* Change debug rendering of tile collision box to consider which sides the tile can collide on.