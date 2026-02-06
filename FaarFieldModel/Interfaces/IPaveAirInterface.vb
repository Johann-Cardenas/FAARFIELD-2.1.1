Imports System.Collections.ObjectModel

Namespace Interfaces
    Public Interface IDatabase
        Property Name As String
    End Interface

    Public Interface INetwork
        Property Id As Integer
        Property Name As String
    End Interface

    Public Interface IBranch
        Property Id As Integer
        Property Name As String
    End Interface

    Public Interface IPaveairSection
        Property Id As Integer
        Property Name As String
    End Interface

    Public Interface IJobDownload
        Property Id As Integer
        Property Name As String
    End Interface
End Namespace