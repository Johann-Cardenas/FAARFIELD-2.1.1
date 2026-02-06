
Imports System.Threading
Imports System.Windows.Forms

Public Module modOVERLAY_NP

    Public CCC As Integer
    Public gNSectionUsed As Integer
    Public gPCC_Surface_Mod(NSection) As Single
    Public gPCC_SCI(NSection) As Single
    Public gPCC_NIKE3D_Stress(MaxSectAC) As Single
    Public gPCC_LEAF_Stress(MaxSectAC) As Single
    'Public gPCC_USED_Stress(MaxSectAC) As Single

    Public gHorStressLEAF() As Double 'ikawa 2017.08.14
    Public gHorStressNIKE3D() As Double
    Public gLifeToB, gLifeToE As Single

    Public gOverlay_NIKE3D_Stress(NSection, MaxSectAC) As Single
    Public gOverlay_LEAF_Stress(NSection, MaxSectAC) As Single
    Public gOverlay_USED_Stress(NSection, MaxSectAC) As Single

    Public Aircraft_100t_lbs As Boolean
    Public bPartBondonRigid As Boolean = False
    'Const gCalibrationFactor As Single = 1.0 'YGC 102811
    Public gCalibrationFactor As Single = 1.0 '2013.01.03 YGC 060713
    'Const gCalibrationFactor As Single = 1.0
    Dim LEAFOutputText As String
    Dim CDFCON(NOFF) As Single
    Dim CDFU(NOFF) As Single
    Dim OFFSET As Single ' N
    Public HorizStressResponse1(,) As Double    'modified to output both stress by LEAF and NIKE3D by YGC 101613
    Public HorizStressResponse2(,) As Double    'modified to output both stress by LEAF and NIKE3D by YGC 101613
    Public HorizStressResponse3(,) As Double    'modified to output both stress by LEAF and NIKE3D for overlay by YGC 102213
    Public VertStress(,), VertCoord() As Double 'modified to transfer vertical stress for rigid compaction by YGC 112213 
    Public StressTypeRigid() As String 'added for rigid comapction by YGC 112213  


    Public Sub DesignRigidOverlay_NP(ByRef Optional ct As CancellationToken = Nothing)
        'was(LeafDesignRigidOverlay_NP())

        ' SCIB = Initial SCI of the Existing Pavement.
        ' LifeE = Percentage of existing pavement life consumed when overlay is applied.

        Dim tth2, Th1, Th2, Thick0 As Single
        Dim I As Short
        Dim YA As Single
        Dim yB As Single
        Dim AggErr As Boolean
        Dim AA, BB As Single
        ' Dim Overflow as Integer was moved to overlay.bas as GLOBAL
        Dim B As Single
        Static RT As Single
        Static DS(NOFF) As Single
        Dim NextraACin As Short
        Dim TimeSave1 As Integer

        Dim OverlayLife111 As Single 'ik2020.10
        Dim Thick111 As Single 'ik2020.10

        FlexonRigidChecker = True

        If LifeComputation Then
        Else '2013.01.03
            SMin = CStr(NullDate)
        End If


        Debug.WriteLine("------------------------------------------")
        Debug.WriteLine("     NEW RESULTS STARTED FROM HERE        ")
        Debug.WriteLine("------------------------------------------")

        If SCIB < 100 Then LifeExistPCC = 100.0!
        Ijk = 0 : SCIE = 80.0! : DThick = 2 'DefaultNsection = 16


        S = LayerType(LCode(1))
        If S = NACO Or S = NND Then
            S = LayerType(LCode(3))
            ''If S = NAgBase Or S = NAgSBase Then
            ''    SCIE = 58.0!
            ''Else
            ''    SCIE = 40.0!
            ''End If

            'Public Const NAgBase As String = "Ag Crushed"
            'Public Const NAgSBase As String = "Ag Uncrushed"
            'Public Const NStBase As String = "St (Rigid)"

            If S = NStBase Then
                SCIE = 40.0!
            Else
                SCIE = 57.0!
            End If

        End If


        LayerSwitch = True ' Required for WESModulus, always sublayer.
        AA = 5.0! : BB = 10.0! : CDFErr = 10.0!

        '   Sublayers = False
        T1min = 0.0! : T2min = 0.0! : RT = 0.0!
        OverlayLife = T1min + T2min

        S = LayerType(LCode(1))
        If S = NACO Or S = NND Then
            tth2 = ThickMin(10)
        ElseIf S = NPCCOU Then
            tth2 = ThickMin(11)
        Else
            tth2 = ThickMin(12)
        End If
        '   Calculating Coverage-To-Pass ratio

        NEvalDepths = 1 ' Uncommented, GFH 10/27/94.
        EvalDepth(1) = -Thick(1)

        Call UpdateCurrentSectData()
        'Call WESModulus(AggErr, SubLayers)
        Call FAAModulus(AggErr, SubLayers)

        'Call frmStructure.DefInstance.DrawStructure()
        'frmStructure.DefInstance.picStructure.Refresh()

        'Call LeafCtoPRigid() ' Leaves results in CtoP(IA, IOFF), used in LifeTotal.
        'Call CoverageToPassRigid() 'DesignRigidOverlay_NP
        Call OneCallToCalcCtoP_RigidType() 'DesignRigidOverlay_NP

        Call WriteFile()

        NStrIterations = 0
        Thick0 = Thick(1)
        T1min = 0.0! : T2min = 0.0! : RT = 0.0!

750:
        I = 0
        Th2 = Thick(1)

        GoTo skip2
        '---------------------------------------------------------------------

        Dim Interval As Microsoft.VisualBasic.DateInterval
        Dim DayOfWeek As Microsoft.VisualBasic.FirstDayOfWeek
        Dim WeekOfYear As Microsoft.VisualBasic.FirstWeekOfYear
        Interval = DateInterval.Second
        DayOfWeek = FirstDayOfWeek.Sunday
        WeekOfYear = FirstWeekOfYear.Jan1

        Dim tStart1, tStart2, tStart3 As Date
        Dim tPart1, tPart2 As Date, sumPart1, sumPart2 As Single
        Dim tdiff, tdiffPart1, tdiffPart2 As Long

        LifeExistPCC = 50 : SCIB = 100 : sumPart1 = 0
        LifeExistPCC = 100 : SCIB = 40 : sumPart2 = 0

        Dim QQQ1(1, 1), WWW1(1, 1) As Single
        Dim count1, c33 As Single
        Dim PCCthick, OverThick As Single
        Dim uuu1 As String

        tStart3 = Now()

        For PCCthick = 8 To 8 Step 3 '6-24          6,9,12,15,18,21,24
            tStart1 = Now()
            sumPart1 = 0 : sumPart2 = 0

            For OverThick = 19 To 19 Step 3 '5-23    5,8,11,14,17,20,23
                Thick(1) = OverThick
                Th2 = Thick(1)
                Thick(2) = PCCthick
                uuu1 = "O" & CStr(Thick(1)) & "_S" & CStr(Thick(2))
                '>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                'Thick(1) = 5 : Thick(2) = 6 'check for LEAF stresses
                SCIB = 100

                tPart1 = Now()
                For count1 = 10 To 100 Step 5 '%CDFU 10 to 100
                    If OverThick = 5 And count1 = 10 Then count1 = 26
                    tStart2 = Now()
                    LifeExistPCC = count1
                    'Call frmStructure.DefInstance.DrawStructure()
                    'frmStructure.DefInstance.picStructure.Refresh()
                    B = LifeTotal_SelectFunction(Th2, NextraACin, ct)
                    tdiff = DateDiff(Interval, tStart2, Now(), DayOfWeek, WeekOfYear)
                    'FileOpen(13, WorkingDir & "\CDFU_001_" & uuu1 & ".txt", OpenMode.Append, , , 1024)
                    'Print(13, LPad(7, Format(count1, "##,##0")))
                    'Print(13, LPad(15, Format(T1min, "##,##0.0000")))
                    'Print(13, LPad(15, Format(T2min, "##,##0.0000")))
                    'Print(13, LPad(5, Format(CCC, "##,##0")))
                    'PrintLine(13, LPad(12, Format(tdiff, "##,##0.00")))
                    'FileClose(13)
                Next
                tdiffPart1 = DateDiff(Interval, tPart1, Now(), DayOfWeek, WeekOfYear)
                sumPart1 = sumPart1 + tdiffPart1

                tPart2 = Now()
                LifeExistPCC = 100 : c33 = 100
                For count1 = 99 To 40 Step -2 'SCI=99 to 40
                    tStart2 = Now()
                    SCIB = count1

                    'Call frmStructure.DefInstance.DrawStructure()
                    'frmStructure.DefInstance.picStructure.Refresh()
                    B = LifeTotal_SelectFunction(Th2, NextraACin, ct)
                    c33 = c33 + 1
                    tdiff = DateDiff(Interval, tStart2, Now(), DayOfWeek, WeekOfYear)
                    'FileOpen(13, WorkingDir & "\CDFU_001_" & uuu1 & ".txt", OpenMode.Append, , , 1024)
                    'Print(13, LPad(7, Format(c33, "##,##0")))
                    'Print(13, LPad(15, Format(T1min, "##,##0.0000")))
                    'Print(13, LPad(15, Format(T2min, "##,##0.0000")))
                    'Print(13, LPad(5, Format(CCC, "##,##0")))
                    'PrintLine(13, LPad(12, Format(tdiff, "##,##0.00")))
                    'FileClose(13)
                Next

                '>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
                tdiffPart2 = DateDiff(Interval, tPart2, Now(), DayOfWeek, WeekOfYear)
                sumPart2 = sumPart2 + tdiffPart2

                tdiff = DateDiff(Interval, tPart1, Now(), DayOfWeek, WeekOfYear)
                'FileOpen(13, WorkingDir & "\Summary.txt", OpenMode.Append, , , 1024)
                'Print(13, LPad(14, "OverLay" & OverThick & " PCC" & CStr(PCCthick)))
                'Print(13, LPad(14, Format(tdiffPart1 / 60, "##,##0.00")))
                'Print(13, LPad(14, Format(tdiffPart2 / 60, "##,##0.00")))
                'PrintLine(13, LPad(14, Format(tdiff / 60, "##,##0.00")) & " min.")
                'FileClose(13)
            Next OverThick

            tdiffPart1 = DateDiff(Interval, tPart1, Now(), DayOfWeek, WeekOfYear)
            tdiffPart2 = DateDiff(Interval, tPart2, Now(), DayOfWeek, WeekOfYear)
            tdiff = DateDiff(Interval, tStart1, Now(), DayOfWeek, WeekOfYear)
            'FileOpen(13, WorkingDir & "\Summary.txt", OpenMode.Append, , , 1024)
            'Print(13, LPad(14, "PCC" & CStr(PCCthick)))
            'Print(13, LPad(14, Format(sumPart1 / 60, "##,##0.00")))
            'Print(13, LPad(14, Format(sumPart2 / 60, "##,##0.00")))
            'PrintLine(13, LPad(14, Format(tdiff / 60, "##,##0.00")) & " min.")
            'PrintLine(13, "")
            'FileClose(13)

        Next PCCthick

        tdiff = DateDiff(Interval, tStart3, Now(), DayOfWeek, WeekOfYear)
        'FileOpen(13, WorkingDir & "\Summary.txt", OpenMode.Append, , , 1024)
        'PrintLine(13, "")
        'Print(13, LPad(14, ""))
        'Print(13, LPad(28, ""))
        'PrintLine(13, LPad(14, Format(tdiff / 60, "##,##0.00")) & " min.")
        'Print(13, LPad(14, ""))
        'Print(13, LPad(28, ""))
        'PrintLine(13, LPad(14, Format(tdiff / 60 / 60, "##,##0.00")) & " hours")
        'PrintLine(13, "")
        'FileClose(13)

        Exit Sub


        '---------------------------------------------------------------------

