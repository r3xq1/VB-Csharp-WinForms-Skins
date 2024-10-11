using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public enum MouseState : byte
{
    MouseNone = 0,
    MouseOver = 1,
    MouseDown = 2
}

public abstract class ThemeControl : Control
{
    protected Graphics G;
    private Bitmap B;

    protected MouseState CurrentMouseState;

    public ThemeControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
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
        ChangeMouseState(MouseState.MouseNone);
        base.OnMouseLeave(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        ChangeMouseState(MouseState.MouseOver);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        ChangeMouseState(MouseState.MouseOver);
        base.OnMouseUp(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            ChangeMouseState(MouseState.MouseDown);
        base.OnMouseDown(e);
    }

    private void ChangeMouseState(MouseState state)
    {
        CurrentMouseState = state;
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

    public int ImageTop => _Image != null ? Height / 2 - _Image.Height / 2 : 0;

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
    private Bitmap B;

    public ThemeContainerControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
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

public abstract class Theme : ContainerControl
{
    protected Graphics G;
    private bool ParentIsForm;
    protected Rectangle Header;

    public Theme()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
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

        // Использование правильного сообщения с lParam в IntPtr.Zero
        Message message = Message.Create(Parent.Handle, 0xA1, Flag, IntPtr.Zero);
        DefWndProc(ref message); // используется ref для передачи сообщения

        base.OnMouseDown(e);
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

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        var rect = new Rectangle(x, y, width, height);
        using (_Gradient = new LinearGradientBrush(rect, c1, c2, angle))
        {
            G.FillRectangle(_Gradient, rect);
        }
    }
}
public class Thief3Theme : Theme
{
    private bool _bStripes;
    public bool HatchEnable
    {
        get => _bStripes;
        set
        {
            _bStripes = value;
            Invalidate();
        }
    }

    private Color _aColor;
    public Color AccentColor
    {
        get => _aColor;
        set
        {
            _aColor = value;
            Invalidate();
        }
    }

    private bool _cStyle;
    public bool DarkTheme
    {
        get => _cStyle;
        set
        {
            _cStyle = value;
            Invalidate();
        }
    }

    private HorizontalAlignment _tAlign;
    public HorizontalAlignment TitleTextAlign
    {
        get => _tAlign;
        set
        {
            _tAlign = value;
            Invalidate();
        }
    }

    public Thief3Theme()
    {
        MoveHeight = 19;
        TransparencyKey = Color.Transparent; // Убираем фуксию
        Resizable = false;
        Font = new Font("Microsoft Sans Serif", 9, FontStyle.Regular);
        // Custom Settings
        HatchEnable = true;
        AccentColor = Color.DodgerBlue;
        DarkTheme = true;
        TitleTextAlign = HorizontalAlignment.Left;
    }

    public override void PaintHook()
    {
        int ClientPtA, ClientPtB, GradA, GradB;
        Pen PenColor;

        // Set gradient and off-screen points based on hatch enable and theme
        if (HatchEnable)
        {
            ClientPtA = 38;
            ClientPtB = 37;
        }
        else
        {
            ClientPtA = 21;
            ClientPtB = -1;
        }

        if (DarkTheme)
        {
            GradA = 51;
            GradB = 30;
            PenColor = Pens.Black;
        }
        else
        {
            GradA = 200;
            GradB = 160;
            PenColor = Pens.DimGray;
        }

        // Clear the background with a solid color instead of using transparency
        G.Clear(Color.FromArgb(GradA, GradA, GradA));
        DrawGradient(Color.FromArgb(GradA, GradA, GradA), Color.FromArgb(GradB, GradB, GradB), 0, 0, Width, 19, 90f);

        if (HatchEnable)
        {
            DrawGradient(Color.FromArgb(GradB, GradB, GradB), Color.FromArgb(GradA, GradA, GradA), 0, 19, Width, 18, 90f);
        }

        // Draw lines based on calculated points
        G.DrawLine(PenColor, 0, 20, Width, 20);
        G.DrawLine(PenColor, 0, ClientPtB, Width, ClientPtB);

        if (HatchEnable)
        {
            for (int I = 0; I <= Width + 17; I += 4)
            {
                G.DrawLine(PenColor, I, 21, I - 17, ClientPtA);
                G.DrawLine(PenColor, I - 1, 21, I - 18, ClientPtA);
            }
        }

        // Draw accent lines
        using (var accentPen = new Pen(AccentColor))
        {
            G.DrawLine(accentPen, 0, ClientPtA, Width, ClientPtA);
            G.DrawLine(accentPen, 0, Height - 2, Width, Height - 2);
            G.DrawLine(accentPen, 1, ClientPtA, 1, Height - 2);
            G.DrawLine(accentPen, Width - 2, ClientPtA, Width - 2, Height - 1);
        }

        // Draw text indicators
        G.DrawString(".", Parent.Font, Brushes.Black, -2, Height - 12);
        G.DrawString(".", Parent.Font, Brushes.Black, Width - 5, Height - 12);

        // Draw the title
        DrawText(TitleTextAlign, AccentColor, 6, 0);
        DrawBorders(Pens.Black, Pens.Transparent, ClientRectangle);
        DrawCorners(Color.Transparent, ClientRectangle); // Убираем розовые углы
    }
}
public class Thief3TopButton : ThemeControl
{
    private Color _aColor;
    public Color AccentColor
    {
        get => _aColor;
        set
        {
            _aColor = value;
            Invalidate();
        }
    }

    private bool _cStyle;
    public bool DarkTheme
    {
        get => _cStyle;
        set
        {
            _cStyle = value;
            Invalidate();
        }
    }

    public Thief3TopButton()
    {
        Size = new Size(11, 5);
        AccentColor = Color.DodgerBlue;
        DarkTheme = true;
    }

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(102, 102, 102));
        int GradA, GradB, GradC;

        // Установка значений градиента на основе темы.
        if (DarkTheme)
        {
            GradA = 61;
            GradB = 49;
            GradC = 51;
        }
        else
        {
            GradA = 200;
            GradB = 155;
            GradC = 155;
        }

        // Рисование фона градиента в зависимости от состояния мыши.
        switch (CurrentMouseState)
        {
            case MouseState.MouseNone:
            case MouseState.MouseOver:
                DrawGradient(Color.FromArgb(GradA, GradA, GradA), Color.FromArgb(GradB, GradB, GradB), 0, 0, Width, Height, 90f);
                break;
            case MouseState.MouseDown:
                DrawGradient(Color.FromArgb(GradB, GradB, GradB), Color.FromArgb(GradA, GradA, GradA), 0, 0, Width, Height, 90f);
                break;
        }

        // Рисование границ и углов.
        DrawBorders(new Pen(AccentColor), Pens.Transparent, ClientRectangle);
        DrawCorners(Color.FromArgb(GradC, GradC, GradC), ClientRectangle);
    }

