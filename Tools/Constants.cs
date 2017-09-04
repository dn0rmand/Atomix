using System;
using System.Collections.Generic;
using SpriteKit;
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
		public const int	GameWidth = 320;
		public const int	GameHeight= 240;

		public const float	BackgroundZIndex = 0;
		public const float	PreviewZIndex 	 = 10;
		public const float	FreeZIndex 		 = 20;
		public const float	WallZIndex		 = 30;
		public const float	AtomZIndex		 = 40;
		public const float	IntroImageZIndex = 50;
		public const float  FrameZIndex		 = 60;
		public const float	LogoZIndex		 = 70;
		public const float	ButtonZIndex	 = 80;

		public const int 	FirstLevel  = 1;
		public const int 	LastLevel	= 30;

		public const int 	TileWidth 	= 16;
		public const int 	TileHeight	= 16;

		public const int	SmallAtomWidth = 8;
		public const int	SmallAtomHeight= 8;

		public static readonly CGSize GameSize = new CGSize(GameWidth, GameHeight);
	}

	static class Extension
	{
		public static void Destroy(this SKNode node)
		{
			if (node != null)
			{
				node.RemoveFromParent();
				node.Dispose();
			}
		}

		public static void Destroy(this UIGestureRecognizer gesture, UIView parentView)
		{
			if (gesture != null)				
			{
				parentView.RemoveGestureRecognizer(gesture);
				gesture.Delegate = null;
				gesture.Dispose();
			}
		}
	}
}

