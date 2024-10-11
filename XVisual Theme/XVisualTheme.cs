using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

//--------------------- [ Theme ] --------------------
// Creator: Mephobia
// Contact: Mephobia.HF (Skype)
// Created: 6.19.2013
// Changed: 6.19.2013
//-------------------- [ /Theme ] ---------------------

enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

static class Draw
{
    // Возвращает SolidBrush для заданного цвета
    public static SolidBrush GetBrush(Color c)
    {
        return new SolidBrush(c);
    }

    // Возвращает Pen для заданного цвета
    public static Pen GetPen(Color c)
    {
        return new Pen(c);
    }

    // Создает текстурную кисть с использованием цветов
    public static TextureBrush NoiseBrush(Color[] colors)
    {
        Bitmap B = new Bitmap(128, 128);
        Random R = new Random(128);

        for (int X = 0; X < B.Width; X++)
        {
            for (int Y = 0; Y < B.Height; Y++)
            {
                B.SetPixel(X, Y, colors[R.Next(colors.Length)]);
            }
        }

        TextureBrush T = new TextureBrush(B);
        B.Dispose();  // Освобождаем память

        return T;
    }

    // Создает округленный графический путь
    public static GraphicsPath CreateRound(int x, int y, int width, int height, int slope)
    {
        Rectangle createRoundAngle = new Rectangle(x, y, width, height);
        return CreateRound(createRoundAngle, slope);
    }

    // Создает округленный графический путь на основе прямоугольника
    public static GraphicsPath CreateRound(Rectangle r, int slope)
    {
        GraphicsPath createRoundPath = new GraphicsPath(FillMode.Winding);
        createRoundPath.AddArc(r.X, r.Y, slope, slope, 180.0F, 90.0F);
        createRoundPath.AddArc(r.Right - slope, r.Y, slope, slope, 270.0F, 90.0F);
        createRoundPath.AddArc(r.Right - slope, r.Bottom - slope, slope, slope, 0.0F, 90.0F);
        createRoundPath.AddArc(r.X, r.Bottom - slope, slope, slope, 90.0F, 90.0F);
        createRoundPath.CloseFigure();
        return createRoundPath;
    }

    // Рисует внутреннее свечение для заданного прямоугольника
    public static void InnerGlow(Graphics G, Rectangle rectangle, Color[] colors)
    {
        int subtractTwo = 1;
        int addOne = 0;
        foreach (Color c in colors)
        {
            G.DrawRectangle(new Pen(new SolidBrush(Color.FromArgb(c.R, c.G, c.B))),
                            rectangle.X + addOne,
                            rectangle.Y + addOne,
                            rectangle.Width - subtractTwo,
                            rectangle.Height - subtractTwo);
            subtractTwo += 2;
            addOne += 1;
        }
    }

    // Рисует внутреннее свечение с округлыми углами
    public static void InnerGlowRounded(Graphics G, Rectangle rectangle, int degree, Color[] colors)
    {
        int subtractTwo = 1;
        int addOne = 0;
        foreach (Color c in colors)
        {
            G.DrawPath(new Pen(new SolidBrush(Color.FromArgb(c.R, c.G, c.B))),
                        CreateRound(rectangle.X + addOne,
                                    rectangle.Y + addOne,
                                    rectangle.Width - subtractTwo,
                                    rectangle.Height - subtractTwo,
                                    degree));
            subtractTwo += 2;
            addOne += 1;
        }
    }
}

public class XVisualTheme : ContainerControl
{
    private TextureBrush TopTexture;
    private TextureBrush InnerTexture;
    private Font drawFont;

    private Point mouseP = new Point(0, 0);
    private bool cap = false;
    private const int MoveHeight = 53;
    private MouseState state = MouseState.None;

    public XVisualTheme()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.FromArgb(46, 43, 40);
        DoubleBuffered = true;

        TopTexture = Draw.NoiseBrush(new Color[] {
            Color.FromArgb(66, 64, 62),
            Color.FromArgb(63, 61, 59),
            Color.FromArgb(69, 67, 65)
        });

        InnerTexture = Draw.NoiseBrush(new Color[] {
            Color.FromArgb(57, 53, 50),
            Color.FromArgb(56, 52, 49),
            Color.FromArgb(58, 55, 51)
        });

        drawFont = new Font("Arial", 11, FontStyle.Bold);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics G = e.Graphics;
        base.OnPaint(e);
        G.Clear(Color.Fuchsia);

        Rectangle mainRect = new Rectangle(0, 0, Width, Height);

        using (LinearGradientBrush LeftHighlight = new LinearGradientBrush(mainRect, Color.FromArgb(66, 64, 63), Color.FromArgb(56, 54, 53), 90F))
        using (LinearGradientBrush RightHighlight = new LinearGradientBrush(mainRect, Color.FromArgb(80, 78, 77), Color.FromArgb(70, 68, 67), 90F))
        using (LinearGradientBrush TopOverlay = new LinearGradientBrush(new Rectangle(0, 0, Width - 1, 53), Color.FromArgb(15, Color.White), Color.FromArgb(100, Color.FromArgb(43, 40, 38)), 90F))
        {
            using (LinearGradientBrush mainGradient = new LinearGradientBrush(mainRect, Color.FromArgb(73, 71, 69), Color.FromArgb(69, 67, 64), 90F))
            {
                G.FillRectangle(mainGradient, mainRect); // Outside Rectangle
                G.DrawRectangle(Pens.Black, new Rectangle(0, 0, Width - 1, Height - 1));
                G.FillRectangle(InnerTexture, new Rectangle(10, 53, Width - 21, Height - 84)); // Inner Rectangle
                G.DrawRectangle(Pens.Black, new Rectangle(10, 53, Width - 21, Height - 84));
                G.FillRectangle(TopTexture, new Rectangle(0, 0, Width - 1, 53)); // Top Bar Rectangle
                G.FillRectangle(TopOverlay, new Rectangle(0, 0, Width - 1, 53));
                G.DrawRectangle(Pens.Black, new Rectangle(0, 0, Width - 1, 53));

                // Blend effect
                ColorBlend blend = new ColorBlend();
                blend.Colors = new Color[] { Color.FromArgb(10, Color.White), Color.FromArgb(10, Color.Black), Color.FromArgb(10, Color.White) };
                blend.Positions = new float[] { 0, 0.7f, 1 };

                Rectangle rect = new Rectangle(0, 0, Width - 1, 53);
                using (LinearGradientBrush br = new LinearGradientBrush(rect, Color.White, Color.Black, LinearGradientMode.Vertical))
                {
                    br.InterpolationColors = blend; // Blend colors into brush
                    G.FillRectangle(br, rect); // Fill rectangle with blend
                }

                // Draw highlights and shadows
                DrawHighlightsAndShadows(G, LeftHighlight, RightHighlight);
                G.DrawString(Text, drawFont, Brushes.White, new Rectangle(0, 0, Width, 37), new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

                // Draw corners
                DrawCorners(G);
            }
        }
    }

