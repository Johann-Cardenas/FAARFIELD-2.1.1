Partial Public Class clsPrintOut  
    Sub tecstress(ByRef nnd As Integer, ByRef nne As Integer, ByRef moe(,) As Integer, ByRef nnt As Integer, ByRef np As Integer, ByRef mee() As Integer, ByVal path As String)


        Const a As Double = 0.5773502691896, b As Double = 1
        Const cc As Double = 1, fx As Double = 100, fy As Double = 100, fz As Double = 100  'scale factors

        Dim c As Double
        c = b / (2 * a)

        Dim xdefomed(3, nnd) As Double 'postion after deformation
        Dim ss(nnd, 7), tt(nne, 7, 8) As Double

        Dim i, j, it, ime, ip, nl As Integer
        Dim fmt As String


        For it = 1 To nnt
            Dim s As String, ns As Integer
            If clsCom.Ntimestep = 1 Then
                For ns = 2 To 20
                    s = path & "model_stress_" & ns & ".dat"
                    If System.IO.File.Exists(s) = True Then
                        System.IO.File.Delete(s)
                    End If
                Next
            End If
            Dim FileNo As Integer, FileName As String
            FileNo = FreeFile()
            FileName = path & "model_stress_" & clsCom.Ntimestep & ".dat"

            FileOpen(FileNo, FileName, OpenMode.Output)

            Dim header As String = "TITLE=""Model_STRESS""" & NL1 &
                                   "VARIABLES=""X"",""Y"",""Z"",""XX"",""YY"",""ZZ"", ""XY"",""YZ"",""ZX"",""MISES"",""TEMPERATURE"""

            PrintLine(FileNo, header)

            For i = 1 To nne
                For j = 1 To 6
                    tt(i, j, 1) = (st(i, j, 1, it) - st(i, j, 7, it)) * c + 0.5 * (st(i, j, 1, it) + st(i, j, 7, it))
                    tt(i, j, 2) = (st(i, j, 2, it) - st(i, j, 8, it)) * c + 0.5 * (st(i, j, 2, it) + st(i, j, 8, it))
                    tt(i, j, 3) = (st(i, j, 3, it) - st(i, j, 5, it)) * c + 0.5 * (st(i, j, 3, it) + st(i, j, 5, it))
                    tt(i, j, 4) = (st(i, j, 4, it) - st(i, j, 6, it)) * c + 0.5 * (st(i, j, 4, it) + st(i, j, 6, it))
                    tt(i, j, 5) = (st(i, j, 5, it) - st(i, j, 3, it)) * c + 0.5 * (st(i, j, 5, it) + st(i, j, 3, it))
                    tt(i, j, 6) = (st(i, j, 6, it) - st(i, j, 4, it)) * c + 0.5 * (st(i, j, 6, it) + st(i, j, 4, it))
                    tt(i, j, 7) = (st(i, j, 7, it) - st(i, j, 1, it)) * c + 0.5 * (st(i, j, 7, it) + st(i, j, 1, it))
                    tt(i, j, 8) = (st(i, j, 8, it) - st(i, j, 2, it)) * c + 0.5 * (st(i, j, 8, it) + st(i, j, 2, it))
                Next j
            Next i

            For i = 1 To nnd
                'ss(i,1:7)=0.
                For j = 1 To 7
                    ss(i, j) = 0
                Next j

                nl = 0
                For j = 1 To nne
                    For k = 1 To 8
                        If (moe(j, k) = i) Then
                            For l = 1 To 6
                                ss(i, l) = ss(i, l) + tt(j, l, k)
                            Next l
                            nl = nl + 1
                        End If
                    Next k

                Next j

                If (nl <> 0) Then
                    For l = 1 To 6
                        ss(i, l) = ss(i, l) / nl
                    Next l
                    ss(i, 7) = Math.Sqrt(Math.Abs(0.5 * ((ss(i, 1) - ss(i, 2)) ^ 2 + (ss(i, 2) - ss(i, 3)) ^ 2 + (ss(i, 3) - ss(i, 1)) ^ 2) + 3 * (ss(i, 4) ^ 2 + ss(i, 5) ^ 2 + ss(i, 6) ^ 2)))
                End If

            Next i



            ime = 0
            For ip = 1 To np
                If (ip > 1) Then
                    ime = ime + mee(ip - 1)
                End If

                PrintLine(FileNo, "ZONE T=""nn." & ip & """")
                PrintLine(FileNo, "N=" & LPad(5, nnd) & ",E=" & LPad(5, mee(ip)) & ",F=FEPOINT,ET=BRICK")

                For i = 1 To nnd
                    xdefomed(1, i) = x(1, i) + cc * fx * snl(i, 1, it)
                    xdefomed(2, i) = x(2, i) + cc * fy * snl(i, 2, it)
                    xdefomed(3, i) = x(3, i) + cc * fz * snl(i, 3, it)

                    fmt = ""
                    For j = 1 To 3
                        fmt = fmt & LPad(20, Format(xdefomed(j, i), "0.0000000000000E+00")) & " "
                    Next j

                    For j = 1 To 7
                        fmt = fmt & LPad(20, Format(ss(i, j), "0.0000000000000E+00")) & " "
                    Next j

                    fmt = fmt & LPad(20, Format(snl(i, 4, it), "0.0000000000000E+00"))

                    PrintLine(FileNo, fmt)
                Next i

                For i = ime + 1 To ime + mee(ip)
                    For j = 1 To 8
                        Print(FileNo, LPad(8, moe(i, j)))
                    Next j
                    PrintLine(FileNo)
                Next i

            Next ip


            FileClose(FileNo)
        Next it

    End Sub
End Class



