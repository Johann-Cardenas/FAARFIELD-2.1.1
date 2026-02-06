Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports FaarFieldModel.Interfaces
Imports FaarFieldModel



<TestClass()> Public Class UnitTest1

#Region "TestAreaCreation"

    <TestMethod()> Public Sub TestAreaCreation()

        'create US and Metric coordinate systems from a FAARFIELD model factory
        'set the coordinate value
        'retrieve it from US and metric coordinate systems

        Dim value As Double = 100

        Dim factory As IFaarFieldModelFactory = New FaarFieldModelFactory

        Dim USCoordinateSystem As IMeasurmentSystem = factory.CreateUsCustomary()
        Dim MetricCoordinateSystem As IMeasurmentSystem = factory.CreateMetric()

        Dim area = New Area(value, USCoordinateSystem)

        Dim result1 As Double = area.GetValue(USCoordinateSystem)
        Dim result2 As Double = area.GetValue(MetricCoordinateSystem)

        Assert.AreEqual(result1, value)
        Assert.AreEqual(result2, value * 25.4 * 25.4)

    End Sub

#End Region

    <TestMethod()> Public Sub TestMethod1()

        'stub to be replaced

        Dim result As Boolean = True

        Assert.AreEqual(result, True)


    End Sub

    <TestMethod()> Public Sub TestMethod2()

        'stub to be replaced

        Dim result As Boolean = True

        Assert.AreEqual(result, True)


    End Sub

End Class