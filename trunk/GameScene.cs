using System;

using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;

namespace Atomix
{
	public class GameScene : SKScene
	{
		Level			_level;
		int				_currentLevel;

		public GameScene(int level) : base(new CGSize(320, 240))
		{
			if (level < Constants.FirstLevel || level > Constants.LastLevel)
				level = Constants.FirstLevel;
			_currentLevel = level;
			_level = null;

			this.ScaleMode = SKSceneScaleMode.AspectFit;
			this.BackgroundColor = UIColor.Black;
		}

		public GameScene (IntPtr handle) : base (handle)
		{
		}

		public override void DidMoveToView (SKView view)
		{
			NextLevel();
		}

		void NextLevel()
		{
			if (_level == null)
			{
				_level = Level.Create(_currentLevel);
			}
			else if (_level.LevelNumber < Constants.LastLevel)
			{
				_level.RemoveFromScene();
				_level = Level.Create(_level.LevelNumber+1);
			}
			else
			{
				return;
			}

			_level.AddToScene(this);
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			// Called when a touch begins
			foreach (var touch in touches) 
			{
				NextLevel();
			}
		}

		public override void Update (double currentTime)
		{
			// Called before each frame is rendered
		}
	}
}

