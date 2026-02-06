Option Strict On
Option Explicit On


Module modZ_Eval
    Public tt As Single

    Public Sub Calculate_Stress_Rigid1(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                       ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Response = Nothing
        LEAStrActiveX.EvalDepth = LEAStrActiveX.Thick(1)
        LEAStrActiveX.EvalLayer = 1
        LeafResp = ACRClassLib.clsLEAF.LEAFoptions.HorizontalStress

        RunLEAF = New ACRClassLib.clsLEAF()
        Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC,
            LEAStrActiveX, Response, AllResp)
        RunLEAF = Nothing

        If Response Is Nothing Then
            gStress = 0
        Else
            gStress = Response(1, 1)
        End If


    End Sub

    Public Sub Eval_Stress_Thick1(ByRef LEAStrActiveX As ACRClassLib.clsLEAF.LEAFStrParms,
                                  ByVal CallAC() As ACRClassLib.clsLEAF.LEAFACParms)

        Dim FNo As Integer
        Dim FileName As String
        FNo = FreeFile()

        Dim ccc As String
        ccc = Format(Math.Round(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) / MPaToPsi, 0), "000")

        FileName = WDir1 & "\Stress_Thick_" & gCat & "_" & ccc & FileExt
        FileOpen(9, FileName, OpenMode.Output) 'Eval_Stress_Thick

        'PrintLine(9, "     Layers= " & LPad(3, CStr(LEAStrActiveX.NLayers)))
        'PrintLine(9, " Eval Depth= " & LPad(9, Format(LEAStrActiveX.EvalDepth, "##0.00000")))
        'gStress = gStress

        For tt = 5 To 0.1 Step -0.1
            LEAStrActiveX.Thick(1) = tt
            Call Calculate_Stress_Rigid1(LEAStrActiveX, CallAC)
            PrintLine(9, LPad(4, CStr(Math.Round(tt, 2))) & "   " & LPad(3, CStr(gStress)))
        Next
        FileClose(9)


    End Sub



End Module
