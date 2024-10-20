using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

public static class Helper
{
    public static GraphicsPath RoundRect(Rectangle R, int radius)
    {
        var GP = new GraphicsPath();
        GP.AddArc(R.X, R.Y, radius, radius, 180, 90);
        GP.AddArc(R.Right - radius, R.Y, radius, radius, 270, 90);
        GP.AddArc(R.Right - radius, R.Bottom - radius, radius, radius, 0, 90);
        GP.AddArc(R.X, R.Bottom - radius, radius, radius, 90, 90);
        GP.CloseFigure();
        return GP;
    }
}

public class ContainerObjectDisposable : IDisposable
{
    private List<IDisposable> iList = new List<IDisposable>();
    private bool disposedValue;

    public void AddObject(IDisposable obj)
    {
        iList.Add(obj);
    }

    public void AddObjectRange(IDisposable[] objs)
    {
        iList.AddRange(objs);
    }

    #region "IDisposable Support"
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                foreach (var objectDisposable in iList)
                {
                    objectDisposable.Dispose();
#if DEBUG
                    System.Diagnostics.Debug.WriteLine("Dispose : " + objectDisposable.ToString());
#endif
                }
            }

            iList.Clear();
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    #endregion
}

public class BitdefenderForm : ContainerControl
{
    private Graphics G;
    private new const int Padding = 10;
    private Size btnSize = new Size(27, 30);
    private Size controlSize = new Size(86, 30);
    private GraphicsPath controlboxPath;

    private Rectangle R1, R2, R4, R5, R6;
    private GraphicsPath GP1, GP2, GP3, GP4;
    private Pen P1, P2, P3, P4, P5, P6;
    private SolidBrush B1;
    private LinearGradientBrush LGB1, LGB2, LGB3, LGB4, LGB5, LGB6;

    private bool _DisableMax;
    public bool DisableControlboxMax
    {
        get => _DisableMax;
        set
        {
            _DisableMax = value;
            Invalidate(false);
        }
    }

    private bool _DisableMin;
    public bool DisableControlboxMin
    {
        get => _DisableMin;
        set
        {
            _DisableMin = value;
            Invalidate(false);
        }
    }

    private bool _DisableClose;
    public bool DisableControlboxClose
    {
        get => _DisableClose;
        set
        {
            _DisableClose = value;
            Invalidate(false);
        }
    }

    private enum State
    {
        None,
        HoverClose,
        HoverMax,
        HoverMin
    }

    private State _MouseState;
    private State MouseState
    {
        get => _MouseState;
        set
        {
            _MouseState = value;
            Invalidate(false);
        }
    }

    private Point mouseOffset;

