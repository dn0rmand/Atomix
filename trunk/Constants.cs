using System;
using SpriteKit;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace Atomix
{
	enum Field : byte
	{
		Type = 0xC0,
		Index= 0x3F,

		Free = 0x80,
		Atom = 0x40,
		Wall = 0xC0
	}

	static class Constants
	{
		public const float	BackgroundZIndex = 0;
		public const float	PreviewZIndex 	 = 10;
		public const float	FreeZIndex 		 = 20;
		public const float	WallZIndex		 = 30;
		public const float	AtomZIndex		 = 40;

		public const int 	FirstLevel  = 1;
		public const int 	LastLevel	= 30;

		public const int 	TileWidth 	= 16;
		public const int 	TileHeight	= 16;

		public const int	SmallAtomWidth = 8;
		public const int	SmallAtomHeight= 8;
	}
}

