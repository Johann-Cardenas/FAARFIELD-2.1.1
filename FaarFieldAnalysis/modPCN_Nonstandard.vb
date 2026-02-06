Option Strict Off
Option Explicit On
Imports Microsoft.VisualBasic
Imports System
Imports System.Windows.Forms

Module modPCN_Nonstandard

    Const PI As Double = 3.14159265359
    Public StressA, StressB As Double
    Public Stress1, Stress2 As Double
    Public YWheelsNew(65) As Double
    Public XWheelsNew(65) As Double
    Public S1Max, S2Max As Double

    Public Structure InputData
        Dim Thickness As Double
        Dim XK As Double
        Dim NOG As Integer
        Dim Angle As Double
        Dim GearLoad As Double
        Dim TirePressure As Double
        Dim Delta As Double
        Dim XNA As Double
        Dim XNB As Double
        Dim XNC As Double
        Dim XND As Double
        Dim XLA As Double
        Dim XLB As Double
        Dim XLC As Double
        Dim XLD As Double
    End Structure

    Public DesignInput As New InputData() ' Used in function EdgeStress (modH51inVB)

    Public Function RTBIS(ByRef X1 As Double, ByRef X2 As Double, ByRef XACC As Double, ByRef FVal As Double, ByRef Conf As Boolean, ByRef MaxStress As Double) As Double
        'adopted from Numerical Recipes - The Art of Scientific Computing, 1986 page 247
        Dim result As Double = 0
        Dim F As Double

        Dim JMAX As Integer = 200 ' 40 ' GFH 07/14/08.
        result = X1
        Dim DX As Double = X2 - X1
        DX *= 0.5
        Dim XMID As Double = result + DX
        Dim FMID As Double = StressValue(XMID, Conf)
        result = XMID

        For J As Integer = 1 To JMAX

            If Not ComputingRigid Then Return result

            DX *= 0.5

            If FMID > MaxStress Then
                XMID = result + DX
            Else
                XMID = result - DX
            End If

            FMID = StressValue(XMID, Conf)
            result = XMID

            '   Debug.Print "J=", J, "  XMID=", Round(XMID, 3)

            If Math.Abs(DX) <= XACC Or FMID = 0 Then 'quit
                J = JMAX
            End If

        Next J

        FVal = FMID

        Return result
    End Function

    Public Function StressValue(ByRef Thickness As Double, ByRef StdConfiguration As Boolean) As Double

        DesignInput.Thickness = Thickness

        If StdConfiguration Then
            Return EdgeStress(DesignInput)
        Else
            Return MaxStressNonStdGear()
        End If

    End Function

    Public Function MaxStressNonStdGear() As Double
        Dim S1, S2 As Double
        Dim BX, AX, CX As Double
        Static istatic As Object

        If Not ComputingRigid Then Exit Function

        Dim XMin As Double = 100000
        Dim XMax As Double = -100000
        For I As Integer = 1 To NWheels1
            If XMin > XWheels1(I) Then
                XMin = XWheels1(I)
            End If
            If XMax < XWheels1(I) Then
                XMax = XWheels1(I)
            End If
        Next I

        Dim XRange As Double = XMax - XMin

        Dim Ymin As Double = 100000
        Dim YMax As Double = -100000
        For I As Integer = 1 To NWheels1
            If Ymin > YWheels1(I) Then
                Ymin = YWheels1(I)
            End If
            If YMax < YWheels1(I) Then
                YMax = YWheels1(I)
            End If
        Next I

        Dim YRange As Double = YMax - Ymin

        If XMin < 0 Then
            For I As Integer = 1 To NWheels1
                XWheelsNew(I) = XWheels1(I) + Math.Abs(XMin)
            Next I
        Else
            For I As Integer = 1 To NWheels1
                XWheelsNew(I) = XWheels1(I) - Math.Abs(XMin)
            Next I
        End If

        If Ymin < 0 Then
            For I As Integer = 1 To NWheels1
                YWheelsNew(I) = YWheels1(I) + Math.Abs(Ymin)
            Next I
        Else
            For I As Integer = 1 To NWheels1
                YWheelsNew(I) = YWheels1(I) - Math.Abs(Ymin)
            Next I
        End If

        SingleWheel = GrossWeight * PcntOnMainGears / 100 / NMainGears / NWheels1
        DesignInput.Angle = 0
        Dim Angle As Boolean = True
        S1Max = GoldenSearch(YWheelsNew(1), YRange, XMin, Angle)

        DesignInput.Angle = 90
        Angle = False
        S2Max = GoldenSearch(XWheelsNew(1), XRange, XMin, Angle)

        If S1Max > S2Max Then
            Return S1Max
        Else
            Return S2Max
        End If

    End Function

    Public Function GoldenSearch(ByRef WheelCord As Double, ByRef Range As Double, ByRef XMin As Double, ByRef ZeroDeg As Boolean) As Double
        'adopted from Numerical Recipes - The Art of Scientific Computing, 1986 page 282
        Dim result As Double = 0
        Dim X1, X2, F0, F3, tempX2 As Double
        Const R As Double = 0.61803399
        Const C As Double = 1 - R
        Const TOL As Double = 0.01

        Dim AX As Double = (-Range) * 0.25
        Dim BX As Double = WheelCord
        Dim CX As Double = Range * 1.25

        Dim X0 As Double = AX
        Dim X3 As Double = CX

        If Math.Abs(CX - BX) > Math.Abs(BX - AX) Then
            X1 = BX
            X2 = BX + C * (CX - BX)
        Else
            X2 = BX
            X1 = BX - C * (BX - AX)
        End If

        Dim F1 As Double = -StressNWheels(X1, ZeroDeg)
        Dim F2 As Double = -StressNWheels(X2, ZeroDeg)