    public BitdefenderForm()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.DoubleBuffer |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor, true);
        DoubleBuffered = true;
        Dock = DockStyle.Fill;
        this.BackColor = Color.Black;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (FindForm() != null)
        {
            FindForm().FormBorderStyle = FormBorderStyle.None;
            // Установите прозрачный цвет, если это необходимо
            FindForm().TransparencyKey = this.BackColor;
        }
    }

    public override string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            Invalidate(new Rectangle(0, 0, Width, 70), false);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        mouseOffset = new Point(-e.X, -e.Y);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        // Move form
        if (e.Button == MouseButtons.Left)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    if (e.Location.Equals(new Point(x, y)))
                    {
                        var mousePos = Control.MousePosition;
                        mousePos.Offset(mouseOffset.X, mouseOffset.Y);
                        FindForm().Location = mousePos;
                    }
                }
            }
        }

        // Handle cursor state
        MouseState = State.None;
        Cursor = Cursors.Default;

        // Minimize
        Rectangle minRect = new Rectangle(Right - Padding - (controlSize.Width + 20), Padding, btnSize.Width, btnSize.Height);
        if (minRect.Contains(e.Location))
        {
            Cursor = Cursors.Hand;
            MouseState = State.HoverMin;
        }

        // Maximize
        Rectangle maxRect = new Rectangle(minRect.X + 29, minRect.Y, btnSize.Width, btnSize.Height);
        if (maxRect.Contains(e.Location))
        {
            Cursor = Cursors.Hand;
            MouseState = State.HoverMax;
        }

        // Close
        Rectangle closeRect = new Rectangle(maxRect.X + 29, minRect.Y, btnSize.Width, btnSize.Height);
        if (closeRect.Contains(e.Location))
        {
            Cursor = Cursors.Hand;
            MouseState = State.HoverClose;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        Rectangle min = new Rectangle(Right - Padding - (controlSize.Width + 20), Padding, btnSize.Width, btnSize.Height);
        Rectangle max = new Rectangle(min.X + 29, min.Y, btnSize.Width, btnSize.Height);
        Rectangle close = new Rectangle(max.X + 29, max.Y, btnSize.Width, btnSize.Height);

        if (min.Contains(e.Location) && e.Button == MouseButtons.Left && !DisableControlboxMin)
        {
            FindForm().WindowState = FormWindowState.Minimized;
        }

        if (max.Contains(e.Location) && e.Button == MouseButtons.Left && !DisableControlboxMax)
        {
            if (FindForm().WindowState == FormWindowState.Maximized)
                FindForm().WindowState = FormWindowState.Normal;
            else
                FindForm().WindowState = FormWindowState.Maximized;
        }

        if (close.Contains(e.Location) && e.Button == MouseButtons.Left && !DisableControlboxClose)
        {
            FindForm().Close();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Init(e);

        // Включаем сглаживание
        G.SmoothingMode = SmoothingMode.AntiAlias;

        // Заполняем форму основным цветом
        G.FillPath(B1, GP1);
        G.SetClip(GP2);
        G.FillRectangle(LGB1, R4);
        G.ResetClip();

        // Изменение положения текста
        G.DrawString(Text, new Font("Microsoft Sans Serif", 10, FontStyle.Regular), Brushes.White, new Point(10, 12)); // Смещаем текст вниз
        G.DrawPath(P1, GP1);
        G.DrawPath(P2, GP2);

        switch (MouseState)
        {
            case State.HoverClose:
                DrawControlBox_Close(G);
                break;
            case State.HoverMax:
                DrawControlBox_Max(G);
                break;
            case State.HoverMin:
                DrawControlBox_Min(G);
                break;
            case State.None:
                DrawControlBox(G);
                break;
        }
    }

    private void Init(PaintEventArgs e)
    {
        G = e.Graphics;

        // Полное заполнение формы без отступов
        R1 = new Rectangle(0, 0, Width, Height);
        R2 = new Rectangle(0, 0, Width, Height); // Убедитесь, что это тоже полный размер
        R4 = new Rectangle(0, 0, Width, 20);
        R5 = new Rectangle(Right - Padding - (controlSize.Width + 20), Padding, controlSize.Width, controlSize.Height);
        R6 = new Rectangle(R5.X + 1, R5.Y + 1, R5.Width - 2, R5.Height - 2);

        GP1 = RoundRect(R1, 18);
        GP2 = RoundRect(R2, 18);
        GP3 = ControlRect(R5, 9);
        GP4 = ControlRect(R6, 9);
        controlboxPath = new GraphicsPath();

        // Инициализация кистей
        B1 = new SolidBrush(Color.FromArgb(32, 32, 32));
        LGB1 = new LinearGradientBrush(R4, Color.FromArgb(81, 81, 81), Color.FromArgb(43, 43, 43), LinearGradientMode.Vertical);

        // Инициализация перьев
        P1 = new Pen(Color.Black, 2);
        P2 = new Pen(Color.Gray, 1);
        P3 = new Pen(Color.Black, 1);
        P4 = new Pen(Color.Gray, 1);
        P5 = new Pen(Color.Black, 1);
        P6 = new Pen(Color.Black, 1);

        // Инициализация градиентов
        LGB2 = new LinearGradientBrush(R5, Color.FromArgb(83, 83, 83), Color.FromArgb(41, 41, 41), LinearGradientMode.Vertical);
        LGB3 = new LinearGradientBrush(R5, Color.FromArgb(33, 33, 33), Color.FromArgb(41, 41, 41), LinearGradientMode.Vertical);
        LGB4 = new LinearGradientBrush(new Point(R5.Left + 27, R5.Top + 2), new Point(R5.Left + 27, R5.Bottom - 2), Color.FromArgb(21, 21, 21), Color.FromArgb(10, 10, 10));
        LGB5 = new LinearGradientBrush(new Point(R5.Left + 28, R5.Top + 2), new Point(R5.Left + 28, R5.Bottom - 1), Color.FromArgb(167, 167, 167), Color.FromArgb(83, 83, 83));
        LGB6 = new LinearGradientBrush(R5, Color.FromArgb(41, 41, 41), Color.FromArgb(77, 77, 77), LinearGradientMode.Vertical);
    }

    private void DrawControlBox(Graphics G)
    {
        G.SmoothingMode = SmoothingMode.HighQuality;

        G.FillPath(LGB2, GP3);
        G.DrawPath(P3, GP3);
        G.DrawPath(P4, GP4);

        // White line
        G.DrawLine(P2, R5.Left, R5.Top + 1, R5.Right, R5.Top + 1);

        // Important fix
        G.DrawLine(P3, R5.Right, R5.Top, R5.Right, R5.Bottom - 4);
        G.DrawLine(P4, R6.Right, R6.Top + 1, R6.Right, R6.Bottom - 4);

        G.SmoothingMode = SmoothingMode.Default;
        G.FillRectangle(P3.Brush, new Rectangle(R5.X, R5.Y + 1, 1, 1));
        G.FillRectangle(P3.Brush, new Rectangle(R5.Right, R5.Top + 1, 1, 1));

        // First lines
        G.DrawLine(P5, R5.Left + 29, R5.Top + 2, R5.Left + 29, R5.Bottom - 2);
        G.DrawLine(P6, R5.Left + 30, R5.Top + 2, R5.Left + 30, R5.Bottom - 2);

        // Second lines
        G.DrawLine(P5, R5.Left + 56, R5.Top + 2, R5.Left + 56, R5.Bottom - 2);
        G.DrawLine(P6, R5.Left + 57, R5.Top + 2, R5.Left + 57, R5.Bottom - 2);

        // Close string
        controlboxPath.AddString("r", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width + 20) + 2, Padding + 8), null);
        G.FillPath(Brushes.White, controlboxPath);
        G.DrawPath(Pens.Black, controlboxPath);

        // Max string
        switch (FindForm().WindowState)
        {
            case FormWindowState.Maximized:
                controlboxPath.AddString("2", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 4, Padding + 8), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
            case FormWindowState.Normal:
                controlboxPath.AddString("1", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 2, Padding + 8), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
        }

        // Min string
        controlboxPath.AddString("0", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 3 + 20) + 2, Padding + 8), null);
        G.DrawPath(Pens.Black, controlboxPath);
        G.FillPath(Brushes.White, controlboxPath);
    }
    private void DrawControlBox_Close(Graphics G)
    {
        G.SmoothingMode = SmoothingMode.HighQuality;

        G.FillPath(LGB2, GP3);

        G.SetClip(new Rectangle(Right - Padding - (btnSize.Width + 23) + 1, Padding, btnSize.Width + 3, btnSize.Height));
        G.FillPath(LGB6, GP3);
        G.ResetClip();

        G.DrawPath(P3, GP3);
        G.DrawPath(P4, GP4);

        // White line
        G.DrawLine(P2, R5.Left, R5.Top + 1, R5.Right, R5.Top + 1);

        // Important fix
        G.DrawLine(P3, R5.Right, R5.Top, R5.Right, R5.Bottom - 4);
        G.DrawLine(P4, R6.Right, R6.Top + 1, R6.Right, R6.Bottom - 4);

        G.SmoothingMode = SmoothingMode.Default;

        G.FillRectangle(P3.Brush, new Rectangle(R5.X, R5.Y + 1, 1, 1));
        G.FillRectangle(P3.Brush, new Rectangle(R5.Right, R5.Top + 1, 1, 1));

        // First lines
        G.DrawLine(P5, R5.Left + 29, R5.Top + 2, R5.Left + 29, R5.Bottom - 2);
        G.DrawLine(P6, R5.Left + 30, R5.Top + 2, R5.Left + 30, R5.Bottom - 2);

        // Second lines
        G.DrawLine(P5, R5.Left + 56, R5.Top + 2, R5.Left + 56, R5.Bottom - 2);

        // Close string
        controlboxPath.AddString("r", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width + 20) + 2, Padding + 10), null);
        G.FillPath(Brushes.White, controlboxPath);
        G.DrawPath(Pens.Black, controlboxPath);

        // Max string
        switch (FindForm().WindowState)
        {
            case FormWindowState.Maximized:
                controlboxPath.AddString("2", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 4, Padding + 8), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
            case FormWindowState.Normal:
                controlboxPath.AddString("1", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 2, Padding + 8), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
        }

        // Min string
        controlboxPath.AddString("0", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 3 + 20) + 2, Padding + 10), null);
        G.DrawPath(Pens.Black, controlboxPath);
        G.FillPath(Brushes.White, controlboxPath);
    }
    private void DrawControlBox_Max(Graphics G)
    {
        G.SmoothingMode = SmoothingMode.HighQuality;

        G.FillPath(LGB2, GP3);

        G.SetClip(new Rectangle(Right - Padding - (btnSize.Width * 2 + 23) + 1, Padding, btnSize.Width, btnSize.Height));
        G.FillPath(LGB6, GP3);
        G.ResetClip();

        G.DrawPath(P3, GP3);
        G.DrawPath(P4, GP4);

        // White line
        G.DrawLine(P2, R5.Left, R5.Top + 1, R5.Right, R5.Top + 1);

        // Important fix
        G.DrawLine(P3, R5.Right, R5.Top, R5.Right, R5.Bottom - 4);
        G.DrawLine(P4, R6.Right, R6.Top + 1, R6.Right, R6.Bottom - 4);

        G.SmoothingMode = SmoothingMode.Default;

        G.FillRectangle(P3.Brush, new Rectangle(R5.X, R5.Y + 1, 1, 1));
        G.FillRectangle(P3.Brush, new Rectangle(R5.Right, R5.Top + 1, 1, 1));

        // First line
        G.DrawLine(P5, R5.Left + 29, R5.Top + 2, R5.Left + 29, R5.Bottom - 2);

        // Second line
        G.DrawLine(P5, R5.Left + 56, R5.Top + 2, R5.Left + 56, R5.Bottom - 2);
        G.DrawLine(P6, R5.Left + 57, R5.Top + 2, R5.Left + 57, R5.Bottom - 2);

        // Close string
        controlboxPath.AddString("r", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width + 20) + 2, Padding + 8), null);
        G.FillPath(Brushes.White, controlboxPath);
        G.DrawPath(Pens.Black, controlboxPath);

        // Max string
        switch (FindForm().WindowState)
        {
            case FormWindowState.Maximized:
                controlboxPath.AddString("2", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 4, Padding + 10), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
            case FormWindowState.Normal:
                controlboxPath.AddString("1", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 2, Padding + 10), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
        }

        // Min string
        controlboxPath.AddString("0", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 3 + 20) + 2, Padding + 8), null);
        G.DrawPath(Pens.Black, controlboxPath);
        G.FillPath(Brushes.White, controlboxPath);
    }
    private void DrawControlBox_Min(Graphics G)
    {
        G.SmoothingMode = SmoothingMode.HighQuality;

        G.FillPath(LGB2, GP3);

        G.SetClip(new Rectangle(Right - Padding - (controlSize.Width + 20) + 1, Padding, btnSize.Width + 3, btnSize.Height));
        G.FillPath(LGB6, GP3);
        G.ResetClip();

        G.DrawPath(P3, GP3);
        G.DrawPath(P4, GP4);

        // White line
        G.DrawLine(P2, R5.Left, R5.Top + 1, R5.Right, R5.Top + 1);

        // Important fix
        G.DrawLine(P3, R5.Right, R5.Top, R5.Right, R5.Bottom - 4);
        G.DrawLine(P4, R6.Right, R6.Top + 1, R6.Right, R6.Bottom - 4);

        G.SmoothingMode = SmoothingMode.Default;

        G.FillRectangle(P3.Brush, new Rectangle(R5.X, R5.Y + 1, 1, 1));
        G.FillRectangle(P3.Brush, new Rectangle(R5.Right, R5.Top + 1, 1, 1));

        // First lines
        G.DrawLine(P5, R5.Left + 29, R5.Top + 2, R5.Left + 29, R5.Bottom - 2);
        G.DrawLine(P6, R5.Left + 30, R5.Top + 2, R5.Left + 30, R5.Bottom - 2);

        // Second lines
        G.DrawLine(P5, R5.Left + 56, R5.Top + 2, R5.Left + 56, R5.Bottom - 2);
        G.DrawLine(P6, R5.Left + 57, R5.Top + 2, R5.Left + 57, R5.Bottom - 2);

        // Close string
        controlboxPath.AddString("r", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width + 20) + 2, Padding + 8), null);
        G.FillPath(Brushes.White, controlboxPath);
        G.DrawPath(Pens.Black, controlboxPath);

        // Max string
        switch (FindForm().WindowState)
        {
            case FormWindowState.Maximized:
                controlboxPath.AddString("2", new FontFamily("Webdings"), (int)FontStyle.Regular, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 4, Padding + 8), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
            case FormWindowState.Normal:
                controlboxPath.AddString("1", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 2 + 20) + 2, Padding + 8), null);
                G.DrawPath(Pens.Black, controlboxPath);
                G.FillPath(Brushes.White, controlboxPath);
                break;
        }

        // Min string
        controlboxPath.AddString("0", new FontFamily("Webdings"), (int)FontStyle.Bold, 15, new Point(Right - Padding - (btnSize.Width * 3 + 20) + 2, Padding + 10), null);
        G.DrawPath(Pens.Black, controlboxPath);
        G.FillPath(Brushes.White, controlboxPath);
    }

    private GraphicsPath RoundRect(Rectangle rectangle, int radius)
    {
        GraphicsPath path = new GraphicsPath();

        // Убедитесь, что радиус не превышает половину ширины или высоты
        int adjustedRadius = Math.Min(radius, Math.Min(rectangle.Width, rectangle.Height) / 2);

        path.AddArc(rectangle.X, rectangle.Y, adjustedRadius, adjustedRadius, 180, 90); // Верхний левый угол
        path.AddArc(rectangle.Right - adjustedRadius, rectangle.Y, adjustedRadius, adjustedRadius, 270, 90); // Верхний правый угол
        path.AddArc(rectangle.Right - adjustedRadius, rectangle.Bottom - adjustedRadius, adjustedRadius, adjustedRadius, 0, 90); // Нижний правый угол
        path.AddArc(rectangle.X, rectangle.Bottom - adjustedRadius, adjustedRadius, adjustedRadius, 90, 90); // Нижний левый угол
        path.CloseFigure(); // Закрыть путь

        return path;
    }

    private GraphicsPath ControlRect(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.AddLine(rect.Left, rect.Top, rect.Right, rect.Top);
        return path;
    }
}

