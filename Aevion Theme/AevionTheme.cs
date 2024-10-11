using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class Helpers
{
    public static Font AevionFont = new Font("Segoe UI", 9);
    public static Font AevionFontBold = new Font("Segoe UI", 9, FontStyle.Bold);
    public static Color AevionBack = Color.FromArgb(48, 57, 65);

    public enum MouseState : byte
    {
        None,
        Hover,
        Down
    }

    public enum Direction
    {
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4
    }

    public static void RoundRect(Graphics g, int x, int y, int width, int height, int curve, Color draw)
    {
        var baseRect = new RectangleF(x, y, width, height);
        var arcRect = new RectangleF(baseRect.Location, new SizeF(curve, curve));

        g.DrawArc(new Pen(draw), arcRect, 180, 90);
        g.DrawLine(new Pen(draw), x + curve / 2, y, x + width - curve / 2, y);

        arcRect.X = baseRect.Right - curve;
        g.DrawArc(new Pen(draw), arcRect, 270, 90);
        g.DrawLine(new Pen(draw), x + width, y + curve / 2, x + width, y + height - curve / 2);

        arcRect.Y = baseRect.Bottom - curve;
        g.DrawArc(new Pen(draw), arcRect, 0, 90);
        g.DrawLine(new Pen(draw), x + curve / 2, y + height, x + width - curve / 2, y + height);

        arcRect.X = baseRect.Left;
        g.DrawArc(new Pen(draw), arcRect, 90, 90);
        g.DrawLine(new Pen(draw), x, y + curve / 2, x, y + height - curve / 2);
    }

    public static void CenterString(Graphics g, string text, Font font, Brush brush, Rectangle rect, bool shadow = false, int yOffset = 0)
    {
        SizeF textSize = g.MeasureString(text, font);
        int textX = rect.X + (rect.Width / 2) - (int)(textSize.Width / 2);
        int textY = rect.Y + (rect.Height / 2) - (int)(textSize.Height / 2) + yOffset;

        if (shadow)
            g.DrawString(text, font, Brushes.Black, textX + 1, textY + 1);

        g.DrawString(text, font, brush, textX, textY + 1);
    }

    public static void DrawTriangle(Graphics g, Rectangle rect, Direction direction, Color draw)
    {
        int halfWidth = rect.Width / 2;
        int halfHeight = rect.Height / 2;
        Point p0 = Point.Empty;
        Point p1 = Point.Empty;
        Point p2 = Point.Empty;

        switch (direction)
        {
            case Direction.Up:
                p0 = new Point(rect.Left + halfWidth, rect.Top);
                p1 = new Point(rect.Left, rect.Bottom);
                p2 = new Point(rect.Right, rect.Bottom);
                break;
            case Direction.Down:
                p0 = new Point(rect.Left + halfWidth, rect.Bottom);
                p1 = new Point(rect.Left, rect.Top);
                p2 = new Point(rect.Right, rect.Top);
                break;
            case Direction.Left:
                p0 = new Point(rect.Left, rect.Top + halfHeight);
                p1 = new Point(rect.Right, rect.Top);
                p2 = new Point(rect.Right, rect.Bottom);
                break;
            case Direction.Right:
                p0 = new Point(rect.Right, rect.Top + halfHeight);
                p1 = new Point(rect.Left, rect.Bottom);
                p2 = new Point(rect.Left, rect.Top);
                break;
        }

        g.FillPolygon(new SolidBrush(draw), new Point[] { p0, p1, p2 });
    }
}

public class AevionForm : Control
{
    public AevionForm()
    {
        DoubleBuffered = true;
        Font = Helpers.AevionFont;
        ForeColor = Color.White;
        BackColor = Helpers.AevionBack;
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        Dock = DockStyle.Fill;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;

        base.OnPaint(e);

        g.Clear(Helpers.AevionBack);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }
}

public class AevionButton : Button
{
    private Helpers.MouseState state;

    public enum Style
    {
        DefaultStyle = 1,
        GreenStyle = 2,
        RedStyle = 3
    }