    private void DrawHighlightsAndShadows(Graphics G, LinearGradientBrush LeftHighlight, LinearGradientBrush RightHighlight)
    {
        G.DrawLine(Draw.GetPen(Color.FromArgb(173, 172, 172)), 4, 1, Width - 5, 1); // Top Middle Highlight
        G.DrawLine(Draw.GetPen(Color.FromArgb(110, 109, 107)), 11, Height - 30, Width - 12, Height - 30); // Bottom Middle Highlight

        // Top Left Corner Highlight
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(173, 172, 172)), 3, 2, 1, 1);
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(133, 132, 132)), 2, 2, 1, 1);
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(113, 112, 112)), 2, 3, 1, 1);
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(83, 82, 82)), 1, 4, 1, 1);

        // Top Right Corner Highlight
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(173, 172, 172)), Width - 4, 2, 1, 1);
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(133, 132, 132)), Width - 3, 2, 1, 1);
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(113, 112, 112)), Width - 3, 3, 1, 1);
        G.FillRectangle(Draw.GetBrush(Color.FromArgb(83, 82, 82)), Width - 2, 4, 1, 1);

        // Shadows
        DrawShadows(G);
    }

    private void DrawShadows(Graphics G)
    {
        G.DrawLine(Draw.GetPen(Color.FromArgb(91, 90, 89)), 1, 52, Width - 2, 52); // Middle Top Horizontal
        G.DrawLine(Draw.GetPen(Color.FromArgb(40, 37, 34)), 11, 54, Width - 12, 54);
        G.DrawLine(Draw.GetPen(Color.FromArgb(45, 42, 39)), 11, 55, Width - 12, 55);
        G.DrawLine(Draw.GetPen(Color.FromArgb(50, 47, 44)), 11, 56, Width - 12, 56);

        G.DrawLine(Draw.GetPen(Color.FromArgb(50, 47, 44)), 11, Height - 32, Width - 12, Height - 32); // Middle Bottom Horizontal
        G.DrawLine(Draw.GetPen(Color.FromArgb(52, 49, 46)), 11, Height - 33, Width - 12, Height - 33);
        G.DrawLine(Draw.GetPen(Color.FromArgb(54, 51, 48)), 11, Height - 34, Width - 12, Height - 34);

        // Draw vertical lines
        DrawVerticalLines(G);
    }

    private void DrawVerticalLines(Graphics G)
    {
        G.DrawLine(Draw.GetPen(Color.FromArgb(59, 57, 55)), 1, 54, 9, 54); // Left Horizontal
        G.DrawLine(Draw.GetPen(Color.FromArgb(64, 62, 60)), 1, 55, 9, 55);
        G.DrawLine(Draw.GetPen(Color.FromArgb(73, 71, 69)), 1, 56, 9, 56);

        G.DrawLine(Draw.GetPen(Color.FromArgb(59, 57, 55)), Width - 10, 54, Width - 2, 54); // Right Horizontal
        G.DrawLine(Draw.GetPen(Color.FromArgb(64, 62, 60)), Width - 10, 55, Width - 2, 55);
        G.DrawLine(Draw.GetPen(Color.FromArgb(73, 71, 69)), Width - 10, 56, Width - 2, 56);

        G.DrawLine(Draw.GetPen(Color.FromArgb(59, 57, 55)), 1, 54, 1, Height - 5); // Left Vertical
        G.DrawLine(Draw.GetPen(Color.FromArgb(64, 62, 60)), 2, 55, 2, Height - 4);
        G.DrawLine(Draw.GetPen(Color.FromArgb(73, 71, 69)), 3, 56, 3, Height - 3);

        G.DrawLine(Draw.GetPen(Color.FromArgb(69, 67, 65)), 9, 56, 9, Height - 31); // Fixed usage of GetPen

        G.DrawLine(Draw.GetPen(Color.FromArgb(59, 57, 55)), Width - 2, 54, Width - 2, Height - 5); // Right Vertical
        G.DrawLine(Draw.GetPen(Color.FromArgb(64, 62, 60)), Width - 3, 55, Width - 3, Height - 4);
        G.DrawLine(Draw.GetPen(Color.FromArgb(73, 71, 69)), Width - 4, 56, Width - 4, Height - 3);
        G.DrawLine(Draw.GetPen(Color.FromArgb(69, 67, 65)), Width - 10, 56, Width - 10, Height - 31); // Fixed usage of GetPen
    }

    private void DrawCorners(Graphics G)
    {
        DrawCorner(G, 0, 0, Brushes.Black); // Левый верхний угол
        DrawCorner(G, Width - 1, 0, Brushes.Black); // Правый верхний угол
        DrawCorner(G, 0, Height - 1, Brushes.Black); // Левый нижний угол
        DrawCorner(G, Width - 1, Height - 1, Brushes.Black); // Правый нижний угол
    }

    private void DrawCorner(Graphics G, int x, int y, Brush brush)
    {
        for (int i = 0; i < 4; i++)
        {
            G.FillRectangle(brush, x - i, y, 1, 1);
            G.FillRectangle(brush, x, y - i, 1, 1);
        }

        G.FillRectangle(Brushes.Black, x - 1, y + 3, 1, 1);
        G.FillRectangle(Brushes.Black, x - 1, y + 2, 1, 1);
        G.FillRectangle(Brushes.Black, x - 2, y + 1, 1, 1);
        G.FillRectangle(Brushes.Black, x - 3, y + 1, 1, 1);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, MoveHeight).Contains(e.Location))
        {
            cap = true;
            mouseP = e.Location;
        }
        state = MouseState.Down;
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

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        cap = false;
        state = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (cap)
        {
            Parent.Location = new Point(MousePosition.X - mouseP.X, MousePosition.Y - mouseP.Y);
        }
        Invalidate();
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ParentForm.FormBorderStyle = FormBorderStyle.None;
        ParentForm.TransparencyKey = Color.Fuchsia;
        Dock = DockStyle.Fill;
        TabStop = false;
    }
}

public class xVisualControlBox : Control
{
    private MouseState state = MouseState.None;
    private int x; // Позиция курсора по оси X
    private Point normalLocation; // Нормальное расположение кнопок

