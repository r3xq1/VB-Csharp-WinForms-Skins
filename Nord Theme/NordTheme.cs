using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class HelperMethods
{
    public static GraphicsPath GP;

    public enum MouseMode : byte
    {
        NormalMode,
        Hovered,
        Pushed
    }

    public static void DrawImageFromBase64(Graphics g, string base64Image, Rectangle rect)
    {
        using (var ms = new MemoryStream(Convert.FromBase64String(base64Image)))
        using (var img = Image.FromStream(ms))
        {
            g.DrawImage(img, rect);
        }
    }

    public static void FillRoundedPath(Graphics g, Color color, Rectangle rect, int curve,
        bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
    {
        using (var brush = new SolidBrush(color))
        {
            g.FillPath(brush, RoundRec(rect, curve, topLeft, topRight, bottomLeft, bottomRight));
        }
    }

    public static void FillRoundedPath(Graphics g, Brush brush, Rectangle rect, int curve,
        bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
    {
        g.FillPath(brush, RoundRec(rect, curve, topLeft, topRight, bottomLeft, bottomRight));
    }

    public static void DrawRoundedPath(Graphics g, Color color, float size, Rectangle rect, int curve,
        bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
    {
        using (var pen = new Pen(color, size))
        {
            g.DrawPath(pen, RoundRec(rect, curve, topLeft, topRight, bottomLeft, bottomRight));
        }
    }

    public static Point[] Triangle(Color clr, Point p1, Point p2, Point p3)
    {
        return new Point[] { p1, p2, p3 };
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
        g.DrawString(text, font, brush,
            new Rectangle(0, rect.Y + (rect.Height / 2) - ((int)g.MeasureString(text, font).Height / 2), rect.Width, rect.Height),
            new StringFormat { Alignment = StringAlignment.Center });
    }

    public static void LeftString(Graphics g, string text, Font font, Brush brush, Rectangle rect)
    {
        g.DrawString(text, font, brush,
            new Rectangle(4, rect.Y + (rect.Height / 2) - ((int)g.MeasureString(text, font).Height / 2), rect.Width, rect.Height),
            new StringFormat { Alignment = StringAlignment.Near });
    }

    public static void RightString(Graphics g, string text, Font font, Brush brush, Rectangle rect)
    {
        g.DrawString(text, font, brush,
            new Rectangle(4, rect.Y + (rect.Height / 2) - ((int)g.MeasureString(text, font).Height / 2), rect.Width - rect.Height + 10, rect.Height),
            new StringFormat { Alignment = StringAlignment.Far });
    }

    public static GraphicsPath RoundRec(Rectangle r, int curve, bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
    {
        var path = new GraphicsPath(FillMode.Winding);

        if (topLeft) path.AddArc(r.X, r.Y, curve, curve, 180f, 90f);
        else path.AddLine(r.X, r.Y, r.X, r.Y);

        if (topRight) path.AddArc(r.Right - curve, r.Y, curve, curve, 270f, 90f);
        else path.AddLine(r.Right - r.Width, r.Y, r.Width, r.Y);

        if (bottomRight) path.AddArc(r.Right - curve, r.Bottom - curve, curve, curve, 0f, 90f);
        else path.AddLine(r.Right, r.Bottom, r.Right, r.Bottom);

        if (bottomLeft) path.AddArc(r.X, r.Bottom - curve, curve, curve, 90f, 90f);
        else path.AddLine(r.X, r.Bottom, r.X, r.Bottom);

        path.CloseFigure();
        return path;
    }

}
public class NordTheme : ContainerControl
{
    public NordTheme()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.ContainerControl | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 13, FontStyle.Bold);
        UpdateStyles();
        Dock = DockStyle.Fill;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        using (SolidBrush backgroundBrush = new SolidBrush(ColorTranslator.FromHtml("#bbd2d8")),
                          headerBrush = new SolidBrush(ColorTranslator.FromHtml("#174b7a")),
                          footerBrush = new SolidBrush(ColorTranslator.FromHtml("#164772")))
        {
            using (Pen linePen = new Pen(ColorTranslator.FromHtml("#002e5e"), 2))
            {
                e.Graphics.FillRectangle(backgroundBrush, new Rectangle(0, 0, Width, Height));
                e.Graphics.FillRectangle(headerBrush, new Rectangle(0, 0, Width, 58));
                e.Graphics.FillRectangle(footerBrush, new Rectangle(0, 58, Width, 10));
                e.Graphics.DrawLine(linePen, new Point(0, 68), new Point(Width, 68));
            }
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        Invalidate();
    }
}
public class NordButtonGreen : Control
{
    private enum MouseMode
    {
        NormalMode,
        Hovered,
        Pushed
    }

    private MouseMode State;
    private Color _NormalColor;
    private Color _NormalBorderColor;
    private Color _NormalTextColor = Color.White;
    private Color _HoverColor;
    private Color _HoverBorderColor;
    private Color _HoverTextColor = Color.White;
    private Color _PushedColor;
    private Color _PushedBorderColor;
    private Color _PushedTextColor = Color.White;

    public NordButtonGreen()
    {
        _NormalColor = GetHTMLColor("#75b81b");
        _NormalBorderColor = GetHTMLColor("#83ae48");
        _HoverColor = GetHTMLColor("#8dd42e");
        _HoverBorderColor = GetHTMLColor("#83ae48");
        _PushedColor = GetHTMLColor("#548710");
        _PushedBorderColor = GetHTMLColor("#83ae48");

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI", 12, FontStyle.Bold);
        UpdateStyles();
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет кнопки в нормальном состоянии мыши.")]
    public Color NormalColor
    {
        get => _NormalColor;
        set { _NormalColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет границы кнопки в нормальном состоянии мыши.")]
    public Color NormalBorderColor
    {
        get => _NormalBorderColor;
        set { _NormalBorderColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет текста кнопки в нормальном состоянии мыши.")]
    public Color NormalTextColor
    {
        get => _NormalTextColor;
        set { _NormalTextColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет кнопки в состоянии наведения мыши.")]
    public Color HoverColor
    {
        get => _HoverColor;
        set { _HoverColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет границы кнопки в состоянии наведения мыши.")]
    public Color HoverBorderColor
    {
        get => _HoverBorderColor;
        set { _HoverBorderColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет текста кнопки в состоянии наведения мыши.")]
    public Color HoverTextColor
    {
        get => _HoverTextColor;
        set { _HoverTextColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет кнопки в состоянии нажатия мыши.")]
    public Color PushedColor
    {
        get => _PushedColor;
        set { _PushedColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет границы кнопки в состоянии нажатия мыши.")]
    public Color PushedBorderColor
    {
        get => _PushedBorderColor;
        set { _PushedBorderColor = value; Invalidate(); }
    }

    [Category("Custom Properties")]
    [Description("Получает или задает цвет текста кнопки в состоянии нажатия мыши.")]
    public Color PushedTextColor
    {
        get => _PushedTextColor;
        set { _PushedTextColor = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        switch (State)
        {
            case MouseMode.NormalMode:
                DrawButton(g, rect, _NormalColor, _NormalBorderColor, _NormalTextColor);
                break;
            case MouseMode.Hovered:
                DrawButton(g, rect, _HoverColor, _HoverBorderColor, _HoverTextColor);
                break;
            case MouseMode.Pushed:
                DrawButton(g, rect, _PushedColor, _PushedBorderColor, _PushedTextColor);
                break;
        }
    }

    private void DrawButton(Graphics g, Rectangle rect, Color bgColor, Color borderColor, Color textColor)
    {
        using (var lgb = new LinearGradientBrush(new Rectangle(0, Height - 5, Width - 1, Height - 1),
                                                  Color.FromArgb(20, 0, 0, 0),
                                                  Color.FromArgb(20, 0, 0, 0), 90F))
        {
            FillRoundedPath(g, new SolidBrush(bgColor), rect, 5);
            FillRoundedPath(g, lgb, rect, 5);
            DrawRoundedPath(g, borderColor, 1, rect, 5);
            CentreString(g, Text, new Font("Arial", 11, FontStyle.Bold), new SolidBrush(textColor), rect);
        }
    }


    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseMode.Hovered; Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseMode.Pushed; Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseMode.Hovered; Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseMode.NormalMode; Invalidate();
    }

    private void FillRoundedPath(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (GraphicsPath path = GetRoundedPath(rect, radius))
        {
            g.FillPath(brush, path);
        }
    }

    private void DrawRoundedPath(Graphics g, Color color, int width, Rectangle rect, int radius)
    {
        using (GraphicsPath path = GetRoundedPath(rect, radius))
        {
            using (Pen pen = new Pen(color, width))
            {
                g.DrawPath(pen, path);
            }
        }
    }

    private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }

    private void CentreString(Graphics g, string text, Font font, Brush brush, Rectangle rect)
    {
        SizeF textSize = g.MeasureString(text, font);
        PointF location = new PointF(
            rect.X + (rect.Width - textSize.Width) / 2,
            rect.Y + (rect.Height - textSize.Height) / 2);
        g.DrawString(text, font, brush, location);
    }

    private static Color GetHTMLColor(string htmlColor)
    {
        return ColorTranslator.FromHtml(htmlColor);
    }
}
public class NordButtonClear : Control
{
    #region " Variables "

    private MouseMode state;
    private int roundRadius = 5;
    private bool isEnabled = true;

    #endregion

    #region " Properties "

    [Category("Пользовательские свойства"),
    Description("Получает или задает значение, указывающее, может ли элемент управления иметь закругленные углы.")]
    public int RoundRadius
    {
        get { return roundRadius; }
        set
        {
            roundRadius = value;
            Invalidate();
        }
    }

    [Category("Пользовательские свойства"),
    Description("Получает или задает значение, указывающее, может ли элемент управления реагировать на взаимодействие с пользователем.")]
    public bool IsEnabled
    {
        get { return isEnabled; }
        set
        {
            Enabled = value;
            isEnabled = value;
            Invalidate();
        }
    }

    #endregion

    #region " Constructors "

    public NordButtonClear()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI", 12, FontStyle.Bold);
        UpdateStyles();
    }

    #endregion

    #region " Draw Control "

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        if (IsEnabled)
        {
            switch (state)
            {
                case MouseMode.NormalMode:
                    DrawRoundedPath(g, GetHTMLColor("164772"), 1, rect, RoundRadius);
                    CentreString(g, Text, new Font("Arial", 11, FontStyle.Regular), SolidBrushHTMLColor("164772"), rect);
                    break;
                case MouseMode.Hovered:
                    FillRoundedPath(g, SolidBrushHTMLColor("eeeeee"), rect, RoundRadius);
                    DrawRoundedPath(g, GetHTMLColor("d7d7d7"), 1, rect, RoundRadius);
                    CentreString(g, Text, new Font("Arial", 9, FontStyle.Bold), SolidBrushHTMLColor("d7d7d7"), rect);
                    break;
                case MouseMode.Pushed:
                    FillRoundedPath(g, SolidBrushHTMLColor("f3f3f3"), rect, RoundRadius);
                    DrawRoundedPath(g, GetHTMLColor("d7d7d7"), 1, rect, RoundRadius);
                    CentreString(g, Text, new Font("Arial", 9, FontStyle.Bold), SolidBrushHTMLColor("747474"), rect);
                    break;
            }
        }
        else
        {
            DrawRoundedPath(g, GetHTMLColor("dadada"), 1, rect, RoundRadius);
            CentreString(g, Text, new Font("Arial", 9, FontStyle.Bold), SolidBrushHTMLColor("dadada"), rect);
        }
    }

    #endregion

    #region " Events "

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

    #endregion

    #region Draw
    private void DrawRoundedPath(Graphics g, Color color, int width, Rectangle rect, int radius)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            using (Pen pen = new Pen(color, width))
            {
                g.DrawPath(pen, path);
            }
        }
    }

    private void FillRoundedPath(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();

            g.FillPath(brush, path);
        }
    }

    private void CentreString(Graphics g, string text, Font font, Brush brush, Rectangle rect)
    {
        SizeF textSize = g.MeasureString(text, font);
        float x = rect.X + (rect.Width - textSize.Width) / 2;
        float y = rect.Y + (rect.Height - textSize.Height) / 2;
        g.DrawString(text, font, brush, x, y);
    }

    private Color GetHTMLColor(string htmlColor)
    {
        return ColorTranslator.FromHtml("#" + htmlColor);
    }

    private Brush SolidBrushHTMLColor(string htmlColor)
    {
        return new SolidBrush(GetHTMLColor(htmlColor));
    }

#endregion
}

public enum MouseMode
{
    NormalMode,
    Hovered,
    Pushed
}

[DefaultEvent("CheckedChanged")]
public class NordSwitchBlue : Control
{
    #region Variables

    private bool _Checked;
    private MouseMode State = MouseMode.NormalMode;
    private Color _UnCheckedColor = Color.Black;
    private Color _CheckedColor = ColorTranslator.FromHtml("#3075bb");
    private Color _CheckedBallColor = Color.White;
    private Color _UnCheckedBallColor = Color.Black;

    #endregion

    #region Properties

    [Category("Appearance")]
    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control color while unchecked")]
    public Color UnCheckedColor
    {
        get => _UnCheckedColor;
        set
        {
            _UnCheckedColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control color while checked")]
    public Color CheckedColor
    {
        get => _CheckedColor;
        set
        {
            _CheckedColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control ball color while checked")]
    public Color CheckedBallColor
    {
        get => _CheckedBallColor;
        set
        {
            _CheckedBallColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control ball color while unchecked")]
    public Color UnCheckedBallColor
    {
        get => _UnCheckedBallColor;
        set
        {
            _UnCheckedBallColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var brush = new SolidBrush(Checked ? CheckedColor : UnCheckedColor))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (Checked)
            {
                FillRoundedPath(e.Graphics, brush, new Rectangle(0, 0, 40, 16), 16);
                e.Graphics.FillEllipse(new SolidBrush(CheckedBallColor), new Rectangle((int)(Width - 14.5f), (int)2.7f, 10, 10));
            }
            else
            {
                DrawRoundedPath(e.Graphics, UnCheckedColor, 1.8f, new Rectangle(0, 0, 40, 16), 16);
                e.Graphics.FillEllipse(new SolidBrush(UnCheckedBallColor), new Rectangle((int)2.7f, (int)2.7f, 10, 10));
            }
        }
    }

    #endregion

    #region Constructors

    public NordSwitchBlue()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        ForeColor = ColorTranslator.FromHtml("#222222");
        UpdateStyles();
    }

    #endregion

    #region Events

    public event EventHandler CheckedChanged;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(42, 18);
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        Invalidate();
        base.OnTextChanged(e);
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        State = MouseMode.Hovered;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseMode.NormalMode;
        Invalidate();
    }

    #endregion

    // Метод для заполнения закругленного пути
    private void FillRoundedPath(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }

    // Метод для рисования закругленного пути
    private void DrawRoundedPath(Graphics g, Color color, float width, Rectangle rect, int radius)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            using (Pen pen = new Pen(color, width))
            {
                g.DrawPath(pen, path);
            }
        }
    }

    private enum MouseMode
    {
        NormalMode,
        Hovered
    }
}

[DefaultEvent("CheckedChanged")]
public class NordSwitchGreen : Control
{
    #region Variables

    private bool _Checked;
    private MouseMode State = MouseMode.NormalMode;
    private Color _UnCheckedColor = ColorTranslator.FromHtml("#dedede");
    private Color _CheckedColor = ColorTranslator.FromHtml("#3acf5f");
    private Color _CheckedBallColor = Color.White;
    private Color _UnCheckedBallColor = Color.White;

    #endregion

    #region Properties

    [Category("Appearance")]
    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control color while unchecked")]
    public Color UnCheckedColor
    {
        get => _UnCheckedColor;
        set
        {
            _UnCheckedColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control color while checked")]
    public Color CheckedColor
    {
        get => _CheckedColor;
        set
        {
            _CheckedColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control ball color while checked")]
    public Color CheckedBallColor
    {
        get => _CheckedBallColor;
        set
        {
            _CheckedBallColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control ball color while unchecked")]
    public Color UnCheckedBallColor
    {
        get => _UnCheckedBallColor;
        set
        {
            _UnCheckedBallColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Constructors

    public NordSwitchGreen()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        UpdateStyles();
    }

    #endregion

    #region Events

    public event EventHandler CheckedChanged;

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(30, 19);
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        Invalidate();
        base.OnTextChanged(e);
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        State = MouseMode.Hovered;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseMode.NormalMode;
        Invalidate();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var brush = new SolidBrush(Checked ? CheckedColor : UnCheckedColor))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (Checked)
            {
                FillRoundedPath(e.Graphics, brush, new Rectangle(0, 1, 28, 16), 16);
                e.Graphics.FillEllipse(new SolidBrush(CheckedBallColor), new Rectangle(Width - 17, 0, 16, 18));
            }
            else
            {
                FillRoundedPath(e.Graphics, brush, new Rectangle(0, 1, 28, 16), 16);
                e.Graphics.FillEllipse(new SolidBrush(UnCheckedBallColor), new Rectangle((int)0.5f, 0, 16, 18));
            }
        }
    }

    #endregion

    // Метод для заполнения закругленного пути
    private void FillRoundedPath(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }

    private enum MouseMode
    {
        NormalMode,
        Hovered
    }
}

[DefaultEvent("CheckedChanged")]
public class NordSwitchPower : Control
{
    #region Variables

    private bool _Checked;
    private MouseMode State = MouseMode.NormalMode;
    private Color _UnCheckedColor = ColorTranslator.FromHtml("#103859");
    private Color _CheckedColor = ColorTranslator.FromHtml("#103859");
    private Color _CheckedBallColor = ColorTranslator.FromHtml("#f1f1f1");
    private Color _UnCheckedBallColor = ColorTranslator.FromHtml("#f1f1f1");
    private Color _CheckedPowerColor = ColorTranslator.FromHtml("#73ba10");
    private Color _UnCheckedPowerColor = ColorTranslator.FromHtml("#c3c3c3");

    #endregion

    #region Properties

    [Category("Appearance")]
    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control color while unchecked")]
    public Color UnCheckedColor
    {
        get => _UnCheckedColor;
        set
        {
            _UnCheckedColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control color while checked")]
    public Color CheckedColor
    {
        get => _CheckedColor;
        set
        {
            _CheckedColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control ball color while checked")]
    public Color CheckedBallColor
    {
        get => _CheckedBallColor;
        set
        {
            _CheckedBallColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control ball color while unchecked")]
    public Color UnCheckedBallColor
    {
        get => _UnCheckedBallColor;
        set
        {
            _UnCheckedBallColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control power symbol color while checked")]
    public Color CheckedPowerColor
    {
        get => _CheckedPowerColor;
        set
        {
            _CheckedPowerColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the switch control power symbol color while unchecked")]
    public Color UnCheckedPowerColor
    {
        get => _UnCheckedPowerColor;
        set
        {
            _UnCheckedPowerColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Constructors

    public NordSwitchPower()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        UpdateStyles();
    }

    #endregion

    #region Events

    public event EventHandler CheckedChanged;

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        Invalidate();
        base.OnTextChanged(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(60, 44);
        Invalidate();
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        State = MouseMode.Hovered;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseMode.NormalMode;
        Invalidate();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var brush = new SolidBrush(Checked ? CheckedColor : UnCheckedColor))
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (Checked)
            {
                FillRoundedPath(e.Graphics, brush, new Rectangle(0, 8, 55, 25), 20);
                e.Graphics.FillEllipse(new SolidBrush(UnCheckedBallColor), new Rectangle(Width - 39, 0, 35, 40));
                e.Graphics.DrawArc(new Pen(CheckedPowerColor, 2), Width - 31, 10, 19, Height - 23, -62, 300);
                e.Graphics.DrawLine(new Pen(CheckedPowerColor, 2), Width - 22, 8, Width - 22, 17);
            }
            else
            {
                FillRoundedPath(e.Graphics, brush, new Rectangle(2, 8, 55, 25), 20);
                e.Graphics.FillEllipse(new SolidBrush(UnCheckedBallColor), new Rectangle(0, 0, 35, 40));
                e.Graphics.DrawArc(new Pen(UnCheckedPowerColor, 2), 7.5f, 10, Width - 41, Height - 23, -62, 300);
                e.Graphics.DrawLine(new Pen(UnCheckedPowerColor, 2), 17, 8, 17, 17);
            }
        }
    }

    // Метод для заполнения закругленного пути
    private void FillRoundedPath(Graphics g, Brush brush, Rectangle rect, int radius)
    {
        using (GraphicsPath path = new GraphicsPath())
        {
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }

    private enum MouseMode
    {
        NormalMode,
        Hovered
    }
    #endregion
}

public class NordHorizontalTabControl : TabControl
{
    #region Variables

    private Color _TabPageColor = ColorTranslator.FromHtml("#bbd2d8");
    private Color _TabColor = ColorTranslator.FromHtml("#174b7a");
    private Color _TabLowerColor = ColorTranslator.FromHtml("#164772");
    private Color _TabSelectedTextColor = Color.White;
    private Color _TabUnSlectedTextColor = ColorTranslator.FromHtml("#7188ad");

    #endregion

    #region Constructors

    public NordHorizontalTabControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        SizeMode = TabSizeMode.Fixed;
        Dock = DockStyle.None;
        ItemSize = new Size(80, 55);
        Alignment = TabAlignment.Top;
        UpdateStyles();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        // Фон TabControl
        g.Clear(_TabPageColor); // Убедитесь, что это цвет фонового цвета

        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

        // Отрисовка основной вкладки и нижней линии
        g.FillRectangle(new SolidBrush(_TabColor), new Rectangle(0, 0, Width, 60));
        g.FillRectangle(new SolidBrush(_TabLowerColor), new Rectangle(0, 52, Width, 8));

        for (int i = 0; i < TabCount; i++)
        {
            Rectangle rect = GetTabRect(i);
            Brush textBrush = (SelectedIndex == i)
                ? new SolidBrush(_TabSelectedTextColor)
                : new SolidBrush(_TabUnSlectedTextColor);

            g.DrawString(TabPages[i].Text, new Font("Helvetica CE", 9, FontStyle.Bold),
                textBrush, rect.X + 30, rect.Y + 20, new StringFormat()
                { Alignment = StringAlignment.Center });
        }
    }

    #endregion

    #region Events

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        foreach (TabPage tab in TabPages)
        {
            tab.BackColor = _TabPageColor; // Установка цвета фона для каждой вкладки
            tab.ForeColor = Color.Black; // Установка цвета текста на вкладке
        }
    }

    #endregion

    #region Properties

    [Category("Custom Properties"),
    Description("Gets or sets the tabpages color of the tabcontrol")]
    public Color TabPageColor
    {
        get => _TabPageColor;
        set
        {
            _TabPageColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the tabcontrol header color")]
    public Color TabColor
    {
        get => _TabColor;
        set
        {
            _TabColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the tabcontrol line color below the header")]
    public Color TabLowerColor
    {
        get => _TabLowerColor;
        set
        {
            _TabLowerColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the tabcontrol Text color while selected")]
    public Color TabSelectedTextColor
    {
        get => _TabSelectedTextColor;
        set
        {
            _TabSelectedTextColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the tabcontrol Text color while unchecked")]
    public Color TabUnSlectedTextColor
    {
        get => _TabUnSlectedTextColor;
        set
        {
            _TabUnSlectedTextColor = value;
            Invalidate();
        }
    }

    #endregion
}
public class NordVerticalTabControl : TabControl
{
    #region Variables

    private Color _TabColor = ColorTranslator.FromHtml("#f6f6f6");
    private Color _TabPageColor = Color.White;
    private Color _TabSelectedTextColor = ColorTranslator.FromHtml("#174b7a");
    private Color _TabUnSlectedTextColor = Color.DarkGray;

    #endregion

    #region Constructors

    public NordVerticalTabControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        SizeMode = TabSizeMode.Fixed;
        Dock = DockStyle.None;
        ItemSize = new Size(35, 135);
        Alignment = TabAlignment.Left;
        Font = new Font("Segoe UI", 8);
        UpdateStyles();
    }

    #endregion

    #region Properties

    [Category("Custom Properties"),
    Description("Gets or sets the tabcontrol left side color")]
    public Color TabColor
    {
        get => _TabColor;
        set
        {
            _TabColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the tabpages color of the tabcontrol")]
    public Color TabPageColor
    {
        get => _TabPageColor;
        set
        {
            _TabPageColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the tabcontrol Text color while selected")]
    public Color TabSelectedTextColor
    {
        get => _TabSelectedTextColor;
        set
        {
            _TabSelectedTextColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the tabcontrol Text color while not selected")]
    public Color TabUnSlectedTextColor
    {
        get => _TabUnSlectedTextColor;
        set
        {
            _TabUnSlectedTextColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.FillRectangle(new SolidBrush(TabColor), new Rectangle(0, 0, ItemSize.Height, Height));
        g.FillRectangle(new SolidBrush(TabPageColor), new Rectangle(ItemSize.Height, 0, Width - ItemSize.Height, Height));

        for (int i = 0; i < TabCount; i++)
        {
            Rectangle r = GetTabRect(i);
            Brush textBrush = (SelectedIndex == i)
                ? new SolidBrush(TabSelectedTextColor)
                : new SolidBrush(TabUnSlectedTextColor);

            CentreString(g, TabPages[i].Text, Font, textBrush, new Rectangle(r.X, r.Y + 5, r.Width - 4, r.Height));

            if (ImageList != null)
            {
                g.DrawImage(ImageList.Images[i], new Rectangle(r.X + 9, r.Y + 10, 20, 20));
            }
        }
    }

    #endregion

    #region Events

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        foreach (TabPage tab in TabPages)
        {
            tab.BackColor = TabPageColor;
        }
    }

    #endregion

    #region Helper Methods

    private void CentreString(Graphics g, string text, Font font, Brush brush, RectangleF layoutRect)
    {
        SizeF textSize = g.MeasureString(text, font);
        PointF drawPoint = new PointF(
            layoutRect.X + (layoutRect.Width - textSize.Width) / 2,
            layoutRect.Y + (layoutRect.Height - textSize.Height) / 2);

        g.DrawString(text, font, brush, drawPoint);
    }

    #endregion
}
public class NordGroupBox : ContainerControl
{
    #region Variables

    private Color _HeaderColor = ColorTranslator.FromHtml("#f8f8f9");
    private Color _HeaderTextColor = ColorTranslator.FromHtml("#dadada");
    private Color _BorderColor = ColorTranslator.FromHtml("#eaeaeb");
    private Color _BaseColor = Color.White;

    #endregion

    #region Properties

    [Category("Custom Properties"),
    Description("Gets or sets the Header color for the control.")]
    public Color HeaderColor
    {
        get => _HeaderColor;
        set
        {
            _HeaderColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the text color for the control.")]
    public Color HeaderTextColor
    {
        get => _HeaderTextColor;
        set
        {
            _HeaderTextColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the background border color for the control.")]
    public Color BorderColor
    {
        get => _BorderColor;
        set
        {
            _BorderColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties"),
    Description("Gets or sets the background color for the control.")]
    public Color BaseColor
    {
        get => _BaseColor;
        set
        {
            _BaseColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Constructors

    public NordGroupBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 10);
        BackColor = Color.Transparent;
        UpdateStyles();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        Rectangle rect = new Rectangle(0, 0, Width, Height);

        g.FillRectangle(new SolidBrush(BaseColor), rect);
        g.FillRectangle(new SolidBrush(HeaderColor), new Rectangle(0, 0, Width, 50));

        g.DrawLine(new Pen(BorderColor, 1), new Point(0, 50), new Point(Width, 50));
        g.DrawRectangle(new Pen(BorderColor, 1), new Rectangle(0, 0, Width - 1, Height - 1));

        g.DrawString(Text, Font, new SolidBrush(HeaderTextColor),
            new Rectangle(5, 0, Width, 50), new StringFormat { LineAlignment = StringAlignment.Center });
    }

    #endregion
}
public class NordTextbox : Control
{
    private readonly TextBox T;
    private HorizontalAlignment _TextAlign;
    private int _MaxLength;
    private bool _ReadOnly;
    private bool _UseSystemPasswordChar;
    private string _WatermarkText;
    private Image _SideImage;
    private Color _BackColor;
    private Color _NormalLineColor;
    private Color _HoverLineColor;
    private Color _PushedLineColor;
    private MouseMode State;

    private const int WM_SETTEXT = 0x000C;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

    public NordTextbox()
    {
        // Инициализация значений по умолчанию
        _TextAlign = HorizontalAlignment.Left;
        _MaxLength = 32767;
        _ReadOnly = false;
        _UseSystemPasswordChar = false;
        _WatermarkText = string.Empty;
        _SideImage = null;
        _BackColor = ColorTranslator.FromHtml("#bbd2d8");
        _NormalLineColor = ColorTranslator.FromHtml("#eaeaeb");
        _HoverLineColor = ColorTranslator.FromHtml("#fc3955");
        _PushedLineColor = ColorTranslator.FromHtml("#fc3955");
        State = MouseMode.NormalMode;

        // Инициализация и настройки TextBox
        T = new TextBox
        {
            Multiline = false,
            Cursor = Cursors.IBeam,
            BackColor = _BackColor,
            ForeColor = ColorTranslator.FromHtml("#eaeaeb"),
            BorderStyle = BorderStyle.None,
            Location = new Point(7, 8),
            Font = new Font("Segoe UI", 11),
            Size = new Size(Width - 10, 30),
            UseSystemPasswordChar = _UseSystemPasswordChar
        };

        // Установка стилей и размера контрола
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                  ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        Size = new Size(135, 40); // Увеличьте высоту, если нужно
        UpdateStyles();

        // Добавляем обработчики событий
        T.TextChanged += T_TextChanged;
        T.KeyDown += T_KeyDown;
        Controls.Add(T);
    }

    public override Color BackColor
    {
        get => _BackColor;
        set
        {
            _BackColor = value;
            T.BackColor = value; // Обновляем BackColor у TextBox
            Invalidate(); // Перерисовываем контрола
        }
    }

    public override string Text
    {
        get => T.Text; // Возвращаем текст из TextBox
        set
        {
            T.Text = value; // Устанавливаем текст в TextBox
            Invalidate(); // Перерисовываем контрола
        }
    }

    public HorizontalAlignment TextAlign
    {
        get => _TextAlign;
        set
        {
            _TextAlign = value;
            T.TextAlign = value;
        }
    }

    public int MaxLength
    {
        get => _MaxLength;
        set
        {
            _MaxLength = value;
            T.MaxLength = value;
        }
    }

    public bool ReadOnly
    {
        get => _ReadOnly;
        set
        {
            _ReadOnly = value;
            T.ReadOnly = value;
        }
    }

    public bool UseSystemPasswordChar
    {
        get => _UseSystemPasswordChar;
        set
        {
            _UseSystemPasswordChar = value;
            T.UseSystemPasswordChar = value;
        }
    }

    public string WatermarkText
    {
        get => _WatermarkText;
        set
        {
            _WatermarkText = value;
            SendMessage(T.Handle, WM_SETTEXT, IntPtr.Zero, value);
            Invalidate();
        }
    }

    public Image SideImage
    {
        get => _SideImage;
        set
        {
            _SideImage = value;
            Invalidate();
        }
    }

    public Color NormalLineColor
    {
        get => _NormalLineColor;
        set
        {
            _NormalLineColor = value;
            Invalidate();
        }
    }

    public Color HoverLineColor
    {
        get => _HoverLineColor;
        set
        {
            _HoverLineColor = value;
            Invalidate();
        }
    }

    public Color PushedLineColor
    {
        get => _PushedLineColor;
        set
        {
            _PushedLineColor = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        switch (State)
        {
            case MouseMode.NormalMode:
                g.DrawLine(new Pen(NormalLineColor, 1), new Point(0, 29), new Point(Width, 29));
                break;
            case MouseMode.Hovered:
                g.DrawLine(new Pen(HoverLineColor, 1), new Point(0, 29), new Point(Width, 29));
                break;
            case MouseMode.Pushed:
                g.DrawLine(new Pen(PushedLineColor, 1), new Point(0, 29), new Point(Width, 29));
                break;
        }

        if (_SideImage != null)
        {
            T.Location = new Point(33, 5);
            T.Width = Width - 60;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(SideImage, new Rectangle(8, 5, 16, 16));
        }
        else
        {
            T.Location = new Point(7, 5);
            T.Width = Width - 10;
        }

        if (ContextMenuStrip != null) T.ContextMenuStrip = ContextMenuStrip;
    }

    private void T_TextChanged(object sender, EventArgs e)
    {
        // Необходимо обновлять текст самого контрола при изменение текста в TextBox
        Text = T.Text;
    }

    private void T_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A) e.SuppressKeyPress = true;
        if (e.Control && e.KeyCode == Keys.C)
        {
            T.Copy();
            e.SuppressKeyPress = true;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 30;
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        State = MouseMode.Hovered;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseMode.NormalMode;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseMode.Pushed;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseMode.NormalMode;
        Invalidate();
    }

    private enum MouseMode
    {
        NormalMode,
        Hovered,
        Pushed
    }
}
public class NordCheckBox : Control
{
    #region Variables

    private bool _Checked;
    private MouseMode State = MouseMode.NormalMode;
    private Color _BorderColor = ColorTranslator.FromHtml("#164772");
    private Color _CheckColor = ColorTranslator.FromHtml("#5db5e9");

    #endregion

    #region Properties

    [Category("Appearance")]
    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            OnCheckedChanged(EventArgs.Empty);
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the Checkbox control color while checked.")]
    public Color BorderColor
    {
        get => _BorderColor;
        set
        {
            _BorderColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the Checkbox control check symbol color while checked.")]
    public Color CheckColor
    {
        get => _CheckColor;
        set
        {
            _CheckColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Constructors

    public NordCheckBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        ForeColor = ColorTranslator.FromHtml("#222222");
        Font = new Font("Segoe UI", 9, FontStyle.Regular);
        UpdateStyles();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using (GraphicsPath checkBorder = new GraphicsPath { FillMode = FillMode.Winding })
        {
            checkBorder.AddArc(0, 0, 10, 8, 180, 90);
            checkBorder.AddArc(8, 0, 8, 10, -90, 90);
            checkBorder.AddArc(8, 8, 8, 8, 0, 70);
            checkBorder.AddArc(0, 8, 10, 8, 90, 90);
            checkBorder.CloseAllFigures();

            g.DrawPath(new Pen(BorderColor, 1.5f), checkBorder);
            if (Checked)
            {
                g.DrawString("a", new Font("Marlett", 13), new SolidBrush(CheckColor), new Rectangle(-2, (int)0.5f, Width, Height));
            }
        }

        g.DrawString(Text, Font, new SolidBrush(ForeColor),
            new Rectangle(18, 1, Width, Height - 4),
            new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
    }

    #endregion

    #region Events

    public event EventHandler CheckedChanged;

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        Invalidate();
        base.OnTextChanged(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 18;
        Invalidate();
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        State = MouseMode.Hovered;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseMode.NormalMode;
        Invalidate();
    }

    #endregion

    private enum MouseMode
    {
        NormalMode,
        Hovered
    }
}
[DefaultEvent("CheckedChanged")]
public class NordRadioButton : Control
{
    #region Variables

    private bool _Checked;
    protected int _Group = 1;
    private MouseMode State = MouseMode.NormalMode;
    private Color _CheckBorderColor = ColorTranslator.FromHtml("#164772");
    private Color _UnCheckBorderColor = Color.Black;
    private Color _CheckColor = Color.Black;

    #endregion

    #region Events

    public event EventHandler CheckedChanged;

    #endregion

    #region Properties

    [Category("Appearance")]
    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            // Здесь мы вызываем событие CheckedChanged
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }


    [Category("Appearance")]
    public int Group
    {
        get => _Group;
        set
        {
            _Group = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the Radiobutton control border color while checked.")]
    public Color CheckBorderColor
    {
        get => _CheckBorderColor;
        set
        {
            _CheckBorderColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the Radiobutton control border color while unchecked.")]
    public Color UnCheckBorderColor
    {
        get => _UnCheckBorderColor;
        set
        {
            _UnCheckBorderColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the Radiobutton control check symbol color while checked.")]
    public Color CheckColor
    {
        get => _CheckColor;
        set
        {
            _CheckColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Constructors

    public NordRadioButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                  ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        ForeColor = ColorTranslator.FromHtml("#222222");
        Font = new Font("Segoe UI", 9, FontStyle.Regular);
        UpdateStyles();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        Rectangle r = new Rectangle(1, 1, 18, 18);

        g.SmoothingMode = SmoothingMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        if (Checked)
        {
            g.FillEllipse(new SolidBrush(CheckColor), new Rectangle(4, 4, 12, 12));
            g.DrawEllipse(new Pen(CheckBorderColor, 2), r);
        }
        else
        {
            g.DrawEllipse(new Pen(UnCheckBorderColor, 2), r);
        }

        g.DrawString(Text, Font, new SolidBrush(ForeColor),
            new Rectangle(21, 1, Width, Height - 2),
            new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
    }

    #endregion

    #region Events

    private void UpdateState()
    {
        if (!IsHandleCreated || !Checked) return;
        foreach (Control c in Parent.Controls)
        {
            if (c != this && c is NordRadioButton radioButton && radioButton.Group == _Group)
            {
                radioButton.Checked = false;
            }
        }
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = true; // Обновление состояния на checked
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
        Invalidate();
        base.OnTextChanged(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 21;
        Invalidate();
    }

    #endregion

    private enum MouseMode
    {
        NormalMode,
        Hovered
    }
}
public class NordComboBox : ComboBox
{
    #region Variables

    private int _StartIndex = 0;
    private Color _BaseColor = ColorTranslator.FromHtml("#bbd2d8");
    private Color _LinesColor = ColorTranslator.FromHtml("#75b81b");
    private Color _TextColor = Color.Gray;

    #endregion

    #region Constructors

    public NordComboBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        BackColor = ColorTranslator.FromHtml("#291a2a");
        Font = new Font("Segoe UI", 12, FontStyle.Regular);
        DrawMode = DrawMode.OwnerDrawFixed;
        DoubleBuffered = true;
        StartIndex = 0;
        DropDownHeight = 100;
        DropDownStyle = ComboBoxStyle.DropDownList;
        UpdateStyles();
    }

    #endregion

    #region Properties

    [Category("Behavior")]
    [Description("When overridden in a derived class, gets or sets the zero-based index of the currently selected item.")]
    private int StartIndex
    {
        get => _StartIndex;
        set
        {
            _StartIndex = value;
            try
            {
                base.SelectedIndex = value;
            }
            catch
            {
                // Ignore exceptions
            }
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the background color for the control.")]
    public Color BaseColor
    {
        get => _BaseColor;
        set
        {
            _BaseColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the lines color for the control.")]
    public Color LinesColor
    {
        get => _LinesColor;
        set
        {
            _LinesColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the text color for the control.")]
    public Color TextColor
    {
        get => _TextColor;
        set
        {
            _TextColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Draw Control

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        try
        {
            Graphics g = e.Graphics;
            e.DrawBackground();
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.FillRectangle(new SolidBrush(BaseColor), e.Bounds);

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                Cursor = Cursors.Hand;
                CentreString(g, Items[e.Index].ToString(), Font, new SolidBrush(TextColor),
                             new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 3, e.Bounds.Width - 2, e.Bounds.Height - 2));
            }
            else
            {
                CentreString(g, Items[e.Index].ToString(), Font, Brushes.DimGray,
                             new Rectangle(e.Bounds.X + 1, e.Bounds.Y + 2, e.Bounds.Width - 2, e.Bounds.Height - 2));
            }
        }
        catch
        {
            // Ignore exceptions
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Rectangle rect = new Rectangle(0, 0, Width, Height - 1);
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.FillRectangle(new SolidBrush(BaseColor), rect);

        // Draw lines
        g.DrawLine(new Pen(LinesColor, 2), new Point(Width - 21, (Height / 2) - 3), new Point(Width - 7, (Height / 2) - 3));
        g.DrawLine(new Pen(LinesColor, 2), new Point(Width - 21, (Height / 2) + 1), new Point(Width - 7, (Height / 2) + 1));
        g.DrawLine(new Pen(LinesColor, 2), new Point(Width - 21, (Height / 2) + 5), new Point(Width - 7, (Height / 2) + 5));

        // Draw bottom line
        g.DrawLine(new Pen(LinesColor, 1), new Point(0, Height - 1), new Point(Width, Height - 1));

        // Draw text
        g.DrawString(Text, Font, new SolidBrush(TextColor),
                     new Rectangle(5, 1, Width - 1, Height - 1),
                     new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });
    }

    #endregion

    #region Events

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    #endregion

    #region Helper Methods

    private void CentreString(Graphics g, string text, Font font, Brush brush, Rectangle rectangle)
    {
        StringFormat format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        g.DrawString(text, font, brush, rectangle, format);
    }

    #endregion
}
public class NordSeperator : Control
{
    #region Variables

    private Style _SepStyle = Style.Horizental;
    private Color _SeperatorColor = ColorTranslator.FromHtml("#eaeaeb");

    #endregion

    #region Enumerators

    public enum Style
    {
        Horizental,
        Vertiacal
    }

    #endregion

    #region Properties

    [Category("Appearance")]
    [Description("Gets or sets the style for the control.")]
    public Style SepStyle
    {
        get => _SepStyle;
        set
        {
            _SepStyle = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the color for the control.")]
    public Color SeperatorColor
    {
        get => _SeperatorColor;
        set
        {
            _SeperatorColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Constructors

    public NordSeperator()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        UpdateStyles();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        switch (SepStyle)
        {
            case Style.Horizental:
                g.DrawLine(new Pen(SeperatorColor), 0, 1, Width, 1);
                break;
            case Style.Vertiacal:
                g.DrawLine(new Pen(SeperatorColor), 1, 0, 1, Height);
                break;
        }
    }

    #endregion

    #region Events

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (SepStyle == Style.Horizental)
            Height = 4;
        else
            Width = 4;
    }

    #endregion
}
[DefaultEvent("TextChanged")]
public class NordLabel : Control
{
    #region Constructors

    public NordLabel()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint |
                  ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI", 12, FontStyle.Regular);
        UpdateStyles();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.DrawString(Text, Font, new SolidBrush(ForeColor), ClientRectangle);
    }

    #endregion

    #region Events

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = Font.Height;
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    #endregion
}
[DefaultEvent("TextChanged")]
public class NordLinkLabel : Control
{
    #region Variables

    private MouseMode State = MouseMode.NormalMode;
    private Color _HoverColor = Color.SteelBlue;
    private Color _PushedColor = Color.DarkBlue;

    #endregion

    #region Properties

    [Category("Custom Properties")]
    [Description("Gets or sets the text color of the control in mouse hover state.")]
    public Color HoverColor
    {
        get => _HoverColor;
        set
        {
            _HoverColor = value;
            Invalidate();
        }
    }

    [Category("Custom Properties")]
    [Description("Gets or sets the text color of the control in mouse down state.")]
    public Color PushedColor
    {
        get => _PushedColor;
        set
        {
            _PushedColor = value;
            Invalidate();
        }
    }

    #endregion

    #region Constructors

    public NordLinkLabel()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint |
                  ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        ForeColor = Color.MediumBlue;
        Font = new Font("Segoe UI", 12, FontStyle.Underline);
        UpdateStyles();
    }

    #endregion

    #region Draw Control

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        switch (State)
        {
            case MouseMode.NormalMode:
                g.DrawString(Text, Font, new SolidBrush(ForeColor), ClientRectangle);
                break;
            case MouseMode.Hovered:
                Cursor = Cursors.Hand;
                g.DrawString(Text, Font, new SolidBrush(HoverColor), ClientRectangle);
                break;
            case MouseMode.Pushed:
                g.DrawString(Text, Font, new SolidBrush(PushedColor), ClientRectangle);
                break;
        }
    }

    #endregion

    #region Events

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = Font.Height + 2;
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseMode.Hovered;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseMode.Pushed;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseMode.Hovered;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseMode.NormalMode;
        Invalidate();
    }

    #endregion

    private enum MouseMode
    {
        NormalMode,
        Hovered,
        Pushed
    }
}