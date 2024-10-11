using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

// .::Tweety Theme::.
// Author:   UnReLaTeDScript
// Converted to C# by: Delirium™ @ HackForums.Net
// Credits:  Aeonhack [Themebase]
// Version:  1.0
public abstract class Theme : ContainerControl
{
    // Initialization
    protected Graphics G;

    public Theme()
    {
        // Set control styles for optimization
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
    }

    private bool ParentIsForm;
    protected override void OnHandleCreated(EventArgs e)
    {
        // Fill parent container
        Dock = DockStyle.Fill;

        // Check if parent is a form
        ParentIsForm = Parent is Form;

        // Set transparency key if defined
        if (ParentIsForm && !_TransparencyKey.IsEmpty)
        {
            ((Form)Parent).TransparencyKey = _TransparencyKey;
        }

        // Remove default form border
        if (ParentIsForm)
        {
            ((Form)Parent).FormBorderStyle = FormBorderStyle.None;
        }

        base.OnHandleCreated(e);
    }

    // Override the Text property to invalidate on change
    public override string Text
    {
        get { return base.Text; }
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    // Sizing and Movement
    private bool _Resizable = true;

    public bool Resizable
    {
        get { return _Resizable; }
        set { _Resizable = value; }
    }

    private int _MoveHeight = 24;

    public int MoveHeight
    {
        get { return _MoveHeight; }
        set
        {
            _MoveHeight = value;
            Header = new Rectangle(7, 7, Width - 14, _MoveHeight - 7);
        }
    }

    private IntPtr Flag;

    protected override void OnMouseDown(MouseEventArgs e)
    {
        // Check if left mouse button is pressed
        if (e.Button != MouseButtons.Left)
            return;

        // Check if parent is a form and if it is maximized
        if (ParentIsForm && ((Form)Parent).WindowState == FormWindowState.Maximized)
            return;

        // Determine if click is within the header area
        if (Header.Contains(e.Location))
        {
            // Set flag for moving
            Flag = new IntPtr(2);
        }
        else if (Current.Position != 0 && _Resizable)
        {
            // Set flag for resizing
            Flag = new IntPtr(Current.Position);
        }
        else
        {
            // Ignore if no resizing or moving is allowed
            return;
        }

        // Capture mouse events
        Capture = true;

        // Send message to parent window
        var m = Message.Create(Parent.Handle, 0xA1, Flag, IntPtr.Zero); // 0xA1 - WM_NCLBUTTONDOWN
        DefWndProc(ref m); // Передаем сообщение по ссылке

        // Call base method to ensure proper event handling
        base.OnMouseDown(e);
    }


    // Structure for pointer data (cursor and position)
    private struct Pointer
    {
        public readonly Cursor Cursor;
        public readonly byte Position;

        public Pointer(Cursor c, byte p)
        {
            Cursor = c;
            Position = p;
        }
    }

    // Flags for pointer position calculation
    private bool F1;
    private bool F2;
    private bool F3;
    private bool F4;
    private Point PTC;

    // Get pointer based on mouse position
    private Pointer GetPointer()
    {
        // Get mouse position relative to control
        PTC = PointToClient(MousePosition);

        // Check flags for pointer position
        F1 = PTC.X < 7;
        F2 = PTC.X > Width - 7;
        F3 = PTC.Y < 7;
        F4 = PTC.Y > Height - 7;

        // Return pointer based on position
        if (F1 && F3)
        {
            return new Pointer(Cursors.SizeNWSE, 13);
        }
        if (F1 && F4)
        {
            return new Pointer(Cursors.SizeNESW, 16);
        }
        if (F2 && F3)
        {
            return new Pointer(Cursors.SizeNESW, 14);
        }
        if (F2 && F4)
        {
            return new Pointer(Cursors.SizeNWSE, 17);
        }
        if (F1)
        {
            return new Pointer(Cursors.SizeWE, 10);
        }
        if (F2)
        {
            return new Pointer(Cursors.SizeWE, 11);
        }
        if (F3)
        {
            return new Pointer(Cursors.SizeNS, 12);
        }
        if (F4)
        {
            return new Pointer(Cursors.SizeNS, 15);
        }
        return new Pointer(Cursors.Default, 0);
    }

    // Current and pending pointer data
    private Pointer Current;
    private Pointer Pending;

    // Update current pointer based on pending data
    private void SetCurrent()
    {
        Pending = GetPointer();

        // Check if pending position is different from current
        if (Current.Position == Pending.Position)
        {
            return;
        }

        // Update current pointer data
        Current = GetPointer();
        Cursor = Current.Cursor;
    }

    // Handle mouse move event to update pointer
    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_Resizable)
        {
            SetCurrent();
        }
        base.OnMouseMove(e);
    }

    // Header area for movement and resizing
    protected Rectangle Header;

    // Handle control size changed event to update header
    protected override void OnSizeChanged(EventArgs e)
    {
        // Ignore if width or height is 0
        if (Width == 0 || Height == 0)
        {
            return;
        }

        // Update header rectangle
        Header = new Rectangle(7, 7, Width - 14, _MoveHeight - 7);

        // Invalidate control for redraw
        Invalidate();

        base.OnSizeChanged(e);
    }

    // Convienence
    public abstract void PaintHook();

    // Override OnPaint to call PaintHook
    protected override sealed void OnPaint(PaintEventArgs e)
    {
        // Ignore if width or height is 0
        if (Width == 0 || Height == 0)
        {
            return;
        }

        // Set graphics object
        G = e.Graphics;

        // Call abstract PaintHook method
        PaintHook();
    }

    private Color _TransparencyKey;

    public Color TransparencyKey
    {
        get { return _TransparencyKey; }
        set
        {
            _TransparencyKey = value;
            Invalidate();
        }
    }

    private Image _Image;

    public Image Image
    {
        get { return _Image; }
        set
        {
            _Image = value;
            Invalidate();
        }
    }

    public int ImageWidth
    {
        get
        {
            if (_Image == null)
            {
                return 0;
            }
            return _Image.Width;
        }
    }

    private Size _Size;
    private Rectangle _Rectangle;
    private LinearGradientBrush _Gradient;
    private SolidBrush _Brush;

    // Draw corners of a rectangle
    protected void DrawCorners(Color c, Rectangle rect)
    {
        _Brush = new SolidBrush(c);

        // Fill corner pixels
        G.FillRectangle(_Brush, rect.X, rect.Y, 1, 1);
        G.FillRectangle(_Brush, rect.X + (rect.Width - 1), rect.Y, 1, 1);
        G.FillRectangle(_Brush, rect.X, rect.Y + (rect.Height - 1), 1, 1);
        G.FillRectangle(_Brush, rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), 1, 1);
    }

    // Draw borders of a rectangle
    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        // Draw outer border
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

        // Draw inner border
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    // Draw text with specified alignment and color
    protected void DrawText(HorizontalAlignment a, Color c, int x)
    {
        DrawText(a, c, x, 0);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y)
    {
        // Ignore if text is empty
        if (string.IsNullOrEmpty(Text))
        {
            return;
        }

        // Measure text size
        _Size = G.MeasureString(Text, Font).ToSize();
        _Brush = new SolidBrush(c);

        // Draw text based on alignment
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

    // Draw icon with specified alignment and offset
    protected void DrawIcon(HorizontalAlignment a, int x)
    {
        DrawIcon(a, x, 0);
    }

    protected void DrawIcon(HorizontalAlignment a, int x, int y)
    {
        // Ignore if image is null
        if (_Image == null)
        {
            return;
        }

        // Draw image based on alignment
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

    // Draw linear gradient with specified colors, position, and angle
    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        _Rectangle = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(_Rectangle, c1, c2, angle);
        G.FillRectangle(_Gradient, _Rectangle);
    }
}

