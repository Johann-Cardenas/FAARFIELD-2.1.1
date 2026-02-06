

Public Module modDesignRigid_Adj
    Public sssLeaf, sssLeaf2 As Single



    Sub LeafInteriorToEdge_Adj(ByRef IA As Short, ByRef GearLoadType As String, ByRef ACTT As Double, ByRef HorizStress As Single)
        ' Convert interior stresses to equivalent 3D-FEM edge stresses.
        Dim LI As Short, ES, SubMod As Single

        SubMod = Modulus(NPLayers)
        SubMod = SubMod / 1000.0! ' Reduce number of zeros in coefficients.

        ' Ratio for DC10-30 Wing (dual tandem)
        ES = CSng(1.048 + SubMod * 0.013227 - SubMod * SubMod * 0.00018004)

        If IA <= NAC Then ' Extra aircraft don't have names. GFH 04/22/03.
            LI = LibIndex(IA)
        End If

        ' ACTT is needed for dual-tandem.
        ' Dual tandems first.
        If GearLoadType = "F" Or GearLoadType = "J" Or GearLoadType = "H" Then
            ES = CSng(ES * (1.385 - 0.0071 * ACTT))
        ElseIf GearLoadType = "A" Or GearLoadType = "B" Then  ' Single wheel.
            ES = CSng(ES * (1.4 - SubMod * 0.0086 + SubMod * SubMod * 0.0001040754))
        ElseIf GearLoadType = "D" Or GearLoadType = "HB" Then  ' Dual or belly.
            'If AC(LI).libACName = "A340-500/600" & BellyExt Then
            If (Mid(AC(LI).libACName, 1, 8) = "A340-500" Or Mid(AC(LI).libACName, 1, 8) = "A340-600") And (InStr(4, AC(LI).libACName, "Belly", CompareMethod.Text) > 0) Then
                ES = CSng(ES * (1.385 - 0.0071 * ACTT)) ' Dual-tandem belly gear.
            Else
                ' Dual belly gear. (Based on B727)
                ES = CSng(ES * (1.303 - SubMod * 0.00524 + SubMod * SubMod * 0.00006516))
            End If
        ElseIf GearLoadType = "N" Or GearLoadType = "M" Then  ' B777-200 Baseline or C-17
            ES = CSng(ES * (0.93746 + 0.00137 * SubMod + 0.0000046124 * SubMod * SubMod))
        ElseIf GearLoadType = "E" Then  ' C-130
            ES = CSng(ES * (1.26799 - 0.0042554 * SubMod + 0.0000541 * SubMod * SubMod))
        ElseIf GearLoadType = "K" Then  ' C-5
            ES = CSng(ES * (1.006 + 0.01 * SubMod - 0.0001466 * SubMod * SubMod))
        End If

        ' ES = predicted ratio of 3D-FEM edge stress to LED interior stress.
        HorizStress = 0.75! * ES * HorizStress

    End Sub



    Function pre_LifeTotal_SelectFunction(ByRef iTh As Single, ByRef iNextraACout As Short) As Single

        'B = LeafLifeTotal_NP(Th2, NextraACin)
        'B = LeafLifeTotal_NP_new(Th2, NextraACin, QQQ1, WWW1)
        'B = PCConRigidLife2014(Th2, NextraACin)

        If DesignType = UnbondOnRigid Or DesignType = PartBondOnRigid Then
            'Unbonded concrete overlay on rigid
            pre_LifeTotal_SelectFunction = pre_LifeTotal_PCConRigid2014(iTh, iNextraACout) '(1)
        Else
            'HMA overlay on rigid
            'SelectLifeFunctionB = LeafLifeTotal_NP(iTh, iNextraACout)
            pre_LifeTotal_SelectFunction = pre_LifeTotal_HMAonRigid2014(iTh, iNextraACout)
        End If

    End Function


    Public Sub pre_DesignRigid_NP()

        Static IL, I, ILoop As Short
        Static TM1 As Single
        Static CDFM1, DELT, DELCDF, LifeM1 As Single
        Static Overflow, AggErr As Boolean
        Static PcntCDFUTemp, CDFMAX As Single
        Static TimeSave1 As Integer
        Static HorizStressResponse(,) As Double
        'HorizStressResponse1(,) As Double    NIKE3D stress YGC 101613
        'HorizStressResponse2(,) As Double    LEAF stress   YGC 101613

        Static CDFtableTemp(MaxSectAC), CDFacrftMaxtableTemp(MaxSectAC) As Double ' GFH 08/13/03.
        Dim RunAM As New AMClassLib.clsAM()       'Instance of AutoMesh.
        Dim maxLoop As Integer = 5

        Dim LeafResp As LEAFClassLib.clsLEAF.LEAFoptions
        LeafResp = LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress


        Try

            ' C/P does not depend on depth for rigid. Therefore call only once.
            'Call LeafCtoPRigid() ' Leaves results in CtoP(IA, IOFF), used in CDFRigid.

            ' GoTo old_method
            If LifeComputation Then '2013.01.03
            Else
                SMin = CStr(NullDate)
            End If

            Dim extra As Short
            extra = 0

            Call OneCallToCalcCtoP_RigidType()
old_method:
            'Call CoverageToPassRigid() 'DesignRigid_NP (modFAILURE_MODEL_NP.vb)

            Debug.WriteLine("_____LeafDesignRigid_NP_____")
            Debug.WriteLine("Start at " & TimeString)

            Beeped = False : SubLayers = False : LayerSwitch = False
            If LifeComputation Then
                LayerSwitch = True : LifeStr = 0.0! : PercentCdfu = 0.0!
            End If


            CDFErr = 10.0! : ILoop = 0 : IL = 1
            NStrIterations = ILoop
            Call UpdateCurrentSectData() ' To display correct
            Call FAAModulus(AggErr, SubLayers)
            Call UM.UpdateStructure()
            Do