    protected new MouseState CurrentMouseState;

    protected override void OnMouseEnter(EventArgs e)
    {
        CurrentMouseState = MouseState.MouseOver;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        CurrentMouseState = MouseState.MouseNone;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            CurrentMouseState = MouseState.MouseDown;
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            CurrentMouseState = MouseState.MouseOver;
            Invalidate();
        }
        base.OnMouseUp(e);
    }
}
public class Thief3Button : ThemeControl
{
    private Color _aColor;

    public Color AccentColor
    {
        get => _aColor;
        set
        {
            _aColor = value;
            Invalidate();
        }
    }

    private bool _cStyle;

    public bool DarkTheme
    {
        get => _cStyle;
        set
        {
            _cStyle = value;
            BackColor = _cStyle ? Color.FromArgb(51, 51, 51) : Color.FromArgb(200, 200, 200);
            Invalidate();
        }
    }

    public Thief3Button()
    {
        DarkTheme = true; // Установка начальной темы
        AccentColor = Color.DodgerBlue;
    }

    public override void PaintHook()
    {
        int GradA, GradB;
        Pen penColor;

        // Определение градиента и цвета пера на основе темы
        if (DarkTheme)
        {
            GradA = 61;
            GradB = 49;
            penColor = Pens.DimGray;
        }
        else
        {
            GradA = 200;
            GradB = 155;
            penColor = Pens.White;
        }

        G.Clear(Color.FromArgb(102, 102, 102));

        // Рисование фона градиента в зависимости от состояния мыши
        switch (CurrentMouseState) // Изменение здесь на CurrentMouseState
        {
            case MouseState.MouseNone:
            case MouseState.MouseOver:
                DrawGradient(Color.FromArgb(GradA, GradA, GradA), Color.FromArgb(GradB, GradB, GradB), 0, 0, Width, Height, 90f);
                G.DrawLine(penColor, 1, 1, Width - 1, 1);
                break;

            case MouseState.MouseDown:
                DrawGradient(Color.FromArgb(GradB, GradB, GradB), Color.FromArgb(GradA, GradA, GradA), 0, 0, Width, Height, 90f);
                G.DrawLine(penColor, 1, Height - 2, Width - 1, Height - 2);
                break;
        }

        // Рисование границ и текста
        DrawBorders(Pens.Black, Pens.Transparent, ClientRectangle);
        DrawText(HorizontalAlignment.Center, AccentColor, -1, 0);
    }

