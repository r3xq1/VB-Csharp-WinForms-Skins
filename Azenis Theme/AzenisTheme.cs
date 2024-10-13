using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

#region Theme Utility Stuff
public class Palette
{
    public Color ColHighest { get; set; }
    public Color ColHigh { get; set; }
    public Color ColMed { get; set; }
    public Color ColDim { get; set; }
    public Color ColDark { get; set; }
}

public enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

public enum GradientAlignment : byte
{
    Vertical = 0,
    Horizontal = 1
}

public class DrawUtils
{
    public void FillDualGradPath(Graphics g, Color col1, Color col2, Rectangle rect, GraphicsPath gp, GradientAlignment align)
    {
        SmoothingMode stored = g.SmoothingMode;
        ColorBlend blend = new ColorBlend();
        g.SmoothingMode = SmoothingMode.HighQuality;

        switch (align)
        {
            case GradientAlignment.Vertical:
                using (var pathGradient = new LinearGradientBrush(new Point(rect.X, rect.Y), new Point(rect.X + rect.Width - 1, rect.Y), Color.Black, Color.Black))
                {
                    blend.Positions = new float[] { 0, 0.5f, 1 };
                    blend.Colors = new Color[] { col1, col2, col1 };
                    pathGradient.InterpolationColors = blend;
                    g.FillPath(pathGradient, gp);
                }
                break;

            case GradientAlignment.Horizontal:
                using (var pathGradient = new LinearGradientBrush(new Point(rect.X, rect.Y), new Point(rect.X, rect.Y + rect.Height), Color.Black, Color.Black))
                {
                    blend.Positions = new float[] { 0, 0.5f, 1 };
                    blend.Colors = new Color[] { col1, col2, col1 };
                    pathGradient.InterpolationColors = blend;
                    g.FillPath(pathGradient, gp);
                }
                break;
        }

        g.SmoothingMode = stored;
    }

    public void DrawShadowPath(Graphics g, ColorBlend colBlend, GraphicsPath path)
    {
        using (PathGradientBrush shadowBrush = new PathGradientBrush(path))
        {
            shadowBrush.InterpolationColors = colBlend;
            g.FillPath(shadowBrush, path);
        }
    }

    public void DrawShadowEllipse(Graphics g, Color col, Rectangle path)
    {
        using (GraphicsPath gPath = new GraphicsPath())
        {
            gPath.AddEllipse(path);
            using (PathGradientBrush pathGradBrush = new PathGradientBrush(gPath))
            {
                pathGradBrush.CenterPoint = new PointF(path.X + path.Width / 2, path.Y + path.Height / 2);
                pathGradBrush.CenterColor = col;
                pathGradBrush.SurroundColors = new Color[] { Color.Transparent };
                pathGradBrush.SetBlendTriangularShape(0.1f, 1.0f);
                pathGradBrush.FocusScales = new PointF(0.0f, 0.0f);
                g.FillPath(pathGradBrush, gPath);
            }
        }
    }

