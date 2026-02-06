Option Explicit On 
Option Strict On
' Updated from Gordon version LEDFAA 1.3 given by David 08/29/03 +
Public Class clsLEAF
    Public gResponseType As clsLEAF.LEAFoptions
    Public DesignType As String
    Public PCCThick As Double 'ikawa 

    Dim ACname() As String
    Dim Gear() As String
    Dim GearOrientation() As Integer
    Dim NAC As Integer
    Dim NLayers As Integer
    Dim HLayer() As Double
    Dim ZInterface() As Double
    Dim Youngs() As Double
    Dim Poissons() As Double
    Dim InterfaceParm() As Double
    Dim Radius(,,) As Double
    Dim NTires() As Integer
    Dim NTiresMax As Integer
    Dim TirePress(,) As Double
    Dim TireRadius(,) As Double
    Dim TireX(,) As Double
    Dim TireY(,) As Double
    Dim NEvalPoints() As Integer
    Dim EvalX(,) As Double
    Dim EvalY(,) As Double
    Dim NEvalPointsMax As Integer
    Dim GLNGauss As Integer
    Dim GLAlpha() As Double
    Dim GLWeight() As Double
    Dim RunTimeSelected As Integer
    Dim ACDATPath, SS As String
    Dim FileName As String
    Dim InterfaceStiffness() As Double
    Dim LayerCode() As Double
    Dim EvalDepth As Double
    Dim GearLoad() As Double
    Dim WheelLoad(,) As Double
    Dim IterationstoConverge() As Integer
    Dim LEAFSolver, OverrideSolver As Integer

    Dim FindingAllResponses As Boolean

    Dim ConvergenceLimit, ConvergenceLimitDefault As Double
    Dim ErrorReturn As String

    'begin kairat replace tandem
    Dim TireXMax() As Double
    Dim TireYMax() As Double
    Dim TireXMin() As Double
    Dim TireYMin(), offsetMax() As Double
    Public Const NOFF As Integer = 41 ' Number of offsets for CDF calculations.- kairat
    Public Const NNodesLong As Short = 1800 ' Number of nodes along longitudinal direction -kairat 1234567
    Public Const OFFSETINC As Single = 10.0! ' Distance between offsets in inches.
    Public gTandemFnew As Boolean
    'end kairat replace tandem


    ' Type for returning all responses to the client program.
    ' Could be a Property Get function since strictly read only.
    ' All responses are spelled out for ease of use.
    ' Dimensioned as an array (1 To NAC, 1 To NEvalPointsMax)
    Public Structure LEAFAllResponses
        Dim DeflX As Double
        Dim DeflY As Double
        Dim DeflZ As Double
        Dim StrainX As Double
        Dim StrainY As Double
        Dim StrainZ As Double
        Dim StrainXZ As Double
        Dim StrainYZ As Double
        Dim StrainXY As Double
        Dim StressX As Double
        Dim StressY As Double
        Dim StressZ As Double
        Dim StressXY As Double
        Dim StressXZ As Double
        Dim StressYZ As Double
        Dim StrainPrin1 As Double
        Dim StrainPrin2 As Double
        Dim StrainPrin3 As Double
        Dim StressPrin1 As Double
        Dim StressPrin2 As Double
        Dim StressPrin3 As Double
        Dim StressMaxShear As Double
        Dim StressOctNormal As Double
        Dim StressOctShear As Double
    End Structure

    Dim AllResp(,) As clsLEAF.LEAFAllResponses

    ' Type for passing aircraft data to LEA routine.
    ' Dimensioned and set in the client program.
    ' Must be dimensioned as an array for compatibility
    ' with sub calling list.
    Public Structure LEAFACParms ' 1 To N Aircraft
        Dim ACname As String
        Dim GearLoad As Double
        Dim NTires As Integer
        Dim TirePress() As Double ' 1 To NTires
        Dim TireX() As Double ' 1 To NTires
        Dim TireY() As Double ' 1 To NTires
        Dim NEvalPoints As Integer
        Dim EvalX() As Double ' 1 To NEvalPoints
        Dim EvalY() As Double ' 1 To NEvalPoints
        Dim libGear As String 'ikawa
        Dim LibGearOrientation As Integer
    End Structure

    ' Type for passing pavement structure data to LEAF.
    ' Dimensioned and set in the client program.
    ' Only one structure is allowed so the type is not an array.
    Public Structure LEAFStrParms
        Dim NLayers As Integer
        Dim Thick() As Double ' 1 To NLayers
        Dim Modulus() As Double ' 1 To NLayers
        Dim Poisson() As Double ' 1 To NLayers
        Dim InterfaceParm() As Double ' 1 To NLayers
        Dim EvalDepth As Double
        Dim EvalLayer As Double
        Dim lngDummy() As Integer ' Expansion
        Dim dblDummy() As Double ' Expansion
        Dim strDummy() As String ' Expansion
    End Structure

    ' Select the response to be computed and returned.
    ' Specifying a single response reduces run time
    ' considerably compared with requesting all responses.
    Public Enum LEAFoptions
        VerticalStrain = 1
        VerticalDeflection = 2
        HorizontalStress = 3
        AllResponses = 4
    End Enum

    Public Enum LEAFSolvers
        FirstDummySolver = 0 ' Add new solvers between FirstDummy and LastDummy.
        PartInvertSolver = 1
        GaussJordanSolver = 2
        LUSolver = 3
        GaussSolver = 4
        LUBandSolver = 5
        LastDummySolver = 6
    End Enum

    Public Event LEAFStopped()

    Private Sub ComputeResponseCheck(ByRef ResponseType As clsLEAF.LEAFoptions, ByRef NACarg As Integer, ByRef LEAAircraft() As clsLEAF.LEAFACParms, ByRef LEAStructure As clsLEAF.LEAFStrParms, ByRef Response(,) As Double, ByRef AllResps(,) As clsLEAF.LEAFAllResponses)

        ' Check data structures and return error code.

        ErrorReturn = "" ' No errors.

        ' Check ResponseType values. Set to AllResponses if out-of-bounds.
        ' Report error but continue computation.

        ' Check dimensions in elements of LEAAircraft and LEAStructure.

        ' Check aircraft parameters for negative values, etc.

        ' Check pavement parameters for unreasonable values, etc.

    End Sub

    ReadOnly Property LEAFVersion() As String
        Get
            LEAFVersion = "06/11/03"
        End Get
    End Property

    ReadOnly Property GLNumberofPoints() As Integer
        Get
            GLNumberofPoints = GLNGauss
        End Get
    End Property

    'Property Let GLNumberofPoints(lngTemp As Long)
    '  GLNGauss = lngTemp
    '  Call gaulag(GLAlpha(), GLWeight(), GLNGaussRet, 0)
    '  Reset maximum wavelength of the Bessel functions.
    'End Property


    Property LEAFConvergenceLimit() As Double
        Get
            LEAFConvergenceLimit = ConvergenceLimit
        End Get
        Set(ByVal Value As Double)
            If 0.0000001 < Value And Value < 1.0# Then
                ConvergenceLimit = Value
            End If
        End Set
    End Property


    Property LEAFOverrideSolver() As Integer
        Get
            LEAFOverrideSolver = OverrideSolver
        End Get
        Set(ByVal Value As Integer)
            OverrideSolver = Value
        End Set
    End Property

    ReadOnly Property LEAFError() As String
        Get
            LEAFError = ErrorReturn
        End Get
    End Property

    Public Sub ComputeResponse(ByRef ResponseType As clsLEAF.LEAFoptions, ByRef NACarg As Integer, ByRef LEAAircraft() As clsLEAF.LEAFACParms, ByRef LEAStructure As clsLEAF.LEAFStrParms, ByRef Response(,) As Double, ByRef AllResps(,) As clsLEAF.LEAFAllResponses)

        Dim I As Integer 'ik*, J As Integer
        'ik* Dim NTime As Object
        Dim ITire, IAC, IEval As Integer
        'ik* Dim Temp
        Dim ZEval As Double
        Dim EvalLayer As Integer
        '  Dim NEvalPoints() As Long
        Const PI As Double = 3.14159265359
        'ik* Dim TimeSave1, TimeSave2 As Integer
        'ik* Dim StrainWW() As Double
        Dim UnbondedInterfaces As Integer
        Dim DummyTop As Boolean

        Dim NEvalx, NEvaly, IEvaly, IEvalx As Integer ' kairat replaces tandem
        Dim ResponseMax As Double 'kairat replaces tandem

        ErrorReturn = ""
        PCCThick = LEAStructure.Thick(1)
        gResponseType = ResponseType


        If LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 0 Then
            DesignType = "FlexOnRigid"
        ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 1 Then
            DesignType = "NewRigid"
        ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 0 Then
            DesignType = "UnbondOnRigid"
        ElseIf LEAStructure.InterfaceParm(1) > 0 And LEAStructure.InterfaceParm(1) < 1 Then
            DesignType = "PartBondOnRigid"
        ElseIf LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 1 And
               LEAStructure.Modulus(2) = 200000 Then
            DesignType = "FlexOnFlex"
        End If


        Call ComputeResponseCheck(ResponseType, NACarg, LEAAircraft, LEAStructure, Response, AllResps)

        NAC = NACarg

        With LEAStructure

            '   Adding a dummy layer at the top improves the conditioning of the matrix
            '   of coefficients under some of the difficult conditions, like fully unbonded
            '   interfaces. Doesn't seem to hurt in other cases. Don't know why though.

            DummyTop = True
            '    DummyTop = False

            If Not DummyTop Then
                NLayers = .NLayers
            Else
                If .NLayers > 1 And .Thick(1) < 2 Then
                    ErrorReturn = "Fatal Error: Top layer less than 2 inches thick." & vbCrLf
                    RaiseEvent LEAFStopped()
                    Exit Sub
                End If
                NLayers = .NLayers + 1 ' Put a dummy layer on top.
            End If

            ReDim ZInterface(NLayers)
            ReDim HLayer(NLayers)
            ReDim Youngs(NLayers)
            ReDim Poissons(NLayers)
            ReDim InterfaceStiffness(NLayers)
            ReDim LayerCode(NLayers)
            ReDim InterfaceParm(NLayers)

            If Not DummyTop Then
                For I = 1 To NLayers
                    HLayer(I) = .Thick(I)
                    Youngs(I) = .Modulus(I)
                    Poissons(I) = .Poisson(I)
                    InterfaceParm(I) = .InterfaceParm(I)
                Next I
                EvalLayer = CInt(.EvalLayer)
            Else
                HLayer(1) = 1.0# ' Dummy Top.
                Youngs(1) = .Modulus(1)
                Poissons(1) = .Poisson(1)
                InterfaceParm(1) = 1.0#
                For I = 2 To NLayers
                    If I = 2 Then
                        HLayer(I) = .Thick(I - 1) - HLayer(1) ' Subtract Dummy Top.
                    Else
                        HLayer(I) = .Thick(I - 1) ' For Dummy Top.
                    End If
                    Youngs(I) = .Modulus(I - 1) ' For Dummy Top.
                    Poissons(I) = .Poisson(I - 1) ' For Dummy Top.
                    InterfaceParm(I) = .InterfaceParm(I - 1) ' For Dummy Top.
                Next I
                EvalLayer = CInt(.EvalLayer + 1) ' For Dummy Top.
            End If

            HLayer(0) = 0
            '   HLayer(0) is set in the response subroutines for trade-off between
            '   matrix conditioning and convergence performance. See note in subroutines.
            HLayer(NLayers) = HLayer(NLayers - 1)
            EvalDepth = .EvalDepth

        End With

        '  OverrideSolver = LUBandSolver
        '  OverrideSolver = LUSolver
        '  OverrideSolver = PartInvertSolver
        '  OverrideSolver = GaussJordanSolver
        If LEAFSolvers.FirstDummySolver < OverrideSolver And OverrideSolver < LEAFSolvers.LastDummySolver Then
            LEAFSolver = OverrideSolver
        Else
            OverrideSolver = 0 ' In case < FirstDummySolver or > LastDummySolver.
            LEAFSolver = 0 ' Will be set below or in Sub FindConstants.
        End If

        UnbondedInterfaces = 0
        For I = 1 To NLayers - 1
            '    InterfaceParm(I) = InterfaceStiffness(I) * (1 + 0.000001) _
            ''                  / (1 + InterfaceStiffness(I))
            If InterfaceParm(I) <> 1.0# Then
                UnbondedInterfaces = UnbondedInterfaces + 1
            End If
        Next I

        ' GFH 06/11/03.
        ' Selection of solver is now automatic in Sub FindConstants if OverrideSolver = 0.
        '  If UnbondedInterfaces < 2 Then
        '    If OverrideSolver = 0 Then
        '      LEAFSolver = Partinvertsolver
        '      Need Gauss-Jordan on flexible structures with large wheel spacings
        '      such as multiple-gear B-747 and A-380.
        '       LEAFSolver = GaussJordanSolver
        '    End If
        '  Else
        '    If OverrideSolver = 0 Then
        '      LEAFSolver = GaussJordanSolver
        '    End If
        '  End If

        If LEAFSolver = LEAFSolvers.PartInvertSolver Then ' Only when set by OverrideSolver.
            For I = 1 To NLayers - 1
                If InterfaceParm(I) = 0.0# Then
                    InterfaceParm(I) = 0.1
                    If Not DummyTop Then
                        LEAStructure.InterfaceParm(I) = InterfaceParm(I) ' Reset so that the user knows it has changed.
                    Else
                        LEAStructure.InterfaceParm(I - 1) = InterfaceParm(I) ' Reset so that the user knows it has changed.
                    End If
                End If
            Next I
        End If

        ZInterface(0) = 0
        For I = 1 To NLayers - 1
            ZInterface(I) = ZInterface(I - 1) + HLayer(I)
        Next I

        ZEval = EvalDepth

        System.Windows.Forms.Application.DoEvents()
        ReDim ACname(NAC)
        ReDim GearLoad(NAC)
        ReDim NTires(NAC)
        ReDim NEvalPoints(NAC)
        ReDim Gear(NAC)
        ReDim GearOrientation(NAC)

        For IAC = 1 To NAC
            With LEAAircraft(IAC)
                ACname(IAC) = .ACname
                GearLoad(IAC) = .GearLoad
                NTires(IAC) = .NTires
                If NTires(IAC) > NTiresMax Then NTiresMax = NTires(IAC)
                NEvalPoints(IAC) = .NEvalPoints
                If NEvalPoints(IAC) > NEvalPointsMax Then NEvalPointsMax = NEvalPoints(IAC)
                Gear(IAC) = .libGear
                GearOrientation(IAC) = .LibGearOrientation
            End With
        Next IAC

        ReDim TirePress(NAC, NTiresMax)
        ReDim TireRadius(NAC, NTiresMax)
        ReDim WheelLoad(NAC, NTiresMax)
        ReDim TireX(NAC, NTiresMax)
        ReDim TireY(NAC, NTiresMax)
        ReDim EvalX(NAC, NEvalPointsMax)
        ReDim EvalY(NAC, NEvalPointsMax)
        For IAC = 1 To NAC
            With LEAAircraft(IAC)
                For ITire = 1 To NTires(IAC)
                    TirePress(IAC, ITire) = .TirePress(ITire)
                    WheelLoad(IAC, ITire) = GearLoad(IAC) / NTires(IAC)
                    TireRadius(IAC, ITire) = System.Math.Sqrt(WheelLoad(IAC, ITire) / TirePress(IAC, ITire) / PI)
                    TireX(IAC, ITire) = .TireX(ITire)
                    TireY(IAC, ITire) = .TireY(ITire)
                Next ITire
                For IEval = 1 To NEvalPoints(IAC)
                    EvalX(IAC, IEval) = .EvalX(IEval)
                    EvalY(IAC, IEval) = .EvalY(IEval)
                Next IEval
            End With
        Next IAC
        System.Windows.Forms.Application.DoEvents()
        ReDim Radius(NAC, NTiresMax, NEvalPointsMax)
        For IAC = 1 To NAC
            For ITire = 1 To NTires(IAC)
                For IEval = 1 To NEvalPoints(IAC)
                    Radius(IAC, ITire, IEval) = System.Math.Sqrt((EvalX(IAC, IEval) - TireX(IAC, ITire)) ^ 2 + (EvalY(IAC, IEval) - TireY(IAC, ITire)) ^ 2)
                Next IEval
            Next ITire
        Next IAC

        ReDim Response(NAC, NEvalPointsMax)

        If ResponseType = LEAFoptions.VerticalStrain Then

            Call IntegrateZStrain(EvalLayer, ZEval, Response)
            For IAC = 1 To -NAC
                System.Diagnostics.Debug.WriteLine("IAC = " & IAC)
                System.Diagnostics.Debug.WriteLine("IEval  EvalX  EvalY  Strain (Comp -ve)")
                For IEval = 1 To NEvalPoints(IAC)
                    System.Diagnostics.Debug.WriteLine(IEval & " " & EvalX(IAC, IEval) & " " & EvalY(IAC, IEval) & " " & Response(IAC, IEval))
                Next IEval
            Next IAC

        ElseIf ResponseType = LEAFoptions.HorizontalStress Then

            Call IntegrateHorizontalStress(EvalLayer, ZEval, Response)
            For IAC = 1 To -NAC
                System.Diagnostics.Debug.WriteLine("IAC = " & IAC)
                System.Diagnostics.Debug.WriteLine("IEval  EvalX  EvalY  Horiz Stress (Comp -ve)")
                For IEval = 1 To NEvalPoints(IAC)
                    System.Diagnostics.Debug.WriteLine(IEval & " " & EvalX(IAC, IEval) & " " & EvalY(IAC, IEval) & " " & Response(IAC, IEval))
                Next IEval
            Next IAC

        ElseIf ResponseType = LEAFoptions.VerticalDeflection Then

            Call IntegrateZDeflection(EvalLayer, ZEval, Response)

        ElseIf ResponseType = LEAFoptions.AllResponses Then

            ReDim AllResp(NAC, NEvalPointsMax)
            Call FindAllResponses(EvalLayer, ZEval)
            ReDim AllResps(NAC, NEvalPointsMax)
            'ikawa AllResps = VB6.CopyArray(AllResp)
            'AllResps = System.Array.Copy(AllResp, AllResps, lenght)

            Dim i1, i2 As Integer 'ikawa added 09/02/03
            For i1 = 1 To NAC
                For i2 = 1 To NEvalPointsMax
                    AllResps(i1, i2) = AllResp(i1, i2)
                Next
            Next


        End If

        For IEval = -NEvalPoints(1) To 1 Step -1 ' 1 To NEvalPoints(1)
            '    Debug.Print LPad(10, Format(EvalY(NAC, IEval), "0.000000")); ", ";
            System.Diagnostics.Debug.Write(LPad(13, VB6.Format(AllResp(NAC, IEval).StressX, "0.00000E+00")) & ", ")
            System.Diagnostics.Debug.Write(LPad(13, VB6.Format(AllResp(NAC, IEval).StressY, "0.00000E+00")) & ", ")
            System.Diagnostics.Debug.Write(LPad(13, VB6.Format(AllResp(NAC, IEval).StressZ, "0.00000E+00")) & ", ")
            System.Diagnostics.Debug.WriteLine(LPad(13, VB6.Format(AllResp(NAC, IEval).StrainZ, "0.00000E+00")))
        Next IEval

        ' Always reset options. Calling program must reset every time.
        ConvergenceLimit = ConvergenceLimitDefault
        OverrideSolver = 0
        '  Debug.Print "IterationstoConverge = "; IterationstoConverge(1); ConvergenceLimit

        RaiseEvent LEAFStopped()

    End Sub

    Public Sub ComputeResponse2(ByRef ResponseType As clsLEAF.LEAFoptions, ByRef NACarg As Integer, ByRef LEAAircraft() As clsLEAF.LEAFACParms, ByRef LEAStructure As clsLEAF.LEAFStrParms, ByRef Response(,) As Double, ByRef AllResps(,) As clsLEAF.LEAFAllResponses) ' kairat replace tandem

        Dim I As Integer 'ik*, J As Integer
        'ik* Dim NTime As Object
        Dim ITire, IAC, IEval As Integer
        'ik* Dim Temp
        Dim ZEval As Double
        Dim EvalLayer As Integer
        '  Dim NEvalPoints() As Long
        Const PI As Double = 3.14159265359
        'ik* Dim TimeSave1, TimeSave2 As Integer
        'ik* Dim StrainWW() As Double
        Dim UnbondedInterfaces As Integer
        Dim DummyTop As Boolean

        Dim NEvalx, NEvaly, IEvaly, IEvalx As Integer ' kairat replaces tandem
        Dim ResponseMax As Double 'kairat replaces tandem

        ErrorReturn = ""
        PCCThick = LEAStructure.Thick(1)
        gResponseType = ResponseType


        If LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 0 Then
            DesignType = "FlexOnRigid"
        ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 1 Then
            DesignType = "NewRigid"
        ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 0 Then
            DesignType = "UnbondOnRigid"
        ElseIf LEAStructure.InterfaceParm(1) > 0 And LEAStructure.InterfaceParm(1) < 1 Then
            DesignType = "PartBondOnRigid"
        ElseIf LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 1 And
               LEAStructure.Modulus(2) = 200000 Then
            DesignType = "FlexOnFlex"
        End If


        Call ComputeResponseCheck(ResponseType, NACarg, LEAAircraft, LEAStructure, Response, AllResps)

        NAC = NACarg

        With LEAStructure

            '   Adding a dummy layer at the top improves the conditioning of the matrix
            '   of coefficients under some of the difficult conditions, like fully unbonded
            '   interfaces. Doesn't seem to hurt in other cases. Don't know why though.

            DummyTop = True
            '    DummyTop = False

            If Not DummyTop Then
                NLayers = .NLayers
            Else
                If .NLayers > 1 And .Thick(1) < 2 Then
                    ErrorReturn = "Fatal Error: Top layer less than 2 inches thick." & vbCrLf
                    RaiseEvent LEAFStopped()
                    Exit Sub
                End If
                NLayers = .NLayers + 1 ' Put a dummy layer on top.
            End If

            ReDim ZInterface(NLayers)
            ReDim HLayer(NLayers)
            ReDim Youngs(NLayers)
            ReDim Poissons(NLayers)
            ReDim InterfaceStiffness(NLayers)
            ReDim LayerCode(NLayers)
            ReDim InterfaceParm(NLayers)

            If Not DummyTop Then
                For I = 1 To NLayers
                    HLayer(I) = .Thick(I)
                    Youngs(I) = .Modulus(I)
                    Poissons(I) = .Poisson(I)
                    InterfaceParm(I) = .InterfaceParm(I)
                Next I
                EvalLayer = CInt(.EvalLayer)
            Else
                HLayer(1) = 1.0# ' Dummy Top.
                Youngs(1) = .Modulus(1)
                Poissons(1) = .Poisson(1)
                InterfaceParm(1) = 1.0#
                For I = 2 To NLayers
                    If I = 2 Then
                        HLayer(I) = .Thick(I - 1) - HLayer(1) ' Subtract Dummy Top.
                    Else
                        HLayer(I) = .Thick(I - 1) ' For Dummy Top.
                    End If
                    Youngs(I) = .Modulus(I - 1) ' For Dummy Top.
                    Poissons(I) = .Poisson(I - 1) ' For Dummy Top.
                    InterfaceParm(I) = .InterfaceParm(I - 1) ' For Dummy Top.
                Next I
                EvalLayer = CInt(.EvalLayer + 1) ' For Dummy Top.
            End If

            HLayer(0) = 0
            '   HLayer(0) is set in the response subroutines for trade-off between
            '   matrix conditioning and convergence performance. See note in subroutines.
            HLayer(NLayers) = HLayer(NLayers - 1)
            EvalDepth = .EvalDepth

        End With

        '  OverrideSolver = LUBandSolver
        '  OverrideSolver = LUSolver
        '  OverrideSolver = PartInvertSolver
        '  OverrideSolver = GaussJordanSolver
        If LEAFSolvers.FirstDummySolver < OverrideSolver And OverrideSolver < LEAFSolvers.LastDummySolver Then
            LEAFSolver = OverrideSolver
        Else
            OverrideSolver = 0 ' In case < FirstDummySolver or > LastDummySolver.
            LEAFSolver = 0 ' Will be set below or in Sub FindConstants.
        End If

        UnbondedInterfaces = 0
        For I = 1 To NLayers - 1
            '    InterfaceParm(I) = InterfaceStiffness(I) * (1 + 0.000001) _
            ''                  / (1 + InterfaceStiffness(I))
            If InterfaceParm(I) <> 1.0# Then
                UnbondedInterfaces = UnbondedInterfaces + 1
            End If
        Next I

        ' GFH 06/11/03.
        ' Selection of solver is now automatic in Sub FindConstants if OverrideSolver = 0.
        '  If UnbondedInterfaces < 2 Then
        '    If OverrideSolver = 0 Then
        '      LEAFSolver = Partinvertsolver
        '      Need Gauss-Jordan on flexible structures with large wheel spacings
        '      such as multiple-gear B-747 and A-380.
        '       LEAFSolver = GaussJordanSolver
        '    End If
        '  Else
        '    If OverrideSolver = 0 Then
        '      LEAFSolver = GaussJordanSolver
        '    End If
        '  End If

        If LEAFSolver = LEAFSolvers.PartInvertSolver Then ' Only when set by OverrideSolver.
            For I = 1 To NLayers - 1
                If InterfaceParm(I) = 0.0# Then
                    InterfaceParm(I) = 0.1
                    If Not DummyTop Then
                        LEAStructure.InterfaceParm(I) = InterfaceParm(I) ' Reset so that the user knows it has changed.
                    Else
                        LEAStructure.InterfaceParm(I - 1) = InterfaceParm(I) ' Reset so that the user knows it has changed.
                    End If
                End If
            Next I
        End If

        ZInterface(0) = 0
        For I = 1 To NLayers - 1
            ZInterface(I) = ZInterface(I - 1) + HLayer(I)
        Next I

        ZEval = EvalDepth

        System.Windows.Forms.Application.DoEvents()
        ReDim ACname(NAC)
        ReDim GearLoad(NAC)
        ReDim NTires(NAC)
        ReDim NEvalPoints(NAC)


        For IAC = 1 To NAC
            With LEAAircraft(IAC)
                ACname(IAC) = .ACname
                GearLoad(IAC) = .GearLoad
                NTires(IAC) = .NTires
                If NTires(IAC) > NTiresMax Then NTiresMax = NTires(IAC)
                NEvalPoints(IAC) = .NEvalPoints
                If NEvalPoints(IAC) > NEvalPointsMax Then NEvalPointsMax = NEvalPoints(IAC)
            End With
        Next IAC

        ReDim TirePress(NAC, NTiresMax)
        ReDim TireRadius(NAC, NTiresMax)
        ReDim WheelLoad(NAC, NTiresMax)
        ReDim TireX(NAC, NTiresMax)
        ReDim TireY(NAC, NTiresMax)
        ReDim EvalX(NAC, NEvalPointsMax)
        ReDim EvalY(NAC, NEvalPointsMax)
        For IAC = 1 To NAC
            With LEAAircraft(IAC)
                For ITire = 1 To NTires(IAC)
                    TirePress(IAC, ITire) = .TirePress(ITire)
                    WheelLoad(IAC, ITire) = GearLoad(IAC) / NTires(IAC)
                    TireRadius(IAC, ITire) = System.Math.Sqrt(WheelLoad(IAC, ITire) / TirePress(IAC, ITire) / PI)
                    TireX(IAC, ITire) = .TireX(ITire)
                    TireY(IAC, ITire) = .TireY(ITire)
                Next ITire
                For IEval = 1 To NEvalPoints(IAC)

                    EvalX(IAC, IEval) = .EvalX(IEval)
                    EvalY(IAC, IEval) = .EvalY(IEval)
                Next IEval
            End With
        Next IAC
        System.Windows.Forms.Application.DoEvents()
        ReDim Radius(NAC, NTiresMax, NEvalPointsMax)
        For IAC = 1 To NAC
            For ITire = 1 To NTires(IAC)
                For IEval = 1 To NEvalPoints(IAC)
                    Radius(IAC, ITire, IEval) = System.Math.Sqrt((EvalX(IAC, IEval) - TireX(IAC, ITire)) ^ 2 + (EvalY(IAC, IEval) - TireY(IAC, ITire)) ^ 2)
                Next IEval
            Next ITire
        Next IAC

        ReDim Response(NAC, NEvalPointsMax)

        If ResponseType = LEAFoptions.VerticalStrain Then
            'begin kairat replace tandem
            Call IntegrateZStrain2(EvalLayer, ZEval, Response) ' kairat

            ReDim TireYMax(NAC)
            ReDim TireYMin(NAC)
            ReDim offsetMax(NAC)

            For IAC = 1 To NAC
                ResponseMax = 0
                ResponseMax = 10 'ikawa 2018.06.05 Tuesday
                For IEval = 1 To NEvalPoints(IAC)
                    '     Compression is -ve, strain for LEDFAA must be -ve.
                    If Response(IAC, IEval) < ResponseMax Then
                        ResponseMax = (Response(IAC, IEval))
                        offsetMax(IAC) = EvalX(IAC, IEval)
                    End If
                Next IEval
                For ITire = 1 To NTires(IAC)
                    If TireY(IAC, ITire) > TireYMax(IAC) Then TireYMax(IAC) = TireY(IAC, ITire)
                    If TireY(IAC, ITire) < TireYMin(IAC) Then TireYMin(IAC) = TireY(IAC, ITire)
                Next ITire
                TireYMin(IAC) = TireYMin(IAC) - 160
                TireYMax(IAC) = TireYMax(IAC) + 160
            Next IAC

            NEvalPointsMax = NNodesLong
            For IAC = 1 To NAC
                With LEAAircraft(IAC)
                    NEvalPoints(IAC) = NEvalPointsMax
                End With
            Next IAC

            ReDim EvalX(NAC, NEvalPointsMax)
            ReDim EvalY(NAC, NEvalPointsMax)

            For IAC = 1 To NAC
                For IEvaly = 1 To NNodesLong
                    EvalX(IAC, IEvaly) = offsetMax(IAC) ' mesh in transverse direction at each 10 inches - kairat
                    EvalY(IAC, IEvaly) = TireYMin(IAC) + IEvaly * (TireYMax(IAC) - TireYMin(IAC)) / NNodesLong

                    'Dim aa, bb, cc As Double
                    'aa = NNodesLong
                    'bb = EvalX(IAC, IEvaly)
                    'cc = EvalY(IAC, IEvaly)

                    'FileName = "AA000000.txt"
                    'FileOpen(9, FileName, OpenMode.Append)
                    'Print(9, LPad(8, Format(aa, "##0.000")) & "   ")
                    'Print(9, LPad(17, Format(bb, "#,###.000000")))
                    'PrintLine(9, LPad(17, Format(cc, "#,###.000000000")))

                    'FileClose(9)


                Next IEvaly
            Next IAC

            ReDim Radius(NAC, NTiresMax, NEvalPointsMax)
            For IAC = 1 To NAC
                For ITire = 1 To NTires(IAC)
                    For IEval = 1 To NEvalPoints(IAC)
                        Radius(IAC, ITire, IEval) = System.Math.Sqrt((EvalX(IAC, IEval) - TireX(IAC, ITire)) ^ 2 + (EvalY(IAC, IEval) - TireY(IAC, ITire)) ^ 2)
                    Next IEval
                Next ITire
            Next IAC

            ReDim Response(NAC, NEvalPointsMax)

            Call IntegrateZStrain2(EvalLayer, ZEval, Response) ' kairat
            'Call IntegrateZStrainK1(EvalLayer, ZEval, Response) ' kairat replace tandem
            'end kairat replace tandem

            For IAC = 1 To -NAC
                System.Diagnostics.Debug.WriteLine("IAC = " & IAC)
                System.Diagnostics.Debug.WriteLine("IEval  EvalX  EvalY  Strain (Comp -ve)")
                For IEval = 1 To NEvalPoints(IAC)
                    System.Diagnostics.Debug.WriteLine(IEval & " " & EvalX(IAC, IEval) & " " & EvalY(IAC, IEval) & " " & Response(IAC, IEval))
                Next IEval
            Next IAC
            ' other response types not needed in this sub and deleted
        End If

        For IEval = -NEvalPoints(1) To 1 Step -1 ' 1 To NEvalPoints(1)
            '    Debug.Print LPad(10, Format(EvalY(NAC, IEval), "0.000000")); ", ";
            System.Diagnostics.Debug.Write(LPad(13, VB6.Format(AllResp(NAC, IEval).StressX, "0.00000E+00")) & ", ")
            System.Diagnostics.Debug.Write(LPad(13, VB6.Format(AllResp(NAC, IEval).StressY, "0.00000E+00")) & ", ")
            System.Diagnostics.Debug.Write(LPad(13, VB6.Format(AllResp(NAC, IEval).StressZ, "0.00000E+00")) & ", ")
            System.Diagnostics.Debug.WriteLine(LPad(13, VB6.Format(AllResp(NAC, IEval).StrainZ, "0.00000E+00")))
        Next IEval

        ' Always reset options. Calling program must reset every time.
        ConvergenceLimit = ConvergenceLimitDefault
        OverrideSolver = 0
        '  Debug.Print "IterationstoConverge = "; IterationstoConverge(1); ConvergenceLimit

        RaiseEvent LEAFStopped()

    End Sub

    Private Sub GetMaxParms(ByRef NEvalPointsMax As Integer, ByRef NTiresMax As Integer, ByRef RMax As Double)

        Dim ITire, IAC, IEval As Integer
        Dim R2, AMAX As Double

        NEvalPointsMax = 0 : NTiresMax = 0 : RMax = 0 : AMAX = 0
        For IAC = 1 To NAC
            If NEvalPoints(IAC) > NEvalPointsMax Then
                NEvalPointsMax = NEvalPoints(IAC)
            End If
            If NTires(IAC) > NTiresMax Then NTiresMax = NTires(IAC)

            If TireRadius(IAC, 1) > AMAX Then AMAX = TireRadius(IAC, 1)
            For ITire = 1 To NTires(IAC)
                For IEval = 1 To NEvalPoints(IAC)
                    R2 = Radius(IAC, ITire, IEval)
                    If R2 > RMax Then RMax = R2
                Next IEval
            Next ITire
        Next IAC
        If AMAX > RMax Then RMax = AMAX

    End Sub

    Private Function LoadFunction(ByRef GLAlpha As Double, ByRef A As Double) As Double

        'ik* Dim Temp As Double
        If True Then
            '   Rectangular load function.
            LoadFunction = bessj1(GLAlpha * A)
        Else
            '   Parabolic load function.
            LoadFunction = 3 * (2 * bessj1(GLAlpha * A) / (GLAlpha * GLAlpha) - A * bessj0(GLAlpha * A) / GLAlpha) / (A * A)
        End If

    End Function

    Private Sub IntegrateHorizontalStress(ByRef EvalLayer As Integer, ByRef ZEval As Double, ByRef Response(,) As Double)

        ' Computes sigmaX, sigmaY, and tauxy.
        Dim TauXZ(,), TauYZ(,) As Double 'ikawa 08/29/03

        Dim I As Integer
        'ik* Dim LFNo As Short
        Dim Temp As Double
        'ik* Dim s As String
        Dim Factor As Double
        Dim B() As Double
        Dim IEval, IAC, ITire, IG As Integer
        Dim CK, AK, BK, DK As Double
        Dim IConstants As Integer
        Dim ZLayer As Double
        'ik* Dim AlphaZ, Theta As Double
        Dim Poisx2 As Double
        Dim Z1, Z2 As Double
        Dim ZLayer1, ZLayer2 As Double
        Dim A1, R1, R2, A2 As Double
        Dim CosTheta, SinTheta As Double
        Dim CosSq, SinSq As Double
        Dim StressRTIG01, StressRTIG11 As Double
        Dim StressRTIG02, StressRTIG12 As Double
        Dim StressRTIG01xJ1A, StressRTIG11xJ1A As Double
        Dim StressRTIG02xJ1A, StressRTIG12xJ1A As Double
        Dim StressRI, StressTI As Double
        Dim StressR(,,) As Double
        Dim StressT(,,) As Double
        Dim StressX(,) As Double
        Dim StressY(,) As Double
        Dim TauXY(,) As Double
        Static StressHmax() As Double
        Static TauRZmax() As Double
        Dim ResponseforConverge As Double
        Dim J1AlphaA1, J1AlphaA2 As Double
        Dim J0AlphaR1, J0AlphaR2 As Double
        Dim J1AlphaR1, J1AlphaR2 As Double
        Dim Converged(,,) As Boolean
        Dim NEvalsConverged(,) As Integer
        Dim NTiresConverged() As Integer
        Dim ACConverged() As Boolean
        Dim NACConverged As Integer
        '  Dim IterationstoConverge() As Long
        'ik* Dim BadCondition As Integer
        Dim FirstTimeFlag1, FirstTimeFlag2 As Boolean
        Dim GLWeightbyZ As Double ' GL means Gauss-Laguerre, not Gross Load.
        '  Static EvalLayerSave(2), ZEvalSave(2)
        Dim TauRZIG01 As Double 'ik* , TauRZIG11 As Double
        Dim TauRZIG02 As Double 'ik* , TauRZIG12 As Double
        Dim TauRZIG01xJ1A As Double 'ik* , TauRZIG11xJ1A As Double
        Dim TauRZIG02xJ1A As Double 'ik*, TauRZIG12xJ1A As Double
        Dim TauRZI As Double
        Dim TauRZ3(,,) As Double
        'ik* Dim TauRZ(,,) As Double
        'ik* Dim TauRXZ() As Double
        'ik* Dim TauRYZ() As Double
        Dim NEvalPointsMax, NTiresMax As Integer
        Dim OShift(,) As Double
        Dim RMax As Double 'ik*, AMAX As Double

        '  GLNGauss, GLAlpha(), and GLWeight() are set in frmStartup.cmdInitLibs_Click().

        Call GetMaxParms(NEvalPointsMax, NTiresMax, RMax)

        ReDim Response(NAC, NEvalPointsMax)
        ReDim StressR(NAC, NTiresMax, NEvalPointsMax)
        ReDim StressT(NAC, NTiresMax, NEvalPointsMax)
        ReDim StressX(NAC, NEvalPointsMax)
        ReDim StressY(NAC, NEvalPointsMax)
        ReDim TauXY(NAC, NEvalPointsMax)
        ReDim Converged(NAC, NTiresMax, NEvalPointsMax)
        ReDim NEvalsConverged(NAC, NTiresMax)
        ReDim NTiresConverged(NAC)
        ReDim ACConverged(NAC)
        ReDim IterationstoConverge(NAC)
        If FindingAllResponses Then
            ReDim TauRZ3(NAC, NTiresMax, NEvalPointsMax)
            ReDim TauXZ(NAC, NEvalPointsMax)
            ReDim TauYZ(NAC, NEvalPointsMax)
        End If

        '  Debug.Print "GLNGauss = "; GLNGauss; GLAlpha(280)
        I = EvalLayer
        ZLayer = ZEval - ZInterface(I - 1)

        Call SetOShifts(OShift, I, ZLayer, RMax)

        Z1 = OShift(I, 1) - ZLayer
        Z2 = OShift(I, 2) + ZLayer

        ZLayer1 = ZLayer / Z1
        ZLayer2 = ZLayer / Z2
        Poisx2 = Poissons(EvalLayer) * 2

        IConstants = (EvalLayer - 1) * 4
        ReDim StressHmax(NAC)
        ReDim TauRZmax(NAC)
        For IG = 1 To GLNGauss
            System.Windows.Forms.Application.DoEvents()
            ResponseforConverge = 0
            '    If EvalLayer < NLayers Then  ' ***
            Call FindConstants(GLAlpha(IG) / Z1, B, OShift, FirstTimeFlag1, 1)
            AK = B(IConstants + 1)
            CK = B(IConstants + 3)
            GLWeightbyZ = GLWeight(IG) / Z1
            StressRTIG01 = Poisx2 * CK * GLWeightbyZ
            If FindingAllResponses Then
                TauRZIG01 = (AK + CK * (Poisx2 + GLAlpha(IG) * ZLayer1)) * GLWeightbyZ
            End If
            ResponseforConverge = Poisx2 * System.Math.Abs(CK)
            CK = CK * (1 + GLAlpha(IG) * ZLayer1)
            StressRTIG11 = (AK + CK) * GLWeightbyZ
            ResponseforConverge = (ResponseforConverge + System.Math.Abs(AK) + System.Math.Abs(CK)) * GLWeightbyZ
            '    End If
            Call FindConstants(GLAlpha(IG) / Z2, B, OShift, FirstTimeFlag2, 2)
            BK = B(IConstants + 2)
            DK = B(IConstants + 4)
            GLWeightbyZ = GLWeight(IG) / Z2
            StressRTIG02 = Poisx2 * DK * GLWeightbyZ
            If FindingAllResponses Then
                TauRZIG02 = (BK - DK * (Poisx2 - GLAlpha(IG) * ZLayer1)) * GLWeightbyZ
            End If
            ResponseforConverge = ResponseforConverge + Poisx2 * System.Math.Abs(DK) * GLWeightbyZ
            DK = DK * (1 - GLAlpha(IG) * ZLayer2)
            StressRTIG12 = -(BK - DK) * GLWeightbyZ
            ResponseforConverge = ResponseforConverge + (System.Math.Abs(BK) + System.Math.Abs(DK)) * GLWeightbyZ
            For IAC = 1 To NAC
                If Not ACConverged(IAC) Then
                    A2 = TireRadius(IAC, 1) ' Move down 1 loop for varying pressures.
                    A1 = A2 / Z1
                    A2 = A2 / Z2
                    If EvalLayer < NLayers Then
                        J1AlphaA1 = LoadFunction(GLAlpha(IG), A1)
                        '          J1AlphaA1 = bessj1(GLAlpha(IG) * A1)
                        StressRTIG01xJ1A = StressRTIG01 * J1AlphaA1
                        StressRTIG11xJ1A = StressRTIG11 * J1AlphaA1
                        If FindingAllResponses Then TauRZIG01xJ1A = TauRZIG01 * J1AlphaA1
                    End If
                    J1AlphaA2 = LoadFunction(GLAlpha(IG), A2)
                    '        J1AlphaA2 = bessj1(GLAlpha(IG) * A2)
                    StressRTIG02xJ1A = StressRTIG02 * J1AlphaA2
                    StressRTIG12xJ1A = StressRTIG12 * J1AlphaA2
                    If FindingAllResponses Then TauRZIG02xJ1A = TauRZIG02 * J1AlphaA2
                    For ITire = 1 To NTires(IAC)
                        For IEval = 1 To NEvalPoints(IAC)
                            R2 = Radius(IAC, ITire, IEval)
                            R1 = R2 / Z1
                            R2 = R2 / Z2
                            If EvalLayer < NLayers Then
                                J0AlphaR1 = bessj0(GLAlpha(IG) * R1)
                                J1AlphaR1 = bessj1byArg(GLAlpha(IG) * R1)
                            Else
                                J0AlphaR1 = 0
                                J1AlphaR1 = 0
                            End If
                            J0AlphaR2 = bessj0(GLAlpha(IG) * R2)
                            J1AlphaR2 = bessj1byArg(GLAlpha(IG) * R2)

                            StressRI = J0AlphaR1 * StressRTIG01xJ1A + J0AlphaR2 * StressRTIG02xJ1A
                            StressTI = StressRI
                            StressRI = StressRI + (J0AlphaR1 - J1AlphaR1) * StressRTIG11xJ1A + (J0AlphaR2 - J1AlphaR2) * StressRTIG12xJ1A
                            StressTI = StressTI + J1AlphaR1 * StressRTIG11xJ1A + J1AlphaR2 * StressRTIG12xJ1A
                            '           Radial and tangential stresses are principal stresses.
                            StressR(IAC, ITire, IEval) = StressR(IAC, ITire, IEval) + StressRI
                            StressT(IAC, ITire, IEval) = StressT(IAC, ITire, IEval) + StressTI
                            If FindingAllResponses Then
                                TauRZI = J1AlphaR1 * TauRZIG01xJ1A * GLAlpha(IG) * R1 + J1AlphaR2 * TauRZIG02xJ1A * GLAlpha(IG) * R2
                                TauRZ3(IAC, ITire, IEval) = TauRZ3(IAC, ITire, IEval) + TauRZI
                            End If

                            '           Need to converge on the largest stress
                            Temp = System.Math.Abs(StressR(IAC, ITire, IEval))
                            If System.Math.Abs(StressT(IAC, ITire, IEval)) > Temp Then Temp = System.Math.Abs(StressT(IAC, ITire, IEval))
                            If Temp < 0.000000000001 Then Temp = 0.000000000001

                            If System.Math.Abs(ResponseforConverge / Temp) < ConvergenceLimit Then
                                If Not Converged(IAC, ITire, IEval) Then
                                    NEvalsConverged(IAC, ITire) = NEvalsConverged(IAC, ITire) + 1
                                    If NEvalsConverged(IAC, ITire) = NEvalPoints(IAC) Then
                                        NTiresConverged(IAC) = NTiresConverged(IAC) + 1
                                        IterationstoConverge(IAC) = IG
                                    End If
                                    Converged(IAC, ITire, IEval) = True
                                    If NTiresConverged(IAC) = NTires(IAC) Then
                                        ACConverged(IAC) = True
                                        NACConverged = NACConverged + 1
                                    End If
                                End If
                            End If
                        Next IEval
                    Next ITire
                End If
            Next IAC
            If NACConverged = NAC Then Exit For
        Next IG

        For IAC = 1 To NAC
            StressHmax(IAC) = 0
            TauRZmax(IAC) = 0
            Factor = TirePress(IAC, 1) * TireRadius(IAC, 1)
            For IEval = 1 To NEvalPoints(IAC)
                For ITire = 1 To NTires(IAC)
                    R1 = Radius(IAC, ITire, IEval)
                    If System.Math.Abs(R1) < 0.000001 Then
                        '         StressR = StressT and shear stress is zero. It doesn't
                        '         matter what the values are as long as CosSq + SinSq = 1.
                        CosTheta = 1
                        SinTheta = 0
                    Else
                        CosTheta = (EvalX(IAC, IEval) - TireX(IAC, ITire)) / R1
                        SinTheta = (EvalY(IAC, IEval) - TireY(IAC, ITire)) / R1
                    End If
                    CosSq = CosTheta * CosTheta
                    SinSq = SinTheta * SinTheta
                    StressRI = StressR(IAC, ITire, IEval)
                    StressTI = StressT(IAC, ITire, IEval)
                    StressX(IAC, IEval) = StressX(IAC, IEval) + StressRI * CosSq + StressTI * SinSq
                    StressY(IAC, IEval) = StressY(IAC, IEval) + StressRI * SinSq + StressTI * CosSq
                    TauXY(IAC, IEval) = TauXY(IAC, IEval) + StressRI * CosTheta * SinTheta - StressTI * CosTheta * SinTheta
                    If FindingAllResponses Then
                        TauRZI = TauRZ3(IAC, ITire, IEval)
                        'ik* UPGRADE_WARNING: Couldn't resolve default property of object TauXZ(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                        'ik* UPGRADE_WARNING: Couldn't resolve default property of object TauXZ(IAC, IEval). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                        TauXZ(IAC, IEval) = TauXZ(IAC, IEval) + TauRZI * CosTheta
                        'ik* UPGRADE_WARNING: Couldn't resolve default property of object TauYZ(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                        'ik* UPGRADE_WARNING: Couldn't resolve default property of object TauYZ(IAC, IEval). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                        TauYZ(IAC, IEval) = TauYZ(IAC, IEval) + TauRZI * SinTheta
                    End If
                Next ITire
                '      StressX(IAC, IEval) = StressX(IAC, IEval) * Factor
                '      StressY(IAC, IEval) = StressY(IAC, IEval) * Factor
                '      TauXY(IAC, IEval) = TauXY(IAC, IEval) * Factor
                '      TauXZ(IAC, IEval) = TauXZ(IAC, IEval) * Factor
                '      TauYZ(IAC, IEval) = TauYZ(IAC, IEval) * Factor
                StressRI = (StressX(IAC, IEval) + StressY(IAC, IEval)) * Factor * 0.5
                StressTI = (StressX(IAC, IEval) - StressY(IAC, IEval)) * Factor * 0.5
                StressTI = System.Math.Sqrt(StressTI * StressTI + (TauXY(IAC, IEval) * Factor) ^ 2)
                '     Max, Min horizontal stresses = (StressRI + StressTI), (StressRI - StressTI).
                '     Tension is +ve, stress for LEDFAA must be +ve.
                StressRI = StressRI + StressTI ' Max horizontal normal stress.
                StressTI = StressRI - 2 * StressTI ' Min horizontal normal stress.
                If StressRI > StressHmax(IAC) Then
                    StressHmax(IAC) = StressRI ' Always most positive.
                End If
                Response(IAC, IEval) = StressRI
                '      If IAC = 1 And IEval = 1 Then Debug.Print "TauRZ = "; TauRZ(IAC, IEval); StressRI + StressTI
                '      Debug.Print IAC; IEval; "Sigs = "; StressRI + StressTI; (StressRI - StressTI)

                'added to compute the largest stress among all evaluation points by YGC 092613
                If Not IEval = 1 Then
                    Response(IAC, 1) = Math.Max(Response(IAC, 1), Response(IAC, IEval))
                    Response(IAC, IEval) = 0
                End If
                'add ended to compute the largest stress among all evaluation points by YGC 092613


                If FindingAllResponses Then
                    AllResp(IAC, IEval).StressX = StressX(IAC, IEval) * Factor
                    AllResp(IAC, IEval).StressY = StressY(IAC, IEval) * Factor
                    AllResp(IAC, IEval).StressXY = TauXY(IAC, IEval) * Factor
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object TauXZ(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    AllResp(IAC, IEval).StressXZ = TauXZ(IAC, IEval) * Factor
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object TauYZ(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    AllResp(IAC, IEval).StressYZ = TauYZ(IAC, IEval) * Factor
                End If
            Next IEval
            '    Debug.Print "Iterations = "; IAC; IterationstoConverge(IAC)
        Next IAC

        ' Debug.Print "Check Evals = "; EvalLayer; ZEval
        '  For IAC = 1 To NAC
        '    For IEval = 1 To IEvalDepth
        '      SS$ = Left$(ACName$(IAC) & "            ", 12) & "  "
        '      SS$ = SS$ & LPad(10, Format(GL(IAC), "0.0"))
        '      SS$ = SS$ & LPad(3, Format(EvalLayerSave(IEval), "0"))
        '      SS$ = SS$ & LPad(7, Format(ZEvalSave(IEval), "0.00000"))
        '      SS$ = SS$ & LPad(17, Format(Youngs(IEval), ".00000000E+00"))
        '      SS$ = SS$ & LPad(17, Format(StressHmax(IAC, IEval), ".00000000E+00"))
        '      Print #LFNo, SS$
        '      Debug.Print IEval; "  "; SS$
        '    Next IEval
        '  Next IAC

    End Sub

    Private Sub FindConstantsPartInvert2(ByRef ALPHA As Double, ByRef B() As Double, ByRef OShift(,) As Double, ByRef BadCondition As Integer)

        ' Part inversion of full matrix. Much faster than standard solvers, but no pivoting.
        ' The subroutine works correctly with only one layer (Boussinesq half-space).
        ' Make DummyTop = False in Sub ComputeResponse to get a single layer.

        Dim JJ, II, I, J, Jm2 As Integer
        Dim IFail, K, N, KK As Integer
        Dim A(,) As Double
        Dim R As Double
        Dim Exp1, Exp2 As Double
        Dim Exp3, Exp4 As Double
        Dim Fact1, Fact2 As Double
        Dim AlphaZ1, AlphaG As Double
        Dim A11(,) As Double
        Dim ATI1(,) As Double
        Dim A11A12(,) As Double
        Dim AStar22, AStar11, AStar12, AStar21 As Double
        Dim IParm As Double
        Dim ResidKKp1, DTemp, Tiny, ResidKKp2 As Double

        On Error GoTo FindConstantsPartInvertError

        BadCondition = 0

        I = 4 * NLayers
        ReDim A(I, I)
        ReDim A11(I, I)
        ReDim B(I)
        ReDim ATI1(4, 4)
        ReDim A11A12(I, 2)

        Exp1 = System.Math.Exp(-ALPHA * OShift(1, 1))
        Exp2 = System.Math.Exp(-ALPHA * OShift(1, 2))

        ' Surface vertical stress.
        A(1, 1) = Exp1 : A(1, 2) = -Exp2
        A(1, 3) = -(1 - 2 * Poissons(1)) * Exp1
        A(1, 4) = -(1 - 2 * Poissons(1)) * Exp2
        ' Surface shear stress.
        A(2, 1) = Exp1 : A(2, 2) = Exp2
        A(2, 3) = 2 * Poissons(1) * Exp1
        A(2, 4) = -2 * Poissons(1) * Exp2

        For I = 1 To NLayers - 1

            Exp1 = System.Math.Exp(-ALPHA * (-HLayer(I) + OShift(I, 1)))
            Exp2 = System.Math.Exp(-ALPHA * (HLayer(I) + OShift(I, 2)))
            Exp3 = System.Math.Exp(-ALPHA * (OShift(I + 1, 1)))
            Exp4 = System.Math.Exp(-ALPHA * (OShift(I + 1, 2)))
            AlphaZ1 = ALPHA * HLayer(I)

            K = (I - 1) * 4

            '   Fill in the lower layer matrix elements first.
            J = I * 4 - 1
            '   Vertical stress.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = -Exp2
            '    A(J, K + 3) = -(1 - 2 * Poissons(I) - AlphaZ1) * Exp1
            '    A(J, K + 4) = -(1 - 2 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = Exp4
            A(J, K + 7) = (1 - 2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (1 - 2 * Poissons(I + 1)) * Exp4

            J = J + 1
            '   Shear stress.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = Exp2
            '    A(J, K + 3) = (2 * Poissons(I) + AlphaZ1) * Exp1
            '    A(J, K + 4) = -(2 * Poissons(I) - AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = -Exp4
            A(J, K + 7) = -(2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (2 * Poissons(I + 1)) * Exp4

            R = (1 + Poissons(I + 1)) * Youngs(I) / (Youngs(I + 1) * (1 + Poissons(I)))
            J = J + 1
            '   Vertical displacement.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = Exp2
            '    A(J, K + 3) = -(2 - 4 * Poissons(I) - AlphaZ1) * Exp1
            '    A(J, K + 4) = (2 - 4 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 * R : A(J, K + 6) = -Exp4 * R
            A(J, K + 7) = (2 - 4 * Poissons(I + 1)) * Exp3 * R
            A(J, K + 8) = -(2 - 4 * Poissons(I + 1)) * Exp4 * R

            J = J + 1
            '   Radial displacement.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = -Exp2
            '    A(J, K + 3) = (1 + AlphaZ1) * Exp1
            '    A(J, K + 4) = (1 - AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 * R : A(J, K + 6) = Exp4 * R
            A(J, K + 7) = -Exp3 * R
            A(J, K + 8) = -Exp4 * R

            '   Combine shear stress and radial displacement for interface shear.
            '   Small values of InterfaceParm, unbonded, cause numerical problems.
            '   Use Sub FindConstantsFull instead. See Sub ComputeResponse.

            IParm = 0.001 ^ 4
            If InterfaceParm(I) > IParm Then IParm = InterfaceParm(I)

            Fact1 = -ALPHA * (1 - IParm) * Youngs(I) / (1 + Poissons(I))
            Fact2 = IParm
            Jm2 = J - 2 ' Reference shear stress equation.
            '    A(J, K + 1) = A(Jm2, K + 1) * Fact1 - A(J, K + 1) * Fact2
            '    A(J, K + 2) = A(Jm2, K + 2) * Fact1 - A(J, K + 2) * Fact2
            '    A(J, K + 3) = A(Jm2, K + 3) * Fact1 - A(J, K + 3) * Fact2
            '    A(J, K + 4) = A(Jm2, K + 4) * Fact1 - A(J, K + 4) * Fact2
            A(J, K + 5) = -A(J, K + 5) * Fact2
            A(J, K + 6) = -A(J, K + 6) * Fact2
            A(J, K + 7) = -A(J, K + 7) * Fact2
            A(J, K + 8) = -A(J, K + 8) * Fact2

            '   Put the inverse of the upper layer matrix into A().
            '   Multiplying a column / row in a matrix is eqivalent to
            '   dividing the same number row / column in its inverse.
            '   Used below to apply Exp1 and Exp2, i.e. divide rows in inverse.

            '   See above for IParm = InterfaceParm(I).
            AlphaG = -ALPHA * (1 - IParm) * Youngs(I) / (1 + Poissons(I))
            Fact1 = 1 / (4 * IParm * (1 - Poissons(I)))
            Fact2 = Fact1 / Exp2
            Fact1 = Fact1 / Exp1
            A(J - 3, K + 1) = IParm * (1 + AlphaZ1) * Fact1
            A(J - 3, K + 2) = (AlphaG * (1 - 2 * Poissons(I) - AlphaZ1) + IParm * (2 - 4 * Poissons(I) - AlphaZ1)) * Fact1
            A(J - 3, K + 3) = IParm * (2 * Poissons(I) + AlphaZ1) * Fact1
            A(J - 3, K + 4) = -(1 - 2 * Poissons(I) - AlphaZ1) * Fact1
            A(J - 2, K + 1) = -IParm * (1 - AlphaZ1) * Fact2
            A(J - 2, K + 2) = (-AlphaG * (1 - 2 * Poissons(I) + AlphaZ1) + IParm * (2 - 4 * Poissons(I) + AlphaZ1)) * Fact2
            A(J - 2, K + 3) = IParm * (2 * Poissons(I) - AlphaZ1) * Fact2
            A(J - 2, K + 4) = (1 - 2 * Poissons(I) + AlphaZ1) * Fact2
            A(J - 1, K + 1) = -IParm * Fact1
            A(J - 1, K + 2) = (AlphaG + IParm) * Fact1
            A(J - 1, K + 3) = -IParm * Fact1
            A(J - 1, K + 4) = -Fact1
            A(J, K + 1) = -IParm * Fact2
            A(J, K + 2) = (AlphaG - IParm) * Fact2
            A(J, K + 3) = IParm * Fact2
            A(J, K + 4) = -Fact2

            '   Post multiply the inverse of the upper layer matrix
            '   by the lower layer matrix.

            For II = 1 To 4
                N = J - 4 + II
                For JJ = 1 To 4
                    KK = K + JJ + 4
                    ATI1(II, JJ) = A(N, K + 1) * A(J - 3, KK) + A(N, K + 2) * A(J - 2, KK) + A(N, K + 3) * A(J - 1, KK) + A(N, K + 4) * A(J - 0, KK)
                Next JJ
            Next II

            '   Refill A() with the reduced elements.
            For II = 1 To 4
                N = J - 4 + II
                A(N, K + 1) = 0.0# ' Fill out the matrix to be complete for printing.
                A(N, K + 2) = 0.0# ' Not needed for computation.
                A(N, K + 3) = 0.0#
                A(N, K + 4) = 0.0#
                A(N, K + 5) = ATI1(II, 1) ' These are needed for computation.
                A(N, K + 6) = ATI1(II, 2)
                A(N, K + 7) = ATI1(II, 3)
                A(N, K + 8) = ATI1(II, 4)
            Next II
            N = J - 4
            A(N + 1, K + 1) = 1.0# ' Fill out the matrix to be complete for printing.
            A(N + 2, K + 2) = 1.0# ' Not needed for computation.
            A(N + 3, K + 3) = 1.0#
            A(N + 4, K + 4) = 1.0#

        Next I

        ' Reduce from an (NLayers * 4) by (NLayers * 4) system to
        ' an (NLayers * 4 - 2) by (NLayers * 4 - 2) system.
        ' This satisfies the infinite subgrade with A and C zero
        ' for the subgrade layer. Return (NLayers * 4) coefficients
        ' by moving returned coefficients and inserting zeros (see below).
        K = 4 * NLayers - 2
        If NLayers > 1 Then
            For I = K - 3 To K
                A(I, K - 1) = A(I, K)
                A(I, K) = A(I, K + 2)
            Next I
        Else
            '   Special case for one layer. A1 = C1 = 0.
            '   Surface boundary 2x4 matrix set at top of subroutine.
            A(1, 1) = A(1, 2)
            A(1, 2) = A(1, 4)
            A(2, 1) = A(2, 2)
            A(2, 2) = A(2, 4)
        End If

        If 0.1 < ALPHA And ALPHA < 0.101 Then
            '    Debug.Print "Alpha = "; Alpha
            For I = 1 To -K
                For J = 1 To K
                    System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A(I, J), "0.00E+00")))
                Next J
                System.Diagnostics.Debug.WriteLine("")
            Next I
        End If

        ' Working array for triangular matrix. Could save some storage here.
        KK = K - 2
        For I = 1 To KK
            For J = 1 To KK
                A11(I, J) = A(I + 2, J)
            Next J
        Next I

        For I = 1 To -KK
            For J = 1 To KK
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A11(I, J), "0.00E+00")))
            Next J
            System.Diagnostics.Debug.WriteLine("")
        Next I

        If NLayers > 1 Then ' Stop subscript going out of range.
            For I = K - 3 To K
                A11A12(I - 2, 1) = -A(I, K - 1) ' Really A12. Saving storage and a variable.
                A11A12(I - 2, 2) = -A(I, K) ' Really A12
            Next I
        End If

        For I = 1 To -KK
            For J = 1 To 2
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A11A12(I, J), "0.00E+00")))
            Next J
            System.Diagnostics.Debug.WriteLine("")
        Next I

        ' Inner loop limits for back substitution ignore zeroes and only index
        ' the ATR matrix (lower layer pre-multiplied by upper layer inverse).
        ' I = 1 is upper left of A11 matrix.

        '   II       I     Jlower    Jupper   (I-1) mod 4    I - ((I-1) mod 4) + 4
        '                 no zeros
        ' KK - 0     1       5         8           0                  5
        ' KK - 1     2       5         8           1                  5
        ' KK - 2     3       5         8           2                  5
        ' KK - 3     4       5         8           3                  5
        ' KK - 4     5       9        12           0                  9
        ' KK - 5     6       9        12           1                  9
        ' KK - 6     7       9        12           2                  9

        ' Back substitution. See Numerical methods by B. Irons for the prototype.
        For I = KK To 1 Step -1
            If I <= KK - 4 Then ' Don't need to do last matrix.
                JJ = I - ((I - 1) Mod 4) + 4
                A11A12(I, 1) = -A11(I, JJ) * A11A12(JJ, 1) - A11(I, JJ + 1) * A11A12(JJ + 1, 1) - A11(I, JJ + 2) * A11A12(JJ + 2, 1) - A11(I, JJ + 3) * A11A12(JJ + 3, 1)
                A11A12(I, 2) = -A11(I, JJ) * A11A12(JJ, 2) - A11(I, JJ + 1) * A11A12(JJ + 1, 2) - A11(I, JJ + 2) * A11A12(JJ + 2, 2) - A11(I, JJ + 3) * A11A12(JJ + 3, 2)
            End If

        Next I

        For I = 1 To -KK
            For J = 1 To 2
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A11A12(I, J), "0.00E+00")))
            Next J
            System.Diagnostics.Debug.WriteLine("")
        Next I

        ' Form A22 (called AStar).
        If NLayers > 1 Then
            For I = 1 To 4
                AStar11 = AStar11 + A(1, I) * A11A12(I, 1)
                AStar12 = AStar12 + A(1, I) * A11A12(I, 2)
                AStar21 = AStar21 + A(2, I) * A11A12(I, 1)
                AStar22 = AStar22 + A(2, I) * A11A12(I, 2)
            Next I
        Else
            '   Special case for one layer. See above.
            AStar11 = A(1, 1)
            AStar12 = A(1, 2)
            AStar21 = A(2, 1)
            AStar22 = A(2, 2)
        End If

        ' Solve for the last two coefficients.
        '  B(KK + 1) = 1 / (AStar11 - AStar12 * (AStar21 / AStar22))  ' GFH 3/8/02.
        Tiny = 1.0E-24
        Tiny = 0.000000001 ' GFH 04/03/03. Changed.
        Tiny = 1.0E-17              ' ikawa 10/27/03
        DTemp = (AStar11 - AStar12 * (AStar21 / AStar22))
        If DTemp = 0 Then DTemp = Tiny
        If System.Math.Abs(DTemp) <= Tiny Then
            B(KK + 1) = 0
        Else
            B(KK + 1) = 1 / DTemp
        End If

        B(KK + 2) = -B(KK + 1) * AStar21 / AStar22

        ResidKKp1 = AStar11 * B(KK + 1) + AStar12 * B(KK + 2) - 1
        ResidKKp2 = AStar21 * B(KK + 1) + AStar22 * B(KK + 2)

        If System.Math.Sqrt(ResidKKp1 * ResidKKp1 + ResidKKp2 * ResidKKp2) / 2 > 0.0001 Then
            BadCondition = -1
        End If

        ' Solve for the remaining coefficients.
        For I = 1 To KK
            B(I) = A11A12(I, 1) * B(KK + 1) + A11A12(I, 2) * B(KK + 2)
        Next I

        If 0.1 < ALPHA And ALPHA < 0.101 Then
            '    Debug.Print "Alpha = "; Alpha; "IFail = "; IFail
            For I = 1 To -K
                System.Diagnostics.Debug.WriteLine(LPad(10, VB6.Format(B(I), "0.00E+00")))
            Next I
            '  Debug.Print
        End If

        ' Put back the full array of coefficients (Alast and Clast are zero for infinite subgrade).
        B(K + 2) = B(K)
        B(K + 1) = 0
        B(K) = B(K - 1)
        B(K - 1) = 0

        Exit Sub

