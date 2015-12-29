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
		public const float  FrameZIndex		 = 500;

		public const int 	FirstLevel  = 1;
		public const int 	LastLevel	= 30;

		public const int 	TileWidth 	= 16;
		public const int 	TileHeight	= 16;

		public const int	SmallAtomWidth = 8;
		public const int	SmallAtomHeight= 8;
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

		public static nfloat DistanceTo(this CGPoint a, CGPoint b)
		{
			var xx = b.X - a.X;
			var yy = b.Y = a.Y;

			var dd = xx*xx + yy*xx;

			return NMath.Sqrt(dd);
		}
	}
}

