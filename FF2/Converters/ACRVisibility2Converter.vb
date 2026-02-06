Imports System.Globalization
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Namespace Converters

    Public Class ACRVisibility2Converter
        Implements IMultiValueConverter

        Private Property MeasurementSystem As IMeasurmentSystem
        Private Property DimensionalProperty As IDimensionalProperty
        Private Property Previous As Object

        Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
            If values(1) Is DependencyProperty.UnsetValue Or values(0) Is DependencyProperty.UnsetValue Then
                Return Visibility.Hidden
            End If
            'Previous = values(0)
            'DimensionalProperty = CType(values(0), IDimensionalProperty)
            'MeasurementSystem = CType(values(1), IMeasurmentSystem)
            If values(0) = 3 Then
                If values(1) = True Then
                    Return Visibility.Visible
                Else
                    Return Visibility.Hidden
                End If
            Else
                Return Visibility.Hidden


            End If


        End Function

        Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack

            Try
                Return {New Thickness(value, MeasurementSystem), MeasurementSystem}
            Catch ex As Exception
                Return {Previous, MeasurementSystem}
            End Try


        End Function
    End Class
End Namespace
