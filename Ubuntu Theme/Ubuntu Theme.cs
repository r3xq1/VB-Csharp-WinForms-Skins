using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

enum MouseState : byte
{
    None = 0,
    Over = 1,
    Down = 2,
    Block = 3
}

static class Draw
{
    // Function for creating rounded rectangles
    public static GraphicsPath RoundRect(Rectangle rectangle, int curve)
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

    // Other draw functions can be added here
    /*
    public static GraphicsPath RoundRect(int x, int y, int width, int height, int curve)
    {
        Rectangle rectangle = new Rectangle(x, y, width, height);
        GraphicsPath path = new GraphicsPath();
        int arcRectangleWidth = curve * 2;
        path.AddArc(new Rectangle(rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -180, 90);
        path.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Y, arcRectangleWidth, arcRectangleWidth), -90, 90);
        path.AddArc(new Rectangle(rectangle.Width - arcRectangleWidth + rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 0, 90);
        path.AddArc(new Rectangle(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y, arcRectangleWidth, arcRectangleWidth), 90, 90);
        path.AddLine(new Point(rectangle.X, rectangle.Height - arcRectangleWidth + rectangle.Y), new Point(rectangle.X, curve + rectangle.Y));
        return path;
    }
    */
}

//public class UbuntuControlBox : Control
//{
//    #region "MouseStates"
//    private MouseState state = MouseState.None;
//    private int x;
//    private Rectangle closeBtn = new Rectangle(43, 2, 17, 17);
//    private Rectangle minBtn = new Rectangle(3, 2, 17, 17);
//    private Rectangle maxBtn = new Rectangle(23, 2, 17, 17);
//    private Rectangle bgr = new Rectangle(0, 0, 62, 21);

//    protected override void OnMouseDown(MouseEventArgs e)
//    {
//        base.OnMouseDown(e);
//        if (x > 3 && x < 20)
//        {
//            FindForm().WindowState = FormWindowState.Minimized;
//        }
//        else if (x > 23 && x < 40)
//        {
//            if (FindForm().WindowState == FormWindowState.Maximized)
//            {
//                FindForm().WindowState = FormWindowState.Normal;
//            }
//            else
//            {
//                FindForm().WindowState = FormWindowState.Maximized;
//            }
//        }
//        else if (x > 43 && x < 60)
//        {
//            FindForm().Close();
//        }
//        state = MouseState.Down;
//        Invalidate();
//    }

//    protected override void OnMouseUp(MouseEventArgs e)
//    {
//        base.OnMouseUp(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseEnter(EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseLeave(EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        state = MouseState.None;
//        Invalidate();
//    }

//    protected override void OnMouseMove(MouseEventArgs e)
//    {
//        base.OnMouseMove(e);
//        x = e.Location.X;
//        Invalidate();
//    }
//    #endregion

//    public UbuntuControlBox()
//    {
//        TabStop = false;
//        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
//        BackColor = Color.Transparent;
//        DoubleBuffered = true;
//        Font = new Font("Marlett", 7);
//        Anchor = AnchorStyles.Top | AnchorStyles.Right;
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        Bitmap b = new Bitmap(Width, Height);
//        using (Graphics g = Graphics.FromImage(b))
//        {
//            base.OnPaint(e);

//            g.SmoothingMode = SmoothingMode.AntiAlias;
//            g.Clear(Color.Transparent);

//            // Draw background
//            using (LinearGradientBrush bg0 = new LinearGradientBrush(bgr, Color.FromArgb(60, 59, 55), Color.FromArgb(60, 59, 55), 90F))
//            {
//                g.FillPath(bg0, Draw.RoundRect(bgr, 10));
//            }

//            // Draw buttons
//            DrawButton(g, minBtn, "0", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//            DrawButton(g, maxBtn, "1", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//            DrawButton(g, closeBtn, "r", Color.FromArgb(247, 150, 116), Color.FromArgb(223, 81, 6));

//            // Draw based on mouse state
//            switch (state)
//            {
//                case MouseState.None:
//                    DrawButtons(g);
//                    break;

//                case MouseState.Over:
//                    DrawButtonsOver(g);
//                    break;

//                case MouseState.Down:
//                    DrawButtonsDown(g);
//                    break;
//            }

//            e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
//        }
//    }

//    private void DrawButtons(Graphics g)
//    {
//        DrawButton(g, minBtn, "0", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//        DrawButton(g, maxBtn, "1", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//        DrawButton(g, closeBtn, "r", Color.FromArgb(247, 150, 116), Color.FromArgb(223, 81, 6));
//    }

//    private void DrawButtonsOver(Graphics g)
//    {
//        if (x > 3 && x < 20)
//            DrawButtonOver(g, minBtn, "0");

//        if (x > 23 && x < 40)
//            DrawButtonOver(g, maxBtn, "1");

//        if (x > 43 && x < 60)
//            DrawButtonOver(g, closeBtn, "r");
//    }

//    private void DrawButtonsDown(Graphics g)
//    {
//        if (x > 3 && x < 20)
//            DrawButtonDown(g, minBtn, "0");

//        if (x > 23 && x < 40)
//            DrawButtonDown(g, maxBtn, "1");

//        if (x > 43 && x < 60)
//            DrawButtonDown(g, closeBtn, "r");
//    }

//    private void DrawButton(Graphics g, Rectangle btnRect, string text, Color color1, Color color2)
//    {
//        using (GraphicsPath path = new GraphicsPath())
//        {
//            path.AddEllipse(btnRect);
//            using (LinearGradientBrush brush = new LinearGradientBrush(btnRect, color1, color2, 90F))
//            {
//                g.FillPath(brush, path);
//                g.DrawPath(Pens.DimGray, path);

//                // Индикатор размера текста
//                var textSize = g.MeasureString(text, Font);

//                // Центрирование текста по горизонтали и вертикали
//                float textX = btnRect.X + (btnRect.Width - textSize.Width) / 1.2f;
//                float textY = btnRect.Y + (btnRect.Height - textSize.Height) / 1.2f;

//                g.DrawString(text, Font, new SolidBrush(Color.FromArgb(58, 57, 53)), new PointF(textX, textY));
//            }
//        }
//    }



//    private void DrawButtonOver(Graphics g, Rectangle btnRect, string text)
//    {
//        DrawButton(g, btnRect, text, Color.FromArgb(172, 171, 166), Color.FromArgb(76, 75, 71));
//    }