    public xVisualControlBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(205, 205, 205);
        Size = new Size(83, 28);
        normalLocation = new Point(Width - 100, 12);
        Location = normalLocation;
        DoubleBuffered = true;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        var form = FindForm();
        switch (x) // Закрыть
        {
            case > 56 and < 71:
                form.Close();
                break;
            case > 33 and < 48:
                form.WindowState = form.WindowState == FormWindowState.Normal ? FormWindowState.Maximized : FormWindowState.Normal;
                break;
            case > 10 and < 25:
                form.WindowState = FormWindowState.Minimized;
                break;
        }
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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        x = e.Location.X;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (Bitmap bitmap = new Bitmap(Width, Height))
        using (Graphics g = Graphics.FromImage(bitmap))
        {
            base.OnPaint(e);
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.HighQuality;

            // Кнопки управления
            using (LinearGradientBrush controlGradient = new LinearGradientBrush(new Rectangle(10, 2, 15, 16), Color.FromArgb(67, 67, 67), Color.FromArgb(80, 80, 81), 90f))
            {
                g.FillEllipse(controlGradient, new Rectangle(10, 2, 15, 16)); // Свернуть
                g.FillEllipse(controlGradient, new Rectangle(33, 2, 15, 16)); // Развернуть
                g.FillEllipse(controlGradient, new Rectangle(56, 2, 15, 16)); // Закрыть
                g.DrawEllipse(Pens.Black, new Rectangle(10, 2, 15, 16));
                g.DrawEllipse(Pens.Black, new Rectangle(33, 2, 15, 16));
                g.DrawEllipse(Pens.Black, new Rectangle(56, 2, 15, 16));
            }

            // Наполнение верхней части кнопок
            using (LinearGradientBrush controlTopCircle = new LinearGradientBrush(new Rectangle(13, 4, 9, 7), Color.FromArgb(193, 190, 176), Color.FromArgb(90, 91, 92), 90f))
            {
                g.FillEllipse(controlTopCircle, new Rectangle(13, 4, 9, 7)); // Top Circle
                g.FillEllipse(controlTopCircle, new Rectangle(36, 4, 9, 7)); // Top Circle
                g.FillEllipse(controlTopCircle, new Rectangle(59, 4, 9, 7)); // Top Circle
            }

            using (LinearGradientBrush nControlBottomCircle = new LinearGradientBrush(new Rectangle(13, 12, 9, 5), Color.FromArgb(90, 91, 92), Color.FromArgb(155, 165, 174), 90f))
            {
                switch (state)
                {
                    case MouseState.None:
                        g.FillEllipse(nControlBottomCircle, new Rectangle(13, 12, 9, 5)); // Свернуть
                        g.FillEllipse(nControlBottomCircle, new Rectangle(36, 12, 9, 5)); // Развернуть
                        g.FillEllipse(nControlBottomCircle, new Rectangle(59, 12, 9, 5)); // Закрыть
                        break;

                    case MouseState.Over:
                        if (x > 10 && x < 25) // Свернуть
                        {
                            using (LinearGradientBrush controlBottomCircle = new LinearGradientBrush(new Rectangle(13, 12, 9, 5), Color.FromArgb(50, Color.Green), Color.FromArgb(10, Color.Green), 90f))
                            {
                                g.FillEllipse(controlBottomCircle, new Rectangle(10, 12, 9, 5)); // Свернуть
                                g.FillEllipse(nControlBottomCircle, new Rectangle(36, 12, 9, 5)); // Развернуть
                                g.FillEllipse(nControlBottomCircle, new Rectangle(59, 12, 9, 5)); // Закрыть
                            }
                        }
                        else if (x > 33 && x < 48) // Развернуть
                        {
                            using (LinearGradientBrush controlBottomCircle = new LinearGradientBrush(new Rectangle(13, 12, 9, 5), Color.FromArgb(50, Color.Yellow), Color.FromArgb(10, Color.Yellow), 90f))
                            {
                                g.FillEllipse(nControlBottomCircle, new Rectangle(10, 12, 9, 5)); // Свернуть
                                g.FillEllipse(controlBottomCircle, new Rectangle(33, 12, 9, 5)); // Развернуть
                                g.FillEllipse(nControlBottomCircle, new Rectangle(59, 12, 9, 5)); // Закрыть
                            }
                        }
                        else if (x > 56 && x < 71) // Закрыть
                        {
                            using (LinearGradientBrush controlBottomCircle = new LinearGradientBrush(new Rectangle(13, 12, 9, 5), Color.FromArgb(50, Color.Red), Color.FromArgb(10, Color.Red), 90f))
                            {
                                g.FillEllipse(nControlBottomCircle, new Rectangle(10, 12, 9, 5)); // Свернуть
                                g.FillEllipse(nControlBottomCircle, new Rectangle(36, 12, 9, 5)); // Развернуть
                                g.FillEllipse(controlBottomCircle, new Rectangle(56, 12, 9, 5)); // Закрыть
                            }
                        }
                        break;

                    case MouseState.Down:
                        // Дополнительная логика для состояния нажатия
                        break;
                }
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
    }

    protected override void OnParentChanged(EventArgs e)
    {
        base.OnParentChanged(e);
        if (Parent != null)
        {
            Parent.SizeChanged += (s, args) => UpdateLocation();
            UpdateLocation(); // Обновить начальное размещение при создании
        }
    }

    private void UpdateLocation()
    {
        var form = FindForm();
        if (form != null)
        {
            // Положение кнопок при нормальном состоянии
            if (form.WindowState == FormWindowState.Normal)
            {
                Location = new Point(form.ClientSize.Width - Width - 12, 12);
            }
            else
            {
                Location = new Point(form.ClientSize.Width - Width - 12, 12); // При развернутой форме
            }
        }
    }
}

public class xVisualButton : Control
{
    #region MouseStates
    private MouseState state = MouseState.None;

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

    public enum InnerShade
    {
        Light,
        Dark
    }

    private InnerShade _shade;
    public InnerShade Shade
    {
        get => _shade;
        set
        {
            _shade = value;
            Invalidate();
        }
    }

    public xVisualButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(205, 205, 205);
        _shade = InnerShade.Dark;
        DoubleBuffered = true;
    }

