Imports System.Collections.ObjectModel

Namespace Interfaces
    Public Interface IFaarFieldModelFactory

#Region "Measurement Systems"
        ''' <summary>
        ''' Create US Customary MeasurementSystem object
        ''' </summary>
        ''' <returns></returns>
        Function CreateUsCustomary() As IMeasurmentSystem
        ''' <summary>
        ''' Create SI/Metric MeasurementSystem object
        ''' </summary>
        ''' <returns></returns>
        Function CreateMetric() As IMeasurmentSystem
#End Region

#Region "FaarField"
        ''' <summary>
        ''' Create a FaarField object from the default files.
        ''' </summary>
        ''' <param name="factory">FAARFIELD factory to inject dependencies</param>
        ''' <param name="defaultFilePath">Path to appropriate default files</param>
        ''' <returns></returns>
        Function CreateFaarFieldJob(factory As IFaarFieldModelFactory, defaultFilePath As String) As IFaarFieldJob
        ''' <summary>
        ''' Create a FaarField object from a serialized FaarField object
        ''' </summary>
        ''' <param name="filePath">File path to file to deserialize</param>
        ''' <returns></returns>
        Function CreateFaarFieldJob(filePath As String) As IFaarFieldJob
        ''' <summary>
        ''' Make of a clone of an existing FaarField object
        ''' </summary>
        ''' <param name="factory">FAARFIELD factory to inject dependencies</param>
        ''' <param name="existingFaarField">Existing FaarField object</param>
        ''' <returns></returns>
        Function CreateFaarFieldJob(factory As IFaarFieldModelFactory, existingFaarField As IFaarFieldJob) As IFaarFieldJob
#End Region

#Region "JobInformation"
        ''' <summary>
        '''Create a empty JobInformation.  There are no default JobInformation fields. 
        ''' </summary>
        ''' <returns></returns>
        Function CreateJobInformation() As IJobInformation

        Function CreateJobInformation(factory As IFaarFieldModelFactory) As IJobInformation
        ''' <summary>
        ''' Clone an existing JobInformation object
        ''' </summary>
        ''' <param name="factory">Factory to inject dependencies for clongin</param>
        ''' <param name="existingJobInformation">Existing JobInformation object</param>
        ''' <returns></returns>
        Function CreateJobInformation(factory As IFaarFieldModelFactory, existingJobInformation As IJobInformation) As IJobInformation
#End Region

#Region "DesignOptions"
        ''' <summary>
        ''' Create a DesignOption object from a default file
        ''' </summary>
        ''' <param name="defaultFilePath">Path to location of DesignOption default file</param>
        ''' <returns></returns>
        Function CreateDesignOptions(defaultFilePath As String) As IDesignOptions


        Function CreateDesignOptions(factory As IFaarFieldModelFactory) As IDesignOptions
        ''' <summary>
        ''' Clone existing DesignOption object
        ''' </summary>
        ''' <param name="factory">Factory to inject dependencies necessary for clone</param>
        ''' <param name="existingDesignOptions">Existing DesignOptions object</param>
        ''' <returns></returns>
        Function CreateDesignOptions(factory As IFaarFieldModelFactory, existingDesignOptions As IDesignOptions) As IDesignOptions
#End Region

