using System;
using System.IO;

namespace Rpc
{
	public static class TestToolkit
	{
		public static byte[] LogToArray(this string text)
		{
			MemoryStream mem = new MemoryStream();
			int hDig = -1;

			for (int i = 0; i < text.Length; i++)
			{
				int num = ParseDigit(text[i]);
				if (num == -1)
					continue;

				if (hDig == -1)
				{
					hDig = num;
					continue;
				}

				mem.WriteByte((byte)(hDig * 16 + num));
				hDig = -1;
			}

			mem.Position = 0;
			return mem.ToArray();
		}

		private static int ParseDigit(Char ch)
		{
			switch (Char.ToLowerInvariant(ch))
			{
				case '0': return 0;
				case '1': return 1;
				case '2': return 2;
				case '3': return 3;
				case '4': return 4;
				case '5': return 5;
				case '6': return 6;
				case '7': return 7;
				case '8': return 8;
				case '9': return 9;
				case 'a': return 10;
				case 'b': return 11;
				case 'c': return 12;
				case 'd': return 13;
				case 'e': return 14;
				case 'f': return 15;
				default: return -1;
			}
		}	
	}
}

