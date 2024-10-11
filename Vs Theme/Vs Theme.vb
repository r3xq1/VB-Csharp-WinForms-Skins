Imports System.Drawing.Drawing2D
Imports System.ComponentModel
Imports System.Windows.Forms

<DesignerCategory("Code")>
Public Class Draw
    Shared Sub Gradient(g As Graphics, c1 As Color, c2 As Color, x As Integer, y As Integer, width As Integer, height As Integer)
        Dim R As New Rectangle(x, y, width, height)
        Using T As New LinearGradientBrush(R, c1, c2, LinearGradientMode.Vertical)
            g.FillRectangle(T, R)
        End Using
    End Sub

    Shared Sub Blend(g As Graphics, c1 As Color, c2 As Color, c3 As Color, c As Single, d As Integer, x As Integer, y As Integer, width As Integer, height As Integer)
        Dim V As New ColorBlend(3)
        V.Colors = New Color() {c1, c2, c3}
        V.Positions = New Single() {0, c, 1}
        Dim R As New Rectangle(x, y, width, height)
        Using T As New LinearGradientBrush(R, c1, c1, CType(d, LinearGradientMode))
            T.InterpolationColors = V
            g.FillRectangle(T, R)
        End Using
    End Sub
End Class

<DesignerCategory("Code")>
Public Class VSTheme
    Inherits ContainerControl

    Private _TitleHeight As Integer = 23
    Private _TitleAlign As HorizontalAlignment

    <Category("Appearance")>
    Public Property TitleHeight() As Integer
        Get
            Return _TitleHeight
        End Get
        Set(value As Integer)
            If value > Height Then value = Height
            If value < 2 Then value = 2
            _TitleHeight = value
            Invalidate()
        End Set
    End Property

    <Category("Appearance")>
    Public Property TitleAlign() As HorizontalAlignment
        Get
            Return _TitleAlign
        End Get
        Set(value As HorizontalAlignment)
            _TitleAlign = value
            Invalidate()
        End Set
    End Property

    Private Tile As Image

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.DoubleBuffer, True)
        Size = New Size(300, 100)
        Me.AutoScroll = True ' Включаем прокрутку для детей

        Using B As New Bitmap(3, 3)
            Using G = Graphics.FromImage(B)
                G.Clear(Color.FromArgb(53, 67, 88))
                G.DrawLine(New Pen(Color.FromArgb(33, 46, 67)), 0, 0, 2, 2)
                Tile = B.Clone()
            End Using
        End Using
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Using B As New Bitmap(Width, Height)
            Using G As Graphics = Graphics.FromImage(B)
                Using T As New TextureBrush(Tile, 0)
                    G.FillRectangle(T, 0, _TitleHeight, Width, Height - _TitleHeight)
                End Using

                Draw.Gradient(G, Color.FromArgb(249, 245, 226), Color.FromArgb(255, 232, 165), 0, 0, Width, _TitleHeight)
                G.FillRectangle(New SolidBrush(Color.FromArgb(100, 255, 255, 255)), 0, 0, Width, CInt(_TitleHeight / 2))

                G.DrawRectangle(New Pen(Color.FromArgb(255, 232, 165), 2), 1, _TitleHeight - 1, Width - 2, Height - _TitleHeight)

                G.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
                Dim S As SizeF = G.MeasureString(Text, Font)
                Dim O As Integer = If(_TitleAlign = HorizontalAlignment.Center, (Width / 2) - (S.Width / 2), If(_TitleAlign = HorizontalAlignment.Right, Width - S.Width - 6, 6))

                G.DrawString(Text, Font, New SolidBrush(Color.FromArgb(111, 88, 38)), O, CInt((_TitleHeight / 2) - (S.Height / 2)))

                e.Graphics.DrawImage(B, 0, 0)
            End Using
        End Using
    End Sub

    Protected Overrides Function IsInputChar(ByVal charCode As Char) As Boolean
        Return True
    End Function

    Protected Overrides Function IsInputKey(ByVal keyData As Keys) As Boolean
        Return MyBase.IsInputKey(keyData)
    End Function

    Protected Overrides Sub OnControlAdded(ByVal e As ControlEventArgs)
        MyBase.OnControlAdded(e)
        e.Control.BringToFront()
    End Sub
End Class

<DesignerCategory("Code")>
Public Class VSButton
    Inherits Control

    Private State As Integer

    Sub New()
        SetStyle(ControlStyles.AllPaintingInWmPaint Or ControlStyles.UserPaint Or ControlStyles.ResizeRedraw Or ControlStyles.DoubleBuffer, True)
        Size = New Size(100, 40)
        ForeColor = Color.FromArgb(111, 88, 38)
    End Sub

    Protected Overrides Sub OnMouseEnter(e As EventArgs)
        State = 1
        Invalidate()
        MyBase.OnMouseEnter(e)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        State = 2
        Invalidate()
        MyBase.OnMouseDown(e)
    End Sub

    Protected Overrides Sub OnMouseLeave(e As EventArgs)
        State = 0
        Invalidate()
        MyBase.OnMouseLeave(e)
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        State = 1
        Invalidate()
        MyBase.OnMouseUp(e)
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        Using B As New Bitmap(Width, Height)
            Using G As Graphics = Graphics.FromImage(B)
                If State = 2 Then
                    Draw.Gradient(G, Color.FromArgb(255, 232, 165), Color.FromArgb(249, 245, 226), 0, 0, Width, Height)
                Else
                    Draw.Gradient(G, Color.FromArgb(249, 245, 226), Color.FromArgb(255, 232, 165), 0, 0, Width, Height)
                End If

                If State < 2 Then
                    G.FillRectangle(New SolidBrush(Color.FromArgb(100, 255, 255, 255)), 0, 0, Width, CInt(Height / 2))
                End If

                G.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
                Dim S As SizeF = G.MeasureString(Text, Font)

                If Not String.IsNullOrEmpty(Text) AndAlso Font IsNot Nothing Then
                    Dim textX As Single = (Width / 2) - (S.Width / 2)
                    Dim textY As Single = (Height / 2) - (S.Height / 2)

                    G.DrawString(Text, Font, New SolidBrush(ForeColor), textX, textY)
                End If

                G.DrawRectangle(New Pen(Color.FromArgb(249, 245, 226)), 0, 0, Width - 1, Height - 1)

                e.Graphics.DrawImage(B, 0, 0)
            End Using
        End Using
    End Sub
End Class
