Option Strict On
Option Explicit On

Imports System.Xml
Imports System.Threading
Imports System.Globalization

Module FILEPROC
    Public Sxml(100) As Date

    ' Define a data type to hold a record:
    ' Define global variables to hold the file number and record number
    ' of the current data file.
    ' Default file name to show in dialog boxes.
    Public Const Err_DeviceUnavailable As Short = 68
	Public Const Err_DiskNotReady As Short = 71
	Public Const Err_FileAlreadyExists As Short = 58
	Public Const Err_TooManyFiles As Short = 67
	Public Const Err_RenameAcrossDisks As Short = 74
	Public Const Err_Path_FileAccessError As Short = 75
	Public Const Err_DeviceIO As Short = 57
	Public Const Err_DiskFull As Short = 61
	Public Const Err_BadFileName As Short = 64
	Public Const Err_BadFileNameOrNumber As Short = 52
	Public Const Err_FileNotFound As Short = 53
	Public Const Err_PathDoesNotExist As Short = 76
	Public Const Err_BadFileMode As Short = 54
	Public Const Err_FileAlreadyOpen As Short = 55
	Public Const Err_InputPastEndOfFile As Short = 62
	Public Const MB_EXCLAIM As Short = 48

    Public Const JobFileFormat As String = "Format3"

    Public CallAC() As LEAFClassLib.clsLEAF.LEAFACParms        'Holds data for aircraft passed to LEAF.
    Public LEAStrActiveX As LEAFClassLib.clsLEAF.LEAFStrParms  'Holds data passed to LEAF.
    Public AllResp(,) As LEAFClassLib.clsLEAF.LEAFAllResponses  'Dimensioned and returned by LEAF
    'after computing AllResponses.

    Public RunLEAF As LEAFClassLib.clsLEAF
    Public I1, I2, J As Integer

    Function FileErrors(ByRef errVal As Integer, ByRef FileName As String) As Short
        ' Return Value  Meaning     Return Value    Meaning
        ' 0             Resume      2               Unrecoverable error
        ' 1             Resume Next 3               Unrecognized error
        Dim MsgType As Short
        Dim Response As Long
        Dim MSG As String
        MsgType = MB_EXCLAIM
        Select Case errVal
            Case Err_DeviceUnavailable ' Error #68
                MSG = "The disk appears to be unavailable."
                MsgType = MB_EXCLAIM + 5
            Case Err_DiskNotReady ' Error #71
                MSG = "The disk is not ready."
            Case Err_DeviceIO
                MSG = "The disk is full."
            Case Err_BadFileName, Err_BadFileNameOrNumber ' Errors #64 & 52
                MSG = FileName & " path or file name is illegal."
            Case Err_PathDoesNotExist ' Error #76
                MSG = "The path for " & FileName & " doesn't exist."
            Case Err_BadFileMode ' Error #54
                MSG = "Can't open " & FileName & " for that type of access."
            Case Err_FileAlreadyOpen ' Error #55
                MSG = FileName & " is already open."
            Case Err_InputPastEndOfFile ' Error #62
                MSG = FileName & " has a nonstandard end-of-file marker,"
                MSG = MSG & "or an attempt was made to read beyond "
                MSG = MSG & "the end-of-file marker."
            Case Err_FileNotFound
                MSG = "Cannot find " & FileName & "."
            Case Else
                FileErrors = 3
                MSG = "Unknown file error"
                Exit Function
        End Select

        Response = MsgBoxDQ(MSG, MsgType, "File Error")
        Select Case Response
            Case 4 ' Retry button.
                FileErrors = 0
            Case 5 ' Ignore button.
                FileErrors = 1
            Case 1, 2, 3 ' Ok and Cancel buttons.
                FileErrors = 2
            Case Else
                FileErrors = 3
        End Select
    End Function

    Sub MakeDecimalPeriod(ByRef SN As String)

        Dim IP As Integer

        IP = InStr(SN, ",")
        If IP <> 0 Then
            Mid(SN, IP) = "."
        End If

    End Sub

    ''' <summary>
    ''' Reads non-XML job file.
    ''' </summary>
    ''' <param name="RetErr">File error</param>
    Sub ReadJobFile(ByRef RetErr As Short)

        Dim I, J As Integer
        Dim FileName As String
        Dim JFNo As Integer
        Dim IErr, L As Integer
        Dim FileFormat As String
        Dim IL As Integer
        Dim INos As Short
        On Error GoTo OldRJobFileError

        RetErr = CShort(False)
        FileName = WorkingDir & JobName & ".JOB"
        JFNo = FreeFile()
        FileOpen(JFNo, FileName, OpenMode.Input, , , 1024)

        If EOF(JFNo) Then
            S = "File " & WorkingDir & JobName & ".JOB is empty." & NL2
            S = S & "Delete the job and create a new one." & NL2
            S = S & "No action will be taken now."
            FileClose(JFNo)
            Exit Sub
        End If

        FileFormat = LineInput(JFNo)

        I = 0
        Do
            If I = MaxSects Then
                SS = MaxSects.ToString("f0")
                S = "The maximum number of sections allowed in a" & NL
                S = S & " job file is " & SS & "."
                S = S & " There are more sections than" & NL
                S = S & " this in " & JobName & "." & NL2
                S = S & " Only the first " & SS & " sections in "
                S = S & JobName & " will be loaded."
                Ret = MsgBoxDQ(S, 0, "Too Many Sections")
                Exit Do
            End If
            If EOF(JFNo) Then Exit Do

            I = I + 1
            jobSectName(I) = LineInput(JFNo)
            If FileFormat <> JobFileFormat Then
                Input(JFNo, jobDesigned(I))
                Input(JFNo, jobCDFAsp(I))
                If jobDesigned(I) = Nothing Then jobDesigned(I) = NullDate
            Else
                Dim info As New System.Globalization.DateTimeFormatInfo()
                jobDesigned(I) = Date.Parse(InputString(JFNo, 20), info)

                'jobDesigned(I) = CDate(InputString(JFNo, 20))
                S = InputString(JFNo, 20)
                Input(JFNo, jobCDFAsp(I))
            End If
            Input(JFNo, jobLife(I))
            Input(JFNo, jobSCIB(I))
            Input(JFNo, jobLifeExistPCC(I))
            Input(JFNo, jobNPLayers(I))
            Input(JFNo, jobNAC(I))
            If jobSCIB(I) < 100.0! Then jobLifeExistPCC(I) = 100.0! ' *****

            For J = 1 To jobNPLayers(I)
                Input(JFNo, jobThick(I, J))
                Input(JFNo, jobModulus(I, J))
                Input(JFNo, jobLCode(I, J))
                S = LayerType(jobLCode(I, J))
                If S = NPCC Or S = NPCCOU Or S = NPCCOB Or S = NPCCOF Then
                    jobRCon(I, J) = jobModulus(I, J)
                    jobModulus(I, J) = DefaultModulus(jobLCode(I, J))
                Else
                    jobRCon(I, J) = 0.0!
                End If
            Next J
            jobThick(I, jobNPLayers(I)) = 0.0!

            SS = MaxSectAC.ToString("f0")
            If jobNAC(I) > MaxSectAC Then

                S = "The maximum number of aircraft allowed in a" & NL
                S = S & " design list is " & SS & "."
                S = S & " There are more aircraft than" & NL
                S = S & " this in " & JobName & "." & NL2
                S = S & " Only the first " & SS & " aircraft in "
                S = S & JobName & " will be loaded."
                Ret = MsgBoxDQ(S, 0, "Too Many Aircraft")
                IErr = jobNAC(I)
                jobNAC(I) = MaxSectAC
            End If


            For J = 1 To jobNAC(I)
                If FileFormat = JobFileFormat Then
                    jobACName(I, J) = Trim(InputString(JFNo, 21))
                    'If jobACName$(I, J) = "B-767" Then jobACName$(I, J) = "B-767-200"
                    'If jobACName$(I, J) = "DC-9" Then jobACName$(I, J) = "DC-9-30"
                    'If jobACName$(I, J) = "MD-80" Then jobACName$(I, J) = "MD-82/88"
                    'If jobACName$(I, J) = "B-777" Then jobACName$(I, J) = "B-777-200 B"
                    'If jobACName$(I, J) = "DC-10-30 (Wing)" Then jobACName$(I, J) = "DC-10-30"
                    'If jobACName$(I, J) = "C-17" Then jobACName$(I, J) = "C-17A"

                    If jobACName$(I, J) = "A-340" Then jobACName$(I, J) = "A340-200/300"
                    If jobACName$(I, J) = "A-340 Belly" Then jobACName$(I, J) = "A340-200/300" & BellyExt$
                    If Left(jobACName$(I, J), 2) = "A-" Then
                        jobACName$(I, J) = "A" & Right(jobACName$(I, J), Len(jobACName$(I, J)) - 2)
                    End If
                    If jobACName(I, J) = "DT-777 C" Then jobACName(I, J) = "DT-777"
                    '       3 lines added 06-18-04, GFH.
                    If jobACName(I, J) = "B-777-200 A" Then jobACName(I, J) = "B-777-200"
                    If jobACName(I, J) = "B-777-200 B" Then jobACName(I, J) = "B-777-200ER"
                    If jobACName(I, J) = "B-777-200 C" Then jobACName(I, J) = "B-777-300"
                    If jobACName(I, J) = "DC-9-30" Then jobACName(I, J) = "DC-9-32" 'ikawa
                    If jobACName(I, J) = "DC-9-50" Then jobACName(I, J) = "DC-9-51" 'ikawa

                    If jobACName(I, J) = "B747-200B Combi Mixd" Then jobACName(I, J) = "B747-200B Combi Mixed"

                Else
                    jobACName(I, J) = Trim(InputString(JFNo, 12))
                End If
                Input(JFNo, jobGL(I, J))
                Input(JFNo, jobRepsAnnual(I, J))
                Input(JFNo, jobRepsInc(I, J))
            Next J


            For J = MaxSectAC + 1 To CShort(IErr)
                S = LineInput(JFNo)
            Next J

            If FileFormat = JobFileFormat Then
                S = LineInput(JFNo)
                S = LineInput(JFNo)
            End If
        Loop
        NSects = CShort(I)
        If NSects = 0 Then ISect = 0 Else ISect = 1
        FileClose(JFNo)
        Exit Sub