    private TextureBrush InnerTexture = CreateNoiseBrush(new[]
    {
        Color.FromArgb(52, 48, 44),
        Color.FromArgb(54, 50, 46),
        Color.FromArgb(50, 46, 42)
    });

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var bitmap = new Bitmap(Width, Height))
        using (var g = Graphics.FromImage(bitmap))
        {
            base.OnPaint(e);
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.HighQuality;

            var clientRectangle = new Rectangle(3, 3, Width - 7, Height - 7);

            if (_shade == InnerShade.Dark)
            {
                DrawDarkShade(g, clientRectangle);
            }
            else
            {
                DrawLightShade(g, clientRectangle);
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
    }

    private void DrawDarkShade(Graphics g, Rectangle clientRectangle)
    {
        g.FillPath(InnerTexture, CreateRoundRectangle(clientRectangle, 3));

        // Drawing borders and highlights
        DrawBorders(g, Color.FromArgb(40, 38, 36), 3);
        DrawBorders(g, Color.FromArgb(45, 43, 41), 3);
        DrawBorders(g, Color.FromArgb(50, 48, 46), 2);

        // Font
        DrawText(g, clientRectangle);
    }

    private void DrawLightShade(Graphics g, Rectangle clientRectangle)
    {
        using (var mainGradient = new LinearGradientBrush(clientRectangle, Color.FromArgb(225, 227, 230), Color.FromArgb(199, 201, 204), 90F))
        {
            g.FillPath(mainGradient, CreateRoundRectangle(clientRectangle, 3));
        }

        DrawBorders(g, Color.FromArgb(167, 168, 171), 3);
        DrawBorders(g, Color.FromArgb(203, 205, 208), 2);

        // Font
        DrawText(g, clientRectangle);
    }

    private void DrawBorders(Graphics g, Color color, int offset)
    {
        using (var pen = new Pen(color))
        {
            g.DrawPath(pen, CreateRoundRectangle(new Rectangle(3, 3, Width - 6 - offset, Height - 6 - offset), 3));
        }
    }

    private void DrawText(Graphics g, Rectangle clientRectangle)
    {
        using (var drawFont = new Font("Arial", 9, FontStyle.Bold))
        {
            var textBrush = Brushes.White;
            g.DrawString(Text, drawFont, textBrush, clientRectangle,
                new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
        }
    }

    private static TextureBrush CreateNoiseBrush(Color[] colors)
    {
        // Implement noise brush creation logic here
        return new TextureBrush(new Bitmap(1, 1)); // Placeholder
    }

    private static GraphicsPath CreateRoundRectangle(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class xVisualGroupBox : ContainerControl
{
    public enum InnerShade
    {
        Light,
        Dark
    }

    private InnerShade _shade;
    public InnerShade Shade
    {
        get => _shade;
        set
        {
            _shade = value;
            Invalidate();
        }
    }

    public xVisualGroupBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(205, 205, 205);
        Size = new Size(174, 115);
        _shade = InnerShade.Light;
        DoubleBuffered = true;
    }

    private TextureBrush TopTexture = CreateNoiseBrush(new[]
    {
        Color.FromArgb(49, 45, 41),
        Color.FromArgb(51, 47, 43),
        Color.FromArgb(47, 43, 39)
    });

    private TextureBrush InnerTexture = CreateNoiseBrush(new[]
    {
        Color.FromArgb(55, 52, 48),
        Color.FromArgb(57, 50, 50),
        Color.FromArgb(53, 50, 46)
    });

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var bitmap = new Bitmap(Width, Height))
        using (var g = Graphics.FromImage(bitmap))
        {
            base.OnPaint(e);
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.HighQuality;

            var clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            var barRectangle = new Rectangle(0, 0, Width - 1, 32);

            DrawBackground(g, clientRectangle);
            DrawTopOverlay(g, barRectangle, clientRectangle);
            DrawHighlightLines(g, barRectangle);

            using (var drawFont = new Font("Arial", 9, FontStyle.Bold))
            {
                g.DrawString(Text, drawFont, Brushes.White, new Rectangle(15, 3, Width - 1, 26),
                    new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
    }

    private void DrawBackground(Graphics g, Rectangle clientRectangle)
    {
        if (_shade == InnerShade.Light)
        {
            using (var mainGradient = new LinearGradientBrush(clientRectangle, Color.FromArgb(228, 230, 232), Color.FromArgb(199, 201, 205), 90F))
            {
                g.FillRectangle(mainGradient, clientRectangle);
            }
        }
        else if (_shade == InnerShade.Dark)
        {
            g.FillRectangle(InnerTexture, clientRectangle);
        }
        g.DrawRectangle(new Pen(Color.Black), clientRectangle);
    }

    private void DrawTopOverlay(Graphics g, Rectangle barRectangle, Rectangle clientRectangle)
    {
        using (var topOverlay = new LinearGradientBrush(clientRectangle, Color.FromArgb(5, Color.White), Color.FromArgb(10, Color.White), 90F))
        {
            g.FillRectangle(TopTexture, barRectangle);
        }

        var blend = new ColorBlend
        {
            Colors = new[] { Color.FromArgb(20, Color.White), Color.FromArgb(10, Color.Black), Color.FromArgb(10, Color.White) },
            Positions = new[] { 0, 0.9f, 1 }
        };

        using (var br = new LinearGradientBrush(barRectangle, Color.White, Color.Black, LinearGradientMode.Vertical))
        {
            br.InterpolationColors = blend;
            g.FillRectangle(br, barRectangle);
        }
        g.DrawRectangle(new Pen(Color.Black), barRectangle);
    }

    private void DrawHighlightLines(Graphics g, Rectangle barRectangle)
    {
        g.DrawLine(new Pen(Color.FromArgb(112, 109, 107)), 1, 1, Width - 2, 1);
        g.DrawLine(new Pen(Color.FromArgb(67, 63, 60)), 1, barRectangle.Height - 1, Width - 2, barRectangle.Height - 1);

        Rectangle innerGlowRectangle = new Rectangle(1, 33, Width - 2, Height - 34);
        Color[] innerGlowColors = _shade == InnerShade.Light ?
            new[] { Color.FromArgb(153, 153, 153), Color.FromArgb(173, 174, 177), Color.FromArgb(200, 201, 204) } :
            new[] { Color.FromArgb(43, 40, 38), Color.FromArgb(50, 47, 44), Color.FromArgb(55, 52, 49) };

        Draw.InnerGlow(g, innerGlowRectangle, innerGlowColors);
    }

    private static TextureBrush CreateNoiseBrush(Color[] colors)
    {
        Bitmap noiseBitmap = new Bitmap(1, 1);
        // Здесь вы можете добавить логику создания текстурного узора
        using (Graphics g = Graphics.FromImage(noiseBitmap))
        {
            // Генерация текстуры (пример код, не законченный)
            g.Clear(colors[0]); // Замена на ваш собственный алгоритм
        }

        return new TextureBrush(noiseBitmap);
    }
}

public class xVisualHeader : ContainerControl
{
    protected override void OnResize(EventArgs e)
    {
        Height = 32;
        base.OnResize(e);
    }

    public xVisualHeader()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(205, 205, 205);
        Size = new Size(174, 32);
        DoubleBuffered = true;
    }

    private TextureBrush TopTexture = CreateNoiseBrush(new[]
    {
        Color.FromArgb(49, 45, 41),
        Color.FromArgb(51, 47, 43),
        Color.FromArgb(47, 43, 39)
    });

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var bitmap = new Bitmap(Width, Height))
        using (var g = Graphics.FromImage(bitmap))
        {
            var barRect = new Rectangle(0, 0, Width - 1, Height - 1);
            base.OnPaint(e);

            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.HighQuality;

            using (var topOverlay = new LinearGradientBrush(barRect, Color.FromArgb(5, Color.White), Color.FromArgb(10, Color.White), 90F))
            {
                g.FillRectangle(TopTexture, barRect);
                g.FillRectangle(topOverlay, barRect);
            }

            var blend = new ColorBlend
            {
                Colors = new[]
                {
                    Color.FromArgb(20, Color.White),
                    Color.FromArgb(10, Color.Black),
                    Color.FromArgb(10, Color.White)
                },
                Positions = new[] { 0f, 0.9f, 1f }
            };

            using (var br = new LinearGradientBrush(barRect, Color.White, Color.Black, LinearGradientMode.Vertical))
            {
                br.InterpolationColors = blend;
                g.FillRectangle(br, barRect);
            }

            g.DrawRectangle(Pens.Black, barRect);

            // Top Bar Highlights
            g.DrawLine(new Pen(Color.FromArgb(112, 109, 107)), 1, 1, Width - 2, 1);
            g.DrawLine(new Pen(Color.FromArgb(67, 63, 60)), 1, barRect.Height - 1, Width - 2, barRect.Height - 1);

            var drawFont = new Font("Arial", 9, FontStyle.Bold);
            g.DrawString(Text, drawFont, Brushes.White, new Rectangle(15, 3, Width - 1, 26),
                new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
    }

    private static TextureBrush CreateNoiseBrush(Color[] colors)
    {
        Bitmap noiseBitmap = new Bitmap(1, 1);
        using (Graphics g = Graphics.FromImage(noiseBitmap))
        {
            g.Clear(colors[0]); // место для вашего алгоритма генерации текстуры
        }

        return new TextureBrush(noiseBitmap);
    }
}

public class xVisualSeparator : Control
{
    public enum LineStyle
    {
        Horizontal,
        Vertical
    }

    private LineStyle _style;
    public LineStyle Style
    {
        get => _style;
        set
        {
            _style = value;
            Invalidate();
        }
    }

    public xVisualSeparator()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(205, 205, 205);
        _style = LineStyle.Horizontal;

        Size = new Size(174, 3);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var bitmap = new Bitmap(Width, Height))
        using (var g = Graphics.FromImage(bitmap))
        {
            base.OnPaint(e);
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            switch (_style)
            {
                case LineStyle.Horizontal:
                    g.DrawLine(new Pen(Color.Black, 1), 0, Height / 2, Width, Height / 2);
                    g.DrawLine(new Pen(Color.FromArgb(99, 97, 94), 1), 0, Height / 2 + 1, Width, Height / 2 + 1);
                    break;

                case LineStyle.Vertical:
                    g.DrawLine(new Pen(Color.Black, 1), Width / 2, 0, Width / 2, Height);
                    g.DrawLine(new Pen(Color.FromArgb(99, 97, 94), 1), Width / 2 + 1, 0, Width / 2 + 1, Height);
                    break;
            }

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
    }
}

[DefaultEvent("TextChanged")]
public class xVisualTextBox : Control
{
    private int W, H;
    private MouseState State = MouseState.None;
    private TextBox TB;

    private HorizontalAlignment _TextAlign = HorizontalAlignment.Left;
    public enum RoundingStyle
    {
        Normal,
        Rounded
    }

    private RoundingStyle _Style;
    [Category("Options")]
    public RoundingStyle Style
    {
        get => _Style;
        set
        {
            _Style = value;
            if (TB != null)
            {
                TB.TextAlign = value == RoundingStyle.Normal ? HorizontalAlignment.Left : HorizontalAlignment.Center;
            }
        }
    }

    [Category("Options")]
    public HorizontalAlignment TextAlign
    {
        get => _TextAlign;
        set
        {
            _TextAlign = value;
            if (TB != null)
            {
                TB.TextAlign = value;
            }
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
            if (TB != null)
            {
                TB.MaxLength = value;
            }
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
            if (TB != null)
            {
                TB.ReadOnly = value;
            }
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
            if (TB != null)
            {
                TB.UseSystemPasswordChar = value;
            }
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
            if (TB != null)
            {
                TB.Multiline = value;
                if (value)
                {
                    TB.Height = Height - 11;
                }
                else
                {
                    Height = TB.Height + 11;
                }
            }
        }
    }

    [Category("Options")]
    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            if (TB != null)
            {
                TB.Text = value;
            }
        }
    }

    [Category("Options")]
    public override Font Font
    {
        get => base.Font;
        set
        {
            base.Font = value;
            if (TB != null)
            {
                TB.Font = value;
                TB.Location = new Point(3, 5);
                TB.Width = Width - 6;

                if (!_Multiline)
                {
                    Height = TB.Height + 11;
                }
            }
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        if (!Controls.Contains(TB))
        {
            Controls.Add(TB);
        }
    }

    private void OnBaseTextChanged(object sender, EventArgs e)
    {
        Text = TB.Text;
    }

    private void OnBaseKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.A)
        {
            TB.SelectAll();
            e.SuppressKeyPress = true;
        }
        if (e.Control && e.KeyCode == Keys.C)
        {
            TB.Copy();
            e.SuppressKeyPress = true;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        TB.Location = new Point(11, 5);
        TB.Width = Width - 14;

        if (_Multiline)
        {
            TB.Height = Height - 11;
        }
        else
        {
            Height = TB.Height + 11;
        }

        base.OnResize(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        State = MouseState.Down;
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        State = MouseState.Over;
        TB.Focus();
        Invalidate();
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        State = MouseState.Over;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        State = MouseState.None;
        Invalidate();
    }

    private Color _BaseColor = Color.FromArgb(242, 242, 242);
    private Color _TextColor = Color.FromArgb(30, 30, 30);

    public xVisualTextBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;

        BackColor = Color.Transparent;

        TB = new TextBox
        {
            Font = new Font("Arial", 8, FontStyle.Bold),
            Text = Text,
            BackColor = _BaseColor,
            ForeColor = _TextColor,
            MaxLength = _MaxLength,
            Multiline = _Multiline,
            ReadOnly = _ReadOnly,
            UseSystemPasswordChar = _UseSystemPasswordChar,
            BorderStyle = BorderStyle.None,
            Location = new Point(11, 5),
            Width = Width - 10
        };

        _Style = RoundingStyle.Normal;

        TB.Cursor = Cursors.IBeam;

        if (_Multiline)
        {
            TB.Height = Height - 11;
        }
        else
        {
            Height = TB.Height + 11;
        }

        TB.TextChanged += OnBaseTextChanged;
        TB.KeyDown += OnBaseKeyDown;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var B = new Bitmap(Width, Height))
        using (var G = Graphics.FromImage(B))
        {
            W = Width - 1;
            H = Height - 1;

            var Base = new Rectangle(0, 0, W, H);

            G.SmoothingMode = SmoothingMode.HighQuality;
            G.Clear(BackColor);

            TB.BackColor = _BaseColor;
            TB.ForeColor = _TextColor;

            switch (_Style)
            {
                case RoundingStyle.Normal:
                    G.FillPath(new SolidBrush(_BaseColor), Draw.CreateRound(Base, 5));
                    using (var tg = new LinearGradientBrush(Base, Color.FromArgb(186, 188, 191), Color.FromArgb(204, 205, 209), 90F))
                    {
                        G.DrawPath(new Pen(tg), Draw.CreateRound(Base, 5));
                    }
                    break;

                case RoundingStyle.Rounded:
                    G.DrawPath(new Pen(new SolidBrush(Color.FromArgb(132, 130, 128))), Draw.CreateRound(
                        new Rectangle(Base.X, Base.Y + 1, Base.Width, Base.Height - 1), 20));
                    G.FillPath(new SolidBrush(_BaseColor), Draw.CreateRound(
                        new Rectangle(Base.X, Base.Y, Base.Width, Base.Height - 1), 20));
                    using (var tg = new LinearGradientBrush(
                        new Rectangle(Base.X, Base.Y, Base.Width, Base.Height - 1),
                        Color.Black, Color.FromArgb(31, 28, 24), 90F))
                    {
                        G.DrawPath(new Pen(tg), Draw.CreateRound(
                            new Rectangle(Base.X, Base.Y, Base.Width, Base.Height - 1), 20));
                    }
                    break;
            }

            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBilinear;
            e.Graphics.DrawImageUnscaled(B, 0, 0);
        }
    }
}