    public void DrawTextWithShadow(Graphics g, Rectangle contRect, string text, Font tFont, HorizontalAlignment tAlign, Color tColor, Color bColor)
    {
        DrawText(g, new Rectangle(contRect.X + 1, contRect.Y + 2, contRect.Width + 1, contRect.Height + 2), text, tFont, tAlign, bColor);
        DrawText(g, contRect, text, tFont, tAlign, tColor);
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
                g.DrawString(text, tFont, new SolidBrush(tColor), contRect.X + 4 + contRect.Width / 2 - textSize.Width / 2, centeredY);
                break;
        }
    }

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

    public GraphicsPath RoundedTopRect(Rectangle rectangle, int curve)
    {
        GraphicsPath path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;

        path.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        path.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        path.AddLine(new Point(rectangle.X + rectangle.Width, rectangle.Y + arcRectangleWidth), new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
        path.AddLine(new Point(rectangle.X, rectangle.Height + rectangle.Y), new Point(rectangle.X, rectangle.Y + curve));

        return path;
    }
}
#endregion
#region Base Classes
public class ThemedControl : Control
{
    public DrawUtils D { get; private set; } = new DrawUtils();
    public MouseState State { get; set; } = MouseState.None;
    public Palette Pal { get; set; }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Обработка рисования фона, если нужно
    }

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
            ColHighest = Color.FromArgb(100, 105, 110),
            ColHigh = Color.FromArgb(65, 67, 69),
            ColMed = Color.FromArgb(40, 42, 44),
            ColDim = Color.FromArgb(30, 32, 34),
            ColDark = Color.FromArgb(15, 16, 17)
        };
        BackColor = Pal.ColDim;
    }
}
[ToolboxItem(false)]
public class ThemedContainer : ContainerControl
{
    public DrawUtils D { get; private set; } = new DrawUtils();
    protected bool Drag { get; set; } = true;
    public MouseState State { get; set; } = MouseState.None;
    protected bool TopCap { get; set; } = false;
    protected bool SizeCap { get; set; } = false;
    public Palette Pal { get; set; }
    protected Point MouseP { get; set; } = new Point(0, 0);
    protected int TopGrip;

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Обработка рисования фона, если нужно
    }

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
                TopCap = true;
                MouseP = e.Location;
            }
            else if (Drag && new Rectangle(Width - 15, Height - 15, 15, 15).Contains(e.Location))
            {
                SizeCap = true;
                MouseP = e.Location;
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
        if (Drag)
        {
            SizeCap = false;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (TopCap)
        {
            Point newLocation = new Point(Cursor.Position.X - MouseP.X, Cursor.Position.Y - MouseP.Y);
            Parent.Location = newLocation;
        }
        if (Drag && SizeCap)
        {
            MouseP = e.Location;
            Parent.Size = new Size(MouseP);
            Invalidate();
        }
    }


    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }
    [ToolboxItem(false)]
    public ThemedContainer()
    {
        MinimumSize = new Size(20, 20);
        ForeColor = Color.FromArgb(146, 149, 152);
        Font = new Font("Trebuchet MS", 10.0F);
        DoubleBuffered = true;
        Pal = new Palette
        {
            ColHighest = Color.FromArgb(100, 105, 110),
            ColHigh = Color.FromArgb(65, 67, 69),
            ColMed = Color.FromArgb(40, 42, 44),
            ColDim = Color.FromArgb(30, 32, 34),
            ColDark = Color.FromArgb(15, 16, 17)
        };
        BackColor = Pal.ColDim;
    }
}
#endregion
//public class AzenisForm : ThemedContainer
//{
//    private Color titleTextColor = Color.White; // Цвет текста заголовка
//    private ContentAlignment titleTextAlignment = ContentAlignment.MiddleCenter; // Выравнивание заголовка

//    public Color TitleTextColor
//    {
//        get => titleTextColor;
//        set { titleTextColor = value; Invalidate(); }
//    }

//    public ContentAlignment TitleTextAlignment
//    {
//        get => titleTextAlignment;
//        set { titleTextAlignment = value; Invalidate(); }
//    }

//    public AzenisForm()
//    {
//        MinimumSize = new Size(305, 150);
//        Dock = DockStyle.Fill;
//        TopGrip = 30;
//        Font = new Font("Segoe UI", 10.0F);
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        Graphics g = e.Graphics;
//        base.OnPaint(e);

//        try
//        {
//            ParentForm.TransparencyKey = Color.Transparent; // Убираем розовый цвет
//            ParentForm.MinimumSize = MinimumSize;
//            if (ParentForm.FormBorderStyle != FormBorderStyle.None)
//            {
//                ParentForm.FormBorderStyle = FormBorderStyle.None;
//            }
//        }
//        catch { }

//        g.Clear(Color.Transparent); // Убираем цвет фона

//        // Main shape
//        GraphicsPath mainShape = D.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 8);
//        GraphicsPath doStuffPath = D.RoundRect(new Rectangle(5, TopGrip + 2, Width - 11, Height - TopGrip - 8), 8);
//        g.FillPath(new SolidBrush(Pal.ColHighest), mainShape);

