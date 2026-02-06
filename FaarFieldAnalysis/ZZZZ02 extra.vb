Option Strict On
Option Explicit On


Imports System.IO

Module ZZZZ02_Extra

    Function Ret_AC(ByRef AC111 As String, ByVal len7 As Integer) As String

        Dim len3 As Integer
        len3 = AC111.Length
        If len3 > len7 Then
            Ret_AC = AC111.Substring(0, len3 - (len3 - len7))
        Else
            Ret_AC = AC111
        End If

    End Function


    'Public Sub After_cmdLife_Click_2()

    '    Dim WDir1 As String, FF1 As String
    '    Dim FileNo As Integer, FF As String
    '    FileNo = FreeFile() : FF = getTodaysDateFormatted()
    '    WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\PCN_Results\"
    '    If Not Directory.Exists(WDir1) Then System.IO.Directory.CreateDirectory(WDir1)

    '    FF1 = WDir1 & SectName & "_Coverages.txt"
    '    FileOpen(FileNo, FF1, OpenMode.Append, , , 1024)

    '    For i As Integer = 1000 To 50000 Step 100
    '        Modulus(NPLayers) = i
    '        ' Call form1.cmdLife_Click(Nothing, Nothing)
    '        'Print(FileNo, LPad(19, Ret_AC(ACName(i), 18)))
    '        'Print(FileNo, LPad(15, CStr(i)))
    '        Print(FileNo, LPad(11, Format(i, "#,##0")))

    '        Print(FileNo, LPad(25, CStr(jobCDFtable(ISect, 1))))
    '        'PrintLine(FileNo, LPad(23, CStr(gCoverage_NtoFail(i))))
    '        PrintLine(FileNo, LPad(25, CStr(publicNtoFail)))

    '    Next
    '    FileClose(FileNo)


    'End Sub



    'Public Sub After_cmdLife_Click()

    '    Dim WDir1 As String, FF1 As String
    '    Dim FileNo As Integer, FF As String
    '    FileNo = FreeFile() : FF = getTodaysDateFormatted()
    '    WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\PCN_Results\"
    '    If Not Directory.Exists(WDir1) Then System.IO.Directory.CreateDirectory(WDir1)

    '    FF1 = WDir1 & SectName & "_Coverages.txt"
    '    FileOpen(FileNo, FF1, OpenMode.Append, , , 1024)

    '    PrintLine(FileNo, "Subgrade modulus= " & Modulus(NPLayers))
    '    PrintLine(FileNo, "CDF=              " & CDFPic)
    '    PrintLine(FileNo, "% of Second AC    " & jobCDFtable(ISect, 2) / CDFPic)


    '    'PrintLine(FileNo, gCDF_target_copy)
    '    'PrintLine(FileNo, gCDF_reached)
    '    'PrintLine(FileNo, gNewAnnualDepart(1))
    '    'PrintLine(FileNo, GL(1))
    '    'PrintLine(FileNo, CDFPic)
    '    'PrintLine(FileNo, gNewPCN(1))
    '    PrintLine(FileNo, "")

    '    Print(FileNo, LPad(19, "ACName"))
    '    'Print(FileNo, LPad(18, "%GW"))
    '    Print(FileNo, LPad(22, "CDF"))
    '    'Print(FileNo, LPad(18, "GL"))
    '    'Print(FileNo, LPad(18, "RepsAnn"))
    '    Print(FileNo, LPad(23, "CovToFailure"))
    '    PrintLine(FileNo, "")

    '    For i As Integer = 1 To NAC
    '        Print(FileNo, LPad(19, Ret_AC(ACName(i), 18)))
    '        'Print(FileNo, MGpcnt(i))
    '        Print(FileNo, LPad(22, CStr(jobCDFtable(ISect, i))))
    '        'Print(FileNo, GL(i))
    '        'PrintLine(FileNo, RepsAnnual(i))
    '        'Print(FileNo, gStrain2C(i))
    '        'PrintLine(FileNo, gNtoFail_copy(i))
    '        'Print(FileNo, gSTRAIN(i))
    '        PrintLine(FileNo, LPad(23, CStr(gCoverage_NtoFail(i))))
    '    Next
    '    FileClose(FileNo)


    'End Sub



    'Public Sub PrintResults002(ByRef form1 As frmStructure)

    '    Dim WDir1 As String
    '    Dim FF1 As String

    '    Dim FileNo As Integer, FF As String
    '    FileNo = FreeFile()
    '    FF = getTodaysDateFormatted()

    '    WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\PCN_Results\"

    '    If Not Directory.Exists(WDir1) Then
    '        System.IO.Directory.CreateDirectory(WDir1)
    '    End If

    '    FF1 = WDir1 & SectName & "_GL.txt"
    '    FileOpen(FileNo, FF1, OpenMode.Append, , , 1024)

    '    'Print(FileNo, LPad(14, Format(RepsAnnual(1), "#,#00.00")))
    '    PrintLine(FileNo, Modulus(NPLayers))
    '    PrintLine(FileNo, gCDF_target_copy)
    '    PrintLine(FileNo, gCDF_reached)
    '    PrintLine(FileNo, gNewAnnualDepart(1))
    '    PrintLine(FileNo, GL(1))
    '    PrintLine(FileNo, CDFPic)
    '    PrintLine(FileNo, gNewPCN(1))
    '    'PrintLine(FileNo, "")
    '    FileClose(FileNo)

    'End Sub




    'Public Sub PrintResults001(ByRef form1 As frmStructure)

    '    Dim WDir1 As String
    '    Dim FF1 As String

    '    Dim FileNo As Integer, FF As String
    '    FileNo = FreeFile()
    '    FF = getTodaysDateFormatted()

    '    WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\PCN_Results\"

    '    If Not Directory.Exists(WDir1) Then
    '        System.IO.Directory.CreateDirectory(WDir1)
    '    End If

    '    'Dim sender As nothing


    '    For ik As Integer = 1050000 To 1100000 Step 1000

    '        GL(1) = ik
    '        Call form1.cmdLife_Click(Nothing, Nothing)

    '        FF1 = WDir1 & SectName & "_" & FF & "_GL.txt"
    '        FileOpen(FileNo, FF1, OpenMode.Append, , , 1024)

    '        Print(FileNo, LPad(14, Format(RepsAnnual(1), "#,#00.00")))
    '        Print(FileNo, LPad(14, Format(GL(1), "#,#00")))
    '        Print(FileNo, "  " & CDFPic)
    '        PrintLine(FileNo, "")

    '        FileClose(FileNo)

    '    Next ik


    'End Sub





    'Public Sub PrintBody(ByVal FileNo As Integer, ByVal FileName1 As String)

    '    FileOpen(FileNo, FileName1, OpenMode.Append, , , 1024)
    '    Print(FileNo, LPad(10, Format(Modulus(NPLayers), "##,##0.0")))
    '    Print(FileNo, LPad(8, Format(RepsInc(1), "##,##0.000")))
    '    Print(FileNo, LPad(32, Format(LifeStr, "##,##0.0000000000000")))
    '    tdiff = DateDiff(DateInterval.Second, gS1, Now(),
    '        FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)
    '    Print(FileNo, LPad(12, Format(tdiff / 60, "##,##0.00")) & " min.")
    '    Print(FileNo, LPad(32, Format(OverlayLife, "##,##0.0000000000000")))
    '    PrintLine(FileNo, "")
    '    FileClose(FileNo)

    'End Sub



    ''Private Sub btnT_Click(sender As Object, e As EventArgs) Handles btnT.Click

    ''    Dim FileNo As Integer, FileName1 As String
    ''    Dim I1 As Integer, tStart As Date
    ''    Dim Now1 As DateTime, Time1 As String


    ''    Try
    ''        Me.Enabled = False
    ''        tStart = Now()  'RepsAnnual(1) = 1200 '500   1000   2000
    ''        LifeError = 0.07

    ''        FileNo = FreeFile() 'CStr(Thick(1)) &
    ''        FileName1 = WorkingDir & "ssssssssssssssss_" & SectName & ".txt"

    ''        Now1 = DateTime.Now
    ''        Time1 = Now1.ToLongDateString

    ''        gHMAonRigid_Mod = False
    ''        gHMAonRigid_Mod = True

    ''        FileOpen(FileNo, FileName1, OpenMode.Append, , , 1024)
    ''        PrintLine(FileNo, Environment.CommandLine)
    ''        PrintLine(FileNo, Now1.ToLongDateString & "   " & Now1.ToLongTimeString)

    ''        PrintLine(FileNo, "JobName: " & JobName & "   SectName: " & SectName & "   HMAonRig=" & gHMAonRigid_Mod)
    ''        PrintLine(FileNo, "")


    ''        Print(FileNo, LPad(8, "%CDFU"))
    ''        Print(FileNo, LPad(7, "SCI"))

    ''        'Print(FileNo, LPad(8, "gTh1"))
    ''        'Print(FileNo, LPad(8, "Thick1"))
    ''        'Print(FileNo, LPad(7, "Thick2"))
    ''        'Print(FileNo, LPad(11, "CDF"))
    ''        'Print(FileNo, LPad(11, "picCDF"))
    ''        'Print(FileNo, LPad(8, "CtoP"))
    ''        'Print(FileNo, LPad(8, "PtoC"))
    ''        'Print(FileNo, LPad(10, "1Crack"))
    ''        'Print(FileNo, LPad(10, "LifeB"))
    ''        'Print(FileNo, LPad(10, "LifeE"))
    ''        'Print(FileNo, LPad(9, "AsFlex"))

    ''        'Print(FileNo, LPad(12, "T1min"))
    ''        'Print(FileNo, LPad(12, "T2min"))
    ''        'Print(FileNo, LPad(12, "LifevOver1"))

    ''        'Print(FileNo, LPad(12, "LifeStr1"))
    ''        'Print(FileNo, LPad(12, "LifeStr"))
    ''        'Print(FileNo, LPad(12, "OverLife"))
    ''        'Print(FileNo, LPad(12, "SubModul"))
    ''        'Print(FileNo, LPad(12, "Time"))
    ''        'Print(FileNo, LPad(7, "Case"))
    ''        'Print(FileNo, LPad(12, "LifeStr2"))
    ''        'Print(FileNo, LPad(12, "OverLife2"))
    ''        'Print(FileNo, LPad(12, "LifeTh2"))
    ''        'Print(FileNo, LPad(12, "OverTh2"))

    ''        PrintLine(FileNo, "") : FileClose(FileNo)

    ''        'LifeExistPCC = 100
    ''        'SCIB = 100
    ''        'For I1 = CShort(lstStrFiles.SelectedIndex) To CShort(lstStrFiles.SelectedIndex)
    ''        I1 = CShort(lstStrFiles.SelectedIndex)
    ''        lstStrFiles.SelectedIndex = I1
    ''        Dim indexAD As Single

    ''        'For sssMod As Single = 1000 To 15000 Step 1000
    ''        'Modulus(NPLayers) = sssMod
    ''        'For ssss1 As Single = 1000 To 999 Step -1000
    ''        For ssss1 As Single = 0 To 0.01! Step CSng(0.001)

    ''            For ik As Integer = 1 To NAC
    ''                RepsInc(ik) = ssss1
    ''            Next


    ''            'If ssss1 < 1000 Then
    ''            '    Exit For
    ''            '    ssss1 = 1000
    ''            'End If

    ''            'Modulus(NPLayers) = ssss1

    ''            'For subm1 As Single = 20 To 3.99 Step -0.2
    ''            'For subm1 As Single = 20 To 3.99 Step -0.2
    ''            gS1 = Now()

    ''            'Thick(1) = 4
    ''            'Thick(2) = subm1
    ''            'Thick(2) = 4

    ''            'For subm1 As Single = 1300 To 1300 Step 200
    ''            '    Modulus(NPLayers) = subm1

    ''            'SCIB = 100
    ''            'For indexAD = 180 To 100 Step 10  'For LifeExistPCC
    ''            '    LifeExistPCC = indexAD
    ''            '    Call subroutine1(sender, e, FileNo, FileName1)
    ''            'Next indexAD

    ''            'LifeExistPCC = 100
    ''            'For indexAD = 195 To 95 Step -1 'For SCIB
    ''            '    SCIB = indexAD
    ''            '    Call subroutine1(sender, e, FileNo, FileName1)
    ''            'Next indexAD

    ''            'FileOpen(FileNo, FileName1, OpenMode.Append, , , 1024)
    ''            'PrintLine(FileNo, "")
    ''            'FileClose(FileNo)
    ''            '    Call subroutine1(sender, e, FileNo, FileName1)

    ''            'Next

    ''            Call cmdLife_Click(sender, e)


    ''            FileOpen(FileNo, FileName1, OpenMode.Append, , , 1024)

    ''            Print(FileNo, LPad(12, Format(RepsInc(1), "##,##0.000")))
    ''            Print(FileNo, LPad(32, Format(LifeStr, "##,##0.0000000000000")))

    ''            tdiff = DateDiff(DateInterval.Second, gS1, Now(), _
    ''                FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)
    ''            Print(FileNo, LPad(12, Format(tdiff / 60, "##,##0.00")) & " min.")
    ''            Print(FileNo, LPad(32, Format(OverlayLife, "##,##0.0000000000000")))

    ''            PrintLine(FileNo, "")
    ''            FileClose(FileNo)
    ''        Next

    ''        tdiff = DateDiff(DateInterval.Second, tStart, Now(), _
    ''                FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)
    ''        FileOpen(FileNo, FileName1, OpenMode.Append, , , 1024)
    ''        PrintLine(FileNo, "") 'tdiff as Long
    ''        PrintLine(FileNo, LPad(12, Format(tdiff / 60, "##,##0.00")) & " min.")
    ''        PrintLine(FileNo, "")
    ''        FileClose(FileNo) : BatchMode = False
    ''        Me.Enabled = True

    ''    Catch ex As Exception

    ''        Dim txt As String
    ''        txt = ex.Message
    ''        txt = txt + Environment.NewLine + Environment.NewLine
    ''        txt = txt + ex.StackTrace
    ''        txt = txt + Environment.NewLine + Environment.NewLine
    ''        MsgBox(txt)
    ''    End Try


    ''End Sub













End Module