skip2:

        B = LifeTotal_SelectFunction(Th2, NextraACin, ct) '(1)
        'Exit Sub 'comment_ik

        If DesigningStr = False Then GoTo BeepandEnd 'ikawa
        OverlayLife = T1min + T2min

        OverlayLife111 = OverlayLife 'ik2020.10
        Thick111 = Thick(1)


        S = LayerType(LCode(1))
        'If S = NACO Or S = NND Then OverlayLife = OverlayLife + Th2 / CrackGrowthRate

        'frmStructure.DefInstance.picStructure.Refresh()
        If LifeComputation Then GoTo BeepandEnd

        NStrIterations = NStrIterations + 1S
        'Call frmStructure.DefInstance.DrawStructure()

        Debug.WriteLine(" LifeTotal: 750 in LeafDesignRigidOverlay")
        Debug.WriteLine("B =   " & B & "  Thick(1)=" & Thick(1) & "   AA= " & AA)
        Debug.WriteLine("Th2 = " & Th2 & "  tth2=" & tth2 & OverlayLife & Life)

        '-----------------------------------------------------------------------
        If DesignType = FlexOnRigid Then

            'If gHMAonRigid_Mod Then 'FlexOnRigid module =DesignRigidOverlay_NP()
            '    If Not LifeComputation Then 'kairat 9
            '        If Th2 < ttresh Then ' HMA-on-Rigid is less than HMAonFlex
            '            If Th2 > 2 Then
            '                HMAonRigidCase = 1 'Case (A)
            '            Else
            '                HMAonRigidCase = 2 'Case (B)
            '            End If
            '        Else ' HMA-on-Rigid is >= than HMAonFlex
            '            If ttresh > Thick(2) Then  ' HMAonFlex > PCC thickness
            '                Thick(1) = ttresh ':GoTo 1503
            '                MinThickReached = True 'izydor 2015
            '                HMAonRigidCase = 3 'Case (C)
            '                'jobCDFtable = jobCDFtableSub 'kairat  10
            '                'jobCDFacrftMaxtable = jobCDFacrftMaxtableSub 'kairat 10

            '                Call copyArray1(jobCDFtableSub, jobCDFtable)
            '                Call copyArray1(jobCDFacrftMaxtableSub, jobCDFacrftMaxtable)
            '                Call copyArray1(jobCtoPtableSub, jobCtoPtable)

            '                GoTo 1503
            '            Else ' HMAonFlex = < PCC thickness
            '                Thick(1) = Thick(2) 'kairat
            '                HMAonRigidCase = 4 'Case (D)
            '            End If
            '        End If
            '    End If
            'End If


            'If gHMAonRigid_Mod Then 'FlexOnRigid module
            '    'If Ijk > 0 And Not LifeComputation Then 'kairat 9
            '    If (Ijk > 0 Or System.Math.Abs(B) < LifeError) And Not LifeComputation Then 'kairat 9

            '        If Th2 < 2 Then
            '            HMAonRigidCase = 2 'Case (B)
            '        Else
            '            If OverlayLife <= Life + LifeError And Th2 > ttresh Then
            '                Thick(1) = ttresh ':GoTo 1503
            '                MinThickReached = True 'izydor 2015
            '                HMAonRigidCase = 3 'Case (C)
            '                jobCDFtable = jobCDFtableSub 'kairat  10
            '                jobCDFacrftMaxtable = jobCDFacrftMaxtableSub 'kairat 10
            '                GoTo 1503
            '            Else
            '                HMAonRigidCase = 4 'Case (D)
            '            End If
            '        End If
            '    End If
            'End If

        End If
        '-----------------------------------------------------------------------

        If System.Math.Abs(B) < LifeError Then GoTo 1503
        If NStrIterations > 12 Then
            GoTo 1503
        End If
        If B > 0 Then
            yB = B
            'If Abs(B) < LifeError Or Abs(Thick(1) - AA) < Thickerror Or _
            '(Th2 = tth2 And OverlayLife > Life) Then
            ' GFH 4/29/95
            If Math.Abs(B) < LifeError Or (Th2 = tth2 And OverlayLife > Life) Then
                ' GFH Required if start AA is too close to Thick(1).
                If Not (NStrIterations = 0 And System.Math.Abs(Thick(1) - AA) < ThickError) Then
                    RT = Thick(1)
                    GoTo 1502
                End If
            End If
            If Ijk <> 0 Then
                BB = Thick(1)
                Call Interpolation(AA, BB, YA + Life, yB + Life, Thick(1))
                GoTo 750
            End If
            Do
                I = I + 1S
                Th1 = Th2

                OverlayLife111 = OverlayLife
                Thick111 = Th1


                'If OverlayLife > 30 Then
                'Call AdjustOverlayThickness(I, Th1, Th2, OverlayLife)
                Call AdjustOverlayThickness2020(I, Th1, Th2, OverlayLife111, OverlayLife)
                'Th2 = Th1 - DThick

                ' GUO    --------------- June 18, 1994  ----------------
                If Th2 < tth2 Then Th2 = tth2
                ' GUO    -----------------------------------------------
                julThick(1) = Th2
                EvalDepth(1) = -julThick(1) ' 4/20/97
                yB = B
                Thick(1) = Th2

                B = LifeTotal_SelectFunction(Th2, NextraACin, ct)  '(2)

                If DesigningStr = False Then GoTo BeepandEnd ' GFH 04/10/03.
                OverlayLife = T1min + T2min
                S = LayerType(LCode(1))
                If S = NACO Or S = NND Then
                    'OverlayLife = OverlayLife + Th2 / CrackGrowthRate
                    OverlayLife = OverlayLife
                End If

                NStrIterations = NStrIterations + 1S
                ' Call frmStructure.DefInstance.DrawStructure()
                'frmStructure.DefInstance.picStructure.Refresh()
                If Not DesigningStr Then GoTo BeepandEnd
                If (Th2 = tth2 And OverlayLife > Life) Then GoTo 1503
                If I > Ncontrol Then GoTo 1301 'Not convergent 'ikawa added line
            Loop Until B < 0
            AA = Th2
            BB = Th1
            YA = B
        Else
            YA = B
            If System.Math.Abs(B) < LifeError Then ' GFH 4/29/95 Or Abs(AA - BB) < Thickerror Then
                RT = Thick(1)
                GoTo 1502
            End If
            If Ijk <> 0 Then
                AA = Thick(1)
                Call Interpolation(AA, BB, YA + Life, yB + Life, Thick(1))
                GoTo 750
            End If
            Do
                I = I + 1S
                Th1 = Th2

                'If OverlayLife < 10 Then
                'Call AdjustOverlayThickness(I, Th1, Th2, OverlayLife)
                Call AdjustOverlayThickness2020(I, Th1, Th2, OverlayLife111, OverlayLife)
                'Th2 = Th1 + DThick

                'check1000
                julThick(1) = Th2
                EvalDepth(1) = -julThick(1) ' 4/20/97
                YA = B
                Call WriteFile()
                Thick(1) = Th2

                B = LifeTotal_SelectFunction(Th2, NextraACin, ct) '(3)

                If DesigningStr = False Then GoTo BeepandEnd ' GFH 04/10/03.
                OverlayLife = T1min + T2min

                S = LayerType(LCode(1))
                'If S = NACO Or S = NND Then OverlayLife = OverlayLife + Th2 / CrackGrowthRate
                NStrIterations = NStrIterations + 1S
                'Call frmStructure.DefInstance.DrawStructure()
                ' frmStructure.DefInstance.picStructure.Refresh()
                If I > Ncontrol Then GoTo 1301 'Not convergent
            Loop Until B > 0
            AA = Th1
            BB = Th2
            yB = B
        End If
        Ijk = 1
        Call Interpolation(AA, BB, YA + Life, yB + Life, Thick(1))
        GoTo 750
1301:
        Debug.WriteLine("The design has not converged after Ncontrol iterations")
        S = "The design has not converged after" & NL
        S = S & Format(Ncontrol, "0") & " iterations." & NL2
        S = S & "Terminating the design."
        ' If Not BatchMode Then
        'Call frmStructure.DefInstance.BeepTmrMsg(S, "Overlay on Rigid Design")
        ' End If
        GoTo BeepandEnd

1503:
        RT = Th2
1502:
        OverlayLife = T1min + T2min
        S = LayerType(LCode(1))
        'If S = NACO Or S = NND Then OverlayLife = OverlayLife + RT / CrackGrowthRate
        Debug.WriteLine(" The Designed Thickness of Overlay =  " & RT)
        Debug.WriteLine(" The Design Life = " & OverlayLife)

        If RT = tth2 Then
            S = "Minimum thickness has been reached." & NL
            S = S & "The calculated life is still greater than" & NL
            S = S & "the required design life." & NL2
            S = S & "The design has been terminated."

            Designed = Now '2013.01.03
            SMin = "Min" '2013.01.03

            'If BatchMode Then

            'Else
            '    Call frmStructure.DefInstance.BeepTmrMsg(S, "Overlay on Rigid Design")
            'End If

            'If Not BatchRunning Then 'ikawa added
            '    Call frmStructure.DefInstance.BeepTmrMsg(S, "Overlay on Rigid Design")
            'End If

            GoTo BeepandEnd
        End If


BeepandEnd:



        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
        If Not LifeComputation Then
            Dim k1, k2, k3 As Integer

            If DesignType = FlexOnRigid Then

            Else 'ik2020.06.22
                For k1 = 1 To NAC + 1
                    For k2 = 1 To 41
                        CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                    Next
                Next

            End If


        End If

        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf


        If NoOutFiles = False Then
            ' Write response data to file.
            TimeSave1 = timeGetTime
            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            Call WriteLEAFData(LEAFOutputText)
            SS = WorkingDir & "FAASR3DPCC.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFOutputText)
            FileClose((I))
        End If

        'added for rigid compaction by YGC 112213
        If LifeComputation And gUseCompaction Then 'ikawa 2013
            Call WriteLEAFCompactionData(LEAFCompactionText)

            ' CompactionDesigned = Now    'added to record time for compaction design by YGC 102213

            SS = WorkingDir & "Compaction.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFCompactionText)
            FileClose((I))
        End If
        'added for rigid compaction by YGC 112213 END


        ' If Not Beeped Then Beep ' GFH 04/23/03
        ' moved to frmStructure.cmdAddDelete.

    End Sub




    Public Sub DesignRigid_NP(ByRef Optional ct As CancellationToken = Nothing)

        Static IL, I, ILoop As Short
        Static TM1 As Single
        Static CDFM1, DELT, DELCDF, LifeM1 As Single
        Static Overflow, AggErr As Boolean
        Static PcntCDFUTemp, CDFMAX As Single
        Static TimeSave1 As Integer
        Static HorizStressResponse(,) As Double
        'HorizStressResponse1(,) As Double    NIKE3D stress YGC 101613
        'HorizStressResponse2(,) As Double    LEAF stress   YGC 101613
        gHorStressNIKE3D = Nothing 'ikawa 2017.08.15
        gHorStressLEAF = Nothing 'ikawa 2017.08.15

        '
        ' Static CDFtableTemp(MaxSectAC), CDFacrftMaxtableTemp(MaxSectAC) As Double ' GFH 08/13/03.
        Dim RunAM As New AMClassLib.clsAM()       'Instance of AutoMesh.
        Dim maxLoop As Integer = 5


        Dim LeafResp As LEAFClassLib.clsLEAF.LEAFoptions
        LeafResp = LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress


        'FileOpen(20, "FEDFAAList.dat", OpenMode.Append)
        'PrintLine(20, LPad(24, CStr(Now)))
        'PrintLine(20, "")
        'PrintLine(20, LPad(4, "###") & _
        '          LPad(21, "libACName") & _
        '          LPad(10, "TG"))
        'FileClose(20)


        'If gRigidCalibrationNewValue Then

        'Else
        '    If Modulus(NPLayers) >= 9000 Then
        '        gCalibrationFactor = 1
        '    Else
        '        gCalibrationFactor = CSng(0.7 + 0.0001! * Modulus(NPLayers) / 3)
        '    End If
        'End If



        Try

            ' C/P does not depend on depth for rigid. Therefore call only once.
            'Call LeafCtoPRigid() ' Leaves results in CtoP(IA, IOFF), used in CDFRigid.

            ' GoTo old_method
            If LifeComputation Then '2013.01.03
            Else
                SMin = CStr(NullDate)
            End If


            Dim IA As Short, extra As Short
            extra = 0

            Call UpdateCurrentSectData()

            Call OneCallToCalcCtoP_RigidType()
old_method:
            'Call CoverageToPassRigid() 'DesignRigid_NP (modFAILURE_MODEL_NP.vb)

            Debug.WriteLine("_____LeafDesignRigid_NP_____")
            Debug.WriteLine("Start at " & TimeString)

            Beeped = False : SubLayers = False : LayerSwitch = False
            If LifeComputation Then
                LayerSwitch = True : LifeStr = 0.0! : PercentCdfu = 0.0!
            End If




            CDFErr = 10.0! : ILoop = 0 : IL = 1
            NStrIterations = ILoop
            Call UpdateCurrentSectData() ' To display correct
            'Call WESModulus(AggErr, SubLayers) ' sublayer information.
            Call FAAModulus(AggErr, SubLayers)

            Do
