using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;

namespace Atomix
{
	public class IntroScene : SKGestureScene
	{
		SKSpriteNode _intro = null;

		int			_index;
		double		_lastRun;
		double		_delay = 0;
		bool		_didTouch = false;

		public IntroScene()
		{
			_index = 0;
		}

		public IntroScene (IntPtr handle) : base (handle)
		{
		}

		void SwitchIntro()
		{
			_index++;
			if (_index <= 3)
			{
				_intro.Destroy();
				_intro = this.CreateImage("Intro-" + _index);
			}
			else
			{
				this.GotoScene(new MenuScene());
			}
		}

		public override void DidMoveToView(SKView view)
		{
			base.DidMoveToView(view);

			var tapGesture = new UITapGestureRecognizer(HandleTap);
			view.AddGestureRecognizer(tapGesture);
		}

		void HandleTap(UITapGestureRecognizer sender)
		{
			_didTouch = true;
		}

		public override void Update (double currentTime)
		{
			if (_delay == 0)
			{
				_lastRun = currentTime;
				_delay   = 2;
				_intro   = this.CreateImage("Logo"); 
				_intro.ZPosition = Constants.LogoZIndex;
			}

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

