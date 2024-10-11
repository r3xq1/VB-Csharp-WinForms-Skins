using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#region Основные классы дизайнера

public abstract class ThemeContainer154 : ContainerControl
{
    #region " Initialization "

    protected Graphics G;
    protected Bitmap B;

    public ThemeContainer154()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);

        _ImageSize = Size.Empty;
        Font = new Font("Verdana", 8);

        MeasureBitmap = new Bitmap(1, 1);
        MeasureGraphics = Graphics.FromImage(MeasureBitmap);

        DrawRadialPath = new GraphicsPath();

        InvalidateCustomization();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        if (DoneCreation)
            InitializeMessages();

        InvalidateCustomization();
        ColorHook();

        if (_LockWidth != 0) Width = _LockWidth;
        if (_LockHeight != 0) Height = _LockHeight;
        if (!_ControlMode) Dock = DockStyle.Fill;

        Transparent = _Transparent;
        if (_Transparent && _BackColor)
            BackColor = Color.Transparent;

        base.OnHandleCreated(e);
    }

    private bool DoneCreation;

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);

        if (Parent == null) return;
        _IsParentForm = Parent is Form;

        if (!_ControlMode)
        {
            InitializeMessages();

            if (_IsParentForm)
            {
                ParentForm.FormBorderStyle = _BorderStyle;
                ParentForm.TransparencyKey = _TransparencyKey;

                if (!DesignMode)
                    ParentForm.Shown += FormShown;
            }

            Parent.BackColor = BackColor;
        }

        OnCreation();
        DoneCreation = true;
        InvalidateTimer();
    }

    #endregion

    private void DoAnimation(bool animate)
    {
        OnAnimation();
        if (animate) Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;

        if (_Transparent && _ControlMode)
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

    protected override void OnHandleDestroyed(EventArgs e)
    {
        ThemeShare.RemoveAnimationCallback(DoAnimation);
        base.OnHandleDestroyed(e);
    }

    private bool HasShown;
    private void FormShown(object sender, EventArgs e)
    {
        if (_ControlMode || HasShown) return;

        if (_StartPosition == FormStartPosition.CenterParent || _StartPosition == FormStartPosition.CenterScreen)
        {
            Rectangle SB = Screen.PrimaryScreen.Bounds;
            Rectangle CB = ParentForm.Bounds;
            ParentForm.Location = new Point(SB.Width / 2 - CB.Width / 2, SB.Height / 2 - CB.Height / 2);
        }

        HasShown = true;
    }

    #region " Size Handling "

    private Rectangle Frame;

    protected override void OnSizeChanged(EventArgs e)
    {
        if (_Movable && !_ControlMode)
        {
            Frame = new Rectangle(7, 7, Width - 14, _Header - 7);
        }

        InvalidateBitmap();
        Invalidate();

        base.OnSizeChanged(e);
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if (_LockWidth != 0) width = _LockWidth;
        if (_LockHeight != 0) height = _LockHeight;
        base.SetBoundsCore(x, y, width, height, specified);
    }

    #endregion

    #region " State Handling "

    protected MouseState State;

    private void SetState(MouseState current)
    {
        State = current;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!(_IsParentForm && ParentForm.WindowState == FormWindowState.Maximized))
        {
            if (_Sizable && !_ControlMode) InvalidateMouse();
        }

        base.OnMouseMove(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        if (Enabled)
            SetState(MouseState.None);
        else
            SetState(MouseState.Block);

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

        if (GetChildAtPoint(PointToClient(MousePosition)) != null)
        {
            if (_Sizable && !_ControlMode)
            {
                Cursor = Cursors.Default;
                Previous = 0;
            }
        }

        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
            SetState(MouseState.Down);

        // Объединение условий для улучшения читаемости
        bool isMaximized = _IsParentForm && ParentForm.WindowState == FormWindowState.Maximized;

        if (!(isMaximized || _ControlMode))
        {
            if (_Movable && Frame.Contains(e.Location))
            {
                Capture = false;
                WM_LMBUTTONDOWN = true;
                DefWndProc(ref Messages[0]); // Передача по ссылке
            }
            else if (_Sizable && Previous != 0)
            {
                Capture = false;
                WM_LMBUTTONDOWN = true;
                DefWndProc(ref Messages[Previous]); // Передача по ссылке
            }
        }

        base.OnMouseDown(e);
    }

    private Message[] Messages = new Message[9];

    private void InitializeMessages()
    {
        Messages[0] = Message.Create(Parent.Handle, 161, new IntPtr(2), IntPtr.Zero);
        for (int i = 1; i <= 8; i++)
        {
            Messages[i] = Message.Create(Parent.Handle, 161, new IntPtr(i + 9), IntPtr.Zero);
        }
    }

    private bool WM_LMBUTTONDOWN;

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (WM_LMBUTTONDOWN && m.Msg == 513) // 513 is WM_LBUTTONUP
        {
            WM_LMBUTTONDOWN = false;

            SetState(MouseState.Over);
            if (!_SmartBounds) return;

            if (IsParentMdi)
            {
                CorrectBounds(new Rectangle(Point.Empty, Parent.Parent.Size));
            }
            else
            {
                CorrectBounds(Screen.FromControl(Parent).WorkingArea);
            }
        }
    }

    private Point GetIndexPoint;
    private bool B1, B2, B3, B4;

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

    private int Current, Previous;

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

    private void CorrectBounds(Rectangle bounds)
    {
        if (Parent.Width > bounds.Width) Parent.Width = bounds.Width;
        if (Parent.Height > bounds.Height) Parent.Height = bounds.Height;
        int x = Parent.Location.X;
        int y = Parent.Location.Y;

        if (x < bounds.X) x = bounds.X;
        if (y < bounds.Y) y = bounds.Y;

        int maxWidth = bounds.X + bounds.Width;
        int maxHeight = bounds.Y + bounds.Height;

        if (x + Parent.Width > maxWidth) x = maxWidth - Parent.Width;
        if (y + Parent.Height > maxHeight) y = maxHeight - Parent.Height;

        Parent.Location = new Point(x, y);
    }

    #endregion

    #region " Base Properties "

    public override DockStyle Dock
    {
        get { return base.Dock; }
        set
        {
            if (!_ControlMode) return;
            base.Dock = value;
        }
    }

    private bool _BackColor;

    [Category("Misc")]
    public override Color BackColor
    {
        get { return base.BackColor; }
        set
        {
            if (value == base.BackColor) return;

            if (!IsHandleCreated && _ControlMode && value == Color.Transparent)
            {
                _BackColor = true;
                return;
            }

            base.BackColor = value;
            if (Parent != null)
            {
                if (!_ControlMode) Parent.BackColor = value;
                ColorHook();
            }
        }
    }

    public override Size MinimumSize
    {
        get { return base.MinimumSize; }
        set
        {
            base.MinimumSize = value;
            if (Parent != null) Parent.MinimumSize = value;
        }
    }

    public override Size MaximumSize
    {
        get { return base.MaximumSize; }
        set
        {
            base.MaximumSize = value;
            if (Parent != null) Parent.MaximumSize = value;
        }
    }

    public override string Text
    {
        get { return base.Text; }
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    public override Font Font
    {
        get { return base.Font; }
        set
        {
            base.Font = value;
            Invalidate();
        }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Color ForeColor
    {
        get { return Color.Empty; }
        set { /* Intentionally left empty */ }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override Image BackgroundImage
    {
        get { return null; }
        set { /* Intentionally left empty */ }
    }

    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public override ImageLayout BackgroundImageLayout
    {
        get { return ImageLayout.None; }
        set { /* Intentionally left empty */ }
    }

    #endregion

    #region " Public Properties "

    private bool _SmartBounds = true;
    public bool SmartBounds
    {
        get { return _SmartBounds; }
        set { _SmartBounds = value; }
    }

    private bool _Movable = true;
    public bool Movable
    {
        get { return _Movable; }
        set { _Movable = value; }
    }

    private bool _Sizable = true;
    public bool Sizable
    {
        get { return _Sizable; }
        set { _Sizable = value; }
    }

    private Color _TransparencyKey;
    public Color TransparencyKey
    {
        get
        {
            if (_IsParentForm && !_ControlMode) return ParentForm.TransparencyKey;
            return _TransparencyKey;
        }
        set
        {
            if (value == _TransparencyKey) return;
            _TransparencyKey = value;

            if (_IsParentForm && !_ControlMode)
            {
                ParentForm.TransparencyKey = value;
                ColorHook();
            }
        }
    }

    private FormBorderStyle _BorderStyle;
    public FormBorderStyle BorderStyle
    {
        get
        {
            if (_IsParentForm && !_ControlMode) return ParentForm.FormBorderStyle;
            return _BorderStyle;
        }
        set
        {
            _BorderStyle = value;

            if (_IsParentForm && !_ControlMode)
            {
                ParentForm.FormBorderStyle = value;

                if (value != FormBorderStyle.None)
                {
                    Movable = false;
                    Sizable = false;
                }
            }
        }
    }

    private FormStartPosition _StartPosition;
    public FormStartPosition StartPosition
    {
        get
        {
            if (_IsParentForm && !_ControlMode) return ParentForm.StartPosition;
            return _StartPosition;
        }
        set
        {
            _StartPosition = value;

            if (_IsParentForm && !_ControlMode)
            {
                ParentForm.StartPosition = value;
            }
        }
    }

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
            if (value == null) _ImageSize = Size.Empty;
            else _ImageSize = value.Size;

            _Image = value;
            Invalidate();
        }
    }

    private Dictionary<string, Color> Items = new Dictionary<string, Color>();

    public Bloom[] Colors
    {
        get
        {
            List<Bloom> colorList = new List<Bloom>();
            foreach (var item in Items)
            {
                colorList.Add(new Bloom(item.Key, item.Value));
            }
            return colorList.ToArray();
        }
        set
        {
            foreach (Bloom bloom in value)
            {
                if (Items.ContainsKey(bloom.Name))
                    Items[bloom.Name] = bloom.Value;
            }

            InvalidateCustomization();
            ColorHook();
            Invalidate();
        }
    }

    private string _Customization;
    public string Customization
    {
        get { return _Customization; }
        set
        {
            if (value == _Customization) return;

            byte[] data;
            Bloom[] items = Colors;

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

    private bool _Transparent;
    public bool Transparent
    {
        get { return _Transparent; }
        set
        {
            _Transparent = value;
            if (!(IsHandleCreated || _ControlMode)) return;

            if (!value && BackColor.A != 255)
            {
                throw new Exception("Unable to change value to false while a transparent BackColor is in use.");
            }

            SetStyle(ControlStyles.Opaque, !value);
            SetStyle(ControlStyles.SupportsTransparentBackColor, value);

            InvalidateBitmap();
            Invalidate();
        }
    }

    #endregion

    #region " Private Properties "

    private Size _ImageSize;
    protected Size ImageSize
    {
        get { return _ImageSize; }
    }

    private bool _IsParentForm;
    protected bool IsParentForm
    {
        get { return _IsParentForm; }
    }

    protected bool IsParentMdi
    {
        get { return Parent != null && Parent.Parent != null; }
    }

    private int _LockWidth;
    protected int LockWidth
    {
        get { return _LockWidth; }
        set
        {
            _LockWidth = value;
            if (_LockWidth != 0 && IsHandleCreated) Width = _LockWidth;
        }
    }

    private int _LockHeight;
    protected int LockHeight
    {
        get { return _LockHeight; }
        set
        {
            _LockHeight = value;
            if (_LockHeight != 0 && IsHandleCreated) Height = _LockHeight;
        }
    }

    private int _Header = 24;
    protected int Header
    {
        get { return _Header; }
        set
        {
            _Header = value;
            if (!_ControlMode)
            {
                Frame = new Rectangle(7, 7, Width - 14, value - 7);
                Invalidate();
            }
        }
    }

    private bool _ControlMode;
    protected bool ControlMode
    {
        get { return _ControlMode; }
        set
        {
            _ControlMode = value;
            Transparent = _Transparent;
            if (_Transparent && _BackColor) BackColor = Color.Transparent;

            InvalidateBitmap();
            Invalidate();
        }
    }

    private bool _IsAnimated;
    protected bool IsAnimated
    {
        get { return _IsAnimated; }
        set
        {
            _IsAnimated = value;
            InvalidateTimer();
        }
    }

    #endregion

    #region " Property Helpers "

    protected Pen GetPen(string name)
    {
        return new Pen(Items[name]);
    }

    protected Pen GetPen(string name, float width)
    {
        return new Pen(Items[name], width);
    }

    protected SolidBrush GetBrush(string name)
    {
        return new SolidBrush(Items[name]);
    }

    protected Color GetColor(string name)
    {
        return Items[name];
    }

    protected void SetColor(string name, Color value)
    {
        if (Items.ContainsKey(name))
            Items[name] = value;
        else
            Items.Add(name, value);
    }

    protected void SetColor(string name, byte r, byte g, byte b)
    {
        SetColor(name, Color.FromArgb(r, g, b));
    }

    protected void SetColor(string name, byte a, byte r, byte g, byte b)
    {
        SetColor(name, Color.FromArgb(a, r, g, b));
    }

    protected void SetColor(string name, byte a, Color value)
    {
        SetColor(name, Color.FromArgb(a, value));
    }

    private void InvalidateBitmap()
    {
        if (_Transparent && _ControlMode)
        {
            if (Width == 0 || Height == 0) return;
            B = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
            G = Graphics.FromImage(B);
        }
        else
        {
            G = null;
            B = null;
        }
    }

    private void InvalidateCustomization()
    {
        using (MemoryStream m = new MemoryStream(Items.Count * 4))
        {
            foreach (Bloom b in Colors)
            {
                m.Write(BitConverter.GetBytes(b.Value.ToArgb()), 0, 4);
            }

            _Customization = Convert.ToBase64String(m.ToArray());
        }
    }

    private void InvalidateTimer()
    {
        if (DesignMode || !DoneCreation) return;

        if (_IsAnimated)
        {
            ThemeShare.AddAnimationCallback(DoAnimation);
        }
        else
        {
            ThemeShare.RemoveAnimationCallback(DoAnimation);
        }
    }

    #endregion

    #region " User Hooks "

    protected abstract void ColorHook();
    protected abstract void PaintHook();

    protected virtual void OnCreation() { }
    protected virtual void OnAnimation() { }

    #endregion

    #region " Offset "

    private Rectangle OffsetReturnRectangle;
    protected Rectangle Offset(Rectangle r, int amount)
    {
        OffsetReturnRectangle = new Rectangle(r.X + amount, r.Y + amount, r.Width - (amount * 2), r.Height - (amount * 2));
        return OffsetReturnRectangle;
    }

    private Size OffsetReturnSize;
    protected Size Offset(Size s, int amount)
    {
        OffsetReturnSize = new Size(s.Width + amount, s.Height + amount);
        return OffsetReturnSize;
    }

    private Point OffsetReturnPoint;
    protected Point Offset(Point p, int amount)
    {
        OffsetReturnPoint = new Point(p.X + amount, p.Y + amount);
        return OffsetReturnPoint;
    }

    #endregion

    #region " Center "

    private Point CenterReturn;

    protected Point Center(Rectangle p, Rectangle c)
    {
        CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X + c.X, (p.Height / 2 - c.Height / 2) + p.Y + c.Y);
        return CenterReturn;
    }

    protected Point Center(Rectangle p, Size c)
    {
        CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X, (p.Height / 2 - c.Height / 2) + p.Y);
        return CenterReturn;
    }

    protected Point Center(Rectangle child)
    {
        return Center(Width, Height, child.Width, child.Height);
    }

    protected Point Center(Size child)
    {
        return Center(Width, Height, child.Width, child.Height);
    }

    protected Point Center(int childWidth, int childHeight)
    {
        return Center(Width, Height, childWidth, childHeight);
    }

    protected Point Center(Size p, Size c)
    {
        return Center(p.Width, p.Height, c.Width, c.Height);
    }

    protected Point Center(int pWidth, int pHeight, int cWidth, int cHeight)
    {
        CenterReturn = new Point(pWidth / 2 - cWidth / 2, pHeight / 2 - cHeight / 2);
        return CenterReturn;
    }

    #endregion

    #region " Measure "

    private Bitmap MeasureBitmap;
    private Graphics MeasureGraphics;

    protected Size Measure()
    {
        lock (MeasureGraphics)
        {
            return MeasureGraphics.MeasureString(Text, Font, Width).ToSize();
        }
    }

    protected Size Measure(string text)
    {
        lock (MeasureGraphics)
        {
            return MeasureGraphics.MeasureString(text, Font, Width).ToSize();
        }
    }

    #endregion

    #region " DrawPixel "

    private SolidBrush DrawPixelBrush;

    protected void DrawPixel(Color c1, int x, int y)
    {
        if (_Transparent)
        {
            B.SetPixel(x, y, c1);
        }
        else
        {
            DrawPixelBrush = new SolidBrush(c1);
            G.FillRectangle(DrawPixelBrush, x, y, 1, 1);
        }
    }

    #endregion

    #region " DrawCorners "

    private SolidBrush DrawCornersBrush;

    protected void DrawCorners(Color c1, int offset)
    {
        DrawCorners(c1, 0, 0, Width, Height, offset);
    }

    protected void DrawCorners(Color c1, Rectangle r1, int offset)
    {
        DrawCorners(c1, r1.X, r1.Y, r1.Width, r1.Height, offset);
    }

    protected void DrawCorners(Color c1, int x, int y, int width, int height, int offset)
    {
        DrawCorners(c1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
    }

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
            DrawCornersBrush = new SolidBrush(c1);
            G.FillRectangle(DrawCornersBrush, x, y, 1, 1);
            G.FillRectangle(DrawCornersBrush, x + (width - 1), y, 1, 1);
            G.FillRectangle(DrawCornersBrush, x, y + (height - 1), 1, 1);
            G.FillRectangle(DrawCornersBrush, x + (width - 1), y + (height - 1), 1, 1);
        }
    }

    #endregion

    #region " DrawBorders "

    #region DrawBorders

    protected void DrawBorders(Pen p1, int offset)
    {
        DrawBorders(p1, 0, 0, Width, Height, offset);
    }

    protected void DrawBorders(Pen p1, Rectangle r, int offset)
    {
        DrawBorders(p1, r.X, r.Y, r.Width, r.Height, offset);
    }

    protected void DrawBorders(Pen p1, int x, int y, int width, int height, int offset)
    {
        DrawBorders(p1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
    }

    protected void DrawBorders(Pen p1)
    {
        DrawBorders(p1, 0, 0, Width, Height);
    }

    protected void DrawBorders(Pen p1, Rectangle r)
    {
        DrawBorders(p1, r.X, r.Y, r.Width, r.Height);
    }

    protected void DrawBorders(Pen p1, int x, int y, int width, int height)
    {
        G.DrawRectangle(p1, x, y, width - 1, height - 1);
    }

    #endregion

    #endregion


    #region " DrawText "

    private Point DrawTextPoint;
    private Size DrawTextSize;

    protected void DrawText(Brush b1, HorizontalAlignment a, int x, int y)
    {
        DrawText(b1, Text, a, x, y);
    }

    protected void DrawText(Brush b1, string text, HorizontalAlignment a, int x, int y)
    {
        if (text.Length == 0) return;

        DrawTextSize = Measure(text);
        DrawTextPoint = new Point(Width / 2 - DrawTextSize.Width / 2, Header / 2 - DrawTextSize.Height / 2);

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawString(text, Font, b1, x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawString(text, Font, b1, DrawTextPoint.X + x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawString(text, Font, b1, Width - DrawTextSize.Width - x, DrawTextPoint.Y + y);
                break;
        }
    }

    protected void DrawText(Brush b1, Point p1)
    {
        if (Text.Length == 0) return;
        G.DrawString(Text, Font, b1, p1);
    }

    protected void DrawText(Brush b1, int x, int y)
    {
        if (Text.Length == 0) return;
        G.DrawString(Text, Font, b1, x, y);
    }

    #endregion

    #region " DrawImage "

    private Point DrawImagePoint;

    protected void DrawImage(HorizontalAlignment a, int x, int y)
    {
        DrawImage(_Image, a, x, y);
    }

    protected void DrawImage(Image image, HorizontalAlignment a, int x, int y)
    {
        if (image == null) return;
        DrawImagePoint = new Point(Width / 2 - image.Width / 2, Header / 2 - image.Height / 2);

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawImage(image, x, DrawImagePoint.Y + y, image.Width, image.Height);
                break;
            case HorizontalAlignment.Center:
                G.DrawImage(image, DrawImagePoint.X + x, DrawImagePoint.Y + y, image.Width, image.Height);
                break;
            case HorizontalAlignment.Right:
                G.DrawImage(image, Width - image.Width - x, DrawImagePoint.Y + y, image.Width, image.Height);
                break;
        }
    }

    protected void DrawImage(Point p1)
    {
        DrawImage(_Image, p1.X, p1.Y);
    }

    protected void DrawImage(int x, int y)
    {
        DrawImage(_Image, x, y);
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

    #region " DrawGradient "

    private LinearGradientBrush DrawGradientBrush;
    private Rectangle DrawGradientRectangle;

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(blend, DrawGradientRectangle);
    }

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(blend, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r)
    {
        DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, 90.0F);
        DrawGradientBrush.InterpolationColors = blend;
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle);
        DrawGradientBrush.InterpolationColors = blend;
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(c1, c2, DrawGradientRectangle);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(c1, c2, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r)
    {
        DrawGradientBrush = new LinearGradientBrush(r, c1, c2, 90.0F);
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, c1, c2, angle);
        G.FillRectangle(DrawGradientBrush, r);
    }

    #endregion

    #region " DrawRadial "

    private GraphicsPath DrawRadialPath;
    private PathGradientBrush DrawRadialBrush1;
    private LinearGradientBrush DrawRadialBrush2;
    private Rectangle DrawRadialRectangle;

    public void DrawRadial(ColorBlend blend, int x, int y, int width, int height)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(blend, DrawRadialRectangle, width / 2, height / 2);
    }

    public void DrawRadial(ColorBlend blend, int x, int y, int width, int height, Point center)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(blend, DrawRadialRectangle, center.X, center.Y);
    }

    public void DrawRadial(ColorBlend blend, int x, int y, int width, int height, int cx, int cy)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(blend, DrawRadialRectangle, cx, cy);
    }

    public void DrawRadial(ColorBlend blend, Rectangle r)
    {
        DrawRadial(blend, r, r.Width / 2, r.Height / 2);
    }

    public void DrawRadial(ColorBlend blend, Rectangle r, Point center)
    {
        DrawRadial(blend, r, center.X, center.Y);
    }

    public void DrawRadial(ColorBlend blend, Rectangle r, int cx, int cy)
    {
        DrawRadialPath.Reset();
        DrawRadialPath.AddEllipse(r.X, r.Y, r.Width - 1, r.Height - 1);

        DrawRadialBrush1 = new PathGradientBrush(DrawRadialPath)
        {
            CenterPoint = new Point(r.X + cx, r.Y + cy),
            InterpolationColors = blend
        };

        if (G.SmoothingMode == SmoothingMode.AntiAlias)
        {
            G.FillEllipse(DrawRadialBrush1, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
        }
        else
        {
            G.FillEllipse(DrawRadialBrush1, r);
        }
    }

    protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(c1, c2, DrawRadialRectangle);
    }

    protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(c1, c2, DrawRadialRectangle, angle);
    }

    protected void DrawRadial(Color c1, Color c2, Rectangle r)
    {
        DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, 90.0F);
        G.FillRectangle(DrawRadialBrush2, r);
    }

    protected void DrawRadial(Color c1, Color c2, Rectangle r, float angle)
    {
        DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, angle);
        G.FillEllipse(DrawRadialBrush2, r);
    }

    #endregion

    #region " CreateRound "

    private GraphicsPath CreateRoundPath;
    private Rectangle CreateRoundRectangle;

    public GraphicsPath CreateRound(int x, int y, int width, int height, int slope)
    {
        CreateRoundRectangle = new Rectangle(x, y, width, height);
        return CreateRound(CreateRoundRectangle, slope);
    }

    public GraphicsPath CreateRound(Rectangle r, int slope)
    {
        CreateRoundPath = new GraphicsPath(FillMode.Winding);
        CreateRoundPath.AddArc(r.X, r.Y, slope, slope, 180.0F, 90.0F);
        CreateRoundPath.AddArc(r.Right - slope, r.Y, slope, slope, 270.0F, 90.0F);
        CreateRoundPath.AddArc(r.Right - slope, r.Bottom - slope, slope, slope, 0.0F, 90.0F);
        CreateRoundPath.AddArc(r.X, r.Bottom - slope, slope, slope, 90.0F, 90.0F);
        CreateRoundPath.CloseFigure();
        return CreateRoundPath;
    }

    #endregion
}

