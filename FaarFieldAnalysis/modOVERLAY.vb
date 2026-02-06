Option Strict On
Option Explicit On
Public Module OVERLAY
    ' T1min:  Time (in years) needed for reaching SCI = 100 for the existing pavement
    ' T2min:  Time (in years) needed for reducing  SCI of the new overlay to the required value
    ' Pavement and traffic information is in "InitSamples"

    Sub Calculate_CurDT(ByRef LastTT As Single, ByRef CurDT As Single, ByRef CurSCI As Single, ByRef PCC_Stress(,) As Single)
        Dim LogCA2 As Single, LogCAB As Single
        Dim Passes As Single, J As Integer, DT(80) As Single, DSum As Single
        Dim TimeA, Time1, Time2, time As Single, DT1 As Single

        DSum = 0

        For J = 1 To NAC + NextraAC
            LogCA2 = CSng(LogCAE(CurSCI, RCon(2), PCC_Stress(J, 2), 1, 1))
            'LogCAB = LogCAE(SCIB, RCon(2), PCC_Stress(J, 2), 1, 1)
            LogCAB = CSng(LogCAE(CurSCI + SCIB / NSection, RCon(2), PCC_Stress(J, 2), 1, 1))
            Passes = CSng((10 ^ LogCA2) / CtoP(J, IControl) - (10 ^ LogCAB) / CtoP(J, IControl))

            If RepsInc(J) = 0 Then
                DT(J) = Passes / RepsAnnual(J) ' - LastTT

            ElseIf RepsInc(J) <> 0 Then

                TimeA = Passes / RepsAnnual(J)
                time = TimeA
                DT1 = DREP(0 + T1min, time + T1min, J)

                If DT1 < Passes Then
                    Do
                        time = CSng(time * 1.1)
                        DT1 = DREP(0 + T1min, time + T1min, J)
                    Loop Until DT1 > Passes
                    Time1 = TimeA
                    Time2 = time
                Else
                    Do
                        time = CSng(time * 1 / 1.1)
                        DT1 = DREP(0 + T1min, time + T1min, J)
                    Loop Until DT1 < Passes
                    Time1 = time
                    Time2 = TimeA
                End If

                Do Until Math.Abs(DT1 - Passes) < 0.2
                    time = (Time1 + Time2) / 2
                    DT1 = DREP(0 + T1min, time + T1min, J)
                    If DT1 > Passes Then
                        Time2 = time
                    Else
                        Time1 = time
                    End If
                Loop

                DT(J) = time - LastTT
            End If

            DSum = 1 / DT(J) + DSum
        Next J

        CurDT = 1 / DSum

    End Sub



    Sub Calculate_Tleft(ByRef Tleft0 As Single, ByRef Coverages() As Single, ByRef CDF1 As Single)
        Dim DSum As Single, TT As Single
        Dim CDF_Left As Single

        CDF_Left = 1 - CDF1

        DSum = 0

        For J = 1 To NAC + NextraAC
            If RepsInc(J) = 0 Then
                TT = Coverages(J) * CDF_Left / CtoP(J, IControl) / RepsAnnual(J)
                'TT = Coverages(J) / CtoP(J, IControl) / RepsAnnual(J)
            End If
            DSum = 1 / TT + DSum
        Next J
        Tleft0 = 1 / DSum

    End Sub



    Function DCDF(ByRef CurDT As Single, ByRef LastTT As Single, ByRef CaEqual() As Single) As Single
        Static R(MaxSectAC) As Single
        Static C As Single
        Static J As Integer
        Static T1, T2 As Single
        C = 0
        T1 = LastTT
        T2 = LastTT + CurDT
        For J = 1 To NAC + NextraAC 'change2
            R(J) = DREP(T1, T2, J)
            C = C + R(J) * CtoP(J, IControl) / CaEqual(J)
        Next J
        DCDF = C
    End Function

    Function DREP(ByRef T1 As Single, ByRef T2 As Single, ByRef J As Integer) As Single
        '   Modified on 7-27-94 for considering RepsInc(j) < 0
        '   Total number of repetitions is an arithmetic series
        '   but not a geometric series as assumed previously
        '  If RepsInc(j) <> 0 Then
        '    DREP = RepsAnnual(j) / RepsInc(j) * ((1 + RepsInc(j)) ^ T2 - (1 + RepsInc(j)) ^ T1)
        '  Else
        '    DREP = (T2 - T1) * RepsAnnual(j)
        '  End If

        Dim A1, A2 As Single

        If System.Math.Abs(RepsInc(J)) < 0.00000001 Then
            DREP = (T2 - T1) * RepsAnnual(J)
        Else
            If T2 = 0 Then
                A2 = 0

            Else
                If RepsInc(J) >= -1 / T2 Then
                    A2 = RepsAnnual(J) * T2 * (1 + RepsInc(J) * T2 / 2)
                Else
                    A2 = -RepsAnnual(J) / RepsInc(J) / 2
                End If
            End If

            If T1 = 0 Then
                A1 = 0
            Else
                If RepsInc(J) >= -1 / T1 Then
                    A1 = RepsAnnual(J) * T1 * (1 + RepsInc(J) * T1 / 2)
                Else
                    A1 = -RepsAnnual(J) / RepsInc(J) / 2
                End If
            End If

            DREP = A2 - A1

        End If

    End Function

    Function DSCI(ByRef CurDT As Single) As Double
        ' DSCI is difference between calculated and required SCI.
        ' LS was introduced for considering the change of starting SCI
        ' for each aircraft.

        Static CurTT As Single
        Static LC2, G, DS, LC1, C0 As Single

        'Debug.Print "in DSCI"

        CurTT = LastTT + CurDT
        DS = 0
        For J = 1 To NAC + NextraAC 'change2
            G = CSng((LogCE(J) - LogCA(J)) / 100)
            C0 = CSng(10 ^ ((100 - SCIB) * G + LogCA(J)))
            '    LC2 = DREP(0, CurTT, J)
            '    LC1 = DREP(0, LastTT, J)
            '    Print #FileGuo, "CurTT="; CurTT; "LastTT="; LastTT
            '    Debug.Print "DREP(0,CurTT,J)="; LC2; "DREP(0,LastTT,J)="; LC1
            LC2 = CSng(System.Math.Log(DREP(0, CurTT, J) * CtoP(J, IControl) + C0) / System.Math.Log(10))
            LC1 = CSng(System.Math.Log(DREP(0, LastTT, J) * CtoP(J, IControl) + C0) / System.Math.Log(10))
            '    Print #FileGuo, "in DSCI, G = ", G
            '    Print #FileGuo, "LC1 = ", LC1, "LC2 =", LC2
            DS = DS + (LC2 - LC1) / G
            '    Debug.Print "LogCA(J)="; LogCA(J); "LogCE(J)="; LogCE(J)
            '    Debug.Print "SCIB="; SCIB; "  "; "C0="; C0
            '    Debug.Print "LC1="; LC1; "LC2="; LC2
            '    Debug.Print "CPH(J,IControl)="; CPH(J, IControl)
        Next J

        If LayerType(LCode(1)) = NACO Or LayerType(LCode(1)) = NND Then
            G = DS - (SCIB - SCIE)
        Else
            G = DS - SCIB / NSection
        End If

        DSCI = G
        '  Debug.Print "DSCI="; G

    End Function


    Function LeafLifeTotal13(ByRef Th As Single, ByRef NextraACout As Short) As Single

        Dim CDFMAX As Single
        Dim Overflow As Boolean
        Dim NumIteration As Short, CurMidSCI As Single, E0 As Single
        Dim AggErr As Boolean, I As Short
        Dim LastCDF, EE0, CurCDF As Single
        '  Dim NextraAC As Integer is a Module level variable set in this sub by LEDFAA_to_LEAF_Rigid
        '                          used in many of the module's subs and functions. It gives
        '                          the number of extra aircraft added by multiple-gear aircraft
        '                          such as the A380 (wing is normal list, body is extra aircraft).
        Dim HorizStressResponse(,) As Double
        Dim TimeSave1 As Integer

        'GUO1210  Static STRS!(MaxSectAC, 2), LogCA2!(MaxSectAC)
        '  Static STRS!(MaxSectAC, 2) ' GFH 02/26/03.
        Dim STRS(MaxSectAC, 2) As Single
        '  Static CallowA0!(MaxSectAC), CallowE0!(MaxSectAC), CaEqual0!(MaxSectAC) ' GFH 02/26/03.
        Dim CallowA0(MaxSectAC) As Single
        Dim CallowE0(MaxSectAC) As Single
        Dim CaEqual0(MaxSectAC) As Single
        Dim LastTT0, CurSCI0, LastSCI0, Tleft As Single

        NextraAC = 0 ' See Sub LeafLifeTotal. GFH 02-26-03.
        NSection = DefaultNSection
        julThick(1) = Th
        Thick(1) = Th

        'GUO1209---------------------------------------------------
        '  Print #FileGuo, "LeafLifeTotal, Th=", Th; "SCIB,SCIE,EXISTLIFE="; SCIB; SCIE; LifeExistPCC
        'GUO1209---------------------------------------------------
        If AggErr = True Then
            Ret = MsgBoxDQ("AggErr in LeafLifeTotal 1", 0, "AggErr")
            System.Diagnostics.Debug.WriteLine("  Error Message: AggErr = True in LeafLifeTotal  ")
            Exit Function
        End If

        If LayerType(LCode(1)) = NACO Or LayerType(LCode(1)) = NND Then
            NEvalDepths = 1
            EvalDepth(1) = -Thick(1) - Thick(2)
            '    Debug.Print "EvalDepth(1)=  "; EvalDepth(1)
        Else
            NEvalDepths = 2
            EvalDepth(1) = -Thick(1)
            EvalDepth(2) = -Thick(1) - Thick(2)
        End If

        For I = 1 To NEvalDepths
            Call WriteFile()
            Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
            LEAStrActiveX.EvalDepth = System.Math.Abs(EvalDepth(I))
            LEAStrActiveX.EvalLayer = I
            If LayerType(LCode(1)) = NACO Or LayerType(LCode(1)) = NND Then LEAStrActiveX.EvalLayer = 2
            TimeSave1 = timeGetTime
            '------------------------
            RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
            Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse, AllResp)
            RunLEAF = Nothing


            'JobTimeDir_SectDir_IterDir = AppDir & "\" & JobName & " " & ResultsDir & "\" & SectName & "\" & Iter
            'Dim Response(,) As Double
            'Dim RunAM As New AMClassLib.clsAM()       'Instance of AutoMesh.
            'Call RunAM.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, Response, AllResp, CalcStress)

            ''''ReDim Response(1, 2)
            ''''Response(1, 1) = 196.991678242148
            ''''Response(1, 2) = 180.072383091498

            'Iter += 1

            'ReDim HorizStressResponse(NAC + NextraAC, 1)
            'Dim ii As Integer
            'For ii = 1 To NAC + NextraAC
            'OverlayStress(ii) = Response(ii, 1)
            'PCCStress(ii) = Response(ii, 2)

            'HorizStressResponse(ii, 1) = Response(ii, 1)
            'Next ii


            RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            Call LeafCDFRigid13(CDFMAX, Overflow, HorizStressResponse) ' Sets STRSH() = Max HorizStressResponse() converted to edge stress for all aircraft.
            For J = 1 To CShort(NAC + NextraAC)
                STRS(J, I) = STRSH(J)
                '      Debug.Print I; J; STRS(J, I)
            Next J

            'If NEvalDepths = 2 Then
            '    ReDim HorizStressResponse(NAC + NextraAC, 1)
            '    For ii = 1 To NAC + NextraAC
            '        'HorizStressResponse(ii, 1) = Response(ii, 2)
            '    Next ii

            '    RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
            '    Call LeafCDFRigid13(CDFMAX, Overflow, HorizStressResponse) ' Sets STRSH() = Max HorizStressResponse() converted to edge stress for all aircraft.
            '    For J = 1 To CShort(NAC + NextraAC)
            '        STRS(J, 2) = STRSH(J) 'ikawa 
            '        '      Debug.Print I; J; STRS(J, I)
            '    Next J
            'End If

        Next I





        '  Call WriteJuleaFile ' GFH 02-26-02
        '  Call JuleaCalc(JuleaErr$) ' GFH 02-26-02
        '  If DesigningStr = False Then Exit Function ' GFH 02-26-02
        '  Call CDFRigid(CDFMAX, Overflow) ' GFH 02-26-02
        '  Debug.Print "Ret1" ' GFH 02-26-02
        '--- Ca and Ce of existing slab are calculated based on
        '--- Compensation for interior-to-edge stress only
        '  Call ReadInDamtmp(STRS!()) ' GFH 02-26-02
        'GUO1210  --------------------------------
        Call CompforStab(FSlope, 3)
        FSlopeComp = CSng((0.392 - FSlope * 0.3881) / (FSlope * 0.0039))
        'Print #FileGuo, "FSlope=", FSlope, "FSlopeComp=", FSlopeComp
        If LayerType(LCode(1)) = NACO Or LayerType(LCode(1)) = NND Then
            For J = 1 To CShort(NAC + NextraAC)
                '    Print #FileGuo, "STRS(J,1)="; STRS(j, 1)
                If STRS(J, 1) < 1.0! Then
                    CallowA(J) = 1.0E+20
                    CallowE(J) = 1.0E+30
                    GoTo 89
                End If
                'GUO1210  Compensation of Stabilized base will not be considered
                LogCA(J) = LogCAE(100, RCon(2), STRS(J, 1), 1, 1)
                If LogCA(J) > 20 Then LogCA(J) = 20
                '   Print #FileGuo, "LogCA(j) = ", LogCA(j)
                CallowA(J) = CSng(10 ^ LogCA(J))
                LogCE(J) = LogCAE(0, RCon(2), STRS(J, 1), 1, 1)
                If LogCE(J) > 30 Then LogCE(J) = 30
                '   Print #FileGuo, "LogCE(j) = ", LogCE(j)
                CallowE(J) = CSng(10 ^ LogCE(J))
89:
            Next J
        Else
            For J = 1 To NAC + NextraAC
                If STRS(J, 2) < 1.0# Then
                    CallowA(J) = 1.0E+20
                    CallowE(J) = 1.0E+30
                    GoTo 88
                End If
                'Print #FileGuo, "STRS(j,2)=", STRS(j, 2)
                LogCA(J) = LogCAE(100, RCon(2), STRS(J, 2), 1, 1)
                If LogCA(J) > 20 Then LogCA(J) = 20
                LogCE(J) = LogCAE(0, RCon(2), STRS(J, 2), 1, 1)
                If LogCE(J) > 30 Then LogCE(J) = 30
                CallowA(J) = CSng(10 ^ LogCA(J))
                CallowE(J) = CSng(10 ^ LogCE(J))
                'Print #FileGuo, "Before 88,  CallowA(j)= ", CallowA(j), "CallowE(j) = ", CallowE(j)
88:
            Next J
        End If
        '  Debug.Print "RCon(1)="; Rcon(1); "RCon(2)="; Rcon(2)
        '  Debug.Print "STRS(1, 1)="; STRS(1, 1); "STRS(1, 2)="; STRS(1, 2)
        '  Debug.Print "CallowA(1)="; CallowA(1); "CallowE(1)="; CallowE(1)
        '  Debug.Print "LogCA(1)="; LogCA(1); "LogCE(1)="; LogCE(1)
        '   ----------------------------------------------------
        CurTT = 0
        CurCDF = 0
        LastCDF = 0
        If LifeExistPCC < 100 Then
            Call LifeExist(T1min, LifeExistPCC, CallowA)
            '   -----------------------------------------------------
            '   Calculating CDF of overlay in period "T1min"
            S = LayerType(LCode(1))
            If S = NPCCOU Or S = NPCCOB Then
                'GUO1210--The remaining life of exisitng PCC slab uses same
                'GUO1210--compensation model as the new PCC pavement
                Call CompforStab(FSlope, 3)
                'Debug.Print "  FSlope = ", FSlope
                FSlopeComp = CSng((0.392 - FSlope * 0.3881) / (FSlope * 0.0039))
                'Debug.Print "  FSlope = ", FSlope, "  FSlopeComp = ", FSlopeComp
                For J = 1 To CShort(NAC + NextraAC)
                    If STRS(J, 1) < 1.0# Then
                        CaEqual(J) = 1.0E+20
                        GoTo 96
                    End If

                    LogCA2(J) = LogCAE(SCIE, RCon(1), STRS(J, 1), FSlope, FSlopeComp)
                    If LogCA2(J) > 30.0# Then LogCA2(J) = 30.0#
                    CaEqual(J) = CSng(10 ^ LogCA2(J))
96:
                Next J
                '      CurDT = T1min
                'UPGRADE_WARNING: Couldn't resolve default property of object DCDF(). Click for more: 'ms-help://MS.VSCC/commoner/redir/redirect.htm?keyword="vbup1037"'
                CurCDF = DCDF(T1min, LastTT, CaEqual)
            End If
            '   -----------------------------------------------------
        Else
            '    CurCDF = 0!
            T1min = 0.0!
        End If
        '  Debug.Print "T1min="; T1min; "  CurCDF="; CurCDF
        '  Debug.Print "STRS(1,1)="; STRS(1, 1); "STRS(1,2)="; STRS(1, 2)
        LastTT = 0
        T2min = 0
        If LayerType(LCode(1)) = NACO Or LayerType(LCode(1)) = NND Then
            '   ------------------------------------------------------
            LastSCI = SCIB
            CurSCI = SCIE
            CurDT = DTInit
            'Print #FileGuo, " Before SCIReduction"
            Call SCIReduction(LastTT, CurDT, CallowA, CallowE, LastSCI, CurSCI, NSection)
            'Print #FileGuo, "After SCIReduction"
            T2min = CurDT
            LeafLifeTotal13 = T1min + T2min + Th - Life
            '    Print #FileGuo, "T1min = "; T1min; "     T2min= "; T2min
            GoTo 701
            '   ------------------------------------------------------
            '   End of Flexible Overlay on Rigid PCC Pavement Analysis
        Else
            LastCDF = CurCDF
            NumIteration = 0
            ' No coverage reduction in the first iteration
            LastSCI = SCIB
            CurSCI = LastSCI - SCIB / NSection
            CurMidSCI = SCIB - SCIB / NSection / 2
            'Print #FileGuo, "LastSCI= "; LastSCI; "  CurSCI= "; CurSCI;
            'Print #FileGuo, " CurMidSCI= "; CurMidSCI
            E0 = Modulus(2)
            EE0 = E0
            Call UpdateCurrentSectData()
            'Call WESModulus(AggErr, SubLayers)
            Call FAAModulus(AggErr, SubLayers)

            julModulus(2) = CSng(E0 * (0.002 + 0.0064 * CurMidSCI + (0.00584 * CurMidSCI) ^ 2))
            '    julModulus(2) = E0
            'Print #FileGuo, "julModulus(2) = ", julModulus(2)
            If AggErr = True Then
                Ret = MsgBoxDQ("AggErr in LeafLifeTotal 2", 0, "AggErr")
                Exit Function
            End If
            NEvalDepths = 1
            EvalDepth(1) = -Thick(1)



            For I = 1 To NEvalDepths
                Call WriteFile()
                Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
                LEAStrActiveX.EvalDepth = System.Math.Abs(EvalDepth(I))
                LEAStrActiveX.EvalLayer = I
                If LayerType(LCode(1)) = NACO Or LayerType(LCode(1)) = NND Then LEAStrActiveX.EvalLayer = 2
                TimeSave1 = timeGetTime

                '----------------------
                RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse, AllResp)
                RunLEAF = Nothing

                'JobTimeDir_SectDir_IterDir = AppDir & "\" & JobName & " " & ResultsDir & "\" & SectName & "\" & Iter
                'RunAM = New AMClassLib.clsAM()
                'Call RunAM.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, Response, AllResp, CalcStress)

                ''ReDim Response(1, 1)
                ''Response(1, 1) = 298.612306513599

                'Iter += 1

                ''''ReDim HorizStressResponse(NAC + NextraAC, 1)
                'For ii = 1 To NAC + NextraAC
                '    'OverlayStress(ii) = Response(ii, 1)
                '    'PCCStress(ii) = Response(ii, 2)

                '    'HorizStressResponse(ii, 1) = Response(ii, 1)
                'Next ii


                RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                Call LeafCDFRigid13(CDFMAX, Overflow, HorizStressResponse) ' Sets STRSH() = Max HorizStressResponse() converted to edge stress for all aircraft.
                For J = 1 To CShort(NAC + NextraAC)
                    STRS(J, I) = STRSH(J)
                Next J
            Next I



            '    Call WriteJuleaFile ' GFH 02-26-02
            '    Call JuleaCalc(JuleaErr$) ' GFH 02-26-02
            '    If DesigningStr = False Then Exit Function ' GFH 02-26-02
            '    Call CDFRigid(CDFMAX, Overflow) ' GFH 02-26-02
            'Debug.Print "Ret2" ' GFH 02-26-02

            'GUO1210  PCC Overlay uses the same compesation for new PCC slab
            'Print #FileGuo, " STRSH(1) = ", STRSH(1)
            'GUO1210    Call ReadInDamtmp(STRS())
            'Print #FileGuo, "  STRS(1,1) = ", STRS(1, 1)
            'GUO1210    Call CompforStab(FSlope, 3)
            'GUO1210    FSlopeComp = (.392 - FSlope * .3881) / (FSlope * .0039)
            'Print #FileGuo, "FSlope =", FSlope, " FSlopeComp = ", FSlopeComp
            For J = 1 To CShort(NAC + NextraAC)
                If STRS(J, 1) < 1.0# Then
                    CaEqual(J) = CSng(1.0E+20 * (1 - CurCDF))
                    GoTo 93
                End If
                '      LogCA2(j) = LogCAE(SCIE, Rcon(1), STRS(j, 1), FSlope, FSlopeComp)
                If LogCA2(J) > 20 Then LogCA2(J) = 20
                CaEqual(J) = CSng(10 ^ LogCA2(J) * (1 - CurCDF))
