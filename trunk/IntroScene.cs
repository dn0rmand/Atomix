using System;

using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;
using System.Collections.Generic;

namespace Atomix
{
	public class IntroScene : SKScene
	{
		const	int 	_slidingWidth = 200;

		SKSpriteNode 				_intro;
		int						 	_index;
		int						 	_level = Constants.FirstLevel;
		SKSpriteNode		 		_preview = null;
		double						_lastRun;
		double						_delay = 0;
		bool						_showingMenu = false;
		bool						_switchingPreview = false;
		bool						_didTouch = false;
		UISwipeGestureRecognizer	_swipeLeftGesture;
		UISwipeGestureRecognizer	_swipeRightGesture;
		UITapGestureRecognizer		_tapGesture;

		public IntroScene() : base(new CGSize(320, 240))
		{
			this.ScaleMode = SKSceneScaleMode.AspectFill;
			this.BackgroundColor = UIColor.FromRGB(0xC3, 0xC3, 0xE3);
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

		void DisposePreview(SKNode preview)
		{
			if (preview != null)
			{
				preview.RemoveFromParent();
				preview.Dispose();
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

		SKSpriteNode CreatePreview(int lev)
		{
			var preview = new SKSpriteNode(UIColor.Clear, this.Size);

			var level = Level.Create(lev);
			level.AddSolution(preview, 0);

			return preview;
		}			

		void SwitchPreview(int direction)
		{
			if (_switchingPreview || ! _showingMenu)
				return;

			_level += direction;
			if (_level < Constants.FirstLevel)
				_level = Constants.LastLevel;
			else if (_level > Constants.LastLevel)
				_level = Constants.FirstLevel;

			var curPreview	= _preview;
			var newPreview = CreatePreview(_level);

			newPreview.AnchorPoint = CGPoint.Empty;
			newPreview.Position    = new CGPoint(-(direction * _slidingWidth), 0);

			this.Add(newPreview);

			SKAction moveRight = SKAction.MoveBy(direction * _slidingWidth, 0, 2);

			_switchingPreview = true;

			if (curPreview != null)
			{
				curPreview.RunAction(moveRight, () => {
					DisposePreview(curPreview);
				});
			}

			if (newPreview != null)
			{
				newPreview.RunAction(moveRight, () => {
					_switchingPreview = false;
				});
			}

			_preview = newPreview;
		}

		void NextPreview()
		{
			SwitchPreview(1);
		}

		void PreviousPreview()
		{
			SwitchPreview(-1);
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

				_level   = 1;
				_preview = CreatePreview(_level);
				_preview.AnchorPoint = CGPoint.Empty;
				_preview.Position    = CGPoint.Empty;
				this.Add(_preview);
				_showingMenu = true;
			}
		}

		public override void DidMoveToView (SKView view)
		{
			var node = SKSpriteNode.FromImageNamed("InfoScreen");
			node.Position = CGPoint.Empty;
			node.AnchorPoint = CGPoint.Empty;
			node.ZPosition = Constants.FrameZIndex;
			this.Add(node);

			_swipeLeftGesture = new UISwipeGestureRecognizer(HandleSwipeLeft);
			_swipeLeftGesture.Direction = UISwipeGestureRecognizerDirection.Left;
			_swipeRightGesture = new UISwipeGestureRecognizer(HandleSwipeRight);
			_swipeRightGesture.Direction = UISwipeGestureRecognizerDirection.Right;
			_tapGesture = new UITapGestureRecognizer(HandleTap);

			view.AddGestureRecognizer(_tapGesture);
			view.AddGestureRecognizer(_swipeLeftGesture);
			view.AddGestureRecognizer(_swipeRightGesture);
		}

		public override void WillMoveFromView (SKView view)
		{
			if (_swipeLeftGesture != null)
			{
				view.RemoveGestureRecognizer(_swipeLeftGesture);
				_swipeLeftGesture.Dispose();
				_swipeLeftGesture = null;
			}

			if (_swipeRightGesture != null)
			{
				view.RemoveGestureRecognizer(_swipeRightGesture);
				_swipeRightGesture.Dispose();
				_swipeRightGesture = null;
			}

			if (_tapGesture != null)
			{
				view.RemoveGestureRecognizer(_tapGesture);
				_tapGesture.Dispose();
				_tapGesture = null;
			}
			base.WillMoveFromView (view);
		}

		void HandleSwipeLeft(UISwipeGestureRecognizer sender)
		{
			Console.WriteLine("Gesture State is " + sender.State.ToString());

			if (sender.State == UIGestureRecognizerState.Ended)
			{
				PreviousPreview();
			}
		}

		void HandleSwipeRight(UISwipeGestureRecognizer sender)
		{
			if (sender.State == UIGestureRecognizerState.Ended)
			{
				NextPreview();
			}
		}

		void StartGame(int level)
		{
			var transition = SKTransition.CrossFadeWithDuration(0.5);
			var game = new GameScene(level);
			this.View.PresentScene(game, transition);
		}

		void HandleTap(UITapGestureRecognizer sender)
		{
			if (_showingMenu)
			{
				var rect 		  = new CGRect(95, 150, 130, 25);
				var touchLocation = sender.LocationInView(sender.View);

		        touchLocation = this.ConvertPointFromView(touchLocation);
		        touchLocation.Y = this.Size.Height - touchLocation.Y;

		        if (rect.Contains(touchLocation))
		        {
		        	StartGame(_level);
			    } 
			}
			else
			{
				var touchLocation = sender.LocationInView(sender.View);

		        touchLocation = this.ConvertPointFromView(touchLocation);

		        Console.WriteLine(touchLocation);

				_didTouch = true;
			}
		}

		public override void Update (double currentTime)
		{
			if (_delay == 0)
			{
				_lastRun = currentTime;
				_delay   = 2;
				_intro   = CreateImage("Logo"); 
			}

			if (! _showingMenu)
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