// Class for drawing rounded rectangles
public static class Draw
{
    // Create a rounded rectangle path
    public static GraphicsPath RoundRect(Rectangle rectangle, int curve)
    {
        // Create a new graphics path
        var p = new GraphicsPath();

        // Calculate arc rectangle width
        int arcRectangleWidth = curve * 2;

        // Add arcs for rounded corners
        p.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        p.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        p.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 0, 90);
        p.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 90, 90);

        // Add a line to connect the last arc to the first
        p.AddLine(new Point(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y), new Point(rectangle.X, curve + rectangle.Y));

        // Return the graphics path
        return p;
    }
}

// Abstract class for theme control
public abstract class ThemeControl : Control
{
    // Initialization
    protected Graphics G;
    protected Bitmap B;

    public ThemeControl()
    {
        // Set control styles for optimization
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
        // Create a new 1x1 bitmap for drawing
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    // Allow transparency for control
    public void AllowTransparent()
    {
        SetStyle(ControlStyles.Opaque, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    // Override the Text property to invalidate on change
    public override string Text
    {
        get { return base.Text; }
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    // Mouse Handling
    protected enum State : byte
    {
        MouseNone = 0,
        MouseOver = 1,
        MouseDown = 2
    }

    // Mouse state for visual updates
    protected State MouseState;

    // Handle mouse leave event
    protected override void OnMouseLeave(EventArgs e)
    {
        ChangeMouseState(State.MouseNone);
        base.OnMouseLeave(e);
    }

    // Handle mouse enter event
    protected override void OnMouseEnter(EventArgs e)
    {
        ChangeMouseState(State.MouseOver);
        base.OnMouseEnter(e);
    }

    // Handle mouse up event
    protected override void OnMouseUp(MouseEventArgs e)
    {
        ChangeMouseState(State.MouseOver);
        base.OnMouseUp(e);
    }

    // Handle mouse down event
    protected override void OnMouseDown(MouseEventArgs e)
    {
        // Check if left mouse button is pressed
        if (e.Button == MouseButtons.Left)
        {
            ChangeMouseState(State.MouseDown);
        }
        base.OnMouseDown(e);
    }

    // Change mouse state and invalidate control
    private void ChangeMouseState(State e)
    {
        MouseState = e;
        Invalidate();
    }

    // Convienence
    public abstract void PaintHook();

    // Override OnPaint to call PaintHook and draw the bitmap
    protected override sealed void OnPaint(PaintEventArgs e)
    {
        // Ignore if width or height is 0
        if (Width == 0 || Height == 0)
        {
            return;
        }

        // Call abstract PaintHook method
        PaintHook();

        // Draw the bitmap to the control
        e.Graphics.DrawImage(B, 0, 0);
    }

    // Handle control size changed event to resize the bitmap
    protected override void OnSizeChanged(EventArgs e)
    {
        // Ignore if width or height is 0
        if (Width == 0 || Height == 0)
        {
            return;
        }

        // Create a new bitmap with updated dimensions
        B = new Bitmap(Width, Height);
        G = Graphics.FromImage(B);

        // Invalidate control for redraw
        Invalidate();

        base.OnSizeChanged(e);
    }

    // Flag for disabling rounded corners
    private bool _NoRounding;

    public bool NoRounding
    {
        get { return _NoRounding; }
        set
        {
            _NoRounding = value;
            Invalidate();
        }
    }

    private Image _Image;

    public Image Image
    {
        get { return _Image; }
        set
        {
            _Image = value;
            Invalidate();
        }
    }

    public int ImageWidth
    {
        get
        {
            if (_Image == null)
            {
                return 0;
            }
            return _Image.Width;
        }
    }

    public int ImageTop
    {
        get
        {
            if (_Image == null)
            {
                return 0;
            }
            return Height / 2 - _Image.Height / 2;
        }
    }

    private Size _Size;
    private Rectangle _Rectangle;
    private LinearGradientBrush _Gradient;
    private SolidBrush _Brush;

    // Draw corners of a rectangle
    protected void DrawCorners(Color c, Rectangle rect)
    {
        // Ignore if no rounding is enabled
        if (_NoRounding)
        {
            return;
        }

        // Set corner pixels
        B.SetPixel(rect.X, rect.Y, c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y, c);
        B.SetPixel(rect.X, rect.Y + (rect.Height - 1), c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), c);
    }

    // Draw borders of a rectangle
    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        // Draw outer border
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

        // Draw inner border
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    // Draw text with specified alignment and color
    protected void DrawText(HorizontalAlignment a, Color c, int x)
    {
        DrawText(a, c, x, 0);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y)
    {
        // Ignore if text is empty
        if (string.IsNullOrEmpty(Text))
        {
            return;
        }

        // Measure text size
        _Size = G.MeasureString(Text, Font).ToSize();
        _Brush = new SolidBrush(c);

        // Draw text based on alignment
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

    // Draw icon with specified alignment and offset
    protected void DrawIcon(HorizontalAlignment a, int x)
    {
        DrawIcon(a, x, 0);
    }

    protected void DrawIcon(HorizontalAlignment a, int x, int y)
    {
        // Ignore if image is null
        if (_Image == null)
        {
            return;
        }

        // Draw image based on alignment
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

    // Draw linear gradient with specified colors, position, and angle
    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        _Rectangle = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(_Rectangle, c1, c2, angle);
        G.FillRectangle(_Gradient, _Rectangle);
    }
}

// Abstract class for theme container control
public abstract class ThemeContainerControl : ContainerControl
{
    // Initialization
    protected Graphics G;
    protected Bitmap B;

    public ThemeContainerControl()
    {
        // Set control styles for optimization
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
        // Create a new 1x1 bitmap for drawing
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    // Allow transparency for control
    public void AllowTransparent()
    {
        SetStyle(ControlStyles.Opaque, false);
        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
    }

    // Convienence
    public abstract void PaintHook();

    // Override OnPaint to call PaintHook and draw the bitmap
    protected override sealed void OnPaint(PaintEventArgs e)
    {
        // Ignore if width or height is 0
        if (Width == 0 || Height == 0)
        {
            return;
        }

        // Call abstract PaintHook method
        PaintHook();

        // Draw the bitmap to the control
        e.Graphics.DrawImage(B, 0, 0);
    }

    // Handle control size changed event to resize the bitmap
    protected override void OnSizeChanged(EventArgs e)
    {
        // Ignore if width or height is 0
        if (Width == 0 || Height == 0)
        {
            return;
        }

        // Create a new bitmap with updated dimensions
        B = new Bitmap(Width, Height);
        G = Graphics.FromImage(B);

        // Invalidate control for redraw
        Invalidate();

        base.OnSizeChanged(e);
    }

    // Flag for disabling rounded corners
    private bool _NoRounding;

    public bool NoRounding
    {
        get { return _NoRounding; }
        set
        {
            _NoRounding = value;
            Invalidate();
        }
    }

    private Rectangle _Rectangle;
    private LinearGradientBrush _Gradient;

    // Draw corners of a rectangle
    protected void DrawCorners(Color c, Rectangle rect)
    {
        // Ignore if no rounding is enabled
        if (_NoRounding)
        {
            return;
        }

        // Set corner pixels
        B.SetPixel(rect.X, rect.Y, c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y, c);
        B.SetPixel(rect.X, rect.Y + (rect.Height - 1), c);
        B.SetPixel(rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), c);
    }

    // Draw borders of a rectangle
    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        // Draw outer border
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

        // Draw inner border
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    // Draw linear gradient with specified colors, position, and angle
    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        _Rectangle = new Rectangle(x, y, width, height);
        _Gradient = new LinearGradientBrush(_Rectangle, c1, c2, angle);
        G.FillRectangle(_Gradient, _Rectangle);
    }
}

public class TxtBox : ThemeControl
{
    private TextBox txtbox = new TextBox();
    private bool _passmask = false;

    public bool UseSystemPasswordChar
    {
        get { return _passmask; }
        set
        {
            _passmask = value;
            txtbox.UseSystemPasswordChar = value;
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
            txtbox.MaxLength = _maxchars;
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
            txtbox.TextAlign = value;
            Invalidate();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        base.OnPaintBackground(pevent);
        // Этот метод можно не переопределять, если нет необходимости.
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        txtbox.BackColor = BackColor; // Устанавливаем цвет фона для txtbox
        Invalidate();
    }

    protected override void OnForeColorChanged(EventArgs e)
    {
        base.OnForeColorChanged(e);
        txtbox.ForeColor = ForeColor; // Устанавливаем цвет текста для txtbox
        Invalidate();
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        txtbox.Font = Font; // Устанавливаем шрифт для txtbox
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        txtbox.Focus(); // Передаем фокус на txtbox
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        txtbox.Text = Text; // Обновляем текст в txtbox
        Invalidate();
    }

    public TxtBox()
    {
        Controls.Add(txtbox);

        // Настройки по умолчанию для TextBox
        txtbox.Multiline = false;
        txtbox.BackColor = Color.FromKnownColor(KnownColor.ControlLightLight); // Устанавливаем цвет фона
        txtbox.ForeColor = ForeColor; // Устанавливаем цвет текста
        txtbox.BorderStyle = BorderStyle.None;
        txtbox.Location = new Point(5, 8);
        txtbox.Size = new Size(Width - 10, Height - 16);

        // Обработчик для обновления текста
        txtbox.TextChanged += (sender, e) => Text = txtbox.Text;

        DoubleBuffered = true;
    }

    public override void PaintHook()
    {
        // Отрисовка фона
        G.Clear(BackColor);
        Pen p = new Pen(Color.FromArgb(204, 204, 204), 1);
        Pen o = new Pen(Color.FromArgb(249, 249, 249), 8);
        G.FillPath(Brushes.White, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 2));
        G.DrawPath(o, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 2));
        G.DrawPath(p, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 2));

        // Обновляем размеры txtbox
        txtbox.Width = Width - 12;
        txtbox.Height = Height - 16;
    }
}

public class PanelBox : ThemeContainerControl
{
    public PanelBox()
    {
        AllowTransparent();
    }

