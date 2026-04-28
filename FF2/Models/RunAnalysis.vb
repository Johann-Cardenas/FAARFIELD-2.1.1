Imports System.Collections.ObjectModel
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports FaarFieldAnalysis
Imports FF2.ViewModels
Imports System.Threading

Public Class RunAnalysis

    Private Property Job As IFaarFieldJob
    Private Property Jobs As ObservableCollection(Of ITreeViewItemViewModel)
    Private Property Section As ISection

    Private Property SandwichBool As Boolean
    Private Property Factory As IFaarFieldModelFactory
    Private Property ViewModel As MainWindowViewModel
    Private Property AnalysisTimer As Timers.Timer = New Timers.Timer(1000)

    Private Property TimeStart As DateTime
    Private Property warning As Integer
    Private Property message As String
    Private Property timemessage As String

    Private jobindex As Integer = 0
    Private sectionindex As Integer = 0
    Private startjobindex As Integer = 0
    Private startsectionindex As Integer = 0
    Private Sections As List(Of ISection)
    Private RunBatch As Boolean = False
    Private BatchIndexList As List(Of Array) = New List(Of Array)
    Private BatchIndexListPointer As Integer = 0

    Private tokenSource As New CancellationTokenSource()
    Private token As CancellationToken = tokenSource.Token
    Private t As Task

    Private UM As UpdateManager

    Private RXSStoredDepartures()


    Public Sub New(MWviewModel As MainWindowViewModel)

        UM = New UpdateManager(MWviewModel)
        modStrDesign.UM = UM

        ViewModel = MWviewModel

        Factory = ViewModel.FaarFieldFactory

        Jobs = ViewModel.Jobs

        Job = ViewModel.CurrentJob

        For i = 0 To ViewModel.Jobs.Count - 1
            If DirectCast(DirectCast(ViewModel.Jobs(i), JobViewModel).FaarFieldJob, FaarFieldJob) Is Job Then
                jobindex = i
            End If
        Next

        Section = ViewModel.CurrentSectionView.Section
        Sections = Job.Sections

        For j = 0 To Sections.Count - 1
            If Sections(j) Is Section Then
                sectionindex = j
            End If
        Next

        startsectionindex = sectionindex
        startjobindex = jobindex

        For jb = 0 To Jobs.Count - 1
            For sec = 0 To DirectCast(DirectCast(Jobs(jb), JobViewModel).FaarFieldJob, FaarFieldJob).Sections.Count - 1
                Dim tmpsec = DirectCast(DirectCast(Jobs(jb), JobViewModel).FaarFieldJob, FaarFieldJob).Sections(sec)
                If tmpsec.RunBatch = True Then

                    ' Check for zero departures on all aircraft in section
                    Dim zerotrafficflag = True
                    For i = 0 To tmpsec.Airplanes.Count - 1
                        If tmpsec.Airplanes.Item(i).NumberDepartures > 0 Then
                            zerotrafficflag = False
                            Exit For
                        End If
                    Next

                    ' Skip adding section to batch if section has zero departures
                    If zerotrafficflag Then
                        MessageBox.Show("Section with no Annual Departures set will be skipped")
                    Else
                        Dim tarry = {jb, sec}
                        BatchIndexList.Add(tarry)
                    End If

                End If
            Next
        Next

        If BatchIndexList.Count > 0 Then
            ' Using Checks

            Job = DirectCast(DirectCast(Jobs(BatchIndexList(0)(0)), JobViewModel).FaarFieldJob, FaarFieldJob)
            Sections = Job.Sections
            Section = Job.Sections(BatchIndexList(0)(1))

            jobindex = BatchIndexList(0)(0)
            sectionindex = BatchIndexList(0)(1)

        Else
            ' Using Selection

            ' Check for zero departures on all aircraft in section
            Dim tmpsec = DirectCast(DirectCast(Jobs(jobindex), JobViewModel).FaarFieldJob, FaarFieldJob).Sections(sectionindex)
            Dim zerotrafficflag = True
            For i = 0 To tmpsec.Airplanes.Count - 1
                If tmpsec.Airplanes.Item(i).NumberDepartures > 0 Then
                    zerotrafficflag = False
                    Exit For
                End If
            Next

            ' Skip adding section to batch if section has zero departures
            If zerotrafficflag Then
                MessageBox.Show("Annual Departures are required on at least one aircraft.")
            Else
                Dim tarry = {jobindex, sectionindex}
                BatchIndexList.Add(tarry)
            End If

        End If

        Section.Factory = Factory
        JobName = Job.Name 'ik2020.03
        SectName = Section.Name 'ik2020.03
        gOutputDirName = JobName & "-" & SectName '& " " & getTodaysDateFormatted2() 'ik2020.02.25

        If Job.DesignOptions.Outfile = True Then 'ik2020.03
            NoOutFiles = False
        Else
            NoOutFiles = True
        End If

    End Sub

    Public Sub CancelRun()
        AnalysisTimer.Stop()
        tokenSource.Cancel()
    End Sub

    Public Sub RunOrCancel()
        ViewModel.RunCompleted = False
        message = ""
        warning = 0

        Call LayerPlacingChecks(Section.Layers)
        Call DesignTypeChecks(Section.Layers)

        If SandwichBool = True Then
            ViewModel.RunCompleted = True
            ViewModel.RunButtonText = "Run"
            AnalysisTimer.Stop()
            tokenSource.Cancel()
        End If

        If warning = 1 Then
            Exit Sub
        End If



        If ViewModel.RunButtonText = "Cancel" Then
            'This will tell the analysis to exit the analysis loop
            'This variable was part of the original analysis code in FAARFIELD 1.41
            'This sets the user interupt flag to false so the analysis will run.
            'Setting this value to true kills the analysis
            gUserInterrupted = True

            'ViewModel.RunButtonText = "Run"
            AnalysisTimer.Stop()
            ViewModel.MessageText += Environment.NewLine + "Analysis canceled by user."
            tokenSource.Cancel()
        Else
            ' Skip run if no sections added to batch, zero departures for single selection, cancel run 
            If BatchIndexList.Count > 0 Then
                gUserInterrupted = False
                RunBatch = ViewModel.RunBatch
                TimeStart = DateTime.Now
                AnalysisTimer.Enabled = True
                Dim span = DateTime.Now - TimeStart

                ViewModel.RunButtonText = "Cancel"

                ViewModel.SelectedTabIndex = 0
                gUseCompaction = False

                RunLoop()
            Else
                gUserInterrupted = True
                ViewModel.RunCompleted = True
                ViewModel.RunButtonText = "Run"
                AnalysisTimer.Stop()
                SandwichBool = False
            End If
        End If

    End Sub

    Private Sub RunLoop()

        If gUserInterrupted = False Then

            Dim sectionview = DirectCast(Jobs(jobindex).Children.Item(3).Children.Item(sectionindex), SectionViewModel)
            System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.SetSelection(sectionview))

            Section.ReducedCrossSectionRun = False

            If Section.SelectedRun = 0 Then
                Section.ThicknessOptimization = True
                ThicknessDesignRun = True

                If Job.DesignOptions.ReducedCrossSection = True And (Section.AnalysisType.Name = "New Flexible" Or Section.AnalysisType.Name = "New Rigid") Then
                    Section.ReducedCrossSectionRun = True
                End If

            Else
                Section.ThicknessOptimization = False
            End If

            If Section.SelectedRun = 2 Then
                gUseCompaction = True
                LifeCompactionRun = True

                'set minimum allowable value of Annual Departures for Life/Compaction
                For Each airplane In Section.Airplanes
                    If airplane.NumberDepartures < 1 Then
                        MessageBox.Show("Minimum allowable value of Annual Departures for Life/Compaction run is 1")
                        gUserInterrupted = True
                        ViewModel.RunCompleted = True
                        ViewModel.RunButtonText = "Run"
                        AnalysisTimer.Stop()
                        airplane.NumberDepartures = 1
                        Exit Sub
                    End If
                Next
            End If

            For Each airplane In Section.Airplanes
                Dim isFound As Boolean = False

                For Each libAC In ViewModel.FullAircraftLibrary
                    If airplane.Name = libAC.Name Then

                        airplane.AircraftNumber = libAC.AircraftNumber

                        airplane.DefaultCp = airplane.Cp
                        airplane.DefaultGrossWeight = airplane.GrossWeight
                        airplane.TBT = airplane.Tt

                        airplane.RunMgPercent = libAC.RunMgPercent
                        airplane.MgPercent = libAC.MgPercent
                        airplane.MgPercentPCN = libAC.MgPercentPCN

                        airplane.Deprecated = libAC.Deprecated

                        airplane.Gear = libAC.Gear
                        airplane.GearOrientation = libAC.GearOrientation

                        Dim wc As ICoordinates
                        Dim ep As ICoordinates

                        airplane.EvaluationPoints.Clear()
                        For Each ep In libAC.EvaluationPoints

                            If TypeOf (Job.DesignOptions.MeasurementSystem) Is UsCustomary Then
                                airplane.EvaluationPoints.Add(Factory.CreateCoordinates(Factory.CreateLength(ep.X.UsCustomary, Job.DesignOptions.MeasurementSystem), Factory.CreateLength(ep.Y.UsCustomary, Job.DesignOptions.MeasurementSystem)))
                            Else
                                airplane.EvaluationPoints.Add(Factory.CreateCoordinates(Factory.CreateLength(ep.X.Metric, Job.DesignOptions.MeasurementSystem), Factory.CreateLength(ep.Y.Metric, Job.DesignOptions.MeasurementSystem)))
                            End If

                        Next

                        airplane.WheelCoordinates.Clear()
                        For Each wc In libAC.WheelCoordinates

                            If TypeOf (Job.DesignOptions.MeasurementSystem) Is UsCustomary Then
                                airplane.WheelCoordinates.Add(Factory.CreateCoordinates(Factory.CreateLength(wc.X.UsCustomary, Job.DesignOptions.MeasurementSystem), Factory.CreateLength(wc.Y.UsCustomary, Job.DesignOptions.MeasurementSystem)))
                            Else
                                airplane.WheelCoordinates.Add(Factory.CreateCoordinates(Factory.CreateLength(wc.X.Metric, Job.DesignOptions.MeasurementSystem), Factory.CreateLength(wc.Y.Metric, Job.DesignOptions.MeasurementSystem)))
                            End If

                        Next

                        airplane.NumberGear = libAC.NumberGear
                        airplane.NumberTireTrack = libAC.NumberTireTrack
                        airplane.NumberWheels = libAC.NumberWheels

                        airplane.ACRB = 0
                        airplane.ACRB1 = 0
                        airplane.ACRB2 = 0
                        airplane.ACRB3 = 0
                        airplane.ACRB4 = 0
                        airplane.ACRThick = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                        airplane.ACRThick1 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                        airplane.ACRThick2 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                        airplane.ACRThick3 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                        airplane.ACRThick4 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                        airplane.ACRCoverage = 0
                        airplane.ACRThickMGW = 0
                        airplane.Cdf = 0
                        airplane.CdfAircraftMax = 0
                        airplane.CtoP = 0
                        airplane.CdfAircraftMaxSub = 0
                        airplane.Coverage = 0
                        airplane.CtoP401 = 0
                        airplane.CtoPAc = 0
                        airplane.CtoPHma = 0
                        airplane.CtoPSub = 0
                        airplane.Tg = 0
                        airplane.Tv = 0
                        airplane.Ts = 0
                        airplane.PCRNumber = 0
                        airplane.PCRThick = 0
                        airplane.gNewAnnualdepartue = 0
                        airplane.gNewGL = CType(Factory.CreateWeight(0, Factory.CreateUsCustomary), Weight)

                        isFound = True
                        Exit For
                    End If

                Next

                If Not isFound Then
                    Debug.WriteLine("AC in Traffic not found")

                    If airplane.DataStorage IsNot Nothing Then
                        airplane.MgPercent = airplane.DataStorage(0)
                        airplane.MgPercentPCN = airplane.DataStorage(1)
                        airplane.RunMgPercent = airplane.DataStorage(2)
                    End If

                    airplane.DefaultCp = airplane.Cp
                    airplane.DefaultGrossWeight = airplane.GrossWeight
                    airplane.TBT = airplane.Tt
                    airplane.ACRB = 0
                    airplane.ACRB1 = 0
                    airplane.ACRB2 = 0
                    airplane.ACRB3 = 0
                    airplane.ACRB4 = 0
                    airplane.ACRThick = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                    airplane.ACRThick1 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                    airplane.ACRThick2 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                    airplane.ACRThick3 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                    airplane.ACRThick4 = CType(Factory.CreateThickness(0, Factory.CreateUsCustomary()), Thickness)
                    airplane.ACRCoverage = 0
                    airplane.ACRThickMGW = 0
                    airplane.Cdf = 0
                    airplane.CdfAircraftMax = 0
                    airplane.CtoP = 0
                    airplane.CdfAircraftMaxSub = 0
                    airplane.Coverage = 0
                    airplane.CtoP401 = 0
                    airplane.CtoPAc = 0
                    airplane.CtoPHma = 0
                    airplane.CtoPSub = 0
                    airplane.Tg = 0
                    airplane.Tv = 0
                    airplane.Ts = 0
                    airplane.PCRNumber = 0
                    airplane.PCRThick = 0
                    airplane.gNewAnnualdepartue = 0
                    airplane.gNewGL = CType(Factory.CreateWeight(0, Factory.CreateUsCustomary), Weight)

                End If

            Next


            FEDFAA1.TrafficList = New List(Of IAirplaneInfo)(Section.Airplanes)
            FEDFAA1.InitTraffic = True

            SetCurrentSectData(Job, Section)

            If Section.SelectedRun = 3 Then
                FEDFAA1.LifeCounterForPCRRuns = 0
                PCRRun = True
                ViewModel.MessageText = "PCR Calculation " + "of " + Section.Name + " started."


                gHMAonRigid_Mod = Job.DesignOptions.ThickPccOverlay
                AircraftLifeCDFvsDesignLife = True

                t = Task.Factory.StartNew(Sub() btnPCR_Click(token), token)
                t.ContinueWith(AddressOf EndTaskCheck)

            Else
                ViewModel.MessageText = Section.AnalysisType.Name + " of " + Section.Name + " started." + vbNewLine + timemessage

                Call Designtypedetermination()

            End If

            ThicknessDesignRun = False
            LifeCompactionRun = False
        End If

    End Sub

    Private Sub Designtypedetermination()

        If gUserInterrupted = False Then

            MinimumStrainChecker = False
            OverflowExit = False
            LowLifeExit = False

            If DesignType = NewFlex Then
                If Section.ReducedCrossSectionRun Then
                    SetupForRXS()
                    t = Task.Factory.StartNew(Sub() NewFlexibleAnalysis(token), token)
                    t.ContinueWith(AddressOf ContinueNewFlexibleFromRXS)
                Else
                    t = Task.Factory.StartNew(Sub() NewFlexibleAnalysis(token), token)
                    t.ContinueWith(AddressOf EndTaskCheck)
                End If

            ElseIf DesignType = NewRigid Then
                If Section.ReducedCrossSectionRun Then
                    SetupForRXS()
                    t = Task.Factory.StartNew(Sub() NewRigidAnalysis(token), token)
                    t.ContinueWith(AddressOf ContinueNewRigidFromRXS)
                Else
                    t = Task.Factory.StartNew(Sub() NewRigidAnalysis(token), token)
                    t.ContinueWith(AddressOf EndTaskCheck)
                End If

            ElseIf DesignType = FlexOnFlex Then
                t = Task.Factory.StartNew(Sub() FlexibleOnFlexibleAnalysis(token), token)
                t.ContinueWith(AddressOf EndTaskCheck)

            ElseIf DesignType = FlexOnRigid Then
                t = Task.Factory.StartNew(Sub() FlexibleOnRigid(token), token)
                t.ContinueWith(AddressOf EndTaskCheck)

            ElseIf DesignType = PCCOnFlex Then
                t = Task.Factory.StartNew(Sub() PccOnFlexible(token), token)
                t.ContinueWith(AddressOf EndTaskCheck)

            ElseIf DesignType = 11 Or DesignType = 12 Then
                t = Task.Factory.StartNew(Sub() UnbondOnRigid(token), token)
                t.ContinueWith(AddressOf EndTaskCheck)

            End If

        End If

    End Sub

    Private Sub EndTaskCheck()

        If gUserInterrupted = False Then
            AnalysisTimer.Stop()
            EndTask()
            ViewModel.ResetSlabStressValues()
            ViewModel.SlabStressUpdate()
            ViewModel.CurrentSectionView.Section.SlabComplete = True
            If BatchIndexListPointer = BatchIndexList.Count - 1 Then
                ViewModel.RunCompleted = True
                ViewModel.RunButtonText = "Run"
                'ViewModel.RunAnalysisIsEnabled = True
                System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.CheckRunAvailable())

                Job = DirectCast(Jobs(BatchIndexList(0)(0)), JobViewModel).FaarFieldJob
                Section = Job.Sections(BatchIndexList(0)(1))

                Dim sectionview = DirectCast(Jobs(BatchIndexList(0)(0)).Children.Item(3).Children.Item(BatchIndexList(0)(1)), SectionViewModel)
                System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.SetSelection(sectionview))

            Else
                BatchIndexListPointer += 1

                jobindex = BatchIndexList(BatchIndexListPointer)(0)
                sectionindex = BatchIndexList(BatchIndexListPointer)(1)

                Job = DirectCast(Jobs(jobindex), JobViewModel).FaarFieldJob
                Section = Job.Sections(sectionindex)

                Dim sectionview = DirectCast(Jobs(jobindex).Children.Item(3).Children.Item(sectionindex), SectionViewModel)
                System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.SetSelection(sectionview))

                RunLoop()

            End If

        End If

        If token.IsCancellationRequested Then
            tokenSource.Dispose()
            ViewModel.RunButtonText = "Run"
            System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.CheckRunAvailable())
        End If

        FlexonRigidChecker = False

    End Sub


    Private Sub NewFlexibleAnalysis(ByRef ct As CancellationToken)

        ct.ThrowIfCancellationRequested()
        If gUserInterrupted Then Exit Sub

        StraightLineModel = -1
        If Not NoACCDF Then : HMA_CDF_Calc = True : End If
        If DesignType = NewFlex Then
            IterLayerChosen = NPLayers - 1S
        End If

        If Not LifeComputation Then : ILayer = IterLayerChosen : End If

        Dim AutoDesign As Boolean
        If DesignType = NewFlex Then
            If Not LifeComputation Then


                For i = 0 To Sections(sectionindex).Layers.Count - 1

                    If Sections(sectionindex).Layers.Item(i).DesignedLayer = "-->" Then
                        IterLayerChosen = i + 1
                    End If

                Next
            End If
            AutoDesign = AutomaticDesign()
        End If

        If gUserInterrupted Then Exit Sub
        If Not AutoDesign Then
            'old design like in LEDFAA 1.3
            DesigningP209DrawStructure = True
            Call LeafDesignFlex()
        End If
        If gUserInterrupted Then Exit Sub
        '   Call WriteToFileCtoPsub("NewFlex")

    End Sub

    Private Sub SetupForRXS()
        If Not Section.ReducedCrossSectionLayerThickness Is Nothing Then
            Section.ReducedCrossSectionLayerThickness.Clear()
        End If
        If Not Section.ReducedCrossSectionLayerModulus Is Nothing Then
            Section.ReducedCrossSectionLayerModulus.Clear()
        End If
        If Not Section.ReducedDesignAnnualDeparture Is Nothing Then
            Section.ReducedDesignAnnualDeparture.Clear()
        End If
        If Not Section.ReducedDesignTotalDeparture Is Nothing Then
            Section.ReducedDesignTotalDeparture.Clear()
        End If
        If Not Section.ReducedDesignCDF Is Nothing Then
            Section.ReducedDesignCDF.Clear()
        End If
        If Not Section.ReducedDesignCDFContribution Is Nothing Then
            Section.ReducedDesignCDFContribution.Clear()
        End If
        If Not Section.ReducedDesignPtoC Is Nothing Then
            Section.ReducedDesignPtoC.Clear()
        End If

        ReDim RXSStoredDepartures(Section.Airplanes.Count - 1)

        For i = 0 To Section.Airplanes.Count - 1
            RXSStoredDepartures(i) = Section.Airplanes(i).NumberDepartures
            Dim reducedDeparture As Integer = Section.Airplanes(i).NumberDepartures / 100
            If reducedDeparture < 1 Then
                Section.Airplanes(i).NumberDepartures = 1
            Else
                Section.Airplanes(i).NumberDepartures = reducedDeparture
            End If
        Next

        SetCurrentSectData(Job, Section)
        ReducedCrossSectionRunChecker = True

        ViewModel.CrossSectionText = "Reduced Cross Section Design in progress"
        ViewModel.CrossSectionVisibility = Visibility.Visible

    End Sub

    Private Sub ContinueNewFlexibleFromRXS()

        AddCdfResultToSection(Section)

        For l = 0 To Section.Layers.Count - 1
            If Section.Layers(l).DesignedLayer = "-->" Then
                Section.Layers(l).Thickness = Section.Factory.CreateThickness(FEDFAA1.Thick(l + 1), New UsCustomary())
                ViewModel.ThicknessVerificationRules(Section.Layers(l))
                Section.Layers(l).Modulus = Section.Factory.CreateModulus(FEDFAA1.Modulus(l + 1), New UsCustomary())
            End If
        Next

        Dim measurementSystem = Factory.CreateUsCustomary()

        Section.ReducedCrossSectionLayerThickness = New List(Of Thickness)
        For Each layer In Section.Layers
            Section.ReducedCrossSectionLayerThickness.Add(Factory.CreateThickness(layer.Thickness.UsCustomary, measurementSystem))
        Next
        Section.ReducedCrossSectionLayerModulus = New List(Of Modulus)

        For Each layer In Section.Layers
            Section.ReducedCrossSectionLayerModulus.Add(Factory.CreateModulus(layer.Modulus.UsCustomary, measurementSystem))
        Next
        Section.ReducedDesignAnnualDeparture = New List(Of Single)
        Section.ReducedDesignTotalDeparture = New List(Of Single)
        Section.ReducedDesignCDF = New List(Of Single)
        Section.ReducedDesignCDFContribution = New List(Of Single)
        Section.ReducedDesignPtoC = New List(Of Single)
        For Each airplane In Section.Airplanes
            Section.ReducedDesignAnnualDeparture.Add(Format(airplane.NumberDepartures, "0"))
            Section.ReducedDesignTotalDeparture.Add(Format(airplane.TotalDepartures, "0"))
            Section.ReducedDesignCDF.Add(airplane.Cdf)
            Section.ReducedDesignCDFContribution.Add(airplane.CdfAircraftMax)
            Section.ReducedDesignPtoC.Add(airplane.CtoP)
        Next

        For i = 0 To Section.Airplanes.Count - 1
            Section.Airplanes(i).NumberDepartures = RXSStoredDepartures(i)
            RXSStoredDepartures(i) = Section.Airplanes(i).NumberDepartures
        Next

        SetCurrentSectData(Job, Section)
        ReducedCrossSectionRunChecker = False

        ViewModel.CrossSectionText = "Full Cross Section in progress"

        t = Task.Factory.StartNew(Sub() NewFlexibleAnalysis(token), token)
        t.ContinueWith(AddressOf EndTaskCheck)

    End Sub

    Private Sub ContinueNewRigidFromRXS()

        AddCdfResultToSection(Section)

        For l = 0 To Section.Layers.Count - 1
            If Section.Layers(l).DesignedLayer = "-->" Then
                Section.Layers(l).Thickness = Section.Factory.CreateThickness(FEDFAA1.Thick(l + 1), New UsCustomary())
                ViewModel.ThicknessVerificationRules(Section.Layers(l))
                Section.Layers(l).Modulus = Section.Factory.CreateModulus(FEDFAA1.Modulus(l + 1), New UsCustomary())
            End If
        Next

        Dim measurementSystem = Factory.CreateUsCustomary()

        Section.ReducedCrossSectionLayerThickness = New List(Of Thickness)
        For Each layer In Section.Layers
            Section.ReducedCrossSectionLayerThickness.Add(Factory.CreateThickness(layer.Thickness.UsCustomary, measurementSystem))
        Next
        Section.ReducedCrossSectionLayerModulus = New List(Of Modulus)

        For Each layer In Section.Layers
            Section.ReducedCrossSectionLayerModulus.Add(Factory.CreateModulus(layer.Modulus.UsCustomary, measurementSystem))
        Next
        Section.ReducedDesignAnnualDeparture = New List(Of Single)
        Section.ReducedDesignTotalDeparture = New List(Of Single)
        Section.ReducedDesignCDF = New List(Of Single)
        Section.ReducedDesignCDFContribution = New List(Of Single)
        Section.ReducedDesignPtoC = New List(Of Single)
        For Each airplane In Section.Airplanes
            Section.ReducedDesignAnnualDeparture.Add(Format(airplane.NumberDepartures, "0"))
            Section.ReducedDesignTotalDeparture.Add(Format(airplane.TotalDepartures, "0"))
            Section.ReducedDesignCDF.Add(airplane.Cdf)
            Section.ReducedDesignCDFContribution.Add(airplane.CdfAircraftMax)
            Section.ReducedDesignPtoC.Add(airplane.CtoP)
        Next

        For i = 0 To Section.Airplanes.Count - 1
            Section.Airplanes(i).NumberDepartures = RXSStoredDepartures(i)
            RXSStoredDepartures(i) = Section.Airplanes(i).NumberDepartures
        Next

        SetCurrentSectData(Job, Section)
        ReducedCrossSectionRunChecker = False

        ViewModel.CrossSectionText = "Full Cross Section in progress"

        t = Task.Factory.StartNew(Sub() NewRigidAnalysis(token), token)
        t.ContinueWith(AddressOf EndTaskCheck)

    End Sub


    Private Sub FlexibleOnFlexibleAnalysis(ByRef ct As CancellationToken)
        ct.ThrowIfCancellationRequested()
        If gUserInterrupted Then Exit Sub

        IterLayerChosen = 1
        If Not NoACCDF Then
            HMA_CDF_Calc = True
        End If
        StraightLineModel = -1
        If Not LifeComputation Then ILayer = 1
        If gUserInterrupted Then Exit Sub
        Call LeafDesignFlexOFlex()


        '   Call WriteToFileCtoPsub("FlexOnFlex")

    End Sub



    Private Sub NewRigidAnalysis(ByRef ct As CancellationToken)

        ct.ThrowIfCancellationRequested()
        If gUserInterrupted Then Exit Sub

        IterLayerChosen = 1
        MinimumThickness = False
        If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
            Call pre_DesignRigid_NP() 'LEAF
            Call SelectACwithCDF005()
            DesigningStr = True : LEDFAA13Thick = Thick(1)
        End If
        gOnly_LEAF = False
        If MinimumThickness = False Then


            If gOnly_LEAF Then
                Call pre_DesignRigid_NP() 'LEAF
            Else

                Call DesignRigid_NP(ct) '3D-FEM stress   

            End If
        End If
        'Call DesignRigid_NP() '3D-FEM stress   
        If gUserInterrupted Then Exit Sub
        '   Call WriteToFileCtoPsub("NewRigid")

    End Sub

    Private Sub SetAsFlexOnFlex() 'ik2020.03

        DesignType = FlexOnFlex
        sLCode2 = LCode(2)
        LCode(2) = 1
        sModulus2 = FEDFAA1.Modulus(2)
        FEDFAA1.Modulus(2) = PCC_Modulus(SCIB)
        sRcon2 = RCon(2)
        RCon(2) = 0.0
        sThick1 = Thick(1)
        sThick2 = Thick(2)

    End Sub


    Private Sub RestoreAConRigid() 'ik2020.03

        'Thick(1) = sThick1
        'Thick(2) = sThick2
        RCon(2) = sRcon2
        FEDFAA1.Modulus(2) = sModulus2
        LCode(2) = sLCode2
        DesignType = FlexOnRigid

    End Sub

    Private Function PCC_Modulus(ByVal SCIB1 As Single) As Single 'ik2020.03
        Dim mod1 As Single
        mod1 = 4000000 '4mln

        PCC_Modulus = CSng(Math.Round(mod1 * (0.02 + 0.0064 * SCIB1 + (0.00584 * SCIB1) ^ 2)))

    End Function

    Private Sub FlexibleOnRigid(ByRef ct As CancellationToken) 'ik2020.03
        ct.ThrowIfCancellationRequested()
        If gUserInterrupted Then Exit Sub

        StraightLineModel = -1
        HMAonRigidCase = 0
        gHMAonRigid_Mod = Job.DesignOptions.ThickPccOverlay

        gHorStressNIKE3D = Nothing 'ikawa 2017.08.15
        gHorStressLEAF = Nothing 'ikawa 2017.08.15

        '>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        HMAonRigidCase = 0 : AnalyzedasFlexible = False
        OverlayLife = 0 : LifeStr = 0

        If Not LifeComputation Then
            ILayer = 1
            IterLayerChosen = 1
        End If
        'Call DrawStructure() : Refresh()

        'GoTo newCalulations

        '+++++++++++++++++++++++++++++++++++++++++++
        If gHMAonRigid_Mod Then
            If LifeComputation Then

                Call DesignRigidOverlay_NP(ct)  '3D-FEM stress 'iktemp
                'save cdf for AConRigid
                Call copyArray1(jobCDFtable, jobCDFtableRigid)
                Call copyArray1(jobCDFacrftMaxtable, jobCDFacrftMaxtableRigid)
                Call copyArray1(jobCtoPtable, jobCtoPtableRigid)



                If Thick(1) < Thick(2) Then
                    'nothing
                Else 'calculate for NewFlexible

                    Call SetAsFlexOnFlex()
                    Call LeafDesignFlexOFlex()
                    Call RestoreAConRigid()


                    If OverlayLife > LifeStr Then
                        'restore cdf for AConRigid
                        Call copyArray1(jobCDFtableRigid, jobCDFtable)
                        Call copyArray1(jobCDFacrftMaxtableRigid, jobCDFacrftMaxtable)
                        Call copyArray1(jobCtoPtableRigid, jobCtoPtable)

                    Else
                        OverlayLife = LifeStr
                        AnalyzedasFlexible = True
                    End If


                End If


            Else 'Design =================================================
                'Thick(1) = Thick(2) - CSng(0.01)
                Thick(1) = Thick(2)

                'Call DrawStructure() : Refresh()
                System.Windows.Forms.Application.DoEvents()

                LifeComputation = True
                Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
                LifeComputation = False

                If Math.Abs(OverlayLife - Life) < LifeError Then

                ElseIf OverlayLife > Life Then

                    'pre-design
                    If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                        Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                        Call SelectACwithCDF005()
                        DesigningStr = True : LEDFAA13Thick = Thick(1)
                    End If

                    Call DesignRigidOverlay_NP(ct)  '3D-FEM stress

                Else

                    'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                    'Flex life
                    'Thick(1) = Thick(2)
                    LifeComputation = True
                    Call SetAsFlexOnFlex()
                    Call LeafDesignFlexOFlex()
                    LifeComputation = False
                    Call RestoreAConRigid()

                    If OverlayLife > LifeStr Then
                        'only AConRigid
                        If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                            Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                            Call SelectACwithCDF005()
                            DesigningStr = True : LEDFAA13Thick = Thick(1)
                        End If

                        Call DesignRigidOverlay_NP(ct)  '3D-FEM stress

                    Else 'only Flex

                        If LifeStr >= Life Then
                            'Design done.
                            OverlayLife = LifeStr
                            HMAonRigidCase = 4
                        Else
                            Call SetAsFlexOnFlex()
                            Call LeafDesignFlexOFlex()
                            LifeStr = Life / CDFPic
                            HMAonRigidCase = 3
                            OverlayLife = LifeStr
                        End If

                        Call RestoreAConRigid()
                        AnalyzedasFlexible = True

                    End If
                End If
            End If

        Else ' ======= Old LifeComputation And Design =======

            'gOnly_LEAF = True 'temporary
            If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                Call SelectACwithCDF005()
                DesigningStr = True : LEDFAA13Thick = Thick(1)
            End If

            'If gUserInterrupted Then GoTo UserInterrupted

            If gOnly_LEAF Then
                Call pre_DesignRigidOverlay_NP()  'LEAF stress
            Else
                Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
            End If
        End If

    End Sub


    Private Sub PccOnFlexible(ByRef ct As CancellationToken)
        ct.ThrowIfCancellationRequested()
        If gUserInterrupted Then Exit Sub

        IterLayerChosen = 1
        If Not LifeComputation Then ILayer = 1

        Debug.WriteLine("PCC Overlay on Flexible" & DesignType)

        If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
            Call pre_DesignRigid_NP() 'LEAF
            Call SelectACwithCDF005()
            DesigningStr = True : LEDFAA13Thick = Thick(1)
        End If
        gOnly_LEAF = False
        If gOnly_LEAF Then
            Call pre_DesignRigid_NP() 'LEAF
        Else
            Call DesignRigid_NP(ct) '3D-FEM stress
        End If

        ' Call WriteToFileCtoPsub("PCCOnFlex")

    End Sub
    Private Sub UnbondOnRigid(ByRef ct As CancellationToken)

        ct.ThrowIfCancellationRequested()
        If gUserInterrupted Then Exit Sub

        gOnly_LEAF = False
        If Not LifeComputation Then ILayer = 1

        If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
            Call pre_DesignRigidOverlay_NP()   'LEAF only stress
            Call SelectACwithCDF005()
            DesigningStr = True : LEDFAA13Thick = Thick(1)
        End If

        'gOnly_LEAF = True 'DesignType = UnbondedOnRigid

        If gOnly_LEAF Then
            Call pre_DesignRigidOverlay_NP()  'LEAF stress
        Else
            Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
        End If
        If LifeComputation Then
            LifeStr = OverlayLife
        End If


        ''   Call WriteToFileCtoPsub("UnbondOnRig")
    End Sub


    Private Sub EndTask()
        Dim S1 As String = ""
        Dim S2 As String = ""
        Dim S3 As String = ""
        Dim S4 As String = ""
        Dim S5 As String = ""
        Dim finalCDF As Double
        Dim finalCDFHMA As Double

        Dim LifeStrString As String
        LifeStrString = Format(CType(LifeStr, Double), "N1")

        Dim span = DateTime.Now - TimeStart

        Dim Info, PInfo As String, IncludeImage As Boolean, gISect As Integer
        Info = "" : PInfo = "" : IncludeImage = True : gISect = ISect

        If Section.SelectedRun = 1 Or Section.SelectedRun = 2 Then
            Call FEDFAA1.CreateHTMLinfo(Info, PInfo, gISect, IncludeImage)
        End If

        AddCdfResultToSection(Section)

        For Each airplane In Section.Airplanes

            If airplane.Cdf > finalCDF Then
                finalCDF = airplane.Cdf
                Section.Deterministicaircraft = airplane
            End If

            Dim meas = Job.DesignOptions.MeasurementSystem
            Dim tmp
            Dim TL
            Dim TW
            Dim TA

            Dim gw = airplane.GrossWeight.GetValue(New UsCustomary)
            Dim mgp = airplane.DataStorage(0)
            Dim rmgp = airplane.RunMgPercent
            Dim tp = airplane.Cp.GetValue(New UsCustomary)
            Dim nw = airplane.NumberWheels

            tmp = (gw * mgp / nw / tp) / 2
            TW = CSng(2 * Math.Sqrt(tmp / (1.6 * 3.14159))) ' Minor axis.
            TL = CSng(TW * 1.6) ' Major axis.
            TA = tmp

            airplane.TireWidth = CType(Factory.CreateThickness(TW, New UsCustomary), Thickness)
            airplane.TireLength = CType(Factory.CreateThickness(TL, New UsCustomary), Thickness)
            airplane.TireArea = CType(Factory.CreateArea(TA, New UsCustomary), Area)


        Next

        For i = 1 To Section.Airplanes.Count
            If SavedgPtoC IsNot Nothing Then
                Section.Airplanes(i - 1).CtoP = Format(SavedgPtoC(i), "N2")
            End If
        Next

        If Section.SelectedRun <> 3 Then

            If LifeComputation Then
                S1 = Section.AnalysisType.Name + " Analysis " + "of " + Section.Name + " Completed"
            Else
                S1 = Section.AnalysisType.Name + " Design " + "of " + Section.Name + " Completed" & NL
            End If

            S2 = "Run Time: " + span.TotalSeconds.ToString("f0") + " seconds"
            SS = "Run Time: " + span.TotalSeconds.ToString("f0") + " seconds"
            finalCDF = 0
            finalCDFHMA = 0

            finalCDF = 0
            For Each airplane In Section.Airplanes
                finalCDF = finalCDF + airplane.Cdf
            Next
            If DesignType = NewRigid Or DesignType = PCCOnFlex Then
                If Not LifeComputation Then
                    S3 = "PCC CDF = " + Format(CDFPic, "0.00") + ";  "
                Else
                    S3 = "%CDFU = " + Format(PercentCdfu, "0.00") + ";  "
                    S3 = S3 + "PCC CDF = " + Format(CDFPic, "0.00") + ";  "
                    'SStr = SStr & "Str Life (PCC) = " & VB6.Format(LifeStr, "0.000") & " yrs;  "
                    'S3 = S3 + "Life  = " + Format(LifeStr, "0.0") + " yrs;  "
                    S3 = S3 + "Life = " + LifeStrString + " yrs;  "
                End If
            End If
            If DesignType = NewFlex Or DesignType = FlexOnFlex Then
                If Not LifeComputation Then
                    S3 = "Subgrade CDF = " + Format(CDFPic, "0.00") + ";  "
                Else
                    S3 = "Sub CDF = " + Format(CDFPic, "0.00") + ";  "
                    ''SStr = SStr & "Str Life (SG) = " & VB6.Format(LifeStr, "0.000") & " yrs;  "
                    'S3 = S3 + "Life = " + Format(LifeStr, "0.0") + " yrs;  "
                    S3 = S3 + "Life = " + LifeStrString + " yrs;  "

                End If
            End If

            If DesignType = 11 Or DesignType = 12 Or DesignType = 13 Then 'ik2020.03 added =13
                'SStr = SStr & "Str Life = " & VB6.Format(OverlayLife, "0.0") & " yrs;  "
                If LifeComputation Then
                    If OverlayLife > 500000 Then 'Kairat
                        S3 = "Str Life > 500,000  yrs; "
                    Else
                        S3 = "Life = " + Format(OverlayLife, "N1") & " yrs;  "
                    End If
                End If
            End If
            If ViewModel.AnalysisType.Name.Contains("HMA") Or ViewModel.AnalysisType.Name.Contains("New Flexible") Then

                If Job.DesignOptions.CalculateHmaCdf Then
                    S4 = "HMA CDF  = " + Format(CDFAsp, "0.00").ToString
                End If
            End If

            'If ViewModel.CurrentJob.DesignOptions.ReducedCrossSection = True Then
            '    S5 = "Reduced Cross Section Design in progress"
            'Else
            '    S5 = "Full Cross Section in progress"
            'End If

            ViewModel.MessageText = S1
            ViewModel.MessageText = ViewModel.MessageText & vbNewLine & S2 & vbNewLine & S3 & vbNewLine & S4 & S5

        End If


        If Section.SelectedRun = 3 Then
            Call AirportMasterRecordData()
            S1 = "PCR Calculation " + "of " + Section.Name + " Completed"
            S2 = "Run Time: " + span.TotalSeconds.ToString("f0") + " seconds"
            S4 = "PCR = " + gPCN_Field39

            ViewModel.MessageText = S1 + vbNewLine + S2 + vbNewLine + S4
        End If

        If Section.SelectedRun = 1 Or Section.SelectedRun = 2 Then
            If DesignType = 11 Or DesignType = 12 Or DesignType = 13 Then
                ViewModel._calculatedLife = Format(OverlayLife, "0.000")
                Section.AnalysedLife = ViewModel._calculatedLife
            Else
                ViewModel._calculatedLife = Format(LifeStr, "0.000")
                Section.AnalysedLife = ViewModel._calculatedLife
            End If

        Else
            ViewModel._calculatedLife = Nothing
            Section.AnalysedLife = 0
        End If

        'If MinimumStrainChecker Then
        '    Section.AnalysedLife = 0
        '    ViewModel._calculatedLife = Nothing
        '    ViewModel.MessageText = "Subgrade strains are too low" + vbNewLine + "to accurately compute life."
        'End If

        'If OverflowExit Then
        '    Section.AnalysedLife = 0
        '    ViewModel._calculatedLife = Nothing
        '    ViewModel.MessageText = "The number of aircraft operation is too low" + vbNewLine + "to accurately calculate PCR."
        '    OverflowExit = False
        'End If

        'If LowLifeExit Then
        '    LowLifeExit = False
        'End If

        If Section.SelectedRun <> 3 Then
            For Each aircraft In Section.Airplanes
                aircraft.ACRThick = Factory.CreateThickness(0, Job.DesignOptions.MeasurementSystem)
                aircraft.ACRB = 0
            Next
        End If

        Section.Factory = Factory

        If Not MinimumStrainChecker Then
            For l = 0 To Section.Layers.Count - 1
                If Section.SelectedRun = 0 Or Section.SelectedRun = 2 Then 'Only change thickness value with Thickness Design and Life/Compaction Run, Le
                    If Not Section.Layers(l).Name.Contains("Variable") Then 'Thickness of "Variable" layer will not be changed with any type of Run, Le
                        'If Section.Layers(l).DesignedLayer = "-->" Then
                        If Section.SelectedRun = 0 And Section.Layers(l).Name = "P-209 Crushed Aggregate" Then 'The minimum thickness of P-209 = 6 inches after a thickness run, Le
                            If Section.Factory.CreateThickness(Thick(l + 1), New UsCustomary()).UsCustomary < 6 Then
                                Section.Layers(l).Thickness = Section.Factory.CreateThickness(6, New UsCustomary())
                            Else
                                Section.Layers(l).Thickness = Section.Factory.CreateThickness(Thick(l + 1), New UsCustomary())
                            End If
                        Else
                            Section.Layers(l).Thickness = Section.Factory.CreateThickness(Thick(l + 1), New UsCustomary())
                        End If

                    End If
                End If
                'End If
                'If ViewModel.CurrentJob.DesignOptions.AutomaticFlexibleBaseDesign Then
                Section.Layers(l).Modulus = Section.Factory.CreateModulus(FEDFAA1.Modulus(l + 1), New UsCustomary())
                'End If
            Next
        End If

        If Section.SelectedRun = 3 Then
            For Each airplane In Section.Airplanes
                airplane.RunMgPercent = ((Format(airplane.MgPercentPCN * 1, "0.000")))
            Next
        Else
            For Each airplane In Section.Airplanes
                If airplane.WheelCoordinates.Count <> 1 Then
                    airplane.RunMgPercent = airplane.MgPercent
                Else
                    airplane.RunMgPercent = 1
                End If
            Next
        End If

        Dim depth = 0!
        Dim index = 0
        For Each layer In Section.Layers
            If index < Section.Layers.Count - 1 Then
                depth = depth + layer.Thickness.GetValue(Job.DesignOptions.MeasurementSystem)
            End If
            index = index + 1
        Next

        Section.TotalThickness = Factory.CreateThickness(depth, Job.DesignOptions.MeasurementSystem)

        If Section.SelectedRun = 0 Then
            Section.LastRun = "Thickness Design"
            Section.SectionDesignrunTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        ElseIf Section.SelectedRun = 1 Then
            Section.LastRun = "Life Analysis"
            Section.SectionLifeRunTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        ElseIf Section.SelectedRun = 2 Then
            Section.LastRun = "Life/Compaction Analysis"
            Section.SectionCompactionRunTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        ElseIf Section.SelectedRun = 3 Then
            Section.LastRun = "PCR"
            Section.SectionPCRRunTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")

        End If

        If Section.SelectedRun = 3 Then

            '  Sections(sectionindex).SectionPCRCDF = FEDFAA1.gCDF_target_copy

            Section.SectionPCRPtoTC = FEDFAA1.PtoTC
            Section.SectionPCRCriticalAirplaneName = gNewACName(indexPCNtable2)
            Section.SectionPCRCriticalAnnualDeparture = gNewAnnualDepart(indexPCNtable2)
            Section.SectionPCRNewGL = gNewGL(indexPCNtable2)
            Section.SectionPCRNewPCNThick = gNewPCNthick(indexPCNtable2)
            Section.SectionPCRNewPCN = FEDFAA1.gNewPCN(indexPCNtable2)

            Section.SectionPCRAirportMasterRecordS = s_MGW / 1000
            Section.SectionPCRAirportMasterRecordD = d_MGW / 1000
            Section.SectionPCRAirportMasterRecord2D = dd_MGW / 1000
            Section.SectionPCRAirportMasterRecord2D2 = dd_2d2_MGW / 1000
            Section.SectionPCRAirportMasterRecordFullPCR = gPCN_Field39
            Section.SectionPCRPaveType = sPaveType
            Section.SectionSubgradeCategory = SubgradeCategory
            Section.DesignType = DesignType

            If (Job.DesignOptions.ACROptions = True) Then
                ViewModel._ACRHeader1 = "ACR/" + Section.SectionPCRPaveType + "/A"
                ViewModel.ACRHeader1 = ViewModel._ACRHeader1

                ViewModel._ACRHeader2 = "ACR/" + Section.SectionPCRPaveType + "/B"
                ViewModel.ACRHeader2 = ViewModel._ACRHeader2

                ViewModel._ACRHeader3 = "ACR/" + Section.SectionPCRPaveType + "/C"
                ViewModel.ACRHeader3 = ViewModel._ACRHeader3

                ViewModel._ACRHeader4 = "ACR/" + Section.SectionPCRPaveType + "/D"
                ViewModel.ACRHeader4 = ViewModel._ACRHeader4

                'ViewModel.ACRThickHeader1 = " (A)"
                'ViewModel.ACRThickHeader2 = " (B)"
                'ViewModel.ACRThickHeader3 = " (C)"
                'ViewModel.ACRThickHeader4 = " (D)"

            End If


            If Section.Layers.Item(Sections(sectionindex).Layers.Count - 1).Modulus.UsCustomary >= 21755.66 Then
                ViewModel._ACRHeader = "ACR/" + Section.SectionPCRPaveType + "/A"
                ViewModel.ACRHeader = ViewModel._ACRHeader
                'ViewModel._ACRHeader = "/A"
                ViewModel.ACRThickHeader = " (A)"
            ElseIf Section.Layers.Item(Sections(sectionindex).Layers.Count - 1).Modulus.UsCustomary >= 14503.7738 Then
                ViewModel._ACRHeader = "ACR/" + Section.SectionPCRPaveType + "/B"
                ViewModel.ACRHeader = ViewModel._ACRHeader
                'ViewModel._ACRHeader = "/B"
                ViewModel.ACRThickHeader = " (B)"
            ElseIf Section.Layers.Item(Sections(sectionindex).Layers.Count - 1).Modulus.UsCustomary >= 8702.264 Then
                ViewModel._ACRHeader = "ACR/" + Section.SectionPCRPaveType + "/C"
                ViewModel.ACRHeader = ViewModel._ACRHeader
                'ViewModel._ACRHeader = "/C"
                ViewModel.ACRThickHeader = " (C)"
            Else
                ViewModel._ACRHeader = "ACR/" + Section.SectionPCRPaveType + "/D"
                ViewModel.ACRHeader = ViewModel._ACRHeader
                'ViewModel._ACRHeader = "/D"
                ViewModel.ACRThickHeader = " (D)"

            End If

            ViewModel.UpdateTrafficACRHeaders()

        End If
        If ViewModel.SectionReportIsHidden = False Then
            ViewModel.SectionReportHtml = ViewModel.refreshsectionreport()
        End If

        If ViewModel.CDFGraphIsHidden = False Then
            System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.CDFGraphHtml = ViewModel.refreshcdfgraph())
        End If

        If ViewModel.PCRReportIsHidden = False Then
            ViewModel.PCRReportHtml = ViewModel.refreshPCRReport()

        End If

        If ViewModel.PCRGraphIsHidden = False Then
            System.Windows.Application.Current.Dispatcher.Invoke(Sub() ViewModel.PCRGraphHtml = ViewModel.RefreshPCRGraph())
        End If

        If ViewModel.AirportMasterRecordIsHidden = False Then
            ViewModel.AirportMasterRecordHtml = ViewModel.refreshAirportMasterRecord()
        End If

        If ViewModel.SummaryReportIsHidden = False Then
            ViewModel.SummaryReportHtml = ViewModel.RefreshHtml()
        End If


        If MinimumStrainChecker Then
            Section.AnalysedLife = 0
            ViewModel._calculatedLife = Nothing
            ViewModel.MessageText = "Subgrade strains are too low" + vbNewLine + "to accurately compute life."
            MinimumStrainChecker = False
        End If

        If OverflowExit Then
            Section.AnalysedLife = 0
            ViewModel._calculatedLife = Nothing
            ViewModel.MessageText = "The number of aircraft operation is too low" + vbNewLine + "to accurately calculate PCR."
            OverflowExit = False
        End If

        If LowLifeExit Then
            LowLifeExit = False
        End If


    End Sub

    Private Sub AddCdfResultToSection(section As ISection)
        Dim measurementSystem = Factory.CreateUsCustomary()
        Sections(sectionindex).SectionPCRCDF = 0

        Dim i = 1
        For Each airplane In section.Airplanes
            If section.SelectedRun = 2 And DesignType = 11 Then
                airplane.Cdf = Format(jobCDFtable(1, i), "0.00")
                airplane.CdfAircraftMax = Format(jobCDFacrftMaxtable(1, i), "0.00")


            ElseIf section.SelectedRun = 2 And DesignType = 12 Then
                airplane.Cdf = Format(jobCDFtable(1, i), "0.00")
                airplane.CdfAircraftMax = Format(jobCDFacrftMaxtable(1, i), "0.00")


            ElseIf section.SelectedRun = 1 Or section.SelectedRun = 2 Then 'Or DesignType = PCCOnFlex Then 'Or DesignType = FlexOnRigid Then
                airplane.Cdf = Format(CDFtableTemp(i), "0.00")
                airplane.CdfAircraftMax = Format(CDFacrftMaxtableTemp(i), "0.00")
            ElseIf section.SelectedRun = 3 Then

                'If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then
                '    airplane.Cdf = Format(jobCDFtable(1, i), "0.00")
                '    airplane.CdfAircraftMax = Format(jobCDFacrftMaxtable(1, i), "0.00")

                'Else
                airplane.Cdf = Format(CDFtableTemp3(i), "0.00")
                airplane.CdfAircraftMax = Format(CDFacrftMaxtableTemp3(i), "0.00")
                'End If


            Else

                airplane.Cdf = Format(jobCDFtable(1, i), "0.00")
                airplane.CdfAircraftMax = Format(jobCDFacrftMaxtable(1, i), "0.00")

            End If

            Sections(sectionindex).SectionPCRCDF += airplane.Cdf

            airplane.CdfAc = Format(jobCDFtableAC(1, i), "0.00")
            airplane.CdfAircraftMaxAc = Format(jobCDFacrftMaxtableAC(1, i), "0.00")
            airplane.CdfHma = Format(jobCDFAsp(ISect), "0.00")
            airplane.CdfAircraftMaxHma = Format(jobCDFacrftMaxtableHMA(ISect, i), "0.00")
            airplane.CdfSub = Format(jobCDFacrftMaxtableHMA(1, i), "0.00")
            airplane.CdfAircraftMaxSub = Format(jobCDFacrftMaxtableSub(1, i), "0.00")
            airplane.Cdf401 = Format(jobCDFtable401(1, i), "0.00")
            airplane.CdfAircraftMax401 = Format(jobCDFacrftMaxtable401(1, i), "0.00")

            airplane.CtoP = Format(1 / jobCtoPtable(ISect, i), "0.00")
            airplane.CtoPAc = Format(1 / jobCtoPtableAC(1, i), "0.00")
            airplane.CtoPHma = Format(1 / jobCtoPtableHMA(ISect, i), "0.00")
            airplane.CtoPSub = Format(1 / jobCtoPtableSub(1, i), "0.00")
            airplane.CtoP401 = Format(1 / jobCtoPtable401(1, i), "0.00")

            airplane.TireWidth = Factory.CreateThickness(Format(WT(i), "0.00"), measurementSystem)
            airplane.TireLength = Factory.CreateThickness(Format(TW(i), "0.00"), measurementSystem)
            airplane.TireArea = Factory.CreateArea(Format(Contactarea(i), "0.00"), measurementSystem)
            'airplane.Cp = Factory.CreatePressure(Format(TirePressureF(i), "0.0000"), measurementSystem)
            airplane.Dtemp = Math.Abs(1 / airplane.CtoPHma)
            Try

                airplane.Coverage = Format(ConversionACCovs(i), "0")

            Catch ex As Exception

            End Try
            If section.SelectedRun = 3 Then

                airplane.ACRThickMGW = Format(Convert.ToSingle(gNewPCNthick(ISect)), "0.00")
                airplane.PCRThick = Format(gNewPCN(ISect), "0.00")
                'If ViewModel.CurrentJob.DesignOptions.MeasurementSystem Is GetType(UsCustomary) Then
                '    airplane.ACRThick = Format(gACN_GL_thick(i), "0.0")
                'Else
                '    airplane.ACRThick = Format(gACN_GL_thick(i) * 2.54, "0.00")
                'End If

                airplane.ACRThick = Factory.CreateThickness(Format(gACN_GL_thick(i), "0.0"), measurementSystem)
                airplane.ACRB = Format(gACN_GL(i), "0.0")
                airplane.PCRNumber = FEDFAA1.gACN_GL(i)
                airplane.ACRCoverage = ConversionACCovs(i)

            End If
            If (Job.DesignOptions.ACROptions = True And section.SelectedRun = 3) Then

                If airplane.Name.Contains("Belly") Then
                    airplane.ACRB = Format(0, "0.0")
                    airplane.ACRB1 = Format(0, "0.0")
                    airplane.ACRB2 = Format(0, "0.0")
                    airplane.ACRB3 = Format(0, "0.0")
                    airplane.ACRB4 = Format(0, "0.0")
                    airplane.ACRThick4 = Factory.CreateThickness(Format(0, "0.0"), measurementSystem)
                    airplane.ACRThick3 = Factory.CreateThickness(Format(0, "0.0"), measurementSystem)
                    airplane.ACRThick2 = Factory.CreateThickness(Format(0, "0.0"), measurementSystem)
                    airplane.ACRThick1 = Factory.CreateThickness(Format(0, "0.0"), measurementSystem)
                    airplane.ACRThick = Factory.CreateThickness(Format(0, "0.0"), measurementSystem)
                Else
                    airplane.ACRB1 = Format(ACRThickDataFed(i, 1), "0.0")
                    airplane.ACRB2 = Format(ACRThickDataFed(i, 2), "0.0")
                    airplane.ACRB3 = Format(ACRThickDataFed(i, 3), "0.0")
                    airplane.ACRB4 = Format(ACRThickDataFed(i, 4), "0.0")
                    airplane.ACRThick4 = Factory.CreateThickness(Format(ACRThickSubgradeData(i, 1), "0.0"), measurementSystem)
                    airplane.ACRThick3 = Factory.CreateThickness(Format(ACRThickSubgradeData(i, 2), "0.0"), measurementSystem)
                    airplane.ACRThick2 = Factory.CreateThickness(Format(ACRThickSubgradeData(i, 3), "0.0"), measurementSystem)
                    airplane.ACRThick1 = Factory.CreateThickness(Format(ACRThickSubgradeData(i, 4), "0.0"), measurementSystem)
                End If

            End If


            airplane.gNewAnnualdepartue = gNewAnnualDepart(ISect)
            airplane.gNewGL = Factory.CreateWeight(Format(gNewGL(ISect), "0.0"), measurementSystem)
            'airplane.PCRB = Format(ConversionACCovs(i) / ConversionACCovsToFailure(i), "0.0")
            If airplane.Dtemp <> 0 Then airplane.Dtemp = 1 / airplane.Dtemp
            airplane.Dtemp = Format(airplane.Dtemp, "0.00")
            'If ViewModel.SelectedRun = 3 Then


            If airplane.Name.Contains("A340") And airplane.Name.Contains("Belly") Then
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000")
                airplane.MgPercent = Format(MGpcnt(i), "0.000")
            ElseIf airplane.Name.Contains("MD-11") And airplane.Name.Contains("Belly") Then
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000")
                airplane.MgPercent = Format(MGpcnt(i), "0.000")
            ElseIf airplane.Name.Contains("30/30F/40") And airplane.Name.Contains("Belly") Then
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000")
                airplane.MgPercent = Format(MGpcnt(i), "0.000")
            ElseIf airplane.Name.Contains("IL-86") And airplane.Name.Contains("Belly") Then
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000")
                airplane.MgPercent = Format(MGpcnt(i), "0.000")
            ElseIf airplane.Name.Contains("KC-10") And airplane.Name.Contains("Belly") Then
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000")
                airplane.MgPercent = Format(MGpcnt(i), "0.000")
            ElseIf MGpcnt(i) > 0.5 And airplane.Name.Contains("UDA") Then
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000")
                airplane.MgPercent = Format(MGpcnt(i), "0.000")
            ElseIf airplane.Name.Contains("SWL") Then
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000")
                airplane.MgPercent = Format(MGpcnt(i), "0.000")
            Else
                airplane.MgPercent = Format(MGpcnt(i), "0.000") * 2
                airplane.MgPercentPCN = Format(MGpcnt(i), "0.000") * 2
            End If



            Dim k As Integer
            k = airplane.NumberDepartures
            If section.SelectedRun = 3 Then
                airplane.TotalDepartures = Reps(i)
            ElseIf section.SelectedRun = 2 Or section.SelectedRun = 1 Then
                If Not MinimumStrainChecker Then
                    airplane.TotalDepartures = airplane.NumberDepartures * LifeStr
                Else
                    airplane.TotalDepartures = airplane.NumberDepartures * ViewModel.DesignLife
                    'Else
                End If


                '    airplane.TotalDepartures = airplane.NumberDepartures * ViewModel.DesignLife
            End If
            airplane.NumberDepartures = k
            'Dim Temp1 As Single = ViewModel.DesignLife
            'If section.SelectedRun = 1 Then
            '    Temp1 = LifeStr
            'End If
            'Dim Temp As Single
            'If 1.0! + airplane.AnnualGrowth / 100 * Temp1 < 0.0! Then
            '    Temp1 = -1.0! / airplane.AnnualGrowth / 1000
            'End If
            'Temp = CSng(1.0! + Temp1 * airplane.AnnualGrowth / 100 * 0.5)
            'airplane.TotalDepartures = Temp * airplane.NumberDepartures * Temp1
            If section.SelectedRun <> 3 Then
                For N = 1 To 41
                    airplane.CDFGraphData(N) = CDFdata2(1, i, N)

                Next
            ElseIf section.SelectedRun = 3 Then
                For N = 1 To 41
                    airplane.CDFGraphData(N) = CDFdata3(1, i, N)
                Next

            End If

            ' CM Report alignment: mirror the legacy CDF Graph data source into det.CDFByOffset
            ' so the per-aircraft "CDF vs Offset" chart in the Detailed Computation Report shows
            ' the SAME Y values as FAARFIELD's built-in "CDF Graph" tree-view item. This overrides
            ' the per-iteration capture in modCDF.vb LeafCDFFlex so the snapshot timing matches
            ' the legacy graph (CDFdata2 for thickness/life, CDFdata3 for PCR).
            Try
                If gDetailedReportData IsNot Nothing AndAlso gDetailedReportData.AircraftDetails IsNot Nothing Then
                    If i >= 0 AndAlso i <= UBound(gDetailedReportData.AircraftDetails) Then
                        Dim detSync = gDetailedReportData.AircraftDetails(i)
                        If detSync IsNot Nothing AndAlso detSync.CDFByOffset IsNot Nothing Then
                            For N = 1 To 41
                                If N <= UBound(detSync.CDFByOffset) Then detSync.CDFByOffset(N) = airplane.CDFGraphData(N)
                            Next
                        End If
                    End If
                End If
                If gDetailedReportData IsNot Nothing AndAlso gDetailedReportData.EvaluationAircraftDetails IsNot Nothing Then
                    If i >= 0 AndAlso i <= UBound(gDetailedReportData.EvaluationAircraftDetails) Then
                        Dim detEval = gDetailedReportData.EvaluationAircraftDetails(i)
                        If detEval IsNot Nothing AndAlso detEval.CDFByOffset IsNot Nothing Then
                            For N = 1 To 41
                                If N <= UBound(detEval.CDFByOffset) Then detEval.CDFByOffset(N) = airplane.CDFGraphData(N)
                            Next
                        End If
                    End If
                End If
            Catch ex As Exception
                ' Non-critical alignment; do not block the run.
            End Try

            i = i + 1
        Next


        If section.SectionCDF Is Nothing Then
            section.SectionCDF = New List(Of Single)
            For N = 0 To 42

                section.SectionCDF.Add(0)

            Next
        End If
        i = section.Airplanes.Count + 1
        If section.SelectedRun <> 3 Then
            For N = 1 To 41
                section.SectionCDF(N) = CDFdata2(1, i, N)

            Next
        Else
            For N = 1 To 41
                section.SectionCDF(N) = CDFdata3(1, i, N)
            Next
        End If


    End Sub


    Private Sub LayerPlacingChecks(layers As ObservableCollection(Of IMaterial))
        Dim index = layers.Count - 1

        If Sections(sectionindex).Layers.Count > 25 Then
            message = ("The maximum number of layers for a pavement is 25.")
        End If

        If Sections(sectionindex).Layers.Count > 0 Then
            If layers.Item(index).Name <> "Subgrade" And layers.Item(index).Name <> "User Defined" Then
                message = "Only subgrade or User Defined layers can be placed at the bottom of the structure. " + vbNewLine + "Please change the structure."
            End If

            For i = 0 To index - 1
                If layers.Item(i).Name = "Subgrade" Then
                    message = "Subgrade layers can only be placed on the bottom of a pavement. " + vbNewLine + "Please change the structure."
                End If
            Next
            If message <> "" Then
                MessageBox.Show(message)
                warning = 1
                message = ""

            End If
            'Displays message if the "sandwich" condition is met. (aggregate between two stiffer layers)
            For i = 0 To index - 1
                'Checks for aggregate layer and avoid check against bottom layer
                If layers.Item(i).Category = "Aggregate" And i < index - 1 Then
                    If (layers.Item(i).Modulus.UsCustomary < layers.Item(i + 1).Modulus.UsCustomary) And (layers.Item(i).Modulus.UsCustomary < layers.Item(i - 1).Modulus.UsCustomary) _
                        And Sections(sectionindex).AnalysisType.Name <> "HMA Overlay on Rigid" Then
                        message = "Aggregate is between two stiffer layers." + vbNewLine + "Please change the structure."
                        SandwichBool = True

                    End If
                End If
            Next
        End If
    End Sub

    Private Sub DesignTypeChecks(Layers As ObservableCollection(Of IMaterial))
        If Sections(sectionindex).Layers.Count > 0 Then


            If Layers.Item(0).Name = "P-401/P-403 HMA Surface" Then
                If Layers.Item(1).Name = "P-501 PCC Surface" Then
                    message = "PCC layers can only be placed on top layers of a pavement structure or directly below an asphalt or PCC overlay. " + vbNewLine + "Please change the structure."
                ElseIf Layers.Item(1).Name = "P-401/P-403 HMA Overlay" Or Layers.Item(1).Name = "P-501 PCC Overlay (unbonded)" Or Layers.Item(1).Name = "P-501 PCC Overlay (partially bonded)" Then
                    message = "Overlays can only be placed on top layers of a pavement structure for the design procedure to succeed. " + vbNewLine + "Please change the structure."

                Else
                    DesignType = NewRigid
                End If
            End If
            If Layers.Item(0).Name = "P-501 PCC Surface" Then
                If Layers.Item(1).Name = "P-401/P-403 HMA Surface" Then
                    message = "Asphalt layers can only be placed on top layers of a pavement structure or directly below an asphalt or PCC overlay. " + vbNewLine + "Please change the structure."
                ElseIf Layers.Item(1).Name = "P-401/P-403 HMA Overlay" Or Layers.Item(1).Name = "P-501 PCC Overlay (unbonded)" Or Layers.Item(1).Name = "P-501 PCC Overlay (partially bonded)" Then
                    message = "Overlays can only be placed on top layers of a pavement structure for the design procedure to succeed. " + vbNewLine + "Please change the structure."

                Else
                    DesignType = NewRigid
                End If
            End If
            If Layers.Item(0).Name = "P-401/P-403 HMA Overlay" Then
                If Layers.Item(1).Name = "P-401/P-403 HMA Surface" Or Layers.Item(1).Name = "User Defined" Then
                    DesignType = FlexOnFlex
                ElseIf Layers.Item(1).Name = "P-501 PCC Surface" Then
                    DesignType = FlexOnRigid
                Else
                    message = "P-401/P-403 HMA Ovrlay can only be placed over asphalt surface, PCC layers, or user defined layers. " + vbNewLine + "Please change the structure."

                End If
            End If

            If Layers.Item(0).Name = "P-501 PCC Overlay (unbonded)" Then
                If Layers.Item(1).Name = "P-501 PCC Surface" Then
                    DesignType = 11
                Else
                    message = "PCC overlay (unbonded) can only be placed over PCC layers. " + vbNewLine + "Please change the structure."
                End If
            End If
            If Layers.Item(0).Name = "P-501 PCC Overlay (partially bonded)" Then
                If Layers.Item(1).Name = "P-501 PCC Surface" Then
                    DesignType = 12
                Else
                    message = "PCC overlay (partially bonded) can only be placed over PCC layers. " + vbNewLine + "Please change the structure."
                End If
            End If
            If Layers.Item(0).Name = "P-501 PCC Overlay on Flexible" Then
                If Layers.Item(1).Name = "P-401/P-403 HMA Surface" Or Layers.Item(1).Name = "User Defined" Then
                    DesignType = PCCOnFlex
                Else
                    message = "P-501 PCC Overlay on Flexible can only be placed over asphalt surface or user defined layers. " + vbNewLine + "Please change the structure."
                End If
            End If

            If Layers.Item(0).Category = "Stabilized" Or Layers.Item(0).Category = "Aggregate" Then
                message = Layers.Item(0).Name + " layers can not be placed on the top of the structure. " + vbNewLine + "Please change the structure."
            End If
            For i = 1 To Layers.Count - 1
                If Layers.Item(i).Name.Contains("Overlay") Then
                    message = Layers.Item(i).Name + " layers can only be placed as the top layer. " + vbNewLine + "Please change the structure."

                End If
            Next
            If message <> "" Then
                MessageBox.Show(message)
                warning = 1
                message = ""
            End If
        End If
    End Sub




End Class