    // Добавьте метод для обновления CurrentMouseState
    protected new MouseState CurrentMouseState;

    protected override void OnMouseEnter(EventArgs e)
    {
        CurrentMouseState = MouseState.MouseOver;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        CurrentMouseState = MouseState.MouseNone;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            CurrentMouseState = MouseState.MouseDown;
            Invalidate();
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            CurrentMouseState = MouseState.MouseOver;
            Invalidate();
        }
        base.OnMouseUp(e);
    }
}
public class Thief3Check : ThemeControl
{
    private bool _cStyle;

    public bool DarkTheme
    {
        get => _cStyle;
        set
        {
            _cStyle = value;
            Invalidate();
        }
    }

    private bool _checkedState;

    public bool CheckedState
    {
        get => _checkedState;
        set
        {
            _checkedState = value;
            Invalidate();
        }
    }

    public Thief3Check()
    {
        Size = new Size(90, 15);
        MinimumSize = new Size(16, 16);
        MaximumSize = new Size(600, 16);

        DarkTheme = true;
        CheckedState = false;
    }

    public override void PaintHook()
    {
        int grad;
        Color fontColor;

        // Установка градиента и цвета текста на основе темы
        if (DarkTheme)
        {
            grad = 51;
            fontColor = Color.White;
        }
        else
        {
            grad = 200;
            fontColor = Color.Black;
        }

        G.Clear(Color.FromArgb(grad, grad, grad));

        // Рисование градиента в зависимости от состояния чекбокса
        if (CheckedState)
        {
            DrawGradient(Color.FromArgb(132, 192, 240), Color.FromArgb(78, 123, 168), 3, 3, 9, 9, 90f);
            DrawGradient(Color.FromArgb(98, 159, 220), Color.FromArgb(62, 102, 147), 4, 4, 7, 7, 90f);
        }
        else
        {
            DrawGradient(Color.FromArgb(80, 80, 80), Color.FromArgb(80, 80, 80), 0, 0, 15, 15, 90f);
        }

        // Рисование рамок
        G.DrawRectangle(Pens.Black, 0, 0, 14, 14);
        G.DrawRectangle(Pens.DimGray, 1, 1, 12, 12);

        // Рисование текста
        DrawText(HorizontalAlignment.Left, fontColor, 17, 0);
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        // Переключение состояния чекбокса при клике
        CheckedState = !CheckedState;
    }
}
public class Thief3ProgressBar : ThemeControl
{
    private int _maximum = 100;
    public int Maximum
    {
        get => _maximum;
        set
        {
            if (value < _value)
                _value = value;
            _maximum = value;
            Invalidate();
        }
    }

    private int _value;
    public int Value
    {
        get => _value == 0 ? 1 : _value;
        set
        {
            if (value > _maximum)
                value = _maximum;
            _value = value;
            Invalidate();
        }
    }

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(51, 51, 51));

        // Заливка
        if (_value > 2)
        {
            DrawGradient(Color.FromArgb(132, 192, 240), Color.FromArgb(78, 123, 168), 3, 3, (int)(_value / (float)_maximum * Width) - 6, Height - 6, 90f);
            DrawGradient(Color.FromArgb(98, 159, 220), Color.FromArgb(62, 102, 147), 4, 4, (int)(_value / (float)_maximum * Width) - 8, Height - 8, 90f);
        }
        else if (_value > 0)
        {
            DrawGradient(Color.FromArgb(132, 192, 240), Color.FromArgb(78, 123, 168), 3, 3, (int)(_value / (float)_maximum * Width), Height - 6, 90f);
            DrawGradient(Color.FromArgb(98, 159, 220), Color.FromArgb(62, 102, 147), 4, 4, (int)(_value / (float)_maximum * Width) - 2, Height - 8, 90f);
        }

        // Рамки
        G.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
        G.DrawRectangle(Pens.Gray, 1, 1, Width - 3, Height - 3);
    }
}