StartRigidLoop:
                NStrIterations = ILoop
                ILoop = ILoop + 1S

                If ILoop > maxLoop Then 'ikawa 12/01/03
                    Exit Do
                End If

                Call UpdateCurrentSectData()
                'Call WESModulus(AggErr, SubLayers)
                Call FAAModulus(AggErr, SubLayers)

                If AggErr = True Then
                    S = "Error computing aggregate Modulus" & NL
                    S = S & "The design has been aborted"
                    ErrorMessageForUser(S, "Rigid Pavement Design")
                    Exit Do
                End If
                Call WriteFile()
                Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
                TimeSave1 = timeGetTime


                'GoTo here123
                Call Set_to_FEM_Structure()

                t1Thick = Now()
                RunAM = New AMClassLib.clsAM()

                Call Set_to_FEM_Structure()


                If DesignType = PCCOnFlex And (NAC + NextraAC) < 1 Then
                    'If True Then
                    Dim j1 As Integer
                    ReDim HorizStressResponse(NAC + NextraAC, 2)
                    Dim OneAC(1) As LEAFClassLib.clsLEAF.LEAFACParms
                    Dim Resp1(1, 1) As Double

                    For j1 = 1 To NAC + NextraAC
                        If CalcStress(j1) = 0 Then
                            Call OneAircraft(CallAC(j1), OneAC(1))
                            RunAM = New AMClassLib.clsAM()
                            Dim CS(1) As Integer : CS(1) = 0
                            Call RunAM.ComputeResponse(LeafResp, CShort(1), OneAC, LEAStrActiveX, Resp1,
                                 VertStress, VertCoord, AllResp, CS, DesignType, gSolverType,
                                 gSlabMeshSize, gOutputDirName, gUserInterrupted, NoOutFiles, ct) 'ik2020.03 JobName -> gOutputDirName DesignRigid_NP
                            If gUserInterrupted Then Exit Sub
                            HorizStressResponse(j1, 1) = Resp1(1, 1)
                        Else
                            HorizStressResponse(j1, 1) = 0.0
                        End If
                        RunAM = Nothing
                        Call FlushMemory1()

                        '------------------------------------------------------------------
                        If InStr(CallAC(j1).ACname, "747", CompareMethod.Text) > 0 _
                        And InStr(CallAC(j1).ACname, "Body", CompareMethod.Text) > 0 Then
                            Dim ACIndex2 As Integer, pos1 As Integer
                            pos1 = InStr(CallAC(j1).ACname, "Body", CompareMethod.Text) - 2
                            For ACIndex2 = 1 To CShort(j1 - 1) 'for all aircraft
                                If Mid(CallAC(j1).ACname, 1, pos1) = Mid(CallAC(ACIndex2).ACname, 1, pos1) _
                                   And CallAC(j1).GearLoad = CallAC(ACIndex2).GearLoad Then
                                    HorizStressResponse(j1, 1) = HorizStressResponse(ACIndex2, 1)
                                End If
                            Next ACIndex2
                        End If
                        '--------------------------------------------------------------------------
                    Next
                Else
                    Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX,
                        HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress, DesignType,
                        gSolverType, gSlabMeshSize, gOutputDirName, gUserInterrupted, NoOutFiles, ct) 'ik2020.03 JobName -> gOutputDirName DesignRigid_NP
                    RunAM = Nothing
                    Call FlushMemory1()


                    Call modDesignRigid_Adj.Set_to_LEAF_Structure() 'ikawa line 700
                    'added for computing larger stress between LEAF and NIKE3D by YGC 092613
                    RunLEAF = New LEAFClassLib.clsLEAF()
                    'Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse, AllResp) 'modified to output both stress by LEAF and NIKE3D by YGC 101613
                    Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse2, AllResp)
                    RunLEAF = Nothing

                    Dim j1 As Integer
                    ReDim HorizStressResponse(NAC + NextraAC, 1) 'added to output both stress by LEAF and NIKE3D by YGC 101613
                    ReDim StressTypeRigid(NAC + NextraAC) 'added for rigid compaction by YGC 112213

                    ReDim gHorStressNIKE3D(NAC + NextraAC) 'ikawa 2017.08.14
                    ReDim gHorStressLEAF(NAC + NextraAC) 'ikawa 2017.08.14

                    For j1 = 1 To NAC + NextraAC
                        'HorizStressResponse2(j1, 1) = 0 'LEAF=0
                        HorizStressResponse2(j1, 1) = HorizStressResponse2(j1, 1) * 0.95 '(1) NP
                        'HorizStressResponse(j1, 1) = Math.Max(HorizStressResponse1(j1, 1), HorizStressResponse(j1, 1))'modified to output both stress by LEAF and NIKE3D by YGC 101613
                        HorizStressResponse(j1, 1) = Math.Max(HorizStressResponse1(j1, 1), HorizStressResponse2(j1, 1))

                        gHorStressNIKE3D(j1) = HorizStressResponse1(j1, 1) 'ikawa 2017.08.14
                        gHorStressLEAF(j1) = HorizStressResponse2(j1, 1) 'ikawa 2017.08.14

                        'added for rigid compaction by YGC 112213
                        If HorizStressResponse1(j1, 1) > HorizStressResponse2(j1, 1) Then
                            StressTypeRigid(j1) = "NIKE3D"
                        Else
                            StressTypeRigid(j1) = "LEAF"
                        End If
                        'added for rigid compaction by YGC 112213 END
                    Next
                    'add ended for computing larger stress between LEAF and NIKE3D by YGC 092613

                    If gUserInterrupted Then Exit Sub
                End If



                'here123:
                '                ReDim HorizStressResponse(1, 1)
                '                HorizStressResponse(1, 1) = 500

                'sssLeaf = CSng(HorizStressResponse(1, 1))
                'sssLeaf2 = HorizStressResponse(2, 1)



                ']]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]
                'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse, AllResp, CalcStress, DesignType, JobName)
                'RunAM = Nothing

                RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                If DesigningStr = False Then Exit Do
                'Call LeafCDFRigid_NP(CDFMAX, Overflow, HorizStressResponse)
                Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(1), 80) '1 qqqqqqq

                'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
                If LifeComputation Then
                    Dim k1, k2, k3 As Integer

                    For k1 = 1 To NAC + 1
                        For k2 = 1 To 41
                            CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                        Next
                    Next
                End If
                If LifeComputation Then
                    LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
                    If LifeCounterForPCRRuns = 1 Then
                        For k1 = 1 To NAC + 1
                            For k2 = 1 To 41
                                CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                            Next
                        Next
                        For K = 1 To NAC ' GFH 08/13/03.
                            CDFtableTemp(K) = jobCDFtable(ISect, K)
                            CDFacrftMaxtableTemp(K) = jobCDFacrftMaxtable(ISect, K)
                            CDFtableTemp3(K) = CDFtableTemp(K)
                            CDFacrftMaxtableTemp3(K) = CDFacrftMaxtableTemp(K)
                        Next
                    End If
                End If

                'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

                If LifeComputation Then
                    LifeM1 = Life
                    CDFPic = CDFMAX
                    CDFM1 = CDFMAX
                    PcntCDFUTemp = PercentCdfu ' Needed for set design life.

                    ' Save from last call to LeafCDFFlex for reporting in Aircraft Table.
                    ' Could multiply final CDFs by (Design Life) / Life but for RepsAnnualInc.
                    For K = 1 To NAC ' GFH 08/13/03.
                        CDFtableTemp(K) = jobCDFtable(ISect, K)
                        CDFacrftMaxtableTemp(K) = jobCDFacrftMaxtable(ISect, K)

                        If FEDFAA1.Save_NAC = 1 And CDFChecker = True Then
                            'CDFtableTemp3(K) = CDFtableTemp(K)
                            'CDFacrftMaxtableTemp3(K) = CDFacrftMaxtableTemp(K)
                            CDFChecker = False
                            'For k1 = 1 To NAC + 1
                            '    For k2 = 1 To 41
                            '        CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                            '    Next
                            'Next
                        ElseIf NAC = FEDFAA1.Save_NAC And FEDFAA1.Save_NAC <> 1 Then
                            'CDFtableTemp3(K) = CDFtableTemp(K)
                            'CDFacrftMaxtableTemp3(K) = CDFacrftMaxtableTemp(K)
                            'For k1 = 1 To NAC + 1
                            '    For k2 = 1 To 41
                            '        CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                            '    Next
                            'Next
                        End If
                    Next K

                    LifeStr = CSng(Life * 1.1)
                    Do
                        For I = 1 To CShort(NAC + NextraAC)
                            Temp1 = LifeStr
                            If 1.0! + RepsInc(I) * LifeStr < 0.0! Then
                                Temp1 = -1.0! / RepsInc(I)
                            End If
                            Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                            Reps(I) = Temp * RepsAnnual(I) * Temp1
                        Next I
                        Overflow = True ' CDF at subgrade.
                        'Call LeafCDFRigid_NP(CDFMAX, Overflow, HorizStressResponse)
                        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(1), 80) '1 qqqqqqq
                        If Overflow Then
                            'S = "PCC stresses are too low" + vbNewLine +
                            'S = S + "to accurately compute life."
                            'MessageBox.Show("PCC stresses are too low" + vbNewLine + "to accurately compute life.")
                            'If Not BatchMode Then
                            '    Ret = MsgBoxDQ(S, 0, "Computing Life")
                            'End If

                            Exit Do
                        ElseIf LifeM1 < 0.0001 Or LifeM1 > 1000000000 Then
                            LifeM1 = 0.0!
                            LifeStr = 0.0!
                            Exit Do
                        End If
                        DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFMAX - CDFM1)
                        Temp = LifeStr
                        LifeStr = LifeM1 + DELT
                        LifeM1 = Temp
                        CDFM1 = CDFMAX
                    Loop Until System.Math.Abs(CDFM1 - 1.0!) < 0.01

                    PercentCdfu = PcntCDFUTemp ' Restore for set design life.
                    If AircraftLifeCDFvsDesignLife Then
                        For I = 1 To NAC ' GFH 08/13/03.
                            '         Saved above before start of Life Do loop.
                            jobCDFtable(ISect, I) = CDFtableTemp(I)
                            jobCDFacrftMaxtable(ISect, I) = CDFacrftMaxtableTemp(I)
                        Next I
                        '     Else uses the CDF values set based on life to failure in last call to LeafCDFRigid.
                    End If

                    ' For I = 1 To NAC: Debug.Print ACName$(I); Reps(I); STRSH(I); TW(I):  Next I
                    ' Debug.Print "Life = "; LifeStr
                    Exit Do
                End If

                CDFPic = CDFMAX

                Debug.WriteLine(ILoop & LayerSwitch & Thick(IL) & EvalDepth(1) & "CDF = " & CDFMAX)
                For I = 1 To CShort(NAC + NextraAC)
                    Debug.WriteLine(CallAC(I).ACname & Reps(I) & STRSH(I)) : Next I
                Debug.WriteLine("")
                CDFErr = CSng(System.Math.Abs(System.Math.Log(CDFMAX)))
                If CDFErr < CDFErrCntrl And SubLayers And Not LayerSwitch Then
                    LayerSwitch = True ' Used in WESModulus and ModulusThick.
                    CDFErr = 10.0! ' Used in ModulusThick. Not really needed here.
                    GoTo StartRigidLoop
                End If
                CDFExitErr = 0.005
                If CDFErr < CDFExitErr Then Exit Do

                If Thick(IL) = ThickMin(LCode(IL)) And CDFMAX < 1.0! Then
                    If NPLayers = 2 Or (DesignType = PCCOnFlex) Then
                        S = "The minimum PCC thickness" & NL
                        S = S & "has been reached. CDF = " & Format(CDFMAX, "0.000") & "."

                        If (DesignType = PCCOnFlex) Then
                            ErrorMessageForUser(S,
                            "Rigid Overlay on Flexible Pavement Design")
                        Else
                            ErrorMessageForUser(S, "Rigid Pavement Design")
                        End If

                        Exit Do
                    Else
                        If Thick(IL + 1) = ThickMin(LCode(IL + 1)) Then
                            If ReducedCrossSectionRunChecker = False Then
                                S = "The minimum layer thicknesses " & NL
                                S = S & "have been reached. CDF = " & Format(CDFMAX, "0.000") & "."
                                ErrorMessageForUser(S, "Rigid Pavement Design")

                                Designed = Now '2013.01.03
                                SMin = "Min" '2013.01.03
                            Else
                                S = "The minimum layer thicknesses " & NL
                                S = S & "for Reduced Cross Section Design " & NL
                                S = S & "have been reached. CDF = " & Format(CDFMAX, "0.000") & "."
                                ErrorMessageForUser(S, "Rigid Pavement Design")

                                Designed = Now '2013.01.03
                                SMin = "Min" '2013.01.03
                            End If


                            Windows.Forms.MessageBox.Show(S)

                                Exit Do
                            End If
                        End If
                End If

                TM1 = Thick(IL)
                CDFM1 = CSng(System.Math.Log(CDFMAX))
                DELT = CSng(Thick(IL) * 0.01)
                If CDFMAX < 1 Then
                    DELT = -DELT
                End If


                If Math.Abs(CDFMAX - 1) < 0.2 Then
                    DELT = CSng(Thick(IL) * (CDFMAX - 1) / 61.8)
                End If

                Thick(IL) = Thick(IL) + DELT
                Call UpdateCurrentSectData()
                'Call WESModulus(AggErr, SubLayers)
                Call FAAModulus(AggErr, SubLayers)

                If AggErr = True Then
                    S = "Error computing aggregate Modulus" & NL
                    S = S & "The design has been aborted"
                    ErrorMessageForUser(S, "Rigid Pavement Design")
                    Exit Do
                End If
                Call WriteFile()

                Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
                TimeSave1 = timeGetTime
                Call Set_to_FEM_Structure()
                t1Thick = Now()

                'RunAM = New AMClassLib.clsAM()
                'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse, AllResp, CalcStress, DesignType, JobName, gUserInterrupted)
                'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse, AllResp, CalcStress, DesignType, gSolverType, JobName, gUserInterrupted) 'added solver choice by YGC 083012
                'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse1, AllResp, CalcStress, DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted) 'added slab mesh size selection by YGC 061113
                'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress, DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted)  'added VertStress, VertCoord for rigid compaction by YGC 112213
                RunAM = New AMClassLib.clsAM()
                Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse1,
                    VertStress, VertCoord, AllResp, CalcStress, DesignType, gSolverType, gSlabMeshSize, JobName,
                    gUserInterrupted, NoOutFiles, ct)  'added VertStress, VertCoord for rigid compaction by YGC 112213
                'ik2020.03 JobName -> gOutputDirName DesignRigid_NP

                RunAM = Nothing
                Call FlushMemory1()

                Call modDesignRigid_Adj.Set_to_LEAF_Structure()

                'added for computing larger stress between LEAF and NIKE3D for design by YGC 101613
                RunLEAF = New LEAFClassLib.clsLEAF()
                Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse2, AllResp)
                RunLEAF = Nothing

                Dim j2 As Integer
                ReDim HorizStressResponse(NAC + NextraAC, 1)
                ReDim StressTypeRigid(NAC + NextraAC) 'added for rigid compaction by YGC 112213

                For j2 = 1 To NAC + NextraAC
                    'HorizStressResponse2(j2, 1) = 0 'LEAF=0
                    HorizStressResponse2(j2, 1) = HorizStressResponse2(j2, 1) * 0.95 '(2) NP
                    HorizStressResponse(j2, 1) = Math.Max(HorizStressResponse1(j2, 1), HorizStressResponse2(j2, 1))

                    gHorStressNIKE3D(j2) = HorizStressResponse1(j2, 1) 'ikawa 2017.08.14
                    gHorStressLEAF(j2) = HorizStressResponse2(j2, 1) 'ikawa 2017.08.14

                    'added for rigid compaction by YGC 112213
                    If HorizStressResponse1(j2, 1) > HorizStressResponse2(j2, 1) Then
                        StressTypeRigid(j2) = "NIKE3D"
                    Else
                        StressTypeRigid(j2) = "LEAF"
                    End If
                    'added for rigid compaction by YGC 112213 END

                Next
                'add ended for computing larger stress between LEAF and NIKE3D for design by YGC 101613


                If gUserInterrupted Then Exit Sub
                'Iter += 1

                RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                If DesigningStr = False Then Exit Do
                'Call LeafCDFRigid_NP(CDFMAX, Overflow, HorizStressResponse)
                Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(1), 80) '1 qqqqqqq
                CDFPic = CDFMAX
                DELCDF = CSng(System.Math.Log(CDFMAX) - CDFM1)
                System.Diagnostics.Debug.WriteLine(ILoop & LayerSwitch & Thick(IL) & EvalDepth(1) & "CDF = " & CDFMAX)
                For I = 1 To NAC : System.Diagnostics.Debug.WriteLine(ACName(I) & Reps(I) & STRSH(I)) : Next I

                If Overflow Then ' CDF not true value for load.
                    Thick(IL) = CSng(Thick(IL) * 0.5)
                    If Thick(IL) < ThickMin(LCode(IL)) Then
                        Thick(IL) = ThickMin(LCode(IL))
                        If NPLayers > 2 And (Not (DesignType = PCCOnFlex)) Then
                            Thick(IL + 1) = CSng(Thick(IL + 1) * 0.5)
                            If Thick(IL + 1) < ThickMin(LCode(IL + 1)) Then
                                Thick(IL + 1) = ThickMin(LCode(IL + 1))
                            End If
                        End If
                    End If
                    GoTo StartRigidLoop
                End If

                'If DELCDF > 0.0! Then
                '    If CDFMAX > 1.0! Then
                '        Thick(IL) = Thick(IL) * 2.0!
                '        GoTo StartRigidLoop
                '    Else
                '        S = "The design procedure cannot converge. In this" & NL
                '        S = S & "case, the cause is that the PCC CDF increases" & NL
                '        S = S & "as PCC thickness increases. It is probably due" & NL
                '        S = S & "to stress increasing with increasing thickness" & NL
                '        S = S & "for a structure with a thin PCC layer on a" & NL
                '        S = S & "stiff subbase." & NL2
                '        S = S & "Please check the structure."
                '        If Not BatchMode Then
                '            ErrorMessageForUser(S, "Cannot Converge")
                '        End If
                '        Exit Do
                '    End If
                'End If

                DELT = -CDFM1 * DELT / DELCDF


                If TM1 + DELT < ThickMin(LCode(IL)) Then

                    If (DELT < 0) And (Math.Abs(DELT) > TM1) Then
                        Thick(1) = CSng(Thick(1) * 0.9)
                    Else
                        Thick(IL) = ThickMin(LCode(IL))

                    End If

                    If NPLayers > 2 And (Not (DesignType = PCCOnFlex)) Then
                        Thick(IL + 1) = CSng(Thick(IL + 1) * 0.5)
                        maxLoop = maxLoop + 5
                        If Thick(IL + 1) < ThickMin(LCode(IL + 1)) Then
                            Thick(IL + 1) = ThickMin(LCode(IL + 1))
                        End If
                    End If
                Else
                    Thick(IL) = TM1 + DELT
                End If

            Loop
            Call Set_to_FEM_Structure() '

            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
            If Not LifeComputation Then
                Dim k1, k2, k3 As Integer

                For k1 = 1 To NAC + 1
                    For k2 = 1 To 41
                        CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                    Next
                Next
            End If
            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

            'NoOutFiles = True
            If OutPutFile Then
                'Write response data to file.
                TimeSave1 = timeGetTime
                RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                Call WriteLEAFData(LEAFOutputText)
                SS = WorkingDir & "FAASR3DPCC.out"
                If Dir(SS) <> "" Then Kill(SS)
                I = CShort(FreeFile())
                FileOpen(I, SS, OpenMode.Output)
                PrintLine(I, LEAFOutputText)
                FileClose((I))
            End If

            'added for rigid compaction by YGC 112213
            If (LifeComputation And gUseCompaction) _
             Or (BatchMode And gUseCompaction) Then 'ikawa 2013 PPPP
                Call WriteLEAFCompactionData(LEAFCompactionText)

                CompactionDesigned(ISect, IJob) = Now    'added to record time for compaction design by YGC 102213

                SS = WorkingDir & "Compaction.out"
                If Dir(SS) <> "" Then Kill(SS)
                I = CShort(FreeFile())
                FileOpen(I, SS, OpenMode.Output)
                PrintLine(I, LEAFCompactionText)
                FileClose((I))
            End If
            'added for rigid compaction by YGC 112213 END


            '  If Not Beeped Then Beep ' GFH 04/23/03
            '  moved to frmStructure.cmdAddDelete.

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            ' MsgBox(txt)

        End Try

    End Sub


    Sub OneAircraft(ByVal CallAC As LEAFClassLib.clsLEAF.LEAFACParms, ByRef OneAC As LEAFClassLib.clsLEAF.LEAFACParms)
        Dim i1 As Integer

        OneAC.ACname = CallAC.ACname
        OneAC.GearLoad = CallAC.GearLoad
        OneAC.NTires = CallAC.NTires

        ReDim OneAC.TirePress(CallAC.NTires)
        ReDim OneAC.TireX(CallAC.NTires)
        ReDim OneAC.TireY(CallAC.NTires)

        For i1 = 1 To CallAC.NTires
            OneAC.TirePress(i1) = CallAC.TirePress(i1)
            OneAC.TireX(i1) = CallAC.TireX(i1)
            OneAC.TireY(i1) = CallAC.TireY(i1)
        Next i1

        OneAC.NEvalPoints = CallAC.NEvalPoints
        ReDim OneAC.EvalX(CallAC.NEvalPoints)
        ReDim OneAC.EvalY(CallAC.NEvalPoints)

        For i1 = 1 To CallAC.NEvalPoints
            OneAC.EvalX(i1) = CallAC.EvalX(i1)
            OneAC.EvalY(i1) = CallAC.EvalY(i1)
        Next i1

        OneAC.libGear = CallAC.libGear

    End Sub







    Sub NewRigidStr(ByVal Ov As LEAFClassLib.clsLEAF.LEAFStrParms, ByRef NewR As LEAFClassLib.clsLEAF.LEAFStrParms)
        Dim i1 As Integer

        NewR.NLayers = Ov.NLayers - 1
        ReDim NewR.Thick(NewR.NLayers)
        ReDim NewR.Modulus(NewR.NLayers)
        ReDim NewR.Poisson(NewR.NLayers)
        ReDim NewR.InterfaceParm(NewR.NLayers)

        For i1 = 1 To NewR.NLayers
            NewR.Thick(i1) = Ov.Thick(i1 + 1)
            NewR.Modulus(i1) = Ov.Modulus(i1 + 1)
            NewR.Poisson(i1) = Ov.Poisson(i1 + 1)
            NewR.InterfaceParm(i1) = Ov.InterfaceParm(i1 + 1)
        Next i1

        NewR.EvalDepth = NewR.Thick(1)
        NewR.EvalLayer = 1

    End Sub



    '§


    Function LifeTotal_PCConRigid2014(ByRef Th As Single, ByRef NextraACout As Short, ByRef ct As CancellationToken) As Single
        Try


            Dim OVER1(1, 1), PCC1(1, 1) As Single
            NSection = DefaultNSection
            'Nsection = 16
            ReDim gPCC_Surface_Mod(NSection)
            ReDim gPCC_SCI(NSection)

            ReDim gPCC_NIKE3D_Stress(NSection)
            ReDim gPCC_LEAF_Stress(NSection)
            'ReDim gPCC_USED_Stress(NSection)

            ReDim gOverlay_NIKE3D_Stress(NSection, MaxSectAC)
            ReDim gOverlay_LEAF_Stress(NSection, MaxSectAC)
            ReDim gOverlay_USED_Stress(NSection, MaxSectAC)

            Dim RepsIncZero As Boolean

            Dim modPCCLayer(NSection) As Double
            Dim stressOverlay2Layer(NSection) As Double
            Dim Overlay_LifeSCI80, OverLife2(NSection) As Single
            Dim PCCSurface_Life1crack, PCCSurface_LifeSCI0 As Single
            Dim LifeUsed(NSection), ItIsOne As Single

            Dim LeafResp As LEAFClassLib.clsLEAF.LEAFoptions
            LeafResp = LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress

            Dim CDFMAX As Single
            'Dim CDFMAX As Double
            Dim E0 As Single
            Dim Overflow, AggErr As Boolean, I, II, k As Short, j2 As Integer
            ' Call LEDFAA_to_LEAF_Rigid to init CallAC(), LEAStrActiveX, NextraAC
            '   NextraAC accounts for extra aircraft such as the A380 
            '   (wing is normal list, body is extra aircraft).
            Dim Response(,), HorStressResp(,) As Double
            Dim STRS1(MaxSectAC, 2) As Single
            Dim TimeSave1 As Integer
            Dim LifePassed As Single

            NSection = DefaultNSection : NextraAC = 0 : E0 = 4000000 '4 mln

            RepsIncZero = True
            For I = 1 To NAC
                If Not RepsInc(I) = 0 Then 'check if b=0
                    RepsIncZero = False : Exit For
                End If
            Next


            CCC = 0
            julThick(1) = Th
            Thick(1) = Th


            EvalDepth(1) = -Thick(1) : EvalDepth(2) = -Thick(1) - Thick(2)

            TimeSave1 = timeGetTime
            t1Thick = Now()

            Dim deltaSCI, SCI_middle(NSection), SCI_end(NSection) As Single
            Dim startLife(NSection), deltaLife(NSection) As Single

            deltaSCI = SCIB / NSection : SCI_end(0) = SCIB
            For I = 1 To NSection
                SCI_middle(I) = SCIB - deltaSCI / 2 - (I - 1) * deltaSCI
                SCI_end(I) = SCIB - I * deltaSCI
            Next

            T1min = 0.0! : T2min = 0.0!

            'GoTo kkk1
            Call UpdateCurrentSectData()
            Call FAAModulus(AggErr, SubLayers)

            julModulus(2) = E0 '4 mln
            Call WriteFile()
            Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)


            If FEDFAA1.FF_true_ACN And gAdjustAnnOrGL Then 'ik2021.05 LifeTotal_PCConRigid2014
                If gLEAFstressGreater(FEDFAA1.NAC_order(1)) And gLEAFstressGreaterOverlay(FEDFAA1.NAC_order(1)) Then
                    ReDim HorizStressResponse1(NAC, 2)
                    GoTo SkipRunAM_loc01
                End If
            End If


            '11111111111111111111111111111111111111111111111111111111111111111111111111111111
            '********** Calculate stresses in overlay and PCC surface layers ****************
            '********** Overlay modulus = 4 mln  ********************************************
            '********** PCC surface modulus 4 mln *******************************************
            '********************************************************************************

            Dim RunAM As New AMClassLib.clsAM()   'Instance of AutoMesh. (1)
            Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX _
                    , HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress,
                    DesignType, gSolverType, gSlabMeshSize, gOutputDirName, gUserInterrupted, NoOutFiles, ct) 'ik2020.03 JobName > gOutputDirName LifeTotal_PCConRigid2014
            RunAM = Nothing

