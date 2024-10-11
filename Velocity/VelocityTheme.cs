using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

using static Helpers;

public static class Helpers
{
    public enum MouseState
    {
        Hover = 1,
        Down = 2,
        None = 3
    }

    public enum TxtAlign
    {
        Left = 1,
        Center = 2,
        Right = 3
    }

    public static Image B64Image(string b64)
    {
        try
        {
            // Преобразование строки Base64 в байтовый массив
            byte[] imageBytes = Convert.FromBase64String(b64);

            // Создание потока памяти с полученными байтами
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                // Создание изображения из потока памяти
                return Image.FromStream(ms);
            }
        }
        catch (FormatException ex)
        {
            // Обработка исключения, если строка не является корректной строкой Base64
            Console.WriteLine("Ошибка: Некорректная строка Base64. " + ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            // Обработка других исключений, которые могут возникнуть при создании изображения
            Console.WriteLine("Ошибка: Не удалось создать изображение. " + ex.Message);
            return null;
        }
    }

    public static Color FromHex(string hex)
    {
        return ColorTranslator.FromHtml(hex);
    }
}

public class VelocityButton : Control
{
    private MouseState state = MouseState.None;
    private bool _enabled = true;

    private TxtAlign _txtAlign = TxtAlign.Center;
    public TxtAlign TextAlign
    {
        get { return _txtAlign; }
        set
        {
            _txtAlign = value;
            Invalidate();
        }
    }

    public VelocityButton()
    {
        DoubleBuffered = true;
        Font = new Font("Segoe UI Semilight", 9);
        ForeColor = Color.White;
        Size = new Size(94, 40);
    }

    public new bool Enabled
    {
        get { return _enabled; }
        set
        {
            _enabled = value;
            Invalidate();
        }
    }

    public void PerformClick()
    {
        OnClick(EventArgs.Empty);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        switch (_enabled)
        {
            case true:
                switch (state)
                {
                    case MouseState.None:
                        g.Clear(FromHex("#435363"));
                        break;
                    case MouseState.Hover:
                        g.Clear(FromHex("#38495A"));
                        break;
                    case MouseState.Down:
                        g.Clear(BackColor);
                        g.FillRectangle(new SolidBrush(FromHex("#2c3e50")), 1, 1, Width - 2, Height - 2);
                        break;
                }
                break;
            case false:
                g.Clear(FromHex("#38495A"));
                break;
        }

        switch (_txtAlign)
        {
            case TxtAlign.Left:
                g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(8, 0, Width, Height), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                break;
            case TxtAlign.Center:
                g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(0, 0, Width, Height), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                break;
            case TxtAlign.Right:
                g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(0, 0, Width - 8, Height), new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                break;
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        state = MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        state = MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        state = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        state = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        state = MouseState.Hover;
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}

[DefaultEvent("CheckChanged")]
public class VelocityCheckBox : Control
{
    private MouseState _state = MouseState.None;
    public event EventHandler CheckChanged;

    private bool _autoSize = true;
    public override bool AutoSize
    {
        get { return _autoSize; }
        set
        {
            _autoSize = value;
            Invalidate();
        }
    }

    private bool _checked = false;
    public bool Checked
    {
        get { return _checked; }
        set
        {
            _checked = value;
            Invalidate();
            OnCheckChanged(EventArgs.Empty);
        }
    }

    public VelocityCheckBox()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        switch (AutoSize)
        {
            case true:
                Size = new Size(TextRenderer.MeasureText(Text, Font).Width + 28, Height);
                break;
        }
        var g = e.Graphics;
        switch (_state)
        {
            case MouseState.Hover:
                g.FillRectangle(new SolidBrush(FromHex("#DBDBDB")), 4, 4, 14, 14);
                break;
            default:
                g.FillRectangle(Brushes.White, 4, 4, 14, 14);
                break;
        }
        if (_checked)
        {
            g.FillRectangle(new SolidBrush(FromHex("#435363")), 7, 7, 9, 9);
        }
        g.DrawRectangle(new Pen(FromHex("#435363")), new Rectangle(4, 4, 14, 14));
        g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(22, 0, Width, Height), new StringFormat { LineAlignment = StringAlignment.Center });
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Checked = !Checked;
        _state = MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _state = MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        _state = MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _state = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _state = MouseState.Down;
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected virtual void OnCheckChanged(EventArgs e)
    {
        CheckChanged?.Invoke(this, e);
    }
}

[DefaultEvent("CheckChanged")]
public class VelocityRadioButton : Control
{
    private MouseState _state;
    public event EventHandler CheckChanged;

