Option Strict On
Option Explicit On

Friend Module TwoGears


    Dim StressX(), StressY(), StressXY() As Double
    Dim StressX1(), StressY1(), StressXY1() As Double
    Dim StressX2(), StressY2(), StressXY2() As Double
    Dim StressHor() As Double

    Friend Sub Combine_Coord(
            ByRef CoordX() As Single,
            ByRef CoordY() As Single,
            ByRef CoordX2() As Single,
            ByRef CoordY2() As Single,
            ByRef C_X() As Single,
            ByRef C_Y() As Single)

        Dim ind1, ind2 As Integer
        ind1 = UBound(CoordX, 1)
        ind2 = UBound(CoordX2, 1)

        For i = 1 To ind1
            C_X(i) = CoordX(i)
            C_Y(i) = CoordY(i)
        Next

        For i = 1 To ind2
            C_X(i + ind1) = CoordX2(i)
            C_Y(i + ind1) = CoordY2(i)
        Next
    End Sub



    Friend Sub Calculate_Rigid_Stress_2gears(ByRef CallAC() As clsLEAF.LEAFACParms,
                                             ByRef CallAC_belly() As clsLEAF.LEAFACParms)

        Dim Dim1 As Integer, a1 As Double

        Response = Nothing
        LEAStrActiveX.EvalDepth = LEAStrActiveX.Thick(1)
        LEAStrActiveX.EvalLayer = 1

        LeafResp = ACRClassLib.clsLEAF.LEAFoptions.HorizontalStress
        LeafResp = ACRClassLib.clsLEAF.LEAFoptions.AllResponses

        RunLEAF = New ACRClassLib.clsLEAF()
        Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC,
            LEAStrActiveX, Response, AllResp)
        RunLEAF = Nothing

        If Response Is Nothing Then
            gStress = 0
        Else
            'gStress = Response(1, 1)
            'gStress = AllResp(1, 1).StressX
        End If

        Dim1 = UBound(AllResp, 2)
        ReDim StressX1(Dim1), StressX2(Dim1), StressX(Dim1)
        ReDim StressY1(Dim1), StressY2(Dim1), StressY(Dim1)
        ReDim StressXY1(Dim1), StressXY2(Dim1), StressXY(Dim1)
        ReDim StressHor(Dim1)

        For i = 1 To Dim1
            Response(1, i) = AllResp(1, i).StressX
            StressX1(i) = AllResp(1, i).StressX
            StressY1(i) = AllResp(1, i).StressY
            StressXY1(i) = AllResp(1, i).StressXY
        Next

        '==========   Calculations for belly gear ==========
        RunLEAF = New ACRClassLib.clsLEAF()
        Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC_belly,
            LEAStrActiveX, Response, AllResp)
        RunLEAF = Nothing

        For i = 1 To Dim1
            StressX2(i) = AllResp(1, i).StressX
            StressY2(i) = AllResp(1, i).StressY
            StressXY2(i) = AllResp(1, i).StressXY
        Next

        For i = 1 To Dim1
            StressX(i) = StressX1(i) + StressX2(i)
            StressY(i) = StressY1(i) + StressY2(i)
            StressXY(i) = StressXY1(i) + StressXY2(i)
        Next

        For i = 1 To Dim1
            a1 = ((StressX(i) - StressY(i)) / 2) ^ 2 + StressXY(i) ^ 2
            StressHor(i) = (StressX(i) + StressY(i)) / 2 + Math.Sqrt(a1)
            Response(1, i) = StressHor(i)
        Next

        'Call Print_Strain_Array_Full_Mesh(LEAStrActiveX)
        Dim max1 As Double
        max1 = -1000000

        For i = 1 To Dim1
            If StressHor(i) > max1 Then
                max1 = StressHor(i)
            End If
        Next

        gStress = max1

        'Call Print_Strain_Array_Full_Mesh(LEAStrActiveX)

    End Sub


    Friend Sub Calculate_Thickness_Rigid_2gears(ByRef CallAC() As clsLEAF.LEAFACParms,
                                             ByRef CallAC_belly() As clsLEAF.LEAFACParms)
        Dim SS1, SS2 As Double, tt1, tt2 As Double

        Call Calculate_Rigid_Stress_2gears(CallAC, CallAC_belly)

        If Math.Abs(gStress - StressACN) > StressDelta Then
            If gStress > StressACN Then
                Do
                    tt2 = LEAStrActiveX.Thick(1) : SS2 = gStress
                    LEAStrActiveX.Thick(1) = LEAStrActiveX.Thick(1) - 2
                    If LEAStrActiveX.Thick(1) <= 2 Then LEAStrActiveX.Thick(1) = 2
                    Call Calculate_Rigid_Stress_2gears(CallAC, CallAC_belly)
                    tt1 = LEAStrActiveX.Thick(1) : SS1 = gStress
                    If LEAStrActiveX.Thick(1) = 2 Then Exit Do
                Loop Until (gStress < StressACN) Or Math.Abs(gStress - StressACN) < StressDelta
            Else
                Do
                    tt1 = LEAStrActiveX.Thick(1) : SS1 = gStress
                    LEAStrActiveX.Thick(1) = LEAStrActiveX.Thick(1) + 4
                    Call Calculate_Rigid_Stress_2gears(CallAC, CallAC_belly)
                    tt2 = LEAStrActiveX.Thick(1) : SS2 = gStress
                Loop Until (gStress > StressACN) Or Math.Abs(gStress - StressACN) < StressDelta
            End If
        Else
            GoTo finish1
        End If

        If Math.Abs(gStress - StressACN) < StressDelta Then
            GoTo finish1
        End If

        If LEAStrActiveX.Thick(1) <= 2 Then
            If StressACN > gStress Then
                GoTo finish1
            End If
        End If


        LEAStrActiveX.Thick(1) = (tt1 + tt2) / 2
        Call Calculate_Rigid_Stress_2gears(CallAC, CallAC_belly)

        Dim zz1, zz2, aa, bb As Double

rep1:

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)

        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1

        Dim ttACN As Double

        ttACN = aa * Math.Log(StressACN) + bb
        LEAStrActiveX.Thick(1) = ttACN
        Call Calculate_Rigid_Stress_2gears(CallAC, CallAC_belly)

        If gStress < StressACN Then
            tt2 = LEAStrActiveX.Thick(1)
            SS2 = gStress
        Else
            tt1 = LEAStrActiveX.Thick(1)
            SS1 = gStress
        End If

        If Math.Abs(gStress - StressACN) > StressDelta Then
            GoTo rep1
        End If

finish1:
        StressGear = gStress

    End Sub





End Module



