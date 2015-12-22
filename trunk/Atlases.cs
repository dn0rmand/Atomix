using System;
using SpriteKit;
using CoreGraphics;

namespace Atomix
{
	public static class Atlases
	{
		static SKTextureAtlas 	_wallsAtlas = null;
		static SKTextureAtlas 	_atomsAtlas = null;
		static SKTextureAtlas 	_backgroundsAtlas = null;
		static SKTextureAtlas 	_font1Atlas = null;

		public static SKTextureAtlas Font1
		{
			get
			{
				if (_font1Atlas == null)
					_font1Atlas = SKTextureAtlas.FromName("Font1");
				return _font1Atlas;
			}
		}

		public static SKTextureAtlas Walls
		{
			get
			{
				if (_wallsAtlas == null)
					_wallsAtlas = SKTextureAtlas.FromName("Walls");
				return _wallsAtlas;
			}
		}

		public static SKTextureAtlas Atoms
		{
			get
			{
				if (_atomsAtlas == null)
					_atomsAtlas = SKTextureAtlas.FromName("Atoms");	
				return _atomsAtlas;
			}
		}

		public static SKTextureAtlas Backgrounds
		{
			get
			{
				if (_backgroundsAtlas == null)
					_backgroundsAtlas = SKTextureAtlas.FromName("Backgrounds");	
				return _backgroundsAtlas;
			}
		}

		public static SKTexture GetWall(int index)
		{
			return Walls.TextureNamed("f" + index);
		}

		public static SKTexture GetFreeTile()
		{
			return Walls.TextureNamed("empty");
		}

		public static SKTexture GetAtom(int index)
		{
			return Atoms.TextureNamed("f" + index);
		}

		public static SKTexture GetBackground(int level)
		{
			return Backgrounds.TextureNamed("f" + (level % 3));
		}

		public static int Width1(this string text)
		{
			SKTextureAtlas 	font = Font1;
			var letter = font.TextureNamed("A");
			return (int)(text.Length * letter.Size.Width);
		}

		public static void Write1(this SKNode destin, nfloat x, nfloat y, string text)
		{
			SKTextureAtlas 	font = Font1;
			foreach(char c in text.ToUpperInvariant())
			{
				SKSpriteNode letter ;

				if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
					letter = SKSpriteNode.FromTexture(font.TextureNamed(new string(c, 1)));
				else
					letter = SKSpriteNode.FromTexture(font.TextureNamed("Other"));

				letter.AnchorPoint = CGPoint.Empty;
				letter.Position    = new CGPoint(x, y);
				letter.ZPosition   = destin.ZPosition+1;

				destin.Add(letter);
				x += (int) (letter.Size.Width);
			}
		}
	}
}

