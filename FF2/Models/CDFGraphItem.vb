Public Class CDFGraphItem

    Private _AircraftName As String
    Public Property AircraftName() As String
        Get
            Return _AircraftName
        End Get
        Set(ByVal value As String)
            _AircraftName = value
        End Set
    End Property

    Private _AircraftCDF As Single
    Public Property AircraftCDF() As Single
        Get
            Return _AircraftCDF
        End Get
        Set(ByVal value As Single)
            _AircraftCDF = value
        End Set
    End Property

    Private _CDFGraphData As List(Of Single)
    Public Property CDFGraphData() As List(Of Single)
        Get
            Return _CDFGraphData
        End Get
        Set(ByVal value As List(Of Single))
            _CDFGraphData = value
        End Set
    End Property

    Private _XRPlotData As List(Of Single)
    Public Property XRPlotData() As List(Of Single)
        Get
            Return _XRPlotData
        End Get
        Set(ByVal value As List(Of Single))
            _XRPlotData = value
        End Set
    End Property

    Private _XLPlotData As List(Of Single)
    Public Property XLPlotData() As List(Of Single)
        Get
            Return _XLPlotData
        End Get
        Set(ByVal value As List(Of Single))
            _XLPlotData = value
        End Set
    End Property

    Private _YPlotData As List(Of Single)
    Public Property YPlotData() As List(Of Single)
        Get
            Return _YPlotData
        End Get
        Set(ByVal value As List(Of Single))
            _YPlotData = value
        End Set
    End Property



End Class