using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

public class ValleyTheme : ContainerControl
{
    #region Variables
    private bool cap = false;
    private Point mousePoint = new Point(0, 0);
    private int moveHeight = 36;
    #endregion

    #region Properties
    public static Color TransparencyKey { private get; set; } = Color.Fuchsia;

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ParentForm.FormBorderStyle = FormBorderStyle.None;
        ParentForm.AllowTransparency = false;
        ParentForm.TransparencyKey = Color.Fuchsia;
        ParentForm.StartPosition = FormStartPosition.CenterScreen;
        Dock = DockStyle.Fill;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, moveHeight).Contains(e.Location))
        {
            cap = true;
            mousePoint = e.Location;
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
            Parent.Location = new Point(e.X - mousePoint.X, e.Y - mousePoint.Y);
        }
    }
    #endregion

    public ValleyTheme()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.OptimizedDoubleBuffer,
                 true);
        DoubleBuffered = true;
        BackColor = Color.FromArgb(242, 242, 242);
        Font = new Font("Segoe UI", 9);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        g.Clear(Color.FromArgb(22, 22, 22));
        g.DrawRectangle(new Pen(Color.FromArgb(38, 38, 38)), new Rectangle(0, 0, Width - 1, Height - 1));
        g.FillRectangle(new LinearGradientBrush(new Point(0, 0), new Point(0, 36),
            Color.FromArgb(50, 50, 50), Color.FromArgb(47, 47, 47)),
            new Rectangle(1, 1, Width - 2, 36));
        g.FillRectangle(new LinearGradientBrush(new Point(0, 0), new Point(0, Height),
            Color.FromArgb(45, 45, 45), Color.FromArgb(23, 23, 23)),
            new Rectangle(1, 36, Width - 2, Height - 46));

        g.DrawRectangle(new Pen(Color.FromArgb(38, 38, 38)), new Rectangle(9, 35, Width - 19, Height - 45));
        g.FillRectangle(new SolidBrush(BackColor), new Rectangle(10, 36, Width - 20, Height - 46));

        g.FillRectangle(new SolidBrush(Color.FromArgb(47, 47, 47)), new Rectangle(9, 35, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(47, 47, 47)), new Rectangle(Width - 10, 35, 1, 1));

        g.FillRectangle(new SolidBrush(Color.Fuchsia), new Rectangle(0, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.Fuchsia), new Rectangle(Width - 1, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.Fuchsia), new Rectangle(0, Height - 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.Fuchsia), new Rectangle(Width - 1, Height - 1, 1, 1));

        g.DrawString(FindForm().Text, new Font("Segoe UI", 10),
                     new SolidBrush(Color.FromArgb(242, 242, 242)), new Point(12, 6));
        base.OnPaint(e);
    }
}

public class ValleyButton : Control
{
    #region Variables
    private MouseState state = MouseState.None;
    #endregion

    #region Properties
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
    #endregion