    private Style buttonStyle;
    public Style ButtonStyle
    {
        get { return buttonStyle; }
        set
        {
            buttonStyle = value;
            Invalidate();
        }
    }

    private Image imagePath;
    public Image ImagePath
    {
        get { return imagePath; }
        set
        {
            imagePath = value;
            Invalidate();
        }
    }

    private bool showIcon;
    public bool ShowIcon
    {
        get { return showIcon; }
        set
        {
            showIcon = value;
            Invalidate();
        }
    }

    public AevionButton()
    {
        DoubleBuffered = true;
        ButtonStyle = Style.DefaultStyle;
        ShowIcon = false;
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;

        base.OnPaint(e);
        g.Clear(Helpers.AevionBack);

        if (ButtonStyle == Style.RedStyle)
        {
            LinearGradientBrush linear;

            switch (state)
            {
                case Helpers.MouseState.Down:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(173, 83, 74), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
                case Helpers.MouseState.Hover:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(193, 103, 94), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
                default:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(183, 93, 84), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
            }
        }
        else if (ButtonStyle == Style.GreenStyle)
        {
            LinearGradientBrush linear;

            switch (state)
            {
                case Helpers.MouseState.Down:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(127, 177, 80), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
                case Helpers.MouseState.Hover:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(157, 197, 100), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
                default:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(147, 187, 90), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
            }
        }
        else
        {
            LinearGradientBrush linear;

            switch (state)
            {
                case Helpers.MouseState.Down:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(88, 105, 123), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
                case Helpers.MouseState.Hover:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(108, 125, 143), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
                default:
                    linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(98, 115, 133), Color.Black, LinearGradientMode.Vertical);
                    g.FillRectangle(linear, new Rectangle(1, 1, Width - 2, Height - 2));
                    break;
            }
        }

        Helpers.RoundRect(g, 0, 0, Width - 1, Height - 1, 3, Color.FromArgb(38, 38, 38));

        if (ShowIcon)
            g.DrawImage(ImagePath, new Point(Width / 8, Height / 2 - 8));

        Helpers.CenterString(g, Text, Helpers.AevionFontBold, Brushes.White, new Rectangle(0, 0, Width, Height));
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        state = Helpers.MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        state = Helpers.MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        state = Helpers.MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        state = Helpers.MouseState.None;
        Invalidate();
    }
}

public class AevionRadioButton : Control
{
    public event EventHandler CheckedChanged;

