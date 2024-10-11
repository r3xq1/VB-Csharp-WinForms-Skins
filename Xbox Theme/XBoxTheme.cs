using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public abstract class Theme : ContainerControl
{
    protected Graphics G;

    public Theme()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
    }

    private bool ParentIsForm;

    protected override void OnHandleCreated(System.EventArgs e)
    {
        Dock = DockStyle.Fill;
        ParentIsForm = Parent is Form;
        if (ParentIsForm)
        {
            if (_TransparencyKey != Color.Empty)
                ParentForm.TransparencyKey = _TransparencyKey;
            ParentForm.FormBorderStyle = FormBorderStyle.None;
        }
        base.OnHandleCreated(e);
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    private bool _Resizable = true;
    public bool Resizable
    {
        get => _Resizable;
        set => _Resizable = value;
    }

    private int _MoveHeight = 24;
    public int MoveHeight
    {
        get => _MoveHeight;
        set
        {
            _MoveHeight = value;
            Header = new Rectangle(7, 7, Width - 14, _MoveHeight - 7);
        }
    }

    private IntPtr Flag;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (ParentIsForm && ParentForm.WindowState == FormWindowState.Maximized) return;

        if (Header.Contains(e.Location))
        {
            Flag = new IntPtr(2);
        }
        else if (Current.Position == 0 || !_Resizable)
        {
            return;
        }
        else
        {
            Flag = new IntPtr(Current.Position);
        }

        Capture = false;
        Message msg = Message.Create(Parent.Handle, 161, Flag, IntPtr.Zero);
        DefWndProc(ref msg);

        base.OnMouseDown(e);
    }

    private struct Pointer
    {
        public Cursor Cursor;
        public byte Position;

        public Pointer(Cursor c, byte p)
        {
            Cursor = c;
            Position = p;
        }
    }

    private bool F1, F2, F3, F4;
    private Point PTC;

    private Pointer GetPointer()
    {
        PTC = PointToClient(MousePosition);
        F1 = PTC.X < 7;
        F2 = PTC.X > Width - 7;
        F3 = PTC.Y < 7;
        F4 = PTC.Y > Height - 7;

        if (F1 && F3) return new Pointer(Cursors.SizeNWSE, 13);
        if (F1 && F4) return new Pointer(Cursors.SizeNESW, 16);
        if (F2 && F3) return new Pointer(Cursors.SizeNESW, 14);
        if (F2 && F4) return new Pointer(Cursors.SizeNWSE, 17);
        if (F1) return new Pointer(Cursors.SizeWE, 10);
        if (F2) return new Pointer(Cursors.SizeWE, 11);
        if (F3) return new Pointer(Cursors.SizeNS, 12);
        if (F4) return new Pointer(Cursors.SizeNS, 15);
        return new Pointer(Cursors.Default, 0);
    }

    private Pointer Current, Pending;

    private void SetCurrent()
    {
        Pending = GetPointer();
        if (Current.Position == Pending.Position) return;
        Current = Pending;
        Cursor = Current.Cursor;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_Resizable) SetCurrent();
        base.OnMouseMove(e);
    }

    protected Rectangle Header;

    protected override void OnSizeChanged(EventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        Header = new Rectangle(7, 7, Width - 14, _MoveHeight - 7);
        Invalidate();
        base.OnSizeChanged(e);
    }

    public abstract void PaintHook();

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        G = e.Graphics;
        PaintHook();
    }

    private Color _TransparencyKey;
    public Color TransparencyKey
    {
        get => _TransparencyKey;
        set
        {
            _TransparencyKey = value;
            Invalidate();
        }
    }

    private Image _Image;
    public Image Image
    {
        get => _Image;
        set
        {
            _Image = value;
            Invalidate();
        }
    }

    public int ImageWidth => _Image?.Width ?? 0;

    private Size _Size;
    private Rectangle _Rectangle;
    private LinearGradientBrush _Gradient;
    private SolidBrush _Brush;

    protected void DrawCorners(Color c, Rectangle rect)
    {
        _Brush = new SolidBrush(c);
        G.FillRectangle(_Brush, rect.X, rect.Y, 1, 1);
        G.FillRectangle(_Brush, rect.X + (rect.Width - 1), rect.Y, 1, 1);
        G.FillRectangle(_Brush, rect.X, rect.Y + (rect.Height - 1), 1, 1);
        G.FillRectangle(_Brush, rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), 1, 1);
    }

    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x)
    {
        DrawText(a, c, x, 0);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y)
    {
        if (string.IsNullOrEmpty(Text)) return;
        _Size = TextRenderer.MeasureText(Text, Font);
        _Brush = new SolidBrush(c);

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawString(Text, Font, _Brush, x, _MoveHeight / 2 - _Size.Height / 2 + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawString(Text, Font, _Brush, Width - _Size.Width - x, _MoveHeight / 2 - _Size.Height / 2 + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawString(Text, Font, _Brush, Width / 2 - _Size.Width / 2 + x, _MoveHeight / 2 - _Size.Height / 2 + y);
                break;
        }
    }

    protected void DrawIcon(HorizontalAlignment a, int x)
    {
        DrawIcon(a, x, 0);
    }

    protected void DrawIcon(HorizontalAlignment a, int x, int y)
    {
        if (_Image == null) return;
        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawImage(_Image, x, _MoveHeight / 2 - _Image.Height / 2 + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawImage(_Image, Width - _Image.Width - x, _MoveHeight / 2 - _Image.Height / 2 + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawImage(_Image, Width / 2 - _Image.Width / 2, _MoveHeight / 2 - _Image.Height / 2);
                break;
        }
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        _Rectangle = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(_Rectangle, c1, c2, angle);
        G.FillRectangle(_Gradient, _Rectangle);
    }
}

public abstract class ThemeControl : Control
{
    protected Graphics G;
    protected Bitmap B;

    public ThemeControl()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    protected void AllowTransparent()
    {
        SetStyle(ControlStyles.Opaque, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    protected enum State : byte
    {
        MouseNone = 0,
        MouseOver = 1,
        MouseDown = 2
    }

    protected State MouseState;

    protected override void OnMouseLeave(System.EventArgs e)
    {
        ChangeMouseState(State.MouseNone);
        base.OnMouseLeave(e);
    }

    protected override void OnMouseEnter(System.EventArgs e)
    {
        ChangeMouseState(State.MouseOver);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        ChangeMouseState(State.MouseOver);
        base.OnMouseUp(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            ChangeMouseState(State.MouseDown);
        base.OnMouseDown(e);
    }

    private void ChangeMouseState(State e)
    {
        MouseState = e;
        Invalidate();
    }

    public abstract void PaintHook();

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        PaintHook();
        e.Graphics.DrawImage(B, 0, 0);
    }

    protected override void OnSizeChanged(System.EventArgs e)
    {
        if (Width != 0 && Height != 0)
        {
            B = new Bitmap(Width, Height);
            G = Graphics.FromImage(B);
            Invalidate();
        }
        base.OnSizeChanged(e);
    }

    private bool _NoRounding;

    public bool NoRounding
    {
        get => _NoRounding;
        set
        {
            _NoRounding = value;
            Invalidate();
        }
    }

    private Image _Image;

    public Image Image
    {
        get => _Image;
        set
        {
            _Image = value;
            Invalidate();
        }
    }

    public int ImageWidth => _Image?.Width ?? 0;

    public int ImageTop => _Image == null ? 0 : Height / 2 - _Image.Height / 2;

    private Size _Size;
    private Rectangle _Rectangle;
    private LinearGradientBrush _Gradient;

    protected void DrawCorners(Color c, Rectangle rect)
    {
        if (_NoRounding) return;

        B.SetPixel(rect.X, rect.Y, c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y, c);
        B.SetPixel(rect.X, rect.Y + (rect.Height - 1), c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), c);
    }

    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x)
    {
        DrawText(a, c, x, 0);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y)
    {
        if (string.IsNullOrEmpty(Text)) return;
        _Size = TextRenderer.MeasureText(Text, Font);
        using SolidBrush _Brush = new SolidBrush(c);

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawString(Text, Font, _Brush, x, Height / 2 - _Size.Height / 2 + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawString(Text, Font, _Brush, Width - _Size.Width - x, Height / 2 - _Size.Height / 2 + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawString(Text, Font, _Brush, Width / 2 - _Size.Width / 2 + x, Height / 2 - _Size.Height / 2 + y);
                break;
        }
    }

    protected void DrawIcon(HorizontalAlignment a, int x)
    {
        DrawIcon(a, x, 0);
    }

    protected void DrawIcon(HorizontalAlignment a, int x, int y)
    {
        if (_Image == null) return;
        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawImage(_Image, x, Height / 2 - _Image.Height / 2 + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawImage(_Image, Width - _Image.Width - x, Height / 2 - _Image.Height / 2 + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawImage(_Image, Width / 2 - _Image.Width / 2, Height / 2 - _Image.Height / 2);
                break;
        }
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        _Rectangle = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(_Rectangle, c1, c2, angle);
        G.FillRectangle(_Gradient, _Rectangle);
    }
}

public abstract class ThemeContainerControl : ContainerControl
{
    protected Graphics G;
    protected Bitmap B;

    public ThemeContainerControl()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    protected void AllowTransparent()
    {
        SetStyle(ControlStyles.Opaque, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    public abstract void PaintHook();

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        PaintHook();
        e.Graphics.DrawImage(B, 0, 0);
    }

    protected override void OnSizeChanged(System.EventArgs e)
    {
        if (Width != 0 && Height != 0)
        {
            B = new Bitmap(Width, Height);
            G = Graphics.FromImage(B);
            Invalidate();
        }
        base.OnSizeChanged(e);
    }

    private bool _NoRounding;

    public bool NoRounding
    {
        get => _NoRounding;
        set
        {
            _NoRounding = value;
            Invalidate();
        }
    }

    private Rectangle _Rectangle;
    private LinearGradientBrush _Gradient;

    protected void DrawCorners(Color c, Rectangle rect)
    {
        if (_NoRounding) return;
        B.SetPixel(rect.X, rect.Y, c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y, c);
        B.SetPixel(rect.X, rect.Y + (rect.Height - 1), c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), c);
    }

    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        _Rectangle = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(_Rectangle, c1, c2, angle);
        G.FillRectangle(_Gradient, _Rectangle);
    }
}

public class XboxTheme : Theme
{
    private Pen _Something = Pens.Black;

    [Description("Draws the border to specified color")]
    public Color Something
    {
        get => _Something.Color;
        set
        {
            _Something = new Pen(value);
            Invalidate();
        }
    }

    public void ColorT()
    {
        BackColor = Color.FromArgb(20, 20, 20);
        MoveHeight = 20;
        TransparencyKey = Color.FromArgb(52, 18, 150);
    }

    //private Pen P1, P2, P3, P4;
    //private SolidBrush B1;
    //private LinearGradientBrush B2, B3;

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(20, 20, 20));
        DrawGradient(Color.FromArgb(48, 255, 0), Color.FromArgb(42, 218, 2), 0, 0, Width, 20, 90F);
        DrawGradient(Color.GhostWhite, Color.LightGray, 0, 20, Width, Height - 20, 90F);
        G.DrawLine(Pens.DarkGray, 0, 20, Width, 20);
        DrawGradient(Color.DarkGreen, Color.FromArgb(18, 255, 0), 0, 0, Width, 20, 90F);
        G.DrawLine(Pens.Green, 0, 20, Width, 20);
        DrawBorders(_Something, Pens.DarkGreen, ClientRectangle);
        DrawCorners(Color.DarkGreen, ClientRectangle);
        DrawText(HorizontalAlignment.Center, Color.FromArgb(210, 210, 210), 0);
    }
}