    public override void PaintHook()
    {
        Font = new Font("Tahoma", 10);
        ForeColor = Color.FromArgb(40, 40, 40);
        G.SmoothingMode = SmoothingMode.AntiAlias;
        G.FillRectangle(new SolidBrush(Color.FromArgb(235, 235, 235)), new Rectangle(2, 0, Width, Height));
        G.FillRectangle(new SolidBrush(Color.FromArgb(249, 249, 249)), new Rectangle(1, 0, Width - 3, Height - 4));
        G.DrawRectangle(new Pen(Color.FromArgb(214, 214, 214)), 0, 0, Width - 2, Height - 3);
    }
}

public class GroupDropBox : ThemeContainerControl
{
    private bool _Checked;
    private int X;
    private int y;
    private Size _OpenedSize;

    public bool Checked
    {
        get { return _Checked; }
        set
        {
            _Checked = value;
            Invalidate();
        }
    }

    public Size OpenSize
    {
        get { return _OpenedSize; }
        set
        {
            _OpenedSize = value;
            Invalidate();
        }
    }

    public GroupDropBox()
    {
        AllowTransparent();
        Size = new Size(90, 30);
        MinimumSize = new Size(5, 30);
        _Checked = true;
        Resize += GroupDropBox_Resize;
        MouseDown += GroupDropBox_MouseDown;
    }

