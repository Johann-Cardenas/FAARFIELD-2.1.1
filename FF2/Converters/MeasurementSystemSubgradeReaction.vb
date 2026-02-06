Imports System.Globalization
Imports FaarFieldModel
Namespace Converters
    Public Class MeasurementSystemSubgradeReaction
        Implements IValueConverter
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value.GetType Is GetType(UsCustomary) Then
                Return "k (pci)"
            Else
                Return "k (MN/m^3)"
            End If
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
