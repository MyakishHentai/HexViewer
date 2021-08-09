using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Cryptosoft.TypesGallery.MVPVM;
using GeneralViewModel.Interfaces;
using HexViewer.CoreLogic;
using ReactiveUI;
using ReactiveObj = HexViewer.GenericTypes;

namespace HexViewer
{
    public class ViewModel : ReactiveObj.ViewModelBase, IMainViewModel
    {
        // Длина вывода - сколько символов в строке.
        private const int BytesInLine = 8;

        // Размер области отображения.
        private Rect m_BlockHeight;

        // Размер файла.
        private long m_FileLength;

        private ReadingFile m_FileMapped; // = new ReadingFile();

        // Директория файла.
        private string m_FilePath = "Choose new file:";

        // Размер выводимых символов.
        private double m_FontSize = 15;

        // Строка байт в 16-ом представлении.
        private string m_Hex = "Hex";

        // Высота выводимой строки (dep кол-во видимых строк).
        private double m_LineHeight = 20.0;

        // Переменная для работы с большими файлами.
        private int m_LinesToIndex;

        // Количество строк в файле для отображения.
        private long m_MaxLines;

        // Строка смещений.
        private string m_Offset = "Offset";

        // Страница - видимые строки.
        private int m_Page;

        // Строка текстового содержимого файла.
        private string m_Text = "Text";

        // Позиция каретки.
        private double m_Value;

        // Количество видимых строк.
        private int m_VisibleLines;

        public double FontSize
        {
            get => m_FontSize;
            set => this.RaiseAndSetIfChanged(ref m_FontSize, value);
        }

        // Начальное смещение.
        public long StartOffset { get; set; }

        // Разрядность строки Offset.
        public int HexNumberLength { get; set; }

        // Нужно ли перерисовать область.
        public bool NeedRedrawing { get; set; }

        // Байты файла.
        public byte[] Bytes => m_FileMapped.MMFBytes;

        public Rect BlockHeight
        {
            get => m_BlockHeight;
            set
            {
                this.RaiseAndSetIfChanged(ref m_BlockHeight, value);
                VisibleLines = (int) Math.Floor(m_BlockHeight.Height / LineHeight);
                if (NeedRedrawing)
                {
                    m_FileMapped.RangeRead = VisibleLines * BytesInLine;
                    ChangeScrolls();
                }
            }
        }

        public double LineHeight
        {
            get => m_LineHeight;
            set => this.RaiseAndSetIfChanged(ref m_LineHeight, value);
        }

        // Модель для работы с файлом.
        public new IBusinesLogicLayer Content
        {
            get => m_FileMapped;
            set => m_FileMapped = (ReadingFile) value;
        }

        public int VisibleLines
        {
            get => m_VisibleLines;
            set => this.RaiseAndSetIfChanged(ref m_VisibleLines, value);
        }

        public double Value
        {
            get => m_Value;
            set
            {
                if (NeedRedrawing) UpdateFileDisplay(value);
                this.RaiseAndSetIfChanged(ref m_Value, value);
            }
        }

        public long MaxLines
        {
            get => m_MaxLines;
            set => this.RaiseAndSetIfChanged(ref m_MaxLines, value);
        }

        public string FilePath
        {
            get => m_FilePath;
            set
            {
                this.RaiseAndSetIfChanged(ref m_FilePath, value);
                ReadFile();
            }
        }

        public string Offset
        {
            get => m_Offset;
            set => this.RaiseAndSetIfChanged(ref m_Offset, value);
        }

        public string Hex
        {
            get => m_Hex;
            set => this.RaiseAndSetIfChanged(ref m_Hex, value);
        }

        public string Text
        {
            get => m_Text;
            set => this.RaiseAndSetIfChanged(ref m_Text, value);
        }


        /// <summary>
        ///     Метод преобразует байты в шестнадцатиричное представление.
        /// </summary>
        private void PrintBytes()
        {
            Hex = null;
            var IndexSeparator = 0;
            foreach (var Byte in Bytes)
            {
                IndexSeparator++;
                // Если конец строки, то добавляем \n в ее конец.
                if (IndexSeparator == BytesInLine)
                {
                    Hex += NumberToHex(Byte, 2) + "\n";
                    IndexSeparator = 0;
                }
                else
                {
                    Hex += NumberToHex(Byte, 2) + " ";
                }
            }
        }