public class BitdefenderSeparator : Control
{
    private Graphics G;
    private LinearGradientBrush LGB1, LGB2;
    private Pen P1, P2;
    private ColorBlend CB1, CB2;
    private Color C1, C2;
    private ContainerObjectDisposable conObj = new ContainerObjectDisposable();

    public BitdefenderSeparator()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.DoubleBuffer |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint, true);

        Width = 400;
        Height = 3;
        BackColor = Color.Transparent;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        List<Color> Colors1 = new List<Color>();
        List<Color> Colors2 = new List<Color>();

        C1 = Color.FromArgb(62, 62, 62);
        C2 = Color.FromArgb(1, 1, 1);

        G = e.Graphics;

        LGB1 = new LinearGradientBrush(new Point(0, 0), new Point(Width, 0), Color.Empty, Color.Empty);
        LGB2 = new LinearGradientBrush(new Point(0, 1), new Point(Width, 1), Color.Empty, Color.Empty);
        conObj.AddObjectRange([LGB1, LGB2]);

        CB1 = new ColorBlend();
        CB2 = new ColorBlend();

        // Colors for first line
        for (int i = 0; i <= 255; i += 51)
        {
            Colors1.Add(Color.FromArgb(i, C1));
        }
        for (int i = 255; i >= 0; i -= 51)
        {
            Colors1.Add(Color.FromArgb(i, C1));
        }

        // Colors for second line
        for (int i = 0; i <= 255; i += 51)
        {
            Colors2.Add(Color.FromArgb(i, C2));
        }
        for (int i = 255; i >= 0; i -= 51)
        {
            Colors2.Add(Color.FromArgb(i, C2));
        }

        // ColorBlend for first line
        CB1.Colors = Colors1.ToArray();
        CB1.Positions = [0.0f, 0.08f, 0.16f, 0.24f, 0.32f, 0.4f, 0.48f, 0.56f, 0.64f, 0.72f, 0.8f, 1.0f];

        // ColorBlend for second line
        CB2.Colors = Colors2.ToArray();
        CB2.Positions = [0.0f, 0.08f, 0.16f, 0.24f, 0.32f, 0.4f, 0.48f, 0.56f, 0.64f, 0.72f, 0.8f, 1.0f];

        // Setting Pen
        LGB1.InterpolationColors = CB1;
        LGB2.InterpolationColors = CB2;

        P1 = new Pen(LGB1);
        P2 = new Pen(LGB2);
        conObj.AddObjectRange([P1, P2]);

        G.DrawLine(P1, 0, 0, Width, 0);
        G.DrawLine(P2, 0, 1, Width, 1);

        conObj.Dispose();
    }
}

