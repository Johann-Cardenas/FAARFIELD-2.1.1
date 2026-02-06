Module modPCN_Alpha

    Public Function New06AlphaFactorFromCurve(ByRef NW As Integer, ByRef Coverages As Double) As Double

        ' NW = number of wheels.
        Dim A4 As Double
        Dim A4define() As Double
        Dim NWA4define() As Integer
        Dim Resid, Slope As Double

        ' Fixed points to define alpha versus log(coverages) curves.
        ' log(coverage) values.
        Dim LogCovA0 As Double = 0 : Dim LogCovA1 As Double = 1 : Dim LogCovA4 As Double = 4
        ' Alphas at log(coverages)
        Dim A0 As Double = 0.1 : Dim A1 As Double = 0.38 ' A4 = derived from new set of alpha at 10,000 coverages.

        ' Incremental damage relative to six-wheels the same as old alphas for higher than 6-wheels.
        Dim NA4define As Integer = 8 ' Number of defined values of alpha at 10,000 coverages.
        ' NWA4define() = number of wheels. A4define = alpha for NWA4define wheels.
        ReDim NWA4define(NA4define)
        ReDim A4define(NA4define)
        NWA4define(1) = 1 : NWA4define(2) = 2 : NWA4define(3) = 4
        NWA4define(4) = 6 : NWA4define(5) = 8 : NWA4define(6) = 12
        NWA4define(7) = 18 : NWA4define(8) = 24
        A4define(1) = 0.995 : A4define(2) = 0.9 : A4define(3) = 0.8
        A4define(4) = 0.72 : A4define(5) = 0.69 : A4define(6) = 0.66
        A4define(7) = 0.64 : A4define(8) = 0.63

        ' Boeing Proposals
        If False Then
            NA4define = 13
            ReDim A4define(NA4define)
            ReDim NWA4define(NA4define)
            NWA4define(1) = 1 : NWA4define(2) = 2 : NWA4define(3) = 4
            NWA4define(4) = 6 : NWA4define(5) = 8 : NWA4define(6) = 10
            NWA4define(7) = 12 : NWA4define(8) = 14 : NWA4define(9) = 16
            NWA4define(10) = 18 : NWA4define(11) = 20 : NWA4define(12) = 22 : NWA4define(13) = 24
            A4define(1) = 0.995 : A4define(2) = 0.9 : A4define(3) = 0.8
            A4define(4) = 0.72 : A4define(5) = 0.679 : A4define(6) = 0.65
            A4define(7) = 0.627 : A4define(8) = 0.608 : A4define(9) = 0.591
            A4define(10) = 0.576 : A4define(11) = 0.563 : A4define(12) = 0.551 : A4define(13) = 0.54
        End If

        If NW < NWA4define(1) Then ' Should never happen. Would be an error.
            Return A4define(1)
        End If

        If Coverages < 10 Then ' Lower end of curves is independent of number of wheels.
            If Coverages < 0.0# Then
                Return 0.15 - 0.23 * Log10PCN(-Coverages) 'PPPP
            Else
                Return 0.15 + 0.23 * Log10PCN(Coverages) 'PPPP
            End If
        End If

        ' Linear interpolation for alpha factors at 10,000 coverages undefined.
        For I As Integer = 1 To NA4define - 1
            If NW < NWA4define(I + 1) Then
                Slope = (A4define(I + 1) - A4define(I)) / (NWA4define(I + 1) - NWA4define(I))
                A4 = A4define(I) + Slope * (NW - NWA4define(I))
                Exit For
            Else
                A4 = A4define(NA4define)
            End If
        Next I

        ' Find the coefficients of the defining exponential association curve (see CurveExpert program).
        ' Alpha = A * (B - e^(-C * log10(Coverages)))
        ' Solve for Resid function (below) by Newton's method.
        Dim A As Double = A4 - A0 + 0.1 ' The second log argument is -ve if A < A4 - A0.
        Dim DelA As Double = A / 1000

        Dim ResidM1 As Double = Math.Log((A0 + A - A1) / A) / LogCovA1 - Math.Log((A0 + A - A4) / A) / LogCovA4
        Do
            A += DelA
            Resid = Math.Log((A0 + A - A1) / A) / LogCovA1 - Math.Log((A0 + A - A4) / A) / LogCovA4
            A -= Resid * DelA / (Resid - ResidM1)
            If A < A4 - A0 + 0.001 Then A = A4 - A0 + 0.001 ' Jumped too far because of nonlinearity.
            ResidM1 = Math.Log((A0 + A - A1) / A) / LogCovA1 - Math.Log((A0 + A - A4) / A) / LogCovA4

            System.Windows.Forms.Application.DoEvents()

        Loop Until Math.Abs(ResidM1) < 0.000001

        Dim B As Double = A0 / A + 1
        Dim C As Double = (-Math.Log((A0 + A - A4) / A)) / LogCovA4

        Return A * (B - Math.Exp((-C) * Log10PCN(Coverages))) 'PPPP
        '  New06AlphaFactorFromCurve = A * (B - Exp(-C * Log10Coverages))

    End Function

    Public Function Log10PCN(ByRef X As Double) As Double

        Const Log10BaseE As Double = 2.30258509299405

        Return Math.Log(X) / Log10BaseE

    End Function


End Module
