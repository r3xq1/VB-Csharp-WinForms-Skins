﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

abstract class Theme : ContainerControl
{
    protected Graphics G;
    private bool ParentIsForm;
    private IntPtr Flag;

    public Theme()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        Dock = DockStyle.Fill;
        ParentIsForm = Parent is Form;
        if (ParentIsForm)
        {
            if (_TransparencyKey != Color.Empty) ParentForm.TransparencyKey = _TransparencyKey;
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

    protected Rectangle Header;
    const int WM_LBUTTONDOWN = 0x0201;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (ParentIsForm && ParentForm.WindowState == FormWindowState.Maximized) return;

        if (Header.Contains(e.Location))
            Flag = new IntPtr(2);
        else if (Current.Position == 0 || !_Resizable)
            return;
        else
            Flag = new IntPtr(Current.Position);

        Capture = false;
        Message msg = Message.Create(Parent.Handle, WM_LBUTTONDOWN, Flag, IntPtr.Zero);
        DefWndProc(ref msg); // Передаем по ссылке

        base.OnMouseDown(e);
    }

    private struct Pointer
    {
        public Cursor Cursor { get; }
        public byte Position { get; }

        public Pointer(Cursor c, byte p)
        {
            Cursor = c;
            Position = p;
        }
    }

    private bool F1, F2, F3, F4;
    private Point PTC;
    private Pointer Current, Pending;

    private Pointer GetPointer()
    {
        PTC = PointToClient(Cursor.Position);
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

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y = 0)
    {
        if (string.IsNullOrEmpty(Text)) return;
        _Size = G.MeasureString(Text, Font).ToSize();
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

    protected void DrawIcon(HorizontalAlignment a, int x, int y = 0)
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
        Rectangle rect = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(rect, c1, c2, angle);
        G.FillRectangle(_Gradient, rect);
    }
}

abstract class ThemeControl : Control
{
    protected Graphics G;
    protected Bitmap B;

    public ThemeControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    public void AllowTransparent()
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

    protected override void OnMouseLeave(EventArgs e)
    {
        ChangeMouseState(State.MouseNone);
        base.OnMouseLeave(e);
    }

    protected override void OnMouseEnter(EventArgs e)
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
        if (e.Button == MouseButtons.Left) ChangeMouseState(State.MouseDown);
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

    protected override void OnSizeChanged(EventArgs e)
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

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y = 0)
    {
        if (string.IsNullOrEmpty(Text)) return;
        _Size = G.MeasureString(Text, Font).ToSize();
        using (var brush = new SolidBrush(c))
        {
            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawString(Text, Font, brush, x, Height / 2 - _Size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawString(Text, Font, brush, Width - _Size.Width - x, Height / 2 - _Size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawString(Text, Font, brush, Width / 2 - _Size.Width / 2 + x, Height / 2 - _Size.Height / 2 + y);
                    break;
            }
        }
    }

    protected void DrawIcon(HorizontalAlignment a, int x, int y = 0)
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
        Rectangle rect = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(rect, c1, c2, angle);
        G.FillRectangle(_Gradient, rect);
    }
}

abstract class ThemeContainerControl : ContainerControl
{
    protected Graphics G;
    protected Bitmap B;

    public ThemeContainerControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    public void AllowTransparent()
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

    protected override void OnSizeChanged(EventArgs e)
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

class TLFTheme : Theme
{
    public TLFTheme()
    {
    }

    public override void PaintHook()
    {
        var lineColor = new Pen(Color.FromArgb(98, 142, 179));
        var borderColor1 = new Pen(Color.FromArgb(48, 71, 92));
        var borderColor2 = new Pen(Color.FromArgb(17, 36, 53));

        // Очищаем графику фоном
        G.Clear(Color.FromArgb(33, 52, 69));

        // Рисуем градиент
        DrawGradient(Color.FromArgb(3, 13, 32), Color.FromArgb(14, 28, 41), 0, 0, Width, 30, 90f);

        // Рисуем линию
        G.DrawLine(lineColor, 0, 30, Width, 30);

        // Рисуем второй градиент
        DrawGradient(Color.FromArgb(61, 105, 144), Color.FromArgb(33, 52, 69), 0, 30, Width, 30, 90f);

        // Рисуем границы
        DrawBorders(Pens.LightSteelBlue, Pens.CadetBlue, ClientRectangle);

        // Рисуем текст
        DrawText(HorizontalAlignment.Left, Color.FromArgb(204, 231, 250), 5, 3);
    }
}
class TLFButton : ThemeControl
{
    public TLFButton()
    {
        AllowTransparent();
    }

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(26, 92, 152));
        var bC1 = new Pen(Color.FromArgb(31, 52, 73));
        var bC2 = new Pen(Color.FromArgb(16, 90, 156));
        var bBC = new SolidBrush(Color.FromArgb(14, 66, 112));

        G.FillRectangle(bBC, ClientRectangle);
        DrawText(HorizontalAlignment.Center, Color.FromArgb(7, 38, 81), 1, 1);
        DrawText(HorizontalAlignment.Center, Color.FromArgb(182, 217, 244), 0);
        DrawText(HorizontalAlignment.Center, Color.FromArgb(7, 38, 81), 1, 1);
        DrawText(HorizontalAlignment.Center, Color.FromArgb(182, 217, 244), 0);

        if (MouseState == State.MouseNone)
        {
            DrawGradient(Color.FromArgb(100, 255, 255, 255), Color.FromArgb(50, 255, 255, 255), 0, 0, Width, Height / 2, 90f);
        }
        else if (MouseState == State.MouseDown)
        {
            DrawGradient(Color.FromArgb(75, 255, 255, 255), Color.FromArgb(25, 255, 255, 255), 0, 0, Width, Height / 2, 90f);
        }
        else if (MouseState == State.MouseOver)
        {
            DrawGradient(Color.FromArgb(125, 255, 255, 255), Color.FromArgb(75, 255, 255, 255), 0, 0, Width, Height / 2, 90f);
        }

        DrawBorders(bC1, bC2, ClientRectangle);
    }
}

class TLFProgressBar : ThemeControl
{
    private int _Maximum = 100;
    public int Maximum
    {
        get => _Maximum;
        set
        {
            if (value < 1) value = 1;
            if (value > _Value) _Value = value;

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
        var bC = new Pen(Color.FromArgb(26, 92, 152));
        var ba = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Color.FromArgb(0, 255, 255, 255));
        var pe = new Pen(ba);

        G.Clear(Color.FromArgb(26, 92, 152));
        G.FillRectangle(ba, 0, 0, (int)(_Value / (float)_Maximum * Width), Height);

        DrawText(HorizontalAlignment.Center, Color.FromArgb(7, 38, 81), 1, 1);
        DrawText(HorizontalAlignment.Center, Color.FromArgb(204, 231, 250), 0);

        DrawGradient(Color.FromArgb(100, 255, 255, 255), Color.FromArgb(15, 255, 255, 255), 0, 0, Width, Height / 2, 90f);
        DrawBorders(Pens.Black, bC, ClientRectangle);
    }
}