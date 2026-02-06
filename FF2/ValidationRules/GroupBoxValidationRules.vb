
Namespace ValidationRules

    Public Class GroupBoxValidationRules
        Inherits ValidationRule



        Public Overrides Function Validate(ByVal value As Object,
                                           ByVal cultureInfo As System.Globalization.CultureInfo) As ValidationResult
            Return ValidationResult.ValidResult

        End Function

    End Class
End Namespace