[DefaultEvent("ChangeChecked")]
public class BitdefenderCheckbox : Control
{
    // Event
    public event EventHandler<bool> ChangeChecked;
    private bool _check;

    public bool Checked
    {
        get => _check;
        set
        {
            _check = value;
            Invalidate();
            ChangeChecked?.Invoke(this, value);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Checked = !Checked;
    }

    public BitdefenderCheckbox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint, true);
        Width = 55;
        Height = 25;
        BackColor = Color.Transparent;
        oldsize = new Size(55, 25);
    }

    // Draw
    private ContainerObjectDisposable cn;
    private Rectangle R1, R2, R3, R4, R5, R6;
    private GraphicsPath GP1, GP2, GP3, GP4, GP5, GP6;
    private LinearGradientBrush LGB1, LGB2, LGB3, LGB4;
    private PathGradientBrush PGB1;
    private SolidBrush B1, B2, B3;
    private Graphics G;
    private Pen P1, P2;
    private Size CheckSize;
    private bool _hover;

    private bool Hover
    {
        get => _hover;
        set
        {
            _hover = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Init(e);
        G.SmoothingMode = SmoothingMode.AntiAlias;
        G.InterpolationMode = InterpolationMode.HighQualityBicubic;
        G.FillPath(B1, GP1);

        if (Checked)
        {
            G.FillPath(B2, GP2);
            G.FillPath(PGB1, GP2);
            G.DrawPath(Pens.Black, GP2);
            DrawOnState();
        }
        else
        {
            G.FillPath(B1, GP1);
            G.FillPath(B2, GP2);
            G.FillPath(PGB1, GP2);
            G.DrawPath(Pens.Black, GP2);
            DrawOffState();
        }

        cn.Dispose();
    }

    private void DrawOnState()
    {
        G.FillPath(Hover ? LGB1 : LGB1, GP3);
        G.DrawPath(Pens.Black, GP3);
        G.DrawPath(P1, GP4);
        G.DrawString("ON", new Font("Microsoft Sans Serif", 7, FontStyle.Bold), Brushes.Black, 7, 6);
        G.DrawString("ON", new Font("Microsoft Sans Serif", 7, FontStyle.Bold), Brushes.White, 7, 7);
    }

    private void DrawOffState()
    {
        G.FillPath(Hover ? LGB3 : LGB3, GP5);
        G.DrawPath(Pens.Black, GP5);
        G.DrawPath(P2, GP6);
        G.DrawString("OFF", new Font("Microsoft Sans Serif", 7, FontStyle.Bold), Brushes.Black, Width - 29, 6);
        G.DrawString("OFF", new Font("Microsoft Sans Serif", 7, FontStyle.Bold), B3, Width - 29, 7);
    }

    private void Init(PaintEventArgs e)
    {
        G = e.Graphics;
        cn = new ContainerObjectDisposable();
        R1 = new Rectangle(1, 1, Width - 2, Height - 2);
        R2 = new Rectangle(2, 2, Width - 4, Height - 4);

        GP1 = RoundRect(R1, 11);
        cn.AddObject(GP1);
        GP2 = RoundRect(R2, 11);
        cn.AddObject(GP2);

        B1 = new SolidBrush(Color.FromArgb(40, 40, 40));
        cn.AddObject(B1);
        if (Checked)
        {
            B2 = new SolidBrush(Color.FromArgb(84, 135, 171));
            PGB1 = new PathGradientBrush(GP2)
            {
                CenterColor = Color.FromArgb(84, 135, 171),
                SurroundColors = [Color.FromArgb(113, 176, 200)],
                FocusScales = new PointF(0.85f, 0.65f)
            };
        }
        else
        {
            B2 = new SolidBrush(Color.FromArgb(29, 29, 29));
            PGB1 = new PathGradientBrush(GP2)
            {
                CenterColor = Color.FromArgb(29, 29, 29),
                SurroundColors = [Color.FromArgb(39, 39, 39)],
                FocusScales = new PointF(0.85f, 0.65f)
            };
        }
        cn.AddObjectRange([B2, PGB1]);
        B3 = new SolidBrush(Color.FromArgb(107, 107, 107));
        cn.AddObject(B3);

        CheckSize = new Size(22, R2.Height);
        R3 = new Rectangle(Width - 2 - CheckSize.Width, 2, CheckSize.Width, CheckSize.Height);
        GP3 = RoundRect(R3, 11);
        R4 = new Rectangle(R3.X + 1, R3.Y + 1, R3.Width - 2, R3.Height - 2);
        GP4 = RoundRect(R4, 11);

        R5 = new Rectangle(0, 2, CheckSize.Width, CheckSize.Height);
        GP5 = RoundRect(R5, 11);
        R6 = new Rectangle(R5.X + 1, R5.Y + 1, R5.Width - 2, R5.Height - 2);
        GP6 = RoundRect(R6, 11);
        cn.AddObjectRange([GP3, GP4, GP5, GP6]);

        InitializeGradients();
    }

    private void InitializeGradients()
    {
        if (Hover)
        {
            LGB1 = new LinearGradientBrush(R3, Color.FromArgb(86, 86, 86), Color.FromArgb(42, 42, 42), LinearGradientMode.Vertical);
            LGB2 = new LinearGradientBrush(new Rectangle(R4.X - 1, R4.Y - 1, R4.Width + 2, R4.Height + 2), Color.FromArgb(147, 147, 147), Color.FromArgb(68, 68, 68), LinearGradientMode.Vertical);
            P1 = new Pen(LGB2);

            LGB3 = new LinearGradientBrush(R5, Color.FromArgb(86, 86, 86), Color.FromArgb(42, 42, 42), LinearGradientMode.Vertical);
            LGB4 = new LinearGradientBrush(new Rectangle(R6.X - 1, R6.Y - 1, R6.Width + 2, R6.Height + 2), Color.FromArgb(147, 147, 147), Color.FromArgb(68, 68, 68), LinearGradientMode.Vertical);
            P2 = new Pen(LGB4);
        }
        else
        {
            LGB1 = new LinearGradientBrush(R3, Color.FromArgb(59, 59, 59), Color.FromArgb(29, 29, 29), LinearGradientMode.Vertical);
            LGB2 = new LinearGradientBrush(new Rectangle(R4.X - 1, R4.Y - 1, R4.Width + 2, R4.Height + 2), Color.FromArgb(101, 101, 101), Color.FromArgb(42, 42, 42), LinearGradientMode.Vertical);
            P1 = new Pen(LGB2);

            LGB3 = new LinearGradientBrush(R5, Color.FromArgb(59, 59, 59), Color.FromArgb(29, 29, 29), LinearGradientMode.Vertical);
            LGB4 = new LinearGradientBrush(new Rectangle(R6.X - 1, R6.Y - 1, R6.Width + 2, R6.Height + 2), Color.FromArgb(101, 101, 101), Color.FromArgb(42, 42, 42), LinearGradientMode.Vertical);
            P2 = new Pen(LGB4);
        }

        cn.AddObjectRange([LGB1, LGB2, LGB3, LGB4, P1, P2]);
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        Hover = true;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Hover = false;
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);
        Size = oldsize;
    }

    private Size oldsize;

    private GraphicsPath RoundRect(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}

