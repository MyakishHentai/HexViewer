using System;
using System.Drawing;
using System.Windows.Forms;

namespace View.WinForms
{
    public sealed partial class DataViewerControl : UserControl
    {

        // Размер в pt шрифта для вывода.
        private const float PtSize = 12.0f;

        // Стиль шрифта.
        private new const string Font = "Courier New";

        // Кисть.
        private readonly SolidBrush m_DrawBrush = new SolidBrush(Color.LimeGreen);

        // Высота выводмого текста.
        private readonly int m_TextHeight;

        // Путь к файлу - отображение в заголовке.
        private string m_FileName;

        // Строка, содержащая данные по смещению.
        public string Offset { get; set; }

        // Строка, содержащая данные по 16 данным.
        public string Hex { get; set; }

        // Строка, содержащая данные по текстовому представлению.
        public string Text { get; set; }

        // Путь файла.
        public string FileName
        {
            get => m_FileName;
            set
            {
                m_FileName = value;
                headTitle.Text = "HexViewer:\n" + value;
            }
        }

        public DataViewerControl()
        {
            InitializeComponent();
            var DrawFont = new Font(Font, PtSize);
            var Len = TextRenderer.MeasureText("FF", DrawFont);
            m_TextHeight = Len.Height;
            DrawFont.Dispose();
        }



        // Количество видимых строк для вывода.
        public int VisibleLines { get; set; }

        public void StartDrawing()
        {
            Invalidate();
        }

        /// <summary>
        ///     Перегружаем метод для корректной отрисовки данных.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            using (var DrawFont = new Font(Font, PtSize))
            {
                float WidthOffsetStr = 0;
                float WidthBytesStr = 0;
                // Подсчитываем отступы на основе заданного стиля и размера шрифта.
                if ( !string.IsNullOrEmpty(Offset) && Offset.Length > 0)
                {
                    var TempStr = "";
                    for (var i = 0; i < Offset.IndexOf('\n'); i++) TempStr += "F";
                    TempStr += "   ";
                    var Len = TextRenderer.MeasureText(TempStr, DrawFont);
                    WidthOffsetStr = Len.Width;

                    TempStr = "  ";
                    var PointEnter = Hex.IndexOf('\n') != -1 ? Hex.IndexOf('\n') : Hex.Length;
                    for (var i = 0; i < PointEnter; i++) TempStr += "F";
                    Len = TextRenderer.MeasureText(TempStr, DrawFont);
                    WidthBytesStr = Len.Width;
                }

                var XPositionView = WidthOffsetStr + WidthBytesStr + WidthBytesStr / 2 < Width
                    ? Width - (WidthOffsetStr + WidthBytesStr + WidthBytesStr / 2) -
                      (Width - (WidthOffsetStr + WidthBytesStr + WidthBytesStr / 2)) / 2
                    : 0.0F;
                var YPositionView = headTitle.Height + 10.0F;
                var DrawFormat = new StringFormat();
                e.Graphics.DrawString(Offset, DrawFont, m_DrawBrush, XPositionView, YPositionView, DrawFormat);
                // прочитать первую строку из офсета, узнать длину в пикселях для заданного размера шрифта
                XPositionView += WidthOffsetStr;
                e.Graphics.DrawString(Hex, DrawFont, m_DrawBrush, XPositionView, YPositionView, DrawFormat);
                XPositionView += WidthBytesStr;
                e.Graphics.DrawString(Text, DrawFont, m_DrawBrush, XPositionView, YPositionView, DrawFormat);
            }
        }

        /// <summary>
        ///     Событие при изменении размеров контрола (посредством родительской формы).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserViewCtl_Resize(object sender, EventArgs e)
        {
            float TextHeight = Height - headTitle.Height;
            VisibleLines = (int) Math.Floor(TextHeight / m_TextHeight);
        }
    }
}