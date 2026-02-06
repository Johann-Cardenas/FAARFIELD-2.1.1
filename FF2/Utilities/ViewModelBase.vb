Imports System
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices

Namespace Utilities

    Public MustInherit Class ViewModelBase
        Implements INotifyPropertyChanged

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
    End Class

End Namespace