    public ValleyButton()
    {
        Size = new Size(150, 50);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        g.Clear(Color.FromArgb(34, 122, 247));
        g.DrawRectangle(new Pen(Color.FromArgb(42, 59, 252)), new Rectangle(0, 0, Width - 1, Height - 1));

        switch (state)
        {
            case MouseState.Over:
                g.FillRectangle(new SolidBrush(Color.FromArgb(20, Color.White)), new Rectangle(1, 1, Width - 2, Height - 2));
                break;
            case MouseState.Down:
                g.FillRectangle(new SolidBrush(Color.FromArgb(20, Color.Black)), new Rectangle(1, 1, Width - 2, Height - 2));
                break;
        }

        g.FillRectangle(new SolidBrush(BackColor), new Rectangle(0, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(0, 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(1, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(0, 2, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(2, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(62, 107, 249)), new Rectangle(1, 1, 1, 1));
        g.FillRectangle(new SolidBrush(BackColor), new Rectangle(Width - 1, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(Width - 1, 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(Width - 2, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(Width - 1, 2, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(Width - 3, 0, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(62, 107, 249)), new Rectangle(Width - 2, 1, 1, 1));
        g.FillRectangle(new SolidBrush(BackColor), new Rectangle(0, Height - 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(0, Height - 2, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(1, Height - 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(0, Height - 3, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(2, Height - 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(62, 107, 249)), new Rectangle(1, Height - 2, 1, 1));
        g.FillRectangle(new SolidBrush(BackColor), new Rectangle(Width - 1, Height - 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(Width - 1, Height - 2, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(134, 144, 253)), new Rectangle(Width - 2, Height - 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(Width - 1, Height - 3, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(56, 72, 251)), new Rectangle(Width - 3, Height - 1, 1, 1));
        g.FillRectangle(new SolidBrush(Color.FromArgb(62, 107, 249)), new Rectangle(Width - 2, Height - 2, 1, 1));

        g.DrawString(Text, Font, Brushes.White,
                     new Point((Width / 2) - (TextRenderer.MeasureText(Text, Font).Width / 2),
                     (Height / 2) - (TextRenderer.MeasureText(Text, Font).Height / 2)));

        base.OnPaint(e);
    }
}

[DefaultEvent("CheckedChanged")]
public class ValleyRadioButton : Control
{
    #region Variables
    private MouseState state = MouseState.None;
    private bool checkedState;

    #endregion

    #region Properties
    public bool Checked
    {
        get => checkedState;
        set
        {
            checkedState = value;
            InvalidateControls();
            CheckedChanged?.DynamicInvoke(this);
            Invalidate();
        }
    }

    public event EventHandler CheckedChanged;

    protected override void OnClick(EventArgs e)
    {
        if (!checkedState) Checked = true;
        base.OnClick(e);
    }

    private void InvalidateControls()
    {
        if (!IsHandleCreated || !checkedState) return;
        foreach (Control c in Parent.Controls)
        {
            if (c != this && c is ValleyRadioButton radioButton)
            {
                radioButton.Checked = false;
                Invalidate();
            }
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        InvalidateControls();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 16;
    }

    #region Mouse States
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

    #endregion
    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.Clear(BackColor);

        g.DrawEllipse(new Pen(Color.FromArgb(191, 191, 191)), new Rectangle(0, 0, 15, 15));

        if (state == MouseState.Over)
            g.DrawEllipse(new Pen(Color.FromArgb(160, 160, 160)), new Rectangle(0, 0, 15, 15));

        if (Checked)
            g.FillEllipse(new SolidBrush(Color.FromArgb(56, 56, 56)), new Rectangle(4, 4, 7, 7));

        g.DrawString(Text, Font, new SolidBrush(Color.FromArgb(131, 131, 131)), new Point(18, -3));
        base.OnPaint(e);
    }
}

[DefaultEvent("CheckedChanged")]
public class ValleyCheckBox : Control
{
    #region Variables
    private MouseState state = MouseState.None;
    private bool checkedState;
    #endregion

    #region Properties
    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    public bool Checked
    {
        get => checkedState;
        set
        {
            checkedState = value;
            Invalidate();
        }
    }

    public event EventHandler CheckedChanged;

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        CheckedChanged?.DynamicInvoke(this);
        base.OnClick(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 16;
    }
    #endregion

    #region Mouse States
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
    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(BackColor);
        g.DrawRectangle(new Pen(Color.FromArgb(192, 192, 192)), new Rectangle(0, 0, 15, 15));

        if (state == MouseState.Over)
            g.DrawRectangle(new Pen(Color.FromArgb(160, 160, 160)), new Rectangle(0, 0, 15, 15));

        if (Checked)
            g.DrawString("ü", new Font("Wingdings", 9), new SolidBrush(Color.FromArgb(56, 56, 56)), new Point(0, 2));

        g.DrawString(Text, Font, new SolidBrush(Color.FromArgb(131, 131, 131)), new Point(18, -3));
        base.OnPaint(e);
    }
}

public class ValleyProgressBar : Control
{
    #region Variables
    private int value = 0;
    private int maximum = 100;
    #endregion

    #region Control
    [Category("Control")]
    public int Maximum
    {
        get => maximum;
        set
        {
            if (value < this.value) this.value = value;
            maximum = value;
            Invalidate();
        }
    }

    [Category("Control")]
    public int Value
    {
        get => value;
        set
        {
            if (value > Maximum) value = Maximum;
            this.value = value;
            Invalidate();
        }
    }
    #endregion

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 25;
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        Height = 25;
    }

    public void Increment(int amount)
    {
        Value += amount;
    }

    public ValleyProgressBar()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.OptimizedDoubleBuffer,
                 true);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(Color.FromArgb(223, 223, 223));
        int progVal = (int)(((float)value / maximum) * Width);
        g.FillRectangle(new SolidBrush(Color.FromArgb(50, 124, 244)), new Rectangle(0, 0, progVal - 1, Height));
        g.FillRectangle(new SolidBrush(Color.FromArgb(223, 223, 223)), new Rectangle(progVal, 0, Width - progVal, Height));
        base.OnPaint(e);
    }
}

//[DefaultEvent("TextChanged")]
//public class ValleyTextBox : Control
//{
//    private TextBox tb;

//    private HorizontalAlignment _textAlign = HorizontalAlignment.Left;
//    [Category("Options")]
//    public HorizontalAlignment TextAlign
//    {
//        get => _textAlign;
//        set
//        {
//            _textAlign = value;
//            tb.TextAlign = value;
//        }
//    }

//    private int _maxLength = 32767;
//    [Category("Options")]
//    public int MaxLength
//    {
//        get => _maxLength;
//        set
//        {
//            _maxLength = value;
//            tb.MaxLength = value;
//        }
//    }

//    private bool _readOnly;
//    [Category("Options")]
//    public bool ReadOnly
//    {
//        get => _readOnly;
//        set
//        {
//            _readOnly = value;
//            tb.ReadOnly = value;
//        }
//    }

//    private bool _multiLine;
//    [Category("Options")]
//    public bool Multiline
//    {
//        get => _multiLine;
//        set
//        {
//            _multiLine = value;
//            tb.Multiline = value;
//            tb.Height = value ? Height - 6 : tb.Height; // Изменить размер TextBox
//            Height = value ? Height : tb.Height + 6; // Изменить размер контрола
//        }
//    }

//    [Category("Options")]
//    public override string Text
//    {
//        get => tb.Text;
//        set => tb.Text = value;
//    }

//    public ValleyTextBox()
//    {
//        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
//                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
//        DoubleBuffered = true;

//        tb = new TextBox
//        {
//            Font = new Font("Microsoft Sans Serif", 10),
//            BackColor = Color.White, // Обычный цвет фона
//            ForeColor = Color.DimGray, // Цвет текста
//            BorderStyle = BorderStyle.None,
//            Location = new Point(5, 5),
//            Width = Width - 10,
//            Height = Height - 10,
//            Cursor = Cursors.IBeam
//        };

//        tb.TextChanged += (s, e) => Text = tb.Text;

//        Controls.Add(tb);
//    }

//    protected override void OnResize(EventArgs e)
//    {
//        base.OnResize(e);
//        tb.Width = Width - 10;
//        tb.Height = Height - 10;
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        base.OnPaint(e);
//        e.Graphics.Clear(Color.White);
//        e.Graphics.DrawRectangle(new Pen(Color.LightGray), 0, 0, Width - 1, Height - 1);
//    }
//}

public class ValleyClose : Control
{
    private MouseState state = MouseState.None;
    private int x;

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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        x = e.X;
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        Environment.Exit(0);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(16, 16);
    }

    public ValleyClose()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.Opaque, true);
        Anchor = AnchorStyles.Top | AnchorStyles.Right;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.FromArgb(49, 49, 49));
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.FillEllipse(new SolidBrush(Color.FromArgb(39, 39, 39)), new Rectangle(0, 0, 15, 15));
        g.FillEllipse(new SolidBrush(Color.FromArgb(254, 97, 82)), new Rectangle(2, 2, 11, 11));

        switch (state)
        {
            case MouseState.Over:
                g.FillEllipse(new SolidBrush(Color.FromArgb(40, Color.White)), new Rectangle(2, 2, 11, 11));
                break;
            case MouseState.Down:
                g.FillEllipse(new SolidBrush(Color.FromArgb(40, Color.Black)), new Rectangle(2, 2, 11, 11));
                break;
        }
    }
}

public class ValleyMax : Control
{
    private MouseState state = MouseState.None;
    private int x;

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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        x = e.X;
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        var form = FindForm();
        if (form != null)
        {
            form.WindowState = form.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(16, 16);
    }

    public ValleyMax()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.Opaque, true);
        Anchor = AnchorStyles.Top | AnchorStyles.Right;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.FromArgb(49, 49, 49));
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.FillEllipse(new SolidBrush(Color.FromArgb(39, 39, 39)), new Rectangle(0, 0, 15, 15));
        g.FillEllipse(new SolidBrush(Color.FromArgb(254, 190, 4)), new Rectangle(2, 2, 11, 11));

        switch (state)
        {
            case MouseState.Over:
                g.FillEllipse(new SolidBrush(Color.FromArgb(40, Color.White)), new Rectangle(2, 2, 11, 11));
                break;
            case MouseState.Down:
                g.FillEllipse(new SolidBrush(Color.FromArgb(40, Color.Black)), new Rectangle(2, 2, 11, 11));
                break;
        }
    }
}

public class ValleyMini : Control
{
    private MouseState state = MouseState.None;
    private int x;

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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        x = e.X;
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        var form = FindForm();
        if (form != null)
        {
            form.WindowState = form.WindowState == FormWindowState.Maximized || form.WindowState == FormWindowState.Normal
                ? FormWindowState.Minimized
                : form.WindowState;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Size = new Size(16, 16);
    }

    public ValleyMini()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        SetStyle(ControlStyles.Opaque, true);
        Anchor = AnchorStyles.Top | AnchorStyles.Right;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.FromArgb(49, 49, 49));
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.FillEllipse(new SolidBrush(Color.FromArgb(39, 39, 39)), new Rectangle(0, 0, 15, 15));
        g.FillEllipse(new SolidBrush(Color.FromArgb(23, 205, 58)), new Rectangle(2, 2, 11, 11));

        switch (state)
        {
            case MouseState.Over:
                g.FillEllipse(new SolidBrush(Color.FromArgb(40, Color.White)), new Rectangle(2, 2, 11, 11));
                break;
            case MouseState.Down:
                g.FillEllipse(new SolidBrush(Color.FromArgb(40, Color.Black)), new Rectangle(2, 2, 11, 11));
                break;
        }
    }
}

[DefaultEvent("TextChanged")]
public class ValleyTextBox : Control
{
    #region Variables
    private readonly TextBox tb;