93:
            Next J
            'Print #FileGuo, "After 93,  LogCA2(1) = ", LogCA2(1)
            For J = 1 To CShort(NAC + NextraAC)
                CallowA0(J) = CallowA(J)
                CallowE0(J) = CallowE(J)
                CaEqual0(J) = CaEqual(J)
            Next J
            LastSCI0 = LastSCI
            CurSCI0 = CurSCI
            LastTT0 = LastTT

1000:
            NumIteration = NumIteration + 1S
            CurDT = DTInit
            'Print #FileGuo, "after 1000"
            'Print #FileGuo, "NumIteration =", NumIteration, "LastTT=", LastTT
            'Print #FileGuo, "CallowE(1)= ", CallowE(1), "LastSCI=", LastSCI
            'Print #FileGuo, "CurSCI=", CurSCI, "Nsection = ", Nsection
            Call SCIReduction(LastTT, CurDT, CallowA, CallowE, LastSCI, CurSCI, NSection)
            'Debug.Print " after SCIReduction  "
            CurTT = LastTT + CurDT
            CurCDF = DCDF(CurDT, LastTT, CaEqual)
            'Print #FileGuo, "CurCDF = ", CurCDF
            System.Diagnostics.Debug.WriteLine("CurCDF = " & CurCDF)
            If CurCDF <= 1 Then
                NSection = DefaultNSection
                LastCDF = CurCDF
                LastTT = CurTT
                LastSCI = CurSCI
                CurSCI = LastSCI - SCIB / NSection
                CurMidSCI = LastSCI - SCIB / NSection / 2
                If CurMidSCI < 0 Then
                    If Not BatchMode Then ' GFH 04/10/03.
                        S = "CurMidSCI < 0, check input data." & vbCrLf
                        S = S & "Terminating the computation." ' GFH 04/10/03.
                        Ret = MsgBoxDQ(S, 0, "Error in LeafLifeTotal")
                    End If
                    DesigningStr = False
                    Exit Function ' GFH 04/10/03.
                End If
                Call UpdateCurrentSectData()
                'Call WESModulus(AggErr, SubLayers)
                Call FAAModulus(AggErr, SubLayers)

                julModulus(2) = CSng(E0 * (0.002 + 0.0064 * CurMidSCI + (0.00584 * CurMidSCI) ^ 2))
                '      julModulus(2) = E0
                'Debug.Print " Second julModulus(2)= ", julModulus(2)
                If AggErr = True Then
                    Ret = MsgBoxDQ("AggErr in LeafLifeTotal 3", 0, "AggErr")
                    Exit Function
                End If
                NEvalDepths = 1
                EvalDepth(1) = -Thick(1)




                For I = 1 To NEvalDepths
                    Call WriteFile()
                    Call LEDFAA_to_LEAF_Rigid(DesignType, NextraAC)
                    LEAStrActiveX.EvalDepth = System.Math.Abs(EvalDepth(I))
                    LEAStrActiveX.EvalLayer = I
                    If LayerType(LCode(1)) = NACO Or LayerType(LCode(1)) = NND Then LEAStrActiveX.EvalLayer = 2
                    TimeSave1 = timeGetTime

                    RunLEAF = New LEAFClassLib.clsLEAF()     'ikawa
                    Call RunLEAF.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, HorizStressResponse, AllResp)
                    RunLEAF = Nothing


                    'JobTimeDir_SectDir_IterDir = AppDir & "\" & JobName & " " & ResultsDir & "\" & SectName & "\" & Iter
                    'RunAM = New AMClassLib.clsAM()
                    'Call RunAM.ComputeResponse(LEAFClassLib.clsLEAF.LEAFoptions.HorizontalStress, CShort(NAC + NextraAC), CallAC, LEAStrActiveX, Response, AllResp, CalcStress)
                    '''''ReDim Response(1, 1)
                    'Iter += 1

                    '''''ReDim HorizStressResponse(NAC + NextraAC, 1)
                    'For ii = 1 To NAC + NextraAC
                    'OverlayStress(ii) = Response(ii, 1)
                    'PCCStress(ii) = Response(ii, 2)

                    'HorizStressResponse(ii, 1) = Response(ii, 1)
                    'Next ii


                    RunTimeSelected = RunTimeSelected + (timeGetTime - TimeSave1)
                    Call LeafCDFRigid13(CDFMAX, Overflow, HorizStressResponse) ' Sets STRSH() = Max HorizStressResponse() converted to edge stress for all aircraft.
                    For J = 1 To CShort(NAC + NextraAC)
                        STRS(J, I) = STRSH(J)
                    Next J
                Next I

                '      Call WriteJuleaFile ' GFH 02-26-03.
                '      Call JuleaCalc(JuleaErr$) ' GFH 02-26-03.
                '      If DesigningStr = False Then Exit Function ' GFH 02-26-03.
                'GUO1210      Call ReadInDamtmp(STRS())
                '      Call CDFRigid(CDFMAX, Overflow)  ' Called to get a value for LogCA2(IA). CDFMAX not used.  ' GFH 02-26-03.
                'Debug.Print "Ret3"; CDFMAX
                '      Print #FileGuo, "CDFMAX,  ICONTROL(SECOND, B<0)="; CDFMAX; Icontrol
                'GUO1210      Call CompforStab(FSlope, 3)
                'GUO1210      FSlopeComp = (.392 - FSlope * .3881) / (FSlope * .0039)
                For J = 1 To CShort(NAC + NextraAC)
                    If STRS(J, 1) < 1.0# Then
                        CaEqual(J) = CSng(1.0E+20 * (1 - CurCDF))
                        GoTo 87
                    End If
                    'GUO1210        LogCA2(j) = LogCAE(SCIE, Rcon(1), STRS(j, 1), FSlope, FSlopeComp)
                    If LogCA2(J) > 20 Then LogCA2(J) = 20
                    CaEqual(J) = CSng(10 ^ LogCA2(J) * (1 - CurCDF))