//    private void DrawButtonDown(Graphics g, Rectangle btnRect, string text)
//    {
//        DrawButton(g, btnRect, text, Color.FromArgb(200, 116, 83), Color.FromArgb(194, 101, 65));
//    }
//}

public class UbuntuButtonOrange : Control
{
    #region "MouseStates"
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

    public UbuntuButtonOrange()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(86, 109, 109);
        DoubleBuffered = true;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Bitmap b = new Bitmap(Width, Height);
        using (Graphics g = Graphics.FromImage(b))
        {
            Rectangle clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            base.OnPaint(e);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(BackColor);

            using Font drawFont = new Font("Tahoma", 11, FontStyle.Regular);
            Brush nb = new SolidBrush(Color.FromArgb(86, 109, 109));
            Pen p = new Pen(Color.FromArgb(157, 118, 103), 1);

            switch (state)
            {
                case MouseState.None:
                    using (LinearGradientBrush lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(249, 163, 128), Color.FromArgb(237, 139, 99), 90F))
                    {
                        g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                        g.DrawPath(p, Draw.RoundRect(clientRectangle, 3));
                    }
                    g.DrawString(Text, drawFont, nb, new Rectangle(0, 0, Width - 1, Height - 1), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    break;

                case MouseState.Over:
                    using (LinearGradientBrush lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(255, 186, 153), Color.FromArgb(255, 171, 135), 90F))
                    {
                        g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                        g.DrawPath(p, Draw.RoundRect(clientRectangle, 3));
                    }
                    g.DrawString(Text, drawFont, nb, new Rectangle(0, 0, Width - 1, Height - 1), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    break;

                case MouseState.Down:
                    using (LinearGradientBrush lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(200, 116, 83), Color.FromArgb(194, 101, 65), 90F))
                    {
                        g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                        g.DrawPath(p, Draw.RoundRect(clientRectangle, 3));
                    }
                    g.DrawString(Text, drawFont, nb, new Rectangle(0, 0, Width - 1, Height - 1), new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    break;
            }
        }
        e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
    }
}

public class UbuntuButtonGray : Control
{
    #region "MouseStates"
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

    public UbuntuButtonGray()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        ForeColor = Color.FromArgb(90, 84, 82);
        DoubleBuffered = true;
        TabStop = false;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Bitmap b = new Bitmap(Width, Height);
        using (Graphics g = Graphics.FromImage(b))
        {
            Rectangle clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
            base.OnPaint(e);

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(BackColor);

            using Font drawFont = new Font("Tahoma", 11, FontStyle.Regular);
            Brush nb = new SolidBrush(Color.FromArgb(80, 84, 82));
            Pen p = new Pen(Color.FromArgb(166, 166, 166), 1);

            switch (state)
            {
                case MouseState.None:
                    using (LinearGradientBrush lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(223, 223, 223), Color.FromArgb(197, 197, 197), 90F))
                    {
                        g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                        g.DrawPath(p, Draw.RoundRect(clientRectangle, 3));
                    }
                    g.DrawString(Text, drawFont, nb, new Rectangle(0, 0, Width - 1, Height - 1),
                                 new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    break;

                case MouseState.Over:
                    using (LinearGradientBrush lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(243, 243, 243), Color.FromArgb(217, 217, 217), 90F))
                    {
                        g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                        g.DrawPath(p, Draw.RoundRect(clientRectangle, 3));
                    }
                    g.DrawString(Text, drawFont, nb, new Rectangle(0, 0, Width - 1, Height - 1),
                                 new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    break;

                case MouseState.Down:
                    using (LinearGradientBrush lgb = new LinearGradientBrush(clientRectangle, Color.FromArgb(212, 211, 216), Color.FromArgb(156, 155, 151), 90F))
                    {
                        g.FillPath(lgb, Draw.RoundRect(clientRectangle, 3));
                        g.DrawPath(p, Draw.RoundRect(clientRectangle, 3));
                    }
                    g.DrawString(Text, drawFont, nb, new Rectangle(0, 0, Width - 1, Height - 1),
                                 new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                    break;
            }
        }
        e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
    }
}

//[DefaultEvent("CheckedChanged")]
//public class UbuntuRadioButton : Control
//{
//    #region " Control Help - MouseState & Flicker Control"
//    private MouseState state = MouseState.None;
//    private bool _checked;

//    protected override void OnMouseEnter(EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseDown(MouseEventArgs e)
//    {
//        base.OnMouseDown(e);
//        state = MouseState.Down;
//        Invalidate();
//    }

//    protected override void OnMouseLeave(EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        state = MouseState.None;
//        Invalidate();
//    }

//    protected override void OnMouseUp(MouseEventArgs e)
//    {
//        base.OnMouseUp(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnResize(EventArgs e)
//    {
//        base.OnResize(e);
//        Height = 16;
//    }

//    protected override void OnTextChanged(EventArgs e)
//    {
//        base.OnTextChanged(e);
//        Invalidate();
//    }

//    public bool Checked
//    {
//        get => _checked;
//        set
//        {
//            _checked = value;
//            InvalidateControls();
//            CheckedChanged?.DynamicInvoke(this);
//            Invalidate();
//        }
//    }

//    private void InvalidateControls()
//    {
//        if (!IsHandleCreated || !_checked) return;

//        foreach (Control c in Parent.Controls)
//        {
//            if (c != this && c is UbuntuRadioButton radioButton)
//            {
//                radioButton.Checked = false;
//            }
//        }
//    }

//    public event EventHandler CheckedChanged;

//    #endregion

//    public UbuntuRadioButton()
//    {
//        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
//        BackColor = Color.WhiteSmoke;
//        ForeColor = Color.Black;
//        Size = new Size(150, 16);
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        Bitmap b = new Bitmap(Width, Height);
//        using (Graphics g = Graphics.FromImage(b))
//        {
//            Rectangle radioBtnRectangle = new Rectangle(0, 0, Height - 1, Height - 1);
//            g.SmoothingMode = SmoothingMode.HighQuality;
//            g.Clear(BackColor);

//            using (LinearGradientBrush bgGrad = new LinearGradientBrush(radioBtnRectangle, Color.FromArgb(102, 101, 96), Color.FromArgb(76, 75, 71), 90F))
//            {
//                g.FillEllipse(bgGrad, radioBtnRectangle);
//                g.DrawEllipse(new Pen(Color.Gray), new Rectangle(1, 1, Height - 3, Height - 3));
//                g.DrawEllipse(new Pen(Color.FromArgb(42, 47, 49)), radioBtnRectangle);
//            }

