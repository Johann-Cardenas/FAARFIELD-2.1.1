Imports System.Globalization
Imports FaarFieldModel
Namespace Converters
    Public Class MeasurementSystemTirePressure
        Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value.GetType Is GetType(UsCustomary)
                Return "Tire Pressure (psi)"
            Else
                Return "Tire Pressure (kPa)"
            End If
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End NameSpace