using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Globalization;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;

namespace Cryptosoft.TypesGallery
{
    public static class CryptoNewNatives
    {
        internal enum ErrorCode
        {
            Success = 0x00000000,
            ErrorObjectNotFound = 0x000010D8,
            FileNotFound = unchecked((int)0x80070002),
            //NoMoreItems = unchecked((int)0x8009002a),
            NoMoreItems = unchecked((int)0x80070103),
            MoreData = 234
        }

        internal enum Identifier
        {
            None = 0x000000,
            KeyAdmin = 0x000002,
            Crypto = 0x000003,
            CrApi = 0x000003,
            VirtualDisk = 0x000004,
            Mandatory = 0x000005,
            Network = 0x000007,
            CryptoFilter = 0x000008,
            Efs = 0x000012,
            Eds = 0x000003
        }

        [Flags]
        internal enum DeleteKeyOptions
        {
            None = 0x0,
            Mask = 0x1,
            OnlyData = 0x2,
            OnlyInfo = 0x4,
            Skip = 0x8
        }

        [Flags]
        internal enum HashContinueFlags
        {
            First = 1,					// Первый блок
            Last = 2,					// Последний блок
            FirstLast = First | Last,	// Первый и последний
            FirstNonlast = First,		// Первый, но не содержит последний 
            NonfirstLast = Last,		// Последний, но не содержит первый
            NonfirstNonlast = 0			// Промежуточный блок
        }

        static Dictionary<Identifier, SafeHandleZeroOrMinusOneIsInvalid> s_Libs = new Dictionary<Identifier, SafeHandleZeroOrMinusOneIsInvalid>();

        internal static void CheckCryptoError(ErrorCode error)
        {
            if (error != ErrorCode.Success)
                throw new Win32Exception((int)error);
        }

        #region GetAddress, LoadLibrary

        [SuppressUnmanagedCodeSecurity]
        static class UnsafeNativeMethods
        {

            [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
            public static extern SafeLibraryHandle LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] String path);


            [DllImport("Kernel32.dll", BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern IntPtr GetProcAddress(
                SafeHandleZeroOrMinusOneIsInvalid module,
                [MarshalAs(UnmanagedType.LPStr)]String file_name);
        }

        internal static T GetAddress<T>(Identifier component, String name)
        {
            SafeHandleZeroOrMinusOneIsInvalid Lib = LoadLibrary(component);
            return GetAddress<T>(Lib, name);
        }

        /// <summary>
        /// Поучение делегата функции по имени и указателю на библиотеку
        /// </summary>
        /// <typeparam name="T">Тип делегата</typeparam>
        /// <param name="handle">Указатель на загруженную библиотеку</param>
        /// <param name="name">Имя функции</param>
        /// <returns>Делегат искомой функции</returns>
        static T GetAddress<T>(SafeHandleZeroOrMinusOneIsInvalid handle, String name)
        {
            IntPtr MethodPointer = UnsafeNativeMethods.GetProcAddress(handle, name);

            if (MethodPointer == IntPtr.Zero)
                throw new InvalidOperationException(
                    String.Format(CultureInfo.CurrentCulture, "Функция {0} не найдена", name));

            return (T)(Object)Marshal.GetDelegateForFunctionPointer(MethodPointer, typeof(T));
        }

        /// <summary>
        /// Загрузка библиотеки
        /// </summary>
        /// <param name="component">Идентификатор библиотеки</param>
        /// <returns>Удазатель на загруженную библиотеку</returns>
        static SafeHandleZeroOrMinusOneIsInvalid LoadLibrary(Identifier component)
        {
            if (s_Libs.ContainsKey(component))
                return s_Libs[component];

            
            if ((component != Identifier.Crypto) && (component != Identifier.Eds))
                throw new NotSupportedException();

            //String ModulePath = BsGetQPRootDirectory() + "CkKernelApi.dll";
            //String ModulePath = GetComponentRootDirectory(0x25000001) + "CkKernelApi.dll";
            String ModulePath = "CkKernelApi.dll";

            SafeHandleZeroOrMinusOneIsInvalid Lib = UnsafeNativeMethods.LoadLibraryW(ModulePath);

            if (Lib.IsInvalid || Lib.IsClosed)
            {
                ModulePath = "CkKernelApi.qpl";
                Lib = UnsafeNativeMethods.LoadLibraryW(ModulePath);

                if (Lib.IsInvalid || Lib.IsClosed)
                {
                    throw new Exception("Ошибка при загрузке библиотеки " + component.ToString());
                }
            }

            s_Libs[component] = Lib;
            return Lib;
        }

        #endregion

        #region CryptoNew

        delegate ErrorCode PrototypeKeyGetData(SafeOldCryptKeyHandle key, [In, Out]Byte[] data, ref Int32 outDataSize);
        internal static Byte[] KeyGetData(SafeOldCryptKeyHandle key, Int32 dataSize)
        {
            PrototypeKeyGetData function = GetAddress<PrototypeKeyGetData>(Identifier.Crypto, "CkKeyGetData");

            Byte[] Data = new Byte[dataSize];
            Int32 DataSize = dataSize;
            ErrorCode Error = function(key, Data, ref DataSize);

            CheckCryptoError(Error);

            return Data;
        }

        delegate ErrorCode PrototypeCloseAllKeys();
        internal static void CloseAllKeys()
        {
            PrototypeCloseAllKeys function = GetAddress<PrototypeCloseAllKeys>(Identifier.Crypto, "CkKeyCloseAll");
            ErrorCode Error = function();
            CheckCryptoError(Error);
        }


        delegate ErrorCode PrototypeKeyDataGet(
            [MarshalAs(UnmanagedType.LPWStr)]String path,
            Int32 pathSize,
            [MarshalAs(UnmanagedType.LPWStr)]String userName,
            Int32 userNameSize,		//включая терминальный символ
            Guid keysType,
            Guid guid,
            IntPtr keyInfo, //ref ReadKeys.CrKeyInfo keyInfo,
            [In, Out] Byte[] keyData,
            Int32 keyDataSize,
            [In, Out, MarshalAs(UnmanagedType.LPWStr)]String name,
            Int32 nameLength);
        internal static Byte[] KeyDataGet(String letter, String userName, Guid keysType, Guid key, Int32 keyDataSize)
        {
            PrototypeKeyDataGet function = GetAddress<PrototypeKeyDataGet>(Identifier.Crypto, "CkKeyDataGet");

            Byte[] Data = new Byte[keyDataSize];

            ErrorCode Error = function(
                letter,
                null == letter ? 0 : (Int32)letter.Length,
                userName,
                null == userName ? 0 : (Int32)userName.Length,
                keysType,
                key,
                IntPtr.Zero,
                Data,
                keyDataSize,
                null,
                0);

            CheckCryptoError(Error);

            return Data;
        }


        /*public static Int32 NumberFromGuid(Guid guid)
        {
            Byte[] Bytes = guid.ToByteArray();
            unsafe
            {
                fixed (byte* PBytes = &Bytes[6])
                {
                    return (Int32)(*(Int16*)PBytes);
                }
            }
        }

        public static Int32 SeriesFromGuid(String guid)
        {
            return SeriesFromGuid(new Guid(guid));
        }

        public static Int32 SeriesFromGuid(Guid guid)
        {
            Byte[] Bytes = guid.ToByteArray();
            unsafe
            {
                fixed (byte* PBytes = &Bytes[4])
                {
                    return (Int32)(*(Int16*)PBytes);
                }
            }
        }*/

        delegate ErrorCode PrototypeKeyOpen(Guid keysType, Guid guid, [Out] out SafeOldCryptKeyHandle key);
        //internal static SafeOldCryptKeyHandle KeyOpen(Guid keyType, Guid keyName)
        //{
        //    PrototypeKeyOpen FunctionOpen = GetAddress<PrototypeKeyOpen>(Identifier.Crypto, "CkKeyGetHandle");
        //    SafeOldCryptKeyHandle Handle;
        //    ErrorCode Error = FunctionOpen(keyType, keyName, out Handle);

        //    CheckCryptoError(Error);

        //    return Handle;
        //}

        internal static SafeOldCryptKeyHandle TryKeyOpen(Guid keyType, Guid keyName)
        {
            PrototypeKeyOpen FunctionOpen = GetAddress<PrototypeKeyOpen>(Identifier.Crypto, "CkKeyGetHandle");
            SafeOldCryptKeyHandle Handle;
            ErrorCode Error = FunctionOpen(keyType, keyName, out Handle);

            if (Error != ErrorCode.Success)
                return null;

            return Handle;
        }



        delegate ErrorCode PrototypeEdsVerifyHashSign(
            SafeOldCryptKeyHandle key,
            Byte[] hash,
            Byte[] sign,
            ref Boolean isEqual);
        /// <summary>
        /// Проверка ЭП от хеша.
        /// </summary>
        /// <param Name="keyHandle">
        /// Хендл открытого ключа проверки ЭП (ключ открывается функцией CkKeyOpen).
        /// </param>
        /// <param Name="hash">Хеш.</param>
        /// <param Name="sign">Подпись хеша (подсчитанная CkEdsGenerateHashSign).</param>
        /// <param Name="isEqual">Признак корректности подписи.</param>
        /// <returns>Статус.</returns>
        internal static Boolean EdsVerifyHashSign(SafeOldCryptKeyHandle key, Byte[] hash, Byte[] sign)
        {
            PrototypeEdsVerifyHashSign function = GetAddress<PrototypeEdsVerifyHashSign>(Identifier.Eds, "CkEdsVerifyHashSign");

            Boolean Equal = false;
            ErrorCode Error = function(key, hash, sign, ref Equal);

            CheckCryptoError(Error);

            return Equal;
        }

        delegate ErrorCode PrototypeEdsVerifyHashSignEx(
            SafeOldCryptKeyHandle key,
            Byte[] hash,
            Int32 HashSize,
            Byte[] pSign,
            Int32 SignSize,
            EdsHashVersion Parameters,
            [Out, MarshalAs(UnmanagedType.U4)]
			out UInt32 pIsEqual);

        internal static Boolean EdsVerifyHashSignEx(SafeOldCryptKeyHandle key, Byte[] hash, Byte[] sign, EdsHashVersion version)
        {
            PrototypeEdsVerifyHashSignEx function = GetAddress<PrototypeEdsVerifyHashSignEx>(Identifier.Eds, "CkEdsVerifyHashSignEx");

            UInt32 Equal = 0;
            ErrorCode Error = function(key, hash, hash.Length, sign, sign.Length, version, out Equal);

            CheckCryptoError(Error);

            return Equal != 0;
        }

        delegate ErrorCode PrototypeEdsVerifyHashSignEx2(
            EdsHashVersion Version,
            Byte[] pCheckKey,
            Int32 KeySize,
            Byte[] pSign,
            Int32 SignSize,
            Byte[] pHash,
            Int32 HashSize,
            [Out, MarshalAs(UnmanagedType.U4)]
			out UInt32 pIsEqual);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawCheckKey">128 байт чистого ключа.</param>
        /// <param name="sign"></param>
        /// <param name="Hash"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        internal static Boolean EdsVerifyHashSignEx2(Byte[] rawCheckKey, Byte[] sign, Byte[] Hash, EdsHashVersion version)
        {
            PrototypeEdsVerifyHashSignEx2 function = GetAddress<PrototypeEdsVerifyHashSignEx2>(Identifier.Eds, "CkEdsVerifyHashSignEx2");

            UInt32 Equal = 0;
            ErrorCode Error = function(version, rawCheckKey, rawCheckKey.Length, sign, sign.Length, Hash, Hash.Length, out Equal);

            CheckCryptoError(Error);

            return Equal != 0;
        }



        delegate ErrorCode PrototypeCkEdsKeyGetVersion(
            SafeOldCryptKeyHandle hKey,
            [Out] out EdsHashVersion pVersion);

        internal static EdsHashVersion EdsKeyGetVersion(SafeOldCryptKeyHandle KeyHandle)
        {
            PrototypeCkEdsKeyGetVersion function = GetAddress<PrototypeCkEdsKeyGetVersion>(Identifier.Eds, "CkEdsKeyGetVersion");

            EdsHashVersion Version;
            ErrorCode Error = function(KeyHandle, out Version);

            CheckCryptoError(Error);

            return Version;
        }




        delegate ErrorCode PrototypeCkEdsGenKeyGet(
            [Out] out SafeOldCryptKeyHandle phKey,
            [Out] out Guid pKeyID);

        internal static SafeOldCryptKeyHandle EdsGenKeyGet()
        {
            PrototypeCkEdsGenKeyGet function = GetAddress<PrototypeCkEdsGenKeyGet>(Identifier.Eds, "CkEdsGenKeyGet");

            SafeOldCryptKeyHandle KeyHandle;
            Guid KeyGuid;

            ErrorCode Error = function(out KeyHandle, out KeyGuid);

            CheckCryptoError(Error);

            return KeyHandle;
        }



        delegate ErrorCode PrototypeCkKeyCreate(
            Guid type,
            Guid algorithm,
            SafeOldCryptKeyHandle baseKey,
            String password,
            Int32 extraDataSize,
            [Out]out SafeOldCryptKeyHandle data);
        public static SafeOldCryptKeyHandle KeyCreate(Guid type, Guid algorithm, String password, SafeOldCryptKeyHandle baseKey)
        {
            SafeOldCryptKeyHandle KeyHandle = null;
            Int32 DataSize = password == null ? 0 : password.Length + 1;

            ErrorCode Error = ErrorCode.FileNotFound;

            PrototypeCkKeyCreate function = GetAddress<PrototypeCkKeyCreate>(Identifier.Crypto, "CkKeyCreate");

            Error = function(
                type,
                algorithm,
                baseKey == null ? new SafeOldCryptKeyHandle() : baseKey,
                password,
                DataSize,
                out KeyHandle);

            CheckCryptoError(Error);

            return KeyHandle;
        }


        delegate ErrorCode PrototypeCkEdsGetParametersSize(
            EdsHashVersion EdsVersion,
            [Out] out Int32 pHashSize,
            [Out] out Int32 pHashBlockSize,
            [Out] out Int32 pHashContextSize,
            [Out] out Int32 pSignSize,
            [Out] out Int32 pSignKeySize,
            [Out] out Int32 pCheckKeySize);

        internal static EdsParameters GetEdsHashParameters(EdsHashVersion edsVersion)
        {
/*
            PrototypeCkEdsGetParametersSize function = GetAddress<PrototypeCkEdsGetParametersSize>(Identifier.Crypto, "CkEdsGetParametersSize");

            EdsParameters Result;

            ErrorCode Error = function(edsVersion, out Result.HashSize, out Result.HashBlockSize, out Result.HashContextSize,
                out Result.SignSize, out Result.SignKeySize, out Result.CheckKeySize);

            CheckCryptoError(Error);*/

/*
 * Old Hash
            EdsParameters Result = new EdsParameters();
            Result.CheckKeySize = 128;
            Result.HashBlockSize = 32;
            Result.HashContextSize = 32 * 3;
            Result.HashSize = 32;
            Result.SignKeySize = 64;
            Result.SignSize = 128;*/

			EdsParameters Result = new EdsParameters();
			Result.CheckKeySize = 128;
			Result.HashBlockSize = 64;
			Result.HashContextSize = 64 * 3;
			Result.HashSize = 64;
			Result.SignKeySize = 64;
			Result.SignSize = 128;

            return Result;
        }


        delegate ErrorCode PrototypeCkDataCalcHashOld(
            Byte[] pData,
            UInt32 DataSize,
            [In, Out] byte[] pHashContext,
            HashContinueFlags ContinueFlag);

        internal static void DataCalcHashOld(Byte[] data, Int32 dataSize, Byte[] context, EdsHashVersion hashVersion, HashContinueFlags continueFlag)
        {

            PrototypeCkDataCalcHashOld function = GetAddress<PrototypeCkDataCalcHashOld>(Identifier.Crypto, "CkDataCalcHashOld");

            ErrorCode Error = function(data, (UInt32)dataSize, context, continueFlag);

            CheckCryptoError(Error);
        }

		delegate ErrorCode PrototypeCkDataCalcHashEx(
			Byte[] pData,
			UInt32 DataSize,
			[In, Out] byte[] pHashContext,
			UInt32 ContextSize,
			UInt32 Parameters,
			HashContinueFlags ContinueFlag);

		internal static void DataCalcHashEx(Byte[] data, Int32 dataSize, Byte[] context, EdsHashVersion hashVersion, HashContinueFlags continueFlag)
		{
			PrototypeCkDataCalcHashEx function = GetAddress<PrototypeCkDataCalcHashEx>(Identifier.Crypto, "CkDataCalcHashEx");

			ErrorCode Error = function(data, (UInt32)dataSize, context, (UInt32)context.Length, (UInt32)hashVersion, continueFlag);

			CheckCryptoError(Error);
		}

        /// <summary>
        /// Делегат функции перешифрования данных
        /// </summary>
        /// <param name="keySource">Исходный ключ, с которого происходит перешифрование</param>
        /// <param name="keyDestination">Новый ключ, на котором будут зашифрованы данные</param>
        /// <param name="context">Контекст</param>
        /// <param name="contextSize">Размер контекста</param>
        /// <param name="dataIn">Перешифровываемые данные</param>
        /// <param name="dataInSize">Размер перешифровываемый данных</param>
        /// <param name="dataOut">Буфер для перешифрованных данных</param>
        /// <param name="dataOutSize">Размер буфера</param>
        /// <returns>Код результата</returns>
        delegate ErrorCode PrototypeDataReencode(
            SafeOldCryptKeyHandle keySource,
            SafeOldCryptKeyHandle keyDestination,
            IntPtr context,
            Int32 contextSize,
            [In]Byte[] dataIn,
            Int32 dataInSize,
            [In, Out]Byte[] dataOut,
            Int32 dataOutSize);

        public static Byte[] DataRecrypt(
            SafeOldCryptKeyHandle source,
            SafeOldCryptKeyHandle destination,
            Byte[] data)
        {
            PrototypeDataReencode function = GetAddress<PrototypeDataReencode>(Identifier.Crypto, "CkDataReencode");

            Byte[] Out = new Byte[data.Length];

            ErrorCode Error = function(source, destination, IntPtr.Zero, 0, data, data.Length, Out, Out.Length);

            CheckCryptoError(Error);

            return Out;
        }

        /// <summary>
        /// Шифрование даных на ключе
        /// </summary>
        /// <param name="key">Ключ шифрования</param>
        /// <param name="context">Контекст шифрования</param>
        /// <param name="contextSize">Размер контекста</param>
        /// <param name="dataIn">Шифруемые данные</param>
        /// <param name="dataInSize">Размер данных</param>
        /// <param name="dataOut">Буфер для зашифрованных данных</param>
        /// <param name="dataOutSize">Размер буфера</param>
        /// <returns>Код результата</returns>
        delegate ErrorCode PrototypeDataEncode(
            SafeOldCryptKeyHandle key,
            [MarshalAs(UnmanagedType.AsAny)]Object context,
            Int32 contextSize,
            [In]Byte[] dataIn,
            Int32 dataInSize,
            [In, Out]Byte[] dataOut,
            Int32 dataOutSize);
        /// <summary>
        /// Шифрование данных на ключе
        /// </summary>		

        public static Byte[] DataEncode(SafeOldCryptKeyHandle key, Byte[] data)
        {
            PrototypeDataEncode function = GetAddress<PrototypeDataEncode>(Identifier.Crypto, "CkDataEncode");

            Byte[] Out = new Byte[data.Length];

            ErrorCode Error = function(key, null, 0, data, data.Length, Out, data.Length);

            CheckCryptoError(Error);

            return Out;
        }

        public static Byte[] DataDecode(SafeOldCryptKeyHandle key, Byte[] data)
        {
            PrototypeDataEncode function = GetAddress<PrototypeDataEncode>(Identifier.Crypto, "CkDataDecode");

            Byte[] Out = new Byte[data.Length];

            ErrorCode Error = function(key, null, 0, data, data.Length, Out, data.Length);

            CheckCryptoError(Error);

            return Out;
        }

        delegate ErrorCode PrototypeCkGetHashFromContext(
            UInt32 Parameters,
            Byte[] pHashContext,
            UInt32 ContextSize,
            [Out] Byte[] pHash,
            UInt32 HashSize);

        internal static Byte[] GetHashFromContext(Byte[] context, EdsHashVersion hashVersion, EdsParameters parameters)
        {
            PrototypeCkGetHashFromContext function = GetAddress<PrototypeCkGetHashFromContext>(Identifier.Crypto, "CkGetHashFromContext");

            Byte[] Hash = new Byte[parameters.HashSize];

            ErrorCode Error = function((UInt32)hashVersion, context, (UInt32)context.Length, Hash, (UInt32)Hash.Length);
            CheckCryptoError(Error);

            return Hash;
        }

        #region CkKeyInit

        delegate ErrorCode PrototypeCkKeyInit(
            Guid Type,
            Byte[] pKeyData,
            UInt32 KeyDataSize,
            ref Guid pParentKeyType,
            ref Guid pParentKeyID,
            out SafeOldCryptKeyHandle keyHandle);

        public static SafeOldCryptKeyHandle KeyInit(Guid type, Byte[] keyData, Guid parentType, Guid parentId)
        {
            PrototypeCkKeyInit function = GetAddress<PrototypeCkKeyInit>(Identifier.Crypto, "CkKeyInit");

            SafeOldCryptKeyHandle Handle;

            ErrorCode Error = function(type, keyData, (UInt32)keyData.Length, ref parentType, ref parentId, out Handle);
            CheckCryptoError(Error);

            return Handle;
        }




        #endregion

        #endregion
    }