OldRJobFileError:
        RetErr = CShort(True)
        IErr = Err.Number

        Ret = FileErrors(IErr, FileName)

        If Ret = 0 Then Resume

        If Ret = 3 Then
            S = "Error # " & IErr.ToString("f0") & " occurred reading" & NL
            S = S & "file " & FileName & "." & NL2
            S = S & "It may have been caused by a corrupted file or by" & NL
            S = S & "a file with incorrect format. Please check the file." & NL2
            S = S & "If the error occurred during startup, try loading" & NL
            S = S & "another job or create a new job. Repair or delete" & NL
            S = S & "the bad job file as soon as possible."
            Ret = MsgBoxDQ(S, 0, "File Error")
        End If
        FileClose(JFNo)
        Exit Sub

    End Sub

    Sub ReadSectNotes()
        Dim IErr, I, SLen As Integer
        Dim FileName As String
        Dim NFNo As Integer
        On Error GoTo RSectNotesFileError

        FileName = WorkingDir & JobName & ".NTS" 'ikawa01    ACDATPath
        NFNo = FreeFile()

        If Dir(FileName) = "" Then
            I = 0
        Else
            FileOpen(NFNo, FileName, OpenMode.Input, , , 1024)
            I = CInt(LOF(NFNo)) ' Check for zero length.
            FileClose(NFNo)
        End If

        If I = 0 Then
            Call WriteNotesFile(jobSectName(ISect), 0)
        End If

        NFNo = FreeFile()
        FileOpen(NFNo, FileName, OpenMode.Input, , , 1024)

        For I = 1 To ISect
            S = LineInput(NFNo) ' Section name.
            Input(NFNo, SLen) ' Length of NotesString.
            NotesString = InputString(NFNo, SLen)
            Input(NFNo, SS) ' Get CRLF.

            'If S$ <> jobSectName$(I) Or EOF(NFNo) Then ' Bad file.   ' GFH 3/27/03 EOF added.

            If S$ <> jobSectName$(I) Then
                FileClose(NFNo) ' Let WriteNotesFile find and fix.
                Call WriteNotesFile(jobSectName(ISect), 0)
                Exit Sub
            End If

        Next I

        FileClose(NFNo)

        Exit Sub

