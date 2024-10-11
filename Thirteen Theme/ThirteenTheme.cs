using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class ThirteenForm : ContainerControl
{
    public enum ColorSchemes
    {
        Light,
        Dark
    }

    public event EventHandler ColorSchemeChanged;

    private ColorSchemes _ColorScheme;
    public ColorSchemes ColorScheme
    {
        get { return _ColorScheme; }
        set
        {
            _ColorScheme = value;
            ColorSchemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public ThirteenForm()
    {
        DoubleBuffered = true;
        Font = new Font("Segoe UI Semilight", 9.75F);
        AccentColor = Color.DodgerBlue;
        ColorScheme = ColorSchemes.Dark;
        ForeColor = Color.White;
        BackColor = Color.FromArgb(50, 50, 50);
        MoveHeight = 32;
    }

    private Color _AccentColor;
    public Color AccentColor
    {
        get { return _AccentColor; }
        set
        {
            _AccentColor = value;
            Invalidate();
            AccentColorChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public event EventHandler AccentColorChanged;

    private Point MouseP = new Point(0, 0);
    private bool Cap = false;
    private int MoveHeight;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, MoveHeight).Contains(e.Location))
        {
            Cap = true;
            MouseP = e.Location;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (Cap)
        {
            Parent.Location = new Point(MousePosition.X - MouseP.X, MousePosition.Y - MouseP.Y);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Cap = false;
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        Dock = DockStyle.Fill;
        Parent.FindForm().FormBorderStyle = FormBorderStyle.None;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bmp = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bmp);
        g.Clear(BackColor);
        g.DrawLine(new Pen(AccentColor, 2), new Point(0, 30), new Point(Width, 30));
        g.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(8, 6, Width - 1, Height - 1), StringFormat.GenericDefault);
        g.DrawLine(new Pen(AccentColor, 3), new Point(8, 27), new Point((int)(8 + g.MeasureString(Text, Font).Width), 27));
        g.DrawRectangle(new Pen(Color.FromArgb(100, 100, 100)), new Rectangle(0, 0, Width - 1, Height - 1));
        e.Graphics.DrawImage(bmp, new Point(0, 0));
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }
}

public class ThirteenControlBox : Control
{
    public enum ColorSchemes
    {
        Light,
        Dark
    }

    public event EventHandler ColorSchemeChanged;

    private ColorSchemes _ColorScheme;
    public ColorSchemes ColorScheme
    {
        get { return _ColorScheme; }
        set
        {
            _ColorScheme = value;
            ColorSchemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private Color _AccentColor;
    public Color AccentColor
    {
        get { return _AccentColor; }
        set
        {
            _AccentColor = value;
            Invalidate();
        }
    }

    private ButtonHover ButtonState = ButtonHover.None;
    private bool _isCloseButtonPressed = false; // Новое поле для отслеживания состояния

    private enum ButtonHover
    {
        Minimize,
        Maximize,
        Close,
        None
    }

    public ThirteenControlBox()
    {
        TabStop = false;
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        ForeColor = Color.White;
        BackColor = Color.FromArgb(50, 50, 50);
        AccentColor = Color.DodgerBlue;
        ColorScheme = ColorSchemes.Dark;
        Anchor = AnchorStyles.Top | AnchorStyles.Right;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(100, 25);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        int x = e.Location.X;
        int y = e.Location.Y;

        if (y > 0 && y < (Height - 2))
        {
            if (x > 0 && x < 34) ButtonState = ButtonHover.Minimize;
            else if (x > 33 && x < 65) ButtonState = ButtonHover.Maximize;
            else if (x > 64 && x < Width) ButtonState = ButtonHover.Close;
            else ButtonState = ButtonHover.None;
        }
        else
        {
            ButtonState = ButtonHover.None;
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bmp = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bmp);
        g.Clear(BackColor);

        Color buttonColor = _AccentColor;

        // Определяем цвет кнопки закрытия на основе состояния
        if (ButtonState == ButtonHover.Close && _isCloseButtonPressed)
            buttonColor = Color.Red; // Красный цвет при нажатии на кнопку закрытия

        switch (ButtonState)
        {
            case ButtonHover.Minimize:
            case ButtonHover.Maximize:
            case ButtonHover.Close:
                g.FillRectangle(new SolidBrush(buttonColor), new Rectangle(ButtonState == ButtonHover.Minimize ? 3 : (ButtonState == ButtonHover.Maximize ? 34 : 65), 0, ButtonState == ButtonHover.Close ? 35 : 30, Height));
                break;
        }

        using (Font buttonFont = new Font("Marlett", 9.75F))
        {
            g.DrawString("r", buttonFont, new SolidBrush(Color.FromArgb(200, 200, 200)), new Point(Width - 16, 7), new StringFormat { Alignment = StringAlignment.Center });
            g.DrawString(Parent.FindForm().WindowState == FormWindowState.Maximized ? "2" : "1", buttonFont, new SolidBrush(Color.FromArgb(200, 200, 200)), new Point(51, 7), new StringFormat { Alignment = StringAlignment.Center });
            g.DrawString("0", buttonFont, new SolidBrush(Color.FromArgb(200, 200, 200)), new Point(20, 7), new StringFormat { Alignment = StringAlignment.Center });
        }

        e.Graphics.DrawImage(bmp, new Point(0, 0));
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        var form = Parent.FindForm();
        switch (ButtonState)
        {
            case ButtonHover.Close:
                form.Close();
                break;
            case ButtonHover.Minimize:
                form.WindowState = FormWindowState.Minimized;
                break;
            case ButtonHover.Maximize:
                form.WindowState = form.WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
                break;
        }

        // Сбрасываем состояние кнопки закрытия
        _isCloseButtonPressed = false;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        ButtonState = ButtonHover.None;
        _isCloseButtonPressed = false; // Сброс состояния при выходе мыши
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (ButtonState == ButtonHover.Close)
        {
            _isCloseButtonPressed = true; // Отметка нажатия кнопки закрытия
            Invalidate();
        }
    }
}

public class ThirteenButton : Button
{
    private enum MouseState
    {
        None,
        Over,
        Down
    }

    public enum ColorSchemes
    {
        Light,
        Dark
    }

    private ColorSchemes _ColorScheme;

    public ColorSchemes ColorScheme
    {
        get { return _ColorScheme; }
        set
        {
            _ColorScheme = value;
            Invalidate();
        }
    }

    private MouseState State = MouseState.None;

    private Color _AccentColor;
    public Color AccentColor
    {
        get { return _AccentColor; }
        set
        {
            _AccentColor = value;
            Invalidate();
        }
    }

    public ThirteenButton()
    {
        TabStop = false;
        Font = new Font("Segoe UI Semilight", 9.75F);
        ForeColor = Color.White;
        BackColor = Color.FromArgb(50, 50, 50);
        AccentColor = Color.DodgerBlue;
        ColorScheme = ColorSchemes.Dark;
        AutoSize = true;
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

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bmp = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bmp);
        Color BGColor = ColorScheme == ColorSchemes.Dark ? Color.FromArgb(50, 50, 50) : Color.White;

        switch (State)
        {
            case MouseState.None:
                g.Clear(BGColor);
                break;
            case MouseState.Over:
                g.Clear(AccentColor);
                break;
            case MouseState.Down:
                g.Clear(AccentColor);
                g.FillRectangle(new SolidBrush(Color.FromArgb(50, Color.Black)), new Rectangle(0, 0, Width - 1, Height - 1));
                break;
        }

        g.DrawRectangle(new Pen(Color.FromArgb(100, 100, 100)), new Rectangle(0, 0, Width - 1, Height - 1));

        var ButtonString = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(Text, Font, ColorScheme == ColorSchemes.Dark ? Brushes.White : Brushes.Black, new Rectangle(0, 0, Width - 1, Height - 1), ButtonString);

        e.Graphics.DrawImage(bmp, new Point(0, 0));
    }
}

public class ThirteenTextBox : TextBox
{
    public enum ColorSchemes
    {
        Light,
        Dark
    }

    public event EventHandler ColorSchemeChanged;

    private ColorSchemes _ColorScheme;
    public ColorSchemes ColorScheme
    {
        get { return _ColorScheme; }
        set
        {
            _ColorScheme = value;
            ColorSchemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public ThirteenTextBox()
    {
        TabStop = false;
        AutoSize = true;
        BorderStyle = BorderStyle.FixedSingle;
        Font = new Font("Segoe UI Semilight", 9.75F);
        BackColor = Color.FromArgb(35, 35, 35);
        ForeColor = Color.White;
        ColorScheme = ColorSchemes.Dark;
        
    }

    protected void OnColorSchemeChanged()
    {
        Invalidate();
        switch (ColorScheme)
        {
            case ColorSchemes.Dark:
                BackColor = Color.FromArgb(35, 35, 35);
                ForeColor = Color.White;
                break;
            case ColorSchemes.Light:
                BackColor = Color.White;
                ForeColor = Color.Black;
                break;
        }
    }
}

public class ThirteenTabControl : TabControl
{
    private Color _AccentColor = Color.DodgerBlue; // Цвет выделенной вкладки
    private Color _TabTextColor = Color.White; // Цвет текста вкладок
    private Color _TabBackgroundColor = Color.FromArgb(35, 35, 35); // Фон вкладок
    private Color _SelectedTabColor = Color.DodgerBlue; // Цвет выделенной вкладки
    private Color _HeaderColor = Color.FromArgb(45, 45, 48); // Цвет заголовка

    public Color AccentColor
    {
        get => _AccentColor;
        set
        {
            _AccentColor = value;
            Invalidate();
        }
    }

    public Color TabTextColor
    {
        get => _TabTextColor;
        set
        {
            _TabTextColor = value;
            Invalidate();
        }
    }

    public Color TabBackgroundColor
    {
        get => _TabBackgroundColor;
        set
        {
            _TabBackgroundColor = value;
            Invalidate();
        }
    }

    public Color SelectedTabColor
    {
        get => _SelectedTabColor;
        set
        {
            _SelectedTabColor = value;
            Invalidate();
        }
    }

    public Color HeaderColor
    {
        get => _HeaderColor;
        set
        {
            _HeaderColor = value;
            Invalidate();
        }
    }

    public ThirteenTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer, true);
        DoubleBuffered = true;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bmp = new Bitmap(Width, Height);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            // Рисование фона заголовка
            g.FillRectangle(new SolidBrush(HeaderColor), new Rectangle(0, 0, Width, 30));

            // Рисование фона вкладок
            g.Clear(TabBackgroundColor);

            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle tabRect = new Rectangle(GetTabRect(i).X, GetTabRect(i).Y + 3, GetTabRect(i).Width + 2, GetTabRect(i).Height);
                g.FillRectangle(new SolidBrush(TabBackgroundColor), tabRect); // Фон заголовка вкладок

                using (var font = new Font("Segoe UI Semilight", 9.75F))
                {
                    g.DrawString(TabPages[i].Text, font, new SolidBrush(TabTextColor), tabRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }
            }

            if (SelectedIndex >= 0 && SelectedIndex < TabPages.Count)
            {
                Rectangle selectedTabRect = new Rectangle(GetTabRect(SelectedIndex).X - 2, GetTabRect(SelectedIndex).Y, GetTabRect(SelectedIndex).Width + 4, GetTabRect(SelectedIndex).Height);
                g.FillRectangle(new SolidBrush(AccentColor), selectedTabRect);
                using (var font = new Font("Segoe UI Semilight", 9.75F))
                {
                    g.DrawString(TabPages[SelectedIndex].Text, font, new SolidBrush(TabTextColor), selectedTabRect, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }

            e.Graphics.DrawImage(bmp, new Point(0, 0));
        }
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        base.OnSelectedIndexChanged(e);
        Invalidate(); // Перерисовываем при изменении выбранной вкладки
    }
}

public class ThirteenComboBox : ComboBox
{
    public enum ColorSchemes
    {
        Light,
        Dark
    }

    private ColorSchemes _colorScheme;
    public ColorSchemes ColorScheme
    {
        get { return _colorScheme; }
        set
        {
            _colorScheme = value;
            Invalidate();
        }
    }

    private Color _accentColor;
    public Color AccentColor
    {
        get { return _accentColor; }
        set
        {
            _accentColor = value;
            Invalidate();
        }
    }

    public new int SelectedIndex
    {
        get { return base.SelectedIndex; }
        set
        {
            if (value >= -1 && value < Items.Count)
            {
                base.SelectedIndex = value;
                Invalidate();
            }
        }
    }

    public ThirteenComboBox()
    {
        DrawMode = DrawMode.OwnerDrawFixed;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        BackColor = Color.FromArgb(50, 50, 50);
        ForeColor = Color.White;
        AccentColor = Color.DodgerBlue;
        ColorScheme = ColorSchemes.Dark;
        DropDownStyle = ComboBoxStyle.DropDownList;
        Font = new Font("Segoe UI Semilight", 9.75F);
        DoubleBuffered = true;

        base.SelectedIndex = -1;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(ColorScheme == ColorSchemes.Dark ? Color.FromArgb(50, 50, 50) : Color.White);

        using (Pen borderPen = new Pen(Color.FromArgb(100, 100, 100)))
        {
            e.Graphics.DrawRectangle(borderPen, new Rectangle(0, 0, Width - 1, Height - 1));
        }

        string text = SelectedItem != null ? SelectedItem.ToString() : string.Empty;
        using (Brush textBrush = new SolidBrush(ColorScheme == ColorSchemes.Dark ? Color.White : Color.Black))
        {
            e.Graphics.DrawString(text, Font, textBrush, new Rectangle(7, (Height - Font.Height) / 2, Width - 20, Font.Height), StringFormat.GenericDefault);
        }

        using (Pen arrowPen = new Pen(Color.White, 2))
        {
            e.Graphics.DrawLine(arrowPen, new Point(Width - 18, 10), new Point(Width - 14, 14));
            e.Graphics.DrawLine(arrowPen, new Point(Width - 14, 14), new Point(Width - 10, 10));
        }
    }

    protected override void OnDropDown(EventArgs e)
    {
        base.OnDropDown(e);
        this.DropDownHeight = Math.Min(Items.Count * 20, 200);
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        if (e.Index < 0) return;

        if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
        {
            e.Graphics.FillRectangle(new SolidBrush(AccentColor), e.Bounds);
        }
        else
        {
            e.Graphics.FillRectangle(new SolidBrush(ColorScheme == ColorSchemes.Dark ? Color.FromArgb(50, 50, 50) : Color.White), e.Bounds);
        }

        using (Brush textBrush = new SolidBrush(ColorScheme == ColorSchemes.Dark ? Color.White : Color.Black))
        {
            e.Graphics.DrawString(Items[e.Index].ToString(), e.Font, textBrush, e.Bounds);
        }

        e.DrawFocusRectangle();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            DroppedDown = !DroppedDown; // Переключить состояние выпадающего списка
        }
    }

    protected override void OnSelectedIndexChanged(EventArgs e)
    {
        // Отключаем реакцию на выделение элементов мышью
        if (DroppedDown)
        {
            DroppedDown = false; // Закрыть список, если он открыт
        }
        base.OnSelectedIndexChanged(e); // Вызов базового метода, если нужно
    }
}

//public class ThirteenComboBox : ComboBox
//{
//    public enum ColorSchemes
//    {
//        Light,
//        Dark
//    }

//    private ColorSchemes _ColorScheme;
//    public ColorSchemes ColorScheme
//    {
//        get { return _ColorScheme; }
//        set
//        {
//            _ColorScheme = value;
//            Invalidate();
//        }
//    }

//    private Color _AccentColor;
//    public Color AccentColor
//    {
//        get { return _AccentColor; }
//        set
//        {
//            _AccentColor = value;
//            Invalidate();
//        }
//    }

//    private int _StartIndex = 0;

//    public new int SelectedIndex
//    {
//        get { return _StartIndex; }
//        set
//        {
//            if (value >= 0 && value < Items.Count) // индекс не выходит за границы.
//            {
//                _StartIndex = value;
//                Invalidate(); // Обновляем отображение ComboBox
//            }
//        }
//    }

//    public ThirteenComboBox()
//    {
//        TabStop = false;
//        DrawMode = DrawMode.OwnerDrawFixed;
//        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
//        BackColor = Color.FromArgb(50, 50, 50);
//        ForeColor = Color.White;
//        AccentColor = Color.DodgerBlue;
//        ColorScheme = ColorSchemes.Dark;
//        DropDownStyle = ComboBoxStyle.DropDownList;
//        Font = new Font("Segoe UI Semilight", 9.75F);
//        SelectedIndex = _StartIndex;
//        DoubleBuffered = true;
//    }


//    protected override void OnPaint(PaintEventArgs e)
//    {
//        using Bitmap bmp = new Bitmap(Width, Height);
//        using Graphics g = Graphics.FromImage(bmp);
//        g.SmoothingMode = SmoothingMode.HighQuality;

//        g.Clear(ColorScheme == ColorSchemes.Dark ? Color.FromArgb(50, 50, 50) : Color.White);
//        g.DrawLine(new Pen(Color.White, 2), new Point(Width - 18, 10), new Point(Width - 14, 14));
//        g.DrawLine(new Pen(Color.White, 2), new Point(Width - 14, 14), new Point(Width - 10, 10));
//        g.DrawLine(new Pen(Color.White), new Point(Width - 14, 15), new Point(Width - 14, 14));

//        g.DrawRectangle(new Pen(Color.FromArgb(100, 100, 100)), new Rectangle(0, 0, Width - 1, Height - 1));

//        try
//        {
//            g.DrawString(Text, Font, ColorScheme == ColorSchemes.Dark ? Brushes.White : Brushes.Black, new Rectangle(7, 0, Width - 1, Height - 1), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });
//        }
//        catch { }

//        e.Graphics.DrawImage((Image)bmp.Clone(), 0, 0);
//    }
//}

public enum MouseState
{
    None,
    Over,
    Down
}

//[DefaultEvent("CheckedChanged")]
//[DefaultProperty("Checked")]
//public class ThirteenRadioButton : Control
//{
//    private enum MouseState { None, Over, Down }

//    public enum ColorSchemes { Dark, Light }

//    private MouseState _state = MouseState.None;
//    private ColorSchemes _colorScheme;

//    public ColorSchemes ColorScheme
//    {
//        get { return _colorScheme; }
//        set
//        {
//            _colorScheme = value;
//            Invalidate();
//        }
//    }

//    private bool _checked;

//    [Category("Behavior")]
//    [Description("Indicates whether the radio button is checked.")]
//    public bool Checked
//    {
//        get { return _checked; }
//        set
//        {
//            if (_checked != value)
//            {
//                _checked = value;
//                InvalidateControls();
//                RaiseEventCheckedChanged();
//                Invalidate();
//            }
//        }
//    }

//    public event EventHandler CheckedChanged;

//    public ThirteenRadioButton()
//    {
//        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
//        ColorScheme = ColorSchemes.Dark; // По умолчанию темная схема
//        BackColor = Color.FromArgb(50, 50, 50);
//        ForeColor = Color.White;
//        Size = new Size(177, 18);
//    }

//    protected override void OnMouseEnter(EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        _state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseLeave(EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        _state = MouseState.None;
//        Invalidate();
//    }

//    protected override void OnMouseDown(MouseEventArgs e)
//    {
//        base.OnMouseDown(e);
//        _state = MouseState.Down;
//        Invalidate();
//    }

//    protected override void OnMouseUp(MouseEventArgs e)
//    {
//        base.OnMouseUp(e);
//        _state = MouseState.None;
//        Invalidate();
//    }

//    protected override void OnClick(EventArgs e)
//    {
//        Checked = true;
//        base.OnClick(e);
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        using (Bitmap b = new Bitmap(Width, Height))
//        using (Graphics g = Graphics.FromImage(b))
//        {
//            g.SmoothingMode = SmoothingMode.HighQuality;
//            g.Clear(BackColor);

//            // Отрисовка радио-кнопки
//            Rectangle radioBtnRectangle = new Rectangle(0, 0, Height - 1, Height - 1);
//            Color borderColor = _colorScheme == ColorSchemes.Dark ? Color.FromArgb(50, 50, 50) : Color.White;

//            // Изменение цвета границы в зависимости от состояния мыши
//            if (_state == MouseState.Over)
//                borderColor = Color.FromArgb(200, borderColor); // Более яркий цвет при наведении

//            g.FillEllipse(new SolidBrush(borderColor), radioBtnRectangle);

//            // Отрисовка внутреннего круга, если кнопка отмечена
//            if (Checked)
//            {
//                Color innerColor = _colorScheme == ColorSchemes.Light ? Color.White : Color.FromArgb(50, 50, 50);
//                g.FillEllipse(new SolidBrush(innerColor), new Rectangle(4, 4, Height - 9, Height - 9));
//            }

//            // Отрисовка текста
//            g.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(22, 1));
//            e.Graphics.DrawImage(b, 0, 0);
//        }
//    }

//    private void InvalidateControls()
//    {
//        if (!IsHandleCreated || !Checked) return;

//        foreach (Control control in Parent.Controls)
//        {
//            if (control is ThirteenRadioButton radioButton && radioButton != this)
//            {
//                radioButton.Checked = false; // Снимаем выделение с других радиокнопок
//            }
//        }
//    }

//    private void RaiseEventCheckedChanged()
//    {
//        CheckedChanged?.Invoke(this, EventArgs.Empty);
//    }
//}

//[DefaultEvent("CheckedChanged")]
//public class ThirteenCheckBox : Control
//{
//    public enum ColorSchemes { Light, Dark }
//    public enum MouseState { None, Over, Down }

//    private ColorSchemes _colorScheme;
//    private MouseState _state = MouseState.None;
//    private bool _checked;

//    public ColorSchemes ColorScheme
//    {
//        get => _colorScheme;
//        set
//        {
//            _colorScheme = value;
//            Invalidate();
//        }
//    }

//    public bool Checked
//    {
//        get => _checked;
//        set
//        {
//            if (_checked != value)
//            {
//                _checked = value;
//                Invalidate();
//                CheckedChanged?.Invoke(this, System.EventArgs.Empty);
//            }
//        }
//    }

//    public event System.EventHandler CheckedChanged;

//    public ThirteenCheckBox()
//    {
//        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
//                 ControlStyles.OptimizedDoubleBuffer, true);
//        ColorScheme = ColorSchemes.Dark;
//        BackColor = System.Drawing.Color.FromArgb(50, 50, 50);
//        ForeColor = System.Drawing.Color.White;
//        Size = new System.Drawing.Size(147, 17);
//        Height = 17; // Зафиксируем высоту
//    }

//    protected override void OnMouseEnter(System.EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        _state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
//    {
//        base.OnMouseDown(e);
//        _state = MouseState.Down;
//        Invalidate();
//    }

//    protected override void OnMouseLeave(System.EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        _state = MouseState.None;
//        Invalidate();
//    }

//    protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
//    {
//        base.OnMouseUp(e);
//        _state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnTextChanged(System.EventArgs e)
//    {
//        base.OnTextChanged(e);
//        Width = (int)CreateGraphics().MeasureString(Text, Font).Width + (2 * 3) + Height;
//        Invalidate();
//    }

//    protected override void OnResize(System.EventArgs e)
//    {
//        base.OnResize(e);
//        Height = 17; // Зафиксируем высоту
//    }

//    protected override void OnClick(System.EventArgs e)
//    {
//        Checked = !Checked;
//        base.OnClick(e);
//    }

//    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
//    {
//        using (var b = new System.Drawing.Bitmap(Width, Height))
//        using (var g = System.Drawing.Graphics.FromImage(b))
//        {
//            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
//            var checkBoxRectangle = new System.Drawing.Rectangle(0, 0, Height - 1, Height - 1);

//            g.Clear(BackColor);

//            // Изменяем цвет фона в зависимости от состояния мыши
//            System.Drawing.Color fillColor = ColorScheme == ColorSchemes.Dark ? System.Drawing.Color.White : System.Drawing.Color.Black;

//            if (_state == MouseState.Over)
//                fillColor = System.Drawing.Color.FromArgb(100, fillColor); // Прозрачность при наведении
//            else if (_state == MouseState.Down)
//                fillColor = System.Drawing.Color.FromArgb(150, fillColor); // Прозрачность при нажатии

//            g.FillRectangle(new System.Drawing.SolidBrush(fillColor), checkBoxRectangle);

//            if (Checked)
//            {
//                var chkPoly = new System.Drawing.Rectangle(checkBoxRectangle.X + checkBoxRectangle.Width / 4,
//                                                            checkBoxRectangle.Y + checkBoxRectangle.Height / 4,
//                                                            checkBoxRectangle.Width / 2,
//                                                            checkBoxRectangle.Height / 2);
//                var poly = new System.Drawing.Point[]
//                {
//                    new System.Drawing.Point(chkPoly.X, chkPoly.Y + chkPoly.Height / 2),
//                    new System.Drawing.Point(chkPoly.X + chkPoly.Width / 2, chkPoly.Y + chkPoly.Height),
//                    new System.Drawing.Point(chkPoly.X + chkPoly.Width, chkPoly.Y)
//                };

//                var penColor = (ColorScheme == ColorSchemes.Dark) ? System.Drawing.Color.Black : System.Drawing.Color.White;
//                using (var pen = new System.Drawing.Pen(penColor, 2))
//                {
//                    for (int i = 0; i < poly.Length - 1; i++)
//                        g.DrawLine(pen, poly[i], poly[i + 1]);
//                }
//            }

//            g.DrawString(Text, Font, new System.Drawing.SolidBrush(ForeColor), new System.Drawing.Point(18, 2));

//            e.Graphics.DrawImage(b, 0, 0);
//        }
//    }
//}

[DefaultEvent("CheckedChanged")]
[DefaultProperty("Checked")]
public class ThirteenRadioButton : Control
{
    private enum MouseState { None, Over, Down }
    public enum ColorSchemes { Dark, Light }

    private MouseState _state = MouseState.None;
    private ColorSchemes _colorScheme;
    private bool _checked;

    public ColorSchemes ColorScheme
    {
        get { return _colorScheme; }
        set
        {
            _colorScheme = value;
            Invalidate();
        }
    }

    [Category("Behavior")]
    [Description("Indicates whether the radio button is checked.")]
    public bool Checked
    {
        get { return _checked; }
        set
        {
            if (_checked != value)
            {
                _checked = value;
                InvalidateControls();
                RaiseEventCheckedChanged();
                Invalidate();
            }
        }
    }

    public event EventHandler CheckedChanged;

    public ThirteenRadioButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
        ColorScheme = ColorSchemes.Dark;
        BackColor = Color.FromArgb(50, 50, 50);
        ForeColor = Color.White;
        Size = new Size(177, 18);
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _state = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _state = MouseState.Over;
        Invalidate();
        Checked = true; // Устанавливаем Checked при отпускании кнопки мыши
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        Checked = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap b = new Bitmap(Width, Height))
        using (Graphics g = Graphics.FromImage(b))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(BackColor);

            Rectangle radioBtnRectangle = new Rectangle(0, 0, Height - 1, Height - 1);
            Color borderColor = Color.FromArgb(50, 50, 50);

            if (_state == MouseState.Over)
                borderColor = Color.FromArgb(200, borderColor); // Ярче при наведении

            g.FillEllipse(new SolidBrush(borderColor), radioBtnRectangle);

            if (Checked)
            {
                Color innerColor = Color.White;
                g.FillEllipse(new SolidBrush(innerColor), new Rectangle(4, 4, Height - 9, Height - 9));
            }

            g.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(22, 1));
            e.Graphics.DrawImage(b, 0, 0);
        }
    }

    private void InvalidateControls()
    {
        if (!IsHandleCreated || !Checked) return;

        foreach (Control control in Parent.Controls)
        {
            if (control is ThirteenRadioButton radioButton && radioButton != this)
            {
                radioButton.Checked = false; // Снять выделение с других радио-кнопок
            }
        }
    }

    private void RaiseEventCheckedChanged()
    {
        CheckedChanged?.Invoke(this, EventArgs.Empty);
    }
}

