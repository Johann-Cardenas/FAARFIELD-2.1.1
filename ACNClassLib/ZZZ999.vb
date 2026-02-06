
Friend Module ZZZ999
    'This is test
    'ver100 2017.10.13 Friday
    Friend gN1 As Integer = 1
    Friend Const n254 As Single = 2.54
    'Friend Const n254 As Single = 1


    Private Sub Try1()

        Try

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub









    Public Sub Create_Matrix(ByRef str1() As Double)

        ReDim str1(211)

        For i1 As Integer = 1 To 101
            str1(i1) = i1 - 1
        Next

        For i1 As Integer = 102 To 151
            str1(i1) = str1(i1 - 1) + 2
        Next

        For i1 As Integer = 152 To 211
            str1(i1) = str1(i1 - 1) + 5
        Next

    End Sub

    Public Function Get_Strain99(ByVal dist1 As Double, ByRef str1() As Double,
                              ByRef CallAC() As ACRClassLib.clsLEAF.LEAFACParms) As Double

        Try

            Dim in1, in2 As Integer
            Dim strain1, strain2 As Double
            Dim dx1, dy1 As Double
            Dim dist_254 As Double

            dist_254 = dist1 * n254
            'dist1 = dist1 * n254


            If dist_254 <= 100 Then
                dx1 = 1 / n254
                in1 = Math.Floor(dist_254)
            ElseIf dist_254 <= 500 Then
                dx1 = 2 / n254
                in1 = 100 + Math.Floor((dist_254 - 100) / 2)
            ElseIf dist_254 <= 1580 - 5 Then
                dx1 = 5 / n254
                in1 = 300 + Math.Floor((dist_254 - 500) / 5)
            End If


            If dist_254 <= 1580 - 5 Then
                in2 = in1 + 1
                strain1 = str1(in1)
                strain2 = str1(in2)


                'Dim dd1 As Double = dist1 / 2.54

                'If (dd1 <= CallAC(1).EvalX(in2)) And (dd1 >= CallAC(1).EvalX(in2)) Then
                'Else
                '    MsgBox(dd1, MsgBoxStyle.Information, CallAC(1).EvalX(in2))
                'End If

                dy1 = strain2 - strain1
                Get_Strain99 = strain1 + dy1 / dx1 * (dist1 - CallAC(1).EvalX(in1 + 1))
            Else
                Get_Strain99 = str1(UBound(str1, 1))
            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Function




    Public Function Get_Strain2(ByVal dist1 As Double, ByRef str1() As Double,
                                 ByRef CallAC() As ACRClassLib.clsLEAF.LEAFACParms) As Double

        Try

            Dim in1, in2 As Integer
            Dim strain1, strain2 As Double
            Dim dx1, dy1 As Double
            Dim dimen1 As Integer

            dist1 = dist1 * n254


            If False Then

                If dist1 * gN1 <= (UBound(str1, 1) - 1) Then

                    dx1 = 1 / gN1
                    in1 = Math.Floor(dist1 / dx1)
                    in2 = in1 + 1

                    strain1 = str1(in1)
                    strain2 = str1(in2)
                    dy1 = strain2 - strain1
                    Get_Strain2 = strain1 + dy1 / n254 * (dist1 - in1 * dx1)

                Else
                    Get_Strain2 = str1(UBound(str1, 1))

                End If



            ElseIf n254 = 1 Then

                If dist1 <= 100 Then

                    dx1 = 1
                    in1 = Math.Floor(dist1)
                    in2 = in1 + 1

                    strain1 = str1(in1)
                    strain2 = str1(in2)
                    dy1 = strain2 - strain1
                    Get_Strain2 = strain1 + dy1 / n254 * (dist1 - in1)


                ElseIf dist1 <= 200 Then

                    dx1 = 2
                    in1 = 101 + Math.Floor((dist1 - 100) / dx1)
                    in2 = in1 + 1

                    strain1 = str1(in1)
                    strain2 = str1(in2)
                    dy1 = strain2 - strain1
                    Get_Strain2 = strain1 + dy1 / n254 * (dist1 - CInt(dist1))

                ElseIf dist1 <= 500 - 5 Then
                    dx1 = 5
                    in1 = 151 + Math.Floor((dist1 - 200) / dx1)
                    in2 = in1 + 1

                    strain1 = str1(in1)
                    strain2 = str1(in2)
                    dy1 = strain2 - strain1
                    Get_Strain2 = strain1 + dy1 / n254 * (dist1 - CInt(dist1))

                Else
                    Get_Strain2 = str1(211)

                End If

            ElseIf n254 = CSng(2.54) Then

                If dist1 <= 100 Then
                    dx1 = 1 / n254
                    in1 = Math.Floor(dist1)
                ElseIf dist1 <= 500 Then
                    dx1 = 2 / n254
                    in1 = 100 + Math.Floor((dist1 - 100) / 2)
                ElseIf dist1 <= 1580 - 5 Then
                    dx1 = 5 / n254
                    in1 = 300 + Math.Floor((dist1 - 500) / 5)
                End If


                If dist1 <= 1580 - 5 Then
                    in2 = in1 + 1
                    strain1 = str1(in1)
                    strain2 = str1(in2)


                    Dim dd1 As Double = dist1 / 2.54

                    'If (dd1 <= CallAC(1).EvalX(in2)) And (dd1 >= CallAC(1).EvalX(in2)) Then
                    'Else
                    '    MsgBox(dd1, MsgBoxStyle.Information, CallAC(1).EvalX(in2))
                    'End If

                    dy1 = strain2 - strain1
                    'Get_Strain2 = strain1 + dy1 / dx1  * (dist1 - CInt(dist1)) / n254
                    Get_Strain2 = strain1 + dy1 / dx1 * (dist1 / n254 - CallAC(1).EvalX(in1 + 1))
                Else
                    Get_Strain2 = str1(UBound(str1, 1))
                End If


            End If






        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Function


    Public Function Get_Strain8(ByVal dist1 As Double, ByRef str1() As Double,
                                ByRef CallAC() As ACRClassLib.clsLEAF.LEAFACParms) As Double

        Try

            Dim strain1, strain2 As Double
            Dim dx1, dy1 As Double
            Dim in1, i1 As Integer
            Dim dim1 As Integer, dim2 As Integer
            dim1 = UBound(CallAC(1).EvalX, 1)
            dim2 = UBound(str1, 1)

            Dim start1 As Integer
            start1 = 1


            If dist1 = 0 Then
                in1 = 1
            Else


                'If dist1 > 1000000 Then
                '    start1 = CallAC(1).NEvalPoints
                'ElseIf dist1 >= CallAC(1).EvalX(CallAC(1).NEvalPoints) Then
                '    start1 = CallAC(1).NEvalPoints

                'ElseIf dist1 >= CallAC(1).EvalX(500) Then
                '    start1 = 500
                'ElseIf dist1 >= CallAC(1).EvalX(400) Then
                '    start1 = 400

                'ElseIf dist1 >= CallAC(1).EvalX(300) Then
                '    start1 = 300

                'ElseIf dist1 >= CallAC(1).EvalX(200) Then
                '    start1 = 200

                'ElseIf dist1 >= CallAC(1).EvalX(100) Then
                '    start1 = 100

                'ElseIf dist1 >= CallAC(1).EvalX(50) Then
                '    start1 = 50

                'End If


                If dist1 > 1000000 Then
                    start1 = CallAC(1).NEvalPoints
                ElseIf dist1 >= CallAC(1).EvalX(CallAC(1).NEvalPoints) Then
                    start1 = CallAC(1).NEvalPoints

                ElseIf dist1 >= CallAC(1).EvalX(502) Then
                    start1 = 502
                Else
                    For i9 As Integer = 501 To 1 Step -100
                        If dist1 >= CallAC(1).EvalX(i9) Then
                            start1 = i9 + 1
                            Exit For
                        End If
                    Next

                    For i9 As Integer = start1 To 1 Step -10
                        If dist1 >= CallAC(1).EvalX(i9) Then
                            start1 = i9
                            Exit For
                        End If
                    Next

                End If


                For i1 = start1 To dim1
                    If (Math.Abs(CallAC(1).EvalX(i1)) - dist1) >= 0 Then
                        in1 = i1 - 1
                        Exit For
                    End If
                Next
            End If



            If i1 > dim1 Then
                Get_Strain8 = str1(dim2)
            Else
                strain1 = str1(in1)
                strain2 = str1(in1 + 1)

                dx1 = CallAC(1).EvalX(in1 + 1) - CallAC(1).EvalX(in1)
                dy1 = strain2 - strain1
                Get_Strain8 = strain1 + dy1 / dx1 * (dist1 - CallAC(1).EvalX(in1))

            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Function



    Public Function Get_Strain7(ByVal dist1 As Double, ByRef str1(,) As Double,
                                ByRef CallAC() As ACRClassLib.clsLEAF.LEAFACParms) As Double

        Try

            Dim strain1, strain2 As Single
            Dim dx1, dy1 As Single
            Dim in1, i1 As Integer
            Dim dim1 As Integer
            dim1 = UBound(CallAC(1).EvalX, 1)


            If dist1 = 0 Then
                in1 = 1
            Else
                For i1 = 1 To dim1
                    If (Math.Abs(CallAC(1).EvalX(i1)) - dist1) >= 0 Then
                        in1 = i1 - 1
                        Exit For
                    End If
                Next
            End If



            If i1 > dim1 Then
                Get_Strain7 = str1(1, dim1)
            Else
                strain1 = str1(1, in1)
                strain2 = str1(1, in1 + 1)

                dx1 = CallAC(1).EvalX(in1 + 1) - CallAC(1).EvalX(in1)
                dy1 = strain2 - strain1
                Get_Strain7 = strain1 + dy1 / dx1 * (dist1 - CallAC(1).EvalX(in1))

            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Function


    Public Function Get_Strain(ByVal dist1 As Single, ByRef str1() As Double) As Single

        Try

            Dim in1, in2 As Integer
            Dim strain1, strain2 As Single
            Dim dx1, dy1 As Single
            Dim dimen1 As Integer

            dx1 = 1

            in1 = dist1
            in2 = in1 + 1

            dimen1 = UBound(str1)
            If in2 > dimen1 Then

                Get_Strain = str1(dimen1)

            Else

                strain1 = str1(in1)
                strain2 = str1(in2)
                dy1 = strain2 - strain1

                Get_Strain = strain1 + dy1 * (dist1 - in1)

            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Function







    Public Function AddTwoC(ByVal v1 As Single, ByVal v2 As Single) As Single
        AddTwoC = v1 + v2
    End Function

    Public Sub AddTwoC2(ByRef v1 As Single, ByRef v2 As Single)
        v1 = v1 + v2
    End Sub


    Public Sub s2222()

        Try

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub

    'Public Sub Create_Eval_Points(ByVal CallAC() As ACNClassLib.clsLEAF.LEAFACParms)

    '    'Symmetry = SymmetryType.XYSymmetry
    '    'Symmetry = SymmetryType.YSymmetry
    '    'Symmetry = SymmetryType.NoSymmetry

    '    Call Mesh_All_Cases(CallAC)
    '    If iSelectEval_ACN <> 2 Then '2 is only Mesh
    '        Call Create_Eval_Points_Under_Wheels(CallAC)
    '    End If
    '    Call Print_Evaluation_Points(CallAC)

    '    If True Then Exit Sub

    '    If Symmetry = SymmetryType.XYSymmetry Then 'Calculate ACN Flexible
    '        If bMeshSize Then
    '            Call Set_Eval_Points_XYsym_Mesh(CallAC)
    '        Else
    '            Call Set_Eval_Points_XYsym(CallAC)
    '        End If

    '    ElseIf Symmetry = SymmetryType.YSymmetry Then

    '        If bMeshSize Then
    '            Call Set_Eval_Points_Ysym_Mesh(CallAC)
    '        Else
    '            Call Set_Eval_Points_Ysym(CallAC)
    '        End If

    '        'Call Set_Eval_Points_Ysym(CallAC)
    '    Else
    '        If bMeshSize Then
    '            Call Set_Eval_Points_XYsym_Mesh_Full(CallAC)
    '        Else
    '            Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
    '        End If
    '    End If

    '    'Call Set_Eval_Points_Dim(CallAC)

    '    If iSelectEval_ACN <> 2 Then '2 is only Mesh
    '        Call Create_Eval_Points_Under_Wheels(CallAC)
    '    End If
    '    Call Print_Evaluation_Points(CallAC)

    'End Sub



End Module