SkipRunAM_loc01:
            ' ReDim HorizStressResponse1(NAC + NextraAC, 2) ' added for not having the RUNAm module Ali.D

            ReDim HorizStressResponse2(NAC + NextraAC, 2)
            ReDim HorizStressResponse3(NAC + NextraAC, 2)

            RunLEAF = New LEAFClassLib.clsLEAF()
            LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1))
            LEAStrActiveX.EvalLayer = 1
            Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
                LEAStrActiveX, HorizStressResponse2, AllResp)
            RunLEAF = Nothing

            RunLEAF = New LEAFClassLib.clsLEAF()
            LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1) + Thick(2))
            LEAStrActiveX.EvalLayer = 2
            Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
                LEAStrActiveX, HorizStressResponse3, AllResp)
            RunLEAF = Nothing

            ReDim Response(NAC + NextraAC, 2)
            'For j2 = 1 To NAC + NextraAC 'ONLY NIKE3D
            '    Response(j2, 1) = HorizStressResponse1(j2, 1)
            '    Response(j2, 2) = HorizStressResponse1(j2, 2)
            'Next
            'For j2 = 1 To NAC + NextraAC
            '    HorizStressResponse1(j2, 1) = 0  ' added for not having the RUNAm module Ali.D
            'Next
            ReDim gOverlay_NIKE3D_Stress(NAC + NextraAC, NSection) '2018.06.28
            ReDim gOverlay_LEAF_Stress(NAC + NextraAC, NSection)
            ReDim gOverlay_USED_Stress(NAC + NextraAC, NSection)
            ReDim gPCC_NIKE3D_Stress(NAC + NextraAC)
            ReDim gPCC_LEAF_Stress(NAC + NextraAC)

            For j2 = 1 To NAC + NextraAC 'Response(j2, 1)- overlay   Response(j2, 2) - PCC surface

                HorizStressResponse2(j2, 1) = HorizStressResponse2(j2, 1) * 0.95 '1 OVL
                HorizStressResponse3(j2, 1) = HorizStressResponse3(j2, 1) * 0.95 '1 OVL

                Response(j2, 1) = Math.Max(HorizStressResponse1(j2, 1), HorizStressResponse2(j2, 1))
                Response(j2, 2) = Math.Max(HorizStressResponse1(j2, 2), HorizStressResponse3(j2, 1))

                gOverlay_NIKE3D_Stress(j2, 0) = CSng(HorizStressResponse1(j2, 1))
                gOverlay_LEAF_Stress(j2, 0) = CSng(HorizStressResponse2(j2, 1))
                gOverlay_USED_Stress(j2, 0) = CSng(Response(j2, 1))

                gPCC_NIKE3D_Stress(j2) = CSng(HorizStressResponse1(j2, 2))
                gPCC_LEAF_Stress(j2) = CSng(HorizStressResponse3(j2, 1))
                'gPCC_USED_Stress(j2) = CSng(Response(j2, 2))
                If gPCR_Life Then 'ik2021.05 LifeTotal_PCConRigid2014
                    Dim j1 As Integer
                    j1 = j2

                    If AC(LibIndex(j1)).libGear = "H" Then
                        Continue For
                    End If

                    If ((j1 - 1) > 0) And (AC(LibIndex(j1 - 1)).libGear = "H") Then
                        Dim bool1_PCC, bool2_PCC As Boolean
                        bool1_PCC = gPCC_LEAF_Stress(j2) >= gPCC_NIKE3D_Stress(j2)
                        bool2_PCC = gPCC_LEAF_Stress(j2 - 1) >= gPCC_NIKE3D_Stress(j2 - 1)

                        If bool2_PCC And bool1_PCC Then
                            gLEAFstressGreater(j1 - 1) = True

                        ElseIf bool2_PCC And (bool1_PCC = False) Then
                            If gPCC_LEAF_Stress(j1 - 1) >= gPCC_NIKE3D_Stress(j1) Then
                                gLEAFstressGreater(j1 - 1) = True
                            Else
                                gLEAFstressGreater(j1 - 1) = False
                            End If
                        ElseIf (bool2_PCC = False) And bool1_PCC Then
                            If gPCC_LEAF_Stress(j1) >= gPCC_NIKE3D_Stress(j1 - 1) Then
                                gLEAFstressGreater(j1 - 1) = True
                            Else
                                gLEAFstressGreater(j1 - 1) = False
                            End If
                        Else
                            gLEAFstressGreater(j1 - 1) = False
                        End If



                        Dim bool1_Overlay, bool2_Overlay As Boolean
                        bool1_Overlay = gOverlay_LEAF_Stress(j2, 0) >= gOverlay_NIKE3D_Stress(j2, 0)
                        bool2_Overlay = gOverlay_LEAF_Stress(j2 - 1, 0) >= gOverlay_NIKE3D_Stress(j2 - 1, 0)

                        If bool2_Overlay And bool1_Overlay Then
                            gLEAFstressGreater(j1 - 1) = True

                        ElseIf bool2_Overlay And (bool1_Overlay = False) Then
                            If gOverlay_LEAF_Stress(j1 - 1, 0) >= gOverlay_NIKE3D_Stress(j1, 0) Then
                                gLEAFstressGreaterOverlay(j1 - 1) = True
                            Else
                                gLEAFstressGreaterOverlay(j1 - 1) = False
                            End If
                        ElseIf (bool2_Overlay = False) And bool1_Overlay Then
                            If gOverlay_LEAF_Stress(j1, 0) >= gOverlay_NIKE3D_Stress(j1 - 1, 0) Then
                                gLEAFstressGreaterOverlay(j1 - 1) = True
                            Else
                                gLEAFstressGreaterOverlay(j1 - 1) = False
                            End If
                        Else
                            gLEAFstressGreaterOverlay(j1 - 1) = False
                        End If

                    Else

                        If gPCC_LEAF_Stress(j2) >= gPCC_NIKE3D_Stress(j2) Then
                            gLEAFstressGreater(j2) = True
                        Else
                            gLEAFstressGreater(j2) = False
                        End If

                        If gOverlay_LEAF_Stress(j2, 0) >= gOverlay_NIKE3D_Stress(j2, 0) Then
                            gLEAFstressGreaterOverlay(j2) = True
                        Else
                            gLEAFstressGreaterOverlay(j2) = False
                        End If

                    End If

                End If


            Next

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1) '02
            If gUserInterrupted Then Exit Function

            ReDim HorStressResp(NAC + NextraAC, 1)
            For II = 1 To CShort(NAC + NextraAC)
                OverlayStress(II) = Response(II, 1)
                PCCStress(II) = Response(II, 2)
                HorStressResp(II, 1) = Response(II, 1) 'for overlay to be used in LeafCDFRigid
            Next II


            OVER1(1, 1) = CSng(HorStressResp(1, 1))
            'Calculate CDF for Rigid Overlay Layer for SCI=80
            Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(1), 80) '1
            'LeafCDFRigid sets STRSH() = Max HorizStressResponse().
            Overlay_LifeSCI80 = CSng(Life / CDFMAX) '001
            If Not RepsIncZero Then
                Overlay_LifeSCI80 = FindLife(0, Overlay_LifeSCI80, CDFMAX,
                                    Overflow, HorStressResp, RCon(1), 80)
            End If


            ReDim HorStressResp(NAC + NextraAC, 1)
            For II = 1 To CShort(NAC + NextraAC)
                HorStressResp(II, 1) = Response(II, 2) 'for PCC surface to be used in LeafCDFRigid
            Next II

            PCC1(1, 1) = CSng(HorStressResp(1, 1))
            Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(2), 100) '2
            PCCSurface_Life1crack = CSng(Life / CDFMAX) '002
            If Not RepsIncZero Then
                PCCSurface_Life1crack = FindLife(0, PCCSurface_Life1crack, CDFMAX,
                                       Overflow, HorStressResp, RCon(2), 100)
            End If


            'PCC Surface Layer - CDF for SCI = 0
            Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(2), 0) '2
            PCCSurface_LifeSCI0 = CSng(Life / CDFMAX) '003
            If Not RepsIncZero Then
                PCCSurface_LifeSCI0 = FindLife(0, PCCSurface_LifeSCI0, CDFMAX,
                                     Overflow, HorStressResp, RCon(2), 0)
            End If

            If PCCSurface_Life1crack * (100 - LifeExistPCC) * 0.01! > Overlay_LifeSCI80 Then
                T1min = Overlay_LifeSCI80
                LifeTotal_PCConRigid2014 = T1min - Life
                CCC = 9
                t1Thick = Now()
                GoTo print1
                Exit Function
            Else
                T1min = PCCSurface_Life1crack * (100 - LifeExistPCC) * 0.01!
                LifeUsed(0) = T1min / Overlay_LifeSCI80
            End If


            'calculate life for each period for PCC surface deterioration.
            For I = 0 To NSection 'starting from SCIB
                Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(2), SCI_end(I))
                startLife(I) = CSng(Life / CDFMAX) '004
                If Not RepsIncZero Then
                    startLife(I) = FindLife(0, startLife(I), CDFMAX, Overflow,
                                            HorStressResp, RCon(2), SCI_end(I))
                End If
            Next


            If SCIB = 100 And LifeExistPCC < 100 Then
                ItIsOne = LifeUsed(0)
            Else
                ItIsOne = 0
            End If

            'YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            ' for %CDFU = 100       and SCIB <= 100
            ' calculate T2min when PCC surface started to deteriorate
            ' PCC surface modulus calculated based on SCI in the middle of section
            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


            For I = 1 To CShort(NSection)
                deltaLife(I) = startLife(I) - startLife(I - 1)
            Next

            LifePassed = T1min

            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            'kkk1:
            '        Dim Interval As Microsoft.VisualBasic.DateInterval
            '        Dim DayOfWeek As Microsoft.VisualBasic.FirstDayOfWeek
            '        Dim WeekOfYear As Microsoft.VisualBasic.FirstWeekOfYear
            '        Interval = DateInterval.Second
            '        DayOfWeek = FirstDayOfWeek.Sunday
            '        WeekOfYear = FirstWeekOfYear.Jan1


            '        Dim tStart1, tStart2, tStart3 As Date
            '        Dim tdiff As Long

            '        tStart1 = Now()

            '        Dim kkk As Single
            '        For kkk = 1040000 To 1000 Step -1000
            '            tStart2 = Now()

            '            Call UpdateCurrentSectData()
            '            Call FAAModulus(AggErr, SubLayers)
            '            julModulus(2) = kkk
            '            If julModulus(2) > E0 Then julModulus(2) = E0
            '            Call WriteFile()
            '            Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)

            '            RunAM = New AMClassLib.clsAM()
            '            Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX _
            '                    , HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress, _
            '                    DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted)
            '            RunAM = Nothing
            '            GC.Collect()
            '            System.Windows.Forms.Application.DoEvents()

            '            tdiff = DateDiff(Interval, tStart2, Now(), DayOfWeek, WeekOfYear)
            '            FileOpen(13, WorkingDir & "\FF141" & SectName & " .txt", OpenMode.Append, , , 1024)
            '            Print(13, LPad(17, Format(julModulus(2), "##,##0.000")))
            '            Print(13, LPad(12, Format(HorizStressResponse1(1, 1), "##,##0.000")))
            '            PrintLine(13, LPad(12, format(tdiff, "##,##0.00")))
            '            FileClose(13)

            '        Next


            '        tdiff = DateDiff(Interval, tStart1, Now(), DayOfWeek, WeekOfYear)
            '        FileOpen(13, WorkingDir & "\FF141" & SectName & " .txt", OpenMode.Append, , , 1024)
            '        PrintLine(13, "")
            '        PrintLine(13, LPad(12, format(tdiff, "##,##0.00")) & " min.")
            '        PrintLine(13, LPad(12, format(tdiff / 60, "##,##0.00")) & " minutes")
            '        PrintLine(13, LPad(12, Format(tdiff / 60 / 60, "##,##0.00")) & " hours")
            '        FileClose(13)

            '        Exit Function
            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            gNSectionUsed = 0

            For I = 1 To NSection

                'calculate modulus julModulus(2) and life
                Call UpdateCurrentSectData()
                Call FAAModulus(AggErr, SubLayers)
                julModulus(2) = CSng(E0 * (0.02 + 0.0064 * SCI_middle(I) + (0.00584 * SCI_middle(I)) ^ 2))
                If julModulus(2) > E0 Then julModulus(2) = E0

                gPCC_Surface_Mod(I) = julModulus(2)
                gPCC_SCI(I) = SCI_middle(I)

                Call WriteFile()
                Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
                If FEDFAA1.FF_true_ACN And gAdjustAnnOrGL Then 'ik2021.05 loc02
                    If gLEAFstressGreaterOverlay(FEDFAA1.NAC_order(1)) Then
                        ReDim HorizStressResponse1(NAC, 1)
                        GoTo SkipRunAM_loc02
                    End If
                End If


                '33333333333333333333333333333333333333333333333333333333333333333333333333333333
                '********** Calculate stresses in overlay layer              ********************
                '********** Overlay modulus = 4 mln  ********************************************
                '********** PCC surface modulus calculated based on SCI in mid-subsection *******
                '********************************************************************************

                RunAM = New AMClassLib.clsAM()
                Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX _
                        , HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress,
                        DesignType, gSolverType, gSlabMeshSize, gOutputDirName, gUserInterrupted, NoOutFiles, ct) 'ik2020.03 JobName ->gOutputDirName LifeTotal_PCConRigid2014
                RunAM = Nothing

