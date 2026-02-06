Imports System.Globalization
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Namespace Converters

    Public Class PressureConverter
        Implements IMultiValueConverter

        Private Property MeasurementSystem As IMeasurmentSystem
        Private Property DimensionalProperty As IDimensionalProperty
        Private Property Previous As Object

        Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
            If values(1) Is DependencyProperty.UnsetValue Or values(0) Is DependencyProperty.UnsetValue Or values(0) Is Nothing
                Return ""
            End If
            Previous = values(0)
            DimensionalProperty = CType(values(0),IDimensionalProperty)
            MeasurementSystem = CType(values(1),IMeasurmentSystem)
            If MeasurementSystem.GetType() Is GetType(UsCustomary) Then
                'Return DimensionalProperty.UsCustomary.ToString("f1")
                Return DimensionalProperty.UsCustomary.ToString("N0")
            Else
                'Return DimensionalProperty.Metric.ToString("f1")
                Return DimensionalProperty.Metric.ToString("f2")
            End If

        End Function

        Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
            
            Try
                return {New Pressure(value,MeasurementSystem), MeasurementSystem}
            Catch ex As Exception
                Return {Previous,MeasurementSystem}
            End Try


        End Function
    End Class
End Namespace