public abstract class ThemeControl154 : Control
{
    #region " Initialization "

    protected Graphics G;
    protected Bitmap B;

    public ThemeControl154()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

        _ImageSize = Size.Empty;
        Font = new Font("Verdana", 8);

        MeasureBitmap = new Bitmap(1, 1);
        MeasureGraphics = Graphics.FromImage(MeasureBitmap);

        DrawRadialPath = new GraphicsPath();

        InvalidateCustomization(); // Remove?
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        InvalidateCustomization();
        ColorHook();

        if (_LockWidth != 0) Width = _LockWidth;
        if (_LockHeight != 0) Height = _LockHeight;

        Transparent = _Transparent;
        if (_Transparent && _BackColor) BackColor = Color.Transparent;

        base.OnHandleCreated(e);
    }

    private bool DoneCreation;
    protected override void OnParentChanged(EventArgs e)
    {
        if (Parent != null)
        {
            OnCreation();
            DoneCreation = true;
            InvalidateTimer();
        }
        base.OnParentChanged(e);
    }

    #endregion

    private void DoAnimation(bool i)
    {
        OnAnimation();
        if (i) Invalidate();
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

    protected override void OnHandleDestroyed(EventArgs e)
    {
        ThemeShare.RemoveAnimationCallback(DoAnimation);
        base.OnHandleDestroyed(e);
    }

    #region " Size Handling "

    protected override void OnSizeChanged(EventArgs e)
    {
        if (_Transparent)
        {
            InvalidateBitmap();
        }

        Invalidate();
        base.OnSizeChanged(e);
    }

    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if (_LockWidth != 0) width = _LockWidth;
        if (_LockHeight != 0) height = _LockHeight;
        base.SetBoundsCore(x, y, width, height, specified);
    }

    #endregion

    #region " State Handling "

    private bool InPosition;
    protected override void OnMouseEnter(EventArgs e)
    {
        InPosition = true;
        SetState(MouseState.Over);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (InPosition) SetState(MouseState.Over);
        base.OnMouseUp(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) SetState(MouseState.Down);
        base.OnMouseDown(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        InPosition = false;
        SetState(MouseState.None);
        base.OnMouseLeave(e);
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        if (Enabled) SetState(MouseState.None);
        else SetState(MouseState.Block);
        base.OnEnabledChanged(e);
    }

    protected MouseState State;

    private void SetState(MouseState current)
    {
        State = current;
        Invalidate();
    }

    #endregion

    #region " Base Properties "

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

    private bool _BackColor;

    [Category("Misc")]
    public override Color BackColor
    {
        get => base.BackColor;
        set
        {
            if (!IsHandleCreated && value == Color.Transparent)
            {
                _BackColor = true;
                return;
            }

            base.BackColor = value;
            if (Parent != null) ColorHook();
        }
    }

    #endregion

    #region " Public Properties "

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
            if (value == null)
            {
                _ImageSize = Size.Empty;
            }
            else
            {
                _ImageSize = value.Size;
            }

            _Image = value;
            Invalidate();
        }
    }

    private bool _Transparent;
    public bool Transparent
    {
        get => _Transparent;
        set
        {
            _Transparent = value;
            if (!IsHandleCreated) return;

            if (!value && BackColor.A != 255)
            {
                throw new Exception("Unable to change value to false while a transparent BackColor is in use.");
            }

            SetStyle(ControlStyles.Opaque, !value);
            SetStyle(ControlStyles.SupportsTransparentBackColor, value);

            if (value) InvalidateBitmap();
            else B = null;

            Invalidate();
        }
    }

    private Dictionary<string, Color> Items = new Dictionary<string, Color>();
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
            foreach (Bloom B in value)
            {
                if (Items.ContainsKey(B.Name)) Items[B.Name] = B.Value;
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

            byte[] Data;
            Bloom[] Items = Colors;

            try
            {
                Data = Convert.FromBase64String(value);
                for (int I = 0; I < Items.Length; I++)
                {
                    Items[I].Value = Color.FromArgb(BitConverter.ToInt32(Data, I * 4));
                }
            }
            catch
            {
                return;
            }

            _Customization = value;

            Colors = Items;
            ColorHook();
            Invalidate();
        }
    }

    #endregion

    #region " Private Properties "

    private Size _ImageSize;
    protected Size ImageSize
    {
        get => _ImageSize;
    }

    private int _LockWidth;
    protected int LockWidth
    {
        get => _LockWidth;
        set
        {
            _LockWidth = value;
            if (_LockWidth != 0 && IsHandleCreated) Width = _LockWidth;
        }
    }

    private int _LockHeight;
    protected int LockHeight
    {
        get => _LockHeight;
        set
        {
            _LockHeight = value;
            if (_LockHeight != 0 && IsHandleCreated) Height = _LockHeight;
        }
    }

    private bool _IsAnimated;
    protected bool IsAnimated
    {
        get => _IsAnimated;
        set
        {
            _IsAnimated = value;
            InvalidateTimer();
        }
    }

    #endregion

    #region " Property Helpers "

    protected Pen GetPen(string name)
    {
        return new Pen(Items[name]);
    }

    protected Pen GetPen(string name, float width)
    {
        return new Pen(Items[name], width);
    }

    protected SolidBrush GetBrush(string name)
    {
        return new SolidBrush(Items[name]);
    }

    protected Color GetColor(string name)
    {
        return Items[name];
    }

    protected void SetColor(string name, Color value)
    {
        if (Items.ContainsKey(name))
            Items[name] = value;
        else
            Items.Add(name, value);
    }

    protected void SetColor(string name, byte r, byte g, byte b)
    {
        SetColor(name, Color.FromArgb(r, g, b));
    }

    protected void SetColor(string name, byte a, byte r, byte g, byte b)
    {
        SetColor(name, Color.FromArgb(a, r, g, b));
    }

    protected void SetColor(string name, byte a, Color value)
    {
        SetColor(name, Color.FromArgb(a, value));
    }

    private void InvalidateBitmap()
    {
        if (Width == 0 || Height == 0) return;
        B = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);
        G = Graphics.FromImage(B);
    }

    private void InvalidateCustomization()
    {
        using (var M = new MemoryStream(Items.Count * 4))
        {
            foreach (Bloom B in Colors)
            {
                M.Write(BitConverter.GetBytes(B.Value.ToArgb()), 0, 4);
            }

            _Customization = Convert.ToBase64String(M.ToArray());
        }
    }

    private void InvalidateTimer()
    {
        if (DesignMode || !DoneCreation) return;

        if (_IsAnimated)
            ThemeShare.AddAnimationCallback(DoAnimation);
        else
            ThemeShare.RemoveAnimationCallback(DoAnimation);
    }

    #endregion

    #region " User Hooks "

    protected abstract void ColorHook();
    protected abstract void PaintHook();

    protected virtual void OnCreation()
    {
    }

    protected virtual void OnAnimation()
    {
    }

    #endregion

    #region " Offset "

    private Rectangle OffsetReturnRectangle;
    protected Rectangle Offset(Rectangle r, int amount)
    {
        OffsetReturnRectangle = new Rectangle(r.X + amount, r.Y + amount, r.Width - (amount * 2), r.Height - (amount * 2));
        return OffsetReturnRectangle;
    }

    private Size OffsetReturnSize;
    protected Size Offset(Size s, int amount)
    {
        OffsetReturnSize = new Size(s.Width + amount, s.Height + amount);
        return OffsetReturnSize;
    }

    private Point OffsetReturnPoint;
    protected Point Offset(Point p, int amount)
    {
        OffsetReturnPoint = new Point(p.X + amount, p.Y + amount);
        return OffsetReturnPoint;
    }

    #endregion

    #region " Center "

    private Point CenterReturn;

    protected Point Center(Rectangle p, Rectangle c)
    {
        CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X + c.X, (p.Height / 2 - c.Height / 2) + p.Y + c.Y);
        return CenterReturn;
    }

    protected Point Center(Rectangle p, Size c)
    {
        CenterReturn = new Point((p.Width / 2 - c.Width / 2) + p.X, (p.Height / 2 - c.Height / 2) + p.Y);
        return CenterReturn;
    }

    protected Point Center(Rectangle child)
    {
        return Center(Width, Height, child.Width, child.Height);
    }

    protected Point Center(Size child)
    {
        return Center(Width, Height, child.Width, child.Height);
    }

    protected Point Center(int childWidth, int childHeight)
    {
        return Center(Width, Height, childWidth, childHeight);
    }

    protected Point Center(Size p, Size c)
    {
        return Center(p.Width, p.Height, c.Width, c.Height);
    }

    protected Point Center(int pWidth, int pHeight, int cWidth, int cHeight)
    {
        CenterReturn = new Point(pWidth / 2 - cWidth / 2, pHeight / 2 - cHeight / 2);
        return CenterReturn;
    }

    #endregion

    #region " Measure "

    private Bitmap MeasureBitmap;
    private Graphics MeasureGraphics; // TODO: Potential issues during multi-threading.

    protected Size Measure()
    {
        return MeasureGraphics.MeasureString(Text, Font, Width).ToSize();
    }

    protected Size Measure(string text)
    {
        return MeasureGraphics.MeasureString(text, Font, Width).ToSize();
    }

    #endregion

    #region " DrawPixel "

    private SolidBrush DrawPixelBrush;

    protected void DrawPixel(Color c1, int x, int y)
    {
        if (_Transparent)
        {
            B.SetPixel(x, y, c1);
        }
        else
        {
            DrawPixelBrush = new SolidBrush(c1);
            G.FillRectangle(DrawPixelBrush, x, y, 1, 1);
        }
    }

    #endregion

    #region " DrawCorners "

    private SolidBrush DrawCornersBrush;

    protected void DrawCorners(Color c1, int offset)
    {
        DrawCorners(c1, 0, 0, Width, Height, offset);
    }

    protected void DrawCorners(Color c1, Rectangle r1, int offset)
    {
        DrawCorners(c1, r1.X, r1.Y, r1.Width, r1.Height, offset);
    }

    protected void DrawCorners(Color c1, int x, int y, int width, int height, int offset)
    {
        DrawCorners(c1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
    }

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
            DrawCornersBrush = new SolidBrush(c1);
            G.FillRectangle(DrawCornersBrush, x, y, 1, 1);
            G.FillRectangle(DrawCornersBrush, x + (width - 1), y, 1, 1);
            G.FillRectangle(DrawCornersBrush, x, y + (height - 1), 1, 1);
            G.FillRectangle(DrawCornersBrush, x + (width - 1), y + (height - 1), 1, 1);
        }
    }

    #endregion

    #region " DrawBorders "

    protected void DrawBorders(Pen p1, int offset)
    {
        DrawBorders(p1, 0, 0, Width, Height, offset);
    }

    protected void DrawBorders(Pen p1, Rectangle r, int offset)
    {
        DrawBorders(p1, r.X, r.Y, r.Width, r.Height, offset);
    }

    protected void DrawBorders(Pen p1, int x, int y, int width, int height, int offset)
    {
        DrawBorders(p1, x + offset, y + offset, width - (offset * 2), height - (offset * 2));
    }

    protected void DrawBorders(Pen p1)
    {
        DrawBorders(p1, 0, 0, Width, Height);
    }

    protected void DrawBorders(Pen p1, Rectangle r)
    {
        DrawBorders(p1, r.X, r.Y, r.Width, r.Height);
    }

    protected void DrawBorders(Pen p1, int x, int y, int width, int height)
    {
        G.DrawRectangle(p1, x, y, width - 1, height - 1);
    }

    #endregion

    #region " DrawText "

    private Point DrawTextPoint;
    private Size DrawTextSize;

    protected void DrawText(Brush b1, HorizontalAlignment a, int x, int y)
    {
        DrawText(b1, Text, a, x, y);
    }

    protected void DrawText(Brush b1, string text, HorizontalAlignment a, int x, int y)
    {
        if (text.Length == 0) return;

        DrawTextSize = Measure(text);
        DrawTextPoint = Center(DrawTextSize);

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawString(text, Font, b1, x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                G.DrawString(text, Font, b1, DrawTextPoint.X + x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                G.DrawString(text, Font, b1, Width - DrawTextSize.Width - x, DrawTextPoint.Y + y);
                break;
        }
    }

    protected void DrawText(Brush b1, Point p1)
    {
        if (Text.Length == 0) return;
        G.DrawString(Text, Font, b1, p1);
    }

    protected void DrawText(Brush b1, int x, int y)
    {
        if (Text.Length == 0) return;
        G.DrawString(Text, Font, b1, x, y);
    }

    #endregion

    #region " DrawImage "

    private Point DrawImagePoint;

    protected void DrawImage(HorizontalAlignment a, int x, int y)
    {
        DrawImage(_Image, a, x, y);
    }

    protected void DrawImage(Image image, HorizontalAlignment a, int x, int y)
    {
        if (image == null) return;
        DrawImagePoint = Center(image.Size);

        switch (a)
        {
            case HorizontalAlignment.Left:
                G.DrawImage(image, x, DrawImagePoint.Y + y, image.Width, image.Height);
                break;
            case HorizontalAlignment.Center:
                G.DrawImage(image, DrawImagePoint.X + x, DrawImagePoint.Y + y, image.Width, image.Height);
                break;
            case HorizontalAlignment.Right:
                G.DrawImage(image, Width - image.Width - x, DrawImagePoint.Y + y, image.Width, image.Height);
                break;
        }
    }

    protected void DrawImage(Point p1)
    {
        DrawImage(_Image, p1.X, p1.Y);
    }

    protected void DrawImage(int x, int y)
    {
        DrawImage(_Image, x, y);
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

    #region " DrawGradient "

    private LinearGradientBrush DrawGradientBrush;
    private Rectangle DrawGradientRectangle;

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(blend, DrawGradientRectangle);
    }

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(blend, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r)
    {
        DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, 90.0f);
        DrawGradientBrush.InterpolationColors = blend;
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle);
        DrawGradientBrush.InterpolationColors = blend;
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(c1, c2, DrawGradientRectangle);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(c1, c2, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r)
    {
        DrawGradientBrush = new LinearGradientBrush(r, c1, c2, 90.0f);
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, c1, c2, angle);
        G.FillRectangle(DrawGradientBrush, r);
    }

    #endregion

    #region " DrawRadial "

    private GraphicsPath DrawRadialPath;
    private PathGradientBrush DrawRadialBrush1;
    private LinearGradientBrush DrawRadialBrush2;
    private Rectangle DrawRadialRectangle;

    protected void DrawRadial(ColorBlend blend, int x, int y, int width, int height)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(blend, DrawRadialRectangle, width / 2, height / 2);
    }

    protected void DrawRadial(ColorBlend blend, int x, int y, int width, int height, Point center)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(blend, DrawRadialRectangle, center.X, center.Y);
    }

    protected void DrawRadial(ColorBlend blend, int x, int y, int width, int height, int cx, int cy)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(blend, DrawRadialRectangle, cx, cy);
    }

    protected void DrawRadial(ColorBlend blend, Rectangle r)
    {
        DrawRadial(blend, r, r.Width / 2, r.Height / 2);
    }

    protected void DrawRadial(ColorBlend blend, Rectangle r, Point center)
    {
        DrawRadial(blend, r, center.X, center.Y);
    }

    protected void DrawRadial(ColorBlend blend, Rectangle r, int cx, int cy)
    {
        DrawRadialPath.Reset();
        DrawRadialPath.AddEllipse(r.X, r.Y, r.Width - 1, r.Height - 1);

        DrawRadialBrush1 = new PathGradientBrush(DrawRadialPath)
        {
            CenterPoint = new Point(r.X + cx, r.Y + cy),
            InterpolationColors = blend
        };

        if (G.SmoothingMode == SmoothingMode.AntiAlias)
        {
            G.FillEllipse(DrawRadialBrush1, r.X + 1, r.Y + 1, r.Width - 3, r.Height - 3);
        }
        else
        {
            G.FillEllipse(DrawRadialBrush1, r);
        }
    }

    protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(c1, c2, DrawRadialRectangle);
    }

    protected void DrawRadial(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        DrawRadialRectangle = new Rectangle(x, y, width, height);
        DrawRadial(c1, c2, DrawRadialRectangle, angle);
    }

    protected void DrawRadial(Color c1, Color c2, Rectangle r)
    {
        DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, 90.0f);
        G.FillEllipse(DrawRadialBrush2, r);
    }

    protected void DrawRadial(Color c1, Color c2, Rectangle r, float angle)
    {
        DrawRadialBrush2 = new LinearGradientBrush(r, c1, c2, angle);
        G.FillEllipse(DrawRadialBrush2, r);
    }

    #endregion

    #region " CreateRound "

    private GraphicsPath CreateRoundPath;
    private Rectangle CreateRoundRectangle;

    public GraphicsPath CreateRound(int x, int y, int width, int height, int slope)
    {
        CreateRoundRectangle = new Rectangle(x, y, width, height);
        return CreateRound(CreateRoundRectangle, slope);
    }

    public GraphicsPath CreateRound(Rectangle r, int slope)
    {
        CreateRoundPath = new GraphicsPath(FillMode.Winding);
        CreateRoundPath.AddArc(r.X, r.Y, slope, slope, 180.0f, 90.0f);
        CreateRoundPath.AddArc(r.Right - slope, r.Y, slope, slope, 270.0f, 90.0f);
        CreateRoundPath.AddArc(r.Right - slope, r.Bottom - slope, slope, slope, 0.0f, 90.0f);
        CreateRoundPath.AddArc(r.X, r.Bottom - slope, slope, slope, 90.0f, 90.0f);
        CreateRoundPath.CloseFigure();
        return CreateRoundPath;
    }

    #endregion
}

