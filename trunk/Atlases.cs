using System;
using System.Collections.Generic;
using SpriteKit;
using CoreGraphics;

namespace Atomix
{
	public class TextNode
	{
		List<SKNode>	_letters = new List<SKNode>();

		public TextNode(int font, CGPoint position)
		{
			Font 	 = font;
			Position = position;
		}

		public void Destroy()
		{
			foreach(SKNode letter in _letters)
			{
				letter.RemoveFromParent();
				letter.Dispose();
			}

			_letters.Clear();
			GC.SuppressFinalize(this);
		}

		public int Font 
		{ 
			get; 
			private set; 
		}

		public CGPoint	Position 
		{ 
			get; 
			private set; 
		}

		public void AddLetter(SKNode node)
		{
			_letters.Add(node);
		}
	}

	public static class Atlases
	{
		static SKTextureAtlas 	_buttonsAtlas = null;
		static SKTextureAtlas 	_wallsAtlas = null;
		static SKTextureAtlas 	_atomsAtlas = null;
		static SKTextureAtlas 	_smallAtomsAtlas = null;
		static SKTextureAtlas 	_backgroundsAtlas = null;
		static SKTextureAtlas[] _fontsAtlas = new SKTextureAtlas[3];

		public static bool Contains(this SKTextureAtlas atlas, string name)
		{
			if (atlas == null || string.IsNullOrWhiteSpace(name))
				return false;

			if (! name.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				name += ".png";

			foreach(string texture in atlas.TextureNames)
				if (texture == name)
					return true;

			return false;
		}

		public static SKTexture Get(this SKTextureAtlas atlas, string name)
		{
			if (atlas.Contains(name))
				return atlas.TextureNamed(name);
			else
				return null;
		}

		public static SKTextureAtlas GetFont(int index)
		{
			if (index < 1 || index > 3)
				throw new ArgumentException("index");

			if (_fontsAtlas[index-1] == null)
				_fontsAtlas[index-1] = SKTextureAtlas.FromName("Font"+index);

			return _fontsAtlas[index-1];
		}

		public static SKTextureAtlas Buttons
		{
			get
			{
				if (_buttonsAtlas == null)
					_buttonsAtlas = SKTextureAtlas.FromName("Buttons");
				return _buttonsAtlas;
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

		public static TextNode Write(this SKNode destin, nfloat x, nfloat y, string text, int fontIndex = 1)
		{
			var textNode = new TextNode(fontIndex, new CGPoint(x, y));

			SKTextureAtlas 	font = GetFont(fontIndex);
			foreach(char c in text.ToUpperInvariant())
			{
				SKTexture texture = null;

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
				textNode.AddLetter(letter);
				x += (int) (texture.Size.Width + 1);
			}

			return textNode;
		}
	}
}

