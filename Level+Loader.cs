using System;
using System.Collections.Generic;
using SpriteKit;
using CoreGraphics;
using UIKit;

namespace Atomix
{
	partial class Level
	{
		static byte[] GetLevelData(int level)
		{
			switch (level)
			{
				case  1: return Level1;
				case  2: return Level2;
				case  3: return Level3;
				case  4: return Level4;
				case  5: return Level5;
				case  6: return Level6;
				case  7: return Level7;
				case  8: return Level8;
				case  9: return Level9;
				case 10: return Level10;
				case 11: return Level11;
				case 12: return Level12;
				case 13: return Level13;
				case 14: return Level14;
				case 15: return Level15;
				case 16: return Level16;
				case 17: return Level17;
				case 18: return Level18;
				case 19: return Level19;
				case 20: return Level20;
				case 21: return Level21;
				case 22: return Level22;
				case 23: return Level23;
				case 24: return Level24;
				case 25: return Level25;
				case 26: return Level26;
				case 27: return Level27;
				case 28: return Level28;
				case 29: return Level29;
				case 30: return Level30;

				default:
					throw new ArgumentException("level");
			}
		}

		class DataReader
		{
			byte[] 	_data;
			int		_index;

			public DataReader(int level)
			{
				_data  = Level.GetLevelData(level);
				_index = 0;
			}

			public byte Byte
			{
				get
				{
					return _data[_index++];
				}
			}

			public int Integer
			{
				get
				{
					var v1 = (int) Byte;
					var v2 = (int) Byte;

					return (v1 << 8) + v2;
				}
			}

			public string String
			{
				get
				{
					var bytes = new List<byte>(16);

					for(var i = 0; i < 15; i++)
					{
						var b = this.Byte;
						if (b != 0)
							bytes.Add(b);
					}

					return System.Text.Encoding.ASCII.GetString(bytes.ToArray());
				}
			}
		}

		void LoadSolution(DataReader reader)
		{
			int maxX = 0, minX = 16;
			int maxY = 0, minY = 16;

			byte[,]	solution = new byte[16,16];

			for (int y = 0 ; y < 16 ; y++)
			{
				for (int x = 0 ; x < 16 ; x++)
				{
					var value = reader.Byte;

					if ((value & (byte)Field.Atom) == (byte)Field.Atom)
					{
						solution[x,y] = value;

						minX = Math.Min(x, minX); 
						minY = Math.Min(y, minY); 
						maxX = Math.Max(x, maxX); 
						maxY = Math.Max(y, maxY); 
					}
					else
						solution[x,y] = 0;
				} 
			}

			_solution = new byte[maxX-minX+1, maxY-minY+1];

			for (int y = minY ; y <= maxY ; y++)
				for (int x = minX ; x <= maxX ; x++)
					_solution[x-minX, y-minY] = solution[x,y];
		}

		void LoadPlayField(DataReader reader)
		{
			int atomIdx = 0;

			for (int y = 0 ; y < 16 ; y++)
			{
				for (int x = 0 ; x < 16 ; x++)
				{
					var value = reader.Byte;

					if ((value & (byte)Field.Type) == (byte)Field.Wall)
					{
						var wall = SKWallNode.Create(value);
						AddNode(wall, x, y);
						Obstables.Add(wall);
					}
					else if ((value & (byte)Field.Type) == (byte)Field.Atom) 
					{
						var free = new SKFreeNode();
						var atom = SKAtomNode.Create(value);

						atom.Name = string.Format("ATOM-{0}", atomIdx++);

						AddNode(free, x, y);
						AddNode(atom, x, y);
						Obstables.Add(atom);
					}
					else if ((value & (byte)Field.Type) == (byte)Field.Free)
					{
						var free = new SKFreeNode();
						AddNode(free, x, y);
					}
				}
			}

			CalculateAccumulatedFrame();
			Flip();
		}

		public static Level Create(int level)
		{
			var field = new Level(level);

			var dataReader = new DataReader(level);

			field.LoadPlayField(dataReader);
			field.LoadSolution(dataReader);
				
			field.Duration 		= dataReader.Integer;

			field.LevelName 	= dataReader.String;
			field.LevelDescription 	= dataReader.String;

//			if (! string.IsNullOrWhiteSpace(field.LevelDescription))
//				Console.WriteLine(field.LevelDescription);
				 
			field.CursorType	= dataReader.Byte;
			field.Background 	= dataReader.Byte;

			//field.Duration  = 5;
			return field;
		}
	}
}
