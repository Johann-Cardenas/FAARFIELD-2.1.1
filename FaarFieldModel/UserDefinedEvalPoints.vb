Imports System.Runtime.Serialization
Imports System.ComponentModel

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
Public Class UserDefinedEvalPoints
    Implements INotifyPropertyChanged
    Implements IUserDefinedEvalPoints

    Private factory = New FaarFieldModelFactory

    Private ReadOnly _argsCache As Dictionary(Of String, PropertyChangedEventArgs) = New Dictionary(Of String, PropertyChangedEventArgs)()

    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Protected Overridable Sub OnPropertyChanged(ByVal propertyName As String)
        If _argsCache IsNot Nothing Then
            If Not _argsCache.ContainsKey(propertyName) Then _argsCache(propertyName) = New PropertyChangedEventArgs(propertyName)
            NotifyChange(_argsCache(propertyName))
        End If
    End Sub

    Private Sub NotifyChange(ByVal e As PropertyChangedEventArgs)
        RaiseEvent PropertyChanged(Me, e)
    End Sub

    Public Property id As Integer Implements IUserDefinedEvalPoints.id

    Private _EvalX As Thickness
    <DataMember>
    Public Property EvalX As Thickness Implements IUserDefinedEvalPoints.EvalX
        Get
            Return _EvalX
        End Get
        Set(value As Thickness)
            _EvalX = value
            If EvalY Is Nothing Then
                EvalY = factory.CreateThickness(0, factory.CreateUsCustomary)
            End If
            OnPropertyChanged(NameOf(EvalX))
        End Set
    End Property

    Private _EvalY As Thickness
    <DataMember>
    Public Property EvalY As Thickness Implements IUserDefinedEvalPoints.EvalY
        Get
            Return _EvalY
        End Get
        Set(value As Thickness)
            _EvalY = value
            If EvalX Is Nothing Then
                EvalX = factory.CreateThickness(0, factory.CreateUsCustomary)
            End If
            OnPropertyChanged(NameOf(EvalY))
        End Set
    End Property

    <DataMember>
    Public Property ContactArea As Double Implements IUserDefinedEvalPoints.ContactArea

    ''' <summary>
    ''' This is the default constructor for the UserDefinedEvalPoints class.
    ''' </summary>
    Public Sub New()

    End Sub
End Class
