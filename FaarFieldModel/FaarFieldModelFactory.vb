Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Text
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

<Serializable()>
Public Class FaarFieldModelFactory
    Implements IFaarFieldModelFactory

    Public Function CreateUsCustomary() As IMeasurmentSystem Implements IFaarFieldModelFactory.CreateUsCustomary
        Return New UsCustomary()
    End Function

    Public Function CreateMetric() As IMeasurmentSystem Implements IFaarFieldModelFactory.CreateMetric
        Return New Metric()
    End Function

    Public Function CreateFaarFieldJob(factory As IFaarFieldModelFactory, defaultFilePath As String) As IFaarFieldJob Implements IFaarFieldModelFactory.CreateFaarFieldJob
        Return New FaarFieldJob(factory, defaultFilePath)
    End Function

    Public Function CreateFaarFieldJob(filePath As String) As IFaarFieldJob Implements IFaarFieldModelFactory.CreateFaarFieldJob
        Dim serializer = New DataContractSerializer(GetType(FaarFieldJob))
        Dim textReader As TextReader = New StreamReader(filePath)
        Dim json = textReader.ReadToEnd()
        Dim stream = New MemoryStream(Encoding.Unicode.GetBytes(json))

        Return DirectCast(serializer.ReadObject(stream), FaarFieldJob)
    End Function

    Public Function CreateFaarFieldJob(factory As IFaarFieldModelFactory, existingFaarField As IFaarFieldJob) As IFaarFieldJob Implements IFaarFieldModelFactory.CreateFaarFieldJob
        Return New FaarFieldJob(factory, existingFaarField)
    End Function

    Public Function CreateJobInformation() As IJobInformation Implements IFaarFieldModelFactory.CreateJobInformation
        Return New JobInformation()
    End Function

    Public Function CreateJobInformation(factory As IFaarFieldModelFactory) As IJobInformation Implements IFaarFieldModelFactory.CreateJobInformation
        Return New JobInformation(factory)
    End Function

    Public Function CreateJobInformation(factory As IFaarFieldModelFactory, existingJobInformation As IJobInformation) As IJobInformation Implements IFaarFieldModelFactory.CreateJobInformation
        Return New JobInformation(factory, existingJobInformation)
    End Function

    Public Function CreateDesignOptions(defaultFilePath As String) As IDesignOptions Implements IFaarFieldModelFactory.CreateDesignOptions
        Dim serializer = New DataContractSerializer(GetType(DesignOptions))
        Dim textReader As TextReader = New StreamReader(defaultFilePath + "\\DesignOption.txt")
        Dim json = textReader.ReadToEnd()
        Dim stream = New MemoryStream(Encoding.Unicode.GetBytes(json))
        Return DirectCast(serializer.ReadObject(stream), DesignOptions)
    End Function
    Public Function CreateDesignOptions(factory As IFaarFieldModelFactory) As IDesignOptions Implements IFaarFieldModelFactory.CreateDesignOptions
        Return New DesignOptions(factory)
    End Function

    Public Function CreateDesignOptions(factory As IFaarFieldModelFactory, existingDesignOptions As IDesignOptions) As IDesignOptions Implements IFaarFieldModelFactory.CreateDesignOptions
        Return New DesignOptions(factory, existingDesignOptions)
    End Function

    Public Function CreateSection(factory As IFaarFieldModelFactory) As ISection Implements IFaarFieldModelFactory.CreateSection
        Return New Section(factory)
    End Function

    Public Function CreateSection(factory As IFaarFieldModelFactory, existingSection As ISection) As ISection Implements IFaarFieldModelFactory.CreateSection
        Return New Section(factory, existingSection)
    End Function

    Public Function CreateMaterial(factory As IFaarFieldModelFactory, defaultFilePath As String) As IMaterial Implements IFaarFieldModelFactory.CreateMaterial
        Throw New NotImplementedException
    End Function

    Public Function CreateMaterial(factory As IFaarFieldModelFactory, existingLayer As IMaterial, canDelete As Boolean) As IMaterial Implements IFaarFieldModelFactory.CreateMaterial
        Return New Material(factory, existingLayer, canDelete)
    End Function

    Public Function CreateAnalysisType(index As Integer, name As String, subgradeReaction As Boolean, rehabilitation As Boolean, layers As ObservableCollection(Of IMaterial)) As IAnalysisType Implements IFaarFieldModelFactory.CreateAnalysisType
        Return New AnalysisType(index, name, subgradeReaction, rehabilitation, layers)
    End Function

    Public Function CreateAnalysisType(factory As IFaarFieldModelFactory, existing As IAnalysisType) As IAnalysisType Implements IFaarFieldModelFactory.CreateAnalysisType
        Return New AnalysisType(factory, existing)
    End Function

    Public Function CreateThickness(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty Implements IFaarFieldModelFactory.CreateThickness
        Return New Thickness(value, measurementSystem)
    End Function

    Public Function CreateArea(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty Implements IFaarFieldModelFactory.CreateArea
        Return New Area(value, measurementSystem)
    End Function

    Public Function CreateSubgradeReaction(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty Implements IFaarFieldModelFactory.CreateSubgradeReaction
        Return New SubgradeReaction(value, measurementSystem)
    End Function

    Public Function CreatePressure(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty Implements IFaarFieldModelFactory.CreatePressure
        Return New Pressure(value, measurementSystem)
    End Function

    Public Function CreateModulus(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty Implements IFaarFieldModelFactory.CreateModulus
        Return New Modulus(value, measurementSystem)
    End Function

    Public Function CreateLength(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty Implements IFaarFieldModelFactory.CreateLength
        Return New Length(value, measurementSystem)
    End Function

    Public Function CreateWeight(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty Implements IFaarFieldModelFactory.CreateWeight
        Return New Weight(value, measurementSystem)
    End Function

    Public Function CreateCoordinates(x As IDimensionalProperty, y As IDimensionalProperty) As ICoordinates Implements IFaarFieldModelFactory.CreateCoordinates
        Return New LengthCoordinates(x, y)
    End Function
    Public Function CreateThicknessValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As ThicknessValidationRange Implements IFaarFieldModelFactory.CreateThicknessValidationRange
        Return New ThicknessValidationRange(Me, minimum, maximum, required, measurementSystem)
    End Function

    Public Function CreateThicknessValidationRange(validation As IValidationRange) As ThicknessValidationRange Implements IFaarFieldModelFactory.CreateThicknessValidationRange
        Return New ThicknessValidationRange(Me, validation)
    End Function

    Public Function CreateLengthValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As LengthValidationRange Implements IFaarFieldModelFactory.CreateLengthValidationRange
        Return New LengthValidationRange(Me, minimum, maximum, required, measurementSystem)
    End Function

    Public Function CreateLengthValidationRange(validation As IValidationRange) As LengthValidationRange Implements IFaarFieldModelFactory.CreateLengthValidationRange
        Return New LengthValidationRange(Me, validation)
    End Function

    Public Function CreateSubgradeReactionValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As SubgradeReactionValidationRange Implements IFaarFieldModelFactory.CreateSubgradeReactionValidationRange
        Return New SubgradeReactionValidationRange(Me, minimum, maximum, required, measurementSystem)
    End Function

    Public Function CreatePressureValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As PressureValidationRange Implements IFaarFieldModelFactory.CreatePressureValidationRange
        Return New PressureValidationRange(Me, minimum, maximum, required, measurementSystem)
    End Function

    Public Function CreateModulusValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As ModulusValidationRange Implements IFaarFieldModelFactory.CreateModulusValidationRange
        Return New ModulusValidationRange(Me, minimum, maximum, required, measurementSystem)
    End Function

    Public Function CreateCoordinateValidation(x As IValidationRange, y As IValidationRange) As CoordinateValidationRange Implements IFaarFieldModelFactory.CreateCoordinateValidation
        Return New CoordinateValidationRange(x, y, False)
    End Function

    Public Function CreateValidationRange(minimum As Double, maximum As Double, required As Boolean) As ValidationRange Implements IFaarFieldModelFactory.CreateValidationRange
        Return New ValidationRange(Me, minimum, maximum, required)
    End Function

    Public Function CreateResults(resultFilesPath As String) As IResult Implements IFaarFieldModelFactory.CreateResults
        Throw New NotImplementedException
    End Function

    Public Function CreateAircraft() As IAirplaneInfo Implements IFaarFieldModelFactory.CreateAircraft
        Return New AirplaneInfo()
    End Function

    Public Function CreateAircraft(existing As IAirplaneInfo, factory As IFaarFieldModelFactory) As IAirplaneInfo Implements IFaarFieldModelFactory.CreateAircraft
        Return New AirplaneInfo(existing, factory)
    End Function
    Public Function CreateAircraftbelly() As IAirplaneInfo Implements IFaarFieldModelFactory.CreateAircraftbelly
        Return New AirplaneInfo()
    End Function
    Public Function CreateAircraftbelly(aircraftbellyinfo As IAirplaneInfo, factory As IFaarFieldModelFactory) As IAirplaneInfo Implements IFaarFieldModelFactory.CreateAircraftbelly
        Return New AirplaneInfo(aircraftbellyinfo, factory)
    End Function

    Public Function CreateValidationMessage(form As String, control As String, label As String, message As String) As IValidation Implements IFaarFieldModelFactory.CreateValidationMessage
        Return New ValidationMessage(form, control, label, message)
    End Function

    Public Function CreateDatabase(Name As String) As IDatabase Implements IFaarFieldModelFactory.CreateDatabase
        Return New Database(Name)
    End Function

    Public Function CreateNetwork(Id As Integer, Name As String) As INetwork Implements IFaarFieldModelFactory.CreateNetwork
        Return New Network(Id, Name)
    End Function

    Public Function CreateBranch(Id As Integer, Name As String) As IBranch Implements IFaarFieldModelFactory.CreateBranch
        Return New Branch(Id, Name)
    End Function

    Public Function CreateSection(Id As Integer, Name As String) As IPaveairSection Implements IFaarFieldModelFactory.CreateSection
        Return New PaveairSection(Id, Name)
    End Function

    Public Function CreateJobDownload(Id As Integer, Name As String) As IJobDownload Implements IFaarFieldModelFactory.CreateJobDownload
        Return New JobDownload(Id, Name)
    End Function

End Class