//            if (Checked)
//            {
//                using (LinearGradientBrush chkGrad = new LinearGradientBrush(new Rectangle(4, 4, Height - 9, Height - 9), Color.FromArgb(247, 150, 116), Color.FromArgb(197, 100, 66), 90F))
//                {
//                    g.FillEllipse(chkGrad, new Rectangle(4, 4, Height - 9, Height - 9));
//                }
//            }

//            using (Font drawFont = new Font("Tahoma", 10, FontStyle.Regular))
//            {
//                Brush nb = new SolidBrush(Color.FromArgb(86, 83, 87));
//                g.DrawString(Text, drawFont, nb, new Point(16, 1), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });
//            }

//            e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
//        }
//    }
//}

public class UbuntuGroupBox : ContainerControl
{
    private StringAlignment _textAlignment = StringAlignment.Center;
    private Color _headerColor = Color.FromArgb(87, 86, 81); // Цвет панели заголовка
    private Color _textColor = Color.White; // Цвет текста заголовка

    public UbuntuGroupBox()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
        TabStop = false;
    }

    public StringAlignment TextAlignment
    {
        get => _textAlignment;
        set
        {
            _textAlignment = value;
            Invalidate(); // Перерисовать контрол при изменении выравнивания
        }
    }

    public Color HeaderColor // Свойство для изменения цвета панели заголовка
    {
        get => _headerColor;
        set
        {
            _headerColor = value;
            Invalidate(); // Перерисовать контрол при изменении цвета панели заголовка
        }
    }

    public Color TextColor // Свойство для изменения цвета текста заголовка
    {
        get => _textColor;
        set
        {
            _textColor = value;
            Invalidate(); // Перерисовать контрол при изменении цвета текста заголовка
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Bitmap b = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(b);
        Rectangle topBar = new Rectangle(0, 0, Width - 1, 20);
        base.OnPaint(e);

        g.Clear(Color.Transparent);
        g.SmoothingMode = SmoothingMode.HighQuality;

        using (LinearGradientBrush bodyBrush = new LinearGradientBrush(ClientRectangle, Color.White, Color.White, 120F))
        {
            g.FillPath(bodyBrush, Draw.RoundRect(new Rectangle(0, 12, Width - 1, Height - 15), 1));
        }

        using (Pen outerBorderPen = new Pen(Color.FromArgb(50, 50, 50)))
        {
            g.DrawPath(outerBorderPen, Draw.RoundRect(new Rectangle(0, 12, Width - 1, Height - 15), 1));
        }

        using (LinearGradientBrush topBarBrush = new LinearGradientBrush(topBar, _headerColor, _headerColor, 90F))
        {
            g.FillPath(topBarBrush, Draw.RoundRect(topBar, 1));
        }

        using (Pen topBarBorderPen = new Pen(Color.FromArgb(50, 50, 50)))
        {
            g.DrawPath(topBarBorderPen, Draw.RoundRect(topBar, 2));
        }

        using (Font drawFont = new Font("Tahoma", 9, FontStyle.Regular))
        {
            // Измерение размера текста
            SizeF textSize = g.MeasureString(Text, drawFont);
            var textX = _textAlignment switch
            {
                // Слева
                StringAlignment.Near => 5,
                // По центру
                StringAlignment.Center => (topBar.Width - textSize.Width) / 2,
                // Справа
                StringAlignment.Far => topBar.Width - textSize.Width - 5,
                _ => 5,// по умолчанию - слева
            };
            g.DrawString(Text, drawFont, new SolidBrush(_textColor), new PointF(textX, 5));
        }

        e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
    }
}

//public class UbuntuTheme : ContainerControl
//{
//    private Point mouseP = new Point(0, 0);
//    private bool cap = false;
//    private int moveHeight = 26;
//    private bool showIcon = true;
//    private Icon formIcon;

//    public Icon FormIcon
//    {
//        get => formIcon;
//        set
//        {
//            formIcon = value;
//            Invalidate();
//        }
//    }

//    public bool ShowIcon
//    {
//        get => showIcon;
//        set
//        {
//            showIcon = value;
//            Invalidate();
//        }
//    }

//    public new string Text
//    {
//        get => base.Text;
//        set
//        {
//            base.Text = value;
//            Invalidate();
//        }
//    }

//    public UbuntuTheme()
//    {
//        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
//        BackColor = Color.FromArgb(25, 25, 25);
//        DoubleBuffered = true;
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        using (Bitmap b = new Bitmap(Width, Height))
//        using (Graphics g = Graphics.FromImage(b))
//        {
//            Rectangle topBar = new Rectangle(0, 0, Width - 1, 30);
//            Rectangle body = new Rectangle(0, 5, Width - 1, Height - 6);

//            // Полный фон заполняем темным цветом
//            g.Clear(Color.FromArgb(25, 25, 25));
//            g.SmoothingMode = SmoothingMode.HighSpeed;

//            using (LinearGradientBrush lbb = new LinearGradientBrush(body, Color.FromArgb(242, 241, 240), Color.FromArgb(240, 240, 238), 90F))
//            {
//                g.FillPath(lbb, RoundRect(body, 1));
//                g.DrawPath(new Pen(Color.FromArgb(60, 60, 60)), RoundRect(body, 1));
//            }

//            using (LinearGradientBrush lgb = new LinearGradientBrush(topBar, Color.FromArgb(87, 86, 81), Color.FromArgb(60, 59, 55), 90F))
//            {
//                g.FillPath(lgb, RoundRect(topBar, 4));
//                g.DrawPath(Pens.Black, RoundRect(topBar, 3));
//            }

//            // Задаем смещение текста в зависимости от состояния видимости иконки
//            int textOffset = showIcon ? 25 : 5;

//            using (Font drawFont = new Font("Tahoma", 10, FontStyle.Regular))
//            {
//                g.DrawString(Text, drawFont, Brushes.WhiteSmoke, new Rectangle(textOffset, 0, Width - 1, 27),
//                              new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
//            }

//            // Отображение иконки только если showIcon равно true
//            if (formIcon != null && showIcon)
//            {
//                g.DrawIcon(formIcon, new Rectangle(5, 5, 16, 16));
//            }

//            e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
//        }
//    }

