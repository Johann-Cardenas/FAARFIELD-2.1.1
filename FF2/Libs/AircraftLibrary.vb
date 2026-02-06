Imports System.Collections.ObjectModel
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Json
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml.Serialization
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports Microsoft.VisualBasic.FileIO
Imports FaarFieldAnalysis
Imports System.Windows.Forms
Imports System.Xml

Namespace Libs

    <DataContract>
    <KnownType(GetType(FaarFieldJob))>
    <KnownType(GetType(AirplaneInfo))>
    <KnownType(GetType(IAirplaneInfo))>
    Public Module AircraftLibrary

        Private LibraryVersion As String
        Private SoftwareVersion As String

        Public Sub SetVersions(libVer As String, sftVer As String)
            LibraryVersion = libVer
            SoftwareVersion = sftVer
        End Sub


        Public Function ValidateSignedAircraftLibrary(aircraftLibrary As SignedAircraftLibrary) As Boolean

            Dim isSigned As Boolean = True

            Dim rsaKeyInfo = New RSAParameters()
            rsaKeyInfo.Modulus = aircraftLibrary.Modulus
            rsaKeyInfo.Exponent = aircraftLibrary.Exponent
            Dim rsaDecrypt = New RSACryptoServiceProvider()
            rsaDecrypt.ImportParameters(rsaKeyInfo)
            Dim rsaDeformatter = New RSAPKCS1SignatureDeformatter(rsaDecrypt)
            rsaDeformatter.SetHashAlgorithm("SHA256")

            'Verifies file is unmodified
            Using sha256 As SHA256 = SHA256.Create()
                Dim hash = sha256.ComputeHash(aircraftLibrary.GetHash())
                If Not rsaDeformatter.VerifySignature(hash, aircraftLibrary.SignedHash) Then
                    isSigned = False
                End If
            End Using

            Return isSigned

        End Function


        'wrg - parameter xmlFilePath added 
        Public Function GetAircrafts(factory As IFaarFieldModelFactory, xmlPath As String, includeBellyAndDeprecated As Boolean) As List(Of IAirplaneInfo)

            Dim aircraftLibrary As SignedAircraftLibrary

            Try
                Dim ser = New DataContractSerializer(GetType(SignedAircraftLibrary))
                Dim textReader As TextReader = New StreamReader(xmlPath)
                Dim xml = textReader.ReadToEnd()
                textReader.Close()
                Dim stream = New MemoryStream(Encoding.UTF8.GetBytes(xml))
                aircraftLibrary = DirectCast(ser.ReadObject(stream), SignedAircraftLibrary)

            Catch ex As Exception
                Return New List(Of IAirplaneInfo)()
            End Try

            'Initial load for UI
            If Not includeBellyAndDeprecated Then

                'Validate only on initial load for UI
                Dim isSignedLibrary As Boolean = ValidateSignedAircraftLibrary(aircraftLibrary)
                If Not isSignedLibrary Then
                    MessageBox.Show("The aircraft library located at " + My.Application.Info.DirectoryPath + "\Defaults\Aircraft\aircraft.xml is not signed by the application.  Use of an unsigned library is allowed by the program, but not FAA approved.")
                End If

                'Initial load removes belly and deprecated aircraft from view
                RemoveBellyAC(aircraftLibrary.Airplanes)
                RemoveDeprecatedAC(aircraftLibrary.Airplanes)

            End If

            For Each airplane In aircraftLibrary.Airplanes

                airplane.AircraftNumber = 0
                'airplane.Gear = "X"

                If airplane.Gear = "N" Then
                    airplane.Gear = "X"
                End If


                airplane.TBT = airplane.Tt
                airplane.Tt = airplane.B
                airplane.B = airplane.TBT

                'airplane.RunMgPercent = airplane.MgPercentPCN
                airplane.RunMgPercent = airplane.MgPercent

                Dim tempDataStorage As Single() = New Single(2) {airplane.MgPercent, airplane.MgPercentPCN, airplane.RunMgPercent}
                airplane.DataStorage = tempDataStorage
            Next

            Return aircraftLibrary.Airplanes
        End Function


        Public Sub RemoveBellyAC(ByRef airplanes As List(Of IAirplaneInfo))
            For i = airplanes.Count - 1 To 0 Step -1
                If Right(airplanes.Item(i).Name, 5) = "Belly" Then
                    airplanes.Remove(airplanes.Item(i))
                End If
            Next
        End Sub


        Public Sub RemoveDeprecatedAC(ByRef airplanes As List(Of IAirplaneInfo))
            For i = airplanes.Count - 1 To 0 Step -1
                If airplanes.Item(i).Deprecated = True Then
                    airplanes.Remove(airplanes.Item(i))
                End If
            Next
        End Sub


        Public Function GetUserDefinedAircrafts(factory As IFaarFieldModelFactory, path As String) As List(Of AirplaneInfo)
            Dim airplaneLibrary As List(Of AirplaneInfo) = New List(Of AirplaneInfo)
            Dim airplaneInfos As List(Of AirplaneInfo)
            Dim airplaneinfo As AirplaneInfo
            Try
                Dim fi As FileInfo
                Dim di As New DirectoryInfo(path)
                Dim filelist As New List(Of String)

                Dim i As Integer

                Dim NumberofUserDefined As Integer = 0
                For Each fi In di.GetFiles
                    If fi.Name.Contains("(UDA)") Then
                        filelist.Add(fi.Name)
                        NumberofUserDefined += 1
                    End If
                Next

                For i = 0 To NumberofUserDefined - 1
                    Dim loadPath As String = path + "\" + filelist.Item(i).ToString

                    Dim ser = New DataContractSerializer(GetType(List(Of AirplaneInfo)))
                    Dim TextReader = New System.IO.StreamReader(loadPath.ToString())
                    Dim Xml = TextReader.ReadToEnd()
                    TextReader.Close()
                    Dim Stream = New MemoryStream(Encoding.UTF8.GetBytes(Xml))

                    Try
                        airplaneInfos = DirectCast(ser.ReadObject(Stream), List(Of AirplaneInfo))
                        If airplaneInfos.Count > 0 Then
                            For Each airplaneinfo In airplaneInfos
                                airplaneinfo.Manufacturer = "External Library"

                                Dim status
                                Dim doc As New XmlDocument()
                                doc.Load(loadPath)
                                Dim root As XmlElement = doc.DocumentElement

                                If root.Attributes("LibraryVersion") IsNot Nothing Then
                                    status = root.Attributes("LibraryVersion").Value
                                Else
                                    If airplaneinfo.MgPercentPCN > 0.5 AndAlso airplaneinfo.WheelCoordinates.Count > 1 Then
                                        airplaneinfo.MgPercentPCN = airplaneinfo.MgPercentPCN / 2
                                    End If
                                End If

                                airplaneinfo.RunMgPercent = airplaneinfo.MgPercentPCN

                                Dim tempDataStorage As Single() = New Single(2) {airplaneinfo.MgPercent, airplaneinfo.MgPercentPCN, airplaneinfo.RunMgPercent}
                                airplaneinfo.DataStorage = tempDataStorage

                                airplaneLibrary.Add(airplaneinfo)
                            Next
                        End If

                    Catch ex As Exception

                    End Try
                Next
            Catch ex As Exception

            End Try

            Return airplaneLibrary
        End Function


        'wrg added xmlFilePath
        Public Function GetAircraftsBelly(factory As IFaarFieldModelFactory,
                                          xmlFilePath As String) As List(Of IAirplaneInfo)
            ' Dim FaarFieldFactory As IFaarFieldModelFactory

            Dim aircraftBellyLibrary As SignedAircraftLibrary

            Try
                Dim ser = New DataContractSerializer(GetType(SignedAircraftLibrary))
                Dim textReader As TextReader = New StreamReader(xmlFilePath)
                Dim xml = textReader.ReadToEnd()
                textReader.Close()
                Dim stream = New MemoryStream(Encoding.UTF8.GetBytes(xml))
                aircraftBellyLibrary = DirectCast(ser.ReadObject(stream), SignedAircraftLibrary)

            Catch ex As Exception
                Return New List(Of IAirplaneInfo)()
            End Try
            Dim rsaKeyInfo = New RSAParameters()
            rsaKeyInfo.Modulus = aircraftBellyLibrary.Modulus
            rsaKeyInfo.Exponent = aircraftBellyLibrary.Exponent
            Dim rsaDecrypt = New RSACryptoServiceProvider()
            rsaDecrypt.ImportParameters(rsaKeyInfo)
            Dim rsaDeformatter = New RSAPKCS1SignatureDeformatter(rsaDecrypt)
            rsaDeformatter.SetHashAlgorithm("SHA256")

            For Each airplane In aircraftBellyLibrary.Airplanes
                airplane.TBT = airplane.Tt
                airplane.Tt = airplane.B
                airplane.B = airplane.TBT
            Next

            Return aircraftBellyLibrary.Airplanes

        End Function


        'List of traffic file names
        Public Function GetTrafficLibrary() As ObservableCollection(Of String)
            Dim trafficFiles = New ObservableCollection(Of String)
            'Dim YourPath = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary\"
            'If (Not System.IO.Directory.Exists(YourPath)) Then
            '    System.IO.Directory.CreateDirectory(YourPath)
            'End If
            Dim libraryFilePath = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary"
            Directory.CreateDirectory(libraryFilePath)
            Dim directoryInfo = New DirectoryInfo(libraryFilePath)
            Dim fileInfos = directoryInfo.GetFiles("*.xml")
            For Each file In fileInfos

                Dim buffersize = 19
                Dim buffer() As Byte = New Byte(buffersize) {}
                Using fs As New FileStream(file.FullName, FileMode.Open, FileAccess.Read, FileShare.None)
                    fs.Read(buffer, 0, buffer.Length)
                End Using

                Dim tmpString = ""
                For Each b In buffer
                    tmpString = tmpString & Convert.ToChar(b).ToString()
                Next

                If tmpString = "<ArrayOfAirplaneInfo" Then
                    trafficFiles.Add(file.Name.Replace(".xml", ""))
                End If

            Next
            Return trafficFiles
        End Function


        Public Function LoadTrafficLibrary(selectedLibrary As String) As ObservableCollection(Of AirplaneInfo)
            Dim loadPath = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary\" + selectedLibrary + ".xml"
            Dim ser = New DataContractSerializer(GetType(List(Of AirplaneInfo)))
            Dim textReader As TextReader = New System.IO.StreamReader(loadPath.ToString())
            Dim json = textReader.ReadToEnd()
            textReader.Close()

            ' Fix File - replace Thickness before load
            Dim updatedjson = Replace(json, "<ACRThick>0</ACRThick>", "<ACRThick><si>0</si><us>0</us></ACRThick>")
            Dim stream As New MemoryStream
            Dim airplaneLibrary = New ObservableCollection(Of AirplaneInfo)
            Dim airplaneInfos As List(Of AirplaneInfo)

            Try
                stream = New MemoryStream(Encoding.UTF8.GetBytes(updatedjson))
                airplaneInfos = DirectCast(ser.ReadObject(stream), List(Of AirplaneInfo))
                If airplaneInfos.Count > 0 Then
                    For Each airplaneInfo In airplaneInfos
                        airplaneLibrary.Add(airplaneInfo)
                    Next
                    For Each airplane In airplaneLibrary
                        airplane.DefaultCp = airplane.Cp
                        airplane.DefaultGrossWeight = airplane.GrossWeight
                        If airplane.MgPercentPCN >= 0.5 AndAlso airplane.WheelCoordinates.Count > 1 Then
                            airplane.MgPercentPCN = airplane.MgPercentPCN / 2
                        End If
                    Next
                End If
            Catch ex As Exception
                MessageBox.Show("File appears to be corrupt")
                Debug.WriteLine(ex)

                Return Nothing
                'Exit Function
            End Try

            Return airplaneLibrary
        End Function


        Public Function SaveTrafficLibrary(currentLibrary As String, currentAirplanes As ObservableCollection(Of AirplaneInfo), trafficLibrary As ObservableCollection(Of String), factory As IFaarFieldModelFactory) As String
            'Dim factory As FaarFieldModel.FaarFieldModelFactory
            Dim coordinateSystem = factory.CreateUsCustomary()
            ' If currentLibrary Is Nothing Then

            If currentAirplanes.Count = 0 Then
                MessageBox.Show("At least one aircraft must be entered to create a library.")
                Return Nothing
            End If

            For Each airplane In currentAirplanes
                airplane.Cdf = 0
                airplane.CdfAircraftMax = 0
                airplane.CtoP = 0
                airplane.ACRThick = factory.CreateThickness(0, coordinateSystem)
                airplane.ACRB = 0

                'Return values to original library values before save
                If airplane.DataStorage IsNot Nothing Then
                    airplane.MgPercent = airplane.DataStorage(0)
                    airplane.MgPercentPCN = airplane.DataStorage(1)
                    airplane.RunMgPercent = airplane.DataStorage(2)
                End If

            Next

            Dim saveFileDialogTrafficLibrary = New SaveFileDialog()
            saveFileDialogTrafficLibrary.InitialDirectory = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary"
            If saveFileDialogTrafficLibrary.ShowDialog() = DialogResult.OK Then
                Dim lastSlashIndex = saveFileDialogTrafficLibrary.FileName.LastIndexOf("\", StringComparison.Ordinal)
                Dim libraryName = saveFileDialogTrafficLibrary.FileName.Substring(lastSlashIndex + 1)
                libraryName = libraryName.Replace(".xml", "")
                If Not trafficLibrary.Contains(libraryName) Then
                    trafficLibrary.Add(libraryName)
                End If
                Dim savePath = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary\" + libraryName + ".xml"
                SaveTrafficLibraryFile(savePath, currentAirplanes)
                Return libraryName
            End If
            'Else
            '    Dim savePath = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary\" + currentLibrary + ".xml"
            '    SaveTrafficLibrary(savePath, currentAirplanes)
            '    Return currentLibrary
            'End If
            'Return Nothing
        End Function


        Public Sub SaveMix(fileName As String, currentAirplanes As ObservableCollection(Of AirplaneInfo))
            Dim savePath = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary\" + fileName + ".xml"
            SaveTrafficLibraryFile(savePath, currentAirplanes)
        End Sub


        Private Sub SaveTrafficLibraryFile(savePath As String, currentAirplanes As ObservableCollection(Of AirplaneInfo))
            Dim factory As New FaarFieldModelFactory
            Dim airplanes = New List(Of AirplaneInfo)()
            For Each airplane In currentAirplanes
                airplane.Cdf = 0
                airplane.CdfAircraftMax = 0
                airplane.CtoP = 0
                airplane.ACRThick = CType(factory.CreateThickness(0, factory.CreateUsCustomary()), Thickness)
                airplane.ACRB = 0

                airplanes.Add(airplane)
            Next
            Dim ser = New DataContractSerializer(GetType(List(Of AirplaneInfo)))
            Dim writer = New FileStream(savePath, FileMode.Create)
            ser.WriteObject(writer, airplanes)
            writer.Close()

            InsertVersionInfo(savePath)

        End Sub

        Public Sub DeleteTrafficLibrary(toDelete As String)
            Dim deletePath = SpecialDirectories.MyDocuments + "\My FAARFIELD\TrafficLibrary\" + toDelete + ".xml"  '.json
            FileSystem.DeleteFile(deletePath)
        End Sub

        Private Sub InsertVersionInfo(path As String)
            Dim libFile As String

            Using reader As New StreamReader(path)
                libFile = reader.ReadToEnd
            End Using

            Dim closeRootPos As Integer
            closeRootPos = InStr(1, libFile, ">", CompareMethod.Text)

            Dim versionAttributes As String = " LibraryVersion=" & ControlChars.Quote & LibraryVersion.ToString() & ControlChars.Quote & " SoftwareVersion=" & ControlChars.Quote & SoftwareVersion.ToString() & ControlChars.Quote

            Dim libFileWithAttribs As String = libFile.Insert(closeRootPos - 1, versionAttributes)

            libFile = libFileWithAttribs

            Using writer As New StreamWriter(path)
                writer.Write(libFile)
            End Using
        End Sub

    End Module

End Namespace