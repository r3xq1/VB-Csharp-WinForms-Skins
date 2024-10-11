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

public class VTheme : ThemeContainer154
{
    private Color _textColor = Color.White; // Цвет текста
    private HorizontalAlignment _textAlign = HorizontalAlignment.Left; // Выравнивание текста

    public Color TextColor
    {
        get { return _textColor; }
        set { _textColor = value; Invalidate(); } // Перерисовать при изменении цвета
    }

    [Browsable(false)] // Убирает свойство из поля свойств
    public new Color BackColor { get; private set; }

    public HorizontalAlignment TextAlign
    {
        get { return _textAlign; }
        set { _textAlign = value; Invalidate(); } // Перерисовать при изменении расположения
    }

    public VTheme()
    {
        TransparencyKey = Color.Transparent;
        ControlMode = true;
        Dock = DockStyle.Fill;
    }

    protected override void ColorHook()
    {
        // Установка цветов
    }

    protected override void PaintHook()
    {
        if (Width <= 0 || Height <= 0)
            return;

        G.Clear(Color.FromArgb(12, 12, 12));

        using (Pen borderPen = new Pen(Color.FromArgb(32, 32, 32)))
        {
            G.DrawLine(borderPen, 11, 31, Width - 12, 31);
            G.DrawLine(borderPen, 11, 8, Width - 12, 8);
        }

        using (LinearGradientBrush brush = new LinearGradientBrush(
            new Rectangle(8, 38, Width - 16, Height - 46),
            Color.FromArgb(12, 12, 12),
            Color.FromArgb(18, 18, 18),
            LinearGradientMode.BackwardDiagonal))
        {
            G.FillRectangle(brush, 8, 38, Width - 16, Height - 46);
        }

        // Получаем текст из свойства
        string text = Text;

        // Рассчитываем размеры текста
        SizeF textSize = G.MeasureString(text, new Font("Arial", 8));
        float textX = 0; // Позиция по X

        // Выравнивание текста
        switch (TextAlign)
        {
            case HorizontalAlignment.Left:
                textX = 10; // Левое выравнивание
                break;
            case HorizontalAlignment.Center:
                textX = (Width - textSize.Width) / 2; // Центрирование
                break;
            case HorizontalAlignment.Right:
                textX = Width - textSize.Width - 10; // Правое выравнивание
                break;
        }

        float textY = 10; // Верхнее положение

        // Рисуем текст
        G.DrawString(text, new Font("Arial", 8), new SolidBrush(TextColor), textX, textY);

        DrawBorders(new Pen(Color.FromArgb(60, 60, 60)), 1);
        DrawBorders(Pens.Black);

        using (Pen pen = new Pen(Color.FromArgb(25, 25, 25)))
        {
            G.DrawLine(Pens.Black, 6, 0, 6, Height - 6);
            G.DrawLine(Pens.Black, Width - 6, 0, Width - 6, Height - 6);
            G.DrawLine(pen, 6, 0, 6, Height - 6);
            G.DrawLine(pen, Width - 8, 0, Width - 8, Height - 6);
        }

        G.DrawRectangle(Pens.Black, 11, 4, Width - 23, 22);
        G.DrawLine(Pens.Black, 6, Height - 8, Width - 8, Height - 8);
    }

    public void UpdateSize(int width, int height)
    {
        Width = width;
        Height = height;
        Invalidate();
    }
}
public class VTextBox : ThemeControl154
{
    private TextBox txt = new TextBox();

    private bool _multiline;
    public bool Multiline
    {
        get => _multiline;
        set
        {
            _multiline = value;
            ResizeControl();
        }
    }

    private bool _passMask;
    public bool UsePasswordMask
    {
        get => _passMask;
        set
        {
            _passMask = value;
            txt.UseSystemPasswordChar = value;
        }
    }

    private int _maxChars;
    public int MaxCharacters
    {
        get => _maxChars;
        set
        {
            _maxChars = value;
            txt.MaxLength = value;
        }
    }

    public VTextBox()
    {
        txt.TextAlign = HorizontalAlignment.Left;
        txt.BorderStyle = BorderStyle.None;
        txt.Location = new Point(10, 6);
        txt.Font = new Font("Verdana", 8);
        txt.TextChanged += TextChngTxtBox;
        Controls.Add(txt);

        Text = "";
        txt.Text = "";
        Size = new Size(150, 25);
    }

    protected override void ColorHook()
    {
        txt.ForeColor = Color.White;
        txt.BackColor = Color.FromArgb(15, 15, 15);
    }