    public override void PaintHook()
    {
        Font = new Font("Tahoma", 10);
        ForeColor = Color.FromArgb(40, 40, 40);
        G.SmoothingMode = SmoothingMode.AntiAlias;
        G.Clear(Color.FromArgb(245, 245, 245));
        G.FillRectangle(new SolidBrush(Color.FromArgb(231, 231, 231)), new Rectangle(0, 0, Width, 30));
        G.DrawLine(new Pen(_Checked ? Color.FromArgb(233, 238, 240) : Color.FromArgb(231, 236, 238)), 1, 1, Width - 2, 1);
        G.DrawRectangle(new Pen(Color.FromArgb(214, 214, 214)), 0, 0, Width - 1, Height - 1);
        G.DrawRectangle(new Pen(Color.FromArgb(214, 214, 214)), 0, 0, Width - 1, 30);
        Size = _Checked ? _OpenedSize : new Size(Width, 30);
        G.DrawString(_Checked ? "t" : "u", new Font("Marlett", 12), new SolidBrush(ForeColor), Width - 25, 5);
        G.DrawString(Text, Font, new SolidBrush(ForeColor), 7, 6);
    }

    private void GroupDropBox_Resize(object sender, EventArgs e)
    {
        if (_Checked)
        {
            _OpenedSize = Size;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        X = e.X;
        y = e.Y;
        Invalidate();
    }

    private void GroupDropBox_MouseDown(object sender, MouseEventArgs e)
    {
        if (X >= Width - 22 && y <= 30)
        {
            Checked = !Checked;
        }
    }
}

public class GroupPanelBox : ThemeContainerControl
{
    public GroupPanelBox()
    {
        AllowTransparent();
    }

