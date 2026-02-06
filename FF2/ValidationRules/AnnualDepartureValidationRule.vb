Imports System.Globalization

Namespace ValidationRules
    Public Class AnnualDepartureValidationRule
        Inherits ValidationRule

        Public Overrides Function Validate(ByVal value As Object, ByVal cultureInfo As System.Globalization.CultureInfo) As ValidationResult
            Dim result As Integer

            If Not Integer.TryParse(value, result) Then
                MessageBox.Show("Annual departures must be an integer greater than 0 and less than or equal to 100,000.")
                Return New ValidationResult(False, "Annual departures must be number")
            End If

            If value > 100000 Then
                MessageBox.Show("Annual departures cannot be greater than 100,000.")

                Return New ValidationResult(False, "Annual departures cannot be greater than 100,000")
            End If
            Return ValidationResult.ValidResult

        End Function

    End Class
End Namespace