FindConstantsPartInvertError:

        BadCondition = -1

    End Sub


    Private Sub FindConstantsPartInvert(ByRef ALPHA As Double, ByRef B() As Double, ByRef OShift(,) As Double, ByRef BadCondition As Integer)
        'original

        ' Part inversion of full matrix. Much faster than standard solvers, but no pivoting.
        ' The subroutine works correctly with only one layer (Boussinesq half-space).
        ' Make DummyTop = False in Sub ComputeResponse to get a single layer.

        Dim JJ, II, I, J, Jm2 As Integer
        Dim IFail, K, N, KK As Integer
        Dim A(,) As Double
        Dim R As Double
        Dim Exp1, Exp2 As Double
        Dim Exp3, Exp4 As Double
        Dim Fact1, Fact2 As Double
        Dim AlphaZ1, AlphaG As Double
        Dim A11(,) As Double
        Dim ATI1(,) As Double
        Dim A11A12(,) As Double
        Dim AStar22, AStar11, AStar12, AStar21 As Double
        Dim IParm As Double
        Dim ResidKKp1, DTemp, Tiny, ResidKKp2 As Double

        On Error GoTo FindConstantsPartInvertError

        BadCondition = 0

        I = 4 * NLayers
        ReDim A(I, I)
        ReDim A11(I, I)
        ReDim B(I)
        ReDim ATI1(4, 4)
        ReDim A11A12(I, 2)

        Exp1 = System.Math.Exp(-ALPHA * OShift(1, 1))
        Exp2 = System.Math.Exp(-ALPHA * OShift(1, 2))

        ' Surface vertical stress.
        A(1, 1) = Exp1 : A(1, 2) = -Exp2
        A(1, 3) = -(1 - 2 * Poissons(1)) * Exp1
        A(1, 4) = -(1 - 2 * Poissons(1)) * Exp2
        ' Surface shear stress.
        A(2, 1) = Exp1 : A(2, 2) = Exp2
        A(2, 3) = 2 * Poissons(1) * Exp1
        A(2, 4) = -2 * Poissons(1) * Exp2

        For I = 1 To NLayers - 1

            Exp1 = System.Math.Exp(-ALPHA * (-HLayer(I) + OShift(I, 1)))
            Exp2 = System.Math.Exp(-ALPHA * (HLayer(I) + OShift(I, 2)))
            Exp3 = System.Math.Exp(-ALPHA * (OShift(I + 1, 1)))
            Exp4 = System.Math.Exp(-ALPHA * (OShift(I + 1, 2)))
            AlphaZ1 = ALPHA * HLayer(I)

            K = (I - 1) * 4

            '   Fill in the lower layer matrix elements first.
            J = I * 4 - 1
            '   Vertical stress.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = -Exp2
            '    A(J, K + 3) = -(1 - 2 * Poissons(I) - AlphaZ1) * Exp1
            '    A(J, K + 4) = -(1 - 2 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = Exp4
            A(J, K + 7) = (1 - 2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (1 - 2 * Poissons(I + 1)) * Exp4

            J = J + 1
            '   Shear stress.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = Exp2
            '    A(J, K + 3) = (2 * Poissons(I) + AlphaZ1) * Exp1
            '    A(J, K + 4) = -(2 * Poissons(I) - AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = -Exp4
            A(J, K + 7) = -(2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (2 * Poissons(I + 1)) * Exp4

            R = (1 + Poissons(I + 1)) * Youngs(I) / (Youngs(I + 1) * (1 + Poissons(I)))
            J = J + 1
            '   Vertical displacement.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = Exp2
            '    A(J, K + 3) = -(2 - 4 * Poissons(I) - AlphaZ1) * Exp1
            '    A(J, K + 4) = (2 - 4 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 * R : A(J, K + 6) = -Exp4 * R
            A(J, K + 7) = (2 - 4 * Poissons(I + 1)) * Exp3 * R
            A(J, K + 8) = -(2 - 4 * Poissons(I + 1)) * Exp4 * R

            J = J + 1
            '   Radial displacement.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = -Exp2
            '    A(J, K + 3) = (1 + AlphaZ1) * Exp1
            '    A(J, K + 4) = (1 - AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 * R : A(J, K + 6) = Exp4 * R
            A(J, K + 7) = -Exp3 * R
            A(J, K + 8) = -Exp4 * R

            '   Combine shear stress and radial displacement for interface shear.
            '   Small values of InterfaceParm, unbonded, cause numerical problems.
            '   Use Sub FindConstantsFull instead. See Sub ComputeResponse.

            IParm = 0.001 ^ 4
            If InterfaceParm(I) > IParm Then IParm = InterfaceParm(I)

            Fact1 = -ALPHA * (1 - IParm) * Youngs(I) / (1 + Poissons(I))
            Fact2 = IParm
            Jm2 = J - 2 ' Reference shear stress equation.
            '    A(J, K + 1) = A(Jm2, K + 1) * Fact1 - A(J, K + 1) * Fact2
            '    A(J, K + 2) = A(Jm2, K + 2) * Fact1 - A(J, K + 2) * Fact2
            '    A(J, K + 3) = A(Jm2, K + 3) * Fact1 - A(J, K + 3) * Fact2
            '    A(J, K + 4) = A(Jm2, K + 4) * Fact1 - A(J, K + 4) * Fact2
            A(J, K + 5) = -A(J, K + 5) * Fact2
            A(J, K + 6) = -A(J, K + 6) * Fact2
            A(J, K + 7) = -A(J, K + 7) * Fact2
            A(J, K + 8) = -A(J, K + 8) * Fact2

            '   Put the inverse of the upper layer matrix into A().
            '   Multiplying a column / row in a matrix is eqivalent to
            '   dividing the same number row / column in its inverse.
            '   Used below to apply Exp1 and Exp2, i.e. divide rows in inverse.

            '   See above for IParm = InterfaceParm(I).
            AlphaG = -ALPHA * (1 - IParm) * Youngs(I) / (1 + Poissons(I))
            Fact1 = 1 / (4 * IParm * (1 - Poissons(I)))
            Fact2 = Fact1 / Exp2
            Fact1 = Fact1 / Exp1
            A(J - 3, K + 1) = IParm * (1 + AlphaZ1) * Fact1
            A(J - 3, K + 2) = (AlphaG * (1 - 2 * Poissons(I) - AlphaZ1) + IParm * (2 - 4 * Poissons(I) - AlphaZ1)) * Fact1
            A(J - 3, K + 3) = IParm * (2 * Poissons(I) + AlphaZ1) * Fact1
            A(J - 3, K + 4) = -(1 - 2 * Poissons(I) - AlphaZ1) * Fact1
            A(J - 2, K + 1) = -IParm * (1 - AlphaZ1) * Fact2
            A(J - 2, K + 2) = (-AlphaG * (1 - 2 * Poissons(I) + AlphaZ1) + IParm * (2 - 4 * Poissons(I) + AlphaZ1)) * Fact2
            A(J - 2, K + 3) = IParm * (2 * Poissons(I) - AlphaZ1) * Fact2
            A(J - 2, K + 4) = (1 - 2 * Poissons(I) + AlphaZ1) * Fact2
            A(J - 1, K + 1) = -IParm * Fact1
            A(J - 1, K + 2) = (AlphaG + IParm) * Fact1
            A(J - 1, K + 3) = -IParm * Fact1
            A(J - 1, K + 4) = -Fact1
            A(J, K + 1) = -IParm * Fact2
            A(J, K + 2) = (AlphaG - IParm) * Fact2
            A(J, K + 3) = IParm * Fact2
            A(J, K + 4) = -Fact2

            '   Post multiply the inverse of the upper layer matrix
            '   by the lower layer matrix.

            For II = 1 To 4
                N = J - 4 + II
                For JJ = 1 To 4
                    KK = K + JJ + 4
                    ATI1(II, JJ) = A(N, K + 1) * A(J - 3, KK) + A(N, K + 2) * A(J - 2, KK) + A(N, K + 3) * A(J - 1, KK) + A(N, K + 4) * A(J - 0, KK)
                Next JJ
            Next II

            '   Refill A() with the reduced elements.
            For II = 1 To 4
                N = J - 4 + II
                A(N, K + 1) = 0.0# ' Fill out the matrix to be complete for printing.
                A(N, K + 2) = 0.0# ' Not needed for computation.
                A(N, K + 3) = 0.0#
                A(N, K + 4) = 0.0#
                A(N, K + 5) = ATI1(II, 1) ' These are needed for computation.
                A(N, K + 6) = ATI1(II, 2)
                A(N, K + 7) = ATI1(II, 3)
                A(N, K + 8) = ATI1(II, 4)
            Next II
            N = J - 4
            A(N + 1, K + 1) = 1.0# ' Fill out the matrix to be complete for printing.
            A(N + 2, K + 2) = 1.0# ' Not needed for computation.
            A(N + 3, K + 3) = 1.0#
            A(N + 4, K + 4) = 1.0#

        Next I

        ' Reduce from an (NLayers * 4) by (NLayers * 4) system to
        ' an (NLayers * 4 - 2) by (NLayers * 4 - 2) system.
        ' This satisfies the infinite subgrade with A and C zero
        ' for the subgrade layer. Return (NLayers * 4) coefficients
        ' by moving returned coefficients and inserting zeros (see below).
        K = 4 * NLayers - 2
        If NLayers > 1 Then
            For I = K - 3 To K
                A(I, K - 1) = A(I, K)
                A(I, K) = A(I, K + 2)
            Next I
        Else
            '   Special case for one layer. A1 = C1 = 0.
            '   Surface boundary 2x4 matrix set at top of subroutine.
            A(1, 1) = A(1, 2)
            A(1, 2) = A(1, 4)
            A(2, 1) = A(2, 2)
            A(2, 2) = A(2, 4)
        End If

        If 0.1 < ALPHA And ALPHA < 0.101 Then
            '    Debug.Print "Alpha = "; Alpha
            For I = 1 To -K
                For J = 1 To K
                    System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A(I, J), "0.00E+00")))
                Next J
                System.Diagnostics.Debug.WriteLine("")
            Next I
        End If

        ' Working array for triangular matrix. Could save some storage here.
        KK = K - 2
        For I = 1 To KK
            For J = 1 To KK
                A11(I, J) = A(I + 2, J)
            Next J
        Next I

        For I = 1 To -KK
            For J = 1 To KK
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A11(I, J), "0.00E+00")))
            Next J
            System.Diagnostics.Debug.WriteLine("")
        Next I

        If NLayers > 1 Then ' Stop subscript going out of range.
            For I = K - 3 To K
                A11A12(I - 2, 1) = -A(I, K - 1) ' Really A12. Saving storage and a variable.
                A11A12(I - 2, 2) = -A(I, K) ' Really A12
            Next I
        End If

        For I = 1 To -KK
            For J = 1 To 2
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A11A12(I, J), "0.00E+00")))
            Next J
            System.Diagnostics.Debug.WriteLine("")
        Next I

        ' Inner loop limits for back substitution ignore zeroes and only index
        ' the ATR matrix (lower layer pre-multiplied by upper layer inverse).
        ' I = 1 is upper left of A11 matrix.

        '   II       I     Jlower    Jupper   (I-1) mod 4    I - ((I-1) mod 4) + 4
        '                 no zeros
        ' KK - 0     1       5         8           0                  5
        ' KK - 1     2       5         8           1                  5
        ' KK - 2     3       5         8           2                  5
        ' KK - 3     4       5         8           3                  5
        ' KK - 4     5       9        12           0                  9
        ' KK - 5     6       9        12           1                  9
        ' KK - 6     7       9        12           2                  9

        ' Back substitution. See Numerical methods by B. Irons for the prototype.
        For I = KK To 1 Step -1
            If I <= KK - 4 Then ' Don't need to do last matrix.
                JJ = I - ((I - 1) Mod 4) + 4
                A11A12(I, 1) = -A11(I, JJ) * A11A12(JJ, 1) - A11(I, JJ + 1) * A11A12(JJ + 1, 1) - A11(I, JJ + 2) * A11A12(JJ + 2, 1) - A11(I, JJ + 3) * A11A12(JJ + 3, 1)
                A11A12(I, 2) = -A11(I, JJ) * A11A12(JJ, 2) - A11(I, JJ + 1) * A11A12(JJ + 1, 2) - A11(I, JJ + 2) * A11A12(JJ + 2, 2) - A11(I, JJ + 3) * A11A12(JJ + 3, 2)
            End If

        Next I

        For I = 1 To -KK
            For J = 1 To 2
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A11A12(I, J), "0.00E+00")))
            Next J
            System.Diagnostics.Debug.WriteLine("")
        Next I

        ' Form A22 (called AStar).
        If NLayers > 1 Then
            For I = 1 To 4
                AStar11 = AStar11 + A(1, I) * A11A12(I, 1)
                AStar12 = AStar12 + A(1, I) * A11A12(I, 2)
                AStar21 = AStar21 + A(2, I) * A11A12(I, 1)
                AStar22 = AStar22 + A(2, I) * A11A12(I, 2)
            Next I
        Else
            '   Special case for one layer. See above.
            AStar11 = A(1, 1)
            AStar12 = A(1, 2)
            AStar21 = A(2, 1)
            AStar22 = A(2, 2)
        End If

        ' Solve for the last two coefficients.
        '  B(KK + 1) = 1 / (AStar11 - AStar12 * (AStar21 / AStar22))  ' GFH 3/8/02.
        Tiny = 1.0E-24
        Tiny = 0.000000001 ' GFH 04/03/03. Changed.
        Tiny = 1.0E-17              ' ikawa 10/27/03
        DTemp = (AStar11 - AStar12 * (AStar21 / AStar22))
        If DTemp = 0 Then DTemp = Tiny
        If System.Math.Abs(DTemp) <= Tiny Then
            B(KK + 1) = 0
        Else
            B(KK + 1) = 1 / DTemp
        End If

        B(KK + 2) = -B(KK + 1) * AStar21 / AStar22

        ResidKKp1 = AStar11 * B(KK + 1) + AStar12 * B(KK + 2) - 1
        ResidKKp2 = AStar21 * B(KK + 1) + AStar22 * B(KK + 2)

        If System.Math.Sqrt(ResidKKp1 * ResidKKp1 + ResidKKp2 * ResidKKp2) / 2 > 0.0001 Then
            BadCondition = -1
        End If

        'ikawa77 Jan06
        If DesignType = "NewRigid" Or DesignType = "UnbondOnRigid" _
        Or DesignType = "PartBondOnRigid" And PCCThick < 18 Then
            BadCondition = -1 'ikawa 11/17/03
        End If

        ' Solve for the remaining coefficients.
        For I = 1 To KK
            B(I) = A11A12(I, 1) * B(KK + 1) + A11A12(I, 2) * B(KK + 2)
        Next I

        If 0.1 < ALPHA And ALPHA < 0.101 Then
            '    Debug.Print "Alpha = "; Alpha; "IFail = "; IFail
            For I = 1 To -K
                System.Diagnostics.Debug.WriteLine(LPad(10, VB6.Format(B(I), "0.00E+00")))
            Next I
            '  Debug.Print
        End If

        ' Put back the full array of coefficients (Alast and Clast are zero for infinite subgrade).
        B(K + 2) = B(K)
        B(K + 1) = 0
        B(K) = B(K - 1)
        B(K - 1) = 0

        Exit Sub

