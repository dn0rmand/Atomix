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

		SKSpriteNode 				_intro = null;
		SKSpriteNode		 		_preview = null;

		int						 	_index;
		double						_lastRun;
		double						_delay = 0;
		bool						_showingHelp = false;
		bool						_showingIntro = false;
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
			if (_switchingPreview || _showingIntro || _showingHelp)
				return;

			Settings.Instance.CurrentLevel += direction;

			var curPreview = _preview;
			var newPreview = CreatePreview(Settings.Instance.CurrentLevel);

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
					ShowCompleted(newPreview, Settings.Instance.CurrentLevel);
				});
			}

			if (_menuItems != null)
			{
				_menuItems.Remove(curPreview);
				_menuItems.Add(newPreview);
			}

			_preview = newPreview;
		}

		void ShowCompleted(SKSpriteNode preview, int level)
		{
			if (Settings.Instance.IsLevelCompleted(level))
			{
				var completed = SKSpriteNode.FromImageNamed("Completed");
				completed.AnchorPoint = new CGPoint(0.5, 0.5);
				completed.Position	  = new CGPoint(this.Size.Width/2 + 6, this.Size.Height/2 - 6);
				completed.ZPosition   = Constants.ButtonZIndex;
				completed.SetScale(5);
				preview.Add(completed);

				completed.RunAction(SKAction.ScaleTo(1, 0.25));
			}
		}

		void NextPreview()
		{
			SwitchPreview(1);
		}

		void PreviousPreview()
		{
			SwitchPreview(-1);
		}

		IList<SKNode>	_menuItems = null;

		void DestroyMenu()
		{
			_tapGesture.Enabled = true;

			_swipeLeftGesture.Destroy(this.View);
			_swipeLeftGesture = null;

			_swipeRightGesture.Destroy(this.View);
			_swipeRightGesture = null;

			if (_gestureFilter != null)
				_gestureFilter.Dispose();
			_gestureFilter = null;

			if (_preview != null)
			{
				_preview.Paused = true;
				_preview = null;
				_switchingPreview = false;
			}

			if (_menuItems != null)
			{
				foreach(SKNode node in _menuItems)
					node.Destroy();
				_menuItems.Clear();
				_menuItems = null;
			}
		}

		void CreateMenu()
		{
			DestroyMenu();
			DisposeIntro();

			_menuItems = new List<SKNode>();

			var menu		= CreateImage("Level-Menu");
			var help 		= SKButton.Create("Help");
			var soundOnOff	= SKButton.Create(Settings.Instance.SoundEnabled ? "SoundOn" : "SoundOff");
			var startButton = SKButton.Create("StartButton");

			_preview = CreatePreview(Settings.Instance.CurrentLevel);

			// Handlers

			help.Clicked += (sender, e) => 
			{
				DestroyMenu();
				_showingHelp = true;
				_intro = CreateImage("Instructions");
			};

			soundOnOff.Clicked 	+= SwitchSound;
			startButton.Clicked += StartGame;

			// Positions

			help.AnchorPoint 		= new CGPoint(-1, 1);
			soundOnOff.AnchorPoint 	= new CGPoint(2, 1);
			startButton.AnchorPoint = new CGPoint(0.5, 1);
			_preview.AnchorPoint 	= CGPoint.Empty;

			help.Position 	 	 = new CGPoint(this.Size.Width/2, _buttonStartY);
			soundOnOff.Position  = new CGPoint(this.Size.Width/2, _buttonStartY);
			startButton.Position = new CGPoint(this.Size.Width/2, _buttonStartY);
			_preview.Position    = CGPoint.Empty;

			// Add 

			this.Add(help);
			this.Add(soundOnOff);
			this.Add(startButton);
			this.Add(_preview);

			_menuItems.Add(menu);
			_menuItems.Add(help);
			_menuItems.Add(soundOnOff);
			_menuItems.Add(startButton);
			_menuItems.Add(_preview);

			// Build Preview

			ShowCompleted(_preview, Settings.Instance.CurrentLevel);

			// Create Gestures

			_tapGesture.Enabled = false;
			_gestureFilter 		= new GestureRecognizerFilter(this);
			_swipeLeftGesture 	= new UISwipeGestureRecognizer(HandleSwipeLeft);
			_swipeRightGesture	= new UISwipeGestureRecognizer(HandleSwipeRight);

			_swipeLeftGesture.Direction  = UISwipeGestureRecognizerDirection.Left;
			_swipeRightGesture.Direction = UISwipeGestureRecognizerDirection.Right;

			_swipeLeftGesture.Delegate 	= _gestureFilter;
			_swipeRightGesture.Delegate = _gestureFilter;

			View.AddGestureRecognizer(_swipeLeftGesture);
			View.AddGestureRecognizer(_swipeRightGesture);

			_showingIntro = false;
			_showingHelp  = false;
		}

		void DisposeIntro()
		{
			_intro.Destroy();
			_intro = null;

			if (_index > 3)
			{
//				_tapGesture.Destroy(View);
//				_tapGesture = null;
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
			}
		}

		public override void DidMoveToView(SKView view)
		{
			var node = SKSpriteNode.FromImageNamed("InfoScreen");
			node.Position = CGPoint.Empty;
			node.AnchorPoint = CGPoint.Empty;
			node.ZPosition = Constants.FrameZIndex;
			this.Add(node);

			_tapGesture = new UITapGestureRecognizer(HandleTap);
			view.AddGestureRecognizer(_tapGesture);
		}

		public override void WillMoveFromView(SKView view)
		{
			DestroyMenu();

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
			var game = new GameScene();
			this.View.PresentScene(game, transition);
		}

		void SwitchSound(object sender, EventArgs e)
		{
			var button = sender as SKButton;

			Settings.Instance.SoundEnabled = ! Settings.Instance.SoundEnabled;
			if (Settings.Instance.SoundEnabled)
				button.Texture = button.NormalTexture = SKTexture.FromImageNamed("SoundOn");
			else
				button.Texture = button.NormalTexture = SKTexture.FromImageNamed("SoundOff");
		}

		void HandleTap(UITapGestureRecognizer sender)
		{
			if (_showingIntro || _showingHelp)
				// Anywhere is good enough since we're not showing the menu yet.
				_didTouch = true;
		}

		public override void Update (double currentTime)
		{
			if (_delay == 0)
			{
				_lastRun = currentTime;
				_delay   = 2;
				_intro   = CreateImage("Logo"); 
				_intro.ZPosition = Constants.LogoZIndex;
				_showingIntro = true;
			}

			if (_showingIntro)
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
			else if (_showingHelp && _didTouch)
			{
				_didTouch = false;
				CreateMenu();
			}
		}
	}
}

