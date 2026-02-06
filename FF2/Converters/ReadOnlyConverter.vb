Imports System.Globalization

Namespace Converters

    Public Class ReadOnlyConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If value Is Nothing Then
                Return False
            End If
            Return True
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException
        End Function
    End Class
End NameSpace