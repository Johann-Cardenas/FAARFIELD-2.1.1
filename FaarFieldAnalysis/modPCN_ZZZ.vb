

Imports System.Windows.Forms

Module modPCN_ZZZ
    Public gCaseCDFlow As Boolean

    Public Const gTolerance As Single = 0.005
    Public indexPCNtable2 As Integer

    Public gIterLevel1A As Integer
    Public gIterLevel1B As Integer
    Public gIterLevel2 As Integer


    Public TFN As String
    Public gNewACName(2) As String
    Public gNewAnnualDepart(2) As Single
    Public gNewGL(2) As Single

    Public gNewPCN(2) As Single
    Public gNewPCNthick(2) As Single
    Public gNewPCNtoReport As Single

    Public H_index As Integer
    Public cdf_max1M(1) As Single
    Public ind2maxACxM(1) As Integer
    Public ind2maxACxMSave(1) As Integer

    'Public gGearInd(MaxSectAC) As Short

    Public gCDF_target As Single 'ACN/PCN Method
    Public gCDF_target_copy As Single
    Public gCDF_reached As Single
    Public gPCN_report As Single
    Public gPCN_report_index As Integer

    Public gOverlayLife_reached As Single

    Public gOverlayLife_target As Single
    Public gOverlayLife_target_copy As Single


    Public gCriticalACEquivCovs(NAC, NAC) As Double
    Public gTARGET As Double
    Public gPcnt1(MaxSectAC) As Single

    Public ICAOCodeIndex As Integer
    Public ICAOCode(3) As String

    Public gETimemsecs As Single

    Public gCoverage_NtoFail(MaxSectAC) As Double
    Public gFlex_NtoFail_Copy(MaxSectAC) As Double

    Public gMGW(MaxSectAC) As Single
    Public gLifeStr(MaxSectAC) As Single

    'Public gACN_Thick(MaxSectAC) As Single

    Public gACN_GL(MaxSectAC) As Single
    Public gACN_GL_thick(MaxSectAC) As Single
    Public gkk As Integer
    Public lPavementType As ACRClassLib.clsACR.PavementType

    Public FF_true_ACN As Boolean = False
    Public ConversionACCovs(MaxSectAC) As Double

    Public SaveM_NAC(1) As Short
    Public SaveM_LibIndex(MaxSectAC, 1) As Short
    Public SaveM_ACName(MaxSectAC, 1) As String
    Public SaveM_GL(MaxSectAC, 1) As Single
    Public SaveM_RepsAnnual(MaxSectAC, 1) As Single
    Public SaveM_RepsInc(MaxSectAC, 1) As Single
    Public SaveM_WT(MaxSectAC, 1) As Single
    Public SaveM_TW(MaxSectAC, 1) As Single
    Public SaveM_MGpcnt(MaxSectAC, 1) As Single
    Public SaveM_NAC_order(MaxSectAC, 1) As Integer




    Public Save_NAC As Short
    Public Save_LibIndex(MaxSectAC) As Short
    Public Save_ACName(MaxSectAC) As String
    Public Save_GL(MaxSectAC) As Single
    Public Save_RepsAnnual(MaxSectAC) As Single
    Public Save_RepsInc(MaxSectAC) As Single
    Public Save_WT(MaxSectAC) As Single
    Public Save_TW(MaxSectAC) As Single
    Public Save_MGpcnt(MaxSectAC) As Single
    Public Save_NAC_order(MaxSectAC) As Integer


    Public NAC_order(MaxSectAC) As Integer

    Public Save2_NAC As Short
    Public Save2_LibIndex(MaxSectAC) As Short
    Public Save2_ACName(MaxSectAC) As String
    Public Save2_GL(MaxSectAC) As Single
    Public Save2_RepsAnnual(MaxSectAC) As Single
    Public Save2_RepsInc(MaxSectAC) As Single
    Public Save2_WT(MaxSectAC) As Single
    Public Save2_TW(MaxSectAC) As Single
    Public Save2_MGpcnt(MaxSectAC) As Single
    Public Save2_NAC_order(MaxSectAC) As Integer



    Public Save3_NAC As Short
    Public Save3_LibIndex(MaxSectAC) As Short
    Public Save3_ACName(MaxSectAC) As String
    Public Save3_GL(MaxSectAC) As Single
    Public Save3_RepsAnnual(MaxSectAC) As Single
    Public Save3_RepsInc(MaxSectAC) As Single
    Public Save3_WT(MaxSectAC) As Single
    Public Save3_TW(MaxSectAC) As Single
    Public Save3_MGpcnt(MaxSectAC) As Single
    Public Save3_NAC_order(MaxSectAC) As Integer




    Dim PercentGross(NAC * 2) As Single


    Public Function ICAOCodeIndexF(ByVal modul1 As Single) As Integer
        'FAARFIELD

        'paragraph 1.1.3.2 a) Subgrade category
        Dim divis1, divis2, divis3 As Single
        'divis1 = 25381.6   '175 MPa
        divis1 = 21755.66 '150 MPa 
        divis2 = 14503.7738  '100 Mpa
        divis3 = 8702.264    '60 Mpa

        'divis1 = 25381.6   '175 MPa
        'divis2 = 14503.77  '100 Mpa
        'divis3 = 8702.26    '60 Mpa

        'If InputCBR >= 13 Then ICAOCodeIndex = 4
        If modul1 >= divis1 Then ICAOCodeIndexF = 4 '                    Category A
        If divis2 < modul1 And modul1 < divis1 Then ICAOCodeIndexF = 3 ' Category B
        If divis3 < modul1 And modul1 <= divis2 Then ICAOCodeIndexF = 2 'Category C
        If modul1 <= divis3 Then ICAOCodeIndexF = 1 '                    Category D

    End Function



    Public Sub SetPCN_for_AC()

        Dim gNAC As Integer
        gNAC = UBound(AC, 1)

        For i As Integer = 1 To gNAC

            If AC(i).libACName = "A380 Belly" Then
                'AC(i).libMGpcntPCN = 57.08 / 100 / 2
                AC(i).libMGpcnt = 57.08 / 100 / 2

            ElseIf AC(i).libACName = "A380e Belly" Then
                AC(i).libMGpcnt = 56.59 / 100 / 2

            ElseIf AC(i).libACName = "B747-100 SF Belly" Then
                AC(i).libMGpcnt = 92.48 / 100 / 4

            ElseIf AC(i).libACName = "B747-200B Combi Mixed Belly" Then
                AC(i).libMGpcnt = 90.96 / 100 / 4

            ElseIf AC(i).libACName = "B747-300 Combi Mixed Belly" Then
                AC(i).libMGpcnt = 90.96 / 100 / 4

            ElseIf AC(i).libACName = "B747-400 Belly" Then
                AC(i).libMGpcnt = 93.32 / 100 / 4

            ElseIf AC(i).libACName = "B747-400ER Belly" Then
                AC(i).libMGpcnt = 93.6 / 100 / 4

            ElseIf AC(i).libACName = "B747-8 Belly" Then
                AC(i).libMGpcnt = 94.7 / 100 / 4

            ElseIf AC(i).libACName = "B747-8F Belly" Then
                AC(i).libMGpcnt = 94.4 / 100 / 4

            ElseIf AC(i).libACName = "B747-SP Belly" Then
                AC(i).libMGpcnt = 87.68 / 100 / 4
            End If

        Next

    End Sub





    Public Sub Print_PCN_results(ByVal lsPavementType As String, ByVal ConversionACCovs() As Double,
                                 ByVal ConversionACCovsToFailure() As Double,
                                 ByVal CriticalACTotalEquivCovs() As Double)

        Dim ThickCriticalAC(NAC) As Double
        Dim ACNThicknessRtn(,) As Double
        Dim S1, SS1 As String

        Dim DTemp As Double ' For printing to see if EvalThick reached.
        Dim UnitsOutName As String
        If EnglishUnits Then
            UnitsOutName = "English"
        Else
            UnitsOutName = "Metric"
        End If

        Dim MaxNWheels As Integer = 0 : Dim MaxNMainGears As Integer = 0

        For I As Integer = 1 To NAC
            If AC(LibIndex(I)).libNTires > MaxNWheels Then
                MaxNWheels = AC(LibIndex(I)).libNTires
            End If
            'If AC(LibIndex(I)).libNTires > MaxNWheels Then
            '    NMainGears = AC(LibIndex(I)).lib
            'End If
        Next

        Dim FF As String

        Dim FullOutput As Boolean = True

        If EnglishUnits Then
            ICAOCode(0) = "D(7k)"
            ICAOCode(1) = "C(11k)"
            ICAOCode(2) = "B(17k)"
            ICAOCode(3) = "A(29k)"
        Else
            ICAOCode(0) = "D(50 MPa)"
            ICAOCode(1) = "C(80 Mpa)"
            ICAOCode(2) = "B(120 MPa)"
            ICAOCode(3) = "A(200 Mpa)"
        End If

        Dim InputGrossWeight(NAC) As Double, InputAnnualDepartures(NAC) As Double
        Dim ThickFlexible(NAC) As Double, ThickRigid(NAC) As Double
        Dim ThickLife(NAC) As Double




        FF = getTodaysDateFormatted()

        If lsPavementType = "Flexible" Then
            FileName = "PCN Results Flexible " & FF & ".txt"
        ElseIf lsPavementType = "Rigid" Then
            FileName = "PCN Results Rigid " & FF & ".txt"
        End If


        S1 = "This file name = " & FileName & NL
        'S = S & "Library file name = " & ExtFilePath & ExternalAircraftFileName & ".Ext" & NL
        'S1 = S1 & "Section name: " & SectName & " in job file: " & WorkingDir & "\" & JobName & ".JOB.xml" & NL
        S1 = S1 & "Section name: " & SectName & " in job file: " & NL & WorkingDir & JobName & ".JOB.xml" & NL

        S1 = S1 & "Units = " & UnitsOutName & NL2


        If lsPavementType = "Flexible" Then
            S1 = S1 & "Evaluation pavement type is flexible and design program is FAARFIELD." & NL
        ElseIf lsPavementType = "Rigid" Then
            S1 = S1 & "Evaluation pavement type is rigid and design program is FAARFIELD." & NL
        End If

        SS1 = ""
        S1 = S1 & SS1.ToString() & NL

        Dim modul1 As Single
        modul1 = Modulus(NPLayers)
        ICAOCodeIndex = ICAOCodeIndexF(modul1)

        SS1 = CStr((ICAOCode(ICAOCodeIndex - 1)))

        S1 = S1 & StrDup(26, " ") & "Subgrade Modulus = " & Format(Modulus(NPLayers), "#,##0") & " psi" & " (Subgrade Category is " & SS1.ToString() & ")" & NL

        If lsPavementType = "Flexible" Then
        ElseIf lsPavementType = "Rigid" Then
            S1 = S1 & StrDup(25, " ") & "flexural strength = "
            S1 = S1 & Format(ConcreteFlexuralStrength * UnitsOut.psi, UnitsOut.psiFormat) & " " & UnitsOut.psiName & NL1
        End If


        S1 = S1 & "             Evaluation pavement thickness = "
        S1 = S1 & Format(EvalThick * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName & NL
        S1 = S1 & "       Pass to Traffic Cycle (PtoTC) Ratio = " & Format(PtoTC, "0.00")
        If PtoTC < 1.0# Or 3.0# < PtoTC Or (PtoTC - Math.Floor(PtoTC)) <> 0 Then S = S & " (non-standard)"
        S1 = S1 & NL

        S1 = S1 & "         Maximum number of wheels per gear = " & Format(MaxNWheels, "0") & NL

        S1 = S1 & NL
        'S1 = S1 & "        Maximum number of gears per aircraft = " & Format(MaxNMainGears, "0") & NL2

        If lsPavementType = "Flexible" Then

            If MaxNWheels >= 4 Then
                S1 = S1 & "At least one aircraft has 4 or more wheels per gear." & NL2  'The FAA recommends a reference section assuming" & NL
                'If UnitsOut.Metric Then
                '    S1 = S1 & "127 mm of HMA and 203 mm of crushed aggregate for equivalent thickness calculations." & NL2
                'Else
                '    S1 = S1 & "5 inches of HMA and 8 inches of crushed aggregate for equivalent thickness calculations." & NL2
                'End If
            Else
                S1 = S1 & "No aircraft have 4 or more wheels per gear." & NL2 'The FAA recommends a reference section assuming" & NL
                'If UnitsOut.Metric Then
                '    S1 = S1 & "76 mm of HMA and 152 mm of crushed aggregate for equivalent thickness calculations." & NL2
                'Else
                '    S1 = S1 & "3 inches of HMA and 6 inches of crushed aggregate for equivalent thickness calculations." & NL2
                'End If
            End If


        End If

        'ACName(1) = "1234567890-1234567890-123"

        S1 = S1 & "Results Table 1. Input Traffic Data" & NL


        S1 = S1 & "                             Gross   Percent    Tire     Annual      20-yr" & NL
        S1 = S1 & " No.  Aircraft Name          Weight  Gross Wt   Press     Deps     Coverages" & NL
        S1 = S1 & StrDup(76, "-") & NL

        For I As Integer = 1 To NAC
            S1 = S1 & LPad(3, Format(I, "0")) & "  " & RPad2(20, ACName(I))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "
            'S = S & LPad(7, String.Format("{0:N2}", PercentGross(I)))
            'S = S & LPad(7, String.Format("{0:N2}", 2 * AC(LibIndex(I)).libMGpcnt * 100))

            If AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            S1 = S1 & LPad(10, String.Format("{0:N1}", AC(LibIndex(I)).libCP * UnitsOut.psi, UnitsOut.psiFormat))
            S1 = S1 & LPad(10, String.Format("{0:N0}", RepsAnnual(I), "#,###,##0")) ' GFH 9/28/09.

            'If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            '    S1 = S1 & LPad(12, String.Format("{0:N0}", RepsAnnual(I) * Life / gPtoC(I), "#,###,##0"))
            'Else
            '    S1 = S1 & LPad(12, String.Format("{0:N0}", RepsAnnual(I) * Life / gPtoC_Rigid(I), "#,###,##0"))
            'End If

            'S1 = S1 & LPad(12, String.Format("{0:N0}", RepsAnnual(I) * Life / gPtoC(I), "#,###,##0"))
            S1 = S1 & LPad(12, String.Format("{0:N0}", ConversionACCovs(I), "#,###,##0"))


            'S1 = S1 & LPad(8, String.Format("{0:N2}", ThickFlexible(I) * UnitsOut.inch)) & NL '6D Thickness
            S1 = S1 & NL
        Next I

        '============================================================================================
        S1 = S1 & NL
        'ListBoxLibraryAircraft.SelectedIndex = lstLibFileListIndexSave

        'S = ""

        S1 = S1 & "Results Table 2. PCN Values" & NL

        FullOutput = False '!!!!!
        If FullOutput Then
            S1 = S1 & "                          Critical        Thickness      Maximum    " & NL
            S1 = S1 & "                       Aircraft Total     for Total     Allowable       PCN at Indicated Code" & NL
            If lsPavementType = "Flexible" Then
                S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Equiv. Covs.  Gross Weight    A(15)  B(10)   C(6)   D(3)    CDF" & NL
            Else
                S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Equiv. Covs.  Gross Weight   " & ICAOCode(3) & ICAOCode(2) & ICAOCode(1) & ICAOCode(0) & "    CDF" & NL
            End If
            S1 = S1 & StrDup(99, "-") & NL
        Else
            'S1 = S1 & "                          Critical        Thickness      Maximum      ACN Thick at" & NL
            'S1 = S1 & "                          Critical        Thickness      Maximum        Life for" & NL
            'S1 = S1 & "                       Aircraft Total     for Total     Allowable    Max. Allowable           PCN on" & NL
            'S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Equiv. Covs.  Gross Weight   Gross Weight    CDF      " & ICAOCode(ICAOCodeIndex - 1) & NL
            'S1 = S1 & StrDup(99, "-") & NL

            'S1 = S1 & "                          Critical          Maximum        Life for" & NL
            'S1 = S1 & "                       Aircraft Total      Allowable         MGW                PCN on" & NL
            'S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Gross Weight                   CDF      " & ICAOCode(ICAOCodeIndex - 1) & NL

            S1 = S1 & "                          Critical          Maximum      ACN Thick" & NL
            S1 = S1 & "                       Aircraft Total      Allowable      at max.              PCN on" & NL
            S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Gross Weight       MGW        CDF     " & ICAOCode(ICAOCodeIndex - 1) & NL
            S1 = S1 & StrDup(86, "-") & NL
        End If


        ReDim ACNrtn(NAC, 4)
        ReDim ACNThicknessRtn(NAC, 4)


        Dim cdf1, cdf_max As Double, cdf_string As String, cdf_length As Integer
        Dim Spaces1, Spaces2 As Integer




        For I As Integer = 1 To NAC
            cdf1 = ConversionACCovs(I) / ConversionACCovsToFailure(I)
            If cdf_max < cdf1 Then cdf_max = cdf1
        Next

        cdf_string = CStr(Format(Math.Round(cdf_max, 4), "#,##0.0000"))
        cdf_length = cdf_string.Length

        If cdf_length > 11 Then
            Spaces1 = cdf_length
            Spaces2 = 9 - (cdf_length - 11)
            If Spaces2 < 6 Then Spaces2 = 6
        Else
            Spaces1 = 11
            Spaces2 = 9
        End If


        Dim seq1 As Integer = 0

        Dim CDFTotal As Double = 0.0#
        For I As Integer = 1 To NAC

            If Not (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
                seq1 = seq1 + 1
                'S1 = S1 & LPad(3, Format(I, "0")) & "  " & RPad2(20, ACName(I))
                S1 = S1 & LPad(3, Format(seq1, "0")) & "  " & RPad2(20, ACName(I))
            End If

            If CriticalACTotalEquivCovs(I) = 0.0 Then
                Continue For
            End If

            If CriticalACTotalEquivCovs(I) <= 5000000 Then ' About one departure per minute for 20 years.
                S1 = S1 & LPad(11, String.Format("{0:N0}", CriticalACTotalEquivCovs(I)))
            Else
                S1 = S1 & LPad(11, ">5,000,000")
            End If
            'S1 = S1 & LPad(13, String.Format("{0:N2}", ThickCriticalAC(I) * UnitsOut.inch)) 'commented


            'S = S & LPad(16, String.Format("{0:N0}", MaxGrossWeight(I) * UnitsOut.pounds))
            S1 = S1 & LPad(16, String.Format("{0:N0}", gMGW(I) * UnitsOut.pounds))
            If FullOutput Then
                S1 = S1 & LPad(10, String.Format("{0:N2}", ACNrtn(I, 4))) & LPad(7, String.Format("{0:N2}", ACNrtn(I, 3)))
                S1 = S1 & LPad(7, String.Format("{0:N2}", ACNrtn(I, 2))) & LPad(7, String.Format("{0:N2}", ACNrtn(I, 1)))
            Else
                'S = S & LPad(14, String.Format("{0:N2}", ACNThicknessRtn(I, ICAOCodeIndex) * UnitsOut.inch))
                'S1 = S1 & LPad(14, String.Format("{0:N3}", gLifeStr(I)))
                S1 = S1 & LPad(12, String.Format("{0:N2}", gACN_GL_thick(I)))

            End If


            DTemp = ConversionACCovs(I) / ConversionACCovsToFailure(I)
            CDFTotal += DTemp
            'S1 = S1 & LPad(11, String.Format("{0:N4}", DTemp))
            S1 = S1 & LPad(Spaces1, String.Format("{0:N4}", DTemp))

            'S1 = S1 & LPad(9, String.Format("{0:N1}", ACNrtn(I, ICAOCodeIndex))) & NL
            'S1 = S1 & LPad(9, String.Format("{0:N1}", gACR(I))) & NL
            S1 = S1 & LPad(Spaces2, String.Format("{0:N1}", gACN_GL(I))) & NL
        Next I

        'J = IIf(FullOutput, 85, 70)
        J = 70

        'S1 = S1 & New String(" "c, J) & "Total CDF =" & LPad(9, String.Format("{0:N4}", CDFTotal)) & NL
        S1 = S1 & New String(" "c, 53) & "Total CDF =" & LPad(11, String.Format("{0:N4}", CDFTotal)) & NL2

        Dim SS7, sT1 As String
        SS7 = ICAOCode(ICAOCodeIndex - 1)
        sT1 = PaveTypeF(lPavementType)

        S1 = S1 & "Results Table 3. " & sT1 & " ACN at Indicated Gross Weight and Strength" & NL1
        S1 = S1 & " No. Aircraft Name          Gross    % GW on     Tire       ACN      ACN on" & NL1
        S1 = S1 & "                            Weight  Main Gear  Pressure    Thick      " & SS7 & NL1
        S1 = S1 & "--------------------------------------------------------------------------" & NL1

        seq1 = 0
        For I As Integer = 1 To NAC

            If Not (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
                seq1 = seq1 + 1
                'S1 = S1 & LPad(3, Format(I, "0")) & "  "
                S1 = S1 & LPad(3, Format(seq1, "0")) & "  "
                S1 = S1 & (RPad2(20, ACName(I)))

            End If

            If CriticalACTotalEquivCovs(I) = 0.0 Then
                Continue For
            End If

            'S1 = S1 & (LPad(10, Format(GL(I), "#,###,###")))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "

            '7777777
            If AC(LibIndex(I)).libGear = "H" Or (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * gPcnt1(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            'S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libMGpcntPCN, "#0.00")))

            S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libCP, "##.0")))
            S1 = S1 & (LPad(11, Format(gACN_GL_thick(I), "##.00")))
            S1 = S1 & (LPad(10, Format(gACN_GL(I), "##.0"))) & NL1
        Next

        S1 = S1 & NL1
        S1 = S1 & LPad(5, CStr(Format(gETimemsecs, "#0"))) & " sec." & NL1
        S1 = S1 & LPad(4, CStr(Format(gETimemsecs / 60, "#0.00"))) & " min."

        'extra output start XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        S1 = S1 & NL2
        S1 = S1 & "===============================================" & NL1

        S1 = S1 & LPad(3, "LP") &
            LPad(10, "PtoC") & "  " &
            LPad(15, "NtoFail_life") & "  " &
            LPad(15, "CriticTot_targ") & "  " &
            LPad(15, "Cov_Life") & "  " _
            & LPad(15, "Difference") &
            LPad(12, "%") & NL


        For i As Integer = 1 To NAC
            S1 = S1 & (LPad(3, Format(i, "#")))
            S1 = S1 & (LPad(10, Format(gPtoC(i), "#0.00000"))) & "  "
            S1 = S1 & (LPad(15, Format(gFlex_NtoFail_Copy(i), "#,##0.00"))) & "  "
            S1 = S1 & (LPad(15, Format(CriticalACTotalEquivCovs(i), "#,##0.00"))) & "  "
            S1 = S1 & (LPad(15, Format(gNtoFail(i), "#,##0.00"))) & "  "
            Dim abc As Double
            abc = (gNtoFail(i) - CriticalACTotalEquivCovs(i)) / CriticalACTotalEquivCovs(i)

            S1 = S1 & (LPad(15, Format(gNtoFail(i) - CriticalACTotalEquivCovs(i), "#,##0.00")))
            S1 = S1 & (LPad(12, Format(abc * 100, "#,##0.00000"))) & NL1
        Next

        S1 = S1 & NL2

        For j1 As Integer = 1 To NAC
            S1 = S1 & (LPad(3, Format(j1, "#")))
            For i1 As Integer = 1 To NAC
                S1 = S1 & (LPad(21, Format(gCriticalACEquivCovs(i1, j1), "#,##0.00")))
            Next
            S1 = S1 & NL1
        Next

        S1 = S1 & NL1

        For j1 As Integer = 1 To NAC
            S1 = S1 & (LPad(3, Format(j1, "#")))
            S1 = S1 & (LPad(21, Format(g1STRAIN(j1), "#,##0.00000000")))
            S1 = S1 & NL1
        Next

        'extra output   end XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        PCNOutputText = S1

        'Dim frmPCN As New frmPCN
        Dim frmPCN As New FormPCN
        frmPCN.Show()

    End Sub



    Public Sub Print_PCN_results2(ByVal lsPavementType As ACRClassLib.clsACR.PavementType, ByVal ConversionACCovs() As Double)

        Dim ThickCriticalAC(NAC) As Double
        Dim ACNThicknessRtn(,) As Double
        Dim S1, SS1 As String

        Dim DTemp As Double ' For printing to see if EvalThick reached.
        Dim UnitsOutName As String
        If EnglishUnits Then
            UnitsOutName = "English"
        Else
            UnitsOutName = "Metric"
        End If

        Dim MaxNWheels As Integer = 0 : Dim MaxNMainGears As Integer = 0

        For I As Integer = 1 To NAC
            If AC(LibIndex(I)).libNTires > MaxNWheels Then
                MaxNWheels = AC(LibIndex(I)).libNTires
            End If
        Next

        Dim FF As String

        Dim FullOutput As Boolean = True

        If EnglishUnits Then
            ICAOCode(0) = "D(7k)"
            ICAOCode(1) = "C(11k)"
            ICAOCode(2) = "B(17k)"
            ICAOCode(3) = "A(29k)"
        Else
            ICAOCode(0) = "D(50 MPa)"
            ICAOCode(1) = "C(80 Mpa)"
            ICAOCode(2) = "B(120 MPa)"
            ICAOCode(3) = "A(200 Mpa)"
        End If

        Dim InputGrossWeight(NAC) As Double, InputAnnualDepartures(NAC) As Double
        Dim ThickFlexible(NAC) As Double, ThickRigid(NAC) As Double
        Dim ThickLife(NAC) As Double




        FF = getTodaysDateFormatted()

        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
            FileName = "PCR Results Flexible " & FF & ".txt"
        ElseIf lsPavementType = ACRClassLib.clsACR.PavementType.Rigid Then
            FileName = "PCR Results Rigid " & FF & ".txt"
        End If

        S1 = "This file name = " & FileName & NL
        S1 = S1 & "Section name: " & SectName & " in job file: " & NL & WorkingDir & JobName & ".JOB.xml" & NL
        S1 = S1 & "Units = " & UnitsOutName & NL2

        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
            S1 = S1 & "Evaluation pavement type is flexible and design program is FAARFIELD." & NL
        ElseIf lsPavementType = ACRClassLib.clsACR.PavementType.Rigid Then
            S1 = S1 & "Evaluation pavement type is rigid and design program is FAARFIELD." & NL
        End If

        SS1 = ""
        S1 = S1 & SS1.ToString() & NL

        Dim modul1 As Single
        modul1 = Modulus(NPLayers)
        ICAOCodeIndex = ICAOCodeIndexF(modul1) 'FAARFIELD


        SS1 = CStr((ICAOCode(ICAOCodeIndex - 1)))

        S1 = S1 & StrDup(24, " ") & "Subgrade Modulus = " & Format(Modulus(NPLayers), "#,##0") & " psi" & " (Subgrade Category is " & SS1.ToString() & ")" & NL

        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
        ElseIf lsPavementType = ACRClassLib.clsACR.PavementType.Rigid Then
            S1 = S1 & StrDup(23, " ") & "Flexural strength = "
            S1 = S1 & Format(ConcreteFlexuralStrength * UnitsOut.psi, UnitsOut.psiFormat) & " " & UnitsOut.psiName & NL1
        End If

        S1 = S1 & StrDup(11, " ") & "Evaluation pavement thickness = "
        S1 = S1 & Format(EvalThick * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName & NL
        S1 = S1 & "     Pass to Traffic Cycle (PtoTC) Ratio = " & Format(PtoTC, "0.00")
        If PtoTC < 1.0# Or 3.0# < PtoTC Or (PtoTC - Math.Floor(PtoTC)) <> 0 Then S = S & " (non-standard)"
        S1 = S1 & NL
        S1 = S1 & "       Maximum number of wheels per gear = " & Format(MaxNWheels, "0") & NL

        If OverlayRigOnRig Or DesignType = FlexOnRigid Then
            S1 = S1 & StrDup(28, " ") & "Overlay Life = " & Format(gOverlayLife_target_copy, "#,##0.000") & NL
            'ElseIf DesignType = FlexOnRigid Then
            '    If AnalyzedasFlexible Then

            '    Else

            '    End If
        Else
            S1 = S1 & StrDup(37, " ") & "CDF = " & Format(gCDF_target_copy, "#,##0.000") & NL
        End If


        S1 = S1 & NL
        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
            'The FAA recommends a reference section assuming" & NL
            If MaxNWheels >= 4 Then
                S1 = S1 & "At least one aircraft has 4 or more wheels per gear." & NL2
            Else
                S1 = S1 & "No aircraft have 4 or more wheels per gear." & NL2
            End If
        End If

        'ACName(1) = "1234567890-1234567890-123"
        S1 = S1 & "Results Table 1. Input Traffic Data" & NL
        S1 = S1 & "                             Gross   Percent    Tire     Annual      20-yr" & NL
        S1 = S1 & " No.  Aircraft Name          Weight  Gross Wt   Press     Deps     Coverages" & NL
        S1 = S1 & StrDup(76, "-") & NL

        For I As Integer = 1 To NAC
            S1 = S1 & LPad(3, Format(I, "0")) & "  " & RPad2(20, ACName(I))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "

            If AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))

            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))

            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            S1 = S1 & LPad(10, String.Format("{0:N1}", AC(LibIndex(I)).libCP * UnitsOut.psi, UnitsOut.psiFormat))
            S1 = S1 & LPad(10, String.Format("{0:N0}", RepsAnnual(I), "#,###,##0")) ' GFH 9/28/09.
            S1 = S1 & LPad(12, String.Format("{0:N0}", ConversionACCovs(I), "#,###,##0"))
            S1 = S1 & NL
        Next I

        '============================================================================================
        S1 = S1 & NL & NL
        'ListBoxLibraryAircraft.SelectedIndex = lstLibFileListIndexSave

        S1 = S1 & "Results Table 2. PCR Value" & NL
        S1 = S1 & "                     Critical        Max. Allowable      ACR Thick" & NL
        S1 = S1 & "                  Aircraft Total    Gross Wt of the        at max.    PCR on" & NL
        S1 = S1 & "Aircraft Name    Equiv. Departures Critical Aircraft         MGW       " & ICAOCode(ICAOCodeIndex - 1) & NL
        S1 = S1 & StrDup(76, "-") & NL

        FullOutput = False '!!!!!
        ReDim ACNrtn(NAC, 4)
        ReDim ACNThicknessRtn(NAC, 4)

        Dim seq1 As Integer = 0
        Dim CDFTotal As Double = 0.0#

        S1 = S1 & RPad2(20, gNewACName(indexPCNtable2))         'S1 = S1 & LPad(11, "")
        S1 = S1 & LPad(12, String.Format("{0:N0}", gNewAnnualDepart(indexPCNtable2)))
        S1 = S1 & LPad(16, String.Format("{0:N0}", gNewGL(indexPCNtable2) * UnitsOut.pounds))
        S1 = S1 & LPad(16, String.Format("{0:N2}", gNewPCNthick(indexPCNtable2) * UnitsOut.inch))
        S1 = S1 & LPad(12, String.Format("{0:N1}", gNewPCN(indexPCNtable2))) & NL

        'J = IIf(FullOutput, 85, 70)
        J = 70
        'S1 = S1 & New String(" "c, J) & "Total CDF =" & LPad(9, String.Format("{0:N4}", CDFTotal)) & NL
        'S1 = S1 & New String(" "c, 53) & "Total CDF =" & LPad(11, String.Format("{0:N4}", CDFTotal)) & NL2
        S1 = S1 & NL & NL
        '==========================================================================================

        Dim SS7, sT1 As String
        SS7 = LPad(8, ICAOCode(ICAOCodeIndex - 1))
        sT1 = PaveTypeF(lPavementType)

        S1 = S1 & "Results Table 3. " & sT1 & " ACR at Indicated Gross Weight and Strength" & NL1
        S1 = S1 & " No. Aircraft Name          Gross    % GW on     Tire       ACR       ACR on" & NL1
        S1 = S1 & "                            Weight  Main Gear  Pressure    Thick    " & SS7 & NL1
        S1 = S1 & StrDup(76, "-") & NL

        seq1 = 0
        For I As Integer = 1 To NAC

            Call Check_H_Aircraft(I)

            'If H_index = I - 1 Then
            '    Continue For
            'End If

            If H_index = 0 Then
            Else
                If H_index = I - 1 Then
                    Continue For
                End If
            End If

            seq1 = seq1 + 1
            S1 = S1 & LPad(3, Format(seq1, "0")) & "  "
            S1 = S1 & (RPad2(20, ACName(I)))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "

            If AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "H" Then
                If (Mid(ACName(I), 1, 4) = "A380") Or (Mid(ACName(I), 1, 4) = "B747") Then
                    S1 = S1 & LPad(7, String.Format("{0:N2}", MGpcnt(I) * 200 + MGpcnt(I + 1) * 200))
                Else
                    S1 = S1 & LPad(7, String.Format("{0:N2}", MGpcnt(I) * 200 + MGpcnt(I + 1) * 100))
                End If
            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            'If AC(LibIndex(I)).libGear = "H" Or (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
            '    'S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * gPcnt1(I) * 100))
            '    'S1 = S1 & LPad(7, String.Format("{0:N2}", MGpcnt(I) * 100 + MGpcnt(I + 1) * 100))
            '    S1 = S1 & LPad(7, String.Format("{0:N2}", AC(LibIndex(I)).libMGpcnt * 200 + AC(LibIndex(I + 1)).libMGpcnt * 200))
            'ElseIf AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
            '    S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            'Else
            '    S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            'End If
            ''S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libMGpcntPCN, "#0.00")))

            S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libCP, "##.0")))
            S1 = S1 & (LPad(11, Format(gACN_GL_thick(I), "##.00")))
            S1 = S1 & (LPad(12, Format(gACN_GL(I), "##.0"))) & NL1
        Next

        S1 = S1 & NL1
        S1 = S1 & LPad(5, CStr(Format(gETimemsecs, "#0"))) & " sec." & NL1
        S1 = S1 & LPad(4, CStr(Format(gETimemsecs / 60, "#0.00"))) & " min."

        PCNOutputText = S1

        'If gRunTest Then Exit Sub

        'Dim frmPCN As New frmPCN
        Dim frmPCN As New FormPCN
        frmPCN.Show()

    End Sub




    Public Sub Set_NAC_lessOne_Aircraft_dataM(ByVal in1 As Integer, ByRef call1 As Integer)
        Dim ind3 As Integer

        NAC = SaveM_NAC(call1)
        ind3 = 0

        If H_index = 0 Then
            For i11 As Integer = 1 To NAC
                If i11 = in1 Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = SaveM_LibIndex(i11, call1)
                ACName(ind3) = SaveM_ACName(i11, call1)
                GL(ind3) = SaveM_GL(i11, call1)
                RepsAnnual(ind3) = SaveM_RepsAnnual(i11, call1)
                RepsInc(ind3) = SaveM_RepsInc(i11, call1)
                MGpcnt(ind3) = SaveM_MGpcnt(i11, call1)
                NAC_order(ind3) = SaveM_NAC_order(i11, call1)
            Next
            NAC = SaveM_NAC(call1) - 1S
        Else
            For i11 As Integer = 1 To NAC
                If (i11 = H_index) Or (i11 = H_index + 1) Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = SaveM_LibIndex(i11, call1)
                ACName(ind3) = SaveM_ACName(i11, call1)
                GL(ind3) = SaveM_GL(i11, call1)
                RepsAnnual(ind3) = SaveM_RepsAnnual(i11, call1)
                RepsInc(ind3) = SaveM_RepsInc(i11, call1)
                MGpcnt(ind3) = SaveM_MGpcnt(i11, call1)
                NAC_order(ind3) = SaveM_NAC_order(i11, call1)
            Next
            NAC = SaveM_NAC(call1) - 2S
        End If

    End Sub



    Public Sub Set_NAC_lessOne_Aircraft_data(ByVal in1 As Integer)
        Dim ind3 As Integer

        NAC = Save_NAC
        ind3 = 0

        If H_index = 0 Then
            For i11 As Integer = 1 To NAC
                If i11 = in1 Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save_LibIndex(i11)
                ACName(ind3) = Save_ACName(i11)
                GL(ind3) = Save_GL(i11)
                RepsAnnual(ind3) = Save_RepsAnnual(i11)
                RepsInc(ind3) = Save_RepsInc(i11)
                MGpcnt(ind3) = Save_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save_NAC - 1S
        Else
            For i11 As Integer = 1 To NAC
                If (i11 = H_index) Or (i11 = H_index + 1) Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save_LibIndex(i11)
                ACName(ind3) = Save_ACName(i11)
                GL(ind3) = Save_GL(i11)
                RepsAnnual(ind3) = Save_RepsAnnual(i11)
                RepsInc(ind3) = Save_RepsInc(i11)
                MGpcnt(ind3) = Save_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save_NAC - 2S
        End If

    End Sub



    Public Sub Set_NAC_lessOne_Aircraft_data2(ByVal in1 As Integer)
        Dim ind3 As Integer

        NAC = Save2_NAC
        ind3 = 0

        If H_index = 0 Then
            For i11 As Integer = 1 To NAC
                If i11 = in1 Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save2_LibIndex(i11)
                ACName(ind3) = Save2_ACName(i11)
                GL(ind3) = Save2_GL(i11)
                RepsAnnual(ind3) = Save2_RepsAnnual(i11)
                RepsInc(ind3) = Save2_RepsInc(i11)
                MGpcnt(ind3) = Save2_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save2_NAC - 1S
        Else
            For i11 As Integer = 1 To NAC
                If (i11 = H_index) Or (i11 = H_index + 1) Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save2_LibIndex(i11)
                ACName(ind3) = Save2_ACName(i11)
                GL(ind3) = Save2_GL(i11)
                RepsAnnual(ind3) = Save2_RepsAnnual(i11)
                RepsInc(ind3) = Save2_RepsInc(i11)
                MGpcnt(ind3) = Save2_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save2_NAC - 2S
        End If

    End Sub






    Public Sub Check_H_Aircraft(ByVal index1 As Integer)

        H_index = 0

        Try

            If index1 >= 1 Then
                If AC(LibIndex(index1)).libACName.Contains("Belly") Then
                    H_index = index1 - 1
                End If
            ElseIf AC(LibIndex(index1 + 1)).libACName.Contains("Belly") Then
                H_index = index1
            End If
        Catch ex As Exception

        End Try

    End Sub


    Public Function Check_H_Aircraft_Function(ByVal index1 As Integer) As Boolean

        H_index = 0


        Try

            If index1 > 1 Then
                If AC(LibIndex(index1)).libACName.Contains("Belly") Then
                    H_index = index1 - 1
                End If
            ElseIf AC(LibIndex(index1 + 1)).libACName.Contains("Belly") Then
                H_index = index1
            End If
        Catch ex As Exception

        End Try

        If H_index = 0 Then
            Check_H_Aircraft_Function = False
        Else
            If H_index = index1 - 1 Then
                Check_H_Aircraft_Function = True
            End If
        End If


        'If AC(LibIndex(index1)).libGear = "H" Then
        '    H_index = index1
        'ElseIf index1 > 1 Then
        '    If AC(LibIndex(index1 - 1)).libGear = "H" Then
        '        H_index = index1 - 1
        '    End If
        'End If

        'If H_index = 0 Then
        '    Check_H_Aircraft_Function = False
        'Else
        '    If H_index = index1 - 1 Then
        '        Check_H_Aircraft_Function = True
        '    End If
        'End If


    End Function



    Public Sub Set_OneAircraft_data3(ByVal in1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = Save3_LibIndex(in1)
            ACName(1) = Save3_ACName(in1)
            GL(1) = Save3_GL(in1)
            RepsAnnual(1) = Save3_RepsAnnual(in1)
            RepsInc(1) = Save3_RepsInc(in1)
            MGpcnt(1) = Save3_MGpcnt(in1)
            NAC_order(1) = Save3_NAC_order(in1)

        Else

            NAC = 2
            LibIndex(1) = Save3_LibIndex(H_index)
            ACName(1) = Save3_ACName(H_index)
            GL(1) = Save3_GL(H_index)
            RepsAnnual(1) = Save3_RepsAnnual(H_index)
            RepsInc(1) = Save3_RepsInc(H_index)
            MGpcnt(1) = Save3_MGpcnt(H_index)
            NAC_order(1) = Save3_NAC_order(H_index)

            LibIndex(2) = Save3_LibIndex(H_index + 1)
            ACName(2) = Save3_ACName(H_index + 1)
            GL(2) = Save3_GL(H_index + 1)
            RepsAnnual(2) = Save3_RepsAnnual(H_index + 1)
            RepsInc(2) = Save3_RepsInc(H_index + 1)
            MGpcnt(2) = Save3_MGpcnt(H_index + 1)
            NAC_order(1) = Save3_NAC_order(H_index + 1)

        End If

    End Sub




    Public Sub Set_OneAircraft_data2(ByVal in1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = Save2_LibIndex(in1)
            ACName(1) = Save2_ACName(in1)
            GL(1) = Save2_GL(in1)
            RepsAnnual(1) = Save2_RepsAnnual(in1)
            RepsInc(1) = Save2_RepsInc(in1)
            MGpcnt(1) = Save2_MGpcnt(in1)
            NAC_order(1) = Save2_NAC_order(in1)

        Else

            NAC = 2
            LibIndex(1) = Save2_LibIndex(H_index)
            ACName(1) = Save2_ACName(H_index)
            GL(1) = Save2_GL(H_index)
            RepsAnnual(1) = Save2_RepsAnnual(H_index)
            RepsInc(1) = Save2_RepsInc(H_index)
            MGpcnt(1) = Save2_MGpcnt(H_index)
            NAC_order(1) = Save2_NAC_order(H_index)

            LibIndex(2) = Save2_LibIndex(H_index + 1)
            ACName(2) = Save2_ACName(H_index + 1)
            GL(2) = Save2_GL(H_index + 1)
            RepsAnnual(2) = Save2_RepsAnnual(H_index + 1)
            RepsInc(2) = Save2_RepsInc(H_index + 1)
            MGpcnt(2) = Save2_MGpcnt(H_index + 1)
            NAC_order(2) = Save2_NAC_order(H_index + 1)

        End If

    End Sub


    Public Sub Set_OneAircraft_dataM(ByVal in1 As Integer, ByVal call1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = SaveM_LibIndex(in1, call1)
            ACName(1) = SaveM_ACName(in1, call1)
            GL(1) = SaveM_GL(in1, call1)
            RepsAnnual(1) = SaveM_RepsAnnual(in1, call1)
            RepsInc(1) = SaveM_RepsInc(in1, call1)
            MGpcnt(1) = SaveM_MGpcnt(in1, call1)
            NAC_order(1) = SaveM_NAC_order(in1, call1)

        Else

            NAC = 2
            LibIndex(1) = SaveM_LibIndex(H_index, call1)
            ACName(1) = SaveM_ACName(H_index, call1)
            GL(1) = SaveM_GL(H_index, call1)
            RepsAnnual(1) = SaveM_RepsAnnual(H_index, call1)
            RepsInc(1) = SaveM_RepsInc(H_index, call1)
            MGpcnt(1) = SaveM_MGpcnt(H_index, call1)
            NAC_order(1) = SaveM_NAC_order(H_index, call1)

            LibIndex(2) = SaveM_LibIndex(H_index + 1, call1)
            ACName(2) = SaveM_ACName(H_index + 1, call1)
            GL(2) = SaveM_GL(H_index + 1, call1)
            RepsAnnual(2) = SaveM_RepsAnnual(H_index + 1, call1)
            RepsInc(2) = SaveM_RepsInc(H_index + 1, call1)
            MGpcnt(2) = SaveM_MGpcnt(H_index + 1, call1)
            NAC_order(2) = SaveM_NAC_order(H_index + 1, call1)

        End If



    End Sub


    Public Sub Set_OneAircraft_data(ByVal in1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = Save_LibIndex(in1)
            ACName(1) = Save_ACName(in1)
            GL(1) = Save_GL(in1)
            RepsAnnual(1) = Save_RepsAnnual(in1)
            RepsInc(1) = Save_RepsInc(in1)
            MGpcnt(1) = Save_MGpcnt(in1)
            NAC_order(1) = Save_NAC_order(in1)

        Else

            NAC = 2
            LibIndex(1) = Save_LibIndex(H_index)
            ACName(1) = Save_ACName(H_index)
            GL(1) = Save_GL(H_index)
            RepsAnnual(1) = Save_RepsAnnual(H_index)
            RepsInc(1) = Save_RepsInc(H_index)
            MGpcnt(1) = Save_MGpcnt(H_index)
            NAC_order(1) = Save_NAC_order(H_index)

            LibIndex(2) = Save_LibIndex(H_index + 1)
            ACName(2) = Save_ACName(H_index + 1)
            GL(2) = Save_GL(H_index + 1)
            RepsAnnual(2) = Save_RepsAnnual(H_index + 1)
            RepsInc(2) = Save_RepsInc(H_index + 1)
            MGpcnt(2) = Save_MGpcnt(H_index + 1)
            NAC_order(2) = Save_NAC_order(H_index + 1)

        End If


    End Sub

    Public Sub RedimSaveM(ByVal iNAC As Integer)

        ReDim SaveM_NAC(iNAC)
        ReDim SaveM_LibIndex(iNAC, iNAC)
        ReDim SaveM_ACName(iNAC, iNAC)
        ReDim SaveM_GL(iNAC, iNAC)
        ReDim SaveM_RepsAnnual(iNAC, iNAC)
        ReDim SaveM_RepsInc(iNAC, iNAC)
        ReDim SaveM_WT(iNAC, iNAC)
        ReDim SaveM_TW(iNAC, iNAC)
        ReDim SaveM_MGpcnt(iNAC, iNAC)
        ReDim SaveM_NAC_order(iNAC, iNAC)

    End Sub



    Public Sub SaveM_Aircraft_data(ByVal ind1 As Integer)

        SaveM_NAC(ind1) = NAC
        For I As Short = 1 To NAC
            SaveM_LibIndex(I, ind1) = LibIndex(I)
            SaveM_ACName(I, ind1) = ACName(I)
            SaveM_GL(I, ind1) = GL(I)
            SaveM_RepsAnnual(I, ind1) = RepsAnnual(I)
            SaveM_RepsInc(I, ind1) = RepsInc(I)
            SaveM_MGpcnt(I, ind1) = MGpcnt(I)
            SaveM_NAC_order(I, ind1) = NAC_order(I)
        Next

    End Sub

    Public Sub Save_Aircraft_data()

        Save_NAC = NAC
        For I As Short = 1 To NAC
            Save_LibIndex(I) = LibIndex(I)
            Save_ACName(I) = ACName(I)
            Save_GL(I) = GL(I)
            Save_RepsAnnual(I) = RepsAnnual(I)
            Save_RepsInc(I) = RepsInc(I)
            Save_MGpcnt(I) = MGpcnt(I)
            Save_NAC_order(I) = NAC_order(I)
        Next

    End Sub

    Public Sub Save2_Aircraft_data()

        Save2_NAC = NAC
        For I As Short = 1 To NAC
            Save2_LibIndex(I) = LibIndex(I)
            Save2_ACName(I) = ACName(I)
            Save2_GL(I) = GL(I)
            Save2_RepsAnnual(I) = RepsAnnual(I)
            Save2_RepsInc(I) = RepsInc(I)
            Save2_MGpcnt(I) = MGpcnt(I)
            Save2_NAC_order(I) = NAC_order(I)
        Next

    End Sub


    Public Sub Save3_Aircraft_data()

        Save3_NAC = NAC
        For I As Short = 1 To NAC
            Save3_LibIndex(I) = LibIndex(I)
            Save3_ACName(I) = ACName(I)
            Save3_GL(I) = GL(I)
            Save3_RepsAnnual(I) = RepsAnnual(I)
            Save3_RepsInc(I) = RepsInc(I)
            Save3_MGpcnt(I) = MGpcnt(I)
            Save3_NAC_order(I) = NAC_order(I)
        Next

    End Sub






    Public Sub Restore_Aircraft_data()

        NAC = Save_NAC
        For I As Short = 1 To NAC
            LibIndex(I) = Save_LibIndex(I)
            ACName(I) = Save_ACName(I)
            GL(I) = Save_GL(I)
            RepsAnnual(I) = Save_RepsAnnual(I)
            RepsInc(I) = Save_RepsInc(I)
            MGpcnt(I) = Save_MGpcnt(I)
            NAC_order(I) = Save_NAC_order(I)
        Next

    End Sub


    Public Sub Set_Aircraft_data(ByVal ind1 As Short)

        LibIndex(1) = Save_LibIndex(ind1)
        ACName(1) = Save_ACName(ind1)
        GL(1) = Save_GL(ind1)
        RepsAnnual(1) = Save_RepsAnnual(ind1)
        RepsInc(1) = Save_RepsInc(ind1)
        MGpcnt(1) = Save_MGpcnt(ind1)
        NAC_order(1) = Save_NAC_order(ind1)

    End Sub

    Public Sub NewIterationMethod(ByVal ind As Integer)

        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If

        GL(1) = CSng(GrossWeight)

        LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        '  ' g1STRAIN(MaxSectAC) As Double

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


    End Sub



    Public Sub NewIterationFlexible(ByVal ind As Integer)

        Dim LDiff As Single = 0.095 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.01
        End If



        GL(1) = CSng(GrossWeight)

        LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        ' Call frmStructure.cmdAddDelete_Click(sender, e)

        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If



        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If



    End Sub

    Public Sub ACNFlexComp_FF_Life(ByVal ind As Integer)

        GL(1) = CSng(GrossWeight)

        LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        '   Call frmStructure.cmdAddDelete_Click(sender, e)

        'GrossWeight = GL(1)

        CBRThickness(1) = 0

        If DesignType = NewFlex Then 'ACNFlexComp_FF_Life
            For i As Short = 1 To NPLayers
                CBRThickness(1) = CBRThickness(1) + Thick(i)
            Next
        Else
            CBRThickness(1) = Thick(1)
        End If




    End Sub

    Public Sub ACNFlexComp_FF(ByVal ind As Integer)

        GL(1) = CSng(GrossWeight)

        LifeComputation = False : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        '  Call frmStructure.cmdAddDelete_Click(sender, e)
        'Call frmStructure.cmdLife_Click(sender, e)

        'GrossWeight = GL(1)

        CBRThickness(1) = 0

        If DesignType = NewFlex Then
            For i As Short = 1 To NPLayers
                CBRThickness(1) = CBRThickness(1) + Thick(i)
            Next
        Else
            CBRThickness(1) = Thick(1)
        End If

    End Sub

    Public Sub GetMGW_FF(ByRef CallSource As String, ByRef MGW As Double,
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

            '-----------------------------------------------------
            'Call NewIterationFlexible(Ind)
            Call NewIterationMethod(Ind)


            CriticalACThick = CBRThickness(1) ' Used only for output.
            FuncM1 = CBRThickness(1) - EvalThick
            ReturnCoverages = FlexibleCoverages
            '-----------------------------------------------------


            GoTo Skip_To_100





            ACNFlexComp_FF(Ind) '1
            CriticalACThick = CBRThickness(1) ' Used only for output.
            FuncM1 = CBRThickness(1) - EvalThick

            Do
                J += 1
                GrossWt = GrossWeight
                GrossWeight += DelGrossWt
                LastThick = CBRThickness(1)
                ACNFlexComp_FF(Ind) '2
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
                ACNFlexComp_FF(Ind) '3
                FuncM1 = CBRThickness(1) - EvalThick

                'Application.DoEvents()
                '      Debug.Print J; "GrossWeight = "; GrossWeight; " "; NewtonCoeff
                NewtonCoeff = 1.0#
            Loop Until Math.Abs(FuncM1) < 0.0001


Skip_To_100:

        Else

            'PPPP
            'chkPCAThicknessDesignSave = CheckBoxPCAThick.IsChecked
            'If CheckBoxPCAMGW.IsChecked = True Then
            '    CheckBoxPCAThick.IsChecked = True
            'Else
            '    CheckBoxPCAThick.IsChecked = False
            'End If

            'Call NewIterationFlexible(Ind) 'for Rigid


            Call NewIterationMethod(Ind) 'for Rigid



            CriticalACThick = CBRThickness(1) ' Used only for output.
            FuncM1 = CBRThickness(1) - EvalThick
            ReturnCoverages = FlexibleCoverages
            '-----------------------------------------------------


            GoTo Skip_To_222



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


Skip_To_222:

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




    Public Sub NewAdjustAnnDepart_Old()

        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If

        LDiff = gCDF_target * CSng(gTolerance)


        'GL(1) = CSng(GrossWeight)

        'LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        'Call frmStructure.cmdAddDelete_Click(sender, e)
        Call FEDFAA1.PCNLifeCalc()

        'If Math.Abs(LifeStr - 20) < LDiff Then
        If Math.Abs(CDFPic - gCDF_target) < LDiff Then
            Exit Sub
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                Call FEDFAA1.PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                Call FEDFAA1.PCNLifeCalc()
            Loop
        End If

        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + 100
                Call FEDFAA1.PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - 100
                If RepsAnnual(1) <= 0 Then
                    RepsAnnual(1) = 100
                End If
                Call FEDFAA1.PCNLifeCalc()
            Loop
        End If




        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + 10
                Call FEDFAA1.PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - 10
                If RepsAnnual(1) <= 0 Then
                    RepsAnnual(1) = 10
                End If
                Call FEDFAA1.PCNLifeCalc()
            Loop
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + 1
                Call FEDFAA1.PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - 1
                If RepsAnnual(1) <= 0 Then
                    RepsAnnual(1) = 1
                End If
                Call FEDFAA1.PCNLifeCalc()
            Loop
        End If



    End Sub



    Public Sub NewAdjustGrossWeight_Old()

        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If

        LDiff = gCDF_target * CSng(gTolerance)

        'GL(1) = CSng(GrossWeight)

        'LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        'Call frmStructure.cmdAddDelete_Click(sender, e)
        Call FEDFAA1.PCNLifeCalc()

        'If Math.Abs(LifeStr - 20) < LDiff Then
        If Math.Abs(CDFPic - gCDF_target) < LDiff Then
            Exit Sub
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + GL(1) * CSng(0.1)
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
                ' frmStructure.Refresh()
                System.Windows.Forms.Application.DoEvents()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - GL(1) * CSng(0.1)
                If GL(1) <= 0 Then
                    GL(1) = 100
                End If
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
                ' frmStructure.Refresh()
                System.Windows.Forms.Application.DoEvents()

            Loop
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + 2000
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - 2000
                If GL(1) <= 0 Then
                    GL(1) = 2000
                    Exit Do
                End If
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
            Loop
        End If



        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + 100
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - 100
                If GL(1) <= 0 Then
                    GL(1) = 100
                    Exit Do
                End If
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
            Loop
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + 10
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - 10
                If GL(1) <= 0 Then
                    GL(1) = 10
                    Exit Do
                End If
                Call FEDFAA1.PCNLifeCalc()
                CDFPic = CStr(Math.Round(CDFPic, 4))
            Loop
        End If



    End Sub



    Public Sub NewAdjustGrossWeight2017()

        Dim target1, result1 As Single
        Dim LDiff As Single
        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        target1 = GetTarget1()
        LDiff = target1 * CSng(gTolerance)
        result1 = GetResult1()


        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            If CDFPic > target1 Then
                Do
                    tt2 = GL(1) : SS2 = CDFPic

                    If CDFPic / gCDF_target > 10 Then
                        GL(1) = CSng(GL(1) * 0.85)
                    ElseIf CDFPic / gCDF_target > 4 Then
                        GL(1) = CSng(GL(1) * 0.9)
                    ElseIf CDFPic / gCDF_target > 2 Then
                        GL(1) = CSng(GL(1) * 0.93)
                    ElseIf CDFPic / gCDF_target > 1.2 Then
                        GL(1) = CSng(GL(1) * 0.95)
                    Else
                        GL(1) = CSng(GL(1) * 0.97)
                    End If

                    If GL(1) <= 100 Then
                        GL(1) = 100
                        GL(2) = GL(1)
                        If SS1 = SS2 Then Exit Sub
                    End If
                    GL(2) = GL(1)

                    result1 = GetResult1()
                    tt1 = GL(1) : SS1 = CDFPic
                Loop Until (Math.Abs(CDFPic - target1) < LDiff) Or (CDFPic < target1)
            Else
                Do
                    tt1 = GL(1) : SS1 = CDFPic
                    If CDFPic / gCDF_target < 0.01 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.25)
                    ElseIf CDFPic / gCDF_target < 0.1 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.17)
                    ElseIf CDFPic / gCDF_target < 0.3 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.075)
                    ElseIf CDFPic / gCDF_target < 0.75 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.045)
                    Else
                        GL(1) = GL(1) + GL(1) * CSng(0.02)
                    End If

                    GL(2) = GL(1)
                    result1 = GetResult1()
                    tt2 = GL(1) : SS2 = CDFPic
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic > gCDF_target)
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic - gCDF_target) < LDiff) Then
            Exit Sub
        End If

        If GL(1) <= 2 Then
            If gCDF_target > CDFPic Then
                Exit Sub
            End If
        End If

        GL(1) = CSng((tt1 + tt2) / 2)
        GL(2) = GL(1)
        result1 = GetResult1()

        Dim zz1, zz2, aa, bb As Double
