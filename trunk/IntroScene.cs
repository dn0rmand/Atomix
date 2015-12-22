using System;

using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;

namespace Atomix
{
	public class IntroScene : SKScene
	{
		SKSpriteNode _intro;
		int			 _index;

		public IntroScene() : base(new CGSize(320, 240))
		{
			this.ScaleMode = SKSceneScaleMode.AspectFill;
			this.BackgroundColor = UIColor.Purple;
			_index = 0;
		}

		public IntroScene (IntPtr handle) : base (handle)
		{
		}

		void DisposeIntro()
		{
			if (_intro != null)
			{
				_intro.RemoveFromParent();
				_intro.Dispose();
				_intro = null;
			}
		}

		SKSpriteNode CreateImage(string name)
		{
			var image = SKSpriteNode.FromImageNamed(name);

			image.Position = CGPoint.Empty;
			image.AnchorPoint = CGPoint.Empty;
			image.ZPosition = 100;
			this.Add(image);

			return image;
		}

		void SwitchIntro()
		{
			DisposeIntro();

			_index++;

			if (_index <= 3)
			{
				_intro = CreateImage("Intro-" + _index);
			}
			else
			{
				CreateImage("Level-Selection");
				CreateImage("Level-Start");

				_showingMenu = true;
			}
		}

		public override void DidMoveToView (SKView view)
		{
			var node = SKSpriteNode.FromImageNamed("InfoScreen");
			node.Position = CGPoint.Empty;
			node.AnchorPoint = CGPoint.Empty;
			node.ZPosition = 0;
			this.Add(node);
		}

		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			// Called when a touch begins
			foreach (var touch in touches) 
			{
				_didTouch = true;
				if (_showingMenu)
				{
					var transition = SKTransition.CrossFadeWithDuration(0.5);
					var game = new GameScene(1);
					this.View.PresentScene(game, transition);
				}
			}
		}

		double	_lastRun;
		double	_delay = 0;
		bool	_showingMenu = false;
		bool	_didTouch = false;

		public override void Update (double currentTime)
		{
			if (_delay == 0)
			{
				_lastRun = currentTime;
				_delay   = 2;
				_intro   = CreateImage("Logo"); 
			}

			if (_index <= 3)
			{
				var timeSinceLast = currentTime - _lastRun;
				if (timeSinceLast > _delay || _didTouch)
				{
					_didTouch = false;
					_delay    = 5;
					_lastRun  = currentTime;
					SwitchIntro();
				}
			}
		}
	}
}

