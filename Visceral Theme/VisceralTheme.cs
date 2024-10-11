using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

public static class Draw
{
    public static GraphicsPath RoundRect(Rectangle rectangle, int curve)
    {
        GraphicsPath path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;
        path.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        path.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        path.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 0, 90);
        path.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 90, 90);
        path.AddLine(new Point(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y), new Point(rectangle.X, curve + rectangle.Y));
        return path;
    }

    public static GraphicsPath RoundRect(int x, int y, int width, int height, int curve)
    {
        return RoundRect(new Rectangle(x, y, width, height), curve);
    }
}

public class VisceralButton : Control
{
    private MouseState state = MouseState.None;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        state = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        state = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        state = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        state = MouseState.None;
        Invalidate();
    }

    public VisceralButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
        ForeColor = Color.FromKnownColor(KnownColor.ControlLight);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        Rectangle clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);

        base.OnPaint(e);

        g.Clear(BackColor);
        Font drawFont = new Font("Arial", 8, FontStyle.Bold);
        LinearGradientBrush gloss;
        LinearGradientBrush lgb;
        switch (state)
        {
            case MouseState.None:
                lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(61, 61, 63), Color.FromArgb(14, 14, 14), 90F);
                g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                gloss = new LinearGradientBrush(new Rectangle(0, 0, Width - 1, Height / 2), Color.FromArgb(100, Color.FromArgb(61, 61, 63)), Color.FromArgb(12, 255, 255, 255), 90F);
                g.FillPath(gloss, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height / 2), 3));
                g.DrawPath(Pens.Black, Draw.RoundRect(clientRectangle, 3));
                break;

            case MouseState.Over:
                lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(245, 61, 61, 63), Color.FromArgb(245, 14, 14, 14), 90F);
                g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                gloss = new LinearGradientBrush(new Rectangle(0, 0, Width - 1, Height / 2), Color.FromArgb(75, Color.FromArgb(61, 61, 63)), Color.FromArgb(20, 255, 255, 255), 90F);
                g.FillPath(gloss, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height / 2), 3));
                g.DrawPath(Pens.Black, Draw.RoundRect(clientRectangle, 3));
                break;

            case MouseState.Down:
                lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(51, 51, 53), Color.FromArgb(4, 4, 4), 90F);
                g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                gloss = new LinearGradientBrush(new Rectangle(0, 0, Width - 1, Height / 2), Color.FromArgb(75, Color.FromArgb(61, 61, 63)), Color.FromArgb(5, 255, 255, 255), 90F);
                g.FillPath(gloss, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height / 2), 3));
                g.DrawPath(Pens.Black, Draw.RoundRect(clientRectangle, 3));
                break;
        }

        g.DrawString(Text, drawFont, new SolidBrush(ForeColor), new Rectangle(0, 0, Width - 1, Height - 1), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

        e.Graphics.DrawImage((Image)bitmap.Clone(), 0, 0);
    }
}

public class VisceralTheme : ContainerControl
{
    private Color titleForeColor = Color.White; // Цвет текста заголовка

    public Color TitleForeColor
    {
        get { return titleForeColor; }
        set { titleForeColor = value; Invalidate(); }
    }

    public VisceralTheme()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.FromArgb(25, 25, 25);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        Rectangle topBar = new Rectangle(0, 0, Width, 30);
        Rectangle body = new Rectangle(0, 10, Width, Height - 10);

        base.OnPaint(e);

        // Заливаем фон текущим цветом
        g.Clear(Color.FromArgb(25, 25, 25));  // Цвет фона формы

        using (LinearGradientBrush lbb = new LinearGradientBrush(body, Color.FromArgb(19, 19, 19), Color.FromArgb(17, 17, 17), 90F))
        using (HatchBrush bodyHatch = new HatchBrush(HatchStyle.DarkUpwardDiagonal, Color.FromArgb(20, 20, 20), Color.Transparent))
        {
            g.FillRectangle(lbb, body);  // Заполнение градиентом прямоугольника
            g.FillRectangle(bodyHatch, body); // Заполнение текстурой
            g.DrawRectangle(Pens.Black, body); // Рисуем границу
        }

        using (LinearGradientBrush lgb = new LinearGradientBrush(topBar, Color.FromArgb(60, 60, 62), Color.FromArgb(25, 25, 25), 90F))
        {
            g.FillRectangle(lgb, topBar); // Заполнение верхней панели
            g.DrawRectangle(Pens.Black, topBar); // Рисуем границу
        }