rep1:

        If SS1 < 0.001 Then
            SS1 = 0.001
        End If

        If SS2 < 0.001 Then
            SS2 = 0.001
        End If

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1

        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        GL(1) = CSng(ttACN)
        GL(2) = GL(1)
        result1 = GetResult1()

        If CDFPic < gCDF_target Then
            tt2 = GL(1)
            SS2 = CDFPic
        Else
            tt1 = GL(1)
            SS1 = CDFPic
        End If
        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            GoTo rep1
        End If

    End Sub




    Public Sub NewAdjustGrossWeight2()

        gIterLevel1A = 0 : gIterLevel1B = 0 : gIterLevel2 = 0
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        LDiff = gCDF_target * CSng(gTolerance)
        Dim SS1, SS2, tt1, tt2 As Double
        '====================================================================
        'Call SubLoopCalculations()
        '====================================================================

        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            If CDFPic > gCDF_target Then
                Do
                    tt2 = GL(1) : SS2 = CDFPic

                    If CDFPic / gCDF_target > 100000 Then
                        GL(1) = CSng(GL(1) * 0.65)
                    ElseIf CDFPic / gCDF_target > 10000 Then
                        GL(1) = CSng(GL(1) * 0.7)
                    ElseIf CDFPic / gCDF_target > 1000 Then
                        GL(1) = CSng(GL(1) * 0.75)
                    ElseIf CDFPic / gCDF_target > 100 Then
                        GL(1) = CSng(GL(1) * 0.8)
                    ElseIf CDFPic / gCDF_target > 10 Then
                        GL(1) = CSng(GL(1) * 0.85)
                    ElseIf CDFPic / gCDF_target > 4 Then
                        GL(1) = CSng(GL(1) * 0.9)
                    ElseIf CDFPic / gCDF_target > 2 Then
                        GL(1) = CSng(GL(1) * 0.93)
                    ElseIf CDFPic / gCDF_target > 1.2 Then
                        GL(1) = CSng(GL(1) * 0.95)
                    Else
                        GL(1) = CSng(GL(1) * 0.97)
                    End If

                    If GL(1) <= 10 Then
                        GL(1) = 10 : GL(2) = 10
                        If SS1 = SS2 Then Exit Sub
                    End If
                    GL(2) = GL(1)

                    ' Call frmStructure.cmdLife_Click(Nothing, Nothing)
                    tt1 = GL(1) : SS1 = CDFPic
                    gIterLevel1A = gIterLevel1A + 1
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic < gCDF_target)
            Else
                Do
                    tt1 = GL(1) : SS1 = CDFPic
                    If CDFPic / gCDF_target < 0.01 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.25)
                    ElseIf CDFPic / gCDF_target < 0.1 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.17)
                    ElseIf CDFPic / gCDF_target < 0.3 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.075)
                    ElseIf CDFPic / gCDF_target < 0.75 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.045)
                    Else
                        GL(1) = GL(1) + GL(1) * CSng(0.02)
                    End If

                    GL(2) = GL(1)
                    ' Call frmStructure.cmdLife_Click(Nothing, Nothing)
                    tt2 = GL(1) : SS2 = CDFPic
                    gIterLevel1B = gIterLevel1B + 1
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic > gCDF_target)
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic - gCDF_target) < LDiff) Then
            Exit Sub
        End If

        If GL(1) <= 2 Then
            If gCDF_target > CDFPic Then
                Exit Sub
            End If
        End If

        GL(1) = CSng((tt1 + tt2) / 2)
        GL(2) = GL(1)
        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        Dim zz1, zz2, aa, bb As Double
        Dim iCountSteps As Integer = 0