//        // Interior shading
//        ColorBlend colBlend = new ColorBlend(3);
//        int blurScale = (int)(Math.Sqrt((Width * Width) + (Height * Height)) / 10);
//        colBlend.Colors = new Color[] { Color.Transparent, Color.FromArgb(255, Pal.ColDim), Color.FromArgb(255, Pal.ColDim) };
//        colBlend.Positions = new float[] { 0, 1f / blurScale, 1 };

//        D.DrawShadowPath(g, colBlend, mainShape);
//        g.DrawPath(new Pen(Pal.ColDark), doStuffPath);

//        // Top bar
//        g.SmoothingMode = SmoothingMode.HighQuality;
//        GraphicsPath topPath = D.RoundedTopRect(new Rectangle(0, 0, Width - 2, TopGrip + 3), 8);
//        LinearGradientBrush topPathLGB = new LinearGradientBrush(new Point(0, 0), new Point(0, TopGrip + 5), Pal.ColHighest, Color.Transparent);
//        g.FillPath(topPathLGB, topPath);

//        // Title text
//        Rectangle titleRectangle = new Rectangle(0, 0, Width, TopGrip);
//        using (SolidBrush titleBrush = new SolidBrush(TitleTextColor))
//        {
//            StringFormat format = new StringFormat
//            {
//                Alignment = titleTextAlignment switch
//                {
//                    ContentAlignment.MiddleLeft => StringAlignment.Near,
//                    ContentAlignment.MiddleRight => StringAlignment.Far,
//                    _ => StringAlignment.Center
//                },
//                LineAlignment = StringAlignment.Center
//            };

//            // Правильное использование метода DrawString
//            g.DrawString(Text, Font, titleBrush, titleRectangle, format);
//        }

//        // Border
//        g.SmoothingMode = SmoothingMode.None;
//        g.DrawPath(new Pen(Pal.ColDim), mainShape);
//    }

//}
public class AzenisForm : ThemedContainer
{
    private Color foreColor = Color.White; // Изначальный цвет текста

    public override Color ForeColor
    {
        get => foreColor;
        set
        {
            foreColor = value;
            Invalidate(); // Перерисовка формы при изменении цвета текста
        }
    }
    public enum TextAlign
    {
        Left,
        Center,
        Right
    }
    private TextAlign textAlignment = TextAlign.Center;
    public TextAlign TextAlignment
    {
        get => textAlignment;
        set
        {
            textAlignment = value;
            Invalidate(); // Перерисовка формы при изменении выравнивания текста
        }
    }

    public AzenisForm()
    {
        MinimumSize = new Size(305, 150);
        Dock = DockStyle.Fill;
        TopGrip = 30;
        Font = new Font("Segoe UI", 10.0F);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        base.OnPaint(e);

        try
        {
            ParentForm.TransparencyKey = Color.Empty;
            ParentForm.MinimumSize = MinimumSize;
            if (ParentForm.FormBorderStyle != FormBorderStyle.None)
            {
                ParentForm.FormBorderStyle = FormBorderStyle.None;
            }
        }
        catch { }

        g.Clear(ParentForm.TransparencyKey);

        // Main shape
        using (GraphicsPath mainShape = D.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 8))
        {
            using (GraphicsPath doStuffPath = D.RoundRect(new Rectangle(5, TopGrip + 2, Width - 11, Height - TopGrip - 8), 8))
            {
                g.FillPath(new SolidBrush(Pal.ColHighest), mainShape);

                // Interior shading
                ColorBlend colBlend = new ColorBlend(3);
                int blurScale = (int)(Math.Sqrt((Width * Width) + (Height * Height)) / 10);
                colBlend.Colors = new[] { Color.Transparent, Color.FromArgb(255, Pal.ColDim), Color.FromArgb(255, Pal.ColDim) };
                colBlend.Positions = new[] { 0, 1f / blurScale, 1 };
                D.DrawShadowPath(g, colBlend, mainShape);

                // Border
                g.DrawPath(new Pen(Pal.ColDark), doStuffPath);
            }
        }

