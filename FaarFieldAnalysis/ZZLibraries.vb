Option Strict On
Option Explicit On

Imports VB = Microsoft.VisualBasic
Imports System.Xml
Imports System.Xml.Serialization
Imports System.IO
Imports System.Threading

Imports System
Imports System.Globalization
Imports System.Security.Permissions



Module ZZLibraries

    Sub WriteAC_ZtoX()

        Dim I, J, K, I2 As Integer
        Dim NWheelGroups As Integer
        Dim RightMostX, SouthMostY, xCenter, yCenter, TrackBy2 As Single
        Dim select1(20), ss1 As Integer


        Try

            Dim xmlFile As String
            'xmlFile = WorkingDir + "AirbusBoeing.xml"
            xmlFile = WorkingDir + "FAAairplaneLibrary.xml"
            Dim xw As New Xml.XmlTextWriter(xmlFile, System.Text.Encoding.UTF8)
            xw.WriteStartDocument()
            xw.WriteStartElement("FAAAirplaneLibrary")




            select1(1) = 108 'An-124
            select1(2) = 109 'An-225
            select1(3) = 113 'Caravelle 10
            select1(4) = 115 'Canadair CL 44
            select1(5) = 137 'IL76T
            select1(6) = 215 'A400M TLL1
            select1(7) = 219 'C-5
            select1(8) = 220 'C-17A



            '2D-200

            For ss1 = 1 To 8
                I = select1(ss1)
                xw.WriteStartElement("AirplaneInfo")
                xw.WriteElementString("Name", "ExtL_" & AC(I).libACName)
                xw.WriteElementString("GrossWt", Format(AC(I).libGL, "0").ToString)
                If AC(I).libGear = "A" Then
                    xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt * 0.5, "0.000").ToString)
                Else
                    xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt * 2, "0.000").ToString)
                End If

                xw.WriteElementString("CP", Format(AC(I).libCP, "0.00").ToString)
                xw.WriteElementString("Gear", "X")
                xw.WriteElementString("IGear", Format(13, "0").ToString)
                'xw.WriteElementString("IGear", Format(AC(I).libIGear, "0").ToString)
                xw.WriteElementString("TT", Format(AC(I).libTT, "0.00").ToString)
                xw.WriteElementString("TS", Format(AC(I).libTS, "0.00").ToString)
                xw.WriteElementString("TG", Format(AC(I).libTG, "0.00").ToString)
                xw.WriteElementString("B", Format(AC(I).libB, "0.00").ToString)
                NWheelGroups = 1
                xw.WriteElementString("NWheelGroups", Format(NWheelGroups, "0").ToString)


                '**********************    WHEEL GROUP INFO    **********************
                xw.WriteStartElement("WheelGroupInfo") '+++++++++++++++++++++++++

                For K = 1 To NWheelGroups

                    xw.WriteElementString("NTires", Format(AC(I).libNTires, "0").ToString)

                    xw.WriteStartElement("Wheel_Coordinates")
                    RightMostX = -1.0E+35

                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTX(I2) > RightMostX Then RightMostX = AC(I).libTX(I2)
                    Next I2

                    SouthMostY = 1.0E+35
                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTY(I2) < SouthMostY Then SouthMostY = AC(I).libTY(I2)
                    Next I2


                    xCenter = 0.0! : yCenter = 0.0!
                    For J = 1 To AC(LI).libNTires
                        xCenter = xCenter + AC(LI).libTX(J)
                        yCenter = yCenter + AC(LI).libTY(J)
                    Next J

                    xCenter = xCenter / AC(LI).libNTires
                    yCenter = yCenter / AC(LI).libNTires


                    TrackBy2 = CSng(AC(I).libTS / 2.0# + RightMostX - xCenter)

                    For J = 1 To AC(I).libNTires
                        xw.WriteElementString("TX", Format(-(TrackBy2 - AC(I).libTX(J) + xCenter), "0.00").ToString)
                        xw.WriteElementString("TY", Format(AC(I).libTY(J) - SouthMostY, "0.00").ToString)
                    Next J
                    xw.WriteEndElement() 'Wheel_Coordinates Info

                    xw.WriteElementString("NEVPTS", Format(AC(I).libNEVPTS, "0").ToString)
                    xw.WriteStartElement("Evaluation_Points")
                    For J = 1 To AC(I).libNEVPTS
                        xw.WriteElementString("EVPTX", Format(-(TrackBy2 - AC(I).libEVPTX(J) + xCenter), "0.00").ToString)
                        xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J) - SouthMostY, "0.00").ToString)
                    Next J
                    xw.WriteEndElement() 'Evaluation_Points Info

                Next

                xw.WriteEndElement() 'Wheel_Group Info '+++++++++++++++++++++++++++
                xw.WriteEndElement() 'Airplane Info
                '--------------------
            Next ss1


            xw.WriteEndElement()
            xw.WriteEndDocument()
            xw.Close()

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub

    Sub WriteAC_A380_as_X_Ver01()
        'write external library aircraft as category 'X'
        'A380     WFBN -> X
        'B747-400 WFBF -> X


        Dim I, J, K As Integer
        Dim NWheelGroups As Integer

        Dim RightMostY, SouthMostX, xCenter, yCenter, TrackBy2 As Single
        Dim I2 As Integer, ss1 As Integer, select1(20) As Integer

        Try

            Dim xmlFile As String
            xmlFile = WorkingDir + "FAAairplaneLibrary.xml"
            Dim xw As New Xml.XmlTextWriter(xmlFile, System.Text.Encoding.UTF8)
            xw.WriteStartDocument()
            xw.WriteStartElement("FAAAirplaneLibrary")

            select1(1) = 52   'A380
            select1(2) = 76   'B747-400


            For ss1 = 1 To 2
                I = select1(ss1)


                Dim xc1 As Single
                xc1 = 0
                If AC(I).libGear = "WFBF" Then
                    For I2 = 5 To 8
                        xc1 = xc1 + AC(I).libTX(I2)
                    Next I2
                    xc1 = 75 - Math.Abs(xc1 / 4)
                ElseIf AC(I).libGear = "WFBN" Then
                    For I2 = 5 To 10
                        xc1 = xc1 + AC(I).libTX(I2)
                    Next I2
                    xc1 = 75 - Math.Abs(xc1 / 6)
                End If


                xw.WriteStartElement("AirplaneInfo")
                xw.WriteElementString("Name", "EL_" & AC(I).libACName)

                xw.WriteElementString("GrossWt", Format(AC(I).libGL, "0.00").ToString)
                xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt, "0.000").ToString)

                xw.WriteElementString("CP", Format(AC(I).libCP, "0.00").ToString)

                '================================================================
                xw.WriteElementString("Gear", "X")
                xw.WriteElementString("IGear", Format(13, "0").ToString)

                'xw.WriteElementString("Gear", AC(I).libGear)
                'xw.WriteElementString("IGear", Format(AC(I).libIGear, "0").ToString)
                '================================================================

                xw.WriteElementString("TT", Format(AC(I).libTT, "0.00").ToString)
                xw.WriteElementString("TS", Format(AC(I).libTS, "0.00").ToString)
                xw.WriteElementString("TS", Format(AC(I).libTS, "0.00").ToString)
                xw.WriteElementString("TG", Format(AC(I).libTG, "0.00").ToString)
                xw.WriteElementString("B", Format(AC(I).libB, "0.00").ToString)

                NWheelGroups = 2
                xw.WriteElementString("NWheelGroups", Format(NWheelGroups, "0").ToString)


                '**********************    WHEEL GROUP INFO    **********************
                xw.WriteStartElement("WheelGroupInfo") '+++++++++++++++++++++++++

                For K = 1 To NWheelGroups

                    If AC(I).libGear = "WFBF" Then
                        xw.WriteElementString("NTires", Format(4, "0").ToString)
                    ElseIf AC(I).libGear = "WFBN" Then
                        If K = 1 Then
                            xw.WriteElementString("NTires", Format(4, "0").ToString)
                        ElseIf K = 2 Then
                            xw.WriteElementString("NTires", Format(6, "0").ToString)
                        End If
                    Else
                        xw.WriteElementString("NTires", Format(AC(I).libNTires, "0").ToString)
                    End If

                    xw.WriteStartElement("Wheel_Coordinates")
                    RightMostY = -1.0E+35

                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTX(I2) > RightMostY Then RightMostY = AC(I).libTX(I2)
                    Next I2

                    SouthMostX = 1.0E+35
                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTY(I2) < SouthMostX Then SouthMostX = AC(I).libTY(I2)
                    Next I2

                    xCenter = 0.0! : yCenter = 0.0!

                    If AC(I).libGear = "WFBF" Then
                        If K = 1 Then
                            For J = 1 To 4
                                xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                        ElseIf K = 2 Then
                            For J = 5 To 8 ' 12
                                xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                        End If
                    ElseIf AC(I).libGear = "WFBN" Then
                        If K = 1 Then
                            For J = 1 To 4
                                xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                        ElseIf K = 2 Then
                            For J = 5 To 10 ' 16
                                xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                        End If
                    Else
                        For J = 1 To AC(I).libNTires
                            xw.WriteElementString("TX", Format(AC(I).libTX(J) - xCenter - TrackBy2, "0.00").ToString)
                            xw.WriteElementString("TY", Format(AC(I).libTY(J) - SouthMostX, "0.00").ToString)
                        Next J
                    End If
                    xw.WriteEndElement() 'Wheel_Coordinates Info




                    If AC(I).libGear = "WFBF" Then
                        If K = 1 Then
                            xw.WriteElementString("NEVPTS", Format(18, "0").ToString)
                        ElseIf K = 2 Then
                            xw.WriteElementString("NEVPTS", Format(28, "0").ToString)
                        End If
                    ElseIf AC(I).libGear = "WFBN" Then
                        If K = 1 Then
                            xw.WriteElementString("NEVPTS", Format(18, "0").ToString)
                        ElseIf K = 2 Then
                            xw.WriteElementString("NEVPTS", Format(28, "0").ToString)
                        End If
                    Else
                        xw.WriteElementString("NEVPTS", Format(AC(I).libNEVPTS, "0").ToString)
                    End If


                    xw.WriteStartElement("Evaluation_Points")
                    If AC(I).libGear = "WFBF" Or AC(I).libGear = "WFBN" Then
                        If K = 1 Then

                            For J = 1 To 8
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J), "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J
                            For J = 37 To AC(I).libNEVPTS
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J), "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J

                        ElseIf K = 2 Then

                            For J = 9 To 16
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J), "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J
                            For J = 17 To 36
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J), "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J

                        End If

                    Else
                        For J = 1 To AC(I).libNEVPTS
                            xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J) - xCenter - TrackBy2, "0.00").ToString)
                            xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J) - SouthMostX, "0.00").ToString)
                        Next J
                    End If
                    xw.WriteEndElement() 'Evaluation_Points Info

                Next


                xw.WriteEndElement() 'Wheel_Group Info '+++++++++++++++++++++++++++
                xw.WriteEndElement() 'Airplane Info
                '--------------------
            Next ss1


            xw.WriteEndElement()
            xw.WriteEndDocument()
            xw.Close()

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

        DesigningStr = False


    End Sub

    Sub WriteACforJeffRappolVer05()

        Dim S As String, iHB, I, J, K As Integer
        Dim cinfo As String, NWheelGroups As Integer

        Dim RightMostY, SouthMostX, xCenter, yCenter, TrackBy2 As Single
        Dim I2 As Integer

        '-------------------------------------------
        'FileOpen(9, "ZACdata.txt", OpenMode.Output)
        cinfo = CultureInfo.CurrentCulture.Name

        Try

            Dim xmlFile As String
            'xmlFile = WorkingDir + "AirbusBoeing.xml"
            xmlFile = WorkingDir + "FAAairplaneLibrary.xml"
            Dim xw As New Xml.XmlTextWriter(xmlFile, System.Text.Encoding.UTF8)
            xw.WriteStartDocument()
            xw.WriteStartElement("FAAAirplaneLibrary")

            'For I = 27 To 103
            For I = 27 To 105

                Dim xc1 As Single
                xc1 = 0
                If AC(I).libGear = "WFBF" Then
                    For I2 = 5 To 8
                        xc1 = xc1 + AC(I).libTX(I2)
                    Next I2
                    xc1 = 75 - Math.Abs(xc1 / 4)
                ElseIf AC(I).libGear = "WFBN" Then
                    For I2 = 5 To 10
                        xc1 = xc1 + AC(I).libTX(I2)
                    Next I2
                    xc1 = 75 - Math.Abs(xc1 / 6)
                End If



                If AC(I).libGear = "H" Then
                    For iHB = I To UBound(AC, 1)
                        If AC(iHB).libACName = AC(I).libACName & " Belly" Then
                            Exit For
                        End If
                    Next
                End If


                xw.WriteStartElement("AirplaneInfo")

                If AC(I).libGear = "H" Then
                    xw.WriteElementString("Name", AC(I).libACName & "_EL")
                Else
                    xw.WriteElementString("Name", "ExLib_" & AC(I).libACName)
                End If



                xw.WriteElementString("GrossWt", Format(AC(I).libGL, "0").ToString)
                If AC(I).libGear = "H" Then
                    'xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt * 2 + AC(iHB).libMGpcnt, "0.00").ToString)
                    'xw.WriteElementString("MGpcnt", Format(0.95, "0.00").ToString)
                    xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt, "0.000").ToString)
                ElseIf AC(I).libGear = "WFBF" Or AC(I).libGear = "WFBN" Then
                    xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt, "0.000").ToString)
                Else
                    xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt, "0.000").ToString)
                End If

                xw.WriteElementString("CP", Format(AC(I).libCP, "0.00").ToString)
                'xw.WriteElementString("Gear", "X")
                xw.WriteElementString("Gear", AC(I).libGear)
                'xw.WriteElementString("IGear", Format(13, "0").ToString)
                xw.WriteElementString("IGear", Format(AC(I).libIGear, "0").ToString)

                xw.WriteElementString("TT", Format(AC(I).libTT, "0.00").ToString)
                'xw.WriteElementString("TS", Format(AC(I).libTS, "0.00").ToString)

                If AC(I).libGear = "XXXXX" Then
                    xw.WriteElementString("TS", Format(150 - AC(I).libTT, "0.00").ToString)
                ElseIf AC(I).libGear = "WFBF" Or AC(I).libGear = "WFBN" Then
                    xw.WriteElementString("TS", Format(AC(I).libTS, "0.00").ToString)
                Else
                    xw.WriteElementString("TS", Format(150 - AC(I).libTT, "0.00").ToString)
                End If



                If Mid(AC(I).libACName$, 1, 8) = "A340-200" Or Mid(AC(I).libACName$, 1, 8) = "A340-300" Then
                    xw.WriteElementString("TG", Format(38, "0.00").ToString)
                    xw.WriteElementString("B", Format(AC(I).libB, "0.00").ToString)
                ElseIf Mid(AC(I).libACName$, 1, 8) = "A340-500" Or Mid(AC(I).libACName$, 1, 8) = "A340-600" Then
                    xw.WriteElementString("TG", Format(46, "0.00").ToString)
                    xw.WriteElementString("B", Format(AC(I).libB, "0.00").ToString)
                Else
                    xw.WriteElementString("TG", Format(AC(I).libTG, "0.00").ToString)
                    xw.WriteElementString("B", Format(AC(I).libB, "0.00").ToString)
                End If






                If AC(I).libGear = "XXXXX" Then
                    NWheelGroups = 4
                ElseIf AC(I).libGear = "WFBF" Or AC(I).libGear = "WFBN" Then
                    NWheelGroups = 2
                Else
                    NWheelGroups = 1
                End If

                If AC(I).libGear = "H1" Then
                    xw.WriteElementString("NWheelGroups", Format(2, "0").ToString)
                Else
                    xw.WriteElementString("NWheelGroups", Format(NWheelGroups, "0").ToString)
                End If




                '**********************    WHEEL GROUP INFO    **********************
                xw.WriteStartElement("WheelGroupInfo") '+++++++++++++++++++++++++

                For K = 1 To NWheelGroups

                    If AC(I).libGear = "WFBF" Then
                        xw.WriteElementString("NTires", Format(4, "0").ToString)
                    ElseIf AC(I).libGear = "WFBN" Then
                        If K = 1 Then
                            xw.WriteElementString("NTires", Format(4, "0").ToString)
                        ElseIf K = 2 Then
                            xw.WriteElementString("NTires", Format(6, "0").ToString)
                        End If
                    Else
                        xw.WriteElementString("NTires", Format(AC(I).libNTires, "0").ToString)
                    End If

                    xw.WriteStartElement("Wheel_Coordinates")
                    RightMostY = -1.0E+35

                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTX(I2) > RightMostY Then RightMostY = AC(I).libTX(I2)
                    Next I2

                    SouthMostX = 1.0E+35
                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTY(I2) < SouthMostX Then SouthMostX = AC(I).libTY(I2)
                    Next I2


                    xCenter = 0.0! : yCenter = 0.0!
                    'For I2 = 1 To AC(I).libNTires
                    '    xCenter = xCenter + AC(I).libTX(I2)
                    '    yCenter = yCenter + AC(I).libTY(I2)
                    'Next I2

                    'xCenter = xCenter / AC(I).libNTires
                    'yCenter = yCenter / AC(I).libNTires

                    'TrackBy2 = CSng((AC(I).libTT + AC(I).libTS) / 2.0#)


                    If AC(I).libGear = "WFBF" Or AC(I).libGear = "WFBN" Then
                    End If



                    If AC(I).libGear = "XXXXX" Then
                        NWheelGroups = 4
                    ElseIf AC(I).libGear = "WFBF" Then
                        If K = 1 Then
                            For J = 1 To 4
                                xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                            'For J = 13 To 16
                            '    xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                            '    xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            'Next J
                        ElseIf K = 2 Then


                            For J = 5 To 8 ' 12
                                xw.WriteElementString("TX", Format(AC(I).libTX(J) - xc1, "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                        End If
                    ElseIf AC(I).libGear = "WFBN" Then
                        If K = 1 Then
                            For J = 1 To 4
                                xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                            'For J = 17 To 20
                            '    xw.WriteElementString("TX", Format(AC(I).libTX(J), "0.00").ToString)
                            '    xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            'Next J
                        ElseIf K = 2 Then
                            For J = 5 To 10 ' 16
                                xw.WriteElementString("TX", Format(AC(I).libTX(J) - xc1, "0.00").ToString)
                                xw.WriteElementString("TY", Format(AC(I).libTY(J), "0.00").ToString)
                            Next J
                        End If
                    Else
                        For J = 1 To AC(I).libNTires
                            xw.WriteElementString("TX", Format(AC(I).libTX(J) - xCenter - TrackBy2, "0.00").ToString)
                            xw.WriteElementString("TY", Format(AC(I).libTY(J) - SouthMostX, "0.00").ToString)
                        Next J
                    End If
                    xw.WriteEndElement() 'Wheel_Coordinates Info




                    If AC(I).libGear = "WFBF" Then
                        If K = 1 Then
                            xw.WriteElementString("NEVPTS", Format(18, "0").ToString)
                        ElseIf K = 2 Then
                            xw.WriteElementString("NEVPTS", Format(28, "0").ToString)
                        End If
                    ElseIf AC(I).libGear = "WFBN" Then
                        If K = 1 Then
                            xw.WriteElementString("NEVPTS", Format(18, "0").ToString)
                        ElseIf K = 2 Then
                            xw.WriteElementString("NEVPTS", Format(28, "0").ToString)
                        End If
                    Else
                        xw.WriteElementString("NEVPTS", Format(AC(I).libNEVPTS, "0").ToString)
                    End If


                    xw.WriteStartElement("Evaluation_Points")
                    If AC(I).libGear = "XXXXX" Then
                        NWheelGroups = 4
                    ElseIf AC(I).libGear = "WFBF" Or AC(I).libGear = "WFBN" Then
                        If K = 1 Then
                            For J = 1 To 8
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J), "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J

                            For J = 37 To AC(I).libNEVPTS
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J), "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J

                        ElseIf K = 2 Then
                            For J = 9 To 16
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J) - xc1, "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J

                            For J = 17 To 36
                                xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J), "0.00").ToString)
                                xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J), "0.00").ToString)
                            Next J

                        End If


                    Else
                        For J = 1 To AC(I).libNEVPTS
                            xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J) - xCenter - TrackBy2, "0.00").ToString)
                            xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J) - SouthMostX, "0.00").ToString)
                        Next J
                    End If
                    xw.WriteEndElement() 'Evaluation_Points Info

                Next






                'HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH BBBBBB
                If AC(I).libGear = "H1" Then
                    If AC(iHB).libNTires = 4 Then
                        yCenter = 0
                    End If

                    xw.WriteElementString("NTires", Format(AC(iHB).libNTires / 2, "0").ToString)
                    xw.WriteStartElement("Wheel_Coordinates")
                    For J = 1 To CInt(AC(iHB).libNTires) Step 2
                        xw.WriteElementString("TX", Format(AC(iHB).libTX(J), "0.00").ToString)
                        xw.WriteElementString("TY", Format(AC(iHB).libTY(J) + yCenter, "0.00").ToString)
                    Next J
                    xw.WriteEndElement() 'Wheel_Coordinates Info

                    xw.WriteElementString("NEVPTS", Format(AC(iHB).libNEVPTS, "0").ToString)
                    xw.WriteStartElement("Evaluation_Points")
                    For J = 1 To AC(iHB).libNEVPTS
                        xw.WriteElementString("EVPTX", Format(AC(iHB).libEVPTX(J), "0.00").ToString)
                        xw.WriteElementString("EVPTY", Format(AC(iHB).libEVPTY(J) + yCenter, "0.00").ToString)
                    Next J
                    xw.WriteEndElement() 'Evaluation_Points Info
                End If
                'HHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHHH BBBBBB


                xw.WriteEndElement() 'Wheel_Group Info '+++++++++++++++++++++++++++
                xw.WriteEndElement() 'Airplane Info
                '--------------------
            Next I


            xw.WriteEndElement()
            xw.WriteEndDocument()
            xw.Close()

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

        DesigningStr = False

    End Sub

    Sub WriteAC_BDEFN_Ver01()
        '          saving as  'X'

        Dim I, J, K, I2 As Integer
        Dim NWheelGroups As Integer
        Dim RightMostY, SouthMostX, xCenter, yCenter, TrackBy2 As Single
        Dim select1(20), ss1 As Integer


        Try

            Dim xmlFile As String
            'xmlFile = WorkingDir + "AirbusBoeing.xml"
            xmlFile = WorkingDir + "FAAairplaneLibraryB777.xml"
            Dim xw As New Xml.XmlTextWriter(xmlFile, System.Text.Encoding.UTF8)
            xw.WriteStartDocument()
            xw.WriteStartElement("FAAAirplaneLibrary")




            select1(1) = 1 'SWL-50
            select1(2) = 2 'S-30
            select1(3) = 7 'D-50
            select1(4) = 12 '2D-100
            select1(5) = 14 '2D-200
            select1(6) = 89 'B777-200 Baseline
            select1(7) = 139 'L-100-20 	          libB=60.6 Single


            select1(8) = 108
            select1(9) = 109
            select1(10) = 113
            select1(11) = 115
            select1(12) = 137
            select1(13) = 215
            select1(14) = 219
            select1(15) = 220

            select1(1) = 93 'B777-300 ER

            '2D-200

            For ss1 = 1 To 1
                I = select1(ss1)
                xw.WriteStartElement("AirplaneInfo")
                xw.WriteElementString("Name", "ExtL_" & AC(I).libACName)
                xw.WriteElementString("GrossWt", Format(AC(I).libGL, "0").ToString)
                If AC(I).libGear = "A" Then
                    xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt * 0.5, "0.000").ToString)
                Else
                    xw.WriteElementString("MGpcnt", Format(AC(I).libMGpcnt * 2, "0.000").ToString)
                End If

                xw.WriteElementString("CP", Format(AC(I).libCP, "0.00").ToString)
                xw.WriteElementString("Gear", "X")
                xw.WriteElementString("IGear", Format(13, "0").ToString)
                'xw.WriteElementString("IGear", Format(AC(I).libIGear, "0").ToString)
                xw.WriteElementString("TT", Format(AC(I).libTT, "0.00").ToString)
                xw.WriteElementString("TS", Format(AC(I).libTS, "0.00").ToString)
                xw.WriteElementString("TG", Format(AC(I).libTG, "0.00").ToString)
                xw.WriteElementString("B", Format(AC(I).libB, "0.00").ToString)
                NWheelGroups = 1
                xw.WriteElementString("NWheelGroups", Format(NWheelGroups, "0").ToString)


                '**********************    WHEEL GROUP INFO    **********************
                xw.WriteStartElement("WheelGroupInfo") '+++++++++++++++++++++++++

                For K = 1 To NWheelGroups

                    xw.WriteElementString("NTires", Format(AC(I).libNTires, "0").ToString)

                    xw.WriteStartElement("Wheel_Coordinates")
                    RightMostY = -1.0E+35

                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTX(I2) > RightMostY Then RightMostY = AC(I).libTX(I2)
                    Next I2

                    SouthMostX = 1.0E+35
                    For I2 = 1 To AC(I).libNTires
                        If AC(I).libTY(I2) < SouthMostX Then SouthMostX = AC(I).libTY(I2)
                    Next I2

                    xCenter = 0.0! : yCenter = 0.0!

                    TrackBy2 = CSng((AC(I).libTT + AC(I).libTS) / 2.0#)

                    For J = 1 To AC(I).libNTires
                        xw.WriteElementString("TX", Format(AC(I).libTX(J) - xCenter - TrackBy2, "0.00").ToString)
                        xw.WriteElementString("TY", Format(AC(I).libTY(J) - SouthMostX, "0.00").ToString)
                    Next J
                    xw.WriteEndElement() 'Wheel_Coordinates Info

                    xw.WriteElementString("NEVPTS", Format(AC(I).libNEVPTS, "0").ToString)
                    xw.WriteStartElement("Evaluation_Points")
                    For J = 1 To AC(I).libNEVPTS
                        xw.WriteElementString("EVPTX", Format(AC(I).libEVPTX(J) - xCenter - TrackBy2, "0.00").ToString)
                        xw.WriteElementString("EVPTY", Format(AC(I).libEVPTY(J) - SouthMostX, "0.00").ToString)
                    Next J
                    xw.WriteEndElement() 'Evaluation_Points Info

                Next

                xw.WriteEndElement() 'Wheel_Group Info '+++++++++++++++++++++++++++
                xw.WriteEndElement() 'Airplane Info
                '--------------------
            Next ss1


            xw.WriteEndElement()
            xw.WriteEndDocument()
            xw.Close()

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

        DesigningStr = False

    End Sub

End Module
