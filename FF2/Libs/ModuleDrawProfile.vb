
Option Explicit On

Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Windows.Forms
Imports FaarFieldAnalysis
Imports System.Windows.Forms.DataVisualization.Charting
Imports System.Data

Namespace Libs



    Public Module ModuleDrawProfile

        Public XYCoord(100, 100) As Single

        ReadOnly _drawPenAc() As Pen = New Pen() _
            {Pens.Blue, Pens.Red, Pens.DarkGreen, Pens.Yellow, Pens.Violet,
             Pens.Orange, Pens.CadetBlue, Pens.BlanchedAlmond, Pens.Chocolate, Pens.Cyan}
        ReadOnly _drawPenCdf As New Pen(Color.Black, 1)
        ReadOnly _displayCdf(80) As Boolean
        ReadOnly _acCdf(80, 2) As Single
        Dim _acCdf2(80, 2) As Single
        Dim I80 As Integer = 80
        ReadOnly _maxAc1 As Integer = 10
        Dim I3 As Integer
        ' Dim gACN_copy(modPCN_ZZZ.Save_NAC) As Single
        Dim LibPCNindex(6) As Integer
        Dim TopIndex1 As Integer
        Dim TopIndex2 As Integer
        Dim factory As IFaarFieldModelFactory

        Dim PCNgZero As Integer = 0
        Dim MaxCDFNumber As Single = 0


        Public Sub PaintGear(g As Graphics, airplane As IAirplaneInfo, pictureBox As PictureBox, font As Font, measurementSystem As IMeasurmentSystem)

            Dim XCoordTracker As Integer = 1
            Dim YCoordTracker As Integer = 1
            For i = 0 To 100
                For j = 0 To 100
                    XYCoord(i, j) = 0.0!
                Next
            Next

            ' ── Rendering setup ──
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic

            Dim W As Integer = pictureBox.Width
            Dim H As Integer = pictureBox.Height
            Dim isMetric As Boolean = Not TypeOf (measurementSystem) Is UsCustomary

            ' Background: white
            g.Clear(Color.White)

            If airplane Is Nothing Then Return

            ' ── Unit label ──
            Dim unitLabel As String = If(isMetric, "mm", "in.")

            ' ── Compute data ranges from wheel coordinates ──
            Dim Ts As Double = CDbl(Format(airplane.Ts, "0.0"))
            Dim TT As Double = CDbl(Format(airplane.Tt.UsCustomary, "0.0"))
            Dim offsetx As Double = (Ts + TT) / 2

            Dim maxDim As Single = 0
            Dim maxY As Single = Single.MinValue
            Dim minY As Single = Single.MaxValue
            For Each wc In airplane.WheelCoordinates
                Dim wx As Single = wc.X.GetValue(measurementSystem) + CSng(offsetx)
                If wx > maxDim Then maxDim = wx
                Dim wy As Single = wc.Y.GetValue(measurementSystem)
                If wy > maxDim Then maxDim = wy
                If wy < minY Then minY = wy
                If wy > maxY Then maxY = wy
            Next

            ' ── Scale calculation (preserve original logic for accuracy) ──
            Dim scale As Single = 0
            Dim scaleRatio As Single = 1
            If Not isMetric Then
                If airplane.Name.Contains("B-52") Then
                    scale = 325
                ElseIf airplane.Name.Contains("A380") Then
                    scale = 150
                Else
                    scale = 125
                End If
                scaleRatio = 125 / scale
            Else
                If airplane.Name.Contains("B-52") Then
                    scale = 8255
                ElseIf airplane.Name.Contains("A380") Then
                    scale = 3810
                Else
                    scale = 3175
                End If
                scaleRatio = 3175 / scale
            End If
            Dim axisRatio As Single = CSng(150 / scale)
            Dim ScaledPictureBox As Single = 320.0F / 270.0F

            ' ── Pixel coordinate lambdas ──
            Dim cx As Single = CSng(W / 2)
            Dim cy As Single = CSng(H / 2)
            Dim toX = Function(val As Single) cx + val * scaleRatio * ScaledPictureBox
            Dim toY = Function(val As Single) cy - val * scaleRatio * ScaledPictureBox

            ' ── Fonts ──
            Dim titleFont As New Font("Segoe UI", 11.0F, FontStyle.Bold)
            Dim subtitleFont As New Font("Segoe UI", 9.0F)
            Dim tickFont As New Font("Segoe UI", 10.0F)
            Dim labelFont As New Font("Segoe UI", 11.0F, FontStyle.Regular)
            Dim coordFont As New Font("Consolas", 8.0F)
            Dim badgeFont As New Font("Segoe UI", 8.5F, FontStyle.Bold)

            ' ── Colors ──
            Dim teal As Color = Color.FromArgb(0, 121, 107)       ' #00796B
            Dim tealLight As Color = Color.FromArgb(178, 223, 219) ' #B2DFDB
            Dim gridColor As Color = Color.FromArgb(230, 230, 225)
            Dim axisColor As Color = Color.FromArgb(120, 120, 115)
            Dim wheelFill As Color = Color.FromArgb(160, 0, 105, 92)
            Dim wheelStroke As Color = Color.FromArgb(0, 77, 64)
            Dim mirrorFill As Color = Color.FromArgb(100, 0, 105, 92)
            Dim mirrorStroke As Color = Color.FromArgb(120, 0, 77, 64)
            Dim evalColor As Color = Color.FromArgb(220, 50, 50)

            ' ── Dot grid ──
            Dim gridPen As New Pen(gridColor, 0.5F)
            Dim tickStep As Single = If(isMetric, 3.333333F, 2.0F)
            For i As Single = -10 To 10 Step tickStep
                If Math.Abs(i) < 0.01 Then Continue For
                Dim d As Single = scale * i / 5
                Dim px As Single = CSng(cx + i * scale * axisRatio / 5)
                Dim py As Single = CSng(cy + i * scale * axisRatio / 5)
                ' Vertical grid line (light dots)
                If px > 10 AndAlso px < W - 10 Then
                    Dim dotPen As New Pen(Color.FromArgb(50, gridColor.R, gridColor.G, gridColor.B), 0.5F)
                    dotPen.DashStyle = Drawing2D.DashStyle.Dot
                    g.DrawLine(dotPen, px, 48, px, CSng(H - 20))
                    dotPen.Dispose()
                End If
                ' Horizontal grid line
                If py > 48 AndAlso py < H - 20 Then
                    Dim dotPen As New Pen(Color.FromArgb(50, gridColor.R, gridColor.G, gridColor.B), 0.5F)
                    dotPen.DashStyle = Drawing2D.DashStyle.Dot
                    g.DrawLine(dotPen, 10, py, CSng(W - 10), py)
                    dotPen.Dispose()
                End If
            Next

            ' ── Axes ──
            Dim axisPen As New Pen(axisColor, 1.2F)
            g.DrawLine(axisPen, 10, cy, CSng(W - 10), cy)    ' horizontal
            g.DrawLine(axisPen, cx, 48, cx, CSng(H - 20))    ' vertical

            ' ── Grid lines + Tick marks and labels ──
            gridPen.DashStyle = Drawing2D.DashStyle.Dot
            Dim tickBrush As New SolidBrush(Color.FromArgb(100, 100, 95))
            For i As Single = -10 To 10 Step tickStep
                If Math.Abs(i) < 0.01 Then Continue For
                Dim d As Single = scale * i / 5
                Dim dlabel As String = d.ToString("f0")

                ' Horizontal axis ticks + vertical grid line
                Dim px As Single = CSng(cx + i * scale * axisRatio / 5)
                If px > 10 AndAlso px < W - 10 Then
                    g.DrawLine(gridPen, px, 48, px, CSng(H - 20))
                    g.DrawLine(axisPen, px, cy - 4, px, cy + 4)
                    Dim sz = g.MeasureString(dlabel, tickFont)
                    g.DrawString(dlabel, tickFont, tickBrush, px - sz.Width / 2, cy + 7)
                End If

                ' Vertical axis ticks + horizontal grid line (note: Y is inverted so negate label)
                Dim py As Single = CSng(cy + i * scale * axisRatio / 5)
                If py > 48 AndAlso py < H - 20 Then
                    g.DrawLine(gridPen, 10, py, CSng(W - 10), py)
                    g.DrawLine(axisPen, cx - 4, py, cx + 4, py)
                    Dim negD As Single = -d
                    Dim negLabel As String = negD.ToString("f0")
                    Dim sz = g.MeasureString(negLabel, tickFont)
                    g.DrawString(negLabel, tickFont, tickBrush, cx + 7, py - sz.Height / 2)
                End If
            Next

            ' ── Axis unit labels ──
            Dim axUnitBrush As New SolidBrush(Color.FromArgb(140, 140, 135))
            g.DrawString("Lateral (" & unitLabel & ")", labelFont, axUnitBrush, CSng(W - 120), cy - 18)
            Dim sfVert As New StringFormat()
            sfVert.FormatFlags = StringFormatFlags.DirectionVertical
            g.DrawString("Longitudinal (" & unitLabel & ")", labelFont, axUnitBrush, cx - 20, 52, sfVert)
            sfVert.Dispose()

            ' Origin marker
            g.FillEllipse(New SolidBrush(axisColor), cx - 2.5F, cy - 2.5F, 5, 5)

            ' ── Title bar (teal header band) ──
            Dim headerRect As New RectangleF(0, 0, W, 44)
            Dim headerBrush As New Drawing2D.LinearGradientBrush(headerRect, Color.FromArgb(0, 77, 64), teal, Drawing2D.LinearGradientMode.Horizontal)
            g.FillRectangle(headerBrush, headerRect)
            g.DrawString(airplane.Name, titleFont, Brushes.White, 10, 5)

            ' Gear type badge (rounded pill)
            Dim gearText As String = airplane.Gear
            If gearText IsNot Nothing AndAlso gearText.Length > 0 Then
                Dim badgeSz = g.MeasureString(gearText, badgeFont)
                Dim badgeX As Single = CSng(W - badgeSz.Width - 22)
                Dim badgeY As Single = 7
                Dim badgeW As Single = badgeSz.Width + 12
                Dim badgeH As Single = badgeSz.Height + 4
                Dim badgeCr As Single = badgeH / 2  ' pill shape
                Dim badgePath As New Drawing2D.GraphicsPath()
                badgePath.AddArc(badgeX, badgeY, badgeCr * 2, badgeH, 90, 180)
                badgePath.AddArc(badgeX + badgeW - badgeCr * 2, badgeY, badgeCr * 2, badgeH, 270, 180)
                badgePath.CloseFigure()
                g.FillPath(New SolidBrush(Color.FromArgb(80, 255, 255, 255)), badgePath)
                g.DrawPath(New Pen(Color.FromArgb(120, 255, 255, 255), 0.8F), badgePath)
                g.DrawString(gearText, badgeFont, New SolidBrush(Color.FromArgb(230, 255, 255, 255)), badgeX + 6, badgeY + 2)
                badgePath.Dispose()
            End If

            ' Wheel count + tire info in header
            Dim infoText As String = airplane.NumberWheels & " wheels"
            If airplane.TireWidth IsNot Nothing Then
                infoText &= " | TW=" & Format(airplane.TireWidth.GetValue(measurementSystem), "0.0") & " " & unitLabel
            End If
            If airplane.Cp IsNot Nothing Then
                Dim cpLabel As String = If(isMetric, "MPa", "psi")
                infoText &= " | p=" & Format(airplane.Cp.GetValue(measurementSystem), "0.0") & " " & cpLabel
            End If
            g.DrawString(infoText, subtitleFont, New SolidBrush(Color.FromArgb(200, 178, 223, 219)), 10, 22)
            headerBrush.Dispose()

            ' ── Draw wheels — right side (primary) ──
            Dim wheelW As Single = 12   ' half-width of the tire imprint rectangle
            Dim wheelH As Single = 22   ' half-height
            Dim wIdx As Integer = 1
            For Each wc In airplane.WheelCoordinates
                Dim wx As Single = toX(wc.X.UsCustomary)
                Dim wy As Single = toY(wc.Y.UsCustomary)

                ' Tire imprint — rounded rectangle with gradient fill
                Dim tireRect As New RectangleF(wx - wheelW / 2, wy - wheelH / 2, wheelW, wheelH)
                Dim tirePath As New Drawing2D.GraphicsPath()
                Dim cr As Single = 3.5F  ' corner radius
                tirePath.AddArc(tireRect.X, tireRect.Y, cr * 2, cr * 2, 180, 90)
                tirePath.AddArc(tireRect.Right - cr * 2, tireRect.Y, cr * 2, cr * 2, 270, 90)
                tirePath.AddArc(tireRect.Right - cr * 2, tireRect.Bottom - cr * 2, cr * 2, cr * 2, 0, 90)
                tirePath.AddArc(tireRect.X, tireRect.Bottom - cr * 2, cr * 2, cr * 2, 90, 90)
                tirePath.CloseFigure()

                ' Fill with gradient
                Using gb As New Drawing2D.LinearGradientBrush(tireRect, wheelFill, Color.FromArgb(200, 0, 77, 64), Drawing2D.LinearGradientMode.Vertical)
                    g.FillPath(gb, tirePath)
                End Using
                g.DrawPath(New Pen(wheelStroke, 1.0F), tirePath)

                ' Wheel number label (centered)
                Dim numStr As String = wIdx.ToString()
                Dim numSz = g.MeasureString(numStr, coordFont)
                g.DrawString(numStr, coordFont, Brushes.White, wx - numSz.Width / 2, wy - numSz.Height / 2)

                tirePath.Dispose()

                ' ── Mirrored wheel (left side, softer) ──
                Dim mx As Single = toX(-wc.X.UsCustomary)
                Dim mirrorRect As New RectangleF(mx - wheelW / 2, wy - wheelH / 2, wheelW, wheelH)
                Dim mirrorPath As New Drawing2D.GraphicsPath()
                mirrorPath.AddArc(mirrorRect.X, mirrorRect.Y, cr * 2, cr * 2, 180, 90)
                mirrorPath.AddArc(mirrorRect.Right - cr * 2, mirrorRect.Y, cr * 2, cr * 2, 270, 90)
                mirrorPath.AddArc(mirrorRect.Right - cr * 2, mirrorRect.Bottom - cr * 2, cr * 2, cr * 2, 0, 90)
                mirrorPath.AddArc(mirrorRect.X, mirrorRect.Bottom - cr * 2, cr * 2, cr * 2, 90, 90)
                mirrorPath.CloseFigure()
                Using gb As New Drawing2D.LinearGradientBrush(mirrorRect, mirrorFill, Color.FromArgb(140, 0, 77, 64), Drawing2D.LinearGradientMode.Vertical)
                    g.FillPath(gb, mirrorPath)
                End Using
                g.DrawPath(New Pen(mirrorStroke, 0.8F), mirrorPath)
                g.DrawString(numStr, coordFont, New SolidBrush(Color.FromArgb(180, 255, 255, 255)), mx - numSz.Width / 2, wy - numSz.Height / 2)
                mirrorPath.Dispose()

                wIdx += 1
            Next

            ' ── Evaluation points ──
            For Each ep In airplane.EvaluationPoints
                Dim ex As Single = toX(ep.X.UsCustomary)
                Dim ey As Single = toY(ep.Y.UsCustomary)
                g.FillEllipse(New SolidBrush(Color.FromArgb(180, evalColor.R, evalColor.G, evalColor.B)), ex - 3, ey - 3, 6, 6)
                g.DrawEllipse(New Pen(evalColor, 0.8F), ex - 3, ey - 3, 6, 6)
            Next


            ' ── Dimension annotations ──
            Dim dimPen As New Pen(Color.FromArgb(160, 160, 155), 0.8F)
            dimPen.DashStyle = Drawing2D.DashStyle.Dash
            Dim dimBrush As New SolidBrush(Color.FromArgb(100, 100, 95))

            ' Dual spacing (B) — if > 0
            If airplane.B IsNot Nothing AndAlso airplane.B.UsCustomary > 0 AndAlso airplane.WheelCoordinates.Count >= 1 Then
                Dim firstWc = airplane.WheelCoordinates(0)
                Dim rWx As Single = toX(firstWc.X.UsCustomary)
                Dim lWx As Single = toX(-firstWc.X.UsCustomary)
                Dim dY As Single = toY(firstWc.Y.UsCustomary) - wheelH / 2 - 14
                g.DrawLine(dimPen, lWx, dY, rWx, dY)
                g.DrawLine(New Pen(dimPen.Color, 0.8F), lWx, dY - 4, lWx, dY + 4)
                g.DrawLine(New Pen(dimPen.Color, 0.8F), rWx, dY - 4, rWx, dY + 4)
                Dim bLabel As String = "B=" & Format(airplane.B.GetValue(measurementSystem), "0.0") & " " & unitLabel
                Dim bSz = g.MeasureString(bLabel, labelFont)
                g.DrawString(bLabel, labelFont, dimBrush, (lWx + rWx) / 2 - bSz.Width / 2, dY - bSz.Height - 1)
            End If

            ' Tandem spacing (Ts) — if > 0 and multiple Y positions exist
            If airplane.Ts > 0 AndAlso airplane.WheelCoordinates.Count >= 2 Then
                ' Find two wheels at different Y
                Dim w1 = airplane.WheelCoordinates(0)
                Dim w2 As ICoordinates = Nothing
                For Each wc In airplane.WheelCoordinates
                    If Math.Abs(wc.Y.UsCustomary - w1.Y.UsCustomary) > 1 Then
                        w2 = wc
                        Exit For
                    End If
                Next
                If w2 IsNot Nothing Then
                    Dim tX As Single = toX(w1.X.UsCustomary) + wheelW / 2 + 14
                    Dim tY1 As Single = toY(w1.Y.UsCustomary)
                    Dim tY2 As Single = toY(w2.Y.UsCustomary)
                    g.DrawLine(dimPen, tX, tY1, tX, tY2)
                    g.DrawLine(New Pen(dimPen.Color, 0.8F), tX - 4, tY1, tX + 4, tY1)
                    g.DrawLine(New Pen(dimPen.Color, 0.8F), tX - 4, tY2, tX + 4, tY2)
                    Dim tsVal As Double = If(isMetric, airplane.Ts * 25.4, airplane.Ts)
                    Dim tsLabel As String = "Ts=" & Format(tsVal, "0.0") & " " & unitLabel
                    Dim tsSz = g.MeasureString(tsLabel, labelFont)
                    g.DrawString(tsLabel, labelFont, dimBrush, tX + 5, (tY1 + tY2) / 2 - tsSz.Height / 2)
                End If
            End If

            ' ── Legend (bottom-right, rounded box) ──
            Dim lgFont As New Font("Segoe UI", 8.5F)
            Dim lgBrush As New SolidBrush(Color.FromArgb(100, 100, 95))
            Dim lgItems As Integer = 2 + If(airplane.EvaluationPoints.Count > 0, 1, 0)
            Dim lgW As Single = 200
            Dim lgH As Single = CSng(lgItems * 18 + 12)
            Dim lgX As Single = CSng(W - lgW - 14)
            Dim lgY As Single = CSng(H - lgH - 24)

            ' Rounded legend background with border
            Dim lgRect As New RectangleF(lgX, lgY, lgW, lgH)
            Dim lgPath As New Drawing2D.GraphicsPath()
            Dim lgCr As Single = 4.0F
            lgPath.AddArc(lgRect.X, lgRect.Y, lgCr * 2, lgCr * 2, 180, 90)
            lgPath.AddArc(lgRect.Right - lgCr * 2, lgRect.Y, lgCr * 2, lgCr * 2, 270, 90)
            lgPath.AddArc(lgRect.Right - lgCr * 2, lgRect.Bottom - lgCr * 2, lgCr * 2, lgCr * 2, 0, 90)
            lgPath.AddArc(lgRect.X, lgRect.Bottom - lgCr * 2, lgCr * 2, lgCr * 2, 90, 90)
            lgPath.CloseFigure()
            g.FillPath(New SolidBrush(Color.FromArgb(235, 255, 255, 255)), lgPath)
            g.DrawPath(New Pen(Color.FromArgb(200, 200, 195), 0.8F), lgPath)
            lgPath.Dispose()

            Dim lgRowY As Single = lgY + 6
            Dim lgRowX As Single = lgX + 8

            ' Wheel legend
            g.FillRectangle(New SolidBrush(wheelFill), lgRowX, lgRowY + 2, 10, 10)
            g.DrawRectangle(New Pen(wheelStroke, 0.8F), lgRowX, lgRowY + 2, 10, 10)
            g.DrawString("Tire imprint (right side)", lgFont, lgBrush, lgRowX + 14, lgRowY)
            lgRowY += 18
            g.FillRectangle(New SolidBrush(mirrorFill), lgRowX, lgRowY + 2, 10, 10)
            g.DrawRectangle(New Pen(mirrorStroke, 0.8F), lgRowX, lgRowY + 2, 10, 10)
            g.DrawString("Mirrored (left side)", lgFont, lgBrush, lgRowX + 14, lgRowY)
            If airplane.EvaluationPoints.Count > 0 Then
                lgRowY += 18
                g.FillEllipse(New SolidBrush(evalColor), lgRowX + 2, lgRowY + 3, 6, 6)
                g.DrawString("Evaluation points", lgFont, lgBrush, lgRowX + 14, lgRowY)
            End If

            ' ── Subtle border ──
            g.DrawRectangle(New Pen(Color.FromArgb(210, 210, 205), 1), 0, 0, W - 1, H - 1)

            ' ── Cleanup ──
            titleFont.Dispose()
            subtitleFont.Dispose()
            tickFont.Dispose()
            labelFont.Dispose()
            coordFont.Dispose()
            badgeFont.Dispose()
            lgFont.Dispose()
            axisPen.Dispose()
            dimPen.Dispose()
            gridPen.Dispose()
            tickBrush.Dispose()
            axUnitBrush.Dispose()
            dimBrush.Dispose()
            lgBrush.Dispose()

        End Sub


        Public Sub PaintCDFGraph(gr1 As Graphics, gr2 As Graphics, gr3 As Graphics, PictureBoxCdf As PictureBox, PictureBox2 As PictureBox, PictureBox3 As PictureBox, font As Font, measurementSystem As IMeasurmentSystem, job As IFaarFieldJob, Section As ISection)
            gr1.Clear(Color.White)
            gr2.Clear(Color.White)
            gr3.Clear(Color.White)


            'job = FaarFieldViewModel.CurrentJob
            '  Section = FaarFieldViewModel.CurrentSectionView.Section
            Dim I As Integer
            Dim SaveFillColor As Color 'ik05 Dim SaveFillColor As Integer
            Dim SaveFillStyle As Brush
            Dim Label3 = New System.Windows.Forms.Label()
            Dim Label5 = New System.Windows.Forms.Label()
            Dim Label4 = New System.Windows.Forms.Label()

            ' Dim gr1 As Graphics = CreateAutoRedrawGraphicsPictureBox(PictureBoxCdf)
            '  Dim gr2 As Graphics = CreateAutoRedrawGraphicsPictureBox(PictureBox2)
            '  Dim gr3 As Graphics = CreateAutoRedrawGraphicsPictureBox(PictureBox3)
            Dim drawPen As New Pen(Color.Black, 1)
            Dim drawFont As New Font("MS Sans Serif", 10, FontStyle.Bold)
            Dim drawFont2 As New Font("MS Sans Serif", 8, FontStyle.Bold)
            Dim drawBrush As New SolidBrush(Color.Black)
            Dim drawBrush2 As New Drawing2D.HatchBrush(Drawing2D.HatchStyle.DiagonalCross, Color.Black, Color.FromArgb(252, 255, 191))
            Dim drawFormat As New StringFormat()

            Dim m As New Drawing2D.Matrix()

            Dim scaleX, scaleY As Double, tick As Double
            Dim cdfMax As Single, yLab As Single

            '  gr2.DrawLine(Pens.Black, New Point(10, (PictureBoxCdf.Height / 2)), New Point(PictureBoxCdf.Width - 10, (PictureBoxCdf.Height / 2)))
            ' g.DrawLine(Pens.Black, New Point((PictureBox.Width / 2), 10), New Point(PictureBox.Width / 2, (PictureBox.Height) - 10))


            Try

                For I = 1 To 80
                    _acCdf(I, 1) = 0
                    _acCdf(I, 2) = I

                    _acCdf2(I, 1) = 0
                    _acCdf2(I, 2) = I
                Next

                For I = 1 To Section.Airplanes.Count
                    For N = 1 To 41



                        If _acCdf(I, 1) < Section.Airplanes.Item(I - 1).CDFGraphData(N) Then
                            _acCdf(I, 1) = Section.Airplanes.Item(I - 1).CDFGraphData(N)
                        End If

                    Next N
                Next I

                For I = 1 To NAC
                    For N = 1 To 41
                        If _acCdf(I, 1) / cdfMax > 0.05 Then
                            _displayCdf(I) = True
                        End If
                    Next N
                Next I

                ' Call PIKSTR2(I80, _acCdf, _acCdf2)

                'For I = 1 To Section.Airplanes.Count
                '    _acCdf(I, 2) = I
                '    _acCdf(I, 1) = Section.Airplanes.Item(I - 1).Cdf
                'Next
                For I = Section.Airplanes.Count + 1 To I80
                    _acCdf(I, 2) = I
                    _acCdf(I, 1) = 0
                Next

                Dim j As Integer, a As Single, b As Single

                For j = 2 To I80
                    a = _acCdf(j, 1)
                    b = _acCdf(j, 2)
                    For I = j - 1 To 1 Step -1
                        If (_acCdf(I, 1) <= a) Then
                            GoTo goto10
                        End If
                        _acCdf(I + 1, 1) = _acCdf(I, 1)
                        _acCdf(I + 1, 2) = _acCdf(I, 2)
                    Next I
                    I = 0
