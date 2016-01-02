using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;

namespace Atomix
{
	public class CreditsScene : SKScene
	{
		public CreditsScene() : base(Constants.GameSize)
		{
			this.ScaleMode = SKSceneScaleMode.AspectFit;
			this.BackgroundColor = UIColor.FromRGB(0xC3, 0xC3, 0xE3);
		}

		public CreditsScene (IntPtr handle) : base (handle)
		{
		}

		public override void DidMoveToView(SKView view)
		{
			var background = SKSpriteNode.FromImageNamed("InfoScreen");
			background.Position 	= CGPoint.Empty;
			background.AnchorPoint 	= CGPoint.Empty;
			background.ZPosition 	= Constants.FrameZIndex;
			this.Add(background);

			var credits = SKSpriteNode.FromImageNamed("Credits"); 
			credits.Position 	= new CGPoint(0, this.Size.Height);
			credits.AnchorPoint	= new CGPoint(0, 1);
			credits.ZPosition	= Constants.IntroImageZIndex;
			this.Add(credits);

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
			var transition = SKTransition.CrossFadeWithDuration(0.5);
			var intro = new IntroScene();
			this.View.PresentScene(intro, transition);
		}
	}
}

