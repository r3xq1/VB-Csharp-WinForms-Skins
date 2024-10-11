using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using static Skins.HelperMethods;

namespace Skins
{
    public static class HelperMethods
    {
        public enum MouseMode : byte
        {
            NormalMode,
            Hovered,
            Pushed
        }

        // Draws an image from a Base64 string
        public static void DrawImageFromBase64(Graphics g, string base64Image, Rectangle rect)
        {
            using var ms = new MemoryStream(Convert.FromBase64String(base64Image));
            using var img = Image.FromStream(ms);
            g.DrawImage(img, rect);
        }

        // Fills a rounded rectangle with a color
        public static void FillRoundedPath(Graphics g, Color color, Rectangle rect, int curve, bool topLeft = true,
                                            bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
        {
            using var brush = new SolidBrush(color);
            g.FillPath(brush, RoundRec(rect, curve, topLeft, topRight, bottomLeft, bottomRight));
        }

        // Overloaded method to fill with a brush
        public static void FillRoundedPath(Graphics g, Brush brush, Rectangle rect, int curve, bool topLeft = true,
                                            bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
        {
            g.FillPath(brush, RoundRec(rect, curve, topLeft, topRight, bottomLeft, bottomRight));
        }

        // Draws a rounded rectangle border
        public static void DrawRoundedPath(Graphics g, Color color, float size, Rectangle rect, int curve,
                                            bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
        {
            using var pen = new Pen(color, size);
            g.DrawPath(pen, RoundRec(rect, curve, topLeft, topRight, bottomLeft, bottomRight));
        }

        // Draws a triangle using three points
        public static void DrawTriangle(Graphics g, Color color, float size, Point p1_0, Point p1_1,
                                         Point p2_0, Point p2_1, Point p3_0, Point p3_1)
        {
            using var pen = new Pen(color, size);
            g.DrawLine(pen, p1_0, p1_1);
            g.DrawLine(pen, p2_0, p2_1);
            g.DrawLine(pen, p3_0, p3_1);
        }

        // Returns an array of points for a triangle
        public static Point[] Triangle(Color color, Point p1, Point p2, Point p3)
        {
            return [p1, p2, p3];
        }

        public static Pen PenRGBColor(Graphics g, int r, int gValue, int b, float size)
        {
            return new Pen(Color.FromArgb(r, gValue, b), size);
        }

        public static Pen PenHTMlColor(string colorWithoutHash, float size)
        {
            return new Pen(GetHTMLColor(colorWithoutHash), size);
        }

        public static SolidBrush SolidBrushRGBColor(int r, int g, int b, int a = 0)
        {
            return new SolidBrush(Color.FromArgb(a, r, g, b));
        }

        public static SolidBrush SolidBrushHTMlColor(string colorWithoutHash)
        {
            return new SolidBrush(GetHTMLColor(colorWithoutHash));
        }

        public static Color GetHTMLColor(string colorWithoutHash)
        {
            return ColorTranslator.FromHtml("#" + colorWithoutHash);
        }

        public static string ColorToHTML(Color color)
        {
            return ColorTranslator.ToHtml(color);
        }

        public static void CentreString(Graphics g, string text, Font font, Brush brush, Rectangle rect)
        {
            var format = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(text, font, brush, new Rectangle(0, rect.Y + (rect.Height / 2) - (int)(g.MeasureString(text, font).Height / 2), rect.Width, rect.Height), format);
        }

        public static void LeftString(Graphics g, string text, Font font, Brush brush, Rectangle rect)
        {
            var format = new StringFormat { Alignment = StringAlignment.Near };
            g.DrawString(text, font, brush, new Rectangle(4, rect.Y + (rect.Height / 2) - (int)(g.MeasureString(text, font).Height / 2), rect.Width, rect.Height), format);
        }

        public static void RightString(Graphics g, string text, Font font, Brush brush, Rectangle rect)
        {
            var format = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(text, font, brush, new Rectangle(4, rect.Y + (rect.Height / 2) - (int)(g.MeasureString(text, font).Height / 2), rect.Width - rect.Height + 10, rect.Height), format);
        }

        // Creates a rounded rectangle GraphicsPath
        public static GraphicsPath RoundRec(Rectangle r, int curve, bool topLeft = true,
                                             bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
        {
            var path = new GraphicsPath(FillMode.Winding);
            if (topLeft) path.AddArc(r.X, r.Y, curve, curve, 180, 90);
            else path.AddLine(r.X, r.Y, r.X, r.Y);
            if (topRight) path.AddArc(r.Right - curve, r.Y, curve, curve, 270, 90);
            else path.AddLine(r.Right - r.Width, r.Y, r.Width, r.Y);
            if (bottomRight) path.AddArc(r.Right - curve, r.Bottom - curve, curve, curve, 0, 90);
            else path.AddLine(r.Right, r.Bottom, r.Right, r.Bottom);
            if (bottomLeft) path.AddArc(r.X, r.Bottom - curve, curve, curve, 90, 90);
            else path.AddLine(r.X, r.Bottom, r.X, r.Bottom);
            path.CloseFigure();
            return path;
        }
    }

    public class AcaciaSkin : ContainerControl
    {
        // Variables
        private bool movable = false;
        private Point mousePoint = new Point(0, 0);
        private readonly int moveHeight = 50;
        private TitlePosition titleTextPosition = TitlePosition.Left;

        // Constructor
        public AcaciaSkin()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            Font = new Font("Arial", 12, FontStyle.Bold);
            UpdateStyles();
        }

        // Properties
        private bool showIcon;
        public bool ShowIcon
        {
            get => showIcon;
            set
            {
                if (value == showIcon) return;
                FindForm().ShowIcon = value;
                showIcon = value;
                Invalidate();
            }
        }

        public virtual TitlePosition TitleTextPosition
        {
            get => titleTextPosition;
            set
            {
                titleTextPosition = value;
                Invalidate();
            }
        }

        public enum TitlePosition
        {
            Left,
            Center,
            Right
        }

        // Paint event handler
        protected override void OnPaint(PaintEventArgs e)
        {
            using var bitmap = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Fuchsia);
            graphics.FillRectangle(SolidBrushHTMlColor("24273e"), new Rectangle(0, 0, Width, Height));
            graphics.FillRectangle(SolidBrushHTMlColor("1e2137"), new Rectangle(0, 0, Width, 55));
            graphics.DrawLine(PenHTMlColor("1d1f38", 1), new Point(0, 55), new Point(Width, 55));
            graphics.DrawRectangle(PenHTMlColor("1d1f38", 1), new Rectangle(0, 0, Width - 1, Height - 1));

            if (FindForm().ShowIcon && FindForm().Icon != null)
            {
                switch (titleTextPosition)
                {
                    case TitlePosition.Left:
                        graphics.DrawString(Text, Font, SolidBrushHTMlColor("e4ecf2"), 27, 18);
                        graphics.DrawIcon(FindForm().Icon, new Rectangle(5, 16, 20, 20));
                        break;
                    case TitlePosition.Center:
                        CentreString(graphics, Text, Font, SolidBrushHTMlColor("e4ecf2"), new Rectangle(0, 0, Width, 50));
                        graphics.DrawIcon(FindForm().Icon, new Rectangle(5, 16, 20, 20));
                        break;
                    case TitlePosition.Right:
                        RightString(graphics, Text, Font, SolidBrushHTMlColor("e4ecf2"), new Rectangle(0, 0, Width, 50));
                        graphics.DrawIcon(FindForm().Icon, new Rectangle(Width - 30, 16, 20, 20));
                        break;
                }
            }
            else
            {
                switch (titleTextPosition)
                {
                    case TitlePosition.Left:
                        graphics.DrawString(Text, Font, SolidBrushHTMlColor("e4ecf2"), 5, 18);
                        break;
                    case TitlePosition.Center:
                        CentreString(graphics, Text, Font, SolidBrushHTMlColor("e4ecf2"), new Rectangle(0, 0, Width, 50));
                        break;
                    case TitlePosition.Right:
                        RightString(graphics, Text, Font, SolidBrushHTMlColor("e4ecf2"), new Rectangle(0, 0, Width, 50));
                        break;
                }
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        // Mouse events
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, moveHeight).Contains(e.Location))
            {
                movable = true;
                mousePoint = e.Location;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            movable = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (movable)
            {
                var delta = new Size(e.X - mousePoint.X, e.Y - mousePoint.Y);
                var newLocation = Parent.Location;
                newLocation.Offset((Point)delta);
                Parent.Location = newLocation;
            }
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            ParentForm.FormBorderStyle = FormBorderStyle.None;
            ParentForm.Dock = DockStyle.None;
            Dock = DockStyle.Fill;
            Invalidate();
        }
    }

    public class AcaciaButton : Control
    {
        // Variables
        private MouseMode state;
        private Image sideImage;
        private SideAlign sideImageAlign = SideAlign.Left;

        // Constructor
        public AcaciaButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            UpdateStyles();
        }

        // Properties
        public Image SideImage
        {
            get => sideImage;
            set
            {
                sideImage = value;
                Invalidate();
            }
        }

        public SideAlign SideImageAlign
        {
            get => sideImageAlign;
            set
            {
                sideImageAlign = value;
                Invalidate();
            }
        }

        // Enumerators
        public enum SideAlign
        {
            Left,
            Right
        }

        // Draw Control
        protected override void OnPaint(PaintEventArgs e)
        {
            using var bitmap = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            var rect = new Rectangle(2, 2, Width - 5, Height - 5);

            switch (state)
            {
                case MouseMode.NormalMode:
                    DrawNormalMode(graphics, rect);
                    break;

                case MouseMode.Hovered:
                    Cursor = Cursors.Hand;
                    DrawHoveredMode(graphics, rect);
                    break;

                case MouseMode.Pushed:
                    DrawPushedMode(graphics, rect);
                    break;
            }

            if (sideImage != null)
            {
                DrawSideImage(graphics, rect);
            }

            CentreString(graphics, Text, Font, SolidBrushHTMlColor("e4ecf2"), rect);
            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        private void DrawNormalMode(Graphics graphics, Rectangle rect)
        {
            using var hb = new PathGradientBrush(RoundRec(new Rectangle(0, 0, Width, Height), 2));
            FillRoundedPath(graphics, SolidBrushHTMlColor("fc3955"), rect, 2);
            hb.WrapMode = WrapMode.Clamp;
            var cb = new ColorBlend(4)
            {
                Colors = [
                Color.FromArgb(220, GetHTMLColor("fc3955")),
                    Color.FromArgb(220, GetHTMLColor("fc3955")),
                    Color.FromArgb(220, GetHTMLColor("fc3955")),
                    Color.FromArgb(220, GetHTMLColor("fc3955"))
            ],
                Positions = [0.0F, 0.2F, 0.8F, 1.0F]
            };
            hb.InterpolationColors = cb;
            FillRoundedPath(graphics, hb, new Rectangle(0, 0, Width - 1, Height - 1), 2);
        }

        private void DrawHoveredMode(Graphics graphics, Rectangle rect)
        {
            using var hb = new PathGradientBrush(RoundRec(new Rectangle(0, 0, Width, Height), 2));
            FillRoundedPath(graphics, new SolidBrush(Color.FromArgb(150, GetHTMLColor("fc3955"))), rect, 2);
            hb.WrapMode = WrapMode.Clamp;
            var cb = new ColorBlend(4)
            {
                Colors = [
                Color.FromArgb(150, GetHTMLColor("fc3955")),
                    Color.FromArgb(150, GetHTMLColor("fc3955")),
                    Color.FromArgb(150, GetHTMLColor("fc3955")),
                    Color.FromArgb(150, GetHTMLColor("fc3955"))
            ],
                Positions = [0.0F, 0.2F, 0.8F, 1.0F]
            };
            hb.InterpolationColors = cb;
            FillRoundedPath(graphics, hb, new Rectangle(0, 0, Width - 1, Height - 1), 2);
        }

        private void DrawPushedMode(Graphics graphics, Rectangle rect)
        {
            using var hb = new PathGradientBrush(RoundRec(new Rectangle(0, 0, Width, Height), 2));
            FillRoundedPath(graphics, SolidBrushHTMlColor("fc3955"), rect, 2);
            hb.WrapMode = WrapMode.Clamp;
            var cb = new ColorBlend(4)
            {
                Colors = [
                Color.FromArgb(220, GetHTMLColor("fc3955")),
                    Color.FromArgb(220, GetHTMLColor("fc3955")),
                    Color.FromArgb(220, GetHTMLColor("fc3955")),
                    Color.FromArgb(220, GetHTMLColor("fc3955"))
            ],
                Positions = [0.0F, 0.2F, 0.8F, 1.0F]
            };
            hb.InterpolationColors = cb;
            FillRoundedPath(graphics, hb, new Rectangle(0, 0, Width - 1, Height - 1), 2);
        }

        private void DrawSideImage(Graphics graphics, Rectangle rect)
        {
            if (sideImageAlign == SideAlign.Right)
            {
                graphics.DrawImage(sideImage, new Rectangle(rect.Width - 24, rect.Y + 7, 16, 16));
            }
            else
            {
                graphics.DrawImage(sideImage, new Rectangle(8, rect.Y + 7, 16, 16));
            }
        }

        // Events
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            state = MouseMode.Hovered;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            state = MouseMode.Pushed;
            Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            state = MouseMode.Hovered;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            state = MouseMode.NormalMode;
            Invalidate();
        }
    }
    public class AcaciaTextbox : Control
    {
        // Variables
        private readonly TextBox textBox = new();
        private HorizontalAlignment textAlign = HorizontalAlignment.Left;
        private int maxLength = 32767;
        private bool readOnly = false;
        private bool useSystemPasswordChar = false;
        private string watermarkText = string.Empty;
        private Color textBoxColor = ColorTranslator.FromHtml("#24273e");
        private Color textColor = ColorTranslator.FromHtml("#585c73");
        public MouseMode state = MouseMode.NormalMode;
        private Color borderColor = Color.FromArgb(88, 92, 115);
        private bool showBorder = true;

