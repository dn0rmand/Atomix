using System;
using Foundation;
using System.IO;
using System.Collections.Generic;

namespace Atomix
{
	public class Settings : NSObject
	{
		const string	SoundEnabledKey = "SoundEnabled";
		const string	HiScoreKey		= "HiScore";
		const string	LevelScoresKey	= "Scores";

		bool					_dirty = false;
		bool					_soundEnabled = true;
		int						_hiScore = 0;
		int						_currentLevel = Constants.FirstLevel;
		Dictionary<int, int>	_completedLevels = new Dictionary<int, int>();

		static Settings 		_instance = null;
		static string 			_path = null;

		private Settings()
		{
		}

		[Export("initWithCoder:")]
		public Settings(NSCoder coder)
		{
			_dirty = false;
			_soundEnabled = coder.DecodeBool(SoundEnabledKey);
			_hiScore	  = coder.DecodeInt(HiScoreKey);

			var completed = coder.DecodeObject(LevelScoresKey) as NSDictionary;

			if (completed != null)
			{
				foreach(NSObject key in completed.Keys)
				{
					var nsLevel = key as NSNumber;
					var nsScore = completed.ObjectForKey(key) as NSNumber;

					 _completedLevels[nsLevel.Int32Value] = nsScore.Int32Value; 
				}
			}
		}

		[Export("encodeWithCoder:")]
		public void EncodeTo(NSCoder coder)
		{
			_dirty = false;

			coder.Encode(_soundEnabled, SoundEnabledKey);
			coder.Encode(_hiScore, HiScoreKey);

			var levels = new NSMutableDictionary();

			foreach(KeyValuePair<int, int> entry in _completedLevels)
			{
				var level = NSNumber.FromInt32(entry.Key);
				var score = NSNumber.FromInt32(entry.Value);
				levels.Add(level, score);
			}

			coder.Encode(levels, LevelScoresKey);
		}

		public int CurrentLevel
		{
			get { return _currentLevel; }
			set 
			{ 
				if (value < Constants.FirstLevel)
					value = Constants.LastLevel;
				else if (value > Constants.LastLevel)
					value = Constants.FirstLevel;

				_currentLevel = value; 
			}
		}

		public bool IsLevelCompleted(int level)
		{
			return GetLevelScore(level) > 0;
		}

		public int GetLevelScore(int level)
		{
			int score;

			if (_completedLevels.TryGetValue(level, out score))
				return score ;
			else
				return 0;
		}

		public void SetLevelCompleted(int level, int score)
		{
			int oldScore = GetLevelScore(level);

			if (oldScore < score)
			{
				_dirty = true;
				_completedLevels[level] = score;
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

		public event EventHandler SoundEnabledChanged;

		public bool SoundEnabled 
		{
			get
			{
				return _soundEnabled;
			}
			set
			{
				if (value != _soundEnabled)
				{
					_soundEnabled = value;
					_dirty = true;
					var handler = SoundEnabledChanged;
					if (handler != null)
						handler(this, EventArgs.Empty);
				}
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
