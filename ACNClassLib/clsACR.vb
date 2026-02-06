Option Strict On
Option Explicit On

Imports System
Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.FileIO
Imports VB = Microsoft.VisualBasic


Public Class clsACR

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub

    'Public gReduction1 As Single 'ver100
    'Public gReduction2 As Single 'ver100
    'Public bDSWL As Boolean      'ver100

    'Friend Const c1000 As Integer = 100

    Public Enum PavementType
        Flexible = 1
        Rigid = 2
    End Enum

    Friend gC_X(), gC_Y() As Double


    Friend bCalculate_DSWL As Boolean

    Dim Cat() As String = {"0", "D", "C", "B", "A"}
    Dim CatMpa() As String = {"0", "50", "80", "120", "200"}

    Public ICAOCodeIndex As Integer
    Friend i1, j1 As Integer
    Friend is1 As Single

    Dim gTandemFnew As Boolean
    Dim Damage As Double
    Dim ExtrType As Short
    Dim IEval, IEvaly As Integer
    Friend Const NNodesLong As Short = 1800 ' 100

    Dim ETimemsecs As Single

    Friend public_is1 As Single

    Friend Const MaxNPLayers As Short = 24 'ikawa was 16
    Friend Const MaxModulusNPLayers As Short = 32 ' Modification on 2/11/96

    Friend julModulus(MaxModulusNPLayers) As Double ' Modification on 2/11/96
    Friend julThick(MaxModulusNPLayers) As Double ' Modification on 2/11/96
    Friend julLCode(MaxModulusNPLayers) As Short ' Modification on 2/11/96

    Friend SBM(MaxNPLayers) As Double

    Friend leafPoissonsRatio(MaxModulusNPLayers) As Double ' Modification on 2012.12.21

    Friend Const CDFErrCntrl As Single = 0.69! ' Switch to multiple layers.

    Friend DesignType As Short
    Friend Const InvalidDesignType As Short = 0
    Friend Const NewFlex As Short = 1
    Friend Const FlexOnFlex As Short = 2
    Friend Const PCCOnFlex As Short = 3
    Friend Const NewRigid As Short = 10
    Friend Const UnbondOnRigid As Short = 11
    Friend Const PartBondOnRigid As Short = 12
    Friend Const FlexOnRigid As Short = 13
    Friend OverlayRigOnRig As Boolean


    Friend Const NAgSBase As String = "Ag Uncrushed"
    Friend Const NAgBase As String = "Ag Crushed"

    Dim S As String
    Friend LCode() As Short
    Friend LayerType() As String
    Dim Ret As Long
    Dim Modulus() As Double
    Dim Temp1, Temp2 As Double
    Dim Thick() As Double
    Dim EvalDepth() As Double

    Dim NEvalDepths As Integer
    Friend NL1 As String = Environment.NewLine 'PPPP
    Friend NL2 As String = NL1 & NL1 'PPPP

    Friend TSS_P154(99) As Double
    Friend TSS_P209(99) As Double
    Friend BaseMod(), SubbaseMod() As Double
    Friend Const Log10 As Single = 2.302585!
    Friend gNewModulus_P154, gNewModulus_P209 As Boolean
    Friend gP154_C688D156, gP209_C1052D20 As Boolean
    Dim NS_P209, publicNS_P209 As Integer
    Dim publicNS_P154, NS_P154 As Integer

    Dim CDFErr As Single
    Dim LayerSwitch As Boolean
    'Dim julThick(), julModulus(), julLCode() As Single
    Dim PoissonsRatio() As Double
    Dim julNPLayers As Short
    Dim AlternateSG As Boolean
    Dim IterlayerChosen As Short
    'Dim leafPoissonsRatio() As Single



    'LEDFAA_LEAF subroutine  
    '    Sub LEDFAA_to_LEAF(ByRef DesignType As Short)

    Dim CallAC(1) As ACRClassLib.clsLEAF.LEAFACParms
    Dim CopyAC(1) As ACRClassLib.clsLEAF.LEAFACParms
    Dim Copy2AC(1) As ACRClassLib.clsLEAF.LEAFACParms

    Dim C1(1) As ACRClassLib.clsLEAF.LEAFACParms

    'For second gear
    Dim CallAC_belly(1) As ACRClassLib.clsLEAF.LEAFACParms
    Dim CopyAC_belly(1) As ACRClassLib.clsLEAF.LEAFACParms
    Dim Copy2AC_belly(1) As ACRClassLib.clsLEAF.LEAFACParms

    Dim NAC, NextraAC As Integer
    Dim NtoFail As Double 'Flexible coverages
    Public gNtoFail As Double
    Public gStrain1 As Double

    Dim gTh_Iter1 As Integer

    Dim Flex_Total_Thick As Double

    'Dim horLimit, vertLimit As Integer

    'Const TopSubDepth As Double = 0.00001
    'Const TopSubDepth As Double = 0.00005
    'Const TopSubDepth As Double = 0.0001
    'Const TopSubDepth As Double = 0.0005
    'Const TopSubDepth As Double = 0.0025
    'Const TopSubDepth As Double = 0.005

    'Dim gSubModulus1 As Single

    Public Structure ACRdata
        Dim libACR() As Single
        Dim libACRthick() As Single
        Dim libSubCat() As String
        Dim libSubCatMPa() As String
    End Structure







    '2 gears output
    Private Overloads Function Z_CalculateACR_Output222(ByVal PavementType As clsACR.PavementType,
         ByVal gross_weight As Single,
         ByVal percent_gw As Single,
         ByVal wheels_number As Integer,
         ByVal tire_pressure As Single,
         ByVal CoordX() As Single,
         ByVal CoordY() As Single,
         ByVal percent_gw2 As Single,
         ByVal wheels_number2 As Integer,
         ByVal tire_pressure2 As Single,
         ByVal CoordX2() As Single,
         ByVal CoordY2() As Single) As ACRdata 'CalculateACR_Output




        gPrintOutput_ACN = True
        Z_CalculateACR_Output222 = CalculateACR(PavementType, gross_weight, percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                          percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)


    End Function

    '10
    Public Overloads Function CalculateACR(
             ByVal PavementType As clsACR.PavementType,
             ByVal gross_weight As Single,
             ByVal percent_gw As Single,
             ByVal wheels_number As Integer,
             ByVal tire_pressure As Single,
             ByVal CoordX() As Single,
             ByVal CoordY() As Single,
             ByVal modul1 As Single,
             ByVal SW() As Integer) As ACRdata

        Try

            bSelectWheels = True
            ReDim gSW(wheels_number)

            For i1 As Integer = 1 To wheels_number
                gSW(i1) = SW(i1)
            Next

            'ReDim CalculateACR.libACR(1)
            'ReDim CalculateACR.libACRthick(1)

            gICAOCodeIndex = ICAOCodeIndexF(modul1)
            start_cat = gICAOCodeIndex
            end_cat = start_cat

            CalculateACR = CalculateACR(PavementType, gross_weight,
                                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
            'CalculateACR.libACRthick(1) = CSng(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers))

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Function





    '05      1 gear 1 ACN  Monday 2017.06.12
    Public Overloads Function CalculateACR(ByVal PavementType As clsACR.PavementType,
                 ByVal gross_weight As Single,
                 ByVal percent_gw As Single,
                 ByVal wheels_number As Integer,
                 ByVal tire_pressure As Single,
                 ByVal CoordX() As Single,
                 ByVal CoordY() As Single,
                 ByVal modul1 As Single) As ACRdata




        Try

            'ReDim CalculateACR.libACR(1)
            'ReDim CalculateACR.libACRthick(1)

            gICAOCodeIndex = ICAOCodeIndexF(modul1)
            start_cat = gICAOCodeIndex
            end_cat = start_cat

            CalculateACR = CalculateACR(PavementType, gross_weight,
                                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
            'CalculateACR.libACRthick(1) = CSng(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers))

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Function

    '02
    Public Overloads Function CalculateACR(
        ByVal PavementType As clsACR.PavementType,
        ByVal gross_weight As Single,
        ByVal percent_gw As Single,
        ByVal wheels_number As Integer,
        ByVal tire_pressure As Single,
        ByVal CoordX() As Single,
        ByVal CoordY() As Single,
        ByVal Metric As Boolean) As ACRdata

        Try

            If Metric Then ' convert Metric input to US units
                gross_weight = CSng(gross_weight * 2204.6225) ' to tonnes
                tire_pressure = CSng(tire_pressure / 6.894757) ' to psi

                For ik As Integer = 1 To wheels_number
                    CoordX(ik) = CSng(CoordX(ik) / 25.4)
                    CoordY(ik) = CSng(CoordY(ik) / 25.4)
                Next

            End If


            CalculateACR = CalculateACR(PavementType, gross_weight,
                                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY)


            If Metric Then ' convert US units to Metric
                For ik As Integer = 1 To 4
                    CalculateACR.libACRthick(ik) = CSng(CalculateACR.libACRthick(ik) * 25.4)
                Next
            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try
    End Function




    '04
    Public Overloads Function CalculateACR(
           ByVal PavementType As clsACR.PavementType,
           ByVal gross_weight As Single,
           ByVal percent_gw As Single,
           ByVal wheels_number As Integer,
           ByVal tire_pressure As Single,
           ByVal CoordX() As Single,
           ByVal CoordY() As Single,
           ByVal SW() As Integer,
           ByVal Metric As Boolean) As ACRdata

        Try

            'PavementType = clsACR.PavementType.Flexible 'ik2020.03

            If Metric Then ' convert Metric input to US units
                gross_weight = CSng(gross_weight * 2204.6225) ' to tonnes
                tire_pressure = CSng(tire_pressure / 6.894757) ' to psi

                For ik As Integer = 1 To wheels_number
                    CoordX(ik) = CSng(CoordX(ik) * 25.4)
                    CoordY(ik) = CSng(CoordY(ik) * 25.4)
                Next

            End If


            CalculateACR = CalculateACR(PavementType, gross_weight,
                                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY, SW)


            If Metric Then ' convert US units to Metric
                For ik As Integer = 1 To 4
                    CalculateACR.libACRthick(ik) = CSng(CalculateACR.libACRthick(ik) * 25.4)
                Next
            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try
    End Function





    '03
    Public Overloads Function CalculateACR(ByVal PavementType As clsACR.PavementType,
             ByVal gross_weight As Single,
             ByVal percent_gw As Single,
             ByVal wheels_number As Integer,
             ByVal tire_pressure As Single,
             ByVal CoordX() As Single,
             ByVal CoordY() As Single,
             ByVal SW() As Integer) As ACRdata

        Try

            bSelectWheels = True
            ReDim gSW(wheels_number)

            For i1 As Integer = 1 To wheels_number
                gSW(i1) = SW(i1)
            Next

            CalculateACR = CalculateACR(PavementType, gross_weight,
                                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY)

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try
    End Function



    '01         1 Gear 4 ACNs
    Public Overloads Function CalculateACR(
                ByVal PavementType As clsACR.PavementType,
                ByVal gross_weight As Single,
                ByVal percent_gw As Single,
                ByVal wheels_number As Integer,
                ByVal tire_pressure As Single,
                ByVal CoordX() As Single,
                ByVal CoordY() As Single) As ACRdata


        Try

            gPrintOutput = True
            gPrintOutput_ACN = True
            gPrintOutput_Responses = True


            Dim path2 As String = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            path2 = path2 & "\FAARFIELD\"

            'FileOpen(33, "Alibaba.txt", OpenMode.Output)
            'Print(33, "Test")
            'FileClose(33)

            'Dim v1, v2 As Double


            'FileOpen(33, path2 & "000.txt", OpenMode.Input)
            'Input(33, gReduction1)
            'Input(33, gReduction2)
            'FileClose(33)

            'FileOpen(33, path2 & "0002.txt", OpenMode.Output)
            'PrintLine(33, gReduction1)
            'PrintLine(33, gReduction2)
            'FileClose(33)



            'Debug.Print(CStr(v1))
            'Debug.Print(CStr(v2))


            gTandemFnew = True 'CalculateACR Function
            gPavementType = PavementType

            gTimeSave1 = timeGetTime
            ReDim CalculateACR.libACR(end_cat - start_cat + 1)
            ReDim CalculateACR.libACRthick(end_cat - start_cat + 1)
            ReDim CalculateACR.libSubCat(end_cat - start_cat + 1)
            ReDim CalculateACR.libSubCatMPa(end_cat - start_cat + 1)

            If end_cat = start_cat Then
                ICAOCodeIndex = gICAOCodeIndex
            Else
                ICAOCodeIndex = 0
            End If

            Call Check_for_Subdirectory()

            If PavementType = clsACR.PavementType.Rigid Then
                GoTo RigidPavements
            End If

            '************************************************
            '*****          Flexible Pavements          *****
            '************************************************
            'start_cat = 4 'to comment
            For iCat As Integer = start_cat To end_cat

                gCat = iCat
                CallAC(1).GearLoad = gross_weight * percent_gw
                CallAC(1).NTires = wheels_number
                Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
                Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry

                CallAC(1).NEvalPoints = 0 : ReDim CallAC(1).EvalX(0) : ReDim CallAC(1).EvalY(0)
                If CallAC(1).NTires = 1 Then
                    CallAC(1).NEvalPoints = 1 : ReDim CallAC(1).EvalX(1) : ReDim CallAC(1).EvalY(1)
                    'CallAC(1).EvalX(1) = 0 : CallAC(1).EvalY(1) = 0 'ik2020.03
                    CallAC(1).EvalX(1) = CallAC(1).TireX(1) : CallAC(1).EvalY(1) = CallAC(1).TireY(1)


                ElseIf (iSelectEval_ACN = 1) Then 'Flex
                    Call Create_Eval_Points_Under_Wheels(CallAC)
                    Call Print_Evaluation_Points(CallAC)
                ElseIf (iSelectEval_ACN = 4) Then 'Flex
                    Call Center_of_Gravity_Eval(CallAC)
                Else
                    Call Create_Eval_Points(CallAC) 'Flexible 1 gear
                End If


                LEAStrActiveX.NLayers = 3
                Call RedimArraysFlexible(LEAStrActiveX.NLayers, iCat, LCode, LEAStrActiveX)


                Dim new_Wheels As Integer
                If bSelectWheels Then
                    new_Wheels = 0
                    For i1 As Integer = 1 To wheels_number
                        If gSW(i1) = 1 Then
                            new_Wheels = new_Wheels + 1
                        End If
                    Next
                Else
                    new_Wheels = wheels_number
                End If

                new_Wheels = CInt(wheels_number / 2)

                If new_Wheels <= 2 Then
                    'paragraph 1.1.3.8 a) Reference Pavement Structure 76 mm
                    'LEAStrActiveX.Thick(1) = 3 'P-401 
                    LEAStrActiveX.Thick(1) = 76 / 2.54 / 10
                    'LEAStrActiveX.Thick(1) = 2.992126 '76 mm
                    If iCat = 1 Then
                        LEAStrActiveX.Thick(2) = 27 'P-209 variable
                    Else
                        If gThickness3 = 0 Then gThickness3 = 23
                        LEAStrActiveX.Thick(2) = gThickness3 * 0.8
                        If LEAStrActiveX.Thick(2) < 1 Then LEAStrActiveX.Thick(2) = 1
                    End If
                Else 'wheels_number > 2
                    LEAStrActiveX.Thick(1) = 5 'P-401   127mm
                    LEAStrActiveX.Thick(1) = 127 / 2.54 / 10 'P-401   127mm
                    If iCat = 1 Then
                        LEAStrActiveX.Thick(2) = 30 'P-209 variable
                    Else
                        If LEAStrActiveX.Thick(2) = 0 Then gThickness3 = 20

                        'gThickness3 = 20 / 0.8 'commentACN
                        LEAStrActiveX.Thick(2) = gThickness3 * 0.8
                        If LEAStrActiveX.Thick(2) < 1 Then LEAStrActiveX.Thick(2) = 1
                    End If
                End If

                'LEAStrActiveX.Thick(2) = 16 'to comment
                'LEAStrActiveX.Modulus(julNPLayers) = 15000

                'LEAStrActiveX.Thick(2) = 13.13216 'to comment
                Call Make_CallAC_Copy(CallAC(1), CopyAC(1))

                'Call PrintOut01(LEAStrActiveX.NLayers - 1) 'to comment


                'Call AnalyzeModulus()

                'Exit Function

                'gT1 = timeGetTime
                'LEAStrActiveX.Thick(2) = 26.480116829272895
                'LEAStrActiveX.Thick(2) = 35.12


                'For t1 As Single = 2.5 To 0.1 Step -0.1
                '    LEAStrActiveX.Thick(2) = t1
                '    Call Calculate_Flex_Coverages()
                '    FileOpen(33, WDir1 & "\" & "44444.txt", OpenMode.Append)
                '    Print(33, LPad(11, Format(gross_weight, "#0.00")))

                '    'Print(33, LPad(11, Format(CovACN, "#0.0")) & "  ")
                '    'Print(33, LPad(15, Format(CovGear, "#0.0")) & "  ")
                '    'Print(33, LPad(15, Format(CovSWL, "#0.0")) & "  ")
                '    Print(33, LPad(11, Format(LEAStrActiveX.Thick(2), "#0.000")) & "  ")
                '    'Print(33, LPad(15, Format(gStrainTarget, "#0.00000000")) & "  ")
                '    Print(33, LPad(15, Format(gStrainMax, "#0.00000000")) & "  ")
                '    'Print(33, LPad(15, Format(StrainMaxSWL, "#0.00000000")) & "  ")
                '    PrintLine(33, LPad(13, Format(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers), "#0.000")))
                '    FileClose(33)
                'Next





                Call Calculate_Thickness_Flex(LEAStrActiveX.NLayers - 1) 'iterate on layer on subgrade
                'gT2 = timeGetTime


                If (iSelectEval_ACN <> 1) And False Then

                    gPrintOutput = True 'comment999
                    RunLEAF = New ACRClassLib.clsLEAF()
                    Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainsPrint, AllResp)
                    RunLEAF = Nothing

                    If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Strain
                        Call Print_Strain_Array_XYsym(False, LEAStrActiveX, StrainsPrint, CallAC)
                    ElseIf Symmetry = SymmetryType.YSymmetry Then
                        Call Print_Strain_Array_Ysym(False, LEAStrActiveX, StrainsPrint, CallAC)
                    Else
                        Call Print_Strain_Array_Full_Mesh(LEAStrActiveX)
                    End If


                    RunLEAF = New ACRClassLib.clsLEAF()
                    Call RunLEAF.ComputeResponse2(LeafResp, 1, CallAC, LEAStrActiveX, StrainsPrint, AllResp)
                    RunLEAF = Nothing

                    Call Print1800(StrainsPrint, Damage, StrainMax)
                    gPrintOutput = False

                End If


                Dim MinReached As Boolean = False
                Dim bMaxStrain As Boolean = False
                'If LEAStrActiveX.Thick(LEAStrActiveX.NLayers - 1) = gFlexMinThick Or True Then
                If LEAStrActiveX.Thick(LEAStrActiveX.NLayers - 1) = gFlexMinThick Then
                    'If (NtoFail > (CovACN + 10)) And (NtoFail > 1000000) Then
                    'If (NtoFail > (CovACN)) And False Or True Then
                    If (NtoFail > (CovACN)) Then

                        'If (NtoFail > (CovACN + 10)) And (NtoFail > 20000000000) Then
                        'If (NtoFail > (CovACN + 10)) Then

                        bMaxStrain = True
                        gStrainTarget = gStrainMax

                        'If UBound(CalculateACR.libACR, 1) = 1 Then
                        '    CalculateACR.libACR(1) = 0
                        '    CalculateACR.libACRthick(1) = 0
                        'Else
                        '    CalculateACR.libACR(iCat) = 0
                        '    CalculateACR.libACRthick(iCat) = 0
                        'End If

                        ''Call Print_Results_Flex(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                        'Continue For
                    Else
                        MinReached = True
                        CovACN = NtoFail
                    End If

                    'If NtoFail > CovACN Then
                    '    CovACN = CSng(NtoFail)
                    'End If

                End If


                'If bMeshSize Then
                '    gTandemFnew = False
                '    Call Calculate_Flex_Coverages()
                '    gPrintOutput = True

                '    If Symmetry = SymmetryType.XYSymmetry Then
                '        Call Print_Strain_Array_XYsym_Mesh(True, LEAStrActiveX, CallAC)
                '    ElseIf Symmetry = SymmetryType.YSymmetry Then
                '        Call Print_Strain_Array_Ysym_Mesh(True, LEAStrActiveX, CallAC)
                '    End If

                '    gTandemFnew = True
                '    gPrint1800 = True
                '    Call Calculate_Flex_Coverages()
                '    gPrint1800 = False
                'End If



                Call Set_SWLdata(CallAC)

                gT3 = timeGetTime
                'Call PrintOut01_GL() 'to comment

                If MinReached Then
                    Call Calculate_DSWL_Flex_NewMin()
                ElseIf bMaxStrain Then
                    Call Calculate_DSWL_Flex_Strain()
                Else
                    Call Calculate_DSWL_Flex()
                End If
                'Call Calculate_DSWL_Flex()


                gT4 = timeGetTime

                CalculateACR.libACR(iCat - start_cat + 1) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
                CalculateACR.libSubCat(iCat - start_cat + 1) = Cat(iCat)
                CalculateACR.libSubCatMPa(iCat - start_cat + 1) = CatMpa(iCat)

                Flex_Total_Thick = Flex_Thick_Func(LEAStrActiveX)
                CalculateACR.libACRthick(iCat - start_cat + 1) = CSng(Flex_Total_Thick)
                'Call Print_Results_Flex(gross_weight, CallAC, CopyAC, LEAStrActiveX)

            Next

            GoTo PrintACN

RigidPavements:

            '************************************************
            '*****             Rigid Pavements          *****
            '************************************************
            For iCat As Integer = start_cat To end_cat
                'bDSWL = False
                gCat = iCat
                CallAC(1).GearLoad = gross_weight * percent_gw
                CallAC(1).NTires = wheels_number
                Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
                Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry

                CallAC(1).NEvalPoints = 0 : ReDim CallAC(1).EvalX(0) : ReDim CallAC(1).EvalY(0)
                If CallAC(1).NTires = 1 Then
                    CallAC(1).NEvalPoints = 1 : ReDim CallAC(1).EvalX(1) : ReDim CallAC(1).EvalY(1)
                    'CallAC(1).EvalX(1) = 0 : CallAC(1).EvalY(1) = 0 'ik2020.03
                    CallAC(1).EvalX(1) = CallAC(1).TireX(1) : CallAC(1).EvalY(1) = CallAC(1).TireY(1)

                ElseIf (iSelectEval_ACN = 2) Or (iSelectEval_ACN = 3) Then 'Rigid
                    Call Create_Eval_Points(CallAC) 'Rigid 1 gear
                ElseIf (iSelectEval_ACN = 0) Or (iSelectEval_ACN = 1) Then
                    Call Create_Eval_Points_Under_Wheels(CallAC)
                    Call Print_Evaluation_Points(CallAC)
                ElseIf (iSelectEval_ACN = 4) Then 'Rigid
                    Call Center_of_Gravity_Eval(CallAC)
                End If

                If True Then
                    LEAStrActiveX.NLayers = 3
                    Call RedimArraysRigid(LEAStrActiveX.NLayers, iCat, LCode, LEAStrActiveX)
                    LEAStrActiveX.Thick(1) = 5 'P-501
                    'paragraph 1.1.3.7 Rigid Pavement  (base 200 mm)
                    LEAStrActiveX.Thick(2) = 20 / 2.54 'P-209  (200 mm to inches)

                ElseIf wheels_number >= 4 And (iCat = 1 Or iCat = 2 Or iCat = 3) Then
                End If

                Call Make_CallAC_Copy(CallAC(1), CopyAC(1))
                Call Calculate_Thickness_Rigid()

                If LEAStrActiveX.Thick(1) = 2 Then ' 2 inches = 50.8 mm
                    If (gStress < (StressACN)) And (gStress < 1) Then

                        If UBound(CalculateACR.libACR, 1) = 1 Then
                            CalculateACR.libACR(1) = 0
                            CalculateACR.libACRthick(1) = 0
                        Else
                            CalculateACR.libACR(iCat) = 0
                            CalculateACR.libACRthick(iCat) = 0
                        End If
                        'Call Print_Results_Rigid(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                        Continue For
                    Else
                        StressACN = gStress
                    End If
                End If

                Call Set_SWLdata(CallAC)
                'bDSWL = True
                Call Calculate_DSWL_Rigid()

                'AliA
                If iCat = 1 Then
                    gRatio1 = CSng(CallAC(1).GearLoad / CopyAC(1).GearLoad)
                ElseIf iCat = 2 Then
                    gRatio2 = CSng(CallAC(1).GearLoad / CopyAC(1).GearLoad)
                ElseIf iCat = 3 Then
                    gRatio3 = CSng(CallAC(1).GearLoad / CopyAC(1).GearLoad)
                ElseIf iCat = 4 Then
                    gRatio4 = CSng(CallAC(1).GearLoad / CopyAC(1).GearLoad)
                End If

                CalculateACR.libACR(iCat - start_cat + 1) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
                CalculateACR.libSubCat(iCat - start_cat + 1) = Cat(iCat)
                CalculateACR.libSubCatMPa(iCat - start_cat + 1) = CatMpa(iCat)

                CalculateACR.libACRthick(iCat - start_cat + 1) = CSng(LEAStrActiveX.Thick(1))
                'Call Print_Results_Rigid(gross_weight, CallAC, CopyAC, LEAStrActiveX)

            Next

PrintACN:
            'Call Print_ACNtoFile(CopyAC, CalculateACR)

            Call ResetValues()


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try





    End Function

    Private Sub Calculate_DSWL_Flex_NewMin()

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        If CovACN = 36500 Then
            CoverageDelta = 1
        Else
            CoverageDelta = NtoFail / 36500
        End If



        bCalculate_DSWL = True
        Call Calculate_Flex_Coverages()

        If Math.Abs(NtoFail - CovACN) > CoverageDelta Then
            If NtoFail < CovACN Then
                Do
                    tt2 = CallAC(1).GearLoad : SS2 = NtoFail : SS2 = StrainMax

                    If NtoFail < 100 * CoverageDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.7
                    ElseIf NtoFail < 1000 * CoverageDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.8
                    ElseIf NtoFail < 5000 * CoverageDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.9
                    Else
                        CallAC(1).GearLoad = CallAC(1).GearLoad - 100
                    End If

                    If CallAC(1).GearLoad <= 100 Then
                        CallAC(1).GearLoad = 100
                        If SS1 = SS2 Then GoTo finish1
                    End If

                    Call Calculate_Flex_Coverages()
                    tt1 = CallAC(1).GearLoad : SS1 = NtoFail : SS1 = StrainMax
                    If CallAC(1).GearLoad = 100 Then Exit Do
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail > CovACN)
            Else
                Do
                    tt1 = CallAC(1).GearLoad : SS1 = NtoFail : SS1 = StrainMax
                    CallAC(1).GearLoad = CallAC(1).GearLoad + 5000
                    Call Calculate_Flex_Coverages()
                    tt2 = CallAC(1).GearLoad : SS2 = NtoFail : SS2 = StrainMax
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail < CovACN)
            End If
        Else
            GoTo finish1
        End If

        If Math.Abs(NtoFail - CovACN) < CoverageDelta Then
            GoTo finish1
        End If


        Dim zz1, zz2, aa, bb As Double

