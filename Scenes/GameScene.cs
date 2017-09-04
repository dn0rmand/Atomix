using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using SpriteKit;
using UIKit;
using System.Globalization;

namespace Atomix
{
	using CountDownSource = TaskCompletionSource<bool>;

	public enum Status
	{
		Undefined,
		Starting,
		Running,
		Success,
		StartCountDown,
		CountingDown,
		Finished
	}

	public class GameScene : SKScene
	{
		const int		_textX = 8;
		const int 		_sectionSpacing = 42;
		const int		_labelHeight = 20;
		const int 		_firstLabelY = Constants.GameHeight-16;
		const int 		_firstValueY = _firstLabelY - _labelHeight;

		Level			_level;
		int				_score = 0;
		int				_startScore = 0;
		int				_time = 0;

		CountDownSource	_countDown = null;
		double 			_nextTick;
		bool			_firstRun = true;
		Status			_status = Status.Undefined;

		TextNode		_hiScoreNodes = null;
		TextNode		_scoreNodes = null;
		TextNode		_levelNodes = null;
		TextNode		_timeNodes = null;
		SKButton		_pauseButton = null;
		SKNode			_pausedScreen = null;

		public event EventHandler DidStart;

		public GameScene() : base(Constants.GameSize)
		{
			_level = null;

			this.ScaleMode = SKSceneScaleMode.AspectFit;
			this.BackgroundColor = UIColor.Black;
		}

		public GameScene (IntPtr handle) : base (handle)
		{
		}

		public override void DidMoveToView (SKView view)
		{			
			base.DidMoveToView(view);

			// Disable all old gestures

			if (view.GestureRecognizers != null)
			{
				// Disable all the old Gestures
				foreach(UIGestureRecognizer gesture in view.GestureRecognizers)
				{
					gesture.Enabled = false;
				}
			}

			// Add Pause Button

			_pauseButton = SKButton.Create("Pause");

			_pauseButton.Position = new CGPoint(Constants.GameWidth - _pauseButton.Size.Width, 0);
			_pauseButton.AnchorPoint = CGPoint.Empty;
			_pauseButton.Clicked += (sender, e) =>
			{
				this.Paused = true;
			};

			this.Add(_pauseButton);

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
			Settings.Instance.CurrentLevel = level;

			_startScore = _score;
			if (_level != null)
				_level.RemoveFromScene();
			_level = Level.Create(level);
			_level.AddToScene(this);
			_time = _level.Duration;
			WriteLevelNumber();
			WriteTime();

			// Start Game

			_status = Status.Starting;

			// Allow user to move Atoms

			SKAtomNode.Unlock();
		}

		void NextLevel()
		{
			if (_level == null)
			{
				if (Settings.Instance.CurrentLevel > Constants.FirstLevel)
					_score = Settings.Instance.GetLevelScore(Settings.Instance.CurrentLevel - 1); // Start with Score of Previous Level
				CreateLevel(Settings.Instance.CurrentLevel);
			}
			else if (_level.LevelNumber < Constants.LastLevel)
			{
				int level = _level.LevelNumber+1;
				CreateLevel(level);
			}
			else
			{
				_firstRun = true;
				var transition = SKTransition.CrossFadeWithDuration(0.5);
				var intro = new CreditsScene();
				this.View.PresentScene(intro, transition);
			}
		}

		void GotoMenuScene()
		{
			var transition = SKTransition.CrossFadeWithDuration(0.5);
			var intro = new MenuScene();
			this.View.PresentScene(intro, transition);
		}

		void HidePausedScreen()
		{
			if (_pausedScreen != null)
				_pausedScreen.Hidden = true;
			if (_pauseButton != null)
				_pauseButton.Hidden = false;		
		}

		void ShowPausedScreen()
		{
			if (_pausedScreen == null)
			{
				var black = SKSpriteNode.FromImageNamed("Black");
				black.AnchorPoint = CGPoint.Empty;
				black.Position    = CGPoint.Empty;
				black.ZPosition   = Constants.FrameZIndex + 1;
				this.Add(black);

				var resume = SKButton.Create("Resume");
				resume.Position = new CGPoint(black.Size.Width / 2, black.Size.Height / 2);
				resume.AnchorPoint = new CGPoint(0.5, 0.5);
				black.Add(resume);

				var exitButton = SKButton.Create("ExitButton");

				exitButton.Position   = new CGPoint(black.Size.Width / 2, black.Size.Height / 2);
				exitButton.AnchorPoint= new CGPoint(-1.5, 0.5);
				black.Add(exitButton);

				_pausedScreen = black;

				resume.Clicked += (sender, e) =>
				{
					Paused = false;
				};

				exitButton.Clicked += (sender, e) => 
				{
					GotoMenuScene();
				};
			}

			_pausedScreen.Hidden = false;	

			if (_pauseButton != null)
				_pauseButton.Hidden  = true;		
		}

