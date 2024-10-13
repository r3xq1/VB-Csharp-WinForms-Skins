using System;
using System.Collections.Generic;
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

public class Bloom
{
    private string _Name;
    public string Name
    {
        get => _Name;
        set => _Name = value;
    }

    private Color _Value;
    public Color Value
    {
        get => _Value;
        set => _Value = value;
    }

    public Bloom() { }

    public Bloom(string name, Color value)
    {
        _Name = name;
        _Value = value;
    }
}
public abstract class ThemeControl151 : Control
{
    protected Graphics G;
    protected Bitmap B;

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

    #region Property Overrides

    private Color BackColorWait;
    public override Color BackColor
    {
        get => base.BackColor;
        set
        {
            if (IsHandleCreated) base.BackColor = value;
            else BackColorWait = value;
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
            _ImageSize = value == null ? Size.Empty : value.Size;
            _Image = value;
            Invalidate();
        }
    }

    private Size _ImageSize;
    protected Size ImageSize => _ImageSize;

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

    private bool _Transparent;
    public bool Transparent
    {
        get => _Transparent;
        set
        {
            if (!value && BackColor.A != 255)
                throw new Exception("Unable to change value to false while a transparent BackColor is in use.");

            SetStyle(ControlStyles.Opaque, !value);
            SetStyle(ControlStyles.SupportsTransparentBackColor, value);

            if (value) InvalidateBitmap(); else B = null;

            _Transparent = value;
            Invalidate();
        }
    }