[DefaultEvent("CheckedChanged")]
public class xVisualRadioButton : Control
{
    private Rectangle R1;
    private LinearGradientBrush G1;

    private MouseState State = MouseState.None;
    private bool _Checked;

    public event EventHandler CheckedChanged;

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

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 21;
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        if (!_Checked) Checked = true;
        base.OnClick(e);
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        InvalidateControls();
    }

    private void InvalidateControls()
    {
        if (!IsHandleCreated || !_Checked) return;

        foreach (Control c in Parent.Controls)
        {
            if (c != this && c is xVisualRadioButton radioButton)
            {
                radioButton.Checked = false;
            }
        }
    }

    public xVisualRadioButton()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Size = new Size(150, 21);
        DoubleBuffered = true;
    }

    private TextureBrush InnerTexture = NoiseBrush(new[]
    {
        Color.FromArgb(55, 52, 48),
        Color.FromArgb(57, 50, 50),
        Color.FromArgb(53, 50, 46)
    });

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var B = new Bitmap(Width, Height))
        using (var G = Graphics.FromImage(B))
        {
            var radioBtnRectangle = new Rectangle(0, 0, Height - 1, Height - 1);

            G.SmoothingMode = SmoothingMode.HighQuality;
            G.CompositingQuality = CompositingQuality.HighQuality;
            G.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            G.Clear(BackColor);

            G.FillRectangle(InnerTexture, radioBtnRectangle);
            G.DrawRectangle(Pens.Black, radioBtnRectangle);
            G.DrawRectangle(new Pen(Color.FromArgb(99, 97, 94)), new Rectangle(1, 1, Height - 3, Height - 3));

            if (Checked)
            {
                G.DrawString("a", new Font("Marlett", 12, FontStyle.Regular), Brushes.White, new Point(1, 2));
            }

            var drawFont = new Font("Arial", 10, FontStyle.Bold);
            using (var nb = new SolidBrush(Color.FromArgb(250, 250, 250)))
            {
                G.DrawString(Text, drawFont, nb, new Point(25, 10), new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                });
            }

            e.Graphics.DrawImage(B, 0, 0);
        }
    }

    private static TextureBrush NoiseBrush(Color[] colors)
    {
        var noiseBitmap = new Bitmap(1, 1);
        var rand = new Random();

        for (int x = 0; x < noiseBitmap.Width; x++)
        {
            for (int y = 0; y < noiseBitmap.Height; y++)
            {
                var color = colors[rand.Next(colors.Length)];
                noiseBitmap.SetPixel(x, y, color);
            }
        }

        return new TextureBrush(noiseBitmap);
    }
}

