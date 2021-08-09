using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Cryptosoft.TypesGallery
{	
	public class IniSection
	{
		private Dictionary<String, String> m_Values = new Dictionary<String, String>();

		public IniSection() { }

		public void AddValue(String name, String value)
		{
			m_Values[name] = value;
		}

		public String GetValue(String name)
		{
			return m_Values[name];
		}

		public Dictionary<String, String> Values
		{
			get { return new Dictionary<String, String>(m_Values); }
		}
	}

	public class IniFile
	{
		Dictionary<String, IniSection> m_Sections = new Dictionary<String, IniSection>();


		public Dictionary<String, IniSection> Sections
		{
			get { return new Dictionary<String, IniSection>(m_Sections); }
		}


		public IniFile()
		{ }

		virtual public void WriteString(String section, String parameter, String value)
		{
			if (!m_Sections.ContainsKey(section))
			{
				m_Sections[section] = new IniSection();
			}

			m_Sections[section].AddValue(parameter, value);
		}

		virtual public String ReadString(String section, String parameter)
		{
			try
			{
				return m_Sections[section].GetValue(parameter);
			}
			catch
			{
				return null;
			}
		}

		virtual public void Parse(String name)
		{
			using (Stream File1 = File.Open(name, FileMode.Open, FileAccess.Read))
			{
				ReadFrom(File1);
			}
		}

		virtual public void Flush(String name)
		{
			if (!Directory.Exists(Path.GetDirectoryName(name)))
				Directory.CreateDirectory(Path.GetDirectoryName(name));

			using (Stream File1 = File.Create(name))
			{
				WriteTo(File1);
			}
		}

		protected void WriteTo(Stream stream)
		{
			StreamWriter Writer = new StreamWriter(stream);

			foreach (String SectorName in m_Sections.Keys)
			{
				Writer.WriteLine("[" + SectorName + "]");

				Dictionary<String, String> Values = m_Sections[SectorName].Values;

				foreach (String ValueName in Values.Keys)
				{
					Writer.WriteLine(ValueName + "=" + Values[ValueName]);
				}

				Writer.WriteLine();
			}

			Writer.Flush();
		}

		protected void ReadFrom(Stream stream)
		{
			stream.Position = 0;

			StreamReader Reader = new StreamReader(stream);
			String Line;
			IniSection CurrentSection = null;
			
			while (null != (Line = Reader.ReadLine()))
			{
				Line = Line.Trim();

				if (Line.Length == 0)
				{
					continue;
				}
				else if (Line[0] == '[' && Line[Line.Length - 1] == ']')
				{
					// секция

					CurrentSection = m_Sections[Line.Substring(1, Line.Length - 2)] = new IniSection();
				}
				else
				{
					// имя = значение

					try
					{
						String[] NameAndValue = Line.Split(new char[] { '=' });

						CurrentSection.AddValue(NameAndValue[0], NameAndValue[1]);
					}
					catch
					{
						throw new Exception("Ошибка при разборе строки параметра.");
					}
				}
			}
		}

		virtual public Byte[] GetBuffer()
		{
			using (MemoryStream MemStream = new MemoryStream())
			{
				WriteTo(MemStream);
				MemStream.Position = 0;

				Byte[] Result = new Byte[MemStream.Length];
				MemStream.Read(Result, 0, (int)MemStream.Length);

				return Result;

				//return MemStream.GetBuffer();
			}
		}
	}	
}