        // Top bar
        g.SmoothingMode = SmoothingMode.HighQuality;
        using (GraphicsPath topPath = D.RoundedTopRect(new Rectangle(0, 0, Width - 2, TopGrip + 3), 8))
        {
            using (LinearGradientBrush topPathLGB = new LinearGradientBrush(new Point(0, 0), new Point(0, TopGrip + 5), Pal.ColHighest, Color.Transparent))
            {
                g.FillPath(topPathLGB, topPath);
            }

            g.DrawLine(new Pen(Color.FromArgb(50, Pal.ColDim)), new Point(9, 0), new Point(9, TopGrip + 5));
            g.DrawLine(new Pen(Color.FromArgb(50, Pal.ColDim)), new Point(Width - 10, 0), new Point(Width - 10, TopGrip + 5));
        }

        // Top bar - Inner
        Rectangle baRct = new Rectangle(15, 4, Width - 31, TopGrip * 4 / 5);
        using (GraphicsPath dp = D.RoundRect(baRct, 5))
        {
            using (HatchBrush textureBrush = new HatchBrush(HatchStyle.DarkDownwardDiagonal, Pal.ColMed, Color.Transparent))
            {
                g.DrawPath(new Pen(Pal.ColDark, 3), dp);
                g.FillPath(new SolidBrush(Pal.ColDark), dp);
                g.FillPath(textureBrush, dp);
                D.FillDualGradPath(g, Pal.ColDark, Color.Transparent, baRct, dp, GradientAlignment.Horizontal);
                g.DrawPath(new Pen(Color.FromArgb(100, Pal.ColHigh)), dp);
            }
        }

        // Final border
        g.SmoothingMode = SmoothingMode.None;
        using (GraphicsPath mainShape = D.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 8))
        {
            g.DrawPath(new Pen(Pal.ColDim), mainShape);
        }

        // Draw text with selected alignment
        DrawTextWithAlignment(g, new Rectangle(0, 0, Width - 1, TopGrip), Text, Font);
    }

    private void DrawTextWithAlignment(Graphics g, Rectangle rect, string text, Font font)
    {
        // Добавляем отступ для текста
        int padding = 15;
        Rectangle paddedRect = new Rectangle(rect.X + padding, rect.Y + padding, rect.Width - 2 * padding, rect.Height - 2 * padding);

        StringFormat stringFormat = new StringFormat
        {
            Alignment = textAlignment switch
            {
                TextAlign.Left => StringAlignment.Near,
                TextAlign.Center => StringAlignment.Center,
                TextAlign.Right => StringAlignment.Far,
                _ => StringAlignment.Center,
            },
            LineAlignment = StringAlignment.Center
        };

        using (SolidBrush textBrush = new SolidBrush(ForeColor))
        {
            g.DrawString(text, font, textBrush, paddedRect, stringFormat);
        }
    }
}
public class AzenisButton : ThemedControl
{
    public AzenisButton()
    {
        Font = new Font("Trebuchet MS", 10.0F);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        base.OnPaint(e);
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.HighQuality;

        using (GraphicsPath mainShape = D.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 2))
        {
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColHigh, Pal.ColDim))
            {
                g.FillPath(lgb, mainShape);
            }

            Color textCol, topColor;
            switch (State)
            {
                case MouseState.None:
                    topColor = Pal.ColHighest;
                    textCol = Color.FromArgb(Pal.ColHighest.R + 10, Pal.ColHighest.G + 10, Pal.ColHighest.B + 10);
                    break;

                case MouseState.Over:
                    topColor = Color.FromArgb(Pal.ColHighest.R + 20, Pal.ColHighest.G + 20, Pal.ColHighest.B + 20);
                    g.FillPath(new SolidBrush(Color.FromArgb(10, Color.WhiteSmoke)), mainShape);
                    textCol = Color.FromArgb(Pal.ColHighest.R + 60, Pal.ColHighest.G + 60, Pal.ColHighest.B + 60);
                    break;

                case MouseState.Down:
                    topColor = Pal.ColDark;
                    g.FillPath(new SolidBrush(Color.FromArgb(100, Pal.ColDark)), mainShape);
                    textCol = Color.FromArgb(Pal.ColHighest.R - 20, Pal.ColHighest.G - 20, Pal.ColHighest.B - 20);
                    break;

                default:
                    topColor = Pal.ColHighest;
                    textCol = Color.FromArgb(Pal.ColHighest.R + 10, Pal.ColHighest.G + 10, Pal.ColHighest.B + 10);
                    break;
            }

            using (LinearGradientBrush lgb2 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), topColor, Pal.ColDark))
            {
                g.DrawPath(new Pen(lgb2), mainShape);
            }

            D.DrawTextWithShadow(g, new Rectangle(0, 0, Width - 1, Height - 1), Text, Font, HorizontalAlignment.Center, textCol, Pal.ColDark);
        }
    }
}
public class AzenisCheckbox : ThemedControl
{
    public bool Checked { get; set; }

