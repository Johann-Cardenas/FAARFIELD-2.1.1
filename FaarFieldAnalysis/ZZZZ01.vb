Imports System.IO

Public Module ZZZZ01


    'https://docs.microsoft.com/en-us/dotnet/framework/winforms/controls/programmatically-resize-cells-to-fit-content-in-the-datagrid
    'google(datagridview column width fit text)


    Public gInd1 As Integer
    Public gDontPrint_PCN As Boolean
    Public gCriticalOffset As Integer
    Public gGraphSection As String
    Public gGraphJob As String

    Function PaveTypeF(ByVal PavementType1 As ACRClassLib.clsACR.PavementType) As String
        If PavementType1 = ACRClassLib.clsACR.PavementType.Flexible Then
            PaveTypeF = "Flexible"
        Else
            PaveTypeF = "Rigid"
        End If

    End Function




    Public Sub gSaveCDFdata()
        Try

            Dim n1, n2, n3 As Integer, col3 As Integer
            Dim WDir1, FF1 As String, FileNo As Integer
            FileNo = FreeFile()
            WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\"
            col3 = 17
            Dim off3 As Integer

            If Not Directory.Exists(WDir1) Then
                System.IO.Directory.CreateDirectory(WDir1)
            End If

            FF1 = WDir1 & SectName & "_" & "CDFdata333" & ".txt"
            'FileOpen(FileNo, FF1, OpenMode.Output, , , 1024)
            FileOpen(FileNo, FF1, OpenMode.Append, , , 1024)

            Dim ACnameCopy(80) As String, len3 As Integer

            For n1 = 1 To NAC
                'PrintLine(FileNo, LPad(3, CStr(n1)) & LPad(25, ACName(n1)))
                len3 = ACName(n1).Length
                If len3 > 16 Then
                    ACnameCopy(n1) = ACName(n1).Substring(0, len3 - (len3 - 16))
                Else
                    ACnameCopy(n1) = ACName(n1)
                End If

            Next
            'PrintLine(FileNo, "") : PrintLine(FileNo, "")

            Print(FileNo, LPad(7, "OFFSET"))
            For n1 = 1 To NAC
                'Print(FileNo, LPad(15, "AC" & CStr(n1)))
                Print(FileNo, LPad(col3, ACnameCopy(n1)))
            Next
            PrintLine(FileNo, LPad(col3, "Cumulative CDF"))

            Dim sum1 As Single
            For n3 = 41 To 2 Step -1
                sum1 = 0.0
                off3 = -(n3 - 1) * 10
                Print(FileNo, LPad(7, CStr(off3)))
                For n2 = 1 To NAC
                    Print(FileNo, LPad(col3, CStr(CDFdata2(1, n2, n3))))
                    sum1 = sum1 + CDFdata2(1, n2, n3)
                Next
                Print(FileNo, LPad(col3, CStr(sum1)))
                PrintLine(FileNo, "")
            Next

            For n3 = 1 To 41
                sum1 = 0.0
                off3 = (n3 - 1) * 10
                Print(FileNo, LPad(7, CStr(off3)))
                For n2 = 1 To NAC
                    Print(FileNo, LPad(col3, CStr(CDFdata2(1, n2, n3))))
                    sum1 = sum1 + CDFdata2(1, n2, n3)
                Next
                Print(FileNo, LPad(col3, CStr(sum1)))
                PrintLine(FileNo, "")
            Next

            PrintLine(FileNo, "")
            PrintLine(FileNo, "")
            FileClose(FileNo)


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub




End Module
