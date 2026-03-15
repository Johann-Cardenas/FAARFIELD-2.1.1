Option Strict Off
Option Explicit On
'Option Strict On ' Added for compaction design by YGC 041312    'Added compaction capability to FF1.4 based on FF1.313 082012 by YGC 021913
Imports System.Windows.Forms

Public Module modStrDesign
    'Dim ggTandemFnew As Boolean = True          'ED
    'Dim gTandemFnew As Boolean = True
    Dim bool1, bool2, bool3, bool4 As Boolean 'ikawa layer minimum thicknesses
    Dim minThick As Single
    Public gRRR As Single

    Public CDFASPMAX, CDFSUBMAX As Single
    Public CDFASPMAX1 As Single
    Public CDFMAX1, CDFPic As Single
    Public ComputingAsphaltCDF As Boolean
    Public AsphaltCDFComputed As Boolean
    Public Beeped As Boolean
    Public NStrIterations As Short
    Public JuleaRetErr As String

    Public LEAFOutputText As String

    'Added compaction capability to FF1.4  based on FF1.313 082012 by YGC 021913
    Public LEAFCompactionText As String
    ' Added for output compaction in Notes function by YGC 042312
    Public DensityNCMax As Double = 100
    Public DensityNCMin As Double = 85

    'Modified to compliy with Table 3-4 of 6E by YGC 082012
    Public DensityCMax As Double = 95
    Public DensityCMin As Double = 80
    'Modify ended to compliy with Table 3-4 of 6E by YGC 082012
    Public CDFtableTemp(MaxSectAC) As Double
    Public CDFtableTemp2(MaxSectAC) As Double
    Public CDFtableTemp3(MaxSectAC) As Double
    Public CDFacrftMaxtableTemp(MaxSectAC) As Double
    Public CDFacrftMaxtableTemp2(MaxSectAC) As Double
    Public CDFacrftMaxtableTemp3(MaxSectAC) As Double
    Public CDFChecker As Boolean = False

    Public NDenLevel As Short = 4
    Public jobCompactionIntDenNCtable(MaxSects, MaxJobs, NDenLevel) As Double
    Public jobCompactionIntDenCtable(MaxSects, MaxJobs, NDenLevel) As Double ' to display Compaction Criteria for Non-Cohesive soil in Notes, YGC 042312
    Public jobCompactionIntDenDepthNCtable(MaxSects, MaxJobs, NDenLevel) As Double
    Public jobCompactionIntDenDepthCtable(MaxSects, MaxJobs, NDenLevel) As Double ' to display Compaction criteria for Cohesive soil in Notes, YGC 042312
    Public jobCompactionIntDenCriticalACNCtable(MaxSects, MaxJobs, NDenLevel) As Integer
    Public jobCompactionIntDenCriticalACCtable(MaxSects, MaxJobs, NDenLevel) As Integer 'added to find critical AC at depth of compacticon by YGC 102213
    Public CompactionIntDenNC(NDenLevel), CompactionIntDenC(NDenLevel) As Double
    Public CompactionIntDenDepthNC(NDenLevel), CompactionIntDenDepthC(NDenLevel) As Double ' Add end for output compaction in Notes function by YGC 042312
    ' Add ended for compaction capability to FF1.4  based on FF1.313 082012 by YGC 021913
    Public IDHeaviestAC As Integer     'added to find the heaviest AC for ASTM by YGC 102213 

    Public UM As Object 'UpdateManager


    Public Sub LeafDesignFlex()
        NStrIterations = 0
        gDetailedReportData.Clear()
        Call LeafDesignFlex2()
    End Sub


    Public Sub LeafDesignFlex2()
        ' The design is based on CDF at the subgrade. Therefore, all calls
        ' to Leaf during the iteration are made with one EvalDepth set
        ' at the top of the subgrade. When CDFSUB = 1 (+/- Err), a call
        ' is made to Leaf with one EvalDepth at the bottom of the asphalt
        ' and CDFASP is calculated. This cuts computation time by 40% to 50%.

        Static IL, I As Short
        Static TM1 As Single
        Static CDFM1, DELT, DELCDF As Single
        Static Overflow As Boolean
        Static MinCount As Short
        Static ILoop As Short
        Static AggErr As Boolean
        Static LifeM1 As Single
        Static lclCDFMAX As Single
        Static VertStrainReponse(,) As Double
        Static HorizStrainResponse(,) As Double
        Static TempD As Double
        Static StrainMR, StrainMC As Double
        Static IAC, NEvalPointsMax, IEval As Integer
        Static TimeSave1 As Integer
        Static Factor As Double
        ' GFH 08/13/03.
        Static CdfHma(MaxSectAC) As Single

        '  Debug.Print: Debug.Print "Start at "; time$:   Debug.Print
        Beeped = False
        ComputingAsphaltCDF = False
        AsphaltCDFComputed = False
        SubLayers = False
        LayerSwitch = False

        Dim gTandemFnewHolder As Boolean
        gTandemFnewHolder = gTandemFnew

        If LifeComputation Then
            LayerSwitch = True
            LifeStr = 0.0!
            SubLayers = True
        Else '2012.12.31
            SMin = CStr(NullDate)
        End If

        If gNewModulus_P154 Or gNewModulus_P209 Then 'ikawa
            LayerSwitch = True
            SubLayers = True
        End If


        CDFErr = 10.0!
        MinCount = 0
        ILoop = 0
        NStrIterations = ILoop
        Call UpdateCurrentSectData() ' To display correct
        'Call WESModulus(AggErr, SubLayers) ' sublayer information.
        Call FAAModulus(AggErr, SubLayers)

        Call UM.UpdateStructure()

        IL = IterLayerChosen 'NPLayers - 1 '- 1

        Do

