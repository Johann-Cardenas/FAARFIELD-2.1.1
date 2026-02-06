Imports FaarFieldModel.Interfaces

Public Class ValidationMessage
    Implements IValidation

    Public Property Form As String Implements IValidation.Form
    Public Property Label As String Implements IValidation.Label
    Public Property Control As String Implements IValidation.Control
    Public Property Message As String Implements IValidation.Message

    Public Sub New(form As String, control As String, label As String, message As String)
        Me.Form = form
        Me.Label = label
        Me.Control = control
        Me.Message = message
    End Sub
End Class
