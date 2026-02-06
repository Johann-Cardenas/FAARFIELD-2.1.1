Imports FaarFieldModel.Interfaces

Public Class LengthValidationRange
    Implements IValidationRange

    Public Property Maximum As IDimensionalProperty
    Public Property Minimum As IDimensionalProperty
    Public Property Required As Boolean Implements IValidationRange.Required
    Private Property Factory As IFaarFieldModelFactory

    ''' <summary>
    ''' This is one of the constructors for the LengthValidationRange class.
    ''' It is used to initialize the values with the parameters passed in.
    ''' </summary>
    ''' <param name="factory">The factory object that has its methods used.</param>
    ''' <param name="minimum">The minimum value that is created from the length method in factory.</param>
    ''' <param name="maximum">The maximum value that is created from the length method in factory.</param>
    ''' <param name="required">A boolean that represents if the validation is required.</param>
    ''' <param name="measurementSystem">This the measurement system that the length is using.</param>
    Sub New(factory As IFaarFieldModelFactory, minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem)
        Me.Minimum = factory.CreateLength(minimum, measurementSystem)
        Me.Maximum = factory.CreateLength(maximum, measurementSystem)
        Me.Required = required
        Me.Factory = factory
    End Sub
    ''' <summary>
    ''' This is the second constructor for the LengthValidationRange class.
    ''' It is used to create the minimum and maximum values.
    ''' </summary>
    ''' <param name="factory">This is the factory object that uses methods to create the length for the minimum and maximum variables. </param>
    ''' <param name="validation">This is the validation object that gets its Required value copied into the Required variable in this class.</param>
    Sub New(factory As IFaarFieldModelFactory, validation As LengthValidationRange)
        Minimum = factory.CreateLength(validation.Minimum.GetValue(New UsCustomary), New UsCustomary)
        Maximum = factory.CreateLength(validation.Maximum.GetValue(New UsCustomary), New UsCustomary)
        Required = validation.Required
    End Sub
    ''' <summary>
    ''' This is the validation method that checks the length and verifies that it is between the minimum and maximum.
    ''' </summary>
    ''' <param name="value">The value that is being checked.</param>
    ''' <param name="form">The form that is passed in as a string.</param>
    ''' <param name="control">The name of the value that is tested.</param>
    ''' <param name="label">The label of the message that is shown.</param>
    ''' <param name="measurementSystem">This is the measurement system that is used to validate the length.</param>
    ''' <returns></returns>
    Public Function Validate(value As Double, form As String, control As String, label As String, measurementSystem As IMeasurmentSystem) As IValidation Implements IValidationRange.Validate
        If value < Minimum.GetValue(measurementSystem) Or value > Maximum.GetValue(measurementSystem) Then
            Return Factory.CreateValidationMessage(form, control, label, "Value is out of range of " + Minimum.GetValue(measurementSystem) + " to " + Maximum.GetValue(measurementSystem) + ".")
        Else
            Return Nothing
        End If
    End Function
End Class