[DefaultEvent("Click")]
public class BitdefenderButton : Control
{
    private bool _down;
    private Thread _openThread;

    public BitdefenderButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint, true);
        Width = 100;
        Height = 55;
        BackColor = Color.Transparent;
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

    private bool Down
    {
        get => _down;
        set
        {
            _down = value;
            Invalidate();
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Down = true;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Down = false;
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _openThread = new Thread(EnterAnimation);
        if (!_openThread.IsAlive)
        {
            _openThread.IsBackground = true;
            _openThread.Start();
        }
    }

    private void EnterAnimation()
    {
        using (Graphics g = CreateGraphics())
        {
            Rectangle r2 = new Rectangle(5, 5, Width - 10, Height - 10);
            GraphicsPath gp2 = RoundRect(r2, 11);
            g.SetClip(gp2);
            for (int fade = 0; fade <= 5; fade += 1)
            {
                Thread.Sleep(50);
                g.FillRectangle(new SolidBrush(Color.FromArgb(fade * 51, Color.White)), ClientRectangle);
            }
        }
    }

    // Draw
    private Color C1, C3, C4, C5, C6;
    private Rectangle R1, R2, R3;
    private GraphicsPath GP1, GP2, GP3;
    private SolidBrush B1, B2;
    private Pen P1, P2;
    private LinearGradientBrush LGB1, LGB2;
    private StringFormat SF1;

    private void Init(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        R1 = new Rectangle(3, 3, Width - 6, Height - 6);
        R2 = new Rectangle(5, 5, Width - 10, Height - 10);
        R3 = new Rectangle(6, 6, Width - 12, Height - 12);

        GP1 = RoundRect(R1, 11);
        GP2 = RoundRect(R2, 11);
        GP3 = RoundRect(R3, 11);

        C1 = Color.FromArgb(100, 41, 41, 41);
        C3 = Color.FromArgb(101, 101, 101);
        C4 = Color.FromArgb(60, 60, 60);
        C5 = Color.FromArgb(28, 28, 28);
        C6 = Color.FromArgb(45, 45, 45);

        B1 = new SolidBrush(C1);
        B2 = (SolidBrush)Brushes.White;
        LGB1 = new LinearGradientBrush(R2, C4, C5, LinearGradientMode.Vertical);
        LGB2 = new LinearGradientBrush(R3, C3, C6, LinearGradientMode.Vertical);

        P1 = new Pen(Brushes.Black);
        P2 = new Pen(LGB2);

        SF1 = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.Character
        };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Init(e);
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;

        e.Graphics.FillPath(B1, GP1);
        e.Graphics.FillPath(LGB1, GP2);
        e.Graphics.DrawPath(P1, GP2);
        e.Graphics.DrawPath(P2, GP3);

        if (!Down)
        {
            e.Graphics.DrawString(Text, Font, B2, R3, SF1);
        }
        else
        {
            R3.X += 1;
            R3.Y += 1;
            e.Graphics.DrawString(Text, Font, B2, R3, SF1);
        }
    }

    private GraphicsPath RoundRect(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}