    private bool _autoSize = true;
    public override bool AutoSize
    {
        get { return _autoSize; }
        set
        {
            _autoSize = value;
            Invalidate();
        }
    }

    private bool _checked = false;
    public bool Checked
    {
        get { return _checked; }
        set
        {
            _checked = value;
            Invalidate();
            InvalidateControls();
            OnCheckChanged(EventArgs.Empty);
        }
    }

    public VelocityRadioButton()
    {
        DoubleBuffered = true;
        InvalidateControls();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        switch (AutoSize)
        {
            case true:
                Size = new Size(TextRenderer.MeasureText(Text, Font).Width + 24, Height);
                break;
        }
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        switch (_state)
        {
            case MouseState.Hover:
                g.FillEllipse(new SolidBrush(FromHex("#DBDBDB")), 4, 4, 14, 14);
                break;
            default:
                g.FillEllipse(Brushes.White, 4, 4, 14, 14);
                break;
        }
        g.DrawEllipse(new Pen(FromHex("#435363")), new Rectangle(4, 4, 14, 14));
        g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(22, 0, Width, Height), new StringFormat { LineAlignment = StringAlignment.Center });
        if (_checked)
        {
            g.FillEllipse(new SolidBrush(FromHex("#435363")), 7, 7, 8, 8);
        }
    }

    private void InvalidateControls()
    {
        if (!IsHandleCreated || !_checked) return;
        foreach (Control c in Parent.Controls)
        {
            if (c != this && c is VelocityRadioButton)
            {
                ((VelocityRadioButton)c).Checked = false;
                Invalidate();
            }
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _state = MouseState.Hover;
        Checked = !Checked;
        _state = MouseState.Hover;
        InvalidateControls();
    }

    protected override void OnMouseHover(EventArgs e)
    {
        base.OnMouseHover(e);
        _state = MouseState.Hover;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _state = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _state = MouseState.None;
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }

    protected virtual void OnCheckChanged(EventArgs e)
    {
        CheckChanged?.Invoke(this, e);
    }
}

public class VelocityTitle : Control
{
    private TxtAlign _txtAlign = TxtAlign.Left;

    public TxtAlign TextAlign
    {
        get { return _txtAlign; }
        set
        {
            _txtAlign = value;
            Invalidate();
        }
    }

    public VelocityTitle()
    {
        DoubleBuffered = true;
        Size = new Size(180, 23);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.DrawLine(new Pen(ColorTranslator.FromHtml("#435363")), new Point(0, Height / 2), new Point(Width, Height / 2));

        var txtSize = g.MeasureString(Text, Font);
        var x = 0f;
        var y = (Height / 2) - (txtSize.Height / 2);

        switch (_txtAlign)
        {
            case TxtAlign.Left:
                g.FillRectangle(new SolidBrush(BackColor), new Rectangle(18, (Height / 2) - (int)txtSize.Height - 2, (int)txtSize.Width + 6, (int)txtSize.Height + 6));
                x = 20;
                break;
            case TxtAlign.Center:
                g.FillRectangle(new SolidBrush(BackColor), new Rectangle((Width / 2) - (int)txtSize.Width / 2 - 2, (Height / 2) - (int)txtSize.Height / 2 - 2, (int)txtSize.Width + 2, (int)txtSize.Height + 2));
                x = (Width / 2) - (int)txtSize.Width / 2;
                break;
            case TxtAlign.Right:
                g.FillRectangle(new SolidBrush(BackColor), new Rectangle(Width - (int)txtSize.Width + 18, (Height / 2) - (int)txtSize.Height - 2, (int)txtSize.Width + 4, Height + 6));
                x = Width - (int)txtSize.Width + 16;
                break;
        }

        // Проверка на возможность преобразования текста в число
        string renderedText;
        if (int.TryParse(Text, out int value))
        {
            renderedText = value.ToString();
        }
        else
        {
            renderedText = Text;
        }

        g.DrawString(renderedText, Font, new SolidBrush(ForeColor), x, y);
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}
[DefaultEvent("XClicked")]

public class VelocityAlert : Control
{
    public event EventHandler XClicked;
    private bool _xHover = false;