//    protected override void OnMouseDown(MouseEventArgs e)
//    {
//        base.OnMouseDown(e);
//        if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, moveHeight).Contains(e.Location))
//        {
//            cap = true;
//            mouseP = e.Location;
//        }
//    }

//    protected override void OnMouseUp(MouseEventArgs e)
//    {
//        base.OnMouseUp(e);
//        cap = false;
//    }

//    protected override void OnMouseMove(MouseEventArgs e)
//    {
//        base.OnMouseMove(e);
//        if (cap)
//        {
//            Parent.Location = new Point(MousePosition.X - mouseP.X, MousePosition.Y - mouseP.Y);
//        }
//    }

//    protected override void OnCreateControl()
//    {
//        base.OnCreateControl();
//        ParentForm.FormBorderStyle = FormBorderStyle.None;
//        ParentForm.TransparencyKey = Color.Fuchsia;
//        Dock = DockStyle.Fill;
//    }

//    private void UbuntuTheme_SizeChanged(object sender, EventArgs e)
//    {
//        // Обработчик события изменения размера
//    }

//    private GraphicsPath RoundRect(Rectangle rect, int radius)
//    {
//        GraphicsPath path = new GraphicsPath();
//        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
//        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
//        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
//        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
//        path.CloseFigure();
//        return path;
//    }
//}

public class UbuntuTextBox : Control
{
    private TextBox txtbox = new TextBox();
    private bool _passmask = false;
    private int _maxchars = 32767;
    private HorizontalAlignment _align;

    public UbuntuTextBox()
    {
        InitializeTextBox();
        BackColor = Color.White;
        ForeColor = Color.FromArgb(102, 102, 102);
        Size = new Size(135, 35);
        DoubleBuffered = true;
        TabStop = false;
        Controls.Add(txtbox);
    }

    private void InitializeTextBox()
    {
        txtbox.Multiline = false;
        txtbox.BackColor = Color.FromArgb(43, 43, 43);
        txtbox.ForeColor = ForeColor;
        txtbox.BorderStyle = BorderStyle.None;
        txtbox.Font = new Font("Trebuchet MS", 8.25F, FontStyle.Bold);
        txtbox.Size = new Size(Width - 10, Height - 11);
        txtbox.TextChanged += (s, e) => Text = txtbox.Text;

        // Установка расположения для центрирования текста
        txtbox.Location = new Point(5, (Height - txtbox.Height) / 2);
    }

    public bool UseSystemPasswordChar
    {
        get => _passmask;
        set
        {
            _passmask = value;
            txtbox.UseSystemPasswordChar = value;
            Invalidate();
        }
    }

    public int MaxLength
    {
        get => _maxchars;
        set
        {
            _maxchars = value;
            txtbox.MaxLength = value;
            Invalidate();
        }
    }

    public HorizontalAlignment TextAlignment
    {
        get => _align;
        set
        {
            _align = value;
            txtbox.TextAlign = value;
            Invalidate();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent) { }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        txtbox.Text = Text; // Синхронизация текста
        Invalidate();
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        txtbox.BackColor = BackColor;
    }

    protected override void OnForeColorChanged(EventArgs e)
    {
        base.OnForeColorChanged(e);
        txtbox.ForeColor = ForeColor;
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        txtbox.Font = Font;
        // Перерасчет расположения при изменении шрифта
        txtbox.Location = new Point(5, (Height - txtbox.Height) / 2);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        txtbox.Focus();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        txtbox.Size = new Size(Width - 10, Height - 11);
        // Перерасчет расположения при изменении размера
        txtbox.Location = new Point(5, (Height - txtbox.Height) / 2);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap B = new Bitmap(Width, Height);
        using Graphics G = Graphics.FromImage(B);
        G.Clear(Color.Transparent);
        G.SmoothingMode = SmoothingMode.HighQuality;
        G.CompositingQuality = CompositingQuality.HighQuality;

        Rectangle clientRectangle = new Rectangle(0, 0, Width - 1, Height - 1);
        G.FillRectangle(new SolidBrush(Color.White), clientRectangle);
        G.DrawPath(new Pen(Color.FromArgb(255, 207, 188), 2), RoundRect(new Rectangle(1, 1, Width - 3, Height - 3), 5));
        G.DrawPath(new Pen(Color.FromArgb(205, 87, 40), 2), RoundRect(new Rectangle(0, 0, Width - 1, Height - 1), 5));

        e.Graphics.DrawImage(B, 0, 0);
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


[DefaultEvent("CheckedChanged")]
public class UbuntuCheckBox : Control
{
    #region " Control Help - MouseState & Flicker Control"
    private bool _checked;
    private Color _checkColor = Color.FromArgb(247, 150, 116); // Цвет галочки

    public enum CheckBoxDesign
    {
        Default,
        Flat
    }

    private CheckBoxDesign _designStyle = CheckBoxDesign.Default;

    public CheckBoxDesign DesignStyle
    {
        get => _designStyle;
        set
        {
            _designStyle = value;
            Invalidate();
        }
    }

    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            Invalidate();
            CheckedChanged?.Invoke(this, EventArgs.Empty); // Вызов события при изменении состояния
        }
    }

    public event EventHandler CheckedChanged;

    public UbuntuCheckBox()
    {
        TabStop = false;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.WhiteSmoke;
        ForeColor = Color.Black;
        Size = new Size(145, 16);
    }

