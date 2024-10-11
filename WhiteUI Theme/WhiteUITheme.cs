using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

#region "Base Classes"
[ToolboxItem(false)]
public class ThemedControl : Control
{
    public DrawUtils D = new DrawUtils();
    public MouseState State = MouseState.None;
    public Palette Pal;

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    public ThemedControl()
    {
        MinimumSize = new Size(20, 20);
        ForeColor = Color.FromArgb(146, 149, 152);
        Font = new Font("Segoe UI", 10.0F);
        DoubleBuffered = true;
        Pal = new Palette
        {
            ColHighest = Color.White,
            ColHigh = Color.FromArgb(215, 215, 215),
            ColMed = Color.FromArgb(180, 180, 180),
            ColDim = Color.FromArgb(100, 100, 100),
            ColDark = Color.FromArgb(50, 50, 50)
        };
        BackColor = Pal.ColDim;
    }
}
[ToolboxItem(false)]
public class ThemedContainer : ContainerControl
{
    public DrawUtils D = new DrawUtils();
    protected bool Drag = true;
    public MouseState State = MouseState.None;
    protected bool TopCap = false;
    protected bool SizeCap = false;
    public Palette Pal;
    protected Point MouseP = new Point(0, 0);
    protected int TopGrip;

    protected override void OnPaintBackground(PaintEventArgs e) { }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        if (e.Button == MouseButtons.Left)
        {
            if (new Rectangle(0, 0, Width, TopGrip).Contains(e.Location))
            {
                TopCap = true; MouseP = e.Location;
            }
            else if (Drag && new Rectangle(Width - 15, Height - 15, 15, 15).Contains(e.Location))
            {
                SizeCap = true; MouseP = e.Location;
            }
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        TopCap = false;
        if (Drag) SizeCap = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (TopCap)
        {
            Point newLocation = new Point(MousePosition.X - MouseP.X, MousePosition.Y - MouseP.Y);
            Parent.Location = newLocation;
        }

        if (Drag && SizeCap)
        {
            MouseP = e.Location;
            Parent.Size = new Size(MousePosition.X - Parent.Location.X, MousePosition.Y - Parent.Location.Y);
            Invalidate();
        }
    }


    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    public ThemedContainer()
    {
        MinimumSize = new Size(20, 20);
        ForeColor = Color.FromArgb(146, 149, 152);
        Font = new Font("Trebuchet MS", 10.0F);
        DoubleBuffered = true;
        Pal = new Palette
        {
            ColHighest = Color.White,
            ColHigh = Color.FromArgb(215, 215, 215),
            ColMed = Color.FromArgb(180, 180, 180),
            ColDim = Color.FromArgb(100, 100, 100),
            ColDark = Color.FromArgb(50, 50, 50)
        };
        BackColor = Pal.ColDim;
    }
}
#endregion

#region "Theme"

//[ToolboxItem(true)]
//public class WhiteUIForm : ThemedContainer
//{
//    public WhiteUIForm()
//    {
//        MinimumSize = new Size(305, 150);
//        Dock = DockStyle.Fill; // Заполнение всей формы
//        TopGrip = 30; // Высота верхней панели
//        Font = new Font("Segoe UI", 10.0F, FontStyle.Bold); // Шрифт
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        var G = e.Graphics; // Получаем графический объект
//        G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; // Устанавливаем сглаживание
//        base.OnPaint(e); // Выполняем стандартные действия

//        // Создаем графический путь с закругленными углами
//        GraphicsPath MainPath = new GraphicsPath();
//        MainPath.AddArc(0, 0, 10, 10, 180, 90); // Верхний левый угол
//        MainPath.AddArc(Width - 10, 0, 10, 10, 270, 90); // Верхний правый угол
//        MainPath.AddArc(Width - 10, Height - 10, 10, 10, 0, 90); // Нижний правый угол
//        MainPath.AddArc(0, Height - 10, 10, 10, 90, 90); // Нижний левый угол
//        MainPath.CloseFigure(); // Закрываем путь

