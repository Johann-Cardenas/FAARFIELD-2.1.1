Option Strict On
Option Explicit On

Public Module T_PCN

    'Public gRunTest As Boolean = False

    Public newCDFPic As Double
    Public oldCDFPic As Double


    'If Not (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
    '    seq1 = seq1 + 1
    '    'S1 = S1 & LPad(3, Format(I, "0")) & "  "
    '    S1 = S1 & LPad(3, Format(seq1, "0")) & "  "
    '    S1 = S1 & (RPad2(20, ACName(I)))
    'End If




    'Dim u1 As Integer
    'u1 = UBound(gNewPCN, 1)

    'If gNewPCN(1) >= gNewPCN(2) Then
    '    indexPCNtable2 = 1
    'Else
    '    indexPCNtable2 = 2
    'End If

    'If u1 = 3 Then
    '    If gNewPCN(3) >= gNewPCN(indexPCNtable2) Then
    '        indexPCNtable2 = 3
    '    End If
    'End If

    Public ind1max As Integer

    Public Sub Redim0NewACN(ByVal size1 As Integer)

        ReDim modPCN_ZZZ.gNewACName(size1)
        ReDim modPCN_ZZZ.gNewAnnualDepart(size1)
        ReDim modPCN_ZZZ.gNewGL(size1)
        ReDim modPCN_ZZZ.gNewPCN(size1)
        ReDim modPCN_ZZZ.gNewPCNthick(size1)

    End Sub


    Public Sub RedimNewACN(ByVal size1 As Integer)

        ReDim Preserve modPCN_ZZZ.gNewACName(size1)
        ReDim Preserve modPCN_ZZZ.gNewAnnualDepart(size1)
        ReDim Preserve modPCN_ZZZ.gNewGL(size1)
        ReDim Preserve modPCN_ZZZ.gNewPCN(size1)
        ReDim Preserve modPCN_ZZZ.gNewPCNthick(size1)

    End Sub

    Public Function AssignACN_thick(ByRef ACN1 As ACRClassLib.clsACR.ACRdata,
                                  ByRef ACN_R1 As ACRClassLib.clsACR.ACRdata,
                                  ByRef ACN_R2 As ACRClassLib.clsACR.ACRdata,
                                  ByVal i1 As Integer) As Single

        If ACN1.libACR(i1) = ACN_R1.libACR(i1) Then
            ACN1.libACRthick(i1) = ACN_R1.libACRthick(i1)
        Else
            ACN1.libACRthick(i1) = ACN_R2.libACRthick(i1)
        End If

    End Function

    'ACNdataFF1.libACRthick(1) = AssignACN_thick(ACNdataFF1, ACNdataFF_Rigid1, ACNdataFF_Rigid2, 1)


    Public Function GetTarget1() As Single

        If (DesignType = FlexOnRigid Or OverlayRigOnRig) Then
            GetTarget1 = modPCN_ZZZ.gOverlayLife_target
        Else
            GetTarget1 = modPCN_ZZZ.gCDF_target
        End If

    End Function



    Public Function GetResult1() As Single

        'Call frmStructure.cmdLife_Click(Nothing, Nothing)

        If (DesignType = FlexOnRigid Or OverlayRigOnRig) Then
            GetResult1 = OverlayLife
        Else
            GetResult1 = CDFPic
        End If

    End Function


    'https://msdn.microsoft.com/en-us/library/ms973905.aspx
    'https://stackoverflow.com/questions/19151515/click-a-button-programmatically


End Module