public class XboxButton : ThemeControl
{
    public override void PaintHook()
    {
        switch (MouseState)
        {
            case State.MouseNone:
                G.Clear(Color.LightGray);
                DrawGradient(Color.GhostWhite, Color.LightGray, 0, 0, Width, 20, 90F);
                break;
            case State.MouseOver:
                G.Clear(Color.Orange);
                DrawGradient(Color.FromArgb(0, 255, 36), Color.FromArgb(0, 140, 20), 0, 0, Width, 25, 90F);
                break;
            case State.MouseDown:
                G.Clear(Color.Orange);
                DrawGradient(Color.DarkGreen, Color.FromArgb(18, 255, 0), 0, 0, Width, 30, 90F);
                break;
        }
        DrawText(HorizontalAlignment.Center, Color.FromArgb(190, 190, 190), 0);
        DrawBorders(Pens.DarkGreen, Pens.LightGray, ClientRectangle);
        DrawCorners(Color.DarkGreen, ClientRectangle);
    }
}

public class XboxProgressBar : ThemeControl
{
    private int _Maximum = 100;
    public int Maximum
    {
        get => _Maximum;
        set
        {
            if (value < 1) value = 1;
            if (value < _Value) _Value = value;

            _Maximum = value;
            Invalidate();
        }
    }