[DefaultEvent("CheckedChanged")]
public class ThirteenCheckBox : Control
{
    public enum ColorSchemes { Light, Dark }
    public enum MouseState { None, Over, Down }

    private ColorSchemes _colorScheme;
    private MouseState _state = MouseState.None;
    private bool _checked;

    public ColorSchemes ColorScheme
    {
        get { return _colorScheme; }
        set
        {
            _colorScheme = value;
            Invalidate();
        }
    }

    public bool Checked
    {
        get { return _checked; }
        set
        {
            if (_checked != value)
            {
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler CheckedChanged;

    public ThirteenCheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        ColorScheme = ColorSchemes.Light;
        BackColor = Color.FromArgb(50, 50, 50);
        ForeColor = Color.White;
        Size = new Size(147, 17);
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

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        _state = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _state = MouseState.Over;
        Checked = !Checked; // Изменение состояния при отпускании кнопки
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var b = new Bitmap(Width, Height))
        using (var g = Graphics.FromImage(b))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            var checkBoxRectangle = new Rectangle(0, 0, Height - 1, Height - 1);

            g.Clear(BackColor);

            Color fillColor = (ColorScheme == ColorSchemes.Light) ? Color.White : Color.Black;

            if (_state == MouseState.Over)
                fillColor = Color.FromArgb(100, fillColor);
            else if (_state == MouseState.Down)
                fillColor = Color.FromArgb(150, fillColor);

            g.FillRectangle(new SolidBrush(fillColor), checkBoxRectangle);

            if (Checked)
            {
                var chkPoly = new Rectangle(checkBoxRectangle.X + checkBoxRectangle.Width / 4,
                                             checkBoxRectangle.Y + checkBoxRectangle.Height / 4,
                                             checkBoxRectangle.Width / 2,
                                             checkBoxRectangle.Height / 2);
                var poly = new Point[]
                {
                    new Point(chkPoly.X, chkPoly.Y + chkPoly.Height / 2),
                    new Point(chkPoly.X + chkPoly.Width / 2, chkPoly.Y + chkPoly.Height),
                    new Point(chkPoly.X + chkPoly.Width, chkPoly.Y)
                };

                var penColor = (ColorScheme == ColorSchemes.Light) ? Color.Black : Color.White;
                using (var pen = new Pen(penColor, 2))
                {
                    for (int i = 0; i < poly.Length - 1; i++)
                        g.DrawLine(pen, poly[i], poly[i + 1]);
                }
            }

            g.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(18, 2));
            e.Graphics.DrawImage(b, 0, 0);
        }
    }
}
