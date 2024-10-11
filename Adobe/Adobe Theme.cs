using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public abstract class Theme : ContainerControl
{
    #region "Initialization"

    protected Graphics G;

    public Theme()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }

    private bool ParentIsForm;

    protected override void OnHandleCreated(EventArgs e)
    {
        Dock = DockStyle.Fill;
        ParentIsForm = Parent is Form;
        if (ParentIsForm)
        {
            if (_TransparencyKey != Color.Empty) ParentForm.TransparencyKey = _TransparencyKey;
            ParentForm.FormBorderStyle = FormBorderStyle.FixedSingle;
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

    #endregion

    #region "Sizing and Movement"

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
        // Создаем сообщение для передачи в DefWndProc
        Message msg = Message.Create(Parent.Handle, 0xA1 /* WM_NCLBUTTONDOWN */, Flag, IntPtr.Zero);
        DefWndProc(ref msg); // Передаем сообщение по ссылке

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

    private Pointer Current, Pending;
    private Point PTC;

    private Pointer GetPointer()
    {
        PTC = PointToClient(MousePosition);
        bool F1 = PTC.X < 7,
             F2 = PTC.X > Width - 7,
             F3 = PTC.Y < 7,
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
        Current = GetPointer();
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

    #endregion

    #region "Convenience"

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

    public int ImageWidth
    {
        get => _Image?.Width ?? 0;
    }

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

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y, Font f)
    {
        if (string.IsNullOrEmpty(Text)) return;
        _Size = G.MeasureString(Text, Font).ToSize();
        _Brush = new SolidBrush(c);

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawString(Text, f, _Brush, x, _MoveHeight / 2 - _Size.Height / 2 + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawString(Text, f, _Brush, Width - _Size.Width - x, _MoveHeight / 2 - _Size.Height / 2 + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawString(Text, f, _Brush, Width / 2 - _Size.Width / 2 + x, _MoveHeight / 2 - _Size.Height / 2 + y);
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

    #endregion
}

public abstract class ThemeControl : Control
{
    #region "Initialization"

    protected Graphics G;
    protected Bitmap B;

    public ThemeControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
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

    #endregion

    #region "Mouse Handling"

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
        if (e.Button == MouseButtons.Left)
            ChangeMouseState(State.MouseDown);
        base.OnMouseDown(e);
    }

    private void ChangeMouseState(State e)
    {
        MouseState = e;
        Invalidate();
    }

    #endregion

    #region "Convenience"

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
    private Rectangle _Rectangle;
    private LinearGradientBrush _Gradient;
    private SolidBrush _Brush;

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
        _Size = G.MeasureString(Text, Font).ToSize();
        _Brush = new SolidBrush(c);

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

    #endregion
}

public abstract class ThemeContainerControl : ContainerControl
{
    #region "Initialization"

    protected Graphics G;
    protected Bitmap B;

    public ThemeContainerControl()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    public void AllowTransparent()
    {
        SetStyle(ControlStyles.Opaque, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    #endregion

    #region "Convenience"

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

    #endregion
}

public class AdobeTheme : Theme
{
    public enum TextAlign
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    private TextAlign TA;

    public TextAlign TextAlignment
    {
        get => TA;
        set
        {
            TA = value;
            Invalidate();
        }
    }

    public AdobeTheme()
    {
        MoveHeight = 19;
        TransparencyKey = Color.Fuchsia;
        TabStop = false;
        Resizable = false;
        BackColor = Color.FromArgb(51, 51, 51);
        Font = new Font("Microsoft Sans Serif", 9, FontStyle.Bold);
        TextAlignment = TextAlign.Left;
    }

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(68, 68, 68));
        DrawGradient(Color.FromArgb(51, 51, 51), Color.FromArgb(51, 51, 51), 0, 0, Width, 37, 45);
        G.DrawLine(new Pen(Color.FromArgb(31, 31, 31)), 0, 37, Width, 37);
        G.DrawLine(new Pen(Color.FromArgb(60, 60, 60)), 0, 38, Width, 38);
        G.FillRectangle(new SolidBrush(Color.FromArgb(68, 68, 68)), 1, 39, Width - 2, Height - 39 - 2);

        switch (TA)
        {
            case TextAlign.Left:
                DrawText(HorizontalAlignment.Left, Color.FromArgb(120, Color.Black), 12, 18, new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
                DrawText(HorizontalAlignment.Left, Color.White, 10, 16, new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
                break;

            case TextAlign.Center:
                DrawText(HorizontalAlignment.Center, Color.FromArgb(120, Color.Black), -1, 18, new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
                DrawText(HorizontalAlignment.Center, Color.White, 1, 16, new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
                break;

            case TextAlign.Right:
                DrawText(HorizontalAlignment.Right, Color.FromArgb(120, Color.Black), -1 + (int)(G.MeasureString(Text, Font).Width / 6), 18, new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
                DrawText(HorizontalAlignment.Right, Color.White, 1 + (int)(G.MeasureString(Text, Font).Width / 6), 16, new Font("Microsoft Sans Serif", 10, FontStyle.Bold));
                break;
        }

        G.FillRectangle(new SolidBrush(Color.FromArgb(51, 51, 51)), 1, Height - 37, Width - 2, Height - 2);
        G.DrawLine(new Pen(Color.FromArgb(31, 31, 31)), 0, Height - 37, Width, Height - 37);
        G.DrawLine(new Pen(Color.FromArgb(60, 60, 60)), 0, Height - 38, Width, Height - 38);

        DrawBorders(Pens.Black, Pens.Gray, ClientRectangle);
        DrawCorners(Color.Fuchsia, ClientRectangle);
    }
}

public class AdobeButton : ThemeControl
{
    #region "Properties"

    private bool _Orange;
    public bool Orange
    {
        get => _Orange;
        set
        {
            _Orange = value;
            Invalidate();
        }
    }

    #endregion

    public AdobeButton()
    {
        TabStop = false;
        BackColor = Color.FromArgb(51, 51, 51);
    }

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(102, 102, 102));
        Color textColor = GetTextColor(out int gC);

        Color[] colors = _Orange
            ?
            [
                Color.FromArgb(255, 209, 51),
                Color.FromArgb(255, 165, 13),
                Color.FromArgb(255, 195, 13),
                Color.FromArgb(255, 163, 0)
            ]
            :
            [
                Color.FromArgb(105, 105, 105),
                Color.FromArgb(56, 56, 56),
                Color.FromArgb(73, 73, 73),
                Color.FromArgb(48, 48, 48)
            ];

        DrawGradient(colors[0], colors[1], 0, 0, Width, Height, 90);
        DrawGradient(colors[2], colors[3], 1, 1, Width - 2, Height - 2, 90);

        DrawBorders(Pens.Black, Pens.Transparent, ClientRectangle);
        DrawText(HorizontalAlignment.Center, textColor, -2, 0);

        DrawBordersWithGradation(gC);
    }

    private Color GetTextColor(out int gC)
    {
        gC = 15;

        switch (MouseState)
        {
            case State.MouseOver:
                gC = _Orange ? 10 : gC;
                return _Orange ? Color.Black : Color.White;

            case State.MouseDown:
                gC = _Orange ? 5 : gC;
                return _Orange ? Color.White : Color.White;

            default: // State.MouseNone
                return Color.White;
        }
    }

    private void DrawBordersWithGradation(int gC)
    {
        for (int i = 1; i <= 5; i++)
        {
            using (var pen = new Pen(Color.FromArgb(255 / (i * gC), Color.Black)))
            {
                G.DrawRectangle(pen, new Rectangle(i, i, Width - 1 - (i * 2), Height - 1 - (i * 2)));
            }
        }
    }
}

public class AdobeCheck : ThemeControl
{
    #region "Properties"

    private bool _CheckedState;
    public bool CheckedState
    {
        get => _CheckedState;
        set
        {
            if (_CheckedState != value)
            {
                _CheckedState = value;
                Invalidate();
            }
        }
    }

    private string _Text; // Хранит текст чекбокса
    public override string Text // Переопределенное свойство
    {
        get => _Text;
        set
        {
            if (_Text != value)
            {
                _Text = value;
                UpdateSize(); // Обновить размер при изменении текста
            }
        }
    }

    #endregion

    public AdobeCheck()
    {
        CheckedState = false;
        TabStop = false;
        Size = new Size(25, 20); // Задаем размеры для чекбокса
        UpdateSize(); // Вызываем обновление размера
    }

    private void UpdateSize()
    {
        if (string.IsNullOrEmpty(_Text)) return;

        using (var g = CreateGraphics())
        {
            SizeF textSize = g.MeasureString(_Text, Font);
            Size = new Size((int)textSize.Width + 35, Math.Max((int)textSize.Height + 5, 20));
        }
    }

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(68, 68, 68));
        DrawCheckBox();
        DrawBorders(Pens.Black, Pens.DimGray, new Rectangle(0, 0, 17, 17)); // Обновлено для правильного размещения границ
        DrawText(HorizontalAlignment.Left, Color.Black, 17, 2); // Сместим текст вниз
        DrawText(HorizontalAlignment.Left, Color.White, 16, 0); // Текст на фоне
    }

    private void DrawCheckBox()
    {
        int x = 1; // Начальная позиция по X
        int y = 1; // Начальная позиция по Y
        Color C1, C2, C3, C4;

        if (CheckedState)
        {
            C1 = Color.FromArgb(62, 62, 62);
            C2 = Color.FromArgb(38, 38, 38);
            DrawGradient(C1, C2, x, y, 15, 15, 90); // Основной цвет
            C3 = Color.FromArgb(132, 192, 240);
            C4 = Color.FromArgb(78, 123, 168);
            DrawGradient(C3, C4, x + 2, y + 2, 11, 11, 90); // Второй цвет
            DrawGradient(Color.FromArgb(98, 159, 220), Color.FromArgb(62, 102, 147), x + 3, y + 3, 9, 9, 90); // Последний цвет
        }
        else
        {
            C1 = Color.FromArgb(80, 80, 80);
            C2 = Color.FromArgb(60, 60, 60);
            DrawGradient(C1, C2, x, y, 15, 15, 90); // Основной цвет для неактивного состояния
        }
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        CheckedState = !CheckedState; // Переключаем состояние
    }
}

public class AdobeProgressBar : ThemeControl
{
    private int _Maximum;
    public int Maximum
    {
        get => _Maximum;
        set
        {
            if (value < _Value)
                _Value = value;
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
            if (value > _Maximum)
                value = _Maximum;
            _Value = value;
            Invalidate();
        }
    }

    public AdobeProgressBar()
    {
        // Default properties can be set here if needed
        TabStop = false;
        Maximum = 100;
        Value = 0;
    }

    public override void PaintHook()
    {
        G.Clear(Color.FromArgb(51, 51, 51));

        // Fill
        if (_Value > 6)
        {
            DrawGradient(Color.FromArgb(132, 192, 240), Color.FromArgb(78, 123, 168), 3, 3, (int)(_Value / (float)_Maximum * Width) - 6, Height - 6, 90);
            DrawGradient(Color.FromArgb(98, 159, 220), Color.FromArgb(62, 102, 147), 4, 4, (int)(_Value / (float)_Maximum * Width) - 8, Height - 8, 90);
        }
        else if (_Value > 1)
        {
            DrawGradient(Color.FromArgb(132, 192, 240), Color.FromArgb(78, 123, 168), 3, 3, (int)(_Value / (float)_Maximum * Width), Height - 6, 90);
            DrawGradient(Color.FromArgb(98, 159, 220), Color.FromArgb(62, 102, 147), 4, 4, (int)(_Value / (float)_Maximum * Width) - 2, Height - 8, 90);
        }

        // Borders
        G.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
        G.DrawRectangle(Pens.Gray, 1, 1, Width - 3, Height - 3);
    }

    public void Increment(int amount)
    {
        if (Value + amount > Maximum)
            Value = Maximum;
        else
            Value += amount;
    }
}