    private Dictionary<string, Color> Items = new Dictionary<string, Color>();
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Bloom[] Colors
    {
        get
        {
            var T = new List<Bloom>();
            foreach (var item in Items)
            {
                T.Add(new Bloom(item.Key, item.Value));
            }
            return T.ToArray();
        }
        set
        {
            foreach (var B in value)
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
            var Items = Colors;

            try
            {
                Data = Convert.FromBase64String(value);
                for (var I = 0; I < Items.Length; I++)
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

    #region Property Helpers

    private void InvalidateBitmap()
    {
        if (Width == 0 || Height == 0) return;
        B = new Bitmap(Width, Height);
        G = Graphics.FromImage(B);
    }

    protected Color GetColor(string name)
    {
        return Items[name];
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
        using (var M = new System.IO.MemoryStream(Items.Count * 4))
        {
            foreach (var B in Colors)
            {
                M.Write(BitConverter.GetBytes(B.Value.ToArgb()), 0, 4);
            }
            _Customization = Convert.ToBase64String(M.ToArray());
        }
    }

    #endregion

    #region User Hooks

    protected abstract void ColorHook();
    protected abstract void PaintHook();

    #endregion

    #region Center Overloads

    protected Point CenterReturn;

    protected Point Center(Rectangle r1, Size s1)
    {
        CenterReturn = new Point((r1.Width / 2 - s1.Width / 2) + r1.X, (r1.Height / 2 - s1.Height / 2) + r1.Y);
        return CenterReturn;
    }

    protected Point Center(Rectangle r1, Rectangle r2)
    {
        return Center(r1, r2.Size);
    }

    protected Point Center(int w1, int h1, int w2, int h2)
    {
        CenterReturn = new Point(w1 / 2 - w2 / 2, h1 / 2 - h2 / 2);
        return CenterReturn;
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

    private Bitmap MeasureBitmap;
    private Graphics MeasureGraphics;

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

    private Point DrawTextPoint;
    private Size DrawTextSize;

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
        if (text.Length == 0) return;

        DrawTextSize = Measure(text);
        DrawTextPoint = Center(DrawTextSize);

        switch (a)
        {
            case HorizontalAlignment.Left:
                DrawText(b1, text, x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                DrawText(b1, text, DrawTextPoint.X + x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                DrawText(b1, text, Width - DrawTextSize.Width - x, DrawTextPoint.Y + y);
                break;
        }
    }

    protected void DrawText(Brush b1, string text, Point p1)
    {
        DrawText(b1, text, p1.X, p1.Y);
    }

    protected void DrawText(Brush b1, string text, int x, int y)
    {
        if (text.Length == 0) return;
        G.DrawString(text, Font, b1, x, y);
    }

    #endregion

    #region DrawImage Overloads

    private Point DrawImagePoint;

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

        DrawImagePoint = Center(image.Size);

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

    private LinearGradientBrush DrawGradientBrush;
    private Rectangle DrawGradientRectangle;

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
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(blend, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(c1, c2, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle);
        DrawGradientBrush.InterpolationColors = blend;
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, c1, c2, angle);
        G.FillRectangle(DrawGradientBrush, r);
    }

    #endregion
}
public abstract class ThemeContainer151 : ContainerControl
{
    protected Graphics G;

    public ThemeContainer151()
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

    private Rectangle Header;

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        if (_Movable && !_ControlMode)
        {
            Header = new Rectangle(7, 7, Width - 14, _MoveHeight - 7);
        }
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        G = e.Graphics;
        PaintHook();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        InitializeMessages();
        InvalidateCustomization();
        ColorHook();

        _IsParentForm = Parent is Form;
        if (!_ControlMode) Dock = DockStyle.Fill;

        if (_LockWidth != 0) Width = _LockWidth;
        if (_LockHeight != 0) Height = _LockHeight;
        if (BackColorWait != Color.Empty) BackColor = BackColorWait;

        if (_IsParentForm && !_ControlMode)
        {
            ParentForm.FormBorderStyle = _BorderStyle;
            ParentForm.TransparencyKey = _TransparencyKey;
        }

        OnCreation();
        base.OnHandleCreated(e);
    }

    protected virtual void OnCreation() { }

    #region Sizing and Movement

    protected MouseState State;

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
    private Message[] Messages = new Message[9];
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


    private void InitializeMessages()
    {
        Messages[0] = Message.Create(Parent.Handle, 161, new IntPtr(2), IntPtr.Zero);
        for (int i = 1; i <= 8; i++)
        {
            Messages[i] = Message.Create(Parent.Handle, 161, new IntPtr(i + 9), IntPtr.Zero);
        }
    }

    #endregion

    #region Property Overrides

    private Color BackColorWait;

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

    #region Properties

    private bool _Movable = true;
    public bool Movable
    {
        get => _Movable;
        set => _Movable = value;
    }

    private bool _Sizable = true;
    public bool Sizable
    {
        get => _Sizable;
        set => _Sizable = value;
    }

    private int _MoveHeight = 24;
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

    private bool _ControlMode;
    protected bool ControlMode
    {
        get => _ControlMode;
        set => _ControlMode = value;
    }

    private Color _TransparencyKey;
    public Color TransparencyKey
    {
        get => _IsParentForm && !_ControlMode ? ParentForm.TransparencyKey : _TransparencyKey;
        set
        {
            if (_IsParentForm && !_ControlMode) ParentForm.TransparencyKey = value;
            _TransparencyKey = value;
        }
    }

    private FormBorderStyle _BorderStyle;
    public FormBorderStyle BorderStyle
    {
        get => _IsParentForm && !_ControlMode ? ParentForm.FormBorderStyle : _BorderStyle;
        set
        {
            if (_IsParentForm && !_ControlMode) ParentForm.FormBorderStyle = value;
            _BorderStyle = value;
        }
    }

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

    private Size _ImageSize;

    protected Size ImageSize => _ImageSize;

    private bool _IsParentForm;

    protected bool IsParentForm => _IsParentForm;

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

    private Dictionary<string, Color> Items = new Dictionary<string, Color>();

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
    public Bloom[] Colors
    {
        get
        {
            var T = new List<Bloom>();
            foreach (var item in Items)
            {
                T.Add(new Bloom(item.Key, item.Value));
            }
            return T.ToArray();
        }
        set
        {
            foreach (var B in value)
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
            var Items = Colors;

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

    #region Property Helpers

    protected Color GetColor(string name)
    {
        return Items[name];
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
        using (var M = new System.IO.MemoryStream(Items.Count * 4))
        {
            foreach (var B in Colors)
            {
                M.Write(BitConverter.GetBytes(B.Value.ToArgb()), 0, 4);
            }
            _Customization = Convert.ToBase64String(M.ToArray());
        }
    }

    #endregion

    #region User Hooks

    protected abstract void ColorHook();
    protected abstract void PaintHook();

    #endregion

    #region Center Overloads

    private Point CenterReturn;

    protected Point Center(Rectangle r1, Size s1)
    {
        CenterReturn = new Point((r1.Width / 2 - s1.Width / 2) + r1.X, (r1.Height / 2 - s1.Height / 2) + r1.Y);
        return CenterReturn;
    }

    protected Point Center(Rectangle r1, Rectangle r2)
    {
        return Center(r1, r2.Size);
    }

    protected Point Center(int w1, int h1, int w2, int h2)
    {
        CenterReturn = new Point(w1 / 2 - w2 / 2, h1 / 2 - h2 / 2);
        return CenterReturn;
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

    private Bitmap MeasureBitmap;
    private Graphics MeasureGraphics;

    protected Size Measure(string text)
    {
        return MeasureGraphics.MeasureString(text, Font, Width).ToSize();
    }

    protected Size Measure()
    {
        return MeasureGraphics.MeasureString(Text, Font).ToSize();
    }

    #endregion

    #region DrawCorners Overloads

    private SolidBrush DrawCornersBrush;

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

        DrawCornersBrush = new SolidBrush(c1);
        G.FillRectangle(DrawCornersBrush, x, y, 1, 1);
        G.FillRectangle(DrawCornersBrush, x + (width - 1), y, 1, 1);
        G.FillRectangle(DrawCornersBrush, x, y + (height - 1), 1, 1);
        G.FillRectangle(DrawCornersBrush, x + (width - 1), y + (height - 1), 1, 1);
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

    private Point DrawTextPoint;
    private Size DrawTextSize;

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
        if (text.Length == 0) return;

        DrawTextSize = Measure(text);
        DrawTextPoint = new Point(Width / 2 - DrawTextSize.Width / 2, MoveHeight / 2 - DrawTextSize.Height / 2);

        switch (a)
        {
            case HorizontalAlignment.Left:
                DrawText(b1, text, x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Center:
                DrawText(b1, text, DrawTextPoint.X + x, DrawTextPoint.Y + y);
                break;
            case HorizontalAlignment.Right:
                DrawText(b1, text, Width - DrawTextSize.Width - x, DrawTextPoint.Y + y);
                break;
        }
    }

    protected void DrawText(Brush b1, string text, Point p1)
    {
        DrawText(b1, text, p1.X, p1.Y);
    }

    protected void DrawText(Brush b1, string text, int x, int y)
    {
        if (text.Length == 0) return;
        G.DrawString(text, Font, b1, x, y);
    }

    #endregion

    #region DrawImage Overloads

    private Point DrawImagePoint;

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
        DrawImagePoint = new Point(Width / 2 - image.Width / 2, MoveHeight / 2 - image.Height / 2);

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

    private LinearGradientBrush DrawGradientBrush;
    private Rectangle DrawGradientRectangle;

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height)
    {
        DrawGradient(blend, x, y, width, height, 90f);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height)
    {
        DrawGradient(c1, c2, x, y, width, height, 90f);
    }

    protected void DrawGradient(ColorBlend blend, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(blend, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        DrawGradientRectangle = new Rectangle(x, y, width, height);
        DrawGradient(c1, c2, DrawGradientRectangle, angle);
    }

    protected void DrawGradient(ColorBlend blend, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, Color.Empty, Color.Empty, angle);
        DrawGradientBrush.InterpolationColors = blend;
        G.FillRectangle(DrawGradientBrush, r);
    }

    protected void DrawGradient(Color c1, Color c2, Rectangle r, float angle)
    {
        DrawGradientBrush = new LinearGradientBrush(r, c1, c2, angle);
        G.FillRectangle(DrawGradientBrush, r);
    }

    #endregion
}
public class AdvantiumCheck : ThemeControl151
{
    private bool _CheckedState;

    public bool CheckedState
    {
        get => _CheckedState;
        set
        {
            _CheckedState = value;
            Invalidate();
        }
    }

    public AdvantiumCheck()
    {
        Size = new Size(100, 15);
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
        switch (CheckedState)
        {
            case true:
                DrawGradient(C1, C2, 3, 3, 9, 9, 90f);
                DrawGradient(C3, C4, 4, 4, 7, 7, 90f);
                break;
            case false:
                DrawGradient(C5, C5, 0, 0, 15, 15, 90f);
                break;
        }
        G.DrawRectangle(new Pen(new SolidBrush(P1)), 0, 0, 14, 14);
        G.DrawRectangle(new Pen(new SolidBrush(P2)), 1, 1, 12, 12);
        DrawText(new SolidBrush(B1), 17, 0);
        DrawCorners(C6, new Rectangle(0, 0, 13, 13));
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        CheckedState = !CheckedState; // Toggle check state
    }
}
public class AdvantiumTopButton : ThemeControl151
{
    public AdvantiumTopButton()
    {
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
            case MouseState.None: //None
                DrawGradient(Color.FromArgb(38, 38, 38), Color.FromArgb(30, 30, 30), ClientRectangle, 90f);
                Cursor = Cursors.Arrow;
                break;
            case MouseState.Down: //Down
                DrawGradient(Color.FromArgb(50, 50, 50), Color.FromArgb(42, 42, 42), ClientRectangle, 90f);
                Cursor = Cursors.Hand;
                break;
            case MouseState.Over: //Over
                DrawGradient(Color.FromArgb(42, 42, 42), Color.FromArgb(50, 50, 50), ClientRectangle, 90f);
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
public class AdvantiumButton : ThemeControl151
{
    public AdvantiumButton()
    {
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
            case MouseState.None: // None
                DrawGradient(Color.FromArgb(50, 50, 50), Color.FromArgb(42, 42, 42), ClientRectangle, 90f);
                Cursor = Cursors.Arrow;
                break;
            case MouseState.Down: // Down
                DrawGradient(Color.FromArgb(50, 50, 50), Color.FromArgb(42, 42, 42), ClientRectangle, 90f);
                Cursor = Cursors.Hand;
                break;
            case MouseState.Over: // Over
                DrawGradient(Color.FromArgb(42, 42, 42), Color.FromArgb(50, 50, 50), ClientRectangle, 90f);
                Cursor = Cursors.Hand;
                break;
        }

        DrawBorders(new Pen(new SolidBrush(Color.FromArgb(59, 59, 59))), 1);
        DrawBorders(new Pen(new SolidBrush(Color.FromArgb(25, 25, 25))));
        DrawCorners(Color.FromArgb(35, 35, 35));
        DrawText(new SolidBrush(T1), HorizontalAlignment.Center, 0, 0);
    }
}
public class AdvantiumTheme : ThemeContainer151
{
    private Color C1, BC, BA, T1;

    public AdvantiumTheme()
    {
        TransparencyKey = Color.Fuchsia; // Этот цвет можно изменить, если нужно
        MoveHeight = 35;

        SetColor("BackColor", Color.FromArgb(40, 40, 40));
        SetColor("BorderInner", Color.FromArgb(65, 65, 65));
        SetColor("BorderColor", Color.Black);
        SetColor("TextColor", Color.LawnGreen);
    }

    protected override void ColorHook()
    {
        C1 = GetColor("BackColor");
        BC = GetColor("BorderColor");
        BA = GetColor("BorderInner");
        T1 = GetColor("TextColor");
    }

    protected override void PaintHook()
    {
        G.Clear(C1);
        DrawGradient(Color.FromArgb(20, 20, 20), Color.FromArgb(40, 40, 40), new Rectangle(0, 0, Width, 35), 90);

        using (HatchBrush T = new HatchBrush(HatchStyle.Percent10, Color.FromArgb(25, 25, 25), Color.FromArgb(35, 35, 35)))
        {
            G.FillRectangle(T, new Rectangle(11, 25, Width - 23, Height - 36));
        }

        G.DrawRectangle(new Pen(Color.FromArgb(22, 22, 22)), new Rectangle(11, 25, Width - 23, Height - 36));
        G.DrawRectangle(new Pen(Color.FromArgb(40, 40, 40)), new Rectangle(12, 26, Width - 25, Height - 38));

        // Закрашиваем углы цветом C1 (или любым другим цветом по вашему выбору)
        DrawCorners(Color.FromArgb(40, 40, 40), new Rectangle(11, 25, Width - 22, Height - 35));

        DrawBorders(new Pen(BA), 1);
        DrawBorders(new Pen(BC));
        DrawCorners(Color.FromArgb(40, 40, 40)); 
        DrawText(new SolidBrush(T1), HorizontalAlignment.Left, 15, -3);
    }
}