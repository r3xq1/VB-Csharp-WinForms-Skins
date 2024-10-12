using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static DesignFunctions;

public static class DesignFunctions
{
    public static Brush ToBrush(int A, int R, int G, int B) => ToBrush(Color.FromArgb(A, R, G, B));

    public static Brush ToBrush(int R, int G, int B) => ToBrush(Color.FromArgb(R, G, B));

    public static Brush ToBrush(int A, Color C) => ToBrush(Color.FromArgb(A, C));

    public static Brush ToBrush(Pen pen) => ToBrush(pen.Color);

    public static Brush ToBrush(Color color) => new SolidBrush(color);

    public static Pen ToPen(int A, int R, int G, int B) => ToPen(Color.FromArgb(A, R, G, B));

    public static Pen ToPen(int R, int G, int B) => ToPen(Color.FromArgb(R, G, B));

    public static Pen ToPen(int A, Color C) => ToPen(Color.FromArgb(A, C));

    public static Pen ToPen(Color color) => ToPen(new SolidBrush(color));

    public static Pen ToPen(SolidBrush brush) => new Pen(brush.Color);

    public class CornerStyle
    {
        public bool TopLeft { get; set; }
        public bool TopRight { get; set; }
        public bool BottomLeft { get; set; }
        public bool BottomRight { get; set; }
    }

