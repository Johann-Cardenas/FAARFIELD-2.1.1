Imports System.Globalization
Imports FaarFieldModel
Namespace Converters
    Public Class MeasurementSystemWarningFrostDepth
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value.GetType Is GetType(UsCustomary)
                Return "Average frost penetration can range from 0 to 240 inches."
            Else
                Return "Average frost penetration can range from 0 to 600 centimeters"
            End If
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace