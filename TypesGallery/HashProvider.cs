using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
    public enum EdsHashVersion : uint
    {
        Short = 0,	// короткая ЭП ГОСТ Р 34.10-2001; хеш ГОСТ Р 34.11–94.
        Long,				// длинная ЭП ГОСТ Р 34.10-2012; хеш "СтриБог" ГОСТ Р 34.11-2012.
        SB32,				// короткая ЭП ГОСТ Р 34.10-2001, использующая хеш ГОСТ Р 34.11-2012 размером 32 байта;
        //  хеш "СтриБог" ГОСТ Р 34.11-2012 размером 32 байта.
        Old = 7,	// подпись, использовавшая старый хеш; старый хеш, использовавшийся до Увода.

        Undefined = 0xffffffff
    }


    public interface IHashProvider : IDisposable, ICloneable
    {
        void HashData(byte[] data, Int32 dataSize);

        byte[] GetResultHash();
    }


    class CryptoNewHashProvider : IHashProvider
    {
        EdsHashVersion m_Version;

        Byte[] m_LastBlock;

        Int32 m_LastBlockRealSize;

        Byte[] m_Context;

        EdsParameters m_EdsParameters;

        CryptoNewNatives.HashContinueFlags m_ContinueFlag = CryptoNewNatives.HashContinueFlags.First;

        public CryptoNewHashProvider(EdsHashVersion hashVersion)
        {
            m_Version = hashVersion;

            m_EdsParameters = CryptoNewNatives.GetEdsHashParameters(m_Version);

            m_LastBlock = new Byte[m_EdsParameters.HashBlockSize];

            m_Context = new Byte[m_EdsParameters.HashContextSize];
        }

        public Object Clone()
        {
            CryptoNewHashProvider Result = new CryptoNewHashProvider(m_Version);

            m_LastBlock.CopyTo(Result.m_LastBlock, 0);
            Result.m_LastBlockRealSize = m_LastBlockRealSize;
            m_Context.CopyTo(Result.m_Context, 0);
            Result.m_EdsParameters = m_EdsParameters;
            Result.m_ContinueFlag = m_ContinueFlag;

            return Result;
        }


        public void HashData(byte[] data, int dataSize)
        {
            if (m_ContinueFlag == CryptoNewNatives.HashContinueFlags.Last)
                throw new Exception("Нарушение порядка использования класса CryptoNewHashProvider." +
                            "Функция HashData() не может быть вызвана после GetResultHash()");

            Int32 DataOffset = 0;
            Int32 DataSize = dataSize;

            if (m_LastBlockRealSize > 0)
            {
                // если у нас есть не кратный блок с предыдущей итерации, дополняем его новыми данными до SIZE_BLOCK_HASH

                Int32 FirstBlockSize = Math.Min(m_EdsParameters.HashBlockSize - m_LastBlockRealSize, dataSize);

                Array.Copy(data, 0, m_LastBlock, m_LastBlockRealSize, FirstBlockSize);

                if (m_EdsParameters.HashBlockSize == FirstBlockSize + m_LastBlockRealSize)
                {
                    // если новых данных оказалось достаточно и получили целый блок, считаем хеш

                    CryptoNewNatives.DataCalcHashEx(m_LastBlock, m_EdsParameters.HashBlockSize, m_Context, m_Version, m_ContinueFlag);

                    m_ContinueFlag = CryptoNewNatives.HashContinueFlags.NonfirstNonlast;

                    m_LastBlock.Initialize();
                    m_LastBlockRealSize = 0;
                }
                else
                {
                    // иначе данные продолжают копиться
                    m_LastBlockRealSize += FirstBlockSize;
                }

                DataOffset += FirstBlockSize;
                DataSize -= FirstBlockSize;
            }


            //Разбиваем данные на блоки по 32 байта и вычисляем контекст для всех блоков

            Byte[] Block = new Byte[m_EdsParameters.HashBlockSize];

            while (DataSize >= m_EdsParameters.HashBlockSize)
            {
                Array.Copy(data, DataOffset, Block, 0, m_EdsParameters.HashBlockSize);

				CryptoNewNatives.DataCalcHashEx(Block, (Int32)m_EdsParameters.HashBlockSize, m_Context, m_Version, m_ContinueFlag);

                m_ContinueFlag = CryptoNewNatives.HashContinueFlags.NonfirstNonlast;

                DataOffset += m_EdsParameters.HashBlockSize;
                DataSize -= m_EdsParameters.HashBlockSize;
            }


            if (DataSize > 0)
            {
                // если размер не кратен SIZE_BLOCK_HASH, сохраняем оставшиеся данные, чтобы потом завершить подсчёт хеша

                Array.Copy(data, DataOffset, m_LastBlock, 0, DataSize);
                m_LastBlockRealSize = DataSize;
            }
        }

        public byte[] GetResultHash()
        {
            m_ContinueFlag = (m_ContinueFlag == CryptoNewNatives.HashContinueFlags.First)
                                ? CryptoNewNatives.HashContinueFlags.FirstLast
                                : CryptoNewNatives.HashContinueFlags.Last;

			CryptoNewNatives.DataCalcHashEx(m_LastBlock, m_LastBlockRealSize, m_Context, m_Version, m_ContinueFlag);

            byte[] ResultHash = new byte[m_EdsParameters.HashSize];

			for (int i = 0; i < m_EdsParameters.HashSize; i++)
            {
                ResultHash[i] = m_Context[i];
            }

            return ResultHash;/*CryptoNewNatives.GetHashFromContext(m_Context, m_Version, m_EdsParameters);*/
        }

        public void Dispose()
        { }
    }
}
