Imports System.Globalization
Namespace Converters                
    Public Class BoolToIndexConverter
        Implements IValueConverter
        Private Function IValueConverter_Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert

            If (value) Then
                Return 1
            End If
            Return 0
        End Function

        Private Function IValueConverter_ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            If value = 1 Then
                Return True
            End If
            Return False
        End Function
    End Class
End Namespace