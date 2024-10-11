using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public abstract class Theme : ContainerControl
{
    protected Graphics G;
    private Color _TransparencyKey;
    protected Image _Image;
    protected Rectangle Header;
    protected int MoveHeight = 24;

    public Theme()
    {
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Dock = DockStyle.Fill;
        if (Parent is Form form)
        {
            if (_TransparencyKey != Color.Empty) form.TransparencyKey = _TransparencyKey;
            form.FormBorderStyle = FormBorderStyle.None;
        }
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

    protected abstract void PaintHook();

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        G = e.Graphics;
        PaintHook();
    }

    public Color TransparencyKey
    {
        get => _TransparencyKey;
        set
        {
            _TransparencyKey = value;
            Invalidate();
        }
    }

    public Image Image
    {
        get => _Image;
        set
        {
            _Image = value;
            Invalidate();
        }
    }

    protected void DrawCorners(Color c, Rectangle rect)
    {
        using (Brush brush = new SolidBrush(c))
        {
            G.FillRectangle(brush, rect.X, rect.Y, 1, 1);
            G.FillRectangle(brush, rect.X + (rect.Width - 1), rect.Y, 1, 1);
            G.FillRectangle(brush, rect.X, rect.Y + (rect.Height - 1), 1, 1);
            G.FillRectangle(brush, rect.X + (rect.Width - 1), rect.Y + (rect.Height - 1), 1, 1);
        }
    }

    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y = 0)
    {
        if (string.IsNullOrEmpty(Text)) return;
        Size size = G.MeasureString(Text, Font).ToSize();
        using (Brush brush = new SolidBrush(c))
        {
            int drawX = a switch
            {
                HorizontalAlignment.Left => x,
                HorizontalAlignment.Right => Width - size.Width - x,
                _ => Width / 2 - size.Width / 2 + x
            };
            int drawY = MoveHeight / 2 - size.Height / 2 + y;
            G.DrawString(Text, Font, brush, drawX, drawY);
        }
    }

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        using (LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle))
        {
            G.FillRectangle(brush, x, y, width, height);
        }
    }
}

public enum State
{
    MouseNone = 0,
    MouseOver = 1,
    MouseDown = 2
}
public abstract class ThemeControl : Control
{
    protected Graphics G;
    private Bitmap B;
    protected State mouseState; // Сделаем mouseState защищённым

    public ThemeControl()
    {
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
        B = new Bitmap(1, 1);
        G = Graphics.FromImage(B);
    }

    protected enum State : byte // Сделаем перечисление защищённым
    {
        MouseNone = 0,
        MouseOver = 1,
        MouseDown = 2
    }

    protected virtual void ChangeMouseState(State newState)
    {
        mouseState = newState;
        Invalidate();
    }

    protected abstract void PaintHook();

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width == 0 || Height == 0) return;
        PaintHook();
        e.Graphics.DrawImage(B, 0, 0);
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        if (Width > 0 && Height > 0)
        {
            B = new Bitmap(Width, Height);
            G = Graphics.FromImage(B);
            Invalidate();
        }
        base.OnSizeChanged(e);
    }

    public System.Drawing.Image Image
    {
        get => _Image;
        set
        {
            _Image = value;
            Invalidate();
        }
    }

    private System.Drawing.Image _Image;
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

    protected void DrawGradient(Color c1, Color c2, int x, int y, int width, int height, float angle)
    {
        using LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(x, y, width, height), c1, c2, angle);
        G.FillRectangle(brush, x, y, width, height);
    }

    protected void DrawBorders(Pen p1, Pen p2, Rectangle rect)
    {
        G.DrawRectangle(p1, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
        G.DrawRectangle(p2, rect.X + 1, rect.Y + 1, rect.Width - 3, rect.Height - 3);
    }

    protected void DrawText(HorizontalAlignment a, Color c, int x, int y = 0)
    {
        if (string.IsNullOrEmpty(Text)) return;
        Size size = G.MeasureString(Text, Font).ToSize();
        using (Brush brush = new SolidBrush(c))
        {
            int drawX = a switch
            {
                HorizontalAlignment.Left => x,
                HorizontalAlignment.Right => Width - size.Width - x,
                _ => Width / 2 - size.Width / 2 + x
            };
            int drawY = Height / 2 - size.Height / 2 + y;
            G.DrawString(Text, Font, brush, drawX, drawY);
        }
    }
}