        /// <summary>
        ///     Метод преобразует байты в символьное представление.
        /// </summary>
        private void PrintChars()
        {
            Text = null;
            var IndexSeparator = 0;
            foreach (var Byte in Bytes)
            {
                var C = Convert.ToChar(Byte);
                IndexSeparator++;
                // Если конец строки, то добавляем \n в ее конец.
                if (IndexSeparator == BytesInLine)
                {
                    Text += (char.IsControl(C) ? "." : C.ToString()) + "\n";
                    IndexSeparator = 0;
                }
                else
                {
                    Text += char.IsControl(C) ? "." : C.ToString();
                }
            }
        }


        /// <summary>
        ///     Метод преобразует число в строковое, шестнадцатирицное представление.
        /// </summary>
        /// <param name="number">Число</param>
        /// <param name="length">Длина</param>
        /// <returns>Число в строковом, шестнадцатирицном представлении.</returns>
        private string NumberToHex(long number, int length)
        {
            var HexNumber = number.ToString("X");
            var ZerosNumber = length - HexNumber.Length;
            var Result = new string('0', ZerosNumber);
            Result += HexNumber;
            return Result;
        }


        /// <summary>
        ///     Метод преобразующий получнные байты.
        /// </summary>
        public void CreateStrings()
        {
            if (Bytes != null)
            {
                Offset = null;
                var LocalOffset = m_FileMapped.StartOffset;
                for (var i = 0; i < Bytes.Length; i += BytesInLine)
                {
                    Offset += NumberToHex(LocalOffset, HexNumberLength);
                    LocalOffset += BytesInLine;
                    if (i + BytesInLine < Bytes.Length)
                        Offset += ":\n";
                    else
                        Offset += ": ";
                }

                PrintBytes();
                PrintChars();
            }
            else
            {
                Offset = "null";
                Hex = "null";
                Text = "null";
            }
        }


        /// <summary>
        ///     Выбор файла в новом диалоговом окне.
        /// </summary>
        /// <returns></returns>
        public void ReadFile()
        {
            Debug.WriteLine($"Opened: {FilePath}");
            try
            {
                NeedRedrawing = false;
                Value = 0;
                NeedRedrawing = true;
                m_FileLength = m_FileMapped.SetFile(FilePath);
                HexNumberLength = m_FileLength == 1 ? 1 : (int) Math.Ceiling(Math.Log(m_FileLength, 16));
                m_FileMapped.RangeRead = VisibleLines * BytesInLine;
                ChangeScrolls();
            }
            catch (Exception Exception)
            {
                NeedRedrawing = false;
                Offset = "Exception";
                Hex = Exception.Message;
                Text = "Exception";
                Debug.WriteLine(Exception.Message);
            }
        }

        #region Methods for ScrollBar

        /// <summary>
        ///     Метод изменяет скролл в зависимости от размеров окна и размера файла.
        /// </summary>
        public void ChangeScrolls()
        {
            int MaxIndex;
            m_FileLength = new FileInfo(FilePath).Length;
            var Lines = VisibleLines;
            MaxLines = (long) Math.Ceiling((double) m_FileLength / BytesInLine);
            if (MaxLines > int.MaxValue)
            {
                m_LinesToIndex = (int) Math.Ceiling((double) MaxLines / int.MaxValue);
                m_Page = Lines / m_LinesToIndex;
                MaxIndex = (int) Math.Ceiling((double) MaxLines / m_LinesToIndex) - m_Page;
            }
            else
            {
                m_LinesToIndex = 1;
                m_Page = Lines;
                MaxIndex = MaxLines <= Lines ? 0 : (int) (MaxLines - m_Page);
            }

            MaxLines = MaxIndex;
            UpdateFileDisplay(Value > MaxIndex ? MaxIndex : Value);
        }


        /// <summary>
        ///     Метод изменяет смещение в файле.
        /// </summary>
        /// <param name="value">Новая позиция каретки</param>
        public void UpdateFileDisplay(double value)
        {
            StartOffset = (long) value * (m_LinesToIndex * BytesInLine);
            m_FileMapped.StartOffset = StartOffset;
            m_FileMapped.RangeRead = VisibleLines * BytesInLine;
            m_FileMapped.ReadFile();
            CreateStrings();
        }

        #endregion
    }
}