//        // Заполняем поверхность основным цветом
//        using (Brush brush = new SolidBrush(Pal.ColHighest))
//        {
//            G.FillPath(brush, MainPath);
//        }

//        // Рисуем границу пути
//        using (Pen pen = new Pen(Pal.ColHighest))
//        {
//            G.DrawPath(pen, MainPath);
//        }

//        // Рисуем линию для верхней панели
//        using (Pen pen = new Pen(Pal.ColHighest))
//        {
//            G.DrawLine(pen, new Point(0, TopGrip), new Point(Width - 1, TopGrip));
//        }

//        // Рисуем текст с тенью
//        D.DrawTextWithShadow(G, new Rectangle(8, 0, Width - 17, TopGrip), Text, Font, HorizontalAlignment.Left, Pal.ColDim, Color.FromArgb(200, 200, 200));
//    }
//}

[ToolboxItem(true)]
public class WhiteUIForm : ThemedContainer
{
    private new DrawUtils D = new();
    private readonly Color _backgroundColor = Color.FromArgb(250, 250, 250); // Цвет фона

    public WhiteUIForm()
    {
        MinimumSize = new Size(305, 150);
        Dock = DockStyle.Fill;
        TopGrip = 30;
        Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        using (var brush = new SolidBrush(_backgroundColor))
        {
            e.Graphics.FillRectangle(brush, e.ClipRectangle);
        }

        // Если нужен градиент, можете оставить этот код
        // using (var gradientBrush = new LinearGradientBrush(ClientRectangle, _backgroundColor, Color.FromArgb(230, 230, 230), 90))
        // {
        //     e.Graphics.FillRectangle(gradientBrush, e.ClipRectangle);
        // }

        base.OnPaintBackground(e); // Выполняем стандартные действия
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var G = e.Graphics;
        G.SmoothingMode = SmoothingMode.AntiAlias; // Устанавливаем сглаживание
        base.OnPaint(e); // Выполняем стандартные действия

        // Создаем графический путь с закругленными углами
        GraphicsPath MainPath = new GraphicsPath();
        MainPath.AddArc(0, 0, 10, 10, 180, 90); // Верхний левый угол
        MainPath.AddArc(Width - 10, 0, 10, 10, 270, 90); // Верхний правый угол
        MainPath.AddArc(Width - 10, Height - 10, 10, 10, 0, 90); // Нижний правый угол
        MainPath.AddArc(0, Height - 10, 10, 10, 90, 90); // Нижний левый угол
        MainPath.CloseFigure(); // Закрываем путь

        // Фоновое статическое изображение
        Bitmap BGStatic = (Bitmap)D.CodeToImage(D.BGWhiteStatic);
        TextureBrush BGTextureBrush = new TextureBrush(BGStatic, WrapMode.TileFlipXY);

        // Заполняем закругленный путь текстурой
        G.FillPath(BGTextureBrush, MainPath);

        // Рисуем границу пути
        using (Pen pen = new Pen(Pal.ColHigh))
        {
            G.DrawPath(pen, MainPath);
        }

        // Рисуем линию для верхней панели
        using (Pen pen = new Pen(Pal.ColHigh))
        {
            G.DrawLine(pen, new Point(0, TopGrip), new Point(Width - 1, TopGrip));
        }

        // Рисуем текст с тенью
        D.DrawTextWithShadow(G, new Rectangle(8, 0, Width - 17, TopGrip), Text, Font, HorizontalAlignment.Left, Pal.ColDim, Color.FromArgb(200, 200, 200));
    }
}