goto10:             _acCdf(I + 1, 1) = a
                    _acCdf(I + 1, 2) = b
                Next

                For I = 1 To I80
                    _acCdf2(I, 1) = _acCdf(I80 - I + 1, 1)
                    _acCdf2(I, 2) = _acCdf(I80 - I + 1, 2)
                Next

                cdfMax = 0
                If Not Section.SectionCDF Is Nothing Then
                    For I = 1 To 41

                        If cdfMax < Section.SectionCDF(I) Then
                            cdfMax = Section.SectionCDF(I)
                        End If
                    Next
                End If


                cdfMax = cdfMax


                If cdfMax > 0.1 Then
                    Label3.Text = Label3.Text & Math.Round(cdfMax, 2)
                Else
                    Dim dd, dd1 As Single
                    dd = CSng(Math.Pow(10, Math.Ceiling(Math.Abs(Math.Log10(cdfMax))))) * 100
                    dd1 = CSng(Math.Truncate(cdfMax * dd) / dd)
                    Label3.Text = Label3.Text & dd1
                End If


                Label4.Text = Label4.Text + job.Name
                Label5.Text = Label5.Text + Section.Name

                CDFMAX1 = cdfMax
                If cdfMax = 0 Then cdfMax = 1


                scaleX = CSng(PictureBoxCdf.Width / (820 + 40))
                scaleY = CSng(PictureBoxCdf.Height / 1.25 / cdfMax)
                'ScaleY = CSng(PictureBox1.Height / CDFmax)
                tick = (5 / scaleY)
                drawFormat.FormatFlags = StringFormatFlags.DirectionVertical
                SaveFillColor = Color.Violet
                gr1.Clear(SaveFillColor)
                gr1.Clear(PictureBoxCdf.BackColor)
                PictureBoxCdf.BackColor = Color.Green
                PictureBoxCdf.BackColor = Color.FromArgb(100, 100, 100, 2)
                'gr1.DrawString("CDF max = " & Math.Round(CDFmax1, 2), drawFont, drawBrush, 10, 0)
                'gr1.DrawString("CDF", drawFont, drawBrush, 225, 0)

                'offset - inches and mm
                If tick > 10 ^ -7 Then
                    If TypeOf (job.DesignOptions.MeasurementSystem) Is UsCustomary Then
                        gr1.DrawString("-400        -300        -200        -100           0" &
                      "           100          200         300         400" & Environment.NewLine & "                                                         Offset - Inches",
                    drawFont, drawBrush, CSng(0), CSng(PictureBoxCdf.Height * 0.89))
                    Else
                        gr1.DrawString("-10.000  -7.500    -5.000     -2.500          0" &
                               "         2.500      5.000      7.500     10.000" & Environment.NewLine & "                                                         Offset - Meters",
                               drawFont, drawBrush, CSng(0), CSng(PictureBoxCdf.Height * 0.89))
                    End If
                End If


                For I3 = 1 To Math.Min(_maxAc1, Section.Airplanes.Count)
                    I = CShort(_acCdf2(I3, 2))
                    If I = 80 Then Exit For
                    gr1.DrawString(Section.Airplanes.Item(I - 1).Name.ToString, drawFont, drawBrush, 500, Convert.ToSingle((0.03 + 0.03 * I3) * PictureBoxCdf.Height))

                Next

                gr1.DrawString("Cumulative CDF", drawFont, drawBrush, 500, PictureBoxCdf.Height * 0.03)

                For I = 1 To Math.Min(_maxAc1, Section.Airplanes.Count)
                    gr1.DrawLine(_drawPenAc(I - 1), 420, Convert.ToSingle((0.052 + 0.03 * I) * PictureBoxCdf.Height), 490, Convert.ToSingle((0.052 + 0.03 * I) * PictureBoxCdf.Height))
                Next

                gr1.DrawLine(_drawPenCdf, 420, Convert.ToSingle(PictureBoxCdf.Height * 0.052), 490, Convert.ToSingle(PictureBoxCdf.Height * 0.052))



                If cdfMax > 10 Then
                    Dim dd As Single
                    'dd = CSng(Math.Pow(10, Math.Ceiling(Math.Abs(Math.Log10(CDFmax)))))
                    dd = CSng(Math.Pow(10, Math.Floor(Math.Abs(Math.Log10(cdfMax)))))
                    yLab = CSng(Math.Truncate(cdfMax / dd) * dd)
                ElseIf cdfMax >= 1.0 Then
                    yLab = CSng(Math.Truncate(cdfMax * 1) / 1)
                Else

                    Dim dd As Single
                    dd = CSng(Math.Pow(10, Math.Ceiling(Math.Abs(Math.Log10(cdfMax)))))
                    yLab = CSng(Math.Truncate(cdfMax * dd) / dd)
                    If cdfMax > 0.98 Then
                        yLab = 1
                    End If

                End If


                'Dim text1 As String

                'If cdfMax <= 1 Then
                '    text1 = CStr(yLab)
                'Else
                '    text1 = CStr(yLab) & ".0"
                'End If

                'Temp = CSng(gr1.MeasureString(text1, drawFont).Width - 24.33)

                'If tick > 10 ^ -7 Then

                '    gr1.DrawString(text1, drawFont, drawBrush, 230 - Temp,
                '           CSng((cdfMax - yLab) / cdfMax * PictureBoxCdf.Height * 0.82) + 5)

                'End If

                'labels for y-axis
                Dim text1 As String
                Dim yTick As Single = 1

                If yLab >= 100 Then
                    yTick = 100
                ElseIf yLab >= 10 Then
                    yTick = 10
                End If

                For h = yTick To yLab Step yTick
                    text1 = If(cdfMax >= 1, CStr(h), CStr(h) & ".0")
                    Temp = CSng(gr1.MeasureString(text1, drawFont).Width - 24.33)

                    If tick > 10 ^ -7 Then
                        gr1.DrawString(text1, drawFont, drawBrush, 230 - Temp,
                        CSng((cdfMax - h) / cdfMax * PictureBoxCdf.Height * 0.82) + 5)

                    End If
                Next

                SaveFillStyle = drawPen.Brush()


                m.Translate(CSng((PictureBoxCdf.Width) * 0.5), CSng((PictureBoxCdf.Height) * 0.85))
                'm.Scale(0.55, -105)
                m.Scale(CSng(scaleX), CSng(-scaleY * 1.01))
                gr1.Transform = m
                drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft

                'gr1.DrawRectangle(drawPen, xLeftLife, yTopLife, xRightLife, yBottomLife)
                tick = (5 / scaleY)


                Try 'added kawa 2013
                    If tick > 0.00001 Then
                        gr1.DrawLine(Pens.Black, 0, CSng(-tick * 1.35), 0, CSng(1.05))
                    End If
                Catch ex As Exception

                    Dim txt As String
                    txt = ex.Message
                    txt = txt + Environment.NewLine + Environment.NewLine
                    txt = txt + ex.StackTrace
                    txt = txt + Environment.NewLine + Environment.NewLine
                    MsgBox(txt)

                End Try
                Try


                    gr1.DrawLine(Pens.Black, -410, 0, 410, 0)


                    'gr1.DrawLine(Pens.Black, 0, 0, 0, 1 * ScaleY)
                    gr1.DrawLine(Pens.Black, 0, 0, 0, CSng(cdfMax * 1.35))
                    gr1.DrawLine(Pens.Black, -8, yLab, 8, yLab)

                    'tick marks for y-axis
                    For k = yTick To yLab Step yTick
                        gr1.DrawLine(Pens.Black, -8, k, 8, k)
                    Next

                Catch ex As Exception


                End Try

                If TypeOf (job.DesignOptions.MeasurementSystem) Is UsCustomary Then

                    For I = 1 To 4
                        gr1.DrawLine(Pens.Black, I * 100, CSng(-tick * 2), I * 100, 0)
                        gr1.DrawLine(Pens.Black, -I * 100, CSng(-tick * 2), -I * 100, 0)
                    Next

                    For I = 1 To 41
                        gr1.DrawLine(Pens.Black, I * 10, CSng(-tick), I * 10, 0)
                        gr1.DrawLine(Pens.Black, -I * 10, CSng(-tick), -I * 10, 0)
                    Next

                Else

                    For I = 1 To 41
                        gr1.DrawLine(Pens.Black, CInt(I * 2500 / 25.4), CSng(-tick * 2), CInt(I * 2500 / 25.4), 0)
                        gr1.DrawLine(Pens.Black, -CInt(I * 2500 / 25.4), CSng(-tick * 2), -CInt(I * 2500 / 25.4), 0)
                    Next

                    For I = 1 To 41
                        gr1.DrawLine(Pens.Black, CInt(I * 250 / 25.4), CSng(-tick), CInt(I * 250 / 25.4), 0)
                        gr1.DrawLine(Pens.Black, -CInt(I * 250 / 25.4), CSng(-tick), -CInt(I * 250 / 25.4), 0)
                    Next


                End If


                For I3 = 1 To Math.Min(_maxAc1, Section.Airplanes.Count)
                    Dim I4 = CInt(_acCdf2(I3, 2))
                    If I4 = 80 Then Exit For
                    For I = 0 To 40
                        'Section.Airplanes.Item(I4 - 1).CDFGraphData(I) = CDFdata2(1, I4 - 1, I)
                    Next
                Next



                'For I3 = 1 To Math.Min(MaxAC1, NAC)
                '    I1 = CInt(MaxAC_CDF2(I3, 2))

                '    For I = 1 To 40
                '        gr1.DrawLine(drawPenAC(I3 - 1), (I - 1) * 10 + 5,
                '    CDFdata2(1, I1, I), I * 10 + 5, CDFdata2(1, I1, I + 1))
                '        gr1.DrawLine(drawPenAC(I3 - 1), -((I - 1) * 10 + 5),
                '    CDFdata2(1, I1, I), -(I * 10 + 5), CDFdata2(1, I1, I + 1))
                '    Next
                '    I = 1
                '    gr1.DrawLine(drawPenAC(I3 - 1), (I - 1) * 10 + 5,
                'CDFdata2(1, I1, I), -((I - 1) * 10 + 5), CDFdata2(1, I1, I))
                'Next


                'For I1 = NAC + 1 To NAC + 1
                '    For I = 1 To 40
                '        gr1.DrawLine(drawPenCDF, (I - 1) * 10 + 5,
                '    CDFdata2(1, I1, I), I * 10 + 5, CDFdata2(1, I1, I + 1))
                '        gr1.DrawLine(drawPenCDF, -((I - 1) * 10 + 5),
                '    CDFdata2(1, I1, I), -(I * 10 + 5), CDFdata2(1, I1, I + 1))
                '    Next
                '    I = 1
                '    gr1.DrawLine(drawPenCDF, (I - 1) * 10 + 5,
                'CDFdata2(1, I1, I), -((I - 1) * 10 + 5), CDFdata2(1, I1, I))
                'Next I1

                If tick > 10 ^ -7 Then

                    'Draw aircraft CDF lines
                    For I3 = 1 To Math.Min(_maxAc1, Section.Airplanes.Count)
                        Dim I4 = CInt(_acCdf2(I3, 2))
                        If I4 = 80 Then Exit For
                        For I = 1 To 40
                            'gr1.DrawLine(_drawPenAc(I3 - 1), (I - 1) * 10 + 5,
                            '(CDFdata2(1, I4, I)), I * 10 + 5, (CDFdata2(1, I4, I + 1)))
                            'gr1.DrawLine(_drawPenAc(I3 - 1), -((I - 1) * 10 + 5),
                            '  (CDFdata2(1, I4, I)), -(I * 10 + 5), (CDFdata2(1, I4, I + 1)))


                            gr1.DrawLine(_drawPenAc(I3 - 1), (I - 1) * 10 + 5,
                              (Section.Airplanes.Item(I4 - 1).CDFGraphData(I)), I * 10 + 5, (Section.Airplanes.Item(I4 - 1).CDFGraphData(I + 1)))
                            gr1.DrawLine(_drawPenAc(I3 - 1), -((I - 1) * 10 + 5),
                                   (Section.Airplanes.Item(I4 - 1).CDFGraphData(I)), -(I * 10 + 5), (Section.Airplanes.Item(I4 - 1).CDFGraphData(I + 1)))
                        Next
                        I = 1
                        'gr1.DrawLine(_drawPenAc(I3 - 1), (I - 1) * 10 + 5,
                        'CDFdata2(1, I4, I), -((I - 1) * 10 + 5), CDFdata2(1, I4, I))

                    Next

                    If Not Section.SectionCDF Is Nothing Then

                        For I4 = Section.Airplanes.Count + 1 To Section.Airplanes.Count + 1
                            For I = 1 To 40
                                gr1.DrawLine(_drawPenCdf, (I - 1) * 10 + 5,
                                     (Section.SectionCDF(I)), I * 10 + 5, (Section.SectionCDF(I + 1)))
                                gr1.DrawLine(_drawPenCdf, -((I - 1) * 10 + 5),
                                     (Section.SectionCDF(I)), -(I * 10 + 5), (Section.SectionCDF(I + 1)))
                            Next
                            I = 1
                            If PCRRun = False Then
                                gr1.DrawLine(_drawPenCdf, (I - 1) * 10 + 5,
                                 (CDFdata2(1, I4, I)), -((I - 1) * 10 + 5), (CDFdata2(1, I4, I)))
                            Else
                                gr1.DrawLine(_drawPenCdf, (I - 1) * 10 + 5,
                                (CDFdata3(1, I4, I)), -((I - 1) * 10 + 5), (CDFdata3(1, I4, I)))
                            End If



                        Next I4
                    End If




                End If





                gr1.Dispose() : drawPen.Dispose() : drawFont.Dispose()
                drawFont2.Dispose() : drawBrush.Dispose()
                drawBrush2.Dispose() : drawFormat.Dispose()



            Catch ex As Exception

                Dim txt As String
                txt = ex.Message
                txt = txt + Environment.NewLine + Environment.NewLine
                txt = txt + ex.StackTrace
                txt = txt + Environment.NewLine + Environment.NewLine
                MsgBox(txt)

            End Try

            'drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft
            'gr1.Clear(PictureBox2.BackColor)

            '  drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft
            ' gr2.DrawLine(Pens.Black, New Point(10, (PictureBoxCdf.Height / 2)), New Point(PictureBoxCdf.Width - 10, (PictureBoxCdf.Height / 2)))

            'gr1.DrawLine(drawPenCDF, 2, 37, 40, 37)


            gr2.Dispose() : drawFont.Dispose()
            drawFont2.Dispose() : drawFormat.Dispose()

            ' drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft

            ' gr1.Clear(PictureBox3.BackColor)

            ' Dim m As New Drawing2D.Matrix()
            ' drawFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft


            For I3 = 1 To CShort(Math.Min(_maxAc1, NAC))
                I = CShort(_acCdf2(I3, 2))

                'gr3.DrawString(ACName(I), drawFont, drawBrush, CSng(0), CSng(I3 * 15 - 15))
            Next

            ' gr3.DrawString("Cumulative CDF", drawFont, drawBrush, CSng(0), CSng(I3 * 15 - 10))

            gr3.Dispose() : drawPen.Dispose() : drawFont.Dispose()
            drawFont2.Dispose() : drawBrush.Dispose()
            drawFormat.Dispose()
        End Sub

        Public Function CreateAutoRedrawGraphicsPictureBox(ByVal oPic As PictureBox) As Graphics
            If oPic.Image Is Nothing Then
                oPic.Image = New Bitmap(oPic.Width, oPic.Height)
            End If
            CreateAutoRedrawGraphicsPictureBox = Graphics.FromImage(oPic.Image)
            oPic.Invalidate()
        End Function

        Public Sub PaintPCRGraph(chart1 As Chart, DataGridView1 As DataGridView, PictureBoxPCR As PictureBox, font As Font, measurementSystem As IMeasurmentSystem, job As IFaarFieldJob, Section As ISection)
            Dim ACName1(MaxSectAC) As String

            Dim gACN_copy(Save_NAC) As Single
            Dim LibPCNindex(6) As Integer
            Dim TopIndex1 As Integer
            Dim TopIndex2 As Integer

            Dim PCNgZero As Integer = 0
            Dim i As Integer, max1 As Single, index1 As Integer
            'For i = 1 To MaxSectAC
            '    If (InStr(4, ACName(i), "Belly", CompareMethod.Text) > 0) Then
            '        ACName1(i) = ACName(i).Substring(0, ACName(i).Length - 6)
            '    Else
            '        ACName1(i) = ACName(i)
            '    End If
            'Next i

            Dim NACgraph As Integer = 0


            'For i = 1 To Save_NAC ' NAC
            '    If (InStr(4, ACName(i), "Belly", CompareMethod.Text) > 0) Then
            '        'ACName1(i) = ACName(i).Substring(0, ACName(i).Length - 6)
            '    Else
            '        NACgraph = NACgraph + 1
            '        'ACName1(NACgraph) = ACName(i)
            '    End If
            '    ACName1(i) = ACName(i)
            'Next i



            'Save_NAC = 3
            'NAC = Save_NAC
            TopIndex1 = Math.Min(NACgraph, 6)
            TopIndex2 = Math.Min(Save_NAC, 6)

            For ii1 As Integer = 1 To Save_NAC '2, 9
                'gACN_copy(ii1) = gACN(ii1)
                gACN_copy(ii1) = gACN_GL(ii1)
            Next

            For ii1 As Integer = 1 To TopIndex1
                max1 = -100

                For ii2 As Integer = 1 To Save_NAC
                    If max1 < gACN_copy(ii2) Then
                        max1 = gACN_copy(ii2)
                        index1 = ii2
                    End If
                Next
                'LibPCNindex(ii1) = index1
                LibPCNindex(TopIndex1 + 1 - ii1) = index1
                gACN_copy(index1) = -100
            Next


            For i = 1 To Save_NAC
                'If gACN(i) > 0 Then
                If gACN_GL(i) > 0 Then
                    PCNgZero = PCNgZero + 1
                End If

            Next


            'For i = 1 To Save_NAC
            '    If LibPCNindex(i) = gPCN_report_index Then
            '        'ACName1(i) = ACName1(i) & "C"
            '        ACName1(i) = ACName1(i) & "*"
            '    End If
            'Next i





            'If Save_NAC > 6 Then
            '    For ii1 As Integer = 1 To NAC
            '        gACN_copy(ii1) = gACN(ii1)
            '    Next

            '    For ii1 As Integer = 1 To 6
            '        max1 = -100

            '        For ii2 As Integer = 1 To NAC
            '            If max1 < gACN_copy(ii2) Then
            '                max1 = gACN_copy(ii2)
            '                index1 = ii2
            '            End If
            '        Next
            '        LibPCNindex(ii1) = index1
            '        gACN_copy(index1) = -100
            '    Next

            '    Dim inc1 As Integer = 1
            '    For ii2 As Integer = 1 To NAC
            '        If gACN_copy(ii2) = -100 Then
            '            LibPCNindex(inc1) = ii2
            '            inc1 = inc1 + 1
            '        End If
            '    Next
            'Else
            '    LibPCNindex(1) = 1 : LibPCNindex(2) = 2 : LibPCNindex(3) = 3
            '    LibPCNindex(4) = 4 : LibPCNindex(5) = 5 : LibPCNindex(6) = 6
            'End If




            'Chart1.ChartAreas(0).AxisY.LabelAutoFitMaxFontSize = "Ala"

            'Me.Chart1.Titles("Name 123")
            'Chart1.Series("Default").Points(0).AxisLabel = "Points 1"

            chart1.ChartAreas(0).AxisY.Title = "ACR/PCR"
            chart1.ChartAreas(0).AxisY.TitleFont = New Font("Tahoma", 12, FontStyle.Bold)
            chart1.ChartAreas(0).AxisY2.Title = "Annual Departures"
            chart1.ChartAreas(0).AxisY2.TitleFont = New Font("Tahoma", 12, FontStyle.Bold)

            For i = 1 To TopIndex1
                'Me.Chart1.Series("Aircraft ACN").Points.AddXY(ACName(i), gACN_GL(LibPCNindex(i)))
                'Me.Chart1.Series("Calculated PCN").Points.AddXY(ACName(i), gACN(LibPCNindex(i)))
                'Me.Chart1.Series("Annual Departures").Points.AddXY(ACName(i), RepsAnnual(LibPCNindex(i)))
                If gACN_GL(LibPCNindex(i)) > 0 Then
                    chart1.Series("Aircraft ACR").Points.AddXY(ACName1(LibPCNindex(i)), gACN_GL(LibPCNindex(i)))
                    'Me.Chart1.Series("Calculated PCN").Points.AddXY(ACName1(LibPCNindex(i)), gACN(LibPCNindex(i)))
                    chart1.Series("Calculated PCR").Points.AddXY(ACName1(LibPCNindex(i)), gPCN_report)
                    chart1.Series("Annual Departures").Points.AddXY(ACName1(LibPCNindex(i)), Save_RepsAnnual(LibPCNindex(i)))
                End If
            Next

            'review999
            If NACgraph = 1 Then
                chart1.Series("Calculated PCR").ChartType = DataVisualization.Charting.SeriesChartType.Point
                chart1.Series("Annual Departures").ChartType = DataVisualization.Charting.SeriesChartType.Point
            Else
                chart1.Series("Calculated PCR").ChartType = DataVisualization.Charting.SeriesChartType.Line
                chart1.Series("Annual Departures").ChartType = DataVisualization.Charting.SeriesChartType.Line
            End If



            'Chart1.Titles.Add("NewTitle1")
            'Chart1.Titles("NewTitle1").Text = "Most Demanging Aircrafts"
            'Chart1.Titles("NewTitle1").Font = New Font("Tahoma", 14, FontStyle.Bold)
            'Chart1.Titles("NewTitle1").DockedToChartArea = Nothing

            Dim ac0 As DataColumn
            Dim ac1 As System.Data.DataColumn
            Dim ac2 As System.Data.DataColumn
            Dim ac3 As System.Data.DataColumn
            Dim ac4 As System.Data.DataColumn
            Dim ac5 As System.Data.DataColumn
            Dim ac6 As System.Data.DataColumn
            Dim ac7 As System.Data.DataColumn

            Dim grdDataTable As New DataTable

            ac0 = New DataColumn(" ")
            grdDataTable.Columns.Add(ac0)




            If Save_NAC >= 1 And PCNgZero >= 1 Then
                ac1 = New DataColumn(ACName1(LibPCNindex(1)) & "1")
                grdDataTable.Columns.Add(ac1)
            End If


            If Save_NAC >= 2 And PCNgZero >= 2 Then
                ac2 = New DataColumn(ACName1(LibPCNindex(2)) & "2")
                grdDataTable.Columns.Add(ac2)
            End If


            If Save_NAC >= 3 And PCNgZero >= 3 Then
                ac3 = New DataColumn(ACName1(LibPCNindex(3)) & "3")
                grdDataTable.Columns.Add(ac3)
            End If

            If Save_NAC >= 4 And PCNgZero >= 4 Then
                ac4 = New DataColumn(ACName1(LibPCNindex(4)) & "4")
                grdDataTable.Columns.Add(ac4)
            End If

            If Save_NAC >= 5 And PCNgZero >= 5 Then
                ac5 = New DataColumn(ACName1(LibPCNindex(5)) & "5")
                grdDataTable.Columns.Add(ac5)
            End If


            If Save_NAC >= 6 And PCNgZero >= 6 Then
                ac6 = New DataColumn(ACName1(LibPCNindex(6)) & "6")
                grdDataTable.Columns.Add(ac6)
            End If



            DataGridView1.DataSource = grdDataTable

            'Call SetValues(DataGridView1)


            Dim row1, row2, row3 As System.Data.DataRow
            row1 = grdDataTable.NewRow
            row2 = grdDataTable.NewRow
            row3 = grdDataTable.NewRow
            grdDataTable.Rows.Add(row1)
            grdDataTable.Rows.Add(row2)
            grdDataTable.Rows.Add(row3)


            'DataGridView1.Rows(0).Cells(0).Value = "Aircraft ACR (blue squares bar)"
            'DataGridView1.Rows(1).Cells(0).Value = "Calculated PCR (black line)"
            'DataGridView1.Rows(2).Cells(0).Value = "Annual Departures (red line)"


            'Dim i1 As Integer = 0
            'For i = 1 To TopIndex1

            '    If gACN_GL(LibPCNindex(i)) > 0 Then
            '        i1 = i1 + 1
            '        DataGridView1.Rows(0).Cells(i1).Value = Format(gACN_GL(LibPCNindex(i)), "#,###.00")
            '        'DataGridView1.Rows(1).Cells(i1).Value = Format(gACN(LibPCNindex(i)), "#,###.00")

            '        If LibPCNindex(i) = gPCN_report_index Then
            '            DataGridView1.Rows(1).Cells(i1).Value = Format(gPCN_report, "#,###.00")
            '        End If
            '        DataGridView1.Rows(2).Cells(i1).Value = Format(Save_RepsAnnual(LibPCNindex(i)), "#,###")

            '        DataGridView1.Columns(i1).HeaderText = ACName1(LibPCNindex(i))

            '    End If

            'Next


            'DataGridView1.CurrentCell = Nothing
            'DataGridView1.AutoResizeColumn(0, DataGridViewAutoSizeColumnMode.DisplayedCells)

        End Sub

        Public Sub SetValues(ByRef DataGridView1 As System.Windows.Forms.DataGridView)

            Dim int1 As Integer = 77
            If DataGridView1 Is Nothing Then Exit Sub
            DataGridView1.Columns(0).Frozen = True
            DataGridView1.Columns(0).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft
            DataGridView1.Columns(0).SortMode = DataGridViewColumnSortMode.NotSortable
            DataGridView1.Columns(0).Width = 168


            DataGridView1.Columns(1).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(1).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(1).SortMode = DataGridViewColumnSortMode.NotSortable
            DataGridView1.Columns(1).Width = int1

            'If Save_NAC = 1 Then Exit Sub
            If PCNgZero = 1 Then Exit Sub

            DataGridView1.Columns(2).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(2).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(2).SortMode = DataGridViewColumnSortMode.NotSortable
            DataGridView1.Columns(2).Width = int1

            'If Save_NAC = 2 Then Exit Sub
            If PCNgZero = 2 Then Exit Sub

            DataGridView1.Columns(3).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(3).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(3).SortMode = DataGridViewColumnSortMode.NotSortable
            DataGridView1.Columns(3).Width = int1

            'If Save_NAC = 3 Then Exit Sub
            If PCNgZero = 3 Then Exit Sub

            DataGridView1.Columns(4).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(4).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(4).SortMode = DataGridViewColumnSortMode.NotSortable
            DataGridView1.Columns(4).Width = int1

            'If Save_NAC = 4 Then Exit Sub
            If PCNgZero = 4 Then Exit Sub

            DataGridView1.Columns(5).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(5).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(5).SortMode = DataGridViewColumnSortMode.NotSortable
            DataGridView1.Columns(5).Width = int1

            'If Save_NAC = 5 Then Exit Sub
            If PCNgZero = 5 Then Exit Sub

            DataGridView1.Columns(6).DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(6).HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter
            DataGridView1.Columns(6).SortMode = DataGridViewColumnSortMode.NotSortable
            DataGridView1.Columns(6).Width = int1


        End Sub

        Public Sub PIKSTR2(n As Integer, arr(,) As Single, ByRef newArr(,) As Single)
            Dim i, j As Integer, a As Single, b As Single

            For j = 2 To n
                a = arr(j, 1)
                b = arr(j, 2)
                For i = j - 1 To 1 Step -1
                    If (arr(i, 1) <= a) Then
                        GoTo goto10
                    End If
                    arr(i + 1, 1) = arr(i, 1)
                    arr(i + 1, 2) = arr(i, 2)
                Next i
                i = 0
