Imports System.Globalization
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Namespace Converters
    Public Class ModulusEnabledConverter
        Implements IValueConverter

        Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
            'if nothing passed to converter then return hidden
            If value Is Nothing Then
                Return Visibility.Hidden
            End If

            'Cast to the analysis type to use the object
            Dim material = value.ToString()


            'if the name of the analysistype is one of the following
            If material = "Subgrade" Then

                Return True

            ElseIf material = "Variable (rigid)" Then
                Return True

            ElseIf material = "Variable (flexible)" Then
                Return True

            ElseIf material = "User Defined" Then
                Return True

            Else
                Return False
            End If

        End Function

        Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace