Option Strict On
Option Explicit On
Module modNumerical
    ' All of the subroutines in this module except for GAUSS and LPad are
    ' from Numerical Recipes for Fortran, translated to VB.



    Function bessj0(ByRef x As Double) As Double

        Dim xx, ax, xxx As Double
        Dim y, z, Temp As Double
        Const p1 As Double = 1.0#
        Const p2 As Double = -0.001098628627
        Const p3 As Double = 0.00002734510407
        Const p4 As Double = -0.000002073370639
        Const p5 As Double = 0.0000002093887211
        Const q1 As Double = -0.01562499995
        Const q2 As Double = 0.0001430488765
        Const q3 As Double = -0.000006911147651
        Const q4 As Double = 0.0000007621095161
        Const q5 As Double = -0.0000000934945152
        Const R1 As Double = 57568490574.0#
        Const R2 As Double = -13362590354.0#
        Const r3 As Double = 651619640.7
        Const r4 As Double = -11214424.18
        Const r5 As Double = 77392.33017
        Const r6 As Double = -184.9052456
        Const S1 As Double = 57568490411.0#
        Const S2 As Double = 1029532985.0#
        Const s3 As Double = 9494680.718
        Const s4 As Double = 59272.64853
        Const s5 As Double = 267.8532712
        Const s6 As Double = 1.0#

        '      DATA p1,p2,p3,p4,p5/1.d0,-.1098628627d-2,.2734510407d-4,
        '     *-.2073370639d-5,.2093887211d-6/, q1,q2,q3,q4,q5/-.1562499995d-1,
        '     *.1430488765d-3,-.6911147651d-5,.7621095161d-6,-.934945152d-7/
        '      DATA r1,r2,r3,r4,r5,r6/57568490574.d0,-13362590354.d0,
        '     *651619640.7d0,-11214424.18d0,77392.33017d0,-184.9052456d0/,s1,s2,
        '     *s3,s4,s5,s6/57568490411.d0,1029532985.d0,9494680.718d0,
        '     *59272.64853d0,267.8532712d0,1.d0/

        If (System.Math.Abs(x) < 8.0#) Then
            y = x * x
            Temp = (r4 + y * (r5 + y * r6))
            xxx = (R1 + y * (R2 + y * (r3 + y * Temp)))
            Temp = (s4 + y * (s5 + y * s6))
            xxx = xxx / (S1 + y * (S2 + y * (s3 + y * Temp)))
            bessj0 = xxx
        Else
            ax = System.Math.Abs(x)
            z = 8.0# / ax
            y = z * z
            xx = ax - 0.785398164
            Temp = (p4 + y * p5)
            xxx = System.Math.Cos(xx) * (p1 + y * (p2 + y * (p3 + y * Temp)))
            Temp = (q3 + y * (q4 + y * q5))
            xxx = xxx - z * System.Math.Sin(xx) * (q1 + y * (q2 + y * Temp))
            bessj0 = System.Math.Sqrt(0.636619772 / ax) * xxx
        End If

    End Function

    Function bessj1(ByRef x As Double) As Double

        Dim xx, ax, xxx As Double
        Dim y, z, Temp As Double
        Const p1 As Double = 1.0#
        Const p2 As Double = 0.00183105
        Const p3 As Double = -0.00003516396496
        Const p4 As Double = 0.000002457520174
        Const p5 As Double = -0.000000240337019
        Const q1 As Double = 0.04687499995
        Const q2 As Double = -0.0002002690873
        Const q3 As Double = 0.000008449199096
        Const q4 As Double = -0.00000088228987
        Const q5 As Double = 0.000000105787412
        Const R1 As Double = 72362614232.0#
        Const R2 As Double = -7895059235.0#
        Const r3 As Double = 242396853.1
        Const r4 As Double = -2972611.439
        Const r5 As Double = 15704.4826
        Const r6 As Double = -30.16036606
        Const S1 As Double = 144725228442.0#
        Const S2 As Double = 2300535178.0#
        Const s3 As Double = 18583304.74
        Const s4 As Double = 99447.43394
        Const s5 As Double = 376.9991397
        Const s6 As Double = 1.0#

        '      DATA r1,r2,r3,r4,r5,r6/72362614232.d0,-7895059235.d0,
        '     *242396853.1d0,-2972611.439d0,15704.48260d0,-30.16036606d0/,s1,s2,
        '     *s3,s4,s5,s6/144725228442.d0,2300535178.d0,18583304.74d0,
        '     *99447.43394d0,376.9991397d0,1.d0/
        '      DATA p1,p2,p3,p4,p5/1.d0,.183105d-2,-.3516396496d-4,
        '     *.2457520174d-5,-.240337019d-6/, q1,q2,q3,q4,q5/.04687499995d0,
        '     *-.2002690873d-3,.8449199096d-5,-.88228987d-6,.105787412d-6/

        If (System.Math.Abs(x) < 8.0#) Then
            y = x * x
            Temp = (r4 + y * (r5 + y * r6))
            xxx = x * (R1 + y * (R2 + y * (r3 + y * Temp)))
            Temp = (s4 + y * (s5 + y * s6))
            xxx = xxx / (S1 + y * (S2 + y * (s3 + y * Temp)))
            bessj1 = xxx
        Else
            ax = System.Math.Abs(x)
            z = 8.0# / ax
            y = z * z
            xx = ax - 2.356194491
            Temp = (p3 + y * (p4 + y * p5))
            xxx = System.Math.Cos(xx) * (p1 + y * (p2 + y * Temp))
            Temp = (q3 + y * (q4 + y * q5))
            xxx = xxx - z * System.Math.Sin(xx) * (q1 + y * (q2 + y * Temp))
            bessj1 = System.Math.Sqrt(0.636619772 / ax) * xxx * System.Math.Sign(x)
            '        bessj1 = Sqr(0.636619772 / ax) * (Cos(xx) * (p1 + y * (p2 + y * (p3 + y * (p4 + y * _
            ''           p5)))) - z * Sin(xx) * (q1 + y * (q2 + y * (q3 + y * (q4 + y * q5))))) * sign(1#, x)
        End If

    End Function
    Function bessj1byArg(ByRef x As Double) As Double
        ' = J1(X) / X  used when X may become zero.

        Dim xx, ax, xxx As Double
        Dim y, z, Temp As Double
        Const p1 As Double = 1.0#
        Const p2 As Double = 0.00183105
        Const p3 As Double = -0.00003516396496
        Const p4 As Double = 0.000002457520174
        Const p5 As Double = -0.000000240337019
        Const q1 As Double = 0.04687499995
        Const q2 As Double = -0.0002002690873
        Const q3 As Double = 0.000008449199096
        Const q4 As Double = -0.00000088228987
        Const q5 As Double = 0.000000105787412
        Const R1 As Double = 72362614232.0#
        Const R2 As Double = -7895059235.0#
        Const r3 As Double = 242396853.1
        Const r4 As Double = -2972611.439
        Const r5 As Double = 15704.4826
        Const r6 As Double = -30.16036606
        Const S1 As Double = 144725228442.0#
        Const S2 As Double = 2300535178.0#
        Const s3 As Double = 18583304.74
        Const s4 As Double = 99447.43394
        Const s5 As Double = 376.9991397
        Const s6 As Double = 1.0#

        '      DATA r1,r2,r3,r4,r5,r6/72362614232.d0,-7895059235.d0,
        '     *242396853.1d0,-2972611.439d0,15704.48260d0,-30.16036606d0/,s1,s2,
        '     *s3,s4,s5,s6/144725228442.d0,2300535178.d0,18583304.74d0,
        '     *99447.43394d0,376.9991397d0,1.d0/
        '      DATA p1,p2,p3,p4,p5/1.d0,.183105d-2,-.3516396496d-4,
        '     *.2457520174d-5,-.240337019d-6/, q1,q2,q3,q4,q5/.04687499995d0,
        '     *-.2002690873d-3,.8449199096d-5,-.88228987d-6,.105787412d-6/

        If (System.Math.Abs(x) < 8.0#) Then
            y = x * x
            Temp = (r4 + y * (r5 + y * r6))
            '    xxx = X * (R1 + y * (R2 + y * (r3 + y * Temp))) changed from bessj1(X)
            xxx = (R1 + y * (R2 + y * (r3 + y * Temp)))
            Temp = (s4 + y * (s5 + y * s6))
            xxx = xxx / (S1 + y * (S2 + y * (s3 + y * Temp)))
            bessj1byArg = xxx
        Else
            ax = System.Math.Abs(x)
            z = 8.0# / ax
            y = z * z
            xx = ax - 2.356194491
            Temp = (p3 + y * (p4 + y * p5))
            xxx = System.Math.Cos(xx) * (p1 + y * (p2 + y * Temp))
            Temp = (q3 + y * (q4 + y * q5))
            xxx = xxx - z * System.Math.Sin(xx) * (q1 + y * (q2 + y * Temp))
            Temp = System.Math.Sqrt(0.636619772 / ax) * xxx * System.Math.Sign(x) ' Change
            bessj1byArg = Temp / x ' Added
            '        bessj1 = Sqr(0.636619772 / ax) * (Cos(xx) * (p1 + y * (p2 + y * (p3 + y * (p4 + y * _
            ''           p5)))) - z * Sin(xx) * (q1 + y * (q2 + y * (q3 + y * (q4 + y * q5))))) * sign(1#, x)
        End If

    End Function

    Function gammln(ByRef xx As Double) As Double
        '  Dim xx As Double
        Dim J As Short
        Dim stp, ser, tmp As Double
        Dim x, y As Double
        Dim cof(6) As Double
        '      SAVE cof, stp
        '      DATA cof,stp/
        cof(1) = 76.1800917294715
        cof(2) = -86.5053203294168
        cof(3) = 24.0140982408309
        cof(4) = -1.23173957245015
        cof(5) = 0.00120865097386618
        cof(6) = -0.000005395239384953
        stp = 2.506628274631
        'ik* UPGRADE_WARNING: Couldn't resolve default property of object xx. Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
        x = xx
        y = x
        tmp = x + 5.5
        tmp = (x + 0.5) * System.Math.Log(tmp) - tmp
        ser = 1.00000000019001
        For J = 1 To 6
            y = y + 1.0#
            ser = ser + cof(J) / y
            '11    continue
        Next J
        gammln = tmp + System.Math.Log(stp * ser / x)
        '      Return
    End Function
    Sub gaulag(ByRef x() As Double, ByRef W() As Double, ByRef N As Integer, ByRef alf As Double)

        Dim MAXIT As Short
        '  Dim alf, w(n), x(n) As double
        Dim EPS As Double
        '  PARAMETER (EPS=3.D-14,MAXIT=10)
        '  USES gammln
        Dim Its, I, J As Short
        Dim AI As Double ', gammln As Double
        Dim p3, p1, p2, pp As Double
        Dim z, Z1 As Double
        EPS = 0.00000000000003
        '  EPS = 0.00000003
        MAXIT = 10
        For I = 1 To CShort(N) '200 'N
            '    DoEvents
            If (I = 1) Then
                z = (1.0# + alf) * (3.0# + 0.92 * alf) / (1.0# + 2.4 * N + 1.8 * alf)
            ElseIf (I = 2) Then
                z = z + (15.0# + 6.25 * alf) / (1.0# + 0.9 * alf + 2.5 * N)
            Else
                AI = I - 2
                z = z + ((1.0# + 2.55 * AI) / (1.9 * AI) + 1.26 * AI * alf / (1.0# + 3.5 * AI)) * (z - x(I - 2)) / (1.0# + 0.3 * alf)
            End If
            For Its = 1 To MAXIT
                p1 = 1.0#
                p2 = 0.0#
                For J = 1 To CShort(N)
                    p3 = p2
                    p2 = p1
                    p1 = ((2 * J - 1 + alf - z) * p2 - (J - 1 + alf) * p3) / J
                    '11        continue
                Next J
                pp = (N * p1 - (N + alf) * p2) / z
                Z1 = z
                z = Z1 - p1 / pp
                If (System.Math.Abs(z - Z1) <= EPS) Then GoTo 1
                '12      continue
            Next Its
            System.Diagnostics.Debug.WriteLine("too many iterations in gaulag")
1:          x(I) = z
            W(I) = -System.Math.Exp(gammln(alf + N) - gammln(CDbl(N))) / (pp * N * p2)
            If W(I) < 1.0E-300 And I > 1 Then
                N = I
                Exit For
            End If
            '13    continue
        Next I

    End Sub
    Sub GAUSS(ByRef A(,) As Double, ByRef R() As Double, ByRef NVAR As Integer, ByRef IFail As Integer)
        ' A(NVAR, NVAR) = MATRIX ELEMENTS
        ' R(NVAR) = RHS

        Dim Fact, Pivot, Gash As Double
        Dim FF(,) As Double
        Dim J, I, II, I1 As Integer ', NVAR As Long

        ReDim FF(NVAR, NVAR)

        Pivot = A(1, 1)
        For I = 1 To NVAR - 1
            For II = I + 1 To NVAR
                '    Fact = A(I, II) / Pivot  ' Only for symmetric matrices.
                If A(II, I) <> 0 Then
                    Fact = A(II, I) / Pivot
                    FF(II, I) = Pivot 'Fact
                    R(II) = R(II) - Fact * R(I)
                    For J = I To NVAR
                        A(II, J) = A(II, J) - Fact * A(I, J)
                    Next J
                End If
            Next II
            I1 = I + 1
            Pivot = A(I1, I1)
            If System.Math.Abs(Pivot) < 0.000001 Then
                '    Debug.Print "Pivot too small in subroutine GAUSS."
                '    Debug.Print "Pivot = "; I1; Pivot
                IFail = 1
                '    Stop
            End If
        Next I

        For I = 1 To -NVAR
            For J = 1 To NVAR
                FF(I, J) = A(I, J)
            Next J
        Next I

        For II = 1 To NVAR
            I = NVAR + 1 - II
            Gash = R(I)
            If I <> NVAR Then
                For J = I + 1 To NVAR
                    Gash = Gash - A(I, J) * R(J)
                Next J
            End If
            R(I) = Gash / A(I, I)
        Next II

        For I = 1 To -NVAR
            For J = 1 To NVAR
                A(I, J) = FF(I, J)
            Next J
        Next I

    End Sub

    Public Sub LUsolve(ByRef A(,) As Double, ByRef B() As Double, ByRef K As Integer, ByRef IFail As Integer)

        Dim ASav(,) As Double
        Dim BSav() As Double
        Dim I As Integer
        Dim J As Integer
        Dim INDX() As Integer
        Dim D As Double
        Dim SDP, RNorm As Double

        ReDim ASav(K, K)
        ReDim BSav(K)
        Dim x(K) As Object
        ReDim INDX(K)
        '  ReDim Y(1 To K, 1 To K)

        For I = 1 To K
            For J = 1 To K
                ASav(I, J) = A(I, J)
            Next J
            BSav(I) = B(I)
        Next I

        Call ludcmp(A, K, K, INDX, D, IFail)
        If IFail = -1 Then Exit Sub
        Call lubksb(A, K, K, INDX, B)

        RNorm = 0.0#
        For I = 1 To K
            SDP = -BSav(I)
            For J = 1 To K
                SDP = SDP + ASav(I, J) * B(J) ' Residual for equation I.
            Next J
            If SDP > 1.0E-50 Then
                RNorm = RNorm + SDP * SDP
            End If
        Next I
        RNorm = System.Math.Sqrt(RNorm)

        If RNorm / K > 0.0001 Then
            IFail = -1
        End If

        '  Call mprove(ASav(), A(), K, K, INDX(), BSav(), B())
        '  Call mprove(ASav(), A(), K, K, INDX(), BSav(), B())
        '  Call mprove(ASav(), A(), K, K, INDX(), BSav(), B())
        '  Call mprove(ASav(), A(), K, K, INDX(), BSav(), B())

    End Sub

    Sub ludcmp(ByRef A(,) As Double, ByRef N As Integer, ByRef NP As Integer, ByRef INDX() As Integer, ByRef D As Double, ByRef IFail As Integer)
        '      INTEGER n,np,indx(n),NMAX
        Dim J, I, imax, K As Integer
        Dim dum, aamax, Sum As Double
        Dim vv() As Double
        '      Real D, A(np, np)
        Const NMAX As Integer = 500
        Const Tiny As Double = 1.0E-20
        '      INTEGER i,imax,j,k
        '      Real aamax, dum, Sum, vv(NMAX)
        ReDim vv(NMAX)
        IFail = 0
        D = 1.0#
        '      do 12 i=1,n
        For I = 1 To N
            aamax = 0.0#
            '        do 11 j=1,n
            For J = 1 To N
                If (System.Math.Abs(A(I, J)) > aamax) Then aamax = System.Math.Abs(A(I, J))
            Next J
            '11 '      continue
            If (aamax = 0.0#) Then
                System.Diagnostics.Debug.WriteLine("Singular") 'pause 'singular matrix in ludcmp'
                IFail = -1
            End If
            vv(I) = 1.0# / aamax
        Next I
        '12 '    continue
        '      do 19 j=1,n
        For J = 1 To N
            '        do 14 i=1,j-1
            For I = 1 To J - 1
                Sum = A(I, J)
                '          do 13 k=1,i-1
                For K = 1 To I - 1
                    Sum = Sum - A(I, K) * A(K, J)
                Next K
                '13 '        continue
                A(I, J) = Sum
            Next I
            '14 '      continue
            aamax = 0.0#
            '        do 16 i=j,n
            For I = J To N
                Sum = A(I, J)
                '          do 15 k=1,j-1
                For K = 1 To J - 1
                    Sum = Sum - A(I, K) * A(K, J)
                Next K
                '15 '        continue
                A(I, J) = Sum
                dum = vv(I) * System.Math.Abs(Sum)
                If (dum >= aamax) Then
                    imax = I
                    aamax = dum
                End If
            Next I
            '16 '      continue
            If (J <> imax) Then
                '          do 17 k=1,n
                For K = 1 To N
                    dum = A(imax, K)
                    A(imax, K) = A(J, K)
                    A(J, K) = dum
                Next K
                '17 '        continue
                D = -D
                vv(imax) = vv(J)
            End If
            INDX(J) = imax
            '        If (A(J, J) = 0#) Then A(J, J) = Tiny
            If (A(J, J) < Tiny ^ 2) Then
                A(J, J) = Tiny
                IFail = -1 ' GFH 05/20/03.
                Exit Sub
            End If
            If (J <> N) Then
                dum = 1.0# / A(J, J)
                '          do 18 i=j+1,n
                For I = J + 1 To N
                    A(I, J) = A(I, J) * dum
                Next I
                '18 '        continue
            End If
        Next J
        '19 '    continue
        '      Return
    End Sub

    Sub lubksb(ByRef A(,) As Double, ByRef N As Integer, ByRef NP As Integer, ByRef INDX() As Integer, ByRef B() As Double)
        '      INTEGER n,np,indx(n)
        '      Real A(np, np), B(n)
        '      INTEGER i,ii,j,ll
        Dim J, I, II, LL As Integer
        Dim Sum As Double
        II = 0
        '      do 12 i=1,n
        For I = 1 To N
            LL = INDX(I)
            Sum = B(LL)
            B(LL) = B(I)
            If (II <> 0) Then
                '          do 11 j=ii,i-1
                For J = II To I - 1
                    Sum = Sum - A(I, J) * B(J)
                Next J
                '11        continue
            ElseIf (Sum <> 0.0#) Then
                II = I
            End If
            B(I) = Sum
        Next I
        '12    continue
        '      do 14 i=n,1,-1
        For I = N To 1 Step -1
            Sum = B(I)
            '        do 13 j=i+1,n
            For J = I + 1 To N
                Sum = Sum - A(I, J) * B(J)
            Next J
            '13 '      continue
            B(I) = Sum / A(I, I)
        Next I
        '14    continue
        '      Return
    End Sub

    Sub mprove(ByRef A(,) As Double, ByRef alud(,) As Double, ByRef N As Integer, ByRef NP As Integer, ByRef INDX() As Integer, ByRef B() As Double, ByRef x() As Double)
        '      INTEGER n,np,indx(n),NMAX
        '      Real A(np, np), alud(np, np), B(n), X(n)
        Const NMAX As Integer = 500
        'CU    USES lubksb
        '      INTEGER i,j
        Dim I, J As Integer
        Dim R() As Double
        Dim SDP As Double
        ReDim R(NMAX)
        '      DOUBLE PRECISION sdp
        '      do 12 i=1,n
        For I = 1 To N
            SDP = -B(I)
            '        do 11 j=1,n
            For J = 1 To N
                '          sdp = sdp + dble(A(I, J)) * dble(X(J))
                SDP = SDP + A(I, J) * x(J)
            Next J
            '11      continue
            R(I) = SDP
        Next I
        '12    continue
        Call lubksb(alud, N, NP, INDX, R)
        '      do 13 i=1,n
        For I = 1 To N
            x(I) = x(I) - R(I)
        Next I
        '13    continue
        '      Return
    End Sub

    Sub GaussJ(ByRef A(,) As Double, ByRef N As Integer, ByRef NP As Integer, ByRef B() As Double, ByRef M As Integer, ByRef MP As Integer, ByRef Inverse As Boolean, ByRef IRefine As Boolean, ByRef IFail As Integer)

        Dim Resid(N) As Double 'ikawa 08/29/03

        '      SUBROUTINE gaussj(a,n,np,b,m,mp)
        '      INTEGER m,mp,n,np,NMAX
        Dim NMAX As Integer
        '      Real a(np, np), b(np, mp)
        '      PARAMETER (NMAX = 50)
        '      INTEGER i,icol,irow,j,k,l,ll,indxc(NMAX),indxr(NMAX),ipiv(NMAX)
        Dim J, Icol, I, Irow, K As Integer
        Dim L, LL As Integer
        Dim indxc() As Integer
        Dim indxr() As Integer
        Dim ipiv() As Integer
        '      Real BIG, dum, pivinv
        Dim [Continue] As Boolean
        Dim dum, BIG, pivinv As Double
        Dim BB(,) As Double
        Dim AA(,) As Double
        Dim Cum As Double

        ReDim indxc(N)
        ReDim indxr(N)
        ReDim ipiv(N)
        ReDim BB(N, M)

        On Error GoTo ErrorGaussJ

        If IRefine Then
            ReDim AA(N, N)
            For I = 1 To N
                For J = 1 To N
                    AA(J, I) = A(J, I)
                Next J
            Next I
        End If

        For J = 1 To N
            BB(J, 1) = B(J)
            ipiv(J) = 0
        Next J
        '      do 22 i=1,n
        For I = 1 To N
            BIG = 0.0#
            '        do 13 j=1,n
            For J = 1 To N
                '          if(ipiv(j).ne.1)then
                If (ipiv(J) <> 1) Then
                    '            do 12 k=1,n
                    For K = 1 To N
                        '              if (ipiv(k).eq.0) then
                        If (ipiv(K) = 0) Then
                            '                if (abs(a(j,k)).ge.big)then
                            If (System.Math.Abs(A(J, K)) >= BIG) Then
                                BIG = System.Math.Abs(A(J, K))
                                Irow = J
                                Icol = K
                            End If
                            '              else if (ipiv(k).gt.1) then
                        ElseIf (ipiv(K) > 1) Then
                            '                pause 'singular matrix in gaussj'
                            IFail = -1
                            Exit Sub
                        End If
12:                     [Continue] = True
                    Next K
                End If
13:             [Continue] = True
            Next J
            ipiv(Icol) = ipiv(Icol) + 1
            '        If (irow.ne.icol) Then
            If (Irow <> Icol) Then
                '          do 14 l=1,n
                For L = 1 To N
                    dum = A(Irow, L)
                    A(Irow, L) = A(Icol, L)
                    A(Icol, L) = dum
14:                 [Continue] = True
                Next L
                '          do 15 l=1,m
                For L = 1 To M
                    dum = BB(Irow, L)
                    BB(Irow, L) = BB(Icol, L)
                    BB(Icol, L) = dum
15:                 [Continue] = True
                Next L
            End If
            indxr(I) = Irow
            indxc(I) = Icol
            '        if (a(icol,icol).eq.0.) pause 'singular matrix in gaussj'
            If (A(Icol, Icol) = 0.0#) Then ' pause 'singular matrix in gaussj'
                IFail = -1
                Exit Sub
            End If
            pivinv = 1.0# / A(Icol, Icol)
            A(Icol, Icol) = 1.0#
            '        do 16 l=1,n
            For L = 1 To N
                A(Icol, L) = A(Icol, L) * pivinv
16:             [Continue] = True
            Next L
            '        do 17 l=1,m
            For L = 1 To M
                BB(Icol, L) = BB(Icol, L) * pivinv
17:             [Continue] = True
            Next L
            '        do 21 ll=1,n
            For LL = 1 To N
                '          If (LL.ne.Icol) Then
                If (LL <> Icol) Then
                    dum = A(LL, Icol)
                    A(LL, Icol) = 0.0#
                    '            do 18 l=1,n
                    For L = 1 To N
                        A(LL, L) = A(LL, L) - A(Icol, L) * dum
                        '              If dum <> 0# Then A(LL, L) = A(LL, L) - A(Icol, L) * dum
                        '              If A(Icol, L) <> 0# Then A(LL, L) = A(LL, L) - A(Icol, L) * dum
18:                     [Continue] = True
                    Next L
                    '            do 19 l=1,m
                    For L = 1 To M
                        BB(LL, L) = BB(LL, L) - BB(Icol, L) * dum
19:                     [Continue] = True
                    Next L
                End If
21:             [Continue] = True
            Next LL
22:         [Continue] = True
        Next I

        If Inverse Or IRefine Then
            '        do 24 l=n,1,-1
            For L = N To 1 Step -1
                '          If (indxr(L).ne.indxc(L)) Then
                If (indxr(L) <> indxc(L)) Then
                    '            do 23 k=1,n
                    For K = 1 To N
                        dum = A(K, indxr(L))
                        A(K, indxr(L)) = A(K, indxc(L))
                        A(K, indxc(L)) = dum
23:                     [Continue] = True
                    Next K
                End If
24:             [Continue] = True
            Next L
        End If

        If IRefine Then
            '        Debug.Print "Residuals = ";
            For I = 1 To N
                Cum = -B(I)
                For J = 1 To N
                    Cum = Cum + AA(I, J) * BB(J, 1) ' Residuals from original matrix.
                Next J
                'ik* UPGRADE_WARNING: Couldn't resolve default property of object Resid(I). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                Resid(I) = Cum
                '          Debug.Print LPad(10, Format(Cum, "0.00E+00"));
            Next I
            '        Debug.Print
            For I = 1 To N
                Cum = 0
                For J = 1 To N
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object Resid(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    Cum = Cum + A(I, J) * Resid(J) ' DelB from inverse of A and the residuals.
                Next J
                BB(I, 1) = BB(I, 1) - Cum
            Next I
        End If

        For J = 1 To N
            B(J) = BB(J, 1)
        Next J

        Exit Sub

ErrorGaussJ:

        IFail = -1

        For J = 1 To N
            B(J) = 0.0#
        Next J

    End Sub

    Sub SVDcmp(ByRef A(,) As Double, ByRef M As Integer, ByRef N As Integer, ByRef MP As Integer, ByRef NP As Integer, ByRef W() As Double, ByRef V(,) As Double, ByRef IFail As Integer)
        '      INTEGER m,mp,n,np,NMAX
        '      Real A(MP, NP), V(NP, NP), W(NP)
        '      PARAMETER (NMAX = 500)
        'CU    USES pythag
        '      INTEGER i,its,j,jj,k,l,nm
        Dim J, I, Its, JJ As Integer
        Dim L, K, NM As Integer
        '      REAL anorm,c,f,g,h,s,scale,x,y,z,rv1(NMAX),pythag
        Dim f, anorm, c, g As Double
        Dim s, h, Ascale As Double
        Dim y, x, z As Double
        Dim rv1() As Double
        Dim [Continue] As Boolean
        Dim ITemp As Integer
        Dim DTemp As Double

        ReDim A(MP, NP)
        ReDim V(NP, NP)
        ReDim W(NP)
        ReDim rv1(N)

        g = 0.0#
        Ascale = 0.0#
        anorm = 0.0#
        '      do 25 i=1,n
        For I = 1 To N
            L = I + 1
            rv1(I) = Ascale * g
            g = 0.0#
            s = 0.0#
            Ascale = 0.0#
            '        If (I.le.M) Then
            If (I <= M) Then
                '          do 11 k=i,m
                For K = I To M
                    Ascale = Ascale + System.Math.Abs(A(K, I))
                Next K
11:             [Continue] = True
                '          if(Ascale.ne.0.0)then
                If (Ascale <> 0.0#) Then
                    '            do 12 k=i,m
                    For K = I To M
                        A(K, I) = A(K, I) / Ascale
                        s = s + A(K, I) * A(K, I)
                    Next K
12:                 [Continue] = True
                    f = A(I, I)
                    '            g = -sign(sqrt(s), f) ' ************
                    g = System.Math.Abs(System.Math.Sqrt(s))
                    If f >= 0.0# Then g = -g
                    h = f * g - s
                    A(I, I) = f - g
                    '            do 15 j=l,n
                    For J = L To N
                        s = 0.0#
                        '              do 13 k=i,m
                        For K = I To M
                            s = s + A(K, I) * A(K, J)
                        Next K
13:                     [Continue] = True
                        f = s / h
                        '              do 14 k=i,m
                        For K = I To M
                            A(K, J) = A(K, J) + f * A(K, I)
                        Next K
14:                     [Continue] = True
                    Next J
15:                 [Continue] = True
                    '            do 16 k=i,m
                    For K = I To M
                        A(K, I) = Ascale * A(K, I)
                    Next K
16:                 [Continue] = True
                End If
            End If
            W(I) = Ascale * g
            g = 0.0#
            s = 0.0#
            Ascale = 0.0#
            '        if((i.le.m).and.(i.ne.n))then
            If ((I <= M) And (I <> N)) Then
                '          do 17 k=l,n
                For K = L To N
                    Ascale = Ascale + System.Math.Abs(A(I, K))
                Next K
17:             [Continue] = True
                '          if(Ascale.ne.0.0)then
                If (Ascale <> 0.0#) Then
                    '            do 18 k=l,n
                    For K = L To N
                        A(I, K) = A(I, K) / Ascale
                        s = s + A(I, K) * A(I, K)
                    Next K
18:                 [Continue] = True
                    f = A(I, L)
                    '            g = -sign(sqrt(s), f) ' **************
                    g = System.Math.Abs(System.Math.Sqrt(s))
                    If f >= 0.0# Then g = -g
                    h = f * g - s
                    A(I, L) = f - g
                    '            do 19 k=l,n
                    For K = L To N
                        rv1(K) = A(I, K) / h
                    Next K
19:                 [Continue] = True
                    '            do 23 j=l,m
                    For J = L To M
                        s = 0.0#
                        '              do 21 k=l,n
                        For K = L To N
                            s = s + A(J, K) * A(I, K)
                        Next K
21:                     [Continue] = True
                        '              do 22 k=l,n
                        For K = L To N
                            A(J, K) = A(J, K) + s * rv1(K)
                        Next K
22:                     [Continue] = True
                    Next J
23:                 [Continue] = True
                    '            do 24 k=l,n
                    For K = L To N
                        A(I, K) = Ascale * A(I, K)
                    Next K
24:                 [Continue] = True
                End If
            End If
            '        anorm = Max(anorm, (Abs(W(I)) + Abs(rv1(I)))) ' *****************
            If (anorm < (System.Math.Abs(W(I)) + System.Math.Abs(rv1(I)))) Then
                anorm = (System.Math.Abs(W(I)) + System.Math.Abs(rv1(I)))
            End If
        Next I
25:     [Continue] = True
        '      do 32 i=n,1,-1
        For I = N To 1 Step -1
            '        If (I.lt.N) Then
            If (I < N) Then
                '          if(g.ne.0.0)then
                If (g <> 0.0#) Then
                    '            do 26 j=l,n
                    For J = L To N
                        V(J, I) = (A(I, J) / A(I, L)) / g
                    Next J
26:                 [Continue] = True
                    '            do 29 j=l,n
                    For J = L To N
                        s = 0.0#
                        '              do 27 k=l,n
                        For K = L To N
                            s = s + A(I, K) * V(K, J)
                        Next K
27:                     [Continue] = True
                        '              do 28 k=l,n
                        For K = L To N
                            V(K, J) = V(K, J) + s * V(K, I)
                        Next K
28:                     [Continue] = True
                    Next J
29:                 [Continue] = True
                End If
                '          do 31 j=l,n
                For J = L To N
                    V(I, J) = 0.0#
                    V(J, I) = 0.0#
                Next J
31:             [Continue] = True
            End If
            V(I, I) = 1.0#
            g = rv1(I)
            L = I
        Next I
32:     [Continue] = True
        '      do 39 i=min(m,n),1,-1                  ' ***************
        If M < N Then ITemp = M Else ITemp = N
        For I = ITemp To 1 Step -1
            L = I + 1
            g = W(I)
            '        do 33 j=l,n
            For J = L To N
                A(I, J) = 0.0#
            Next J
33:         [Continue] = True
            '        if(g.ne.0.0)then
            If (g <> 0.0#) Then
                g = 1.0# / g
                '          do 36 j=l,n
                For J = L To N
                    s = 0.0#
                    '            do 34 k=l,m
                    For K = L To M
                        s = s + A(K, I) * A(K, J)
                    Next K
34:                 [Continue] = True
                    f = (s / A(I, I)) * g
                    '            do 35 k=i,m
                    For K = I To M
                        A(K, J) = A(K, J) + f * A(K, I)
                    Next K
35:                 [Continue] = True
                Next J
36:             [Continue] = True
                '          do 37 j=i,m
                For J = I To M
                    A(J, I) = A(J, I) * g
                Next J
37:             [Continue] = True
            Else
                '          do 38 j= i,m
                For J = I To M
                    A(J, I) = 0.0#
                Next J
38:             [Continue] = True
            End If
            A(I, I) = A(I, I) + 1.0#
        Next I
39:     [Continue] = True
        '      do 49 k=n,1,-1
        For K = N To 1 Step -1
            '        do 48 its=1,30
            For Its = 1 To 30
                '          do 41 l=k,1,-1
                For L = K To 1 Step -1
                    NM = L - 1
                    '            if((abs(rv1(l))+anorm).eq.anorm)  goto 2
                    If ((System.Math.Abs(rv1(L)) + anorm) = anorm) Then GoTo 2
                    '            if((abs(w(nm))+anorm).eq.anorm)  goto 1
                    If ((System.Math.Abs(W(NM)) + anorm) = anorm) Then GoTo 1
                Next L
41:             [Continue] = True
1:              c = 0.0#
                s = 1.0#
                '          do 43 i=l,k
                For I = L To K
                    f = s * rv1(I)
                    rv1(I) = c * rv1(I)
                    '            if((abs(f)+anorm).eq.anorm) goto 2
                    If ((System.Math.Abs(f) + anorm) = anorm) Then GoTo 2
                    g = W(I)
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object pythag(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    h = pythag(f, g)
                    W(I) = h
                    h = 1.0# / h
                    c = (g * h)
                    s = -(f * h)
                    '            do 42 j=1,m
                    For J = 1 To M
                        y = A(J, NM)
                        z = A(J, I)
                        A(J, NM) = (y * c) + (z * s)
                        A(J, I) = -(y * s) + (z * c)
                    Next J
42:                 [Continue] = True
                Next I
43:             [Continue] = True
2:              z = W(K)
                '          If (L.eq.K) Then
                If (L = K) Then
                    '            if(z.lt.0.0)then
                    If (z < 0.0#) Then
                        W(K) = -z
                        '              do 44 j=1,n
                        For J = 1 To N
                            V(J, K) = -V(J, K)
                        Next J
44:                     [Continue] = True
                    End If
                    GoTo 3
                End If
                '          if(its.eq.30) pause 'no convergence in svdcmp'
                If (Its = 30) Then ' pause 'no convergence in svdcmp'
                    IFail = -1
                    Exit Sub
                End If
                x = W(L)
                NM = K - 1
                y = W(NM)
                g = rv1(NM)
                h = rv1(K)
                f = ((y - z) * (y + z) + (g - h) * (g + h)) / (2.0# * h * y)
                'ik* UPGRADE_WARNING: Couldn't resolve default property of object pythag(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                g = pythag(f, 1.0#)
                '          f = ((x - z) * (x + z) + h * ((y / (f + sign(g, f))) - h)) / x
                If f >= 0 Then DTemp = System.Math.Abs(g) Else DTemp = -System.Math.Abs(g)
                f = ((x - z) * (x + z) + h * ((y / (f + DTemp)) - h)) / x
                c = 1.0#
                s = 1.0#
                '          do 47 j=l,nm
                For J = L To NM
                    I = J + 1
                    g = rv1(I)
                    y = W(I)
                    h = s * g
                    g = c * g
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object pythag(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    z = pythag(f, h)
                    rv1(J) = z
                    c = f / z
                    s = h / z
                    f = (x * c) + (g * s)
                    g = -(x * s) + (g * c)
                    h = y * s
                    y = y * c
                    '            do 45 jj=1,n
                    For JJ = 1 To N
                        x = V(JJ, J)
                        z = V(JJ, I)
                        V(JJ, J) = (x * c) + (z * s)
                        V(JJ, I) = -(x * s) + (z * c)
                    Next JJ
45:                 [Continue] = True
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object pythag(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    z = pythag(f, h)
                    W(J) = z
                    '            if(z.ne.0.0)then
                    If (z <> 0.0#) Then
                        z = 1.0# / z
                        c = f * z
                        s = h * z
                    End If
                    f = (c * g) + (s * y)
                    x = -(s * g) + (c * y)
                    '            do 46 jj=1,m
                    For JJ = 1 To M
                        y = A(JJ, J)
                        z = A(JJ, I)
                        A(JJ, J) = (y * c) + (z * s)
                        A(JJ, I) = -(y * s) + (z * c)
                    Next JJ
46:                 [Continue] = True
                Next J
47:             [Continue] = True
                rv1(L) = 0.0#
                rv1(K) = f
                W(K) = x
            Next Its
48:         [Continue] = True
3:          [Continue] = True
        Next K
49:     [Continue] = True

    End Sub

    Sub SVBksb(ByRef U(,) As Double, ByRef W() As Double, ByRef V(,) As Double, ByRef M As Integer, ByRef N As Integer, ByRef MP As Integer, ByRef NP As Integer, ByRef B() As Double, ByRef x() As Double)
        '     From Numerical Recipes.
        '      INTEGER m,mp,n,np,NMAX
        '      Real b(mp), U(mp, np), V(np, np), W(np), X(np)
        '      PARAMETER (NMAX = 500)
        '      INTEGER i,j,jj
        Dim J, I, JJ As Integer
        '      Real S, tmp(NMAX)
        Dim s As Double
        Dim tmp() As Double
        Dim [Continue] As Boolean
        ReDim tmp(N)
        '      do 12 j=1,n
        For J = 1 To N
            s = 0.0#
            '        if(w(j).ne.0.)then
            If (W(J) <> 0.0#) Then
                '          do 11 i=1,m
                For I = 1 To M
                    s = s + U(I, J) * B(I)
11:                 [Continue] = True
                Next I
                s = s / W(J)
            End If
            tmp(J) = s
12:         [Continue] = True
        Next J
        '      do 14 j=1,n
        For J = 1 To N
            s = 0.0#
            '        do 13 jj=1,n
            For JJ = 1 To N
                s = s + V(J, JJ) * tmp(JJ)
13:             [Continue] = True
            Next JJ
            x(J) = s
14:         [Continue] = True
        Next J
    End Sub

    Function pythag(ByRef A As Double, ByRef B As Double) As Double
        '     From Numerical Recipes. Used in SVDcmp.
        '      Real a, b, pythag
        '      Real absa, absb
        Dim absa, absb As Double
        absa = System.Math.Abs(A)
        absb = System.Math.Abs(B)
        '      If (absa.gt.absb) Then
        If (absa > absb) Then
            pythag = absa * System.Math.Sqrt(1.0# + (absb / absa) ^ 2)
        Else
            '        if(absb.eq.0.)then
            If (absb = 0.0#) Then
                pythag = 0.0#
            Else
                pythag = absb * System.Math.Sqrt(1.0# + (absa / absb) ^ 2)
            End If
        End If

    End Function

    Public Function LPad(ByRef N As Integer, ByRef SS As String) As String
        ' Adds leading spaces to variant string SS to make it N characters long.
        ' Used to format output to a file. #### characters in a Format function
        ' do not force spaces like QuickBasic.
        ' Typically, SS = Format(XX, "0.00")
        Dim ITemp As Short
        ITemp = CShort(Len(SS))
        If ITemp > N Then N = ITemp ' Length = Len if Len > N
        LPad = Space(N - ITemp) & SS
    End Function
End Module