StartRigidLoop:
                NStrIterations = ILoop
                ILoop = ILoop + 1S

                If ILoop > maxLoop Then 'ikawa 12/01/03
                    Exit Do
                End If

                Call UpdateCurrentSectData()
                Call FAAModulus(AggErr, SubLayers)

                If AggErr = True Then
                    S = "Error computing aggregate Modulus" & NL
                    S = S & "The design has been aborted"
                    ErrorMessageForUser(S, "Rigid Pavement Design")
                    Exit Do
                End If
                Call WriteFile()
                Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
                TimeSave1 = timeGetTime
                Call Set_to_LEAF_Structure()
                'GoTo here123

                t1Thick = Now()
                'added VertStress, VertCoord for rigid compaction by YGC 112213
                'RunAM = New AMClassLib.clsAM()
                'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, _
                ' LEAStrActiveX, HorizStressResponse1, VertStress, VertCoord, AllResp, _
                ' CalcStress, DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted)
                ReDim HorizStressResponse1(NAC + NextraAC, 1)


                'added for computing larger stress between LEAF and NIKE3D by YGC 092613
                RunLEAF = New LEAFClassLib.clsLEAF()
                Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
                    LEAStrActiveX, HorizStressResponse2, AllResp)
                RunLEAF = Nothing

                Dim j1 As Integer
                'added to output both stress by LEAF and NIKE3D by YGC 101613
                ReDim HorizStressResponse(NAC + NextraAC, 1)
                'added for rigid compaction by YGC 112213
                ReDim StressTypeRigid(NAC + NextraAC)

                For j1 = 1 To NAC + NextraAC
                    HorizStressResponse2(j1, 1) = HorizStressResponse2(j1, 1) * 0.95 '1 pre_NP
                    HorizStressResponse(j1, 1) = Math.Max(HorizStressResponse1(j1, 1),
                                                          HorizStressResponse2(j1, 1))

                    'added for rigid compaction by YGC 112213
                    If HorizStressResponse1(j1, 1) > HorizStressResponse2(j1, 1) Then
                        StressTypeRigid(j1) = "NIKE3D"
                    Else
                        StressTypeRigid(j1) = "LEAF"
                    End If
                Next

                If gUserInterrupted Then Exit Sub

                RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                If DesigningStr = False Then Exit Do
                'Call LeafCDFRigid_NP(CDFMAX, Overflow, HorizStressResponse)
                Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(1), 80) '1 qqqqqqq

                'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
                If LifeComputation Then
                    Dim k1, k2 As Integer

                    For k1 = 1 To NAC + 1
                        For k2 = 1 To 41
                            CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                        Next
                    Next
                End If
                'If LifeComputation Then
                '    LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
                '    If LifeCounterForPCRRuns = 1 Then
                '        For k1 = 1 To NAC + 1
                '            For k2 = 1 To 41
                '                CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                '            Next
                '        Next
                '        For I = 1 To NAC
                '            CDFtableTemp(I) = jobCDFtable(ISect, I)
                '            CDFacrftMaxtableTemp(I) = jobCDFacrftMaxtable(ISect, I)
                '            CDFtableTemp3(I) = CDFtableTemp(I)
                '            CDFacrftMaxtableTemp3(I) = CDFacrftMaxtableTemp(I)
                '        Next
                '    End If
                'End If

                'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

                If LifeComputation Then
                    LifeM1 = Life
                    CDFPic = CDFMAX
                    CDFM1 = CDFMAX
                    PcntCDFUTemp = PercentCdfu ' Needed for set design life.

                    ' Save from last call to LeafCDFFlex for reporting in Aircraft Table.
                    ' Could multiply final CDFs by (Design Life) / Life but for RepsAnnualInc.
                    For I = 1 To NAC ' GFH 08/13/03.
                        CDFtableTemp(I) = jobCDFtable(ISect, I)
                        CDFacrftMaxtableTemp(I) = jobCDFacrftMaxtable(ISect, I)

                        If FEDFAA1.Save_NAC = 1 And CDFChecker = True Then
                            'CDFtableTemp3(I) = CDFtableTemp(I)
                            'CDFacrftMaxtableTemp3(I) = CDFacrftMaxtableTemp(I)
                            CDFChecker = False
                            'For k1 = 1 To NAC + 1
                            '    For k2 = 1 To 41
                            '        CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                            '    Next
                            'Next
                        ElseIf NAC = FEDFAA1.Save_NAC And FEDFAA1.Save_NAC <> 1 Then
                            'CDFtableTemp3(I) = CDFtableTemp(I)
                            'CDFacrftMaxtableTemp3(I) = CDFacrftMaxtableTemp(I)
                            'For k1 = 1 To NAC + 1
                            '    For k2 = 1 To 41
                            '        CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                            '    Next
                            'Next
                        End If
                    Next I
                    LifeStr = CSng(Life * 1.1)
                    Do
                        For I = 1 To CShort(NAC + NextraAC)
                            Temp1 = LifeStr
                            If 1.0! + RepsInc(I) * LifeStr < 0.0! Then
                                Temp1 = -1.0! / RepsInc(I)
                            End If
                            Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                            Reps(I) = Temp * RepsAnnual(I) * Temp1
                        Next I
                        Overflow = True ' CDF at subgrade.
                        'Call LeafCDFRigid_NP(CDFMAX, Overflow, HorizStressResponse)
                        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(1), 80) '1 qqqqqqq
                        If Overflow Then
                            'S = "PCC stresses are too low" & NL
                            'S = S & "to accurately compute life."

                            'If Not BatchMode Then
                            '    Ret = MsgBoxDQ(S, 0, "Computing Life")
                            'End If

                            Exit Do

                        ElseIf LifeM1 < 0.0001 Then
                            LifeM1 = 0.0!
                            LifeStr = 0.0!
                            Exit Do
                        End If

                        DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFMAX - CDFM1)

                        Temp = LifeStr
                        LifeStr = LifeM1 + DELT
                        LifeM1 = Temp
                        CDFM1 = CDFMAX
                    Loop Until System.Math.Abs(CDFM1 - 1.0!) < 0.01



                    PercentCdfu = PcntCDFUTemp ' Restore for set design life.
                    If AircraftLifeCDFvsDesignLife Then
                        For I = 1 To NAC ' GFH 08/13/03.
                            '         Saved above before start of Life Do loop.
                            jobCDFtable(ISect, I) = CDFtableTemp(I)
                            jobCDFacrftMaxtable(ISect, I) = CDFacrftMaxtableTemp(I)
                        Next I
                        'Else uses the CDF values set based on life to failure in last call to LeafCDFRigid.
                    End If
                    Call UM.UpdateStructure()
                    ' For I = 1 To NAC: Debug.Print ACName$(I); Reps(I); STRSH(I); TW(I):  Next I
                    ' Debug.Print "Life = "; LifeStr
                    Exit Do
                End If

                CDFPic = CDFMAX
                Call UM.UpdateStructure()
                Debug.WriteLine(ILoop & LayerSwitch & Thick(IL) & EvalDepth(1) & "CDF = " & CDFMAX)
                For I = 1 To CShort(NAC + NextraAC)
                    Debug.WriteLine(CallAC(I).ACname & Reps(I) & STRSH(I)) : Next I
                Debug.WriteLine("")
                CDFErr = CSng(System.Math.Abs(System.Math.Log(CDFMAX)))

                If CDFErr < CDFErrCntrl And SubLayers And Not LayerSwitch Then
                    LayerSwitch = True ' Used in WESModulus and ModulusThick.
                    CDFErr = 10.0! ' Used in ModulusThick. Not really needed here.
                    GoTo StartRigidLoop
                End If
                CDFExitErr = 0.005
                If CDFErr < CDFExitErr Then Exit Do

                If Thick(IL) = ThickMin(LCode(IL)) And CDFMAX < 1.0! Then
                    If NPLayers = 2 Or (DesignType = PCCOnFlex) Then
                        S = "The minimum PCC thickness" & NL
                        S = S & "has been reached. CDF = " & Format(CDFMAX, "0.000") & "."

                        If (DesignType = PCCOnFlex) Then
                            ErrorMessageForUser(S,
                            "Rigid Overlay on Flexible Pavement Design")
                        Else
                            ErrorMessageForUser(S, "Rigid Pavement Design")
                        End If

                        Exit Do
                    Else
                        If Thick(IL + 1) = ThickMin(LCode(IL + 1)) Then
                            If ReducedCrossSectionRunChecker = False Then
                                S = "The minimum layer thicknesses " & NL
                                S = S & "have been reached. CDF = " & Format(CDFMAX, "0.000") & "."
                                ErrorMessageForUser(S, "Rigid Pavement Design")

                                Designed = Now '2013.01.03
                                SMin = "Min" '2013.01.03
                            Else
                                S = "The minimum layer thicknesses " & NL
                                S = S & "for Reduced Cross Section Design " & NL
                                S = S & "have been reached. CDF = " & Format(CDFMAX, "0.000") & "."
                                ErrorMessageForUser(S, "Rigid Pavement Design")

                                Designed = Now '2013.01.03
                                SMin = "Min" '2013.01.03
                            End If

                            Windows.Forms.MessageBox.Show(S)
                            MinimumThickness = True
                            Exit Do
                        End If
                    End If
                End If

                TM1 = Thick(IL)
                CDFM1 = CSng(System.Math.Log(CDFMAX))
                DELT = CSng(Thick(IL) * 0.01)

                Thick(IL) = Thick(IL) + DELT
                Call UpdateCurrentSectData()
                Call FAAModulus(AggErr, SubLayers)

                If AggErr = True Then
                    S = "Error computing aggregate Modulus" & NL
                    S = S & "The design has been aborted"
                    ErrorMessageForUser(S, "Rigid Pavement Design")
                    Exit Do
                End If
                Call WriteFile()

                Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
                TimeSave1 = timeGetTime


                t1Thick = Now()
                'added VertStress, VertCoord for rigid compaction by YGC 112213
                'RunAM = New AMClassLib.clsAM()
                'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, _
                '  LEAStrActiveX, HorizStressResponse1, VertStress, VertCoord, AllResp, _
                '  CalcStress, DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted)
                ReDim HorizStressResponse1(NAC + NextraAC, 1)

                'for larger stress between LEAF and NIKE3D for design by YGC 101613
                RunLEAF = New LEAFClassLib.clsLEAF()
                Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
                    LEAStrActiveX, HorizStressResponse2, AllResp)
                RunLEAF = Nothing

                Dim j2 As Integer
                ReDim HorizStressResponse(NAC + NextraAC, 1)
                ReDim StressTypeRigid(NAC + NextraAC) 'for rigid compaction by YGC 112213

                For j2 = 1 To NAC + NextraAC
                    HorizStressResponse2(j2, 1) = HorizStressResponse2(j2, 1) * 0.95 '2 pre_NP
                    HorizStressResponse(j2, 1) = Math.Max(HorizStressResponse1(j2, 1),
                                                          HorizStressResponse2(j2, 1))
                    'added for rigid compaction by YGC 112213
                    If HorizStressResponse1(j2, 1) > HorizStressResponse2(j2, 1) Then
                        StressTypeRigid(j2) = "NIKE3D"
                    Else
                        StressTypeRigid(j2) = "LEAF"
                    End If
                Next


                If gUserInterrupted Then Exit Sub
                'Iter += 1

                RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                If DesigningStr = False Then Exit Do
                'Call LeafCDFRigid_NP(CDFMAX, Overflow, HorizStressResponse)
                Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(1), 80) '1 qqqqqqq
                DELCDF = CSng(System.Math.Log(CDFMAX) - CDFM1)
                System.Diagnostics.Debug.WriteLine(ILoop & LayerSwitch & Thick(IL) & EvalDepth(1) & "CDF = " & CDFMAX)
                For I = 1 To NAC : System.Diagnostics.Debug.WriteLine(ACName(I) & Reps(I) & STRSH(I)) : Next I

                If Overflow Then ' CDF not true value for load.
                    Thick(IL) = CSng(Thick(IL) * 0.5)
                    If Thick(IL) < ThickMin(LCode(IL)) Then
                        Thick(IL) = ThickMin(LCode(IL))
                        If NPLayers > 2 And (Not (DesignType = PCCOnFlex)) Then
                            Thick(IL + 1) = CSng(Thick(IL + 1) * 0.5)
                            If Thick(IL + 1) < ThickMin(LCode(IL + 1)) Then
                                Thick(IL + 1) = ThickMin(LCode(IL + 1))
                            End If
                        End If
                    End If
                    GoTo StartRigidLoop
                End If

                If DELCDF > 0.0! Then
                    If CDFMAX > 1.0! Then
                        Thick(IL) = Thick(IL) * 2.0!
                        GoTo StartRigidLoop
                    Else
                        S = "The design procedure cannot converge. In this" & NL
                        S = S & "case, the cause is that the PCC CDF increases" & NL
                        S = S & "as PCC thickness increases. It is probably due" & NL
                        S = S & "to stress increasing with increasing thickness" & NL
                        S = S & "for a structure with a thin PCC layer on a" & NL
                        S = S & "stiff subbase." & NL2
                        S = S & "Please check the structure."
                        If Not BatchMode Then
                            ErrorMessageForUser(S, "Cannot Converge")
                        End If
                        Exit Do
                    End If
                End If

                DELT = -CDFM1 * DELT / DELCDF


                If TM1 + DELT < ThickMin(LCode(IL)) Then

                    If (DELT < 0) And (Math.Abs(DELT) > TM1) Then
                        Thick(1) = CSng(Thick(1) * 0.9)
                    Else
                        Thick(IL) = ThickMin(LCode(IL))

                    End If

                    If NPLayers > 2 And (Not (DesignType = PCCOnFlex)) Then
                        Thick(IL + 1) = CSng(Thick(IL + 1) * 0.5)
                        maxLoop = maxLoop + 5
                        If Thick(IL + 1) < ThickMin(LCode(IL + 1)) Then
                            Thick(IL + 1) = ThickMin(LCode(IL + 1))
                        End If
                    End If
                Else
                    Thick(IL) = TM1 + DELT
                End If

            Loop


            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
            If Not LifeComputation Then
                Dim k1, k2, k3 As Integer

                For k1 = 1 To NAC + 1
                    For k2 = 1 To 41
                        CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                    Next
                Next
            End If
            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
            Call UM.UpdateStructure()
        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub


    Public Sub pre_DesignRigidOverlay_NP()
        'was(LeafDesignRigidOverlay_NP())

        ' SCIB = Initial SCI of the Existing Pavement.
        ' LifeE = Percentage of existing pavement life consumed when overlay is applied.

        Dim tth2, Th1, Th2, Thick0 As Single
        Dim I As Short
        Dim YA As Single
        Dim yB As Single
        Dim AggErr As Boolean
        Dim AA, BB As Single
        ' Dim Overflow as Integer was moved to overlay.bas as GLOBAL
        Dim B As Single
        Static RT As Single
        Static DS(NOFF) As Single
        Dim NextraACin As Short
        Dim TimeSave1 As Integer


        If LifeComputation Then
        Else '2013.01.03
            SMin = CStr(NullDate)
        End If


        Debug.WriteLine("------------------------------------------")
        Debug.WriteLine("     NEW RESULTS STARTED FROM HERE        ")
        Debug.WriteLine("------------------------------------------")

        If SCIB < 100 Then LifeExistPCC = 100.0!
        Ijk = 0 : SCIE = 80.0! : DThick = 2 'DefaultNsection = 16


        S = LayerType(LCode(1))
        If S = NACO Or S = NND Then
            S = LayerType(LCode(3))
            ''If S = NAgBase Or S = NAgSBase Then
            ''    SCIE = 58.0!
            ''Else
            ''    SCIE = 40.0!
            ''End If

            'Public Const NAgBase As String = "Ag Crushed"
            'Public Const NAgSBase As String = "Ag Uncrushed"
            'Public Const NStBase As String = "St (Rigid)"

            If S = NStBase Then
                SCIE = 40.0!
            Else
                SCIE = 57.0!
            End If

        End If


        LayerSwitch = True ' Required for WESModulus, always sublayer.
        AA = 5.0! : BB = 10.0! : CDFErr = 10.0!

        '   Sublayers = False
        T1min = 0.0! : T2min = 0.0! : RT = 0.0!
        OverlayLife = T1min + T2min

        S = LayerType(LCode(1))
        If S = NACO Or S = NND Then
            tth2 = ThickMin(10)
        ElseIf S = NPCCOU Then
            tth2 = ThickMin(11)
        Else
            tth2 = ThickMin(12)
        End If
        '   Calculating Coverage-To-Pass ratio

        NEvalDepths = 1 ' Uncommented, GFH 10/27/94.
        EvalDepth(1) = -Thick(1)

        Call UpdateCurrentSectData()

        Call FAAModulus(AggErr, SubLayers)

        Call OneCallToCalcCtoP_RigidType() 'DesignRigidOverlay_NP

        Call WriteFile()

        NStrIterations = 0
        Thick0 = Thick(1)
        T1min = 0.0! : T2min = 0.0! : RT = 0.0!