public static class ThemeShare
{
    #region " Animation "

    private static int frames;
    private static bool invalidate;
    public static PrecisionTimer ThemeTimer = new PrecisionTimer();

    private const int FPS = 50; // 1000 / 50 = 20 FPS
    private const int Rate = 10;

    public delegate void AnimationDelegate(bool invalidate);

    private static readonly List<AnimationDelegate> callbacks = new List<AnimationDelegate>();

    private static void HandleCallbacks(IntPtr state, bool reserve)
    {
        invalidate = (frames >= FPS);
        if (invalidate) frames = 0;

        lock (callbacks)
        {
            foreach (var callback in callbacks)
            {
                callback.Invoke(invalidate);
            }
        }

        frames += Rate;
    }

    private static void InvalidateThemeTimer()
    {
        if (callbacks.Count == 0)
        {
            ThemeTimer.Delete();
        }
        else
        {
            ThemeTimer.Create(0, Rate, HandleCallbacks);
        }
    }

    public static void AddAnimationCallback(AnimationDelegate callback)
    {
        lock (callbacks)
        {
            if (!callbacks.Contains(callback))
            {
                callbacks.Add(callback);
                InvalidateThemeTimer();
            }
        }
    }

    public static void RemoveAnimationCallback(AnimationDelegate callback)
    {
        lock (callbacks)
        {
            if (callbacks.Contains(callback))
            {
                callbacks.Remove(callback);
                InvalidateThemeTimer();
            }
        }
    }

