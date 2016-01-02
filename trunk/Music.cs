using System;
using Foundation;
using AVFoundation;

namespace Atomix
{
	public static class Music
	{
		static Music()
		{
			Settings.Instance.SoundEnabledChanged += (sender, e) => 
			{
				if (Settings.Instance.SoundEnabled)
					Start();
				else
				{
					Stop(() => {
						if (_backgroundMusic != null)
						{
							_backgroundMusic.Stop();
							_backgroundMusic.Dispose();
							_backgroundMusic = null;
						}
					});
				}
			};
		}

		static AVAudioPlayer	_backgroundMusic = null;
		static NSTimer			_fadder = null;

		const float MaxVolume = 0.5f;

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

		public static void Start()
		{
			if (! Settings.Instance.SoundEnabled)
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
				FadeTo(MaxVolume, 1, null);
			}
		}

		public static void Stop(Action completed = null)
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
	}
}

