using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
    public static class StreamHelper
    {
        const int DefaultBufferSize = 4096;

        public static void CopyTo(this Stream source, Stream destination, Action<Int32> progressCallback)
        {
            CopyTo(source, destination, DefaultBufferSize, CancellationToken.None, progressCallback);
        }

        public static void CopyTo(this Stream source, Stream destination, int bufferSize, Action<Int32> progressCallback)
        {
            CopyTo(source, destination, DefaultBufferSize, CancellationToken.None, progressCallback);
        }

        public static Task CopyToAsync(this Stream source, Stream destination, Action<Int32> progressCallback)
        {
            return Task.Run(() => CopyTo(source, destination, DefaultBufferSize, CancellationToken.None, progressCallback));
        }

        public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize, Action<Int32> progressCallback)
        {
            return Task.Run(() => CopyTo(source, destination, bufferSize, CancellationToken.None, progressCallback));
        }

        public static Task CopyToAsync(this Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken, Action<Int32> progressCallback)
        {
            return Task.Run(() => CopyTo(source, destination, bufferSize, cancellationToken, progressCallback));
        }

        private static void CopyTo(this Stream source, Stream destination, int bufferSize, CancellationToken cancellationToken, Action<Int32> progressCallback)
        {
            #region Проверка входных параметров
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (progressCallback == null)
            {
                throw new ArgumentNullException("progressCallback");
            }
            if (bufferSize == 0)
            {
                throw new ArgumentException("bufferSize");
            }
            if (!source.CanRead)
            {
                throw new NotSupportedException("Source недоступен для чтения");
            }
            if (!destination.CanWrite)
            {
                throw new NotSupportedException("Destination недоступен для записи");
            }
            #endregion

            Byte[] Buffer = new Byte[bufferSize];
            Int32 PrevProgress = 0;

            for (int i = 0; i < source.Length; i += bufferSize)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException();
                }

                Int32 BytesRead = source.Read(Buffer, 0, bufferSize);

                destination.Write(Buffer, 0, BytesRead);

                Int32 CurrentProgress = (Int32)(((Double)source.Position / (Double)source.Length) * 100);

                if (PrevProgress != CurrentProgress)
                {
                    progressCallback(CurrentProgress);
                    PrevProgress = CurrentProgress;
                }
            }
        }
    }
}
