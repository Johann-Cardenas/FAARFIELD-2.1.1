Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Module ZZZ4
    Public gXcg, gYcg As Double

    Public Const MaxIter As Integer = 75
    Public iter1 As Integer

    Public bNewDiv1 As Boolean = False

    Public bSelectWheels As Boolean = False
    Public gSW() As Integer
    Public gMeshSize As Single = 10 / 2.54 'new123
    Public gBuffer As Single = 2 * gMeshSize 'new123

    Public bMeshSize As Boolean = True 'new123
    Public gFileName As String
    Public gMeshSizeCount As Integer = 0 'new123

    Public gH_count, gV_count As Integer
    Public gSum, gSum2 As Single
    'Public gFF As Integer

    Public gPrint1800 As Boolean = False

    Public gEvalX(1, 1800) As Double 'ikawa
    Public gEvalY(1, 1800) As Double 'ikawa

    Public Function getTodaysDateFormatted() As String
        Return DateTime.Now.ToString("yyyy-MM-dd HH;mm;ss")
    End Function


    Public Sub Print1800(ByRef Strain1(,) As Double, ByVal Dam As Double, ByVal ss As Double)

        Dim FNo As Integer, Width As Integer = 14
        Dim FileName As String, Zs As String = "#,##0.0000000000"
        FNo = FreeFile()

        Dim ccc As String
        'ccc = CSng(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / 1500, 2))
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")


        FileName = WDir1 & "\1800_Cat" & gCat & "_" & ccc & "_"
        FileName = FileName & "AAA" & FileExt
        FileOpen(9, FileName, OpenMode.Output)

        PrintLine(9, "")
        PrintLine(9, "Damage     = " & Dam)
        PrintLine(9, "NtoFail    = " & 1 / Dam)
        'PrintLine(9, "Max strain = " & ss)
        PrintLine(9, "")
        For i1 As Integer = 1 To 1800
            PrintLine(9, i1, TAB(7), LPad(9, Format(gEvalX(1, i1), "#0.000")) & LPad(13, Format(gEvalY(1, i1), "#0.00000")) & LPad(17, Format(Strain1(1, i1), Zs)))
        Next

        FileClose(9)

    End Sub

    Public Sub Set_Eval_Points_Ysym_Mesh(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        If CallAC(1).NTires = 1 Then
            CallAC(1).NEvalPoints = 1
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)
            'CallAC(1).EvalX(1) = 0 : CallAC(1).EvalY(1) = 0 'ik2020.03
            CallAC(1).EvalX(1) = CallAC(1).TireX(1) : CallAC(1).EvalY(1) = CallAC(1).TireY(1)
            Exit Sub
        End If

        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i As Integer = 1 To CallAC(1).NTires
            If CallAC(1).TireX(i) > East Then
                East = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) > North Then
                North = CSng(CallAC(1).TireY(i))
            End If
            If CallAC(1).TireX(i) < West Then
                West = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) < South Then
                South = CSng(CallAC(1).TireY(i))
            End If
        Next

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2
        gH_count = 0 : gV_count = 0
        gSum = horMiddle
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East

        gSum2 = South
        Do
            gV_count = gV_count + 1
            gSum2 = gSum2 + gMeshSize
        Loop Until gSum2 >= North

        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize


        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + horMiddle
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + South
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints
        'Call Add_Wheels_Coord(CallAC)

        Exit Sub

        If Not gPrintOutput Then Exit Sub

        'Dim gFF2 As Integer
        'gFF2 = FreeFile()
        'FileOpen(gFF2, WDir1 & "\" & gFileName & "_Mesh.txt", OpenMode.Output)


        'Dim v1 As Integer = 0
        'For i1 = 1 To gMeshSizeCount
        '    Print(gFF2, LPad(6, Format(CallAC(1).EvalX(i1), "#0.0")) & ":" & LPad(6, Format(CallAC(1).EvalY(i1), "#0.0")))
        '    If (i1 Mod (horE_Limit + 1) = 0) Then
        '        PrintLine(gFF2, "")
        '        v1 = v1 + 1
        '    End If
        'Next

        'PrintLine(gFF2, "")
        'Dim c2 As Integer = 0
        'For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
        '    c2 = c2 + 1
        '    PrintLine(gFF2, j1, TAB(9), LPad(6, Format(CallAC(1).EvalX(j1), "#0.0")) & ":" & LPad(6, Format(CallAC(1).EvalY(j1), "#0.0")))
        'Next

        'PrintLine(gFF2, "")
        'PrintLine(gFF2, "Gear Load    = " & CallAC(1).GearLoad)
        'PrintLine(gFF2, "Tire Pressure= " & CallAC(1).TirePress(1))
        'FileClose(gFF2)

    End Sub


    Public Sub Get_NorthSouth_EastWest_2(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Try

            North = -1.0E+35 : South = 1.0E+35
            East = -1.0E+35 : West = 1.0E+35

            For i As Integer = 1 To CallAC(1).NTires
                If gSW(i) = 1 Then
                    If CallAC(1).TireX(i) > East Then
                        East = CSng(CallAC(1).TireX(i))
                    End If
                    If CallAC(1).TireY(i) > North Then
                        North = CSng(CallAC(1).TireY(i))
                    End If
                    If CallAC(1).TireX(i) < West Then
                        West = CSng(CallAC(1).TireX(i))
                    End If
                    If CallAC(1).TireY(i) < South Then
                        South = CSng(CallAC(1).TireY(i))
                    End If
                End If
            Next

            West = West - gBuffer '123 2018.07.18
            East = East + gBuffer '123 2018.07.18
            North = North + gBuffer '123 2018.07.18
            South = South - gBuffer '123 2018.07.18


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub


    Public Sub Get_NorthSouth_EastWest(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i As Integer = 1 To CallAC(1).NTires
            If CallAC(1).TireX(i) > East Then
                East = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) > North Then
                North = CSng(CallAC(1).TireY(i))
            End If
            If CallAC(1).TireX(i) < West Then
                West = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) < South Then
                South = CSng(CallAC(1).TireY(i))
            End If
        Next


    End Sub

    Public Sub Set_Eval_Points_Ysym_Mesh2_SW(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)
        Call Get_NorthSouth_EastWest_2(CallAC)
        Call Get_Center_of_Gravity_2(CallAC, gXcg, gYcg)

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2

        gH_count = 0 : gV_count = 0
        WestEdge = gXcg

        gSum = gXcg
        Do
            gSum = gSum - gMeshSize
        Loop Until gSum < West
        WestEdge = gSum ' - gMeshSize * 2 '123 2018.07.18

        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum > East


        SouthEdge = gYcg
        Do
            SouthEdge = SouthEdge - gMeshSize
        Loop Until SouthEdge < South

        gSum = SouthEdge ' - gMeshSize * 2 '123 2018.07.18
        Do
            gV_count = gV_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum > North

        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize

        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + WestEdge
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + SouthEdge
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub



    Public Sub Set_Eval_Points_Ysym_Mesh2(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Call Get_NorthSouth_EastWest(CallAC)
        Call Get_Center_of_Gravity(CallAC, gXcg, gYcg)

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2

        East = East + gBuffer
        North = North + gBuffer
        South = South - gBuffer

        gH_count = 0 : gV_count = 0
        WestEdge = gXcg

        gSum = gXcg
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East

        SouthEdge = gYcg
        Do
            gV_count = gV_count + 1
            SouthEdge = SouthEdge - gMeshSize
        Loop Until SouthEdge <= South

        gSum = gYcg
        Do
            gV_count = gV_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= North

        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize

        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + WestEdge
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + SouthEdge
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub






    Public Sub Set_Eval_Points_XYsym_Mesh_Full2(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Call Get_NorthSouth_EastWest(CallAC)
        Call Get_Center_of_Gravity(CallAC, gXcg, gYcg)

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2

        West = West - gBuffer
        East = East + gBuffer
        North = North + gBuffer
        South = South - gBuffer



        gH_count = 0 : gV_count = 0
        WestEdge = gXcg

        Do
            gH_count = gH_count + 1
            WestEdge = WestEdge - gMeshSize
        Loop Until WestEdge <= West

        gSum = gXcg
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East


        SouthEdge = gYcg
        Do
            gV_count = gV_count + 1
            SouthEdge = SouthEdge - gMeshSize
        Loop Until SouthEdge <= South

        gSum = gYcg
        Do
            gV_count = gV_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= North

        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize


        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + WestEdge
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + SouthEdge
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub



    Public Sub Set_Eval_Points_XYsym_Mesh_Full(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Call Get_NorthSouth_EastWest(CallAC)
        Call Get_Center_of_Gravity(CallAC, gXcg, gYcg)

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2

        gH_count = 0 : gV_count = 0
        gSum = West
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East

        gSum = South
        Do
            gV_count = gV_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= North

        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize


        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + West
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + South
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub

    Public Sub Add_Wheels_Coord(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        If iSelectEval_ACN <> 2 Then
            '*****   Adding wheel coordinates as evaluation points   *****
            CallAC(1).NEvalPoints = CallAC(1).NEvalPoints + CallAC(1).NTires
            ReDim Preserve CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim Preserve CallAC(1).EvalY(CallAC(1).NEvalPoints)

            Dim q1, c2 As Integer
            q1 = CallAC(1).NEvalPoints - CallAC(1).NTires

            c2 = 0
            For j1 = q1 + 1 To CallAC(1).NEvalPoints
                c2 = c2 + 1
                CallAC(1).EvalX(j1) = CallAC(1).TireX(c2)
                CallAC(1).EvalY(j1) = CallAC(1).TireY(c2)
            Next
        End If

    End Sub


    Public Sub Set_Eval_Points_XYsym_Mesh(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i As Integer = 1 To CallAC(1).NTires
            If CallAC(1).TireX(i) > East Then
                East = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) > North Then
                North = CSng(CallAC(1).TireY(i))
            End If
            If CallAC(1).TireX(i) < West Then
                West = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) < South Then
                South = CSng(CallAC(1).TireY(i))
            End If
        Next

        Call Get_Center_of_Gravity(CallAC, gXcg, gYcg)

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2

        East = East + gBuffer '123 2018.07.19
        North = North + gBuffer '123 2018.07.19

        gH_count = 0 : gV_count = 0
        gSum = horMiddle
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East
        gSum = vertMiddle
        Do
            gV_count = gV_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= North

        horE_Limit = gH_count
        vertN_Limit = gV_count

        If East - West = 0 Then horE_Limit = 0
        If North - South = 0 Then vertN_Limit = 0

        horStep = gMeshSize
        vertStep = gMeshSize

        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + horMiddle
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + vertMiddle
            Next
        Next
        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub

    Public Sub Print_Strain_Array_XYsym_Mesh(ByVal Full As Boolean, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                             ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

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
            Next
            PrintLine(9, "")
        Next

        'If Full Then : i1Start = -horE_Limit : Else : i1Start = 0 : End If
        PrintLine(9, "") : PrintLine(9, "")


        Dim c2 As Integer
        c2 = 0

        For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
            PrintLine(9, j1, TAB(9), Format(Response(1, j1), Zs))
        Next

        Dim loc1 As Integer
        Dim max1 As Double = 1.0E+35
        For j1 = 1 To gMeshSizeCount

            If max1 > Response(1, j1) Then
                max1 = Response(1, j1)
                loc1 = j1
            End If

        Next
        PrintLine(9, "")
        PrintLine(9, "Max Strain= " & Response(1, loc1) & "  " & CallAC(1).EvalX(loc1) & " " & CallAC(1).EvalY(loc1))
        PrintLine(9, "")


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
        FileClose(9)


    End Sub




    Public Sub Print_Strain_Array_Ysym_Mesh(ByVal Full As Boolean, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                             ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

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


        Dim i1, j1 As Integer, i1Start As Integer = 0
        If Full Then i1Start = -horE_Limit

        Print(9, LPad(Width, ""))
        For i1 = i1Start To horE_Limit Step 1
            Print(9, LPad(Width, Format(i1 * horStep + horMiddle, Zs)))
        Next
        PrintLine(9, "")
        '===========================================================================

        'If Full Then
        '    For j1 = vertN_Limit + 1 To 2 Step -1
        '        Print(9, LPad(Width, Format(-(j1 - 1) * vertStep + vertMiddle, "#,###.00000")))
        '        For i1 = horE_Limit + 1 To 2 Step -1
        '            Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
        '        Next

        '        For i1 = 1 To horE_Limit + 1
        '            Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
        '        Next
        '        PrintLine(9, "")
        '    Next
        'End If

        For j1 = 1 To vertN_Limit + 1
            'Print(9, LPad(Width, Format((j1 - 1) * vertStep + vertMiddle, "#,###.00000")))
            Print(9, LPad(Width, Format((j1 - 1) * vertStep + South, "#,###.00000")))

            If Full Then
                For i1 = horE_Limit + 1 To 2 Step -1
                    Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
                Next
            End If

            For i1 = 1 To horE_Limit + 1
                'Print(9, LPad(Width, CStr(i1 + (j1 - 1) * (horLimit+1))))
                Print(9, LPad(Width, Format(Response(1, i1 + (j1 - 1) * (horE_Limit + 1)), Zs)))
            Next
            PrintLine(9, "")
        Next

        'If Full Then : i1Start = -horE_Limit : Else : i1Start = 0 : End If
        PrintLine(9, "") : PrintLine(9, "")


        Dim c2 As Integer
        c2 = 0

        For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
            c2 = c2 + 1
            PrintLine(9, LPad(4, CStr(c2)) & "  " & j1, TAB(15), Format(Response(1, j1), Zs))
        Next

        Dim loc1 As Integer
        Dim max1 As Double = 1.0E+35
        For j1 = 1 To gMeshSizeCount

            If max1 > Response(1, j1) Then
                max1 = Response(1, j1)
                loc1 = j1
            End If

        Next
        PrintLine(9, "")
        PrintLine(9, "Max Strain= " & Response(1, loc1) & "  " & CallAC(1).EvalX(loc1) & " " & CallAC(1).EvalY(loc1))
        PrintLine(9, "")


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
        FileClose(9)


    End Sub



    Public Sub Print_Evaluation_Points2(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        'Try
        '    If Not gPrintOutput Then Exit Sub
        '    Dim i1, c2 As Integer, ss1 As String, s1 As String = "#0.00"
        '    Dim gFF3 As Integer = FreeFile()
        '    Dim klen1 As Integer = 7

        '    If bMeshSize Then
        '        FileOpen(gFF3, WDir1 & "\" & gFileName & "_Mesh.txt", OpenMode.Output)
        '    Else 'bNewDiv1 = true
        '        FileOpen(gFF3, WDir1 & "\" & gFileName & "_MeshDiv1.txt", OpenMode.Output)
        '    End If

        '    For i1 = 1 To gMeshSizeCount
        '        ss1 = LPad(klen1, Format(CallAC(1).EvalX(i1), s1)) & ":" & LPad(klen1, Format(CallAC(1).EvalY(i1), s1))
        '        Print(gFF3, ss1)
        '        If (i1 Mod (horE_Limit + 1) = 0) Then
        '            PrintLine(gFF3, "" & "  (" & i1 & ")")
        '        End If
        '    Next

        '    PrintLine(gFF3, "")
        '    c2 = 0
        '    For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
        '        c2 = c2 + 1
        '        ss1 = LPad(7, Format(CallAC(1).EvalX(j1), s1)) & ":" & LPad(7, Format(CallAC(1).EvalY(j1), s1))
        '        PrintLine(gFF3, j1, TAB(9), ss1)
        '    Next

        '    PrintLine(gFF3, "")
        '    PrintLine(gFF3, "Gear Load    = " & CallAC(1).GearLoad)
        '    PrintLine(gFF3, "Tire Pressure= " & CallAC(1).TirePress(1))
        '    PrintLine(gFF3, "")

        '    If bMeshSize Then
        '        PrintLine(gFF3, "Center of Gravity")
        '        PrintLine(gFF3, "Xcg= " & gXcg)
        '        PrintLine(gFF3, "Ycg= " & gYcg)
        '    End If
        '    FileClose(gFF3)


        '======================================== 
        'gFF3 = FreeFile() 
        'FileOpen(gFF3, WDir1 & "\" & "MMMM.txt", OpenMode.Output) 

        'For i1 = 1 To gMeshSizeCount
        '    ss1 = LPad(8, Format(CallAC(1).EvalX(i1), s1)) & "  " & LPad(8, Format(CallAC(1).EvalY(i1), s1))
        '    PrintLine(gFF3, ss1)
        'Next

        'PrintLine(gFF3, "")
        'For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
        '    ss1 = LPad(8, Format(CallAC(1).EvalX(j1), s1)) & " " & LPad(8, Format(CallAC(1).EvalY(j1), s1))
        '    PrintLine(gFF3, ss1)
        'Next
        'FileClose(gFF3)

        'Catch ex As Exception

        '    Dim txt As String
        '    txt = ex.Message
        '    txt = txt + Environment.NewLine + Environment.NewLine
        '    txt = txt + ex.StackTrace
        '    txt = txt + Environment.NewLine + Environment.NewLine
        '    MsgBox(txt)

        'End Try


    End Sub



    Public Sub Print_Evaluation_Points(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)



        'Try
        '    If Not gPrintOutput Then Exit Sub
        '    Dim i1, c2 As Integer, ss1 As String, s1 As String = "#0.00"
        '    Dim gFF3 As Integer = FreeFile()

        '    'WDir1 = SpecialDirectories.MyDocuments + "\My FAARFIELD\ACR_Results_" & PavementType
        '    'If Directory.Exists(WDir1) Then
        '    'Else
        '    '    System.IO.Directory.CreateDirectory(WDir1)
        '    'End If


        '    If bMeshSize Then
        '        FileOpen(gFF3, WDir1 & "\" & gFileName & "_Mesh.txt", OpenMode.Output)
        '    Else 'bNewDiv1 = true
        '        FileOpen(gFF3, WDir1 & "\" & gFileName & "_MeshDiv1.txt", OpenMode.Output)
        '    End If

        '    For i1 = 1 To gMeshSizeCount
        '        ss1 = LPad(7, Format(CallAC(1).EvalX(i1), s1)) & ":" & LPad(7, Format(CallAC(1).EvalY(i1), s1))
        '        Print(gFF3, ss1)
        '        If (i1 Mod (horE_Limit + 1) = 0) Then
        '            PrintLine(gFF3, "")
        '        End If
        '    Next

        '    PrintLine(gFF3, "")
        '    c2 = 0
        '    For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
        '        c2 = c2 + 1
        '        ss1 = LPad(7, Format(CallAC(1).EvalX(j1), s1)) & ":" & LPad(7, Format(CallAC(1).EvalY(j1), s1))

        '        If bSelectWheels Then
        '            ss1 = ss1 & LPad(7, gSW(c2))
        '        End If

        '        PrintLine(gFF3, j1, TAB(9), ss1)
        '    Next

        '    PrintLine(gFF3, "")
        '    PrintLine(gFF3, "Gear Load    = " & CallAC(1).GearLoad)
        '    PrintLine(gFF3, "Tire Pressure= " & CallAC(1).TirePress(1))
        '    PrintLine(gFF3, "")

        '    If bMeshSize Then
        '        PrintLine(gFF3, "Center of Gravity")
        '        PrintLine(gFF3, "Xcg= " & gXcg)
        '        PrintLine(gFF3, "Ycg= " & gYcg)
        '    End If
        '    FileClose(gFF3)


        '    '======================================== 
        '    gFF3 = FreeFile()
        '    'FileOpen(gFF3, WDir1 & "\" & "MMMM.txt", OpenMode.Output)

        '    's1 = "#0.000"
        '    'For i1 = 1 To gMeshSizeCount
        '    '    ss1 = LPad(8, Format(CallAC(1).EvalX(i1), s1)) & "  " & LPad(8, Format(CallAC(1).EvalY(i1), s1))
        '    '    PrintLine(gFF3, ss1)
        '    'Next

        '    'PrintLine(gFF3, "")
        '    'For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
        '    '    ss1 = LPad(8, Format(CallAC(1).EvalX(j1), s1)) & " " & LPad(8, Format(CallAC(1).EvalY(j1), s1))
        '    '    PrintLine(gFF3, ss1)
        '    'Next
        '    'FileClose(gFF3)

        'Catch ex As Exception

        '    Dim txt As String
        '    txt = ex.Message
        '    txt = txt + Environment.NewLine + Environment.NewLine
        '    txt = txt + ex.StackTrace
        '    txt = txt + Environment.NewLine + Environment.NewLine
        '    MsgBox(txt)

        'End Try


    End Sub




    Public Sub Print_Evaluation_Points_2gears(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                              ByVal CallAC_belly() As ACRClassLib.clsLEAF.LEAFACParms)

        If Not gPrintOutput Then Exit Sub
        Dim i1, c2 As Integer, ss1 As String, s1 As String = "#0.00"
        Dim gFF1 As Integer = FreeFile()

        gFF1 = FreeFile()

        If bMeshSize Then
            FileOpen(gFF1, WDir1 & "\" & gFileName & "_Mesh.txt", OpenMode.Output)
        Else 'bNewDiv1
            FileOpen(gFF1, WDir1 & "\" & gFileName & "_MeshDiv1.txt", OpenMode.Output)
        End If

        For i1 = 1 To gMeshSizeCount
            ss1 = LPad(7, Format(CallAC(1).EvalX(i1), s1)) & ":" & LPad(7, Format(CallAC(1).EvalY(i1), s1))
            Print(gFF1, ss1)
            If (i1 Mod (horE_Limit + 1) = 0) Then
                PrintLine(gFF1, "")
            End If
        Next

        PrintLine(gFF1, "")
        c2 = 0
        For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
            c2 = c2 + 1
            ss1 = LPad(7, Format(CallAC(1).EvalX(j1), s1)) & ":" & LPad(7, Format(CallAC(1).EvalY(j1), s1))
            PrintLine(gFF1, j1, TAB(9), ss1)
        Next

        PrintLine(gFF1, "")
        PrintLine(gFF1, "Two Gears")
        PrintLine(gFF1, "")

        PrintLine(gFF1, "Gear Load 1    wing  = " & CallAC(1).GearLoad)
        PrintLine(gFF1, "Tire Pressure1 wing  = " & CallAC(1).TirePress(1))
        PrintLine(gFF1, "Gear Load 2    belly = " & CallAC_belly(1).GearLoad)
        PrintLine(gFF1, "Tire Pressure2 belly = " & CallAC_belly(1).TirePress(1))


        If bMeshSize Then
            PrintLine(gFF1, "Center of Gravity")
            PrintLine(gFF1, "Xcg= " & gXcg)
            PrintLine(gFF1, "Ycg= " & gYcg)
        End If
        FileClose(gFF1)

        '======================================== 
        Dim gFF3 As Integer
        gFF3 = FreeFile()
        s1 = "#0.000"
        'FileOpen(gFF3, WDir1 & "\" & "MMMM2.txt", OpenMode.Output)

        'For i1 = 1 To gMeshSizeCount
        '    ss1 = LPad(8, Format(CallAC(1).EvalX(i1), s1)) & "  " & LPad(8, Format(CallAC(1).EvalY(i1), s1))
        '    PrintLine(gFF3, ss1)
        'Next

        'PrintLine(gFF3, "")
        'For j1 = gMeshSizeCount + 1 To CallAC(1).NEvalPoints
        '    ss1 = LPad(8, Format(CallAC(1).EvalX(j1), s1)) & " " & LPad(8, Format(CallAC(1).EvalY(j1), s1))
        '    PrintLine(gFF3, ss1)
        'Next
        'FileClose(gFF3)


    End Sub




    Public Sub Set_Eval_Points_Ysym_Mesh_2gears(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                               ByRef C_X() As Single, ByRef C_Y() As Single)

        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i As Integer = 1 To UBound(C_X, 1)
            If C_X(i) > East Then
                East = CSng(C_X(i))
            End If
            If C_Y(i) > North Then
                North = CSng(C_Y(i))
            End If
            If C_X(i) < West Then
                West = CSng(C_X(i))
            End If
            If C_Y(i) < South Then
                South = CSng(C_Y(i))
            End If
        Next

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2
        gH_count = 0 : gV_count = 0
        gSum = horMiddle
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East

        gSum2 = South
        Do
            gV_count = gV_count + 1
            gSum2 = gSum2 + gMeshSize
        Loop Until gSum2 >= North

        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize


        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + horMiddle
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + South
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints
        Call Create_Eval_Points_Under_Wheels_2gears(CallAC, C_X, C_Y)
        'Call Print_Evaluation_Points_2gears(CallAC)

    End Sub



    Public Sub Set_Eval_Points_Ysym_Mesh_2gearsCG(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                               ByRef C_X() As Single, ByRef C_Y() As Single)

        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i As Integer = 1 To UBound(C_X, 1)
            If C_X(i) > East Then
                East = CSng(C_X(i))
            End If
            If C_Y(i) > North Then
                North = CSng(C_Y(i))
            End If
            If C_X(i) < West Then
                West = CSng(C_X(i))
            End If
            If C_Y(i) < South Then
                South = CSng(C_Y(i))
            End If
        Next

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2
        gH_count = 0 : gV_count = 0
        '==================================
        WestEdge = gXcg

        gSum = gXcg
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East

        SouthEdge = gYcg
        Do
            gV_count = gV_count + 1
            SouthEdge = SouthEdge - gMeshSize
        Loop Until SouthEdge <= South

        gSum = gYcg
        Do
            gV_count = gV_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= North


        '===================================
        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize


        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + WestEdge
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + SouthEdge
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints
        Call Create_Eval_Points_Under_Wheels_2gears(CallAC, C_X, C_Y)
        'Call Print_Evaluation_Points_2gears(CallAC)

    End Sub




    Public Sub Set_Eval_Points_Ysym_Mesh_2gearsCG_SW(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                               ByRef C_X() As Single, ByRef C_Y() As Single)

        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i As Integer = 1 To UBound(C_X, 1)
            If gSW(i) = 1 Then
                If C_X(i) > East Then
                    East = CSng(C_X(i))
                End If
                If C_Y(i) > North Then
                    North = CSng(C_Y(i))
                End If
                If C_X(i) < West Then
                    West = CSng(C_X(i))
                End If
                If C_Y(i) < South Then
                    South = CSng(C_Y(i))
                End If
            End If
        Next

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2
        gH_count = 0 : gV_count = 0
        '==================================

        North = North + gBuffer
        South = South - gBuffer
        East = East + gBuffer

        West = West - gBuffer
        If West < 0 Then West = 0

        WestEdge = gXcg
        WestEdge = West

        gSum = gXcg
        Do
            gH_count = gH_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= East

        SouthEdge = gYcg
        Do
            gV_count = gV_count + 1
            SouthEdge = SouthEdge - gMeshSize
        Loop Until SouthEdge <= South

        gSum = gYcg
        'gSum = South
        Do
            gV_count = gV_count + 1
            gSum = gSum + gMeshSize
        Loop Until gSum >= North


        '===================================
        horE_Limit = gH_count
        vertN_Limit = gV_count

        horStep = gMeshSize
        vertStep = gMeshSize


        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + WestEdge
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + SouthEdge
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints
        Call Create_Eval_Points_Under_Wheels_2gears(CallAC, C_X, C_Y)
        'Call Print_Evaluation_Points_2gears(CallAC)

    End Sub



End Module