rep1:

        CallAC(1).GearLoad = (tt1 + tt2) / 2
        Call Calculate_Flex_Coverages()


        If NtoFail > CovACN Then
            tt1 = CallAC(1).GearLoad
        Else
            tt2 = CallAC(1).GearLoad
        End If

        If Math.Abs(NtoFail - CovACN) > CoverageDelta Then
            GoTo rep1
        End If

finish1:

        CovSWL = NtoFail
        StrainMaxSWL = StrainMax


    End Sub




    Private Sub Calculate_DSWL_Flex_Strain()
        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        gStrainDelta = gStrainTarget * 0.003
        bCalculate_DSWL = True
        Call Calculate_Flex_Coverages()

        If Math.Abs(gStrainMax - gStrainTarget) > gStrainDelta Then
            If gStrainMax > gStrainTarget Then
                Do
                    tt2 = CallAC(1).GearLoad : SS2 = gStrainMax

                    If gStrainMax < 100 * gStrainDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.7
                    ElseIf gStrainMax < 1000 * gStrainDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.8
                    ElseIf gStrainMax < 5000 * gStrainDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.9
                    Else
                        'CallAC(1).GearLoad = CallAC(1).GearLoad - 2000

                        If CallAC(1).GearLoad > 5000 Then
                            CallAC(1).GearLoad = CallAC(1).GearLoad - 2000
                        Else
                            CallAC(1).GearLoad = CallAC(1).GearLoad * 0.95
                        End If

                    End If

                    If CallAC(1).GearLoad <= 10 Then
                        CallAC(1).GearLoad = 10
                        If SS1 = SS2 Then GoTo finish1
                    End If

                    Call Calculate_Flex_Coverages()
                    tt1 = CallAC(1).GearLoad : SS1 = gStrainMax
                    If CallAC(1).GearLoad = 10 Then Exit Do
                Loop Until Math.Abs(gStrainMax - gStrainTarget) < gStrainDelta Or (gStrainMax < gStrainTarget)
            Else
                Do
                    tt1 = CallAC(1).GearLoad : SS1 = gStrainMax
                    CallAC(1).GearLoad = CallAC(1).GearLoad + 5000
                    Call Calculate_Flex_Coverages()
                    tt2 = CallAC(1).GearLoad : SS2 = gStrainMax
                Loop Until Math.Abs(gStrainMax - gStrainTarget) < gStrainDelta Or (gStrainMax > gStrainTarget)
            End If
        Else
            GoTo finish1
        End If


        If Math.Abs(gStrainMax - gStrainTarget) < gStrainDelta Then
            GoTo finish1
        End If



        Dim zz1, zz2, aa, bb As Double

rep1:

        aa = (tt2 - tt1) / (SS2 - SS1)
        bb = tt1 - aa * SS1

        Dim ttACN As Double
        ttACN = aa * (gStrainTarget) + bb
        If ttACN < gFlexMinThick Then
            ttACN = gFlexMinThick
        End If

        CallAC(1).GearLoad = ttACN
        Call Calculate_Flex_Coverages()


        If gStrainMax > gStrainTarget Then
            tt2 = CallAC(1).GearLoad
            SS2 = gStrainMax
        Else
            tt1 = CallAC(1).GearLoad
            SS1 = gStrainMax
        End If

        If Math.Abs(gStrainMax - gStrainTarget) > gStrainDelta Then
            GoTo rep1
        End If

finish1:

        CovSWL = NtoFail
        StrainMaxSWL = gStrainMax



    End Sub



    Private Sub Calculate_DSWL_Flex()
        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        If CovACN = 36500 Then
            CoverageDelta = 1
        Else
            CoverageDelta = NtoFail / 36500
        End If


        bCalculate_DSWL = True
        Call Calculate_Flex_Coverages()

        If Math.Abs(NtoFail - CovACN) > CoverageDelta Then
            If NtoFail < CovACN Then
                Do
                    tt2 = CallAC(1).GearLoad : SS2 = NtoFail : SS2 = StrainMax

                    If NtoFail < 100 * CoverageDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.7
                    ElseIf NtoFail < 1000 * CoverageDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.8
                    ElseIf NtoFail < 5000 * CoverageDelta Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.9
                    Else
                        If CallAC(1).GearLoad > 5000 Then
                            CallAC(1).GearLoad = CallAC(1).GearLoad - 2000
                        Else
                            CallAC(1).GearLoad = CallAC(1).GearLoad * 0.95
                        End If

                    End If

                    If CallAC(1).GearLoad <= 100 Then
                        CallAC(1).GearLoad = 100
                        If SS1 = SS2 Then GoTo finish1
                    End If

                    Call Calculate_Flex_Coverages()
                    tt1 = CallAC(1).GearLoad : SS1 = NtoFail : SS1 = StrainMax
                    If CallAC(1).GearLoad = 100 Then Exit Do
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail > CovACN)
            Else
                Do
                    tt1 = CallAC(1).GearLoad : SS1 = NtoFail : SS1 = StrainMax
                    CallAC(1).GearLoad = CallAC(1).GearLoad + 5000
                    Call Calculate_Flex_Coverages()
                    tt2 = CallAC(1).GearLoad : SS2 = NtoFail : SS2 = StrainMax
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail < CovACN)
            End If
        Else
            GoTo finish1
        End If

        If Math.Abs(NtoFail - CovACN) < CoverageDelta Then
            GoTo finish1
        End If


        'Dim FileName As String
        'FileName = WDir1 & "\Strain_DSWL_Fail_" & FileExt

        'FileOpen(9, FileName, OpenMode.Output)
        'For i = tt1 To tt2 Step 200
        '    CallAC(1).GearLoad = i
        '    Call Calculate_Coverages_Flex()
        '    PrintLine(9, Format(CallAC(1).GearLoad, "#,##0.000") & "     " & NtoFail)
        'Next

        'FileClose(9)
        '================================================================================



        'CallAC(1).GearLoad = (tt1 + tt2) / 2
        'Call Calculate_Coverages_Flex()

        Dim zz1, zz2, aa, bb As Double

rep1:

        'zz1 = Math.Log(SS1)
        'zz2 = Math.Log(SS2)


        'aa = (tt1 - tt2) / (zz1 - zz2)
        aa = (tt2 - tt1) / (SS2 - SS1)
        bb = tt1 - aa * SS1

        Dim ttACN As Double

        'ttACN = aa * Math.Log(StressACN) + bb
        ttACN = aa * (StrainACN) + bb
        If ttACN <= 0 Then
            ttACN = 1
        End If

        CallAC(1).GearLoad = ttACN
        Call Calculate_Flex_Coverages()


        If StrainMax < StrainACN Then
            tt2 = CallAC(1).GearLoad
            SS2 = StrainMax
        Else
            tt1 = CallAC(1).GearLoad
            SS1 = StrainMax
        End If

        If Math.Abs(NtoFail - CovACN) > CoverageDelta Then
            GoTo rep1
        End If

finish1:

        CovSWL = NtoFail
        StrainMaxSWL = StrainMax



    End Sub


    Private Sub Calculate_DSWL_Rigid()

        'AliA
        'StressDelta = StressACN / 1000
        StressDelta = StressACN / 2000
        StressDelta = StressACN / 3000

        If StressDelta < 0.03 Then
            StressDelta = 0.03
        End If

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Call Calculate_Rigid_Stress()

        If Math.Abs(gStress - StressACN) > StressDelta Then
            If gStress > StressACN Then
                Do
                    tt2 = CallAC(1).GearLoad : SS2 = gStress

                    If CallAC(1).GearLoad > 35000 Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad - 2000
                    ElseIf CallAC(1).GearLoad > 5000 Then
                        CallAC(1).GearLoad = CallAC(1).GearLoad - 1000
                    Else
                        CallAC(1).GearLoad = CallAC(1).GearLoad * 0.9
                    End If
                    'CallAC(1).GearLoad = CallAC(1).GearLoad - 2000

                    If CallAC(1).GearLoad <= 10 Then CallAC(1).GearLoad = 10

                    Call Calculate_Rigid_Stress()
                    tt1 = CallAC(1).GearLoad : SS1 = gStress
                    'If CallAC(1).GearLoad = 100 Then Exit Do

                    If CallAC(1).GearLoad <= 10 Then
                        CallAC(1).GearLoad = 10
                        If SS1 = SS2 Then GoTo finish1
                    End If


                Loop Until (gStress < StressACN) Or Math.Abs(gStress - StressACN) < StressDelta
            Else
                Do
                    tt1 = CallAC(1).GearLoad : SS1 = gStress
                    CallAC(1).GearLoad = CallAC(1).GearLoad + 5000
                    Call Calculate_Rigid_Stress()
                    tt2 = CallAC(1).GearLoad : SS2 = gStress
                Loop Until (gStress > StressACN) Or Math.Abs(gStress - StressACN) < StressDelta
            End If
        Else
            GoTo finish1
        End If

        If Math.Abs(gStress - StressACN) < StressDelta Then
            GoTo finish1
        End If


        CallAC(1).GearLoad = (tt1 + tt2) / 2
        Call Calculate_Rigid_Stress()
        Dim zz1, zz2, aa, bb As Double

rep1:

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)

        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1

        Dim ttACN As Double

        ttACN = aa * Math.Log(StressACN) + bb

        If ttACN <= 0 Then
            ttACN = 1
        End If


        CallAC(1).GearLoad = ttACN
        Call Calculate_Rigid_Stress()

        If gStress < StressACN Then
            tt2 = CallAC(1).GearLoad
            SS2 = gStress
        Else
            tt1 = CallAC(1).GearLoad
            SS1 = gStress
        End If

        If Math.Abs(gStress - StressACN) > StressDelta Then
            GoTo rep1
        End If

finish1:

        StressSWL = gStress

        Exit Sub

        'Dim FileName As String
        'FileName = WDir1 & "\PCC_DSWL_" & FileExt

        'FileOpen(9, FileName, OpenMode.Output)
        'For i = tt1 To tt2 Step 200
        '    CallAC(1).GearLoad = i
        '    Call Calculate_Stress_Rigid()
        '    PrintLine(9, Format(CallAC(1).GearLoad, "#,##0.000") & "     " & gStress)
        'Next

        'FileClose(9)


        Dim FileName As String
        FileName = WDir1 & "\PCC_Conv_" & FileExt
        FileOpen(9, FileName, OpenMode.Append)

        PrintLine(9, Format(gStress, "#0.0000000") & "   " & Format(CallAC(1).GearLoad, "#,##0.000") & "   " & "    " & tt1 & "    " & tt2)
        Dim count1 As Integer