    #endregion
}

public enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

public struct Bloom(string name, Color value)
{
    private Color _Value = value;

    public string Name { get; } = name;

    public Color Value
    {
        get => _Value;
        set => _Value = value;
    }

    public string ValueHex
    {
        get => $"#{_Value.R:X2}{_Value.G:X2}{_Value.B:X2}";
        set
        {
            try
            {
                _Value = ColorTranslator.FromHtml(value);
            }
            catch
            {
                // Ignore exceptions
            }
        }
    }
}

public class PrecisionTimer : IDisposable
{
    private bool _Enabled;

    public bool Enabled => _Enabled;

    private IntPtr Handle;
    public TimerDelegate TimerCallback; // Изменено на public

    [DllImport("kernel32.dll", EntryPoint = "CreateTimerQueueTimer")]
    private static extern bool CreateTimerQueueTimer(
        ref IntPtr handle,
        IntPtr queue,
        TimerDelegate callback,
        IntPtr state,
        uint dueTime,
        uint period,
        uint flags);

    [DllImport("kernel32.dll", EntryPoint = "DeleteTimerQueueTimer")]
    private static extern bool DeleteTimerQueueTimer(
        IntPtr queue,
        IntPtr handle,
        IntPtr callback);

    public delegate void TimerDelegate(IntPtr r1, bool r2); // Если нужно оставить делегат внутренним, оставьте его здесь.