        // Используем только TitleForeColor для текста заголовка
        g.DrawString(Text, Font, new SolidBrush(titleForeColor), new Rectangle(33, 0, Width - 1, 30), new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
        g.DrawIcon(FindForm().Icon, new Rectangle(11, 8, 16, 16));

        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    private Point mousePoint = new Point(0, 0);
    private bool isDragging = false;
    private readonly int moveHeight = 30;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, moveHeight).Contains(e.Location))
        {
            isDragging = true;
            mousePoint = e.Location;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        isDragging = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (isDragging)
        {
            Parent.Location = new Point(MousePosition.X - mousePoint.X, MousePosition.Y - mousePoint.Y);
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ParentForm.FormBorderStyle = FormBorderStyle.None;
        ParentForm.TransparencyKey = Color.Fuchsia;
        Dock = DockStyle.Fill;
    }
}

public class VisceralTextBox : Control
{
    private readonly TextBox txtbox = new TextBox();
    private bool _passmask = false;
    private int _maxchars = 32767;
    private HorizontalAlignment _align;

    public bool UseSystemPasswordChar
    {
        get => _passmask;
        set
        {
            txtbox.UseSystemPasswordChar = value;
            _passmask = value;
            Invalidate();
        }
    }

    public int MaxLength
    {
        get => _maxchars;
        set
        {
            _maxchars = value;
            txtbox.MaxLength = _maxchars;
            Invalidate();
        }
    }

    public HorizontalAlignment TextAlignment
    {
        get => _align;
        set
        {
            _align = value;
            Invalidate();
        }
    }

    public VisceralTextBox()
    {
        InitializeTextBox();
        Controls.Add(txtbox);

        Text = string.Empty;
        BackColor = Color.FromArgb(15, 15, 15);
        ForeColor = Color.Silver;
        Size = new Size(135, 35);
        DoubleBuffered = true;
    }

    private void InitializeTextBox()
    {
        txtbox.Multiline = false;
        txtbox.BackColor = Color.FromArgb(43, 43, 43);
        txtbox.ForeColor = ForeColor;
        txtbox.Text = string.Empty;
        txtbox.TextAlign = HorizontalAlignment.Center;
        txtbox.BorderStyle = BorderStyle.None;
        txtbox.Location = new Point(5, 4);
        txtbox.Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
        txtbox.Size = new Size(Width - 10, Height - 11);
        txtbox.UseSystemPasswordChar = UseSystemPasswordChar;

        txtbox.TextChanged += (s, e) => { Text = txtbox.Text; };
    }

    protected override void OnPaintBackground(PaintEventArgs pevent) { }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        txtbox.Text = Text;
        Invalidate();
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        txtbox.BackColor = BackColor;
        Invalidate();
    }

    protected override void OnForeColorChanged(EventArgs e)
    {
        base.OnForeColorChanged(e);
        txtbox.ForeColor = ForeColor;
        Invalidate();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        txtbox.Font = Font;
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        txtbox.Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap B = new Bitmap(Width, Height);
        using Graphics G = Graphics.FromImage(B);
        G.SmoothingMode = SmoothingMode.HighQuality;
        Rectangle ClientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
        Height = txtbox.Height + 11;

        txtbox.Width = Width - 10;
        txtbox.TextAlign = TextAlignment;
        txtbox.UseSystemPasswordChar = UseSystemPasswordChar;

        G.Clear(BackColor);
        G.FillRectangle(new SolidBrush(Color.FromArgb(10, 10, 10)), ClientRectangle);
        G.DrawRectangle(new Pen(Color.FromArgb(53, 57, 60)), ClientRectangle);

        e.Graphics.DrawImage((Image)B.Clone(), 0, 0);
    }
}

public class VisceralGroupBox : ContainerControl
{
    private Size _imageSize;
    protected Size ImageSize => _imageSize;

    private Image _image;
    public Image Image
    {
        get => _image;
        set
        {
            if (value == null)
            {
                _imageSize = Size.Empty;
            }
            else
            {
                _imageSize = value.Size;
            }

            _image = value;
            Invalidate();
        }
    }

    public VisceralGroupBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap B = new Bitmap(Width, Height);
        using Graphics G = Graphics.FromImage(B);
        Rectangle TopBar = new Rectangle(10, 0, 130, 25);
        Rectangle box = new Rectangle(0, 0, Width - 1, Height - 10);

