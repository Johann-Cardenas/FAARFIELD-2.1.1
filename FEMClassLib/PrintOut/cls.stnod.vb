Partial Public Class clsPrintOut  
    Sub stnod(ByRef nnd As Integer, ByRef ie As Integer, ByRef nne As Integer, ByRef ixp(,) As Integer, ByRef ast(,) As Double, ByRef ss(,) As Double, ByRef mn() As Integer)

        Dim c As Double = 0.8660254037845

        Dim j, k, l As Integer
        Dim tt(6, 8) As Double

        For j = 1 To 6
            k = j + 1
            tt(j, 1) = (ast(1, k) - ast(7, k)) * c + 0.5 * (ast(1, k) + ast(7, k))
            tt(j, 2) = (ast(2, k) - ast(8, k)) * c + 0.5 * (ast(2, k) + ast(8, k))
            tt(j, 3) = (ast(3, k) - ast(5, k)) * c + 0.5 * (ast(3, k) + ast(5, k))
            tt(j, 4) = (ast(4, k) - ast(6, k)) * c + 0.5 * (ast(4, k) + ast(6, k))
            tt(j, 5) = (ast(5, k) - ast(3, k)) * c + 0.5 * (ast(5, k) + ast(3, k))
            tt(j, 6) = (ast(6, k) - ast(4, k)) * c + 0.5 * (ast(6, k) + ast(4, k))
            tt(j, 7) = (ast(7, k) - ast(1, k)) * c + 0.5 * (ast(7, k) + ast(1, k))
            tt(j, 8) = (ast(8, k) - ast(2, k)) * c + 0.5 * (ast(8, k) + ast(2, k))
        Next j

        For k = 1 To 8
            For l = 1 To 6
                ss(l, ixp(k + 1, ie)) = ss(l, ixp(k + 1, ie)) + tt(l, k)
            Next l

            ss(7, ixp(k + 1, ie)) = ss(7, ixp(k + 1, ie)) + 1
            mn(ixp(k + 1, ie)) = ixp(1, ie)
        Next k

    End Sub
End Class



