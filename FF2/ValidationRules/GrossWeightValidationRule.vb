

Imports FaarFieldModel.Interfaces

Namespace ValidationRules
    Public Class GrossWeightValidationRule
        Inherits ValidationRule
        Public Property AirplaneName As String
        Public Property MeasurementSystem As IMeasurmentSystem
        Public Overrides Function Validate(ByVal value As Object, ByVal cultureInfo As System.Globalization.CultureInfo) As ValidationResult
            Dim result As Integer

            If Not Integer.TryParse(value, result) Then

            End If


            Return ValidationResult.ValidResult

        End Function

    End Class




End Namespace

