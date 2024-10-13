using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Windows.Forms;

public abstract class Theme : ContainerControl
{
    protected Graphics G;
    private bool _Resizable = true;
    private int _MoveHeight = 24;
    private Color _TransparencyKey;
    private Image _Image;
    protected Rectangle Header;
    private IntPtr Flag;

    protected Theme()
    {
        SetStyle((ControlStyles)139270, true);
    }

    protected override void OnHandleCreated(System.EventArgs e)
    {
        Dock = DockStyle.Fill;
        if (Parent is Form parentForm)
        {
            if (_TransparencyKey != Color.Empty) parentForm.TransparencyKey = _TransparencyKey;
            parentForm.FormBorderStyle = FormBorderStyle.None;
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

        if (Header.Contains(e.Location))
        {
            Flag = new IntPtr(2);
        }
        else if (CurrentPointer.Position == 0 || !_Resizable)
        {
            return;
        }
        else
        {
            Flag = new IntPtr(CurrentPointer.Position);
        }

        Capture = false;

        // Используйте IntPtr.Zero, чтобы логика нажатия мыши работала
        Message msg = Message.Create(Parent.Handle, 161, Flag, IntPtr.Zero);
        DefWndProc(ref msg); // Применение ref здесь, следует использовать para ref
        base.OnMouseDown(e);
    }


    private Pointer GetPointer()
    {
        Point PTC = PointToClient(Cursor.Position);
        bool F1 = PTC.X < 7;
        bool F2 = PTC.X > Width - 7;
        bool F3 = PTC.Y < 7;
        bool F4 = PTC.Y > Height - 7;

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

    private Pointer CurrentPointer { get; set; }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_Resizable) SetCurrent();
        base.OnMouseMove(e);
    }

    private void SetCurrent()
    {
        var pending = GetPointer();
        if (CurrentPointer.Position == pending.Position) return;
        CurrentPointer = pending;
        Cursor = CurrentPointer.Cursor;
    }

    protected override void OnSizeChanged(System.EventArgs e)
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