87:
                Next J
                GoTo 1000

            Else
                Call LifeExist(Tleft, LastCDF * 100, CaEqual)
                T2min = LastTT + Tleft

            End If

            'Print #FileGuo, "T1min = ", T1min, "T2min = ", T2min
            LeafLifeTotal13 = T1min + T2min - Life
            julModulus(2) = EE0

        End If

700:
        'Debug.Print "T1min,TSCIReduction,TleftForOverlay,TTOTAL = "; T1min; LastTT; Tleft; T1min + T2min
701:

        NextraACout = CShort(NextraAC)

    End Function



    Sub Interpolation(ByRef AA As Single, ByRef BB As Single, ByRef YA As Single, ByRef yB As Single, ByRef H As Single)
        Dim CA As Single
        Dim CB As Single
        CA = CSng(System.Math.Log(YA))
        CB = CSng(System.Math.Log(yB))
        H = CSng((System.Math.Log(Life) - CA) / (CB - CA) * (BB - AA) + AA)
    End Sub


    Sub LifeExist(ByRef T1min As Single, ByRef Lf As Single, ByRef CC() As Single)
        Dim YA, yB As Single

        YA = LifeFirst(0, Lf, CC)
        '  YB = LifeFirst(40, Lf, CC())
        '  Call Root(0, 40, 40, .0001, .01, T1min, 3, YA, YB, Lf, CC())
        yB = LifeFirst(10000, Lf, CC)
        Call Root(0, 10000, 10000, 0.0001, 0.01, T1min, 3, YA, yB, Lf, CC)
    End Sub


    Function LifeFirst(ByRef T As Single, ByRef Lf As Single, ByRef CC() As Single) As Single

        Static C As Single
        Static J As Integer

        C = 0
        For J = 1 To NAC + NextraAC 'change2
            C = C + DREP(0, T, J) * CtoP(J, IControl) / CC(J)
        Next J
        C = C - (1 - Lf / 100)
        LifeFirst = C

    End Function



    Function LogCAE(ByRef SCI As Single, ByRef R As Single, ByRef Stress As Single, ByRef FSlope As Single, ByRef FSlopeComp As Single) As Double
        If Stress < 0.0000001 Then
            LogCAE = 12.0!
            GoTo 95
        End If
        LogCAE = (R / Stress - 0.2967 - 0.002267 * SCI) / FSlope / (0.3881 + FSlopeComp * 0.000039 * SCI)
