using System;
using SpriteKit;
using System.Collections.Generic;
using CoreGraphics;
using UIKit;

namespace Atomix
{
	partial class Level : SKNode
	{
		#region Private Fields

		int 			_maxX , _maxY;
		byte[,]			_solution;
		SKSpriteNode	_background;

		#endregion

		private Level(int level)
		{
			Size = CGSize.Empty;
			LevelNumber = level;
			_maxX = _maxY = 0;
		}

		#region Properties

		public string LevelName
		{
			get;
			private set;
		}

		public string LevelDescription
		{
			get;
			private set;
		}

		public int Duration
		{
			get;
			private set;
		}

		public byte Background
		{
			get;
			private set;
		}

		public byte CursorType
		{
			get;
			private set;
		}

		public int LevelNumber 
		{ 
			get; 
			private set; 
		}

		public CGSize Size
		{
			get;
			private set;
		}

		#endregion

		public void RemoveFromScene(bool dispose = true)
		{
			if (_background != null)
			{
				_background.RemoveFromParent();
				_background.Dispose();
				_background = null;
			}

			if (_preview != null)
			{
				_preview.RemoveFromParent();
				_preview.Dispose();
				_preview = null;
			}

			this.RemoveFromParent();

			if (dispose)
				this.Dispose();
		}

		public void AddToScene(SKScene scene)
		{
			if (_background == null)
				_background = GetBackground();

			if (_preview == null)
				_preview = GetPreview();
			
			var s1 = this.Size;
			var s2 = scene.Size;

			var x = (s2.Width - s1.Width)/2;
			var y = (s2.Height- s1.Height)/2;

			this.Position = new CGPoint(x + 40, y - 15);
		
			scene.Add(_background);
			scene.Add(_preview);
			scene.Add(this);
		}

		SKSpriteNode GetBackground()
		{
			var background = SKSpriteNode.FromTexture(Atlases.GetBackground(this.Background));

			background.Position 	= CGPoint.Empty;
			background.AnchorPoint	= CGPoint.Empty;
			background.ZPosition	= Constants.BackgroundZIndex;
			background.UserInteractionEnabled = false;

			return background;
		}

		public override CGRect CalculateAccumulatedFrame ()
		{
			CGPoint pt = this.Position;

			var size = new CGSize((_maxX+1) * Constants.TileWidth, (_maxY+1) * Constants.TileHeight);

			Size = size;

			pt.X -= (size.Width / 2);
			pt.Y -= (size.Height/ 2);

			return new CGRect(pt, size);
		}

		void AddNode(SKSpriteNode node, int x, int y)
		{
			_maxX = Math.Max(x, _maxX);
			_maxY = Math.Max(y, _maxY);

			node.Position = new CGPoint(x * Constants.TileWidth, y * Constants.TileHeight);
			node.AnchorPoint = CGPoint.Empty;
			this.Add(node);
		}

		void Flip()
		{
			var height = this.Size.Height;

			foreach(SKNode node in this.Children)
			{
				var position = node.Position;
				position.Y = height - position.Y;
				node.Position = position;
			}
		}
	}
}

