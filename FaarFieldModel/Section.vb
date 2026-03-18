Imports System.Runtime.Serialization
Imports System.Text.RegularExpressions
Imports System.Windows.Forms
Imports FaarFieldModel.Interfaces
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports System.Collections.ObjectModel
Imports System.Drawing

<DataContract>
<KnownType(GetType(Section))>
<KnownType(GetType(AnalysisType))>
<KnownType(GetType(Lcca))>
<KnownType(GetType(AirplaneInfo))>
<KnownType(GetType(Length))>
<KnownType(GetType(Weight))>
<KnownType(GetType(LengthCoordinates))>
<KnownType(GetType(UsCustomary))>
<KnownType(GetType(Metric))>
Public Class Section
    Implements ISection
    Implements IModelSectionAction
    Implements IModelAction
    Implements INotifyPropertyChanged

    Public Enum RunState
        FailedValidation = 0
        PassedValidation = 1
        Run = 2
    End Enum

    <DataMember>
    Public Property SelectedRun As Integer Implements ISection.SelectedRun

    <DataMember>
    Public Property RunBatch As Boolean Implements ISection.RunBatch

    <DataMember>
    Public Property AnalysisType As IAnalysisType Implements ISection.AnalysisType

    Private m_Name As String = ""
    <DataMember>
    Public Property Name As String Implements ISection.Name
        Get
            Return m_Name
        End Get
        Set(value As String)
            If Not value.Trim() = "" Then
                m_Name = value
            End If
        End Set
    End Property

    <DataMember>
    Public Property LastRun As String Implements ISection.LastRun
    <DataMember>
    Public Property NameIconNdx As Integer Implements ISection.NameIconNdx
    <DataMember>
    Public Property _IncludeInSummary As Boolean
    <DataMember>
    Public Property IncludeLcca As Boolean Implements ISection.IncludeLcca
    <DataMember>
    Public Property Life As Double Implements ISection.Life
    <DataMember>
    Public Property TotalThickness As Thickness Implements ISection.TotalThickness
    <DataMember>
    Public Property AnalysedLife As Double Implements ISection.AnalysedLife

    Public Property Deterministicaircraft As IAirplaneInfo Implements ISection.Deterministicaircraft
    <DataMember>
    Public Property Layers As ObservableCollection(Of IMaterial) Implements ISection.Layers
    <DataMember>
    Public Property State As Integer Implements ISection.State
    <DataMember>
    Public Property Result As IResult Implements ISection.Result
    <DataMember>
    Public Property Lcca As ILcca Implements ISection.Lcca
    <DataMember>
    Public Property Sci As Double Implements ISection.Sci
    <DataMember>
    Public Property SlabEdgeStress As Double Implements ISection.SlabEdgeStress
    <DataMember>
    Public Property SlabInteriorStress As Double Implements ISection.SlabInteriorStress
    <DataMember>
    Public Property CriticalStressAircraft As String Implements ISection.CriticalStressAicraft
    <DataMember>
    Public Property SlabEdgeStressArray As Double() Implements ISection.SlabEdgeStressArray
    <DataMember>
    Public Property SlabEdgeInteriorArray As Double() Implements ISection.SlabInteriorStressArray
    <DataMember>
    Public Property SlabComplete As Boolean Implements ISection.SlabComplete
    <DataMember>
    Public Property PtoTC As Double Implements ISection.PtoTC
    <DataMember>
    Public Property PercentCdfu As Double Implements ISection.PercentCdfu

    <DataMember>
    Public Property Airplanes As List(Of IAirplaneInfo) Implements ISection.Airplanes
    Public Property ValidationCount As Integer Implements ISection.ValidationCount
    Private Property LifeValidation As IValidationRange
    Private Property SciValidation As IValidationRange
    Private Property CdfuValidation As IValidationRange
    Private Property StressValidation As IValidationRange
    Public Property Factory As IFaarFieldModelFactory Implements ISection.Factory

    <DataMember>
    Public Property Note As String Implements ISection.Note
    <DataMember>
    Public Property ClonedAirplanes As List(Of IAirplaneInfo) Implements ISection.ClonedAirplanes
    <DataMember>
    Public Property ThicknessOptimization As Boolean Implements ISection.ThicknessOptimization
    <DataMember>
    Public Property RunStatus As Boolean Implements ISection.RunStatus
    <DataMember>
    Public Property SectionCDF As List(Of Single) Implements ISection.SectionCDF
    <DataMember>
    Public Property ReducedDesignAnnualDeparture As List(Of Single) Implements ISection.ReducedDesignAnnualDeparture
    <DataMember>
    Public Property ReducedDesignTotalDeparture As List(Of Single) Implements ISection.ReducedDesignTotalDeparture
    <DataMember>
    Public Property ReducedDesignCDF As List(Of Single) Implements ISection.ReducedDesignCDF
    <DataMember>
    Public Property ReducedDesignCDFContribution As List(Of Single) Implements ISection.ReducedDesignCDFContribution

    <DataMember>
    Public Property ReducedDesignPtoC As List(Of Single) Implements ISection.ReducedDesignPtoC
    <DataMember>
    Public Property ReducedCrossSectionLayerThickness As List(Of Thickness) Implements ISection.ReducedCrossSectionLayerThickness
    <DataMember>
    Public Property ReducedCrossSectionLayerModulus As List(Of Modulus) Implements ISection.ReducedCrossSectionLayerModulus
    <DataMember>
    Public Property SectionPCRCDF As Single Implements ISection.SectionPCRCDF
    <DataMember>
    Public Property SectionPCRPtoTC As Single Implements ISection.SectionPCRPtoTC
    <DataMember>
    Public Property SectionPCRCriticalAirplaneName As String Implements ISection.SectionPCRCriticalAirplaneName
    <DataMember>
    Public Property SectionPCRCriticalAnnualDeparture As Single Implements ISection.SectionPCRCriticalAnnualDeparture
    <DataMember>
    Public Property SectionPCRNewGL As Single Implements ISection.SectionPCRNewGL
    <DataMember>
    Public Property SectionPCRNewPCNThick As Single Implements ISection.SectionPCRNewPCNThick
    <DataMember>
    Public Property SectionPCRNewPCN As Single Implements ISection.SectionPCRNewPCN
    <DataMember>
    Public Property SectionPCRAirportMasterRecordS As Single Implements ISection.SectionPCRAirportMasterRecordS
    <DataMember>
    Public Property SectionPCRAirportMasterRecordD As Single Implements ISection.SectionPCRAirportMasterRecordD
    <DataMember>
    Public Property SectionPCRAirportMasterRecord2D As Single Implements ISection.SectionPCRAirportMasterRecord2D
    <DataMember>
    Public Property SectionPCRAirportMasterRecord2D2 As Single Implements ISection.SectionPCRAirportMasterRecord2D2
    <DataMember>
    Public Property SectionPCRPaveType As String Implements ISection.SectionPCRPaveType
    <DataMember>
    Public Property SectionSubgradeCategory As String Implements ISection.SectionSubgradeCategory
    <DataMember>
    Public Property DesignType As Short Implements ISection.DesignType
    <DataMember>
    Public Property SectionPCRAirportMasterRecordFullPCR As String Implements ISection.SectionPCRAirportMasterRecordFullPCR

    <DataMember>
    Public Property TrafficMixName As String Implements ISection.TrafficMixName

    <DataMember>
    Public Property SectionRunStatus As String Implements ISection.SectionRunStatus
    <DataMember>
    Public Property SectionDesignRunTime As String Implements ISection.SectionDesignrunTime
    <DataMember>
    Public Property SectionLifeRunTime As String Implements ISection.SectionLifeRunTime
    <DataMember>
    Public Property SectionCompactionRunTime As String Implements ISection.SectionCompactionRunTime
    <DataMember>
    Public Property SectionPCRRunTime As String Implements ISection.SectionPCRRunTime
    <DataMember>
    Public Property NCHRPTracker As Boolean Implements ISection.NCHRPTracker
    <DataMember>
    Public Property PCAConversionTracker As Boolean Implements ISection.PCAConversionTracker
    <DataMember>
    Public Property ReducedCrossSectionRun As Boolean Implements ISection.ReducedCrossSectionRun

    Public Property SavedPCRhtml As String Implements ISection.SavedPCRhtml
    Public Property SavedPCRgraph As String Implements ISection.SavedPCRgraph
    Public Property SavedAirportMasterRecordhtml As String Implements ISection.SavedAirportMasterRecordhtml
    Public Property SavedCDFgraph As String Implements ISection.SavedCDFgraph

    <DataMember>
    Public Property RdecFlexuralMod As Single Implements ISection.RdecFlexuralMod
    <DataMember>
    Public Property RdecAirVoids As Single Implements ISection.RdecAirVoids
    <DataMember>
    Public Property RdecAsphaltContentByVol As Single Implements ISection.RdecAsphaltContentByVol
    <DataMember>
    Public Property RdecPNMS As Single Implements ISection.RdecPNMS
    <DataMember>
    Public Property RdecPPCS As Single Implements ISection.RdecPPCS
    <DataMember>
    Public Property RdecP200 As Single Implements ISection.RdecP200

    ''' <summary>
    ''' This is the default constructor for the Section class.
    ''' It initializes multiple properties at once.
    ''' </summary>
    Public Sub New()
        Life = 20
        IncludeInSummary = True
        IncludeLcca = False
        State = RunState.PassedValidation
        Layers = Nothing
        Result = Nothing
        Lcca = Nothing
        SlabInteriorStress = 0
        SlabEdgeStress = 0
        Airplanes = New List(Of IAirplaneInfo)
        Layers = New ObservableCollection(Of IMaterial)
        ValidationCount = 0
        ThicknessOptimization = False
        RunStatus = False
        PtoTC = 1

        RdecFlexuralMod = 600000
        RdecAirVoids = 3.5
        RdecAsphaltContentByVol = 12
        RdecPNMS = 95
        RdecPPCS = 58
        RdecP200 = 4.5

        For i = 0 To 42
            SectionCDF.Add(0)

        Next
        SectionRunStatus = ""
    End Sub

    ''' <summary>
    ''' This is an alternate constructor for the Section class.
    ''' Initializes many validation properties using the methods in a factory object.
    ''' </summary>
    ''' <param name="factory">The object passed in that is used for its methods within it.</param>
    Public Sub New(factory As IFaarFieldModelFactory)
        Life = 20
        IncludeInSummary = True
        IncludeLcca = False
        State = RunState.PassedValidation
        Layers = Nothing
        Result = Nothing
        Lcca = Nothing
        Airplanes = New List(Of IAirplaneInfo)
        Layers = New ObservableCollection(Of IMaterial)
        PtoTC = 1
        Sci = 80
        SlabEdgeStress = 0
        SlabInteriorStress = 0
        PercentCdfu = 100
        LifeValidation = factory.CreateValidationRange(1, 50, True)
        SciValidation = factory.CreateValidationRange(0, 100, True)
        CdfuValidation = factory.CreateValidationRange(0, 100, True)
        ThicknessOptimization = False
        ValidationCount = 1
        Note = ""
        SectionCDF = New List(Of Single)
        For i = 0 To 42
            SectionCDF.Add(0)
        Next
        RdecFlexuralMod = 600000
        RdecAirVoids = 3.5
        RdecAsphaltContentByVol = 12
        RdecPNMS = 95
        RdecPPCS = 58
        RdecP200 = 4.5
        RunStatus = False
        SectionRunStatus = ""
    End Sub

    ''' <summary>
    ''' This is the constructor for the Section that is used when a section is being cloned.
    ''' It uses an already existing factory and section to initialize its values.
    ''' </summary>
    ''' <param name="factory">This is the existing factory that get its values copied and its methods used.</param>
    ''' <param name="existing">This is the already existing section that is being used for cloning.</param>
    Public Sub New(factory As IFaarFieldModelFactory, existing As ISection)
        AnalysisType = factory.CreateAnalysisType(factory, existing.AnalysisType)
        Name = New String(existing.Name)
        NameIconNdx = existing.NameIconNdx
        IncludeInSummary = existing.IncludeInSummary
        IncludeLcca = existing.IncludeLcca
        Life = existing.Life
        Result = Nothing
        Lcca = Nothing
        Airplanes = New List(Of IAirplaneInfo)
        For Each airplane In existing.Airplanes
            Airplanes.Add(factory.CreateAircraft(airplane, factory))
        Next
        LifeValidation = factory.CreateValidationRange(1, 50, True)
        SciValidation = factory.CreateValidationRange(0, 100, True)
        CdfuValidation = factory.CreateValidationRange(0, 100, True)
        StressValidation = factory.CreateValidationRange(0, 10, True)
        ValidationCount = existing.ValidationCount
        Note = existing.Note
        ThicknessOptimization = existing.ThicknessOptimization
        RunStatus = False
        Layers = New ObservableCollection(Of IMaterial)
        For Each layer In existing.Layers
            Layers.Add(factory.CreateMaterial(factory, layer, layer.CanDelete))
        Next

        Sci = existing.Sci
        PercentCdfu = existing.PercentCdfu
        PtoTC = existing.PtoTC
        SlabInteriorStress = existing.SlabInteriorStress
        SlabEdgeStress = existing.SlabEdgeStress
        SectionRunStatus = ""
        PCAConversionTracker = existing.PCAConversionTracker
        NCHRPTracker = existing.NCHRPTracker
        RdecFlexuralMod = existing.RdecFlexuralMod
        RdecAirVoids = existing.RdecAirVoids
        RdecAsphaltContentByVol = existing.RdecAsphaltContentByVol
        RdecPNMS = existing.RdecPNMS
        RdecPPCS = existing.RdecPPCS
        RdecP200 = existing.RdecP200
    End Sub

    ''' <summary>
    ''' This method validates the number of departures in each aircraft.
    ''' </summary>
    ''' <param name="measurementSystem">The measurement system used.</param>
    ''' <returns>A list of validations.</returns>
    Public Function Validate(measurementSystem As IMeasurmentSystem) As List(Of IValidation) Implements IModelAction.Validate
        Dim messages As New List(Of IValidation)
        Dim count = 0
        For Each airplaneInfo In Airplanes
            If airplaneInfo.NumberDepartures < 0 Then
                messages.Add(New ValidationMessage("FormAirplanes", "Number of Departures", "MessageAirplanes", "Number of departures for " + airplaneInfo.Name + " must be greater than 0."))
            End If

            If airplaneInfo.AnnualGrowth < -20 Or airplaneInfo.AnnualGrowth > 20 Then
                messages.Add(New ValidationMessage("FormAirplanes", "Annual Growth", "MessageAirplanes", "The Annual Growth for " + airplaneInfo.Name + " must be between -20 and 20."))
            End If
            count += airplaneInfo.NumberDepartures
        Next

        If Airplanes.Count = 0 Then
            messages.Add(New ValidationMessage("FormAirplanes", "", "MessageAirplanes", "No airplane traffic defined."))
        End If

        If count = 0 Then
            messages.Add(New ValidationMessage("FormAirplanes", "", "MessageAirplanes", "No data for annual departures entered."))
        End If

        Return messages
    End Function

    ''' <summary>
    ''' This method validates the LifeValidation, SciValidation, CdfuValidation properties. 
    ''' </summary>
    ''' <param name="defaults">This is a collection of MaterialDefault objects used for the validateMaterials method.</param>
    ''' <param name="measurementSystem">This is the measurement system that is being used.</param>
    ''' <returns></returns>
    Public Function Validate(defaults As ObservableCollection(Of MaterialDefault), measurementSystem As IMeasurmentSystem) As List(Of IValidation) Implements IModelSectionAction.Validate
        Dim messages As New List(Of IValidation)

        If LifeValidation Is Nothing Then
            LifeValidation = Factory.CreateValidationRange(1, 50, True)
            SciValidation = Factory.CreateValidationRange(0, 100, True)
            CdfuValidation = Factory.CreateValidationRange(0, 100, True)
            ValidationCount = 0
        End If

        Dim message = LifeValidation.Validate(Life, "FormSection", "life", "MessageLife", measurementSystem)

        If message IsNot Nothing Then
            messages.Add(New ValidationMessage("FormSection", "Life", "MessageLife", "AC 150/5320-6D [7] §302.a. The standard design life for pavement section is 20 years (1 to 50 allowed)."))
        End If

        message = SciValidation.Validate(Sci, "FormSection", "SCI", "MesssgeSciPercentCdfu", measurementSystem)
        If message IsNot Nothing Then
            messages.Add(message)
        End If


        message = CdfuValidation.Validate(PercentCdfu, "FormSection", "Percent CDFU", "MesssgeSciPercentCdfu", measurementSystem)
        If message IsNot Nothing Then
            messages.Add(message)
        End If

        If PercentCdfu < 100 And Sci < 100 Then
            messages.Add(New ValidationMessage("FormSection", "", "MesssgeSciPercentCdfu", "Either SCI or Percent CDFU must be 100 percent."))
        End If

        Dim pattern = "[A-Za-z0-9 _.,!']$"
        Dim regex = New Regex(pattern, RegexOptions.IgnoreCase)

        If Not regex.IsMatch(Name) Then
            messages.Add(New ValidationMessage("FormSection", "Name", "MessageSectionName", "Section names are allowed upper and lower case letters, numbers, and common punctuation."))
        End If

        If AnalysisType Is Nothing Then
            messages.Add(New ValidationMessage("FormSection", "", "MessageStructure", "Analysis type not selected."))
        End If

        messages.AddRange(ValidateMaterials(defaults, measurementSystem))
        Return messages
    End Function

    ''' <summary>
    ''' This is the method used to validate a collection of materials.
    ''' </summary>
    ''' <param name="defaults">This is the collections of MaterialDefault objects.</param>
    ''' <param name="measurementSystem">This is the measurement system that is being used.</param>
    ''' <returns></returns>
    Private Function ValidateMaterials(defaults As ObservableCollection(Of MaterialDefault), measurementSystem As IMeasurmentSystem) As List(Of IValidation)
        Dim messages As New List(Of IValidation)

        For Each material In Layers
            Dim defaultMaterial As New MaterialDefault
            For Each mat In defaults
                If mat.Name = material.Name Then
                    defaultMaterial = mat
                End If
            Next

            If defaultMaterial.ThicknessValidation IsNot Nothing Then
                Dim message = defaultMaterial.ThicknessValidation.Validate(material.Thickness.GetValue(measurementSystem), "FormSection", "thickness", "MessageStructure", measurementSystem)
                If message IsNot Nothing Then
                    messages.Add(message)
                End If
            End If

            If defaultMaterial.SubgradeReactionValidation IsNot Nothing Then
                If defaultMaterial.Name <> "User Defined" Then
                    Dim message = defaultMaterial.SubgradeReactionValidation.Validate(material.SubgradeReaction.GetValue(measurementSystem), "FormSection", "subgrade reaction", "MessageStructure", measurementSystem)
                    If message IsNot Nothing Then
                        messages.Add(message)
                    End If

                ElseIf Convert.ToDouble(defaultMaterial.SubgradeReaction.GetValue(measurementSystem)) < 20.9 Then
                    Dim message = defaultMaterial.SubgradeReactionValidation.Validate(material.SubgradeReaction.GetValue(measurementSystem), "FormSection", "subgrade reaction", "MessageStructure", measurementSystem)
                    If message IsNot Nothing Then
                        messages.Add(message)
                    End If

                End If
            End If

            If defaultMaterial.ModulusValidation IsNot Nothing Then
                Dim message = defaultMaterial.ModulusValidation.Validate(material.Modulus.GetValue(measurementSystem), "FormSection", "modulus", "MessageStructure", measurementSystem)
                If message IsNot Nothing Then
                    messages.Add(message)
                End If
            End If

            If defaultMaterial.CbrValidation IsNot Nothing Then
                If defaultMaterial.Name <> "User Defined" Then
                    Dim message = defaultMaterial.CbrValidation.Validate(material.Cbr, "FormSection", "CBR", "MessageStructure", measurementSystem)
                    If message IsNot Nothing Then
                        messages.Add(message)
                    End If
                End If
            End If
        Next
        Return messages
    End Function
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    ' This method is called by the Set accessor of each property.
    ' The CallerMemberName attribute that is applied to the optional propertyName
    ' parameter causes the property name of the caller to be substituted as an argument.
    Private Sub OnPropertyChanged(<CallerMemberName()> Optional ByVal info As String = Nothing)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
    End Sub
    Public Property IncludeInSummary As Boolean Implements ISection.IncludeInSummary
        Get
            Return _IncludeInSummary
        End Get
        Set(value As Boolean)
            _IncludeInSummary = value
            OnPropertyChanged("IncludeInSummary")
        End Set
    End Property


    <OnDeserialized>
    Private Sub OnDeserialized(context As StreamingContext)
        If RdecFlexuralMod = 0 Then RdecFlexuralMod = 600000
        If RdecAirVoids = 0 Then RdecAirVoids = 3.5
        If RdecAsphaltContentByVol = 0 Then RdecAsphaltContentByVol = 12
        If RdecPNMS = 0 Then RdecPNMS = 95
        If RdecPPCS = 0 Then RdecPPCS = 58
        If RdecP200 = 0 Then RdecP200 = 4.5
    End Sub

End Class
