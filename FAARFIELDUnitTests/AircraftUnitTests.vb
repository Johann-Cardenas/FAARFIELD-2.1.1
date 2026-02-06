Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports FaarFieldModel.Interfaces
Imports FaarFieldModel
Imports FF2.Libs
Imports FF2.ViewModels
Imports FF2.Utilities

Imports System.Runtime.Serialization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security.Cryptography
Imports FF2

<TestClass()> Public Class AircraftUnitTests

#Region "AircraftListUnitTest()"

    <TestMethod()> Public Sub AircraftListUnitTest()

        Dim result As Boolean = Nothing
        Dim xmlPath = My.Application.Info.DirectoryPath + "\..\..\aircraft.xml"
        Dim vm1 As MainWindowViewModel
        Dim vm2 As MainWindowViewModel

        Dim addedAirbusBelly1 As IAirplaneInfo = Nothing
        Dim addedAirbusBelly2 As IAirplaneInfo = Nothing
        Dim addedAirbusNoBelly1 As IAirplaneInfo = Nothing

        vm1 = New MainWindowViewModel(xmlPath, True)
        For Each aircraftInfo As IAirplaneInfo In vm1.AircraftLibrary

            If aircraftInfo.Manufacturer = "Airbus" Then

                If aircraftInfo.Name = "A380-800 WV002" Then
                    addedAirbusBelly1 = aircraftInfo
                End If

                If aircraftInfo.Name = "A310-300" Then
                    addedAirbusNoBelly1 = aircraftInfo
                End If

            End If

        Next

        'second query is necessary to get a second copy of the object on to the list instead of a reference to the same object
        vm2 = New MainWindowViewModel(xmlPath, True)
        For Each aircraftInfo As IAirplaneInfo In vm2.AircraftLibrary

            If aircraftInfo.Manufacturer = "Airbus" Then

                If aircraftInfo.Name = "A380-800 WV002" Then
                    addedAirbusBelly2 = aircraftInfo
                End If

            End If

        Next

        'instantiate aircraft list 
        Dim acList As New AircraftList(xmlPath)

        acList.Add(addedAirbusBelly1)
        acList.Add(addedAirbusNoBelly1)
        acList.Add(addedAirbusBelly2)
        Assert.AreEqual(acList.AircraftList.Count(), 5)
        Assert.AreEqual(acList.AircraftList(0).Name, "A380-800 WV002")
        Assert.AreEqual(acList.AircraftList(1).Name, "A380-800 WV002 Belly")
        Assert.AreEqual(acList.AircraftList(2).Name, "A310-300")
        Assert.AreEqual(acList.AircraftList(3).Name, "A380-800 WV002")
        Assert.AreEqual(acList.AircraftList(4).Name, "A380-800 WV002 Belly")


        acList.UpdateNumberDepartures(0, 55555)
        Assert.AreEqual(True, acList.AircraftList(0).Name = "A380-800 WV002" And acList.AircraftList(0).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(1).Name = "A380-800 WV002 Belly" And acList.AircraftList(1).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(2).Name = "A310-300" And acList.AircraftList(2).NumberDepartures = 1200)
        Assert.AreEqual(True, acList.AircraftList(3).Name = "A380-800 WV002" And acList.AircraftList(3).NumberDepartures = 1200)
        Assert.AreEqual(True, acList.AircraftList(4).Name = "A380-800 WV002 Belly" And acList.AircraftList(4).NumberDepartures = 1200)

        acList.UpdateNumberDepartures(4, 77777)
        Assert.AreEqual(True, acList.AircraftList(0).Name = "A380-800 WV002" And acList.AircraftList(0).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(1).Name = "A380-800 WV002 Belly" And acList.AircraftList(1).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(2).Name = "A310-300" And acList.AircraftList(2).NumberDepartures = 1200)
        Assert.AreEqual(True, acList.AircraftList(3).Name = "A380-800 WV002" And acList.AircraftList(3).NumberDepartures = 77777)
        Assert.AreEqual(True, acList.AircraftList(4).Name = "A380-800 WV002 Belly" And acList.AircraftList(4).NumberDepartures = 77777)

        acList.UpdateNumberDepartures(2, 12345)
        Assert.AreEqual(True, acList.AircraftList(0).Name = "A380-800 WV002" And acList.AircraftList(0).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(1).Name = "A380-800 WV002 Belly" And acList.AircraftList(1).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(2).Name = "A310-300" And acList.AircraftList(2).NumberDepartures = 12345)
        Assert.AreEqual(True, acList.AircraftList(3).Name = "A380-800 WV002" And acList.AircraftList(3).NumberDepartures = 77777)
        Assert.AreEqual(True, acList.AircraftList(4).Name = "A380-800 WV002 Belly" And acList.AircraftList(4).NumberDepartures = 77777)

        result = acList.UpdateNumberDepartures(0, 123456789)
        Assert.AreEqual(result, False)
        Assert.AreEqual(acList.ErrorMessages.Count(), 1)
        Assert.AreEqual(acList.ErrorMessages(0), "Maximum allowable number of Annual Departures is 100,000")

        result = acList.UpdateNumberDepartures(0, -1)
        Assert.AreEqual(result, False)
        Assert.AreEqual(acList.ErrorMessages.Count(), 1)
        Assert.AreEqual(acList.ErrorMessages(0), "Minimum allowable number of Annual Departures is 0")

        result = acList.UpdateNumberDepartures(10, 5555)
        Assert.AreEqual(result, False)
        Assert.AreEqual(acList.ErrorMessages.Count(), 1)
        Assert.AreEqual(acList.ErrorMessages(0), "10 index is larger than the aircraft list")

        Assert.AreEqual(True, acList.AircraftList(0).Name = "A380-800 WV002" And acList.AircraftList(0).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(1).Name = "A380-800 WV002 Belly" And acList.AircraftList(1).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(2).Name = "A310-300" And acList.AircraftList(2).NumberDepartures = 12345)
        Assert.AreEqual(True, acList.AircraftList(3).Name = "A380-800 WV002" And acList.AircraftList(3).NumberDepartures = 77777)
        Assert.AreEqual(True, acList.AircraftList(4).Name = "A380-800 WV002 Belly" And acList.AircraftList(4).NumberDepartures = 77777)

        ''delete aircraft

        result = acList.Delete(10)
        Assert.AreEqual(result, False)
        Assert.AreEqual(acList.ErrorMessages.Count(), 1)
        Assert.AreEqual(acList.ErrorMessages(0), "10 index is larger than the aircraft list")

        acList.Delete(4)
        Assert.AreEqual(True, acList.AircraftList(0).Name = "A380-800 WV002" And acList.AircraftList(0).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(1).Name = "A380-800 WV002 Belly" And acList.AircraftList(1).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(2).Name = "A310-300" And acList.AircraftList(2).NumberDepartures = 12345)
        Assert.AreEqual(acList.AircraftList.Count(), 3)

        acList.Delete(2)
        Assert.AreEqual(True, acList.AircraftList(0).Name = "A380-800 WV002" And acList.AircraftList(0).NumberDepartures = 55555)
        Assert.AreEqual(True, acList.AircraftList(1).Name = "A380-800 WV002 Belly" And acList.AircraftList(1).NumberDepartures = 55555)
        Assert.AreEqual(acList.AircraftList.Count(), 2)

        acList.Delete(0)
        Assert.AreEqual(acList.AircraftList.Count(), 0)

        'a
        'Assert.AreEqual(acList.AircraftList.Count(), 0)

        ''delete belly
        'acList.Add(addedAirplane)
        'acList.Delete(acList.AircraftList(1))
        'Assert.AreEqual(acList.AircraftList.Count(), 0)




    End Sub