750:
        I = 0
        Th2 = Thick(1)

        B = pre_LifeTotal_SelectFunction(Th2, NextraACin) '(1)

        'Exit Sub 'comment_ik

        If DesigningStr = False Then GoTo BeepandEnd
        OverlayLife = T1min + T2min

        S = LayerType(LCode(1))
        'If S = NACO Or S = NND Then OverlayLife = OverlayLife + Th2 / CrackGrowthRate
        Call UM.UpdateStructure()
        If LifeComputation Then GoTo BeepandEnd

        NStrIterations = NStrIterations + 1S

        Debug.WriteLine(" LifeTotal: 750 in LeafDesignRigidOverlay")
        Debug.WriteLine("B =   " & B & "  Thick(1)=" & Thick(1) & "   AA= " & AA)
        Debug.WriteLine("Th2 = " & Th2 & "  tth2=" & tth2 & OverlayLife & Life)

        If System.Math.Abs(B) < LifeError Then GoTo 1503
        If NStrIterations > 12 Then
            GoTo 1503
        End If
        If B > 0 Then
            yB = B
            'If Abs(B) < LifeError Or Abs(Thick(1) - AA) < Thickerror Or _
            '(Th2 = tth2 And OverlayLife > Life) Then
            ' GFH 4/29/95
            If Math.Abs(B) < LifeError Or (Th2 = tth2 And OverlayLife > Life) Then
                ' GFH Required if start AA is too close to Thick(1).
                If Not (NStrIterations = 0 And System.Math.Abs(Thick(1) - AA) < ThickError) Then
                    RT = Thick(1)
                    GoTo 1502
                End If
            End If
            If Ijk <> 0 Then
                BB = Thick(1)
                Call Interpolation(AA, BB, YA + Life, yB + Life, Thick(1))
                GoTo 750
            End If
            Do
                I = I + 1S
                Th1 = Th2
                'If OverlayLife > 30 Then
                Th2 = Th1 - DThick
                'Else
                'Th2 = Th1 - CSng(0.5) * OverlayLife / CSng(20)
                'End If

                ' GUO    --------------- June 18, 1994  ----------------
                If Th2 < tth2 Then Th2 = tth2
                ' GUO    -----------------------------------------------
                julThick(1) = Th2
                EvalDepth(1) = -julThick(1) ' 4/20/97
                yB = B
                Thick(1) = Th2


                B = pre_LifeTotal_SelectFunction(Th2, NextraACin) '(2)


                If DesigningStr = False Then GoTo BeepandEnd ' GFH 04/10/03.
                OverlayLife = T1min + T2min
                S = LayerType(LCode(1))
                If S = NACO Or S = NND Then
                    'OverlayLife = OverlayLife + Th2 / CrackGrowthRate
                    OverlayLife = OverlayLife
                End If




                NStrIterations = NStrIterations + 1S
                Call UM.UpdateStructure()
                If Not DesigningStr Then GoTo BeepandEnd
                If (Th2 = tth2 And OverlayLife > Life) Then GoTo 1503
                If I > Ncontrol2 Then GoTo 1301 'Not convergent 'ikawa added line
            Loop Until B < 0
            AA = Th2
            BB = Th1
            YA = B
        Else
            YA = B
            If System.Math.Abs(B) < LifeError Then ' GFH 4/29/95 Or Abs(AA - BB) < Thickerror Then
                RT = Thick(1)
                GoTo 1502
            End If
            If Ijk <> 0 Then
                AA = Thick(1)
                Call Interpolation(AA, BB, YA + Life, yB + Life, Thick(1))
                GoTo 750
            End If
            Do
                I = I + 1S
                Th1 = Th2
                'If OverlayLife < 10 Then
                Th2 = Th1 + DThick
                'Else
                'Th2 = Th1 + CSng(0.6 * 20) / OverlayLife
                'Th2 = Th1 + CSng(1.5 * (20 - OverlayLife) / OverlayLife)
                'End If
                'check1000
                julThick(1) = Th2
                EvalDepth(1) = -julThick(1) ' 4/20/97
                YA = B
                Call WriteFile()
                Thick(1) = Th2


                B = pre_LifeTotal_SelectFunction(Th2, NextraACin) '(3)

                If DesigningStr = False Then GoTo BeepandEnd ' GFH 04/10/03.
                OverlayLife = T1min + T2min

                S = LayerType(LCode(1))
                'If S = NACO Or S = NND Then OverlayLife = OverlayLife + Th2 / CrackGrowthRate
                NStrIterations = NStrIterations + 1S
                S = LayerType(LCode(1))
                If I > Ncontrol2 Then GoTo 1301 'Not convergent
            Loop Until B > 0
            AA = Th1
            BB = Th2
            yB = B
        End If
        Ijk = 1
        Call Interpolation(AA, BB, YA + Life, yB + Life, Thick(1))
        GoTo 750
