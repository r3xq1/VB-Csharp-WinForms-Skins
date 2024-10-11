using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public static class ConversionFunctions
{
    public static Brush ToBrush(int A, int R, int G, int B) => new SolidBrush(Color.FromArgb(A, R, G, B));
    public static Brush ToBrush(int R, int G, int B) => new SolidBrush(Color.FromArgb(R, G, B));
    public static Brush ToBrush(int A, Color C) => new SolidBrush(Color.FromArgb(A, C));
    public static Brush ToBrush(Pen pen) => new SolidBrush(pen.Color);
    public static Brush ToBrush(Color color) => new SolidBrush(color);

    public static Pen ToPen(int A, int R, int G, int B) => new Pen(ToBrush(A, R, G, B));
    public static Pen ToPen(int R, int G, int B) => new Pen(ToBrush(R, G, B));
    public static Pen ToPen(int A, Color C) => new Pen(ToBrush(A, C));
    public static Pen ToPen(SolidBrush brush) => new Pen(brush);
    public static Pen ToPen(Color color) => new Pen(ToBrush(color));
}

public static class RRM
{
    public static GraphicsPath RoundRect(Rectangle rectangle, int curve)
    {
        GraphicsPath path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;
        path.AddArc(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth, 180, 90);
        path.AddArc(rectangle.X + rectangle.Width - arcRectangleWidth, rectangle.Y, arcRectangleWidth, arcRectangleWidth, 270, 90);
        path.AddArc(rectangle.X + rectangle.Width - arcRectangleWidth, rectangle.Y + rectangle.Height - arcRectangleWidth, arcRectangleWidth, arcRectangleWidth, 0, 90);
        path.AddArc(rectangle.X, rectangle.Y + rectangle.Height - arcRectangleWidth, arcRectangleWidth, arcRectangleWidth, 90, 90);
        path.AddLine(new Point(rectangle.X, rectangle.Y + rectangle.Height - arcRectangleWidth), new Point(rectangle.X, rectangle.Y + curve));
        return path;
    }

    public static GraphicsPath RoundRect(int x, int y, int width, int height, int curve) => RoundRect(new Rectangle(x, y, width, height), curve);
}

public static class Shapes
{
    public static Point[] Triangle(Point location, Size size)
    {
        return new Point[]
        {
            location,
            new Point(location.X + size.Width, location.Y),
            new Point(location.X + size.Width / 2, location.Y + size.Height),
            location
        };
    }
}

public class SkyDarkForm : ContainerControl
{
    private SkyDarkTop maxim;
    private SkyDarkTop exim;

    public SkyDarkForm()
    {
        AllowDrop = true; // Позволяем перетаскивание
        DragEnter += SkyDarkForm_DragEnter;
        DragDrop += SkyDarkForm_DragDrop;
        Dock = DockStyle.Fill;
        BackColor = Color.FromArgb(62, 60, 58);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        Parent.FindForm().FormBorderStyle = FormBorderStyle.None;

        maxim = new SkyDarkTop { Location = new Point(Width - 41, 3), Size = new Size(14, 14), Parent = this };
        exim = new SkyDarkTop { Location = new Point(Width - 22, 3), Size = new Size(14, 14), Parent = this };

        maxim.Click += Maxim_Click;
        exim.Click += Exim_Click;

        Controls.Add(maxim);
        Controls.Add(exim);
    }

    private void Exim_Click(object sender, EventArgs e)
    {
        Parent.FindForm().Close(); // Закрываем форму
    }

    private void Maxim_Click(object sender, EventArgs e)
    {
        var form = Parent.FindForm();
        form.WindowState = form.WindowState == FormWindowState.Normal
            ? FormWindowState.Minimized
            : FormWindowState.Normal; // Минимизируем или восстанавливаем форму
    }