    protected override void PaintHook()
    {
        G.Clear(Color.FromArgb(15, 15, 15));
        txt.Size = new Size(Width - 20, Height - 10);

        G.FillRectangle(new SolidBrush(Color.FromArgb(15, 15, 15)), new Rectangle(1, 1, Width - 2, Height - 2));
        DrawBorders(new Pen(new SolidBrush(Color.FromArgb(32, 32, 32))), 1);
        DrawBorders(new Pen(new SolidBrush(Color.Black)));
        DrawCorners(Color.FromArgb(15, 15, 15));
        DrawCorners(Color.FromArgb(15, 15, 15), new Rectangle(1, 1, Width - 2, Height - 2));
    }

    private void TextChngTxtBox(object sender, EventArgs e)
    {
        Text = txt.Text;
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        txt.Text = Text;
    }

    private void ResizeControl()
    {
        if (Multiline)
        {
            Size = new Size(Width, Height);
        }
        else
        {
            Size = new Size(Width, 25);
        }
    }
}
public class VButton : ThemeControl154
{
    protected override void ColorHook()
    {
        // Здесь можно установить цвета, если это необходимо.
    }

    protected override void PaintHook()
    {
        DrawBorders(new Pen(Color.FromArgb(16, 16, 16)), 1);
        G.FillRectangle(new SolidBrush(Color.FromArgb(5, 5, 5)), 0, 0, Width, 8);
        DrawBorders(Pens.Black, 3);
        DrawBorders(new Pen(Color.FromArgb(24, 24, 24)));

        if (State == MouseState.Over)
        {
            G.FillRectangle(new SolidBrush(Color.FromArgb(25, 25, 25)), 3, 3, Width - 6, Height - 6);
            DrawBorders(new Pen(Color.FromArgb(0, 0, 0)), 2);
        }
        else if (State == MouseState.Down)
        {
            G.FillRectangle(new LinearGradientBrush(new Rectangle(3, 3, Width - 6, Height - 6), Color.FromArgb(12, 12, 12), Color.FromArgb(30, 30, 30), LinearGradientMode.BackwardDiagonal), 3, 3, Width - 6, Height - 6);
            DrawBorders(new Pen(Color.FromArgb(0, 0, 0)), 2);
        }
        else
        {
            G.FillRectangle(new LinearGradientBrush(new Rectangle(3, 3, Width - 6, Height - 6), Color.FromArgb(9, 9, 9), Color.FromArgb(18, 18, 18), LinearGradientMode.Vertical), 3, 3, Width - 6, Height - 6);
            DrawBorders(new Pen(Color.FromArgb(32, 32, 32)), 2);
        }

        if (State == MouseState.Down)
        {
            DrawText(Brushes.White, HorizontalAlignment.Center, 2, 2);
        }
        else
        {
            DrawText(Brushes.White, HorizontalAlignment.Center, 0, 0);
        }
    }
}
public class VRadiobutton : ThemeControl154
{
    private bool _checked;

    public VRadiobutton()
    {
        BackColor = Color.Transparent;
        Transparent = true;
        Size = new Size(50, 17);
        _checked = false; // Инициализация состояния
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                Invalidate(); // Перерисовка контрола
                OnCheckedChanged(EventArgs.Empty); // Вызов события при изменении состояния
            }
        }
    }

    public event EventHandler CheckedChanged;

    protected virtual void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        // Снимаем отметку со всех других радиокнопок в родительском контейнере
        foreach (Control control in Parent.Controls)
        {
            if (control is VRadiobutton radioButton && radioButton != this)
            {
                radioButton.Checked = false;
            }
        }
        Checked = true; // Устанавливаем текущую радиокнопку как отмеченную
    }

    protected override void ColorHook() { }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        // Устанавливаем ширину в зависимости от текста
        int textSize = (int)CreateGraphics().MeasureString(Text, Font).Width;
        Width = 25 + textSize;
    }

    protected override void PaintHook()
    {
        G.Clear(BackColor);
        G.SmoothingMode = SmoothingMode.AntiAlias;

        // Рисуем радиокнопку
        DrawRadioButton(G, _checked);
        G.DrawString(Text, Font, Brushes.White, 22, 2);
    }

    private void DrawRadioButton(Graphics g, bool isChecked)
    {
        g.FillEllipse(new SolidBrush(Color.Black), 0, 0, 16, 16);
        using (var gBrush = new LinearGradientBrush(new Rectangle(1, 1, 14, 14),
            isChecked ? Color.FromArgb(45, 45, 45) : Color.FromArgb(24, 30, 36),
            isChecked ? Color.FromArgb(10, 10, 10) : Color.FromArgb(25, 25, 25), 90.0F))
        {
            g.FillEllipse(gBrush, new Rectangle(1, 1, 14, 14));
        }

        if (isChecked)
        {
            using (var gBrush = new LinearGradientBrush(new Rectangle(2, 2, 12, 12),
                Color.FromArgb(25, 25, 25), Color.FromArgb(20, 20, 20), 90.0F))
            {
                g.FillEllipse(gBrush, new Rectangle(2, 2, 12, 12));
            }
            g.FillEllipse(Brushes.Black, new Rectangle(5, 6, 5, 5));
            using (var gBrush2 = new LinearGradientBrush(new Rectangle(1, 1, 14, 14),
                Color.FromArgb(130, 130, 130), Color.FromArgb(20, 20, 20), LinearGradientMode.ForwardDiagonal))
            {
                g.FillEllipse(gBrush2, new Rectangle(3, 3, 10, 10));
            }
        }
    }
}
public class VCheckBox : ThemeControl154
{
    private Pen P1;
    private Brush B1;
    private Brush B2;
    private Brush B3;
    private bool _Checked = false;