1301:
        Debug.WriteLine("The design has not converged after Ncontrol iterations")
        S = "The design has not converged after" & NL
        S = S & Format(Ncontrol2, "0") & " iterations." & NL2
        S = S & "Terminating the design."
        If Not BatchMode Then
            'Call frmStructure.DefInstance.BeepTmrMsg(S, "Overlay on Rigid Design")
        End If
        GoTo BeepandEnd

1503:
        RT = Th2
1502:
        OverlayLife = T1min + T2min
        S = LayerType(LCode(1))
        'If S = NACO Or S = NND Then OverlayLife = OverlayLife + RT / CrackGrowthRate
        Debug.WriteLine(" The Designed Thickness of Overlay =  " & RT)
        Debug.WriteLine(" The Design Life = " & OverlayLife)

        If RT = tth2 Then
            S = "Minimum thickness has been reached." & NL
            S = S & "The calculated life is still greater than" & NL
            S = S & "the required design life." & NL2
            S = S & "The design has been terminated."

            Designed = Now '2013.01.03
            SMin = "Min" '2013.01.03

            If BatchMode Then

            Else
                'Call frmStructure.DefInstance.BeepTmrMsg(S, "Overlay on Rigid Design")
            End If

            'If Not BatchRunning Then 'ikawa added
            '    Call frmStructure.DefInstance.BeepTmrMsg(S, "Overlay on Rigid Design")
            'End If

            GoTo BeepandEnd
        End If