    private void SkyDarkForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(Control)))
        {
            e.Effect = DragDropEffects.Move; // Указываем, что можно переместить
        }
    }

    private void SkyDarkForm_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(Control)))
        {
            Control control = (Control)e.Data.GetData(typeof(Control));

            // Получение позиции курсора и установка новой позиции компонента
            Point cursorPosition = PointToClient(new Point(e.X, e.Y));
            control.Location = new Point(cursorPosition.X - control.Width / 2, cursorPosition.Y - control.Height / 2);

            // Добавляем компонент на форму, если он еще не добавлен
            if (!Controls.Contains(control))
            {
                Controls.Add(control);
            }
        }
    }
}

public class SkyDarkButton : Control
{
    private Color c1 = Color.FromArgb(51, 49, 47);
    private Color c2 = Color.FromArgb(90, 91, 90);
    private Color c3 = Color.FromArgb(70, 71, 70);
    private Color c4 = Color.FromArgb(62, 61, 58);
    private MouseState state;

    public override string Text
    {
        get { return base.Text; }
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    public SkyDarkButton()
    {
        DoubleBuffered = true;
        AutoSize = true;
        TabStop = false;
        Size = new Size(116, 23);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        LinearGradientBrush g1 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), c3, c4);
        g.FillRectangle(g1, 0, 0, Width, Height);

        if (Enabled)
        {
            switch (state)
            {
                case MouseState.Over:
                    g.FillRectangle(new SolidBrush(Color.FromArgb(20, Color.White)), new Rectangle(0, 0, Width, Height));
                    break;

                case MouseState.Down:
                    g.FillRectangle(new SolidBrush(Color.FromArgb(10, Color.Black)), new Rectangle(0, 0, Width, Height));
                    break;
            }
        }

        StringFormat sf = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center
        };

        g.DrawString(Text, Font, Enabled ? ConversionFunctions.ToBrush(113, 170, 186) : Brushes.Gray,
            new Rectangle(0, 0, Width - 1, Height - 1), sf);

        g.DrawRectangle(ConversionFunctions.ToPen(c1), 0, 0, Width - 1, Height - 1);
        g.DrawRectangle(ConversionFunctions.ToPen(c2), 1, 1, Width - 3, Height - 3);

        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    private enum MouseState { None, Over, Down }

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

        // Проверяем, был ли клик внутри кнопки
        if (ClientRectangle.Contains(e.Location))
        {
            OnClick(EventArgs.Empty);
        }

        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        // Ваш код для обработки нажатия кнопки
    }
}

public class SkyDarkProgress : Control
{
    private int val;
    private int max;
    private Color c1 = Color.FromArgb(51, 49, 47);
    private Color c2 = Color.FromArgb(81, 77, 77);
    private Color c3 = Color.FromArgb(64, 60, 59);
    private Color c4 = Color.FromArgb(70, 71, 70);
    private Color c5 = Color.FromArgb(62, 59, 58);

    public SkyDarkProgress()
    {
        max = 100;
    }

    public int Value
    {
        get => val;
        set => SetValue(value);
    }

    public int Maximum
    {
        get => max;
        set => SetMaximum(value);
    }
    private int Clamp(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
    }

    private void SetValue(int value)
    {
        val = Clamp(value, 0, max);
        Invalidate();
    }

    private void SetMaximum(int value)
    {
        max = Math.Max(value, 1);
        if (max < val) val = max;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        Rectangle fill = new Rectangle(3, 3, (int)((Width - 7) * (val / (float)max)), Height - 7);
        g.Clear(c5);

        using (LinearGradientBrush g1 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), c3, c4))
        {
            g.FillRectangle(g1, fill);
        }
        g.DrawRectangle(ConversionFunctions.ToPen(c2), fill);
        g.DrawRectangle(ConversionFunctions.ToPen(c1), 0, 0, Width - 1, Height - 1);
        g.DrawRectangle(ConversionFunctions.ToPen(c2), 1, 1, Width - 3, Height - 3);

        e.Graphics.DrawImage(bitmap, 0, 0);
    }
}

public class SkyDarkTabControl : TabControl
{
    private Color c1 = Color.FromArgb(62, 60, 58);
    private Color c2 = Color.FromArgb(80, 78, 76);
    private Color c3 = Color.FromArgb(51, 49, 47);

