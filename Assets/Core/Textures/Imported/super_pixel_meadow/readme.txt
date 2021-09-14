//////////
//THANKS//
//////////

Hi there, this is Will from unTied Games! Nice to meet you.

Super huge THANK YOU for downloading this asset... It means a lot to me that you've chosen it for your game!
If you use this asset in your game, give me a shoutout if you can at @untiedgames, so I can follow along on your development journey!
Want to follow me? Sign up for my newsletter! Or follow me using the Twitter / FB / Youtube links below.
Newsletter signup: http://untiedgames.com/signup

Did you know? You can get access to ALL of my assets if you support me on Patreon!
Check it out: http://patreon.com/untiedgames
You read that right! At the $10 tier, you can get access to ALL of my assets. Not your cup of tea? At the $5 tier, you can get access to all my assets that are 1 year old or more. There's also a lot more than just pixel art... I make Youtube videos, indie games, development logs, and more!

MORE LINKS:
Browse my other assets: untiedgames.itch.io/
Watch me make pixel art, games, and more: youtube.com/c/unTiedGamesTV
Follow on Facebook: facebook.com/untiedgames
Follow on Twitter: twitter.com/untiedgames
Visit my blog: untiedgames.com

Thanks again,
- Will

///////////////////
//VERSION HISTORY//
///////////////////

Version 1.1 (10/24/19)
	- Added color theme E (Soul Harvest)
	- Added color theme F (Fall Colors)
	- Happy Halloween! <3

Version 1.0 (7/7/19)
	- Initial release

/////////////////////////
//HOW TO USE THIS ASSET//
/////////////////////////

Hello! Thank you for downloading the Super Pixel Meadow Tileset. Here are a few pointers to help you navigate and make sense of this zip file.

- In the root folder, you will find six style folders. These folders correspond to each tileset color style.

- In the style folders, you will find a folder named PNG. This folder contains all the objects and tiles as descriptively-named PNGs.

- In the style folders, you will also find a folder named spritesheet. This folder contains the spritesheet, and an easy-to-parse metadata format for reading the spritesheet. The metadata format is: asset_name = x y w h.

- UNITY USERS: I recommend adding the individual PNGs to your project rather than using the spritesheets. The spritesheet is aligned to a 16x16 grid, but if you try to slice it in Unity you'll notice not every asset is 16x16 (e.g. the background images, larger slope tiles, etc). So while slicing will work for the basic tiles, please use the already-sliced PNG files in the PNG folders for best results.

- You can help make sense of the labels using the following guide, which applies to all my tilesets.

File name definitions:
	- "terrain"				Part of the tileset's terrain. The player probably stands on this.

	- "left/right/top/bottom"		The left/right/top/bottom wall or edge of the terrain. Directions ALWAYS refer to the surface normal, but if you're not sure take a look at the spritesheet!

	- "terrain_fill"			The inner part of the terrain. It's optional! You can go without it for smaller platforms, or if you want a simpler design.

	- "midground"				Tiles that should be placed behind the terrain and behind the player.

	- "midground_fill"			The inner part of the midground.

	- "midground_shadowed"			A part of the midground that has a shadow on it. Use it to add depth to your level!

	- "_inner"				An inner corner tile. All corners not labeled "inner" are outer corners.

	- "platform"				Part of the terrain that can be used to make a Nx1 platform.

	- "pillar"				Part of the terrain that can be used to make a 1xN pillar.

	TIP: Pillars and platforms can intersect with other terrain and each other! There are special dual inner corner tiles for intersecting non-platform/non-pillar terrain and an intersection tile for platform/pillar intersection.

	- "bg", "bg1", "bg2", etc.		Background. If there's a number, a higher number indicates further back.

	- "fg", "fg1", "fg2", etc.		Foreground. If there's a number, a higher number indicates further forwards.

	- "near", "far"				Usually for backgrounds, specifies which part is closer and which part is further in the distance.

	- "_A", "_B", "_C", etc.		A suffix with a letter indicates there are multiple variants of this object. Objects with letter suffixes are almost always interchangeable, depending on the tileset.
						Don't worry if there's an "A" without a "B"! That means if I include more variants of this object in an update, you won't have to worry about tile name changes.

	- "frame0000", "frame0001", etc.	Filenames like this indicate an animation.
	  or "f0", "f1", etc.

	- "2x2", "4x4", etc.			Indicates the size of this object, in tiles.

SUPER PIXEL MEADOW SPECIFIC TIPS
	- This tileset has grass that extends beyond the boundaries of the main flat ground tile (terrain_top_center and others). A grass tile is included that should be placed on top of these: terrain_topper_A.
	- Due to the way the grass pattern works, two special tiles are necessary to join grassy slopes with left-facing and right-facing walls. They are: terrain_slope_edge_left, terrain_slope_edge_right
	- For color themes D and E, the moon is a separate object (not part of the sky). Just a heads-up.

SUPER PIXEL MEADOW LAYER ORDER (Front to back)
	- FG bushes
	- Terrain
	- FG flowers
	*** DRAW YOUR PLAYER HERE ***
	- BG flowers
	- BG bushes
	- BG trees
	- Midground
	- BG grass near
	- BG grass far
	- BG mountains near
	- BG mountains far
	- BG clouds near
	- BG clouds far
	- (Moon, color theme D only)
	- BG sky

SUPER PIXEL MEADOW HALLOWEEN 2019 UPDATE
	The Halloween 2019 update brings two new themes. Theme F is "Fall Colors", and is pretty straightforward and uses the same layer order as above.
	Theme E is "Soul Harvest", for your spooky needs! In this color theme, the clouds have been repurposed into fog layers. The FG fog layer should be drawn in front of everything. You can play around with the BG fog layer, but I personally like it between the BG trees and midground layers. The fog PNGs are provided at 100% transparency, so you can also feel free to play around with the alpha in-game. In the wallpaper images, these layers are 50% alpha to give you an idea of what it looks like.
	Happy Halloween!

- Pretty much every object and tile is a size multiple of 16x16 pixels. They're made to stick to a 16x16 grid, but you don't necessarily have to stick to a grid if you don't want to.

- Be sure to check out the included example images to see how to put the tileset together.

Any questions?
Email me at contact@untiedgames.com and I'll try to answer as best I can!

-Will