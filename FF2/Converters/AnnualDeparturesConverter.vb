Imports System.Globalization
Imports FaarFieldModel.Interfaces

Namespace Converters

    Public Class AnnualDeparturesConverter
        Implements IMultiValueConverter

        Private Property MeasurementSystem As IMeasurmentSystem
        Private Property DimensionalProperty As IDimensionalProperty
        Private Property Previous As Object

        Public Function Convert(values() As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IMultiValueConverter.Convert
            If values(0) Is DependencyProperty.UnsetValue Or values(1) Is DependencyProperty.UnsetValue Or values(2) Is DependencyProperty.UnsetValue Then
                Return 0
            End If
            Dim totalAnnualDepartures As Integer
            totalAnnualDepartures = 0
            Dim life As Integer = values(2)
            Dim i As Integer
            For i = 0 To life - 1
                totalAnnualDepartures += values(0) * Math.Pow((1 + values(1) / 100), i)
            Next
            Return totalAnnualDepartures.ToString("N0")
        End Function

        Public Function ConvertBack(value As Object, targetTypes() As Type, parameter As Object, culture As CultureInfo) As Object() Implements IMultiValueConverter.ConvertBack
            'Readonly.  Does not convert back
            Throw New NotImplementedException()
        End Function
    End Class
End NameSpace