[ToolboxItem(true)]
public class WhiteUICheckbox : ThemedControl
{
    private bool _checked;
    public event EventHandler CheckedChanged;

    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                Invalidate(); // Перерисовка контрола
                CheckedChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие
            }
        }
    }

    public override string Text { get; set; } = "Checkbox";

    private Color _foreColor = Color.Black;
    public override Color ForeColor
    {
        get => _foreColor;
        set
        {
            if (_foreColor != value)
            {
                _foreColor = value;
                Invalidate(); // Перерисовка контрола
            }
        }
    }

    public override Color BackColor { get; set; } = Color.Transparent; // Установка фонового цвета по умолчанию

    private static readonly Size DefaultButtonSize = new(149, 30); // Стандартный размер кнопки

    public WhiteUICheckbox()
    {
        Font = new Font("Segoe UI", 10.0F, FontStyle.Regular);
        ForeColor = Color.FromKnownColor(KnownColor.WindowText);
        Size = DefaultButtonSize; // Установка стандартного размера
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics G = e.Graphics;
        G.Clear(BackColor);  // Устанавливаем цвет фона
        G.SmoothingMode = SmoothingMode.AntiAlias;

        // Рисуем чекбокс
        Rectangle checkBoxRect = new Rectangle(2, 2, Height - 6, Height - 6);
        using (var checkBoxBrush = new LinearGradientBrush(
            checkBoxRect,
            Color.FromArgb(200, 200, 200),
            Color.FromArgb(245, 245, 245),
            LinearGradientMode.Vertical))
        {
            G.FillRectangle(checkBoxBrush, checkBoxRect);
        }

        // Рисуем границу чекбокса без верхней линии
        using (var pen = new Pen(Color.FromArgb(200, 200, 200))) // Цвет границы под цвет формы
        {
            G.DrawLine(pen, checkBoxRect.Left, checkBoxRect.Top, checkBoxRect.Right, checkBoxRect.Top);
            G.DrawLine(pen, checkBoxRect.Left, checkBoxRect.Top, checkBoxRect.Left, checkBoxRect.Bottom);
            G.DrawLine(pen, checkBoxRect.Right, checkBoxRect.Top, checkBoxRect.Right, checkBoxRect.Bottom);
            G.DrawLine(pen, checkBoxRect.Left, checkBoxRect.Bottom, checkBoxRect.Right, checkBoxRect.Bottom);
        }

        // Рисуем галочку
        if (Checked)
        {
            using var pen = new Pen(Color.Black, 2);
            G.DrawLine(pen, checkBoxRect.Left + 5, checkBoxRect.Top + checkBoxRect.Height / 2,
                        checkBoxRect.Left + checkBoxRect.Width / 2, checkBoxRect.Bottom - 5);
            G.DrawLine(pen, checkBoxRect.Left + checkBoxRect.Width / 2, checkBoxRect.Bottom - 5,
                        checkBoxRect.Right - 5, checkBoxRect.Top + 5);
        }

        // Рисуем текст с использованием ForeColor
        using var textBrush = new SolidBrush(ForeColor);
        G.DrawString(Text, Font, textBrush, new Point(30, 5));
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Left)
        {
            Checked = !Checked; // Переключаем состояние
            Invalidate(); // Перерисовка контрола
        }
    }
}

[ToolboxItem(true)]
public class WhiteUIButton : ThemedControl
{
    private static readonly Size DefaultButtonSize = new(145, 30); // Стандартный размер кнопки
    private bool _isHovered; // Состояние наведения
    private bool _isPressed; // Состояние нажатия

    public WhiteUIButton()
    {
        Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
        Size = DefaultButtonSize; // Установка стандартного размера

        // Обработчики событий мыши
        MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
        MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
        MouseDown += (s, e) => { _isPressed = true; Invalidate(); };
        MouseUp += (s, e) => { _isPressed = false; Invalidate(); };
    }