    protected void DrawCorners(Color c, Rectangle rect)
    {
        using (var brush = new SolidBrush(c))
        {
            G.FillRectangle(brush, rect.X, rect.Y, 1, 1);
            G.FillRectangle(brush, rect.X + (rect.Width - 1), rect.Y, 1, 1);
            G.FillRectangle(brush, rect.X, rect.Y + (rect.Height - 1), 1, 1);
            G.FillRectangle(brush, rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), 1, 1);
        }
    }

    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y = 0)
    {
        if (string.IsNullOrEmpty(Text)) return;
        Size size = G.MeasureString(Text, Font).ToSize();
        using (var brush = new SolidBrush(c))
        {
            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawString(Text, Font, brush, x, _MoveHeight / 2 - size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawString(Text, Font, brush, Width - size.Width - x, _MoveHeight / 2 - size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawString(Text, Font, brush, Width / 2 - size.Width / 2 + x, _MoveHeight / 2 - size.Height / 2 + y);
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
        using (var brush = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle))
        {
            G.FillRectangle(brush, new Rectangle(x, y, width, height));
        }
    }

    private struct Pointer
    {
        public Cursor Cursor { get; }
        public byte Position { get; }

        public Pointer(Cursor cursor, byte position)
        {
            Cursor = cursor;
            Position = position;
        }
    }
}
public abstract class ThemeControl : Control
{
    protected Graphics G;
    private Bitmap B;

    protected ThemeControl()
    {
        SetStyle((ControlStyles)139270, true);
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

    public Image Image { get; set; }

    public int ImageWidth => Image?.Width ?? 0;

    public int ImageTop => Image == null ? 0 : Height / 2 - Image.Height / 2;

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
        using (var brush = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle))
        {
            G.FillRectangle(brush, new Rectangle(x, y, width, height));
        }
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y = 0)
    {
        if (string.IsNullOrEmpty(Text)) return;
        Size size = G.MeasureString(Text, Font).ToSize();
        using (var brush = new SolidBrush(c))
        {
            switch (a)
            {
                case HorizontalAlignment.Left:
                    G.DrawString(Text, Font, brush, x, Height / 2 - size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Right:
                    G.DrawString(Text, Font, brush, Width - size.Width - x, Height / 2 - size.Height / 2 + y);
                    break;
                case HorizontalAlignment.Center:
                    G.DrawString(Text, Font, brush, Width / 2 - size.Width / 2 + x, Height / 2 - size.Height / 2 + y);
                    break;
            }
        }
    }

    protected void DrawIcon(HorizontalAlignment a, int x, int y = 0)
    {
        if (Image == null) return;

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawImage(Image, x, Height / 2 - Image.Height / 2 + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawImage(Image, Width - Image.Width - x, Height / 2 - Image.Height / 2 + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawImage(Image, Width / 2 - Image.Width / 2, Height / 2 - Image.Height / 2);
                break;
        }
    }
}

public abstract class ThemeContainerControl : ContainerControl
{
    protected Graphics G;
    private Bitmap B;

    protected ThemeContainerControl()
    {
        SetStyle((ControlStyles)139270, true);
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
        using (var brush = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle))
        {
            G.FillRectangle(brush, new Rectangle(x, y, width, height));
        }
    }
}

// Theme Code
public class BetaBlueTheme : Theme
{
    public BetaBlueTheme()
    {
        BackColor = Color.FromKnownColor(KnownColor.Control);
        MoveHeight = 25;
        TransparencyKey = Color.Fuchsia;
    }

    public override void PaintHook()
    {
        G.Clear(BackColor);
        G.Clear(Color.FromArgb(0, 95, 218));
        DrawGradient(Color.FromArgb(0, 95, 218), Color.FromArgb(0, 55, 202), 0, 0, Width, 25, 90);

        DrawCorners(Color.Fuchsia, ClientRectangle);
        DrawBorders(Pens.DarkBlue, Pens.DodgerBlue, ClientRectangle);
        G.DrawLine(Pens.Black, 0, 25, Width, 25);
        DrawText(HorizontalAlignment.Left, Color.White, 8, 2);
    }
}

public class BetaBlueButton : ThemeControl
{
    private bool _Dark;

    public bool Dark
    {
        get => _Dark;
        set
        {
            _Dark = value;
            Invalidate();
        }
    }

    public override void PaintHook()
    {
        Color gradA, gradB;
        Pen penColor = Pens.DodgerBlue;

        if (Dark)
        {
            gradA = Color.FromArgb(62, 62, 62);
            gradB = Color.FromArgb(38, 38, 38);
            penColor = Pens.DimGray;
        }
        else
        {
            gradA = Color.FromArgb(0, 105, 246);
            gradB = Color.FromArgb(0, 83, 221);
        }

        switch (MouseState)
        {
            case State.MouseNone:
            case State.MouseOver:
                G.Clear(Color.Gray);
                DrawGradient(gradA, gradB, 0, 0, Width, Height, 90);
                break;
            case State.MouseDown:
                G.Clear(Color.DarkGray);
                DrawGradient(gradB, gradA, 0, 0, Width, Height, 90);
                break;
        }

        DrawBorders(Pens.Black, penColor, ClientRectangle);
        DrawCorners(Color.Black, ClientRectangle);
        DrawText(HorizontalAlignment.Center, Color.White, 0);
    }
}

public class BetaBlueSeparator : Control
{
    private Orientation _Orientation;
    private Graphics G;
    private Bitmap B;
    private int I;
    private Color C1;
    private Pen P1;
    private Pen P2;

    public Orientation Orientation
    {
        get => _Orientation;
        set
        {
            _Orientation = value;
            UpdateOffset();
            Invalidate();
        }
    }

    public BetaBlueSeparator()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
        C1 = Color.FromArgb(0, 95, 218);
        P1 = new Pen(Color.FromArgb(55, 55, 55));
        P2 = new Pen(Color.FromArgb(0, 105, 246));
    }

    protected override void OnSizeChanged(System.EventArgs e)
    {
        UpdateOffset();
        base.OnSizeChanged(e);
    }

    private void UpdateOffset()
    {
        I = _Orientation == Orientation.Horizontal ? Height / 2 - 1 : Width / 2 - 1;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        B = new Bitmap(Width, Height);
        G = Graphics.FromImage(B);
        G.Clear(C1);

        if (_Orientation == Orientation.Horizontal)
        {
            G.DrawLine(P1, 0, I, Width, I);
            G.DrawLine(P2, 0, I + 1, Width, I + 1);
        }
        else
        {
            G.DrawLine(P2, I, 0, I, Height);
            G.DrawLine(P1, I + 1, 0, I + 1, Height);
        }

        e.Graphics.DrawImage(B, 0, 0);
        G.Dispose();
        B.Dispose();
    }

    protected override void OnPaintBackground(PaintEventArgs pevent) { }
}