    public SkyDarkTabControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer, true);
        DoubleBuffered = true;
    }

    protected override void CreateHandle()
    {
        base.CreateHandle();
        Alignment = TabAlignment.Top;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        try { SelectedTab.BackColor = c1; } catch { }

        g.Clear(Parent.BackColor);
        for (int i = 0; i < TabCount; i++)
        {
            if (i != SelectedIndex)
            {
                Rectangle tabRect = new Rectangle(GetTabRect(i).X, GetTabRect(i).Y + 3, GetTabRect(i).Width + 2, GetTabRect(i).Height);
                using (LinearGradientBrush g1 = new LinearGradientBrush(new Point(tabRect.X, tabRect.Y), new Point(tabRect.X, tabRect.Y + tabRect.Height), Color.FromArgb(60, 59, 58), Color.FromArgb(69, 69, 70)))
                {
                    g.FillRectangle(g1, tabRect);
                }

                g.DrawRectangle(ConversionFunctions.ToPen(c3), tabRect);
                g.DrawRectangle(ConversionFunctions.ToPen(c2), new Rectangle(tabRect.X + 1, tabRect.Y + 1, tabRect.Width - 2, tabRect.Height));
                g.DrawString(TabPages[i].Text, Font, ConversionFunctions.ToBrush(130, 176, 190), tabRect,
                             new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
            }
        }

        g.FillRectangle(ConversionFunctions.ToBrush(c1), 0, ItemSize.Height, Width, Height);
        g.DrawRectangle(ConversionFunctions.ToPen(c2), 0, ItemSize.Height, Width - 1, Height - ItemSize.Height - 1);
        g.DrawRectangle(ConversionFunctions.ToPen(c3), 1, ItemSize.Height + 1, Width - 3, Height - ItemSize.Height - 3);

        if (SelectedIndex != -1)
        {
            Rectangle selectedTabRect = new Rectangle(GetTabRect(SelectedIndex).X - 2, GetTabRect(SelectedIndex).Y, GetTabRect(SelectedIndex).Width + 3, GetTabRect(SelectedIndex).Height);
            g.FillRectangle(ConversionFunctions.ToBrush(c1), new Rectangle(selectedTabRect.X + 2, selectedTabRect.Y + 2, selectedTabRect.Width - 2, selectedTabRect.Height));
            g.DrawLine(ConversionFunctions.ToPen(c2), new Point(selectedTabRect.X, selectedTabRect.Y + selectedTabRect.Height - 2), new Point(selectedTabRect.X, selectedTabRect.Y));
            g.DrawLine(ConversionFunctions.ToPen(c2), new Point(selectedTabRect.X, selectedTabRect.Y), new Point(selectedTabRect.X + selectedTabRect.Width, selectedTabRect.Y));
            g.DrawLine(ConversionFunctions.ToPen(c2), new Point(selectedTabRect.X + selectedTabRect.Width, selectedTabRect.Y), new Point(selectedTabRect.X + selectedTabRect.Width, selectedTabRect.Y + selectedTabRect.Height - 2));

            g.DrawLine(ConversionFunctions.ToPen(c3), new Point(selectedTabRect.X + 1, selectedTabRect.Y + selectedTabRect.Height - 1), new Point(selectedTabRect.X + 1, selectedTabRect.Y + 1));
            g.DrawLine(ConversionFunctions.ToPen(c3), new Point(selectedTabRect.X + 1, selectedTabRect.Y + 1), new Point(selectedTabRect.X + selectedTabRect.Width - 1, selectedTabRect.Y + 1));
            g.DrawLine(ConversionFunctions.ToPen(c3), new Point(selectedTabRect.X + selectedTabRect.Width - 1, selectedTabRect.Y + 1), new Point(selectedTabRect.X + selectedTabRect.Width - 1, selectedTabRect.Y + selectedTabRect.Height - 1));

            g.DrawString(TabPages[SelectedIndex].Text, Font, ConversionFunctions.ToBrush(130, 176, 190), selectedTabRect,
                         new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
        }

        e.Graphics.DrawImage(bitmap, 0, 0);
    }
}

public class SkyDarkCombo : ComboBox
{
    private int _startIndex;
    private Color c1 = Color.FromArgb(48, 48, 48);
    private Color c2 = Color.FromArgb(81, 79, 77);
    private Color c3 = Color.FromArgb(62, 60, 58);

    public SkyDarkCombo()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer, true);
        DrawMode = DrawMode.OwnerDrawFixed;
        BackColor = Color.FromArgb(235, 235, 235);
        ForeColor = Color.FromArgb(31, 31, 31);
        DropDownStyle = ComboBoxStyle.DropDownList;
    }

    public int StartIndex
    {
        get => _startIndex;
        set
        {
            _startIndex = value;
            try { SelectedIndex = value; } catch { }
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (DropDownStyle != ComboBoxStyle.DropDownList) DropDownStyle = ComboBoxStyle.DropDownList;

        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(c3);

        float textHeight = g.MeasureString("...", Font).Height;
        g.DrawString(SelectedIndex != -1 ? Items[SelectedIndex].ToString() : (Items.Count > 0 ? Items[0].ToString() : "..."),
                     Font, ConversionFunctions.ToBrush(152, 182, 192), 4, Height / 2 - textHeight / 2);

        using (LinearGradientBrush g1 = new LinearGradientBrush(new Point(Width - 30, Height / 2), new Point(Width - 22, Height / 2), Color.Transparent, c3))
        {
            g.FillRectangle(g1, Width - 30, 0, 8, Height);
        }

        g.DrawRectangle(ConversionFunctions.ToPen(c1), new Rectangle(0, 0, Width - 1, Height - 1));
        g.DrawLine(ConversionFunctions.ToPen(c1), new Point(Width - 21, 0), new Point(Width - 21, Height));
        g.DrawRectangle(ConversionFunctions.ToPen(c2), 1, 1, Width - 23, Height - 3);

        g.FillRectangle(ConversionFunctions.ToBrush(c3), Width - 20, 1, 18, Height - 3);
        g.FillRectangle(ConversionFunctions.ToBrush(10, Color.White), Width - 20, 1, 18, Height - 3);
        g.DrawRectangle(ConversionFunctions.ToPen(c2), Width - 20, 1, 18, Height - 3);

        g.FillPolygon(Brushes.Black, Shapes.Triangle(new Point(Width - 12, Height / 2), new Size(5, 3)));
        g.FillPolygon(Brushes.LightBlue, Shapes.Triangle(new Point(Width - 13, Height / 2 - 1), new Size(5, 3)));

        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    private void ReplaceItem(object sender, DrawItemEventArgs e)
    {
        Graphics g = e.Graphics;
        Color itemColor = Color.Empty;
        e.DrawBackground();

        try
        {
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                g.FillRectangle(ConversionFunctions.ToBrush(50, 80, 120), new Rectangle(e.Bounds.X - 1, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height + 2));
                g.DrawRectangle(new Pen(ConversionFunctions.ToBrush(180, Color.Black), 1), new Rectangle(e.Bounds.X - 1, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height + 2));
                itemColor = Color.FromArgb(100, 165, 185);
            }
            else
            {
                g.FillRectangle(ConversionFunctions.ToBrush(62, 60, 58), new Rectangle(e.Bounds.X - 1, e.Bounds.Y, e.Bounds.Width, e.Bounds.Height + 2));
                itemColor = Color.FromArgb(200, 200, 200);
            }

            g.DrawString(GetItemText(Items[e.Index]), e.Font, ConversionFunctions.ToBrush(itemColor), e.Bounds, new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Center });
        }
        catch { }
    }

    private void SkyDarkCombo_TextChanged(object sender, EventArgs e)
    {
        Invalidate();
    }

    protected override void OnDrawItem(DrawItemEventArgs e)
    {
        ReplaceItem(this, e);
    }
}

