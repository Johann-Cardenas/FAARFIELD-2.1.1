Imports System.Globalization
Imports FaarFieldModel
Namespace Converters
    Public Class MeasurementSystemTireArea
        Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            'If value.GetType Is GetType(UsCustomary) Then
            '    Return "Tire Contact Area (in.^2)"
            'Else
            '    Return "Tire Contact Area (mm^2)"
            'End If
            If value.GetType Is GetType(UsCustomary) Then
                Return "Tire Contact Area (in.²)"
            Else
                Return "Tire Contact Area (mm²)"
            End If

        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End NameSpace