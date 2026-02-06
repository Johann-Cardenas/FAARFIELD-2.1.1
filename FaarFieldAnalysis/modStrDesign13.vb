Option Strict On
Option Explicit On

Public Module modStrDesign13
    'for subroutine LeafCDFRigid13
    Dim CDFU(NOFF) As Single
    Dim OFFSET As Single ' Numerical value of current offset, inches.
    Dim CDFCON(NOFF) As Single

    Public Sub SelectACwithCDF005()

        ' *** SelectACwithCDF005() Eliminates aircraft   ***
        ' *** with insignificant CDF contribution        ***
        ' *** to save calculation time with NIKE3D       ***


        Dim I As Integer
        'If (NAC + NextraAC) = 1 Then Exit Sub

        For I = 1 To NAC + CShort(NextraAC)
            If jobCDFtable(ISect, I) / gCDF2max < 0.005 And jobCDFtable(ISect, I) > 0.0 Then
                CalcStress(I) = 1 'exclude AC from 3D-FEM stress calculations
            Else
                CalcStress(I) = 0 'include AC in 3D-FEM stress calculations
            End If
        Next

        ''FileOpen(9, "rrr.txt", OpenMode.Output)
        ''For I = 1 To NAC + CShort(NextraAC)
        ''    PrintLine(9, I & "   " & CalcStress(I, 2))
        ''Next I
        ''FileClose(9)

    End Sub

    Public Sub LeafCDFRigid13(ByRef lclCDFMAX As Single, ByRef Overflow As Boolean, ByRef StressResponse(,) As Double)
        ' Compute CDFs for all lateral offsets on rigid pavement.
        Static IA As Short ' Aircraft index.
        Static IOFF As Short
        Static CPC As Single
        Static I, LI As Short
        Static AA, BB As Double
        Static NtoFail, DesFactor, NtoFailCDFU As Single
        Static LFNo, FileErr As Short
        Static SCITemp As Double
        '  Global FSlope As Single, FSlopeComp As Single  in CDF.BAS Declarations.
        Static GearLoadType() As String
        Static IextraAC, NACpIex As Integer
        Static ACTT() As Double
        Static lclCDF(MaxSectAC, NOFF) As Double
        Static IextraACIndex(MaxSectAC) As Integer
        Static ACIndex As Integer ' GFH 08/13/03.
        Static ACCDF As Double ' GFH 08/13/03. Aircraft CDF.

        SCITemp = 20 'SCI
        SCITemp = SCI

        IextraAC = 0
        For IA = 1 To NAC
            LI = LibIndex(IA)
            If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
                IextraAC = IextraAC + 1
            End If
        Next IA

        ReDim GearLoadType(NAC + IextraAC)
        ReDim ACTT(NAC + IextraAC)

        ' Compensation for edge stress requires knowledge of the gear type.
        ' StressResponse and CtoP have the extra gears from the multiple-
        ' gear aircraft. So create GearLoadType$() and do all gears together.
        ' Get the maximum stress for each gear at the same time.

        IextraAC = 0
        For IA = 1 To NAC
            STRSH(IA) = 0
            LI = LibIndex(IA)
            If AC(LI).libGear = "WFBF" Then ' 4-4 wing-body.
                IextraAC = IextraAC + 1
                IextraACIndex(IextraAC) = IA
                NACpIex = NAC + IextraAC
                GearLoadType(IA) = "F"
                GearLoadType(NACpIex) = "F"
                RepsAnnual(NACpIex) = RepsAnnual(IA) ' Used in Life computation.
                RepsInc(NACpIex) = RepsInc(IA) ' Used in Life computation.
                Reps(NACpIex) = Reps(IA)
                ACTT(IA) = AC(LI).libTT
                ACTT(NACpIex) = ACTT(IA)
                STRSH(NACpIex) = 0
                'For I = 1 To 8
                For I = 1 To CShort(UBound(StressResponse, 2)) 'ikawa 
                    If System.Math.Abs(StressResponse(IA, I)) > STRSH(IA) Then
                        STRSH(IA) = CSng(System.Math.Abs(StressResponse(IA, I)))
                    End If
                    If System.Math.Abs(StressResponse(NACpIex, I)) > STRSH(NACpIex) Then
                        STRSH(NACpIex) = CSng(System.Math.Abs(StressResponse(NACpIex, I)))
                    End If
                Next I
                System.Diagnostics.Debug.WriteLine("STRSH() = " & STRSH(IA) & STRSH(NACpIex))
            ElseIf AC(LI).libGear = "WFBN" Then  ' 4-6 wing-body.
                IextraAC = IextraAC + 1
                IextraACIndex(IextraAC) = IA
                NACpIex = NAC + IextraAC
                GearLoadType(IA) = "F"
                GearLoadType(NACpIex) = "N" 'ikawa
                RepsAnnual(NACpIex) = RepsAnnual(IA)
                RepsInc(NACpIex) = RepsInc(IA)
                Reps(NACpIex) = Reps(IA)
                ACTT(IA) = AC(LI).libTT
                ACTT(NACpIex) = ACTT(IA)
                STRSH(NACpIex) = 0
                'For I = 1 To 8
                For I = 1 To CShort(UBound(StressResponse, 2)) 'ikawa 
                    If System.Math.Abs(StressResponse(IA, I)) > STRSH(IA) Then
                        STRSH(IA) = CSng(System.Math.Abs(StressResponse(IA, I)))
                    End If
                    If System.Math.Abs(StressResponse(NACpIex, I)) > STRSH(NACpIex) Then
                        STRSH(NACpIex) = CSng(System.Math.Abs(StressResponse(NACpIex, I)))
                    End If
                Next I
            Else
                GearLoadType(IA) = AC(LI).libGear
                ACTT(IA) = AC(LI).libTT
                'For I = 1 To AC(LI).libNEVPTS
                For I = 1 To CShort(UBound(StressResponse, 2)) 'ikawa 
                    If System.Math.Abs(StressResponse(IA, I)) > STRSH(IA) Then
                        STRSH(IA) = CSng(System.Math.Abs(StressResponse(IA, I)))
                    End If
                Next I
            End If
        Next IA

        For IA = 1 To CShort(NAC + IextraAC)
            System.Diagnostics.Debug.Write("STRSH " & IA & GearLoadType(IA) & " Interior = " & STRSH(IA))
            'Call LeafInteriorToEdge(IA, GearLoadType(IA), ACTT(IA), STRSH(IA)) ' Compensate for edge stresses.
            System.Diagnostics.Debug.WriteLine(" Edge = " & STRSH(IA))
        Next IA
        FileClose(LFNo)

        If Not OverlayRigOnRig Then 'And Not (LayerType$(Lcode(1)) = NPCC$ And LayerType$(Lcode(2)) = NND$) Then
            'GuoReplace  If Not OverlayRigOnRig Then
            Call CompforStab(FSlope, 2) ' Increase N-to-failure for stabilized base.
        Else
            'GUO1210    FSlope = 1!  ' No change in stabilized N.
            Call CompforStab(FSlope, 3) ' Increase N-to-failure for stabilized base.(for new PCC Design)
        End If

        For IOFF = 1 To NOFF
            CDFCON(IOFF) = 0.0!
            CDFU(IOFF) = 0.0!
        Next IOFF
        'FSlope = 1
        Overflow = True ' All aircraft must overflow to return True.
        FSlopeComp = CSng((0.392 - FSlope * 0.3881) / (FSlope * 0.0039))

        LEDFAA13Stress = STRSH(1)

        For IA = 1 To CShort(NAC + IextraAC)
            DesFactor = CSng(julRCon / (STRSH(IA) + 0.000000000001))
            AA = DesFactor - 0.2967 - 0.002267 * SCITemp
            BB = FSlope * (0.3881 + 0.000039 * SCITemp * FSlopeComp)
            Temp = CSng(AA / BB)
            LogCA2(IA) = Temp ' GuoAdd
            If Temp > 10.0! Then Temp = 10.0! Else Overflow = False
            NtoFail = CSng(10.0! ^ Temp)
            '    Debug.Print NtoFail; DesFactor; ((DesFactor - 0.5236) / 0.392)
            If ((DesFactor - 0.5236) / 0.392) > 10.0! Then
                NtoFailCDFU = NtoFail
            Else
                NtoFailCDFU = CSng(10.0! ^ ((DesFactor - 0.5236) / 0.392))
            End If

            If IA <= NAC Then ' Report maximum CDF and CtoP for multiple gear aircraft. GFH 08/13/03.
                jobCDFacrftMaxtable(ISect, IA) = 0 ' GFH 08/04/03. Global variables, so must be reset.
                jobCtoPtable(ISect, IA) = 0 ' GFH 08/04/03.
            End If
            OFFSET = 0.0!
            For IOFF = 1 To NOFF
                CPC = CtoP(IA, IOFF) ' See LeafCtoPRigid. CtoP is set for extra aircraft in LeafCtoPRigid.
                ACCDF = CPC * Reps(IA) / NtoFail ' Aircraft CDF.
                CDFCON(IOFF) = CSng(CDFCON(IOFF) + ACCDF) ' CDF for design.
                CDFU(IOFF) = CDFU(IOFF) + CPC * Reps(IA) / NtoFailCDFU ' CDF used. Report for overlay design.

                '     > GFH 08/13/03
                If IA <= NAC Then ' GFH 08/13/03
                    lclCDF(IA, IOFF) = ACCDF ' Save for aircraft table entries.
                    If lclCDF(IA, IOFF) > jobCDFacrftMaxtable(ISect, IA) Then jobCDFacrftMaxtable(ISect, IA) = lclCDF(IA, IOFF) ' GFH 08/10/03.
                    If jobCtoPtable(ISect, IA) < CPC Then jobCtoPtable(ISect, IA) = CPC
                Else
                    ACIndex = IextraACIndex(IA - NAC)
                    '        Sum CDF over gears for multiple gear aircraft.
                    lclCDF(ACIndex, IOFF) = lclCDF(ACIndex, IOFF) + ACCDF
                    If lclCDF(ACIndex, IOFF) > jobCDFacrftMaxtable(ISect, ACIndex) Then jobCDFacrftMaxtable(ISect, ACIndex) = lclCDF(ACIndex, IOFF) ' GFH 08/10/03.
                    '        Report maximum CtoP for multiple gear aircraft.
                    If jobCtoPtable(ISect, ACIndex) < CPC Then jobCtoPtable(ISect, ACIndex) = CPC
                End If
                '     GFH 08/13/03 <

                OFFSET = OFFSET + OFFSETINC
            Next IOFF
        Next IA

        lclCDFMAX = 0.0! : PercentCdfu = 0.0! : gCDF2max = 0
        For IOFF = 1 To NOFF
            If lclCDFMAX < CDFCON(IOFF) Then
                lclCDFMAX = CDFCON(IOFF)
                IControl = IOFF
                gCDF2max = CDFCON(IOFF) 'ikawa added line September 02, 2005
            End If
            If PercentCdfu < CDFU(IOFF) Then PercentCdfu = CDFU(IOFF)
        Next IOFF
        PercentCdfu = PercentCdfu * 100.0!

        For IA = 1 To NAC 'ikawa999
            gCDF(IA) = CSng(lclCDF(IA, IControl))
        Next


        '  Debug.Print IControl; NtoFail; lclCDFMAX
        '  Debug.Print FSlope; NtoFail; lclCDFMAX; NtoFailCDFU; PcntCDFU

        ' > Jia 08/04/03
        For IA = 1 To NAC
            jobCDFtable(ISect, IA) = lclCDF(IA, IControl)
        Next IA
        ' Jia 08/04/03 <


        For IA = 1 To CShort(NAC + NextraAC) 'ikawa555
            gCDF(IA) = CSng(lclCDF(IA, IControl))

            If gCDF(IA) / lclCDFMAX < 0.001 Then
                'ikawa CalcStress(IA, 2) = 1
            End If
        Next


        'commented Jan 04, 2004
        PrintResults13 = False
        If PrintResults13 Then
            'Dim J As Integer
            Dim TimeFil As Integer
            TimeFil = FreeFile()
            FileOpen(TimeFil, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
            PrintLine(TimeFil, "LeafCDFRig13 " & LPad(9, "Aircraft") & LPad(10, "Gear_Load") _
                  & LPad(10, "Tire_Load") & LPad(15, "Job") & LPad(9, "Section") _
                  & LPad(12, "CDF") _
                  & LPad(10, "Stress") _
                  & LPad(10, CStr(Math.Round(LEAStructure.Thick(1), 4))) & LPad(24, CStr(Now)))
            For J = 1 To NAC + CShort(NextraAC)
                'If Not RunBatchLife Then
                PrintLine(TimeFil, LPad(3, CStr(J)) & LPad(19, CallAC(J).ACname) _
                & LPad(10, CallAC(J).GearLoad.ToString("#,##0,000")) _
                & LPad(10, (CallAC(J).GearLoad / CallAC(J).NTires).ToString("#,##0,000")) _
                & LPad(15, JobName) & LPad(9, SectName) _
                & LPad(12, gCDF(J).ToString("##,##0.00000")) _
                & LPad(10, STRSH(J).ToString("#,##0.000")))
                'End If
            Next J
            PrintLine(TimeFil, StrDup(46, " "), LPad(22, Format(lclCDFMAX, "##,##0.00000")))
            PrintLine(TimeFil, StrDup(103, "-"))
            FileClose(TimeFil)
        End If


        If DesigningStr And CBool(FileErr) Then
            DesigningStr = False
            S = "There has been an error reading" & NL
            S = S & "stresses from the data file. This" & NL
            S = S & "was probably caused by the PCC" & NL
            S = S & "layer being too thin. The design" & NL
            S = S & "will be ended." & NL2
            S = S & "Please check the structure."
            Ret = MsgBoxDQ(S, 0, "File Error")
        End If

    End Sub

End Module

