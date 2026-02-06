Module modZ_Edward

    'search
    'ElseIf DesignType = NewRigid Then

    Public Sub Edward_Input()

        Static is1 As Integer
        is1 += 1

        If SectName = "MRS1-N-105E2" Then
            Modulus(1) = 4500000 : RCon(1) = 660
        ElseIf SectName = "MRS1-N-105E3" Then
            Modulus(1) = 4500000 : RCon(1) = 660
        End If

        If SectName = "MRS1-N-105E" Then '1
            Modulus(1) = 4500000 : RCon(1) = 660
        ElseIf SectName = "MRS1-N-105R" Then '2
            Modulus(1) = 4286000 : RCon(1) = 693
        ElseIf SectName = "MRS1-N-4m" Then '3
            Modulus(1) = 4000000 : RCon(1) = 700
        ElseIf SectName = "MRS1-N-8m" Then '4
            Modulus(1) = 4000000 : RCon(1) = 700
        ElseIf SectName = "MRS1-N-95E" Then '5
            Modulus(1) = 4072000 : RCon(1) = 660


        ElseIf SectName = "MRS1-N-95R" Then '6
            Modulus(1) = 4286000 : RCon(1) = 627
        ElseIf SectName = "MRS1-North" Then '7
            Modulus(1) = 4286000 : RCon(1) = 660
        ElseIf SectName = "MRS1-S-105E" Then '8
            Modulus(1) = 4500000 : RCon(1) = 660
        ElseIf SectName = "MRS1-S-105R" Then '9
            Modulus(1) = 4286000 : RCon(1) = 693
        ElseIf SectName = "MRS1-S-95E" Then '10
            Modulus(1) = 4072000 : RCon(1) = 660


        ElseIf SectName = "MRS1-S-95R" Then '11
            Modulus(1) = 4286000 : RCon(1) = 627
        ElseIf SectName = "MRS1-South" Then '12
            Modulus(1) = 4286000 : RCon(1) = 660


        ElseIf SectName = "MRS2-North" Then '13
            Modulus(1) = 6028000 : RCon(1) = 749
        ElseIf SectName = "MRS2-North45" Then '**
            Modulus(1) = 6028000 : RCon(1) = 749
        ElseIf SectName = "MRS2-North52" Then '13
            Modulus(1) = 6028000 : RCon(1) = 749
        ElseIf SectName = "MRS2-North70" Then '13
            Modulus(1) = 6028000 : RCon(1) = 749


        ElseIf SectName = "MRS2-South" Then '14
            Modulus(1) = 6028000 : RCon(1) = 749
        ElseIf SectName = "MRS2-South45" Then '**
            Modulus(1) = 6028000 : RCon(1) = 749
        ElseIf SectName = "MRS2-South52" Then '14
            Modulus(1) = 6028000 : RCon(1) = 749
        ElseIf SectName = "MRS2-South70" Then '14
            Modulus(1) = 6028000 : RCon(1) = 749



        ElseIf SectName = "MRS3-N-105E" Then '15
            Modulus(1) = 6837000 : RCon(1) = 932
        ElseIf SectName = "MRS3-N-105R" Then '16
            Modulus(1) = 6511000 : RCon(1) = 978.6
        ElseIf SectName = "MRS3-N-95E" Then '17
            Modulus(1) = 6185000 : RCon(1) = 932
        ElseIf SectName = "MRS3-N-95R" Then '18
            Modulus(1) = 6511000 : RCon(1) = 885.4
        ElseIf SectName = "MRS3-North" Then '19
            Modulus(1) = 6511000 : RCon(1) = 932
        ElseIf SectName = "MRS3-S-105E" Then '20
            Modulus(1) = 6837000 : RCon(1) = 932


        ElseIf SectName = "MRS3-S-105R" Then '21
            Modulus(1) = 6511000 : RCon(1) = 978.6
        ElseIf SectName = "MRS3-S-95E" Then '22
            Modulus(1) = 6185000 : RCon(1) = 932
        ElseIf SectName = "MRS3-S-95R" Then '23
            Modulus(1) = 6511000 : RCon(1) = 885.4
        ElseIf SectName = "MRS3-South" Then '24
            Modulus(1) = 6511000 : RCon(1) = 932

        ElseIf SectName = "MRS1" Then '25
            Modulus(1) = 4286000 : RCon(1) = 660
        ElseIf SectName = "MRS2" Then '26
            Modulus(1) = 6028000 : RCon(1) = 749
        ElseIf SectName = "MRS3" Then '27
            Modulus(1) = 6511000 : RCon(1) = 932
        Else
            'MsgBox("Section: " & SectName, MsgBoxStyle.OkOnly, "Section not found")

        End If


    End Sub



    Public Sub Edward_Output()

        ' Job   Section  Modulus    

        'FileOpen(13, WorkingDir & "\Edward_Input.txt", OpenMode.Append, , , 1024)
        ''Print(13, LPad(9, JobName) & "   ")
        ''Print(13, SectName & Space(14 - SectName.Length))
        'Print(13, LPad(12, JobName))
        'Print(13, LPad(14, SectName))
        'Print(13, LPad(14, Format(Modulus(1), "##,##0.00")))
        'Print(13, LPad(10, Format(RCon(1), "##,##0.00")))
        ''Print(13, lstSects.Items(I).ToString.Substring(0, lstSects.Items(I).ToString.Length - 10))
        'Print(13, LPad(13, Format(CallAC(1).GearLoad / CallAC(1).NTires, "##,##0.00")))
        'Print(13, LPad(12, Format(CallAC(1).TirePress(1), "##,##0.00")))
        'PrintLine(13, LPad(12, Format(Modulus(NPLayers), "##,##0.00")))

        'FileClose(13)



        Dim FileNo As Integer
        FileNo = FreeFile()
        FileName = WorkingDir & "\Edward_Output.txt"
        'FileOpen(FileNo, FileName, OpenMode.Append, , , 1024)
        'Print(FileNo, LPad(12, JobName))
        'Print(FileNo, LPad(14, SectName))
        'Print(FileNo, LPad(14, Format(HorizStressResponse1(1, 1), "##,##0.0000")))
        'Print(FileNo, LPad(14, Format(HorizStressResponse2(1, 1), "##,##0.0000")))
        'PrintLine(FileNo, LPad(16, Format(LifeStr, "##,##0.00")))
        'FileClose(FileNo)


    End Sub




End Module
