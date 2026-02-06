Imports System.Globalization
Imports FaarFieldModel
Namespace Converters
    Public Class ACRVisibility1Converter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            'if nothing passed to converter then return hidden
            If value = 3 Then
                Return Visibility.Visible
            Else
                Return Visibility.Hidden
            End If

        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
