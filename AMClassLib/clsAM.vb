' change "Single" to "Double", "CSng" to "CDbl" for FAASR3D by YC 102418-041519

Option Strict On
Option Explicit On

Imports System.IO
Imports System.Threading

Public Class clsAM
    Public Event AMInitialized()
    Public Event AMStopped()
    Public gCalcStress(80) As Integer
    Public rgdFile As String
    Public ingFile As String
    Public WorkingDir0 As String    'moved by YGC 112213
    Public gearOrient As Short
    Public ModelOut As Integer
    Public NoOutFiles As Boolean        'QW 09-15-2019

    Public Sub ComputeResponse(
        ByVal ResponseType As LEAFClassLib.clsLEAF.LEAFoptions,
        ByVal NACarg As Integer,
        ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
        ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
        ByRef Response(,) As Double,
        ByRef VertStress(,) As Double, ByRef VertCoord() As Double,
        ByRef AllResps(,) As LEAFClassLib.clsLEAF.LEAFAllResponses,
        ByRef CalcStress() As Integer,
        ByVal DesignType2 As Integer,
        ByVal SolverType2 As Integer,
        ByVal SlabMeshSize2 As Integer,
        ByVal JobName As String,
        ByRef UserInterrupted As Boolean, ByRef NoOutFiles As Boolean,
        ByRef ct As CancellationToken)
        Dim SymmetryChecker(,) As Double 'Ali.D 20220224
        Dim NTireCounter As Integer
        'added "ByRef VertStress(,) As Double, ByRef VertCoord() As Double, _" to transfer vertical stress for rigid compaction by YGC 112213 
        'added "ByVal SolverType2 As Integer " for solver choice by YGC 083012
        'added "ByVal SlabMeshSize2 As Integer" for slab mesh size selection by YGC 061113
        'subroutine for Multiple aircraft in 1 step, automated by YGC 031312
        Dim ACcopy() As LEAFClassLib.clsLEAF.LEAFACParms
        Dim s1 As Integer
        s1 = UBound(LEAAircraft)
        ReDim ACcopy(s1)

        Call MakeCopy(LEAAircraft, ACcopy, s1)

        If NoOutFiles = False Then          'QW 09-15-2019
            ModelOut = 1
        Else
            ModelOut = 0
        End If

        Try

            'Exit Sub

            gDesignType2 = DesignType2
            'LEAAircraft(1).NTires = 2
            'LEAAircraft(1).GearLoad = LEAAircraft(1).GearLoad * LEAAircraft(1).NTires / 14

            Dim I As Short, ACIndex As Short
            Dim EdgeStressFEM0PCC(gAC), EdgeStressFEM90PCC(gAC) As Double
            Dim EdgeStressFEM0Overlay(gAC), EdgeStressFEM90Overlay(gAC) As Double
            Dim InteriorStressLEAF, InteriorStressFEM(gAC) As Double
            Dim InteriorStressLEAFOverlay, InteriorStressFEMOverlay(gAC) As Double

            'Dim MaxStressPCC(gAC), MaxStressOverlay(gAC) As Double
            Dim PrintResults As Boolean
            Dim DesignType As String


            Dim t1 As Date, tdiff As Long
            Dim ThinLayer As Boolean

            Dim GearOrientation(gAC * 2) As Short 'added to define aircraft orientation by YGC 102213

            'DesignType2 = 1

            'Public Const NewFlex As Short = 1
            'Public Const FlexOnFlex As Short = 2
            'Public Const PCCOnFlex As Short = 3

            'Public Const NewRigid As Short = 10
            'Public Const UnbondOnRigid As Short = 11
            'Public Const PartBondOnRigid As Short = 12
            'Public Const FlexOnRigid As Short = 13

            SolverType = SolverType2 'added for solver choice by YGC 083012
            SlabMeshSize = SlabMeshSize2 'added for slab mesh size selection by YGC 061113

            gearOrient = CShort(LEAStructure.lngDummy(1))


            'Izydor Kawa 10/16/2012
            'Dim MyDoc1 As System.Environment.SpecialFolder = Environment.SpecialFolder.MyDocuments
            rgdFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.rgd"
            ingFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.ing"


            ThinLayer = False
            If DesignType2 = 10 Then

            ElseIf DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                If LEAStructure.NLayers = 3 Then
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(4)
                    ReDim Preserve LEAStructure.Poisson(4)
                    ReDim Preserve LEAStructure.Thick(4)
                    ReDim Preserve LEAStructure.InterfaceParm(4)

                    LEAStructure.Modulus(4) = LEAStructure.Modulus(3)

                    LEAStructure.Thick(4) = LEAStructure.Thick(3)
                    LEAStructure.Thick(3) = 4

                    LEAStructure.InterfaceParm(4) = LEAStructure.InterfaceParm(3)
                    'LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(2)

                    LEAStructure.Poisson(4) = LEAStructure.Poisson(3)
                    'LEAStructure.Poisson(3) = 0.35
                    ThinLayer = True

                End If
            End If



            'Call INGRIDMAIN()
            'Call INGRID()

            For I = 1 To CShort(NACarg)
                gCalcStress(I) = CalcStress(I)
            Next

            'LEAAircraft(2).GearLoad = 999
            'Exit Sub

            If UserInterrupted Then Exit Sub 'September 13, 2006

            gUserInterrupted = UserInterrupted
            gDesignType = DesignType2
            gNACarg = NACarg
            PrintResults = False
            Overlay = False
            ReDim NCat(gMeshCategories)
            ReDim firstACCat(gMeshCategories)
            ReDim GroupIndex(gAC * 2)

            '.InterfaceParm(1) = 1 And .InterfaceParm(1) = 0 FlexOnRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(1) = 1 NewRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(2) = 0 UnbondOnRigid
            '.InterfaceParm(1) > 0 And .InterfaceParm(1) < 1 PartBondOnRigid

            If LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "FlexOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 1 Then
                DesignType = "NewRigid"
                Overlay = False
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "UnbondOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) > 0 And LEAStructure.InterfaceParm(1) < 1 Then
                DesignType = "PartBondOnRigid"
                Overlay = True



            End If


            If Overlay Then
                ReDim Response(NACarg, 2)
            Else
                ReDim Response(NACarg, 8)
            End If

            PrintResults = False
            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                PrintLine(3, "NIKE3D   " _
                    & LPad(13, "Aircraft") _
                    & LPad(10, "Edge0") _
                    & LPad(10, "Edge90") _
                    & LPad(10, "IntFEM") _
                    & LPad(10, "MaxS") _
                    & LPad(10, "IntLEAF") _
                    & LPad(12, "Thick= " _
                    & VB6.Format(LEAStructure.Thick(1), "#0.0000")) _
                    & LPad(22, CStr(Now)))
                FileClose(3)
            End If

            t1 = Now()
            System.Windows.Forms.Application.DoEvents()



            For ACIndex = 1 To CShort(NACarg)
                Dim B747New As Boolean


                B747New = (InStr(LEAAircraft(ACIndex).ACname, "747", CompareMethod.Text) > 0) _
                And LEAAircraft(ACIndex).NTires = 4 And LEAAircraft(ACIndex).libGear = "Z"




                If CalcStress(ACIndex) = 0 Then
                    If LEAAircraft(ACIndex).ACname = "C-5" Then
                        LEAAircraft(ACIndex).NTires = 6
                        LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex).GearLoad / 2
                    End If


                    ''''If LEAAircraft(ACIndex).ACname = "C-130" Then
                    ''''    Dim x1, x2, y1, y2 As Double
                    ''''    x1 = CDbl(LEAAircraft(ACIndex).TireX(1))
                    ''''    x2 = CDbl(LEAAircraft(ACIndex).TireX(2))
                    ''''    y1 = CDbl(LEAAircraft(ACIndex).TireY(1))
                    ''''    y2 = CDbl(LEAAircraft(ACIndex).TireY(2))
                    ''''    LEAAircraft(ACIndex).TireX(1) = y1
                    ''''    LEAAircraft(ACIndex).TireX(2) = y2
                    ''''    LEAAircraft(ACIndex).TireY(1) = x1
                    ''''    LEAAircraft(ACIndex).TireY(2) = x2
                    ''''End If



                    If LEAAircraft(ACIndex).libGear = "A" _
                       Or LEAAircraft(ACIndex).libGear = "B" _
                       Or LEAAircraft(ACIndex).libGear = "D" _
                       Or (LEAAircraft(ACIndex).libGear = "HB" And
                       LEAAircraft(ACIndex).NTires = 2) Then
                        NCat(1) += 1S
                        GroupIndex(ACIndex) = 1
                        If firstACCat(1) = 0 Then
                            firstACCat(1) = ACIndex
                        End If

                        GearOrientation(ACIndex) = 0   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).libGear = "F" _
                          Or LEAAircraft(ACIndex).libGear = "E" _
                          Or LEAAircraft(ACIndex).libGear = "H" _
                          Or LEAAircraft(ACIndex).libGear = "WFBF" _
                          Or LEAAircraft(ACIndex).libGear = "WFBN" _
                          Or LEAAircraft(ACIndex).ACname = "IL86" _
                          Or (LEAAircraft(ACIndex).libGear = "HB" And
                           LEAAircraft(ACIndex).NTires = 4) _
                           Or B747New Then

                        'changed for symmetrical gear by YC 082216 092016
                        'NCat(2) += 1S
                        'GroupIndex(ACIndex) = 2
                        'If firstACCat(2) = 0 Then
                        '    firstACCat(2) = ACIndex
                        'End If
                        NCat(1) += 1S
                        GroupIndex(ACIndex) = 1
                        If firstACCat(1) = 0 Then
                            firstACCat(1) = ACIndex
                        End If
                        'changed for symmetrical gear by YC 082216 092016 END

                        GearOrientation(ACIndex) = CShort(LEAStructure.lngDummy(1))   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).libGear = "N" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" Or
                         (Mid(LEAAircraft(ACIndex).ACname, 1, 4) = "A380" And LEAAircraft(ACIndex).NTires = 6) Then

                        'changed for symmetrical gear by YC 082216 092016
                        'NCat(3) += 1S
                        'GroupIndex(ACIndex) = 3
                        'If firstACCat(3) = 0 Then
                        '    firstACCat(3) = ACIndex
                        'End If
                        NCat(1) += 1S
                        GroupIndex(ACIndex) = 1
                        If firstACCat(1) = 0 Then
                            firstACCat(1) = ACIndex
                        End If
                        'changed for symmetrical gear by YC 082216 092016 END

                        GearOrientation(ACIndex) = 90   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).libGear = "M" _
                        Or LEAAircraft(ACIndex).libGear = "K" _
                        Or LEAAircraft(ACIndex).libGear = "Z   " _
                        Or LEAAircraft(ACIndex).ACname = "IL76T" _
                        Or LEAAircraft(ACIndex).ACname = "00000" _
                        Or LEAAircraft(ACIndex).ACname = "V" Then

                        'changed for unsymmetrical gear by YC 082216 092016
                        'NCat(4) += 1S
                        'GroupIndex(ACIndex) = 4
                        'If firstACCat(4) = 0 Then
                        '    firstACCat(4) = ACIndex
                        'End If
                        NCat(2) += 1S
                        GroupIndex(ACIndex) = 2
                        If firstACCat(2) = 0 Then
                            firstACCat(2) = ACIndex
                        End If
                        'changed for unsymmetrical gear by YC 082216 092016 END

                        GearOrientation(ACIndex) = 0   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).libGear = "Z" _
                        Or LEAAircraft(ACIndex).libGear = "X" _
                        Or LEAAircraft(ACIndex).ACname = "An-124" _
                        Or LEAAircraft(ACIndex).ACname = "An-225" Then
                        ' Or LEAAircraft(ACIndex).libGear = "X" _

                        'changed for unsymmetrical gear by YC 082216 092016
                        'NCat(5) += 1S
                        'GroupIndex(ACIndex) = 5
                        'If firstACCat(5) = 0 Then
                        '    firstACCat(5) = ACIndex


                        'End If
                        ReDim SymmetryChecker(LEAAircraft(ACIndex).NTires, 2)
                        Dim J As Integer
                        Dim MaxTireX As Double = -1000000
                        Dim MaxTireY As Double = -100000
                        Dim MinTireX As Double = 100000
                        Dim MinTireY As Double = 100000
                        Dim MedTireX As Double
                        Dim MedTireY As Double
                        Dim MaxMedLenghtX As Double
                        Dim MaxMedLenghtY As Double
                        Dim MinMedLenghtX As Double
                        Dim MinMedLenghtY As Double
                        Dim SymmetryVector() As Integer
                        Dim SymmetryXGear As Boolean = False
                        Dim NotSymmetryXGear As Boolean = False
                        Dim SCounter As Integer
                        ReDim SymmetryVector(LEAAircraft(ACIndex).NTires * 2)

                        With LEAAircraft(ACIndex)

                            'For NTireCounter = 1 To LEAAircraft(ACIndex).NTires
                            '    SymmetryChecker(NTireCounter, 1) = (LEAAircraft(ACIndex).TireX(NTireCounter))
                            '    SymmetryChecker(NTireCounter, 2) = (LEAAircraft(ACIndex).TireY(NTireCounter))
                            'Next

                            For NTireCounter = 1 To LEAAircraft(ACIndex).NTires
                                If LEAAircraft(ACIndex).TireX(NTireCounter) >= MaxTireX Then
                                    MaxTireX = LEAAircraft(ACIndex).TireX(NTireCounter)
                                End If
                                If LEAAircraft(ACIndex).TireY(NTireCounter) >= MaxTireY Then
                                    MaxTireY = LEAAircraft(ACIndex).TireY(NTireCounter)
                                End If
                                If LEAAircraft(ACIndex).TireX(NTireCounter) <= MinTireX Then
                                    MinTireX = LEAAircraft(ACIndex).TireX(NTireCounter)
                                End If
                                If LEAAircraft(ACIndex).TireY(NTireCounter) <= MinTireY Then
                                    MinTireY = LEAAircraft(ACIndex).TireY(NTireCounter)
                                End If
                            Next

                            MedTireX = (MaxTireX + MinTireX) / 2
                            MedTireY = (MaxTireY + MinTireY) / 2

                            MaxMedLenghtX = MaxTireX - MedTireX
                            MaxMedLenghtY = MaxTireY - MedTireY



                            SCounter = 1

                            For NTireCounter = 1 To LEAAircraft(ACIndex).NTires

                                If LEAAircraft(ACIndex).TireX(NTireCounter) >= MedTireX And LEAAircraft(ACIndex).TireY(NTireCounter) >= MedTireY Then
                                    For J = 1 To LEAAircraft(ACIndex).NTires
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) - 2 * MaxMedLenghtX And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) - 2 * MaxMedLenghtY Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                    Next
                                End If

                                If LEAAircraft(ACIndex).TireX(NTireCounter) < MedTireX And LEAAircraft(ACIndex).TireY(NTireCounter) >= MedTireY Then
                                    For J = 1 To LEAAircraft(ACIndex).NTires
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) + 2 * MaxMedLenghtX And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) - 2 * MaxMedLenghtY Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                    Next
                                End If

                                If LEAAircraft(ACIndex).TireX(NTireCounter) < MedTireX And LEAAircraft(ACIndex).TireY(NTireCounter) < MedTireY Then
                                    For J = 1 To LEAAircraft(ACIndex).NTires
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) + 2 * MaxMedLenghtX And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) + 2 * MaxMedLenghtY Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                    Next
                                End If

                                If LEAAircraft(ACIndex).TireX(NTireCounter) >= MedTireX And LEAAircraft(ACIndex).TireY(NTireCounter) < MedTireY Then
                                    For J = 1 To LEAAircraft(ACIndex).NTires
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) - 2 * MaxMedLenghtX And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                        If LEAAircraft(ACIndex).TireX(J) = LEAAircraft(ACIndex).TireX(NTireCounter) And LEAAircraft(ACIndex).TireY(J) = LEAAircraft(ACIndex).TireY(NTireCounter) + 2 * MaxMedLenghtY Then
                                            SymmetryVector(SCounter) = 1
                                            SCounter = SCounter + 1
                                        End If
                                    Next
                                End If

                            Next

                        End With

                        If LEAAircraft(ACIndex).NTires = 6 Then
                            For J = 1 To LEAAircraft(ACIndex).NTires
                                If MinTireY < LEAAircraft(ACIndex).TireY(J) And LEAAircraft(ACIndex).TireY(J) < MaxTireY Then
                                    SymmetryChecker(1, 1) = (LEAAircraft(ACIndex).TireX(J))
                                    SymmetryChecker(1, 2) = (LEAAircraft(ACIndex).TireX(J))
                                    SymmetryChecker(2, 1) = (LEAAircraft(ACIndex).TireY(J))
                                    SymmetryChecker(2, 2) = (LEAAircraft(ACIndex).TireY(J))
                                End If
                            Next
                        End If

                        Dim ZeroCounter As Integer = 0
                        For J = 1 To LEAAircraft(ACIndex).NTires * 2
                            If SymmetryVector(J) = 0 Then
                                NotSymmetryXGear = True
                                ZeroCounter = ZeroCounter + 1
                            End If
                        Next
                        If MaxTireX = MinTireX And MaxTireY = MinTireY Then
                            NotSymmetryXGear = False
                        End If

                        If LEAAircraft(ACIndex).NTires = 6 Then
                            If ZeroCounter = 2 Then
                                If SymmetryChecker(1, 1) = (MaxTireX) Or SymmetryChecker(1, 2) = (MinTireX) Then
                                    If SymmetryChecker(1, 2) = (MaxTireX) Or SymmetryChecker(1, 2) = (MinTireX) Then
                                        If SymmetryChecker(2, 1) = MedTireY And SymmetryChecker(2, 1) = MedTireY Then
                                            NotSymmetryXGear = False
                                        End If
                                    End If
                                End If
                            End If
                        End If

                        If NotSymmetryXGear = False Then
                            SymmetryXGear = True
                        End If

                        If SymmetryXGear = False Then
                            NCat(2) += 1S
                            GroupIndex(ACIndex) = 2
                            If firstACCat(2) = 0 Then
                                firstACCat(2) = ACIndex
                            End If
                        Else
                            NCat(1) += 1S
                            GroupIndex(ACIndex) = 1
                            If firstACCat(1) = 0 Then
                                firstACCat(1) = ACIndex
                            End If
                        End If

                        'NCat(2) += 1S
                        'GroupIndex(ACIndex) = 2
                        'If firstACCat(2) = 0 Then
                        '    firstACCat(2) = ACIndex
                        'End If
                        'changed for unsymmetrical gear by YC 082216 092016 END

                        ' GearOrientation(ACIndex) = 90   'added to define aircraft orientation by YGC 102213
                        If LEAAircraft(ACIndex).libGear = "X" Then
                            GearOrientation(ACIndex) = Convert.ToInt16(LEAAircraft(ACIndex).LibGearOrientation)
                        End If

                    Else 'undetermined aircrafts

                    End If
                End If
            Next ACIndex



            'changed to separate sym and unsym gear by YC 082216 092016
            'added to compute all aircraft in 1 step by YGC 102213
            'NCat(0) = CShort(gNACarg)
            'firstACCat(0) = 1

            'Call GetFEMStressAllAC(LEAAircraft, LEAStructure, ACIndex, "Edge", GearOrientation)
            ''Call GetFEMStress(LEAAircraft, LEAStructure, ACIndex, "Edge", GearOrientation(1))
            'If gUserInterrupted Then Exit Sub

            For iCat = 1 To gMeshCategories '2 (SYM and UNSYM) mesh categories ***
                If NCat(iCat) > 0 Then 'are there any aircraft in this category
                    If iCat = 1 Then
                        MeshCase = "SYM"
                    Else
                        MeshCase = "UNSYM"
                    End If
                    'MeshCase = "UNSYM"
                    'Call GetFEMStress(LEAAircraft, LEAStructure, ACIndex, "Edge", GearOrientation)     ' QW 4-11-2019
                    Call GetFEMStress(LEAAircraft, LEAStructure, ACIndex, "Edge", GearOrientation, JobName, ct)
                    If gUserInterrupted Then Exit Sub

                    Dim kk As Integer = 0

                    For ACIndex = 1 To CShort(NACarg)
                        If GroupIndex(ACIndex) = iCat Then
                            kk += 1
                            If Overlay Then
                                Response(ACIndex, 1) = modWorld.Stress8(kk)    'overlay stress
                                Response(ACIndex, 2) = modWorld.Stress1(kk)    ' underlay stress
                            Else
                                Response(ACIndex, 1) = modWorld.Stress1(kk)
                            End If
                        End If
                    Next ACIndex

                End If 'are there any aircraft in this category
            Next iCat '2 mesh categories ***
            'changed to separate sym and unsym gear by YC 082216 092016 END


            'added to transfer vertical stress for rigid compaction by YGC 112213
            Dim k As Integer
            ReDim VertStress(NACarg, NSGLayer), VertCoord(NSGLayer)


            For k = 1 To gNumberOfFoundationInterfaces
                For ACIndex = 1 To CShort(NACarg)
                    VertStress(ACIndex, k) = stress33(ACIndex, k)
                Next ACIndex
                VertCoord(k) = zstress33(k)
            Next k

            'added to transfer vertical stress for rigid compaction by YGC 112213 END

            GoTo 370
            'add ended to compute all aircraft in 1 step by YGC 102213


370:        'added to compute all aircraft in 1 step by YGC 102213

            'ikawa 
            For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                If InStr(LEAAircraft(ACIndex).ACname, "747", CompareMethod.Text) > 0 _
                   And InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) > 0 Then
                    Dim ACIndex2 As Integer, pos1, pos2 As Integer
                    pos1 = InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) - 2
                    For ACIndex2 = 1 To CShort(NACarg) 'for all aircraft
                        pos2 = InStr(LEAAircraft(ACIndex2).ACname, "Body", CompareMethod.Text) - 2
                        If pos2 > 0 Then
                            If (Not ACIndex = ACIndex2) _
                               And Mid(LEAAircraft(ACIndex).ACname, 1, pos1) = Mid(LEAAircraft(ACIndex2).ACname, 1, pos2) _
                               And LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex2).GearLoad Then
                                Response(ACIndex2, 1) = Response(ACIndex, 1)
                                If Overlay Then
                                    Response(ACIndex2, 2) = Response(ACIndex, 2)
                                End If
                            End If
                        End If
                    Next ACIndex2
                End If
            Next ACIndex



            If ThinLayer Then
                ThinLayer = False

                If DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                    'return to previous values
                    LEAStructure.Thick(3) = LEAStructure.Thick(4)
                    LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(4)
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(3)
                    ReDim Preserve LEAStructure.Poisson(3)
                    ReDim Preserve LEAStructure.Thick(3)
                    ReDim Preserve LEAStructure.InterfaceParm(3)

                End If

            End If




            tdiff = DateDiff(DateInterval.Second, t1, Now(), FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)

            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                For ACIndex = 1 To CShort(NACarg)
                    Dim pr As Integer
                    pr = 1
                    If Overlay Then
                        pr = 2
                        PrintLine(3, LPad(3, CStr(ACIndex)) _
                      & LPad(19, LEAAircraft(ACIndex).ACname) _
                      & LPad(10, VB6.Format(EdgeStressFEM0Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(EdgeStressFEM90Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressFEMOverlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(Response(ACIndex, 1), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressLEAFOverlay, "#,###,###.00")) _
                      & LPad(5, "Over") _
                      & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                      & LPad(10, VB6.Format(LEAStructure.Modulus(1), "#,###,###.00")) _
                      & LPad(23, CStr(Now())))
                    End If

                    PrintLine(3, LPad(3, CStr(ACIndex)) _
                  & LPad(19, LEAAircraft(ACIndex).ACname) _
                  & LPad(10, VB6.Format(EdgeStressFEM0PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(EdgeStressFEM90PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressFEM(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(Response(ACIndex, pr), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressLEAF, "#,###,###.00")) _
                  & LPad(5, "Slab") _
                  & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                  & LPad(10, VB6.Format(LEAStructure.Modulus(pr), "#,###,###.00")) _
                  & LPad(23, CStr(Now())))
                Next ACIndex
                FileClose(3)
            End If


            Call MakeCopy(ACcopy, LEAAircraft, s1)

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try




    End Sub

    Private Sub GetFEMStress(ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
          ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
          ByVal ACIndex As Short, ByVal StressType As String, ByVal GA() As Short, ByVal JobName As String, ByRef ct As CancellationToken)  ' QW 04-11-2019
        '          ByVal ACIndex As Short, ByVal StressType As String, ByVal GA() As Short)
        'subroutine to compute all aircraft in 1 step by YGC 102213

        'Microsoft Scripting Runtime checked in Project References
        Dim fso As New Scripting.FileSystemObject(), fil As Boolean
        Dim ExtFilePath As String

        Dim Sorting As Boolean                                            'for sorting
        Dim WheelLoad(NCat(iCat), 2), WheelLoad2(NCat(iCat), 2) As Double 'for sorting


        '112017-030719 YC for sorting
        Dim GearLoadSD2D3D2(NCat(iCat), 2) As Double
        Dim GearLoadSD(NCat(iCat), 2), GearLoadSD2(NCat(iCat), 2) As Double
        Dim GearLoad2D3D(NCat(iCat), 2), GearLoad2D3D2(NCat(iCat), 2) As Double
        Dim NACSD, NAC2D3D As Integer
        '112017-030719 YC for sorting 



        Dim PreviousIndex As Integer, Diff As Double                      'for scaling 
        Dim DiffNum As Double = 0.15                                        'for scaling 

        Dim index1() As Integer
        'Dim ScaleFactorA() As Double

        ' ************ Scaling *************
        'Dim Scaled(80) As Boolean
        'Public Const ScFactor As Double = 0.25
        Dim ScalingImplemented As Boolean
        ' ************ Scaling *************

        Dim SInfo() As ScalingInfo
        ReDim SInfo(NCat(iCat))

        ReDim index1(NCat(iCat))


        'ReDim Scaled(NCat(iCat))
        'ReDim ScaleFactorA(NCat(iCat))


        'Stress1 = -40 * LEAStructure.Thick(1) + 850 + LEAAircraft(ACIndex).GearLoad * 0.0002
        'Stress1 = 20
        'Exit Sub

        'GoTo gotoIngrid
        ScalingImplemented = True
        Sorting = True

        If NCat(iCat) = 1 Then 'NCat() - number of aircraft in particular category
            Sorting = False
            ScalingImplemented = False
        End If

        ChDir(VB6.GetPath)
        ExtFilePath = VB6.GetPath & "\"

        If StressType = "Edge" Then
            EdgeLoad = True
        Else
            EdgeLoad = False
        End If



        'GearAngle = GA      '90-perpendicular to edge ' 0-parallel to edge         'suppressed by YGC 102213 


        Call frmStructure.frmStructure_Load(LEAStructure)



        If Sorting Then
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   WITH SORTING   *****************************
            Dim i1, i2 As Integer

            i2 = 0
            For i1 = 1 To gNACarg

                If GroupIndex(i1) = iCat And gCalcStress(i1) = 0 Then 'modified to compute all aircraft in 1 step by YGC 102213,retrived to seperate sym/unsym aircraft by YC 082216 092016
                    'If gCalcStress(i1) = 0 Then

                    i2 += 1
                    index1(i2) = i1
                End If
            Next


            For i1 = 1 To NCat(iCat)
                WheelLoad(i1, 1) = CDbl(LEAAircraft(index1(i1)).GearLoad /
                                        LEAAircraft(index1(i1)).NTires)
                WheelLoad(i1, 2) = i1
            Next

            Call PIKSTR2(NCat(iCat), WheelLoad, WheelLoad2)
            'WheelLoad2 - indexes of aircraft sorted array according to wheel load - descending



            ' 112017-030719 YC new sorting per gear type (SD-2D3D )and gear load
            Dim j1, j2 As Integer
            j1 = 1 : j2 = 1
            NACSD = 0 : NAC2D3D = 0

            For i1 = 1 To NCat(iCat)

                With LEAAircraft(index1(i1))

                    If .NTires <= 2 Then
                        GearLoadSD(j1, 1) = CDbl(.GearLoad)
                        GearLoadSD(j1, 2) = i1
                        j1 = j1 + 1
                        NACSD = NACSD + 1
                    Else
                        GearLoad2D3D(j2, 1) = CDbl(.GearLoad)
                        GearLoad2D3D(j2, 2) = i1
                        j2 = j2 + 1
                        NAC2D3D = NAC2D3D + 1
                    End If

                End With

            Next

            Call SortingAscending(NACSD, GearLoadSD, GearLoadSD2)
            Call SortingAscending(NAC2D3D, GearLoad2D3D, GearLoad2D3D2)

            GearLoadSD2D3D2 = GearLoadSD2
            For i1 = NACSD + 1 To NCat(iCat)
                GearLoadSD2D3D2(i1, 1) = GearLoad2D3D2(i1 - NACSD, 1)
                GearLoadSD2D3D2(i1, 2) = GearLoad2D3D2(i1 - NACSD, 2)
            Next
            ' 112017-030719 YC new sorting per gear type (SD-2D3D )and gear load  END



            'ACIndex = CShort(WheelLoad2(1, 2)) 'index for first aircraft in the group

            'ACIndex = CShort(index1(CInt(WheelLoad2(1, 2))))    '112017-030719 YC sorting
            ACIndex = CShort(index1(CInt(GearLoadSD2D3D2(1, 2))))


            GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213

            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                 ' QW 09-15-201
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '


            Dim icc As Short
            For icc = 2 To NCat(iCat)

                'ACIndex = CShort(WheelLoad2(icc, 2))

                'ACIndex = CShort(index1(CInt(WheelLoad2(icc, 2)))) '112017-030719 YC sorting
                ACIndex = CShort(index1(CInt(GearLoadSD2D3D2(icc, 2))))


                GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213


                ScalingImplemented = False 'Izydor 2014.04.30
                ScalingImplemented = True
                ScalingImplemented = False '112017-030719 YC sorting by gear type and gear laod

                ' ****************** scaling begin ******************
                If ScalingImplemented Then
                    If (LEAAircraft(ACIndex).GearLoad >
                        LEAAircraft(PreviousIndex).GearLoad) Then
                        Diff = CDbl((LEAAircraft(ACIndex).GearLoad -
                                     LEAAircraft(PreviousIndex).GearLoad) /
                                     LEAAircraft(PreviousIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 1.15
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 1.5
                            'ScaleFactorA(icc) = 2
                        End If
                    Else
                        Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                     LEAAircraft(ACIndex).GearLoad) /
                                     LEAAircraft(ACIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 0.75
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 0.5
                        End If
                    End If


                    If Diff < DiffNum Then
                        SInfo(icc).Scaled = True
                        Dim TireNumb As Integer
                        For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                            LEAAircraft(ACIndex).TirePress(TireNumb) =
                            LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                        Next
                        LEAAircraft(ACIndex).GearLoad =
                        LEAAircraft(ACIndex).GearLoad * SInfo(icc).ScaleFactorA

                        SInfo(icc).index2 = ACIndex
                    End If
                    'PreviousIndex = icc
                    PreviousIndex = ACIndex
                End If
                ' ****************** scaling end ******************


                Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                Call GearLoads(ACIndex, LEAAircraft)
                'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                Call WriteTextFile(OpenMode.Append, rgdFile)
            Next icc
            '**************   WITH SORTING   *****************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        Else

            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   without sorting   *****************************
            ACIndex = firstACCat(iCat) 'index for first aircraft in the group

            GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213

            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                                             ' QW 09-15-2019
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '


            Dim icc As Short
            For icc = (firstACCat(iCat) + 1S) To CShort(gNACarg)
                If GroupIndex(icc) = iCat Then   'suppressed to compute all aircraft in 1 step by YGC 102213,retrived to seperate sym/unsym aircraft by YC 082216 092016
                    ACIndex = icc

                    GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213

                    ' ****************** scaling begin ******************
                    If ScalingImplemented Then
                        If (LEAAircraft(icc).GearLoad >
                            LEAAircraft(PreviousIndex).GearLoad) Then
                            Diff = CDbl((LEAAircraft(icc).GearLoad -
                                         LEAAircraft(PreviousIndex).GearLoad) /
                                         LEAAircraft(PreviousIndex).GearLoad)
                        Else
                            Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                         LEAAircraft(icc).GearLoad) /
                                         LEAAircraft(icc).GearLoad)
                        End If


                        If Diff < DiffNum Then
                            SInfo(icc).Scaled = True
                            Dim TireNumb As Integer
                            For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                                LEAAircraft(ACIndex).TirePress(TireNumb) =
                                LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                            Next
                            LEAAircraft(icc).GearLoad =
                            LEAAircraft(icc).GearLoad * SInfo(icc).ScaleFactorA
                        End If
                        PreviousIndex = icc
                    End If
                    ' ****************** scaling end ******************

                    Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                    Call GearLoads(ACIndex, LEAAircraft)

                    'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                    Call WriteTextFile(OpenMode.Append, rgdFile)
                End If     'suppressed to compute all aircraft in 1 step by YGC 102213,retrived to seperate sym/unsym aircraft by YC 082216 092016
            Next icc
            '*******************   without sorting   ************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        End If


gotoIngrid:
        'GoTo gotoNike3d

        'modified for user-specified directory by YGC 101012
        'Call INGRIDMAIN() 
        'Dim WorkingDir0 As String  'moevd by YGC 112213
        WorkingDir0 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\"

        'add Ingrill exception error handeling by YC 052416
        'Call INGRIDMAIN(WorkingDir0, Len(WorkingDir0))
        'modify ended for user-specified directory by YGC 101012

        Dim txt, title As String
        Dim response As MsgBoxResult
        Dim IUnitErr0, IErr0 As Short
        Dim ErrFlNm As String = Nothing

        'modified by YC 101215 092016

        'Try

        'Call INGRIDMAIN(IUnitErr0, IErr0, WorkingDir0, Len(WorkingDir0))
        'If IErr0 = 0 Then GoTo IngridDoneLabel

        'If IUnitErr0 = 13 Then
        '    ErrFlNm = "nikein"
        'ElseIf IUnitErr0 = 41 Then
        '    ErrFlNm = "isave"
        'ElseIf IUnitErr0 = 15 Then
        '    ErrFlNm = "tape15"
        'ElseIf IUnitErr0 = 18 Then
        '    ErrFlNm = "tape18"
        'ElseIf IUnitErr0 = 7 Then
        '    ErrFlNm = "tape7"
        'ElseIf IUnitErr0 = 8 Then
        '    ErrFlNm = "tape8"
        'ElseIf IUnitErr0 = 9 Then
        '    ErrFlNm = "tape9"
        'Else
        '    ErrFlNm = "unknown"
        'End If

        'txt = "File " & ErrFlNm & ", Error Code " & IErr0 & Environment.NewLine

        'title = "Ingrid Fortran Error"

        '        Catch ex As Exception
        '            txt = ex.Message
        '            txt = txt + Environment.NewLine + Environment.NewLine
        '            'txt = txt + ex.StackTrace
        '            'txt = txt + Environment.NewLine + Environment.NewLine

        '            title = "Ingrid DLL Exception Error"

        '        End Try

        '        txt = txt & Environment.NewLine & Environment.NewLine & "FAARFIELD could not complete the design. Retry?"

        '        response = MsgBox(txt, MsgBoxStyle.YesNo, title)

        '        If response = MsgBoxResult.Yes Then
        '            GoTo GoToIngrid
        '        Else
        '            gUserInterrupted = True
        '            Exit Sub
        '        End If

        'IngridDoneLabel:
        'add Ingrill exception error handeling by YC 052416 END

        Try
            Dim RunMesh As New FAAMeshClassLib.clsMesh()
            'Call RunMesh.MeshGeneration(LCD, SLD, MAT, PART, PARTSLD, PARTBC, PARTMAT, ACLoad, WorkingDir0)

            Call RunMesh.MeshGeneration(LCD, Nd, BrickElement, SpringType, SpringElement, SlidingElement, NodalLoad,
                                        SLD, MAT, PART, PARTSLD, PARTBC, PARTMAT, ACLoad, WorkingDir0, JobName, ModelOut)   '  QW 09-15-2019


        Catch ex As Exception
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)
        End Try

        'modified by YC 101215 092016 END



        'Call Ingrid()
        System.Windows.Forms.Application.DoEvents()
        '------------------------------------------------------------------------------
        Dim StopFEDFAA As Short, FEDFAAStopped As Short
        Static AtomName As New VB6.FixedLengthString(10)
        Dim II As Integer, i As Integer
        Dim ACDATPath As String, DesigningStr As Boolean

        ACDATPath = Environment.CurrentDirectory
        DesigningStr = False
        AtomName.Value = "StopFEDFAA"

        Do
            II = GlobalFindAtom(AtomName.Value)
            If II <> 0 Then i = GlobalDeleteAtom(CShort(II))
        Loop Until II = 0
        StopFEDFAA = 0 : FEDFAAStopped = 0

        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped)
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, MeshCase, CShort(gDesignType))
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), MeshCase)

        If MeshCase = "1DSYM" Then
            iSymCase = 1S
        ElseIf MeshCase = "2DSYM" Then
            iSymCase = 2S
        ElseIf MeshCase = "3DSYM" Then
            iSymCase = 3S
        ElseIf MeshCase = "4DNSY" Then
            iSymCase = 4S
        ElseIf MeshCase = "5DSYM" Then
            iSymCase = 5S
        End If

        Dim tStart, tEnd As Date, tdiff As Long
        Dim d1, d2 As Double, St As String
        Dim timeSave1, timeSave2 As Integer, timeDiff As Double
        tStart = Now()
        d1 = TimeOfDay.ToOADate
        timeSave1 = timeGetTime

        'Modified to use printed stress by YGC 112213
        'YGC 101012
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase)
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))


        ' ****************** scaling begin ****************** scaling back !!!!
        'If ScalingImplemented Then
        '    Dim in1 As Integer
        '    For in1 = 1 To 2 Step UBound(SInfo, 1)

        '        If SInfo(in1).Scaled = True Then

        '            Dim TireNumb As Integer
        '            For TireNumb = 1 To LEAAircraft(ACIndex).NTires
        '                LEAAircraft(ACIndex).TirePress(TireNumb) = _
        '                LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(in1).ScaleFactorA
        '            Next
        '            LEAAircraft(ACIndex).GearLoad = _
        '            LEAAircraft(ACIndex).GearLoad * SInfo(in1).ScaleFactorA

        '            SInfo(in1).index2 = ACIndex
        '        End If
        '    Next in1
        'End If
        ' ****************** scaling end ******************



        ' recovered to use transfered stress by YC 052615
        'Call NIKE3D(StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))
        'Call Readstressmax(Stress1, Stress8, zstress33, stress33)
        ''Modified to use printed stress by YGC 112213




        ' add Nike3D exception error handeling by YC 052416
        'Call NIKE3D(Stress1(1), Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))
        ' recovered to use transfered stress by YC 052615 END

        'Exit Sub 'to comment

Nike3DStartLabel:

        Dim NewNike3D As Integer = 2        ' Test NIKE3D   QW 08-23-2017
        If NewNike3D = 1 Then
            Try
                Call NIKE3D(Stress1(1), Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))

                txt = "File " & ErrFlNm & ", Error Code " & IErr0 & Environment.NewLine

                title = "Nike3D Fortran Error"

            Catch ex As Exception

                txt = ex.Message
                txt = txt + Environment.NewLine + Environment.NewLine
                'txt = txt + ex.StackTrace
                'txt = txt + Environment.NewLine + Environment.NewLine

                title = "Nike3D DLL Exception Error"

                txt = txt & Environment.NewLine & Environment.NewLine & "FAARFIELD could not complete the design. Retry?"

                response = MsgBox(txt, MsgBoxStyle.YesNo, title)

                If response = MsgBoxResult.Yes Then
                    GoTo Nike3DStartLabel
                Else
                    gUserInterrupted = True
                    Exit Sub
                End If

            End Try
            'add Nike3D exception error handeling by YC 052416 END

        Else                                            ' Test NIKE3D   QW 08-23-2017
            Try
                Call Conversion()
                Dim RunFEM As New FEMClassLib.clsFEM()
                'ModelOut = 0
                Call RunFEM.FAASR3D(IPC, Stress1, Stress8, StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, JobName, ModelOut, ct)
            Catch ex As Exception
                txt = ex.Message
                txt = txt + Environment.NewLine + Environment.NewLine
                txt = txt + ex.StackTrace
                txt = txt + Environment.NewLine + Environment.NewLine
                MsgBox(txt)
            End Try
        End If
        ' End Test NIKE3D   QW 08-23-2017

        timeSave2 = timeGetTime
        timeDiff = CDbl((timeSave2 - timeSave1) / 1000)

        d2 = TimeOfDay.ToOADate

        St = VB6.Format(System.DateTime.FromOADate(d2 - d1), "hh:mm:ss")
        tEnd = Now()

        tdiff = DateDiff(DateInterval.Second, tStart, tEnd, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)


        'Dim FileNoZ As Integer, FileNameZ As String 'IK 2016.05.31 clsAM
        'FileNoZ = FreeFile()
        'FileNameZ = WorkingDir0 & "\Nike3DTime.dat"
        'FileOpen(FileNoZ, FileNameZ, OpenMode.Append)
        'PrintLine(FileNoZ, timeSave1 & "   " & timeSave2 & "   " & timeDiff)
        '     PrintLine(FileNoZ, d1 & "    " & d2 & "   " & tdiff)
        'FileClose(FileNoZ)






        StopFEDFAA = -1
        'II = GlobalFindAtom(AtomName.Value)
        FEDFAAStopped = -1              ' QW 4-11-2019

        System.Windows.Forms.Application.DoEvents()
        System.Diagnostics.Debug.WriteLine("StopFEDFAA = " & StopFEDFAA)
        Do
            System.Windows.Forms.Application.DoEvents()
            If FEDFAAStopped = -1 Then
                System.Diagnostics.Debug.WriteLine("FEDFAAStopped = " & FEDFAAStopped)
                Exit Do
            End If
        Loop

        Do
            ' The Dir$ function is very time consuming, therefore use
            ' a For Loop for DoEvents.
            ' 1,000 times through the loop is the best compromise for long
            ' Julea executions and short Julea executions. Marking the end of
            ' a Julea computation with an atom does not reduce total time much.
            For i = 1 To 1000
                System.Windows.Forms.Application.DoEvents()
                If DesigningStr = False Then ' Stop Julea
                    II = GlobalAddAtom(AtomName.Value)
                    System.Diagnostics.Debug.WriteLine("AddAtom = " & II)
                    '        StartTime = Timer
                    Do
                        System.Windows.Forms.Application.DoEvents() ' Let Julea delete the atom.
                        '          ETime = Abs(Timer - StartTime)
                        '          Debug.Print ETime
                        II = GlobalFindAtom(AtomName.Value)
                        System.Diagnostics.Debug.WriteLine("FindAtom = " & II)
                        '         Need to check file in case atom added after JULEA stopped
                        '         normally but file writes not finished.
                        'ikawa If II = 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then

                        If II <> 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then
                            System.Diagnostics.Debug.WriteLine("Check before delete " _
                                   & GlobalFindAtom(AtomName.Value))
                            If II <> 0 Then II = GlobalDeleteAtom(CShort(II))
                            System.Diagnostics.Debug.Write("DeleteAtom = " & II)
                            System.Diagnostics.Debug.WriteLine(GlobalFindAtom(AtomName.Value))
                            If Dir(ACDATPath & "JULEA.ERR") = "" Then
                                Exit Do ' Make sure all file activity is finished
                            End If ' before closing Julea below.
                        End If
                    Loop
                    Exit For
                End If
            Next i
            If Dir(ACDATPath & "JULEA.ERR") <> "" Or DesigningStr = False Then
                '      IL = SendMessage(hJULEA, WM_CLOSE, 0&, 0&)
                Exit Do
            End If
        Loop

        'If StopFEDFAA = -1 Then
        '    gUserInterrupted = True
        'End If

        If gUserInterrupted Then Exit Sub 'September 13, 2006
gotoNike3d:


        'For i = 1 To NCat(iCat)
        '    If StressType = "Edge" Then
        '        modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
        '        modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
        '    End If
        'Next i

        For i = 1 To NCat(iCat)
            If StressType = "Edge" Then
                If gDesignType2 = 13 Then 'HMA Overlay on Rigid - FlexOnRigid
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.5
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.5
                Else
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
                End If
            End If
        Next i




        '********************* Scaling Part Begin *********************
        If ScalingImplemented Then
            For i = 1 To NCat(iCat) 'for all aircraft
                If SInfo(i).Scaled Then
                    LEAAircraft(SInfo(i).index2).GearLoad = LEAAircraft(SInfo(i).index2).GearLoad / SInfo(i).ScaleFactorA
                    modWorld.Stress1(i) = modWorld.Stress1(i) / SInfo(i).ScaleFactorA
                    modWorld.Stress8(i) = modWorld.Stress8(i) / SInfo(i).ScaleFactorA

                    'added to scale foundation vertical stress for rigid compaction by YGC 112213
                    Dim k As Integer

                    For k = 1 To gNumberOfFoundationInterfaces
                        stress33(i, k) = stress33(i, k) / SInfo(i).ScaleFactorA
                    Next k
                    'added to scale for foundation vertical stress for rigid compaction by YGC 112213 END

                End If
            Next
        End If

        '********************* Scaling Part End *********************
        If Sorting Then
            Dim S11(NCat(iCat)), S88(NCat(iCat)) As Double

            For i = 1 To NCat(iCat)
                S11(i) = modWorld.Stress1(i)
                S88(i) = modWorld.Stress8(i)
            Next

            For i = 1 To NCat(iCat)
                'modWorld.Stress1(index1(CInt(WheelLoad2(i, 2)))) = S11(i)
                'modWorld.Stress8(index1(CInt(WheelLoad2(i, 2)))) = S88(i)

                '112017-030719 YC Sorting
                'modWorld.Stress1(CInt(WheelLoad2(i, 2))) = S11(i)
                'modWorld.Stress8(CInt(WheelLoad2(i, 2))) = S88(i)
                modWorld.Stress1(CInt(GearLoadSD2D3D2(i, 2))) = S11(i)
                modWorld.Stress8(CInt(GearLoadSD2D3D2(i, 2))) = S88(i)

            Next

            'added to sort foundation vertical stress for rigid compaction by YGC 112213
            Dim S33(NCat(iCat), gNumberOfFoundationInterfaces) As Double
            Dim k As Integer

            For i = 1 To NCat(iCat)
                For k = 1 To gNumberOfFoundationInterfaces
                    S33(i, k) = stress33(i, k)
                Next k
            Next i

            For i = 1 To NCat(iCat)
                For k = 1 To gNumberOfFoundationInterfaces

                    '112017-030719 YC Sorting
                    'stress33(CInt(WheelLoad2(i, 2)), k) = S33(i, k)
                    stress33(CInt(GearLoadSD2D3D2(i, 2)), k) = S33(i, k)

                Next k
            Next i
            'added to sort foundation vertical stress for rigid compaction by YGC 112213 END

        End If

        ACIndex = ACIndex

    End Sub


    Public Sub ComputeResponseAllAC(
       ByVal ResponseType As LEAFClassLib.clsLEAF.LEAFoptions,
       ByVal NACarg As Integer,
       ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
       ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
       ByRef Response(,) As Double,
       ByRef VertStress(,) As Double, ByRef VertCoord() As Double,
       ByRef AllResps(,) As LEAFClassLib.clsLEAF.LEAFAllResponses,
       ByRef CalcStress() As Integer,
       ByVal DesignType2 As Integer,
       ByVal SolverType2 As Integer,
       ByVal SlabMeshSize2 As Integer,
       ByVal JobName As String,
       ByRef UserInterrupted As Boolean)

        'used in version up to FF 2016.09.20 v1.41.0011, renamed by YC 082216 092016
        'added "ByRef VertStress(,) As Double, ByRef VertCoord() As Double, _" to transfer vertical stress for rigid compaction by YGC 112213 
        'added "ByVal SolverType2 As Integer " for solver choice by YGC 083012
        'added "ByVal SlabMeshSize2 As Integer" for slab mesh size selection by YGC 061113
        'subroutine for Multiple aircraft in 1 step, automated by YGC 031312
        Dim ACcopy() As LEAFClassLib.clsLEAF.LEAFACParms
        Dim s1 As Integer
        s1 = UBound(LEAAircraft)
        ReDim ACcopy(s1)

        Call MakeCopy(LEAAircraft, ACcopy, s1)


        Try

            'Exit Sub

            gDesignType2 = DesignType2
            'LEAAircraft(1).NTires = 2
            'LEAAircraft(1).GearLoad = LEAAircraft(1).GearLoad * LEAAircraft(1).NTires / 14

            Dim I As Short, ACIndex As Short
            Dim EdgeStressFEM0PCC(gAC), EdgeStressFEM90PCC(gAC) As Double
            Dim EdgeStressFEM0Overlay(gAC), EdgeStressFEM90Overlay(gAC) As Double
            Dim InteriorStressLEAF, InteriorStressFEM(gAC) As Double
            Dim InteriorStressLEAFOverlay, InteriorStressFEMOverlay(gAC) As Double

            'Dim MaxStressPCC(gAC), MaxStressOverlay(gAC) As Double
            Dim PrintResults As Boolean
            Dim DesignType As String


            Dim t1 As Date, tdiff As Long
            Dim ThinLayer As Boolean

            Dim GearOrientation(gAC) As Short 'added to define aircraft orientation by YGC 102213

            'DesignType2 = 1

            'Public Const NewFlex As Short = 1
            'Public Const FlexOnFlex As Short = 2
            'Public Const PCCOnFlex As Short = 3

            'Public Const NewRigid As Short = 10
            'Public Const UnbondOnRigid As Short = 11
            'Public Const PartBondOnRigid As Short = 12
            'Public Const FlexOnRigid As Short = 13

            SolverType = SolverType2 'added for solver choice by YGC 083012
            SlabMeshSize = SlabMeshSize2 'added for slab mesh size selection by YGC 061113

            gearOrient = CShort(LEAStructure.lngDummy(1))


            'Izydor Kawa 10/16/2012
            'Dim MyDoc1 As System.Environment.SpecialFolder = Environment.SpecialFolder.MyDocuments
            rgdFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.rgd"
            ingFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.ing"


            ThinLayer = False
            If DesignType2 = 10 Then

            ElseIf DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                If LEAStructure.NLayers = 3 Then
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(4)
                    ReDim Preserve LEAStructure.Poisson(4)
                    ReDim Preserve LEAStructure.Thick(4)
                    ReDim Preserve LEAStructure.InterfaceParm(4)

                    LEAStructure.Modulus(4) = LEAStructure.Modulus(3)

                    LEAStructure.Thick(4) = LEAStructure.Thick(3)
                    LEAStructure.Thick(3) = 4

                    LEAStructure.InterfaceParm(4) = LEAStructure.InterfaceParm(3)
                    'LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(2)

                    LEAStructure.Poisson(4) = LEAStructure.Poisson(3)
                    'LEAStructure.Poisson(3) = 0.35
                    ThinLayer = True

                End If
            End If



            'Call INGRIDMAIN()
            'Call INGRID()

            For I = 1 To CShort(NACarg)
                gCalcStress(I) = CalcStress(I)
            Next

            'LEAAircraft(2).GearLoad = 999
            'Exit Sub

            If UserInterrupted Then Exit Sub 'September 13, 2006

            gUserInterrupted = UserInterrupted
            gDesignType = DesignType2
            gNACarg = NACarg
            PrintResults = False
            Overlay = False
            ReDim NCat(gMeshCategories)
            ReDim firstACCat(gMeshCategories)
            ReDim GroupIndex(gAC * 2)

            '.InterfaceParm(1) = 1 And .InterfaceParm(1) = 0 FlexOnRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(1) = 1 NewRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(2) = 0 UnbondOnRigid
            '.InterfaceParm(1) > 0 And .InterfaceParm(1) < 1 PartBondOnRigid

            If LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "FlexOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 1 Then
                DesignType = "NewRigid"
                Overlay = False
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "UnbondOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) > 0 And LEAStructure.InterfaceParm(1) < 1 Then
                DesignType = "PartBondOnRigid"
                Overlay = True



            End If


            If Overlay Then
                ReDim Response(NACarg, 2)
            Else
                ReDim Response(NACarg, 8)
            End If


            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                PrintLine(3, "NIKE3D   " _
                    & LPad(13, "Aircraft") _
                    & LPad(10, "Edge0") _
                    & LPad(10, "Edge90") _
                    & LPad(10, "IntFEM") _
                    & LPad(10, "MaxS") _
                    & LPad(10, "IntLEAF") _
                    & LPad(12, "Thick= " _
                    & VB6.Format(LEAStructure.Thick(1), "#0.0000")) _
                    & LPad(22, CStr(Now)))
                FileClose(3)
            End If

            t1 = Now()
            System.Windows.Forms.Application.DoEvents()



            For ACIndex = 1 To CShort(NACarg)
                Dim B747New As Boolean


                B747New = (InStr(LEAAircraft(ACIndex).ACname, "747", CompareMethod.Text) > 0) _
                And LEAAircraft(ACIndex).NTires = 4 And LEAAircraft(ACIndex).libGear = "Z"




                If CalcStress(ACIndex) = 0 Then
                    If LEAAircraft(ACIndex).ACname = "C-5" Then
                        LEAAircraft(ACIndex).NTires = 6
                        LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex).GearLoad / 2
                    End If


                    ''''If LEAAircraft(ACIndex).ACname = "C-130" Then
                    ''''    Dim x1, x2, y1, y2 As Double
                    ''''    x1 = CDbl(LEAAircraft(ACIndex).TireX(1))
                    ''''    x2 = CDbl(LEAAircraft(ACIndex).TireX(2))
                    ''''    y1 = CDbl(LEAAircraft(ACIndex).TireY(1))
                    ''''    y2 = CDbl(LEAAircraft(ACIndex).TireY(2))
                    ''''    LEAAircraft(ACIndex).TireX(1) = y1
                    ''''    LEAAircraft(ACIndex).TireX(2) = y2
                    ''''    LEAAircraft(ACIndex).TireY(1) = x1
                    ''''    LEAAircraft(ACIndex).TireY(2) = x2
                    ''''End If



                    If LEAAircraft(ACIndex).libGear = "A" _
                       Or LEAAircraft(ACIndex).libGear = "B" _
                       Or LEAAircraft(ACIndex).libGear = "D" _
                       Or (LEAAircraft(ACIndex).libGear = "HB" And
                       LEAAircraft(ACIndex).NTires = 2) Then
                        NCat(1) += 1S
                        GroupIndex(ACIndex) = 1
                        If firstACCat(1) = 0 Then
                            firstACCat(1) = ACIndex
                        End If

                        GearOrientation(ACIndex) = 0   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).libGear = "F" _
                          Or LEAAircraft(ACIndex).libGear = "E" _
                          Or LEAAircraft(ACIndex).libGear = "H" _
                          Or LEAAircraft(ACIndex).libGear = "WFBF" _
                          Or LEAAircraft(ACIndex).libGear = "WFBN" _
                          Or LEAAircraft(ACIndex).ACname = "IL86" _
                          Or (LEAAircraft(ACIndex).libGear = "HB" And
                           LEAAircraft(ACIndex).NTires = 4) _
                           Or B747New Then
                        NCat(2) += 1S
                        GroupIndex(ACIndex) = 2
                        If firstACCat(2) = 0 Then
                            firstACCat(2) = ACIndex
                        End If

                        GearOrientation(ACIndex) = CShort(LEAStructure.lngDummy(1))   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).libGear = "N" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" Or
                         (Mid(LEAAircraft(ACIndex).ACname, 1, 4) = "A380" And LEAAircraft(ACIndex).NTires = 6) Then
                        NCat(3) += 1S
                        GroupIndex(ACIndex) = 3
                        If firstACCat(3) = 0 Then
                            firstACCat(3) = ACIndex
                        End If

                        GearOrientation(ACIndex) = 90   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).libGear = "M" _
                        Or LEAAircraft(ACIndex).libGear = "K" _
                        Or LEAAircraft(ACIndex).ACname = "IL76T" _
                        Or LEAAircraft(ACIndex).ACname = "00000" _
                        Or LEAAircraft(ACIndex).ACname = "V" Then
                        NCat(4) += 1S
                        GroupIndex(ACIndex) = 4
                        If firstACCat(4) = 0 Then
                            firstACCat(4) = ACIndex
                        End If

                        GearOrientation(ACIndex) = 0   'added to define aircraft orientation by YGC 102213

                    ElseIf LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).ACname = "An-124" _
                        Or LEAAircraft(ACIndex).ACname = "An-225" Then
                        NCat(5) += 1S
                        GroupIndex(ACIndex) = 5
                        If firstACCat(5) = 0 Then
                            firstACCat(5) = ACIndex
                        End If

                        GearOrientation(ACIndex) = 90   'added to define aircraft orientation by YGC 102213

                    Else 'undetermined aircrafts

                    End If
                End If
            Next ACIndex


            'added to compute all aircraft in 1 step by YGC 102213
            NCat(0) = CShort(gNACarg)
            firstACCat(0) = 1


            Call GetFEMStressAllAC(LEAAircraft, LEAStructure, ACIndex, "Edge", GearOrientation)
            'Call GetFEMStress(LEAAircraft, LEAStructure, ACIndex, "Edge", GearOrientation(1))
            If gUserInterrupted Then Exit Sub


            If Overlay Then
                For ACIndex = 1 To CShort(NACarg)
                    Response(ACIndex, 1) = modWorld.Stress8(ACIndex)    'overlay stress
                    Response(ACIndex, 2) = modWorld.Stress1(ACIndex)    ' underlay stress
                Next ACIndex
            Else
                For ACIndex = 1 To CShort(NACarg)
                    Response(ACIndex, 1) = modWorld.Stress1(ACIndex)
                Next ACIndex
            End If

            'added to transfer vertical stress for rigid compaction by YGC 112213
            Dim k As Integer
            ReDim VertStress(NACarg, NSGLayer), VertCoord(NSGLayer)


            For k = 1 To gNumberOfFoundationInterfaces
                For ACIndex = 1 To CShort(NACarg)
                    VertStress(ACIndex, k) = stress33(ACIndex, k)
                Next ACIndex
                VertCoord(k) = zstress33(k)
            Next k

            'added to transfer vertical stress for rigid compaction by YGC 112213 END

            GoTo 370
            'add ended to compute all aircraft in 1 step by YGC 102213


