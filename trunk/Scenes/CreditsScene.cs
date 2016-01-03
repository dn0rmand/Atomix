using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;

namespace Atomix
{
	public class CreditsScene : SKGestureScene
	{
		public CreditsScene()
		{
		}

		public CreditsScene (IntPtr handle) : base (handle)
		{
		}

		public override void DidMoveToView(SKView view)
		{
			base.DidMoveToView(view);

			var credits = this.CreateImage("Credits"); 
			credits.Position 	= new CGPoint(0, this.Size.Height);
			credits.AnchorPoint	= new CGPoint(0, 1);

			var tapGesture = new UITapGestureRecognizer(HandleTap);
			view.AddGestureRecognizer(tapGesture);

			var delay  		= SKAction.WaitForDuration(5);
			var scrollUp 	= SKAction.MoveBy(new CGVector(0, credits.Size.Height/2), 1);
			var scrollDown 	= SKAction.MoveBy(new CGVector(0, -credits.Size.Height/2), 1);

			var actions = SKAction.Sequence(delay, scrollUp, delay, scrollDown);

			Action reRun = null;

			reRun = () => { credits.RunAction(actions, reRun); };

			reRun();
		}

		void HandleTap(UITapGestureRecognizer sender)
		{
			this.GotoScene(new MenuScene());
		}
	}
}

