Partial Public Class clsFEM

    Sub InitialSet(ByVal I1 As Integer, ByRef IDCase As String, ByRef lutty As Integer,
                   ByVal WorkingDir As String, ByVal ModelOut As Integer)
        Dim FileName As String, FileName2 As String

        Dim FileName1 As String, int1 As Integer 'ik2019.12.18
        int1 = WorkingDir.IndexOf("PrintOut-") + 9
        FileName1 = WorkingDir.Substring(int1, WorkingDir.Length - int1)
        FileName$ = WorkingDir.Substring(0, int1 - 10) & "\FAASR3d-" & FileName1 & ".txt"
        FileName2 = WorkingDir & "\FAASR3d.txt"

        lutty = 59

        If ModelOut = 1 Then
            FileOpen(lutty, FileName2, OpenMode.Output, , , 1024) 'ik2020.02.10
        End If

        'FileOpen(lutty, FileName, OpenMode.Output, , , 1024)

        If I1 = 1 Then
            IDCase = "1DSYM"
        ElseIf I1 = 2 Then
            IDCase = "2DSYM"
        ElseIf I1 = 3 Then
            IDCase = "3DSYM"
        ElseIf I1 = 4 Then
            IDCase = "4DNSY"
        ElseIf I1 = 5 Then
            IDCase = "5DSYM"
        End If

    End Sub

End Class