    private readonly string FillerImage = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAIAAAAlC+aJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAHBhaW50Lm5ldCA0LjAuMTM0A1t6AAADZUlEQVRoQ+2W2ytsYRjG9x85Cikpp1KORSkihITkyinkdCERm0wi3IiS5ELKIQkXkiQkh9m/tZ7PNHuwWi707dl9v4vpfd/1zut51ncYvyIpzv9iIJaCSLkzYA8pdwbsIeXOgD2k3Bmwh5Q7A/aQcmfAHlLuDNhDyp0Be0i5M2APKXcG7CHlzoA9pPx7Bh4fH4+Pj/f29m5vb03pHSpnZ2cmCeTt7Y1OhlxdXZnSO8/PzyGHgJR/w8Dc3Fx6err6s7OzTdXn9fW1sLCQ+urqqil9AQ15eXkaAkdHR+aBT21tLcWJiQmTB6IJYQ1sbW3RtrKyovTy8lKBGBsb05zl5WVT+ozT01N6pqenlSatwNLSkoaMjIyYUiBqDmugoaGhr6/PJH/DojNhf3+fz2g0SoXPqampu7s7Nfz2Iejt7W1sbFQxiaenJ1YG/wwZHBw01UA83eENsHnW1tZ4Z4eHh/f396bq09bW1tLSQpE5MgBFRUWdnZ0EJycn1Dc2NojLysrYHhhjSNIpGh4ezs/PJ6C5v79fxWA83SENXF9f01NVVaVmGBgY0KPNzU1SgiQDKCZlx1dUVHR0dKiYlZVVWlrqfd+nvb1ddS4G0ouLC2ICFkr1YPwZ4Qycn5/TMzQ0pHRmZoYU6cTl5eXaHkkGoLm52Rsdidzc3JBy+RC3trZqASV6dnaWmAXs6enxvvNDBlh0era3t00eixUUFKCb45iZmcnqj4+P6xwjJf5eFxYWqLACSiEnJ4erzCSxWF1dHedKa8W+ZwIbjJillrFg6ISwZyA3N3d+fl4xvwZpaWmsAAa6u7u7uro4Bk1NTcwpLi4uKSmhhy3OsRkdHaUY/2JNTU187wHNTMAAQwDnGsJhYKZp+hpPd3gD/IHq6moOw8vLC++JG4O73zzzeXh4YA4HXSn99fX1BLzajIwM7aLJyUl+Lg4ODogXFxfpZ3N63QmwpD9yjfJGWXE18+Z2d3fNg3cSDezs7BDHf1MrKytJFbNW3ohIhO20vr6uYiIsNXvJJIFoTlgDgs3DhW2SD3z8/+JTmJB0EScScghI+fcM/FNIuTNgDyl3Buwh5c6APaTcGbCHlDsD9pByZ8AeUu4M2EPKnQF7SLkzYA8pdwbsIeXOgD2k3Bmwh5QbA6lLihuIRP4AXubLj7lh8ksAAAAASUVORK5CYII=";

    private bool _xChangeCursor = true;

    public bool XChangeCursor
    {
        get => _xChangeCursor;
        set
        {
            _xChangeCursor = value;
            Invalidate();
        }
    }

    private string _title = "";

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            Invalidate();
        }
    }

    private bool _exitButton = false;

    public bool ShowExit
    {
        get => _exitButton;
        set
        {
            _exitButton = value;
            Invalidate();
        }
    }

    private bool _showImage = true;

    public bool ShowImage
    {
        get => _showImage;
        set
        {
            _showImage = value;
            Invalidate();
        }
    }

    private Image _image;

    public Image Image
    {
        get => _image;
        set
        {
            _image = value;
            Invalidate();
        }
    }

    private Color _border = FromHex("#435363");

    public Color Border
    {
        get => _border;
        set
        {
            _border = value;
            Invalidate();
        }
    }

    public VelocityAlert()
    {
        Size = new Size(370, 80);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        if (ShowImage)
        {
            if (_image == null)
            {
                g.DrawImage(B64Image(FillerImage), 13, 8);
            }
            else
            {
                g.DrawImage(_image, 12, 8, 64, 64);
            }
            g.DrawString(_title, new Font("Segoe UI Semilight", 14), new SolidBrush(ForeColor), 84, 6);
            g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(86, 33, Width - 88, Height - 10));
        }
        else
        {
            g.DrawString(_title, new Font("Segoe UI Semilight", 14), new SolidBrush(ForeColor), 18, 6);
            g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(20, 33, Width - 28, Height - 10));
        }

        if (ShowExit)
        {
            var exitPosition = new Point(Width - 18, 4);
            var exitColor = _xHover ? FromHex("#596372") : FromHex("#435363");
            g.DrawString("r", new Font("Marlett", 9), new SolidBrush(exitColor), exitPosition);
        }

        g.DrawRectangle(new Pen(_border), 0, 0, Width - 1, Height - 1);
        g.FillRectangle(new SolidBrush(_border), 0, 0, 6, Height);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_exitButton)
        {
            if (new Rectangle(Width - 16, 4, 12, 13).Contains(e.Location))
            {
                _xHover = true;
                if (_xChangeCursor)
                {
                    Cursor = Cursors.Hand;
                }
            }
            else
            {
                _xHover = false;
                Cursor = Cursors.Default;
            }
        }
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_exitButton && _xHover)
        {
            XClicked?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}