SkipRunAM_loc02:
                Call FlushMemory1()


                RunLEAF = New LEAFClassLib.clsLEAF()
                LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1))
                LEAStrActiveX.EvalLayer = 1
                Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
                    LEAStrActiveX, HorizStressResponse2, AllResp)
                RunLEAF = Nothing

                ReDim Response(NAC + NextraAC, 1)
                'For j2 = 1 To NAC + NextraAC 'ONLY NIKE3D
                '    Response(j2, 1) = HorizStressResponse1(j2, 1)
                'Next

                For j2 = 1 To NAC + NextraAC

                    HorizStressResponse2(j2, 1) = HorizStressResponse2(j2, 1) * 0.95
                    Response(j2, 1) = Math.Max(HorizStressResponse1(j2, 1), HorizStressResponse2(j2, 1))
                    gOverlay_NIKE3D_Stress(j2, I) = CSng(HorizStressResponse1(j2, 1))
                    gOverlay_LEAF_Stress(j2, I) = CSng(HorizStressResponse2(j2, 1))
                    gOverlay_USED_Stress(j2, I) = CSng(Response(j2, 1))
                Next


                If gUserInterrupted Then Exit Function
                ReDim HorStressResp(NAC + NextraAC, 1)
                For II = 1 To CShort(NAC + NextraAC)
                    HorStressResp(II, 1) = Response(II, 1) 'for Overlay to be used in LeafCDFRigid
                Next II

                'Call LeafCDFRigid_NP(CDFMAX, Overflow, SSS, RCon(1), 80)

                'Calculate CDF for overlay at SCI=80 and changing base modulus
                Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(1), 80)
                stressOverlay2Layer(I) = HorStressResp(1, 1)
                modPCCLayer(I) = julModulus(2)

                OverLife2(I) = CSng(Life / CDFMAX) '005

                If Not RepsIncZero Then
                    OverLife2(I) = FindLife(LifePassed, OverLife2(I), CDFMAX,
                                        Overflow, HorStressResp, RCon(1), 80)
                    LifePassed = LifePassed + deltaLife(I)
                End If




                LifeUsed(I) = deltaLife(I) / OverLife2(I)
                ItIsOne = ItIsOne + LifeUsed(I)
                If ItIsOne >= 1 Then Exit For

                If I = 1 Then
                    If startLife(0) = startLife(NSection) And startLife(0) > 10000000 Then
                        'If OverLife2(1) < 1000 Then Exit For
                        Exit For
                    End If
                End If
            Next I

            gNSectionUsed = I
            If gNSectionUsed > NSection Then
                gNSectionUsed = NSection
            End If

            T2min = 0 : CCC = 0
            If ItIsOne >= 1 And I <= NSection Then 'OK case

                If I = 1 Then 'overlay life < subsection 1 PCC surface life

                    If LifeExistPCC < 100 Then
                        T2min = OverLife2(1) * (1 - (ItIsOne - LifeUsed(1)))
                        CCC = 7 '******************************************
                    Else
                        T2min = OverLife2(1)
                        CCC = 8 '******************************************
                    End If

                Else
                    For k = 1 To I - 1S
                        T2min = T2min + deltaLife(k)
                    Next
                    'T2min = T2min + (1 - (ItIsOne - LifeUsed(I))) / LifeUsed(I) * deltaLife(I)
                    T2min = T2min + (1 - (ItIsOne - LifeUsed(I))) * OverLife2(I)
                    CCC = 1 '******************************************
                End If
                '----------------------------------------------------------------------
            Else 'Special conditions (life used < 1)

                If ItIsOne = 0 Then 'very long life for PCC surface
                    T2min = OverLife2(1)
                    CCC = 2 '******************************************

                ElseIf ItIsOne < 1 Then

                    For I = 1 To NSection - 1S
                        If LifeUsed(I) = 0 And deltaLife(I) = 0 Then
                            T2min = T2min + (1 - ItIsOne) * OverLife2(I)
                            Exit For
                        ElseIf LifeUsed(I) = 0 Then
                            T2min = T2min + deltaLife(I)
                        ElseIf deltaLife(I) = 0 Then
                            CCC = 5
                        Else
                            T2min = T2min + deltaLife(I)
                            CCC = 6
                        End If
                    Next

                    If LifeUsed(NSection) > 0 And deltaLife(NSection) > 0 Then
                        T2min = T2min + OverLife2(NSection)
                    End If
                    CCC = 3 '******************************************
                Else
                    CCC = 4 '******************************************
                End If

            End If


            LifeTotal_PCConRigid2014 = T1min + T2min - Life

            'End Function

            t1Thick = Now()

            'Start of new code 2021.03.18
            For k1 = 1 To NAC + NextraAC + 1
                For k2 = 1 To 41
                    CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                Next
            Next




            Dim scaling_ratio As Single

            scaling_ratio = (Life / (T1min + T2min)) / CDFMAX

            For ik As Integer = 1 To NAC
                CDFtableTemp(ik) = jobCDFtable(ISect, ik) * scaling_ratio
                CDFacrftMaxtableTemp(ik) = jobCDFacrftMaxtable(ISect, ik) * scaling_ratio
                jobCDFtable(ISect, ik) = jobCDFtable(ISect, ik) * scaling_ratio
                jobCDFacrftMaxtable(ISect, ik) = jobCDFacrftMaxtable(ISect, ik) * scaling_ratio
            Next
            CDFMAX = CDFMAX * scaling_ratio

            For k1 = 1 To NAC + NextraAC + 1
                For k2 = 1 To 41
                    CDFdata2(1, k1, k2) = CDFdata2(1, k1, k2) * scaling_ratio
                Next
            Next
            If LifeComputation Then
                LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
                If LifeCounterForPCRRuns = 1 Then
                    For k1 = 1 To NAC + NextraAC + 1
                        For k2 = 1 To 41
                            CDFdata3(1, k1, k2) = CDFdata2(1, k1, k2)
                        Next
                    Next
                    For ik As Integer = 1 To NAC
                        CDFtableTemp3(ik) = CDFtableTemp(ik)
                        CDFacrftMaxtableTemp3(ik) = CDFacrftMaxtableTemp(ik)
                    Next
                End If
            End If
            'End of new code 2021.03.18



            If gPrintPCConRigidInfo Then
            Else
                Exit Function
            End If