[Browsable(false)]
public class BitdefenderGroupbox : ContainerControl
{
    private string _title = "Title";
    private string _subtitle = "Subtitle";

    public BitdefenderGroupbox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint, true);
        Width = 100;
        Height = 55;
        BackColor = Color.Transparent;
    }

    public override string Text
    {
        get => base.Text;
        set => base.Text = value; // Base text property is overridden without custom behavior
    }

    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            Invalidate(false);
        }
    }

    public string Subtitle
    {
        get => _subtitle;
        set
        {
            _subtitle = value;
            Invalidate(false);
        }
    }

    private Rectangle R1, R2;
    private GraphicsPath GP1, GP2;
    private Pen P1, P2;
    private SolidBrush B1, B2;
    private Size SZ1, SZ2;
    private Font F1, F2;
    private string S1, S2;

    private void Init(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        R1 = new Rectangle(3, 3, Width - 6, Height - 6);
        R2 = new Rectangle(4, 4, Width - 8, Height - 8);

        GP1 = RoundRect(R1, 11);
        GP2 = RoundRect(R2, 11);

        P1 = Pens.Black;
        P2 = new Pen(Color.FromArgb(68, 68, 68));

        B1 = new SolidBrush(Color.FromArgb(41, 41, 41));
        B2 = (SolidBrush)Brushes.White;

        F1 = new Font("Verdana", 10, FontStyle.Bold);
        F2 = new Font("Verdana", 7, FontStyle.Regular);

        S1 = Title.ToUpper();
        S2 = Subtitle;

        SZ1 = g.MeasureString(S1, F1, Width).ToSize();
        SZ2 = g.MeasureString(S2, F2, Width).ToSize();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Init(e);
        e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
        e.Graphics.FillPath(B1, GP1);

        e.Graphics.DrawPath(P1, GP1);
        e.Graphics.DrawPath(P2, GP2);

        e.Graphics.DrawString(S1, F1, B2, (Width - SZ1.Width) / 2, 20);
        e.Graphics.DrawString(S2, F2, B2, (Width - SZ2.Width) / 2, 32);
    }

    private GraphicsPath RoundRect(Rectangle rect, int radius)
    {
        GraphicsPath path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return path;
    }
}