public class VelocitySplitter : Control
{
    private int _offset = 8;

    public int Offset
    {
        get => _offset;
        set
        {
            _offset = value;
            Invalidate();
        }
    }

    public VelocitySplitter()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias; // Установите режим сглаживания
        g.DrawLine(new Pen(ForeColor), new Point(_offset, Height / 2), new Point(Width - _offset, Height / 2)); // Отрисовка линии по центру
    }
}

public class VelocityTabControl : TabControl
{
    private int _overtab = 0;

    private TxtAlign _txtAlign = TxtAlign.Center;
    public TxtAlign TextAlign
    {
        get { return _txtAlign; }
        set
        {
            _txtAlign = value;
            Invalidate();
        }
    }

    public VelocityTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        DoubleBuffered = true;
        SizeMode = TabSizeMode.Fixed;
        ItemSize = new Size(40, 130);
        Alignment = TabAlignment.Left;
        Font = new Font("Segoe UI Semilight", 9);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var b = new Bitmap(Width, Height);
        var g = Graphics.FromImage(b);
        g.Clear(FromHex("#435363"));
        for (int i = 0; i < TabCount; i++)
        {
            var tabRect = GetTabRect(i);
            if (i == SelectedIndex)
            {
                g.FillRectangle(new SolidBrush(FromHex("#2c3e50")), tabRect);
            }
            else if (i == _overtab)
            {
                g.FillRectangle(new SolidBrush(FromHex("#435363")), tabRect);
            }
            else
            {
                g.FillRectangle(new SolidBrush(FromHex("#38495A")), tabRect);
            }
            switch (_txtAlign)
            {
                case TxtAlign.Left:
                    g.DrawString(TabPages[i].Text, Font, Brushes.White, new Rectangle(tabRect.X + 8, tabRect.Y, tabRect.Width, tabRect.Height), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                    break;
                case TxtAlign.Center:
                    g.DrawString(TabPages[i].Text, Font, Brushes.White, tabRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    break;
                case TxtAlign.Right:
                    g.DrawString(TabPages[i].Text, Font, Brushes.White, new Rectangle(tabRect.X - 8, tabRect.Y, tabRect.Width, tabRect.Height), new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center });
                    break;
            }
        }

        e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
        g.Dispose();
        b.Dispose();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        for (int i = 0; i < TabPages.Count; i++)
        {
            if (GetTabRect(i).Contains(e.Location))
            {
                _overtab = i;
            }
            Invalidate();
        }
    }
}

public class VelocityTag : Control
{
    private Color _border = FromHex("#2c3e50");

    public Color Border
    {
        get { return _border; }
        set
        {
            _border = value;
            Invalidate();
        }
    }

    public VelocityTag()
    {
        DoubleBuffered = true;
        BackColor = FromHex("#34495e");
        ForeColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(BackColor);
        g.DrawRectangle(new Pen(_border), 0, 0, Width - 1, Height - 1);
        g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(0, 0, Width, Height), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}

public class VelocityProgressBar : Control
{
    private Color _border = FromHex("#485e75");

    public Color Border
    {
        get { return _border; }
        set
        {
            _border = value;
            Invalidate();
        }
    }

    private Color _progressColor = FromHex("#2c3e50");

    public Color ProgressColor
    {
        get { return _progressColor; }
        set
        {
            _progressColor = value;
            Invalidate();
        }
    }

    private int _val = 0;

    public int Value
    {
        get { return _val; }
        set
        {
            _val = value;
            ValChanged();
            Invalidate();
        }
    }

    private int _min = 0;

    public int Min
    {
        get { return _min; }
        set
        {
            _min = value;
            Invalidate();
        }
    }

    private int _max = 100;

    public int Max
    {
        get { return _max; }
        set
        {
            _max = value;
            Invalidate();
        }
    }

    private bool _showPercent = false;

    public bool ShowPercent
    {
        get { return _showPercent; }
        set
        {
            _showPercent = value;
            Invalidate();
        }
    }

    private void ValChanged()
    {
        if (_val > _max)
        {
            _val = _max;
        }
    }

