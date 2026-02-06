Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports FF2.Libs


'Public Interface IAircraftInfo : Inherits IAirplaneInfo

'    Property AircraftID As Integer

'End Interface


'Public Class AircraftInfo22 : Inherits AirplaneInfo
'    Implements IAircraftInfo

'    Private _aircraftID
'    Public Property AircraftID As Integer Implements IAircraftInfo.AircraftID
'        Get
'            Return _aircraftID
'        End Get
'        Set
'            _aircraftID = Value
'        End Set
'    End Property

'End Class


Public Class AircraftList

    Public Sub New(xmlPath As String)

        _errorMsgList = New List(Of String)
        Me._airplaneXMLFileLocation = xmlPath
        _aircraftList = New List(Of IAirplaneInfo)

        FaarFieldFactory = New FaarFieldModelFactory()
        Me.InitBellyList()


    End Sub

#Region "Properties"

    Dim FaarFieldFactory As FaarFieldModelFactory = Nothing

    Private _errorMsgList As List(Of String)
    Public ReadOnly Property ErrorMessages As List(Of String)
        Get
            Return _errorMsgList
        End Get
    End Property

    Private _aircraftList As List(Of IAirplaneInfo)
    Public ReadOnly Property AircraftList As List(Of IAirplaneInfo)
        Get
            Return _aircraftList
        End Get
    End Property

    Private _aircraftBellyList As List(Of IAirplaneInfo)
    Public ReadOnly Property AircraftBellyList As List(Of IAirplaneInfo)
        Get
            Return _aircraftBellyList
        End Get
    End Property

    Private _airplaneXMLFileLocation As String = Nothing
    Public Property airplaneXMLFileLocation As String
        Get
            If String.IsNullOrEmpty(_airplaneXMLFileLocation) Or String.IsNullOrWhiteSpace(_airplaneXMLFileLocation) Then
                _airplaneXMLFileLocation = My.Application.Info.DirectoryPath + "\Defaults\Aircraft\aircraft.xml"
            End If

            Return _airplaneXMLFileLocation
        End Get
        Set(ByVal value As String)
            _airplaneXMLFileLocation = value
        End Set
    End Property


#End Region

#Region "Add airplane to the list"
    Public Function Add(item As IAirplaneInfo) As Boolean
        Dim status As Boolean = True

        'item.AircraftID = Me._aircraftIDCounter

        _aircraftList.Add(item)
        AddBellyAC(item)

        Return status
    End Function

#End Region

#Region "delete airplane from list"
    Public Function Delete(acListLocationID As Integer) As Boolean

        Dim status As Boolean = True
        Dim isBelly = Nothing

        Me.ErrorMessages.Clear()

        Try

            If (status = True And _aircraftList IsNot Nothing) Then

                Dim maxlistLocationID = _aircraftList.Count() - 1

                If (maxlistLocationID >= acListLocationID) Then

                    Dim airCraft As IAirplaneInfo = _aircraftList(acListLocationID)

                    If airCraft.Name.Contains(" Belly") Then isBelly = True Else isBelly = False

                    Dim acName As String = airCraft.Name.Replace(" Belly", "") 'if the user is modifying the belly
                    Dim bellyName As String = acName + " Belly"

                    If (isBelly And acListLocationID = 0) Then 'error if belly is first item in list
                        status = False
                        Me.ErrorMessages.Add(acName + " Belly aircraft cannot be the first item in the aircraft list")
                    ElseIf (isBelly And acListLocationID > 0) Then 'if belly check for main ac before it
                        If _aircraftList(acListLocationID - 1).Name = acName Then
                            _aircraftList.RemoveAt(acListLocationID)
                            _aircraftList.RemoveAt(acListLocationID - 1)
                        Else
                            status = False
                            Me.ErrorMessages.Add(bellyName + " Belly aircraft not preceeded by its aircraft counterpart")
                        End If
                    ElseIf (isBelly = False And acListLocationID = maxlistLocationID) Then 'if main aircraft is last item on list
                        _aircraftList.RemoveAt(acListLocationID)
                    ElseIf (isBelly = False And acListLocationID < maxlistLocationID) Then 'if main aircraft is not last item on list, check for belly
                        If _aircraftList(acListLocationID + 1).Name = bellyName Then
                            _aircraftList.RemoveAt(acListLocationID + 1)
                            _aircraftList.RemoveAt(acListLocationID)
                        Else
                            _aircraftList.RemoveAt(acListLocationID)
                        End If
                    End If
                Else
                    status = False
                    Me.ErrorMessages.Add(Convert.ToString(acListLocationID) + " index is larger than the aircraft list")
                End If
            End If

        Catch ex As Exception

        End Try

        Return status








        'Dim acIndex = _aircraftList.FindIndex(Function(x) x.Name = acName)
        'Dim bellyIndex = _aircraftList.FindIndex(Function(x) x.Name = bellyName)

        'Me._aircraftList.RemoveAll(Function(x) (x.Name = acName Or x.Name = bellyName))

        Return status

    End Function

