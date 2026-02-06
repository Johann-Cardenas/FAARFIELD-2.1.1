

Option Strict On
Option Explicit On


Friend Module ZZZ



    'http://stackoverflow.com/questions/2101207/datagridview-header-alignment
    'aaa = DataGridViewContentAlignment.MiddleCenter
    'dgwXY.Columns[0].HeaderCell.Style.Alignment = DataGridViewContentAlignment.BottomCenter


    'http://stackoverflow.com/questions/2593458/executable-directory-where-application-is-running-from
    'System.Windows.Forms.Application.StartupPath

    'https://msdn.microsoft.com/en-us/library/system.io.directory.exists(v=vs.110).aspx
    'http://stackoverflow.com/questions/85996/how-do-i-create-a-folder-in-vb-if-it-doesnt-exist

    'http://stackoverflow.com/questions/2773430/vb-net-how-to-prevent-user-input-in-a-combobox
    'http://stackoverflow.com/questions/5560019/vb-net-get-only-year-from-date

    'http://stackoverflow.com/questions/2154154/datagridview-how-to-set-column-width


    'http://stackoverflow.com/questions/19721984/vb-net-how-to-get-cell-value-from-datagridview
    'https://msdn.microsoft.com/library/x8x9zk5a(v=vs.100).aspx


    'Public Structure ACRdata
    '    Dim libACN() As Single
    '    Dim libACRthick() As Single
    'End Structure


    'Public Function CalculateACR(ByVal PavementType As String, _
    '             ByVal gross_weight As Single, _
    '             ByVal percent_gw As Single, _
    '             ByVal wheels_number As Integer, _
    '             ByVal tire_pressure As Single, _
    '             ByVal CoordX() As Single, _
    '             ByVal CoordY() As Single) As ACRdata


    '    ReDim CalculateACN.libACN(4)
    '    ReDim CalculateACN.libACRthick(4)


    '    CalculateACN.libACN(1) = 300
    '    CalculateACN.libACN(2) = 233
    '    CalculateACN.libACN(3) = 11
    '    CalculateACN.libACN(4) = 5

    '    CalculateACN.libACRthick(1) = 44
    '    CalculateACN.libACRthick(2) = 44
    '    CalculateACN.libACRthick(3) = 44
    '    CalculateACN.libACRthick(4) = 22

    'End Function


    '            If wheels_number <= 2 And iCat = 4 Then
    '            LEAStrActiveX.NLayers = 3
    '            Call RedimArraysFlexible(LEAStrActiveX.NLayers, iCat)

    '            LEAStrActiveX.Thick(1) = 3 'P-401
    '            LEAStrActiveX.Thick(2) = 4 'P-209 variable

    '        ElseIf wheels_number <= 2 And (iCat = 1 Or iCat = 2 Or iCat = 3) Then
    '            LEAStrActiveX.NLayers = 4
    '            Call RedimArraysFlexible(LEAStrActiveX.NLayers, iCat)

    '            LEAStrActiveX.Thick(1) = 3 'P-401
    '            LEAStrActiveX.Thick(2) = 6 'P-209
    '            LEAStrActiveX.Thick(3) = 16 'P-154 variable

    ''==============================================================
    '        ElseIf wheels_number >= 4 And iCat = 4 Then
    '            LEAStrActiveX.NLayers = 3
    '            Call RedimArraysFlexible(LEAStrActiveX.NLayers, iCat)

    '            LEAStrActiveX.Thick(1) = 5 'P-401
    '            LEAStrActiveX.Thick(2) = 4  'P-209 variable

    '        ElseIf wheels_number >= 4 And (iCat = 1 Or iCat = 2 Or iCat = 3) Then
    '            LEAStrActiveX.NLayers = 4
    '            Call RedimArraysFlexible(LEAStrActiveX.NLayers, iCat)

    '            LEAStrActiveX.Thick(1) = 5 'P-401
    '            LEAStrActiveX.Thick(2) = 8 'P-209 defined as User Defined
    '            LEAStrActiveX.Thick(3) = 20 'P-154 variable
    '        End If




    'WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    'WDir1 = WDir1 & "\FAARFIELD\ACR"

    'CalculateACN.libACN(1) = 123.2222
    'Exit Function

    'Function CalculateACR(ByRef CallAC() As ACNClassLib.clsLEAF.LEAFACParms, _
    '       ByRef LEAStrActiveX As ACNClassLib.clsLEAF.LEAFStrParms) As ACRdata

    'For Me.gSubModulus1 = 1000 To 35000 Step 1000
    'Dim iCat As Integer

    'If gSubModulus1 < 20000 Then
    '    iCat = 1
    'Else
    '    iCat = 4
    'End If



    '*********   FLEXIBLE   ***********
    'Call Set_Thick_Values(iCat) 'to comment
    'Call Calculate_Coverages_Flex()
    'CovGear = CSng(NtoFail) : StrainMaxGear = StrainMax : iMaxGear = iMax
    'Call Set_EvaluationPointsForAC2()
    'Call Print_Stress_Array_Full2(True)
    'Continue For

    'LEAStrActiveX.Thick(3) = 1
    'Call Calculate_Coverages_Flex()

    '*********   RIGID  ************
    'Call Evaluate_Stress_Rigid(LEAStrActiveX, CallAC(1)) 'Evaluate stress
    'LEAStrActiveX.Thick(1) = 2
    'Call Calculate_Stress_Rigid()
    'gStress = gStress
    'Call Eval_Stress_Thick()

    Friend Function AddTwoM(ByVal v1 As Single, ByVal v2 As Single) As Single
        AddTwoM = v1 + v2
    End Function

    Friend Sub AddTwoM2(ByRef v1 As Single, ByRef v2 As Single)
        v1 = v1 + v2
    End Sub
    'Public Sub Set_to_LEAF_Structure()
    '    'NewRigid, PCCOnFlex     'Call DesignRigid_NP() '3D-FEM stress   
    '    '                         Call pre_DesignRigid_NP()
    '    'UnbondOnRigid           'Call DesignRigidOverlay_NP()  '3D-FEM stress
    '    'FlexOnRigid             'Call DesignRigidOverlay_NP()  '3D-FEM stress
    '    Dim I As Integer

    '    If NPLayers <> LEAStructure.NLayers Then

    '        For I = 1 To LEAStructure.NLayers
    '            LEAStrActiveX.Thick(I) = LEAStructure.Thick(I)
    '            LEAStrActiveX.Modulus(I) = LEAStructure.Modulus(I)
    '            LEAStrActiveX.Poisson(I) = LEAStructure.Poisson(I)

    '            If LEAStructure.InterfaceCode(I) = 0 Then
    '                LEAStrActiveX.InterfaceParm(I) = 1
    '            ElseIf LEAStructure.InterfaceCode(I) >= 100000 Then
    '                LEAStrActiveX.InterfaceParm(I) = 0
    '            End If
    '        Next

    '        LEAStrActiveX.Poisson(LEAStructure.NLayers) = DefaultPoissonSGPCC
    '        LEAStrActiveX.NLayers = LEAStructure.NLayers

    '    End If

    'End Sub

End Module