RSectNotesFileError:

        IErr = Err.Number

        Ret = FileErrors(IErr, FileName$)
        If Ret = Err_InputPastEndOfFile Then Resume Next ' GFH 3/27/03 Error correction added.
        If Ret = 0 Then Resume

        If Ret = 3 Then
            S = "Error # " & Err.ToString() & " occurred reading" & NL
            S = S & "file " & FileName & "."
            Ret = MsgBoxDQ(S, 0, "File Error")
        End If
        FileClose(NFNo)
        Exit Sub

    End Sub

    Sub WriteFile() 'ik2020.03

        Dim J, I, LI As Short
        Dim IA As Integer
        Dim NTemp As Short
        Dim S As String
        Dim TempPois As Double
        Dim FileName As String
        Dim SFNo, LFNo As Integer
        Dim StrThick As Single
        Dim IErr As Integer
        Dim TRAD, TLOAD, TAREA, TPRESS As Single
        Dim VariableSGPoisson As Boolean ' Set in Sub WriteJuleaFile
        Static NotFirstTime As Boolean
        Static DefaultPoissonSGACsave As Double
        Const FExp As String = "0.0000000E-00" ' Format string.
        On Error GoTo WJuleaFileError

        VariableSGPoisson = False

        If NotFirstTime Then
            DefaultPoissonSGAC = CSng(DefaultPoissonSGACsave)
        Else
            DefaultPoissonSGACsave = DefaultPoissonSGAC
            NotFirstTime = True
        End If

        '  Modification on 2/11/96
        If julNPLayers > MaxNPLayers Then
            SS = MaxNPLayers.ToString("f0")
            S = "Sublayering in aggregate layers has exceeded" & NL
            S = S & "the allowed number of layers in a structure; "
            S = S & SS & "."
            S = S & NL & "The design cannot be continued. Try adding one"
            'S = S & NL & "or more undefined layers for an approximate design."
            S = S & NL & "or more user defined layers for an approximate design."
            Ret = MsgBoxDQ(S, 0, "Too Many Layers")
            DesigningStr = False
            Exit Sub
        End If

        With LEAStructure
            .NLayers = julNPLayers
            ReDim .Thick(julNPLayers)
            ReDim .Modulus(julNPLayers)
            ReDim .Poisson(julNPLayers)
            ReDim .InterfaceCode(julNPLayers)
            ReDim .LayerCode(julNPLayers)
            ReDim .EvalDepth(NEvalDepths)
        End With

        'FileName = ACDATPath & "JULEA.STR"
        'FileName$ = ACDATPath$ & "Leaf.str" ' Julea File GFH 04/24/03.
        'FileName$ = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\Temporary\Leaf.str" ' Julea File GFH 04/24/03.
        FileName$ = WorkingDir & "Leaf.str" 'ik2020.03
        SFNo = FreeFile()
        NoOutFiles = True
        If Not NoOutFiles Then
            FileOpen(SFNo, FileName, OpenMode.Output, , , 1024)

            ' Structure file for JULEA.
            PrintLine(SFNo, " STRUCTURE Data File")
            PrintLine(SFNo, " Job Title")
            PrintLine(SFNo, "Title")
            PrintLine(SFNo, " Number of Pavements")
            PrintLine(SFNo, LPad(2, NPVMNTS.ToString("f0")))
            PrintLine(SFNo, " Number Thicknesses & Moduli Variations")
            PrintLine(SFNo, LPad(12, NTH.ToString("f0")) & LPad(12, NMO.ToString("f0")))
        End If



        julRCon = 0.0!
        For I = 1 To NPLayers
            If RCon(I) > 0.0! Then julRCon = RCon(I)
        Next I

        If Not NoOutFiles Then
            PrintLine(SFNo, " Pavement Description")
            PrintLine(SFNo, SectName)
            PrintLine(SFNo, " Slab Flexural Strength (only for rigid pavements)")
        End If

        S = LPad(20, julRCon.ToString("f8")) : Call MakeDecimalPeriod(S)
        If Not NoOutFiles Then
            PrintLine(SFNo, S)
        End If

        LEAStructure.RCon = julRCon
        If Not NoOutFiles Then
            PrintLine(SFNo, " No. of Layers")
            PrintLine(SFNo, LPad(4, julNPLayers.ToString("f0")))
            PrintLine(SFNo, "  Layer      Thicknesses     Modulus of   Poisson's   Interface")
            PrintLine(SFNo, "  Number       ( in )        Elasticity     Ratio     Condition     Layer Code")
            PrintLine(SFNo, "                              ( psi )")
            PrintLine(SFNo, "  -------------------------------------------------------------")
        End If

        For I = 1 To julNPLayers
            If I < julNPLayers Then
                If Not NoOutFiles Then
                    Print(SFNo, LPad(5, I.ToString("f0")))
                End If

                S = LPad(20, julThick(I).ToString(FExp)) : Call MakeDecimalPeriod(S)
                If Not NoOutFiles Then
                    Print(SFNo, S)
                End If

                LEAStructure.Thick(I) = julThick(I)
                S = LPad(15, julModulus(I).ToString(FExp)) : Call MakeDecimalPeriod(S)
                If Not NoOutFiles Then
                    Print(SFNo, S)
                End If

                LEAStructure.Modulus(I) = julModulus(I)

                TempPois = DefaultPoisson(julLCode(I))
                If AlternateSG And VariableSGPoisson Then
                    '        DefaultPoissonSGAC = Temp   ' To print correctly in Info text.
                    Temp = 0.0!
                    For IA = 1 To I ' julNPLayers - 1
                        Temp = Temp + julThick(IA)
                        If Temp > EvalDepth(1) And IA = I Then
                            '           Poisson's Ratio as a function of subgrade modulus.
                            TempPois = 0.82 - 0.1 * System.Math.Log(julModulus(I) / 145)
                            If TempPois > 0.45 Then TempPois = 0.45
                            If TempPois < 0.35 Then TempPois = 0.35
                            Exit For
                        End If
                    Next IA
                End If

                'If gPoissonsRatio And julLCode(I) = 0 Then
                If julLCode(I) = 0 Then
                    'TempPois = UserDefPoissonRatio(ISect, I)
                    TempPois = leafPoissonsRatio(I)
                End If

                System.Diagnostics.Debug.WriteLine("VarPois = " & I & TempPois)
                S = LPad(14, TempPois.ToString(FExp)) : Call MakeDecimalPeriod(S)
                If Not NoOutFiles Then
                    Print(SFNo, S)
                End If

                LEAStructure.Poisson(I) = TempPois
                If Not NoOutFiles Then
                    PrintLine(SFNo, LPad(18, julLCode(I).ToString("f0")))
                End If

                LEAStructure.LayerCode(I) = julLCode(I)

                'Debug.Print I; julThick(I); julModulus(I), DefaultPoisson(julLCode(I)); julLCode(I)
            Else ' Subgrade
                If Not NoOutFiles Then
                    Print(SFNo, LPad(5, I.ToString()), SPC(20))
                End If

                S = LPad(15, julModulus(I).ToString(FExp)) : Call MakeDecimalPeriod(S)
                If Not NoOutFiles Then
                    Print(SFNo, S)
                End If

                LEAStructure.Modulus(I) = julModulus(I)
                If RCon(1) > 0.0! Or RCon(2) > 0.0! Then
                    Temp = DefaultPoissonSGPCC ' Subgrade Poisson's Ratio is
                Else ' different for rigid and
                    Temp = DefaultPoissonSGAC ' and flexible pavements. May change.
                    If VariableSGPoisson Then
                        '         Poisson's Ratio as a function of subgrade modulus.
                        Temp = CSng(0.82 - 0.1 * System.Math.Log(julModulus(I) / 145))
                        If Temp > 0.45 Then Temp = 0.45
                        If Temp < 0.35 Then Temp = 0.35
                        DefaultPoissonSGAC = Temp ' To print correctly in Info text.
                        System.Diagnostics.Debug.WriteLine("VarPois = " & Temp)
                    End If
                End If
                If LayerType(julLCode(I)) = NND Then ' The Undefined layer can
                    Temp = DefaultPoisson(julLCode(I)) ' be on the bottom also.
                End If
                S = LPad(14, Temp.ToString(FExp)) : Call MakeDecimalPeriod(S)
                If Not NoOutFiles Then
                    Print(SFNo, S)
                End If

                LEAStructure.Poisson(I) = Temp
                '      Print #SFNo, LPad$(11, Format(DefaultPoisson(julLCode(I)), "0.00"));
                If Not NoOutFiles Then
                    PrintLine(SFNo, LPad(18, julLCode(I).ToString("f0")))
                End If

                LEAStructure.LayerCode(I) = julLCode(I)
                'Debug.Print I; julModulus(I), DefaultPoisson(julLCode(I)); julLCode(I)
            End If
            If I <> julNPLayers Then
                '     Interface friction, interleaved in file format.
                If Not NoOutFiles Then
                    Print(SFNo, SPC(51))
                    PrintLine(SFNo, LPad(12, DefaultInterface(julLCode(I)).ToString("f0")))
                End If

                LEAStructure.InterfaceCode(I) = DefaultInterface(julLCode(I))
                '     Debug.Print DefaultInterface(julLCode(I))
            End If
        Next I

        If Not NoOutFiles Then
            PrintLine(SFNo, " No. of Depths")
            PrintLine(SFNo, LPad(4, NEvalDepths.ToString("f0")))
            PrintLine(SFNo, "  Depth No.       Depth (In)")
            PrintLine(SFNo, "  ---------------------------")
        End If

        LEAStructure.NEvalDepths = NEvalDepths
        For I = 1 To NEvalDepths
            If Not NoOutFiles Then
                Print(SFNo, LPad(6, I.ToString("f0")))
            End If

            S = LPad(19, EvalDepth(I).ToString("f6")) : Call MakeDecimalPeriod(S)
            If Not NoOutFiles Then
                PrintLine(SFNo, S)
            End If

            LEAStructure.EvalDepth(I) = EvalDepth(I)
            '   Debug.Print EvalDepth(I)
        Next I

        If Not NoOutFiles Then
            FileClose(SFNo)
        End If


        ReDim LEAAircraft(NAC)

        ' Load file for JULEA.
        'FileName = ACDATPath & "JULEA.LOA"
        'FileName$ = ACDATPath$ & "Leaf.loa" ' Julea File GFH 04/24/03.
        'FileName$ = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\Temporary\Leaf.loa" ' Julea File GFH 04/24/03.
        FileName$ = WorkingDir & "Leaf.loa" 'ik2020.03

        LFNo = FreeFile()

        If Not NoOutFiles Then
            FileOpen(LFNo, FileName, OpenMode.Output, , , 1024)

            PrintLine(LFNo, " LOAD Data File")
            PrintLine(LFNo, " Job Title")
            PrintLine(LFNo, "Title")
            PrintLine(LFNo, " No. Of Aircraft")
            PrintLine(LFNo, LPad(2, NAC.ToString("f0")))
        End If


        For I = 1 To NAC
            LI = LibIndex(I)
            If Not NoOutFiles Then
                PrintLine(LFNo, " Aircraft Identification #  " & I)
                PrintLine(LFNo, ACName(I))
                PrintLine(LFNo, " Gross Load")
            End If

            S = LPad(12, GL(I).ToString("f2")) : Call MakeDecimalPeriod(S)
            If Not NoOutFiles Then
                PrintLine(LFNo, S)
            End If

            LEAAircraft(I).GearLoad = GL(I) * MGpcnt(I) ' GFH 04/22/03.



            'LEAAircraft(I).GearLoad = GL(I) * AC(LI).libMGpcnt
            If Not NoOutFiles Then
                PrintLine(LFNo, " Fraction Of Gross Load On Gear To be Analyzed")
            End If

            S$ = LPad$(14, Format(MGpcnt(I), FExp$)) : Call MakeDecimalPeriod(S$) ' GFH 04/22/03.
            'S = LPad(14, Format(AC(LI).libMGpcnt, FExp)) : Call MakeDecimalPeriod(S)
            If Not NoOutFiles Then
                PrintLine(LFNo, S)
                PrintLine(LFNo, " No. Of Tires")
                PrintLine(LFNo, LPad(2, AC(LI).libNTires.ToString("f0")))
            End If


            LEAAircraft(I).NTires = AC(LI).libNTires
            LEAAircraft(I).NEvalPoints = AC(LI).libNEVPTS

            With LEAAircraft(I)
                ReDim .TirePress(.NTires)
                ReDim .TireX(.NTires)
                ReDim .TireY(.NTires)
                ReDim .EvalX(.NEvalPoints)
                ReDim .EvalY(.NEvalPoints)
            End With

            If Not NoOutFiles Then
                PrintLine(LFNo, " Tire  Radius  Cont.Area  Cont.Press.   Tire Load   X-coord.   Y-coord.")
                PrintLine(LFNo, " No.    (In)(sq.in)(psi)(pounds)(in)(in)  ")
                PrintLine(LFNo, " ------------------------------------------------------------------------")
            End If

            TLOAD = GL(I) * MGpcnt(I) / AC(LI).libNTires ' GFH 04/22/03.
            'TLOAD = GL(I) * AC(LI).libMGpcnt / AC(LI).libNTires
            TPRESS = AC(LI).libCP '* 1.01

            'ikawa March 11, 2008 constant tire area
            Dim std_area As Single

            If gConstTirePressure Then
                TPRESS = AC(LI).libCP
            Else
                std_area = AC(LI).libGL * AC(LI).libMGpcnt / AC(LI).libNTires / AC(LI).libCP
                TPRESS = GL(I) * AC(LI).libMGpcnt / AC(LI).libNTires / std_area
            End If


            TAREA = TLOAD / TPRESS
            TRAD = CSng(System.Math.Sqrt(TAREA / 3.14159))
            For J = 1 To AC(LI).libNTires
                '      Print #LFNo, LPad$(3, Format(J, "0")); LPad$(8, Format(TRAD, FExp$)); LPad$(12, Format(TAREA, "0.000000")); LPad$(12, Format(libCP(LI), "0.000000"));
                If Not NoOutFiles Then
                    Print(LFNo, LPad(3, J.ToString("f0")))
                    S = LPad(14, TRAD.ToString(FExp)) : Call MakeDecimalPeriod(S)
                    Print(LFNo, S)
                    S = LPad(14, TAREA.ToString(FExp)) : Call MakeDecimalPeriod(S)
                    Print(LFNo, S)
                    S = LPad(14, TPRESS.ToString(FExp)) : Call MakeDecimalPeriod(S)
                    Print(LFNo, S)
                    S = LPad(14, (TLOAD.ToString(FExp))) : Call MakeDecimalPeriod(S)
                    Print(LFNo, S)
                    S = LPad(14, AC(LI).libTX(J).ToString(FExp)) : Call MakeDecimalPeriod(S)
                    Print(LFNo, S)
                    S = LPad(14, AC(LI).libTY(J).ToString(FExp)) : Call MakeDecimalPeriod(S)
                    PrintLine(LFNo, S)
                End If

                LEAAircraft(I).TirePress(J) = TPRESS
                LEAAircraft(I).TireX(J) = AC(LI).libTX(J)
                LEAAircraft(I).TireY(J) = AC(LI).libTY(J)
                LEAAircraft(I).GearOrientation = AC(LI).libGearOrientation
            Next J
            NTemp = AC(LI).libNEVPTS
            '    If RCon(1) > 0! Or RCon(2) > 0! Then
            '      If NTemp > 4 Then NTemp = 4
            '    End If

            If Not NoOutFiles Then
                PrintLine(LFNo, " No. Of Evaluation Points (( X, Y Sets )")
                PrintLine(LFNo, LPad(3, NTemp.ToString("f0")))
                PrintLine(LFNo, " Point No.      X-coord.        Y-coord.")
                PrintLine(LFNo, "                 ( In )          ( In )")
                PrintLine(LFNo, " --------------------------------------")
            End If

            For J = 1 To NTemp
                If Not NoOutFiles Then
                    Print(LFNo, LPad(6, J.ToString("f0")))
                    S = LPad(20, AC(LI).libEVPTX(J).ToString(FExp)) : Call MakeDecimalPeriod(S)
                    Print(LFNo, S)
                    S = LPad(16, AC(LI).libEVPTY(J).ToString(FExp)) : Call MakeDecimalPeriod(S)
                    PrintLine(LFNo, S)
                End If

                LEAAircraft(I).EvalX(J) = AC(LI).libEVPTX(J)
                LEAAircraft(I).EvalY(J) = AC(LI).libEVPTY(J)
            Next J
        Next I
        If Not NoOutFiles Then
            FileClose(LFNo)
        End If

        Exit Sub

