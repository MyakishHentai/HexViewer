using System;
using System.Threading;

namespace Cryptosoft.TypesGallery.BaseTypes
{
    public class DisposableMutex : IDisposable
    {
        private readonly Mutex m_Mutex;

        private bool m_Disposed;

        public delegate bool NeedStop();

        public DisposableMutex(string name, NeedStop needStop)
        {
            m_Mutex = new Mutex(false, name);

            try
            {
                if (needStop == null)
                {
                    m_Mutex.WaitOne();
                }
                else
                {
                    do
                    {
                        if (m_Mutex.WaitOne(1000))
                            break;
                    }
                    while (!needStop());
                }
            }
            catch (AbandonedMutexException/* Amex*/)
            {
                // Мьютекс был брошен
            }
        }

        ~DisposableMutex()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (m_Disposed)
                return;

            if (disposing)
            {
                // Освобождение управляемых ресурсов
            }

            // Освобождение неуправляемых ресурсов
            m_Mutex.ReleaseMutex();
            m_Mutex.Close();

            m_Disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
