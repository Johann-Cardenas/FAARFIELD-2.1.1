Public Class DelegateCommand
    Implements ICommand

    Private ReadOnly _canExecute As Func(Of Object, Boolean)
    Private ReadOnly _executeAction As Action(Of Object)
    Private _canExecuteCache As Boolean

    Public Event CanExecuteChanged(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommand.CanExecuteChanged

    Public Sub New(executeAction As Action(Of Object), canExecute As Func(Of Object, Boolean))
        _executeAction = executeAction
        _canExecute = canExecute
    End Sub

    Public Function CanExecute(ByVal parameter As Object) As Boolean Implements ICommand.CanExecute
        Dim temp As Boolean = _canExecute(parameter)
        If _canExecuteCache <> temp Then
            _canExecuteCache = temp
            RaiseEvent CanExecuteChanged(Me, New EventArgs())
        End If
        Return _canExecuteCache
    End Function

    Public Sub Execute(ByVal parameter As Object) Implements ICommand.Execute
        _executeAction(parameter)
    End Sub

End Class


Public Class DelegateCommand(Of T)
    Implements ICommand

    Private ReadOnly _canExecute As Predicate(Of T)
    Private ReadOnly _execute As Action(Of T)

    Public Sub New(ByVal execute As Action(Of T))
        Me.New(execute, Nothing)
        _execute = execute
    End Sub

    Public Sub New(ByVal execute As Action(Of T), ByVal canExecute As Predicate(Of T))
        _execute = execute
        _canExecute = canExecute
    End Sub

    Public Function CanExecute(ByVal parameter As Object) As Boolean Implements ICommand.CanExecute
        Return _canExecute Is Nothing OrElse _canExecute(CType(parameter, T))
    End Function

    Public Sub Execute(ByVal parameter As Object) Implements ICommand.Execute
        _execute(CType(parameter, T))
    End Sub

    Public Event CanExecuteChanged(ByVal sender As Object, ByVal e As System.EventArgs) Implements ICommand.CanExecuteChanged

End Class






