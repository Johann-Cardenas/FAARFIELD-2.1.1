Imports FaarFieldModel.Interfaces
Public Class PressureValidationRange
    Implements IValidationRange

    ''' <summary>
    ''' Maximum for validation range
    ''' </summary>
    Public Property Maximum As Modulus

    ''' <summary>
    ''' Minimum for validation range
    ''' </summary>
    Public Property Minimum As Modulus

    ''' <summary>
    ''' This validation ranged must be run
    ''' </summary>
    Public Property Required As Boolean Implements IValidationRange.Required

    Private Property Factory As IFaarFieldModelFactory

    ''' <summary>
    ''' Constructor for validation range
    ''' </summary>
    ''' <param name="minimum">Minium for validation range</param>
    ''' <param name="maximum">Maximum for validation range</param>
    ''' <param name="required">Validation range must be checked</param>
    ''' <param name="measurementSystem">Measurement system for which minimum/maximum values entered</param>
    Sub New (factory As IFaarFieldModelFactory, minimum As Double, maximum As Double, required As Boolean,measurementSystem As IMeasurmentSystem)
        Me.Minimum = factory.CreateModulus(minimum,measurementSystem)
        Me.Maximum = factory.CreateModulus(maximum,measurementSystem)
        Me.Required = required
        Me.Factory = factory
    End Sub

    ''' <summary>
    ''' This is the validation method that checks the pressure and verifies that it is between the minimum and maximum.
    ''' </summary>
    ''' <param name="value">The value that is being checked.</param>
    ''' <param name="form">The form that is passed in as a string.</param>
    ''' <param name="control">The name of the value that is tested.</param>
    ''' <param name="label">The label of the message that is shown.</param>
    ''' <param name="measurementSystem">This is the measurement system that is used to validate the length.</param>
    Public Function Validate(value As Double, form As String, control As String, label As String, measurementSystem As IMeasurmentSystem) As IValidation Implements IValidationRange.Validate
        If value < Minimum.GetValue(measurementSystem) Or value > Maximum.GetValue(measurementSystem) Then
            Return Factory.CreateValidationMessage(form, control, label, "The value for " + control + " is out of range of " + Minimum.GetValue(measurementSystem).ToString() + " to " + Maximum.GetValue(measurementSystem).ToString() + ".")
        Else
            Return Nothing
        End If
    End Function
End Class