370:        'added to compute all aircraft in 1 step by YGC 102213

            'ikawa 
            For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                If InStr(LEAAircraft(ACIndex).ACname, "747", CompareMethod.Text) > 0 _
                   And InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) > 0 Then
                    Dim ACIndex2 As Integer, pos1, pos2 As Integer
                    pos1 = InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) - 2
                    For ACIndex2 = 1 To CShort(NACarg) 'for all aircraft
                        pos2 = InStr(LEAAircraft(ACIndex2).ACname, "Body", CompareMethod.Text) - 2
                        If pos2 > 0 Then
                            If (Not ACIndex = ACIndex2) _
                               And Mid(LEAAircraft(ACIndex).ACname, 1, pos1) = Mid(LEAAircraft(ACIndex2).ACname, 1, pos2) _
                               And LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex2).GearLoad Then
                                Response(ACIndex2, 1) = Response(ACIndex, 1)
                                If Overlay Then
                                    Response(ACIndex2, 2) = Response(ACIndex, 2)
                                End If
                            End If
                        End If
                    Next ACIndex2
                End If
            Next ACIndex



            If ThinLayer Then
                ThinLayer = False

                If DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                    'return to previous values
                    LEAStructure.Thick(3) = LEAStructure.Thick(4)
                    LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(4)
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(3)
                    ReDim Preserve LEAStructure.Poisson(3)
                    ReDim Preserve LEAStructure.Thick(3)
                    ReDim Preserve LEAStructure.InterfaceParm(3)

                End If

            End If




            tdiff = DateDiff(DateInterval.Second, t1, Now(), FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)

            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                For ACIndex = 1 To CShort(NACarg)
                    Dim pr As Integer
                    pr = 1
                    If Overlay Then
                        pr = 2
                        PrintLine(3, LPad(3, CStr(ACIndex)) _
                      & LPad(19, LEAAircraft(ACIndex).ACname) _
                      & LPad(10, VB6.Format(EdgeStressFEM0Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(EdgeStressFEM90Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressFEMOverlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(Response(ACIndex, 1), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressLEAFOverlay, "#,###,###.00")) _
                      & LPad(5, "Over") _
                      & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                      & LPad(10, VB6.Format(LEAStructure.Modulus(1), "#,###,###.00")) _
                      & LPad(23, CStr(Now())))
                    End If

                    PrintLine(3, LPad(3, CStr(ACIndex)) _
                  & LPad(19, LEAAircraft(ACIndex).ACname) _
                  & LPad(10, VB6.Format(EdgeStressFEM0PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(EdgeStressFEM90PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressFEM(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(Response(ACIndex, pr), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressLEAF, "#,###,###.00")) _
                  & LPad(5, "Slab") _
                  & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                  & LPad(10, VB6.Format(LEAStructure.Modulus(pr), "#,###,###.00")) _
                  & LPad(23, CStr(Now())))
                Next ACIndex
                FileClose(3)
            End If


            Call MakeCopy(ACcopy, LEAAircraft, s1)

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Sub
    Private Sub GetFEMStressAllAC(ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
             ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
             ByVal ACIndex As Short, ByVal StressType As String, ByVal GA() As Short)
        'used in version up to FF 2016.09.20 v1.41.0011, renamed by YC 082216 092016
        'subroutine to compute all aircraft in 1 step by YGC 102213

        'Microsoft Scripting Runtime checked in Project References
        Dim fso As New Scripting.FileSystemObject(), fil As Boolean
        Dim ExtFilePath As String

        Dim Sorting As Boolean                                            'for sorting
        Dim WheelLoad(NCat(iCat), 2), WheelLoad2(NCat(iCat), 2) As Double 'for sorting
        Dim PreviousIndex As Integer, Diff As Double                      'for scaling 
        Dim DiffNum As Double = 0.15                                        'for scaling 

        Dim index1() As Integer
        'Dim ScaleFactorA() As Double

        ' ************ Scaling *************
        'Dim Scaled(80) As Boolean
        'Public Const ScFactor As Double = 0.25
        Dim ScalingImplemented As Boolean
        ' ************ Scaling *************

        Dim SInfo() As ScalingInfo
        ReDim SInfo(NCat(iCat))

        ReDim index1(NCat(iCat))


        'ReDim Scaled(NCat(iCat))
        'ReDim ScaleFactorA(NCat(iCat))


        'Stress1 = -40 * LEAStructure.Thick(1) + 850 + LEAAircraft(ACIndex).GearLoad * 0.0002
        'Stress1 = 20
        'Exit Sub

        'GoTo gotoIngrid
        ScalingImplemented = True
        Sorting = True

        If NCat(iCat) = 1 Then 'NCat() - number of aircraft in particular category
            Sorting = False
            ScalingImplemented = False
        End If

        ChDir(VB6.GetPath)
        ExtFilePath = VB6.GetPath & "\"

        If StressType = "Edge" Then
            EdgeLoad = True
        Else
            EdgeLoad = False
        End If



        'GearAngle = GA      '90-perpendicular to edge ' 0-parallel to edge         'suppressed by YGC 102213 


        Call frmStructure.frmStructure_Load(LEAStructure)



        If Sorting Then
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   WITH SORTING   *****************************
            Dim i1, i2 As Integer

            i2 = 0
            For i1 = 1 To gNACarg

                'If GroupIndex(i1) = iCat And gCalcStress(i1) = 0 Then 'modified to compute all aircraft in 1 step by YGC 102213
                If gCalcStress(i1) = 0 Then

                    i2 += 1
                    index1(i2) = i1
                End If
            Next


            For i1 = 1 To NCat(iCat)
                WheelLoad(i1, 1) = CDbl(LEAAircraft(index1(i1)).GearLoad /
                                        LEAAircraft(index1(i1)).NTires)
                WheelLoad(i1, 2) = i1
            Next

            Call PIKSTR2(NCat(iCat), WheelLoad, WheelLoad2)
            'WheelLoad2 - indexes of aircraft sorted array according to wheel load - descending


            'ACIndex = CShort(WheelLoad2(1, 2)) 'index for first aircraft in the group
            ACIndex = CShort(index1(CInt(WheelLoad2(1, 2))))

            GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213

            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                                         ' QW 09-15-2019
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '


            Dim icc As Short
            For icc = 2 To NCat(iCat)
                'ACIndex = CShort(WheelLoad2(icc, 2))
                ACIndex = CShort(index1(CInt(WheelLoad2(icc, 2))))

                GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213


                ScalingImplemented = False 'Izydor 2014.04.30
                ScalingImplemented = True
                ' ****************** scaling begin ******************
                If ScalingImplemented Then
                    If (LEAAircraft(ACIndex).GearLoad >
                        LEAAircraft(PreviousIndex).GearLoad) Then
                        Diff = CDbl((LEAAircraft(ACIndex).GearLoad -
                                     LEAAircraft(PreviousIndex).GearLoad) /
                                     LEAAircraft(PreviousIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 1.15
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 1.5
                            'ScaleFactorA(icc) = 2
                        End If
                    Else
                        Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                     LEAAircraft(ACIndex).GearLoad) /
                                     LEAAircraft(ACIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 0.75
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 0.5
                        End If
                    End If


                    If Diff < DiffNum Then
                        SInfo(icc).Scaled = True
                        Dim TireNumb As Integer
                        For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                            LEAAircraft(ACIndex).TirePress(TireNumb) =
                            LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                        Next
                        LEAAircraft(ACIndex).GearLoad =
                        LEAAircraft(ACIndex).GearLoad * SInfo(icc).ScaleFactorA

                        SInfo(icc).index2 = ACIndex
                    End If
                    'PreviousIndex = icc
                    PreviousIndex = ACIndex
                End If
                ' ****************** scaling end ******************


                Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                Call GearLoads(ACIndex, LEAAircraft)
                'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                Call WriteTextFile(OpenMode.Append, rgdFile)
            Next icc
            '**************   WITH SORTING   *****************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        Else

            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   without sorting   *****************************
            ACIndex = firstACCat(iCat) 'index for first aircraft in the group

            GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213

            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                                                  ' QW 09-15-201
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '


            Dim icc As Short
            For icc = (firstACCat(iCat) + 1S) To CShort(gNACarg)
                'If GroupIndex(icc) = iCat Then   'suppressed to compute all aircraft in 1 step by YGC 102213
                ACIndex = icc

                GearAngle = GA(ACIndex)      'added to define gear orientation by YGC 102213

                ' ****************** scaling begin ******************
                If ScalingImplemented Then
                    If (LEAAircraft(icc).GearLoad >
                        LEAAircraft(PreviousIndex).GearLoad) Then
                        Diff = CDbl((LEAAircraft(icc).GearLoad -
                                     LEAAircraft(PreviousIndex).GearLoad) /
                                     LEAAircraft(PreviousIndex).GearLoad)
                    Else
                        Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                     LEAAircraft(icc).GearLoad) /
                                     LEAAircraft(icc).GearLoad)
                    End If


                    If Diff < DiffNum Then
                        SInfo(icc).Scaled = True
                        Dim TireNumb As Integer
                        For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                            LEAAircraft(ACIndex).TirePress(TireNumb) =
                            LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                        Next
                        LEAAircraft(icc).GearLoad =
                        LEAAircraft(icc).GearLoad * SInfo(icc).ScaleFactorA
                    End If
                    PreviousIndex = icc
                End If
                ' ****************** scaling end ******************

                Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                Call GearLoads(ACIndex, LEAAircraft)

                'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                Call WriteTextFile(OpenMode.Append, rgdFile)
                'End If     'suppressed to compute all aircraft in 1 step by YGC 102213
            Next icc
            '*******************   without sorting   ************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        End If


gotoIngrid:
        'GoTo gotoNike3d

        'modified for user-specified directory by YGC 101012
        'Call INGRIDMAIN() 
        'Dim WorkingDir0 As String  'moevd by YGC 112213
        WorkingDir0 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\"

        'add Ingrill exception error handeling by YC 052416
        'Call INGRIDMAIN(WorkingDir0, Len(WorkingDir0))
        'modify ended for user-specified directory by YGC 101012

        Dim txt, title As String
        Dim response As MsgBoxResult
        Dim IUnitErr0, IErr0 As Short
        Dim ErrFlNm As String = Nothing

        Try

            'Call INGRIDMAIN(IUnitErr0, IErr0, WorkingDir0, Len(WorkingDir0))

            If IErr0 = 0 Then GoTo IngridDoneLabel

            If IUnitErr0 = 13 Then
                ErrFlNm = "nikein"
            ElseIf IUnitErr0 = 41 Then
                ErrFlNm = "isave"
            ElseIf IUnitErr0 = 15 Then
                ErrFlNm = "tape15"
            ElseIf IUnitErr0 = 18 Then
                ErrFlNm = "tape18"
            ElseIf IUnitErr0 = 7 Then
                ErrFlNm = "tape7"
            ElseIf IUnitErr0 = 8 Then
                ErrFlNm = "tape8"
            ElseIf IUnitErr0 = 9 Then
                ErrFlNm = "tape9"
            Else
                ErrFlNm = "unknown"
            End If

            txt = "File " & ErrFlNm & ", Error Code " & IErr0 & Environment.NewLine

            title = "Ingrid Fortran Error"

        Catch ex As Exception
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            'txt = txt + ex.StackTrace
            'txt = txt + Environment.NewLine + Environment.NewLine

            title = "Ingrid DLL Exception Error"

        End Try

        txt = txt & Environment.NewLine & Environment.NewLine & "FAARFIELD could not complete the design. Retry?"

        response = MsgBox(txt, MsgBoxStyle.YesNo, title)

        If response = MsgBoxResult.Yes Then
            GoTo gotoIngrid
        Else
            gUserInterrupted = True
            Exit Sub
        End If

IngridDoneLabel:
        'add Ingrill exception error handeling by YC 052416 END



        'Call Ingrid()
        System.Windows.Forms.Application.DoEvents()
        '------------------------------------------------------------------------------
        Dim StopFEDFAA As Short, FEDFAAStopped As Short
        Static AtomName As New VB6.FixedLengthString(10)
        Dim II As Integer, i As Integer
        Dim ACDATPath As String, DesigningStr As Boolean

        ACDATPath = Environment.CurrentDirectory
        DesigningStr = False
        AtomName.Value = "StopFEDFAA"

        Do
            II = GlobalFindAtom(AtomName.Value)
            If II <> 0 Then i = GlobalDeleteAtom(CShort(II))
        Loop Until II = 0
        StopFEDFAA = 0 : FEDFAAStopped = 0

        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped)
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, MeshCase, CShort(gDesignType))
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), MeshCase)

        If MeshCase = "1DSYM" Then
            iSymCase = 1S
        ElseIf MeshCase = "2DSYM" Then
            iSymCase = 2S
        ElseIf MeshCase = "3DSYM" Then
            iSymCase = 3S
        ElseIf MeshCase = "4DNSY" Then
            iSymCase = 4S
        ElseIf MeshCase = "5DSYM" Then
            iSymCase = 5S
        End If

        Dim tStart, tEnd As Date, tdiff As Long
        Dim d1, d2 As Double, St As String
        Dim timeSave1, timeSave2 As Integer, timeDiff As Double
        tStart = Now()
        d1 = TimeOfDay.ToOADate
        timeSave1 = timeGetTime

        'Modified to use printed stress by YGC 112213
        'YGC 101012
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase)
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))


        ' ****************** scaling begin ****************** scaling back !!!!
        'If ScalingImplemented Then
        '    Dim in1 As Integer
        '    For in1 = 1 To 2 Step UBound(SInfo, 1)

        '        If SInfo(in1).Scaled = True Then

        '            Dim TireNumb As Integer
        '            For TireNumb = 1 To LEAAircraft(ACIndex).NTires
        '                LEAAircraft(ACIndex).TirePress(TireNumb) = _
        '                LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(in1).ScaleFactorA
        '            Next
        '            LEAAircraft(ACIndex).GearLoad = _
        '            LEAAircraft(ACIndex).GearLoad * SInfo(in1).ScaleFactorA

        '            SInfo(in1).index2 = ACIndex
        '        End If
        '    Next in1
        'End If
        ' ****************** scaling end ******************



        ' recovered to use transfered stress by YC 052615
        'Call NIKE3D(StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))
        'Call Readstressmax(Stress1, Stress8, zstress33, stress33)
        ''Modified to use printed stress by YGC 112213




        ' add Nike3D exception error handeling by YC 052416
        'Call NIKE3D(Stress1(1), Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))
        ' recovered to use transfered stress by YC 052615 END

        'Exit Sub 'to comment

