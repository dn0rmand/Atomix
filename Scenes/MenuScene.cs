using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;

namespace Atomix
{
	public class MenuScene : SKGestureScene
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

		SKSpriteNode		 		_preview = null;
		bool						_switchingPreview = false;
		UISwipeGestureRecognizer	_swipeLeftGesture = null;
		UISwipeGestureRecognizer	_swipeRightGesture = null;
		GestureRecognizerFilter		_gestureFilter = null;

		public MenuScene()
		{
		}

		public MenuScene(IntPtr handle) : base (handle)
		{
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
			if (_switchingPreview)
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

		public override void DidMoveToView(SKView view)
		{
			base.DidMoveToView(view);

			this.CreateImage("Level-Menu");

			var help 		= SKButton.Create("Help");
			var soundOnOff	= SKButton.Create(Settings.Instance.SoundEnabled ? "SoundOn" : "SoundOff");
			var startButton = SKButton.Create("StartButton");

			_preview = CreatePreview(Settings.Instance.CurrentLevel);

			// Handlers

			help.Clicked += (sender, e) => 
			{
				this.GotoScene(new IntructionsScene());
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

			// Build Preview

			ShowCompleted(_preview, Settings.Instance.CurrentLevel);

			// Create Gestures

			_gestureFilter 		= new GestureRecognizerFilter(this);
			_swipeLeftGesture 	= new UISwipeGestureRecognizer(HandleSwipeLeft);
			_swipeRightGesture	= new UISwipeGestureRecognizer(HandleSwipeRight);

			_swipeLeftGesture.Direction  = UISwipeGestureRecognizerDirection.Left;
			_swipeRightGesture.Direction = UISwipeGestureRecognizerDirection.Right;

			_swipeLeftGesture.Delegate 	= _gestureFilter;
			_swipeRightGesture.Delegate = _gestureFilter;

			View.AddGestureRecognizer(_swipeLeftGesture);
			View.AddGestureRecognizer(_swipeRightGesture);
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
	}
}