    public override void PaintHook()
    {
        Font = new Font("Tahoma", 10);
        ForeColor = Color.FromArgb(40, 40, 40);
        G.SmoothingMode = SmoothingMode.AntiAlias;
        G.Clear(Color.FromArgb(245, 245, 245));
        G.FillRectangle(new SolidBrush(Color.FromArgb(231, 231, 231)), new Rectangle(0, 0, Width, 30));
        G.DrawLine(new Pen(Color.FromArgb(233, 238, 240)), 1, 1, Width - 2, 1);
        G.DrawRectangle(new Pen(Color.FromArgb(214, 214, 214)), 0, 0, Width - 1, Height - 1);
        G.DrawRectangle(new Pen(Color.FromArgb(214, 214, 214)), 0, 0, Width - 1, 30);
        G.DrawString(Text, Font, new SolidBrush(ForeColor), 7, 6);
    }
}

public class ButtonGreen : ThemeControl
{
    public override void PaintHook()
    {
        Font = new Font("Arial", 10);
        G.Clear(BackColor);
        G.SmoothingMode = SmoothingMode.HighQuality;

        switch (MouseState)
        {
            case State.MouseNone:
                using (Pen p1 = new Pen(Color.FromArgb(120, 159, 22), 1))
                using (LinearGradientBrush x1 = new LinearGradientBrush(ClientRectangle, Color.FromArgb(157, 209, 57), Color.FromArgb(130, 181, 18), LinearGradientMode.Vertical))
                {
                    G.FillPath(x1, Draw.RoundRect(ClientRectangle, 4));
                    G.DrawPath(p1, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 3));
                    G.DrawLine(new Pen(Color.FromArgb(190, 232, 109)), 2, 1, Width - 3, 1);
                    DrawText(HorizontalAlignment.Center, Color.FromArgb(240, 240, 240), 0);
                }
                break;

            case State.MouseDown:
                using (Pen p2 = new Pen(Color.FromArgb(120, 159, 22), 1))
                using (LinearGradientBrush x2 = new LinearGradientBrush(ClientRectangle, Color.FromArgb(125, 171, 25), Color.FromArgb(142, 192, 40), LinearGradientMode.Vertical))
                {
                    G.FillPath(x2, Draw.RoundRect(ClientRectangle, 4));
                    G.DrawPath(p2, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 3));
                    G.DrawLine(new Pen(Color.FromArgb(142, 172, 30)), 2, 1, Width - 3, 1);
                    DrawText(HorizontalAlignment.Center, Color.FromArgb(250, 250, 250), 1);
                }
                break;

