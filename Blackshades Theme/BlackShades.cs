using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// PLEASE LEAVE CREDITS IN SOURCE, DO NOT REDISTRIBUTE!
// --------------------- [ Theme ] --------------------
// Creator: Recuperare
// Contact: cschaefer2183 (Skype)
// Created: 09.22.2012
// Changed: 09.22.2012
// -------------------- [ /Theme ] ---------------------

public enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

#region GLOBAL FUNCTIONS

public class Draw
{
    public GraphicsPath RoundRect(Rectangle rectangle, int curve)
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

    public GraphicsPath RoundRect(int x, int y, int width, int height, int curve)
    {
        Rectangle rectangle = new Rectangle(x, y, width, height);
        return RoundRect(rectangle, curve);
    }
}

#endregion
public class BlackShadesNetForm : ContainerControl
{
    #region Control Help - Movement & Flicker Control
    private Point mouseP = new Point(0, 0);
    private bool cap = false;
    private int moveHeight;

    private void MinimBtnClick(object sender, EventArgs e)
    {
        ParentForm.WindowState = FormWindowState.Minimized;
    }

    private void CloseBtnClick(object sender, EventArgs e)
    {
        if (CloseButtonExitsApp)
        {
            Environment.Exit(0);
        }
        else
        {
            ParentForm.Close();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, moveHeight).Contains(e.Location))
        {
            cap = true;
            mouseP = e.Location;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        cap = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (cap)
        {
            var parentForm = this.FindForm();
            if (parentForm != null)
            {
                var newLocation = new Point(MousePosition.X - mouseP.X, MousePosition.Y - mouseP.Y);
                parentForm.Location = newLocation;
            }
        }
    }

    protected override void OnInvalidated(InvalidateEventArgs e)
    {
        base.OnInvalidated(e);
        ParentForm.Text = Text;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        e.Graphics.Clear(Color.FromArgb(42, 47, 49)); // Задайте желаемый цвет фона
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ParentForm.FormBorderStyle = FormBorderStyle.None;
    }

    private bool _closeButtonExitsApp = false;
    public bool CloseButtonExitsApp
    {
        get { return _closeButtonExitsApp; }
        set
        {
            _closeButtonExitsApp = value;
            Invalidate();
        }
    }

    private bool _minimizeButton = true;
    public bool MinimizeButton
    {
        get { return _minimizeButton; }
        set
        {
            _minimizeButton = value;
            Invalidate();
        }
    }

    #endregion

    private BlackShadesNetTopButton minimBtn;
    private BlackShadesNetTopButton closeBtn;

    public BlackShadesNetForm()
    {
        Dock = DockStyle.Fill;
        moveHeight = 25;
        Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
        ForeColor = Color.FromArgb(142, 152, 156);
        DoubleBuffered = true;

        // Инициализация кнопок
        minimBtn = new BlackShadesNetTopButton() { Location = new Point(Width - 44, 7) };
        closeBtn = new BlackShadesNetTopButton() { Location = new Point(Width - 27, 7) };

        closeBtn.Click += CloseBtnClick;
        minimBtn.Click += MinimBtnClick;

        Controls.Add(minimBtn);
        Controls.Add(closeBtn); // Убедитесь, что обе кнопки добавлены в контролы
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        const int curve = 7;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        Rectangle clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
        Draw draw = new Draw();

        g.Clear(Color.FromArgb(42, 47, 49));
        g.FillPath(new SolidBrush(Color.FromArgb(42, 47, 49)), draw.RoundRect(clientRectangle, curve));

        // DRAW GRADIENTED BORDER
        using (LinearGradientBrush innerGradLeft = new LinearGradientBrush(new Rectangle(1, 1, Width / 2 - 1, Height - 3),
                Color.FromArgb(102, 108, 112), Color.FromArgb(204, 216, 224), 0F))
        {
            using (LinearGradientBrush innerGradRight = new LinearGradientBrush(new Rectangle(1, 1, Width / 2 - 1, Height - 3),
                    Color.FromArgb(204, 216, 224), Color.FromArgb(102, 108, 112), 0F))
            {
                g.DrawPath(new Pen(innerGradLeft), draw.RoundRect(new Rectangle(1, 1, Width / 2 + 3, Height - 3), curve));
                g.DrawPath(new Pen(innerGradRight), draw.RoundRect(new Rectangle(Width / 2 - 5, 1, Width / 2 + 3, Height - 3), curve));
            }
        }

        g.FillPath(new SolidBrush(Color.FromArgb(42, 47, 49)), draw.RoundRect(new Rectangle(2, 2, Width - 5, Height - 5), curve));
        g.DrawString(Text, Font, Brushes.White, new Rectangle(curve, curve - 2, Width - 1, 22),
            new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });

