Imports System.Globalization
Imports FaarFieldAnalysis
Imports FaarFieldModel

Namespace Converters

    Public Class MeasurementSystemToIndexConverter
        Implements IValueConverter

        'Dim thick As Single()


        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value.GetType() Is GetType(UsCustomary) Then

                Return 0
            End If
            Return 1
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            If value = 1 Then
                Return New Metric
            End If
            Return New UsCustomary
        End Function
    End Class
End Namespace