    private Color AdjustBrightness(Color color, float factor)
    {
        return Color.FromArgb(
            Math.Min(255, (int)(color.R * factor)),
            Math.Min(255, (int)(color.G * factor)),
            Math.Min(255, (int)(color.B * factor))
        );
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var G = e.Graphics;
        base.OnPaint(e);
        G.Clear(Parent.BackColor);
        G.SmoothingMode = SmoothingMode.HighQuality;

        // Определение цвета в зависимости от состояния
        Color backgroundColor = Pal.ColHigh;
        if (_isPressed)
        {
            backgroundColor = AdjustBrightness(Pal.ColHigh, 0.9f); // Затемнение для нажатия
        }
        else if (_isHovered)
        {
            backgroundColor = AdjustBrightness(Pal.ColHigh, 1.1f); // Осветление для наведения
        }

        Rectangle TextRect = new Rectangle(0, 0, Width + 1, Height - 3);
        GraphicsPath MainPath = D.RoundRect(new Rectangle(2, 2, Width - 6, Height - 6), 5);
        G.FillRectangle(new SolidBrush(Pal.ColHigh), new Rectangle(-1, -1, Width + 1, Height + 1));
        // Используем определенный цвет фона
        G.FillPath(new LinearGradientBrush(new Point(0, 2), new Point(0, Height * (3 / 2)), Color.WhiteSmoke, backgroundColor), MainPath);
        D.DrawTextWithShadow(G, TextRect, Text, Font, HorizontalAlignment.Center, Pal.ColDim, Color.FromArgb(200, 200, 200));
        G.DrawPath(new Pen(Pal.ColHigh), MainPath);
    }
}

#endregion

#region "Theme Utility Stuff"
public class Palette
{
    public Color ColHighest;
    public Color ColHigh;
    public Color ColMed;
    public Color ColDim;
    public Color ColDark;
}

public enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

public class DrawUtils
{
    public GraphicsPath RoundRect(Rectangle rect, int curve)
    {
        var path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;
        path.AddArc(new Rectangle(rect.X, rect.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        path.AddArc(new Rectangle(rect.Width - arcRectangleWidth + rect.X, rect.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        path.AddArc(new Rectangle(rect.Width - arcRectangleWidth + rect.X, rect.Height - arcRectangleWidth + rect.Y, arcRectangleWidth, arcRectangleWidth), 0, 90);
        path.AddArc(new Rectangle(rect.X, rect.Height - arcRectangleWidth + rect.Y, arcRectangleWidth, arcRectangleWidth), 90, 90);
        path.AddLine(new Point(rect.X, rect.Height - arcRectangleWidth + rect.Y), new Point(rect.X, curve + rect.Y));
        return path;
    }

    public void DrawTextWithShadow(Graphics g, Rectangle contRect, string text, Font tFont, HorizontalAlignment tAlign, Color tColor, Color bColor)
    {
        DrawText(g, new Rectangle(contRect.X + 1, contRect.Y + 2, contRect.Width + 1, contRect.Height + 2), text, tFont, tAlign, bColor);
        DrawText(g, contRect, text, tFont, tAlign, tColor);
    }

    public void DrawShadowEllipse(Graphics G, Color col, Rectangle Path)
    {
        using var gp = new GraphicsPath();
        gp.AddEllipse(Path);

        using var pgb = new PathGradientBrush(gp);
        pgb.CenterPoint = new PointF(Path.Width / 2, Path.Height / 2);
        pgb.CenterColor = col;
        pgb.SurroundColors = [Color.Transparent];
        pgb.SetBlendTriangularShape(0.1F, 1.0F);
        pgb.FocusScales = new PointF(0.0F, 0.0F);

        G.FillPath(pgb, gp);
    }

    public void DrawText(Graphics g, Rectangle contRect, string text, Font tFont, HorizontalAlignment tAlign, Color tColor)
    {
        if (string.IsNullOrEmpty(text)) return;
        Size textSize = TextRenderer.MeasureText(text, tFont);
        int centeredY = contRect.Height / 2 - textSize.Height / 2;
        switch (tAlign)
        {
            case HorizontalAlignment.Left:
                g.DrawString(text, tFont, new SolidBrush(tColor), contRect.X, centeredY);
                break;
            case HorizontalAlignment.Right:
                g.DrawString(text, tFont, new SolidBrush(tColor), contRect.X + contRect.Width - textSize.Width - 5, centeredY);
                break;
            case HorizontalAlignment.Center:
                g.DrawString(text, tFont, new SolidBrush(tColor), contRect.X + contRect.Width / 2 - textSize.Width / 2, centeredY);
                break;
        }
    }

    public Image CodeToImage(string code)
    {
        return Image.FromStream(new MemoryStream(Convert.FromBase64String(code)));
    }

    public string BGWhiteStatic = "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";
}
#endregion