    public void Create(uint dueTime, uint period, TimerDelegate callback)
    {
        if (_Enabled) return;

        TimerCallback = callback;
        bool success = CreateTimerQueueTimer(ref Handle, IntPtr.Zero, TimerCallback, IntPtr.Zero, dueTime, period, 0);

        if (!success) ThrowNewException("CreateTimerQueueTimer");
        _Enabled = success;
    }

    public void Delete()
    {
        if (!_Enabled) return;

        bool success = DeleteTimerQueueTimer(IntPtr.Zero, Handle, IntPtr.Zero);

        if (!success && Marshal.GetLastWin32Error() != 997)
        {
            ThrowNewException("DeleteTimerQueueTimer");
        }

        _Enabled = !success;
    }

    private void ThrowNewException(string name)
    {
        throw new Exception($"{name} failed. Win32Error: {Marshal.GetLastWin32Error()}");
    }

    public void Dispose()
    {
        Delete();
    }
}

#endregion

#region Дизайн компонентов

public class Theme : ThemeContainer154
{
    private Color G1, G2, Glow, BG, Edge;
    private RoundingType _Rounding;
    private Color _backColor, _foreColor; // Переменные для цвета фона и текста

    public enum RoundingType
    {
        TypeOne = 1,
        TypeTwo = 2,
        None = 0
    }

