Imports System.Globalization
Imports FaarFieldModel
Namespace Converters
    Public Class MeasurementSystemUpdateModulus
        Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value.GetType Is GetType(UsCustomary) Then
                Return "Update Modulus (psi)"
            Else
                Return "Update Modulus (MPa)"
            End If
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