public class SkyDarkTop : Control
{
    private Color c1 = Color.FromArgb(94, 103, 106);
    private Color c2 = Color.FromArgb(152, 182, 192);
    private Color cd = Color.FromArgb(86, 94, 96);
    private Color c3 = Color.FromArgb(71, 70, 69);
    private Color c4 = Color.FromArgb(58, 56, 54);
    private MouseState state;

    public SkyDarkTop()
    {
        DoubleBuffered = true;
        Size = new Size(10, 10);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        using (LinearGradientBrush g1 = new LinearGradientBrush(new Point(0, 0), new Point(0, Height), c3, c4))
        {
            g.FillRectangle(g1, ClientRectangle);
        }
        g.SmoothingMode = SmoothingMode.HighQuality;

        switch (state)
        {
            case MouseState.None:
                g.DrawEllipse(new Pen(c1, 2), new Rectangle(2, 2, Width - 5, Height - 5));
                break;
            case MouseState.Over:
                g.DrawEllipse(new Pen(c2, 2), new Rectangle(2, 2, Width - 5, Height - 5));
                break;
            case MouseState.Down:
                g.DrawEllipse(new Pen(cd, 2), new Rectangle(2, 2, Width - 5, Height - 5));
                break;
        }

        g.FillEllipse(ConversionFunctions.ToBrush(c2), new Rectangle(5, 5, Width - 11, Height - 11));

        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    private enum MouseState { None, Over, Down }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        state = MouseState.Over;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        state = MouseState.None;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        state = MouseState.Down;
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        state = MouseState.Over;
    }
}

