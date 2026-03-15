Option Explicit On

''' <summary>
''' Data collection classes for the Detailed Computation Report.
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