    public AzenisCheckbox()
    {
        Font = new Font("Trebuchet MS", 10.0F);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Checked = !Checked;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        base.OnPaint(e);
        g.Clear(BackColor);
        Height = 21;
        g.SmoothingMode = SmoothingMode.HighQuality;

        // |====| Основная форма
        using (GraphicsPath mainShape = D.RoundRect(new Rectangle(0, 0, Height - 1, Height - 1), 2))
        {
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColHigh, Pal.ColDim))
            {
                g.FillPath(lgb, mainShape);
            }

            // Граница
            Color textCol = Color.FromArgb(Pal.ColHighest.R + 10, Pal.ColHighest.G + 10, Pal.ColHighest.B + 10);
            using (LinearGradientBrush lgb2 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColHighest, Pal.ColDark))
            {
                g.DrawPath(new Pen(lgb2), mainShape);
            }

            // |====| Внутренняя форма
            using (GraphicsPath innerShape = D.RoundRect(new Rectangle(2, 2, Height - 5, Height - 5), 2))
            {
                using (LinearGradientBrush lgb3 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColDark, Pal.ColHighest))
                {
                    g.FillPath(lgb3, innerShape);
                }

                // Проверка
                if (Checked)
                {
                    using (GraphicsPath innerCheckShape = D.RoundRect(new Rectangle(3, 3, Height - 7, Height - 8), 2))
                    {
                        g.FillPath(new SolidBrush(Color.CadetBlue), innerCheckShape);
                        ColorBlend colBlend = new ColorBlend(2);
                        colBlend.Colors = new Color[] { Color.FromArgb(255, Color.FromArgb(11, 20, 70)), Color.Transparent };
                        colBlend.Positions = new float[] { 0, 1 };
                        D.DrawShadowPath(g, colBlend, innerCheckShape);
                        g.DrawPath(Pens.Black, innerCheckShape);
                    }
                }

                D.DrawTextWithShadow(g, new Rectangle(Height + 3, 0, Width - 1, Height - 4), Text, Font, HorizontalAlignment.Left, textCol, Pal.ColDark);
            }
        }
    }
}
public class AzenisRadiobutton : ThemedControl
{
    public bool Checked { get; set; }

    public AzenisRadiobutton()
    {
        Font = new Font("Trebuchet MS", 10.0F);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        foreach (Control cont in Parent.Controls)
        {
            if (cont is AzenisRadiobutton radioButton)
            {
                radioButton.Checked = false;
                cont.Invalidate();
            }
        }
        Checked = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        base.OnPaint(e);
        g.Clear(BackColor);
        Height = 21;
        g.SmoothingMode = SmoothingMode.HighQuality;

        // |====| Основная форма
        Rectangle mainShape = new Rectangle(0, 0, Height - 1, Height - 1);
        using (LinearGradientBrush lgb = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColHigh, Pal.ColDim))
        {
            g.FillEllipse(lgb, mainShape);
        }

