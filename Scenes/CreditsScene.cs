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

			Music.SetMusicName("Sounds/end.mp3");

			var credits = this.CreateImage("Credits"); 
			credits.Position 	= new CGPoint(this.Size.Width/2, this.Size.Height-credits.Size.Height/2);
			credits.AnchorPoint	= new CGPoint(0.5, 0.5);

			var tapGesture = new UITapGestureRecognizer(HandleTap);
			view.AddGestureRecognizer(tapGesture);

			var delay  	= SKAction.WaitForDuration(5);
			var rotate  = SKAction.RotateByAngle(NMath.PI, 1);

			var actions = SKAction.Sequence(delay, rotate);

			Action reRun = null;

			reRun = () => { credits.RunAction(actions, reRun); };

			reRun();
		}

		void HandleTap(UITapGestureRecognizer sender)
		{
			Music.SetMusicName("Sounds/title.mp3");
			this.GotoScene(new MenuScene());
		}
	}
}