StartA1:
        count1 = count1 + 1
        If gStress > StressACN Then
            tt2 = CallAC(1).GearLoad
            CallAC(1).GearLoad = (tt1 + tt2) / 2
        Else
            tt1 = CallAC(1).GearLoad
            CallAC(1).GearLoad = (tt1 + tt2) / 2
        End If
        Call Calculate_Rigid_Stress()
        PrintLine(9, Format(gStress, "#0.0000000") & "   " & Format(CallAC(1).GearLoad, "#,##0.000") & "   " & "    " & tt1 & "    " & tt2)

        If Math.Abs(gStress - StressACN) > StressDelta Then
            GoTo StartA1
        End If

        PrintLine(9, "")
        FileClose(9)


    End Sub


    Private Sub Calculate_Thickness_Rigid()

        'Dim FileName As String = "StressGraph.txt"
        'FileOpen(9, FileName, OpenMode.Append)

        'LEAStrActiveX.Thick(1) = i
        'Print(9, LPad(12, "Thick"))
        'Print(9, LPad(12, "Modul"))
        'Print(9, LPad(12, "Stress"))
        'PrintLine(9, "")
        'FileClose(9)

        'LEAStrActiveX.Thick(1) = 12

        'Dim iMod As Single

        'For iMod = 1000 To 50000 Step 1000
        '    LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) = iMod
        '    Call Calculate_Rigid_Stress()
        '    FileOpen(9, FileName, OpenMode.Append)
        '    Print(9, LPad(12, Format(LEAStrActiveX.Thick(1), "#,##0.000")))
        '    Print(9, LPad(12, Format(LEAStrActiveX.Modulus(LEAStrActiveX.NLayers), "#,##0")))
        '    Print(9, LPad(12, Format(gStress, "#,##0.000")))

        '    PrintLine(9, "")
        '    FileClose(9)
        'Next





        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Call Calculate_Rigid_Stress()

        If Math.Abs(gStress - StressACN) > StressDelta Then
            If gStress < StressACN Then
                Do
                    tt2 = LEAStrActiveX.Thick(1) : SS2 = gStress
                    LEAStrActiveX.Thick(1) = LEAStrActiveX.Thick(1) - 2
                    If LEAStrActiveX.Thick(1) <= 2 Then LEAStrActiveX.Thick(1) = 2
                    Call Calculate_Rigid_Stress()
                    tt1 = LEAStrActiveX.Thick(1) : SS1 = gStress
                    If LEAStrActiveX.Thick(1) = 2 Then Exit Do
                Loop Until (gStress > StressACN) Or Math.Abs(gStress - StressACN) < StressDelta
            Else
                Do
                    tt1 = LEAStrActiveX.Thick(1) : SS1 = gStress
                    LEAStrActiveX.Thick(1) = LEAStrActiveX.Thick(1) + 4
                    Call Calculate_Rigid_Stress()
                    tt2 = LEAStrActiveX.Thick(1) : SS2 = gStress
                Loop Until (gStress < StressACN) Or Math.Abs(gStress - StressACN) < StressDelta
            End If
        Else
            GoTo finish1
        End If

        If Math.Abs(gStress - StressACN) < StressDelta Then
            GoTo finish1
        End If


        'Print(9, Format(tt1, "#,##0.00000") & "   " & Format(SS1, "#,##0.00000") & "     ")
        'PrintLine(9, Format(tt2, "#,##0.00000") & "   " & Format(SS2, "#,##0.00000"))

        If LEAStrActiveX.Thick(1) <= 2 Then
            If StressACN > gStress Then
                GoTo finish1
            End If
        End If


        'FileOpen(9, FileName, OpenMode.Append)
        'For i = tt1 To tt2 + 0.00001 Step 0.1
        '    LEAStrActiveX.Thick(1) = i
        '    Call Calculate_Stress_Rigid()
        '    PrintLine(9, Format(LEAStrActiveX.Thick(1), "#,##0.000") & "   " & gStress)
        'Next
        'FileClose(9)

        LEAStrActiveX.Thick(1) = (tt1 + tt2) / 2
        Call Calculate_Rigid_Stress()

        Dim zz1, zz2, aa, bb As Double

rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)

        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1

        Dim ttACN As Double

        ttACN = aa * Math.Log(StressACN) + bb
        LEAStrActiveX.Thick(1) = ttACN
        Call Calculate_Rigid_Stress()


        If gStress < StressACN Then
            tt2 = LEAStrActiveX.Thick(1)
            SS2 = gStress
        Else
            tt1 = LEAStrActiveX.Thick(1)
            SS1 = gStress
        End If

        'Print(9, Format(tt1, "#,##0.00000") & "   " & Format(SS1, "#,##0.00000") & "     ")
        'Print(9, Format(tt2, "#,##0.00000") & "   " & Format(SS2, "#,##0.00000") & "     ")
        'PrintLine(9, Format(Math.Abs(gStress - StressACN), "#,##0.00000"))

        If Math.Abs(gStress - StressACN) > StressDelta Then
            GoTo rep1
        End If

finish1:

        StressGear = gStress

        'PrintLine(9, Format(LEAStrActiveX.Thick(1), "#,##0.00000") & "   " & Format(gStress, "#,##0.00000"))
        'PrintLine(9, "")
        'FileClose(9)
        Exit Sub


        Dim FileName1 As String
        FileName1 = WDir1 & "\PCC_Conv_" & FileExt
        FileOpen(9, FileName1, OpenMode.Append)

        PrintLine(9, Format(gStress, "#0.0000000") & "   " & Format(LEAStrActiveX.Thick(1), "#,##0.000") _
                  & "   " & "    " & tt1 & "    " & tt2)
        Dim count1 As Integer
StartA1:
        count1 = count1 + 1
        If gStress > StressACN Then
            tt1 = LEAStrActiveX.Thick(1)
            LEAStrActiveX.Thick(1) = (tt1 + tt2) / 2
        Else
            tt2 = LEAStrActiveX.Thick(1)
            LEAStrActiveX.Thick(1) = (tt1 + tt2) / 2
        End If
        Call Calculate_Rigid_Stress()
        PrintLine(9, Format(gStress, "#0.0000000") & "   " & Format(LEAStrActiveX.Thick(1), "#,##0.000") _
                  & "   " & "    " & tt1 & "    " & tt2)

        If Math.Abs(StressACN - gStress) < StressDelta Then
        Else
            GoTo StartA1
        End If

        PrintLine(9, "")
        FileClose(9)

    End Sub



    Private Sub ZZZ_Cal3_Stress_Rigid_Array_Eval_Points()

        ReDim RRR(1, CallAC(1).NEvalPoints)
        CallAC(1).NEvalPoints = 1
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 1 To CopyAC(1).NEvalPoints Step 1

            CallAC(1).EvalX(1) = CopyAC(1).EvalX(j1)
            CallAC(1).EvalY(1) = CopyAC(1).EvalY(j1)

            Response = Nothing
            LEAStrActiveX.EvalDepth = LEAStrActiveX.Thick(1)
            LEAStrActiveX.EvalLayer = 1
            LeafResp = ACRClassLib.clsLEAF.LEAFoptions.HorizontalStress

            RunLEAF = New ACRClassLib.clsLEAF()
            Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, Response, AllResp)
            RunLEAF = Nothing

            If Response Is Nothing Then
                gStress = 0
            Else
                gStress = Response(1, 1)
            End If

            RRR(1, j1) = CSng(gStress)
        Next




    End Sub


    Private Sub ZZZ_Cal3_Stress_Rigid_Array_A380()

        ReDim RRR(horE_Limit, iNorth - iSouth)
        CallAC(1).NEvalPoints = 1
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)


        For j1 = 0 To iNorth - iSouth Step 1
            For i1 = 0 To horE_Limit Step 1

                CallAC(1).EvalX(1) = i1 * horStep + horMiddle
                CallAC(1).EvalY(1) = j1 * vertStep + South

                Response = Nothing
                LEAStrActiveX.EvalDepth = LEAStrActiveX.Thick(1)
                LEAStrActiveX.EvalLayer = 1
                LeafResp = ACRClassLib.clsLEAF.LEAFoptions.HorizontalStress

                RunLEAF = New ACRClassLib.clsLEAF()
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, Response, AllResp)
                RunLEAF = Nothing

                If Response Is Nothing Then
                    gStress = 0
                Else
                    gStress = Response(1, 1)
                End If

                RRR(i1, j1) = CSng(gStress)
            Next
        Next




    End Sub



    Friend Sub Calculate_Rigid_Stress()

        Response = Nothing
        LEAStrActiveX.EvalDepth = LEAStrActiveX.Thick(1)

        'LEAStrActiveX.EvalDepth = LEAStrActiveX.Thick(1) + LEAStrActiveX.Thick(2) + 0.001
        LEAStrActiveX.EvalLayer = 1
        'LEAStrActiveX.EvalLayer = 3

        LeafResp = ACRClassLib.clsLEAF.LEAFoptions.HorizontalStress
        'LeafResp = ACNClassLib.clsLEAF.LEAFoptions.VerticalStrain
        'LeafResp = ACNClassLib.clsLEAF.LEAFoptions.AllResponses

        RunLEAF = New ACRClassLib.clsLEAF()

        gComputeResponseAsym = False 'rigid

        If gComputeResponseAsym Then
            Call RunLEAF.ComputeResponseAsym(LeafResp, 1, CallAC,
                LEAStrActiveX, Response, AllResp)
        Else
            Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC,
                LEAStrActiveX, Response, AllResp)
        End If
        RunLEAF = Nothing

        If Response Is Nothing Then
            gStress = 0
        Else

            'For i = 1 To UBound(Response, 2)
            '    If bDSWL Then
            '        Response(1, i) = Response(1, i) * gReduction2
            '    Else
            '        Response(1, i) = Response(1, i) * gReduction1
            '    End If
            '    'Response(1, i) = Response(1, i) * 0.95 'Calculate_Rigid_Stress
            'Next

            gStress = Response(1, 1)
            'gStress = AllResp(1, 1).StressX
        End If

        For i = 1 To UBound(Response, 2)
            If gStress < Response(1, i) Then
                gStress = Response(1, i)
            End If
        Next

        'For i = 1 To UBound(AllResp, 2)
        'Response(1, i) = AllResp(1, i).StressX
        'Response(1, i) = AllResp(1, i).StressX
        'Next


        'If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Strain Print
        '    Call Print_Array_XYsym(True, LEAStrActiveX, Response, CallAC)
        'ElseIf Symmetry = SymmetryType.YSymmetry Then
        '    'Call Print_Strain_Array_XYsym(True, LEAStrActiveX)
        'Else
        '    Call Print_Array_Full_Mesh(LEAStrActiveX, Response, CallAC)
        'End If




    End Sub





    Private Sub ZZZ_Cal3_Thickness__Flex2(ByVal iTh As Integer)

        Call Calculate_Flex_Coverages()

        Call ZZZ_Cal3(iTh, 5000, 5)
        Call ZZZ_Cal3(iTh, 500, 0.5)
        Call ZZZ_Cal3(iTh, 5000, 5)
        Call ZZZ_Cal3(iTh, 5000, 5)

    End Sub

    Private Sub ZZZ_Cal3(ByVal iTh As Integer, ByVal Thr1cov As Double, ByVal thr2th As Double)


        If Math.Abs(CovACN - NtoFail) > Thr1cov Then
            If CovACN > NtoFail Then
                Do
                    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) + thr2th
                    Call Calculate_Flex_Coverages()
                Loop While Math.Abs(CovACN - NtoFail) > Thr1cov And (CovACN > NtoFail)
            Else
                Do
                    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) - thr2th
                    If LEAStrActiveX.Thick(iTh) <= 1 Then LEAStrActiveX.Thick(iTh) = 1
                    Call Calculate_Flex_Coverages()
                    If LEAStrActiveX.Thick(iTh) = 1 Then Exit Do
                Loop While Math.Abs(CovACN - NtoFail) > Thr1cov And (CovACN < NtoFail)
            End If
        End If

        If LEAStrActiveX.Thick(iTh) = 1 Then
            If NtoFail > (CovACN + 10) Then
                Exit Sub
            End If
        End If


    End Sub

    'FF

    Private Sub Calculate_Thickness_Flex(ByVal iTh As Integer)
        Dim Fail1, Fail2 As Double
        Dim tt1, tt2 As Double
        Dim SS1, SS2 As Double

        bCalculate_DSWL = False 'Calculate_Thickness_Flex

        '===============================================
        'Dim thick01 As Single
        'Dim gFF1 As Integer = FreeFile()
        'Dim gFileN9 As String = "CCCCCC03"


        'FileOpen(gFF1, WDir1 & "\" & gFileN9 & ".txt", OpenMode.Append)
        'Print(gFF1, LPad(7, "Thick"))
        'Print(gFF1, LPad(7, "Thick2"))
        'Print(gFF1, LPad(17, "Strain"))
        'Print(gFF1, LPad(22, "Cover"))

        'PrintLine(gFF1, "")
        'FileClose(gFF1)

        'LEAStrActiveX.Thick(iTh) = 25 - 0.1

        'For thick01 = 1 To 11 Step 1
        '    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) + 0.1
        '    Call Calculate_Flex_Coverages()

        '    FileOpen(gFF1, WDir1 & "\" & gFileN9 & ".txt", OpenMode.Append)
        '    Print(gFF1, LPad(7, Format(LEAStrActiveX.EvalDepth - LEAStrActiveX.Thick(1), "#0.00")))
        '    Print(gFF1, LPad(7, Format(gThickness3, "#0.00")))
        '    Print(gFF1, LPad(17, Format(gStrain1, "#0.000000000000")))
        '    Print(gFF1, LPad(22, Format(NtoFail, "#,##0.00")))
        '    PrintLine(gFF1, "")
        '    FileClose(gFF1)

        'Next

        '===============================================

        gT5 = timeGetTime
        Call Calculate_Flex_Coverages()
        gT6 = timeGetTime

        If Math.Abs(NtoFail - CovACN) > CoverageDelta Then
            If NtoFail < CovACN Then
                Do
                    tt1 = LEAStrActiveX.Thick(iTh) : tt1 = GetThick(iTh)
                    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) + 4
                    tt2 = LEAStrActiveX.Thick(iTh) : tt2 = tt1 + 4
                    Fail1 = NtoFail : SS1 = StrainMax
                    Call Calculate_Flex_Coverages()
                    Fail2 = NtoFail : SS2 = StrainMax
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail > CovACN)
            Else
                Do
                    tt2 = LEAStrActiveX.Thick(iTh) : tt2 = GetThick(iTh)
                    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) - 2
                    tt1 = LEAStrActiveX.Thick(iTh) : tt1 = tt2 - 2
                    If LEAStrActiveX.Thick(iTh) <= gFlexMinThick Then
                        LEAStrActiveX.Thick(iTh) = gFlexMinThick
                        tt1 = gFlexMinThick
                    End If
                    Fail2 = NtoFail : SS2 = StrainMax
                    Call Calculate_Flex_Coverages()
                    Fail1 = NtoFail : SS1 = StrainMax
                    If tt1 = gFlexMinThick Then Exit Do
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail < CovACN)
            End If
        Else
            GoTo finish1
        End If

        If Math.Abs(NtoFail - CovACN) < CoverageDelta Then
            GoTo finish1
        End If


        'If tt1 <= 1 Then
        If LEAStrActiveX.Thick(iTh) <= gFlexMinThick Then
            'If StrainACN > StrainMax Then
            If NtoFail > CovACN Then
                GoTo finish1
            End If
        End If

        Dim aa, bb As Double
        Dim zz1, zz2 As Double

        Dim count_11 As Integer
        count_11 = 0


rep1:

        zz1 = Math.Log(Fail1)
        zz2 = Math.Log(Fail2)

        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * Math.Log(Fail1)

        Dim ttACN As Double

        ttACN = aa * Math.Log(CovACN) + bb
        If ttACN <= 0 Then
            ttACN = gFlexMinThick
            GoTo finish2
        End If

        ReDim Preserve LEAStrActiveX.Thick(iTh + 1)
        LEAStrActiveX.NLayers = 3

        LEAStrActiveX.Thick(iTh) = ttACN
        Call Calculate_Flex_Coverages()

        If StrainMax < StrainACN Then
            tt2 = GetThick(iTh)
            SS2 = StrainMax
            Fail2 = NtoFail
        Else
            tt1 = GetThick(iTh)
            SS1 = StrainMax
            Fail1 = NtoFail
        End If

        If Math.Abs(NtoFail - CovACN) > CoverageDelta And count_11 < MaxIter Then
            count_11 = count_11 + 1
            GoTo rep1
        End If

finish1:  'Calculate_Thickness_Flex

        CovGear = NtoFail
        StrainMaxGear = StrainMax

        Exit Sub

finish2:


        'Do
        '    tt1 = LEAStrActiveX.Thick(iTh) : tt1 = GetThick(iTh)
        '    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) + 0.05
        '    tt2 = LEAStrActiveX.Thick(iTh) : tt2 = tt1 + 0.05
        '    Fail1 = NtoFail : SS1 = StrainMax
        '    Call Calculate_Flex_Coverages()
        '    Fail2 = NtoFail : SS2 = StrainMax
        'Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail > CovACN)

finish3:

        ttACN = (tt1 + tt2) / 2
        ReDim Preserve LEAStrActiveX.Thick(iTh + 1)
        LEAStrActiveX.NLayers = 3

        LEAStrActiveX.Thick(iTh) = ttACN
        Call Calculate_Flex_Coverages()

        If NtoFail < CovACN Then
            tt2 = GetThick(iTh)
            SS2 = StrainMax
            Fail2 = NtoFail
        Else
            tt1 = GetThick(iTh)
            SS1 = StrainMax
            Fail1 = NtoFail
        End If

        If Math.Abs(NtoFail - CovACN) > CoverageDelta And count_11 < MaxIter Then
            count_11 = count_11 + 1
            GoTo finish3
        End If

        GoTo finish1



    End Sub







    Private Sub Calculate_Thickness_Flex_2gears(ByVal iTh As Integer)
        Dim Fail1, Fail2 As Double
        Dim tt1, tt2 As Double
        Dim SS1, SS2 As Double

        bCalculate_DSWL = False 'Calculate_Thickness_Flex_2gears
        Call Calculate_Flex_Coverages_2gears()

        If Math.Abs(NtoFail - CovACN) > CoverageDelta Then
            If NtoFail < CovACN Then
                Do
                    tt1 = LEAStrActiveX.Thick(iTh) : tt1 = GetThick(iTh)
                    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) + 4
                    tt2 = LEAStrActiveX.Thick(iTh) : tt2 = tt1 + 4
                    Fail1 = NtoFail : SS1 = StrainMax
                    Call Calculate_Flex_Coverages_2gears()
                    Fail2 = NtoFail : SS2 = StrainMax
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail > CovACN)
            Else
                Do
                    tt2 = LEAStrActiveX.Thick(iTh) : tt2 = GetThick(iTh)
                    LEAStrActiveX.Thick(iTh) = LEAStrActiveX.Thick(iTh) - 2
                    tt1 = LEAStrActiveX.Thick(iTh) : tt1 = tt2 - 2
                    If LEAStrActiveX.Thick(iTh) <= 1 Then
                        LEAStrActiveX.Thick(iTh) = 1
                        tt1 = 1
                    End If
                    SS2 = StrainMax
                    Fail2 = NtoFail
                    Call Calculate_Flex_Coverages_2gears()
                    Fail1 = NtoFail
                    SS1 = StrainMax
                    If tt1 = 1 Then Exit Do
                Loop Until Math.Abs(NtoFail - CovACN) < CoverageDelta Or (NtoFail < CovACN)
            End If
        Else
            GoTo finish1
        End If

        If Math.Abs(NtoFail - CovACN) < CoverageDelta Then
            GoTo finish1
        End If

        'If tt1 <= 1 Then
        If LEAStrActiveX.Thick(iTh) <= 1 Then
            'If StrainACN > StrainMax Then
            If NtoFail > CovACN Then
                GoTo finish1
            End If
        End If


        Dim aa, bb As Double
        Dim zz1, zz2 As Double

rep1:

        zz1 = Math.Log(Fail1)
        zz2 = Math.Log(Fail2)


        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * Math.Log(Fail1)

        Dim ttACN As Double

        ttACN = aa * Math.Log(CovACN) + bb


        ReDim Preserve LEAStrActiveX.Thick(iTh + 1)
        LEAStrActiveX.NLayers = 3


        LEAStrActiveX.Thick(iTh) = ttACN
        Call Calculate_Flex_Coverages_2gears()


        If StrainMax < StrainACN Then
            tt2 = GetThick(iTh)
            SS2 = StrainMax
            Fail2 = NtoFail
        Else
            tt1 = GetThick(iTh)
            SS1 = StrainMax
            Fail1 = NtoFail
        End If

        If Math.Abs(NtoFail - CovACN) > CoverageDelta Then
            GoTo rep1
        End If

