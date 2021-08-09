using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public static class MarshalHelper
	{
		public static Byte[] StructureToByteArray(Object structure)
		{
			IntPtr Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
			Byte[] Result = new Byte[Marshal.SizeOf(structure)];

			try
			{
				Marshal.StructureToPtr(structure, Ptr, false);
				Marshal.Copy(Ptr, Result, 0, Result.Length);
			}
			finally
			{
				Marshal.FreeHGlobal(Ptr);
			}

			return Result;
		}

		public static T ByteArrayToStructure<T>(Byte[] RawData)
		{
			return ByteArrayToStructure<T>(RawData, 0);
		}

		public static T ByteArrayToStructure<T>(Byte[] RawData, int offset)
		{
			GCHandle Ptr = GCHandle.Alloc(RawData, GCHandleType.Pinned);

			T Struct;

			try
			{
				Struct = (T)Marshal.PtrToStructure(IntPtr.Add(Ptr.AddrOfPinnedObject(), offset), typeof(T));
			}
			finally
			{
				Ptr.Free();
			}

			return Struct;
		}

		public static Int32 GetSizeOf(Type type)
		{
			return Marshal.SizeOf(type);
		}

		public static Int32 GetSizeOf(Object data)
		{
			if (null == data)
				return 0;
			else if (data.GetType().IsArray)
				return Marshal.SizeOf(data.GetType().GetElementType()) * (data as Array).Length;
			else
				return Marshal.SizeOf(data);
		}
	}
}
