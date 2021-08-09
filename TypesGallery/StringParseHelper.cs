using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public class StringParseHelper
	{
		public static int SkipWhiteSpaces(string path, int index)
		{
			while (index < path.Length && char.IsWhiteSpace(path, index))
				index++;

			return index;
		}
	}

	public class StringParser
	{
		private string m_Input;

		private StringBuilder m_Builder = new StringBuilder(256);

		private int m_Offset;

		public void Init(string input)
		{
			m_Input = input;
			m_Offset = 0;
		}


	}
}
