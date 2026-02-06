Option Strict On
Option Explicit On

Friend Module ACN_Module

    Friend Function Get1() As Integer
        Get1 = 33
    End Function


    Private Sub ZZZ_PrintOut01(ByVal iTh As Integer)

        gPrintOutput_ACN = False
        Call Check_for_Subdirectory()

        Dim FNo As Integer
        FNo = FreeFile()
        Dim FileName As String
        Dim th7 As Double
        FileName = WDir1 & "\PCC222_AAAAA_22" & FileExt
        FileOpen(FNo, FileName, OpenMode.Append)

        Dim i7 As Single
        'For i7 = 1 To 21 Step 1
        For i7 = 13 To 17.01 Step 0.2
            LEAStrActiveX.Thick(iTh) = i7
            th7 = GetThick(iTh)

            Dim RunACN As ACRClassLib.clsACR
            RunACN = New ACRClassLib.clsACR()
            Call RunACN.Calculate_Flex_Coverages()
            RunACN = Nothing

            Dim NtoFail As Single, julNPLayers As Short
            'NtoFail = RunACN.julLCode
            'julNPLayers = RunACN.julLCode(1)

            PrintLine(FNo, LPad(4, CStr(i7)), TAB(7), LPad(22, Format(NtoFail, "#,##0.00")) &
                      "    " & LEAStrActiveX.Modulus(julNPLayers))


        Next
        FileClose(FNo)

    End Sub




    Friend Sub AnalyzeModulus()


        Dim i3 As Single

        Call Check_WDir1_Directory()
        Dim FileName As String
        FileName = WDir1 & "\Cat3333" & FileExt

        FileOpen(9, FileName, OpenMode.Append) 'Cat_1234


        For i3 = 19.09 + 5 To 19.14 + 5 Step 0.01
            LEAStrActiveX.Thick(2) = i3 - 5
            'Call Calculate_Flex_Coverages()

            Dim RunACN As ACRClassLib.clsACR
            RunACN = New ACRClassLib.clsACR()
            Call RunACN.Calculate_Flex_Coverages()
            RunACN = Nothing

            'For i As Integer = 1 To LEAStrActiveX.NLayers
            Print(9, LPad(5, Format(i3, "###.00")))
            Print(9, LPad(7, Format(LEAStrActiveX.Thick(2), "##0.00")))
            Print(9, LPad(7, Format(LEAStrActiveX.Thick(3), "##0.00")))
            Print(9, LPad(15, Format(LEAStrActiveX.Modulus(2), "###,###.00")))
            Print(9, LPad(15, Format(LEAStrActiveX.Modulus(3), "###,###.00")))
            PrintLine(9, "")
            'Next

        Next
        'PrintLine(9, "") : PrintLine(9, "")
        FileClose(9)


    End Sub



End Module
