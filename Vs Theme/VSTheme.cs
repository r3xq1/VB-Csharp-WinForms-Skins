using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

[DesignerCategory("Code")]
public class Draw
{
    // Выполняет градиентное заполнение прямоугольника
    public static void Gradient(Graphics g, Color c1, Color c2, int x, int y, int width, int height)
    {
        Rectangle rect = new Rectangle(x, y, width, height);
        using var brush = new LinearGradientBrush(rect, c1, c2, LinearGradientMode.Vertical);
        g.FillRectangle(brush, rect);
    }

    // Выполняет смешивание цветов и заполняет градиент
    public static void Blend(Graphics g, Color c1, Color c2, Color c3, float ratio, int mode, int x, int y, int width, int height)
    {
        var colorBlend = new ColorBlend(3)
        {
            Colors = [c1, c2, c3],
            Positions = [0, ratio, 1]
        };
        Rectangle rect = new Rectangle(x, y, width, height);
        using var brush = new LinearGradientBrush(rect, c1, c1, (LinearGradientMode)mode);
        brush.InterpolationColors = colorBlend;
        g.FillRectangle(brush, rect);
    }
}

[DesignerCategory("Code")]
public class VSTheme : ContainerControl
{
    private int titleHeight = 23;
    private HorizontalAlignment titleAlign;

    [Category("Appearance")]
    public int TitleHeight
    {
        get => titleHeight;
        set
        {
            // Ограничиваем значение по высоте
            titleHeight = value > Height ? Height : (value < 2 ? 2 : value);
            Invalidate();
        }
    }

    [Category("Appearance")]
    public HorizontalAlignment TitleAlign
    {
        get => titleAlign;
        set
        {
            titleAlign = value;
            Invalidate();
        }
    }

    private Image tile;

    public VSTheme()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
        Size = new Size(300, 100);
        AutoScroll = true;
        Dock = DockStyle.Fill;
        // Создаем текстуру для фона
        using Bitmap bmp = new Bitmap(3, 3);
        using Graphics g = Graphics.FromImage(bmp);
        g.Clear(Color.FromArgb(53, 67, 88));
        g.DrawLine(new Pen(Color.FromArgb(33, 46, 67)), 0, 0, 2, 2);
        tile = (Image)bmp.Clone();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bmp = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bmp);
        using (var brush = new TextureBrush(tile, 0))
        {
            g.FillRectangle(brush, 0, TitleHeight, Width, Height - TitleHeight);
        }

        Draw.Gradient(g, Color.FromArgb(249, 245, 226), Color.FromArgb(255, 232, 165), 0, 0, Width, TitleHeight);
        g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), 0, 0, Width, TitleHeight / 2);

        g.DrawRectangle(new Pen(Color.FromArgb(255, 232, 165), 2), 1, TitleHeight - 1, Width - 2, Height - TitleHeight);

        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        SizeF size = g.MeasureString(Text, Font);
        float offsetX = titleAlign switch
        {
            HorizontalAlignment.Center => (Width / 2) - (size.Width / 2),
            HorizontalAlignment.Right => Width - size.Width - 6,
            _ => 6,
        };

        g.DrawString(Text, Font, new SolidBrush(Color.FromArgb(111, 88, 38)), offsetX, (TitleHeight / 2) - (size.Height / 2));

        e.Graphics.DrawImage(bmp, 0, 0);
    }

    protected override bool IsInputChar(char charCode) => true;

    protected override bool IsInputKey(Keys keyData) => base.IsInputKey(keyData);

    protected override void OnControlAdded(ControlEventArgs e)
    {
        base.OnControlAdded(e);
        e.Control.BringToFront();
    }
}

[DesignerCategory("Code")]
public class VSButton : Control
{
    private int state;

    public VSButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.DoubleBuffer, true);
        Size = new Size(100, 40);
        ForeColor = Color.FromArgb(111, 88, 38);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        state = 1;
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        state = 2;
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        state = 0;
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        state = 1;
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bmp = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bmp);
        var backgroundColor1 = state == 2 ? Color.FromArgb(255, 232, 165) : Color.FromArgb(249, 245, 226);
        var backgroundColor2 = state == 2 ? Color.FromArgb(249, 245, 226) : Color.FromArgb(255, 232, 165);
        Draw.Gradient(g, backgroundColor1, backgroundColor2, 0, 0, Width, Height);

        if (state < 2)
        {
            g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 255, 255)), 0, 0, Width, Height / 2);
        }

        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        SizeF size = g.MeasureString(Text, Font);

        if (!string.IsNullOrEmpty(Text) && Font != null)
        {
            float textX = (Width / 2) - (size.Width / 2);
            float textY = (Height / 2) - (size.Height / 2);
            g.DrawString(Text, Font, new SolidBrush(ForeColor), textX, textY);
        }

        g.DrawRectangle(new Pen(Color.FromArgb(249, 245, 226)), 0, 0, Width - 1, Height - 1);

        e.Graphics.DrawImage(bmp, 0, 0);
    }
}
