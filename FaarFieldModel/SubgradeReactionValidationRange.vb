Imports FaarFieldModel.Interfaces
Public Class SubgradeReactionValidationRange
    Implements IValidationRange

    Public  Property Maximum As SubgradeReaction
    Public  Property Minimum As SubgradeReaction
    Public Property Required As Boolean Implements IValidationRange.Required
    Private Property Factory As IFaarFieldModelFactory
    ''' <summary>
    ''' This is the constructor for the SubgradeReactionValidationRange class.
    ''' Its parameters are used to initialize its properties.
    ''' </summary>
    ''' <param name="factory">This is the factory object that is used.</param>
    ''' <param name="minimum">This is a double variable that is used to represent the minimum value.</param>
    ''' <param name="maximum">This is a double variable that is used to represent the maximum value.</param>
    ''' <param name="required">This is a boolean variable that represents if this validation range is required.</param>
    ''' <param name="measurementSystem">This is the measurement system that the values are used in.</param>
    Sub New (factory As IFaarFieldModelFactory, minimum As Double, maximum As Double, required As Boolean,measurementSystem As IMeasurmentSystem)
        Me.Minimum = factory.CreateSubgradeReaction(minimum,measurementSystem)
        Me.Maximum = factory.CreateSubgradeReaction(maximum, measurementSystem)
        Me.Required = required
        Me.Factory = factory
    End Sub

    Public Function Validate(value As Double, form As String, control As String, label As String, measurementSystem As IMeasurmentSystem) As IValidation Implements IValidationRange.Validate
        If value < Minimum.GetValue(measurementSystem) Or value > Maximum.GetValue(measurementSystem) Then
            Return Factory.CreateValidationMessage(form, control, label, "The value for " + control + " is out of range of " + Minimum.GetValue(measurementSystem).ToString() + " to " + Maximum.GetValue(measurementSystem).ToString() + ".")
        Else
            Return Nothing
        End If
    End Function
End Class