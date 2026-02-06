Option Strict On
Option Explicit On

Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Friend Module modPrint
    Friend Const c1000 As Integer = 100
    Friend Const lbsTokg As Double = 0.453592
    Friend Const MPaToPsi As Double = 145.03773773

    'Rigid Pavements
    Friend StressDelta As Double = 0.2
    Friend gStress As Double
    'paragraph 1.1.3.2 b) Concrete working stress for rigid pavements
    Friend StressACN As Double = 2.75 * MPaToPsi '398.8537795

    Friend gComputeResponseAsym As Boolean = False
    Friend StrainsPrint(,) As Double

    Friend LeafResp As ACRClassLib.clsLEAF.LEAFoptions
    Friend LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms
    Friend AllResp(,) As ACRClassLib.clsLEAF.LEAFAllResponses  'Dimensioned and returned by LEAF
    Friend RunLEAF As ACRClassLib.clsLEAF

    Friend div1 As Single = 4 ' 4  '* 2 * 2 * 2 ' * 2
    Friend gPavementType As clsACR.PavementType

    Friend North, South, West, East As Single
    Friend WestEdge, SouthEdge As Single
    Friend iNorth, iSouth, iWest, iEast As Integer
    Friend horE_Limit, vertN_Limit As Integer
    Friend horW_Limit, vertS_Limit As Integer
    Friend horStep, vertStep As Single
    Friend horMiddle, vertMiddle As Single

    Friend iMax, iMaxGear As Integer
    Friend iHorGear, jVertGear As Integer
    Friend xHorGear, yVertGear As Single


    Friend CoverageDelta As Double = 1.0
    Friend gStrainTarget As Double
    Friend gStrainDelta As Double
    Friend gStrainMax As Double

    Friend WDir1 As String
    Friend gPrintOutput_ACN As Boolean = False
    Friend gPrintOutput_Responses As Boolean = False
    Friend gPrintOutput As Boolean = False



    Friend gCat As Integer
    Friend Const FileExt As String = ".txt"

    Friend StrainMax, StrainMaxSWL, StrainMaxGear As Double
    Friend Response(,) As Double

    Friend StressGear, StressSWL As Double
    Friend RRR(,) As Single

    'paragraph 1.1.3.8 Flexible Pavement 36,500 passes
    Friend CovACN As Double = 36500 '36500 10000
    'Friend CovACN_New As Single = 500000
    Friend gFlexMinThick As Single = 40 / 2.54 / 10 '1.0

    Friend Const StrainACN As Double = 0.00132524078262523
    Friend CovSWL, CovGear As Double

    Friend gThickness3 As Double

    Friend gTimeSave1, gTimeSave2 As Integer
    Friend gT1, gT2, gT3, gT4, gT5, gT6, gT7, gT8, gT9, gT10 As Integer
    Friend gT11, gT12, gT13, gT14, gT15, gT16, gT17, gT18, gT19, gT20 As Integer
    Friend ETimemsecs As Single

    Friend Declare Function timeGetTime Lib "winmm" () As Integer


    Private Sub Check999(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        'If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer, FileName As String
        FNo = FreeFile()
        Call Check_WDir1_Directory()

        FileName = WDir1 & "\Cat_LEAStr333" & gCat & FileExt

        FileOpen(FNo, FileName, OpenMode.Append) '
        Print(FNo, LPad(7, Format(LEAStrActiveX.Thick(2), "##0.00")))
        Print(FNo, LPad(7, Format(LEAStrActiveX.Thick(3), "##0.00")))
        Print(FNo, LPad(15, Format(LEAStrActiveX.Modulus(2), "###,###.00")))
        Print(FNo, LPad(15, Format(LEAStrActiveX.Modulus(3), "###,###.00")))
        PrintLine(FNo, "")
        FileClose(FNo)



    End Sub




    Friend Function Flex_Thick_Func(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms) As Double
        Flex_Thick_Func = 0

        For i = 1 To LEAStrActiveX.NLayers - 1
            Flex_Thick_Func = Flex_Thick_Func + LEAStrActiveX.Thick(i)
        Next

    End Function


    Friend Function GetThick(iTh As Integer) As Double

        'If gCat = 4 Then
        '    GetThick = LEAStrActiveX.Thick(2)
        'Else
        '    GetThick = gThickness3
        'End If

        GetThick = gThickness3


    End Function

    Friend Sub Print_Strain_Array_Full_Mesh(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.00000000"
        FNo = FreeFile()

        Dim ccc As String
        'ccc = CSng(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / 1500, 2))
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & "\Strains_" & gCat & "_" & ccc
        FileName = FileName & "_Full_Mesh" & FileExt
        'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt

        FileOpen(9, FileName, OpenMode.Output) 'Strains Full Mesh

        Dim i1, j1, ind1 As Integer
        Print(9, LPad(Width, ""))

        'For i1 = iWest To iEast Step 1
        For i1 = 0 To iEast - iWest Step 1
            'Print(9, LPad(Width, Format(i1 * horStep + horMiddle, Zs)))
            Print(9, LPad(Width, Format(i1 * horStep + West, Zs)))
        Next
        PrintLine(9, "")

        '===========================================================================
        'For j1 = iSouth To iNorth
        For j1 = 0 To iNorth - iSouth
            Print(9, LPad(Width, Format(j1 * vertStep + South, Zs)))
            'For i1 = iWest To iEast
            For i1 = 0 To iEast - iWest
                'ind1 = i1 - iWest + 1 + (j1 - iSouth) * (iEast - iWest + 1)
                ind1 = i1 + 1 + (j1) * (iEast - iWest + 1)
                Print(9, LPad(Width, Format(Response(1, ind1), Zs)))
            Next
            PrintLine(9, "")
        Next

        'PrintLine(9, "") : PrintLine(9, "")
        'For i1 = 1 To CallAC(1).NTires
        '    Print(9, LPad(5, CStr(i1)))
        '    Print(9, LPad(13, Format(CopyAC(1).TireX(i1), "##0.000")))
        '    PrintLine(9, LPad(13, Format(CopyAC(1).TireY(i1), "##0.000")))
        'Next

        PrintLine(9, "")
        PrintLine(9, "EvalDepth = " & LEAStrActiveX.EvalDepth)
        PrintLine(9, "Max Strain= " & Format(StrainMaxGear, Zs))
        FileClose(9)


    End Sub


    Private Sub Print_Strain_Array(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer
        Dim FileName As String
        FNo = FreeFile()

        Dim ccc As String
        'ccc = CSng(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / 1500, 2))
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & "\Strains_" & gCat & "_" & ccc & FileExt
        FileOpen(9, FileName, OpenMode.Output) 'Print_Stress_Array


        Dim i1, j1 As Integer

        Print(9, LPad(13, ""))
        For i1 = 0 To horE_Limit Step 1
            Print(9, LPad(13, Format(i1 * horStep, "#,###.000000000")))
        Next
        PrintLine(9, "")

        i1 = 0 : j1 = 0
        For j2 As Integer = 1 To (vertN_Limit + 1) * (horE_Limit + 1) Step 1

            If i1 = 0 Then
                Print(9, LPad(13, Format(j1 * vertStep, "#,###.000000000")))
            End If

            Print(9, LPad(13, Format(Response(1, j2), "#,###.000000000")))
            i1 = i1 + 1
            If i1 = horE_Limit + 1 Then
                PrintLine(9, "")
                i1 = 0
                j1 = j1 + 1
            End If

        Next

        PrintLine(9, "")
        PrintLine(9, "")
        PrintLine(9, LEAStrActiveX.EvalDepth)
        FileClose(9)


    End Sub


    Private Sub Print_Strain_Array_Ysym(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.00000000"
        FNo = FreeFile()

        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & "\Strains_" & gCat & "_" & ccc
        FileName = FileName & "_Full_MeshY" & FileExt
        'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt

        FileOpen(9, FileName, OpenMode.Output) 'Strains Full Mesh

        Dim i1, j1, ind1 As Integer
        Print(9, LPad(Width, ""))


        For i1 = 0 To iEast - iWest Step 1
            Print(9, LPad(Width, Format(i1 * horStep + West, Zs)))
        Next
        PrintLine(9, "")

        '===========================================================================
        'For j1 = iSouth To iNorth
        For j1 = 0 To iNorth - iSouth
            Print(9, LPad(Width, Format(j1 * vertStep + South, Zs)))
            'For i1 = iWest To iEast
            For i1 = 0 To iEast - iWest
                ind1 = i1 + 1 + (j1) * (iEast - iWest + 1)
                Print(9, LPad(Width, Format(Response(1, ind1), Zs)))
            Next
            PrintLine(9, "")
        Next

        PrintLine(9, "")
        PrintLine(9, "EvalDepth = " & LEAStrActiveX.Thick(1))
        PrintLine(9, "Max Strain= " & Format(StressGear, Zs))
        FileClose(9)


    End Sub



    Friend Sub Print_Strain_Array_XYsym(ByVal Full As Boolean, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                        ByVal Response(,) As Double, ByRef CallAC() As clsLEAF.LEAFACParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.00000000"
        FNo = FreeFile()

        Dim ccc As String
        'ccc = CSng(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / 1500, 2))
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        If Full Then
            FileName = WDir1 & "\Strains_" & gCat & "_" & ccc
            FileName = FileName & "_Full2" & FileExt
            'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt
        Else
            FileName = WDir1 & "\Strains_" & gCat & "_" & ccc & FileExt
        End If
        FileOpen(9, FileName, OpenMode.Output) 'Strains XYsym

        Dim maxStrain1 As Double = 10000000000.0
        Dim xMax1, yMax1 As Single

        For i As Integer = 1 To UBound(Response, 2)
            If maxStrain1 > Response(1, i) Then
                maxStrain1 = Response(1, i)
            End If
        Next


        Dim i1, j1 As Integer, i1Start As Integer = 0
        If Full Then i1Start = -horE_Limit

        Print(9, LPad(Width, ""))
        For i1 = i1Start To horE_Limit Step 1
            Print(9, LPad(Width, Format(i1 * horStep + horMiddle, Zs)))
        Next
        PrintLine(9, "")
        '===========================================================================

        If Full Then
            For j1 = vertN_Limit + 1 To 2 Step -1
                Print(9, LPad(Width, Format(-(j1 - 1) * vertStep + vertMiddle, "#,###.00000")))
                For i1 = horE_Limit + 1 To 2 Step -1
                    Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next

                For i1 = 1 To horE_Limit + 1
                    Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next
                PrintLine(9, "")
            Next
        End If

        For j1 = 1 To vertN_Limit + 1
            Print(9, LPad(Width, Format((j1 - 1) * vertStep + vertMiddle, "#,###.00000")))

            If Full Then
                For i1 = horE_Limit + 1 To 2 Step -1
                    Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next
            End If

            For i1 = 1 To horE_Limit + 1
                'Print(9, LPad(Width, CStr(i1 + (j1 - 1) * (horLimit+1))))
                Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                If maxStrain1 = Response(1, i1 + (j1 - 1) * (horE_Limit + 1)) Then
                    xMax1 = horMiddle + (i1 - 1) * horStep
                    yMax1 = vertMiddle + (j1 - 1) * vertStep
                End If
            Next
            PrintLine(9, "")
        Next

        'If Full Then : i1Start = -horE_Limit : Else : i1Start = 0 : End If
        PrintLine(9, "") : PrintLine(9, "")

        If iSelectEval_ACN = 3 Then
            'Print(9, LPad(11, ""))
            For i1 = CallAC(1).NEvalPoints - CallAC(1).NTires + 1 To CallAC(1).NEvalPoints
                PrintLine(9, LPad(5, CStr(i1)) & LPad(Width, Format(Response(1, i1), Zs)))
            Next
            PrintLine(9, "")

        End If
        'Print(9, LPad(11, ""))
        'For i1 = 1 To CallAC(1).NTires
        '    Print(9, LPad(11, Format(CopyAC(1).TireX(i1), "##0.000")))
        'Next
        'PrintLine(9, "")

        'For i1 = 1 To CallAC(1).NTires
        '    Print(9, LPad(11, Format(CopyAC(1).TireY(i1), "##0.000")))
        '    For j1 = 1 To CallAC(1).NTires
        '        Print(9, LPad(11, Format(0, "##0.000")))
        '    Next
        '    PrintLine(9, "")
        'Next


        PrintLine(9, "")
        PrintLine(9, "EvalDepth = " & LEAStrActiveX.EvalDepth)
        PrintLine(9, "Max Strain= " & Format(StrainMaxGear, Zs))
        PrintLine(9, "Max S1    = " & Format(maxStrain1, Zs))
        PrintLine(9, "X1= " & xMax1 & "   Y1= " & yMax1)
        FileClose(9)


    End Sub

    Friend Sub Print_Strain_Array_Ysym(ByVal Full As Boolean, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                     ByVal Response(,) As Double, ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.00000000"
        FNo = FreeFile()

        Dim ccc As String
        'ccc = CSng(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / 1500, 2))
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        If Full Then
            FileName = WDir1 & "\Strains_" & gCat & "_" & ccc
            FileName = FileName & "_Full2" & FileExt
            'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt
        Else
            FileName = WDir1 & "\Strains_" & gCat & "_" & ccc & FileExt
        End If
        FileOpen(9, FileName, OpenMode.Output) 'Strains XYsym

        Dim maxStrain1 As Double = 10000000000.0
        Dim xMax1, yMax1 As Single

        For i As Integer = 1 To UBound(Response, 2)
            If maxStrain1 > Response(1, i) Then
                maxStrain1 = Response(1, i)
            End If
        Next


        Dim i1, j1 As Integer, i1Start As Integer = 0
        If Full Then i1Start = -horE_Limit

        Print(9, LPad(Width, ""))
        For i1 = i1Start To horE_Limit Step 1
            Print(9, LPad(Width, Format(i1 * horStep + horMiddle, Zs)))
        Next
        PrintLine(9, "")
        '===========================================================================

        If Full Then
            For j1 = vertN_Limit + 1 To 2 Step -1
                Print(9, LPad(Width, Format(-(j1 - 1) * vertStep + vertMiddle, "#,###.00000")))
                For i1 = horE_Limit + 1 To 2 Step -1
                    Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next

                For i1 = 1 To horE_Limit + 1
                    Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next
                PrintLine(9, "")
            Next
        End If

        For j1 = 1 To vertN_Limit + 1
            'Print(9, LPad(Width, Format((j1 - 1) * vertStep + vertMiddle, "#,###.00000")))
            Print(9, LPad(Width, Format((j1 - 1) * vertStep, "#,###.00000")))

            If Full Then
                For i1 = horE_Limit + 1 To 2 Step -1
                    Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next
            End If

            For i1 = 1 To horE_Limit + 1
                'Print(9, LPad(Width, CStr(i1 + (j1 - 1) * (horLimit+1))))
                Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                If maxStrain1 = Response(1, i1 + (j1 - 1) * (horE_Limit + 1)) Then
                    xMax1 = horMiddle + (i1 - 1) * horStep
                    'yMax1 = vertMiddle + (j1 - 1) * vertStep
                    yMax1 = (j1 - 1) * vertStep
                End If
            Next
            PrintLine(9, "")
        Next

        'If Full Then : i1Start = -horE_Limit : Else : i1Start = 0 : End If
        PrintLine(9, "") : PrintLine(9, "")

        'Print(9, LPad(11, ""))
        'For i1 = 1 To CallAC(1).NTires
        '    Print(9, LPad(11, Format(CopyAC(1).TireX(i1), "##0.000")))
        'Next
        'PrintLine(9, "")

        'For i1 = 1 To CallAC(1).NTires
        '    Print(9, LPad(11, Format(CopyAC(1).TireY(i1), "##0.000")))
        '    For j1 = 1 To CallAC(1).NTires
        '        Print(9, LPad(11, Format(0, "##0.000")))
        '    Next
        '    PrintLine(9, "")
        'Next

        Dim c2 As Integer
        Dim ss1 As String
        Dim s1 As String = "#0.00"

        PrintLine(9, "")
        c2 = 0
        For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
            c2 = c2 + 1
            'ss1 = LPad(7, Format(CallAC(1).EvalX(j1), s1)) & ":" & LPad(7, Format(CallAC(1).EvalY(j1), s1))
            'PrintLine(9, j1, TAB(9), ss1)
            PrintLine(9, c2 & "    " & Response(1, j1))
        Next

        PrintLine(9, "")
        PrintLine(9, "EvalDepth = " & LEAStrActiveX.EvalDepth)
        PrintLine(9, "Max Strain= " & Format(StrainMaxGear, Zs))
        PrintLine(9, "Max S1    = " & Format(maxStrain1, Zs))
        PrintLine(9, "X1= " & xMax1 & "   Y1= " & yMax1)
        FileClose(9)


    End Sub


    Friend Sub Print_RRR_Array_Ysym(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.000"
        FNo = FreeFile()

        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & "\Stresses_" & gCat & "_" & ccc
        FileName = FileName & "_Full_Mesh" & FileExt
        'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt

        FileOpen(9, FileName, OpenMode.Output) 'Strains Full Mesh

        Dim i1, j1, ind1 As Integer
        Print(9, LPad(Width, ""))


        For i1 = horE_Limit To 1 Step -1
            Print(9, LPad(Width, Format(-i1 * horStep, Zs)))
        Next

        For i1 = 0 To horE_Limit Step 1
            Print(9, LPad(Width, Format(i1 * horStep, Zs)))
        Next
        PrintLine(9, "")

        '===========================================================================
        'For j1 = iSouth To iNorth
        For j1 = 0 To iNorth - iSouth
            Print(9, LPad(Width, Format(j1 * vertStep + South, Zs)))
            'For i1 = iWest To iEast

            For i1 = horE_Limit To 1 Step -1
                'ind1 = i1 + 1 + (j1) * (horE_Limit + 1)
                Print(9, LPad(Width, Format(RRR(i1, j1), Zs)))
            Next

            For i1 = 0 To horE_Limit
                'ind1 = i1 + 1 + (j1) * (horE_Limit + 1)
                Print(9, LPad(Width, Format(RRR(i1, j1), Zs)))
            Next
            PrintLine(9, "")
        Next

        PrintLine(9, "")
        PrintLine(9, "EvalDepth = " & LEAStrActiveX.Thick(1))
        PrintLine(9, "Max Stress= " & Format(StressGear, Zs))
        FileClose(9)

    End Sub


    Friend Sub Print_Results_Rigid_2gears(ByVal gross_weight As Single,
                                 ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                 ByVal CopyAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                 ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer
        Dim FileName As String
        FNo = FreeFile()

        'Call Check_for_Subdirectory()
        Call Check_WDir1_Directory()

        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")
        FileName = WDir1 & "\Cat_" & gCat & "_" & ccc & FileExt

        FileOpen(9, FileName, OpenMode.Output) 'Cat_1234
        'Print(9, LPad(4, CStr(5555)))

        PrintLine(9, "     Layers= " & LPad(3, CStr(LEAStrActiveX.NLayers)))
        PrintLine(9, " Eval Layer= " & LPad(3, CStr(LEAStrActiveX.EvalLayer)))
        PrintLine(9, " Eval Depth= " & LPad(9, Format(LEAStrActiveX.EvalDepth, "##0.00000")))

        PrintLine(9, "")

        For i As Integer = 1 To LEAStrActiveX.NLayers
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(7, Format(LEAStrActiveX.Thick(i), "##0.00")))
            Print(9, LPad(15, Format(LEAStrActiveX.Modulus(i), "###,###.00")))
            Print(9, LPad(7, Format(LEAStrActiveX.Poisson(i), "##0.00")))
            PrintLine(9, LPad(7, Format(LEAStrActiveX.InterfaceParm(i), "##0.00")))
        Next

        PrintLine(9, "") : PrintLine(9, "")

        Print(9, LPad(19, " Stress to match= "))
        PrintLine(9, LPad(15, Format(StressACN, "##0.0000")) & LPad(15, Format(StressACN - StressACN, "##0.0000")))

        Print(9, LPad(19, "            Gear= "))
        PrintLine(9, LPad(15, Format(StressGear, "##0.0000")) & LPad(15, Format(StressACN - StressGear, "##0.0000")))

        Print(9, LPad(19, "            DSWL= "))
        PrintLine(9, LPad(15, Format(StressSWL, "##0.0000")) & LPad(15, Format(StressACN - StressSWL, "##0.0000")))


        PrintLine(9, "")
        PrintLine(9, "")

        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "AC   Load=" & LPad(17, Format(gross_weight, "#,###.00000")))
        'PrintLine(9, "Gear Load=" & LPad(17, Format(CopyAC(1).GearLoad, "#,###.00000")))
        'PrintLine(9, "")
        'PrintLine(9, "N Tires=" & LPad(5, Format(CopyAC(1).NTires, "###")))
        ''PrintLine(9, "")

        'Print(9, LPad(5, "Tire#"))
        'Print(9, LPad(13, "X-coord"))
        'Print(9, LPad(13, "Y-coord"))
        'PrintLine(9, LPad(13, "Pressure"))

        'For i As Integer = 1 To CopyAC(1).NTires
        '    Print(9, LPad(5, Format(i, "###")))
        '    Print(9, LPad(13, Format(CopyAC(1).TireX(i), "##0.000")))
        '    Print(9, LPad(13, Format(CopyAC(1).TireY(i), "##0.000")))
        '    PrintLine(9, LPad(13, Format(CopyAC(1).TirePress(i), "###.000")))
        'Next

        PrintLine(9, "")
        PrintLine(9, "First five evaluation points!")
        PrintLine(9, "N Eval=" & LPad(7, Format(CopyAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CopyAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CopyAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CopyAC(1).EvalY(i), "##0.000")))
        Next

        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "SWL Load=" & LPad(17, Format(CallAC(1).GearLoad, "#,###.00000")))
        PrintLine(9, "")
        PrintLine(9, "N Tires=" & LPad(5, Format(CallAC(1).NTires, "###")))

        For i As Integer = 1 To CallAC(1).NTires
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).TireX(i), "##0.000")))
            Print(9, LPad(13, Format(CallAC(1).TireY(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).TirePress(i), "###.000")))
        Next

        'PrintLine(9, "")
        PrintLine(9, "N Eval=" & LPad(5, Format(CallAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CallAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).EvalY(i), "##0.000")))
        Next

        FileClose(9)



    End Sub




    Friend Sub Print_Results_Rigid(ByVal gross_weight As Single,
                                   ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                   ByVal CopyAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                   ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer
        Dim FileName As String
        FNo = FreeFile()


        'Call Check_for_Subdirectory()
        Call Check_WDir1_Directory()

        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")
        'FileName = WDir1 & "\Cat_" & gCat & "_" & ccc & FileExt
        FileName = WDir1 & "\" & gFileName & "_Cat_" & gCat & "_" & ccc & FileExt

        FileOpen(9, FileName, OpenMode.Output) 'Cat_1234
        'Print(9, LPad(4, CStr(5555)))

        PrintLine(9, "     Layers= " & LPad(3, CStr(LEAStrActiveX.NLayers)))
        PrintLine(9, " Eval Layer= " & LPad(3, CStr(LEAStrActiveX.EvalLayer)))
        PrintLine(9, " Eval Depth= " & LPad(9, Format(LEAStrActiveX.EvalDepth, "##0.00000")))

        PrintLine(9, "")

        For i As Integer = 1 To LEAStrActiveX.NLayers
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(7, Format(LEAStrActiveX.Thick(i), "##0.00")))
            Print(9, LPad(15, Format(LEAStrActiveX.Modulus(i), "###,###.00")))
            Print(9, LPad(7, Format(LEAStrActiveX.Poisson(i), "##0.00")))
            PrintLine(9, LPad(7, Format(LEAStrActiveX.InterfaceParm(i), "##0.00")))
        Next

        PrintLine(9, "") : PrintLine(9, "")

        Print(9, LPad(19, " Stress to match= "))
        PrintLine(9, LPad(15, Format(StressACN, "##0.0000")) & LPad(15, Format(StressACN - StressACN, "##0.0000")))

        Print(9, LPad(19, "            Gear= "))
        PrintLine(9, LPad(15, Format(StressGear, "##0.0000")) & LPad(15, Format(StressACN - StressGear, "##0.0000")))

        Print(9, LPad(19, "            DSWL= "))
        PrintLine(9, LPad(15, Format(StressSWL, "##0.0000")) & LPad(15, Format(StressACN - StressSWL, "##0.0000")))


        PrintLine(9, "")
        PrintLine(9, "")

        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "AC   Load=" & LPad(17, Format(gross_weight, "#,###.00000")))
        PrintLine(9, "Gear Load=" & LPad(17, Format(CopyAC(1).GearLoad, "#,###.00000")))
        PrintLine(9, "")
        PrintLine(9, "N Tires=" & LPad(5, Format(CopyAC(1).NTires, "###")))
        'PrintLine(9, "")

        Print(9, LPad(5, "Tire#"))
        Print(9, LPad(13, "X-coord"))
        Print(9, LPad(13, "Y-coord"))
        PrintLine(9, LPad(13, "Pressure"))

        For i As Integer = 1 To CopyAC(1).NTires
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CopyAC(1).TireX(i), "##0.000")))
            Print(9, LPad(13, Format(CopyAC(1).TireY(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CopyAC(1).TirePress(i), "###.000")))
        Next

        PrintLine(9, "")
        PrintLine(9, "First five evaluation points!")
        PrintLine(9, "N Eval=" & LPad(7, Format(CopyAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CopyAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CopyAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CopyAC(1).EvalY(i), "##0.000")))
        Next

        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "SWL Load=" & LPad(17, Format(CallAC(1).GearLoad, "#,###.00000")))
        PrintLine(9, "")
        PrintLine(9, "N Tires=" & LPad(5, Format(CallAC(1).NTires, "###")))

        For i As Integer = 1 To CallAC(1).NTires
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).TireX(i), "##0.000")))
            Print(9, LPad(13, Format(CallAC(1).TireY(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).TirePress(i), "###.000")))
        Next

        'PrintLine(9, "")
        PrintLine(9, "N Eval=" & LPad(5, Format(CallAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CallAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).EvalY(i), "##0.000")))
        Next

        FileClose(9)



    End Sub


    Friend Sub Print_Results_Flex(ByVal gross_weight As Single,
                                   ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                   ByVal CopyAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                   ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not bMeshSize Then
            If Not gPrintOutput_ACN Then Exit Sub
        End If
        'If Not gPrintOutput Then Exit Sub

        Dim FNo As Integer
        Dim FileName As String
        FNo = FreeFile()

        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        'FileName = WDir1 & "\Cat_" & gCat & "_" & ccc & FileExt
        FileName = WDir1 & "\" & gFileName & "_Cat_" & gCat & "_" & ccc & FileExt

        FileOpen(9, FileName, OpenMode.Output) 'Cat_1234
        'Print(9, LPad(4, CStr(5555)))


        PrintLine(9, "     Layers= " & LPad(3, CStr(LEAStrActiveX.NLayers)))
        PrintLine(9, " Eval Layer= " & LPad(3, CStr(LEAStrActiveX.EvalLayer)))
        PrintLine(9, " Eval Depth= " & LPad(9, Format(LEAStrActiveX.EvalDepth, "##0.00000")))

        Dim ttt As Double = 0.0
        For i7 As Integer = 1 To LEAStrActiveX.NLayers - 1
            ttt = ttt + LEAStrActiveX.Thick(i7)
        Next
        PrintLine(9, " Tol Thick = " & LPad(9, Format(ttt, "##0.00000")))
        PrintLine(9, " Differ    = " & LPad(9, Format(LEAStrActiveX.EvalDepth - ttt, "##0.00000")))


        PrintLine(9, "")

        For i As Integer = 1 To LEAStrActiveX.NLayers
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(7, Format(LEAStrActiveX.Thick(i), "##0.00")))
            Print(9, LPad(15, Format(LEAStrActiveX.Modulus(i), "###,###.00")))
            Print(9, LPad(7, Format(LEAStrActiveX.Poisson(i), "##0.00")))
            PrintLine(9, LPad(7, Format(LEAStrActiveX.InterfaceParm(i), "##0.00")))
        Next

        PrintLine(9, "")
        PrintLine(9, "")
        PrintLine(9, "Coverages to match= " & Format(CovACN, "#,###"))
        PrintLine(9, "")


        PrintLine(9, LPad(12, " ") & LPad(15, "Strain" & LPad(16, "Coverages")))
        Print(9, LPad(5, "Gear"))
        Print(9, LPad(15, Format(StrainMaxGear, "##0.000000000")))
        PrintLine(9, LPad(15, Format(CovGear, "###,##0.00000")))

        Print(9, LPad(5, "DSWL"))
        Print(9, LPad(15, Format(StrainMaxSWL, "##0.000000000")))
        PrintLine(9, LPad(15, Format(CovSWL, "###,##0.00000")))

        'PrintLine(9, "")
        'PrintLine(9, "Max Strain Location")
        'PrintLine(9, "iMaxGear =" & LPad(5, Format(iMaxGear, "###,##0")))
        'PrintLine(9, "iHorGear =" & LPad(5, Format(iHorGear, "###,##0")))
        'PrintLine(9, "jVertGear=" & LPad(5, Format(jVertGear, "###,##0")))
        'PrintLine(9, "xHorGear =" & LPad(11, Format(xHorGear, "##0.00000")))
        'PrintLine(9, "yVertGear=" & LPad(11, Format(yVertGear, "##0.00000")))


        PrintLine(9, "")

        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "AC   Load=" & LPad(17, Format(gross_weight, "#,###.00000")))
        PrintLine(9, "Gear Load=" & LPad(17, Format(CopyAC(1).GearLoad, "#,###.00000")))
        PrintLine(9, "")
        PrintLine(9, "N Tires=" & LPad(5, Format(CopyAC(1).NTires, "###")))
        'PrintLine(9, "")

        Print(9, LPad(5, "Tire#"))
        Print(9, LPad(13, "X-coord"))
        Print(9, LPad(13, "Y-coord"))
        PrintLine(9, LPad(13, "Pressure"))

        For i As Integer = 1 To CopyAC(1).NTires
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CopyAC(1).TireX(i), "##0.000")))
            Print(9, LPad(13, Format(CopyAC(1).TireY(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CopyAC(1).TirePress(i), "###.000")))
        Next

        PrintLine(9, "")
        PrintLine(9, "First five evaluation points!")
        PrintLine(9, "N Eval=" & LPad(7, Format(CopyAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CopyAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CopyAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CopyAC(1).EvalY(i), "##0.000")))
        Next




        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "SWL Load=" & LPad(17, Format(CallAC(1).GearLoad, "#,###.00000")))
        PrintLine(9, "")
        PrintLine(9, "N Tires=" & LPad(5, Format(CallAC(1).NTires, "###")))

        For i As Integer = 1 To CallAC(1).NTires
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).TireX(i), "##0.000")))
            Print(9, LPad(13, Format(CallAC(1).TireY(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).TirePress(i), "###.000")))
        Next

        'PrintLine(9, "")
        PrintLine(9, "N Eval=" & LPad(5, Format(CallAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CallAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).EvalY(i), "##0.000")))
        Next

        FileClose(9)


        '=====================================================================
        '=====================================================================
        'Dim Filename1 As String

        'Filename1 = WDir1 & "\Eval_Points" & FileExt
        'FileOpen(9, Filename1, OpenMode.Append) 'Eval_Points

        'Print(9, LPad(13, Format(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers), "###,###.00")))
        'Print(9, LPad(11, Format(xHorGear, "###,##0.00000")))
        'Print(9, LPad(11, Format(yVertGear, "###,##0.00000")))
        'Print(9, LPad(5, Format(iHorGear, "###,##0")))
        'Print(9, LPad(5, Format(jVertGear, "###,##0")))
        'Print(9, LPad(7, Format(iMaxGear, "###,##0")))
        'Print(9, LPad(20, Format(LEAStrActiveX.EvalDepth, "###,##0.0000000")))
        'PrintLine(9, "")
        'If gCat = 4 Then PrintLine(9, "")
        'FileClose(9)



    End Sub


    Friend Sub Print_Results_Flex_2gears(ByVal gross_weight As Single,
                                ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                ByVal CopyAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If Not bMeshSize Then
            If Not gPrintOutput_ACN Then Exit Sub
        End If
        'If Not gPrintOutput Then Exit Sub
        Call Check_for_Subdirectory()


        Dim FNo As Integer
        Dim FileName As String
        FNo = FreeFile()

        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & "\Cat_" & gCat & "_" & ccc & FileExt

        FileOpen(9, FileName, OpenMode.Output) 'Cat_1234
        'Print(9, LPad(4, CStr(5555)))


        PrintLine(9, "     Layers= " & LPad(3, CStr(LEAStrActiveX.NLayers)))
        PrintLine(9, " Eval Layer= " & LPad(3, CStr(LEAStrActiveX.EvalLayer)))
        PrintLine(9, " Eval Depth= " & LPad(9, Format(LEAStrActiveX.EvalDepth, "##0.00000")))

        Dim ttt As Double = 0.0
        For i7 As Integer = 1 To LEAStrActiveX.NLayers - 1
            ttt = ttt + LEAStrActiveX.Thick(i7)
        Next
        PrintLine(9, " Tol Thick = " & LPad(9, Format(ttt, "##0.00000")))
        PrintLine(9, " Differ    = " & LPad(9, Format(LEAStrActiveX.EvalDepth - ttt, "##0.00000")))


        PrintLine(9, "")

        For i As Integer = 1 To LEAStrActiveX.NLayers
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(7, Format(LEAStrActiveX.Thick(i), "##0.00")))
            Print(9, LPad(15, Format(LEAStrActiveX.Modulus(i), "###,###.00")))
            Print(9, LPad(7, Format(LEAStrActiveX.Poisson(i), "##0.00")))
            PrintLine(9, LPad(7, Format(LEAStrActiveX.InterfaceParm(i), "##0.00")))
        Next

        PrintLine(9, "")
        PrintLine(9, "")
        PrintLine(9, "Coverages to match= " & Format(CovACN, "#,###"))
        PrintLine(9, "")


        PrintLine(9, LPad(12, " ") & LPad(15, "Strain" & LPad(16, "Coverages")))
        Print(9, LPad(5, "Gear"))
        Print(9, LPad(15, Format(StrainMaxGear, "##0.000000000")))
        PrintLine(9, LPad(15, Format(CovGear, "###,##0.00000")))

        Print(9, LPad(5, "DSWL"))
        Print(9, LPad(15, Format(StrainMaxSWL, "##0.000000000")))
        PrintLine(9, LPad(15, Format(CovSWL, "###,##0.00000")))

        'PrintLine(9, "")
        'PrintLine(9, "Max Strain Location")
        'PrintLine(9, "iMaxGear =" & LPad(5, Format(iMaxGear, "###,##0")))
        'PrintLine(9, "iHorGear =" & LPad(5, Format(iHorGear, "###,##0")))
        'PrintLine(9, "jVertGear=" & LPad(5, Format(jVertGear, "###,##0")))
        'PrintLine(9, "xHorGear =" & LPad(11, Format(xHorGear, "##0.00000")))
        'PrintLine(9, "yVertGear=" & LPad(11, Format(yVertGear, "##0.00000")))


        PrintLine(9, "")

        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "AC   Load=" & LPad(17, Format(gross_weight, "#,###.00000")))
        'PrintLine(9, "Gear Load=" & LPad(17, Format(CopyAC(1).GearLoad, "#,###.00000")))
        'PrintLine(9, "")
        'PrintLine(9, "N Tires=" & LPad(5, Format(CopyAC(1).NTires, "###")))
        ''PrintLine(9, "")

        'Print(9, LPad(5, "Tire#"))
        'Print(9, LPad(13, "X-coord"))
        'Print(9, LPad(13, "Y-coord"))
        'PrintLine(9, LPad(13, "Pressure"))

        'For i As Integer = 1 To CopyAC(1).NTires
        '    Print(9, LPad(5, Format(i, "###")))
        '    Print(9, LPad(13, Format(CopyAC(1).TireX(i), "##0.000")))
        '    Print(9, LPad(13, Format(CopyAC(1).TireY(i), "##0.000")))
        '    PrintLine(9, LPad(13, Format(CopyAC(1).TirePress(i), "###.000")))
        'Next

        PrintLine(9, "")
        PrintLine(9, "First five evaluation points!")
        PrintLine(9, "N Eval=" & LPad(7, Format(CopyAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CopyAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CopyAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CopyAC(1).EvalY(i), "##0.000")))
        Next




        PrintLine(9, "")
        PrintLine(9, "=============================================")
        PrintLine(9, "SWL Load=" & LPad(17, Format(CallAC(1).GearLoad, "#,###.00000")))
        PrintLine(9, "")
        PrintLine(9, "N Tires=" & LPad(5, Format(CallAC(1).NTires, "###")))

        For i As Integer = 1 To CallAC(1).NTires
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).TireX(i), "##0.000")))
            Print(9, LPad(13, Format(CallAC(1).TireY(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).TirePress(i), "###.000")))
        Next

        'PrintLine(9, "")
        PrintLine(9, "N Eval=" & LPad(5, Format(CallAC(1).NEvalPoints, "###")))

        For i As Integer = 1 To Math.Min(CallAC(1).NEvalPoints, 5)
            Print(9, LPad(5, Format(i, "###")))
            Print(9, LPad(13, Format(CallAC(1).EvalX(i), "##0.000")))
            PrintLine(9, LPad(13, Format(CallAC(1).EvalY(i), "##0.000")))
        Next

        FileClose(9)


        '=====================================================================
        '=====================================================================
        'Dim Filename1 As String

        'Filename1 = WDir1 & "\Eval_Points" & FileExt
        'FileOpen(9, Filename1, OpenMode.Append) 'Eval_Points

        'Print(9, LPad(13, Format(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers), "###,###.00")))
        'Print(9, LPad(11, Format(xHorGear, "###,##0.00000")))
        'Print(9, LPad(11, Format(yVertGear, "###,##0.00000")))
        'Print(9, LPad(5, Format(iHorGear, "###,##0")))
        'Print(9, LPad(5, Format(jVertGear, "###,##0")))
        'Print(9, LPad(7, Format(iMaxGear, "###,##0")))
        'Print(9, LPad(20, Format(LEAStrActiveX.EvalDepth, "###,##0.0000000")))
        'PrintLine(9, "")
        'If gCat = 4 Then PrintLine(9, "")
        'FileClose(9)



    End Sub


    Friend Function PaveTypeF(ByVal PavementType1 As clsACR.PavementType) As String
        If PavementType1 = clsACR.PavementType.Flexible Then
            PaveTypeF = "Flexible"
        Else
            PaveTypeF = "Rigid"
        End If

    End Function



    Friend Sub Check_for_Subdirectory()

        If gPrintOutput_ACN Then
            'WDir1 = System.Windows.Forms.Application.StartupPath & "\ACR_Results_" & gPavementType
            WDir1 = SpecialDirectories.MyDocuments + "\My FAARFIELD\ACR_Results_" & PaveTypeF(gPavementType)
            If Not Directory.Exists(WDir1) Then
                System.IO.Directory.CreateDirectory(WDir1)
            End If
        End If

    End Sub

    Friend Sub Print_ACNtoFile(ByVal CopyAC() As ACRClassLib.clsLEAF.LEAFACParms,
                     ByVal CalculateACN As ACRClassLib.clsACR.ACRdata)

        If Not gPrintOutput_ACN Then Exit Sub
        Dim FileName, Date1 As String, FNo As Integer


        gTimeSave2 = timeGetTime
        ETimemsecs = CSng((gTimeSave2 - gTimeSave1) / 1000)
        FNo = FreeFile()

        Date1 = Year(Now) & "." & Format(Month(Now), "00") & "." & Format(Day(Now), "00")
        FileName = WDir1 & "\ACR_" & Date1 & FileExt

        FileOpen(FNo, FileName, OpenMode.Append) 'ACN_

        PrintLine(FNo, "")
        PrintLine(FNo, "Now= " & Now())
        PrintLine(FNo, PaveTypeF(gPavementType))
        Print(FNo, "#Wheels= " & CopyAC(1).NTires)
        PrintLine(FNo, "        Gear Load= " & Format(CopyAC(1).GearLoad, "##0.00") & " lbs")

        PrintLine(FNo, "")

        If gPavementType = clsACR.PavementType.Flexible And Not (iSelectEval_ACN = 1) Then
            Dim sizeX, sizeY As Single

            If (CopyAC(1).NTires > 1) And (gMeshSizeCount > 1) Then
                sizeX = CSng(CopyAC(1).EvalX(2) - CopyAC(1).EvalX(1))
                sizeY = CSng(CopyAC(1).EvalY(horE_Limit + 2) - CopyAC(1).EvalY(1))
                PrintLine(FNo, "Mesh size: " & Format(sizeX, "#0.000") & " x " & Format(sizeY, "#0.000") & " inches")

            End If


            If bMeshSize Then
                PrintLine(FNo, "Input Mesh Size: " & Format(gMeshSize, "#0.000"))
            Else
                PrintLine(FNo, "Input Div: " & Format(div1, "#0.000"))
            End If

            If Symmetry = SymmetryType.XYSymmetry Then 'Print_ACNtoFile
                PrintLine(FNo, "Gear symmetry: XY symmetry used")
            ElseIf Symmetry = SymmetryType.YSymmetry Then
                PrintLine(FNo, "Gear symmetry: Y symmetry used")
            Else
                PrintLine(FNo, "Gear symmetry: No symmetry used")
            End If

        End If

        PrintLine(FNo, "")

        Print(FNo, LPad(4, "")) : Print(FNo, LPad(11, "ACR   "))
        PrintLine(FNo, LPad(13, "Thickness") & " [in]")

        For i As Integer = 1 To UBound(CalculateACN.libACR, 1)
            Print(FNo, LPad(4, Format(i, "###")))
            Print(FNo, LPad(11, Format(CalculateACN.libACR(i), "###.00000")))
            PrintLine(FNo, LPad(13, Format(CalculateACN.libACRthick(i), "###.000000")))
            'PrintLine(FNo, LPad(7, Format(LEAStrActiveX.InterfaceParm(i), "###.00")))
        Next

        PrintLine(FNo, "")
        PrintLine(FNo, LPad(12, CStr(Format(ETimemsecs, "##,##0.000"))) & " sec.")
        PrintLine(FNo, LPad(12, CStr(Format(ETimemsecs / 60, "##,##0.000"))) & " min.")
        PrintLine(FNo, "============================")



        'Dim File2 As String = WDir1 & "\ACR_Summary" & FileExt
        'Dim FNo2 As Integer = FreeFile()
        'FileOpen(FNo2, File2, OpenMode.Append) 'ACN_

        'If UBound(CalculateACN.libACR, 1) = 1 Then
        '    If bMeshSize Then
        '        Print(FNo2, "Mesh= " & Format(gMeshSize, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
        '        PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Scode(gICAOCodeIndex))
        '    Else
        '        Print(FNo2, "Div1= " & Format(div1, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
        '        PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Scode(gICAOCodeIndex))
        '    End If
        'Else
        '    If bMeshSize Then
        '        Print(FNo2, "Mesh= " & Format(gMeshSize, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
        '        PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Format(CalculateACN.libACR(2), "###.00") & "  " & Format(CalculateACN.libACR(3), "###.00") & "  " & Format(CalculateACN.libACR(4), "###.00"))
        '    Else
        '        Print(FNo2, "Div1= " & Format(div1, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
        '        PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Format(CalculateACN.libACR(2), "###.00") & "  " & Format(CalculateACN.libACR(3), "###.00") & "  " & Format(CalculateACN.libACR(4), "###.00"))
        '    End If
        'End If

        'FileClose(FNo2)
        ''
        'PrintLine(FNo, "============================")
        ''PrintLine(FNo, (gT2 - gT1))
        ''PrintLine(FNo, (gT4 - gT3))
        ''PrintLine(FNo, (gT6 - gT5))
        ''PrintLine(FNo, (gT8 - gT7))
        'PrintLine(FNo, "")
        'FileClose(FNo)


    End Sub


    Friend Function Scode(ByVal int1 As Integer) As String

        If int1 = 1 Then
            Scode = "D"
        ElseIf int1 = 2 Then
            Scode = "C"
        ElseIf int1 = 3 Then
            Scode = "B"
        ElseIf int1 = 4 Then
            Scode = "A"
        Else
            Scode = "?"
        End If

    End Function


    Friend Sub Print_ACNtoFile_2gears(ByVal CopyAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                      ByVal CopyAC_belly() As ACRClassLib.clsLEAF.LEAFACParms,
                 ByVal CalculateACN As ACRClassLib.clsACR.ACRdata)

        If Not gPrintOutput_ACN Then Exit Sub
        Dim FileName, Date1 As String, FNo As Integer


        gTimeSave2 = timeGetTime
        ETimemsecs = CSng((gTimeSave2 - gTimeSave1) / 1000)
        FNo = FreeFile()

        Date1 = Year(Now) & "." & Format(Month(Now), "00") & "." & Format(Day(Now), "00")
        FileName = WDir1 & "\ACR_" & Date1 & FileExt

        FileOpen(FNo, FileName, OpenMode.Append) 'ACN_

        PrintLine(FNo, "")
        PrintLine(FNo, "Now= " & Now())
        PrintLine(FNo, PaveTypeF(gPavementType))
        Print(FNo, "#Wheels= " & CopyAC(1).NTires)
        PrintLine(FNo, "        Gear Load= " & Format(CopyAC(1).GearLoad, "##0.00") & " lbs")
        Print(FNo, "#Wheels= " & CopyAC_belly(1).NTires)
        PrintLine(FNo, "        Gear Load= " & Format(CopyAC_belly(1).GearLoad, "##0.00") & " lbs")

        PrintLine(FNo, "")
        If gPavementType = clsACR.PavementType.Flexible Then
            Dim sizeX, sizeY As Single
            sizeX = CSng(CopyAC(1).EvalX(2) - CopyAC(1).EvalX(1))
            sizeY = CSng(CopyAC(1).EvalY(horE_Limit + 2) - CopyAC(1).EvalY(1))
            PrintLine(FNo, "Mesh size: " & Format(sizeX, "#0.000") & " x " & Format(sizeY, "#0.000") & " inches")


            If bMeshSize Then
                PrintLine(FNo, "Input Mesh Size: " & Format(gMeshSize, "#0.000"))
            Else
                PrintLine(FNo, "Input Div: " & Format(div1, "#0.000"))
            End If



            If Symmetry = SymmetryType.XYSymmetry Then 'Print_ACNtoFile
                PrintLine(FNo, "Gear symmetry: XY symmetry used")
            ElseIf Symmetry = SymmetryType.YSymmetry Then
                PrintLine(FNo, "Gear symmetry: Y symmetry used")
            Else
                PrintLine(FNo, "Gear symmetry: No symmetry used")
            End If
        End If

        PrintLine(FNo, "")

        Print(FNo, LPad(4, "")) : Print(FNo, LPad(11, "ACN   "))
        PrintLine(FNo, LPad(13, "Thickness") & " [in]")

        For i As Integer = 1 To UBound(CalculateACN.libACR, 1)
            Print(FNo, LPad(4, Format(i, "###")))
            Print(FNo, LPad(11, Format(CalculateACN.libACR(i), "###.00000")))
            PrintLine(FNo, LPad(13, Format(CalculateACN.libACRthick(i), "###.000000")))
            'PrintLine(FNo, LPad(7, Format(LEAStrActiveX.InterfaceParm(i), "###.00")))
        Next

        PrintLine(FNo, "")
        PrintLine(FNo, LPad(12, CStr(Format(ETimemsecs, "##,##0.000"))) & " sec.")
        PrintLine(FNo, LPad(12, CStr(Format(ETimemsecs / 60, "##,##0.000"))) & " min.")
        PrintLine(FNo, "============================")
        PrintLine(FNo, "")
        FileClose(FNo)


        Dim File2 As String = WDir1 & "\ACR_Summary" & FileExt
        Dim FNo2 As Integer = FreeFile()
        FileOpen(FNo2, File2, OpenMode.Append) 'ACN_

        If UBound(CalculateACN.libACR, 1) = 1 Then
            If bMeshSize Then
                Print(FNo2, "Mesh= " & Format(gMeshSize, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
                PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Scode(gICAOCodeIndex))
            Else
                Print(FNo2, "Div1= " & Format(div1, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
                PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Scode(gICAOCodeIndex))
            End If
        Else
            If bMeshSize Then
                Print(FNo2, "Mesh= " & Format(gMeshSize, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
                PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Format(CalculateACN.libACR(2), "###.00") & "  " & Format(CalculateACN.libACR(3), "###.00") & "  " & Format(CalculateACN.libACR(4), "###.00"))
            Else
                Print(FNo2, "Div1= " & Format(div1, "#0.000") & "  " & LPad(5, CStr(Format(ETimemsecs, "##,##0.00"))) & "   ")
                PrintLine(FNo2, Format(CalculateACN.libACR(1), "###.00") & "  " & Format(CalculateACN.libACR(2), "###.00") & "  " & Format(CalculateACN.libACR(3), "###.00") & "  " & Format(CalculateACN.libACR(4), "###.00"))
            End If
        End If

        FileClose(FNo2)
        '
        'PrintLine(FNo, "============================")
        'PrintLine(FNo, (gT2 - gT1))
        'PrintLine(FNo, (gT4 - gT3))
        'PrintLine(FNo, (gT6 - gT5))
        'PrintLine(FNo, (gT8 - gT7))
        'PrintLine(FNo, "")
        'FileClose(FNo)





    End Sub


    Friend Sub Print_Array_Full_Mesh(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                 ByRef Response(,) As Double,
                                 ByRef CopyAC() As ACRClassLib.clsLEAF.LEAFACParms)

        If Not gPrintOutput_Responses Then Exit Sub 'to uncomment
        Exit Sub 'Print

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.00000000" 'flexible
        If gPavementType = clsACR.PavementType.Rigid Then Zs = "#,##0.00000" 'Rigid


        FNo = FreeFile()
        Dim ccc As String
        'ccc = CSng(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / 1500, 2))
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & "\Strains_" & gCat & "_" & ccc
        FileName = FileName & "_ZZZ_C17A" & FileExt
        'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt

        FileOpen(FNo, FileName, OpenMode.Output) 'Strains Full Mesh

        Dim i1, j1, ind1 As Integer
        Print(FNo, LPad(Width, ""))

        'For i1 = iWest To iEast Step 1
        For i1 = 0 To iEast - iWest Step 1
            'Print(FNo, LPad(Width, Format(i1 * horStep + horMiddle, Zs)))
            Print(FNo, LPad(Width, Format(i1 * horStep + West, Zs)))
        Next
        PrintLine(FNo, "")

        '===========================================================================
        'For j1 = iSouth To iNorth
        For j1 = 0 To iNorth - iSouth
            Print(FNo, LPad(Width, Format(j1 * vertStep + South, Zs)))
            'For i1 = iWest To iEast
            For i1 = 0 To iEast - iWest
                'ind1 = i1 - iWest + 1 + (j1 - iSouth) * (iEast - iWest + 1)
                ind1 = i1 + 1 + (j1) * (iEast - iWest + 1)
                Print(FNo, LPad(Width, Format(Response(1, ind1), Zs)))
            Next
            PrintLine(FNo, "")
        Next

        PrintLine(FNo, "") : PrintLine(FNo, "")
        For i1 = 1 To CopyAC(1).NTires
            Print(FNo, LPad(5, CStr(i1))) : j1 = CopyAC(1).NEvalPoints - CopyAC(1).NTires + i1
            Print(FNo, LPad(13, Format(CopyAC(1).TireX(i1), "##0.000")))
            Print(FNo, LPad(13, Format(CopyAC(1).TireY(i1), "##0.000")))
            PrintLine(FNo, LPad(13, Format(Response(1, j1), Zs)))
        Next
        PrintLine(FNo, "")
        PrintLine(FNo, "EvalDepth = " & LEAStrActiveX.EvalDepth)
        If gPavementType = clsACR.PavementType.Flexible Then
            PrintLine(FNo, "Max Strain= " & Format(StrainMaxGear, Zs))
        Else
            PrintLine(FNo, "Max Stress= " & Format(gStress, Zs))
        End If

        Dim max1 As Double
        max1 = -10000
        For i1 = 1 To CopyAC(1).NEvalPoints - CopyAC(1).NTires
            If max1 < Response(1, i1) Then
                max1 = Response(1, i1)
            End If
        Next
        PrintLine(FNo, "Max Mesh=   " & Format(max1, Zs))
        FileClose(FNo)

    End Sub


    Friend Sub Print_Array_XYsym(ByVal Full As Boolean, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                 ByRef Response(,) As Double,
                                 ByRef CopyAC() As ACRClassLib.clsLEAF.LEAFACParms)


        If Not gPrintOutput_Responses Then Exit Sub 'to uncomment

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.00000000" 'flexible
        If gPavementType = clsACR.PavementType.Rigid Then Zs = "#,##0.00000" 'Rigid

        FNo = FreeFile()
        Dim ccc As String
        'ccc = CSng(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / 1500, 2))
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        If Full Then
            FileName = WDir1 & "\Strains_" & gCat & "_" & ccc
            FileName = FileName & "_Full2" & FileExt
            'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt
        Else
            FileName = WDir1 & "\Strains_" & gCat & "_" & ccc & FileExt
        End If
        FileOpen(FNo, FileName, OpenMode.Output) 'Strains XYsym


        Dim i1, j1 As Integer, i1Start As Integer = 0
        If Full Then i1Start = -horE_Limit

        Print(FNo, LPad(Width, ""))
        For i1 = i1Start To horE_Limit Step 1
            Print(FNo, LPad(Width, Format(i1 * horStep + horMiddle, Zs)))
        Next
        PrintLine(FNo, "")
        '===========================================================================

        If Full Then
            For j1 = vertN_Limit + 1 To 2 Step -1
                Print(FNo, LPad(Width, Format(-(j1 - 1) * vertStep + vertMiddle, "#,###.00000")))
                For i1 = horE_Limit + 1 To 2 Step -1
                    Print(FNo, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next

                For i1 = 1 To horE_Limit + 1
                    Print(FNo, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next
                PrintLine(FNo, "")
            Next
        End If

        For j1 = 1 To vertN_Limit + 1
            Print(FNo, LPad(Width, Format((j1 - 1) * vertStep + vertMiddle, "#,###.00000")))

            If Full Then
                For i1 = horE_Limit + 1 To 2 Step -1
                    Print(FNo, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next
            End If

            For i1 = 1 To horE_Limit + 1
                'Print(FNo, LPad(Width, CStr(i1 + (j1 - 1) * (horLimit+1))))
                Print(FNo, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
            Next
            PrintLine(FNo, "")
        Next

        'If Full Then : i1Start = -horE_Limit : Else : i1Start = 0 : End If
        PrintLine(FNo, "") : PrintLine(FNo, "")

        For i1 = 1 To CopyAC(1).NTires
            Print(FNo, LPad(5, CStr(i1))) : j1 = CopyAC(1).NEvalPoints - CopyAC(1).NTires + i1
            Print(FNo, LPad(13, Format(CopyAC(1).TireX(i1), "##0.000")))
            Print(FNo, LPad(13, Format(CopyAC(1).TireY(i1), "##0.000")))
            PrintLine(FNo, LPad(13, Format(Response(1, j1), Zs)))
        Next
        PrintLine(FNo, "")
        PrintLine(FNo, "EvalDepth = " & LEAStrActiveX.EvalDepth)
        If gPavementType = clsACR.PavementType.Flexible Then
            PrintLine(FNo, "Max Strain= " & Format(StrainMaxGear, Zs))
        Else
            PrintLine(FNo, "Max Stress= " & Format(gStress, Zs))
        End If


        Dim max1 As Double
        max1 = -10000
        For i1 = 1 To CopyAC(1).NEvalPoints - CopyAC(1).NTires
            If max1 < Response(1, i1) Then
                max1 = Response(1, i1)
            End If
        Next
        PrintLine(FNo, "Max Mesh=   " & Format(max1, Zs))
        FileClose(FNo)



    End Sub

    Private Sub Print_Array_Ysym(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                 ByRef Response(,) As Double,
                                 ByRef CopyAC() As ACRClassLib.clsLEAF.LEAFACParms)


        'If Not gPrintOutput Then Exit Sub 'to uncomment

        Dim FNo As Integer, Width As Integer = 14, str1 As String = "\Strains_"
        Dim FileName As String, Zs As String = "#,##0.00000000" 'flexible
        If gPavementType = clsACR.PavementType.Rigid Then Zs = "#,##0.00000" 'Rigid
        If gPavementType = clsACR.PavementType.Rigid Then str1 = "\Stresses_" 'Rigid

        FNo = FreeFile()
        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & str1 & gCat & "_" & ccc
        FileName = FileName & "_Full_MeshY" & FileExt
        'FileName = FileName & "_Full_" & Format(TopSubDepth, "0.00000") & FileExt

        FileOpen(FNo, FileName, OpenMode.Output) 'Strains Full Mesh

        Dim i1, j1, ind1 As Integer
        Print(9, LPad(Width, ""))


        For i1 = 0 To iEast - iWest Step 1
            Print(FNo, LPad(Width, Format(i1 * horStep + West, Zs)))
        Next
        PrintLine(FNo, "")

        '===========================================================================
        'For j1 = iSouth To iNorth
        For j1 = 0 To iNorth - iSouth
            Print(FNo, LPad(Width, Format(j1 * vertStep + South, Zs)))
            'For i1 = iWest To iEast
            For i1 = 0 To iEast - iWest
                ind1 = i1 + 1 + (j1) * (iEast - iWest + 1)
                Print(FNo, LPad(Width, Format(Response(1, ind1), Zs)))
            Next
            PrintLine(FNo, "")
        Next

        PrintLine(FNo, "") : PrintLine(FNo, "")
        For i1 = 1 To CopyAC(1).NTires
            Print(FNo, LPad(5, CStr(i1))) : j1 = CopyAC(1).NEvalPoints - CopyAC(1).NTires + i1
            Print(FNo, LPad(13, Format(CopyAC(1).TireX(i1), "##0.000")))
            Print(FNo, LPad(13, Format(CopyAC(1).TireY(i1), "##0.000")))
            PrintLine(FNo, LPad(13, Format(Response(1, j1), Zs)))
        Next
        PrintLine(FNo, "")
        PrintLine(FNo, "EvalDepth = " & LEAStrActiveX.EvalDepth)
        If gPavementType = clsACR.PavementType.Flexible Then
            PrintLine(FNo, "Max Strain= " & Format(StrainMaxGear, Zs))
        Else
            PrintLine(FNo, "Max Stress= " & Format(gStress, Zs))
        End If

        Dim max1 As Double
        max1 = -10000
        For i1 = 1 To CopyAC(1).NEvalPoints - CopyAC(1).NTires
            If max1 < Response(1, i1) Then
                max1 = Response(1, i1)
            End If
        Next
        PrintLine(FNo, "Max Mesh=   " & Format(max1, Zs))
        FileClose(FNo)



    End Sub


End Module

'ByVal CallAC() As ACNClassLib.clsLEAF.LEAFACParms
' ByRef LEAStrActiveX As ACNClassLib.clsLEAF.LEAFStrParms