#End Region


#Region "update properties for airplane in list"
    Public Function UpdateNumberDepartures(acListLocationID As Integer, numDepartures As Integer) As Boolean
        Dim status As Boolean = True
        Dim isBelly = Nothing

        Me.ErrorMessages.Clear()

        status = ValidateNumberDepartures(numDepartures)

        Try

            If (status = True And _aircraftList IsNot Nothing) Then

                Dim maxlistLocationID = _aircraftList.Count() - 1

                If (maxlistLocationID >= acListLocationID) Then

                    Dim airCraft As IAirplaneInfo = _aircraftList(acListLocationID)

                    If airCraft.Name.Contains(" Belly") Then isBelly = True Else isBelly = False

                    Dim acName As String = airCraft.Name.Replace(" Belly", "") 'if the user is modifying the belly
                    Dim bellyName As String = acName + " Belly"

                    If (isBelly And acListLocationID = 0) Then 'error if belly is first item in list
                        status = False
                        Me.ErrorMessages.Add(acName + " Belly aircraft cannot be the first item in the aircraft list")
                    ElseIf (isBelly And acListLocationID > 0) Then 'if belly check for main ac before it
                        If _aircraftList(acListLocationID - 1).Name = acName Then
                            _aircraftList(acListLocationID).NumberDepartures = numDepartures
                            _aircraftList(acListLocationID - 1).NumberDepartures = numDepartures
                        Else
                            status = False
                            Me.ErrorMessages.Add(bellyName + " Belly aircraft not preceeded by its aircraft counterpart")
                        End If
                    ElseIf (isBelly = False And acListLocationID = maxlistLocationID) Then 'if main aircraft is last item on list
                        _aircraftList(acListLocationID).NumberDepartures = numDepartures
                    ElseIf (isBelly = False And acListLocationID < maxlistLocationID) Then 'if main aircraft is not last item on list, check for belly
                        If _aircraftList(acListLocationID + 1).Name = bellyName Then
                            _aircraftList(acListLocationID).NumberDepartures = numDepartures
                            _aircraftList(acListLocationID + 1).NumberDepartures = numDepartures
                        Else
                            _aircraftList(acListLocationID).NumberDepartures = numDepartures
                        End If
                    End If


                Else
                    status = False
                    Me.ErrorMessages.Add(Convert.ToString(acListLocationID) + " index is larger than the aircraft list")
                End If
            End If

            'Dim acIndex = _aircraftList.FindIndex(Function(x) x.Name = acName)
            'Dim bellyIndex = _aircraftList.FindIndex(Function(x) x.Name = bellyName)

            '_aircraftList(acIndex).NumberDepartures = numDepartures
            '_aircraftList(bellyIndex).NumberDepartures = numDepartures

        Catch ex As Exception

        End Try

        Return status

    End Function

#End Region


#Region "Initalize the contents of the Belly library from the airplane info XML file"
    Private Sub InitBellyList()

        Dim status = True

        Me._aircraftBellyList = GetAircraftsBelly(FaarFieldFactory, Me.airplaneXMLFileLocation)

    End Sub

#End Region

#Region "add belly if applicable"
    Private Sub AddBellyAC(addedAirplane As IAirplaneInfo)

        Dim bellyIndex As Integer = Nothing
        Dim bellyAC As IAirplaneInfo = Nothing

        If FindBelly(addedAirplane, bellyAC) = True Then
            'add belly
            Me._aircraftList.Add(bellyAC)
        End If

    End Sub

#End Region

#Region "search for corresponding belly counterpart"

    Private Function FindBelly(mainAircraft As IAirplaneInfo, ByRef bellyAircraft As IAirplaneInfo) As Boolean
        Dim status As Boolean = True
        Dim numItems As Integer
        Dim bellyIndex As Integer = Nothing

        If mainAircraft.IsBelly = True Then

            Try
                numItems = Me._aircraftBellyList.Count()

                For I = 1 To numItems - 1
                    If Me._aircraftBellyList.Item(I).Name = mainAircraft.Name + " Belly" Then
                        bellyIndex = I
                        Exit For
                    End If
                Next

                bellyAircraft = FaarFieldFactory.CreateAircraftbelly(Me._aircraftBellyList.Item(bellyIndex), FaarFieldFactory)

            Catch ex As Exception
                status = False
            End Try
        Else
            status = False
        End If

        Return status
    End Function
#End Region

#Region "ValidationRules"

    Private Function ValidateNumberDepartures(value As Integer) As Boolean

        Dim status As Boolean = True

        Me._errorMsgList.Clear()

        If value > 100000 Then
            _errorMsgList.Add("Maximum allowable number of Annual Departures is 100,000")
            status = False
        ElseIf value < 0 Then
            _errorMsgList.Add("Minimum allowable number of Annual Departures is 0")
            status = False
        Else
            'no action
        End If

        Return status

    End Function


#End Region

End Class
