Imports System.Windows.Forms
Module modPCN_MainWindow
    'Izydor 11/11/2014
    Dim LastX1, LastY1 As Single

    Dim NM1 As Integer
    Dim lstAircraftName As String = ""
    Public MaxGWFlexible As Boolean
    'Dim ACNBatchOutput
    Dim Dragging As Boolean
    Dim DragX, DragY As Single
    Dim PercentGross(NAC * 2) As Single

    Public Sub RigidACN(ByVal Ind As Integer)

        Dim I As Integer
        Dim cmdCaption, S As String
        Dim kValueSave As Double

        On Error GoTo RigidACNErr

        If Not ComputingRigid Then
            'PPPP TextBlockStatus.Text = "Computing Rigid"

            'PPPP cmdCaption = ButtonRigid.Content
            'PPPP ButtonRigid.Content = "Stop Rigid"

            'PPPP GroupBoxComputationalMode.IsEnabled = False
            'PPPP GroupBoxEditWheels.IsEnabled = False
            'fraLibraryFunctions.Enabled = False
            'PPPP ButtonExit.IsEnabled = False
            'PPPP ListBoxAircraftGroup.IsEnabled = False
            'PPPP ListBoxLibraryAircraft.IsEnabled = False
            'PPPP ButtonFlexible.IsEnabled = False

            For I = 1 To NSubgrades
                ACNRigidk(I) = 0
                RigidThickness(I) = 0
                ACNRigid(I) = 0
            Next I
            'PPPP WriteOutputGrid()
            ComputingRigid = True

            '    Izydor Kawa code Begin
            '    Call ACNRigComp old version for center stress with PCA progam.


            TirePressure = AC(LI).libCP 'PPPP

            If ACN_mode_true Then
                kValueSave = InputkValue
                InputkValue = 0.0#
                ACNRigComp(Ind)
                InputkValue = kValueSave
            Else
                Coverages = RigidCoverages

                SamePcntAndPress = True 'PPPP
                If Not SamePcntAndPress Then
                    TirePressure = GrossWeight * PcntOnMainGears / NWheels1 / NMainGears / TireContactArea / 100
                End If

                'PPPP If CheckBoxPCAThick.IsChecked Then ' GFH 05-08-08.
                If False Then
                    ACNRigComp(I)
                Else
                    ThicknessDesign() ' Edge stress using H-51 program.
                End If

            End If
            '   Izydor Kawa code End

        Else

            StopComputation = True
            Exit Sub

        End If

        'PPPP WriteParmGrid()
        'PPPP WriteOutputGrid()
        PlotGear()

        ComputingRigid = False

        '  If ACN_mode_true Then
        'PPPP ButtonRigid.IsEnabled = True
        'PPPP ButtonFlexible.IsEnabled = True
        '  End If

        'PPPP SetStatusMessage("Rigid Computation Finished")
        'PPPP ButtonRigid.Content = cmdCaption
        'PPPP GroupBoxEditWheels.IsEnabled = True
        '  fraLibraryFunctions.Enabled = True
        'PPPP ButtonExit.IsEnabled = True
        'PPPP ListBoxAircraftGroup.IsEnabled = True
        'PPPP ListBoxLibraryAircraft.IsEnabled = True
        'PPPP GroupBoxComputationalMode.IsEnabled = True 'ik004

        Exit Sub