public class SkyDarkSeparator : Control
{
    public enum Alignment
    {
        Vertical,
        Horizontal
    }

    private Alignment alignment;
    private Color c1 = Color.FromArgb(51, 49, 47);
    private Color c2 = Color.FromArgb(90, 91, 90);

    public Alignment Align
    {
        get => alignment;
        set
        {
            alignment = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        switch (alignment)
        {
            case Alignment.Horizontal:
                g.DrawLine(ConversionFunctions.ToPen(c1), new Point(0, Height / 2), new Point(Width, Height / 2));
                g.DrawLine(ConversionFunctions.ToPen(c2), new Point(0, Height / 2 + 1), new Point(Width, Height / 2 + 1));
                break;
            case Alignment.Vertical:
                g.DrawLine(ConversionFunctions.ToPen(c1), new Point(Width / 2, 0), new Point(Width / 2, Height));
                g.DrawLine(ConversionFunctions.ToPen(c2), new Point(Width / 2 + 1, 0), new Point(Width / 2 + 1, Height));
                break;
        }

        e.Graphics.DrawImage(bitmap, 0, 0);
    }
}

public class SkyDarkCheck : Control
{
    private bool check;
    private SkyDarkRadio.MouseState state;
    private Color c1 = Color.FromArgb(51, 49, 47);
    private Color c2 = Color.FromArgb(80, 77, 77);
    private Color c3 = Color.FromArgb(70, 69, 68);
    private Color c4 = Color.FromArgb(64, 60, 59);
    private Color c5 = Color.Transparent;

    public bool Checked
    {
        get => check;
        set
        {
            check = value;
            Invalidate();
        }
    }

    public SkyDarkRadio.MouseState State
    {
        get => state;
        set
        {
            state = value;
            Invalidate();
        }
    }

    public SkyDarkCheck()
    {
        // Включаем двойную буферизацию
        this.DoubleBuffered = true;
        Size = new Size(Width, 13);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(Parent.BackColor);
        c3 = Color.FromArgb(70, 69, 68);
        c4 = Color.FromArgb(64, 60, 59);

        if (Enabled)
        {
            switch (state)
            {
                case SkyDarkRadio.MouseState.Down:
                    c5 = Color.FromArgb(121, 151, 160);
                    break;
                case SkyDarkRadio.MouseState.None:
                    c5 = Color.FromArgb(151, 181, 190);
                    break;
            }
        }
        else
        {
            c5 = Color.FromArgb(88, 88, 88);
        }

        Rectangle chkRec = new Rectangle(0, 0, Height - 1, Height - 1);
        using (LinearGradientBrush g1 = new LinearGradientBrush(new Point(chkRec.X, chkRec.Y), new Point(chkRec.X, chkRec.Y + chkRec.Height), c3, c4))
        {
            g.FillRectangle(g1, chkRec);
        }
        g.DrawRectangle(new Pen(c1), chkRec);
        g.DrawRectangle(new Pen(c2), new Rectangle(chkRec.X + 1, chkRec.Y + 1, chkRec.Width - 2, chkRec.Height - 2));

        if (Checked)
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            var poly = new[]
            {
                new Point(chkRec.X + chkRec.Width / 4, chkRec.Y + chkRec.Height / 2),
                new Point(chkRec.X + chkRec.Width / 2, chkRec.Y + chkRec.Height),
                new Point(chkRec.X + chkRec.Width, chkRec.Y)
            };

            using (Pen p1 = new Pen(c5, 2))
            {
                for (int i = 0; i < poly.Length - 1; i++)
                {
                    g.DrawLine(p1, poly[i], poly[i + 1]);
                }
            }
        }

