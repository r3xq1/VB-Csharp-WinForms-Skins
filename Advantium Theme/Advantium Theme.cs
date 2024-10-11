using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

public enum MouseState : byte { None, Over, Down, Block }

// Theme
public class AdvantiumTheme : ThemeControl151
{
    public AdvantiumTheme()
    {
        TabStop = false;
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        TransparencyKey = Color.Fuchsia;
        MoveHeight = 35;

        SetColor("BackColor", Color.FromArgb(40, 40, 40));
        SetColor("BorderInner", Color.FromArgb(65, 65, 65));
        SetColor("BorderColor", Color.Black);
        SetColor("TextColor", Color.LawnGreen);
    }

    private Color C1, BC, BA, T1;

    protected override void ColorHook()
    {
        C1 = GetColor("BackColor");
        BC = GetColor("BorderColor");
        BA = GetColor("BorderInner");
        T1 = GetColor("TextColor");
    }

    protected override void PaintHook()
    {
        // Очистка фона
        G.Clear(C1);

        // Узнать параметры текста
        string text = "advantiumTheme1";
        Size textSize = Measure(text);

        // Проверка цвета для фона
        using (var brush = new SolidBrush(Color.FromArgb(40, 40, 40)))
        {
            G.FillRectangle(brush, ClientRectangle); // Заливка фона
        }

        // Отрисовка текста в верхнем углу
        G.DrawString(text, Font, new SolidBrush(Color.LawnGreen), new Point(15, 5));
    }


}

// Button
public class AdvantiumButton : ThemeControl151
{
    public AdvantiumButton()
    {
        TabStop = false;
        SetColor("BackColor", Color.FromArgb(40, 40, 40));
        SetColor("TextColor", Color.LawnGreen);
    }

    private Color C1, T1;

    protected override void ColorHook()
    {
        C1 = GetColor("BackColor");
        T1 = GetColor("TextColor");
    }

    protected override void PaintHook()
    {
        G.Clear(C1);
        switch (State)
        {
            case MouseState.None:
                DrawGradient(Color.FromArgb(50, 50, 50), Color.FromArgb(42, 42, 42), ClientRectangle, 90);
                Cursor = Cursors.Arrow;
                break;
            case MouseState.Down:
                DrawGradient(Color.FromArgb(50, 50, 50), Color.FromArgb(42, 42, 42), ClientRectangle, 90);
                Cursor = Cursors.Hand;
                break;
            case MouseState.Over:
                DrawGradient(Color.FromArgb(42, 42, 42), Color.FromArgb(50, 50, 50), ClientRectangle, 90);
                Cursor = Cursors.Hand;
                break;
        }

        DrawBorders(new Pen(new SolidBrush(Color.FromArgb(59, 59, 59))), 1);
        DrawBorders(new Pen(new SolidBrush(Color.FromArgb(25, 25, 25))));
        DrawCorners(Color.FromArgb(35, 35, 35));
        DrawText(new SolidBrush(T1), HorizontalAlignment.Center, 0, 0);
    }
}

// Top Button
public class AdvantiumTopButton : ThemeControl151
{
    public AdvantiumTopButton()
    {
        TabStop = false;
        SetColor("BackColor", Color.FromArgb(40, 40, 40));
        SetColor("TextColor", Color.LawnGreen);
        Size = new Size(28, 26);
    }

    private Color C1, T1;

    protected override void ColorHook()
    {
        C1 = GetColor("BackColor");
        T1 = GetColor("TextColor");
    }

    protected override void PaintHook()
    {
        G.Clear(C1);
        switch (State)
        {
            case MouseState.None:
                DrawGradient(Color.FromArgb(38, 38, 38), Color.FromArgb(30, 30, 30), ClientRectangle, 90);
                Cursor = Cursors.Arrow;
                break;
            case MouseState.Down:
                DrawGradient(Color.FromArgb(50, 50, 50), Color.FromArgb(42, 42, 42), ClientRectangle, 90);
                Cursor = Cursors.Hand;
                break;
            case MouseState.Over:
                DrawGradient(Color.FromArgb(42, 42, 42), Color.FromArgb(50, 50, 50), ClientRectangle, 90);
                Cursor = Cursors.Hand;
                break;
        }

        DrawBorders(new Pen(new SolidBrush(Color.FromArgb(65, 65, 65))), new Rectangle(1, 0, Width - 2, Height));
        DrawBorders(new Pen(new SolidBrush(Color.FromArgb(22, 22, 22))));
        G.DrawLine(new Pen(new SolidBrush(Color.FromArgb(22, 22, 22))), 2, Height - 1, Width - 3, Height - 1);
        G.DrawLine(new Pen(new SolidBrush(Color.FromArgb(65, 65, 65))), 0, 1, Width - 1, 1);
        G.DrawLine(new Pen(new SolidBrush(Color.Black)), 0, 0, Width, 0);
    }
}

// CheckBox
public class AdvantiumCheck : ThemeControl151
{
    private bool _CheckedState;