        // Properties for Border
        public Color BorderColor
        {
            get => borderColor;
            set
            {
                borderColor = value;
                Invalidate();
            }
        }

        public bool ShowBorder
        {
            get => showBorder;
            set
            {
                showBorder = value;
                Invalidate();
            }
        }

        // Other existing properties
        public override Color BackColor
        {
            get => textBox.BackColor;
            set
            {
                textBox.BackColor = value;
                Invalidate();
            }
        }

        public HorizontalAlignment TextAlign
        {
            get => textAlign;
            set
            {
                textAlign = value;
                textBox.TextAlign = value;
            }
        }

        public int MaxLength
        {
            get => maxLength;
            set
            {
                maxLength = value;
                textBox.MaxLength = value;
            }
        }

        public bool ReadOnly
        {
            get => readOnly;
            set
            {
                readOnly = value;
                textBox.ReadOnly = value;
            }
        }

        public bool UseSystemPasswordChar
        {
            get => useSystemPasswordChar;
            set
            {
                useSystemPasswordChar = value;
                textBox.UseSystemPasswordChar = value;
            }
        }

        public override string Text
        {
            get => textBox.Text;
            set
            {
                textBox.Text = value;
                base.Text = value;
            }
        }

        public string WatermarkText
        {
            get => watermarkText;
            set
            {
                watermarkText = value;
                Invalidate(); // Обновление для отображения водяного знака
            }
        }

