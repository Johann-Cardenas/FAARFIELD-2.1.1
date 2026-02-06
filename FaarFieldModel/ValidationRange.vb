Imports FaarFieldModel.Interfaces

Public Class ValidationRange
    Implements IValidationRange
    Public  Property Maximum As Double
    Public  Property Minimum As Double
    Public Property Required As Boolean Implements IValidationRange.Required
    Private Factory As IFaarFieldModelFactory

    Sub New(factory As IFaarFieldModelFactory, minimum As Double, maximum As Double, required As Boolean)
        Me.Minimum = minimum
        Me.Maximum = maximum
        Me.Required = required
        Me.Factory = factory
    End Sub

    Public Function Validate(value As Double, form As String, control As String, label As String, measurementSystem As IMeasurmentSystem) As IValidation Implements IValidationRange.Validate
        If value < Minimum Or value > Maximum Then
            Return Factory.CreateValidationMessage(form, control, label, "The value for " + control + " is out of range of " + Minimum.ToString() + " to " + Maximum.ToString() + ".")
        Else
            Return Nothing
        End If
    End Function
End Class
