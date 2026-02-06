Imports System.IO
Imports System.Runtime.Serialization
Imports System.Text
Imports FaarFieldModel.Interfaces
<DataContract>
<KnownType(GetType(JobInformation))>
<KnownType(GetType(Thickness))>
Public Class JobInformation
    Implements IJobInformation
    Implements IModelAction
    <DataMember>
    Public Property LocationIdentifier As String Implements IJobInformation.LocationIdentifier
    <DataMember>
    Public Property State As String Implements IJobInformation.State
    <DataMember>
    Public Property City As String Implements IJobInformation.City
    <DataMember>
    Public Property Country As String Implements IJobInformation.Country
    <DataMember>
    Public Property Airport As String Implements IJobInformation.Airport
    <DataMember>
    Public Property AipProjectNumber As String Implements IJobInformation.AipProjectNumber
    <DataMember>
    Public Property Sponsor As String Implements IJobInformation.Sponsor
    <DataMember>
    Public Property DesignEngineer As String Implements IJobInformation.DesignEngineer
    <DataMember>
    Public Property Description As String Implements IJobInformation.Description
    <DataMember>
    Public Property FrostDepth As IDimensionalProperty Implements IJobInformation.FrostDepth

    Public Property FrostDepthValdiation As IValidationRange Implements IJobInformation.FrostDepthValdiation

    <DataMember>
    Public Property SurfaceDrainage As String Implements IJobInformation.SurfaceDrainage
    <DataMember>
    Public Property FrostDesign As String Implements IJobInformation.FrostDesign
    <DataMember>
    Public Property Comments As String Implements IJobInformation.Comments
    <DataMember>
    Public Property Network As String Implements IJobInformation.Network
    <DataMember>
    Public Property Branch As String Implements IJobInformation.Branch
    <DataMember>
    Public Property Section As String Implements IJobInformation.Section
    <DataMember>
    Public Property Factory As IFaarFieldModelFactory Implements IJobInformation.Factory
    ''' <summary>
    ''' This is the default constructor that is used by the JobInformation class.
    ''' </summary>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' This is one of the alternate constructors for the JobInformation class.
    ''' It is meant to be used when there are frost depth values to be initialized.
    ''' </summary>
    ''' <param name="factory">This is the factory object passed in to create the frost depth values.</param>
    Public Sub New(factory As IFaarFieldModelFactory)
        FrostDepth = factory.CreateThickness(0, factory.CreateUsCustomary())
        FrostDepthValdiation = factory.CreateThicknessValidationRange(0, 200, True, New UsCustomary)
    End Sub

    ''' <summary>
    ''' This is another alternate constructor for the JobInformation class.
    ''' It is meant to be used when there is another JobInformation class that exists so the values can be copied.
    ''' </summary>
    ''' <param name="factory">This is the factory object that is passed in to create certain values.</param>
    ''' <param name="existing">This is the already existing JobInformation object that gets its values copied.</param>
    Public Sub New(factory As IFaarFieldModelFactory, existing As IJobInformation)
        LocationIdentifier = New String(existing.LocationIdentifier)
        State = New String(existing.State)
        City = New String(existing.City)
        Country = New String(existing.Country)
        Airport = New String(existing.Airport)
        AipProjectNumber = New String(existing.AipProjectNumber)
        Sponsor = New String(existing.Sponsor)
        DesignEngineer = New String(existing.DesignEngineer)
        Description = New String(existing.Description)
        FrostDepth = factory.CreateThickness(existing.FrostDepth.UsCustomary, factory.CreateUsCustomary())
        If FrostDepthValdiation Is Nothing Then
            FrostDepthValdiation = factory.CreateThicknessValidationRange(0, 200, True, New UsCustomary)
        End If
        FrostDepthValdiation = factory.CreateThicknessValidationRange(FrostDepthValdiation)
        SurfaceDrainage = existing.SurfaceDrainage
        FrostDesign = existing.FrostDesign
        Comments = existing.Comments
        Network = existing.Network
        Branch = existing.Branch
        Section = existing.Section

    End Sub

    Public Function Validate(measurementSystem As IMeasurmentSystem) As List(Of IValidation) Implements IModelAction.Validate
        Dim messages As New List(Of IValidation)

        If FrostDepthValdiation Is Nothing Then
            FrostDepthValdiation = Factory.CreateThicknessValidationRange(0, 240, True, New UsCustomary)
        End If

        Dim message = FrostDepthValdiation.Validate(FrostDepth.GetValue(measurementSystem), "Form5100JobInformation", "average frost penetration depth", "MessageFrostPenetration", measurementSystem)
        If message IsNot Nothing Then
            messages.Add(message)
        End If
        Return messages
    End Function
End Class