Nike3DStartLabel:
        Try
            Call NIKE3D(Stress1(1), Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))

            txt = "File " & ErrFlNm & ", Error Code " & IErr0 & Environment.NewLine

            title = "Nike3D Fortran Error"

        Catch ex As Exception

            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            'txt = txt + ex.StackTrace
            'txt = txt + Environment.NewLine + Environment.NewLine

            title = "Nike3D DLL Exception Error"

            txt = txt & Environment.NewLine & Environment.NewLine & "FAARFIELD could not complete the design. Retry?"

            response = MsgBox(txt, MsgBoxStyle.YesNo, title)

            If response = MsgBoxResult.Yes Then
                GoTo Nike3DStartLabel
            Else
                gUserInterrupted = True
                Exit Sub
            End If

        End Try
        'add Nike3D exception error handeling by YC 052416 END





        timeSave2 = timeGetTime
        timeDiff = CDbl((timeSave2 - timeSave1) / 1000)

        d2 = TimeOfDay.ToOADate

        St = VB6.Format(System.DateTime.FromOADate(d2 - d1), "hh:mm:ss")
        tEnd = Now()

        tdiff = DateDiff(DateInterval.Second, tStart, tEnd, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)


        'Dim FileNoZ As Integer, FileNameZ As String 'IK 2016.05.31 clsAM
        'FileNoZ = FreeFile()
        'FileNameZ = WorkingDir0 & "\Nike3DTime.dat"
        'FileOpen(FileNoZ, FileNameZ, OpenMode.Append)
        'PrintLine(FileNoZ, timeSave1 & "   " & timeSave2 & "   " & timeDiff)
        '     PrintLine(FileNoZ, d1 & "    " & d2 & "   " & tdiff)
        'FileClose(FileNoZ)






        StopFEDFAA = -1
        'II = GlobalFindAtom(AtomName.Value)

        System.Windows.Forms.Application.DoEvents()
        System.Diagnostics.Debug.WriteLine("StopFEDFAA = " & StopFEDFAA)
        Do
            System.Windows.Forms.Application.DoEvents()
            If FEDFAAStopped = -1 Then
                System.Diagnostics.Debug.WriteLine("FEDFAAStopped = " & FEDFAAStopped)
                Exit Do
            End If
        Loop

        Do
            ' The Dir$ function is very time consuming, therefore use
            ' a For Loop for DoEvents.
            ' 1,000 times through the loop is the best compromise for long
            ' Julea executions and short Julea executions. Marking the end of
            ' a Julea computation with an atom does not reduce total time much.
            For i = 1 To 1000
                System.Windows.Forms.Application.DoEvents()
                If DesigningStr = False Then ' Stop Julea
                    II = GlobalAddAtom(AtomName.Value)
                    System.Diagnostics.Debug.WriteLine("AddAtom = " & II)
                    '        StartTime = Timer
                    Do
                        System.Windows.Forms.Application.DoEvents() ' Let Julea delete the atom.
                        '          ETime = Abs(Timer - StartTime)
                        '          Debug.Print ETime
                        II = GlobalFindAtom(AtomName.Value)
                        System.Diagnostics.Debug.WriteLine("FindAtom = " & II)
                        '         Need to check file in case atom added after JULEA stopped
                        '         normally but file writes not finished.
                        'ikawa If II = 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then

                        If II <> 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then
                            System.Diagnostics.Debug.WriteLine("Check before delete " _
                                   & GlobalFindAtom(AtomName.Value))
                            If II <> 0 Then II = GlobalDeleteAtom(CShort(II))
                            System.Diagnostics.Debug.Write("DeleteAtom = " & II)
                            System.Diagnostics.Debug.WriteLine(GlobalFindAtom(AtomName.Value))
                            If Dir(ACDATPath & "JULEA.ERR") = "" Then
                                Exit Do ' Make sure all file activity is finished
                            End If ' before closing Julea below.
                        End If
                    Loop
                    Exit For
                End If
            Next i
            If Dir(ACDATPath & "JULEA.ERR") <> "" Or DesigningStr = False Then
                '      IL = SendMessage(hJULEA, WM_CLOSE, 0&, 0&)
                Exit Do
            End If
        Loop

        'If StopFEDFAA = -1 Then
        '    gUserInterrupted = True
        'End If

        If gUserInterrupted Then Exit Sub 'September 13, 2006
