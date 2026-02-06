
'http://www.vbforums.com/showthread.php?320745-Your-VB-NET-App-taking-up-too-much-memory
Module ZZZ
    Private Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (
                            ByVal process As IntPtr,
                            ByVal minimumWorkingSetSize As Integer,
                            ByVal maximumWorkingSetSize As Integer) As Integer


    'Public Sub Set_to_LEAF_Structure()
    '    'NewRigid, PCCOnFlex     'Call DesignRigid_NP() '3D-FEM stress   
    '    '                         Call pre_DesignRigid_NP()
    '    'UnbondOnRigid           'Call DesignRigidOverlay_NP()  '3D-FEM stress
    '    'FlexOnRigid             'Call DesignRigidOverlay_NP()  '3D-FEM stress
    '    Dim I As Integer

    '    If NPLayers <> LEAStructure.NLayers Then

    '        For I = 1 To LEAStructure.NLayers
    '            LEAStrActiveX.Thick(I) = LEAStructure.Thick(I)
    '            LEAStrActiveX.Modulus(I) = LEAStructure.Modulus(I)
    '            LEAStrActiveX.Poisson(I) = LEAStructure.Poisson(I)

    '            If LEAStructure.InterfaceCode(I) = 0 Then
    '                LEAStrActiveX.InterfaceParm(I) = 1
    '            ElseIf LEAStructure.InterfaceCode(I) >= 100000 Then
    '                LEAStrActiveX.InterfaceParm(I) = 0
    '            End If
    '        Next

    '        LEAStrActiveX.Poisson(LEAStructure.NLayers) = DefaultPoissonSGPCC
    '        LEAStrActiveX.NLayers = LEAStructure.NLayers

    '    End If

    'End Sub


    Public Sub Set_to_FEM_Structure()
        Dim I As Integer

        If NPLayers < LEAStructure.NLayers Then

            For I = 1 To NPLayers
                LEAStrActiveX.Thick(I) = Thick(I)
                LEAStrActiveX.Modulus(I) = Modulus(I)
                LEAStrActiveX.Poisson(I) = LEAStructure.Poisson(I)

                If LEAStructure.InterfaceCode(I) = 0 Then
                    LEAStrActiveX.InterfaceParm(I) = 1
                ElseIf LEAStructure.InterfaceCode(I) >= 100000 Then
                    LEAStrActiveX.InterfaceParm(I) = 0
                End If
            Next
            LEAStrActiveX.Poisson(NPLayers) = DefaultPoissonSGPCC
            LEAStrActiveX.NLayers = NPLayers

        End If

    End Sub









    Public Sub FlushMemory1()
        'GC.Collect()
        'GC.WaitForPendingFinalizers()
        'If (Environment.OSVersion.Platform = PlatformID.Win32NT) Then
        '    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1)
        'End If
    End Sub



    'Izydor Notes
    'Sub SetCurrentSectData()
    'Sub ReadJobFileXML()


    Public Sub OneCallToCalcCtoP_RigidType()
        'For DesignType 
        ' * NewRigid
        ' * PCConFlex
        ' * OverlayRigOnRig

        Dim IA, OFFSET, extra As Short

        extra = 0

        Try
            ' C/P does not depend on depth for rigid. Therefore call only once.
            'Call LeafCtoPRigid() ' Leaves results in CtoP(IA, IOFF), used in CDFRigid.

            For IA = 1 To NAC

                LI = LibIndex(IA)
                If AC(LI).libGear = "X" Then ' RigOnRig

                    Dim Depth As Double, IGearLoads As Integer
                    Dim CovToPass As Single
                    Depth = 0 'IGearLoads = AC(LI).libIGear
                    'IA = 1S :

                    For IGearLoads = 1 To AC(LI).libNGroups

                        If IGearLoads = 1 Then

                            For OFFSET = 0 To 400 Step 10
                                Call CoverageToPassRigid13(IA, IGearLoads, OFFSET, CovToPass)
                                CtoP(IA, CInt(OFFSET / 10 + 1)) = CovToPass
                            Next OFFSET

                        ElseIf IGearLoads = 2 Then
                            extra = extra + 1S
                            LibIndex(NAC + extra) = LibIndex(IA)
                            For OFFSET = 0 To 400 Step 10
                                Call CoverageToPassRigid13(IA, IGearLoads, OFFSET, CovToPass)
                                CtoP(NAC + extra, CInt(OFFSET / 10 + 1)) = CovToPass
                            Next OFFSET

                        End If

                    Next

                Else

                    If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
                        extra = extra + 1S 'PPPP
                    End If

                    Call CoverageToPassRigidSingleAC(IA, extra)
                End If
            Next IA


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub



    Public Sub WriteToFileCtoPsub(ByVal DesignType1 As String)
        Dim i, IA, IextraAC As Short
        Dim OFFSET As Integer

        'gPrintCoverages = True
        If gPrintCoverages Then
        Else
            Exit Sub
        End If

        IextraAC = 0
        OFFSET = 0
        FileOpen(18, WorkingDir & "CtoP" & DesignType1 & ".txt", OpenMode.Append)
        PrintLine(18, "")
        For IA = 1 To NAC
            LI = LibIndex(IA)
            PrintLine(18, "JobName: " & JobName & " SectName: " & SectName)
            PrintLine(18, "Aircraft: " & AC(LI).libACName)
            PrintLine(18, "Gear: " & AC(LI).libGear)
            PrintLine(18, "IGear: " & AC(LI).libIGear)
            PrintLine(18, "EvalDepth: " & LEAStrActiveX.EvalDepth)
            PrintLine(18, "")

            Dim s1 As String
            Dim TwoGear As Boolean
            TwoGear = False

            LI = LibIndex(IA)
            If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then
                IextraAC = IextraAC + 1S : TwoGear = True
            End If

            If AC(LI).libGear = "X" And AC(LI).libNGroups = 2 Then ' WriteToFileCtoPsub
                IextraAC = IextraAC + 1S : TwoGear = True
            End If


            If DesignType = NewFlex Or DesignType = FlexOnFlex Then
                For i = 1 To 41
                    OFFSET = (i - 1) * 10
                    If TwoGear Then
                        s1 = LPad(20, CStr(AA1(IA, i))) & LPad(20, CStr(AA2(IA, i)))
                        PrintLine(18, LPad(5, CStr(OFFSET)) & LPad(20, CStr(CtoP(IA, i))) & s1)
                    Else
                        PrintLine(18, LPad(5, CStr(OFFSET)) & LPad(20, CStr(CtoP(IA, i))))
                    End If
                Next i
            Else
                For i = 1 To 41
                    OFFSET = (i - 1) * 10
                    If TwoGear Then
                        s1 = LPad(20, CStr(CtoP(IA + IextraAC, i)))
                        PrintLine(18, LPad(5, CStr(OFFSET)) & LPad(20, CStr(CtoP(IA, i))) & s1)
                    Else
                        PrintLine(18, LPad(5, CStr(OFFSET)) & LPad(20, CStr(CtoP(IA, i))))
                    End If
                Next i
            End If
            PrintLine(18, "")
        Next IA
        FileClose(18)

    End Sub


    Public Sub CreateFAAairplaneLibrary(ByVal ExternalLibFile As String)

        Dim FileNo As Integer, str1, str2 As String
        FileNo = FreeFile()
        FileOpen(FileNo, ExternalLibFile, OpenMode.Output, , , 1024)

        'Dim i111 As Integer = 3

        'If i111 = 2 Then
        '    str1 = "<?xml version=""1.0"" encoding=""utf-8""?>"
        '    str2 = "<FAAairplaneLibrary><AirplaneInfo><Name>C-141A ICAO Flexible</Name><GrossWt>320005</GrossWt><MGpcnt>0.45</MGpcnt><MGpcntPCN>0.45</MGpcntPCN><CP>172.6</CP><Gear>F</Gear><IGear>3</IGear><TT>32.50</TT><TS>177.50</TS><TG>0.00</TG><B>48.00</B><NTires>4</NTires><Wheel_Coordinates><TX>-16.25</TX><TY>0.00</TY><TX>16.25</TX><TY>0.00</TY><TX>-16.25</TX><TY>48.00</TY><TX>16.25</TX><TY>48.00</TY></Wheel_Coordinates><NEVPTS>8</NEVPTS><Evaluation_Points><EVPTX>16.25</EVPTX><EVPTY>0.00</EVPTY><EVPTX>13.00</EVPTX><EVPTY>1.11</EVPTY><EVPTX>9.75</EVPTX><EVPTY>2.22</EVPTY><EVPTX>6.50</EVPTX><EVPTY>3.33</EVPTY><EVPTX>3.25</EVPTX><EVPTY>4.44</EVPTY><EVPTX>0.00</EVPTX><EVPTY>5.56</EVPTY><EVPTX>0.00</EVPTX><EVPTY>14.78</EVPTY><EVPTX>0.00</EVPTX><EVPTY>24.00</EVPTY></Evaluation_Points></AirplaneInfo><AirplaneInfo><Name>SWL 100 ACN</Name><GrossWt>110231</GrossWt><MGpcnt>1.00</MGpcnt><MGpcntPCN>1.00</MGpcntPCN><CP>181.3</CP><Gear>A</Gear><IGear>1</IGear><TT>0.00</TT><TS>0.00</TS><TG>0.00</TG><B>0.00</B><NTires>1</NTires><Wheel_Coordinates><TX>0.00</TX><TY>0.00</TY></Wheel_Coordinates><NEVPTS>1</NEVPTS><Evaluation_Points><EVPTX>0.00</EVPTX><EVPTY>0.00</EVPTY></Evaluation_Points></AirplaneInfo></FAAairplaneLibrary>"
        'ElseIf i111 = 3 Then
        '    str1 = "<?xml version=""1.0"" encoding=""utf-8""?>"

        '    str2 = "<FAAairplaneLibrary><AirplaneInfo><Name>C-141A ICAO Flexible</Name><GrossWt>320005</GrossWt><MGpcnt>0.45</MGpcnt><MGpcntPCN>0.45</MGpcntPCN><CP>172.6</CP><Gear>F</Gear><IGear>3</IGear><TT>32.50</TT><TS>177.50</TS><TG>0.00</TG><B>48.00</B><TV>0.00</TV>"
        '    str2 = str2 & "<NTires>4</NTires><Wheel_Coordinates><TX>-16.25</TX><TY>0.00</TY><TX>16.25</TX><TY>0.00</TY><TX>-16.25</TX><TY>48.00</TY><TX>16.25</TX><TY>48.00</TY></Wheel_Coordinates><NEVPTS>8</NEVPTS><Evaluation_Points><EVPTX>16.25</EVPTX><EVPTY>0.00</EVPTY><EVPTX>13.00</EVPTX><EVPTY>1.11</EVPTY><EVPTX>9.75</EVPTX><EVPTY>2.22</EVPTY><EVPTX>6.50</EVPTX><EVPTY>3.33</EVPTY><EVPTX>3.25</EVPTX><EVPTY>4.44</EVPTY><EVPTX>0.00</EVPTX><EVPTY>5.56</EVPTY><EVPTX>0.00</EVPTX><EVPTY>14.78</EVPTY><EVPTX>0.00</EVPTX><EVPTY>24.00</EVPTY></Evaluation_Points>"

        '    str2 = str2 & "</AirplaneInfo><AirplaneInfo><Name>SWL 100 ACN</Name><GrossWt>110231</GrossWt><MGpcnt>1.00</MGpcnt><MGpcntPCN>1.00</MGpcntPCN><CP>181.3</CP><Gear>A</Gear><IGear>1</IGear><TT>0.00</TT><TS>0.00</TS><TG>0.00</TG><B>0.00</B><TV>0.00</TV><NTires>1</NTires>"
        '    str2 = str2 & "<Wheel_Coordinates><TX>0.00</TX><TY>0.00</TY></Wheel_Coordinates><NEVPTS>1</NEVPTS><Evaluation_Points><EVPTX>0.00</EVPTX><EVPTY>0.00</EVPTY></Evaluation_Points></AirplaneInfo>"

        '    str2 = str2 & "<AirplaneInfo><Name>A380-800ExLib</Name><GrossWt>1238997.845</GrossWt><MGpcnt>0.95</MGpcnt><MGpcntPCN>0.9513</MGpcntPCN><CP>217.6</CP><Gear>X</Gear><IGear>13</IGear>"



        '    str2 = str2 & "<NWheelGroups>2</NWheelGroups><WheelGroupInfo><NTires>4</NTires><Wheel_Coordinates><TX>-271.75</TX><TY>229.35</TY><TX>-271.75</TX><TY>162.45</TY><TX>-218.65</TX><TY>162.45</TY><TX>-218.65</TX><TY>229.35</TY></Wheel_Coordinates><NEVPTS>18</NEVPTS><Evaluation_Points><EVPTX>-218.65</EVPTX><EVPTY>162.45</EVPTY><EVPTX>-223.96</EVPTX><EVPTY>164.87</EVPTY><EVPTX>-229.27</EVPTX><EVPTY>167.30</EVPTY><EVPTX>-234.58</EVPTX><EVPTY>169.72</EVPTY><EVPTX>-239.89</EVPTX><EVPTY>172.14</EVPTY><EVPTX>-245.20</EVPTX><EVPTY>174.57</EVPTY><EVPTX>-245.20</EVPTX><EVPTY>185.23</EVPTY><EVPTX>-245.20</EVPTX>"
        '    str2 = str2 & "<EVPTY>195.90</EVPTY><EVPTX>-217.40</EVPTX><EVPTY>0.00</EVPTY><EVPTX>-217.40</EVPTX><EVPTY>57.34</EVPTY><EVPTX>-217.40</EVPTX><EVPTY>114.68</EVPTY><EVPTX>-217.40</EVPTX><EVPTY>172.01</EVPTY><EVPTX>-217.40</EVPTX><EVPTY>229.35</EVPTY><EVPTX>-271.75</EVPTX><EVPTY>0.00</EVPTY><EVPTX>-271.75</EVPTX><EVPTY>57.34</EVPTY><EVPTX>-271.75</EVPTX><EVPTY>114.68</EVPTY><EVPTX>-271.75</EVPTX><EVPTY>172.01</EVPTY><EVPTX>-271.75</EVPTX><EVPTY>229.35</EVPTY>"
        '    str2 = str2 & "</Evaluation_Points><NTires>6</NTires><Wheel_Coordinates><TX>-133.70</TX><TY>133.80</TY><TX>-134.10</TX><TY>66.90</TY><TX>-133.70</TX><TY>0.00</TY><TX>-73.50</TX><TY>0.00</TY><TX>-73.10</TX><TY>66.90</TY><TX>-73.50</TX><TY>133.80</TY></Wheel_Coordinates><NEVPTS>28</NEVPTS><Evaluation_Points><EVPTX>-73.10</EVPTX><EVPTY>66.90</EVPTY><EVPTX>-77.46</EVPTX><EVPTY>66.90</EVPTY><EVPTX>-81.81</EVPTX><EVPTY>66.90</EVPTY><EVPTX>-86.17</EVPTX><EVPTY>66.90</EVPTY><EVPTX>-90.53</EVPTX><EVPTY>66.90</EVPTY><EVPTX>-94.89</EVPTX><EVPTY>66.90</EVPTY><EVPTX>-99.24</EVPTX><EVPTY>66.90</EVPTY><EVPTX>-103.60</EVPTX><EVPTY>66.90</EVPTY><EVPTX>0.00</EVPTX><EVPTY>0.00</EVPTY><EVPTX>0.00</EVPTX><EVPTY>57.34</EVPTY><EVPTX>0.00</EVPTX><EVPTY>114.68</EVPTY><EVPTX>0.00</EVPTX><EVPTY>172.01</EVPTY><EVPTX>0.00</EVPTX><EVPTY>229.35</EVPTY><EVPTX>-54.35</EVPTX><EVPTY>0.00</EVPTY><EVPTX>-54.35</EVPTX><EVPTY>57.34</EVPTY><EVPTX>-54.35</EVPTX><EVPTY>114.68</EVPTY><EVPTX>-54.35</EVPTX><EVPTY>172.01</EVPTY><EVPTX>-54.35</EVPTX><EVPTY>229.35</EVPTY><EVPTX>-108.70</EVPTX><EVPTY>0.00</EVPTY><EVPTX>-108.70</EVPTX><EVPTY>57.34</EVPTY><EVPTX>-108.70</EVPTX><EVPTY>114.68</EVPTY><EVPTX>-108.70</EVPTX><EVPTY>172.01</EVPTY><EVPTX>-108.70</EVPTX><EVPTY>229.35</EVPTY><EVPTX>-163.05</EVPTX><EVPTY>0.00</EVPTY><EVPTX>-163.05</EVPTX><EVPTY>57.34</EVPTY><EVPTX>-163.05</EVPTX><EVPTY>114.68</EVPTY><EVPTX>-163.05</EVPTX><EVPTY>172.01</EVPTY><EVPTX>-163.05</EVPTX><EVPTY>229.35</EVPTY></Evaluation_Points></WheelGroupInfo></AirplaneInfo>"


        '    =============================
        '    str2 = str2 & "</AirplaneInfo><AirplaneInfo>"
        '    str2 = str2 & "<Name>B777-200 Baseline ExtLib</Name>"
        '    str2 = str2 & "<GrossWt>547000</GrossWt>"
        '    str2 = str2 & "<MGpcnt>0.45</MGpcnt>"
        '    str2 = str2 & "<MGpcntPCN>0.45</MGpcntPCN>"
        '    str2 = str2 & "<CP>182</CP>"
        '    str2 = str2 & "<Gear>X</Gear>"
        '    str2 = str2 & "<IGear>13</IGear>"
        '    str2 = str2 & ""
        '    str2 = str2 & ""
        '    str2 = str2 & ""


        '    str2 = str2 & "<TT>0.00</TT><TS>0.00</TS><TG>0.00</TG><B>0.00</B><TV>0.00</TV>"
        '    str2 = str2 & "<NTires>1</NTires><Wheel_Coordinates><TX>0.00</TX><TY>0.00</TY></Wheel_Coordinates>"
        '    str2 = str2 & "<NEVPTS>1</NEVPTS><Evaluation_Points><EVPTX>0.00</EVPTX><EVPTY>0.00</EVPTY></Evaluation_Points></AirplaneInfo>"
        '    str2 = str2 & ""
        '    ===============================


        '    str2 = str2 & "</FAAairplaneLibrary>"

        'End If


        Print(FileNo, str1)
        PrintLine(FileNo, str2)
        FileClose(FileNo)

    End Sub

    Private Function LibExAirplaneA380() As String
        Dim str99 As String


        LibExAirplaneA380 = str99
    End Function





    Public Sub print_aircraft_list()
        'DO 0007 4.2.5.1.6. Updated aircraft library listing.
        'Modifications to 6E



        Dim I, J As Integer

        FileOpen(9, "AircraftList.txt", OpenMode.Output, , , 1024)
        PrintLine(9, "FAARFIELD 2013.04.19")
        PrintLine(9, "v 1.4")
        PrintLine(9, " ")


        For ILibACGroup = 1 To NLibACGroups
            PrintLine(9, LibACGroupName$(ILibACGroup))

            If ILibACGroup = NLibACGroups Then
                J = libNAC - NBelly
                'J = libNAC
            Else
                J = LibACGroup(ILibACGroup + 1) - 1
            End If


            Dim ss1 As String
            For I = LibACGroup(ILibACGroup) To J
                'PrintLine(9, I & " " & AC(I).libACName)
                ss1 = "0"
                If AC(I).libGear = "A" Or AC(I).libGear = "B" Then
                    ss1 = "S"
                ElseIf AC(I).libGear = "D" Then
                    ss1 = "D"
                ElseIf AC(I).libGear = "E" Then
                    ss1 = "2S"
                ElseIf AC(I).libGear = "F" Then
                    ss1 = "2D"
                ElseIf AC(I).libGear = "N" Then
                    ss1 = "3D"
                ElseIf AC(I).libGear = "WFBF" Then
                    ss1 = "2D/2D2"
                ElseIf AC(I).libGear = "WFBN" Then
                    ss1 = "2D/3D2"
                ElseIf Mid(AC(I).libACName, 1, 8) = "A340-200" And AC(I).libGear = "H" Then
                    ss1 = "2D/D1"
                ElseIf Mid(AC(I).libACName, 1, 8) = "A340-300" And AC(I).libGear = "H" Then
                    ss1 = "2D/D1"
                ElseIf Mid(AC(I).libACName, 1, 8) = "A340-500" And AC(I).libGear = "H" Then
                    ss1 = "2D/2D1"
                ElseIf Mid(AC(I).libACName, 1, 8) = "A340-600" And AC(I).libGear = "H" Then
                    ss1 = "2D/2D1"
                ElseIf AC(I).libACName = "An-124" Then
                    ss1 = "5D"
                ElseIf AC(I).libACName = "An-224" Then
                    ss1 = "7D"
                ElseIf AC(I).libACName = "IL76T" Then
                    ss1 = "3Q"
                ElseIf AC(I).libGear = "Z" Then
                    ss1 = "Complex"
                ElseIf AC(I).libACName = "C-5" Or AC(I).libACName = "C-17A" Then
                    ss1 = "Complex"
                ElseIf AC(I).libACName = "DC10-30/40" Or AC(I).libACName = "KC-10" Then
                    ss1 = "2D/D1"
                ElseIf AC(I).libACName = "IL86" Or AC(I).libACName = "MD11ER" Then
                    ss1 = "2D/2D1"

                End If

                PrintLine(9, LPad(4, CStr(I)) & " " & LPad(30, AC(I).libACName) & LPad(17, Format(AC(I).libGL, "###,##0.00")) & LPad(9, ss1) & LPad(9, AC(I).libGear))
            Next I

            PrintLine(9, "")

        Next ILibACGroup


        For I = libNAC - NBelly + 1 To libNAC
            PrintLine(9, LPad(4, CStr(I)) & " " & LPad(30, AC(I).libACName) & LPad(17, Format(AC(I).libGL, "###,##0.00")) & LPad(9, AC(I).libGear))
        Next


        FileClose(9)

    End Sub



    Public Sub print_aircraft_list2()


        Dim I, J, K3, G1 As Integer
        Dim str1 As String
        str1 = WorkingDir & "AircraftListFF2.txt"

        FileOpen(9, str1, OpenMode.Output, , , 1024)
        PrintLine(9, "FAARFIELD Aircraft List")
        PrintLine(9, "print_aircraft_list2()")
        PrintLine(9, " ")


        For ILibACGroup = 1 To NLibACGroups + 1S

            If ILibACGroup = NLibACGroups + 1S Then
                PrintLine(9, "Belly Group")
            Else
                PrintLine(9, LibACGroupName$(ILibACGroup) & " Group")
            End If

            Print(9, LPad(4, "#") & " " & LPad(30, "AC_Name"))
            Print(9, LPad(12, "GL"))
            Print(9, LPad(8, "MG%"))
            Print(9, LPad(10, "CP"))
            Print(9, LPad(6, "Gear"))
            Print(9, LPad(6, "IGear"))
            Print(9, LPad(10, "TT"))
            Print(9, LPad(10, "B"))
            Print(9, LPad(10, "TS"))
            Print(9, LPad(10, "TG"))
            PrintLine(9, LPad(7, "Tire"))


            If ILibACGroup = NLibACGroups Then
                J = libNAC - NBelly
                'J = libNAC
                G1 = LibACGroup(ILibACGroup)
            ElseIf ILibACGroup = NLibACGroups + 1 Then
                J = libNAC
                G1 = libNAC - NBelly + 1
            Else
                J = LibACGroup(ILibACGroup + 1) - 1
                G1 = LibACGroup(ILibACGroup)
            End If



            For I = G1 To J
                Print(9, LPad(4, CStr(I)) & " " & LPad(30, AC(I).libACName))
                Print(9, LPad(12, CStr(Format(AC(I).libGL, "0.000"))))
                Print(9, LPad(8, CStr(Format(AC(I).libMGpcnt, "0.000"))))
                Print(9, LPad(10, CStr(Format(AC(I).libCP, "0.000"))))
                Print(9, LPad(6, CStr(AC(I).libGear)))
                Print(9, LPad(6, CStr(AC(I).libIGear)))
                Print(9, LPad(10, CStr(Format(AC(I).libTT, "0.000"))))
                Print(9, LPad(10, CStr(Format(AC(I).libB, "0.000"))))
                Print(9, LPad(10, CStr(Format(AC(I).libTS, "0.000"))))
                Print(9, LPad(10, CStr(Format(AC(I).libTG, "0.000"))))
                Print(9, LPad(7, CStr(AC(I).libNTires)))

                For K3 = 1 To AC(I).libNTires - 1
                    Print(9, LPad(11, CStr(Format(AC(I).libTX(K3), "0.000"))))
                    Print(9, LPad(11, CStr(Format(AC(I).libTY(K3), "0.000"))))
                Next

                Print(9, LPad(11, CStr(Format(AC(I).libTX(K3), "0.000"))))
                PrintLine(9, LPad(11, CStr(Format(AC(I).libTY(K3), "0.000"))))

            Next I

            PrintLine(9, "")

        Next ILibACGroup



        FileClose(9)

    End Sub



    Public Sub print_aircraft_list3()


        Dim I, J As Integer

        FileOpen(9, "AircraftListFF.txt", OpenMode.Output, , , 1024)
        PrintLine(9, "FAARFIELD Aircraft List")
        PrintLine(9, "print_aircraft_list2()")
        PrintLine(9, " ")


        For ILibACGroup = 1 To 1  ' NLibACGroups
            PrintLine(9, LibACGroupName$(ILibACGroup) & " Group")
            Print(9, LPad(3, "#") & " " & LPad(15, "AC_Name"))
            Print(9, LPad(9, "GL"))
            Print(9, LPad(6, "MG%"))
            Print(9, LPad(7, "CP"))
            Print(9, LPad(6, "Gear"))
            Print(9, LPad(6, "IGear"))
            Print(9, LPad(7, "TT"))
            Print(9, LPad(7, "B"))
            PrintLine(9, LPad(7, "TS"))

            If ILibACGroup = NLibACGroups Then
                J = libNAC - NBelly
                'J = libNAC
            Else
                J = LibACGroup(ILibACGroup + 1) - 1
            End If

            For I = LibACGroup(ILibACGroup) To J
                Print(9, LPad(3, CStr(I)) & " " & LPad(15, AC(I).libACName))
                Print(9, LPad(9, CStr(AC(I).libGL)))
                Print(9, LPad(6, CStr(Format(AC(I).libMGpcnt, "0.000"))))
                Print(9, LPad(7, CStr(AC(I).libCP)))
                Print(9, LPad(6, CStr(AC(I).libGear)))
                Print(9, LPad(6, CStr(AC(I).libIGear)))
                Print(9, LPad(7, CStr(AC(I).libTT)))
                Print(9, LPad(7, CStr(AC(I).libB)))
                PrintLine(9, LPad(7, CStr(AC(I).libTS)))
            Next I

            PrintLine(9, "")

        Next ILibACGroup

        FileClose(9)

    End Sub



    ' June 26, 2008 
    ' AC(IA).libACName = "B787-8 (Prelim)"
    ' AC(IA).libACName = "B787-9 (Prelim)"

    '    ElseIf ACName(I) = "B787-8" Then
    '    MatchFound = True
    '    ACName(I) = "B787-8 (Prelim)"
    '    jobACName(ISect, I) = ACName(I)
    'ElseIf ACName(I) = "B787-9" Then
    '    MatchFound = True
    '    ACName(I) = "B787-9 (Prelim)"
    '    jobACName(ISect, I) = ACName(I)
    'End If





    '            If File.Exists(XMLpath) Then
    '            Call ReadExternalXMLFile(AC, libNAC)

    '            If File.Exists(LEDFAApath) Then
    '                System.IO.Directory.CreateDirectory(ACDATPath + "old_job_files")

    '                If Not File.Exists(ACDATPath + "old_job_files\LEDFAAacLibrary.Ext") Then
    '                    System.IO.File.Move(ACDATPath & "LEDFAAacLibrary.Ext", ACDATPath & "old_job_files\LEDFAAacLibrary.Ext")
    '                End If

    '            End If



    '        ElseIf File.Exists(LEDFAApath) Then
    'Dim iStart As Integer
    '            iStart = libNAC
    '            Call ReadExternalFile(AC, libNAC) ' Sub checks for too many aircraft.
    '            Call WriteExternalXMLFile(AC, iStart, libNAC)

    '            If File.Exists(LEDFAApath) Then
    '                System.IO.Directory.CreateDirectory(ACDATPath + "old_job_files")

    '                If Not File.Exists(ACDATPath + "old_job_files\LEDFAAacLibrary.Ext") Then
    '                    System.IO.File.Move(ACDATPath & "LEDFAAacLibrary.Ext", ACDATPath & "old_job_files\LEDFAAacLibrary.Ext")
    '                End If

    '            End If

    '        End If












    'If Dir(ACDATPath$ & ExternalAircraftFileName$ & ".Ext") = "" Then
    '    ExternalLibraryActive = False
    'Else
    '    ExternalLibraryActive = True
    'End If

    '        ExternalLibraryActive = True








    'July 7, 2008
    'Friend Class dlgAddDelete
    'Private Sub dlgAddDelete_Load(

    '    'Left = CInt(VB6.TwipsToPixelsX(LastFormLeft + VB6.PixelsToTwipsX(Left) - FirstLeft))
    '    'Top = CInt(VB6.TwipsToPixelsY(LastFormTop + VB6.PixelsToTwipsY(Top) - FirstTop))

    '    Left = Left() + Me.Width / 1.3








    'July 11, 2008
    'Private Sub frmLoadFile_Load(ByVal eventSender As System.Object, ByVal eventArgs As System.EventArgs) Handles MyBase.Load
    'Me.btnGraphCDF.Enabled = buttonCDFgraph


    'add lastIndex
    'lastIndex = lstStrFiles.SelectedIndex




End Module
