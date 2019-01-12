using System;
using System.Collections.Generic;
using SpriteKit;
using CoreGraphics;
using UIKit;

namespace Atomix
{
	partial class Level : SKNode
	{
		#region Private Fields

		SKSpriteNode	_preview;

		#endregion

		public void AddSolution(SKSpriteNode parent, int offset = 8)
		{
			var maxX = _solution.GetLength(0);
			var maxY = _solution.GetLength(1);

			var width = maxX * Constants.SmallAtomWidth;
			var height = maxY * Constants.SmallAtomHeight;

			var positionX = (parent.Size.Width - width)/2;
			var positionY = ((parent.Size.Height + height) / 2) - offset; // - 1 row of text for molecule title.
			 
			for (var y = 0; y < maxY; y++)
			{
				for (var x = 0; x < maxX; x++)
				{
					var atomIndex = _solution[x,y];
					if (atomIndex == 0)
						continue;

					var texture = Atlases.GetSmallAtom(atomIndex);
					var atom = SKSpriteNode.FromTexture(texture);

					atom.Position 	 = new CGPoint(positionX + (x * Constants.SmallAtomWidth), positionY - (y * Constants.SmallAtomHeight));
					atom.AnchorPoint = new CGPoint(0, 1);
					atom.ZPosition   = Constants.AtomZIndex;

					parent.Add(atom);
				}
			}
		}

		SKSpriteNode GetPreview()
		{
			var preview = SKSpriteNode.FromImageNamed("Preview");

			preview.AnchorPoint = CGPoint.Empty;
			preview.Position	= CGPoint.Empty;
			preview.ZPosition	= Constants.PreviewZIndex;

			var y = preview.Size.Height - 18;
			var x = (preview.Size.Width - this.LevelName.GetWidth())/2;

			preview.Write(x, y, this.LevelName);

			AddSolution(preview);

			return preview;
		}

//		void AddNode(SKSpriteNode node, int x, int y)
//		{
//			_maxX = Math.Max(x, _maxX);
//			_maxY = Math.Max(y, _maxY);
//
//			node.Position = new CGPoint(x * Constants.TileWidth, y * Constants.TileHeight);
//			node.AnchorPoint = CGPoint.Empty;
//			this.Add(node);
//		}

//		void Flip()
//		{
//			var height = this.Size.Height;
//
//			foreach(SKNode node in this.Children)
//			{
//				var position = node.Position;
//				position.Y = height - position.Y;
//				node.Position = position;
//			}
//		}
	}
}