gotoNike3d:


        'For i = 1 To NCat(iCat)
        '    If StressType = "Edge" Then
        '        modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
        '        modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
        '    End If
        'Next i

        For i = 1 To NCat(iCat)
            If StressType = "Edge" Then
                If gDesignType2 = 13 Then 'HMA Overlay on Rigid - FlexOnRigid
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.5
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.5
                Else
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
                End If
            End If
        Next i




        '********************* Scaling Part Begin *********************
        If ScalingImplemented Then
            For i = 1 To NCat(iCat) 'for all aircraft
                If SInfo(i).Scaled Then
                    LEAAircraft(SInfo(i).index2).GearLoad = LEAAircraft(SInfo(i).index2).GearLoad / SInfo(i).ScaleFactorA
                    modWorld.Stress1(i) = modWorld.Stress1(i) / SInfo(i).ScaleFactorA
                    modWorld.Stress8(i) = modWorld.Stress8(i) / SInfo(i).ScaleFactorA

                    'added to scale foundation vertical stress for rigid compaction by YGC 112213
                    Dim k As Integer

                    For k = 1 To gNumberOfFoundationInterfaces
                        stress33(i, k) = stress33(i, k) / SInfo(i).ScaleFactorA
                    Next k
                    'added to scale for foundation vertical stress for rigid compaction by YGC 112213 END

                End If
            Next
        End If

        '********************* Scaling Part End *********************
        If Sorting Then
            Dim S11(NCat(iCat)), S88(NCat(iCat)) As Double

            For i = 1 To NCat(iCat)
                S11(i) = modWorld.Stress1(i)
                S88(i) = modWorld.Stress8(i)
            Next

            For i = 1 To NCat(iCat)
                'modWorld.Stress1(index1(CInt(WheelLoad2(i, 2)))) = S11(i)
                'modWorld.Stress8(index1(CInt(WheelLoad2(i, 2)))) = S88(i)
                modWorld.Stress1(CInt(WheelLoad2(i, 2))) = S11(i)
                modWorld.Stress8(CInt(WheelLoad2(i, 2))) = S88(i)
            Next

            'added to sort foundation vertical stress for rigid compaction by YGC 112213
            Dim S33(NCat(iCat), gNumberOfFoundationInterfaces) As Double
            Dim k As Integer

            For i = 1 To NCat(iCat)
                For k = 1 To gNumberOfFoundationInterfaces
                    S33(i, k) = stress33(i, k)
                Next k
            Next i

            For i = 1 To NCat(iCat)
                For k = 1 To gNumberOfFoundationInterfaces
                    stress33(CInt(WheelLoad2(i, 2)), k) = S33(i, k)
                Next k
            Next i
            'added to sort foundation vertical stress for rigid compaction by YGC 112213 END

        End If

        ACIndex = ACIndex

    End Sub


    Public Sub ComputeResponsebyGear(
     ByVal ResponseType As LEAFClassLib.clsLEAF.LEAFoptions,
     ByVal NACarg As Integer,
     ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
     ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
     ByRef Response(,) As Double,
     ByRef AllResps(,) As LEAFClassLib.clsLEAF.LEAFAllResponses,
     ByRef CalcStress() As Integer,
     ByVal DesignType2 As Integer,
     ByVal SolverType2 As Integer,
     ByVal SlabMeshSize2 As Integer,
     ByVal JobName As String,
     ByRef UserInterrupted As Boolean)


        'added "ByVal SolverType2 As Integer " for solver choice by YGC 083012
        'added "ByVal SlabMeshSize2 As Integer" for slab mesh size selection by YGC 061113

        'subroutine for Multiple aircraft in 1 step, automated by YGC 031312

        'rename ComputeResponse for aircraft in one gear type to ComputeResponsebyGear by YGC 102213

        Try

            'Exit Sub

            'LEAAircraft(1).NTires = 2
            'LEAAircraft(1).GearLoad = LEAAircraft(1).GearLoad * LEAAircraft(1).NTires / 14

            Dim I As Short, ACIndex As Short
            Dim EdgeStressFEM0PCC(gAC), EdgeStressFEM90PCC(gAC) As Double
            Dim EdgeStressFEM0Overlay(gAC), EdgeStressFEM90Overlay(gAC) As Double
            Dim InteriorStressLEAF, InteriorStressFEM(gAC) As Double
            Dim InteriorStressLEAFOverlay, InteriorStressFEMOverlay(gAC) As Double

            'Dim MaxStressPCC(gAC), MaxStressOverlay(gAC) As Double
            Dim PrintResults As Boolean
            Dim DesignType As String


            Dim t1 As Date, tdiff As Long
            Dim ThinLayer As Boolean

            'DesignType2 = 1

            'Public Const NewFlex As Short = 1
            'Public Const FlexOnFlex As Short = 2
            'Public Const PCCOnFlex As Short = 3

            'Public Const NewRigid As Short = 10
            'Public Const UnbondOnRigid As Short = 11
            'Public Const PartBondOnRigid As Short = 12
            'Public Const FlexOnRigid As Short = 13

            SolverType = SolverType2 'added for solver choice by YGC 083012
            SlabMeshSize = SlabMeshSize2 'added for slab mesh size selection by YGC 061113

            gearOrient = CShort(LEAStructure.lngDummy(1))


            'Izydor Kawa 10/16/2012
            rgdFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.rgd"
            ingFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.ing"


            ThinLayer = False
            If DesignType2 = 10 Then

            ElseIf DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                If LEAStructure.NLayers = 3 Then
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(4)
                    ReDim Preserve LEAStructure.Poisson(4)
                    ReDim Preserve LEAStructure.Thick(4)
                    ReDim Preserve LEAStructure.InterfaceParm(4)

                    LEAStructure.Modulus(4) = LEAStructure.Modulus(3)

                    LEAStructure.Thick(4) = LEAStructure.Thick(3)
                    LEAStructure.Thick(3) = 4

                    LEAStructure.InterfaceParm(4) = LEAStructure.InterfaceParm(3)
                    'LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(2)

                    LEAStructure.Poisson(4) = LEAStructure.Poisson(3)
                    'LEAStructure.Poisson(3) = 0.35
                    ThinLayer = True

                End If
            End If



            'Call INGRIDMAIN()
            'Call INGRID()

            For I = 1 To CShort(NACarg)
                gCalcStress(I) = CalcStress(I)
            Next

            'LEAAircraft(2).GearLoad = 999
            'Exit Sub

            If UserInterrupted Then Exit Sub 'September 13, 2006

            gUserInterrupted = UserInterrupted
            gDesignType = DesignType2
            gNACarg = NACarg
            PrintResults = False
            Overlay = False
            ReDim NCat(gMeshCategories)
            ReDim firstACCat(gMeshCategories)
            ReDim GroupIndex(gAC * 2)

            '.InterfaceParm(1) = 1 And .InterfaceParm(1) = 0 FlexOnRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(1) = 1 NewRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(2) = 0 UnbondOnRigid
            '.InterfaceParm(1) > 0 And .InterfaceParm(1) < 1 PartBondOnRigid

            If LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "FlexOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 1 Then
                DesignType = "NewRigid"
                Overlay = False
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "UnbondOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) > 0 And LEAStructure.InterfaceParm(1) < 1 Then
                DesignType = "PartBondOnRigid"
                Overlay = True



            End If


            If Overlay Then
                ReDim Response(NACarg, 2)
            Else
                ReDim Response(NACarg, 8)
            End If


            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                PrintLine(3, "NIKE3D   " _
                    & LPad(13, "Aircraft") _
                    & LPad(10, "Edge0") _
                    & LPad(10, "Edge90") _
                    & LPad(10, "IntFEM") _
                    & LPad(10, "MaxS") _
                    & LPad(10, "IntLEAF") _
                    & LPad(12, "Thick= " _
                    & VB6.Format(LEAStructure.Thick(1), "#0.0000")) _
                    & LPad(22, CStr(Now)))
                FileClose(3)
            End If

            t1 = Now()
            System.Windows.Forms.Application.DoEvents()



            For ACIndex = 1 To CShort(NACarg)
                If CalcStress(ACIndex) = 0 Then
                    If LEAAircraft(ACIndex).ACname = "C-5" Then
                        LEAAircraft(ACIndex).NTires = 6
                        LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex).GearLoad / 2
                    End If


                    ''''If LEAAircraft(ACIndex).ACname = "C-130" Then
                    ''''    Dim x1, x2, y1, y2 As Double
                    ''''    x1 = CDbl(LEAAircraft(ACIndex).TireX(1))
                    ''''    x2 = CDbl(LEAAircraft(ACIndex).TireX(2))
                    ''''    y1 = CDbl(LEAAircraft(ACIndex).TireY(1))
                    ''''    y2 = CDbl(LEAAircraft(ACIndex).TireY(2))
                    ''''    LEAAircraft(ACIndex).TireX(1) = y1
                    ''''    LEAAircraft(ACIndex).TireX(2) = y2
                    ''''    LEAAircraft(ACIndex).TireY(1) = x1
                    ''''    LEAAircraft(ACIndex).TireY(2) = x2
                    ''''End If



                    If LEAAircraft(ACIndex).libGear = "A" _
                       Or LEAAircraft(ACIndex).libGear = "B" _
                       Or LEAAircraft(ACIndex).libGear = "D" _
                       Or (LEAAircraft(ACIndex).libGear = "HB" And
                       LEAAircraft(ACIndex).NTires = 2) Then
                        NCat(1) += 1S
                        GroupIndex(ACIndex) = 1
                        If firstACCat(1) = 0 Then
                            firstACCat(1) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).libGear = "F" _
                          Or LEAAircraft(ACIndex).libGear = "E" _
                          Or LEAAircraft(ACIndex).libGear = "H" _
                          Or LEAAircraft(ACIndex).libGear = "WFBF" _
                          Or LEAAircraft(ACIndex).libGear = "WFBN" _
                          Or LEAAircraft(ACIndex).ACname = "IL86" _
                          Or (LEAAircraft(ACIndex).libGear = "HB" And
                           LEAAircraft(ACIndex).NTires = 4) Then
                        NCat(2) += 1S
                        GroupIndex(ACIndex) = 2
                        If firstACCat(2) = 0 Then
                            firstACCat(2) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).libGear = "N" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" Then
                        NCat(3) += 1S
                        GroupIndex(ACIndex) = 3
                        If firstACCat(3) = 0 Then
                            firstACCat(3) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).libGear = "M" _
                        Or LEAAircraft(ACIndex).libGear = "K" _
                        Or LEAAircraft(ACIndex).ACname = "IL76T" _
                        Or LEAAircraft(ACIndex).ACname = "00000" Then
                        NCat(4) += 1S
                        GroupIndex(ACIndex) = 4
                        If firstACCat(4) = 0 Then
                            firstACCat(4) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).ACname = "An-124" _
                        Or LEAAircraft(ACIndex).ACname = "An-225" Then
                        NCat(5) += 1S
                        GroupIndex(ACIndex) = 5
                        If firstACCat(5) = 0 Then
                            firstACCat(5) = ACIndex
                        End If
                    Else 'undetermined aircrafts

                    End If
                End If
            Next ACIndex


            For iCat = 1 To gMeshCategories 'four mesh categories ***
                If NCat(iCat) > 0 Then 'are there any aircraft in this category
                    If iCat = 1 Then
                        'default 0
                        MeshCase = "1DSYM"
                        Call GetFEMStressbyGear(LEAAircraft, LEAStructure, ACIndex, "Edge", 0)
                        If gUserInterrupted Then Exit Sub
                    ElseIf iCat = 2 Then
                        MeshCase = "2DSYM"
                        'default 90
                        'Call GetFEMStress(LEAAircraft, LEAStructure, ACIndex, "Edge", 90)
                        Call GetFEMStressbyGear(LEAAircraft, LEAStructure, ACIndex, "Edge", gearOrient)
                        If gUserInterrupted Then Exit Sub
                    ElseIf iCat = 3 Then
                        'default 90 B777
                        MeshCase = "3DSYM"
                        Call GetFEMStressbyGear(LEAAircraft, LEAStructure, ACIndex, "Edge", 90)
                        If gUserInterrupted Then Exit Sub
                    ElseIf iCat = 4 Then
                        'no default
                        MeshCase = "4DNSY"
                        Call GetFEMStressbyGear(LEAAircraft, LEAStructure, ACIndex, "Edge", 0)
                        If gUserInterrupted Then Exit Sub
                    ElseIf iCat = 5 Then
                        MeshCase = "5DSYM"
                        Call GetFEMStressbyGear(LEAAircraft, LEAStructure, ACIndex, "Edge", 90)
                        If gUserInterrupted Then Exit Sub
                    End If


                    Dim kk, ii As Integer
                    kk = 0
                    For ii = 1 To gNACarg
                        If GroupIndex(ii) = iCat Then
                            If CalcStress(ii) = 0 Then
                                kk += 1
                                If iCat = 1 Then
                                    EdgeStressFEM0PCC(ii) = modWorld.Stress1(kk)
                                    EdgeStressFEM0Overlay(ii) = modWorld.Stress8(kk)
                                ElseIf iCat = 2 Or iCat = 3 Or iCat = 5 Then
                                    EdgeStressFEM90PCC(ii) = modWorld.Stress1(kk)
                                    EdgeStressFEM90Overlay(ii) = modWorld.Stress8(kk)
                                ElseIf iCat = 4 Then
                                    EdgeStressFEM0PCC(ii) = modWorld.Stress1(kk)
                                    EdgeStressFEM0Overlay(ii) = modWorld.Stress8(kk)
                                End If
                            End If
                        End If
                    Next ii

                End If 'are there any aircraft in this category
            Next iCat 'four mesh categories ***


            If Overlay Then
                For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                    If GroupIndex(ACIndex) = 1 Then
                        Response(ACIndex, 1) = EdgeStressFEM0Overlay(ACIndex)
                        Response(ACIndex, 2) = EdgeStressFEM0PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 2 Or GroupIndex(ACIndex) = 3 Or GroupIndex(ACIndex) = 5 Then
                        Response(ACIndex, 1) = EdgeStressFEM90Overlay(ACIndex)
                        Response(ACIndex, 2) = EdgeStressFEM90PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 4 Then
                        Response(ACIndex, 1) = EdgeStressFEM0Overlay(ACIndex)
                        Response(ACIndex, 2) = EdgeStressFEM0PCC(ACIndex)
                    End If
                Next ACIndex
            Else
                For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                    If GroupIndex(ACIndex) = 1 Then
                        Response(ACIndex, 1) = EdgeStressFEM0PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 2 Or GroupIndex(ACIndex) = 3 Or GroupIndex(ACIndex) = 5 Then
                        Response(ACIndex, 1) = EdgeStressFEM90PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 4 Then
                        Response(ACIndex, 1) = EdgeStressFEM0PCC(ACIndex)
                    End If
                Next ACIndex
            End If


            'ikawa 
            For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                If InStr(LEAAircraft(ACIndex).ACname, "747", CompareMethod.Text) > 0 _
                   And InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) > 0 Then
                    Dim ACIndex2 As Integer, pos1, pos2 As Integer
                    pos1 = InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) - 2
                    For ACIndex2 = 1 To CShort(NACarg) 'for all aircraft
                        pos2 = InStr(LEAAircraft(ACIndex2).ACname, "Body", CompareMethod.Text) - 2
                        If pos2 > 0 Then
                            If (Not ACIndex = ACIndex2) _
                               And Mid(LEAAircraft(ACIndex).ACname, 1, pos1) = Mid(LEAAircraft(ACIndex2).ACname, 1, pos2) _
                               And LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex2).GearLoad Then
                                Response(ACIndex2, 1) = Response(ACIndex, 1)
                                If Overlay Then
                                    Response(ACIndex2, 2) = Response(ACIndex, 2)
                                End If
                            End If
                        End If
                    Next ACIndex2
                End If
            Next ACIndex



            If ThinLayer Then
                ThinLayer = False

                If DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                    'return to previous values
                    LEAStructure.Thick(3) = LEAStructure.Thick(4)
                    LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(4)
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(3)
                    ReDim Preserve LEAStructure.Poisson(3)
                    ReDim Preserve LEAStructure.Thick(3)
                    ReDim Preserve LEAStructure.InterfaceParm(3)

                End If

            End If




            tdiff = DateDiff(DateInterval.Second, t1, Now(), FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)

            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                For ACIndex = 1 To CShort(NACarg)
                    Dim pr As Integer
                    pr = 1
                    If Overlay Then
                        pr = 2
                        PrintLine(3, LPad(3, CStr(ACIndex)) _
                      & LPad(19, LEAAircraft(ACIndex).ACname) _
                      & LPad(10, VB6.Format(EdgeStressFEM0Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(EdgeStressFEM90Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressFEMOverlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(Response(ACIndex, 1), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressLEAFOverlay, "#,###,###.00")) _
                      & LPad(5, "Over") _
                      & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                      & LPad(10, VB6.Format(LEAStructure.Modulus(1), "#,###,###.00")) _
                      & LPad(23, CStr(Now())))
                    End If

                    PrintLine(3, LPad(3, CStr(ACIndex)) _
                  & LPad(19, LEAAircraft(ACIndex).ACname) _
                  & LPad(10, VB6.Format(EdgeStressFEM0PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(EdgeStressFEM90PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressFEM(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(Response(ACIndex, pr), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressLEAF, "#,###,###.00")) _
                  & LPad(5, "Slab") _
                  & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                  & LPad(10, VB6.Format(LEAStructure.Modulus(pr), "#,###,###.00")) _
                  & LPad(23, CStr(Now())))
                Next ACIndex
                FileClose(3)
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
    Private Sub GetFEMStressbyGear(ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
              ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
              ByVal ACIndex As Short, ByVal StressType As String, ByVal GA As Short)

        'subroutine for Multiple aircraft in 1 step, automated by YGC 031312


        'Microsoft Scripting Runtime checked in Project References
        Dim fso As New Scripting.FileSystemObject(), fil As Boolean
        Dim ExtFilePath As String

        Dim Sorting As Boolean                                            'for sorting
        Dim WheelLoad(NCat(iCat), 2), WheelLoad2(NCat(iCat), 2) As Double 'for sorting
        Dim PreviousIndex As Integer, Diff As Double                      'for scaling 
        Dim DiffNum As Double = 0.15                                        'for scaling 

        Dim index1() As Integer
        'Dim ScaleFactorA() As Double

        ' ************ Scaling *************
        'Dim Scaled(80) As Boolean
        'Public Const ScFactor As Double = 0.25
        Dim ScalingImplemented As Boolean
        ' ************ Scaling *************

        Dim SInfo() As ScalingInfo
        ReDim SInfo(NCat(iCat))

        ReDim index1(NCat(iCat))
        'ReDim Scaled(NCat(iCat))
        'ReDim ScaleFactorA(NCat(iCat))


        'Stress1 = -40 * LEAStructure.Thick(1) + 850 + LEAAircraft(ACIndex).GearLoad * 0.0002
        'Stress1 = 20
        'Exit Sub

        'GoTo gotoIngrid
        ScalingImplemented = True
        Sorting = True

        If NCat(iCat) = 1 Then 'NCat() - number of aircraft in particular category
            Sorting = False
            ScalingImplemented = False
        End If

        ChDir(VB6.GetPath)
        ExtFilePath = VB6.GetPath & "\"

        If StressType = "Edge" Then
            EdgeLoad = True
        Else
            EdgeLoad = False
        End If
        GearAngle = GA      '90-perpendicular to edge ' 0-parallel to edge


        Call frmStructure.frmStructure_Load(LEAStructure)



        If Sorting Then
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   WITH SORTING   *****************************
            Dim i1, i2 As Integer

            i2 = 0
            For i1 = 1 To gNACarg
                If GroupIndex(i1) = iCat And gCalcStress(i1) = 0 Then
                    i2 += 1
                    index1(i2) = i1
                End If
            Next


            For i1 = 1 To NCat(iCat)
                WheelLoad(i1, 1) = CDbl(LEAAircraft(index1(i1)).GearLoad /
                                        LEAAircraft(index1(i1)).NTires)
                WheelLoad(i1, 2) = i1
            Next

            Call PIKSTR2(NCat(iCat), WheelLoad, WheelLoad2)
            'WheelLoad2 - indexes of aircraft sorted array according to wheel load - descending


            'ACIndex = CShort(WheelLoad2(1, 2)) 'index for first aircraft in the group
            ACIndex = CShort(index1(CInt(WheelLoad2(1, 2))))
            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                                              ' QW 09-15-2019
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '


            Dim icc As Short
            For icc = 2 To NCat(iCat)
                ACIndex = CShort(index1(CInt(WheelLoad2(icc, 2))))
                'ACIndex = CShort(WheelLoad2(icc, 2))



                ' ****************** scaling begin ******************
                If ScalingImplemented Then
                    If (LEAAircraft(ACIndex).GearLoad >
                        LEAAircraft(PreviousIndex).GearLoad) Then
                        Diff = CDbl((LEAAircraft(ACIndex).GearLoad -
                                     LEAAircraft(PreviousIndex).GearLoad) /
                                     LEAAircraft(PreviousIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 1.15
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 1.5
                            'ScaleFactorA(icc) = 2
                        End If
                    Else
                        Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                     LEAAircraft(ACIndex).GearLoad) /
                                     LEAAircraft(ACIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 0.75
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 0.5
                        End If
                    End If


                    If Diff < DiffNum Then
                        SInfo(icc).Scaled = True
                        Dim TireNumb As Integer
                        For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                            LEAAircraft(ACIndex).TirePress(TireNumb) =
                            LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                        Next
                        LEAAircraft(ACIndex).GearLoad =
                        LEAAircraft(ACIndex).GearLoad * SInfo(icc).ScaleFactorA

                        SInfo(icc).index2 = ACIndex
                    End If
                    'PreviousIndex = icc
                    PreviousIndex = ACIndex
                End If
                ' ****************** scaling end ******************


                Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                Call GearLoads(ACIndex, LEAAircraft)
                'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                Call WriteTextFile(OpenMode.Append, rgdFile)
            Next icc
            '**************   WITH SORTING   *****************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        Else

            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   without sorting   *****************************
            ACIndex = firstACCat(iCat) 'index for first aircraft in the group
            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                                      ' QW 09-15-2019
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '


            Dim icc As Short
            For icc = (firstACCat(iCat) + 1S) To CShort(gNACarg)
                If GroupIndex(icc) = iCat Then
                    ACIndex = icc

                    ' ****************** scaling begin ******************
                    If ScalingImplemented Then
                        If (LEAAircraft(icc).GearLoad >
                            LEAAircraft(PreviousIndex).GearLoad) Then
                            Diff = CDbl((LEAAircraft(icc).GearLoad -
                                         LEAAircraft(PreviousIndex).GearLoad) /
                                         LEAAircraft(PreviousIndex).GearLoad)
                        Else
                            Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                         LEAAircraft(icc).GearLoad) /
                                         LEAAircraft(icc).GearLoad)
                        End If


                        If Diff < DiffNum Then
                            SInfo(icc).Scaled = True
                            Dim TireNumb As Integer
                            For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                                LEAAircraft(ACIndex).TirePress(TireNumb) =
                                LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                            Next
                            LEAAircraft(icc).GearLoad =
                            LEAAircraft(icc).GearLoad * SInfo(icc).ScaleFactorA
                        End If
                        PreviousIndex = icc
                    End If
                    ' ****************** scaling end ******************

                    Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                    Call GearLoads(ACIndex, LEAAircraft)

                    'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                    Call WriteTextFile(OpenMode.Append, rgdFile)
                End If
            Next icc
            '*******************   without sorting   ************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        End If


gotoIngrid:
        'GoTo gotoNike3d

        'modified for user-specified directory by YGC 101012
        'Call INGRIDMAIN() 
        Dim WorkingDir0 As String
        WorkingDir0 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\" & "FAARFIELD\"
        'Call INGRIDMAIN(WorkingDir0, Len(WorkingDir0)) ' suppressed by YC 052416
        'modify ended for user-specified directory by YGC 101012

        'Call Ingrid()
        System.Windows.Forms.Application.DoEvents()
        '------------------------------------------------------------------------------
        Dim StopFEDFAA As Short, FEDFAAStopped As Short
        Static AtomName As New VB6.FixedLengthString(10)
        Dim II As Integer, i As Integer
        Dim ACDATPath As String, DesigningStr As Boolean

        ACDATPath = Environment.CurrentDirectory
        DesigningStr = False
        AtomName.Value = "StopFEDFAA"

        Do
            II = GlobalFindAtom(AtomName.Value)
            If II <> 0 Then i = GlobalDeleteAtom(CShort(II))
        Loop Until II = 0
        StopFEDFAA = 0 : FEDFAAStopped = 0

        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped)
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, MeshCase, CShort(gDesignType))
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), MeshCase)

        If MeshCase = "1DSYM" Then
            iSymCase = 1S
        ElseIf MeshCase = "2DSYM" Then
            iSymCase = 2S
        ElseIf MeshCase = "3DSYM" Then
            iSymCase = 3S
        ElseIf MeshCase = "4DNSY" Then
            iSymCase = 4S
        ElseIf MeshCase = "5DSYM" Then
            iSymCase = 5S
        End If

        Dim tStart, tEnd As Date, tdiff As Long
        Dim d1, d2 As Double, St As String
        Dim timeSave1, timeSave2 As Integer, timeDiff As Double
        tStart = Now()
        d1 = TimeOfDay.ToOADate
        timeSave1 = timeGetTime

        'Modified to use printed stress by YGC 112213
        'YGC 101012
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase)

        ' recovered to use transfered stress by YC 052615
        Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))
        'Call NIKE3D(StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase, WorkingDir0, Len(WorkingDir0))
        'Modified to use printed stress by YGC 112213 END
        ' recovered to use transfered stress by YC 052615 END

        timeSave2 = timeGetTime
        timeDiff = CDbl((timeSave2 - timeSave1) / 1000)

        d2 = TimeOfDay.ToOADate

        St = VB6.Format(System.DateTime.FromOADate(d2 - d1), "hh:mm:ss")
        tEnd = Now()

        tdiff = DateDiff(DateInterval.Second, tStart, tEnd, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)

        StopFEDFAA = -1

        'II = GlobalFindAtom(AtomName.Value)

        System.Windows.Forms.Application.DoEvents()
        System.Diagnostics.Debug.WriteLine("StopFEDFAA = " & StopFEDFAA)
        Do
            System.Windows.Forms.Application.DoEvents()
            If FEDFAAStopped = -1 Then
                System.Diagnostics.Debug.WriteLine("FEDFAAStopped = " & FEDFAAStopped)
                Exit Do
            End If
        Loop

        Do
            ' The Dir$ function is very time consuming, therefore use
            ' a For Loop for DoEvents.
            ' 1,000 times through the loop is the best compromise for long
            ' Julea executions and short Julea executions. Marking the end of
            ' a Julea computation with an atom does not reduce total time much.
            For i = 1 To 1000
                System.Windows.Forms.Application.DoEvents()
                If DesigningStr = False Then ' Stop Julea
                    II = GlobalAddAtom(AtomName.Value)
                    System.Diagnostics.Debug.WriteLine("AddAtom = " & II)
                    '        StartTime = Timer
                    Do
                        System.Windows.Forms.Application.DoEvents() ' Let Julea delete the atom.
                        '          ETime = Abs(Timer - StartTime)
                        '          Debug.Print ETime
                        II = GlobalFindAtom(AtomName.Value)
                        System.Diagnostics.Debug.WriteLine("FindAtom = " & II)
                        '         Need to check file in case atom added after JULEA stopped
                        '         normally but file writes not finished.
                        'ikawa If II = 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then

                        If II <> 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then
                            System.Diagnostics.Debug.WriteLine("Check before delete " _
                                   & GlobalFindAtom(AtomName.Value))
                            If II <> 0 Then II = GlobalDeleteAtom(CShort(II))
                            System.Diagnostics.Debug.Write("DeleteAtom = " & II)
                            System.Diagnostics.Debug.WriteLine(GlobalFindAtom(AtomName.Value))
                            If Dir(ACDATPath & "JULEA.ERR") = "" Then
                                Exit Do ' Make sure all file activity is finished
                            End If ' before closing Julea below.
                        End If
                    Loop
                    Exit For
                End If
            Next i
            If Dir(ACDATPath & "JULEA.ERR") <> "" Or DesigningStr = False Then
                '      IL = SendMessage(hJULEA, WM_CLOSE, 0&, 0&)
                Exit Do
            End If
        Loop

        'If StopFEDFAA = -1 Then
        '    gUserInterrupted = True
        'End If

        If gUserInterrupted Then Exit Sub 'September 13, 2006
gotoNike3d:


        'For i = 1 To NCat(iCat)
        '    If StressType = "Edge" Then
        '        modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
        '        modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
        '    End If
        'Next i


        For i = 1 To NCat(iCat)
            If StressType = "Edge" Then
                If gDesignType2 = 13 Then 'HMA Overlay on Rigid - FlexOnRigid
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.5
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.5
                Else
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
                End If
            End If
        Next i


        '********************* Scaling Part Begin *********************
        If ScalingImplemented Then
            For i = 1 To NCat(iCat) 'for all aircraft
                If SInfo(i).Scaled Then
                    LEAAircraft(SInfo(i).index2).GearLoad = LEAAircraft(SInfo(i).index2).GearLoad / SInfo(i).ScaleFactorA
                    modWorld.Stress1(i) = modWorld.Stress1(i) / SInfo(i).ScaleFactorA
                    modWorld.Stress8(i) = modWorld.Stress8(i) / SInfo(i).ScaleFactorA
                End If
            Next
        End If

        '********************* Scaling Part End *********************
        If Sorting Then
            Dim S11(NCat(iCat)), S88(NCat(iCat)) As Double

            For i = 1 To NCat(iCat)
                S11(i) = modWorld.Stress1(i)
                S88(i) = modWorld.Stress8(i)
            Next

            For i = 1 To NCat(iCat)
                'modWorld.Stress1(index1(CInt(WheelLoad2(i, 2)))) = S11(i)
                'modWorld.Stress8(index1(CInt(WheelLoad2(i, 2)))) = S88(i)
                modWorld.Stress1(CInt(WheelLoad2(i, 2))) = S11(i)
                modWorld.Stress8(CInt(WheelLoad2(i, 2))) = S88(i)
            Next

        End If

        ACIndex = ACIndex

    End Sub


    Public Sub ComputeResponse1AC(
ByVal ResponseType As LEAFClassLib.clsLEAF.LEAFoptions,
ByVal NACarg As Integer,
ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
ByRef Response(,) As Double,
ByRef AllResps(,) As LEAFClassLib.clsLEAF.LEAFAllResponses,
ByRef CalcStress() As Integer,
ByVal DesignType2 As Integer, ByVal JobName As String,
ByRef UserInterrupted As Boolean)

        'subroutine for 1 aircraft in 1 step, automated by YGC 031312

        Try

            'LEAAircraft(1).NTires = 2
            'LEAAircraft(1).GearLoad = LEAAircraft(1).GearLoad * LEAAircraft(1).NTires / 14

            Dim I As Short, ACIndex As Short
            Dim EdgeStressFEM0PCC(gAC), EdgeStressFEM90PCC(gAC) As Double
            Dim EdgeStressFEM0Overlay(gAC), EdgeStressFEM90Overlay(gAC) As Double
            Dim InteriorStressLEAF, InteriorStressFEM(gAC) As Double
            Dim InteriorStressLEAFOverlay, InteriorStressFEMOverlay(gAC) As Double

            'Dim MaxStressPCC(gAC), MaxStressOverlay(gAC) As Double
            Dim PrintResults As Boolean
            Dim DesignType As String

            Dim t1 As Date, tdiff As Long
            Dim ThinLayer As Boolean

            'DesignType2 = 1

            'Public Const NewFlex As Short = 1
            'Public Const FlexOnFlex As Short = 2
            'Public Const PCCOnFlex As Short = 3

            'Public Const NewRigid As Short = 10
            'Public Const UnbondOnRigid As Short = 11
            'Public Const PartBondOnRigid As Short = 12
            'Public Const FlexOnRigid As Short = 13


            ThinLayer = False
            If DesignType2 = 10 Then

            ElseIf DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                If LEAStructure.NLayers = 3 Then
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(4)
                    ReDim Preserve LEAStructure.Poisson(4)
                    ReDim Preserve LEAStructure.Thick(4)
                    ReDim Preserve LEAStructure.InterfaceParm(4)

                    LEAStructure.Modulus(4) = LEAStructure.Modulus(3)

                    LEAStructure.Thick(4) = LEAStructure.Thick(3)
                    LEAStructure.Thick(3) = 4

                    LEAStructure.InterfaceParm(4) = LEAStructure.InterfaceParm(3)
                    'LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(2)

                    LEAStructure.Poisson(4) = LEAStructure.Poisson(3)
                    'LEAStructure.Poisson(3) = 0.35
                    ThinLayer = True

                End If
            End If



            'Call INGRIDMAIN()
            'Call INGRID()

            For I = 1 To CShort(NACarg)
                gCalcStress(I) = CalcStress(I)
            Next

            'LEAAircraft(2).GearLoad = 999
            'Exit Sub

            If UserInterrupted Then Exit Sub 'September 13, 2006

            gUserInterrupted = UserInterrupted
            gDesignType = DesignType2
            gNACarg = NACarg
            PrintResults = False
            Overlay = False
            ReDim NCat(gMeshCategories)
            ReDim firstACCat(gMeshCategories)
            ReDim GroupIndex(gAC * 2)

            '.InterfaceParm(1) = 1 And .InterfaceParm(1) = 0 FlexOnRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(1) = 1 NewRigid
            '.InterfaceParm(1) = 0 And .InterfaceParm(2) = 0 UnbondOnRigid
            '.InterfaceParm(1) > 0 And .InterfaceParm(1) < 1 PartBondOnRigid

            If LEAStructure.InterfaceParm(1) = 1 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "FlexOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 1 Then
                DesignType = "NewRigid"
                Overlay = False
            ElseIf LEAStructure.InterfaceParm(1) = 0 And LEAStructure.InterfaceParm(2) = 0 Then
                DesignType = "UnbondOnRigid"
                Overlay = True
            ElseIf LEAStructure.InterfaceParm(1) > 0 And LEAStructure.InterfaceParm(1) < 1 Then
                DesignType = "PartBondOnRigid"
                Overlay = True



            End If


            If Overlay Then
                ReDim Response(NACarg, 2)
            Else
                ReDim Response(NACarg, 8)
            End If


            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                PrintLine(3, "NIKE3D   " _
                    & LPad(13, "Aircraft") _
                    & LPad(10, "Edge0") _
                    & LPad(10, "Edge90") _
                    & LPad(10, "IntFEM") _
                    & LPad(10, "MaxS") _
                    & LPad(10, "IntLEAF") _
                    & LPad(12, "Thick= " _
                    & VB6.Format(LEAStructure.Thick(1), "#0.0000")) _
                    & LPad(22, CStr(Now)))
                FileClose(3)
            End If

            t1 = Now()
            System.Windows.Forms.Application.DoEvents()



            For ACIndex = 1 To CShort(NACarg)
                If CalcStress(ACIndex) = 0 Then
                    If LEAAircraft(ACIndex).ACname = "C-5" Then
                        LEAAircraft(ACIndex).NTires = 6
                        LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex).GearLoad / 2
                    End If


                    ''''If LEAAircraft(ACIndex).ACname = "C-130" Then
                    ''''    Dim x1, x2, y1, y2 As Double
                    ''''    x1 = CDbl(LEAAircraft(ACIndex).TireX(1))
                    ''''    x2 = CDbl(LEAAircraft(ACIndex).TireX(2))
                    ''''    y1 = CDbl(LEAAircraft(ACIndex).TireY(1))
                    ''''    y2 = CDbl(LEAAircraft(ACIndex).TireY(2))
                    ''''    LEAAircraft(ACIndex).TireX(1) = y1
                    ''''    LEAAircraft(ACIndex).TireX(2) = y2
                    ''''    LEAAircraft(ACIndex).TireY(1) = x1
                    ''''    LEAAircraft(ACIndex).TireY(2) = x2
                    ''''End If



                    If LEAAircraft(ACIndex).libGear = "A" _
                       Or LEAAircraft(ACIndex).libGear = "B" _
                       Or LEAAircraft(ACIndex).libGear = "D" _
                       Or (LEAAircraft(ACIndex).libGear = "HB" And
                       LEAAircraft(ACIndex).NTires = 2) Then
                        NCat(1) += 1S
                        GroupIndex(ACIndex) = 1
                        If firstACCat(1) = 0 Then
                            firstACCat(1) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).libGear = "F" _
                          Or LEAAircraft(ACIndex).libGear = "E" _
                          Or LEAAircraft(ACIndex).libGear = "H" _
                          Or LEAAircraft(ACIndex).libGear = "WFBF" _
                          Or LEAAircraft(ACIndex).libGear = "WFBN" _
                          Or LEAAircraft(ACIndex).ACname = "IL86" _
                          Or (LEAAircraft(ACIndex).libGear = "HB" And
                           LEAAircraft(ACIndex).NTires = 4) Then
                        NCat(2) += 1S
                        GroupIndex(ACIndex) = 2
                        If firstACCat(2) = 0 Then
                            firstACCat(2) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).libGear = "N" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" Then
                        NCat(3) += 1S
                        GroupIndex(ACIndex) = 3
                        If firstACCat(3) = 0 Then
                            firstACCat(3) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).libGear = "M" _
                        Or LEAAircraft(ACIndex).libGear = "K" _
                        Or LEAAircraft(ACIndex).ACname = "IL76T" _
                        Or LEAAircraft(ACIndex).ACname = "00000" Then
                        NCat(4) += 1S
                        GroupIndex(ACIndex) = 4
                        If firstACCat(4) = 0 Then
                            firstACCat(4) = ACIndex
                        End If
                    ElseIf LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).ACname = "+++++" _
                        Or LEAAircraft(ACIndex).ACname = "An-124" _
                        Or LEAAircraft(ACIndex).ACname = "An-225" Then
                        NCat(5) += 1S
                        GroupIndex(ACIndex) = 5
                        If firstACCat(5) = 0 Then
                            firstACCat(5) = ACIndex
                        End If
                    Else 'undetermined aircrafts

                    End If
                End If
            Next ACIndex


            'ikawa 10/18/2011
            For iCat = 1 To gMeshCategories
                If NCat(iCat) > 1 Then
                    NCat(iCat) = 1
                End If
            Next



            For ACIndex = 1 To CShort(NACarg)
                iCat = GroupIndex(ACIndex)






                'For iCat = 1 To gMeshCategories 'four mesh categories ***
                'If NCat(iCat) > 0 Then 'are there any aircraft in this category
                If iCat = 1 Then
                    'default 0
                    MeshCase = "1DSYM"
                    Call GetFEMStress1AC(LEAAircraft, LEAStructure, ACIndex, "Edge", 0)
                    If gUserInterrupted Then Exit Sub
                ElseIf iCat = 2 Then
                    MeshCase = "2DSYM"
                    'default 90
                    Call GetFEMStress1AC(LEAAircraft, LEAStructure, ACIndex, "Edge", 90)
                    If gUserInterrupted Then Exit Sub
                ElseIf iCat = 3 Then
                    'default 90 B777
                    MeshCase = "3DSYM"
                    Call GetFEMStress1AC(LEAAircraft, LEAStructure, ACIndex, "Edge", 90)
                    If gUserInterrupted Then Exit Sub
                ElseIf iCat = 4 Then
                    'no default
                    MeshCase = "4DNSY"
                    Call GetFEMStress1AC(LEAAircraft, LEAStructure, ACIndex, "Edge", 0)
                    If gUserInterrupted Then Exit Sub
                ElseIf iCat = 5 Then
                    MeshCase = "5DSYM"
                    Call GetFEMStress1AC(LEAAircraft, LEAStructure, ACIndex, "Edge", 90)
                    If gUserInterrupted Then Exit Sub
                End If


                Dim kk, ii As Integer
                kk = 0
                'ikawa 10/18/2011 For ii = 1 To gNACarg

                ii = ACIndex 'ikawa 10/18/2011
                If GroupIndex(ii) = iCat Then
                    If CalcStress(ii) = 0 Then
                        kk += 1
                        If iCat = 1 Then
                            EdgeStressFEM0PCC(ii) = modWorld.Stress1(kk)
                            EdgeStressFEM0Overlay(ii) = modWorld.Stress8(kk)
                        ElseIf iCat = 2 Or iCat = 3 Or iCat = 5 Then
                            EdgeStressFEM90PCC(ii) = modWorld.Stress1(kk)
                            EdgeStressFEM90Overlay(ii) = modWorld.Stress8(kk)
                        ElseIf iCat = 4 Then
                            EdgeStressFEM0PCC(ii) = modWorld.Stress1(kk)
                            EdgeStressFEM0Overlay(ii) = modWorld.Stress8(kk)
                        End If
                    End If
                End If
                'ikawa 10/18/2011 Next ii

                'End If 'are there any aircraft in this category
                'Next iCat 'four mesh categories ***

            Next ACIndex











            If Overlay Then
                For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                    If GroupIndex(ACIndex) = 1 Then
                        Response(ACIndex, 1) = EdgeStressFEM0Overlay(ACIndex)
                        Response(ACIndex, 2) = EdgeStressFEM0PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 2 Or GroupIndex(ACIndex) = 3 Or GroupIndex(ACIndex) = 5 Then
                        Response(ACIndex, 1) = EdgeStressFEM90Overlay(ACIndex)
                        Response(ACIndex, 2) = EdgeStressFEM90PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 4 Then
                        Response(ACIndex, 1) = EdgeStressFEM0Overlay(ACIndex)
                        Response(ACIndex, 2) = EdgeStressFEM0PCC(ACIndex)
                    End If
                Next ACIndex
            Else
                For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                    If GroupIndex(ACIndex) = 1 Then
                        Response(ACIndex, 1) = EdgeStressFEM0PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 2 Or GroupIndex(ACIndex) = 3 Or GroupIndex(ACIndex) = 5 Then
                        Response(ACIndex, 1) = EdgeStressFEM90PCC(ACIndex)
                    ElseIf GroupIndex(ACIndex) = 4 Then
                        Response(ACIndex, 1) = EdgeStressFEM0PCC(ACIndex)
                    End If
                Next ACIndex
            End If


            'ikawa 
            For ACIndex = 1 To CShort(NACarg) 'for all aircraft
                If InStr(LEAAircraft(ACIndex).ACname, "747", CompareMethod.Text) > 0 _
                   And InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) > 0 Then
                    Dim ACIndex2 As Integer, pos1, pos2 As Integer
                    pos1 = InStr(LEAAircraft(ACIndex).ACname, "Wing", CompareMethod.Text) - 2
                    For ACIndex2 = 1 To CShort(NACarg) 'for all aircraft
                        pos2 = InStr(LEAAircraft(ACIndex2).ACname, "Body", CompareMethod.Text) - 2
                        If pos2 > 0 Then
                            If (Not ACIndex = ACIndex2) _
                               And Mid(LEAAircraft(ACIndex).ACname, 1, pos1) = Mid(LEAAircraft(ACIndex2).ACname, 1, pos2) _
                               And LEAAircraft(ACIndex).GearLoad = LEAAircraft(ACIndex2).GearLoad Then
                                Response(ACIndex2, 1) = Response(ACIndex, 1)
                                If Overlay Then
                                    Response(ACIndex2, 2) = Response(ACIndex, 2)
                                End If
                            End If
                        End If
                    Next ACIndex2
                End If
            Next ACIndex



            If ThinLayer Then
                ThinLayer = False

                If DesignType2 = 11 _
                Or DesignType2 = 12 _
                Or DesignType2 = 13 Then

                    'return to previous values
                    LEAStructure.Thick(3) = LEAStructure.Thick(4)
                    LEAStructure.InterfaceParm(3) = LEAStructure.InterfaceParm(4)
                    LEAStructure.NLayers = 4

                    ReDim Preserve LEAStructure.Modulus(3)
                    ReDim Preserve LEAStructure.Poisson(3)
                    ReDim Preserve LEAStructure.Thick(3)
                    ReDim Preserve LEAStructure.InterfaceParm(3)

                End If

            End If




            tdiff = DateDiff(DateInterval.Second, t1, Now(), FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)

            If PrintResults Then
                FileOpen(3, "Time" & JobName & ".txt", OpenMode.Append, , , 1024)
                For ACIndex = 1 To CShort(NACarg)
                    Dim pr As Integer
                    pr = 1
                    If Overlay Then
                        pr = 2
                        PrintLine(3, LPad(3, CStr(ACIndex)) _
                      & LPad(19, LEAAircraft(ACIndex).ACname) _
                      & LPad(10, VB6.Format(EdgeStressFEM0Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(EdgeStressFEM90Overlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressFEMOverlay(ACIndex), "#,###,###.00")) _
                      & LPad(10, VB6.Format(Response(ACIndex, 1), "#,###,###.00")) _
                      & LPad(10, VB6.Format(InteriorStressLEAFOverlay, "#,###,###.00")) _
                      & LPad(5, "Over") _
                      & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                      & LPad(10, VB6.Format(LEAStructure.Modulus(1), "#,###,###.00")) _
                      & LPad(23, CStr(Now())))
                    End If

                    PrintLine(3, LPad(3, CStr(ACIndex)) _
                  & LPad(19, LEAAircraft(ACIndex).ACname) _
                  & LPad(10, VB6.Format(EdgeStressFEM0PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(EdgeStressFEM90PCC(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressFEM(ACIndex), "#,###,###.00")) _
                  & LPad(10, VB6.Format(Response(ACIndex, pr), "#,###,###.00")) _
                  & LPad(10, VB6.Format(InteriorStressLEAF, "#,###,###.00")) _
                  & LPad(5, "Slab") _
                  & LPad(12, VB6.Format(tdiff / 60, "##,##0.00")) & " min." _
                  & LPad(10, VB6.Format(LEAStructure.Modulus(pr), "#,###,###.00")) _
                  & LPad(23, CStr(Now())))
                Next ACIndex
                FileClose(3)
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
    Private Sub GetFEMStress1AC(ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms,
             ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms,
             ByVal ACIndex As Short, ByVal StressType As String, ByVal GA As Short)

        'subroutine for 1 aircraft in 1 step, automated by YGC 031312

        'Microsoft Scripting Runtime checked in Project References
        Dim fso As New Scripting.FileSystemObject(), fil As Boolean
        Dim ExtFilePath As String

        Dim Sorting As Boolean                                            'for sorting
        Dim WheelLoad(NCat(iCat), 2), WheelLoad2(NCat(iCat), 2) As Double 'for sorting
        Dim PreviousIndex As Integer, Diff As Double                      'for scaling 
        Dim DiffNum As Double = 0.15                                        'for scaling 

        Dim index1() As Integer
        'Dim ScaleFactorA() As Double

        ' ************ Scaling *************
        'Dim Scaled(80) As Boolean
        'Public Const ScFactor As Double = 0.25
        Dim ScalingImplemented As Boolean
        ' ************ Scaling *************

        Dim SInfo() As ScalingInfo
        ReDim SInfo(NCat(iCat))

        ReDim index1(NCat(iCat))
        'ReDim Scaled(NCat(iCat))
        'ReDim ScaleFactorA(NCat(iCat))


        'Stress1 = -40 * LEAStructure.Thick(1) + 850 + LEAAircraft(ACIndex).GearLoad * 0.0002
        'Stress1 = 20
        'Exit Sub

        'GoTo gotoIngrid
        ScalingImplemented = True
        Sorting = True

        If NCat(iCat) = 1 Then 'NCat() - number of aircraft in particular category
            Sorting = False
            ScalingImplemented = False
        End If

        ChDir(VB6.GetPath)
        ExtFilePath = VB6.GetPath & "\"

        If StressType = "Edge" Then
            EdgeLoad = True
        Else
            EdgeLoad = False
        End If
        GearAngle = GA      '90-perpendicular to edge ' 0-parallel to edge


        Call frmStructure.frmStructure_Load(LEAStructure)



        If Sorting Then
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   WITH SORTING   *****************************
            Dim i1, i2 As Integer

            i2 = 0
            For i1 = 1 To gNACarg
                If GroupIndex(i1) = iCat And gCalcStress(i1) = 0 Then
                    i2 += 1
                    index1(i2) = i1
                End If
            Next


            For i1 = 1 To NCat(iCat)
                WheelLoad(i1, 1) = CDbl(LEAAircraft(index1(i1)).GearLoad /
                                        LEAAircraft(index1(i1)).NTires)
                WheelLoad(i1, 2) = i1
            Next

            Call PIKSTR2(NCat(iCat), WheelLoad, WheelLoad2)
            'WheelLoad2 - indexes of aircraft sorted array according to wheel load - descending


            'ACIndex = CShort(WheelLoad2(1, 2)) 'index for first aircraft in the group
            ACIndex = CShort(index1(CInt(WheelLoad2(1, 2))))
            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                                          ' QW 09-15-2019
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '


            Dim icc As Short
            For icc = 2 To NCat(iCat)
                ACIndex = CShort(index1(CInt(WheelLoad2(icc, 2))))
                'ACIndex = CShort(WheelLoad2(icc, 2))



                ' ****************** scaling begin ******************
                If ScalingImplemented Then
                    If (LEAAircraft(ACIndex).GearLoad >
                        LEAAircraft(PreviousIndex).GearLoad) Then
                        Diff = CDbl((LEAAircraft(ACIndex).GearLoad -
                                     LEAAircraft(PreviousIndex).GearLoad) /
                                     LEAAircraft(PreviousIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 1.15
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 1.5
                            'ScaleFactorA(icc) = 2
                        End If
                    Else
                        Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                     LEAAircraft(ACIndex).GearLoad) /
                                     LEAAircraft(ACIndex).GearLoad)
                        SInfo(icc).ScaleFactorA = 0.75
                        If gDesignType = 13 Then
                            SInfo(icc).ScaleFactorA = 0.5
                        End If
                    End If


                    If Diff < DiffNum Then
                        SInfo(icc).Scaled = True
                        Dim TireNumb As Integer
                        For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                            LEAAircraft(ACIndex).TirePress(TireNumb) =
                            LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                        Next
                        LEAAircraft(ACIndex).GearLoad =
                        LEAAircraft(ACIndex).GearLoad * SInfo(icc).ScaleFactorA

                        SInfo(icc).index2 = ACIndex
                    End If
                    'PreviousIndex = icc
                    PreviousIndex = ACIndex
                End If
                ' ****************** scaling end ******************


                Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                Call GearLoads(ACIndex, LEAAircraft)
                'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                Call WriteTextFile(OpenMode.Append, rgdFile)
            Next icc
            '**************   WITH SORTING   *****************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        Else

            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
            '**************   without sorting   *****************************
            'ikawa 10/18/2011  ACIndex = firstACCat(iCat) 'index for first aircraft in the group
            PreviousIndex = ACIndex '*** for scaling
            Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
            Call AMMain(ExtFilePath & gFileName, Interior, LEAStructure, ACIndex, LEAAircraft)

            System.Windows.Forms.Application.DoEvents()
            'Do                                          ' QW 09-15-2019
            'System.Windows.Forms.Application.DoEvents()
            'fil = fso.FileExists(VB6.GetPath & "\" & "nikein.rgd")
            'fil = fso.FileExists(rgdFile)
            'Loop While Not fil '



            GoTo gotoIngrid              'ikawa 10/18/2011

            Dim icc As Short
            For icc = (firstACCat(iCat) + 1S) To CShort(gNACarg)
                If GroupIndex(icc) = iCat Then
                    ACIndex = icc

                    ' ****************** scaling begin ******************
                    If ScalingImplemented Then
                        If (LEAAircraft(icc).GearLoad >
                            LEAAircraft(PreviousIndex).GearLoad) Then
                            Diff = CDbl((LEAAircraft(icc).GearLoad -
                                         LEAAircraft(PreviousIndex).GearLoad) /
                                         LEAAircraft(PreviousIndex).GearLoad)
                        Else
                            Diff = CDbl((LEAAircraft(PreviousIndex).GearLoad -
                                         LEAAircraft(icc).GearLoad) /
                                         LEAAircraft(icc).GearLoad)
                        End If


                        If Diff < DiffNum Then
                            SInfo(icc).Scaled = True
                            Dim TireNumb As Integer
                            For TireNumb = 1 To LEAAircraft(ACIndex).NTires
                                LEAAircraft(ACIndex).TirePress(TireNumb) =
                                LEAAircraft(ACIndex).TirePress(TireNumb) * SInfo(icc).ScaleFactorA
                            Next
                            LEAAircraft(icc).GearLoad =
                            LEAAircraft(icc).GearLoad * SInfo(icc).ScaleFactorA
                        End If
                        PreviousIndex = icc
                    End If
                    ' ****************** scaling end ******************

                    Call frmGear.fromGear_Load(CShort(ACIndex), LEAAircraft)
                    Call GearLoads(ACIndex, LEAAircraft)

                    'Call WriteTextFile(OpenMode.Append, "nikein.rgd")
                    Call WriteTextFile(OpenMode.Append, rgdFile)
                End If
            Next icc
            '*******************   without sorting   ************************
            'VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV

        End If


gotoIngrid:
        'GoTo gotoNike3d

        'Call INGRIDMAIN()

        'Call Ingrid()
        System.Windows.Forms.Application.DoEvents()




        '------------------------------------------------------------------------------
        Dim StopFEDFAA As Short, FEDFAAStopped As Short
        Static AtomName As New VB6.FixedLengthString(10)
        Dim II As Integer, i As Integer
        Dim ACDATPath As String, DesigningStr As Boolean

        ACDATPath = Environment.CurrentDirectory
        DesigningStr = False
        AtomName.Value = "StopFEDFAA"

        Do
            II = GlobalFindAtom(AtomName.Value)
            If II <> 0 Then i = GlobalDeleteAtom(CShort(II))
        Loop Until II = 0
        StopFEDFAA = 0 : FEDFAAStopped = 0

        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped)
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, MeshCase, CShort(gDesignType))
        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), MeshCase)

        If MeshCase = "1DSYM" Then
            iSymCase = 1S
        ElseIf MeshCase = "2DSYM" Then
            iSymCase = 2S
        ElseIf MeshCase = "3DSYM" Then
            iSymCase = 3S
        ElseIf MeshCase = "4DNSY" Then
            iSymCase = 4S
        ElseIf MeshCase = "5DSYM" Then
            iSymCase = 5S
        End If

        Dim tStart, tEnd As Date, tdiff As Long
        Dim d1, d2 As Double, St As String
        Dim timeSave1, timeSave2 As Integer, timeDiff As Double
        tStart = Now()
        d1 = TimeOfDay.ToOADate
        timeSave1 = timeGetTime

        'Call NIKE3D(modWorld.Stress1(1), modWorld.Stress8(1), StopFEDFAA, FEDFAAStopped, CShort(gDesignType), iSymCase)

        timeSave2 = timeGetTime
        timeDiff = CDbl((timeSave2 - timeSave1) / 1000)

        d2 = TimeOfDay.ToOADate

        St = VB6.Format(System.DateTime.FromOADate(d2 - d1), "hh:mm:ss")
        tEnd = Now()

        tdiff = DateDiff(DateInterval.Second, tStart, tEnd, FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1)

        StopFEDFAA = -1

        'II = GlobalFindAtom(AtomName.Value)

        System.Windows.Forms.Application.DoEvents()
        System.Diagnostics.Debug.WriteLine("StopFEDFAA = " & StopFEDFAA)
        Do
            System.Windows.Forms.Application.DoEvents()
            If FEDFAAStopped = -1 Then
                System.Diagnostics.Debug.WriteLine("FEDFAAStopped = " & FEDFAAStopped)
                Exit Do
            End If
        Loop

        Do
            ' The Dir$ function is very time consuming, therefore use
            ' a For Loop for DoEvents.
            ' 1,000 times through the loop is the best compromise for long
            ' Julea executions and short Julea executions. Marking the end of
            ' a Julea computation with an atom does not reduce total time much.
            For i = 1 To 1000
                System.Windows.Forms.Application.DoEvents()
                If DesigningStr = False Then ' Stop Julea
                    II = GlobalAddAtom(AtomName.Value)
                    System.Diagnostics.Debug.WriteLine("AddAtom = " & II)
                    '        StartTime = Timer
                    Do
                        System.Windows.Forms.Application.DoEvents() ' Let Julea delete the atom.
                        '          ETime = Abs(Timer - StartTime)
                        '          Debug.Print ETime
                        II = GlobalFindAtom(AtomName.Value)
                        System.Diagnostics.Debug.WriteLine("FindAtom = " & II)
                        '         Need to check file in case atom added after JULEA stopped
                        '         normally but file writes not finished.
                        'ikawa If II = 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then

                        If II <> 0 Or Dir(ACDATPath & "JULEA.ERR") <> "" Then
                            System.Diagnostics.Debug.WriteLine("Check before delete " _
                                   & GlobalFindAtom(AtomName.Value))
                            If II <> 0 Then II = GlobalDeleteAtom(CShort(II))
                            System.Diagnostics.Debug.Write("DeleteAtom = " & II)
                            System.Diagnostics.Debug.WriteLine(GlobalFindAtom(AtomName.Value))
                            If Dir(ACDATPath & "JULEA.ERR") = "" Then
                                Exit Do ' Make sure all file activity is finished
                            End If ' before closing Julea below.
                        End If
                    Loop
                    Exit For
                End If
            Next i
            If Dir(ACDATPath & "JULEA.ERR") <> "" Or DesigningStr = False Then
                '      IL = SendMessage(hJULEA, WM_CLOSE, 0&, 0&)
                Exit Do
            End If
        Loop

        'If StopFEDFAA = -1 Then
        '    gUserInterrupted = True
        'End If

        If gUserInterrupted Then Exit Sub 'September 13, 2006
