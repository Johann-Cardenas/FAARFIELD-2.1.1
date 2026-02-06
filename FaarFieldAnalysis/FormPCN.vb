Imports System
Imports System.IO


Public Class FormPCN

    Private Sub FormPCN_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Try

            'lblPCN.Text = PCNOutputText
            'lblPCN.AutoSize = True

            Me.Text = "PCR Results for Section " & gGraphSection & " in Job " & gGraphJob

            tbPCN.Text = PCNOutputText
            tbPCN.Select(0, 0)

            Dim WDir1 As String
            Dim FF1 As String

            Dim FileNo As Integer, FF As String
            FileNo = FreeFile()
            FF = getTodaysDateFormatted()

            'WDir1 = System.Windows.Forms.Application.StartupPath & "\FAARFIELD\PCN_Results"

            WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\PCN_Results\"

            If Not Directory.Exists(WDir1) Then
                System.IO.Directory.CreateDirectory(WDir1)
            End If

            FF1 = WDir1 & SectName & "_" & FF & ".txt"

            'FileOpen(FileNo, SectName & FF & ".txt", OpenMode.Append, , , 1024)
            FileOpen(FileNo, FF1, OpenMode.Append, , , 1024)
            PrintLine(FileNo, PCNOutputText)
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

End Class