#End Region

#Region "RetrieveBellyInfo"

    <TestMethod()> Public Sub RetrieveBellyInfo()

        Dim status As Boolean = False
        Dim xmlPath = My.Application.Info.DirectoryPath + "\..\..\aircraft.xml"

        Dim FaarFieldFactory As IFaarFieldModelFactory = New FaarFieldModelFactory
        Dim result As Boolean = True
        Dim apInfo As AirplaneInfo = New AirplaneInfo()
        Dim AircraftLibrary As New List(Of IAirplaneInfo)

        Dim vm As MainWindowViewModel
        Dim airbusAircraftList As New List(Of IAirplaneInfo)
        Dim addedAirplane As IAirplaneInfo = Nothing
        Dim belly As IAirplaneInfo = Nothing

        Try
            vm = New MainWindowViewModel(xmlPath, True)


            For Each aircraftInfo As IAirplaneInfo In vm.AircraftLibrary

                If aircraftInfo.Manufacturer = "Airbus" Then
                    airbusAircraftList.Add(aircraftInfo)

                    If aircraftInfo.Name = "A380-800 WV002" Then
                        addedAirplane = aircraftInfo
                    End If
                End If



            Next

            belly = vm.RetrieveBellyInfo(addedAirplane)

            If belly.Name = "A380-800 WV002 Belly" Then
                result = True
            End If

        Catch ex As Exception
            result = False
        End Try



        Assert.AreEqual(result, True)

    End Sub


#End Region

End Class