StartFlexLoop:
            NStrIterations = ILoop
            ILoop = ILoop + 1S
            '    Debug.Print "Prelim " & Time$ & "  ";
            Call UpdateCurrentSectData()
            'Call WESModulus(AggErr, SubLayers)
            Call FAAModulus(AggErr, SubLayers)

            Call UM.UpdateStructure()

            If AggErr = True Then
                S = "Error computing aggregate Modulus" & NL
                S = S & "The design has been aborted"
                ErrorMessageForUser(S, "Flexible Pavement Design")
                Exit Do
            End If
            '    Debug.Print "EvalDepth = "; EvalDepth(1)
            '    Debug.Print julNPLayers
            For I = 1 To -julNPLayers
                System.Diagnostics.Debug.WriteLine(I & julThick(I) & julModulus(I) & julLCode(I))
            Next I

            Call WriteFile()
            Call LEDFAA_to_LEAF(DesignType)
            TimeSave1 = timeGetTime

            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa   check for flexible input ln 280
            'Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            '
            'Call RunLEAF.ComputeResponse2(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            gTandemFnew = True
            If gTandemFnew Then ' kairat replace tandem
                Call RunLEAF.ComputeResponse2(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            Else
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            End If
            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            If DesigningStr = False Then Exit Do ' Also set by JULEA error.
            '   DesigningStr is set False when an error occurs and allows
            '   closing down the design loop. LifeComputation is true when
            '   computing life instead of iterating to a design thickness.
            Overflow = True ' CDF at subgrade.
            '   Keep LeafCDFFlex separate from RunLeaf.ComputeResponse because it it used
            '   independently when computing life. Could combine WriteJuleaFile with both
            '   in a separate subroutine and use a switch on LifeComputation. But more obscure.

            'xxx

            Call LeafCDFFlex(lclCDFMAX, EvalDepth(1), Overflow, VertStrainReponse)

            Call CheckAdvisoryRequirements() 'PPPP
            minThick = Math.Max(AC_layer_min(IL), ThickMin(LCode(IL)))

            'If gBleasdaleModel And Thick(IL) > ThickMin(LCode(IL)) Then 'ikawa 2012/08/21
            If gBleasdaleModel And Thick(IL) > minThick Then 'PPPP
                'If max_STRAIN = CSng(0.001) And Not LifeComputation Then
                If lclCDFMAX < 0.3 And Not LifeComputation Then
                    Thick(IL) = CSng(0.8 * Thick(IL))


                    'If Thick(IL) < ThickMin(LCode(IL)) Then
                    '    Thick(IL) = ThickMin(LCode(IL))
                    'End If

                    If Thick(IL) < minThick Then
                        Thick(IL) = minThick
                    End If



                    Call UpdateCurrentSectData()
                    'Call WESModulus(AggErr, SubLayers)
                    Call FAAModulus(AggErr, SubLayers)

                    Call UM.UpdateStructure()

                    GoTo StartFlexLoop
                End If
            End If

            CDFSUBMAX = lclCDFMAX
            '    Debug.Print "Overflow 1 = "; Overflow
            System.Diagnostics.Debug.WriteLine(ILoop & LayerSwitch & Thick(IL) & EvalDepth(1) & "CDF = " & CDFSUBMAX)

            ' Capture iteration record for detailed report
            Try
                Dim iterRec As New clsIterationRecord()
                iterRec.IterationNumber = ILoop
                iterRec.Thickness = Thick(IL)
                iterRec.CDFMAX = CDFSUBMAX
                iterRec.CDFErr = CSng(System.Math.Abs(System.Math.Log(Math.Max(CDFSUBMAX, 0.000001))))
                iterRec.SubLayered = SubLayers
                gDetailedReportData.Iterations.Add(iterRec)
            Catch ex As Exception
            End Try
            For I = 1 To NAC
                '      Debug.Print ACName$(I); " Strain = "; STRNV(I)  '**
            Next I
            '    Debug.Print "Strain 1 = "; STRNV(1); Log(CDFSUBMAX)

            If LifeComputation Then
                SubLayers = True
                Dim k1, k2, k3 As Integer
                LifeM1 = Life
                CDFPic = CDFSUBMAX
                CDFM1 = CDFSUBMAX
                For I = 1 To NAC ' GFH 08/13/03.
                    '       Save from last call to LeafCDFFlex for reporting in Aircraft Table.
                    '       Could multiply final CDFs by (Design Life) / Life but for RepsAnnualInc.
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

                'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf


                For k1 = 1 To NAC + 1
                    For k2 = 1 To 41
                        CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                    Next
                Next
                If LifeComputation Then
                    LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
                    If LifeCounterForPCRRuns = 1 Then
                        For k1 = 1 To NAC + 1
                            For k2 = 1 To 41
                                CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                            Next
                        Next
                        For I = 1 To NAC
                            CDFtableTemp(I) = jobCDFtable(ISect, I)
                            CDFacrftMaxtableTemp(I) = jobCDFacrftMaxtable(ISect, I)
                            CDFtableTemp3(I) = CDFtableTemp(I)
                            CDFacrftMaxtableTemp3(I) = CDFacrftMaxtableTemp(I)
                        Next
                    End If
                End If

                'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

                Dim Infi_Life As Boolean = False 'ikawa 

                LifeStr = CSng(Life * 1.1)
                Do
                    For I = 1 To NAC
                        Temp1 = LifeStr
                        If 1.0! + RepsInc(I) * LifeStr < 0.0! Then
                            Temp1 = -1.0! / RepsInc(I)
                        End If
                        Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                        Reps(I) = Temp * RepsAnnual(I) * Temp1
                        gRRR = Reps(I)
                    Next I
                    Overflow = True ' CDF at subgrade.
                    Call LeafCDFFlex(CDFSUBMAX, EvalDepth(1), Overflow, VertStrainReponse)
                    If Overflow Then
                        S = "Subgrade strains are too low" & vbCrLf
                        S = S & "to accurately compute life."
                        Ret = MsgBoxDQ(S, 0, "Computing Life")
                        MinimumStrainChecker = True
                        'MessageBox.Show("Subgrade strains are too low" + vbNewLine + "to accurately compute life.")
                        Debug.WriteLine("Subgrade strains are too low to accurately compute life.")
                        OverflowExit = True
                        Exit Do
                    ElseIf LifeM1 < 0.0001 Then
                        LifeM1 = 0.0!
                        LifeStr = 0.0!
                        LowLifeExit = True
                        Exit Do
                    End If


                    'DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFSUBMAX - CDFM1)

                    If (CDFSUBMAX - CDFM1) = 0 Then 'added kawa 2013
                        'Nothing
                        Infi_Life = True
                    Else
                        DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFSUBMAX - CDFM1)
                    End If


                    Temp = LifeStr
                    LifeStr = LifeM1 + DELT
                    LifeM1 = Temp
                    CDFM1 = CDFSUBMAX
                    '        Debug.Print "Life = "; LifeM1; LifeStr; "CDF = "; CDFM1
                    'Loop Until System.Math.Abs(CDFM1 - 1.0!) < 0.001 Or (CDFSUBMAX - CDFM1) = 0
                Loop Until System.Math.Abs(CDFM1 - 1.0!) < 0.001 Or ((FEDFAA1.FF_true_ACN) And (Infi_Life))

                'End If




                If AircraftLifeCDFvsDesignLife Then
                    For I = 1 To NAC ' GFH 08/13/03.
                        '         Saved above before start of Life Do loop.
                        jobCDFtable(ISect, I) = CDFtableTemp(I)
                        jobCDFacrftMaxtable(ISect, I) = CDFacrftMaxtableTemp(I)
                    Next I
                    '     Else uses the CDF values set based on life to failure in last call to LeafCDFFlex.
                End If

                Exit Do
            End If

            CDFPic = CDFSUBMAX
            CDFErr = CSng(System.Math.Abs(System.Math.Log(CDFSUBMAX))) 'Abs(1! - CDFSUBMAX)
            If CDFErr < CDFErrCntrl And SubLayers And Not LayerSwitch Then
                LayerSwitch = True ' Used in WESModulus and ModulusThick.
                CDFErr = 10.0! ' Used in ModulusThick.
                GoTo StartFlexLoop
            End If

            If CDFErr < CDFExitErr Then Exit Do

            bool1 = Thick(IL) = ThickMin(LCode(IL)) And Thick(IL - 1) = ThickMin(LCode(IL - 1)) And CDFSUBMAX < 1.0!
            bool2 = Thick(IL) = ThickMin(LCode(IL)) And NPLayers = 3 And CDFSUBMAX < 1.0!
            bool3 = Thick(IL) = ThickMin(LCode(IL)) And DesigningP209 And CDFSUBMAX < 1.0!
            bool4 = Thick(IL) = minThick And (CDFSUBMAX <= 1) 'PPPP


            If bool1 Or bool2 Or bool3 Or bool4 Then
                If FEDFAA1.numberoferrors = False Then
                    If IL = designlayernumber Then
                        FEDFAA1.numberoferrors = True
                        If ReducedCrossSectionRunChecker = False Then
                            S = "The minimum layer thicknesses " & NL
                            S = S & "have been reached. CDF = " & Format(CDFPic, "0.000") & "."
                            ErrorMessageForUser(S, "Rigid Pavement Design")

                            Designed = Now '2013.01.03
                            SMin = "Min" '2013.01.03
                        Else
                            S = "The minimum layer thicknesses " & NL
                            S = S & "for Reduced Cross Section Design " & NL
                            S = S & "have been reached. CDF = " & Format(CDFPic, "0.000") & "."
                            ErrorMessageForUser(S, "Rigid Pavement Design")

                            Designed = Now '2013.01.03
                            SMin = "Min" '2013.01.03
                        End If

                        Windows.Forms.MessageBox.Show(S)
                    End If
                End If




                If BatchMode Then
                ElseIf (DesigningP209DrawStructure = False) And (DesigningP209 = True) Then
                Else
                    ErrorMessageForUser(S, "Flexible Pavement Design")
                End If

                If DesignType = NewFlex And jobLCode(ISect, 2) = 6 Then
                    Call CheckMinThickness()
                End If

                Exit Do
            End If

            If Overflow Then
                For I = IL - 1S To IL
                    Thick(I) = CSng(Thick(I) * 0.5)
                    If Thick(I) < ThickMin(LCode(I)) Then
                        Thick(I) = ThickMin(LCode(I))
                    End If
                Next I
                CDFErr = 10.0!
                GoTo StartFlexLoop
            End If

            TM1 = Thick(IL)
            CDFM1 = CSng(System.Math.Log(CDFSUBMAX))
            DELT = CSng(Thick(IL) * 0.01)
            Thick(IL) = Thick(IL) + DELT
            Call UpdateCurrentSectData()
            'Call WESModulus(AggErr, SubLayers)
            Call FAAModulus(AggErr, SubLayers)

            Call UM.UpdateStructure()

            If AggErr Then
                S = "Error computing aggregate Modulus" & NL
                S = S & "The design has been aborted"
                ErrorMessageForUser(S, "Flexible Pavement Design")
                Exit Do
            End If
            Call WriteFile()

            Call LEDFAA_to_LEAF(DesignType)
            TimeSave1 = timeGetTime


            RunLEAF = New LEAFClassLib.clsLEAF()
            If gTandemFnew Then ' kairat replace tandem
                Call RunLEAF.ComputeResponse2(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            Else
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            End If 'ikawa
            'Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)

            If DesigningStr = False Then Exit Do ' Also JULEA error.

            Overflow = True ' CDF at subgrade.
            Call LeafCDFFlex(CDFSUBMAX, EvalDepth(1), Overflow, VertStrainReponse)

            Debug.WriteLine("Overflow 2 = " & Overflow)

            DELCDF = CSng(System.Math.Log(CDFSUBMAX) - CDFM1)

            If DELCDF > 0.0! And CDFSUBMAX > 0.000001 Then
                If CDFSUBMAX > 1.0! Then
                    Thick(IL) = Thick(IL) * 2.0!
                    CDFErr = 10.0!
                    GoTo StartFlexLoop
                Else
                    S = "The design procedure cannot converge. In this" & NL
                    S = S & "case, the cause is an inconsistency in the model." & NL
                    S = S & "CDF at the top of the subgrade increases when" & NL
                    S = S & "thickness increases and CDF < 1." & NL2
                    S = S & "At the least, this can occur when a very" & NL
                    S = S & "stiff layer is placed on a very flexible" & NL
                    S = S & "subgrade. Please check the structure."
                    If Not BatchMode Then
                        ErrorMessageForUser(S, "Cannot Converge")
                    End If
                    Exit Do
                End If
            End If

            '   Control overshoot with far-from-design structures.
            If -CDFErrCntrl < CDFM1 And CDFM1 < CDFErrCntrl Then ' .2 < CDF < 5.0
                Factor = 1.0#
            ElseIf -(CDFErrCntrl + 1) < CDFM1 And CDFM1 < (CDFErrCntrl + 1) Then  ' .02 < CDF < 50.0
                Factor = 0.95
            Else
                Factor = 0.6
            End If

            DELT = CSng((-CDFM1 * DELT / DELCDF) * Factor)

            '    Debug.Print Thick(IL); DELT; CDFASPMAX; CDFSUBMAX


            If TM1 + DELT < ThickMin(LCode(IL)) Or CDFSUBMAX < 0.000001 Then 'ikawa 2012.11.20
                Thick(IL) = ThickMin(LCode(IL))
                ' Thick(IL - 1) = CSng(Thick(IL - 1) * 0.5)
                ' If Thick(IL - 1) < ThickMin(LCode(IL - 1)) Then
                'Thick(IL - 1) = ThickMin(LCode(IL - 1))
                'End If
                CDFErr = 10.0!
            Else
                If DELT > 50 Then DELT = 50 'ikawa 2012.11.20

                Thick(IL) = TM1 + DELT
                '      If Abs(DELT) < .1 Then Exit Do
            End If

            '    Debug.Print time$
            If ILoop > 25 Then
                S = "The design will not converge." & NL
                S = S & "The design is being aborted. " & NL2
                S = S & "Please check the structure."
                ErrorMessageForUser(S, "No Convergence")
                Exit Do
                '------------------------------
            ElseIf NPLayers = 4 Then 'ikawa 999

                If DesigningP209 And (Not LifeComputation) And jobLCode(ISect, 2) = 6 And jobLCode(ISect, 3) = 8 Or
                   DesigningP209 And (Not LifeComputation) And jobLCode(ISect, 2) = 14 And jobLCode(ISect, 3) = 6 Then
                    If Thick(3) = ThickMin(LCode(3)) Then


                        S = "Design Stopped - The subbase has reached" & NL
                        S = S & "minimum thickness. " & NL2
                        S = S & "Hit OK to return to the program." & NL
                        ErrorMessageForUser(S, "Subbase Minimum Thickness")
                        Exit Sub
                    End If
                End If
                '---------------------------------
            End If
        Loop

        ' Capture sublayer data and mark detailed report as populated
        Try
            gDetailedReportData.SublayerData = New clsSublayerData()
            gDetailedReportData.SublayerData.EvalDepthSubgrade = EvalDepth(1)
            ' Capture design layers
            For I = 1 To NPLayers
                Dim dli As New clsLayerInfo()
                dli.Thickness = Thick(I)
                dli.Modulus = Modulus(I)
                dli.LCode = LCode(I)
                gDetailedReportData.SublayerData.DesignLayers.Add(dli)
            Next
            ' Capture expanded sublayers
            For I = 1 To julNPLayers
                Dim dli As New clsLayerInfo()
                dli.Thickness = julThick(I)
                dli.Modulus = julModulus(I)
                dli.LCode = julLCode(I)
                gDetailedReportData.SublayerData.ExpandedSublayers.Add(dli)
            Next
            gDetailedReportData.IsPopulated = True
        Catch ex As Exception
        End Try

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

        'to preserve subgrade CDF results
        For I = 1 To MaxSects
            For J = 1 To MaxSectAC
                jobCDFtableSub(I, J) = jobCDFtable(I, J)
                jobCDFacrftMaxtableSub(I, J) = jobCDFacrftMaxtable(I, J)
                jobCtoPtableSub(I, J) = jobCtoPtable(I, J)
            Next J
        Next I
        ' NoOutFiles = True
        If OutPutFile Then
            '   Write subgrade strain data to file.
            TimeSave1 = timeGetTime

            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
            Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            RunLEAF = Nothing
            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            Call WriteLEAFData(LEAFOutputText)
            SS = WorkingDir & "LeafSG.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFOutputText)
            FileClose((I))
        End If

        'Added compaction capability to FF1.4 based on FF1.313 082012 by YGC 021913
        'Modified for Compaction Criteria using Life function by YGC 042312
        If ((LifeComputation And gUseCompaction) _
            Or (BatchMode And gUseCompaction)) _
           And DesigningP209DrawStructure Then 'ikawa 2013 LeadDesignFlex2 PPPP
            NextraAC = 0 'ikawa 2015
            Call WriteLEAFCompactionData(LEAFCompactionText)

            CompactionDesigned(ISect, IJob) = Now    'added to record time for compaction design by YGC 102213

            SS = WorkingDir & "Compaction.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFCompactionText)
            FileClose((I))
        End If
        ' Modify end for compaction design using Life function by YGC 042312
        'Added compaction capability to FF1.4 based on FF1.313 082012 by YGC 021913

        SS = ACDATPath & "DAMTMPSG.DAT"
        If Dir(SS) <> "" Then Kill(SS)
        S = ACDATPath & "DAMTMP.DAT"
        If Dir(S) <> "" Then Rename(S, SS)


        AsphaltCDFComputed = False
        'NoACCDF = True
        If NoACCDF = False Then
            If DesigningStr = True Then ' Get asphalt CDF.
                ComputingAsphaltCDF = True
                EvalDepth(1) = -julThick(1)
                Call WriteFile() ' Write new EvalDepth.
                Call LEDFAA_to_LEAF(DesignType)

                TimeSave1 = timeGetTime
                RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                RunLEAF = Nothing
                RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                NEvalPointsMax = UBound(AllResp, 2)
                ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                For IAC = 1 To NAC
                    For IEval = 1 To CallAC(IAC).NEvalPoints
                        StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                        StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                        TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                        If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                            HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                        Else
                            HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                        End If

                        ' - sign: compression
                        ' + sign: tension
                        If HorizStrainResponse(IAC, IEval) < 0 Then
                            HorizStrainResponse(IAC, IEval) = 0.0
                        End If

                    Next IEval
                Next IAC



                If DesigningStr = True Then ' Also JULEA error.
                    Call UpdateCurrentSectData()
                    Overflow = False ' CDF in asphalt.
                    '        Call CDFFlex(CDFASPMAX, EvalDepth(1), Overflow)
                    gTandemFnew = False
                    Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                    Call UM.UpdateStructure()

                    gTandemFnew = gTandemFnewHolder
                    CDFAsp = CDFASPMAX
                    '        Debug.Print "CDFAsp = "; CDFAsp
                    AsphaltCDFComputed = True

                    CDFASPMAX1 = CDFASPMAX 'ikawa 999
                End If

                'CDF for HMA layer
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtableHMA(I, J) = jobCDFtable(I, J)
                        jobCDFacrftMaxtableHMA(I, J) = jobCDFacrftMaxtable(I, J)
                        jobCtoPtableHMA(I, J) = jobCtoPtable(I, J)
                    Next J
                Next I


                'checking for stabilized base
                If DesignType = NewFlex Then 'LeafDesignFlex2
                    If LCode(2) = 14 Then 'LayerTypePic$(14) = "P-401/ P-403 St (flex)"
                        EvalDepth(1) = -julThick(1) - julThick(2)
                        'EvalDepth(1) = kk
                        Call WriteFile() ' Write new EvalDepth.
                        Call LEDFAA_to_LEAF(DesignType)

                        TimeSave1 = timeGetTime
                        RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                        LEAStrActiveX.EvalLayer = 2 'ikawa
                        Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                        RunLEAF = Nothing
                        RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                        NEvalPointsMax = UBound(AllResp, 2)
                        ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                        For IAC = 1 To NAC
                            For IEval = 1 To CallAC(IAC).NEvalPoints
                                StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                                StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                                TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                                If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                                    HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                                Else
                                    HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                                End If

                                ' - sign: compression
                                ' + sign: tension
                                If HorizStrainResponse(IAC, IEval) < 0 Then
                                    HorizStrainResponse(IAC, IEval) = 0.0
                                End If

                            Next IEval
                        Next IAC

                    End If
                End If



                If DesigningStr = True Then ' Also JULEA error.
                    Call UpdateCurrentSectData()
                    Overflow = False ' CDF in asphalt.
                    '        Call CDFFlex(CDFASPMAX, EvalDepth(1), Overflow)
                    gTandemFnew = False
                    Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                    Call UM.UpdateStructure()

                    gTandemFnew = gTandemFnewHolder
                    If CDFASPMAX1 > CDFASPMAX Then 'ikawa 999
                        CDFASPMAX = CDFASPMAX1
                    End If


                    CDFAsp = CDFASPMAX
                    '        Debug.Print "CDFAsp = "; CDFAsp
                    AsphaltCDFComputed = True
                End If




                'CDF for P-401/P-403 St (flex) layer
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtable401(I, J) = jobCDFtable(I, J)
                        jobCDFacrftMaxtable401(I, J) = jobCDFacrftMaxtable(I, J)
                        jobCtoPtable401(I, J) = jobCtoPtable(I, J)
                    Next J
                Next I

                'reinstate CDF for subgrade
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtable(I, J) = jobCDFtableSub(I, J)
                        jobCDFacrftMaxtable(I, J) = jobCDFacrftMaxtableSub(I, J)
                        jobCtoPtable(I, J) = jobCtoPtableSub(I, J)
                    Next J
                Next I

            End If

            '   Write asphalt strain data to file.
            If OutPutFile Then
                Call WriteLEAFData(LEAFOutputText)

                SS = WorkingDir & "LeafAC.out"

                If Dir(SS) <> "" Then Kill(SS)
                I = CShort(FreeFile())
                FileOpen(I, SS, OpenMode.Output)
                PrintLine(I, LEAFOutputText)
                FileClose((I))
            End If


        Else '-----------------------------   If NoACCDF = False Then
            CDFAsp = -1
        End If


        ComputingAsphaltCDF = False

    End Sub



    Public Sub LeafDesignFlexOFlex()

        Static IL, I As Short
        Static TM1 As Single
        Static CDFM1, DELT, DELCDF As Single
        Static Overflow As Boolean, MinCount As Short
        Static ILoop As Short
        Static AggErr As Boolean
        Static LifeM1 As Single
        'ikawa 08/28/2009
        'Static CDFSUBMAX, CDFASPMAX As Single
        Static VertStrainReponse(,) As Double
        Static HorizStrainResponse(,) As Double
        Static TempD As Double
        Static StrainMR, StrainMC As Double
        Static IAC, NEvalPointsMax, IEval As Integer
        Static TimeSave1 As Integer
        Static Factor As Double
        Dim gTandemFnewHolder As Boolean
        gTandemFnewHolder = gTandemFnew
        Beeped = False
        ComputingAsphaltCDF = False
        AsphaltCDFComputed = False
        SubLayers = False
        LayerSwitch = False

        If LifeComputation Then
            LayerSwitch = True
            LifeStr = 0.0!
        Else '2013.01.03
            SMin = CStr(NullDate)
        End If


        If gNewModulus_P154 Or gNewModulus_P154 Then 'ikawa
            LayerSwitch = True
            SubLayers = True
        End If




        MinCount = 0
        CDFErr = 10.0!
        ILoop = 0
        'gFirstIter = True
        NStrIterations = ILoop
        Call UpdateCurrentSectData() ' To display correct
        'Call WESModulus(AggErr, SubLayers) ' sublayer information.
        Call FAAModulus(AggErr, SubLayers)

        Call UM.UpdateStructure()

        IL = 1
        Do
StartFlexOFlexLoop:
            NStrIterations = ILoop
            ILoop = ILoop + 1S
            Call UpdateCurrentSectData()
            'Call WESModulus(AggErr, SubLayers)
            Call FAAModulus(AggErr, SubLayers)

            Call UM.UpdateStructure()

            If AggErr = True Then
                S = "Error computing aggregate Modulus" & NL
                S = S & "The design has been aborted"
                Call ErrorMessageForUser(S, "Flexible Over Flexible Design")
                Exit Do
            End If

            Call WriteFile()
            Call LEDFAA_to_LEAF(DesignType)
            TimeSave1 = timeGetTime

            RunLEAF = New LEAFClassLib.clsLEAF()
            If gTandemFnew Then
                Call RunLEAF.ComputeResponse2(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            Else
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            End If
            'Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)

            If DesigningStr = False Then Exit Do ' Also JULEA error.
            'If LifeComputation Then Exit Do 'PPPP

            Overflow = True ' CDF at subgrade.
            Call LeafCDFFlex(CDFSUBMAX, EvalDepth(1), Overflow, VertStrainReponse)

            'If LifeComputation Then Exit Do 'PPPP


            'kawa 2015.01.12 LeafDesignFlexOFlex
            If Not LifeComputation Then 'PPPP
                If CDFSUBMAX < 0.01 And (Thick(1) > ThickMin(LCode(1))) Then
                    'Thick(1) = Thick(1) * 0.75
                    Thick(1) = Thick(1) * 0.9 'kairat 

                    If Thick(1) < ThickMin(LCode(1)) Then
                        Thick(1) = ThickMin(LCode(1))
                    End If

                    GoTo StartFlexOFlexLoop
                End If
            End If



            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
            Dim k1, k2, k3 As Integer

            For k1 = 1 To NAC + 1
                For k2 = 1 To 41
                    CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                Next
            Next
            If LifeComputation Then
                LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
                FlexonRigidCounter = FlexonRigidCounter + 1
                If LifeCounterForPCRRuns = 1 Then
                    For k1 = 1 To NAC + 1
                        For k2 = 1 To 41
                            CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                        Next
                    Next
                    For I = 1 To NAC
                        CDFtableTemp(I) = jobCDFtable(ISect, I)
                        CDFacrftMaxtableTemp(I) = jobCDFacrftMaxtable(ISect, I)
                        CDFtableTemp3(I) = CDFtableTemp(I)
                        CDFacrftMaxtableTemp3(I) = CDFacrftMaxtableTemp(I)
                    Next
                End If
                If (FlexonRigidChecker = True And FlexonRigidCounter = 1) Then
                    For k1 = 1 To NAC + 1
                        For k2 = 1 To 41
                            CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                        Next
                    Next
                    For I = 1 To NAC
                        CDFtableTemp(I) = jobCDFtable(ISect, I)
                        CDFacrftMaxtableTemp(I) = jobCDFacrftMaxtable(ISect, I)
                        CDFtableTemp3(I) = CDFtableTemp(I)
                        CDFacrftMaxtableTemp3(I) = CDFacrftMaxtableTemp(I)
                    Next
                End If
            End If

            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

            If LifeComputation Then
                SubLayers = True
                LifeM1 = Life
                CDFPic = CDFSUBMAX
                CDFM1 = CDFSUBMAX
                For I = 1 To NAC ' GFH 08/13/03.
                    '       Save from last call to LeafCDFFlex for reporting in Aircraft Table.
                    '       Could multiply final CDFs by (Design Life) / Life but for RepsAnnualInc.
                    'CDFtableTemp(I) = jobCDFtable(ISect, I)
                    'CDFacrftMaxtableTemp(I) = jobCDFacrftMaxtable(ISect, I)
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
                    For I = 1 To NAC
                        Temp1 = LifeStr
                        If 1.0! + RepsInc(I) * LifeStr < 0.0! Then
                            Temp1 = -1.0! / RepsInc(I)
                        End If
                        Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                        Reps(I) = Temp * RepsAnnual(I) * Temp1
                    Next I
                    Overflow = True ' CDF at subgrade.
                    Call LeafCDFFlex(CDFSUBMAX, EvalDepth(1), Overflow, VertStrainReponse)
                    If Overflow Then
                        S = "Subgrade strains are too low" & vbCrLf
                        S = S & "to accurately compute life."
                        Ret = MsgBoxDQ(S, 0, "Computing Life")
                        Exit Do
                    ElseIf LifeM1 < 0.0001 Then
                        LifeM1 = 0.0!
                        LifeStr = 0.0!
                        Exit Do
                    End If
                    DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFSUBMAX - CDFM1)
                    Temp = LifeStr
                    LifeStr = LifeM1 + DELT
                    LifeM1 = Temp
                    CDFM1 = CDFSUBMAX
                Loop Until System.Math.Abs(CDFM1 - 1.0!) < 0.001 Or (FEDFAA1.FF_true_ACN)

                If AircraftLifeCDFvsDesignLife Then
                    For I = 1 To NAC ' GFH 08/13/03.
                        '         Saved above before start of Life Do loop.
                        jobCDFtable(ISect, I) = CDFtableTemp(I)
                        jobCDFacrftMaxtable(ISect, I) = CDFacrftMaxtableTemp(I)
                    Next I
                    '     Else uses the CDF values set based on life to failure in last call to LeafCDFFlex.
                End If
                Exit Do
            End If

            CDFPic = CDFSUBMAX


            System.Diagnostics.Debug.WriteLine(ILoop & LayerSwitch & Thick(IL) & EvalDepth(1) & "CDF = " & CDFSUBMAX)
            CDFErr = CSng(System.Math.Abs(System.Math.Log(CDFSUBMAX))) 'Abs(1! - CDFSUBMAX)
            If CDFErr < CDFErrCntrl And SubLayers And Not LayerSwitch Then
                LayerSwitch = True ' Used in WESModulus and ModulusThick.
                CDFErr = 10.0! ' Used in ModulusThick. Not really needed here.
                GoTo StartFlexOFlexLoop
            End If

            If CDFErr < CDFExitErr Then Exit Do

            If Thick(IL) = ThickMin(LCode(IL)) And CDFSUBMAX < 1.0! Then
                S = "The minimum overlay thickness has" & NL
                S = S & " been reached. CDF = " & Format(CDFSUBMAX, "0.000") & "."
                Call ErrorMessageForUser(S, "Flexible Over Flexible Design")

                Designed = Now '2013.01.03
                SMin = "Min" '2013.01.03
                Windows.Forms.MessageBox.Show(S)

                Exit Do
            End If

            If Overflow Then
                '     Only change the thickness of the overlay layer.
                Thick(IL) = CSng(Thick(IL) * 0.5)
                If Thick(IL) < ThickMin(LCode(IL)) Then
                    Thick(IL) = ThickMin(LCode(IL))
                End If
                GoTo StartFlexOFlexLoop
            End If

            TM1 = Thick(IL)
            CDFM1 = CSng(System.Math.Log(CDFSUBMAX))
            DELT = CSng(Thick(IL) * 0.01)
            Thick(IL) = Thick(IL) + DELT
            Call UpdateCurrentSectData()
            'Call WESModulus(AggErr, SubLayers)
            Call FAAModulus(AggErr, SubLayers)

            Call UM.UpdateStructure()

            If AggErr Then
                S = "Error computing aggregate Modulus" & NL
                S = S & "The design has been aborted"
                Call ErrorMessageForUser(S, "Flexible Over Flexible Design")
                Exit Do
            End If
            '    Debug.Print julNPLayers
            For I = 1 To julNPLayers
                '      Debug.Print I; julThick(I); julModulus(I); julLCode(I)
            Next I
            Call WriteFile()

            Call LEDFAA_to_LEAF(DesignType)
            TimeSave1 = timeGetTime

            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
            If gTandemFnew Then ' kairat replace tandem
                Call RunLEAF.ComputeResponse2(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            Else
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            End If
            'Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)

            If DesigningStr = False Then Exit Do ' Also JULEA error.
            Overflow = True ' CDF at subgrade.
            Call LeafCDFFlex(CDFSUBMAX, EvalDepth(1), Overflow, VertStrainReponse)

            '3333333333






            DELCDF = CSng(System.Math.Log(CDFSUBMAX) - CDFM1)

            If DELCDF > 0.0! Then
                If CDFSUBMAX > 1.0! Then
                    Thick(IL) = Thick(IL) * 2.0!
                    CDFErr = 10.0!
                    GoTo StartFlexOFlexLoop
                Else
                    S = "The design procedure cannot converge. In this" & NL
                    S = S & "case, the cause is an inconsistency in the model." & NL
                    S = S & "CDF at the top of the subgrade increases when" & NL
                    S = S & "thickness increases and CDF < 1." & NL2
                    S = S & "At the least, this can occur when a very" & NL
                    S = S & "stiff layer is placed on a very flexible" & NL
                    S = S & "subgrade. Please check the structure."
                    If Not BatchMode Then
                        Call ErrorMessageForUser(S, "Cannot Converge")
                    End If
                    Exit Do
                End If
            End If

            '   Control overshoot with far-from-design structures.
            If -CDFErrCntrl < CDFM1 And CDFM1 < CDFErrCntrl Then ' .2 < CDF < 5.0
                Factor = 1.0#
            ElseIf -(CDFErrCntrl + 1) < CDFM1 And CDFM1 < (CDFErrCntrl + 1) Then  ' .02 < CDF < 50.0
                Factor = 0.95
            Else
                Factor = 0.6
            End If

            DELT = CSng((-CDFM1 * DELT / DELCDF) * Factor)

            '    Debug.Print Thick(IL); DELT; CDFASPMAX; CDFSUBMAX
            If TM1 + DELT < ThickMin(LCode(IL)) Then
                System.Diagnostics.Debug.WriteLine(TM1 & DELT)
                Thick(IL) = ThickMin(LCode(IL))
            Else
                Thick(IL) = TM1 + DELT
                '      If Abs(DELT) < .1 Then Exit Do
            End If

            If ILoop > 25 Then
                Dim msg As String
                msg = "The design will not converge" + vbNewLine + "The design will not converge" + vbNewLine + "and is being aborted. "
                S = "The design will not converge" & NL
                S = S & "and is being aborted. " & NL2
                S = S & "Please check the structure."

                ErrorMessageForUser(S, "No Convergence")
                MessageBox.Show(msg)
                Exit Do
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


        'to preserve CDF for subgrade
        For I = 1 To MaxSects
            For J = 1 To MaxSectAC
                jobCDFtableSub(I, J) = jobCDFtable(I, J)
                jobCDFacrftMaxtableSub(I, J) = jobCDFacrftMaxtable(I, J)
                jobCtoPtableSub(I, J) = jobCtoPtable(I, J)
            Next J
        Next I


        If OutPutFile Then
            '   Write subgrade strain data to file.
            TimeSave1 = timeGetTime


            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
            Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            Call WriteLEAFData(LEAFOutputText)
            SS = WorkingDir & "LeafSG.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFOutputText)
            FileClose((I))
        End If

        'added for flex-on-flex compaction by YGC 112213
        If LifeComputation And gUseCompaction Then 'ikawa 2013
            Call WriteLEAFCompactionData(LEAFCompactionText)

            ' CompactionDesigned = Now    'added to record time for compaction design by YGC 102213

            SS = WorkingDir & "Compaction.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFCompactionText)
            FileClose((I))
        End If
        'added for flex-on-flex compaction by YGC 112213 END


        SS = ACDATPath & "DAMTMPSG.DAT"
        If Dir(SS) <> "" Then Kill(SS)
        S = ACDATPath & "DAMTMP.DAT"
        If Dir(S) <> "" Then Rename(S, SS)



        AsphaltCDFComputed = False
        If NoACCDF = False Then
            If DesigningStr = True Then ' Get asphalt CDF.
                ComputingAsphaltCDF = True
                EvalDepth(1) = -julThick(1) + 0.01
                Call WriteFile() ' Write new EvalDepth.
                Call LEDFAA_to_LEAF(DesignType)


                'LEAStrActiveX.EvalLayer = 2
                RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                RunLEAF = Nothing

                NEvalPointsMax = UBound(AllResp, 2)
                ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                For IAC = 1 To NAC
                    For IEval = 1 To CallAC(IAC).NEvalPoints
                        StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                        StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                        TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                        If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                            HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                        Else
                            HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                        End If

                        ' - sign: compression
                        ' + sign: tension
                        If HorizStrainResponse(IAC, IEval) < 0 Then
                            HorizStrainResponse(IAC, IEval) = 0.0
                        End If

                    Next IEval
                Next IAC

                If DesigningStr = True Then ' Also JULEA error.
                    Call UpdateCurrentSectData()
                    gTandemFnew = False
                    Overflow = False ' CDF in asphalt.
                    Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                    Call UM.UpdateStructure()

                    gTandemFnew = gTandemFnewHolder
                    CDFAsp = CDFASPMAX
                    CDFASPMAX1 = CDFASPMAX 'ikawa 999
                    System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
                    AsphaltCDFComputed = True
                End If

                'CDF values for the overlay layer
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtableAC(I, J) = jobCDFtable(I, J)
                        jobCDFacrftMaxtableAC(I, J) = jobCDFacrftMaxtable(I, J)
                        jobCtoPtableAC(I, J) = jobCtoPtable(I, J)
                    Next J
                Next I




                '123+++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                If DesignType = FlexOnFlex Then
                    ComputingAsphaltCDF = True
                    EvalDepth(1) = -(julThick(1) + julThick(2)) + 0.01
                    'EvalDepth(1) = kk
                    Call WriteFile() ' Write new EvalDepth.
                    Call LEDFAA_to_LEAF(DesignType)

                    LEAStrActiveX.EvalLayer = 2
                    RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                    Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                    RunLEAF = Nothing

                    NEvalPointsMax = UBound(AllResp, 2)
                    ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                    For IAC = 1 To NAC
                        For IEval = 1 To CallAC(IAC).NEvalPoints
                            StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                            StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                            TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                            If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                                HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                            Else
                                HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                            End If

                            ' - sign: compression
                            ' + sign: tension
                            If HorizStrainResponse(IAC, IEval) < 0 Then
                                HorizStrainResponse(IAC, IEval) = 0.0
                            End If

                        Next IEval
                    Next IAC

                    If DesigningStr = True Then ' Also JULEA error.
                        Call UpdateCurrentSectData()
                        Overflow = False ' CDF in asphalt.
                        gTandemFnew = False
                        Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                        Call UM.UpdateStructure()

                        gTandemFnew = gTandemFnewHolder
                        If CDFASPMAX1 > CDFASPMAX Then
                            CDFASPMAX = CDFASPMAX1
                        End If

                        CDFAsp = CDFASPMAX
                        System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
                        AsphaltCDFComputed = True
                    End If


                End If

                'CDF values for HMA under HMA overlay
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtableHMA(I, J) = jobCDFtable(I, J)
                        jobCDFacrftMaxtableHMA(I, J) = jobCDFacrftMaxtable(I, J)
                        jobCtoPtableHMA(I, J) = jobCtoPtable(I, J)
                    Next J
                Next I


                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                'reinstating CDF for subgrade layer
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtable(I, J) = jobCDFtableSub(I, J)
                        jobCDFacrftMaxtable(I, J) = jobCDFacrftMaxtableSub(I, J)
                        jobCtoPtable(I, J) = jobCtoPtableSub(I, J)
                    Next J
                Next I








                'start for 401 stabilized base +++++++++++++++++++++++++++++++++
                If (DesignType = FlexOnFlex) And (LCode(3) = 14) Then
                    ComputingAsphaltCDF = True
                    EvalDepth(1) = -(julThick(1) + julThick(2) + julThick(3)) + 0.01
                    'EvalDepth(1) = kk
                    Call WriteFile() ' Write new EvalDepth.
                    Call LEDFAA_to_LEAF(DesignType)

                    LEAStrActiveX.EvalLayer = 3
                    RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                    Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                    RunLEAF = Nothing

                    NEvalPointsMax = UBound(AllResp, 2)
                    ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                    For IAC = 1 To NAC
                        For IEval = 1 To CallAC(IAC).NEvalPoints
                            StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                            StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                            TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                            If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                                HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                            Else
                                HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                            End If

                            ' - sign: compression
                            ' + sign: tension
                            If HorizStrainResponse(IAC, IEval) < 0 Then
                                HorizStrainResponse(IAC, IEval) = 0.0
                            End If

                        Next IEval
                    Next IAC

                    If DesigningStr = True Then ' Also JULEA error.
                        Call UpdateCurrentSectData()
                        Overflow = False ' CDF in asphalt.
                        gTandemFnew = False
                        Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                        Call UM.UpdateStructure()

                        gTandemFnew = gTandemFnewHolder
                        If CDFASPMAX1 > CDFASPMAX Then
                            CDFASPMAX = CDFASPMAX1
                        End If

                        CDFAsp = CDFASPMAX
                        System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
                        AsphaltCDFComputed = True
                    End If


                    'CDF values for HMA under HMA overlay
                    For I = 1 To MaxSects
                        For J = 1 To MaxSectAC
                            jobCDFtable401(I, J) = jobCDFtable(I, J)
                            jobCDFacrftMaxtable401(I, J) = jobCDFacrftMaxtable(I, J)
                            jobCtoPtable401(I, J) = jobCtoPtable(I, J)
                        Next J
                    Next I

                    'reinstating CDF for subgrade layer
                    For I = 1 To MaxSects
                        For J = 1 To MaxSectAC
                            jobCDFtable(I, J) = jobCDFtableSub(I, J)
                            jobCDFacrftMaxtable(I, J) = jobCDFacrftMaxtableSub(I, J)
                            jobCtoPtable(I, J) = jobCtoPtableSub(I, J)
                        Next J
                    Next I

                End If

                'end for 401 stabilized base ++++++++++++++++++++++++++++++++++++


            End If 'If DesigningStr = True Then ' Get asphalt CDF.
            ''   Write asphalt strain data to file.
            If OutPutFile Then
                Call WriteLEAFData(LEAFOutputText)

                SS = WorkingDir & "LeafAC.out"

                If Dir(SS) <> "" Then Kill(SS)
                I = CShort(FreeFile())
                FileOpen(I, SS, OpenMode.Output)
                PrintLine(I, LEAFOutputText)
                FileClose((I))
            End If


        Else '------------------------------------   If NoACCDF = False Then
            CDFAsp = -1 : System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
        End If


        FlexonRigidCounter = 0
        ComputingAsphaltCDF = False
    End Sub



    Sub WriteLEAFData(ByRef LEAFOutputText As String)
        Dim IPad, IEval, I, NAC As Integer
        Dim S As String
        Dim IAC As Integer
        Dim TempD As Double
        ' Used in Sub PRINPL, find principal stresses.
        Dim DIREC(3, 3) As Double
        Dim PRIN(3) As Double
        Dim SIG(6) As Double
        Dim RespFmt As String
        Const PI As Double = 3.14159265358979

        S = "     Layer No.  Thickness    ELasticity  Poisson's  Interface" & vbCrLf
        S = S & "                              Modulus      Ratio    Condition" & vbCrLf
        S = S & "     --------------------------------------------------------" & vbCrLf
        With LEAStrActiveX
            For I = 1 To .NLayers
                If I < .NLayers Then TempD = .Thick(I) Else TempD = 0
                S = S & LPad(10, I.ToString("f0")) & LPad(13, TempD.ToString("f2"))
                S = S & LPad(16, Modulus(I).ToString("#,###,###.0"))
                S = S & LPad(9, .Poisson(I).ToString("f3"))
                S = S & LPad(12, .InterfaceParm(I).ToString("f6")) & vbCrLf
            Next I
            S = S & vbCrLf & vbCrLf
        End With


        NAC = UBound(CallAC)
        For IAC = 1 To NAC

            With CallAC(IAC)
                S = S & "          Aircraft No. " & IAC.ToString("f0") & "  " & .ACname & vbCrLf
                S = S & "          Aircraft design load         :" & " Not Applicable" & vbCrLf
                S = S & "          Fraction of load on main gear:" & LPad(12, "100.0") & vbCrLf
                S = S & "          Gear load                    :" & LPad(12, .GearLoad.ToString("#,###,##0.0")) & vbCrLf
                S = S & "          Number of tires              :" & LPad(10, .NTires.ToString("0")) & vbCrLf & vbCrLf

                S = S & " Tire  Radius  Cont.Area  Cont.Press   Tire Load    X-Coord   Y-Coord" & vbCrLf
                S = S & " No.    (in)    (sq.in)     (psi)       (pounds)      (in)      (in.)" & vbCrLf
                S = S & " --------------------------------------------------------------------" & vbCrLf
                For I = 1 To .NTires
                    TempD = System.Math.Sqrt((.GearLoad / .NTires) / (.TirePress(I) * PI))
                    S = S & LPad(3, I.ToString("f0")) & LPad(9, TempD.ToString("f2")) ' Radius.
                    S = S & LPad(11, (PI * TempD ^ 2).ToString("0.00")) ' Area.
                    S = S & LPad(11, .TirePress(I).ToString("0.00"))
                    S = S & LPad(14, (.GearLoad / .NTires).ToString("#,##0.00"))
                    S = S & LPad(10, .TireX(I).ToString("f2"))
                    S = S & LPad(10, .TireY(I).ToString("f2")) & vbCrLf
                Next I
                S = S & vbCrLf
            End With
        Next IAC

        RespFmt = "0.00000E+00"
        IPad = Len(RespFmt) + 2


        If DesignType = UnbondOnRigid Then
            GoTo UnbondOnRigid1
        End If

        For IAC = 1 To NAC
            S = S & "          Aircraft No. " & IAC.ToString("f0") & "  " & CallAC(IAC).ACname & vbCrLf
            If DesignType = NewFlex Or DesignType = FlexOnFlex Then
                For IEval = 1 To CallAC(IAC).NEvalPoints
                    With AllResp(IAC, IEval)

                        S = S & " Eval Point = " & LPad(4, IEval.ToString("f0")) & "      "
                        S = S & " Layer No. = " & LPad(4, LEAStrActiveX.EvalLayer.ToString("f0")) & vbCrLf
                        S = S & " X-Coord.   = " & LPad(8, CallAC(IAC).EvalX(IEval).ToString("f3")) & "  "
                        S = S & " Y-Coord.  = " & LPad(8, CallAC(IAC).EvalY(IEval).ToString("f3")) & "  "
                        S = S & " Z-Depth = " & LPad(8, LEAStrActiveX.EvalDepth.ToString("f6")) & vbCrLf
                        S = S & vbCrLf

                        I = IPad - 9
                        S = S & "         VERT STR" & Space(I + 1) & "HOR Y STR" & Space(I) & "HOR X STR" & Space(I) & "XZ SHEAR" & Space(I + 1) & "YZ SHEAR" & Space(I + 1) & "XY SHEAR" & vbCrLf

                        S = S & "Stress"
                        S = S & LPad(IPad, .StressZ.ToString(RespFmt))
                        S = S & LPad(IPad, .StressY.ToString(RespFmt))
                        S = S & LPad(IPad, .StressX.ToString(RespFmt))
                        S = S & LPad(IPad, .StressXZ.ToString(RespFmt))
                        S = S & LPad(IPad, .StressYZ.ToString(RespFmt))
                        S = S & LPad(IPad, .StressXY.ToString(RespFmt)) & vbCrLf
                        S = S & "Strain"
                        S = S & LPad(IPad, .StrainZ.ToString(RespFmt))
                        S = S & LPad(IPad, .StrainY.ToString(RespFmt))
                        S = S & LPad(IPad, .StrainX.ToString(RespFmt))
                        S = S & LPad(IPad, .StrainXZ.ToString(RespFmt))
                        S = S & LPad(IPad, .StrainYZ.ToString(RespFmt))
                        S = S & LPad(IPad, .StrainXY.ToString(RespFmt)) & vbCrLf
                        S = S & "Displt"
                        S = S & LPad(IPad, .DeflZ.ToString(RespFmt))
                        S = S & LPad(IPad, .DeflY.ToString(RespFmt))
                        S = S & LPad(IPad, .DeflX.ToString(RespFmt)) & vbCrLf
                        S = S & vbCrLf
                        S = S & "         PRIN 1" & Space(I + 3) & "PRIN 2" & Space(I + 3) & "PRIN 3" & Space(I + 2) & "MAX SHEAR" & Space(I) & "OCT NORMAL" & Space(I - 1) & "OCT SHEAR" & vbCrLf
                        S = S & "Stress"
                        S = S & LPad(IPad, .StressPrin1.ToString(RespFmt))
                        S = S & LPad(IPad, .StressPrin2.ToString(RespFmt))
                        S = S & LPad(IPad, .StressPrin3.ToString(RespFmt))
                        S = S & LPad(IPad, .StressMaxShear.ToString(RespFmt))
                        S = S & LPad(IPad, .StressOctNormal.ToString(RespFmt))
                        S = S & LPad(IPad, .StressOctShear.ToString(RespFmt)) & vbCrLf
                        S = S & "Strain"
                        S = S & LPad(IPad, .StrainPrin1.ToString(RespFmt))
                        S = S & LPad(IPad, .StrainPrin2.ToString(RespFmt))
                        S = S & LPad(IPad, .StrainPrin3.ToString(RespFmt)) & vbCrLf
                        S = S & vbCrLf

                    End With
                Next IEval
            Else
                S = S & vbCrLf
                If DesignType = NewRigid Or DesignType = PCCOnFlex Then

                    'modified to output both edge(NIKE3D) and interior(LEAF) stress by YGC 101613
                    'S = S & "   PCC SLAB HOR STRESS" & vbCrLf
                    ''S = S & LPad(14, CStr(STRSH(IAC))) & vbCrLf
                    S = S & "   PCC SLAB Edge HOR STRESS" & "   PCC SLAB Interior HOR STRESS" & vbCrLf
                    S = S & LPad(25, HorizStressResponse1(IAC, 1).ToString("f4")) & LPad(25, HorizStressResponse2(IAC, 1).ToString("f4")) & vbCrLf
                    S = S & "   PCC SLAB HOR STRESS" & vbCrLf
                    S = S & LPad(25, STRSH(IAC).ToString("f4")) & vbCrLf
                    'modify ended to output both edge(NIKE3D) and interior(LEAF) stress by YGC 101613

                    S = S & vbCrLf
                ElseIf DesignType = UnbondOnRigid Then
                ElseIf DesignType = FlexOnRigid Then
                    S = S & "   PCC SLAB Edge HOR STRESS" & "   PCC SLAB Interior HOR STRESS" & vbCrLf
                    S = S & LPad(25, HorizStressResponse1(IAC, 2).ToString("f4")) & LPad(25, HorizStressResponse3(IAC, 1).ToString("f4")) & vbCrLf
                    S = S & "   PCC SLAB HOR STRESS" & vbCrLf
                    S = S & LPad(25, CStr(Math.Round(PCCStress(IAC), 4))) & vbCrLf
                    'modify ended to output both edge(NIKE3D) and interior(LEAF) stress by YGC 102213
                    S = S & vbCrLf
                End If
            End If
            S = S & vbCrLf
        Next IAC



UnbondOnRigid1:

        If DesignType = UnbondOnRigid Then
            S = S & vbCrLf
            For IAC = 1 To NAC
                S = S & vbCrLf
                S = S & "          Aircraft No. " & IAC.ToString("f0") & "  " & CallAC(IAC).ACname & vbCrLf
                S = S & "" & vbCrLf
                S = S & "   PCC Surface             PCC SURFACE HOR STRESS" & vbCrLf
                S = S & "  SCI     Modulus        Edge   Interior  Selected" & vbCrLf
                S = S & "           (psi)        (psi)     (psi)     (psi)" & vbCrLf
                S = S & "--------------------------------------------------" & vbCrLf
                S = S & "100.00   4,000,000"
                S = S & LPad(12, gPCC_NIKE3D_Stress(IAC).ToString("##,##0.000")) &
                LPad(10, gPCC_LEAF_Stress(IAC).ToString("##,##0.000")) &
                LPad(10, PCCStress(IAC).ToString("##,##0.000")) & vbCrLf


                S = S & "--------------------------------------------------" & vbCrLf
                S = S & "" & vbCrLf
                S = S & "   PCC Surface             PCC OVERLAY HOR STRESS" & vbCrLf
                S = S & "  SCI     Modulus        Edge   Interior  Selected" & vbCrLf
                S = S & "           (psi)        (psi)     (psi)     (psi)" & vbCrLf
                S = S & "--------------------------------------------------" & vbCrLf
                S = S & "100.00   4,000,000"
                S = S & LPad(12, gOverlay_NIKE3D_Stress(IAC, 0).ToString("##,##0.000")) &
                        LPad(10, gOverlay_LEAF_Stress(IAC, 0).ToString("##,##0.000")) &
                        LPad(10, OverlayStress(IAC).ToString("##,##0.000")) & vbCrLf

                Dim n1 As Integer
                For n1 = 1 To gNSectionUsed
                    S = S & LPad(6, gPCC_SCI(n1).ToString("##0.00"))
                    S = S & LPad(12, gPCC_Surface_Mod(n1).ToString("##,##0"))
                    S = S & LPad(12, gOverlay_NIKE3D_Stress(IAC, n1).ToString("##,##0.000")) &
                            LPad(10, gOverlay_LEAF_Stress(IAC, n1).ToString("##,##0.000")) &
                            LPad(10, gOverlay_USED_Stress(IAC, n1).ToString("##,##0.000")) & vbCrLf
                Next

                S = S & "--------------------------------------------------" & vbCrLf

                S = S & vbCrLf

            Next IAC
        End If

        LEAFOutputText = S

    End Sub


    Sub WriteLEAFCompactionData(ByRef LEAFCompactionText As String)
        'This sub is modified to extent compaction capability to rigid pavement by YGC 112213 
        'This sub is to add compaction capability to FF1.4  based on FF1.313 082012 by YGC 021913

        'This sub is added to capture the max response of all evaluation points 
        'and to write the compaction depth requirement referenced to pavement surface by YGC 041312
        'the critical AC is the one having the largest CI by YGC 042312

        Try


            'Static VertStrainReponse(,) As Double
            Dim VertStrainReponse(,) As Double

            Static TimeSave1 As Integer

            Static Overflow As Boolean

            Dim JDenLevel As Short

            Dim StressZMax As Double

            'Dim IPad, IEval, I, NAC As Integer
            Dim IPad, IEval, I As Integer

            Dim S As String
            Dim IAC As Integer
            Dim TempD As Double     'temporarial variable for load radius
            Dim TempTh As Double    'temporarial variable for layer thickness
            ' Used in Sub PRINPL, find principal stresses.
            Dim RespFmt As String
            Const PI As Double = 3.14159265358979


            'Dim NSGLayer As Integer = 13
            Dim NSGLayer As Integer = 25 'Modified to deeper depth by YGC 050912


            Dim CI(NSGLayer) As Double

            Dim CompactionDensityNC(NSGLayer) As Double
            Const aNC As Double = 105.07606
            Const bNC As Double = 60.76794
            Const cNC As Double = 1.0010464
            Const dNC As Double = 0.39894008


            Dim CompactionDensityC(NSGLayer) As Double
            Const aC As Double = 102.63107
            Const bC As Double = 152.97372
            Const cC As Double = 1.4796433
            Const dC As Double = 0.29956917

            Dim gTandemFnewHolder As Boolean 'WriteLEAFCompactionData()		
            gTandemFnewHolder = gTandemFnew

            S = "     Layer No.  Thickness    ELasticity  Poisson's  Interface" & vbCrLf
            S = S & "                              Modulus      Ratio    Condition" & vbCrLf
            S = S & "     --------------------------------------------------------" & vbCrLf
            With LEAStrActiveX
                For I = 1 To .NLayers
                    If I < .NLayers Then TempTh = .Thick(I) Else TempTh = 0
                    S = S & LPad(10, I.ToString("0")) & LPad(13, TempTh.ToString("0.00"))
                    S = S & LPad(16, .Modulus(I).ToString("#,###,###.0"))
                    S = S & LPad(9, .Poisson(I).ToString("0.000"))
                    S = S & LPad(11, .InterfaceParm(I).ToString("0.000000")) & vbCrLf
                Next I
                S = S & vbCrLf & vbCrLf
            End With

            Dim TempRadius(MaxSectAC) As Double  'added to get the tire radius by YGC 042312

            Dim HeaviestAC As Single         'added to find the heaviest AC for ASTM by YGC 102213 
            HeaviestAC = 0.0


            NAC = UBound(CallAC) - NextraAC
            'redefine CallAC by AC instead of by GEAR for large AC(B747) rigid compaction by YGC 112213
            ReDim CallAC(NAC)

            For I = 1 To NAC


                CallAC(I).ACname = ACName(I)
                CallAC(I).GearLoad = LEAAircraft(I).GearLoad
                CallAC(I).NTires = LEAAircraft(I).NTires

                ReDim CallAC(I).TirePress(CallAC(I).NTires)
                ReDim CallAC(I).TireX(CallAC(I).NTires)
                ReDim CallAC(I).TireY(CallAC(I).NTires)

                CallAC(I).NEvalPoints = LEAAircraft(I).NEvalPoints
                ReDim CallAC(I).EvalX(CallAC(I).NEvalPoints)
                ReDim CallAC(I).EvalY(CallAC(I).NEvalPoints)


                For I2 = 1 To CallAC(I).NTires
                    CallAC(I).TirePress(I2) = LEAAircraft(I).TirePress(I2)
                    CallAC(I).TireX(I2) = LEAAircraft(I).TireX(I2)
                    CallAC(I).TireY(I2) = LEAAircraft(I).TireY(I2)
                Next I2

                For I2 = 1 To CallAC(I).NEvalPoints
                    CallAC(I).EvalX(I2) = LEAAircraft(I).EvalX(I2)
                    CallAC(I).EvalY(I2) = LEAAircraft(I).EvalY(I2)
                Next I2
            Next I
            'redefine CallAC by AC instead of by GEAR END



            For IAC = 1 To NAC
                With CallAC(IAC)
                    S = S & "          Aircraft No. " & IAC.ToString("0") & "  " & .ACname & vbCrLf
                    S = S & "          Aircraft design load         :" & " Not Applicable" & vbCrLf
                    S = S & "          Fraction of load on main gear:" & LPad(12, "100.0") & vbCrLf
                    S = S & "          Gear load                    :" & LPad(12, .GearLoad.ToString("#,###,##0.0")) & vbCrLf
                    S = S & "          Number of tires              :" & LPad(10, .NTires.ToString("0")) & vbCrLf & vbCrLf

                    S = S & " Tire  Radius  Cont.Area  Cont.Press   Tire Load    X-Coord   Y-Coord" & vbCrLf
                    S = S & " No.    (in)    (sq.in)     (psi)       (pounds)      (in)      (in.)" & vbCrLf
                    S = S & " --------------------------------------------------------------------" & vbCrLf
                    For I = 1 To .NTires
                        TempD = System.Math.Sqrt((.GearLoad / .NTires) / (.TirePress(I) * PI))
                        S = S & LPad(3, I.ToString("0")) & LPad(9, TempD.ToString("0.00")) ' Radius.
                        S = S & LPad(11, (PI * TempD ^ 2).ToString("0.00")) ' Area.
                        S = S & LPad(11, .TirePress(I).ToString("0.00"))
                        S = S & LPad(14, (.GearLoad / .NTires).ToString("#,##0.00"))
                        S = S & LPad(10, .TireX(I).ToString("0.00"))
                        S = S & LPad(10, .TireY(I).ToString("0.00")) & vbCrLf
                    Next I
                    TempRadius(IAC) = TempD  'added to get the tire radius by YGC 042312
                End With

                'added to find the heaviest AC for ASTM by YGC 102213
                If HeaviestAC < CallAC(IAC).GearLoad Then
                    HeaviestAC = CallAC(IAC).GearLoad
                    IDHeaviestAC = IAC
                End If


                S = S & vbCrLf & vbCrLf
            Next IAC

            RespFmt = "0.00000E+00  "
            IPad = Len(RespFmt) + 2

            Dim DepthSG As Double         'added to save the values at the SG top by YGC 042312

            Dim jobCtoPtableTemp(MaxSectAC) As Double
            Dim jobCDFtableTemp(MaxSectAC) As Double
            Dim jobCDFacrftMaxtableTemp(MaxSectAC) As Double
            For IAC = 1 To NAC
                jobCtoPtableTemp(IAC) = jobCtoPtable(ISect, IAC)
                jobCDFtableTemp(IAC) = jobCDFtable(ISect, IAC)
                jobCDFacrftMaxtableTemp(IAC) = jobCDFacrftMaxtable(ISect, IAC)
            Next IAC
            'add ended by YGC 042312


            'Modified to fix the total Reptition at 120,000 (annual departure 6,000, 20 yrs life) by YGC 042312  
            'added to find the repetition adjusted factor based on Max gross weight or AC CDF by YGC 042312 
            Dim RepAdjusted As Integer

            Dim IDACCritical As Integer


            'RepAdjusted = 120000
            'Modify end by YGC 042312

            RepAdjusted = 20 * gCompactionDeparture
            'RepAdjusted = 20 * CSng(dlgOptionsTest.lblCompactionDeparture.Text)
            'Modified to get input annual departue by YGC 021913 


            Dim CompactionCPRatio(NAC, NSGLayer), CompactionStress(NAC, NSGLayer), CompactionDepth(NSGLayer) As Double 'added to save CtoP ratio, stress, and depth of compaction by YGC 112213
            Dim CI2AC(NAC, NSGLayer) As Double  ' CI2 for each AC


            Dim Covs As Double

            Dim LogBETA As Double
            Dim BETA As Double


            'added to initiate zero value to avoid result transmit by YGC 050912
            For J = 1 To NDenLevel
                CompactionIntDenNC(J) = 0
                CompactionIntDenC(J) = 0

                CompactionIntDenDepthNC(J) = 0
                CompactionIntDenDepthC(J) = 0
            Next J
            'add end to initiate zero value to avoid result transmit by YGC 050912



            '**************************Compute Compaction Stress and Depth *********************************'

            'rewritten for flexible compaction by YGC 112213 
            If DesignType = NewFlex Or DesignType = FlexOnFlex Then

                DepthSG = LEAStrActiveX.EvalDepth

                I = 1
                Do
                    CompactionDepth(I) = 10 * (I - 1)
                    LEAStrActiveX.EvalDepth = CompactionDepth(I)

                    ' find LEAStrActiveX.EvalLayer
                    If LEAStrActiveX.EvalDepth >= DepthSG Then
                        LEAStrActiveX.EvalLayer = LEAStrActiveX.NLayers
                    Else
                        Dim jj As Integer
                        Dim depth As Double
                        depth = 0
                        For jj = 1 To LEAStrActiveX.NLayers - 1
                            If depth <= LEAStrActiveX.EvalDepth And LEAStrActiveX.EvalDepth < depth + LEAStrActiveX.Thick(jj) Then
                                LEAStrActiveX.EvalLayer = jj
                                Exit For
                            End If
                            depth = depth + LEAStrActiveX.Thick(jj)
                        Next
                    End If
                    ' find LEAStrActiveX.EvalLayer

                    TimeSave1 = timeGetTime
                    RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                    Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp) ' compute AllResp (all response) at compaction depth
                    RunLEAF = Nothing

                    RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)

                    Overflow = True
                    gTandemFnew = False
                    Call LeafCDFFlex(CDFASPMAX, LEAStrActiveX.EvalDepth, Overflow, VertStrainReponse) ' to compute jobCtoPtable (CtoP ratio) at compaction depth

                    For IAC = 1 To NAC
                        CompactionCPRatio(IAC, I) = jobCtoPtable(ISect, IAC)

                        StressZMax = 0.0
                        For IEval = 1 To CallAC(IAC).NEvalPoints
                            With AllResp(IAC, IEval)
                                If Math.Abs(StressZMax) <= Math.Abs(.StressZ) Then
                                    StressZMax = .StressZ
                                End If

                            End With
                        Next IEval

                        CompactionStress(IAC, I) = StressZMax

                    Next IAC

                    I = I + 1
                Loop Until I = NSGLayer + 1
                'rewritten for flexible compaction by YGC 112213 END


            Else
                'find DepthSG for Rigid
                DepthSG = 0
                For I = 1 To LEAStrActiveX.NLayers - 1
                    DepthSG = DepthSG + LEAStrActiveX.Thick(I)
                Next I
                ' find DepthSG for Rigid END


                Dim CompactionDepthLEAF(NSGLayer), CompactionDepthNIKE3D(NSGLayer) As Double

                ' ****************LEAF interior stress as the critical stress****************
                I = 1
                Do
                    CompactionDepthLEAF(I) = 10 * (I - 1)
                    LEAStrActiveX.EvalDepth = CompactionDepthLEAF(I)

                    ' find LEAStrActiveX.EvalLayer
                    If LEAStrActiveX.EvalDepth >= DepthSG Then
                        LEAStrActiveX.EvalLayer = LEAStrActiveX.NLayers
                    Else
                        Dim jj As Integer
                        Dim depth As Double
                        depth = 0
                        For jj = 1 To LEAStrActiveX.NLayers - 1
                            If depth <= LEAStrActiveX.EvalDepth And LEAStrActiveX.EvalDepth < depth + LEAStrActiveX.Thick(jj) Then
                                LEAStrActiveX.EvalLayer = jj
                                Exit For
                            End If
                            depth = depth + LEAStrActiveX.Thick(jj)
                        Next
                    End If
                    ' find LEAStrActiveX.EvalLayer

                    TimeSave1 = timeGetTime                    '
                    RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                    Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp) ' compute AllResp (all response) at compaction depth
                    RunLEAF = Nothing

                    RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)

                    For IAC = 1 To NAC
                        'If StressTypeRigid(IAC) = "LEAF" Then

                        StressZMax = 0.0
                        For IEval = 1 To CallAC(IAC).NEvalPoints
                            With AllResp(IAC, IEval)

                                If Math.Abs(StressZMax) <= Math.Abs(.StressZ) Then
                                    StressZMax = .StressZ
                                    '    IEvalStressZMax = IEval
                                End If
                            End With
                        Next IEval

                        CompactionStress(IAC, I) = StressZMax

                        CompactionDepth(I) = CompactionDepthLEAF(I)
                        CompactionCPRatio(IAC, I) = jobCtoPtableTemp(IAC)
                        'End If
                    Next IAC

                    I = I + 1
                Loop Until I = NSGLayer + 1
                ' ****************LEAF interior stress as the critical stress****************

            End If

            '**************************Compute CI *********************************'
            I = 1
            Do
                S = S & " Z-Depth = " & LPad(8, CompactionDepth(I).ToString("0.00")) & "      "
                'S = S & " Layer No. = " & LPad(4, Format(LEAStrActiveX.EvalLayer, "0")) & vbCrLf
                S = S & vbCrLf

                Dim CI2ACMax As Double 'CI2: the max CI for all AC (calibrated by elastic theory with n=2) 
                CI2ACMax = 0

                For IAC = 1 To NAC
                    S = S & "          Aircraft No. " & IAC.ToString("0") & "  " & CallAC(IAC).ACname
                    S = S & "       C/P Ratio = " & CompactionCPRatio(IAC, I).ToString("0.00") & vbCrLf


                    Covs = RepAdjusted * CompactionCPRatio(IAC, I)
                    'LogBETA = (1.7782 + 0.3966 * System.Math.Log(Covs, 10)) / (1 + 0.6758 * System.Math.Log(Covs, 10))
                    LogBETA = (1.7782 + 0.2397 * System.Math.Log(Covs, 10)) / (1 + 0.5031 * System.Math.Log(Covs, 10)) 'Modified for new Beta computation paramete(ERDC/GSL TR-12-16) by YGC 082012
                    BETA = 10 ^ LogBETA

                    Dim TempU As Double
                    Dim RatioN3N2 As Double

                    If CompactionDepth(I) < 0.01 * TempD Then
                        RatioN3N2 = 1
                    Else
                        TempU = 1 + (TempRadius(IAC) / CompactionDepth(I)) ^ 2 'modified to account for different tire rafius by YGC 042312
                        RatioN3N2 = (TempU - 1 / Math.Sqrt(TempU)) / (TempU - 1)
                    End If



                    CI2AC(IAC, I) = PI * Math.Abs(CompactionStress(IAC, I)) * RatioN3N2 / BETA

                    If CI2AC(IAC, I) >= CI2ACMax Then
                        CI2ACMax = CI2AC(IAC, I)
                        IDACCritical = IAC
                    End If

                    S = S & "Compaction Stress         CI" & vbCrLf
                    S = S & LPad(IPad, CompactionStress(IAC, I).ToString(RespFmt)) & LPad(8, CI2AC(IAC, I).ToString("0.00")) & vbCrLf


                Next IAC

                CI(I) = CI2ACMax

                S = S & vbCrLf & "CI at this depth: " & CI(I).ToString("0.00") & " the Critical AC: " & CallAC(IDACCritical).ACname
                S = S & vbCrLf
                S = S & vbCrLf


                CompactionDensityNC(I) = aNC - bNC * Math.Exp(-cNC * CI(I) ^ dNC)
                CompactionDensityC(I) = aC - bC * Math.Exp(-cC * CI(I) ^ dC)

                If CompactionDensityNC(I) <= DensityNCMin And CompactionDensityC(I) <= DensityCMin Then
                    NSGLayer = I
                    Exit Do
                End If

                I = I + 1
            Loop Until I = NSGLayer + 1

            '**************************Compute CI END*********************************'



            '**************************Compute Compaction Criteria *********************************'
            S = S & vbCrLf & vbCrLf
            S = S & "Pavement Thickness (in):" & LPad(8, DepthSG.ToString("0.00")) & vbCrLf & vbCrLf  'add pavement thickness by YGC 041312

            S = S & "   Compaction Depth(in)       Compaction Index     " & vbCrLf
            S = S & "  from Pavement Surface                            " & vbCrLf
            For I = 1 To NSGLayer
                S = S & "      "
                S = S & LPad(8, CompactionDepth(I).ToString("0 ")) & "                  "
                S = S & LPad(8, CI(I).ToString("0.00")) & vbCrLf
            Next I
            S = S & vbCrLf


            S = S & "           Compaction Density - NonCohesive        " & vbCrLf
            S = S & "   Compaction Depth(in)       Compaction Density(%)" & vbCrLf
            S = S & "  from Pavement Surface                            " & vbCrLf
            For I = 1 To NSGLayer
                S = S & "      "
                S = S & LPad(8, CompactionDepth(I).ToString("0 ")) & "                  "
                S = S & LPad(8, CompactionDensityNC(I).ToString("0.00")) & vbCrLf
            Next I
            S = S & vbCrLf

            S = S & "          Compaction Density - Cohesive            " & vbCrLf
            S = S & "   Compaction Depth(in)       Compaction Density(%)" & vbCrLf
            S = S & "  from Pavement Surface                            " & vbCrLf
            For I = 1 To NSGLayer
                S = S & "      "
                S = S & LPad(8, CompactionDepth(I).ToString("0 ")) & "                  "
                S = S & LPad(8, CompactionDensityC(I).ToString("0.00")) & vbCrLf
            Next I
            S = S & vbCrLf & vbCrLf



            Dim IntgerDensityNC, IntgerDensityC As Integer
            Dim tempNC, tempC As Integer
            Dim IntgerDensityDepthNC, IntgerDensityDepthC As Double

            tempNC = Int(CompactionDensityNC(1) / 5)
            IntgerDensityNC = tempNC * 5

            If IntgerDensityNC > DensityNCMax Then
                Do
                    IntgerDensityNC = IntgerDensityNC - 5
                Loop Until IntgerDensityNC <= DensityNCMax
            End If

            For JDenLevel = 1 To NDenLevel
                For I = 1 To NSGLayer - 1
                    If CompactionDensityNC(I + 1) <= IntgerDensityNC And IntgerDensityNC <= CompactionDensityNC(I) Then
                        IntgerDensityDepthNC = CompactionDepth(I) + (CompactionDepth(I + 1) - CompactionDepth(I)) * (CompactionDensityNC(I) - IntgerDensityNC) / (CompactionDensityNC(I) - CompactionDensityNC(I + 1))
                        CompactionIntDenDepthNC(JDenLevel) = IntgerDensityDepthNC
                        CompactionIntDenNC(JDenLevel) = IntgerDensityNC


                        'modified to find the critical AC at depth of compaction for noncohesive soil by YGC 112213
                        Dim CIIntDenNC(NAC), CIIntDenNCMax As Double ' CI at integer density
                        CIIntDenNCMax = 0

                        For IAC = 1 To NAC
                            CIIntDenNC(IAC) = CI2AC(IAC, I) + (CI2AC(IAC, I + 1) - CI2AC(IAC, I)) * (IntgerDensityDepthNC - CompactionDepth(I)) / (CompactionDepth(I + 1) - CompactionDepth(I))
                            If CIIntDenNC(IAC) >= CIIntDenNCMax Then
                                CIIntDenNCMax = CIIntDenNC(IAC)
                                jobCompactionIntDenCriticalACNCtable(ISect, IJob, JDenLevel) = IAC
                            End If
                        Next IAC
                        'modified to find the critical AC at depth of compaction for noncohesive soil by YGC 112213 END

                        Exit For
                    End If
                Next I
                IntgerDensityNC = IntgerDensityNC - 5
                If IntgerDensityNC < DensityNCMin Then Exit For
            Next JDenLevel



            S = S & "                      Compaction Criteria - NonCohesive                       " & vbCrLf
            S = S & "   Compaction Density(%)      Compaction Depth(in)       Compaction Depth(in) " & vbCrLf
            S = S & "                              from Pavement Surface        from Subgrade Top  " & vbCrLf

            For J = 1 To NDenLevel
                If CompactionIntDenNC(J) >= DensityNCMin Then
                    S = S & "      "
                    S = S & LPad(8, CompactionIntDenNC(J).ToString("0")) & "                  "
                    S = S & LPad(8, CompactionIntDenDepthNC(J).ToString("0")) & "                  "
                    If CompactionIntDenDepthNC(J) <= DepthSG Then
                        S = S & "      --" & vbCrLf
                    Else
                        S = S & LPad(8, (CompactionIntDenDepthNC(J) - DepthSG).ToString("0")) & vbCrLf
                    End If
                End If
            Next J

            S = S & vbCrLf


            tempC = Int(CompactionDensityC(1) / 5)
            IntgerDensityC = tempC * 5

            If IntgerDensityC > DensityCMax Then
                Do
                    IntgerDensityC = IntgerDensityC - 5
                Loop Until IntgerDensityC <= DensityCMax
            End If

            For JDenLevel = 1 To NDenLevel
                For I = 1 To NSGLayer - 1
                    If CompactionDensityC(I + 1) <= IntgerDensityC And IntgerDensityC <= CompactionDensityC(I) Then
                        IntgerDensityDepthC = CompactionDepth(I) + (CompactionDepth(I + 1) - CompactionDepth(I)) * (CompactionDensityC(I) - IntgerDensityC) / (CompactionDensityC(I) - CompactionDensityC(I + 1))
                        CompactionIntDenDepthC(JDenLevel) = IntgerDensityDepthC
                        CompactionIntDenC(JDenLevel) = IntgerDensityC


                        'modified to find the critical AC at depth of compaction for cohesive soil by YGC 112213
                        Dim CIIntDenC(NAC), CIIntDenCMax As Double ' CI at integer density
                        CIIntDenCMax = 0

                        For IAC = 1 To NAC
                            CIIntDenC(IAC) = CI2AC(IAC, I) + (CI2AC(IAC, I + 1) - CI2AC(IAC, I)) * (IntgerDensityDepthC - CompactionDepth(I)) / (CompactionDepth(I + 1) - CompactionDepth(I))
                            If CIIntDenC(IAC) >= CIIntDenCMax Then
                                CIIntDenCMax = CIIntDenC(IAC)
                                jobCompactionIntDenCriticalACCtable(ISect, IJob, JDenLevel) = IAC
                            End If
                        Next IAC
                        'modified to find the critical AC at depth of compaction for cohesive soil by YGC 112213 END

                        Exit For
                    End If
                Next I
                IntgerDensityC = IntgerDensityC - 5
                If IntgerDensityC < DensityCMin Then Exit For
            Next JDenLevel



            S = S & "                      Compaction Criteria - Cohesive                       " & vbCrLf
            S = S & "   Compaction Density(%)      Compaction Depth(in)       Compaction Depth(in) " & vbCrLf
            S = S & "                              from Pavement Surface        from Subgrade Top  " & vbCrLf

            For J = 1 To NDenLevel
                If CompactionIntDenC(J) >= DensityCMin Then
                    S = S & "      "
                    S = S & LPad(8, CompactionIntDenC(J).ToString("0")) & "                  "
                    S = S & LPad(8, CompactionIntDenDepthC(J).ToString("0")) & "                  "
                    If CompactionIntDenDepthC(J) <= DepthSG Then
                        S = S & "      --" & vbCrLf
                    Else
                        S = S & LPad(8, (CompactionIntDenDepthC(J) - DepthSG).ToString("0")) & vbCrLf
                    End If
                End If
            Next J



            For J = 1 To NDenLevel
                jobCompactionIntDenNCtable(ISect, IJob, J) = CompactionIntDenNC(J)
                jobCompactionIntDenCtable(ISect, IJob, J) = CompactionIntDenC(J)

                jobCompactionIntDenDepthNCtable(ISect, IJob, J) = CompactionIntDenDepthNC(J)
                jobCompactionIntDenDepthCtable(ISect, IJob, J) = CompactionIntDenDepthC(J)
            Next J


            S = S & vbCrLf
            LEAFCompactionText = S

            '**************************Compute Compaction Criteria *********************************'


            'added to retrive the values at the SG top by YGC 042312
            LEAStrActiveX.EvalDepth = DepthSG

            For IAC = 1 To NAC
                jobCtoPtable(ISect, IAC) = jobCtoPtableTemp(IAC)
                jobCDFtable(ISect, IAC) = jobCDFtableTemp(IAC)
                jobCDFacrftMaxtable(ISect, IAC) = jobCDFacrftMaxtableTemp(IAC)
            Next IAC
            'add ended by YGC 042312


        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub


    Public Sub LeafDesignFlexOFlex_light() 'kairat 8

        Dim IL, I As Short
        Dim TM1 As Single
        Dim CDFM1, DELT, DELCDF As Single
        Dim Overflow As Boolean, MinCount As Short
        Dim ILoop As Short
        Dim AggErr As Boolean
        'ikawa 08/28/2009
        'Static CDFSUBMAX, CDFASPMAX As Single
        Dim VertStrainReponse(,) As Double
        Dim HorizStrainResponse(,) As Double
        Dim TempD As Double
        Dim StrainMR, StrainMC As Double
        Dim IAC, NEvalPointsMax, IEval As Integer
        Dim TimeSave1 As Integer
        Dim Factor As Double
        Dim CDFtableTemp(MaxSectAC) As Double
        Dim CDFacrftMaxtableTemp(MaxSectAC) As Double ' GFH 08/13/03.

        Debug.WriteLine("")
        Debug.WriteLine("Start at " & TimeString)
        Debug.WriteLine("")

        Beeped = False
        ComputingAsphaltCDF = False
        AsphaltCDFComputed = False
        SubLayers = False
        LayerSwitch = False

        'Dim gTandemFnew As Boolean
        Dim gTandemFnewHolder As Boolean
        gTandemFnewHolder = gTandemFnew        'ED
        'end kairat


        If LifeComputation Then
            LayerSwitch = True
            LifeStr = 0.0!
        Else '2013.01.03
            SMin = CStr(NullDate)
        End If


        If gNewModulus_P154 Or gNewModulus_P154 Then 'ikawa
            LayerSwitch = True
            SubLayers = True
        End If




        MinCount = 0
        CDFErr = 10.0!
        ILoop = 0
        NStrIterations = ILoop
        Call UpdateCurrentSectData() ' To display correct
        'Call WESModulus(AggErr, SubLayers) ' sublayer information.
        Call FAAModulus(AggErr, SubLayers)

        Call UM.UpdateStructure()

        IL = 1

        Do