95:
    End Function


    Function LogCurC1(ByRef LogCA As Double, ByRef LogCE As Double, ByRef CurSCI As Single) As Double
        LogCurC1 = (100.0! - CurSCI) * (LogCE - LogCA) / 100.0! + LogCA
    End Function


    Sub ReadInDamtmp(ByRef STRS(,) As Single) 'ik02 ByRef STRS(,)

        Dim IB, IA, LFNo As Short

        FileName = ACDATPath & "DAMTMP.DAT"
        LFNo = CShort(FreeFile())
        FileOpen(LFNo, FileName, OpenMode.Input)
        SS = LineInput(LFNo)
        SS = LineInput(LFNo)
        SS = LineInput(LFNo)
        SS = LineInput(LFNo)
        SS = LineInput(LFNo)
        SS = LineInput(LFNo)
        For IA = 1 To NAC
            For IB = 1 To NEvalDepths
                SS = InputString(LFNo, 12)
                '     GFH
                Input(LFNo, Temp)
                Input(LFNo, Temp)
                Input(LFNo, Temp)
                Input(LFNo, Temp)
                Input(LFNo, STRS(IA, IB))
                STRS(IA, IB) = System.Math.Abs(STRS(IA, IB)) ' See LEDNEW sub RDAMTMP
                '      Input #LFNo, Temp, Temp, Temp, Temp, STRSH(IA)
                '      STRSH(IA) = Abs(STRSH(IA))  ' See LEDNEW sub RDAMTMP
                '      GUO1209-------------------------------
                'ikawa Call InteriorToEdge(IA, STRS(IA, IB)) ' Compensate for edge stresses.
                'ikawa need commment
                '      For new PCC pavement, STRSH(IA) is always the maximum
                '      tension stress at bottom of the concrete slab. However
                '      for overlay on PCC, stresses at bottom of existing slab
                '      and of the overlay both are needed for determining the
                '      further deterioration degree of the existing pavement
                '      GUO1209---------------------------------------
                '      STRS(IA, ib) = STRSH(IA)
                '     GFH
            Next IB
        Next IA
        FileClose(LFNo)
    End Sub



    Sub Root(ByRef AA As Single, ByRef BB As Single, ByRef H As Single,
              ByRef E1 As Single, ByRef E2 As Single, ByRef RT As Single,
              ByRef IZ As Single, ByRef YA As Single, ByRef yB As Single,
              ByRef Lf As Single, ByRef CC() As Single)

        Dim A0, FA, B, A, FB, F0 As Single
        Dim K As Short

        A = AA : B = BB : FA = YA : FB = yB : K = 0

        If System.Math.Abs(FA) <= E1 Then
            RT = A
            If IZ = 2 Then YA = FA + Life
            Exit Sub
        End If

        If System.Math.Abs(FB) <= E1 Then
            RT = B
            If IZ = 2 Then YA = FB + Life
            Exit Sub
        End If

        A = AA
        B = BB

        Do
            A0 = (A + B) / 2.0!
            K = K + 1S
            If IZ = 1 Then
                F0 = CSng(DSCI(A0))
            ElseIf IZ = 2 Then
                F0 = CSng(DSCI(A0)) '- temporary
                'F0 = LifeTotal(A0) in progress
            Else
                F0 = LifeFirst(A0, Lf, CC)
            End If
            If System.Math.Abs(F0) <= E1 Then
                RT = A0
                If IZ = 2 Then YA = F0 + Life
                Exit Sub
            End If
            If System.Math.Abs(B - A0) <= E2 Then
                RT = A0
                If IZ = 2 Then YA = F0 + Life
                Exit Sub
            End If

            If F0 * FA < 0 Then
                B = A0
                FB = F0
            Else
                FA = F0
                A = A0
            End If
        Loop


    End Sub


    Sub SCIReduction(ByRef LastTT As Single, ByRef CurDT As Single, ByRef CallowA() As Single, ByRef CallowE() As Single, ByRef LastSCI As Single, ByRef CurSCI As Single, ByRef Nsection As Short)
        ' Years needed to reduce SCI from LastSCI to CurSCI by iteration
        ' CurDT   a given value for calculating CalSCI, the required
        '         life in years will be stored in "CurDT"
        ' CalSCI  calculated value of SCI using a function DSCI
        ' CurSCI  A required value, Abs(CurSCI-CalSCI) < SCIError,
        '         calculation will be completed

        Dim I As Short
        Dim YA, AA, H, B, BB, yB As Single
        Static RT As Single

        I = 0
        B = CSng(DSCI(CurDT))
        ' Print #FileGuo, "  after first B=DSCI(CurDT), B= ", B
        ' Debug.Print "in SCIReduction"; "  B=DS-SCIB/Nsection= "; B
        If System.Math.Abs(B) < SCIError Then Exit Sub
        If B > 0 Then
            Do
                yB = B
                I = I + 1S
                LastDT = CurDT
                CurDT = LastDT / 2
                B = CSng(DSCI(CurDT))
                If I > Ncontrol2 Then GoTo 1300 'Not convergent
            Loop Until B < 0
            AA = CurDT
            BB = LastDT
            YA = B
        Else
            Do
                YA = B
                I = I + 1S
                LastDT = CurDT
                CurDT = LastDT * 2
                B = CSng(DSCI(CurDT))
                '      Print #FileGuo, "after Second B=DSCI(CurDT), B =", B,
                '      Print #FileGuo, "CurDT=", CurDT
                If I > Ncontrol2 Then GoTo 1300 ' Not convergent
            Loop Until B > 0
            AA = LastDT
            BB = CurDT
            yB = B
        End If
        H = BB - AA
        '  Debug.Print " Before ROOT in SCIReduction "

        Dim libTT(libNAC) As Single
        For I = 1 To libNAC
            libTT(I) = AC(I).libTT
        Next I

        Call Root(AA, BB, H, SCIError, 0.001, RT, 1, YA, yB, 1.0!, libTT)
        '  Debug.Print " After ROOT in SCIReduction "
        CurDT = RT
        Exit Sub
1300:
        System.Diagnostics.Debug.WriteLine("It is not convergent in root seaching")

    End Sub

End Module