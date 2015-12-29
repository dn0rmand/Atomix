using System;
using SpriteKit;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Atomix
{
	public class SKWallNode : SKSpriteNode
	{
		public SKWallNode(IntPtr handle) : base(handle)
		{
		}

		SKWallNode(SKTexture texture) : base(texture)
		{
		}

		public static SKWallNode Create(byte value)
		{
			var texture = Atlases.GetWall(value);
			var wall 	= new SKWallNode(texture);
			wall.ZPosition = Constants.WallZIndex;
			wall.UserInteractionEnabled = false;

			return wall;
		}
	}

	public class SKFreeNode : SKSpriteNode
	{
		public SKFreeNode(IntPtr handle) : base(handle)
		{
		}

		SKFreeNode(SKTexture texture) : base(texture)
		{
		}

		public SKFreeNode() : base(Atlases.GetFreeTile())
		{
			ZPosition = Constants.FreeZIndex;
			UserInteractionEnabled = false;
		}
	}

	public class SKAtomNode : SKSpriteNode
	{		
		static bool _locked = false;
		static bool _autoLock = false;

		CGPoint? 	_startPosition = null;
		CGPoint		_startTouch;
		CGPoint		_lastTouch;

		bool		_horizontalMove 	= false;
		bool		_verticalMove 		= false;

		nfloat		_maxX, _maxY, _minX, _minY;

		public SKAtomNode(IntPtr handle) : base(handle)
		{
		}

		SKAtomNode(SKTexture texture) : base(texture)
		{
		}

		public static SKAtomNode Create(byte value)
		{
			var atom = new SKAtomNode(Atlases.GetAtom(value));

			atom.Value = value;
			atom.ZPosition = Constants.AtomZIndex;
			atom.UserInteractionEnabled = true;

			return atom;
		}

		public static void Unlock()
		{
			_autoLock 	= false;
			_locked 	= false;
		}

		public static void Lock()
		{
			_locked = true;
		}

		public static bool Locked
		{
			get { return _locked || _autoLock; }
		}

		public byte Value 
		{ 
			get; 
			private set; 
		}

		void CalculateMinMaxPositions()
		{
			_maxX = this.Scene.Size.Width+1;
			_minX = -1;
			_maxY = this.Scene.Size.Height+1;
			_minY = -1;

			var level = this.Parent as Level;

			// Search Max X

			foreach(SKSpriteNode obstable in level.Obstables)
			{
				if (obstable == this)
					continue;

				if (obstable.Position.Y == this.Position.Y) // Same Row
				{
					if (obstable.Position.X > this.Position.X && obstable.Position.X < _maxX)
						_maxX = obstable.Position.X;
					if (obstable.Position.X < this.Position.X && obstable.Position.X > _minX)
						_minX = obstable.Position.X;
				}

				if (obstable.Position.X == this.Position.X) // Same Column
				{
					if (obstable.Position.Y > this.Position.Y && obstable.Position.Y < _maxY)
						_maxY = obstable.Position.Y;
					if (obstable.Position.Y < this.Position.Y && obstable.Position.Y > _minY)
						_minY = obstable.Position.Y;
				}
			}

			if (_minX < 0)
				_minX = this.Position.X;
			else
				_minX += Constants.TileWidth;

			if (_minY < 0)
				_minY = this.Position.Y;
			else
				_minY += Constants.TileHeight;

			if (_maxX > this.Scene.Size.Width)
				_maxX = this.Position.X;
			else
				_maxX -= Constants.TileWidth;

			if (_maxY > this.Scene.Size.Height)
				_maxY = this.Position.Y;
			else
				_maxY -= Constants.TileHeight;
		}

		public override void TouchesBegan (Foundation.NSSet touches, UIKit.UIEvent evt)
		{
			base.TouchesBegan (touches, evt);

			// Initialize

			_startPosition = null;
			_verticalMove = _horizontalMove = false; 

			if (Locked)
				return;

			// Set State

			var touch = touches.AnyObject as UITouch;
			var touchPoint = touch.LocationInNode(this.Parent);
			if (this.Frame.Contains(touchPoint))
			{
				_startPosition = this.Position;
				_startTouch    = touchPoint;
				_lastTouch	   = touchPoint;
				CalculateMinMaxPositions();
			}
		} 

		public override void TouchesCancelled (Foundation.NSSet touches, UIKit.UIEvent evt)
		{
			base.TouchesCancelled (touches, evt);

			// Move back to original Position

			if (_startPosition != null)
				this.Position = _startPosition.Value;

			_startPosition = null;
			_verticalMove  = _horizontalMove = false; 
		}

		public override async void TouchesEnded (Foundation.NSSet touches, UIKit.UIEvent evt)
		{
			base.TouchesEnded (touches, evt);

			if (_startPosition != null && ! Locked)
			{
				var touchPoint	= this.Position;
				var xDelta 		= _lastTouch.X - _startTouch.X;
				var yDelta 		= _lastTouch.Y - _startTouch.Y;
				var endPos		= _startPosition.Value;
				var level 		= (Level)Parent;
				double speed	= 0;

				// Need to move by at least 4 pixels

				if (_horizontalMove && NMath.Abs(xDelta) >= 4) 
				{
					if (xDelta > 0)
						endPos.X = _maxX;
					else
						endPos.X = _minX;

					speed = NMath.Abs(endPos.X - this.Position.X) / level.Size.Width;
				}
				else if (_verticalMove && NMath.Abs(yDelta) >= 4) 
				{
					if (yDelta > 0)
						endPos.Y = _maxY;
					else
						endPos.Y = _minY;

					speed = NMath.Abs(endPos.Y - this.Position.Y) / level.Size.Height;
				}

				if (speed > 0)
				{
					if (speed > 2)
					{
						// Just in case!!!
						Console.WriteLine("Too Slow!!!");
						speed = 2;
					}

					_autoLock = true; // Lock while moving

					var taskSource = new TaskCompletionSource<bool>();

					this.RunAction( SKAction.MoveTo(endPos, speed), () => { taskSource.SetResult(true); });

					await taskSource.Task; // Wait for move to be done

					_autoLock = false;
				}
				else
				{
					this.Position = endPos;
				}

				if (level.CheckSolution())
				{
					var game = (GameScene) this.Scene;
					await game.Success();
				}
			}

			_startPosition = null;
			_verticalMove = _horizontalMove = false; 
		}

		public override void TouchesMoved (Foundation.NSSet touches, UIKit.UIEvent evt)
		{
			base.TouchesMoved (touches, evt);

			if (_startPosition != null && ! Locked) // Check that we did begin
			{
				var origin 		= _startPosition.Value;
				var touch 		= touches.AnyObject as UITouch;
				var touchPoint	= touch.LocationInNode(this.Parent);

				_lastTouch = touchPoint;

				var xDelta = touchPoint.X - _startTouch.X;
				var yDelta = touchPoint.Y - _startTouch.Y;

				// Decide which direction to go

				//if (NMath.Abs(xDelta) >= 4 || NMath.Abs(yDelta) >= 4)
				{
					if (! _horizontalMove && ! _verticalMove)
					{
						if (NMath.Abs(xDelta) >= NMath.Abs(yDelta))
							_horizontalMove = true;
						else
							_verticalMove = true;
					}
				}
//				else
//				{
//					_horizontalMove = false;
//					_verticalMove   = false;
//				}

				if (_horizontalMove)// || NMath.Abs(xDelta) > NMath.Abs(yDelta))
				{
					touchPoint.X -= Constants.TileHeight / 2;
					touchPoint.Y = origin.Y;
				}
				else if (_verticalMove)
				{
					touchPoint.X = origin.X;
					touchPoint.Y -= Constants.TileHeight / 2;
				}

				touchPoint.X = NMath.Min(NMath.Max(touchPoint.X, _minX), _maxX);
				touchPoint.Y = NMath.Min(NMath.Max(touchPoint.Y, _minY), _maxY);

				this.Position = touchPoint;
			}
		}
	}
}

