using System;

using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;
using System.Threading.Tasks;

namespace Atomix
{
	public class GameScene : SKScene
	{
		const int		_textX = 8;
		const int 		_sectionSpacing = 42;
		const int		_labelHeight = 20;
		const int 		_firstLabelY = Constants.GameHeight-16;
		const int 		_firstValueY = _firstLabelY - _labelHeight;

		Level			_level;
		int				_currentLevel;
		int				_score = 0;
		int				_startScore= 0;
		int				_time = 0;

		TextNode		_hiScoreNodes = null;
		TextNode		_scoreNodes = null;
		TextNode		_levelNodes = null;
		TextNode		_timeNodes = null;

		public GameScene(int level) : base(Constants.GameSize)
		{
			if (level < Constants.FirstLevel || level > Constants.LastLevel)
				level = Constants.FirstLevel;
			_currentLevel = level;
			_level = null;

			this.ScaleMode = SKSceneScaleMode.AspectFit;
			this.BackgroundColor = UIColor.Black;
		}

		public GameScene (IntPtr handle) : base (handle)
		{
		}

		public override void DidMoveToView (SKView view)
		{			
			// Labels

			this.Write(_textX, _firstLabelY, "HISCORE", 3);
			this.Write(_textX, _firstLabelY - _sectionSpacing, "SCORE", 3);
			this.Write(_textX, _firstLabelY - _sectionSpacing*2, "LEVEL", 3);
			this.Write(_textX, _firstLabelY - _sectionSpacing*3, "TIME", 3);

			// Write Values that are not Level dependent

			WriteHiScore();
			WriteScore();

			// Build Level

			NextLevel();
		}

		void IncrementScore(int value)
		{
			_score += value;
			WriteScore();
			if (_score > Settings.Instance.HiScore)
			{
				Settings.Instance.HiScore = _score;
				WriteHiScore();
			}
		}

		void WriteTime()
		{
			if (_timeNodes != null)
			{
				_timeNodes.Destroy();
				_timeNodes = null;
			}

			var time	= _time > 0 ? _time : 0;
			var seconds = time % 60;
			var minutes = (int)(time / 60);
			var sTime = string.Format("{0}:{1:00}", minutes, seconds);

			_timeNodes = this.Write(_textX, _firstValueY - _sectionSpacing*3, sTime, 2);
		}

		void WriteHiScore()
		{
			if (_hiScoreNodes != null)
			{
				_hiScoreNodes.Destroy();
				_hiScoreNodes = null;
			}
			_hiScoreNodes = this.Write(_textX, _firstValueY, Settings.Instance.HiScore.ToString(), 2);
		}

		void WriteScore()
		{
			if (_scoreNodes != null)
			{
				_scoreNodes.Destroy();
				_scoreNodes = null;
			}
			_scoreNodes = this.Write(_textX, _firstValueY - _sectionSpacing, _score.ToString(), 2);
		}

		void WriteLevelNumber()
		{
			if (_levelNodes != null)
			{
				_levelNodes.Destroy();
				_levelNodes = null;
			}
			if (_level != null)
			{
				var lev = string.Format("{0:00}", _level.LevelNumber);
				_levelNodes = this.Write(_textX, _firstValueY - _sectionSpacing*2, lev, 2);
			}
		}

		void CreateLevel(int level)
		{
			_startScore = _score;
			_level = Level.Create(level);
			_level.AddToScene(this);
			_time = _level.Duration;
			WriteLevelNumber();
			WriteTime();

			// Reset timer

			_nextTick = null;
			_done = false;
			_countDown = null;

			// Allow user to move Atoms

			SKAtomNode.Unlock();
		}

		void NextLevel()
		{
			if (_level == null)
			{
				if (_currentLevel > Constants.FirstLevel)
					_score = Settings.Instance.GetLevelScore(_currentLevel-1); // Start with Score of Previous Level
				CreateLevel(_currentLevel);
			}
			else if (_level.LevelNumber < Constants.LastLevel)
			{
				int level = _level.LevelNumber+1;
				_level.RemoveFromScene();
				CreateLevel(level);
			}
			else
			{
				GotoIntroScene();
			}
		}

		void GotoIntroScene()
		{
			var transition = SKTransition.CrossFadeWithDuration(0.5);
			var intro = new IntroScene();
			this.View.PresentScene(intro, transition);
		}

		public async Task Success()
		{
			SKAtomNode.Lock();
			Settings.Instance.SetLevelCompleted(_level.LevelNumber, _score);

			_done 	  = true;
			_nextTick = null;

			await _level.Explosion(() => { IncrementScore(500); });

			// Count down remaining time and increment Score

			_countDown = new TaskCompletionSource<bool>();
			await _countDown.Task;
			await Task.Delay(500); // Litte delay at the end of the count down

			// Now is a good time to save settings
			Settings.Instance.Save();

			// Move to next Level
			NextLevel();
		}

		void Timeout()
		{
			_done = true;
			SKAtomNode.Lock(); // No more moving atoms

			var timeout = new SKSpriteNode(SKTexture.FromImageNamed("Timeout"));
			timeout.ZPosition = Constants.FrameZIndex;
			timeout.Position = CGPoint.Empty;
			timeout.AnchorPoint = CGPoint.Empty;
			this.Add(timeout);

			var center = new CGPoint(this.Frame.GetMidX(), this.Frame.GetMidY());

			var exitButton = SKButton.Create("ExitButton");

			exitButton.Position  = new CGPoint(center.X, center.Y - _sectionSpacing / 2);
			exitButton.AnchorPoint= new CGPoint(0.5, 0.5);

			var retryButton = SKButton.Create("RetryButton");

			retryButton.Position  = new CGPoint(center.X, center.Y - _sectionSpacing / 2 - _sectionSpacing);
			retryButton.AnchorPoint= new CGPoint(0.5, 0.5);

			this.Add(exitButton);
			this.Add(retryButton);

			exitButton.Clicked += (sender, e) => 
			{
				timeout.Destroy();
				exitButton.Destroy();
				retryButton.Destroy();

				GotoIntroScene();
			};

			retryButton.Clicked += (sender, e) => 
			{
				timeout.Destroy();
				exitButton.Destroy();
				retryButton.Destroy();

				_score = _startScore; // Resert score to what it was at the start of the level
				CreateLevel(_level.LevelNumber);
			};
		}

		public override void WillMoveFromView (SKView view)
		{
			base.WillMoveFromView (view);
			if (_level != null)
				_level.RemoveFromScene(true);
			this.Dispose();
		}

		bool						_done = false;
		TaskCompletionSource<bool>	_countDown = null;
		double? 					_nextTick = null;

		public override void Update (double currentTime)
		{
			if (! _done)
			{
				// Initialize _nextTick if necessary
				if (_nextTick == null)
					_nextTick = currentTime + 1;

				if (_time == 0)
				{
					Timeout();
				}
				else
				{
					// Decrement Timer
					if (currentTime >= _nextTick)
					{
						_nextTick += 1;
						_time	  -= 1;
						WriteTime();
					}
				}
			}
			else if (_countDown != null)
			{
				// Initialize _nextTick if necessary
				if (_nextTick == null)
					_nextTick = currentTime;

				if (_time > 0)
				{
					int points = 0;

					while (_time > 0 && currentTime >= _nextTick)
					{
						_nextTick += 0.025;
						_time 	  -= 1;
						points	  += 10;
					}
					IncrementScore(points);
					WriteTime();
				}
				else
				{
					_countDown.SetResult(true);
					_countDown = null;
				}
			}
		}
	}
}