[DefaultEvent("ChangeChecked")]
public class BitdefenderRadiobutton : Control
{
    // Event
    public event EventHandler<bool> ChangeChecked;

    private bool _check;
    private int oldHeight;

    public BitdefenderRadiobutton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer |
                 ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint, true);
        Width = 100;
        Height = 15;
        BackColor = Color.Transparent;
        oldHeight = 15;
    }

    public bool Checked
    {
        get => _check;
        set
        {
            _check = value;
            Invalidate();
            ChangeChecked?.Invoke(this, value);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        Checked = !Checked;
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);
        Height = oldHeight;
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

    private Rectangle R1, R2, R3;
    private GraphicsPath GP1, GP2, GP3;
    private LinearGradientBrush LGB1, LGB2;
    private Pen P1, P2;
    private Font F1;

    private void Init(PaintEventArgs e)
    {
        Graphics g = e.Graphics;

        R1 = new Rectangle(0, 0, Height - 1, Height - 1);
        R2 = new Rectangle(R1.X + 1, R1.Y + 1, R1.Width - 2, R1.Height - 2);
        R3 = new Rectangle(R2.X + 1, R2.Y + 1, R2.Width - 2, R2.Height - 2);

        GP1 = new GraphicsPath();
        GP1.AddEllipse(R1);
        GP2 = new GraphicsPath();
        GP2.AddEllipse(R2);
        GP3 = new GraphicsPath();
        GP3.AddEllipse(R3);

        LGB1 = new LinearGradientBrush(R1, Color.FromArgb(29, 29, 29), Color.FromArgb(39, 39, 39), LinearGradientMode.Vertical);
        LGB2 = new LinearGradientBrush(R3, Color.FromArgb(84, 135, 171), Color.FromArgb(113, 176, 200), LinearGradientMode.Vertical);

        P1 = Pens.Black;
        P2 = new Pen(Color.FromArgb(68, 68, 68));

        F1 = new Font("Verdana", 8, FontStyle.Regular);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Init(e);

        Graphics g = e.Graphics;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        if (Checked)
        {
            g.FillPath(LGB2, GP3);
            g.DrawPath(P1, GP3);
            g.DrawPath(P2, GP2);
            g.DrawString(Text, F1, new SolidBrush(Color.FromArgb(106, 166, 193)), 18, 1);
        }
        else
        {
            g.FillPath(LGB1, GP1);
            g.DrawPath(P1, GP1);
            g.DrawPath(P2, GP2);
            g.DrawString(Text, F1, new SolidBrush(Color.FromArgb(147, 147, 147)), 18, 1);
        }
    }
}