    private int _Value;
    public int Value
    {
        get => _Value;
        set
        {
            if (value > _Maximum) value = _Maximum;

            _Value = value;
            Invalidate();
        }
    }

    public override void PaintHook()
    {
        G.Clear(Color.LightGray);
        DrawGradient(Color.GhostWhite, Color.LightGray, 0, 0, Width, 20, 90F);
        G.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
        DrawBorders(Pens.DarkGreen, Pens.LawnGreen, ClientRectangle);
        DrawCorners(Color.Black, ClientRectangle);

        switch (_Value)
        {
            case > 2:
                DrawGradient(Color.DarkGreen, Color.FromArgb(18, 255, 0), 3, 3, (int)(_Value / (float)_Maximum * Width) - 6, Height - 6, 90F);
                DrawGradient(Color.DarkGreen, Color.FromArgb(18, 255, 0), 4, 4, (int)(_Value / (float)_Maximum * Width) - 8, Height - 8, 90F);
                break;
            case > 0:
                DrawGradient(Color.DarkGreen, Color.FromArgb(18, 255, 0), 3, 3, (int)(_Value / (float)_Maximum * Width), Height - 6, 90F);
                DrawGradient(Color.DarkGreen, Color.FromArgb(18, 255, 0), 4, 4, (int)(_Value / (float)_Maximum * Width) - 2, Height - 8, 90F);
                break;
        }
    }
}