StartFlexOFlexLoop:
            NStrIterations = ILoop
            ILoop = ILoop + 1S
            Call UpdateCurrentSectData()
            'Call WESModulus(AggErr, SubLayers)
            Call FAAModulus(AggErr, SubLayers)

            Call UM.UpdateStructure()

            If AggErr = True Then
                S = "Error computing aggregate Modulus" & NL
                S = S & "The design has been aborted"
                Call ErrorMessageForUser(S, "Flexible Over Flexible Design")
                Exit Do
            End If

            Call WriteFile()
            Call LEDFAA_to_LEAF(DesignType)
            TimeSave1 = timeGetTime

            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
            'If ggTandemFnew Then
            'Call RunLEAF.ComputeResponse2(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            'Else
            Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)

            'End If
            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)

            If DesigningStr = False Then Exit Do ' Also JULEA error.
            Overflow = True ' CDF at subgrade.
            Call LeafCDFFlex(CDFSUBMAX, EvalDepth(1), Overflow, VertStrainReponse)

            'kawa 2015.01.12 LeafDesignFlexOFlex_light


            'If CDFSUBMAX < 0.01 And (Thick(1) > ThickMin(10)) Then
            'If CDFSUBMAX < 0.01 And (Thick(1) > 2) Then

            If CDFSUBMAX < 0.01 And (Thick(1) > ThickMin(LCode(1))) Then   'ED temporarily commented this original code
                'Thick(1) = Thick(1) * 0.75             
                Thick(1) = Thick(1) * 0.9 'kairat 

                If Thick(1) < ThickMin(LCode(1)) Then
                    Thick(1) = ThickMin(LCode(1))
                End If
                GoTo StartFlexOFlexLoop

            End If


            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
            Dim k1, k2, k3 As Integer

            For k1 = 1 To NAC + 1
                For k2 = 1 To 41
                    CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
                Next
            Next
            If LifeComputation Then
                LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
                If LifeCounterForPCRRuns = 1 Then
                    For k1 = 1 To NAC + 1
                        For k2 = 1 To 41
                            CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
                        Next
                    Next
                End If
                For I = 1 To NAC
                    CDFtableTemp(I) = jobCDFtable(ISect, I)
                    CDFacrftMaxtableTemp(I) = jobCDFacrftMaxtable(ISect, I)
                    CDFtableTemp3(I) = CDFtableTemp(I)
                    CDFacrftMaxtableTemp3(I) = CDFacrftMaxtableTemp(I)
                Next
            End If

            'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

            CDFPic = CDFSUBMAX

            System.Diagnostics.Debug.WriteLine(ILoop & LayerSwitch & Thick(IL) & EvalDepth(1) & "CDF = " & CDFSUBMAX)
            CDFErr = CSng(System.Math.Abs(System.Math.Log(CDFSUBMAX))) 'Abs(1! - CDFSUBMAX)
            If CDFErr < CDFErrCntrl And SubLayers And Not LayerSwitch Then
                LayerSwitch = True ' Used in WESModulus and ModulusThick.
                CDFErr = 10.0! ' Used in ModulusThick. Not really needed here.
                GoTo StartFlexOFlexLoop
            End If

            If CDFErr < CDFExitErr Then
                Exit Do
            End If

            If Thick(IL) = ThickMin(LCode(IL)) And CDFSUBMAX < 1.0! Then
                'Thick(1) = 0.0 : GoTo EndSub 'kairat 8.2
            End If

            If Overflow Then
                '     Only change the thickness of the overlay layer.
                Thick(IL) = CSng(Thick(IL) * 0.5)
                If Thick(IL) < ThickMin(LCode(IL)) Then
                    Thick(IL) = ThickMin(LCode(IL))
                End If
                GoTo StartFlexOFlexLoop
            End If

            TM1 = Thick(IL)
            CDFM1 = CSng(System.Math.Log(CDFSUBMAX))
            DELT = CSng(Thick(IL) * 0.01)
            Thick(IL) = Thick(IL) + DELT
            Call UpdateCurrentSectData()
            Call FAAModulus(AggErr, SubLayers)

            Call UM.UpdateStructure()

            If AggErr Then
                S = "Error computing aggregate Modulus" & NL
                S = S & "The design has been aborted"
                Call ErrorMessageForUser(S, "Flexible Over Flexible Design")
                Exit Do
            End If
            '    Debug.Print julNPLayers
            Call WriteFile()
            Call LEDFAA_to_LEAF(DesignType)
            TimeSave1 = timeGetTime

            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
            'If gTandemFnew Then ' kairat replace tandem
            'Call RunLEAF.ComputeResponse2(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            'Else
            Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.VerticalStrain, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            'End If

            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)

            If DesigningStr = False Then Exit Do ' Also JULEA error.
            Overflow = True ' CDF at subgrade.
            Call LeafCDFFlex(CDFSUBMAX, EvalDepth(1), Overflow, VertStrainReponse)

            '3333333333
            DELCDF = CSng(System.Math.Log(CDFSUBMAX) - CDFM1)

            If DELCDF > 0.0! Then
                If CDFSUBMAX > 1.0! Then
                    Thick(IL) = Thick(IL) * 2.0!
                    CDFErr = 10.0!
                    GoTo StartFlexOFlexLoop
                Else
                    'Thick(1) = 0.0 : GoTo EndSub 'kairat 8.3
                End If
            End If

            '   Control overshoot with far-from-design structures.
            If -CDFErrCntrl < CDFM1 And CDFM1 < CDFErrCntrl Then ' .2 < CDF < 5.0
                Factor = 1.0#
            ElseIf -(CDFErrCntrl + 1) < CDFM1 And CDFM1 < (CDFErrCntrl + 1) Then  ' .02 < CDF < 50.0
                Factor = 0.95
            Else
                Factor = 0.6
            End If

            DELT = CSng((-CDFM1 * DELT / DELCDF) * Factor)

            '    Debug.Print Thick(IL); DELT; CDFASPMAX; CDFSUBMAX
            If TM1 + DELT < ThickMin(LCode(IL)) Then
                System.Diagnostics.Debug.WriteLine(TM1 & DELT)
                Thick(IL) = ThickMin(LCode(IL))
            Else
                Thick(IL) = TM1 + DELT
                '      If Abs(DELT) < .1 Then Exit Do
            End If

            If ILoop > 25 Then
                'Thick(1) = 0.0
                Thick(1) = Thick(2) 'kairat
                GoTo EndSub 'kairat 8.4
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


        'to preserve CDF for subgrade
        For I = 1 To MaxSects
            For J = 1 To MaxSectAC
                jobCDFtableSub(I, J) = jobCDFtable(I, J)
                jobCDFacrftMaxtableSub(I, J) = jobCDFacrftMaxtable(I, J)
                jobCtoPtableSub(I, J) = jobCtoPtable(I, J)
            Next J
        Next I


        If OutPutFile Then
            '   Write subgrade strain data to file.
            TimeSave1 = timeGetTime


            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
            Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, VertStrainReponse, AllResp)
            RunLEAF = Nothing

            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            Call WriteLEAFData(LEAFOutputText)
            SS = WorkingDir & "LeafSG.out"
            If Dir(SS) <> "" Then Kill(SS)
            I = CShort(FreeFile())
            FileOpen(I, SS, OpenMode.Output)
            PrintLine(I, LEAFOutputText)
            FileClose((I))
        End If

        SS = ACDATPath & "DAMTMPSG.DAT"
        If Dir(SS) <> "" Then Kill(SS)
        S = ACDATPath & "DAMTMP.DAT"
        If Dir(S) <> "" Then Rename(S, SS)

        AsphaltCDFComputed = False
        If NoACCDF = False Then
            If DesigningStr = True Then ' Get asphalt CDF.
                ComputingAsphaltCDF = True
                EvalDepth(1) = -julThick(1) + 0.01
                Call WriteFile() ' Write new EvalDepth.
                Call LEDFAA_to_LEAF(DesignType)

                RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                RunLEAF = Nothing

                NEvalPointsMax = UBound(AllResp, 2)
                ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                For IAC = 1 To NAC
                    For IEval = 1 To CallAC(IAC).NEvalPoints
                        StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                        StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                        TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                        If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                            HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                        Else
                            HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                        End If

                        ' - sign: compression
                        ' + sign: tension
                        If HorizStrainResponse(IAC, IEval) < 0 Then
                            HorizStrainResponse(IAC, IEval) = 0.0
                        End If

                    Next IEval
                Next IAC

                If DesigningStr = True Then ' Also JULEA error.
                    Call UpdateCurrentSectData()
                    Overflow = False ' CDF in asphalt.

                    'begin kairat
                    gTandemFnew = False
                    Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                    Call UM.UpdateStructure()

                    gTandemFnew = gTandemFnewHolder
                    'end kairat

                    CDFAsp = CDFASPMAX
                    CDFASPMAX1 = CDFASPMAX 'ikawa 999

                    System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
                    AsphaltCDFComputed = True
                End If

                'CDF values for the overlay layer
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtableAC(I, J) = jobCDFtable(I, J)
                        jobCDFacrftMaxtableAC(I, J) = jobCDFacrftMaxtable(I, J)
                        jobCtoPtableAC(I, J) = jobCtoPtable(I, J)
                    Next J
                Next I

                '123+++++++++++++++++++++++++++++++++++++++++++++++++++++++++
                If DesignType = FlexOnFlex Then
                    ComputingAsphaltCDF = True

                    EvalDepth(1) = -(julThick(1) + julThick(2)) + 0.01
                    'EvalDepth(1) = kk
                    Call WriteFile() ' Write new EvalDepth.
                    Call LEDFAA_to_LEAF(DesignType)

                    LEAStrActiveX.EvalLayer = 2
                    RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                    Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                    RunLEAF = Nothing

                    NEvalPointsMax = UBound(AllResp, 2)
                    ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                    For IAC = 1 To NAC
                        For IEval = 1 To CallAC(IAC).NEvalPoints
                            StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                            StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                            TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                            If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                                HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                            Else
                                HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                            End If

                            ' - sign: compression
                            ' + sign: tension
                            If HorizStrainResponse(IAC, IEval) < 0 Then
                                HorizStrainResponse(IAC, IEval) = 0.0
                            End If

                        Next IEval
                    Next IAC

                    If DesigningStr = True Then ' Also JULEA error.
                        Call UpdateCurrentSectData()
                        Overflow = False ' CDF in asphalt.
                        'begin kairat
                        gTandemFnew = False
                        Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                        Call UM.UpdateStructure()

                        gTandemFnew = gTandemFnewHolder
                        'end kairat

                        If CDFASPMAX1 > CDFASPMAX Then
                            CDFASPMAX = CDFASPMAX1
                        End If

                        CDFAsp = CDFASPMAX
                        System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
                        AsphaltCDFComputed = True
                    End If


                End If

                'CDF values for HMA under HMA overlay
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtableHMA(I, J) = jobCDFtable(I, J)
                        jobCDFacrftMaxtableHMA(I, J) = jobCDFacrftMaxtable(I, J)
                        jobCtoPtableHMA(I, J) = jobCtoPtable(I, J)
                    Next J
                Next I


                '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                'reinstating CDF for subgrade layer
                For I = 1 To MaxSects
                    For J = 1 To MaxSectAC
                        jobCDFtable(I, J) = jobCDFtableSub(I, J)
                        jobCDFacrftMaxtable(I, J) = jobCDFacrftMaxtableSub(I, J)
                        jobCtoPtable(I, J) = jobCtoPtableSub(I, J)
                    Next J
                Next I

                'start for 401 stabilized base +++++++++++++++++++++++++++++++++
                If (DesignType = FlexOnFlex) And (LCode(3) = 14) Then
                    ComputingAsphaltCDF = True
                    EvalDepth(1) = -(julThick(1) + julThick(2) + julThick(3)) + 0.01
                    'EvalDepth(1) = kk
                    Call WriteFile() ' Write new EvalDepth.
                    Call LEDFAA_to_LEAF(DesignType)

                    LEAStrActiveX.EvalLayer = 3
                    RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                    Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.AllResponses, CShort(NAC), CallAC, LEAStrActiveX, HorizStrainResponse, AllResp)
                    RunLEAF = Nothing

                    NEvalPointsMax = UBound(AllResp, 2)
                    ReDim HorizStrainResponse(NAC, NEvalPointsMax)

                    For IAC = 1 To NAC
                        For IEval = 1 To CallAC(IAC).NEvalPoints
                            StrainMC = (AllResp(IAC, IEval).StrainX + AllResp(IAC, IEval).StrainY) / 2
                            StrainMR = (AllResp(IAC, IEval).StrainX - AllResp(IAC, IEval).StrainY) / 2
                            TempD = System.Math.Sqrt(StrainMR ^ 2 + AllResp(IAC, IEval).StrainXY ^ 2)

                            If System.Math.Abs(StrainMC + TempD) > System.Math.Abs(StrainMC - TempD) Then
                                HorizStrainResponse(IAC, IEval) = StrainMC + TempD
                            Else
                                HorizStrainResponse(IAC, IEval) = StrainMC - TempD
                            End If

                            ' - sign: compression
                            ' + sign: tension
                            If HorizStrainResponse(IAC, IEval) < 0 Then
                                HorizStrainResponse(IAC, IEval) = 0.0
                            End If

                        Next IEval
                    Next IAC

                    If DesigningStr = True Then ' Also JULEA error.
                        Call UpdateCurrentSectData()
                        Overflow = False ' CDF in asphalt.
                        'begin kairat
                        gTandemFnew = False
                        Call LeafCDFFlex(CDFASPMAX, EvalDepth(1), Overflow, HorizStrainResponse)

                        Call UM.UpdateStructure()

                        gTandemFnew = gTandemFnewHolder
                        'end kairat

                        If CDFASPMAX1 > CDFASPMAX Then
                            CDFASPMAX = CDFASPMAX1
                        End If

                        CDFAsp = CDFASPMAX
                        System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
                        AsphaltCDFComputed = True
                    End If


                    'CDF values for HMA under HMA overlay
                    For I = 1 To MaxSects
                        For J = 1 To MaxSectAC
                            jobCDFtable401(I, J) = jobCDFtable(I, J)
                            jobCDFacrftMaxtable401(I, J) = jobCDFacrftMaxtable(I, J)
                            jobCtoPtable401(I, J) = jobCtoPtable(I, J)
                        Next J
                    Next I

                    'reinstating CDF for subgrade layer
                    For I = 1 To MaxSects
                        For J = 1 To MaxSectAC
                            jobCDFtable(I, J) = jobCDFtableSub(I, J)
                            jobCDFacrftMaxtable(I, J) = jobCDFacrftMaxtableSub(I, J)
                            jobCtoPtable(I, J) = jobCtoPtableSub(I, J)
                        Next J
                    Next I

                End If

                'end for 401 stabilized base ++++++++++++++++++++++++++++++++++++


            End If 'If DesigningStr = True Then ' Get asphalt CDF.

            If OutPutFile Then

                Call WriteLEAFData(LEAFOutputText)

                SS = WorkingDir & "LeafAC.out"

                If Dir(SS) <> "" Then Kill(SS)

                I = CShort(FreeFile())
                FileOpen(I, SS, OpenMode.Output)
                PrintLine(I, LEAFOutputText)
                FileClose((I))
            End If



        Else '------------------------------------   If NoACCDF = False Then
            CDFAsp = -1 : System.Diagnostics.Debug.WriteLine("CDFAsp = " & CDFAsp)
        End If
        '   Write asphalt strain data to file.


        ComputingAsphaltCDF = False
EndSub:
    End Sub




End Module