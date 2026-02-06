Option Strict On
Option Explicit On

'Imports System.Windows.Forms.Form

Module clsACNsub

    Public XGridMax, XGridOrigin As Double
    Public YGridMax, YGridOrigin As Double

    Public Function ICAOCodeIndexF(ByVal modul1 As Single) As Integer
        'clsACNsub

        'paragraph 1.1.3.2 a) Subgrade category
        Dim divis1, divis2, divis3 As Single
        divis1 = 21755.66 '150 MPa 
        divis2 = 14503.7738  '100 Mpa
        divis3 = 8702.264    '60 Mpa

        'Dim d1, d2, d3 As Double
        'd1 = 150 * MPaToPsi
        'd2 = 100 * MPaToPsi
        'd3 = 60 * MPaToPsi

        If modul1 >= divis1 Then ICAOCodeIndexF = 4 '                    Category A
        If modul1 >= divis2 And modul1 < divis1 Then ICAOCodeIndexF = 3 'Category B
        If modul1 >= divis3 And modul1 < divis2 Then ICAOCodeIndexF = 2 'Category C
        If modul1 < divis3 Then ICAOCodeIndexF = 1 '                     Category D

    End Function


    Public Enum SymmetryType
        XYSymmetry = 1
        XSymmetry = 2
        YSymmetry = 3
        NoSymmetry = 4
    End Enum

    Public Symmetry As SymmetryType

    '    C1 = CallAC
    'CallAC(1).ACname = "NewName"

    'http://www.vbdotnetforums.com/vb-net-general-discussion/39335-copying-object.html


    Public Function AddTwo(ByVal num1 As Integer, ByVal num2 As Integer) As Integer
        AddTwo = num1 + num2

    End Function


    Public Sub Create_Eval_Points(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        'Symmetry = SymmetryType.XYSymmetry
        'Symmetry = SymmetryType.YSymmetry
        'Symmetry = SymmetryType.NoSymmetry

        If bMeshSize Then
            If bSelectWheels Then
                Call Set_Eval_Points_Ysym_Mesh2_SW(CallAC) 'bSelectWheels
            ElseIf Symmetry = SymmetryType.XYSymmetry Then 'Calculate ACN Flexible
                Call Set_Eval_Points_XYsym_Mesh(CallAC)
            ElseIf Symmetry = SymmetryType.YSymmetry Then
                Call Set_Eval_Points_Ysym_Mesh2(CallAC)
                'Call Set_Eval_Points_Ysym(CallAC)
            Else
                Call Set_Eval_Points_XYsym_Mesh_Full2(CallAC)
            End If
        Else
            Call Mesh_All_Cases_div1(CallAC)
        End If

        If iSelectEval_ACN <> 2 Then '2 is only Mesh
            Call Create_Eval_Points_Under_Wheels(CallAC)
        End If
        Call Print_Evaluation_Points(CallAC)
        'Call Print_Evaluation_Points2(CallAC) 'to comment123

        If True Then Exit Sub

        If Symmetry = SymmetryType.XYSymmetry Then 'Calculate ACN Flexible
            If bMeshSize Then
                Call Set_Eval_Points_XYsym_Mesh(CallAC)
            Else
                Call Set_Eval_Points_XYsym(CallAC)
            End If

        ElseIf Symmetry = SymmetryType.YSymmetry Then

            If bMeshSize Then
                Call Set_Eval_Points_Ysym_Mesh(CallAC)
            Else
                Call Set_Eval_Points_Ysym(CallAC)
            End If

            'Call Set_Eval_Points_Ysym(CallAC)
        Else
            If bMeshSize Then
                Call Set_Eval_Points_XYsym_Mesh_Full(CallAC)
            Else
                Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
            End If
        End If

        'Call Set_Eval_Points_Dim(CallAC)

        If iSelectEval_ACN <> 2 Then '2 is only Mesh
            Call Create_Eval_Points_Under_Wheels(CallAC)
        End If
        Call Print_Evaluation_Points(CallAC)


    End Sub

    Public Sub Create_Eval_Points_2gears(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                                     ByRef C_X() As Single, ByRef C_Y() As Single)

        'Symmetry = SymmetryType.XYSymmetry
        'Symmetry = SymmetryType.YSymmetry
        'Symmetry = SymmetryType.NoSymmetry

        If Symmetry = SymmetryType.XYSymmetry Then 'Calculate ACN Flexible
            'Call Set_Eval_Points_XYsym(CallAC)
            If bMeshSize Then
                If bSelectWheels Then
                    Call Set_Eval_Points_Ysym_Mesh_2gearsCG_SW(CallAC, C_X, C_Y)
                Else
                    'Call Set_Eval_Points_Ysym_Mesh_2gears(CallAC, C_X, C_Y)
                    Call Set_Eval_Points_Ysym_Mesh_2gearsCG(CallAC, C_X, C_Y)
                End If

            Else
                Call Set_Eval_Points_Ysym_2gears(CallAC, C_X, C_Y)
            End If
        ElseIf Symmetry = SymmetryType.YSymmetry Then
            If bMeshSize Then

                If bSelectWheels Then
                    Call Set_Eval_Points_Ysym_Mesh_2gearsCG_SW(CallAC, C_X, C_Y)
                Else

                    'Call Set_Eval_Points_XYsym_Mesh(CallAC)
                    'Call Set_Eval_Points_Ysym_Mesh_2gears(CallAC, C_X, C_Y)
                    Call Set_Eval_Points_Ysym_Mesh_2gearsCG(CallAC, C_X, C_Y)
                End If

            Else
                'Call Set_Eval_Points_XYsym(CallAC)
                Call Set_Eval_Points_Ysym_2gears(CallAC, C_X, C_Y)
            End If
        Else
            If bMeshSize Then

            Else

            End If


            'Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
        End If

        'Call Set_Eval_Points_Dim(CallAC)

    End Sub

    Public Sub Get_Center_of_Gravity_2gears(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                            ByVal CallAC_Belly() As ACRClassLib.clsLEAF.LEAFACParms,
                                            ByRef Xcg As Double, ByRef Ycg As Double)
        Dim icount As Integer
        Xcg = 0 : Ycg = 0

        'If bSelectWheels Then

        '    For icount = 1 To CallAC(1).NTires
        '        Xcg = Xcg + CallAC(1).TireX(icount)
        '        Ycg = Ycg + CallAC(1).TireY(icount)
        '    Next

        '    For icount = 1 To CallAC_Belly(1).NTires
        '        Xcg = Xcg + CallAC_Belly(1).TireX(icount)
        '        Ycg = Ycg + CallAC_Belly(1).TireY(icount)
        '    Next

        '    Xcg = Xcg / (CallAC(1).NTires + CallAC_Belly(1).NTires)
        '    Ycg = Ycg / (CallAC(1).NTires + CallAC_Belly(1).NTires)

        'Else

        For icount = 1 To CallAC(1).NTires
            Xcg = Xcg + CallAC(1).TireX(icount)
            Ycg = Ycg + CallAC(1).TireY(icount)
        Next

        For icount = 1 To CallAC_Belly(1).NTires
            Xcg = Xcg + CallAC_Belly(1).TireX(icount)
            Ycg = Ycg + CallAC_Belly(1).TireY(icount)
        Next

        Xcg = Xcg / (CallAC(1).NTires + CallAC_Belly(1).NTires)
        Ycg = Ycg / (CallAC(1).NTires + CallAC_Belly(1).NTires)

        'End If



    End Sub

    Public Sub Get_Center_of_Gravity_2(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                         ByRef Xcg As Double, ByRef Ycg As Double)

        Try

            Dim icount As Integer
            Xcg = 0 : Ycg = 0

            Dim WheelNumber1 As Integer
            WheelNumber1 = 0

            For icount = 1 To CallAC(1).NTires
                If gSW(icount) = 1 Then
                    WheelNumber1 = WheelNumber1 + 1
                    Xcg = Xcg + CallAC(1).TireX(icount)
                    Ycg = Ycg + CallAC(1).TireY(icount)
                End If
            Next

            Xcg = Xcg / WheelNumber1
            Ycg = Ycg / WheelNumber1


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Sub


    Public Sub Get_Center_of_Gravity(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                             ByRef Xcg As Double, ByRef Ycg As Double)

        Try

            Dim icount As Integer
            Xcg = 0 : Ycg = 0

            For icount = 1 To CallAC(1).NTires
                Xcg = Xcg + CallAC(1).TireX(icount)
                Ycg = Ycg + CallAC(1).TireY(icount)
            Next
            Xcg = Xcg / CallAC(1).NTires
            Ycg = Ycg / CallAC(1).NTires


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub



    Public Sub Center_of_Gravity_Eval_2gears(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                             ByRef C_X() As Single, ByRef C_Y() As Single)
        Try

            'center of gravity
            Dim Xcg, Ycg As Double, icount As Integer
            Xcg = 0 : Ycg = 0

            For icount = 1 To CallAC(1).NTires
                Xcg = Xcg + CallAC(1).TireX(icount)
                Ycg = Ycg + CallAC(1).TireY(icount)
            Next
            Xcg = Xcg / CallAC(1).NTires : Ycg = Ycg / CallAC(1).NTires

            gMeshSizeCount = 1


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub




    Public Sub Center_of_Gravity_Eval(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        'center of gravity
        Dim Xcg, Ycg As Double, icount As Integer
        Xcg = 0 : Ycg = 0

        For icount = 1 To CallAC(1).NTires
            Xcg = Xcg + CallAC(1).TireX(icount)
            Ycg = Ycg + CallAC(1).TireY(icount)
        Next
        Xcg = Xcg / CallAC(1).NTires : Ycg = Ycg / CallAC(1).NTires

        CallAC(1).NEvalPoints = 1
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

        CallAC(1).EvalX(1) = Xcg
        CallAC(1).EvalY(1) = Ycg
        gMeshSizeCount = 1

        Call Create_Eval_Points_Under_Wheels(CallAC)
        Call Print_Evaluation_Points(CallAC)

    End Sub


    Public Sub Create_Eval_Points_Under_Wheels(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        '*****   Adding wheel coordinates as evaluation points   *****
        Dim q1, c2 As Integer
        q1 = CallAC(1).NEvalPoints

        CallAC(1).NEvalPoints = CallAC(1).NEvalPoints + CallAC(1).NTires
        ReDim Preserve CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim Preserve CallAC(1).EvalY(CallAC(1).NEvalPoints)

        c2 = 0
        For j1 = q1 + 1 To CallAC(1).NEvalPoints
            c2 = c2 + 1
            CallAC(1).EvalX(j1) = CallAC(1).TireX(c2)
            CallAC(1).EvalY(j1) = CallAC(1).TireY(c2)
        Next

    End Sub




    Public Sub Create_Eval_Points_Under_Wheels_2gears(ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms,
                                                      ByRef C_X() As Single, ByRef C_Y() As Single)

        '*****   Adding wheel coordinates as evaluation points   *****
        Dim q1, c2 As Integer
        q1 = CallAC(1).NEvalPoints

        CallAC(1).NEvalPoints = CallAC(1).NEvalPoints + UBound(C_X, 1)
        ReDim Preserve CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim Preserve CallAC(1).EvalY(CallAC(1).NEvalPoints)

        c2 = 0
        For i = q1 + 1 To CallAC(1).NEvalPoints
            c2 = c2 + 1
            CallAC(1).EvalX(i) = C_X(c2)
            CallAC(1).EvalY(i) = C_Y(c2)
        Next


    End Sub






    Public Sub Make_LEAStr_Copy(ByRef dat1 As ACRClassLib.clsLEAF.LEAFStrParms,
                                ByRef tar1 As ACRClassLib.clsLEAF.LEAFStrParms)

        tar1.NLayers = dat1.NLayers
        ReDim tar1.Thick(tar1.NLayers)
        ReDim tar1.Modulus(tar1.NLayers)
        ReDim tar1.Poisson(tar1.NLayers)
        ReDim tar1.InterfaceParm(tar1.NLayers)

        For i9 As Integer = 1 To tar1.NLayers
            tar1.Thick(i9) = dat1.Thick(i9)
            tar1.Modulus(i9) = dat1.Modulus(i9)
            tar1.Poisson(i9) = dat1.Poisson(i9)
            tar1.InterfaceParm(i9) = dat1.InterfaceParm(i9)
        Next

        tar1.EvalDepth = dat1.EvalDepth
        tar1.EvalLayer = dat1.EvalLayer


    End Sub



    Public Sub Make_CallAC_Copy(ByVal dat1 As ACRClassLib.clsLEAF.LEAFACParms,
                                ByRef tar1 As ACRClassLib.clsLEAF.LEAFACParms)


        tar1.ACname = dat1.ACname
        tar1.GearLoad = dat1.GearLoad
        tar1.NTires = dat1.NTires
        ReDim tar1.TirePress(dat1.NTires)
        ReDim tar1.TireX(dat1.NTires)
        ReDim tar1.TireY(dat1.NTires)

        For i9 As Integer = 1 To tar1.NTires
            tar1.TirePress(i9) = dat1.TirePress(i9)
            tar1.TireX(i9) = dat1.TireX(i9)
            tar1.TireY(i9) = dat1.TireY(i9)
        Next

        tar1.NEvalPoints = dat1.NEvalPoints
        ReDim tar1.EvalX(dat1.NEvalPoints)
        ReDim tar1.EvalY(dat1.NEvalPoints)

        For i9 As Integer = 1 To tar1.NEvalPoints
            tar1.EvalX(i9) = dat1.EvalX(i9)
            tar1.EvalY(i9) = dat1.EvalY(i9)
        Next

        tar1.libGear = dat1.libGear


    End Sub


    Public Sub GearCG(ByRef XW() As Single, ByRef YW() As Single, ByRef N As Integer)

        Dim I As Integer

        Dim Xcg, Ycg As Double
        Dim XRcg(N) As Double, YRcg(N) As Double, IXRcg(N) As Double, IYRcg(N) As Double


        Xcg = 0 : Ycg = 0
        For I = 1 To N
            Xcg += XW(I)
            Ycg += YW(I)
        Next I
        Xcg /= N
        Ycg /= N

        For I = 1 To N
            XRcg(I) = XW(I) - Xcg
            YRcg(I) = YW(I) - Ycg
            IXRcg(I) = CDbl(IIf(XRcg(I) > 0, Math.Floor(XRcg(I)), Math.Ceiling(XRcg(I)))) ' Test symmetry to the nearest inch.
            IYRcg(I) = CDbl(IIf(YRcg(I) > 0, Math.Floor(YRcg(I)), Math.Ceiling(YRcg(I)))) 'PPPP
        Next I

        Dim NXSymmetric As Integer = 0 : Dim NYSymmetric As Integer = 0
        For I = 1 To N
            For J As Integer = 1 To N
                ' Test symmetry about the X axis.
                If IXRcg(I) = IXRcg(J) And IYRcg(I) = -IYRcg(J) And I <> J Then
                    NXSymmetric += 1
                End If
                ' Test symmetry about the Y axis.
                If IYRcg(I) = IYRcg(J) And IXRcg(I) = -IXRcg(J) And I <> J Then
                    NYSymmetric += 1
                End If
            Next J
            ' Wheels on an axis are symmetric but not found above.
            If IYRcg(I) = 0 Then NXSymmetric += 1
            If IXRcg(I) = 0 Then NYSymmetric += 1
        Next I

        XGridOrigin = 1.0E+35 : XGridMax = -1.0E+35
        YGridOrigin = 1.0E+35 : YGridMax = -1.0E+35
        If NXSymmetric = N And NYSymmetric = N Then ' Symmetric about both axes.
            Symmetry = SymmetryType.XYSymmetry 'GearCG
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
            Next I
            XGridOrigin += Xcg
            XGridMax = Xcg
            YGridOrigin += Ycg
            YGridMax = Ycg
        ElseIf NXSymmetric = N And NYSymmetric < N Then  ' Symmetric about X axis.
            Symmetry = SymmetryType.XSymmetry
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
                If XRcg(I) > XGridMax Then XGridMax = XRcg(I)
            Next I
            XGridOrigin += Xcg
            XGridMax += Xcg
            YGridOrigin += Ycg
            YGridMax = Ycg
        ElseIf NXSymmetric < N And NYSymmetric = N Then  ' Symmetric about Y axis.
            Symmetry = SymmetryType.YSymmetry
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
                If YRcg(I) > YGridMax Then YGridMax = YRcg(I)
            Next I
            XGridOrigin += Xcg
            XGridMax = Xcg
            YGridOrigin += Ycg
            YGridMax += Ycg
        ElseIf NXSymmetric < N And NYSymmetric < N Then  ' No symmetry.
            Symmetry = SymmetryType.NoSymmetry
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If XRcg(I) > XGridMax Then XGridMax = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
                If YRcg(I) > YGridMax Then YGridMax = YRcg(I)
            Next I
            XGridOrigin += Xcg
            XGridMax += Xcg
            YGridOrigin += Ycg
            YGridMax += Ycg
        Else
            ' Two tires in the same place.
            '    Error recovery here
        End If

        'Symmetry = SymmetryType.NoSymmetry


    End Sub


End Module