    public bool Checked
    {
        get { return _Checked; }
        set
        {
            _Checked = value;
            Invalidate();
        }
    }

    public VCheckBox()
    {
        BackColor = Color.Transparent;
        Transparent = true;
        Size = new Size(150, 16);
        AutoSize = true;
        TabStop = false;
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        _Checked = !_Checked;
    }

    protected override void ColorHook()
    {
        P1 = new Pen(Color.FromArgb(0, 0, 0));
        B1 = new SolidBrush(Color.FromArgb(15, Color.FromArgb(26, 26, 26)));
        B2 = new SolidBrush(Color.White);
        B3 = new SolidBrush(Color.FromArgb(0, 0, 0));
    }

    protected override void PaintHook()
    {
        G.Clear(BackColor);
        G.FillRectangle(B3, 0, 0, 45, 15);
        G.DrawRectangle(P1, 0, 0, 45, 15);

        if (!_Checked)
        {
            G.DrawString("OFF", Font, Brushes.Gray, 3, 1);
            G.FillRectangle(new LinearGradientBrush(new Rectangle(29, -1, 13, 16), Color.FromArgb(35, 35, 35), Color.FromArgb(25, 25, 25), 90f), 29, -1, 13, 16);
        }
        else
        {
            DrawGradient(Color.FromArgb(10, 10, 10), Color.FromArgb(20, 20, 20), 15, 2, 28, 11, 90f);
            G.DrawString("ON", Font, Brushes.White, 18, 0);
            G.FillRectangle(new LinearGradientBrush(new Rectangle(2, -1, 13, 16), Color.FromArgb(80, 80, 80), Color.FromArgb(60, 60, 60), 90f), 2, -1, 13, 16);
        }

        G.FillRectangle(B1, 2, 2, 41, 11);
        DrawText(B2, HorizontalAlignment.Left, 50, 0);
    }
}
public class VGroupBox : ThemeContainer154
{
    public VGroupBox()
    {
        ControlMode = true;
    }

    protected override void ColorHook()
    {
    }

    protected override void PaintHook()
    {
        G.Clear(Color.Transparent);
        DrawBorders(new Pen(Color.FromArgb(36, 36, 36)), 1);
        DrawBorders(Pens.Black);
        G.DrawLine(new Pen(Color.FromArgb(48, 48, 48)), 1, 1, Width - 2, 1);
        G.FillRectangle(new LinearGradientBrush(new Rectangle(8, 8, Width - 16, Height - 16), Color.FromArgb(12, 12, 12), Color.FromArgb(18, 18, 18), LinearGradientMode.BackwardDiagonal), 8, 8, Width - 16, Height - 16);
        DrawBorders(new Pen(Color.FromArgb(36, 36, 36)), 7);
    }
}
public class VSeparator : ThemeControl154
{
    private Orientation _Orientation;

    public Orientation Orientation
    {
        get { return _Orientation; }
        set
        {
            _Orientation = value;

            if (_Orientation == Orientation.Vertical)
            {
                LockHeight = 0;
                LockWidth = 14;
            }
            else
            {
                LockHeight = 14;
                LockWidth = 0;
            }

            Invalidate();
        }
    }

    public VSeparator()
    {
        Transparent = true;
        BackColor = Color.Transparent;
        LockHeight = 14;
    }

    protected override void ColorHook()
    {
    }

    protected override void PaintHook()
    {
        G.Clear(BackColor);

        ColorBlend BL1 = new ColorBlend
        {
            Positions = new float[] { 0.0f, 0.15f, 0.85f, 1.0f },
            Colors = new Color[] { Color.Transparent, Color.Black, Color.Black, Color.Transparent }
        };

        ColorBlend BL2 = new ColorBlend
        {
            Positions = new float[] { 0.0f, 0.15f, 0.5f, 0.85f, 1.0f },
            Colors = new Color[] { Color.Transparent, Color.FromArgb(24, 24, 24), Color.FromArgb(40, 40, 40), Color.FromArgb(36, 36, 36), Color.Transparent }
        };

        if (_Orientation == Orientation.Vertical)
        {
            DrawGradient(BL1, 6, 0, 1, Height);
            DrawGradient(BL2, 7, 0, 1, Height);
        }
        else
        {
            DrawGradient(BL1, 0, 6, Width, 1, 0.0f);
            DrawGradient(BL2, 0, 7, Width, 1, 0.0f);
        }
    }
}
public class VTabControl : TabControl
{
    private int oldIndex = 0;
    private int speed = 10;
    private Color lightBlack = Color.FromArgb(18, 18, 18);
    private Color lighterBlack = Color.FromArgb(21, 21, 21);
    private Color tabTextColor = Color.White;

