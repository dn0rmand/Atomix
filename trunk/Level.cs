using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SpriteKit;
using CoreGraphics;
using UIKit;

namespace Atomix
{
	partial class Level : SKNode
	{
		static SKAction _explosionSound = SKAction.PlaySoundFileNamed("Sounds/explode.mp3", false);

		#region Private Fields

		int 				_maxX , _maxY;
		byte[,]				_solution;
		SKSpriteNode		_background;
		IList<SKSpriteNode>	_obstables = new List<SKSpriteNode>();

		#endregion

		private Level(int level)
		{
			Size = CGSize.Empty;
			LevelNumber = level;
			_maxX = _maxY = 0;
		}

		public Level(IntPtr handle) : base(handle) { }

		protected override void Dispose (bool disposing)
		{
			_obstables.Clear();
			base.Dispose (disposing);
		}

		#region Properties

		public IList<SKSpriteNode> Obstables
		{
			get
			{
				return _obstables;
			}
		}

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
			_background.Destroy();
			_background = null;

			_preview.Destroy();
			_preview = null;

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

		public bool CheckSolution()
		{
			int	minX = 16, maxX = 0;
			int minY = 16, maxY = 0;

			var atoms = new Dictionary<Tuple<int, int>, SKAtomNode>();

			foreach(SKNode node in Obstables)
			{
				if (node is SKAtomNode)
				{
					var x = (int)(node.Position.X / Constants.TileWidth);
					var y = (int)(node.Position.Y / Constants.TileWidth);

					atoms.Add(new Tuple<int, int>(x, y), (SKAtomNode)node);

					minX = Math.Min(minX, x);
					minY = Math.Min(minY, y);
					maxX = Math.Max(maxX, x);
					maxY = Math.Max(maxY, y);
				}
			}

			var xl = _solution.GetLength(0);
			var yl = _solution.GetLength(1);

			if ((maxX - minX + 1) != xl)
				return false;
			if ((maxY - minY + 1) != yl)
				return false;

			for(var yy = 0 ; yy < yl ; yy++)
			{
				for(var xx = 0 ; xx < xl ; xx++)
				{
					var value = _solution[xx, yy];

					if (value == 0)
						continue;

					var key = new Tuple<int, int>(xx+minX, maxY-yy);
					SKAtomNode atom;

					if (! atoms.TryGetValue(key, out atom))
						return false;

					if (atom.Value != value)
						return false;
				} 
			}

			return true;
		}

		public async Task Explosion(Action forEachAtom)
		{
			// Build list of Texture for the explosions
			var explosion 	= SKTextureAtlas.FromName("Explosion");
			var count 		= explosion.TextureNames.Length;
			var textures 	= new SKTexture[count];

			for(var i = 0; i < textures.Length; i++)
			{
				var name = "f" + (count-1-i);
				textures[i] = explosion.TextureNamed(name);
			}

			// Explode atom one by one
			foreach(var node in Obstables)
			{
				var atom = node as SKAtomNode;

				if (atom != null)
				{
					SKAction explode;

					var taskSource = new TaskCompletionSource<bool>();

					var animate = SKAction.AnimateWithTextures(textures, 0.35 / count);
					if (Settings.Instance.SoundEnabled)
						explode = SKAction.Group(animate,_explosionSound);
					else
						explode = animate;

					atom.RunAction(explode, () =>
					{
						atom.Hidden = true;
						if (forEachAtom != null)
							forEachAtom();
						taskSource.SetResult(true);
					});

					await taskSource.Task;
				}
			}
		}
	}
}