    protected override void ColorHook()
    {
        G1 = GetColor("Gradient 1");
        G2 = GetColor("Gradient 2");
        Glow = GetColor("Glow");
        BG = GetColor("Background");
        Edge = GetColor("Edges");
    }

    public RoundingType Rounding
    {
        get => _Rounding;
        set => _Rounding = value;
    }

    [Browsable(true)]
    public new Color BackColor
    {
        get => _backColor;
        set
        {
            _backColor = value;
            Invalidate(); // Перерисовка компонента для применения нового цвета
        }
    }

    [Browsable(true)]
    public new Color ForeColor
    {
        get => _foreColor;
        set
        {
            _foreColor = value;
            Invalidate(); // Перерисовка компонента для применения нового цвета
        }
    }

    public Theme()
    {
        ControlMode = true;
        Dock = DockStyle.Fill;
        SetColor("Gradient 1", Color.FromArgb(30, 30, 30));
        SetColor("Gradient 2", Color.FromArgb(50, 50, 50));
        SetColor("Glow", Color.FromArgb(240, 240, 240));
        SetColor("Background", Color.FromArgb(240, 240, 240));
        SetColor("Edges", Color.FromArgb(50, 50, 50));
        TransparencyKey = Color.Fuchsia;
        MinimumSize = new Size(175, 150);
        _backColor = Color.FromArgb(240, 240, 240);
        _foreColor = Color.White;
        StartPosition = FormStartPosition.CenterScreen;
    }

    protected override void PaintHook()
    {
        G.Clear(_backColor); // Используем новое свойство BackColor для очистки

        G.DrawRectangle(new Pen(Edge), new Rectangle(0, 0, Width - 1, Height - 1));
        G.DrawLine(new Pen(Edge), new Point(0, 26), new Point(Width, 26));

        var LB = new LinearGradientBrush(new Rectangle(new Point(1, 1), new Size(Width - 2, 25)), G2, G1, 90.0F);
        G.FillRectangle(LB, new Rectangle(new Point(1, 1), new Size(Width - 2, 25)));

        // Draw glow
        G.FillRectangle(new SolidBrush(G2), new Rectangle(new Point(1, 1), new Size(Width - 2, 11)));

        // Заголовок от формы
        G.DrawString(Text, new Font("Segoe UI", 9), new SolidBrush(_foreColor), new Point(5, 4)); // Используем _foreColor

        switch (_Rounding)
        {
            case RoundingType.TypeOne:
                DrawCornersTypeOne();
                break;
            case RoundingType.TypeTwo:
                DrawCornersTypeTwo();
                break;
        }
    }

    private void DrawCornersTypeOne()
    {
        // Левый верхний угол
        DrawPixel(Color.Fuchsia, 0, 0);
        DrawPixel(Color.Fuchsia, 1, 0);
        DrawPixel(Color.Fuchsia, 0, 1);
        DrawPixel(Edge, 1, 1);

        // Правый верхний угол
        DrawPixel(Color.Fuchsia, Width - 1, 0);
        DrawPixel(Color.Fuchsia, Width - 2, 0);
        DrawPixel(Color.Fuchsia, Width - 1, 1);
        DrawPixel(Edge, Width - 2, 1);

        // Левый нижний угол
        DrawPixel(Color.Fuchsia, 0, Height - 1);
        DrawPixel(Color.Fuchsia, 1, Height - 1);
        DrawPixel(Color.Fuchsia, 0, Height - 2);
        DrawPixel(Edge, 1, Height - 2);

        // Правый нижний угол
        DrawPixel(Color.Fuchsia, Width - 1, Height - 1);
        DrawPixel(Color.Fuchsia, Width - 2, Height - 1);
        DrawPixel(Color.Fuchsia, Width - 1, Height - 2);
        DrawPixel(Edge, Width - 2, Height - 2);
    }