            case State.MouseOver:
                using (Pen p3 = new Pen(Color.FromArgb(120, 159, 22), 1))
                using (LinearGradientBrush x3 = new LinearGradientBrush(ClientRectangle, Color.FromArgb(165, 220, 59), Color.FromArgb(137, 191, 18), LinearGradientMode.Vertical))
                {
                    G.FillPath(x3, Draw.RoundRect(ClientRectangle, 4));
                    G.DrawPath(p3, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 3));
                    G.DrawLine(new Pen(Color.FromArgb(190, 232, 109)), 2, 1, Width - 3, 1);
                    DrawText(HorizontalAlignment.Center, Color.FromArgb(240, 240, 240), -1);
                }
                break;
        }

        Cursor = Cursors.Hand;
    }
}

public class ButtonBlue : ThemeControl
{
    public override void PaintHook()
    {
        Font = new Font("Arial", 10);
        G.Clear(BackColor);
        G.SmoothingMode = SmoothingMode.HighQuality;

        switch (MouseState)
        {
            case State.MouseNone:
                using (Pen p = new Pen(Color.FromArgb(34, 112, 171), 1))
                using (LinearGradientBrush x = new LinearGradientBrush(ClientRectangle, Color.FromArgb(51, 159, 231), Color.FromArgb(33, 128, 206), LinearGradientMode.Vertical))
                {
                    G.FillPath(x, Draw.RoundRect(ClientRectangle, 4));
                    G.DrawPath(p, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 3));
                    G.DrawLine(new Pen(Color.FromArgb(131, 197, 241)), 2, 1, Width - 3, 1);
                    DrawText(HorizontalAlignment.Center, Color.FromArgb(240, 240, 240), 0);
                }
                break;

            case State.MouseDown:
                using (Pen p1 = new Pen(Color.FromArgb(34, 112, 171), 1))
                using (LinearGradientBrush x1 = new LinearGradientBrush(ClientRectangle, Color.FromArgb(37, 124, 196), Color.FromArgb(53, 153, 219), LinearGradientMode.Vertical))
                {
                    G.FillPath(x1, Draw.RoundRect(ClientRectangle, 4));
                    G.DrawPath(p1, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 3));
                    DrawText(HorizontalAlignment.Center, Color.FromArgb(250, 250, 250), 1);
                }
                break;

            case State.MouseOver:
                using (Pen p2 = new Pen(Color.FromArgb(34, 112, 171), 1))
                using (LinearGradientBrush x2 = new LinearGradientBrush(ClientRectangle, Color.FromArgb(54, 167, 243), Color.FromArgb(35, 165, 217), LinearGradientMode.Vertical))
                {
                    G.FillPath(x2, Draw.RoundRect(ClientRectangle, 4));
                    G.DrawPath(p2, Draw.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 3));
                    G.DrawLine(new Pen(Color.FromArgb(131, 197, 241)), 2, 1, Width - 3, 1);
                    DrawText(HorizontalAlignment.Center, Color.FromArgb(240, 240, 240), -1);
                }
                break;
        }

        Cursor = Cursors.Hand;
    }
}