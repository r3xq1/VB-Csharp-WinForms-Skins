using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

public enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

public enum IconStyle
{
    VSIcon,
    FormIcon
}

public enum FormOrWhole
{
    WholeApplication,
    Form
}

// Helper Methods for Drawing
public static class DrawHelpers
{
    public static GraphicsPath RoundRectangle(Rectangle rectangle, int curve)
    {
        var path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;

        path.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        path.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        path.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 0, 90);
        path.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 90, 90);
        path.AddLine(new Point(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y), new Point(rectangle.X, curve + rectangle.Y));
        return path;
    }

    public static GraphicsPath RoundRect(float x, float y, float w, float h, float r = 0.3f, bool TL = true, bool TR = true, bool BR = true, bool BL = true)
    {
        float d = Math.Min(w, h) * r;
        var path = new GraphicsPath();
        float xw = x + w, yh = y + h;

        if (TL) path.AddArc(x, y, d, d, 180, 90);
        else path.AddLine(x, y, x, y);
        if (TR) path.AddArc(xw - d, y, d, d, 270, 90);
        else path.AddLine(xw, y, xw, y);
        if (BR) path.AddArc(xw - d, yh - d, d, d, 0, 90);
        else path.AddLine(xw, yh, xw, yh);
        if (BL) path.AddArc(x, yh - d, d, d, 90, 90);
        else path.AddLine(x, yh, x, yh);

        path.CloseFigure();
        return path;
    }
}

public class VisualStudioContainer : ContainerControl
{
    private bool _allowClose = true;
    private bool _allowMinimize = true;
    private bool _allowMaximize = true;
    private int _fontSize = 12;
    private bool _showIcon = true;
    private MouseState _state = MouseState.None;
    private int _mouseXLoc;
    private int _mouseYLoc;
    private bool _captureMovement = false;
    private const int MoveHeight = 35;
    private Point _mouseP;
    private Color _fontColour = Color.FromArgb(153, 153, 153);
    private Color _baseColour = Color.FromArgb(45, 45, 48);
    private Color _iconColour = Color.FromArgb(255, 255, 255);
    private Color _controlBoxColours = Color.FromArgb(248, 248, 248);
    private Color _borderColour = Color.FromArgb(15, 15, 18);
    private Color _hoverColour = Color.FromArgb(63, 63, 65);
    private Color _pressedColour = Color.FromArgb(0, 122, 204);
    private Font _font = new Font("Microsoft Sans Serif", 9);
    private IconStyle _iconStyle = IconStyle.FormIcon;
    private FormOrWhole _formOrWhole = FormOrWhole.WholeApplication;
    private Form _form = Form.ActiveForm;