    /// <summary>
    /// Базовый класс для хендлов криптографических ключей
    /// </summary>
    public abstract class SafeKeyHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected SafeKeyHandle()
            : base(true)
        { }
    }


    /// <summary>
    /// Хендл ключа из старой криптухи
    /// </summary>
    public sealed class SafeOldCryptKeyHandle : SafeKeyHandle
    {
        delegate CryptoNewNatives.ErrorCode PrototypeCloseKey(IntPtr key);
        static readonly PrototypeCloseKey FunctionKeyClose;

        /// <summary>
        /// Статический конструктор
        /// </summary>
        static SafeOldCryptKeyHandle()
        {
            // получаем адрес функции закрытия ключа заранее
            // (чтобы при освобождении ключа не обращаться к освобожденному хендлу библиотеки)
            FunctionKeyClose = CryptoNewNatives.GetAddress<PrototypeCloseKey>(
                CryptoNewNatives.Identifier.Crypto, "CkKeyClose");
        }

        protected sealed override bool ReleaseHandle()
        {
            if (handle != IntPtr.Zero)
                return CloseKey(handle) == CryptoNewNatives.ErrorCode.Success;

            return true;
        }

        /// <summary>
        /// После вызова этого метода, ключом нельзя будет пользоваться, однако он останется открытым в криптухе!
        /// </summary>
        public void MakeImmortal()
        {
            handle = IntPtr.Zero;
        }

