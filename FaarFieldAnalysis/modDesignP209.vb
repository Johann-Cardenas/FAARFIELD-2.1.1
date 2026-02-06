Public Module modDesignP209

    Public DesigningP209 As Boolean 'to exit sub DrawStructure when P209 performed.
    Public OutPutFile As Boolean
    Public DesigningP209DrawStructure As Boolean

    'Stores previous values for iterative P209 solve
    Public oldThick As Single
    Public oldModulus As Single
    Public oldLCode As Short
    Public oldNPLayers As Short



    Sub CheckMinThickness() 'for minP209
        Dim IAircraft As Integer, valCDF As Single
        Dim IA As Integer, minP209 As Single

        If DesignType <> NewFlex Then Exit Sub
        If LCode(2) <> 6 Then Exit Sub 'PPPP

        valCDF = 0

        For IA = 1 To NAC
            If valCDF < jobCDFtable(ISect, IA) Then
                valCDF = CSng(jobCDFtable(ISect, IA))
                IAircraft = IA
            End If
        Next IA

        If lightAircraft30kLess Then
            minP209 = 3
        ElseIf CallAC(IAircraft).NTires = 1 And
           CallAC(IAircraft).GearLoad < 50000 Then
            minP209 = 4
        ElseIf CallAC(IAircraft).NTires = 1 And
               CallAC(IAircraft).GearLoad <= 75000 Then
            minP209 = 6
            'ElseIf Mid(CallAC(IAircraft).ACname, 1, 8) = "Dual Whl" And _
        ElseIf CallAC(IAircraft).NTires = 2 And
               CallAC(IAircraft).GearLoad < 100000 Then
            minP209 = 6
            'ElseIf Mid(CallAC(IAircraft).ACname, 1, 8) = "Dual Tan" And _
        ElseIf CallAC(IAircraft).NTires = 4 And
               CallAC(IAircraft).GearLoad < 250000 Then
            minP209 = 6
        ElseIf Mid(CallAC(IAircraft).ACname, 1, 4) = "B757" And
               CallAC(IAircraft).GearLoad < 400000 Then
            minP209 = 6
        ElseIf Mid(CallAC(IAircraft).ACname, 1, 4) = "B767" And
               CallAC(IAircraft).GearLoad < 400000 Then
            minP209 = 6
        ElseIf Mid(CallAC(IAircraft).ACname, 1, 4) = "B747" And
               CallAC(IAircraft).GearLoad < 600000 Then
            minP209 = 6
        ElseIf CallAC(IAircraft).ACname = "C-130" And
               CallAC(IAircraft).GearLoad < 125000 Then
            minP209 = 4
        ElseIf CallAC(IAircraft).ACname = "C-130" And
               CallAC(IAircraft).GearLoad <= 175000 Then
            minP209 = 6
        Else
            minP209 = 8
        End If

        If Thick(2) < minP209 Then
            'TODO: Warn user about minimum thickness.  Should this update automatically?
            Thick(2) = minP209
        End If

    End Sub

    Sub SetData_DesignBase_SubgadeCBR20_4Layers()

        oldThick = Thick(3)
        oldModulus = Modulus(3)
        oldLCode = LCode(3)
        oldNPLayers = NPLayers

        Thick(3) = Thick(NPLayers)
        Modulus(3) = 30000
        LCode(3) = LCode(NPLayers)

        NPLayers = 3

    End Sub


    Sub DesignRestoreStructure_4Layers()

        Call CheckMinThickness()

        Thick(3) = oldThick
        Modulus(3) = oldModulus
        LCode(3) = oldLCode
        NPLayers = oldNPLayers


        Call CheckMinThickness()
        'This is deprecated.   Thickness checks are now done elsewhere
        ''ikawa
        'If jobThick(ISect, 2) > ThickMin(6) Or Thick(2) > ThickMin(6) Then
        '    ThickMin(6) = Math.Max(jobThick(ISect, 2), Thick(2))
        'End If
    End Sub

    Sub SetData_DesignBase_SubgadeCBR20_5Layers()
        '5 layer case
        oldThick = Thick(4)
        oldModulus = Modulus(4)
        oldLCode = LCode(4)
        oldNPLayers = NPLayers

        Thick(4) = Thick(NPLayers)
        Modulus(4) = 30000
        LCode(4) = LCode(NPLayers)
        NPLayers = 4

    End Sub

    Sub DesignRestoreStructure_5Layers()
        Call CheckMinThickness()

        Thick(4) = oldThick
        Modulus(4) = oldModulus
        LCode(4) = oldLCode
        NPLayers = oldNPLayers

        Call CheckMinThickness()

        'This is deprecated.   Thickness checks are now done elsewhere
        ''ikawa
        'If jobThick(ISect, 3) > ThickMin(6) Or Thick(3) > ThickMin(6) Then
        '    ThickMin(6) = Math.Max(jobThick(ISect, 3), Thick(2))
        'End If

    End Sub




End Module
