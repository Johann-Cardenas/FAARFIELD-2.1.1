Imports System.Reflection
Imports System.Windows.Interactivity

Namespace Utilities

    Public Class EventToCommandBehavior
        Inherits Behavior(Of FrameworkElement)

        Private _handler As [Delegate]
        Private _oldEvent As EventInfo

        Public Property [Event] As String
            Get
                Return CStr(GetValue(EventProperty))
            End Get
            Set(ByVal value As String)
                SetValue(EventProperty, value)
            End Set
        End Property

        Public Shared ReadOnly EventProperty As DependencyProperty = DependencyProperty.Register("Event", GetType(String), GetType(EventToCommandBehavior), New PropertyMetadata(Nothing, AddressOf OnEventChanged))

        Public Property Command As ICommand
            Get
                Return CType(GetValue(CommandProperty), ICommand)
            End Get
            Set(ByVal value As ICommand)
                SetValue(CommandProperty, value)
            End Set
        End Property

        Public Shared ReadOnly CommandProperty As DependencyProperty = DependencyProperty.Register("Command", GetType(ICommand), GetType(EventToCommandBehavior), New PropertyMetadata(Nothing))

        Public Property PassArguments As Boolean
            Get
                Return CBool(GetValue(PassArgumentsProperty))
            End Get
            Set(ByVal value As Boolean)
                SetValue(PassArgumentsProperty, value)
            End Set
        End Property

        Public Shared ReadOnly PassArgumentsProperty As DependencyProperty = DependencyProperty.Register("PassArguments", GetType(Boolean), GetType(EventToCommandBehavior), New PropertyMetadata(False))

        Private Shared Sub OnEventChanged(ByVal d As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
            Dim beh = CType(d, EventToCommandBehavior)
            If beh.AssociatedObject IsNot Nothing Then beh.AttachHandler(CStr(e.NewValue))
        End Sub

        Protected Overrides Sub OnAttached()
            AttachHandler(Me.[Event])
        End Sub

        Private Sub AttachHandler(ByVal eventName As String)
            If _oldEvent IsNot Nothing Then _oldEvent.RemoveEventHandler(Me.AssociatedObject, _handler)

            If Not String.IsNullOrEmpty(eventName) Then
                Dim ei As EventInfo = Me.AssociatedObject.[GetType]().GetEvent(eventName)

                If ei IsNot Nothing Then
                    Dim mi As MethodInfo = Me.[GetType]().GetMethod("ExecuteCommand", BindingFlags.Instance Or BindingFlags.NonPublic)
                    _handler = [Delegate].CreateDelegate(ei.EventHandlerType, Me, mi)
                    ei.AddEventHandler(Me.AssociatedObject, _handler)
                    _oldEvent = ei
                Else
                    Throw New ArgumentException(String.Format("The event '{0}' was not found on type '{1}'", eventName, Me.AssociatedObject.[GetType]().Name))
                End If
            End If
        End Sub

        Private Sub ExecuteCommand(ByVal sender As Object, ByVal e As EventArgs)
            Dim parameter As Object = If(Me.PassArguments, e, Nothing)

            If Me.Command IsNot Nothing Then
                If Me.Command.CanExecute(parameter) Then Me.Command.Execute(parameter)
            End If
        End Sub

    End Class

End Namespace