    public bool CheckedState
    {
        get { return _CheckedState; }
        set
        {
            _CheckedState = value;
            Invalidate();
        }
    }

    public AdvantiumCheck()
    {
        AutoSize = true;
        TabStop = false;
        Size = new Size(135, 16);
        MinimumSize = new Size(16, 16);
        MaximumSize = new Size(600, 16);
        CheckedState = false;
        
        SetColor("CheckBorderOut", Color.FromArgb(25, 25, 25));
        SetColor("CheckBorderIn", Color.FromArgb(59, 59, 59));
        SetColor("TextColor", Color.LawnGreen);
        SetColor("CheckBack1", Color.FromArgb(132, 192, 240));
        SetColor("CheckBack2", Color.LawnGreen);
        SetColor("CheckFore1", Color.LawnGreen);
        SetColor("CheckFore2", Color.FromArgb(42, 242, 77));
        SetColor("ColorUncheck", Color.FromArgb(35, 35, 35));
        SetColor("BackColor", Color.FromArgb(35, 35, 35));
    }

    private Color C1, C2, C3, C4, C5, C6, P1, P2, B1;

    protected override void ColorHook()
    {
        C1 = GetColor("CheckBack1");
        C2 = GetColor("CheckBack2");
        C3 = GetColor("CheckFore1");
        C4 = GetColor("CheckFore2");
        C5 = GetColor("ColorUncheck");
        C6 = GetColor("BackColor");
        P1 = GetColor("CheckBorderOut");
        P2 = GetColor("CheckBorderIn");
        B1 = GetColor("TextColor");
    }

    protected override void PaintHook()
    {
        G.Clear(C6);

        if (CheckedState)
        {
            DrawGradient(C1, C2, 3, 3, 9, 9, 90);
            DrawGradient(C3, C4, 4, 4, 7, 7, 90);
        }
        else
        {
            DrawGradient(C5, C5, 0, 0, 15, 15, 90);
        }

        G.DrawRectangle(new Pen(new SolidBrush(P1)), 0, 0, 14, 14);
        G.DrawRectangle(new Pen(new SolidBrush(P2)), 1, 1, 12, 12);
        DrawText(new SolidBrush(B1), 17, 0);
        DrawCorners(C6, new Rectangle(0, 0, 13, 13));
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);

        CheckedState = !CheckedState;
    }
}

// Bloom Class
public class Bloom
{
    private string _Name;
    public string Name
    {
        get { return _Name; }
        set { _Name = value; }
    }

    private Color _Value;
    public Color Value
    {
        get { return _Value; }
        set { _Value = value; }
    }

    public Bloom()
    {
    }

    public Bloom(string name, Color value)
    {
        _Name = name;
        _Value = value;
    }
}

public abstract class ThemeControl151 : Control
{
    protected Graphics G;
    private Bitmap B;
    private Bitmap MeasureBitmap;
    private Graphics MeasureGraphics;

    private Size _ImageSize;
    private int _LockWidth;
    private int _LockHeight;
    private bool _Transparent;
    private bool _NoRounding;
    private Image _Image;
    private Color BackColorWait;

    private Dictionary<string, Color> Items = new();

    public Color TransparencyKey { get; set; }
    public int MoveHeight { get; set; } // Добавлено свойство MoveHeight