    [Category("Control")]
    public FormOrWhole FormOrWhole
    {
        get => _formOrWhole;
        set
        {
            _formOrWhole = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public Form Form
    {
        get => _form;
        set
        {
            if (value != null)
            {
                _form = value;
                Invalidate();
            }
        }
    }

    [Category("Control")]
    public IconStyle IconStyle
    {
        get => _iconStyle;
        set
        {
            _iconStyle = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public int FontSize
    {
        get => _fontSize;
        set => _fontSize = value;
    }

    [Category("Control")]
    public bool AllowMinimize
    {
        get => _allowMinimize;
        set => _allowMinimize = value;
    }

    [Category("Control")]
    public bool AllowMaximize
    {
        get => _allowMaximize;
        set => _allowMaximize = value;
    }

    [Category("Control")]
    public bool ShowIcon
    {
        get => _showIcon;
        set
        {
            _showIcon = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool AllowClose
    {
        get => _allowClose;
        set => _allowClose = value;
    }

    [Category("Colours")]
    public Color BorderColour
    {
        get => _borderColour;
        set => _borderColour = value;
    }

    [Category("Colours")]
    public Color HoverColour
    {
        get => _hoverColour;
        set => _hoverColour = value;
    }

    [Category("Colours")]
    public Color BaseColour
    {
        get => _baseColour;
        set => _baseColour = value;
    }

    [Category("Colours")]
    public Color FontColour
    {
        get => _fontColour;
        set => _fontColour = value;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _captureMovement = false;
        _state = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _state = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _state = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _mouseXLoc = e.Location.X;
        _mouseYLoc = e.Location.Y;
        Invalidate();

        if (_captureMovement)
        {
            Parent.Location = MousePosition - (Size)_mouseP;
        }
        Cursor = e.Y > 26 ? Cursors.Arrow : Cursors.Hand;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _state = MouseState.Down;

        if (_mouseXLoc > Width - 30 && _mouseXLoc < Width && _mouseYLoc < 26)
        {
            if (_allowClose)
            {
                if (_formOrWhole == FormOrWhole.Form)
                {
                    if (_form == null)
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        if (_form.InvokeRequired)
                        {
                            _form.Invoke(new Action(() => _form.Close()));
                        }
                        else
                        {
                            _form.Close();
                        }
                    }
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }
        else if (_mouseXLoc > Width - 60 && _mouseXLoc < Width - 30 && _mouseYLoc < 26)
        {
            if (_allowMaximize)
            {
                switch (FindForm().WindowState)
                {
                    case FormWindowState.Maximized:
                        FindForm().WindowState = FormWindowState.Normal;
                        break;
                    case FormWindowState.Normal:
                        FindForm().WindowState = FormWindowState.Maximized;
                        break;
                }
            }
        }
        else if (_mouseXLoc > Width - 90 && _mouseXLoc < Width - 60 && _mouseYLoc < 26)
        {
            if (_allowMinimize)
            {
                if (FindForm().WindowState == FormWindowState.Normal)
                {
                    FindForm().WindowState = FormWindowState.Minimized;
                }
            }
        }
        else if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width - 90, MoveHeight).Contains(e.Location))
        {
            _captureMovement = true;
            _mouseP = e.Location;
        }
        else
        {
            Focus();
        }

        Invalidate();
    }

    public VisualStudioContainer()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        BackColor = _baseColour;
        Dock = DockStyle.Fill;
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (ParentForm != null)
        {
            ParentForm.FormBorderStyle = FormBorderStyle.None;
            ParentForm.AllowTransparency = false;
            ParentForm.TransparencyKey = Color.Fuchsia;
            ParentForm.StartPosition = FormStartPosition.CenterParent;
            Dock = DockStyle.Fill;
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.FillRectangle(new SolidBrush(_baseColour), new Rectangle(0, 0, Width, Height));
        g.DrawRectangle(new Pen(_borderColour), new Rectangle(0, 0, Width, Height));

        if (_state == MouseState.Over)
        {
            if (_mouseXLoc > Width - 30 && _mouseXLoc < Width && _mouseYLoc < 26)
            {
                g.FillRectangle(new SolidBrush(_hoverColour), new Rectangle(Width - 30, 1, 29, 25));
            }
            else if (_mouseXLoc > Width - 60 && _mouseXLoc < Width - 30 && _mouseYLoc < 26)
            {
                g.FillRectangle(new SolidBrush(_hoverColour), new Rectangle(Width - 60, 1, 30, 25));
            }
            else if (_mouseXLoc > Width - 90 && _mouseXLoc < Width - 60 && _mouseYLoc < 26)
            {
                g.FillRectangle(new SolidBrush(_hoverColour), new Rectangle(Width - 90, 1, 30, 25));
            }
        }

        // Close Button
        g.DrawLine(new Pen(_controlBoxColours, 2), Width - 20, 10, Width - 12, 18);
        g.DrawLine(new Pen(_controlBoxColours, 2), Width - 20, 18, Width - 12, 10);

        // Minimize Button
        g.FillRectangle(new SolidBrush(_controlBoxColours), Width - 79, 17, 8, 2);

        // Maximize Button
        if (FindForm().WindowState == FormWindowState.Normal)
        {
            g.DrawLine(new Pen(_controlBoxColours), Width - 49, 18, Width - 40, 18);
            g.DrawLine(new Pen(_controlBoxColours), Width - 49, 18, Width - 49, 10);
            g.DrawLine(new Pen(_controlBoxColours), Width - 40, 18, Width - 40, 10);
            g.DrawLine(new Pen(_controlBoxColours), Width - 49, 10, Width - 40, 10);
            g.DrawLine(new Pen(_controlBoxColours), Width - 49, 11, Width - 40, 11);
        }
        else if (FindForm().WindowState == FormWindowState.Maximized)
        {
            g.DrawLine(new Pen(_controlBoxColours), Width - 48, 16, Width - 39, 16);
            g.DrawLine(new Pen(_controlBoxColours), Width - 48, 16, Width - 48, 8);
            g.DrawLine(new Pen(_controlBoxColours), Width - 39, 16, Width - 39, 8);
            g.DrawLine(new Pen(_controlBoxColours), Width - 48, 8, Width - 39, 8);
            g.DrawLine(new Pen(_controlBoxColours), Width - 48, 9, Width - 39, 9);
            g.FillRectangle(new SolidBrush(_baseColour), new Rectangle(Width - 51, 12, 9, 8));
            g.DrawLine(new Pen(_controlBoxColours), Width - 51, 20, Width - 42, 20);
            g.DrawLine(new Pen(_controlBoxColours), Width - 51, 20, Width - 51, 12);
            g.DrawLine(new Pen(_controlBoxColours), Width - 42, 20, Width - 42, 12);
            g.DrawLine(new Pen(_controlBoxColours), Width - 51, 12, Width - 42, 12);
            g.DrawLine(new Pen(_controlBoxColours), Width - 51, 13, Width - 42, 13);
        }

        if (_showIcon)
        {
            if (_iconStyle == IconStyle.FormIcon)
            {
                g.DrawIcon(FindForm().Icon, new Rectangle(6, 6, 22, 22));
                g.DrawString(Text, _font, new SolidBrush(_fontColour), new RectangleF(37, 0, Width - 110, 32), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });
            }
            else
            {
                // Custom Icon Drawing Logic
                // Replace this placeholder with actual drawing code based on your icon definition.
            }
        }
        else
        {
            g.DrawString(Text, _font, new SolidBrush(_fontColour), new RectangleF(5, 0, Width - 110, 30), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });
        }
    }
}

public class VisualStudioButton : Control
{
    #region "Declarations"
    private MouseState State = MouseState.None;
    private Color _FontColour = Color.White; // Изменено на белый для лучшей видимости
    private Color _BaseColour = Color.FromArgb(45, 45, 48);
    private Color _HoverColour = Color.FromArgb(60, 60, 62);
    private Color _PressedColour = Color.FromArgb(37, 37, 39);
    private Color _BorderColour = Color.FromArgb(15, 15, 18);
    private bool _ShowBorder = true;
    private bool _ShowImage = false;
    private bool _ShowText = true; // Убедитесь, что значение по умолчанию true
    private Image _Image = null;
    private StringAlignment _TextAlignment = StringAlignment.Center;
    private ImageAlignment _ImageAlignment = ImageAlignment.Left;

    #endregion

    #region "Properties"

    public enum ImageAlignment
    {
        Left,
        Middle,
        Right
    }

    [Category("Control")]
    public ImageAlignment ImageAlignmentX
    {
        get { return _ImageAlignment; }
        set
        {
            _ImageAlignment = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public Image ImageChoice
    {
        get { return _Image; }
        set
        {
            _Image = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public StringAlignment TextAlignment
    {
        get { return _TextAlignment; }
        set
        {
            _TextAlignment = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool ShowImage
    {
        get { return _ShowImage; }
        set
        {
            _ShowImage = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool ShowText
    {
        get { return _ShowText; }
        set
        {
            _ShowText = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color BorderColour
    {
        get { return _BorderColour; }
        set
        {
            _BorderColour = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color HoverColour
    {
        get { return _HoverColour; }
        set
        {
            _HoverColour = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color BaseColour
    {
        get { return _BaseColour; }
        set
        {
            _BaseColour = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color FontColour
    {
        get { return _FontColour; }
        set
        {
            _FontColour = value;
            Invalidate();
        }
    }

    #endregion

    #region "Mouse Events"

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        Invalidate();
    }

    #endregion

    #region "Draw Control"

    public VisualStudioButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        BackColor = _BaseColour;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var G = e.Graphics;
        G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        G.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

        switch (State)
        {
            case MouseState.None:
                G.FillRectangle(new SolidBrush(_BaseColour), new Rectangle(0, 0, Width, Height));
                break;
            case MouseState.Over:
                G.FillRectangle(new SolidBrush(_HoverColour), new Rectangle(0, 0, Width, Height));
                break;
            case MouseState.Down:
                G.FillRectangle(new SolidBrush(_PressedColour), new Rectangle(0, 0, Width, Height));
                break;
        }

        if (_ShowBorder)
        {
            G.DrawRectangle(new Pen(_BorderColour, 1), new Rectangle(0, 0, Width - 1, Height - 1));
        }

        if (_ShowImage)
        {
            if ((Width > 50) && (Height > 30))
            {
                if (_ImageAlignment == ImageAlignment.Left)
                {
                    G.DrawImage(_Image, new Rectangle(10, 10, Height - 20, Height - 20));
                    if (_ShowText)
                    {
                        G.DrawString(Text, Font, new SolidBrush(_FontColour), new Rectangle(10 + Height - 20 + 10, 0, Width - 20 - (Height - 10) - 10, Height), new StringFormat { Alignment = _TextAlignment, LineAlignment = StringAlignment.Center });
                    }
                }
                else if (_ImageAlignment == ImageAlignment.Right)
                {
                    G.DrawImage(_Image, new Rectangle(Width - Height + 10, 10, Height - 20, Height - 20));
                    if (_ShowText)
                    {
                        G.DrawString(Text, Font, new SolidBrush(_FontColour), new Rectangle(10, 0, Width - 20 - (Height - 20) - 10, Height), new StringFormat { Alignment = _TextAlignment, LineAlignment = StringAlignment.Center });
                    }
                }
            }
        }
        else
        {
            if (_ShowText)
            {
                G.DrawString(Text, Font, new SolidBrush(_FontColour), new Rectangle(10, 0, Width - 20, Height), new StringFormat { Alignment = _TextAlignment, LineAlignment = StringAlignment.Center });
            }
        }
        G.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
    }

    #endregion
}

public class VisualStudioSeperator : Control
{
    #region Declarations
    private Color _FontColour = Color.FromArgb(153, 153, 153);
    private Color _LineColour = Color.FromArgb(0, 122, 204);
    private Font _Font = new Font("Microsoft Sans Serif", 8);
    private bool _ShowText;
    private StringAlignment _TextAlignment = StringAlignment.Center;
    private __TextLocation _TextLocation = __TextLocation.Left;
    private bool _AddEndNotch = false;
    private bool _UnderlineText = false;
    private bool _ShowTextAboveLine = false;
    private bool _OnlyUnderlineText = false;
    #endregion

    #region Properties

    [Category("Control")]
    public __TextLocation TextLocation
    {
        get => _TextLocation;
        set
        {
            _TextLocation = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public StringAlignment TextAlignment
    {
        get => _TextAlignment;
        set
        {
            _TextAlignment = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool ShowTextAboveLine
    {
        get => _ShowTextAboveLine;
        set
        {
            _ShowTextAboveLine = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool OnlyUnderlineText
    {
        get => _OnlyUnderlineText;
        set
        {
            _OnlyUnderlineText = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool UnderlineText
    {
        get => _UnderlineText;
        set
        {
            _UnderlineText = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool AddEndNotch
    {
        get => _AddEndNotch;
        set
        {
            _AddEndNotch = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool ShowText
    {
        get => _ShowText;
        set
        {
            _ShowText = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color LineColour
    {
        get => _LineColour;
        set => _LineColour = value;
    }

    [Category("Colours")]
    public Color FontColour
    {
        get => _FontColour;
        set => _FontColour = value;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (_ShowText && (Height < _Font.Size * 2 + 3))
        {
            this.Size = new Size(Width, (int)(_Font.Size * 2 + 3));
        }
        Invalidate();
    }

    public enum __TextLocation
    {
        Left,
        Middle,
        Right
    }
    #endregion

    #region Draw Control

    public VisualStudioSeperator()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);
        this.DoubleBuffered = true;
        this.BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics G = e.Graphics;
        G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        G.SmoothingMode = SmoothingMode.AntiAlias;
        G.PixelOffsetMode = PixelOffsetMode.HighQuality;

        if (_ShowText && !_ShowTextAboveLine)
        {
            DrawTextAndLine(G);
        }
        else if (_ShowText)
        {
            DrawTextAboveLine(G);
        }
        else
        {
            DrawLine(G);
        }

        G.InterpolationMode = InterpolationMode.HighQualityBicubic;
    }

    private void DrawTextAndLine(Graphics G)
    {
        float textWidth = G.MeasureString(Text, _Font).Width;
        float lineY = Height / 2;

        switch (_TextLocation)
        {
            case __TextLocation.Left:
                G.DrawString(Text, _Font, new SolidBrush(_FontColour),
                    new Rectangle(0, 0, (int)(textWidth + 10), Height),
                    new StringFormat { Alignment = _TextAlignment, LineAlignment = StringAlignment.Center });

                G.DrawLine(new Pen(_LineColour), new Point((int)(textWidth + 20), (int)lineY), new Point(Width, (int)lineY));
                DrawEndNotch(G, lineY, textWidth);
                DrawUnderlineIfNeeded(G, lineY, textWidth, true);
                break;

            case __TextLocation.Middle:
                G.DrawString(Text, _Font, new SolidBrush(_FontColour),
                    new Rectangle((int)((Width / 2) - (textWidth / 2) - 10), 0, (int)(textWidth + 10), Height),
                    new StringFormat { Alignment = _TextAlignment, LineAlignment = StringAlignment.Center });

                G.DrawLine(new Pen(_LineColour), new Point(0, (int)lineY), new Point((int)((Width / 2) - (textWidth / 2) - 20), (int)lineY));
                G.DrawLine(new Pen(_LineColour), new Point((int)((Width / 2) + (textWidth / 2) + 10), (int)lineY), new Point(Width, (int)lineY));
                DrawEndNotch(G, lineY, textWidth);
                DrawUnderlineIfNeeded(G, lineY, textWidth, false);
                break;

            case __TextLocation.Right:
                G.DrawString(Text, _Font, new SolidBrush(_FontColour),
                    new Rectangle((int)(Width - textWidth - 10), 0, (int)(textWidth + 10), Height),
                    new StringFormat { Alignment = _TextAlignment, LineAlignment = StringAlignment.Center });

                G.DrawLine(new Pen(_LineColour), new Point(0, (int)lineY), new Point((int)(Width - textWidth - 20), (int)lineY));
                DrawEndNotch(G, lineY, textWidth);
                DrawUnderlineIfNeeded(G, lineY, textWidth, true);
                break;
        }
    }

    private void DrawTextAboveLine(Graphics G)
    {
        float textWidth = G.MeasureString(Text, _Font).Width;
        float lineY = Height / 2 + 6;

        if (_OnlyUnderlineText)
        {
            G.DrawLine(new Pen(_LineColour), new Point(5, (int)lineY), new Point((int)(textWidth + 8), (int)lineY));
            G.DrawString(Text, _Font, new SolidBrush(_FontColour),
                new Rectangle(5, 0, Width - 10, Height / 2 + 3),
                new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Far });
        }
        else
        {
            G.DrawLine(new Pen(_LineColour), new Point(0, (int)lineY), new Point(Width, (int)lineY));
            if (_AddEndNotch)
            {
                G.DrawLine(new Pen(_LineColour), new Point(Width - 1, Height / 2 - 5),
                    new Point(Width - 1, Height / 2 + 5));
                G.DrawLine(new Pen(_LineColour), new Point(1, Height / 2 - 5),
                    new Point(1, Height / 2 + 5));
            }
            G.DrawString(Text, _Font, new SolidBrush(_FontColour),
                new Rectangle(5, 0, Width - 10, Height / 2 + 3),
                new StringFormat { Alignment = _TextAlignment, LineAlignment = StringAlignment.Far });
        }
    }

    private void DrawLine(Graphics G)
    {
        float lineY = Height / 2;

        G.DrawLine(new Pen(_LineColour), new Point(0, (int)lineY), new Point(Width, (int)lineY));
        if (_AddEndNotch)
        {
            G.DrawLine(new Pen(_LineColour), new Point(Width - 1, (int)(lineY - 0.5f)),
                new Point(Width - 1, (int)(lineY + 0.5f)));
            G.DrawLine(new Pen(_LineColour), new Point(1, (int)(lineY - 0.5f)),
                new Point(1, (int)(lineY + 0.5f)));
        }
    }

    private void DrawUnderlineIfNeeded(Graphics G, float lineY, float textWidth, bool isLeftLocation)
    {
        if (_UnderlineText)
        {
            float underlineY = lineY + G.MeasureString(Text, _Font).Height / 2 + 3;

            if (isLeftLocation)
            {
                G.DrawLine(new Pen(_LineColour), 0, (int)underlineY, (int)(textWidth + 20), (int)underlineY);
                G.DrawLine(new Pen(_LineColour), (int)(textWidth + 20), (int)underlineY, (int)(textWidth + 20), (int)lineY);
            }
            else
            {
                float drawX = (Width / 2) - (textWidth / 2) - 20;
                G.DrawLine(new Pen(_LineColour), drawX, (int)lineY, drawX, (int)underlineY);
                drawX = (Width / 2) + (textWidth / 2) + 10;
                G.DrawLine(new Pen(_LineColour), drawX, (int)lineY, drawX, (int)underlineY);
                G.DrawLine(new Pen(_LineColour), Width / 2 - (textWidth / 2) - 20,
                    (int)underlineY, Width / 2 + (textWidth / 2) + 10, (int)underlineY);
            }
        }
    }

    private void DrawEndNotch(Graphics G, float lineY, float textWidth)
    {
        if (_AddEndNotch)
        {
            G.DrawLine(new Pen(_LineColour), new Point(Width - 1, (int)(lineY - G.MeasureString(Text, _Font).Height / 2)),
                new Point(Width - 1, (int)(lineY + G.MeasureString(Text, _Font).Height / 2)));
            G.DrawLine(new Pen(_LineColour), new Point(1, (int)(lineY - G.MeasureString(Text, _Font).Height / 2)),
                new Point(1, (int)(lineY + G.MeasureString(Text, _Font).Height / 2)));
        }
    }
    #endregion
}

public class VisualStudioStatusBar : Control
{
    #region Variables
    private Color _TextColour = Color.FromArgb(153, 153, 153);
    private Color _BaseColour = Color.FromArgb(45, 45, 48);
    private Color _RectColour = Color.FromArgb(0, 122, 204);
    private Color _BorderColour = Color.FromArgb(27, 27, 29);
    private Color _SeperatorColour = Color.FromArgb(45, 45, 48);
    private bool _ShowLine = true;
    private LinesCount _LinesToShow = LinesCount.One;
    private AmountOfStrings _NumberOfStrings = AmountOfStrings.One;
    private bool _ShowBorder = true;
    private StringFormat _FirstLabelStringFormat;
    private string _FirstLabelText = "Label1";
    private Alignments _FirstLabelAlignment = Alignments.Left;
    private StringFormat _SecondLabelStringFormat;
    private string _SecondLabelText = "Label2";
    private Alignments _SecondLabelAlignment = Alignments.Center;
    private StringFormat _ThirdLabelStringFormat;
    private string _ThirdLabelText = "Label3";
    private Alignments _ThirdLabelAlignment = Alignments.Center;
    #endregion

    #region Properties

    [Category("First Label Options")]
    public string FirstLabelText
    {
        get => _FirstLabelText;
        set => _FirstLabelText = value;
    }

    [Category("First Label Options")]
    public Alignments FirstLabelAlignment
    {
        get => _FirstLabelAlignment;
        set
        {
            _FirstLabelAlignment = value;
            SetStringFormat(value, ref _FirstLabelStringFormat);
        }
    }

    [Category("Second Label Options")]
    public string SecondLabelText
    {
        get => _SecondLabelText;
        set => _SecondLabelText = value;
    }

    [Category("Second Label Options")]
    public Alignments SecondLabelAlignment
    {
        get => _SecondLabelAlignment;
        set
        {
            _SecondLabelAlignment = value;
            SetStringFormat(value, ref _SecondLabelStringFormat);
        }
    }

    [Category("Third Label Options")]
    public string ThirdLabelText
    {
        get => _ThirdLabelText;
        set => _ThirdLabelText = value;
    }

    [Category("Third Label Options")]
    public Alignments ThirdLabelAlignment
    {
        get => _ThirdLabelAlignment;
        set
        {
            _ThirdLabelAlignment = value;
            SetStringFormat(value, ref _ThirdLabelStringFormat);
        }
    }

    [Category("Colours")]
    public Color BaseColour
    {
        get => _BaseColour;
        set => _BaseColour = value;
    }

    [Category("Colours")]
    public Color BorderColour
    {
        get => _BorderColour;
        set => _BorderColour = value;
    }

    [Category("Colours")]
    public Color TextColour
    {
        get => _TextColour;
        set => _TextColour = value;
    }

    public enum LinesCount
    {
        None = 0,
        One = 1,
        Two = 2
    }

    public enum AmountOfStrings
    {
        One,
        Two,
        Three
    }

    public enum Alignments
    {
        Left,
        Center,
        Right
    }

    [Category("Control")]
    public AmountOfStrings AmountOfString
    {
        get => _NumberOfStrings;
        set => _NumberOfStrings = value;
    }

    [Category("Control")]
    public LinesCount LinesToShow
    {
        get => _LinesToShow;
        set => _LinesToShow = value;
    }

    public bool ShowBorder
    {
        get => _ShowBorder;
        set => _ShowBorder = value;
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        Dock = DockStyle.Bottom;
    }

    [Category("Colours")]
    public Color RectangleColor
    {
        get => _RectColour;
        set => _RectColour = value;
    }

    public bool ShowLine
    {
        get => _ShowLine;
        set => _ShowLine = value;
    }
    #endregion

    #region Draw Control

    public VisualStudioStatusBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        Font = new Font("Segoe UI", 9);
        Size = new Size(Width, 20);
        Cursor = Cursors.Arrow;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics G = e.Graphics;
        var baseRectangle = new Rectangle(0, 0, Width, Height);

        using (var baseBrush = new SolidBrush(BaseColour))
        {
            G.SmoothingMode = SmoothingMode.HighQuality;
            G.PixelOffsetMode = PixelOffsetMode.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            G.FillRectangle(baseBrush, baseRectangle);

            switch (_LinesToShow)
            {
                case LinesCount.None:
                    DrawStrings(G);
                    break;

                case LinesCount.One:
                    DrawStrings(G);
                    G.FillRectangle(new SolidBrush(_RectColour), new Rectangle(5, 10, 14, 3));
                    break;

                case LinesCount.Two:
                    DrawStrings(G);
                    G.FillRectangle(new SolidBrush(_SeperatorColour), new Rectangle(5, 10, 14, 3));
                    G.FillRectangle(new SolidBrush(_SeperatorColour), new Rectangle(Width - 20, 10, 14, 3));
                    break;
            }

            if (_ShowBorder)
                G.DrawRectangle(new Pen(_BorderColour, 2), new Rectangle(0, 0, Width, Height));
        }
    }

    private void DrawStrings(Graphics G)
    {
        int padding = 25; // Отступ для текста
        switch (_NumberOfStrings)
        {
            case AmountOfStrings.One:
                G.DrawString(_FirstLabelText, Font, new SolidBrush(_TextColour), new Rectangle(padding, 1, Width - 5, Height), _FirstLabelStringFormat);
                break;

            case AmountOfStrings.Two:
                G.DrawString(_FirstLabelText, Font, new SolidBrush(_TextColour), new Rectangle(padding, 1, Width / 2 - 6, Height), _FirstLabelStringFormat);
                G.DrawString(_SecondLabelText, Font, new SolidBrush(_TextColour), new Rectangle(Width / 2 + 5, 1, Width / 2 - 10, Height), _SecondLabelStringFormat);
                G.DrawLine(new Pen(_SeperatorColour, 1), new Point(Width / 2, 6), new Point(Width / 2, Height - 6));
                break;

            case AmountOfStrings.Three:
                G.DrawString(_FirstLabelText, Font, new SolidBrush(_TextColour), new Rectangle(padding, 1, Width - Width / 3 * 2 - 6, Height), _FirstLabelStringFormat);
                G.DrawString(_SecondLabelText, Font, new SolidBrush(_TextColour), new Rectangle(Width - Width / 3 * 2 + 5, 1, Width - Width / 3 * 2 - 12, Height), _SecondLabelStringFormat);
                G.DrawString(_ThirdLabelText, Font, new SolidBrush(_TextColour), new Rectangle(Width - (Width / 3) + 6, 1, Width / 3 - 22, Height), _ThirdLabelStringFormat);
                G.DrawLine(new Pen(_SeperatorColour, 1), new Point(Width - Width / 3 * 2, 6), new Point(Width - Width / 3 * 2, Height - 6));
                G.DrawLine(new Pen(_SeperatorColour, 1), new Point(Width - (Width / 3), 6), new Point(Width - (Width / 3), Height - 6));
                break;
        }
    }

    private void SetStringFormat(Alignments alignment, ref StringFormat stringFormat)
    {
        stringFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = alignment switch
            {
                Alignments.Left => StringAlignment.Near,
                Alignments.Center => StringAlignment.Center,
                Alignments.Right => StringAlignment.Far,
                _ => stringFormat.Alignment
            }
        };
    }
    #endregion
}

[DefaultEvent("Scroll")]
public class VisualStudioVerticalScrollBar : Control
{
    #region "Declarations"

    private Color _BaseColour = Color.FromArgb(62, 62, 66);
    private Color _ThumbNormalColour = Color.FromArgb(104, 104, 104);
    private Color _ThumbHoverColour = Color.FromArgb(158, 158, 158);
    private Color _ThumbPressedColour = Color.FromArgb(239, 235, 239);
    private Color _ArrowNormalColour = Color.FromArgb(153, 153, 153);
    private Color _ArrowHoverColour = Color.FromArgb(39, 123, 181);
    private Color _ArrowPressedColour = Color.FromArgb(0, 113, 171);
    private Color _OuterBorderColour;
    private Color _ThumbBorderColour;

    private int _Minimum = 0;
    private int _Maximum = 100;
    private int _Value = 0;
    private int _SmallChange = 1;
    private int _ButtonSize = 16;
    private int _LargeChange = 10;
    private bool _ShowOuterBorder = false;
    private bool _ShowThumbBorder = false;
    private InnerLineCount _AmountOfInnerLines = InnerLineCount.None;  // Access changed from __InnerLineCount to InnerLineCount

    private Point _MousePos = new Point(0,0);
    private MouseState _ThumbState = MouseState.None;
    private MouseState _ArrowState = MouseState.None;
    private int _MouseXLoc;
    private int _MouseYLoc;

    private int ThumbMovement;
    private Rectangle TSA;
    private Rectangle BSA;
    private Rectangle Shaft;
    private Rectangle Thumb;
    private bool ShowThumb;
    private int _ThumbSize = 24;

    #endregion

    #region "Properties & Events"

    [Category("Colours")]
    public Color BaseColour
    {
        get { return _BaseColour; }
        set { _BaseColour = value; }
    }

    [Category("Colours")]
    public Color ThumbNormalColour
    {
        get { return _ThumbNormalColour; }
        set { _ThumbNormalColour = value; }
    }

    [Category("Colours")]
    public Color ThumbHoverColour
    {
        get { return _ThumbHoverColour; }
        set { _ThumbHoverColour = value; }
    }

    [Category("Colours")]
    public Color ThumbPressedColour
    {
        get { return _ThumbPressedColour; }
        set { _ThumbPressedColour = value; }
    }

    [Category("Colours")]
    public Color ArrowNormalColour
    {
        get { return _ArrowNormalColour; }
        set { _ArrowNormalColour = value; }
    }

    [Category("Colours")]
    public Color ArrowHoverColour
    {
        get { return _ArrowHoverColour; }
        set { _ArrowHoverColour = value; }
    }

    [Category("Colours")]
    public Color ArrowPressedColour
    {
        get { return _ArrowPressedColour; }
        set { _ArrowPressedColour = value; }
    }

    [Category("Colours")]
    public Color OuterBorderColour
    {
        get { return _OuterBorderColour; }
        set { _OuterBorderColour = value; }
    }

    [Category("Colours")]
    public Color ThumbBorderColour
    {
        get { return _ThumbBorderColour; }
        set { _ThumbBorderColour = value; }
    }

    [Category("Control")]
    public int Minimum
    {
        get { return _Minimum; }
        set
        {
            _Minimum = value;
            if (value > _Value) _Value = value;
            if (value > _Maximum) _Maximum = value;
            InvalidateLayout();
        }
    }

    [Category("Control")]
    public int Maximum
    {
        get { return _Maximum; }
        set
        {
            if (value < _Value) _Value = value;
            if (value < _Minimum) _Minimum = value;
            InvalidateLayout();
        }
    }

    [Category("Control")]
    public int Value
    {
        get { return _Value; }
        set
        {
            if (value == _Value) return;
            if (value < _Minimum) _Value = _Minimum;
            else if (value > _Maximum) _Value = _Maximum;
            else _Value = value;

            InvalidatePosition();
            Scroll?.DynamicInvoke(this);
        }
    }

    [Category("Control")]
    public int SmallChange
    {
        get { return _SmallChange; }
        set
        {
            if (value >= 1) _SmallChange = value;
        }
    }

    [Category("Control")]
    public int LargeChange
    {
        get { return _LargeChange; }
        set
        {
            if (value >= 1) _LargeChange = value;
        }
    }

    [Category("Control")]
    public int ButtonSize
    {
        get { return _ButtonSize; }
        set
        {
            if (value < 16) _ButtonSize = 16;
            else _ButtonSize = value;
        }
    }

    [Category("Control")]
    public bool ShowOuterBorder
    {
        get { return _ShowOuterBorder; }
        set
        {
            _ShowOuterBorder = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool ShowThumbBorder
    {
        get { return _ShowThumbBorder; }
        set
        {
            _ShowThumbBorder = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public InnerLineCount AmountOfInnerLines
    {
        get { return _AmountOfInnerLines; }
        set { _AmountOfInnerLines = value; }
    }

    public event EventHandler Scroll;

    #endregion

    #region "Methods"

    public VisualStudioVerticalScrollBar()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.Selectable |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        Size = new Size(19, 50);
    }

    protected override void OnSizeChanged(EventArgs e)  // Corrected event argument
    {
        base.OnSizeChanged(e);
        InvalidateLayout();
    }

    private void InvalidateLayout()
    {
        TSA = new Rectangle(0, 0, Width, 16);
        BSA = new Rectangle(0, Height - ButtonSize, Width, ButtonSize);
        Shaft = new Rectangle(0, TSA.Bottom + 1, Width, Height - Height / 8 - 8);
        ShowThumb = (_Maximum - _Minimum) > 0;

        if (ShowThumb)
        {
            Thumb = new Rectangle(4, 0, Width - 8, Height / 8);
        }

        Scroll?.DynamicInvoke(this);
        InvalidatePosition();
    }

    public enum InnerLineCount  // Changed access modifier from __InnerLineCount to InnerLineCount
    {
        None,
        One,
        Two,
        Three
    }

    public void InvalidatePosition()
    {
        Thumb.Y = (_Value - _Minimum) / (_Maximum - _Minimum) * (Shaft.Height - _ThumbSize) + 16;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && ShowThumb)
        {
            if (TSA.Contains(e.Location))
            {
                _ArrowState = MouseState.Down;
                ThumbMovement = _Value - _SmallChange;
            }
            else if (BSA.Contains(e.Location))
            {
                ThumbMovement = _Value + _SmallChange;
                _ArrowState = MouseState.Down;
            }
            else if (Thumb.Contains(e.Location))
            {
                _ThumbState = MouseState.Down;
                Invalidate();
                return;
            }
            else
            {
                ThumbMovement = e.Y < Thumb.Y ? _Value - _LargeChange : _Value + _LargeChange;
            }

            Value = Math.Min(Math.Max(ThumbMovement, _Minimum), _Maximum);
            Invalidate();
            InvalidatePosition();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _MouseXLoc = e.Location.X;
        _MouseYLoc = e.Location.Y;

        if (TSA.Contains(e.Location)) _ArrowState = MouseState.Over;
        else if (BSA.Contains(e.Location)) _ArrowState = MouseState.Over;
        else if (_ArrowState == MouseState.Down) _ArrowState = MouseState.None;

        if (Thumb.Contains(e.Location) && _ThumbState != MouseState.Down)
            _ThumbState = MouseState.Over;
        else if (_ThumbState != MouseState.Down)
            _ThumbState = MouseState.None;

        Invalidate();

        if (_ThumbState == MouseState.Down || (_ArrowState == MouseState.Down && ShowThumb))
        {
            int ThumbPosition = e.Y + 2 - TSA.Height - (_ThumbSize / 2);
            int ThumbBounds = Shaft.Height - _ThumbSize;
            ThumbMovement = (int)(ThumbPosition / (float)ThumbBounds * (_Maximum - _Minimum)) - _Minimum;
            Value = Math.Min(Math.Max(ThumbMovement, _Minimum), _Maximum);
            InvalidatePosition();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (Thumb.Contains(e.Location))
            _ThumbState = MouseState.Over;
        else
            _ThumbState = MouseState.None;

        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _ThumbState = MouseState.None;
        _ArrowState = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(_BaseColour);

        Point[] TrianglePointTop = { new Point(Width / 2, 5), new Point(Width / 4, 11), new Point(Width / 2 + Width / 4, 11) };
        Point[] TrianglePointBottom = { new Point(Width / 2, Height - 5), new Point(Width / 4, Height - 11), new Point(Width / 2 + Width / 4, Height - 11) };

        switch (_ThumbState)
        {
            case MouseState.None:
                using (SolidBrush SBrush = new SolidBrush(_ThumbNormalColour))
                    g.FillRectangle(SBrush, Thumb);
                break;
            case MouseState.Over:
                using (SolidBrush SBrush = new SolidBrush(_ThumbHoverColour))
                    g.FillRectangle(SBrush, Thumb);
                break;
            case MouseState.Down:
                using (SolidBrush SBrush = new SolidBrush(_ThumbPressedColour))
                    g.FillRectangle(SBrush, Thumb);
                break;
        }

        switch (_ArrowState)
        {
            case MouseState.Down:
                if (!Thumb.Contains(_MousePos))
                {
                    using (SolidBrush SBrush = new SolidBrush(_ThumbNormalColour))
                        g.FillRectangle(SBrush, Thumb);
                }
                if (_MouseYLoc < 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowPressedColour), TrianglePointTop);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointBottom);
                }
                else if (_MouseXLoc > Width - 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowPressedColour), TrianglePointBottom);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointTop);
                }
                else
                {
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointTop);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointBottom);
                }
                break;
            case MouseState.Over:
                if (_MouseYLoc < 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowHoverColour), TrianglePointTop);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointBottom);
                }
                else if (_MouseXLoc > Width - 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowHoverColour), TrianglePointBottom);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointTop);
                }
                else
                {
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointTop);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointBottom);
                }
                break;
            case MouseState.None:
                g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointTop);
                g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointBottom);
                break;
        }
    }

    #endregion

    private enum MouseState
    {
        None,
        Over,
        Down
    }
}

[DefaultEvent("Scroll")]
public class VisualStudioHorizontalScrollBar : Control
{
    #region "Declarations"

    private Color _BaseColour = Color.FromArgb(62, 62, 66);
    private Color _ThumbNormalColour = Color.FromArgb(104, 104, 104);
    private Color _ThumbHoverColour = Color.FromArgb(158, 158, 158);
    private Color _ThumbPressedColour = Color.FromArgb(239, 235, 239);
    private Color _ArrowNormalColour = Color.FromArgb(153, 153, 153);
    private Color _ArrowHoveerColour = Color.FromArgb(39, 123, 181);
    private Color _ArrowPressedColour = Color.FromArgb(0, 113, 171);
    private Color _OuterBorderColour;
    private Color _ThumbBorderColour;
    private int _Minimum = 0;
    private int _Maximum = 100;
    private int _Value = 0;
    private int _SmallChange = 1;
    private int _ButtonSize = 16;
    private int _LargeChange = 10;
    private bool _ShowOuterBorder = false;
    private bool _ShowThumbBorder = false;
    private Point _MousePos = new Point(0, 0);
    private MouseState _ThumbState = MouseState.None;
    private MouseState _ArrowState = MouseState.None;
    private int _MouseXLoc;
    private int _MouseYLoc;
    private int ThumbMovement;
    private Rectangle LSA;
    private Rectangle RSA;
    private Rectangle Shaft;
    private Rectangle Thumb;
    private bool ShowThumb;
    private int _ThumbSize = 24;

    #endregion

    #region "Properties & Events"

    [Category("Colours")]
    public Color BaseColour
    {
        get { return _BaseColour; }
        set { _BaseColour = value; }
    }

    [Category("Colours")]
    public Color ThumbNormalColour
    {
        get { return _ThumbNormalColour; }
        set { _ThumbNormalColour = value; }
    }

    [Category("Colours")]
    public Color ThumbHoverColour
    {
        get { return _ThumbHoverColour; }
        set { _ThumbHoverColour = value; }
    }

    [Category("Colours")]
    public Color ThumbPressedColour
    {
        get { return _ThumbPressedColour; }
        set { _ThumbPressedColour = value; }
    }

    [Category("Colours")]
    public Color ArrowNormalColour
    {
        get { return _ArrowNormalColour; }
        set { _ArrowNormalColour = value; }
    }

    [Category("Colours")]
    public Color ArrowHoveerColour
    {
        get { return _ArrowHoveerColour; }
        set { _ArrowHoveerColour = value; }
    }

    [Category("Colours")]
    public Color ArrowPressedColour
    {
        get { return _ArrowPressedColour; }
        set { _ArrowPressedColour = value; }
    }

    [Category("Colours")]
    public Color OuterBorderColour
    {
        get { return _OuterBorderColour; }
        set { _OuterBorderColour = value; }
    }

    [Category("Colours")]
    public Color ThumbBorderColour
    {
        get { return _ThumbBorderColour; }
        set { _ThumbBorderColour = value; }
    }

    [Category("Control")]
    public int Minimum
    {
        get { return _Minimum; }
        set
        {
            _Minimum = value;
            if (value > _Value) _Value = value;
            if (value > _Maximum) _Maximum = value;
            InvalidateLayout();
        }
    }

    [Category("Control")]
    public int Maximum
    {
        get { return _Maximum; }
        set
        {
            if (value < _Value) _Value = value;
            if (value < _Minimum) _Minimum = value;
            InvalidateLayout();
        }
    }

    [Category("Control")]
    public int Value
    {
        get { return _Value; }
        set
        {
            if (value == _Value) return;

            if (value < _Minimum) _Value = _Minimum;
            else if (value > _Maximum) _Value = _Maximum;
            else _Value = value;

            InvalidatePosition();
            Scroll?.DynamicInvoke(this);
        }
    }

    [Category("Control")]
    public int SmallChange
    {
        get { return _SmallChange; }
        set
        {
            if (value < 1) return;
            _SmallChange = value;
        }
    }

    [Category("Control")]
    public int LargeChange
    {
        get { return _LargeChange; }
        set
        {
            if (value < 1) return;
            _LargeChange = value;
        }
    }

    [Category("Control")]
    public int ButtonSize
    {
        get { return _ButtonSize; }
        set
        {
            if (value < 16) _ButtonSize = 16;
            else _ButtonSize = value;
        }
    }

    [Category("Control")]
    public bool ShowOuterBorder
    {
        get { return _ShowOuterBorder; }
        set
        {
            _ShowOuterBorder = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool ShowThumbBorder
    {
        get { return _ShowThumbBorder; }
        set
        {
            _ShowThumbBorder = value;
            Invalidate();
        }
    }

    public enum InnerLineCount  // Changed access modifier from __InnerLineCount to InnerLineCount
    {
        None,
        One,
        Two,
        Three
    }

    [Category("Control")]
    public InnerLineCount AmountOfInnerLines { get; set; } = InnerLineCount.None;

    protected override void OnSizeChanged(EventArgs e)
    {
        InvalidateLayout();
    }

    private void InvalidateLayout()
    {
        LSA = new Rectangle(0, 0, 16, Height);
        RSA = new Rectangle(Width - ButtonSize, 0, ButtonSize, Height);
        Shaft = new Rectangle(LSA.Right + 1, 0, Width - Width / 8 - 8, Height);
        ShowThumb = (_Maximum - _Minimum) > 0;

        if (ShowThumb)
        {
            Thumb = new Rectangle(0, 4, Width / 8, Height - 8);
        }

        Scroll?.DynamicInvoke(this);
        InvalidatePosition();
    }


    public event EventHandler Scroll;

    private void InvalidatePosition()
    {
        Thumb.X = (_Value - _Minimum) / (_Maximum - _Minimum) * (Shaft.Width - _ThumbSize) + 16;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && ShowThumb)
        {
            if (LSA.Contains(e.Location))
            {
                _ArrowState = MouseState.Down;
                ThumbMovement = _Value - _SmallChange;
            }
            else if (RSA.Contains(e.Location))
            {
                ThumbMovement = _Value + _SmallChange;
                _ArrowState = MouseState.Down;
            }
            else
            {
                if (Thumb.Contains(e.Location))
                {
                    _ThumbState = MouseState.Down;
                    Invalidate();
                    return;
                }
                else
                {
                    if (e.X < Thumb.X)
                    {
                        ThumbMovement = _Value - _LargeChange;
                    }
                    else
                    {
                        ThumbMovement = _Value + _LargeChange;
                    }
                }
            }

            Value = Math.Min(Math.Max(ThumbMovement, _Minimum), _Maximum);
            Invalidate();
            InvalidatePosition();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        _MouseXLoc = e.Location.X;
        _MouseYLoc = e.Location.Y;

        if (LSA.Contains(e.Location))
        {
            _ArrowState = MouseState.Over;
        }
        else if (RSA.Contains(e.Location))
        {
            _ArrowState = MouseState.Over;
        }
        else if (_ArrowState == MouseState.Down)
        {
            // Do nothing
        }
        else
        {
            _ArrowState = MouseState.None;
        }

        if (Thumb.Contains(e.Location) && _ThumbState != MouseState.Down)
        {
            _ThumbState = MouseState.Over;
        }
        else if (_ThumbState != MouseState.Down)
        {
            _ThumbState = MouseState.None;
        }

        Invalidate();

        if (_ThumbState == MouseState.Down || (_ArrowState == MouseState.Down && ShowThumb))
        {
            int ThumbPosition = e.X + 2 - LSA.Width - (_ThumbSize / 2);
            int ThumbBounds = Shaft.Width - _ThumbSize;
            ThumbMovement = (int)(ThumbPosition / (float)ThumbBounds * (_Maximum - _Minimum)) - _Minimum;
            Value = Math.Min(Math.Max(ThumbMovement, _Minimum), _Maximum);
            InvalidatePosition();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (Thumb.Contains(e.Location))
        {
            _ThumbState = MouseState.Over;
        }
        else
        {
            _ThumbState = MouseState.None;
        }

        if (e.Location.X < 16 || e.Location.X > Width - 16)
        {
            _ThumbState = MouseState.Over;
        }
        else
        {
            _ThumbState = MouseState.None;
        }

        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _ThumbState = MouseState.None;
        _ArrowState = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Invalidate();
    }

    #endregion

    #region "Draw Control"

    public VisualStudioHorizontalScrollBar()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint | ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        Size = new Size(50, 19);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(_BaseColour);

        Point[] TrianglePointLeft = {
            new Point(5, Height / 2),
            new Point(11, Height / 4),
            new Point(11, Height / 2 + Height / 4)
        };

        Point[] TrianglePointRight = {
            new Point(Width - 5, Height / 2),
            new Point(Width - 11, Height / 4),
            new Point(Width - 11, Height / 2 + Height / 4)
        };

        switch (_ThumbState)
        {
            case MouseState.None:
                using (SolidBrush sBrush = new SolidBrush(_ThumbNormalColour))
                {
                    g.FillRectangle(sBrush, Thumb);
                }
                break;
            case MouseState.Over:
                using (SolidBrush sBrush = new SolidBrush(_ThumbHoverColour))
                {
                    g.FillRectangle(sBrush, Thumb);
                }
                break;
            case MouseState.Down:
                using (SolidBrush sBrush = new SolidBrush(_ThumbPressedColour))
                {
                    g.FillRectangle(sBrush, Thumb);
                }
                break;
        }

        switch (_ArrowState)
        {
            case MouseState.Down:
                if (!Thumb.Contains(_MousePos))
                {
                    using (SolidBrush sBrush = new SolidBrush(_ThumbNormalColour))
                    {
                        g.FillRectangle(sBrush, Thumb);
                    }
                }
                if (_MouseXLoc < 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowPressedColour), TrianglePointLeft);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointRight);
                }
                else if (_MouseXLoc > Width - 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowPressedColour), TrianglePointRight);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointLeft);
                }
                else
                {
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointLeft);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointRight);
                }
                break;
            case MouseState.Over:
                if (_MouseXLoc < 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowHoveerColour), TrianglePointLeft);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointRight);
                }
                else if (_MouseXLoc > Width - 16)
                {
                    g.FillPolygon(new SolidBrush(_ArrowHoveerColour), TrianglePointRight);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointLeft);
                }
                else
                {
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointLeft);
                    g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointRight);
                }
                break;
            case MouseState.None:
                g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointLeft);
                g.FillPolygon(new SolidBrush(_ArrowNormalColour), TrianglePointRight);
                break;
        }

        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    }

    #endregion
}

