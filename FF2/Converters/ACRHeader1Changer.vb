Imports System.Globalization
Imports FaarFieldModel
Imports FF2.ViewModels
Namespace Converters
    Public Class ACRHeader1Changer
        Implements IValueConverter
        Property ViewModel As MainWindowViewModel
        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            Return value
        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End NameSpace