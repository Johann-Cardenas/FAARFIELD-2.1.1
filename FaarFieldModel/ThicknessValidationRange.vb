Imports FaarFieldModel.Interfaces

Public Class ThicknessValidationRange
    Implements IValidationRange

    Public  Property Maximum As IDimensionalProperty
    Public  Property Minimum As IDimensionalProperty
    Public Property Required As Boolean Implements IValidationRange.Required
    Private Property Factory As IFaarFieldModelFactory

    ''' <summary>
    ''' This is the constructor for the ThicknessValidationRange.
    ''' It initializes the properties in this class.
    ''' </summary>
    ''' <param name="factory"></param>
    ''' <param name="minimum"></param>
    ''' <param name="maximum"></param>
    ''' <param name="required"></param>
    ''' <param name="measurementSystem"></param>
    Sub New(factory As IFaarFieldModelFactory, minimum As Double, maximum As Double, required As Boolean, measurementSystem As IMeasurmentSystem)
        Me.Minimum = factory.CreateThickness(minimum, measurementSystem)
        Me.Maximum = factory.CreateThickness(maximum, measurementSystem)
        Me.Required = required
        Me.Factory = factory
    End Sub

    Sub New(factory As IFaarFieldModelFactory, validation As ThicknessValidationRange)
        Minimum = factory.CreateThickness(validation.Minimum.GetValue(New UsCustomary), New UsCustomary)
        Maximum = factory.CreateThickness(validation.Maximum.GetValue(New UsCustomary), New UsCustomary)
        Required = validation.Required
    End Sub

    Public Function Validate(value As Double, form As String, control As String, label As String, measurementSystem As IMeasurmentSystem) As IValidation Implements IValidationRange.Validate
        If value < Minimum.GetValue(measurementSystem) Or value > Maximum.GetValue(measurementSystem) Then
            Return Factory.CreateValidationMessage(form, control, label, "The value for " + control + " is out of range of " + Minimum.GetValue(measurementSystem).ToString() + " to " + Maximum.GetValue(measurementSystem).ToString() + ".")
        Else
            Return Nothing
        End If
    End Function
End Class
