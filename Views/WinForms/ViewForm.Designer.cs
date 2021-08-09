using View.WinForms;

namespace View.WinForms
{
    partial class ViewForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ViewForm));
            this.m_OpenFileTool = new System.Windows.Forms.ToolStripMenuItem();
            this.m_ExitTool = new System.Windows.Forms.ToolStripMenuItem();
            this.m_MenuStrip = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_ScrollBar = new System.Windows.Forms.VScrollBar();
            this.m_VeiwPanel = new System.Windows.Forms.Panel();
            this.m_ViewData = new View.WinForms.DataViewerControl();
            this.m_ScrLong = new View.WinForms.ScrollBarLong();
            this.m_ContextMenu1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_ContextMenu2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.m_MenuStrip.SuspendLayout();
            this.m_VeiwPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_OpenFileTool
            // 
            this.m_OpenFileTool.Image = ((System.Drawing.Image)(resources.GetObject("m_OpenFileTool.Image")));
            this.m_OpenFileTool.Name = "m_OpenFileTool";
            this.m_OpenFileTool.Size = new System.Drawing.Size(121, 22);
            this.m_OpenFileTool.Text = "OpenFile";
            this.m_OpenFileTool.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // m_ExitTool
            // 
            this.m_ExitTool.Image = ((System.Drawing.Image)(resources.GetObject("m_ExitTool.Image")));
            this.m_ExitTool.Name = "m_ExitTool";
            this.m_ExitTool.Size = new System.Drawing.Size(121, 22);
            this.m_ExitTool.Text = "Exit";
            this.m_ExitTool.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // m_MenuStrip
            // 
            this.m_MenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.m_MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.m_MenuStrip.Location = new System.Drawing.Point(0, 0);
            this.m_MenuStrip.Name = "m_MenuStrip";
            this.m_MenuStrip.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.m_MenuStrip.Size = new System.Drawing.Size(529, 24);
            this.m_MenuStrip.TabIndex = 2;
            this.m_MenuStrip.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_OpenFileTool,
            this.m_ExitTool});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // m_ScrollBar
            // 
            this.m_ScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.m_ScrollBar.LargeChange = 1;
            this.m_ScrollBar.Location = new System.Drawing.Point(504, 24);
            this.m_ScrollBar.Maximum = 0;
            this.m_ScrollBar.Name = "m_ScrollBar";
            this.m_ScrollBar.Size = new System.Drawing.Size(25, 303);
            this.m_ScrollBar.TabIndex = 5;
            this.m_ScrollBar.Visible = false;
            this.m_ScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.m_ScrollBar_Scroll);
            // 
            // m_VeiwPanel
            // 
            this.m_VeiwPanel.Controls.Add(this.m_ViewData);
            this.m_VeiwPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_VeiwPanel.Location = new System.Drawing.Point(0, 24);
            this.m_VeiwPanel.Margin = new System.Windows.Forms.Padding(2);
            this.m_VeiwPanel.Name = "m_VeiwPanel";
            this.m_VeiwPanel.Size = new System.Drawing.Size(484, 303);
            this.m_VeiwPanel.TabIndex = 7;
            // 
            // m_ViewData
            // 
            this.m_ViewData.BackColor = System.Drawing.Color.Black;
            this.m_ViewData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ViewData.FileName = null;
            this.m_ViewData.Hex = null;
            this.m_ViewData.Location = new System.Drawing.Point(0, 0);
            this.m_ViewData.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.m_ViewData.Name = "m_ViewData";
            this.m_ViewData.Offset = null;
            this.m_ViewData.Size = new System.Drawing.Size(484, 303);
            this.m_ViewData.TabIndex = 0;
            this.m_ViewData.VisibleLines = 14;
            // 
            // m_ScrLong
            // 
            this.m_ScrLong.BackColor = System.Drawing.SystemColors.Control;
            this.m_ScrLong.BorderColor = System.Drawing.Color.Silver;
            this.m_ScrLong.Dock = System.Windows.Forms.DockStyle.Right;
            this.m_ScrLong.Location = new System.Drawing.Point(484, 24);
            this.m_ScrLong.Margin = new System.Windows.Forms.Padding(2);
            this.m_ScrLong.Maximum = ((long)(100));
            this.m_ScrLong.Name = "m_ScrLong";
            this.m_ScrLong.Orientation = System.Windows.Forms.ScrollOrientation.VerticalScroll;
            this.m_ScrLong.Size = new System.Drawing.Size(20, 303);
            this.m_ScrLong.SmallStep = ((long)(1));
            this.m_ScrLong.TabIndex = 6;
            this.m_ScrLong.Text = "scrollBarLong";
            this.m_ScrLong.ThumbColor = System.Drawing.Color.Gray;
            this.m_ScrLong.ThumbSize = 10;
            this.m_ScrLong.Value = ((long)(0));
            this.m_ScrLong.ValueChanged += new System.EventHandler(this.m_ScrLong_ValueChanged);
            this.m_ScrLong.KeyDown += new System.Windows.Forms.KeyEventHandler(this.scrollBarLong_KeyDown);
            // 
            // m_ContextMenu1
            // 
            this.m_ContextMenu1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.m_ContextMenu1.Name = "contextMenuStrip1";
            this.m_ContextMenu1.Size = new System.Drawing.Size(61, 4);
            // 
            // m_ContextMenu2
            // 
            this.m_ContextMenu2.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.m_ContextMenu2.Name = "contextMenuStrip2";
            this.m_ContextMenu2.Size = new System.Drawing.Size(61, 4);
            // 
            // ViewForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(529, 327);
            this.Controls.Add(this.m_VeiwPanel);
            this.Controls.Add(this.m_ScrLong);
            this.Controls.Add(this.m_ScrollBar);
            this.Controls.Add(this.m_MenuStrip);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.m_MenuStrip;
            this.MinimumSize = new System.Drawing.Size(228, 249);
            this.Name = "ViewForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinForms - HexViewer";
            this.Resize += new System.EventHandler(this.ViewForm_Resize);
            this.m_MenuStrip.ResumeLayout(false);
            this.m_MenuStrip.PerformLayout();
            this.m_VeiwPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip m_MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem m_OpenFileTool;
        private System.Windows.Forms.ToolStripMenuItem m_ExitTool;
        private System.Windows.Forms.VScrollBar m_ScrollBar;
        private ScrollBarLong m_ScrLong;
        private System.Windows.Forms.Panel m_VeiwPanel;
        private DataViewerControl m_ViewData;
        private System.Windows.Forms.ContextMenuStrip m_ContextMenu1;
        private System.Windows.Forms.ContextMenuStrip m_ContextMenu2;
    }
}

