using System;
using SpriteKit;
using CoreGraphics;

namespace Atomix
{
	public static class Atlases
	{
		static SKTextureAtlas 	_wallsAtlas = null;
		static SKTextureAtlas 	_atomsAtlas = null;
		static SKTextureAtlas 	_smallAtomsAtlas = null;
		static SKTextureAtlas 	_backgroundsAtlas = null;
		static SKTextureAtlas[] _fontsAtlas = new SKTextureAtlas[3];

		public static SKTextureAtlas GetFont(int index)
		{
			if (index < 1 || index > 3)
				throw new ArgumentException("index");

			if (_fontsAtlas[index] == null)
				_fontsAtlas[index] = SKTextureAtlas.FromName("Font"+index);

			return _fontsAtlas[index];
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

		public static SKTextureAtlas SmallAtoms
		{
			get
			{
				if (_smallAtomsAtlas == null)
					_smallAtomsAtlas = SKTextureAtlas.FromName("SmallAtoms");	
				return _smallAtomsAtlas;
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
			return Walls.TextureNamed("f" + (index & (byte)Field.Index));
		}

		public static SKTexture GetAtom(int index)
		{
			return Atoms.TextureNamed("f" + (index & (byte)Field.Index));
		}

		public static SKTexture GetSmallAtom(int index)
		{
			return SmallAtoms.TextureNamed("f" + (index & (byte)Field.Index));
		}

		public static SKTexture GetFreeTile()
		{
			return Walls.TextureNamed("empty");
		}

		public static SKTexture GetBackground(int level)
		{
			return Backgrounds.TextureNamed("f" + (level % 3));
		}

		// Text Writer

		public static int GetWidth(this string text, int fontIndex = 1)
		{
			var space = (fontIndex == 1 ? 1 : 0);
			var font  = GetFont(fontIndex);

			var width = 0;

			foreach(char c in text.ToUpperInvariant())
			{
				SKTexture		texture = null;

				if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
					texture = font.TextureNamed(new string(c, 1));
				else if (c == ':')
					texture = font.TextureNamed("Colon");

				if (texture == null) // Letter not found!
					texture = font.TextureNamed("Other");

				width += (int) (texture.Size.Width + space);
			}

			return width;
		}

		public static void Write(this SKNode destin, nfloat x, nfloat y, string text, int fontIndex = 1)
		{
			var space = (fontIndex == 1 ? 1 : 0);

			SKTextureAtlas 	font = GetFont(fontIndex);
			foreach(char c in text.ToUpperInvariant())
			{
				SKTexture		texture = null;

				if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
					texture = font.TextureNamed(new string(c, 1));
				else if (c == ':')
					texture = font.TextureNamed("Colon");

				if (texture == null) // Letter not found!
					texture = font.TextureNamed("Other");

				var letter = SKSpriteNode.FromTexture(texture);

				letter.AnchorPoint = CGPoint.Empty;
				letter.Position    = new CGPoint(x, y);
				letter.ZPosition   = destin.ZPosition+1;

				destin.Add(letter);
				x += (int) (texture.Size.Width + space);
			}
		}
	}
}