        // Constructor
        public AcaciaTextbox()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.SupportsTransparentBackColor, true);

            DoubleBuffered = true;

            textBox.Multiline = false;
            textBox.Cursor = Cursors.IBeam;
            textBox.BackColor = textBoxColor;
            textBox.ForeColor = textColor;
            textBox.BorderStyle = BorderStyle.None;
            textBox.Location = new Point(5, 5);
            textBox.Font = new Font("Arial", 11, FontStyle.Regular);
            textBox.Size = new Size(Width - 10, 30);
            textBox.UseSystemPasswordChar = useSystemPasswordChar;

            // Subscribe to events
            textBox.TextChanged += TextBox_TextChanged;
            textBox.MouseHover += TextBox_MouseHover;
            textBox.MouseLeave += TextBox_MouseLeave;
            textBox.MouseEnter += TextBox_MouseEnter;
            textBox.MouseUp += TextBox_MouseUp;
            textBox.MouseDown += TextBox_MouseDown;

            Size = new Size(135, 30);
            UpdateStyles();
        }

        // Events
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (!Controls.Contains(textBox))
                Controls.Add(textBox);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            textBox.Size = new Size(Width - 10, Height - 10);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Рисуем обводку, если она включена
            if (showBorder)
            {
                using Pen pen = new(borderColor);
                e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, Width - 1, Height - 1));
            }

            // Рисуем текст водяного знака
            if (!string.IsNullOrEmpty(watermarkText) && string.IsNullOrEmpty(Text))
            {
                using Brush brush = new SolidBrush(Color.FromArgb(150, textColor)); // Полупрозрачный цвет
                e.Graphics.DrawString(watermarkText, textBox.Font, brush, new PointF(7, 7));
            }
        }

        // Private methods for events
        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            Text = textBox.Text;
            Invalidate();
        }

        private void TextBox_MouseHover(object sender, EventArgs e)
        {
            state = MouseMode.Hovered;
            Invalidate();
        }

        private void TextBox_MouseLeave(object sender, EventArgs e)
        {
            state = MouseMode.NormalMode;
            Invalidate();
        }

        private void TextBox_MouseUp(object sender, MouseEventArgs e)
        {
            state = MouseMode.NormalMode;
            Invalidate();
        }

        private void TextBox_MouseEnter(object sender, EventArgs e)
        {
            state = MouseMode.NormalMode;
            Invalidate();
        }

        private void TextBox_MouseDown(object sender, MouseEventArgs e)
        {
            state = MouseMode.Pushed;
            Invalidate();
        }
    }
    public class AcaciaCheckBox : Control
    {
        #region Variables

        private bool _checked;
        private bool autoSize = true; // Поддержка AutoSize
        protected MouseMode state = MouseMode.NormalMode;

        #endregion

        #region Events

        public event EventHandler CheckedChanged;

        #endregion

        #region Properties

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public override bool AutoSize
        {
            get => autoSize;
            set
            {
                autoSize = value;
                if (autoSize)
                {
                    Size = new Size(CalculateWidth(), Height);
                }
            }
        }

        #endregion

        #region Constructors

        public AcaciaCheckBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor |
                      ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            Font = new Font("Arial", 11, FontStyle.Regular);
            UpdateStyles();
            Size = new Size(100, 20); // Размер по умолчанию
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            using var bitmap = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (Checked)
            {
                DrawRoundedPath(graphics, Color.FromArgb(252, 57, 85), 2.5f, new Rectangle(1, 1, 17, 17));
                FillRoundedPath(graphics, new SolidBrush(Color.FromArgb(252, 57, 85)), new Rectangle(5, 5, 9, 9));
                graphics.DrawString(Text, Font, new SolidBrush(Color.FromArgb(228, 236, 242)), new Rectangle(22, 1, Width, Height - 2), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            }
            else
            {
                switch (state)
                {
                    case MouseMode.NormalMode:
                        DrawRoundedPath(graphics, Color.FromArgb(150, 252, 57, 85), 2.5f, new Rectangle(1, 1, 17, 17));
                        graphics.DrawString(Text, Font, new SolidBrush(Color.Silver), new Rectangle(22, 1, Width, Height - 2), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                        break;

                    case MouseMode.Hovered:
                        DrawRoundedPath(graphics, Color.FromArgb(252, 57, 85), 2.5f, new Rectangle(1, 1, 17, 17));
                        FillRoundedPath(graphics, new SolidBrush(Color.FromArgb(252, 57, 85)), new Rectangle(5, 5, 9, 9));
                        graphics.DrawString(Text, Font, new SolidBrush(Color.FromArgb(228, 236, 242)), new Rectangle(22, 1, Width, Height - 2), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                        break;
                }
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        #region Event Handlers

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
            Invalidate();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (AutoSize)
            {
                Size = new Size(CalculateWidth(), Height);
            }
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = 20;
            Invalidate();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            state = MouseMode.Hovered;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            state = MouseMode.NormalMode;
            Invalidate();
        }

        #endregion

        #region Helper Methods

        private int CalculateWidth()
        {
            // Добавляем дополнительно пространство для текста и чекбокса
            return TextRenderer.MeasureText(Text, Font).Width + 30; // 30 для области чекбокса и отступов
        }

        private void DrawRoundedPath(Graphics g, Color color, float radius, Rectangle rect)
        {
            using var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            using var pen = new Pen(color);
            g.DrawPath(pen, path);
        }

        private void FillRoundedPath(Graphics g, Brush brush, Rectangle rect)
        {
            using var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, 2.5f, 2.5f, 180, 90);
            path.AddArc(rect.X + rect.Width - 2.5f, rect.Y, 2.5f, 2.5f, 270, 90);
            path.AddArc(rect.X + rect.Width - 2.5f, rect.Y + rect.Height - 2.5f, 2.5f, 2.5f, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - 2.5f, 2.5f, 2.5f, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }

        #endregion
    }
    public class AcaciaRadioButton : Control
    {
        #region Variables

        private bool _checked;
        private int _group = 1;
        private bool autoSize = true; // Поддержка AutoSize
        protected MouseMode state = MouseMode.NormalMode;

        #endregion

        #region Events

        public event EventHandler CheckedChanged;

        #endregion

        #region Properties

        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked != value)
                {
                    _checked = value;
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                    Invalidate();
                }
            }
        }

        public int Group
        {
            get => _group;
            set
            {
                _group = value;
                Invalidate();
            }
        }

        public override bool AutoSize
        {
            get => autoSize;
            set
            {
                autoSize = value;
                if (autoSize)
                {
                    Size = new Size(CalculateWidth(), Height);
                }
            }
        }

        #endregion

        #region Constructors

        public AcaciaRadioButton()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor |
                      ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            Font = new Font("Arial", 11, FontStyle.Regular);
            UpdateStyles();
            Size = new Size(100, 21); // Размер по умолчанию
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            using var bitmap = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            if (Checked)
            {
                graphics.DrawEllipse(new Pen(Color.FromArgb(252, 57, 85), 2.8f), new Rectangle(1, 1, 18, 18));
                graphics.FillEllipse(new SolidBrush(Color.FromArgb(252, 57, 85)), new Rectangle(5, 5, 10, 10));
                graphics.DrawString(Text, Font, new SolidBrush(Color.FromArgb(228, 236, 242)), new Rectangle(22, 1, Width, Height - 2), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            }
            else
            {
                switch (state)
                {
                    case MouseMode.NormalMode:
                        graphics.DrawEllipse(new Pen(Color.FromArgb(150, 252, 57, 85), 2.8f), new Rectangle(1, 1, 18, 18));
                        graphics.DrawString(Text, Font, new SolidBrush(Color.Silver), new Rectangle(22, 1, Width, Height - 2), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                        break;

                    case MouseMode.Hovered:
                        graphics.DrawEllipse(new Pen(Color.FromArgb(252, 57, 85), 2.8f), new Rectangle(1, 1, 18, 18));
                        graphics.FillEllipse(new SolidBrush(Color.FromArgb(252, 57, 85)), new Rectangle(5, 5, 10, 10));
                        graphics.DrawString(Text, Font, new SolidBrush(Color.FromArgb(228, 236, 242)), new Rectangle(22, 1, Width, Height - 2), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                        break;
                }
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        #region Event Handlers

        private void UpdateState()
        {
            if (!IsHandleCreated || !Checked) return;
            foreach (Control c in Parent.Controls)
            {
                if (c != this && c is AcaciaRadioButton radioButton && radioButton.Group == _group)
                {
                    radioButton.Checked = false;
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            UpdateState();
            base.OnClick(e);
            Invalidate();
        }

        protected override void OnCreateControl()
        {
            UpdateState();
            base.OnCreateControl();
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            if (AutoSize)
            {
                Size = new Size(CalculateWidth(), Height);
            }
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = 21;
            Invalidate();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            state = MouseMode.Hovered;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            state = MouseMode.NormalMode;
            Invalidate();
        }

        #endregion

        #region Helper Methods

        private int CalculateWidth()
        {
            return TextRenderer.MeasureText(Text, Font).Width + 30; // 30 для области радиокнопки и отступов
        }

        #endregion
    }
    public class AcaciaLabel : Control
    {
        #region Variables

        private bool autoSize = true; // Поддержка AutoSize

        #endregion

        #region Properties

        public override bool AutoSize
        {
            get => autoSize;
            set
            {
                autoSize = value;
                if (autoSize)
                {
                    Size = new Size(CalculateWidth(), Height);
                }
            }
        }

        #endregion

        #region Constructors

        public AcaciaLabel()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            Font = new Font("Arial", 10, FontStyle.Bold);
            UpdateStyles();
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            e.Graphics.DrawString(Text, Font, new SolidBrush(Color.FromArgb(228, 236, 242)), ClientRectangle);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (AutoSize)
            {
                Height = Font.Height; // Высота метки равна высоте шрифта
                Width = CalculateWidth(); // Вычисляем ширину при изменении размера
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
            if (AutoSize)
            {
                Size = new Size(CalculateWidth(), Height); // Автоматически изменяем размер при изменении текста
            }
        }

        #region Helper Methods

        private int CalculateWidth()
        {
            return TextRenderer.MeasureText(Text, Font).Width + 5; // Добавление небольшого отступа
        }

        #endregion
    }

    public class AcaciaSeperator : Control
    {
        #region Variables

        private Style _sepStyle = Style.Horizental;
        private bool autoSize = true; // Поддержка AutoSize

        #endregion

        #region Enumerators

        public enum Style
        {
            Horizental,
            Vertical
        }

        #endregion

        #region Constructors

        public AcaciaSeperator()
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor |
                     ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            ForeColor = Color.FromArgb(150, GetHTMLColor("fc3955"));
            UpdateStyles();
        }

        #endregion

        #region Draw Control

        protected override void OnPaint(PaintEventArgs e)
        {
            using var bitmap = new Bitmap(Width, Height);
            using var graphics = Graphics.FromImage(bitmap);

            graphics.SmoothingMode = SmoothingMode.HighQuality;
            var colorBlend = new ColorBlend
            {
                Positions = new float[] { 0.0F, 0.15F, 0.85F, 1.0F },
                Colors = new Color[] { Color.Transparent, ForeColor, ForeColor, Color.Transparent }
            };

            switch (SepStyle)
            {
                case Style.Horizental:
                    using (var brush = new LinearGradientBrush(ClientRectangle, Color.Empty, Color.Empty, 0.0F))
                    {
                        brush.InterpolationColors = colorBlend;
                        graphics.DrawLine(new Pen(brush), 0, 1, Width, 1);
                    }
                    break;

                case Style.Vertical:
                    using (var brush = new LinearGradientBrush(ClientRectangle, Color.Empty, Color.Empty, 0.0F))
                    {
                        brush.InterpolationColors = colorBlend;
                        graphics.DrawLine(new Pen(brush), 1, 0, 1, Height);
                    }
                    break;
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        #endregion

        protected override void OnResize(EventArgs e)
        {
            if (SepStyle == Style.Horizental)
            {
                Height = 4;
            }
            else
            {
                Width = 4;
            }
        }

        #region Properties

        public Style SepStyle
        {
            get => _sepStyle;
            set
            {
                _sepStyle = value;
                if (value == Style.Horizental)
                {
                    Height = 4;
                }
                else
                {
                    Width = 4;
                }
            }
        }

        public override bool AutoSize // Override для свойства AutoSize
        {
            get => autoSize;
            set
            {
                autoSize = value;
                if (autoSize)
                {
                    if (SepStyle == Style.Horizental)
                    {
                        Size = new Size(Width, 4); // Высота для горизонтального разделителя
                    }
                    else
                    {
                        Size = new Size(4, Height); // Ширина для вертикального разделителя
                    }
                }
            }
        }

        #endregion

        #region Helper Methods

        private static Color GetHTMLColor(string hex)
        {
            return ColorTranslator.FromHtml($"#{hex}");
        }

        #endregion
    }
    public class AcaciaComboBox : ComboBox
    {
        #region Variables

        private int _startIndex = 0;
        private bool autoSize = true; // Поддержка AutoSize
        private bool isDropDown = false; // Состояние раскрытия
        private Color dropDownBackColor; // Цвет фона для DropDownList

        #endregion

        #region Constructors

        public AcaciaComboBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
            dropDownBackColor = GetHTMLColor("585c73"); // Устанавливаем цвет фона по умолчанию
            Font = new Font("Arial", 12);
            DrawMode = DrawMode.OwnerDrawFixed;
            DoubleBuffered = true;
            StartIndex = 0;
            DropDownStyle = ComboBoxStyle.DropDown; // Установлено в DropDown
            UpdateStyles();
        }

        #endregion

        #region Properties

        private int StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value;
                try
                {
                    base.SelectedIndex = value;
                }
                catch { /* Игнорировать ошибки */ }
                Invalidate();
            }
        }

        public override bool AutoSize // Override для свойства AutoSize
        {
            get => autoSize;
            set
            {
                autoSize = value;
                if (autoSize)
                {
                    Size = new Size(Width, Font.Height + 10); // Автоматически определяем высоту по высоте шрифта
                }
            }
        }

        // Свойство для изменения цвета фона для DropDownList
        public Color DropDownBackColor
        {
            get => dropDownBackColor;
            set
            {
                dropDownBackColor = value;
                Invalidate();
            }
        }

        #endregion

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            try
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                {
                    g.FillRectangle(new SolidBrush(Color.FromArgb(120, GetHTMLColor("fc3955"))), e.Bounds);
                    g.DrawString(GetItemText(Items[e.Index]), Font, new SolidBrush(GetHTMLColor("585c73")), 1, e.Bounds.Y + 4);
                }
                else
                {
                    g.FillRectangle(new SolidBrush(dropDownBackColor), e.Bounds); // Используем цвет фона для DropDownList
                    g.DrawString(GetItemText(Items[e.Index]), Font, new SolidBrush(GetHTMLColor("585c73")), 1, e.Bounds.Y + 4);
                }
            }
            catch { /* Игнорировать ошибки */ }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using var bitmap = new Bitmap(Width, Height);
            using var g = Graphics.FromImage(bitmap);
            var rect = new Rectangle(1, 1, (int)(Width - 2.5), (int)(Height - 2.5));

            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            DrawRoundedPath(g, GetHTMLColor("585c73"), 1.7f, rect, 1);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Рисуем стрелку: вниз в обычном состоянии
            if (!isDropDown)
            {
                DrawTriangle(g, GetHTMLColor("fc3955"), 1.0f,
                    new Point(Width - 20, 12),
                    new Point(Width - 16, 16),
                    new Point(Width - 12, 12));
            }
            else
            {
                // Когда выпадающий список открыт, стрелка направлена вверх
                DrawTriangle(g, GetHTMLColor("fc3955"), 1.0f,
                    new Point(Width - 20, 12),
                    new Point(Width - 16, 8),
                    new Point(Width - 12, 12));
            }

            g.SmoothingMode = SmoothingMode.None;
            g.DrawString(Text, Font, new SolidBrush(GetHTMLColor("585c73")),
                new Rectangle(7, 1, Width - 1, Height - 1),
                new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });

            e.Graphics.DrawImage(bitmap, 0, 0);
        }

        #region Events

        protected override void OnDropDown(EventArgs e)
        {
            isDropDown = true; // Меняем состояние на раскрытое
            base.OnDropDown(e);
            Invalidate();
        }

        protected override void OnDropDownClosed(EventArgs e)
        {
            isDropDown = false; // Меняем состояние на закрытое
            base.OnDropDownClosed(e);
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        #endregion

        #region Helper Methods

        private static Color GetHTMLColor(string hex)
        {
            return ColorTranslator.FromHtml("#" + hex);
        }

        private void DrawRoundedPath(Graphics g, Color color, float radius, Rectangle rect, int thickness)
        {
            using var path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();

            using var pen = new Pen(color, thickness);
            g.DrawPath(pen, path);
        }

        private void DrawTriangle(Graphics g, Color color, float thickness, params Point[] points)
        {
            using var pen = new Pen(color, thickness);
            g.DrawPolygon(pen, points);
            g.FillPolygon(new SolidBrush(color), points);
        }

        #endregion
    }

    public class AcaciaProgressBar : Control
    {
        #region Variables

        private int _maximum = 100;
        private int _value = 0;

        #endregion

        #region Constructors

        public AcaciaProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                      ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            UpdateStyles();
        }

        #endregion

        #region Properties

        public int Value
        {
            get => _value < 0 ? 0 : _value;
            set
            {
                if (value > Maximum) value = Maximum;
                _value = value;
                Invalidate();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                if (value < _value) _value = value;
                _maximum = value;
                Invalidate();
            }
        }

        #endregion

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Bitmap B = new(Width, Height))
            using (Graphics G = Graphics.FromImage(B))
            {
                G.SmoothingMode = SmoothingMode.HighQuality;
                int currentValue = (int)((double)Value / Maximum * Width);
                Rectangle rect = new(0, 0, Width - 1, Height - 1);

                FillRoundedPath(G, GetHTMLColor("1e2137"), rect, 1);

                if (currentValue > 0)
                {
                    FillRoundedPath(G, GetHTMLColor("fc3955"),
                        new Rectangle(rect.X, rect.Y, currentValue, rect.Height), 1);
                }

                e.Graphics.DrawImage(B, 0, 0);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = 20;
        }

        private void FillRoundedPath(Graphics g, Color color, Rectangle rect, int radius)
        {
            using (GraphicsPath path = new())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                using (SolidBrush brush = new(color))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private Color GetHTMLColor(string hex)
        {
            return ColorTranslator.FromHtml("#" + hex);
        }
    }

    public class AcaciaTrackBar : Control
    {
        #region Variables

        private bool variable;
        private Rectangle track, trackSide;
        private int _maximum = 100;
        private int _minimum;
        private int _value;
        private int currentValue;

        #endregion

        #region Events

        public event EventHandler Scroll;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.Down || e.KeyCode == Keys.Left)
            {
                if (Value > 0) Value--;
            }
            else if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Up || e.KeyCode == Keys.Right)
            {
                if (Value < Maximum) Value++;
            }
            base.OnKeyDown(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Height > 0)
            {
                RenewCurrentValue();
                try
                {
                    track = new Rectangle(currentValue + 1, 0, 25, 24);
                }
                catch { }

                variable = new Rectangle(currentValue, 0, 24, Height).Contains(e.Location);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (variable && e.X > -1 && e.X < Width + 1)
            {
                Value = Minimum + (int)((Maximum - Minimum) * (e.X / (double)Width));
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            variable = false;
            base.OnMouseUp(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                RenewCurrentValue();
                MoveTrack();
            }
            Invalidate();
            base.OnResize(e);
        }

        private void RenewCurrentValue()
        {
            currentValue = (int)((Value - Minimum) / (double)(Maximum - Minimum) * (Width - 23.5));
        }

        private void MoveTrack()
        {
            if (Height > 0 && Width > 0)
                track = new Rectangle(currentValue + 1, 0, 21, 20);
            trackSide = new Rectangle(currentValue + 8, 7, 6, 6);
        }

        #endregion

        #region Properties

        public int Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                RenewCurrentValue();
                MoveTrack();
                Invalidate();
            }
        }

        public int Minimum
        {
            get => _minimum;
            set
            {
                if (value >= 0)
                {
                    _minimum = value;
                    RenewCurrentValue();
                    MoveTrack();
                    Invalidate();
                }
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                if (value != _value)
                {
                    _value = value;
                    RenewCurrentValue();
                    MoveTrack();
                    Invalidate();
                    Scroll?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        #endregion

        #region Constructors

        public AcaciaTrackBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
            DoubleBuffered = true;
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
            UpdateStyles();
        }

        #endregion

        #region Draw Control

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Bitmap B = new(Width, Height))
            using (Graphics G = Graphics.FromImage(B))
            {
                G.SmoothingMode = SmoothingMode.HighQuality;
                G.PixelOffsetMode = PixelOffsetMode.HighQuality;

                FillRoundedPath(G, GetHTMLColor("1e2137"), new Rectangle(0, 5, Width, 8), 8);

                if (currentValue > 0)
                {
                    FillRoundedPath(G, GetHTMLColor("fc3955"),
                        new Rectangle(0, 5, currentValue + 4, 8), 6);
                }

                G.PixelOffsetMode = PixelOffsetMode.Half;
                G.FillEllipse(new SolidBrush(GetHTMLColor("fc3955")), track);
                G.FillEllipse(new SolidBrush(GetHTMLColor("1e2137")), trackSide);

                e.Graphics.DrawImage(B, 0, 0);
            }
        }

        private void FillRoundedPath(Graphics g, Color color, Rectangle rect, int radius)
        {
            using (GraphicsPath path = new())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                using (SolidBrush brush = new(color))
                {
                    g.FillPath(brush, path);
                }
            }
        }

        private Color GetHTMLColor(string hex)
        {
            return ColorTranslator.FromHtml("#" + hex);
        }

        #endregion
    }

    public class AcaciaPanel : ContainerControl
    {
        #region Constructors

        public AcaciaPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                      ControlStyles.UserPaint |
                      ControlStyles.OptimizedDoubleBuffer |
                      ControlStyles.SupportsTransparentBackColor, true);
            DoubleBuffered = true;
            BackColor = Color.Transparent;
            UpdateStyles();
        }

        #endregion

        #region Draw Control

        protected override void OnPaint(PaintEventArgs e)
        {
            using (Bitmap B = new(Width, Height))
            using (Graphics G = Graphics.FromImage(B))
            {
                Rectangle rect = new(0, 0, Width - 1, Height - 1);

                G.FillRectangle(new SolidBrush(GetHTMLColor("24273e")), rect);
                G.DrawRectangle(new Pen(GetHTMLColor("1d1f38"), 1), rect);

                e.Graphics.DrawImage(B, 0, 0);
            }
        }

        private Color GetHTMLColor(string hex)
        {
            return ColorTranslator.FromHtml("#" + hex);
        }

        #endregion
    }

    public enum MouseMode
    {
        NormalMode,
        Hovered,
        Pushed
    }
}