WJuleaFileError:
        IErr = Err.Number

        Ret = FileErrors(IErr, FileName)

        If Ret = 0 Then Resume
        If Ret = 3 Then
            S = "Error # " & IErr.ToString("f0") & " occurred writing" & NL
            S = S & "file " & FileName & "."
            Ret = MsgBoxDQ(S, 0, "File Error")
        End If
        DesigningStr = False
        If Not NoOutFiles Then
            FileClose(SFNo, LFNo)
        End If

        Exit Sub

    End Sub

    Sub WriteNotesFile(ByRef SaveString As String, ByRef Mode As Short)
        '  Mode:    Change = 0,  Add = 1,  Delete = -1
        Dim SS, S, SEnd As String
        Dim J, I, SLen As Integer
        Dim FileName As String
        Dim NFNo, IErr As Integer
        Dim TempFileName As String
        Dim TFNo As Integer
        On Error GoTo WNotesFileError

        FileName = WorkingDir & JobName & ".NTS"
        NFNo = FreeFile()

        If Dir(FileName) = "" Then
            I = 0
        Else
            FileOpen(NFNo, FileName, OpenMode.Input, , , 1024)
            I = CInt(LOF(NFNo)) ' Check for zero length.
            FileClose(NFNo)
        End If

        If I = 0 Then
            NFNo = FreeFile()
            FileOpen(NFNo, FileName, OpenMode.Output, , , 1024)
            For I = 1 To NSects
                S = jobSectName(I)
                PrintLine(NFNo, S)
                PrintLine(NFNo, Len(S).ToString("f0"))
                PrintLine(NFNo, S)
            Next I
            FileClose(NFNo)
            Exit Sub
        End If
        'Stop
        TempFileName = WorkingDir & "TEMP" & ".NTS"
        FileCopy(FileName, TempFileName)
        NFNo = FreeFile()
        FileOpen(NFNo, FileName, OpenMode.Output, , , 1024)
        TFNo = FreeFile()
        FileOpen(TFNo, TempFileName, OpenMode.Input, , , 1024)

        For I = 1 To NSects
            If I = ISect Then
                If Mode = 0 Then ' Change string.
                    S = LineInput(TFNo)
                    Input(TFNo, SLen)
                    SS = InputString(TFNo, SLen)
                    Input(TFNo, SEnd)
                    SLen = Len(SaveString)
                    SS = SaveString
                ElseIf Mode = -1 Then  ' Delete complete record.
                    If SaveString = "NotOldLastSect" Then
                        S = LineInput(TFNo) ' See cmdDeleteSect_Click.
                        Input(TFNo, SLen)
                        SS = InputString(TFNo, SLen)
                        Input(TFNo, SEnd)
                    End If
                    S = LineInput(TFNo)
                    Input(TFNo, SLen)
                    SS = InputString(TFNo, SLen)
                    Input(TFNo, SEnd)
                Else                        ' Add complete record.
                    S = jobSectName(I)
                    SLen = Len(SaveString)
                    SS = SaveString
                End If
            Else

                If EOF(TFNo) Then
                    S = jobSectName$(I)  ' GFH 3/27/03 Error correction added.
                    SS = S & vbCrLf & "Tried To read beyond the End Of the file." & vbCrLf & "Data may have been lost."
                    SLen = Len(SS)
                Else
                    S = LineInput(TFNo)
                    Input(TFNo, SLen)
                    'SS$ = Input(SLen, TFNo)
                    Input(TFNo, SS)
                    SS = SS.Substring(0, SLen) 'ikawa 
                    'Input(TFNo, SEnd)
                    'SEnd = SS 'ikawa.att
                End If
            End If

            If S <> jobSectName(I) Then

                S = "File " & JobName & ".NTS Is corrupted." & NL2
                S = S & "The corrupted file has been stored In NOTESBAD.NTS," & NL
                S = S & "And As much Of the corrupted file As possible" & NL
                S = S & "retained In " & JobName & ".NTS." & NL
                S = S & "Try To fix the file In a text editor immediately."

                Ret = MsgBoxDQ(S, 0, "Notes File Error")

                For J = I To NSects
                    S = jobSectName(J)
                    PrintLine(NFNo, S)
                    PrintLine(NFNo, Len(S).ToString())
                    PrintLine(NFNo, S)
                Next J
                FileClose(NFNo, TFNo)
                FileCopy(TempFileName, WorkingDir & "NOTESBAD.NTS")
                Exit Sub
            End If

            PrintLine(NFNo, jobSectName(I))
            PrintLine(NFNo, SLen.ToString())
            PrintLine(NFNo, SS)

        Next I

        FileClose(NFNo, TFNo)
        Exit Sub

