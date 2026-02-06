'Option Strict On
'Option Explicit On


'Module ZZZZZ_TEST
'    Public gWDir1 As String
'    Public gFileName1 As String
'    Public gFileN1 As Integer


'    Public Sub gOpenFile1()

'        'Exit Sub
'        Dim myDoc1 As String

'        myDoc1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
'        gWDir1 = myDoc1 & "\FAARFIELD\PCN_Results\"
'        'gFileName1 = "RRR" & "_" & Format(Modulus(NPLayers), "#")
'        gFileName1 = JobName & "_" & SectName

'        gFileN1 = FreeFile()
'        FileOpen(gFileN1, gWDir1 & "\" & gFileName1 & ".txt", OpenMode.Append, , , 1024)
'        'FileOpen(FileNo, FileName, OpenMode.Append, , , 1024)

'    End Sub

'    Public Sub gCloseFile1()
'        'Exit Sub
'        FileClose(gFileN1)
'    End Sub





'    Public Sub TEST_NewAdjustAnnDepart2017()

'        Dim gFF1 As Integer, gFileName1, WDir1, myDoc1 As String
'        Static iii As Integer
'        myDoc1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
'        WDir1 = myDoc1 & "\FAARFIELD\PCN_Results\"

'        iii = iii + 1
'        'gFileName1 = "NewAdjustAnnDepart2017" & "_" & CStr(iii)
'        gFileName1 = "NewAdjustAnnDepart2017" & "_" & Format(Modulus(NPLayers), "#")

'        For ik As Single = 0 To 1000000 Step 1000

'            RepsAnnual(1) = ik
'            RepsAnnual(2) = RepsAnnual(1)
'            Call frmStructure.cmdLife_Click(Nothing, Nothing)

'            gFF1 = FreeFile()
'            FileOpen(gFF1, WDir1 & "\" & gFileName1 & ".txt", OpenMode.Append)
'            'FileOpen(gFF1, gFileName1 & ".txt", OpenMode.Append)
'            'Print(gFF1, LPad(12, Format(Modulus(NPLayers), "#0.00")))
'            Print(gFF1, LPad(12, Format(RepsAnnual(1), "#0.00")) & "   ")
'            Print(gFF1, CDFPic)
'            PrintLine(gFF1, "")
'            FileClose(gFF1)
'        Next


'    End Sub



'End Module
