using System;

namespace Cryptosoft.TypesGallery.BaseTypes.Data
{
	/// <summary>
	/// Исключение генерируется при ошибках разрешения путей в операциях привязки
	/// </summary>
	public class PathNotResolvedException : Exception
	{
		internal PathNotResolvedException(BindingPath path):base(GetMessage(path))
		{

		}

		static string GetMessage(BindingPath path)
		{
			if (path.IsResolved)
				return "Непредвиденная ошибка при разрешении привязки";

			if (path.Object == null)
				return "Не указан целевой объект привязки";

			return "Не удалось разрешить привязку для " + path;
		}
	}
}