        g.DrawString(Text, Font, new SolidBrush(c5), new Rectangle(chkRec.X + chkRec.Width + 5, 0, Width - chkRec.X - chkRec.Width - 5, Height),
            new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });

        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (Enabled)
        {
            State = SkyDarkRadio.MouseState.None;
            Checked = !Checked;
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled)
        {
            State = SkyDarkRadio.MouseState.Down;
        }
    }
}

public class SkyDarkRadio : Control
{
    private MouseState state;
    private bool check;

    public enum MouseState
    {
        None,
        Down
    }

    public MouseState State
    {
        get => state;
        set
        {
            state = value;
            Invalidate();
        }
    }

    public bool Checked
    {
        get => check;
        set
        {
            check = value;
            Invalidate();
            // Удаляем отметку у других радиокнопок в родительском контроле
            if (value)
            {
                foreach (Control ctl in Parent.Controls)
                {
                    if (ctl is SkyDarkRadio radio && radio != this && radio.Enabled)
                    {
                        radio.Checked = false;
                    }
                }
            }
        }
    }

    private Color c1 = Color.FromArgb(35, 35, 35);
    private Color c2 = Color.Transparent;
    private Color c3 = Color.Transparent;
    private Color c4 = Color.Transparent;

    public SkyDarkRadio()
    {
        // Включаем двойную буферизацию для уменьшения мерцания
        this.DoubleBuffered = true;
        Size = new Size(Width, 13);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap bitmap = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(bitmap);
        g.Clear(Parent.BackColor);

        switch (state)
        {
            case MouseState.None:
                c2 = Color.FromArgb(70, 70, 70);
                c3 = Color.FromArgb(54, 54, 51);
                c4 = Color.FromArgb(152, 182, 192);
                break;
            case MouseState.Down:
                c2 = Color.FromArgb(54, 54, 51);
                c3 = Color.FromArgb(70, 70, 70);
                c4 = Color.FromArgb(112, 142, 152);
                break;
        }

        Rectangle radRec = new Rectangle(0, 0, Height - 1, Height - 1);
        using (LinearGradientBrush brush = new LinearGradientBrush(new Point(radRec.X + radRec.Width / 2, radRec.Y), new Point(radRec.X + radRec.Width / 2, radRec.Y + radRec.Height), c2, c3))
        {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.FillEllipse(brush, radRec);
        }

        g.DrawEllipse(new Pen(ConversionFunctions.ToBrush(c1)), radRec);
        if (Checked)
        {
            g.FillEllipse(ConversionFunctions.ToBrush(c4), new Rectangle(radRec.X + radRec.Width / 4, radRec.Y + radRec.Height / 4, radRec.Width / 2, radRec.Height / 2));
        }
        g.DrawString(Text, Font, ConversionFunctions.ToBrush(c4), new Rectangle(radRec.X + radRec.Width + 5, 0, Width - radRec.X - radRec.Width - 5, Height), new StringFormat { LineAlignment = StringAlignment.Center, Alignment = StringAlignment.Near });

        e.Graphics.DrawImage(bitmap, 0, 0);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (Enabled)
        {
            State = MouseState.None;
            if (!Checked)
            {
                Checked = true;
            }
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (Enabled)
        {
            State = MouseState.Down;
        }
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        if (Enabled)
        {
            State = MouseState.None;
            Invalidate();
        }
    }
}