        /// <summary>
        ///  Закрывает открытый ключ
        /// </summary>
        /// <param name="handle">Хендл ключа</param>
        static CryptoNewNatives.ErrorCode CloseKey(IntPtr handle)
        {
            return FunctionKeyClose(handle);
        }
    }

    /// <summary>
    /// SafeHandle for a native HMODULE
    /// </summary>
    internal sealed class SafeBsLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeBsLibraryHandle()
            : base(true)
        { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("qpbase2.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean BsFreeLibrary(IntPtr module);

        protected override Boolean ReleaseHandle()
        {
            return BsFreeLibrary(handle);
        }
    }

    /// <summary>
    /// Handle for a native HMODULE
    /// </summary>
    internal sealed class SafeLibraryHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeLibraryHandle()
            : base(true)
        { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1060:MovePInvokesToNativeMethodsClass")]
        [DllImport("Kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern Boolean FreeLibrary(IntPtr module);

        protected override Boolean ReleaseHandle()
        {
            return FreeLibrary(handle);
        }
    }

    public struct EdsParameters
    {
        public Int32 HashSize;
        public Int32 HashContextSize;
        public Int32 SignSize;
        public Int32 HashBlockSize;
        public Int32 SignKeySize;
        public Int32 CheckKeySize;
    }
}
