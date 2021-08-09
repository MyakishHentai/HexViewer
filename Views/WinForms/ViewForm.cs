using System;
using System.Windows.Forms;
using Cryptosoft.TypesGallery.MVPVM;
using GeneralViewModel.Interfaces;

namespace View.WinForms
{
    public partial class ViewForm : Form, IMainView
    {
        // VM.
        private IMainViewModel m_Model;

        // Количество видимых строк на вывод.
        private int m_VisibleLines;

        public ViewForm()
        {
            // ScrollBar (int) - не работает с большими файлами
            // (если только перед этим сравнить обычный o_O).
            InitializeComponent();
            m_ScrollBar.Maximum = 0;
            m_ScrLong.Maximum = 0;
        }

        public IMainViewModel Model
        {
            get => DataSource as IMainViewModel;
            set
            {
                if (m_Model == value)
                    return;
                DataSource = value;
            }
        }

        public object DataSource { get; set; }

        public event EventHandler<ViewClosedEventArgs> ViewClosed;

        public void Activate(IView parentView)
        {
            Application.EnableVisualStyles();
            Application.Run(this);
        }

        public void OnHide()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        ///     Событие создания диалога для выбора файла и его последующего чтения.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var Ofd = new OpenFileDialog();
            try
            {
                if (Ofd.ShowDialog() == DialogResult.OK)
                {
                    var FileName = Ofd.FileName;
                    m_ViewData.FileName = FileName;
                    m_VisibleLines = m_ViewData.VisibleLines;
                    Model.VisibleLines = m_VisibleLines;
                    Model.FilePath = FileName;

                    m_ScrLong.Maximum = Model.MaxLines;

                    UpdateView();
                }
            }
            catch (Exception Exception)
            {
                ClearView();
                MessageBox.Show(Exception.Message, "Error Viewer of File", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }


        /// <summary>
        ///     Очистка отображения.
        /// </summary>
        private void ClearView()
        {
            m_ViewData.Offset = null;
            m_ViewData.Hex = null;
            m_ViewData.Text = null;
            m_ViewData.StartDrawing();
        }


        /// <summary>
        ///     Обновление выводимой информации в Control.
        /// </summary>
        private void UpdateView()
        {
            m_ViewData.Offset = Model.Offset;
            m_ViewData.Hex = Model.Hex;
            m_ViewData.Text = Model.Text;

            m_ViewData.StartDrawing();
        }

        /// <summary>
        ///     Событие по изменению размеров окна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewForm_Resize(object sender, EventArgs e)
        {
            try
            {
                m_VisibleLines = m_ViewData.VisibleLines;
                Model.VisibleLines = m_VisibleLines;
                Model.ChangeScrolls();
                m_ScrLong.Maximum = Model.MaxLines;
                UpdateView();
            }
            catch (Exception Exception)
            {
                ClearView();
                MessageBox.Show(Exception.Message, "Error Viewer of File", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }


        /// <summary>
        ///     События по нажатию ToolStripExit - завершение работы.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        /// <summary>
        ///     Перемещение ползунка <see cref="m_ScrollBar"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_ScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                Model.Value = m_ScrollBar.Value;
                UpdateView();
            }
            catch (Exception Exception)
            {
                ClearView();
                MessageBox.Show(Exception.Message, "Error Viewer of File", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        ///     Перемещение ползунка <see cref="m_ScrLong"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void m_ScrLong_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                Model.Value = m_ScrLong.Value;
                UpdateView();
                m_ScrLong.Focus();
            }
            catch (Exception Exception)
            {
                ClearView();
                MessageBox.Show(Exception.Message, "Error Viewer of File", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }

        /// <summary>
        ///     Событие по обработке нажатий клавиш на ScrollLong.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void scrollBarLong_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.NumPad2:
                    m_ScrLong.Value = m_ScrLong.Value + 1 >= m_ScrLong.Maximum
                        ? m_ScrLong.Maximum
                        : m_ScrLong.Value + 1;
                    break;
                case Keys.NumPad8:
                    m_ScrLong.Value = m_ScrLong.Value - 1 <= 0 ? 0 : m_ScrLong.Value - 1;
                    break;
                case Keys.NumPad4:
                    m_ScrLong.Value = m_ScrLong.Value - m_VisibleLines <= 0 ? 0 : m_ScrLong.Value - m_VisibleLines;
                    break;
                case Keys.NumPad6:
                    m_ScrLong.Value = m_ScrLong.Value + m_VisibleLines >= m_ScrLong.Maximum
                        ? m_ScrLong.Maximum
                        : m_ScrLong.Value + m_VisibleLines;
                    break;
                case Keys.PageDown:
                    m_ScrLong.Value = m_ScrLong.Value - m_VisibleLines <= 0 ? 0 : m_ScrLong.Value - m_VisibleLines;
                    break;
                case Keys.PageUp:
                    m_ScrLong.Value = m_ScrLong.Value + m_VisibleLines >= m_ScrLong.Maximum
                        ? m_ScrLong.Maximum
                        : m_ScrLong.Value + m_VisibleLines;
                    break;
                case Keys.Home:
                    m_ScrLong.Value = 0;
                    break;
                case Keys.End:
                    m_ScrLong.Value = m_ScrLong.Maximum;
                    break;
                case Keys.Enter:
                    openFileToolStripMenuItem_Click(this, EventArgs.Empty);
                    break;
                case Keys.Escape:
                    exitToolStripMenuItem_Click(this, EventArgs.Empty);
                    break;
            }
        }
    }
}