gotoNike3d:


        'For i = 1 To NCat(iCat)
        '    If StressType = "Edge" Then
        '        modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
        '        modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
        '    End If
        'Next i


        For i = 1 To NCat(iCat)
            If StressType = "Edge" Then
                If gDesignType2 = 13 Then 'HMA Overlay on Rigid - FlexOnRigid
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.5
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.5
                Else
                    modWorld.Stress1(i) = modWorld.Stress1(i) * 0.75
                    modWorld.Stress8(i) = modWorld.Stress8(i) * 0.75
                End If
            End If
        Next i


        '********************* Scaling Part Begin *********************
        If ScalingImplemented Then
            For i = 1 To NCat(iCat) 'for all aircraft
                If SInfo(i).Scaled Then
                    LEAAircraft(SInfo(i).index2).GearLoad = LEAAircraft(SInfo(i).index2).GearLoad / SInfo(i).ScaleFactorA
                    modWorld.Stress1(i) = modWorld.Stress1(i) / SInfo(i).ScaleFactorA
                    modWorld.Stress8(i) = modWorld.Stress8(i) / SInfo(i).ScaleFactorA
                End If
            Next
        End If

        '********************* Scaling Part End *********************
        If Sorting Then
            Dim S11(NCat(iCat)), S88(NCat(iCat)) As Double

            For i = 1 To NCat(iCat)
                S11(i) = modWorld.Stress1(i)
                S88(i) = modWorld.Stress8(i)
            Next

            For i = 1 To NCat(iCat)
                'modWorld.Stress1(index1(CInt(WheelLoad2(i, 2)))) = S11(i)
                'modWorld.Stress8(index1(CInt(WheelLoad2(i, 2)))) = S88(i)
                modWorld.Stress1(CInt(WheelLoad2(i, 2))) = S11(i)
                modWorld.Stress8(CInt(WheelLoad2(i, 2))) = S88(i)
            Next

        End If

        ACIndex = ACIndex

    End Sub


    Private Sub GetInteriorStress(ByRef InteriorStressLEAF As Double, _
    ByRef InteriorStressLEAFOverlay As Double, _
    ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms, _
    ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms, _
    ByVal ACIndex As Short)

        Dim OneAC(1) As LEAFClassLib.clsLEAF.LEAFACParms
        Dim AllResp(,) As LEAFClassLib.clsLEAF.LEAFAllResponses
        Dim RunLEAF As LEAFClassLib.clsLEAF
        Dim I As Integer, Response(,) As Double
        Dim Lower1, Lower2, Upper1, Upper2, I1, I2 As Integer


        Try

            RunLEAF = New LEAFClassLib.clsLEAF()

            OneAC(1).ACname = LEAAircraft(ACIndex).ACname
            OneAC(1).GearLoad = LEAAircraft(ACIndex).GearLoad
            OneAC(1).NTires = LEAAircraft(ACIndex).NTires
            OneAC(1).TirePress = LEAAircraft(ACIndex).TirePress
            OneAC(1).NEvalPoints = LEAAircraft(ACIndex).NEvalPoints

            ReDim OneAC(1).TireX(OneAC(1).NTires)
            ReDim OneAC(1).TireY(OneAC(1).NTires)

            For I = 1 To CShort(OneAC(1).NTires)
                OneAC(1).TireX(I) = LEAAircraft(ACIndex).TireX(I)
                OneAC(1).TireY(I) = LEAAircraft(ACIndex).TireY(I)
            Next I

            ReDim OneAC(1).EvalX(OneAC(1).NEvalPoints)
            ReDim OneAC(1).EvalY(OneAC(1).NEvalPoints)

            For I = 1 To CShort(OneAC(1).NEvalPoints)
                OneAC(1).EvalX(I) = LEAAircraft(ACIndex).EvalX(I)
                OneAC(1).EvalY(I) = LEAAircraft(ACIndex).EvalY(I)
            Next I

            LEAStructure.EvalLayer = 1
            LEAStructure.EvalDepth = LEAStructure.Thick(1)

            Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, _
            1, OneAC, LEAStructure, Response, AllResp)


            Lower1 = LBound(Response, 1) : Lower2 = LBound(Response, 2)
            Upper1 = UBound(Response, 1) : Upper2 = UBound(Response, 2)

            'new rigid pavement 
            For I1 = Lower1 To Upper1
                InteriorStressLEAF = CDbl(Response(I1, 1))
                For I2 = Lower2 To Upper2
                    If Math.Abs(InteriorStressLEAF) < Math.Abs(Response(I1, I2)) Then
                        InteriorStressLEAF = CDbl(Response(I1, I2))
                    End If
                Next I2
            Next I1

            'rigid overlay
            '??????

            RunLEAF = Nothing

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub

   
    Private Sub Readstressmax(ByVal SlabULCriticalStress() As Double, ByVal SlabOLCriticalStress() As Double, ByVal ZFoundationStress() As Double, ByVal FoundationStress(,) As Double)

        Try

            Dim FNstressmax As String = WorkingDir0 & "stressmax.dat"
            FileOpen(99, FNstressmax, OpenMode.Input)

            Dim SlabULStress(gAC, 4), SlabOLStress(gAC, 4) As Double

            Dim IAC, k As Integer
            Dim StrLine, NumberStr As String

            For IAC = 1 To gNACarg
                StrLine = LineInput(99) 'read " Step =            1"
                StrLine = LineInput(99) 'read " Maximum Horizontal Tensile Stress on the Concrete Slab"
                StrLine = LineInput(99) 'read "  location direction node              x              y              z         stress"


                'Initialized to fixed the bug in reading max stress by YGC 032614   
                SlabULCriticalStress(IAC) = 0.0
                SlabOLCriticalStress(IAC) = 0.0
                'Initialized to fixed the bug in reading max stress by YGC 032614 END  

                ' read underlay slab stress
                For k = 1 To 4
                    StrLine = LineInput(99)
                    NumberStr = Right(StrLine, 12)
                    SlabULStress(IAC, k) = CDbl(NumberStr)

                    If SlabULCriticalStress(IAC) < SlabULStress(IAC, k) Then
                        SlabULCriticalStress(IAC) = SlabULStress(IAC, k)
                    End If
                Next k

                StrLine = LineInput(99)
                If StrLine = " Overlay" Then ' read overlay stress
                    For k = 1 To 4
                        StrLine = LineInput(99)
                        NumberStr = Right(StrLine, 12)
                        SlabOLStress(IAC, k) = CDbl(NumberStr)

                        If SlabOLCriticalStress(IAC) < SlabOLStress(IAC, k) Then
                            SlabOLCriticalStress(IAC) = SlabOLStress(IAC, k)
                        End If
                    Next k

                    StrLine = LineInput(99)
                End If


                ' read foudnation stress
                StrLine = LineInput(99) ' read " Maximum Vertical Compressive Stress on the Foundation"
                StrLine = LineInput(99) ' read "    layer direction node              x              y              z         stress"

                Dim NumberOfFoundationLayers As Integer 'define number of foundation layers

                NumberOfFoundationLayers = NumberOfLayers - 1 'alreay exclude overlay layer in Sub Layer_BaseEdgNew


                gNumberOfFoundationInterfaces = (NumberOfFoundationLayers - 1 + 2) + 1 ' -1 to exclude SG layer (consider only BS+SB), +2 to include 2 sublayers for finite SG mesh, +1 for one extra inferface   

                For k = 1 To gNumberOfFoundationInterfaces
                    StrLine = LineInput(99)
                    NumberStr = Mid(StrLine, 58, 12)
                    ZFoundationStress(k) = CDbl(NumberStr)
                    NumberStr = Right(StrLine, 12)
                    FoundationStress(IAC, k) = CDbl(NumberStr)
                Next k
                StrLine = LineInput(99)


            Next IAC

            FileClose(99)

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub


    Public Sub MakeCopy(ByRef Orig1() As LEAFClassLib.clsLEAF.LEAFACParms, _
                        ByRef Copy1() As LEAFClassLib.clsLEAF.LEAFACParms, ByVal k1 As Integer)

        Dim i1, n1 As Integer
        For i1 = 1 To k1
            Copy1(i1).ACname = Orig1(i1).ACname
            Copy1(i1).GearLoad = Orig1(i1).GearLoad
            Copy1(i1).NTires = Orig1(i1).NTires

            ReDim Copy1(i1).TirePress(Orig1(i1).NTires)
            ReDim Copy1(i1).TireX(Orig1(i1).NTires)
            ReDim Copy1(i1).TireY(Orig1(i1).NTires)

            For n1 = 1 To Orig1(i1).NTires
                Copy1(i1).TirePress(n1) = Orig1(i1).TirePress(n1)
                Copy1(i1).TireX(n1) = Orig1(i1).TireX(n1)
                Copy1(i1).TireY(n1) = Orig1(i1).TireY(n1)
            Next

            Copy1(i1).NEvalPoints = Orig1(i1).NEvalPoints
            ReDim Copy1(i1).EvalX(Orig1(i1).NEvalPoints)
            ReDim Copy1(i1).EvalY(Orig1(i1).NEvalPoints)


            For n1 = 1 To Orig1(i1).NEvalPoints
                Copy1(i1).EvalX(n1) = Orig1(i1).EvalX(n1)
                Copy1(i1).EvalY(n1) = Orig1(i1).EvalY(n1)
            Next

            Copy1(i1).libGear = Orig1(i1).libGear
        Next



    End Sub



    Dim TirePress() As Double ' 1 To NTires
    Dim TireX() As Double ' 1 To NTires
    Dim TireY() As Double ' 1 To NTires

    Dim EvalX() As Double ' 1 To NEvalPoints
    Dim EvalY() As Double ' 1 To NEvalPoints


End Class