    private void DrawCornersTypeTwo()
    {
        // Левый верхний угол
        DrawPixel(Color.Fuchsia, 0, 0, 4, 4);
        DrawPixel(Edge, 2, 1);
        DrawPixel(Edge, 3, 1);

        // Правый верхний угол
        DrawPixel(Color.Fuchsia, Width - 1, 0, 4, 4);
        DrawPixel(Edge, Width - 3, 1);
        DrawPixel(Edge, Width - 4, 1);

        // Левый нижний угол
        DrawPixel(Color.Fuchsia, 0, Height - 1, 4, 4);
        DrawPixel(Edge, 2, Height - 2);
        DrawPixel(Edge, 3, Height - 2);

        // Правый нижний угол
        DrawPixel(Color.Fuchsia, Width - 1, Height - 1, 4, 4);
        DrawPixel(Edge, Width - 3, Height - 2);
        DrawPixel(Edge, Width - 4, Height - 2);
    }

    private void DrawPixel(Color color, int x, int y, int width = 1, int height = 1)
    {
        using var brush = new SolidBrush(color);
        G.FillRectangle(brush, x, y, width, height);
    }
}
public class CustomButton : ThemeControl154
{
    private Color G1, G2, Glow, Edge, HoverColor;
    private Color _foreColor; // Переменная для цвета текста
    private int a = 0;

    protected override void ColorHook()
    {
        G1 = GetColor("Gradient 1");
        G2 = GetColor("Gradient 2");
        Glow = GetColor("Glow");
        Edge = GetColor("Edge");
        HoverColor = GetColor("HoverColor");
    }

    protected override void OnAnimation()
    {
        base.OnAnimation();
        switch (State)
        {
            case MouseState.Over:
                if (a < 40)
                {
                    a += 8;
                    Invalidate();
                    Application.DoEvents();
                }
                break;

            case MouseState.None:
                if (a > 0)
                {
                    a -= 10;
                    if (a < 0) a = 0;
                    Invalidate();
                    Application.DoEvents();
                }
                break;
        }
    }

    protected override void PaintHook()
    {
        G.Clear(G1);
        using (var LGB = new LinearGradientBrush(new Rectangle(1, 1, Width - 2, Height - 2), G1, G2, 90.0F))
        {
            G.FillRectangle(LGB, new Rectangle(1, 1, Width - 2, Height - 2));
        }
        G.FillRectangle(new SolidBrush(Glow), new Rectangle(1, 1, Width - 2, (Height / 2) - 3));

        if (State == MouseState.Over || State == MouseState.None)
        {
            using (var SB = new SolidBrush(Color.FromArgb(a * 2, Color.FromArgb(30, 30, 30))))
            {
                G.FillRectangle(SB, new Rectangle(1, 1, Width - 2, Height - 2));
            }
        }
        else if (State == MouseState.Down)
        {
            using (var SB = new SolidBrush(Color.FromArgb(2, Color.Black)))
            {
                G.FillRectangle(SB, new Rectangle(1, 1, Width - 2, Height - 2));
            }
        }

        G.DrawRectangle(new Pen(Edge), new Rectangle(1, 1, Width - 2, Height - 2));
        var sf = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };
        G.DrawString(Text, Font, new SolidBrush(_foreColor), new RectangleF(2, 2, Width - 5, Height - 4), sf);
    }

    [Browsable(true)]
    [Category("Appearance")]
    [Description("Gets or sets the foreground color of the text.")]
    public override Color ForeColor
    {
        get => _foreColor;
        set
        {
            _foreColor = value;
            Invalidate(); // Перерисовка компонента для применения нового цвета
        }
    }

    public CustomButton()
    {
        IsAnimated = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        SetColor("Gradient 1", 60, 60, 60);
        SetColor("Gradient 2", 65, 65, 65);
        SetColor("Glow", 70, 70, 70);
        SetColor("Edge", 60, 60, 60);
        SetColor("HoverColor", 30, 30, 30);

        // Установить начальное значение для ForeColor
        _foreColor = Color.White; // Установите цвет текста по умолчанию

        Size = new Size(145, 25);
    }
}

[DefaultEvent("CheckedChanged")]
public class CheckBox : ThemeControl154
{
    private int X;
    private Color TextColor, G1, G2, Glow, Edge;
    private Color BG;
    private bool _Checked;

    public CheckBox()
    {
        LockHeight = 17;
        SetColor("Text", Color.Black);
        SetColor("Gradient 1", Color.FromArgb(230, 230, 230));
        SetColor("Gradient 2", Color.FromArgb(210, 210, 210));
        SetColor("Glow", Color.FromArgb(230, 230, 230));
        SetColor("Edges", Color.FromArgb(170, 170, 170));
        Width = 160;
    }

    protected override void ColorHook()
    {
        TextColor = GetColor("Text");
        G1 = GetColor("Gradient 1");
        G2 = GetColor("Gradient 2");
        Glow = GetColor("Glow");
        Edge = GetColor("Edges");
        BG = Color.FromArgb(221, 221, 221);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        X = e.Location.X;
        Invalidate();
    }

    protected override void PaintHook()
    {
        G.Clear(BackColor); // Устанавливаем фон в соответствии с текущим цветом фона

        // Рисуем фон чекбокса
        var rect = new Rectangle(0, 0, 9, 9);
        var lgb = new LinearGradientBrush(rect, G1, G2, 90.0F);
        G.FillRectangle(lgb, rect);

        // Отрисовка состояния
        if (_Checked)
        {
            G.FillRectangle(new SolidBrush(Glow), new Rectangle(0, 0, 9, 4));
            G.DrawString("g", new Font("Marlett", 6), Brushes.Black, new Point(-1, 1));
        }
        else
        {
            G.FillRectangle(new SolidBrush(Glow), new Rectangle(0, 0, 9, 4));
        }

        // Эффекты наведения
        if (State == MouseState.Over && X < 15)
        {
            using (var sb = new SolidBrush(Color.FromArgb(70, Color.White)))
            {
                G.FillRectangle(sb, rect);
            }
        }
        else if (State == MouseState.Down && X < 15)
        {
            using (var sb = new SolidBrush(Color.FromArgb(10, Color.Black)))
            {
                G.FillRectangle(sb, rect);
            }
        }

        // Рисуем границы
        G.DrawRectangle(new Pen(Edge), rect);

        // Рисуем текст
        DrawText(new SolidBrush(TextColor), HorizontalAlignment.Left, 19, -1);
    }

    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Checked = !Checked;
        CheckedChanged?.Invoke(this, EventArgs.Empty);
        base.OnMouseDown(e);
    }

    public event EventHandler CheckedChanged;
}

[DefaultEvent("CheckedChanged")]
public class RadioButton : ThemeControl154
{
    private int X;
    private Color TextColor, G1, G2, Glow, Edge, BG;

    public RadioButton()
    {
        LockHeight = 17;
        SetColor("Text", Color.Black);
        SetColor("Gradient 1", Color.FromArgb(230, 230, 230));
        SetColor("Gradient 2", Color.FromArgb(210, 210, 210));
        SetColor("Glow", Color.FromArgb(230, 230, 230));
        SetColor("Edges", Color.FromArgb(170, 170, 170));
        SetColor("Bullet", Color.FromArgb(40, 40, 40));
        Width = 180;
    }

    protected override void ColorHook()
    {
        TextColor = GetColor("Text");
        G1 = GetColor("Gradient 1");
        G2 = GetColor("Gradient 2");
        Glow = GetColor("Glow");
        Edge = GetColor("Edges");
        BG = Color.FromArgb(221, 221, 221);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        X = e.Location.X;
        Invalidate();
    }

