using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public abstract class Theme : ContainerControl
{
    protected Graphics G;
    private bool ParentIsForm;
    private Color _TransparencyKey;
    private bool _Resizable = true;
    private int _MoveHeight = 24;
    protected Rectangle Header;

    public Theme()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
    }

    protected override void OnHandleCreated(EventArgs e)
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

    public bool Resizable
    {
        get => _Resizable;
        set => _Resizable = value;
    }

    public int MoveHeight
    {
        get => _MoveHeight;
        set
        {
            _MoveHeight = value;
            Header = new Rectangle(7, 7, Width - 14, _MoveHeight - 7);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;
        if (ParentIsForm && ParentForm.WindowState == FormWindowState.Maximized) return;

        IntPtr flag;
        if (Header.Contains(e.Location))
        {
            flag = new IntPtr(2);
        }
        else if (Current.Position == 0 || !_Resizable)
        {
            return;
        }
        else
        {
            flag = new IntPtr(Current.Position);
        }

        Capture = false;

        // Здесь передаем Message по ссылке
        Message msg = Message.Create(Parent.Handle, 161, flag, IntPtr.Zero);
        DefWndProc(ref msg); // Передаем ссылку на msg

        base.OnMouseDown(e);
    }


    private Pointer GetPointer()
    {
        Point ptc = PointToClient(MousePosition);
        bool f1 = ptc.X < 7;
        bool f2 = ptc.X > Width - 7;
        bool f3 = ptc.Y < 7;
        bool f4 = ptc.Y > Height - 7;

        if (f1 && f3) return new Pointer(Cursors.SizeNWSE, 13);
        if (f1 && f4) return new Pointer(Cursors.SizeNESW, 16);
        if (f2 && f3) return new Pointer(Cursors.SizeNESW, 14);
        if (f2 && f4) return new Pointer(Cursors.SizeNWSE, 17);
        if (f1) return new Pointer(Cursors.SizeWE, 10);
        if (f2) return new Pointer(Cursors.SizeWE, 11);
        if (f3) return new Pointer(Cursors.SizeNS, 12);
        if (f4) return new Pointer(Cursors.SizeNS, 15);
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
        using (var gradient = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle))
        {
            G.FillRectangle(gradient, x, y, width, height);
        }
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
}

public abstract class ThemeControl : Control
{
    protected Graphics G;
    protected Bitmap B;
    protected enum State : byte
    {
        MouseNone = 0,
        MouseOver = 1,
        MouseDown = 2
    }

    protected State MouseState;

    public ThemeControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
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
        if (e.Button == MouseButtons.Left)
            ChangeMouseState(State.MouseDown);
        base.OnMouseDown(e);
    }

    private void ChangeMouseState(State state)
    {
        MouseState = state;
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
        using (var gradient = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle))
        {
            G.FillRectangle(gradient, x, y, width, height);
        }
    }

    protected void DrawText(HorizontalAlignment alignment, Color color, int offsetX)
    {
        if (string.IsNullOrEmpty(Text)) return;

        using (var brush = new SolidBrush(color))
        {
            SizeF textSize = G.MeasureString(Text, Font);
            float x = 0;

            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    x = offsetX;
                    break;
                case HorizontalAlignment.Center:
                    x = (Width - textSize.Width) / 2 + offsetX;
                    break;
                case HorizontalAlignment.Right:
                    x = Width - textSize.Width - offsetX;
                    break;
            }

            G.DrawString(Text, Font, brush, x, (Height - textSize.Height) / 2);
        }
    }
}

public class YTTheme : Theme
{
    public YTTheme()
    {
        Resizable = false;
        Font = new Font("Verdana", 8.25F);
        ForeColor = Color.White;
        BackColor = Color.White;
        TransparencyKey = Color.Fuchsia;
        MoveHeight = 20;
    }

    public override void PaintHook()
    {
        // Очищаем фон
        G.Clear(BackColor);

        // Верхняя часть
        DrawGradient(Color.FromArgb(175, 0, 0), Color.FromArgb(220, 0, 0), 0, 0, Width, MoveHeight, 90F);

        // Основная область
        DrawGradient(Color.White, Color.LightGray, 0, MoveHeight, Width, Height - MoveHeight - 25, 90F);

        // Нижняя часть
        DrawGradient(Color.FromArgb(175, 0, 0), Color.FromArgb(220, 0, 0), 0, Height - 25, Width, 25, 90F);

        // Левый боковой градиент
        DrawGradient(Color.FromArgb(175, 0, 0), Color.FromArgb(220, 0, 0), 0, MoveHeight, 10, Height - MoveHeight - 25, 90F);

        // Правый боковой градиент
        DrawGradient(Color.FromArgb(175, 0, 0), Color.FromArgb(220, 0, 0), Width - 10, MoveHeight, 10, Height - MoveHeight - 25, 90F);

        // Боковые градиенты для нижней части
        DrawGradient(Color.FromArgb(175, 0, 0), Color.DarkRed, 0, Height - 45, 10, 45, 180F); // Левый нижний градиент
        DrawGradient(Color.Red, Color.FromArgb(175, 0, 0), Width - 10, Height - 45, 10, 45, 180F); // Правый нижний градиент

        // Рисуем углы
        DrawCorners(Color.Fuchsia, ClientRectangle);

        // Рисуем текст
        DrawText(HorizontalAlignment.Center, ForeColor, ImageWidth);

        // Рисуем границы
        DrawBorders(Pens.DarkRed, Pens.White, ClientRectangle);
    }
}


public class YTButton : ThemeControl
{
    public override void PaintHook()
    {
        if (MouseState == State.MouseDown)
        {
            DrawGradient(Color.Red, Color.DarkRed, 0, 0, Width, Height, 90F);
        }
        else if (MouseState == State.MouseOver)
        {
            DrawGradient(Color.DarkRed, Color.Red, 0, 0, Width, Height, 90F);
        }
        else
        {
            DrawGradient(Color.Red, Color.DarkRed, 0, 0, Width, Height, 90F);
        }

        DrawText(HorizontalAlignment.Center, ForeColor, 0); // Убедитесь, что ForeColor доступен
        DrawBorders(Pens.Red, Pens.White, ClientRectangle);
    }
}

public class YTProgressBar : ThemeControl
{
    private int _Maximum = 100;
    private int _Value = 50;

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

    public int Value
    {
        get => _Value;
        set
        {
            if (value > _Maximum) value = _Maximum;
            if (value < 1) value = 1;
            _Value = value;
            Invalidate();
        }
    }

    public override void PaintHook()
    {
        G.Clear(Color.DarkRed);
        if (_Value > 0)
        {
            DrawGradient(Color.Red, Color.DarkRed, 0, 0, (int)((_Value / (float)_Maximum) * Width), Height, 90F);
        }
        G.DrawRectangle(Pens.White, 0, 0, Width - 1, Height - 1);
        DrawBorders(Pens.DarkRed, Pens.White, ClientRectangle);
    }
}