public class ZeusTheme : Theme
{
    private bool _isDragging = false;
    private Point _startPoint = new Point(0, 0);

    public ZeusTheme()
    {
        Font = new Font("Candara", 8, FontStyle.Bold);
        MoveHeight = 15; // Высота для перемещения
    }

    protected override void PaintHook()
    {
        Color c1 = Color.FromArgb(38, 38, 38);
        Color c2 = Color.AliceBlue;

        using Pen p1 = new Pen(Color.Black);
        using Pen p2 = new Pen(c2);

        G.Clear(c1);

        // Линии
        G.DrawLine(p2, 50, 0, 0, 50);                           // Левый верхний
        G.DrawLine(p2, Width - 50, 0, Width, 50);             // Правый верхний
        G.DrawLine(p2, 50, 20, Width - 50, 20);               // Горизонтальная линия
        G.DrawLine(p2, 70, 0, 50, 20);                         // Левый диагональный
        G.DrawLine(p2, Width - 70, 0, Width - 50, 20);       // Правый диагональный
        G.DrawLine(p2, 0, 75, 35, 40);                         // Левый наклонный
        G.DrawLine(p2, Width - 35, 40, Width, 75);           // Правый наклонный
        G.DrawLine(p2, 35, 40, Width - 35, 40);               // Горизонтальная середина
        G.DrawRectangle(p2, 15, 75, Width - 30, Height - 90); // Основной прямоугольник

        // Двигаемый текст
        using Brush textBrush = new SolidBrush(c2);
        G.DrawString("<<>>", Font, textBrush, Width - 32, 0);
        G.DrawString("<<>>", Font, textBrush, 8, 0);

        DrawBorders(p1, p2, ClientRectangle);
        DrawText(HorizontalAlignment.Center, c2, 0);
    }

    private void DrawText(HorizontalAlignment alignment, Color textColor, int padding)
    {
        SizeF textSize = G.MeasureString(Text, Font);
        float textX = (Width - textSize.Width) / 1.9f; // Центрирование по горизонтали
        float textY = padding + 4; // Расположение по вертикали
        using Brush brush = new SolidBrush(textColor);
        G.DrawString(Text, Font, brush, textX, textY);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && e.Y < MoveHeight)
        {
            _isDragging = true;
            _startPoint = new Point(e.X, e.Y);
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (_isDragging)
        {
            Point newLocation = new Point(Cursor.Position.X - _startPoint.X, Cursor.Position.Y - _startPoint.Y);
            this.FindForm().Location = newLocation;
        }
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            _isDragging = false;
        }
        base.OnMouseUp(e);
    }
}


public class ZeusButton : ThemeControl
{
    protected override void PaintHook()
    {
        Color c1 = Color.FromArgb(38, 38, 38);
        Color c2 = Color.AliceBlue;
        Color c3 = Color.FromArgb(150, 255, 255);
        using (Pen p1 = new Pen(Color.Black), p2 = new Pen(c2))
        {
            switch (mouseState)
            {
                case State.MouseNone:
                    G.Clear(c1);
                    DrawGradient(c2, c3, 0, 0, Width, Height, 90);
                    DrawText(HorizontalAlignment.Center, c1, 0);
                    DrawBorders(p1, p2, ClientRectangle);
                    break;
                case State.MouseOver:
                    G.Clear(c1);
                    DrawGradient(c2, c3, 0, 0, Width, Height, 90);
                    DrawText(HorizontalAlignment.Center, c1, 0);
                    DrawBorders(p2, p1, ClientRectangle);
                    break;
                case State.MouseDown:
                    G.Clear(c1);
                    DrawGradient(c2, c3, 0, 0, Width - 1, Height - 1, 90);
                    G.DrawRectangle(p1, 2, 2, Width - 5, Height - 5);
                    DrawText(HorizontalAlignment.Center, c1, 0);
                    DrawBorders(p1, p2, ClientRectangle);
                    break;
            }
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        ChangeMouseState(State.MouseOver);
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        ChangeMouseState(State.MouseNone);
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left) ChangeMouseState(State.MouseDown);
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        ChangeMouseState(State.MouseOver);
        base.OnMouseUp(e);
    }
}

