using Foundation;
using UIKit;
using SpriteKit;
using AVFoundation;
using System;

namespace Atomix
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
	[Register ("AppDelegate")]
	public class AppDelegate : UIApplicationDelegate
	{
		static AVAudioPlayer	_backgroundMusic = null;
		static NSTimer			_fadder = null;

		public override UIWindow Window 
		{
			get;
			set;
		}

		public static bool SoundEnabled
		{
			get { return Settings.Instance.SoundEnabled; }
			set
			{
				Settings.Instance.SoundEnabled = value;

				if (! value)
				{
					StopMusic(() => {
						if (_backgroundMusic != null)
						{
							_backgroundMusic.Stop();
							_backgroundMusic.Dispose();
							_backgroundMusic = null;
						}
					});
				}
				else
					StartMusic();
			}
		}

		static void FadeTo(float volume, double duration, Action completed = null)
		{
			var startVolume  = _backgroundMusic.Volume;
			var targetVolume = volume;
			var fadeStart 	 = NSDate.Now.SecondsSinceReferenceDate;

			if (_backgroundMusic == null || _backgroundMusic.Volume == targetVolume) // Already there.
			{
				if (completed != null)
					completed();
				return;
			}

			if (_fadder != null)
			{
				_fadder.Invalidate();
				_fadder.Dispose();
			}

			_fadder = NSTimer.CreateRepeatingScheduledTimer(1.0 / 60.0, (t) =>
			{
				var		now  = NSDate.Now.SecondsSinceReferenceDate;
				var		delta= (float) ((now - fadeStart) / duration * (targetVolume - startVolume));

				_backgroundMusic.Volume = startVolume + delta;

				if ((delta > 0 && _backgroundMusic.Volume >= targetVolume) ||
				    (delta < 0 && _backgroundMusic.Volume <= targetVolume))
				{
					_backgroundMusic.Volume = targetVolume;
					if (completed != null)
						completed();
					_fadder.Invalidate();
					_fadder.Dispose();
					_fadder = null;
				}
			});
		}

		public static void StartMusic()
		{
			if (! SoundEnabled)
				return;

			if (_backgroundMusic == null)
			{
				var path = NSBundle.MainBundle.PathForResource("Sounds/title.mp3", string.Empty);
				var url  = new NSUrl(path, false);

				_backgroundMusic = AVAudioPlayer.FromUrl(url);
				if (_backgroundMusic != null)
				{
					_backgroundMusic.NumberOfLoops = -1;

					if (_backgroundMusic.PrepareToPlay())
						_backgroundMusic.Play();
				}
			}
			else
			{
				if (! _backgroundMusic.Playing)
					_backgroundMusic.Play();
				FadeTo(1.0f, 1, null);
			}
		}

		public static void StopMusic(Action completed = null)
		{
			if (_backgroundMusic != null && _backgroundMusic.Playing)
			{
				FadeTo(0.0f, 1, () =>
				{
					if (_backgroundMusic != null)
						_backgroundMusic.Pause();

					if (completed != null)
						completed();
				});
			}
			else if (completed != null)
				completed();
				
		}

		public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
		{
			return true;
		}

		public override void OnActivated (UIApplication application)
		{
			StartMusic();
		}

		public override void OnResignActivation (UIApplication application)
		{
			Settings.Instance.Save(); // Good time to save.
			StopMusic();
		}
	}
}