print1:
            '==========================================================================
            '========== Printing Results ==============================================
            '==========================================================================

            Dim uuu1 As String
            'uuu1 = CStr(Thick(1)) & "_" & CStr(Thick(2))
            uuu1 = "O" & CStr(Thick(1)) & "_S" & CStr(Thick(2))


            'FileOpen(13, WorkingDir & "\FF141debug_" & uuu1 & ".txt", OpenMode.Append, , , 1024)
            'PrintLine(13, "")
            'PrintLine(13, "SCIB = " & SCIB & "    %CDFU = " & LifeExistPCC)
            'SS = Space(11 - Format(OVER1(1, 1), "##,##0.000").Length) & Format(OVER1(1, 1), "##,##0.000")
            'Print(13, "Overlay_Stress= " & SS)
            'PrintLine(13, "   " & Thick(1))
            'SS = Space(11 - Format(PCC1(1, 1), "##,##0.000").Length) & Format(PCC1(1, 1), "##,##0.000")
            'Print(13, "    PCC_Stress= " & SS)
            'PrintLine(13, "   " & Thick(2))
            'PrintLine(13, "    Overlay_LifeSCI80= " & LPad(10, Format(Overlay_LifeSCI80, "##,##0.000")))
            'PrintLine(13, "PCCSurface_Life1crack= " & LPad(10, Format(PCCSurface_Life1crack, "##,##0.000")))
            'PrintLine(13, "  PCCSurface_LifeSCI0= " & LPad(10, Format(PCCSurface_LifeSCI0, "##,##0.000")))
            'PrintLine(13, "")


            'Dim usedSum, lifeSum As Single, yyy As Boolean

            'usedSum = LifeUsed(0) : lifeSum = 0 : yyy = False
            'Print(13, LPad(20, "startLife") & LPad(20, "deltaLife") & LPad(20, "OverLife"))
            'PrintLine(13, LPad(15, "LifeUsed") & LPad(15, "usedSum") & LPad(15, "lifeSum"))
            'Print(13, LPad(20, Format(startLife(0), "##,##0.000")))
            'PrintLine(13, LPad(49, Format(LifeUsed(0), "##,##0.000")))
            'For I = 1 To NSection
            '    usedSum = usedSum + LifeUsed(I)
            '    lifeSum = lifeSum + deltaLife(I)

            '    If startLife(I) > 1 Then
            '        Print(13, LPad(20, Format(startLife(I), "##,##0.000")))
            '    Else
            '        Print(13, LPad(20, Format(startLife(I), "##,##0.000000000000")))
            '    End If

            '    If deltaLife(I) > 1 Then
            '        Print(13, LPad(20, Format(deltaLife(I), "##,##0.000")))
            '    Else
            '        Print(13, LPad(20, Format(deltaLife(I), "##,##0.000000000000")))
            '    End If

            '    If OverLife2(I) > 1 Then
            '        Print(13, LPad(20, Format(OverLife2(I), "##,##0.000")))
            '    Else
            '        Print(13, LPad(20, Format(OverLife2(I), "##,##0.000000000000")))
            '    End If

            '    If LifeUsed(I) > 1 Then
            '        Print(13, LPad(15, Format(LifeUsed(I), "##,##0.000")))
            '    Else
            '        Print(13, LPad(15, Format(LifeUsed(I), "##,##0.000000000")))
            '    End If

            '    If usedSum > 1 Then
            '        Print(13, LPad(15, Format(usedSum, "##,##0.000")))
            '    Else
            '        Print(13, LPad(15, Format(usedSum, "##,##0.000000000")))
            '    End If

            '    If lifeSum > 1 Then
            '        PrintLine(13, LPad(15, Format(lifeSum, "##,##0.000")))
            '    Else
            '        PrintLine(13, LPad(15, Format(lifeSum, "##,##0.000000000")))
            '    End If


            '    If usedSum > 1 And Not yyy Then
            '        Dim restSum, restLife, newLife As Single
            '        restSum = 1 - (usedSum - LifeUsed(I))
            '        restLife = deltaLife(I) * restSum
            '        newLife = lifeSum - deltaLife(I) + restLife
            '        PrintLine(13, "")
            '        Print(13, "restSum= " & LPad(10, Format(restSum, "##,##0.000")))
            '        Print(13, "   restLife= " & LPad(10, Format(restLife, "##,##0.000")))
            '        PrintLine(13, "   newLife= " & LPad(10, Format(newLife, "##,##0.000")))
            '        PrintLine(13, "") : yyy = True
            '    End If

            'Next

            'PrintLine(13, "")
            'For I = 1 To NSection
            '    Print(13, LPad(9, Format(SCI_middle(I), "##,##0.000")))
            '    Print(13, LPad(15, Format(modPCCLayer(I), "##,##0.000")))
            '    Print(13, LPad(11, Format(stressOverlay2Layer(I), "##,##0.000")))
            '    PrintLine(13, LPad(11, Format(OverLife2(I), "##,##0.000")))
            'Next


            'PrintLine(13, "")
            'S = LPad(10, Format(ItIsOne, "##,##0.00000000000000"))
            'SS = Space(23 - Format(ItIsOne, "##,##0.000").Length) & S
            'PrintLine(13, "ItIsOne= " & SS)
            'SS = Space(23 - Format(T1min, "##,##0.000").Length) & LPad(10, Format(T1min, "##,##0.00000000"))
            'PrintLine(13, "T1min= " & SS)
            'SS = Space(23 - Format(T2min, "##,##0.000").Length) & LPad(10, Format(T2min, "##,##0.00000000"))
            'PrintLine(13, "T2min= " & SS)
            'PrintLine(13, "CCC= " & LPad(3, Format(CCC, "##,##0")))
            'PrintLine(13, "")
            'FileClose(13)
        Catch ex As Exception

        End Try
    End Function



    Sub LeafCDFRigid_2014(ByRef lclCDFMAX As Single, ByRef Overflow As Boolean,
        ByRef StressResponse(,) As Double, ByVal subrFlexStrength As Single, ByVal subrSCI As Single)
        ' David's model
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

        Dim ParamA, ParamB, ParamC, ParamD As Single
        Dim ParA1, ParC1 As Double, mods1 As Single

        Try

            SCITemp = subrSCI

            ParamA = 1.027 : ParamB = 0.16 : ParamC = 1.1 : ParamD = 0.16
            'added to implement E(SG)-dependent failure model parameter by YGC 112213
            If Modulus(NPLayers) <= 4500 Then
                ParamA = 0.76
                ParamC = 0.857

            ElseIf Modulus(NPLayers) >= 45000 Then
                'CASE04
                ParamA = CSng(0.76 + (1.027 - 0.76) * (Modulus(NPLayers) - 4500) / 10500)
                ParamC = CSng(1.7942915 + 0.00002542857143 * (Modulus(NPLayers) - 45000))

            ElseIf Modulus(NPLayers) > 25000 Then
                '1.41.0008 linear IK 2016.06.21 CASE00
                ParamA = CSng(0.76 + (1.027 - 0.76) * (Modulus(NPLayers) - 4500) / 10500)
                ParamC = CSng(0.857 + 0.000023143 * (Modulus(NPLayers) - 4500))
            Else
                ParamA = CSng(0.76 + (1.027 - 0.76) * (Modulus(NPLayers) - 4500) / 10500)
                ParamC = CSng(0.857 + 0.000023143 * (Modulus(NPLayers) - 4500))

            End If
            'add ended to implement E(SG)-dependent failure model parameter by YGC 112213


            If gRigidNewParam Then 'LeafCDFRigid_NP
                ParamA = gParamA
                ParamB = gParamB
                ParamC = gParamC
                ParamD = gParamD
            Else
                gParamA = ParamA
                gParamB = ParamB
                gParamC = ParamC
                gParamD = ParamD
            End If



            IextraAC = 0
            For IA = 1 To NAC
                LI = LibIndex(IA)
                If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
                    IextraAC = IextraAC + 1
                End If

                If AC(LI).libGear = "X" And AC(LI).libNGroups = 2 Then '(1) modOVERLAY_NP
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
                    GearLoadType(NACpIex) = "F" 'ikawa
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

                ElseIf AC(LI).libGear = "X" Then '(2) modOVERLAY_NP

                    If AC(LI).libNGroups = 2 Then
                        IextraAC = IextraAC + 1
                        IextraACIndex(IextraAC) = IA
                    End If

                    NACpIex = NAC + IextraAC
                    GearLoadType(IA) = "X"

                    If AC(LI).libNGroups = 2 Then
                        GearLoadType(NACpIex) = "X"
                        RepsAnnual(NACpIex) = RepsAnnual(IA)
                        RepsInc(NACpIex) = RepsInc(IA)
                        Reps(NACpIex) = Reps(IA)
                        ACTT(NACpIex) = ACTT(IA)
                        STRSH(NACpIex) = 0
                    End If
                    ACTT(IA) = AC(LI).libTT
                    'For I = 1 To 8
                    For I = 1 To CShort(UBound(StressResponse, 2)) 'ikawa 
                        If System.Math.Abs(StressResponse(IA, I)) > STRSH(IA) Then
                            STRSH(IA) = CSng(System.Math.Abs(StressResponse(IA, I)))
                        End If

                        If AC(LI).libNGroups = 2 Then
                            If System.Math.Abs(StressResponse(NACpIex, I)) > STRSH(NACpIex) Then
                                STRSH(NACpIex) = CSng(System.Math.Abs(StressResponse(NACpIex, I)))
                            End If
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
                'ikawa Call LeafInteriorToEdge(IA, GearLoadType(IA), ACTT(IA), STRSH(IA)) ' Compensate for edge stresses.
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


            Overflow = True ' All aircraft must overflow to return True.
            'FSlopeComp = CSng((0.392 - FSlope * 0.3881) / (FSlope * 0.0039))

            'If OverlayRigOnRig Then
            '    FSlope = 1
            '    FSlopeComp = 1
            'End If


            For IA = 1 To CShort(NAC + IextraAC)
                System.Windows.Forms.Application.DoEvents()

                'STRSH(IA) = STRSH(IA) * EScale
                'STRSH(IA) = CSng(STRSH(IA) * 1.25)

                If gFSlopeNewValue Then
                    FSlope = gFSlope
                Else
                    gFSlope = FSlope
                End If



                If gRigidCalibrationNewValue Then

                Else

                End If


                'MsgBox(ParamA & "   " & ParamC, MsgBoxStyle.OkOnly, "ParamA & C P1")

                'New FAARFIELD model
                'DesFactor = CSng(julRCon / (STRSH(IA) * gCalibrationFactor + 0.000000000001))
                DesFactor = CSng(subrFlexStrength / (STRSH(IA) * gCalibrationFactor + 0.000000000001))
                AA = DesFactor - ((1.0 - SCITemp / 100.0) * (ParamA * ParamD - ParamB * ParamC) + FSlope * ParamB * ParamC) / ((1.0 - SCITemp / 100.0) * (ParamD - ParamB) + FSlope * ParamB)
                BB = (FSlope * ParamB * ParamD) / ((1.0 - SCITemp / 100.0) * (ParamD - ParamB) + FSlope * ParamB)
                Temp = CSng(AA / BB)
                LogCA2(IA) = Temp ' GuoAdd

                If OverlayRigOnRig Then
                    If Temp > 30.0! Then Temp = 30.0! Else Overflow = False 'LeafCDFRigid_2014
                Else
                    If Temp > 10.0! Then Temp = 10.0! Else Overflow = False 'LeafCDFRigid_2014
                End If

                'If Temp > 10.0! Then Temp = 10.0! Else Overflow = False
                NtoFail = CSng(10.0! ^ Temp)
                If ((DesFactor - ParamC) / ParamD) > 10.0! Then
                    NtoFailCDFU = NtoFail
                Else
                    NtoFailCDFU = CSng(10.0! ^ ((DesFactor - ParamC) / ParamD))
                End If

                modPCN_ZZZ.gCoverage_NtoFail(IA) = CSng(NtoFail) 'PCN method for rigid
                publicNtoFail = CSng(NtoFail) 'PCN method for rigid

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

            lclCDFMAX = 0.0! : PercentCdfu = 0.0!
            For IOFF = 1 To NOFF
                If lclCDFMAX < CDFCON(IOFF) Then
                    lclCDFMAX = CDFCON(IOFF)
                    IControl = IOFF
                End If
                If PercentCdfu < CDFU(IOFF) Then PercentCdfu = CDFU(IOFF)
            Next IOFF
            PercentCdfu = PercentCdfu * 100.0!

            ReDim gCDF(NAC + 1)
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

            'CDFPic = lclCDFMAX 'ikawa 10/05/04

            For IA = 1 To CShort(NAC + NextraAC) 'ikawa555
                gCDF(IA) = CSng(lclCDF(IA, IControl))

                If gCDF(IA) / lclCDFMAX < 0.001 Then
                    'ikawa CalcStress(IA, 2) = 1
                End If
            Next



            'If (PrintHist Or (DesignType = NewRigid)) And BatchMode Then
            '    Dim FNo As Integer
            '    FNo = FreeFile()
            '    tdiff = DateDiff(DateInterval.Second, t1Thick, Now(), FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)
            '    FileOpen(FNo, "ThickHist" & JobName & ".txt", OpenMode.Append, , , 1024)
            '    PrintLine(FNo, "Job: " & JobName & "   Sect: " & LPad(8, SectName) _
            '    & " Thick: " & LPad(8, Format(Thick(1), "###.0000")) _
            '    & "  CDF: " & LPad(8, Format(CDFPic, "##0.000")) _
            '    & "  Time: " & LPad(8, Format(tdiff / 60, "##0.00")) & " min.")
            '    FileClose(FNo)
            'End If

            PrintResultsCDF = True
            If PrintResultsCDF Then
                ''Dim LLL As String
                ''If OverlayRigOnRig Then
                ''    LLL = "  OverlayLife: " & LPad(8, Format(OverlayLife, "#,##0.000"))
                ''Else
                ''    LLL = "  Life: " & LPad(8, Format(LifeStr, "#,##0.000"))
                ''End If

                'Dim J As Integer
                'FileOpen(TimeFile, WorkingDir & "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                'PrintLine(TimeFile, "LeafCDFRigid" & LPad(10, "Aircraft") & LPad(10, "Gear_Load") _
                '      & LPad(10, "Tire_Load") & LPad(15, "Job") & LPad(9, "Section") _
                '      & LPad(12, "CDF") & LPad(24, CStr(Now)))
                'For J = 1 To NAC + CShort(NextraAC)
                '    'If Not RunBatchLife Then
                '    PrintLine(TimeFile, LPad(3, CStr(J)) & LPad(19, CallAC(J).ACname) _
                '    & LPad(10, Format(CallAC(J).GearLoad, "#,##0,000")) _
                '    & LPad(10, Format(CallAC(J).GearLoad / CallAC(J).NTires, "#,##0,000")) _
                '    & LPad(15, JobName) & LPad(9, SectName) _
                '    & LPad(12, CStr(Format(gCDF(J), "##,##0.00000"))))
                '    'End If
                'Next J
                'PrintLine(TimeFile, StrDup(46, " "),
                'LPad(22, Format(lclCDFMAX, "##,##0.00000")) & "  ") ' & LLL)
                'PrintLine(TimeFile, StrDup(103, "-"))
                'FileClose(TimeFile)
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


            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
            For IA = 1 To NAC
                For IOFF = 1 To NOFF
                    CDFdata(1, IA, IOFF) = 0
                    CDFdata(1, NAC + 1, IOFF) = 0
                Next IOFF
            Next IA

            For IA = 1 To NAC
                For IOFF = 1 To NOFF
                    'jobCDFtable(ISect, IA) = lclCDF(IA, IOFF)
                    CDFdata(1, IA, IOFF) = CSng(lclCDF(IA, IOFF))
                    CDFdata(1, NAC + 1, IOFF) = CDFdata(1, NAC + 1, IOFF) + CSng(lclCDF(IA, IOFF))
                Next IOFF
            Next IA
            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

            gCDF_Rigid = lclCDFMAX

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub

    Public Sub AdjustOverlayThickness2020(ByVal I As Integer, ByRef Th1 As Single, ByRef Th2 As Single, ByVal OverlayLife1 As Single, ByVal OverlayLife As Single)
        'new subroutine AdjustOverlayThickness2020

        Dim cdf_temp As Single, oldTh2 As Single

        oldTh2 = Th2
        cdf_temp = Life / OverlayLife

        Dim CDFxx(15), Coeffxx(15) As Single, ik As Integer, coeff1 As Single


        If DesignType = UnbondOnRigid Or DesignType = PartBondOnRigid Then

            If Thick(2) >= 16 Then

                CDFxx(1) = 4.152 : Coeffxx(1) = 1.388 'xxx > 16
                CDFxx(2) = 3.625 : Coeffxx(2) = 1.336
                CDFxx(3) = 3.332 : Coeffxx(3) = 1.288
                CDFxx(4) = 3 : Coeffxx(4) = 1.243
                CDFxx(5) = 2.646 : Coeffxx(5) = 1.202
                CDFxx(6) = 2.218 : Coeffxx(6) = 1.163
                CDFxx(7) = 1.968 : Coeffxx(7) = 1.126
                CDFxx(8) = 1.703 : Coeffxx(8) = 1.092
                CDFxx(9) = 1.434 : Coeffxx(9) = 1.059
                CDFxx(10) = 1.155 : Coeffxx(10) = 1.029
                CDFxx(11) = 1 : Coeffxx(11) = 1
                CDFxx(12) = 0.861 : Coeffxx(12) = 0.973
                CDFxx(13) = 0.735 : Coeffxx(13) = 0.947
                CDFxx(14) = 0.595 : Coeffxx(14) = 0.923
                CDFxx(15) = 0.488 : Coeffxx(15) = 0.899

            ElseIf Thick(2) >= 14 Then

                CDFxx(1) = 7.16 : Coeffxx(1) = 1.283 'xxx
                CDFxx(2) = 6.134 : Coeffxx(2) = 1.248
                CDFxx(3) = 5.249 : Coeffxx(3) = 1.214
                CDFxx(4) = 4.317 : Coeffxx(4) = 1.183
                CDFxx(5) = 3.55 : Coeffxx(5) = 1.153
                CDFxx(6) = 2.931 : Coeffxx(6) = 1.124
                CDFxx(7) = 2.438 : Coeffxx(7) = 1.097
                CDFxx(8) = 1.941 : Coeffxx(8) = 1.071
                CDFxx(9) = 1.542 : Coeffxx(9) = 1.046
                CDFxx(10) = 1.222 : Coeffxx(10) = 1.023
                CDFxx(11) = 1 : Coeffxx(11) = 1
                CDFxx(12) = 0.79 : Coeffxx(12) = 0.978
                CDFxx(13) = 0.624 : Coeffxx(13) = 0.958
                CDFxx(14) = 0.482 : Coeffxx(14) = 0.938
                CDFxx(15) = 0.402 : Coeffxx(15) = 0.919

            ElseIf Thick(3) >= 10 Then

                CDFxx(1) = 12.031 : Coeffxx(1) = 1.208 'xxx
                CDFxx(2) = 9.933 : Coeffxx(2) = 1.184
                CDFxx(3) = 8.112 : Coeffxx(3) = 1.16
                CDFxx(4) = 6.24 : Coeffxx(4) = 1.137
                CDFxx(5) = 4.991 : Coeffxx(5) = 1.115
                CDFxx(6) = 3.949 : Coeffxx(6) = 1.094
                CDFxx(7) = 2.948 : Coeffxx(7) = 1.074
                CDFxx(8) = 2.286 : Coeffxx(8) = 1.055
                CDFxx(9) = 1.755 : Coeffxx(9) = 1.036
                CDFxx(10) = 1.333 : Coeffxx(10) = 1.018
                CDFxx(11) = 1.002 : Coeffxx(11) = 1
                CDFxx(12) = 0.712 : Coeffxx(12) = 0.983
                CDFxx(13) = 0.525 : Coeffxx(13) = 0.967
                CDFxx(14) = 0.383 : Coeffxx(14) = 0.951
                CDFxx(15) = 0.264 : Coeffxx(15) = 0.935

            Else

                CDFxx(1) = 38.655 : Coeffxx(1) = 1.182
                CDFxx(2) = 28.158 : Coeffxx(2) = 1.161
                CDFxx(3) = 20.299 : Coeffxx(3) = 1.141
                CDFxx(4) = 14.485 : Coeffxx(4) = 1.121
                CDFxx(5) = 10.051 : Coeffxx(5) = 1.102
                CDFxx(6) = 7.033 : Coeffxx(6) = 1.083
                CDFxx(7) = 4.871 : Coeffxx(7) = 1.066
                CDFxx(8) = 3.341 : Coeffxx(8) = 1.048
                CDFxx(9) = 2.232 : Coeffxx(9) = 1.032
                CDFxx(10) = 1.501 : Coeffxx(10) = 1.016
                CDFxx(11) = 1 : Coeffxx(11) = 1
                CDFxx(12) = 0.659 : Coeffxx(12) = 0.985
                CDFxx(13) = 0.425 : Coeffxx(13) = 0.97
                CDFxx(14) = 0.275 : Coeffxx(14) = 0.956
                CDFxx(15) = 0.176 : Coeffxx(15) = 0.942

            End If


            For ik = 1 To 15
                If cdf_temp > CDFxx(ik) Then
                    Exit For
                End If
            Next


            If ik = 1 Then
                coeff1 = Coeffxx(1)
            ElseIf ik = 20 Then
                coeff1 = Coeffxx(15)
            Else
                Dim temp11, temp22, temp33 As Single

                temp11 = CSng((CDFxx(ik - 1) - CDFxx(ik)) / 0.25)
                temp22 = CSng((cdf_temp - CDFxx(ik)) / temp11)
                temp33 = CSng((Coeffxx(ik - 1) - Coeffxx(ik)) / 0.25)
                coeff1 = Coeffxx(ik) + temp22 * temp33

            End If

            If I = 1 Then
                Th2 = Th1 * coeff1

            ElseIf I = 2 Or I = 3 Or I = 4 Or I = 5 Then
                Dim logOverLife1, logOverLife2 As Single
                Dim AA1, BB1 As Single

                logOverLife1 = CSng(Math.Log(OverlayLife1))
                logOverLife2 = CSng(Math.Log(OverlayLife))

                BB1 = (Th2 - Th1) / (logOverLife2 - logOverLife1)

                AA1 = CSng(Th1 + BB1 * (Math.Log(Life) - logOverLife1))

                If I > 2 Then
                    AA1 = CSng(AA1 * (1.0 + I / 200.0))
                End If

                Th2 = AA1
                'Th2 = Th1 * (coeff1 + coeff1 - 1)
            Else
                Dim zz1 As Single
                Th2 = Th2 * (coeff1 + coeff1 - 1 + coeff1 - 1)
                zz1 = (coeff1 + coeff1 - 1 + coeff1 - 1)

            End If

            Th1 = oldTh2
            Exit Sub

        End If


        If DesignType = FlexOnRigid Then
            If OverlayLife > Life Then
                If OverlayLife / Life > 60 Then
                    Th2 = Th1 * 0.815! * 0.7!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">20")
                ElseIf OverlayLife / Life > 20 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.815! * 0.8!
                    Else
                        Th2 = Th1 * 0.845! * 0.8!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 4 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.845! * 0.85!
                    Else
                        Th2 = Th1 * 0.895! * 0.85!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 1.76 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.895! * 0.9!
                    Else
                        Th2 = Th1 * 0.945! * 0.9!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">2")
                ElseIf OverlayLife / Life > 1.32 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.945! * 0.95!
                    Else
                        Th2 = Th1 * 0.975! * 0.95!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1.5")
                Else 'If OverlayLife / Life > 1 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.975!
                    Else
                        Th2 = Th1 * 0.985!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1")
                End If

            Else

                If OverlayLife / Life < 0.05 Then
                    Th2 = Th1 * 1.23! * 1.35!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.05")
                ElseIf OverlayLife / Life < 0.15 Then
                    Th2 = Th1 * 1.123! * 1.25!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.15")
                ElseIf OverlayLife / Life < 0.25 Then
                    Th2 = Th1 * 1.073! * 1.2!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.25")
                ElseIf OverlayLife / Life < 0.5 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.073! * 1.15!
                    Else
                        Th2 = Th1 * 1.0553! * 1.15!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.5")
                ElseIf OverlayLife / Life < 0.75 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.0553! * 1.1!
                    Else
                        Th2 = Th1 * 1.0273! * 1.1!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.75")
                Else
                    If I > 1 Then
                        Th2 = Th1 * 1.0273! * 1.05!
                    Else
                        Th2 = Th1 * 1.0123! * 1.05!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "---")
                End If

            End If

        Else

            If OverlayLife > Life Then

                If OverlayLife / Life > 60 Then
                    Th2 = Th1 * 0.815!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">20")
                ElseIf OverlayLife / Life > 20 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.815!
                    Else
                        Th2 = Th1 * 0.845!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 4 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.845!
                    Else
                        Th2 = Th1 * 0.895!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 1.76 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.895!
                    Else
                        Th2 = Th1 * 0.945!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">2")
                ElseIf OverlayLife / Life > 1.32 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.945!
                    Else
                        Th2 = Th1 * 0.975!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1.5")
                Else 'If OverlayLife / Life > 1 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.975!
                    Else
                        Th2 = Th1 * 0.985!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1")
                End If

            Else

                If OverlayLife / Life < 0.05 Then
                    Th2 = Th1 * 1.23!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.05")
                ElseIf OverlayLife / Life < 0.15 Then
                    Th2 = Th1 * 1.123!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.15")
                ElseIf OverlayLife / Life < 0.25 Then
                    Th2 = Th1 * 1.073!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.25")
                ElseIf OverlayLife / Life < 0.5 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.073!
                    Else
                        Th2 = Th1 * 1.0553!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.5")
                ElseIf OverlayLife / Life < 0.75 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.0553!
                    Else
                        Th2 = Th1 * 1.0273!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.75")
                Else
                    If I > 1 Then
                        Th2 = Th1 * 1.0273!
                    Else
                        Th2 = Th1 * 1.0123!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "---")
                End If

            End If

        End If
    End Sub








    Public Sub AdjustOverlayThickness(ByVal I As Integer, ByVal Th1 As Single, ByRef Th2 As Single, ByVal OverlayLife As Single)


        'Else
        'Th2 = Th1 - CSng(0.5) * OverlayLife / CSng(20)
        'End If

        'Else
        'Th2 = Th1 + CSng(0.6 * 20) / OverlayLife
        'Th2 = Th1 + CSng(1.5 * (20 - OverlayLife) / OverlayLife)
        'End If

        If DesignType = FlexOnRigid Then

            If OverlayLife > Life Then

                If OverlayLife / Life > 60 Then
                    Th2 = Th1 * 0.815! * 0.7!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">20")
                ElseIf OverlayLife / Life > 20 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.815! * 0.8!
                    Else
                        Th2 = Th1 * 0.845! * 0.8!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 4 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.845! * 0.85!
                    Else
                        Th2 = Th1 * 0.895! * 0.85!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 1.76 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.895! * 0.9!
                    Else
                        Th2 = Th1 * 0.945! * 0.9!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">2")
                ElseIf OverlayLife / Life > 1.32 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.945! * 0.95!
                    Else
                        Th2 = Th1 * 0.975! * 0.95!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1.5")
                Else 'If OverlayLife / Life > 1 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.975!
                    Else
                        Th2 = Th1 * 0.985!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1")
                End If

            Else

                If OverlayLife / Life < 0.05 Then
                    Th2 = Th1 * 1.23! * 1.35!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.05")
                ElseIf OverlayLife / Life < 0.15 Then
                    Th2 = Th1 * 1.123! * 1.25!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.15")
                ElseIf OverlayLife / Life < 0.25 Then
                    Th2 = Th1 * 1.073! * 1.2!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.25")
                ElseIf OverlayLife / Life < 0.5 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.073! * 1.15!
                    Else
                        Th2 = Th1 * 1.0553! * 1.15!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.5")
                ElseIf OverlayLife / Life < 0.75 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.0553! * 1.1!
                    Else
                        Th2 = Th1 * 1.0273! * 1.1!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.75")
                Else
                    If I > 1 Then
                        Th2 = Th1 * 1.0273! * 1.05!
                    Else
                        Th2 = Th1 * 1.0123! * 1.05!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "---")
                End If

            End If

        Else

            If OverlayLife > Life Then

                If OverlayLife / Life > 60 Then
                    Th2 = Th1 * 0.815!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">20")
                ElseIf OverlayLife / Life > 20 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.815!
                    Else
                        Th2 = Th1 * 0.845!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 4 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.845!
                    Else
                        Th2 = Th1 * 0.895!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">5")
                ElseIf OverlayLife / Life > 1.76 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.895!
                    Else
                        Th2 = Th1 * 0.945!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">2")
                ElseIf OverlayLife / Life > 1.32 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.945!
                    Else
                        Th2 = Th1 * 0.975!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1.5")
                Else 'If OverlayLife / Life > 1 Then
                    If I > 1 Then
                        Th2 = Th1 * 0.975!
                    Else
                        Th2 = Th1 * 0.985!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, ">1")
                End If

            Else

                If OverlayLife / Life < 0.05 Then
                    Th2 = Th1 * 1.23!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.05")
                ElseIf OverlayLife / Life < 0.15 Then
                    Th2 = Th1 * 1.123!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.15")
                ElseIf OverlayLife / Life < 0.25 Then
                    Th2 = Th1 * 1.073!
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.25")
                ElseIf OverlayLife / Life < 0.5 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.073!
                    Else
                        Th2 = Th1 * 1.0553!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.5")
                ElseIf OverlayLife / Life < 0.75 Then
                    If I > 1 Then
                        Th2 = Th1 * 1.0553!
                    Else
                        Th2 = Th1 * 1.0273!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "<0.75")
                Else
                    If I > 1 Then
                        Th2 = Th1 * 1.0273!
                    Else
                        Th2 = Th1 * 1.0123!
                    End If
                    'MsgBox(CStr(Th2), MsgBoxStyle.OkOnly, "---")
                End If

            End If


        End If

    End Sub


    Function LifeTotal_SelectFunction(ByRef iTh As Single, ByRef iNextraACout As Short, ByRef ct As CancellationToken) As Single

        'B = LeafLifeTotal_NP(Th2, NextraACin)
        'B = LeafLifeTotal_NP_new(Th2, NextraACin, QQQ1, WWW1)
        'B = PCConRigidLife2014(Th2, NextraACin)

        If DesignType = UnbondOnRigid Or DesignType = PartBondOnRigid Then
            'Unbonded concrete overlay on rigid
            LifeTotal_SelectFunction = LifeTotal_PCConRigid2014(iTh, iNextraACout, ct) '(1)
        Else
            'HMA overlay on rigid
            'SelectLifeFunctionB = LeafLifeTotal_NP(iTh, iNextraACout)
            LifeTotal_SelectFunction = LifeTotal_HMAonRigid2014(iTh, iNextraACout, ct)
        End If

    End Function



    Function LifeTotal_HMAonRigid2014(ByRef Th As Single, ByRef NextraACout As Short, ByRef ct As CancellationToken) As Single
        Dim CDFMAX, PCCSurface_Life1crack As Single
        Dim Overflow As Boolean
        Dim II As Short, TimeSave1, k1, k2 As Integer
        Dim Response(,), HorizStressResponse(,) As Double

        NextraAC = 0 ' See Sub LeafLifeTotal
        julThick(1) = Th
        Thick(1) = Th

        Call WriteFile()
        Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
        LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1) + Thick(2))
        LEAStrActiveX.EvalLayer = 2

        TimeSave1 = timeGetTime
        t1Thick = Now()

        'GoTo go123

        Dim RunAM As New AMClassLib.clsAM()   'Instance of AutoMesh.
        If DesignType = FlexOnRigid And (NAC + NextraAC) > 1000 Then
            Dim j1 As Integer
            ReDim Response(NAC + NextraAC, 2)
            Dim OneAC(1) As LEAFClassLib.clsLEAF.LEAFACParms
            Dim Resp1(1, 1) As Double

            For j1 = 1 To NAC + NextraAC
                If CalcStress(j1) = 0 Then
                    Call OneAircraft(CallAC(j1), OneAC(1))
                    RunAM = New AMClassLib.clsAM()
                    Dim CS(1) As Integer : CS(1) = 0
                    'Call RunAM.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, _
                    Call RunAM.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress,
                       CShort(1), OneAC, LEAStrActiveX, Resp1, VertStress, VertCoord, AllResp, CS,
                       DesignType, gSolverType, gSlabMeshSize, gOutputDirName, gUserInterrupted, NoOutFiles, ct) 'ik2020.03 JobName -> gOutputDirName LifeTotal_HMAonRigid2014
                    'added VertStress, VertCoord for rigid compaction by YGC 112213
                    Response(j1, 2) = Resp1(1, 2)
                    HorizStressResponse1(j1, 2) = Resp1(1, 2)
                Else
                    Response(j1, 2) = 0.0
                End If
                RunAM = Nothing
                Call FlushMemory1()

                If gUserInterrupted Then Exit Function
                '------------------------------------------------------------------
                If (InStr(CallAC(j1).ACname, "747", CompareMethod.Text) > 0) _
                And InStr(CallAC(j1).ACname, "Body", CompareMethod.Text) > 0 Then
                    Dim ACIndex2 As Integer, pos1 As Integer
                    pos1 = InStr(CallAC(j1).ACname, "Body", CompareMethod.Text) - 2
                    For ACIndex2 = 1 To CShort(j1 - 1) 'for all aircraft
                        If Mid(CallAC(j1).ACname, 1, pos1) = Mid(CallAC(ACIndex2).ACname, 1, pos1) _
                           And CallAC(j1).GearLoad = CallAC(ACIndex2).GearLoad Then
                            Response(j1, 2) = Response(ACIndex2, 2)
                        End If
                    Next ACIndex2
                End If
                '----------------------------------------------------------------
            Next
        Else
            'Call RunAM.ComputeResponse(
            Call RunAM.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress,
                CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse1,
                VertStress, VertCoord, AllResp, CalcStress, DesignType, gSolverType,
                gSlabMeshSize, gOutputDirName, gUserInterrupted, NoOutFiles, ct) 'ik2020.03 JobName -> gOutputDirName LifeTotal_HMAonRigid2014
            'added VertStress, VertCoord for rigid compaction by YGC 112213

            RunAM = Nothing
            Call FlushMemory1()
            If gUserInterrupted Then Exit Function
        End If

        'added for computing larger stress between LEAF and NIKE3D for design by YGC 102213
        RunLEAF = New LEAFClassLib.clsLEAF()
        LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1) + Thick(2))
        LEAStrActiveX.EvalLayer = 2
        Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse3, AllResp)
        RunLEAF = Nothing
        'ReDim HorizStressResponse3(NAC + NextraAC, 1)


        Dim j2 As Integer
        ReDim Response(NAC + NextraAC, 2)
        For j2 = 1 To NAC + NextraAC
            'Response(j2, 1) = Math.Max(HorizStressResponse1(j2, 1), HorizStressResponse2(j2, 1))  'overlay

            HorizStressResponse3(j2, 1) = HorizStressResponse3(j2, 1) * 0.95 '3 OVL
            Response(j2, 2) = Math.Max(HorizStressResponse1(j2, 2), HorizStressResponse3(j2, 1))  'underlay 
        Next
        'add ended for computing larger stress between LEAF and NIKE3D for design by YGC 102213


        ReDim HorizStressResponse(NAC + NextraAC, 1)
        For II = 1 To CShort(NAC + NextraAC)
            OverlayStress(II) = Response(II, 1)
            PCCStress(II) = Response(II, 2)
            HorizStressResponse(II, 1) = Response(II, 2)
        Next II