    public Color CheckColor // Свойство для настройки цвета галочки
    {
        get => _checkColor;
        set
        {
            _checkColor = value;
            Invalidate(); // Обновляем отображение при изменении цвета
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        Invalidate();
        base.OnMouseDown(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        Invalidate();
        base.OnMouseUp(e);
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked; // Переключаем состояние
        CheckedChanged?.Invoke(this, EventArgs.Empty); // Вызываем событие
        base.OnClick(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Bitmap b = new Bitmap(Width, Height);
        using (Graphics g = Graphics.FromImage(b))
        {
            g.Clear(BackColor);
            Rectangle checkBoxRectangle = new Rectangle(0, 0, Height - 1, Height - 1);

            // Проверяем стиль оформления
            if (DesignStyle == CheckBoxDesign.Default)
            {
                using (LinearGradientBrush bodyBrush = new LinearGradientBrush(checkBoxRectangle, Color.FromArgb(102, 101, 96), Color.FromArgb(76, 75, 71), 90F))
                {
                    g.FillRectangle(bodyBrush, checkBoxRectangle);
                    g.DrawRectangle(new Pen(Color.Gray), new Rectangle(1, 1, Height - 3, Height - 3));
                    g.DrawRectangle(new Pen(Color.FromArgb(42, 47, 49)), checkBoxRectangle);
                }

                using (Font drawFont = new Font("Tahoma", 10, FontStyle.Regular))
                {
                    Brush textBrush = new SolidBrush(ForeColor);
                    g.DrawString(Text, drawFont, textBrush, new Point(16, 7), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
                }

                if (Checked)
                {
                    Rectangle chkPoly = new Rectangle(checkBoxRectangle.X + checkBoxRectangle.Width / 4, checkBoxRectangle.Y + checkBoxRectangle.Height / 4, checkBoxRectangle.Width / 2, checkBoxRectangle.Height / 2);
                    Point[] poly =
                    {
                        new Point(chkPoly.X, chkPoly.Y + chkPoly.Height / 2),
                        new Point(chkPoly.X + chkPoly.Width / 2, chkPoly.Y + chkPoly.Height),
                        new Point(chkPoly.X + chkPoly.Width, chkPoly.Y)
                    };

                    g.SmoothingMode = SmoothingMode.HighQuality;
                    using Pen p1 = new Pen(CheckColor, 2); // Используем CheckColor
                    for (int i = 0; i < poly.Length - 1; i++)
                    {
                        g.DrawLine(p1, poly[i], poly[i + 1]);
                    }
                }
            }
            else // Для стиля Flat
            {
                using (var bgBrush = new SolidBrush(BackColor))
                {
                    g.FillRectangle(bgBrush, ClientRectangle);
                }

                using (Pen borderPen = new Pen(Color.Gray))
                {
                    g.DrawRectangle(borderPen, checkBoxRectangle);
                }

                if (Checked)
                {
                    Rectangle chkPoly = new Rectangle(checkBoxRectangle.X + checkBoxRectangle.Width / 4, checkBoxRectangle.Y + checkBoxRectangle.Height / 4, checkBoxRectangle.Width / 2, checkBoxRectangle.Height / 2);
                    Point[] poly =
                    {
                        new Point(chkPoly.X, chkPoly.Y + chkPoly.Height / 2),
                        new Point(chkPoly.X + chkPoly.Width / 2, chkPoly.Y + chkPoly.Height),
                        new Point(chkPoly.X + chkPoly.Width, chkPoly.Y)
                    };

                    using Pen checkPen = new Pen(CheckColor, 2); // Используем CheckColor
                    for (int i = 0; i < poly.Length - 1; i++)
                    {
                        g.DrawLine(checkPen, poly[i], poly[i + 1]);
                    }
                }

                using Font drawFont = new Font("Tahoma", 10, FontStyle.Regular);
                using Brush textBrush = new SolidBrush(ForeColor);
                g.DrawString(Text, drawFont, textBrush, new Point(16, 7), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
            }
        }

        e.Graphics.DrawImage(b, 0, 0);
    }
    #endregion
}


[DefaultEvent("CheckedChanged")]
public class UbuntuRadioButton : Control
{
    #region " Control Help - MouseState & Flicker Control"
    private bool _checked;
    private Color _checkedColor = Color.FromArgb(247, 150, 116); // Цвет для состояния Checked

    public bool Checked
    {
        get => _checked;
        set
        {
            _checked = value;
            InvalidateControls();
            CheckedChanged?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }
    }

    public event EventHandler CheckedChanged;

    public override Color ForeColor // Переопределяем свойство ForeColor
    {
        get => base.ForeColor;
        set
        {
            base.ForeColor = value;
            Invalidate();
        }
    }

    public Color CheckedColor
    {
        get => _checkedColor;
        set
        {
            _checkedColor = value;
            Invalidate(); // Обновляем отображение при изменении цвета
        }
    }

    public UbuntuRadioButton()
    {
        TabStop = false;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.WhiteSmoke;
        Size = new Size(150, 16);
        ForeColor = SystemColors.ControlText; // Устанавливаем цвет по умолчанию
    }

    private void InvalidateControls()
    {
        if (!IsHandleCreated || !_checked) return;

        foreach (Control c in Parent.Controls)
        {
            if (c != this && c is UbuntuRadioButton radioButton)
            {
                radioButton.Checked = false;
            }
        }
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        Invalidate();
        base.OnMouseEnter(e);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Checked = true; // Переключаем состояние нажатия
            Invalidate(); // Обновляем отображение
        }
        base.OnMouseDown(e);
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        Invalidate();
        base.OnMouseLeave(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Bitmap b = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(b);
        Rectangle radioBtnRectangle = new Rectangle(0, 0, Height - 1, Height - 1);
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.Clear(BackColor);

        using (LinearGradientBrush bgGrad = new LinearGradientBrush(radioBtnRectangle, Color.FromArgb(102, 101, 96), Color.FromArgb(76, 75, 71), 90F))
        {
            g.FillEllipse(bgGrad, radioBtnRectangle);
            g.DrawEllipse(new Pen(Color.Gray), new Rectangle(1, 1, Height - 3, Height - 3));
            g.DrawEllipse(new Pen(Color.FromArgb(42, 47, 49)), radioBtnRectangle);
        }

        if (Checked)
        {
            using LinearGradientBrush chkGrad = new LinearGradientBrush(new Rectangle(4, 4, Height - 9, Height - 9), CheckedColor, Color.FromArgb(197, 100, 66), 90F);
            g.FillEllipse(chkGrad, new Rectangle(4, 4, Height - 9, Height - 9));
        }

        using (Font drawFont = new Font("Tahoma", 10, FontStyle.Regular))
        {
            using Brush textBrush = new SolidBrush(ForeColor); // Используем ForeColor
            g.DrawString(Text, drawFont, textBrush, new Point(16, 1), new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Near });
        }

        e.Graphics.DrawImage(b, 0, 0);
    }
    #endregion
}


//public class UbuntuControlBox : Control
//{
//    #region "MouseStates"
//    private MouseState state = MouseState.None;
//    private int x;
//    private Rectangle closeBtn = new Rectangle(43, 2, 17, 17);
//    private Rectangle minBtn = new Rectangle(3, 2, 17, 17);
//    private Rectangle maxBtn = new Rectangle(23, 2, 17, 17);
//    private Rectangle bgr = new Rectangle(0, 0, 62, 21);

//    protected override void OnMouseDown(MouseEventArgs e)
//    {
//        base.OnMouseDown(e);
//        if (x > 3 && x < 20)
//        {
//            FindForm().WindowState = FormWindowState.Minimized;
//        }
//        else if (x > 23 && x < 40)
//        {
//            if (FindForm().WindowState == FormWindowState.Maximized)
//            {
//                FindForm().WindowState = FormWindowState.Normal;
//            }
//            else
//            {
//                FindForm().WindowState = FormWindowState.Maximized;
//            }
//        }
//        else if (x > 43 && x < 60)
//        {
//            FindForm().Close();
//        }
//        state = MouseState.Down;
//        Invalidate();
//    }

//    protected override void OnMouseUp(MouseEventArgs e)
//    {
//        base.OnMouseUp(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseEnter(EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseLeave(EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        state = MouseState.None;
//        Invalidate();
//    }

//    protected override void OnMouseMove(MouseEventArgs e)
//    {
//        base.OnMouseMove(e);
//        x = e.Location.X;
//        Invalidate();
//    }
//    #endregion

//    public UbuntuControlBox()
//    {
//        TabStop = false;
//        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
//        BackColor = Color.Transparent;
//        DoubleBuffered = true;
//        Font = new Font("Marlett", 7);
//        Anchor = AnchorStyles.Top | AnchorStyles.Right;

//        // Установка фиксированного размера
//        Size = new Size(67, 23); // Фиксированный размер
//        MaximumSize = Size; // Максимальный размер
//        MinimumSize = Size; // Минимальный размер
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        Bitmap b = new Bitmap(Width, Height);
//        using (Graphics g = Graphics.FromImage(b))
//        {
//            base.OnPaint(e);

//            g.SmoothingMode = SmoothingMode.AntiAlias;
//            g.Clear(Color.Transparent);

//            // Draw background
//            using (LinearGradientBrush bg0 = new LinearGradientBrush(bgr, Color.FromArgb(60, 59, 55), Color.FromArgb(60, 59, 55), 90F))
//            {
//                g.FillPath(bg0, Draw.RoundRect(bgr, 10));
//            }

//            // Draw buttons
//            DrawButton(g, minBtn, "0", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//            DrawButton(g, maxBtn, "1", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//            DrawButton(g, closeBtn, "r", Color.FromArgb(247, 150, 116), Color.FromArgb(223, 81, 6));

//            // Draw based on mouse state
//            switch (state)
//            {
//                case MouseState.None:
//                    DrawButtons(g);
//                    break;

//                case MouseState.Over:
//                    DrawButtonsOver(g);
//                    break;

//                case MouseState.Down:
//                    DrawButtonsDown(g);
//                    break;
//            }

//            e.Graphics.DrawImage(b, 0, 0);
//        }
//    }

//    private void DrawButtons(Graphics g)
//    {
//        DrawButton(g, minBtn, "0", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//        DrawButton(g, maxBtn, "1", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//        DrawButton(g, closeBtn, "r", Color.FromArgb(247, 150, 116), Color.FromArgb(223, 81, 6));
//    }

//    private void DrawButtonsOver(Graphics g)
//    {
//        if (x > 3 && x < 20)
//            DrawButtonOver(g, minBtn, "0");

//        if (x > 23 && x < 40)
//            DrawButtonOver(g, maxBtn, "1");

//        if (x > 43 && x < 60)
//            DrawButtonOver(g, closeBtn, "r");
//    }

//    private void DrawButtonsDown(Graphics g)
//    {
//        if (x > 3 && x < 20)
//            DrawButtonDown(g, minBtn, "0");

//        if (x > 23 && x < 40)
//            DrawButtonDown(g, maxBtn, "1");

//        if (x > 43 && x < 60)
//            DrawButtonDown(g, closeBtn, "r");
//    }

//    private void DrawButton(Graphics g, Rectangle btnRect, string text, Color color1, Color color2)
//    {
//        using (GraphicsPath path = new GraphicsPath())
//        {
//            path.AddEllipse(btnRect);
//            using (LinearGradientBrush brush = new LinearGradientBrush(btnRect, color1, color2, 90F))
//            {
//                g.FillPath(brush, path);
//                g.DrawPath(Pens.DimGray, path);

//                // Индикатор размера текста
//                var textSize = g.MeasureString(text, Font);

//                // Центрирование текста по горизонтали и вертикали
//                float textX = btnRect.X + (btnRect.Width - textSize.Width) / 2 + 1.5f; // Смещение текста влево
//                float textY = btnRect.Y + (btnRect.Height - textSize.Height) / 2;

//                // Убедитесь, что текст не выходит за пределы кнопки
//                textX = Math.Max(btnRect.X + 2, textX);
//                textY = Math.Max(btnRect.Y + 2, textY);

//                // Центрирование текста по вертикали
//                if (textSize.Height < btnRect.Height)
//                {
//                    textY += (btnRect.Height - textSize.Height) / 4;
//                }

//                g.DrawString(text, Font, new SolidBrush(Color.FromArgb(58, 57, 53)), new PointF(textX, textY));
//            }
//        }
//    }

//    private void DrawButtonOver(Graphics g, Rectangle btnRect, string text)
//    {
//        DrawButton(g, btnRect, text, Color.FromArgb(172, 171, 166), Color.FromArgb(76, 75, 71));
//    }

//    private void DrawButtonDown(Graphics g, Rectangle btnRect, string text)
//    {
//        DrawButton(g, btnRect, text, Color.FromArgb(200, 116, 83), Color.FromArgb(194, 101, 65));
//    }
//}

public class UbuntuTheme : ContainerControl
{
    private Point mouseP = new Point(0, 0);
    private bool cap = false;
    private int moveHeight = 26;
    private bool showIcon = true;
    private Icon formIcon;
    private Color headerColor = Color.FromArgb(87, 86, 81); // (2) Цвет панели заголовка
    private Color textColor = Color.WhiteSmoke; // (1) Цвет текста

    public Icon FormIcon
    {
        get => formIcon;
        set
        {
            formIcon = value;
            Invalidate();
        }
    }

    public bool ShowIcon
    {
        get => showIcon;
        set
        {
            showIcon = value;
            Invalidate();
        }
    }

    public new string Text
    {
        get => base.Text;
        set
        {
            base.Text = value;
            Invalidate();
        }
    }

    // Свойство для изменения цвета текста
    public Color TextColor
    {
        get => textColor;
        set
        {
            textColor = value;
            Invalidate();
        }
    }

    // Свойство для изменения цвета панели заголовка
    public Color HeaderColor
    {
        get => headerColor;
        set
        {
            headerColor = value;
            Invalidate();
        }
    }

    public UbuntuTheme()
    {
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.FromArgb(25, 25, 25);
        DoubleBuffered = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        using Bitmap b = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(b);
        Rectangle topBar = new Rectangle(0, 0, Width - 1, 30);
        Rectangle body = new Rectangle(0, 5, Width - 1, Height - 6);

        g.Clear(Color.FromArgb(25, 25, 25));
        g.SmoothingMode = SmoothingMode.HighSpeed;

        using (LinearGradientBrush lbb = new LinearGradientBrush(body, Color.FromArgb(242, 241, 240), Color.FromArgb(240, 240, 238), 90F))
        {
            g.FillPath(lbb, RoundRect(body, 1));
            g.DrawPath(new Pen(Color.FromArgb(60, 60, 60)), RoundRect(body, 1));
        }

        using (LinearGradientBrush lgb = new LinearGradientBrush(topBar, headerColor, Color.FromArgb(60, 59, 55), 90F)) // Используем headerColor
        {
            g.FillPath(lgb, RoundRect(topBar, 4));
            g.DrawPath(Pens.Black, RoundRect(topBar, 3));
        }

        int textOffset = showIcon ? 25 : 5;

        using (Font drawFont = new Font("Tahoma", 10, FontStyle.Regular))
        {
            g.DrawString(Text, drawFont, new SolidBrush(textColor), new Rectangle(textOffset, 0, Width - 1, 27),
                          new StringFormat() { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center });
        }

        if (formIcon != null && showIcon)
        {
            g.DrawIcon(formIcon, new Rectangle(5, 5, 16, 16));
        }

        e.Graphics.DrawImage((Image)b.Clone(), 0, 0);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left && new Rectangle(0, 0, Width, moveHeight).Contains(e.Location))
        {
            cap = true;
            mouseP = e.Location;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        cap = false;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (cap)
        {
            Parent.Location = new Point(MousePosition.X - mouseP.X, MousePosition.Y - mouseP.Y);
        }
    }

    protected override void OnCreateControl()
    {
        base.OnCreateControl();
        ParentForm.FormBorderStyle = FormBorderStyle.None;
        ParentForm.TransparencyKey = Color.Fuchsia;
        Dock = DockStyle.Fill;
    }

    private void UbuntuTheme_SizeChanged(object sender, EventArgs e)
    {
        // Обработчик события изменения размера
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

// test

//public class UbuntuControlBox : Control
//{
//    #region "MouseStates"
//    private MouseState state = MouseState.None;
//    private int x;
//    private Rectangle closeBtn = new Rectangle(43, 2, 17, 17);
//    private Rectangle minBtn = new Rectangle(3, 2, 17, 17);
//    private Rectangle maxBtn = new Rectangle(23, 2, 17, 17);
//    private Rectangle bgr = new Rectangle(0, 0, 62, 21);

//    protected override void OnMouseDown(MouseEventArgs e)
//    {
//        base.OnMouseDown(e);
//        if (x > 3 && x < 20)
//        {
//            FindForm().WindowState = FormWindowState.Minimized;
//        }
//        else if (x > 23 && x < 40)
//        {
//            if (FindForm().WindowState == FormWindowState.Maximized)
//            {
//                FindForm().WindowState = FormWindowState.Normal;
//            }
//            else
//            {
//                FindForm().WindowState = FormWindowState.Maximized;
//            }
//        }
//        else if (x > 43 && x < 60)
//        {
//            FindForm().Close();
//        }
//        state = MouseState.Down;
//        Invalidate();
//    }

//    protected override void OnMouseUp(MouseEventArgs e)
//    {
//        base.OnMouseUp(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseEnter(EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        state = MouseState.Over;
//        Invalidate();
//    }

//    protected override void OnMouseLeave(EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        state = MouseState.None; // Здесь можно не очищать состояние, если нужно
//        Invalidate();
//    }

//    protected override void OnMouseMove(MouseEventArgs e)
//    {
//        base.OnMouseMove(e);
//        x = e.Location.X;
//        Invalidate();
//    }
//    #endregion

//    public UbuntuControlBox()
//    {
//        TabStop = false;
//        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
//        BackColor = Color.Transparent;
//        DoubleBuffered = true;
//        Font = new Font("Marlett", 7);
//        Anchor = AnchorStyles.Top | AnchorStyles.Right;

//        // Установка фиксированного размера
//        Size = new Size(67, 23); // Фиксированный размер
//        MaximumSize = Size; // Максимальный размер
//        MinimumSize = Size; // Минимальный размер
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        Bitmap b = new Bitmap(Width, Height);
//        using (Graphics g = Graphics.FromImage(b))
//        {
//            base.OnPaint(e);

//            g.Clear(Color.Transparent); // Убираем фон полностью
//            g.SmoothingMode = SmoothingMode.AntiAlias; // Включаем сглаживание

//            // Рисуем кнопки в зависимости от состояния мыши
//            DrawButtons(g);

//            // Рисуем кнопки в состоянии "наведено" или "нажато"
//            if (state == MouseState.Over)
//            {
//                DrawButtonsOver(g);
//            }
//            else if (state == MouseState.Down)
//            {
//                DrawButtonsDown(g);
//            }

//            e.Graphics.DrawImage(b, 0, 0);
//        }
//    }

//    private void DrawButtons(Graphics g)
//    {
//        DrawButton(g, minBtn, "0", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//        DrawButton(g, maxBtn, "1", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
//        DrawButton(g, closeBtn, "r", Color.FromArgb(247, 150, 116), Color.FromArgb(223, 81, 6));
//    }

//    private void DrawButtonsOver(Graphics g)
//    {
//        // Перерисовываем кнопки без обнуления предыдущих
//        if (x > 3 && x < 20)
//            DrawButtonOver(g, minBtn, "0");

//        if (x > 23 && x < 40)
//            DrawButtonOver(g, maxBtn, "1");

//        if (x > 43 && x < 60)
//            DrawButtonOver(g, closeBtn, "r");
//    }

//    private void DrawButtonsDown(Graphics g)
//    {
//        // Перерисовываем кнопки без обнуления предыдущих
//        if (x > 3 && x < 20)
//            DrawButtonDown(g, minBtn, "0");

//        if (x > 23 && x < 40)
//            DrawButtonDown(g, maxBtn, "1");

//        if (x > 43 && x < 60)
//            DrawButtonDown(g, closeBtn, "r");
//    }

//    private void DrawButton(Graphics g, Rectangle btnRect, string text, Color color1, Color color2)
//    {
//        using (GraphicsPath path = new GraphicsPath())
//        {
//            path.AddEllipse(btnRect);
//            using (LinearGradientBrush brush = new LinearGradientBrush(btnRect, color1, color2, 90F))
//            {
//                g.FillPath(brush, path);
//                g.DrawPath(Pens.DimGray, path);

//                // Индикатор размера текста
//                var textSize = g.MeasureString(text, Font);

//                // Центрирование текста по горизонтали и вертикали
//                float textX = btnRect.X + (btnRect.Width - textSize.Width) / 2 + 1.5f; // Смещение текста влево
//                float textY = btnRect.Y + (btnRect.Height - textSize.Height) / 2;

//                // Убедитесь, что текст не выходит за пределы кнопки
//                textX = Math.Max(btnRect.X + 2, textX);
//                textY = Math.Max(btnRect.Y + 2, textY);

//                // Центрирование текста по вертикали
//                if (textSize.Height < btnRect.Height)
//                {
//                    textY += (btnRect.Height - textSize.Height) / 4;
//                }

//                g.DrawString(text, Font, new SolidBrush(Color.FromArgb(58, 57, 53)), new PointF(textX, textY));
//            }
//        }
//    }

//    private void DrawButtonOver(Graphics g, Rectangle btnRect, string text)
//    {
//        DrawButton(g, btnRect, text, Color.FromArgb(172, 171, 166), Color.FromArgb(76, 75, 71));
//    }

//    private void DrawButtonDown(Graphics g, Rectangle btnRect, string text)
//    {
//        DrawButton(g, btnRect, text, Color.FromArgb(200, 116, 83), Color.FromArgb(194, 101, 65));
//    }



// New Control Box

// test

public class UbuntuControlBox : Control
{
    #region "MouseStates"
    private MouseState state = MouseState.None;
    private int x;
    private Rectangle closeBtn = new Rectangle(43, 2, 17, 17);
    private Rectangle minBtn = new Rectangle(3, 2, 17, 17);
    private Rectangle maxBtn = new Rectangle(23, 2, 17, 17);
    private Rectangle bgr = new Rectangle(0, 0, 62, 21);

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        switch (x)
        {
            case > 3 and < 20:
                FindForm().WindowState = FormWindowState.Minimized;
                break;
            case > 23 and < 40:
                FindForm().WindowState = FindForm().WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                break;
            case > 43 and < 60:
                FindForm().Close();
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

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        // Обновляем размеры кнопок в зависимости от новой ширины
        UpdateButtonLocations();
    }

    private void UpdateButtonLocations()
    {
        closeBtn = new Rectangle(Width - 22, 2, 17, 17);
        maxBtn = new Rectangle(Width - 42, 2, 17, 17);
        minBtn = new Rectangle(Width - 62, 2, 17, 17);
    }
    #endregion

    public UbuntuControlBox()
    {
        TabStop = false;
        SetStyle(ControlStyles.UserPaint | ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
        DoubleBuffered = true;
        Font = new Font("Marlett", 7);
        Anchor = AnchorStyles.Top | AnchorStyles.Right;

        Size = new Size(67, 23);
        MaximumSize = Size;
        MinimumSize = Size;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Bitmap b = new Bitmap(Width, Height);
        using Graphics g = Graphics.FromImage(b);
        base.OnPaint(e);

        g.Clear(Color.Transparent); // Убираем фон
        g.SmoothingMode = SmoothingMode.AntiAlias; // Сглаживание

        // Рисуем кнопки в зависимости от состояния
        DrawButtons(g);
        if (state == MouseState.Over) DrawButtonsOver(g);
        else if (state == MouseState.Down) DrawButtonsDown(g);

        e.Graphics.DrawImage(b, 0, 0);
    }

    private void DrawButtons(Graphics g)
    {
        DrawButton(g, minBtn, "0", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
        DrawButton(g, maxBtn, "1", Color.FromArgb(152, 151, 146), Color.FromArgb(56, 55, 51));
        DrawButton(g, closeBtn, "r", Color.FromArgb(247, 150, 116), Color.FromArgb(223, 81, 6));
    }

    private void DrawButtonsOver(Graphics g)
    {
        if (x is > 3 and < 20) DrawButtonOver(g, minBtn, "0");
        if (x is > 23 and < 40) DrawButtonOver(g, maxBtn, "1");
        if (x is > 43 and < 60) DrawButtonOver(g, closeBtn, "r");
    }

    private void DrawButtonsDown(Graphics g)
    {
        if (x is > 3 and < 20) DrawButtonDown(g, minBtn, "0");
        if (x is > 23 and < 40) DrawButtonDown(g, maxBtn, "1");
        if (x is > 43 and < 60) DrawButtonDown(g, closeBtn, "r");
    }

    private void DrawButton(Graphics g, Rectangle btnRect, string text, Color color1, Color color2)
    {
        using GraphicsPath path = new GraphicsPath();
        path.AddEllipse(btnRect);
        using LinearGradientBrush brush = new LinearGradientBrush(btnRect, color1, color2, 90F);
        g.FillPath(brush, path);
        g.DrawPath(Pens.DimGray, path);

        var textSize = g.MeasureString(text, Font);
        float textX = btnRect.X + (btnRect.Width - textSize.Width) / 2 + 1.5f;
        float textY = btnRect.Y + (btnRect.Height - textSize.Height) / 2;

        textX = Math.Max(btnRect.X + 2, textX);
        textY = Math.Max(btnRect.Y + 2, textY);
        if (textSize.Height < btnRect.Height)
        {
            textY += (btnRect.Height - textSize.Height) / 4;
        }

        g.DrawString(text, Font, new SolidBrush(Color.FromArgb(58, 57, 53)), new PointF(textX, textY));
    }

    private void DrawButtonOver(Graphics g, Rectangle btnRect, string text)
    {
        DrawButton(g, btnRect, text, Color.FromArgb(172, 171, 166), Color.FromArgb(76, 75, 71));
    }

    private void DrawButtonDown(Graphics g, Rectangle btnRect, string text)
    {
        DrawButton(g, btnRect, text, Color.FromArgb(200, 116, 83), Color.FromArgb(194, 101, 65));
    }
}