BeepandEnd:



        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
        If Not LifeComputation Then
            Dim k1, k2, k3 As Integer

            For k1 = 1 To NAC + 1
                For k2 = 1 To 41
                    CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                Next
            Next
        End If
        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf


        If NoOutFiles = False Then
            ' Write response data to file.
            TimeSave1 = timeGetTime
            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            Call WriteLEAFData(LEAFOutputText)
            SS = WorkingDir & "FAASR3DPCC.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFOutputText)
            FileClose((I))
        End If

        'added for rigid compaction by YGC 112213
        If LifeComputation And gUseCompaction Then 'ikawa 2013
            Call WriteLEAFCompactionData(LEAFCompactionText)


            'CompactionDesigned = Now    'added to record time for compaction design by YGC 102213

            SS = WorkingDir & "Compaction.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFCompactionText)
            FileClose((I))
        End If
        'added for rigid compaction by YGC 112213 END


        ' If Not Beeped Then Beep ' GFH 04/23/03
        ' moved to frmStructure.cmdAddDelete.

    End Sub


    Function pre_LifeTotal_PCConRigid2014(ByRef Th As Single, ByRef NextraACout As Short) As Single

        Dim OVER1(1, 1), PCC1(1, 1) As Single
        NSection = DefaultNSection
        'Nsection = 16
        ReDim gPCC_Surface_Mod(NSection)
        ReDim gPCC_SCI(NSection)

        ReDim gPCC_NIKE3D_Stress(NSection)
        ReDim gPCC_LEAF_Stress(NSection)
        'ReDim gPCC_USED_Stress(NSection)

        ReDim gOverlay_NIKE3D_Stress(NSection, MaxSectAC)
        ReDim gOverlay_LEAF_Stress(NSection, MaxSectAC)
        ReDim gOverlay_USED_Stress(NSection, MaxSectAC)

        Dim RepsIncZero As Boolean


        Dim modPCCLayer(NSection) As Double
        Dim stressOverlay2Layer(NSection) As Double
        Dim Overlay_LifeSCI80, OverLife2(NSection) As Single
        Dim PCCSurface_Life1crack, PCCSurface_LifeSCI0 As Single
        Dim LifeUsed(NSection), ItIsOne As Single

        Dim LeafResp As LEAFClassLib.clsLEAF.LEAFoptions
        LeafResp = LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress

        Dim CDFMAX As Single
        'Dim CDFMAX As Double
        Dim E0 As Single
        Dim Overflow, AggErr As Boolean, I, II, k As Short, j2 As Integer
        ' Call LEDFAA_to_LEAF_Rigid to init CallAC(), LEAStrActiveX, NextraAC
        '   NextraAC accounts for extra aircraft such as the A380 
        '   (wing is normal list, body is extra aircraft).
        Dim Response(,), HorStressResp(,) As Double
        Dim STRS1(MaxSectAC, 2) As Single
        Dim TimeSave1 As Integer
        Dim LifePassed As Single

        NSection = DefaultNSection : NextraAC = 0 : E0 = 4000000 '4 mln

        RepsIncZero = True
        For I = 1 To NAC
            If Not RepsInc(I) = 0 Then 'check if b=0
                RepsIncZero = False : Exit For
            End If
        Next

        'NextraAC = 0
        CCC = 0
        julThick(1) = Th
        Thick(1) = Th
        'E0 = 4000000 '4 mln

        EvalDepth(1) = -Thick(1) : EvalDepth(2) = -Thick(1) - Thick(2)

        TimeSave1 = timeGetTime
        t1Thick = Now()

        Dim deltaSCI, SCI_middle(NSection), SCI_end(NSection) As Single
        Dim startLife(NSection), deltaLife(NSection) As Single

        deltaSCI = SCIB / NSection : SCI_end(0) = SCIB
        For I = 1 To NSection
            SCI_middle(I) = SCIB - deltaSCI / 2 - (I - 1) * deltaSCI
            SCI_end(I) = SCIB - I * deltaSCI
        Next

        T1min = 0.0! : T2min = 0.0!

        'GoTo kkk1
        Call UpdateCurrentSectData()
        Call FAAModulus(AggErr, SubLayers)

        julModulus(2) = E0 '4 mln
        Call WriteFile()
        Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)



        '11111111111111111111111111111111111111111111111111111111111111111111111111111111
        '********** Calculate stresses in overlay and PCC surface layers ****************
        '********** Overlay modulus = 4 mln  ********************************************
        '********** PCC surface modulus 4 mln *******************************************
        '********************************************************************************

        'Dim RunAM As New AMClassLib.clsAM()   'Instance of AutoMesh. (1)
        'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX _
        '        , HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress, _
        '        DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted)
        ReDim HorizStressResponse1(NAC + NextraAC, 2) 'temporarily (to be commented)

        ReDim HorizStressResponse2(NAC + NextraAC, 2)
        ReDim HorizStressResponse3(NAC + NextraAC, 2)

        RunLEAF = New LEAFClassLib.clsLEAF()
        LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1))
        LEAStrActiveX.EvalLayer = 1
        Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
            LEAStrActiveX, HorizStressResponse2, AllResp)
        RunLEAF = Nothing

        RunLEAF = New LEAFClassLib.clsLEAF()
        LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1) + Thick(2))
        LEAStrActiveX.EvalLayer = 2
        Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
            LEAStrActiveX, HorizStressResponse3, AllResp)
        RunLEAF = Nothing

        ReDim Response(NAC + NextraAC, 2)
        'For j2 = 1 To NAC + NextraAC 'ONLY NIKE3D
        '    Response(j2, 1) = HorizStressResponse1(j2, 1)
        '    Response(j2, 2) = HorizStressResponse1(j2, 2)
        'Next

        For j2 = 1 To NAC + NextraAC 'Response(j2, 1)- overlay   Response(j2, 2) - PCC surface
            HorizStressResponse2(j2, 1) = HorizStressResponse2(j2, 1) * 0.95 '1 pre_OV
            HorizStressResponse3(j2, 1) = HorizStressResponse3(j2, 1) * 0.95 '1 pre_OV


            Response(j2, 1) = Math.Max(HorizStressResponse1(j2, 1), HorizStressResponse2(j2, 1))
            Response(j2, 2) = Math.Max(HorizStressResponse1(j2, 2), HorizStressResponse3(j2, 1))

            gOverlay_NIKE3D_Stress(j2, 0) = CSng(HorizStressResponse1(j2, 1))
            gOverlay_LEAF_Stress(j2, 0) = CSng(HorizStressResponse2(j2, 1))
            gOverlay_USED_Stress(j2, 0) = CSng(Response(j2, 1))

            gPCC_NIKE3D_Stress(j2) = CSng(HorizStressResponse1(j2, 2))
            gPCC_LEAF_Stress(j2) = CSng(HorizStressResponse3(j2, 1))
            'gPCC_USED_Stress(j2) = CSng(Response(j2, 2))
        Next

        RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1) '02
        If gUserInterrupted Then Exit Function

        ReDim HorStressResp(NAC + NextraAC, 1)
        For II = 1 To CShort(NAC + NextraAC)
            OverlayStress(II) = Response(II, 1)
            PCCStress(II) = Response(II, 2)
            HorStressResp(II, 1) = Response(II, 1) 'for overlay to be used in LeafCDFRigid
        Next II


        OVER1(1, 1) = CSng(HorStressResp(1, 1))
        'Calculate CDF for Rigid Overlay Layer for SCI=80
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(1), 80) '1
        'LeafCDFRigid sets STRSH() = Max HorizStressResponse().
        Overlay_LifeSCI80 = CSng(Life / CDFMAX) '001
        If Not RepsIncZero Then
            Overlay_LifeSCI80 = FindLife(0, Overlay_LifeSCI80, CDFMAX,
                                Overflow, HorStressResp, RCon(1), 80)
        End If


        ReDim HorStressResp(NAC + NextraAC, 1)
        For II = 1 To CShort(NAC + NextraAC)
            HorStressResp(II, 1) = Response(II, 2) 'for PCC surface to be used in LeafCDFRigid
        Next II

        PCC1(1, 1) = CSng(HorStressResp(1, 1))
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(2), 100) '2
        PCCSurface_Life1crack = Life / CSng(CDFMAX)

        If Not RepsIncZero Then
            PCCSurface_Life1crack = FindLife(0, PCCSurface_Life1crack, CDFMAX,
                                   Overflow, HorStressResp, RCon(2), 100)
        End If

        'PCC Surface Layer - CDF for SCI = 0
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(2), 0) '2
        PCCSurface_LifeSCI0 = CSng(Life / CDFMAX) '003
        If Not RepsIncZero Then
            PCCSurface_LifeSCI0 = FindLife(0, PCCSurface_LifeSCI0, CDFMAX,
                                 Overflow, HorStressResp, RCon(2), 0)
        End If
        ' CurrentSectionView.Section.PercentCdfu = LifeExistPCC

        If PCCSurface_Life1crack * (100 - LifeExistPCC) * 0.01! > Overlay_LifeSCI80 Then
            T1min = Overlay_LifeSCI80
            pre_LifeTotal_PCConRigid2014 = T1min - Life
            CCC = 9
            t1Thick = Now()
            GoTo print1
            Exit Function
        Else
            T1min = PCCSurface_Life1crack * (100 - LifeExistPCC) * 0.01!
            LifeUsed(0) = T1min / Overlay_LifeSCI80
        End If


        'calculate life for each period for PCC surface deterioration.
        For I = 0 To NSection 'starting from SCIB
            Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(2), SCI_end(I))
            startLife(I) = CSng(Life / CDFMAX) '004
            If Not RepsIncZero Then
                startLife(I) = FindLife(0, startLife(I), CDFMAX, Overflow,
                                        HorStressResp, RCon(2), SCI_end(I))
            End If
        Next


        If SCIB = 100 And LifeExistPCC < 100 Then
            ItIsOne = LifeUsed(0)
        Else
            ItIsOne = 0
        End If

        'YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY

        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        ' for %CDFU = 100       and SCIB <= 100
        ' calculate T2min when PCC surface started to deteriorate
        ' PCC surface modulus calculated based on SCI in the middle of section
        '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        For I = 1 To CShort(NSection)
            deltaLife(I) = startLife(I) - startLife(I - 1)
        Next


        LifePassed = T1min

        '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        'kkk1:
        '        Dim Interval As Microsoft.VisualBasic.DateInterval
        '        Dim DayOfWeek As Microsoft.VisualBasic.FirstDayOfWeek
        '        Dim WeekOfYear As Microsoft.VisualBasic.FirstWeekOfYear
        '        Interval = DateInterval.Second
        '        DayOfWeek = FirstDayOfWeek.Sunday
        '        WeekOfYear = FirstWeekOfYear.Jan1


        '        Dim tStart1, tStart2, tStart3 As Date
        '        Dim tdiff As Long

        '        tStart1 = Now()

        '        Dim kkk As Single
        '        For kkk = 1040000 To 1000 Step -1000
        '            tStart2 = Now()

        '            Call UpdateCurrentSectData()
        '            Call FAAModulus(AggErr, SubLayers)
        '            julModulus(2) = kkk
        '            If julModulus(2) > E0 Then julModulus(2) = E0
        '            Call WriteFile()
        '            Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)

        '          RunAM = New AMClassLib.clsAM()
        '            Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX _
        '                    , HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress, _
        '                    DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted)

        '            tdiff = DateDiff(Interval, tStart2, Now(), DayOfWeek, WeekOfYear)
        '            FileOpen(13, WorkingDir & "\FF141" & SectName & " .txt", OpenMode.Append, , , 1024)
        '            Print(13, LPad(17, Format(julModulus(2), "##,##0.000")))
        '            Print(13, LPad(12, Format(HorizStressResponse1(1, 1), "##,##0.000")))
        '            PrintLine(13, LPad(12, format(tdiff, "##,##0.00")))
        '            FileClose(13)

        '        Next


        '        tdiff = DateDiff(Interval, tStart1, Now(), DayOfWeek, WeekOfYear)
        '        FileOpen(13, WorkingDir & "\FF141" & SectName & " .txt", OpenMode.Append, , , 1024)
        '        PrintLine(13, "")
        '        PrintLine(13, LPad(12, format(tdiff, "##,##0.00")) & " min.")
        '        PrintLine(13, LPad(12, format(tdiff / 60, "##,##0.00")) & " minutes")
        '        PrintLine(13, LPad(12, format(tdiff / 60 / 60, "##,##0.00")) & " hours")
        '        FileClose(13)

        '        Exit Function
        '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        gNSectionUsed = 0

        For I = 1 To NSection

            'calculate modulus julModulus(2) and life
            Call UpdateCurrentSectData()
            Call FAAModulus(AggErr, SubLayers)
            julModulus(2) = CSng(E0 * (0.02 + 0.0064 * SCI_middle(I) + (0.00584 * SCI_middle(I)) ^ 2))
            If julModulus(2) > E0 Then julModulus(2) = E0

            gPCC_Surface_Mod(I) = julModulus(2)
            gPCC_SCI(I) = SCI_middle(I)

            Call WriteFile()
            Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)

            '33333333333333333333333333333333333333333333333333333333333333333333333333333333
            '********** Calculate stresses in overlay layer              ********************
            '********** Overlay modulus = 4 mln  ********************************************
            '********** PCC surface modulus calculated based on SCI in mid-subsection *******
            '********************************************************************************

            'RunAM = New AMClassLib.clsAM()
            'Call RunAM.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC, LEAStrActiveX _
            '        , HorizStressResponse1, VertStress, VertCoord, AllResp, CalcStress, _
            '        DesignType, gSolverType, gSlabMeshSize, JobName, gUserInterrupted)
            ReDim HorizStressResponse1(NAC + NextraAC, 2) 'temporarily (to be commented)

            RunLEAF = New LEAFClassLib.clsLEAF()
            LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1))
            LEAStrActiveX.EvalLayer = 1
            Call RunLEAF.ComputeResponse(LeafResp, CShort(NAC + NextraAC), CallAC,
                LEAStrActiveX, HorizStressResponse2, AllResp)
            RunLEAF = Nothing

            ReDim Response(NAC + NextraAC, 1)

            For j2 = 1 To NAC + NextraAC
                HorizStressResponse2(j2, 1) = HorizStressResponse2(j2, 1) * 0.95 '2 pre_OVL

                Response(j2, 1) = Math.Max(HorizStressResponse1(j2, 1), HorizStressResponse2(j2, 1))
                gOverlay_NIKE3D_Stress(j2, I) = CSng(HorizStressResponse1(j2, 1))
                gOverlay_LEAF_Stress(j2, I) = CSng(HorizStressResponse2(j2, 1))
                gOverlay_USED_Stress(j2, I) = CSng(Response(j2, 1))
            Next


            If gUserInterrupted Then Exit Function
            ReDim HorStressResp(NAC + NextraAC, 1)
            For II = 1 To CShort(NAC + NextraAC)
                HorStressResp(II, 1) = Response(II, 1) 'for Overlay to be used in LeafCDFRigid
            Next II

            'Call LeafCDFRigid_NP(CDFMAX, Overflow, SSS, RCon(1), 80)

            'Calculate CDF for overlay at SCI=80 and changing base modulus
            Call LeafCDFRigid_2014(CDFMAX, Overflow, HorStressResp, RCon(1), 80)
            stressOverlay2Layer(I) = HorStressResp(1, 1)
            modPCCLayer(I) = julModulus(2)

            OverLife2(I) = CSng(Life / CDFMAX) '005

            If Not RepsIncZero Then
                OverLife2(I) = FindLife(LifePassed, OverLife2(I), CDFMAX,
                                    Overflow, HorStressResp, RCon(1), 80)
                LifePassed = LifePassed + deltaLife(I)
            End If





            LifeUsed(I) = deltaLife(I) / OverLife2(I)
            ItIsOne = ItIsOne + LifeUsed(I)
            If ItIsOne >= 1 Then Exit For

            If I = 1 Then
                If startLife(0) = startLife(NSection) And startLife(0) > 10000000 Then
                    'If OverLife2(1) < 1000 Then Exit For
                    Exit For
                End If
            End If
        Next I

        gNSectionUsed = I
        If gNSectionUsed > NSection Then
            gNSectionUsed = NSection
        End If

        T2min = 0 : CCC = 0
        If ItIsOne >= 1 And I <= NSection Then 'OK case

            If I = 1 Then 'overlay life < subsection 1 PCC surface life

                If LifeExistPCC < 100 Then
                    T2min = OverLife2(1) * (1 - (ItIsOne - LifeUsed(1)))
                    CCC = 7 '******************************************
                Else
                    T2min = OverLife2(1)
                    CCC = 8 '******************************************
                End If

            Else
                For k = 1 To I - 1S
                    T2min = T2min + deltaLife(k)
                Next
                'T2min = T2min + (1 - (ItIsOne - LifeUsed(I))) / LifeUsed(I) * deltaLife(I)
                T2min = T2min + (1 - (ItIsOne - LifeUsed(I))) * OverLife2(I)
                CCC = 1 '******************************************
            End If
            '----------------------------------------------------------------------
        Else 'Special conditions (life used < 1)

            If ItIsOne = 0 Then 'very long life for PCC surface
                T2min = OverLife2(1)
                CCC = 2 '******************************************

            ElseIf ItIsOne < 1 Then

                For I = 1 To NSection - 1S
                    If LifeUsed(I) = 0 And deltaLife(I) = 0 Then
                        T2min = T2min + (1 - ItIsOne) * OverLife2(I)
                        Exit For
                    ElseIf LifeUsed(I) = 0 Then
                        T2min = T2min + deltaLife(I)
                    ElseIf deltaLife(I) = 0 Then
                        CCC = 5
                    Else
                        T2min = T2min + deltaLife(I)
                        CCC = 6
                    End If
                Next

                If LifeUsed(NSection) > 0 And deltaLife(NSection) > 0 Then
                    T2min = T2min + OverLife2(NSection)
                End If
                CCC = 3 '******************************************
            Else
                CCC = 4 '******************************************
            End If

        End If


        pre_LifeTotal_PCConRigid2014 = T1min + T2min - Life


        'End Function

        t1Thick = Now()

        If gPrintPCConRigidInfo Then
        Else
            Exit Function
        End If