    public ThemeControl151()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);

        // Инициализация значения по умолчанию
        MoveHeight = 35;

        _ImageSize = Size.Empty;

        MeasureBitmap = new Bitmap(1, 1);
        MeasureGraphics = Graphics.FromImage(MeasureBitmap);

        Font = new Font("Verdana", 8);
        InvalidateCustomization();
        TransparencyKey = Color.Fuchsia; // Установка значения по умолчанию
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
    {
        using (var gradientBrush = new LinearGradientBrush(r, c1, c2, angle))
        {
            G.FillRectangle(gradientBrush, r);
        }
    }
    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        using (var gradientBrush = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle))
        {
            G.FillRectangle(gradientBrush, x, y, width, height);
        }
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if (_LockWidth != 0) width = _LockWidth;
        if (_LockHeight != 0) height = _LockHeight;
        base.SetBoundsCore(x, y, width, height, specified);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        if (_Transparent && !(Width == 0 || Height == 0))
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

    protected MouseState State;

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
            if (IsHandleCreated)
                base.BackColor = value;
            else
                BackColorWait = value;
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

    protected Size ImageSize => _ImageSize;

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
                throw new Exception("Unable to change value to false while a transparent BackColor is in use.");

            SetStyle(ControlStyles.Opaque, !value);
            SetStyle(ControlStyles.SupportsTransparentBackColor, value);

            if (value) InvalidateBitmap();
            else B = null;

            _Transparent = value;
            Invalidate();
        }
    }

    public Bloom[] Colors
    {
        get
        {
            var T = new List<Bloom>();
            foreach (var kv in Items)
            {
                T.Add(new Bloom(kv.Key, kv.Value));
            }
            return T.ToArray();
        }
        set
        {
            foreach (var B in value)
            {
                if (Items.ContainsKey(B.Name))
                    Items[B.Name] = B.Value;
            }
            InvalidateCustomization();
            ColorHook();
            Invalidate();
        }
    }

    private string _Customization;

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

    protected void SetColor(string name, byte r, byte g, byte b) =>
        SetColor(name, Color.FromArgb(r, g, b));

    protected void SetColor(string name, byte a, byte r, byte g, byte b) =>
        SetColor(name, Color.FromArgb(a, r, g, b));

    protected void SetColor(string name, byte a, Color color) =>
        SetColor(name, Color.FromArgb(a, color));

    private void InvalidateCustomization()
    {
        using (var ms = new MemoryStream(Items.Count * 4))
        {
            foreach (var B in Colors)
            {
                var bytes = BitConverter.GetBytes(B.Value.ToArgb());
                ms.Write(bytes, 0, 4);
            }
            _Customization = Convert.ToBase64String(ms.ToArray());
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

    protected Point Center(Rectangle r1, Rectangle r2) => Center(r1, r2.Size);

    protected Point Center(int w1, int h1, int w2, int h2)
    {
        return new Point(w1 / 2 - w2 / 2, h1 / 2 - h2 / 2);
    }

    protected Point Center(Size s1, Size s2) => Center(s1.Width, s1.Height, s2.Width, s2.Height);

    protected Point Center(Rectangle r1) => Center(ClientRectangle.Width, ClientRectangle.Height, r1.Width, r1.Height);

    protected Point Center(Size s1) => Center(Width, Height, s1.Width, s1.Height);

    protected Point Center(int w1, int h1) => Center(Width, Height, w1, h1);

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

    private SolidBrush DrawCornersBrush;

    protected void DrawCorners(Color color)
    {
        DrawCorners(color, 0, 0, Width, Height);
    }

    protected void DrawCorners(Color color, Rectangle r)
    {
        DrawCorners(color, r.X, r.Y, r.Width, r.Height);
    }

    protected void DrawCorners(Color color, int x, int y, int width, int height)
    {
        if (_NoRounding) return;

        if (_Transparent)
        {
            B.SetPixel(x, y, color);
            B.SetPixel(x + (width - 1), y, color);
            B.SetPixel(x, y + (height - 1), color);
            B.SetPixel(x + (width - 1), y + (height - 1), color);
        }
        else
        {
            DrawCornersBrush = new SolidBrush(color);
            G.FillRectangle(DrawCornersBrush, x, y, 1, 1);
            G.FillRectangle(DrawCornersBrush, x + (width - 1), y, 1, 1);
            G.FillRectangle(DrawCornersBrush, x, y + (height - 1), 1, 1);
            G.FillRectangle(DrawCornersBrush, x + (width - 1), y + (height - 1), 1, 1);
        }
    }

    #endregion

    #region DrawBorders Overloads

    protected void DrawBorders(Pen pen, int x, int y, int width, int height, int offset)
    {
        DrawBorders(pen, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
    }

    protected void DrawBorders(Pen pen, int offset)
    {
        DrawBorders(pen, 0, 0, Width, Height, offset);
    }

    protected void DrawBorders(Pen pen, Rectangle rect, int offset)
    {
        DrawBorders(pen, rect.X, rect.Y, rect.Width, rect.Height, offset);
    }

    protected void DrawBorders(Pen pen, int x, int y, int width, int height)
    {
        G.DrawRectangle(pen, x, y, width - 1, height - 1);
    }

    protected void DrawBorders(Pen pen)
    {
        DrawBorders(pen, 0, 0, Width, Height);
    }

    protected void DrawBorders(Pen pen, Rectangle rect)
    {
        DrawBorders(pen, rect.X, rect.Y, rect.Width, rect.Height);
    }

    #endregion

    #region DrawText Overloads

    private Point DrawTextPoint;
    private Size DrawTextSize;

    protected void DrawText(Brush brush, HorizontalAlignment alignment, int x, int y)
    {
        DrawText(brush, Text, alignment, x, y);
    }

    protected void DrawText(Brush brush, Point point)
    {
        DrawText(brush, Text, point.X, point.Y);
    }

    protected void DrawText(Brush brush, int x, int y)
    {
        DrawText(brush, Text, x, y);
    }

    protected void DrawText(Brush brush, string text, HorizontalAlignment alignment, int x, int y)
    {
        if (text.Length == 0) return;

        DrawTextSize = Measure(text);
        DrawTextPoint = Center(DrawTextSize);

        switch (alignment)
        {
            case HorizontalAlignment.Left:
                DrawText(brush, text, x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                DrawText(brush, text, DrawTextPoint.X + x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                DrawText(brush, text, Width - DrawTextSize.Width - x, DrawTextPoint.Y + y);
                break;
        }
    }

    protected void DrawText(Brush brush, string text, Point point)
    {
        DrawText(brush, text, point.X, point.Y);
    }

    protected void DrawText(Brush brush, string text, int x, int y)
    {
        if (text.Length == 0) return;
        G.DrawString(text, Font, brush, x, y);
    }

    #endregion
}