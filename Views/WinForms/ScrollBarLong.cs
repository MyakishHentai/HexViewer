using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace View.WinForms
{
    [DefaultEvent("ValueChanged")]
    public class ScrollBarLong : ScrollableControl
    {
        private Color m_BorderColor = Color.Silver;

        private long m_Maximum = 100;

        private ScrollOrientation m_Orientation;

        private Color m_ThumbColor = Color.Gray;

        private int m_ThumbSize = 10;
        private long m_Value;

        public ScrollBarLong()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.UserPaint,
                true);

            SmallStep = 1;
            ShowButtons = true;
        }

        public long Value
        {
            get => m_Value;
            set
            {
                if (this.m_Value == value)
                    return;
                this.m_Value = value;
                Refresh();
                OnScroll();
            }
        }

        public long Maximum
        {
            get => m_Maximum;
            set
            {
                m_Maximum = value;
                Invalidate();
            }
        }

        public int ThumbSize
        {
            get => m_ThumbSize;
            set
            {
                m_ThumbSize = value;
                Invalidate();
            }
        }

        public Color ThumbColor
        {
            get => m_ThumbColor;
            set
            {
                m_ThumbColor = value;
                Invalidate();
            }
        }

        public Color BorderColor
        {
            get => m_BorderColor;
            set
            {
                m_BorderColor = value;
                Invalidate();
            }
        }

        public ScrollOrientation Orientation
        {
            get => m_Orientation;
            set
            {
                m_Orientation = value;
                Invalidate();
            }
        }

        [DefaultValue(1)] public long SmallStep { get; set; }

        [DefaultValue(true)] public bool ShowButtons { get; set; }

        private int ButtonPadding => ShowButtons ? 9 : 0;

        public event EventHandler ValueChanged;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseScroll(e);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                MouseScroll(e);
            base.OnMouseMove(e);
        }

        private void MouseScroll(MouseEventArgs e)
        {
            var V = Value;
            var Pad = ButtonPadding;

            switch (Orientation)
            {
                case ScrollOrientation.VerticalScroll:
                    if (e.Y < Pad) V -= SmallStep;
                    else if (e.Y > Height - Pad) V += SmallStep;
                    else V = Maximum * (e.Y - m_ThumbSize / 2 - Pad) / (Height - m_ThumbSize - Pad * 2);
                    break;
                case ScrollOrientation.HorizontalScroll:
                    if (e.X < Pad) V -= SmallStep;
                    else if (e.X > Width - Pad) V += SmallStep;
                    else V = Maximum * (e.X - m_ThumbSize / 2 - Pad) / (Width - m_ThumbSize - Pad * 2);
                    break;
            }

            Value = Math.Max(0, Math.Min(Maximum, V));
        }

        public virtual void OnScroll(ScrollEventType type = ScrollEventType.ThumbPosition)
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Maximum <= 0)
                return;

            var W = Width;
            var H = Height;

            var ThumbRect = Rectangle.Empty;

            switch (Orientation)
            {
                case ScrollOrientation.HorizontalScroll:
                    W -= ButtonPadding * 2;
                    ThumbRect = new Rectangle((int) (m_Value * (W - m_ThumbSize) / Maximum) + ButtonPadding, 2, m_ThumbSize,
                        H - 4);
                    if (ShowButtons)
                        using (var Pen = new Pen(BorderColor, 4) {StartCap = LineCap.ArrowAnchor})
                        {
                            e.Graphics.DrawLine(Pen, 0, Height / 2, ButtonPadding - 3, Height / 2);
                            e.Graphics.DrawLine(Pen, Width - 1, Height / 2, Width - ButtonPadding + 2, Height / 2);
                        }

                    break;
                case ScrollOrientation.VerticalScroll:
                    H -= ButtonPadding * 2;
                    ThumbRect = new Rectangle(2, (int) (m_Value * (H - m_ThumbSize) / Maximum) + ButtonPadding, W - 4,
                        m_ThumbSize);

                    if (ShowButtons)
                        using (var Pen = new Pen(BorderColor, 4) {StartCap = LineCap.ArrowAnchor})
                        {
                            e.Graphics.DrawLine(Pen, Width / 2, 0, Width / 2, ButtonPadding - 3);
                            e.Graphics.DrawLine(Pen, Width / 2, Height - 1, Width / 2, Height - ButtonPadding + 2);
                        }

                    break;
            }

            using (var Brush = new SolidBrush(m_ThumbColor))
            {
                e.Graphics.FillRectangle(Brush, ThumbRect);
            }

            using (var Pen = new Pen(m_BorderColor))
            {
                switch (Orientation)
                {
                    case ScrollOrientation.HorizontalScroll:
                        e.Graphics.DrawRectangle(Pen, new Rectangle(ButtonPadding, 0, W - 1, H - 1));
                        break;
                    case ScrollOrientation.VerticalScroll:
                        e.Graphics.DrawRectangle(Pen, new Rectangle(0, ButtonPadding, W - 1, H - 1));
                        break;
                }
            }
        }
    }
}