        // Обновить размещение кнопок
        minimBtn.Location = new Point(Width - 44, 7);
        closeBtn.Location = new Point(Width - 27, 7);
    }
}
public class BlackShadesNetTopButton : Control
{
    #region Control Help - MouseState & Flicker Control

    private MouseState state = MouseState.None;

    protected override void OnMouseEnter(EventArgs e)
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

    protected override void OnMouseLeave(EventArgs e)
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

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    #endregion

    public BlackShadesNetTopButton()
    {
        BackColor = Color.FromArgb(38, 38, 38);
        Font = new Font("Verdana", 8.25F);
        Size = new Size(15, 11);
        DoubleBuffered = true;
        Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap b = new Bitmap(Width, Height))
        using (Graphics g = Graphics.FromImage(b))
        {
            var d = new Draw();

            var clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            g.Clear(Color.FromArgb(49, 53, 55));

            switch (state)
            {
                case MouseState.None:
                    DrawDefaultState(g, d, clientRectangle);
                    break;
                case MouseState.Over:
                    DrawHoverState(g, d, clientRectangle);
                    break;
                case MouseState.Down:
                    DrawDownState(g, d, clientRectangle);
                    break;
            }

            e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
        }
    }

    private void DrawDefaultState(Graphics g, Draw d, Rectangle clientRectangle)
    {
        var border = new LinearGradientBrush(clientRectangle, Color.FromArgb(200, 44, 47, 51), Color.FromArgb(80, 64, 69, 71), 90F);
        g.FillPath(border, d.RoundRect(clientRectangle, 1));
        var bodyGrad = new LinearGradientBrush(new Rectangle(2, 2, Width - 6, Height - 6), Color.FromArgb(90, 97, 101), Color.FromArgb(63, 69, 73), 90F);
        g.FillPath(bodyGrad, d.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 1));
        g.DrawPath(new Pen(Color.FromArgb(30, 32, 35)), d.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 1));
    }

    private void DrawHoverState(Graphics g, Draw d, Rectangle clientRectangle)
    {
        var border = new LinearGradientBrush(clientRectangle, Color.FromArgb(200, 44, 47, 51), Color.FromArgb(80, 64, 69, 71), 90F);
        g.FillPath(border, d.RoundRect(clientRectangle, 1));
        var bodyGrad = new LinearGradientBrush(new Rectangle(2, 2, Width - 6, Height - 6), Color.FromArgb(90, 97, 101), Color.FromArgb(63, 69, 73), 90F);
        g.FillPath(bodyGrad, d.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 1));
        g.DrawPath(new Pen(Color.FromArgb(30, 32, 35)), d.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 1));
        g.DrawPath(new Pen(Color.FromArgb(200, 0, 186, 255)), d.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 1));
    }

    private void DrawDownState(Graphics g, Draw d, Rectangle clientRectangle)
    {
        var border = new LinearGradientBrush(clientRectangle, Color.FromArgb(200, 44, 47, 51), Color.FromArgb(80, 64, 69, 71), 90F);
        g.FillPath(border, d.RoundRect(clientRectangle, 1));
        var bodyGrad = new LinearGradientBrush(new Rectangle(2, 2, Width - 6, Height - 6), Color.FromArgb(90, 97, 101), Color.FromArgb(63, 69, 73), 135F);
        g.FillPath(bodyGrad, d.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 1));
        g.DrawPath(new Pen(Color.FromArgb(30, 32, 35)), d.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 1));
    }
}
public class BlackShadesNetMultiLineTextBox : Control
{
    private System.Windows.Forms.TextBox txtbox = new System.Windows.Forms.TextBox();

    #region Control Help - Properties & Flicker Control 
    private int _maxChars = 32767;
    public int MaxCharacters
    {
        get { return _maxChars; }
        set
        {
            _maxChars = value;
            txtbox.MaxLength = _maxChars; // Устанавливаем максимальное количество символов для TextBox
            Invalidate();
        }
    }