goto10:         arr(i + 1, 1) = a
                arr(i + 1, 2) = b
            Next

            For i = 1 To n
                newArr(i, 1) = arr(n - i + 1, 1)
                newArr(i, 2) = arr(n - i + 1, 2)
            Next
        End Sub

        Public Sub Paint(g As Graphics, layers As ObservableCollection(Of IMaterial), pictureBox As PictureBox, font As Font, pavementBrushes As Dictionary(Of String, Brush), measurementSystem As IMeasurmentSystem)

            g.Clear(Color.White)
            g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
            g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
            ' Dim fontsize As Single
            ' fontsize = 18
            If layers.Count = 0 Then
                font = New Font(font.Name, 14, FontStyle.Bold)
                g.DrawString("To begin select a Pavement Type", font, Brushes.Black, New PointF(10, pictureBox.Height / 4))
                Return
            End If
            Dim factory As New FaarFieldModelFactory
            Dim totalThickness As Single = layers.Sum(Function(l) l.Thickness.UsCustomary)
            If totalThickness <= 0 Then Return

            Dim ratio As Single = (pictureBox.Height) / (totalThickness + 10)
            Dim line As Single = ratio * 10

            Dim units = " mm"
            Dim modulus = " MPa"
            Dim Kvalue = " MN/m^3"

            If TypeOf (measurementSystem) Is UsCustomary Then
                units = " inches"
                modulus = " psi"
                Kvalue = " pci"
            End If


            g.FillEllipse(Brushes.Black, CType((pictureBox.Width / 2), Single), -line, line * 2, line * 2)
            g.FillEllipse(Brushes.Gray, CType((pictureBox.Width / 2 + line / 2), Single), -line / 2, line, line)
            g.DrawLine(Pens.Black, New Point(0, line), New Point(pictureBox.Width, (line)))

            Dim index = 0
            Dim outputLabel1 As String
            Dim outputLabel2 As String
            Dim outputLabel3 As String
            Dim layer As IMaterial
            For Each layer In layers
                If TypeOf (measurementSystem) Is UsCustomary Then
                    outputLabel1 = layer.Name
                    outputLabel2 = "T=" + Format(layer.Thickness.GetValue(measurementSystem), "0.0").ToString() + units
                    If layer.Category = "P-501 PCC" Then
                        outputLabel3 = "R=" + layer.Rupture.GetValue(measurementSystem).ToString("f0") + modulus
                    Else
                        outputLabel3 = "E=" + layer.Modulus.GetValue(measurementSystem).ToString("N0") + modulus
                    End If
                    g.FillRectangle(pavementBrushes(layer.Name), CType(0, Single), line, CType(pictureBox.Width, Single), CType(ratio * layer.Thickness.UsCustomary, Single))
                    If layer.DesignedLayer IsNot "" And layer.DesignedLayer IsNot Nothing Then
                        'font = New Font(font.Name, 100)
                        Dim pen = New Pen(Color.FromArgb(200, 46, 94, 168), 6)
                        g.DrawRectangle(pen, CType(0, Single), line, CType(pictureBox.Width, Single), CType(ratio * layer.Thickness.UsCustomary, Single))
                    End If

                    font = New Font(font.Name, 12)
                    Dim semiWhiteBrush As New SolidBrush(Color.FromArgb(220, 255, 255, 255))
                    Dim labelBorderPen As New Pen(Color.FromArgb(160, 160, 160), 0.5F)
                    Dim textBrush As New SolidBrush(Color.FromArgb(31, 41, 55))
                    Dim labelH = CInt(g.MeasureString(layer.Name, font).Height) + 2

                    ' Label 1: Material Name — size to text
                    Dim sz1 = g.MeasureString(outputLabel1, font)
                    Dim rect1 = New Rectangle(4, line + 3, CInt(sz1.Width) + 8, labelH)
                    g.FillRectangle(semiWhiteBrush, rect1)
                    g.DrawRectangle(labelBorderPen, rect1)
                    g.DrawString(outputLabel1, font, textBrush, New PointF(6, line + 1))

                    ' Label 2: Thickness — size to text
                    Dim sz2 = g.MeasureString(outputLabel2, font)
                    Dim rect2 = New Rectangle(324, line + 3, CInt(sz2.Width) + 8, labelH)
                    g.FillRectangle(semiWhiteBrush, rect2)
                    g.DrawRectangle(labelBorderPen, rect2)
                    g.DrawString(outputLabel2, font, textBrush, New PointF(326, line + 1))

                    ' Label 3: Modulus — size to text
                    Dim sz3 = g.MeasureString(outputLabel3, font)
                    Dim rect3 = New Rectangle(494, line + 3, CInt(sz3.Width) + 8, labelH)
                    g.FillRectangle(semiWhiteBrush, rect3)
                    g.DrawRectangle(labelBorderPen, rect3)
                    g.DrawString(outputLabel3, font, textBrush, New PointF(496, line + 1))
                Else
                    outputLabel1 = layer.Name
                    outputLabel2 = "T=" + Format(layer.Thickness.GetValue(measurementSystem), "0").ToString() + units
                    If Not layer.Category = "P-501 PCC" Then
                        outputLabel3 = "E=" + layer.Modulus.GetValue(measurementSystem).ToString("N") + modulus
                    Else
                        outputLabel3 = "R=" + layer.Rupture.GetValue(measurementSystem).ToString("f2") + modulus
                    End If
                    g.FillRectangle(pavementBrushes(layer.Name), CType(0, Single), line, CType(pictureBox.Width, Single), CType(ratio * layer.Thickness.UsCustomary, Single))
                    If layer.DesignedLayer IsNot "" And layer.DesignedLayer IsNot Nothing Then
                        'font = New Font(font.Name, 100)
                        Dim pen = New Pen(Color.FromArgb(200, 46, 94, 168), 6)
                        g.DrawRectangle(pen, CType(0, Single), line, CType(pictureBox.Width, Single), CType(ratio * layer.Thickness.UsCustomary, Single))
                    End If
                    font = New Font(font.Name, 12)
                    Dim semiWhite As New SolidBrush(Color.FromArgb(220, 255, 255, 255))
                    Dim lblBorder As New Pen(Color.FromArgb(160, 160, 160), 0.5F)
                    Dim txtBrush As New SolidBrush(Color.FromArgb(31, 41, 55))
                    Dim lblH = CInt(g.MeasureString(layer.Name, font).Height) + 2

                    Dim s1 = g.MeasureString(outputLabel1, font)
                    Dim rect1 = New Rectangle(4, line + 3, CInt(s1.Width) + 8, lblH)
                    g.FillRectangle(semiWhite, rect1)
                    g.DrawRectangle(lblBorder, rect1)
                    g.DrawString(outputLabel1, font, txtBrush, New PointF(6, line + 1))

                    Dim s2 = g.MeasureString(outputLabel2, font)
                    Dim rect2 = New Rectangle(324, line + 3, CInt(s2.Width) + 8, lblH)
                    g.FillRectangle(semiWhite, rect2)
                    g.DrawRectangle(lblBorder, rect2)
                    g.DrawString(outputLabel2, font, txtBrush, New PointF(326, line + 1))

                    Dim s3 = g.MeasureString(outputLabel3, font)
                    Dim rect3 = New Rectangle(494, line + 3, CInt(s3.Width) + 8, lblH)
                    g.FillRectangle(semiWhite, rect3)
                    g.DrawRectangle(lblBorder, rect3)
                    g.DrawString(outputLabel3, font, txtBrush, New PointF(496, line + 1))
                End If
                Dim Int As Integer
                Int = 0

                If (index = layers.Count - 1) Then
                    outputLabel1 = layer.Name
                    For i = 0 To layers.Count - 1
                        If layers.Item(i).Category = "P-501 PCC" Then
                            Int = 1
                        End If
                    Next
                    If Int = 1 Then
                        If layer.SubgradeReaction IsNot Nothing Then
                            outputLabel2 = "k=" + Format(layer.SubgradeReaction.GetValue(measurementSystem), "0.0").ToString() + Kvalue
                        End If
                    Else
                        If layer.NCHRPActive = True Then
                            outputLabel2 = "CBR=" + Format((layer.Modulus.UsCustomary / 2555) ^ (1 / 0.64), "0.##").ToString()
                        Else
                            outputLabel2 = "CBR=" + Format(layer.Modulus.UsCustomary / 1500, "0.##").ToString()
                        End If


                    End If
                    If TypeOf (measurementSystem) Is UsCustomary Then
                        outputLabel3 = "E=" + layer.Modulus.GetValue(measurementSystem).ToString("N0") + modulus
                    Else
                        outputLabel3 = "E=" + layer.Modulus.GetValue(measurementSystem).ToString("N") + modulus
                    End If
                    g.FillRectangle(pavementBrushes(layer.Name), CType(0, Single), line, CType(pictureBox.Width, Single), CType(ratio * layer.Thickness.UsCustomary, Single))
                    font = New Font(font.Name, 12)
                    Dim semiWhite As New SolidBrush(Color.FromArgb(220, 255, 255, 255))
                    Dim lblBorder As New Pen(Color.FromArgb(160, 160, 160), 0.5F)
                    Dim txtBrush As New SolidBrush(Color.FromArgb(31, 41, 55))
                    Dim lblH = CInt(g.MeasureString(layer.Name, font).Height) + 2

                    Dim s1 = g.MeasureString(outputLabel1, font)
                    Dim rect1 = New Rectangle(4, line + 3, CInt(s1.Width) + 8, lblH)
                    g.FillRectangle(semiWhite, rect1)
                    g.DrawRectangle(lblBorder, rect1)
                    g.DrawString(outputLabel1, font, txtBrush, New PointF(6, line + 1))

                    Dim s2 = g.MeasureString(outputLabel2, font)
                    Dim rect2 = New Rectangle(324, line + 3, CInt(s2.Width) + 8, lblH)
                    g.FillRectangle(semiWhite, rect2)
                    g.DrawRectangle(lblBorder, rect2)
                    g.DrawString(outputLabel2, font, txtBrush, New PointF(326, line + 1))

                    Dim s3 = g.MeasureString(outputLabel3, font)
                    Dim rect3 = New Rectangle(494, line + 3, CInt(s3.Width) + 8, lblH)
                    g.FillRectangle(semiWhite, rect3)
                    g.DrawRectangle(lblBorder, rect3)
                    g.DrawString(outputLabel3, font, txtBrush, New PointF(496, line + 1))
                End If

                line += ratio * layer.Thickness.UsCustomary
                index = index + 1
            Next

            line = ratio * 10

            For Each layer In layers
                line += ratio * layer.Thickness.UsCustomary
                g.DrawLine(Pens.Black, New Point(0, line), New Point(pictureBox.Width, line))
            Next




        End Sub


        Public Function LoadBrushes() As Dictionary(Of String, Brush)

            Dim localpath As String = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)

            Dim brushes = New Dictionary(Of String, Brush)

            brushes.Add("P-301 Soil Cement Base", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-304 Cement Treated Base", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-306 Lean Concrete", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("Variable (flexible)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("Variable (rigid)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-209 Crushed Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Aggregate.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-208 Crushed Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Aggregate.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-154 Uncrushed Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Aggregate.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-211 Lime Rock", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Aggregate.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-219 Recycled Concrete Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Aggregate.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-401/P-403 HMA Surface", New TextureBrush(Image.FromFile(localpath + "\\Defaults\Materials\HMA.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-401/P-403 HMA Overlay", New TextureBrush(Image.FromFile(localpath + "\\Defaults\Materials\HMA.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-401/P-403 HMA Stabilized", New TextureBrush(Image.FromFile(localpath + "\\Defaults\Materials\HMA.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Surface", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Overlay (unbonded)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Overlay (partially bonded)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Overlay on Flexible", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("Subgrade", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\General.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("User Defined", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\UserDefined2.jpg"), Drawing2D.WrapMode.Tile))
            brushes.Add("Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Aggregate.jpg"), Drawing2D.WrapMode.TileFlipXY))

            Return brushes
        End Function

        Public Function LoadBrushes2() As Dictionary(Of String, Brush)

            Dim localpath As String = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath)

            Dim brushes = New Dictionary(Of String, Brush)

            brushes.Add("P-301 Soil Cement Base", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-304 Cement Treated Base", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-306 Lean Concrete", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("Variable (flexible)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("Variable (rigid)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Stabilized-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-209 Crushed Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\AggregateCr-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-208 Crushed Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\AggregateCr-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-154 Uncrushed Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\AggregateUnCr-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-211 Lime Rock", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\AggregateCr-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-219 Recycled Concrete Aggregate", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\AggregateCr-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-401/P-403 HMA Surface", New TextureBrush(Image.FromFile(localpath + "\\Defaults\Materials\HMA-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-401/P-403 HMA Overlay", New TextureBrush(Image.FromFile(localpath + "\\Defaults\Materials\HMA-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-401/P-403 HMA Stabilized", New TextureBrush(Image.FromFile(localpath + "\\Defaults\Materials\HMA-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Surface", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Overlay (unbonded)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Overlay (partially bonded)", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("P-501 PCC Overlay on Flexible", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\PCC-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("Subgrade", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\Subgrade-Hatch.jpg"), Drawing2D.WrapMode.TileFlipXY))
            brushes.Add("User Defined", New TextureBrush(Image.FromFile(localpath + "\Defaults\Materials\UserDefined2.jpg"), Drawing2D.WrapMode.Tile))
            'brushes.Add("Aggregate", New TextureBrush(Image.FromFile(Application.StartupPath + "\Defaults\Materials\Aggregate.jpg"), Drawing2D.WrapMode.TileFlipXY))

            Return brushes
        End Function

        Public Sub PaintUserDefinedGear(g As Graphics, CurrentWheel As ObservableCollection(Of UserDefinedWheel), CurrentEval As ObservableCollection(Of UserDefinedEvalPoints), PictureBox As PictureBox, font As Font, measurementSystem As IMeasurmentSystem, TS As Single)
            g.Clear(Color.White)
            Dim x As Single
            Dim y As Single
            '  Dim Ts As Double
            'Dim TT As Double
            'Dim B As Double
            ''B = airplane.B.UsCustomary

            ''Ts = Format(airplane.Ts, "0.0")
            ''TT = Format(airplane.Tt.UsCustomary, "0.0")

            'Dim offsetx As Double
            ''offsetx = (TS + TT) / 2
            'Dim offsety As Double
            ''If Not TypeOf (measurementSystem) Is UsCustomary Then
            ''    Ts = Ts * 25.4
            ''    TT = TT * 25.4
            ''    offsetx = offsetx * 25.4
            ''End If


            g.DrawLine(Pens.Black, New Point(10, (PictureBox.Height / 2)), New Point(PictureBox.Width - 10, (PictureBox.Height / 2)))
            g.DrawLine(Pens.Black, New Point((PictureBox.Width / 2), 10), New Point(PictureBox.Width / 2, (PictureBox.Height) - 10))

            'If CurrentWheel.Count = 0 Then Return
            font = New Font(font.Name, 11)
            ' g.DrawString("Airplane: " + AircraftName, font, Brushes.Black, New PointF(5, 5))

            Dim maximumDimension As Single = 0
            Dim maximumY As Single = Single.MinValue
            Dim minimumY As Single = Single.MaxValue

            'If Not TypeOf (measurementSystem) Is UsCustomary Then
            '    Ts = Ts * 25.4
            '    TT = TT * 25.4
            '    offsetx = offsetx * 25.4
            'End If


            'For Each wheelCoordinate In airplane.WheelCoordinates
            '    If wheelCoordinate.X.GetValue(measurementSystem) + offsetx > maximumDimension Then
            '        maximumDimension = wheelCoordinate.X.GetValue(measurementSystem) + offsetx
            '    End If

            '    If wheelCoordinate.Y.GetValue(measurementSystem) > maximumDimension Then
            '        maximumDimension = wheelCoordinate.Y.GetValue(measurementSystem)
            '    End If

            '    If wheelCoordinate.Y.GetValue(measurementSystem) < minimumY Then
            '        minimumY = wheelCoordinate.Y.GetValue(measurementSystem)
            '    End If

            '    If wheelCoordinate.Y.GetValue(measurementSystem) > maximumY Then
            '        maximumY = wheelCoordinate.Y.GetValue(measurementSystem)
            '    End If
            'Next
            'offsety = (maximumY - minimumY) / 2

            ' Draw wheels
            'Dim scale As Single = 0
            'If TypeOf (measurementSystem) Is UsCustomary Then
            '    scale = 125
            '    'If maximumDimension < 25 Then
            '    '    scale = 12.5
            '    'ElseIf maximumDimension < 50 Then
            '    '    scale = 25
            '    'ElseIf maximumDimension < 100 Then
            '    '    scale = 50
            '    'ElseIf maximumDimension < 200 Then
            '    '    scale = 100

            '    'Else

            '    '    scale = 200
            '    'End If
            'Else
            '    'If maximumDimension < 10 Then
            '    '    scale = 10
            '    'ElseIf maximumDimension < 20 Then
            '    '    scale = 20
            '    'ElseIf maximumDimension < 40 Then
            '    '    scale = 40
            '    'Else
            '    '    scale = 80
            '    'End If
            '    scale = 3000
            'End If

            ' Draw wheels
            Dim scale As Single = 0
            Dim scaleRatio As Single = 1
            If TypeOf (measurementSystem) Is UsCustomary Then
                'If airplane IsNot Nothing AndAlso airplane.Name.Contains("B-52") Then
                '    scale = 325
                'ElseIf airplane IsNot Nothing AndAlso airplane.Name.Contains("A380") Then
                '    scale = 150
                'Else
                '    scale = 125
                'End If

                scale = 125
                scaleRatio = 125 / scale
            Else
                'If airplane IsNot Nothing AndAlso airplane.Name.Contains("B-52") Then
                '    scale = 8255
                'ElseIf airplane IsNot Nothing AndAlso airplane.Name.Contains("A380") Then
                '    scale = 3810
                'Else
                '    scale = 3175
                'End If

                scale = 3175
                scaleRatio = 3175 / scale
            End If


            'Each unit is 160 (half of the constant height)


            ' Draw axes
            Dim ratio = 150 / scale
            If Not TypeOf (measurementSystem) Is UsCustomary Then
                Dim i As Single
                For i = -10 To 10 Step 3.333333333333
                    If i = 0 Then Continue For
                    Dim d = scale * i / 5

                    Dim labelWidth = g.MeasureString(d.ToString("f0"), font).Width
                    Dim labelHeight = g.MeasureString(d.ToString("f0"), font).Height
                    'Horizontal
                    Dim tick = PictureBox.Width / 2 + (i * scale * ratio / 5)
                    g.DrawLine(Pens.Black, New Point(tick, PictureBox.Height / 2 + 4), New Point(tick, PictureBox.Height / 2 - 3))

                    g.DrawString(d.ToString("f0"), font, Brushes.Black, New Point(tick - labelWidth / 2, PictureBox.Height / 2 + 5))

                    'Vertical
                    tick = PictureBox.Height / 2 + (i * scale * ratio / 5)
                    g.DrawLine(Pens.Black, New Point(PictureBox.Width / 2 + 4, tick), New Point(PictureBox.Width / 2 - 3, tick))
                    d = d * -1
                    g.DrawString(d.ToString("f0"), font, Brushes.Black, New Point(PictureBox.Width / 2 + 5, tick - labelHeight / 2))
                Next

            Else
                Dim i As Integer
                For i = -10 To 10 Step 2
                    If i = 0 Then Continue For
                    Dim d = scale * i / 5

                    Dim labelWidth = g.MeasureString(d.ToString("f0"), font).Width
                    Dim labelHeight = g.MeasureString(d.ToString("f0"), font).Height
                    'Horizontal
                    Dim tick = PictureBox.Width / 2 + (i * scale * ratio / 5)
                    g.DrawLine(Pens.Black, New Point(tick, PictureBox.Height / 2 + 4), New Point(tick, PictureBox.Height / 2 - 3))


                    g.DrawString(d.ToString("f0"), font, Brushes.Black, New Point(tick - labelWidth / 2, PictureBox.Height / 2 + 5))

                    'Vertical
                    tick = PictureBox.Height / 2 + (i * scale * ratio / 5)
                    g.DrawLine(Pens.Black, New Point(PictureBox.Width / 2 + 4, tick), New Point(PictureBox.Width / 2 - 3, tick))
                    d = d * -1
                    g.DrawString(d.ToString("f0"), font, Brushes.Black, New Point(PictureBox.Width / 2 + 5, tick - labelHeight / 2))
                Next
            End If

            Dim ScaledPictureBox As Single = 320 / 270

            For Each UserDefinedWheel In CurrentWheel

                If UserDefinedWheel.TireX IsNot Nothing AndAlso UserDefinedWheel.TireY IsNot Nothing Then

                    'Wheel
                    x = PictureBox.Width / 2 + (UserDefinedWheel.TireX.UsCustomary) * scaleRatio * ScaledPictureBox
                    y = PictureBox.Height / 2 - (UserDefinedWheel.TireY.UsCustomary) * scaleRatio * ScaledPictureBox
                    g.FillEllipse(Brushes.Black, x - 6, y - 12, 12, 24)

                    'Mirrored Wheel
                    x = PictureBox.Width / 2 - (UserDefinedWheel.TireX.UsCustomary) * scaleRatio * ScaledPictureBox
                    y = PictureBox.Height / 2 - (UserDefinedWheel.TireY.UsCustomary) * scaleRatio * ScaledPictureBox
                    g.FillEllipse(Brushes.Black, x - 6, y - 12, 12, 24)

                End If

            Next

            For Each UserDefinedEvalPoints In CurrentEval

                If UserDefinedEvalPoints.EvalX IsNot Nothing AndAlso UserDefinedEvalPoints.EvalY IsNot Nothing Then

                    x = PictureBox.Width / 2 + (UserDefinedEvalPoints.EvalX.UsCustomary) * scaleRatio * ScaledPictureBox
                    y = PictureBox.Height / 2 - (UserDefinedEvalPoints.EvalY.UsCustomary) * scaleRatio * ScaledPictureBox
                    g.FillEllipse(Brushes.Black, x, y, 2, 4)

                End If

            Next

            If TypeOf (measurementSystem) Is UsCustomary Then
                g.DrawString("(Inches)", font, Brushes.Black, New Point(PictureBox.Width / 2 + 45, 10))
                g.DrawString("(Inches)", font, Brushes.Black, New Point(PictureBox.Width / 2 + 45, 10))
            Else
                g.DrawString("(Millimeters)", font, Brushes.Black, New Point(PictureBox.Width / 2 + 45, 10))
            End If

        End Sub

    End Module

End Namespace