print1:
        '==========================================================================
        '========== Printing Results ==============================================
        '==========================================================================

        Dim uuu1 As String
        'uuu1 = CStr(Thick(1)) & "_" & CStr(Thick(2))
        uuu1 = "O" & CStr(Thick(1)) & "_S" & CStr(Thick(2))


        'FileOpen(13, WorkingDir & "\FF141debug_" & uuu1 & ".txt", OpenMode.Append, , , 1024)
        'PrintLine(13, "")
        'PrintLine(13, "SCIB = " & SCIB & "    %CDFU = " & LifeExistPCC)
        'SS = Space(11 - Format(OVER1(1, 1), "##,##0.000").Length) & Format(OVER1(1, 1), "##,##0.000")
        'Print(13, "Overlay_Stress= " & SS)
        'PrintLine(13, "   " & Thick(1))
        'SS = Space(11 - Format(PCC1(1, 1), "##,##0.000").Length) & Format(PCC1(1, 1), "##,##0.000")
        'Print(13, "    PCC_Stress= " & SS)
        'PrintLine(13, "   " & Thick(2))
        'PrintLine(13, "    Overlay_LifeSCI80= " & LPad(10, Format(Overlay_LifeSCI80, "##,##0.000")))
        'PrintLine(13, "PCCSurface_Life1crack= " & LPad(10, Format(PCCSurface_Life1crack, "##,##0.000")))
        'PrintLine(13, "  PCCSurface_LifeSCI0= " & LPad(10, Format(PCCSurface_LifeSCI0, "##,##0.000")))
        'PrintLine(13, "")


        'Dim usedSum, lifeSum As Single, yyy As Boolean

        'usedSum = LifeUsed(0) : lifeSum = 0 : yyy = False
        'Print(13, LPad(20, "startLife") & LPad(20, "deltaLife") & LPad(20, "OverLife"))
        'PrintLine(13, LPad(15, "LifeUsed") & LPad(15, "usedSum") & LPad(15, "lifeSum"))
        'Print(13, LPad(20, Format(startLife(0), "##,##0.000")))
        'PrintLine(13, LPad(49, Format(LifeUsed(0), "##,##0.000")))
        'For I = 1 To NSection
        '    usedSum = usedSum + LifeUsed(I)
        '    lifeSum = lifeSum + deltaLife(I)

        '    If startLife(I) > 1 Then
        '        Print(13, LPad(20, Format(startLife(I), "##,##0.000")))
        '    Else
        '        Print(13, LPad(20, Format(startLife(I), "##,##0.000000000000")))
        '    End If

        '    If deltaLife(I) > 1 Then
        '        Print(13, LPad(20, Format(deltaLife(I), "##,##0.000")))
        '    Else
        '        Print(13, LPad(20, Format(deltaLife(I), "##,##0.000000000000")))
        '    End If

        '    If OverLife2(I) > 1 Then
        '        Print(13, LPad(20, Format(OverLife2(I), "##,##0.000")))
        '    Else
        '        Print(13, LPad(20, Format(OverLife2(I), "##,##0.000000000000")))
        '    End If

        '    If LifeUsed(I) > 1 Then
        '        Print(13, LPad(15, Format(LifeUsed(I), "##,##0.000")))
        '    Else
        '        Print(13, LPad(15, Format(LifeUsed(I), "##,##0.000000000")))
        '    End If

        '    If usedSum > 1 Then
        '        Print(13, LPad(15, Format(usedSum, "##,##0.000")))
        '    Else
        '        Print(13, LPad(15, Format(usedSum, "##,##0.000000000")))
        '    End If

        '    If lifeSum > 1 Then
        '        PrintLine(13, LPad(15, Format(lifeSum, "##,##0.000")))
        '    Else
        '        PrintLine(13, LPad(15, Format(lifeSum, "##,##0.000000000")))
        '    End If


        '    If usedSum > 1 And Not yyy Then
        '        Dim restSum, restLife, newLife As Single
        '        restSum = 1 - (usedSum - LifeUsed(I))
        '        restLife = deltaLife(I) * restSum
        '        newLife = lifeSum - deltaLife(I) + restLife
        '        PrintLine(13, "")
        '        Print(13, "restSum= " & LPad(10, Format(restSum, "##,##0.000")))
        '        Print(13, "   restLife= " & LPad(10, Format(restLife, "##,##0.000")))
        '        PrintLine(13, "   newLife= " & LPad(10, Format(newLife, "##,##0.000")))
        '        PrintLine(13, "") : yyy = True
        '    End If

        'Next

        'PrintLine(13, "")
        'For I = 1 To NSection
        '    Print(13, LPad(9, Format(SCI_middle(I), "##,##0.000")))
        '    Print(13, LPad(15, Format(modPCCLayer(I), "##,##0.000")))
        '    Print(13, LPad(11, Format(stressOverlay2Layer(I), "##,##0.000")))
        '    PrintLine(13, LPad(11, Format(OverLife2(I), "##,##0.000")))
        'Next


        'PrintLine(13, "")
        'S = LPad(10, Format(ItIsOne, "##,##0.00000000000000"))
        'SS = Space(23 - Format(ItIsOne, "##,##0.000").Length) & S
        'PrintLine(13, "ItIsOne= " & SS)
        'SS = Space(23 - Format(T1min, "##,##0.000").Length) & LPad(10, Format(T1min, "##,##0.00000000"))
        'PrintLine(13, "T1min= " & SS)
        'SS = Space(23 - Format(T2min, "##,##0.000").Length) & LPad(10, Format(T2min, "##,##0.00000000"))
        'PrintLine(13, "T2min= " & SS)
        'PrintLine(13, "CCC= " & LPad(3, Format(CCC, "##,##0")))
        'PrintLine(13, "")
        'FileClose(13)

    End Function


    Function pre_LifeTotal_HMAonRigid2014(ByRef Th As Single, ByRef NextraACout As Short) As Single

        Dim CDFMAX, PCCSurface_Life1crack As Single
        Dim Overflow As Boolean
        Dim II As Short, TimeSave1, k1, k2 As Integer
        Dim Response(,), HorizStressResponse(,) As Double

        NextraAC = 0 ' See Sub LeafLifeTotal
        julThick(1) = Th
        Thick(1) = Th

        Call WriteFile()
        Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
        LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1) + Thick(2))
        LEAStrActiveX.EvalLayer = 2

        TimeSave1 = timeGetTime
        t1Thick = Now()

        '======================================================================
        'removed NIKE3D stress calculation
        ReDim HorizStressResponse1(NAC + NextraAC, 2)
        'ReDim HorizStressResponse2(NAC + NextraAC, 2) 'added 2015.01.12
        '======================================================================

        'added for computing larger stress between LEAF and NIKE3D for design by YGC 102213
        RunLEAF = New LEAFClassLib.clsLEAF()
        LEAStrActiveX.EvalDepth = System.Math.Abs(Thick(1) + Thick(2))
        LEAStrActiveX.EvalLayer = 2
        Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse3, AllResp)
        RunLEAF = Nothing
        'ReDim HorizStressResponse3(NAC + NextraAC, 1)


        Dim j2 As Integer
        ReDim Response(NAC + NextraAC, 2)
        For j2 = 1 To NAC + NextraAC

            HorizStressResponse3(j2, 1) = HorizStressResponse3(j2, 1) * 0.95 '3 pre_OVL
            'Response(j2, 1) = Math.Max(HorizStressResponse1(j2, 1), HorizStressResponse2(j2, 1))  'overlay
            Response(j2, 2) = Math.Max(HorizStressResponse1(j2, 2), HorizStressResponse3(j2, 1))  'underlay 
        Next
        'add ended for computing larger stress between LEAF and NIKE3D for design by YGC 102213


        ReDim HorizStressResponse(NAC + NextraAC, 1)
        For II = 1 To CShort(NAC + NextraAC)
            OverlayStress(II) = Response(II, 1)
            PCCStress(II) = Response(II, 2)
            HorizStressResponse(II, 1) = Response(II, 2)
        Next II


