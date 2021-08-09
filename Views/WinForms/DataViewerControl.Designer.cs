namespace View.WinForms
{
    partial class DataViewerControl
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

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.headTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // headTitle
            // 
            this.headTitle.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.headTitle.Dock = System.Windows.Forms.DockStyle.Top;
            this.headTitle.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.headTitle.Location = new System.Drawing.Point(0, 0);
            this.headTitle.Name = "headTitle";
            this.headTitle.Size = new System.Drawing.Size(458, 62);
            this.headTitle.TabIndex = 0;
            this.headTitle.Text = "Bytes Viewer:";
            this.headTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // DataViewerControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.headTitle);
            this.DoubleBuffered = true;
            this.Name = "DataViewerControl";
            this.Size = new System.Drawing.Size(458, 419);
            this.Resize += new System.EventHandler(this.UserViewCtl_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label headTitle;
    }
}
