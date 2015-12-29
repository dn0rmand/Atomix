using System;
using Foundation;
using System.IO;
using System.Collections.Generic;

namespace Atomix
{
	public class Settings : NSObject
	{
		bool			_dirty = false;

		bool			_soundEnabled = true;
		int				_hiScore = 0;
		HashSet<int>	_completedLevels = new HashSet<int>();

		static Settings _instance = null;
		static string 	_path = null;

		private Settings()
		{
		}

		[Export("initWithCoder:")]
		public Settings(NSCoder coder)
		{
			_dirty = false;
			_soundEnabled = coder.DecodeBool("SoundEnabled");
			_hiScore	  = coder.DecodeInt("HiScore");

			var completed = coder.DecodeObject("Completed") as NSArray;

			if (completed != null)
			{
				for(nuint i = 0 ; i < completed.Count ; i++)
				{
					var number = completed.GetItem<NSNumber>(i);
					var level  = number.Int32Value;

					_completedLevels.Add(level);
				}
			}
		}

		[Export("encodeWithCoder:")]
		public void EncodeTo(NSCoder coder)
		{
			_dirty = false;

			coder.Encode(_soundEnabled, "SoundEnabled");
			coder.Encode(_hiScore, "HiScore");

			var levels = new NSMutableArray((nuint) _completedLevels.Count);

			foreach(int l in _completedLevels)
			{
				var level = NSNumber.FromInt32(l);
				levels.Add(level);
			}

			coder.Encode(levels, "Completed");
		}

		public bool IsLevelCompleted(int level)
		{
			return _completedLevels.Contains(level);
		}

		public void SetLevelCompleted(int level)
		{
			if (! IsLevelCompleted(level)) // Already Completed?
			{
				_completedLevels.Add(level);
				_dirty = true;
			}
		}

		public int HiScore
		{
			get { return _hiScore; }
			set
			{
				_hiScore = value;
				_dirty   = true;
			}
		}

		public bool SoundEnabled 
		{
			get
			{
				return _soundEnabled;
			}
			set
			{
				_soundEnabled = value;
				_dirty = true;
			}
		} 

		static Settings Load()
		{
			Settings settings = null;

			var path = Settings.Path;
			if (File.Exists(path))
			{
				var data = NSData.FromFile(path);

				settings = NSKeyedUnarchiver.UnarchiveObject(data) as Settings;
			}
			if (settings == null)
				settings = new Settings();

			return settings;
		}

		public void Save() // Only if required / dirty.
		{
			if (_dirty)
			{
				NSData data = NSKeyedArchiver.ArchivedDataWithRootObject(this);
				data.Save(Settings.Path, true);
			}
		}

		static string Path
		{
			get
			{
				if (_path == null)
				{
					var path = NSSearchPath.GetDirectories(NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User)[0];

					_path = (string) ((NSString)path).AppendPathComponent((NSString)"game.data");
				}
				return _path;
			}
		}

		public static Settings Instance
		{
			get
			{
				if (_instance == null)
					_instance = Settings.Load();
				return _instance;
			}
		}
	}
}