#Region "Section"
        ''' <summary>
        ''' Create a new Section by setting all Properties
        ''' </summary>
        ''' <param name="factory">Factory to inject dependencies</param>
        ''' <returns></returns>
        Function CreateSection(factory As IFaarFieldModelFactory) As ISection
        ''' <summary>
        ''' Create a new Section by cloning an existing section
        ''' </summary>
        ''' <param name="factory">Factory to inject dependencies</param>
        ''' <param name="existingSection">Existing FAARFIELD Section</param>
        ''' <returns></returns>
        Function CreateSection(factory As IFaarFieldModelFactory, existingSection As ISection) As ISection
#End Region


#Region "Material"
        ''' <summary>
        ''' Create a material by opening opening a default version of material
        ''' </summary>
        ''' <param name="factory">Factory to inject the dependencies</param>
        ''' <param name="defaultFilePath">File path to material to deserialize</param>
        ''' <returns></returns>
        Function CreateMaterial(factory As IFaarFieldModelFactory, defaultFilePath As String) As IMaterial
        ''' <summary>
        ''' Clone an exisiting material.
        ''' </summary>
        ''' <param name="factory">Factory to inject dependencies</param>
        ''' <param name="existingLayer">Layer to clone (can be a default layer)</param>
        ''' <returns></returns>
        Function CreateMaterial(factory As IFaarFieldModelFactory, existingLayer As IMaterial, canDelete As Boolean) As IMaterial
#End Region

#Region "Dimensional Properties"
        ''' <summary>
        ''' Stores thickness data (inches/mm)
        ''' </summary>
        ''' <param name="value">Thickness value</param>
        ''' <param name="measurementSystem">US Cutomary or metric</param>
        ''' <returns></returns>
        Function CreateThickness(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty

        ''' <summary>
        ''' Stores area data (inches/mm)
        ''' </summary>
        ''' <param name="value">Area value</param>
        ''' <param name="measurementSystem">US Cutomary or metric</param>
        ''' <returns></returns>
        Function CreateArea(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty

        ''' <summary>
        ''' Stores subgrade reaction data (psi/in or kpa/m)
        ''' </summary>
        ''' <param name="value">Subgrade reaction value</param>
        ''' <param name="measurementSystem">US Cutomary or metric</param>
        ''' <returns></returns>
        Function CreateSubgradeReaction(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty
        ''' <summary>
        ''' Stores pressure data (psi or kpa)
        ''' </summary>
        ''' <param name="value">Thickness property</param>
        ''' <param name="measurementSystem">US Cutomary or metric</param>
        ''' <returns></returns>
        Function CreatePressure(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty
        ''' <summary>
        ''' Stores modulues data (psi or kpa)
        ''' </summary>
        ''' <param name="value">Modulus value</param>
        ''' <param name="measurementSystem">US Cutomary or metric</param>
        ''' <returns></returns>
        Function CreateModulus(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty

        Function CreateLength(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty

        Function CreateCoordinates(x As IDimensionalProperty, y As IDimensionalProperty) As ICoordinates

        Function CreateWeight(value As Double, measurementSystem As IMeasurmentSystem) As IDimensionalProperty


#End Region

#Region "AnalysisType"
        Function CreateAnalysisType(index As Integer, name As String, subgradeReaction As Boolean, rehabilitation As Boolean, layers As ObservableCollection(Of IMaterial)) As IAnalysisType
        Function CreateAnalysisType(factory As IFaarFieldModelFactory, existing As IAnalysisType) As IAnalysisType
#End Region


        Function CreateThicknessValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As ThicknessValidationRange
        Function CreateThicknessValidationRange(validation As IValidationRange) As ThicknessValidationRange
        Function CreateLengthValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As LengthValidationRange
        Function CreateLengthValidationRange(validation As IValidationRange) As LengthValidationRange
        Function CreateSubgradeReactionValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As SubgradeReactionValidationRange
        Function CreatePressureValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As PressureValidationRange
        Function CreateModulusValidationRange(minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem) As ModulusValidationRange
        Function CreateCoordinateValidation(x As IValidationRange, y As IValidationRange) As CoordinateValidationRange
        Function CreateValidationRange(minimum As Double, maximum As Double, required As Boolean) As ValidationRange

        Function CreateResults(resultFilesPath As String) As IResult


        Function CreateAircraft() As IAirplaneInfo

        Function CreateAircraft(existing As IAirplaneInfo, factory As IFaarFieldModelFactory) As IAirplaneInfo

        Function CreateAircraftbelly() As IAirplaneInfo

        Function CreateAircraftbelly(aircraftbellyinfo As IAirplaneInfo, factory As IFaarFieldModelFactory) As IAirplaneInfo


        Function CreateValidationMessage(form As String, control As String, label As String, message As String) As IValidation

        Function CreateDatabase(name As String) As IDatabase
        Function CreateNetwork(id As Integer, name As String) As INetwork
        Function CreateBranch(id As Integer, name As String) As IBranch
        Function CreateSection(id As Integer, name As String) As IPaveairSection
        Function CreateJobDownload(id As Integer, name As String) As IJobDownload
    End Interface
End Namespace