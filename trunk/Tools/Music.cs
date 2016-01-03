using System;
using System.Threading.Tasks;
using Foundation;
using AVFoundation;

namespace Atomix
{
	public static class Music
	{
		static Music()
		{
			Settings.Instance.SoundEnabledChanged += async (sender, e) => 
			{
				if (Settings.Instance.SoundEnabled)
					Start();
				else
				{
					await Stop();
					if (_backgroundMusic != null)
					{
						_backgroundMusic.Stop();
						_backgroundMusic.Dispose();
						_backgroundMusic = null;
					}
				}
			};
		}

		static AVAudioPlayer	_backgroundMusic = null;
		static NSTimer			_fadder = null;

		const float MaxVolume = 0.5f;

		static async Task FadeTo(float volume, double duration)
		{
			var startVolume  = _backgroundMusic.Volume;
			var targetVolume = volume;
			var fadeStart 	 = NSDate.Now.SecondsSinceReferenceDate;

			if (_backgroundMusic == null || _backgroundMusic.Volume == targetVolume) // Already there.
			{
				return;
			}

			if (_fadder != null)
			{
				_fadder.Invalidate();
				_fadder.Dispose();
			}

			var ts = new TaskCompletionSource<bool>();

			_fadder = NSTimer.CreateRepeatingScheduledTimer(1.0 / 60.0, (t) =>
			{
				var		now  = NSDate.Now.SecondsSinceReferenceDate;
				var		delta= (float) ((now - fadeStart) / duration * (targetVolume - startVolume));

				_backgroundMusic.Volume = startVolume + delta;

				if ((delta > 0 && _backgroundMusic.Volume >= targetVolume) ||
				    (delta < 0 && _backgroundMusic.Volume <= targetVolume))
				{
					_backgroundMusic.Volume = targetVolume;
					_fadder.Invalidate();
					_fadder.Dispose();
					_fadder = null;
					ts.SetResult(true);
				}
			});

			await ts.Task;
		}

		public static async Task Start()
		{
			if (! Settings.Instance.SoundEnabled)
				return;

			if (_backgroundMusic == null)
			{
				var path = NSBundle.MainBundle.PathForResource(_musicName, string.Empty);
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
				await FadeTo(MaxVolume, 1);
			}
		}

		public static async Task Stop()
		{
			if (_backgroundMusic != null && _backgroundMusic.Playing)
			{
				await FadeTo(0.0f, 1);
				if (_backgroundMusic != null)
					_backgroundMusic.Pause();
			}
		}

		static string _musicName = "Sounds/title.mp3";

		public static async Task<string> SetMusicName(string name)
		{
			var oldName = _musicName;

			if (_musicName != name)
			{
				await Stop();
				if (_backgroundMusic != null)
				{
					_backgroundMusic.Dispose();
					_backgroundMusic = null;
				}

				_musicName = name;
				await Start();
			}

			return oldName;
		}
	}
}