finish1:

        CovGear = NtoFail
        StrainMaxGear = StrainMax


    End Sub


    Friend Sub Calculate_Flex_Coverages()

        Try

            'Call RedimArraysFlexible(iTh_Iter + 1, gCat) 'X123
            Call RedimArraysFlexible(LEAStrActiveX.NLayers, gCat, LCode, LEAStrActiveX) 'X123


            'If LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) < 20000 Then
            Call FAAModulus_P154()
            'End If

            LEAStrActiveX.EvalDepth = 0
            For i = 1 To LEAStrActiveX.NLayers - 1
                LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth + LEAStrActiveX.Thick(i)
            Next
            'LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth * 1.0001
            'LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth + TopSubDepth

            'gThickness3 = LEAStrActiveX.EvalDepth - LEAStrActiveX.Thick(1) - LEAStrActiveX.Thick(2)
            gThickness3 = LEAStrActiveX.EvalDepth - LEAStrActiveX.Thick(1)
            LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth + 0.001

            LEAStrActiveX.EvalLayer = LEAStrActiveX.NLayers
            LeafResp = ACRClassLib.clsLEAF.LEAFoptions.VerticalStrain

            If gTandemFnew Then gPrintOutput = False

            Dim StrainResponse(,) As Double 'ikawa

            If Not gTandemFnew Then

                RunLEAF = New ACRClassLib.clsLEAF()
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC,
                    LEAStrActiveX, Response, AllResp)
                RunLEAF = Nothing

                StrainMax = 0 : iMax = 0
                For i = 1 To UBound(Response, 2)
                    If StrainMax < -Response(1, i) Then
                        StrainMax = CSng(-Response(1, i))
                        iMax = i
                    End If
                Next


                Dim a11, b11, c11 As Single

                'Bleasdale 2
                a11 = -0.163768916705
                b11 = 185.192806802
                c11 = 1.65054449461

                If StrainMax < 0.001 Then '1 Gear
                    StrainMax = 0.001
                End If

                'Bleasdale 2
                If StrainMax <= 0.001765093 Then
                    NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
                Else
                    NtoFail = (0.00414131183 / StrainMax) ^ 8.1
                End If


            Else 'new tandem factor
                'Dim StrainResponse(,) As Double


                'Call Check999(LEAStrActiveX) 'commentACN
                'gTandemFnew = False
                'gPrintOutput = True


                'check superposition
                'Call Set_OneWheelData(CallAC)
                'CallAC(1).NEvalPoints = 20
                'ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
                'ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)
                'Dim dx1, dy1, dist11 As Double
                'Dim tP As Integer
                'tP = 1313

                'For ii1 As Integer = 1 To 20
                '    dx1 = CopyAC(1).TireX(ii1) - CopyAC(1).EvalX(tP)
                '    dy1 = CopyAC(1).TireY(ii1) - CopyAC(1).EvalY(tP)
                '    dist11 = CSng(Math.Sqrt(dx1 * dx1 + dy1 * dy1))
                '    CallAC(1).EvalX(ii1) = dist11
                '    CallAC(1).EvalY(ii1) = 0
                'Next

                'tP = tP


                'check

                'CallAC(1).NEvalPoints = 1
                'ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
                'ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

                'CallAC(1).EvalX(1) = 129.92126083374
                'CallAC(1).EvalY(1) = 67.31884765625

                'CallAC(1).EvalX(1) = 133.858268737792
                'CallAC(1).EvalY(1) = 67.31884765625

                'RunLEAF = New ACRClassLib.clsLEAF()
                'Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
                'RunLEAF = Nothing
                'gPrintOutput = True
                ''Call Print_Strain_Array_Ysym(False, LEAStrActiveX, StrainResponse)
                'Call Print_Strain_Array_XYsym(True, LEAStrActiveX, StrainResponse, CallAC)

                '=============================================================


                'bCalculate_DSWL = False
                If bCalculate_DSWL Then 'Calculate_Flex_Coverages
                    GoTo old1
                End If
                'GoTo old1

                Dim str1() As Double
                'Call Create_Matrix(str1) 

                RunLEAF = New ACRClassLib.clsLEAF()
                Call Set_OneWheelData(CallAC)
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
                RunLEAF = Nothing

                Call Make_CallAC_Copy(CallAC(1), Copy2AC(1))


                Dim ArrayDim As Integer
                ArrayDim = UBound(CallAC(1).EvalX, 1)
                Dim CopyStr(ArrayDim - 1) As Double
                Dim CopyStr2(ArrayDim) As Double

                For i1 = 1 To ArrayDim
                    CopyStr(i1 - 1) = StrainResponse(1, i1)
                    CopyStr2(i1) = StrainResponse(1, i1)
                Next


                'create evaluation mesh
                'add strains for wheels
                Dim dim1 As Integer, dist1, dist2 As Double
                dim1 = UBound(CopyAC(1).EvalX, 1)
                Dim StrainMesh() As Double
                ReDim StrainMesh(dim1)
                Dim dx, dy As Double, res1 As Double

                Dim maxX As Double = -1
                Dim maxX2 As Double = -1

                For eval1 As Integer = 1 To CopyAC(1).NEvalPoints
                    StrainMesh(eval1) = 0

                    For ii1 As Integer = 1 To CopyAC(1).NTires
                        dx = CopyAC(1).TireX(ii1) - CopyAC(1).EvalX(eval1)
                        dy = CopyAC(1).TireY(ii1) - CopyAC(1).EvalY(eval1)

                        dist1 = CSng(Math.Sqrt(dx * dx + dy * dy))
                        If maxX < dist1 Then
                            maxX = dist1
                        End If

                        'dist1 = 3.1
                        'res1 = Get_Strain2(dist1, CopyStr)
                        'res1 = Get_Strain7(dist1, StrainResponse, CallAC)

                        'res1 = Get_Strain8(dist1, CopyStr2, CallAC) '1 gear
                        res1 = Get_Strain2(dist1, CopyStr, CallAC) '1 gear


                        StrainMesh(eval1) = StrainMesh(eval1) + res1
                    Next
                Next

                'temporary to comment
                'dist1 = dist1
                'ReDim StrainResponse(1, UBound(StrainMesh, 1))
                'For i1 = 1 To UBound(StrainMesh, 1)
                '    StrainResponse(1, i1) = StrainMesh(i1)
                'Next
                'gPrintOutput = True
                'Call Print_Strain_Array_Ysym(False, LEAStrActiveX, StrainResponse)


                'Locate max strain
                Dim zStrainMax As Double
                Dim StrainInd As Integer
                Dim xStrain, yStrain As Double
                zStrainMax = 1.0E+35

                For ii1 As Integer = 1 To CopyAC(1).NEvalPoints
                    If zStrainMax > StrainMesh(ii1) Then
                        zStrainMax = StrainMesh(ii1)
                        StrainInd = ii1
                    End If
                Next

                xStrain = CopyAC(1).EvalX(StrainInd)
                yStrain = CopyAC(1).EvalY(StrainInd)


                Call Make_CallAC_Copy(CopyAC(1), CallAC(1))



                CallAC(1).NEvalPoints = NNodesLong
                ReDim CallAC(1).EvalX(NNodesLong)
                ReDim CallAC(1).EvalY(NNodesLong)

                For IEvaly = 1 To NNodesLong
                    CallAC(1).EvalX(IEvaly) = xStrain
                    CallAC(1).EvalY(IEvaly) = South - 160 + IEvaly * (North - South + 320) / NNodesLong
                Next IEvaly

                'RunLEAF = New ACRClassLib.clsLEAF()
                'Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
                'RunLEAF = Nothing


                ReDim StrainResponse(1, 1800)
                For IEvaly = 1 To NNodesLong
                    For ii1 As Integer = 1 To CopyAC(1).NTires
                        dx = CopyAC(1).TireX(ii1) - CallAC(1).EvalX(IEvaly)
                        dy = CopyAC(1).TireY(ii1) - CallAC(1).EvalY(IEvaly)

                        dist1 = CSng(Math.Sqrt(dx * dx + dy * dy))
                        If maxX2 < dist1 Then
                            maxX2 = dist1
                        End If
                        'res1 = Get_Strain2(dist1, CopyStr)
                        'res1 = Get_Strain7(dist1, StrainResponse, CallAC)

                        res1 = Get_Strain8(dist1, CopyStr2, Copy2AC) '1 gear
                        'res1 = Get_Strain2(dist1, CopyStr, Copy2AC) '1 gear

                        StrainResponse(1, IEvaly) = StrainResponse(1, IEvaly) + res1
                    Next

                Next IEvaly


                GoTo new1



old1:

                If bCalculate_DSWL Then

                    Dim StrainResponseD(,) As Double
                    CallAC(1).NEvalPoints = CShort(NNodesLong / 2) + 1
                    ReDim CallAC(1).EvalX(NNodesLong)
                    ReDim CallAC(1).EvalY(NNodesLong)

                    For IEvaly = 1 To CallAC(1).NEvalPoints
                        CallAC(1).EvalX(IEvaly) = 0
                        CallAC(1).EvalY(IEvaly) = 0 + (IEvaly - 1) * (320) / NNodesLong
                    Next IEvaly

                    RunLEAF = New ACRClassLib.clsLEAF()
                    Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponseD, AllResp)

                    ReDim StrainResponse(1, NNodesLong)
                    For i3 As Integer = 1 To 901
                        StrainResponse(1, 899 + i3) = StrainResponseD(1, i3)
                    Next

                    Dim temp1 As Integer
                    For i3 As Integer = 1 To 899
                        temp1 = 1801 - i3
                        StrainResponse(1, i3) = StrainResponse(1, 1800 - i3)
                    Next
                    RunLEAF = Nothing

                Else
                    RunLEAF = New ACRClassLib.clsLEAF()
                    If gTandemFnew Then ' kairat replace tandem
                        Call RunLEAF.ComputeResponse2(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
                    Else
                        Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
                        'Call Print_Strain_Array_XYsym(False, LEAStrActiveX, StrainResponse, CallAC) 'commentACN
                    End If
                    RunLEAF = Nothing
                End If
new1:

                Dim IA As Integer = 1
                Dim Overflow As Boolean
                Dim publicStrain As Double


                Damage = 0.0!

                For Me.IEvaly = 2 To NNodesLong - 1S
                    If StrainResponse(IA, IEvaly) < 0 Then
                        ''MAX strain
                        If StrainResponse(IA, IEvaly - 1) < StrainResponse(IA, IEvaly) And
                            StrainResponse(IA, IEvaly) > StrainResponse(IA, IEvaly + 1) Then
                            StrainMax = System.Math.Abs(StrainResponse(IA, IEvaly))
                            ExtrType = 1
                            GoTo FAILURELAW
                        End If

                        'MIN strain
                        If StrainResponse(IA, IEvaly - 1) > StrainResponse(IA, IEvaly) And
                            StrainResponse(IA, IEvaly) < StrainResponse(IA, IEvaly + 1) Then
                            StrainMax = System.Math.Abs(StrainResponse(IA, IEvaly))
                            ExtrType = 2
FAILURELAW:
                            'HHHHHHHHHHHHHHH
                            gStrainMax = StrainMax
                            'If StrainMax < 0.0001 Then StrainMax = 0.0001 Else Overflow = False '1 Gear
                            '       Standard subgrade criterion.

                            'Bleasdale Model (log10 on cov)
                            Dim a11, b11, c11 As Single
                            'Bleasdale 2
                            a11 = -0.163768916705
                            b11 = 185.192806802
                            c11 = 1.65054449461

                            If StrainMax < 0.001 Then '1 Gear
                                StrainMax = 0.001
                            End If

                            If StrainMax <= 0.001765093 Then
                                NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
                            Else
                                NtoFail = (0.00414131183 / StrainMax) ^ 8.1
                            End If

                            'FFFFFFFFFFFFFFF
                            Damage = Damage + (-1) ^ ExtrType * 1.0 / NtoFail
                        End If
                    End If
                Next IEvaly

                NtoFail = CSng(1.0 / Damage)
                gNtoFail = NtoFail
                publicStrain = CSng(StrainMax)
                gStrain1 = StrainMax

            End If

            'gPrint1800 = True 'commentACN
            If gPrint1800 Then
                Call Print1800(StrainResponse, Damage, StrainMax)
            End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub




    Friend Sub Calculate_Flex_Coverages_2gears()

        'Call RedimArraysFlexible(iTh_Iter + 1, gCat) 'X123
        Call RedimArraysFlexible(LEAStrActiveX.NLayers, gCat, LCode, LEAStrActiveX) 'X123


        'If LEAStrActiveX.Modulus(LEAStrActiveX.NLayers) < 20000 Then
        Call FAAModulus_P154()
        'End If

        LEAStrActiveX.EvalDepth = 0
        For i = 1 To LEAStrActiveX.NLayers - 1
            LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth + LEAStrActiveX.Thick(i)
        Next
        'LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth * 1.0001
        'LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth + TopSubDepth

        'gThickness3 = LEAStrActiveX.EvalDepth - LEAStrActiveX.Thick(1) - LEAStrActiveX.Thick(2)
        gThickness3 = LEAStrActiveX.EvalDepth - LEAStrActiveX.Thick(1)
        LEAStrActiveX.EvalDepth = LEAStrActiveX.EvalDepth + 0.001

        LEAStrActiveX.EvalLayer = LEAStrActiveX.NLayers
        LeafResp = ACRClassLib.clsLEAF.LEAFoptions.VerticalStrain

        If gTandemFnew Then gPrintOutput = False



        If Not gTandemFnew Then

            RunLEAF = New ACRClassLib.clsLEAF()

            If gComputeResponseAsym Then
                Call RunLEAF.ComputeResponseAsym(LeafResp, 1, CallAC,
                    LEAStrActiveX, Response, AllResp)
            Else
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC,
                    LEAStrActiveX, Response, AllResp)
            End If
            RunLEAF = Nothing

            StrainMax = 0 : iMax = 0
            For i = 1 To UBound(Response, 2)
                If StrainMax < -Response(1, i) Then
                    StrainMax = CSng(-Response(1, i))
                    iMax = i
                End If
            Next


            Dim a11, b11, c11 As Single

            'Bleasdale 2
            a11 = -0.163768916705
            b11 = 185.192806802
            c11 = 1.65054449461

            If StrainMax < 0.001 Then '2 gears
                StrainMax = 0.001
            End If

            'Bleasdale 2
            If StrainMax <= 0.001765093 Then
                NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
            Else
                NtoFail = (0.00414131183 / StrainMax) ^ 8.1
            End If


        Else 'new tandem factor

            Dim StrainResponse(,) As Double
            Dim Strain_Belly(,) As Double
            '=======================================================================================
            If bCalculate_DSWL Then 'Calculate_Flex_Coverages_2gears
                GoTo old2
            End If
            'GoTo old2

            Dim str1() As Double
            Dim AllTires As Integer
            AllTires = UBound(gC_X, 1)

            RunLEAF = New ACRClassLib.clsLEAF()
            Call Set_OneWheelData(CallAC)
            Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
            RunLEAF = Nothing
            Call Make_CallAC_Copy(CallAC(1), Copy2AC(1))

            'For belly
            RunLEAF = New ACRClassLib.clsLEAF()
            Call Set_OneWheelData(CallAC_belly)
            Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC_belly, LEAStrActiveX, Strain_Belly, AllResp)
            RunLEAF = Nothing
            Call Make_CallAC_Copy(CallAC_belly(1), Copy2AC_belly(1))
            '====================================================================

            Dim ArrayDim As Integer
            ArrayDim = UBound(CallAC(1).EvalX, 1)
            Dim CopyStr(ArrayDim - 1) As Double
            Dim CopyStr2(ArrayDim) As Double

            For i1 = 1 To ArrayDim
                CopyStr(i1 - 1) = StrainResponse(1, i1)
                CopyStr2(i1) = StrainResponse(1, i1)
            Next

            'For belly
            Dim ArrayDim_belly As Integer
            ArrayDim_belly = UBound(CallAC_belly(1).EvalX, 1)
            Dim CopyStr_belly(ArrayDim - 1) As Double
            Dim CopyStr2_belly(ArrayDim) As Double

            For i1 = 1 To ArrayDim
                CopyStr_belly(i1 - 1) = Strain_Belly(1, i1)
                CopyStr2_belly(i1) = Strain_Belly(1, i1)
            Next
            '====================================================================


            'create evaluation mesh
            'add strains for wheels
            Dim dim22 As Integer, dist1, dist2 As Double
            dim22 = UBound(CopyAC(1).EvalX, 1)
            Dim StrainMesh() As Double
            Dim StrainMesh_Belly() As Double

            ReDim StrainMesh(dim22)
            Dim dx, dy As Double, res1 As Double

            Dim maxX As Double = -1
            Dim maxX2 As Double = -1

            Dim Stemp1(1, CopyAC(1).NEvalPoints) As Double

            For eval1 As Integer = 1 To CopyAC(1).NEvalPoints
                StrainMesh(eval1) = 0

                For ii1 As Integer = 1 To CopyAC(1).NTires
                    dx = CopyAC(1).TireX(ii1) - CopyAC(1).EvalX(eval1)
                    dy = CopyAC(1).TireY(ii1) - CopyAC(1).EvalY(eval1)

                    dist1 = CSng(Math.Sqrt(dx * dx + dy * dy))
                    If maxX < dist1 Then
                        maxX = dist1
                    End If
                    'res1 = Get_Strain8(dist1, CopyStr2, CallAC) '2 gears
                    'res1 = Get_Strain2(dist1, CopyStr, CallAC) '2 gears
                    res1 = Get_Strain99(dist1, CopyStr, CallAC) '2 gears

                    StrainMesh(eval1) = StrainMesh(eval1) + res1
                    Stemp1(1, eval1) = Stemp1(1, eval1) + res1
                Next
            Next

            gPrintOutput = True
            'Call Print_Strain_Array_Ysym(False, LEAStrActiveX, Stemp1, CopyAC)

            'for belly
            ReDim Stemp1(1, CopyAC(1).NEvalPoints)
            For eval1 As Integer = 1 To CopyAC_belly(1).NEvalPoints

                For ii1 As Integer = 1 To CopyAC_belly(1).NTires
                    dx = CopyAC_belly(1).TireX(ii1) - CopyAC(1).EvalX(eval1)
                    dy = CopyAC_belly(1).TireY(ii1) - CopyAC(1).EvalY(eval1)

                    dist1 = CSng(Math.Sqrt(dx * dx + dy * dy))
                    If maxX < dist1 Then
                        maxX = dist1
                    End If
                    'If IEval = 527 Then
                    '    IEval = IEval
                    'End If
                    'res1 = Get_Strain8(dist1, CopyStr2, CallAC) '2 gears
                    'res1 = Get_Strain2(dist1, CopyStr_belly, CallAC_belly) '2 gears
                    res1 = Get_Strain99(dist1, CopyStr_belly, CallAC_belly) '2 gears
                    StrainMesh(eval1) = StrainMesh(eval1) + res1
                    Stemp1(1, eval1) = Stemp1(1, eval1) + res1
                Next
            Next
            'Call Print_Strain_Array_Ysym(False, LEAStrActiveX, Stemp1, CopyAC)
            '====================================================================

            'temporary to comment
            'dist1 = dist1
            'ReDim StrainResponse(1, UBound(StrainMesh, 1))
            'For i1 = 1 To UBound(StrainMesh, 1)
            '    StrainResponse(1, i1) = StrainMesh(i1)
            'Next
            'gPrintOutput = True
            'Call Print_Strain_Array_Ysym(False, LEAStrActiveX, StrainResponse)


            'Locate max strain
            Dim zStrainMax As Double
            Dim StrainInd As Integer
            Dim xStrain, yStrain As Double
            zStrainMax = 1.0E+35

            For ii1 As Integer = 1 To CopyAC(1).NEvalPoints
                If zStrainMax > StrainMesh(ii1) Then
                    zStrainMax = StrainMesh(ii1)
                    StrainInd = ii1
                End If
            Next

            xStrain = CopyAC(1).EvalX(StrainInd)
            yStrain = CopyAC(1).EvalY(StrainInd)

            Call Make_CallAC_Copy(CopyAC(1), CallAC(1))

            CallAC(1).NEvalPoints = NNodesLong
            ReDim CallAC(1).EvalX(NNodesLong)
            ReDim CallAC(1).EvalY(NNodesLong)

            For IEvaly = 1 To NNodesLong
                CallAC(1).EvalX(IEvaly) = xStrain
                CallAC(1).EvalY(IEvaly) = South - 160 + IEvaly * (North - South + 320) / NNodesLong
            Next IEvaly

            Dim keepDist1 As Double

            'for belly
            Call Make_CallAC_Copy(CopyAC_belly(1), CallAC_belly(1))
            CallAC_belly(1).NEvalPoints = NNodesLong
            ReDim CallAC_belly(1).EvalX(NNodesLong)
            ReDim CallAC_belly(1).EvalY(NNodesLong)

            For IEvaly = 1 To NNodesLong
                CallAC_belly(1).EvalX(IEvaly) = xStrain
                CallAC_belly(1).EvalY(IEvaly) = South - 160 + IEvaly * (North - South + 320) / NNodesLong
            Next IEvaly
            '====================================================================

            'RunLEAF = New ACRClassLib.clsLEAF()
            'Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
            'RunLEAF = Nothing


            ReDim StrainResponse(1, 1800)
            For IEvaly = 1 To NNodesLong
                For ii1 As Integer = 1 To CopyAC(1).NTires
                    dx = CopyAC(1).TireX(ii1) - CallAC(1).EvalX(IEvaly)
                    dy = CopyAC(1).TireY(ii1) - CallAC(1).EvalY(IEvaly)

                    dist1 = CSng(Math.Sqrt(dx * dx + dy * dy))
                    If maxX2 < dist1 Then
                        maxX2 = dist1
                    End If
                    'res1 = Get_Strain8(dist1, CopyStr2, Copy2AC) '2 gears 1800
                    'res1 = Get_Strain2(dist1, CopyStr, Copy2AC) '2 gears 1800
                    res1 = Get_Strain99(dist1, CopyStr, Copy2AC) '2 gears 1800
                    StrainResponse(1, IEvaly) = StrainResponse(1, IEvaly) + res1
                Next
            Next IEvaly

            'for belly
            Dim rBelly(1, 1800) As Double
            For IEvaly = 1 To NNodesLong
                For ii1 As Integer = 1 To CopyAC_belly(1).NTires
                    dx = CopyAC_belly(1).TireX(ii1) - CallAC_belly(1).EvalX(IEvaly)
                    dy = CopyAC_belly(1).TireY(ii1) - CallAC_belly(1).EvalY(IEvaly)

                    dist1 = CSng(Math.Sqrt(dx * dx + dy * dy))
                    If maxX2 < dist1 Then
                        maxX2 = dist1
                    End If

                    If IEvaly = 527 Then
                        IEvaly = IEvaly
                        keepDist1 = dist1
                    End If
                    'res1 = Get_Strain8(dist1, CopyStr2, Copy2AC) '2 gears 1800
                    'res1 = Get_Strain2(dist1, CopyStr_belly, Copy2AC_belly) '2 gears 1800
                    res1 = Get_Strain99(dist1, CopyStr_belly, Copy2AC_belly) '2 gears 1800
                    'res1 = Get_Strain2(dist1, CopyStr, Copy2AC)
                    StrainResponse(1, IEvaly) = StrainResponse(1, IEvaly) + res1
                    rBelly(1, IEvaly) = rBelly(1, IEvaly) + res1
                Next
            Next IEvaly
            '====================================================================

            'Copy2AC_belly(1).NEvalPoints = 1
            'ReDim Copy2AC_belly(1).EvalX(Copy2AC_belly(1).NEvalPoints)
            'ReDim Copy2AC_belly(1).EvalY(Copy2AC_belly(1).NEvalPoints)
            'Copy2AC_belly(1).EvalX(1) = keepDist1
            'Copy2AC_belly(1).EvalY(1) = 0


            'RunLEAF = New ACRClassLib.clsLEAF()
            'Call RunLEAF.ComputeResponse(LeafResp, 1, Copy2AC_belly, LEAStrActiveX, StrainResponse, AllResp)
            'RunLEAF = Nothing



            GoTo new2


            'If bCalculate_DSWL Then

            '    Dim StrainResponseD(,) As Double
            '    CallAC(1).NEvalPoints = CShort(NNodesLong / 2) + 1
            '    ReDim CallAC(1).EvalX(NNodesLong)
            '    ReDim CallAC(1).EvalY(NNodesLong)

            '    For IEvaly = 1 To CallAC(1).NEvalPoints
            '        CallAC(1).EvalX(IEvaly) = 0
            '        CallAC(1).EvalY(IEvaly) = 0 + (IEvaly - 1) * (320) / NNodesLong
            '    Next IEvaly

            '    RunLEAF = New ACRClassLib.clsLEAF()
            '    Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponseD, AllResp)

            '    ReDim StrainResponse(1, NNodesLong)
            '    For i3 As Integer = 1 To 901
            '        StrainResponse(1, 899 + i3) = StrainResponseD(1, i3)
            '    Next

            '    Dim temp1 As Integer
            '    For i3 As Integer = 1 To 899
            '        temp1 = 1801 - i3
            '        StrainResponse(1, i3) = StrainResponse(1, 1800 - i3)
            '    Next
            '    RunLEAF = Nothing

            '    GoTo new2
            'End If



            '========================================================================================

