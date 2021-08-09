using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
    public class HashStream : Stream
    {
        bool m_HashingFinished = false;

        public override bool CanRead 
        {
            get { return m_BaseStream.CanRead; }
        }
        public override bool CanSeek
        {
            get { return m_BaseStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return m_BaseStream.CanWrite; }
        }
        public override long Length
        {
            get { return m_BaseStream.Length; }
        }
        public override long Position
        {
            get { return m_BaseStream.Position; }
            set { m_BaseStream.Position = value; }
        }

        Stream m_BaseStream;
        CryptoNewHashProvider m_HashProvider;

        public HashStream(Stream baseStream)
        {
            m_BaseStream = baseStream;
        #if !DEBUG
			m_HashProvider = new CryptoNewHashProvider(EdsHashVersion.Long);
        #endif
        }

        public override void Flush()
        {
            m_BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (m_HashingFinished)
            {
                throw new ApplicationException("Процесс хэширования был завершён. Дальнейшее чтение не доступно.");
            }

            Int32 Result = m_BaseStream.Read(buffer, offset, count);

        #if !DEBUG
            // Временный буфер для передачи в хэшер
            byte[] TempBuffer = new byte[count];
            Array.Copy(buffer, offset, TempBuffer, 0, count);

            // Считаем хэш
            m_HashProvider.HashData(TempBuffer, Result);
        #endif
            return Result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
        #if !DEBUG
            if (m_HashingFinished)
            {
                throw new ApplicationException("Процесс хэширования был завершён. Дальнейшая запись не доступна.");
            }

            // TODO: Написать без использования доп.буффера. Возможно, замодифать хэшер

            // Временный буфер для передачи в хэшер
            byte[] TempBuffer = new byte[count];

            Array.Copy(buffer, offset, TempBuffer, 0, count);

            // Считаем хэш
            m_HashProvider.HashData(TempBuffer, TempBuffer.Length);
        #endif
            m_BaseStream.Write(buffer, offset, count);
        }

        public byte[] GetResultHash()
        {
            // Запросили результирующий хэш => больше потоком пользоваться не следует
            m_HashingFinished = true;

            // Возвращаем итоговый хэш
        #if !DEBUG
            return m_HashProvider.GetResultHash();
        #else
            return new byte[32];
        #endif
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return m_BaseStream.Seek(offset, origin);
        }
        public override void SetLength(long value)
        {
            m_BaseStream.SetLength(value);
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_BaseStream.Dispose();
				base.Dispose(disposing);
			}
		}
    }
}
