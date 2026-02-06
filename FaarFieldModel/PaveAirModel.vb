Imports System.Collections.ObjectModel
Imports System.Runtime.Serialization
Imports FaarFieldModel.Interfaces

Public Class Database
    Implements IDatabase

    Public Property Name As String Implements IDatabase.Name

    Private Sub New()
        Name = String.Empty
    End Sub

    Public Sub New(name As String)
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class

Public Class Network
    Implements INetwork

    Public Property Id As Integer Implements INetwork.Id
    Public Property Name As String Implements INetwork.Name

    Private Sub New()
        Id = 0
        Name = String.Empty
    End Sub

    Public Sub New(id As Integer, name As String)
        Me.Id = id
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class

Public Class Branch
    Implements IBranch

    Public Property Id As Integer Implements IBranch.Id
    Public Property Name As String Implements IBranch.Name

    Private Sub New()
        Id = 0
        Name = String.Empty
    End Sub

    Public Sub New(id As Integer, name As String)
        Me.Id = id
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class

Public Class PaveairSection
    Implements IPaveairSection

    Public Property Id As Integer Implements IPaveairSection.Id
    Public Property Name As String Implements IPaveairSection.Name

    Private Sub New()
        Id = 0
        Name = String.Empty
    End Sub

    Public Sub New(id As Integer, name As String)
        Me.Id = id
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Name
    End Function

End Class

Public Class JobDownload
    Implements IJobDownload

    Public Property Id As Integer Implements IJobDownload.Id
    Public Property Name As String Implements IJobDownload.Name

    Private Sub New()
        Id = 0
        Name = String.Empty
    End Sub

    Public Sub New(id As Integer, name As String)
        Me.Id = id
        Me.Name = name
    End Sub

    Public Overrides Function ToString() As String
        Return Name & $" #{Id}"
    End Function

End Class
