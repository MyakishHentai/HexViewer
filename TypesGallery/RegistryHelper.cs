using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Win32;

namespace Cryptosoft.TypesGallery
{
	public abstract class RegistryHelper
	{
		static public RegistryKey OpenKey(String fullKeyName)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.OpenSubKey(KeyName);
		}
		static public RegistryKey OpenKey(String fullKeyName, bool writable)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.OpenSubKey(KeyName, writable);
		}
		static public RegistryKey OpenKey(String fullKeyName, RegistryKeyPermissionCheck permissionCheck)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.OpenSubKey(KeyName, permissionCheck);
		}
		static public RegistryKey OpenKey(String fullKeyName, RegistryKeyPermissionCheck permissionCheck,
			RegistryRights rights)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.OpenSubKey(KeyName, permissionCheck, rights);
		}

		static public RegistryKey CreateKey(String fullKeyName)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.CreateSubKey(KeyName);
		}
		static public RegistryKey CreateKey(String fullKeyName, RegistryKeyPermissionCheck permissionCheck)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.CreateSubKey(KeyName, permissionCheck);
		}
		static public RegistryKey CreateKey(String fullKeyName, RegistryKeyPermissionCheck permissionCheck, RegistryOptions options)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.CreateSubKey(KeyName, permissionCheck, options);
		}
		static public RegistryKey CreateKey(String fullKeyName, RegistryKeyPermissionCheck permissionCheck, RegistrySecurity registrySecurity)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.CreateSubKey(KeyName, permissionCheck, registrySecurity);
		}
		static public RegistryKey CreateKey(String fullKeyName, RegistryKeyPermissionCheck permissionCheck, RegistryOptions registryOptions, RegistrySecurity registrySecurity)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			return Root.CreateSubKey(KeyName, permissionCheck, registryOptions, registrySecurity);
		}

		static public void DeleteKey(string fullKeyName)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			Root.DeleteSubKey(KeyName);
		}
		static public void DeleteKey(string fullKeyName, bool throwOnMissingSubKey)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			Root.DeleteSubKey(KeyName, throwOnMissingSubKey);
		}

		static public void DeleteKeyTree(string fullKeyName, bool throwOnMissingSubKey)
		{
			String KeyName;
			RegistryKey Root = GetRootNode(fullKeyName, out KeyName);

			Exception FirstEx = null;

			// Пробуем удалить дерево ключей
			for (var i = 0; i < 60; i++)
			{
				try
				{
					Root.DeleteSubKeyTree(KeyName, throwOnMissingSubKey);

					// Отработало - зануляем эксепшн и выходим из цикла
					FirstEx = null;
					break;
				}
				catch (Exception ex)
				{
					if (FirstEx == null)
					{
						FirstEx = ex;
					}
				}

				System.Threading.Thread.Sleep(100);
			}

			// Если исключение так и не занулилось, значит все попытки исчерпаны
			if (FirstEx != null)
			{
				Log.Error("Не удалось выполнить удаление дерева ключей : " + fullKeyName, FirstEx);
				throw new ApplicationException("Не удалось выполнить операцию после множества попыток", FirstEx);
			}
		}

		static RegistryKey GetRootNode(String fullKeyName, out String subKeyName)
		{
			var RootKeys = new RegistryKey[]
			{
				Registry.ClassesRoot,
				Registry.CurrentConfig,
				Registry.CurrentUser,
				Registry.LocalMachine,
				Registry.PerformanceData,
				Registry.Users
			};

			foreach (var Root in RootKeys)
			{
				if (fullKeyName.StartsWith(Root.Name))
				{
					subKeyName = fullKeyName.Substring(Root.Name.Length + 1);
					return Root;
				}
			}

			throw new ArgumentException(fullKeyName + " не начинается именем стандартного куста реестра.");
		}

		public static void SaveKeyTreeToXml(String fullKeyName, XmlWriter writer)
		{
			writer.WriteStartElement("Key");
			writer.WriteAttributeString("fullName", fullKeyName);

			var Key = OpenKey(fullKeyName);

			foreach (var ValueName in Key.GetValueNames())
			{
				SaveValueToXml(Key, ValueName, writer);
			}

			foreach (var SubKeyName in Key.GetSubKeyNames())
			{
				SaveSubKeyToXml(Key, SubKeyName, writer);
			}

			writer.WriteEndElement();
		}
		static void SaveSubKeyToXml(RegistryKey parentKey, String keyName, XmlWriter writer)
		{
			writer.WriteStartElement("Key");
			writer.WriteAttributeString("name", keyName);

			var Key = parentKey.OpenSubKey(keyName);

			foreach (var ValueName in Key.GetValueNames())
			{
				SaveValueToXml(Key, ValueName, writer);
			}

			foreach (var SubKeyName in Key.GetSubKeyNames())
			{
				SaveSubKeyToXml(Key, SubKeyName, writer);
			}

			writer.WriteEndElement();
		}
		static void SaveValueToXml(RegistryKey key, String valueName, XmlWriter writer)
		{
			var Kind = key.GetValueKind(valueName);

			writer.WriteStartElement("Value");
			writer.WriteAttributeString("name", valueName);
			writer.WriteAttributeString("kind", Kind.ToString());

			switch (Kind)
			{
				case RegistryValueKind.String:
				case RegistryValueKind.ExpandString:
					var Val1 = (String)key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
					writer.WriteCData(Val1.TrimEnd('\0'));
					break;

				case RegistryValueKind.DWord:
				case RegistryValueKind.QWord:
					writer.WriteValue(key.GetValue(valueName));
					break;

				case RegistryValueKind.Binary:
					var Value = (Byte[])key.GetValue(valueName);
					writer.WriteValue(SequenceHelper.ByteArrayToString(Value));
					break;

				case RegistryValueKind.MultiString:
					var Strings = (String[])key.GetValue(valueName, null, RegistryValueOptions.DoNotExpandEnvironmentNames);
					foreach (var Val in Strings)
					{
						writer.WriteStartElement("String");
						writer.WriteCData(Val);
						writer.WriteEndElement();
					}
					break;

				case RegistryValueKind.None:
				case RegistryValueKind.Unknown:
				default:
					break;
			}

			writer.WriteEndElement();
		}

		public static void RestoreKeyTreeFromXml(XmlReader reader, Boolean deleteExistent)
		{
			if (!reader.ReadToFollowing("Key"))
				throw new ApplicationException("Не удалось найти корневой элемент.");

			var FullName = reader.GetAttribute("fullName");
			if (String.IsNullOrWhiteSpace(FullName))
				throw new ApplicationException("Элемент \"Key\" не содержит атрибут \"fullName\"");

			if (deleteExistent)
				DeleteKeyTree(FullName, false);

			var Key = CreateKey(FullName);

			reader.Read();

			while ((reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "Key") && !reader.EOF)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (0 == String.Compare(reader.Name, "Key"))
					{
						// Если элемент типа <MyElement/>, т.е. пустой элемент - рекурсия не нужна
						if (!reader.IsEmptyElement)
						{
							RestoreSubKeyTreeFromXml(Key, reader);
						}
						else
						{
							FullName = reader.GetAttribute("name");

							Key.CreateSubKey(FullName);

							reader.Read();
						}
					}
					else if (0 == String.Compare(reader.Name, "Value"))
					{
						RestoreValueFromXml(Key, reader);
					}
					else
					{
						throw new ApplicationException("Обнаружен неизвестный дочерний элемент " + reader.Name);
					}
				}
				else
				{
					reader.Read();
				}
			}
		}

		static void RestoreSubKeyTreeFromXml(RegistryKey parentKey, XmlReader reader)
		{
			var FullName = reader.GetAttribute("name");
			if (String.IsNullOrWhiteSpace(FullName))
				throw new ApplicationException("Элемент \"Key\" не содержит атрибут \"name\"");

			var Key = parentKey.CreateSubKey(FullName);

			reader.Read();

			while ((reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "Key") && !reader.EOF)
			{
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (0 == String.Compare(reader.Name, "Key"))
					{
						// Если элемент типа <MyElement/>, т.е. пустой элемент - рекурсия не нужна
						if (!reader.IsEmptyElement)
						{
							RestoreSubKeyTreeFromXml(Key, reader);
						}
						else
						{
							FullName = reader.GetAttribute("name");

							Key.CreateSubKey(FullName);

							reader.Read();
						}
					}
					else if (0 == String.Compare(reader.Name, "Value"))
					{
						RestoreValueFromXml(Key, reader);
					}
					else
					{
						throw new ApplicationException("Обнаружен неизвестный дочерний элемент " + reader.Name);
					}
				}
				else
				{
					reader.Read();
				}
			}

			reader.Read();
		}

		static void RestoreValueFromXml(RegistryKey key, XmlReader reader)
		{
			String ValName = reader.GetAttribute("name");

			String KindStr = reader.GetAttribute("kind");
			if (String.IsNullOrWhiteSpace(KindStr))
				throw new ApplicationException("Атрибут \"kind\" не может отсутствовать или не содержать значения");

			RegistryValueKind Kind;
			if (!Enum.TryParse<RegistryValueKind>(KindStr, out Kind))
				throw new ApplicationException("Не удалось разобрать значение атрибута \"kind\"");

			switch (Kind)
			{
				case RegistryValueKind.String:
				case RegistryValueKind.ExpandString:
					key.SetValue(ValName, reader.ReadElementContentAsString(), Kind);
					break;

				case RegistryValueKind.DWord:
					key.SetValue(ValName, reader.ReadElementContentAsInt(), Kind);
					break;

				case RegistryValueKind.QWord:
					key.SetValue(ValName, reader.ReadElementContentAsLong(), Kind);
					break;

				case RegistryValueKind.Binary:
					key.SetValue(ValName, SequenceHelper.StringToByteArray(reader.ReadElementContentAsString()), Kind);
					break;

				case RegistryValueKind.MultiString:
					var Strings = new List<String>();

					reader.Read();
					while ((reader.NodeType != XmlNodeType.EndElement || reader.LocalName != "Value") && !reader.EOF)
					{
						if (reader.NodeType == XmlNodeType.Element)
						{
							if (0 == String.Compare(reader.Name, "String"))
							{
								Strings.Add(reader.ReadElementContentAsString());
							}
							else
							{
								throw new ApplicationException("Обнаружен неизвестный дочерний элемент " + reader.Name);
							}
						}
						else
						{
							reader.Read();
						}
					}

					key.SetValue(ValName, Strings.ToArray(), Kind);
					break;

				case RegistryValueKind.None:
				case RegistryValueKind.Unknown:
				default:
					break;
			}
		}
	}
}