    private Color textColor = Color.FromArgb(131, 131, 131);
    private readonly Color borderColor = Color.FromArgb(191, 191, 191);
    private readonly Color backgroundColor = Color.FromArgb(242, 242, 242); // Цвет фона
    #endregion

    #region Properties
    private HorizontalAlignment _TextAlign = HorizontalAlignment.Left;

    [Category("Options")]
    public HorizontalAlignment TextAlign
    {
        get => _TextAlign;
        set
        {
            _TextAlign = value;
            if (tb != null) tb.TextAlign = value;
        }
    }

    private int _MaxLength = 32767;

    [Category("Options")]
    public int MaxLength
    {
        get => _MaxLength;
        set
        {
            _MaxLength = value;
            if (tb != null) tb.MaxLength = value;
        }
    }

    private bool _ReadOnly;

    [Category("Options")]
    public bool ReadOnly
    {
        get => _ReadOnly;
        set
        {
            _ReadOnly = value;
            if (tb != null) tb.ReadOnly = value;
        }
    }

    private bool _UseSystemPasswordChar;

    [Category("Options")]
    public bool UseSystemPasswordChar
    {
        get => _UseSystemPasswordChar;
        set
        {
            _UseSystemPasswordChar = value;
            if (tb != null) tb.UseSystemPasswordChar = value;
        }
    }