    protected override void PaintHook()
    {
        G.Clear(BackColor); // Используем цвет фона из свойства темы
        G.SmoothingMode = SmoothingMode.HighQuality;

        // Отрисовка радиокнопки
        var radius = 9; // Радиус кнопки
        var ellipseRect = new Rectangle(0, 0, radius, radius);

        // Используем градиенты для радиокнопки
        using (var lgb = new LinearGradientBrush(ellipseRect, G1, G2, 90.0F))
        {
            G.FillEllipse(lgb, ellipseRect);
        }

        // Эффекты при нажатии или наведении
        if (_Checked)
        {
            G.FillEllipse(new SolidBrush(Glow), new Rectangle(0, 0, radius, 4));
        }

        // Эффекты наведения
        if (State == MouseState.Over && X < 15)
        {
            using (var sb = new SolidBrush(Color.FromArgb(70, Color.White)))
            {
                G.FillEllipse(sb, ellipseRect);
            }
        }
        else if (State == MouseState.Down && X < 15)
        {
            using (var sb = new SolidBrush(Color.FromArgb(10, Color.Black)))
            {
                G.FillEllipse(sb, ellipseRect);
            }
        }

        // Рисуем границы
        G.DrawEllipse(new Pen(Edge), ellipseRect);
        if (_Checked)
        {
            var bulletRect = new Rectangle(2, 2, 5, 5);
            G.FillEllipse(GetBrush("Bullet"), bulletRect);
        }

        // Рисуем текст
        DrawText(new SolidBrush(TextColor), HorizontalAlignment.Left, 19, -1);
    }

    private int _Field = 16;
    public int Field
    {
        get => _Field;
        set
        {
            if (value < 4) return;
            _Field = value;
            LockHeight = value;
            Invalidate();
        }
    }

    private bool _Checked;
    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            InvalidateControls();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (!_Checked) Checked = true;
        base.OnMouseDown(e);
    }

    public event EventHandler CheckedChanged;

    protected override void OnCreation()
    {
        InvalidateControls();
    }

    private void InvalidateControls()
    {
        if (!IsHandleCreated || !_Checked) return;

        foreach (Control C in Parent.Controls)
        {
            if (C is RadioButton radioButton && C != this)
            {
                radioButton.Checked = false;
            }
        }
    }
}

public class UTabControl : TabControl
{
    private Color _BG;

    public override Color BackColor
    {
        get => _BG;
        set => _BG = value;
    }

    public UTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        DoubleBuffered = true;
        BackColor = Color.FromArgb(50, 50, 50);
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        Alignment = TabAlignment.Top;
    }

    private Pen ToPen(Color color)
    {
        return new Pen(color);
    }

    private Brush ToBrush(Color color)
    {
        return new SolidBrush(color);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap B = new Bitmap(Width, Height))
        using (Graphics G = Graphics.FromImage(B))
        {
            try { SelectedTab.BackColor = Color.FromArgb(221, 221, 221); } catch { }

            G.Clear(BackColor);
            G.DrawRectangle(new Pen(Color.FromArgb(50, 50, 50)), new Rectangle(0, 10, Width - 1, Height - 2));
            G.Transform = new Matrix(1, 0, 0, 1, 4, 0);

            for (int i = 0; i < TabCount; i++)
            {
                Rectangle tabRect = GetTabRect(i);
                Rectangle x2 = new Rectangle(tabRect.X - 1, tabRect.Y, tabRect.Width - 3, tabRect.Height - 2);
                Rectangle x3 = new Rectangle(tabRect.X - 1, tabRect.Y, tabRect.Width - 3, tabRect.Height - 2);
                Rectangle x4 = new Rectangle(tabRect.X - 1, tabRect.Y, tabRect.Width - 3, tabRect.Height - 2);

                if (i == SelectedIndex)
                {
                    using (LinearGradientBrush G1 = new LinearGradientBrush(x3, Color.FromArgb(240, 240, 240), Color.FromArgb(190, 190, 190), 90.0F))
                    {
                        G.FillRectangle(G1, x3);
                    }

                    G.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), x2.Location, new Point(x2.Location.X, x2.Location.Y + x2.Height));
                    G.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), new Point(x2.Location.X + x2.Width, x2.Location.Y), new Point(x2.Location.X + x2.Width, x2.Location.Y + x2.Height));
                    G.DrawLine(new Pen(Color.FromArgb(80, 80, 80)), new Point(x2.Location.X, x2.Location.Y), new Point(x2.Location.X + x2.Width, x2.Location.Y));
                    G.DrawString(TabPages[i].Text, Font, new SolidBrush(Color.Black), x4, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }
                else
                {
                    Rectangle x2Inactive = new Rectangle(tabRect.X - 2, tabRect.Y + 3, tabRect.Width - 7, tabRect.Height - 5);
                    using (LinearGradientBrush G1 = new LinearGradientBrush(x2Inactive, Color.FromArgb(50, 50, 50), Color.FromArgb(50, 50, 50), -90.0F))
                    {
                        G.FillRectangle(G1, x2Inactive);
                    }

                    G.DrawRectangle(new Pen(Color.FromArgb(50, 50, 50)), x2Inactive);
                    G.DrawString(TabPages[i].Text, Font, new SolidBrush(Color.White), x2Inactive, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                }
            }

            e.Graphics.DrawImage(B, 0, 0);
        }
    }
}

[DefaultEvent("TextChanged")]
public class UTextBox : ThemeControl154
{
    private HorizontalAlignment _TextAlign = HorizontalAlignment.Left;
    private int _MaxLength = 32767;
    private bool _ReadOnly;
    private bool _UseSystemPasswordChar;
    private bool _Multiline;
    private TextBox Base;

    public HorizontalAlignment TextAlign
    {
        get => _TextAlign;
        set
        {
            _TextAlign = value;
            if (Base != null)
            {
                Base.TextAlign = value;
            }
        }
    }

    public int MaxLength
    {
        get => _MaxLength;
        set
        {
            _MaxLength = value;
            if (Base != null)
            {
                Base.MaxLength = value;
            }
        }
    }

    public bool ReadOnly
    {
        get => _ReadOnly;
        set
        {
            _ReadOnly = value;
            if (Base != null)
            {
                Base.ReadOnly = value;
            }
        }
    }

    public bool UseSystemPasswordChar
    {
        get => _UseSystemPasswordChar;
        set
        {
            _UseSystemPasswordChar = value;
            if (Base != null)
            {
                Base.UseSystemPasswordChar = value;
            }
        }
    }

    public bool Multiline
    {
        get => _Multiline;
        set
        {
            _Multiline = value;
            if (Base != null)
            {
                Base.Multiline = value;
                if (value)
                {
                    LockHeight = 0;
                    Base.Height = Height - 11;
                }
                else
                {
                    LockHeight = Base.Height + 11;
                }
            }
        }
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            if (Base != null)
            {
                Base.Text = value;
            }
        }
    }

    public override Font Font
    {
        get => base.Font;
        set
        {
            base.Font = value;
            if (Base != null)
            {
                Base.Font = value;
                Base.Location = new Point(3, 5);
                Base.Width = Width - 6;

                if (!_Multiline)
                {
                    LockHeight = Base.Height + 11;
                }
            }
        }
    }

    protected override void OnCreation()
    {
        if (!Controls.Contains(Base))
        {
            Controls.Add(Base);
        }
    }

    public UTextBox()
    {
        Base = new TextBox
        {
            Font = Font,
            Text = Text,
            MaxLength = _MaxLength,
            Multiline = _Multiline,
            ReadOnly = _ReadOnly,
            UseSystemPasswordChar = _UseSystemPasswordChar,
            BorderStyle = BorderStyle.None,
            Location = new Point(4, 4),
            Width = Width - 10
        };

        if (_Multiline)
        {
            Base.Height = Height - 11;
        }
        else
        {
            LockHeight = Base.Height + 11;
        }

        Base.TextChanged += OnBaseTextChanged;
        Base.KeyDown += OnBaseKeyDown;

        SetColor("Text", Color.Black);
        SetColor("Backcolor", 230, 230, 230);
        SetColor("Border", 50, 50, 50);
    }

    private Color BG;
    private Pen P1;

    protected override void ColorHook()
    {
        BG = GetColor("Backcolor");
        P1 = GetPen("Border");

        Base.ForeColor = GetColor("Text");
        Base.BackColor = GetColor("Backcolor");
    }

    protected override void PaintHook()
    {
        G.Clear(BG);
        DrawBorders(P1);
    }

    private void OnBaseTextChanged(object sender, EventArgs e)
    {
        Text = Base.Text;
    }

    private void OnBaseKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A)
        {
            Base.SelectAll();
            e.SuppressKeyPress = true;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        Base.Location = new Point(4, 5);
        Base.Width = Width - 8;

        if (_Multiline)
        {
            Base.Height = Height - 5;
        }

        base.OnResize(e);
    }
}


#endregion