old2:
            'gPrintOutput_Responses = True 'FF 2017.07.14
            Dim Ymin, Ymax As Double

            Ymin = 10000
            Ymax = -10000

            For i = 1 To CallAC(1).NTires
                If Ymin > CallAC(1).TireY(i) Then
                    Ymin = CallAC(1).TireY(i)
                End If
                If Ymax < CallAC(1).TireY(i) Then
                    Ymax = CallAC(1).TireY(i)
                End If
            Next

            For i = 1 To CallAC_belly(1).NTires
                If Ymin > CallAC_belly(1).TireY(i) Then
                    Ymin = CallAC_belly(1).TireY(i)
                End If
                If Ymax < CallAC_belly(1).TireY(i) Then
                    Ymax = CallAC_belly(1).TireY(i)
                End If
            Next


            'GoTo sss1
            Dim StrainZ1(), StrainZ2(), StrainZ() As Double
            Dim Dim1 As Integer

            RunLEAF = New ACRClassLib.clsLEAF()

            If gComputeResponseAsym Then
                Call RunLEAF.ComputeResponseAsym(LeafResp, 1, CallAC, LEAStrActiveX, Response, AllResp)
            Else
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, Response, AllResp)
            End If
            RunLEAF = Nothing
            'Call Print_Strain_Array_Full_Mesh(LEAStrActiveX) 'Print ******
            'gPrintOutput = True
            'Call Print_Strain_Array_Ysym(False, LEAStrActiveX, Response, CallAC)

            Dim1 = UBound(Response, 2)
            ReDim StrainZ1(Dim1), StrainZ2(Dim1), StrainZ(Dim1)

            For i = 1 To Dim1
                StrainZ1(i) = Response(1, i)
            Next

            RunLEAF = New ACRClassLib.clsLEAF()

            If gComputeResponseAsym Then
                Call RunLEAF.ComputeResponseAsym(LeafResp, 1, CallAC_belly, LEAStrActiveX, Response, AllResp)
            Else
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC_belly, LEAStrActiveX, Response, AllResp)
            End If
            RunLEAF = Nothing
            'Call Print_Strain_Array_Full_Mesh(LEAStrActiveX) 'Print ******
            'Call Print_Strain_Array_Ysym(False, LEAStrActiveX, Response, CallAC)

            For i = 1 To Dim1
                StrainZ2(i) = Response(1, i)
            Next

            For i = 1 To Dim1
                StrainZ(i) = StrainZ1(i) + StrainZ2(i)
            Next

            Dim min1 As Double, x1, y1 As Double, ind1 As Integer
            min1 = 10000


            For i = 1 To Dim1
                If min1 > StrainZ(i) Then
                    min1 = StrainZ(i)
                    ind1 = i
                End If
            Next


            x1 = CallAC(1).EvalX(ind1)
            y1 = CallAC(1).EvalY(ind1)

            CallAC(1).NEvalPoints = 1
            ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)

            CallAC(1).EvalX(1) = x1
            CallAC(1).EvalY(1) = y1


            CallAC_belly(1).NEvalPoints = 1
            ReDim CallAC_belly(1).EvalX(CallAC_belly(1).NEvalPoints)
            ReDim CallAC_belly(1).EvalY(CallAC_belly(1).NEvalPoints)

            CallAC_belly(1).EvalX(1) = x1
            CallAC_belly(1).EvalY(1) = y1

sss1:

            Dim StrainResponse1(,) As Double
            RunLEAF = New ACRClassLib.clsLEAF()
            If gTandemFnew Then ' kairat replace tandem
                'Call RunLEAF.ComputeResponse2(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse1, AllResp)
                Call RunLEAF.ComputeResponse3(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse1, AllResp, Ymin, Ymax)
            Else
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
            End If
            RunLEAF = Nothing


            Dim StrainResponse2(,) As Double
            RunLEAF = New ACRClassLib.clsLEAF()
            If gTandemFnew Then ' kairat replace tandem
                'Call RunLEAF.ComputeResponse2(LeafResp, 1, CallAC_belly, LEAStrActiveX, StrainResponse2, AllResp)
                Call RunLEAF.ComputeResponse3(LeafResp, 1, CallAC_belly, LEAStrActiveX, StrainResponse2, AllResp, Ymin, Ymax)
            Else
                Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC_belly, LEAStrActiveX, StrainResponse, AllResp)
            End If
            RunLEAF = Nothing


            Dim dim2 As Integer
            dim2 = UBound(StrainResponse1, 2)

            ReDim StrainResponse(1, dim2)
            For i = 1 To dim2
                StrainResponse(1, i) = StrainResponse1(1, i) + StrainResponse2(1, i)

            Next

            ' 111======================================================================
            ' 111======================================================================
            xStrain = x1
            yStrain = y1

            CallAC(1).NEvalPoints = NNodesLong
            ReDim CallAC(1).EvalX(NNodesLong)
            ReDim CallAC(1).EvalY(NNodesLong)

            For IEvaly = 1 To NNodesLong
                CallAC(1).EvalX(IEvaly) = xStrain
                CallAC(1).EvalY(IEvaly) = South - 160 + IEvaly * (North - South + 320) / NNodesLong
            Next IEvaly

            Dim check1(,) As Double
            RunLEAF = New ACRClassLib.clsLEAF()
            Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, check1, AllResp)
            RunLEAF = Nothing

            CallAC_belly(1).NEvalPoints = NNodesLong
            ReDim CallAC_belly(1).EvalX(NNodesLong)
            ReDim CallAC_belly(1).EvalY(NNodesLong)

            For IEvaly = 1 To NNodesLong
                CallAC_belly(1).EvalX(IEvaly) = xStrain
                CallAC_belly(1).EvalY(IEvaly) = South - 160 + IEvaly * (North - South + 320) / NNodesLong
            Next IEvaly

            Dim check1_belly(,) As Double
            RunLEAF = New ACRClassLib.clsLEAF()
            Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC_belly, LEAStrActiveX, check1_belly, AllResp)
            RunLEAF = Nothing




            ' 111======================================================================
            ' 111======================================================================




            'RunLEAF = New ACNClassLib.clsLEAF()
            'If gTandemFnew Then ' kairat replace tandem
            '    Call RunLEAF.ComputeResponse2(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
            'Else
            '    Call RunLEAF.ComputeResponse(LeafResp, 1, CallAC, LEAStrActiveX, StrainResponse, AllResp)
            'End If
            'RunLEAF = Nothing

new2:

            Dim IA As Integer = 1
            Dim Overflow As Boolean
            Dim publicStrain As Double


            Damage = 0.0!

            For Me.IEvaly = 2 To NNodesLong - 1S
                If StrainResponse(IA, IEvaly) < 0 Then
                    ''MAX strain
                    If StrainResponse(IA, IEvaly - 1) < StrainResponse(IA, IEvaly) And
                        StrainResponse(IA, IEvaly) > StrainResponse(IA, IEvaly + 1) Then
                        StrainMax = System.Math.Abs(StrainResponse(IA, IEvaly))
                        ExtrType = 1
                        GoTo FAILURELAW
                    End If

                    'MIN strain
                    If StrainResponse(IA, IEvaly - 1) > StrainResponse(IA, IEvaly) And
                        StrainResponse(IA, IEvaly) < StrainResponse(IA, IEvaly + 1) Then
                        StrainMax = System.Math.Abs(StrainResponse(IA, IEvaly))
                        ExtrType = 2