    private bool _Multiline;

    [Category("Options")]
    public bool Multiline
    {
        get => _Multiline;
        set
        {
            _Multiline = value;
            if (tb != null)
            {
                tb.Multiline = value;
                tb.Size = new Size(Width - 10, value ? Height - 11 : tb.Height);
            }
        }
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            if (tb != null) tb.Text = value;
        }
    }

    public override Font Font
    {
        get => base.Font;
        set
        {
            base.Font = value;
            if (tb != null)
            {
                tb.Font = value;
                tb.Location = new Point(5, 5);
                tb.Width = Width - 10;
            }
        }
    }

    public override Color ForeColor
    {
        get => textColor;
        set => textColor = value;
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (!Controls.Contains(tb))
        {
            Controls.Add(tb);
        }
    }

    private void OnBaseTextChanged(object sender, EventArgs e)
    {
        Text = tb.Text;
    }

    private void OnBaseKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A)
        {
            tb.SelectAll();
            e.SuppressKeyPress = true;
        }
        if (e.Control && e.KeyCode == Keys.C)
        {
            tb.Copy();
            e.SuppressKeyPress = true;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        tb.Location = new Point(5, 5);
        tb.Size = new Size(Width - 10, Height - 11);

        if (!_Multiline)
            Height = tb.Height + 11;

        base.OnResize(e);
    }

    #region Constructor
    public ValleyTextBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;

        tb = new TextBox
        {
            Font = Font,
            Text = Text,
            BackColor = backgroundColor, // Установить цвет фона, аналогичный формам
            ForeColor = textColor,
            MaxLength = _MaxLength,
            Multiline = _Multiline,
            ReadOnly = _ReadOnly,
            UseSystemPasswordChar = _UseSystemPasswordChar,
            BorderStyle = BorderStyle.None,
            Location = new Point(5, 5),
            Size = new Size(Width - 10, Height - 11),
            Cursor = Cursors.IBeam
        };

        tb.TextChanged += OnBaseTextChanged;
        tb.KeyDown += OnBaseKeyDown;
    }
    #endregion

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        g.Clear(backgroundColor); // Цвет фона
        g.DrawRectangle(new Pen(borderColor), new Rectangle(0, 0, Width - 1, Height - 1));
        base.OnPaint(e);
    }
    #endregion
}