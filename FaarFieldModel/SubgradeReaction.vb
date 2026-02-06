Imports System.Runtime.Serialization
Imports FaarFieldModel.Interfaces
<DataContract>
Public Class SubgradeReaction
    Implements IDimensionalProperty
    <DataMember>
    Dim us As Double
    <DataMember>
    Dim si As Double 

    Dim _setMeasurementSystem As IMeasurmentSystem

    Public ReadOnly Property UsCustomary As Double Implements IDimensionalProperty.UsCustomary
       Get
          Return us
       End Get
    End Property

    Public ReadOnly Property Metric As Double Implements IDimensionalProperty.Metric
       Get
          Return si
       End Get
    End Property

    ''' <summary>
    ''' This is the default constructor for the SubgradeReaction class.
    ''' </summary>
    Private Sub New()

    End Sub

    ''' <summary>
    ''' The constructor for the SubgradeReaction class.
    ''' Uses a value and measurement syster as parameters.
    ''' Converts psi/in to MPa/m.
    ''' </summary>
    ''' <param name="value">The reaction value.</param>
    ''' <param name="measurementSystem">The measurement system used.</param>
    Public Sub New(value As Double, measurementSystem As IMeasurmentSystem)
        measurementSystem.SetValue(value, 0.271447, us, si)
    End Sub

    Public Function GetValue(measurementSystem As IMeasurmentSystem) As Double Implements IDimensionalProperty.GetValue
        Return measurementSystem.GetValue(us,si)
    End Function
End Class