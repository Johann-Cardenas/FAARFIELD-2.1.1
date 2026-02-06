Imports System.Globalization
Imports FaarFieldModel

Namespace Converters
    Public Class RigidVisibilityConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            'if nothing passed to converter then return hidden
            If value Is Nothing Then
                Return Visibility.Hidden
            End If

            'Cast to the analysis type to use the object
            Dim analysisType = CType(value, AnalysisType)

            'if the name of the analysistype is one of the following
            If analysisType.Name = "New Flexible" Or analysisType.Name = "HMA on Aggregate" Or analysisType.Name = "HMA Overlay on Flexible" Then
                Return Visibility.Hidden
            Else
                Return Visibility.Visible
            End If

        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace