using System;
using Foundation;
using UIKit;
using SpriteKit;

namespace Atomix
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		public override UIWindow Window 
		{
			get;
			set;
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			return true;
		}

		public override void OnActivated (UIApplication application)
		{
			Music.Start();
			var game = CurrentGame;

			if (game != null && game.View != null)
			{
				this.Invoke(() => {
					game.View.Paused = false;
					game.Paused = true;
				}, 0.5);
			}
		}

		public override void OnResignActivation (UIApplication application)
		{
			var game = CurrentGame;

			if (game != null && game.View != null)
			{
				game.View.Paused = true;
				game.Paused = true;
			}

			Music.Stop();
		}

		GameScene CurrentGame
		{
			get
			{
				if (this.Window != null)
				{
					var controller = this.Window.RootViewController as GameViewController;
					if (controller != null)
					{
						var view = controller.View as SKView;
						if (view != null)
							return view.Scene as GameScene;
					}
				}

				return null;
			}
		}
	}
}