    public VelocityProgressBar()
    {
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;

        if (_showPercent)
        {
            g.FillRectangle(new SolidBrush(FromHex("#506070")), 0, 0, Width - 35, Height - 1);
            g.FillRectangle(new SolidBrush(_progressColor), new Rectangle(0, 0, _val * (Width - 35) / (_max - _min), Height));
            g.DrawRectangle(new Pen(Color.Black), 0, 0, Width - 35, Height - 1);
            g.DrawString($"{_val}%", Font, new SolidBrush(ForeColor), Width - 30, (Height / 2) - (g.MeasureString($"{_val}%", Font).Height / 2) - 1);
        }
        else
        {
            g.Clear(FromHex("#506070"));
            g.FillRectangle(new SolidBrush(_progressColor), new Rectangle(0, 0, ((_val - 0) * (Width - 0) / (_max - _min)) + 0, Height));
            g.DrawRectangle(new Pen(Color.Black), 0, 0, Width - 1, Height - 1);
        }
    }
}

public class VelocityToggle : Control
{
    private bool _checked = false;

    public bool Checked
    {
        get { return _checked; }
        set
        {
            _checked = value;
            Invalidate();
        }
    }

    public VelocityToggle()
    {
        Size = new Size(50, 23);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(FromHex("#435363"));

        switch (_checked)
        {
            case true:
                g.FillRectangle(Brushes.White, Width - 19, Height - 19, 15, 15);
                break;
            case false:
                g.FillRectangle(new SolidBrush(FromHex("#2c3e50")), 4, 4, 15, 15);
                break;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _checked = !_checked;
        Invalidate();
    }
}

public class VelocityAlertNew : Control
{
    public event EventHandler XClicked;
    private bool _xHover = false;

    private bool _xChangeCursor = true;

    public bool XChangeCursor
    {
        get => _xChangeCursor;
        set
        {
            _xChangeCursor = value;
            Invalidate();
        }
    }

    private string _title = "";

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            Invalidate();
        }
    }

    private bool _exitButton = false;

    public bool ShowExit
    {
        get => _exitButton;
        set
        {
            _exitButton = value;
            Invalidate();
        }
    }

    private Color _border = Color.FromArgb(67, 83, 99);

    public Color Border
    {
        get => _border;
        set
        {
            _border = value;
            Invalidate();
        }
    }

    private Image _image;

    public Image Image
    {
        get => _image;
        set
        {
            _image = value;
            Invalidate();
        }
    }

    public VelocityAlertNew()
    {
        Size = new Size(370, 80);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        // Фон области изображения - белый
        g.FillRectangle(Brushes.White, 12, 8, 64, 64);

        // Рисуем изображение или оставляем черный квадрат
        if (_image != null)
        {
            g.DrawImage(_image, 12, 8, 64, 64);
        }
        else
        {
            g.DrawRectangle(Pens.Black, 12, 8, 64, 64); // Контур
        }

        // Добавляем черный текст, выровненный по центру
        string text = "64x64";
        SizeF textSize = g.MeasureString(text, new Font("Segoe UI", 12));
        float textX = 12 + (64 - textSize.Width) / 2; // Центруем текст
        float textY = 8 + (64 - textSize.Height) / 2; // Центруем текст
        g.DrawString(text, new Font("Segoe UI", 12), Brushes.Black, new PointF(textX, textY));

        // Рисуем заголовок и текст
        g.DrawString(_title, new Font("Segoe UI Semilight", 14), new SolidBrush(ForeColor), 84, 6);
        g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(86, 33, Width - 88, Height - 10));

        // Кнопка закрытия
        if (ShowExit)
        {
            var exitPosition = new Point(Width - 18, 4);
            var exitColor = _xHover ? Color.FromArgb(89, 99, 114) : _border;
            g.DrawString("r", new Font("Marlett", 9), new SolidBrush(exitColor), exitPosition);
        }

        g.DrawRectangle(new Pen(_border), 0, 0, Width - 1, Height - 1);
        g.FillRectangle(new SolidBrush(_border), 0, 0, 6, Height);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_exitButton)
        {
            if (new Rectangle(Width - 16, 4, 12, 13).Contains(e.Location))
            {
                _xHover = true;
                if (_xChangeCursor)
                {
                    Cursor = Cursors.Hand;
                }
            }
            else
            {
                _xHover = false;
                Cursor = Cursors.Default;
            }
        }
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_exitButton && _xHover)
        {
            XClicked?.Invoke(this, EventArgs.Empty);
        }
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
}


