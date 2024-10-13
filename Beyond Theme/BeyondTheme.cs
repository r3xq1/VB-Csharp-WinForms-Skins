using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

public enum MouseState : byte { None, Over, Down, Block }

public class Bloom
{
    private string _Name; // Имя
    public string Name // Свойство: Имя
    {
        get { return _Name; }
        set { _Name = value; }
    }

    private Color _Value; // Цвет
    public Color Value // Свойство: Цвет
    {
        get { return _Value; }
        set { _Value = value; }
    }

    // Конструктор без параметров
    public Bloom() { }

    // Конструктор с параметрами
    public Bloom(string name, Color value)
    {
        _Name = name;
        _Value = value;
    }
}

// Абстрактный класс ThemeControl151 и ThemeContainer151
public abstract class ThemeControl151 : Control
{
    protected Graphics G;
    protected Bitmap B;

    private Bitmap MeasureBitmap;
    private Graphics MeasureGraphics;

    private Color BackColorWait;
    public MouseState State;

    private bool _NoRounding;
    private Image _Image;
    private Size _ImageSize;
    private int _LockWidth;
    private int _LockHeight;
    private bool _Transparent;
    private string _Customization;
    private Dictionary<string, Color> Items = new Dictionary<string, Color>();

    public ThemeControl151()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);
        _ImageSize = Size.Empty;

        MeasureBitmap = new Bitmap(1, 1);
        MeasureGraphics = Graphics.FromImage(MeasureBitmap);
        Font = new Font("Verdana", 8);

        InvalidateCustomization();
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if (_LockWidth != 0) width = _LockWidth;
        if (_LockHeight != 0) height = _LockHeight;
        base.SetBoundsCore(x, y, width, height, specified);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        if (_Transparent && Width != 0 && Height != 0)
        {
            B = new Bitmap(Width, Height);
            G = Graphics.FromImage(B);
        }

        Invalidate();
        base.OnSizeChanged(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;

        if (_Transparent)
        {
            PaintHook();
            e.Graphics.DrawImage(B, 0, 0);
        }
        else
        {
            G = e.Graphics;
            PaintHook();
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        InvalidateCustomization();
        ColorHook();

        if (_LockWidth != 0) Width = _LockWidth;
        if (_LockHeight != 0) Height = _LockHeight;
        if (BackColorWait != Color.Empty) BackColor = BackColorWait;

        OnCreation();
        base.OnHandleCreated(e);
    }

    protected virtual void OnCreation() { }

    #region State Handling

    protected override void OnMouseEnter(EventArgs e)
    {
        SetState(MouseState.Over);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        SetState(MouseState.Over);
        base.OnMouseUp(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) SetState(MouseState.Down);
        base.OnMouseDown(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        SetState(MouseState.None);
        base.OnMouseLeave(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        SetState(Enabled ? MouseState.None : MouseState.Block);
        base.OnEnabledChanged(e);
    }

    private void SetState(MouseState current)
    {
        State = current;
        Invalidate();
    }

    #endregion

    #region Property Overrides

    public override Color BackColor
    {
        get => base.BackColor;
        set
        {
            if (IsHandleCreated) base.BackColor = value;
            else BackColorWait = value;
        }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public override Color ForeColor
    {
        get => Color.Empty;
        set { }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public override Image BackgroundImage
    {
        get => null;
        set { }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public override ImageLayout BackgroundImageLayout
    {
        get => ImageLayout.None;
        set { }
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

    public override Font Font
    {
        get => base.Font;
        set
        {
            base.Font = value;
            Invalidate();
        }
    }

    #endregion

    #region Properties

    public bool NoRounding
    {
        get => _NoRounding;
        set
        {
            _NoRounding = value;
            Invalidate();
        }
    }

    public Image Image
    {
        get => _Image;
        set
        {
            _ImageSize = value?.Size ?? Size.Empty;
            _Image = value;
            Invalidate();
        }
    }

    public Size ImageSize => _ImageSize;

    public int LockWidth
    {
        get => _LockWidth;
        set
        {
            _LockWidth = value;
            if (_LockWidth != 0 && IsHandleCreated) Width = _LockWidth;
        }
    }

    public int LockHeight
    {
        get => _LockHeight;
        set
        {
            _LockHeight = value;
            if (_LockHeight != 0 && IsHandleCreated) Height = _LockHeight;
        }
    }

    public bool Transparent
    {
        get => _Transparent;
        set
        {
            if (!value && BackColor.A != 255)
            {
                throw new Exception("Cannot set Transparent to false with a transparent BackColor.");
            }

            SetStyle(ControlStyles.Opaque, !value);
            SetStyle(ControlStyles.SupportsTransparentBackColor, value);
            if (value) InvalidateBitmap();
            else B = null;

            _Transparent = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Bloom[] Colors
    {
        get
        {
            var T = new List<Bloom>();
            foreach (var kvp in Items)
            {
                T.Add(new Bloom(kvp.Key, kvp.Value));
            }
            return T.ToArray();
        }
        set
        {
            foreach (var bloom in value)
            {
                Items[bloom.Name] = bloom.Value;
            }
            InvalidateCustomization();
            ColorHook();
            Invalidate();
        }
    }

    public string Customization
    {
        get => _Customization;
        set
        {
            if (value == _Customization) return;

            try
            {
                var data = Convert.FromBase64String(value);
                var items = Colors; // Получаем текущие цвета

                for (int i = 0; i < items.Length; i++)
                {
                    // Обновляем цвет согласно данным из Base64
                    items[i].Value = Color.FromArgb(BitConverter.ToInt32(data, i * 4));
                }

                _Customization = value; // Обновляем индивидуальную настройку
                Colors = items; // Обновляем цвета
                ColorHook(); // Вызываем метод, который должен быть реализован в производном классе
                Invalidate(); // Обновляем интерфейс
            }
            catch
            {
                return; // В случае ошибки возвращаемся
            }
        }
    }


    #endregion

    #region Property Helpers

    private void InvalidateBitmap()
    {
        if (Width == 0 || Height == 0) return;
        B = new Bitmap(Width, Height);
        G = Graphics.FromImage(B);
    }

    protected Color GetColor(string name)
    {
        return Items.TryGetValue(name, out var color) ? color : Color.Empty;
    }

    protected void SetColor(string name, Color color)
    {
        if (Items.ContainsKey(name)) Items[name] = color;
        else Items.Add(name, color);
    }

    protected void SetColor(string name, byte r, byte g, byte b)
    {
        SetColor(name, Color.FromArgb(r, g, b));
    }

    protected void SetColor(string name, byte a, byte r, byte g, byte b)
    {
        SetColor(name, Color.FromArgb(a, r, g, b));
    }

    protected void SetColor(string name, byte a, Color color)
    {
        SetColor(name, Color.FromArgb(a, color));
    }

    private void InvalidateCustomization()
    {
        using (var memoryStream = new MemoryStream(Items.Count * 4))
        {
            foreach (var bloom in Colors)
            {
                memoryStream.Write(BitConverter.GetBytes(bloom.Value.ToArgb()), 0, 4);
            }
            _Customization = Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    #endregion

    #region User Hooks

    protected abstract void ColorHook();
    protected abstract void PaintHook();

    #endregion

    #region Center Overloads

    protected Point Center(Rectangle r1, Size s1)
    {
        return new Point((r1.Width / 2 - s1.Width / 2) + r1.X, (r1.Height / 2 - s1.Height / 2) + r1.Y);
    }

    protected Point Center(Rectangle r1, Rectangle r2)
    {
        return Center(r1, r2.Size);
    }

    protected Point Center(int w1, int h1, int w2, int h2)
    {
        return new Point(w1 / 2 - w2 / 2, h1 / 2 - h2 / 2);
    }

    protected Point Center(Size s1, Size s2)
    {
        return Center(s1.Width, s1.Height, s2.Width, s2.Height);
    }

    protected Point Center(Rectangle r1)
    {
        return Center(ClientRectangle.Width, ClientRectangle.Height, r1.Width, r1.Height);
    }

    protected Point Center(Size s1)
    {
        return Center(Width, Height, s1.Width, s1.Height);
    }

    protected Point Center(int w1, int h1)
    {
        return Center(Width, Height, w1, h1);
    }

    #endregion

    #region Measure Overloads

    protected Size Measure(string text)
    {
        return MeasureGraphics.MeasureString(text, Font, Width).ToSize();
    }

    protected Size Measure()
    {
        return MeasureGraphics.MeasureString(Text, Font, Width).ToSize();
    }

    #endregion

    #region DrawCorners Overloads

    protected void DrawCorners(Color c1)
    {
        DrawCorners(c1, 0, 0, Width, Height);
    }

    protected void DrawCorners(Color c1, Rectangle r1)
    {
        DrawCorners(c1, r1.X, r1.Y, r1.Width, r1.Height);
    }

    protected void DrawCorners(Color c1, int x, int y, int width, int height)
    {
        if (_NoRounding) return;

        if (_Transparent)
        {
            B.SetPixel(x, y, c1);
            B.SetPixel(x + (width - 1), y, c1);
            B.SetPixel(x, y + (height - 1), c1);
            B.SetPixel(x + (width - 1), y + (height - 1), c1);
        }
        else
        {
            using (var drawCornersBrush = new SolidBrush(c1))
            {
                G.FillRectangle(drawCornersBrush, x, y, 1, 1);
                G.FillRectangle(drawCornersBrush, x + (width - 1), y, 1, 1);
                G.FillRectangle(drawCornersBrush, x, y + (height - 1), 1, 1);
                G.FillRectangle(drawCornersBrush, x + (width - 1), y + (height - 1), 1, 1);
            }
        }
    }

    #endregion

    #region DrawBorders Overloads

    protected void DrawBorders(Pen p1, int x, int y, int width, int height, int offset)
    {
        DrawBorders(p1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
    }

    protected void DrawBorders(Pen p1, int offset)
    {
        DrawBorders(p1, 0, 0, Width, Height, offset);
    }

    protected void DrawBorders(Pen p1, Rectangle r, int offset)
    {
        DrawBorders(p1, r.X, r.Y, r.Width, r.Height, offset);
    }

    protected void DrawBorders(Pen p1, int x, int y, int width, int height)
    {
        G.DrawRectangle(p1, x, y, width - 1, height - 1);
    }

    protected void DrawBorders(Pen p1)
    {
        DrawBorders(p1, 0, 0, Width, Height);
    }

    protected void DrawBorders(Pen p1, Rectangle r)
    {
        DrawBorders(p1, r.X, r.Y, r.Width, r.Height);
    }

    #endregion

    #region DrawText Overloads

    protected void DrawText(Brush b1, HorizontalAlignment a, int x, int y)
    {
        DrawText(b1, Text, a, x, y);
    }

    protected void DrawText(Brush b1, Point p1)
    {
        DrawText(b1, Text, p1.X, p1.Y);
    }

    protected void DrawText(Brush b1, int x, int y)
    {
        DrawText(b1, Text, x, y);
    }

    protected void DrawText(Brush b1, string text, HorizontalAlignment a, int x, int y)
    {
        if (string.IsNullOrEmpty(text)) return;

        var drawTextSize = Measure(text);
        var drawTextPoint = Center(drawTextSize);

        switch (a)
        {
            case HorizontalAlignment.Left:
                DrawText(b1, text, x, drawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                DrawText(b1, text, drawTextPoint.X + x, drawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                DrawText(b1, text, Width - drawTextSize.Width - x, drawTextPoint.Y + y);
                break;
        }
    }

    protected void DrawText(Brush b1, string text, Point p1)
    {
        DrawText(b1, text, p1.X, p1.Y);
    }

    protected void DrawText(Brush b1, string text, int x, int y)
    {
        if (string.IsNullOrEmpty(text)) return;
        G.DrawString(text, Font, b1, x, y);
    }

    #endregion

    #region DrawImage Overloads

    protected void DrawImage(HorizontalAlignment a, int x, int y)
    {
        DrawImage(_Image, a, x, y);
    }

    protected void DrawImage(Point p1)
    {
        DrawImage(_Image, p1.X, p1.Y);
    }

    protected void DrawImage(int x, int y)
    {
        DrawImage(_Image, x, y);
    }

    protected void DrawImage(Image image, HorizontalAlignment a, int x, int y)
    {
        if (image == null) return;

        var drawImagePoint = Center(image.Size);

        switch (a)
        {
            case HorizontalAlignment.Left:
                DrawImage(image, x, drawImagePoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                DrawImage(image, drawImagePoint.X + x, drawImagePoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                DrawImage(image, Width - image.Width - x, drawImagePoint.Y + y);
                break;
        }
    }

    protected void DrawImage(Image image, Point p1)
    {
        DrawImage(image, p1.X, p1.Y);
    }

    protected void DrawImage(Image image, int x, int y)
    {
        if (image == null) return;
        G.DrawImage(image, x, y, image.Width, image.Height);
    }

    #endregion

    #region DrawGradient Overloads

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height)
    {
        DrawGradient(blend, x, y, width, height, 90F);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height)
    {
        DrawGradient(c1, c2, x, y, width, height, 90F);
    }

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height, float angle)
    {
        var drawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(blend, drawGradientRectangle, angle);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        var drawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(c1, c2, drawGradientRectangle, angle);
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
    {
        using (var drawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle))
        {
            drawGradientBrush.InterpolationColors = blend;
            G.FillRectangle(drawGradientBrush, r);
        }
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
    {
        using (var drawGradientBrush = new LinearGradientBrush(r, c1, c2, angle))
        {
            G.FillRectangle(drawGradientBrush, r);
        }
    }

    #endregion
}
public abstract class ThemeContainer151 : ContainerControl
{
    protected Graphics G; // Графика

    protected ThemeContainer151()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
        _ImageSize = Size.Empty; // Инициализация размера изображения

        MeasureBitmap = new Bitmap(1, 1); // Измерение битмапа
        MeasureGraphics = Graphics.FromImage(MeasureBitmap); // Создание графики

        Font = new Font("Verdana", 8f); // Шрифт

        InvalidateCustimization(); // Повторная настройка
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if (_LockWidth != 0) width = _LockWidth; // Фиксация ширины
        if (_LockHeight != 0) height = _LockHeight; // Фиксация высоты
        base.SetBoundsCore(x, y, width, height, specified);
    }

    private Rectangle Header; // Заголовок
    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (_Movable && !_ControlMode)
            Header = new Rectangle(7, 7, Width - 14, _MoveHeight - 7);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        G = e.Graphics; // Используем графику
        PaintHook(); // Вызов специального метода рисования
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        InitializeMessages();
        InvalidateCustimization();
        ColorHook();

        _IsParentForm = Parent is Form; // Проверка родительской формы
        if (!_ControlMode) Dock = DockStyle.Fill;

        if (_LockWidth != 0) Width = _LockWidth; // Фиксация ширины
        if (_LockHeight != 0) Height = _LockHeight; // Фиксация высоты
        if (BackColorWait != Color.Empty) BackColor = BackColorWait; // Установка цвета фона

        if (_IsParentForm && !_ControlMode)
        {
            ParentForm.FormBorderStyle = _BorderStyle; // Установка стиля рамки
            ParentForm.TransparencyKey = _TransparencyKey; // Установка цвета прозрачности
        }

        OnCreation();
        base.OnHandleCreated(e);
    }

    protected virtual void OnCreation()
    {
        // Дополнительная логика, если нужно
    }

    #region " Sizing and Movement "

    protected MouseState State; // Состояние мыши
    private void SetState(MouseState current)
    {
        State = current;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_Sizable && !_ControlMode) InvalidateMouse();
        base.OnMouseMove(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        SetState(Enabled ? MouseState.None : MouseState.Block);
        base.OnEnabledChanged(e);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        SetState(MouseState.Over);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        SetState(MouseState.Over);
        base.OnMouseUp(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        SetState(MouseState.None);

        if (_Sizable && !_ControlMode && GetChildAtPoint(PointToClient(MousePosition)) != null)
        {
            Cursor = Cursors.Default;
            Previous = 0;
        }

        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button != MouseButtons.Left) return;

        SetState(MouseState.Down);

        if (_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized || _ControlMode) return;

        if (_Movable && Header.Contains(e.Location))
        {
            Capture = false;
            DefWndProc(ref Messages[0]);
        }
        else if (_Sizable && Previous != 0)
        {
            Capture = false;
            DefWndProc(ref Messages[Previous]);
        }
    }

    private Point GetIndexPoint; // Точка индекса
    private bool B1, B2, B3, B4; // Флаги для углов

    private int GetIndex()
    {
        GetIndexPoint = PointToClient(MousePosition);
        B1 = GetIndexPoint.X < 7;
        B2 = GetIndexPoint.X > Width - 7;
        B3 = GetIndexPoint.Y < 7;
        B4 = GetIndexPoint.Y > Height - 7;

        if (B1 && B3) return 4;
        if (B1 && B4) return 7;
        if (B2 && B3) return 5;
        if (B2 && B4) return 8;
        if (B1) return 1;
        if (B2) return 2;
        if (B3) return 3;
        if (B4) return 6;
        return 0;
    }

    private int Current, Previous; // Текущий и предыдущий индексы
    private void InvalidateMouse()
    {
        Current = GetIndex();
        if (Current == Previous) return;

        Previous = Current;
        switch (Previous)
        {
            case 0:
                Cursor = Cursors.Default;
                break;
            case 1:
            case 2:
                Cursor = Cursors.SizeWE;
                break;
            case 3:
            case 6:
                Cursor = Cursors.SizeNS;
                break;
            case 4:
            case 8:
                Cursor = Cursors.SizeNWSE;
                break;
            case 5:
            case 7:
                Cursor = Cursors.SizeNESW;
                break;
        }
    }

    private Message[] Messages = new Message[8]; // Сообщения для управления окнами
    private void InitializeMessages()
    {
        Messages[0] = Message.Create(Parent.Handle, 161, new IntPtr(2), IntPtr.Zero);
        for (int i = 1; i < Messages.Length; i++) // Измените <= на < для правильной инициализации
        {
            Messages[i] = Message.Create(Parent.Handle, 161, new IntPtr(i + 9), IntPtr.Zero);
        }
    }


    #endregion

    #region " Property Overrides "

    private Color BackColorWait; // Ожидаемый цвет фона
    public override Color BackColor
    {
        get => base.BackColor;
        set
        {
            if (IsHandleCreated)
            {
                if (!_ControlMode) Parent.BackColor = value;
                base.BackColor = value;
            }
            else
            {
                BackColorWait = value;
            }
        }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Color ForeColor
    {
        get => Color.Empty;
        set { }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Image BackgroundImage
    {
        get => null;
        set { }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override ImageLayout BackgroundImageLayout
    {
        get => ImageLayout.None;
        set { }
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

    public override Font Font
    {
        get => base.Font;
        set
        {
            base.Font = value;
            Invalidate();
        }
    }

    #endregion

    #region " Properties "

    private bool _Movable = true; // Возможность перемещения
    public bool Movable
    {
        get => _Movable;
        set => _Movable = value;
    }

    private bool _Sizable = true; // Возможность изменения размера
    public bool Sizable
    {
        get => _Sizable;
        set => _Sizable = value;
    }

    private int _MoveHeight = 24; // Высота перемещения
    protected int MoveHeight
    {
        get => _MoveHeight;
        set
        {
            if (value < 8) return;
            Header = new Rectangle(7, 7, Width - 14, value - 7);
            _MoveHeight = value;
            Invalidate();
        }
    }

    private bool _ControlMode; // Режим управления
    protected bool ControlMode
    {
        get => _ControlMode;
        set => _ControlMode = value;
    }

    private Color _TransparencyKey; // Цвет прозрачности
    public Color TransparencyKey
    {
        get => _IsParentForm && !_ControlMode ? ParentForm.TransparencyKey : _TransparencyKey;
        set
        {
            if (_IsParentForm && !_ControlMode) ParentForm.TransparencyKey = value;
            _TransparencyKey = value;
        }
    }

    private FormBorderStyle _BorderStyle; // Стиль границы
    public FormBorderStyle BorderStyle
    {
        get => _IsParentForm && !_ControlMode ? ParentForm.FormBorderStyle : _BorderStyle;
        set
        {
            if (_IsParentForm && !_ControlMode) ParentForm.FormBorderStyle = value;
            _BorderStyle = value;
        }
    }

    private bool _NoRounding; // Без округлений
    public bool NoRounding
    {
        get => _NoRounding;
        set
        {
            _NoRounding = value;
            Invalidate();
        }
    }

    private Image _Image; // Изображение
    public Image Image
    {
        get => _Image;
        set
        {
            _ImageSize = value == null ? Size.Empty : value.Size;
            _Image = value;
            Invalidate();
        }
    }

    private Size _ImageSize; // Размер изображения
    protected Size ImageSize => _ImageSize;

    private bool _IsParentForm; // Является ли родительская форма
    protected bool IsParentForm => _IsParentForm;

    private int _LockWidth; // Фиксированная ширина
    protected int LockWidth
    {
        get => _LockWidth;
        set
        {
            _LockWidth = value;
            if (_LockWidth != 0 && IsHandleCreated) Width = _LockWidth;
        }
    }

    private int _LockHeight; // Фиксированная высота
    protected int LockHeight
    {
        get => _LockHeight;
        set
        {
            _LockHeight = value;
            if (_LockHeight != 0 && IsHandleCreated) Height = _LockHeight;
        }
    }

    private Dictionary<string, Color> Items = new Dictionary<string, Color>(); // Словарь цветов
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Bloom[] Colors
    {
        get
        {
            var T = new List<Bloom>();
            var E = Items.GetEnumerator();

            while (E.MoveNext())
            {
                T.Add(new Bloom(E.Current.Key, E.Current.Value));
            }

            return T.ToArray();
        }
        set
        {
            foreach (var B in value)
            {
                if (Items.ContainsKey(B.Name)) Items[B.Name] = B.Value;
            }

            InvalidateCustimization();
            ColorHook();
            Invalidate();
        }
    }

    private string _Customization; // Кастомизация
    public string Customization
    {
        get => _Customization;
        set
        {
            if (value == _Customization) return;

            byte[] data;
            var items = Colors;

            try
            {
                data = Convert.FromBase64String(value);
                for (int i = 0; i < items.Length; i++)
                {
                    items[i].Value = Color.FromArgb(BitConverter.ToInt32(data, i * 4));
                }
            }
            catch
            {
                return;
            }

            _Customization = value;
            Colors = items;
            ColorHook();
            Invalidate();
        }
    }

    #endregion

    #region " Property Helpers "

    protected Color GetColor(string name) => Items[name]; // Получить цвет по имени

    protected void SetColor(string name, Color color)
    {
        if (Items.ContainsKey(name)) Items[name] = color;
        else Items.Add(name, color);
    }

    protected void SetColor(string name, byte r, byte g, byte b) =>
        SetColor(name, Color.FromArgb(r, g, b)); // Установить цвет по RGB

    protected void SetColor(string name, byte a, byte r, byte g, byte b) =>
        SetColor(name, Color.FromArgb(a, r, g, b)); // Установить цвет по ARGB

    protected void SetColor(string name, byte a, Color color) =>
        SetColor(name, Color.FromArgb(a, color)); // Установить цвет с учетом прозрачности

    private void InvalidateCustimization()
    {
        using (var ms = new System.IO.MemoryStream(Items.Count * 4))
        {
            foreach (var B in Colors)
            {
                ms.Write(BitConverter.GetBytes(B.Value.ToArgb()), 0, 4);
            }
            _Customization = Convert.ToBase64String(ms.ToArray()); // Преобразование в Base64
        }
    }

    #endregion

    #region " User Hooks "

    protected abstract void ColorHook(); // Абстрактный метод для переопределения
    protected abstract void PaintHook(); // Абстрактный метод для переопределения

    #endregion

    #region " Center Overloads "

    private Point CenterReturn; // Возврат центра

    protected Point Center(Rectangle r1, Size s1) =>
        CenterReturn = new Point((r1.Width / 2 - s1.Width / 2) + r1.X, (r1.Height / 2 - s1.Height / 2) + r1.Y);

    protected Point Center(Rectangle r1, Rectangle r2) => Center(r1, r2.Size); // Центрировать 2 прямоугольника

    protected Point Center(int w1, int h1, int w2, int h2) =>
        CenterReturn = new Point(w1 / 2 - w2 / 2, h1 / 2 - h2 / 2); // Центрировать размеры

    protected Point Center(Size s1, Size s2) => Center(s1.Width, s1.Height, s2.Width, s2.Height); // Центрировать размеры

    protected Point Center(Rectangle r1) => Center(ClientRectangle.Width, ClientRectangle.Height, r1.Width, r1.Height); // Центрировать прямоугольник

    protected Point Center(Size s1) => Center(Width, Height, s1.Width, s1.Height); // Центрировать размеры

    protected Point Center(int w1, int h1) => Center(Width, Height, w1, h1); // Центрировать размеры

    #endregion

    #region " Measure Overloads "

    private Bitmap MeasureBitmap; // Битмап для измерения
    private Graphics MeasureGraphics; // Графика для измерения

    protected Size Measure(string text) => MeasureGraphics.MeasureString(text, Font, Width).ToSize(); // Измерение строки

    protected Size Measure() => MeasureGraphics.MeasureString(Text, Font).ToSize(); // Измерение текста

    #endregion

    #region " DrawImage Overloads "

    private Point DrawImagePoint; // Точка для рисования изображения

    protected void DrawImage(HorizontalAlignment a, int x, int y) => DrawImage(_Image, a, x, y); // Рисование изображения с выравниванием

    protected void DrawImage(Point p1) => DrawImage(_Image, p1.X, p1.Y); // Рисование изображения по точке

    protected void DrawImage(int x, int y) => DrawImage(_Image, x, y); // Рисование изображения по координатам

    protected void DrawImage(Image image, HorizontalAlignment a, int x, int y)
    {
        if (image == null) return;
        DrawImagePoint = new Point(Width / 2 - image.Width / 2, MoveHeight / 2 - image.Height / 2); // Центрирование изображения

        switch (a)
        {
            case HorizontalAlignment.Left:
                DrawImage(image, x, DrawImagePoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                DrawImage(image, DrawImagePoint.X + x, DrawImagePoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                DrawImage(image, Width - image.Width - x, DrawImagePoint.Y + y);
                break;
        }
    }

    protected void DrawImage(Image image, Point p1) => DrawImage(image, p1.X, p1.Y); // Рисование изображения по точке

    protected void DrawImage(Image image, int x, int y)
    {
        if (image == null) return;
        G.DrawImage(image, x, y, image.Width, image.Height); // Рисование изображения
    }

    #endregion

    #region " DrawGradient Overloads "

    private LinearGradientBrush DrawGradientBrush; // Кисть для рисования градиента
    private Rectangle DrawGradientRectangle; // Прямоугольник для градиента

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height)
    {
        DrawGradient(blend, x, y, width, height, 90f); // Рисование градиента
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height)
    {
        DrawGradient(c1, c2, x, y, width, height, 90f); // Рисование градиента
    }

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height); // Создание прямоугольника
        DrawGradient(blend, DrawGradientRectangle, angle); // Рисование градиента
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height); // Создание прямоугольника
        DrawGradient(c1, c2, DrawGradientRectangle, angle); // Рисование градиента
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle); // Создание кисти
        DrawGradientBrush.InterpolationColors = blend; // Установка цветов градиента
        G.FillRectangle(DrawGradientBrush, r); // Рисование градиента
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, c1, c2, angle); // Создание кисти
        G.FillRectangle(DrawGradientBrush, r); // Рисование градиента
    }

    #endregion
}

public class BeyondTheme : ThemeContainer151
{
    private Color C1, C2, C3;
    private SolidBrush B1;
    private Pen P1, P2;

    public BeyondTheme()
    {
        MoveHeight = 20;
        SetColor("BackColor", Color.White);
        TransparencyKey = Color.Fuchsia;
    }

    protected override void ColorHook()
    {
        C1 = GetColor("BackColor");
        C2 = Color.FromArgb(50, 50, 50);
        C3 = Color.FromArgb(70, 70, 70);
        B1 = new SolidBrush(Color.FromArgb(70, 70, 70));
        P1 = new Pen(Color.FromArgb(50, 50, 50));
        P2 = new Pen(Color.FromArgb(20, 20, 20));
    }

    protected override void PaintHook()
    {
        G.Clear(C1);
        DrawGradient(Color.FromArgb(15, 15, 15), Color.FromArgb(30, 30, 30), 0, 0, Width, Height, 90);
        DrawGradient(C2, C3, 0, 0, Width, Height);
        G.DrawLine(P1, 0, 0, 0, 20);
        G.DrawLine(P1, Width - 1, 0, Width - 1, 25);
        G.DrawLine(P2, 0, 0, 0, Height);
        G.DrawLine(P2, Width - 1, 0, Width - 1, Height);
        G.DrawLine(P2, 0, Height - 1, Width, Height - 1);
        G.FillRectangle(new SolidBrush(Color.FromArgb(15, 15, 15)), 10, 20, Width - 20, Height - 30);
        G.DrawLine(P2, 0, 0, Width, 0);
        DrawText(Brushes.White, HorizontalAlignment.Center, 0, 0, Text);
    }
    protected void DrawText(Brush brush, HorizontalAlignment alignment, int offsetX, int offsetY, string text)
    {
        Font font = new Font("Arial", 12); // Укажите нужный шрифт и размер

        // Размер текста
        SizeF textSize = G.MeasureString(text, font);

        // Определяем начальную позицию
        int x = offsetX;
        if (alignment == HorizontalAlignment.Center)
        {
            x = (Width - (int)textSize.Width) / 2;
        }
        else if (alignment == HorizontalAlignment.Right)
        {
            x = Width - (int)textSize.Width - offsetX;
        }

        // Рисуем текст
        G.DrawString(text, font, brush, x, offsetY);
    }
}

class BeyondButton : ThemeControl151
{
    private Color C1;
    private Pen P1;

    public BeyondButton()
    {
        SetColor("BackColor", Color.White);
    }

    protected override void ColorHook()
    {
        C1 = GetColor("BackColor");
        P1 = new Pen(Color.FromArgb(50, 50, 50));
    }

    protected override void PaintHook()
    {
        G.Clear(C1);
        if (State == MouseState.Over)
        {
            DrawGradient(Color.FromArgb(30, 30, 30), Color.FromArgb(15, 15, 15), 0, 0, Width, Height);
        }
        else if (State == MouseState.Down)
        {
            DrawGradient(Color.FromArgb(15, 15, 15), Color.FromArgb(30, 30, 30), 0, 0, Width, Height);
        }
        else
        {
            DrawGradient(Color.FromArgb(15, 15, 15), Color.FromArgb(30, 30, 30), 0, 0, Width, Height);
        }
        DrawBorders(P1, ClientRectangle);
        DrawText(Brushes.White, HorizontalAlignment.Center, 0, 0);
    }
    protected void DrawText(Brush brush, HorizontalAlignment alignment, int x, int y, string text, Font font = null)
    {
        if (font == null)
            font = new Font("Arial", 12); // Укажите нужный шрифт и размер по умолчанию

        // Определяем размер текста
        SizeF textSize = G.MeasureString(text, font);

        // Устанавливаем координаты в зависимости от типа выравнивания
        if (alignment == HorizontalAlignment.Center)
            x = (Width - (int)textSize.Width) / 2;
        else if (alignment == HorizontalAlignment.Right)
            x = Width - (int)textSize.Width - x;

        // Рисуем текст
        G.DrawString(text, font, brush, x, y);
    }

}

class BeyondButton2 : ThemeControl151
{
    private Color C1, C2, C3;
    private Pen P1;

    public BeyondButton2()
    {
        SetColor("BackColor", Color.White);
    }

    protected override void ColorHook()
    {
        C1 = GetColor("BackColor");
        C2 = Color.FromArgb(50, 50, 50);
        C3 = Color.FromArgb(70, 70, 70);
        P1 = new Pen(Color.Black);
    }

    protected override void PaintHook()
    {
        G.Clear(C1);
        if (State == MouseState.Over)
        {
            DrawGradient(C2, C3, 0, 0, Width, Height);
        }
        else if (State == MouseState.Down)
        {
            DrawGradient(C3, C2, 0, 0, Width, Height);
        }
        else
        {
            DrawGradient(C3, C2, 0, 0, Width, Height);
        }
        DrawBorders(P1, ClientRectangle);
        DrawText(Brushes.White, HorizontalAlignment.Center, 0, 0);
    }
}