    private bool checkedState;
    public bool Checked
    {
        get { return checkedState; }
        set
        {
            if (checkedState != value)
            {
                checkedState = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public AevionRadioButton()
    {
        DoubleBuffered = true;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(Width, 16);
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;

        base.OnPaint(e);
        g.Clear(Helpers.AevionBack);

        using (LinearGradientBrush linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(98, 115, 133), Color.Black, LinearGradientMode.Vertical))
        {
            g.FillEllipse(linear, new Rectangle(1, 1, 14, 14));
            g.DrawEllipse(new Pen(Color.FromArgb(35, 35, 40)), new Rectangle(0, 0, 15, 15));
        }

        if (Checked)
            g.FillEllipse(new SolidBrush(Color.FromArgb(220, 220, 255)), new Rectangle(5, 5, 5, 5));

        g.DrawString(Text, Helpers.AevionFont, Brushes.White, new Point(20, 0));
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Checked = !Checked;
    }
}

public class AevionCheckBox : Control
{
    public event EventHandler CheckedChanged;

    private bool checkedState;
    public bool Checked
    {
        get { return checkedState; }
        set
        {
            if (checkedState != value)
            {
                checkedState = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public AevionCheckBox()
    {
        DoubleBuffered = true;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(Width, 16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;

        base.OnPaint(e);
        g.Clear(Helpers.AevionBack);

        using (LinearGradientBrush linear = new LinearGradientBrush(new Rectangle(0, 0, Width, Height + 35), Color.FromArgb(98, 115, 133), Color.Black, LinearGradientMode.Vertical))
        {
            g.FillRectangle(linear, new Rectangle(1, 1, 13, 13));
            Helpers.RoundRect(g, 0, 0, 14, 14, 3, Color.FromArgb(35, 35, 40));
        }

        if (Checked)
            Helpers.CenterString(g, "a", new Font("Marlett", 10), Brushes.White, new Rectangle(2, 1, 13, 13));

        g.DrawString(Text, Helpers.AevionFont, Brushes.White, new Point(20, -1));
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Checked = !Checked;
    }
}

public class AevionProgressBar : Control
{
    private int maximum;
    public int Maximum
    {
        get { return maximum; }
        set
        {
            maximum = value;
            Invalidate();
        }
    }

    private int minimum;
    public int Minimum
    {
        get { return minimum; }
        set
        {
            minimum = value;
            Invalidate();
        }
    }

    private int value;
    public int Value
    {
        get { return value; }
        set
        {
            this.value = value;
            Invalidate();
        }
    }

    private bool showText;
    public bool ShowText
    {
        get { return showText; }
        set
        {
            showText = value;
            Invalidate();
        }
    }

    public AevionProgressBar()
    {
        DoubleBuffered = true;
        Maximum = 100;
        Minimum = 0;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;

        base.OnPaint(e);
        g.Clear(Helpers.AevionBack);

        using (LinearGradientBrush linear = new LinearGradientBrush(new Point(0, 0), new Point(Width + Value + 50, Height), Color.FromArgb(98, 115, 133), Color.Black))
        {
            g.FillRectangle(linear, new Rectangle(0, 0, (int)((Value - Minimum) / (double)(Maximum - Minimum) * Width), Height));
        }

        Helpers.RoundRect(g, 0, 0, Width - 1, Height - 1, 3, Color.FromArgb(38, 38, 38));

        if (ShowText)
            Helpers.CenterString(g, Text, Helpers.AevionFont, Brushes.White, new Rectangle(0, 0, Width, Height));
    }
}

public class AevionNotice : TextBox
{
    private HorizontalAlignment _textAlign;

    public AevionNotice()
    {
        DoubleBuffered = true;
        Enabled = false;
        Multiline = true;
        BorderStyle = BorderStyle.None;
        TextAlign = HorizontalAlignment.Left; // Значение по умолчанию
    }

    public new HorizontalAlignment TextAlign
    {
        get => _textAlign;
        set
        {
            _textAlign = value;
            Invalidate(); // Перерисовываем компонент при изменении
        }
    }

    public bool ShowIcon { get; set; }
    public Image ImagePath { get; set; }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;

        base.OnPaint(e);
        g.Clear(Helpers.AevionBack);

        // Логика заполнения фона кнопки
        Helpers.RoundRect(g, 0, 0, Width - 1, Height - 1, 3, Color.FromArgb(35, 35, 40));

        if (ShowIcon)
        {
            g.DrawImage(ImagePath, new Point(Width / 8, Height / 2 - 8));
        }

        // Логика отображения текста
        SizeF textSize = g.MeasureString(Text, Helpers.AevionFont);
        int textX = 0;
        int textY = Height / 2 - (int)(textSize.Height / 2);

        // Выравнивание текста
        switch (TextAlign)
        {
            case HorizontalAlignment.Left:
                textX = (ShowIcon ? Width / 8 : 0) + 5;
                break;
            case HorizontalAlignment.Center:
                textX = Width / 2 - (int)(textSize.Width / 2);
                break;
            case HorizontalAlignment.Right:
                textX = Width - (int)(textSize.Width) - 5;
                break;
        }

        // Если Multiline, отрисовываем текст
        if (Multiline)
        {
            g.DrawString(Text, Helpers.AevionFont, Brushes.White, textX, textY);
        }
        else
        {
            // Логика для однострочного текста
            Helpers.CenterString(g, Text, Helpers.AevionFont, Brushes.White, new Rectangle(0, 0, Width, Height));
        }
    }
}

public class AevionLabel : Label
{
    public AevionLabel()
    {
        DoubleBuffered = true;
        Font = Helpers.AevionFont;
        ForeColor = Color.White;
        BackColor = Helpers.AevionBack;
    }
}