WNotesFileError:

        IErr = Err.Number
        Ret = FileErrors(IErr, FileName & " Or " & TempFileName)

        If Ret = 0 Then Resume

        If Ret = 3 Then
            S = "Error # " & IErr.ToString() & " occurred writing" & NL
            S = S & FileName & " Or reading " & TempFileName & "."
            Ret = MsgBoxDQ(S, 0, "File Error")
        End If
        FileClose(NFNo, TFNo)
        Exit Sub

    End Sub

    Sub LEDFAA_to_LEAF(ByRef DesignType As Short)
        ' First version by Izydor Kawa.
        Dim I As Integer

        ReDim CallAC(NAC)

        For I = 1 To NAC
            CallAC(I).ACname = ACName(I)
            CallAC(I).GearLoad = LEAAircraft(I).GearLoad
            CallAC(I).NTires = LEAAircraft(I).NTires
            'CallAC(I).libGear = LEAAircraft(I).Gear
            ReDim CallAC(I).TirePress(CallAC(I).NTires)
            ReDim CallAC(I).TireX(CallAC(I).NTires)
            ReDim CallAC(I).TireY(CallAC(I).NTires)


            CallAC(I).NEvalPoints = LEAAircraft(I).NEvalPoints
            ReDim CallAC(I).EvalX(CallAC(I).NEvalPoints)
            ReDim CallAC(I).EvalY(CallAC(I).NEvalPoints)

            For I2 = 1 To CallAC(I).NTires
                CallAC(I).TirePress(I2) = LEAAircraft(I).TirePress(I2)
                CallAC(I).TireX(I2) = LEAAircraft(I).TireX(I2)
                CallAC(I).TireY(I2) = LEAAircraft(I).TireY(I2)
            Next I2

            For I2 = 1 To CallAC(I).NEvalPoints
                CallAC(I).EvalX(I2) = LEAAircraft(I).EvalX(I2)
                CallAC(I).EvalY(I2) = LEAAircraft(I).EvalY(I2)
            Next I2
        Next I

        LEAStrActiveX.NLayers = LEAStructure.NLayers

        ReDim LEAStrActiveX.Thick(LEAStrActiveX.NLayers)
        ReDim LEAStrActiveX.Modulus(LEAStrActiveX.NLayers)
        ReDim LEAStrActiveX.Poisson(LEAStrActiveX.NLayers)
        ReDim LEAStrActiveX.InterfaceParm(LEAStrActiveX.NLayers)

        For I = 1 To LEAStrActiveX.NLayers
            LEAStrActiveX.Thick(I) = LEAStructure.Thick(I)
            LEAStrActiveX.Modulus(I) = LEAStructure.Modulus(I)
            LEAStrActiveX.Poisson(I) = LEAStructure.Poisson(I)
            LEAStrActiveX.InterfaceParm(I) = LEAStructure.InterfaceCode(I)
        Next I

        ' GFH 02-20-03
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then

            If LEAStructure.EvalDepth(1) < 0 Then
                LEAStrActiveX.EvalDepth = System.Math.Abs(LEAStructure.EvalDepth(1))
                LEAStrActiveX.EvalLayer = 1
            Else

                LEAStrActiveX.EvalDepth = LEAStructure.EvalDepth(1)
                If CBool(AlternateSG) Then
                    LEAStrActiveX.EvalLayer = LEAStructure.NLayers ' If EvalLayer is just above the subgrade.
                    Temp = 0.0!
                    For I = 1 To LEAStrActiveX.NLayers - 1
                        'Temp = Temp + julThick(I)
                        Temp = Temp + CSng(LEAStrActiveX.Thick(I) * 1.001) 'GFH 09-04-07
                        If Temp >= LEAStrActiveX.EvalDepth Then
                            LEAStrActiveX.EvalLayer = I + 1
                            Exit For
                        End If
                    Next I
                Else
                    LEAStrActiveX.EvalLayer = LEAStructure.NLayers
                End If
            End If
        ElseIf DesignType = NewRigid Or DesignType = PCCOnFlex Then
            LEAStrActiveX.EvalDepth = System.Math.Abs(LEAStructure.EvalDepth(1))
            LEAStrActiveX.EvalLayer = 1
        End If

        I = NEvalDepths
        '  If LEAStructure.NEvalDepths > 1 Then
        '  MsgBox "ala", vbOKOnly, "NEvalDepths>1", "ala11"
        '  End If

        For J = 1 To LEAStrActiveX.NLayers
            If LEAStrActiveX.InterfaceParm(J) = 0 Then
                LEAStrActiveX.InterfaceParm(J) = 1
            ElseIf LEAStrActiveX.InterfaceParm(J) = 100000 Then
                LEAStrActiveX.InterfaceParm(J) = 0
            ElseIf LEAStrActiveX.InterfaceParm(J) = 700 Then
                LEAStrActiveX.InterfaceParm(J) = 0.999597129
                'LEAStrActiveX.InterfaceParm(J) = 0.7
            End If
        Next J

    End Sub

    Sub LEDFAA_to_LEAF_Rigid(ByRef DesignType As Short, ByRef NextraAC As Integer)

        ' GFH 02/23/03. For rigid structure calls to LEAF. Treats multiple gear
        ' aircraft as two separate aircraft, wing and body gears. Mod of LEDFAA_LEAF.

        Dim I As Integer
        Dim IA, IextraAC As Integer
        Dim NACpIex As Integer


        NextraAC = 0
        For IA = 1 To NAC
            LI = LibIndex(IA)
            If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
                NextraAC = NextraAC + 1S
            End If

            If AC(LI).libGear = "X" And AC(LI).libNGroups = 2 Then
                NextraAC = NextraAC + 1S
            End If

        Next IA

        ReDim CallAC(NAC + NextraAC)





        IextraAC = 0
        For I = 1 To NAC
            LI = LibIndex(I)
            CallAC(I).libGear = AC(LI).libGear 'ikawa 08/20/03
            CallAC(I).LibGearOrientation = AC(LI).libGearOrientation


            If AC(LI).libGear = "X" Then

                If AC(LI).libNGroups = 2 Then
                    IextraAC = IextraAC + 1 ' Add the body gear to the aircraft list.
                    NACpIex = NAC + IextraAC
                End If

                'CallAC(I).ACname = ACName(I) & " Wing" ' Leave wing gear in the same place.


                CallAC(I).ACname = ACName(I)
                CallAC(I).GearLoad = LEAAircraft(I).GearLoad
                CallAC(I).NTires = LEAAircraft(I).NTires
                CallAC(I).LibGearOrientation = LEAAircraft(I).GearOrientation
                If AC(LI).libNGroups = 2 Then
                    CallAC(NACpIex).ACname = ACName(I) & " Body" ' Add the body gear to the aircraft list.
                    CallAC(NACpIex).GearLoad = LEAAircraft(I).GearLoad * AC(LI).libNTires2 / AC(LI).libNTires
                    CallAC(NACpIex).NTires = AC(LI).libNTires2
                End If

                '========================================================================

                ReDim CallAC(I).TirePress(CallAC(I).NTires)
                ReDim CallAC(I).TireX(CallAC(I).NTires)
                ReDim CallAC(I).TireY(CallAC(I).NTires)
                If AC(LI).libNGroups = 2 Then
                    ReDim CallAC(NACpIex).TirePress(CallAC(NACpIex).NTires)
                    ReDim CallAC(NACpIex).TireX(CallAC(NACpIex).NTires)
                    ReDim CallAC(NACpIex).TireY(CallAC(NACpIex).NTires)
                End If

                CallAC(I).NEvalPoints = AC(LI).libNEVPTS1
                ReDim CallAC(I).EvalX(CallAC(I).NEvalPoints)
                ReDim CallAC(I).EvalY(CallAC(I).NEvalPoints)

                If AC(LI).libNGroups = 2 Then
                    CallAC(NACpIex).NEvalPoints = AC(LI).libNEVPTS2
                    ReDim CallAC(NACpIex).EvalX(CallAC(NACpIex).NEvalPoints)
                    ReDim CallAC(NACpIex).EvalY(CallAC(NACpIex).NEvalPoints)

                    CallAC(NACpIex).libGear = "X"
                End If

                For I2 = 1 To CallAC(I).NTires
                    CallAC(I).TirePress(I2) = LEAAircraft(I).TirePress(I2)
                    CallAC(I).TireX(I2) = LEAAircraft(I).TireX(I2)
                    CallAC(I).TireY(I2) = LEAAircraft(I).TireY(I2)
                Next I2

                J = CallAC(I).NTires * 2

                If AC(LI).libNGroups = 2 Then
                    For I2 = 1 To CallAC(NACpIex).NTires
                        CallAC(NACpIex).TirePress(I2) = LEAAircraft(I).TirePress(I2 + J)
                        CallAC(NACpIex).TireX(I2) = LEAAircraft(I).TireX(I2 + J)
                        CallAC(NACpIex).TireY(I2) = LEAAircraft(I).TireY(I2 + J)
                    Next I2
                End If



                For I2 = 1 To CallAC(I).NEvalPoints
                    CallAC(I).EvalX(I2) = LEAAircraft(I).EvalX(I2)
                    CallAC(I).EvalY(I2) = LEAAircraft(I).EvalY(I2)
                Next I2
                J = CallAC(I).NEvalPoints
                If AC(LI).libNGroups = 2 Then
                    For I2 = 1 To CallAC(NACpIex).NEvalPoints
                        CallAC(NACpIex).EvalX(I2) = LEAAircraft(I).EvalX(I2 + J)
                        CallAC(NACpIex).EvalY(I2) = LEAAircraft(I).EvalY(I2 + J)
                    Next I2
                End If

                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            ElseIf AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.


                'If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.

                IextraAC = IextraAC + 1 ' Add the body gear to the aircraft list.
                NACpIex = NAC + IextraAC
                CallAC(I).ACname = ACName(I) & " Wing" ' Leave wing gear in the same place.
                CallAC(NACpIex).ACname = ACName(I) & " Body" ' Add the body gear to the aircraft list.
                If AC(LI).libGear = "WFBF" Then
                    CallAC(I).GearLoad = LEAAircraft(I).GearLoad * 4 / 16
                    CallAC(I).NTires = 4
                    CallAC(NACpIex).GearLoad = LEAAircraft(I).GearLoad * 4 / 16
                    CallAC(NACpIex).NTires = 4
                    CallAC(NACpIex).libGear = "F" 'ikawa 2012.12.14 (for B747)
                ElseIf AC(LI).libGear = "WFBN" Then
                    CallAC(I).GearLoad = LEAAircraft(I).GearLoad * 4 / 20
                    CallAC(I).NTires = 4
                    CallAC(NACpIex).GearLoad = LEAAircraft(I).GearLoad * 6 / 20
                    CallAC(NACpIex).NTires = 6
                    CallAC(NACpIex).libGear = "N" 'ikawa 2012.12.14 (for A380)
                End If
                ReDim CallAC(I).TirePress(CallAC(I).NTires)
                ReDim CallAC(I).TireX(CallAC(I).NTires)
                ReDim CallAC(I).TireY(CallAC(I).NTires)
                ReDim CallAC(NACpIex).TirePress(CallAC(NACpIex).NTires)
                ReDim CallAC(NACpIex).TireX(CallAC(NACpIex).NTires)
                ReDim CallAC(NACpIex).TireY(CallAC(NACpIex).NTires)

                CallAC(I).NEvalPoints = 8
                ReDim CallAC(I).EvalX(CallAC(I).NEvalPoints)
                ReDim CallAC(I).EvalY(CallAC(I).NEvalPoints)
                CallAC(NACpIex).NEvalPoints = 8
                ReDim CallAC(NACpIex).EvalX(CallAC(NACpIex).NEvalPoints)
                ReDim CallAC(NACpIex).EvalY(CallAC(NACpIex).NEvalPoints)

                For I2 = 1 To CallAC(I).NTires
                    CallAC(I).TirePress(I2) = LEAAircraft(I).TirePress(I2)
                    CallAC(I).TireX(I2) = LEAAircraft(I).TireX(I2)
                    CallAC(I).TireY(I2) = LEAAircraft(I).TireY(I2)
                Next I2
                J = CallAC(I).NTires
                For I2 = 1 To CallAC(NACpIex).NTires
                    CallAC(NACpIex).TirePress(I2) = LEAAircraft(I).TirePress(I2 + J)
                    CallAC(NACpIex).TireX(I2) = LEAAircraft(I).TireX(I2 + J)
                    CallAC(NACpIex).TireY(I2) = LEAAircraft(I).TireY(I2 + J)
                Next I2


                For I2 = 1 To CallAC(I).NEvalPoints
                    CallAC(I).EvalX(I2) = LEAAircraft(I).EvalX(I2)
                    CallAC(I).EvalY(I2) = LEAAircraft(I).EvalY(I2)
                Next I2
                J = CallAC(I).NEvalPoints
                For I2 = 1 To CallAC(NACpIex).NEvalPoints
                    CallAC(NACpIex).EvalX(I2) = LEAAircraft(I).EvalX(I2 + J)
                    CallAC(NACpIex).EvalY(I2) = LEAAircraft(I).EvalY(I2 + J)
                Next I2

            Else

                CallAC(I).ACname = ACName(I)
                CallAC(I).GearLoad = LEAAircraft(I).GearLoad
                CallAC(I).NTires = LEAAircraft(I).NTires
                'CallAC(I).libGear = 

                ReDim CallAC(I).TirePress(CallAC(I).NTires)
                ReDim CallAC(I).TireX(CallAC(I).NTires)
                ReDim CallAC(I).TireY(CallAC(I).NTires)

                CallAC(I).NEvalPoints = LEAAircraft(I).NEvalPoints
                ReDim CallAC(I).EvalX(CallAC(I).NEvalPoints)
                ReDim CallAC(I).EvalY(CallAC(I).NEvalPoints)


                For I2 = 1 To CallAC(I).NTires
                    CallAC(I).TirePress(I2) = LEAAircraft(I).TirePress(I2)
                    CallAC(I).TireX(I2) = LEAAircraft(I).TireX(I2)
                    CallAC(I).TireY(I2) = LEAAircraft(I).TireY(I2)
                Next I2

                For I2 = 1 To CallAC(I).NEvalPoints
                    CallAC(I).EvalX(I2) = LEAAircraft(I).EvalX(I2)
                    CallAC(I).EvalY(I2) = LEAAircraft(I).EvalY(I2)
                Next I2

            End If

        Next I




        'Pavement Structure for Rigid
        'LEAStrActiveX.NLayers = LEAStructure.NLayers
        'ReDim LEAStrActiveX.Thick(LEAStrActiveX.NLayers)
        'ReDim LEAStrActiveX.Modulus(LEAStrActiveX.NLayers)
        'ReDim LEAStrActiveX.Poisson(LEAStrActiveX.NLayers)
        'ReDim LEAStrActiveX.InterfaceParm(LEAStrActiveX.NLayers)

        'For I = 1 To LEAStrActiveX.NLayers
        '    LEAStrActiveX.Thick(I) = LEAStructure.Thick(I)
        '    LEAStrActiveX.Modulus(I) = LEAStructure.Modulus(I)
        '    LEAStrActiveX.Poisson(I) = LEAStructure.Poisson(I)
        '    LEAStrActiveX.InterfaceParm(I) = LEAStructure.InterfaceCode(I)
        'Next I



        '===============================================================
        'LEAStrActiveX.NLayers = NPLayers
        LEAStrActiveX.NLayers = LEAStructure.NLayers
        ReDim LEAStrActiveX.Thick(LEAStructure.NLayers)
        ReDim LEAStrActiveX.Modulus(LEAStructure.NLayers)
        ReDim LEAStrActiveX.Poisson(LEAStructure.NLayers)
        ReDim LEAStrActiveX.InterfaceParm(LEAStructure.NLayers)

        For I = 1 To LEAStrActiveX.NLayers  ' NPLayers
            'reassumed to unify strutural info for Leaf.STR and INGRID.ing by YGC 112213
            'LEAStrActiveX.Thick(I) = Thick(I)
            'LEAStrActiveX.Modulus(I) = Modulus(I)
            'LEAStrActiveX.Poisson(I) = PoissonsRatio(I)
            LEAStrActiveX.Thick(I) = LEAStructure.Thick(I)
            LEAStrActiveX.Modulus(I) = LEAStructure.Modulus(I)
            LEAStrActiveX.Poisson(I) = LEAStructure.Poisson(I)
            'reassumed to unify strutural info for Leaf.STR and INGRID.ing by YGC 112213 END

            LEAStrActiveX.InterfaceParm(I) = LEAStructure.InterfaceCode(I) '(?)
        Next I
        '===============================================================





        ' GFH 02-23-03
        If DesignType = NewRigid Or DesignType = PCCOnFlex Then
            LEAStrActiveX.EvalDepth = System.Math.Abs(LEAStructure.EvalDepth(1))
            LEAStrActiveX.EvalLayer = 1
        End If

        I = NEvalDepths
        '  If LEAStructure.NEvalDepths > 1 Then
        '  MsgBox "ala", vbOKOnly, "NEvalDepths>1", "ala11"
        '  End If

        For J = 1 To NPLayers ' LEAStrActiveX.NLayers
            If LEAStrActiveX.InterfaceParm(J) = 0 Then
                LEAStrActiveX.InterfaceParm(J) = 1
            ElseIf LEAStrActiveX.InterfaceParm(J) >= 100000 Then
                LEAStrActiveX.InterfaceParm(J) = 0
                '    ElseIf LEAStrActiveX.InterfaceParm(J) = 700 Then
                '      LEAStrActiveX.InterfaceParm(J) = 0.999597129
            Else ' GFH 04/09/03.
                LEAStrActiveX.InterfaceParm(J) = 10 ^ (-LEAStrActiveX.InterfaceParm(J) / LEAStrActiveX.Modulus(2))
                '      LEAStrActiveX.InterfaceParm(J) = 0.999
                '      If J = 1 Then Debug.Print "InterfaceParm(J) = "; LEAStrActiveX.InterfaceParm(J)
            End If
        Next J


        ReDim LEAStrActiveX.lngDummy(1)
        LEAStrActiveX.lngDummy(1) = g2Dgear

    End Sub

    Sub ReadJobFileWriteXML()

        Dim I, J, JFNo As Integer
        Dim FileName, FileFormat As String

        Try
            FileName = WorkingDir & JobName & ".JOB"
            JFNo = FreeFile()
            FileOpen(JFNo, FileName, OpenMode.Input, , , 1024)

            If EOF(JFNo) Then
                S = "File " & WorkingDir & JobName & ".JOB Is empty." & NL2
                S = S & "Delete the job And create a New one." & NL2
                S = S & "No action will be taken now."
                FileClose(JFNo)
                Exit Sub
            End If

            FileFormat = LineInput(JFNo)

            I = 0
            Do
                If I = MaxSects Then
                    SS = MaxSects.ToString("f0")
                    S = "The maximum number Of sections allowed In a" & NL
                    S = S & " job file Is " & SS & "."
                    S = S & " There are more sections than" & NL
                    S = S & " this In " & JobName & "." & NL2
                    S = S & " Only the first " & SS & " sections In "
                    S = S & JobName & " will be loaded."
                    Ret = MsgBoxDQ(S, 0, "Too Many Sections")
                    Exit Do
                End If
                If EOF(JFNo) Then Exit Do

                I = I + 1
                jobSectName(I) = LineInput(JFNo)
                If FileFormat <> JobFileFormat Then
                    Input(JFNo, jobDesigned(I))
                    Input(JFNo, jobCDFAsp(I))
                    If jobDesigned(I) = Nothing Then jobDesigned(I) = NullDate
                Else
                    Dim info As New System.Globalization.DateTimeFormatInfo()
                    jobDesigned(I) = Date.Parse(InputString(JFNo, 20), info)
                    'jobDesigned(I) = CDate(InputString(JFNo, 20))

                    'Sxml(I) = CDate(InputString(JFNo, 20))

                    Dim s1temp As String
                    s1temp = InputString(JFNo, 20)

                    If IsDate(s1temp) Then
                        Sxml(I) = CDate(s1temp)
                    Else
                        Sxml(I) = NullDate
                    End If

                    Input(JFNo, jobCDFAsp(I))
                End If
                Input(JFNo, jobLife(I))
                Input(JFNo, jobSCIB(I))
                Input(JFNo, jobLifeExistPCC(I))
                Input(JFNo, jobNPLayers(I))
                Input(JFNo, jobNAC(I))
                If jobSCIB(I) < 100.0! Then jobLifeExistPCC(I) = 100.0! ' *****

                For J = 1 To jobNPLayers(I)
                    Input(JFNo, jobThick(I, J))
                    Input(JFNo, jobModulus(I, J))
                    Input(JFNo, jobLCode(I, J))
                    S = LayerType(jobLCode(I, J))

                    If S = NPCC Or S = NPCCOU Or S = NPCCOB Or S = NPCCOF Then
                        jobRCon(I, J) = jobModulus(I, J)
                        jobModulus(I, J) = DefaultModulus(jobLCode(I, J))
                    Else
                        jobRCon(I, J) = 0.0!
                    End If

                Next J
                jobThick(I, jobNPLayers(I)) = 0.0!

                SS = MaxSectAC.ToString()

                If jobNAC(I) > MaxSectAC Then

                    S = "The maximum number Of aircraft allowed In a" & NL
                    S = S & " design list Is " & SS & "."
                    S = S & " There are more aircraft than" & NL
                    S = S & " this In " & JobName & "." & NL2
                    S = S & " Only the first " & SS & " aircraft In "
                    S = S & JobName & " will be loaded."
                    Ret = MsgBoxDQ(S, 0, "Too Many Aircraft")
                    'IErr = jobNAC(I)
                    jobNAC(I) = MaxSectAC
                End If


                For J = 1 To jobNAC(I)
                    If FileFormat = JobFileFormat Then
                        jobACName(I, J) = Trim(InputString(JFNo, 21))
                        'If jobACName$(I, J) = "B-767" Then jobACName$(I, J) = "B-767-200"
                        'If jobACName$(I, J) = "DC-9" Then jobACName$(I, J) = "DC-9-30"
                        'If jobACName$(I, J) = "MD-80" Then jobACName$(I, J) = "MD-82/88"
                        'If jobACName$(I, J) = "B-777" Then jobACName$(I, J) = "B-777-200 B"
                        'If jobACName$(I, J) = "DC-10-30 (Wing)" Then jobACName$(I, J) = "DC-10-30"
                        'If jobACName$(I, J) = "C-17" Then jobACName$(I, J) = "C-17A"

                        If jobACName$(I, J) = "A-340" Then jobACName$(I, J) = "A340-200/300"
                        If jobACName$(I, J) = "A-340 Belly" Then jobACName$(I, J) = "A340-200/300" & BellyExt$
                        If Left(jobACName$(I, J), 2) = "A-" Then
                            jobACName$(I, J) = "A" & Right(jobACName$(I, J), Len(jobACName$(I, J)) - 2)
                        End If
                        If jobACName(I, J) = "DT-777 C" Then jobACName(I, J) = "DT-777"
                        '       3 lines added 06-18-04, GFH.
                        If jobACName(I, J) = "B-777-200 A" Then jobACName(I, J) = "B-777-200"
                        If jobACName(I, J) = "B-777-200 B" Then jobACName(I, J) = "B-777-200ER"
                        If jobACName(I, J) = "B-777-200 C" Then jobACName(I, J) = "B-777-300"
                        If jobACName(I, J) = "DC-9-30" Then jobACName(I, J) = "DC-9-32" 'ikawa
                        If jobACName(I, J) = "DC-9-50" Then jobACName(I, J) = "DC-9-51" 'ikawa
                    Else
                        jobACName(I, J) = Trim(InputString(JFNo, 12))
                    End If
                    Input(JFNo, jobGL(I, J))
                    Input(JFNo, jobRepsAnnual(I, J))
                    Input(JFNo, jobRepsInc(I, J))
                Next J


                'For J = MaxSectAC + 1 To CShort(IErr)
                '    S = LineInput(JFNo)
                'Next J

                If FileFormat = JobFileFormat Then
                    S = LineInput(JFNo)
                    S = LineInput(JFNo)
                End If
            Loop
            NSects = CShort(I)
            If NSects = 0 Then ISect = 0 Else ISect = 1
            FileClose(JFNo)


            Call WriteJobFileXML()
            Exit Sub

        Catch ex As Exception

            FileClose(JFNo)
            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

            If NSects = 0 Then ISect = 0 Else ISect = 1
        End Try

    End Sub

    Sub WriteJobFileXML()


        'CompactionWasRun = False 'WriteJobFileXML
        'frmStructure.DefInstance.cmdLife.Text = "&Life"
        'frmStructure.DefInstance.cmdLife.Width = 64
        'frmStructure.DefInstance.cmdLife.Location = New Point(162, 400)
        'frmStructure.DefInstance.cmdModifyStr.Location = New Point(240, 400)
        'frmStructure.DefInstance.cmdAddDelete.Location = New Point(367, 400)
        'frmStructure.DefInstance.cmdSaveStr.Location = New Point(502, 400)
        'lastSectName = ""
        'lastJobName = ""


        Dim S As String, I, J As Integer
        Dim DDesigned As Date, cinfo As String


        cinfo = CultureInfo.CurrentCulture.Name



        Try

            Dim xmlFile As String
            xmlFile = WorkingDir + JobName + ".JOB.xml"
            Dim xw As New Xml.XmlTextWriter(xmlFile, System.Text.Encoding.UTF8)
            xw.WriteStartDocument()
            xw.WriteStartElement("JobInfo")

            For I = 1 To NSects

                xw.WriteStartElement("SectionInfo")
                xw.WriteElementString("SectionName", jobSectName(I))

                If CultureInfo.CurrentCulture.Name = "en-US" Then
                    xw.WriteElementString("jobDesigned", jobDesigned(I).ToString("mm/dd/yy hh:mm:ss"))
                Else
                    xw.WriteElementString("jobDesigned", CStr(jobDesigned(I)))
                End If
                DDesigned = jobDesigned(I)
                'xw.WriteElementString("S", Format(Sxml(I), "mm/dd/yy").ToString)


                If jobSMin(I) = "Min" Then '2012.12.31
                    xw.WriteElementString("S", jobSMin(I))
                Else

                    If CultureInfo.CurrentCulture.Name = "en-US" Then
                        xw.WriteElementString("S", Format(DDesigned, "mm/dd/yy").ToString)
                    Else
                        xw.WriteElementString("S", DDesigned.ToShortDateString)
                    End If

                End If

                'If CultureInfo.CurrentCulture.Name = "en-US" Then
                '    xw.WriteElementString("S", Format(DDesigned, "mm/dd/yy").ToString)
                'Else
                '    xw.WriteElementString("S", DDesigned.ToShortDateString)
                'End If



                xw.WriteElementString("jobCDFAsp", jobCDFAsp(I).ToString("f6").ToString)

                xw.WriteElementString("jobLife", jobLife(I).ToString("f0"))
                xw.WriteElementString("jobSCIB", jobSCIB(I).ToString("f2"))
                xw.WriteElementString("jobLifeExistPCC", jobLifeExistPCC(I).ToString("f2"))
                xw.WriteElementString("jobNPLayers", LPad(4, jobNPLayers(I).ToString("f0")))
                xw.WriteElementString("jobNAC", LPad(4, jobNAC(I).ToString("f0")))

                jobThick(I, jobNPLayers(I)) = 0.0!

                If DesignType = NewFlex Or DesignType = FlexOnFlex Then
                    xw.WriteStartElement("RDEC_Model_Info")
                    xw.WriteElementString("jobFlexuralMod", Format(gFlexuralMod(I), "0").ToString)
                    xw.WriteElementString("jobAirVoids", Format(gAirVoids(I), "0.0").ToString)
                    xw.WriteElementString("jobAsphaltContentByVol", Format(gAsphaltContentByVol(I), "0.0").ToString)
                    xw.WriteElementString("jobPNMS ", Format(gPNMS(I), "0").ToString)
                    xw.WriteElementString("jobPPCS ", Format(gPPCS(I), "0").ToString)
                    xw.WriteElementString("jobP200 ", Format(gP200(I), "0.0").ToString)
                    xw.WriteEndElement() 'RDEC_Model_Info
                End If


                For J = 1 To jobNPLayers(I)

                    xw.WriteStartElement("LayerInfo")
                    xw.WriteElementString("jobThick", jobThick(I, J).ToString("f4"))

                    If jobRCon(I, J) > 0.0! Then
                        Temp = jobRCon(I, J)
                    Else
                        Temp = jobModulus(I, J)
                    End If

                    xw.WriteElementString("jobModulus", Temp.ToString("f2").ToString)
                    xw.WriteElementString("jobPoissonsRatio", jobPoissonsRatio(I, J).ToString("f2").ToString)
                    xw.WriteElementString("jobLCode", jobLCode(I, J).ToString("f2").ToString)
                    xw.WriteEndElement() 'Layer Info

                Next J

                For J = 1 To jobNAC(I)

                    xw.WriteStartElement("AircraftInfo")
                    xw.WriteElementString("jobACName", jobACName(I, J))
                    xw.WriteElementString("jobGL", jobGL(I, J).ToString("f0"))
                    xw.WriteElementString("jobRepsAnnual", jobRepsAnnual(I, J).ToString("f0"))
                    xw.WriteElementString("jobRepsInc", jobRepsInc(I, J).ToString("f4"))
                    xw.WriteEndElement() 'Aircraft Info
                Next J

                xw.WriteEndElement() 'Section Info
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

    Sub ReadJobFileXML()


        Dim I, J, K As Integer
        Dim FileNameXML As String
        Dim ConvertToDot As Boolean

        If Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator.ToString = "." Then
            ConvertToDot = True
        Else
            ConvertToDot = False
        End If


        Try

            FileNameXML = WorkingDir & JobName & ".JOB.xml"
            Dim reader As XmlTextReader = New XmlTextReader(FileNameXML)
            Dim Element As String
            Element = ""

            I = 0
            Do While (reader.Read())
                Select Case reader.NodeType
                    Case XmlNodeType.Element 'Display beginning of element. (like GrossWt)
                        'Debug.Write("<" + reader.Name)
                        Element = reader.Name

                        If reader.HasAttributes Then 'If attributes exist
                            While reader.MoveToNextAttribute()
                                'Display attribute name and value.
                                'Debug.Write(" {0}='{1}' " & reader.Name & "   " & reader.Value)
                            End While
                        End If

                        'Debug.WriteLine(">")
                    Case XmlNodeType.Text 'Display the text in each element. (like 25,000)
                        If Element = "SectionName" Then
                            I += 1 : J = 0 : K = 0
                            jobSectName(I) = reader.Value

                        ElseIf Element = "jobDesigned" Then

                            If IsDate(reader.Value) Then
                                jobDesigned(I) = CDate(reader.Value)
                            Else
                                jobDesigned(I) = NullDate
                            End If

                        ElseIf Element = "S" Then

                            S = reader.Value
                            jobSMin(I) = reader.Value '2012.12.31


                        ElseIf Element = "jobCDFAsp" Then

                            If ConvertToDot Then
                                jobCDFAsp(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobCDFAsp(I) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobCDFAsp(I) = CSng(reader.Value)

                        ElseIf Element = "jobLife" Then

                            If ConvertToDot Then
                                jobLife(I) = CShort(reader.Value.Replace(",", "."))
                            Else
                                jobLife(I) = CShort(reader.Value.Replace(".", ","))
                            End If
                            'jobLife(I) = CShort(reader.Value)

                        ElseIf Element = "jobSCIB" Then

                            If ConvertToDot Then
                                jobSCIB(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobSCIB(I) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobSCIB(I) = CSng(reader.Value)

                        ElseIf Element = "jobLifeExistPCC" Then


                            If ConvertToDot Then
                                jobLifeExistPCC(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobLifeExistPCC(I) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobLifeExistPCC(I) = CSng(reader.Value)

                            If jobSCIB(I) < 100.0! Then
                                jobLifeExistPCC(I) = 100.0!
                            End If


                        ElseIf Element = "jobNPLayers" Then

                            If ConvertToDot Then
                                jobNPLayers(I) = CShort(reader.Value.Replace(",", "."))
                            Else
                                jobNPLayers(I) = CShort(reader.Value.Replace(".", ","))
                            End If
                            'jobNPLayers(I) = CShort(reader.Value)

                        ElseIf Element = "jobNAC" Then

                            If ConvertToDot Then
                                jobNAC(I) = CShort(reader.Value.Replace(",", "."))
                            Else
                                jobNAC(I) = CShort(reader.Value.Replace(".", ","))
                            End If
                            'jobNAC(I) = CShort(reader.Value)

                        ElseIf Element = "jobThick" Then

                            J += 1

                            If ConvertToDot Then
                                jobThick(I, J) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobThick(I, J) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobThick(I, J) = CSng(reader.Value)

                        ElseIf Element = "jobModulus" Then

                            If ConvertToDot Then
                                jobModulus(I, J) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobModulus(I, J) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobModulus(I, J) = CSng(reader.Value)

                        ElseIf Element = "jobPoissonsRatio" Then

                            If ConvertToDot Then
                                jobPoissonsRatio(I, J) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobPoissonsRatio(I, J) = CSng(reader.Value.Replace(".", ","))
                            End If

                        ElseIf Element = "jobLCode" Then

                            If ConvertToDot Then
                                jobLCode(I, J) = CShort(reader.Value.Replace(",", "."))
                            Else
                                jobLCode(I, J) = CShort(reader.Value.Replace(".", ","))
                            End If
                            'jobLCode(I, J) = CShort(reader.Value)

                            Dim S8 As String
                            S8 = LayerType(jobLCode(I, J))
                            If S8 = NPCC Or S8 = NPCCOU Or S8 = NPCCOB Or S8 = NPCCOF Then
                                jobRCon(I, J) = jobModulus(I, J)
                                jobModulus(I, J) = DefaultModulus(jobLCode(I, J))
                            Else
                                jobRCon(I, J) = 0.0!
                            End If

                            'jobThick(I, jobNPLayers(I)) = 0.0! 'Subgrade

                        ElseIf Element = "jobACName" Then
                            K += 1
                            jobACName(I, K) = reader.Value

                            'ikawa 12/03/2012
                            Call CheckAirplaneName(jobACName$(I, K))


                        ElseIf Element = "jobGL" Then

                            If ConvertToDot Then
                                jobGL(I, K) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobGL(I, K) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobGL(I, K) = CSng(reader.Value)

                        ElseIf Element = "jobRepsAnnual" Then

                            If ConvertToDot Then
                                jobRepsAnnual(I, K) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobRepsAnnual(I, K) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobRepsAnnual(I, K) = CSng(reader.Value)

                        ElseIf Element = "jobRepsInc" Then

                            If ConvertToDot Then
                                jobRepsInc(I, K) = CSng(reader.Value.Replace(",", "."))
                            Else
                                jobRepsInc(I, K) = CSng(reader.Value.Replace(".", ","))
                            End If
                            'jobRepsInc(I, K) = CSng(reader.Value)

                        ElseIf Element = "jobFlexuralMod" Then

                            If ConvertToDot Then
                                gFlexuralMod(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                gFlexuralMod(I) = CSng(reader.Value.Replace(".", ","))
                            End If

                        ElseIf Element = "jobAirVoids" Then

                            If ConvertToDot Then
                                gAirVoids(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                gAirVoids(I) = CSng(reader.Value.Replace(".", ","))
                            End If

                        ElseIf Element = "jobAsphaltContentByVol" Then

                            If ConvertToDot Then
                                gAsphaltContentByVol(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                gAsphaltContentByVol(I) = CSng(reader.Value.Replace(".", ","))
                            End If

                        ElseIf Element = "jobPNMS" Then

                            If ConvertToDot Then
                                gPNMS(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                gPNMS(I) = CSng(reader.Value.Replace(".", ","))
                            End If


                        ElseIf Element = "jobPPCS" Then

                            If ConvertToDot Then
                                gPPCS(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                gPPCS(I) = CSng(reader.Value.Replace(".", ","))
                            End If

                        ElseIf Element = "jobP200" Then

                            If ConvertToDot Then
                                gP200(I) = CSng(reader.Value.Replace(",", "."))
                            Else
                                gP200(I) = CSng(reader.Value.Replace(".", ","))
                            End If



                        ElseIf Element = "????" Then


                        End If

                        'Debug.WriteLine(reader.Value)

                    Case XmlNodeType.EndElement 'Display end of element.
                        'Debug.Write("</" + reader.Name) : Debug.WriteLine(">")
                End Select

            Loop

            reader.Close()
            NSects = CShort(I)
            If NSects = 0 Then ISect = 0 Else ISect = 1

            If NSects = 0 Then
                ReDim jobSectName(MaxSects)
            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub

    Sub CheckAirplaneName(ByRef jobACName1 As String)
        Exit Sub

        If jobACName1 = "DT-777 C" Then jobACName1 = "DT-777"

        'Generic
        If jobACName1 = "Sngl Whl-30" Then jobACName1 = "S-30"
        If jobACName1 = "Sngl Whl-45" Then jobACName1 = "S-45"
        If jobACName1 = "Sngl Whl-60" Then jobACName1 = "S-60"
        If jobACName1 = "Sngl Whl-75" Then jobACName1 = "S-75"

        If jobACName1 = "Dual Whl-50" Then jobACName1 = "D-50"
        If jobACName1 = "Dual Whl-75" Then jobACName1 = "D-75"
        If jobACName1 = "Dual Whl-100" Then jobACName1 = "D-100"
        If jobACName1 = "Dual Whl-150" Then jobACName1 = "D-150"
        If jobACName1 = "Dual Whl-200" Then jobACName1 = "D-200"

        If jobACName1 = "Dual Tan-100" Then jobACName1 = "2D-100"
        If jobACName1 = "Dual Tan-150" Then jobACName1 = "2D-150"
        If jobACName1 = "Dual Tan-200" Then jobACName1 = "2D-200"
        If jobACName1 = "Dual Tan-300" Then jobACName1 = "2D-300"
        If jobACName1 = "Dual Tan-400" Then jobACName1 = "2D-400"

        'Airbus
        If jobACName1 = "A300-B2" Then jobACName1 = "A300-B2 SB"
        If jobACName1 = "A300-B4" Then jobACName1 = "A300-B4 std"
        If jobACName1 = "A300-600" Then jobACName1 = "A300-600 std"
        If jobACName1 = "A300-600-opt" Then jobACName1 = "A300-600 LB"
        If jobACName1 = "A320" Then jobACName1 = "A320-100"
        If jobACName1 = "A320-opt" Then jobACName1 = "A320 Bogie"
        If jobACName1 = "A330" Then jobACName1 = "A330-200 std"

        If jobACName1 = "A340-200/300" Then jobACName1 = "A340-300 std"
        If jobACName1 = "A340-200/300 Belly" Then jobACName1 = "A340-300 std Belly"
        If jobACName1 = "A340-500/600" Then jobACName1 = "A340-600 std"
        If jobACName1 = "A340-500/600 Belly" Then jobACName1 = "A340-600 std Belly"

        'If jobACName1 = "A-340" Then jobACName1 = "A340-200/300"
        'If jobACName1 = "A-340 Belly" Then jobACName1 = "A340-200/300" & BellyExt$

        If jobACName1 = "A340-500 opt" Then jobACName1 = "A340-500 HGW"
        If jobACName1 = "A340-600 opt" Then jobACName1 = "A340-600 HGW"
        If jobACName1 = "A340-600 opt Belly" Then jobACName1 = "A340-600 HGW Belly"
        If jobACName1 = "A380-800" Then jobACName1 = "A380"
        If jobACName1 = "A380-800F" Then jobACName1 = "A380e"


        'Boeing
        If jobACName1 = "B-707" Then jobACName1 = "B707-320C"
        If jobACName1 = "B-717" Then jobACName1 = "B717-200 HGW"
        If jobACName1 = "B-727" Then jobACName1 = "B727-100C Alternate"

        If jobACName1 = "B-737" Then jobACName1 = "Adv. B737-200"
        If jobACName1 = "B-737-200" Then jobACName1 = "Adv. B737-200"

        If jobACName1 = "B-737-900" Then jobACName1 = "B737-900 ER"

        If jobACName1 = "B-747-200" Then jobACName1 = "B747-200B Combi Mixed"
        If jobACName1 = "B-747" Then jobACName1 = "B747-200B Combi Mixed"

        If jobACName1 = "B-757" Then jobACName1 = "B757-200"

        'If jobACName1 = "B-767" Then jobACName1 = "B-767-200"
        If jobACName1 = "B-767-300ER" Then jobACName1 = "B767-300 ER"
        If jobACName1 = "B-767-400ER" Then jobACName1 = "B767-400 ER"



        'If jobACName1 = "B-777" Then jobACName1 = "B-777-200 B"
        'If jobACName1 = "B-777-200 A" Then jobACName1 = "B-777-200"
        If jobACName1 = "B-777-200 A" Then jobACName1 = "B777-200 Baseline"
        'If jobACName1 = "B-777-200 B" Then jobACName1 = "B-777-200ER"
        If jobACName1 = "B-777-200 B" Then jobACName1 = "B777-200 ER"
        'If jobACName1 = "B-777-200 C" Then jobACName1 = "B-777-300"
        If jobACName1 = "B-777-200 C" Then jobACName1 = "B777-300 Baseline"

        If jobACName1 = "B-777-200" Then jobACName1 = "B777-200 Baseline"
        If jobACName1 = "B-777-200ER" Then jobACName1 = "B777-200 ER"
        If jobACName1 = "B777-200ER" Then jobACName1 = "B777-200 ER"

        If jobACName1 = "B-777-300" Then jobACName1 = "B777-300 Baseline"
        If jobACName1 = "B-777-300ER" Then jobACName1 = "B777-300 ER"


        If jobACName1 = "B747-200B Combi Mixd" Then jobACName1 = "B747-200B Combi Mixed"
        If jobACName1 = "B747-400B Combi" Then jobACName1 = "B747-400"
        If jobACName1 = "B747-400ER Passenger" Then jobACName1 = "B747-400ER"
        If jobACName1 = "B747-400ER Freighter" Then jobACName1 = "B747-400ER"
        If jobACName1 = "B747-8 Intercontinental" Then jobACName1 = "B747-8"
        If jobACName1 = "B747-8 Freigher (Preliminary)" Then jobACName1 = "B747-8F"
        If jobACName1 = "B787-8 (Preliminary)" Then jobACName1 = "B787-8"
        If jobACName1 = "B787-9" Then jobACName1 = "B787-9 (Prelim)"

        'McDonnell Douglas
        'If jobACName1 = "DC-9" Then jobACName1 = "DC-9-30"
        'If jobACName1 = "MD-80" Then jobACName1 = "MD-82/88"
        'If jobACName1 = "DC-10-30 (Wing)" Then jobACName1 = "DC-10-30"

        'If jobACName1 = "DC-9-30" Then jobACName1 = "DC-9-32" 
        If jobACName1 = "DC-9-30" Then jobACName1 = "DC9-32"
        'If jobACName1 = "DC-9-50" Then jobACName1 = "DC-9-51" 
        If jobACName1 = "DC-9-50" Then jobACName1 = "DC9-51"

        If jobACName1 = "DC-8" Then
            jobACName1 = "DC8-63/73"
        End If

        If jobACName1 = "DC-9-32" Then jobACName1 = "DC9-32"
        If jobACName1 = "DC-9-41" Then jobACName1 = "DC9-32"
        If jobACName1 = "DC-9-51" Then jobACName1 = "DC9-51"
        If jobACName1 = "DC-10-10" Then jobACName1 = "DC10-10"
        If jobACName1 = "DC-10-30" Then jobACName1 = "DC10-30/40"
        If jobACName1 = "DC-10-30 Belly" Then jobACName1 = "DC10-30/40 Belly"
        'If jobACName1 = "MD-11" Then jobACName1 = "MD11ER"
        'If jobACName1 = "MD-11 Belly" Then jobACName1 = "MD11ER Belly"
        If jobACName1 = "MD-82/88" Then jobACName1 = "MD83"
        If jobACName1 = "MD-83" Then jobACName1 = "MD83"
        If jobACName1 = "MD-90-30" Then jobACName1 = "MD90-30 ER"

        'General Aviation
        If jobACName1 = "Sngl Whl-3" Then jobACName1 = "S-3"
        If jobACName1 = "Sngl Whl-5" Then jobACName1 = "S-5"
        If jobACName1 = "Sngl Whl-10" Then jobACName1 = "S-10"
        If jobACName1 = "Single Whl-12.5" Then jobACName1 = "S-12.5"
        If jobACName1 = "Single Whl-15" Then jobACName1 = "S-15"
        If jobACName1 = "Single Whl-20" Then jobACName1 = "S-20"
        If jobACName1 = "Dual Whl-20" Then jobACName1 = "D-20"
        If jobACName1 = "Dual Whl-30" Then jobACName1 = "D-30"

        'Military
        'If jobACName1 = "C-17" Then jobACName1 = "C-17A"
    End Sub


End Module

