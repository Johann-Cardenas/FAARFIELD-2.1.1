Imports System.ComponentModel
Imports System.Runtime.Serialization


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
Public Class UserDefinedWheel
    Implements INotifyPropertyChanged
    Implements IUserDefinedWheel

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

    Public Property id As Integer Implements IUserDefinedWheel.id

    Private _TireX As Thickness
    <DataMember>
    Public Property TireX As Thickness Implements IUserDefinedWheel.TireX
        Get
            Return _TireX
        End Get
        Set(value As Thickness)
            _TireX = value
            If TireY Is Nothing Then
                TireY = factory.CreateThickness(0, factory.CreateUsCustomary)
            End If
            OnPropertyChanged(NameOf(TireX))
        End Set
    End Property

    Private _TireY As Thickness
    <DataMember>
    Public Property TireY As Thickness Implements IUserDefinedWheel.TireY
        Get
            Return _TireY
        End Get
        Set(value As Thickness)
            _TireY = value
            If TireX Is Nothing Then
                TireX = factory.CreateThickness(0, factory.CreateUsCustomary)
            End If
            OnPropertyChanged(NameOf(TireY))
        End Set
    End Property

    <DataMember>
    Public Property ContactArea As Double Implements IUserDefinedWheel.ContactArea

    ''' <summary>
    ''' This is the default constructor for the UserDefinedWheel class.
    ''' </summary>
    Public Sub New()

    End Sub

End Class
