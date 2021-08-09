using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Cryptosoft.TypesGallery.BaseTypes;
using Cryptosoft.TypesGallery.Commands;
using Cryptosoft.TypesGallery.Events;
using Cryptosoft.TypesGallery.MVPVM;

namespace HexViewer.CoreLogic
{
    public class ReadingFile : IBusinesLogicLayer
    {
        // Емкость представления.
        private const long SizeOfAccessor = 0x01000000;

        // Массив представлений заданного размера для отображения всего файла.
        private MemoryMappedViewAccessor m_Accessor1;
        private MemoryMappedViewAccessor m_Accessor2;

        // Номер текущих представлений среди их общего числа.
        private long m_CurrentStartNumberOfAcsr;

        // Директория файла.
        private string m_FileName;

        // Проецирование файла.
        private MemoryMappedFile m_MMFile;

        // Переменная, определяющая работу с представлениями.
        private int m_State;

        // Массив полученных байтов с проецирования.
        public byte[] MMFBytes { get; set; }

        // Смещение проецирования.
        public long StartOffset { get; set; }

        // Длина чтения byte из MMF.
        public int RangeRead { get; set; }
        public bool IsChanged => false;

        public ITypesFactory Factory => throw new NotImplementedException();

        public CommandTargetSite CommandSite => throw new NotImplementedException();

        public IRoutedEventsNode RoutedParent => throw new NotImplementedException();

        public RoutedEventSite RoutedSite => throw new NotImplementedException();


        /// <summary>
        ///     Задание пути к файлу, передача View размера файла.
        /// </summary>
        /// <param name="filePath">Директория</param>
        /// <returns></returns>
        public long SetFile(string filePath)
        {
            m_FileName = filePath;
            var FileInfo = new FileInfo(filePath);
            var MaxFileSize = FileInfo.Length;
            StartOffset = 0;
            m_State = 0;
            CreateMMF();
            return MaxFileSize;
        }

        /// <summary>
        ///     Размещение в памяти файла.
        /// </summary>
        private void CreateMMF()
        {
            // Освобождаем ресурсы предыдущего файла.
            m_Accessor1?.Dispose();
            m_Accessor1 = null;
            m_Accessor2?.Dispose();
            m_Accessor2 = null;
            m_MMFile?.Dispose();
            // Состояние для создания представлений в ReadFile().
            m_State = 0;
            var FileInfo = new FileInfo(m_FileName);
            m_MMFile = MemoryMappedFile.CreateFromFile(m_FileName, FileMode.Open, "fileHandle", FileInfo.Length);
        }

        /// <summary>
        ///     Чтение спроецированного файла; работа с Accessor'ами.
        /// </summary>
        public void ReadFile()
        {
            var FileLength = new FileInfo(m_FileName).Length;
            // Проверяем область чтения на прeвышение размера файла.
            if (StartOffset + RangeRead >= FileLength) RangeRead = (int) (FileLength - StartOffset);
            MMFBytes = null;
            MMFBytes = new byte[RangeRead];

            switch (m_State)
            {
                // Единовременное создание Accessor'ов при выборе файла.
                case 0:
                    if (FileLength <= SizeOfAccessor)
                    {
                        m_Accessor1 = m_MMFile.CreateViewAccessor(0L, 0L);
                        m_State = 1;
                        goto case 1;
                    }

                    m_Accessor1 = m_MMFile.CreateViewAccessor(0, SizeOfAccessor);
                    // Проверяем, нужно ли второе представление стандартного размера или меньше.
                    var CheckSize = m_Accessor1.Capacity + SizeOfAccessor >= FileLength
                        ? FileLength - m_Accessor1.Capacity
                        : SizeOfAccessor;
                    m_Accessor2 = m_MMFile.CreateViewAccessor(SizeOfAccessor, CheckSize);

                    m_CurrentStartNumberOfAcsr = 0;
                    m_State = 2;
                    goto case 2;
                // Работа с одним представлением.
                case 1:
                    m_Accessor1.ReadArray(StartOffset, MMFBytes, 0, RangeRead);
                    m_State = 1;
                    break;
                // Работа с двумя представлениями.
                case 2:
                    // Если необходимо создать представления за верхней границей - прямое чтение.
                    if (StartOffset + RangeRead > (m_CurrentStartNumberOfAcsr + 1) * SizeOfAccessor +
                        m_Accessor2.Capacity)
                    {
                        // Если новое представление крайнее - проверить вместимость.
                        var NewIndex = StartOffset / SizeOfAccessor;
                        var NewSize = NewIndex * SizeOfAccessor + SizeOfAccessor > FileLength
                            ? FileLength - NewIndex * SizeOfAccessor
                            : SizeOfAccessor;
                        m_Accessor1.Dispose();
                        m_Accessor1 = m_Accessor2;
                        m_Accessor2 = m_MMFile.CreateViewAccessor(NewIndex * SizeOfAccessor, NewSize);
                        m_CurrentStartNumberOfAcsr = NewIndex - 1;
                        goto case 2;
                    }

                    // Если необходимо создать представления за нижней границей - обратное чтение.
                    else if (StartOffset < m_CurrentStartNumberOfAcsr * SizeOfAccessor)
                    {
                        // Если новое представление крайнее - проверить вместимость.
                        var NewIndex = StartOffset / SizeOfAccessor;
                        m_Accessor2.Dispose();
                        m_Accessor2 = m_Accessor1;
                        m_Accessor1 = m_MMFile.CreateViewAccessor(NewIndex * SizeOfAccessor, SizeOfAccessor);
                        m_CurrentStartNumberOfAcsr = NewIndex;
                        goto case 2;
                    }

                    // Если созданы представления на заданную область.
                    else
                    {
                        // Считываем данные только с первого представления, пока смещение и длина чтения в его диапазоне.
                        if (StartOffset + RangeRead < m_CurrentStartNumberOfAcsr * SizeOfAccessor +
                            m_Accessor1.Capacity)
                        {
                            m_Accessor1.ReadArray(
                                StartOffset - m_CurrentStartNumberOfAcsr * SizeOfAccessor,
                                MMFBytes, 0, RangeRead);
                            break;
                        }

                        // Если необходмо считывать данные на пересечении двух представлений.
                        // Определяем точку разрыва двух представлений в длине чтения (сколько байт читать из первого представления).
                        // (0 - чтение первого завершено).
                        var BreakPoint = m_CurrentStartNumberOfAcsr * SizeOfAccessor +
                                         m_Accessor1.Capacity - StartOffset <= 0
                            ? 0
                            : (int) (m_CurrentStartNumberOfAcsr * SizeOfAccessor +
                                     m_Accessor1.Capacity - StartOffset);
                        // Если чтение за пределами первого представления.
                        if (BreakPoint == 0)
                        {
                            m_Accessor2.ReadArray(
                                StartOffset - (m_CurrentStartNumberOfAcsr + 1) * SizeOfAccessor, MMFBytes, 0,
                                RangeRead);
                            break;
                        }

                        // Чтение в пределах первого и второго представлений.
                        m_Accessor1.ReadArray(
                            StartOffset - m_CurrentStartNumberOfAcsr * SizeOfAccessor, MMFBytes, 0, BreakPoint);
                        m_Accessor2.ReadArray(0, MMFBytes, BreakPoint, RangeRead - BreakPoint);
                    }

                    break;
            }
        }
    }
}