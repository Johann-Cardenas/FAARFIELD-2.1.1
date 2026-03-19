Imports System.Collections.ObjectModel
Imports System.Drawing

Namespace Interfaces
    Public Interface ISection
        Property SelectedRun As Integer
        Property RunBatch As Boolean
        Property AnalysisType As IAnalysisType
        Property Name As String
        Property NameIconNdx As Integer
        Property LastRun As String
        Property IncludeInSummary As Boolean
        Property IncludeLcca As Boolean
        Property Life As Double
        Property AnalysedLife As Double
        '        Property PavementSystem As String
        Property Layers As ObservableCollection(Of IMaterial)
        Property State As Integer
        Property Result As IResult
        Property Lcca As ILcca
        Property Sci As Double
        Property PtoTC As Double
        Property SlabEdgeStress As Double
        Property CriticalStressAicraft As String
        Property SlabInteriorStress As Double
        Property SlabEdgeStressArray As Double()
        Property SlabInteriorStressArray As Double()
        Property SlabComplete As Boolean
        Property TotalThickness As Thickness
        Property PercentCdfu As Double
        Property Airplanes As List(Of IAirplaneInfo)
        Property ClonedAirplanes As List(Of IAirplaneInfo)
        Property Deterministicaircraft As IAirplaneInfo
        Property ValidationCount As Integer
        Property Note As String
        Property ThicknessOptimization As Boolean
        Property Factory As IFaarFieldModelFactory
        Property RunStatus As Boolean
        Property SectionCDF As List(Of Single)
        Property SectionPCRCDF As Single
        Property SectionPCRPtoTC As Single
        Property SectionPCRCriticalAirplaneName As String
        Property SectionPCRCriticalAnnualDeparture As Single
        Property SectionPCRNewGL As Single
        Property SectionPCRNewPCNThick As Single
        Property SectionPCRNewPCN As Single
        Property SectionPCRAirportMasterRecordS As Single
        Property SectionPCRAirportMasterRecordD As Single
        Property SectionPCRAirportMasterRecord2D As Single
        Property SectionPCRAirportMasterRecord2D2 As Single
        Property SectionPCRAirportMasterRecordFullPCR As String
        Property SectionPCRPaveType As String
        Property SectionSubgradeCategory As String
        Property DesignType As Short
        Property SectionRunStatus As String
        Property TrafficMixName As String
        'Property ClonedTrafficMixName As String
        Property SectionPCRRunTime As String
        Property SectionDesignrunTime As String
        Property SectionLifeRunTime As String
        Property SectionCompactionRunTime As String
        Property NCHRPTracker As Boolean
        Property PCAConversionTracker As Boolean
        Property ReducedCrossSectionRun As Boolean
        Property ReducedCrossSectionLayerThickness As List(Of Thickness)
        Property ReducedCrossSectionLayerModulus As List(Of Modulus)

        Property ReducedDesignAnnualDeparture As List(Of Single)

        Property ReducedDesignTotalDeparture As List(Of Single)

        Property ReducedDesignCDF As List(Of Single)

        Property ReducedDesignCDFContribution As List(Of Single)


        Property ReducedDesignPtoC As List(Of Single)

        Property SavedPCRhtml As String
        Property SavedPCRgraph As String
        Property SavedAirportMasterRecordhtml As String
        Property SavedCDFgraph As String
        Property SavedDetailedReportHtml As String
        Property SavedSectionReportHtml As String

        Property RdecFlexuralMod As Single
        Property RdecAirVoids As Single
        Property RdecAsphaltContentByVol As Single
        Property RdecPNMS As Single
        Property RdecPPCS As Single
        Property RdecP200 As Single

    End Interface
End Namespace