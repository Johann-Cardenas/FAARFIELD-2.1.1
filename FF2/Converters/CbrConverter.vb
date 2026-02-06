Imports System.Globalization
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Namespace Converters

    Public Class CbrConverter
        Implements IMultiValueConverter

        Private Property MeasurementSystem As IMeasurmentSystem
        Private Property DimensionalProperty As IDimensionalProperty
        Private Property Previous As Object

        Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
            If values(0) Is Nothing
                Return ""
            End If

            If values(0) Is DependencyProperty.UnsetValue
                Return ""
            End If

            MeasurementSystem = CType(values(1),IMeasurmentSystem)
            If values(0) = 0
                Return ""
            End If

            Return System.Convert.ToDouble(values(0)).ToString("0.##")
        End Function

        Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
            return {System.Convert.ToDouble(value), MeasurementSystem}
        End Function
    End Class
End Namespace