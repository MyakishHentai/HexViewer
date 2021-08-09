using System;
using System.Runtime.Serialization;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Исключение генерируется при ошибках разбора путей в операциях привязки
	/// </summary>
	[Serializable]
	public class InvalidPathException : Exception
	{
		/// <summary>
		/// Конструктор по-умолчанию, указывает на все ошибки НЕ связанные с некорректными символами в названиях
		/// </summary>
		public InvalidPathException() : base("Путь к свойству имеет неверный формат или содержит недопустимые символы")
		{
		}

		public InvalidPathException(string message) : base(message)
		{
		}

		public InvalidPathException(string message, Exception inner) : base(message, inner)
		{
		}

		protected InvalidPathException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}