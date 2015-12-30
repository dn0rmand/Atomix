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
		const	int		_buttonStartY = 80;

		class GestureRecognizerFilter : UIGestureRecognizerDelegate
		{
			WeakReference<SKScene>	_parent;

			public GestureRecognizerFilter(SKScene parent)
			{
				_parent = new WeakReference<SKScene>(parent);
			}

			public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
			{
				SKScene parent;

				if (_parent.TryGetTarget(out parent))
				{
					CGPoint touchPoint = touch.LocationInNode(parent);

					return (touchPoint.Y > _buttonStartY && touchPoint.Y < parent.Size.Height-_buttonStartY);
				}
				else
					return false;
			}
		}

		SKSpriteNode 				_intro;
		int						 	_index;
		int						 	_level = Constants.FirstLevel;
		SKSpriteNode		 		_preview = null;
		double						_lastRun;
		double						_delay = 0;
		bool						_showingMenu = false;
		bool						_switchingPreview = false;
		bool						_didTouch = false;
		UISwipeGestureRecognizer	_swipeLeftGesture = null;
		UISwipeGestureRecognizer	_swipeRightGesture = null;
		UITapGestureRecognizer		_tapGesture = null;
		GestureRecognizerFilter		_gestureFilter = null;

		public IntroScene() : base(Constants.GameSize)
		{
			this.ScaleMode = SKSceneScaleMode.AspectFit;
			this.BackgroundColor = UIColor.FromRGB(0xC3, 0xC3, 0xE3);
			_index = 0;
		}

		public IntroScene (IntPtr handle) : base (handle)
		{
		}

		SKSpriteNode CreateImage(string name)
		{
			var image = SKSpriteNode.FromImageNamed(name);

			image.Position = new CGPoint(Constants.GameWidth/2, Constants.GameHeight/2);
			image.AnchorPoint = new CGPoint(0.5, 0.5); //.Empty;
			image.ZPosition = Constants.IntroImageZIndex;
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
			newPreview.Position    = new CGPoint(direction * _slidingWidth, 0);

			this.Add(newPreview);

			SKAction moveRight = SKAction.MoveBy(-(direction * _slidingWidth), 0, 0.5);

			_switchingPreview = true;

			if (curPreview != null)
			{
				curPreview.RunAction(moveRight, () => {
					curPreview.Destroy();
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

		void CreateMenu()
		{
			CreateImage("Level-Menu");

			var soundOnOff = SKButton.Create(AppDelegate.SoundEnabled ? "SoundOn" : "SoundOff");
			soundOnOff.Clicked += SwitchSound;

			soundOnOff.Position = new CGPoint(55, _buttonStartY-5);
			soundOnOff.AnchorPoint = new CGPoint(0, 1);

			this.Add(soundOnOff);

			var button = SKButton.Create("StartButton");
			button.Clicked += StartGame;

			var x = (this.Size.Width - button.Size.Width)/2;
			button.Position = new CGPoint(x, _buttonStartY);
			button.AnchorPoint = new CGPoint(0, 1);

			this.Add(button);

			_level   = 1;

			_preview = CreatePreview(_level);
			_preview.AnchorPoint = CGPoint.Empty;
			_preview.Position    = CGPoint.Empty;
			this.Add(_preview);

			// Create Gestures

			_gestureFilter = new GestureRecognizerFilter(this);

			_swipeLeftGesture = new UISwipeGestureRecognizer(HandleSwipeLeft);
			_swipeRightGesture = new UISwipeGestureRecognizer(HandleSwipeRight);

			_swipeLeftGesture.Direction = UISwipeGestureRecognizerDirection.Left;
			_swipeRightGesture.Direction = UISwipeGestureRecognizerDirection.Right;

			_swipeLeftGesture.Delegate = _gestureFilter;
			_swipeRightGesture.Delegate = _gestureFilter;

			View.AddGestureRecognizer(_swipeLeftGesture);
			View.AddGestureRecognizer(_swipeRightGesture);

			_showingMenu = true;
		}

		void DisposeIntro()
		{
			_intro.Destroy();
			_intro = null;

			if (_index > 3)
			{
				_tapGesture.Destroy(View);
				_tapGesture = null;
			}
		}

		void SwitchIntro()
		{
			_index++;

			DisposeIntro();

			if (_index <= 3)
			{
				_intro = CreateImage("Intro-" + _index);
			}
			else
			{
				CreateMenu();
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

			_tapGesture = new UITapGestureRecognizer(HandleTap);
			view.AddGestureRecognizer(_tapGesture);
		}

		public override void WillMoveFromView (SKView view)
		{
			_swipeLeftGesture.Destroy(view);
			_swipeLeftGesture = null;

			_swipeRightGesture.Destroy(view);
			_swipeRightGesture = null;

			_tapGesture.Destroy(view);
			_tapGesture = null;

			base.WillMoveFromView (view);
		}

		void HandleSwipeLeft(UISwipeGestureRecognizer sender)
		{
			if (sender.State == UIGestureRecognizerState.Ended)
			{
				NextPreview();
			}
		}

		void HandleSwipeRight(UISwipeGestureRecognizer sender)
		{
			if (sender.State == UIGestureRecognizerState.Ended)
			{
				PreviousPreview();
			}
		}

		void StartGame(object sender, EventArgs e)
		{
			var transition = SKTransition.CrossFadeWithDuration(0.5);
			var game = new GameScene(_level);
			this.View.PresentScene(game, transition);
		}

		void SwitchSound(object sender, EventArgs e)
		{
			var button = sender as SKButton;

			AppDelegate.SoundEnabled = ! AppDelegate.SoundEnabled;
			if (AppDelegate.SoundEnabled)
				button.Texture = button.NormalTexture = SKTexture.FromImageNamed("SoundOn");
			else
				button.Texture = button.NormalTexture = SKTexture.FromImageNamed("SoundOff");
		}

		void HandleTap(UITapGestureRecognizer sender)
		{
			if (_showingMenu)
			{
				var touchLocation = sender.LocationInView(sender.View);

		        touchLocation = this.ConvertPointFromView(touchLocation);
		        touchLocation.Y = this.Size.Height - touchLocation.Y;
			}
			else // Anywhere is good enough since we're not showing the menu yet.
			{
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
				_intro.ZPosition = Constants.FrameZIndex + 1;
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

