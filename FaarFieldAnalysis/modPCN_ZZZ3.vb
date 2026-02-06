Option Strict On
Option Explicit On

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.IO

Public Module modPCN_ZZZ3

    Public Sub SubLoopCalculations()

        Dim WDir1, myDoc1 As String
        myDoc1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        WDir1 = myDoc1 & "\FAARFIELD\PCN_Results\"
        If Not Directory.Exists(WDir1) Then System.IO.Directory.CreateDirectory(WDir1)

        Dim FileNo As Integer, FileName1 As String
        Dim ik As Integer, tStart As Date, Now1 As DateTime, Time1 As String

        FileNo = FreeFile()
        FileName1 = WDir1 & "CDF_" & SectName & "_" & getTodaysDateFormatted() & ".txt"
        Now1 = DateTime.Now
        Time1 = Now1.ToLongDateString

        '===================================================================================
        FileOpen(FileNo, FileName1, OpenMode.Append, , , 1024)
        PrintLine(FileNo, Environment.CommandLine)
        PrintLine(FileNo, "JobName: " & JobName & "   SectName: " & SectName)
        PrintLine(FileNo, "")
        Print(FileNo, LPad(12, "GL1"))
        Print(FileNo, LPad(15, "CDF"))
        PrintLine(FileNo, "")
        FileClose(FileNo)

        Dim GL1 As Single

        For GL1 = 10000 To 900000 Step 10000
            GL(1) = GL1

            ' Call frmStructure.cmdLife_Click(Nothing, Nothing)

            FileOpen(FileNo, FileName1, OpenMode.Append, , , 1024)
            Print(FileNo, LPad(12, Format(GL(1), "#,##0.00")))

            If CDFPic < 0.001 Then
                Print(FileNo, (CDFPic))
            ElseIf CDFPic < 10 Then
                Print(FileNo, LPad(15, Format(CDFPic, "#,##0.000000000")))
            ElseIf CDFPic < 1000 Then
                Print(FileNo, LPad(15, Format(CDFPic, "#,##0.000")))
            Else
                Print(FileNo, LPad(15, Format(CDFPic, "#,##0")))
            End If


            'TimeSave2 = timeGetTime
            'gETimemsecs = CSng((TimeSave2 - TimeSave1) / 1000)
            'Print(FileNo, LPad(8, CStr(Format(gETimemsecs, "#0"))))
            PrintLine(FileNo, "")
            FileClose(FileNo)
        Next



    End Sub


End Module