    private HorizontalAlignment _align = HorizontalAlignment.Left; // Инициализация по умолчанию
    public HorizontalAlignment TextAlignment
    {
        get { return _align; }
        set
        {
            _align = value;
            txtbox.TextAlign = _align; // Устанавливаем выравнивание для TextBox
            Invalidate();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Не вызываем базовую реализацию для предотвращения мерцания
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        txtbox.Text = Text; // Обновляем текст в TextBox
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

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        txtbox.Size = new Size(Width - 10, Height - 10);
        txtbox.Location = new Point(5, 5); // Установите расстояние от границ
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

    private void Txtbox_TextChanged(object sender, EventArgs e)
    {
        Text = txtbox.Text; // Обновляем текст в контроле
    }

    private void InitializeTextBox()
    {
        txtbox.Multiline = true;
        txtbox.MaxLength = _maxChars; // Установите максимальное количество символов
        txtbox.BackColor = BackColor;
        txtbox.ForeColor = ForeColor;
        txtbox.Text = Text; // Инициализируем текст
        txtbox.BorderStyle = BorderStyle.None;
        txtbox.Location = new Point(5, 5); // Расположение TextBox
        txtbox.Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
        txtbox.Size = new Size(Width - 10, Height - 10);

        txtbox.TextChanged += Txtbox_TextChanged;
        Controls.Add(txtbox); // Перенести сюда
    }
    #endregion

    public BlackShadesNetMultiLineTextBox()
    {
        InitializeTextBox();

        Text = string.Empty;
        BackColor = Color.FromArgb(36, 40, 42);
        ForeColor = Color.FromArgb(142, 152, 156);
        Size = new Size(135, 35);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap b = new Bitmap(Width, Height))
        using (Graphics g = Graphics.FromImage(b))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            Rectangle clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);

            g.Clear(Color.FromArgb(36, 40, 42));
            g.FillRectangle(new SolidBrush(Color.FromArgb(36, 40, 42)), clientRectangle);
            g.DrawRectangle(new Pen(Color.FromArgb(53, 57, 60)), clientRectangle);

            e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
        }
    }
}
public class BlackShadesNetTextBox : Control
{
    private System.Windows.Forms.TextBox txtbox = new System.Windows.Forms.TextBox();

    #region " Control Help - Properties & Flicker Control "
    private bool _passmask = false;
    public bool UseSystemPasswordChar
    {
        get { return _passmask; }
        set
        {
            txtbox.UseSystemPasswordChar = value;
            _passmask = value;
            Invalidate();
        }
    }

    private int _maxchars = 32767;
    public int MaxLength
    {
        get { return _maxchars; }
        set
        {
            _maxchars = value;
            txtbox.MaxLength = value;
            Invalidate();
        }
    }

    private HorizontalAlignment _align;
    public HorizontalAlignment TextAlignment
    {
        get { return _align; }
        set
        {
            _align = value;
            Invalidate();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Do nothing
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
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

    private void TextChngTxtBox(object sender, EventArgs e)
    {
        Text = txtbox.Text;
    }

    private void TextChng(object sender, EventArgs e)
    {
        txtbox.Text = Text;
    }

    private void NewTextBox()
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

        txtbox.TextChanged += new EventHandler(TextChngTxtBox);
        this.TextChanged += new EventHandler(TextChng);
    }
    #endregion 
    public BlackShadesNetTextBox()
    {
        NewTextBox();
    Controls.Add(txtbox);

        Text = "";
        BackColor = Color.FromArgb(36, 40, 42);
        ForeColor = Color.FromArgb(142, 152, 156);
        Size = new Size(135, 35);
    DoubleBuffered = true;
    }

protected override void OnPaint(PaintEventArgs e)
{
    Bitmap B = new Bitmap(Width, Height);
    Graphics G = Graphics.FromImage(B);
    G.SmoothingMode = SmoothingMode.HighQuality;
    Rectangle ClientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);

    Height = txtbox.Height + 11;
    txtbox.Width = Width - 10;
    txtbox.TextAlign = TextAlignment;
    txtbox.UseSystemPasswordChar = UseSystemPasswordChar;

    G.Clear(Color.FromArgb(36, 40, 42));

    G.FillRectangle(new SolidBrush(Color.FromArgb(36, 40, 42)), ClientRectangle);
    G.DrawRectangle(new Pen(Color.FromArgb(53, 57, 60)), ClientRectangle);

    e.Graphics.DrawImage((Image)B.Clone(), 0, 0);
    G.Dispose();
    B.Dispose();
}
}

public class BlackShadesNetRichTextBox : Control
{
    private RichTextBox txtbox = new RichTextBox();

    #region " Control Help - Properties & Flicker Control "

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // Do nothing to prevent flicker
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
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

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        txtbox.Size = new Size(Width - 10, Height - 11);
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

    private void TextChngTxtBox(object sender, EventArgs e)
    {
        Text = txtbox.Text;
    }

    private void TextChng(object sender, EventArgs e)
    {
        txtbox.Text = Text;
    }

