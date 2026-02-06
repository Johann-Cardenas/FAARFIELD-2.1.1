
Module Global_Renamed

    Public SavePCNOutputtoFile As Integer
    Public ACNOnly As Boolean
    Public WorkingInAircraftWindow As Boolean
    Public ACWNAC, ACWIAStart As Integer
    Public frmGearCaptionStart As String = ""
    'Public AppPath, Units, FilePath, FileName, ExtFilePath, FileFormat As String
    Public AppPath, Units, FilePath, ExtFilePath, FileFormat As String
    Public FirstFileRead As Boolean
    Public ExternalLibraryIndex As Integer
    'Public Ret As MessageBoxResult 'PPPP
    'Public S As String = "" 'PPPP

    'Izydor 11/11/2014 (ENL = Environment.NewLine)
    Public NL1 As String = Environment.NewLine 'PPPP
    'Public NL2 As String = NL1 & NL1 'PPPP
    Public Const PI As Double = 3.14159265359

    Public ComputingFlexible As Boolean
    Public ComputingRigid As Boolean
    Public StopComputation As Boolean

    Public PCNFlexibleOutput, PCNRigidOutput As String ' Not used.
    Public PCNOutputText, ACNBatchOutputText As String
    Public ThicknessOutputText, LifeOutputText As String
    Public MGWOutputText, ExtFileText As String

    Public Const StandardCoverages As Double = 10000
    Public Coverages As Double
    Public FlexibleCoverages As Double
    Public RigidCoverages As Double
    Public AnnualDepartures As Double
    Public Const DefaultAnnualDepartures As Double = 1200

    Public Const StandardRigidCutoff As Double = 5 ' Changed from 3 02/17/12 by GFH.
    Public RigidCutoff As Double ' Radius of relative stiffness cutoff.
    ' Parameters for plotting the radius cutoff for the PCA computations.
    ' Set in Module ACNRigidBAS Sub WriteRigidOutputData().
    Public PlotCutoffY, PlotCutOffX, PlotCutoffRad As Double
    Public Const StandardConcreteStrength As Double = 650 'Izydor Kawa added code. GFH 700 to 650 01/17/07.
    Public ConcreteFlexuralStrength As Double
    Public TireContactArea As Double 'ik03

    Public JobTitle As String = ""
    Public NWheels1 As Integer 'PPPP
    Public Const NSubs As Integer = 4
    Public Const NMaxWheels As Integer = 56
    Public XWheels1(NMaxWheels) As Double 'Double 'ikawa
    Public YWheels1(NMaxWheels) As Double 'Double 'ikawa
    Public XGridMax, XGridOrigin, XGridNPoints As Double
    Public YGridMax, YGridOrigin, YGridNPoints As Double
    Public GrossWeight, TirePressure As Double
    Public PcntOnMainGears, NMainGears As Double
    Public AlphaFactor, AlternateAlpha As Double
    Public WheelRadius1, InputAlpha As Double
    Public InputCBR, InputkValue As Double
    Public EvalThick As Double
    Public CriticalACIndex As Integer
    Public CriticalACTotalEquivCovs() As Double
    Public MaxGrossWeight() As Double
    Public ACNrtn(,) As Double
    Public ACNInputACrtn(,) As Double

    Public GrossWeightRow, TirePressureRow As Integer
    Public PcntOnMainGearsRow, NMainGearsRow As Integer
    Public InputAlphaRow, AlphaFactorRow As Integer
    Public FlexibleCoveragesRow, RigidCoveragesRow As Integer
    Public AnnualDeparturesRow, PtoTCRow As Integer
    'Public FlexibleAnnualDeparturesRow As Integer, RigidAnnualDeparturesRow As Integer
    Public NWheelsRow, RigidCutoffRow As Integer
    Public FlexStrengthOfConcRow As Integer 'Izydor Kawa added code

    Public Const NSubgrades As Integer = 4
    Public ACNFlex(NSubgrades) As Double
    Public ACNRigid(NSubgrades) As Double
    Public ACNFlexCBR(NSubgrades) As Double
    Public ACNRigidk(NSubgrades) As Double
    Public CBRThickness(NSubgrades) As Double
    Public RigidThickness(NSubgrades) As Double
    Public RigidStress(NSubgrades) As Double

    Public ACNFlexibleOutputText, ACNRigidOutputText As String

    Public Const XINCH As Double = 2.54 ' inches to cm
    Public Const XPRES As Double = 145.0377438 ' Mpa to psi
    Public Const XPOUND As Double = 2.2046225 ' kg to lb

    Public NotShowACNGraph, CancelChangeAircraft As Boolean


    'Izydor Kawa added code Begin
    Public Structure UnitsConversion
        Dim Name As String
        Dim Metric As Boolean
        Dim inch As Double
        Dim inchName As String
        Dim inchFormat As String
        Dim squareInch As Double
        Dim squareInchName As String
        Dim squareInchFormat As String
        Dim pounds As Double
        Dim poundsName As String
        Dim poundsFormat As String
        Dim psi As Double
        Dim psiName As String
        Dim psiFormat As String
        Dim psiMPa As Double
        Dim psiMPaName As String
        Dim psiMPaFormat As String
        Dim pci As Double
        Dim pciName As String
        Dim pciFormat As String
        Dim covFormat As String
        Public Shared Function CreateInstance() As UnitsConversion
            Dim result As New UnitsConversion()
            result.Name = String.Empty
            result.inchName = String.Empty
            result.inchFormat = String.Empty
            result.squareInchName = String.Empty
            result.squareInchFormat = String.Empty
            result.poundsName = String.Empty
            result.poundsFormat = String.Empty
            result.psiName = String.Empty
            result.psiFormat = String.Empty
            result.psiMPaName = String.Empty
            result.psiMPaFormat = String.Empty
            result.pciName = String.Empty
            result.pciFormat = String.Empty
            result.covFormat = String.Empty
            Return result
        End Function
    End Structure

    Public Enum SymmetryType
        XYSymmetry = 1
        XSymmetry = 2
        YSymmetry = 3
        NoSymmetry = 4
    End Enum

    Public Symmetry As SymmetryType

    Public Function ALOG10(ByRef X As Double) As Double
        Const Log10BaseE As Double = 2.30258509299405
        Return Math.Log(X) / Log10BaseE
    End Function

    Public Function RPad(ByRef N As Double, ByRef SS As String) As String
        ' Adds traiing spaces to variant string SS to make it N characters long.
        ' Used to format output to a file. #### characters in a Format function
        ' do not force spaces like QuickBasic.
        ' Typically, SS = Format(XX, "0.00")
        Dim ITemp As Integer = SS.Length
        If N - ITemp < 1 Then N = ITemp '+ 1
        Return SS & New String(" "c, CInt(N - ITemp)) 'PPPP
    End Function

    Public Function RPad2(ByRef N As Double, ByRef SS As String) As String
        ' Adds traiing spaces to variant string SS to make it N characters long.
        ' Used to format output to a file. #### characters in a Format function
        ' do not force spaces like QuickBasic.
        ' Typically, SS = Format(XX, "0.00")
        Dim ITemp As Integer = SS.Length
        If ITemp <= N Then
            'N = ITemp '+ 1
            Return SS & New String(" "c, CInt(N - ITemp)) 'PPPP
        Else
            Return SS.Substring(0, CInt(N))
        End If


    End Function



    Public Sub GearCG(ByRef XW() As Double, ByRef YW() As Double, ByRef N As Integer,
                      ByRef Xcg As Double, ByRef Ycg As Double, ByVal Ind As Integer)

        Dim I As Integer

        Dim XRcg(N) As Double, YRcg(N) As Double, IXRcg(N) As Double, IYRcg(N) As Double


        LI = LibIndex(Ind)

        If AC(LI).libGear = "A" Then
            NMainGears = 1
            PcntOnMainGears = 100
        ElseIf AC(LI).libGear = "WFBF" Then
            NMainGears = 1
            PcntOnMainGears = AC(LibIndex(Ind)).libMGpcnt * 100
        Else
            NMainGears = 2
            PcntOnMainGears = AC(LibIndex(Ind)).libMGpcnt * 100
        End If


        ' Added contact area option for thickness mode. GFH 02/10/04.
        If ACN_mode_true Or True Then
            'WheelRadius = GrossWeight * PcntOnMainGears / (100 * NMainGears * NWheels) 'PPPP
            WheelRadius1 = GL(Ind) * PcntOnMainGears / (100 * NMainGears * AC(LI).libNTires) 'PPPP
            'WheelRadius = Math.Sqrt(WheelRadius / TirePressure / PI)
            WheelRadius1 = Math.Sqrt(WheelRadius1 / AC(LI).libCP / PI)
        Else
            WheelRadius1 = Math.Sqrt(TireContactArea / PI)
        End If

        Xcg = 0 : Ycg = 0
        For I = 1 To N
            Xcg += XW(I)
            Ycg += YW(I)
        Next I
        Xcg /= N
        Ycg /= N


        'If libXGridNPoints(LibIndex) <> 0 And libYGridNPoints(LibIndex) <> 0 Then 'PPPP
        If libXGridNPoints(LI) <> 0 And libYGridNPoints(LI) <> 0 Then
            Exit Sub
        End If

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
            Symmetry = SymmetryType.XYSymmetry
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

        Dim GridInterval As Double = WheelRadius1 / 3
        XGridNPoints = CInt((XGridMax - XGridOrigin) / GridInterval)
        YGridNPoints = CInt((YGridMax - YGridOrigin) / GridInterval)

        I = 9 ' Changed from 3 to be compatible with Boeing. GFH 04/17/06.
        If XGridNPoints < I Then XGridNPoints = I
        If YGridNPoints < I Then YGridNPoints = I
        I = 35
        If XGridNPoints > I Then XGridNPoints = I
        If YGridNPoints > I Then YGridNPoints = I

    End Sub

End Module