        // Граница
        Color textCol = Color.FromArgb(Pal.ColHighest.R + 10, Pal.ColHighest.G + 10, Pal.ColHighest.B + 10);
        using (LinearGradientBrush lgb2 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColHighest, Pal.ColDark))
        {
            g.DrawEllipse(new Pen(lgb2), mainShape);
        }

        // |====| Внутренняя форма
        Rectangle innerShape = new Rectangle(2, 2, Height - 5, Height - 5);
        using (LinearGradientBrush lgb3 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColDark, Pal.ColHighest))
        {
            g.FillEllipse(lgb3, innerShape);
        }

        // Проверка
        if (Checked)
        {
            Rectangle innerCheckShape = new Rectangle(3, 3, Height - 8, Height - 8);
            g.FillEllipse(new SolidBrush(Color.CadetBlue), innerCheckShape);
            D.DrawShadowEllipse(g, Color.FromArgb(11, 20, 70), innerCheckShape);
            g.DrawEllipse(Pens.Black, innerCheckShape);
        }

        D.DrawTextWithShadow(g, new Rectangle(Height + 3, 0, Width - 1, Height - 4), Text, Font, HorizontalAlignment.Left, textCol, Pal.ColDark);
    }
}
public class WhiteUIHorizontalBar : ThemedControl
{
    public int Minimum { get; set; } = 0;
    public int Value { get; set; } = 50;
    public int Maximum { get; set; } = 100;

    public WhiteUIHorizontalBar()
    {
        Font = new Font("Segoe UI", 10.0F, FontStyle.Bold);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (e.Button == MouseButtons.Left)
        {
            int newMin = Minimum - 10;
            int newMax = Maximum + 10;
            int offset = 24;

            if (e.X > offset && e.X < Width - offset)
            {
                Value = (newMin + (e.X * ((newMax - newMin) / Width)));
            }
            else if (e.X <= offset)
            {
                Value = (newMin + (offset * ((newMax - newMin) / Width)));
            }
            else if (e.X >= Width - offset)
            {
                Value = (newMin + ((Width - offset) * ((newMax - newMin) / Width)));
            }
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        base.OnPaint(e);
        g.Clear(Parent.BackColor);
        g.SmoothingMode = SmoothingMode.HighQuality;

        using (GraphicsPath mainPath = D.RoundRect(new Rectangle(2, 4, Width - 6, Height - 14), 4))
        {
            using (LinearGradientBrush mainPathLGB = new LinearGradientBrush(new Point(0, 0), new Point(0, Height - 1), Color.FromArgb(20, 100, 180), Color.FromArgb(15, 70, 140)))
            using (LinearGradientBrush mainPathHighlightLGB = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Color.FromArgb(80, 80, 80), Color.FromArgb(22, 22, 22)))
            {
                g.FillPath(mainPathLGB, mainPath);
                g.DrawPath(new Pen(Color.FromArgb(60, Color.Black), 2), mainPath);
                g.DrawPath(new Pen(mainPathHighlightLGB, 2), mainPath);
            }
        }

        // |====| Заполнение и граница
        int gripX = (int)(ValueToPercentage(Value) * Width) - 15;
        Rectangle gripRect = new Rectangle(gripX, 0, 30, Height - 5);
        using (GraphicsPath gripPath = D.RoundRect(gripRect, 6))
        {
            Rectangle gripShadowRect = new Rectangle(gripX - 2, -1, 34, Height);
            using (GraphicsPath gripShadowPath = D.RoundRect(gripShadowRect, 6))
            {
                ColorBlend colBlend = new ColorBlend(2);
                colBlend.Colors = new Color[] { Color.Transparent, Color.FromArgb(70, Color.Black) };
                colBlend.Positions = new float[] { 0, 1 };
                D.DrawShadowPath(g, colBlend, gripShadowPath);
            }

            using (LinearGradientBrush gripLGB = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Color.FromArgb(80, 80, 80), Color.FromArgb(22, 22, 22)))
            using (LinearGradientBrush gripLGB2 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Color.FromArgb(22, 22, 22), Color.FromArgb(80, 80, 80)))
            {
                g.FillPath(gripLGB, gripPath);
                g.FillPath(gripLGB2, gripPath);
                g.DrawPath(new Pen(new LinearGradientBrush(new Point(0, 0), new Point(0, Height - 3), Color.FromArgb(130, 130, 130), Color.FromArgb(22, 22, 22))), gripPath);
            }
        }
    }

    private float ValueToPercentage(int value)
    {
        int min = Minimum - 10;
        int max = Maximum + 10;
        return (float)(value - min) / (max - min);
    }
}
[ToolboxItem(false)]
public class ThemedComboBox : ComboBox
{
    public DrawUtils D { get; private set; } = new DrawUtils();
    public MouseState State { get; set; } = MouseState.None;
    public Palette Pal { get; set; }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Переопределение без выполнения действий
    }

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

    public ThemedComboBox()
    {
        MinimumSize = new Size(20, 20);
        ForeColor = Color.FromArgb(146, 149, 152);
        Font = new Font("Segoe UI", 10.0F);
        DoubleBuffered = true;

        Pal = new Palette
        {
            ColHighest = Color.FromArgb(100, 105, 110),
            ColHigh = Color.FromArgb(65, 67, 69),
            ColMed = Color.FromArgb(40, 42, 44),
            ColDim = Color.FromArgb(30, 32, 34),
            ColDark = Color.FromArgb(15, 16, 17)
        };
        BackColor = Pal.ColDim;
    }
}
public class AzenisCombobox : ThemedComboBox
{
    private int strtIndex = 0;