FindConstantsPartInvertError:

        BadCondition = -1

    End Sub


    Private Sub FindConstantsFull(ByRef ALPHA As Double, ByRef B() As Double, ByRef OShift(,) As Double, ByRef BadCondition As Integer)

        ' Version with standard matrix solvers. See Irons and Numerical Recipes.
        ' The subroutine works correctly with only one layer (Boussinesq half-space).
        ' Make DummyTop = False in Sub ComputeResponse to get a single layer.

        Dim J, I As Integer 'ik* , Jm2 As Integer
        Dim K, IFail As Integer
        Dim AlphaG As Double
        Dim A(,) As Double
        Dim R As Double 'ik* , Factor As Double
        Dim Exp1, Exp2 As Double
        Dim Exp3, Exp4 As Double
        Dim AlphaZ1 As Double
        Dim R1, R2 As Double
        Dim Inverse, IRefine As Boolean ' For GaussJ.
        Dim ITemp1, ITemp2 As Integer

        ' Variables for LAPack band LU solver SGBSVX.
        'ik* Dim EQUED As String
        Dim ipiv() As Integer
        Dim KL, KU As Integer
        'ik* Dim Info As Integer
        Dim LUIWork() As Integer
        Dim LDB, LDAB, LDAFB, NRHS As Integer
        'ik* Dim RCOND As Double
        Dim AB(,) As Double
        Dim AFB(,) As Double
        Dim LUR() As Double
        Dim LUC() As Double
        Dim LUX(,) As Double
        Dim FERR() As Double
        Dim BERR() As Double
        Dim LUWork() As Double

        BadCondition = 0

        I = 4 * NLayers
        ReDim A(I, I)
        ReDim B(I)

        Exp1 = System.Math.Exp(-ALPHA * OShift(1, 1))
        Exp2 = System.Math.Exp(-ALPHA * OShift(1, 2))

        ' Surface vertical stress.
        A(1, 1) = Exp1 : A(1, 2) = -Exp2
        A(1, 3) = -(1 - 2 * Poissons(1)) * Exp1
        A(1, 4) = -(1 - 2 * Poissons(1)) * Exp2
        ' Surface shear stress.
        A(2, 1) = Exp1 : A(2, 2) = Exp2
        A(2, 3) = 2 * Poissons(1) * Exp1
        A(2, 4) = -2 * Poissons(1) * Exp2

        For I = 1 To NLayers - 1

            Exp1 = System.Math.Exp(-ALPHA * (-HLayer(I) + OShift(I, 1)))
            Exp2 = System.Math.Exp(-ALPHA * (HLayer(I) + OShift(I, 2)))
            Exp3 = System.Math.Exp(-ALPHA * (OShift(I + 1, 1)))
            Exp4 = System.Math.Exp(-ALPHA * (OShift(I + 1, 2)))
            AlphaZ1 = ALPHA * HLayer(I)

            K = (I - 1) * 4

            J = I * 4 - 1
            '   Vertical stress.
            A(J, K + 1) = Exp1 : A(J, K + 2) = -Exp2
            A(J, K + 3) = -(1 - 2 * Poissons(I) - AlphaZ1) * Exp1
            A(J, K + 4) = -(1 - 2 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = Exp4
            A(J, K + 7) = (1 - 2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (1 - 2 * Poissons(I + 1)) * Exp4

            J = J + 1
            '   Shear stress.
            A(J, K + 1) = Exp1 : A(J, K + 2) = Exp2
            A(J, K + 3) = (2 * Poissons(I) + AlphaZ1) * Exp1
            A(J, K + 4) = -(2 * Poissons(I) - AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = -Exp4
            A(J, K + 7) = -(2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (2 * Poissons(I + 1)) * Exp4

            R = (1 + Poissons(I + 1)) * Youngs(I) / (Youngs(I + 1) * (1 + Poissons(I)))
            J = J + 1
            '   Vertical displacement.
            A(J, K + 1) = Exp1 : A(J, K + 2) = Exp2
            A(J, K + 3) = -(2 - 4 * Poissons(I) - AlphaZ1) * Exp1
            A(J, K + 4) = (2 - 4 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 * R : A(J, K + 6) = -Exp4 * R
            A(J, K + 7) = (2 - 4 * Poissons(I + 1)) * Exp3 * R
            A(J, K + 8) = -(2 - 4 * Poissons(I + 1)) * Exp4 * R

            J = J + 1
            '   Radial displacement for fully bonded (InterfaceParm = 1.0).
            '   Replaced with variable InterfaceParm equations below.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = -Exp2
            '    A(J, K + 3) = (1 + AlphaZ1) * Exp1
            '    A(J, K + 4) = (1 - AlphaZ1) * Exp2
            '    A(J, K + 5) = -Exp3 * R:  A(J, K + 6) = Exp4 * R
            '    A(J, K + 7) = -Exp3 * R
            '    A(J, K + 8) = -Exp4 * R

            '   Combine shear stress and radial displacement for interface shear.
            '   Small values of InterfaceParm, unbonded, cause numerical problems
            '   when pivotting is not done in the solver.

            AlphaG = -ALPHA * (1 - InterfaceParm(I))
            R1 = InterfaceParm(I) * (1 + Poissons(I)) / Youngs(I)
            R2 = InterfaceParm(I) * (1 + Poissons(I + 1)) / Youngs(I + 1)

            A(J, K + 1) = (AlphaG - R1) * Exp1
            A(J, K + 2) = (AlphaG + R1) * Exp2
            A(J, K + 3) = (AlphaG * (2 * Poissons(I) + AlphaZ1) - (1 + AlphaZ1) * R1) * Exp1
            A(J, K + 4) = (-AlphaG * (2 * Poissons(I) - AlphaZ1) - (1 - AlphaZ1) * R1) * Exp2
            A(J, K + 5) = R2 * Exp3
            A(J, K + 6) = -R2 * Exp4
            A(J, K + 7) = R2 * Exp3
            A(J, K + 8) = R2 * Exp4

        Next I

        ' Reduce from an (NLayers * 4) by (NLayers * 4) system to
        ' an (NLayers * 4 - 2) by (NLayers * 4 - 2) system.
        ' This satisfies the infinite subgrade with A and C zero
        ' for the subgrade layer. Return (NLayers * 4) coefficients
        ' by moving returned coefficients and inserting zeros (see below).
        K = 4 * NLayers - 2
        If NLayers > 1 Then
            For I = K - 3 To K
                A(I, K - 1) = A(I, K)
                A(I, K) = A(I, K + 2)
            Next I
        Else
            '   Special case for one layer. A1 = C1 = 0.
            '   Surface boundary 2x4 matrix set at top of subroutine.
            A(1, 1) = A(1, 2)
            A(1, 2) = A(1, 4)
            A(2, 1) = A(2, 2)
            A(2, 2) = A(2, 4)
        End If

        If 2.4 < ALPHA And ALPHA < 2.4 Then
            System.Diagnostics.Debug.WriteLine("Alpha = " & ALPHA)
            For I = 1 To K '+ 2
                For J = 1 To K '+ 2
                    System.Diagnostics.Debug.Write(LPad(10, VB6.Format(A(I, J), "0.00E+00")))
                Next J
                System.Diagnostics.Debug.WriteLine("")
            Next I
        End If

        IFail = 0
        ' B() is set in first call to FindConstantsPartInvert, so initialize.
        For I = 1 To K : B(I) = 0.0# : Next I
        B(1) = 1.0#

        If LEAFSolver = LEAFSolvers.GaussJordanSolver Then
            Inverse = False
            IRefine = False
            Call GaussJ(A, K, K, B, 1, 1, Inverse, IRefine, IFail) ' Full pivoting.
        ElseIf LEAFSolver = LEAFSolvers.LUSolver Then
            Call LUsolve(A, B, K, IFail) ' Partial pivoting.
            BadCondition = IFail
        ElseIf LEAFSolver = LEAFSolvers.GaussSolver Then
            Call GAUSS(A, B, K, IFail) ' No pivoting.
        ElseIf LEAFSolver = LEAFSolvers.LUBandSolver Then

            ReDim ipiv(K)
            KL = 5
            KU = 5
            LDAB = KL + KU + 1
            LDAFB = 2 * KL + KU + 1
            LDB = K
            NRHS = 1
            ReDim AB(LDAB, K)
            ReDim AFB(LDAFB, K)
            ReDim LUR(K)
            ReDim LUC(K)
            Dim LUB(LDB, NRHS) As Double
            ReDim LUX(LDB, NRHS)
            ReDim FERR(NRHS)
            ReDim BERR(NRHS)
            ReDim LUWork(3 * K)
            ReDim LUIWork(K)

            For J = 1 To K
                If J - KU > 1 Then ITemp1 = J - KU Else ITemp1 = 1
                If J + KL < K Then ITemp2 = J + KL Else ITemp2 = K
                For I = ITemp1 To ITemp2
                    AB(KU + 1 + I - J, J) = A(I, J)
                Next I
            Next J

            For I = 1 To K
                LUB(I, 1) = B(I) : Next I

            '    Call SGBSVX("N", "N", K, KL, KU, NRHS, AB(), LDAB, AFB(), LDAFB, ipiv(), EQUED, LUR(), LUC(), B(), K, LUX(), K, RCOND, FERR(), BERR(), LUWork(), LUIWork(), Info)

            For I = 1 To K
                B(I) = LUB(I, 1) : Next I
        End If

        If IFail <> 0 Then
            '    Debug.Print "IFail = "; IFail; "Alpha = "; ALPHA
        End If

        If 2.4 < ALPHA And ALPHA < 2.4 Then
            System.Diagnostics.Debug.WriteLine("Alpha = " & ALPHA & "IFail = " & IFail)
            For I = 1 To K '+ 2
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(B(I), "0.00E+00")))
            Next I
            System.Diagnostics.Debug.WriteLine("")
        End If

        ' Correctly align the coefficients for the infinite subgrade
        ' and insert two zeros.
        B(K + 2) = B(K)
        B(K + 1) = 0
        B(K) = B(K - 1)
        B(K - 1) = 0

    End Sub


    Private Sub IntegrateZStrain(ByRef EvalLayer As Integer, ByRef ZEval As Double, ByRef StrainW(,) As Double)

        ' Computes vertical strain.

        Dim I As Integer
        'ik* Dim LFNo As Short
        Dim s As String
        Dim Factor As Double
        Dim B() As Double
        Dim IEval, IAC, ITire, IG As Integer
        Dim CK, AK, BK, DK As Double
        Dim IConstants As Integer
        Dim ZLayer As Double
        Dim Poisx2, AlphaZ As Double
        Dim ZLayer1, Z1, Z2, ZLayer2 As Double
        Dim A1, R1, R2, A2 As Double
        Dim StrainWI As Double
        'ik* Dim StrainW1, StrainW2 As Double
        Dim StrainWIG1, StrainWIG2 As Double
        Dim StrainWIGforConverge As Double
        Dim StrainWmax() As Double
        Dim J1AlphaA1, J1AlphaA2 As Double
        Dim J0AlphaR1, J0AlphaR2 As Double
        Dim Converged(,,) As Boolean
        Dim NEvalsConverged(,) As Integer
        Dim NTiresConverged() As Integer
        Dim ACConverged() As Boolean
        Dim NACConverged As Integer
        '  Dim IterationstoConverge() As Long
        'ik* Dim TimeSave2, TimeSave1, TimeTotal As Integer
        'ik* Dim BadCondition As Integer
        Dim FirstTimeFlag1, FirstTimeFlag2 As Boolean
        Dim NEvalPointsMax, NConverge As Integer
        Dim OShift(,) As Double
        Dim RMax As Double 'ik* , AMAX As Double

        Call GetMaxParms(NEvalPointsMax, NTiresMax, RMax)

        '  GLNGauss, GLAlpha(), and GLWeight() are set in frmStartup.cmdInitLibs_Click().

        ReDim StrainW(NAC, NEvalPointsMax)
        ReDim StrainWmax(NAC)
        ReDim Converged(NAC, NTiresMax, NEvalPointsMax)
        ReDim NEvalsConverged(NAC, NTiresMax)
        ReDim NTiresConverged(NAC)
        ReDim ACConverged(NAC)
        ReDim IterationstoConverge(NAC)

        I = EvalLayer
        ZLayer = ZEval - ZInterface(I - 1)

        Call SetOShifts(OShift, I, ZLayer, RMax)

        Z1 = OShift(I, 1) - ZLayer
        Z2 = OShift(I, 2) + ZLayer

        ZLayer1 = ZLayer / Z1
        ZLayer2 = ZLayer / Z2
        Poisx2 = Poissons(EvalLayer) * 2

        IConstants = (EvalLayer - 1) * 4

        For IG = 1 To GLNGauss
            System.Windows.Forms.Application.DoEvents()
            StrainWIGforConverge = 0
            If EvalLayer < NLayers Then
                Call FindConstants(GLAlpha(IG) / Z1, B, OShift, FirstTimeFlag1, 1)
                AK = B(IConstants + 1)
                AlphaZ = GLAlpha(IG) * ZLayer1
                CK = B(IConstants + 3) * (1 - Poisx2 * 2 - AlphaZ)
                StrainWIG1 = -(AK - CK) * GLWeight(IG) / Z1
                StrainWIGforConverge = (System.Math.Abs(AK) + System.Math.Abs(CK)) * GLWeight(IG) / Z1
            End If
            Call FindConstants(GLAlpha(IG) / Z2, B, OShift, FirstTimeFlag2, 2)
            BK = B(IConstants + 2)
            AlphaZ = GLAlpha(IG) * ZLayer2
            DK = B(IConstants + 4) * (1 - Poisx2 * 2 + AlphaZ)
            StrainWIG2 = (BK + DK) * GLWeight(IG) / Z2
            StrainWIGforConverge = StrainWIGforConverge + (System.Math.Abs(BK) + System.Math.Abs(DK)) * GLWeight(IG) / Z2

            For IAC = 1 To NAC
                If Not ACConverged(IAC) Then
                    A2 = TireRadius(IAC, 1) ' Move down 1 loop for varying pressures.
                    A1 = A2 / Z1
                    A2 = A2 / Z2
                    If EvalLayer < NLayers Then
                        J1AlphaA1 = LoadFunction(GLAlpha(IG), A1) * StrainWIG1
                        '          J1AlphaA1 = bessj1(GLAlpha(IG) * A1) * StrainWIG1
                    End If
                    J1AlphaA2 = LoadFunction(GLAlpha(IG), A2) * StrainWIG2
                    '        J1AlphaA2 = bessj1(GLAlpha(IG) * A2) * StrainWIG2
                    For ITire = 1 To NTires(IAC)
                        For IEval = 1 To NEvalPoints(IAC)
                            R2 = Radius(IAC, ITire, IEval)
                            R1 = R2 / Z1
                            R2 = R2 / Z2
                            If EvalLayer < NLayers Then
                                J0AlphaR1 = bessj0(GLAlpha(IG) * R1)
                            End If
                            J0AlphaR2 = bessj0(GLAlpha(IG) * R2)
                            StrainWI = J0AlphaR1 * J1AlphaA1 + J0AlphaR2 * J1AlphaA2
                            StrainW(IAC, IEval) = StrainW(IAC, IEval) + StrainWI
                            If System.Math.Abs(StrainWIGforConverge / StrainW(IAC, IEval)) < ConvergenceLimit Then
                                If Not Converged(IAC, ITire, IEval) Then
                                    NEvalsConverged(IAC, ITire) = NEvalsConverged(IAC, ITire) + 1
                                    If NEvalsConverged(IAC, ITire) = NEvalPoints(IAC) Then
                                        NTiresConverged(IAC) = NTiresConverged(IAC) + 1
                                        IterationstoConverge(IAC) = IG
                                    End If
                                    Converged(IAC, ITire, IEval) = True
                                    If NTiresConverged(IAC) = NTires(IAC) Then
                                        ACConverged(IAC) = True
                                        NACConverged = NACConverged + 1
                                    End If
                                End If
                            End If
                            If IEval = -1 Then
                                System.Diagnostics.Debug.Write(VB6.Format(IG, "0") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(GLAlpha(IG), "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(GLWeight(IG), "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format((BK + DK) / Z2, "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(LoadFunction(GLAlpha(IG), A2), "0.000E+00") & " ")
                                System.Diagnostics.Debug.WriteLine(VB6.Format(J0AlphaR2, "0.000E+00"))
                            End If
                        Next IEval
                    Next ITire
                End If
            Next IAC
            If NACConverged = NAC Then
                NConverge = IG
                Exit For
            End If
        Next IG

        For IAC = 1 To NAC
            StrainWmax(IAC) = 0
            Factor = TirePress(IAC, 1) * TireRadius(IAC, 1)
            Factor = Factor * (1 + Poissons(EvalLayer)) / Youngs(EvalLayer)
            For IEval = 1 To NEvalPoints(IAC)
                StrainW(IAC, IEval) = (StrainW(IAC, IEval)) * Factor
                '     Compression is -ve, strain for LEDFAA must be -ve.
                If (StrainW(IAC, IEval)) < StrainWmax(IAC) Then
                    StrainWmax(IAC) = (StrainW(IAC, IEval))
                End If
                If FindingAllResponses Then
                    AllResp(IAC, IEval).StrainZ = StrainW(IAC, IEval)
                End If
            Next IEval
            s = "StrainWmax(" & VB6.Format(IAC, "0") & ") = " & LPad(14, VB6.Format(StrainWmax(IAC), "0.000000E+00")) ' & LPad(14, Format(TauRZ, "0.000000E+00")) & vbCrLf   '&; ILoop

        Next IAC

        '  For IAC = 1 To NAC
        '      SS$ = Left$(ACName$(IAC) & "            ", 12) & "  "
        '      Print #LFNo, SS$;
        '      Print #LFNo, LPad(10, Format(GL(IAC), "0.0"));
        '      Print #LFNo, LPad(3, Format(EvalLayer, "0"));
        '      Print #LFNo, LPad(7, Format(ZEval, "0.0"));
        '      Print #LFNo, LPad(17, Format(Youngs(EvalLayer), ".00000000E+00"));
        '      Print #LFNo, LPad(7, Format(IterationstoConverge(IAC), "0"));
        '      Print #LFNo, LPad(17, Format(StrainWmax(IAC), ".00000000E+00"))
        '    Debug.Print IA; SS$; STRNV(IA)
        '  Next IAC

    End Sub


    Private Sub IntegrateZStrain2(ByRef EvalLayer As Integer, ByRef ZEval As Double, ByRef StrainW(,) As Double) ' kairat replace tandem

        ' Computes vertical strain.

        Dim I As Integer
        'ik* Dim LFNo As Short
        Dim s As String
        Dim Factor As Double
        Dim B() As Double
        Dim IEval, IAC, ITire, IG As Integer
        Dim CK, AK, BK, DK As Double
        Dim IConstants As Integer
        Dim ZLayer As Double
        Dim Poisx2, AlphaZ As Double
        Dim ZLayer1, Z1, Z2, ZLayer2 As Double
        Dim A1, R1, R2, A2 As Double
        Dim StrainWI As Double
        'ik* Dim StrainW1, StrainW2 As Double
        Dim StrainWIG1, StrainWIG2 As Double
        Dim StrainWIGforConverge As Double
        Dim StrainWmax() As Double
        Dim J1AlphaA1, J1AlphaA2 As Double
        Dim J0AlphaR1, J0AlphaR2 As Double
        Dim Converged(,,) As Boolean
        Dim NEvalsConverged(,) As Integer
        Dim NTiresConverged() As Integer
        Dim ACConverged() As Boolean
        Dim NACConverged As Integer
        '  Dim IterationstoConverge() As Long
        'ik* Dim TimeSave2, TimeSave1, TimeTotal As Integer
        'ik* Dim BadCondition As Integer
        Dim FirstTimeFlag1, FirstTimeFlag2 As Boolean
        Dim NEvalPointsMax, NConverge As Integer
        Dim OShift(,) As Double
        Dim RMax As Double 'ik* , AMAX As Double

        Call GetMaxParms(NEvalPointsMax, NTiresMax, RMax)

        '  GLNGauss, GLAlpha(), and GLWeight() are set in frmStartup.cmdInitLibs_Click().

        ReDim StrainW(NAC, NEvalPointsMax)
        ReDim StrainWmax(NAC)
        ReDim Converged(NAC, NTiresMax, NEvalPointsMax)
        ReDim NEvalsConverged(NAC, NTiresMax)
        ReDim NTiresConverged(NAC)
        ReDim ACConverged(NAC)
        ReDim IterationstoConverge(NAC)

        I = EvalLayer
        ZLayer = ZEval - ZInterface(I - 1)

        Call SetOShifts(OShift, I, ZLayer, RMax)

        Z1 = OShift(I, 1) - ZLayer
        Z2 = OShift(I, 2) + ZLayer

        ZLayer1 = ZLayer / Z1
        ZLayer2 = ZLayer / Z2
        Poisx2 = Poissons(EvalLayer) * 2

        IConstants = (EvalLayer - 1) * 4

        For IG = 1 To GLNGauss
            System.Windows.Forms.Application.DoEvents()
            StrainWIGforConverge = 0
            If EvalLayer < NLayers Then
                Call FindConstants(GLAlpha(IG) / Z1, B, OShift, FirstTimeFlag1, 1)
                AK = B(IConstants + 1)
                AlphaZ = GLAlpha(IG) * ZLayer1
                CK = B(IConstants + 3) * (1 - Poisx2 * 2 - AlphaZ)
                StrainWIG1 = -(AK - CK) * GLWeight(IG) / Z1
                StrainWIGforConverge = (System.Math.Abs(AK) + System.Math.Abs(CK)) * GLWeight(IG) / Z1
            End If
            Call FindConstants(GLAlpha(IG) / Z2, B, OShift, FirstTimeFlag2, 2)
            BK = B(IConstants + 2)
            AlphaZ = GLAlpha(IG) * ZLayer2
            DK = B(IConstants + 4) * (1 - Poisx2 * 2 + AlphaZ)
            StrainWIG2 = (BK + DK) * GLWeight(IG) / Z2
            StrainWIGforConverge = StrainWIGforConverge + (System.Math.Abs(BK) + System.Math.Abs(DK)) * GLWeight(IG) / Z2

            For IAC = 1 To NAC
                If Not ACConverged(IAC) Then
                    A2 = TireRadius(IAC, 1) ' Move down 1 loop for varying pressures.

                    If Double.IsNaN(A2) Then
                        A2 = 0
                    End If

                    A1 = A2 / Z1
                    A2 = A2 / Z2
                    If EvalLayer < NLayers Then
                        J1AlphaA1 = LoadFunction(GLAlpha(IG), A1) * StrainWIG1
                        '          J1AlphaA1 = bessj1(GLAlpha(IG) * A1) * StrainWIG1
                    End If
                    J1AlphaA2 = LoadFunction(GLAlpha(IG), A2) * StrainWIG2
                    '        J1AlphaA2 = bessj1(GLAlpha(IG) * A2) * StrainWIG2
                    For ITire = 1 To NTires(IAC)
                        For IEval = 1 To NEvalPoints(IAC)
                            R2 = Radius(IAC, ITire, IEval)
                            R1 = R2 / Z1
                            R2 = R2 / Z2
                            If EvalLayer < NLayers Then
                                J0AlphaR1 = bessj0(GLAlpha(IG) * R1)
                            End If
                            J0AlphaR2 = bessj0(GLAlpha(IG) * R2)
                            StrainWI = J0AlphaR1 * J1AlphaA1 + J0AlphaR2 * J1AlphaA2
                            StrainW(IAC, IEval) = StrainW(IAC, IEval) + StrainWI
                            If System.Math.Abs(StrainWIGforConverge / StrainW(IAC, IEval)) < ConvergenceLimit Then
                                If Not Converged(IAC, ITire, IEval) Then
                                    NEvalsConverged(IAC, ITire) = NEvalsConverged(IAC, ITire) + 1
                                    If NEvalsConverged(IAC, ITire) = NEvalPoints(IAC) Then
                                        NTiresConverged(IAC) = NTiresConverged(IAC) + 1
                                        IterationstoConverge(IAC) = IG
                                    End If
                                    Converged(IAC, ITire, IEval) = True
                                    If NTiresConverged(IAC) = NTires(IAC) Then
                                        ACConverged(IAC) = True
                                        NACConverged = NACConverged + 1
                                    End If
                                End If
                            End If
                            If IEval = -1 Then
                                System.Diagnostics.Debug.Write(VB6.Format(IG, "0") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(GLAlpha(IG), "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(GLWeight(IG), "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format((BK + DK) / Z2, "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(LoadFunction(GLAlpha(IG), A2), "0.000E+00") & " ")
                                System.Diagnostics.Debug.WriteLine(VB6.Format(J0AlphaR2, "0.000E+00"))
                            End If
                        Next IEval
                    Next ITire
                End If
            Next IAC
            If NACConverged = NAC Then
                NConverge = IG
                Exit For
            End If
        Next IG

        For IAC = 1 To NAC
            'StrainWmax(IAC) = 0  ' we do not need max of strain at this subroutine kairat       
            Factor = TirePress(IAC, 1) * TireRadius(IAC, 1)
            Factor = Factor * (1 + Poissons(EvalLayer)) / Youngs(EvalLayer)
            For IEval = 1 To NEvalPoints(IAC)
                StrainW(IAC, IEval) = (StrainW(IAC, IEval)) * Factor
                '     Compression is -ve, strain for LEDFAA must be -ve.
                'If (StrainW(IAC, IEval)) < StrainWmax(IAC) Then
                '    StrainWmax(IAC) = (StrainW(IAC, IEval))
                'End If
                'If FindingAllResponses Then
                '    AllResp(IAC, IEval).StrainZ = StrainW(IAC, IEval)
                'End If
            Next IEval
            's = "StrainWmax(" & VB6.Format(IAC, "0") & ") = " & LPad(14, VB6.Format(StrainWmax(IAC), "0.000000E+00")) ' & LPad(14, Format(TauRZ, "0.000000E+00")) & vbCrLf   '&; ILoop

        Next IAC

    End Sub

    Private Sub IntegrateZStrainK1(ByRef EvalLayer As Integer, ByRef ZEval As Double, ByRef StrainW(,) As Double) ' Kairat Tuleubekov

        ' Computes vertical strain.

        Dim I As Integer
        'ik* Dim LFNo As Short
        Dim s As String
        Dim Factor As Double
        Dim B() As Double
        Dim IEval, IAC, ITire, IG As Integer
        Dim CK, AK, BK, DK As Double
        Dim IConstants As Integer
        Dim ZLayer As Double
        Dim Poisx2, AlphaZ As Double
        Dim ZLayer1, Z1, Z2, ZLayer2 As Double
        Dim A1, R1, R2, A2 As Double
        Dim StrainWI As Double
        'ik* Dim StrainW1, StrainW2 As Double
        Dim StrainWIG1, StrainWIG2 As Double
        Dim StrainWIGforConverge As Double
        Dim StrainWmax() As Double
        Dim J1AlphaA1, J1AlphaA2 As Double
        Dim J0AlphaR1, J0AlphaR2 As Double
        Dim Converged(,,) As Boolean
        Dim NEvalsConverged(,) As Integer
        Dim NTiresConverged() As Integer
        Dim ACConverged() As Boolean
        Dim NACConverged As Integer
        '  Dim IterationstoConverge() As Long
        'ik* Dim TimeSave2, TimeSave1, TimeTotal As Integer
        'ik* Dim BadCondition As Integer
        Dim FirstTimeFlag1, FirstTimeFlag2 As Boolean
        Dim NEvalPointsMax, NConverge As Integer
        Dim OShift(,) As Double
        Dim RMax As Double 'ik* , AMAX As Double

        Call GetMaxParms(NEvalPointsMax, NTiresMax, RMax)

        '  GLNGauss, GLAlpha(), and GLWeight() are set in frmStartup.cmdInitLibs_Click().

        ReDim StrainW(NAC, NEvalPointsMax)
        ReDim StrainWmax(NAC)
        ReDim Converged(NAC, NTiresMax, NEvalPointsMax)
        ReDim NEvalsConverged(NAC, NTiresMax)
        ReDim NTiresConverged(NAC)
        ReDim ACConverged(NAC)
        ReDim IterationstoConverge(NAC)

        I = EvalLayer
        ZLayer = ZEval - ZInterface(I - 1)

        Call SetOShifts(OShift, I, ZLayer, RMax)

        Z1 = OShift(I, 1) - ZLayer
        Z2 = OShift(I, 2) + ZLayer

        ZLayer1 = ZLayer / Z1
        ZLayer2 = ZLayer / Z2
        Poisx2 = Poissons(EvalLayer) * 2

        IConstants = (EvalLayer - 1) * 4

        For IG = 1 To GLNGauss
            System.Windows.Forms.Application.DoEvents()
            StrainWIGforConverge = 0
            If EvalLayer < NLayers Then
                Call FindConstants(GLAlpha(IG) / Z1, B, OShift, FirstTimeFlag1, 1)
                AK = B(IConstants + 1)
                AlphaZ = GLAlpha(IG) * ZLayer1
                CK = B(IConstants + 3) * (1 - Poisx2 * 2 - AlphaZ)
                StrainWIG1 = -(AK - CK) * GLWeight(IG) / Z1
                StrainWIGforConverge = (System.Math.Abs(AK) + System.Math.Abs(CK)) * GLWeight(IG) / Z1
            End If
            Call FindConstants(GLAlpha(IG) / Z2, B, OShift, FirstTimeFlag2, 2)
            BK = B(IConstants + 2)
            AlphaZ = GLAlpha(IG) * ZLayer2
            DK = B(IConstants + 4) * (1 - Poisx2 * 2 + AlphaZ)
            StrainWIG2 = (BK + DK) * GLWeight(IG) / Z2
            StrainWIGforConverge = StrainWIGforConverge + (System.Math.Abs(BK) + System.Math.Abs(DK)) * GLWeight(IG) / Z2

            For IAC = 1 To NAC
                'PrintLine(9, LPad(3, CStr(IAC))) 'kairat
                If Not ACConverged(IAC) Then
                    A2 = TireRadius(IAC, 1) ' Move down 1 loop for varying pressures.
                    A1 = A2 / Z1
                    A2 = A2 / Z2
                    If EvalLayer < NLayers Then
                        J1AlphaA1 = LoadFunction(GLAlpha(IG), A1) * StrainWIG1
                        '          J1AlphaA1 = bessj1(GLAlpha(IG) * A1) * StrainWIG1
                    End If
                    J1AlphaA2 = LoadFunction(GLAlpha(IG), A2) * StrainWIG2
                    '        J1AlphaA2 = bessj1(GLAlpha(IG) * A2) * StrainWIG2
                    For ITire = 1 To NTires(IAC)
                        For IEval = 1 To NEvalPoints(IAC)
                            R2 = Radius(IAC, ITire, IEval)
                            R1 = R2 / Z1
                            R2 = R2 / Z2
                            If EvalLayer < NLayers Then
                                J0AlphaR1 = bessj0(GLAlpha(IG) * R1)
                            End If
                            J0AlphaR2 = bessj0(GLAlpha(IG) * R2)
                            StrainWI = J0AlphaR1 * J1AlphaA1 + J0AlphaR2 * J1AlphaA2
                            StrainW(IAC, IEval) = StrainW(IAC, IEval) + StrainWI
                            If System.Math.Abs(StrainWIGforConverge / StrainW(IAC, IEval)) < ConvergenceLimit Then
                                If Not Converged(IAC, ITire, IEval) Then
                                    NEvalsConverged(IAC, ITire) = NEvalsConverged(IAC, ITire) + 1
                                    If NEvalsConverged(IAC, ITire) = NEvalPoints(IAC) Then
                                        NTiresConverged(IAC) = NTiresConverged(IAC) + 1
                                        IterationstoConverge(IAC) = IG
                                    End If
                                    Converged(IAC, ITire, IEval) = True
                                    If NTiresConverged(IAC) = NTires(IAC) Then
                                        ACConverged(IAC) = True
                                        NACConverged = NACConverged + 1
                                    End If
                                End If
                            End If
                            If IEval = -1 Then
                                System.Diagnostics.Debug.Write(VB6.Format(IG, "0") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(GLAlpha(IG), "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(GLWeight(IG), "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format((BK + DK) / Z2, "0.000E+00") & " ")
                                System.Diagnostics.Debug.Write(VB6.Format(LoadFunction(GLAlpha(IG), A2), "0.000E+00") & " ")
                                System.Diagnostics.Debug.WriteLine(VB6.Format(J0AlphaR2, "0.000E+00"))
                            End If
                        Next IEval
                    Next ITire
                End If
            Next IAC
            If NACConverged = NAC Then
                NConverge = IG
                Exit For
            End If
        Next IG

        For IAC = 1 To NAC
            'StrainWmax(IAC) = 0  ' we do not need max of strain at this subroutine kairat
            Factor = TirePress(IAC, 1) * TireRadius(IAC, 1)
            Factor = Factor * (1 + Poissons(EvalLayer)) / Youngs(EvalLayer)
            For IEval = 1 To NEvalPoints(IAC)
                StrainW(IAC, IEval) = (StrainW(IAC, IEval)) * Factor
                '     Compression is -ve, strain for LEDFAA must be -ve.

                'If (StrainW(IAC, IEval)) < StrainWmax(IAC) Then  
                '    StrainWmax(IAC) = (StrainW(IAC, IEval))
                'End If
                'If FindingAllResponses Then
                '    AllResp(IAC, IEval).StrainZ = StrainW(IAC, IEval)
                'End If 
            Next IEval
            's = "StrainWmax(" & VB6.Format(IAC, "0") & ") = " & LPad(14, VB6.Format(StrainWmax(IAC), "0.000000E+00")) ' & LPad(14, Format(TauRZ, "0.000000E+00")) & vbCrLf   '&; ILoop
        Next IAC

        'Print(9, " XWhls(IA, I) " & LPad(10, CStr(XWhls(IA, I))) & "  xCenter " & LPad(10, CStr(xCenter)))
        'Print(9, " CtoP1 " & LPad(10, CStr(Math.Round(CtoP1, 7))))
        'PrintLine(9, " ")

        'FileClose(9)

    End Sub

    Private Sub IntegrateZDeflection(ByRef EvalLayer As Integer, ByRef ZEval As Double, ByRef DeflW(,) As Double)

        ' Computes vertical deflection

        Dim I As Integer
        'ik* Dim LFNo As Short
        Dim s As String
        Dim Factor As Double
        Dim B() As Double
        Dim IEval, IAC, ITire, IG As Integer
        Dim CK, AK, BK, DK As Double
        Dim IConstants As Integer
        Dim ZLayer As Double
        Dim Poisx2, AlphaZ As Double
        Dim ZLayer1, Z1, Z2, ZLayer2 As Double
        Dim A1, R1, R2, A2 As Double
        'ik* Dim Defl() As Double
        'ik* Dim DeflMax() As Double
        Dim DeflWI As Double
        'ik* Dim DeflW1, DeflW2 As Double
        Dim DeflWIG1, DeflWIG2 As Double
        Dim DeflWIGforConverge As Double
        Dim J1AlphaA1, J1AlphaA2 As Double
        Dim J0AlphaR1, J0AlphaR2 As Double
        Dim Converged(,,) As Boolean
        Dim NEvalsConverged(,) As Integer
        Dim NTiresConverged() As Integer
        Dim ACConverged() As Boolean
        Dim NACConverged As Integer
        '  Dim IterationstoConverge() As Long
        'ik* Dim TimeSave2, TimeSave1, TimeTotal As Integer
        'ik* Dim BadCondition As Integer
        Dim FirstTimeFlag1, FirstTimeFlag2 As Boolean
        Dim NEvalPointsMax As Integer
        Dim OShift(,) As Double
        Dim RMax As Double 'ik*, AMAX As Double

        Call GetMaxParms(NEvalPointsMax, NTiresMax, RMax)

        ReDim DeflW(NAC, NEvalPointsMax)
        Dim DeflWmax(NAC) As Double
        ReDim Converged(NAC, NTiresMax, NEvalPointsMax)
        ReDim NEvalsConverged(NAC, NTiresMax)
        ReDim NTiresConverged(NAC)
        ReDim ACConverged(NAC)
        ReDim IterationstoConverge(NAC)

        I = EvalLayer
        ZLayer = ZEval - ZInterface(I - 1)

        Call SetOShifts(OShift, I, ZLayer, RMax)

        Z1 = OShift(I, 1) - ZLayer
        Z2 = OShift(I, 2) + ZLayer

        ZLayer1 = ZLayer / Z1
        ZLayer2 = ZLayer / Z2
        Poisx2 = Poissons(EvalLayer) * 2

        IConstants = (EvalLayer - 1) * 4

        For IG = 1 To GLNGauss
            System.Windows.Forms.Application.DoEvents()
            DeflWIGforConverge = 0
            If EvalLayer < NLayers Then
                Call FindConstants(GLAlpha(IG) / Z1, B, OShift, FirstTimeFlag1, 1)
                AK = B(IConstants + 1)
                AlphaZ = GLAlpha(IG) * ZLayer1
                CK = B(IConstants + 3) * (2 - Poisx2 * 2 - AlphaZ)
                DeflWIG1 = -(AK - CK) * GLWeight(IG) / GLAlpha(IG)
                DeflWIGforConverge = (System.Math.Abs(AK) + System.Math.Abs(CK)) * GLWeight(IG) / GLAlpha(IG)
            End If
            Call FindConstants(GLAlpha(IG) / Z2, B, OShift, FirstTimeFlag2, 2)
            BK = B(IConstants + 2)
            AlphaZ = GLAlpha(IG) * ZLayer2
            DK = B(IConstants + 4) * (2 - Poisx2 * 2 + AlphaZ)
            DeflWIG2 = -(BK + DK) * GLWeight(IG) / GLAlpha(IG)
            DeflWIGforConverge = DeflWIGforConverge + (System.Math.Abs(BK) + System.Math.Abs(DK)) * GLWeight(IG) / GLAlpha(IG)

            For IAC = 1 To NAC
                If Not ACConverged(IAC) Then
                    A2 = TireRadius(IAC, 1) ' Move down 1 loop for varying pressures.
                    A1 = A2 / Z1
                    A2 = A2 / Z2
                    If EvalLayer < NLayers Then
                        J1AlphaA1 = LoadFunction(GLAlpha(IG), A1) * DeflWIG1
                    End If
                    J1AlphaA2 = LoadFunction(GLAlpha(IG), A2) * DeflWIG2
                    For ITire = 1 To NTires(IAC)
                        For IEval = 1 To NEvalPoints(IAC)
                            R2 = Radius(IAC, ITire, IEval)
                            R1 = R2 / Z1
                            R2 = R2 / Z2
                            If EvalLayer < NLayers Then J0AlphaR1 = bessj0(GLAlpha(IG) * R1)
                            J0AlphaR2 = bessj0(GLAlpha(IG) * R2)
                            DeflWI = J0AlphaR1 * J1AlphaA1 + J0AlphaR2 * J1AlphaA2
                            DeflW(IAC, IEval) = DeflW(IAC, IEval) + DeflWI
                            If System.Math.Abs(DeflWIGforConverge / (DeflW(IAC, IEval) + 1.0E-200)) < ConvergenceLimit Then
                                If Not Converged(IAC, ITire, IEval) Then
                                    NEvalsConverged(IAC, ITire) = NEvalsConverged(IAC, ITire) + 1
                                    If NEvalsConverged(IAC, ITire) = NEvalPoints(IAC) Then
                                        NTiresConverged(IAC) = NTiresConverged(IAC) + 1
                                        IterationstoConverge(IAC) = IG
                                    End If
                                    Converged(IAC, ITire, IEval) = True
                                    If NTiresConverged(IAC) = NTires(IAC) Then
                                        ACConverged(IAC) = True
                                        NACConverged = NACConverged + 1
                                    End If
                                End If
                            End If
                        Next IEval
                    Next ITire
                End If
            Next IAC
            If NACConverged = NAC Then Exit For
        Next IG

        For IAC = 1 To NAC
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object DeflWmax(IAC). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            DeflWmax(IAC) = 0
            Factor = TirePress(IAC, 1) * TireRadius(IAC, 1)
            Factor = Factor * (1 + Poissons(EvalLayer)) / Youngs(EvalLayer)
            For IEval = 1 To NEvalPoints(IAC)
                DeflW(IAC, IEval) = (DeflW(IAC, IEval)) * Factor
                'ik* UPGRADE_WARNING: Couldn't resolve default property of object DeflWmax(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                If (System.Math.Abs(DeflW(IAC, IEval))) > DeflWmax(IAC) Then
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object DeflWmax(IAC). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    DeflWmax(IAC) = (DeflW(IAC, IEval))
                End If
                If FindingAllResponses Then
                    AllResp(IAC, IEval).DeflZ = DeflW(IAC, IEval)
                End If
            Next IEval
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object DeflWmax(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            s = "DeflWmax(" & VB6.Format(IAC, "0") & ") = " & LPad(14, VB6.Format(DeflWmax(IAC), "0.000000E+00"))

        Next IAC

        '  For IAC = 1 To NAC
        '      SS$ = Left$(ACName$(IAC) & "            ", 12) & "  "
        '      Print #LFNo, SS$;
        '      Print #LFNo, LPad(10, Format(GL(IAC), "0.0"));
        '      Print #LFNo, LPad(3, Format(EvalLayer, "0"));
        '      Print #LFNo, LPad(7, Format(ZEval, "0.0"));
        '      Print #LFNo, LPad(17, Format(Youngs(EvalLayer), ".00000000E+00"));
        '      Print #LFNo, LPad(7, Format(IterationstoConverge(IAC), "0"));
        '      Print #LFNo, LPad(17, Format(DeflWmax(IAC), ".00000000E+00"))
        '    Debug.Print IA; SS$; STRNV(IA)
        '  Next IAC

    End Sub

    Private Sub IntegrateHorizontalDeflection(ByRef EvalLayer As Integer, ByRef ZEval As Double, ByRef Response(,,) As Double)

        ' Computes vertical deflection

        Dim I As Integer
        'ik* Dim LFNo As Short
        'ik* Dim s As String
        Dim Factor As Double
        Dim B() As Double
        Dim IEval, IAC, ITire, IG As Integer
        Dim CK, AK, BK, DK As Double
        Dim IConstants As Integer
        Dim ZLayer As Double
        Dim Poisx2, AlphaZ As Double
        Dim CosTheta, SinTheta As Double
        Dim ZLayer1, Z1, Z2, ZLayer2 As Double
        Dim A1, R1, R2, A2 As Double
        Dim DeflUI As Double
        Dim DeflU(,,) As Double
        'ik* Dim DeflU1, DeflU2 As Double
        Dim DeflUIG1, DeflUIG2 As Double
        Dim DeflUIGforConverge As Double
        Dim DeflX(,) As Double
        Dim DeflXmax() As Double
        Dim DeflY(,) As Double
        Dim DeflYmax() As Double
        Dim J1AlphaA1, J1AlphaA2 As Double
        Dim J1AlphaR1, J1AlphaR2 As Double
        Dim Converged(,,) As Boolean
        Dim NEvalsConverged(,) As Integer
        Dim NTiresConverged() As Integer
        Dim ACConverged() As Boolean
        Dim NACConverged As Integer
        '  Dim IterationstoConverge() As Long
        'ik* Dim TimeSave2, TimeSave1, TimeTotal As Integer
        'ik* Dim BadCondition As Integer
        Dim FirstTimeFlag1, FirstTimeFlag2 As Boolean
        Dim NEvalPointsMax As Integer
        Dim OShift(,) As Double
        Dim RMax As Double 'ik*, AMAX As Double

        Call GetMaxParms(NEvalPointsMax, NTiresMax, RMax)

        ReDim Response(NAC, NEvalPointsMax, 2)
        ReDim DeflU(NAC, NTiresMax, NEvalPointsMax)
        ReDim DeflX(NAC, NEvalPointsMax)
        ReDim DeflY(NAC, NEvalPointsMax)
        ReDim DeflXmax(NAC)
        ReDim DeflYmax(NAC)
        ReDim Converged(NAC, NTiresMax, NEvalPointsMax)
        ReDim NEvalsConverged(NAC, NTiresMax)
        ReDim NTiresConverged(NAC)
        ReDim ACConverged(NAC)
        ReDim IterationstoConverge(NAC)

        I = EvalLayer
        ZLayer = ZEval - ZInterface(I - 1)

        Call SetOShifts(OShift, I, ZLayer, RMax)

        Z1 = OShift(I, 1) - ZLayer
        Z2 = OShift(I, 2) + ZLayer

        ZLayer1 = ZLayer / Z1
        ZLayer2 = ZLayer / Z2
        Poisx2 = Poissons(EvalLayer) * 2

        IConstants = (EvalLayer - 1) * 4

        '  TimeSave1 = timeGetTime
        For IG = 1 To GLNGauss
            System.Windows.Forms.Application.DoEvents()
            DeflUIGforConverge = 0
            If EvalLayer < NLayers Then
                Call FindConstants(GLAlpha(IG) / Z1, B, OShift, FirstTimeFlag1, 1)
                AK = B(IConstants + 1)
                AlphaZ = GLAlpha(IG) * ZLayer1
                CK = B(IConstants + 3) * (1 + AlphaZ)
                DeflUIG1 = (AK + CK) * GLWeight(IG) / GLAlpha(IG)
                DeflUIGforConverge = (System.Math.Abs(AK) + System.Math.Abs(CK)) * GLWeight(IG) / GLAlpha(IG)
            End If
            '    TimeSave1 = timeGetTime
            Call FindConstants(GLAlpha(IG) / Z2, B, OShift, FirstTimeFlag2, 2)
            BK = B(IConstants + 2)
            AlphaZ = GLAlpha(IG) * ZLayer2
            DK = B(IConstants + 4) * (1 - AlphaZ)
            DeflUIG2 = -(BK - DK) * GLWeight(IG) / GLAlpha(IG)
            DeflUIGforConverge = DeflUIGforConverge + (System.Math.Abs(BK) + System.Math.Abs(DK)) * GLWeight(IG) / GLAlpha(IG)
            For IAC = 1 To NAC
                If Not ACConverged(IAC) Then
                    A2 = TireRadius(IAC, 1) ' Move down 1 loop for varying pressures.
                    A1 = A2 / Z1
                    A2 = A2 / Z2
                    If EvalLayer < NLayers Then
                        J1AlphaA1 = LoadFunction(GLAlpha(IG), A1) * DeflUIG1
                    End If
                    J1AlphaA2 = LoadFunction(GLAlpha(IG), A2) * DeflUIG2
                    For ITire = 1 To NTires(IAC)
                        For IEval = 1 To NEvalPoints(IAC)
                            R2 = Radius(IAC, ITire, IEval)
                            R1 = R2 / Z1
                            R2 = R2 / Z2
                            If EvalLayer < NLayers Then J1AlphaR1 = bessj1(GLAlpha(IG) * R1)
                            J1AlphaR2 = bessj1(GLAlpha(IG) * R2)
                            DeflUI = J1AlphaR1 * J1AlphaA1 + J1AlphaR2 * J1AlphaA2
                            DeflU(IAC, ITire, IEval) = DeflU(IAC, ITire, IEval) + DeflUI
                            If System.Math.Abs(DeflUIGforConverge / (DeflU(IAC, ITire, IEval) + 0.0001)) < ConvergenceLimit Then
                                If Not Converged(IAC, ITire, IEval) Then
                                    NEvalsConverged(IAC, ITire) = NEvalsConverged(IAC, ITire) + 1
                                    If NEvalsConverged(IAC, ITire) = NEvalPoints(IAC) Then
                                        NTiresConverged(IAC) = NTiresConverged(IAC) + 1
                                        IterationstoConverge(IAC) = IG
                                    End If
                                    Converged(IAC, ITire, IEval) = True
                                    If NTiresConverged(IAC) = NTires(IAC) Then
                                        ACConverged(IAC) = True
                                        NACConverged = NACConverged + 1
                                    End If
                                End If
                            End If
                        Next IEval
                    Next ITire
                End If
            Next IAC
            If NACConverged = NAC Then Exit For
        Next IG

        IterationstoConverge(1) = IG
        For IAC = 1 To NAC
            DeflXmax(IAC) = 0
            DeflYmax(IAC) = 0
            Factor = TirePress(IAC, 1) * TireRadius(IAC, 1)
            Factor = Factor * (1 + Poissons(EvalLayer)) / Youngs(EvalLayer)
            For IEval = 1 To NEvalPoints(IAC)
                For ITire = 1 To NTires(IAC)
                    R1 = Radius(IAC, ITire, IEval)
                    If System.Math.Abs(R1) < 0.000001 Then
                        '         StressR = StressT and shear stress is zero. It doesn't
                        '         matter what the values are as long as CosSq + SinSq = 1.
                        CosTheta = 1
                        SinTheta = 0
                    Else
                        CosTheta = (EvalX(IAC, IEval) - TireX(IAC, ITire)) / R1
                        SinTheta = (EvalY(IAC, IEval) - TireY(IAC, ITire)) / R1
                    End If
                    If FindingAllResponses Then
                        DeflUI = DeflU(IAC, ITire, IEval)
                        DeflX(IAC, IEval) = DeflX(IAC, IEval) + DeflUI * CosTheta
                        DeflY(IAC, IEval) = DeflY(IAC, IEval) + DeflUI * SinTheta
                    End If
                Next ITire
                DeflX(IAC, IEval) = DeflX(IAC, IEval) * Factor
                DeflY(IAC, IEval) = DeflY(IAC, IEval) * Factor
                Response(IAC, IEval, 1) = DeflX(IAC, IEval)
                Response(IAC, IEval, 2) = DeflY(IAC, IEval)
                If (System.Math.Abs(DeflX(IAC, IEval))) > DeflXmax(IAC) Then
                    DeflXmax(IAC) = DeflX(IAC, IEval)
                End If
                If (System.Math.Abs(DeflY(IAC, IEval))) > DeflYmax(IAC) Then
                    DeflYmax(IAC) = DeflY(IAC, IEval)
                End If
                If FindingAllResponses Then
                    AllResp(IAC, IEval).DeflX = DeflX(IAC, IEval)
                    AllResp(IAC, IEval).DeflY = DeflY(IAC, IEval)
                End If
            Next IEval

        Next IAC

    End Sub

    Sub FindConstants(ByRef GLAlphaByZ As Double, ByRef B() As Double, ByRef OShift(,) As Double, ByRef Started As Boolean, ByRef CallNo As Integer)

        Dim BadConditionPI, BadConditionLU As Integer
        Static BadConditionPIOccurred(2) As Boolean
        Static BadConditionLUOccurred(2) As Boolean


        If DesignType = "FlexOnRigid" Or _
           DesignType = "NewRigid" Or _
            DesignType = "UnbondOnRigid" Or _
            DesignType = "PartBondOnRigid" Or _
            DesignType = "FlexOnFlex" Then
            GoTo gotoGJS
        End If

        If (DesignType = "FlexOnFlex" Or DesignType = Nothing) _
        And gResponseType = LEAFoptions.AllResponses Then
            GoTo gotoGJS
        End If


        'OverrideSolver = 0 'ikawa 

        If OverrideSolver <> 0 Then
            If LEAFSolver = LEAFSolvers.PartInvertSolver Then
                Call FindConstantsPartInvert(GLAlphaByZ, B, OShift, BadConditionPI)
            Else
                Call FindConstantsFull(GLAlphaByZ, B, OShift, BadConditionLU)
            End If
            Exit Sub
        End If

        If Not Started Then
            BadConditionPIOccurred(CallNo) = False
            BadConditionLUOccurred(CallNo) = False
            Started = True ' Different variables in Calling routine for different CallNo.
        End If

        If Not BadConditionPIOccurred(CallNo) Then
            LEAFSolver = LEAFSolvers.PartInvertSolver
            Call FindConstantsPartInvert(GLAlphaByZ, B, OShift, BadConditionPI)
            If BadConditionPI = -1 Then BadConditionPIOccurred(CallNo) = True
        End If

        'BadConditionPIOccurred(CallNo) = True 'ikawa Jan06

        If BadConditionPIOccurred(CallNo) And Not BadConditionLUOccurred(CallNo) Then
            LEAFSolver = LEAFSolvers.LUSolver
            Call FindConstantsFull(GLAlphaByZ, B, OShift, BadConditionLU)
            If BadConditionLU = -1 Then BadConditionLUOccurred(CallNo) = True
        End If

        'BadConditionLUOccurred(CallNo) = True 'ikawa Jan06

gotoGJS:

        If BadConditionPIOccurred(CallNo) And BadConditionLUOccurred(CallNo) Then
            LEAFSolver = LEAFSolvers.GaussJordanSolver
            Call FindConstantsFull(GLAlphaByZ, B, OShift, BadConditionLU)
        End If

        Dim iii1 As Integer

        If DesignType = "FlexOnRigid" Or _
           DesignType = "NewRigid" Or _
            DesignType = "UnbondOnRigid" Or _
            DesignType = "PartBondOnRigid" Or _
            DesignType = "FlexOnFlex" Or DesignType = Nothing Then

            LEAFSolver = LEAFSolvers.GaussJordanSolver 'added 
            Call FindConstantsFull(GLAlphaByZ, B, OShift, BadConditionLU) 'added

            For iii1 = 1 To UBound(B, 1)
                If Double.IsNaN(B(iii1)) Or Double.IsInfinity(B(iii1)) Then
                    'If Double.IsNaN(B(iii1)) Then
                    B(iii1) = 0.0
                End If
            Next

            'For iii1 = 1 To UBound(B, 1)
            '    If Double.IsNaN(B(iii1)) Or Double.IsInfinity(B(iii1)) Then
            '        B(iii1) = 0.0
            '    ElseIf Double.IsNegativeInfinity(B(iii1)) Then
            '        B(iii1) = -4.9 * 10 ^ 324
            '    ElseIf Double.IsPositiveInfinity(B(iii1)) Then
            '        B(iii1) = 1.7 * 10 ^ 308
            '    End If
            'Next



            iii1 = 0
        End If


        ''Dim ii, jj, kk As Integer
        ''Static iii As Integer = 0
        ''iii = iii + 1
        ''FileOpen(21, "FED.txt", OpenMode.Append)
        ''PrintLine(21)
        ''PrintLine(21, iii)
        ''PrintLine(21, "Solver " & BadConditionPIOccurred(CallNo) & "  " _
        ''              & BadConditionLUOccurred(CallNo))
        ''PrintLine(21, "GLAlphaByZ= " & GLAlphaByZ)
        ''PrintLine(21, "")
        ''For ii = 1 To UBound(OShift, 1)
        ''    For jj = 1 To UBound(OShift, 2)
        ''        PrintLine(21, OShift(ii, jj))
        ''    Next
        ''Next

        ''PrintLine(21, "")
        ''PrintLine(21, "------------------------------")
        ''For ii = 1 To UBound(B, 1)
        ''    PrintLine(21, LPad(8, CStr(Math.Round(B(ii), 5))))
        ''Next ii
        ''PrintLine(21, "------------------------------")
        ''PrintLine(21)
        ''FileClose(21)


        ''If BadConditionLU <> 0 Then 'ikawa added
        ''    Call FindConstantsFull(GLAlphaByZ, B, OShift, BadConditionLU)
        ''End If

    End Sub

    Private Sub Prinpl(ByRef NSTYL As Integer, ByRef Theta As Double, ByRef Degree As Double, ByRef DIREC(,) As Double, ByRef PRIN() As Double, ByRef SIG() As Double)

        '      SUBROUTINE Prinpl(NSTYL, Theta, DEGREE, DIREC, PRIN, SIG) from FEMSKI
        '1508
        '*** TO FIND PRINCIPAL STRESSES AND THEIR DIRECTIONS IN THREE DIMENSIONS
        '
        '      IMPLICIT DOUBLE PRECISION (A-H,O-Z)
        '      DIMENSION ADJ(3, 3), DIREC(3, 3), PRIN(3), SIG(6), TENS(3, 3)
        '      DATA PI/3.14159265359/, ONE/1.0/
        Dim ADJ(3, 3) As Double
        Dim TENS(3, 3) As Double
        Const PI As Double = 3.14159265359
        Const ONE As Double = 1
        Dim K, I, J, II As Integer
        Dim S2, S1, s3 As Double
        Dim D, B, c, R As Double
        Dim ARG, Gash As Double
        Dim MAXCOL As Integer
        Dim [Continue] As Boolean
        Dim BIG As Double
        Dim COSE, SINE, Del As Double
        Dim SCALER As Double ' Replaces SCALE for compatibility with VB.
        Dim U(3) As Double
        Dim V(3) As Double
        Dim W(3) As Double
        Dim AlmostOne As Double

        S1 = SIG(1)
        S2 = SIG(2)
        s3 = SIG(3)

        '      GO TO (2, 4, 10), NSTYL
        If NSTYL = 1 Then GoTo 2
        If NSTYL = 2 Then GoTo 4
        If NSTYL = 3 Then GoTo 10

        '2     Theta = ATAN(S2 / (S1 + 1E-30))
        '      PRIN(1) = SQRT(S1 * S1 + S2 * S2)
        '      IF(S1.LT.0.0. AND .S2.GT.0.0) THETA = THETA + PI
        '      IF(S1.LT.0.0. AND .S2.LE.0.0) PRIN(1) = -PRIN(1)
2:      Theta = System.Math.Atan(S2 / (S1 + 1.0E-30))
        PRIN(1) = System.Math.Sqrt(S1 * S1 + S2 * S2)
        If (S1 < 0.0# And S2 > 0.0#) Then Theta = Theta + PI
        If (S1 < 0.0# And S2 <= 0.0#) Then PRIN(1) = -PRIN(1)
        GoTo 8

        '4     Theta = 0.5 * ATAN(2# * s3 / (S1 - S2 + 1E-30))
        '    6 IF(THETA.LT.0.0) THETA = THETA + PI
4:      Theta = 0.5 * System.Math.Atan(2.0# * s3 / (S1 - S2 + 1.0E-30))
6:      If (Theta < 0.0#) Then Theta = Theta + PI
        SINE = System.Math.Sin(Theta)
        COSE = System.Math.Cos(Theta)
        PRIN(1) = S1 + SINE * (2.0# * COSE * s3 + SINE * (S2 - S1))
        PRIN(2) = S1 + S2 - PRIN(1)
        '
        '*** MAKE THE FIRST PRINCIPAL STRESS THE LARGER OF THE TWO IN EVERY CASE
        '
        '      IF(ABS(PRIN(1)).GE.0.99999*ABS(PRIN(2))) GO TO 8
        If (System.Math.Abs(PRIN(1)) >= 0.99999 * System.Math.Abs(PRIN(2))) Then GoTo 8
        Theta = Theta - 0.5 * PI
        GoTo 6
8:      Degree = Theta * 180.0# / PI
        '      Return
        Exit Sub
        '
        '*** RAPID AND FOOLPROOF ANALYTIC PROCEDURE, FOR 3X3 EIGENVALUES.
        '
10:     J = 3
        '      DO 12 I = 1,3
        For I = 1 To 3
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(I, I). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            TENS(I, I) = SIG(I)
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(J, 6 - I - J). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            TENS(J, 6 - I - J) = SIG(3 + I)
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(6 - I - J, J). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            TENS(6 - I - J, J) = SIG(3 + I)
12:         J = I
        Next I
        B = 0.0#
        c = 0.0#
        '      DO 14 I = 1,3
        For I = 1 To 3
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            B = B + TENS(I, I)
            J = I + 1
            '       IF(J.EQ.4) J = 1
            '    14 C = C - TENS(I,I)*TENS(J,J) + TENS(I,J)**2
            If (J = 4) Then J = 1
14:         'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            c = c - TENS(I, I) * TENS(J, J) + TENS(I, J) ^ 2
        Next I
        '      Call Vector(TENS, TENS(1, 2), ADJ)
        For II = 1 To 3
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(II, 1). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            U(II) = TENS(II, 1)
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(II, 2). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            V(II) = TENS(II, 2)
        Next II
        Call Vector(U, V, W)
        '      Call Scalar(ADJ, TENS(1, 3), D)
        For II = 1 To 3
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object ADJ(II, 1). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            ADJ(II, 1) = W(II)
            'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(II, 3). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
            V(II) = TENS(II, 3)
        Next II
        Call Scalar(W, V, D) ' W is result from VECTOR.
        '
        '*** SOLVE CUBIC, -P**3 + B*P**2 - C*P + D = 0 BY TRIGONOMETRICAL METHOD.
        '
        Del = B / 3.0#
        c = c + B * Del + 1.0E-35
        '      IF(C.LT.0.0) C = -C
        If (c < 0.0#) Then c = -c
        D = D + Del * (c - Del * Del)
        '      R = SQRT(0.75 / C)
        R = System.Math.Sqrt(0.75 / c)
        ARG = 3.0# * D * R / c
        '      IF(ARG.GT.ONE) ARG = ONE
        '      IF(ARG.LT.-ONE) ARG = -ONE
        If (ARG > ONE) Then ARG = ONE
        If (ARG < -ONE) Then ARG = -ONE
        '      Theta = ACOS(ARG) / 3#
        '      Arccos(X) = Atn(-X / Sqr(-X * X + 1)) + 2 * Atn(1) from VB manual.
        AlmostOne = 0.9999999999
        If ARG < -AlmostOne Then ARG = -AlmostOne
        If ARG > AlmostOne Then ARG = AlmostOne
        Theta = (System.Math.Atan(-ARG / System.Math.Sqrt(-ARG * ARG + 1)) + 2 * System.Math.Atan(1)) / 3
        '
        '*** PUT THE THREE ROOTS INTO DESCENDING ORDER OF MAGNITUDE.
        '
        '      DO 16 I = 1,3
        For I = 1 To 3
16:         PRIN(I) = System.Math.Cos(Theta + (I - 2) * PI * 2.0# / 3.0#) / R + Del
        Next I
        '      DO 20 I = 1,2
        '      DO 18 J = I,3
        '      IF(ABS(PRIN(J)).LE.ABS(PRIN(I))) GO TO 18
        For I = 1 To 2
            For J = I To 3
                If (System.Math.Abs(PRIN(J)) <= System.Math.Abs(PRIN(I))) Then GoTo 18
                Gash = PRIN(I)
                PRIN(I) = PRIN(J)
                PRIN(J) = Gash
18:             [Continue] = True
            Next J
20:         [Continue] = True
        Next I
        '
        '*** HAVING THE THREE EIGENVALUES, NOW FIND THE EIGENVECTORS.
        '
        '     DO 34 I = 1,3
        For I = 1 To 3
            Del = PRIN(1)
            '       IF(I.GT.1) DEL = PRIN(I) - PRIN(I-1)
            '       DO 22 J = 1,3
            If (I > 1) Then Del = PRIN(I) - PRIN(I - 1)
            '       DO 22 J = 1,3
            For J = 1 To 3
22:             'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(J, J). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                TENS(J, J) = TENS(J, J) - Del
            Next J
            '       DO 24 J = 1,3
            For J = 1 To 3
                K = J + 1
                '         IF(K.EQ.4) K = 1
                If (K = 4) Then K = 1
                '24        Call Vector(TENS(1, J), TENS(1, K), ADJ(1, 6 - J - K))
                For II = 1 To 3
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(II, J). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    U(II) = TENS(II, J)
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(II, K). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    V(II) = TENS(II, K)
                Next II
24:             Call Vector(U, V, W)
                For II = 1 To 3
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object ADJ(II, 6 - J - K). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    ADJ(II, 6 - J - K) = W(II)
                Next II
            Next J
            BIG = 0.0#
            '       DO 28 J = 1,3
            '       DO 26 K = 1,3
            '       IF(ABS(ADJ(J,K)).LT.BIG) GO TO 26
            For J = 1 To 3
                For K = 1 To 3
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object ADJ(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    If (System.Math.Abs(ADJ(J, K)) < BIG) Then GoTo 26
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object ADJ(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    BIG = System.Math.Abs(ADJ(J, K))
                    MAXCOL = K
26:                 [Continue] = True
                Next K
28:             [Continue] = True
            Next J
            '       DO 30 J = 1,3
            For J = 1 To 3
                DIREC(J, I) = 0.0#
                '         DO 30 K = 1,3
                For K = 1 To 3
30:                 'ik* UPGRADE_WARNING: Couldn't resolve default property of object ADJ(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    DIREC(J, I) = DIREC(J, I) + ADJ(J, K) * ADJ(K, MAXCOL) / (BIG * BIG)
                Next K
            Next J

            '       CALL SCALAR(DIREC(1,I), DIREC(1,I), SCALE) SCALE is reserved word in VB.
            '       DO 32 J = 1,3
            '       32 DIREC(J,I) = DIREC(J,I)/SQRT(SCALE)
            For II = 1 To 3
                U(II) = DIREC(II, I)
            Next II
            Call Scalar(U, U, SCALER)
            For J = 1 To 3
32:             DIREC(J, I) = DIREC(J, I) / System.Math.Sqrt(SCALER)
            Next J
            '       DO 34 J = 1,3
            '       DO 34 K = 1,3
            For J = 1 To 3
                For K = 1 To 3
34:                 'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    'ik* UPGRADE_WARNING: Couldn't resolve default property of object TENS(J, K). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                    TENS(J, K) = TENS(J, K) + PRIN(I) * DIREC(J, I) * DIREC(K, I)
                Next K
            Next J
        Next I
        '      Return
        '      End

    End Sub

    Private Sub Scalar(ByRef U() As Double, ByRef V() As Double, ByRef PROD As Double)

        '      SUBROUTINE Scalar(U, V, PROD) from FEMSKI
        '1616
        '*** TO COMPUTE SCALAR PRODUCT OF VECTORS U AND V.

        '      IMPLICIT DOUBLE PRECISION (A-H,O-Z)
        '      DIMENSION U(3), V(3)
        '      Dim U(1 To 3) As Double, V(1 To 3) As Double
        Dim I As Integer
        PROD = 0.0#
        '      DO 2 I = 1,3
        For I = 1 To 3
2:          PROD = PROD + U(I) * V(I)
        Next I
        '      Return
        '      End

    End Sub

    Private Sub SetOShiftsO(ByRef OShift(,) As Double, ByRef IL As Integer, ByRef ZLayer As Double, ByRef RMax As Double)

        ' IL = evaluation layer.
        Dim I As Integer
        Dim Factor, ZMin As Double
        Factor = 1
        ReDim OShift(NLayers, 2)

        '  ZMin = 1.5 * RMax / 3
        ZMin = 0.75 * RMax / 3
        '  ZMin = ZMin * 5

        OShift(IL, 1) = ZMin + ZLayer
        OShift(IL, 2) = ZMin - ZLayer
        '  OShift(IL, 2) = OShift(IL, 1)

        If IL <> 1 Then
            OShift(1, 1) = 2 * HLayer(1) + HLayer(0)
            OShift(1, 2) = 2 * HLayer(1) + HLayer(0)
        End If

        For I = 2 To IL - 1
            OShift(I, 1) = Factor * (OShift(I - 1, 2) + HLayer(I - 1))
            OShift(I, 2) = OShift(I, 1)
        Next I

        If IL > 1 Then
            If OShift(IL, 1) < Factor * (OShift(I - 1, 2) + HLayer(I - 1)) Then
                OShift(IL, 1) = Factor * (OShift(I - 1, 2) + HLayer(I - 1))
            End If
        End If

        If IL < NLayers Then
            For I = IL + 1 To NLayers
                OShift(I, 1) = Factor * (OShift(I - 1, 2) + HLayer(I - 1))
                OShift(I, 2) = OShift(I, 1)
            Next I
        Else
            If OShift(IL, 1) < Factor * (OShift(I - 1, 2) + HLayer(I - 1)) Then
                OShift(IL, 1) = Factor * (OShift(I - 1, 2) + HLayer(I - 1))
            End If
        End If

        '  OShift(NLayers, 1) = (ZMin + ZLayer) * 1
        '  OShift(NLayers, 2) = OShift(NLayers, 1) '(ZMin - ZLayer) * 1
        '  OShift(1, 1) = 2 * HLayer(1) + HLayer(0)
        '  OShift(1, 1) = 0   ' 8.904
        '  OShift(1, 2) = OShift(1, 1)

        For I = 1 To -NLayers
            System.Diagnostics.Debug.Write(I & OShift(I, 1))
            System.Diagnostics.Debug.WriteLine(OShift(I, 2) & RMax & ZLayer)
        Next I


    End Sub

    Private Sub SetOShifts(ByRef OShift(,) As Double, ByRef IL As Integer, ByRef ZLayer As Double, ByRef RMax As Double)

        ' IL = evaluation layer.
        Dim I As Integer
        Dim ZMin, Factor, Factor2 As Double
        Factor = 1
        Factor2 = 1
        ReDim OShift(NLayers, 2)

        ZMin = 0.75 * RMax / 3
        HLayer(0) = 0

        If IL <> 1 Then
            OShift(1, 1) = 2 * HLayer(1) + HLayer(0)
            OShift(1, 2) = (2 * HLayer(1) + HLayer(0)) * Factor2
        Else
            OShift(1, 1) = 2 + HLayer(0)
            OShift(1, 2) = (2 + HLayer(0)) * Factor2
        End If

        For I = 2 To NLayers
            OShift(I, 1) = Factor * (OShift(I - 1, 2) + HLayer(I - 1))
            OShift(I, 2) = OShift(I, 1) * Factor2
        Next I

        If OShift(IL, 1) < ZMin + ZLayer Then 'Or OShift(IL, 2) < ZMin - ZLayer Then
            OShift(IL, 1) = (ZMin + ZLayer) * 1.0#
            OShift(IL, 2) = (ZMin + ZLayer) * Factor2

            For I = IL - 1 To 1 Step -1
                OShift(I, 1) = Factor * (OShift(I + 1, 2) - HLayer(I))
                OShift(I, 2) = OShift(I, 1) * Factor2
            Next I

            For I = IL + 1 To NLayers
                OShift(I, 1) = Factor * (OShift(I - 1, 2) + HLayer(I - 1))
                OShift(I, 2) = OShift(I, 1) * Factor2
            Next I

        End If

    End Sub


    Private Sub Vector(ByRef U() As Double, ByRef V() As Double, ByRef W() As Double)

        '      SUBROUTINE Vector(U, V, W) from FEMSKI
        '1631
        '*** TO COMPUTE VECTOR PRODUCT U*V INTO AREA W.
        '
        '      IMPLICIT DOUBLE PRECISION (A-H,O-Z)
        '      DIMENSION U(3), V(3), W(3)
        '      Dim U(1 To 3) As Double, V(1 To 3) As Double, W(1 To 3) As Double
        Dim I, K As Integer
        K = 3
        '      DO 2 I = 1,3
        For I = 1 To 3
            W(6 - I - K) = U(K) * V(I) - U(I) * V(K)
2:          K = I
        Next I
        '      Return
        '      End

    End Sub

    'UPGRADE_NOTE: Class_Initialize was upgraded to Class_Initialize_Renamed. Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1061"'
    Private Sub Class_Initialize_Renamed()

        Dim GLNGaussRet, I As Integer
        Static RunOnce As Boolean
        '  SaveCaption$ = Caption
        '  Caption = "Computing Weights"
        GLNGauss = 500
        '  GLNGauss = 1500  ' GFH 3/8/02.
        '  GLNGauss = 2500  ' GFH 3/8/02.
        ReDim GLAlpha(GLNGauss)
        ReDim GLWeight(GLNGauss)
        If Not RunOnce Then
            GLNGaussRet = GLNGauss
            Call gaulag(GLAlpha, GLWeight, GLNGaussRet, 0)
            GLNGauss = GLNGaussRet
            RunOnce = True
        End If
        System.Diagnostics.Debug.WriteLine("GLNGauss = " & GLNGauss & GLNGaussRet & GLAlpha(GLNGaussRet) & GLWeight(GLNGaussRet))
        For I = 1 To -GLNGauss
            System.Diagnostics.Debug.WriteLine(I & GLAlpha(I) & GLWeight(I))
        Next I

        ConvergenceLimitDefault = 0.000001
        ConvergenceLimit = ConvergenceLimitDefault
        OverrideSolver = 0

    End Sub
    Public Sub New()
        MyBase.New()
        Class_Initialize_Renamed()
    End Sub

    Private Sub FindAllResponses(ByRef EvalLayer As Integer, ByRef ZEval As Double)

        Dim EL, I, IEval, IPad As Integer
        Dim IAC, NEvalPointsMax As Integer
        Dim Response(,) As Double
        Dim TempD, StrainG As Double
        'ik* Dim YYW, ZZW, XXW As Double
        'ik* Dim XZW, YZW, XYW As Double
        'ik* Dim XSW, ZSW, YSW As Double
        'ik* Dim OCTN, SMAX, OCTS As Double
        'ik* Dim XZSW, XYSW, YZSW As Double
        ' Used in Sub PRINPL, find principal stresses.
        Dim DIREC(3, 3) As Double
        Dim PRIN(3) As Double
        Dim SIG(6) As Double
        Dim DebugPrint As Boolean
        'ik* Dim NSTYL As Integer
        Dim Theta, Degree As Double
        Dim RespFmt As String
        Const PI As Double = 3.14159265358979

        DebugPrint = False

        If DebugPrint Then

            System.Diagnostics.Debug.WriteLine("     Layer No.  Thickness    ELasticity  Poisson's  Interface")
            System.Diagnostics.Debug.WriteLine("                              Modulus      Ratio    Condition")
            System.Diagnostics.Debug.WriteLine("     --------------------------------------------------------")

            For I = 1 To NLayers
                If I < NLayers Then TempD = HLayer(I) Else TempD = 0
                System.Diagnostics.Debug.Write(LPad(10, VB6.Format(I, "0")) & LPad(13, VB6.Format(TempD, "0.00")))
                System.Diagnostics.Debug.Write(LPad(16, VB6.Format(Youngs(I), "#,###,###.0")))
                System.Diagnostics.Debug.Write(LPad(9, VB6.Format(Poissons(I), "0.000")))
                System.Diagnostics.Debug.WriteLine(LPad(11, VB6.Format(InterfaceParm(I), "0.000")))
            Next I
            System.Diagnostics.Debug.WriteLine("")
            System.Diagnostics.Debug.WriteLine("")

            For IAC = 1 To NAC

                System.Diagnostics.Debug.WriteLine("          Aircraft No. " & VB6.Format(IAC, "0") & "  " & ACname(IAC))
                System.Diagnostics.Debug.WriteLine("          Aircraft design load         :" & " Not Applicable") ' LPad(Format(GL(IAC), "#,###,##0.00"))
                System.Diagnostics.Debug.WriteLine("          Fraction of load on main gear:" & LPad(12, VB6.Format(100, "0.0"))) 'libMGpcnt(LI) * 100, "0.0"))
                System.Diagnostics.Debug.WriteLine("          Gear load                    :" & LPad(12, VB6.Format(GearLoad(IAC), "#,###,##0.0")))
                System.Diagnostics.Debug.WriteLine("          Number of tires              :" & LPad(10, VB6.Format(NTires(IAC), "0")))
                System.Diagnostics.Debug.WriteLine("")
                System.Diagnostics.Debug.WriteLine(" Tire  Radius  Cont.Area  Cont.Press   Tire Load    X-Coord   Y-Coord")
                System.Diagnostics.Debug.WriteLine(" No.    (in)    (sq.in)     (psi)       (pounds)      (in)      (in.)")
                System.Diagnostics.Debug.WriteLine(" --------------------------------------------------------------------")

                For I = 1 To NTires(IAC)
                    System.Diagnostics.Debug.Write(LPad(3, VB6.Format(I, "0")) & LPad(9, VB6.Format(TireRadius(IAC, I), "0.00")))
                    System.Diagnostics.Debug.Write(LPad(11, VB6.Format(PI * TireRadius(IAC, I) ^ 2, "0.00")))
                    System.Diagnostics.Debug.Write(LPad(11, VB6.Format(TirePress(IAC, I), "0.00")))
                    System.Diagnostics.Debug.Write(LPad(14, VB6.Format(WheelLoad(IAC, I), "#,##0.00")))
                    System.Diagnostics.Debug.Write(LPad(10, VB6.Format(TireX(IAC, I), "0.00")))
                    System.Diagnostics.Debug.WriteLine(LPad(10, VB6.Format(TireY(IAC, I), "0.00")))
                Next I
                System.Diagnostics.Debug.WriteLine("")

            Next IAC

        End If

        FindingAllResponses = True ' Used in integration routines.
        EL = EvalLayer

        NEvalPointsMax = 0
        For IAC = 1 To NAC
            If NEvalPoints(IAC) > NEvalPointsMax Then
                NEvalPointsMax = NEvalPoints(IAC)
            End If
        Next IAC

        StrainG = Youngs(EL) / (2 * (1 + Poissons(EL)))

        Call IntegrateZDeflection(EL, ZEval, Response)
        Call IntegrateZStrain(EL, ZEval, Response)
        Call IntegrateHorizontalStress(EL, ZEval, Response)

        Dim Resp3(,,) As Double 'ikawa 
        'Call IntegrateHorizontalDeflection(EL, ZEval, Response)
        Call IntegrateHorizontalDeflection(EL, ZEval, Resp3) 'ikawa

        RespFmt = "0.00000E+00"
        IPad = Len(RespFmt) + 2

        For IAC = 1 To NAC
            For IEval = 1 To NEvalPoints(IAC)
                With AllResp(IAC, IEval)
                    TempD = .StrainZ * Youngs(EL)
                    .StressZ = TempD + (.StressX + .StressY) * Poissons(EL)
                    TempD = Poissons(EL) * (.StressY + .StressZ)
                    .StrainX = (.StressX - TempD) / Youngs(EL)
                    TempD = Poissons(EL) * (.StressX + .StressZ)
                    .StrainY = (.StressY - TempD) / Youngs(EL)
                    .StrainXY = .StressXY / StrainG
                    .StrainXZ = .StressXZ / StrainG
                    .StrainYZ = .StressYZ / StrainG

                    SIG(1) = .StressX
                    SIG(2) = .StressY
                    SIG(3) = .StressZ
                    SIG(4) = .StressYZ
                    SIG(5) = .StressXZ
                    SIG(6) = .StressXY
                    Call Prinpl(3, Theta, Degree, DIREC, PRIN, SIG)
                    .StressPrin1 = PRIN(1)
                    .StressPrin2 = PRIN(2)
                    .StressPrin3 = PRIN(3)
                    .StrainPrin1 = (PRIN(1) - Poissons(EL) * (PRIN(2) + PRIN(3))) / Youngs(EL)
                    .StrainPrin2 = (PRIN(2) - Poissons(EL) * (PRIN(1) + PRIN(3))) / Youngs(EL)
                    .StrainPrin3 = (PRIN(3) - Poissons(EL) * (PRIN(1) + PRIN(2))) / Youngs(EL)
                    .StressMaxShear = (PRIN(1) - PRIN(3)) / 2.0#
                    .StressOctNormal = (PRIN(1) + PRIN(2) + PRIN(3)) / 3
                    '        OCTS=(((ZZW-YYW)**2+(YYW-XXW)**2+(XXW-ZZW)**2)**.5)/3
                    .StressOctShear = (((PRIN(1) - PRIN(2)) ^ 2 + (PRIN(2) - PRIN(3)) ^ 2 + (PRIN(3) - PRIN(1)) ^ 2) ^ 0.5) / 3

                    If DebugPrint Then

                        System.Diagnostics.Debug.Write(" Eval Point = " & LPad(4, VB6.Format(IEval, "0")) & "      ")
                        System.Diagnostics.Debug.WriteLine(" Layer No. = " & LPad(4, VB6.Format(EvalLayer, "0")))
                        System.Diagnostics.Debug.Write(" X-Coord.   = " & LPad(8, VB6.Format(EvalX(IAC, IEval), "0.000")) & "  ")
                        System.Diagnostics.Debug.Write(" Y-Coord.  = " & LPad(8, VB6.Format(EvalY(IAC, IEval), "0.000")) & "  ")
                        System.Diagnostics.Debug.WriteLine(" Z-Depth = " & LPad(8, VB6.Format(ZEval, "0.000")))
                        System.Diagnostics.Debug.WriteLine("")

                        I = IPad - 9
                        System.Diagnostics.Debug.WriteLine("         VERT STR" & Space(I + 1) & "HOR Y STR" & Space(I) & "HOR X STR" & Space(I) & "XZ SHEAR" & Space(I + 1) & "YZ SHEAR" & Space(I + 1) & "XY SHEAR")

                        If IEval >= 1 Then
                            System.Diagnostics.Debug.Write("Stress")
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressZ, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressY, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressX, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressXZ, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressYZ, RespFmt)))
                            System.Diagnostics.Debug.WriteLine(LPad(IPad, VB6.Format(.StressXY, RespFmt)))
                            System.Diagnostics.Debug.Write("Strain")
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StrainZ, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StrainY, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StrainX, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StrainXZ, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StrainYZ, RespFmt)))
                            System.Diagnostics.Debug.WriteLine(LPad(IPad, VB6.Format(.StrainXY, RespFmt)))
                            System.Diagnostics.Debug.Write("Displt")
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.DeflZ, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.DeflY, RespFmt)))
                            System.Diagnostics.Debug.WriteLine(LPad(IPad, VB6.Format(.DeflX, RespFmt)))
                            System.Diagnostics.Debug.WriteLine("")
                            System.Diagnostics.Debug.WriteLine("         PRIN 1" & Space(I + 3) & "PRIN 2" & Space(I + 3) & "PRIN 3" & Space(I + 2) & "MAX SHEAR" & Space(I) & "OCT NORMAL" & Space(I - 1) & "OCT SHEAR")
                            System.Diagnostics.Debug.Write("Stress")
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressPrin1, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressPrin2, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressPrin3, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressMaxShear, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StressOctNormal, RespFmt)))
                            System.Diagnostics.Debug.WriteLine(LPad(IPad, VB6.Format(.StressOctShear, RespFmt)))
                            System.Diagnostics.Debug.Write("Strain")
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StrainPrin1, RespFmt)))
                            System.Diagnostics.Debug.Write(LPad(IPad, VB6.Format(.StrainPrin2, RespFmt)))
                            System.Diagnostics.Debug.WriteLine(LPad(IPad, VB6.Format(.StrainPrin3, RespFmt)))
                            System.Diagnostics.Debug.WriteLine("")
                        End If

                    End If

                End With
            Next IEval
        Next IAC

        FindingAllResponses = False

    End Sub

    Private Sub FindConstantsPartInvert_modified(ByVal ALPHA As Double, ByVal B() As Double, ByVal OShift(,) As Double, ByVal BadCondition As Long)
        'modified

        ' Part inversion of full matrix. Much faster than standard solvers, but no pivoting.
        ' The subroutine works correctly with only one layer (Boussinesq half-space).
        ' Make DummyTop = False in Sub ComputeResponse to get a single layer.

        Dim I As Integer, II As Integer, J As Integer, JJ As Integer, Jm2 As Long
        Dim K As Integer, N As Integer, IFail As Long, KK As Integer
        Dim A(,) As Double, R As Double
        Dim Exp1 As Double, Exp2 As Double
        Dim Exp3 As Double, Exp4 As Double
        Dim Fact1 As Double, Fact2 As Double
        Dim AlphaZ1 As Double, AlphaG As Double
        Dim A11(,) As Double, ATI1(,) As Double, A11A12(,) As Double
        Dim AStar11 As Double, AStar12 As Double, AStar22 As Double, AStar21 As Double
        Dim IParm As Double
        Dim DTemp As Double, Tiny As Double, ResidKKp1 As Double, ResidKKp2 As Double

        On Error GoTo FindConstantsPartInvertError

        BadCondition = 0

        FileOpen(4, "TTT.TXT", OpenMode.Output, , , 1024)
        PrintLine(4, "FEDFAA")


        I = 4 * NLayers
        ReDim A(I, I), A11(I, I), B(I)
        ReDim ATI1(4, 4)
        ReDim A11A12(I, 2)

        Exp1 = Math.Exp(-ALPHA * OShift(1, 1))
        Exp2 = Math.Exp(-ALPHA * OShift(1, 2))


        PrintLine(4, "Exp1= " & Exp1)
        PrintLine(4, "Exp2= " & Exp2)


        ' Surface vertical stress.
        A(1, 1) = Exp1 : A(1, 2) = -Exp2
        A(1, 3) = -(1 - 2 * Poissons(1)) * Exp1
        A(1, 4) = -(1 - 2 * Poissons(1)) * Exp2


        PrintLine(4, "A(1, 1)= " & A(1, 1))
        PrintLine(4, "A(1, 3)= " & A(1, 3))
        PrintLine(4, "A(1, 4)= " & A(1, 4))

        ' Surface shear stress.
        A(2, 1) = Exp1 : A(2, 2) = Exp2
        A(2, 3) = 2 * Poissons(1) * Exp1
        A(2, 4) = -2 * Poissons(1) * Exp2

        PrintLine(4, "A(2, 1)= " & A(2, 1))
        PrintLine(4, "A(2, 3)= " & A(2, 3))
        PrintLine(4, "A(2, 4)= " & A(2, 4))
        PrintLine(4, "")

        For I = 1 To NLayers - 1

            Exp1 = Math.Exp(-ALPHA * (-HLayer(I) + OShift(I, 1)))
            Exp2 = Math.Exp(-ALPHA * (HLayer(I) + OShift(I, 2)))
            Exp3 = Math.Exp(-ALPHA * (OShift(I + 1, 1)))
            Exp4 = Math.Exp(-ALPHA * (OShift(I + 1, 2)))
            AlphaZ1 = ALPHA * HLayer(I)

            K = (I - 1) * 4

            PrintLine(4, "Exp1= " & Exp1)
            PrintLine(4, "Exp2= " & Exp2)
            PrintLine(4, "Exp3= " & Exp3)
            PrintLine(4, "Exp4= " & Exp4)
            PrintLine(4, "AlphaZ1= " & AlphaZ1)


            '   Fill in the lower layer matrix elements first.
            J = I * 4 - 1
            '   Vertical stress.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = -Exp2
            '    A(J, K + 3) = -(1 - 2 * Poissons(I) - AlphaZ1) * Exp1
            '    A(J, K + 4) = -(1 - 2 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = Exp4
            A(J, K + 7) = (1 - 2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (1 - 2 * Poissons(I + 1)) * Exp4



            PrintLine(4, "A(J, K + 5)= " & A(J, K + 5))
            PrintLine(4, "A(J, K + 7)= " & A(J, K + 7))
            PrintLine(4, "A(J, K + 8)= " & A(J, K + 8))

            PrintLine(4, "J= " & J)

            J = J + 1
            '   Shear stress.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = Exp2
            '    A(J, K + 3) = (2 * Poissons(I) + AlphaZ1) * Exp1
            '    A(J, K + 4) = -(2 * Poissons(I) - AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 : A(J, K + 6) = -Exp4
            A(J, K + 7) = -(2 * Poissons(I + 1)) * Exp3
            A(J, K + 8) = (2 * Poissons(I + 1)) * Exp4


            PrintLine(4, "A(J, K + 5)= " & A(J, K + 5))
            PrintLine(4, "A(J, K + 7)= " & A(J, K + 7))
            PrintLine(4, "A(J, K + 8)= " & A(J, K + 8))



            R = (1 + Poissons(I + 1)) * Youngs(I) / (Youngs(I + 1) * (1 + Poissons(I)))
            PrintLine(4, "R= " & R)
            J = J + 1



            '   Vertical displacement.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = Exp2
            '    A(J, K + 3) = -(2 - 4 * Poissons(I) - AlphaZ1) * Exp1
            '    A(J, K + 4) = (2 - 4 * Poissons(I) + AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 * R : A(J, K + 6) = -Exp4 * R
            A(J, K + 7) = (2 - 4 * Poissons(I + 1)) * Exp3 * R
            A(J, K + 8) = -(2 - 4 * Poissons(I + 1)) * Exp4 * R

            PrintLine(4, "A(J, K + 5)= " & A(J, K + 5))
            PrintLine(4, "A(J, K + 7)= " & A(J, K + 7))
            PrintLine(4, "A(J, K + 8)= " & A(J, K + 8))




            J = J + 1
            '   Radial displacement.
            '    A(J, K + 1) = Exp1:  A(J, K + 2) = -Exp2
            '    A(J, K + 3) = (1 + AlphaZ1) * Exp1
            '    A(J, K + 4) = (1 - AlphaZ1) * Exp2
            A(J, K + 5) = -Exp3 * R : A(J, K + 6) = Exp4 * R
            A(J, K + 7) = -Exp3 * R
            A(J, K + 8) = -Exp4 * R

            PrintLine(4, "A(J, K + 5)= " & A(J, K + 5))
            PrintLine(4, "A(J, K + 7)= " & A(J, K + 7))
            PrintLine(4, "A(J, K + 8)= " & A(J, K + 8))





            '   Combine shear stress and radial displacement for interface shear.
            '   Small values of InterfaceParm, unbonded, cause numerical problems.
            '   Use Sub FindConstantsFull instead. See Sub ComputeResponse.

            IParm = 0.001 ^ 4
            If InterfaceParm(I) > IParm Then IParm = InterfaceParm(I)

            Fact1 = -ALPHA * (1 - IParm) * Youngs(I) / (1 + Poissons(I))
            Fact2 = IParm

            PrintLine(4, "Fact1= " & Fact1)
            PrintLine(4, "Fact2= " & Fact2)

            Jm2 = J - 2   ' Reference shear stress equation.
            '    A(J, K + 1) = A(Jm2, K + 1) * Fact1 - A(J, K + 1) * Fact2
            '    A(J, K + 2) = A(Jm2, K + 2) * Fact1 - A(J, K + 2) * Fact2
            '    A(J, K + 3) = A(Jm2, K + 3) * Fact1 - A(J, K + 3) * Fact2
            '    A(J, K + 4) = A(Jm2, K + 4) * Fact1 - A(J, K + 4) * Fact2
            A(J, K + 5) = -A(J, K + 5) * Fact2
            A(J, K + 6) = -A(J, K + 6) * Fact2
            A(J, K + 7) = -A(J, K + 7) * Fact2
            A(J, K + 8) = -A(J, K + 8) * Fact2


            PrintLine(4, "A(J, K + 5)= " & A(J, K + 5))
            PrintLine(4, "A(J, K + 6)= " & A(J, K + 6))
            PrintLine(4, "A(J, K + 7)= " & A(J, K + 7))
            PrintLine(4, "A(J, K + 8)= " & A(J, K + 8))



            '   Put the inverse of the upper layer matrix into A().
            '   Multiplying a column / row in a matrix is eqivalent to
            '   dividing the same number row / column in its inverse.
            '   Used below to apply Exp1 and Exp2, i.e. divide rows in inverse.

            '   See above for IParm = InterfaceParm(I).
            AlphaG = -ALPHA * (1 - IParm) * Youngs(I) / (1 + Poissons(I))
            Fact1 = 1 / (4 * IParm * (1 - Poissons(I)))
            Fact2 = Fact1 / Exp2
            Fact1 = Fact1 / Exp1

            PrintLine(4, "AlphaG= " & AlphaG)
            PrintLine(4, "Fact1= " & Fact1)
            PrintLine(4, "Fact2= " & Fact2)



            A(J - 3, K + 1) = IParm * (1 + AlphaZ1) * Fact1
            A(J - 3, K + 2) = (AlphaG * (1 - 2 * Poissons(I) - AlphaZ1) _
                            + IParm * (2 - 4 * Poissons(I) - AlphaZ1)) * Fact1
            A(J - 3, K + 3) = IParm * (2 * Poissons(I) + AlphaZ1) * Fact1
            A(J - 3, K + 4) = -(1 - 2 * Poissons(I) - AlphaZ1) * Fact1
            A(J - 2, K + 1) = -IParm * (1 - AlphaZ1) * Fact2
            A(J - 2, K + 2) = (-AlphaG * (1 - 2 * Poissons(I) + AlphaZ1) _
                            + IParm * (2 - 4 * Poissons(I) + AlphaZ1)) * Fact2
            A(J - 2, K + 3) = IParm * (2 * Poissons(I) - AlphaZ1) * Fact2
            A(J - 2, K + 4) = (1 - 2 * Poissons(I) + AlphaZ1) * Fact2
            A(J - 1, K + 1) = -IParm * Fact1
            A(J - 1, K + 2) = (AlphaG + IParm) * Fact1
            A(J - 1, K + 3) = -IParm * Fact1
            A(J - 1, K + 4) = -Fact1
            A(J, K + 1) = -IParm * Fact2
            A(J, K + 2) = (AlphaG - IParm) * Fact2
            A(J, K + 3) = IParm * Fact2
            A(J, K + 4) = -Fact2


            PrintLine(4, "A(J - 3, K + 1)= " & A(J - 3, K + 1))
            PrintLine(4, "A(J - 3, K + 2)= " & A(J - 3, K + 2))
            PrintLine(4, "A(J - 3, K + 3)= " & A(J - 3, K + 3))
            PrintLine(4, "A(J - 3, K + 4)= " & A(J - 3, K + 4))

            PrintLine(4, "A(J - 2, K + 1)= " & A(J - 2, K + 1))
            PrintLine(4, "A(J - 2, K + 2)= " & A(J - 2, K + 2))
            PrintLine(4, "A(J - 2, K + 3)= " & A(J - 2, K + 3))
            PrintLine(4, "A(J - 2, K + 4)= " & A(J - 2, K + 4))

            PrintLine(4, "A(J - 1, K + 1)= " & A(J - 1, K + 1))
            PrintLine(4, "A(J - 1, K + 2)= " & A(J - 1, K + 2))
            PrintLine(4, "A(J - 1, K + 3)= " & A(J - 1, K + 3))
            PrintLine(4, "A(J - 1, K + 4)= " & A(J - 1, K + 4))

            PrintLine(4, "A(J, K + 1)= " & A(J, K + 1))
            PrintLine(4, "A(J, K + 2)= " & A(J, K + 2))
            PrintLine(4, "A(J, K + 3)= " & A(J, K + 3))
            PrintLine(4, "A(J, K + 4)= " & A(J, K + 4))

            PrintLine(4, "")

            '   Post multiply the inverse of the upper layer matrix
            '   by the lower layer matrix.

            For II = 1 To 4
                N = J - 4 + II
                For JJ = 1 To 4
                    KK = K + JJ + 4
                    ATI1(II, JJ) = A(N, K + 1) * A(J - 3, KK) _
                                 + A(N, K + 2) * A(J - 2, KK) _
                                 + A(N, K + 3) * A(J - 1, KK) _
                                 + A(N, K + 4) * A(J - 0, KK)
                    PrintLine(4, "ATI1(" & II & "," & JJ & ")= " & ATI1(II, JJ))
                Next JJ
            Next II

            '   Refill A() with the reduced elements.
            For II = 1 To 4
                N = J - 4 + II
                A(N, K + 1) = 0.0# ' Fill out the matrix to be complete for printing.
                A(N, K + 2) = 0.0# ' Not needed for computation.
                A(N, K + 3) = 0.0#
                A(N, K + 4) = 0.0#
                A(N, K + 5) = ATI1(II, 1) ' These are needed for computation.
                A(N, K + 6) = ATI1(II, 2)
                A(N, K + 7) = ATI1(II, 3)
                A(N, K + 8) = ATI1(II, 4)
            Next II
            N = J - 4
            A(N + 1, K + 1) = 1.0# ' Fill out the matrix to be complete for printing.
            A(N + 2, K + 2) = 1.0# ' Not needed for computation.
            A(N + 3, K + 3) = 1.0#
            A(N + 4, K + 4) = 1.0#

        Next I

        ' Reduce from an (NLayers * 4) by (NLayers * 4) system to
        ' an (NLayers * 4 - 2) by (NLayers * 4 - 2) system.
        ' This satisfies the infinite subgrade with A and C zero
        ' for the subgrade layer. Return (NLayers * 4) coefficients
        ' by moving returned coefficients and inserting zeros (see below).
        K = 4 * NLayers - 2
        If NLayers > 1 Then
            For I = K - 3 To K
                A(I, K - 1) = A(I, K)
                A(I, K) = A(I, K + 2)
            Next I
        Else
            '   Special case for one layer. A1 = C1 = 0.
            '   Surface boundary 2x4 matrix set at top of subroutine.
            A(1, 1) = A(1, 2)
            A(1, 2) = A(1, 4)
            A(2, 1) = A(2, 2)
            A(2, 2) = A(2, 4)
        End If

        If 0.1 < ALPHA And ALPHA < 0.101 Then
            '    Debug.Print "Alpha = "; Alpha
            For I = 1 To -K
                For J = 1 To K
                    'Debug.Print LPad(10, Format(A(I, J), "0.00E+00"));
                Next J
                'Debug.Print()
            Next I
        End If

        ' Working array for triangular matrix. Could save some storage here.
        KK = K - 2
        For I = 1 To KK
            For J = 1 To KK
                A11(I, J) = A(I + 2, J)
            Next J
        Next I

        For I = 1 To -KK
            For J = 1 To KK
                'Debug.Print(LPad(10, Format(A11(I, J), "0.00E+00")))
            Next J
            '            Debug.Print()
        Next I

        If NLayers > 1 Then ' Stop subscript going out of range.
            For I = K - 3 To K
                A11A12(I - 2, 1) = -A(I, K - 1) ' Really A12. Saving storage and a variable.
                A11A12(I - 2, 2) = -A(I, K)     ' Really A12
            Next I
        End If

        For I = 1 To -KK
            For J = 1 To 2
                'Debug.Print(LPad(10, Format(A11A12(I, J), "0.00E+00")))
            Next J
            'Debug.Print()
        Next I

        ' Inner loop limits for back substitution ignore zeroes and only index
        ' the ATR matrix (lower layer pre-multiplied by upper layer inverse).
        ' I = 1 is upper left of A11 matrix.

        '   II       I     Jlower    Jupper   (I-1) mod 4    I - ((I-1) mod 4) + 4
        '                 no zeros
        ' KK - 0     1       5         8           0                  5
        ' KK - 1     2       5         8           1                  5
        ' KK - 2     3       5         8           2                  5
        ' KK - 3     4       5         8           3                  5
        ' KK - 4     5       9        12           0                  9
        ' KK - 5     6       9        12           1                  9
        ' KK - 6     7       9        12           2                  9

        ' Back substitution. See Numerical methods by B. Irons for the prototype.
        For I = KK To 1 Step -1
            If I <= KK - 4 Then             ' Don't need to do last matrix.
                JJ = I - ((I - 1) Mod 4) + 4
                A11A12(I, 1) = -A11(I, JJ) * A11A12(JJ, 1) _
                              - A11(I, JJ + 1) * A11A12(JJ + 1, 1) _
                              - A11(I, JJ + 2) * A11A12(JJ + 2, 1) _
                              - A11(I, JJ + 3) * A11A12(JJ + 3, 1)
                A11A12(I, 2) = -A11(I, JJ) * A11A12(JJ, 2) _
                              - A11(I, JJ + 1) * A11A12(JJ + 1, 2) _
                              - A11(I, JJ + 2) * A11A12(JJ + 2, 2) _
                              - A11(I, JJ + 3) * A11A12(JJ + 3, 2)


                PrintLine(4, "A11A12(I, 1)= " & A11A12(I, 1))
                PrintLine(4, "A11A12(I, 2)= " & A11A12(I, 2))
            End If

        Next I

        For I = 1 To -KK
            For J = 1 To 2
                'Debug.Print LPad(10, Format(A11A12(I, J), "0.00E+00"));
            Next J
            'Debug.Print()
        Next I

        ' Form A22 (called AStar).
        If NLayers > 1 Then
            For I = 1 To 4
                AStar11 = AStar11 + A(1, I) * A11A12(I, 1)
                AStar12 = AStar12 + A(1, I) * A11A12(I, 2)
                AStar21 = AStar21 + A(2, I) * A11A12(I, 1)
                AStar22 = AStar22 + A(2, I) * A11A12(I, 2)

                PrintLine(4, "AStar11= " & AStar11)
                PrintLine(4, "AStar12= " & AStar12)
                PrintLine(4, "AStar21= " & AStar21)
                PrintLine(4, "AStar22= " & AStar22)
                PrintLine(4, "")
            Next I
        Else
            '   Special case for one layer. See above.
            AStar11 = A(1, 1)
            AStar12 = A(1, 2)
            AStar21 = A(2, 1)
            AStar22 = A(2, 2)

            PrintLine(4, "AStar11= " & AStar11)
            PrintLine(4, "AStar12= " & AStar12)
            PrintLine(4, "AStar21= " & AStar21)
            PrintLine(4, "AStar22= " & AStar22)
            PrintLine(4, "")

        End If

        ' Solve for the last two coefficients.
        '  B(KK + 1) = 1 / (AStar11 - AStar12 * (AStar21 / AStar22))  ' GFH 3/8/02.
        Tiny = 1.0E-24
        Tiny = 0.000000001       ' GFH 04/03/03. Changed.
        DTemp = (AStar11 - AStar12 * (AStar21 / AStar22))
        If DTemp = 0 Then DTemp = Tiny
        If Math.Abs(DTemp) <= Tiny Then
            B(KK + 1) = 0
        Else
            B(KK + 1) = 1 / DTemp
        End If

        B(KK + 2) = -B(KK + 1) * AStar21 / AStar22

        ResidKKp1 = AStar11 * B(KK + 1) + AStar12 * B(KK + 2) - 1
        ResidKKp2 = AStar21 * B(KK + 1) + AStar22 * B(KK + 2)


        PrintLine(4, "DTemp= " & DTemp)
        PrintLine(4, "B(KK + 1)= " & B(KK + 1))
        PrintLine(4, "B(KK + 2)= " & B(KK + 2))
        PrintLine(4, "ResidKKp1= " & ResidKKp1)
        PrintLine(4, "ResidKKp2= " & ResidKKp2)


        If Math.Sqrt(ResidKKp1 * ResidKKp1 + ResidKKp2 * ResidKKp2) / 2 > 0.0001 Then
            BadCondition = -1
        End If

        'ikawa Jan06 2
        If DesignType = "NewRigid" Or DesignType = "UnbondOnRigid" _
        Or DesignType = "PartBondOnRigid" Then
            BadCondition = -1 'ikawa 11/17/03
        End If


        PrintLine(4, "")
        ' Solve for the remaining coefficients.
        For I = 1 To KK
            B(I) = A11A12(I, 1) * B(KK + 1) + A11A12(I, 2) * B(KK + 2)
            PrintLine(4, "B(I)= " & B(I))
        Next I

        If 0.1 < ALPHA And ALPHA < 0.101 Then
            '    Debug.Print "Alpha = "; Alpha; "IFail = "; IFail
            For I = 1 To -K
                'Debug.Print(LPad(10, Format(B(I), "0.00E+00")))
            Next I
            '  Debug.Print
        End If

        ' Put back the full array of coefficients (Alast and Clast are zero for infinite subgrade).
        B(K + 2) = B(K)
        B(K + 1) = 0
        B(K) = B(K - 1)
        B(K - 1) = 0


        FileClose(4)
        Exit Sub

FindConstantsPartInvertError:

        BadCondition = -1

    End Sub




End Class