        G.Clear(Color.Transparent);
        G.SmoothingMode = SmoothingMode.HighQuality;

        using (LinearGradientBrush bodyGrade = new LinearGradientBrush(ClientRectangle, Color.FromArgb(15, 15, 15), Color.FromArgb(22, 22, 22), 120F))
        {
            G.FillPath(bodyGrade, Draw.RoundRect(new Rectangle(1, 12, Width - 3, box.Height - 1), 1));
        }

        using (LinearGradientBrush outerBorder = new LinearGradientBrush(ClientRectangle, Color.DimGray, Color.Gray, 90F))
        {
            G.DrawPath(new Pen(outerBorder), Draw.RoundRect(new Rectangle(1, 12, Width - 3, Height - 13), 1));
        }

        using (LinearGradientBrush outerBorder2 = new LinearGradientBrush(ClientRectangle, Color.FromArgb(0, 0, 0), Color.FromArgb(0, 0, 0), 90F))
        {
            G.DrawPath(new Pen(outerBorder2), Draw.RoundRect(new Rectangle(2, 13, Width - 5, Height - 15), 1));
        }

        using (LinearGradientBrush lbb = new LinearGradientBrush(TopBar, Color.FromArgb(30, 30, 32), Color.FromArgb(25, 25, 25), 90F))
        {
            G.FillPath(lbb, Draw.RoundRect(TopBar, 1));
        }

        G.DrawPath(Pens.DimGray, Draw.RoundRect(TopBar, 2));

        if (_image != null)
        {
            G.InterpolationMode = InterpolationMode.HighQualityBicubic;
            G.DrawImage(_image, new Rectangle(TopBar.Width - 115, 5, 16, 16));
            G.DrawString(Text, Font, Brushes.White, 35, 5);
        }
        else
        {
            G.DrawString(Text, Font, new SolidBrush(Color.White), TopBar, new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }

        e.Graphics.DrawImage((Image)B.Clone(), 0, 0);
    }
}

public class VisceralControlBox : Control
{
    #region MouseStates
    // Перечисление для представления состояния мыши
    private enum MouseState
    {
        None,
        Over,
        Down
    }

    private MouseState state = MouseState.None; // Текущее состояние мыши
    private int mouseX; // X позиция курсора мыши
    private readonly Rectangle minBtn = new Rectangle(0, 0, 35, 20); // Параметры кнопки минимизации
    private readonly Rectangle closeBtn = new Rectangle(35, 0, 35, 20); // Параметры кнопки закрытия

    protected override void OnMouseDown(MouseEventArgs e)
    {
        // Обработка события нажатия мыши
        base.OnMouseDown(e);
        if (IsWithinMinButton(e.Location))
        {
            FindForm().WindowState = FormWindowState.Minimized; // Минимизировать форму
        }
        else
        {
            FindForm().Close(); // Закрыть форму
        }
        state = MouseState.Down; // Обновить состояние мыши
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        // Обработка события отпускания мыши
        base.OnMouseUp(e);
        state = MouseState.Over; // Обновить состояние мыши
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        // Обработка события перемещения мыши внутрь элемента
        base.OnMouseEnter(e);
        state = MouseState.Over; // Обновить состояние мыши
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        // Обработка события перемещения мыши за пределы элемента
        base.OnMouseLeave(e);
        state = MouseState.None; // Обновить состояние мыши
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        // Обработка события движения мыши
        base.OnMouseMove(e);
        mouseX = e.Location.X; // Обновить X позицию курсора мыши
        Invalidate();
    }

    private bool IsWithinMinButton(Point location)
    {
        // Проверка, находится ли курсор в области кнопки минимизации
        return location.X > minBtn.X && location.X < minBtn.X + minBtn.Width;
    }
    #endregion

