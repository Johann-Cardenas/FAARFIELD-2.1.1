Option Strict Off
Option Explicit On
Imports System

Module modPCN_ThicknessDesign

    Private AllowableStress, BoxVar As Double
    Private MaxStress, Alpha, I5, Stress As Double
    Private StdConfiguration As Boolean
    Public SingleWheel As Double


    Public Sub ThicknessDesign()

        Dim X2, X1, XACC, FVal, d5, d3, d1, D2, D4, D6, D10, D8, d7, d9, d11, dmin, dmax, aa As Double
        Dim i1, i2 As Integer

        Try 'added kawa 2013

            If WheelPosChanged() Then
                EditWheels = True
            End If


            If LI >= 234 Then
                ILibACGroup = 9 'External Library
            Else
                ILibACGroup = 0
            End If


            'If (AC(LI).libGear(I) = "A" Or AC(LI).libGear(I) = "B" Or AC(LI).libGear(I) = "D" _
            '    Or AC(LI).libGear(I) = "Y" Or AC(LI).libGear(I) = "E" Or AC(LI).libGear(I) = "F" _
            '    Or AC(LI).libGear(I) = "H" Or AC(LI).libGear(I) = "J" Or AC(LI).libGear(I) = "X") _
            '    And Not EditWheels And Not (LibACGroupName(ILibACGroup) = "External Library") Then
            '    StdConfiguration = True
            'Else
            '    StdConfiguration = False
            'End If




            If EditWheels And (Symmetry = Global_Renamed.SymmetryType.NoSymmetry Or Symmetry = Global_Renamed.SymmetryType.XSymmetry Or Symmetry = Global_Renamed.SymmetryType.YSymmetry) Then

                StdConfiguration = False

            Else

                '     libGear$(libIndex) = "N") Or _ GFH 03/13/06. Removed from If statement because
                '     libB() replaced with libBF and libBR for later B777s with unequal tandem spacings.
                '     libB() is used in Sub InitVar, so, for now, compute stress wheel-by-wheel.

                'If (libGear(libIndex) = "A" Or libGear(libIndex) = "B" Or libGear(libIndex) = "D" Or libGear(libIndex) = "Y" Or libGear(libIndex) = "E" Or libGear(libIndex) = "F" Or libGear(libIndex) = "H" Or libGear(libIndex) = "J" Or libGear(libIndex) = "X") And Not EditWheels And Not (LibACGroupName(ILibACGroup) = "External Library") Then
                If (AC(LI).libGear = "A" Or AC(LI).libGear = "B" Or AC(LI).libGear = "D" Or
                   AC(LI).libGear = "Y" Or AC(LI).libGear = "E" Or AC(LI).libGear = "F" Or
                    AC(LI).libGear = "H" Or AC(LI).libGear = "J" Or AC(LI).libGear = "X") _
                    And Not EditWheels And Not (LibACGroupName(ILibACGroup) = "External Library") Then
                    StdConfiguration = True
                Else

                    If (NWheels1 = 2) And Symmetry = Global_Renamed.SymmetryType.XYSymmetry Then
                        d1 = Math.Abs(XWheels1(2) - XWheels1(1))
                        D2 = Math.Abs(YWheels1(2) - YWheels1(1))

                        If D2 > d1 Then
                            'libGear(LibIndex) = "D" 'PPPP
                            'AC(LI).libGear(I) = "D" 'PPPP

                            'libTT(LibIndex) = D2 'PPPP
                            AC(LI).libTT = D2 'PPPP
                        Else
                            'libGear(LibIndex) = "E" 'PPPP

                            'libTY(LibIndex, 2) = XWheels1(2)
                            'libTY(LibIndex, 1) = XWheels1(1)
                            AC(LI).libTY(2) = XWheels1(2)
                            AC(LI).libTY(1) = XWheels1(1)

                        End If

                        StdConfiguration = True
                    Else
                        StdConfiguration = False
                    End If

                    If (NWheels1 = 6) And Symmetry = Global_Renamed.SymmetryType.XYSymmetry Then

                        'longitudinal
                        d1 = Math.Abs(XWheels1(2) - XWheels1(1))
                        D2 = Math.Abs(XWheels1(3) - XWheels1(1))
                        d3 = Math.Abs(XWheels1(4) - XWheels1(1))
                        D4 = Math.Abs(XWheels1(5) - XWheels1(1))
                        d5 = Math.Abs(XWheels1(6) - XWheels1(1))

                        dmax = 0
                        If d1 > dmax Then
                            dmax = d1
                        End If
                        If D2 > dmax Then
                            dmax = D2
                        End If
                        If d3 > dmax Then
                            dmax = d3
                        End If
                        If D4 > dmax Then
                            dmax = D4
                        End If
                        If d5 > dmax Then
                            dmax = d5
                        End If
                        If D6 > dmax Then
                            dmax = D6
                        End If

                        'libB(LibIndex) = dmax / 2 'Longitudinal spacing between tires
                        AC(LI).libB = dmax / 2 'Longitudinal spacing between tires

                        d1 = Math.Abs(YWheels1(2) - YWheels1(1))
                        D2 = Math.Abs(YWheels1(3) - YWheels1(1))
                        d3 = Math.Abs(YWheels1(4) - YWheels1(1))
                        D4 = Math.Abs(YWheels1(3) - YWheels1(2))
                        d5 = Math.Abs(YWheels1(4) - YWheels1(2))
                        D6 = Math.Abs(YWheels1(4) - YWheels1(3))

                        dmax = 0
                        If d1 > dmax Then
                            dmax = d1
                        End If
                        If D2 > dmax Then
                            dmax = D2
                        End If
                        If d3 > dmax Then
                            dmax = d3
                        End If
                        If D4 > dmax Then
                            dmax = D4
                        End If
                        If d5 > dmax Then
                            dmax = d5
                        End If
                        If D6 > dmax Then
                            dmax = D6
                        End If

                        'libTT(LibIndex) = dmax 'Transverse spacing between tires
                        AC(LI).libTT = dmax 'Transverse spacing between tires

                        'If libB(LibIndex) > 2 And libTT(LibIndex) > 1 Then 'resolution is to 1 inch
                        If AC(LI).libB > 2 And AC(LI).libTT > 1 Then 'resolution is to 1 inch
                            'libGear(LibIndex) = "N"
                            StdConfiguration = True
                        Else
                            'libGear(LibIndex) = ""
                            StdConfiguration = False
                        End If

                    Else
                        If NWheels1 = 6 Then
                            StdConfiguration = False
                        End If
                    End If

                    '***********************************************************************************

                    If (NWheels1 = 4) And Symmetry = Global_Renamed.SymmetryType.XYSymmetry Then

                        'longitudinal
                        d1 = Math.Abs(XWheels1(2) - XWheels1(1))
                        D2 = Math.Abs(XWheels1(3) - XWheels1(1))
                        d3 = Math.Abs(XWheels1(4) - XWheels1(1))
                        D4 = Math.Abs(XWheels1(3) - XWheels1(2))
                        d5 = Math.Abs(XWheels1(4) - XWheels1(2))
                        D6 = Math.Abs(XWheels1(4) - XWheels1(3))

                        dmax = 0
                        If d1 > dmax Then
                            dmax = d1
                        End If
                        If D2 > dmax Then
                            dmax = D2
                        End If
                        If d3 > dmax Then
                            dmax = d3
                        End If
                        If D4 > dmax Then
                            dmax = D4
                        End If
                        If d5 > dmax Then
                            dmax = d5
                        End If
                        If D6 > dmax Then
                            dmax = D6
                        End If

                        'libB(LibIndex) = dmax 'Longitudinal spacing between tires
                        AC(LI).libB = dmax 'Longitudinal spacing between tires

                        d1 = Math.Abs(YWheels1(2) - YWheels1(1))
                        D2 = Math.Abs(YWheels1(3) - YWheels1(1))
                        d3 = Math.Abs(YWheels1(4) - YWheels1(1))
                        D4 = Math.Abs(YWheels1(3) - YWheels1(2))
                        d5 = Math.Abs(YWheels1(4) - YWheels1(2))
                        D6 = Math.Abs(YWheels1(4) - YWheels1(3))

                        dmax = 0
                        If d1 > dmax Then
                            dmax = d1
                        End If
                        If D2 > dmax Then
                            dmax = D2
                        End If
                        If d3 > dmax Then
                            dmax = d3
                        End If
                        If D4 > dmax Then
                            dmax = D4
                        End If
                        If d5 > dmax Then
                            dmax = d5
                        End If
                        If D6 > dmax Then
                            dmax = D6
                        End If

                        'libTT(LibIndex) = dmax 'Transverse spacing between tires
                        AC(LI).libTT = dmax 'Transverse spacing between tires

                        'If libB(LibIndex) > 1 And libB(LibIndex) > 1 Then 'resolution is to 1 inch
                        If AC(LI).libB > 1 And AC(LI).libB > 1 Then 'resolution is to 1 inch
                            'libGear(LibIndex) = "F"
                            StdConfiguration = True
                        Else
                            'If libB(LibIndex) = 0 Then
                            If AC(LI).libB = 0 Then

                                d1 = Math.Abs(YWheels1(2) - YWheels1(1))
                                D2 = Math.Abs(YWheels1(3) - YWheels1(1))
                                d3 = Math.Abs(YWheels1(4) - YWheels1(1))
                                D4 = Math.Abs(YWheels1(3) - YWheels1(2))
                                d5 = Math.Abs(YWheels1(4) - YWheels1(2))
                                D6 = Math.Abs(YWheels1(4) - YWheels1(3))

                                dmax = 0
                                If d1 > dmax Then
                                    dmax = d1 : i1 = 1
                                End If
                                If D2 > dmax Then
                                    dmax = D2 : i1 = 2
                                End If
                                If d3 > dmax Then
                                    dmax = d3 : i1 = 3
                                End If
                                If D4 > dmax Then
                                    dmax = D4 : i1 = 4
                                End If
                                If d5 > dmax Then
                                    dmax = d5 : i1 = 5
                                End If
                                If D6 > dmax Then
                                    dmax = D6 : i1 = 6
                                End If

                                Select Case i1
                                    Case 1
                                        aa = Math.Abs(YWheels1(4) - YWheels1(3))
                                    Case 2
                                        aa = Math.Abs(YWheels1(4) - YWheels1(2))
                                    Case 3
                                        aa = Math.Abs(YWheels1(3) - YWheels1(2))
                                    Case 4
                                        aa = Math.Abs(YWheels1(4) - YWheels1(1))
                                    Case 5
                                        aa = Math.Abs(YWheels1(3) - YWheels1(1))
                                    Case 6
                                        aa = Math.Abs(YWheels1(2) - YWheels1(1))
                                End Select

                                'libTX(LibIndex, 3) = aa / 2
                                'libTX(LibIndex, 2) = (-aa) / 2
                                'libTX(LibIndex, 4) = dmax / 2

                                AC(LI).libTX(3) = aa / 2
                                AC(LI).libTX(2) = (-aa) / 2
                                AC(LI).libTX(4) = dmax / 2


                                'libGear(LibIndex) = "Y"
                                StdConfiguration = True

                            Else
                                'libGear(LibIndex) = ""
                                StdConfiguration = False
                            End If
                        End If

                    Else
                        If NWheels1 = 4 Then
                            StdConfiguration = False
                        End If
                    End If

                End If

            End If

            If InputkValue > 0 Then

                AllowableStress = ConcreteFlexuralStrength / 1.3 / 0.75
                '  AllowableStress = 700 / 1.3 / 0.75
                MaxStress = AllowableStress 'MaxStress - Adjusted Allowable Stress

                '   ikawa seattle
                'Dim rbEdgeStress = CType(My.Application.FindName(Of MainWindow)("RadioEdgeStress"), Controls.RadioButton)
                'Dim txtEvaluationThickness = CType(My.Application.FindName(Of MainWindow)("TextBoxEvaluationThickness"), Controls.TextBox)
                'Dim txtStress = CType(My.Application.FindName(Of MainWindow)("TextBoxStress"), Controls.TextBox)

                Dim rbEdgeStress As Boolean
                Dim txtEvaluationThickness As Boolean
                Dim txtStress As Boolean

                If rbEdgeStress Then

                    'PPPP I = CDbl(txtEvaluationThickness.Text)
                    I5 = 16 'PPPP Evaluation thickness

                    InitVar(I5, StdConfiguration, DesignInput)

                    If StdConfiguration Then
                        Stress = EdgeStress(DesignInput)
                    Else
                        Stress = MaxStressNonStdGear()
                    End If

                    'txtStress.Text = CStr(Math.Round(CDbl(Stress), 2))
                    Exit Sub

                End If

                I5 = 8

                Do
                    I5 += 4
                    DesignInput.Thickness = I5
                    If StdConfiguration Then
                        InitVar(I5, StdConfiguration, DesignInput)
                        Stress = EdgeStress(DesignInput)
                    Else
                        InitVar(I5, StdConfiguration, DesignInput)
                        Stress = MaxStressNonStdGear()
                    End If
                Loop While (MaxStress < Stress)
                ' Debug.Print "MaxStress, Stress = "; MaxStress; Stress
                If I5 > 12 Then
                    X1 = I5 - 4
                    X2 = I5
                Else
                    X1 = 0
                    X2 = I5
                End If

                XACC = 0.0001 ' 0.01 ' GFH 07/14/08.

                I5 = RTBIS(X1, X2, XACC, FVal, StdConfiguration, MaxStress)
                DesignInput.Thickness = I5 'IK 10/06/08
                Stress = EdgeStress(DesignInput) 'IK 10/06/08

                If Not ComputingRigid Then
                    Exit Sub
                End If

                If Coverages >= 5000 Then
                    Alpha = 1 + 0.15603 * Log10PCN(Coverages / 5000)
                Else
                    Alpha = 1 + 0.07058 * Log10PCN(Coverages / 5000)
                End If

                I5 *= Alpha
                ACNRigidk(1) = InputkValue
                RigidThickness(1) = I5
                RigidStress(1) = MaxStress / XPRES ' Convert to MPa.
                '  Debug.Print "Alpha = "; Alpha; "Thick = "; I; "Coverages = "; Coverages; "I = "; I / Alpha

                'PPPP WriteOutputGrid()
                'PPPP WriteRigidOutputData()

                Dim I As Integer

                With UnitsOut
                    S = IIf(UnitsOut.Metric, "Metric Units", "English Units")
                    SSS = JobTitle & ", " & S & Environment.NewLine & Environment.NewLine
                    SSS = SSS & "  WEIGHT    PCNT ON MAIN    NO. OF WHLS.  CONTACT AREA   CONTACT PRESSURE" & Environment.NewLine
                    SSS = SSS & LPad(9, Format(GrossWeight * .pounds, .poundsFormat))
                    SSS = SSS & LPad(12, Format(PcntOnMainGears, "0.00"))
                    SSS = SSS & LPad(13, Format(NWheels1, "0"))
                    'PPPP SSS = SSS & LPad(17, Format(TireContactArea * .squareInch, .squareInchFormat))
                    SSS = SSS & LPad(17, Format(TirePressure * .psi, .psiFormat)) & Environment.NewLine & Environment.NewLine
                    SSS = SSS & "Coordinates of wheels" & Environment.NewLine
                    SSS = SSS & "           End ACN" & Environment.NewLine
                    SSS = SSS & " No.      X       Y" & Environment.NewLine
                    For I = 1 To NWheels1
                        SSS = SSS & LPad(3, Format(I, "0"))
                        SSS = SSS & LPad(10, Format(XWheels1(CInt(I)) * .inch, .inchFormat))
                        SSS = SSS & LPad(8, Format(YWheels1(CInt(I)) * .inch, .inchFormat)) & Environment.NewLine
                    Next I
                    SSS = SSS & Environment.NewLine
                    SSS = SSS & " SUPPORT   PAVEMENT   " & Environment.NewLine
                    SSS = SSS & " K VALUE   THICKNESS  STRESS" & Environment.NewLine
                    SSS = SSS & LPad(8, Format(InputkValue * .pci, .pciFormat))
                    SSS = SSS & LPad(10, Format(RigidThickness(1) * .inch, .inchFormat))
                    SSS = SSS & LPad(10, Format(Stress * .psi, .psiFormat))
                    SSS = SSS & Environment.NewLine
                    ACNRigidOutputText = SSS
                End With

            End If 'InputkValue > 0 Then

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Sub

    Function WheelPosChanged() As Boolean
        Dim result As Boolean = False


        'If NWheels <> libNTires(libIndex) Then result = True
        If NWheels1 <> AC(LI).libNTires Then result = True

        For I As Integer = 1 To NWheels1
            If XWheels1(I) <> AC(LI).libTY(I) Then result = True
            If YWheels1(I) <> AC(LI).libTX(I) Then result = True
        Next I

        Return result
    End Function

    Sub InitVar(ByRef Thick As Double, ByRef stdconfig As Boolean, ByRef DesignInput As modPCN_Nonstandard.InputData)

        DesignInput.Thickness = Thick
        DesignInput.XK = InputkValue
        DesignInput.Angle = 0
        DesignInput.TirePressure = TirePressure
        DesignInput.Delta = 0

        If stdconfig Then
            DesignInput.NOG = 2
            DesignInput.GearLoad = GrossWeight * PcntOnMainGears / 100 / NMainGears

            Select Case AC(LI).libGear
                Case "A", "B" 'Single Wheel (OK!)
                    DesignInput.XNA = 1.0#
                    DesignInput.XNB = 1.0#
                    DesignInput.XNC = 1.0#
                    DesignInput.XND = 1.0#
                    DesignInput.XLA = 0.0#
                    DesignInput.XLB = 0.0#
                    DesignInput.XLC = 0.0#
                    DesignInput.XLD = 0.0#
                Case "D" 'Twin (OK!)
                    DesignInput.XNA = 2.0#
                    DesignInput.XNB = 1.0#
                    DesignInput.XNC = 1.0#
                    DesignInput.XND = 1.0#
                    DesignInput.XLA = AC(LI).libTT 'Spacing between two tires in a dual
                    DesignInput.XLB = 0.0#
                    DesignInput.XLC = 0.0#
                    DesignInput.XLD = 0.0#
                Case "Y" 'Dual Twin
                    DesignInput.XNA = 2.0#
                    DesignInput.XNB = 2.0#
                    DesignInput.XNC = 1.0#
                    DesignInput.XND = 1.0#
                    'Transverse center to center distance between two inside tires
                    DesignInput.XLA = AC(LI).libTX(3) - AC(LI).libTX(2)
                    'Transverse center to center spacing between tires in a dual
                    DesignInput.XLB = AC(LI).libTX(4) - AC(LI).libTX(3)
                    DesignInput.XLC = 0.0#
                    DesignInput.XLD = 0.0#
                Case "E" 'Single Tandem
                    DesignInput.XNA = 1.0#
                    DesignInput.XNB = 1.0#
                    DesignInput.XNC = 2.0#
                    DesignInput.XND = 1.0#
                    DesignInput.XLA = 0.0#
                    DesignInput.XLB = 0.0#
                    ' Distance between tires
                    DesignInput.XLC = Math.Abs(AC(LI).libTY(2) - AC(LI).libTY(1))
                    DesignInput.XLD = 0.0#
                Case "F", "H", "J" 'Twin Tandem (OK!)
                    DesignInput.XNA = 2.0#
                    DesignInput.XNB = 1.0#
                    DesignInput.XNC = 2.0#
                    DesignInput.XND = 1.0#
                    DesignInput.XLA = AC(LI).libTT 'Transverse spacing between tires
                    DesignInput.XLB = 0.0#
                    DesignInput.XLC = AC(LI).libB 'Longitudinal spacing between tires
                    DesignInput.XLD = 0.0#
                Case "X" 'Dual Twin Tandem
                    DesignInput.XNA = 2.0#
                    DesignInput.XNB = 2.0#
                    DesignInput.XNC = 2.0#
                    DesignInput.XND = 1.0#
                    'Transverse spacing between inside tires in the gear
                    DesignInput.XLA = AC(LI).libTX(3) - AC(LI).libTX(2)
                    'Transverse spacing between tires in a dual set
                    DesignInput.XLB = AC(LI).libTX(4) - AC(LI).libTX(3)
                    'Longitudinal spacing between tires in a dual set
                    DesignInput.XLC = AC(LI).libTY(5) - AC(LI).libTY(1)
                    DesignInput.XLD = 0.0#
                Case "N" 'Triple Twin Tandem (OK!)
                    DesignInput.XNA = 2.0#
                    DesignInput.XNB = 1.0#
                    DesignInput.XNC = 3.0#
                    DesignInput.XND = 1.0#
                    DesignInput.XLA = AC(LI).libTT 'Tranvser spacing between tires
                    DesignInput.XLB = 0.0#
                    DesignInput.XLC = AC(LI).libB 'Longitudinal spacing between tires
                    DesignInput.XLD = 0.0#
                Case "OTHERS" 'Others
                    'ACNComp calculates slab edge stress for "others" gear configurations
                    'using the concept of superposition
            End Select

        Else
            DesignInput.NOG = 1
        End If

    End Sub

End Module
