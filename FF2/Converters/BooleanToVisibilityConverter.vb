Imports System.Globalization

Namespace Converters

    Public NotInheritable Class BooleanToVisibilityConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            If TypeOf value Is Boolean AndAlso DirectCast(value, Boolean) Then Return Visibility.Visible
            Return Visibility.Collapsed
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Return TypeOf value Is Visibility AndAlso DirectCast(value, Visibility) = Visibility.Visible
        End Function

    End Class

End Namespace