public class xVisualProgressBar : Control
{
    private int OFS = 0;
    private int Speed = 50;
    private int _Maximum = 100;

    public int Maximum
    {
        get => _Maximum;
        set
        {
            if (value < _Value)
            {
                _Value = value;
            }
            _Maximum = value;
            Invalidate();
        }
    }

    private int _Value = 0;
    public int Value
    {
        get => _Value == 0 ? 0 : _Value;
        set
        {
            if (value > _Maximum)
            {
                value = _Maximum;
            }
            _Value = value;
            Invalidate();
        }
    }

    private bool _ShowPercentage = false;
    public bool ShowPercentage
    {
        get => _ShowPercentage;
        set
        {
            _ShowPercentage = value;
            Invalidate();
        }
    }

    public xVisualProgressBar()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        Size = new Size(274, 30);
        InnerTexture = NoiseBrush(new[] { Color.FromArgb(55, 52, 48), Color.FromArgb(57, 50, 50), Color.FromArgb(53, 50, 46) });
    }

    private TextureBrush InnerTexture;

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var B = new Bitmap(Width, Height))
        using (var G = Graphics.FromImage(B))
        {
            int intValue = (int)(_Value / (float)_Maximum * Width);
            G.Clear(BackColor);

            // Fill background pattern
            G.FillRectangle(InnerTexture, new Rectangle(0, 0, Width - 1, Height - 1));

            // Draw gradient
            using (var br = new LinearGradientBrush(new Rectangle(0, 0, Width - 1, Height - 1),
                Color.White, Color.Black, LinearGradientMode.Vertical))
            {
                var blend = new ColorBlend
                {
                    Colors = new[] { Color.FromArgb(20, Color.White), Color.FromArgb(10, Color.Black), Color.FromArgb(10, Color.White) },
                    Positions = new[] { 0f, 0.8f, 1f }
                };
                br.InterpolationColors = blend;
                G.FillRectangle(br, new Rectangle(0, 0, Width - 1, Height - 1));
            }

            // Draw border
            G.DrawRectangle(Pens.Black, new Rectangle(0, 0, Width - 1, Height - 1));
            G.DrawLine(new Pen(Color.FromArgb(99, 97, 94)), 1, 1, Width - 3, 1);
            G.DrawLine(new Pen(Color.FromArgb(64, 60, 57)), 1, Height - 2, Width - 3, Height - 2);

            // Fill progress
            if (intValue > 0)
            {
                using (var progressBrush = new LinearGradientBrush(new Rectangle(2, 2, intValue - 3, Height - 4),
                    Color.FromArgb(114, 203, 232), Color.FromArgb(58, 118, 188), 90F))
                {
                    G.FillRectangle(progressBrush, new Rectangle(2, 2, intValue - 3, Height - 4));
                }
                G.DrawLine(new Pen(Color.FromArgb(235, 255, 255)), 2, 2, intValue - 2, 2);
            }

            // Draw percentage text
            if (_ShowPercentage)
            {
                using (var percentBrush = new SolidBrush(Color.White))
                {
                    G.DrawString($"{Value}%", new Font("Arial", 10, FontStyle.Bold), percentBrush,
                        new Rectangle(0, 0, Width - 1, Height - 1),
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                }
            }

            e.Graphics.DrawImage(B, 0, 0); // Используем B напрямую
        }
    }

    private TextureBrush NoiseBrush(Color[] colors)
    {
        Bitmap noiseBitmap = new Bitmap(Width, Height);
        Random rand = new Random();

        // Заполнение изображения случайными цветами
        for (int x = 0; x < noiseBitmap.Width; x++)
        {
            for (int y = 0; y < noiseBitmap.Height; y++)
            {
                // Выбор случайного цвета из переданного массива
                var color = colors[rand.Next(colors.Length)];
                noiseBitmap.SetPixel(x, y, color);
            }
        }

        // Создание текстуры на основе созданного битмапа
        return new TextureBrush(noiseBitmap);
    }


    // Implement Animate method for animation if required.
    public void Animate()
    {
        while (true)
        {
            OFS = (OFS < Width) ? OFS + 1 : 0;
            Invalidate();
            Thread.Sleep(Speed);
        }
    }
}

