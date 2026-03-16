'Option Strict On
Option Explicit On

Public Module CDF

    ' Computes cumulative damage factors (CDFs) 
    ' for flexible, rigid, and overlay pavements.
    ' The Gauss probability integral subroutine has been changed (from
    ' the version in LEDNEW). More efficient in most cases, always succeeds,
    ' and added error checking. Returns correct values for SIGMA = 0.
    Public gFirstIter As Boolean = True
    Public gParamA, gParamB, gParamC, gParamD, gFSlope As Single '
    Public multiCalculator As Integer = 0

    Public gTandemFnew As Boolean
    Public gHMA_Strain
    Public publicNtoFail As Double
    Public gNtoFail(MaxSectAC) As Double
    Public gNtoFail_copy(MaxSectAC) As Double
    Public Const NNodesLong As Short = 1800 '  kairat replace tandem  1234567
    Public gSTRAIN(MaxSectAC) As Double 'ikawa

    Public jobCDFtableRigid(MaxSects, MaxSectAC) As Double
    Public jobCDFacrftMaxtableRigid(MaxSects, MaxSectAC) As Double 'ikawa 2017.05.01 
    Public jobCtoPtableRigid(MaxSects, MaxSectAC) As Double

    Public gUseCompaction As Boolean 'Module CDF
    Public gCompactionDeparture As Single = 6000 'ikawa 2013
    Public gPrintPCConRigidInfo As Boolean
    Public gHMAonRigid_Mod As Boolean
    Public gSolverType As Integer 'added solver choice (EBE vs Direct) by YGC 083012
    Public gSlabMeshSize As Integer 'YGC 061113
    Public gNewModulus_P154 As Boolean
    Public gNewModulus_P209 As Boolean
    Public gP154_C688D156 As Boolean
    Public gP209_C1052D20 As Boolean
    Public gBleasdaleModel As Boolean
    Public g2Dgear As Integer
    Public g3inchPCC As Boolean
    Public g2inchHMA As Boolean
    Public gRigidNewParam As Boolean
    Public gFSlopeNewValue As Boolean
    Public gRigidCalibrationNewValue As Boolean = False 'ikawa 2013
    Public TSS_P154(99) As Single
    Public TSS_P209(99) As Single
    Public NS_P209, NS_P154 As Single 'ikawa
    Public publicNS_P154, publicNS_P209 As Single
    Public Const NOFF As Short = 41 ' Number of offsets for CDF calculations.
    Public Const OFFSETINC As Single = 10.0! ' Distance between offsets in inches.
    Dim CDFFlexVal(NOFF) As Single ' Sum over all aircraft for each offset.

    ' > GFH 08/13/03
    Public ComputeAircraftCDF, AircraftLifeCDFvsDesignLife As Boolean
    Public jobCDFtable(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFacrftMaxtable(MaxSects, MaxSectAC) As Double ' Same, except max for one aircraft, GFH.
    Public jobCtoPtable(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFtableAC(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFacrftMaxtableAC(MaxSects, MaxSectAC) As Double ' Same, except max for one aircraft, GFH.
    Public jobCtoPtableAC(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFtableHMA(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFacrftMaxtableHMA(MaxSects, MaxSectAC) As Double ' Same, except max for one aircraft, GFH.
    Public jobCtoPtableHMA(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFtable401(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFacrftMaxtable401(MaxSects, MaxSectAC) As Double ' Same, except max for one aircraft, GFH.
    Public jobCtoPtable401(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFtableSub(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
    Public jobCDFacrftMaxtableSub(MaxSects, MaxSectAC) As Double ' Same, except max for one aircraft, GFH.
    Public jobCtoPtableSub(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.

    Public gUseLEAF_preDesign_forRigid As Boolean = True
    Public gPrintCoverages As Boolean = False
    Public publicNtoFRail As Double
    Public publicSTRAIN As Double

    Public AA1(MaxSectAC, NOFF), AA2(MaxSectAC, NOFF) As Single
    Public gCDF_Rigid As Single
    Public gOnly_LEAF As Boolean

    Public MinimumThickness As Boolean = False

    ' GFH 08/13/03 <

    Public PercentCdfu As Single
    Dim OFFSET As Single ' Numerical value of current offset, inches.

    Public CtoP(MaxSectAC, NOFF) As Single ' GFH 02-24-03 was Variant ' C/P for each aircraft and each offset.

    Public CDFdata(1, MaxSectAC, NOFF) As Single
    Public CDFdata2(1, MaxSectAC, NOFF) As Single
    Public CDFdata3(1, MaxSectAC, NOFF) As Single

    Public BaseMod(MaxNPLayers) As Single
    Public SubbaseMod(MaxNPLayers) As Single
    Public gStrain2(MaxSectAC) As Double
    Public gStrain2C(MaxSectAC) As Double
    Public gStrain2_copy As Double

    Public CDFErr As Single
    Public CDFExitErr As Single = 0.005! ' Exit design loop.
    ' Exit design when .995 < CDF < 1.005
    Public Const CDFErrCntrl As Single = 0.69! ' Switch to multiple layers.

    ' Make the switch when .5 < CDF < 2.     (Ln(2) = .693)

    Public LayerSwitch As Boolean ' See WESModulus.
    Public IControl As Short ' Identifier of point having max. CDF
    ' value. Used in frmOverlay.
    Public IterLayerChosen As Short

    Public PcntCDFU As Single

    Public AlternateSG As Boolean ' See frmStructure.Load.

    Public BatchMode, NoACCDF, NoOutFiles, EnglishUnits As Boolean

    Public StraightLineModel As Short
    Public FSlope, FSlopeComp As Single


    'GUO1210 LogCA2 is used in Overlay Design,  = AA/BB in damage model
    Public LogCA2(MaxSectAC) As Double
    Public MultipleSubgradeLayers As Boolean
    Public RunTimeSelected As Integer

    Public g1STRAIN(MaxSectAC) As Double

    'FLEXIBLE
    Sub LeafCDFFlex(ByRef CDFMAX As Single, ByRef Depth As Double, ByRef Overflow As Boolean, ByRef StrainResponse(,) As Double)


        ' Compute CDFs for all lateral offsets on flexible pavement.
        Dim IA As Short ' Aircraft index.
        Dim IOFF As Short
        Dim CovToPass As Single
        Dim ACCDF, ACCDFmax As Single
        Dim Asphalt, SubGrade As Boolean
        Dim ASPMOD, SubMod As Single
        Dim NtoFail As Double
        Dim AA, BB As Double
        Dim FileErr As Boolean
        Dim NGearLoads As Integer ' Number 0f individual gear loads to be applied
        Dim IGearLoads As Integer ' to the pavement per aircraft.
        Dim StrainMax, StrainMax2 As Double
        Dim NGridby2, I As Integer
        Dim GearLoadType As String ' Military letter designations in libGear$()
        ' modified to allow multiple wheels in A380, etc.
        Dim GearACType As Short ' Aircraft types are the constants below.
        Const SingleWheelLoad As Short = 1
        Const DualWheelMainGear As Short = 2
        Const DualTandemMainGear As Short = 3 ' Includes six-wheels.
        Const FourGearsinMain As Short = 4 ' Includes mixed four- and six-wheel gears.
        Dim BBorig, AAorig, StrainBreak As Double
        Dim lclCDF(MaxSectAC, NOFF) As Double
        Dim ExtrType As Short ' kairat replace tandem
        Dim Damage As Double ' kairat replace tandem
        Dim IEval, IEvaly As Short ' kairat replace tandem
        ReDim gStrain2(80) : ReDim gNtoFail(80)


        Try

            '  STRNH(IA) = Abs(STRNH(IA))  ' See LEDNEW sub RDAMTMP
            '  STRNV(IA) = Abs(STRNV(IA)) '/ 0.885 ' in file CDF.FOR.
            If Overflow = True Then ' Subgrade CDF required.
                SubGrade = True : Asphalt = False ': STRNH(IA) = 0!
            Else ' Asphalt CDF required.
                Asphalt = True : SubGrade = False ': STRNV(IA) = 0.0002
            End If

            ASPMOD = Modulus(1)
            SubMod = Modulus(NPLayers)
            '  SubMod = 15000    ' Remove variation with subgrade modulus.
            ' For variation of Poisson's ratio with subgrade modulus
            ' see Sub WriteJuleaFile in FileProc.Bas.

            If AlternateSG Then
                Temp = 0.0!
                If IterLayerChosen <> NPLayers - 1 Then
                    For IA = 1 To julNPLayers - 1S
                        Temp = Temp + julThick(IA)
                        If Temp > EvalDepth(1) Then
                            SubMod = julModulus(IA)
                            Exit For
                        End If
                    Next IA
                End If
            End If

            '  Debug.Print "Criteria "; ASPMOD; SubMod; NAC

            For IOFF = 1 To NOFF
                CDFFlexVal(IOFF) = 0.0! ':  CDFSUB(IOFF) = 0!
            Next IOFF

            Overflow = True
            For IA = 1 To NAC
                LI = LibIndex(IA)

                If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
                    NGearLoads = 2
                    StrainMax = 0
                    StrainMax2 = 0
                    For I = 1 To 8
                        If System.Math.Abs(StrainResponse(IA, I)) > StrainMax Then
                            StrainMax = System.Math.Abs(StrainResponse(IA, I)) ' Wing gear interior.
                        End If
                        If System.Math.Abs(StrainResponse(IA, I + 8)) > StrainMax2 Then
                            StrainMax2 = System.Math.Abs(StrainResponse(IA, I + 8)) ' Body gear interior.
                        End If
                    Next I

                    'StrainMax = 0.0038801 'TEST

                    'NGridby2 = CInt((AC(LI).libNEVPTS - 16) / 2)
                    'For I = 17 To NGridby2
                    '    If System.Math.Abs(StrainResponse(IA, I + NGridby2)) > StrainMax Then
                    '        StrainMax = System.Math.Abs(StrainResponse(IA, I + NGridby2)) ' Wing gear grid.
                    '    End If
                    '    If System.Math.Abs(StrainResponse(IA, I)) > StrainMax2 Then
                    '        StrainMax2 = System.Math.Abs(StrainResponse(IA, I)) ' Body gear grid.
                    '    End If
                    'Next I

                    For I = 37 To 46
                        If System.Math.Abs(StrainResponse(IA, I)) > StrainMax Then
                            StrainMax = System.Math.Abs(StrainResponse(IA, I)) ' Wing gear grid.
                        End If
                    Next I

                    For I = 17 To 36
                        If System.Math.Abs(StrainResponse(IA, I)) > StrainMax2 Then
                            StrainMax2 = System.Math.Abs(StrainResponse(IA, I)) ' Body gear grid.
                        End If
                    Next I



                ElseIf AC(LI).libGear = "X" Then 'added ikawa 2013
                    AC(LI).libXAC = Nothing
                    NGearLoads = AC(LI).libNGroups
                    StrainMax = 0
                    'NGearLoads = 1
                    'AC(LI).libNEVPTS1 = 5
                    For I = 1 To AC(LI).libNEVPTS1
                        If System.Math.Abs(StrainResponse(IA, I)) > StrainMax Then
                            StrainMax = System.Math.Abs(StrainResponse(IA, I))
                        End If
                    Next I

                    StrainMax2 = 0
                    For I = AC(LI).libNEVPTS1 + 1 To AC(LI).libNEVPTS
                        If System.Math.Abs(StrainResponse(IA, I)) > StrainMax2 Then
                            StrainMax2 = System.Math.Abs(StrainResponse(IA, I))
                        End If
                    Next I



                Else ' Same gear for strain and CDF computation.
                    NGearLoads = 1
                    StrainMax = 0
                    For I = 1 To AC(LI).libNEVPTS

                        If AC(LI).libNEVPTS > CShort(UBound(StrainResponse, 2)) Then
                            MsgBox("IA=" & IA, MsgBoxStyle.OkOnly, "LI=" & LI)
                            Exit Sub
                        ElseIf IA > CShort(UBound(StrainResponse, 1)) Then
                            MsgBox("+IA=" & IA & " NAC=" & NAC & " B=" & CShort(UBound(StrainResponse, 1)),
                                   MsgBoxStyle.OkOnly, "LI=" & LI)
                            Exit Sub
                        End If

                        If System.Math.Abs(StrainResponse(IA, I)) > StrainMax Then '375 line
                            StrainMax = System.Math.Abs(StrainResponse(IA, I))
                        End If
                    Next I
                End If

                For IGearLoads = 1 To NGearLoads
                    If IGearLoads = 2 Then
                        StrainMax = StrainMax2
                    End If

                    If Asphalt And DesigningP209DrawStructure = True Then 'ikawa 2012\09\19

                        If StrainMax < 0.000001 Then StrainMax = 0.000001 ' Asphalt

                        gHMA_Strain = CSng(StrainMax)
                        gRDEC = True

                        If gRDEC Then  'RDEC Model subroutine LeafCDFFlex
                            Dim PV As Double
                            'StrainMax = 0.001

                            If gFlexuralMod(ISect) = 0 Then gFlexuralMod(ISect) = 600000
                            If gAirVoids(ISect) = 0 Then gAirVoids(ISect) = 3.5
                            If gAsphaltContentByVol(ISect) = 0 Then gAsphaltContentByVol(ISect) = 12

                            gVoidPar(ISect) = gAirVoids(ISect) / (gAirVoids(ISect) + gAsphaltContentByVol(ISect))

                            If gPNMS(ISect) = 0 Then gPNMS(ISect) = 95
                            If gPPCS(ISect) = 0 Then gPPCS(ISect) = 58
                            If gP200(ISect) = 0 Then gP200(ISect) = 4.5

                            gGradationPar(ISect) = (gPNMS(ISect) - gPPCS(ISect)) / gP200(ISect)


                            PV = 44.422 * (StrainMax ^ 5.14) * ((gFlexuralMod(ISect) * 0.0068948) ^ 2.993) *
                                 (gVoidPar(ISect) ^ 1.85) * (gGradationPar(ISect) ^ (-0.4063))

                            'NtoFail = 0.4428 * (PV ^ (-1.1102))   'StrainMax = 0.001 gives 33741.474136811965
                            NtoFail = 0.4801 * (PV ^ (-0.90074)) 'StrainMax = 0.001 gives 4387.4858677915981 
                        Else
                            AA = 2.68! - 5.0! * System.Math.Log(StrainMax) / Log10
                            BB = 2.665 * System.Math.Log(ASPMOD) / Log10
                            NtoFail = 10.0! ^ (AA - BB)  'StrainMax = 0.001 gives 3570.7151530155552
                            'Damage = (1.0 / NtoFail)
                        End If




                    Else ' Subgrade
                        If Not gTandemFnew Then ' kairat replace tandem
                            If StrainMax < 0.0001 Then StrainMax = 0.0001 Else Overflow = False
                            '       Standard subgrade criterion.
                            If Not CBool(StraightLineModel) Then
                                AA = 0.000247 + 0.000245 * System.Math.Log(SubMod) / Log10
                                BB = 0.0658 * SubMod ^ 0.559!
                                NtoFail = 10000.0! * (AA / StrainMax) ^ BB
                            Else
                                '         Straight line subgrade criterion.
                                '          BB = 6.22439  ' Original
                                '          AA = 0.005    ' Original

                                SubMod = 15000 ' Use the original model with SG modulus at 15,000 for large departure levels.
                                AAorig = 0.000247 + 0.000245 * System.Math.Log(SubMod) / Log10
                                BBorig = 0.0658 * SubMod ^ 0.559!
                                AAorig = AAorig * 10000 ^ (1 / BBorig) '* 0.93
                                BB = 8.1
                                AA = 0.004 '* 0.93

                                StrainBreak = (BB * System.Math.Log(AA) / Log10 - BBorig * System.Math.Log(AAorig) / Log10)
                                StrainBreak = 10.0# ^ (StrainBreak / (BB - BBorig))
                                '          Debug.Print "StrainBreak = "; StrainBreak; AAorig; BBorig

                                If StrainMax > StrainBreak Then
                                    NtoFail = (AA / StrainMax) ^ BB
                                Else
                                    NtoFail = (AAorig / StrainMax) ^ BBorig
                                End If


                                If gBleasdaleModel Then 'Bleasdale Model (log10 on cov)
                                    Dim a11, b11, c11 As Single
                                    'Bleasdale 1
                                    a11 = -0.152516199 : b11 = 166.9196 : c11 = 1.755388

                                    'Bleasdale 2
                                    a11 = -0.163768916705
                                    b11 = 185.192806802
                                    c11 = 1.65054449461

                                    'If FF_true_ACN Then
                                    'Else
                                    'End If

                                    If StrainMax < 0.001 Then
                                        StrainMax = 0.001
                                    End If



                                    'If StrainMax <= 0.0069 Then
                                    '    NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
                                    'Else
                                    '    NtoFail = 10
                                    'End If

                                    'Bleasdale 1
                                    'If StrainMax <= 0.0017846 Then
                                    '    NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
                                    'Else
                                    '    NtoFail = (0.00418708 / StrainMax) ^ 8.1
                                    'End If

                                    'Bleasdale 2
                                    If StrainMax <= 0.001765093 Then
                                        NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
                                    Else
                                        NtoFail = (0.00414131183 / StrainMax) ^ 8.1
                                    End If


                                End If 'gBleasdaleModel


                                publicNtoFail = CSng(NtoFail)
                                publicSTRAIN = CSng(StrainMax)

                                'If NGearLoads = 2 Then 'to comment
                                '    gSTRAIN(IGearLoads) = CSng(StrainMax)
                                'ElseIf NAC = 2 Then
                                '    gSTRAIN(IA) = CSng(StrainMax)
                                'End If
                            End If  'Not CBool
                        Else ' begin kairat replace tandem
                            Damage = 0.0!

                            For IEvaly = 2 To NNodesLong - 1
                                'Changed by G.Larson and E.Phillips because without this check StrainResponse goes out of bounds.
                                'Cause may be that StrainResponse is not being properly intialized.
                                If StrainResponse.GetUpperBound(1) <= IEvaly Then
                                    Continue For
                                End If
                                If StrainResponse(IA, IEvaly) < 0 Then

                                    'FileOpen(9, "SSSS.TXT", OpenMode.Append) 'to commnet 1000
                                    'PrintLine(9, "     IA=" & IA)
                                    'PrintLine(9, " IEValy=" & IEvaly)
                                    'PrintLine(9, "UBound1=" & UBound(StrainResponse, 1))
                                    'PrintLine(9, "UBound2=" & UBound(StrainResponse, 2))
                                    'PrintLine(9, "Test1" & UBound(StrainResponse, 2))
                                    'PrintLine(9, "")
                                    'FileClose(9)


                                    ''MAX strain          525 line
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
                                        'ReDim Preserve gStrain2(NAC)

                                        gStrain2(IA) = StrainMax
                                        If StrainMax < 0.0001 Then StrainMax = 0.0001 Else Overflow = False
                                        '       Standard subgrade criterion.
                                        If Not CBool(StraightLineModel) Then
                                            AA = 0.000247 + 0.000245 * System.Math.Log(SubMod) / Log10
                                            BB = 0.0658 * SubMod ^ 0.559!
                                            NtoFail = 10000.0! * (AA / StrainMax) ^ BB
                                        Else

                                            SubMod = 15000 ' Use the original model with SG modulus at 15,000 for large departure levels.
                                            AAorig = 0.000247 + 0.000245 * System.Math.Log(SubMod) / Log10
                                            BBorig = 0.0658 * SubMod ^ 0.559!
                                            AAorig = AAorig * 10000 ^ (1 / BBorig) '* 0.93
                                            BB = 8.1
                                            AA = 0.004 '* 0.93

                                            StrainBreak = (BB * System.Math.Log(AA) / Log10 - BBorig * System.Math.Log(AAorig) / Log10)
                                            StrainBreak = 10.0# ^ (StrainBreak / (BB - BBorig))
                                            '          Debug.Print "StrainBreak = "; StrainBreak; AAorig; BBorig

                                            If StrainMax > StrainBreak Then
                                                NtoFail = (AA / StrainMax) ^ BB
                                            Else
                                                NtoFail = (AAorig / StrainMax) ^ BBorig
                                            End If


                                            If gBleasdaleModel Then 'Bleasdale Model (log10 on cov)
                                                Dim a11, b11, c11 As Single
                                                'Bleasdale 1
                                                a11 = -0.152516199 : b11 = 166.9196 : c11 = 1.755388

                                                'Bleasdale 2
                                                a11 = -0.163768916705
                                                b11 = 185.192806802
                                                c11 = 1.65054449461


                                                'If FF_true_ACN Then
                                                'Else
                                                'End If
                                                If StrainMax < 0.001 Then
                                                    StrainMax = 0.001
                                                End If



                                                If StrainMax <= 0.001765093 Then
                                                    NtoFail = 10 ^ ((a11 + b11 * StrainMax) ^ (-1 / c11))
                                                Else
                                                    NtoFail = (0.00414131183 / StrainMax) ^ 8.1
                                                End If
                                            End If 'gBleasdaleModel
                                        End If 'endif Not CBool
                                        'FFFFFFFFFFFFFFF
                                        Damage = Damage + (-1) ^ ExtrType * 1.0 / NtoFail
                                    End If   'StrainResponse(IA, IEvaly - 1)
                                End If   ' StrainResponse(IA, IEvaly) < 0
                            Next IEvaly

                            publicNtoFail = (1.0 / Damage)
                            gNtoFail(IA) = NtoFail
                            publicSTRAIN = (StrainMax)

                        End If ' end kairat replace tandem
                    End If


                    'gFlex_NtoFail(IA) = CSng(NtoFail) 'PCN method for flexible
                    'gFlex_NtoFail(IA) = (publicNtoFail) 'PCN method for flexible. Commented by ED: unused
                    gSTRAIN(IA) = publicSTRAIN

                    ACCDFmax = 0 ' AC means aircraft not asphalt.
                    If IGearLoads = 1 Then ' Report maximum CDF and CtoP for multiple gear aircraft. GFH 08/10/03.
                        jobCDFacrftMaxtable(ISect, IA) = 0 ' GFH 08/04/03. Global variables, so must be reset.
                        jobCtoPtable(ISect, IA) = 0 ' GFH 08/04/03.
                    End If
                    OFFSET = 0.0!
                    For IOFF = 1 To NOFF
                        'System.Windows.Forms.Application.DoEvents()

                        If Mid(ACName(I), 2, 4) = "747" Then
                            GearACType = 3
                        End If

                        GearACType = AC(LI).libIGear
                        GearLoadType = AC(LI).libGear
                        If Mid(AC(LI).libACName, 1, 4) = "B747" Then
                            GearACType = 3
                        End If

                        If GearLoadType = "WFBF" Then
                            GearACType = DualTandemMainGear
                            If IGearLoads = 1 Then GearLoadType = "WF" ' Wing dual-tandem.
                            If IGearLoads = 2 Then GearLoadType = "BF" ' Body dual-tandem.
                        End If
                        If GearLoadType = "WFBN" Then
                            GearACType = DualTandemMainGear
                            If IGearLoads = 1 Then GearLoadType = "WF" ' Wing dual-tandem.
                            If IGearLoads = 2 Then GearLoadType = "BN" ' Body triple-dual-tandem.
                        End If


                        '!!!!!!!!!
                        'Dim time1, time2 As Single
                        'FileOpen(18, "9999.txt", OpenMode.Append)
                        ''PrintLine(18, "1")
                        'time1 = timeGetTime


                        'Call LeafCtoPFlex(IA, GearACType, GearLoadType, OFFSET, CovToPass, Depth)
                        'Call CoverageToPassFlexible(IA, GearACType, IGearLoads, GearLoadType, OFFSET, CovToPass, Depth)


                        If AC(LI).libGear = "X" Then
                            'CToPFlexGeneral13(ByRef IA As Short, ByVal OFFSET As Single, ByRef CovToPass As Single, ByRef Depth As Double)
                            Call CoverageToPassFlexGeneral13B(IA, IGearLoads, OFFSET, CovToPass, Depth)
                            'Call CoverageToPassFlexGeneral13B(IA, IGearLoads, OFFSET, CovToPass, 0)
                            'If AC(LI).libNTires = 8 Then
                            '    CovToPass = CovToPass / 2
                            'End If

                        Else
                                Call CoverageToPassFlexible(IA, GearACType, IGearLoads, GearLoadType, OFFSET, CovToPass, Depth)
                        End If


                        'to comment CovToPass
                        'CovToPass = 1 / 1.95 '2D
                        'CovToPass = 1 / 1.4 '3D

                        If IGearLoads = 1 Then
                            AA1(IA, IOFF) = CovToPass
                        ElseIf IGearLoads = 2 Then
                            AA2(IA, IOFF) = CovToPass
                        End If



                        'time2 = timeGetTime

                        'IA, GearACType,             GearLoadType, OFFSET, CovToPass, Depth
                        'IA, GearACType, IGearLoads, GearLoadType, OFFSET, CovToPass, Depth

                        'FileOpen(18, "ZZZZZ.txt", OpenMode.Append)
                        'PrintLine(18, IA & " " & GearACType & " " & IGearLoads & " " & GearLoadType & " " & OFFSET & " " & CovToPass & " " & Depth)
                        'FileClose(18)

                        'PrintLine(18, (time2 - time1))
                        'FileClose(18)

                        If gTandemFnew Then ' kairat replace tandem
                            ACCDF = CSng(Reps(IA) * CovToPass * Damage) ' Aircraft CDF -kairat
                        Else
                            ACCDF = CSng(Reps(IA) * CovToPass / NtoFail) ' Aircraft CDF.
                        End If

                        If ACCDF > ACCDFmax Then ACCDFmax = ACCDF

                        If IGearLoads = 1 Then ' GFH 08/10/03.
                            lclCDF(IA, IOFF) = ACCDF ' JIA 08/04/03.
                            CtoP(IA, IOFF) = CovToPass ' GFH 08/10/03.
                        Else
                            '         Sum CDF over gears for multiple gear aircraft.
                            lclCDF(IA, IOFF) = lclCDF(IA, IOFF) + ACCDF ' GFH 08/10/03.
                            '         Report maximum CtoP for multiple gear aircraft.
                            If CovToPass > CtoP(IA, IOFF) Then CtoP(IA, IOFF) = CovToPass ' GFH 08/10/03.
                        End If
                        If lclCDF(IA, IOFF) > jobCDFacrftMaxtable(ISect, IA) Then jobCDFacrftMaxtable(ISect, IA) = lclCDF(IA, IOFF) ' GFH 08/10/03.
                        If jobCtoPtable(ISect, IA) < CtoP(IA, IOFF) Then jobCtoPtable(ISect, IA) = CtoP(IA, IOFF) ' GFH 08/10/03.
                        '       Note: CtoP(IA, IOFF) is used only for printing when required.

                        CDFFlexVal(IOFF) = CDFFlexVal(IOFF) + ACCDF

                        'FileOpen(18, "zzcdfflexval.txt", OpenMode.Append)
                        'PrintLine(18, IOFF & " " & OFFSET & " " & CDFFlexVal(IOFF) & " " & CovToPass & " " & Damage)
                        'FileClose(18)

                        OFFSET = OFFSET + OFFSETINC

                    Next IOFF
                    '      Debug.Print "LeafCDFFlex"; IA; IGearLoads; StrainMax; ACCDFmax
                    'Exit Try ' option for MATLAB
                    'End ' option for MATLAB
                Next IGearLoads


            Next IA




            CDFMAX = 0.0!
            For IOFF = 1 To NOFF
                If CDFMAX < CDFFlexVal(IOFF) Then
                    CDFMAX = CDFFlexVal(IOFF)
                    IControl = IOFF ' JIA 08/04/03.
                End If
            Next IOFF

            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
            For IA = 1 To NAC
                For IOFF = 1 To NOFF
                    CDFdata(1, IA, IOFF) = 0
                    CDFdata(1, NAC + 1, IOFF) = 0
                Next IOFF
            Next IA


            For IA = 1 To NAC
                For IOFF = 1 To NOFF
                    jobCDFtable(ISect, IA) = lclCDF(IA, IOFF)
                    CDFdata(1, IA, IOFF) = CSng(lclCDF(IA, IOFF))
                    CDFdata(1, NAC + 1, IOFF) = CDFdata(1, NAC + 1, IOFF) + CSng(lclCDF(IA, IOFF))
                Next IOFF
            Next IA
            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf


            ' > Jia 08/04/03
            For IA = 1 To NAC
                jobCDFtable(ISect, IA) = lclCDF(IA, IControl)
            Next IA
            ' Jia 08/04/03 <

            ' Capture CDF sweep and per-aircraft data for detailed report
            Try
                gDetailedReportData.CDFSweep.Capture(NAC, NOFF, lclCDF, CDFFlexVal, CtoP, IControl)

                If gDetailedReportData.AircraftDetails Is Nothing OrElse gDetailedReportData.AircraftDetails.Length < NAC + 1 Then
                    ReDim gDetailedReportData.AircraftDetails(NAC)
                End If
                For IA = 1 To NAC
                    Dim det As New clsAircraftDetail()
                    det.ACName = ACName(IA)
                    LI = LibIndex(IA)
                    det.GearType = AC(LI).libGear
                    det.GrossLoad = GL(IA)
                    det.TireWidth = TW(IA)
                    det.TirePressure = WT(IA)
                    det.AnnualDepartures = RepsAnnual(IA)
                    det.TotalRepetitions = Reps(IA)
                    det.VerticalStrain = gSTRAIN(IA)
                    det.NtoFail = gNtoFail(IA)
                    det.MaxCDF = jobCDFacrftMaxtable(ISect, IA)
                    det.CDFAtCriticalOffset = lclCDF(IA, IControl)
                    det.MaxCtoP = jobCtoPtable(ISect, IA)
                    det.ProjectedTireWidthAtSubgrade = TW(IA) + 2 * Depth
                    det.SubgradeModelUsed = If(CBool(StraightLineModel), If(gBleasdaleModel, "Bleasdale", "Straight-Line"), "Standard")
                    det.AsphaltModelUsed = If(Asphalt AndAlso DesigningP209DrawStructure, If(gRDEC, "RDEC", "AI"), "N/A")

                    ' Capture AA/BB used for this aircraft's NtoFail computation
                    If Not CBool(StraightLineModel) Then
                        det.NtoFailAA = 0.000247 + 0.000245 * System.Math.Log(SubMod) / Log10
                        det.NtoFailBB = 0.0658 * SubMod ^ 0.559!
                    Else
                        det.NtoFailAA = 0.004
                        det.NtoFailBB = 8.1
                        Dim detStrainBreak As Double = det.VerticalStrain
                        Dim detAAorig As Double = 0.000247 + 0.000245 * System.Math.Log(15000) / Log10
                        Dim detBBorig As Double = 0.0658 * 15000 ^ 0.559!
                        detAAorig = detAAorig * 10000 ^ (1 / detBBorig)
                        Dim sb As Double = (8.1 * System.Math.Log(0.004) / Log10 - detBBorig * System.Math.Log(detAAorig) / Log10)
                        sb = 10.0# ^ (sb / (8.1 - detBBorig))
                        det.StrainBreakpoint = sb
                        If det.VerticalStrain <= sb Then
                            det.NtoFailAA = detAAorig
                            det.NtoFailBB = detBBorig
                        End If
                    End If
                    det.NGearLoads = If(AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN", 2, If(AC(LI).libGear = "X", AC(LI).libNGroups, 1))

                    ' Capture per-offset data
                    For IOFF = 1 To NOFF
                        det.CDFByOffset(IOFF) = lclCDF(IA, IOFF)
                        det.CtoPByOffset(IOFF) = CtoP(IA, IOFF)
                    Next
                    gDetailedReportData.AircraftDetails(IA) = det
                Next IA
            Catch ex As Exception
                ' Non-critical reporting; do not interrupt computation
            End Try

            '  For IOFF = 1 To NOFF
            '    OFFSET = (IOFF - 1) * 10!
            '    Debug.Print LPad$(6, Format(OFFSET, "0.0"));
            '    Debug.Print LPad$(8, Format(CtoP(1, IOFF) * 100!, "0.000"));
            '    Debug.Print LPad$(8, Format(CtoP(2, IOFF) * 100!, "0.000"));
            '    Debug.Print LPad$(8, Format(CtoP(3, IOFF) * 100!, "0.000"));
            '    Debug.Print LPad$(8, Format(CtoP(4, IOFF) * 100!, "0.000"));
            '    Debug.Print LPad$(8, Format(CDFFlexVal(IOFF), "0.00000"))
            '  Next IOFF
            '  Debug.Print "Max CDF     = "; LPad$(8, Format(CDFMAX, "0.00000"))

            If DesigningStr And FileErr Then
                DesigningStr = False
                S = "There has been an error reading" & NL
                S = S & "strains from the data file. This" & NL
                S = S & "may have been caused by a very" & NL
                S = S & "thin surface layer. The design" & NL
                S = S & "will be ended." & NL2
                S = S & "Please check the structure."
                Ret = MsgBoxDQ(S, 0, "File Error")
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

    Sub CompforStab(ByRef FSlope As Single, ByRef IG As Single)
        ' Compensation for stabilized base in failure model.
        ' First compute an equivalent thickness based on stabilized,
        ' or undefined, layer modulus, or if aggregate. Stabilized
        ' base with E = 500,000 has thickness unchanged. All other
        ' layers have thickness changed to find equivalent thickness.
        Dim A, Slope, B As Single
        Dim EquivThick, AggEquivFactor As Single
        Dim EquivFactor As Single
        Dim I, II As Short
        AggEquivFactor = 0.5

        EquivThick = 0.0!
        If MultipleSubgradeLayers Then II = 3 Else II = NPLayers - 1S
        '  For I = IG To NPLayers - 1
        For I = CShort(IG) To II

            S = LayerType(LCode(I))
            If S = NSG Then Exit For ' Just in case.
            If S = NAgBase Or S = NAgSBase Then
                EquivThick = EquivThick + AggEquivFactor * Thick(I) / 8.0!
            Else
                Temp = Modulus(I)
                If Temp < 200000.0! Then Temp = 200000.0!
                If Temp > 700000.0! Then Temp = 700000.0!
                Slope = (1.0! - AggEquivFactor) / (700000.0! - 200000.0!)
                EquivFactor = AggEquivFactor + Slope * (Temp - 200000.0!)
                EquivThick = EquivThick + EquivFactor * Thick(I) / 8
            End If
            '    Debug.Print "EquivThick = "; I; EquivThick; EquivFactor; Slope

        Next I

        If EquivThick < 0.4 Then EquivThick = 0.4
        FSlope = CSng(0.25 * 10 ^ (1.2 * (1.0! - EquivThick)))

        System.Diagnostics.Debug.WriteLine("EquivThick = " & EquivThick & FSlope)

        Temp = Modulus(NPLayers)
        If MultipleSubgradeLayers Then Temp = Modulus(4)
        Temp1 = 50000.0!
        If Temp > Temp1 Then Temp = Temp1
        Temp = Temp / 1000.0!
        A = (1.0! - FSlope) / 25.0!
        B = (FSlope - 1.0!) / 2500.0!
        FSlope = FSlope + Temp * A + Temp * Temp * B
        System.Diagnostics.Debug.WriteLine("FSlope = " & FSlope)

        'FSlope = 1 
    End Sub

    Function GaussArea(ByRef AP As Single, ByRef BP As Single, ByRef SIGMA As Single) As Single
        ' Compute the area under Gauss curve between A and B.
        ' Uses Euler-McLaurin between 0 and B and 0 and A.
        ' See 6.7 in Numerical Methods by Irons.

        Const INTMUL As Single = 0.3989423 ' 1. / Sqr(2 * PI)
        Const N As Short = 4
        Const HALF As Single = 1.0! / 2.0!
        Const CORREC As Single = 1.0! / 24.0!
        Static ZA, A, B, ZB As Single
        Static HA, Temp, HB As Single
        Static INTA, INTB As Single
        Static I As Short

        If SIGMA < 0.000001 Then ' If the tire touches the point
            If AP <= 0.0! And 0.0! <= BP Then ' on the runway when SIGMA is
                GaussArea = 1.0! ' zero, then the point is
            Else ' always covered.
                GaussArea = 0.0!
            End If
            Exit Function
        End If

        A = AP / SIGMA : B = BP / SIGMA
        If A > B Then ' Just in case.
            Temp = A : A = B : B = Temp
        End If
        HA = A / N : HB = B / N ' Same signs as A and B.
        ZA = -HA * HALF : ZB = -HB * HALF
        INTA = 0.0! : INTB = 0.0!

        For I = 1 To N
            ZA = ZA + HA : ZB = ZB + HB ' Backwards for negative A or B.
            INTA = CSng(INTA + System.Math.Exp(-HALF * ZA * ZA)) ' Always positive.
            INTB = CSng(INTB + System.Math.Exp(-HALF * ZB * ZB))
        Next I

        ZA = ZA + HA * HALF : ZB = ZB + HB * HALF
        ' CORREC = 1! / 24! + (3! - X * X) * H * H * 7! / 5760! ' Full correction.
        ' Signs of integrals now take the signs of A and B
        INTA = CSng(HA * (INTA - (HA * ZA * System.Math.Exp(-HALF * ZA * ZA)) * CORREC))
        INTB = CSng(HB * (INTB - (HB * ZB * System.Math.Exp(-HALF * ZB * ZB)) * CORREC))
        If System.Math.Abs(A) > 5.0! Then INTA = HALF * System.Math.Sign(A) / INTMUL
        If B > 5.0! Then INTB = HALF / INTMUL
        ' Negative values for A and B are handled correctly. Area is always positive.
        GaussArea = (INTB - INTA) * INTMUL

    End Function

    Sub FAAModulusThick(ByRef ThickS As Single, ByRef LCode As Short, ByRef ModUnder As Single, ByRef NS As Short, ByRef TS As Single)
        ' Compute thicknesses and modulus values for sublayers within
        ' aggregate base and subbase layers.
        'Static NSsubbase As Short 'ikawa 999
        Dim I As Short
        Dim D, C, julThickMin As Single
        Dim DIV As Single
        Static NSAgBase, NSAgSBase As Short
        Dim MaxThick8 As Single
        Dim MaxThick10 As Single

        MaxThick8 = 8 : MaxThick10 = 10

        If LayerType(LCode) = NAgBase Then
            '  If LCode = AggBaseLC Then
            C = 10.52 : D = 2.1 : julThickMin = 10.0!

            If gP209_C1052D20 Then
                C = 10.52 : D = 2.0 : julThickMin = 10.0! 'new values
            End If

            'If LCode = 21 Then
            '    C = 10.52 : D = 2.2 : julThickMin = 10.0! 'PPPP
            '    C = 10.82 : D = 2.25 : julThickMin = 10.0! 'PPPP
            '    C = 10.52 : D = 2.2 : julThickMin = 10.0! 'PPPP
            'End If


        ElseIf LayerType(LCode) = NAgSBase Then
            C = 7.18 : D = 1.56 : julThickMin = 8.0!

            If gP154_C688D156 Then
                C = 6.88 : D = 1.56 : julThickMin = 8.0! 'new values
            End If

            'to comment C D values
            'C = 7.18 : D = 1.66 : julThickMin = 8.0!

        Else


            Ret = MsgBoxDQ("Incorrect layer type in ModulusThick", 0, "")
            Stop
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
        Dim LT1, minThick As Single
        Dim TSarray(NS) As Single

        If gNewModulus_P154 And LayerType(LCode) = NAgSBase Then 'Aggregate Uncrushed

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

            If LT1 < minThick And NS > 0 Then
                TSarray(1) = MaxThick8 + LT1
                TSS_P154(1) = TSarray(1)
            ElseIf NS = 0 Then 'PPPP NS = 0 case
                ReDim TSarray(1)
                TSarray(1) = LT1
                TSS_P154(1) = TSarray(1)
            Else
                TSarray(1) = LT1
                TSS_P154(1) = TSarray(1)
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





        Dim KeepMod2 As Single
        Dim tempTS, TS1, expr1 As Single

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
                If NS = 0 Then NS = 1 'PPPP NS = 1
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



    End Sub

    Sub FAAModulus(ByRef RErr As Boolean, ByRef SubLayers As Boolean)

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
        Dim TSB As Single
        Dim NSS As Short
        Dim TSS As Single
        '  Dim BaseMod(MaxNPLayers) as Single       in Decs.
        '  Dim SubbaseMod(MaxNPLayers) as Single    in Decs.

        SubLayers = False
        RErr = CBool(0)
        IDB = 0 : LB = 0 : NSB = 0
        IDS = 0 : LS = 0 : NSS = 0
        For I = 1 To NPLayers
            If LayerType(LCode(I)) = NAgBase Then
                IDB = IDB + 1S : LB = I
            End If
            If LayerType(LCode(I)) = NAgSBase Then
                IDS = IDS + 1S : LS = I
            End If
        Next I

        If IDB > 1 Then
            S = "There are " & IDB.ToString("f0") & " aggregate base layers. "
            S = S & "This is not a valid structure for the design procedure. "
            S = S & "Please change the structure so that it has only one aggregate base layer."


            Ret = MsgBoxDQ(S, 0, "Calculating Modulus in Design")
            RErr = True
            Exit Sub
        End If

        If IDS > 1 Then
            S = "There are " & IDS.ToString("f0") & " aggregate subbase layers. "
            S = S & "This is not a valid structure for the design procedure. "
            S = S & "Please change the structure so that it has only one aggregate subbase layer."


            Ret = MsgBoxDQ(S, 0, "Calculating Modulus in Design")
            RErr = True
            Exit Sub
        End If

        If IDB > 0 And IDS > 0 Then ' Base and subbase both present.
            If LB > LS Then
                S = "An aggregate base layer is located below an aggregate subbase layer. "
                S = S & "This is not a valid structure for the design procedure. "
                S = S & "Please change the structure so that the base is above the subbase."


                Ret = MsgBoxDQ(S, 0, "Calculating Modulus in Design")
                RErr = True
                Exit Sub
            End If
        End If

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

        If Not LayerSwitch Then

            '    Debug.Print "Mod "; LayerSwitch; SubLayers
            For I = 1 To NPLayers
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
            julNPLayers = NPLayers

        Else

            II = 1
            For I = 1 To NPLayers

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

        S = LayerType(LCode(1))

        If DesignType = NewFlex Then 'FAAModulus
            NEvalDepths = 1
            EvalDepth(1) = -julThick(1)
            EvalDepth(2) = 0.0!
            For J = 1 To julNPLayers - 1S
                EvalDepth(2) = EvalDepth(2) + julThick(J)
            Next J
            EvalDepth(2) = EvalDepth(2) * 1.0001
            EvalDepth(1) = EvalDepth(2)
        End If

        If AlternateSG Then
            If DesignType = NewFlex Then 'FAAModulus
                NEvalDepths = 1
                EvalDepth(1) = 0.0!
                For J = 1 To IterLayerChosen
                    EvalDepth(1) = EvalDepth(1) + Thick(J)
                Next J
                EvalDepth(1) = EvalDepth(1) * 1.0001
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
            EvalDepth(1) = EvalDepth(1) * 1.0001
        End If

        ' EvalDepths for rigid overlays are set in design subs.




        Dim z1 As Integer

        For z1 = 1 To NPLayers - 1
            If LCode(z1) = 21 Then
                Modulus(z1) = CSng(0.8) * Modulus(z1)
            End If
        Next







    End Sub

    Sub CoverageToPassFlexible(ByRef IA As Short, ByRef GearACType As Short, ByVal IGearLoads As Integer,
            ByRef GearLoadType As String, ByVal OFFSET As Single, ByRef CovToPass As Single, ByRef Depth As Double)

        Static SigmaW, TireWidth() As Single, SouthMost As Single, I As Integer
        Static NFWheels() As Integer, XFWheels(,), YFWheels(,) As Single
        Static Keep(,), NotBelly As Boolean
        Static NumOfTires As Short, iWFBF As Integer

        If OFFSET > 0.0! Then GoTo CPTOTAL1 ' at very end of sub.
        'If OFFSET > 0.0! Then GoTo CPTOTAL ' at very end of sub.

        ReDim XFWheels(NAC * 2, MaxNTires * 2), YFWheels(NAC * 2, MaxNTires * 2)
        ReDim NFWheels(NAC * 2), TireWidth(NAC * 2), Keep(NAC * 2, MaxNTires * 2)

        If gPtoC1 Then 'flexible
            SigmaW = 0
        Else
            SigmaW = 30.435          ' Wander width of 70 inches.
        End If
        'SigmaW = 0.5
        Dim check4 As Double
        LI = LibIndex(IA)
        TireWidth(IA) = WT(IA)
        check4 = WT(IA)
        NumOfTires = AC(LI).libNTires
        'AC(79).libTV = 121
        'AC(79).libTG = 107
        'AC(79).libIGear = 3
        'AC(80).libIGear = 3
        'AC(80).libTS = 107
        'AC(80).libNTTrack = 0
        If AC(LI).libACName = "C-5" Then
            NumOfTires = 6
        End If

        If IGearLoads = 1 And GearLoadType = "WF" Then
            NumOfTires = 4 : iWFBF = 0
        ElseIf IGearLoads = 1 And GearLoadType = "WF" Then
            NumOfTires = 4 : iWFBF = 0
        ElseIf IGearLoads = 2 And GearLoadType = "BF" Then
            NumOfTires = 4 : iWFBF = 4
        ElseIf IGearLoads = 2 And GearLoadType = "BN" Then
            NumOfTires = 6 : iWFBF = 4
        Else
            iWFBF = 0
        End If

        SouthMost = 1.0E+35
        For I = (1 + iWFBF) To (NumOfTires + iWFBF)
            If AC(LI).libTY(I) < SouthMost Then SouthMost = AC(LI).libTY(I)
        Next I

        'Locate the most south row of wheels
        NFWheels(IA) = 0
        For I = (1 + iWFBF) To (NumOfTires + iWFBF)
            If Math.Abs(SouthMost - AC(LI).libTY(I)) <= TW(IA) / 2 Then
                NFWheels(IA) = NFWheels(IA) + 1S
                XFWheels(IA, NFWheels(IA)) = AC(LI).libTX(I)
                YFWheels(IA, NFWheels(IA)) = AC(LI).libTY(I)
                Keep(IA, I) = True
            Else
                Keep(IA, I) = False
            End If
        Next I

        'Dim TandemNum(NFWheels(IA)) As Short
        'Dim multiplier(NFWheels(IA)) As Single
        'Dim TandemDist(AC(LI).libNTires) As Single
        Static TandemNum(NFWheels(IA)) As Short
        Static multiplier1(NFWheels(IA)) As Single
        Static TandemDist(AC(LI).libNTires) As Single

        ReDim Preserve TandemNum(NFWheels(IA))
        ReDim Preserve multiplier1(NFWheels(IA))
        ReDim Preserve TandemDist(AC(LI).libNTires)

        For I = 1 To (AC(LI).libNTires)
            TandemDist(I) = 0
        Next I

        For I = 1 To NFWheels(IA)
            multiplier1(I) = 0
            TandemNum(I) = 0
        Next

        For I = (1 + 0) To (NFWheels(IA) + 0)
            'NFWheels(IA) = 0

            For I2 = (1 + iWFBF) To (NumOfTires + iWFBF)
                If Not Keep(IA, I2) Then
                    If (Math.Abs(XFWheels(IA, I) - AC(LI).libTX(I2)) <= WT(IA) / 2) Then
                        TandemNum(I) = TandemNum(I) + 1S
                        If TandemDist(I) = 0 Or (Math.Abs(YFWheels(IA, I) - AC(LI).libTY(I2)) < TandemDist(I)) Then
                            TandemDist(I) = Math.Abs(YFWheels(IA, I) - AC(LI).libTY(I2))
                        End If
                        '        NFWheels(IA) = NFWheels(IA) + 1S
                        '        XFWheels(IA, NFWheels(IA)) = AC(LI).libTX(I)
                    End If
                End If
            Next I2

        Next

        For I = (1) To (NFWheels(IA))
            Dim TNumber As Single
            TNumber = TandemNum(I) + 1
            If TandemNum(I) > 0 Then
                If Depth > 2 * (TandemDist(I) - TW(IA)) Then
                    multiplier1(I) = 1
                ElseIf Depth > (TandemDist(I) - TW(IA)) Then
                    multiplier1(I) = CSng((TNumber * 2 - 1)) - CSng((TNumber - 1) * Depth / (TandemDist(I) - TW(IA)))
                Else
                    multiplier1(I) = TNumber
                End If
            Else
                multiplier1(I) = 1
            End If
        Next

        'CPTOTAL:


CPTOTAL:  ' Jump here on all calls after the first.

        'Call CoverageToPassFlexibleCore2(SigmaW, TireWidth, NFWheels, XFWheels, _
        'IA, iWFBF, GearACType, GearLoadType, OFFSET, CovToPass, Depth, multiplier)

        'Dim aa(), BB(), AREA, YOFF As Single, aa2(), BB2() As Single
        'Dim CtoP1, TEFF As Single, GP As Single
        'Dim RightMostY, xCenter, yCenter As Single, NWheels As Short
        'Dim Left(), Right(), Left2(), Right2() As Single
        'Dim Factor As Single

        Static aa(), BB(), AREA, YOFF As Single, aa2(), BB2() As Single
        Static CtoP1, TEFF As Single, GP As Single
        Static RightMostY, xCenter, yCenter As Single, NWheels As Short
        Static Left(), Right(), Left2(), Right2() As Single
        Static Factor As Single
        Dim check As Double
        check = TireWidth(IA)
        LI = LibIndex(IA)
        TEFF = CSng(Math.Abs(Depth))
        GP = TEFF + CSng(TireWidth(IA))
        Dim check2 As Double
        check2 = GP
        'PPPP1
        NotBelly = (InStr(4, ACName(IA), "Belly", CompareMethod.Text) = 0) And Not (AC(LI).libGear = "A")
        NotBelly = NotBelly Or (InStr(1, ACName(IA), "A380", CompareMethod.Text) > 0)
        NotBelly = NotBelly Or (InStr(1, ACName(IA), "B747", CompareMethod.Text) > 0)

        RightMostY = -1.0E+35
        For I = 1 To NFWheels(IA)
            If XFWheels(IA, I) > RightMostY Then RightMostY = XFWheels(IA, I)
        Next I

        xCenter = 0.0! : yCenter = 0.0!
        NWheels = AC(LI).libNTires
        check2 = NWheels
        If AC(LI).libACName = "C-5" Or GearLoadType = "BN" Then
            NWheels = 6
        ElseIf GearLoadType = "WF" Or GearLoadType = "BF" Then
            NWheels = 4
        End If

        For J = 1 + iWFBF To NWheels + iWFBF
            xCenter = xCenter + AC(LI).libTX(J)
            yCenter = yCenter + AC(LI).libTY(J)
        Next J
        xCenter = xCenter / NWheels : yCenter = yCenter / NWheels


        If GearLoadType = "B" Then 'added ikawa 2013
            xCenter = xCenter + AC(LI).libTT / 2 + Math.Abs(RightMostY)
        End If


        Dim chk11, chk12 As Boolean '2556 line
        chk11 = (GearLoadType = "WF" Or GearLoadType = "BF" Or GearLoadType = "BF" Or GearLoadType = "BN")

        If NotBelly And Not chk11 Then 'Line 2537 =====================================
            xCenter = xCenter + AC(LI).libTS / 2 + Math.Abs(RightMostY)
        ElseIf chk11 Then
            For I = 1 To NFWheels(IA)
                XFWheels(IA, I) = XFWheels(IA, I) - xCenter
            Next
            'xCenter = Math.Abs(xCenter + Math.Abs(RightMostY))
            xCenter = Math.Abs(xCenter)
        End If


        ReDim aa(NFWheels(IA)), BB(NFWheels(IA))
        ReDim aa2(NFWheels(IA)), BB2(NFWheels(IA))
        ReDim Left(NFWheels(IA)), Right(NFWheels(IA)), Left2(NFWheels(IA)), Right2(NFWheels(IA))


        Left(1) = CSng(GP / 2.0)
        For I = 1 To NFWheels(IA) - 1
            If Math.Abs(XFWheels(IA, I) - XFWheels(IA, I + 1)) < GP Then
                Right(I) = Math.Abs(XFWheels(IA, I) - XFWheels(IA, I + 1)) / 2
                Left(I + 1) = Math.Abs(XFWheels(IA, I) - XFWheels(IA, I + 1)) / 2
            Else
                Right(I) = CSng(GP / 2.0) : Left(I + 1) = CSng(GP / 2.0)
            End If
        Next
        Right(NFWheels(IA)) = CSng(GP / 2.0)

        For I = 1 To NFWheels(IA)    ' Do for each wheel.
            check2 = XFWheels(IA, I)
            check2 = -xCenter
            YOFF = CSng((OFFSET + (-xCenter + XFWheels(IA, I))))
            check2 = YOFF
            check2 = Left(I)
            check2 = Right(I)
            aa(I) = (CSng(YOFF - Left(I)))
            BB(I) = (CSng(YOFF + Right(I)))
            check2 = aa(I)
            check2 = BB(I)
        Next I


        If NotBelly Then 'Line 2571 ===========================
            Left2(1) = CSng(GP / 2.0)
            For I = 1 To NFWheels(IA) - 1
                If Math.Abs(XFWheels(IA, I) - XFWheels(IA, I + 1)) < GP Then
                    Right2(I) = Math.Abs(XFWheels(IA, I) - XFWheels(IA, I + 1)) / 2
                    Left2(I + 1) = Math.Abs(XFWheels(IA, I) - XFWheels(IA, I + 1)) / 2
                Else
                    Right2(I) = CSng(GP / 2.0) : Left2(I + 1) = CSng(GP / 2.0)
                End If
            Next

            Right2(NFWheels(IA)) = CSng(GP / 2.0)
            For I = 1 To NFWheels(IA)    ' Do for each wheel.


                YOFF = CSng((OFFSET + (xCenter + XFWheels(IA, I))))
                aa2(I) = (CSng(YOFF - Left2(I)))
                BB2(I) = (CSng(YOFF + Right2(I)))
            Next I
        End If

        If BB(NFWheels(IA)) > aa2(1) And NotBelly Then 'Line 2590 ===================
            BB(NFWheels(IA)) = (BB(NFWheels(IA)) + aa2(1)) / 2
            aa2(1) = BB(NFWheels(IA))
        End If


CPTOTAL1:


        If OFFSET > 0.0! Then
            Dim oo1 As Integer
            For oo1 = 1 To NFWheels(IA)
                aa(oo1) = aa(oo1) + 10
                BB(oo1) = BB(oo1) + 10
                aa2(oo1) = aa2(oo1) + 10
                BB2(oo1) = BB2(oo1) + 10
                YOFF = YOFF + 10
            Next oo1

        End If

        If OFFSET = 40 Then
            OFFSET = 40
        End If


        CtoP1 = 0.0!
        For I = 1 To NFWheels(IA)    ' Do for each wheel.
            If (aa(I) < 0 And BB(I) < 0) Then
                AREA = GaussArea(Math.Abs(aa(I)), Math.Abs(BB(I)), CSng(SigmaW))
            Else
                AREA = GaussArea(aa(I), BB(I), CSng(SigmaW))
            End If

            If AC(LI).libACName = "C-5" Then
                AREA = AREA * 2
            End If
            'CtoP1 = CtoP1 + AREA
            If gTandemFnew Then
                CtoP1 = CtoP1 + AREA

            Else
                CtoP1 = CtoP1 + AREA * multiplier1(I)
            End If
            gFirstIter = False

            'FileOpen(9, "yyyy.txt", OpenMode.Append, OpenAccess.Write)
            'Print(9, LPad(3, CStr(I)))
            'Print(9, LPad(9, CStr(OFFSET)))
            'Print(9, LPad(9, CStr(XFWheels(IA, I))))
            'Print(9, LPad(9, CStr(OFFSET - XFWheels(IA, I))))
            'Print(9, LPad(9, CStr(GP)))
            'Print(9, LPad(14, CStr(Math.Round(aa(I), 7))))
            'Print(9, LPad(14, CStr(Math.Round(BB(I), 7))))
            'Print(9, LPad(14, CStr(Math.Round(AREA, 7))))
            'Print(9, LPad(14, CStr(Math.Round(CtoP1, 7))))
            'PrintLine(9, " ")
            'FileClose(9)

        Next I

        If OFFSET = 400 Then
            OFFSET = 400
        End If


        If NotBelly Then 'Line 2649 =================================
            For I = 1 To NFWheels(IA)    ' Do for each wheel.
                AREA = GaussArea(aa2(I), BB2(I), CSng(SigmaW))
                'CtoP1 = CtoP1 + AREA * multiplier1(I)

                '   gTandemFnew = false

                If gTandemFnew Then
                    CtoP1 = CtoP1 + AREA

                Else
                    CtoP1 = CtoP1 + AREA * multiplier1(I)
                End If
                gFirstIter = False

                If AC(LI).libACName = "C-5" Then
                    AREA = AREA * 2
                End If

                'FileOpen(9, "yyyy.txt", OpenMode.Append, OpenAccess.Write)
                'Print(9, LPad(3, CStr(I)))
                'Print(9, LPad(9, CStr(OFFSET)))
                'Print(9, LPad(9, CStr(XFWheels(IA, I))))
                'Print(9, LPad(9, CStr(OFFSET - XFWheels(IA, I))))
                'Print(9, LPad(9, CStr(GP)))
                'Print(9, LPad(14, CStr(Math.Round(aa2(I), 7))))
                'Print(9, LPad(14, CStr(Math.Round(BB2(I), 7))))
                'Print(9, LPad(14, CStr(Math.Round(AREA, 7))))
                'Print(9, LPad(14, CStr(Math.Round(CtoP1, 7))))
                'PrintLine(9, " ")
                'FileClose(9)
            Next I

        End If



        Dim NFWheels1 As Integer, YFWheels2() As Single
        Dim TEMP As Single
        ReDim YFWheels2(20)

        NFWheels1 = 1 : YFWheels2(1) = AC(LI).libTY(1)
        For I = 2 To AC(LI).libNTires
            If Math.Abs(AC(LI).libTX(1) - AC(LI).libTX(I)) <= WT(IA) / 2 Then
                NFWheels1 = NFWheels1 + 1
                YFWheels2(NFWheels1) = AC(LI).libTY(I)
            End If
        Next I

        TEMP = Math.Abs(YFWheels2(2) - YFWheels2(1)) - CSng(TW(IA))
        TEFF = CSng(Depth)
        If TEFF > 2 * TEMP Then
            Factor = 1
        ElseIf TEFF > TEMP Then
            Factor = (NFWheels1 * 2 - 1) - (NFWheels1 - 1) * TEFF / TEMP
        Else
            Factor = NFWheels1
        End If

        'CtoP(IA, CInt(OFFSET / 10) + 1) = CtoP1
        CovToPass = CtoP1

        'If gPtoC1 Then 'flexible pavements
        '    CovToPass = Factor
        'End If



        'Next IA

    End Sub

    Sub CoverageToPassFlexGeneral13B(ByRef IA As Short, ByVal GearNumber As Integer, ByVal OFFSET As Single, ByRef CovToPass As Single, ByRef Depth As Double)

        Static SigmaW, TireWidth() As Single, I As Integer
        Static NFWheels() As Integer, XFWheels(,), YFWheels(,) As Single
        Static NGWheels() As Integer, XGWheels(,), YGWheels(,) As Single
        Static Keep(,) As Boolean, BottomIndex(,) As Integer
        Static NumOfTires As Integer
        Dim shift As Short, k As Integer

        If OFFSET > 0.0! Then GoTo CPTOTAL1 ' at very end of sub.

        ReDim XFWheels(NAC * 2, MaxNTires * 2), YFWheels(NAC * 2, MaxNTires * 2)
        ReDim NFWheels(NAC * 2), TireWidth(NAC * 2), Keep(NAC * 2, MaxNTires * 2)

        ReDim NGWheels(NAC * 2)
        ReDim XGWheels(NAC * 2, MaxNTires * 2)
        ReDim YGWheels(NAC * 2, MaxNTires * 2)
        ReDim BottomIndex(NAC * 2, MaxNTires * 2)


        If gPtoC1 Then 'flexible - general aircraft
            SigmaW = 0
        Else
            SigmaW = 30.435          ' Wander width of 70 inches.
        End If


        LI = LibIndex(IA)
        TireWidth(IA) = WT(IA)

        'NumOfTires = AC(LI).libWheelsInGroup(GearNumber)

        k = AC(LI).libNTires1
        If GearNumber = 1 Then
            NumOfTires = AC(LI).libNTires
            shift = 0
        Else
            NumOfTires = AC(LI).libNTires2 * 2
            shift = CShort(AC(LI).libNTires1 * 2)
        End If
        'AliA
        'If AC(LI).libACName = "C-5" Then
        '    NumOfTires = 6
        '    'NWheels = 6
        'End If

        'XGWheels, YGWheels coordinates of tires only for one gear
        'on both sides of airplane
        For J = 1 + shift To NumOfTires + shift
            XGWheels(IA, J - shift) = AC(LI).libTX(J)
            YGWheels(IA, J - shift) = AC(LI).libTY(J)
        Next J

        'sort tandem wheels according y coordinate to determine bottom wheels
        Dim Loop2 As Integer, Switch1 As Boolean, TempX, TempY As Single


        For I = 1 To NumOfTires

            Do
                Switch1 = False
                For Loop2 = 1 To NumOfTires - 1

                    If YGWheels(IA, Loop2) > YGWheels(IA, Loop2 + 1) Then

                        TempX = XGWheels(IA, Loop2)
                        XGWheels(IA, Loop2) = XGWheels(IA, Loop2 + 1)
                        XGWheels(IA, Loop2 + 1) = TempX

                        TempY = YGWheels(IA, Loop2)
                        YGWheels(IA, Loop2) = YGWheels(IA, Loop2 + 1)
                        YGWheels(IA, Loop2 + 1) = TempY

                        Switch1 = True
                        'Limit2 = Loop2
                    End If

                Next Loop2
            Loop While Switch1

        Next

        Dim BottomWheels(NAC * 2, MaxNTires * 2) As Boolean
        Dim TDmultiplier(NumOfTires) As Single

        For I = 1 To NumOfTires
            TDmultiplier(I) = 1
            BottomWheels(IA, I) = False
            BottomIndex(IA, J) = 0
        Next

        'Locate bottom wheels - 1st iteration
        For I = 1 To NumOfTires
            For J = I + 1 To NumOfTires

                If Math.Abs(XGWheels(IA, J) - XGWheels(IA, I)) <= TireWidth(IA) / 2 Then

                    If BottomIndex(IA, I) > 0 Then
                        If Math.Abs(XGWheels(IA, J) - XGWheels(IA, BottomIndex(IA, J))) <= TireWidth(IA) / 2 Then
                            BottomWheels(IA, I) = False
                            BottomIndex(IA, J) = I
                        Else
                            BottomWheels(IA, J) = True
                            BottomIndex(IA, J) = 0
                        End If
                    Else
                        BottomWheels(IA, I) = True
                        BottomWheels(IA, J) = False
                        BottomIndex(IA, J) = I
                    End If

                End If
            Next J
        Next I

        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then Continue For
            If BottomIndex(IA, I) > 0 Then Continue For

            BottomWheels(IA, I) = True
        Next I


        'For bottom wheels determine wheels in the columns
        'limit1 - # of wheels in column

        Dim ww(NumOfTires, k) As Integer
        Dim yValues(NumOfTires, k) As Single
        Dim i1 As Integer, limit1(NumOfTires) As Integer

        'repeat
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                i1 = 0

                For J = 1 To NumOfTires
                    If I <> J Then
                        If Math.Abs(XGWheels(IA, I) - XGWheels(IA, J)) <= TW(IA) / 2 Then

                            i1 = i1 + 1
                            ww(I, i1) = J
                            limit1(I) = i1
                            yValues(I, i1) = YGWheels(IA, J)
                        End If
                    End If
                Next

            End If 'BottomWheels(I) = True
        Next

        'sort tandem wheels according y coordinate to calculate tandem distance
        Dim Limit, Loop1 As Integer, Switch As Boolean, TempVal As Single

        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                Limit = limit1(I) - 1

                Do
                    Switch = False
                    For Loop1 = 1 To Limit

                        If yValues(I, Loop1) > yValues(I, Loop1 + 1) Then

                            TempVal = yValues(I, Loop1)
                            yValues(I, Loop1) = yValues(I, Loop1 + 1)
                            yValues(I, Loop1 + 1) = TempVal

                            TempVal = ww(I, Loop1)
                            ww(I, Loop1) = ww(I, Loop1 + 1)
                            ww(I, Loop1 + 1) = CInt(TempVal)

                            Switch = True
                            Limit = Loop1
                        End If

                    Next Loop1
                Loop While Switch

            End If 'BottomWheels(I) = True
        Next


        'calculate tandem distance
        Dim tdValues(NumOfTires, k) As Single
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                For J = 1 To limit1(I)

                    If J = 1 Then
                        tdValues(I, J) = yValues(I, J) - YGWheels(IA, I)
                    Else
                        tdValues(I, J) = yValues(I, J) - yValues(I, J - 1)
                    End If

                Next
            End If 'BottomWheels(I) = True
        Next

        'calculate tandem multiplier
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                For J = 1 To limit1(I)

                    If Depth > 2 * (tdValues(I, J) - TW(IA)) Then
                        'no change
                    ElseIf Depth > (tdValues(I, J) - TW(IA)) Then

                        If J = 1 Then
                            TDmultiplier(I) = 3 - CSng(Depth / (tdValues(I, J) - TW(IA)))
                        Else
                            TDmultiplier(I) = TDmultiplier(I) + 2 - CSng(Depth / (tdValues(I, J) - TW(IA)))
                        End If

                    Else
                        TDmultiplier(I) = TDmultiplier(I) + 1
                    End If

                Next
            End If 'BottomWheels(I) = True
        Next


        Static TDnumber() As Single
        'Locate the most south row of wheels in seperate arrays
        NFWheels(IA) = 0
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                NFWheels(IA) = NFWheels(IA) + 1S
                ReDim Preserve TDnumber(NFWheels(IA))

                XFWheels(IA, NFWheels(IA)) = XGWheels(IA, I)
                YFWheels(IA, NFWheels(IA)) = YGWheels(IA, I)

                TDnumber(NFWheels(IA)) = TDmultiplier(I)
                Keep(IA, I) = True
            Else
                Keep(IA, I) = False
            End If
        Next I

        'make sure X coordinates increase from left to rigth
        For I = 1 To NFWheels(IA)

            Limit = NFWheels(IA) - 1
            Do
                Switch = False
                For Loop1 = 1 To Limit

                    If XFWheels(IA, Loop1) > XFWheels(IA, Loop1 + 1) Then

                        TempVal = XFWheels(IA, Loop1)
                        XFWheels(IA, Loop1) = XFWheels(IA, Loop1 + 1)
                        XFWheels(IA, Loop1 + 1) = TempVal

                        TempVal = TDnumber(Loop1)
                        TDnumber(Loop1) = TDnumber(Loop1 + 1)
                        TDnumber(Loop1 + 1) = TempVal

                        Switch = True
                        Limit = Loop1
                    End If

                Next Loop1
            Loop While Switch


        Next


CPTOTAL:  ' Jump here on all calls after the first.

        Static AREA, YOFF As Single
        Static CtoP1, TEFF As Single, GP As Single
        Static Left(), Right() As Single
        Dim middle As Single


        LI = LibIndex(IA)
        TEFF = CSng(Math.Abs(Depth))
        GP = TEFF + CSng(TireWidth(IA))

        ReDim Left(NFWheels(IA)), Right(NFWheels(IA))

        Left(1) = XFWheels(IA, 1) - GP / 2
        For I = 1 To NFWheels(IA) - 1
            middle = (XFWheels(IA, I) + XFWheels(IA, I + 1)) / 2

            If (XFWheels(IA, I) + GP / 2 < middle) Then
                Right(I) = XFWheels(IA, I) + GP / 2
                Left(I + 1) = XFWheels(IA, I + 1) - GP / 2
            Else
                Right(I) = middle
                Left(I + 1) = middle
            End If

        Next
        Right(NFWheels(IA)) = XFWheels(IA, NFWheels(IA)) + GP / 2


CPTOTAL1:


        If OFFSET > 0.0! Then
            Dim oo1 As Integer
            For oo1 = 1 To NFWheels(IA)
                Left(oo1) = Left(oo1) + 10
                Right(oo1) = Right(oo1) + 10
                YOFF = YOFF + 10
            Next oo1

        End If

        CtoP1 = 0.0!
        For I = 1 To NFWheels(IA)    ' Do for each wheel.
            If (Left(I) < 0 And Right(I) < 0) Then
                AREA = GaussArea(Math.Abs(Left(I)), Math.Abs(Right(I)), CSng(SigmaW))
            Else
                AREA = GaussArea(Left(I), Right(I), CSng(SigmaW))
            End If
            'AliA
            'If AC(LI).libACName = "C-5" Then
            '    AREA = AREA * 2
            'End If
            If gTandemFnew Then ' kairat replace tandem =CoverageToPassFlexGeneral13B
                CtoP1 = CtoP1 + AREA ' tandem factor expressed by multiplier1 is deleted
                'If gFirstIter Then
                '    MsgBox("Tandem factor is replaced, but for this case it would be equal to " & TDnumber(I), MsgBoxStyle.OkOnly)
                'End If
            Else
                CtoP1 = CtoP1 + AREA * TDnumber(I)
                'If gFirstIter Then
                '    MsgBox("Tandem factor is on and equal to " & TDnumber(I), MsgBoxStyle.OkOnly)
                'End If
            End If
            gFirstIter = False ' kairat replace tandem

        Next I

        CovToPass = CtoP1

    End Sub

    Sub CoverageToPassRigidSingleAC(ByVal AA As Integer, ByVal extra As Integer)

        Dim I, J As Integer
        Dim SigmaW As Double
        Dim CtoPmaxRigid As Double, CtoPmaxPointRigid As Double
        Dim AspectRatio As Double, TireWidth() As Double
        Dim NRWheels() As Integer
        Dim XRWheels(,) As Single, YRWheels(,) As Single
        Dim KeepForRigid(,) As Boolean
        Dim IA As Short, NWheels() As Short
        Dim IextraAC As Short


        IextraAC = CShort(extra)

        AspectRatio = 0.625 ' = 1 / 1.6   
        'TireWidth = 2 * WheelRadius * Math.Sqrt(AspectRatio)   ' Minor axis.


        If gPtoC1 Then 'rigid SingleAC
            SigmaW = 0
        Else
            SigmaW = 30.435          ' Wander width of 70 inches.
        End If



        ReDim XRWheels(NAC * 2, MaxNTires * 2), YRWheels(NAC * 2, MaxNTires * 2),
              KeepForRigid(NAC * 2, MaxNTires * 2), NRWheels(NAC * 2), NWheels(NAC * 2),
              TireWidth(NAC * 2)

        For IA = CShort(AA) To CShort(AA)
            LI = LibIndex(IA)
            TireWidth(IA) = WT(IA)

            NWheels(IA) = AC(LI).libNTires
            If NWheels(IA) = 0 Then
                CtoPmaxRigid = 0 : CtoPmaxPointRigid = 0
                Exit Sub
            End If

            ' Now find the rigid pass-to-coverage ratio. Only wheels in tandem which
            ' are furthern apart than 72 inches contribute to the pass-to-coverage ratio.
            ' Two wheels are defined to be in tandem when the transverse coordinates of
            ' the wheel centers are closer than, or equal to, one half the tire width.


            '""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""
            'If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
            '    IextraAC = IextraAC + 1S ' Add the body gear to the aircraft list.
            'End If

            Dim Xc, Yc As Single

            If AC(LI).libGear = "WFBF" Then

                For I = 1 To 4
                    XRWheels(IA, I) = AC(LI).libTX(I)
                    YRWheels(IA, I) = AC(LI).libTY(I)
                    KeepForRigid(IA, I) = True
                Next I

                Xc = 0 : Yc = 0
                For I1 = 1 To 4
                    Xc = Xc + XRWheels(IA, I1)
                    Yc = Yc + YRWheels(IA, I1)
                Next

                Xc = Xc / 4 : Yc = Yc / 4

                For I1 = 1 To 4
                    XRWheels(IA, I1) = XRWheels(IA, I1) - Xc
                    YRWheels(IA, I1) = YRWheels(IA, I1) - Yc
                Next I1


                For I = 1 To 4
                    XRWheels(NAC + IextraAC, I) = AC(LI).libTX(I + 4)
                    YRWheels(NAC + IextraAC, I) = AC(LI).libTY(I + 4)
                    KeepForRigid(NAC + IextraAC, I) = True
                Next I

                Xc = 0 : Yc = 0
                For I1 = 1 To 4
                    Xc = Xc + XRWheels(NAC + IextraAC, I1)
                    Yc = Yc + YRWheels(NAC + IextraAC, I1)
                Next

                Xc = Xc / 4 : Yc = Yc / 4

                For I1 = 1 To 4
                    XRWheels(NAC + IextraAC, I1) = XRWheels(NAC + IextraAC, I1) - Xc
                    YRWheels(NAC + IextraAC, I1) = YRWheels(NAC + IextraAC, I1) - Yc
                Next I1

            ElseIf AC(LI).libGear = "WFBN" Then

                For I = 1 To 4
                    XRWheels(IA, I) = AC(LI).libTX(I)
                    YRWheels(IA, I) = AC(LI).libTY(I)
                    KeepForRigid(IA, I) = True
                Next I

                Xc = 0 : Yc = 0
                For I1 = 1 To 4
                    Xc = Xc + XRWheels(IA, I1)
                    Yc = Yc + YRWheels(IA, I1)
                Next

                Xc = Xc / 4 : Yc = Yc / 4

                For I1 = 1 To 4
                    XRWheels(IA, I1) = XRWheels(IA, I1) - Xc
                    YRWheels(IA, I1) = YRWheels(IA, I1) - Yc
                Next I1

                For I = 1 To 6
                    XRWheels(NAC + IextraAC, I) = AC(LI).libTX(I + 4)
                    YRWheels(NAC + IextraAC, I) = AC(LI).libTY(I + 4)
                    KeepForRigid(NAC + IextraAC, I) = True
                Next I

                Xc = 0 : Yc = 0
                For I1 = 1 To 6
                    Xc = Xc + XRWheels(NAC + IextraAC, I1)
                    Yc = Yc + YRWheels(NAC + IextraAC, I1)
                Next

                Xc = Xc / 6 : Yc = Yc / 6

                For I1 = 1 To 6
                    XRWheels(NAC + IextraAC, I1) = XRWheels(NAC + IextraAC, I1) - Xc
                    YRWheels(NAC + IextraAC, I1) = YRWheels(NAC + IextraAC, I1) - Yc
                Next I1

            Else

                For I = 1 To NWheels(IA)
                    XRWheels(IA, I) = AC(LI).libTX(I)
                    YRWheels(IA, I) = AC(LI).libTY(I)
                    KeepForRigid(IA, I) = True
                Next I

            End If
            '""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""""




            ' Must identify all of the wheels to be removed before
            ' assembling the list of wheels to keep.

            '11111111111111111111111111111111111111111111111111111111111111111111111111111
            If AC(LI).libGear = "WFBF" Then

                For I = 1 To 4
                    For J = I + 1 To 4
                        If Math.Abs(XRWheels(IA, J) - XRWheels(IA, I)) <= TireWidth(IA) / 2 And
                           Math.Abs(YRWheels(IA, J) - YRWheels(IA, I)) <= 72 Then
                            ' 72 inches tandem spacing cutoff is from LEDFAA.
                            KeepForRigid(IA, J) = False
                        End If
                    Next J
                Next I

                For I = 1 To 4
                    For J = I + 1 To 4
                        If Math.Abs(XRWheels(NAC + IextraAC, J) - XRWheels(NAC + IextraAC, I)) <= TireWidth(IA) / 2 And
                           Math.Abs(YRWheels(NAC + IextraAC, J) - YRWheels(NAC + IextraAC, I)) <= 72 Then
                            ' 72 inches tandem spacing cutoff is from LEDFAA.
                            KeepForRigid(NAC + IextraAC, J) = False
                        End If
                    Next J
                Next I

            ElseIf AC(LI).libGear = "WFBN" Then

                For I = 1 To 4
                    For J = I + 1 To 4
                        If Math.Abs(XRWheels(IA, J) - XRWheels(IA, I)) <= TireWidth(IA) / 2 And
                           Math.Abs(YRWheels(IA, J) - YRWheels(IA, I)) <= 72 Then
                            ' 72 inches tandem spacing cutoff is from LEDFAA.
                            KeepForRigid(IA, J) = False
                        End If
                    Next J
                Next I

                For I = 1 To 6
                    For J = I + 1 To 6
                        If Math.Abs(XRWheels(NAC + IextraAC, J) - XRWheels(NAC + IextraAC, I)) <= TireWidth(IA) / 2 And
                           Math.Abs(YRWheels(NAC + IextraAC, J) - YRWheels(NAC + IextraAC, I)) <= 72 Then
                            ' 72 inches tandem spacing cutoff is from LEDFAA.
                            KeepForRigid(NAC + IextraAC, J) = False
                        End If
                    Next J
                Next I

            Else

                For I = 1 To NWheels(IA)
                    For J = I + 1 To NWheels(IA)
                        If Math.Abs(XRWheels(IA, J) - XRWheels(IA, I)) <= TireWidth(IA) / 2 And
                           Math.Abs(YRWheels(IA, J) - YRWheels(IA, I)) <= 72 Then
                            ' 72 inches tandem spacing cutoff is from LEDFAA.
                            KeepForRigid(IA, J) = False
                        End If
                    Next J
                Next I

            End If

            '000000000000000000000000000000000000000000000000000000000000000000000
            NRWheels(IA) = 0
            If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
                NRWheels(NAC + IextraAC) = 0
            End If

            If AC(LI).libGear = "WFBF" Then

                For I = 1 To 4
                    If KeepForRigid(IA, I) Then
                        NRWheels(IA) = NRWheels(IA) + 1
                        XRWheels(IA, NRWheels(IA)) = XRWheels(IA, I) ' Don't need these for CtoP, just for record.
                        YRWheels(IA, NRWheels(IA)) = YRWheels(IA, I)
                    End If
                Next I

                For I = 1 To 4
                    If KeepForRigid(NAC + IextraAC, I) Then
                        NRWheels(NAC + IextraAC) = NRWheels(NAC + IextraAC) + 1
                        XRWheels(NAC + IextraAC, NRWheels(NAC + IextraAC)) = XRWheels(NAC + IextraAC, I) ' Don't need these for CtoP, just for record.
                        YRWheels(NAC + IextraAC, NRWheels(NAC + IextraAC)) = YRWheels(NAC + IextraAC, I)
                    End If
                Next I

            ElseIf AC(LI).libGear = "WFBN" Then

                For I = 1 To 4
                    If KeepForRigid(IA, I) Then
                        NRWheels(IA) = NRWheels(IA) + 1
                        XRWheels(IA, NRWheels(IA)) = XRWheels(IA, I) ' Don't need these for CtoP, just for record.
                        YRWheels(IA, NRWheels(IA)) = YRWheels(IA, I)
                    End If
                Next I

                For I = 1 To 6
                    If KeepForRigid(NAC + IextraAC, I) Then
                        NRWheels(NAC + IextraAC) = NRWheels(NAC + IextraAC) + 1
                        XRWheels(NAC + IextraAC, NRWheels(NAC + IextraAC)) = XRWheels(NAC + IextraAC, I) ' Don't need these for CtoP, just for record.
                        YRWheels(NAC + IextraAC, NRWheels(NAC + IextraAC)) = YRWheels(NAC + IextraAC, I)
                    End If
                Next I

            Else

                For I = 1 To NWheels(IA)
                    If KeepForRigid(IA, I) Then
                        NRWheels(IA) = NRWheels(IA) + 1
                        XRWheels(IA, NRWheels(IA)) = AC(LI).libTX(I) ' Don't need these for CtoP, just for record.
                        YRWheels(IA, NRWheels(IA)) = AC(LI).libTY(I)
                    End If
                Next I

            End If

        Next IA

        Call CoverageToPassCoreRigidSingleAC(AA, CShort(extra), SigmaW, TireWidth, NRWheels, XRWheels, CtoPmaxRigid, CtoPmaxPointRigid)
        'ik001 frmGear.lblPtoCrigid.Caption = "Rigid P/C = " & Format(1 / CtoPmaxRigid, "0.00")
        IA = IA


        'FileOpen(18, "CovToPass.txt", OpenMode.Append)
        'PrintLine(18, "")

        'Dim J1, K1 As Integer

        'For J1 = 1 To NAC
        '    LI = LibIndex(J1)

        '    If AC(LI).libGear = "WFBN" Then
        '        K1 = K1 + 1

        '        For I = 1 To 41
        '            PrintLine(18, AC(LI).libGear & " " & LPad(3, CStr(I)) & " " & LPad(20, CStr(CtoP(1, I))) & " " & LPad(20, CStr(CtoP(NAC + K1, I))))
        '        Next
        '        PrintLine(18, "")
        '    Else
        '        For I = 1 To 41
        '            PrintLine(18, AC(LI).libGear & " " & LPad(3, CStr(I)) & " " & LPad(20, CStr(CtoP(1, I))))
        '        Next
        '        PrintLine(18, "")
        '    End If


        'Next


        'FileClose(18)




    End Sub

    Sub CoverageToPassCoreRigidSingleAC(ByVal AA8 As Integer, ByVal IextraAC As Short, ByVal SigmaW As Double, ByVal TireWidth() As Double,
         ByVal NWhls() As Integer, ByVal XWhls(,) As Single, ByVal CtoPmax As Double, ByVal CtoPmaxPoint As Double)

        ' Calculate coverage-to-pass ratio. Modification of the CtoPPCC
        ' subroutine in CDF.BAS from LEDFAA. Pass-to-coverage is the reciprocal of CtoP.
        ' Offset = Coordinate of point where coverage-to-pass is required.

        Dim I, IOFF, NOFF As Integer
        Dim aa, BB, AREA, YOFF As Single
        Dim Offset, OffsetInc, CtoP1 As Single
        Dim LeftMostY, RightMostY As Single
        Dim IA As Short ', IextraAC As Short

        Dim xCenter, yCenter As Single, NWheels As Short
        Dim xCenter2, yCenter2 As Single

        For IA = CShort(AA8) To CShort(AA8)
            LI = LibIndex(IA)

            'If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then ' 4-4 or 4-6 wing-body.
            '    IextraAC = IextraAC + 1S ' Add the body gear to the aircraft list.
            'End If

            If AC(LI).libGear = "WFBF" Then ' 4-4  wing-body.

                xCenter = 0.0! : yCenter = 0.0!
                For J = 1 To 4
                    xCenter = xCenter + AC(LI).libTX(J)
                    yCenter = yCenter + AC(LI).libTY(J)
                Next J
                xCenter = Math.Abs(xCenter) / 4 : yCenter = yCenter / 4

                xCenter2 = 0.0! : yCenter2 = 0.0!
                For J = 5 To 8
                    xCenter2 = xCenter2 + AC(LI).libTX(J)
                    yCenter2 = yCenter2 + AC(LI).libTY(J)
                Next J
                xCenter2 = Math.Abs(xCenter2) / 4 : yCenter2 = yCenter2 / 4

            ElseIf AC(LI).libGear = "WFBN" Then ' 4-6 wing-body.

                xCenter = 0.0! : yCenter = 0.0!
                For J = 1 To 4
                    xCenter = xCenter + AC(LI).libTX(J)
                    yCenter = yCenter + AC(LI).libTY(J)
                Next J
                xCenter = Math.Abs(xCenter) / 4 : yCenter = yCenter / 4

                xCenter2 = 0.0! : yCenter2 = 0.0!
                For J = 5 To 10
                    xCenter2 = xCenter2 + AC(LI).libTX(J)
                    yCenter2 = yCenter2 + AC(LI).libTY(J)
                Next J
                xCenter2 = Math.Abs(xCenter2) / 6 : yCenter2 = yCenter2 / 6

            Else

                LeftMostY = 1.0E+35 : RightMostY = -1.0E+35
                For I = 1 To AC(LI).libNTires
                    If XWhls(IA, I) < LeftMostY Then LeftMostY = XWhls(IA, I)
                    If XWhls(IA, I) > RightMostY Then RightMostY = XWhls(IA, I)
                Next I

                xCenter = 0.0! : yCenter = 0.0! : NWheels = AC(LI).libNTires
                For J = 1 To NWheels
                    xCenter = xCenter + AC(LI).libTX(J)
                    yCenter = yCenter + AC(LI).libTY(J)
                Next J
                xCenter = xCenter / NWheels : yCenter = yCenter / NWheels
                xCenter = xCenter + AC(LI).libTS / 2 + Math.Abs(RightMostY)

                If (InStr(4, ACName(IA), "Belly", CompareMethod.Text) > 0) _
                And Not ((InStr(1, ACName(IA), "B747", CompareMethod.Text) > 0) _
                    Or (InStr(1, ACName(IA), "A380", CompareMethod.Text) > 0)) Then
                    xCenter = 0
                End If

            End If


            NOFF = 40 : OffsetInc = 10 : Offset = 0 : CtoPmax = 0

            'Dim iCount As Integer, s1 As Char, s2 As String = "abc/adb"
            'If InStr(AC(LI).libStdNamingConv, "/", CompareMethod.Text) > 0 Then
            '    Debug.Print("Found /")
            'End If

            'If AC(LI).libStdNamingConv = " " Then
            '    iCount = 1

            '    s1 = CChar(AC(1).libStdNamingConv.Substring(AC(1).libStdNamingConv.Length - 1, 1))
            '    If Char.IsNumber(s1) Then
            '        iCount = Microsoft.VisualBasic.Val(s1)
            '    Else
            '        iCount = 1
            '    End If
            'End If

            For IOFF = 1 To NOFF + 1

                If AC(LI).libGear = "WFBF" Or AC(LI).libGear = "WFBN" Then

                    CtoP1 = 0.0!
                    For I = 1 To NWhls(IA)    ' Do for each wheel.
                        YOFF = CSng(Math.Abs(Offset - (xCenter + XWhls(IA, I))))
                        aa = CSng(YOFF - TireWidth(IA) / 2.0!)
                        BB = CSng(YOFF + TireWidth(IA) / 2.0!)
                        AREA = GaussArea(aa, BB, CSng(SigmaW))
                        CtoP1 = CtoP1 + AREA
                    Next I

                    For I = 1 To NWhls(IA)    ' Do for each wheel.
                        YOFF = CSng(Math.Abs(Offset + (xCenter + XWhls(IA, I))))
                        aa = CSng(YOFF - TireWidth(IA) / 2.0!)
                        BB = CSng(YOFF + TireWidth(IA) / 2.0!)
                        AREA = GaussArea(aa, BB, CSng(SigmaW))
                        CtoP1 = CtoP1 + AREA
                    Next I

                    CtoP(IA, IOFF) = CtoP1

                    CtoP1 = 0.0!
                    For I = 1 To NWhls(NAC + IextraAC)    ' Do for each wheel.
                        YOFF = CSng(Math.Abs(Offset - (xCenter2 + XWhls(NAC + IextraAC, I))))
                        aa = CSng(YOFF - TireWidth(IA) / 2.0!)
                        BB = CSng(YOFF + TireWidth(IA) / 2.0!)
                        AREA = GaussArea(aa, BB, CSng(SigmaW))
                        CtoP1 = CtoP1 + AREA
                    Next I

                    For I = 1 To NWhls(NAC + IextraAC)    ' Do for each wheel.
                        YOFF = CSng(Math.Abs(Offset + (xCenter2 + XWhls(NAC + IextraAC, I))))
                        aa = CSng(YOFF - TireWidth(IA) / 2.0!)
                        BB = CSng(YOFF + TireWidth(IA) / 2.0!)
                        AREA = GaussArea(aa, BB, CSng(SigmaW))
                        CtoP1 = CtoP1 + AREA
                    Next I

                    CtoP(NAC + IextraAC, IOFF) = CtoP1

                Else

                    'Dim YOFF1(2) As Single
                    CtoP1 = 0.0!
                    For I = 1 To NWhls(IA)    ' Do for each wheel.
                        YOFF = CSng(Math.Abs(Offset - (xCenter + XWhls(IA, I))))
                        'YOFF1(I) = YOFF
                        aa = CSng(YOFF - TireWidth(IA) / 2.0!)
                        BB = CSng(YOFF + TireWidth(IA) / 2.0!)
                        AREA = GaussArea(aa, BB, CSng(SigmaW))
                        CtoP1 = CtoP1 + AREA

                        'FileOpen(9, "ola555.txt", OpenMode.Append, OpenAccess.Write)
                        'Print(9, " I " & LPad(3, CStr(I)))
                        'Print(9, " XWhls(IA, I) " & LPad(10, CStr(XWhls(IA, I))) & "  xCenter " & LPad(10, CStr(xCenter)))
                        'Print(9, " 333 " & LPad(10, CStr((XWhls(IA, I) + xCenter))))
                        'Print(9, " YOFF " & LPad(10, CStr(YOFF)) & "  aa " & LPad(13, CStr(Math.Round(aa, 7))))
                        'Print(9, " BB " & LPad(13, CStr(Math.Round(BB, 7))) & "  AREA " & LPad(11, CStr(Math.Round(AREA, 7))))
                        'Print(9, " CtoP1 " & LPad(10, CStr(Math.Round(CtoP1, 7))))
                        'PrintLine(9, " ")
                        'FileClose(9)

                    Next I

                    'FileOpen(9, "ola555.txt", OpenMode.Append, OpenAccess.Write)
                    'Print(9, " Offset " & LPad(5, CStr(Offset)))
                    'Print(9, " YOFF1 " & LPad(10, CStr(YOFF1(1))))
                    'Print(9, " YOFF " & LPad(10, CStr(YOFF1(2))))
                    'PrintLine(9, " ")
                    'FileClose(9)





                    If ((InStr(4, ACName(IA), "Belly", CompareMethod.Text) = 0) And Not (AC(LI).libGear = "A")) Or
                         ACName(IA) = "IL86a" Then

                        For I = 1 To NWhls(IA)    ' Do for each wheel.
                            YOFF = CSng(Math.Abs(Offset + (xCenter + XWhls(IA, I))))
                            aa = CSng(YOFF - TireWidth(IA) / 2.0!)
                            BB = CSng(YOFF + TireWidth(IA) / 2.0!)
                            AREA = GaussArea(aa, BB, CSng(SigmaW))
                            CtoP1 = CtoP1 + AREA

                            'FileOpen(9, "ala.txt", OpenMode.Append, OpenAccess.Write)
                            'Print(9, " I " & LPad(3, CStr(I)))
                            'Print(9, " XWhls(IA, I) " & LPad(10, CStr(XWhls(IA, I))) & "  xCenter " & LPad(10, CStr(xCenter)))
                            'Print(9, " 333 " & LPad(10, CStr((XWhls(IA, I) + xCenter))))
                            'Print(9, " YOFF " & LPad(10, CStr(YOFF)) & "  aa " & LPad(13, CStr(Math.Round(aa, 7))))
                            'Print(9, " BB " & LPad(13, CStr(Math.Round(BB, 7))) & "  AREA " & LPad(11, CStr(Math.Round(AREA, 7))))
                            'Print(9, " CtoP1 " & LPad(10, CStr(Math.Round(CtoP1, 7))))
                            'PrintLine(9, " ")
                            'FileClose(9)

                        Next I

                    End If

                    CtoP(IA, IOFF) = CtoP1

                    If ACName(IA) = "IL86" Then
                        For I = 1 To NWhls(IA)    ' Do for each wheel.
                            YOFF = CSng(Math.Abs(Offset + (0 + XWhls(IA, I))))
                            aa = CSng(YOFF - TireWidth(IA) / 2.0!)
                            BB = CSng(YOFF + TireWidth(IA) / 2.0!)
                            AREA = GaussArea(aa, BB, CSng(SigmaW))
                            CtoP1 = CtoP1 + AREA
                        Next I

                        'FileOpen(9, "xxxxx.txt", OpenMode.Append, OpenAccess.Write)
                        'Print(9, " I " & LPad(3, CStr(I)))
                        'Print(9, " XWhls(IA, I) " & LPad(10, CStr(XWhls(IA, I))) & "  xCenter " & LPad(10, CStr(xCenter)))
                        'Print(9, " 333 " & LPad(10, CStr((XWhls(IA, I) + xCenter))))
                        'Print(9, " YOFF " & LPad(10, CStr(YOFF)) & "  aa " & LPad(13, CStr(Math.Round(aa, 7))))
                        'Print(9, " BB " & LPad(13, CStr(Math.Round(BB, 7))) & "  AREA " & LPad(11, CStr(Math.Round(AREA, 7))))
                        'Print(9, " CtoP1 " & LPad(10, CStr(Math.Round(CtoP1, 7))))
                        'PrintLine(9, " ")
                        'FileClose(9)

                        'CtoP(IA, IOFF) = CtoP(IA, IOFF) + CtoP1
                        CtoP(IA, IOFF) = CtoP1
                    End If

                End If


                If CtoP1 > CtoPmax Then
                    CtoPmax = CtoP1
                    CtoPmaxPoint = Offset
                End If
                Offset = Offset + OffsetInc

            Next IOFF

        Next IA






    End Sub

    Sub CoverageToPassRigid13(ByRef IA As Short, ByVal GearNumber As Integer, ByVal OFFSET As Single, ByRef CovToPass As Single)

        Static SigmaW, TireWidth() As Single, I As Integer
        Static NFWheels() As Integer, XFWheels(,), YFWheels(,) As Single
        Static NGWheels() As Integer, XGWheels(,), YGWheels(,) As Single
        Static Keep(,) As Boolean, BottomIndex(,) As Integer
        Static NumOfTires As Integer
        Dim shift As Short, k As Integer
        Dim XRWheels(,) As Single, YRWheels(,) As Single
        Dim KeepForRigid(,) As Boolean

        If OFFSET > 0.0! Then GoTo CPTOTAL1 ' at very end of sub.

        ReDim XFWheels(NAC * 2, MaxNTires * 2), YFWheels(NAC * 2, MaxNTires * 2)
        ReDim NFWheels(NAC * 2), TireWidth(NAC * 2), Keep(NAC * 2, MaxNTires * 2)
        ReDim XRWheels(NAC * 2, MaxNTires * 2), YRWheels(NAC * 2, MaxNTires * 2), KeepForRigid(NAC * 2, MaxNTires * 2)

        ReDim NGWheels(NAC * 2)
        ReDim XGWheels(NAC * 2, MaxNTires * 2)
        ReDim YGWheels(NAC * 2, MaxNTires * 2)
        ReDim BottomIndex(NAC * 2, MaxNTires * 2)


        If gPtoC1 Then 'Rigid13
            SigmaW = 0
        Else
            SigmaW = 30.435          ' Wander width of 70 inches.
        End If


        LI = LibIndex(IA)
        TireWidth(IA) = WT(IA)
        Dim check As Single
        check = TireWidth(1)
        check = TireWidth(2)
        Dim check2 As Integer


        'NumOfTires = AC(LI).libWheelsInGroup(GearNumber)

        k = AC(LI).libNTires1
        If GearNumber = 1 Then
            NumOfTires = AC(LI).libNTires
            shift = 0
            For I = 1 To NumOfTires
                XRWheels(IA, I) = AC(LI).libTX(I)
                YRWheels(IA, I) = AC(LI).libTY(I)
                KeepForRigid(IA, I) = True
            Next I
        Else
            NumOfTires = AC(LI).libNTires2 * 2
            shift = CShort(AC(LI).libNTires1 * 2)
        End If
        multiCalculator = 0
        If NumOfTires >= 4 Then
            For I = 1 To NumOfTires
                For J = I + 1 To NumOfTires
                    If Math.Abs(XRWheels(IA, J) - XRWheels(IA, I)) <= TireWidth(IA) / 2 And
                           Math.Abs((YRWheels(IA, J)) - (YRWheels(IA, I))) <= 72 Then
                        ' 72 inches tandem spacing cutoff is from LEDFAA.
                        KeepForRigid(IA, J) = False

                    End If
                Next J
            Next I
        Else

        End If

        For I = 1 To NumOfTires
            If KeepForRigid(IA, I) Then
                multiCalculator = multiCalculator + 1
            End If
        Next I



        If multiCalculator > 1 Then
            multiCalculator = multiCalculator / 2
        End If




        'XGWheels, YGWheels coordinates of tires only for one gear
        'on both sides of airplane
        For J = 1 + shift To NumOfTires + shift
            XGWheels(IA, J - shift) = AC(LI).libTX(J)
            YGWheels(IA, J - shift) = AC(LI).libTY(J)
        Next J

        'sort tandem wheels according y coordinate to determine bottom wheels
        Dim Loop2 As Integer, Switch1 As Boolean, TempX, TempY As Single


        For I = 1 To NumOfTires

            Do
                Switch1 = False
                For Loop2 = 1 To NumOfTires - 1

                    If YGWheels(IA, Loop2) > YGWheels(IA, Loop2 + 1) Then

                        TempX = XGWheels(IA, Loop2)
                        XGWheels(IA, Loop2) = XGWheels(IA, Loop2 + 1)
                        XGWheels(IA, Loop2 + 1) = TempX

                        TempY = YGWheels(IA, Loop2)
                        YGWheels(IA, Loop2) = YGWheels(IA, Loop2 + 1)
                        YGWheels(IA, Loop2 + 1) = TempY

                        Switch1 = True
                        'Limit2 = Loop2
                    End If

                Next Loop2
            Loop While Switch1

        Next

        Dim BottomWheels(NAC * 2, MaxNTires * 2) As Boolean
        Dim TDmultiplier(NumOfTires) As Single

        For I = 1 To NumOfTires
            TDmultiplier(I) = 1
            BottomWheels(IA, I) = False
            BottomIndex(IA, J) = 0
        Next

        'Locate bottom wheels - 1st iteration
        For I = 1 To NumOfTires
            For J = I + 1 To NumOfTires

                If Math.Abs(XGWheels(IA, J) - XGWheels(IA, I)) <= TireWidth(IA) / 2 Then

                    If BottomIndex(IA, I) > 0 Then
                        If Math.Abs(XGWheels(IA, J) - XGWheels(IA, BottomIndex(IA, J))) <= TireWidth(IA) / 2 Then
                            BottomWheels(IA, I) = False
                            BottomIndex(IA, J) = I
                        Else
                            BottomWheels(IA, J) = True
                            BottomIndex(IA, J) = 0
                        End If
                    Else
                        BottomWheels(IA, I) = True
                        BottomWheels(IA, J) = False
                        BottomIndex(IA, J) = I
                    End If

                End If
            Next J
        Next I

        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then Continue For
            If BottomIndex(IA, I) > 0 Then Continue For

            BottomWheels(IA, I) = True
        Next I


        'For bottom wheels determine wheels in the columns
        'limit1 - # of wheels in column

        Dim ww(NumOfTires, k) As Integer
        Dim yValues(NumOfTires, k) As Single
        Dim i1 As Integer, limit1(NumOfTires) As Integer

        'repeat
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                i1 = 0

                For J = 1 To NumOfTires
                    If I <> J Then
                        If Math.Abs(XGWheels(IA, I) - XGWheels(IA, J)) <= TW(IA) / 2 Then

                            i1 = i1 + 1
                            ww(I, i1) = J
                            limit1(I) = i1
                            yValues(I, i1) = YGWheels(IA, J)
                        End If
                    End If
                Next

            End If 'BottomWheels(I) = True
        Next

        'sort tandem wheels according y coordinate to calculate tandem distance
        Dim Limit, Loop1 As Integer, Switch As Boolean, TempVal As Single

        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                Limit = limit1(I) - 1

                Do
                    Switch = False
                    For Loop1 = 1 To Limit

                        If yValues(I, Loop1) > yValues(I, Loop1 + 1) Then

                            TempVal = yValues(I, Loop1)
                            yValues(I, Loop1) = yValues(I, Loop1 + 1)
                            yValues(I, Loop1 + 1) = TempVal

                            TempVal = ww(I, Loop1)
                            ww(I, Loop1) = ww(I, Loop1 + 1)
                            ww(I, Loop1 + 1) = CInt(TempVal)

                            Switch = True
                            Limit = Loop1
                        End If

                    Next Loop1
                Loop While Switch

            End If 'BottomWheels(I) = True
        Next


        'calculate tandem distance
        Dim tdValues(NumOfTires, k) As Single
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                For J = 1 To limit1(I)

                    If J = 1 Then
                        tdValues(I, J) = yValues(I, J) - YGWheels(IA, I)
                    Else
                        tdValues(I, J) = yValues(I, J) - yValues(I, J - 1)
                    End If

                Next
            End If 'BottomWheels(I) = True
        Next

        'calculate tandem multiplier
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                For J = 1 To limit1(I)
                    'AliA
                    'If (tdValues(I, J) - TW(IA)) < 72 Then
                    If tdValues(I, J) < 72 Then
                        'no change
                    Else
                        TDmultiplier(I) = TDmultiplier(I) + 1
                    End If

                Next
            End If 'BottomWheels(I) = True
        Next


        Static TDnumber() As Single
        'Locate the most south row of wheels in seperate arrays
        NFWheels(IA) = 0
        For I = 1 To NumOfTires
            If BottomWheels(IA, I) = True Then
                NFWheels(IA) = NFWheels(IA) + 1S
                ReDim Preserve TDnumber(NFWheels(IA))

                XFWheels(IA, NFWheels(IA)) = XGWheels(IA, I)
                YFWheels(IA, NFWheels(IA)) = YGWheels(IA, I)

                TDnumber(NFWheels(IA)) = TDmultiplier(I)
                Keep(IA, I) = True
            Else
                Keep(IA, I) = False
            End If
        Next I

        'make sure X coordinates increase from left to rigth
        For I = 1 To NFWheels(IA)

            Limit = NFWheels(IA) - 1
            Do
                Switch = False
                For Loop1 = 1 To Limit

                    If XFWheels(IA, Loop1) > XFWheels(IA, Loop1 + 1) Then

                        TempVal = XFWheels(IA, Loop1)
                        XFWheels(IA, Loop1) = XFWheels(IA, Loop1 + 1)
                        XFWheels(IA, Loop1 + 1) = TempVal

                        TempVal = TDnumber(Loop1)
                        TDnumber(Loop1) = TDnumber(Loop1 + 1)
                        TDnumber(Loop1 + 1) = TempVal

                        Switch = True
                        Limit = Loop1
                    End If

                Next Loop1
            Loop While Switch


        Next


CPTOTAL:  ' Jump here on all calls after the first.

        Static AREA, YOFF As Single
        Static CtoP1, TEFF As Single, GP As Single
        Static Left(), Right() As Single
        Dim middle As Single


        LI = LibIndex(IA)
        TEFF = CSng(Math.Abs(0))
        GP = TEFF + CSng(TireWidth(IA))

        ReDim Left(NFWheels(IA)), Right(NFWheels(IA))

        Left(1) = XFWheels(IA, 1) - GP / 2
        For I = 1 To NFWheels(IA) - 1
            middle = (XFWheels(IA, I) + XFWheels(IA, I + 1)) / 2

            If (XFWheels(IA, I) + GP / 2 < middle) Then
                Right(I) = XFWheels(IA, I) + GP / 2
                Left(I + 1) = XFWheels(IA, I + 1) - GP / 2
            Else
                Right(I) = middle
                Left(I + 1) = middle
            End If

        Next
        Right(NFWheels(IA)) = XFWheels(IA, NFWheels(IA)) + GP / 2


CPTOTAL1:


        If OFFSET > 0.0! Then
            Dim oo1 As Integer
            For oo1 = 1 To NFWheels(IA)
                Left(oo1) = Left(oo1) + 10
                Right(oo1) = Right(oo1) + 10
                YOFF = YOFF + 10
            Next oo1

        End If

        CtoP1 = 0.0!
        For I = 1 To NFWheels(IA)    ' Do for each wheel.
            If (Left(I) < 0 And Right(I) < 0) Then
                AREA = GaussArea(Math.Abs(Left(I)), Math.Abs(Right(I)), CSng(SigmaW))
            Else
                AREA = GaussArea(Left(I), Right(I), CSng(SigmaW))
            End If


            CtoP1 = CtoP1 + AREA * TDnumber(I)

        Next I

        CovToPass = CtoP1 '* multiCalculator


    End Sub

End Module