public class VisualStudioRadialProgressBar : Control
{
    #region Declarations
    private Color _BorderColour = Color.FromArgb(28, 28, 28);
    private Color _BaseColour = Color.FromArgb(45, 45, 48);
    private Color _ProgressColour = Color.FromArgb(62, 62, 66);
    private Color _TextColour = Color.FromArgb(153, 153, 153);
    private int _Value = 0;
    private int _Maximum = 100;
    private int _StartingAngle = 110;
    private int _RotationAngle = 255;
    private Font _Font = new Font("Segoe UI", 20);
    #endregion

    #region Properties

    [Category("Control")]
    public int Maximum
    {
        get { return _Maximum; }
        set
        {
            if (value < _Value)
                _Value = value;
            _Maximum = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public int Value
    {
        get
        {
            return _Value;
        }
        set
        {
            if (value > _Maximum)
                value = _Maximum;
            _Value = value;
            Invalidate();
        }
    }

    public void Increment(int amount)
    {
        Value += amount;
    }

    [Category("Colours")]
    public Color BorderColour
    {
        get { return _BorderColour; }
        set
        {
            _BorderColour = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color TextColour
    {
        get { return _TextColour; }
        set
        {
            _TextColour = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color ProgressColour
    {
        get { return _ProgressColour; }
        set
        {
            _ProgressColour = value;
            Invalidate();
        }
    }

    [Category("Colours")]
    public Color BaseColour
    {
        get { return _BaseColour; }
        set
        {
            _BaseColour = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public int StartingAngle
    {
        get { return _StartingAngle; }
        set { _StartingAngle = value; }
    }

    [Category("Control")]
    public int RotationAngle
    {
        get { return _RotationAngle; }
        set { _RotationAngle = value; }
    }

    #endregion

    #region Draw Control

    public VisualStudioRadialProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        Size = new Size(78, 78);
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap B = new Bitmap(Width, Height))
        using (Graphics G = Graphics.FromImage(B))
        {
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            G.SmoothingMode = SmoothingMode.HighQuality;
            G.PixelOffsetMode = PixelOffsetMode.HighQuality;
            G.Clear(BackColor);

            DrawProgress(G);

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImageUnscaled(B, 0, 0);
        }
    }

    private void DrawProgress(Graphics G)
    {
        int centerX = Width / 2;
        int centerY = Height / 2;
        int diameter = Math.Min(Width - 4, Height - 4);
        int radius = diameter / 2;
        int offset = 3;

        // Draw Border and Base
        G.DrawArc(new Pen(_BorderColour, 1 + 6), offset, offset, diameter, diameter, _StartingAngle - 3, _RotationAngle + 5);
        G.DrawArc(new Pen(_BaseColour, 1 + 3), offset, offset, diameter, diameter, _StartingAngle, _RotationAngle);

        // Draw Progress
        if (_Value > 0)
        {
            int progressAngle = (int)((_RotationAngle / (double)_Maximum) * _Value);
            G.DrawArc(new Pen(_ProgressColour, 1 + 3), offset, offset, diameter, diameter, _StartingAngle, progressAngle);
        }

        // Draw Text
        G.DrawString(_Value.ToString(), _Font, new SolidBrush(_TextColour), new Point(centerX, centerY - 1), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
    }

    #endregion
}

public class VisualStudioTabControl : TabControl
{
    private Color _TextColour = Color.FromArgb(255, 255, 255);
    private Color _BackTabColour = Color.FromArgb(28, 28, 28);
    private Color _BaseColour = Color.FromArgb(45, 45, 48);
    private Color _ActiveColour = Color.FromArgb(0, 122, 204);
    private Color _BorderColour = Color.FromArgb(30, 30, 30);
    private Color _HorizLineColour = Color.FromArgb(0, 122, 204);
    private StringFormat CenterSF = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

    [Category("Colours")]
    public Color BorderColour
    {
        get { return _BorderColour; }
        set { _BorderColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color HorizontalLineColour
    {
        get { return _HorizLineColour; }
        set { _HorizLineColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color TextColour
    {
        get { return _TextColour; }
        set { _TextColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color BackTabColour
    {
        get { return _BackTabColour; }
        set { _BackTabColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color BaseColour
    {
        get { return _BaseColour; }
        set { _BaseColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color ActiveColour
    {
        get { return _ActiveColour; }
        set { _ActiveColour = value; Invalidate(); }
    }

    public VisualStudioTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        SizeMode = TabSizeMode.Normal;
        ItemSize = new Size(240, 16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        e.Graphics.Clear(_BaseColour);

        for (int i = 0; i < TabCount; i++)
        {
            Rectangle tabRect = GetTabRect(i);
            bool isSelected = i == SelectedIndex;

            // Рисуем фоновый цвет вкладки
            if (isSelected)
            {
                e.Graphics.FillRectangle(new SolidBrush(_ActiveColour), tabRect);
            }
            else
            {
                e.Graphics.FillRectangle(new SolidBrush(_BackTabColour), tabRect);
            }

            // Рисуем текст
            Rectangle textRect = new Rectangle(tabRect.X + 2, tabRect.Y + 2, tabRect.Width - 4, tabRect.Height - 4);
            e.Graphics.DrawString(TabPages[i].Text, Font, new SolidBrush(_TextColour), textRect, CenterSF);
        }

        // Рисуем горизонтальную линию под вкладками
        e.Graphics.DrawLine(new Pen(_HorizLineColour, 2), new Point(0, 19), new Point(Width, 19));
        e.Graphics.DrawRectangle(new Pen(_BorderColour, 2), new Rectangle(0, 0, Width - 1, Height - 1));
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        Invalidate();  // Перерисовываем при изменении выбранной вкладки
    }
}

public class VisualStudioNormalTextBox : Control
{
    #region Declarations
    private MouseState State = MouseState.None;
    private TextBox TB;
    private Color _BaseColour = Color.FromArgb(51, 51, 55);
    private Color _TextColour = Color.FromArgb(153, 153, 153);
    private Color _BorderColour = Color.FromArgb(35, 35, 35);
    private Styles _Style = Styles.NotRounded;
    private HorizontalAlignment _TextAlign = HorizontalAlignment.Left;
    private int _MaxLength = 32767;
    private bool _ReadOnly;
    private bool _UseSystemPasswordChar;
    private bool _Multiline;
    #endregion

    #region TextBox Properties

    public enum Styles
    {
        Rounded,
        NotRounded
    }

    [Category("Options")]
    public HorizontalAlignment TextAlign
    {
        get { return _TextAlign; }
        set
        {
            _TextAlign = value;
            if (TB != null) TB.TextAlign = value;
        }
    }

    [Category("Options")]
    public int MaxLength
    {
        get { return _MaxLength; }
        set
        {
            _MaxLength = value;
            if (TB != null) TB.MaxLength = value;
        }
    }

    [Category("Options")]
    public bool ReadOnly
    {
        get { return _ReadOnly; }
        set
        {
            _ReadOnly = value;
            if (TB != null) TB.ReadOnly = value;
        }
    }

    [Category("Options")]
    public bool UseSystemPasswordChar
    {
        get { return _UseSystemPasswordChar; }
        set
        {
            _UseSystemPasswordChar = value;
            if (TB != null) TB.UseSystemPasswordChar = value;
        }
    }

    [Category("Options")]
    public bool Multiline
    {
        get { return _Multiline; }
        set
        {
            _Multiline = value;
            if (TB != null)
            {
                TB.Multiline = value;

                if (value)
                {
                    TB.Height = Height - 7;
                }
                else
                {
                    Height = TB.Height + 7;
                }
            }
        }
    }

    [Category("Options")]
    public override string Text
    {
        get { return base.Text; }
        set
        {
            base.Text = value;
            if (TB != null) TB.Text = value;
        }
    }

    [Category("Options")]
    public override Font Font
    {
        get { return base.Font; }
        set
        {
            base.Font = value;
            if (TB != null)
            {
                TB.Font = value;
                TB.Location = new Point(3, 5);
                TB.Width = Width - 6;

                if (!_Multiline)
                {
                    Height = TB.Height + 7;
                }
            }
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (!Controls.Contains(TB))
        {
            Controls.Add(TB);
        }
    }

    private void OnBaseTextChanged(object sender, EventArgs e)
    {
        Text = TB.Text;
    }

    private void OnBaseKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A)
        {
            TB.SelectAll();
            e.SuppressKeyPress = true;
        }
        if (e.Control && e.KeyCode == Keys.C)
        {
            TB.Copy();
            e.SuppressKeyPress = true;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        TB.Location = new Point(5, 5);
        TB.Width = Width - 10;

        if (_Multiline)
        {
            TB.Height = Height - 7;
        }
        else
        {
            Height = TB.Height + 7;
        }

        base.OnResize(e);
    }

    public Styles Style
    {
        get { return _Style; }
        set
        {
            _Style = value;
            Invalidate();
        }
    }

    public void SelectAll()
    {
        TB.Focus();
        TB.SelectAll();
    }

    #endregion

    #region Colour Properties

    [Category("Colours")]
    public Color BackgroundColour
    {
        get { return _BaseColour; }
        set { _BaseColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color TextColour
    {
        get { return _TextColour; }
        set { _TextColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color BorderColour
    {
        get { return _BorderColour; }
        set { _BorderColour = value; Invalidate(); }
    }

    #endregion

    #region Mouse States

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        TB.Focus();
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }

    #endregion

    #region Draw Control

    public VisualStudioNormalTextBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;

        TB = new TextBox()
        {
            Height = 20,
            Font = new Font("Segoe UI", 10),
            BackColor = _BaseColour,
            ForeColor = _TextColour,
            MaxLength = _MaxLength,
            Multiline = false,
            ReadOnly = _ReadOnly,
            UseSystemPasswordChar = _UseSystemPasswordChar,
            BorderStyle = BorderStyle.None,
            Location = new Point(5, 5),
            Width = Width - 35
        };

        TB.TextChanged += OnBaseTextChanged;
        TB.KeyDown += OnBaseKeyDown;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        GraphicsPath GP;
        Rectangle Base = new Rectangle(0, 0, Width, Height);
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(BackColor);

        TB.BackColor = _BaseColour;
        TB.ForeColor = _TextColour;

        // Изменение цвета фона в зависимости от состояния мыши
        Color backgroundColor = _BaseColour;
        Color borderColor = Color.FromArgb(63, 63, 70);

        if (State == MouseState.Down)
        {
            backgroundColor = Color.FromArgb(72, 72, 76); // Пример цвета при нажатии
        }
        else if (State == MouseState.Over)
        {
            backgroundColor = Color.FromArgb(60, 60, 64); // Пример цвета при наведении
        }

        switch (_Style)
        {
            case Styles.Rounded:
                GP = RoundRectangle(Base, 6);
                g.FillPath(new SolidBrush(backgroundColor), GP);
                g.DrawPath(new Pen(borderColor, 2), GP);
                GP.Dispose();
                break;

            case Styles.NotRounded:
                g.FillRectangle(new SolidBrush(backgroundColor), new Rectangle(0, 0, Width - 1, Height - 1));
                g.DrawRectangle(new Pen(borderColor, 2), new Rectangle(0, 0, Width, Height));
                break;
        }
    }


    private GraphicsPath RoundRectangle(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.StartFigure();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }

    #endregion
}

public class VisualStudioGroupBox : ContainerControl
{
    #region Declarations
    private Color _MainColour = Color.FromArgb(37, 37, 38);
    private Color _HeaderColour = Color.FromArgb(45, 45, 48);
    private Color _TextColour = Color.FromArgb(129, 129, 131);
    private Color _BorderColour = Color.FromArgb(2, 118, 196);
    #endregion

    #region Properties

    [Category("Colours")]
    public Color BorderColour
    {
        get { return _BorderColour; }
        set { _BorderColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color TextColour
    {
        get { return _TextColour; }
        set { _TextColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color HeaderColour
    {
        get { return _HeaderColour; }
        set { _HeaderColour = value; Invalidate(); }
    }

    [Category("Colours")]
    public Color MainColour
    {
        get { return _MainColour; }
        set { _MainColour = value; Invalidate(); }
    }

    #endregion

    #region Draw Control
    public VisualStudioGroupBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        Size = new Size(160, 110);
        Font = new Font("Segoe UI", 10, FontStyle.Regular);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        g.FillRectangle(new SolidBrush(_MainColour), new Rectangle(0, 28, Width, Height));
        g.FillRectangle(new SolidBrush(_HeaderColour), new Rectangle(0, 0, Width, 28));
        g.FillRectangle(new SolidBrush(Color.FromArgb(33, 33, 33)), new Rectangle(0, 28, Width, 1));
        g.DrawString(Text, Font, new SolidBrush(_TextColour), new Point(5, 5));
        g.DrawRectangle(new Pen(_BorderColour, 2), new Rectangle(0, 0, Width, Height));
        g.InterpolationMode = InterpolationMode.HighQualityBicubic; // актуальная настройка для повышения качества
    }
    #endregion
}

public class VisualStudioListBoxWBuiltInScrollBar : Control
{
    #region Declarations

    private List<VSListBoxItem> _Items = new List<VSListBoxItem>();
    private readonly List<VSListBoxItem> _SelectedItems = new List<VSListBoxItem>();
    private bool _MultiSelect = true;
    private int ItemHeight = 24;
    private int _ScrollValue = 0; // значение скролла
    private Color _BaseColour = Color.FromArgb(37, 37, 38);
    private Color _NonSelectedItemColour = Color.FromArgb(62, 62, 64);
    private Color _SelectedItemColour = Color.FromArgb(47, 47, 47);
    private Color _BorderColour = Color.FromArgb(35, 35, 35);
    private Color _FontColour = Color.FromArgb(199, 199, 199);
    private int _SelectedWidth = 1;
    private int _SelectedHeight = 1;
    private bool _DontShowInnerScrollbarBorder = false;
    private bool _ShowWholeInnerBorder = true;

    #endregion

    #region Properties

    [Category("Colours")]
    public Color FontColour
    {
        get => _FontColour;
        set => _FontColour = value;
    }

    [Category("Colours")]
    public Color BaseColour
    {
        get => _BaseColour;
        set => _BaseColour = value;
    }

    [Category("Colours")]
    public Color SelectedItemColour
    {
        get => _SelectedItemColour;
        set => _SelectedItemColour = value;
    }

    [Category("Colours")]
    public Color NonSelectedItemColour
    {
        get => _NonSelectedItemColour;
        set => _NonSelectedItemColour = value;
    }

    [Category("Colours")]
    public Color BorderColour
    {
        get => _BorderColour;
        set => _BorderColour = value;
    }

    [Category("Control")]
    public int SelectedHeight => _SelectedHeight;

    [Category("Control")]
    public int SelectedWidth => _SelectedWidth;

    [Category("Control")]
    public bool DontShowInnerScrollbarBorder
    {
        get => _DontShowInnerScrollbarBorder;
        set
        {
            _DontShowInnerScrollbarBorder = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public bool ShowWholeInnerBorder
    {
        get => _ShowWholeInnerBorder;
        set
        {
            _ShowWholeInnerBorder = value;
            Invalidate();
        }
    }

    [Category("Control")]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public VSListBoxItem[] Items
    {
        get => _Items.ToArray();
        set
        {
            _Items = new List<VSListBoxItem>(value);
            Invalidate();
            InvalidateScroll();
        }
    }

    [Category("Control")]
    public VSListBoxItem[] SelectedItems => _SelectedItems.ToArray();

    [Category("Control")]
    public bool MultiSelect
    {
        get => _MultiSelect;
        set
        {
            _MultiSelect = value;
            if (_SelectedItems.Count > 1)
                _SelectedItems.RemoveRange(1, _SelectedItems.Count - 1);
            Invalidate();
        }
    }

    #endregion

    public VisualStudioListBoxWBuiltInScrollBar()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.UserPaint |
                 ControlStyles.Selectable | ControlStyles.SupportsTransparentBackColor, true);

        DoubleBuffered = true;
        InvalidateLayout();
    }

    private void InvalidateScroll()
    {
        Invalidate();
    }

    private void InvalidateLayout()
    {
        Invalidate();
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        _SelectedWidth = Width;
        _SelectedHeight = Height;
        InvalidateLayout();
        base.OnSizeChanged(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Focus();
        if (e.Button == MouseButtons.Left)
        {
            int offset = _ScrollValue * (Height / ItemHeight);
            int index = (e.Y + offset) / ItemHeight;
            if (index < 0 || index >= _Items.Count) return;

            if (ModifierKeys == Keys.Control && _MultiSelect)
            {
                if (_SelectedItems.Contains(_Items[index]))
                    _SelectedItems.Remove(_Items[index]);
                else
                    _SelectedItems.Add(_Items[index]);
            }
            else
            {
                _SelectedItems.Clear();
                _SelectedItems.Add(_Items[index]);
            }

            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        int move = -(e.Delta * SystemInformation.MouseWheelScrollLines / 120);
        _ScrollValue = Math.Max(0, Math.Min(_ScrollValue + move, CalculateMaxScroll()));
        Invalidate();
        base.OnMouseWheel(e);
    }

    private int CalculateMaxScroll()
    {
        return Math.Max(0, (_Items.Count * ItemHeight) - Height) / ItemHeight;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.Clear(_BaseColour);

        int offset = _ScrollValue * ItemHeight;
        int startIndex = Math.Max(0, offset / ItemHeight);
        int endIndex = Math.Min(startIndex + Height / ItemHeight, _Items.Count - 1);

        if (!_DontShowInnerScrollbarBorder && !_ShowWholeInnerBorder)
        {
            g.DrawLine(new Pen(_BorderColour, 2), Width - 2, 0, Width - 2, Height);
        }

        for (int i = startIndex; i <= endIndex; i++)
        {
            var item = _Items[i];
            Rectangle itemRect = new Rectangle(0, (i * ItemHeight) - offset, Width - 19, ItemHeight - 1);

            g.FillRectangle(new SolidBrush(_SelectedItems.Contains(item) ? _SelectedItemColour : _NonSelectedItemColour), itemRect);
            g.DrawString(item.Text, new Font("Segoe UI", 8), new SolidBrush(_FontColour), new PointF(7, itemRect.Y + 2));
        }

        g.DrawRectangle(new Pen(Color.FromArgb(35, 35, 35), 2), 1, 1, Width - 2, Height - 2);

        if (_ShowWholeInnerBorder)
        {
            g.DrawLine(new Pen(_BorderColour, 2), Width - 2, 0, Width - 2, Height);
        }
    }

    public void AddItem(string itemText)
    {
        _Items.Add(new VSListBoxItem { Text = itemText });
        Invalidate();
    }

    public void AddItems(string[] items)
    {
        foreach (var item in items)
        {
            _Items.Add(new VSListBoxItem { Text = item });
        }
        Invalidate();
    }

    public void RemoveItemAt(int index)
    {
        if (index >= 0 && index < _Items.Count)
        {
            _Items.RemoveAt(index);
            Invalidate();
        }
    }

    public void RemoveItem(VSListBoxItem item)
    {
        _Items.Remove(item);
        Invalidate();
    }

    public void RemoveItems(VSListBoxItem[] items)
    {
        foreach (var item in items)
        {
            _Items.Remove(item);
        }
        Invalidate();
    }

    public class VSListBoxItem
    {
        public string Text { get; set; }

        public override string ToString() => Text;
    }
}