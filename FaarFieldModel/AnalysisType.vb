Imports System.Collections.ObjectModel
Imports System.Runtime.Serialization
Imports FaarFieldModel.Interfaces
<KnownType(GetType(AnalysisType))>
<KnownType(GetType(Material))>
Public Class AnalysisType
    Implements IAnalysisType

    Public Property Index As Integer Implements IAnalysisType.Index
    Public Property Name As String Implements IAnalysisType.Name
    Public Property SubgradeReaction As Boolean Implements IAnalysisType.SubgradeReaction
    Public Property Rehabilitation As Boolean Implements IAnalysisType.Rehabilitation
    Public Property DefaultLayers As ObservableCollection(Of IMaterial) Implements IAnalysisType.DefaultLayers
    ''' <summary>
    ''' This is the default constructor of the AnalysisType.
    ''' Sets all the values to default.
    ''' </summary>
    Private Sub New()
        Index = 0
        Name = String.Empty
        SubgradeReaction = False
        Rehabilitation = False
        DefaultLayers = New ObservableCollection(Of IMaterial)()
    End Sub

    ''' <summary>
    ''' This is an alternate constructor.
    ''' It is used when there is a new kind of analaysis type.
    ''' </summary>
    ''' <param name="index">An integer variable used to represent the index of the analysis type in the list.</param>
    ''' <param name="name">A string variable used to represent the name of the analysis type. </param>
    ''' <param name="subgradeReaction">A boolean varaible created to determine if an analysis type has a subgrade reaction.</param>
    ''' <param name="rehabilitation">A boolean variable created to determine if an analysis type is rehabilitized.</param>
    ''' <param name="layers">This is the collection of materials meant to represent the layers within an analysis type.</param>
    Public Sub New(index As Integer, name As String, subgradeReaction As Boolean, rehabilitation As Boolean, layers As ObservableCollection(Of IMaterial))
        Me.Index = index
        Me.Name = name
        Me.SubgradeReaction = subgradeReaction
        Me.Rehabilitation = rehabilitation
        DefaultLayers = layers
    End Sub

    ''' <summary>
    ''' This is the an alternate constructor.
    ''' It is used when there is a new analysis type that it being copied from an already existing analysis type.
    ''' </summary>
    ''' <param name="factory">This the factory object used for copying the values.</param>
    ''' <param name="existing">This is the already existing analysis type that is passed in.</param>
    Sub New(factory As IFaarFieldModelFactory, existing As IAnalysisType)
        If existing Is Nothing Then
            Index = 0
            Name = String.Empty
            SubgradeReaction = False
            Rehabilitation = False
            DefaultLayers = New ObservableCollection(Of IMaterial)()
        End If

        Index = existing.Index
        Name = New String(existing.Name)
        SubgradeReaction = existing.SubgradeReaction
        Rehabilitation = existing.Rehabilitation
        DefaultLayers = New ObservableCollection(Of IMaterial)()

        For Each layer In existing.DefaultLayers
            DefaultLayers.Add(factory.CreateMaterial(factory, layer, layer.CanDelete))
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class