public class xVisualComboBox : ComboBox
{
    #region Control Help - Properties & Flicker Control
    private int _startIndex = 0;

    public int StartIndex
    {
        get => _startIndex;
        set
        {
            _startIndex = value;
            try
            {
                base.SelectedIndex = value;
            }
            catch
            {
                // Игнорируем ошибки
            }
            Invalidate();
        }
    }

    public override Rectangle DisplayRectangle => base.DisplayRectangle;

    private Color _highlightColor = Color.FromArgb(99, 97, 94);
    public Color ItemHighlightColor
    {
        get => _highlightColor;
        set
        {
            _highlightColor = value;
            Invalidate();
        }
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        e.DrawBackground();
        try
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                e.Graphics.FillRectangle(new SolidBrush(_highlightColor), e.Bounds);
                using (var gloss = new LinearGradientBrush(e.Bounds, Color.FromArgb(20, Color.White), Color.FromArgb(0, Color.White), 90F))
                {
                    e.Graphics.FillRectangle(gloss, new Rectangle(new Point(e.Bounds.X, e.Bounds.Y), new Size(e.Bounds.Width, e.Bounds.Height)));
                }
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(90, Color.Black)) { DashStyle = DashStyle.Solid }, new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1));
            }
            else
            {
                e.Graphics.FillRectangle(InnerTexture, e.Bounds);
            }

            using (var brush = new SolidBrush(Color.FromArgb(230, 230, 230)))
            {
                e.Graphics.DrawString(GetItemText(Items[e.Index]), e.Font, brush, new Rectangle(e.Bounds.X + 2, e.Bounds.Y, e.Bounds.Width - 4, e.Bounds.Height));
            }
        }
        catch { }
        e.DrawFocusRectangle();
    }

    protected void DrawTriangle(Color clr, Point firstPoint, Point secondPoint, Point thirdPoint, Graphics g)
    {
        var points = new List<Point> { firstPoint, secondPoint, thirdPoint };
        g.FillPolygon(new SolidBrush(clr), points.ToArray());
        g.DrawPolygon(Pens.Black, points.ToArray());
    }
    #endregion

    public xVisualComboBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        DrawMode = DrawMode.OwnerDrawFixed;
        BackColor = Color.Transparent;
        ForeColor = Color.Silver;
        Font = new Font("Arial", 9, FontStyle.Bold);
        DropDownStyle = ComboBoxStyle.DropDownList;
        DoubleBuffered = true;
        Size = new Size(Width + 1, 21);
        ItemHeight = 16;
    }

    private TextureBrush InnerTexture = NoiseBrush(new[]
    {
        Color.FromArgb(55, 52, 48),
        Color.FromArgb(57, 50, 50),
        Color.FromArgb(53, 50, 46)
    });

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var B = new Bitmap(Width, Height))
        using (var G = Graphics.FromImage(B))
        {
            G.SmoothingMode = SmoothingMode.HighQuality;

            G.Clear(BackColor);
            G.FillRectangle(InnerTexture, new Rectangle(0, 0, Width, Height - 1));
            G.DrawLine(new Pen(Color.FromArgb(99, 97, 94)), 1, 1, Width - 2, 1);
            G.DrawRectangle(new Pen(Color.FromArgb(99, 97, 94)), new Rectangle(1, 1, Width - 3, Height - 3));

            DrawTriangle(Color.FromArgb(99, 97, 94), new Point(Width - 14, 9), new Point(Width - 6, 9), new Point(Width - 10, 14), G);
            G.DrawRectangle(Pens.Black, new Rectangle(0, 0, Width - 1, Height - 1));

            // Draw Separator line
            G.DrawLine(new Pen(Color.FromArgb(99, 97, 94)), new Point(Width - 21, 1), new Point(Width - 21, Height - 3));
            G.DrawLine(Pens.Black, new Point(Width - 20, 2), new Point(Width - 20, Height - 3));
            G.DrawLine(new Pen(Color.FromArgb(99, 97, 94)), new Point(Width - 19, 1), new Point(Width - 19, Height - 3));

            var blend = new ColorBlend
            {
                Colors = new[]
                {
                    Color.FromArgb(15, Color.White),
                    Color.FromArgb(10, Color.Black),
                    Color.FromArgb(10, Color.White)
                },
                Positions = new[] { 0f, 0.75f, 1f }
            };

            using (var br = new LinearGradientBrush(new Rectangle(0, 0, Width, Height - 1), Color.White, Color.Black, LinearGradientMode.Vertical))
            {
                br.InterpolationColors = blend;

                // Fill the rect with the blend
                G.FillRectangle(br, new Rectangle(0, 0, Width, Height - 1));
            }

            try
            {
                G.DrawString(Text, Font, new SolidBrush(Color.FromArgb(250, 250, 250)), new Rectangle(5, 0, Width - 20, Height), new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Alignment = StringAlignment.Near
                });
            }
            catch
            {
                // Игнорируем ошибки
            }

            e.Graphics.DrawImage(B, 0, 0);
        }
    }

    private static TextureBrush NoiseBrush(Color[] colors)
    {
        var noiseBitmap = new Bitmap(1, 1);
        var rand = new Random();

        for (int x = 0; x < noiseBitmap.Width; x++)
        {
            for (int y = 0; y < noiseBitmap.Height; y++)
            {
                var color = colors[rand.Next(colors.Length)];
                noiseBitmap.SetPixel(x, y, color);
            }
        }

        return new TextureBrush(noiseBitmap);
    }
}