go123:
        'to comment STRT(1, 1)
        'ReDim HorizStressResponse(NAC + NextraAC, 1)
        'HorizStressResponse(1, 1) = 462.7980845
        'HorizStressResponse(2, 1) = 478.3371122
        'HorizStressResponse(3, 1) = 520.3758507
        'HorizStressResponse(4, 1) = 478.3371122

        RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1) '02
        ' Sets STRSH() = Max HorizStressResponse().
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(2), 100)
        PCCSurface_Life1crack = Life / CSng(CDFMAX)

        Call CompforStab(FSlope, 3)
        'FSlope = 1
        FSlopeComp = CSng((0.392 - FSlope * 0.3881) / (FSlope * 0.0039))


        If LifeExistPCC < 100 Then
            T1min = PCCSurface_Life1crack * (100 - LifeExistPCC) * 0.01!
            S = LayerType(LCode(1))
        Else
            T1min = 0.0!
        End If



        '    ********** Calculating T2min **********
        Dim LifeToB, LifeToE As Single
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(2), SCIB)
        LifeToB = Life / CDFMAX
        gLifeToB = LifeToB

        For k1 = 1 To NAC + NextraAC
            For k2 = 1 To 41
                CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
            Next
        Next
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(2), SCIE)
        LifeToE = Life / CDFMAX
        'gLifeToE = LifeToE
        T2min = LifeToE - LifeToB

        'ik2020.06.15 added
        For k1 = 1 To NAC + NextraAC + 1
            For k2 = 1 To 41
                CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)

            Next
        Next



        T2min = LifeToE - LifeToB
        If PCCSurface_Life1crack > 1000000 And (T2min = 0) Then
            T2min = 100000000 'LifeTotal_HMAonRigid2014
        End If

        LifeTotal_HMAonRigid2014 = T1min + T2min - Life
        NextraACout = CShort(NextraAC)


        Dim scaling_ratio As Single

        scaling_ratio = (Life / (T1min + T2min)) / CDFMAX

        For ik As Integer = 1 To NAC
            CDFtableTemp(ik) = jobCDFtable(ISect, ik) * scaling_ratio
            CDFacrftMaxtableTemp(ik) = jobCDFacrftMaxtable(ISect, ik) * scaling_ratio
            jobCDFtable(ISect, ik) = jobCDFtable(ISect, ik) * scaling_ratio
            jobCDFacrftMaxtable(ISect, ik) = jobCDFacrftMaxtable(ISect, ik) * scaling_ratio
        Next
        CDFMAX = CDFMAX * scaling_ratio


        For k1 = 1 To NAC + NextraAC + 1
            For k2 = 1 To 41
                CDFdata2(1, k1, k2) = CDFdata2(1, k1, k2) * scaling_ratio
            Next
        Next
        If LifeComputation Then
            LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
            If LifeCounterForPCRRuns = 1 Then
                For k1 = 1 To NAC + NextraAC + 1
                    For k2 = 1 To 41
                        CDFdata3(1, k1, k2) = CDFdata2(1, k1, k2)
                    Next
                Next
                For ik As Integer = 1 To NAC
                    CDFtableTemp3(ik) = CDFtableTemp(ik)
                    CDFacrftMaxtableTemp3(ik) = CDFacrftMaxtableTemp(ik)
                Next
            End If
        End If

    End Function



    Function NtoFail1(ByVal IA1 As Integer, ByVal stress1 As Double, ByVal SCITemp1 As Double,
        ByVal subrFlexStrength1 As Double, ByRef Overflow1 As Boolean,
        ByVal ParamA As Single, ByVal ParamB As Single,
        ByVal ParamC As Single, ByVal ParamD As Single) As Double
        Dim AA, BB As Double, DesFactor As Double
        DesFactor = CSng(subrFlexStrength1 / (stress1 * gCalibrationFactor + 0.000000000001))
        AA = DesFactor - ((1.0 - SCITemp1 / 100.0) * (ParamA * ParamD - ParamB * ParamC) + FSlope * ParamB * ParamC) / ((1.0 - SCITemp1 / 100.0) * (ParamD - ParamB) + FSlope * ParamB)
        BB = (FSlope * ParamB * ParamD) / ((1.0 - SCITemp1 / 100.0) * (ParamD - ParamB) + FSlope * ParamB)
        Temp = CSng(AA / BB)
        LogCA2(IA1) = Temp ' GuoAdd
        'If Temp > 10.0! Then Temp = 10.0! Else Overflow = False
        If OverlayRigOnRig Then
            If Temp > 30.0! Then Temp = 30.0! Else Overflow1 = False 'LeafCDFRigid_2014
        Else
            If Temp > 10.0! Then Temp = 10.0! Else Overflow1 = False 'LeafCDFRigid_2014
        End If
        NtoFail1 = CSng(10.0! ^ Temp)

    End Function



End Module