    public int StartIndex
    {
        get => strtIndex;
        set
        {
            strtIndex = value;
            try
            {
                base.SelectedIndex = value;
            }
            catch
            {
                // Игнорируем исключения
            }
            Invalidate();
        }
    }

    public AzenisCombobox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        DrawMode = DrawMode.OwnerDrawFixed;
        DropDownStyle = ComboBoxStyle.DropDownList;
        Font = new Font("Trebuchet MS", 10.0F);
    }

    protected void OnItemPaint(object sender, DrawItemEventArgs e)
    {
        Graphics g = e.Graphics;
        e.DrawBackground();
        Rectangle borderRect = e.Bounds;

        try
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(0, 145, 235)), borderRect);
            }
            g.DrawString(base.GetItemText(base.Items[e.Index]), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
        }
        catch
        {
            // Игнорируем исключения
        }

        e.DrawFocusRectangle();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics g = e.Graphics;
        base.OnPaint(e);
        g.Clear(BackColor);
        g.SmoothingMode = SmoothingMode.HighQuality;

        using (GraphicsPath mainShape = D.RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 2))
        using (GraphicsPath textRect = D.RoundRect(new Rectangle(3, 3, Width - 21, Height - 7), 2))
        {
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColHigh, Pal.ColDim))
            {
                g.FillPath(lgb, mainShape);
            }

            g.FillPath(new SolidBrush(Pal.ColDim), textRect);
            g.DrawPath(new Pen(Pal.ColDark), textRect);

            Color topColor = Color.FromArgb(Pal.ColHighest.R + 10, Pal.ColHighest.G + 10, Pal.ColHighest.B + 10);
            using (LinearGradientBrush lgb2 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), Pal.ColHighest, Pal.ColDark))
            {
                g.DrawPath(new Pen(lgb2), mainShape);
            }

            // Рисуем стрелки
            g.DrawLine(new Pen(Pal.ColDark, 3), new Point(Width - 15, 7), new Point(Width - 3, 7));
            g.DrawLine(new Pen(Pal.ColHighest, 1), new Point(Width - 15, 7), new Point(Width - 3, 7));

            g.DrawLine(new Pen(Pal.ColDark, 3), new Point(Width - 15, Height - 9), new Point(Width - 3, Height - 9));
            g.DrawLine(new Pen(Pal.ColHighest, 1), new Point(Width - 15, Height - 9), new Point(Width - 3, Height - 9));

            g.DrawLine(new Pen(Pal.ColDark, 3), new Point(Width - 15, Height - 13), new Point(Width - 3, Height - 13));
            g.DrawLine(new Pen(Pal.ColHighest, 1), new Point(Width - 15, Height - 13), new Point(Width - 3, Height - 13));
        }

        D.DrawTextWithShadow(g, new Rectangle(5, 0, Width - 11, Height - 1), Text, Font, HorizontalAlignment.Left, Pal.ColHighest, Pal.ColDark);
    }
}