go123:

        RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1) '02
        ' Sets STRSH() = Max HorizStressResponse().
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(2), 100)
        PCCSurface_Life1crack = Life / CSng(CDFMAX)

        Call CompforStab(FSlope, 3)
        'FSlope = 1
        FSlopeComp = CSng((0.392 - FSlope * 0.3881) / (FSlope * 0.0039))


        If LifeExistPCC < 100 Then
            T1min = PCCSurface_Life1crack * (100 - LifeExistPCC) * 0.01!
            S = LayerType(LCode(1))
        Else
            T1min = 0.0!
        End If

        '    ********** Calculating T2min **********
        Dim LifeToB, LifeToE As Single
        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(2), SCIB)
        LifeToB = Life / CDFMAX

        For k1 = 1 To NAC + NextraAC
            For k2 = 1 To 41
                CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
            Next
        Next
        'If LifeComputation Then
        '    LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
        '    If LifeCounterForPCRRuns = 1 Then
        '        For k1 = 1 To NAC + NextraAC + 1
        '            For k2 = 1 To 41
        '                CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
        '            Next
        '        Next
        '    End If
        'End If


        Call LeafCDFRigid_2014(CDFMAX, Overflow, HorizStressResponse, RCon(2), SCIE)
        LifeToE = Life / CDFMAX
        T2min = LifeToE - LifeToB

        If PCCSurface_Life1crack > 1000000 And (T2min = 0) Then
            T2min = 100000000 'pre_LifeTotal_HMAonRigid2014
        End If

        'LeafLifeTotal_NP = T1min + T2min + Th / CrackGrowthRate - Life
        pre_LifeTotal_HMAonRigid2014 = T1min + T2min - Life
        NextraACout = CShort(NextraAC)


    End Function

    Public Sub Set_to_LEAF_Structure()
        'NewRigid, PCCOnFlex     'Call DesignRigid_NP() '3D-FEM stress   
        '                         Call pre_DesignRigid_NP()
        'UnbondOnRigid           'Call DesignRigidOverlay_NP()  '3D-FEM stress
        'FlexOnRigid             'Call DesignRigidOverlay_NP()  '3D-FEM stress
        Dim I As Integer

        If NPLayers <> LEAStructure.NLayers Then
            LEAStrActiveX.NLayers = LEAStructure.NLayers

            ReDim LEAStrActiveX.Thick(LEAStrActiveX.NLayers)
            ReDim LEAStrActiveX.Modulus(LEAStrActiveX.NLayers)
            ReDim LEAStrActiveX.Poisson(LEAStrActiveX.NLayers)
            ReDim LEAStrActiveX.InterfaceParm(LEAStrActiveX.NLayers)

            For I = 1 To LEAStructure.NLayers
                LEAStrActiveX.Thick(I) = LEAStructure.Thick(I)
                LEAStrActiveX.Modulus(I) = LEAStructure.Modulus(I)
                LEAStrActiveX.Poisson(I) = LEAStructure.Poisson(I)

                If LEAStructure.InterfaceCode(I) = 0 Then
                    LEAStrActiveX.InterfaceParm(I) = 1
                ElseIf LEAStructure.InterfaceCode(I) >= 100000 Then
                    LEAStrActiveX.InterfaceParm(I) = 0
                End If
            Next

            LEAStrActiveX.Poisson(LEAStructure.NLayers) = DefaultPoissonSGPCC
            LEAStrActiveX.NLayers = LEAStructure.NLayers

        End If

    End Sub


End Module