[DesignerCategory("Code")]
public class xVisualTabControl : TabControl
{
    private TextureBrush InnerTexture = NoiseBrush(new[]
    {
        Color.FromArgb(45, 41, 37),
        Color.FromArgb(47, 43, 39),
        Color.FromArgb(43, 39, 35)
    });

    private TextureBrush TabBGTexture = NoiseBrush(new[]
    {
        Color.FromArgb(55, 51, 48),
        Color.FromArgb(57, 53, 50),
        Color.FromArgb(53, 49, 46)
    });

    public xVisualTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        DoubleBuffered = true;
        SizeMode = TabSizeMode.Fixed;
        ItemSize = new Size(35, 122);
        Alignment = TabAlignment.Left;
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        Alignment = TabAlignment.Left;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate(); // Обновляем отрисовку при изменении размера
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e); // Вызов метода родительского класса
        using (var B = new Bitmap(Width, Height))
        using (var G = Graphics.FromImage(B))
        {
            var font = new Font("Arial", 9, FontStyle.Bold);

            // Проверяем наличие вкладок
            if (TabCount > 0)
            {
                try
                {
                    if (SelectedTab != null)
                    {
                        SelectedTab.BackColor = Color.FromArgb(56, 52, 49);
                    }
                }
                catch
                {
                    // Игнорируем ошибки
                }

                G.Clear(Parent.BackColor);

                // Фон вкладок
                var tabBgRect = new Rectangle(0, 0, ItemSize.Height + 3, Height - 1);
                G.FillRectangle(TabBGTexture, tabBgRect);
                G.DrawLine(GetPen(Color.FromArgb(44, 42, 39)), 1, Height - 3, ItemSize.Height + 3, Height - 3);

                // Проход по всем вкладкам
                for (int i = 0; i < TabCount; i++)
                {
                    var tabRect = GetTabRect(i);
                    if (i == SelectedIndex)
                    {
                        var selectedTabRect = new Rectangle(tabRect.X - 2, tabRect.Y - 2, tabRect.Width + 3, tabRect.Height - 1);
                        using (var tabOverlay = new LinearGradientBrush(selectedTabRect, Color.FromArgb(114, 203, 232), Color.FromArgb(58, 118, 188), 90F))
                        {
                            G.FillRectangle(tabOverlay, selectedTabRect);
                            G.DrawString(TabPages[i].Text, font, Brushes.White, selectedTabRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                        }
                    }
                    else
                    {
                        G.DrawString(TabPages[i].Text, font, Brushes.Gray, tabRect, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
                    }
                }
            }

            // Конечная отрисовка
            e.Graphics.DrawImage(B, 0, 0);
        }
    }

    private Pen GetPen(Color color)
    {
        return new Pen(color);
    }

    private Brush GetBrush(Color color)
    {
        return new SolidBrush(color);
    }

    private static TextureBrush NoiseBrush(Color[] colors)
    {
        var noiseBitmap = new Bitmap(1, 1);
        var rand = new Random();

        // Генерация случайного цвета для текстуры
        for (int x = 0; x < noiseBitmap.Width; x++)
        {
            for (int y = 0; y < noiseBitmap.Height; y++)
            {
                var color = colors[rand.Next(colors.Length)];
                noiseBitmap.SetPixel(x, y, color);
            }
        }

        return new TextureBrush(noiseBitmap) { WrapMode = WrapMode.Tile };
    }
}

[DefaultEvent("CheckedChanged")]
public class xVisualCheckBox : Control
{
    #region Control Help - MouseState & Flicker Control
    private enum MouseState
    {
        None,
        Over,
        Down
    }

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

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        Invalidate();
    }

    private bool _checked;
    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            Invalidate();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Height = 21;
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        CheckedChanged?.Invoke(this, EventArgs.Empty); // Исправлено
        base.OnClick(e);
    }

    public event EventHandler CheckedChanged;
    #endregion

    public xVisualCheckBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.Transparent;
        ForeColor = Color.Black;
        Size = new Size(250, 21);
        DoubleBuffered = true;
    }

    private TextureBrush innerTexture = CreateNoiseBrush(new[]
    {
        Color.FromArgb(55, 52, 48),
        Color.FromArgb(57, 50, 50),
        Color.FromArgb(53, 50, 46)
    });

    protected override void OnPaint(PaintEventArgs e)
    {
        using (var bitmap = new Bitmap(Width, Height))
        using (var graphics = Graphics.FromImage(bitmap))
        {
            var checkBoxRectangle = new Rectangle(0, 0, Height - 1, Height - 1);

            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;
            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            graphics.Clear(BackColor);
            graphics.FillRectangle(innerTexture, checkBoxRectangle);
            graphics.DrawRectangle(Pens.Black, checkBoxRectangle);
            graphics.DrawRectangle(new Pen(Color.FromArgb(99, 97, 94)), new Rectangle(1, 1, Height - 3, Height - 3));

            if (Checked)
            {
                graphics.DrawString("a", new Font("Marlett", 12, FontStyle.Regular), Brushes.White, new Point(1, 2));
            }

            var drawFont = new Font("Arial", 10, FontStyle.Bold);
            var textBrush = new SolidBrush(Color.FromArgb(250, 250, 250));
            graphics.DrawString(Text, drawFont, textBrush, new Point(25, 10), new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center
            });

            e.Graphics.DrawImage(bitmap, 0, 0);
        }
    }

    private static TextureBrush CreateNoiseBrush(Color[] colors)
    {
        var noiseBitmap = new Bitmap(1, 1);
        noiseBitmap.SetPixel(0, 0, colors[0]);
        return new TextureBrush(noiseBitmap);
    }
}
