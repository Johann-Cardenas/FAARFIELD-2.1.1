Option Explicit On

''' <summary>
''' Data collection classes for the CM Report (Computational Mechanics).
''' Captures intermediate computational values during LEAF design,
''' CDF computation, and ACR/PCR analysis for reporting.
''' </summary>
Public Class clsDetailedReportData

    Public AircraftDetails() As clsAircraftDetail
    Public Iterations As New List(Of clsIterationRecord)
    Public CDFSweep As New clsCDFSweepData
    Public SublayerData As New clsSublayerData
    Public ACRDetails As New List(Of clsACRDetail)
    Public PCRRounds As New List(Of clsPCRRound)
    Public IsPopulated As Boolean = False

    ' Asphalt CDF data (section-level)
    Public AsphaltCDFTotal As Single            ' CDFAsp — total asphalt CDF for the section
    Public AsphaltCDFComputed As Boolean = False ' Whether asphalt CDF was actually computed
    Public AsphaltModel As String = "N/A"       ' "RDEC" or "AI"
    Public RdecFlexuralMod As Single            ' Flexural modulus (psi)
    Public RdecAirVoids As Single               ' Air voids (%)
    Public RdecAsphaltContent As Single         ' Asphalt content by volume (%)
    Public RdecVoidParameter As Single          ' Voids / (Voids + AsphaltContent)
    Public RdecPNMS As Single                   ' Nominal max sieve passing (%)
    Public RdecPPCS As Single                   ' P-200 coarse (%)
    Public RdecP200 As Single                   ' Fraction passing #200 sieve (%)
    Public RdecGradationParameter As Single     ' (PNMS - PPCS) / P200

    Public Sub Clear()
        AircraftDetails = Nothing
        Iterations.Clear()
        CDFSweep = New clsCDFSweepData
        SublayerData = New clsSublayerData
        ACRDetails.Clear()
        PCRRounds.Clear()
        IsPopulated = False
    End Sub

End Class


Public Class clsAircraftDetail

    Public ACName As String
    Public GearType As String
    Public GrossLoad As Single
    Public TireWidth As Single
    Public TandemSpacing As Single
    Public ContactArea As Single
    Public TirePressure As Single
    Public AnnualDepartures As Single
    Public TotalRepetitions As Double
    Public ProjectedTireWidthAtSubgrade As Double
    Public VerticalStrain As Double
    Public HorizontalStrain As Double
    Public NtoFail As Double
    Public MaxCDF As Double
    Public CDFAtCriticalOffset As Double
    Public MaxCtoP As Single
    Public SubgradeModelUsed As String
    Public AsphaltModelUsed As String
    Public NtoFailAA As Double
    Public NtoFailBB As Double
    Public StrainBreakpoint As Double
    Public NGearLoads As Integer
    Public GearAdjusted As Boolean
    Public CtoPBeforeGearAdj As Single
    Public CtoPAfterGearAdj As Single
    Public CDFByOffset(CDF.NOFF) As Double
    Public CtoPByOffset(CDF.NOFF) As Single

    ' Gear geometry for visualization (1-indexed arrays)
    Public WheelX() As Single       ' libTX — lateral X position of each wheel (inches)
    Public WheelY() As Single       ' libTY — longitudinal Y position of each wheel (inches)
    Public NWheels As Integer        ' libNTires — number of tires
    Public DualSpacing As Single     ' libB — dual wheel spacing (inches)
    Public GearSpacing As Single     ' libTG — gear spacing (inches)

    ' Asphalt CDF per aircraft
    Public AsphaltCDF As Double         ' HMA layer CDF for this aircraft
    Public AsphaltNtoFail As Double     ' N_fail for HMA fatigue (RDEC or AI)
    Public AsphaltStrain As Double      ' Horizontal tensile strain in HMA layer

End Class


Public Class clsIterationRecord

    Public IterationNumber As Integer
    Public Thickness As Single
    Public CDFMAX As Single
    Public CDFErr As Single
    Public DELT As Single
    Public Factor As Double
    Public SubLayered As Boolean

End Class


Public Class clsCDFSweepData

    Public CDFPerAircraftPerOffset(,) As Double
    Public CDFTotalPerOffset() As Single
    Public CtoPPerAircraftPerOffset(,) As Single
    Public MaxCDFOffset As Integer
    Public MaxCDF As Single
    Public NAircraftCaptured As Integer

    Public Sub Capture(nac As Short, noff As Short, lclCDF(,) As Double, CDFFlexVal() As Single, CtoP(,) As Single, iControl As Short)
        NAircraftCaptured = nac
        ReDim CDFPerAircraftPerOffset(nac, noff)
        ReDim CDFTotalPerOffset(noff)
        ReDim CtoPPerAircraftPerOffset(nac, noff)
        MaxCDFOffset = iControl

        For ia As Integer = 1 To nac
            For ioff As Integer = 1 To noff
                CDFPerAircraftPerOffset(ia, ioff) = lclCDF(ia, ioff)
                CtoPPerAircraftPerOffset(ia, ioff) = CtoP(ia, ioff)
            Next
        Next
        For ioff As Integer = 1 To noff
            CDFTotalPerOffset(ioff) = CDFFlexVal(ioff)
        Next

        MaxCDF = 0
        For ioff As Integer = 1 To noff
            If CDFTotalPerOffset(ioff) > MaxCDF Then
                MaxCDF = CDFTotalPerOffset(ioff)
            End If
        Next
    End Sub

End Class


Public Class clsSublayerData

    Public DesignLayers As New List(Of clsLayerInfo)
    Public ExpandedSublayers As New List(Of clsLayerInfo)
    Public EvalDepthSubgrade As Double

    ' Aggregate sublayering parameters (captured from FAAModulusThick)
    Public HasAggregateSublayers As Boolean = False
    Public BaseCoeffC As Single         ' C coefficient for P-209 base
    Public BaseCoeffD As Single         ' D coefficient for P-209 base
    Public SubbaseCoeffC As Single      ' C coefficient for P-154 subbase
    Public SubbaseCoeffD As Single      ' D coefficient for P-154 subbase
    Public BaseModUnder As Single       ' Modulus of layer below aggregate base (psi)
    Public SubbaseModUnder As Single    ' Modulus of layer below aggregate subbase (psi)
    Public BaseSublayerCount As Integer ' Number of sublayers in base
    Public SubbaseSublayerCount As Integer ' Number of sublayers in subbase
    Public BaseSublayers As New List(Of clsLayerInfo)    ' Individual base sublayers (bottom-up computed)
    Public SubbaseSublayers As New List(Of clsLayerInfo)  ' Individual subbase sublayers (bottom-up computed)

End Class


Public Class clsLayerInfo

    Public Thickness As Single
    Public Modulus As Single
    Public LCode As Short

End Class


Public Class clsACRDetail

    Public ACName As String
    Public SubgradeCategory As String
    Public ReferenceStructure As New List(Of clsLayerInfo)
    Public DesignedBaseThickness As Single
    Public DSWLIterations As New List(Of clsDSWLIteration)
    Public FinalDSWL As Double
    Public FinalACR As Double

End Class


Public Class clsDSWLIteration

    Public IterationNumber As Integer
    Public Load As Double
    Public NtoFail As Double
    Public CovACN As Double
    Public Delta As Double

End Class


Public Class clsPCRRound

    Public RoundNumber As Integer
    Public CriticalAircraftName As String
    Public CriticalAircraftCDF As Double
    Public MGWIterations As New List(Of clsMGWIteration)
    Public FinalMGW As Single
    Public RoundPCR As Single
    Public EarlyExit As Boolean

End Class


Public Class clsMGWIteration

    Public IterationNumber As Integer
    Public GrossWeight As Single
    Public CDF As Double
    Public Delta As Double

End Class