    public int Speed
    {
        get => speed;
        set
        {
            if (value > 20 || value < -20)
            {
                MessageBox.Show("Speed needs to be in between -20 and 20.");
            }
            else
            {
                speed = value;
            }
        }
    }

    public Color TabTextColor
    {
        get => tabTextColor;
        set
        {
            tabTextColor = value;
            Invalidate();
        }
    }

    public VTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        TabTextColor = Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Rectangle r2 = new Rectangle(2, 0, Width, 25);
        e.Graphics.Clear(lightBlack);

        LinearGradientBrush brush = new LinearGradientBrush(r2, Color.FromArgb(25, 25, 25), Color.FromArgb(42, 42, 42), LinearGradientMode.ForwardDiagonal);
        e.Graphics.FillRectangle(brush, r2);

        for (int tabIndex = 0; tabIndex < TabCount; tabIndex++)
        {
            Rectangle itemBounds = GetTabRect(tabIndex);
            e.Graphics.FillRectangle(Brushes.Transparent, itemBounds);

            using (Pen borderPen = new Pen(tabIndex == SelectedIndex ? Color.Black : Color.Black, 1))
            {
                Rectangle rPen = new Rectangle(itemBounds.X + 3, itemBounds.Y + 0, itemBounds.Width - 4, itemBounds.Height - 2);
                e.Graphics.DrawRectangle(borderPen, rPen);
            }

            using (Brush textBrush = new SolidBrush(tabIndex == SelectedIndex ? TabTextColor : Color.Gray))
            {
                StringFormat sf = new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center };
                e.Graphics.DrawString(TabPages[tabIndex].Text, Font, textBrush, itemBounds, sf);
            }
        }

        e.Graphics.DrawRectangle(new Pen(Color.Black), 2, 0, Width - 3, Height - 3);
        e.Graphics.DrawRectangle(new Pen(lighterBlack), new Rectangle(3, 24, Width - 5, Height - 28));
        e.Graphics.DrawLine(new Pen(Color.Black), 2, 23, Width - 2, 23);
    }

    protected override void OnSelecting(TabControlCancelEventArgs e)
    {
        base.OnSelecting(e);
        if (oldIndex < e.TabPageIndex)
        {
            DoAnimationScrollUp(TabPages[oldIndex], TabPages[e.TabPageIndex]);
        }
        else
        {
            DoAnimationScrollDown(TabPages[oldIndex], TabPages[e.TabPageIndex]);
        }
    }

    protected override void OnDeselecting(TabControlCancelEventArgs e)
    {
        oldIndex = e.TabPageIndex;
    }

    private void DoAnimationScrollDown(Control control1, Control control2)
    {
        AnimateScroll(control1, control2, true);
    }

    private void DoAnimationScrollUp(Control control1, Control control2)
    {
        AnimateScroll(control1, control2, false);
    }

    private void AnimateScroll(Control control1, Control control2, bool scrollDown)
    {
        using (Graphics g = control1.CreateGraphics())
        {
            Bitmap p1 = new Bitmap(control1.Width, control1.Height);
            Bitmap p2 = new Bitmap(control2.Width, control2.Height);
            control1.DrawToBitmap(p1, new Rectangle(0, 0, control1.Width, control1.Height));
            control2.DrawToBitmap(p2, new Rectangle(0, 0, control2.Width, control2.Height));

            foreach (Control c in control1.Controls)
                c.Hide();

            int slide = control1.Height - (control1.Height % speed);
            for (int a = 0; a <= slide; a += speed)
            {
                g.DrawImage(p1, new Rectangle(0, scrollDown ? a : -a, control1.Width, control1.Height));
                g.DrawImage(p2, new Rectangle(0, scrollDown ? a - control2.Height : a + control2.Height, control2.Width, control2.Height));
            }

            SelectedTab = (TabPage)control2;

            foreach (Control c in control2.Controls)
                c.Show();
            foreach (Control c in control1.Controls)
                c.Show();
        }
    }

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        e.Control.Dock = DockStyle.Fill; // Устанавливаем Dock для добавляемых контролов
    }
}


#endregion