		void Timeout()
		{
			if (_pauseButton != null)
				_pauseButton.Hidden  = true;		

			_status = Status.Finished;

			SKAtomNode.Lock(); // No more moving atoms

			var timeout = new SKSpriteNode(SKTexture.FromImageNamed("Timeout"));
			timeout.ZPosition = Constants.FrameZIndex;
			timeout.Position = CGPoint.Empty;
			timeout.AnchorPoint = CGPoint.Empty;
			this.Add(timeout);

			var retryButton = SKButton.Create("RetryButton");
			var exitButton = SKButton.Create("ExitButton");

			var center = new CGPoint(this.Frame.GetMidX(), this.Frame.GetMidY());

			retryButton.Position  = new CGPoint(center.X, center.Y - _sectionSpacing);
			retryButton.AnchorPoint= new CGPoint(0.5, 0.5);

			exitButton.Position  = new CGPoint(center.X + retryButton.Size.Width, center.Y - _sectionSpacing);
			exitButton.AnchorPoint= new CGPoint(0.5, 0.5);

			this.Add(exitButton);
			this.Add(retryButton);

			exitButton.Clicked += (sender, e) => 
			{
				GotoMenuScene();
			};

			retryButton.Clicked += (sender, e) => 
			{
				if (_pauseButton != null)
					_pauseButton.Hidden = false;		

				timeout.Destroy();
				exitButton.Destroy();
				retryButton.Destroy();

				_score = _startScore; // Resert score to what it was at the start of the level
				_firstRun = true;
				CreateLevel(_level.LevelNumber);
			};
		}

		public int 		RemainingTime 	{ get { return _time; } }
		public int 		Score		 	{ get { return _score; } }
		public Level	Level		  	{ get { return _level; } }
		public Status	Status			{ get { return _status; } }

		public async Task Success()
		{
			// Set Status as Success
			_status = Status.Success;

			SKAtomNode.Lock();

			await _level.Explosion(() => { IncrementScore(500); });

			// Count down remaining time and increment Score

			_countDown = new CountDownSource();

			_status = Status.StartCountDown;
			await _countDown.Task;
			_status = Status.Finished;

			// Set Level as Completed and save it's Score ( if higher )
			Settings.Instance.SetLevelCompleted(_level.LevelNumber, _score);

			// Now is a good time to save settings
			Settings.Instance.Save();

			// Move to next Level
			NextLevel();
		}

		public override void WillMoveFromView(SKView view)
		{
			base.WillMoveFromView (view);
			if (_level != null)
				_level.RemoveFromScene(true);
			this.Dispose();
		}

		public override bool Paused 
		{
			get 
			{
				return base.Paused;
			}
			set 
			{
				if (base.Paused != value && (_status == Status.Running || _status == Status.CountingDown))
				{
					var currentTime = NSDate.Now.SecondsSinceReferenceDate;
					if (value) 
						_nextTick -= currentTime;
					else
						_nextTick += currentTime;
				}

				if (! _firstRun)
				{
					if (value)
						ShowPausedScreen();
					else
						HidePausedScreen();
				}

				base.Paused = value;
			}
		}

		public override void Update(double currentTime)
		{
			currentTime = NSDate.Now.SecondsSinceReferenceDate; // Use my own currentTime
			switch (_status)
			{
				case Status.Starting:
					_nextTick = currentTime+1;
					_status = Status.Running;
					if (DidStart != null)
						DidStart(this, EventArgs.Empty);
					break;

				case Status.Running:
					_firstRun = false;
					if (_time == 0)
					{
						Timeout();
					}
					else if (currentTime >= _nextTick)
					{
						_nextTick += 1;
						_time     -= 1;
						WriteTime();
					}
					break;

				case Status.Finished:
					break;

				case Status.StartCountDown:
					_nextTick = currentTime;
					_status = Status.CountingDown;
					break;

				case Status.CountingDown:
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
						_status = Status.Finished;
						_countDown.SetResult(true);
					}				
					break;
			}
		}

		public void Restore(GameState state)
		{
			if (_level == null || _level.LevelNumber != state.LevelNumber)
				CreateLevel(state.LevelNumber);

			_time  = state.RemainingTime;
			_score = state.Score;

			var keys = state.Atoms.Keys;

			foreach(NSString key in keys)
			{
				string[] points = ((string)(state.Atoms.ObjectForKey(key) as NSString)).Split(',');
				var x = nfloat.Parse(points[0], CultureInfo.InvariantCulture);
				var y = nfloat.Parse(points[1], CultureInfo.InvariantCulture);

				SKAtomNode atom = _level.GetChildNode(key) as SKAtomNode;
				if (atom != null)
					atom.Position = new CGPoint(x,y);
			}
		}
	}
}