    private void NewTextBox()
    {
        txtbox.Multiline = true;
        txtbox.BackColor = BackColor;
        txtbox.ForeColor = ForeColor;
        txtbox.Text = string.Empty;
        txtbox.BorderStyle = BorderStyle.None;
        txtbox.Location = new Point(3, 4);
        txtbox.Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
        txtbox.Size = new Size(Width - 10, Height - 10);

        txtbox.TextChanged += new EventHandler(TextChngTxtBox);
        this.TextChanged += new EventHandler(TextChng);
    }
    #endregion
    public BlackShadesNetRichTextBox()
    {
        NewTextBox();
    Controls.Add(txtbox);

        Text = "";
        BackColor = Color.FromArgb(36, 40, 42);
        ForeColor = Color.FromArgb(142, 152, 156);
        Size = new Size(135, 35);
    DoubleBuffered = true;
    }

protected override void OnPaint(PaintEventArgs e)
{
    Bitmap B = new Bitmap(Width, Height);
    Graphics G = Graphics.FromImage(B);
    G.SmoothingMode = SmoothingMode.HighQuality;
    Rectangle ClientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);

    G.Clear(Color.FromArgb(36, 40, 42));

    G.DrawRectangle(new Pen(Color.FromArgb(30, 33, 35), 2), ClientRectangle);
    G.DrawLine(new Pen(Color.FromArgb(83, 90, 94)), Width - 1, 0, Width - 1, Height);
    G.DrawLine(new Pen(Color.FromArgb(83, 90, 94)), 0, Height - 1, Width - 1, Height - 1);

    e.Graphics.DrawImage(B.Clone() as Image, 0, 0);
    G.Dispose();
    B.Dispose();
}
}
[DefaultEvent("CheckedChanged")]
public class BlackShadesNetCheckBox : Control
{
    #region Control Help - MouseState & Flicker Control
    private MouseState state = MouseState.None;
    private bool _checked;

    protected override void OnMouseEnter(EventArgs e)
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

    protected override void OnMouseLeave(EventArgs e)
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

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // No background painting
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    public bool Checked
    {
        get { return _checked; }
        set
        {
            _checked = value;
            Invalidate();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 14;
    }

    protected override void OnClick(EventArgs e)
    {
        _checked = !_checked;
        CheckedChanged?.Invoke(this, EventArgs.Empty);
        base.OnClick(e);
    }

    public event EventHandler CheckedChanged;
    #endregion

    public BlackShadesNetCheckBox()
    {
        BackColor = Color.FromArgb(20, 20, 20);
        ForeColor = Color.White;
        Size = new Size(145, 16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap B = new Bitmap(Width, Height))
        using (Graphics G = Graphics.FromImage(B))
        {
            Rectangle checkBoxRectangle = new Rectangle(0, 0, Height - 1, Height - 1);
            G.Clear(Color.FromArgb(42, 47, 49));

            using (LinearGradientBrush bodyGrad = new LinearGradientBrush(checkBoxRectangle, Color.FromArgb(36, 40, 42), Color.FromArgb(64, 71, 74), 90F))
            {
                G.FillRectangle(bodyGrad, bodyGrad.Rectangle);
            }

            G.DrawRectangle(new Pen(Color.FromArgb(42, 47, 49)), new Rectangle(1, 1, Height - 3, Height - 3));
            G.DrawRectangle(new Pen(Color.FromArgb(102, 108, 112)), checkBoxRectangle);

            if (Checked)
            {
                Rectangle chkPoly = new Rectangle(checkBoxRectangle.X + checkBoxRectangle.Width / 4, checkBoxRectangle.Y + checkBoxRectangle.Height / 4,
                    checkBoxRectangle.Width / 2, checkBoxRectangle.Height / 2);
                Point[] poly = {
                    new Point(chkPoly.X, chkPoly.Y + chkPoly.Height / 2),
                    new Point(chkPoly.X + chkPoly.Width / 2, chkPoly.Y + chkPoly.Height),
                    new Point(chkPoly.X + chkPoly.Width, chkPoly.Y)
                };

                G.SmoothingMode = SmoothingMode.HighQuality;
                using (Pen P1 = new Pen(Color.FromArgb(250, 255, 255, 255), 2))
                using (LinearGradientBrush chkGrad = new LinearGradientBrush(chkPoly, Color.FromArgb(200, 200, 200), Color.FromArgb(255, 255, 255), 0F))
                {
                    for (int i = 0; i < poly.Length - 1; i++)
                    {
                        G.DrawLine(P1, poly[i], poly[i + 1]);
                    }
                }
            }

            G.DrawString(Text, Font, new SolidBrush(ForeColor), new Point(18, -1), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });

            e.Graphics.DrawImage(B, 0, 0);
        }
    }