public partial class ZeusCheckBox : ThemeControl
{
    private bool _Checked;
    private State _MouseState;

    public bool Checked
    {
        get => _Checked;
        set
        {
            _Checked = value;
            Invalidate();
        }
    }

    public ZeusCheckBox()
    {
        Checked = false;
        Size = new Size(115, 23);
        MinimumSize = new Size(0, 23);
        MaximumSize = new Size(900, 23);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        _MouseState = State.MouseOver;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        _MouseState = State.MouseNone;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        _MouseState = State.MouseDown;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        _MouseState = State.MouseOver;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
    }

    protected override void PaintHook()
    {
        Color backgroundColor = Color.FromArgb(38, 38, 38);
        Color checkedColor = Color.AliceBlue;
        Color checkMarkColor = Color.FromArgb(150, 255, 255);
        Pen borderPen = Pens.Black;
        Pen highlightPen = Pens.AliceBlue;

        G.Clear(backgroundColor);

        // Отображение основных состояний мыши
        switch (_MouseState)
        {
            case State.MouseNone:
                DrawText(HorizontalAlignment.Center, checkedColor, 0, 1);
                break;
            case State.MouseOver:
                DrawText(HorizontalAlignment.Center, checkedColor, 0, 1);
                break; // Убрана обводка при наведении
            case State.MouseDown:
                DrawText(HorizontalAlignment.Center, checkedColor, 0, 1);
                break;
        }

        // Рисование квадрата CheckBox
        int boxSize = 10;
        int offset = 6;

        switch (Checked)
        {
            case true:
                DrawGradient(checkedColor, checkMarkColor, offset, offset, boxSize, boxSize, 90);
                G.DrawRectangle(highlightPen, offset, offset, boxSize, boxSize);
                // Рисование галочки
                using (Pen checkPen = new Pen(Color.Green, 2))
                {
                    G.DrawLine(checkPen, offset + 2, offset + 5, offset + 4, offset + 7);
                    G.DrawLine(checkPen, offset + 4, offset + 7, offset + 8, offset + 3);
                }
                break;

            case false:
                DrawGradient(backgroundColor, backgroundColor, offset, offset, 5, 5, 90);
                G.DrawRectangle(highlightPen, offset, offset, boxSize, boxSize);
                break;
        }
    }
}

public class ZeusProgressBar : ThemeControl
{
    private int _Maximum = 100;
    private int _Value;

    public int Maximum
    {
        get => _Maximum;
        set
        {
            if (value < 1) value = 1;
            if (value < _Value) _Value = value;
            _Maximum = value;
            Invalidate();
        }
    }

    public int Value
    {
        get => _Value;
        set
        {
            if (value > _Maximum) value = _Maximum;
            _Value = value;
            Invalidate();
        }
    }

    protected override void PaintHook()
    {
        Color c1 = Color.FromArgb(38, 38, 38);
        Color c2 = Color.AliceBlue;
        Color c3 = Color.FromArgb(150, 255, 255);
        using (Pen p1 = new Pen(Color.Black), p2 = new Pen(c2))
        {
            G.Clear(c1);

            if (_Value > 2)
            {
                DrawGradient(c2, c3, 3, 3, (int)((_Value / (float)_Maximum) * Width) - 6, Height - 6, 90);
            }

            G.DrawRectangle(p2, 0, 0, Width - 1, Height - 1);
        }
    }
}
