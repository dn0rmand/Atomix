using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;

namespace Atomix
{
	public class IntructionsScene : SKGestureScene
	{
		public IntructionsScene()
		{
		}

		public IntructionsScene(IntPtr handle) : base (handle)
		{
		}

		public override void DidMoveToView(SKView view)
		{
			base.DidMoveToView(view);

			this.CreateImage("Instructions");

			var tapGesture = new UITapGestureRecognizer(HandleTap);
			view.AddGestureRecognizer(tapGesture);
		}

		void HandleTap(UITapGestureRecognizer sender)
		{
			this.GotoScene(new MenuScene());
		}
	}
}

