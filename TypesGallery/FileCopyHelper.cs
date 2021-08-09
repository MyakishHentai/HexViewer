using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public class FileCopyHelper
	{
		private int m_Cancel;

		public event EventHandler UserCancel;
		public event EventHandler Success;
		public event EventHandler Error;


		public uint ErrorCode
		{ get; set; }

		public string ErrorMessage
		{ get; set; }

		public FileCopyHelper()
		{
			ErrorCode = 0;
			ErrorMessage = "";
		}


		// Отмена копирования
		public void Cancel()
		{
			m_Cancel = 1;
		}

		private void OnError(uint errorCode, string message, string sourceFile, string destinationFile)
		{
			ErrorCode = errorCode;
			ErrorMessage = message;

			// Если ошибка, восстанавливаем исходный файл
			if (File.Exists(destinationFile + "_"))
			{
				File.Move(destinationFile + "_", destinationFile);
			}

			// Прервано пользователем
			if (ErrorCode == 995)
			{
				if (UserCancel != null)
				{
					UserCancel(this, new EventArgs());
				}

				return;
			}

			if (Error != null)
			{
				Error(this, new EventArgs());
			}
		}

		public void CopyTo(string source, string destination, SetProgress progressCallback = null)
		{
			// Сначала забэкапаем исходный файл
			if (File.Exists(destination))
			{
				File.Move(destination, destination + "_");
			}

			m_Cancel = 0;

			NativeCopy(source, destination, progressCallback, OnError, ref m_Cancel);

			// Если всё успешно - вызываем событие успеха
			if (ErrorCode == 0)
			{
				// Т.к. успех - удаляем забэкапаный файл
				if (File.Exists(destination + "_"))
				{
					File.Delete(destination + "_");
				}

				if (Success != null)
				{
					Success(this, new EventArgs());
				}
			}
		}


		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate bool SetProgress(long tranfered, long size);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public delegate void SetError(
								uint dErrorCode,
								[In()] [MarshalAs(UnmanagedType.LPWStr)] string lpMessaage,
								[In()] [MarshalAs(UnmanagedType.LPWStr)] string lpExistingFileName,
								[In()] [MarshalAs(UnmanagedType.LPWStr)] string lpNewFileName);

		[DllImport("FileManUnmanaged.qpl", EntryPoint = "Copy", CallingConvention = CallingConvention.StdCall)]
		private static extern void NativeCopy(
			[In()] [MarshalAs(UnmanagedType.LPWStr)] string lpExistingFileName,
			[In()] [MarshalAs(UnmanagedType.LPWStr)] string lpNewFileName,
			SetProgress onUpdate,
			SetError onError,
			ref int cancel);
	}
}
