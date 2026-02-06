Imports System.Collections.ObjectModel

Namespace Interfaces
    Public Interface IAnalysisType
        Property Index As Integer
        Property Name As String
        Property SubgradeReaction As Boolean
        Property Rehabilitation As Boolean
        Property DefaultLayers As ObservableCollection(Of IMaterial)
    End Interface
End Namespace