RigidACNErr:

        S = "An unexpected error has occurred:" & NL2
        S = S & "Number =" & Conversion.Str(Information.Err().Number) & NL
        S = S & "Source = " & Information.Err().Source & NL
        S = S & "Description = " & Information.Err().Description
        I = MessageBox.Show(S, "Unexpected Error")
        FileSystem.FileClose()
        Resume Next

    End Sub

    Private Sub GetMGW(ByRef CallSource As String, ByRef MGW As Double,
                       ByRef ReturnCoverages As Double, ByRef CriticalACThick As Double,
                       ByVal Ind As Integer)

        Dim I, NAircraft As Integer
        '  Dim CriticalACIndex As Long ' Made a public variable so it can be set from a mouse down event.
        '  Dim EvalThick As Double ' Made a public variable so it can be set from a click event.
        Dim CriticalACCovsToFailure As Double
        Dim CriticalACEquivCovs() As Double, ConversionACCovs() As Double
        'Dim CriticalACTotalEquivCovs As Double
        'Dim ConversionACCovsToFailure() As Double 2015.09.16
        Dim Thickness, Covs, DelCovs, LastThick, DelThick, ThickM3, ThickM1, ThickM2, ThickM4, Func, FuncM1, GrossWeightM1, GrossWt, GrossWeightM2 As Double
        Dim FlexiblePCN As Boolean
        Dim LogCovs, LogCoverages, DelLogCovs As Double
        Dim SS, S, FullFileName As String
        Dim chkPCAThicknessDesignSave As Boolean
        Const Exp As Double = 2.7182818285
        Const MaxFlexLogCovs As Double = 18.42 ' = Log(100,000,000)

        'If Not (MinEvalThickness <= EvalThick And EvalThick <= MaxEvalThickness) Then
        '    MessageBox.Show("Please enter a valid evaluation thickness.", "Enter Evaluation Thickness")
        '    Exit Sub
        'End If
        '  lstLibFile.ListIndex = CriticalACIndex



        GrossWeight = GL(Ind)
        AnnualDepartures = RepsAnnual(Ind)
        'WheelRadius1 = 8.6947454228772738
        'PtoCFlex = 1 : PtoCRigid = 1
        'FlexibleCoverages = AnnualDepartures * Life / PtoCFlex
        'RigidCoverages = AnnualDepartures * Life / PtoCRigid

        'CriticalACIndex = ListBoxLibraryAircraft.SelectedIndex 'PPPP
        Dim GrossWeightSave As Double = GrossWeight
        Dim AnnualDeparturesSave As Double = AnnualDepartures
        Dim WheelRadiusSave As Double = WheelRadius1
        Dim FlexibleCoveragesSave As Double = FlexibleCoverages
        Dim RigidCoveragesSave As Double = RigidCoverages
        '  Coverages = CriticalACTotalEquivCovs
        Dim DelGrossWt As Double = GrossWeight / 100 ' Do not set to be very small or successive thickness calculations may be equal.
        Dim NewtonCoeff As Double = 1.0#
        Dim J As Integer = 0

        If MaxGWFlexible Then

            ACNFlexComp(Ind) '1
            CriticalACThick = CBRThickness(1) ' Used only for output.
            FuncM1 = CBRThickness(1) - EvalThick

            Do
                J += 1
                GrossWt = GrossWeight
                GrossWeight += DelGrossWt
                LastThick = CBRThickness(1)
                ACNFlexComp(Ind) '2
                If J >= 50 Then
                    SS = "The gross weight iteration cannot converge." & NL2
                    SS = SS & "The last two thickness values were:" & NL2
                    SS = SS & Format(LastThick * UnitsOut.inch, UnitsOut.inchFormat) & " and "
                    SS = SS & Format(CBRThickness(1) * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName
                    MessageBox.Show(SS, "Cannot Converge")
                    Exit Do
                End If
                DelThick = CBRThickness(1) - LastThick
                If DelThick = 0 Then
                    SS = "Two successive thickness evaluations gave the same thickness." & NL
                    SS = SS & "The calculation of allowable gross weight cannot converge." & NL
                    SS = SS & "The evaluation thickness may be too large."
                    MessageBox.Show(SS, "Thickness May Be Too Large")
                    Exit Do
                End If

                NewtonCoeff = 1
                Do
                    'If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * MinGLFraction) Then
                    If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * 0.1) Then
                        NewtonCoeff /= 2
                    Else
                        Exit Do
                    End If
                Loop
                GrossWeight = GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick
                '      If CallSource = "MGWOnly" Then
                '      If chkMGWcovs.Value = vbUnchecked Then
                '       Computes MGW based on annual departures because converts to coverages in CoverageToPass().
                '        WheelRadius = GrossWeight * PcntOnMainGears / (100 * NMainGears * NWheels)
                '        WheelRadius = Sqr(WheelRadius / TirePressure / PI)
                '        Call CoverageToPass ' Sets FlexibleCoverages and RigidCoverages, used in ACNFlexComp().
                '        ReturnCoverages = FlexibleCoverages
                '      ElseIf CallSource = "MGWforPCN" Then
                '      ElseIf chkMGWcovs.Value = vbChecked Then ' Do nothing.
                '       Computes MGW based on coverages not annual departures.
                ReturnCoverages = FlexibleCoverages
                '      End If
                ACNFlexComp(Ind) '3
                FuncM1 = CBRThickness(1) - EvalThick

                'Application.DoEvents()
                '      Debug.Print J; "GrossWeight = "; GrossWeight; " "; NewtonCoeff
                NewtonCoeff = 1.0#
            Loop Until Math.Abs(FuncM1) < 0.0001

        Else

            'PPPP
            'chkPCAThicknessDesignSave = CheckBoxPCAThick.IsChecked
            'If CheckBoxPCAMGW.IsChecked = True Then
            '    CheckBoxPCAThick.IsChecked = True
            'Else
            '    CheckBoxPCAThick.IsChecked = False
            'End If

            RigidACN(I)
            CriticalACThick = RigidThickness(1) ' Used only for output.
            FuncM1 = RigidThickness(1) - EvalThick

            Do
                J += 1
                GrossWt = GrossWeight
                GrossWeight += DelGrossWt
                LastThick = RigidThickness(1)
                RigidACN(I)
                If RigidThickness(1) = 0 Then Exit Do ' Occurs in ACNRigComp when very light load for PCA.
                DelThick = RigidThickness(1) - LastThick
                If J >= 50 Then
                    SS = "The gross weight iteration cannot converge." & NL2
                    SS = SS & "The last two thickness values were:" & NL2
                    SS = SS & Format(LastThick * UnitsOut.inch, UnitsOut.inchFormat) & " and "
                    SS = SS & Format(RigidThickness(1) * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName
                    MessageBox.Show(SS, "Cannot Converge")
                    Exit Do
                End If
                If DelThick = 0 Then
                    SS = "Two successive thickness evaluations gave the same thickness." & NL
                    SS = SS & "The calculation of allowable gross weight cannot converge." & NL
                    SS = SS & "The evaluation thickness may be too large."
                    MessageBox.Show(SS, "Thickness May Be Too Large")
                    Exit Do
                End If
                Do
                    'If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * MinGLFraction) Then
                    If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * 0.1) Then
                        NewtonCoeff /= 2
                    Else
                        Exit Do
                    End If
                Loop
                GrossWeight = GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick

                '      If CallSource = "MGWOnly" Then
                '      If chkMGWcovs.Value = vbUnchecked Then
                '       Computes MGW based on annual departures because converts to coverages in CoverageToPass().
                '        WheelRadius = GrossWeight * PcntOnMainGears / (100 * NMainGears * NWheels)
                '        WheelRadius = Sqr(WheelRadius / TirePressure / PI)
                '        Call CoverageToPass ' Sets FlexibleCoverages and RigidCoverages, used in RigidACN().
                '        ReturnCoverages = RigidCoverages
                '      ElseIf CallSource = "MGWforPCN" Then
                '      ElseIf chkMGWcovs.Value = vbChecked Then
                '       Computes MGW based on coverages not annual departures.
                '        ReturnCoverages = FlexibleCoverages ' Output error found by Jeff Rapol. Fixed by GFH 03/06/13.
                ReturnCoverages = RigidCoverages
                '      End If

                RigidACN(I)
                If RigidThickness(1) = 0 Then Exit Do
                '     The following is required for the H51 program gross weight determination. H51 appears to have fairly
                '     large granularity in this mode. Maybe gross weight is converted to an integer, or something similar.
                '     The linear interpolation should work ok with the PCA method for the last step.
                GrossWeightM2 = GrossWeightM1
                GrossWeightM1 = GrossWeight
                ThickM4 = ThickM3
                ThickM3 = ThickM2
                ThickM2 = ThickM1
                ThickM1 = RigidThickness(1)
                '      Debug.Print J; ThickM4; ThickM3; ThickM2; ThickM1; RigidThickness(1); GrossWeightM2; GrossWeightM1
                If (ThickM3 = ThickM1 And ThickM4 = ThickM2) Or (Math.Abs(EvalThick - ThickM1) < 0.1 And J > 1) Then
                    DelThick = ThickM1 - ThickM2
                    If Math.Abs(DelThick) > 1.0E-20 Then ' Let the J >= 20 test above terminate if necessary.
                        GrossWeight = ((GrossWeightM1 - GrossWeightM2) / DelThick) * (EvalThick - ThickM2) + GrossWeightM2
                        RigidThickness(1) = EvalThick
                        '          Debug.Print J; ThickM2; ThickM1; GrossWeightM2; GrossWeightM1; EvalThick; GrossWeight
                        Exit Do
                    End If
                End If
                FuncM1 = RigidThickness(1) - EvalThick
                '      Debug.Print J; "GrossWeight = "; GrossWeight; "Thickness = "; RigidThickness(1); "Coverages = "; Coverages; " "; NewtonCoeff
                NewtonCoeff = 1.0#
            Loop Until Math.Abs(FuncM1) < 0.0001
            System.Windows.Forms.Application.DoEvents()
            '    Debug.Print J; "GrossWeight = "; GrossWeight; "Thickness = "; RigidThickness(1); "Coverages = "; Coverages
            Debug.WriteLine("Pcnt = " & PcntOnMainGears & "Area = " & TireContactArea)
            'CheckBoxPCAThick.IsChecked = chkPCAThicknessDesignSave 'PPPP

        End If

        'PPPP
        'ButtonFlexible.IsEnabled = True
        'ButtonRigid.IsEnabled = True

        'PPPP
        'SetStatusMessage("MGW = " & Format(GrossWeight * UnitsOut.pounds, UnitsOut.poundsFormat) & " " & UnitsOut.poundsName)
        '  S = S & "    Critical Airplane Allowable Gross Weight = "
        '  S = S & Format(GrossWeight * UnitsOut.pounds, UnitsOut.poundsFormat) & " " & UnitsOut.poundsName & vbCrLf & vbCrLf

        '  txtMGW(lstLibFile.ListIndex) = GrossWeight
        MGW = GrossWeight

        '  libGL(libIndex) = GrossWeightSave
        '  libCoverages(libIndex) = CoveragesSave
        GrossWeight = GrossWeightSave
        WheelRadius1 = WheelRadiusSave
        FlexibleCoverages = FlexibleCoveragesSave
        RigidCoverages = RigidCoveragesSave
        AnnualDepartures = AnnualDeparturesSave
        'WriteParmGrid() 'PPPP

    End Sub

    Public Sub FlexibleandRigidMGW(ByRef PavementType As String)

        Dim LoopStart, LoopEnd As Integer
        Dim MGW, Thick1, MGWCoverages As Double
        Dim SS As String = ""
        Dim S As String

        'PPPP Dim NAircraft As Integer = ListBoxLibraryAircraft.Items.Count
        'PPPP Dim lstLibFileListIndexSave As Integer = ListBoxLibraryAircraft.SelectedIndex
        Dim NAircraft As Integer
        Dim lstLibFileListIndexSave As Integer
        NAircraft = NAC



        If PavementType = "Flexible" Then

            EvalThick = 0
            If DesignType = NewFlex Or DesignType = FlexOnFlex Then 'FlexibleandRigidMGW
                For I As Integer = 1 To NPLayers - 1
                    EvalThick = EvalThick + Thick(I)
                Next
            Else
                EvalThick = Thick(1)
            End If

        ElseIf PavementType = "Rigid" Then

            EvalThick = Thick(1)

        End If
        'EvalThick = EvalThick + 0.01





        'PPPP If CheckBoxBatch.IsChecked Then ' All aircraft in the list.
        LoopStart = 1 '0
        LoopEnd = NAircraft      'NAircraft - 1
        'PPPP Else
        ' The currently selected aircraft.
        'LoopStart = lstLibFileListIndexSave
        'LoopEnd = lstLibFileListIndexSave
        'PPPP End If

        If PavementType = "Flexible" Then
            MaxGWFlexible = True ' Needed by Sub GetMGW().

            '   S = "Library file name = " & ExtFilePath & ExternalAircraftFileName & ".Ext" & NL2
            S = S & "Evaluation pavement type is flexible and design procedure is CBR" & NL

            'PPPP If CheckBoxNew07Alphas.IsChecked Then
            SS = "Alpha Values are those approved by the ICAO in 2007." & NL
            'PPPP Else
            SS = "Alpha Values are those used in AC 150/5320-6C/D design charts." & NL
            'PPPP End If

            S = S & SS & NL
            S = S & "                                         CBR = " & Format(InputCBR, "0.00") & NL
            S = S & "               Evaluation pavement thickness = "
            S = S & Format(EvalThick * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName & NL2
            SS = CStr(IIf(UnitsOut.Metric, "Metric.", "Imperial."))
            S = S & "Results Table: Maximum Allowable Gross Weight Computations, Units = " & SS & NL

            S = S & "                             Gross   Percent    Tire     Annual    6D      20-yr      Max. Allowable" & NL
            S = S & " No.  Aircraft Name          Weight  Gross Wt   Press     Deps    Thick  Coverages     Gross Weight" & NL
            S = S & StrDup(106, "-") & NL

            SS = S.ToString()

            S = ""
            For I As Integer = LoopStart To LoopEnd
                'PPPP ListBoxLibraryAircraft.SelectedIndex = I

                If modPCN_ZZZ.FF_true_ACN Then
                    modPCN_ZZZ.GetMGW_FF("MGWOnly", MGW, MGWCoverages, Thick1, I)
                Else
                    GetMGW("MGWOnly", MGW, MGWCoverages, Thick1, I)
                End If

                modPCN_ZZZ.gMGW(modPCN_ZZZ.gkk) = CSng(MGW)


                'PPPP S = S & LPad(3, Format(I + 1, "0")) & "  " & RPad(21, ListBoxLibraryAircraft.SelectedItem.ToString())

                'MsgBox("MGW=" & MGW, MsgBoxStyle.OkOnly, "+++")

                S = S & LPad(3, Format(I + 1, "0")) & "  "

                S = S & LPad(9, String.Format("{0:N0}", GrossWeight * UnitsOut.pounds))
                S = S & LPad(8, String.Format("{0:N2}", PcntOnMainGears, "0.00"))
                S = S & LPad(10, String.Format("{0:N1}", TirePressure * UnitsOut.psi, UnitsOut.psiFormat))
                S = S & LPad(10, String.Format("{0:N0}", AnnualDepartures, "#,###,##0"))
                S = S & LPad(8, String.Format("{0:N2}", Thick1 * UnitsOut.inch, UnitsOut.inchFormat))
                S = S & LPad(10, String.Format("{0:N0}", MGWCoverages, "#,###,##0"))
                S = S & LPad(15, String.Format("{0:N0}", MGW * UnitsOut.pounds, UnitsOut.poundsFormat)) & NL
            Next I

        Else
            MaxGWFlexible = False ' Needed by Sub GetMGW().

            ' S = "Library file name = " & ExtFilePath & ExternalAircraftFileName & ".Ext" & NL2
            S = S & "Evaluation pavement type is rigid" & NL
            S = S & SS & NL

            'PPPP If CheckBoxPCAMGW.IsChecked Then
            SS = "Maximum gross weight computed with the PCA interior stress design method."
            'PPPP Else
            SS = "Maximum gross weight computed with the AC 150/5320-6C/D edge stress design method."
            'PPPP End If

            InputkValue = 840 'PPPP
            ConcreteFlexuralStrength = RCon(1) '650 'PPPP


            S = S & SS & NL2
            S = S & "                                     k Value = "
            S = S & Format(InputkValue * UnitsOut.pci, UnitsOut.pciFormat) & " " & UnitsOut.pciName & NL
            S = S & "                           flexural strength = "
            S = S & Format(ConcreteFlexuralStrength * UnitsOut.psi, UnitsOut.psiFormat) & " " & UnitsOut.psiName & NL
            S = S & "               Evaluation pavement thickness = "
            S = S & Format(EvalThick * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName & NL2

            SS = CStr(IIf(UnitsOut.Metric, "Metric.", "Imperial."))
            S = S & "Results Table: Maximum Allowable Gross Weight Computations, Units = " & SS & NL

            S = S & "                             Gross   Percent    Tire     Annual    6D      20-yr      Max. Allowable" & NL
            S = S & " No.  Aircraft Name          Weight  Gross Wt   Press     Deps    Thick  Coverages     Gross Weight" & NL
            S = S & StrDup(106, "-") & NL
            SS = S.ToString()

            'S = ""

            '      For I = 0 To lstLibFile.ListCount - 1
            For I As Integer = LoopStart To LoopEnd
                'PPPP ListBoxLibraryAircraft.SelectedIndex = I

                If modPCN_ZZZ.FF_true_ACN Then
                    modPCN_ZZZ.GetMGW_FF("MGWOnly", MGW, MGWCoverages, Thick1, I)
                Else
                    GetMGW("MGWOnly", MGW, MGWCoverages, Thick1, I)
                End If

                modPCN_ZZZ.gMGW(modPCN_ZZZ.gkk) = CSng(MGW)

                'PPPP S = S & LPad(3, Format(I + 1, "0")) & "  " & RPad(21, ListBoxLibraryAircraft.SelectedItem.ToString())
                S = S & LPad(3, Format(I + 1, "0")) & "  "

                'MsgBox("MGW=" & MGW, MsgBoxStyle.OkOnly, "+++")

                S = S & LPad(9, String.Format("{0:N0}", GrossWeight * UnitsOut.pounds, UnitsOut.poundsFormat))
                S = S & LPad(8, String.Format("{0:N2}", PcntOnMainGears, "0.00"))
                S = S & LPad(10, String.Format("{0:N1}", TirePressure * UnitsOut.psi, UnitsOut.psiFormat))
                S = S & LPad(10, String.Format("{0:N0}", AnnualDepartures, "#,###,##0"))
                S = S & LPad(8, String.Format("{0:N2}", Thick1 * UnitsOut.inch, UnitsOut.inchFormat))
                S = S & LPad(10, String.Format("{0:N0}", MGWCoverages, "#,###,##0"))
                S = S & LPad(15, String.Format("{0:N0}", MGW * UnitsOut.pounds, UnitsOut.poundsFormat)) & NL
            Next I

        End If ' Pavement Type.

        'PPPP ListBoxLibraryAircraft.SelectedIndex = lstLibFileListIndexSave
        MGWOutputText = SS & S.ToString()
        'RadioIntStress.IsChecked = True

    End Sub



    Public Sub FlexibleACNforBatch(ByRef Caller As String)

    End Sub

    Public Sub RigidACNforBatch(ByRef Caller As String)

    End Sub

    Public Function getTodaysDateFormatted() As String
        Return DateTime.Now.ToString("yyyy-MM-dd HH;mm;ss")
    End Function

    Public Sub PlotGear()

        Dim I As Integer, II As Integer, N As Integer, Ilst As Integer
        Dim MaxWidthBy2 As Single, picAspect As Single, L As Long
        Dim xtl As Single, ytl As Single, xbr As Single, ybr As Single
        Dim XMin As Double, XMax As Double, Ymin As Double, YMax As Double
        Dim MaxHeightBy2 As Double, Rad As Double
        Dim XCenter As Double, YCenter As Double
        Dim Xcg As Double, Ycg As Double
        Dim Delta As Double, Add As Double
        Dim SaveFontSize As Single
        Const PI As Double = Math.PI

        ' Gear axes are X +ve forward, Y +ve to the right.
        ' Gear axes on screen are X +ve upward, Y +ve to the right.
        ' Screen axes are X +ve to the right, Y +ve upward.
        ' Therefore, screen coordinates are written (y, x) in gear axes.
        ' Remember when drawing on screen and returning screen coordinates from mouse.

        ' Global units are English units.



        Call GearCG(XWheels1, YWheels1, NWheels1, Xcg, Ycg, 1)

        'XMin = Double.MaxValue : XMax = Double.MinValue
        'Ymin = Double.MaxValue : YMax = Double.MinValue
        'For I = 1 To NWheels
        '    If XWheels(I) < XMin Then XMin = XWheels(I)
        '    If XWheels(I) > XMax Then XMax = XWheels(I)
        '    If YWheels(I) < Ymin Then Ymin = YWheels(I)
        '    If YWheels(I) > YMax Then YMax = YWheels(I)
        'Next I
        'XMin = XMin - WheelRadius1 : XMax = XMax + WheelRadius1
        'Ymin = Ymin - WheelRadius1 : YMax = YMax + WheelRadius1


    End Sub

    Public Sub ListBoxLibraryAircraft_SelectedIndex(ByVal I As Integer)


        PtoCFlex = 1 : PtoCRigid = 1
        AnnualDepartures = RepsAnnual(I)
        FlexibleCoverages = AnnualDepartures * Life / PtoCFlex
        RigidCoverages = AnnualDepartures * Life / PtoCRigid


        GrossWeight = GL(I) : TirePressure = AC(LibIndex(I)).libCP

        If AC(LibIndex(I)).libGear = "A" Then
            NMainGears = 1 : PcntOnMainGears = 100
        ElseIf AC(LI).libGear = "WFBF" Then
            NMainGears = 1
            PcntOnMainGears = AC(LibIndex(I)).libMGpcnt * 100
        Else
            NMainGears = 2
            PcntOnMainGears = AC(LibIndex(I)).libMGpcnt * 100
        End If

        PercentGross(I) = CSng(PcntOnMainGears)

        Dim I5 As Integer, Xcg As Double, Ycg As Double
        NWheels1 = AC(LibIndex(I)).libNTires
        For I5 = 1 To NWheels1
            XWheels1(I5) = AC(LibIndex(I)).libTY(I5)
            YWheels1(I5) = AC(LibIndex(I)).libTX(I5)
        Next
        Call GearCG(XWheels1, YWheels1, NWheels1, Xcg, Ycg, I)



        'If ACN_mode_true Then
        '    PcntOnMainGears = libPcntOnMainGears(LibIndex, ACN_mode)
        '    Coverages = StandardCoverages
        'Else
        '    PcntOnMainGears = libPcntOnMainGears(LibIndex, Thick_mode)
        '    '    Coverages = libCoverages(libIndex)
        '    AnnualDepartures = libAnnualDepartures(LibIndex)
        '    If SamePcntAndPress Then
        '        PcntOnMainGears = libPcntOnMainGears(LibIndex, ACN_mode)
        '    End If
        'End If

        'NMainGears = libNMainGears(LibIndex)
        'InputAlpha = libAlpha(LibIndex)
        'AlphaFactor = InputAlpha
        'XGridOrigin = libXGridOrigin(LibIndex)
        'XGridMax = libXGridMax(LibIndex)
        'XGridNPoints = libXGridNPoints(LibIndex)
        'YGridOrigin = libYGridOrigin(LibIndex)
        'YGridMax = libYGridMax(LibIndex)
        'YGridNPoints = libYGridNPoints(LibIndex)

        'Public Sub ResetOutputs()


        For I1 As Integer = 1 To NSubgrades
            ACNFlexCBR(I1) = 0
            CBRThickness(I1) = 0
            ACNFlex(I1) = 0
            ACNRigidk(I1) = 0
            RigidThickness(I1) = 0
            ACNRigid(I1) = 0
        Next I1


        'Dim btn = CType(My.Application.FindName(Of MainWindow)("BtnSelect"), ToggleButton)
        'btn.IsChecked = False
        'btn.Content = "Se_lect"
        'CType(My.Application.FindName(Of MainWindow)("LabelXSelCoord"), Controls.Label).Content = ""
        'CType(My.Application.FindName(Of MainWindow)("LabelYSelCoord"), Controls.Label).Content = ""

        'Operation = NoOperation
        'LastOperation = NoOperation

        'Call ResetOutputs()
        'Call PlotGear() ' Also calls GearCG and updates the parameter output grid.

        'Call WriteOutputGrid()
        'Call CoverageToPass()




    End Sub

End Module
