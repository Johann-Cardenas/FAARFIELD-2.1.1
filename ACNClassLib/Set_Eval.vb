Option Strict On
Option Explicit On

Module modSet_Eval

    Public i As Integer
    Public j1 As Integer

    Public Sub Mesh_All_Cases_div1(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)
        Dim divH, divV As Integer
        Dim westEdge, southEdge As Single

        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i As Integer = 1 To CallAC(1).NTires
            If CallAC(1).TireX(i) > East Then East = CSng(CallAC(1).TireX(i))
            If CallAC(1).TireY(i) > North Then North = CSng(CallAC(1).TireY(i))
            If CallAC(1).TireX(i) < West Then West = CSng(CallAC(1).TireX(i))
            If CallAC(1).TireY(i) < South Then South = CSng(CallAC(1).TireY(i))
        Next

        horMiddle = (East + West) / 2
        vertMiddle = (North + South) / 2

        iEast = CInt(Math.Ceiling(East) / div1)
        iWest = CInt(Math.Floor(West) / div1)
        iSouth = CInt(Math.Floor(South) / div1)
        iNorth = CInt(Math.Ceiling(North) / div1)

        If Symmetry = SymmetryType.XYSymmetry Then 'Mesh_All_Cases
            divH = 2 : westEdge = horMiddle
            divV = 2 : southEdge = vertMiddle
        ElseIf Symmetry = SymmetryType.YSymmetry Then
            divH = 2 : westEdge = horMiddle
            divV = 1 : southEdge = South
        Else
            divH = 1 : westEdge = West
            divV = 1 : southEdge = South
        End If

        horE_Limit = CInt(Math.Ceiling((iEast - iWest) / divH))
        vertN_Limit = CInt(Math.Ceiling((iNorth - iSouth) / divV))

        If (iEast - iWest) = 0 Then horStep = 1
        If (iNorth - iSouth) = 0 Then vertStep = 1

        If (iEast - iWest) > 0 Then
            horStep = (East - West) / divH / horE_Limit
        End If

        If (iNorth - iSouth) > 0 Then
            vertStep = (North - South) / divV / vertN_Limit
        End If

        Dim i1, j1 As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + westEdge
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + southEdge
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub



    Public Sub RedimArraysFlexible(ByVal NLayer1 As Integer, ByVal iCat1 As Integer,
         ByRef LCode() As Short, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)


        'If NLayer1 > 4 Then
        '    For i As Integer = 4 To NLayer1 - 1
        '        LEAStrActiveX.Thick(3) = LEAStrActiveX.Thick(3) + LEAStrActiveX.Thick(i)
        '    Next
        '    LEAStrActiveX.Thick(4) = 0
        '    NLayer1 = 4
        '    LEAStrActiveX.NLayers = 4
        'End If

        'Reset to 3 layers
        If NLayer1 > 3 Then
            For i As Integer = 3 To NLayer1 - 1
                LEAStrActiveX.Thick(2) = LEAStrActiveX.Thick(2) + LEAStrActiveX.Thick(i)
            Next
            LEAStrActiveX.Thick(3) = 0
            NLayer1 = 3
            LEAStrActiveX.NLayers = 3
        End If


        ReDim Preserve LEAStrActiveX.Thick(NLayer1)
        ReDim Preserve LEAStrActiveX.Modulus(NLayer1)
        ReDim Preserve LEAStrActiveX.Poisson(NLayer1)
        ReDim Preserve LEAStrActiveX.InterfaceParm(NLayer1)

        ReDim Preserve LCode(NLayer1)

        'Category	Mpa		            psi		  CBR
        'D	         50	  145.0377    7,252		 4.83
        'C	         80	  145.0377   11,603		 7.74
        'B	        120	  145.0377	 17,405		11.60
        'A	        200	  145.0377	 29,008		19.34

        'paragraph 1.1.3.8 a) Flexible Pavements.
        LEAStrActiveX.Modulus(1) = 200000
        LEAStrActiveX.Modulus(1) = 1379 * MPaToPsi
        LEAStrActiveX.Modulus(2) = 400 * MPaToPsi 'P-209 (58,015.0938)

        'paragraph 1.1.3.2 a) Subgrade category (flexible)
        If iCat1 = 1 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 50 * MPaToPsi
        If iCat1 = 2 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 80 * MPaToPsi
        If iCat1 = 3 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 120 * MPaToPsi
        If iCat1 = 4 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 200 * MPaToPsi

        'to comment
        'If iCat1 = 1 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 7500
        'If iCat1 = 2 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 12000
        'If iCat1 = 3 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 17000
        'If iCat1 = 4 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 25000

        For i = 1 To LEAStrActiveX.NLayers
            LEAStrActiveX.Poisson(i) = 0.35
            LEAStrActiveX.InterfaceParm(i) = 1
        Next

        LCode(1) = 1 'P-401
        LCode(2) = 6 'P-209
        'LCode(2) = 0 'User Defined

        If LEAStrActiveX.NLayers = 4 Then
            LCode(3) = 8 'P-154
        End If

        LCode(LEAStrActiveX.NLayers) = 4
        LEAStrActiveX.Thick(NLayer1) = 0

        'LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = gSubModulus1 'remove comment  Public Function CalculateACN_Output(
        'LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = public_is1 'to comment 101     Public Sub Evaluation_Loop

    End Sub


    Public Sub RedimArraysRigid(ByVal NLayer1 As Integer, ByVal iCat1 As Integer,
            ByRef LCode() As Short, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        ReDim LEAStrActiveX.Thick(NLayer1)
        ReDim LEAStrActiveX.Modulus(NLayer1)
        ReDim LEAStrActiveX.Poisson(NLayer1)
        ReDim LEAStrActiveX.InterfaceParm(NLayer1)

        ReDim LCode(NLayer1)

        'paragraph 1.1.3.7 Rigid Pavements. 
        LEAStrActiveX.Modulus(1) = 4000000 '27579.03 Mpa
        LEAStrActiveX.Modulus(1) = 27579 * MPaToPsi '3,999,995.76885567
        LEAStrActiveX.Modulus(2) = 500 * MPaToPsi 'P-209 (58,015.0938)

        'paragraph 1.1.3.2 a) Subgrade category (rigid)
        If iCat1 = 1 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 50 * MPaToPsi
        If iCat1 = 2 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 80 * MPaToPsi
        If iCat1 = 3 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 120 * MPaToPsi
        If iCat1 = 4 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 200 * MPaToPsi


        'If iCat1 = 1 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 3 * 1500
        'If iCat1 = 2 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 6 * 1500
        'If iCat1 = 3 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 10 * 1500
        'If iCat1 = 4 Then LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 15 * 1500


        LEAStrActiveX.Poisson(1) = 0.15
        LEAStrActiveX.InterfaceParm(1) = 0

        For i = 2 To LEAStrActiveX.NLayers
            LEAStrActiveX.Poisson(i) = 0.35
            LEAStrActiveX.InterfaceParm(i) = 1
        Next

        LEAStrActiveX.Poisson(LEAStrActiveX.NLayers) = 0.4

        LCode(1) = 5 'P-501
        'LCode(2) = 6 'P-209
        LCode(2) = 0 'User Defined
        LCode(LEAStrActiveX.NLayers) = 4

        'LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = 10000

    End Sub



    Public Sub Set_Eval_Points_Dim(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Dim div2 As Integer = 4 * 4  '1
        North = 55 : South = 0
        East = 15 : West = -15

        North = 140 : South = -20
        East = 250 : West = -250


        iEast = CInt(Math.Ceiling(East) / div2)
        iWest = CInt(Math.Floor(West) / div2)
        iSouth = CInt(Math.Floor(South) / div2)
        iNorth = CInt(Math.Ceiling(North) / div2)

        If (iEast - iWest) = 0 Then
            horStep = 1
        Else
            horStep = (East - West) / (iEast - iWest)
        End If

        If (iNorth - iSouth) = 0 Then
            vertStep = 1
        Else
            vertStep = (North - South) / (iNorth - iSouth)
        End If

        Dim i1, j1, iLoc, ind1 As Integer
        CallAC(1).NEvalPoints = (iEast - iWest + 1) * (iNorth - iSouth + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        iLoc = 0
        For j1 = 0 To iNorth - iSouth Step 1
            For i1 = 0 To iEast - iWest Step 1
                ind1 = (i1 + 1 + j1 + j1 * (iEast - iWest))
                CallAC(1).EvalX(ind1) = West + i1 * horStep
                CallAC(1).EvalY(ind1) = South + j1 * vertStep

                iLoc = iLoc + 1
                If iLoc = iMaxGear Then
                    iHorGear = i1
                    xHorGear = West + i1 * horStep

                    jVertGear = j1
                    yVertGear = South + j1 * vertStep
                End If
            Next
        Next


    End Sub


    Public Sub Set_Eval_Points_Dim_2(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Dim i1, j1, iLoc, ind1 As Integer
        CallAC(1).NEvalPoints = 1001
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        iLoc = 0
        For i1 = 0 To 1000 Step 1
            CallAC(1).EvalX(i1 + 1) = i1
            CallAC(1).EvalY(i1 + 1) = 0
        Next


    End Sub





    Private Sub Set_Thick_Values(ByVal iCat1 As Integer, ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms)

        If iCat1 = 1 Then
            LEAStrActiveX.Thick(3) = 54.33836 - LEAStrActiveX.Thick(1) - LEAStrActiveX.Thick(2)
        ElseIf iCat1 = 2 Then
            LEAStrActiveX.Thick(3) = 39.23796 - LEAStrActiveX.Thick(1) - LEAStrActiveX.Thick(2)
        ElseIf iCat1 = 3 Then
            LEAStrActiveX.Thick(3) = 27.6482 - LEAStrActiveX.Thick(1) - LEAStrActiveX.Thick(2)
        ElseIf iCat1 = 4 Then
            LEAStrActiveX.Thick(2) = 18.73403 - LEAStrActiveX.Thick(1)
        End If

    End Sub


    Public Sub Set_TireDataForAC(ByVal CoordX() As Single, ByVal CoordY() As Single,
                              ByVal tire_pressure As Single,
                              ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        ReDim CallAC(1).TirePress(CallAC(1).NTires)
        ReDim CallAC(1).TireX(CallAC(1).NTires)
        ReDim CallAC(1).TireY(CallAC(1).NTires)

        For i = 1 To CallAC(1).NTires
            CallAC(1).TirePress(i) = tire_pressure
            CallAC(1).TireX(i) = CoordX(i)
            CallAC(1).TireY(i) = CoordY(i)
        Next

    End Sub

    Public Sub Set_Eval_Points_FF(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        'If CallAC(1).NTires = 6 Then
        '    vertLimit = 0
        '    'GoTo SixWheels
        'End If


        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        iLoc = 0

        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep

                iLoc = iLoc + 1
                If iLoc = iMaxGear Then
                    iHorGear = i1
                    xHorGear = i1 * horStep

                    jVertGear = j1
                    yVertGear = j1 * vertStep
                End If
            Next
        Next


        Exit Sub

SixWheels:




        Exit Sub

        If CallAC(1).NTires = 4 Then

            CallAC(1).NEvalPoints = 8
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)
            Dim Temp, C, M, libTT, libB As Single

            libTT = 30
            libB = 55

            Temp = libTT * CSng(0.5)
            C = CSng(0.5604) * libTT - CSng(0.2637) * libB
            If C < 0.0! Then C = 0.0!
            M = -C / Temp
            CallAC(1).EvalX(1) = Temp
            CallAC(1).EvalY(1) = 0.0!
            CallAC(1).EvalX(2) = Temp * CSng(0.8)
            CallAC(1).EvalY(2) = C + M * CallAC(1).EvalX(2)
            CallAC(1).EvalX(3) = Temp * CSng(0.6)
            CallAC(1).EvalY(3) = C + M * CallAC(1).EvalX(3)
            CallAC(1).EvalX(4) = Temp * CSng(0.4)
            CallAC(1).EvalY(4) = C + M * CallAC(1).EvalX(4)
            CallAC(1).EvalX(5) = Temp * CSng(0.2)
            CallAC(1).EvalY(5) = C + M * CallAC(1).EvalX(5)
            CallAC(1).EvalX(6) = 0.0!
            CallAC(1).EvalY(6) = C
            CallAC(1).EvalX(7) = 0.0!
            CallAC(1).EvalY(7) = (libB * CSng(0.5) - C) * CSng(0.5) + C
            CallAC(1).EvalX(8) = 0.0!
            CallAC(1).EvalY(8) = libB * CSng(0.5)

        ElseIf CallAC(1).NTires = 6 Then

            Dim Temp As Single


            'ElseIf AC(IA).libGear = "N" Then  ' 777 configuration.
            'AC(IA).libNTires = 6
            'Temp = CSng(AC(IA).libTT * CSng(0.5)) : Temp1 = AC(IA).libB

            'If AC(IA).libACName <> "TU154B" Then
            '    ReDim AC(IA).libTX(AC(IA).libNTires)
            '    ReDim AC(IA).libTY(AC(IA).libNTires)

            '    AC(IA).libTX(1) = -Temp : AC(IA).libTY(1) = -Temp1
            '    AC(IA).libTX(2) = Temp : AC(IA).libTY(2) = -Temp1
            '    AC(IA).libTX(3) = -Temp : AC(IA).libTY(3) = 0.0!
            '    AC(IA).libTX(4) = Temp : AC(IA).libTY(4) = 0.0!
            '    AC(IA).libTX(5) = -Temp : AC(IA).libTY(5) = Temp1
            '    AC(IA).libTX(6) = Temp : AC(IA).libTY(6) = Temp1

            'End If

            CallAC(1).NEvalPoints = 6
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

            'Temp = CallAC(1).TT
            Temp = 55
            CallAC(1).EvalX(1) = CSng(Temp * CSng(0.5)) : CallAC(1).EvalY(1) = 0.0!
            CallAC(1).EvalX(2) = CSng(Temp * 0.4) : CallAC(1).EvalY(2) = 0.0!
            CallAC(1).EvalX(3) = CSng(Temp * 0.3) : CallAC(1).EvalY(3) = 0.0!
            CallAC(1).EvalX(4) = CSng(Temp * 0.2) : CallAC(1).EvalY(4) = 0.0!
            CallAC(1).EvalX(5) = CSng(Temp * 0.1) : CallAC(1).EvalY(5) = 0.0!
            CallAC(1).EvalX(6) = 0.0! : CallAC(1).EvalY(6) = 0.0!

        End If

    End Sub


    Public Sub Set_Eval_Points_Full_Mesh_Div1(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

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

        iEast = CInt(Math.Ceiling(East) / div1)
        iWest = CInt(Math.Floor(West) / div1)
        iSouth = CInt(Math.Floor(South) / div1)
        iNorth = CInt(Math.Ceiling(North) / div1)

        If (iEast - iWest) = 0 Then
            horStep = 1
        Else
            horStep = (East - West) / CSng(Math.Round((East - West) / div1, 0))
        End If

        If (iNorth - iSouth) = 0 Then
            vertStep = 1
        Else
            vertStep = (North - South) / CSng(Math.Round((North - South) / div1, 0))
        End If

        horE_Limit = CInt(Math.Round((East - West) / div1))

        Dim i1, j1, iLoc, ind1 As Integer
        CallAC(1).NEvalPoints = (iEast - iWest + 1) * (iNorth - iSouth + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        iLoc = 0
        For j1 = 0 To iNorth - iSouth Step 1
            For i1 = 0 To iEast - iWest Step 1
                ind1 = (i1 + 1 + j1 + j1 * (iEast - iWest))
                CallAC(1).EvalX(ind1) = West + i1 * horStep
                CallAC(1).EvalY(ind1) = South + j1 * vertStep

                iLoc = iLoc + 1
                If iLoc = iMaxGear Then
                    iHorGear = i1
                    xHorGear = West + i1 * horStep

                    jVertGear = j1
                    yVertGear = South + j1 * vertStep
                End If
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub

    Public Sub Set_Eval_Points_Rigid(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)
        Dim count1 As Integer

        CallAC(1).NEvalPoints = CallAC(1).NTires
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        For count1 = 1 To CallAC(1).NEvalPoints
            CallAC(1).EvalX(j1) = CallAC(1).TireX(count1)
            CallAC(1).EvalY(j1) = CallAC(1).TireY(count1)
        Next


    End Sub





    Public Sub Set_Eval_Points_Wheel_Coord(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

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


        CallAC(1).NEvalPoints = CallAC(1).NTires
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        For j1 = 1 To CallAC(1).NEvalPoints
            CallAC(1).EvalX(j1) = CallAC(1).TireX(j1)
            CallAC(1).EvalY(j1) = CallAC(1).TireY(j1)
        Next

        Exit Sub

        Dim q2 As Integer = 0
        If Symmetry = SymmetryType.XYSymmetry Then 'Set_Eval_Points_Wheel_Coord

        ElseIf Symmetry = SymmetryType.YSymmetry Then
            CallAC(1).NEvalPoints = CInt(CallAC(1).NTires / 2)
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

            For j1 = 1 To CallAC(1).NTires
                If CallAC(1).TireX(j1) > horMiddle Then
                    CallAC(1).EvalX(j1) = CallAC(1).TireX(j1)
                    CallAC(1).EvalY(j1) = CallAC(1).TireY(j1)
                End If
            Next

        ElseIf Symmetry = SymmetryType.XSymmetry Then


        Else 'No Symmetry
            CallAC(1).NEvalPoints = CallAC(1).NTires
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

            For j1 = 1 To CallAC(1).NEvalPoints
                CallAC(1).EvalX(j1) = CallAC(1).TireX(j1)
                CallAC(1).EvalY(j1) = CallAC(1).TireY(j1)
            Next
        End If

    End Sub



    Public Sub Set_Eval_Points_X_longitunal(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        CallAC(1).NEvalPoints = 201
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        Dim j2 As Single = 0

        For j1 = 1 To CallAC(1).NEvalPoints Step 1
            'j2 = CSng(j2 + 0.1)
            j2 = j1 - 1
            CallAC(1).EvalX(j1) = j2
            CallAC(1).EvalY(j1) = 0
        Next

    End Sub



    Public Sub Set_Eval_Points_XYsym(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

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

        horE_Limit = CInt(Math.Round((East - West) / 2 / div1))
        vertN_Limit = CInt(Math.Round((North - South) / 2 / div1))

        If horE_Limit = 0 Then
            horStep = 1
        Else
            horStep = (East - West) / 2 / horE_Limit
        End If

        If vertN_Limit = 0 Then
            vertStep = 1
        Else
            vertStep = (North - South) / 2 / vertN_Limit
        End If

        Dim i1, j1, iLoc As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (vertN_Limit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        iLoc = 0

        For j1 = 0 To vertN_Limit Step 1
            For i1 = 0 To horE_Limit Step 1
                CallAC(1).EvalX(i1 + 1 + j1 + j1 * horE_Limit) = i1 * horStep + horMiddle
                CallAC(1).EvalY(i1 + 1 + j1 + j1 * horE_Limit) = j1 * vertStep + vertMiddle

                iLoc = iLoc + 1
                If iLoc = iMaxGear Then
                    iHorGear = i1
                    xHorGear = i1 * horStep + horMiddle

                    jVertGear = j1
                    yVertGear = j1 * vertStep + vertMiddle
                End If
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub



    Public Sub Set_Eval_Points_Ysym(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

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
        iSouth = CInt(Math.Floor(South) / div1)
        iNorth = CInt(Math.Ceiling(North) / div1)

        horE_Limit = CInt(Math.Round((East - West) / 2 / div1))
        vertN_Limit = CInt(Math.Round((North - South) / div1))

        If horE_Limit = 0 Then
            horStep = 1
        Else
            horStep = (East - West) / 2 / horE_Limit
        End If

        If vertN_Limit = 0 Then
            vertStep = 1
        Else
            vertStep = (North - South) / vertN_Limit
        End If

        Dim i1, j1, iLoc, ind1 As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (iNorth - iSouth + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        iLoc = 0
        For j1 = 0 To iNorth - iSouth Step 1
            For i1 = 0 To horE_Limit Step 1
                ind1 = (i1 + 1 + j1 + j1 * (horE_Limit - 0))
                CallAC(1).EvalX(ind1) = i1 * horStep + horMiddle
                CallAC(1).EvalY(ind1) = j1 * vertStep + South

                iLoc = iLoc + 1
                If iLoc = iMaxGear Then
                    iHorGear = i1
                    xHorGear = i1 * horStep + horMiddle

                    jVertGear = j1
                    yVertGear = j1 * vertStep + South
                End If
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

    End Sub

    Public Sub Set_OneWheelData(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        CallAC(1).GearLoad = CallAC(1).GearLoad / CallAC(1).NTires
        CallAC(1).NTires = 1

        ReDim Preserve CallAC(1).TirePress(CallAC(1).NTires)
        ReDim CallAC(1).TireX(CallAC(1).NTires)
        ReDim CallAC(1).TireY(CallAC(1).NTires)

        For i = 1 To CallAC(1).NTires
            CallAC(1).TireX(i) = 0
            CallAC(1).TireY(i) = 0
        Next

        CallAC(1).NEvalPoints = 251
        CallAC(1).NEvalPoints = 401
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)
        For i = 1 To CallAC(1).NEvalPoints
            CallAC(1).EvalX(i) = i - 1
            CallAC(1).EvalY(i) = 0
        Next

        If True And Not (n254 = CSng(2.54)) Then

            CallAC(1).NEvalPoints = 600 * gN1 + 1
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

            For i = 1 To CallAC(1).NEvalPoints
                'CallAC(1).EvalX(i) = (i - 1) / n254
                CallAC(1).EvalX(i) = (i - 1) * 1 / gN1
                CallAC(1).EvalY(i) = 0
            Next


        ElseIf n254 = 1 Then

            CallAC(1).NEvalPoints = 211
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

            For i = 1 To 101
                CallAC(1).EvalX(i) = (i - 1) / n254
                CallAC(1).EvalY(i) = 0
            Next

            For i = 102 To 151
                CallAC(1).EvalX(i) = CallAC(1).EvalX(i - 1) + 2 / n254
                CallAC(1).EvalY(i) = 0
            Next

            For i = 152 To 211
                CallAC(1).EvalX(i) = CallAC(1).EvalX(i - 1) + 5 / n254
                CallAC(1).EvalY(i) = 0
            Next


        ElseIf n254 = CSng(2.54) Then

            Dim d1, d2, d3, d4 As Double
            d1 = (North - South) + 160
            d2 = West - East
            d3 = Math.Sqrt(d1 * d1 + d2 * d2)



            CallAC(1).NEvalPoints = 517
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

            For i = 1 To 101
                CallAC(1).EvalX(i) = (i - 1) / n254
                CallAC(1).EvalY(i) = 0
            Next

            For i = 102 To 301
                CallAC(1).EvalX(i) = CallAC(1).EvalX(i - 1) + 2 / n254
                CallAC(1).EvalY(i) = 0
            Next

            For i = 302 To 517
                CallAC(1).EvalX(i) = CallAC(1).EvalX(i - 1) + 5 / n254
                CallAC(1).EvalY(i) = 0
            Next

        End If




    End Sub


    Public Sub Set_SWLdata(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)
        Dim coef1 As Single

        'AliA
        'If gCat = 1 Then
        '    coef1 = 2.2
        'ElseIf gCat = 2 Then
        '    coef1 = 1.5
        'ElseIf gCat = 3 Then
        '    coef1 = 1.2
        'Else
        '    coef1 = 1.07
        'End If

        If gCat = 1 Then

            If gPavementType = clsACR.PavementType.Flexible Then
                If CallAC(1).NTires > 1 Then
                    coef1 = CSng(1 / CallAC(1).NTires * 2)
                End If
            Else
                coef1 = CSng(1 / CallAC(1).NTires)
            End If

        ElseIf gCat = 2 Then
            coef1 = gRatio1
        ElseIf gCat = 3 Then
            coef1 = gRatio2 / gRatio1 * gRatio2
        ElseIf gCat = 4 Then
            coef1 = gRatio3 / gRatio2 * gRatio3
        End If

        'CallAC(1).GearLoad = CallAC(1).GearLoad / 4 * 1.5

        'AliA
        'CallAC(1).GearLoad = CallAC(1).GearLoad / CallAC(1).NTires * coef1
        CallAC(1).GearLoad = CallAC(1).GearLoad * coef1

        CallAC(1).NTires = 1

        ReDim CallAC(1).TirePress(CallAC(1).NTires)
        ReDim CallAC(1).TireX(CallAC(1).NTires)
        ReDim CallAC(1).TireY(CallAC(1).NTires)

        'paragraph 1.1.3.2 d) Mathematically derived single wheel load
        For i = 1 To CallAC(1).NTires
            CallAC(1).TirePress(i) = 1.5 * MPaToPsi '1500 kPa  1.50 MPa
            CallAC(1).TireX(i) = 0
            CallAC(1).TireY(i) = 0
        Next

        CallAC(1).NEvalPoints = 1
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)
        For i = 1 To CallAC(1).NEvalPoints
            CallAC(1).EvalX(i) = 0
            CallAC(1).EvalY(i) = 0
        Next

    End Sub



    Public Sub Set_Eval_Points_Ysym_2gears(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
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
        iSouth = CInt(Math.Floor(South) / div1)
        iNorth = CInt(Math.Ceiling(North) / div1)

        'horE_Limit = CInt(Math.Ceiling((East - West) / 2 / div1))
        'vertN_Limit = CInt(Math.Ceiling((North - South) / div1))

        horE_Limit = CInt(Math.Round((East - West) / 2 / div1))
        vertN_Limit = CInt(Math.Round((North - South) / div1))

        If horE_Limit = 0 Then
            horStep = 1
        Else
            horStep = (East - West) / 2 / horE_Limit
        End If

        If vertN_Limit = 0 Then
            vertStep = 1
        Else
            vertStep = (North - South) / vertN_Limit
        End If

        Dim i1, j1, iLoc, ind1 As Integer
        CallAC(1).NEvalPoints = (horE_Limit + 1) * (iNorth - iSouth + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        iLoc = 0
        For j1 = 0 To iNorth - iSouth Step 1
            For i1 = 0 To horE_Limit Step 1
                ind1 = (i1 + 1 + j1 + j1 * (horE_Limit - 0))
                CallAC(1).EvalX(ind1) = i1 * horStep + horMiddle
                CallAC(1).EvalY(ind1) = j1 * vertStep + South

                iLoc = iLoc + 1
                If iLoc = iMaxGear Then
                    iHorGear = i1
                    xHorGear = i1 * horStep + horMiddle

                    jVertGear = j1
                    yVertGear = j1 * vertStep + South
                End If
            Next
        Next

        gMeshSizeCount = CallAC(1).NEvalPoints

        '*****   Adding wheel coordinates as evaluation points   *****
        Dim n1Tires As Integer
        n1Tires = UBound(C_X, 1)

        CallAC(1).NEvalPoints = CallAC(1).NEvalPoints + n1Tires
        ReDim Preserve CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim Preserve CallAC(1).EvalY(CallAC(1).NEvalPoints)

        Dim q1, c2 As Integer
        q1 = CallAC(1).NEvalPoints - n1Tires

        c2 = 0
        For j1 = q1 + 1 To CallAC(1).NEvalPoints
            c2 = c2 + 1
            CallAC(1).EvalX(j1) = C_X(c2)
            CallAC(1).EvalY(j1) = C_Y(c2)
        Next

        'Call Print_Evaluation_Points_2gears(CallAC)

    End Sub


    Public Sub Set_TireDataForAC_belly(ByVal CoordX2() As Single, ByVal CoordY2() As Single,
                      ByVal tire_pressure As Single,
                      ByVal CallAC_belly() As ACRClassLib.clsLEAF.LEAFACParms)

        ReDim CallAC_belly(1).TirePress(CallAC_belly(1).NTires)
        ReDim CallAC_belly(1).TireX(CallAC_belly(1).NTires)
        ReDim CallAC_belly(1).TireY(CallAC_belly(1).NTires)

        For i = 1 To CallAC_belly(1).NTires
            CallAC_belly(1).TirePress(i) = tire_pressure
            CallAC_belly(1).TireX(i) = CoordX2(i)
            CallAC_belly(1).TireY(i) = CoordY2(i)
        Next

    End Sub





End Module
