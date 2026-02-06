Public Class PCRGraphItem

    Private _AircraftName As String
    Public Property AircraftName() As String
        Get
            Return _AircraftName
        End Get
        Set(ByVal value As String)
            _AircraftName = value
        End Set
    End Property

    Private _ACRB As Double
    Public Property ACRB() As Double
        Get
            Return _ACRB
        End Get
        Set(ByVal value As Double)
            _ACRB = value
        End Set
    End Property

    Private _Departures As Integer
    Public Property Departures() As Integer
        Get
            Return _Departures
        End Get
        Set(ByVal value As Integer)
            _Departures = value
        End Set
    End Property

End Class