public class XboxCheckBox : ThemeControl
{
    private bool _cStyle;
    public bool SWTheme
    {
        get => _cStyle;
        set
        {
            _cStyle = value;
            Invalidate();
        }
    }

    private bool _CheckedState;
    public bool CheckedState
    {
        get => _CheckedState;
        set
        {
            _CheckedState = value;
            Invalidate();
        }
    }

    public XboxCheckBox()
    {
        Size = new Size(90, 15);
        MinimumSize = new Size(16, 16);
        MaximumSize = new Size(600, 16);
        SWTheme = true;
        CheckedState = false;
    }

    public override void PaintHook()
    {
        G.Clear(Color.LightGray);
        DrawGradient(Color.GhostWhite, Color.LightGray, 0, 0, Width, 20, 90F);
        switch (CheckedState)
        {
            case true:
                DrawGradient(Color.FromArgb(0, 255, 36), Color.FromArgb(0, 140, 20), 4, 4, 7, 7, 90F);
                break;
            case false:
                DrawGradient(Color.GhostWhite, Color.LightGray, 0, 0, 15, 15, 90F);
                break;
        }
        G.DrawRectangle(Pens.Green, 1, 1, 12, 12);
        DrawText(HorizontalAlignment.Left, Color.FromArgb(190, 190, 190), 17, 0);
    }

    protected override void OnClick(System.EventArgs e)
    {
        base.OnClick(e);
        CheckedState = !CheckedState;
    }
}

public class XboxSeperator : ThemeControl
{
    private Orientation _Direction;
    public Orientation Direction
    {
        get => _Direction;
        set
        {
            _Direction = value;
            Invalidate();
        }
    }

    public override void PaintHook()
    {
        G.Clear(Color.LightGray);
        DrawGradient(Color.GhostWhite, Color.LightGray, 0, 0, Width, 5, 90F);
        if (_Direction == Orientation.Horizontal)
        {
            G.DrawLine(new Pen(Color.FromArgb(190, 190, 190)), 0, Height / 2, Width, Height / 2);
            G.DrawLine(Pens.LightGray, 0, Height / 2 + 1, Width, Height / 2 + 1);
        }
        else
        {
            G.DrawLine(Pens.LightGray, Width / 2, 0, Width / 2, Height);
            G.DrawLine(new Pen(Color.FromArgb(190, 190, 190)), Width / 2 + 1, 0, Width / 2 + 1, Height);
            DrawGradient(Color.FromArgb(52, 18, 150), Color.FromArgb(32, 32, 32), 0, 0, Width, 10, 90F);
        }
    }
}