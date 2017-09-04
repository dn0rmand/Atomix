using System;

using SpriteKit;
using UIKit;
using CoreGraphics;

namespace Atomix
{
	public partial class GameViewController : UIViewController
	{
		public GameViewController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Configure the view.
			var skView = (SKView)View;

#if DEBUG
			skView.ShowsFPS = true;
			skView.ShowsNodeCount = true;
#endif

			/* Sprite Kit applies additional optimizations to improve rendering performance */
			skView.IgnoresSiblingOrder = true;

			GameState.Restore(skView);
		}

		public override bool ShouldAutorotate ()
		{
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations ()
		{
			return UIInterfaceOrientationMask.Landscape;
		}

		public override bool PrefersStatusBarHidden ()
		{
			return true;
		}
	}
}