rep1:
        iCountSteps = iCountSteps + 1

        Dim min99 As Double = 0.0000000001 'mod900
        min99 = 0.001
        If SS1 < min99 Then
            SS1 = min99
        End If

        If SS2 < min99 Then
            SS2 = min99
        End If

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1

        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        GL(1) = CSng(ttACN)
        GL(2) = GL(1)
        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        If CDFPic < gCDF_target Then
            tt2 = GL(1)
            SS2 = CDFPic
        Else
            tt1 = GL(1)
            SS1 = CDFPic
        End If
        If (Math.Abs(CDFPic - gCDF_target) > LDiff) And (iCountSteps < 25) Then
            gIterLevel2 = gIterLevel2 + 1
            GoTo rep1
        End If

    End Sub

    Public Sub NewAdjustAnnDepart2017() 'mod900

        Dim LDiff As Double = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gCDF_target * gTolerance
        Dim SS1, SS2 As Double, tt1, tt2 As Double
        Dim i11, i22, i33 As Integer
        i11 = 0 : i22 = 0 : i33 = 0

        Dim x11, x22, y11, y22, aaa, bbb As Double

        'Dim keep As Single
        'keep = RepsAnnual(1)
        'Call TEST_NewAdjustAnnDepart2017()
        'RepsAnnual(1) = keep


        If RepsAnnual(1) <= 0 And gCDF_target > 0 Then
            RepsAnnual(1) = 10
        End If

        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            If CDFPic > gCDF_target Then
                Do
                    i11 = i11 + 1
                    tt2 = RepsAnnual(1) : SS2 = CDFPic

                    If i11 = 1 Then
                        x11 = CDFPic
                        y11 = RepsAnnual(1)
                    End If

                    If i11 = 2 Then
                        x22 = CDFPic
                        y22 = RepsAnnual(1)
                    End If


                    If CDFPic / gCDF_target > 10 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf CDFPic / gCDF_target > 4 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf CDFPic / gCDF_target > 2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf CDFPic / gCDF_target > 1.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If
                    If RepsAnnual(1) <= 2 Then
                        RepsAnnual(1) = 2
                    End If

                    If i11 = 2 Then
                        bbb = (y22 - y11) / (x22 - x11)
                        aaa = y11 - bbb * x11
                        RepsAnnual(1) = CSng(aaa + bbb * gCDF_target)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    ' Call frmStructure.cmdLife_Click(Nothing, Nothing)
                    tt1 = RepsAnnual(1) : SS1 = CDFPic
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic < gCDF_target)
            Else
                Do
                    i22 = i22 + 1
                    tt1 = RepsAnnual(1) : SS1 = CDFPic

                    If i22 = 1 Then
                        x11 = CDFPic
                        y11 = RepsAnnual(1)
                    End If

                    If i22 = 2 Then
                        x22 = CDFPic
                        y22 = RepsAnnual(1)
                    End If

                    If CDFPic / gCDF_target < 0.001 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(36)
                    ElseIf CDFPic / gCDF_target < 0.01 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(18)
                    ElseIf CDFPic / gCDF_target < 0.1 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(9)
                    ElseIf CDFPic / gCDF_target < 0.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf CDFPic / gCDF_target < 0.75 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf CDFPic / gCDF_target < 0.85 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    If i22 = 2 Then
                        bbb = (y22 - y11) / (x22 - x11)
                        aaa = y11 - bbb * x11
                        RepsAnnual(1) = CSng(aaa + bbb * gCDF_target)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    ' Call frmStructure.cmdLife_Click(Nothing, Nothing)
                    tt2 = RepsAnnual(1) : SS2 = CDFPic
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic > gCDF_target)
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic - gCDF_target) < LDiff) Then
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gCDF_target > CDFPic Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        Dim zz1, zz2, aa, bb As Double
rep1:
        If SS1 <= 0 Then
            SS1 = 1.1
        End If

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)
        i33 = i33 + 1

        If CDFPic < gCDF_target Then
            tt2 = RepsAnnual(1)
            SS2 = CDFPic
        Else
            tt1 = RepsAnnual(1)
            SS1 = CDFPic
        End If
        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            GoTo rep1
        End If

    End Sub



    Public Function CDFPic8() As Single
        'CDFPic8 = CDFdata2(1, 1, gCriticalOffset)
        CDFPic8 = CDFPic
    End Function



    Public Sub NewAdjustAnnDepart2()

        'Call NewAdjustAnnDepart2new()
        'Exit Sub
        gIterLevel1A = 0 : gIterLevel1B = 0 : gIterLevel2 = 0


        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gCDF_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Dim sender As Object, e As System.EventArgs
        Call FEDFAA1.PCNLifeCalc()

        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            If gCDF_target < CDFPic8() Then
                Do
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()

                    If CDFPic8() / gCDF_target > 10 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf CDFPic8() / gCDF_target > 4 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf CDFPic8() / gCDF_target > 2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf CDFPic8() / gCDF_target > 1.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If

                    'RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                    If RepsAnnual(1) <= 100 Then RepsAnnual(1) = 100
                    RepsAnnual(2) = RepsAnnual(1)
                    Call FEDFAA1.PCNLifeCalc()
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()
                    'Loop While (gCDF_target < CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                    gIterLevel1A = gIterLevel1A + 1
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target > CDFPic8())
            Else
                Do
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()

                    If CDFPic8() / gCDF_target < 0.01 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(15)
                    ElseIf CDFPic8() / gCDF_target < 0.1 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(7)
                    ElseIf CDFPic8() / gCDF_target < 0.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf CDFPic8() / gCDF_target < 0.75 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf CDFPic8() / gCDF_target < 0.85 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call FEDFAA1.PCNLifeCalc()
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()
                    gIterLevel1B = gIterLevel1B + 1
                    'Loop While (gCDF_target > CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target < CDFPic8())
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Then
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gCDF_target > CDFPic8() Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        Call FEDFAA1.PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        Call FEDFAA1.PCNLifeCalc()

        If CDFPic8() < gCDF_target Then
            tt2 = RepsAnnual(1)
            SS2 = CDFPic8()
        Else
            tt1 = RepsAnnual(1)
            SS1 = CDFPic8()
        End If

        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            gIterLevel2 = gIterLevel2 + 1
            GoTo rep1
        End If

    End Sub


    Public Sub NewAdjustAnnDepart2new()

        'Dim CDFPic8 As Single
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gCDF_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Dim sender As Object, e As System.EventArgs
        Call FEDFAA1.PCNLifeCalc()

        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            If gCDF_target < CDFPic8() Then
                Do
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()

                    If CDFPic8() / gCDF_target > 10 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf CDFPic8() / gCDF_target > 4 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf CDFPic8() / gCDF_target > 2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf CDFPic8() / gCDF_target > 1.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If

                    'RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                    If RepsAnnual(1) <= 100 Then RepsAnnual(1) = 100
                    RepsAnnual(2) = RepsAnnual(1)
                    Call FEDFAA1.PCNLifeCalc()
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()
                    'Loop While (gCDF_target < CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target > CDFPic8())
            Else
                Do
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()
                    If CDFPic8() / gCDF_target < 0.01 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(15)
                    ElseIf CDFPic8() / gCDF_target < 0.1 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(7)
                    ElseIf CDFPic8() / gCDF_target < 0.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf CDFPic8() / gCDF_target < 0.75 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf CDFPic8() / gCDF_target < 0.85 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call FEDFAA1.PCNLifeCalc()
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()
                    'Loop While (gCDF_target > CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target < CDFPic8())
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Then
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gCDF_target > CDFPic8() Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        Call FEDFAA1.PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        Call FEDFAA1.PCNLifeCalc()

        If CDFPic8() < gCDF_target Then
            tt2 = RepsAnnual(1)
            SS2 = CDFPic8()
        Else
            tt1 = RepsAnnual(1)
            SS1 = CDFPic8()
        End If
        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            GoTo rep1
        End If

    End Sub



    Public Sub NewAdjustAnnDepartOverlay()

        Dim gLife_target As Single = gOverlayLife_target
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gLife_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Dim sender As Object, e As System.EventArgs
        Call FEDFAA1.PCNLifeCalc()


        'Dim t77(100) As Single
        'For i As Integer = 100 To 1000 Step 1000
        '   call FEDFAA1.PCNLifeCalc()
        '    t77(i) = OverlayLife
        'Next


        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            If gLife_target > OverlayLife Then
                Do
                    tt2 = RepsAnnual(1) : SS2 = OverlayLife

                    If OverlayLife / gLife_target < 0.01 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf OverlayLife / gLife_target < 0.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf OverlayLife / gLife_target < 0.5 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf OverlayLife / gLife_target < 0.75 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If

                    'RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                    If RepsAnnual(1) <= 100 Then RepsAnnual(1) = 100
                    RepsAnnual(2) = RepsAnnual(1)
                    Call FEDFAA1.PCNLifeCalc()
                    tt1 = RepsAnnual(1) : SS1 = OverlayLife
                    'Loop While (gLife_target > OverlayLife) ' Or (Math.Abs(OverlayLife - gLife_target) < LDiff)
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (gLife_target < OverlayLife)
            Else
                Do
                    tt1 = RepsAnnual(1) : SS1 = OverlayLife

                    If OverlayLife / gLife_target > 100 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(15)
                    ElseIf OverlayLife / gLife_target > 10 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(7)
                    ElseIf OverlayLife / gLife_target > 3.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf OverlayLife / gLife_target > 1.33 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf OverlayLife / gLife_target > 1.2 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call FEDFAA1.PCNLifeCalc()
                    tt2 = RepsAnnual(1) : SS2 = OverlayLife
                    'Loop While (gLife_target < OverlayLife) 'Or (Math.Abs(OverlayLife - gLife_target) < LDiff)
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (gLife_target > OverlayLife)
            End If
        Else
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gLife_target > OverlayLife Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        Call FEDFAA1.PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gLife_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        Call FEDFAA1.PCNLifeCalc()

        If OverlayLife < gLife_target Then
            tt2 = RepsAnnual(1)
            SS2 = OverlayLife
        Else
            tt1 = RepsAnnual(1)
            SS1 = OverlayLife
        End If
        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            GoTo rep1
        End If

    End Sub




    Public Sub NewAdjustGrossWeightOverlay()

        Dim gLife_target As Single = gOverlayLife_target
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        LDiff = gLife_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            If (OverlayLife > gLife_target) Then
                Do
                    tt1 = GL(1) : SS1 = OverlayLife
                    If OverlayLife / gLife_target > 1000 Then
                        GL(1) = CSng(GL(1) * 2)
                    ElseIf OverlayLife / gLife_target > 100 Then
                        GL(1) = CSng(GL(1) * 1.75)
                    ElseIf OverlayLife / gLife_target > 10 Then
                        GL(1) = CSng(GL(1) * 1.5)
                    ElseIf OverlayLife / gLife_target > 5 Then
                        GL(1) = CSng(GL(1) * 1.25)
                    ElseIf OverlayLife / gLife_target < 2 Then
                        GL(1) = CSng(GL(1) * 1.1)
                    ElseIf OverlayLife / gLife_target < 1.5 Then
                        GL(1) = CSng(GL(1) * 1.05)
                    Else
                        GL(1) = CSng(GL(1) * 1.03)
                    End If

                    If GL(1) <= 100 Then
                        GL(1) = 100
                        GL(2) = GL(1)
                        If SS1 = SS2 Then Exit Sub
                    End If

                    GL(2) = GL(1)
                    ' Call frmStructure.cmdLife_Click(Nothing, Nothing)
                    tt2 = GL(1) : SS2 = OverlayLife
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (OverlayLife < gLife_target)
            Else
                Do
                    tt2 = GL(1) : SS2 = OverlayLife
                    If OverlayLife / gLife_target < 0.01 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.25)
                    ElseIf OverlayLife / gLife_target < 0.1 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.1)
                    ElseIf OverlayLife / gLife_target < 0.3 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.075)
                    ElseIf OverlayLife / gLife_target < 0.5 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.045)
                    Else
                        GL(1) = GL(1) - GL(1) * CSng(0.02)
                    End If

                    GL(2) = GL(1)
                    ' Call frmStructure.cmdLife_Click(Nothing, Nothing)
                    tt1 = GL(1) : SS1 = OverlayLife
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (OverlayLife > gLife_target)
            End If
        Else
            Exit Sub
        End If

        If (Math.Abs(OverlayLife - gLife_target) < LDiff) Then
            Exit Sub
        End If

        If GL(1) <= 2 Then
            If gLife_target > OverlayLife Then
                Exit Sub
            End If
        End If

        GL(1) = CSng((tt1 + tt2) / 2)
        GL(2) = GL(1)
        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gLife_target) + bb
        GL(1) = CSng(ttACN)
        GL(2) = GL(1)
        ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

        If OverlayLife < gLife_target Then
            tt2 = GL(1)
            SS2 = OverlayLife
        Else
            tt1 = GL(1)
            SS1 = OverlayLife
        End If
        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            GoTo rep1
        End If

    End Sub


    Public Sub ABC_Reset_CDFdata()

        If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then

        Else
            Exit Sub
        End If



        Dim IA, IOFF As Integer

        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
        For IA = 1 To NAC
            For IOFF = 1 To NOFF
                CDFdata(1, IA, IOFF) = 0
                CDFdata(1, NAC + 1, IOFF) = 0
            Next IOFF
        Next IA


        For IA = 1 To NAC
            For IOFF = 1 To NOFF
                'jobCDFtable(ISect, IA) = lclCDF(IA, IOFF) 'LeafCDFFlex
                CDFdata(1, IA, IOFF) = CSng(jobCDFtable(ISect, IA))
                CDFdata(1, NAC + 1, IOFF) = CDFdata(1, NAC + 1, IOFF) + CSng(jobCDFtable(ISect, IA))
            Next IOFF
        Next IA
        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

        Dim k1, k2, k3 As Integer

        For k1 = 1 To NAC + 1
            For k2 = 1 To 41
                CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
            Next
        Next
        'If LifeComputation Then
        '    LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
        '    If LifeCounterForPCRRuns = 1 Then
        '        For k1 = 1 To NAC + 1
        '            For k2 = 1 To 41
        '                CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
        '            Next
        '        Next
        '    End If
        'End If


    End Sub


    Public Sub AdjustGrossWeight2017()

        If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then
            Call NewAdjustGrossWeightOverlay() 'Review777 GL1

            If DesignType = FlexOnRigid Then
                If AnalyzedasFlexible Then
                    lPavementType = ACRClassLib.clsACR.PavementType.Flexible
                Else
                    lPavementType = ACRClassLib.clsACR.PavementType.Rigid
                End If
            End If
        Else
            Call NewAdjustGrossWeight2() 'Review777 GL1
        End If

    End Sub


    Public Sub Set_targets()

        If SectName = "RunW_A_F-1" Then '1
            CDFPic = 0.0000191146482
        ElseIf SectName = "RunW_B_F-2" Then '2
            CDFPic = 0.0008227169
        ElseIf SectName = "RunW_C_F-3" Then '3
            CDFPic = 0.000004627695
        ElseIf SectName = "RunW_D_F-4" Then '4
            CDFPic = 0.269525
        ElseIf SectName = "RunW_E_F-5" Then '5
            CDFPic = 78.88951
        ElseIf SectName = "RunW_F_F-6" Then '6
            CDFPic = 22.34727
        ElseIf SectName = "RunW_G_F-7" Then '7
            CDFPic = 0.000208526573
        ElseIf SectName = "RunW_H_F-8" Then '8
            CDFPic = 0.027195327
        ElseIf SectName = "RunW_I_R-1" Then '9
            CDFPic = 4195080.0
        ElseIf SectName = "RunW_J_R-2" Then '10
            CDFPic = 3.01121736
        End If



    End Sub



End Module