    public VisceralControlBox()
    {
        // Инициализация свойств элемента управления
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
        Anchor = AnchorStyles.Top | AnchorStyles.Right; // Установка якоря элемента управления
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // Обработка отрисовки элемента управления
        using (Bitmap bitmap = new Bitmap(Width, Height))
        using (Graphics graphics = Graphics.FromImage(bitmap))
        {
            base.OnPaint(e);
            graphics.Clear(BackColor); // Очистка фона
            Font drawFont = new Font("Merlett", 8, FontStyle.Bold); // Шрифт для кнопок

            // Рисование кнопок в зависимости от текущего состояния мыши
            switch (state)
            {
                case MouseState.None:
                    DrawButton(graphics, drawFont, Color.FromArgb(50, 50, 50), Color.FromArgb(45, 45, 45), minBtn, "_");
                    DrawButton(graphics, drawFont, Color.FromArgb(50, 50, 50), Color.FromArgb(45, 45, 45), closeBtn, "x");
                    break;

                case MouseState.Over:
                    if (IsWithinMinButton(new Point(mouseX, 0)))
                    {
                        DrawButton(graphics, drawFont, Color.FromArgb(50, 85, 255, 85), Color.FromArgb(45, 45, 45), minBtn, "_");
                    }
                    else
                    {
                        DrawButton(graphics, drawFont, Color.FromArgb(50, 30, 30), Color.FromArgb(45, 45, 45), closeBtn, "x");
                    }
                    DrawButton(graphics, drawFont, Color.FromArgb(50, 50, 50), Color.FromArgb(45, 45, 45), closeBtn, "x");
                    break;
            }

            e.Graphics.DrawImage(bitmap, 0, 0); // Отрисовка финального битмапа
        }
    }

    private void DrawButton(Graphics graphics, Font font, Color color1, Color color2, Rectangle rect, string text)
    {
        // Рисование кнопки с градиентным фоном и текстом
        using (LinearGradientBrush gradientBrush = new LinearGradientBrush(rect, color1, color2, 90F))
        {
            graphics.FillPath(gradientBrush, Draw.RoundRect(rect, 2)); // Заполнение градиентом
        }
        graphics.DrawPath(Pens.Black, Draw.RoundRect(rect, 2)); // Рисование обводки кнопки
        graphics.DrawString(text, font, Brushes.Silver, rect, new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        }); // Отрисовка текста кнопки
    }
}

[DefaultEvent("CheckedChanged")]
public class VisceralCheckBox : Control
{
    private MouseState state = MouseState.None;
    private bool _checked;

    public event EventHandler CheckedChanged;

    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            Invalidate();
        }
    }

    public VisceralCheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.White;
        Size = new Size(145, 16);
        DoubleBuffered = true;
    }

    protected override void OnMouseEnter(System.EventArgs e)
    {
        base.OnMouseEnter(e);
        state = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        state = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseLeave(System.EventArgs e)
    {
        base.OnMouseLeave(e);
        state = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        state = MouseState.Over;
        Invalidate();
    }

    protected override void OnTextChanged(System.EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnResize(System.EventArgs e)
    {
        base.OnResize(e);
        Height = 14;
    }

    protected override void OnClick(System.EventArgs e)
    {
        Checked = !Checked;
        CheckedChanged?.Invoke(this, EventArgs.Empty);
        base.OnClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var bitmap = new Bitmap(Width, Height))
        using (var g = Graphics.FromImage(bitmap))
        {
            var checkBoxRectangle = new Rectangle(0, 0, Height - 1, Height - 1);

            g.Clear(BackColor);

            using (var bodyGrad = new LinearGradientBrush(checkBoxRectangle, Color.FromArgb(25, 25, 25), Color.FromArgb(35, 35, 35), 120))
            {
                g.FillRectangle(bodyGrad, bodyGrad.Rectangle);
            }

            g.DrawRectangle(new Pen(Color.FromArgb(42, 47, 49)), new Rectangle(1, 1, Height - 3, Height - 3));
            g.DrawRectangle(new Pen(Color.FromArgb(87, 87, 89)), checkBoxRectangle);

            if (Checked)
            {
                var chkPoly = new Rectangle(
                    checkBoxRectangle.X + checkBoxRectangle.Width / 4,
                    checkBoxRectangle.Y + checkBoxRectangle.Height / 4,
                    checkBoxRectangle.Width / 2,
                    checkBoxRectangle.Height / 2);

                var poly = new Point[]
                {
                    new Point(chkPoly.X, chkPoly.Y + chkPoly.Height / 2),
                    new Point(chkPoly.X + chkPoly.Width / 2, chkPoly.Y + chkPoly.Height),
                    new Point(chkPoly.X + chkPoly.Width, chkPoly.Y)
                };

                g.SmoothingMode = SmoothingMode.HighQuality;
                using (var p1 = new Pen(Color.FromArgb(250, 255, 255, 255), 2))
                {
                    for (int i = 0; i < poly.Length - 1; i++)
                    {
                        g.DrawLine(p1, poly[i], poly[i + 1]);
                    }
                }
            }

            g.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(18, -1), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
    }
}