FAILURELAW:
                        'HHHHHHHHHHHHHHH
                        If StrainMax < 0.0001 Then StrainMax = 0.0001 Else Overflow = False '2 Gears
                        '       Standard subgrade criterion.

                        'Bleasdale Model (log10 on cov)
                        Dim a11, b11, c11 As Single
                        'Bleasdale 2
                        a11 = -0.163768916705
                        b11 = 185.192806802
                        c11 = 1.65054449461

                        If StrainMax < 0.001 Then '2 Gears
                            StrainMax = 0.001
                        End If

                        If StrainMax <= 0.001765093 Then
                            NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
                        Else
                            NtoFail = (0.00414131183 / StrainMax) ^ 8.1
                        End If

                        'FFFFFFFFFFFFFFF
                        Damage = Damage + (-1) ^ ExtrType * 1.0 / NtoFail
                    End If
                End If
            Next IEvaly

            NtoFail = CSng(1.0 / Damage)
            publicStrain = CSng(StrainMax)

        End If


    End Sub

    Private Sub FAAModulus_P154()

        Try
            'LEAStrActiveX.Modulus(3) = 25000

            Dim AggErr As Boolean = False
            Dim SubLayers As Boolean = True

            ReDim LayerType(20)
            LayerType(0) = "User Defined"
            LayerType(1) = "Asphalt"
            LayerType(2) = "Base"
            LayerType(3) = "Subbase"
            LayerType(4) = "Subgrade" 'Subgrade
            LayerType(5) = "PCC"
            LayerType(6) = "Ag Crushed" 'P-209
            LayerType(7) = "St (Rigid)"
            LayerType(8) = "Ag Uncrushed" 'P-154
            LayerType(9) = "St (Flexible)"
            LayerType(10) = "AC Overlay"
            LayerType(11) = "PCC Overlay Unbond"
            LayerType(12) = "PCC Overlay Part Bond"
            LayerType(13) = "PCC Overlay on Flex."
            LayerType(14) = "St (Flexible)"
            LayerType(15) = "St (Rigid)"
            LayerType(16) = "St (Rigid)"
            LayerType(17) = "St (Rigid)"
            LayerType(18) = "Ag Crushed"
            LayerType(19) = "Rubblized PCC Base"
            LayerType(20) = "St (Flexible)"


            CDFErr = 10.0!

            Call FAAModulus(AggErr, SubLayers, CShort(LEAStrActiveX.NLayers))

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Sub

    Private Sub FAAModulus(ByRef RErr As Boolean, ByRef SubLayers As Boolean, ByVal NPlayers As Short)

        Try



            ReDim BaseMod(MaxNPLayers)
            ReDim SubbaseMod(MaxNPLayers)



            ReDim Thick(NPlayers)
            ReDim Modulus(NPlayers)
            ReDim PoissonsRatio(NPlayers)

            For i1 As Integer = 1 To NPlayers
                Thick(i1) = LEAStrActiveX.Thick(i1)
                Modulus(i1) = LEAStrActiveX.Modulus(i1)
                PoissonsRatio(i1) = LEAStrActiveX.Poisson(i1)
            Next



            ' Sets the values of julStrVar for writing to JULEA.STR through
            ' call to WriteJuleaFiles.

            ' Sets the values of NEvalDepths and EvalDepths(1 & 2) except for
            ' rigid overlays. See OverLay for rigid overlays.
            ' Has been modified for only one call to CDFFlex, 1 EvalDepth.

            ' If aggregate layers are present, calculates the number of
            ' sublayers and the modulus of each using the same procedure
            ' as in the LEDNEW Modulus program.

            Dim II, I, J As Short
            Dim IDB, IDS As Short
            Dim LB, LS As Short
            Dim NSB As Short
            Dim TSB As Double
            Dim NSS As Short
            Dim TSS As Double
            '  Dim BaseMod(MaxNPLayers) as Single       in Decs.
            '  Dim SubbaseMod(MaxNPLayers) as Single    in Decs.

            SubLayers = False
            LayerSwitch = True 'implement subdivisions

            RErr = CBool(0)
            IDB = 0 : LB = 0 : NSB = 0
            IDS = 0 : LS = 0 : NSS = 0

            For I = 1 To NPlayers
                If LayerType(LCode(I)) = NAgBase Then
                    IDB = IDB + 1S : LB = I
                End If
                If LayerType(LCode(I)) = NAgSBase Then
                    IDS = IDS + 1S : LS = I
                End If
            Next I

            'LEAStrActiveX.Modulus(NPlayers) = 15000 'to comment $$$$$
            'Modulus(NPlayers) = 15000 'to comment $$$$$

            If IDS > 0 Then ' Subbase first.
                'Call ModulusThick(Thick(LS), LCode(LS), Modulus(LS + 1), NSS, TSS)
                Call FAAModulusThick(Thick(LS), LCode(LS), Modulus(LS + 1), NSS, TSS)
            End If

            If IDB > 0 Then ' Followed by base.
                Temp1 = Modulus(LB + 1) ' No subbase or separating layer.
                If IDS > 0 Then
                    If LS = LB + 1 Then Temp1 = SubbaseMod(1) ' Base next to subbase.
                End If
                'Call ModulusThick(Thick(LB), LCode(LB), Temp1, NSB, TSB)
                Call FAAModulusThick(Thick(LB), LCode(LB), Temp1, NSB, TSB)
            End If

            If NSB > 1 Or NSS > 1 Then SubLayers = True

            ' The first option in the If below computes average aggregate layer
            ' modulus and sets the modulus for the full layer to that value.
            ' The second option computes the average for printing but assigns
            ' the correct values to individual layers at the same time the new
            ' layers are created. This is the main reason for having the
            ' julVarName variables; retain the design structure as is but send
            ' the sublayered structure to Julea without disturbing either.

            If Not LayerSwitch Then 'clsACNsub - no layer division

                '    Debug.Print "Mod "; LayerSwitch; SubLayers
                For I = 1 To NPlayers
                    If I = LB Then
                        Modulus(I) = 0
                        For J = 1 To NSB
                            Modulus(I) = Modulus(I) + BaseMod(J)
                        Next J
                        Modulus(I) = Modulus(I) / NSB
                    ElseIf I = LS Then
                        Modulus(I) = 0
                        For J = 1 To NSS
                            Modulus(I) = Modulus(I) + SubbaseMod(J)
                        Next J
                        Modulus(I) = Modulus(I) / NSS
                    End If
                    julThick(I) = Thick(I)
                    julModulus(I) = Modulus(I)
                    julLCode(I) = LCode(I)
                    'leafPoissonsRatio(I) = jobPoissonsRatio(ISect, I)
                    leafPoissonsRatio(I) = PoissonsRatio(I)
                Next I
                julNPLayers = NPlayers

            Else 'subdivide layer thickness

                II = 1
                For I = 1 To NPlayers

                    If LayerType(LCode(I)) = NAgBase Then 'P-209 CrAg  "Ag Crushed"
                        Modulus(I) = 0 ' Print average on structure drawing.
                        If gNewModulus_P209 Then
                            For J = 1 To NSB
                                julThick(II) = TSS_P209(J)
                                julModulus(II) = BaseMod(J)
                                julLCode(II) = LCode(I) 'AggBaseLC
                                'leafPoissonsRatio(II) = jobPoissonsRatio(ISect, I)
                                leafPoissonsRatio(II) = PoissonsRatio(I)
                                Modulus(I) = Modulus(I) + BaseMod(J) * TSS_P209(J)
                                II = II + 1S
                            Next J
                            Modulus(I) = Modulus(I) / Thick(I)
                        Else
                            For J = 1 To NSB
                                julThick(II) = TSB
                                julModulus(II) = BaseMod(J)
                                julLCode(II) = LCode(I) 'AggBaseLC
                                'leafPoissonsRatio(II) = jobPoissonsRatio(ISect, I)
                                leafPoissonsRatio(II) = PoissonsRatio(I)
                                Modulus(I) = Modulus(I) + BaseMod(J)
                                II = II + 1S
                            Next J
                            Modulus(I) = Modulus(I) / NSB
                        End If

                    ElseIf LayerType(LCode(I)) = NAgSBase Then 'P-154 UnCrAg "Ag Uncrushed"
                        Modulus(I) = 0 ' Print average on structure drawing.
                        If gNewModulus_P154 Then
                            For J = 1 To NSS
                                julThick(II) = TSS_P154(J)
                                julModulus(II) = SubbaseMod(J)
                                julLCode(II) = LCode(I) 'AggSubbaseLC
                                'leafPoissonsRatio(II) = jobPoissonsRatio(ISect, I)
                                leafPoissonsRatio(II) = PoissonsRatio(I)
                                Modulus(I) = Modulus(I) + SubbaseMod(J) * TSS_P154(J)
                                II = II + 1S
                            Next J
                            Modulus(I) = Modulus(I) / Thick(I)
                        Else
                            For J = 1 To NSS
                                julThick(II) = TSS
                                julModulus(II) = SubbaseMod(J)
                                julLCode(II) = LCode(I) 'AggSubbaseLC
                                'leafPoissonsRatio(II) = jobPoissonsRatio(ISect, I)
                                leafPoissonsRatio(II) = PoissonsRatio(I)
                                Modulus(I) = Modulus(I) + SubbaseMod(J)
                                II = II + 1S
                            Next J
                            Modulus(I) = Modulus(I) / NSS
                        End If

                    Else 'HMA, subgrade, user defined layer etc.

                        julThick(II) = Thick(I)
                        julModulus(II) = Modulus(I)
                        julLCode(II) = LCode(I)
                        'leafPoissonsRatio(II) = jobPoissonsRatio(ISect, I)
                        leafPoissonsRatio(II) = PoissonsRatio(I)
                        II = II + 1S

                    End If

                Next I
                julNPLayers = II - 1S
                ' Debug.Print julNPLayers; julModulus(3); julModulus(4)

                'calculated for average modulus
                'julNPLayers = NPLayers
                'For I = 1 To julNPLayers
                '    julThick(I) = Thick(I)
                '    julModulus(I) = Modulus(I)
                '    julLCode(I) = LCode(I)
                'Next


            End If




            LEAStrActiveX.NLayers = julNPLayers

            ReDim LEAStrActiveX.Thick(LEAStrActiveX.NLayers)
            ReDim LEAStrActiveX.Modulus(LEAStrActiveX.NLayers)
            ReDim LEAStrActiveX.Poisson(LEAStrActiveX.NLayers)
            ReDim LEAStrActiveX.InterfaceParm(LEAStrActiveX.NLayers)

            For i3 As Integer = 1 To LEAStrActiveX.NLayers
                LEAStrActiveX.Thick(i3) = julThick(i3)
                LEAStrActiveX.Modulus(i3) = julModulus(i3)
                'LEAStrActiveX.Poisson(i3) = jul
                LEAStrActiveX.Poisson(i3) = 0.35
                LEAStrActiveX.InterfaceParm(i3) = 1
            Next







            S = LayerType(LCode(1))

            If DesignType = NewFlex Then
                NEvalDepths = 1
                EvalDepth(1) = -julThick(1)
                EvalDepth(2) = 0.0!
                For J = 1 To julNPLayers - 1S
                    EvalDepth(2) = EvalDepth(2) + julThick(J)
                Next J
                EvalDepth(2) = CSng(EvalDepth(2) * 1.0001)
                EvalDepth(1) = EvalDepth(2)
            End If

            If AlternateSG Then
                If DesignType = NewFlex Then
                    NEvalDepths = 1
                    EvalDepth(1) = 0.0!
                    For J = 1 To IterlayerChosen
                        EvalDepth(1) = EvalDepth(1) + Thick(J)
                    Next J
                    EvalDepth(1) = CSng(EvalDepth(1) * 1.0001)
                End If
            End If

            If DesignType = NewRigid Or DesignType = PCCOnFlex Then
                NEvalDepths = 1
                EvalDepth(1) = -julThick(1)
                '    EvalDepth(2) = 500
            End If

            If DesignType = FlexOnFlex Then
                NEvalDepths = 1
                EvalDepth(1) = 0.0!
                For J = 1 To julNPLayers - 1S
                    EvalDepth(1) = EvalDepth(1) + julThick(J)
                Next J
                EvalDepth(1) = CSng(EvalDepth(1) * 1.0001)
            End If

            ' EvalDepths for rigid overlays are set in design subs.


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub

    Private Sub FAAModulusThick(ByRef ThickS As Double, ByRef LCode As Short, ByRef ModUnder As Double, ByRef NS As Short, ByRef TS As Double)

        Try


            ' Compute thicknesses and modulus values for sublayers within
            ' aggregate base and subbase layers.
            'Static NSsubbase As Short 'ikawa 999
            Dim I As Short
            Dim D, C, julThickMin As Single
            Dim DIV As Double
            Static NSAgBase, NSAgSBase As Short
            Dim MaxThick8 As Single
            Dim MaxThick10 As Single


            gP154_C688D156 = True
            gNewModulus_P154 = True
            gNewModulus_P209 = True
            gP209_C1052D20 = True '2017.07.25 1.42


            MaxThick8 = 8 : MaxThick10 = 10

            If LayerType(LCode) = NAgBase Then
                '  If LCode = AggBaseLC Then
                C = 10.52 : D = 2.1 : julThickMin = 10.0!

                If gP209_C1052D20 Then
                    C = 10.52 : D = 2.0 : julThickMin = 10.0! 'new values
                End If

            ElseIf LayerType(LCode) = NAgSBase Then
                C = 7.18 : D = 1.56 : julThickMin = 8.0!

                If gP154_C688D156 Then
                    C = 6.88 : D = 1.56 : julThickMin = 8.0! 'new values
                End If

                'to comment C D values
                'C = 7.18 : D = 1.66 : julThickMin = 8.0!

            End If

            TS = ThickS
            NS = 1
            'If LayerSwitch And CDFErr <= CDFErrCntrl * 0.5 Then
            If LayerSwitch And CDFErr <= CDFErrCntrl * 0.7 Then ' GFH 04/23/03.
                If LayerType(LCode) = NAgBase Then
                    'If the final design has sublayers very close in thickness to the
                    NS = NSAgBase ' minimum thicknesses, then the iteration
                Else ' may not converge because of different NS
                    NS = NSAgSBase ' either side of CDF = 1. Freeze NS to stop
                End If ' this happening.
            ElseIf TS > julThickMin Then  'WES layer subdivisions ***
                DIV = TS / julThickMin
                NS = CShort(Int(Int(TS) / Int(julThickMin)))
                If (DIV - NS) <> 0.0! Then NS = NS + 1S
            End If
            TS = TS / NS


            '---------- Begin Rigid layers subdivisions
            'Dim NofLayers As Short

            'If DesignType = NewRigid Or _
            '   DesignType = PCCOnFlex Then

            '    NofLayers = NPLayers + NS - 1S

            '    If NSsubbase > 0 Then
            '        NofLayers = NofLayers + NSsubbase - 1S
            '    End If
            '    If NofLayers > 6 Then
            '        NS = NS - (NofLayers - 6S)
            '        TS = (NS + (NofLayers - 6S)) * TS / NS
            '    End If
            'End If

            ''If DesignType = PCCOnFlex Or _
            'If DesignType = UnbondOnRigid Or _
            'DesignType = PartBondOnRigid Or _
            'DesignType = FlexOnRigid Then

            '    NofLayers = NPLayers + NS - 1S
            '    If NSsubbase > 0 Then
            '        NofLayers = NofLayers + NSsubbase - 1S
            '    End If
            '    If NofLayers > 7 Then
            '        NS = NS - (NofLayers - 7S)
            '        TS = (NS + (NofLayers - 7S)) * TS / NS
            '    End If

            'End If
            '--------- End of rigid part



            'FAA layer subdivisions for flexible pavements (LEAF analysis)
            'ThickS - total thickness of layer (P-154 or P-209)
            Dim LT1, minThick As Double
            Dim TSarray(NS) As Double

            If gNewModulus_P154 And LayerType(LCode) = NAgSBase Then 'Aggregate Uncrushed

                If LEAStrActiveX.Thick(LEAStrActiveX.NLayers - 1) < 4 Then '2016.03.08
                    NS = 1
                    ReDim TSarray(NS)
                    TSarray(1) = LEAStrActiveX.Thick(LEAStrActiveX.NLayers - 1)
                    TSS_P154(1) = TSarray(1)

                Else

                    minThick = 4
                    LT1 = ThickS - CInt(ThickS / MaxThick8) * MaxThick8
                    If LT1 < minThick Then
                        NS = CShort(ThickS / MaxThick8)
                    Else
                        NS = CShort(ThickS / MaxThick8) + 1S
                    End If

                    ReDim TSarray(NS)
                    For I = 1 To NS
                        TSarray(I) = MaxThick8
                        TSS_P154(I) = TSarray(I)
                    Next

                    If LT1 < minThick Then
                        TSarray(1) = MaxThick8 + LT1
                        TSS_P154(1) = TSarray(1)
                    Else
                        TSarray(1) = LT1
                        TSS_P154(1) = TSarray(1)
                    End If

                End If


            ElseIf gNewModulus_P209 And LayerType(LCode) = NAgBase Then 'Aggregate Crushed

                minThick = 5
                If ThickS < minThick Then
                    NS = 1 : ReDim TSarray(NS)
                    TSarray(1) = ThickS
                    TSS_P209(1) = TSarray(1)
                Else

                    LT1 = ThickS - CInt(ThickS / MaxThick10) * MaxThick10
                    If LT1 < minThick Then
                        NS = CShort(ThickS / MaxThick10)
                    Else
                        NS = CShort(ThickS / MaxThick10) + 1S
                    End If

                    ReDim TSarray(NS)
                    For I = 1 To NS
                        TSarray(I) = MaxThick10
                        TSS_P209(I) = TSarray(I)
                    Next

                    If LT1 < minThick Then
                        TSarray(1) = MaxThick10 + LT1
                        TSS_P209(1) = TSarray(1)
                    Else
                        TSarray(1) = LT1
                        TSS_P209(1) = TSarray(1)
                    End If
                End If
            End If





            Dim KeepMod2 As Double
            Dim tempTS, TS1, expr1 As Double

            If LayerType(LCode) = NAgBase Then '"Ag Crushed"
                If gNewModulus_P209 Then 'modified modulus procedure

                    NSAgBase = NS : NS_P209 = NS
                    BaseMod(NS + 1) = ModUnder
                    TS1 = TSS_P209(1)
                    For I = NS To 1 Step -1
                        tempTS = TSS_P209(I)
                        Temp1 = CSng(1.0! + C * System.Math.Log(tempTS) / Log10)
                        Temp2 = CSng(D * System.Math.Log(BaseMod(I + 1)) * System.Math.Log(tempTS) / (Log10 * Log10))
                        BaseMod(I) = BaseMod(I + 1) * (Temp1 - Temp2)

                        If I = 2 And TS1 < MaxThick10 Then
                            KeepMod2 = BaseMod(I)
                            expr1 = tempTS + MaxThick10 - TS1
                            Temp1 = CSng(1.0! + C * System.Math.Log(expr1) / Log10)
                            Temp2 = CSng(D * System.Math.Log(BaseMod(I + 1)) * System.Math.Log(expr1) / (Log10 * Log10))
                            BaseMod(I) = BaseMod(I + 1) * (Temp1 - Temp2)
                        End If

                        If I = 1 And TS1 < MaxThick10 And NS <> 1 Then
                            Temp1 = CSng(1.0! + C * System.Math.Log(MaxThick10) / Log10)
                            Temp2 = CSng(D * System.Math.Log(KeepMod2) * System.Math.Log(MaxThick10) / (Log10 * Log10))
                            expr1 = MaxThick10 / 2
                            BaseMod(I) = KeepMod2 * (Temp1 - Temp2)
                            BaseMod(I) = BaseMod(I + 1) + (TS1 - expr1) / expr1 * (BaseMod(I) - BaseMod(I + 1))
                        End If

                    Next I

                Else 'WES procedure

                    NSAgBase = NS : publicNS_P209 = NS
                    BaseMod(NS + 1) = ModUnder
                    For I = NS To 1 Step -1
                        Temp1 = CSng(1.0! + C * System.Math.Log(TS) / Log10)
                        Temp2 = CSng(D * System.Math.Log(BaseMod(I + 1)) * System.Math.Log(TS) / (Log10 * Log10))
                        BaseMod(I) = BaseMod(I + 1) * (Temp1 - Temp2)
                    Next I

                End If

            Else '"Ag Uncrushed"

                If gNewModulus_P154 Then 'modified modulus procedure
                    NSAgSBase = NS : NS_P154 = NS
                    SubbaseMod(NS + 1) = ModUnder
                    TS1 = TSS_P154(1)
                    For I = NS To 1 Step -1
                        tempTS = TSS_P154(I)
                        Temp1 = CSng(1.0! + C * System.Math.Log(tempTS) / Log10)
                        Temp2 = CSng(D * System.Math.Log(SubbaseMod(I + 1)) * System.Math.Log(tempTS) / (Log10 * Log10))
                        SubbaseMod(I) = SubbaseMod(I + 1) * (Temp1 - Temp2)

                        If I = 2 And TSarray(1) < MaxThick8 Then
                            KeepMod2 = SubbaseMod(I)
                            expr1 = TSarray(I) + MaxThick8 - TSarray(1)
                            Temp1 = CSng(1.0! + C * System.Math.Log(expr1) / Log10)
                            Temp2 = CSng(D * System.Math.Log(SubbaseMod(I + 1)) * System.Math.Log(expr1) / (Log10 * Log10))
                            SubbaseMod(I) = SubbaseMod(I + 1) * (Temp1 - Temp2)
                        End If

                        If I = 1 And TSarray(1) < MaxThick8 And NS <> 1 Then
                            Temp1 = CSng(1.0! + C * System.Math.Log(MaxThick8) / Log10)
                            Temp2 = CSng(D * System.Math.Log(KeepMod2) * System.Math.Log(MaxThick8) / (Log10 * Log10))
                            expr1 = MaxThick8 / 2
                            SubbaseMod(I) = KeepMod2 * (Temp1 - Temp2)
                            SubbaseMod(I) = SubbaseMod(I + 1) + (TS1 - expr1) / expr1 * (SubbaseMod(I) - SubbaseMod(I + 1))
                        End If

                        SBM(I) = SubbaseMod(I) '???????
                    Next I

                Else 'WES procedure
                    NSAgSBase = NS : publicNS_P154 = NS
                    SubbaseMod(NS + 1) = ModUnder
                    For I = NS To 1 Step -1
                        Temp1 = CSng(1.0! + C * System.Math.Log(TS) / Log10)
                        Temp2 = CSng(D * System.Math.Log(SubbaseMod(I + 1)) * System.Math.Log(TS) / (Log10 * Log10))
                        SubbaseMod(I) = SubbaseMod(I + 1) * (Temp1 - Temp2)
                    Next I
                End If

            End If


            'ikawa 999
            'If DesignType = NewRigid Or DesignType = PCCOnFlex Or _
            '    DesignType = UnbondOnRigid Or DesignType = PartBondOnRigid Or _
            '    DesignType = FlexOnRigid Then

            '    If NSsubbase = 0 Then
            '        NSsubbase = NS
            '    Else
            '        NSsubbase = 0
            '    End If
            'End If


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Sub

    Private Sub Z_Eval_Stress_Thick()

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
            Call Calculate_Rigid_Stress()
            PrintLine(9, LPad(4, CStr(Math.Round(tt, 2))) & "   " & LPad(3, CStr(gStress)))
        Next
        FileClose(9)


    End Sub


    Private Sub Z_Evaluate_Stress_Rigid(ByRef Str1 As ACRClassLib.clsLEAF.LEAFStrParms,
                                 ByRef Air1 As ACRClassLib.clsLEAF.LEAFACParms)

        Dim North, South, West, East As Single
        Dim hor1, vert1 As Single
        Dim horLimit, vertLimit As Integer
        Dim horStep, vertStep As Single

        'North = -1000 : South = 1000
        'East = -1000 : West = 1000
        North = -1.0E+35 : South = 1.0E+35
        East = -1.0E+35 : West = 1.0E+35

        For i = 1 To CallAC(1).NTires
            If CallAC(1).TireX(i) > East Then
                East = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) > North Then
                North = CSng(CallAC(1).TireY(i))
            End If
            If CallAC(1).TireX(i) < West Then
                West = CSng(CallAC(1).TireX(i))
            End If
            If CallAC(1).TireY(i) < South Then
                South = CSng(CallAC(1).TireY(i))
            End If
        Next


        hor1 = (East - West) / 2
        vert1 = (North - South) / 2

        horLimit = CInt(Math.Ceiling(hor1))
        vertLimit = CInt(Math.Ceiling(vert1))

        horStep = hor1 / horLimit
        vertStep = vert1 / vertLimit


        Dim i1, j1 As Integer
        CallAC(1).NEvalPoints = (horLimit + 1) * (vertLimit + 1)
        ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
        ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)



        For i1 = 0 To horLimit Step 1
            For j1 = 0 To vertLimit Step 1
                CallAC(1).EvalX(i1 + 1) = i1 * horStep
                CallAC(1).EvalY(i1 + 1) = i1 * vertStep
            Next
        Next


        Call Calculate_Rigid_Stress()

    End Sub


    Public Sub Z_Evaluation_Loop999(ByVal PavementType As clsACR.PavementType,
         ByVal gross_weight As Single,
         ByVal percent_gw As Single,
         ByVal wheels_number As Integer,
         ByVal tire_pressure As Single,
         ByVal CoordX() As Single,
         ByVal CoordY() As Single,
         ByVal modul1 As Single)

        gPrintOutput_ACN = True
        gPavementType = clsACR.PavementType.Flexible
        Call Check_for_Subdirectory()
        Dim CalculateACR As ACRdata

        gTimeSave1 = timeGetTime
        Dim File1 As String
        File1 = "7777777.txt"

        If PavementType = clsACR.PavementType.Flexible Then
            FileOpen(33, WDir1 & "\" & File1, OpenMode.Append)
            Print(33, LPad(11, "gross_weight"))

            Print(33, LPad(11, "CovACN") & "  ")
            Print(33, LPad(15, "CovGear") & "  ")
            Print(33, LPad(15, "CovSWL") & "  ")

            Print(33, LPad(15, "gStrainTarget") & "  ")
            Print(33, LPad(15, "StrainMaxSWL") & "  ")

            Print(33, LPad(11, "libACR(1)"))
            Print(33, LPad(11, "ACRthick"))
            PrintLine(33, LPad(13, "modul1"))
            FileClose(33)

        End If

        Dim FileName, Date1 As String

        For i111 As Integer = CInt(gross_weight) To 10 Step -100
            'For i111 As Integer = 50000 To 100 Step -200
            'For i111 As Integer = 15740 To 15730 Step -1
            'For i111 As Integer = 16000 To 15400 Step -10

            ReDim CalculateACR.libACR(1)
            ReDim CalculateACR.libACRthick(1)
            StressSWL = 0

            gICAOCodeIndex = ICAOCodeIndexF(modul1)
            start_cat = gICAOCodeIndex
            end_cat = start_cat

            gross_weight = i111
            Dim RunACN As ACRClassLib.clsACR
            RunACN = New ACRClassLib.clsACR()
            CalculateACR = RunACN.CalculateACR(PavementType, gross_weight, percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
            RunACN = Nothing

            FileOpen(33, WDir1 & "\" & File1, OpenMode.Append)
            Print(33, LPad(11, Format(gross_weight, "#0.00")))

            If PavementType = clsACR.PavementType.Rigid Then
                Print(33, LPad(11, Format(StressACN, "#0.000")) & "  ")
                Print(33, LPad(11, Format(StressGear, "#0.000")) & "  ")
                Print(33, LPad(11, Format(StressSWL, "#0.000")) & "  ")
            Else
                Print(33, LPad(11, Format(CovACN, "#0.0")) & "  ")
                Print(33, LPad(15, Format(CovGear, "#0.0")) & "  ")
                Print(33, LPad(15, Format(CovSWL, "#0.0")) & "  ")

                Print(33, LPad(15, Format(gStrainTarget, "#0.00000000")) & "  ")
                Print(33, LPad(15, Format(StrainMaxSWL, "#0.00000000")) & "  ")
            End If

            Print(33, LPad(11, Format(CalculateACR.libACR(1), "#0.0000")))
            Print(33, LPad(11, Format(CalculateACR.libACRthick(1), "#0.0000")))
            PrintLine(33, LPad(13, Format(modul1, "#0.000")))
            FileClose(33)

        Next

    End Sub





    Private Sub Z_Evaluation_Loop(ByVal PavementType As clsACR.PavementType,
             ByVal gross_weight As Single,
             ByVal percent_gw As Single,
             ByVal wheels_number As Integer,
             ByVal tire_pressure As Single,
             ByVal CoordX() As Single,
             ByVal CoordY() As Single)

        Dim CalculateACR As ACRdata

        gTimeSave1 = timeGetTime
        ReDim CalculateACR.libACR(4)
        ReDim CalculateACR.libACRthick(4)

        gTandemFnew = False 'Evaluation_Loop

        'WDir1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        'WDir1 = WDir1 & "\FAARFIELD\ACR"

        If True Then
            WDir1 = SpecialDirectories.MyDocuments + "\My FAARFIELD\ACR_Results_" & PavementType
            If Directory.Exists(WDir1) Then
            Else
                System.IO.Directory.CreateDirectory(WDir1)
            End If
        End If

        Dim FileName, Date1 As String

        '************************  Evaluation Part for Stress ************************
        '  Loop over subgrade strength to find maximum stress locations for A380-800
        '*****************************************************************************
        PavementType = clsACR.PavementType.Rigid
        gPrintOutput = True

        'Print stress array values
        CallAC(1).GearLoad = gross_weight * percent_gw
        CallAC(1).NTires = wheels_number



        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        CallAC(1).GearLoad = gross_weight * percent_gw / wheels_number * 2
        wheels_number = 2
        CallAC(1).NTires = wheels_number
        ReDim CoordX(2) : ReDim CoordY(2)
        CoordX(1) = 0 : CoordY(1) = 0
        CoordX(1) = 30 : CoordY(1) = 0
        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
        Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry

        'Call Set_Eval_Points_Ysym()

        'Call Set_Eval_Points_Wheel_Coord()
        'Call Make_CallAC_Copy(CallAC(1), CopyAC(1))

        Call Set_Eval_Points_X_longitunal(CallAC)
        Call Make_CallAC_Copy(CallAC(1), CopyAC(1))


        LEAStrActiveX.NLayers = 3
        Call RedimArraysRigid(LEAStrActiveX.NLayers, 1, LCode, LEAStrActiveX) 'Cat 1
        'Call RedimArraysRigid(LEAStrActiveX.NLayers, 4) 'Cat 4

        LEAStrActiveX.Thick(1) = 24.94 'P-401      Cat 1
        'LEAStrActiveX.Thick(1) = 14.57 'P-401      Cat 4

        LEAStrActiveX.Thick(2) = 20 / 2.54 'P-209  (200 mm to inches)

        'Call Calculate_Stress_Rigid_Array_A380()
        Call ZZZ_Cal3_Stress_Rigid_Array_Eval_Points()

        Call Print_RRR_Array_Ysym(LEAStrActiveX)

        Exit Sub



        'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        For is1 As Integer = 1000 To 1000 Step 5000
            public_is1 = is1

            CallAC(1).GearLoad = gross_weight * percent_gw
            CallAC(1).NTires = wheels_number
            Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
            Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry

            If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Loop
                Call Set_Eval_Points_XYsym(CallAC)
            ElseIf Symmetry = SymmetryType.YSymmetry Then
                Call Set_Eval_Points_Ysym(CallAC)
            Else
                Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
            End If

            LEAStrActiveX.NLayers = 3
            Call RedimArraysRigid(LEAStrActiveX.NLayers, 1, LCode, LEAStrActiveX)
            LEAStrActiveX.Thick(1) = 5 'P-401
            LEAStrActiveX.Thick(2) = 20 / 2.54 'P-209  (200 mm to inches)


            Call Make_CallAC_Copy(CallAC(1), CopyAC(1))
            Call Calculate_Thickness_Rigid()

            iMaxGear = iMax
            If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Loop
                Call Set_Eval_Points_XYsym(CallAC)
            ElseIf Symmetry = SymmetryType.YSymmetry Then
                Call Set_Eval_Points_Ysym(CallAC)
            Else
                Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
            End If

            If LEAStrActiveX.Thick(1) = 2 Then
                If gStress < (StressACN - 1) Then
                    CalculateACR.libACR(1) = 0
                    CalculateACR.libACRthick(1) = 0
                    'Call Print_Results_Rigid(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                    Continue For
                End If
            End If

            Call Set_SWLdata(CallAC)
            Call Calculate_DSWL_Rigid()

            CalculateACR.libACR(1) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
            CalculateACR.libACRthick(1) = CSng(LEAStrActiveX.Thick(1))
            'Call Print_Results_Rigid(gross_weight, CallAC, CopyAC, LEAStrActiveX)

            gTimeSave2 = timeGetTime
            ETimemsecs = CSng((gTimeSave2 - gTimeSave1) / 1000)

            FileName = WDir1 & "\ACR_A380.txt"
            FileOpen(9, FileName, OpenMode.Append) 'ACN_
            Print(9, LPad(4, Format(is1, "###")))
            Print(9, LPad(11, Format(CalculateACR.libACR(1), "###.00000")))
            Print(9, LPad(13, Format(CalculateACR.libACRthick(1), "###.000000")))
            Print(9, LPad(12, CStr(Format(ETimemsecs, "##,##0.000"))) & " sec.")
            Print(9, LPad(12, CStr(Format(ETimemsecs / 60, "##,##0.000"))) & " min.")
            PrintLine(9, "")
            FileClose(9)

        Next

        Exit Sub




        '************************  Evaluation Part for Strain ************************
        '  Loop over subgrade strength to find maximum strain locations for A380-800
        '*****************************************************************************
        PavementType = clsACR.PavementType.Flexible
        gPrintOutput = True

        For is1 As Integer = 10000 To 10000 Step 100
            public_is1 = is1

            CallAC(1).GearLoad = gross_weight * percent_gw
            CallAC(1).NTires = wheels_number
            Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
            Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry

            If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Loop
                Call Set_Eval_Points_XYsym(CallAC)
            Else
                Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
            End If

            LEAStrActiveX.NLayers = 3
            Call RedimArraysFlexible(LEAStrActiveX.NLayers, 1, LCode, LEAStrActiveX)
            LEAStrActiveX.Thick(1) = 5 'P-401
            LEAStrActiveX.Thick(2) = 30 'P-209 variable


            Call Make_CallAC_Copy(CallAC(1), CopyAC(1))
            Call Calculate_Thickness_Flex(LEAStrActiveX.NLayers - 1) 'iterate on layer on subgrade

            iMaxGear = iMax
            If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Loop
                Call Set_Eval_Points_XYsym(CallAC)
            Else
                Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
            End If

            If LEAStrActiveX.Thick(LEAStrActiveX.NLayers - 1) = 1 Then
                If NtoFail > (CovACN + 10) Then

                    CalculateACR.libACR(1) = 0
                    CalculateACR.libACRthick(1) = 0
                    'Call Print_Results_Flex(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                    Continue For
                End If
            End If

            Call Set_SWLdata(CallAC)
            Call Calculate_DSWL_Flex()

            CalculateACR.libACR(1) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
            Flex_Total_Thick = Flex_Thick_Func(LEAStrActiveX)
            CalculateACR.libACRthick(1) = CSng(Flex_Total_Thick)
            'Call Print_Results_Flex(gross_weight, CallAC, CopyAC, LEAStrActiveX)

            gTimeSave2 = timeGetTime
            ETimemsecs = CSng((gTimeSave2 - gTimeSave1) / 1000)

            FileName = WDir1 & "\ACR_A380.txt"
            FileOpen(9, FileName, OpenMode.Append) 'ACN_
            Print(9, LPad(4, Format(is1, "###")))
            Print(9, LPad(11, Format(CalculateACR.libACR(1), "###.00000")))
            Print(9, LPad(13, Format(CalculateACR.libACRthick(1), "###.000000")))
            Print(9, LPad(12, CStr(Format(ETimemsecs, "##,##0.000"))) & " sec.")
            Print(9, LPad(12, CStr(Format(ETimemsecs / 60, "##,##0.000"))) & " min.")
            PrintLine(9, "")
            FileClose(9)

        Next

        Exit Sub




        '************************       Evaluation Part       ************************
        For is1 = 0 To 400 Step 2
            CoordY(2) = is1
            gCat = 4

            CallAC(1).GearLoad = gross_weight * percent_gw
            CallAC(1).NTires = wheels_number
            Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
            Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry

            If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Loop
                Call Set_Eval_Points_XYsym(CallAC)
            Else
                Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
            End If

            'If wheels_number <= 2 Then
            LEAStrActiveX.NLayers = 3
            Call RedimArraysFlexible(LEAStrActiveX.NLayers, gCat, LCode, LEAStrActiveX)
            LEAStrActiveX.Thick(1) = 3 'P-401
            LEAStrActiveX.Thick(2) = 25 'P-209 variable

            Call Make_CallAC_Copy(CallAC(1), CopyAC(1))
            Call Calculate_Thickness_Flex(LEAStrActiveX.NLayers - 1) 'iterate on layer on subgrade

            If LEAStrActiveX.Thick(LEAStrActiveX.NLayers - 1) = 1 Then
                If NtoFail > (CovACN + 10) Then

                    CalculateACR.libACR(gCat) = 0
                    CalculateACR.libACRthick(gCat) = 0
                    'Call Print_Results_Flex(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                    Continue For
                End If
            End If

            Call Set_SWLdata(CallAC)
            Call Calculate_DSWL_Flex()

            CalculateACR.libACR(gCat) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
            Flex_Total_Thick = Flex_Thick_Func(LEAStrActiveX)
            CalculateACR.libACRthick(gCat) = CSng(Flex_Total_Thick)
            'Call Print_Results_Flex(gross_weight, CallAC, CopyAC, LEAStrActiveX)



            gTimeSave2 = timeGetTime
            ETimemsecs = CSng((gTimeSave2 - gTimeSave1) / 1000)

            Date1 = Year(Now) & "." & Format(Month(Now), "00") & "." & Format(Day(Now), "00")
            'FileName = WDir1 & "\ACN_" & Date1 & FileExt
            FileName = WDir1 & "\ACR_C_130.txt"

            FileOpen(9, FileName, OpenMode.Append) 'ACN_
            'PrintLine(9, "")

            For i As Integer = 4 To 4
                Print(9, LPad(4, Format(i, "###")))
                Print(9, LPad(8, Format(CoordY(2), "##0.00")))
                Print(9, LPad(11, Format(CalculateACR.libACR(i), "###.00000")))
                PrintLine(9, LPad(13, Format(CalculateACR.libACRthick(i), "###.000000")))
                'PrintLine(9, LPad(7, Format(LEAStrActiveX.InterfaceParm(i), "###.00")))
            Next

            'PrintLine(9, "")
            'PrintLine(9, LPad(12, CStr(Format(ETimemsecs, "##,##0.000"))) & " sec.")
            'PrintLine(9, LPad(12, CStr(Format(ETimemsecs / 60, "##,##0.000"))) & " min.")
            'PrintLine(9, "============================")
            'PrintLine(9, "")
            FileClose(9)



        Next

        Exit Sub

        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        Dim iCat As Integer

        For is1 = gross_weight To gross_weight + 20000 Step 2000

            For iCat = 1 To 1

                gCat = iCat
                'CallAC(1).GearLoad = gross_weight * percent_gw
                CallAC(1).GearLoad = is1 * percent_gw
                CallAC(1).NTires = wheels_number
                Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
                Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry

                If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Loop
                    Call Set_Eval_Points_XYsym(CallAC)
                Else
                    Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
                End If

                If True Then
                    LEAStrActiveX.NLayers = 3
                    Call RedimArraysRigid(LEAStrActiveX.NLayers, iCat, LCode, LEAStrActiveX)
                    LEAStrActiveX.Thick(1) = 5 'P-501
                    LEAStrActiveX.Thick(2) = 20 / 2.54 'P-209  (200 mm to inches)

                ElseIf wheels_number >= 4 And (iCat = 1 Or iCat = 2 Or iCat = 3) Then
                End If

                'Call Evaluate_Stress_Rigid(LEAStrActiveX, CallAC(1)) 'Evaluate stress
                'LEAStrActiveX.Thick(1) = 2
                'Call Calculate_Stress_Rigid()
                'gStress = gStress
                'Call Eval_Stress_Thick()


                Call Make_CallAC_Copy(CallAC(1), CopyAC(1))

                Call Calculate_Thickness_Rigid()

                Exit For



                If LEAStrActiveX.Thick(1) = 2 Then
                    If gStress < (StressACN - 1) Then
                        CalculateACR.libACR(iCat) = 0
                        CalculateACR.libACRthick(iCat) = 0
                        'Call Print_Results_Rigid(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                        Continue For
                    End If
                End If

                Call Set_SWLdata(CallAC)
                Call Calculate_DSWL_Rigid()

                CalculateACR.libACR(iCat) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
                CalculateACR.libACRthick(iCat) = CSng(LEAStrActiveX.Thick(1))
                'Call Print_Results_Rigid(gross_weight)
            Next

        Next is1

    End Sub


    Private Sub ZZZ_aluation_Strain(ByVal PavementType As clsACR.PavementType,
                 ByVal gross_weight As Single,
                 ByVal percent_gw As Single,
                 ByVal wheels_number As Integer,
                 ByVal tire_pressure As Single,
                 ByVal CoordX() As Single,
                 ByVal CoordY() As Single)

        gTimeSave1 = timeGetTime

        gPrintOutput = True
        If gPrintOutput Then
            WDir1 = SpecialDirectories.MyDocuments + "\My FAARFIELD\ACR_Results_" & PavementType
            If Directory.Exists(WDir1) Then
            Else
                System.IO.Directory.CreateDirectory(WDir1)
            End If
        End If

        gCat = 3

        CallAC(1).GearLoad = gross_weight * percent_gw
        CallAC(1).NTires = wheels_number

        Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
        Call GearCG(CoordX, CoordY, wheels_number)         'Check gear symmetry!!!

        If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Strain
            Call Set_Eval_Points_XYsym(CallAC)
        Else
            Call Set_Eval_Points_Full_Mesh_Div1(CallAC)
        End If

        LEAStrActiveX.NLayers = 4
        Call RedimArraysFlexible(LEAStrActiveX.NLayers, gCat, LCode, LEAStrActiveX)

        LEAStrActiveX.Thick(1) = 3 'P-401
        LEAStrActiveX.Thick(2) = 6 'P-209
        LEAStrActiveX.Thick(3) = 0.001 'P-154 variable


        'Call Set_Thick_Values(gCat)
        Call Calculate_Flex_Coverages()
        CovGear = CSng(NtoFail)
        StrainMaxGear = StrainMax
        iMaxGear = iMax

        If Symmetry = SymmetryType.XYSymmetry Then 'Evaluation_Strain
            Call Print_Strain_Array_XYsym(True, LEAStrActiveX, StrainsPrint, CallAC)
        ElseIf Symmetry = SymmetryType.YSymmetry Then
            Call Print_Strain_Array_XYsym(True, LEAStrActiveX, StrainsPrint, CallAC)
        Else
            Call Print_Strain_Array_Full_Mesh(LEAStrActiveX)
        End If


    End Sub


    '09
    Public Overloads Function CalculateACR(ByVal PavementType As clsACR.PavementType,
                ByVal gross_weight As Single,
                ByVal percent_gw As Single,
                ByVal wheels_number As Integer,
                ByVal tire_pressure As Single,
                ByVal CoordX() As Single,
                ByVal CoordY() As Single,
                ByVal percent_gw2 As Single,
                ByVal wheels_number2 As Integer,
                ByVal tire_pressure2 As Single,
                ByVal CoordX2() As Single,
                ByVal CoordY2() As Single,
                ByVal modul1 As Single,
                ByVal SW1() As Integer,
                ByVal SW2() As Integer) As ACRdata 'ACN for two gears



        gICAOCodeIndex = ICAOCodeIndexF(modul1)
        start_cat = gICAOCodeIndex
        end_cat = start_cat

        bSelectWheels = True
        ReDim gSW(wheels_number + wheels_number2)
        For i1 As Integer = 1 To wheels_number
            gSW(i1) = SW1(i1)
        Next

        For i1 As Integer = 1 + wheels_number To wheels_number + wheels_number2
            gSW(i1) = SW2(i1 - wheels_number)
        Next

        CalculateACR = CalculateACR(PavementType, gross_weight,
                                    percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                    percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)

    End Function


    '07Metric
    Public Overloads Function CalculateACR(ByVal PavementType As clsACR.PavementType,
            ByVal gross_weight As Single,
            ByVal percent_gw As Single,
            ByVal wheels_number As Integer,
            ByVal tire_pressure As Single,
            ByVal CoordX() As Single,
            ByVal CoordY() As Single,
            ByVal percent_gw2 As Single,
            ByVal wheels_number2 As Integer,
            ByVal tire_pressure2 As Single,
            ByVal CoordX2() As Single,
            ByVal CoordY2() As Single,
            ByVal SW1() As Integer,
            ByVal SW2() As Integer,
            ByVal Metric As Boolean) As ACRdata


        If Metric Then ' convert Metric input to US units
            gross_weight = CSng(gross_weight * 2204.6225) ' to tonnes
            tire_pressure = CSng(tire_pressure / 6.894757) ' to psi

            For ik As Integer = 1 To wheels_number
                CoordX(ik) = CSng(CoordX(ik) * 25.4)
                CoordY(ik) = CSng(CoordY(ik) * 25.4)
            Next

            tire_pressure2 = CSng(tire_pressure2 / 6.894757) ' to psi

            For ik As Integer = 1 To wheels_number
                CoordX2(ik) = CSng(CoordX2(ik) * 25.4)
                CoordY2(ik) = CSng(CoordY2(ik) * 25.4)
            Next

        End If


        CalculateACR = CalculateACR(PavementType, gross_weight,
                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                        percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2, SW1, SW2)


        If Metric Then ' convert US units to Metric
            For ik As Integer = 1 To 4
                CalculateACR.libACRthick(ik) = CSng(CalculateACR.libACRthick(ik) * 25.4)
            Next
        End If


    End Function






    '07
    Public Overloads Function CalculateACR(ByVal PavementType As clsACR.PavementType,
            ByVal gross_weight As Single,
            ByVal percent_gw As Single,
            ByVal wheels_number As Integer,
            ByVal tire_pressure As Single,
            ByVal CoordX() As Single,
            ByVal CoordY() As Single,
            ByVal percent_gw2 As Single,
            ByVal wheels_number2 As Integer,
            ByVal tire_pressure2 As Single,
            ByVal CoordX2() As Single,
            ByVal CoordY2() As Single,
            ByVal SW1() As Integer,
            ByVal SW2() As Integer) As ACRdata 'ACR for two gears

        bSelectWheels = True

        ReDim gSW(wheels_number + wheels_number2)

        For i1 As Integer = 1 To wheels_number
            gSW(i1) = SW1(i1)
        Next

        For i1 As Integer = 1 + wheels_number To wheels_number + wheels_number2
            gSW(i1) = SW2(i1 - wheels_number)
        Next

        CalculateACR = CalculateACR(PavementType, gross_weight,
                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                        percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)




    End Function

    '33          2 gears 4 ACNs
    Public Overloads Function CalculateACR(
                ByVal PavementType As clsACR.PavementType,
                ByVal gross_weight As Single,
                ByVal percent_gw As Single,
                ByVal wheels_number As Integer,
                ByVal tire_pressure As Single,
                ByVal CoordX() As Single,
                ByVal CoordY() As Single,
                ByVal percent_gw2 As Single,
                ByVal wheels_number2 As Integer,
                ByVal tire_pressure2 As Single,
                ByVal CoordX2() As Single,
                ByVal CoordY2() As Single,
                ByVal Metric As Boolean) As ACRdata

        If Metric Then ' convert Metric input to US units
            gross_weight = CSng(gross_weight * 2204.6225) ' to tonnes
            tire_pressure = CSng(tire_pressure / 6.894757) ' to psi

            For ik As Integer = 1 To wheels_number
                CoordX(ik) = CSng(CoordX(ik) * 25.4)
                CoordY(ik) = CSng(CoordY(ik) * 25.4)
            Next

            tire_pressure2 = CSng(tire_pressure2 / 6.894757) ' to psi

            For ik As Integer = 1 To wheels_number
                CoordX2(ik) = CSng(CoordX2(ik) * 25.4)
                CoordY2(ik) = CSng(CoordY2(ik) * 25.4)
            Next

        End If


        CalculateACR = CalculateACR(PavementType, gross_weight,
                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                        percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)


        If Metric Then ' convert US units to Metric
            For ik As Integer = 1 To 4
                CalculateACR.libACRthick(ik) = CSng(CalculateACR.libACRthick(ik) * 25.4)
            Next
        End If


    End Function






    '06          2 gears 4 ACNs FF Monday 2017.06.12
    Public Overloads Function CalculateACR(ByVal PavementType As clsACR.PavementType,
                ByVal gross_weight As Single,
                ByVal percent_gw As Single,
                ByVal wheels_number As Integer,
                ByVal tire_pressure As Single,
                ByVal CoordX() As Single,
                ByVal CoordY() As Single,
                ByVal percent_gw2 As Single,
                ByVal wheels_number2 As Integer,
                ByVal tire_pressure2 As Single,
                ByVal CoordX2() As Single,
                ByVal CoordY2() As Single) As ACRdata 'ACN for two gears

        gPrintOutput = True
        gPrintOutput_ACN = True
        gPrintOutput_Responses = True

        gTandemFnew = True 'CalculateACR Function 2 gears
        gPavementType = PavementType

        gTimeSave1 = timeGetTime
        ReDim CalculateACR.libACR(end_cat - start_cat + 1)
        ReDim CalculateACR.libACRthick(end_cat - start_cat + 1)
        ReDim CalculateACR.libSubCat(end_cat - start_cat + 1)
        ReDim CalculateACR.libSubCatMPa(end_cat - start_cat + 1)

        If end_cat = start_cat Then
            ICAOCodeIndex = gICAOCodeIndex
        Else
            ICAOCodeIndex = 0
        End If


        Call Check_for_Subdirectory()
        If PavementType = clsACR.PavementType.Rigid Then
            GoTo RigidPavements
        End If


        '************************************************
        '*****          Flexible Pavements          *****
        '************************************************
        For iCat As Integer = start_cat To end_cat
            gCat = iCat

            Dim C_X(wheels_number + wheels_number2) As Single
            Dim C_Y(wheels_number + wheels_number2) As Single

            ReDim gC_X(wheels_number + wheels_number2)
            ReDim gC_Y(wheels_number + wheels_number2)

            Call Combine_Coord(CoordX, CoordY, CoordX2, CoordY2, C_X, C_Y)

            For i5 As Integer = 1 To wheels_number + wheels_number2
                gC_X(i5) = C_X(i5)
                gC_Y(i5) = C_Y(i5)
            Next

            CallAC(1).GearLoad = gross_weight * percent_gw
            CallAC(1).NTires = wheels_number
            Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
            CallAC_belly(1).GearLoad = gross_weight * percent_gw2 'Belly
            CallAC_belly(1).NTires = wheels_number2
            Call Set_TireDataForAC_belly(CoordX2, CoordY2, tire_pressure2, CallAC_belly)
            Call GearCG(C_X, C_Y, wheels_number + wheels_number2) 'Check symmetry


            'If (iSelectEval_ACN = 0) Or (iSelectEval_ACN = 2) Or (iSelectEval_ACN = 3) Then 'Flex
            '    Call Create_Eval_Points_2gears(CallAC, C_X, C_Y) '2 gears
            'Else
            '    Call Create_Eval_Points_Under_Wheels(CallAC)
            'End If

            Call Get_Center_of_Gravity_2gears(CallAC, CallAC_belly, gXcg, gYcg)
            Call Create_Eval_Points_2gears(CallAC, C_X, C_Y) '2 gears flexible

            Call Print_Evaluation_Points_2gears(CallAC, CallAC_belly) 'Flexible

            Dim Dim1 As Integer
            Dim1 = UBound(CallAC(1).EvalX, 1)
            CallAC_belly(1).NEvalPoints = Dim1
            ReDim CallAC_belly(1).EvalX(CallAC_belly(1).NEvalPoints)
            ReDim CallAC_belly(1).EvalY(CallAC_belly(1).NEvalPoints)

            For i = 1 To Dim1
                CallAC_belly(1).EvalX(i) = CallAC(1).EvalX(i)
                CallAC_belly(1).EvalY(i) = CallAC(1).EvalY(i)
            Next


            LEAStrActiveX.NLayers = 3
            Call RedimArraysFlexible(LEAStrActiveX.NLayers, iCat, LCode, LEAStrActiveX)

            If (wheels_number + wheels_number2) <= 2 Then
                LEAStrActiveX.Thick(1) = 3 'P-401
                If iCat = 1 Then
                    LEAStrActiveX.Thick(2) = 27 'P-209 variable
                Else
                    If gThickness3 = 0 Then gThickness3 = 23
                    LEAStrActiveX.Thick(2) = gThickness3 * 0.8
                    If LEAStrActiveX.Thick(2) < 1 Then LEAStrActiveX.Thick(2) = 1
                End If
            Else 'wheels_number > 2
                LEAStrActiveX.Thick(1) = 5 'P-401
                If iCat = 1 Then
                    LEAStrActiveX.Thick(2) = 30 'P-209 variable
                Else
                    If LEAStrActiveX.Thick(2) = 0 Then gThickness3 = 20
                    LEAStrActiveX.Thick(2) = gThickness3 * 0.8
                    If LEAStrActiveX.Thick(2) < 1 Then LEAStrActiveX.Thick(2) = 1
                End If
            End If

            Call Make_CallAC_Copy(CallAC(1), CopyAC(1))
            Call Make_CallAC_Copy(CallAC_belly(1), CopyAC_belly(1))

            'LEAStrActiveX.Thick(2) = 40.28
            Call Calculate_Thickness_Flex_2gears(LEAStrActiveX.NLayers - 1) 'iterate on layer on subgrade
            'Call Calculate_Thickness_Flex_2gears(LEAStrActiveX.NLayers - 1) 'iterate on layer on subgrade

            If LEAStrActiveX.Thick(LEAStrActiveX.NLayers - 1) = 1 Then
                If NtoFail > (CovACN + 10) Then

                    If UBound(CalculateACR.libACR, 1) = 1 Then
                        CalculateACR.libACR(1) = 0
                        CalculateACR.libACRthick(1) = 0
                    Else
                        CalculateACR.libACR(iCat) = 0
                        CalculateACR.libACRthick(iCat) = 0
                    End If

                    'CalculateACR.libACR(iCat) = 0
                    'CalculateACR.libACRthick(iCat) = 0
                    'Call Print_Results_Flex_2gears(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                    Continue For
                End If
            End If

            Call Set_SWLdata(CallAC)
            Call Calculate_DSWL_Flex()

            CalculateACR.libACR(iCat - start_cat + 1) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
            CalculateACR.libSubCat(iCat - start_cat + 1) = Cat(iCat)
            CalculateACR.libSubCatMPa(iCat - start_cat + 1) = CatMpa(iCat)

            Flex_Total_Thick = Flex_Thick_Func(LEAStrActiveX)
            CalculateACR.libACRthick(iCat - start_cat + 1) = CSng(Flex_Total_Thick)
            'Call Print_Results_Flex_2gears(gross_weight, CallAC, CopyAC, LEAStrActiveX)

        Next

        GoTo PrintACN

RigidPavements:

        '************************************************
        '*****             Rigid Pavements          *****
        '************************************************
        For iCat As Integer = start_cat To end_cat
            gCat = iCat

            Dim C_X(wheels_number + wheels_number2) As Single
            Dim C_Y(wheels_number + wheels_number2) As Single
            Call Combine_Coord(CoordX, CoordY, CoordX2, CoordY2, C_X, C_Y)

            CallAC(1).GearLoad = gross_weight * percent_gw
            CallAC(1).NTires = wheels_number
            Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)
            CallAC_belly(1).GearLoad = gross_weight * percent_gw2 'Belly
            CallAC_belly(1).NTires = wheels_number2
            Call Set_TireDataForAC_belly(CoordX2, CoordY2, tire_pressure2, CallAC_belly)
            Call GearCG(C_X, C_Y, wheels_number + wheels_number2) 'Check symmetry

            CallAC(1).NEvalPoints = 0 : ReDim CallAC(1).EvalX(0) : ReDim CallAC(1).EvalY(0)

            If (iSelectEval_ACN = 2) Or (iSelectEval_ACN = 3) Then
                Call Create_Eval_Points_2gears(CallAC, C_X, C_Y) '2 gears rigid
            Else
                Call Create_Eval_Points_Under_Wheels_2gears(CallAC, C_X, C_Y)
            End If

            Call Print_Evaluation_Points_2gears(CallAC, CallAC_belly) 'Rigid

            'CallAC(1).NEvalPoints = 1
            'ReDim CallAC(1).EvalX(CallAC(1).NEvalPoints)
            'ReDim CallAC(1).EvalY(CallAC(1).NEvalPoints)
            'CallAC(1).EvalX(1) = -250
            'CallAC(1).EvalY(1) = -20

            Dim Dim1 As Integer
            Dim1 = UBound(CallAC(1).EvalX, 1)
            CallAC_belly(1).NEvalPoints = Dim1
            ReDim CallAC_belly(1).EvalX(CallAC_belly(1).NEvalPoints)
            ReDim CallAC_belly(1).EvalY(CallAC_belly(1).NEvalPoints)

            For i = 1 To Dim1
                CallAC_belly(1).EvalX(i) = CallAC(1).EvalX(i)
                CallAC_belly(1).EvalY(i) = CallAC(1).EvalY(i)
            Next



            If True Then
                LEAStrActiveX.NLayers = 3
                Call RedimArraysRigid(LEAStrActiveX.NLayers, iCat, LCode, LEAStrActiveX)
                LEAStrActiveX.Thick(1) = 5 'P-501
                LEAStrActiveX.Thick(2) = 20 / 2.54 'P-209  (200 mm to inches)

            ElseIf wheels_number >= 4 And (iCat = 1 Or iCat = 2 Or iCat = 3) Then
            End If


            Call Make_CallAC_Copy(CallAC(1), CopyAC(1))
            Call Make_CallAC_Copy(CallAC_belly(1), CopyAC_belly(1))
            'Call Calculate_Thickness_Rigid()
            'Call TwoGears.Calculate_Rigid_Stress_2gears(CallAC, CallAC_belly)
            Call Calculate_Thickness_Rigid_2gears(CallAC, CallAC_belly)


            If LEAStrActiveX.Thick(1) = 2 Then
                If gStress < (StressACN - 1) Then

                    If UBound(CalculateACR.libACR, 1) = 1 Then
                        CalculateACR.libACR(1) = 0
                        CalculateACR.libACRthick(1) = 0
                    Else
                        CalculateACR.libACR(iCat) = 0
                        CalculateACR.libACRthick(iCat) = 0
                    End If

                    'CalculateACR.libACR(iCat) = 0
                    'CalculateACR.libACRthick(iCat) = 0

                    'Call Print_Results_Rigid_2gears(gross_weight, CallAC, CopyAC, LEAStrActiveX)
                    Continue For
                End If
            End If

            Call Set_SWLdata(CallAC)
            Call Calculate_DSWL_Rigid()

            CalculateACR.libACR(iCat - start_cat + 1) = CSng(2 * CallAC(1).GearLoad * lbsTokg / c1000)
            CalculateACR.libSubCat(iCat - start_cat + 1) = Cat(iCat)
            CalculateACR.libSubCatMPa(iCat - start_cat + 1) = CatMpa(iCat)

            CalculateACR.libACRthick(iCat - start_cat + 1) = CSng(LEAStrActiveX.Thick(1))
            'Call Print_Results_Rigid_2gears(gross_weight, CallAC, CopyAC, LEAStrActiveX)

        Next

PrintACN:

        'Call Print_ACNtoFile_2gears(CopyAC, CopyAC_belly, CalculateACR)

        Call ResetValues()

    End Function

    Private Sub ZZZ_Evaluation_Loop_Flexible(ByVal PavementType As clsACR.PavementType,
     ByVal gross_weight As Single,
     ByVal percent_gw As Single,
     ByVal wheels_number As Integer,
     ByVal tire_pressure As Single,
     ByVal CoordX() As Single,
     ByVal CoordY() As Single)

        Dim CalculateACR As ACRdata

        gTimeSave1 = timeGetTime
        ReDim CalculateACR.libACR(4)
        ReDim CalculateACR.libACRthick(4)

        gTandemFnew = False
        'gPavementType = "Flexible"
        'gPavementType = "Rigid"
        gPavementType = clsACR.PavementType.Flexible
        gPavementType = clsACR.PavementType.Rigid
        gPavementType = PavementType

        Call Check_for_Subdirectory()
        gCat = 1

        CallAC(1).GearLoad = gross_weight * percent_gw
        CallAC(1).NTires = wheels_number
        Call Set_TireDataForAC(CoordX, CoordY, tire_pressure, CallAC)

        'Call Set_Eval_Points_Rigid(CallAC)
        'CallAC(1).TirePress(2) = 10
        'CallAC(1).TirePress(3) = 10
        'CallAC(1).TirePress(4) = 10


        Call GearCG(CoordX, CoordY, wheels_number) 'Check symmetry
        'Call Set_Eval_Points_Dim(CallAC)

        Call Set_Eval_Points_Dim_2(CallAC)

        'CallAC(1).NEvalPoints = 1
        'ReDim CallAC(1).EvalX(1)
        'ReDim CallAC(1).EvalY(1)
        'CallAC(1).EvalX(1) = -250
        'CallAC(1).EvalY(1) = -20


        LEAStrActiveX.NLayers = 3
        If gPavementType = clsACR.PavementType.Flexible Then
            Call RedimArraysFlexible(LEAStrActiveX.NLayers, gCat, LCode, LEAStrActiveX)
        Else
            Call RedimArraysRigid(LEAStrActiveX.NLayers, gCat, LCode, LEAStrActiveX)
        End If


        'For flexible
        LEAStrActiveX.Thick(1) = 5 'P-401
        LEAStrActiveX.Thick(2) = 30 'P-209 variable



        'For rigid
        LEAStrActiveX.Thick(1) = 5 'P-401
        LEAStrActiveX.Thick(2) = 30 'P-209 variable

        Call Make_CallAC_Copy(CallAC(1), CopyAC(1))
        'Call Calculate_Thickness_Flex(LEAStrActiveX.NLayers - 1) 'iterate on layer on subgrade

        If gPavementType = clsACR.PavementType.Flexible Then
            Call Calculate_Flex_Coverages()
        Else
            Call Calculate_Rigid_Stress()
        End If



        'gPrintOutput_Responses = True 'FF 2017.07.14 Friday
        Call Print_Array_Full_Mesh(LEAStrActiveX, Response, CallAC)

    End Sub


    '08      2 gears 1 ACNs   Monday 2017.06.12
    Public Overloads Function CalculateACR(ByVal PavementType As clsACR.PavementType,
     ByVal gross_weight As Single,
     ByVal percent_gw As Single,
     ByVal wheels_number As Integer,
     ByVal tire_pressure As Single,
     ByVal CoordX() As Single,
     ByVal CoordY() As Single,
     ByVal percent_gw2 As Single,
     ByVal wheels_number2 As Integer,
     ByVal tire_pressure2 As Single,
     ByVal CoordX2() As Single,
     ByVal CoordY2() As Single,
     ByVal modul1 As Single) As ACRdata

        Try

            'CalculateACR.libACR(1) = 33
            'CalculateACR.libACRthick(1) = 55
            gICAOCodeIndex = ICAOCodeIndexF(modul1)
            start_cat = gICAOCodeIndex
            end_cat = start_cat

            CalculateACR = CalculateACR(PavementType, gross_weight,
                                        percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                        percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Function


    Private Sub Z_Evaluation_Loop(ByVal PavementType As String,
     ByVal gross_weight As Single,
     ByVal percent_gw As Single,
     ByVal wheels_number As Integer,
     ByVal tire_pressure As Single,
     ByVal CoordX() As Single,
     ByVal CoordY() As Single,
     ByVal percent_gw2 As Single,
     ByVal wheels_number2 As Integer,
     ByVal tire_pressure2 As Single,
     ByVal CoordX2() As Single,
     ByVal CoordY2() As Single)

        Dim CalculateACR As ACRdata

        gTimeSave1 = timeGetTime
        ReDim CalculateACR.libACR(4)
        ReDim CalculateACR.libACRthick(4)

    End Sub








    Private Sub ZZZ_PrintOut01_GL()

        gPrintOutput_ACN = True
        Call Check_for_Subdirectory()

        Dim FNo As Integer
        FNo = FreeFile()
        Dim FileName As String
        Dim th7 As Double
        FileName = WDir1 & "\PCC222_GGGGG_22" & FileExt
        FileOpen(FNo, FileName, OpenMode.Append)

        Dim i7 As Single
        'For i7 = 1 To 21 Step 1
        For i7 = 35000 To 115000.02 Step 5000
            CallAC(1).GearLoad = i7
            Call Calculate_Flex_Coverages()
            PrintLine(FNo, LPad(4, CStr(i7)), TAB(7), LPad(22, Format(NtoFail, "#,##0.00")) &
                      "    " & LEAStrActiveX.Modulus(julNPLayers))
        Next
        FileClose(FNo)

    End Sub


    Friend Sub ResetValues()

        CovACN = 36500 ' 36500
        CoverageDelta = 1
        StressACN = 2.75 * MPaToPsi
        'StressDelta = 0.4
        StressDelta = StressACN / 1000

        start_cat = 1 : end_cat = 4
        div1 = 4
        gPrintOutput = False
        gPrintOutput_ACN = True 'Update111
        gPrintOutput_Responses = False
        gPrint1800 = False
        bMeshSize = True 'new123
        gMeshSize = 10 / 2.54 'new123
        bNewDiv1 = False
        iSelectEval_ACN = 0
        bSelectWheels = False

        horE_Limit = 0
        vertN_Limit = 0
        horStep = 0
        vertStep = 0
        gMeshSizeCount = 0

    End Sub






End Class