    private enum MouseState
    {
        None,
        Over,
        Down
    }
}
[DefaultEvent("CheckedChanged")]
public class BlackShadesNetRadioButton : Control
{

    #region Control Help - MouseState & Flicker Control

    private MouseState state = MouseState.None;
    private bool _checked;

    protected override void OnMouseEnter(EventArgs e)
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

    protected override void OnMouseLeave(EventArgs e)
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

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 16;
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // No background painting
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    public bool Checked
    {
        get { return _checked; }
        set
        {
            _checked = value;
            InvalidateControls();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    protected override void OnClick(EventArgs e)
    {
        if (!_checked) Checked = true;
        base.OnClick(e);
    }

    public event EventHandler CheckedChanged;

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        InvalidateControls();
    }

    private void InvalidateControls()
    {
        if (!IsHandleCreated || !_checked) return;

        foreach (Control c in Parent.Controls)
        {
            if (c != this && c is BlackShadesNetRadioButton)
            {
                ((BlackShadesNetRadioButton)c).Checked = false;
            }
        }
    }
    #endregion

    public BlackShadesNetRadioButton()
    {
        BackColor = Color.FromArgb(42, 47, 49);
        ForeColor = Color.White;
        Size = new Size(150, 16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap B = new Bitmap(Width, Height))
        using (Graphics G = Graphics.FromImage(B))
        {
            Rectangle radioBtnRectangle = new Rectangle(0, 0, Height - 1, Height - 1);

            G.SmoothingMode = SmoothingMode.HighQuality;
            G.Clear(BackColor);

            using (LinearGradientBrush bgGrad = new LinearGradientBrush(radioBtnRectangle, Color.FromArgb(36, 40, 42), Color.FromArgb(66, 70, 72), 90F))
            {
                G.FillEllipse(bgGrad, radioBtnRectangle);
            }

            G.DrawEllipse(new Pen(Color.FromArgb(44, 48, 50)), new Rectangle(1, 1, Height - 3, Height - 3));
            G.DrawEllipse(new Pen(Color.FromArgb(102, 108, 112)), radioBtnRectangle);

            if (Checked)
            {
                using (LinearGradientBrush chkGrad = new LinearGradientBrush(new Rectangle(4, 4, Height - 9, Height - 8), Color.White, Color.Black, 90F))
                {
                    G.FillEllipse(chkGrad, new Rectangle(4, 4, Height - 9, Height - 9));
                }
            }

            G.DrawString(Text, Font, Brushes.White, new Point(18, 0), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });

            e.Graphics.DrawImage(B, 0, 0);
        }
    }

    private enum MouseState
    {
        None,
        Over,
        Down
    }
}
public class BlackShadesNetGroupBox : ContainerControl
{
    #region Control Help - Properties & Flicker Control
    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        // No background painting
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
    #endregion

    public BlackShadesNetGroupBox()
    {
        BackColor = Color.FromArgb(33, 33, 33);
        Size = new Size(200, 100);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap B = new Bitmap(Width, Height))
        using (Graphics G = Graphics.FromImage(B))
        {
            G.SmoothingMode = SmoothingMode.HighSpeed;
            const int curve = 3;
            Rectangle ClientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            Color TransparencyKey = this.ParentForm.TransparencyKey;
            Draw d = new Draw();
            base.OnPaint(e);

            G.Clear(Color.FromArgb(42, 47, 49));

            G.DrawPath(new Pen(Color.FromArgb(67, 75, 78)), d.RoundRect(new Rectangle(2, 7, Width - 5, Height - 9), curve));
            using (LinearGradientBrush outerBorder = new LinearGradientBrush(ClientRectangle, Color.FromArgb(30, 32, 32), Color.Transparent, 90F))
            {
                G.DrawPath(new Pen(outerBorder), d.RoundRect(new Rectangle(1, 6, Width - 3, Height - 9), curve));
            }
            using (LinearGradientBrush innerBorder = new LinearGradientBrush(new Rectangle(3, 7, Width - 7, Height - 10), Color.Transparent, Color.FromArgb(30, 32, 32), 90F))
            {
                G.DrawPath(new Pen(innerBorder), d.RoundRect(new Rectangle(3, 7, Width - 7, Height - 10), curve));
            }

            G.FillRectangle(new SolidBrush(Color.FromArgb(42, 47, 49)), new Rectangle(8, 0, Text.Length * 6, 11));
            G.DrawString(Text, Font, new SolidBrush(ForeColor), new Rectangle(8, 0, Width - 1, 11), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });

            e.Graphics.DrawImage(B, 0, 0);
        }
    }
}