1:      If Math.Abs(X3 - X0) > TOL * (Math.Abs(X1) + Math.Abs(X2)) Then
            If F2 < F1 Then
                X0 = X1
                X1 = X2
                X2 = R * X1 + C * X3
                F0 = F1
                F1 = F2
                F2 = -StressNWheels(X2, ZeroDeg)
            Else
                X3 = X2
                X2 = X1
                X1 = R * X2 + C * X0
                F3 = F2
                F2 = F1
                F1 = -StressNWheels(X1, ZeroDeg)
            End If
            GoTo 1
        End If

        If F1 < F2 Then
            result = -F1
            XMin = X1
        Else
            result = -F2
            XMin = X2
        End If

        Return result
    End Function

    Public Function StressNWheels(ByRef displ As Double, ByRef ZeroDeg As Boolean) As Double
        Dim result As Double = 0
        StressA = 0.0#

        For I As Integer = 1 To NWheels1
            '(transverse distance, longitudinal distance)
            If ZeroDeg Then
                Stress1 = Stress1Wheel(Math.Abs(YWheelsNew(I) - displ), Math.Abs(XWheelsNew(I)))
            Else
                Stress1 = Stress1Wheel(Math.Abs(YWheelsNew(I)), Math.Abs(XWheelsNew(I) - displ))
            End If
            StressA += Stress1
        Next I

        result = StressA

        If Not ComputingRigid Then
            Return result
        End If

        Return result
    End Function

    Public Function Stress1Wheel(ByRef trans As Double, ByRef longit As Double) As Double
        Dim s2a, s1a, s3a, GearLoad As Double

        If trans = 0 And longit = 0 Then

            '  Single
            DesignInput.GearLoad = SingleWheel
            '  DesignInput.libGear = "B"
            DesignInput.Delta = 0
            DesignInput.GearLoad = SingleWheel
            DesignInput.XNA = 1 : DesignInput.XNB = 1 : DesignInput.XNC = 1 : DesignInput.XND = 1
            DesignInput.XLA = 0 : DesignInput.XLB = 0 : DesignInput.XLC = 0 : DesignInput.XLD = 0
            Return EdgeStress(DesignInput)

        ElseIf longit = 0 Then

            '  Single
            DesignInput.GearLoad = SingleWheel
            '  DesignInput.libGear = "B"
            DesignInput.Delta = 0
            DesignInput.XNA = 1 : DesignInput.XNB = 1 : DesignInput.XNC = 1 : DesignInput.XND = 1
            DesignInput.XLA = 0 : DesignInput.XLB = 0 : DesignInput.XLC = 0 : DesignInput.XLD = 0
            s1a = EdgeStress(DesignInput)
            '  Twin
            DesignInput.GearLoad = SingleWheel * 2
            '  DesignInput.libGear = "D"
            DesignInput.Delta = 0
            DesignInput.XNA = 2 : DesignInput.XNB = 1 : DesignInput.XNC = 1 : DesignInput.XND = 1
            DesignInput.XLA = trans : DesignInput.XLB = 0 : DesignInput.XLC = 0 : DesignInput.XLD = 0
            s2a = EdgeStress(DesignInput)
            Return s2a - s1a

        ElseIf trans = 0 Then

            '  Single
            DesignInput.GearLoad = SingleWheel
            '  DesignInput.libGear = "B"
            DesignInput.Delta = 0
            DesignInput.XNA = 1 : DesignInput.XNB = 1 : DesignInput.XNC = 1 : DesignInput.XND = 1
            DesignInput.XLA = 0 : DesignInput.XLB = 0 : DesignInput.XLC = 0 : DesignInput.XLD = 0
            s1a = EdgeStress(DesignInput)
            '  Single Tandem
            DesignInput.GearLoad = SingleWheel * 2
            '  DesignInput.libGear = "E"
            DesignInput.Delta = 0
            DesignInput.XNA = 1 : DesignInput.XNB = 1 : DesignInput.XNC = 2 : DesignInput.XND = 1
            DesignInput.XLA = 0 : DesignInput.XLB = 0 : DesignInput.XLC = longit : DesignInput.XLD = 0
            s2a = EdgeStress(DesignInput)
            Return s2a - s1a

        Else
            '(longit<>0 , trans<>0)

            If DesignInput.Angle = 0 Then
                '    Single displaced
                DesignInput.GearLoad = SingleWheel
                '    DesignInput.libGear = "B"
                DesignInput.Delta = trans
                DesignInput.XNA = 1 : DesignInput.XNB = 1 : DesignInput.XNC = 1 : DesignInput.XND = 1
                DesignInput.XLA = 0 : DesignInput.XLB = 0 : DesignInput.XLC = 0 : DesignInput.XLD = 0
                s1a = EdgeStress(DesignInput)
                '    Single Tandem
                DesignInput.GearLoad = SingleWheel * 2
                '    DesignInput.libGear = "E"
                DesignInput.Delta = 0
                DesignInput.XNA = 1 : DesignInput.XNB = 1 : DesignInput.XNC = 2 : DesignInput.XND = 1
                DesignInput.XLA = 0 : DesignInput.XLB = 0 : DesignInput.XLC = longit : DesignInput.XLD = 0
                s2a = EdgeStress(DesignInput)

            Else

                '    Single displaced
                DesignInput.GearLoad = SingleWheel
                '    DesignInput.libGear = "B"
                DesignInput.Delta = longit
                DesignInput.XNA = 1 : DesignInput.XNB = 1 : DesignInput.XNC = 1 : DesignInput.XND = 1
                DesignInput.XLA = 0 : DesignInput.XLB = 0 : DesignInput.XLC = 0 : DesignInput.XLD = 0
                s1a = EdgeStress(DesignInput)
                '    Twin
                DesignInput.GearLoad = SingleWheel * 2
                '    DesignInput.libGear = "D"
                DesignInput.Delta = 0
                DesignInput.XNA = 2 : DesignInput.XNB = 1 : DesignInput.XNC = 1 : DesignInput.XND = 1
                DesignInput.XLA = trans : DesignInput.XLB = 0 : DesignInput.XLC = 0 : DesignInput.XLD = 0
                s2a = EdgeStress(DesignInput)

            End If

            '  Twin Tandem (OK!)
            DesignInput.GearLoad = SingleWheel * 4
            '  DesignInput.libGear = "F"
            DesignInput.Delta = 0
            DesignInput.XNA = 2 : DesignInput.XNB = 1 : DesignInput.XNC = 2 : DesignInput.XND = 1
            DesignInput.XLA = trans : DesignInput.XLB = 0 : DesignInput.XLC = longit : DesignInput.XLD = 0
            s3a = EdgeStress(DesignInput)
            Return s3a - s2a - s1a

        End If

    End Function
End Module