    public static GraphicsPath AdvRect(Rectangle rectangle, CornerStyle cornerStyle, int curve)
    {
        var path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;

        if (cornerStyle.TopLeft)
            path.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        else
            path.AddLine(rectangle.X, rectangle.Y, rectangle.X + arcRectangleWidth, rectangle.Y);

        if (cornerStyle.TopRight)
            path.AddArc(new Rectangle(rectangle.Right - arcRectangleWidth, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        else
            path.AddLine(rectangle.Right, rectangle.Y, rectangle.Right, rectangle.Y + arcRectangleWidth);

        if (cornerStyle.BottomRight)
            path.AddArc(new Rectangle(rectangle.Right - arcRectangleWidth, rectangle.Bottom - arcRectangleWidth, arcRectangleWidth, arcRectangleWidth), 0, 90);
        else
            path.AddLine(rectangle.Right, rectangle.Bottom, rectangle.Right - arcRectangleWidth, rectangle.Bottom);

        if (cornerStyle.BottomLeft)
            path.AddArc(new Rectangle(rectangle.X, rectangle.Bottom - arcRectangleWidth, arcRectangleWidth, arcRectangleWidth), 90, 90);
        else
            path.AddLine(rectangle.X, rectangle.Bottom, rectangle.X, rectangle.Bottom - arcRectangleWidth);

        path.CloseAllFigures();

        return path;
    }

    public static GraphicsPath RoundRect(Rectangle rectangle, int curve)
    {
        var path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;

        path.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        path.AddArc(new Rectangle(rectangle.Right - arcRectangleWidth, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        path.AddArc(new Rectangle(rectangle.Right - arcRectangleWidth, rectangle.Bottom - arcRectangleWidth, arcRectangleWidth, arcRectangleWidth), 0, 90);
        path.AddArc(new Rectangle(rectangle.X, rectangle.Bottom - arcRectangleWidth, arcRectangleWidth, arcRectangleWidth), 90, 90);
        path.AddLine(new Point(rectangle.X, rectangle.Bottom - arcRectangleWidth), new Point(rectangle.X, rectangle.Y + arcRectangleWidth));
        path.CloseAllFigures();

        return path;
    }

    public static GraphicsPath RoundRect(int x, int y, int width, int height, int curve)
    {
        return RoundRect(new Rectangle(x, y, width, height), curve);
    }

    public class PillStyle
    {
        public bool Left { get; set; }
        public bool Right { get; set; }
    }

    public static GraphicsPath Pill(Rectangle rectangle, PillStyle pillStyle)
    {
        var path = new GraphicsPath();

        if (pillStyle.Left)
            path.AddArc(new Rectangle(rectangle.X, rectangle.Y, rectangle.Height, rectangle.Height), -270, 180);
        else
            path.AddLine(rectangle.X, rectangle.Y + rectangle.Height, rectangle.X, rectangle.Y);

        if (pillStyle.Right)
            path.AddArc(new Rectangle(rectangle.Right - rectangle.Height, rectangle.Y, rectangle.Height, rectangle.Height), -90, 180);
        else
            path.AddLine(rectangle.Right, rectangle.Y, rectangle.Right, rectangle.Y + rectangle.Height);

        path.CloseAllFigures();

        return path;
    }

    public static GraphicsPath Pill(int x, int y, int width, int height, PillStyle pillStyle)
    {
        return Pill(new Rectangle(x, y, width, height), pillStyle);
    }
}

public class AresioButton : Control
{
    private enum MouseState
    {
        None,
        Over,
        Down
    }

    private MouseState state = MouseState.None;

    public AresioButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        g.SmoothingMode = SmoothingMode.HighQuality;

        // Background
        using (var brush = new LinearGradientBrush(new Point(0, 0), new Point(0, Height),
                                                   Color.FromArgb(250, 200, 70),
                                                   Color.FromArgb(250, 160, 40)))
        {
            g.FillPath(brush, RoundRect(0, 0, Width - 1, Height - 1, 4));
        }

        g.DrawPath(ToPen(50, Color.White), RoundRect(0, 1, Width - 1, Height - 2, 4));
        g.DrawPath(ToPen(150, Color.FromArgb(100, 70, 70)), RoundRect(0, 0, Width - 1, Height - 1, 4));

        if (Enabled)
        {
            switch (state)
            {
                case MouseState.Over:
                    using (var brush = new LinearGradientBrush(new Point(0, 0), new Point(0, Height),
                                                               Color.FromArgb(50, Color.White),
                                                               Color.Transparent))
                    {
                        g.FillPath(brush, RoundRect(0, 0, Width - 1, Height - 1, 4));
                    }
                    break;

                case MouseState.Down:
                    using (var brush = new LinearGradientBrush(new Point(0, 0), new Point(0, Height),
                                                               Color.FromArgb(50, Color.Black),
                                                               Color.Transparent))
                    {
                        g.FillPath(brush, RoundRect(0, 0, Width - 1, Height - 1, 4));
                    }
                    break;
            }

            DrawText(g, Color.White, 0);
            DrawText(g, Color.Black, 1);
        }
        else
        {
            DrawText(g, Color.Gray, 0);
        }
    }

    private void DrawText(Graphics g, Color color, int offset)
    {
        using (var font = new Font(Font.FontFamily, Font.Size, FontStyle.Regular))
        {
            var textSize = g.MeasureString(Text, font);
            var textX = (Width / 2) - (textSize.Width / 2);
            var textY = (Height / 2) - (textSize.Height / 2);

            // Draw text with given offsets
            g.DrawString(Text, font, new SolidBrush(Color.FromArgb(offset * 100, color)),
                         new PointF(textX + offset, textY + offset));
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        state = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        state = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        state = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        state = MouseState.Over;
        Invalidate();
    }

    private GraphicsPath RoundRect(int x, int y, int width, int height, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(x, y, radius, radius, 180, 90);
        path.AddArc(x + width - radius, y, radius, radius, 270, 90);
        path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
        path.AddArc(x, y + height - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }

    private Pen ToPen(int alpha, Color color)
    {
        return new Pen(Color.FromArgb(alpha, color));
    }
}
public class AresioTrackBar : Control
{
    #region Properties
    private int _maximum = 10;
    public int Maximum
    {
        get { return _maximum; }
        set
        {
            if (value > 0)
                _maximum = value;
            if (value < _value)
                _value = value;
            Invalidate();
        }
    }

    public event EventHandler ValueChanged;
    private int _value = 0;
    public int Value
    {
        get { return _value; }
        set
        {
            if (value == _value) return;
            if (value < 0) _value = 0;
            else if (value > _maximum) _value = _maximum;
            else _value = value;

            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    #endregion

    public AresioTrackBar()
    {
        SetStyle(ControlStyles.DoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.Selectable |
                 ControlStyles.SupportsTransparentBackColor, true);
    }

    private bool captureM = false;
    private Rectangle bar = new Rectangle(0, 10, 0, 0);
    private Size trackSize = new Size(20, 20);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;

        bar = new Rectangle(10, 10, Width - 21, Height - 21);
        g.Clear(Parent.FindForm().BackColor);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Background
        using (var backLinear = new LinearGradientBrush(new Point(0, (Height / 2) - 4),
                                                        new Point(0, (Height / 2) + 4),
                                                        Color.FromArgb(50, Color.Black),
                                                        Color.Transparent))
        {
            g.FillPath(backLinear, RoundRect(0, (Height / 2) - 4, Width - 1, 8, 3));
            g.DrawPath(ToPen(50, Color.Black), RoundRect(0, (Height / 2) - 4, Width - 1, 8, 3));
        }

        // Fill
        g.FillPath(new LinearGradientBrush(new Point(1, (Height / 2) - 4),
                                            new Point(1, (Height / 2) + 4),
                                            Color.FromArgb(250, 200, 70),
                                            Color.FromArgb(250, 160, 40)),
                    RoundRect(1, (Height / 2) - 4, (int)(bar.Width * (Value / (float)Maximum)) + (trackSize.Width / 2), 8, 3));

        g.DrawPath(ToPen(100, Color.White),
                   RoundRect(2, (Height / 2) - 2,
                              (int)(bar.Width * (Value / (float)Maximum)) + (trackSize.Width / 2), 4, 3));

        g.SetClip(RoundRect(1, (Height / 2) - 4,
            (int)(bar.Width * (Value / (float)Maximum)) + (trackSize.Width / 2), 8, 3));

        for (int i = 0; i <= (int)(bar.Width * (Value / (float)Maximum)) + (trackSize.Width / 2); i += 10)
        {
            g.DrawLine(new Pen(Color.FromArgb(20, Color.Black), 4),
                        new Point(i, (Height / 2) - 10),
                        new Point(i - 10, (Height / 2) + 10));
        }
        g.SetClip(new Rectangle(0, 0, Width, Height));

        // Button
        g.FillEllipse(Brushes.White,
                      bar.X + (int)(bar.Width * (Value / (float)Maximum)) - (trackSize.Width / 2),
                      bar.Y + (bar.Height / 2) - (trackSize.Height / 2),
                      trackSize.Width,
                      trackSize.Height);

        g.DrawEllipse(ToPen(50, Color.Black),
                      bar.X + (int)(bar.Width * (Value / (float)Maximum)) - (trackSize.Width / 2),
                      bar.Y + (bar.Height / 2) - (trackSize.Height / 2),
                      trackSize.Width,
                      trackSize.Height);

        g.FillEllipse(new LinearGradientBrush(new Point(0, bar.Y + (bar.Height / 2) - (trackSize.Height / 2)),
                                              new Point(0, bar.Y + (bar.Height / 2) - (trackSize.Height / 2) + trackSize.Height),
                                              Color.FromArgb(200, Color.Black),
                                              Color.FromArgb(100, Color.Black)),
                      new Rectangle(bar.X + (int)(bar.Width * (Value / (float)Maximum)) - (trackSize.Width / 2) + 6,
                                    bar.Y + (bar.Height / 2) - (trackSize.Height / 2) + 6,
                                    trackSize.Width - 12,
                                    trackSize.Height - 12));
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        this.BackColor = Color.Transparent;
        base.OnHandleCreated(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Rectangle mp = new Rectangle(new Point(e.Location.X, e.Location.Y), new Size(1, 1));
        if (new Rectangle(bar.X + (int)(bar.Width * (Value / (float)Maximum)) - (trackSize.Width / 2), 0, trackSize.Width, Height).IntersectsWith(mp))
        {
            captureM = true;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        captureM = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (captureM)
        {
            Point mp = new Point(e.X, e.Y);
            Value = (int)(Maximum * ((mp.X - bar.X) / (float)bar.Width));
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        captureM = false;
    }

    private GraphicsPath RoundRect(int x, int y, int width, int height, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(x, y, radius, radius, 180, 90);
        path.AddArc(x + width - radius, y, radius, radius, 270, 90);
        path.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90);
        path.AddArc(x, y + height - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }

    private Pen ToPen(int alpha, Color color)
    {
        return new Pen(Color.FromArgb(alpha, color));
    }
}
public class AresioSwitch : Control
{
    private int ToggleLocation = 0;
    private Timer ToggleAnimation = new Timer { Interval = 1 };
    public event EventHandler ToggledChanged;

    private bool _toggled;
    public bool Toggled
    {
        get { return _toggled; }
        set
        {
            _toggled = value;
            Invalidate();
            ToggledChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public AresioSwitch()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer, true);
        ToggleAnimation.Tick += Animation; // Привязка метода к событию Tick
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        ToggleAnimation.Start();
    }

    private void Animation(object sender, EventArgs e)
    {
        if (_toggled)
        {
            if (ToggleLocation < 100)
                ToggleLocation += 10;
        }
        else
        {
            if (ToggleLocation > 0)
                ToggleLocation -= 10;
        }

        Invalidate();
    }

    private Rectangle Bar;
    private Size Track = new Size(20, 20);

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics G = e.Graphics;
        Bar = new Rectangle(10, 10, Width - 21, Height - 21);
        G.Clear(Parent.FindForm().BackColor);
        G.SmoothingMode = SmoothingMode.AntiAlias;

        // Background
        using (LinearGradientBrush BackLinear =
            new LinearGradientBrush(new Point(0, (Height / 2) - (Track.Height / 2)),
            new Point(0, (Height / 2) + (Track.Height / 2)),
            Color.FromArgb(50, Color.Black), Color.Transparent))
        {
            G.FillPath(BackLinear, Pill(0, (Height / 2 - Track.Height / 2), Width - 1, Track.Height - 2,
                new PillStyle { Left = true, Right = true }));
            G.DrawPath(ToPen(50, Color.Black), Pill(0, (Height / 2 - Track.Height / 2), Width - 1,
                Track.Height - 2, new PillStyle { Left = true, Right = true }));
        }

        // Fill
        if (ToggleLocation > 0)
        {
            G.FillPath(new LinearGradientBrush(
                new Point(0, (Height / 2) - Track.Height / 2),
                new Point(1, (Height / 2) + Track.Height / 2),
                Color.FromArgb(250, 200, 70),
                Color.FromArgb(250, 160, 40)),
                Pill(1, (Height / 2 - Track.Height / 2),
                (int)(Bar.Width * (ToggleLocation / 100.0)) + (Track.Width / 2),
                Track.Height - 3,
                new PillStyle { Left = true, Right = true }));

            G.DrawPath(ToPen(100, Color.White),
                Pill(1, (Height / 2 - Track.Height / 2 + 1),
                (int)(Bar.Width * (ToggleLocation / 100.0)) + (Track.Width / 2),
                Track.Height - 5,
                new PillStyle { Left = true, Right = true }));
        }

        G.DrawString(Toggled ? "ON" : "OFF",
            new Font("Arial", 6, FontStyle.Bold),
            ToBrush(150, Color.Black),
            new Rectangle(0, -1, Width - Track.Width + Track.Width / 3, Height),
            new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

        // Button
        G.FillEllipse(Brushes.White,
            Bar.X + (int)(Bar.Width * (ToggleLocation / 100.0)) - (Track.Width / 2),
            Bar.Y + (Bar.Height / 2) - (Track.Height / 2),
            Track.Width, Track.Height);
        G.DrawEllipse(ToPen(50, Color.Black),
            Bar.X + (int)(Bar.Width * (ToggleLocation / 100.0)) - (Track.Width / 2),
            Bar.Y + (Bar.Height / 2) - (Track.Height / 2),
            Track.Width, Track.Height);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Toggled = !Toggled; // Переключение состояния
    }
}
public class AresioTabControl : TabControl
{
    public AresioTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer, true);
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        SizeMode = TabSizeMode.Normal;
        ItemSize = new Size(77, 31);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics G = e.Graphics;
        Rectangle ItemBounds;
        SolidBrush TextBrush = new SolidBrush(Color.Empty);
        int SOFF;

        G.Clear(Color.FromArgb(236, 237, 239));

        for (int TabItemIndex = 0; TabItemIndex < TabCount; TabItemIndex++)
        {
            ItemBounds = GetTabRect(TabItemIndex);

            if (TabItemIndex != SelectedIndex)
            {
                // Рисуем невыбранную вкладку
                SOFF = 2;
                G.FillPath(ToBrush(236, 237, 239), RoundRect(new Rectangle(ItemBounds.X, ItemBounds.Y + SOFF, ItemBounds.Width, ItemBounds.Height), 2));
                G.DrawPath(ToPen(150, 151, 153), RoundRect(new Rectangle(ItemBounds.X, ItemBounds.Y + SOFF, ItemBounds.Width, ItemBounds.Height), 2));

                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                TextBrush.Color = Color.FromArgb(80, 80, 80);
                G.DrawString(TabPages[TabItemIndex].Text, new Font(Font.Name, Font.Size - 1), TextBrush, new Rectangle(ItemBounds.Location, ItemBounds.Size), sf);
            }
            else
            {
                // Рисуем выбранную вкладку
                SOFF = 0;
                G.FillPath(ToBrush(236, 237, 239), RoundRect(new Rectangle(ItemBounds.X, ItemBounds.Y, ItemBounds.Width, ItemBounds.Height), 2));
                G.DrawPath(ToPen(150, 151, 153), RoundRect(new Rectangle(ItemBounds.X, ItemBounds.Y, ItemBounds.Width, ItemBounds.Height), 2));

                StringFormat sf = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Center
                };
                TextBrush.Color = Color.Black;
                G.DrawString(TabPages[TabItemIndex].Text, Font, TextBrush, new Rectangle(ItemBounds.Location, ItemBounds.Size), sf);
            }
        }
    }

    private GraphicsPath RoundRect(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }

    private Brush ToBrush(int r, int g, int b)
    {
        return new SolidBrush(Color.FromArgb(r, g, b));
    }

    private Pen ToPen(int r, int g, int b)
    {
        return new Pen(Color.FromArgb(r, g, b));
    }
}
public class AresioProgressBar : Control
{
    #region Properties

    private int _minimum;
    public int Minimum
    {
        get { return _minimum; }
        set
        {
            _minimum = value;

            if (value > _maximum) _maximum = value;
            if (value > _value) _value = value;

            Invalidate();
        }
    }

    private int _maximum;
    public int Maximum
    {
        get { return _maximum; }
        set
        {
            _maximum = value;

            if (value < _minimum) _minimum = value;
            if (value < _value) _value = value;

            Invalidate();
        }
    }

    public event EventHandler ValueChanged;
    private int _value;
    public int Value
    {
        get { return _value; }
        set
        {
            if (value < _minimum)
            {
                _value = _minimum;
            }
            else if (value > _maximum)
            {
                _value = _maximum;
            }
            else
            {
                _value = value;
            }

            Invalidate();
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    #endregion

    public AresioProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer, true);
        _maximum = 100;
        _minimum = 0;
        _value = 0;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics G = e.Graphics;
        G.Clear(Parent.FindForm().BackColor);
        G.SmoothingMode = SmoothingMode.AntiAlias;

        // Background
        using (LinearGradientBrush BackLinear = new LinearGradientBrush(
            new Point(0, (Height / 2) - 4),
            new Point(0, (Height / 2) + 4),
            Color.FromArgb(50, Color.Black),
            Color.Transparent))
        {
            G.FillPath(BackLinear, RoundRect(0, (Height / 2) - 4, Width - 1, 8, 3));
            using (Pen pen = ToPen(50, Color.Black))
            {
                G.DrawPath(pen, RoundRect(0, (Height / 2) - 4, Width - 1, 8, 3));
            }
        }


        // Fill
        if (_value > 0)
        {
            using (LinearGradientBrush fillBrush = new LinearGradientBrush(
               new Point(1, (Height / 2) - 4),
               new Point(1, (Height / 2) + 4),
               Color.FromArgb(250, 200, 70),
               Color.FromArgb(250, 160, 40)))
            {
                G.FillPath(fillBrush, RoundRect(1, (Height / 2) - 4, (int)((Width - 2) * ((double)Value / Maximum)), 8, 3));
            }

            using (Pen pen = ToPen(100, Color.White))
            {
                G.DrawPath(pen, RoundRect(2, (Height / 2) - 2, (int)((Width - 4) * ((double)Value / Maximum)), 4, 3));
            }
        }
    }

    // Вспомогательный метод для создания закругленного прямоугольника
    private GraphicsPath RoundRect(int x, int y, int width, int height, int radius)
    {
        GraphicsPath gp = new GraphicsPath();
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);
        gp.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
        gp.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
        gp.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
        gp.CloseFigure();
        return gp;
    }

    // Вспомогательный метод для создания Pen с заданной прозрачностью
    private Pen ToPen(int alpha, Color baseColor)
    {
        return new Pen(Color.FromArgb(alpha, baseColor));
    }
}
public class AresioRadioButton : Control
{
    public event EventHandler CheckedChanged;
    private bool _checked;
    public bool Checked
    {
        get { return _checked; }
        set
        {
            _checked = value;

            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public AresioRadioButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics G = e.Graphics;

        G.SmoothingMode = SmoothingMode.AntiAlias;

        using (LinearGradientBrush MLG = new LinearGradientBrush(
            new Point(Height / 2, 0),
            new Point(Height / 2, Height),
            Color.FromArgb(50, Color.Black),
            Color.Transparent))
        {
            G.FillEllipse(MLG, new Rectangle(0, 0, Height - 1, Height - 1));
            using (Pen pen = ToPen(50, Color.Black))
            {
                G.DrawEllipse(pen, new Rectangle(0, 0, Height - 1, Height - 1));
            }
        }

        using (StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near })
        {
            G.DrawString(Text, Font, Brushes.Black, new Rectangle(Height + 5, 0, Width - Height + 4, Height), sf);
        }


        if (_checked)
        {
            using (LinearGradientBrush MLG2 = new LinearGradientBrush(
                new Point(Height / 2, 3),
                new Point(Height / 2, Height - 6),
                Color.FromArgb(200, Color.White),
                Color.FromArgb(10, Color.White)))
            {
                G.FillEllipse(MLG2, new Rectangle(3, 3, Height - 7, Height - 7));
                using (Pen pen = ToPen(50, Color.Black))
                {
                    G.DrawEllipse(pen, new Rectangle(3, 3, Height - 7, Height - 7));
                }
            }
        }
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        if (!Checked) Checked = true;

        foreach (Control ctl in Parent.Controls)
        {
            if (ctl is AresioRadioButton)
            {
                if (ctl.Handle == this.Handle) continue;
                if (ctl.Enabled) ((AresioRadioButton)ctl).Checked = false;
            }
        }
    }

    // Вспомогательный метод для создания Pen с заданной прозрачностью
    private Pen ToPen(int alpha, Color baseColor)
    {
        return new Pen(Color.FromArgb(alpha, baseColor));
    }
}