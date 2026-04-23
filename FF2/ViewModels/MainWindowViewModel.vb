Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.IO
Imports System.IO.IsolatedStorage
Imports System.Reflection
Imports System.Runtime.Serialization
Imports System.Text
Imports System.Windows.Threading
Imports System.Xml
Imports FF2.Libs
Imports FaarFieldAnalysis
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.Win32
Imports Telerik.Windows.Controls
Imports Telerik.Windows.Data
Imports Telerik.Windows.DragDrop
Imports Telerik.Windows.Controls.ChartView
Imports System.Globalization
Imports System.Net.Http
Imports System.Net

Namespace ViewModels

    Public Class MainWindowViewModel
        Inherits Utilities.ViewModelBase

#Region "Fields and Properties"

        Private _MainWindowTitle As String = "FAARFIELD 2.1.1"
        Public Property MainWindowTitle As String
            Get
                Return _MainWindowTitle
            End Get
            Set(ByVal value As String)
                _MainWindowTitle = value
                OnPropertyChanged(NameOf(MainWindowTitle))
            End Set
        End Property


        Public _BuildDate As String
        Public Property BuildDate As String
            Get
                Return _BuildDate
            End Get
            Set(ByVal value As String)
                _BuildDate = value
                OnPropertyChanged(NameOf(BuildDate))
            End Set
        End Property


        Private _TimerVisibility As Visibility = Visibility.Hidden
        Public Property TimerVisibility As Visibility
            Get
                Return _TimerVisibility
            End Get
            Set(value As Visibility)
                _TimerVisibility = value
                OnPropertyChanged(NameOf(TimerVisibility))
            End Set
        End Property


        Private _PCAConversionFormula As Boolean
        Public Property PCAConversionFormula As Boolean
            Get
                Return _PCAConversionFormula
            End Get
            Set(value As Boolean)
                If _PCAConversionFormula <> value Then
                    ResetCalculatedAirplaneValues()
                End If
                _PCAConversionFormula = value
                If _PCAConversionFormula = True Then
                    'If NCHRPFormula = True Then
                    '    MessageBox.Show("CBR-E NCHRP1-37A option changed to NO to be able to use E-k PCA Conversion ")
                    '    _NCHRPFormula = False
                    '    NCHRPFormula = _NCHRPFormula
                    'End If
                    Try
                        CurrentSectionView.Section.PCAConversionTracker = True

                        If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                            If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).KValueActive Then
                                CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).PCAConversionActive = True
                            End If
                        End If

                    Catch ex As Exception
                        Debug.WriteLine("PCAConversionFormula setter: " & ex.Message)
                    End Try
                Else

                    CurrentSectionView.Section.PCAConversionTracker = False

                    If CurrentSectionView.Section.AnalysisType IsNot Nothing AndAlso (CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid") Then
                        If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).KValueActive Then
                            CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).PCAConversionActive = False
                        End If
                    End If

                End If
                If CurrentSectionView IsNot Nothing Then
                    ProfileImage = DrawProfile()
                End If

                OnPropertyChanged(NameOf(PCAConversionFormula))
                OnPropertyChanged(NameOf(CurrentSectionView))
                OnPropertyChanged(NameOf(CurrentLayers))
            End Set
        End Property


        Private _NCHRPFormula As Boolean
        Public Property NCHRPFormula As Boolean
            Get
                Return _NCHRPFormula
            End Get
            Set(value As Boolean)
                If _NCHRPFormula <> value Then
                    ResetCalculatedAirplaneValues()
                End If
                _NCHRPFormula = value

                If _NCHRPFormula = True Then
                    'If PCAConversionFormula = True Then
                    '    MessageBox.Show("E-k PCA Conversion option changed to NO to be able to use CBR-E NCHRP1-37A")
                    '    _PCAConversionFormula = False
                    '    PCAConversionFormula = _PCAConversionFormula
                    'End If
                    Try
                        CurrentSectionView.Section.NCHRPTracker = True

                        If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                            If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).CBRActive Then
                                CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).NCHRPActive = True
                            End If
                        End If

                    Catch ex As Exception
                        Debug.WriteLine("NCHRPFormula setter: " & ex.Message)
                    End Try

                Else
                    CurrentSectionView.Section.NCHRPTracker = False

                    If CurrentSectionView.Section.AnalysisType IsNot Nothing AndAlso (CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible") Then
                        If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).CBRActive Then
                            CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).NCHRPActive = False
                        End If
                    End If

                End If

                If CurrentSectionView IsNot Nothing Then
                    ProfileImage = DrawProfile()
                End If

                OnPropertyChanged(NameOf(NCHRPFormula))
                OnPropertyChanged(NameOf(CurrentSectionView))
                OnPropertyChanged(NameOf(CurrentLayers))
            End Set
        End Property


        Private _TimerText As String = "00:00:00"
        Public Property TimerText As String
            Get
                Return _TimerText
            End Get
            Set(value As String)
                _TimerText = value
                OnPropertyChanged(NameOf(TimerText))
            End Set
        End Property


        Private _RunningTimeVisibility As Visibility = Visibility.Hidden
        Public Property RunningTimeVisibility As Visibility
            Get
                Return _RunningTimeVisibility
            End Get
            Set(value As Visibility)
                _RunningTimeVisibility = value
                OnPropertyChanged(NameOf(RunningTimeVisibility))
            End Set
        End Property


        Private _RunningTimeText As String = "Running Time : "
        Public Property RunningTimeText As String
            Get
                Return _RunningTimeText
            End Get
            Set(value As String)
                _RunningTimeText = value
                OnPropertyChanged(NameOf(RunningTimeText))
            End Set
        End Property


        ' Show the "Cross Section in progress"
        Private _CrossSectionVisibility As Visibility = Visibility.Hidden
        Public Property CrossSectionVisibility As Visibility
            Get
                Return _CrossSectionVisibility
            End Get
            Set(value As Visibility)
                _CrossSectionVisibility = value
                OnPropertyChanged(NameOf(CrossSectionVisibility))
            End Set
        End Property


        Private _CrossSectionText As String = Nothing
        Public Property CrossSectionText As String
            Get
                Return _CrossSectionText
            End Get
            Set(value As String)
                _CrossSectionText = value
                OnPropertyChanged(NameOf(CrossSectionText))
            End Set
        End Property


        Private _StopwatchVisibility As Visibility = Visibility.Hidden
        Public Property StopwatchVisibility As Visibility
            Get
                Return _StopwatchVisibility
            End Get
            Set(value As Visibility)
                _StopwatchVisibility = value
                OnPropertyChanged(NameOf(StopwatchVisibility))
            End Set
        End Property


        Private _StopwatchText As String = "Running Time : "
        Public Property StopwatchText As String
            Get
                Return _StopwatchText
            End Get
            Set(value As String)
                _StopwatchText = value
                OnPropertyChanged(NameOf(StopwatchText))
            End Set
        End Property


        ' ── Progress banner properties ──

        Private _ProgressBannerVisibility As Visibility = Visibility.Collapsed
        Public Property ProgressBannerVisibility As Visibility
            Get
                Return _ProgressBannerVisibility
            End Get
            Set(value As Visibility)
                _ProgressBannerVisibility = value
                OnPropertyChanged(NameOf(ProgressBannerVisibility))
            End Set
        End Property

        Private _ProgressBannerBackground As Media.Brush = New Media.SolidColorBrush(Media.Color.FromRgb(&HE3, &HF2, &HFD))
        Public Property ProgressBannerBackground As Media.Brush
            Get
                Return _ProgressBannerBackground
            End Get
            Set(value As Media.Brush)
                _ProgressBannerBackground = value
                OnPropertyChanged(NameOf(ProgressBannerBackground))
            End Set
        End Property

        Private _ProgressIcon As String = ""
        Public Property ProgressIcon As String
            Get
                Return _ProgressIcon
            End Get
            Set(value As String)
                _ProgressIcon = value
                OnPropertyChanged(NameOf(ProgressIcon))
            End Set
        End Property

        Private _ProgressStatusText As String = ""
        Public Property ProgressStatusText As String
            Get
                Return _ProgressStatusText
            End Get
            Set(value As String)
                _ProgressStatusText = value
                OnPropertyChanged(NameOf(ProgressStatusText))
            End Set
        End Property

        Private _ProgressTextBrush As Media.Brush = Media.Brushes.DarkSlateGray
        Public Property ProgressTextBrush As Media.Brush
            Get
                Return _ProgressTextBrush
            End Get
            Set(value As Media.Brush)
                _ProgressTextBrush = value
                OnPropertyChanged(NameOf(ProgressTextBrush))
            End Set
        End Property

        Private _ProgressIsIndeterminate As Boolean = False
        Public Property ProgressIsIndeterminate As Boolean
            Get
                Return _ProgressIsIndeterminate
            End Get
            Set(value As Boolean)
                _ProgressIsIndeterminate = value
                OnPropertyChanged(NameOf(ProgressIsIndeterminate))
                OnPropertyChanged(NameOf(ProgressPercentText))
            End Set
        End Property

        Private _ProgressValue As Double = 0
        Public Property ProgressValue As Double
            Get
                Return _ProgressValue
            End Get
            Set(value As Double)
                _ProgressValue = value
                OnPropertyChanged(NameOf(ProgressValue))
                OnPropertyChanged(NameOf(ProgressPercentText))
            End Set
        End Property

        ''' <summary>
        ''' Formatted percentage text for the progress bar overlay (e.g. "100%" or "Complete").
        ''' </summary>
        Public ReadOnly Property ProgressPercentText As String
            Get
                If _ProgressIsIndeterminate Then Return ""
                If _ProgressValue >= 100 Then Return "Complete"
                If _ProgressValue <= 0 Then Return ""
                Return CInt(_ProgressValue).ToString() & "%"
            End Get
        End Property

        Private _ProgressBarBrush As Media.Brush = New Media.SolidColorBrush(Media.Color.FromRgb(&H15, &H65, &HC0))
        Public Property ProgressBarBrush As Media.Brush
            Get
                Return _ProgressBarBrush
            End Get
            Set(value As Media.Brush)
                _ProgressBarBrush = value
                OnPropertyChanged(NameOf(ProgressBarBrush))
            End Set
        End Property

        Private _ProgressSubText As String = ""
        Public Property ProgressSubText As String
            Get
                Return _ProgressSubText
            End Get
            Set(value As String)
                _ProgressSubText = value
                OnPropertyChanged(NameOf(ProgressSubText))
            End Set
        End Property

        Private _ProgressSubTextVisibility As Visibility = Visibility.Collapsed
        Public Property ProgressSubTextVisibility As Visibility
            Get
                Return _ProgressSubTextVisibility
            End Get
            Set(value As Visibility)
                _ProgressSubTextVisibility = value
                OnPropertyChanged(NameOf(ProgressSubTextVisibility))
            End Set
        End Property

        Private _ProgressSubTextBrush As Media.Brush = Media.Brushes.Gray
        Public Property ProgressSubTextBrush As Media.Brush
            Get
                Return _ProgressSubTextBrush
            End Get
            Set(value As Media.Brush)
                _ProgressSubTextBrush = value
                OnPropertyChanged(NameOf(ProgressSubTextBrush))
            End Set
        End Property


        ''' <summary>
        ''' Configures the progress banner for the "running" state (indeterminate bar, blue theme).
        ''' </summary>
        Public Sub ShowProgressRunning(statusText As String, Optional subText As String = Nothing)
            ProgressBannerVisibility = Visibility.Visible
            ProgressBannerBackground = New Media.SolidColorBrush(Media.Color.FromRgb(&HE3, &HF2, &HFD))
            ProgressIcon = ChrW(&H23F3)  ' hourglass
            ProgressStatusText = statusText
            ProgressTextBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&H0D, &H47, &HA1))
            ProgressIsIndeterminate = True
            ProgressValue = 0
            ProgressBarBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&H15, &H65, &HC0))
            If subText IsNot Nothing Then
                ProgressSubText = subText
                ProgressSubTextVisibility = Visibility.Visible
                ProgressSubTextBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&H54, &H6E, &H7A))
            Else
                ProgressSubTextVisibility = Visibility.Collapsed
            End If
        End Sub


        ''' <summary>
        ''' Configures the progress banner for the "completed" state (full bar, green theme).
        ''' </summary>
        Public Sub ShowProgressCompleted(elapsedText As String)
            ProgressBannerVisibility = Visibility.Visible
            ProgressBannerBackground = New Media.SolidColorBrush(Media.Color.FromRgb(&HE8, &HF5, &HE9))
            ProgressIcon = ChrW(&H2705)  ' check mark
            ProgressStatusText = "Analysis Complete"
            ProgressTextBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&H1B, &H5E, &H20))
            ProgressIsIndeterminate = False
            ProgressValue = 100
            ProgressBarBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&H2E, &H7D, &H32))
            ProgressSubText = "Completed in " & elapsedText & " — results are ready for review."
            ProgressSubTextVisibility = Visibility.Visible
            ProgressSubTextBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&H2E, &H7D, &H32))
        End Sub


        ''' <summary>
        ''' Configures the progress banner for the "canceled" state (amber theme).
        ''' </summary>
        Public Sub ShowProgressCanceled()
            ProgressBannerVisibility = Visibility.Visible
            ProgressBannerBackground = New Media.SolidColorBrush(Media.Color.FromRgb(&HFF, &HF3, &HE0))
            ProgressIcon = ChrW(&H26A0)  ' warning
            ProgressStatusText = "Analysis Canceled"
            ProgressTextBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&HE6, &H51, &H00))
            ProgressIsIndeterminate = False
            ProgressValue = 0
            ProgressBarBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&HFF, &HA0, &H00))
            ProgressSubTextVisibility = Visibility.Collapsed
        End Sub


        ''' <summary>
        ''' Hides the progress banner.
        ''' </summary>
        ''' <summary>
        ''' Configures the progress banner for the "failed" state (red theme).
        ''' </summary>
        Public Sub ShowProgressFailed(errorMessage As String)
            ProgressBannerVisibility = Visibility.Visible
            ProgressBannerBackground = New Media.SolidColorBrush(Media.Color.FromRgb(&HFD, &HE8, &HE8))
            ProgressIcon = ChrW(&H274C)  ' cross mark
            ProgressStatusText = "Analysis Failed"
            ProgressTextBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&HC6, &H28, &H28))
            ProgressIsIndeterminate = False
            ProgressValue = 0
            ProgressBarBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&HDC, &H26, &H26))
            ProgressSubText = errorMessage
            ProgressSubTextVisibility = Visibility.Visible
            ProgressSubTextBrush = New Media.SolidColorBrush(Media.Color.FromRgb(&H99, &H1B, &H1B))
        End Sub

        Public Sub HideProgressBanner()
            ProgressBannerVisibility = Visibility.Collapsed
            ProgressIsIndeterminate = False
        End Sub

#Region "Toast Notification"

        Private _ToastVisible As Boolean = False
        Public Property ToastVisible As Boolean
            Get
                Return _ToastVisible
            End Get
            Set(value As Boolean)
                _ToastVisible = value
                OnPropertyChanged(NameOf(ToastVisible))
            End Set
        End Property

        Private _ToastMessage As String = ""
        Public Property ToastMessage As String
            Get
                Return _ToastMessage
            End Get
            Set(value As String)
                _ToastMessage = value
                OnPropertyChanged(NameOf(ToastMessage))
            End Set
        End Property

        Private _ToastIcon As String = ""
        Public Property ToastIcon As String
            Get
                Return _ToastIcon
            End Get
            Set(value As String)
                _ToastIcon = value
                OnPropertyChanged(NameOf(ToastIcon))
            End Set
        End Property

        Private _ToastBackground As Media.Brush = New Media.SolidColorBrush(Media.Color.FromRgb(&HE8, &HF5, &HE9))
        Public Property ToastBackground As Media.Brush
            Get
                Return _ToastBackground
            End Get
            Set(value As Media.Brush)
                _ToastBackground = value
                OnPropertyChanged(NameOf(ToastBackground))
            End Set
        End Property

        Private _toastTimer As Windows.Threading.DispatcherTimer

        ''' <summary>
        ''' Shows an auto-dismissing toast notification at top-right of the window.
        ''' </summary>
        ''' <param name="message">The message to display.</param>
        ''' <param name="icon">Unicode icon character (default: checkmark).</param>
        ''' <param name="durationSeconds">Seconds before auto-dismiss (default: 3).</param>
        ''' <param name="isError">If True, uses red/error styling instead of green/success.</param>
        Public Sub ShowToast(message As String, Optional icon As String = Nothing, Optional durationSeconds As Integer = 3, Optional isError As Boolean = False)
            If icon Is Nothing Then
                icon = If(isError, ChrW(&H274C), ChrW(&H2714)) ' cross or checkmark
            End If

            ToastMessage = message
            ToastIcon = icon

            If isError Then
                ToastBackground = New Media.SolidColorBrush(Media.Color.FromRgb(&HFD, &HE8, &HE8))
            Else
                ToastBackground = New Media.SolidColorBrush(Media.Color.FromRgb(&HE8, &HF5, &HE9))
            End If

            ToastVisible = True

            ' Cancel any existing timer
            If _toastTimer IsNot Nothing Then
                _toastTimer.Stop()
            End If

            ' Auto-dismiss after duration
            _toastTimer = New Windows.Threading.DispatcherTimer()
            _toastTimer.Interval = TimeSpan.FromSeconds(durationSeconds)
            AddHandler _toastTimer.Tick, Sub(s, e)
                                             ToastVisible = False
                                             _toastTimer.Stop()
                                         End Sub
            _toastTimer.Start()
        End Sub

#End Region

        Private _UserDefinedLIsEnabled As Boolean = True
        Public Property UserDefinedLIsEnabled As Boolean
            Get
                Return _UserDefinedLIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _UserDefinedLIsEnabled = value
                OnPropertyChanged(NameOf(UserDefinedLIsEnabled))
            End Set
        End Property


        Private _SubgradeLIsEnabled As Boolean = True
        Public Property SubgradeLIsEnabled As Boolean
            Get
                Return _SubgradeLIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _SubgradeLIsEnabled = value
                OnPropertyChanged(NameOf(SubgradeLIsEnabled))
            End Set
        End Property


        Private _HMAOverlayIsEnabled As Boolean = True
        Public Property HMAOverlayIsEnabled As Boolean
            Get
                Return _HMAOverlayIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _HMAOverlayIsEnabled = value
                OnPropertyChanged(NameOf(HMAOverlayIsEnabled))
            End Set
        End Property


        Private _PCCOverlayFLIsEnabled As Boolean = True
        Public Property PCCOverlayFLIsEnabled As Boolean
            Get
                Return _PCCOverlayFLIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _PCCOverlayFLIsEnabled = value
                OnPropertyChanged(NameOf(PCCOverlayFLIsEnabled))
            End Set
        End Property


        Private _PCCOverlayUnLIsEnabled As Boolean = True
        Public Property PCCOverlayUnLIsEnabled As Boolean
            Get
                Return _PCCOverlayUnLIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _PCCOverlayUnLIsEnabled = value
                OnPropertyChanged(NameOf(PCCOverlayUnLIsEnabled))
            End Set
        End Property


        Private _HMASurfaceIsEnabled As Boolean = True
        Public Property HMASurfaceIsEnabled As Boolean
            Get
                Return _HMASurfaceIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _HMASurfaceIsEnabled = value
                OnPropertyChanged(NameOf(HMASurfaceIsEnabled))
            End Set
        End Property


        Private _PCCSurfaceLIsEnabled As Boolean = True
        Public Property PCCSurfaceLIsEnabled As Boolean
            Get
                Return _PCCSurfaceLIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _PCCSurfaceLIsEnabled = value
                OnPropertyChanged(NameOf(PCCSurfaceLIsEnabled))
            End Set
        End Property


        Private _P301LIsEnabled As Boolean = True
        Public Property P301LIsEnabled As Boolean
            Get
                Return _P301LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P301LIsEnabled = value
                OnPropertyChanged(NameOf(P301LIsEnabled))
            End Set
        End Property


        Private _P304LIsEnabled As Boolean = True
        Public Property P304LIsEnabled As Boolean
            Get
                Return _P304LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P304LIsEnabled = value
                OnPropertyChanged(NameOf(P304LIsEnabled))
            End Set
        End Property


        Private _P306LIsEnabled As Boolean = True
        Public Property P306LIsEnabled As Boolean
            Get
                Return _P306LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P306LIsEnabled = value
                OnPropertyChanged(NameOf(P306LIsEnabled))
            End Set
        End Property


        Private _HMAStLIsEnabled As Boolean = True
        Public Property HMAStLIsEnabled As Boolean
            Get
                Return _HMAStLIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _HMAStLIsEnabled = value
                OnPropertyChanged(NameOf(HMAStLIsEnabled))
            End Set
        End Property


        Private _VariableFlIsEnabled As Boolean = True
        Public Property VariableFlIsEnabled As Boolean
            Get
                Return _VariableFlIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _VariableFlIsEnabled = value
                OnPropertyChanged(NameOf(VariableFlIsEnabled))
            End Set
        End Property


        Private _VariableRgIsEnabled As Boolean = True
        Public Property VariableRgIsEnabled As Boolean
            Get
                Return _VariableRgIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _VariableRgIsEnabled = value
                OnPropertyChanged(NameOf(VariableRgIsEnabled))
            End Set
        End Property


        Private _P154LIsEnabled As Boolean = True
        Public Property P154LIsEnabled As Boolean
            Get
                Return _P154LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P154LIsEnabled = value
                OnPropertyChanged(NameOf(P154LIsEnabled))
            End Set
        End Property


        Private _P208LIsEnabled As Boolean = True
        Public Property P208LIsEnabled As Boolean
            Get
                Return _P208LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P208LIsEnabled = value
                OnPropertyChanged(NameOf(P208LIsEnabled))
            End Set
        End Property


        Private _P209LIsEnabled As Boolean = True
        Public Property P209LIsEnabled As Boolean
            Get
                Return _P209LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P209LIsEnabled = value
                OnPropertyChanged(NameOf(P209LIsEnabled))
            End Set
        End Property


        Private _P211LIsEnabled As Boolean = True
        Public Property P211LIsEnabled As Boolean
            Get
                Return _P211LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P211LIsEnabled = value
                OnPropertyChanged(NameOf(P211LIsEnabled))
            End Set
        End Property


        Private _P219LIsEnabled As Boolean = True
        Public Property P219LIsEnabled As Boolean
            Get
                Return _P219LIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _P219LIsEnabled = value
                OnPropertyChanged(NameOf(P219LIsEnabled))
            End Set
        End Property


        Private _AddLayerAboveIsEnabled As Boolean = True
        Public Property AddLayerAboveIsEnabled As Boolean
            Get
                Return _AddLayerAboveIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _AddLayerAboveIsEnabled = value
                OnPropertyChanged(NameOf(AddLayerAboveIsEnabled))

                If _AddLayerAbove_Command IsNot Nothing Then
                    _AddLayerAbove_Command.CanExecute(value)
                End If

            End Set
        End Property


        Private _AddLayerBelowIsEnabled As Boolean = True
        Public Property AddLayerBelowIsEnabled As Boolean
            Get
                Return _AddLayerBelowIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _AddLayerBelowIsEnabled = value
                OnPropertyChanged(NameOf(AddLayerBelowIsEnabled))

                If _AddLayerBelow_Command IsNot Nothing Then
                    _AddLayerBelow_Command.CanExecute(value)
                End If

            End Set
        End Property


        Private _RunAnalysisIsEnabled As Boolean = False
        Public Property RunAnalysisIsEnabled As Boolean
            Get
                Return _RunAnalysisIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _RunAnalysisIsEnabled = value
                OnPropertyChanged(NameOf(RunAnalysisIsEnabled))
                If _RunButton_Command IsNot Nothing Then
                    _RunButton_Command.CanExecute(value)
                End If
                If _AddToBatch_Command IsNot Nothing Then
                    _AddToBatch_Command.CanExecute(value)
                    If Not value Then
                        CurrentSectionView.Section.RunBatch = value
                    End If
                End If
            End Set
        End Property


        Private _XWCoordinate1Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate1Visibility As Visibility
            Get
                Return _XWCoordinate1Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate1Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate1Visibility))
            End Set
        End Property


        Private _XWCoordinate1Text As String = ""
        Public Property XWCoordinate1Text As String
            Get
                Return _XWCoordinate1Text
            End Get
            Set(value As String)
                _XWCoordinate1Text = value
                OnPropertyChanged(NameOf(XWCoordinate1Text))
            End Set
        End Property


        Private _XWCoordinate2Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate2Visibility As Visibility
            Get
                Return _XWCoordinate2Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate2Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate2Visibility))
            End Set
        End Property


        Private _XWCoordinate2Text As String = ""
        Public Property XWCoordinate2Text As String
            Get
                Return _XWCoordinate2Text
            End Get
            Set(value As String)
                _XWCoordinate2Text = value
                OnPropertyChanged(NameOf(XWCoordinate2Text))
            End Set
        End Property


        Private _XWCoordinate3Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate3Visibility As Visibility
            Get
                Return _XWCoordinate3Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate3Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate3Visibility))
            End Set
        End Property


        Private _XWCoordinate3Text As String = ""
        Public Property XWCoordinate3Text As String
            Get
                Return _XWCoordinate3Text
            End Get
            Set(value As String)
                _XWCoordinate3Text = value
                OnPropertyChanged(NameOf(XWCoordinate3Text))
            End Set
        End Property


        Private _XWCoordinate4Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate4Visibility As Visibility
            Get
                Return _XWCoordinate4Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate4Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate4Visibility))
            End Set
        End Property


        Private _XWCoordinate4Text As String = ""
        Public Property XWCoordinate4Text As String
            Get
                Return _XWCoordinate4Text
            End Get
            Set(value As String)
                _XWCoordinate4Text = value
                OnPropertyChanged(NameOf(XWCoordinate4Text))
            End Set
        End Property


        Private _XWCoordinate5Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate5Visibility As Visibility
            Get
                Return _XWCoordinate5Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate5Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate5Visibility))
            End Set
        End Property


        Private _XWCoordinate5Text As String = ""
        Public Property XWCoordinate5Text As String
            Get
                Return _XWCoordinate5Text
            End Get
            Set(value As String)
                _XWCoordinate5Text = value
                OnPropertyChanged(NameOf(XWCoordinate5Text))
            End Set
        End Property


        Private _XWCoordinate6Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate6Visibility As Visibility
            Get
                Return _XWCoordinate6Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate6Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate6Visibility))
            End Set
        End Property


        Private _XWCoordinate6Text As String = ""
        Public Property XWCoordinate6Text As String
            Get
                Return _XWCoordinate6Text
            End Get
            Set(value As String)
                _XWCoordinate6Text = value
                OnPropertyChanged(NameOf(XWCoordinate6Text))
            End Set
        End Property


        Private _XWCoordinate7Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate7Visibility As Visibility
            Get
                Return _XWCoordinate7Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate7Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate7Visibility))
            End Set
        End Property


        Private _XWCoordinate7Text As String = ""
        Public Property XWCoordinate7Text As String
            Get
                Return _XWCoordinate7Text
            End Get
            Set(value As String)
                _XWCoordinate7Text = value
                OnPropertyChanged(NameOf(XWCoordinate7Text))
            End Set
        End Property


        Private _XWCoordinate8Visibility As Visibility = Visibility.Hidden
        Public Property XWCoordinate8Visibility As Visibility
            Get
                Return _XWCoordinate8Visibility
            End Get
            Set(value As Visibility)
                _XWCoordinate8Visibility = value
                OnPropertyChanged(NameOf(XWCoordinate8Visibility))
            End Set
        End Property


        Private _XWCoordinate8Text As String = ""
        Public Property XWCoordinate8Text As String
            Get
                Return _XWCoordinate8Text
            End Get
            Set(value As String)
                _XWCoordinate8Text = value
                OnPropertyChanged(NameOf(XWCoordinate8Text))
            End Set
        End Property


        Private _ThicknessUpdateEnabled As Boolean = True
        Public Property ThicknessUpdateEnabled As Boolean
            Get
                Return _ThicknessUpdateEnabled
            End Get
            Set(value As Boolean)
                _ThicknessUpdateEnabled = value
                OnPropertyChanged(NameOf(ThicknessUpdateEnabled))
            End Set
        End Property


        Private _ModulusUpdateEnabled As Boolean = True
        Public Property ModulusUpdateEnabled As Boolean
            Get
                Return _ModulusUpdateEnabled
            End Get
            Set(value As Boolean)
                _ModulusUpdateEnabled = value
                OnPropertyChanged(NameOf(ModulusUpdateEnabled))
            End Set
        End Property


        Private _RuptureUpdateEnabled As Boolean = True
        Public Property RuptureUpdateEnabled As Boolean
            Get
                Return _RuptureUpdateEnabled
            End Get
            Set(value As Boolean)
                _RuptureUpdateEnabled = value
                OnPropertyChanged(NameOf(RuptureUpdateEnabled))
            End Set
        End Property


        Private _CBRUpdateEnabled As Boolean = True
        Public Property CBRUpdateEnabled As Boolean
            Get
                Return _CBRUpdateEnabled
            End Get
            Set(value As Boolean)
                _CBRUpdateEnabled = value
                OnPropertyChanged(NameOf(CBRUpdateEnabled))
            End Set
        End Property


        Private _KValueUpdateEnabled As Boolean = True
        Public Property KValueUpdateEnabled As Boolean
            Get
                Return _KValueUpdateEnabled
            End Get
            Set(value As Boolean)
                _KValueUpdateEnabled = value
                OnPropertyChanged(NameOf(KValueUpdateEnabled))
            End Set
        End Property


        Private _CloseJobMenuItemVisibility As Visibility = Visibility.Collapsed
        Public Property CloseJobMenuItemVisibility As Visibility
            Get
                Return _CloseJobMenuItemVisibility
            End Get
            Set(value As Visibility)
                _CloseJobMenuItemVisibility = value
                OnPropertyChanged(NameOf(CloseJobMenuItemVisibility))
            End Set
        End Property


        Private _UserDefinedLIsChecked As Boolean = False
        Public Property UserDefinedLIsChecked As Boolean
            Get
                Return _UserDefinedLIsChecked
            End Get
            Set(ByVal value As Boolean)
                _UserDefinedLIsChecked = value
                OnPropertyChanged(NameOf(UserDefinedLIsChecked))
            End Set
        End Property


        Private _SubgradeLIsChecked As Boolean = False
        Public Property SubgradeLIsChecked As Boolean
            Get
                Return _SubgradeLIsChecked
            End Get
            Set(ByVal value As Boolean)
                _SubgradeLIsChecked = value
                OnPropertyChanged(NameOf(SubgradeLIsChecked))
            End Set
        End Property


        Private _HMAOverlayIsChecked As Boolean = False
        Public Property HMAOverlayIsChecked As Boolean
            Get
                Return _HMAOverlayIsChecked
            End Get
            Set(ByVal value As Boolean)
                _HMAOverlayIsChecked = value
                OnPropertyChanged(NameOf(HMAOverlayIsChecked))
            End Set
        End Property


        Private _PCCOverlayFLIsChecked As Boolean = False
        Public Property PCCOverlayFLIsChecked As Boolean
            Get
                Return _PCCOverlayFLIsChecked
            End Get
            Set(ByVal value As Boolean)
                _PCCOverlayFLIsChecked = value
                OnPropertyChanged(NameOf(PCCOverlayFLIsChecked))
            End Set
        End Property


        Private _PCCOverlayUnLIsChecked As Boolean = False
        Public Property PCCOverlayUnLIsChecked As Boolean
            Get
                Return _PCCOverlayUnLIsChecked
            End Get
            Set(ByVal value As Boolean)
                _PCCOverlayUnLIsChecked = value
                OnPropertyChanged(NameOf(PCCOverlayUnLIsChecked))
            End Set
        End Property


        Private _HMASurfaceIsChecked As Boolean = False
        Public Property HMASurfaceIsChecked As Boolean
            Get
                Return _HMASurfaceIsChecked
            End Get
            Set(ByVal value As Boolean)
                _HMASurfaceIsChecked = value
                OnPropertyChanged(NameOf(HMASurfaceIsChecked))
            End Set
        End Property


        Private _PCCSurfaceLIsChecked As Boolean = False
        Public Property PCCSurfaceLIsChecked As Boolean
            Get
                Return _PCCSurfaceLIsChecked
            End Get
            Set(ByVal value As Boolean)
                _PCCSurfaceLIsChecked = value
                OnPropertyChanged(NameOf(PCCSurfaceLIsChecked))
            End Set
        End Property


        Private _P301LIsChecked As Boolean = False
        Public Property P301LIsChecked As Boolean
            Get
                Return _P301LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P301LIsChecked = value
                OnPropertyChanged(NameOf(P301LIsChecked))
            End Set
        End Property


        Private _P304LIsChecked As Boolean = False
        Public Property P304LIsChecked As Boolean
            Get
                Return _P304LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P304LIsChecked = value
                OnPropertyChanged(NameOf(P304LIsChecked))
            End Set
        End Property


        Private _P306LIsChecked As Boolean = False
        Public Property P306LIsChecked As Boolean
            Get
                Return _P306LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P306LIsChecked = value
                OnPropertyChanged(NameOf(P306LIsChecked))
            End Set
        End Property


        Private _HMAStLIsChecked As Boolean = False
        Public Property HMAStLIsChecked As Boolean
            Get
                Return _HMAStLIsChecked
            End Get
            Set(ByVal value As Boolean)
                _HMAStLIsChecked = value
                OnPropertyChanged(NameOf(HMAStLIsChecked))
            End Set
        End Property


        Private _VariableFlIsChecked As Boolean = False
        Public Property VariableFlIsChecked As Boolean
            Get
                Return _VariableFlIsChecked
            End Get
            Set(ByVal value As Boolean)
                _VariableFlIsChecked = value
                OnPropertyChanged(NameOf(VariableFlIsChecked))
            End Set
        End Property


        Private _VariableRgIsChecked As Boolean = False
        Public Property VariableRgIsChecked As Boolean
            Get
                Return _VariableRgIsChecked
            End Get
            Set(ByVal value As Boolean)
                _VariableRgIsChecked = value
                OnPropertyChanged(NameOf(VariableRgIsChecked))
            End Set
        End Property


        Private _P154LIsChecked As Boolean = False
        Public Property P154LIsChecked As Boolean
            Get
                Return _P154LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P154LIsChecked = value
                OnPropertyChanged(NameOf(P154LIsChecked))
            End Set
        End Property


        Private _P208LIsChecked As Boolean = False
        Public Property P208LIsChecked As Boolean
            Get
                Return _P208LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P208LIsChecked = value
                OnPropertyChanged(NameOf(P208LIsChecked))
            End Set
        End Property


        Private _P209LIsChecked As Boolean = False
        Public Property P209LIsChecked As Boolean
            Get
                Return _P209LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P209LIsChecked = value
                OnPropertyChanged(NameOf(P209LIsChecked))
            End Set
        End Property


        Private _P211LIsChecked As Boolean = False
        Public Property P211LIsChecked As Boolean
            Get
                Return _P211LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P211LIsChecked = value
                OnPropertyChanged(NameOf(P211LIsChecked))
            End Set
        End Property


        Private _P219LIsChecked As Boolean = False
        Public Property P219LIsChecked As Boolean
            Get
                Return _P219LIsChecked
            End Get
            Set(ByVal value As Boolean)
                _P219LIsChecked = value
                OnPropertyChanged(NameOf(P219LIsChecked))
            End Set
        End Property


        Private _materialsSelectionIsHidden As Boolean = True
        Public Property MaterialsSelectionIsHidden As Boolean
            Get
                Return _materialsSelectionIsHidden
            End Get
            Set(ByVal value As Boolean)
                _materialsSelectionIsHidden = value
                OnPropertyChanged(NameOf(MaterialsSelectionIsHidden))
            End Set
        End Property


        Private _AnalysisListSelectedIndex As Integer = -1
        Public Property AnalysisListSelectedIndex As Integer
            Get
                Return _AnalysisListSelectedIndex
            End Get
            Set(ByVal value As Integer)
                _AnalysisListSelectedIndex = value
                OnPropertyChanged(NameOf(AnalysisListSelectedIndex))
            End Set
        End Property


        Private _selectedtree As Integer = 0
        Public Property SelectedTree As Integer
            Get
                Return _selectedtree
            End Get
            Set(value As Integer)
                _selectedtree = value
                OnPropertyChanged(NameOf(SelectedTree))
            End Set
        End Property


        Private _windowState As WindowState = WindowState.Maximized
        Public Property WindowState As WindowState
            Get
                Return _windowState
            End Get
            Set(ByVal value As WindowState)
                _windowState = value
                OnPropertyChanged(NameOf(WindowState))
            End Set
        End Property


        Private _DockingContainer As Telerik.Windows.Controls.RadDocking
        Public Property DockingContainer As Telerik.Windows.Controls.RadDocking
            Get
                Return _DockingContainer
            End Get
            Set(ByVal value As Telerik.Windows.Controls.RadDocking)
                _DockingContainer = value
                OnPropertyChanged(NameOf(DockingContainer))
            End Set
        End Property


        Private _InnerDockingContainer As Telerik.Windows.Controls.RadDocking
        Public Property InnerDockingContainer As Telerik.Windows.Controls.RadDocking
            Get
                Return _InnerDockingContainer
            End Get
            Set(ByVal value As Telerik.Windows.Controls.RadDocking)
                _InnerDockingContainer = value
                OnPropertyChanged(NameOf(InnerDockingContainer))
            End Set
        End Property


        Private _AdvancedOptionsVisibility As Visibility = Visibility.Collapsed
        Public Property AdvancedOptionsVisibility As Visibility
            Get
                Return _AdvancedOptionsVisibility
            End Get
            Set(value As Visibility)
                _AdvancedOptionsVisibility = value
                OnPropertyChanged(NameOf(AdvancedOptionsVisibility))
            End Set
        End Property


        Private _AdvancedOptionsToggleText As String = "Show Advanced Options"
        Public Property AdvancedOptionsToggleText As String
            Get
                Return _AdvancedOptionsToggleText
            End Get
            Set(ByVal value As String)
                _AdvancedOptionsToggleText = value
                OnPropertyChanged(NameOf(AdvancedOptionsToggleText))
            End Set
        End Property


        Public _ACRHeaderB As String = "ACR"
        Public Property ACRHeaderB As String
            Get
                Return _ACRHeaderB
            End Get
            Set(ByVal value As String)
                _ACRHeaderB = value
                OnPropertyChanged(NameOf(ACRHeaderB))
            End Set
        End Property


        Public _ACRHeader As String = "ACR"
        Public Property ACRHeader As String
            Get
                Return _ACRHeader
            End Get
            Set(ByVal value As String)
                _ACRHeader = value
                OnPropertyChanged(NameOf(ACRHeader))
            End Set
        End Property


        Public _ACRHeader1 As String = "ACR"
        Public Property ACRHeader1 As String
            Get
                Return _ACRHeader1
            End Get
            Set(ByVal value As String)
                _ACRHeader1 = value
                OnPropertyChanged(NameOf(ACRHeader1))
            End Set
        End Property


        Public _ACRHeader2 As String = "ACR"
        Public Property ACRHeader2 As String
            Get
                Return _ACRHeader2
            End Get
            Set(ByVal value As String)
                _ACRHeader2 = value
                OnPropertyChanged(NameOf(ACRHeader2))
            End Set
        End Property


        Public _ACRHeader3 As String = "ACR"
        Public Property ACRHeader3 As String
            Get
                Return _ACRHeader3
            End Get
            Set(ByVal value As String)
                _ACRHeader3 = value
                OnPropertyChanged(NameOf(ACRHeader3))
            End Set
        End Property


        Public _ACRHeader4 As String = "ACR"
        Public Property ACRHeader4 As String
            Get
                Return _ACRHeader4
            End Get
            Set(ByVal value As String)
                _ACRHeader4 = value
                OnPropertyChanged(NameOf(ACRHeader4))
            End Set
        End Property


        Public _ACRThickHeader1 As String = "ACR Thick"
        Public Property ACRThickHeader1 As String
            Get
                Return _ACRThickHeader1
            End Get
            Set(ByVal value As String)
                _ACRThickHeader1 = value
                OnPropertyChanged(NameOf(ACRThickHeader1))
            End Set
        End Property


        Public _ACRThickHeader2 As String = "ACR Thick"
        Public Property ACRThickHeader2 As String
            Get
                Return _ACRThickHeader2
            End Get
            Set(ByVal value As String)
                _ACRThickHeader2 = value
                OnPropertyChanged(NameOf(ACRThickHeader2))
            End Set
        End Property


        Public _ACRThickHeader3 As String = "ACR Thick"
        Public Property ACRThickHeader3 As String
            Get
                Return _ACRThickHeader3
            End Get
            Set(ByVal value As String)
                _ACRThickHeader3 = value
                OnPropertyChanged(NameOf(ACRThickHeader3))
            End Set
        End Property


        Public _ACRThickHeader4 As String = "ACR Thick"
        Public Property ACRThickHeader4 As String
            Get
                Return _ACRThickHeader4
            End Get
            Set(ByVal value As String)
                _ACRThickHeader4 = value
                OnPropertyChanged(NameOf(ACRThickHeader4))
            End Set
        End Property


        Public _ACRThickHeader As String = "ACR Thick"
        Public Property ACRThickHeader As String
            Get
                Return _ACRThickHeader
            End Get
            Set(ByVal value As String)
                _ACRThickHeader = value
                OnPropertyChanged(NameOf(ACRThickHeader))
            End Set
        End Property


        Private _RunBatch As Boolean = False
        Public Property RunBatch As Boolean
            Get
                Return _RunBatch
            End Get
            Set(ByVal value As Boolean)
                _RunBatch = value
                OnPropertyChanged(NameOf(RunBatch))
            End Set
        End Property


        Private _RunCompleted As Boolean = True
        Public Property RunCompleted As Boolean
            Get
                Return _RunCompleted
            End Get
            Set(ByVal value As Boolean)
                _RunCompleted = value
                OnPropertyChanged(NameOf(RunCompleted))
            End Set
        End Property


        Private _SlabCompleted As Boolean = True
        Public Property SlabCompleted As Boolean
            Get
                Return _SlabCompleted
            End Get
            Set(ByVal value As Boolean)
                _SlabCompleted = value
                OnPropertyChanged(NameOf(SlabCompleted))
            End Set
        End Property


        Private _DataGridCellValue As String
        Public Property DataGridCellValue() As String
            Get
                Return _DataGridCellValue
            End Get
            Set(ByVal value As String)
                _DataGridCellValue = value
                OnPropertyChanged(NameOf(DataGridCellValue))
            End Set
        End Property


        'wrg
        Private _airplaneXMLFileLocation As String = Nothing
        Public Property airplaneXMLFileLocation As String
            Get
                If String.IsNullOrEmpty(_airplaneXMLFileLocation) Or String.IsNullOrWhiteSpace(_airplaneXMLFileLocation) Then
                    _airplaneXMLFileLocation = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "Defaults" & Path.DirectorySeparatorChar & "Aircraft" & Path.DirectorySeparatorChar & "aircraft.xml"
                End If

                Return _airplaneXMLFileLocation
            End Get
            Set(ByVal value As String)
                _airplaneXMLFileLocation = value
            End Set
        End Property


        Private _DomActionEnabled As Boolean = True
        Public Property DomActionEnabled As Boolean
            Get
                Return _DomActionEnabled
            End Get
            Set(ByVal value As Boolean)
                _DomActionEnabled = value
            End Set
        End Property


        Private _OnLogin_Command As DelegateCommand
        Public ReadOnly Property OnLogin_Command As ICommand
            Get
                If _OnLogin_Command Is Nothing Then
                    _OnLogin_Command = New DelegateCommand(AddressOf OnLogin, AddressOf DomActionCanExecute)
                End If
                Return _OnLogin_Command
            End Get
        End Property


        Private _OnOpenDomAccessTab_Command As DelegateCommand
        Public ReadOnly Property OnOpenDomAccessTab_Command As ICommand
            Get
                If _OnOpenDomAccessTab_Command Is Nothing Then
                    _OnOpenDomAccessTab_Command = New DelegateCommand(AddressOf OnOpenDomAccessTab, AddressOf DomActionCanExecute)
                End If
                Return _OnOpenDomAccessTab_Command
            End Get
        End Property


        Private _OnLogout_Command As DelegateCommand
        Public ReadOnly Property OnLogout_Command As ICommand
            Get
                If _OnLogout_Command Is Nothing Then
                    _OnLogout_Command = New DelegateCommand(AddressOf OnLogout, AddressOf DomActionCanExecute)
                End If
                Return _OnLogout_Command
            End Get
        End Property


        Private _ToggleLogoutVisibility As Visibility = Visibility.Collapsed
        Public Property ToggleLogout_Display As Visibility
            Get
                Return _ToggleLogoutVisibility
            End Get
            Set(value As Visibility)
                _ToggleLogoutVisibility = value
                OnPropertyChanged(NameOf(ToggleLogout_Display))
            End Set
        End Property


        Private _LoginVisibility As Visibility = Visibility.Visible
        Public Property ToggleLogin_Display As Visibility
            Get
                Return _LoginVisibility
            End Get
            Set(value As Visibility)
                _LoginVisibility = value
                OnPropertyChanged(NameOf(ToggleLogin_Display))
            End Set
        End Property


        Private _ProgressBarWindowIsHidden As Boolean = True
        Public Property ProgressBarWindowIsHidden As Boolean
            Get
                Return _ProgressBarWindowIsHidden
            End Get
            Set(ByVal value As Boolean)
                _ProgressBarWindowIsHidden = value
                OnPropertyChanged(NameOf(ProgressBarWindowIsHidden))
            End Set
        End Property


        Private _ComboBoxWindowIsHidden As Boolean = True
        Public Property ComboBoxWindowIsHidden As Boolean
            Get
                Return _ComboBoxWindowIsHidden
            End Get
            Set(ByVal value As Boolean)
                _ComboBoxWindowIsHidden = value
                OnPropertyChanged(NameOf(ComboBoxWindowIsHidden))
            End Set
        End Property


        Private _LoginWindowIsHidden As Boolean = True
        Public Property LoginWindowIsHidden As Boolean
            Get
                Return _LoginWindowIsHidden
            End Get
            Set(ByVal value As Boolean)
                _LoginWindowIsHidden = value
                OnPropertyChanged(NameOf(LoginWindowIsHidden))
            End Set
        End Property


        Private _ErrorMessage As String
        Public Property ErrorMessage As String
            Get
                Return _ErrorMessage
            End Get
            Set(ByVal value As String)
                _ErrorMessage = value
            End Set
        End Property


        Private _CanUpload As Boolean = True
        Public Property CanUpload As Boolean
            Get
                Return _CanUpload
            End Get
            Set(ByVal value As Boolean)
                _CanUpload = value
                OnPropertyChanged(NameOf(CanUpload))
            End Set
        End Property


        Private _CanDownload As Boolean = True
        Public Property CanDownload As Boolean
            Get
                Return _CanDownload
            End Get
            Set(ByVal value As Boolean)
                _CanDownload = value
                OnPropertyChanged(NameOf(CanDownload))
            End Set
        End Property


        Private _LibUpdateAvailable As Boolean = False
        Public Property LibUpdateAvailable As Boolean
            Get
                Return _LibUpdateAvailable
            End Get
            Set(ByVal value As Boolean)
                _LibUpdateAvailable = value
                OnPropertyChanged(NameOf(LibUpdateAvailable))
            End Set
        End Property


        Private _Username As String
        Public Property Username As String
            Get
                Return _Username
            End Get
            Set(ByVal value As String)
                _Username = value
                OnPropertyChanged(NameOf(Username))
            End Set
        End Property


        Private _LibraryVersion As String
        Public Property LibraryVersion As String
            Get
                Return _LibraryVersion
            End Get
            Set(ByVal value As String)
                _LibraryVersion = value
                OnPropertyChanged(NameOf(LibraryVersion))
            End Set
        End Property


        Private _SoftwareVersion As String = MainWindowTitle
        Public Property SoftwareVersion As String
            Get
                Dim titleArgs() As String = _SoftwareVersion.Split(" ")

                If titleArgs.Count > 1 Then
                    _SoftwareVersion = titleArgs(1)
                End If
                Return _SoftwareVersion
            End Get
            Set(ByVal value As String)
                _SoftwareVersion = value
                OnPropertyChanged(NameOf(SoftwareVersion))
            End Set
        End Property


        Private _RemoteLibraryVersion As String
        Public Property RemoteLibraryVersion As String
            Get
                Return _RemoteLibraryVersion
            End Get
            Set(ByVal value As String)
                _RemoteLibraryVersion = value
                OnPropertyChanged(NameOf(RemoteLibraryVersion))
            End Set
        End Property


        Private _RemoteSoftwareVersion As String
        Public Property RemoteSoftwareVersion As String
            Get
                Return _RemoteSoftwareVersion
            End Get
            Set(ByVal value As String)
                _RemoteSoftwareVersion = value
                OnPropertyChanged(NameOf(RemoteSoftwareVersion))
            End Set
        End Property


        Private _SelectedPaAction As String
        Public Property SelectedPaAction As String
            Get
                Return _SelectedPaAction
            End Get
            Set(ByVal value As String)
                _SelectedPaAction = value
            End Set
        End Property


        Private _DownloadAircraftLib_Command As DelegateCommand
        Public ReadOnly Property DownloadAircraftLib_Command As ICommand
            Get
                If _DownloadAircraftLib_Command Is Nothing Then
                    _DownloadAircraftLib_Command = New DelegateCommand(AddressOf DownloadAircraftLib, AddressOf DomActionCanExecute)
                End If
                Return _DownloadAircraftLib_Command
            End Get
        End Property


        Private _LoginButton_Command As DelegateCommand
        Public ReadOnly Property LoginButton_Command As ICommand
            Get
                If _LoginButton_Command Is Nothing Then
                    _LoginButton_Command = New DelegateCommand(AddressOf AttemptLogin, AddressOf DomActionCanExecute)
                End If
                Return _LoginButton_Command
            End Get
        End Property


        Private _ExecuteUpload_Command As DelegateCommand
        Public ReadOnly Property ExecuteUpload_Command As ICommand
            Get
                If _ExecuteUpload_Command Is Nothing Then
                    _ExecuteUpload_Command = New DelegateCommand(AddressOf ExecuteJobUpload, AddressOf DomActionCanExecute)
                End If
                Return _ExecuteUpload_Command
            End Get
        End Property


        Private _ExecutePaAction_Command As DelegateCommand
        Public ReadOnly Property ExecutePaAction_Command As ICommand
            Get
                If _ExecutePaAction_Command Is Nothing Then
                    _ExecutePaAction_Command = New DelegateCommand(AddressOf ExecutePaAction, AddressOf DomActionCanExecute)
                End If
                Return _ExecutePaAction_Command
            End Get
        End Property


        Private _SelectedDatabase As Database
        Public Property SelectedDatabase As Database
            Get
                Return _SelectedDatabase
            End Get
            Set(ByVal value As Database)
                _SelectedDatabase = value
                ChangeSelectedDatabase()
                OnPropertyChanged(NameOf(SelectedDatabase))
            End Set
        End Property


        Private _SelectedNetwork As Network
        Public Property SelectedNetwork As Network
            Get
                Return _SelectedNetwork
            End Get
            Set(ByVal value As Network)
                _SelectedNetwork = value
                ChangeSelectedNetwork()
                OnPropertyChanged(NameOf(SelectedNetwork))
            End Set
        End Property


        Private _SelectedBranch As Branch
        Public Property SelectedBranch As Branch
            Get
                Return _SelectedBranch
            End Get
            Set(ByVal value As Branch)
                _SelectedBranch = value
                ChangeSelectedBranch()
                OnPropertyChanged(NameOf(SelectedBranch))
            End Set
        End Property


        Private _SelectedSection As PaveairSection
        Public Property SelectedSection As PaveairSection
            Get
                Return _SelectedSection
            End Get
            Set(ByVal value As PaveairSection)
                _SelectedSection = value
                OnPropertyChanged(NameOf(SelectedSection))

                If GlobalDOMViewModel.AuthCheck Then
                    ChangeSelectedSection()
                End If

                If _SelectedSection Is Nothing And JobsByPaveAirSection IsNot Nothing Then
                    JobsByPaveAirSection.Clear()
                End If
            End Set
        End Property


        Private _UDAFolderPath As String = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "User Defined Aircraft" & Path.DirectorySeparatorChar
        Public Property UDAFolderPath As String
            Get
                Return _UDAFolderPath
            End Get
            Set(ByVal value As String)
                _UDAFolderPath = value
                My.Settings.UDADirectory = value
                OnPropertyChanged(NameOf(UDAFolderPath))
            End Set
        End Property

#End Region


#Region "Can Execute Fields, Properties and Functions"

        Private Function DomActionCanExecute() As Boolean
            Return DomActionEnabled
        End Function


        Private Function PreparePaveairActions() As Boolean
            Dim passable = True
            If CurrentJob.Sections.Count = 0 Then
                MessageBox.Show("At least one structure should be defined to upload the job to PAVEAIR.")
                passable = False
                Return passable
            End If

            For Each Section As Object In CurrentJob.Sections
                If Section.AnalysisType Is Nothing Then
                    passable = False

                    MessageBox.Show("Pavement Type should be defined for each structure to upload the job to PAVEAIR. ")
                    Return passable
                End If
            Next

            For Each Section As Object In CurrentJob.Sections
                If Section.Airplanes.Count = 0 Then
                    passable = False

                    MessageBox.Show("Each structure in the job needs to have at least one aircraft on the traffic list to upload the job to PAVEAIR.")
                    Return passable
                End If
            Next

            Return passable
        End Function


        Private Sub AttemptLogin()
            Try
                Dim password As String = GlobalDOMViewModel.Password
                Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/GetDBListAsString/{Username}/{password}"

                Dim BaseAddress = New Uri(endpoint)
                Dim client = New HttpClient()
                Dim resp As HttpResponseMessage = client.GetAsync(BaseAddress).Result
                Dim result As String = resp.Content.ReadAsStringAsync().Result
                result = Replace(result, "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">", "")
                result = Replace(result, "</string>", "")
                result = Replace(result, "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/""/>", "")

                If result <> "Not Authenticated!" And result <> "Error" Then
                    GlobalDOMViewModel.AuthCheck = True

                    Dim databases = GetDatabases(FaarFieldFactory, result)
                    AvailableDatabases = New ObservableCollection(Of IDatabase)

                    For Each database In databases
                        AvailableDatabases.Add(database)
                    Next

                    If AvailableDatabases.Count >= 1 Then
                        SelectedDatabase = databases(0)
                    Else
                        CanUpload = False
                    End If

                    ComboBoxWindowIsHidden = False
                    ToggleLogin_Display = Visibility.Collapsed
                    ToggleLogout_Display = Visibility.Visible
                    OnPropertyChanged(NameOf(AvailableDatabases))
                    OnPropertyChanged(NameOf(SelectedDatabase))

                    LoginWindowIsHidden = True
                    ComboBoxWindowIsHidden = False
                Else
                    ErrorMessage = result
                    Username = ""
                    OnPropertyChanged(NameOf(ErrorMessage))
                End If

                OnPropertyChanged(NameOf(Username))
            Catch ex As Exception
                Debug.WriteLine("AttemptLogin has failed: Unable to contact PAVEAIR")
                MessageBox.Show("The login attempt has failed, unable to contact PAVEAIR.")
            End Try
        End Sub


        Private Sub ChangeSelectedDatabase()
            'Ensure the logged in user has at least one database
            If SelectedDatabase Is Nothing Then
                CanUpload = False
                Exit Sub
            End If

            Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/getDbDescendants/{Username}/{GlobalDOMViewModel.Password}/network/{SelectedDatabase}"
            Dim rawData As String = WebServiceQueryWrapper(endpoint)

            'Network list is null do not continue
            If rawData = "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/""/>" Then
                If AvailableNetworks IsNot Nothing Then
                    AvailableNetworks.Clear()
                End If
                SelectedNetwork = Nothing
                CanUpload = False
                Exit Sub
            End If

            Dim networks = GetNetworks(FaarFieldFactory, rawData)
            AvailableNetworks = New ObservableCollection(Of INetwork)

            For Each network As Object In networks
                AvailableNetworks.Add(network)
            Next

            If networks.Count >= 1 Then
                SelectedNetwork = networks(0)
            Else
                SelectedNetwork = Nothing
            End If

            OnPropertyChanged(NameOf(AvailableNetworks))
            OnPropertyChanged(NameOf(SelectedNetwork))
        End Sub


        Private Sub ChangeSelectedNetwork()
            Dim networkId As Integer

            If SelectedNetwork IsNot Nothing Then
                networkId = SelectedNetwork.Id
            Else
                If AvailableBranches IsNot Nothing Then
                    AvailableBranches.Clear()
                End If
                OnPropertyChanged(NameOf(AvailableBranches))
                Exit Sub
            End If

            Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/getDbDescendants/{Username}/{GlobalDOMViewModel.Password}/branch/{networkId}"
            Dim rawData As String = WebServiceQueryWrapper(endpoint)

            'Do not continue if the branch list is null 
            If rawData = "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/""/>" Then
                If AvailableBranches IsNot Nothing Then
                    AvailableBranches.Clear()
                End If
                SelectedBranch = Nothing
                CanUpload = False
                Exit Sub
            End If

            Dim branches = GetBranches(FaarFieldFactory, rawData)
            AvailableBranches = New ObservableCollection(Of IBranch)

            For Each branch As Object In branches
                AvailableBranches.Add(branch)
            Next

            If branches.Count >= 1 Then
                SelectedBranch = branches(0)
            Else
                SelectedBranch = Nothing
            End If

            OnPropertyChanged(NameOf(AvailableBranches))
            OnPropertyChanged(NameOf(SelectedBranch))
        End Sub


        Private Sub ChangeSelectedBranch()
            Dim branchId As Integer
            If SelectedBranch IsNot Nothing Then
                branchId = SelectedBranch.Id
                CanUpload = True
            Else
                CanUpload = False
                If AvailableSections IsNot Nothing Then
                    AvailableSections.Clear()
                End If
                OnPropertyChanged(NameOf(AvailableSections))
                Exit Sub
            End If

            Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/getDbDescendants/{Username}/{GlobalDOMViewModel.Password}/section/{branchId}"
            Dim rawData As String = WebServiceQueryWrapper(endpoint)

            'Do not continue if the section list is null 
            If rawData = "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/""/>" Then
                If AvailableSections IsNot Nothing Then
                    AvailableSections.Clear()
                End If
                SelectedSection = Nothing
                CanUpload = False
                Exit Sub
            End If

            Dim sections = GetSections(FaarFieldFactory, rawData)
            AvailableSections = New ObservableCollection(Of IPaveairSection)

            For Each section As Object In sections
                AvailableSections.Add(section)
            Next

            If sections.Count >= 1 Then
                SelectedSection = sections(0)
            Else
                CanUpload = False
                SelectedSection = Nothing
            End If
            OnPropertyChanged(NameOf(AvailableSections))
            OnPropertyChanged(NameOf(SelectedSection))
        End Sub


        Private Sub ChangeSelectedSection()
            Try
                Dim sectionId As Integer
                If SelectedSection IsNot Nothing Then
                    sectionId = SelectedSection.Id
                    CanDownload = True
                    ShowUpdatedJobList(sectionId)
                Else
                    CanDownload = False
                    OnPropertyChanged(NameOf(AvailableSections))
                    Exit Sub
                End If
            Catch ex As Exception
                Debug.WriteLine(ex.Message)
            End Try
        End Sub


        Private Sub ShowUpdatedJobList(sectionId As Integer)
            Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/getDbDescendants/{Username}/{GlobalDOMViewModel.Password}/job/{sectionId}"
            Dim rawData As String = WebServiceQueryWrapper(endpoint)

            Dim jobs = GetFaarFieldJobs(FaarFieldFactory, rawData)
            JobsByPaveAirSection = New ObservableCollection(Of IJobDownload)

            For Each job As Object In jobs
                JobsByPaveAirSection.Add(job)
            Next

            If jobs.Count >= 1 Then
                SelectedJobDownload = jobs(0)
            Else
                CanDownload = False
            End If
            OnPropertyChanged(NameOf(JobsByPaveAirSection))
            OnPropertyChanged(NameOf(SelectedJobDownload))
        End Sub


        Private Function WebServiceQueryWrapper(endpoint As String) As String
            Dim result As String
            Try
                Dim BaseAddress = New Uri(endpoint)
                Dim client = New HttpClient With {
                    .Timeout = TimeSpan.FromSeconds(25)
                }
                Dim resp As HttpResponseMessage = client.GetAsync(BaseAddress).Result
                result = resp.Content.ReadAsStringAsync().Result
                result = Replace(result, "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">", "")
                result = Replace(result, "</string>", "")

                If result <> "Not Authenticated" Then
                    Return result
                Else
                    OnPropertyChanged(NameOf(ErrorMessage))
                    Return result
                End If

            Catch
                Debug.WriteLine("AttemptLogin failed in DomAccessViewModel.vb: Unable to contact PAVEAIR")
                Return "Error"
            End Try
        End Function


        Public Function GetDatabases(factory As IFaarFieldModelFactory, rawData As String) As List(Of IDatabase)
            Dim databases = New List(Of IDatabase)

            If rawData = "" Then
                Return databases
            End If

            For Each x As String In rawData.Split(New Char() {"|"c})
                databases.Add(factory.CreateDatabase(x))
            Next

            Return databases
        End Function


        Public Function GetNetworks(factory As IFaarFieldModelFactory, rawData As String) As List(Of INetwork)
            Dim networks = New List(Of INetwork)
            Dim counter As Integer = 1

            Dim currentId As Integer
            Dim currentName As String

            For Each x As String In rawData.Split(New Char() {"|"c})
                If counter Mod 2 = 1 And IsNumeric(x) Then
                    currentId = CType(x, Integer)
                ElseIf counter Mod 2 = 0 Then
                    currentName = x
                    networks.Add(factory.CreateNetwork(currentId, currentName))
                End If

                counter += 1
            Next

            Return networks
        End Function


        Public Function GetBranches(factory As IFaarFieldModelFactory, rawData As String) As List(Of IBranch)
            Dim branches = New List(Of IBranch)
            Dim counter As Integer = 1

            Dim currentId As Integer
            Dim currentName As String

            For Each x As String In rawData.Split(New Char() {"|"c})
                If counter Mod 2 = 1 And IsNumeric(x) Then
                    currentId = CType(x, Integer)
                ElseIf counter Mod 2 = 0 Then
                    currentName = x
                    branches.Add(factory.CreateBranch(currentId, currentName))
                End If

                counter += 1
            Next

            Return branches
        End Function


        Public Function GetSections(factory As IFaarFieldModelFactory, rawData As String) As List(Of IPaveairSection)
            Dim sections = New List(Of IPaveairSection)
            Dim counter As Integer = 1

            Dim currentId As Integer
            Dim currentName As String

            For Each x As String In rawData.Split(New Char() {"|"c})
                If counter Mod 2 = 1 And IsNumeric(x) Then
                    currentId = CType(x, Integer)
                ElseIf counter Mod 2 = 0 Then
                    currentName = x
                    sections.Add(factory.CreateSection(currentId, currentName))
                End If

                counter += 1
            Next

            Return sections
        End Function


        Public Function GetFaarFieldJobs(factory As IFaarFieldModelFactory, rawData As String) As List(Of IJobDownload)
            Dim sections = New List(Of IJobDownload)
            Dim counter As Integer = 1

            Dim currentId As Integer
            Dim currentName As String

            For Each x As String In rawData.Split(New Char() {"|"c})
                If counter Mod 2 = 1 And IsNumeric(x) Then
                    currentId = CType(x, Integer)
                ElseIf counter Mod 2 = 0 Then
                    currentName = x
                    sections.Add(factory.CreateJobDownload(currentId, currentName))
                End If

                counter += 1
            Next

            Return sections
        End Function


        Public Function CheckPaveAirValues() As Boolean
            Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/getPaveairValues/{SelectedSection.Id}/{Username}/{GlobalDOMViewModel.Password}"
            Dim rawData As String = WebServiceQueryWrapper(endpoint)

            Dim changedPaveAirVals As List(Of String) = GetChangedPaveAirValues(rawData)

            If changedPaveAirVals.Count > 0 Then
                Dim changeMsg As New StringBuilder("Do you want to update the following values in PaveAir?")
                For Each val As String In changedPaveAirVals
                    changeMsg.Append(vbCrLf & Chr(149) & val)
                Next
                Dim msgBoxResult As MessageBoxResult = MessageBox.Show(changeMsg.ToString, "Update Confirmation", MessageBoxButton.YesNo)
                If msgBoxResult = MessageBoxResult.No Then
                    SetPaveAirCurrent(rawData)
                ElseIf String.IsNullOrEmpty(CurrentJob.JobInformation.Network) Or String.IsNullOrEmpty(CurrentJob.JobInformation.Branch) Or String.IsNullOrEmpty(CurrentJob.JobInformation.Section) Then
                    Return False
                End If
            End If

            Return True
        End Function


        Public Function GetChangedPaveAirValues(rawData As String) As List(Of String)
            Dim counter As Integer = 0
            Dim paveairVals As New Dictionary(Of String, String)
            Dim currentKey As String = ""
            Dim changedValues As New List(Of String)
            For Each value In rawData.Split(New Char() {"|"c})
                If counter Mod 2 = 0 Then
                    currentKey = value
                Else
                    Select Case currentKey
                        Case "Network"
                            If value <> CurrentJob.JobInformation.Network Then
                                changedValues.Add("Network: " & value & " -> " & CurrentJob.JobInformation.Network)
                            End If
                        Case "Branch"
                            If value <> CurrentJob.JobInformation.Branch Then
                                changedValues.Add("Branch: " & value & " -> " & CurrentJob.JobInformation.Branch)
                            End If
                        Case "Section"
                            If value <> CurrentJob.JobInformation.Section Then
                                changedValues.Add("Section: " & value & " -> " & CurrentJob.JobInformation.Section)
                            End If
                        Case "Airport"
                            If value <> CurrentJob.JobInformation.Airport Then
                                changedValues.Add("Airport: " & value & " -> " & CurrentJob.JobInformation.Airport)
                            End If
                        Case "State"
                            If value <> CurrentJob.JobInformation.State Then
                                changedValues.Add("State: " & value & " -> " & CurrentJob.JobInformation.State)
                            End If
                        Case "Country"
                            If value <> CurrentJob.JobInformation.Country Then
                                changedValues.Add("Country: " & value & " -> " & CurrentJob.JobInformation.Country)
                            End If
                    End Select
                End If

                counter += 1
            Next

            Return changedValues
        End Function


        Public Sub SetPaveAirCurrent(rawData As String)
            Dim counter As Integer = 0
            Dim paveairVals As New Dictionary(Of String, String)
            Dim currentKey As String = ""
            For Each value In rawData.Split(New Char() {"|"c})
                If counter Mod 2 = 0 Then
                    currentKey = value
                Else
                    Select Case currentKey
                        Case "Network"
                            CurrentJob.JobInformation.Network = value
                        Case "Branch"
                            CurrentJob.JobInformation.Branch = value
                        Case "Section"
                            CurrentJob.JobInformation.Section = value
                        Case "Airport"
                            CurrentJob.JobInformation.Airport = value
                        Case "State"
                            CurrentJob.JobInformation.State = value
                        Case "Country"
                            CurrentJob.JobInformation.Country = value
                    End Select
                End If

                counter += 1
            Next
        End Sub


        Private Function DataContractSerializeObject(Of T)(ByVal objectToSerialize As T) As String
            Using memStm As MemoryStream = New MemoryStream()
                Dim serializer = New DataContractSerializer(GetType(T))
                serializer.WriteObject(memStm, objectToSerialize)
                memStm.Seek(0, SeekOrigin.Begin)

                Using streamReader = New StreamReader(memStm)
                    Dim result As String = streamReader.ReadToEnd()
                    Return result
                End Using
            End Using
        End Function


        Private Sub ExecutePaAction()
            Dim selection = Replace(SelectedPaAction, "System.Windows.Controls.ComboBoxItem: ", "")
            Select Case selection
                Case "Download Job"
                    ExecuteJobDownload()
                Case "Update Job"
                    ExecuteJobUpdate()
                Case "Delete Job"
                    ExecuteJobDeletion()
            End Select
        End Sub


        Private Sub ExecuteJobUpload()
            Try
                If Not PreparePaveairActions() Then
                    Exit Sub
                End If

                Dim srcXPath As String = "/ns:FaarFieldJob"
                Dim destXPath As String = "/FaarFieldData"

                If Not CheckPaveAirValues() Then
                    MessageBox.Show("Network, Branch, and Section cannot be empty. Job not uploaded.")
                    Exit Sub
                End If

                Dim doc As New XmlDocument()
                Dim root As XmlElement = doc.CreateElement("FaarFieldData")
                Dim sectionID As XmlElement = doc.CreateElement("SectionID")
                sectionID.InnerText = SelectedSection.Id
                root.AppendChild(sectionID)
                doc.AppendChild(root)

                Dim ff2Job As New FaarFieldJob()
                ff2Job = CurrentJob
                Dim jobData As String = DataContractSerializeObject(ff2Job)

                Dim oDOM = New DOM()
                oDOM.XMLDom.ReadXml(New XmlTextReader(New StringReader(doc.DocumentElement.OuterXml)))
                oDOM.WriteXmlToFile()
                oDOM.AddNodeToXPath(jobData, srcXPath, destXPath)
                oDOM.SendPutRequest(Username, GlobalDOMViewModel.Password, SelectedDatabase.Name, "UploadFaarfieldJob")
                ShowUpdatedJobList(SelectedSection.Id)

                If Not CanDownload Then
                    CanDownload = True
                End If

            Catch ex As Exception
                Debug.WriteLine(ex.Message)
            End Try

        End Sub


        Private Sub ExecuteJobUpdate()
            Try
                If SelectedJobDownload Is Nothing Then
                    MessageBox.Show("Please select a job first.")
                    Exit Sub
                End If

                If Not PreparePaveairActions() Then
                    Exit Sub
                End If

                Dim srcXPath As String = "/ns:FaarFieldJob"
                Dim destXPath As String = "/FaarFieldData"

                If Not CheckPaveAirValues() Then
                    MessageBox.Show("Network, Branch, and Section cannot be empty. Job not updated.")
                    Exit Sub
                End If

                Dim doc As New XmlDocument()
                Dim root As XmlElement = doc.CreateElement("FaarFieldData")
                Dim jobID As XmlElement = doc.CreateElement("JobID")
                jobID.InnerText = SelectedJobDownload.Id
                root.AppendChild(jobID)
                doc.AppendChild(root)

                Dim ff2Job As New FaarFieldJob()
                ff2Job = CurrentJob
                Dim jobData As String = DataContractSerializeObject(ff2Job)

                Dim oDOM = New DOM()
                oDOM.XMLDom.ReadXml(New XmlTextReader(New StringReader(doc.DocumentElement.OuterXml)))
                oDOM.WriteXmlToFile()
                oDOM.AddNodeToXPath(jobData, srcXPath, destXPath)
                oDOM.SendPutRequest(Username, GlobalDOMViewModel.Password, SelectedDatabase.Name, "UpdateFaarfieldJob")
                ShowUpdatedJobList(SelectedSection.Id)

                If Not CanDownload Then
                    CanDownload = True
                End If

            Catch ex As Exception
                Debug.WriteLine(ex.Message)
            End Try

        End Sub


        Private corruptFlag As Boolean = False

        ''' <summary>
        ''' Downloads a FAARFIELD job as string data. Opens file dialog and writes the string data as an xml file.
        ''' </summary>
        Private Sub ExecuteJobDownload()
            Dim outputString As String = Nothing

            Try
                If SelectedJobDownload Is Nothing Then
                    MessageBox.Show("Please select a job first.")
                    Exit Sub
                End If

                Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/downloadfaarfieldjob/{Username}/{GlobalDOMViewModel.Password}/{SelectedJobDownload.Id}"
                Dim baseAddress As New Uri(endpoint)
                Dim client As New HttpClient()
                Dim response As HttpResponseMessage = client.GetAsync(baseAddress).Result

                outputString = response.Content.ReadAsStringAsync().Result
                outputString = Replace(outputString, "&lt;", "<")
                outputString = Replace(outputString, "&gt;", ">")
                outputString = Replace(outputString, "&gt;&#xD;", ">")
                outputString = Replace(outputString, "/&gt;&#xD;", "/>")
                outputString = Replace(outputString, "&#xD;", "")
                outputString = Replace(outputString, "--&amp;gt;", "--&gt;")
                outputString = Replace(outputString, "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">", "")
                outputString = Replace(outputString, "</string>", "")

                Dim saveFileDialog1 As New SaveFileDialog()
                saveFileDialog1.Filter = "jobx files (*.jobx)|*.jobx"
                saveFileDialog1.FilterIndex = 2
                saveFileDialog1.RestoreDirectory = True

                Dim downloadPath As String = SpecialDirectories.MyDocuments + Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "Defaults" & Path.DirectorySeparatorChar & "PAVEAIR Downloads" & Path.DirectorySeparatorChar

                If Not File.Exists(downloadPath) Then
                    Dim fInfo As System.IO.FileInfo = New System.IO.FileInfo(downloadPath)
                    fInfo.Directory.Create()
                End If

                Dim fullJobPath As String = downloadPath + CType(SelectedJobDownload.Id, String) + "_" + SelectedJobDownload.Name + ".jobx"
                File.WriteAllText(fullJobPath, outputString)

                LoadJob(fullJobPath)

                If Not corruptFlag Then
                    MessageBox.Show("Job download is complete.")
                End If

            Catch ex As Exception
                Debug.WriteLine("Unable To download job from PAVEAIR" & vbCrLf & outputString)
            End Try

        End Sub


        Private Sub ExecuteJobDeletion()
            Try
                If SelectedJobDownload Is Nothing Then
                    MessageBox.Show("Please select a job first.")
                    Exit Sub
                End If

                Dim msgBoxResult As MessageBoxResult = MessageBox.Show($"Do you want to proceed and delete job {SelectedJobDownload}?", "Delete Confirmation", MessageBoxButton.YesNo)

                If msgBoxResult = MessageBoxResult.Yes Then
                    Dim query As String = $"{GlobalDOMViewModel.WebServiceUrl}/DeleteFaarFieldJob/{Username}/{GlobalDOMViewModel.Password}/{SelectedJobDownload.Id}"
                    Dim results As String = WebServiceQueryWrapper(query)
                    Dim deletedJob As String = SelectedJobDownload.ToString()

                    'Confirm the file has been previously downloaded if so prompt for delete
                    Dim downloadPath As String = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "Defaults" & Path.DirectorySeparatorChar & "PAVEAIR Downloads" & Path.DirectorySeparatorChar
                    Dim filename As String = $"{CType(SelectedJobDownload.Id, String)}_{SelectedJobDownload.Name}.jobx"

                    If FileSystem.FileExists(downloadPath + filename) Then
                        Dim confirmDelete As MessageBoxResult = MessageBox.Show($"Would you like to delete {deletedJob} from local storage?", "Delete Confirmation", MessageBoxButton.YesNo)

                        If confirmDelete = MessageBoxResult.Yes Then
                            FileSystem.DeleteFile(downloadPath + filename)
                        End If
                    End If

                    'Update DOM access tab UI data
                    If SelectedSection IsNot Nothing Then
                        ShowUpdatedJobList(SelectedSection.Id)
                    End If

                    If JobsByPaveAirSection.Count >= 1 Then
                        SelectedJobDownload = JobsByPaveAirSection(0)
                    Else
                        CanDownload = False
                    End If

                    MessageBox.Show("Deletion Complete", "Complete", MessageBoxButton.OK)
                End If
            Catch ex As Exception
                MessageBox.Show(ex.Message)
            End Try
        End Sub


        Public Function GetAircraftLibVersion(xmlPath As String) As String
            Dim status As String = "0.0.0"

            If Not FileSystem.FileExists(xmlPath) Then
                Dim originalPath = My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "Defaults" & Path.DirectorySeparatorChar & "Aircraft" & Path.DirectorySeparatorChar & "aircraft.xml"
                Dim data = FileSystem.ReadAllText(originalPath)
                Dim fInfo As System.IO.FileInfo = New System.IO.FileInfo(airplaneXMLFileLocation)
                fInfo.Directory.Create()
                File.WriteAllText(airplaneXMLFileLocation, data)
            End If

            Dim doc As New XmlDocument()
            doc.Load(xmlPath)
            Dim root As XmlElement = doc.DocumentElement

            If root.Attributes("LibraryVersion") IsNot Nothing Then
                status = root.Attributes("LibraryVersion").Value
            End If

            Return status
        End Function


        Private Sub DownloadAircraftLib()
            Try
                Dim msgBoxResult As MessageBoxResult
                Dim downloadFlag As Boolean

                If Not FileSystem.FileExists(airplaneXMLFileLocation) Then
                    Dim fInfo As System.IO.FileInfo = New System.IO.FileInfo(airplaneXMLFileLocation)
                    fInfo.Directory.Create()
                    downloadFlag = True
                Else
                    msgBoxResult = MessageBox.Show("Downloading the latest FAA aircraft library will overwrite the library located in the file path below. Do you want to proceed?" & vbCrLf & vbCrLf & $"Current library path: {airplaneXMLFileLocation}", "Delete Confirmation", MessageBoxButton.YesNo)
                    If msgBoxResult = MessageBoxResult.Yes Then
                        downloadFlag = True
                    End If
                End If

                'Confirm internet connectivity before downloading
                If downloadFlag And My.Computer.Network.IsAvailable Then
                    Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/DownloadNewAircraftLibrary"
                    Dim newAircraftLib As String = WebServiceQueryWrapper(endpoint)
                    newAircraftLib = Replace(newAircraftLib, "&lt;", "<")
                    newAircraftLib = Replace(newAircraftLib, "&gt;", ">")
                    newAircraftLib = Replace(newAircraftLib, "&gt;&#xD;", ">")
                    newAircraftLib = Replace(newAircraftLib, "/&gt;&#xD;", "/>")
                    newAircraftLib = Replace(newAircraftLib, "&#xD;", "")
                    newAircraftLib = Replace(newAircraftLib, "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">", "")
                    newAircraftLib = Replace(newAircraftLib, "</string>", "")
                    File.WriteAllText(airplaneXMLFileLocation, newAircraftLib)

                    LibraryVersion = GetAircraftLibVersion(airplaneXMLFileLocation)
                    LibUpdateAvailable = False

                    MessageBox.Show($"The current aircraft library has been updated to version {LibraryVersion}.")

                    LoadAircraftLibrary()

                End If
            Catch ex As Exception
                MessageBox.Show("Sorry, we are unable to download latest FAARFIELD aircraft libray. Error: " & vbCrLf & ex.Message)
            End Try
        End Sub


        Private Sub OnOpenDomAccessTab()
            If Not GlobalDOMViewModel.AuthCheck Then
                LoginWindowIsHidden = False
            End If
        End Sub


        Private Sub OnLogin(obj As Object)
            LoginWindowIsHidden = False

            Dim gVM = New ViewModels.GlobalDOMViewModel()
            Dim pw = New NetworkCredential("", CType(obj.SecurePassword, System.Security.SecureString)).Password
            Dim password = gVM.Encrypt(pw)
            ViewModels.GlobalDOMViewModel.Password = password

            AttemptLogin()

        End Sub


        Private Sub OnLogout()
            Username = Nothing
            ErrorMessage = Nothing
            GlobalDOMViewModel.Password = Nothing
            GlobalDOMViewModel.AuthCheck = False
            GlobalDOMViewModel.Username = Nothing

            ToggleLogout_Display = Visibility.Collapsed
            ToggleLogin_Display = Visibility.Visible

            If AvailableDatabases IsNot Nothing Then
                AvailableDatabases.Clear()
            End If
            If AvailableNetworks IsNot Nothing Then
                AvailableNetworks.Clear()
            End If
            If AvailableBranches IsNot Nothing Then
                AvailableBranches.Clear()
            End If
            If AvailableSections IsNot Nothing Then
                AvailableSections.Clear()
            End If
            If JobsByPaveAirSection IsNot Nothing Then
                JobsByPaveAirSection.Clear()
            End If

            ComboBoxWindowIsHidden = True
            OnPropertyChanged(NameOf(AvailableDatabases))
            OnPropertyChanged(NameOf(AvailableNetworks))
            OnPropertyChanged(NameOf(AvailableBranches))
            OnPropertyChanged(NameOf(AvailableSections))
            OnPropertyChanged(NameOf(Username))
            OnPropertyChanged(NameOf(ErrorMessage))
        End Sub


        ''' <summary>
        ''' Populates RemoteLibraryVersion and RemoteSoftwareVersion from server
        ''' Compares local versions to remote versions for available update.
        ''' </summary>
        Private Sub CheckForUpdates()

            If My.Computer.Network.IsAvailable Then
                Dim endpoint As String = $"{GlobalDOMViewModel.WebServiceUrl}/GetNewAircraftLibVersion"
                Dim versions As String = WebServiceQueryWrapper(endpoint)
                If versions.Split("\").Count = 2 Then
                    RemoteLibraryVersion = versions.Split("\")(0)
                    RemoteSoftwareVersion = versions.Split("\")(1)
                End If
            End If

            LibraryVersion = GetAircraftLibVersion(airplaneXMLFileLocation)

            ' Lettered versions have error - remove letter before compare
            'Dim remoteSoftware = New Version(RemoteSoftwareVersion)
            'Dim currentSoftware = New Version(SoftwareVersion)

            'Check if web service returned result
            If RemoteLibraryVersion IsNot Nothing Then
                Dim remoteLibrary = New Version(RemoteLibraryVersion)
                Dim currentLibrary = New Version(LibraryVersion)
                Dim result = remoteLibrary.CompareTo(currentLibrary)

                If result > 0 Then
                    Debug.WriteLine("Updated aircraft library is available.")
                    LibUpdateAvailable = True
                ElseIf result < 0 Then
                    Debug.WriteLine("Current aircraft library is newer than available version.")
                Else
                    Debug.WriteLine("Current aircraft library is newest version available.")
                End If

                If LibUpdateAvailable Then
                    Dim userResponse As Integer
                    userResponse = MsgBox("A newer version of the aircraft library is available, would you like to download the newer version?", MsgBoxStyle.YesNo Or MsgBoxStyle.Question,
              "Aircraft Library - Version Update Available")

                    If userResponse = 6 Then
                        DownloadAircraftLib()
                    ElseIf userResponse = 7 Then
                        MsgBox("The library can be updated later using the Download Aircraft Library button found in the User Information tab.", MsgBoxStyle.Information,
              "Aircraft Library - Version Update Available")
                    End If
                End If
            End If

        End Sub


#End Region


#Region "Can Execute Fields, Properties and Functions"

        Private _jobRequiredButtonEnabled As Boolean = False
        Public Property JobRequiredButtonEnabled As Boolean
            Get
                Return _jobRequiredButtonEnabled
            End Get
            Set(ByVal value As Boolean)
                _jobRequiredButtonEnabled = value
                OnPropertyChanged(NameOf(JobRequiredButtonEnabled))

                If _OnSectionNew_Command IsNot Nothing Then
                    _OnSectionNew_Command.CanExecute(value)
                End If
                If _OnCloseJob_Command IsNot Nothing Then
                    _OnCloseJob_Command.CanExecute(value)
                End If
                If _OnCreateUserDefined_Command IsNot Nothing Then
                    _OnCreateUserDefined_Command.CanExecute(value)
                End If
                If _OnEditUserDefined_Command IsNot Nothing Then
                    _OnEditUserDefined_Command.CanExecute(value)
                End If

            End Set
        End Property


        Private _sectionRequiredButtonEnabled As Boolean = False
        Public Property SectionRequiredButtonEnabled As Boolean
            Get
                Return _sectionRequiredButtonEnabled
            End Get
            Set(ByVal value As Boolean)
                _sectionRequiredButtonEnabled = value
                OnPropertyChanged(NameOf(SectionRequiredButtonEnabled))

                If _OnSaveJob_Command IsNot Nothing Then
                    _OnSaveJob_Command.CanExecute(value)
                End If
                If _OnSaveAsJob_Command IsNot Nothing Then
                    _OnSaveAsJob_Command.CanExecute(value)
                End If
                If _OnSaveAllJob_Command IsNot Nothing Then
                    _OnSaveAllJob_Command.CanExecute(value)
                End If
                If _OnSelectAllSections_Command IsNot Nothing Then
                    _OnSelectAllSections_Command.CanExecute(value)
                End If
                If _OnDeSelectAllSections_Command IsNot Nothing Then
                    _OnDeSelectAllSections_Command.CanExecute(value)
                End If
                If _OnChangeImageGraphics_Command IsNot Nothing Then
                    _OnChangeImageGraphics_Command.CanExecute(value)
                End If

            End Set
        End Property


        Public Sub CheckRunAvailable()
            If CurrentAirplanes Is Nothing Or CurrentAirplanes.Count < 1 Or AnalysisType Is Nothing Then
            Else
                RunAnalysisIsEnabled = True
            End If
        End Sub


        Private Function RunButtonCanExecute() As Boolean
            Return RunAnalysisIsEnabled
        End Function


        Private Function JobRequiredButtonCanExecute() As Boolean
            Return JobRequiredButtonEnabled
        End Function


        Private Function SectionRequiredButtonCanExecute() As Boolean
            Return SectionRequiredButtonEnabled
        End Function

        Private Function AddLayerAboveCanExecute() As Boolean
            Return AddLayerAboveIsEnabled
        End Function

        Private Function AddLayerBelowCanExecute() As Boolean
            Return AddLayerBelowIsEnabled
        End Function

        Private Shared Function InitializedEnabled() As Boolean
            Return True
        End Function


        'Property to Enable/Disable Life/Compaction in drop down list
        Private _CompactionLifeIsEnabled As Boolean = True
        Public Property CompactionLifeIsEnabled As Boolean
            Get
                Return _CompactionLifeIsEnabled
            End Get
            Set(ByVal value As Boolean)
                _CompactionLifeIsEnabled = value
                OnPropertyChanged(NameOf(CompactionLifeIsEnabled))
            End Set
        End Property

#End Region


#Region "Toolbar Commands"

        Private _OnNewJob_Command As DelegateCommand
        Public ReadOnly Property OnNewJob_Command As ICommand
            Get
                If _OnNewJob_Command Is Nothing Then
                    _OnNewJob_Command = New DelegateCommand(AddressOf OnNewJob, AddressOf NewJobEnabled)
                End If
                Return _OnNewJob_Command
            End Get
        End Property


        Private _OnOpenJob_Command As DelegateCommand
        Public ReadOnly Property OnOpenJob_Command As ICommand
            Get
                If _OnOpenJob_Command Is Nothing Then
                    _OnOpenJob_Command = New DelegateCommand(AddressOf OnOpenJob, AddressOf OpenJobEnabled)
                End If
                Return _OnOpenJob_Command
            End Get
        End Property


        Private _OnSectionNew_Command As DelegateCommand
        Public ReadOnly Property OnSectionNew_Command As ICommand
            Get
                If _OnSectionNew_Command Is Nothing Then
                    _OnSectionNew_Command = New DelegateCommand(AddressOf OnSectionNew, AddressOf SectionNewEnabled)
                End If
                Return _OnSectionNew_Command
            End Get
        End Property


        Private _OnSaveJob_Command As DelegateCommand
        Public ReadOnly Property OnSaveJob_Command As ICommand
            Get
                If _OnSaveJob_Command Is Nothing Then
                    _OnSaveJob_Command = New DelegateCommand(AddressOf OnSaveJob, AddressOf SectionRequiredButtonCanExecute)
                End If
                Return _OnSaveJob_Command
            End Get
        End Property


        Private _OnSaveAsJob_Command As DelegateCommand
        Public ReadOnly Property OnSaveAsJob_Command As ICommand
            Get
                If _OnSaveAsJob_Command Is Nothing Then
                    _OnSaveAsJob_Command = New DelegateCommand(AddressOf OnSaveAsJob, AddressOf SectionRequiredButtonCanExecute)
                End If
                Return _OnSaveAsJob_Command
            End Get
        End Property


        Private _OnSaveAllJob_Command As DelegateCommand
        Public ReadOnly Property OnSaveAllJob_Command As ICommand
            Get
                If _OnSaveAllJob_Command Is Nothing Then
                    _OnSaveAllJob_Command = New DelegateCommand(AddressOf OnSaveAllJob, AddressOf SectionRequiredButtonCanExecute)
                End If
                Return _OnSaveAllJob_Command
            End Get
        End Property


        Private _OnCloseJob_Command As DelegateCommand
        Public ReadOnly Property OnCloseJob_Command As ICommand
            Get
                If _OnCloseJob_Command Is Nothing Then
                    _OnCloseJob_Command = New DelegateCommand(AddressOf OnCloseJob, AddressOf JobRequiredButtonCanExecute)
                End If
                Return _OnCloseJob_Command
            End Get
        End Property


        Private _OnCreateUserDefined_Command As DelegateCommand
        Public ReadOnly Property OnCreateUserDefined_Command As ICommand
            Get
                If _OnCreateUserDefined_Command Is Nothing Then
                    _OnCreateUserDefined_Command = New DelegateCommand(AddressOf OnCreateUserDefined, AddressOf JobRequiredButtonCanExecute)
                End If
                Return _OnCreateUserDefined_Command
            End Get
        End Property


        Private _OnEditUserDefined_Command As DelegateCommand
        Public ReadOnly Property OnEditUserDefined_Command As ICommand
            Get
                If _OnEditUserDefined_Command Is Nothing Then
                    _OnEditUserDefined_Command = New DelegateCommand(AddressOf OnEditUserDefined, AddressOf JobRequiredButtonCanExecute)
                End If
                Return _OnEditUserDefined_Command
            End Get
        End Property


        Private _ExitApp_Command As DelegateCommand
        Public ReadOnly Property ExitApp_Command As ICommand
            Get
                If _ExitApp_Command Is Nothing Then
                    _ExitApp_Command = New DelegateCommand(AddressOf ExitApp, AddressOf InitializedEnabled)
                End If

                Return _ExitApp_Command
            End Get
        End Property


        Private _OnReset_Command As DelegateCommand
        Public ReadOnly Property OnReset_Command As ICommand
            Get
                If _OnReset_Command Is Nothing Then
                    _OnReset_Command = New DelegateCommand(AddressOf OnReset, AddressOf InitializedEnabled)
                End If
                Return _OnReset_Command
            End Get
        End Property


        Private _OnHelp_Command As DelegateCommand
        Public ReadOnly Property OnHelp_Command As ICommand
            Get
                If _OnHelp_Command Is Nothing Then
                    _OnHelp_Command = New DelegateCommand(AddressOf OnHelp, AddressOf InitializedEnabled)
                End If
                Return _OnHelp_Command
            End Get
        End Property


        Private _OnAbout_Command As DelegateCommand
        Public ReadOnly Property OnAbout_Command As ICommand
            Get
                If _OnAbout_Command Is Nothing Then
                    _OnAbout_Command = New DelegateCommand(AddressOf OnAbout, Function() True)
                End If
                Return _OnAbout_Command
            End Get
        End Property


        Private _OnSelectAllSections_Command As DelegateCommand
        Public ReadOnly Property OnSelectAllSections_Command As ICommand
            Get
                If _OnSelectAllSections_Command Is Nothing Then
                    _OnSelectAllSections_Command = New DelegateCommand(AddressOf OnSelectAllSections, AddressOf SectionRequiredButtonCanExecute)
                End If
                Return _OnSelectAllSections_Command
            End Get
        End Property


        Private _OnDeSelectAllSections_Command As DelegateCommand
        Public ReadOnly Property OnDeSelectAllSections_Command As ICommand
            Get
                If _OnDeSelectAllSections_Command Is Nothing Then
                    _OnDeSelectAllSections_Command = New DelegateCommand(AddressOf OnDeSelectAllSections, AddressOf SectionRequiredButtonCanExecute)
                End If
                Return _OnDeSelectAllSections_Command
            End Get
        End Property


        Private _OnChangeImageGraphics_Command As DelegateCommand
        Public ReadOnly Property OnChangeImageGraphics_Command As ICommand
            Get
                If _OnChangeImageGraphics_Command Is Nothing Then
                    _OnChangeImageGraphics_Command = New DelegateCommand(AddressOf OnChangeImageGraphics, AddressOf SectionRequiredButtonCanExecute)
                End If
                Return _OnChangeImageGraphics_Command
            End Get
        End Property


#End Region


#Region "Commands From UI"

        Private _UIElement_OnPreviewMouseWheel_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property UIElement_OnPreviewMouseWheel_Command As ICommand
            Get
                If _UIElement_OnPreviewMouseWheel_Command Is Nothing Then
                    _UIElement_OnPreviewMouseWheel_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf UIElement_OnPreviewMouseWheel, AddressOf InitializedEnabled)
                End If
                Return _UIElement_OnPreviewMouseWheel_Command
            End Get
        End Property


        Private _OnPreviewMouseRightButtonDown_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property OnPreviewMouseRightButtonDown_Command As ICommand
            Get
                If _OnPreviewMouseRightButtonDown_Command Is Nothing Then
                    _OnPreviewMouseRightButtonDown_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf OnPreviewMouseRightButtonDown, AddressOf InitializedEnabled)
                End If
                Return _OnPreviewMouseRightButtonDown_Command
            End Get
        End Property


        Private _OnGearMouseMove_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property OnGearMouseMove_Command As ICommand
            Get
                If _OnGearMouseMove_Command Is Nothing Then
                    _OnGearMouseMove_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf OnGearMouseMove, AddressOf InitializedEnabled)
                End If
                Return _OnGearMouseMove_Command
            End Get
        End Property


        Private _RunComboBox_Changed_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property RunComboBox_Changed_Command As ICommand
            Get
                If _RunComboBox_Changed_Command Is Nothing Then
                    _RunComboBox_Changed_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf RunComboBox_Changed, AddressOf InitializedEnabled)
                End If
                Return _RunComboBox_Changed_Command
            End Get
        End Property


        Private _UnitsComboBox_Changed_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property UnitsComboBox_Changed_Command As ICommand
            Get
                If _UnitsComboBox_Changed_Command Is Nothing Then
                    _UnitsComboBox_Changed_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf UnitsCombobox_Changed, AddressOf InitializedEnabled)
                End If
                Return _UnitsComboBox_Changed_Command
            End Get
        End Property


        Private _Slab_Changed_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property Slab_Changed_Command As ICommand
            Get
                If _Slab_Changed_Command Is Nothing Then
                    _Slab_Changed_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf Slab_Changed, AddressOf InitializedEnabled)
                End If
                Return _Slab_Changed_Command
            End Get
        End Property


        Private _MenuItemCopy_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property MenuItemCopy_Command As ICommand
            Get
                If _MenuItemCopy_Command Is Nothing Then
                    _MenuItemCopy_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf MenuItemCopy_Click, AddressOf InitializedEnabled)
                End If
                Return _MenuItemCopy_Command
            End Get
        End Property


        Private _MenuItemDelete_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property MenuItemDelete_Command As ICommand
            Get
                If _MenuItemDelete_Command Is Nothing Then
                    _MenuItemDelete_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf MenuItemDelete_Click, AddressOf InitializedEnabled)
                End If
                Return _MenuItemDelete_Command
            End Get
        End Property


        Private _MenuItemPaste_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property MenuItemPaste_Command As ICommand
            Get
                If _MenuItemPaste_Command Is Nothing Then
                    _MenuItemPaste_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf MenuItemPaste_Click, AddressOf InitializedEnabled)
                End If
                Return _MenuItemPaste_Command
            End Get
        End Property


        Private _RunButton_Command As DelegateCommand
        Public ReadOnly Property RunButton_Command As ICommand
            Get
                If _RunButton_Command Is Nothing Then
                    _RunButton_Command = New DelegateCommand(AddressOf RunButton_Click, AddressOf RunButtonCanExecute)
                End If
                Return _RunButton_Command
            End Get
        End Property


        Private _AddToBatch_Command As DelegateCommand
        Public ReadOnly Property AddToBatch_Command As ICommand
            Get
                If _AddToBatch_Command Is Nothing Then
                    _AddToBatch_Command = New DelegateCommand(AddressOf AddToBatch_Click, AddressOf RunButtonCanExecute)
                End If
                Return _AddToBatch_Command
            End Get
        End Property


        Private Shared _Dg1_LostFocus_Command As DelegateCommand(Of Object)
        Public ReadOnly Property Dg1_LostFocus_Command As ICommand
            Get
                If _Dg1_LostFocus_Command Is Nothing Then
                    _Dg1_LostFocus_Command = New DelegateCommand(Of Object)(AddressOf Dg1_LostFocus, AddressOf InitializedEnabled)
                End If
                Return _Dg1_LostFocus_Command
            End Get
        End Property


        Private Shared _Dg1_CellChanged_Command As DelegateCommand(Of Object)
        Public ReadOnly Property Dg1_CellChanged_Command As ICommand
            Get
                If _Dg1_CellChanged_Command Is Nothing Then
                    _Dg1_CellChanged_Command = New DelegateCommand(Of Object)(AddressOf Dg1_CellChanged, AddressOf InitializedEnabled)
                End If
                Return _Dg1_CellChanged_Command
            End Get
        End Property


        Private _Dg1_Keyup_Command As DelegateCommand(Of Object)
        Public ReadOnly Property Dg1_Keyup_Command As ICommand
            Get
                If _Dg1_Keyup_Command Is Nothing Then
                    _Dg1_Keyup_Command = New DelegateCommand(Of Object)(AddressOf Dg1_Keyup, AddressOf InitializedEnabled)
                End If
                Return _Dg1_Keyup_Command
            End Get
        End Property


        Private Shared _Dg2_LostFocus_Command As DelegateCommand(Of Object)
        Public ReadOnly Property Dg2_LostFocus_Command As ICommand
            Get
                If _Dg2_LostFocus_Command Is Nothing Then
                    _Dg2_LostFocus_Command = New DelegateCommand(Of Object)(AddressOf Dg2_LostFocus, AddressOf InitializedEnabled)
                End If
                Return _Dg2_LostFocus_Command
            End Get
        End Property


        Private Shared _Dg2_CellChanged_Command As DelegateCommand(Of Object)
        Public ReadOnly Property Dg2_CellChanged_Command As ICommand
            Get
                If _Dg2_CellChanged_Command Is Nothing Then
                    _Dg2_CellChanged_Command = New DelegateCommand(Of Object)(AddressOf Dg2_CellChanged, AddressOf InitializedEnabled)
                End If
                Return _Dg2_CellChanged_Command
            End Get
        End Property


        Private _Dg2_Keyup_Command As DelegateCommand(Of Object)
        Public ReadOnly Property Dg2_Keyup_Command As ICommand
            Get
                If _Dg2_Keyup_Command Is Nothing Then
                    _Dg2_Keyup_Command = New DelegateCommand(Of Object)(AddressOf Dg2_Keyup, AddressOf InitializedEnabled)
                End If
                Return _Dg2_Keyup_Command
            End Get
        End Property


        Private Shared _DesignLife_LostFocus_Command As DelegateCommand(Of Object)
        Public ReadOnly Property DesignLife_LostFocus_Command As ICommand
            Get
                If _DesignLife_LostFocus_Command Is Nothing Then
                    _DesignLife_LostFocus_Command = New DelegateCommand(Of Object)(AddressOf DesignLife_LostFocus, AddressOf InitializedEnabled)
                End If
                Return _DesignLife_LostFocus_Command
            End Get
        End Property


        Private _Window_Closing_Command As DelegateCommand
        Public ReadOnly Property Window_Closing_Command As ICommand
            Get
                If _Window_Closing_Command Is Nothing Then
                    _Window_Closing_Command = New DelegateCommand(AddressOf Window_Closing, AddressOf InitializedEnabled)
                End If
                Return _Window_Closing_Command
            End Get
        End Property


        Private _Window_Loaded_Command As DelegateCommand
        Public ReadOnly Property Window_Loaded_Command As ICommand
            Get
                If _Window_Loaded_Command Is Nothing Then
                    _Window_Loaded_Command = New DelegateCommand(AddressOf Window_Loaded, AddressOf InitializedEnabled)
                End If
                Return _Window_Loaded_Command
            End Get
        End Property


        Private _Structure_Loaded_Command As DelegateCommand
        Public ReadOnly Property Structure_Loaded_Command As ICommand
            Get
                If _Structure_Loaded_Command Is Nothing Then
                    _Structure_Loaded_Command = New DelegateCommand(AddressOf Structure_Loaded, AddressOf InitializedEnabled)
                End If
                Return _Structure_Loaded_Command
            End Get
        End Property


        Private _OnSetDefault_Command As DelegateCommand
        Public ReadOnly Property OnSetDefault_Command As ICommand
            Get
                If _OnSetDefault_Command Is Nothing Then
                    _OnSetDefault_Command = New DelegateCommand(AddressOf OnSetDefault, AddressOf InitializedEnabled)
                End If
                Return _OnSetDefault_Command
            End Get
        End Property


        Private _OnResetDefault_Command As DelegateCommand
        Public ReadOnly Property OnResetDefault_Command As ICommand
            Get
                If _OnResetDefault_Command Is Nothing Then
                    _OnResetDefault_Command = New DelegateCommand(AddressOf OnResetDefault, AddressOf InitializedEnabled)
                End If
                Return _OnResetDefault_Command
            End Get
        End Property


        ' RDEC Mix Properties commands and state
        Private _RdecInfoVisible As Visibility = Visibility.Collapsed
        Public Property RdecInfoVisible As Visibility
            Get
                Return _RdecInfoVisible
            End Get
            Set(value As Visibility)
                _RdecInfoVisible = value
                OnPropertyChanged(NameOf(RdecInfoVisible))
            End Set
        End Property

        Private _ToggleRdecInfo_Command As DelegateCommand
        Public ReadOnly Property ToggleRdecInfo_Command As ICommand
            Get
                If _ToggleRdecInfo_Command Is Nothing Then
                    _ToggleRdecInfo_Command = New DelegateCommand(AddressOf ToggleRdecInfo, AddressOf InitializedEnabled)
                End If
                Return _ToggleRdecInfo_Command
            End Get
        End Property

        Private Sub ToggleRdecInfo()
            If RdecInfoVisible = Visibility.Collapsed Then
                RdecInfoVisible = Visibility.Visible
            Else
                RdecInfoVisible = Visibility.Collapsed
            End If
        End Sub

        Private _ResetRdecDefaults_Command As DelegateCommand
        Public ReadOnly Property ResetRdecDefaults_Command As ICommand
            Get
                If _ResetRdecDefaults_Command Is Nothing Then
                    _ResetRdecDefaults_Command = New DelegateCommand(AddressOf ResetRdecDefaults, AddressOf InitializedEnabled)
                End If
                Return _ResetRdecDefaults_Command
            End Get
        End Property

        Private Sub ResetRdecDefaults()
            If CurrentSectionView IsNot Nothing AndAlso CurrentSectionView.Section IsNot Nothing Then
                CurrentSectionView.Section.RdecFlexuralMod = 600000
                CurrentSectionView.Section.RdecAirVoids = 3.5
                CurrentSectionView.Section.RdecAsphaltContentByVol = 12
                CurrentSectionView.Section.RdecPNMS = 95
                CurrentSectionView.Section.RdecPPCS = 58
                CurrentSectionView.Section.RdecP200 = 4.5
                OnPropertyChanged(NameOf(CurrentSectionView))
            End If
        End Sub


        Private _AddLayerBelow_Command As DelegateCommand
        Public ReadOnly Property AddLayerBelow_Command As ICommand
            Get
                If _AddLayerBelow_Command Is Nothing Then
                    _AddLayerBelow_Command = New DelegateCommand(AddressOf AddLayerBelow_Click, AddressOf AddLayerBelowCanExecute)
                End If
                Return _AddLayerBelow_Command
            End Get
        End Property


        Private _AddLayerAbove_Command As DelegateCommand
        Public ReadOnly Property AddLayerAbove_Command As ICommand
            Get
                If _AddLayerAbove_Command Is Nothing Then
                    _AddLayerAbove_Command = New DelegateCommand(AddressOf AddLayerAbove_Click, AddressOf AddLayerAboveCanExecute)
                End If
                Return _AddLayerAbove_Command
            End Get
        End Property


        Private _Replace_Command As DelegateCommand
        Public ReadOnly Property Replace_Command As ICommand
            Get
                If _Replace_Command Is Nothing Then
                    _Replace_Command = New DelegateCommand(AddressOf Replace_Click, AddressOf InitializedEnabled)
                End If
                Return _Replace_Command
            End Get
        End Property


        Private _DeleteLayer_Command As DelegateCommand
        Public ReadOnly Property DeleteLayer_Command As ICommand
            Get
                If _DeleteLayer_Command Is Nothing Then
                    _DeleteLayer_Command = New DelegateCommand(AddressOf DeleteLayer_Click, AddressOf InitializedEnabled)
                End If
                Return _DeleteLayer_Command
            End Get
        End Property


        Private _CancelMaterial_Command As DelegateCommand
        Public ReadOnly Property CancelMaterial_Command As ICommand
            Get
                If _CancelMaterial_Command Is Nothing Then
                    _CancelMaterial_Command = New DelegateCommand(AddressOf CancelMaterial_Click, AddressOf InitializedEnabled)
                End If
                Return _CancelMaterial_Command
            End Get
        End Property


        Private _Layer_Button_Command As DelegateCommand
        Public ReadOnly Property Layer_Button_Command As ICommand
            Get
                If _Layer_Button_Command Is Nothing Then
                    _Layer_Button_Command = New DelegateCommand(AddressOf Layer_Command, AddressOf InitializedEnabled)
                End If
                Return _Layer_Button_Command
            End Get
        End Property


        Private _ListBox_MouseDoubleClick_Command As DelegateCommand
        Public ReadOnly Property ListBox_MouseDoubleClick_Command As ICommand
            Get
                If _ListBox_MouseDoubleClick_Command Is Nothing Then
                    _ListBox_MouseDoubleClick_Command = New DelegateCommand(AddressOf ListBox_MouseDoubleClick_Command_Sub, AddressOf InitializedEnabled)
                End If
                Return _ListBox_MouseDoubleClick_Command
            End Get
        End Property


        Private _AdvancedOptionsToggle_Command As DelegateCommand(Of RoutedEventArgs)
        Public ReadOnly Property AdvancedOptionsToggle_Command As ICommand
            Get
                If _AdvancedOptionsToggle_Command Is Nothing Then
                    _AdvancedOptionsToggle_Command = New DelegateCommand(Of RoutedEventArgs)(AddressOf AdvancedOptionsToggle, AddressOf InitializedEnabled)
                End If
                Return _AdvancedOptionsToggle_Command
            End Get
        End Property


        Private _DataGridBeginEdit_Command As DelegateCommand(Of DataGridBeginningEditEventArgs)
        Public ReadOnly Property DataGridBeginEdit_Command As ICommand
            Get
                If _DataGridBeginEdit_Command Is Nothing Then
                    _DataGridBeginEdit_Command = New DelegateCommand(Of DataGridBeginningEditEventArgs)(AddressOf DataGridBeginEdit, AddressOf InitializedEnabled)
                End If
                Return _DataGridBeginEdit_Command
            End Get
        End Property


        Private _DataGridEndEdit_Command As DelegateCommand(Of DataGridCellEditEndingEventArgs)
        Public ReadOnly Property DataGridEndEdit_Command As ICommand
            Get
                If _DataGridEndEdit_Command Is Nothing Then
                    _DataGridEndEdit_Command = New DelegateCommand(Of DataGridCellEditEndingEventArgs)(AddressOf DataGridEndEdit, AddressOf InitializedEnabled)
                End If
                Return _DataGridEndEdit_Command
            End Get
        End Property


        Private _AirplaneaGridEndEdit_Command As DelegateCommand(Of DataGridCellEditEndingEventArgs)
        Public ReadOnly Property AirplaneGridEndEdit_Command As ICommand
            Get
                If _AirplaneaGridEndEdit_Command Is Nothing Then
                    _AirplaneaGridEndEdit_Command = New DelegateCommand(Of DataGridCellEditEndingEventArgs)(AddressOf AirplaneGridEndEdit, AddressOf InitializedEnabled)
                End If
                Return _AirplaneaGridEndEdit_Command
            End Get
        End Property


        Private _MaterialEditorInputLostFocus_Command As DelegateCommand
        Public ReadOnly Property MaterialEditorInputLostFocus_Command As ICommand
            Get
                If _MaterialEditorInputLostFocus_Command Is Nothing Then
                    _MaterialEditorInputLostFocus_Command = New DelegateCommand(AddressOf MaterialEditorInputLostFocus, AddressOf InitializedEnabled)
                End If
                Return _MaterialEditorInputLostFocus_Command
            End Get
        End Property


        Private _RBSelectionChanged_Command As DelegateCommand
        Public ReadOnly Property RBSelectionChanged_Command As ICommand
            Get
                If _RBSelectionChanged_Command Is Nothing Then
                    _RBSelectionChanged_Command = New DelegateCommand(AddressOf RBSelectionChanged, AddressOf InitializedEnabled)
                End If
                Return _RBSelectionChanged_Command
            End Get
        End Property


#End Region


#Region "Command Subs"
        Private Sub UIElement_OnPreviewMouseWheel(ByVal e As MouseWheelEventArgs) 'ByVal sender As Object, 
            If Not e.Handled Then
                e.Handled = True
                Dim eventArg = New MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
                eventArg.RoutedEvent = UIElement.MouseWheelEvent
                eventArg.Source = e.Source
                Dim parent = TryCast((CType(e.Source, Control)).Parent, UIElement)
                parent?.[RaiseEvent](eventArg)
            End If
        End Sub


        Private Sub Layer_Command(context As Object)
            Dim item = context.DataContext
            Dim index = CurrentLayers.IndexOf(item)

            If index = CurrentLayers.Count - 1 AndAlso item.Name = "Subgrade" Then
                Debug.WriteLine("Bottom/Subgrade layer is Selected")

                '    UserDefinedLIsEnabled = True
                '    SubgradeLIsEnabled = True
                '    HMASurfaceIsEnabled = False
                '    HMAOverlayIsEnabled = False
                '    P301LIsEnabled = False
                '    P304LIsEnabled = False
                '    P306LIsEnabled = False
                '    HMAStLIsEnabled = False
                '    VariableFlIsEnabled = False
                '    VariableRgIsEnabled = False
                '    P154LIsEnabled = False
                '    P208LIsEnabled = False
                '    P209LIsEnabled = False
                '    P211LIsEnabled = False
                '    P219LIsEnabled = False
                '    PCCOverlayFLIsEnabled = False
                '    PCCOverlayUnLIsEnabled = False
                '    PCCSurfaceLIsEnabled = False

                '    'MaterialIsSelected = False
                '    'material = Nothing
                '    'ReplacedLayer = Nothing
                '    'MaterialsSelectionIsHidden = True

                '    AddLayerAboveIsEnabled = False
                '    AddLayerBelowIsEnabled = False

                'Else
                '    AddLayerAboveIsEnabled = True
                '    AddLayerBelowIsEnabled = True
            End If

            Layer_Click(index + 1)
        End Sub


        Private Sub ListBox_MouseDoubleClick_Command_Sub(context As Object)
            ListBox_MouseDoubleClick(context)
        End Sub


        Private Sub OnNewJob(context As Object)
            NewJob(context)
        End Sub


        Private Sub OnOpenJob(context As Object)
            OpenJob(context)
        End Sub


        Private Sub OnSectionNew(context As Object)
            SectionNew(context)
        End Sub


        Private Sub OnSaveJob(context As Object)
            SaveJob(context)
        End Sub


        Private Sub OnSaveAsJob(context As Object)
            SaveAsJob(context)
        End Sub


        Private Sub OnSaveAllJob(context As Object)
            SaveAllJob(context)
        End Sub


        Private Sub OnCloseJob(context As Object)
            CloseJob(context)
        End Sub


        Private Sub OnCreateUserDefined()
            OnCreateUserDefined_Click()
        End Sub


        Private Sub OnEditUserDefined()
            OnEditUserDefined_Click()
        End Sub


        Private Sub ExitApp()
            'Application.Current.MainWindow.Close()
            For Each win As Window In Application.Current.Windows
                win.Close()
            Next
            System.GC.Collect()
        End Sub


        Private Sub OnReset()
            ResetButton_Click()
        End Sub


        Private Sub OnHelp()
            Help()
        End Sub


        Private Sub OnAbout()
            Dim aboutWindow As New Views.AboutWindow()
            aboutWindow.Owner = System.Windows.Application.Current.MainWindow
            aboutWindow.ShowDialog()
        End Sub


        Private Sub OnSetDefault()
            'Save Design Options

            If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                My.Settings.EnglishUnits = True
            Else
                My.Settings.EnglishUnits = False
            End If

            My.Settings.CalculateHmaCdf = CurrentJob.DesignOptions.CalculateHmaCdf
            My.Settings.ReducedCrossSection = CurrentJob.DesignOptions.ReducedCrossSection
            My.Settings.AutomaticFlexibleBaseDesign = CurrentJob.DesignOptions.AutomaticFlexibleBaseDesign
            My.Settings.Outfile = CurrentJob.DesignOptions.Outfile
            My.Settings.ThickPccOverlay = CurrentJob.DesignOptions.ThickPccOverlay
            My.Settings.ACROptions = CurrentJob.DesignOptions.ACROptions
            My.Settings.CdfTolerance = CurrentJob.DesignOptions.CdfTolerance
            My.Settings.LifeTolerance = CurrentJob.DesignOptions.LifeTolerance
            My.Settings.AlternateSubgrade = CurrentJob.DesignOptions.AlternateSubgrade
            My.Settings.Nsection = Nsection
            My.Settings.AllowPartiallyBonded = CurrentJob.DesignOptions.AllowPartiallyBonded
            'My.Settings.PCAConversion = CurrentJob.DesignOptions.PCAConversion

            If ImageVisibility = Visibility.Visible Then
                My.Settings.ImageVisibility = True
            Else
                My.Settings.ImageVisibility = False
            End If

            If _brushes.Count = 20 Then
                My.Settings._brushes = True
            Else
                My.Settings._brushes = False
            End If

            My.Settings.Save()

        End Sub


        Private Sub OnResetDefault()
            'Reset Design Options

            My.Settings.EnglishUnits = True
            My.Settings.CalculateHmaCdf = False
            My.Settings.ReducedCrossSection = False
            My.Settings.AutomaticFlexibleBaseDesign = True
            My.Settings.Outfile = False
            My.Settings.ThickPccOverlay = True
            My.Settings.ACROptions = False
            My.Settings.CdfTolerance = 0.005
            My.Settings.LifeTolerance = 0.4
            My.Settings.AlternateSubgrade = False
            My.Settings.Nsection = 16
            My.Settings.AllowPartiallyBonded = False
            My.Settings.PCAConversionFormula = False
            My.Settings.NCHRPFormula = False
            My.Settings.ImageVisibility = True
            My.Settings._brushes = False
            My.Settings.Save()

            CurrentJob.DesignOptions.MeasurementSystem = FaarFieldFactory.CreateUsCustomary()
            CurrentJob.DesignOptions.CalculateHmaCdf = 0
            CurrentJob.DesignOptions.ReducedCrossSection = 0
            CurrentJob.DesignOptions.AutomaticFlexibleBaseDesign = 1
            CurrentJob.DesignOptions.Outfile = 0
            CurrentJob.DesignOptions.ThickPccOverlay = 1
            CurrentJob.DesignOptions.ACROptions = 0
            CurrentJob.DesignOptions.CdfTolerance = 0.005
            CurrentJob.DesignOptions.LifeTolerance = 0.4
            CurrentJob.DesignOptions.AlternateSubgrade = 0
            Nsection = 16
            CurrentJob.DesignOptions.AllowPartiallyBonded = 0
            ' CurrentJob.DesignOptions.PCAConversion = 0
            ImageVisibility = Visibility.Visible
            _brushes = LoadBrushes2()
            ProfileImage = DrawProfile()

        End Sub


        Private Sub OnSelectAllSections()
            For jb = 0 To Jobs.Count - 1
                For sec = 0 To DirectCast(DirectCast(Jobs(jb), JobViewModel).FaarFieldJob, FaarFieldJob).Sections.Count - 1
                    DirectCast(DirectCast(Jobs(jb), JobViewModel).FaarFieldJob, FaarFieldJob).Sections(sec).RunBatch = True
                Next
            Next
            SetSelection(CurrentSectionView)
        End Sub


        Private Sub OnDeSelectAllSections()
            For jb = 0 To Jobs.Count - 1
                For sec = 0 To DirectCast(DirectCast(Jobs(jb), JobViewModel).FaarFieldJob, FaarFieldJob).Sections.Count - 1
                    DirectCast(DirectCast(Jobs(jb), JobViewModel).FaarFieldJob, FaarFieldJob).Sections(sec).RunBatch = False
                Next
            Next
            SetSelection(CurrentSectionView)
        End Sub


        Private Sub Window_Loaded(obj As Object)

            'Auto-maximize on laptops whose work area is smaller than the
            '1280x760 design size so the UI never gets clipped by screen chrome.
            Const designW As Double = 1400
            Const designH As Double = 820
            Dim wa = System.Windows.SystemParameters.WorkArea
            If wa.Width < designW OrElse wa.Height < designH Then
                Me.WindowState = System.Windows.WindowState.Maximized
            End If

            'Check for outdated saved layout preferences and delete if exists
            Dim SavedLayoutText As String = ""
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                If storage.FileExists("RadDocking_SavedLayout.xml") Then
                    Using isoStream = storage.OpenFile("RadDocking_SavedLayout.xml", FileMode.Open)
                        Using reader As StreamReader = New StreamReader(isoStream)
                            SavedLayoutText = reader.ReadToEnd
                        End Using
                    End Using
                End If
            End Using
            If SavedLayoutText.Contains("Form 5010") OrElse
               (SavedLayoutText.Length > 0 AndAlso Not SavedLayoutText.Contains("DetailedReport")) Then
                Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                    storage.DeleteFile("RadDocking_SavedLayout.xml")
                    storage.DeleteFile("RadDocking_SavedInnerLayout.xml")
                End Using
            End If

            DockingContainer = TryCast(Application.Current.MainWindow.FindName("Docking"), RadDocking)
            InnerDockingContainer = TryCast(Application.Current.MainWindow.FindName("InnerDock"), RadDocking)

            SaveStartupLayout()

            Dim isoStore As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
            If (isoStore.FileExists("RadDocking_SavedInnerLayout.xml")) AndAlso (isoStore.FileExists("RadDocking_SavedLayout.xml")) Then
                LoadSavedLayoutFromFile()
            End If

            If My.Settings.IsMaximized = True Then
                WindowState = WindowState.Maximized
            Else
                WindowState = WindowState.Normal
            End If

            If Not Application.Current.Properties("StartFilePath") = "" Then
                OpenStartupJob(Application.Current.Properties("StartFilePath"))
            End If
            CheckForUpdates()
        End Sub


        Private Sub Structure_Loaded(obj As Object)
            If StartupJobLoading Then
                SectionIsHidden = False
                InnerSectionIsHidden = False
                AirplanesIsHidden = False
                StartupJobLoading = False
            End If
        End Sub


        Private Sub Window_Closing(obj As Object)
            Dim ret As Integer, M1 As String
            M1 = "This will close the software. Do you want to proceed?"

            ret = MsgBox(M1, MsgBoxStyle.YesNo, "FAARFIELD 2.1")

            If ret = 6 Then

                SaveLayout()

                If WindowState = WindowState.Maximized Then
                    My.Settings.IsMaximized = True
                Else
                    My.Settings.IsMaximized = False
                End If

                If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                    My.Settings.EnglishUnits = True
                Else
                    My.Settings.EnglishUnits = False
                End If

                My.Settings.Save()

            Else
                obj.Cancel = True
            End If

        End Sub


        Private Sub AdvancedOptionsToggle()
            If AdvancedOptionsToggleText Is "Show Advanced Options" Then
                AdvancedOptionsVisibility = Visibility.Visible
                AdvancedOptionsToggleText = "Hide Advanced Options"
            Else
                AdvancedOptionsVisibility = Visibility.Collapsed
                AdvancedOptionsToggleText = "Show Advanced Options"
            End If
        End Sub


        Private Sub LastLayerCheck()
            If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Name = "User Defined" Then
                MessageBox.Show("User Defined Thickness cannot be edited if at the bottom")
                CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
            End If
        End Sub


        Public PCRReset As Boolean = False
        Public PCRGraphReset As Boolean = False
        Public AirportMasterRecordReset As Boolean = False

        Public Property GridSelectedAirplane As IAirplaneInfo


        Private Sub DataGridBeginEdit(obj As DataGridBeginningEditEventArgs)
            DataGridCellValue = CType(obj.Column.GetCellContent(obj.Row), TextBlock).Text
            GridSelectedAirplane = SelectedAirplane
        End Sub


        Private Sub DataGridEndEdit(obj As DataGridCellEditEndingEventArgs)
            Dim amount = CType(obj.EditingElement, TextBox).Text
            If amount <> DataGridCellValue Then
                Application.Current.Dispatcher.InvokeAsync(New Action(Function()
                                                                          ResetCalculatedAirplaneValues()
                                                                      End Function), DispatcherPriority.ContextIdle)

                CurrentSectionView.Section.SavedPCRhtml = Nothing
                CurrentSectionView.Section.SavedPCRgraph = Nothing
                CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
                CurrentSectionView.Section.SavedCDFgraph = Nothing
                CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
                CurrentSectionView.Section.SavedSectionReportHtml = Nothing
                CurrentSectionView.Section.LastRun = Nothing
            End If
        End Sub


        Private Sub AirplaneGridEndEdit(obj As DataGridCellEditEndingEventArgs)
            Dim amount = CType(obj.EditingElement, TextBox).Text
            If amount <> DataGridCellValue Then
                Application.Current.Dispatcher.InvokeAsync(New Action(Function()
                                                                          ResetCalculatedAirplaneValues()
                                                                          'clear Store Aircraft Mix combo box after deleting an aircraft in a saved file
                                                                          CurrentLibraryIndex = -1
                                                                          CurrentSectionView.Section.TrafficMixName = Nothing
                                                                          OnPropertyChanged(NameOf(CurrentLibraryIndex))
                                                                      End Function), DispatcherPriority.ContextIdle)
                CurrentSectionView.Section.SavedPCRhtml = Nothing
                CurrentSectionView.Section.SavedPCRgraph = Nothing
                CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
                CurrentSectionView.Section.SavedCDFgraph = Nothing
                CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
                CurrentSectionView.Section.SavedSectionReportHtml = Nothing
                CurrentSectionView.Section.LastRun = Nothing
            End If
        End Sub


        Private Sub LoadAircraftLibrary()
            AirplaneGroup = New ObservableItemCollection(Of IListBoxItemViewModel)()
            AirplaneByManufacturer = New ObservableItemCollection(Of IListBoxItemViewModel)()

            'wrg xmlFilePath is for unit testing only
            AircraftLibrary = GetAircrafts(FaarFieldFactory, airplaneXMLFileLocation, False)

            FullAircraftLibrary = GetAircrafts(FaarFieldFactory, airplaneXMLFileLocation, True)

            If UDALibrary IsNot Nothing Then
                For Each airplane In UDALibrary
                    AircraftLibrary.Add(airplane)
                    FullAircraftLibrary.Add(airplane)
                Next
            End If

            Dim aircraftManufacturer = AircraftLibrary.GroupBy(Function(x) x.Manufacturer).Select(Function(x) x.First).ToList

            For Each airplaneInfo As AirplaneInfo In aircraftManufacturer
                AirplaneGroup.Add(New AirplaneGroupViewModel(airplaneInfo, Me))
            Next
            Try
                AirplaneGroup.Item(0).IsSelected = True
            Catch ex As Exception

            End Try
        End Sub

#End Region


#Region "Functions"

        Private Function FindMaterial(materialLibrary As ObservableCollection(Of MaterialDefault), name As String) As MaterialDefault
            Dim libMaterial = New MaterialDefault
            For Each mat In materialLibrary
                If mat.Name = name Then
                    libMaterial = mat
                End If
            Next
            Return libMaterial
        End Function


        ''' <summary>
        ''' Update related values when changes are made in material editor window
        ''' </summary>
        ''' <param name="obj"></param>
        Private Sub MaterialEditorInputLostFocus(obj As Object)

            If obj = "CBR" Then
                If NewCBR <> ReplacedLayer.Cbr Then
                    If NCHRPFormula Then
                        NewModulus = FaarFieldFactory.CreateModulus((2555 * (NewCBR ^ 0.64)), New UsCustomary)
                    Else
                        NewModulus = FaarFieldFactory.CreateModulus(NewCBR * 1500, FaarFieldFactory.CreateUsCustomary)
                    End If

                    'Do not update SubgradeReaction if KValueActive is False
                    If ReplacedLayer.KValueActive Then
                        If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                            If PCAConversionFormula Then
                                NewKvalue = FaarFieldFactory.CreateSubgradeReaction((0.8155 * (NewModulus.UsCustomary ^ 0.5719)), New UsCustomary)
                            Else
                                NewKvalue = FaarFieldFactory.CreateSubgradeReaction(Math.Pow(NewModulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                            End If

                        End If
                    End If
                End If
            End If

            If obj = "Modulus" Then
                If NewModulus.UsCustomary <> ReplacedLayer.Modulus.UsCustomary Then
                    'Do not update CBR if CBRActive is False
                    If ReplacedLayer.CBRActive Then
                        If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                            If NCHRPFormula Then
                                NewCBR = (NewModulus.UsCustomary / 2555) ^ (1 / 0.64)
                            Else
                                NewCBR = NewModulus.UsCustomary / 1500
                            End If
                        End If
                    End If
                    'Do not update SubgradeReaction if KValueActive is False
                    If ReplacedLayer.KValueActive Then
                        If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                            If PCAConversionFormula Then
                                NewKvalue = FaarFieldFactory.CreateSubgradeReaction((0.8155 * (NewModulus.UsCustomary ^ 0.5719)), New UsCustomary)
                            Else
                                NewKvalue = FaarFieldFactory.CreateSubgradeReaction(Math.Pow(NewModulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                            End If

                        End If

                    End If
                End If
            End If

            If obj = "KValue" Then
                If NewKvalue.UsCustomary <> ReplacedLayer.SubgradeReaction.UsCustomary Then
                    If PCAConversionFormula Then
                        NewKvalue = FaarFieldFactory.CreateSubgradeReaction((0.8155 * (NewModulus.UsCustomary ^ 0.5719)), New UsCustomary)
                    Else
                        NewModulus = FaarFieldFactory.CreateModulus(Math.Pow(NewKvalue.UsCustomary, 1.28405) * 20.15, New UsCustomary)
                    End If

                    'Do not update CBR if CBRActive is False
                    If ReplacedLayer.CBRActive Then
                        If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                            If NCHRPFormula Then
                                NewCBR = (NewModulus.UsCustomary / 2555) ^ (1 / 0.64)
                            Else
                                NewCBR = NewModulus.UsCustomary / 1500
                            End If
                        End If

                    End If
                End If
            End If

        End Sub


        Private Sub Layer_Click(obj As Object)

            For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                If TypeOf CurrentSectionView.Section.Layers.Item(i) IsNot FaarFieldModel.Material Then
                    CurrentSectionView.Section.Layers.Item(i) = CType(CurrentSectionView.Section.Layers.Item(i), FaarFieldModel.Material)
                End If
            Next

            ReplacedLayerCounter = obj - 1
            ReplacedLayer = CurrentSectionView.Section.Layers.Item(obj - 1)
            If ReplacedLayerCounter = CurrentLayers.Count - 1 Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                    ReplacedLayer.SubgradeReaction = Nothing
                    KValueUpdateEnabled = False
                    OnPropertyChanged(NameOf(KValueUpdateEnabled))

                ElseIf CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                    ReplacedLayer.Cbr = Nothing
                    CBRUpdateEnabled = False
                    OnPropertyChanged(NameOf(CBRUpdateEnabled))

                End If
            End If


            Try

                Dim FirstScreen = System.Windows.Forms.Screen.AllScreens.FirstOrDefault
                Dim SecondryScreen = System.Windows.Forms.Screen.AllScreens(1)
                Dim workingArea = SecondryScreen.WorkingArea

            Catch ex As Exception

            End Try

            'Set radio button check binding by material name
            Call CheckedMaterial()


            'copy material from library
            material = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, ReplacedLayer.Name), FindMaterial(Materials, ReplacedLayer.Name).CanDelete)
            'Set flag for material is selected
            MaterialIsSelected = True

            'Set textbox active for enabled values
            Call PreUpdateLayerScreen()

            'Set text entry values in material editor window from existing layer
            Call UpdateLayerScreen()

            MaterialsSelectionIsHidden = False

        End Sub


        Private Sub RBSelectionChanged()
            selectCheckedMaterial()
            NewModulus = material.Modulus
            NewRupture = material.Rupture
            NewCBR = material.Cbr
            NewKvalue = material.SubgradeReaction
            Call PreUpdateLayerScreen()
            If NewThickness Is Nothing Then
                NewThickness = material.Thickness
            End If
        End Sub


        Private Sub PreUpdateLayerScreen()

            If material.ThicknessActive = True Then
                ThicknessUpdateEnabled = True
            Else
                ThicknessUpdateEnabled = False
            End If

            If material.ModulusActive = True Then
                ModulusUpdateEnabled = True
            Else
                ModulusUpdateEnabled = False
            End If

            If material.RuptureActive = True Then

                RuptureUpdateEnabled = True

            Else
                RuptureUpdateEnabled = False
            End If

            If material.CBRActive = True Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                    CBRUpdateEnabled = True
                End If
            Else
                CBRUpdateEnabled = False
            End If

            If material.KValueActive = True Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                    KValueUpdateEnabled = True
                End If
            Else
                KValueUpdateEnabled = False
            End If

            If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Name = "User Defined" Then
                CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).ThicknessActive = False
                ThicknessUpdateEnabled = False
            End If
        End Sub


        Private Sub UpdateLayerScreen()
            NewThickness = ReplacedLayer.Thickness
            NewModulus = ReplacedLayer.Modulus
            NewRupture = ReplacedLayer.Rupture
            NewCBR = ReplacedLayer.Cbr
            NewKvalue = ReplacedLayer.SubgradeReaction
        End Sub


        Private Sub CheckedMaterial()
            If ReplacedLayer.Name = "User Defined" Then
                UserDefinedLIsChecked = True
            ElseIf ReplacedLayer.Name = "Subgrade" Then
                SubgradeLIsChecked = True
            ElseIf ReplacedLayer.Name = "P-154 Uncrushed Aggregate" Then
                P154LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-208 Crushed Aggregate" Then
                P208LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-209 Crushed Aggregate" Then
                P209LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-211 Lime Rock" Then
                P211LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-219 Recycled Concrete Aggregate" Then
                P219LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-401/P-403 HMA Surface" Then
                HMASurfaceIsChecked = True
            ElseIf ReplacedLayer.Name = "P-401/P-403 HMA Overlay" Then
                HMAOverlayIsChecked = True
            ElseIf ReplacedLayer.Name = "P-501 PCC Surface" Then
                PCCSurfaceLIsChecked = True
            ElseIf ReplacedLayer.Name = "P-501 PCC Overlay (unbonded)" Then
                PCCOverlayUnLIsChecked = True
            ElseIf ReplacedLayer.Name = "P-501 PCC Overlay on Flexible" Then
                PCCOverlayFLIsChecked = True
            ElseIf ReplacedLayer.Name = "P-301 Soil Cement Base" Then
                P301LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-304 Cement Treated Base" Then
                P304LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-306 Lean Concrete" Then
                P306LIsChecked = True
            ElseIf ReplacedLayer.Name = "P-401/P-403 HMA Stabilized" Then
                HMAStLIsChecked = True
            ElseIf ReplacedLayer.Name = "Variable (flexible)" Then
                VariableFlIsChecked = True
            ElseIf ReplacedLayer.Name = "Variable (rigid)" Then
                VariableRgIsChecked = True
            End If
        End Sub


        Private Sub Help()
            Dim ApplicationDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
            Dim HelpFilePath = System.IO.Path.Combine(ApplicationDir, "Resources" & Path.DirectorySeparatorChar & "FAARFIELD.chm")
            Process.Start(HelpFilePath)
        End Sub


        'End Functions
#End Region

        'wrg added to support normal instantiation - i.e., operational run, not a unit test
        Public Sub New()
            Me.New(Nothing, False)
            'cs get aircraft library version
            LibraryVersion = GetAircraftLibVersion(airplaneXMLFileLocation)
            OnPropertyChanged(NameOf(LibraryVersion))
        End Sub


        Public Sub New(xmlFilePath As String, Optional IsUnitTest As Boolean = False)

            'wrg logic to set xml file path with this alternate constructor
            'e.g., used during unit testing
            'note that when retrieving this value if it is null or white space or empty string then 
            'the getter property logic will auto set it to the default value of:
            'My.Application.Info.DirectoryPath + "\Defaults\Aircraft\aircraft.xml
            Me.airplaneXMLFileLocation = xmlFilePath


            '==================================================================
            ' MainWindow New()
            '==================================================================

            ' Read the build timestamp from the copied Resources\BuildDate.txt
            ' in the output directory (written by the FF2 PreBuildEvent). When
            ' running under a unit-test host the file may not be present, in
            ' which case the title simply omits the build suffix.
            Try
                Dim buildDatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "BuildDate.txt")
                If File.Exists(buildDatePath) Then
                    Dim raw = File.ReadAllText(buildDatePath)
                    Dim parts = Split(raw, " ")
                    If parts.Length >= 2 Then
                        BuildDate = parts(1)
                        MainWindowTitle = MainWindowTitle + " (Build " + BuildDate + ")"
                    End If
                End If
            Catch ex As Exception
                Debug.WriteLine("MainWindowViewModel: unable to read BuildDate.txt — " & ex.Message)
            End Try

            '==================================================================
            ' FaarFieldViewModel New()
            '==================================================================
            FaarFieldFactory = New FaarFieldModelFactory()
            AirplaneGroup = New ObservableItemCollection(Of IListBoxItemViewModel)()
            AirplaneByManufacturer = New ObservableItemCollection(Of IListBoxItemViewModel)()

            'Check if aircraft.xml exists in Documents\My Faarfield\Defaults\Aircraft\ directory
            If Not File.Exists(airplaneXMLFileLocation) Then
                ' Aircraft library does not exist 

                Dim folder As String = Path.GetDirectoryName(airplaneXMLFileLocation)
                If Not Directory.Exists(folder) Then
                    Directory.CreateDirectory(folder)
                End If

                FileCopy(My.Application.Info.DirectoryPath & Path.DirectorySeparatorChar & "Defaults" & Path.DirectorySeparatorChar & "Aircraft" & Path.DirectorySeparatorChar & "aircraft.xml", airplaneXMLFileLocation)
            End If

            'wrg xmlFilePath is for unit testing only
            AircraftLibrary = GetAircrafts(FaarFieldFactory, airplaneXMLFileLocation, False, IsUnitTest)

            FullAircraftLibrary = GetAircrafts(FaarFieldFactory, airplaneXMLFileLocation, True, IsUnitTest)

            If My.Settings.UDADirectory Is Nothing Or My.Settings.UDADirectory = "" Then
                UDAFolderPath = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "User Defined Aircraft" & Path.DirectorySeparatorChar
                If (Not System.IO.Directory.Exists(UDAFolderPath)) Then
                    System.IO.Directory.CreateDirectory(UDAFolderPath)
                End If
            Else
                UDAFolderPath = My.Settings.UDADirectory
            End If

            UDALibrary = GetUserDefinedAircrafts(FaarFieldFactory, UDAFolderPath)

            If Not UDALibrary Is Nothing Then
                For Each airplane In UDALibrary

                    MirrorUDAWheels(airplane)

                    AircraftLibrary.Add(airplane)
                    FullAircraftLibrary.Add(airplane)
                Next
            End If

            TrafficLibrary = GetTrafficLibrary()
            Dim aircraftManufacturer = AircraftLibrary.GroupBy(Function(x) x.Manufacturer).Select(Function(x) x.First).ToList

            For Each airplaneInfo As AirplaneInfo In aircraftManufacturer
                AirplaneGroup.Add(New AirplaneGroupViewModel(airplaneInfo, Me))
            Next
            Try
                AirplaneGroup.Item(0).IsSelected = True
            Catch ex As Exception

            End Try

            Jobs = New ObservableCollection(Of ITreeViewItemViewModel)()

            Materials = GetMaterials(FaarFieldFactory)

            Dim categories = New List(Of String)
            For Each material As Object In Materials
                If Not categories.Contains(material.Category) Then
                    categories.Add(material.Category)
                End If
            Next

            MaterialLibrary = New ObservableCollection(Of ITreeViewItemViewModel)
            For Each category In categories

                Dim materialsInCategory As New ObservableCollection(Of MaterialDefault)
                For Each mat In Materials
                    If mat.Category = category Then
                        materialsInCategory.Add(mat)
                    End If
                Next

                MaterialLibrary.Add(New MaterialCategoryViewModel(FaarFieldFactory, materialsInCategory))

            Next
            Dim analysisTypes = GetAnalysisType(FaarFieldFactory, Materials)
            ListAnalysis = New ObservableCollection(Of IAnalysisType)
            For Each Analysis As Object In analysisTypes
                ListAnalysis.Add(Analysis)
            Next

            ListExternalAircraft = New ObservableCollection(Of AirplaneInfo)
            For Each airplane In AircraftLibrary
                If airplane.Manufacturer = "External Library" Then
                    ListExternalAircraft.Add(airplane)
                End If
            Next

            CurrentAirplanes = New ObservableCollection(Of AirplaneInfo)()
            CurrentWheel = New ObservableCollection(Of UserDefinedWheel)()
            CurrentEval = New ObservableCollection(Of UserDefinedEvalPoints)()

            CurrentLayers = New ObservableCollection(Of IMaterial)()

            ListUnits = New ObservableCollection(Of IMeasurmentSystem)

            SectionNew(check)
            SelectedTabIndex = 2

            If My.Settings.EnglishUnits = True Then
                CurrentJob.DesignOptions.MeasurementSystem = FaarFieldFactory.CreateUsCustomary()
            Else
                CurrentJob.DesignOptions.MeasurementSystem = FaarFieldFactory.CreateMetric()
            End If

            CurrentJob.DesignOptions.CalculateHmaCdf = My.Settings.CalculateHmaCdf
            CurrentJob.DesignOptions.ReducedCrossSection = My.Settings.ReducedCrossSection
            CurrentJob.DesignOptions.AutomaticFlexibleBaseDesign = My.Settings.AutomaticFlexibleBaseDesign
            CurrentJob.DesignOptions.SlabStress = My.Settings.SlabStress
            CurrentJob.DesignOptions.Outfile = My.Settings.Outfile
            CurrentJob.DesignOptions.ThickPccOverlay = My.Settings.ThickPccOverlay
            CurrentJob.DesignOptions.ACROptions = My.Settings.ACROptions
            CurrentJob.DesignOptions.CdfTolerance = My.Settings.CdfTolerance
            CurrentJob.DesignOptions.LifeTolerance = My.Settings.LifeTolerance
            CurrentJob.DesignOptions.AlternateSubgrade = My.Settings.AlternateSubgrade
            Nsection = My.Settings.Nsection
            CurrentJob.DesignOptions.AllowPartiallyBonded = My.Settings.AllowPartiallyBonded
            ' CurrentJob.DesignOptions.PCAConversion = My.Settings.PCAConversion

            If My.Settings.ImageVisibility = True Then
                ImageVisibility = Visibility.Visible
            Else
                ImageVisibility = Visibility.Hidden
            End If

            'wrg avoid if unit testing because throws exception if images are not on local disk
            If (IsUnitTest = False) Then
                If My.Settings._brushes = False Then
                    _brushes = LoadBrushes2()
                Else
                    _brushes = LoadBrushes()
                End If
                ProfileImage = DrawProfile()
            End If

        End Sub


#Region "MainWindow Code Behind Properties and Functions"
        Public dt As DispatcherTimer = New DispatcherTimer()

        Public timestart As DateTime

        Public mylabel As New System.Windows.Forms.Label

        Public Property measurementsystem As IMeasurmentSystem
        Public Property GXY As Point
        Property Analysis As RunAnalysis
        Property Section As ISection
        Private _Job As IFaarFieldJob

        Public Property AirplaneWeightChecker As Double
        Public Property CopiedSectionNumber As Integer
        Public Property DesignLifeChecker As Boolean = True

        Public Property Job As IFaarFieldJob
            Get
                Return _Job
            End Get
            Set(value As IFaarFieldJob)
                _Job = value
                OnPropertyChanged(NameOf(Job))
            End Set
        End Property

        Public Property S As String = ""

        Public Property ReplacedLayerCounter As Integer
        Public Property material As IMaterial

        Private Property MaterialIsSelected As Boolean = False

        Public Property MaterialReplace As Boolean = True


        Private Sub OnDropAirplane(ByVal args As System.Windows.DragEventArgs)
            If Not CurrentSectionView Is Nothing Then

                Dim data = DragDropPayloadManager.GetDataFromObject(args.Data, "FF2.ViewModels.IListBoxItemViewModel")
                If data IsNot Nothing Then
                    For Each ap In data
                        Dim listBoxItem = DirectCast(ap, AirplaneByManufacturerViewModel)
                        Dim addedAirplane = FaarFieldFactory.CreateAircraft(listBoxItem.Airplane, FaarFieldFactory)

                        CurrentSectionView.Section.Airplanes.Add(addedAirplane)
                        CurrentAirplanes.Add(addedAirplane)

                        If listBoxItem.Airplane.IsBelly Then
                            Dim addedAirplaneBelly As IAirplaneInfo = RetrieveBellyInfo(listBoxItem.Airplane) 'wrg 
                            CurrentSectionView.Section.Airplanes.Add(addedAirplaneBelly)
                            CurrentAirplanes.Add(addedAirplaneBelly)
                        End If

                        RefreshGWPercent()

                        ResetCalculatedAirplaneValues()
                        ResetSlabStressValues()
                        UpdateSectionAirplanes()
                        CurrentSectionView.Section.SlabComplete = False
                    Next
                End If

                CheckTrafficCount(True)

            Else
                MessageBox.Show("A pavement structure should be defined before adding aircraft")
                Exit Sub
            End If

            CurrentSectionView.Section.SavedPCRhtml = Nothing
            CurrentSectionView.Section.SavedPCRgraph = Nothing
            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
            CurrentSectionView.Section.SavedCDFgraph = Nothing
            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
            CurrentSectionView.Section.LastRun = Nothing

            CurrentSectionView.Section.TrafficMixName = Nothing
            CurrentLibraryIndex = -1

        End Sub


        Private Sub OnDragMaterial(ByVal args As System.Windows.DragEventArgs)
            args.Effects = DragDropEffects.None
            args.Handled = True
        End Sub


        Private Sub OnDropMaterial(ByVal args As System.Windows.DragEventArgs)

            Try

                Dim data = DragDropPayloadManager.GetDataFromObject(args.Data, "TreeViewDragDropOptions")
                Dim dragDropPayload = CType(data, TreeView.TreeViewDragDropOptions)

                If data IsNot Nothing AndAlso TypeOf (data.DragSourceItem.Item) Is MaterialViewModel Then

                    Dim structureGrid = CType(args.Source, DataGrid)
                    Dim position = args.GetPosition(structureGrid)
                    Dim materialViewModel = CType(dragDropPayload.DraggedItems(0), MaterialViewModel)

                    Dim material = materialViewModel.Material
                    Dim materialName = material.Name


                    Dim hitTestResult = VisualTreeHelper.HitTest(structureGrid, position)
                    Dim visualTree = hitTestResult.VisualHit.GetParents()

                    For Each control In visualTree

                        If TypeOf (control) Is DataGridRow Then

                            Dim row = CType(control, DataGridRow)
                            Dim index = row.GetIndex()
                            Dim i As Integer
                            Dim j As Integer

                            If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                                For i = 1 To 25
                                    j = 15
                                    If position.Y > j + (i - 1) * 25 And position.Y <= j + i * 25 Then
                                        index = i - 1
                                    End If
                                Next
                            Else
                                For i = 1 To 25
                                    j = 17
                                    If position.Y > j + (i - 1) * 25 And position.Y <= j + i * 25 Then
                                        index = i - 1
                                    End If
                                Next
                            End If

                            MaterialsArrangementValidationRules(material, index)
                            SelectDesignedLayer()
                            OnUpdateselectedMaterial()
                            MaterialReplace = True
                            CurrentSectionView.Section.SavedPCRhtml = Nothing
                            CurrentSectionView.Section.SavedPCRgraph = Nothing
                            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
                            CurrentSectionView.Section.SavedCDFgraph = Nothing
                            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
                            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
                            CurrentSectionView.Section.LastRun = Nothing
                        End If
                    Next
                    ResetCalculatedAirplaneValues()
                    SetDesignComboBox(CurrentSectionView)
                End If

            Catch ex As Exception

                Debug.WriteLine(ex.Message, "MainWindowViewModel - OnDropMaterial")
                MessageBox.Show(ex.Message + " MainWindowViewModel - OnDropMaterial")
                Debug.WriteLine(ex)

            End Try

        End Sub


        Private Sub OnDragDropCompleted(sender As Object, e As DragDropCompletedEventArgs)
            e.Handled = True
        End Sub


        Public Sub EnterKeyAct(sender As Object, e As KeyboardEventArgs)

        End Sub


        Private Sub UnitsCombobox_Changed(e As Object)
            Try

                DrawProfile()
                GearImage = DrawGear()
                UserDefinedGearImage = DrawUserDefinedGear()
                OnUpdateselectedMaterial()
                UpdateTrafficACRHeaders()

            Catch ex As Exception
            End Try
        End Sub


        Private Sub Slab_Changed(e As Object)

            If AnalysisType IsNot Nothing AndAlso AnalysisType.Name = "New Rigid" AndAlso (CurrentSectionView.Section.SelectedRun = 0 OrElse CurrentSectionView.Section.SelectedRun = 1) Then
                If CurrentJob.DesignOptions.SlabStress = True Then
                    SlabOptions = True
                Else
                    SlabOptions = False
                End If
            Else
                SlabOptions = False
            End If

        End Sub


        Private Sub RunComboBox_Changed(e As Object)

            If CurrentSectionView IsNot Nothing Then

                If CurrentSectionView.Section.LastRun <> e.Source.SelectedValue.Content Then
                    ResetCalculatedAirplaneValues()

                    For Each airplane In CurrentSectionView.Section.Airplanes
                        If airplane.DataStorage IsNot Nothing Then
                            airplane.MgPercent = airplane.DataStorage(0)
                            airplane.MgPercentPCN = airplane.DataStorage(1)
                            airplane.RunMgPercent = airplane.MgPercent
                        End If
                    Next

                End If

            End If

            'RefreshMGPercent()

            Slab_Changed(Nothing)

            OnPropertyChanged(NameOf(CurrentSectionView))
        End Sub


        Private Sub SaveStartupLayout()
            ' Save inner docking layout in the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                Using isoStream = storage.OpenFile("RadDocking_InnerLayout.xml", FileMode.Create)
                    InnerDockingContainer.SaveLayout(isoStream)
                End Using
            End Using

            ' Save docking layout in the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                Using isoStream = storage.OpenFile("RadDocking_Layout.xml", FileMode.Create)
                    DockingContainer.SaveLayout(isoStream)
                End Using
            End Using
        End Sub


        Private Sub SaveLayout()
            ' Save inner docking layout in the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                Using isoStream = storage.OpenFile("RadDocking_SavedInnerLayout.xml", FileMode.Create)
                    InnerDockingContainer.SaveLayout(isoStream)
                End Using
            End Using

            ' Save docking layout in the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                Using isoStream = storage.OpenFile("RadDocking_SavedLayout.xml", FileMode.Create)
                    DockingContainer.SaveLayout(isoStream)
                End Using
            End Using
        End Sub


        Private Sub ResetLayout()

            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                If storage.FileExists("RadDocking_SavedInnerLayout.xml") Then
                    storage.DeleteFile("RadDocking_SavedInnerLayout.xml")
                End If
            End Using

            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                If storage.FileExists("RadDocking_SavedLayout.xml") Then
                    storage.DeleteFile("RadDocking_SavedLayout.xml")
                End If
            End Using

            ' Load inner docking layout from the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                If storage.FileExists("RadDocking_InnerLayout.xml") Then
                    Using isoStream = storage.OpenFile("RadDocking_InnerLayout.xml", FileMode.Open)
                        InnerDockingContainer.LoadLayout(isoStream)
                    End Using
                End If
            End Using

            ' Load docking layout from the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                If storage.FileExists("RadDocking_Layout.xml") Then
                    Using isoStream = storage.OpenFile("RadDocking_Layout.xml", FileMode.Open)
                        DockingContainer.LoadLayout(isoStream)
                    End Using
                End If
            End Using

            If Jobs.Count = 0 Then
                SectionIsHidden = True
                InnerSectionIsHidden = True
                AirplanesIsHidden = True
            Else
                If CurrentJob.Sections.Count > 0 Then
                    SectionIsHidden = False
                    InnerSectionIsHidden = False
                    AirplanesIsHidden = False
                Else
                    SectionIsHidden = True
                    InnerSectionIsHidden = True
                    AirplanesIsHidden = True
                End If

            End If

            Me.WindowState = Forms.FormWindowState.Maximized

        End Sub


        Private Sub LoadSavedLayoutFromFile()
            ' Load your saved inner layout from the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                If storage.FileExists("RadDocking_SavedInnerLayout.xml") Then
                    Using isoStream = storage.OpenFile("RadDocking_SavedInnerLayout.xml", FileMode.Open)
                        InnerDockingContainer.LoadLayout(isoStream)
                    End Using
                End If
            End Using

            ' Load your saved layout from the isolated storage.
            Using storage As IsolatedStorageFile = IsolatedStorageFile.GetUserStoreForAssembly()
                If storage.FileExists("RadDocking_SavedLayout.xml") Then
                    Using isoStream = storage.OpenFile("RadDocking_SavedLayout.xml", FileMode.Open)
                        DockingContainer.LoadLayout(isoStream)
                    End Using
                End If
            End Using

            If Jobs.Count = 0 Then
                SectionIsHidden = True
                InnerSectionIsHidden = True
                AirplanesIsHidden = True
            Else
                If CurrentJob.Sections.Count > 0 Then
                    SectionIsHidden = False
                    InnerSectionIsHidden = False
                    AirplanesIsHidden = False
                Else
                    SectionIsHidden = True
                    InnerSectionIsHidden = True
                    AirplanesIsHidden = True
                End If

            End If

            Me.WindowState = Forms.FormWindowState.Maximized
        End Sub


        Private Sub OnEditUserDefined_Click()
            Call ClearUserDefinedPage()

            VisibleForCreate = Visibility.Hidden

            VehicleEditIsHidden = True
            OnPropertyChanged(NameOf(VehicleEditIsHidden))

            VehicleEditIsHidden = False
            OnPropertyChanged(NameOf(VehicleEditIsHidden))

            VisibleForEdit = Visibility.Visible

        End Sub


        Private Sub OnCreateUserDefined_Click()

            Call ClearUserDefinedPage()

            VisibleForEdit = Visibility.Hidden

            VehicleEditIsHidden = True
            OnPropertyChanged(NameOf(VehicleEditIsHidden))

            VehicleEditIsHidden = False
            OnPropertyChanged(NameOf(VehicleEditIsHidden))

            VisibleForCreate = Visibility.Visible

        End Sub


        Private Sub AddToBatch_Click()
            'Add to batch checkbox click
        End Sub


        Public Sub RunButton_Click()
            timestart = DateTime.Now

            StopwatchText = 0.0
            dt.Interval = New TimeSpan(0, 0, 1)
            If CurrentAirplanes.Count = 0 Then
                MessageBox.Show("At least one aircraft is needed for a run to start.")
                Exit Sub
            End If
            If CurrentLayers.Count = 0 Then
                MessageBox.Show("Pavement type should be selected for a run to start.")
                Exit Sub
            End If

            If RunButtonText = "Run" Then
                ResetCalculatedAirplaneValues()
                ResetSlabStressValues()
                AddHandler dt.Tick, AddressOf dispatcherTimer_Tick
                dt.Start()
                RunningTimeVisibility = Visibility.Visible
                TimerVisibility = Visibility.Visible
                ShowProgressRunning("Running analysis...", "Initializing computation engine")
                Analysis = New RunAnalysis(Me)
                Analysis.RunOrCancel()
            Else
                RunAnalysisIsEnabled = False
                dt.Stop()
                RunningTimeVisibility = Visibility.Hidden
                StopwatchVisibility = Visibility.Hidden
                TimerVisibility = Visibility.Hidden
                CrossSectionVisibility = Visibility.Hidden
                ShowProgressCanceled()
                Analysis.RunOrCancel()
            End If

        End Sub


        Public Sub dispatcherTimer_Tick(ByVal sender As Object, ByVal e As EventArgs)

            If RunCompleted = False Then
                Dim span = DateTime.Now - timestart
                TimerText = span.ToString("hh\:mm\:ss")

                ' Update progress banner sub-text with the latest status line
                If _messageText IsNot Nothing AndAlso CStr(_messageText).Length > 0 Then
                    Dim lines = CStr(_messageText).Split({vbNewLine, vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
                    If lines.Length > 0 Then
                        Dim lastLine = lines(lines.Length - 1).Trim()
                        If lastLine.Length > 0 Then
                            ProgressSubText = lastLine
                            ProgressSubTextVisibility = Visibility.Visible
                        End If
                    End If
                End If

                If OverflowExit Then
                    Analysis.CancelRun()
                    OverflowExit = False
                End If

                If LowLifeExit Then
                    Analysis.CancelRun()
                    LowLifeExit = False
                End If

            Else
                Dim finalElapsed = (DateTime.Now - timestart).ToString("hh\:mm\:ss")
                dt.Stop()
                RunningTimeVisibility = Visibility.Hidden
                StopwatchVisibility = Visibility.Hidden
                TimerVisibility = Visibility.Hidden
                CrossSectionVisibility = Visibility.Hidden
                ShowProgressCompleted(finalElapsed)
                TimerText = finalElapsed
            End If
        End Sub


        Private Sub DeleteLayer_Click()

            _selectedMaterial = ReplacedLayer

            OnDeleteselectedLayer(Nothing)

            OnUpdateselectedMaterial()
            Call clearmaterialform()
            Call SetDesignComboBox(CurrentSectionView)

            ResetCalculatedAirplaneValues()

            CurrentSectionView.Section.SavedPCRhtml = Nothing
            CurrentSectionView.Section.SavedPCRgraph = Nothing
            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
            CurrentSectionView.Section.SavedCDFgraph = Nothing
            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
            CurrentSectionView.Section.LastRun = Nothing
        End Sub


        Private Sub AddLayerBelow_Click()

            If Not MaterialIsSelected Then
                MessageBox.Show("No material has been selected.")
            Else
                ReplacedLayerCounter = ReplacedLayerCounter + 1

                Call MaterialsArrangementValidationRules(material, ReplacedLayerCounter)

                If Not MaterialReplace Then
                    ReplacedLayerCounter -= 1
                End If

                MaterialReplace = True
                ResetCalculatedAirplaneValues()

            End If

            MaterialIsSelected = False
            OKThickness_Click(material, ReplacedLayerCounter)
            OKModulus_Click()
            SelectDesignedLayer()
            OnUpdateselectedMaterial()
            Call clearmaterialform()
            Call SetDesignComboBox(CurrentSectionView)

            CurrentSectionView.Section.SavedPCRhtml = Nothing
            CurrentSectionView.Section.SavedPCRgraph = Nothing
            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
            CurrentSectionView.Section.SavedCDFgraph = Nothing
            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
            CurrentSectionView.Section.LastRun = Nothing
        End Sub


        Private Sub AddLayerAbove_Click()

            If Not MaterialIsSelected Then
                MessageBox.Show("No material has been selected.")
            Else

                Call MaterialsArrangementValidationRules(material, ReplacedLayerCounter)
                MaterialReplace = True
                ResetCalculatedAirplaneValues()

            End If

            MaterialIsSelected = False
            OKThickness_Click(material, ReplacedLayerCounter)
            OKModulus_Click()
            SelectDesignedLayer()
            OnUpdateselectedMaterial()
            Call clearmaterialform()
            Call SetDesignComboBox(CurrentSectionView)

            CurrentSectionView.Section.SavedPCRhtml = Nothing
            CurrentSectionView.Section.SavedPCRgraph = Nothing
            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
            CurrentSectionView.Section.SavedCDFgraph = Nothing
            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
            CurrentSectionView.Section.LastRun = Nothing
        End Sub


        Private Sub Replace_Click()
            Dim _factory As IFaarFieldModelFactory
            Dim layers = New ObservableCollection(Of IMaterial)()
            Dim _materialLibrary As ObservableCollection(Of MaterialDefault)
            Dim _usCustomary As IMeasurmentSystem
            Dim FaarFieldFactory As New FaarFieldModelFactory
            Dim I As Integer = 0
            Dim K As Integer = 0
            Dim L As Integer

            StoreThickness = ReplacedLayer.Thickness
            StoreModulus = ReplacedLayer.Modulus
            StoreRupture = ReplacedLayer.Rupture
            StoreCBR = ReplacedLayer.Cbr
            StoreKvalue = ReplacedLayer.SubgradeReaction

            CurrentSectionView.Section.Layers.Remove(ReplacedLayer)
            CurrentLayers.Remove(ReplacedLayer)

            material.Thickness = NewThickness
            material.Modulus = NewModulus
            material.Rupture = NewRupture
            If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                material.Cbr = NewCBR
            End If
            If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                material.SubgradeReaction = NewKvalue
            End If

            If material.Name = ReplacedLayer.Name Then

                CurrentSectionView.Section.Layers.Insert(ReplacedLayerCounter, material)
                CurrentLayers.Insert(ReplacedLayerCounter, material)

            Else

                Call MaterialsArrangementValidationRules(material, ReplacedLayerCounter)

                If MaterialReplace = False Then
                    CurrentSectionView.Section.Layers.Insert(ReplacedLayerCounter, ReplacedLayer)
                    CurrentSectionView.Section.Layers.Item(ReplacedLayerCounter).Modulus = StoreModulus
                    CurrentSectionView.Section.Layers.Item(ReplacedLayerCounter).Thickness = StoreThickness
                    CurrentSectionView.Section.Layers.Item(ReplacedLayerCounter).Cbr = StoreCBR
                    CurrentSectionView.Section.Layers.Item(ReplacedLayerCounter).SubgradeReaction = StoreKvalue
                    CurrentSectionView.Section.Layers.Item(ReplacedLayerCounter).Rupture = StoreRupture
                    CurrentLayers.Insert(ReplacedLayerCounter, ReplacedLayer)
                    CurrentLayers.Item(ReplacedLayerCounter).Modulus = StoreModulus
                    CurrentLayers.Item(ReplacedLayerCounter).Thickness = StoreThickness
                    CurrentLayers.Item(ReplacedLayerCounter).Cbr = StoreCBR
                    CurrentLayers.Item(ReplacedLayerCounter).SubgradeReaction = StoreKvalue
                    CurrentLayers.Item(ReplacedLayerCounter).Rupture = StoreRupture

                    MaterialReplace = True
                End If

                ResetCalculatedAirplaneValues()

            End If

            MaterialIsSelected = False
            OKThickness_Click(material, ReplacedLayerCounter)
            CurrentSectionView.Section.Layers.Item(ReplacedLayerCounter).Thickness = material.Thickness
            OKModulus_Click()
            SelectDesignedLayer()
            OnUpdateselectedMaterial()
            Call clearmaterialform()
            Call SetDesignComboBox(CurrentSectionView)

            CurrentSectionView.Section.SavedPCRhtml = Nothing
            CurrentSectionView.Section.SavedPCRgraph = Nothing
            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
            CurrentSectionView.Section.SavedCDFgraph = Nothing
            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
            CurrentSectionView.Section.LastRun = Nothing
        End Sub


        Public Sub MaterialsArrangementValidationRules(material As IMaterial, ReplacedLayerCounter As Integer)

            Dim materialToInsert = FaarFieldFactory.CreateMaterial(FaarFieldFactory, material, True)
            Dim layers = New ObservableCollection(Of IMaterial)()
            Dim K As Integer = 0
            Dim L As Integer
            MaterialReplace = True

            Dim message = CheckLayerNumber(materialToInsert)
            If message <> "" Then
                Exit Sub
            End If

            Materials = GetMaterials(FaarFieldFactory)
            Dim analysisTypes = GetAnalysisType(FaarFieldFactory, Materials)

            If MaterialReplace = True Then
                Try
                    'User Defined can't be placed below Subgrade
                    If materialToInsert.Name = "User Defined" And CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Name = "Subgrade" And CurrentSectionView.Section.Layers.Count = ReplacedLayerCounter Then
                        MessageBox.Show("User Defined layer can not be presented below the Subgrade layer.")
                        MaterialReplace = False

                    ElseIf materialToInsert.Name = "User Defined" And materialToInsert.Thickness.UsCustomary = 0 Then
                        materialToInsert.CBRActive = False
                        materialToInsert.KValueActive = False
                    End If

                    CurrentSectionView.Section.Layers.Insert(ReplacedLayerCounter, materialToInsert)
                    CurrentLayers.Insert(ReplacedLayerCounter, materialToInsert)

                    'Subgrade layer
                    If material.Name = "Subgrade" And CurrentSectionView.Section.Layers.Count - 1 > ReplacedLayerCounter Then
                        MessageBox.Show("Subgrade layers can only be placed at the bottom layer of the pavement.")
                        MaterialReplace = False
                    End If

                    'User Defined layer
                    If materialToInsert.Name = "User Defined" And CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Name = "Subgrade" And CurrentSectionView.Section.Layers.Count = ReplacedLayerCounter Then
                        MessageBox.Show("User Defined layer can not be presented below the Subgrade layer.")
                        MaterialReplace = False
                    End If

                    'P-401/P-403 HMA Surface layer
                    If CurrentSectionView.Section.Layers.Item(0).Name = "P-401/P-403 HMA Surface" Then
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(0)
                        _analysisType = analysisTypes.Item(0)
                        AnalysisListSelectedIndex = 0

                        'P-501 PCC Surface layer
                    ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Surface" Then
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(4)
                        _analysisType = analysisTypes.Item(4)
                        AnalysisListSelectedIndex = 4

                        'P-401/P-403 HMA Overlay layer
                    ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-401/P-403 HMA Overlay" Then
                        If CurrentSectionView.Section.Layers.Item(1).Name = "P-401/P-403 HMA Surface" Then
                            CurrentSectionView.Section.AnalysisType = analysisTypes.Item(2)
                            _analysisType = analysisTypes.Item(2)
                            AnalysisListSelectedIndex = 2
                        ElseIf CurrentSectionView.Section.Layers.Item(1).Name = "P-501 PCC Surface" Then
                            CurrentSectionView.Section.AnalysisType = analysisTypes.Item(3)
                            _analysisType = analysisTypes.Item(3)
                            AnalysisListSelectedIndex = 3
                        ElseIf CurrentSectionView.Section.Layers.Item(1).Name = "User Defined" Then
                            CurrentSectionView.Section.AnalysisType = analysisTypes.Item(2)
                            _analysisType = analysisTypes.Item(2)
                            AnalysisListSelectedIndex = 2

                        Else
                            _analysisType = Nothing
                        End If

                        If CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                            materialToInsert.Thickness = FaarFieldFactory.CreateThickness(4, FaarFieldFactory.CreateUsCustomary())
                        ElseIf CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                            materialToInsert.Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
                        End If

                        'P-501 PCC Overlay (unbonded)
                    ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay (unbonded)" Then
                        If CurrentSectionView.Section.Layers.Item(1).Name = "P-501 PCC Surface" Then
                            CurrentSectionView.Section.AnalysisType = analysisTypes.Item(6)
                            AnalysisListSelectedIndex = 6
                        Else
                            MessageBox.Show("P-501 PCC Overlay (unbonded) can only be placed over PCC surface layer.")
                            MaterialReplace = False
                        End If
                        OnUpdateselectedMaterial()
                    ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay on Flexible" Then
                        If CurrentSectionView.Section.Layers.Item(1).Name = "P-401/P-403 HMA Surface" Or CurrentSectionView.Section.Layers.Item(1).Name = "User Defined" Then
                            CurrentSectionView.Section.AnalysisType = analysisTypes.Item(5)
                            AnalysisListSelectedIndex = 5
                        Else
                            MessageBox.Show("P-501 PCC Overlay on Flexible can only be placed over asphalt surface or User Defined layer.")
                            MaterialReplace = False
                        End If

                    End If

                    'Only PCC and HMA are allowded as top layer
                    If materialToInsert.Name <> "User Defined" And ReplacedLayerCounter < CurrentSectionView.Section.Layers.Count - 1 Then
                        If CurrentSectionView.Section.Layers.Item(0).Category <> "P-401/P-403 HMA" And CurrentSectionView.Section.Layers.Item(0).Category <> "P-501 PCC" And CurrentSectionView.Section.Layers.Item(0).Name <> "User Defined" Then
                            MessageBox.Show("Only surface material (PCC or HMA) are allowed at the top of the pavement")
                            MaterialReplace = False
                        End If
                    End If

                    For J = 0 To CurrentSectionView.Section.Layers.Count - 1
                        If materialToInsert.Name <> "User Defined" And materialToInsert.Name <> "Variable (flexible)" And materialToInsert.Name <> "Variable (rigid)" And materialToInsert.Category <> "Stabilized" Then
                            If J <> ReplacedLayerCounter Then
                                If materialToInsert.Name = CurrentSectionView.Section.Layers.Item(J).Name Then
                                    MessageBox.Show("Only one " + materialToInsert.Name + " layer can be presented in the pavement structure.")
                                    MaterialReplace = False
                                    K = 1
                                End If
                            End If
                        End If
                    Next

                    For J = 0 To CurrentSectionView.Section.Layers.Count - 1
                        If J <> ReplacedLayerCounter Then
                            If MaterialReplace = True Then
                                If materialToInsert.Category = "Aggregate" And Not materialToInsert.Name = "P-154 Uncrushed Aggregate" Then
                                    If materialToInsert.Category = CurrentSectionView.Section.Layers.Item(J).Category Then
                                        If CurrentSectionView.Section.Layers.Item(J).Name = "P-154 Uncrushed Aggregate" Then
                                            If J < ReplacedLayerCounter Then
                                                MessageBox.Show("An aggregate base layer cannot be located below a subbase layer.")
                                                MaterialReplace = False
                                            End If
                                        Else
                                            MessageBox.Show("Only one aggregate layer can be present in the pavement structure.")
                                            MaterialReplace = False

                                        End If
                                    End If

                                    If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Then
                                        materialToInsert.Thickness = FaarFieldFactory.CreateThickness(10, FaarFieldFactory.CreateUsCustomary())
                                    ElseIf CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Then
                                        materialToInsert.Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
                                    End If

                                End If
                            End If
                        End If
                    Next

                    If materialToInsert.Name = "P-154 Uncrushed Aggregate" Then
                        For J = 0 To CurrentSectionView.Section.Layers.Count - 1
                            If CurrentSectionView.Section.Layers.Item(J).Name = materialToInsert.Name And K <> 1 Then
                                L = J
                                For M = L + 1 To CurrentSectionView.Section.Layers.Count - 1
                                    If CurrentSectionView.Section.Layers.Item(M).Category = "Aggregate" Then
                                        MessageBox.Show("An aggregate base layer cannot be located below a subbase layer.")
                                        MaterialReplace = False
                                    End If
                                Next
                            End If
                        Next
                        K = 0
                    End If

                    If materialToInsert.Name.Contains("Overlay") Then
                        If ReplacedLayerCounter <> 0 Then
                            MessageBox.Show("Overlay can only be placed As the top layer.")
                            MaterialReplace = False
                        End If
                    End If

                    If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Category <> "General" Then
                        MessageBox.Show("Only Subgrade And User Defined layers can be placed at the bottom Of the Structure.")
                        MaterialReplace = False
                    End If

                    If (material.Category = "Stabilized") And ReplacedLayerCounter = 0 Then
                        MessageBox.Show(materialToInsert.Name + " can Not be placed On the top Of the pavement Structure")
                        MaterialReplace = False
                    End If


                    If materialToInsert.Name.Contains("Surface") And ReplacedLayerCounter = 1 Then
                        If Not CurrentSectionView.Section.Layers.Item(0).Name.Contains("Overlay") Then
                            MessageBox.Show(materialToInsert.Name + " Layer can only be placed On the top layer Of the Structure, " + vbNewLine + "Or directly below On asphalt, Or PCC overlay.")
                            MaterialReplace = False
                        End If
                    End If

                    If materialToInsert.Name.Contains("Surface") And ReplacedLayerCounter > 1 Then
                        MessageBox.Show(materialToInsert.Name + " Layer can only be placed On the top layer Of the Structure, " + vbNewLine + "Or directly below On asphalt, Or PCC overlay.")
                        MaterialReplace = False
                    End If

                    If ReplacedLayerCounter < CurrentSectionView.Section.Layers.Count - 1 Then
                        materialToInsert.CBRActive = False
                        materialToInsert.KValueActive = False
                    Else
                        If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).Name = "User Defined" Then
                            CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).CBRActive = False
                            CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).KValueActive = False
                            CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).ThicknessActive = True
                            CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
                        End If

                    End If
                Catch ex As Exception
                End Try
            End If

            If MaterialReplace = False Then
                CurrentSectionView.Section.Layers.Remove(materialToInsert)
                CurrentLayers.Remove(materialToInsert)
            End If
        End Sub


        Private Function CheckLayerNumber(material As IMaterial)
            Dim materialName = material.Name
            Dim Pavementtypechecker As Boolean = False
            MaterialReplace = True

            For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                If CurrentSectionView.Section.Layers.Item(i).Name.Contains("PCC") Then
                    Pavementtypechecker = True
                ElseIf AnalysisType.Name.Contains("Rigid") Then
                    Pavementtypechecker = True
                ElseIf materialName.Contains("PCC") Then
                    Pavementtypechecker = True
                End If
            Next

            If (CurrentSectionView.Section.Layers.Count >= 5 And Pavementtypechecker = True) Then
                If materialName.Contains("Overlay") Or CurrentSectionView.Section.Layers.Item(0).Name.Contains("Overlay") Then
                    If CurrentSectionView.Section.Layers.Count >= 6 Then
                        MessageBox.Show("The maximum number of layers allowed in Rigid Pavements with Overlay is 6")
                        MaterialReplace = False
                    End If
                Else
                    MessageBox.Show("The maximum number of layers allowed in Rigid Pavements is 5")
                    MaterialReplace = False
                End If
            End If
            Return ""
        End Function


        Public Sub SetDesignComboBox(CurrentSectionView As SectionViewModel)
            Materials = GetMaterials(FaarFieldFactory)
            CurrentSectionView = CurrentSectionView
            Dim storeLayers As New ObservableCollection(Of IMaterial)
            Dim numberoflayers As Integer = 0
            If Not CurrentSectionView Is Nothing Then
                For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                    storeLayers.Add(CurrentSectionView.Section.Layers.Item(i))
                    numberoflayers += 1
                Next
            End If

            Try
                CurrentLayers.Clear()
                CurrentLayers.Clear()
                CurrentSectionView.Section.Layers.Clear()
                For i = 0 To numberoflayers - 1
                    CurrentSectionView.Section.Layers.Add(storeLayers(i))
                Next

                For Each layer In CurrentSectionView.Section.Layers
                    CurrentLayers.Add(layer)
                Next
            Catch ex As Exception
                Debug.WriteLine(ex)
            End Try

            Try

                Dim analysisTypes = GetAnalysisType(FaarFieldFactory, Materials)
                If CurrentSectionView.Section.Layers.Item(0).Name = "P-401/P-403 HMA Surface" Then
                    If CurrentSectionView.Section.Layers.Item(1).Name.Contains("Aggregate") Then

                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(1)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(1)
                        AnalysisListSelectedIndex = 1

                    Else
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(0)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(0)
                        AnalysisListSelectedIndex = 0
                    End If

                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Surface" Then
                    CurrentSectionView.Section.AnalysisType = Nothing
                    _analysisType = Nothing
                    _analysisType = analysisTypes.Item(4)
                    OnPropertyChanged(NameOf(AnalysisType))
                    CurrentSectionView.Section.AnalysisType = analysisTypes.Item(4)
                    AnalysisListSelectedIndex = 4
                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-401/P-403 HMA Overlay" Then
                    If CurrentSectionView.Section.Layers.Item(1).Name = "P-401/P-403 HMA Surface" Then
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(2)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(2)
                        AnalysisListSelectedIndex = 2

                    ElseIf CurrentSectionView.Section.Layers.Item(1).Name = "P-501 PCC Surface" Then
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(3)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(3)
                        AnalysisListSelectedIndex = 3

                    ElseIf CurrentSectionView.Section.Layers.Item(1).Name = "User Defined" Then
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(2)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(2)
                        AnalysisListSelectedIndex = 2

                    Else
                        _analysisType = Nothing
                        OnUpdateAnalysisType()

                    End If
                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay (unbonded)" Then
                    If CurrentSectionView.Section.Layers.Item(1).Name = "P-501 PCC Surface" Then
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(6)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(6)
                        AnalysisListSelectedIndex = 6
                    Else
                        _analysisType = Nothing
                        OnUpdateAnalysisType()

                    End If
                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay on Flexible" Then
                    If CurrentSectionView.Section.Layers.Item(1).Name = "P-401/P-403 HMA Surface" Or CurrentSectionView.Section.Layers.Item(1).Name = "User Defined" Then
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(5)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(5)
                        AnalysisListSelectedIndex = 5

                    Else
                        _analysisType = Nothing
                        OnUpdateAnalysisType()
                    End If
                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "User Defined" Then
                    If CurrentSectionView.Section.Layers.Item(1).Name = "P-401/P-403 HMA Surface" Then
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(2)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(2)
                        AnalysisListSelectedIndex = 2

                    ElseIf CurrentSectionView.Section.Layers.Item(1).Name = "P-501 PCC Surface" Then
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(3)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(3)
                        AnalysisListSelectedIndex = 3
                    Else
                        CurrentSectionView.Section.AnalysisType = Nothing
                        _analysisType = Nothing
                        _analysisType = analysisTypes.Item(0)
                        OnPropertyChanged(NameOf(AnalysisType))
                        CurrentSectionView.Section.AnalysisType = analysisTypes.Item(0)
                        AnalysisListSelectedIndex = 0
                    End If
                End If

            Catch ex As Exception

            End Try
            CurrentSectionView = CurrentSectionView
            OnUpdateselectedMaterial()
        End Sub


        Function selectCheckedMaterial()

            Dim layers = New ObservableCollection(Of IMaterial)()
            Dim FaarFieldFactory As New FaarFieldModelFactory
            Dim I As Integer = 0
            Dim K As Integer = 0
            Dim L As Integer

            Materials = GetMaterials(FaarFieldFactory)

            Dim UserDefined = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "User Defined"), False)
            Dim Subgrade = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "Subgrade"), False)
            If ReplacedLayerCounter < CurrentLayers.Count - 1 Then

                UserDefined.Cbr = Nothing
                UserDefined.CBRActive = False
                UserDefined.Modulus = FaarFieldFactory.CreateModulus(100000, FaarFieldFactory.CreateUsCustomary())
                UserDefined.SubgradeReaction = Nothing
                UserDefined.ThicknessActive = True
                UserDefined.Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
            Else
                If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                    UserDefined.Cbr = 66.6666
                    UserDefined.ThicknessActive = False
                    UserDefined.Thickness = Nothing
                    UserDefined.SubgradeReaction = Nothing
                    KValueUpdateEnabled = False
                    OnPropertyChanged(NameOf(KValueUpdateEnabled))
                Else

                    UserDefined.ThicknessActive = False
                    UserDefined.Thickness = Nothing
                    UserDefined.SubgradeReaction = FaarFieldFactory.CreateSubgradeReaction(172.4, New UsCustomary)
                    UserDefined.Cbr = Nothing
                    CBRUpdateEnabled = False
                    OnPropertyChanged(NameOf(CBRUpdateEnabled))
                End If

            End If
            If ReplacedLayerCounter = CurrentLayers.Count - 1 Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                    Subgrade.SubgradeReaction = Nothing
                    KValueUpdateEnabled = False
                    OnPropertyChanged(NameOf(KValueUpdateEnabled))

                ElseIf CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                    Subgrade.Cbr = Nothing
                    CBRUpdateEnabled = False
                    OnPropertyChanged(NameOf(CBRUpdateEnabled))

                End If
            End If

            UserDefined.CanDelete = True
            layers.Add(UserDefined)

            Subgrade.Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
            layers.Add(Subgrade)

            Dim uncrushed = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-154 Uncrushed Aggregate"), True)
            uncrushed.Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
            layers.Add(uncrushed)

            Dim P208 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-208 Crushed Aggregate"), True)
            P208.Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P208)

            Dim P209 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-209 Crushed Aggregate"), True)
            P209.Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P209)

            Dim P211 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-211 Lime Rock"), True)
            P211.Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P211)

            Dim P219 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-219 Recycled Concrete Aggregate"), True)
            P219.Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P219)

            Dim flexible = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-401/P-403 HMA Surface"), False)
            flexible.Thickness = FaarFieldFactory.CreateThickness(4, FaarFieldFactory.CreateUsCustomary())
            layers.Add(flexible)

            Dim overlay = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-401/P-403 HMA Overlay"), False)
            overlay.Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
            layers.Add(overlay)

            Dim rigid = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Surface"), False)
            rigid.Thickness = FaarFieldFactory.CreateThickness(14, FaarFieldFactory.CreateUsCustomary())
            layers.Add(rigid)

            Dim P501OverlayUn = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Overlay (unbonded)"), False)
            P501OverlayUn.Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P501OverlayUn)

            Dim P501OverlayFl = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Overlay on Flexible"), False)
            P501OverlayFl.Thickness = FaarFieldFactory.CreateThickness(15, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P501OverlayFl)

            Dim P301 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-301 Soil Cement Base"), True)
            P301.Thickness = FaarFieldFactory.CreateThickness(5, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P301)

            Dim P304 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-304 Cement Treated Base"), True)
            P304.Thickness = FaarFieldFactory.CreateThickness(5, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P304)

            Dim P306 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-306 Lean Concrete"), True)
            P306.Thickness = FaarFieldFactory.CreateThickness(5, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P306)

            Dim stabilized = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-401/P-403 HMA Stabilized"), True)
            stabilized.Thickness = FaarFieldFactory.CreateThickness(5, FaarFieldFactory.CreateUsCustomary())
            layers.Add(stabilized)

            Dim VariableFlex = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "Variable (flexible)"), True)
            VariableFlex.Thickness = FaarFieldFactory.CreateThickness(5, FaarFieldFactory.CreateUsCustomary())
            layers.Add(VariableFlex)

            Dim VariableRigid = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "Variable (rigid)"), True)
            VariableRigid.Thickness = FaarFieldFactory.CreateThickness(5, FaarFieldFactory.CreateUsCustomary())
            layers.Add(VariableRigid)

            Dim P501OverlayPB = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Overlay (partially bonded)"), False)
            P501OverlayPB.Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
            layers.Add(P501OverlayPB)

            MaterialIsSelected = True
            If HMASurfaceIsChecked Then
                material = layers(7)
            ElseIf UserDefinedLIsChecked Then
                material = layers(0)
            ElseIf SubgradeLIsChecked Then
                material = layers(1)
            ElseIf P154LIsChecked Then
                material = layers(2)
            ElseIf P208LIsChecked Then
                material = layers(3)
            ElseIf P209LIsChecked Then
                material = layers(4)
            ElseIf P211LIsChecked Then
                material = layers(5)
            ElseIf P219LIsChecked Then
                material = layers(6)
            ElseIf HMASurfaceIsChecked Then
                material = layers(7)
            ElseIf HMAOverlayIsChecked Then
                material = layers(8)
            ElseIf PCCSurfaceLIsChecked Then
                material = layers(9)
            ElseIf PCCOverlayUnLIsChecked Then
                material = layers(10)
            ElseIf PCCOverlayFLIsChecked Then
                material = layers(11)
            ElseIf P301LIsChecked Then
                material = layers(12)
            ElseIf P304LIsChecked Then
                material = layers(13)
            ElseIf P306LIsChecked Then
                material = layers(14)
            ElseIf HMAStLIsChecked Then
                material = layers(15)
            ElseIf VariableFlIsChecked Then
                material = layers(16)
            ElseIf VariableRgIsChecked Then
                material = layers(17)
            Else
                material = Nothing
                MaterialIsSelected = False
            End If
            Return material
        End Function


        Private Sub CancelMaterial_Click()
            clearmaterialform()
        End Sub


        Public Sub clearmaterialform()
            UserDefinedLIsEnabled = True
            SubgradeLIsEnabled = True
            HMASurfaceIsEnabled = True
            HMAOverlayIsEnabled = True
            P301LIsEnabled = True
            P304LIsEnabled = True
            P306LIsEnabled = True
            HMAStLIsEnabled = True
            VariableFlIsEnabled = True
            VariableRgIsEnabled = True
            P154LIsEnabled = True
            P208LIsEnabled = True
            P209LIsEnabled = True
            P211LIsEnabled = True
            P219LIsEnabled = True
            PCCOverlayFLIsEnabled = True
            PCCOverlayUnLIsEnabled = True
            PCCSurfaceLIsEnabled = True
            MaterialIsSelected = False
            material = Nothing
            ReplacedLayer = Nothing
            MaterialsSelectionIsHidden = True
        End Sub


        'Thickness rules
        Private Sub OKThickness_Click(material As IMaterial, ReplacedLayerCounter As Integer)
            Dim FaarFieldFactory As New FaarFieldModelFactory
            Dim CurrentLayers As New ObservableCollection(Of IMaterial)
            Dim CheckforRigidLayer As Integer = 0

            Dim msg As String = ""

            For I = 0 To CurrentSectionView.Section.Layers.Count - 1
                If CurrentSectionView.Section.Layers.Item(I).Category = "P-501 PCC" Then
                    CheckforRigidLayer = 1
                End If
            Next
            If Not NewThickness Is Nothing Then

                msg = ThicknessVerificationRules(material)

                If Not msg = "" Then
                    MessageBox.Show(msg)
                    NewThickness = material.Thickness
                    Exit Sub
                End If

            End If

            Dim index = 1
            For Each layer In CurrentSectionView.Section.Layers
                CurrentLayers.Add(layer)
                index += 1
            Next
            OnUpdateselectedMaterial()
            NewThickness = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary())
            LayerThicknessMenuIsHidden = True

        End Sub


        Private Sub OKModulus_Click()
            Dim FaarFieldFactory As New FaarFieldModelFactory
            Dim CurrentLayers As New ObservableCollection(Of IMaterial)

            Dim index = 1
            For Each layer In CurrentSectionView.Section.Layers
                CurrentLayers.Add(layer)
                index += 1
            Next
            OnUpdateselectedMaterial()
            NewModulus = FaarFieldFactory.CreateModulus(0, FaarFieldFactory.CreateUsCustomary())

            If material.Name = "User Defined" Then

                If ReplacedLayerCounter = CurrentSectionView.Section.Layers.Count - 1 Then
                    material.Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary)
                Else
                    material.CBRActive = False
                    material.KValueActive = False
                    material.ThicknessActive = True
                End If

            End If

        End Sub


        Private Sub ResetButton_Click()
            If Jobs.Count <= 0 Then
                Jobs.Clear()
                SectionNew(Nothing)
            End If
            clearmaterialform()
            ResetLayout()
        End Sub


        Private Sub ListBox_MouseDoubleClick(obj As MouseButtonEventArgs)

            If TypeOf (obj.OriginalSource) IsNot TextBlock Then
                Exit Sub
            End If

            If obj.OriginalSource.DataContext.IsSelected = False Then
                obj.OriginalSource.DataContext.IsSelected = True
            End If

            Dim listBox = DirectCast(obj.Source, ListBox)
            For Each lbi In listBox.SelectedItems
                Dim listBoxItem = DirectCast(lbi, AirplaneByManufacturerViewModel)
                Dim addedAirplane = FaarFieldFactory.CreateAircraft(listBoxItem.Airplane, FaarFieldFactory)

                If Jobs.Count = 0 Then
                    MessageBox.Show("A job and at least one structure should be defined before adding aircraft to the Traffic.")
                    RunButtonText = "Run"

                    Exit Sub
                End If

                If SectionIsHidden = True Then
                    RunCompleted = True
                    MessageBox.Show("Analysis cannot run because no Structure has been defined.")
                    RunButtonText = "Run"

                    Exit Sub
                End If
                If addedAirplane.ACRThick Is Nothing Then
                    addedAirplane.ACRThick = FaarFieldFactory.CreateThickness(0, CurrentJob.DesignOptions.MeasurementSystem)
                End If
                If CurrentSectionView Is Nothing Then
                    MessageBox.Show("A pavement structure should be defined before adding aircraft")
                    Exit Sub
                End If

                CurrentSectionView.Section.Airplanes.Add(addedAirplane)
                CurrentAirplanes.Add(addedAirplane)

                If listBoxItem.Airplane.IsBelly Then
                    Dim addedAirplaneBelly As IAirplaneInfo = RetrieveBellyInfo(listBoxItem.Airplane) 'wrg 
                    CurrentSectionView.Section.Airplanes.Add(addedAirplaneBelly)
                    CurrentAirplanes.Add(addedAirplaneBelly)
                End If

                RefreshGWPercent()

                ResetCalculatedAirplaneValues()
                UpdateSectionAirplanes()
            Next

            CheckTrafficCount(True)

            For Each lbis In listBox.ItemsSource
                lbis.IsSelected = False
            Next

            CurrentSectionView.Section.TrafficMixName = Nothing
            CurrentLibraryIndex = -1

        End Sub


        'wrg
        Public Function RetrieveBellyInfo(addedAirplane As IAirplaneInfo) As IAirplaneInfo

            Dim bellyIndex As Integer = Nothing

            For i = 0 To FullAircraftLibrary.Count - 1
                If FullAircraftLibrary.Item(i).Name = addedAirplane.Name + " Belly" Then
                    bellyIndex = i
                End If
            Next

            Dim addedAirplaneBelly As IAirplaneInfo = FaarFieldFactory.CreateAircraftbelly(FullAircraftLibrary.Item(bellyIndex), FaarFieldFactory)

            Return addedAirplaneBelly

        End Function


        Public Sub OnPreviewMouseRightButtonDown(obj As RoutedEventArgs)

            If obj.OriginalSource.GetType() = GetType(TextBlock) Then
                Dim tb As TextBlock = CType(obj.OriginalSource, TextBlock)

                'If Tree Node is type Job or type Section, then no other nodes will display context menu
                If Not (tb.DataContext.GetType() Is GetType(JobViewModel) Or tb.DataContext.GetType() Is GetType(SectionViewModel)) Then

                    tb.ContextMenu = Nothing

                End If

                If tb.DataContext.GetType() Is GetType(JobViewModel) Then
                    CloseJobMenuItemVisibility = Visibility.Visible
                Else
                    CloseJobMenuItemVisibility = Visibility.Collapsed
                End If

            End If

        End Sub


        Private Function ContextDisable(strNodeName As String) As Boolean
            If strNodeName = "Job Information" Then Return True
            If strNodeName = "Design Options" Then Return True
            If strNodeName = "Summary Report" Then Return True
            If strNodeName = "Structures" Then Return True
            If strNodeName = "Structure Report" Then Return True
            If strNodeName = "CDF Graph" Then Return True
            If strNodeName = "PCR Report" Then Return True

            Return False
        End Function


        Private Sub MenuItemCopy_Click(obj As RoutedEventArgs)
            ' Treeview contextmenu click event
            ' This copies either an Entire Job or a Section to the clipboard
            Dim menuChoice = DirectCast(obj.Source, MenuItem)

            If menuChoice.DataContext.Parent Is Nothing Then
                Dim jobView = CType(menuChoice.DataContext, JobViewModel)

                Dim canCloneJob = False

                For Each svm In menuChoice.DataContext.Children(3).Children
                    If svm.Section.Layers.Count > 0 Then
                        canCloneJob = True
                    End If
                Next
                If canCloneJob Then
                    CloneJob(jobView)
                    'MessageText = ""
                Else
                    MessageBox.Show("You cannot copy a job that does not have Pavement Layers.")
                End If

            Else
                'Clone SECTION
                Dim sectionView = CType(menuChoice.DataContext, SectionViewModel)
                If menuChoice.DataContext.Section.Layers.Count = 0 Then
                    MessageBox.Show("You cannot copy a structure that does not have Pavement Layers.")
                Else
                    CloneSection(sectionView)
                End If

            End If

        End Sub


        Public Sub MenuItemDelete_Click(obj As RoutedEventArgs)
            Try
                Dim menuChoice = DirectCast(obj.Source, MenuItem)
                Dim jvm As JobViewModel
                Dim svm As SectionViewModel

                If TypeOf menuChoice.DataContext Is JobViewModel Then
                    ' Removing Job
                    jvm = menuChoice.DataContext
                    If Not jvm Is Nothing Then
                        DeleteJob(jvm)
                    End If
                ElseIf TypeOf menuChoice.DataContext Is SectionViewModel Then
                    ' Removing Section
                    svm = menuChoice.DataContext
                    If Not svm Is Nothing Then
                        jvm = svm.Parent.Parent

                        ' Additional reference needs removed
                        Dim index = jvm.SectionFolder.Children.IndexOf(svm)
                        jvm.FaarFieldJob.Sections.RemoveAt(index)

                        jvm.SectionFolder.Children.Remove(svm)

                        ' Manually update ui
                        If jvm.SectionFolder.Children.Count <> 0 Then
                            CurrentSectionView = jvm.SectionFolder.Children.Item(0)
                            CurrentLayers.Clear()
                            For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                                CurrentLayers.Add(CurrentSectionView.Section.Layers.Item(i))
                            Next
                            SetCurrentSection(CurrentSectionView)
                        Else
                            'Clear page contents and selections
                            CurrentLayers.Clear()
                            CurrentAirplanes.Clear()
                            CurrentAirplanes.Clear()
                            ProfileImage = Nothing
                            AnalysisListSelectedIndex = -1
                            CurrentLibraryIndex = -1
                            CurrentSectionView = Nothing
                            SectionRequiredButtonEnabled = False
                            SetSelection(jvm)

                            SectionIsHidden = True
                            InnerSectionIsHidden = True
                            AirplanesIsHidden = True

                        End If
                    End If
                End If
            Catch ex As Exception
                Debug.WriteLine(ex.Message, "MainWindowViewModel - MenuItemDelete_Click")
                MessageBox.Show(ex.Message + " MainWindowViewModel - MenuItemDelete_Click")
                Debug.WriteLine(ex)
            End Try
        End Sub


        Private Sub MenuItemPaste_Click(obj As RoutedEventArgs)
            ' Treeview contextmenu click event
            Dim menuChoice = DirectCast(obj.Source, MenuItem)

            'Clone JOB
            If menuChoice.DataContext.Parent Is Nothing Then

            Else
                If ContextDisable(menuChoice.DataContext.Name) And menuChoice.DataContext.Name IsNot "Structures" Then
                    Return
                End If
                'PASTE section
                Dim sectionView = CType(menuChoice.DataContext.Parent, SectionFolderViewModel)

                PasteSection(sectionView)

            End If

        End Sub


        Private Sub ValidateGridChanges()
            Dim msg As String = ""

            If SelectedMaterial Is Nothing Then
                Exit Sub
            End If

            Try
                If SelectedMaterial.Name = "Subgrade" Then
                    ProfileImage = DrawProfile()
                    Exit Sub
                End If
            Catch ex As Exception

            End Try

            If SelectedMaterial Is CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1) Then
                If SelectedMaterial.Name = "User Defined" Then
                    ProfileImage = DrawProfile()
                    Exit Sub
                End If
            End If

            msg = ThicknessVerificationRules(SelectedMaterial)

            If Not msg = "" Then
                MessageBox.Show(msg)
            End If

            ProfileImage = DrawProfile()
            OnUpdateselectedMaterial()

        End Sub


        'Thickness rules
        Public Function ThicknessVerificationRules(SelectedMaterial As IMaterial) As String
            Dim FaarfieldFactory As New FaarFieldModelFactory
            Dim msg As String = ""
            Dim aiplaneWeightList = New List(Of Double)

            For Each airplane In CurrentAirplanes
                Dim airplaneWeight As Double = airplane.GrossWeight.UsCustomary
                aiplaneWeightList.Add(airplaneWeight)
            Next

            Dim maxAiplaneWeight As Double
            If CurrentAirplanes.Count = 0 Then
                maxAiplaneWeight = 0
            Else
                maxAiplaneWeight = Enumerable.Max(aiplaneWeightList)
            End If

            If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" And maxAiplaneWeight < 30000 Then
                AirplaneWeightChecker = 4
            Else
                If maxAiplaneWeight < 60000 Then
                    AirplaneWeightChecker = 1
                ElseIf maxAiplaneWeight > 60000 And maxAiplaneWeight < 100000 Then
                    AirplaneWeightChecker = 2
                Else
                    AirplaneWeightChecker = 3
                End If
            End If

            Dim PavementType = CurrentSectionView.Section.AnalysisType.Name
            Dim DesignType = CurrentSectionView.Section.SelectedRun
            Dim MinimumThickness As Integer = 0

            '1. P-401/P-403 HMA Surface
            If SelectedMaterial.Name = "P-401/P-403 HMA Surface" Then
                If PavementType = "New Flexible" Or PavementType = "HMA on Aggregate" Then
                    If DesignType = 0 Or DesignType = 2 Then
                        If (AirplaneWeightChecker = 1 Or AirplaneWeightChecker = 4) Then
                            MinimumThickness = 3
                        Else
                            MinimumThickness = 4
                        End If
                    Else
                        MinimumThickness = 2
                    End If
                ElseIf PavementType = "PCC Overlay on Flexible" Then
                    MinimumThickness = 3
                ElseIf PavementType = "HMA Overlay on Flexible" Then
                    MinimumThickness = 2
                Else
                    MinimumThickness = 2
                End If

                '2. P-401/P-403 HMA Stabilized or P-304 Cement Treated Base or P-306 Lean Concrete
            ElseIf SelectedMaterial.Name = "P-401/P-403 HMA Stabilized" Or SelectedMaterial.Name = "P-304 Cement Treated Base" Or SelectedMaterial.Name = "P-306 Lean Concrete" Then
                If PavementType = "New Flexible" Or PavementType = "HMA on Aggregate" Then
                    If DesignType = 0 Or DesignType = 2 Then
                        MinimumThickness = 5
                    Else
                        MinimumThickness = 2
                    End If
                ElseIf PavementType = "HMA Overlay on Flexible" Then
                    MinimumThickness = 2
                ElseIf PavementType = "HMA Overlay on Rigid" Or PavementType = "PCC Overlay on Flexible" Or PavementType = "Unbonded PCC Overlay on Rigid" Then
                    MinimumThickness = 3
                ElseIf PavementType = "New Rigid" Then
                    MinimumThickness = 5
                Else
                    MinimumThickness = 2
                End If

                '3. P-209 Crushed Aggregate or P-211 Lime Rock Or P-154 Uncrushed Aggregate
            ElseIf SelectedMaterial.Name = "P-209 Crushed Aggregate" Or SelectedMaterial.Name = "P-211 Lime Rock" Or SelectedMaterial.Name = "P-154 Uncrushed Aggregate" Then
                If PavementType = "New Flexible" Or PavementType = "HMA on Aggregate" Or PavementType = "New Rigid" Then
                    If DesignType = 0 Or DesignType = 2 Then
                        MinimumThickness = 6
                    Else
                        MinimumThickness = 4
                    End If
                Else
                    MinimumThickness = 4
                End If

                '4. P-208 Crushed Aggregate Or P-219 recycled Concrete Aggregate 
            ElseIf SelectedMaterial.Name = "P-208 Crushed Aggregate" Or SelectedMaterial.Name = "P-219 Recycled Concrete Aggregate" Then
                If PavementType = "New Flexible" Or PavementType = "HMA on Aggregate" Or PavementType = "New Rigid" Then
                    If DesignType = 0 Or DesignType = 2 Then
                        MinimumThickness = 6
                    Else
                        MinimumThickness = 4
                    End If
                Else
                    MinimumThickness = 4
                End If

                '5. P401/P403 HMA Overlay
            ElseIf SelectedMaterial.Name = "P-401/P-403 HMA Overlay" Then
                MinimumThickness = 2

                '6. P-501 PCC Overlay on Flexible 
            ElseIf SelectedMaterial.Name = "P-501 PCC Overlay on Flexible" Then
                If PavementType = "PCC Overlay on Flexible" Then
                    If DesignType = 0 Or DesignType = 2 Then
                        If AirplaneWeightChecker = 4 Then
                            MinimumThickness = 5
                        Else
                            MinimumThickness = 6
                        End If
                    Else
                        MinimumThickness = 5
                    End If
                Else
                    MinimumThickness = 5
                End If

                '7. P-501 PCC Overlay (unbonded)
            ElseIf SelectedMaterial.Name = "P-501 PCC Overlay (unbonded)" Then
                If PavementType = "Unbonded PCC Overlay on Rigid" Then
                    If DesignType = 0 Or DesignType = 2 Then
                        If AirplaneWeightChecker = 4 Then
                            MinimumThickness = 5
                        Else
                            MinimumThickness = 6
                        End If
                    Else
                        MinimumThickness = 5
                    End If
                Else
                    MinimumThickness = 5
                End If

                '8. P-501 PCC Surface
            ElseIf SelectedMaterial.Name = "P-501 PCC Surface" Then
                If PavementType = "New Rigid" Then
                    If AirplaneWeightChecker = 4 Then
                        MinimumThickness = 5
                    Else
                        MinimumThickness = 6
                    End If
                Else
                    MinimumThickness = 5
                End If

                '9. Variable (flexible)
            ElseIf SelectedMaterial.Name = "Variable (flexible)" Then
                If PavementType = "HMA Overlay on Flexible" Then
                    MinimumThickness = 2
                Else
                    MinimumThickness = 2
                End If

                '10. Variable (rigid)
            ElseIf SelectedMaterial.Name = "Variable (rigid)" Then
                MinimumThickness = 5

                '11. P-301 Soil Cement Base
            ElseIf SelectedMaterial.Name = "P-301 Soil Cement Base" Then
                MinimumThickness = 4

                '12. User Defined
            ElseIf SelectedMaterial.Name = "User Defined" Then
                MinimumThickness = 2

            End If

            If SelectedMaterial.Thickness.UsCustomary < MinimumThickness Then
                If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                    msg = "The thickness of " + SelectedMaterial.Name + " cannot be less than " + MinimumThickness.ToString() + " inches"
                Else
                    msg = "The thickness of " + SelectedMaterial.Name + " cannot be less than " + Format(MinimumThickness * 25.4, "N0") + " mm"
                End If
                SelectedMaterial.Thickness = FaarfieldFactory.CreateThickness(MinimumThickness, FaarfieldFactory.CreateUsCustomary())
            End If

            Return msg
        End Function


        'Datagrid changes for Structure
        Public Sub Dg1_LostFocus(e As EventArgs)
            ValidateGridChanges()
        End Sub


        Public Sub Dg1_CellChanged(e As EventArgs)
            ValidateGridChanges()
        End Sub


        Public Sub Dg1_Keyup(e As KeyEventArgs)
            If e.Key = Key.Enter Then
                ValidateGridChanges()
            End If
        End Sub


        'Datagrid change for airplane
        Private Sub ValidateAirplaneGridChanges()
            Dim msg As String = ""
            If SelectedAirplane Is Nothing Then
                Exit Sub
            End If

            Dim value = SelectedAirplane.NumberDepartures
            If value > 500000 Then
                MessageBox.Show("Maximum allowable number for Annual Departure is 500,000. ")
                SelectedAirplane.NumberDepartures = 1200
            ElseIf value < 1 And CurrentSectionView.Section.SelectedRun = 2 Then
                MessageBox.Show("Minimum allowable value of Annual Departures for Life/Compaction run is 1")
                SelectedAirplane.NumberDepartures = 1
            ElseIf value < 0 Then
                msg = "Minimum allowable value for Annual Departures is 0"
                SelectedAirplane.NumberDepartures = 0
                SelectedAirplane.TotalDepartures = 0
            Else
                SelectedAirplane.NumberDepartures = value
            End If


            If Not msg = "" Then
                MessageBox.Show(msg)
            End If
        End Sub


        Public Sub Dg2_LostFocus(e As EventArgs)
            ValidateAirplaneGridChanges()
        End Sub


        Public Sub Dg2_CellChanged(e As EventArgs)
            ValidateAirplaneGridChanges()
        End Sub


        Public Sub Dg2_Keyup(e As KeyEventArgs)
            If e.Key = Key.Enter Then
                ValidateAirplaneGridChanges()
            End If
        End Sub


        Public Sub DesignLife_LostFocus(e As RoutedEventArgs)
            Dim DesignLifeNum As Integer = 0
            If Decimal.TryParse(e.Source.Text, DesignLifeNum) Then
                If DesignLifeNum > 50 Then
                    MessageBox.Show("Pavement life can not be outside the range of 1 to 50 years. " + vbNewLine + "Life would be set to the default of 20 years.")
                    e.Source.Text = 20
                ElseIf DesignLifeNum < 1 Then
                    MessageBox.Show("Pavement life can not be outside the range of 1 to 50 years. " + vbNewLine + "Life would be set to the default of 20 years.")
                    e.Source.Text = 20

                End If
            End If
        End Sub


        Public Sub Timer_Tick(sender As Object, e As EventArgs)
            StopwatchText = StopwatchText + 1
        End Sub


        Public Sub OnGearMouseMove(e As MouseEventArgs)
            Dim CX As Single
            Dim CY As Single

            Dim MXC As Single
            Dim MYC As Single

            Dim InchToMM As Single = 25.4

            Dim mouseposition = e.GetPosition(e.Source)
            CX = mouseposition.X
            CY = mouseposition.Y

            Dim Ameasurementsystem As IMeasurmentSystem = CurrentJob.DesignOptions.MeasurementSystem

            If SelectedTabIndex = 1 Then
                XWCoordinate5Visibility = Visibility.Hidden
                XWCoordinate6Visibility = Visibility.Hidden
                If SelectedAirplane IsNot Nothing Then

                    Dim scaleGear As Single

                    If SelectedAirplane.Name.Contains("B-52") Then
                        scaleGear = 650 / 244
                    ElseIf SelectedAirplane.Name.Contains("A380") Then
                        scaleGear = 300 / 244
                    Else
                        scaleGear = 250 / 244
                    End If

                    If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                        MXC = (CX - 260) * scaleGear
                        MYC = (260 - CY) * scaleGear

                        If CX >= 10 And CX <= 510 Then
                            If CY >= 10 And CY <= 510 Then
                                XWCoordinate5Visibility = Visibility.Visible
                                XWCoordinate6Visibility = Visibility.Visible
                                XWCoordinate5Text = "X= " & Format(MXC, "0")
                                XWCoordinate6Text = "Y= " & Format(MYC, "0")
                            End If
                        End If
                        XWCoordinate1Visibility = Visibility.Hidden
                        XWCoordinate2Visibility = Visibility.Hidden
                        XWCoordinate3Visibility = Visibility.Hidden
                        XWCoordinate4Visibility = Visibility.Hidden
                        XWCoordinate7Visibility = Visibility.Hidden
                        XWCoordinate8Visibility = Visibility.Hidden
                    Else

                        MXC = (CX - 260) * scaleGear * InchToMM
                        MYC = (260 - CY) * scaleGear * InchToMM

                        If CX >= 10 And CX <= 510 Then
                            If CY >= 10 And CY <= 510 Then
                                XWCoordinate5Visibility = Visibility.Visible
                                XWCoordinate6Visibility = Visibility.Visible
                                XWCoordinate5Text = "X= " & Format(MXC, "0")
                                XWCoordinate6Text = "Y= " & Format(MYC, "0")
                            End If
                        End If
                        XWCoordinate1Visibility = Visibility.Hidden
                        XWCoordinate2Visibility = Visibility.Hidden
                        XWCoordinate3Visibility = Visibility.Hidden
                        XWCoordinate4Visibility = Visibility.Hidden
                        XWCoordinate7Visibility = Visibility.Hidden
                        XWCoordinate8Visibility = Visibility.Hidden
                    End If

                    If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                        For Each wheelCoordinate In SelectedAirplane.WheelCoordinates
                            If (MXC >= wheelCoordinate.X.UsCustomary - 5 And MXC <= wheelCoordinate.X.UsCustomary + 10) Or (MXC >= -wheelCoordinate.X.UsCustomary - 5 And MXC <= -wheelCoordinate.X.UsCustomary + 10) Then
                                If (MYC >= wheelCoordinate.Y.UsCustomary - 5 And MYC <= wheelCoordinate.Y.UsCustomary + 10) Then

                                    If (MXC >= 0) Or (wheelCoordinate.X.UsCustomary = 0 And wheelCoordinate.Y.UsCustomary = 0) Then
                                        XWCoordinate3Visibility = Visibility.Visible

                                        XWCoordinate3Text = "Wheel Coordinates:" & vbNewLine & "X= " & Format(Math.Abs(wheelCoordinate.X.UsCustomary), "0.0") & vbNewLine & "Y= " & Format(wheelCoordinate.Y.UsCustomary, "0.0")

                                    ElseIf MXC <= 0 Then
                                        XWCoordinate1Visibility = Visibility.Visible

                                        XWCoordinate1Text = "Wheel Coordinates:" & vbNewLine & "X= " & Format(Math.Abs(wheelCoordinate.X.UsCustomary) * -1, "0.0") & vbNewLine & "Y= " & Format(wheelCoordinate.Y.UsCustomary, "0.0")

                                    Else
                                        XWCoordinate1Visibility = Visibility.Hidden
                                    End If

                                End If
                            End If
                        Next
                    Else
                        For Each wheelCoordinate In SelectedAirplane.WheelCoordinates
                            If (MXC >= wheelCoordinate.X.UsCustomary * InchToMM - 100 And MXC <= wheelCoordinate.X.UsCustomary * InchToMM + 100) Or (MXC >= -wheelCoordinate.X.UsCustomary * InchToMM - 100 And MXC <= -wheelCoordinate.X.UsCustomary * InchToMM + 100) Then
                                If (MYC >= wheelCoordinate.Y.UsCustomary * InchToMM - 200 And MYC <= wheelCoordinate.Y.UsCustomary * InchToMM + 200) Then

                                    If (MXC * InchToMM >= 0) Or (wheelCoordinate.X.UsCustomary * InchToMM = 0 And wheelCoordinate.Y.UsCustomary * InchToMM = 0) Then
                                        XWCoordinate3Visibility = Visibility.Visible

                                        XWCoordinate3Text = "Wheel Coordinates:" & vbNewLine & "X= " & Format(Math.Abs(wheelCoordinate.X.Metric), "0.0") & vbNewLine & "Y= " & Format(wheelCoordinate.Y.Metric, "0.0")

                                    ElseIf MXC * InchToMM <= 0 Then
                                        XWCoordinate1Visibility = Visibility.Visible

                                        XWCoordinate1Text = "Wheel Coordinates:" & vbNewLine & "X= " & Format(Math.Abs(wheelCoordinate.X.Metric) * -1, "0.0") & vbNewLine & "Y= " & Format(wheelCoordinate.Y.Metric, "0.0")

                                    Else
                                        XWCoordinate1Visibility = Visibility.Hidden
                                    End If

                                End If
                            End If
                        Next
                    End If
                End If
            End If

        End Sub

#End Region


#Region "FaarFieldViewModel Properties and Functions"
        Private hu As New HtmlUtils
        Public Property tempPictureBox As New Forms.PictureBox()
        Public Property RunAnalysis As RunAnalysis

        Public Property Jobs As ObservableCollection(Of ITreeViewItemViewModel)
        Public Property AirplaneGroup As ObservableCollection(Of IListBoxItemViewModel)
        Public Property AirplaneByManufacturer As ObservableCollection(Of IListBoxItemViewModel)
        Public Property AircraftLibrary As List(Of IAirplaneInfo)
        Public Property UDALibrary As List(Of AirplaneInfo)
        Public Property FullAircraftLibrary As List(Of IAirplaneInfo)
        Public ReadOnly Property TrafficLibrary As ObservableCollection(Of String)
        Public Property MaterialLibrary As ObservableCollection(Of ITreeViewItemViewModel)

        ' Aircraft filter text — filters AirplaneByManufacturer list live
        Private _aircraftFilterText As String = ""
        Public Property AircraftFilterText As String
            Get
                Return _aircraftFilterText
            End Get
            Set(value As String)
                If _aircraftFilterText <> value Then
                    _aircraftFilterText = value
                    OnPropertyChanged(NameOf(AircraftFilterText))
                    ApplyAircraftFilter()
                End If
            End Set
        End Property

        ' Filtered aircraft collection view
        Private _allAirplaneByManufacturer As New List(Of IListBoxItemViewModel)

        ' Material filter text — filters MaterialLibrary tree items live
        Private _materialFilterText As String = ""
        Public Property MaterialFilterText As String
            Get
                Return _materialFilterText
            End Get
            Set(value As String)
                If _materialFilterText <> value Then
                    _materialFilterText = value
                    OnPropertyChanged(NameOf(MaterialFilterText))
                    ApplyMaterialFilter()
                End If
            End Set
        End Property

        Private Sub ApplyMaterialFilter()
            If MaterialLibrary Is Nothing Then Return
            Dim filter = If(String.IsNullOrWhiteSpace(_materialFilterText), "", _materialFilterText.ToLower())
            For Each category As ITreeViewItemViewModel In MaterialLibrary
                Dim anyChildVisible = False
                If category.Children IsNot Nothing Then
                    For Each child As ITreeViewItemViewModel In category.Children
                        If String.IsNullOrEmpty(filter) OrElse
                           (child.Name IsNot Nothing AndAlso child.Name.ToLower().Contains(filter)) Then
                            child.IsExpanded = child.IsExpanded ' keep state
                            anyChildVisible = True
                        End If
                    Next
                End If
                ' Expand categories with matches when filtering
                If Not String.IsNullOrEmpty(filter) AndAlso anyChildVisible Then
                    category.IsExpanded = True
                End If
            Next
        End Sub

        Private Sub ApplyAircraftFilter()
            If String.IsNullOrWhiteSpace(_aircraftFilterText) Then
                ' Restore all items
                AirplaneByManufacturer.Clear()
                For Each item In _allAirplaneByManufacturer
                    AirplaneByManufacturer.Add(item)
                Next
            Else
                Dim filter = _aircraftFilterText.ToLower()
                AirplaneByManufacturer.Clear()
                For Each item In _allAirplaneByManufacturer
                    Dim acVm = TryCast(item, AirplaneByManufacturerViewModel)
                    If acVm IsNot Nothing AndAlso acVm.Name IsNot Nothing AndAlso acVm.Name.ToLower().Contains(filter) Then
                        AirplaneByManufacturer.Add(item)
                    End If
                Next
            End If
        End Sub
        Public ReadOnly Property ListAnalysis As ObservableCollection(Of IAnalysisType)
        Public ReadOnly Property ListUnits As ObservableCollection(Of IMeasurmentSystem)
        Public Property ListExternalAircraft As ObservableCollection(Of AirplaneInfo)
        Public Property StoreThickness As Thickness
        Public Property StoreRupture As Modulus
        Public Property StoreModulus As Modulus
        Public Property StoreCBR As Double
        Public Property StoreKvalue As SubgradeReaction

        Public Property AvailableDatabases As ObservableCollection(Of IDatabase)
        Public Property AvailableNetworks As ObservableCollection(Of INetwork)
        Public Property AvailableBranches As ObservableCollection(Of IBranch)
        Public Property AvailableSections As ObservableCollection(Of IPaveairSection)
        Public Property JobsByPaveAirSection As ObservableCollection(Of IJobDownload)


        Private _selectedJobDownload As JobDownload
        Public Property SelectedJobDownload As JobDownload
            Get
                Return _selectedJobDownload
            End Get
            Set(ByVal value As JobDownload)
                _selectedJobDownload = value
            End Set
        End Property


        Private _CurrentJob As IFaarFieldJob
        Public Property CurrentJob As IFaarFieldJob
            Get
                Return _CurrentJob
            End Get
            Set(value As IFaarFieldJob)
                _CurrentJob = value
                OnPropertyChanged(NameOf(CurrentJob))
            End Set
        End Property


        Private _CurrentJobView As JobViewModel
        Public Property CurrentJobView As JobViewModel
            Get
                Return _CurrentJobView
            End Get
            Set(value As JobViewModel)
                _CurrentJobView = value
                OnPropertyChanged(NameOf(CurrentJobView))
            End Set
        End Property


        Private _CurrentAirplanes As ObservableCollection(Of AirplaneInfo)
        Public Property CurrentAirplanes As ObservableCollection(Of AirplaneInfo)
            Get
                Return _CurrentAirplanes
            End Get
            Set(value As ObservableCollection(Of AirplaneInfo))
                _CurrentAirplanes = value
                OnPropertyChanged(NameOf(CurrentAirplanes))
            End Set
        End Property


        Public Property check As SectionViewModel
        Public Property selectedaricraft As IAirplaneInfo
        Public Property ReplacedLayer As Material

        Private Property PartiallyBondedMaterial As MaterialViewModel
        Private Property CategoryP501 As MaterialCategoryViewModel
        Property K As Integer
        Property airplainselector As IAirplaneInfo
        Property MaterialReplace2 As Boolean = True
        Public Property _DLBackground As Color
        Property pdfcreatorchecker As Boolean = True
        Dim _ThicknessMenuPlace As Point


        Public Property DLBackground As Color
            Get
                Return _DLBackground
            End Get
            Set(value As Color)
                _DLBackground = value
                OnPropertyChanged(NameOf(DLBackground))
            End Set
        End Property


        Public Property CurrentLayers As ObservableCollection(Of IMaterial)
            Get
                Return _currentlayers
            End Get
            Set(value As ObservableCollection(Of IMaterial))
                _currentlayers = value
                OnPropertyChanged(NameOf(CurrentLayers))
            End Set
        End Property


        Private Property _CurrentWheel As New ObservableCollection(Of UserDefinedWheel)
        Public Property CurrentWheel As ObservableCollection(Of UserDefinedWheel)
            Get
                Return _CurrentWheel
            End Get
            Set(value As ObservableCollection(Of UserDefinedWheel))
                If value IsNot Nothing Then
                    _CurrentWheel = value
                    OnPropertyChanged(NameOf(CurrentWheel))
                    UserDefinedGearImage = DrawUserDefinedGear()
                End If
            End Set
        End Property


        Private Property _SelectedWheel As New UserDefinedWheel
        Public Property SelectedWheel As UserDefinedWheel
            Get
                Return _SelectedWheel
            End Get
            Set(value As UserDefinedWheel)
                If value IsNot Nothing Then
                    If value.TireX Is Nothing AndAlso value.TireY Is Nothing Then
                        value.TireX = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                        value.TireY = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                        value.ContactArea = 0
                        value.id = CurrentWheel.Count - 1
                    End If

                    _SelectedWheel = value
                    OnPropertyChanged(NameOf(SelectedWheel))
                    UserDefinedGearImage = DrawUserDefinedGear()
                End If
            End Set
        End Property


        Private Property _SelectedEval As New UserDefinedEvalPoints
        Public Property SelectedEval As UserDefinedEvalPoints
            Get
                Return _SelectedEval
            End Get
            Set(value As UserDefinedEvalPoints)
                If Not value Is Nothing Then
                    If value.EvalX Is Nothing AndAlso value.EvalY Is Nothing Then
                        value.EvalX = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                        value.EvalY = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                        value.ContactArea = 0
                        value.id = CurrentWheel.Count - 1
                    End If

                    _SelectedEval = value
                    OnPropertyChanged(NameOf(SelectedEval))
                    UserDefinedGearImage = DrawUserDefinedGear()
                End If
            End Set
        End Property


        Private Property _CurrentEval As New ObservableCollection(Of UserDefinedEvalPoints)
        Public Property CurrentEval As ObservableCollection(Of UserDefinedEvalPoints)
            Get
                Return _CurrentEval
            End Get
            Set(value As ObservableCollection(Of UserDefinedEvalPoints))
                If value IsNot Nothing Then
                    _CurrentEval = value
                    OnPropertyChanged(NameOf(CurrentEval))
                    UserDefinedGearImage = DrawUserDefinedGear()
                End If
            End Set
        End Property


        Public Property _CurrentUserDefinedAircraft As AirplaneInfo
        Public Property CurrentUserDefinedAircraft As AirplaneInfo
            Get
                Return _CurrentUserDefinedAircraft
            End Get
            Set(value As AirplaneInfo)
                _CurrentUserDefinedAircraft = value

                _CurrentWheel.Clear()
                _CurrentEval.Clear()

                Dim tmpWheels As New ObservableCollection(Of UserDefinedWheel)

                For i = 0 To CurrentUserDefinedAircraft.WheelCoordinates.Count - 1
                    Dim udWheel As New UserDefinedWheel
                    udWheel.TireX = FaarFieldFactory.CreateThickness(Math.Abs(CurrentUserDefinedAircraft.WheelCoordinates.Item(i).X.GetValue(FaarFieldFactory.CreateUsCustomary)) * -1, FaarFieldFactory.CreateUsCustomary)
                    udWheel.TireY = FaarFieldFactory.CreateThickness(CurrentUserDefinedAircraft.WheelCoordinates.Item(i).Y.GetValue(FaarFieldFactory.CreateUsCustomary), FaarFieldFactory.CreateUsCustomary)
                    tmpWheels.Add(udWheel)
                Next

                For i = 0 To tmpWheels.Count - 1
                    Dim exists As Boolean = False

                    For j = i + 1 To tmpWheels.Count - 1
                        If (tmpWheels(i).TireX.UsCustomary = tmpWheels(j).TireX.UsCustomary AndAlso tmpWheels(i).TireY.UsCustomary = tmpWheels(j).TireY.UsCustomary) OrElse (tmpWheels(i).TireX.UsCustomary = tmpWheels(j).TireX.UsCustomary * -1 AndAlso tmpWheels(i).TireY.UsCustomary = tmpWheels(j).TireY.UsCustomary) Then
                            exists = True
                        End If
                    Next

                    If Not exists Then
                        _CurrentWheel.Add(tmpWheels(i))
                    End If

                Next


                For i = 0 To CurrentUserDefinedAircraft.EvaluationPoints.Count - 1
                    Dim EvalPoint As New UserDefinedEvalPoints
                    EvalPoint.id = i + 1
                    EvalPoint.EvalX = FaarFieldFactory.CreateThickness(CurrentUserDefinedAircraft.EvaluationPoints.Item(i).X.GetValue(FaarFieldFactory.CreateUsCustomary), FaarFieldFactory.CreateUsCustomary)
                    EvalPoint.EvalY = FaarFieldFactory.CreateThickness(CurrentUserDefinedAircraft.EvaluationPoints.Item(i).Y.GetValue(FaarFieldFactory.CreateUsCustomary), FaarFieldFactory.CreateUsCustomary)
                    _CurrentEval.Add(EvalPoint)
                Next


                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDGrossWeight = FaarFieldFactory.CreateWeight(_CurrentUserDefinedAircraft.GrossWeight.UsCustomary, FaarFieldFactory.CreateUsCustomary)
                End If
                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDMGPercent = _CurrentUserDefinedAircraft.MgPercent
                End If
                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    If _CurrentUserDefinedAircraft.MgPercentPCN <= 0.5 Then
                        _UDMGPercentPCN = _CurrentUserDefinedAircraft.MgPercentPCN * 2
                    Else
                        _UDMGPercentPCN = _CurrentUserDefinedAircraft.MgPercentPCN
                    End If
                End If

                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDTirePressure = _CurrentUserDefinedAircraft.Cp
                End If

                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    UDGearOrientation = _CurrentUserDefinedAircraft.GearOrientation
                End If

                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDGeartype = _CurrentUserDefinedAircraft.Gear
                End If
                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDGearNumber = _CurrentUserDefinedAircraft.NumberGear
                End If
                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDNumberOfWheels = _CurrentUserDefinedAircraft.NumberWheels ' / 2

                End If
                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDNumberOfEvalPoints = _CurrentUserDefinedAircraft.EvaluationPoints.Count
                End If
                If Not _CurrentUserDefinedAircraft Is Nothing Then
                    _UDTs = _CurrentUserDefinedAircraft.Ts
                End If

                OnPropertyChanged(NameOf(CurrentUserDefinedAircraft))
                OnPropertyChanged(NameOf(CurrentWheel))
                OnPropertyChanged(NameOf(CurrentEval))
                OnPropertyChanged(NameOf(UDGrossWeight))
                OnPropertyChanged(NameOf(UDMGPercent))
                OnPropertyChanged(NameOf(UDMGPercentPCN))
                OnPropertyChanged(NameOf(UDTirePressure))
                OnPropertyChanged(NameOf(UDGearOrientation))
                OnPropertyChanged(NameOf(UDGearType))
                OnPropertyChanged(NameOf(UDGearNumber))
                OnPropertyChanged(NameOf(UDNumberOfWheels))
                OnPropertyChanged(NameOf(UDNumberOfEvalPoints))
                UserDefinedGearImage = DrawUserDefinedGear()
                OnPropertyChanged(NameOf(UDTs))
            End Set
        End Property


        Public Sub MirrorUDAWheels(ByRef airplane As AirplaneInfo)
            Dim coordinateSystem As IMeasurmentSystem = FaarFieldFactory.CreateUsCustomary()

            Dim tmpWheels As New List(Of UserDefinedWheel)
            Dim tmpWheelsCoords As New List(Of UserDefinedWheel)
            Dim index As Integer
            Dim index2 As Integer
            Dim numWheels As Integer = 0

            For index = 0 To airplane.WheelCoordinates.Count - 1
                Dim udWheel As New UserDefinedWheel With {
                    .TireX = CType(FaarFieldFactory.CreateThickness(airplane.WheelCoordinates.Item(index).X.GetValue(coordinateSystem), coordinateSystem), Thickness),
                    .TireY = CType(FaarFieldFactory.CreateThickness(airplane.WheelCoordinates.Item(index).Y.GetValue(coordinateSystem), coordinateSystem), Thickness)
                }
                tmpWheels.Add(udWheel)
            Next

            airplane.WheelCoordinates.Clear()

            For index = 0 To tmpWheels.Count - 1
                Dim exists As Boolean = False

                For index2 = index + 1 To tmpWheels.Count - 1
                    If (tmpWheels(index).TireX.UsCustomary = tmpWheels(index2).TireX.UsCustomary AndAlso tmpWheels(index).TireY.UsCustomary = tmpWheels(index2).TireY.UsCustomary) OrElse (tmpWheels(index).TireX.UsCustomary = tmpWheels(index2).TireX.UsCustomary * -1 AndAlso tmpWheels(index).TireY.UsCustomary = tmpWheels(index2).TireY.UsCustomary) Then
                        exists = True
                    End If
                Next

                If Not exists Then
                    If tmpWheels(index).TireX.UsCustomary = 0 Then
                        airplane.WheelCoordinates.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpWheels.Item(index).TireX.UsCustomary), coordinateSystem), FaarFieldFactory.CreateLength(tmpWheels.Item(index).TireY.UsCustomary, coordinateSystem)))
                    Else
                        airplane.WheelCoordinates.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpWheels.Item(index).TireX.UsCustomary) * -1, coordinateSystem), FaarFieldFactory.CreateLength(tmpWheels.Item(index).TireY.UsCustomary, coordinateSystem)))
                        airplane.WheelCoordinates.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpWheels.Item(index).TireX.UsCustomary), coordinateSystem), FaarFieldFactory.CreateLength(tmpWheels.Item(index).TireY.UsCustomary, coordinateSystem)))
                    End If
                    numWheels += 1
                End If

            Next

            airplane.WheelCoordinates = airplane.WheelCoordinates.OrderBy(Function(x) x.X.UsCustomary).ToList()
            airplane.NumberWheels = numWheels
            airplane.TireTrackX = New List(Of IDimensionalProperty)

            For index = 0 To airplane.WheelCoordinates.Count - 1
                Dim exists As Boolean = False
                For index2 = index + 1 To airplane.WheelCoordinates.Count - 1
                    If (airplane.WheelCoordinates(index).X.UsCustomary = airplane.WheelCoordinates(index2).X.UsCustomary) Then
                        exists = True
                    End If
                Next

                If Not exists Then
                    airplane.TireTrackX.Add(FaarFieldFactory.CreateLength(airplane.WheelCoordinates.Item(index).X.UsCustomary, coordinateSystem))
                End If

            Next

            airplane.TireTrackX = airplane.TireTrackX.OrderBy(Function(x) x.UsCustomary).ToList()


            Dim tmpEvals As New ObservableCollection(Of UserDefinedEvalPoints)

            For i = 0 To airplane.EvaluationPoints.Count - 1
                Dim udEval As New UserDefinedEvalPoints With {
                    .EvalX = FaarFieldFactory.CreateThickness(airplane.EvaluationPoints.Item(i).X.GetValue(coordinateSystem), coordinateSystem),
                    .EvalY = FaarFieldFactory.CreateThickness(airplane.EvaluationPoints.Item(i).Y.GetValue(coordinateSystem), coordinateSystem)
                }
                tmpEvals.Add(udEval)
            Next

            airplane.EvaluationPoints.Clear()

            For i = 0 To tmpEvals.Count - 1
                Dim exists As Boolean = False

                For j = i + 1 To tmpEvals.Count - 1
                    If (tmpEvals(i).EvalX.UsCustomary = tmpEvals(j).EvalX.UsCustomary AndAlso tmpEvals(i).EvalY.UsCustomary = tmpEvals(j).EvalY.UsCustomary) OrElse (tmpEvals(i).EvalX.UsCustomary = tmpEvals(j).EvalX.UsCustomary * -1 AndAlso tmpEvals(i).EvalY.UsCustomary = tmpEvals(j).EvalY.UsCustomary) Then
                        exists = True
                    End If
                Next

                If Not exists Then
                    airplane.EvaluationPoints.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpEvals.Item(i).EvalX.UsCustomary) * -1, coordinateSystem), FaarFieldFactory.CreateLength(tmpEvals.Item(i).EvalY.UsCustomary, coordinateSystem)))
                End If

            Next
        End Sub


        Public Property UDGrossWeight As Weight
            Get
                Return _UDGrossWeight
            End Get
            Set(value As Weight)
                If Not value Is Nothing Then
                    If value.UsCustomary > 0 Then
                        _UDGrossWeight = value
                        OnPropertyChanged(NameOf(UDGrossWeight))
                    Else
                        MessageBox.Show("Aircraft Gross Weight has to be a positive number")
                    End If
                Else
                    MessageBox.Show("Aircraft Gross Weight has to be a positive number")
                End If
            End Set
        End Property


        Public Property UDMGPercent As Single
            Get
                Return _UDMGPercent
            End Get
            Set(value As Single)
                If value > 1 Or value < 0 Then
                    MessageBox.Show("Percent Gross weight on wheel should be a number between 0 and 1")
                Else
                    _UDMGPercent = value
                    OnPropertyChanged(NameOf(UDMGPercent))
                End If
            End Set
        End Property


        Public Property UDMGPercentPCN As Single
            Get
                Return _UDMGPercentPCN
            End Get
            Set(value As Single)
                If value > 1 Or value < 0 Then
                    MessageBox.Show("Percent Gross wight on wheel should be a number between 0 and 1")
                Else
                    _UDMGPercentPCN = value
                    OnPropertyChanged(NameOf(UDMGPercentPCN))
                End If
            End Set
        End Property


        Public Property UDTirePressure As Pressure
            Get
                Return _UDTirePressure
            End Get
            Set(value As Pressure)
                If value.UsCustomary < 0 Then
                    MessageBox.Show("Tire Pressure has to be a positive number")
                ElseIf value.UsCustomary > 350 Then
                    MessageBox.Show("Tire Pressure cannot be higher than 350 psi (2,413.17 kPa).")
                Else
                    _UDTirePressure = value
                    OnPropertyChanged(NameOf(UDTirePressure))
                End If
            End Set
        End Property


        Private _UDGearOrientation As Integer
        Public Property UDGearOrientation As Integer
            Get
                Return _UDGearOrientation
            End Get
            Set(value As Integer)
                If value = 0 Or value = 90 Then
                    _UDGearOrientation = value
                    OnPropertyChanged(NameOf(UDGearOrientation))
                Else
                    MessageBox.Show("Gear Orientation can only be 0 or 90")
                End If
            End Set
        End Property


        Public Property UDGearType As String
            Get
                Return _UDGeartype
            End Get
            Set(value As String)
                _UDGeartype = "V"
                OnPropertyChanged(NameOf(UDGearType))
            End Set
        End Property


        Public Property UDGearNumber As String
            Get
                Return _UDGearNumber
            End Get
            Set(value As String)
                _UDGearNumber = 13
                OnPropertyChanged(NameOf(UDGearNumber))
            End Set
        End Property


        Public Property UDNumberOfWheels As Integer
            Get
                Return _UDNumberOfWheels
            End Get
            Set(value As Integer)

                If Not value <= 0 Then
                    _UDNumberOfWheels = value
                    OnPropertyChanged(NameOf(UDNumberOfWheels))
                Else
                    MessageBox.Show("Number of wheels should be a positive number")
                End If
            End Set
        End Property


        Public Property UDNumberOfEvalPoints As Integer
            Get
                Return _UDNumberOfEvalPoints
            End Get
            Set(value As Integer)

                If Not value <= 0 Then
                    _UDNumberOfEvalPoints = value
                    OnPropertyChanged(NameOf(UDNumberOfEvalPoints))
                Else
                    MessageBox.Show("Number of wheels should be a positive number")
                End If
            End Set
        End Property


        Public Property UDName As String
            Get
                Return _UDName
            End Get
            Set(value As String)
                If Not value Is Nothing Then
                    _UDName = value
                    OnPropertyChanged(NameOf(UDName))
                End If
            End Set
        End Property


        Public Property UDTs As Single
            Get
                Return _UDTs
            End Get
            Set(value As Single)
                _UDTs = value
                OnPropertyChanged(NameOf(UDTs))
            End Set
        End Property


        Public Property VisibleForEdit As Visibility
            Get
                Return _VisibleForEdit
            End Get
            Set(value As Visibility)
                _VisibleForEdit = value
                OnPropertyChanged(NameOf(VisibleForEdit))
            End Set
        End Property


        Public Property VisibleForCreate As Visibility
            Get
                Return _VisibleForCreate
            End Get
            Set(value As Visibility)
                _VisibleForCreate = value
                OnPropertyChanged(NameOf(VisibleForCreate))
            End Set
        End Property


        Public Sub OnEditUserDefinedAircraft()
            VisibleForEdit = Visibility.Visible
        End Sub


        Public Sub OnCreateUserDefinedAircraft()
            VisibleForCreate = Visibility.Visible
        End Sub


        Dim _UDGrossWeight As Weight
        Dim _UDMGPercent As Single
        Dim _UDTs As Single
        Dim _UDMGPercentPCN As Single
        Dim _UDTirePressure As Pressure
        Dim _UDGeartype As String
        Dim _UDGearNumber As Integer
        Dim _UDNumberOfWheels As Integer
        Dim _UDNumberOfEvalPoints As Integer
        Dim _UDName As String

        Public Property _VisibleForEdit As Visibility = Visibility.Hidden
        Public Property _VisibleForCreate As Visibility = Visibility.Hidden


        Private _CurrentSectionView As SectionViewModel
        Public Property CurrentSectionView As SectionViewModel
            Get
                Return _CurrentSectionView
            End Get
            Set(value As SectionViewModel)
                _CurrentSectionView = value
                OnPropertyChanged(NameOf(CurrentSectionView))
            End Set
        End Property


        Public Property SavePath As String

        Public Property AircraftbellyLibrary As List(Of IAirplaneInfo)
        Public Property openUDAfolder As Boolean

        Public bAddNewSection As Boolean = False

        Public FaarFieldFactory As IFaarFieldModelFactory
        Public Property _sectionIsHidden As Boolean = True
        Dim _designOptionsIsHidden As Boolean = True
        Dim _VehicleEditIsHidden As Boolean = True
        Dim _layerthicknessmenuIsHidden As Boolean = True
        Dim _modulusmenuIsHidden As Boolean = True
        Dim _jobInformationIsHidden As Boolean = True
        Dim _notesIsHidden As Boolean = True
        Dim _airplanesIsHidden As Boolean = True
        Dim _summaryReportIsHidden As Boolean = True
        Dim _sectionReportIsHidden As Boolean = True
        Dim _PCRReportIsHidden As Boolean = True
        Dim _AirportMasterRecordIsHidden As Boolean = True
        Dim _CDFGraphIsHidden As Boolean = True
        Dim _DetailedReportIsHidden As Boolean = True
        Dim _PCRGraphIsHidden As Boolean = True
        Dim _jobReportIsHidden As Boolean = True
        Dim _explorerIsHidden As Boolean = False
        Dim _vehiclEditIsHidden As Boolean = False

        Dim _runButtonText As String = "Run"
        Dim _runPCN As String = "PCN"
        Dim _messageText As String = ""
        Dim _selectedTabIndex As Integer = 0
        Dim _selectedrun As Integer = 0
        Public Property _analysisType As AnalysisType
        Dim _imagevisibility As Visibility = Visibility.Visible
        Dim _totalthicknessupdate As String
        Dim _AverageFrostLabel As String


        Dim _backgroundUnit As Color
        Dim _USUnitsContent As String

        Dim _DeleteTrafficEnabled As Boolean = False
        Dim _DeleteAircraftMixEnabled As Boolean = False
        Dim _SelectDesignLayerEnabled As Boolean = False
        Dim _summaryReportHtml As String
        Dim _sectionReportHtml As String
        Dim _CDFGraphHtml As String
        Dim _DetailedReportHtml As String
        Dim _PCRReportHtml As String
        Dim _AirportMasterRecordHtml As String
        Dim _PCRGraphHtml As String

        Dim _currentlayers As ObservableCollection(Of IMaterial)
        Dim _clonedSection As FaarFieldModel.Section
        Dim StoreSci As Double
        Dim StoreCdfu As Double
        Dim StorePtoTC As Double
        Dim StoreStress As Double
        Private Property _currentLibrary As String
        Private Property _designLife As Double
        Private Property _Nsection As Double = 16

        Public Property _modulusmenutext As String
        Public Property _thicknessmenutext As String
        Private Property _newthickness As Thickness
        Private Property _newmodulus As Modulus
        Private Property _newRupture As Modulus
        Private Property _newCBR As Double
        Private Property _newKvalue As SubgradeReaction
        Private Property _sci As Double
        Private Property _cdfu As Double
        Private Property _PtoTC As Double = 1

        Private Property _slabEdgeStress As Double = 0
        Property _slabEdgeStressreport As String
        Private Property _slabInteriorStress As Double = 0
        Property _slabInteriorStressreport As String
        Property _criticalAircraftReport As String
        Property _calculatedLife As Double
        Property _calculatedlifereport As String
        Property _aircraftStressLabel As String = "Selected Aircraft:"
        Private Property _subgradeDepth As String
        Private Property _selectedAirplane As IAirplaneInfo
        Public Property _selectedMaterial As IMaterial
        Private Property _structureErrorMessage As String
        Private Property _layername1 As String

        Property LastRowWeight1 As Weight
        Property LastRowWeight2 As Weight
        Property LastRowDeparture1 As Double
        Property LastRowDeparture2 As Double
        Property LastRowGrowth1 As Double
        Property LastRowGrowth2 As Double
        Property rowchecker As Boolean = False

        Public Property Materials As ObservableCollection(Of MaterialDefault)
        Public Property WarningCdfToleranceHidden As Visibility = Visibility.Hidden
        Public Property WarningLifeToleranceHidden As Visibility = Visibility.Hidden
        Public Property WarningSectionParameterNHidden As Visibility = Visibility.Hidden
        Public Property WarningCrackPropogationHidden As Visibility = Visibility.Hidden
        Public Property WarningFrostPenetrationHidden As Visibility = Visibility.Hidden
        Public Property WarningStructureError As Visibility = Visibility.Hidden
        Public Property WarningSciCdfu As Visibility = Visibility.Hidden
        Public Property WarningDesignLife As Visibility = Visibility.Hidden
        Public Property WarningNsection As Visibility = Visibility.Hidden
        Public Property WarningAirplanes As Visibility = Visibility.Hidden
        Public Property WarningSectionName As Visibility = Visibility.Hidden
        Private Property SectionChange As Boolean = True

        Private __brushes As Dictionary(Of String, Brush)
        Public Property _brushes As Dictionary(Of String, Brush)
            Get
                Return __brushes
            End Get
            Set(value As Dictionary(Of String, Brush))
                __brushes = value
                OnPropertyChanged(NameOf(_brushes))
            End Set
        End Property

        Property CalculatedLifeTracker As Boolean = False


        Public Sub UpdateAirplaneByManufacturer(manufacturer As String)
            Dim airplaneInfos = AircraftLibrary.FindAll(Function(x) x.Manufacturer = manufacturer)
            _allAirplaneByManufacturer.Clear()
            AirplaneByManufacturer.Clear()
            For Each airplaneInfo As AirplaneInfo In airplaneInfos
                Dim vm = New AirplaneByManufacturerViewModel(airplaneInfo, Me)
                _allAirplaneByManufacturer.Add(vm)
                AirplaneByManufacturer.Add(vm)
            Next
            ' Re-apply filter if text is set
            If Not String.IsNullOrWhiteSpace(_aircraftFilterText) Then
                ApplyAircraftFilter()
            End If
            OnPropertyChanged(NameOf(AirplaneByManufacturer))

        End Sub


        Public Sub SetCurrentJob(jobView As JobViewModel)
            CurrentJob = jobView.FaarFieldJob
            CurrentJobView = jobView
            ValidateDesignOptions(Nothing)
            ValidateJobInformation(Nothing)
            If CurrentJob.Version_1_4_File Then
                JobViewBorderThickness = 5
            Else
                JobViewBorderThickness = 0
            End If
            If CurrentJob.Sections.Count > 0 Then
                If jobView.SectionFolder.Children.Count > 0 Then
                    K = 1234
                    Call SetCurrentSection(jobView.SectionFolder.Children.Item(0))
                    K = 0
                End If
                SectionRequiredButtonEnabled = True
            Else
                SectionRequiredButtonEnabled = False
                SectionIsHidden = True
                InnerSectionIsHidden = True
                AirplanesIsHidden = True
            End If

            OnPropertyChanged(NameOf(Jobs))
        End Sub


        Private Sub UpdateSectionLayers()
            'Check if CurrentLayers is null.  If it is return
            'Sometimes CurrentSectionView is null.--Ed
            If CurrentSectionView Is Nothing Then
                Return
            End If

            If (Not bAddNewSection) Then
                CurrentSectionView.Section.Layers.Clear()
                If Not CurrentLayers Is Nothing Then

                    For Each layer In CurrentLayers
                        CurrentSectionView.Section.Layers.Add(layer)
                    Next
                End If
            End If
            bAddNewSection = False

        End Sub


        Public Sub SetCurrentSection(sectionViewModel As SectionViewModel)

            UpdateSectionLayers()
            Dim jobview = CType(sectionViewModel.Parent.Parent, JobViewModel)
            If K <> 1234 Then
                SetCurrentJob(jobview)
            End If

            CurrentSectionView = sectionViewModel

            If Not CurrentSectionView.Section.TotalThickness Is Nothing Then
                SubgradeDepth = CurrentSectionView.Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem)
            End If

            SectionIsHidden = True
            InnerSectionIsHidden = True
            AirplanesIsHidden = True

            SectionIsHidden = False
            InnerSectionIsHidden = False
            AirplanesIsHidden = False

            If CurrentSectionView.Section.PCAConversionTracker = True Then
                PCAConversionFormula = True
            Else
                PCAConversionFormula = False
            End If
            If CurrentSectionView.Section.NCHRPTracker = True Then
                NCHRPFormula = True
            Else
                NCHRPFormula = False
            End If

            OnPropertyChanged(NameOf(CurrentLayers))
            OnPropertyChanged(NameOf(CurrentSectionView))

            Try
                CurrentAirplanes.Clear()
                For Each airplane In CurrentSectionView.Section.Airplanes
                    CurrentAirplanes.Add(airplane)
                Next
            Catch ex As Exception
                Debug.WriteLine(ex)
            End Try

            Dim storeLayers As New ObservableCollection(Of IMaterial)
            Dim numberoflayers As Integer = 0
            If Not CurrentSectionView Is Nothing Then
                For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                    storeLayers.Add(CurrentSectionView.Section.Layers.Item(i))
                    numberoflayers += 1
                Next
            End If

            Try
                CurrentLayers.Clear()
                CurrentLayers.Clear()
                CurrentSectionView.Section.Layers.Clear()
                For i = 0 To numberoflayers - 1
                    CurrentSectionView.Section.Layers.Add(storeLayers(i))
                Next

                For Each layer In CurrentSectionView.Section.Layers
                    CurrentLayers.Add(layer)
                Next
            Catch ex As Exception
                Debug.WriteLine(ex)
            End Try

            DesignLife = sectionViewModel.Section.Life
            Sci = sectionViewModel.Section.Sci
            Cdfu = sectionViewModel.Section.PercentCdfu
            SlabEdgeStress = sectionViewModel.Section.SlabEdgeStress
            SlabInteriorStress = sectionViewModel.Section.SlabInteriorStress
            CriticalStressAircraft = sectionViewModel.Section.CriticalStressAicraft
            AircraftStressLabel = "Most Demanding Aircraft: " + sectionViewModel.Section.CriticalStressAicraft

            UpdateAnalysisTypeFromList(sectionViewModel.Section)
            AnalysisType = sectionViewModel.Section.AnalysisType
            OnPropertyChanged(NameOf(AnalysisType))
            Call OnUpdateselectedMaterial()

            If CurrentSectionView.Section.SectionPCRRunTime Is Nothing Then
                CurrentSectionView.Section.SectionPCRRunTime = "None"
            End If
            If CurrentSectionView.Section.SectionDesignrunTime Is Nothing Then
                CurrentSectionView.Section.SectionDesignrunTime = "None"
            End If
            If CurrentSectionView.Section.SectionLifeRunTime Is Nothing Then
                CurrentSectionView.Section.SectionLifeRunTime = "None"
            End If
            If CurrentSectionView.Section.SectionCompactionRunTime Is Nothing Then
                CurrentSectionView.Section.SectionCompactionRunTime = "None"
            End If

            Dim TempL, TempL1 As Single
            TempL1 = DesignLife
            For Each airplane In CurrentSectionView.Section.Airplanes
                Dim ag = airplane.AnnualGrowth / 100
                Dim nd = airplane.NumberDepartures
                If 1.0! + ag * DesignLife < 0.0! Then
                    TempL1 = -1.0! / ag
                End If
                TempL = CSng(1.0! + TempL1 * ag * 0.5)
                airplane.TotalDepartures = TempL * nd * TempL1
                If airplane.DefaultCp Is Nothing Then
                    airplane.DefaultCp = airplane.Cp
                End If

                If airplane.DefaultGrossWeight Is Nothing Then
                    airplane.DefaultGrossWeight = airplane.GrossWeight
                End If

            Next
            If CurrentSectionView.Section IsNot Nothing Then
                If CurrentJob.DesignOptions.ACROptions = True Then
                    _ACRHeader1 = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/A"
                    ACRHeader1 = _ACRHeader1

                    _ACRHeader2 = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/B"
                    ACRHeader2 = _ACRHeader2

                    _ACRHeader3 = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/C"
                    ACRHeader3 = _ACRHeader3

                    _ACRHeader4 = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/D"
                    ACRHeader4 = _ACRHeader4
                End If

                If CurrentSectionView.Section.Layers.Count > 0 Then
                    If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Modulus.UsCustomary >= 21755.66 Then
                        _ACRHeader = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/A"
                        ACRHeader = _ACRHeader
                        ACRThickHeader = " (A)"
                    ElseIf CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Modulus.UsCustomary >= 14503.7738 Then
                        _ACRHeader = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/B"
                        ACRHeader = _ACRHeader
                        ACRThickHeader = " (B)"
                    ElseIf CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Modulus.UsCustomary >= 8702.264 Then
                        _ACRHeader = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/C"
                        ACRHeader = _ACRHeader
                        ACRThickHeader = " (C)"
                    Else
                        _ACRHeader = "ACR/" + CurrentSectionView.Section.SectionPCRPaveType + "/D"
                        ACRHeader = _ACRHeader
                        ACRThickHeader = " (D)"
                    End If

                    UpdateTrafficACRHeaders()
                End If

            End If

            If Not CurrentSectionView.Section.Name Is Nothing AndAlso RunButtonText = "Run" Then
                'Updating the PCR Graph data — BY Ali.D
                FEDFAA1.Save_NAC = CurrentAirplanes.Count

                If CurrentAirplanes.Count = 0 Then
                    For i = 1 To FEDFAA1.gACN_GL.Count - 1
                        FEDFAA1.gACN_GL(i) = 0
                    Next
                Else
                    ReDim FEDFAA1.gACN_GL(CurrentAirplanes.Count)
                    For i = 0 To CurrentAirplanes.Count - 1
                        FEDFAA1.gACN_GL(i + 1) = CurrentAirplanes.Item(i).PCRNumber
                    Next
                    If FEDFAA1.gACN_GL.Count - 1 > CurrentAirplanes.Count Then
                        For i = CurrentAirplanes.Count To FEDFAA1.gACN_GL.Count - 2
                            FEDFAA1.gACN_GL(i + 1) = 0
                        Next
                    End If
                End If
                ' Reports are refreshed on demand by their individual ViewModels
            End If

            OnPropertyChanged(NameOf(MessageText))
            OnPropertyChanged(NameOf(CurrentAirplanes))

            If CurrentSectionView.Section.TrafficMixName = "" Then
                _currentLibrary = Nothing
                OnPropertyChanged(NameOf(CurrentLibrary))
            Else
                _currentLibrary = CurrentSectionView.Section.TrafficMixName
                OnPropertyChanged(NameOf(CurrentLibrary))
            End If

            If CurrentSectionView.Section.AnalysedLife = 0 Then

                _calculatedLife = Nothing
            Else

                _calculatedLife = CurrentSectionView.Section.AnalysedLife

            End If
            OnPropertyChanged(NameOf(CalculatedLife))

            If CurrentSectionView.Section.SlabEdgeStress = 0 And CurrentSectionView.Section.SlabInteriorStress = 0 Then
                _slabEdgeStress = Nothing
                _slabInteriorStress = Nothing
                _criticalStressAircraft = Nothing
            Else
                _slabInteriorStress = CurrentSectionView.Section.SlabInteriorStress
                _slabEdgeStress = CurrentSectionView.Section.SlabEdgeStress
                _criticalStressAircraft = CurrentSectionView.Section.CriticalStressAicraft
                _criticalAircraftReport = CurrentSectionView.Section.CriticalStressAicraft
            End If
            OnPropertyChanged(NameOf(SlabInteriorStress))
            OnPropertyChanged(NameOf(SlabEdgeStress))
            OnPropertyChanged(NameOf(CriticalStressAircraft))
            OnPropertyChanged(NameOf(CriticalStressReport))

            MessageText = CurrentSectionView.Section.SectionRunStatus

            'If last run was not "Thickness Design", "Life/ Compaction" in drop down list will be disabled
            If CurrentSectionView.Section.LastRun = "Thickness Design" Then
                CompactionLifeIsEnabled = True
            Else
                CompactionLifeIsEnabled = False
            End If

            RefreshGWPercent()

        End Sub


        Public Sub SetSelection(sv As SectionViewModel)
            If sv.IsSelected = True Then
                sv.IsSelected = False
            End If
            sv.IsSelected = True
        End Sub


        Public Sub SetSelection(sv As JobViewModel)
            If sv.IsSelected = True Then
                sv.IsSelected = False
            End If
            sv.IsSelected = True
        End Sub


        Public Sub RefreshCalculatedLife(CalcLife As Double)
            OnPropertyChanged(NameOf(CalculatedLife))
        End Sub


        Public Sub RefreshSlabEdgeStress(MinStress As Double)
            OnPropertyChanged(NameOf(SlabEdgeStress))
        End Sub


        Public Sub RefreshSlabInteriorStress(MaxStress As Double)
            OnPropertyChanged(NameOf(SlabInteriorStress))
        End Sub


        Public Sub RefreshGWPercent()
            'If CurrentSectionView.Section.SelectedRun = 3 Then
            '    RefreshPCRMGPercent()
            'Else
            '    RefreshMGPercent()
            'End If
        End Sub


        Public Sub RefreshPCRMGPercent()
            For Each airplane In CurrentAirplanes
                If airplane.Gear = "HB" Or airplane.Gear = "A" Then
                    airplane.RunMgPercent = ((Format(airplane.MgPercentPCN * 1, "0.00000")))
                ElseIf airplane.Gear = "X" Then
                    airplane.RunMgPercent = ((Format(airplane.MgPercentPCN * 1, "0.00000")))
                ElseIf airplane.Deprecated = True Then
                    airplane.RunMgPercent = ((Format(airplane.MgPercentPCN * 1, "0.00000")))
                Else
                    airplane.RunMgPercent = ((Format(airplane.MgPercentPCN * 2, "0.00000")))
                End If
                OnPropertyChanged(NameOf(airplane.RunMgPercent))
            Next
        End Sub


        Public Sub RefreshMGPercent()
            Try
                For Each airplane In CurrentAirplanes
                    airplane.RunMgPercent = airplane.MgPercent
                    OnPropertyChanged(NameOf(airplane.RunMgPercent))
                Next
            Catch ex As Exception

            End Try
        End Sub


        Public Function RefreshHtml() As String

            If CurrentJob.Sections Is Nothing Then
                Return "No Structures have been defined."
                Exit Function
            End If
            Dim Thicknessunit As String
            Dim PressureUnit As String
            Dim WeightUnit As String

            If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                Thicknessunit = "in."
                PressureUnit = "psi"
                WeightUnit = "lbs"
            Else
                Thicknessunit = "mm"
                PressureUnit = "MPa"
                WeightUnit = "kg"

            End If

            Dim src = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"

            Dim reportTitle = "Federal Aviation Administration FAARFIELD 2.1 Summary Report"
            Dim html As String = ""
            Dim tmpdiv As String = ""
            Dim tmptable As String = ""
            Dim tmptd As String = ""
            Dim tmptd1 As String = ""
            Dim tmptd2 As String = ""
            Dim tmptd3 As String = ""
            Dim tmptd4 As String = ""
            Dim tmptr As String = ""
            Dim tmpthead As String = ""
            Dim tmpth As String = ""
            Dim img As String = ""

            ' Report Title Div
            tmpdiv = hu.wrap_h2(reportTitle)
            tmpdiv += hu.wrap_p(MainWindowTitle)
            html += hu.wrap_div(tmpdiv, "report")

            ' Job Title Div
            tmpdiv = hu.wrap_h3("Job Name: " + CurrentJob.Name)

            html += hu.wrap_div(tmpdiv, "job")

            ' Table Title Div
            tmpdiv = hu.wrap_h3("Structure Data")
            html += hu.wrap_div(tmpdiv, "title")

            'New Table
            'Table Row
            tmpth = hu.wrap_p("No.")
            tmptr = hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Structure Name")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Pavement Type")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Run Type")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Completed")
            tmptr += hu.wrap_th(tmpth)
            tmpthead = hu.wrap_tr(tmptr)
            tmptable = hu.wrap_thead(tmpthead)

            Dim rowNum As Integer = 1
            For Each Section As Object In CurrentJob.Sections
                If Section.IncludeInSummary = True Then


                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Section.Name.ToString())
                    tmptr += hu.wrap_td(tmptd)
                    If Not Section.analysistype Is Nothing Then
                        tmptd = hu.wrap_p(Section.AnalysisType.ToString())
                    Else
                        tmptd = hu.wrap_p("None")
                    End If

                    tmptr += hu.wrap_td(tmptd)
                    tmptd1 = hu.wrap_p("Thickness Design")
                    tmptd2 = hu.wrap_p("Life")
                    tmptd3 = hu.wrap_p("Compaction")
                    tmptd4 = hu.wrap_p("PCR")

                    tmptr += hu.wrap_td(tmptd1 + vbNewLine + tmptd2 + vbNewLine + tmptd3 + vbNewLine + tmptd4)
                    tmptd1 = hu.wrap_p((Section.SectionDesignRunTime))
                    tmptd2 = hu.wrap_p((Section.SectionLifeRunTime))
                    tmptd3 = hu.wrap_p((Section.SectionCompactionRunTime))
                    tmptd4 = hu.wrap_p((Section.SectionPCRRunTime))
                    tmptr += hu.wrap_td(tmptd1 + vbNewLine + tmptd2 + vbNewLine + tmptd3 + vbNewLine + tmptd4)
                    tmptable += hu.wrap_tr(tmptr)

                    rowNum += 1
                End If

            Next    'each layer

            'Table Div
            tmpdiv = hu.wrap_table(tmptable)
            html += hu.wrap_div(tmpdiv)

            ' Table Title Div
            ' Complete HTML Page
            html = hu.CreateHtmlPage(html, reportTitle)
            Return html

        End Function


        'Persist ReducedCrossSection Report Section
        Private htmlRXSArray(50, 50) As String


        Public Function refreshsectionreport() As String

            Dim Section As ISection
            Dim jobName As String, sectionName As String
            jobName = CurrentJob.Name
            Section = CurrentSectionView.Section

            If Section.SavedSectionReportHtml IsNot Nothing Then
                Return Section.SavedSectionReportHtml
            End If

            Dim SectionIndex = CurrentJob.Sections.IndexOf(Section)
            Dim jobindex As Integer

            For i = 0 To Jobs.Count - 1
                If DirectCast(DirectCast(Jobs(i), JobViewModel).FaarFieldJob, FaarFieldJob) Is CurrentJob Then
                    jobindex = i
                End If
            Next

            Dim theJob = CurrentJob
            Dim Thicknessunit As String
            Dim PressureUnit As String
            Dim LenghtUnit As String
            Dim WeightUnit As String
            Dim SubgradeUnit As String
            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                Thicknessunit = "in."
                PressureUnit = "psi"
                WeightUnit = "lbs"
                LenghtUnit = "in."
                SubgradeUnit = "pci"
            Else
                Thicknessunit = "mm"
                PressureUnit = "MPa"
                WeightUnit = "kg"
                LenghtUnit = "mm"
                SubgradeUnit = "MN/m^3"
            End If

            sectionName = Section.Name
            If Section.AnalysisType Is Nothing Then
                Return "Analysis Type is missing. "
            End If

            Dim src = FileIO.SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"

            Dim html As String = ""
            Dim html1 As String = ""
            Dim reportTitle1 = "Federal Aviation Administration FAARFIELD 2.1 Structure Report"
            Dim reportTitle2 = ""
            Dim tmpdiv As String = ""
            Dim tmpnbdiv As String = ""
            Dim tmptable As String = ""
            Dim tmptd As String = ""
            Dim tmptr As String = ""
            Dim tmpthead As String = ""
            Dim tmpth As String = ""
            Dim img As String = ""

            ' Report Title Div
            tmpdiv = hu.wrap_h2(reportTitle1)
            tmpdiv += hu.wrap_p(MainWindowTitle)
            html1 += hu.wrap_div(tmpdiv, "report")

            ' Job Title Div
            tmpdiv = hu.wrap_h3("Job Name: " + jobName)
            tmpdiv += hu.wrap_h4("Structure: " + sectionName)
            tmpdiv += hu.wrap_p("Analysis Type: " + Section.AnalysisType.Name)

            If Not Section.LastRun Is Nothing Then
                If Section.LastRun = "Thickness Design" Then
                    tmpdiv += hu.wrap_p("Last Run: " + Section.LastRun + " " + Section.SectionDesignrunTime)
                    tmpdiv += hu.wrap_p("Design Life = " + Section.Life.ToString + " Years")
                    If Section.AnalysisType.Name = "New Rigid" Then

                        If CurrentJob.DesignOptions.SlabStress = True Then
                            If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                                tmpdiv += hu.wrap_p("Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft)
                                tmpdiv += hu.wrap_p("Slab edge stress results: " + _slabEdgeStressreport + " (psi)")
                                tmpdiv += hu.wrap_p("Slab interior stress results: " + _slabInteriorStressreport + " (psi)")
                            Else
                                tmpdiv += hu.wrap_p("Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft)
                                tmpdiv += hu.wrap_p("Slab edge stress results: " + _slabEdgeStressreport + " (MPa)")
                                tmpdiv += hu.wrap_p("Slab interior stress results: " + _slabInteriorStressreport + " (MPa)")
                            End If
                        End If

                    End If
                    If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0.0").ToString + LenghtUnit)
                    Else
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0").ToString + LenghtUnit)
                    End If

                ElseIf Section.LastRun = "Life Analysis" Or Section.LastRun = "Life/Compaction Analysis" Then

                    If Section.LastRun = "Life Analysis" Then
                        tmpdiv += hu.wrap_p("Last Run: " + Section.LastRun + " " + Section.SectionLifeRunTime)
                    Else
                        tmpdiv += hu.wrap_p("Last Run: " + Section.LastRun + " " + Section.SectionCompactionRunTime)
                    End If
                    If Section.AnalysisType.Name = "New Rigid" Then

                        If CurrentJob.DesignOptions.SlabStress = True Then
                            If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                                tmpdiv += hu.wrap_p("Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft)
                                tmpdiv += hu.wrap_p("Slab edge stress results: " + _slabEdgeStressreport + " (psi)")
                                tmpdiv += hu.wrap_p("Slab interior stress results: " + _slabInteriorStressreport + " (psi)")
                            Else
                                tmpdiv += hu.wrap_p("Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft)
                                tmpdiv += hu.wrap_p("Slab edge stress results: " + _slabEdgeStressreport + " (MPa)")
                                tmpdiv += hu.wrap_p("Slab interior stress results: " + _slabInteriorStressreport + " (MPa)")
                            End If
                        End If

                    End If
                    tmpdiv += hu.wrap_p("Calculated Life = " + Format(Section.AnalysedLife, "N1").ToString + " Years")
                    If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0.0").ToString + LenghtUnit)
                    Else
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0").ToString + LenghtUnit)
                    End If
                Else
                    If Section.AnalysisType.Name = "New Rigid" Then

                        tmpdiv += hu.wrap_p("Last Run: " + Section.LastRun + " " + Section.SectionPCRRunTime)

                        If CurrentJob.DesignOptions.SlabStress = True AndAlso _criticalAircraftReport <> "" Then
                            If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                                tmpdiv += hu.wrap_p("Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft)
                                tmpdiv += hu.wrap_p("Slab edge stress results: " + _slabEdgeStressreport + " (psi)")
                                tmpdiv += hu.wrap_p("Slab interior stress results: " + _slabInteriorStressreport + " (psi)")
                            Else
                                tmpdiv += hu.wrap_p("Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft)
                                tmpdiv += hu.wrap_p("Slab edge stress results: " + _slabEdgeStressreport + " (MPa)")
                                tmpdiv += hu.wrap_p("Slab interior stress results: " + _slabInteriorStressreport + " (MPa)")
                            End If
                        End If

                    End If

                    tmpdiv += hu.wrap_p("No run has been done on this structure.")
                End If
            End If

            If DesignType = FlexOnRigid And Designed <> NullDate Then
                If HMAonRigidCase = 1 Then

                ElseIf HMAonRigidCase = 2 Then
                    tmpdiv += hu.wrap_p("Minimum HMA thickness was reached. Therefore, CDF totals may be less than 1.0.")
                ElseIf HMAonRigidCase = 3 Then
                    tmpdiv += hu.wrap_p("Overlay thickness was determined using a flexible pavement design procedure, considering the existing PCC as a high-stiffness base layer.")
                    tmpdiv += hu.wrap_p("CDF is determined for the top of the subgrade. Refer to AC 150/5320-6, paragraph 405, for additional information.")
                ElseIf HMAonRigidCase = 4 Then

                    tmpdiv += hu.wrap_p("Overlay thickness was determined using a flexible pavement design procedure, considering the existing PCC as a high-stiffness base layer.")
                    tmpdiv += hu.wrap_p("Minimum HMA thickness was reached. Therefore, CDF totals may be less than 1.0. CDF is determined for the top of the subgrade.")
                    tmpdiv += hu.wrap_p("Refer to AC 150/5320-6, paragraph 405, for additional information.")

                End If
            End If

            html1 += hu.wrap_div(tmpdiv, "job")

            'Title for Full Cross Section

            If CurrentJob.DesignOptions.ReducedCrossSection = True And Section.ReducedCrossSectionRun = True And htmlRXSArray(jobindex, SectionIndex) <> "" Then
                html1 += hu.wrap_h3("Full Cross Section (100% Annual Departures)")
            End If

            ' Table Title Div
            tmpdiv = hu.wrap_h3("Pavement Structure Information by Layer")
            tmpnbdiv = hu.wrap_div(tmpdiv, "title")

            ' New Table
            ' Table Row
            tmpth = hu.wrap_p("No.")
            tmptr = hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Type")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Thickness<br />" + "(" + Thicknessunit + ")")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Modulus<br />" + "(" + PressureUnit + ")")
            tmptr += hu.wrap_th(tmpth)
            'show k or CBR column in Pavement Structure table
            If CurrentSectionView.Section.AnalysisType.Name.Contains("Rigid") Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Then
                tmpth = hu.wrap_p("k (" + SubgradeUnit + ")")
            Else
                tmpth = hu.wrap_p("CBR")
            End If
            tmptr += hu.wrap_th(tmpth)

            tmpth = hu.wrap_p("Poisson's<br />Ratio")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Strength R<br />" + "(" + PressureUnit + ")")
            tmptr += hu.wrap_th(tmpth)

            tmpthead = hu.wrap_tr(tmptr)
            tmptable = hu.wrap_thead(tmpthead)

            Dim rowNum As Integer = 1
            Dim rigidchecker As Integer = 0
            For Each layer In Section.Layers
                If layer.Name.Contains("PCC") Then
                    rigidchecker = 1
                End If
            Next

            For Each layer In Section.Layers
                tmptd = hu.wrap_p(rowNum.ToString())
                tmptr = hu.wrap_td(tmptd)

                ' NEED TO INCLUDE Poisson, Strength
                tmptd = hu.wrap_p(layer.Name.ToString())
                tmptr += hu.wrap_td(tmptd)

                If layer.Name = "Subgrade" Then
                    tmptd = hu.wrap_p("0")
                Else
                    If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                        tmptd = hu.wrap_p(CDbl(layer.Thickness.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N1"))
                    Else
                        tmptd = hu.wrap_p(CDbl(layer.Thickness.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N0"))
                    End If
                End If
                tmptr += hu.wrap_td(tmptd)

                If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                    tmptd = hu.wrap_p(CDbl(layer.Modulus.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N0"))
                Else
                    tmptd = hu.wrap_p(CDbl(layer.Modulus.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N2"))
                End If

                tmptr += hu.wrap_td(tmptd)

                'add value of k/CBR to the column
                If layer.Name <> "Subgrade" Then
                    tmptd = hu.wrap_p("0")
                Else
                    If CurrentSectionView.Section.AnalysisType.Name.Contains("Rigid") Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Then
                        tmptd = hu.wrap_p(CDbl(layer.SubgradeReaction.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N1"))
                    Else
                        tmptd = hu.wrap_p(layer.Cbr.ToString())
                    End If
                End If
                tmptr += hu.wrap_td(tmptd)


                ' I assume that Poisson is a constant in this system.
                If layer.Category = "P-501 PCC" Then
                    tmptd = hu.wrap_p("0.15")
                ElseIf layer.Name = "Subgrade" And rigidchecker = 0 Then
                    tmptd = hu.wrap_p("0.35")
                ElseIf layer.Name = "Subgrade" And rigidchecker = 1 Then
                    tmptd = hu.wrap_p("0.4")
                ElseIf layer.Name = "Variable (rigid)" Or layer.Name = "P-306 Lean Concrete" Or layer.Name = "P-304 Cement Treated Base" Or layer.Name = "P-301 Soil Cement Base" Then
                    tmptd = hu.wrap_p("0.2")

                Else
                    tmptd = hu.wrap_p("0.35")
                End If
                tmptr += hu.wrap_td(tmptd)

                ' Strength is not found in this project, for now assuming value is 0
                If layer.Category = "P-501 PCC" Then

                    If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                        tmptd = hu.wrap_p(CDbl(layer.Rupture.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N0"))
                    Else
                        tmptd = hu.wrap_p(CDbl(layer.Rupture.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N2"))
                    End If

                Else
                    tmptd = hu.wrap_p("0")
                End If
                tmptr += hu.wrap_td(tmptd)

                tmptable += hu.wrap_tr(tmptr)

                rowNum += 1
            Next    'each layer

            ' Table Div
            tmpdiv = hu.wrap_table(tmptable)
            tmpnbdiv += hu.wrap_div(tmpdiv)
            html1 += hu.wrap_div(tmpnbdiv, "no-break")

            ' Table Title Div
            tmpdiv = hu.wrap_h3("Airplane Information")
            tmpnbdiv = hu.wrap_div(tmpdiv, "title")

            ' New Table
            ' Table Row
            tmpth = hu.wrap_p("No.")
            tmptr = hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Name")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Gross Wt.<br />" + "(" + WeightUnit + ")")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Annual<br />Departures")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("% Annual<br />Growth")
            tmptr += hu.wrap_th(tmpth)

            tmpthead = hu.wrap_tr(tmptr)
            tmptable = hu.wrap_thead(tmpthead)

            rowNum = 1      'Airplane information
            For Each airplane In Section.Airplanes
                tmptd = hu.wrap_p(rowNum.ToString())
                tmptr = hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(airplane.Name)
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(CDbl(airplane.GrossWeight.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N0"))
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(airplane.NumberDepartures.ToString("N0"))
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(airplane.AnnualGrowth.ToString())
                tmptr += hu.wrap_td(tmptd)

                tmptable += hu.wrap_tr(tmptr)

                rowNum += 1
            Next    'each airplane

            ' Table Div
            tmpdiv = hu.wrap_table(tmptable)
            tmpnbdiv += hu.wrap_div(tmpdiv)
            html1 += hu.wrap_div(tmpnbdiv, "no-break")

            ' Table Title Div
            tmpdiv = hu.wrap_h3("Additional Airplane Information")
            If (DesignType = NewFlex) Or (DesignType = FlexOnFlex) Then
                tmpdiv += hu.wrap_p("Subgrade CDF")
            End If
            tmpnbdiv = hu.wrap_div(tmpdiv, "title")

            ' New Table
            ' Table Row
            tmpth = hu.wrap_p("No.")
            tmptr = hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("Name")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("CDF<br />Contribution")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("CDF Max<br />for Airplane")
            tmptr += hu.wrap_th(tmpth)
            tmpth = hu.wrap_p("P/C<br />Ratio")      'PROBABLY NOT IN THE SYSTEM YET
            tmptr += hu.wrap_th(tmpth)

            tmpthead = hu.wrap_tr(tmptr)
            tmptable = hu.wrap_thead(tmpthead)

            rowNum = 1
            For Each airplane In Section.Airplanes
                tmptd = hu.wrap_p(rowNum.ToString())
                tmptr = hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(airplane.Name)
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(CDbl(airplane.Cdf).ToString("f2"))
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(CDbl(airplane.CdfAircraftMax).ToString("f2"))
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(airplane.CtoP.ToString)
                tmptr += hu.wrap_td(tmptd)

                tmptable += hu.wrap_tr(tmptr)

                rowNum += 1
            Next    'each additional data for airplane

            ' Table Div
            tmpdiv = hu.wrap_table(tmptable)
            tmpnbdiv += hu.wrap_div(tmpdiv)
            html1 += hu.wrap_div(tmpnbdiv, "no-break")

            If CurrentJob.DesignOptions.CalculateHmaCdf And (DesignType = NewFlex) Then
                ' Table Title Div
                tmpdiv = hu.wrap_h3("HMA CDF")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF<br />Contribution")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF Max<br />for Airplane")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("P/C<br />Ratio")      'PROBABLY NOT IN THE SYSTEM YET
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                Dim I = 1
                rowNum = 1
                For Each airplane In Section.Airplanes
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.Name)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(jobCDFtableHMA(ISect, I), "#,###,##0.00").ToString)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(jobCDFacrftMaxtableHMA(ISect, I), "#,###,##0.00").ToString)
                    tmptr += hu.wrap_td(tmptd)

                    FEDFAA1.DTemp = Math.Abs(jobCtoPtableHMA(ISect, I))
                    If DTemp <> 0 Then DTemp = 1 / DTemp
                    If DTemp > 100 Then
                        tmptd = hu.wrap_p(">100")
                    Else
                        tmptd = hu.wrap_p(Format(DTemp, "0.00"))
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    tmptable += hu.wrap_tr(tmptr)

                    I = I + 1

                    rowNum += 1
                Next

                ' Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                html1 += hu.wrap_div(tmpnbdiv, "no-break")

            End If
            If Section.Layers.Count > 0 Then
                If CurrentJob.DesignOptions.CalculateHmaCdf And (DesignType = NewFlex) And Section.Layers.Item(1).Name = "P-401/P-403 HMA Stabilized" Then

                    ' Table Title Div
                    tmpdiv = hu.wrap_h3("P-401/P-403 HMA Stabilized CDF")
                    tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                    ' New Table
                    ' Table Row
                    tmpth = hu.wrap_p("No.")
                    tmptr = hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Name")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("CDF<br />Contribution")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("CDF Max<br />for Airplane")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("P/C<br />Ratio")      'PROBABLY NOT IN THE SYSTEM YET
                    tmptr += hu.wrap_th(tmpth)

                    tmpthead = hu.wrap_tr(tmptr)
                    tmptable = hu.wrap_thead(tmpthead)

                    rowNum = 1
                    Dim I = 1
                    For Each airplane In Section.Airplanes
                        tmptd = hu.wrap_p(rowNum.ToString())
                        tmptr = hu.wrap_td(tmptd)
                        tmptd = hu.wrap_p(airplane.Name)
                        tmptr += hu.wrap_td(tmptd)
                        tmptd = hu.wrap_p(Format(jobCDFtable401(ISect, I), "#,###,##0.00").ToString)
                        tmptr += hu.wrap_td(tmptd)
                        tmptd = hu.wrap_p(Format(jobCDFacrftMaxtable401(ISect, I), "#,###,##0.00").ToString)
                        tmptr += hu.wrap_td(tmptd)
                        FEDFAA1.DTemp = Math.Abs(jobCtoPtable401(ISect, I))
                        If DTemp <> 0 Then DTemp = 1 / DTemp
                        If DTemp > 100 Then
                            tmptd = hu.wrap_p(">100")
                        Else
                            tmptd = hu.wrap_p(Format(DTemp, "0.00"))
                        End If

                        tmptr += hu.wrap_td(tmptd)

                        tmptable += hu.wrap_tr(tmptr)

                        I = I + 1
                        rowNum += 1
                    Next

                    ' Table Div
                    tmpdiv = hu.wrap_table(tmptable)
                    tmpnbdiv += hu.wrap_div(tmpdiv)
                    html1 += hu.wrap_div(tmpnbdiv, "no-break")

                End If
            End If

            'HMA Overlay over HMA - CDF
            If (Not NoACCDF) And (DesignType = FlexOnFlex) Then
                ' Table Title Div
                tmpdiv = hu.wrap_h3("Overlay HMA CDF")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF<br />Contribution")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF Max<br />for Airplane")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("P/C<br />Ratio")      'PROBABLY NOT IN THE SYSTEM YET
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                rowNum = 1
                Dim I = 1
                For Each airplane In Section.Airplanes
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.Name)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(jobCDFtableAC(ISect, I), "#,###,##0.00").ToString)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(jobCDFacrftMaxtableAC(ISect, I), "#,###,##0.00").ToString)
                    tmptr += hu.wrap_td(tmptd)
                    FEDFAA1.DTemp = Math.Abs(jobCtoPtableAC(ISect, I))
                    If DTemp <> 0 Then DTemp = 1 / DTemp
                    If DTemp > 100 Then
                        tmptd = hu.wrap_p(">100")
                    Else
                        tmptd = hu.wrap_p(Format(DTemp, "0.00"))
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    tmptable += hu.wrap_tr(tmptr)

                    I = I + 1
                    rowNum += 1
                Next

                ' Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                html1 += hu.wrap_div(tmpnbdiv, "no-break")

                'HMA under HMA overlay
                ' Table Title Div
                tmpdiv = hu.wrap_h3("HMA CDF")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF<br />Contribution")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF Max<br />for Airplane")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("P/C<br />Ratio")      'PROBABLY NOT IN THE SYSTEM YET
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                rowNum = 1
                I = 1
                For Each airplane In Section.Airplanes
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.Name)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(jobCDFtableHMA(ISect, I), "#,###,##0.00").ToString)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(jobCDFacrftMaxtableHMA(ISect, I), "#,###,##0.00").ToString)
                    tmptr += hu.wrap_td(tmptd)
                    FEDFAA1.DTemp = Math.Abs(jobCtoPtableHMA(ISect, I))
                    If DTemp <> 0 Then DTemp = 1 / DTemp
                    If DTemp > 100 Then
                        tmptd = hu.wrap_p(">100")
                    Else
                        tmptd = hu.wrap_p(Format(DTemp, "0.00"))
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    tmptable += hu.wrap_tr(tmptr)

                    I = I + 1
                    rowNum += 1
                Next

                ' Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                html1 += hu.wrap_div(tmpnbdiv, "no-break")

                If Section.Layers.Item(2).Name = "P-401/P-403 HMA Stabilized" Then

                    ' Table Title Div
                    tmpdiv = hu.wrap_h3("P-401/P-403 HMA Stabilized CDF")
                    tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                    ' New Table
                    ' Table Row
                    tmpth = hu.wrap_p("No.")
                    tmptr = hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Name")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("CDF<br />Contribution")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("CDF Max<br />for Airplane")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("P/C<br />Ratio")      'PROBABLY NOT IN THE SYSTEM YET
                    tmptr += hu.wrap_th(tmpth)

                    tmpthead = hu.wrap_tr(tmptr)
                    tmptable = hu.wrap_thead(tmpthead)

                    rowNum = 1
                    I = 1
                    For Each airplane In Section.Airplanes
                        tmptd = hu.wrap_p(rowNum.ToString())
                        tmptr = hu.wrap_td(tmptd)
                        tmptd = hu.wrap_p(airplane.Name)
                        tmptr += hu.wrap_td(tmptd)
                        tmptd = hu.wrap_p(Format(jobCDFtable401(ISect, I), "#,###,##0.00").ToString)
                        tmptr += hu.wrap_td(tmptd)
                        tmptd = hu.wrap_p(Format(jobCDFacrftMaxtable401(ISect, I), "#,###,##0.00").ToString)
                        tmptr += hu.wrap_td(tmptd)
                        FEDFAA1.DTemp = Math.Abs(jobCtoPtable401(ISect, I))
                        If DTemp <> 0 Then DTemp = 1 / DTemp
                        If DTemp > 100 Then
                            tmptd = hu.wrap_p(">100")
                        Else
                            tmptd = hu.wrap_p(Format(DTemp, "0.00"))
                        End If
                        tmptr += hu.wrap_td(tmptd)

                        tmptable += hu.wrap_tr(tmptr)

                        I = I + 1
                        rowNum += 1
                    Next

                    ' Table Div
                    tmpdiv = hu.wrap_table(tmptable)
                    tmpnbdiv += hu.wrap_div(tmpdiv)
                    html1 += hu.wrap_div(tmpnbdiv, "no-break")
                End If

            End If

            If (DesignType = NewFlex) Or DesignType = NewRigid Then
                If CurrentSectionView.Section.SelectedRun = 2 Then
                    ' Table Title Div
                    tmpdiv = hu.wrap_h3("Subgrade Compaction Requirements")
                    tmpdiv += hu.wrap_p("NonCohesive Soil")
                    tmpnbdiv = hu.wrap_div(tmpdiv, "title")


                    'implementing metric
                    ' New Table
                    ' Table Row
                    tmpth = hu.wrap_p("Percent Maximum Dry Density(%)")
                    tmptr = hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Depth of compaction <br /> from pavement surface (" + Thicknessunit + ")")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Depth of compaction <br /> from top of subgrade (" & Thicknessunit & ")")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Critical Airplane for Compaction")
                    tmptr += hu.wrap_th(tmpth)

                    tmpthead = hu.wrap_tr(tmptr)
                    tmptable = hu.wrap_thead(tmpthead)

                    rowNum = 1

                    For J = 1 To NDenLevel
                        If jobCompactionIntDenNCtable(ISect, IJob, J) >= DensityNCMin Then
                            tmptd = hu.wrap_p(Format(jobCompactionIntDenNCtable(ISect, IJob, J), "0").ToString)
                            tmptr = hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p((Format(jobCompactionIntDenDepthNCtable(ISect, IJob, J - 1) * UnitsOut.inch, "0")).ToString + " - " + (Format(jobCompactionIntDenDepthNCtable(ISect, IJob, J) * UnitsOut.inch, "0")).ToString)
                            tmptr += hu.wrap_td(tmptd)
                            If jobCompactionIntDenDepthNCtable(ISect, IJob, J) <= ThicktoSubgrade Then
                                tmptd = hu.wrap_p("--")
                            Else
                                tmptd = hu.wrap_p(Format(Math.Max(jobCompactionIntDenDepthNCtable(ISect, IJob, J - 1) - ThicktoSubgrade, 0) * UnitsOut.inch, "0") + " - " + Format((jobCompactionIntDenDepthNCtable(ISect, IJob, J) - ThicktoSubgrade) * UnitsOut.inch, "0"))
                            End If
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(ACName(jobCompactionIntDenCriticalACNCtable(ISect, IJob, J)))
                            tmptr += hu.wrap_td(tmptd)

                            tmptable += hu.wrap_tr(tmptr)

                            rowNum += 1

                        End If
                    Next J

                    ' Table Div
                    tmpdiv = hu.wrap_table(tmptable)
                    tmpnbdiv += hu.wrap_div(tmpdiv)
                    html1 += hu.wrap_div(tmpnbdiv, "no-break")

                    ' Table Title Div
                    tmpdiv = hu.wrap_p("Cohesive Soil")
                    tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                    'implementing metric
                    ' New Table
                    ' Table Row
                    tmpth = hu.wrap_p("Percent Maximum Dry Density(%)")
                    tmptr = hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Depth of compaction <br /> from pavement surface (" & Thicknessunit & ")")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Depth of compaction <br /> from top of subgrade (" & Thicknessunit & ")")
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Critical Airplane for Compaction")
                    tmptr += hu.wrap_th(tmpth)

                    tmpthead = hu.wrap_tr(tmptr)
                    tmptable = hu.wrap_thead(tmpthead)

                    rowNum = 1

                    For J = 1 To NDenLevel
                        If jobCompactionIntDenCtable(ISect, IJob, J) >= DensityCMin Then
                            tmptd = hu.wrap_p(Format(jobCompactionIntDenCtable(ISect, IJob, J), "0").ToString)
                            tmptr = hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p((Format(jobCompactionIntDenDepthCtable(ISect, IJob, J - 1) * UnitsOut.inch, "0")).ToString + " - " + (Format(jobCompactionIntDenDepthCtable(ISect, IJob, J) * UnitsOut.inch, "0")).ToString)
                            tmptr += hu.wrap_td(tmptd)
                            If jobCompactionIntDenDepthCtable(ISect, IJob, J) <= ThicktoSubgrade Then
                                tmptd = hu.wrap_p("--")
                            Else
                                tmptd = hu.wrap_p(Format(Math.Max(jobCompactionIntDenDepthCtable(ISect, IJob, J - 1) - ThicktoSubgrade, 0) * UnitsOut.inch, "0") + " - " + Format((jobCompactionIntDenDepthCtable(ISect, IJob, J) - ThicktoSubgrade) * UnitsOut.inch, "0"))
                            End If
                            tmptr += hu.wrap_td(tmptd)

                            tmptd = hu.wrap_p(ACName(jobCompactionIntDenCriticalACCtable(ISect, IJob, J)))
                            tmptr += hu.wrap_td(tmptd)

                            tmptable += hu.wrap_tr(tmptr)

                            rowNum += 1
                        End If
                    Next J

                    ' Table Div
                    tmpdiv = hu.wrap_table(tmptable)
                    tmpnbdiv += hu.wrap_div(tmpdiv)
                    html1 += hu.wrap_div(tmpnbdiv, "no-break")

                    Dim ASTMcodecheck As Boolean = False
                    'added for NOTES by YGC 102213 
                    tmpdiv = hu.wrap_p("<b>Subgrade Compaction Notes:</b>")
                    tmpdiv += hu.wrap_p("1.	Noncohesive  soils, for the purpose of determining compaction control, are those with a plasticity index (PI) less than 3.")
                    tmpdiv += hu.wrap_p("2.	Tabulated values indicate depth ranges within which densities should equal or exceed the indicated percentage of the maximum dry density as specified in item P-152.")
                    For Each airplane In CurrentSectionView.Section.Airplanes
                        If airplane.GrossWeight.UsCustomary * airplane.MgPercent >= 60000 Then
                            ASTMcodecheck = True
                        End If
                    Next
                    If ASTMcodecheck = True Then
                        tmpdiv += hu.wrap_p("3.	Maximum dry density is determined using ASTM Method D 1557.")
                    Else
                        tmpdiv += hu.wrap_p("3.	Maximum dry density is determined using ASTM Method D 698.")
                    End If
                    tmpdiv += hu.wrap_p("4.	The subgrade in cut areas should have natural densities shown or should (a) be compacted from the surface to achieve the required densities, (b) be removed and replaced at the densities shown, or (c) when economics and grades permit, be covered with sufficient select or subbase material so that the uncompacted subgrade is at a depth where the in-place densities are satisfactory.")
                    tmpdiv += hu.wrap_p("5.	For swelling soils refer to AC 150/5320-6F paragraph 3.10.")
                    'add ended for NOTES by YGC 102213 

                    html1 += hu.wrap_div(tmpdiv)

                End If
            End If

            tmpdiv = hu.wrap_h4("NOTE:")
            tmpdiv += hu.wrap_p("User is responsible for checking frost protection requirements.")
            html1 += hu.wrap_div(tmpdiv, "disclaimer")

            ' Image
            Dim bitmap As Bitmap
            bitmap = BitmapImage2Bitmap(ProfileImage)
            tmpdiv = hu.wrap_bmp_img(bitmap)
            bitmap.Dispose()
            html1 += hu.wrap_div(tmpdiv, "image")

            ' Complete HTML Page
            html1 = hu.wrap_div(html1, "page")

            tmpdiv = hu.wrap_p("")
            html1 += hu.wrap_div(tmpdiv, "page-break")

            ' Create HTML page
            html1 = hu.CreateHtmlPage(html1, reportTitle1)

            'Reduced Cross Section result:
            If Section.LastRun = "Thickness Design" And Section.ReducedCrossSectionRun = True And CurrentJob.DesignOptions.ReducedCrossSection = True And (CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "New Rigid") Then
                rowNum = 1
                htmlRXSArray(jobindex, SectionIndex) = hu.wrap_h3("Reduced Cross Section (1% Annual Departures)")
                tmpdiv = hu.wrap_h3("Pavement Structure Information by Layer")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Type")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Thickness<br />" + "(" + Thicknessunit + ")")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Modulus<br />" + "(" + PressureUnit + ")")
                tmptr += hu.wrap_th(tmpth)
                'show K Or CBR column in Pavement Structure table
                If CurrentSectionView.Section.AnalysisType.Name.Contains("Rigid") Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Then
                    tmpth = hu.wrap_p("k (" + SubgradeUnit + ")")
                Else
                    tmpth = hu.wrap_p("CBR")
                End If
                tmptr += hu.wrap_th(tmpth)

                tmpth = hu.wrap_p("Poisson's<br />Ratio")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Strength R<br />" + "(" + PressureUnit + ")")
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)


                For Each layer In Section.Layers
                    If layer.Name.Contains("PCC") Then
                        rigidchecker = 1
                    End If
                Next

                For Each layer In Section.Layers
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)

                    'NEED TO INCLUDE Poisson, Strength
                    tmptd = hu.wrap_p(layer.Name.ToString())
                    tmptr += hu.wrap_td(tmptd)

                    If layer.Name = "Subgrade" Then
                        tmptd = hu.wrap_p("0")
                    Else
                        If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                            tmptd = hu.wrap_p(CDbl(Section.ReducedCrossSectionLayerThickness.Item(rowNum - 1).GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N1"))
                        Else
                            tmptd = hu.wrap_p(CDbl(Section.ReducedCrossSectionLayerThickness.Item(rowNum - 1).GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N1"))
                        End If
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                        tmptd = hu.wrap_p(CDbl(Section.ReducedCrossSectionLayerModulus.Item(rowNum - 1).GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N0"))
                    Else
                        tmptd = hu.wrap_p(CDbl(Section.ReducedCrossSectionLayerModulus.Item(rowNum - 1).GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N2"))
                    End If

                    tmptr += hu.wrap_td(tmptd)

                    ' add value of k/CBR to the column
                    If layer.Name <> "Subgrade" Then
                        tmptd = hu.wrap_p("0")
                    Else
                        If CurrentSectionView.Section.AnalysisType.Name.Contains("Rigid") Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Then
                            tmptd = hu.wrap_p(CDbl(layer.SubgradeReaction.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N1"))
                        Else
                            tmptd = hu.wrap_p(layer.Cbr.ToString())
                        End If
                    End If
                    tmptr += hu.wrap_td(tmptd)


                    'I assume that Poisson Is a constant in this system.
                    If layer.Category = "P-501 PCC" Then
                        tmptd = hu.wrap_p("0.15")
                    ElseIf layer.Name = "Subgrade" And rigidchecker = 0 Then
                        tmptd = hu.wrap_p("0.35")
                    ElseIf layer.Name = "Subgrade" And rigidchecker = 1 Then
                        tmptd = hu.wrap_p("0.4")
                    ElseIf layer.Name = "Variable (rigid)" Or layer.Name = "P-306 Lean Concrete" Or layer.Name = "P-304 Cement Treated Base" Or layer.Name = "P-301 Soil Cement Base" Then
                        tmptd = hu.wrap_p("0.2")

                    Else
                        tmptd = hu.wrap_p("0.35")
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    'Strength Is Not found In this project, for now assuming value Is 0
                    If layer.Category = "P-501 PCC" Then

                        If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                            tmptd = hu.wrap_p(CDbl(layer.Rupture.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N0"))
                        Else
                            tmptd = hu.wrap_p(CDbl(layer.Rupture.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N2"))
                        End If

                    Else
                        tmptd = hu.wrap_p("0")
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    tmptable += hu.wrap_tr(tmptr)

                    rowNum += 1
                Next    'each layer

                'Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                htmlRXSArray(jobindex, SectionIndex) += hu.wrap_div(tmpnbdiv, "no-break")

                'Table Title Div
                tmpdiv = hu.wrap_h3("Airplane Information")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Gross Wt.<br />" + "(" + WeightUnit + ")")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Annual<br />Departures")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("% Annual<br />Growth")
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                rowNum = 1      'Airplane information
                For Each airplane In Section.Airplanes
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.Name)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(CDbl(airplane.GrossWeight.GetValue(theJob.DesignOptions.MeasurementSystem)).ToString("N0"))
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Section.ReducedDesignAnnualDeparture.Item(rowNum - 1).ToString("N0"))
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.AnnualGrowth.ToString())
                    tmptr += hu.wrap_td(tmptd)

                    tmptable += hu.wrap_tr(tmptr)

                    rowNum += 1
                Next    'each airplane

                'Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                htmlRXSArray(jobindex, SectionIndex) += hu.wrap_div(tmpnbdiv, "no-break")

                'Table Title Div
                tmpdiv = hu.wrap_h3("Additional Airplane Information")
                If (DesignType = NewFlex) Or (DesignType = FlexOnFlex) Then
                    tmpdiv += hu.wrap_p("Subgrade CDF")
                End If
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                'New Table
                'Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF<br />Contribution")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("CDF Max<br />for Airplane")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("P/C<br />Ratio")      'PROBABLY NOT IN THE SYSTEM YET
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                rowNum = 1
                For Each airplane In Section.Airplanes
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.Name)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(CDbl(Section.ReducedDesignCDF.Item(rowNum - 1)).ToString("f2"))
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(CDbl(Section.ReducedDesignCDFContribution.Item(rowNum - 1)).ToString("f2"))
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Section.ReducedDesignPtoC.Item(rowNum - 1).ToString)
                    tmptr += hu.wrap_td(tmptd)

                    tmptable += hu.wrap_tr(tmptr)

                    rowNum += 1
                Next    'each additional data for airplane

                'Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                htmlRXSArray(jobindex, SectionIndex) += hu.wrap_div(tmpnbdiv, "no-break")

                ' Complete HTML Page
                htmlRXSArray(jobindex, SectionIndex) = hu.wrap_div(htmlRXSArray(jobindex, SectionIndex), "page")

                ' Create HTML Page
                htmlRXSArray(jobindex, SectionIndex) = hu.CreateHtmlPage(htmlRXSArray(jobindex, SectionIndex), reportTitle2)

            ElseIf Not (CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "New Rigid") Then
                htmlRXSArray(jobindex, SectionIndex) = ""
            End If

            If CurrentJob.DesignOptions.ReducedCrossSection = True And Section.ReducedCrossSectionRun = True And htmlRXSArray(jobindex, SectionIndex) <> "" Then
                html = html1 + htmlRXSArray(jobindex, SectionIndex)
            Else
                html = html1
            End If

            Section.SavedSectionReportHtml = html
            Return html

        End Function


        Public Function refreshcdfgraph() As String

            Dim Section As ISection
            Dim jobName As String, sectionName As String
            Section = CurrentSectionView.Section
            jobName = CurrentJob.Name

            Dim theJob = CurrentJob
            Dim Thicknessunit As String
            Dim PressureUnit As String
            Dim WeightUnit As String
            Dim LenghtUnit As String

            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                Thicknessunit = "in."
                PressureUnit = "psi"
                WeightUnit = "lbs"
                LenghtUnit = "in."
            Else
                Thicknessunit = "mm"
                PressureUnit = "MPa"
                WeightUnit = "kg"
                LenghtUnit = "mm"
            End If

            sectionName = Section.Name
            If Section.AnalysisType Is Nothing Then
                Return "Analysis Type is missing. "
            End If

            Dim src = FileIO.SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"

            Dim reportTitle = "Federal Aviation Administration FAARFIELD CDF Graph"
            Dim html As String = ""
            Dim tmpdiv As String = ""
            Dim tmptable As String = ""
            Dim tmptd As String = ""
            Dim tmptr As String = ""
            Dim img As String = ""

            ' Report Title Div
            tmpdiv = hu.wrap_h2(reportTitle)
            tmpdiv += hu.wrap_p(MainWindowTitle)
            html += hu.wrap_div(tmpdiv, "report")

            ' Job Title Div
            tmpdiv = hu.wrap_h3("Job Name: " + jobName)
            tmpdiv += hu.wrap_h4("Structure: " + sectionName)

            tmpdiv += hu.wrap_p("Analysis Type: " + Section.AnalysisType.Name)
            If Section.SelectedRun = 0 Then
                tmpdiv += hu.wrap_p("Analysis Run Time: " + Section.SectionDesignrunTime)
            ElseIf Section.SelectedRun = 1 Then
                tmpdiv += hu.wrap_p("Analysis Run Time: " + Section.SectionLifeRunTime)
            ElseIf Section.SelectedRun = 2 Then
                tmpdiv += hu.wrap_p("Analysis Run Time: " + Section.SectionLifeRunTime)
            ElseIf Section.SelectedRun = 3 Then
                tmpdiv += hu.wrap_p("Analysis Run Time: " + Section.SectionPCRRunTime)
            End If

            If Not Section.LastRun Is Nothing Then
                If Not Section.LastRun = "Life Analysis" Then
                    tmpdiv += hu.wrap_p("Last Run: " + Section.LastRun)
                    tmpdiv += hu.wrap_p("Design Life = " + Section.Life.ToString("N1") + " Years")
                    If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0.0").ToString + LenghtUnit)
                    Else
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0").ToString + LenghtUnit)
                    End If
                Else
                    tmpdiv += hu.wrap_p("Last Run: " + Section.LastRun)
                    tmpdiv += hu.wrap_p("Calculated Life = " + Format(Section.AnalysedLife, "N1").ToString + " Years")
                    If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0.0").ToString + LenghtUnit)
                    Else
                        tmpdiv += hu.wrap_p("Total thickness to the top of the subgrade = " + Format(Section.TotalThickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem), "0").ToString + LenghtUnit)
                    End If
                End If
            Else
                If Section.SavedCDFgraph IsNot Nothing Then
                    Return Section.SavedCDFgraph
                Else
                    tmpdiv += hu.wrap_p("No run has been done on this structure.")
                End If
            End If

            html += hu.wrap_div(tmpdiv, "job")

            ' Table Title Div
            tmpdiv = hu.wrap_h3(" ")
            html += hu.wrap_div(tmpdiv, "title")

            'If there is no RUN, the chart will not show
            If Section.LastRun Is Nothing Then
                tmpdiv = hu.wrap_p(" ")
            Else
                ' Image
                Dim bitmap As Bitmap
                bitmap = DrawCDFGraph()
                tmpdiv = hu.wrap_bmp_img(bitmap)
                bitmap.Dispose()
                html += hu.wrap_div(tmpdiv, "image")
                html += hu.wrap_p("Figure: CDF distribution across pavement width for all aircraft in the traffic mix. " &
                    "The heavy blue line shows cumulative CDF. Individual aircraft are shown in distinct colors.", "chart-caption")
            End If

            ' Complete HTML Page
            html = hu.CreateHtmlPage(html, reportTitle)
            Section.SavedCDFgraph = html
            Return html

        End Function


        Public Function CreateDetailedReportErrorPage(errorMessage As String) As String
            Dim html As String = ""
            Dim tmpdiv As String = hu.wrap_h3("Error generating CM Report")
            tmpdiv += hu.wrap_p(errorMessage)
            html += hu.wrap_div(tmpdiv, "report")
            html = hu.CreateHtmlPage(html, "CM Report Error")
            Return html
        End Function

        Public Function refreshDetailedReport() As String

            Try

            Dim Section As ISection
            Dim jobName As String, sectionName As String
            Section = CurrentSectionView.Section
            jobName = CurrentJob.Name
            sectionName = Section.Name

            If Section.SavedDetailedReportHtml IsNot Nothing Then
                Return Section.SavedDetailedReportHtml
            End If

            Dim theJob = CurrentJob
            Dim Thicknessunit As String
            Dim PressureUnit As String
            Dim WeightUnit As String
            Dim LenghtUnit As String

            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                Thicknessunit = "in."
                PressureUnit = "psi"
                WeightUnit = "lbs"
                LenghtUnit = "in."
            Else
                Thicknessunit = "mm"
                PressureUnit = "MPa"
                WeightUnit = "kg"
                LenghtUnit = "mm"
            End If

            ' Delegate to the unified SVG-based HTML report generator
            Dim analysisName As String = If(Section.AnalysisType IsNot Nothing, Section.AnalysisType.Name, Nothing)
            Dim html As String = Libs.HtmlReportGenerator.Generate(
                FEDFAA1.gDetailedReportData,
                jobName,
                sectionName,
                analysisName,
                MainWindowTitle,
                Thicknessunit, PressureUnit, WeightUnit, LenghtUnit)

            Section.SavedDetailedReportHtml = html
            Return html

            Catch ex As Exception
                Debug.WriteLine("refreshDetailedReport error: " & ex.Message)
                Return CreateDetailedReportErrorPage(ex.Message)
            End Try

        End Function


        ''' <summary>
        ''' Draws a CDF vs Offset chart for a single aircraft using System.Drawing.
        ''' </summary>
        Private Function DrawSingleAircraftCDFChart(det As clsAircraftDetail, criticalOffset As Integer, maxCDF As Single, title As String, offsetUnit As String, acColor As Color, Optional evalDepth As Double = 0) As Bitmap
            Dim chartWidth As Integer = 950
            Dim chartHeight As Integer = 500
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                ' Chart margins
                Dim marginLeft As Integer = 80
                Dim marginRight As Integer = 30
                Dim marginTop As Integer = 50
                Dim marginBottom As Integer = 65
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Draw title
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleSize = g.MeasureString(title & " — CDF vs Offset", titleFont)
                g.DrawString(title & " — CDF vs Offset", titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 10)

                ' Determine data range
                Dim maxDataCDF As Double = 0
                For ioff As Integer = 1 To CDF.NOFF
                    If det.CDFByOffset(ioff) > maxDataCDF Then maxDataCDF = det.CDFByOffset(ioff)
                Next
                If maxDataCDF <= 0 Then maxDataCDF = 0.001
                Dim yMax As Double = maxDataCDF * 1.15

                ' Bilateral X range: -400 to +400
                Dim xMin As Double = -CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
                Dim xMax As Double = CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
                Dim xRange As Double = xMax - xMin

                ' Pixel mapping helper
                Dim toXPx = Function(xVal As Double) CSng(marginLeft + ((xVal - xMin) / xRange) * plotWidth)

                ' Draw plot area background
                Dim plotRect As New Rectangle(marginLeft, marginTop, plotWidth, plotHeight)
                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), plotRect)
                g.DrawRectangle(Pens.DarkGray, plotRect)

                ' Draw gridlines
                Dim gridPen As New Pen(Color.FromArgb(210, 214, 220), 1)
                gridPen.DashStyle = Drawing2D.DashStyle.Dot
                Dim nGridY As Integer = 5
                Dim axisFont As New Font("Segoe UI", 8.5F)
                Dim labelFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)

                For i As Integer = 0 To nGridY
                    Dim yVal As Double = yMax * i / nGridY
                    Dim yPx As Integer = CInt(marginTop + plotHeight - (yVal / yMax) * plotHeight)
                    If i > 0 AndAlso i < nGridY Then g.DrawLine(gridPen, marginLeft, yPx, marginLeft + plotWidth, yPx)
                    Dim yLabel = Format(yVal, "0.0000")
                    Dim yLabelSize = g.MeasureString(yLabel, axisFont)
                    g.DrawString(yLabel, axisFont, Brushes.Black, marginLeft - yLabelSize.Width - 4, yPx - yLabelSize.Height / 2)
                Next

                ' X-axis ticks: -400, -300, ..., 0, ..., 300, 400
                For xTick As Integer = CInt(xMin) To CInt(xMax) Step 100
                    Dim xPx As Single = toXPx(xTick)
                    If xTick <> CInt(xMin) AndAlso xTick <> CInt(xMax) Then
                        g.DrawLine(gridPen, CInt(xPx), marginTop, CInt(xPx), marginTop + plotHeight)
                    End If
                    Dim xLabel = xTick.ToString()
                    Dim xLabelSize = g.MeasureString(xLabel, axisFont)
                    g.DrawString(xLabel, axisFont, Brushes.Black, xPx - xLabelSize.Width / 2, marginTop + plotHeight + 4)
                Next

                ' Draw axis labels
                Dim xAxisLabel = "Offset (" & offsetUnit & ")"
                Dim xAxisSize = g.MeasureString(xAxisLabel, labelFont)
                g.DrawString(xAxisLabel, labelFont, Brushes.Black, CSng(marginLeft + (plotWidth - xAxisSize.Width) / 2), chartHeight - 22)

                ' Y-axis label (drawn rotated)
                Dim yAxisLabel = "CDF"
                g.TranslateTransform(30, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(-90)
                Dim yAxisSize = g.MeasureString(yAxisLabel, labelFont)
                g.DrawString(yAxisLabel, labelFont, Brushes.Black, -yAxisSize.Width / 2, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' --- Wheel reconstruction for symmetric mirror inference ---
                ' The backend may capture only one wheel of a symmetric pair (libNTires truncation).
                ' Reconstructing the mirror partner restores the correct unimodal CDF shape.
                Dim hasTireData As Boolean = (det.NWheels > 0 AndAlso det.WheelX IsNot Nothing AndAlso det.XCenter > 0)
                Dim tireColors() As Color = {
                    Color.FromArgb(31, 119, 180),   ' blue
                    Color.FromArgb(255, 127, 14),   ' orange
                    Color.FromArgb(44, 160, 44),     ' green
                    Color.FromArgb(214, 39, 40),     ' red
                    Color.FromArgb(148, 103, 189),   ' purple
                    Color.FromArgb(140, 86, 75),     ' brown
                    Color.FromArgb(227, 119, 194),   ' pink
                    Color.FromArgb(127, 127, 127)    ' gray
                }

                Dim allWheelsX As New List(Of Single)
                Dim reconstructedWheels As Boolean = False
                If det.NWheels > 0 AndAlso det.WheelX IsNot Nothing Then
                    For iw As Integer = 1 To det.NWheels
                        allWheelsX.Add(det.WheelX(iw))
                    Next
                    Dim mirrWheels As New List(Of Single)
                    For i As Integer = 0 To allWheelsX.Count - 1
                        If Math.Abs(allWheelsX(i)) > 1 Then
                            Dim mirrorX As Single = -allWheelsX(i)
                            Dim hasMirror As Boolean = False
                            For j As Integer = 0 To allWheelsX.Count - 1
                                If Math.Abs(allWheelsX(j) - mirrorX) < 1 Then hasMirror = True : Exit For
                            Next
                            If Not hasMirror Then mirrWheels.Add(mirrorX)
                        End If
                    Next
                    If mirrWheels.Count > 0 Then
                        allWheelsX.AddRange(mirrWheels)
                        reconstructedWheels = True
                    End If
                End If

                ' If mirror inference reconstructed missing wheels, recompute bilateral CDF
                ' analytically with the full wheel set so the curve has the correct shape.
                Dim useReconstructedCDF As Boolean = False
                Dim reconBilateralCDF() As Double = Nothing
                Dim nBilRecon As Integer = 2 * (CDF.NOFF - 1) + 1
                If reconstructedWheels AndAlso allWheelsX.Count > 1 AndAlso det.TireWidth > 0 Then
                    Dim sigma As Double = 30.435
                    Dim halfTW As Double = det.TireWidth / 2.0

                    ' Compute corrected xCenter from full wheel set
                    Dim avgX As Double = 0
                    For Each wx In allWheelsX : avgX += wx : Next
                    avgX /= allWheelsX.Count
                    Dim rightmostX As Double = Single.MinValue
                    For Each wx In allWheelsX
                        If wx > rightmostX Then rightmostX = CDbl(wx)
                    Next
                    Dim notBellyGear As Boolean = (InStr(4, det.ACName, "Belly", CompareMethod.Text) = 0 _
                                                  Or InStr(1, det.ACName, "B747", CompareMethod.Text) > 0 _
                                                  Or InStr(1, det.ACName, "A380", CompareMethod.Text) > 0)
                    Dim corrXCenter As Double
                    If notBellyGear AndAlso det.GearType <> "A" Then
                        corrXCenter = avgX + det.TandemSpacing / 2.0 + Math.Abs(rightmostX)
                    Else
                        corrXCenter = avgX
                    End If

                    ' Recompute bilateral C/P with full wheel set
                    ReDim reconBilateralCDF(nBilRecon - 1)
                    For ib As Integer = 0 To nBilRecon - 1
                        Dim offVal As Double = xMin + ib * CDF.OFFSETINC
                        reconBilateralCDF(ib) = 0
                        For Each wx As Single In allWheelsX
                            Dim wheelOff As Double = corrXCenter + wx
                            Dim yoffR As Double = Math.Abs(offVal - wheelOff)
                            Dim cpR As Double = GaussAreaCalc(yoffR - halfTW, yoffR + halfTW, sigma)
                            Dim yoffL As Double = Math.Abs(offVal + wheelOff)
                            Dim cpL As Double = GaussAreaCalc(yoffL - halfTW, yoffL + halfTW, sigma)
                            reconBilateralCDF(ib) += cpR + cpL
                        Next
                    Next

                    ' Scale to match backend peak CDF
                    Dim storedPk As Double = 0
                    For ioff As Integer = 1 To CDF.NOFF
                        If det.CDFByOffset(ioff) > storedPk Then storedPk = det.CDFByOffset(ioff)
                    Next
                    Dim reconPk As Double = 0
                    For ib As Integer = 0 To nBilRecon - 1
                        If reconBilateralCDF(ib) > reconPk Then reconPk = reconBilateralCDF(ib)
                    Next
                    If reconPk > 0.000001 Then
                        Dim sf As Double = storedPk / reconPk
                        For ib As Integer = 0 To nBilRecon - 1
                            reconBilateralCDF(ib) *= sf
                        Next
                    End If

                    useReconstructedCDF = True
                    hasTireData = True
                End If

                ' --- Per-tire CDF contribution curves ---
                ' Use effective (possibly reconstructed) wheel set for per-tire decomposition
                Dim effectiveXCenter As Double = det.XCenter
                Dim effectiveWheelsX As List(Of Single) = Nothing
                Dim effectiveNWheels As Integer = det.NWheels
                If useReconstructedCDF Then
                    ' Recompute corrected xCenter (same logic as reconstruction above)
                    Dim avgX2 As Double = 0
                    For Each wx In allWheelsX : avgX2 += wx : Next
                    avgX2 /= allWheelsX.Count
                    Dim rmX As Double = Single.MinValue
                    For Each wx In allWheelsX
                        If wx > rmX Then rmX = CDbl(wx)
                    Next
                    Dim nb As Boolean = (InStr(4, det.ACName, "Belly", CompareMethod.Text) = 0 _
                                        Or InStr(1, det.ACName, "B747", CompareMethod.Text) > 0 _
                                        Or InStr(1, det.ACName, "A380", CompareMethod.Text) > 0)
                    If nb AndAlso det.GearType <> "A" Then
                        effectiveXCenter = avgX2 + det.TandemSpacing / 2.0 + Math.Abs(rmX)
                    Else
                        effectiveXCenter = avgX2
                    End If
                    effectiveNWheels = allWheelsX.Count
                    effectiveWheelsX = allWheelsX
                End If

                If hasTireData AndAlso det.TireWidth > 0 Then
                    Dim sigma As Double = 30.435
                    Dim halfTW As Double = det.TireWidth / 2.0
                    Dim nBilateral As Integer = 2 * (CDF.NOFF - 1) + 1  ' 81 points

                    ' Compute per-tire C/P at each bilateral offset
                    Dim nTiresForCurves As Integer = If(effectiveWheelsX IsNot Nothing, effectiveWheelsX.Count, det.NWheels)
                    Dim perTireCtoP(nTiresForCurves, nBilateral - 1) As Double
                    Dim totalCtoP(nBilateral - 1) As Double

                    For ib As Integer = 0 To nBilateral - 1
                        Dim offVal As Double = xMin + ib * CDF.OFFSETINC
                        totalCtoP(ib) = 0
                        For iw As Integer = 1 To nTiresForCurves
                            Dim wx As Double = If(effectiveWheelsX IsNot Nothing, effectiveWheelsX(iw - 1), det.WheelX(iw))
                            Dim wheelOff As Double = effectiveXCenter + wx
                            ' Right-side contribution
                            Dim yoffR As Double = Math.Abs(offVal - wheelOff)
                            Dim cpR As Double = GaussAreaCalc(yoffR - halfTW, yoffR + halfTW, sigma)
                            ' Mirror (left-side) contribution
                            Dim yoffL As Double = Math.Abs(offVal + wheelOff)
                            Dim cpL As Double = GaussAreaCalc(yoffL - halfTW, yoffL + halfTW, sigma)
                            perTireCtoP(iw, ib) = cpR + cpL
                            totalCtoP(ib) += cpR + cpL
                        Next
                    Next

                    ' Scale factor: match peak of recomputed total to stored CDF peak
                    Dim scaleFactor As Double = 1.0
                    Dim storedPeak As Double = 0
                    For ioff As Integer = 1 To CDF.NOFF
                        If det.CDFByOffset(ioff) > storedPeak Then storedPeak = det.CDFByOffset(ioff)
                    Next
                    Dim recompPeak As Double = 0
                    For ib As Integer = 0 To nBilateral - 1
                        If totalCtoP(ib) > recompPeak Then recompPeak = totalCtoP(ib)
                    Next
                    If recompPeak > 0.000001 Then scaleFactor = storedPeak / recompPeak

                    ' Draw tire position bands
                    For iw As Integer = 1 To nTiresForCurves
                        Dim wx As Double = If(effectiveWheelsX IsNot Nothing, effectiveWheelsX(iw - 1), det.WheelX(iw))
                        Dim wheelOff As Double = effectiveXCenter + wx
                        Dim cIdx As Integer = (iw - 1) Mod tireColors.Length
                        ' Right side band
                        Dim bandL As Single = toXPx(wheelOff - halfTW)
                        Dim bandR As Single = toXPx(wheelOff + halfTW)
                        bandL = Math.Max(bandL, CSng(marginLeft))
                        bandR = Math.Min(bandR, CSng(marginLeft + plotWidth))
                        If bandR > bandL Then
                            g.FillRectangle(New SolidBrush(Color.FromArgb(20, tireColors(cIdx).R, tireColors(cIdx).G, tireColors(cIdx).B)),
                                            bandL, CSng(marginTop), bandR - bandL, CSng(plotHeight))
                            Dim centerPx As Single = toXPx(wheelOff)
                            Dim dashPen As New Pen(Color.FromArgb(80, tireColors(cIdx).R, tireColors(cIdx).G, tireColors(cIdx).B), 1.0F)
                            dashPen.DashStyle = Drawing2D.DashStyle.Dash
                            g.DrawLine(dashPen, centerPx, CSng(marginTop), centerPx, CSng(marginTop + plotHeight))
                            dashPen.Dispose()
                        End If
                        ' Mirror (left) side band
                        Dim mBandL As Single = toXPx(-wheelOff - halfTW)
                        Dim mBandR As Single = toXPx(-wheelOff + halfTW)
                        mBandL = Math.Max(mBandL, CSng(marginLeft))
                        mBandR = Math.Min(mBandR, CSng(marginLeft + plotWidth))
                        If mBandR > mBandL Then
                            g.FillRectangle(New SolidBrush(Color.FromArgb(20, tireColors(cIdx).R, tireColors(cIdx).G, tireColors(cIdx).B)),
                                            mBandL, CSng(marginTop), mBandR - mBandL, CSng(plotHeight))
                            Dim mCenterPx As Single = toXPx(-wheelOff)
                            Dim mDashPen As New Pen(Color.FromArgb(80, tireColors(cIdx).R, tireColors(cIdx).G, tireColors(cIdx).B), 1.0F)
                            mDashPen.DashStyle = Drawing2D.DashStyle.Dash
                            g.DrawLine(mDashPen, mCenterPx, CSng(marginTop), mCenterPx, CSng(marginTop + plotHeight))
                            mDashPen.Dispose()
                        End If
                    Next

                    ' Draw per-tire CDF curves (scaled)
                    For iw As Integer = 1 To nTiresForCurves
                        Dim cIdx As Integer = (iw - 1) Mod tireColors.Length
                        Dim tirePen As New Pen(tireColors(cIdx), 1.5F)
                        tirePen.DashStyle = Drawing2D.DashStyle.Dash
                        Dim tirePts(nBilateral - 1) As PointF
                        For ib As Integer = 0 To nBilateral - 1
                            Dim offVal As Double = xMin + ib * CDF.OFFSETINC
                            Dim scaledCDF As Double = perTireCtoP(iw, ib) * scaleFactor
                            tirePts(ib) = New PointF(toXPx(offVal), CSng(marginTop + plotHeight - (scaledCDF / yMax) * plotHeight))
                        Next
                        g.DrawLines(tirePen, tirePts)
                        tirePen.Dispose()
                    Next
                End If

                ' Draw combined CDF curve (reconstructed bilateral if available, else simple mirror)
                Dim dataPen As New Pen(acColor, 2.5F)
                Dim nBilateralPts As Integer = 2 * (CDF.NOFF - 1) + 1
                Dim bilateralPts(nBilateralPts - 1) As PointF
                If useReconstructedCDF AndAlso reconBilateralCDF IsNot Nothing Then
                    ' Use analytically recomputed bilateral CDF (correct unimodal shape)
                    For ib As Integer = 0 To nBilateralPts - 1
                        Dim offVal As Double = xMin + ib * CDF.OFFSETINC
                        Dim cdfVal As Double = reconBilateralCDF(ib)
                        bilateralPts(ib) = New PointF(toXPx(offVal), CSng(marginTop + plotHeight - (cdfVal / yMax) * plotHeight))
                    Next
                Else
                    ' Simple mirror of stored half-array (symmetric gears with complete wheel data)
                    For ib As Integer = 0 To nBilateralPts - 1
                        Dim offVal As Double = xMin + ib * CDF.OFFSETINC
                        Dim absOff As Integer = Math.Abs(ib - (CDF.NOFF - 1))  ' maps to 0..NOFF-1
                        Dim storedIdx As Integer = absOff + 1  ' 1-indexed
                        Dim cdfVal As Double = 0
                        If storedIdx >= 1 AndAlso storedIdx <= CDF.NOFF Then cdfVal = det.CDFByOffset(storedIdx)
                        bilateralPts(ib) = New PointF(toXPx(offVal), CSng(marginTop + plotHeight - (cdfVal / yMax) * plotHeight))
                    Next
                End If
                ' Fill area under bilateral CDF curve
                If bilateralPts.Length > 1 Then
                    Dim fillPts As New List(Of PointF)
                    fillPts.Add(New PointF(bilateralPts(0).X, CSng(marginTop + plotHeight)))
                    fillPts.AddRange(bilateralPts)
                    fillPts.Add(New PointF(bilateralPts(bilateralPts.Length - 1).X, CSng(marginTop + plotHeight)))
                    Dim fillBrush As New SolidBrush(Color.FromArgb(25, acColor.R, acColor.G, acColor.B))
                    g.FillPolygon(fillBrush, fillPts.ToArray())
                    fillBrush.Dispose()
                End If
                g.DrawLines(dataPen, bilateralPts)

                ' Draw zero-line (centerline at offset=0)
                Dim zeroXPx As Single = toXPx(0)
                Dim zeroPen As New Pen(Color.Black, 1.5F)
                g.DrawLine(zeroPen, zeroXPx, marginTop, zeroXPx, marginTop + plotHeight)
                zeroPen.Dispose()

                ' Draw critical offset vertical lines (red dashed, both sides)
                Dim critOffsetVal As Double = (criticalOffset - 1) * CDF.OFFSETINC
                Dim critPen As New Pen(Color.Red, 1.5F)
                critPen.DashStyle = Drawing2D.DashStyle.Dash
                Dim critXPxR As Single = toXPx(critOffsetVal)
                Dim critXPxL As Single = toXPx(-critOffsetVal)
                g.DrawLine(critPen, critXPxR, marginTop, critXPxR, marginTop + plotHeight)
                If critOffsetVal > 0 Then g.DrawLine(critPen, critXPxL, marginTop, critXPxL, marginTop + plotHeight)

                ' Draw max CDF marker (red diamond) on both sides
                Dim maxCDFIdx As Integer = 1
                Dim maxCDFVal As Double = 0
                For ioff As Integer = 1 To CDF.NOFF
                    If det.CDFByOffset(ioff) > maxCDFVal Then
                        maxCDFVal = det.CDFByOffset(ioff)
                        maxCDFIdx = ioff
                    End If
                Next
                Dim diamondSize As Integer = 7
                ' Right side diamond
                Dim maxXPxR As Single = toXPx((maxCDFIdx - 1) * CDF.OFFSETINC)
                Dim maxYPx As Single = CSng(marginTop + plotHeight - (maxCDFVal / yMax) * plotHeight)
                Dim diamondPtsR() As PointF = {
                    New PointF(maxXPxR, maxYPx - diamondSize),
                    New PointF(maxXPxR + diamondSize, maxYPx),
                    New PointF(maxXPxR, maxYPx + diamondSize),
                    New PointF(maxXPxR - diamondSize, maxYPx)
                }
                g.FillPolygon(Brushes.Red, diamondPtsR)
                ' Left side diamond (mirror)
                Dim maxXPxL As Single = toXPx(-CDbl((maxCDFIdx - 1) * CDF.OFFSETINC))
                Dim diamondPtsL() As PointF = {
                    New PointF(maxXPxL, maxYPx - diamondSize),
                    New PointF(maxXPxL + diamondSize, maxYPx),
                    New PointF(maxXPxL, maxYPx + diamondSize),
                    New PointF(maxXPxL - diamondSize, maxYPx)
                }
                g.FillPolygon(Brushes.Red, diamondPtsL)

                ' Draw legend
                Dim legendFont As New Font("Segoe UI", 8.0F)
                Dim legendLineH As Integer = 15
                Dim nLegendEntries As Integer = 4
                Dim legendTireCount As Integer = If(effectiveWheelsX IsNot Nothing, effectiveWheelsX.Count, det.NWheels)
                If hasTireData AndAlso det.TireWidth > 0 Then nLegendEntries += Math.Min(legendTireCount, tireColors.Length) + 1
                Dim legendW As Integer = 220
                Dim legendH As Integer = nLegendEntries * legendLineH + 10
                Dim legendX As Integer = marginLeft + plotWidth - legendW - 10
                Dim legendY As Integer = marginTop + 10
                Dim legendBg As New Rectangle(legendX, legendY, legendW, legendH)
                g.FillRectangle(New SolidBrush(Color.FromArgb(240, 245, 245, 245)), legendBg)
                g.DrawRectangle(New Pen(Color.FromArgb(200, 204, 210), 1), legendBg)

                Dim ly As Integer = legendY + 5

                ' Combined CDF curve
                g.DrawLine(dataPen, legendX + 6, ly + 6, legendX + 24, ly + 6)
                g.DrawString("Combined CDF", legendFont, Brushes.Black, legendX + 28, ly)
                ly += legendLineH

                ' Max CDF diamond
                Dim ldPts() As PointF = {
                    New PointF(legendX + 15, ly + 1),
                    New PointF(legendX + 21, ly + 6),
                    New PointF(legendX + 15, ly + 11),
                    New PointF(legendX + 9, ly + 6)
                }
                g.FillPolygon(Brushes.Red, ldPts)
                g.DrawString("Max CDF = " & Format(maxCDFVal, "0.000000"), legendFont, Brushes.Black, legendX + 28, ly)
                ly += legendLineH

                ' Critical offset
                g.DrawLine(critPen, CSng(legendX + 6), CSng(ly + 6), CSng(legendX + 24), CSng(ly + 6))
                g.DrawString("Critical offset (" & Format(critOffsetVal, "0") & " " & offsetUnit & ")", legendFont, Brushes.Black, legendX + 28, ly)
                ly += legendLineH

                ' Centerline
                Dim lgZeroPen As New Pen(Color.Black, 1.5F)
                g.DrawLine(lgZeroPen, CSng(legendX + 6), CSng(ly + 6), CSng(legendX + 24), CSng(ly + 6))
                g.DrawString("Centerline (0)", legendFont, Brushes.Black, legendX + 28, ly)
                lgZeroPen.Dispose()
                ly += legendLineH

                ' Per-tire entries
                If hasTireData AndAlso det.TireWidth > 0 Then
                    For iw As Integer = 1 To Math.Min(legendTireCount, tireColors.Length)
                        Dim cIdx As Integer = (iw - 1) Mod tireColors.Length
                        Dim tLgPen As New Pen(tireColors(cIdx), 1.5F)
                        tLgPen.DashStyle = Drawing2D.DashStyle.Dash
                        g.DrawLine(tLgPen, CSng(legendX + 6), CSng(ly + 6), CSng(legendX + 24), CSng(ly + 6))
                        Dim wx As Double = If(effectiveWheelsX IsNot Nothing, effectiveWheelsX(iw - 1), det.WheelX(iw))
                        Dim wheelPos As Double = effectiveXCenter + wx
                        g.DrawString("Tire " & iw & " (x=" & Format(wheelPos, "0.0") & ")", legendFont, Brushes.Black, legendX + 28, ly)
                        tLgPen.Dispose()
                        ly += legendLineH
                    Next
                    ' Tire band entry
                    g.FillRectangle(New SolidBrush(Color.FromArgb(30, 100, 100, 100)), legendX + 8, ly + 2, 14, 10)
                    g.DrawString("Tire contact band", legendFont, Brushes.Black, legendX + 28, ly)
                End If

                ' Cleanup GDI+ objects
                titleFont.Dispose()
                axisFont.Dispose()
                labelFont.Dispose()
                legendFont.Dispose()
                gridPen.Dispose()
                dataPen.Dispose()
                critPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws a composite CDF chart superposing all aircraft CDF curves and the cumulative CDF.
        ''' </summary>
        Private Function DrawCompositeCDFChart(rpt As clsDetailedReportData, offsetUnit As String, chartColors() As Color) As Bitmap
            Dim chartWidth As Integer = 900
            Dim chartHeight As Integer = 550
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                ' Chart margins
                Dim marginLeft As Integer = 85
                Dim marginRight As Integer = 35
                Dim marginTop As Integer = 50
                Dim marginBottom As Integer = 70
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Draw title
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleText = "CDF Distribution Across Pavement Width"
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 10)

                ' Determine y-axis range from cumulative CDF
                Dim maxDataCDF As Double = 0
                For ioff As Integer = 1 To CDF.NOFF
                    If rpt.CDFSweep.CDFTotalPerOffset(ioff) > maxDataCDF Then
                        maxDataCDF = rpt.CDFSweep.CDFTotalPerOffset(ioff)
                    End If
                Next
                If maxDataCDF <= 0 Then maxDataCDF = 0.001
                Dim yMax As Double = maxDataCDF * 1.15
                Dim xMax As Double = (CDF.NOFF - 1) * CDF.OFFSETINC

                ' Draw plot area
                Dim plotRect As New Rectangle(marginLeft, marginTop, plotWidth, plotHeight)
                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), plotRect)
                g.DrawRectangle(Pens.DarkGray, plotRect)

                ' Draw gridlines
                Dim gridPen As New Pen(Color.FromArgb(210, 214, 220), 1)
                gridPen.DashStyle = Drawing2D.DashStyle.Dot
                Dim nGridY As Integer = 5
                Dim axisFont As New Font("Segoe UI", 8.5F)
                Dim labelFont As New Font("Segoe UI", 9.5F)

                ' Draw Y=0 baseline explicitly
                Dim zeroYPx As Integer = CInt(marginTop + plotHeight)
                g.DrawLine(New Pen(Color.FromArgb(180, 180, 180), 1), marginLeft, zeroYPx, marginLeft + plotWidth, zeroYPx)

                For i As Integer = 0 To nGridY
                    Dim yVal As Double = yMax * i / nGridY
                    Dim yPx As Integer = CInt(marginTop + plotHeight - (yVal / yMax) * plotHeight)
                    If i > 0 AndAlso i < nGridY Then g.DrawLine(gridPen, marginLeft, yPx, marginLeft + plotWidth, yPx)
                    Dim yLabel = Format(yVal, "0.0000")
                    Dim yLabelSize = g.MeasureString(yLabel, axisFont)
                    g.DrawString(yLabel, axisFont, Brushes.Black, marginLeft - yLabelSize.Width - 4, yPx - yLabelSize.Height / 2)
                Next

                Dim nGridX As Integer = 8
                For i As Integer = 0 To nGridX
                    Dim xVal As Double = xMax * i / nGridX
                    Dim xPx As Integer = CInt(marginLeft + (xVal / xMax) * plotWidth)
                    If i > 0 AndAlso i < nGridX Then g.DrawLine(gridPen, xPx, marginTop, xPx, marginTop + plotHeight)
                    Dim xLabel = Format(xVal, "0")
                    Dim xLabelSize = g.MeasureString(xLabel, axisFont)
                    g.DrawString(xLabel, axisFont, Brushes.Black, xPx - xLabelSize.Width / 2, marginTop + plotHeight + 4)
                Next

                ' Axis labels
                Dim xAxisLabel = "Offset (" & offsetUnit & ")"
                Dim xAxisSize = g.MeasureString(xAxisLabel, labelFont)
                g.DrawString(xAxisLabel, labelFont, Brushes.Black, CSng(marginLeft + (plotWidth - xAxisSize.Width) / 2), chartHeight - 25)

                g.TranslateTransform(30, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(-90)
                Dim yAxisLabel = "CDF"
                Dim yAxisSize = g.MeasureString(yAxisLabel, labelFont)
                g.DrawString(yAxisLabel, labelFont, Brushes.Black, -yAxisSize.Width / 2, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' Draw per-aircraft CDF curves
                Dim nAC As Integer = rpt.CDFSweep.NAircraftCaptured
                Dim legendEntries As New List(Of Tuple(Of String, Color))

                For ia As Integer = 1 To nAC
                    Dim acColor As Color = chartColors((ia - 1) Mod chartColors.Length)
                    Dim acPen As New Pen(acColor, 1.8F)

                    Dim acName As String = "AC" & ia.ToString()
                    If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                        acName = rpt.AircraftDetails(ia).ACName
                    End If

                    Dim points(CDF.NOFF - 1) As PointF
                    For ioff As Integer = 1 To CDF.NOFF
                        Dim xVal As Double = (ioff - 1) * CDF.OFFSETINC
                        Dim xPx As Single = CSng(marginLeft + (xVal / xMax) * plotWidth)
                        Dim yPx As Single = CSng(marginTop + plotHeight - (rpt.CDFSweep.CDFPerAircraftPerOffset(ia, ioff) / yMax) * plotHeight)
                        points(ioff - 1) = New PointF(xPx, yPx)
                    Next
                    If points.Length > 1 Then g.DrawLines(acPen, points)
                    legendEntries.Add(New Tuple(Of String, Color)(acName, acColor))
                    acPen.Dispose()
                Next

                ' Draw cumulative CDF curve (thick black line)
                Dim cumPen As New Pen(Color.Black, 3.0F)
                Dim cumPoints(CDF.NOFF - 1) As PointF
                For ioff As Integer = 1 To CDF.NOFF
                    Dim xVal As Double = (ioff - 1) * CDF.OFFSETINC
                    Dim xPx As Single = CSng(marginLeft + (xVal / xMax) * plotWidth)
                    Dim yPx As Single = CSng(marginTop + plotHeight - (rpt.CDFSweep.CDFTotalPerOffset(ioff) / yMax) * plotHeight)
                    cumPoints(ioff - 1) = New PointF(xPx, yPx)
                Next
                ' Fill area under cumulative CDF curve
                If cumPoints.Length > 1 Then
                    Dim cumFillPts As New List(Of PointF)
                    cumFillPts.Add(New PointF(cumPoints(0).X, CSng(marginTop + plotHeight)))
                    cumFillPts.AddRange(cumPoints)
                    cumFillPts.Add(New PointF(cumPoints(cumPoints.Length - 1).X, CSng(marginTop + plotHeight)))
                    Dim cumFillBrush As New SolidBrush(Color.FromArgb(18, 0, 0, 0))
                    g.FillPolygon(cumFillBrush, cumFillPts.ToArray())
                    cumFillBrush.Dispose()
                End If

                If cumPoints.Length > 1 Then g.DrawLines(cumPen, cumPoints)
                legendEntries.Add(New Tuple(Of String, Color)("Cumulative CDF", Color.Black))

                ' Draw critical offset vertical line (red dashed)
                Dim critOffsetVal As Double = (rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC
                Dim critXPx As Single = CSng(marginLeft + (critOffsetVal / xMax) * plotWidth)
                Dim critPen As New Pen(Color.Red, 1.5F)
                critPen.DashStyle = Drawing2D.DashStyle.Dash
                g.DrawLine(critPen, critXPx, marginTop, critXPx, marginTop + plotHeight)

                ' Draw max cumulative CDF point (large red dot)
                Dim maxCumCDFVal As Double = rpt.CDFSweep.MaxCDF
                Dim maxCumYPx As Single = CSng(marginTop + plotHeight - (maxCumCDFVal / yMax) * plotHeight)
                g.FillEllipse(Brushes.Red, critXPx - 6, maxCumYPx - 6, 12, 12)

                ' Draw legend — measure content width dynamically
                Dim legendFont As New Font("Segoe UI", 8.0F)
                Dim legendLineH As Integer = 14
                Dim legendH As Integer = legendEntries.Count * legendLineH + 8

                ' Measure widest legend entry
                Dim maxEntryW As Single = 0
                For Each entry In legendEntries
                    Dim entrySize = g.MeasureString(entry.Item1, legendFont)
                    If entrySize.Width > maxEntryW Then maxEntryW = entrySize.Width
                Next
                Dim legendW As Integer = CInt(maxEntryW) + 40
                Dim legendX As Integer = marginLeft + plotWidth - legendW - 8
                Dim legendY As Integer = marginTop + plotHeight - legendH - 8
                Dim legendBg As New Rectangle(legendX, legendY, legendW, legendH)
                g.FillRectangle(New SolidBrush(Color.FromArgb(240, 245, 245, 245)), legendBg)
                g.DrawRectangle(New Pen(Color.FromArgb(200, 204, 210), 1), legendBg)

                For idx As Integer = 0 To legendEntries.Count - 1
                    Dim entry = legendEntries(idx)
                    Dim ly As Integer = legendY + 4 + idx * legendLineH
                    Dim lPen As New Pen(entry.Item2, If(entry.Item2 = Color.Black, 2.5F, 1.5F))
                    g.DrawLine(lPen, legendX + 6, ly + 5, legendX + 22, ly + 5)
                    g.DrawString(entry.Item1, legendFont, Brushes.Black, legendX + 26, ly)
                    lPen.Dispose()
                Next

                ' Add critical strip label
                Dim critFont As New Font("Segoe UI", 7.5F, FontStyle.Italic)
                Dim critLabel = "Critical: " & Format(critOffsetVal, "0") & " " & offsetUnit
                g.DrawString(critLabel, critFont, Brushes.Red, critXPx + 4, marginTop + 4)

                ' Cleanup GDI+ objects
                titleFont.Dispose()
                axisFont.Dispose()
                labelFont.Dispose()
                legendFont.Dispose()
                critFont.Dispose()
                gridPen.Dispose()
                cumPen.Dispose()
                critPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws a string with ^{...} markers rendered as superscripts (60% size, raised 35%).
        ''' Returns the total width drawn.
        ''' </summary>
        Private Function DrawFormattedString(g As Graphics, text As String, baseFont As Font, brush As Brush, x As Single, y As Single) As Single
            Dim totalWidth As Single = 0
            Dim superFontSize As Single = baseFont.Size * 0.6F
            Dim superShift As Single = baseFont.GetHeight(g) * 0.35F

            Using superFont As New Font(baseFont.FontFamily, superFontSize, baseFont.Style)
                Dim pos As Integer = 0
                While pos < text.Length
                    Dim caretIdx As Integer = text.IndexOf("^{", pos)
                    If caretIdx < 0 Then
                        ' No more superscripts — draw remainder
                        Dim remainder = text.Substring(pos)
                        g.DrawString(remainder, baseFont, brush, x + totalWidth, y)
                        totalWidth += g.MeasureString(remainder, baseFont).Width - 6 ' trim GDI+ padding
                        Exit While
                    End If

                    ' Draw normal text before the ^{
                    If caretIdx > pos Then
                        Dim normal = text.Substring(pos, caretIdx - pos)
                        g.DrawString(normal, baseFont, brush, x + totalWidth, y)
                        totalWidth += g.MeasureString(normal, baseFont).Width - 6
                    End If

                    ' Find closing brace (handle nested ^{...} by counting braces)
                    Dim braceStart As Integer = caretIdx + 2
                    Dim depth As Integer = 1
                    Dim braceEnd As Integer = braceStart
                    While braceEnd < text.Length AndAlso depth > 0
                        If text(braceEnd) = "{"c Then depth += 1
                        If text(braceEnd) = "}"c Then depth -= 1
                        braceEnd += 1
                    End While
                    Dim superText = text.Substring(braceStart, braceEnd - braceStart - 1)

                    ' Recursively draw superscript content (handles nested ^{})
                    Dim superWidth = DrawFormattedString(g, superText, superFont, brush, x + totalWidth, y - superShift)
                    totalWidth += superWidth
                    pos = braceEnd
                End While
            End Using

            Return totalWidth
        End Function

        ''' <summary>
        ''' Measures the width of a formatted string with ^{...} superscript markers.
        ''' </summary>
        Private Function MeasureFormattedString(g As Graphics, text As String, baseFont As Font) As Single
            Dim totalWidth As Single = 0
            Dim superFontSize As Single = baseFont.Size * 0.6F

            Using superFont As New Font(baseFont.FontFamily, superFontSize, baseFont.Style)
                Dim pos As Integer = 0
                While pos < text.Length
                    Dim caretIdx As Integer = text.IndexOf("^{", pos)
                    If caretIdx < 0 Then
                        totalWidth += g.MeasureString(text.Substring(pos), baseFont).Width - 6
                        Exit While
                    End If
                    If caretIdx > pos Then
                        totalWidth += g.MeasureString(text.Substring(pos, caretIdx - pos), baseFont).Width - 6
                    End If
                    Dim braceStart As Integer = caretIdx + 2
                    Dim depth As Integer = 1
                    Dim braceEnd As Integer = braceStart
                    While braceEnd < text.Length AndAlso depth > 0
                        If text(braceEnd) = "{"c Then depth += 1
                        If text(braceEnd) = "}"c Then depth -= 1
                        braceEnd += 1
                    End While
                    Dim superText = text.Substring(braceStart, braceEnd - braceStart - 1)
                    totalWidth += MeasureFormattedString(g, superText, superFont)
                    pos = braceEnd
                End While
            End Using

            Return totalWidth
        End Function

        ''' <summary>
        ''' Renders mathematical equations as a high-quality bitmap using GDI+.
        ''' </summary>
        Private Function DrawEquationImage(title As String, equationLines() As String, Optional width As Integer = 750) As Bitmap
            Dim scale As Integer = 3 ' render at 3x for extra crispness
            Dim intW As Integer = width * scale
            Dim titleFontSize As Single = 8.5F * scale
            Dim eqFontSize As Single = 9.0F * scale
            Dim lineSpacing As Integer = CInt(eqFontSize * 1.65)
            Dim topPad As Integer = 10 * scale
            Dim leftPad As Integer = 20 * scale

            ' Equations - try Cambria Math, fall back to Consolas
            Dim eqFontFamily As String = "Cambria Math"
            Try
                Using testFont As New Font(eqFontFamily, 12)
                    If testFont.Name <> eqFontFamily Then eqFontFamily = "Consolas"
                End Using
            Catch
                eqFontFamily = "Consolas"
            End Try

            ' Pre-measure title to get accurate underline position
            Dim titleFont As New Font("Segoe UI", titleFontSize, FontStyle.Bold)
            Dim eqFont As New Font(eqFontFamily, eqFontSize, FontStyle.Regular)

            ' Calculate height using measurement
            Dim titleH As Integer
            Using tmpBmp As New Bitmap(1, 1)
                Using tmpG As Graphics = Graphics.FromImage(tmpBmp)
                    Dim titleMeasured = tmpG.MeasureString(title, titleFont)
                    titleH = CInt(titleMeasured.Height)
                End Using
            End Using
            Dim underGap As Integer = CInt(4 * scale)
            Dim bodyTopGap As Integer = CInt(8 * scale)
            Dim bodyH As Integer = equationLines.Length * lineSpacing + 6 * scale
            Dim intH As Integer = topPad + titleH + underGap + bodyTopGap + bodyH + 10 * scale

            Dim bmpHi As New Bitmap(intW, intH)
            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit

                ' Subtle gradient background
                Using gradBrush As New Drawing2D.LinearGradientBrush(
                    New Point(0, 0), New Point(0, intH),
                    Color.FromArgb(252, 252, 254), Color.FromArgb(245, 246, 250))
                    g.FillRectangle(gradBrush, 0, 0, intW, intH)
                End Using

                ' Left accent stripe
                Using accentBrush As New SolidBrush(Color.FromArgb(46, 94, 168))
                    g.FillRectangle(accentBrush, 0, 0, 4 * scale, intH)
                End Using

                ' Border
                Using borderPen As New Pen(Color.FromArgb(200, 206, 214), scale),
                      titleBrush As New SolidBrush(Color.FromArgb(26, 58, 110)),
                      underPen As New Pen(Color.FromArgb(46, 94, 168), CInt(scale * 1.0)),
                      eqBrush As New SolidBrush(Color.FromArgb(31, 41, 55)),
                      computedBrush As New SolidBrush(Color.FromArgb(80, 100, 130))

                    g.DrawRectangle(borderPen, 0, 0, intW - 1, intH - 1)

                    ' Title
                    g.DrawString(title, titleFont, titleBrush, leftPad, topPad)

                    ' Underline — positioned using measured title height
                    Dim underY As Integer = topPad + titleH + underGap
                    g.DrawLine(underPen, leftPad, underY, intW - leftPad, underY)

                    Dim yPos As Integer = underY + bodyTopGap

                    For Each line In equationLines
                        ' Use dimmer color for "For this structure" computed values
                        Dim brush As SolidBrush = eqBrush
                        If line.StartsWith("For this") OrElse line.StartsWith("   ") Then
                            brush = computedBrush
                        End If
                        DrawFormattedString(g, line, eqFont, brush, leftPad + 10 * scale, yPos)
                        yPos += lineSpacing
                    Next
                End Using
            End Using

            titleFont.Dispose()
            eqFont.Dispose()

            ' Scale down to target size
            Dim finalW As Integer = intW \ scale
            Dim finalH As Integer = intH \ scale
            Dim bmp As New Bitmap(finalW, finalH)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.DrawImage(bmpHi, 0, 0, finalW, finalH)
            End Using
            bmpHi.Dispose()
            Return bmp
        End Function


        ''' <summary>
        ''' Draws a pavement cross-section diagram with tire projection for an aircraft.
        ''' </summary>
        Private Function DrawPavementCrossSection(rpt As clsDetailedReportData, det As clsAircraftDetail, thicknessUnit As String, lengthUnit As String) As Bitmap
            Dim chartWidth As Integer = 900
            Dim chartHeight As Integer = 600
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                ' Title
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleText = "Pavement Cross-Section & Tire Stress Projection — " & det.ACName
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 8)

                ' Layout regions
                Dim layerX As Integer = 50 ' left edge of layer stack
                Dim layerW As Integer = 320 ' width of layer rectangles
                Dim projX As Integer = 480 ' left edge of projection diagram
                Dim projW As Integer = 370 ' width of projection area
                Dim topY As Integer = 50
                Dim labelFont As New Font("Segoe UI", 8.5F)
                Dim labelFontBold As New Font("Segoe UI", 8.5F, FontStyle.Bold)
                Dim dimFont As New Font("Segoe UI", 7.5F, FontStyle.Italic)

                ' Layer colors by type
                Dim layerColors() As Color = {
                    Color.FromArgb(80, 80, 80),       ' HMA - dark gray
                    Color.FromArgb(194, 178, 128),     ' Base - sandy
                    Color.FromArgb(210, 180, 140),     ' Subbase - tan
                    Color.FromArgb(139, 119, 101),     ' Stabilized - brown
                    Color.FromArgb(120, 150, 90)       ' Subgrade - green
                }

                ' Calculate total depth for scaling
                Dim totalDepth As Single = 0
                Dim layers = rpt.SublayerData.DesignLayers
                For i As Integer = 0 To layers.Count - 2 ' exclude semi-infinite subgrade
                    totalDepth += layers(i).Thickness
                Next
                If totalDepth < 1 Then totalDepth = 24 ' fallback
                Dim subgradeDisplay As Single = totalDepth * 0.35F ' show some subgrade
                Dim displayDepth As Single = totalDepth + subgradeDisplay
                Dim availH As Integer = chartHeight - topY - 80
                Dim pxPerInch As Single = availH / displayDepth

                ' ===== LEFT: Draw layer stack =====
                Dim yAccum As Single = topY
                Dim layerIdx As Integer = 0
                For Each layer In layers
                    Dim colorIdx As Integer = Math.Min(layerIdx, layerColors.Length - 1)
                    Dim lColor = layerColors(colorIdx)
                    Dim lBrush As New SolidBrush(Color.FromArgb(180, lColor.R, lColor.G, lColor.B))
                    Dim lH As Single
                    Dim isSubgrade As Boolean = (layerIdx = layers.Count - 1)

                    If isSubgrade Then
                        lH = subgradeDisplay * pxPerInch
                    Else
                        lH = layer.Thickness * pxPerInch
                    End If

                    Dim layerRect As New RectangleF(layerX, yAccum, layerW, lH)
                    g.FillRectangle(lBrush, layerRect)
                    g.DrawRectangle(Pens.DarkGray, layerX, CInt(yAccum), layerW, CInt(lH))

                    ' Label
                    Dim lbl As String
                    If isSubgrade Then
                        lbl = "Layer " & (layerIdx + 1) & ": Subgrade, E=" & Format(layer.Modulus, "#,##0") & " " & thicknessUnit
                    Else
                        lbl = "Layer " & (layerIdx + 1) & ": " & Format(layer.Thickness, "0.0") & " " & thicknessUnit & ", E=" & Format(layer.Modulus, "#,##0")
                    End If
                    Dim lblSize = g.MeasureString(lbl, labelFont)
                    Dim lblY As Single = yAccum + (lH - lblSize.Height) / 2
                    If lH > lblSize.Height Then
                        g.DrawString(lbl, labelFont, Brushes.White, layerX + 8, lblY)
                    End If

                    ' Thickness dimension on right side
                    If Not isSubgrade Then
                        Dim dimTxt = Format(layer.Thickness, "0.00") & Chr(34)
                        Dim dimSize = g.MeasureString(dimTxt, dimFont)
                        g.DrawString(dimTxt, dimFont, Brushes.Black, layerX + layerW + 5, yAccum + (lH - dimSize.Height) / 2)
                    End If

                    yAccum += lH
                    layerIdx += 1
                    lBrush.Dispose()
                Next

                ' ===== RIGHT: Tire projection diagram =====
                Dim projCenterX As Single = projX + projW / 2
                Dim surfaceY As Single = topY
                Dim subgradeY As Single = topY + totalDepth * pxPerInch

                ' Build list of all physical tire lateral positions (stored + inferred dual partners)
                Dim tirePositions As New List(Of Single)
                If det.NWheels > 0 AndAlso det.WheelX IsNot Nothing Then
                    For i As Integer = 1 To det.NWheels
                        tirePositions.Add(det.WheelX(i))
                    Next
                End If
                If det.DualSpacing > 0 Then
                    Dim newPartners As New List(Of Single)
                    For i As Integer = 0 To tirePositions.Count - 1
                        Dim partnerX As Single = tirePositions(i) + det.DualSpacing
                        Dim exists As Boolean = False
                        For j As Integer = 0 To tirePositions.Count - 1
                            If j <> i AndAlso Math.Abs(tirePositions(j) - partnerX) < 1 Then exists = True : Exit For
                        Next
                        If Not exists Then newPartners.Add(partnerX)
                    Next
                    tirePositions.AddRange(newPartners)
                End If
                If tirePositions.Count = 0 Then tirePositions.Add(0) ' fallback

                ' Determine coordinate range needed (all tires + projected widths)
                Dim tireMinX As Single = Single.MaxValue, tireMaxX As Single = Single.MinValue
                For Each tp In tirePositions
                    If tp < tireMinX Then tireMinX = tp
                    If tp > tireMaxX Then tireMaxX = tp
                Next
                ' Total span includes projected widths at subgrade
                Dim halfProj As Single = CSng(det.ProjectedTireWidthAtSubgrade / 2.0)
                Dim spanMin As Single = tireMinX - halfProj
                Dim spanMax As Single = tireMaxX + halfProj
                Dim totalSpan As Single = Math.Max(spanMax - spanMin, det.ProjectedTireWidthAtSubgrade)
                Dim inchesPerPx As Single = totalSpan * 1.3F / projW ' 1.3 = margin factor

                ' Map function: inches (relative to gear center) → pixel X
                Dim gearCenterInches As Single = (tireMinX + tireMaxX) / 2
                Dim toProjPx = Function(xInches As Single) projCenterX + (xInches - gearCenterInches) / inchesPerPx

                ' Draw pavement depth region
                Dim pavRect As New RectangleF(projX, surfaceY, projW, (subgradeY - surfaceY))
                g.FillRectangle(New SolidBrush(Color.FromArgb(30, 150, 150, 150)), pavRect)
                g.DrawLine(Pens.Black, projX, surfaceY, projX + projW, surfaceY)
                g.DrawLine(New Pen(Color.FromArgb(120, 150, 90), 2), projX, subgradeY, projX + projW, subgradeY)

                ' Labels
                g.DrawString("Surface", labelFontBold, Brushes.Black, projX + 2, surfaceY - 14)
                g.DrawString("Subgrade (Eval Depth = " & Format(rpt.SublayerData.EvalDepthSubgrade, "0.0") & " " & thicknessUnit & ")", labelFont, New SolidBrush(Color.FromArgb(120, 150, 90)), projX + 2, subgradeY + 2)

                ' Draw each tire with its projection cone
                Dim tireColors() As Color = {
                    Color.FromArgb(40, 40, 40),       ' dark gray
                    Color.FromArgb(46, 94, 168)        ' blue
                }
                Dim spreadColors() As Color = {
                    Color.FromArgb(214, 39, 40),       ' red
                    Color.FromArgb(255, 127, 14)        ' orange
                }
                Dim halfTW As Single = det.TireWidth / 2.0F
                Dim tireBrush As New SolidBrush(Color.FromArgb(200, 40, 40, 40))

                For iTire As Integer = 0 To tirePositions.Count - 1
                    Dim tireX As Single = tirePositions(iTire)
                    Dim tcIdx As Integer = iTire Mod tireColors.Length
                    Dim scIdx As Integer = iTire Mod spreadColors.Length

                    ' Tire rectangle at surface
                    Dim tLeft As Single = toProjPx(tireX - halfTW)
                    Dim tRight As Single = toProjPx(tireX + halfTW)
                    Dim tW As Single = Math.Max(tRight - tLeft, 8)
                    g.FillRectangle(New SolidBrush(Color.FromArgb(200, tireColors(tcIdx).R, tireColors(tcIdx).G, tireColors(tcIdx).B)),
                                    tLeft, surfaceY - 12, tW, 12)
                    If iTire = 0 Then
                        g.DrawString("TW=" & Format(det.TireWidth, "0.0") & Chr(34), dimFont, Brushes.Black, tRight + 3, surfaceY - 14)
                    End If

                    ' Projected width at subgrade for this tire
                    Dim pLeft As Single = toProjPx(tireX - halfProj)
                    Dim pRight As Single = toProjPx(tireX + halfProj)

                    ' 45° stress spread lines
                    Dim spreadPen As New Pen(Color.FromArgb(150, spreadColors(scIdx).R, spreadColors(scIdx).G, spreadColors(scIdx).B), 1.5F)
                    spreadPen.DashStyle = Drawing2D.DashStyle.DashDot
                    g.DrawLine(spreadPen, tLeft, surfaceY, pLeft, subgradeY)
                    g.DrawLine(spreadPen, tLeft + tW, surfaceY, pRight, subgradeY)

                    ' Projected width bar at subgrade
                    Dim projBarPen As New Pen(Color.FromArgb(200, spreadColors(scIdx).R, spreadColors(scIdx).G, spreadColors(scIdx).B), 2)
                    g.DrawLine(projBarPen, pLeft, subgradeY, pRight, subgradeY)

                    spreadPen.Dispose()
                    projBarPen.Dispose()
                Next

                ' Projected width label
                g.DrawString("Proj. Width=" & Format(det.ProjectedTireWidthAtSubgrade, "0.0") & Chr(34) & " (per tire)",
                             dimFont, Brushes.Red, projCenterX, subgradeY + 14)

                ' Show overlap region if projections overlap
                If tirePositions.Count >= 2 Then
                    tirePositions.Sort()
                    Dim gap As Single = (tirePositions(1) - tirePositions(0)) - CSng(det.ProjectedTireWidthAtSubgrade)
                    If gap < 0 Then
                        ' Projections overlap — shade the overlap region
                        Dim overlapLeft As Single = toProjPx(tirePositions(1) - halfProj)
                        Dim overlapRight As Single = toProjPx(tirePositions(0) + halfProj)
                        If overlapRight > overlapLeft Then
                            Dim overlapBrush As New SolidBrush(Color.FromArgb(40, 255, 0, 0))
                            g.FillRectangle(overlapBrush, overlapLeft, subgradeY - 6, overlapRight - overlapLeft, 12)
                            overlapBrush.Dispose()
                            Dim overlapLabel = "Overlap: " & Format(Math.Abs(gap), "0.0") & Chr(34)
                            g.DrawString(overlapLabel, dimFont, Brushes.Red, (overlapLeft + overlapRight) / 2 - 20, subgradeY + 26)
                        End If
                    Else
                        Dim gapLabel = "Gap: " & Format(gap, "0.0") & Chr(34)
                        g.DrawString(gapLabel, dimFont, Brushes.DarkGray, projCenterX - 15, subgradeY + 26)
                    End If
                End If

                ' Depth dimension
                Dim depthMidY As Single = (surfaceY + subgradeY) / 2
                g.DrawString("D=" & Format(totalDepth, "0.0") & Chr(34), dimFont, Brushes.DarkGray, projX + projW + 5, depthMidY - 6)

                ' Cleanup
                titleFont.Dispose()
                labelFont.Dispose()
                labelFontBold.Dispose()
                dimFont.Dispose()
                tireBrush.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Computes NtoFail for a given absolute strain using the specified subgrade model.
        ''' </summary>
        Private Function ComputeNtoFail(strainAbs As Double, modelName As String, subMod As Double) As Double
            If strainAbs < 0.00001 Then strainAbs = 0.00001

            If modelName = "Bleasdale" Then
                ' Bleasdale 2 model (modCDF.vb lines 360-392)
                Dim a11 As Double = -0.163768916705
                Dim b11 As Double = 185.192806802
                Dim c11 As Double = 1.65054449461
                Dim inner As Double = a11 + b11 * strainAbs
                If inner <= 0.0001 Then Return 1.0E+15 ' Below endurance limit (asymptote)
                If strainAbs <= 0.001765093 Then
                    Dim expn As Double = inner ^ (-1.0 / c11)
                    If expn > 15 Then Return 1.0E+15
                    Return 10.0 ^ expn
                Else
                    Return (0.00414131183 / strainAbs) ^ 8.1
                End If

            ElseIf modelName = "Straight-Line" Then
                ' Straight-Line model (modCDF.vb lines 336-351)
                Dim AAorig As Double = 0.000247 + 0.000245 * Math.Log(15000) / 2.302585
                Dim BBorig As Double = 0.0658 * 15000 ^ 0.559
                AAorig = AAorig * 10000.0 ^ (1.0 / BBorig)
                Dim AA_sl As Double = 0.004
                Dim BB_sl As Double = 8.1
                Dim strainBreak As Double = (BB_sl * Math.Log10(AA_sl) - BBorig * Math.Log10(AAorig))
                strainBreak = 10.0 ^ (strainBreak / (BB_sl - BBorig))
                If strainAbs > strainBreak Then
                    Return (AA_sl / strainAbs) ^ BB_sl
                Else
                    Return (AAorig / strainAbs) ^ BBorig
                End If

            Else ' Standard model
                Dim AA As Double = 0.000247 + 0.000245 * Math.Log(subMod) / 2.302585
                Dim BB As Double = 0.0658 * subMod ^ 0.559
                Return 10000.0 * (AA / strainAbs) ^ BB
            End If
        End Function

        Private Function DrawFatigueCurve(rpt As clsDetailedReportData, chartColors() As Color) As Bitmap
            Dim chartWidth As Integer = 900
            Dim chartHeight As Integer = 600
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                Dim faaBlue As Color = Color.FromArgb(46, 94, 168)
                Dim marginLeft As Integer = 95
                Dim marginRight As Integer = 45
                Dim marginTop As Integer = 50
                Dim marginBottom As Integer = 70
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Get subgrade modulus from last layer
                Dim subMod As Double = 15000
                If rpt.SublayerData.DesignLayers.Count > 0 Then
                    subMod = rpt.SublayerData.DesignLayers(rpt.SublayerData.DesignLayers.Count - 1).Modulus
                End If

                ' Detect which subgrade model(s) the aircraft use
                Dim activeModel As String = "Standard"
                If rpt.AircraftDetails IsNot Nothing Then
                    For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                        If rpt.AircraftDetails(ia) IsNot Nothing AndAlso rpt.AircraftDetails(ia).SubgradeModelUsed <> "" Then
                            activeModel = rpt.AircraftDetails(ia).SubgradeModelUsed
                            Exit For
                        End If
                    Next
                End If

                ' Bleasdale zone parameters
                Dim isBleasdale As Boolean = (activeModel = "Bleasdale")
                Dim blea_a11 As Double = -0.163768916705
                Dim blea_b11 As Double = 185.192806802
                Dim strainAsymptote As Double = -blea_a11 / blea_b11          ' ~0.000884 abs strain
                Dim microAsymptote As Double = strainAsymptote * 1000000.0    ' ~884.3 με
                Dim strainTransition As Double = 0.001765093                   ' Bleasdale → power-law
                Dim microTransition As Double = strainTransition * 1000000.0  ' ~1765.1 με

                ' Title — includes model name
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleText = "Subgrade Fatigue Model (" & activeModel & If(isBleasdale, " — Piecewise", "") & ")"
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 10)

                ' Collect aircraft data points: (strainMicro, NtoFail, TotalReps, Name, Color)
                Dim acPoints As New List(Of Tuple(Of Double, Double, Double, String, Color))
                If rpt.AircraftDetails IsNot Nothing Then
                    For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                        If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                        Dim det = rpt.AircraftDetails(ia)
                        Dim strainMicro As Double = det.VerticalStrain * 1000000
                        If strainMicro > 0 AndAlso det.NtoFail > 0 Then
                            Dim acColor As Color = chartColors((ia - 1) Mod chartColors.Length)
                            acPoints.Add(New Tuple(Of Double, Double, Double, String, Color)(strainMicro, det.NtoFail, det.TotalRepetitions, det.ACName, acColor))
                        End If
                    Next
                End If

                ' Determine axis ranges from data
                Dim dataMinS As Double = Double.MaxValue
                Dim dataMaxS As Double = Double.MinValue
                Dim dataMinN As Double = Double.MaxValue
                Dim dataMaxN As Double = Double.MinValue
                For Each pt In acPoints
                    If pt.Item1 < dataMinS Then dataMinS = pt.Item1
                    If pt.Item1 > dataMaxS Then dataMaxS = pt.Item1
                    If pt.Item2 < dataMinN Then dataMinN = pt.Item2
                    If pt.Item2 > dataMaxN Then dataMaxN = pt.Item2
                    If pt.Item3 > 0 Then
                        If pt.Item3 < dataMinN Then dataMinN = pt.Item3
                        If pt.Item3 > dataMaxN Then dataMaxN = pt.Item3
                    End If
                Next

                ' Fallback
                If dataMinS = Double.MaxValue Then
                    dataMinS = 100 : dataMaxS = 1000
                    dataMinN = 1.0E+4 : dataMaxN = 1.0E+12
                End If

                ' Axis ranges
                Dim logMinS, logMaxS, logMinN, logMaxN As Double

                If isBleasdale Then
                    ' X: show from near asymptote to beyond data and transition
                    logMinS = Math.Min(Math.Log10(microAsymptote) - 0.08, Math.Log10(dataMinS) - 0.3)
                    logMaxS = Math.Max(Math.Log10(dataMaxS) + 0.5, Math.Log10(microTransition) + 0.5)
                    ' Y: fixed 10^0 to 10^8
                    logMinN = 0
                    logMaxN = 8
                Else
                    ' X-axis range — adaptive padding based on data spread
                    Dim logDataCenter As Double = (Math.Log10(dataMinS) + Math.Log10(dataMaxS)) / 2
                    Dim logDataSpan As Double = Math.Log10(dataMaxS) - Math.Log10(dataMinS)
                    If logDataSpan < 0.5 Then
                        logMinS = logDataCenter - 1.0
                        logMaxS = logDataCenter + 1.0
                    ElseIf logDataSpan < 1.5 Then
                        logMinS = Math.Log10(dataMinS) - 0.4
                        logMaxS = Math.Log10(dataMaxS) + 0.4
                    Else
                        logMinS = Math.Floor(Math.Log10(dataMinS)) - 0.3
                        logMaxS = Math.Ceiling(Math.Log10(dataMaxS)) + 0.3
                    End If

                    ' Y-axis range from data with 1 decade padding
                    logMinN = Math.Floor(Math.Log10(Math.Max(dataMinN, 1))) - 1
                    logMaxN = Math.Ceiling(Math.Log10(Math.Max(dataMaxN, 10))) + 1
                    If logMaxN - logMinN < 3 Then
                        Dim logNCenter As Double = (logMinN + logMaxN) / 2
                        logMinN = logNCenter - 2
                        logMaxN = logNCenter + 2
                    End If
                End If

                ' Plot area
                Dim plotRect As New Rectangle(marginLeft, marginTop, plotWidth, plotHeight)
                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), plotRect)

                ' Bleasdale zone backgrounds
                If isBleasdale Then
                    Dim plotL As Single = CSng(marginLeft)
                    Dim plotR As Single = CSng(marginLeft + plotWidth)
                    Dim plotT As Single = CSng(marginTop)
                    Dim plotB As Single = CSng(marginTop + plotHeight)
                    ' Compute transition pixel positions inline (xScale not yet defined)
                    Dim xAsymZone As Single = CSng(marginLeft + (Math.Log10(microAsymptote) - logMinS) / (logMaxS - logMinS) * plotWidth)
                    Dim xTransZone As Single = CSng(marginLeft + (Math.Log10(microTransition) - logMinS) / (logMaxS - logMinS) * plotWidth)

                    ' Zone A: left edge to asymptote — endurance limit (green tint)
                    If xAsymZone > plotL Then
                        g.FillRectangle(New SolidBrush(Color.FromArgb(18, 76, 175, 80)),
                            plotL, plotT, Math.Min(xAsymZone, plotR) - plotL, plotB - plotT)
                    End If
                    ' Zone B: asymptote to transition — Bleasdale curve (blue tint)
                    Dim zBL As Single = Math.Max(xAsymZone, plotL)
                    Dim zBR As Single = Math.Min(xTransZone, plotR)
                    If zBR > zBL Then
                        g.FillRectangle(New SolidBrush(Color.FromArgb(18, 46, 94, 168)),
                            zBL, plotT, zBR - zBL, plotB - plotT)
                    End If
                    ' Zone C: transition to right edge — power-law tail (amber tint)
                    If xTransZone < plotR Then
                        g.FillRectangle(New SolidBrush(Color.FromArgb(18, 214, 130, 40)),
                            Math.Max(xTransZone, plotL), plotT, plotR - Math.Max(xTransZone, plotL), plotB - plotT)
                    End If
                End If

                g.DrawRectangle(New Pen(Color.DarkGray, 1.0F), plotRect)

                ' Mapping functions
                Dim xScale = Function(logVal As Double) As Single
                                 Return CSng(marginLeft + (logVal - logMinS) / (logMaxS - logMinS) * plotWidth)
                             End Function
                Dim yScale = Function(logVal As Double) As Single
                                 Return CSng(marginTop + plotHeight - (logVal - logMinN) / (logMaxN - logMinN) * plotHeight)
                             End Function

                ' Grid lines
                Dim gridPen As New Pen(Color.FromArgb(210, 214, 220), 1)
                gridPen.DashStyle = Drawing2D.DashStyle.Dot
                Dim axisFont As New Font("Segoe UI", 9.5F)
                Dim labelFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)
                Dim minorFont As New Font("Segoe UI", 8.0F)
                Dim minorPen As New Pen(Color.FromArgb(240, 240, 240), 1)
                minorPen.DashStyle = Drawing2D.DashStyle.Dot

                ' X-axis ticks: major at 10^n, minor at 2×,3×,5×
                Dim minorMults() As Double = {2, 3, 5}
                For logV As Integer = CInt(Math.Floor(logMinS)) - 1 To CInt(Math.Ceiling(logMaxS)) + 1
                    Dim xPx As Single = xScale(logV)
                    If xPx > marginLeft AndAlso xPx < marginLeft + plotWidth Then
                        g.DrawLine(gridPen, xPx, marginTop, xPx, marginTop + plotHeight)
                        Dim micVal As Double = 10.0 ^ logV
                        Dim xLabel As String = If(micVal >= 1000, Format(micVal, "#,##0"), Format(micVal, "0"))
                        Dim xlSz = g.MeasureString(xLabel, axisFont)
                        g.DrawString(xLabel, axisFont, Brushes.Black, xPx - xlSz.Width / 2, marginTop + plotHeight + 5)
                    End If
                    For Each m In minorMults
                        Dim mLog As Double = logV + Math.Log10(m)
                        Dim mPx As Single = xScale(mLog)
                        If mPx > marginLeft + 3 AndAlso mPx < marginLeft + plotWidth - 3 Then
                            g.DrawLine(minorPen, mPx, marginTop, mPx, marginTop + plotHeight)
                            Dim mVal As Double = 10.0 ^ logV * m
                            Dim mLabel As String = If(mVal >= 1000, Format(mVal, "#,##0"), Format(mVal, "0"))
                            Dim mSz = g.MeasureString(mLabel, minorFont)
                            g.DrawString(mLabel, minorFont, New SolidBrush(Color.FromArgb(130, 130, 130)), mPx - mSz.Width / 2, marginTop + plotHeight + 6)
                        End If
                    Next
                Next

                ' Y-axis ticks
                For logV As Integer = CInt(Math.Ceiling(logMinN)) To CInt(Math.Floor(logMaxN))
                    Dim yPx As Single = yScale(logV)
                    If yPx > marginTop AndAlso yPx < marginTop + plotHeight Then
                        g.DrawLine(gridPen, marginLeft, yPx, marginLeft + plotWidth, yPx)
                    End If
                    Dim yLabel As String = "10^" & logV.ToString()
                    Dim ylSz = g.MeasureString(yLabel, axisFont)
                    g.DrawString(yLabel, axisFont, Brushes.Black, marginLeft - ylSz.Width - 6, yPx - ylSz.Height / 2)
                Next

                ' Axis labels
                Dim xAxisLabel = "Vertical Strain (" & ChrW(&H03BC) & ChrW(&H03B5) & ")"
                Dim xAxisSize = g.MeasureString(xAxisLabel, labelFont)
                g.DrawString(xAxisLabel, labelFont, Brushes.Black, CSng(marginLeft + (plotWidth - xAxisSize.Width) / 2), chartHeight - 22)

                g.TranslateTransform(30, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(-90)
                Dim yAxisLabel = "Allowable Repetitions (N)"
                Dim yAxisSize = g.MeasureString(yAxisLabel, labelFont)
                g.DrawString(yAxisLabel, labelFont, Brushes.Black, -yAxisSize.Width / 2, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' Bleasdale transition markers and zone labels
                If isBleasdale Then
                    Dim transFont As New Font("Segoe UI", 8.0F, FontStyle.Bold)
                    Dim zoneFont As New Font("Segoe UI", 8.5F, FontStyle.Italic)
                    Dim plotT As Single = CSng(marginTop)
                    Dim plotB As Single = CSng(marginTop + plotHeight)
                    Dim plotL As Single = CSng(marginLeft)
                    Dim plotR As Single = CSng(marginLeft + plotWidth)

                    ' Asymptote vertical line (ε ≈ 884 με)
                    Dim xAsymPx As Single = xScale(Math.Log10(microAsymptote))
                    If xAsymPx > plotL AndAlso xAsymPx < plotR Then
                        Dim asymPen As New Pen(Color.FromArgb(160, 56, 142, 60), 1.5F)
                        asymPen.DashStyle = Drawing2D.DashStyle.DashDot
                        g.DrawLine(asymPen, xAsymPx, plotT, xAsymPx, plotB)
                        ' Diamond marker
                        Dim dm As Single = 5
                        Dim dmY As Single = plotT + 10
                        g.FillPolygon(New SolidBrush(Color.FromArgb(56, 142, 60)),
                            {New PointF(xAsymPx, dmY - dm), New PointF(xAsymPx + dm, dmY),
                             New PointF(xAsymPx, dmY + dm), New PointF(xAsymPx - dm, dmY)})
                        ' Strain label
                        Dim aLbl = Format(microAsymptote, "0.0") & " " & ChrW(&H03BC) & ChrW(&H03B5)
                        Dim aLblSz = g.MeasureString(aLbl, transFont)
                        g.FillRectangle(New SolidBrush(Color.FromArgb(220, 255, 255, 255)),
                            xAsymPx + 3, plotT + 18, aLblSz.Width + 4, aLblSz.Height + 2)
                        g.DrawString(aLbl, transFont, New SolidBrush(Color.FromArgb(56, 142, 60)),
                            xAsymPx + 5, plotT + 19)
                        asymPen.Dispose()
                    End If

                    ' Transition vertical line (ε ≈ 1765 με, N ≈ 1000)
                    Dim xTransPx As Single = xScale(Math.Log10(microTransition))
                    If xTransPx > plotL AndAlso xTransPx < plotR Then
                        Dim transPen As New Pen(Color.FromArgb(160, 180, 100, 20), 1.5F)
                        transPen.DashStyle = Drawing2D.DashStyle.DashDot
                        g.DrawLine(transPen, xTransPx, plotT, xTransPx, plotB)
                        ' Diamond marker
                        Dim dm As Single = 5
                        Dim dmY As Single = plotT + 10
                        g.FillPolygon(New SolidBrush(Color.FromArgb(180, 100, 20)),
                            {New PointF(xTransPx, dmY - dm), New PointF(xTransPx + dm, dmY),
                             New PointF(xTransPx, dmY + dm), New PointF(xTransPx - dm, dmY)})
                        ' Strain label
                        Dim tLbl = Format(microTransition, "0.0") & " " & ChrW(&H03BC) & ChrW(&H03B5)
                        Dim tLblSz = g.MeasureString(tLbl, transFont)
                        g.FillRectangle(New SolidBrush(Color.FromArgb(220, 255, 255, 255)),
                            xTransPx + 3, plotT + 18, tLblSz.Width + 4, tLblSz.Height + 2)
                        g.DrawString(tLbl, transFont, New SolidBrush(Color.FromArgb(180, 100, 20)),
                            xTransPx + 5, plotT + 19)
                        transPen.Dispose()
                    End If

                    ' N = 1,000 horizontal reference line
                    Dim yN1000 As Single = yScale(3) ' log10(1000) = 3
                    If yN1000 > plotT + 5 AndAlso yN1000 < plotB - 5 Then
                        Dim n1000Pen As New Pen(Color.FromArgb(120, 180, 100, 20), 1.0F)
                        n1000Pen.DashStyle = Drawing2D.DashStyle.Dash
                        g.DrawLine(n1000Pen, plotL, yN1000, plotR, yN1000)
                        Dim nLbl = "N = 1,000"
                        Dim nLblSz = g.MeasureString(nLbl, transFont)
                        g.FillRectangle(New SolidBrush(Color.FromArgb(220, 255, 255, 255)),
                            plotR - nLblSz.Width - 8, yN1000 - nLblSz.Height - 2, nLblSz.Width + 6, nLblSz.Height + 2)
                        g.DrawString(nLbl, transFont, New SolidBrush(Color.FromArgb(180, 100, 20)),
                            plotR - nLblSz.Width - 5, yN1000 - nLblSz.Height - 1)
                        n1000Pen.Dispose()
                    End If

                    ' Zone labels at top of each zone
                    If xAsymPx > plotL + 50 Then
                        Dim zACenter As Single = (plotL + Math.Min(xAsymPx, plotR)) / 2
                        Dim zALbl = "Endurance Limit"
                        Dim zASz = g.MeasureString(zALbl, zoneFont)
                        g.DrawString(zALbl, zoneFont, New SolidBrush(Color.FromArgb(100, 56, 142, 60)),
                            zACenter - zASz.Width / 2, plotT + 4)
                    End If

                    If xTransPx > xAsymPx + 60 Then
                        Dim zBCenter As Single = (Math.Max(xAsymPx, plotL) + Math.Min(xTransPx, plotR)) / 2
                        Dim zBLbl1 = "Bleasdale Curve"
                        Dim zBSz1 = g.MeasureString(zBLbl1, zoneFont)
                        g.DrawString(zBLbl1, zoneFont, New SolidBrush(Color.FromArgb(100, 46, 94, 168)),
                            zBCenter - zBSz1.Width / 2, plotT + 4)
                        Dim zBLbl2 = "(Cov " & ChrW(&H2265) & " 1,000)"
                        Dim zBSz2 = g.MeasureString(zBLbl2, zoneFont)
                        g.DrawString(zBLbl2, zoneFont, New SolidBrush(Color.FromArgb(100, 46, 94, 168)),
                            zBCenter - zBSz2.Width / 2, plotT + 4 + zBSz1.Height)
                    End If

                    If plotR > xTransPx + 60 Then
                        Dim zCCenter As Single = (Math.Max(xTransPx, plotL) + plotR) / 2
                        Dim zCLbl1 = "Power Law"
                        Dim zCSz1 = g.MeasureString(zCLbl1, zoneFont)
                        g.DrawString(zCLbl1, zoneFont, New SolidBrush(Color.FromArgb(100, 180, 100, 20)),
                            zCCenter - zCSz1.Width / 2, plotT + 4)
                        Dim zCLbl2 = "(Cov < 1,000)"
                        Dim zCSz2 = g.MeasureString(zCLbl2, zoneFont)
                        g.DrawString(zCLbl2, zoneFont, New SolidBrush(Color.FromArgb(100, 180, 100, 20)),
                            zCCenter - zCSz2.Width / 2, plotT + 4 + zCSz1.Height)
                    End If

                    transFont.Dispose()
                    zoneFont.Dispose()
                End If

                ' Draw the fatigue model curve
                Dim nSteps As Integer = 500

                If isBleasdale Then
                    ' Two segments: Bleasdale (blue solid) and power-law (amber dashed)
                    Dim bleaPen As New Pen(faaBlue, 2.5F)
                    Dim powPen As New Pen(Color.FromArgb(180, 100, 20), 2.5F)
                    powPen.DashStyle = Drawing2D.DashStyle.Dash
                    Dim bleaPoints As New List(Of PointF)
                    Dim powPoints As New List(Of PointF)

                    For i As Integer = 0 To nSteps
                        Dim logS As Double = logMinS + (logMaxS - logMinS) * i / nSteps
                        Dim strainAbs As Double = 10.0 ^ logS / 1000000.0
                        Dim nFail As Double = ComputeNtoFail(strainAbs, activeModel, subMod)
                        Dim logNF As Double = Math.Log10(Math.Max(nFail, 1))
                        If logNF < logMinN Then logNF = logMinN
                        If logNF > logMaxN Then logNF = logMaxN
                        Dim pt As New PointF(xScale(logS), yScale(logNF))

                        If strainAbs <= strainTransition Then
                            bleaPoints.Add(pt)
                        Else
                            If powPoints.Count = 0 AndAlso bleaPoints.Count > 0 Then
                                ' Ensure continuity at the transition point
                                Dim tLogS = Math.Log10(microTransition)
                                Dim tNF = ComputeNtoFail(strainTransition, activeModel, subMod)
                                Dim tLogNF = Math.Log10(Math.Max(tNF, 1))
                                If tLogNF < logMinN Then tLogNF = logMinN
                                If tLogNF > logMaxN Then tLogNF = logMaxN
                                Dim tPt As New PointF(xScale(tLogS), yScale(tLogNF))
                                bleaPoints.Add(tPt)
                                powPoints.Add(tPt)
                            End If
                            powPoints.Add(pt)
                        End If
                    Next

                    ' Fill under Bleasdale segment
                    If bleaPoints.Count > 1 Then
                        Dim fillPts As New List(Of PointF)
                        fillPts.Add(New PointF(bleaPoints(0).X, CSng(marginTop + plotHeight)))
                        fillPts.AddRange(bleaPoints)
                        fillPts.Add(New PointF(bleaPoints(bleaPoints.Count - 1).X, CSng(marginTop + plotHeight)))
                        g.FillPolygon(New SolidBrush(Color.FromArgb(12, faaBlue.R, faaBlue.G, faaBlue.B)), fillPts.ToArray())
                        g.DrawLines(bleaPen, bleaPoints.ToArray())
                    End If
                    ' Fill under power-law segment
                    If powPoints.Count > 1 Then
                        Dim fillPts As New List(Of PointF)
                        fillPts.Add(New PointF(powPoints(0).X, CSng(marginTop + plotHeight)))
                        fillPts.AddRange(powPoints)
                        fillPts.Add(New PointF(powPoints(powPoints.Count - 1).X, CSng(marginTop + plotHeight)))
                        g.FillPolygon(New SolidBrush(Color.FromArgb(12, 180, 100, 20)), fillPts.ToArray())
                        g.DrawLines(powPen, powPoints.ToArray())
                    End If

                    bleaPen.Dispose()
                    powPen.Dispose()
                Else
                    ' Single-color curve for Standard / Straight-Line models
                    Dim curvePen As New Pen(faaBlue, 2.5F)
                    Dim curvePoints As New List(Of PointF)
                    For i As Integer = 0 To nSteps
                        Dim logS As Double = logMinS + (logMaxS - logMinS) * i / nSteps
                        Dim strainAbs As Double = 10.0 ^ logS / 1000000.0
                        Dim nFail As Double = ComputeNtoFail(strainAbs, activeModel, subMod)
                        Dim logNF As Double = Math.Log10(Math.Max(nFail, 1))
                        If logNF < logMinN Then logNF = logMinN
                        If logNF > logMaxN Then logNF = logMaxN
                        curvePoints.Add(New PointF(xScale(logS), yScale(logNF)))
                    Next

                    If curvePoints.Count > 1 Then
                        Dim fillPts As New List(Of PointF)
                        fillPts.Add(New PointF(curvePoints(0).X, CSng(marginTop + plotHeight)))
                        fillPts.AddRange(curvePoints)
                        fillPts.Add(New PointF(curvePoints(curvePoints.Count - 1).X, CSng(marginTop + plotHeight)))
                        g.FillPolygon(New SolidBrush(Color.FromArgb(15, faaBlue.R, faaBlue.G, faaBlue.B)), fillPts.ToArray())
                    End If
                    If curvePoints.Count > 1 Then g.DrawLines(curvePen, curvePoints.ToArray())
                    curvePen.Dispose()
                End If

                ' Equation box
                Dim eqBoxFont As New Font("Consolas", If(isBleasdale, 4.5F, 9.0F), FontStyle.Bold)
                Dim paramBoxFont As New Font("Segoe UI", If(isBleasdale, 4.25F, 8.5F))

                Dim eqLines As New List(Of String)
                Dim paramLines As New List(Of String)
                If activeModel = "Bleasdale" Then
                    eqLines.Add("Bleasdale (piecewise):")
                    eqLines.Add(ChrW(&H03B5) & " " & ChrW(&H2264) & " " & Format(microTransition, "0.0") & " " & ChrW(&H03BC) & ChrW(&H03B5) & ":")
                    eqLines.Add("  N = 10^{((a + b" & ChrW(&H00B7) & ChrW(&H03B5) & "v)^{-1/c})}")
                    eqLines.Add(ChrW(&H03B5) & " > " & Format(microTransition, "0.0") & " " & ChrW(&H03BC) & ChrW(&H03B5) & ":")
                    eqLines.Add("  N = (0.00414 / " & ChrW(&H03B5) & "v)^{8.1}")
                    paramLines.Add("a = -0.1638  b = 185.19  c = 1.651")
                    paramLines.Add("Asymptote: " & Format(microAsymptote, "0.0") & " " & ChrW(&H03BC) & ChrW(&H03B5))
                    paramLines.Add("Transition: " & Format(microTransition, "0.0") & " " & ChrW(&H03BC) & ChrW(&H03B5) & " (N " & ChrW(&H2248) & " 1,000)")
                ElseIf activeModel = "Straight-Line" Then
                    eqLines.Add("Straight-Line Model:")
                    eqLines.Add("N = (AA / " & ChrW(&H03B5) & "v)^{BB}")
                    paramLines.Add("E_subgrade = " & Format(subMod, "#,##0") & " psi")
                Else
                    eqLines.Add("Standard Model:")
                    eqLines.Add("N = 10,000 " & ChrW(&H00D7) & " (AA / " & ChrW(&H03B5) & "v)^{BB}")
                    Dim stdAA As Double = 0.000247 + 0.000245 * Math.Log(subMod) / 2.302585
                    Dim stdBB As Double = 0.0658 * subMod ^ 0.559
                    paramLines.Add("AA = " & Format(stdAA, "0.000000") & "   BB = " & Format(stdBB, "0.000"))
                    paramLines.Add("E_subgrade = " & Format(subMod, "#,##0") & " psi")
                End If

                ' Measure all lines to compute box size
                Dim boxW As Single = 0
                Dim boxH As Single = 8
                For Each line In eqLines
                    Dim w = MeasureFormattedString(g, line, eqBoxFont)
                    If w > boxW Then boxW = w
                    Dim sz = g.MeasureString(line, eqBoxFont)
                    boxH += sz.Height + 2
                Next
                For Each line In paramLines
                    Dim sz = g.MeasureString(line, paramBoxFont)
                    If sz.Width > boxW Then boxW = sz.Width
                    boxH += sz.Height + 1
                Next
                boxW += 24
                boxH += 4

                Dim infoX As Single = marginLeft + 12
                ' infoY will be repositioned for Bleasdale after legend is laid out; use placeholder
                Dim infoY As Single = CSng(marginTop + 10)
                Dim infoRect As New RectangleF(infoX, infoY, boxW, boxH)

                ' Draw equation box at default position for non-Bleasdale (Bleasdale redraws above legend later)
                If Not isBleasdale Then
                    g.FillRectangle(New SolidBrush(Color.FromArgb(240, 255, 255, 255)), infoRect)
                    g.DrawRectangle(New Pen(Color.FromArgb(100, faaBlue.R, faaBlue.G, faaBlue.B), 1.0F), infoRect.X, infoRect.Y, infoRect.Width, infoRect.Height)

                    Dim txtY As Single = infoY + 5
                    For Each line In eqLines
                        DrawFormattedString(g, line, eqBoxFont, New SolidBrush(faaBlue), infoX + 12, txtY)
                        txtY += g.MeasureString(line, eqBoxFont).Height + 2
                    Next
                    For Each line In paramLines
                        g.DrawString(line, paramBoxFont, New SolidBrush(Color.FromArgb(80, 80, 80)), infoX + 12, txtY)
                        txtY += g.MeasureString(line, paramBoxFont).Height + 1
                    Next
                End If

                ' Scatter points and labels
                Dim labelPositions As New List(Of RectangleF)
                labelPositions.Add(infoRect)
                Dim ptFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)

                ' Legend — bottom-left (curve typically descends from upper-left to lower-right)
                Dim legendFont As New Font("Segoe UI", If(isBleasdale, 8.0F, 8.5F))
                Dim legendLineH As Integer = If(isBleasdale, 14, 18)
                Dim legendEntries As Integer = acPoints.Count + If(isBleasdale, 3, 2)
                Dim legendH As Integer = legendEntries * legendLineH + 12
                Dim maxLegEntryW As Single = 0
                For Each pt In acPoints
                    Dim ew = g.MeasureString(pt.Item4 & " (N_fail)", legendFont)
                    If ew.Width > maxLegEntryW Then maxLegEntryW = ew.Width
                Next
                If isBleasdale Then
                    Dim blew = g.MeasureString("Power law (Cov < 1,000)", legendFont)
                    If blew.Width > maxLegEntryW Then maxLegEntryW = blew.Width
                End If
                Dim legendW As Integer = CInt(maxLegEntryW) + 46
                Dim legendX As Integer = marginLeft + 12
                Dim legendY As Integer = marginTop + plotHeight - legendH - 10
                labelPositions.Add(New RectangleF(legendX, legendY, legendW, legendH))

                For Each pt In acPoints
                    Dim strainMicro = pt.Item1
                    Dim nFail = pt.Item2
                    Dim totalReps = pt.Item3
                    Dim acName = pt.Item4
                    Dim acColor = pt.Item5

                    Dim logS = Math.Log10(strainMicro)
                    Dim logNF = Math.Log10(Math.Max(nFail, 1))
                    Dim xPx = xScale(logS)
                    Dim yPx = yScale(logNF)

                    ' White halo + scatter point
                    g.FillEllipse(Brushes.White, xPx - 9, yPx - 9, 18, 18)
                    g.FillEllipse(New SolidBrush(acColor), xPx - 7, yPx - 7, 14, 14)
                    g.DrawEllipse(New Pen(Color.FromArgb(80, 0, 0, 0), 1.0F), xPx - 7, yPx - 7, 14, 14)

                    ' Repetitions dashed line
                    If totalReps > 0 Then
                        Dim logReps = Math.Log10(totalReps)
                        If logReps >= logMinN AndAlso logReps <= logMaxN Then
                            Dim repsYPx = yScale(logReps)
                            Dim repsPen As New Pen(Color.FromArgb(140, acColor.R, acColor.G, acColor.B), 1.0F)
                            repsPen.DashStyle = Drawing2D.DashStyle.Dash
                            g.DrawLine(repsPen, marginLeft, repsYPx, xPx, repsYPx)
                            Dim connPen As New Pen(Color.FromArgb(100, acColor.R, acColor.G, acColor.B), 0.8F)
                            connPen.DashStyle = Drawing2D.DashStyle.Dot
                            g.DrawLine(connPen, xPx, repsYPx, xPx, yPx)
                            connPen.Dispose()
                            repsPen.Dispose()
                        End If
                    End If

                    ' Label with strain value
                    Dim lblText As String = acName & " (" & Format(strainMicro, "0.0") & " " & ChrW(&H03BC) & ChrW(&H03B5) & ")"
                    Dim lblSize = g.MeasureString(lblText, ptFont)
                    Dim lblX As Single = xPx + 10
                    Dim lblY As Single = yPx - lblSize.Height / 2
                    Dim lblRect As New RectangleF(lblX, lblY, lblSize.Width, lblSize.Height)

                    Dim placed As Boolean = False
                    For attempt As Integer = 0 To 5
                        Dim overlaps As Boolean = False
                        For Each existing In labelPositions
                            If lblRect.IntersectsWith(existing) Then overlaps = True : Exit For
                        Next
                        If Not overlaps Then placed = True : Exit For
                        lblY += lblSize.Height + 2
                        lblRect = New RectangleF(lblX, lblY, lblSize.Width, lblSize.Height)
                    Next
                    If Not placed Then
                        lblX = xPx - lblSize.Width - 10
                        lblY = yPx - lblSize.Height / 2
                        lblRect = New RectangleF(lblX, lblY, lblSize.Width, lblSize.Height)
                        For attempt As Integer = 0 To 4
                            Dim overlaps As Boolean = False
                            For Each existing In labelPositions
                                If lblRect.IntersectsWith(existing) Then overlaps = True : Exit For
                            Next
                            If Not overlaps Then placed = True : Exit For
                            lblY += lblSize.Height + 2
                            lblRect = New RectangleF(lblX, lblY, lblSize.Width, lblSize.Height)
                        Next
                    End If
                    If Not placed Then
                        lblX = xPx - lblSize.Width / 2
                        lblY = yPx - lblSize.Height - 14
                        lblRect = New RectangleF(lblX, lblY, lblSize.Width, lblSize.Height)
                    End If
                    labelPositions.Add(lblRect)

                    Dim leaderPen As New Pen(Color.FromArgb(120, acColor.R, acColor.G, acColor.B), 0.8F)
                    g.DrawLine(leaderPen, xPx, yPx, lblX + If(lblX > xPx, 0, lblSize.Width), lblY + lblSize.Height / 2)
                    leaderPen.Dispose()

                    Dim haloRect As New RectangleF(lblX - 2, lblY - 1, lblSize.Width + 4, lblSize.Height + 2)
                    g.FillRectangle(New SolidBrush(Color.FromArgb(220, 255, 255, 255)), haloRect)
                    g.DrawString(lblText, ptFont, New SolidBrush(acColor), lblX, lblY)
                Next
                ptFont.Dispose()

                ' Reposition equation box on top of legend for Bleasdale
                If isBleasdale Then
                    infoY = legendY - boxH - 6
                    infoRect = New RectangleF(CSng(legendX), infoY, boxW, boxH)
                    ' Redraw equation box at new position
                    g.FillRectangle(New SolidBrush(Color.FromArgb(240, 255, 255, 255)), infoRect)
                    g.DrawRectangle(New Pen(Color.FromArgb(100, faaBlue.R, faaBlue.G, faaBlue.B), 1.0F), infoRect.X, infoRect.Y, infoRect.Width, infoRect.Height)
                    Dim reTxtY As Single = infoY + 5
                    For Each line In eqLines
                        DrawFormattedString(g, line, eqBoxFont, New SolidBrush(faaBlue), infoRect.X + 8, reTxtY)
                        reTxtY += g.MeasureString(line, eqBoxFont).Height + 1
                    Next
                    For Each line In paramLines
                        g.DrawString(line, paramBoxFont, New SolidBrush(Color.FromArgb(80, 80, 80)), infoRect.X + 8, reTxtY)
                        reTxtY += g.MeasureString(line, paramBoxFont).Height + 1
                    Next
                End If

                ' Legend drawing
                g.FillRectangle(New SolidBrush(Color.FromArgb(240, 255, 255, 255)), legendX, legendY, legendW, legendH)
                g.DrawRectangle(New Pen(Color.FromArgb(170, 170, 170), 1.0F), legendX, legendY, legendW, legendH)

                Dim lly As Integer = legendY + 6

                If isBleasdale Then
                    ' Bleasdale curve line
                    Dim bLegPen As New Pen(faaBlue, 2.5F)
                    g.DrawLine(bLegPen, legendX + 8, lly + 8, legendX + 26, lly + 8)
                    g.DrawString("Bleasdale curve", legendFont, Brushes.Black, legendX + 30, lly)
                    lly += legendLineH
                    bLegPen.Dispose()
                    ' Power-law line
                    Dim pLegPen As New Pen(Color.FromArgb(180, 100, 20), 2.5F)
                    pLegPen.DashStyle = Drawing2D.DashStyle.Dash
                    g.DrawLine(pLegPen, legendX + 8, lly + 8, legendX + 26, lly + 8)
                    g.DrawString("Power law (Cov < 1,000)", legendFont, Brushes.Black, legendX + 30, lly)
                    lly += legendLineH
                    pLegPen.Dispose()
                Else
                    Dim cLegPen As New Pen(faaBlue, 2.5F)
                    g.DrawLine(cLegPen, legendX + 8, lly + 8, legendX + 26, lly + 8)
                    g.DrawString(activeModel & " model", legendFont, Brushes.Black, legendX + 30, lly)
                    lly += legendLineH
                    cLegPen.Dispose()
                End If

                Dim repLegPen As New Pen(Color.FromArgb(120, 120, 120), 1.0F)
                repLegPen.DashStyle = Drawing2D.DashStyle.Dash
                g.DrawLine(repLegPen, legendX + 8, lly + 8, legendX + 26, lly + 8)
                g.DrawString("Total repetitions", legendFont, Brushes.Black, legendX + 30, lly)
                lly += legendLineH
                repLegPen.Dispose()

                For Each pt In acPoints
                    g.FillEllipse(New SolidBrush(pt.Item5), legendX + 12, lly + 3, 10, 10)
                    g.DrawString(pt.Item4, legendFont, Brushes.Black, legendX + 30, lly)
                    lly += legendLineH
                Next

                ' Cleanup
                titleFont.Dispose()
                axisFont.Dispose()
                labelFont.Dispose()
                minorFont.Dispose()
                legendFont.Dispose()
                eqBoxFont.Dispose()
                paramBoxFont.Dispose()
                gridPen.Dispose()
                minorPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws a dual-axis convergence plot showing |ln(CDF)| and thickness vs iteration.
        ''' </summary>
        Private Function DrawConvergencePlot(rpt As clsDetailedReportData, thicknessUnit As String) As Bitmap
            Dim chartWidth As Integer = 850
            Dim chartHeight As Integer = 450

            If rpt.Iterations.Count < 2 Then Return New Bitmap(chartWidth, chartHeight)

            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                Dim marginLeft As Integer = 80
                Dim marginRight As Integer = 80
                Dim marginTop As Integer = 50
                Dim marginBottom As Integer = 60
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Title
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleText = "Newton-Raphson Convergence History"
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 10)

                ' Data ranges
                Dim nIter As Integer = rpt.Iterations.Count
                Dim minErr As Double = Double.MaxValue
                Dim maxErr As Double = Double.MinValue
                Dim minThk As Double = Double.MaxValue
                Dim maxThk As Double = Double.MinValue

                For Each iter In rpt.Iterations
                    Dim errVal As Double = Math.Max(iter.CDFErr, 0.0001)
                    If errVal < minErr Then minErr = errVal
                    If errVal > maxErr Then maxErr = errVal
                    If iter.Thickness < minThk Then minThk = iter.Thickness
                    If iter.Thickness > maxThk Then maxThk = iter.Thickness
                Next

                Dim logMinErr As Double = Math.Floor(Math.Log10(Math.Max(minErr, 0.0001)))
                Dim logMaxErr As Double = Math.Ceiling(Math.Log10(Math.Max(maxErr, 0.01)))
                If logMaxErr <= logMinErr Then logMaxErr = logMinErr + 2

                ' Thickness padding
                Dim thkRange As Double = maxThk - minThk
                If thkRange < 1 Then thkRange = 10
                minThk -= thkRange * 0.1
                maxThk += thkRange * 0.1

                ' Plot area
                Dim plotRect As New Rectangle(marginLeft, marginTop, plotWidth, plotHeight)
                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), plotRect)
                g.DrawRectangle(Pens.DarkGray, plotRect)

                ' X-axis (iterations)
                Dim axisFont As New Font("Segoe UI", 8.5F)
                Dim labelFont As New Font("Segoe UI", 9.5F)
                ' Target ~8 x-axis labels max; nIter \ 10 produced 12+ labels for 25 iterations,
                ' which collided at 8.5pt font.
                Dim xStep As Integer = Math.Max(1, CInt(Math.Ceiling(nIter / 8.0)))
                For i As Integer = 1 To nIter Step xStep
                    Dim xPx As Single = CSng(marginLeft + (i - 1) / Math.Max(nIter - 1, 1) * plotWidth)
                    Dim iLabel = i.ToString()
                    Dim iSize = g.MeasureString(iLabel, axisFont)
                    g.DrawString(iLabel, axisFont, Brushes.Black, xPx - iSize.Width / 2, marginTop + plotHeight + 4)
                Next
                g.DrawString("Iteration", labelFont, Brushes.Black, CSng(marginLeft + plotWidth / 2 - 25), chartHeight - 20)

                ' Left Y-axis: |ln(CDF)| (log scale, blue)
                Dim blueColor As Color = Color.FromArgb(46, 94, 168)
                Dim bluePen As New Pen(blueColor, 2)
                For logV As Integer = CInt(logMinErr) To CInt(logMaxErr)
                    Dim yPx As Single = CSng(marginTop + plotHeight - (logV - logMinErr) / (logMaxErr - logMinErr) * plotHeight)
                    Dim yLabel = "1E" & logV.ToString()
                    If logV = -1 Then yLabel = "0.1"
                    If logV = -2 Then yLabel = "0.01"
                    If logV = -3 Then yLabel = "0.001"
                    If logV = 0 Then yLabel = "1"
                    Dim ylSize = g.MeasureString(yLabel, axisFont)
                    g.DrawString(yLabel, axisFont, New SolidBrush(blueColor), marginLeft - ylSize.Width - 6, yPx - ylSize.Height / 2)
                Next
                g.TranslateTransform(25, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(-90)
                g.DrawString("|ln(CDF)|", labelFont, New SolidBrush(blueColor), -30, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' Right Y-axis: Thickness (linear, red)
                Dim redColor As Color = Color.FromArgb(214, 39, 40)
                Dim redPen As New Pen(redColor, 2)
                Dim nThkTicks As Integer = 5
                For i As Integer = 0 To nThkTicks
                    Dim thkVal As Double = minThk + (maxThk - minThk) * i / nThkTicks
                    Dim yPx As Single = CSng(marginTop + plotHeight - CDbl(i) / nThkTicks * plotHeight)
                    Dim yLabel = Format(thkVal, "0.0")
                    g.DrawString(yLabel, axisFont, New SolidBrush(redColor), marginLeft + plotWidth + 8, yPx - 6)
                Next
                g.TranslateTransform(chartWidth - 25, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(90)
                g.DrawString("Thickness (" & thicknessUnit & ")", labelFont, New SolidBrush(redColor), -50, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' CDFExitErr threshold line
                Dim exitErrLog As Double = Math.Log10(CDF.CDFExitErr)
                If exitErrLog >= logMinErr AndAlso exitErrLog <= logMaxErr Then
                    Dim exitYPx As Single = CSng(marginTop + plotHeight - (exitErrLog - logMinErr) / (logMaxErr - logMinErr) * plotHeight)
                    Dim threshPen As New Pen(Color.FromArgb(34, 139, 34), 1.5F)
                    threshPen.DashStyle = Drawing2D.DashStyle.DashDot
                    g.DrawLine(threshPen, marginLeft, exitYPx, marginLeft + plotWidth, exitYPx)
                    g.DrawString("Convergence threshold = " & Format(CDF.CDFExitErr, "0.000"), New Font("Segoe UI", 8, FontStyle.Italic), New SolidBrush(Color.FromArgb(34, 139, 34)), marginLeft + 5, exitYPx - 14)
                    threshPen.Dispose()
                End If

                ' Plot |ln(CDF)| line (blue)
                Dim errPoints As New List(Of PointF)
                Dim thkPoints As New List(Of PointF)
                Dim firstSublayer As Integer = -1

                For idx As Integer = 0 To rpt.Iterations.Count - 1
                    Dim iter = rpt.Iterations(idx)
                    Dim xPx As Single = CSng(marginLeft + CDbl(idx) / Math.Max(nIter - 1, 1) * plotWidth)

                    ' Error point
                    Dim errVal As Double = Math.Max(iter.CDFErr, 0.0001)
                    Dim logErr As Double = Math.Log10(errVal)
                    Dim errYPx As Single = CSng(marginTop + plotHeight - (logErr - logMinErr) / (logMaxErr - logMinErr) * plotHeight)
                    errPoints.Add(New PointF(xPx, errYPx))

                    ' Thickness point
                    Dim thkYPx As Single = CSng(marginTop + plotHeight - (iter.Thickness - minThk) / (maxThk - minThk) * plotHeight)
                    thkPoints.Add(New PointF(xPx, thkYPx))

                    If iter.SubLayered AndAlso firstSublayer < 0 Then firstSublayer = idx
                Next

                ' Draw lines
                If errPoints.Count > 1 Then g.DrawLines(bluePen, errPoints.ToArray())
                If thkPoints.Count > 1 Then g.DrawLines(redPen, thkPoints.ToArray())

                ' Draw dots
                For Each pt In errPoints
                    g.FillEllipse(New SolidBrush(blueColor), pt.X - 3, pt.Y - 3, 6, 6)
                Next
                For Each pt In thkPoints
                    g.FillEllipse(New SolidBrush(redColor), pt.X - 3, pt.Y - 3, 6, 6)
                Next

                ' Sublayer activation marker
                If firstSublayer >= 0 Then
                    Dim slXPx As Single = CSng(marginLeft + CDbl(firstSublayer) / Math.Max(nIter - 1, 1) * plotWidth)
                    Dim slPen As New Pen(Color.FromArgb(148, 103, 189), 1.5F)
                    slPen.DashStyle = Drawing2D.DashStyle.Dash
                    g.DrawLine(slPen, slXPx, marginTop, slXPx, marginTop + plotHeight)
                    g.DrawString("Sublayers ON", New Font("Segoe UI", 8, FontStyle.Italic), New SolidBrush(Color.FromArgb(148, 103, 189)), slXPx + 3, marginTop + plotHeight - 16)
                    slPen.Dispose()
                End If

                ' Legend
                Dim legendFont As New Font("Segoe UI", 8.0F)
                Dim legendX As Integer = marginLeft + plotWidth - 170
                Dim legendY As Integer = marginTop + 10
                g.FillRectangle(New SolidBrush(Color.FromArgb(240, 245, 245, 245)), legendX, legendY, 160, 38)
                g.DrawRectangle(New Pen(Color.FromArgb(200, 204, 210), 1), legendX, legendY, 160, 38)
                g.DrawLine(bluePen, legendX + 6, legendY + 10, legendX + 22, legendY + 10)
                g.DrawString("|ln(CDF)| (left axis)", legendFont, Brushes.Black, legendX + 26, legendY + 4)
                g.DrawLine(redPen, legendX + 6, legendY + 24, legendX + 22, legendY + 24)
                g.DrawString("Thickness (right axis)", legendFont, Brushes.Black, legendX + 26, legendY + 18)

                ' Cleanup
                titleFont.Dispose()
                axisFont.Dispose()
                labelFont.Dispose()
                legendFont.Dispose()
                bluePen.Dispose()
                redPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws C/P ratio distribution across pavement width for all aircraft.
        ''' </summary>
        Private Function DrawCoveragePlot(rpt As clsDetailedReportData, offsetUnit As String, chartColors() As Color) As Bitmap
            Dim chartWidth As Integer = 850
            Dim chartHeight As Integer = 450
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                Dim marginLeft As Integer = 80
                Dim marginRight As Integer = 30
                Dim marginTop As Integer = 50
                Dim marginBottom As Integer = 65
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Title
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleText = "Coverage-to-Pass (C/P) Distribution Across Pavement Width"
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 10)

                ' Determine Y range from C/P data
                Dim maxCtoP As Double = 0
                Dim nAC As Integer = rpt.CDFSweep.NAircraftCaptured
                For ia As Integer = 1 To nAC
                    For ioff As Integer = 1 To CDF.NOFF
                        If rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, ioff) > maxCtoP Then
                            maxCtoP = rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, ioff)
                        End If
                    Next
                Next
                If maxCtoP <= 0 Then maxCtoP = 0.01
                Dim yMax As Double = maxCtoP * 1.15
                Dim xMax As Double = (CDF.NOFF - 1) * CDF.OFFSETINC

                ' Plot area
                Dim plotRect As New Rectangle(marginLeft, marginTop, plotWidth, plotHeight)
                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), plotRect)
                g.DrawRectangle(Pens.DarkGray, plotRect)

                ' Grid
                Dim gridPen As New Pen(Color.FromArgb(210, 214, 220), 1)
                gridPen.DashStyle = Drawing2D.DashStyle.Dot
                Dim axisFont As New Font("Segoe UI", 8.5F)
                Dim labelFont As New Font("Segoe UI", 9.5F)

                Dim nGridY As Integer = 5
                For i As Integer = 0 To nGridY
                    Dim yVal As Double = yMax * i / nGridY
                    Dim yPx As Integer = CInt(marginTop + plotHeight - (yVal / yMax) * plotHeight)
                    If i > 0 AndAlso i < nGridY Then g.DrawLine(gridPen, marginLeft, yPx, marginLeft + plotWidth, yPx)
                    Dim yLabel = Format(yVal, "0.0000")
                    Dim yLabelSize = g.MeasureString(yLabel, axisFont)
                    g.DrawString(yLabel, axisFont, Brushes.Black, marginLeft - yLabelSize.Width - 4, yPx - yLabelSize.Height / 2)
                Next

                Dim nGridX As Integer = 8
                For i As Integer = 0 To nGridX
                    Dim xVal As Double = xMax * i / nGridX
                    Dim xPx As Integer = CInt(marginLeft + (xVal / xMax) * plotWidth)
                    If i > 0 AndAlso i < nGridX Then g.DrawLine(gridPen, xPx, marginTop, xPx, marginTop + plotHeight)
                    Dim xLabel = Format(xVal, "0")
                    Dim xLabelSize = g.MeasureString(xLabel, axisFont)
                    g.DrawString(xLabel, axisFont, Brushes.Black, xPx - xLabelSize.Width / 2, marginTop + plotHeight + 4)
                Next

                ' Axis labels
                g.DrawString("Offset (" & offsetUnit & ")", labelFont, Brushes.Black, CSng(marginLeft + plotWidth / 2 - 40), chartHeight - 22)
                g.TranslateTransform(30, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(-90)
                g.DrawString("C/P Ratio", labelFont, Brushes.Black, -30, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' Draw curves
                Dim legendEntries As New List(Of Tuple(Of String, Color))
                For ia As Integer = 1 To nAC
                    Dim acColor As Color = chartColors((ia - 1) Mod chartColors.Length)
                    Dim acPen As New Pen(acColor, 2.0F)

                    Dim acName As String = "AC" & ia.ToString()
                    If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                        acName = rpt.AircraftDetails(ia).ACName
                    End If

                    Dim points(CDF.NOFF - 1) As PointF
                    For ioff As Integer = 1 To CDF.NOFF
                        Dim xVal As Double = (ioff - 1) * CDF.OFFSETINC
                        Dim xPx As Single = CSng(marginLeft + (xVal / xMax) * plotWidth)
                        Dim yPx As Single = CSng(marginTop + plotHeight - (rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, ioff) / yMax) * plotHeight)
                        points(ioff - 1) = New PointF(xPx, yPx)
                    Next
                    If points.Length > 1 Then g.DrawLines(acPen, points)
                    legendEntries.Add(New Tuple(Of String, Color)(acName, acColor))
                    acPen.Dispose()
                Next

                ' Legend — dynamic width, positioned at bottom-right
                Dim legendFont As New Font("Segoe UI", 8.0F)
                Dim legendLineH As Integer = 14
                Dim legendH As Integer = legendEntries.Count * legendLineH + 8

                ' Measure widest legend entry
                Dim maxLegW As Single = 0
                For Each entry In legendEntries
                    Dim eSize = g.MeasureString(entry.Item1, legendFont)
                    If eSize.Width > maxLegW Then maxLegW = eSize.Width
                Next
                Dim legendW As Integer = CInt(maxLegW) + 36
                Dim legendX As Integer = marginLeft + plotWidth - legendW - 8
                Dim legendY As Integer = marginTop + plotHeight - legendH - 8
                g.FillRectangle(New SolidBrush(Color.FromArgb(240, 245, 245, 245)), legendX, legendY, legendW, legendH)
                g.DrawRectangle(New Pen(Color.FromArgb(200, 204, 210), 1), legendX, legendY, legendW, legendH)

                For idx As Integer = 0 To legendEntries.Count - 1
                    Dim entry = legendEntries(idx)
                    Dim ly As Integer = legendY + 4 + idx * legendLineH
                    Dim lPen As New Pen(entry.Item2, 1.5F)
                    g.DrawLine(lPen, legendX + 6, ly + 5, legendX + 22, ly + 5)
                    g.DrawString(entry.Item1, legendFont, Brushes.Black, legendX + 26, ly)
                    lPen.Dispose()
                Next

                ' Critical offset line
                Dim critOffsetVal As Double = (rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC
                Dim critXPx As Single = CSng(marginLeft + (critOffsetVal / xMax) * plotWidth)
                Dim critPen As New Pen(Color.Red, 1.5F)
                critPen.DashStyle = Drawing2D.DashStyle.Dash
                g.DrawLine(critPen, critXPx, marginTop, critXPx, marginTop + plotHeight)
                Dim critFont As New Font("Segoe UI", 7.5F, FontStyle.Italic)
                g.DrawString("Critical: " & Format(critOffsetVal, "0") & " " & offsetUnit, critFont, Brushes.Red, critXPx + 4, marginTop + 4)

                ' Cleanup
                titleFont.Dispose()
                axisFont.Dispose()
                labelFont.Dispose()
                legendFont.Dispose()
                gridPen.Dispose()
                critPen.Dispose()
                critFont.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Standard normal cumulative distribution function (polynomial approximation).
        ''' </summary>
        Private Function NormalCDF(x As Double) As Double
            Dim t As Double = 1.0 / (1.0 + 0.2316419 * Math.Abs(x))
            Dim d As Double = 0.3989422804014327 ' 1/sqrt(2*pi)
            Dim p As Double = d * Math.Exp(-x * x / 2.0) * (t * (0.31938153 + t * (-0.356563782 + t * (1.781477937 + t * (-1.821255978 + t * 1.330274429)))))
            If x >= 0 Then Return 1.0 - p
            Return p
        End Function

        ''' <summary>
        ''' Compute area under Gaussian between a and b with std dev sigma.
        ''' Matches the FAA GaussArea function in modCDF.vb.
        ''' </summary>
        Private Function GaussAreaCalc(a As Double, b As Double, sigma As Double) As Double
            If sigma < 0.000001 Then
                If a <= 0 AndAlso 0 <= b Then Return 1.0
                Return 0.0
            End If
            Return NormalCDF(b / sigma) - NormalCDF(a / sigma)
        End Function


        ''' <summary>
        ''' Draws a comprehensive educational diagram explaining how C/P is computed
        ''' from the Gaussian lateral wander model for single and multi-wheel gears.
        ''' </summary>
        Private Function DrawCoverageConceptDiagram() As Bitmap
            Dim chartWidth As Integer = 950
            Dim chartHeight As Integer = 720
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim headFont As New Font("Segoe UI", 9, FontStyle.Bold)
                Dim textFont As New Font("Segoe UI", 8.0F)
                Dim smallFont As New Font("Segoe UI", 7.5F)
                Dim mathFont As New Font("Consolas", 7.0F)
                Dim faaBlue As Color = Color.FromArgb(46, 94, 168)
                Dim faaBlueLight As Color = Color.FromArgb(30, 46, 94, 168)

                g.DrawString("How Coverage-to-Pass (C/P) Is Computed at Each Strip", titleFont, Brushes.Black, 20, 8)

                Dim sigma As Double = 30.435
                Dim exTW As Double = 16 ' example tire width
                Dim pxPerInch As Single = 2.8F
                Dim axisMidX As Integer = chartWidth \ 2

                ' ===== PANEL A: Gaussian Wander Concept (top) =====
                Dim panelAY As Integer = 36
                g.DrawString("Panel A: Single Wheel — The aircraft tire wanders laterally with a Gaussian distribution", headFont, New SolidBrush(faaBlue), 20, panelAY)

                Dim axisY As Integer = panelAY + 140
                Dim bellH As Integer = 70

                ' Draw Gaussian bell curve
                Dim gaussPen As New Pen(faaBlue, 2)
                Dim gaussPts As New List(Of PointF)
                Dim fillPts As New List(Of PointF)
                fillPts.Add(New PointF(50, axisY))
                For px As Integer = 50 To chartWidth - 50 Step 2
                    Dim xIn As Double = (px - axisMidX) / pxPerInch
                    Dim gVal As Double = Math.Exp(-0.5 * (xIn / sigma) ^ 2)
                    Dim yPx As Single = CSng(axisY - gVal * bellH)
                    gaussPts.Add(New PointF(px, yPx))
                    fillPts.Add(New PointF(px, yPx))
                Next
                fillPts.Add(New PointF(chartWidth - 50, axisY))
                g.FillPolygon(New SolidBrush(faaBlueLight), fillPts.ToArray())
                If gaussPts.Count > 1 Then g.DrawLines(gaussPen, gaussPts.ToArray())

                ' Axis line
                g.DrawLine(Pens.Black, 50, axisY, chartWidth - 50, axisY)

                ' Tire rectangle at nominal position (center) — high contrast
                Dim twPx As Single = CSng(exTW * pxPerInch)
                Dim tireLeft As Single = axisMidX - twPx / 2
                g.FillRectangle(New SolidBrush(Color.FromArgb(255, 30, 30, 30)), tireLeft, axisY + 2, twPx, 18)
                g.DrawRectangle(New Pen(Color.FromArgb(80, 80, 80)), tireLeft, axisY + 2, twPx, 18)
                Dim tireLabel As String = "Tire (TW=" & Format(exTW, "0") & """)"
                Dim tireLblSize = g.MeasureString(tireLabel, smallFont)
                g.DrawString(tireLabel, smallFont, New SolidBrush(Color.FromArgb(255, 240, 200, 50)), tireLeft + (twPx - tireLblSize.Width) / 2, axisY + 4)

                ' sigma annotations
                For s As Integer = 1 To 3
                    Dim sPx As Single = CSng(s * sigma * pxPerInch)
                    Dim dimPen As New Pen(Color.Gray, 1)
                    dimPen.DashStyle = Drawing2D.DashStyle.Dot
                    g.DrawLine(dimPen, axisMidX + sPx, axisY - 5, axisMidX + sPx, axisY + 5)
                    g.DrawLine(dimPen, axisMidX - sPx, axisY - 5, axisMidX - sPx, axisY + 5)
                    g.DrawString(s.ToString() & ChrW(&H03C3), smallFont, Brushes.Gray, axisMidX + sPx - 6, axisY + 7)
                    dimPen.Dispose()
                Next
                ' Sigma label — positioned on the LEFT side of the bell to avoid overlapping eval strip
                Dim sigmaLblX As Single = CSng(axisMidX - 1.5 * sigma * pxPerInch)
                g.DrawString(ChrW(&H03C3) & " = 30.435""  (wander std. dev.)", smallFont, New SolidBrush(faaBlue), sigmaLblX, panelAY + 50)
                g.DrawString("0", smallFont, Brushes.Black, axisMidX - 3, axisY + 22)
                g.DrawString("Nominal wheel path centerline", smallFont, Brushes.Black, axisMidX - 60, axisY + 32)

                ' Show evaluation point and shaded C/P area
                Dim evalOff As Double = 30 ' example offset
                Dim evalPx As Single = CSng(axisMidX + evalOff * pxPerInch)
                Dim evalPen As New Pen(Color.Red, 1.5F)
                evalPen.DashStyle = Drawing2D.DashStyle.Dash
                g.DrawLine(evalPen, evalPx, panelAY + 50, evalPx, axisY + 20)
                g.DrawString("Evaluation strip", smallFont, Brushes.Red, evalPx + 4, panelAY + 50)
                g.DrawString("at offset = " & Format(evalOff, "0") & """", smallFont, Brushes.Red, evalPx + 4, panelAY + 60)

                ' Shade the coverage region under Gaussian
                ' C/P at this eval point = P(wander between evalOff - TW/2 and evalOff + TW/2)
                Dim aa As Double = evalOff - exTW / 2
                Dim bb As Double = evalOff + exTW / 2
                Dim shadePts As New List(Of PointF)
                Dim aaPixel As Integer = CInt(axisMidX + aa * pxPerInch)
                Dim bbPixel As Integer = CInt(axisMidX + bb * pxPerInch)
                shadePts.Add(New PointF(aaPixel, axisY))
                For px As Integer = aaPixel To bbPixel Step 2
                    Dim xIn As Double = (px - axisMidX) / pxPerInch
                    Dim gVal As Double = Math.Exp(-0.5 * (xIn / sigma) ^ 2)
                    shadePts.Add(New PointF(px, CSng(axisY - gVal * bellH)))
                Next
                shadePts.Add(New PointF(bbPixel, axisY))
                g.FillPolygon(New SolidBrush(Color.FromArgb(100, 255, 140, 0)), shadePts.ToArray())

                Dim cpVal As Double = GaussAreaCalc(aa, bb, sigma)
                g.DrawString("C/P = shaded area = " & Format(cpVal, "0.00000"), textFont, New SolidBrush(Color.FromArgb(200, 100, 0)), evalPx + 4, panelAY + 72)

                ' Explanation text
                g.DrawString("The tire covers the evaluation strip when the lateral wander places the tire edge over that strip.", textFont, Brushes.Black, 20, axisY + 42)
                g.DrawString("C/P(offset) = P(offset - TW/2  <  wander  <  offset + TW/2)  =  integral of Gaussian between those limits", mathFont, Brushes.DarkGray, 20, axisY + 58)

                ' ===== PANEL B: Resulting C/P curve for single wheel =====
                Dim panelBY As Integer = axisY + 90
                g.DrawString("Panel B: Resulting C/P Curve — One curve for single wheel, two peaks for dual wheel", headFont, New SolidBrush(faaBlue), 20, panelBY)

                Dim cpPlotTop As Integer = panelBY + 18
                Dim cpPlotH As Integer = 150
                Dim cpPlotLeft As Integer = 80
                Dim cpPlotW As Integer = chartWidth - 130
                Dim cpPlotBot As Integer = cpPlotTop + cpPlotH

                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), cpPlotLeft, cpPlotTop, cpPlotW, cpPlotH)
                g.DrawRectangle(Pens.DarkGray, cpPlotLeft, cpPlotTop, cpPlotW, cpPlotH)

                ' X-axis: offset 0 to 200
                Dim cpXMax As Double = 200
                Dim cpPxPerInch2 As Single = CSng(cpPlotW / cpXMax)

                ' Dual wheel spacing (declared here so Y-scale computation can use it)
                Dim dualSpacing As Double = 40

                ' Compute max C/P across BOTH single and dual curves for proper Y scaling
                Dim maxCP_any As Double = 0
                For i As Integer = 0 To 200
                    Dim cpSingle As Double = GaussAreaCalc(i - exTW / 2, i + exTW / 2, sigma)
                    If cpSingle > maxCP_any Then maxCP_any = cpSingle
                    Dim cpDual1 As Double = GaussAreaCalc(i + dualSpacing / 2 - exTW / 2, i + dualSpacing / 2 + exTW / 2, sigma)
                    Dim cpDual2 As Double = GaussAreaCalc(i - dualSpacing / 2 - exTW / 2, i - dualSpacing / 2 + exTW / 2, sigma)
                    If (cpDual1 + cpDual2) > maxCP_any Then maxCP_any = cpDual1 + cpDual2
                Next
                Dim cpYScale As Single = CSng(cpPlotH * 0.85 / Math.Max(maxCP_any, 0.001))

                ' Single wheel curve (blue)
                Dim singlePts As New List(Of PointF)
                For i As Integer = 0 To 200
                    Dim cp As Double = GaussAreaCalc(i - exTW / 2, i + exTW / 2, sigma)
                    singlePts.Add(New PointF(cpPlotLeft + CSng(i * cpPxPerInch2), cpPlotBot - CSng(cp * cpYScale)))
                Next
                g.DrawLines(New Pen(faaBlue, 2), singlePts.ToArray())

                ' Dual wheel curve (orange) — wheels at ±20" (example 40" dual spacing)
                Dim dualPts As New List(Of PointF)
                For i As Integer = 0 To 200
                    ' Wheel 1 at -dualSpacing/2, wheel 2 at +dualSpacing/2
                    Dim cp1 As Double = GaussAreaCalc(i + dualSpacing / 2 - exTW / 2, i + dualSpacing / 2 + exTW / 2, sigma)
                    Dim cp2 As Double = GaussAreaCalc(i - dualSpacing / 2 - exTW / 2, i - dualSpacing / 2 + exTW / 2, sigma)
                    dualPts.Add(New PointF(cpPlotLeft + CSng(i * cpPxPerInch2), cpPlotBot - CSng((cp1 + cp2) * cpYScale)))
                Next
                g.DrawLines(New Pen(Color.FromArgb(255, 200, 80, 0), 2), dualPts.ToArray())

                ' Individual wheel 1 and 2 contributions (dashed)
                Dim w1Pts As New List(Of PointF)
                Dim w2Pts As New List(Of PointF)
                For i As Integer = 0 To 200
                    Dim cp1 As Double = GaussAreaCalc(i + dualSpacing / 2 - exTW / 2, i + dualSpacing / 2 + exTW / 2, sigma)
                    Dim cp2 As Double = GaussAreaCalc(i - dualSpacing / 2 - exTW / 2, i - dualSpacing / 2 + exTW / 2, sigma)
                    w1Pts.Add(New PointF(cpPlotLeft + CSng(i * cpPxPerInch2), cpPlotBot - CSng(cp1 * cpYScale)))
                    w2Pts.Add(New PointF(cpPlotLeft + CSng(i * cpPxPerInch2), cpPlotBot - CSng(cp2 * cpYScale)))
                Next
                Dim dashPen1 As New Pen(Color.FromArgb(150, 200, 80, 0), 1)
                dashPen1.DashStyle = Drawing2D.DashStyle.Dash
                g.DrawLines(dashPen1, w1Pts.ToArray())
                g.DrawLines(dashPen1, w2Pts.ToArray())

                ' Axis labels
                For i As Integer = 0 To 4
                    Dim xVal As Integer = i * 50
                    Dim xPx As Single = cpPlotLeft + CSng(xVal * cpPxPerInch2)
                    g.DrawString(xVal.ToString(), smallFont, Brushes.Black, xPx - 6, cpPlotBot + 3)
                Next
                g.DrawString("Offset from nominal wheel path (inches)", smallFont, Brushes.Black, cpPlotLeft + cpPlotW \ 2 - 80, cpPlotBot + 16)
                g.DrawString("C/P", smallFont, Brushes.Black, cpPlotLeft - 22, cpPlotTop + cpPlotH \ 2 - 6)

                ' Legend — positioned below the plot area to avoid overlap
                Dim lgX As Integer = cpPlotLeft + 10
                Dim lgY As Integer = cpPlotBot + 30
                g.DrawLine(New Pen(faaBlue, 1.5F), lgX, lgY + 5, lgX + 16, lgY + 5)
                g.DrawString("Single wheel", smallFont, Brushes.Black, lgX + 20, lgY)
                Dim lgX2 As Integer = lgX + 120
                g.DrawLine(New Pen(Color.FromArgb(255, 200, 80, 0), 1.5F), lgX2, lgY + 5, lgX2 + 16, lgY + 5)
                g.DrawString("Dual wheel (40"" spacing)", smallFont, Brushes.Black, lgX2 + 20, lgY)
                Dim lgX3 As Integer = lgX2 + 190
                g.DrawLine(dashPen1, lgX3, lgY + 5, lgX3 + 16, lgY + 5)
                g.DrawString("Individual wheel contributions", smallFont, Brushes.Black, lgX3 + 20, lgY)

                ' ===== PANEL C: 41 strips visualization =====
                Dim panelCY As Integer = cpPlotBot + 52
                g.DrawString("Panel C: FAARFIELD evaluates " & CDF.NOFF.ToString() & " strips at " & Format(CDF.OFFSETINC, "0") &
                    """ intervals (offsets 0 to " & Format((CDF.NOFF - 1) * CDF.OFFSETINC, "0") & """)", headFont, New SolidBrush(faaBlue), 20, panelCY)

                Dim stripY As Integer = panelCY + 22
                Dim stripH As Integer = 25
                Dim stripScale As Single = CSng((chartWidth - 100) / ((CDF.NOFF - 1) * CDF.OFFSETINC))

                ' Draw strips
                For i As Integer = 0 To CDF.NOFF - 1
                    Dim xOff As Double = i * CDF.OFFSETINC
                    Dim xPx As Single = 50 + CSng(xOff * stripScale)
                    Dim stripW As Single = CSng(CDF.OFFSETINC * stripScale)
                    Dim alpha As Integer = If(i Mod 2 = 0, 30, 50)
                    g.FillRectangle(New SolidBrush(Color.FromArgb(alpha, faaBlue.R, faaBlue.G, faaBlue.B)), xPx, stripY, stripW, stripH)
                    g.DrawRectangle(New Pen(Color.FromArgb(80, faaBlue.R, faaBlue.G, faaBlue.B)), xPx, stripY, stripW, stripH)
                    If i Mod 5 = 0 Then
                        g.DrawString(Format(xOff, "0"), smallFont, Brushes.DarkGray, xPx - 3, stripY + stripH + 2)
                    End If
                Next
                g.DrawString("Only one side is evaluated (symmetry about the nominal wheel path centerline)", textFont, Brushes.Black, 50, stripY + stripH + 18)

                ' ===== PANEL D: Multi-wheel gear superposition summary =====
                Dim panelDY As Integer = stripY + stripH + 50
                g.DrawString("Panel D: For complex gears, C/P at each strip = sum of all individual wheel contributions", headFont, New SolidBrush(faaBlue), 20, panelDY)

                Dim summaryY As Integer = panelDY + 20
                g.DrawString("Single Wheel:  C/P(offset) = GaussArea(offset - TW/2, offset + TW/2, " & ChrW(&H03C3) & ")", mathFont, Brushes.Black, 40, summaryY)
                g.DrawString("Dual Wheel:    C/P(offset) = GaussArea for left wheel + GaussArea for right wheel", mathFont, Brushes.Black, 40, summaryY + 16)
                g.DrawString("Dual Tandem:   Same as dual, but each lateral track has 2 longitudinal wheels " & ChrW(&H2192) & " doubles coverage", mathFont, Brushes.Black, 40, summaryY + 32)
                g.DrawString("Complex gear:  C/P(offset) = " & ChrW(&H2211) & " GaussArea(offset - X_i - TW_i/2, offset - X_i + TW_i/2, " & ChrW(&H03C3) & ")  over all tires i", mathFont, Brushes.Black, 40, summaryY + 48)

                g.DrawString("Where X_i = lateral position of wheel i relative to the nominal wheel path centerline", textFont, Brushes.DarkGray, 60, summaryY + 68)
                g.DrawString("The entire gear assembly wanders as a rigid body " & ChrW(&H2014) & " individual wheels do not wander independently.", textFont, Brushes.DarkGray, 60, summaryY + 82)

                ' Border
                g.DrawRectangle(New Pen(Color.FromArgb(209, 213, 219), 1), 0, 0, chartWidth - 1, chartHeight - 1)

                ' Cleanup
                titleFont.Dispose()
                headFont.Dispose()
                textFont.Dispose()
                smallFont.Dispose()
                mathFont.Dispose()
                gaussPen.Dispose()
                evalPen.Dispose()
                dashPen1.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws a per-aircraft C/P visualization showing the actual C/P curve,
        ''' approximate wheel positions, and individual wheel contributions.
        ''' </summary>
        Private Function DrawWheelCPVisualization(det As clsAircraftDetail) As Bitmap
            Dim chartWidth As Integer = 900
            Dim chartHeight As Integer = 550
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                Dim faaBlue As Color = Color.FromArgb(46, 94, 168)
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim labelFont As New Font("Segoe UI", 9.5F)
                Dim smallFont As New Font("Segoe UI", 8.5F)
                Dim mathFont As New Font("Consolas", 7.5F)
                Dim axisTickFont As New Font("Segoe UI", 8.5F)
                Dim axisLabelFont As New Font("Segoe UI", 9.5F, FontStyle.Bold)

                ' Title
                Dim cpTitle = "C/P Analysis: " & det.ACName & " (" & det.GearType & ", TW=" & Format(det.TireWidth, "0.0") & """)"
                Dim cpTitleSize = g.MeasureString(cpTitle, titleFont)
                g.DrawString(cpTitle, titleFont, Brushes.Black, CSng((chartWidth - cpTitleSize.Width) / 2), 8)

                ' Plot area
                Dim marginLeft As Integer = 85
                Dim marginRight As Integer = 25
                Dim marginTop As Integer = 95
                Dim marginBottom As Integer = 70
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Determine Y range from actual C/P data
                Dim maxCtoP As Double = 0
                For ioff As Integer = 1 To CDF.NOFF
                    If det.CtoPByOffset(ioff) > maxCtoP Then maxCtoP = det.CtoPByOffset(ioff)
                Next
                If maxCtoP <= 0 Then maxCtoP = 0.001
                Dim yMax As Double = maxCtoP * 1.2
                Dim xMax As Double = (CDF.NOFF - 1) * CDF.OFFSETINC

                ' Plot background
                Dim plotRect As New Rectangle(marginLeft, marginTop, plotWidth, plotHeight)
                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), plotRect)
                g.DrawRectangle(Pens.DarkGray, plotRect)

                ' Grid
                Dim gridPen As New Pen(Color.FromArgb(210, 214, 220), 1)
                gridPen.DashStyle = Drawing2D.DashStyle.Dot
                Dim nGridY As Integer = 5
                For i As Integer = 0 To nGridY
                    Dim yVal As Double = yMax * i / nGridY
                    Dim yPx As Integer = CInt(marginTop + plotHeight - (yVal / yMax) * plotHeight)
                    If i > 0 AndAlso i < nGridY Then g.DrawLine(gridPen, marginLeft, yPx, marginLeft + plotWidth, yPx)
                    Dim yLabel = Format(yVal, "0.000")
                    Dim yLabelSize = g.MeasureString(yLabel, axisTickFont)
                    g.DrawString(yLabel, axisTickFont, Brushes.Black, marginLeft - yLabelSize.Width - 4, yPx - yLabelSize.Height / 2)
                Next
                Dim nGridX As Integer = 8
                For i As Integer = 0 To nGridX
                    Dim xVal As Double = xMax * i / nGridX
                    Dim xPx As Integer = CInt(marginLeft + (xVal / xMax) * plotWidth)
                    If i > 0 AndAlso i < nGridX Then g.DrawLine(gridPen, xPx, marginTop, xPx, marginTop + plotHeight)
                    Dim xLabel = Format(xVal, "0")
                    Dim xLabelSize = g.MeasureString(xLabel, axisTickFont)
                    g.DrawString(xLabel, axisTickFont, Brushes.Black, xPx - xLabelSize.Width / 2, marginTop + plotHeight + 8)
                Next

                ' X-axis label (centered)
                Dim xAxisLabel = "Offset from nominal wheel path centerline (inches)"
                Dim xAxisSize = g.MeasureString(xAxisLabel, axisLabelFont)
                g.DrawString(xAxisLabel, axisLabelFont, Brushes.Black, CSng(marginLeft + (plotWidth - xAxisSize.Width) / 2), chartHeight - 22)

                ' Y-axis label (rotated)
                g.TranslateTransform(22, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(-90)
                Dim cpYAxisLabel = "Coverage / Pass (C/P)"
                Dim cpYAxisSize = g.MeasureString(cpYAxisLabel, axisLabelFont)
                g.DrawString(cpYAxisLabel, axisLabelFont, Brushes.Black, -cpYAxisSize.Width / 2, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' Tire width band at top
                Dim halfTW As Double = det.TireWidth / 2.0
                If halfTW > 0 AndAlso halfTW < xMax Then
                    Dim twXPx As Single = CSng(marginLeft + (halfTW / xMax) * plotWidth)
                    g.FillRectangle(New SolidBrush(Color.FromArgb(20, 255, 165, 0)), marginLeft, marginTop, twXPx - marginLeft, plotHeight)
                    Dim twPen As New Pen(Color.FromArgb(150, 255, 140, 0), 1.2F)
                    twPen.DashStyle = Drawing2D.DashStyle.DashDot
                    g.DrawLine(twPen, twXPx, marginTop, twXPx, marginTop + plotHeight)
                    g.DrawString("TW/2", smallFont, New SolidBrush(Color.FromArgb(200, 120, 0)), twXPx + 2, marginTop + plotHeight - 12)
                    twPen.Dispose()
                End If

                ' Compute and draw approximate individual wheel contributions
                ' Estimate wheel positions from gear type
                Dim sigma As Double = 30.435
                Dim tw As Double = det.TireWidth
                Dim wheelPositions As New List(Of Double)
                Dim wheelMultipliers As New List(Of Integer) ' how many passes per position (tandem doubles)

                ' Infer approximate wheel layout from GearType and C/P curve
                Dim gearStr As String = If(det.GearType, "").ToLower()
                If gearStr.Contains("single") OrElse det.NGearLoads = 1 Then
                    wheelPositions.Add(0)
                    wheelMultipliers.Add(1)
                ElseIf gearStr.Contains("dual") AndAlso gearStr.Contains("tandem") Then
                    ' Dual tandem: estimate dual spacing from C/P peak shape
                    ' Find offset of max C/P — for a dual gear, peak is near 0 but shape is broader
                    Dim bestD As Double = 0
                    Dim bestErr As Double = Double.MaxValue
                    ' Search for dual spacing that best matches the actual C/P curve
                    For tryD As Double = 10 To 80 Step 2
                        Dim err As Double = 0
                        For ioff As Integer = 1 To Math.Min(CDF.NOFF, 15)
                            Dim offset As Double = (ioff - 1) * CDF.OFFSETINC
                            Dim cp1 As Double = GaussAreaCalc(offset + tryD / 2 - tw / 2, offset + tryD / 2 + tw / 2, sigma)
                            Dim cp2 As Double = GaussAreaCalc(offset - tryD / 2 - tw / 2, offset - tryD / 2 + tw / 2, sigma)
                            Dim cpModel As Double = (cp1 + cp2) * 2 ' x2 for tandem
                            Dim cpActual As Double = det.CtoPByOffset(ioff)
                            err += (cpModel - cpActual) ^ 2
                        Next
                        If err < bestErr Then bestErr = err : bestD = tryD
                    Next
                    If bestD < 1 Then bestD = 30 ' fallback
                    wheelPositions.Add(-bestD / 2)
                    wheelPositions.Add(bestD / 2)
                    wheelMultipliers.Add(2) ' tandem doubles
                    wheelMultipliers.Add(2)
                ElseIf gearStr.Contains("dual") Then
                    ' Dual wheel: find best spacing
                    Dim bestD As Double = 0
                    Dim bestErr As Double = Double.MaxValue
                    For tryD As Double = 10 To 80 Step 2
                        Dim err As Double = 0
                        For ioff As Integer = 1 To Math.Min(CDF.NOFF, 15)
                            Dim offset As Double = (ioff - 1) * CDF.OFFSETINC
                            Dim cp1 As Double = GaussAreaCalc(offset + tryD / 2 - tw / 2, offset + tryD / 2 + tw / 2, sigma)
                            Dim cp2 As Double = GaussAreaCalc(offset - tryD / 2 - tw / 2, offset - tryD / 2 + tw / 2, sigma)
                            Dim cpModel As Double = cp1 + cp2
                            err += (cpModel - det.CtoPByOffset(ioff)) ^ 2
                        Next
                        If err < bestErr Then bestErr = err : bestD = tryD
                    Next
                    If bestD < 1 Then bestD = 30
                    wheelPositions.Add(-bestD / 2)
                    wheelPositions.Add(bestD / 2)
                    wheelMultipliers.Add(1)
                    wheelMultipliers.Add(1)
                Else
                    ' Unknown gear type — treat as single wheel for display
                    wheelPositions.Add(0)
                    wheelMultipliers.Add(1)
                End If

                ' Draw individual wheel contribution curves (dashed, colored)
                Dim wheelColors() As Color = {
                    Color.FromArgb(44, 160, 44),
                    Color.FromArgb(148, 103, 189),
                    Color.FromArgb(227, 119, 194),
                    Color.FromArgb(188, 189, 34)
                }

                For wIdx As Integer = 0 To wheelPositions.Count - 1
                    Dim wPos As Double = wheelPositions(wIdx)
                    Dim wMult As Integer = wheelMultipliers(wIdx)
                    Dim wColor As Color = wheelColors(wIdx Mod wheelColors.Length)
                    Dim wPen As New Pen(Color.FromArgb(180, wColor.R, wColor.G, wColor.B), 1.5F)
                    wPen.DashStyle = Drawing2D.DashStyle.Dash

                    Dim wPts As New List(Of PointF)
                    For ioff As Integer = 1 To CDF.NOFF
                        Dim offset As Double = (ioff - 1) * CDF.OFFSETINC
                        Dim cpW As Double = GaussAreaCalc(offset - wPos - tw / 2, offset - wPos + tw / 2, sigma) * wMult
                        Dim xPx As Single = CSng(marginLeft + (offset / xMax) * plotWidth)
                        Dim yPx As Single = CSng(marginTop + plotHeight - (cpW / yMax) * plotHeight)
                        wPts.Add(New PointF(xPx, yPx))
                    Next
                    If wPts.Count > 1 Then g.DrawLines(wPen, wPts.ToArray())

                    wPen.Dispose()
                Next

                ' Draw actual C/P curve (solid, thick, dark blue)
                Dim actualPen As New Pen(faaBlue, 2.5F)
                Dim actualPts As New List(Of PointF)
                For ioff As Integer = 1 To CDF.NOFF
                    Dim offset As Double = (ioff - 1) * CDF.OFFSETINC
                    Dim xPx As Single = CSng(marginLeft + (offset / xMax) * plotWidth)
                    Dim yPx As Single = CSng(marginTop + plotHeight - (det.CtoPByOffset(ioff) / yMax) * plotHeight)
                    actualPts.Add(New PointF(xPx, yPx))
                Next
                If actualPts.Count > 1 Then g.DrawLines(actualPen, actualPts.ToArray())

                ' Draw reconstructed sum (thin orange, to show match)
                Dim sumPen As New Pen(Color.FromArgb(180, 255, 127, 14), 1.5F)
                sumPen.DashStyle = Drawing2D.DashStyle.Dot
                Dim sumPts As New List(Of PointF)
                For ioff As Integer = 1 To CDF.NOFF
                    Dim offset As Double = (ioff - 1) * CDF.OFFSETINC
                    Dim cpSum As Double = 0
                    For wIdx As Integer = 0 To wheelPositions.Count - 1
                        cpSum += GaussAreaCalc(offset - wheelPositions(wIdx) - tw / 2, offset - wheelPositions(wIdx) + tw / 2, sigma) * wheelMultipliers(wIdx)
                    Next
                    Dim xPx As Single = CSng(marginLeft + (offset / xMax) * plotWidth)
                    Dim yPx As Single = CSng(marginTop + plotHeight - (cpSum / yMax) * plotHeight)
                    sumPts.Add(New PointF(xPx, yPx))
                Next
                If sumPts.Count > 1 Then g.DrawLines(sumPen, sumPts.ToArray())

                ' Gear diagram at top (simple schematic)
                Dim gearY As Integer = 34
                Dim gearCenterX As Integer = marginLeft + 80
                g.DrawString("Gear layout (approx.):", smallFont, Brushes.DarkGray, marginLeft, gearY)
                For wIdx As Integer = 0 To wheelPositions.Count - 1
                    Dim wPos As Double = wheelPositions(wIdx)
                    Dim wMult As Integer = wheelMultipliers(wIdx)
                    Dim wColor As Color = wheelColors(wIdx Mod wheelColors.Length)
                    Dim tireDrawX As Single = CSng(gearCenterX + wPos * 1.5)
                    Dim tireDrawW As Single = CSng(tw * 1.5)
                    g.FillRectangle(New SolidBrush(Color.FromArgb(180, wColor.R, wColor.G, wColor.B)), tireDrawX - tireDrawW / 2, gearY + 14, tireDrawW, 16)
                    g.DrawRectangle(New Pen(wColor), tireDrawX - tireDrawW / 2, gearY + 14, tireDrawW, 16)
                    If wMult > 1 Then
                        g.FillRectangle(New SolidBrush(Color.FromArgb(120, wColor.R, wColor.G, wColor.B)), tireDrawX - tireDrawW / 2, gearY + 32, tireDrawW, 16)
                        g.DrawRectangle(New Pen(wColor), tireDrawX - tireDrawW / 2, gearY + 32, tireDrawW, 16)
                    End If
                Next

                ' Legend — compact box in upper-right (curves peak near offset=0 on the left, so upper-right is clear)
                Dim legendFont As New Font("Segoe UI", 8.5F)
                Dim legendLineH As Integer = 16
                Dim legendCount As Integer = wheelPositions.Count + 2 ' wheels + actual + reconstructed
                Dim lgH As Integer = legendCount * legendLineH + 10
                Dim lgW As Integer = 200
                Dim lgX As Integer = marginLeft + plotWidth - lgW - 10
                Dim lgY2 As Integer = marginTop + 10
                g.FillRectangle(New SolidBrush(Color.FromArgb(240, 245, 245, 245)), lgX, lgY2, lgW, lgH)
                g.DrawRectangle(New Pen(Color.FromArgb(200, 204, 210), 1), lgX, lgY2, lgW, lgH)

                ' Actual C/P entry
                Dim lgRow As Integer = lgY2 + 5
                g.DrawLine(actualPen, lgX + 6, lgRow + 6, lgX + 24, lgRow + 6)
                g.DrawString("Actual C/P (FAARFIELD)", legendFont, Brushes.Black, lgX + 28, lgRow)
                lgRow += legendLineH

                ' Reconstructed sum entry
                g.DrawLine(sumPen, lgX + 6, lgRow + 6, lgX + 24, lgRow + 6)
                g.DrawString("Reconstructed sum", legendFont, Brushes.Black, lgX + 28, lgRow)
                lgRow += legendLineH

                ' Wheel entries
                For wIdx As Integer = 0 To wheelPositions.Count - 1
                    Dim wPos As Double = wheelPositions(wIdx)
                    Dim wMult As Integer = wheelMultipliers(wIdx)
                    Dim wColor As Color = wheelColors(wIdx Mod wheelColors.Length)
                    Dim wLabel As String = "Wheel at " & If(wPos >= 0, "+", "") & Format(wPos, "0") & """"
                    If wMult > 1 Then wLabel &= " (" & ChrW(&H00D7) & wMult & ")"
                    Dim wLegPen As New Pen(Color.FromArgb(180, wColor.R, wColor.G, wColor.B), 1.2F)
                    wLegPen.DashStyle = Drawing2D.DashStyle.Dash
                    g.DrawLine(wLegPen, lgX + 6, lgRow + 6, lgX + 24, lgRow + 6)
                    g.DrawString(wLabel, legendFont, New SolidBrush(wColor), lgX + 28, lgRow)
                    wLegPen.Dispose()
                    lgRow += legendLineH
                Next
                legendFont.Dispose()

                ' Cleanup
                titleFont.Dispose()
                labelFont.Dispose()
                smallFont.Dispose()
                mathFont.Dispose()
                axisTickFont.Dispose()
                axisLabelFont.Dispose()
                gridPen.Dispose()
                actualPen.Dispose()
                sumPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws a plan-view gear configuration diagram showing wheel positions,
        ''' CDF offset strips, dimension annotations, and Gaussian wander overlay.
        ''' </summary>
        Private Function DrawGearConfiguration(det As clsAircraftDetail, criticalOffset As Integer, lengthUnit As String) As Bitmap
            Dim chartWidth As Integer = 1100
            Dim chartHeight As Integer = 600
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                Dim faaBlue As Color = Color.FromArgb(46, 94, 168)
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim labelFont As New Font("Segoe UI", 9.5F)
                Dim tickFont As New Font("Segoe UI", 8.5F)
                Dim legendFont As New Font("Segoe UI", 8.0F)
                Dim annotFont As New Font("Segoe UI", 7.5F)

                ' Title
                Dim titleText = "Gear Configuration: " & det.ACName & " (" & det.GearType & ")"
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 8)

                ' Plot area
                Dim marginLeft As Integer = 80
                Dim marginRight As Integer = 40
                Dim marginTop As Integer = 40
                Dim marginBottom As Integer = 60
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Build list of ALL physical tire positions (stored + inferred dual partners)
                Dim allTiresX As New List(Of Single)
                Dim allTiresY As New List(Of Single)
                For i As Integer = 1 To det.NWheels
                    allTiresX.Add(det.WheelX(i))
                    allTiresY.Add(det.WheelY(i))
                Next
                ' Infer dual partners when DualSpacing > 0
                If det.DualSpacing > 0 Then
                    Dim partnersX As New List(Of Single)
                    Dim partnersY As New List(Of Single)
                    For i As Integer = 0 To allTiresX.Count - 1
                        Dim partnerX As Single = allTiresX(i) + det.DualSpacing
                        Dim hasPartner As Boolean = False
                        For j As Integer = 0 To allTiresX.Count - 1
                            If j <> i AndAlso Math.Abs(allTiresX(j) - partnerX) < 1 AndAlso Math.Abs(allTiresY(j) - allTiresY(i)) < 1 Then
                                hasPartner = True
                                Exit For
                            End If
                        Next
                        If Not hasPartner Then
                            partnersX.Add(partnerX)
                            partnersY.Add(allTiresY(i))
                        End If
                    Next
                    allTiresX.AddRange(partnersX)
                    allTiresY.AddRange(partnersY)
                End If

                ' Find wheel coordinate ranges (using all physical tires)
                Dim minX As Single = Single.MaxValue, maxX As Single = Single.MinValue
                Dim minY As Single = Single.MaxValue, maxY As Single = Single.MinValue
                For i As Integer = 0 To allTiresX.Count - 1
                    If allTiresX(i) < minX Then minX = allTiresX(i)
                    If allTiresX(i) > maxX Then maxX = allTiresX(i)
                    If allTiresY(i) < minY Then minY = allTiresY(i)
                    If allTiresY(i) > maxY Then maxY = allTiresY(i)
                Next
                ' Pad range by tire width + margin
                Dim pad As Single = Math.Max(det.TireWidth * 2, 30)
                minX -= pad : maxX += pad
                minY -= pad : maxY += pad

                ' Extend X range for bilateral CDF strips
                Dim centerX As Single = (minX + pad + maxX - pad) / 2
                Dim bilateralExtent As Single = CSng((CDF.NOFF - 1) * CDF.OFFSETINC) + pad
                minX = Math.Min(minX, centerX - bilateralExtent)
                maxX = Math.Max(maxX, centerX + bilateralExtent)

                ' Scale: map gear coordinates to plot area
                ' X (lateral) -> horizontal, Y (longitudinal) -> vertical
                Dim rangeX As Single = Math.Max(maxX - minX, 1)
                Dim rangeY As Single = Math.Max(maxY - minY, 1)
                Dim scaleX As Single = CSng(plotWidth / rangeX)
                Dim scaleY As Single = CSng(plotHeight / rangeY)
                Dim scale As Single = Math.Min(scaleX, scaleY) ' uniform scale
                ' Center the drawing
                Dim usedW As Single = rangeX * scale
                Dim usedH As Single = rangeY * scale
                Dim offsetX As Single = marginLeft + (plotWidth - usedW) / 2
                Dim offsetY As Single = marginTop + (plotHeight - usedH) / 2

                ' Transform functions
                Dim toPixelX = Function(wx As Single) offsetX + (wx - minX) * scale
                Dim toPixelY = Function(wy As Single) offsetY + (maxY - wy) * scale  ' flip Y

                ' Draw CDF offset strips (vertical dashed lines at wheel path centerline)
                ' The strips are lateral offsets from the nominal wheel path
                Dim stripPen As New Pen(Color.FromArgb(60, 160, 160, 160), 0.5F)
                stripPen.DashStyle = Drawing2D.DashStyle.Dash
                Dim critStripPen As New Pen(Color.FromArgb(180, 220, 50, 50), 1.0F)
                critStripPen.DashStyle = Drawing2D.DashStyle.Dash

                ' Draw bilateral CDF offset strips (-40 to +40 = 81 strips)
                For ioff As Integer = -(CDF.NOFF - 1) To (CDF.NOFF - 1)
                    Dim offsetInches As Single = CSng(ioff * CDF.OFFSETINC)
                    Dim stripXpos As Single = centerX + offsetInches
                    If stripXpos >= minX AndAlso stripXpos <= maxX Then
                        Dim px As Single = toPixelX(stripXpos)
                        If Math.Abs(ioff) = criticalOffset - 1 AndAlso ioff <> 0 Then
                            g.DrawLine(critStripPen, px, CSng(marginTop), px, CSng(marginTop + plotHeight))
                            If ioff > 0 Then
                                Dim critInches As Integer = CInt((criticalOffset - 1) * CDF.OFFSETINC)
                                Dim critText As String = "Critical: " & ChrW(&H00B1) & critInches & " " & lengthUnit
                                g.DrawString(critText, annotFont, Brushes.Red, px - 30, CSng(marginTop - 12))
                            End If
                        ElseIf ioff <> 0 Then
                            g.DrawLine(stripPen, px, CSng(marginTop), px, CSng(marginTop + plotHeight))
                        End If
                        If ioff Mod 10 = 0 AndAlso ioff <> 0 Then
                            Dim stripLabel = If(ioff > 0, "+" & Format(offsetInches, "0"), Format(offsetInches, "0"))
                            g.DrawString(stripLabel, annotFont, Brushes.Gray, px - 8, CSng(marginTop + plotHeight + 2))
                        End If
                    End If
                Next

                ' Draw prominent zero-line (centerline)
                Dim zeroPx As Single = toPixelX(centerX)
                Dim zeroPen As New Pen(Color.Black, 1.5F)
                g.DrawLine(zeroPen, zeroPx, CSng(marginTop), zeroPx, CSng(marginTop + plotHeight))
                g.DrawString("0", annotFont, Brushes.Black, zeroPx - 3, CSng(marginTop + plotHeight + 2))
                zeroPen.Dispose()

                ' Per-tire Gaussian wander overlays (sigma = 30.435 in., one per physical tire)
                Dim sigma As Double = 30.435
                Dim gaussMax As Double = 1.0 / (sigma * Math.Sqrt(2 * Math.PI))
                Dim gaussHeight As Single = 40  ' max pixel height for the bell curve
                Dim tireGaussColors() As Color = {
                    Color.FromArgb(46, 94, 168),    ' blue
                    Color.FromArgb(214, 39, 40),     ' red
                    Color.FromArgb(44, 160, 44),     ' green
                    Color.FromArgb(255, 127, 14)     ' orange
                }
                For iTire As Integer = 0 To allTiresX.Count - 1
                    Dim tireCenter As Single = allTiresX(iTire)
                    Dim gColor As Color = tireGaussColors(iTire Mod tireGaussColors.Length)
                    Dim gaussPts As New List(Of PointF)
                    For px As Integer = marginLeft To marginLeft + plotWidth
                        Dim wx As Single = minX + CSng((px - offsetX) / scale)
                        Dim dist As Double = wx - tireCenter
                        Dim gVal As Double = Math.Exp(-dist * dist / (2 * sigma * sigma)) / (sigma * Math.Sqrt(2 * Math.PI))
                        Dim py As Single = CSng(marginTop + plotHeight - (gVal / gaussMax) * gaussHeight)
                        gaussPts.Add(New PointF(CSng(px), py))
                    Next
                    If gaussPts.Count > 2 Then
                        Dim fillPts As New List(Of PointF)
                        fillPts.Add(New PointF(gaussPts(0).X, CSng(marginTop + plotHeight)))
                        fillPts.AddRange(gaussPts)
                        fillPts.Add(New PointF(gaussPts(gaussPts.Count - 1).X, CSng(marginTop + plotHeight)))
                        g.FillPolygon(New SolidBrush(Color.FromArgb(20, gColor.R, gColor.G, gColor.B)), fillPts.ToArray())
                        g.DrawLines(New Pen(Color.FromArgb(100, gColor.R, gColor.G, gColor.B), 1.0F), gaussPts.ToArray())
                    End If
                Next

                ' Draw all physical tires as filled ellipses (per-wheel coloring).
                ' Pass 1 collects wheel pixel bounds so Pass 2 can skip labels that
                ' would overlap another wheel or another label on dense gears
                ' (A380/B747/B777).
                Dim tireRadius As Single = det.TireWidth / 2
                Dim wheelRects As New List(Of RectangleF)()
                For i As Integer = 0 To allTiresX.Count - 1
                    Dim cx As Single = toPixelX(allTiresX(i))
                    Dim cy As Single = toPixelY(allTiresY(i))
                    Dim rPx As Single = tireRadius * scale
                    rPx = Math.Max(rPx, 6)
                    rPx = Math.Min(rPx, 40)
                    Dim wColor As Color = tireGaussColors(i Mod tireGaussColors.Length)
                    Using fillBrush As New SolidBrush(Color.FromArgb(140, wColor.R, wColor.G, wColor.B)),
                          outlinePen As New Pen(wColor, 1.0F)
                        g.FillEllipse(fillBrush, cx - rPx, cy - rPx, rPx * 2, rPx * 2)
                        g.DrawEllipse(outlinePen, cx - rPx, cy - rPx, rPx * 2, rPx * 2)
                    End Using
                    wheelRects.Add(New RectangleF(cx - rPx, cy - rPx, rPx * 2, rPx * 2))
                Next

                ' Pass 2: place wheel coordinate labels with bounding-box collision
                ' detection. Try 4 candidate positions per wheel; skip label if none fit.
                Dim labelRects As New List(Of RectangleF)()
                For i As Integer = 0 To allTiresX.Count - 1
                    Dim cx As Single = toPixelX(allTiresX(i))
                    Dim cy As Single = toPixelY(allTiresY(i))
                    Dim rPx As Single = Math.Max(6, Math.Min(40, tireRadius * scale))
                    Dim coordLabel = "(" & Format(allTiresX(i), "0.0") & ", " & Format(allTiresY(i), "0.0") & ")"
                    Dim sz As SizeF = g.MeasureString(coordLabel, annotFont)
                    Dim candidates As PointF() = {
                        New PointF(cx + rPx + 2, cy - sz.Height / 2),
                        New PointF(cx - rPx - sz.Width - 2, cy - sz.Height / 2),
                        New PointF(cx - sz.Width / 2, cy - rPx - sz.Height - 2),
                        New PointF(cx - sz.Width / 2, cy + rPx + 2)
                    }
                    For Each pt In candidates
                        Dim rc As New RectangleF(pt.X, pt.Y, sz.Width, sz.Height)
                        Dim collides As Boolean = False
                        For Each wr In wheelRects
                            If rc.IntersectsWith(wr) Then collides = True : Exit For
                        Next
                        If Not collides Then
                            For Each lr In labelRects
                                If rc.IntersectsWith(lr) Then collides = True : Exit For
                            Next
                        End If
                        If Not collides Then
                            g.DrawString(coordLabel, annotFont, Brushes.Black, rc.X, rc.Y)
                            labelRects.Add(rc)
                            Exit For
                        End If
                    Next
                Next

                ' Dimension annotations
                Dim dimPen As New Pen(Color.FromArgb(180, 80, 80, 80), 0.8F)
                Dim dimFont As New Font("Segoe UI", 7.5F)

                ' Dual spacing annotation (find a pair of tires at same Y with different X)
                If det.DualSpacing > 0 AndAlso allTiresX.Count >= 2 Then
                    Dim w1 As Integer = 0, w2 As Integer = 1
                    For i As Integer = 0 To allTiresX.Count - 2
                        For j As Integer = i + 1 To allTiresX.Count - 1
                            If Math.Abs(allTiresY(i) - allTiresY(j)) < 1 AndAlso Math.Abs(allTiresX(i) - allTiresX(j)) > 1 Then
                                w1 = i : w2 = j
                                GoTo FoundDualPair
                            End If
                        Next
                    Next
FoundDualPair:
                    Dim px1 As Single = toPixelX(allTiresX(w1))
                    Dim px2 As Single = toPixelX(allTiresX(w2))
                    Dim py As Single = toPixelY(allTiresY(w1)) - tireRadius * scale - 12
                    py = Math.Max(py, CSng(marginTop + 5))
                    g.DrawLine(dimPen, px1, py, px2, py)
                    g.DrawLine(dimPen, px1, py - 3, px1, py + 3)
                    g.DrawLine(dimPen, px2, py - 3, px2, py + 3)
                    Dim dualLabel = Format(det.DualSpacing, "0.0") & """ dual"
                    Dim dualSize = g.MeasureString(dualLabel, dimFont)
                    g.DrawString(dualLabel, dimFont, Brushes.DimGray, (px1 + px2) / 2 - dualSize.Width / 2, py - dualSize.Height - 1)
                End If

                ' Tandem spacing annotation (find a pair at same X with different Y)
                If det.TandemSpacing > 0 AndAlso allTiresX.Count >= 2 Then
                    Dim w1 As Integer = 0, w2 As Integer = 1
                    For i As Integer = 0 To allTiresX.Count - 2
                        For j As Integer = i + 1 To allTiresX.Count - 1
                            If Math.Abs(allTiresX(i) - allTiresX(j)) < 1 AndAlso Math.Abs(allTiresY(i) - allTiresY(j)) > 1 Then
                                w1 = i : w2 = j
                                GoTo FoundTandemPair
                            End If
                        Next
                    Next
FoundTandemPair:
                    Dim py1 As Single = toPixelY(allTiresY(w1))
                    Dim py2 As Single = toPixelY(allTiresY(w2))
                    Dim wheelX As Single = toPixelX(allTiresX(w1))
                    Dim dimOffset As Single = tireRadius * scale + 15
                    Dim tanLabel = Format(det.TandemSpacing, "0.0") & """ tandem"
                    Dim tanSize = g.MeasureString(tanLabel, dimFont)
                    ' Pick the side (right preferred) that leaves room for the label,
                    ' so wide gears (A380/B747/B777) don't jam text into the axis.
                    Dim rightDim As Single = wheelX + dimOffset
                    Dim leftDim As Single = wheelX - dimOffset
                    Dim px As Single
                    Dim labelOnLeft As Boolean = False
                    If rightDim + tanSize.Width + 4 <= marginLeft + plotWidth - 2 Then
                        px = rightDim
                    ElseIf leftDim - tanSize.Width - 4 >= marginLeft + 2 Then
                        px = leftDim
                        labelOnLeft = True
                    Else
                        px = Math.Min(rightDim, CSng(marginLeft + plotWidth - 5))
                    End If
                    g.DrawLine(dimPen, px, py1, px, py2)
                    g.DrawLine(dimPen, px - 3, py1, px + 3, py1)
                    g.DrawLine(dimPen, px - 3, py2, px + 3, py2)
                    Dim midY As Single = (py1 + py2) / 2
                    Dim labelX As Single = If(labelOnLeft, px - 4 - tanSize.Width, px + 4)
                    g.DrawString(tanLabel, dimFont, Brushes.DimGray, labelX, midY - tanSize.Height / 2)
                End If

                ' Contact area annotation
                If det.ContactArea > 0 Then
                    Dim caText = "Contact area: " & Format(det.ContactArea, "0.0") & " " & lengthUnit & ChrW(&H00B2)
                    g.DrawString(caText, annotFont, Brushes.DimGray, CSng(marginLeft + 5), CSng(marginTop + plotHeight + 18))
                End If

                ' Gaussian sigma annotation
                g.DrawString(ChrW(&H03C3) & " = 30.435 in. (Gaussian lateral wander)", annotFont, New SolidBrush(faaBlue), CSng(marginLeft + 5), CSng(marginTop + plotHeight + 30))

                ' Axes
                g.DrawLine(Pens.Black, marginLeft, marginTop, marginLeft, marginTop + plotHeight)
                g.DrawLine(Pens.Black, marginLeft, marginTop + plotHeight, marginLeft + plotWidth, marginTop + plotHeight)
                g.DrawString("Lateral position (" & lengthUnit & ")", labelFont, Brushes.Black, CSng(marginLeft + plotWidth / 2 - 50), CSng(marginTop + plotHeight + 42))

                ' Y-axis label (drawn vertically)
                Dim yLabel = "Longitudinal (" & lengthUnit & ")"
                Dim sf As New StringFormat()
                sf.FormatFlags = StringFormatFlags.DirectionVertical
                g.DrawString(yLabel, labelFont, Brushes.Black, 5, CSng(marginTop + plotHeight / 2 - 30), sf)
                sf.Dispose()

                ' Legend — per-wheel entries, collision detection, background
                Dim swatchSz As Integer = 14
                Dim lgRowH As Integer = 20
                Dim lgPad As Integer = 8

                ' Determine number of per-wheel entries (cap at 4 for readability)
                Dim usePerWheel As Boolean = allTiresX.Count > 1 AndAlso allTiresX.Count <= 4
                Dim nTireEntries As Integer = If(usePerWheel, allTiresX.Count, 1)
                Dim nWanderEntries As Integer = If(usePerWheel, allTiresX.Count, 1)
                Dim nLgEntries As Integer = nTireEntries + nWanderEntries + 3 ' + strips + critical + centerline

                ' Measure widest legend label
                Dim lgLabels As New List(Of String)
                If usePerWheel Then
                    For t As Integer = 0 To allTiresX.Count - 1
                        lgLabels.Add("Wheel " & (t + 1))
                    Next
                    For t As Integer = 0 To allTiresX.Count - 1
                        lgLabels.Add("Wander — Wh." & (t + 1) & " (" & ChrW(&H03C3) & "=30.4"")")
                    Next
                Else
                    If allTiresX.Count > 4 Then
                        lgLabels.Add("Tire patches (" & allTiresX.Count & " wheels)")
                    Else
                        lgLabels.Add("Tire contact patch")
                    End If
                    lgLabels.Add("Lateral wander (" & ChrW(&H03C3) & "=30.4"")")
                End If
                lgLabels.Add("Eval. strips (" & Format(CDF.OFFSETINC, "0") & " " & lengthUnit & " spacing)")
                lgLabels.Add("Critical strip (max CDF)")
                lgLabels.Add("Wheel path centerline (0)")

                Dim maxLabelW As Single = 0
                For Each lbl In lgLabels
                    Dim sz = g.MeasureString(lbl, legendFont)
                    If sz.Width > maxLabelW Then maxLabelW = sz.Width
                Next
                Dim lgW As Integer = CInt(maxLabelW + swatchSz + lgPad * 3)
                Dim lgH As Integer = nLgEntries * lgRowH + lgPad * 2

                ' Default position: upper-right
                Dim lgDefaultX As Integer = chartWidth - marginRight - lgW - 5
                Dim lgDefaultY As Integer = marginTop + 5
                Dim lgFinalX As Integer = lgDefaultX
                Dim lgFinalY As Integer = lgDefaultY

                ' Collision detection: check if any tire circle or coord label overlaps legend
                Dim lgBounds As New RectangleF(lgFinalX, lgFinalY, lgW, lgH)
                Dim lgCollides As Boolean = False
                For i As Integer = 0 To allTiresX.Count - 1
                    Dim tcx As Single = toPixelX(allTiresX(i))
                    Dim tcy As Single = toPixelY(allTiresY(i))
                    Dim trPx As Single = Math.Min(Math.Max(tireRadius * scale, 6), 40)
                    Dim tireRect As New RectangleF(tcx - trPx, tcy - trPx, trPx * 2, trPx * 2)
                    Dim coordSz = g.MeasureString("(-999.0, 999.0)", annotFont)
                    Dim labelRect As New RectangleF(tcx + trPx + 2, tcy - 5, coordSz.Width, coordSz.Height)
                    If lgBounds.IntersectsWith(tireRect) OrElse lgBounds.IntersectsWith(labelRect) Then
                        lgCollides = True
                        Exit For
                    End If
                Next
                If lgCollides Then lgFinalX = marginLeft + 5

                ' Draw legend background (semi-transparent with rounded corners)
                Dim lgBgRect As New RectangleF(lgFinalX, lgFinalY, lgW, lgH)
                Using lgBgPath As New Drawing2D.GraphicsPath()
                    Dim lgCr As Single = 3.0F
                    lgBgPath.AddArc(lgBgRect.X, lgBgRect.Y, lgCr * 2, lgCr * 2, 180, 90)
                    lgBgPath.AddArc(lgBgRect.Right - lgCr * 2, lgBgRect.Y, lgCr * 2, lgCr * 2, 270, 90)
                    lgBgPath.AddArc(lgBgRect.Right - lgCr * 2, lgBgRect.Bottom - lgCr * 2, lgCr * 2, lgCr * 2, 0, 90)
                    lgBgPath.AddArc(lgBgRect.X, lgBgRect.Bottom - lgCr * 2, lgCr * 2, lgCr * 2, 90, 90)
                    lgBgPath.CloseFigure()
                    g.FillPath(New SolidBrush(Color.FromArgb(230, 255, 255, 255)), lgBgPath)
                    g.DrawPath(New Pen(Color.FromArgb(213, 220, 230), 1.0F), lgBgPath)
                End Using

                ' Draw legend entries
                Dim lgCurY As Integer = lgFinalY + lgPad
                Dim lgSwX As Integer = lgFinalX + lgPad
                Dim lgTxX As Integer = lgSwX + swatchSz + lgPad

                ' Tire entries
                If usePerWheel Then
                    For t As Integer = 0 To allTiresX.Count - 1
                        Dim tColor As Color = tireGaussColors(t Mod tireGaussColors.Length)
                        g.FillEllipse(New SolidBrush(Color.FromArgb(140, tColor.R, tColor.G, tColor.B)), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                        g.DrawEllipse(New Pen(tColor, 1.0F), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                        g.DrawString("Wheel " & (t + 1), legendFont, Brushes.Black, lgTxX, lgCurY + 2)
                        lgCurY += lgRowH
                    Next
                Else
                    Dim sColor As Color = tireGaussColors(0)
                    g.FillEllipse(New SolidBrush(Color.FromArgb(140, sColor.R, sColor.G, sColor.B)), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                    g.DrawEllipse(New Pen(sColor, 1.0F), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                    Dim tireLabel As String = If(allTiresX.Count > 4, "Tire patches (" & allTiresX.Count & " wheels)", "Tire contact patch")
                    g.DrawString(tireLabel, legendFont, Brushes.Black, lgTxX, lgCurY + 2)
                    lgCurY += lgRowH
                End If

                ' Wander entries
                If usePerWheel Then
                    For t As Integer = 0 To allTiresX.Count - 1
                        Dim tColor As Color = tireGaussColors(t Mod tireGaussColors.Length)
                        g.FillRectangle(New SolidBrush(Color.FromArgb(25, tColor.R, tColor.G, tColor.B)), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                        g.DrawRectangle(New Pen(Color.FromArgb(100, tColor.R, tColor.G, tColor.B), 0.8F), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                        g.DrawString("Wander — Wh." & (t + 1) & " (" & ChrW(&H03C3) & "=30.4"")", legendFont, New SolidBrush(tColor), lgTxX, lgCurY + 2)
                        lgCurY += lgRowH
                    Next
                Else
                    g.FillRectangle(New SolidBrush(Color.FromArgb(25, faaBlue.R, faaBlue.G, faaBlue.B)), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                    g.DrawRectangle(New Pen(Color.FromArgb(100, faaBlue.R, faaBlue.G, faaBlue.B), 0.8F), lgSwX, lgCurY + 1, swatchSz, swatchSz)
                    g.DrawString("Lateral wander (" & ChrW(&H03C3) & "=30.4"")", legendFont, New SolidBrush(faaBlue), lgTxX, lgCurY + 2)
                    lgCurY += lgRowH
                End If

                ' Evaluation strips entry
                Using espn As New Pen(Color.FromArgb(160, 160, 160), 1.0F)
                    espn.DashStyle = Drawing2D.DashStyle.Dash
                    g.DrawLine(espn, CSng(lgSwX), CSng(lgCurY + swatchSz \ 2 + 1), CSng(lgSwX + swatchSz), CSng(lgCurY + swatchSz \ 2 + 1))
                End Using
                g.DrawString("Eval. strips (" & Format(CDF.OFFSETINC, "0") & " " & lengthUnit & " spacing)", legendFont, Brushes.Gray, lgTxX, lgCurY + 2)
                lgCurY += lgRowH

                ' Critical strip entry
                Using cspn As New Pen(Color.FromArgb(220, 50, 50), 1.5F)
                    cspn.DashStyle = Drawing2D.DashStyle.Dash
                    g.DrawLine(cspn, CSng(lgSwX), CSng(lgCurY + swatchSz \ 2 + 1), CSng(lgSwX + swatchSz), CSng(lgCurY + swatchSz \ 2 + 1))
                End Using
                g.DrawString("Critical strip (max CDF)", legendFont, Brushes.Red, lgTxX, lgCurY + 2)
                lgCurY += lgRowH

                ' Centerline entry
                g.DrawLine(New Pen(Color.Black, 1.5F), CSng(lgSwX), CSng(lgCurY + swatchSz \ 2 + 1), CSng(lgSwX + swatchSz), CSng(lgCurY + swatchSz \ 2 + 1))
                g.DrawString("Wheel path centerline (0)", legendFont, Brushes.Black, lgTxX, lgCurY + 2)

                ' Border
                g.DrawRectangle(New Pen(Color.FromArgb(209, 213, 219), 1), 0, 0, chartWidth - 1, chartHeight - 1)

                ' Cleanup
                titleFont.Dispose()
                labelFont.Dispose()
                tickFont.Dispose()
                legendFont.Dispose()
                annotFont.Dispose()
                dimFont.Dispose()
                stripPen.Dispose()
                critStripPen.Dispose()
                dimPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws an ACR vs normalized CDF per departure scatter chart.
        ''' </summary>
        Private Function DrawACRDamageChart(rpt As clsDetailedReportData, chartColors() As Color) As Bitmap
            Dim chartWidth As Integer = 850
            Dim chartHeight As Integer = 500
            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim labelFont As New Font("Segoe UI", 9.5F)
                Dim axisFont As New Font("Segoe UI", 8.5F)
                Dim smallFont As New Font("Segoe UI", 7.5F)

                Dim acrTitle As String = "Aircraft Classification Rating (ACR) vs. Damage per Departure"
                Dim acrTitleSize = g.MeasureString(acrTitle, titleFont)
                g.DrawString(acrTitle, titleFont, Brushes.Black, CSng((chartWidth - acrTitleSize.Width) / 2), 10)

                Dim marginLeft As Integer = 90
                Dim marginRight As Integer = 30
                Dim marginTop As Integer = 50
                Dim marginBottom As Integer = 65
                Dim plotWidth As Integer = chartWidth - marginLeft - marginRight
                Dim plotHeight As Integer = chartHeight - marginTop - marginBottom

                ' Collect data points: (ACR, CDF per departure, name, color)
                Dim points As New List(Of Tuple(Of Double, Double, String, Color, Double))
                If rpt.AircraftDetails IsNot Nothing AndAlso rpt.ACRDetails IsNot Nothing Then
                    For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                        If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                        Dim det = rpt.AircraftDetails(ia)
                        Dim acr As Double = 0

                        ' Find matching ACR
                        For Each acrDet In rpt.ACRDetails
                            If acrDet.ACName = det.ACName Then
                                acr = acrDet.FinalACR
                                Exit For
                            End If
                        Next

                        Dim cdfPerDep As Double = 0
                        If det.TotalRepetitions > 0 Then cdfPerDep = det.MaxCDF / det.TotalRepetitions

                        If acr > 0 AndAlso cdfPerDep > 0 Then
                            Dim acColor As Color = chartColors((ia - 1) Mod chartColors.Length)
                            points.Add(New Tuple(Of Double, Double, String, Color, Double)(acr, cdfPerDep, det.ACName, acColor, det.AnnualDepartures))
                        End If
                    Next
                End If

                If points.Count = 0 Then
                    g.DrawString("No ACR data available. Run ACR computation first.", labelFont, Brushes.Gray, chartWidth \ 2 - 120, chartHeight \ 2)
                    titleFont.Dispose() : labelFont.Dispose() : axisFont.Dispose() : smallFont.Dispose()
                    Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
                End If

                ' Determine axis ranges
                Dim minACR As Double = points.Min(Function(p) p.Item1) * 0.8
                Dim maxACR As Double = points.Max(Function(p) p.Item1) * 1.2
                Dim minDmg As Double = points.Min(Function(p) p.Item2)
                Dim maxDmg As Double = points.Max(Function(p) p.Item2)

                ' Use log scale for damage
                Dim logMinD As Double = Math.Floor(Math.Log10(Math.Max(minDmg, 1.0E-20)))
                Dim logMaxD As Double = Math.Ceiling(Math.Log10(Math.Max(maxDmg, 1.0E-10)))
                If logMaxD <= logMinD Then logMaxD = logMinD + 2

                ' Plot area
                Dim plotRect As New Rectangle(marginLeft, marginTop, plotWidth, plotHeight)
                g.FillRectangle(New SolidBrush(Color.FromArgb(250, 250, 252)), plotRect)
                g.DrawRectangle(Pens.DarkGray, plotRect)

                ' X-axis (ACR, linear)
                Dim nTicksX As Integer = 5
                Dim gridPen As New Pen(Color.FromArgb(210, 214, 220), 1)
                gridPen.DashStyle = Drawing2D.DashStyle.Dot
                For i As Integer = 0 To nTicksX
                    Dim acrVal As Double = minACR + (maxACR - minACR) * i / nTicksX
                    Dim xPx As Single = CSng(marginLeft + CDbl(i) / nTicksX * plotWidth)
                    If i > 0 AndAlso i < nTicksX Then g.DrawLine(gridPen, xPx, marginTop, xPx, marginTop + plotHeight)
                    g.DrawString(Format(acrVal, "0.0"), axisFont, Brushes.Black, xPx - 10, marginTop + plotHeight + 4)
                Next
                g.DrawString("Aircraft Classification Rating (ACR)", labelFont, Brushes.Black, marginLeft + plotWidth \ 2 - 100, chartHeight - 20)

                ' Y-axis (damage per departure, log scale)
                For logV As Integer = CInt(logMinD) To CInt(logMaxD)
                    Dim yPx As Single = CSng(marginTop + plotHeight - (logV - logMinD) / (logMaxD - logMinD) * plotHeight)
                    If logV > logMinD AndAlso logV < logMaxD Then g.DrawLine(gridPen, marginLeft, yPx, marginLeft + plotWidth, yPx)
                    Dim tickLabel = "1E" & logV.ToString()
                    Dim tickSize = g.MeasureString(tickLabel, axisFont)
                    g.DrawString(tickLabel, axisFont, Brushes.Black, marginLeft - tickSize.Width - 4, yPx - tickSize.Height / 2)
                Next
                g.TranslateTransform(18, CSng(marginTop + plotHeight / 2))
                g.RotateTransform(-90)
                g.DrawString("CDF per Departure (damage/pass)", labelFont, Brushes.Black, -90, 0)
                g.ResetTransform()
                g.ScaleTransform(2, 2) ' Restore 2x supersampling scale

                ' Plot points
                For Each pt In points
                    Dim acr As Double = pt.Item1
                    Dim dmg As Double = pt.Item2
                    Dim acName As String = pt.Item3
                    Dim acColor As Color = pt.Item4
                    Dim annDep As Double = pt.Item5

                    Dim xPx As Single = CSng(marginLeft + (acr - minACR) / (maxACR - minACR) * plotWidth)
                    Dim logDmg As Double = Math.Log10(Math.Max(dmg, 1.0E-20))
                    Dim yPx As Single = CSng(marginTop + plotHeight - (logDmg - logMinD) / (logMaxD - logMinD) * plotHeight)

                    ' Size proportional to annual departures (scaled)
                    Dim maxDep As Double = points.Max(Function(p) p.Item5)
                    Dim radius As Integer = CInt(Math.Max(6, Math.Min(20, 6 + 14 * annDep / Math.Max(maxDep, 1))))

                    g.FillEllipse(New SolidBrush(Color.FromArgb(180, acColor.R, acColor.G, acColor.B)), xPx - radius, yPx - radius, radius * 2, radius * 2)
                    g.DrawEllipse(New Pen(acColor, 1.5F), xPx - radius, yPx - radius, radius * 2, radius * 2)

                    ' Label
                    g.DrawString(acName, smallFont, New SolidBrush(acColor), xPx + radius + 3, yPx - 6)
                Next

                ' Note about bubble size
                g.DrawString("Bubble size proportional to annual departures", smallFont, Brushes.Gray, marginLeft + 5, marginTop + plotHeight - 16)

                ' Cleanup
                titleFont.Dispose()
                labelFont.Dispose()
                axisFont.Dispose()
                smallFont.Dispose()
                gridPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws a modulus-vs-depth profile showing aggregate sublayer modulus gradient.
        ''' Left panel: stacked layer bars colored by modulus. Right panel: modulus-depth step chart.
        ''' </summary>
        Private Function DrawModulusDepthProfile(sublayerData As clsSublayerData, thicknessUnit As String, pressureUnit As String) As Bitmap
            Dim chartWidth As Integer = 850
            Dim chartHeight As Integer = 500
            Dim ssF As Integer = 2
            Dim bmpHi As New Bitmap(chartWidth * ssF, chartHeight * ssF)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.ScaleTransform(ssF, ssF)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit
                g.Clear(Color.White)

                ' Fonts
                Dim titleFont As New Font("Segoe UI", 10, FontStyle.Bold)
                Dim axisFont As New Font("Segoe UI", 8.5F)
                Dim tickFont As New Font("Segoe UI", 7.5F)
                Dim labelFont As New Font("Segoe UI", 7.0F)
                Dim smallFont As New Font("Segoe UI", 6.5F)
                Dim mathFont As New Font("Consolas", 6.5F)

                ' Colors
                Dim teal As Color = Color.FromArgb(0, 121, 107)
                Dim tealLight As Color = Color.FromArgb(224, 242, 241)
                Dim gridColor As Color = Color.FromArgb(230, 230, 230)
                Dim axisColor As Color = Color.FromArgb(80, 80, 80)

                ' Title
                g.DrawString("Aggregate Sublayer Modulus Profile", titleFont, Brushes.Black, 20, 12)

                ' Collect all sublayers for the depth profile
                Dim allLayers As New List(Of clsLayerInfo)
                For Each sl In sublayerData.ExpandedSublayers
                    allLayers.Add(sl)
                Next

                If allLayers.Count < 2 Then
                    titleFont.Dispose() : axisFont.Dispose() : tickFont.Dispose()
                    labelFont.Dispose() : smallFont.Dispose() : mathFont.Dispose()
                    Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
                End If

                ' Compute depth array and modulus range (exclude semi-infinite last layer from depth)
                Dim depths As New List(Of Single)  ' depth to top of each layer
                Dim cumDepth As Single = 0
                For i As Integer = 0 To allLayers.Count - 2  ' skip last (semi-infinite)
                    depths.Add(cumDepth)
                    cumDepth += allLayers(i).Thickness
                Next
                depths.Add(cumDepth) ' bottom of last structural layer

                Dim maxDepth As Single = cumDepth * 1.1F
                If maxDepth < 10 Then maxDepth = 10

                ' Find modulus range across all layers (including subgrade)
                Dim minMod As Single = Single.MaxValue
                Dim maxMod As Single = Single.MinValue
                For Each sl In allLayers
                    If sl.Modulus > maxMod Then maxMod = sl.Modulus
                    If sl.Modulus < minMod Then minMod = sl.Modulus
                Next
                Dim modRange As Single = maxMod - minMod
                If modRange < 1000 Then modRange = 1000

                ' ── Layout: left = layer column (200px), right = chart area ──
                Dim layerColX As Single = 60
                Dim layerColW As Single = 140
                Dim plotLeft As Single = 260
                Dim plotRight As Single = 790
                Dim plotTop As Single = 55
                Dim plotBottom As Single = 440
                Dim plotH As Single = plotBottom - plotTop
                Dim plotW As Single = plotRight - plotLeft

                ' Helper lambdas
                Dim depthToY As Func(Of Single, Single) = Function(d As Single) plotTop + (d / maxDepth) * plotH
                Dim modToX As Func(Of Single, Single) = Function(m As Single) plotLeft + ((m - minMod * 0.9F) / (maxMod * 1.1F - minMod * 0.9F)) * plotW

                ' ── Left panel: stacked layer bars ──
                Dim layerColors() As Color = {
                    Color.FromArgb(55, 71, 79),    ' dark gray (HMA)
                    Color.FromArgb(0, 121, 107),   ' teal (stabilized)
                    Color.FromArgb(121, 85, 72),   ' brown (aggregate base)
                    Color.FromArgb(161, 136, 127), ' light brown (aggregate subbase)
                    Color.FromArgb(189, 189, 189), ' light gray (subgrade)
                    Color.FromArgb(96, 125, 139)   ' blue-gray (other)
                }

                For i As Integer = 0 To allLayers.Count - 2
                    Dim y1 As Single = depthToY.Invoke(depths(i))
                    Dim y2 As Single = depthToY.Invoke(depths(i + 1))
                    Dim lh As Single = Math.Max(y2 - y1, 2)

                    ' Pick color based on layer type
                    Dim cIdx As Integer = Math.Min(i, layerColors.Length - 1)
                    ' Use brown shades for aggregate sublayers
                    Dim isAggregate As Boolean = False
                    For Each bsl In sublayerData.BaseSublayers
                        If Math.Abs(allLayers(i).Modulus - bsl.Modulus) < 1 AndAlso allLayers(i).LCode = bsl.LCode Then isAggregate = True
                    Next
                    For Each ssl In sublayerData.SubbaseSublayers
                        If Math.Abs(allLayers(i).Modulus - ssl.Modulus) < 1 AndAlso allLayers(i).LCode = ssl.LCode Then isAggregate = True
                    Next

                    Dim barColor As Color
                    If isAggregate Then
                        ' Gradient shade based on modulus within aggregate range
                        Dim t As Single = 0.5F
                        If modRange > 100 Then t = (allLayers(i).Modulus - minMod) / modRange
                        t = Math.Max(0, Math.Min(1, t))
                        barColor = Color.FromArgb(
                            CInt(161 + (0 - 161) * t * 0.6),
                            CInt(136 + (121 - 136) * t * 0.6),
                            CInt(127 + (107 - 127) * t * 0.6))
                    ElseIf i = allLayers.Count - 2 Then
                        barColor = layerColors(4) ' subgrade-like for last structural
                    ElseIf i = 0 Then
                        barColor = layerColors(0) ' HMA surface
                    Else
                        barColor = layerColors(Math.Min(cIdx, layerColors.Length - 1))
                    End If

                    Using br As New SolidBrush(Color.FromArgb(200, barColor.R, barColor.G, barColor.B))
                        g.FillRectangle(br, layerColX, y1, layerColW, lh)
                    End Using
                    g.DrawRectangle(New Pen(Color.FromArgb(120, 80, 80, 80), 0.5F), layerColX, y1, layerColW, lh)

                    ' Layer label
                    Dim layerLabel As String = Format(allLayers(i).Thickness, "0.0") & " " & thicknessUnit
                    If lh > 14 Then
                        Dim lsz = g.MeasureString(layerLabel, smallFont)
                        g.DrawString(layerLabel, smallFont, Brushes.White, layerColX + layerColW / 2 - lsz.Width / 2, y1 + lh / 2 - lsz.Height / 2)
                    End If
                Next

                ' Depth axis (left of layer column)
                g.DrawString("Depth (" & thicknessUnit & ")", axisFont, New SolidBrush(axisColor), 2, plotTop - 18)
                For Each d As Single In depths
                    Dim y As Single = depthToY.Invoke(d)
                    g.DrawLine(New Pen(gridColor, 0.5F), plotLeft, y, plotRight, y)
                    Dim dStr As String = Format(d, "0.0")
                    Dim dsz = g.MeasureString(dStr, tickFont)
                    g.DrawString(dStr, tickFont, New SolidBrush(axisColor), layerColX - dsz.Width - 4, y - dsz.Height / 2)
                Next

                ' ── Right panel: modulus step chart ──
                ' Modulus axis
                g.DrawLine(New Pen(axisColor, 1.2F), plotLeft, plotTop, plotLeft, plotBottom)
                g.DrawLine(New Pen(axisColor, 1.2F), plotLeft, plotBottom, plotRight, plotBottom)
                g.DrawString("Modulus (" & pressureUnit & ")", axisFont, New SolidBrush(axisColor), plotLeft + plotW / 2 - 50, plotBottom + 20)

                ' Modulus tick marks
                Dim modMin As Single = minMod * 0.9F
                Dim modMax As Single = maxMod * 1.1F
                Dim modStep As Single = CSng(Math.Pow(10, Math.Floor(Math.Log10(modMax - modMin))))
                If (modMax - modMin) / modStep < 3 Then modStep /= 2
                If (modMax - modMin) / modStep > 8 Then modStep *= 2
                Dim mTick As Single = CSng(Math.Ceiling(modMin / modStep) * modStep)
                While mTick <= modMax
                    Dim x As Single = modToX.Invoke(mTick)
                    g.DrawLine(New Pen(gridColor, 0.5F), x, plotTop, x, plotBottom)
                    g.DrawLine(New Pen(axisColor, 0.8F), x, plotBottom, x, plotBottom + 4)
                    Dim mStr As String = Format(mTick, "#,##0")
                    Dim msz = g.MeasureString(mStr, tickFont)
                    g.DrawString(mStr, tickFont, New SolidBrush(axisColor), x - msz.Width / 2, plotBottom + 6)
                    mTick += modStep
                End While

                ' Draw step profile (modulus at each depth)
                Dim profilePen As New Pen(teal, 2.5F)
                For i As Integer = 0 To allLayers.Count - 2
                    Dim y1 As Single = depthToY.Invoke(depths(i))
                    Dim y2 As Single = depthToY.Invoke(depths(i + 1))
                    Dim x As Single = modToX.Invoke(allLayers(i).Modulus)

                    ' Horizontal line at this modulus across the layer depth
                    g.DrawLine(profilePen, x, y1, x, y2)

                    ' Vertical step between layers
                    If i < allLayers.Count - 2 Then
                        Dim xNext As Single = modToX.Invoke(allLayers(i + 1).Modulus)
                        g.DrawLine(profilePen, x, y2, xNext, y2)
                    End If

                    ' Modulus value label
                    Dim modLabel As String = Format(allLayers(i).Modulus, "#,##0")
                    Dim mlsz = g.MeasureString(modLabel, smallFont)
                    Dim labelX As Single = x + 4
                    If labelX + mlsz.Width > plotRight - 5 Then labelX = x - mlsz.Width - 4
                    g.DrawString(modLabel, smallFont, New SolidBrush(teal), labelX, (y1 + y2) / 2 - mlsz.Height / 2)
                Next

                ' Highlight aggregate sublayers with filled area
                For i As Integer = 0 To allLayers.Count - 2
                    Dim isAgg As Boolean = False
                    For Each bsl In sublayerData.BaseSublayers
                        If Math.Abs(allLayers(i).Modulus - bsl.Modulus) < 1 AndAlso allLayers(i).LCode = bsl.LCode Then isAgg = True
                    Next
                    For Each ssl In sublayerData.SubbaseSublayers
                        If Math.Abs(allLayers(i).Modulus - ssl.Modulus) < 1 AndAlso allLayers(i).LCode = ssl.LCode Then isAgg = True
                    Next
                    If isAgg Then
                        Dim y1 As Single = depthToY.Invoke(depths(i))
                        Dim y2 As Single = depthToY.Invoke(depths(i + 1))
                        Dim x As Single = modToX.Invoke(allLayers(i).Modulus)
                        Using br As New SolidBrush(Color.FromArgb(40, teal.R, teal.G, teal.B))
                            g.FillRectangle(br, plotLeft, y1, x - plotLeft, y2 - y1)
                        End Using
                    End If
                Next

                ' Draw connecting line from layer column to chart
                Dim connPen As New Pen(Color.FromArgb(100, 180, 180, 180), 0.6F)
                connPen.DashStyle = Drawing2D.DashStyle.Dot
                For Each d As Single In depths
                    Dim y As Single = depthToY.Invoke(d)
                    g.DrawLine(connPen, layerColX + layerColW, y, plotLeft, y)
                Next
                connPen.Dispose()

                profilePen.Dispose()
                titleFont.Dispose()
                axisFont.Dispose()
                tickFont.Dispose()
                labelFont.Dispose()
                smallFont.Dispose()
                mathFont.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Scales down a high-resolution bitmap to the target size using bicubic interpolation.
        ''' Used for 2x supersampled chart rendering.
        ''' </summary>
        Private Function SupersampleBitmap(bmpHi As Bitmap, targetWidth As Integer, targetHeight As Integer) As Bitmap
            Dim bmp As New Bitmap(targetWidth, targetHeight)
            Using g As Graphics = Graphics.FromImage(bmp)
                g.InterpolationMode = Drawing2D.InterpolationMode.HighQualityBicubic
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.DrawImage(bmpHi, 0, 0, targetWidth, targetHeight)
            End Using
            bmpHi.Dispose()
            Return bmp
        End Function


        ''' <summary>
        ''' Draws a horizontal bar chart showing each aircraft's CDF contribution at the critical offset.
        ''' </summary>
        Private Function DrawCDFContributionChart(rpt As clsDetailedReportData, chartColors() As Color) As Bitmap
            Dim nAC As Integer = rpt.CDFSweep.NAircraftCaptured
            If nAC <= 0 Then Return New Bitmap(1, 1)

            Dim barHeight As Integer = 32
            Dim spacing As Integer = 8
            Dim chartWidth As Integer = 800
            Dim chartHeight As Integer = 70 + (nAC + 1) * (barHeight + spacing) + 20
            Dim marginLeft As Integer = 180
            Dim marginRight As Integer = 90
            Dim marginTop As Integer = 50
            Dim plotWidth As Integer = chartWidth - marginLeft - marginRight

            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                ' Title
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleText = "CDF Contribution by Aircraft at Critical Offset"
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 12)

                ' Border
                g.DrawRectangle(New Pen(Color.FromArgb(209, 213, 219), 1), 0, 0, chartWidth - 1, chartHeight - 1)

                ' Get CDF values at critical offset
                Dim critOffset = rpt.CDFSweep.MaxCDFOffset
                Dim totalCDF As Double = rpt.CDFSweep.CDFTotalPerOffset(critOffset)

                Dim labelFont As New Font("Segoe UI", 9.5F)
                Dim valueFont As New Font("Segoe UI", 8.5F)
                Dim pctFont As New Font("Segoe UI", 9, FontStyle.Bold)

                Dim yPos As Integer = marginTop
                For ia As Integer = 1 To nAC
                    Dim acName As String = "AC" & ia.ToString()
                    If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                        acName = rpt.AircraftDetails(ia).ACName
                    End If

                    Dim acCDF As Double = rpt.CDFSweep.CDFPerAircraftPerOffset(ia, critOffset)
                    Dim pct As Double = If(totalCDF > 0, acCDF / totalCDF * 100, 0)
                    Dim barW As Single = CSng(If(totalCDF > 0, acCDF / totalCDF * plotWidth, 0))
                    Dim acColor As Color = chartColors((ia - 1) Mod chartColors.Length)

                    ' Aircraft name (right-aligned)
                    Dim nameSize = g.MeasureString(acName, labelFont)
                    g.DrawString(acName, labelFont, Brushes.Black, marginLeft - nameSize.Width - 8, yPos + (barHeight - nameSize.Height) / 2)

                    ' Bar with rounded corners effect
                    If barW > 2 Then
                        Dim barRect As New RectangleF(marginLeft, yPos, barW, barHeight)
                        g.FillRectangle(New SolidBrush(Color.FromArgb(210, acColor.R, acColor.G, acColor.B)), barRect)
                        g.DrawRectangle(New Pen(acColor, 1), marginLeft, yPos, barW, barHeight)

                        ' Percentage inside bar if wide enough
                        Dim valText = Format(pct, "0.0") & "%"
                        Dim valSize = g.MeasureString(valText, pctFont)
                        If barW > valSize.Width + 10 Then
                            g.DrawString(valText, pctFont, Brushes.White, marginLeft + barW - valSize.Width - 6, yPos + (barHeight - valSize.Height) / 2)
                        Else
                            g.DrawString(valText, pctFont, New SolidBrush(acColor), marginLeft + barW + 5, yPos + (barHeight - valSize.Height) / 2)
                        End If
                    ElseIf barW > 0 Then
                        g.FillRectangle(New SolidBrush(acColor), marginLeft, yPos, Math.Max(barW, 2), barHeight)
                        g.DrawString(Format(pct, "0.0") & "%", pctFont, New SolidBrush(acColor), marginLeft + 6, yPos + (barHeight - 14) / 2)
                    End If

                    ' CDF value annotation
                    g.DrawString(Format(acCDF, "0.000000"), valueFont, Brushes.Gray, marginLeft + plotWidth + 8, yPos + (barHeight - 12) / 2)

                    yPos += barHeight + spacing
                Next

                ' Separator line
                g.DrawLine(New Pen(Color.FromArgb(180, 180, 180), 1), marginLeft, yPos - 2, marginLeft + plotWidth, yPos - 2)

                ' Total bar
                Dim totalLabel = "TOTAL"
                Dim totalLabelSize = g.MeasureString(totalLabel, New Font("Segoe UI", 9.5F, FontStyle.Bold))
                g.DrawString(totalLabel, New Font("Segoe UI", 9.5F, FontStyle.Bold), Brushes.Black, marginLeft - totalLabelSize.Width - 8, yPos + (barHeight - totalLabelSize.Height) / 2 + 2)
                Dim totalBarBrush As New SolidBrush(Color.FromArgb(80, 46, 94, 168))
                g.FillRectangle(totalBarBrush, marginLeft, yPos + 2, plotWidth, barHeight)
                g.DrawRectangle(New Pen(Color.FromArgb(46, 94, 168), 1.5F), marginLeft, yPos + 2, plotWidth, barHeight)
                g.DrawString("100%", pctFont, Brushes.White, marginLeft + plotWidth - 42, yPos + (barHeight - 12) / 2 + 2)
                g.DrawString(Format(totalCDF, "0.000000"), valueFont, Brushes.Gray, marginLeft + plotWidth + 8, yPos + 6)
                totalBarBrush.Dispose()

                ' Cleanup
                titleFont.Dispose()
                labelFont.Dispose()
                valueFont.Dispose()
                pctFont.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        ''' <summary>
        ''' Draws a horizontal bar chart showing the fatigue life ratio (N_fail / Repetitions) for each aircraft.
        ''' Bars are green when ratio > 1 (life reserve) and red when ratio &lt; 1 (overstressed).
        ''' </summary>
        Private Function DrawLifeRatioChart(rpt As clsDetailedReportData, chartColors() As Color) As Bitmap
            If rpt.AircraftDetails Is Nothing Then Return New Bitmap(1, 1)

            ' Count valid aircraft
            Dim acList As New List(Of Tuple(Of String, Double, Double, Color))
            Dim acIdx As Integer = 0
            For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                Dim det = rpt.AircraftDetails(ia)
                Dim ratio As Double = 0
                If det.TotalRepetitions > 0 AndAlso det.NtoFail > 0 Then
                    ratio = det.NtoFail / det.TotalRepetitions
                End If
                Dim acColor As Color = chartColors(acIdx Mod chartColors.Length)
                acList.Add(New Tuple(Of String, Double, Double, Color)(det.ACName, ratio, det.MaxCDF, acColor))
                acIdx += 1
            Next
            If acList.Count = 0 Then Return New Bitmap(1, 1)

            Dim barHeight As Integer = 32
            Dim spacing As Integer = 8
            Dim chartWidth As Integer = 800
            Dim chartHeight As Integer = 70 + acList.Count * (barHeight + spacing) + 30
            Dim marginLeft As Integer = 180
            Dim marginRight As Integer = 100
            Dim marginTop As Integer = 50
            Dim plotWidth As Integer = chartWidth - marginLeft - marginRight

            ' Use log scale for bar width
            Dim maxLogRatio As Double = 0
            For Each ac In acList
                If ac.Item2 > 0 Then
                    Dim absLog As Double = Math.Abs(Math.Log10(ac.Item2))
                    If absLog > maxLogRatio Then maxLogRatio = absLog
                End If
            Next
            If maxLogRatio < 1 Then maxLogRatio = 1

            ' Center the zero line (ratio=1.0) in the plot
            Dim centerX As Integer = marginLeft + plotWidth \ 2

            Dim bmpHi As New Bitmap(chartWidth * 2, chartHeight * 2)

            Using g As Graphics = Graphics.FromImage(bmpHi)
                g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
                g.TextRenderingHint = Text.TextRenderingHint.ClearTypeGridFit
                g.CompositingQuality = Drawing2D.CompositingQuality.HighQuality
                g.ScaleTransform(2, 2)
                g.Clear(Color.White)

                ' Title
                Dim titleFont As New Font("Segoe UI", 11, FontStyle.Bold)
                Dim titleText = "Fatigue Life Reserve (N_fail / Repetitions)"
                Dim titleSize = g.MeasureString(titleText, titleFont)
                g.DrawString(titleText, titleFont, Brushes.Black, CSng((chartWidth - titleSize.Width) / 2), 8)

                ' Border
                g.DrawRectangle(New Pen(Color.FromArgb(209, 213, 219), 1), 0, 0, chartWidth - 1, chartHeight - 1)

                Dim labelFont As New Font("Segoe UI", 9.5F)
                Dim valueFont As New Font("Segoe UI", 9.0F)

                ' Reference line at ratio = 1.0 (center)
                Dim refPen As New Pen(Color.FromArgb(100, 0, 0, 0), 1.5F)
                refPen.DashStyle = Drawing2D.DashStyle.Dash
                g.DrawLine(refPen, centerX, marginTop - 5, centerX, chartHeight - 20)

                ' "Ratio = 1.0" label — draw centered above the reference line, above the region labels
                Dim ratioLabelFont As New Font("Segoe UI", 8.5F, FontStyle.Italic)
                Dim ratioLabelText = "Ratio = 1.0"
                Dim ratioLabelSize = g.MeasureString(ratioLabelText, ratioLabelFont)
                g.DrawString(ratioLabelText, ratioLabelFont, Brushes.DarkGray, centerX - ratioLabelSize.Width / 2, marginTop - 30)
                ratioLabelFont.Dispose()

                ' Label regions — positioned just above the bars, spaced away from center label
                Dim regionFont As New Font("Segoe UI", 9.0F)
                Dim overstressedText = ChrW(&H2190) & " Overstressed"
                g.DrawString(overstressedText, regionFont, New SolidBrush(Color.FromArgb(200, 214, 39, 40)), marginLeft + 5, marginTop - 16)
                Dim reserveText = "Life Reserve " & ChrW(&H2192)
                Dim reserveSize = g.MeasureString(reserveText, regionFont)
                g.DrawString(reserveText, regionFont, New SolidBrush(Color.FromArgb(200, 34, 139, 34)), marginLeft + plotWidth - reserveSize.Width - 5, marginTop - 16)

                Dim yPos As Integer = marginTop
                For Each ac In acList
                    Dim acName = ac.Item1
                    Dim ratio = ac.Item2

                    ' Aircraft name (right-aligned)
                    Dim nameSize = g.MeasureString(acName, labelFont)
                    g.DrawString(acName, labelFont, Brushes.Black, marginLeft - nameSize.Width - 8, yPos + (barHeight - nameSize.Height) / 2)

                    If ratio > 0 Then
                        Dim logRatio As Double = Math.Log10(ratio)
                        Dim barW As Single = CSng(Math.Abs(logRatio) / maxLogRatio * (plotWidth / 2 - 10))
                        barW = Math.Min(barW, plotWidth / 2 - 5)

                        Dim barColor As Color
                        Dim barLeft As Single
                        If ratio >= 1 Then
                            ' Green bar extends right from center
                            barColor = Color.FromArgb(200, 34, 139, 34)
                            barLeft = centerX
                        Else
                            ' Red bar extends left from center
                            barColor = Color.FromArgb(200, 214, 39, 40)
                            barLeft = centerX - barW
                        End If

                        If barW > 1 Then
                            g.FillRectangle(New SolidBrush(barColor), barLeft, yPos, barW, barHeight)
                            g.DrawRectangle(New Pen(Color.FromArgb(barColor.R, barColor.G, barColor.B), 1), barLeft, yPos, barW, barHeight)
                        End If

                        ' Value annotation
                        Dim ratioText As String
                        If ratio >= 100 Then
                            ratioText = Format(ratio, "0.0E+00")
                        ElseIf ratio >= 1 Then
                            ratioText = Format(ratio, "#,##0.0")
                        Else
                            ratioText = Format(ratio, "0.000")
                        End If
                        Dim annotX As Single = If(ratio >= 1, centerX + barW + 4, barLeft - g.MeasureString(ratioText, valueFont).Width - 4)
                        g.DrawString(ratioText, valueFont, Brushes.Black, annotX, yPos + (barHeight - 12) / 2)
                    Else
                        g.DrawString("N/A", valueFont, Brushes.Gray, centerX + 5, yPos + (barHeight - 12) / 2)
                    End If

                    yPos += barHeight + spacing
                Next

                ' Cleanup
                titleFont.Dispose()
                labelFont.Dispose()
                valueFont.Dispose()
                regionFont.Dispose()
                refPen.Dispose()
            End Using

            Return SupersampleBitmap(bmpHi, chartWidth, chartHeight)
        End Function


        Public Function PCRReportPage() As String
            Return Nothing
        End Function


        Public Function refreshPCRReport() As String

            Dim Section As ISection
            Dim jobName As String, sectionName As String
            jobName = CurrentJob.Name
            Section = CurrentSectionView.Section

            Dim theJob = CurrentJob
            Dim Thicknessunit As String
            Dim PressureUnit As String
            Dim WeightUnit As String

            jobName = CurrentJob.Name

            Dim PCRMAXFinder As Integer = 0

            'If copy a calculated section having PCR run, the report will also be sopied and showed
            If Section.LastRun = Nothing Then 'Section.LastRun = Nothing when starting the software or when copying a section or when changing the structure/airplane information
                If Section.SavedPCRhtml Is Nothing Then
                    Return "The PCR calculation must be run before opening the report."
                Else
                    Return Section.SavedPCRhtml
                End If
            End If

            'If there was PCR Run before Section.LastRun, the report will be showed
            If (Section.LastRun = "Thickness Design" Or Section.LastRun = "Life Analysis" Or Section.LastRun = "Life/Compaction Analysis") Then
                If Section.SavedPCRhtml IsNot Nothing Then

                    Return Section.SavedPCRhtml
                Else
                    Return "The PCR calculation must be run before opening the report."
                End If
            End If

            Dim LenghtUnit As String
            Dim GLUnitChangeMultiplier As Double
            Dim LenghtUnitChangeMultiplier As Double
            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                Thicknessunit = "in."
                PressureUnit = "psi"
                WeightUnit = "lbs"
                LenghtUnit = "in."
                GLUnitChangeMultiplier = 1
                LenghtUnitChangeMultiplier = 1

            Else
                Thicknessunit = "mm"
                PressureUnit = "MPa"
                WeightUnit = "kg"
                LenghtUnit = "mm"
                GLUnitChangeMultiplier = 0.45359239506990429
                LenghtUnitChangeMultiplier = 25.4
            End If

            sectionName = Section.Name

            Dim src = FileIO.SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"

            Dim reportTitle = "Federal Aviation Administration FAARFIELD 2.1 PCR Report"
            Dim html As String = ""
            Dim tmpdiv As String = ""
            Dim tmpnbdiv As String = ""
            Dim tmptable As String = ""
            Dim tmptd As String = ""
            Dim tmptr As String = ""
            Dim tmpthead As String = ""
            Dim tmpth As String = ""
            Dim img As String = ""

            ' Report Title Div
            tmpdiv = hu.wrap_h2(reportTitle)
            tmpdiv += hu.wrap_p(MainWindowTitle)
            html += hu.wrap_div(tmpdiv, "report")

            ' Job Title Div
            tmpdiv = hu.wrap_h3("Job Name: " + jobName)
            tmpdiv += hu.wrap_h4("Structure: " + sectionName)

            FileName = "PCR Results for " + Section.AnalysisType.ToString + "  " + Section.SectionPCRRunTime

            tmpdiv += hu.wrap_p("This file name = " + FileName)

            If Section.DesignType = NewFlex Or DesignType = FlexOnFlex Then
                tmpdiv += hu.wrap_p("Evaluation pavement type is flexible and design program is FAARFIELD.")
            ElseIf Section.DesignType = NewRigid Or DesignType = FlexOnRigid Or DesignType = PartBondOnRigid Then
                tmpdiv += hu.wrap_p("Evaluation pavement type is rigid and design program is FAARFIELD.")
            End If

            tmpdiv += hu.wrap_p("Structure name: " + Section.Name + " in job file: " + jobName + ".JOB.xml")

            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                tmpdiv += hu.wrap_p("Units = US Customary")
            Else
                tmpdiv += hu.wrap_p("Units = Metric")
            End If

            If Not Section.AnalysisType Is Nothing Then
                tmpdiv += hu.wrap_p("Analysis Type: " + Section.AnalysisType.Name)
                If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                    tmpdiv += hu.wrap_p("Subgrade  Modulus =" + Format(Section.Layers.Last.Modulus.GetValue(theJob.DesignOptions.MeasurementSystem), "N0").ToString + PressureUnit + " (Subgrade Category is " + Section.SectionSubgradeCategory + ")")

                Else
                    tmpdiv += hu.wrap_p("Subgrade  Modulus =" + Format(Section.Layers.Last.Modulus.GetValue(theJob.DesignOptions.MeasurementSystem), "N2").ToString + PressureUnit + " (Subgrade Category is " + Section.SectionSubgradeCategory + ")")

                End If

                Dim TotalThickness As Single = 0
                For i = 0 To Section.Layers.Count - 2
                    TotalThickness = TotalThickness + Section.Layers.Item(i).Thickness.GetValue(theJob.DesignOptions.MeasurementSystem)
                Next
                If Thicknessunit = "in." Then
                    tmpdiv += hu.wrap_p("Evaluation Pavement Thickness =  " + Format(TotalThickness, "0.0").ToString + " " + Thicknessunit)
                Else
                    tmpdiv += hu.wrap_p("Evaluation Pavement Thickness =  " + Format(TotalThickness, "0").ToString + " " + Thicknessunit)
                End If


                If PtoTC < 1.0# Or 3.0# < PtoTC Or (PtoTC - Math.Floor(PtoTC)) <> 0 Then
                    tmpdiv += hu.wrap_p("Pass to Traffic Cycle (PtoTC) Ratio =  " + Format(PtoTC, "0.00") + " (Non-Standard)")
                Else
                    tmpdiv += hu.wrap_p("Pass to Traffic Cycle (PtoTC) Ratio =  " + Format(PtoTC, "0.00"))
                End If

                Dim MaxNoWheels As Integer = 0

                For i = 0 To Section.Airplanes.Count - 1
                    If Section.Airplanes.Item(i).NumberWheels > MaxNoWheels Then
                        MaxNoWheels = Section.Airplanes.Item(i).NumberWheels
                    End If

                Next
                tmpdiv += hu.wrap_p("Maximum number of wheels per gear = " + MaxNoWheels.ToString)

                tmpdiv += hu.wrap_p("CDF = " + Format(Section.SectionPCRCDF, "0.000").ToString)

                If DesignType = NewFlex Then
                    If MaxNoWheels >= 4 Then
                        tmpdiv += hu.wrap_p("At least one aircraft has 4 or more wheels per gear.")
                    Else
                        tmpdiv += hu.wrap_p("No aircraft have 4 or more wheels per gear.")
                    End If
                End If

                html += hu.wrap_div(tmpdiv, "job")

                ' Table Title Div
                tmpdiv = hu.wrap_h3("Results Table 1. Input Traffic Data")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Aircraft Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Gross Weight<br />" + "(" + WeightUnit + ")")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Percent Gross Weight")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Tire Pressure<br />" + "(" + PressureUnit + ")")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Annual Departure")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("20 Years Coverage")
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                Dim rowNum As Integer = 1
                Dim isbelly As Boolean = False
                Dim Counter As Integer = 1
                For Each airplane In Section.Airplanes
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.Name)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(airplane.GrossWeight.GetValue(theJob.DesignOptions.MeasurementSystem), "N0").ToString)
                    tmptr += hu.wrap_td(tmptd)

                    If airplane.Gear = "HB" Or airplane.Gear = "A" Then
                        tmptd = hu.wrap_p(Format(airplane.MgPercentPCN * 100, "0.00").ToString)
                    ElseIf airplane.Gear = "X" Then
                        tmptd = hu.wrap_p(Format(airplane.MgPercentPCN * 100, "0.00").ToString)
                    Else
                        tmptd = hu.wrap_p(Format(airplane.MgPercentPCN * 100, "0.00").ToString)
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                        tmptd = hu.wrap_p(Format(airplane.Cp.GetValue(theJob.DesignOptions.MeasurementSystem), "N0").ToString)
                    Else
                        tmptd = hu.wrap_p(Format(airplane.Cp.GetValue(theJob.DesignOptions.MeasurementSystem), "N2").ToString)
                    End If

                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.NumberDepartures.ToString("N0"))
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(airplane.ACRCoverage, "N0").ToString)
                    tmptr += hu.wrap_td(tmptd)

                    Counter += 1
                    rowNum += 1

                    tmptable += hu.wrap_tr(tmptr)

                Next

                ' Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                html += hu.wrap_div(tmpnbdiv, "no-break")

                ' Table Title Div
                tmpdiv = hu.wrap_h3("Results Table 2. PCR Value")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Aircraft Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Critical aircraft Total equiv. departures")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Max allowable Gross Weight of critical aircraft" + " (" + WeightUnit + ")")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("ACR Thick at max. MGW" + " (" + Thicknessunit + ")")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("PCR" + "/" + Section.SectionPCRPaveType + "/" + Section.SectionSubgradeCategory)
                tmptr += hu.wrap_th(tmpth)

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                rowNum = 1
                For Each airplane In Section.Airplanes
                    Dim PCRThickMax As Double
                    PCRThickMax = 0
                    If airplane.PCRThick < PCRThickMax Then
                        PCRMAXFinder = PCRMAXFinder + 1

                        PCRThickMax = airplane.PCRThick
                    End If
                Next

                tmptd = hu.wrap_p(rowNum.ToString())
                tmptr = hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(Section.SectionPCRCriticalAirplaneName)
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(Section.SectionPCRCriticalAnnualDeparture.ToString("N0"))
                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p((Section.SectionPCRNewGL * GLUnitChangeMultiplier).ToString("N0"))
                tmptr += hu.wrap_td(tmptd)

                If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                    tmptd = hu.wrap_p(Format(Section.SectionPCRNewPCNThick * LenghtUnitChangeMultiplier, "N1").ToString)
                Else
                    tmptd = hu.wrap_p(Format(Section.SectionPCRNewPCNThick * LenghtUnitChangeMultiplier, "N0").ToString)
                End If

                tmptr += hu.wrap_td(tmptd)
                tmptd = hu.wrap_p(Format(Section.SectionPCRNewPCN, "0.0").ToString)
                tmptr += hu.wrap_td(tmptd)

                tmptable += hu.wrap_tr(tmptr)

                ' Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                html += hu.wrap_div(tmpnbdiv, "no-break")

                ' Table Title Div
                tmpdiv = hu.wrap_h3("Results Table 3. " + Section.AnalysisType.Name + " ACR at Indicated Gross Weight and Strength")
                tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                ' New Table
                ' Table Row
                tmpth = hu.wrap_p("No.")
                tmptr = hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Aircraft Name")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Gross Weight<br />" + "(" + WeightUnit + ")")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Percent Gross Weight on Main Gear")
                tmptr += hu.wrap_th(tmpth)
                tmpth = hu.wrap_p("Tire Pressure<br />" + "(" + PressureUnit + ")")
                tmptr += hu.wrap_th(tmpth)

                If theJob.DesignOptions.ACROptions = False Then

                    tmpth = hu.wrap_p("ACR Thick (" + Thicknessunit + ") (" + Section.SectionSubgradeCategory + ")")
                    tmptr += hu.wrap_th(tmpth)

                    tmpth = hu.wrap_p("ACR" + "/" + Section.SectionPCRPaveType + "/" + Section.SectionSubgradeCategory)
                    tmptr += hu.wrap_th(tmpth)

                End If

                tmpthead = hu.wrap_tr(tmptr)
                tmptable = hu.wrap_thead(tmpthead)

                rowNum = 1
                For I = 0 To Section.Airplanes.Count - 1
                    If Not Right(Section.Airplanes.Item(I).Name, 5) = "Belly" Then
                        tmptd = hu.wrap_p(rowNum.ToString())
                        tmptr = hu.wrap_td(tmptd)

                        tmptd = hu.wrap_p(Section.Airplanes.Item(I).Name)
                        tmptr += hu.wrap_td(tmptd)

                        tmptd = hu.wrap_p(Format((Section.Airplanes.Item(I).GrossWeight.GetValue(theJob.DesignOptions.MeasurementSystem)), "N0").ToString)
                        tmptr += hu.wrap_td(tmptd)

                        If Section.Airplanes.Item(I).Gear = "X" Then
                            tmptd = hu.wrap_p(((Section.Airplanes.Item(I).MgPercentPCN * 100)).ToString)
                        Else
                            tmptd = hu.wrap_p(Format(Section.Airplanes.Item(I).MgPercentPCN * 100, "0.00").ToString)
                        End If
                        Try
                            If I < Section.Airplanes.Count - 1 AndAlso Section.Airplanes.Item(I + 1).Name.Contains("Belly") Then
                                tmptd = hu.wrap_p(((Section.Airplanes.Item(I).MgPercentPCN) * 100 + (Section.Airplanes.Item(I + 1).MgPercentPCN) * 100).ToString)
                            End If
                        Catch ex As Exception

                        End Try

                        tmptr += hu.wrap_td(tmptd)

                        If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then

                            tmptd = hu.wrap_p(Format(Section.Airplanes.Item(I).Cp.GetValue(theJob.DesignOptions.MeasurementSystem), "N0").ToString)
                        Else
                            tmptd = hu.wrap_p(Format(Section.Airplanes.Item(I).Cp.GetValue(theJob.DesignOptions.MeasurementSystem), "N2").ToString)
                        End If
                        tmptr += hu.wrap_td(tmptd)

                        If theJob.DesignOptions.ACROptions = False Then

                            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                                tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRThick.GetValue(theJob.DesignOptions.MeasurementSystem).ToString("N1"))
                            Else
                                tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRThick.GetValue(theJob.DesignOptions.MeasurementSystem).ToString("N0"))
                            End If
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRB.ToString)
                            tmptr += hu.wrap_td(tmptd)
                        End If

                        tmptable += hu.wrap_tr(tmptr)
                        rowNum += 1
                    End If

                Next

                ' Table Div
                tmpdiv = hu.wrap_table(tmptable)
                tmpnbdiv += hu.wrap_div(tmpdiv)
                html += hu.wrap_div(tmpnbdiv, "no-break")

                If theJob.DesignOptions.ACROptions = True Then
                    tmpdiv = hu.wrap_h3("Results Table 3. Continue")
                    tmpnbdiv = hu.wrap_div(tmpdiv, "title")

                    ' New Table
                    ' Table Row
                    tmpth = hu.wrap_p("No.")
                    tmptr = hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p("Aircraft Name")
                    tmptr += hu.wrap_th(tmpth)

                    tmpth = hu.wrap_p(ACRThickHeader1)
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p(ACRThickHeader2)
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p(ACRThickHeader3)
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p(ACRThickHeader4)
                    tmptr += hu.wrap_th(tmpth)

                    tmpth = hu.wrap_p(ACRHeader1)
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p(ACRHeader2)
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p(ACRHeader3)
                    tmptr += hu.wrap_th(tmpth)
                    tmpth = hu.wrap_p(ACRHeader4)
                    tmptr += hu.wrap_th(tmpth)
                    tmpthead = hu.wrap_tr(tmptr)
                    tmptable = hu.wrap_thead(tmpthead)

                    rowNum = 1
                    For I = 0 To Section.Airplanes.Count - 1
                        If Not Right(Section.Airplanes.Item(I).Name, 5) = "Belly" Then
                            tmptd = hu.wrap_p(rowNum.ToString())
                            tmptr = hu.wrap_td(tmptd)

                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).Name)
                            tmptr += hu.wrap_td(tmptd)

                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRThick1.GetValue(theJob.DesignOptions.MeasurementSystem).ToString())
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRThick2.GetValue(theJob.DesignOptions.MeasurementSystem).ToString())
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRThick3.GetValue(theJob.DesignOptions.MeasurementSystem).ToString())
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRThick4.GetValue(theJob.DesignOptions.MeasurementSystem).ToString())
                            tmptr += hu.wrap_td(tmptd)

                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRB4.ToString)
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRB3.ToString)
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRB2.ToString)
                            tmptr += hu.wrap_td(tmptd)
                            tmptd = hu.wrap_p(Section.Airplanes.Item(I).ACRB1.ToString)
                            tmptr += hu.wrap_td(tmptd)

                            tmptable += hu.wrap_tr(tmptr)

                        End If
                        rowNum += 1
                    Next

                    ' Table Div
                    tmpdiv = hu.wrap_table(tmptable)
                    tmpnbdiv += hu.wrap_div(tmpdiv)
                    html += hu.wrap_div(tmpnbdiv, "no-break")
                End If

            End If

            ' Complete HTML Page
            html = hu.CreateHtmlPage(html, reportTitle)
            Section.SavedPCRhtml = html
            Return html

        End Function


        Public Function refreshAirportMasterRecord() As String

            Dim Section As ISection
            Dim jobName As String, sectionName As String
            Section = CurrentSectionView.Section
            jobName = CurrentJob.Name

            Dim theJob = CurrentJob
            Dim Thicknessunit As String
            Dim PressureUnit As String
            Dim WeightUnit As String

            'If copy a calculated section having PCR run, the report will also be sopied and showed
            If Section.LastRun = Nothing Then 'Section.LastRun = Nothing when starting the software or when copying a section
                If Section.SavedAirportMasterRecordhtml Is Nothing Then
                    Return "The PCR calculation must be run before opening the report."
                Else
                    Return Section.SavedAirportMasterRecordhtml
                End If
            End If

            'If there was PCR Run before Section.LastRun, the report will be showed
            If (Section.LastRun = "Thickness Design" Or Section.LastRun = "Life Analysis" Or Section.LastRun = "Life/Compaction Analysis") Then
                If Section.SavedAirportMasterRecordhtml IsNot Nothing Then

                    Return Section.SavedAirportMasterRecordhtml
                Else
                    Return "The PCR calculation must be run before opening the report."
                End If
            End If

            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                Thicknessunit = "in."
                PressureUnit = "psi"
                WeightUnit = "lbs"
            Else
                Thicknessunit = "mm"
                PressureUnit = "MPa"
                WeightUnit = "kg"
            End If

            sectionName = Section.Name

            Dim src = FileIO.SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"

            Dim reportTitle = "Federal Aviation Administration FAARFIELD 2.1 Airport Master Record"
            Dim html As String = ""
            Dim tmpdiv As String = ""
            Dim tmptable As String = ""
            Dim tmptd As String = ""
            Dim tmptr As String = ""

            ' Report Title Div
            tmpdiv = hu.wrap_h2(reportTitle)
            tmpdiv += hu.wrap_p(MainWindowTitle)
            html += hu.wrap_div(tmpdiv, "report")

            ' Job Title Div
            tmpdiv = hu.wrap_h3("RUNWAY DATA")
            tmpdiv += hu.wrap_h4("Job Name: " + jobName)
            tmpdiv += hu.wrap_p("Structure: " + sectionName)
            html += hu.wrap_div(tmpdiv, "job")

            ' Table Title Div
            tmpdiv = hu.wrap_h3("Gross Weight (In THSDS)")
            html += hu.wrap_div(tmpdiv, "title")

            ' New Table
            ' Table Row
            tmptd = hu.wrap_p("35 S")
            tmptr = hu.wrap_td(tmptd)

            tmptd = hu.wrap_p(Format(Section.SectionPCRAirportMasterRecordS, "N0"))
            tmptr += hu.wrap_td(tmptd)
            tmptable += hu.wrap_tr(tmptr)

            ' Table Row
            tmptd = hu.wrap_p("36 D")
            tmptr = hu.wrap_td(tmptd)

            tmptd = hu.wrap_p(Format(Section.SectionPCRAirportMasterRecordD, "N0"))
            tmptr += hu.wrap_td(tmptd)
            tmptable += hu.wrap_tr(tmptr)

            ' Table Row
            tmptd = hu.wrap_p("37 2D")
            tmptr = hu.wrap_td(tmptd)

            tmptd = hu.wrap_p(Format(Section.SectionPCRAirportMasterRecord2D, "N0"))
            tmptr += hu.wrap_td(tmptd)
            tmptable += hu.wrap_tr(tmptr)

            ' Table Row
            tmptd = hu.wrap_p("38 2D/2D2")
            tmptr = hu.wrap_td(tmptd)

            tmptd = hu.wrap_p(Format(Section.SectionPCRAirportMasterRecord2D2, "N0"))
            tmptr += hu.wrap_td(tmptd)
            tmptable += hu.wrap_tr(tmptr)

            ' Table Div
            tmpdiv = hu.wrap_table(tmptable)
            html += hu.wrap_div(tmpdiv, "AirportMasterRecord")

            ' Table Title Div
            tmpdiv = hu.wrap_h3(" ")
            html += hu.wrap_div(tmpdiv, "title")

            ' New Table
            ' Table Row
            tmptd = hu.wrap_p("39 PCR")
            tmptr = hu.wrap_td(tmptd)

            tmptd = hu.wrap_p(Section.SectionPCRAirportMasterRecordFullPCR)
            tmptr += hu.wrap_td(tmptd)
            tmptable = hu.wrap_tr(tmptr)

            ' Table Div
            tmpdiv = hu.wrap_table(tmptable)
            html += hu.wrap_div(tmpdiv, "AirportMasterRecord")

            ' Complete HTML Page
            html = hu.CreateHtmlPage(html, reportTitle)
            Section.SavedAirportMasterRecordhtml = html
            Return html

        End Function


        Public Function RefreshPCRGraph() As String

            Dim Section As ISection
            Dim jobName As String, sectionName As String
            Section = CurrentSectionView.Section
            jobName = CurrentJob.Name

            Dim theJob = CurrentJob
            Dim Thicknessunit As String
            Dim PressureUnit As String
            Dim WeightUnit As String

            'If copy a calculated section having PCR run, the graph will also be sopied and showed
            If Section.LastRun = Nothing Then 'Section.LastRun = Nothing when starting the software or when copying a section
                If Section.SavedPCRgraph Is Nothing Then
                    Return "The PCR calculation must be run before opening the report."
                Else
                    Return Section.SavedPCRgraph
                End If
            End If

            'If there was PCR Run before Section.LastRun, the graph will be showed
            If (Section.LastRun = "Thickness Design" Or Section.LastRun = "Life Analysis" Or Section.LastRun = "Life/Compaction Analysis") Then
                If Section.SavedPCRgraph IsNot Nothing Then

                    Return Section.SavedPCRgraph
                Else
                    Return "The PCR calculation must be run before opening the report."
                End If
            End If

            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                Thicknessunit = "in."
                PressureUnit = "psi"
                WeightUnit = "lbs"
            Else
                Thicknessunit = "mm"
                PressureUnit = "MPa"
                WeightUnit = "kg"

            End If

            sectionName = Section.Name
            Dim rowNum As Integer = 1
            Dim Rowchecker As Integer = 1

            Dim src = FileIO.SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"

            Dim reportTitle = "Federal Aviation Administration FAARFIELD 2.1 PCR Graph"
            Dim html As String = ""
            Dim tmpdiv As String = ""
            Dim tmptable As String = ""
            Dim tmptd As String = ""
            Dim tmptr As String = ""
            Dim img As String = ""
            Dim tmpthead As String = ""

            ' Report Title Div
            tmpdiv = hu.wrap_h2(reportTitle)
            tmpdiv += hu.wrap_p(MainWindowTitle)
            html += hu.wrap_div(tmpdiv, "report")

            ' Job Title Div
            tmpdiv = hu.wrap_h3("Job Name: " + jobName)
            tmpdiv += hu.wrap_h4("Structure: " + sectionName)

            If Not Section.AnalysisType Is Nothing Then
                tmpdiv += hu.wrap_p("Analysis Type: " + Section.AnalysisType.Name)
            End If

            html += hu.wrap_div(tmpdiv, "job")

            ' Image
            Dim bitmap As Bitmap
            bitmap = DrawPCRGraph()
            tmpdiv = hu.wrap_bmp_img(bitmap)
            bitmap.Dispose()
            html += hu.wrap_div(tmpdiv, "image")
            html += hu.wrap_p("Figure: Aircraft ACR comparison with calculated PCR. Bars represent ACR values; " &
                "the horizontal black line marks the calculated PCR. The dashed orange line shows annual departures (secondary axis).", "chart-caption")

            ' Table Title Div
            tmpdiv = hu.wrap_h3(" ")
            html += hu.wrap_div(tmpdiv, "title")

            'Le: transposed table
            tmptd = hu.wrap_p("No.")
            tmptr = hu.wrap_th(tmptd)
            tmptd = hu.wrap_p("Aircraft Name")
            tmptr += hu.wrap_th(tmptd)
            tmptd = hu.wrap_p("Aircraft ACR")
            tmptr += hu.wrap_th(tmptd)
            tmptd = hu.wrap_p("Calculated PCR")
            tmptr += hu.wrap_th(tmptd)
            tmptd = hu.wrap_p("Annual Departure")
            tmptr += hu.wrap_th(tmptd)

            tmpthead = hu.wrap_tr(tmptr)
            tmptable = hu.wrap_thead(tmpthead)

            Dim tmpAPList = New List(Of IAirplaneInfo)(Section.Airplanes)
            tmpAPList.Sort(Function(x, y) y.ACRB.CompareTo(x.ACRB))

            For Each airplane In tmpAPList
                If Not Right(airplane.Name, 5) = "Belly" Then
                    tmptd = hu.wrap_p(rowNum.ToString())
                    tmptr = hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(airplane.Name)
                    tmptr += hu.wrap_td(tmptd)
                    tmptd = hu.wrap_p(Format(airplane.ACRB.ToString))
                    tmptr += hu.wrap_td(tmptd)

                    If airplane.Name = Section.SectionPCRCriticalAirplaneName Then
                        tmptd = hu.wrap_p(Format(Section.SectionPCRNewPCN, "0.0").ToString)
                    Else
                        tmptd = hu.wrap_p("-")
                    End If
                    tmptr += hu.wrap_td(tmptd)

                    tmptd = hu.wrap_p(Format(airplane.NumberDepartures, "N0").ToString)
                    tmptr += hu.wrap_td(tmptd)

                    tmptable += hu.wrap_tr(tmptr)
                    Rowchecker += 1
                    rowNum += 1
                End If
            Next

            ' Table Div
            tmpdiv = hu.wrap_table(tmptable)
            html += hu.wrap_div(tmpdiv)

            ' Complete HTML Page
            html = hu.CreateHtmlPage(html, reportTitle)
            Section.SavedPCRgraph = html
            Return html

        End Function


        Private Function BitmapImage2Bitmap(ByVal bitmapImage As BitmapImage) As Bitmap
            Dim bm As Bitmap
            Using outStream As MemoryStream = New MemoryStream()
                Dim enc As BitmapEncoder = New PngBitmapEncoder()
                enc.Frames.Add(BitmapFrame.Create(bitmapImage))
                enc.Save(outStream)
                Using bitmap As New Bitmap(outStream)
                    bm = New Bitmap(bitmap)
                End Using
            End Using
            Return bm
        End Function


        ''' <summary>
        ''' Draws PCR Chart for current Section
        ''' </summary>
        ''' <returns>PCR Graph Image as Bitmap</returns>
        Public Function DrawPCRGraph() As Bitmap

            Dim maxDepartures As Integer = 0
            Dim trafficScale As Integer = 1
            Dim chart As New RadCartesianChart()
            Dim data = New RadObservableCollection(Of PCRGraphItem)()
            Dim maxACR As Double = 400
            Dim tmpAPList = New List(Of AirplaneInfo)(CurrentAirplanes)

            tmpAPList.Sort(Function(x, y) x.ACRB.CompareTo(y.ACRB))

            For i = 0 To tmpAPList.Count - 1
                If Not tmpAPList(i).Name Is Nothing AndAlso tmpAPList(i).ACRB > 0 Then
                    data.Add(New PCRGraphItem() With {
                            .AircraftName = tmpAPList(i).Name,
                            .ACRB = tmpAPList(i).ACRB,
                            .Departures = tmpAPList(i).NumberDepartures
                        }
                    )
                    If tmpAPList(i).NumberDepartures > maxDepartures Then
                        maxDepartures = tmpAPList(i).NumberDepartures
                    End If
                    If tmpAPList(i).ACRB > maxACR Then
                        maxACR = tmpAPList(i).ACRB
                    End If
                End If
            Next

            ' Prevent multiple aircraft from being added to same category in graph 
            Dim acNames(data.Count - 1) As String
            Dim uniqueNames As New List(Of String)
            Dim acNamesCount As New List(Of Integer)
            Dim tmpName As String = String.Empty

            For count = 0 To data.Count - 1
                acNames(count) = data(count).AircraftName
            Next

            For Each s1 In acNames.Distinct().ToList()
                tmpName = s1
                uniqueNames.Add(tmpName)
                acNamesCount.Add(acNames.Count(Function(s) s = tmpName))
            Next

            ' Add count tag to repeated aircraft names for graph
            For count2 = 0 To acNamesCount.Count - 1
                If acNamesCount(count2) > 1 Then
                    Dim tmpNumber = 1
                    For Each ap In data
                        If ap.AircraftName = uniqueNames(count2) Then
                            ap.AircraftName = ap.AircraftName & " (" & tmpNumber & ")"
                            tmpNumber += 1
                        End If
                    Next
                End If
            Next

            If CurrentSectionView.Section.SectionPCRNewPCN > maxACR Then
                maxACR = CurrentSectionView.Section.SectionPCRNewPCN
            End If

            If maxACR > 400 Then
                Dim tmpMaxACR = CInt(maxACR / 100) + 1
                maxACR = tmpMaxACR * 100
            End If

            ' Scale annual departures to range
            If maxDepartures > maxACR AndAlso maxDepartures <= maxACR * 10 Then
                trafficScale = 10
            ElseIf maxDepartures > maxACR * 10 Then
                trafficScale = 100
            End If

            Dim departuresLineSeries As New ChartView.LineSeries() With {
                .ItemsSource = data,
                .CategoryBinding = New PropertyNameDataPointBinding("AircraftName"),
                .ValueBinding = New PropertyNameDataPointBinding("Departures"),
                .VerticalAxis = New LinearAxis() With {
                    .Title = "Annual Departures",
                    .HorizontalLocation = Telerik.Charting.AxisHorizontalLocation.Right,
                    .LineStroke = New SolidColorBrush(Colors.DarkOrange),
                    .LineThickness = 3,
                    .Maximum = maxACR * trafficScale,
                    .Minimum = 0
                },
                .DashArray = New DoubleCollection() From {8, 4},
                .Stroke = New SolidColorBrush(Colors.DarkOrange),
                .LegendSettings = New SeriesLegendSettings() With {.Title = "Annual Departures"}
            }

            Dim acrBarSeries As New BarSeries() With {
                .Width = 20,
                .ItemsSource = data,
                .CategoryBinding = New PropertyNameDataPointBinding("AircraftName"),
                .ValueBinding = New PropertyNameDataPointBinding("ACRB"),
                .VerticalAxis = New LinearAxis() With {
                    .Title = "ACR or PCR (" & (CurrentSectionView.Section.SectionPCRPaveType).ToString & "/" & (CurrentSectionView.Section.SectionSubgradeCategory).ToString & ")",
                    .HorizontalLocation = Telerik.Charting.AxisHorizontalLocation.Left,
                    .LineStroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211)),
                    .LineThickness = 3,
                    .Maximum = maxACR,
                    .Minimum = 0
                },
                .LegendSettings = New SeriesLegendSettings() With {.Title = "ACR"}
            }

            'Using Annotations to simulate Major Grid Lines
            If maxACR < 1100 Then
                For i = 100 To maxACR Step 100
                    Dim majLineAnnt = New CartesianGridLineAnnotation() With {.Value = i, .Axis = acrBarSeries.VerticalAxis, .StrokeThickness = 0.5, .Stroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211))}
                    chart.Annotations.Add(majLineAnnt)
                Next
            Else
                For i = 500 To maxACR Step 500
                    Dim majLineAnnt = New CartesianGridLineAnnotation() With {.Value = i, .Axis = acrBarSeries.VerticalAxis, .StrokeThickness = 0.5, .Stroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211))}
                    chart.Annotations.Add(majLineAnnt)
                Next
            End If

            chart.Series.Add(acrBarSeries)
            chart.Series.Add(departuresLineSeries)

            chart.HorizontalAxis = New CategoricalAxis() With {
                .Title = "Aircraft",
                .LineThickness = 3,
                .LineStroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211)),
                .LabelFitMode = Telerik.Charting.AxisLabelFitMode.Rotate ', .LabelRotationAngle = 270
            }

            'Using Annotation to display Calculated PCR
            Dim labelDef = New ChartAnnotationLabelDefinition() With {.HorizontalAlignment = AlignmentX.Left, .Location = ChartAnnotationLabelLocation.Inside, .VerticalOffset = -20, .HorizontalOffset = 20}
            Dim pcrAnnt = New CartesianGridLineAnnotation() With {.Value = CurrentSectionView.Section.SectionPCRNewPCN, .Axis = acrBarSeries.VerticalAxis, .StrokeThickness = 2, .Stroke = New SolidColorBrush(System.Windows.Media.Colors.Black), .Label = Format(CurrentSectionView.Section.SectionPCRNewPCN, "0.0").ToString & "/" & (CurrentSectionView.Section.SectionPCRPaveType).ToString & "/" & (CurrentSectionView.Section.SectionSubgradeCategory).ToString, .LabelDefinition = labelDef}
            chart.Annotations.Add(pcrAnnt)

            'Using Annotation To simulate annual departure For Single aircraft
            If tmpAPList.Count = 1 Then
                Dim departureAnnt = New CartesianGridLineAnnotation() With {.Value = tmpAPList(0).NumberDepartures, .Axis = departuresLineSeries.VerticalAxis, .StrokeThickness = 2, .DashArray = New DoubleCollection() From {8, 4}, .Stroke = New SolidColorBrush(Colors.DarkOrange)}
                chart.Annotations.Add(departureAnnt)
            End If

            ' Text Size needs increased for higher dpi
            Dim chartSize As New System.Windows.Size(800, 600)

            chart.Measure(chartSize)
            chart.Arrange(New Rect(0, 30, 800, 600))
            chart.UpdateLayout()

            ' Offset chart legend to miss PCR annotation
            Dim legendOffset = 0
            Dim legendOffsetScale = CInt(1000 / maxACR)
            If maxACR - CurrentSectionView.Section.SectionPCRNewPCN < 200 * legendOffsetScale Then
                legendOffset = 60 * legendOffsetScale
            End If

            Return CreateBitmapFromChart(chart, legendOffset)

        End Function


        'Le: Change graph visual
        Private Function CreateBitmapFromChart(chart As RadCartesianChart, legendOffset As Integer) As Bitmap

            Dim chartSize As System.Windows.Size = chart.RenderSize
            Dim bmp As Bitmap

            Dim scale As Double = 2.0
            Dim rtb As New RenderTargetBitmap(CInt(chartSize.Width * scale), CInt((chartSize.Height + 100) * scale), 96D * scale, 96D * scale, PixelFormats.Pbgra32)

            Dim dv As DrawingVisual = New DrawingVisual()
            Dim dc As DrawingContext = dv.RenderOpen()

            dc.DrawRectangle(Windows.Media.Brushes.White, Nothing, New Rect(New Windows.Size(800, 800)))

            dc.Close()
            rtb.Render(dv)

            dc = dv.RenderOpen()

            'Draw Chart Title
            dc.DrawRectangle(Windows.Media.Brushes.White, Nothing, New Rect(0, 1, 800, 17))
            dc.DrawText(New FormattedText(CurrentJob.Name & ": " & CurrentSectionView.Section.Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 16, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(300, 1))

            dc.Close()
            rtb.Render(dv)

            rtb.Render(chart)

            dc = dv.RenderOpen()

            'Draw custom legend
            dc.DrawRectangle(New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211)), Nothing, New Rect(75, 678, 20, 10))
            dc.DrawRectangle(New SolidColorBrush(System.Windows.Media.Colors.Black), Nothing, New Rect(200, 680, 20, 3))
            dc.DrawRectangle(New SolidColorBrush(Colors.DarkOrange), Nothing, New Rect(300, 680, 15, 3))
            dc.DrawRectangle(New SolidColorBrush(Colors.DarkOrange), Nothing, New Rect(320, 680, 15, 3))
            dc.DrawText(New FormattedText("ACR", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(100, 675))
            dc.DrawText(New FormattedText("PCR", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(225, 675))
            dc.DrawText(New FormattedText("Annual Departures", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(342, 665))
            dc.DrawText(New FormattedText("(Secondary Axis)", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(342, 680))

            dc.Close()
            rtb.Render(dv)

            Dim strm As MemoryStream = New MemoryStream()
            Dim enc As BitmapEncoder = New PngBitmapEncoder()
            enc.Frames.Add(BitmapFrame.Create(rtb))
            enc.Save(strm)
            bmp = New Bitmap(strm)

            Return bmp
        End Function


        ''' <summary>
        ''' Updates the sections AnalysisType so that it can be found on the AnalysisType combobox.
        ''' The deserialized AnalysisType object is on on the ListAnalysis so it can't be found with SelectValue binding
        ''' This function replaces the deserialized Analysis type with the matching name from the ListAnalysis
        ''' </summary>
        ''' <param name="section"></param>
        Private Sub UpdateAnalysisTypeFromList(section As ISection)
            If section.AnalysisType Is Nothing Then Return
            For Each Analysis As Object In ListAnalysis
                If Analysis.Name = section.AnalysisType.Name Then
                    section.AnalysisType = Analysis
                End If
            Next
        End Sub


        Private _unitLabel As String = ""
        Public Property UnitLabel As String

            Get
                _unitLabel = "Total thickness to the top of the subgrade (" + _unitLabel + "):"
                Return _unitLabel
            End Get
            Set(value As String)
                _unitLabel = value
                OnPropertyChanged(NameOf(UnitLabel))
            End Set
        End Property


        Private _edgeStressLabel As String = ""
        Public Property EdgeStressLabel As String
            Get
                If _edgeStressLabel = "mm" Then
                    _edgeStressLabel = "(MPa)"
                Else
                    _edgeStressLabel = "(psi)"
                End If
                Return _edgeStressLabel
            End Get
            Set(value As String)
                _edgeStressLabel = value
                OnPropertyChanged(NameOf(EdgeStressLabel))
            End Set
        End Property


        Private _interiorStressLabel As String = ""
        Public Property InteriorStressLabel As String
            Get
                If _interiorStressLabel = "mm" Then
                    _interiorStressLabel = "(MPa)"
                Else
                    _interiorStressLabel = "(psi)"
                End If
                Return _interiorStressLabel
            End Get
            Set(value As String)
                _interiorStressLabel = value
                OnPropertyChanged(NameOf(InteriorStressLabel))
            End Set
        End Property


        Private _slabOptions As Boolean = False
        Public Property SlabOptions As Boolean
            Get
                Return _slabOptions
            End Get
            Set(value As Boolean)
                _slabOptions = value
                OnPropertyChanged(NameOf(SlabOptions))
            End Set
        End Property


        Public Property RunButtonText
            Get
                Return _runButtonText
            End Get
            Set(value)
                _runButtonText = value
                OnPropertyChanged(NameOf(RunButtonText))
            End Set
        End Property


        Public Property RunPCN
            Get
                Return _runPCN
            End Get
            Set(value)
                _runPCN = value
                OnPropertyChanged(NameOf(RunPCN))
            End Set
        End Property


        Public Property MessageText
            Get
                Return _messageText
            End Get
            Set(value)
                _messageText = value
                CurrentSectionView.Section.SectionRunStatus = value
                OnPropertyChanged(NameOf(MessageText))
            End Set
        End Property


        Public Property ImageVisibility As Visibility
            Get
                Return _imagevisibility
            End Get
            Set(value As Visibility)
                _imagevisibility = value
                OnPropertyChanged(NameOf(ImageVisibility))
            End Set
        End Property


        Private _JobViewBorderThickness As Integer = 0
        Public Property JobViewBorderThickness As Integer
            Get
                Return _JobViewBorderThickness
            End Get
            Set(value As Integer)
                _JobViewBorderThickness = value
                OnPropertyChanged(NameOf(JobViewBorderThickness))
            End Set
        End Property


        Public Sub SlabListUpdate()
            If CurrentSectionView.Section.SlabComplete = True And SlabOptions = True And CurrentAirplanes.Count <> 0 And CurrentSectionView.Section.AnalysisType IsNot Nothing AndAlso CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Then
                If CurrentSectionView.Section.LastRun = "PCR" Then
                    _slabEdgeStress = CurrentSectionView.Section.SlabEdgeStressArray(1)
                    _slabInteriorStress = CurrentSectionView.Section.SlabInteriorStressArray(1)

                    CriticalStressAircraft = CurrentAirplanes.Item(0).Name
                    OnPropertyChanged(NameOf(CriticalStressAircraft))
                    OnPropertyChanged(NameOf(SlabEdgeStress))
                    OnPropertyChanged(NameOf(SlabInteriorStress))
                Else
                    Dim index = 0
                    For i = 0 To CurrentAirplanes.Count - 1
                        If CurrentAirplanes.Item(i) Is SelectedAirplane Then
                            index = i
                        End If
                    Next
                    If CurrentSectionView.Section.SlabEdgeStressArray(index + 1) <> CurrentSectionView.Section.SlabEdgeStressArray.Max Then
                        AircraftStressLabel = "Selected Aircraft: " + CurrentAirplanes.Item(index).Name
                    Else
                        AircraftStressLabel = "Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft
                    End If

                    _slabEdgeStress = CurrentSectionView.Section.SlabEdgeStressArray(index + 1)
                    _slabInteriorStress = CurrentSectionView.Section.SlabInteriorStressArray(index + 1)
                    _criticalStressAircraft = CurrentAirplanes.Item(index).Name

                    OnPropertyChanged(NameOf(CriticalStressAircraft))
                    OnPropertyChanged(NameOf(SlabEdgeStress))
                    OnPropertyChanged(NameOf(SlabInteriorStress))
                End If
            End If

        End Sub


        Public Sub SlabStressUpdate()

            If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Then 'Only functioning for New Rigid pavement type so far
                ReDim CurrentSectionView.Section.SlabEdgeStressArray(CurrentAirplanes.Count)
                ReDim CurrentSectionView.Section.SlabInteriorStressArray(CurrentAirplanes.Count)
                'Initializes the first stress values
                Dim tempCriticalStressAircraft As String = "" ' = CurrentAirplanes.Item(0).Name
                Dim tempSlabEdge = modOVERLAY_NP.HorizStressResponse1(1, 1)
                Dim tempSlabInterior = modOVERLAY_NP.HorizStressResponse2(1, 1)
                If CurrentSectionView.Section.LastRun = "PCR" Then
                    CurrentSectionView.Section.SlabEdgeStressArray(1) = HorizStressResponse1(1, 1)
                    CurrentSectionView.Section.SlabInteriorStressArray(1) = HorizStressResponse2(1, 1)
                Else
                    For i = 1 To CurrentAirplanes.Count
                        If HorizStressResponse1(i, 1) <> 0 Then
                            CurrentSectionView.Section.SlabEdgeStressArray(i) = HorizStressResponse1(i, 1)
                        End If
                        If HorizStressResponse2(i, 1) <> 0 Then
                            CurrentSectionView.Section.SlabInteriorStressArray(i) = HorizStressResponse2(i, 1)
                        End If
                    Next

                End If

                'Sorts for the edge and interior values
                If CurrentSectionView.Section.LastRun = "PCR" Then

                Else
                    For i = 1 To CurrentAirplanes.Count
                        If CurrentSectionView.Section.SlabEdgeStressArray(i) > tempSlabEdge Then
                            tempSlabEdge = CurrentSectionView.Section.SlabEdgeStressArray(i)
                            tempSlabInterior = CurrentSectionView.Section.SlabInteriorStressArray(i)
                            tempCriticalStressAircraft = CurrentAirplanes.Item(i - 1).Name
                        End If
                    Next
                End If
                'sets the variables to the highest values in the arrays after the sort
                If CurrentJob.DesignOptions.SlabStress = False Then

                Else
                    _slabEdgeStress = tempSlabEdge
                    _slabInteriorStress = tempSlabInterior
                    CurrentSectionView.Section.SlabEdgeStress = _slabEdgeStress
                    CurrentSectionView.Section.SlabInteriorStress = _slabInteriorStress

                    If tempCriticalStressAircraft <> "" Then
                        _criticalStressAircraft = tempCriticalStressAircraft
                        CriticalStressReport = tempCriticalStressAircraft
                        CurrentSectionView.Section.CriticalStressAicraft = tempCriticalStressAircraft
                    End If

                End If
                OnPropertyChanged(NameOf(CriticalStressAircraft))
                OnPropertyChanged(NameOf(SlabEdgeStress))
                OnPropertyChanged(NameOf(SlabInteriorStress))
                _slabEdgeStress = Nothing
                _slabInteriorStress = Nothing
                SlabEdgeStress = Nothing
                SlabInteriorStress = Nothing
                AircraftStressLabel = "Most Demanding Aircraft: " + CurrentSectionView.Section.CriticalStressAicraft
            Else

            End If

        End Sub


        ReadOnly Property TotalThicknessUpdate As String
            Get
                _totalthicknessupdate = "0"
                Dim TotalThickness As Double
                Dim NLayers As Integer = CurrentSectionView.Section.Layers.Count - 1
                Dim NlayerToTopOfSubgrade As Integer = 0
                If CurrentSectionView.Section.Layers.Count <> 0 Then
                    If CurrentSectionView.Section.Layers.Item(NLayers).Category = "General" Then
                        NlayerToTopOfSubgrade = NLayers - 1
                    Else
                        NlayerToTopOfSubgrade = NLayers

                    End If

                    For i = 0 To NlayerToTopOfSubgrade
                        TotalThickness += CurrentSectionView.Section.Layers.Item(i).Thickness.GetValue(CurrentJob.DesignOptions.MeasurementSystem)
                    Next
                End If
                Dim Unit As String
                If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                    Unit = "in."
                    _totalthicknessupdate = Format(TotalThickness, "N1").ToString + " "
                Else
                    Unit = "mm"
                    _totalthicknessupdate = Format(TotalThickness, "N0").ToString + " "
                End If

                Return _totalthicknessupdate
            End Get
        End Property


        Public Property AnalysisType As AnalysisType
            Get
                Return _analysisType
            End Get
            Set(value As AnalysisType)
                If value IsNot Nothing AndAlso _analysisType IsNot Nothing AndAlso value.Name <> _analysisType.Name Then
                    OnPropertyChanged(NameOf(CurrentAirplanes))
                    OnPropertyChanged(NameOf(CurrentSectionView))
                End If

                _analysisType = value

                Slab_Changed(Nothing)

                If CurrentSectionView.Section.AnalysisType Is Nothing Then
                    If Not value Is Nothing Then
                        Debug.WriteLine("Initial update analysis type.  TODO:Update Layers")
                        CurrentSectionView.Section.AnalysisType = value
                        UpdateLayersFromDefault()
                    End If
                ElseIf CurrentSectionView.Section.AnalysisType.Name <> _analysisType.Name Then
                    Debug.WriteLine("Changing analysis type.  TODO:Update Layers")
                    CurrentSectionView.Section.AnalysisType = value
                    UpdateLayersFromDefault()
                    ResetSlabStressValues()
                Else
                    CurrentSectionView.Section.AnalysisType = value
                End If

                If Not CurrentSectionView.Section.Layers.Count = 0 Then
                    Layername1 = CurrentSectionView.Section.Layers.Item(0).Name
                End If

                If Not AnalysisType Is Nothing Then
                    _SelectDesignLayerEnabled = True
                    OnPropertyChanged(NameOf(SelectDesignLayerEnabled))
                End If
                Try
                    If PCAConversionFormula = True Then
                        CurrentSectionView.Section.PCAConversionTracker = True
                        If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                            If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).KValueActive Then
                                CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).PCAConversionActive = True

                            End If
                        End If
                    End If
                Catch ex As Exception

                End Try
                Try
                    If NCHRPFormula = True Then
                        CurrentSectionView.Section.NCHRPTracker = True
                        If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                            If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).KValueActive Then
                                CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).NCHRPActive = True

                            End If
                        End If
                    End If
                Catch ex As Exception

                End Try
                NewThickness = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary())
                ProfileImage = DrawProfile()

                CheckRunAvailable()

            End Set
        End Property


        Public Property AircraftStressLabel As String
            Get
                Return _aircraftStressLabel
            End Get
            Set(value As String)
                _aircraftStressLabel = value
                OnPropertyChanged(NameOf(AircraftStressLabel))
            End Set
        End Property


        ''' <summary>
        ''' Copy the default layers that come with an Analysis Type to the current list of layers.
        ''' </summary>
        Public Sub UpdateLayersFromDefault()
            CurrentSectionView.Section.Layers = New ObservableCollection(Of IMaterial)()
            CurrentLayers = New ObservableCollection(Of IMaterial)()
            For Each defaultLayer In CurrentSectionView.Section.AnalysisType.DefaultLayers
                Dim layer = FaarFieldFactory.CreateMaterial(FaarFieldFactory, defaultLayer, True)
                CurrentSectionView.Section.Layers.Add(layer)
                CurrentLayers.Add(layer)
            Next
            Call SelectDesignedLayer()
            Call OnUpdateselectedMaterial()
            Call OnUpdateAnalysisType()
        End Sub


        Public Property SelectedTabIndex
            Get
                Return _selectedTabIndex
            End Get
            Set(value)
                _selectedTabIndex = value
                OnPropertyChanged(NameOf(SelectedTabIndex))
            End Set
        End Property


        Public Property Layername1 As String
            Get
                If Not CurrentSectionView.Section.Layers.Count = 0 Then
                    _layername1 = CurrentSectionView.Section.Layers.Item(0).Name
                End If

                Return _layername1
            End Get
            Set(value As String)

            End Set
        End Property


        Dim _StructureListView_Margin As System.Windows.Thickness
        Public Property StructureListView_Margin As System.Windows.Thickness
            Get
                Return _StructureListView_Margin
            End Get
            Set(value As System.Windows.Thickness)

                _StructureListView_Margin = value
                OnPropertyChanged(NameOf(StructureListView_Margin))
            End Set
        End Property


        Public ReadOnly Property OnValidateModulus As ICommand
            Get
                Return New DelegateCommand(AddressOf ValidateModulus, AddressOf NewJobEnabled)
            End Get
        End Property


        Private Sub ValidateModulus(context As Object)
            If Not _selectedMaterial Is Nothing Then

                If _selectedMaterial.Name = "Subgrade" Then
                    OnPropertyChanged(NameOf(CurrentLayers))
                End If
            End If
        End Sub


        Private Shared Function ValidateModulusEndable(context As Object) As Boolean
            Return True
        End Function


#Region "Menu Handlers"

        Private Sub NewJob(context As Object)

            Dim jobchecker As Boolean = False
            Dim job = FaarFieldFactory.CreateFaarFieldJob(FaarFieldFactory, "")
            job.Name = "New Job " + (Jobs.Count + 1).ToString()

            Jobs.Add(New JobViewModel(FaarFieldFactory, job, Me))
            Dim job1 = CType(Jobs(Jobs.Count - 1), JobViewModel)
            SetCurrentJob(job1)

            Dim section = FaarFieldFactory.CreateSection(FaarFieldFactory)
            section.Name = GetNextSectionName()

            CurrentSectionView = CurrentJobView.SectionFolder.AddSection(section)
            CurrentJob.Sections.Add(section)
            CurrentSectionView.Section.PCAConversionTracker = False
            PCAConversionFormula = False
            CurrentSectionView.Section.NCHRPTracker = False
            NCHRPFormula = False

            DesignLife = CurrentSectionView.Section.Life

            Sci = CurrentSectionView.Section.Sci

            Cdfu = CurrentSectionView.Section.PercentCdfu
            CurrentSectionView.IsSelected = True

            For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                CurrentSectionView.Section.Layers.Remove(CurrentSectionView.Section.Layers.Item(0))
                CurrentLayers.Clear()
                CurrentLayers.Clear()
            Next

            ProfileImage = DrawProfile()
            NewThickness = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary())

            _sectionIsHidden = False
            _InnerSectionIsHidden = False
            _airplanesIsHidden = False
            _notesIsHidden = False
            _selectedtree = 0
            OnPropertyChanged(NameOf(SelectedTree))
            JobRequiredButtonEnabled = True
            SectionRequiredButtonEnabled = True
            OnPropertyChanged(NameOf(SectionIsHidden))
            OnPropertyChanged(NameOf(AirplanesIsHidden))
            OnPropertyChanged(NameOf(NotesIsHidden))
            OnPropertyChanged(NameOf(Jobs))

        End Sub


        Public ReadOnly Property OnSectionReportCreatePdf As ICommand
            Get
                Return New DelegateCommand(AddressOf SectionReportCreatePdf, AddressOf SectionReportCreatePdfEnabled)
            End Get
        End Property

        Public ReadOnly Property OnSectionReportOpenHtml As ICommand
            Get
                Return New DelegateCommand(AddressOf SectionReportOpenHtml, AddressOf SectionReportCreatePdfEnabled)
            End Get
        End Property


        Public ReadOnly Property ImageShowHide As ICommand
            Get
                Return New DelegateCommand(AddressOf OnImageShowHide, AddressOf OnImageShowHideEnabled)
            End Get
        End Property


        Public Sub OnImageShowHide(context As Object)
            If _imagevisibility = Visibility.Hidden Then
                _imagevisibility = Visibility.Visible
            Else
                _imagevisibility = Visibility.Hidden
            End If

            OnPropertyChanged(NameOf(ImageVisibility))
            SelectedTabIndex = 2

        End Sub


        Private Function OnImageShowHideEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Sub OnChangeImageGraphics(context As Object)
            If _brushes.Count = 20 Then
                _brushes = LoadBrushes2()
            Else
                _brushes = LoadBrushes()
            End If

            ProfileImage = DrawProfile()

            OnPropertyChanged(NameOf(TotalThicknessUpdate))
            SelectedTabIndex = 2

        End Sub


        Private Function OnChangeImageGraphicsEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Shared Function SectionReportCreatePdfEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Sub SectionReportCreatePdf(context As Object)
            Dim testName As String
            Dim rptName = context
            Dim pdfFile As String
            Dim sName As String
            Dim resultName As String = ""

            Dim myFile As String = ""
            Dim justName As String = ""
            Dim justPath As String = ""
            Dim justExtension As String = ""
            Dim savePDF As System.Windows.Forms.SaveFileDialog = New System.Windows.Forms.SaveFileDialog()
            savePDF.Filter = "PDF Files|*.pdf|Text file|*.txt"
            savePDF.Title = "Save as a PDF file"

            Dim numPlanes As Integer = 0
            numPlanes = CurrentSectionView.Section.Airplanes.Count
            If context = "SummaryRpt" Then
                savePDF.FileName = Me.CurrentJob.Name + "-SummaryReport"

            ElseIf context = "StructureRpt" Then
                savePDF.FileName = Me.CurrentJob.Name + "-" + Me.CurrentSectionView.Report.Parent.Name + "-StructureReport"

            ElseIf context = "CDFGraphRpt" Then
                savePDF.FileName = Me.CurrentJob.Name + "-" + Me.CurrentSectionView.Report.Parent.Name + "-CDFGraphReport"

            ElseIf context = "PCRRpt" Then
                savePDF.FileName = Me.CurrentJob.Name + "-" + Me.CurrentSectionView.Report.Parent.Name + "-PCR Report"
            ElseIf context = "PCRGraphRpt" Then
                savePDF.FileName = Me.CurrentJob.Name + "-" + Me.CurrentSectionView.Report.Parent.Name + "-PCRGraphReport"
            ElseIf context = "AirportMasterRecordRpt" Then
                savePDF.FileName = Me.CurrentJob.Name + "-" + Me.CurrentSectionView.Report.Parent.Name + "-Airport Master Record"

            ElseIf context = "DetailedRpt" Then
                savePDF.FileName = Me.CurrentJob.Name + "-" + Me.CurrentSectionView.Report.Parent.Name + "-CM Report"

            End If

            If savePDF.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                myFile = savePDF.FileName   'includes filename in path
                justName = Path.GetFileNameWithoutExtension(myFile) 'include only filename
                justPath = Path.GetDirectoryName(myFile)    'include only path
                justExtension = Path.GetExtension(myFile)   'include only ext

            Else
                MessageBox.Show("PDF generation was canceled by user.")
                Return
            End If

            Dim ACnum As Integer = 0

            If rptName = "SummaryRpt" Then
                testName = Me.RefreshHtml()
                resultName = justPath & Path.DirectorySeparatorChar & justName & justExtension
                sName = "SummRpt.pdf"

            ElseIf rptName = "StructureRpt" Then
                ACnum = Me.CurrentSectionView.Section.Airplanes.Count       'airplanes count
                testName = Me.refreshsectionreport()
                resultName = justPath & Path.DirectorySeparatorChar & justName & justExtension
                sName = "StrucRpt.pdf"

            ElseIf rptName = "CDFGraphRpt" Then
                testName = Me.refreshcdfgraph()
                resultName = justPath & Path.DirectorySeparatorChar & justName & justExtension
                sName = "CDFRpt.pdf"

            ElseIf rptName = "PCRRpt" Then
                testName = Me.refreshPCRReport()
                resultName = justPath & Path.DirectorySeparatorChar & justName & justExtension
                sName = "PCRRpt.pdf"

            ElseIf rptName = "PCRGraphRpt" Then
                testName = Me.RefreshPCRGraph()
                resultName = justPath & Path.DirectorySeparatorChar & justName & justExtension
                sName = "PCRGraphRpt.pdf"

            ElseIf rptName = "AirportMasterRecordRpt" Then
                testName = Me.refreshAirportMasterRecord()
                resultName = justPath & Path.DirectorySeparatorChar & justName & justExtension
                sName = "AirportMasterRecordRpt.pdf"

            ElseIf rptName = "DetailedRpt" Then
                ' Use the SVG-based HTML report generator for high-quality vector PDF output
                Dim Section = CurrentSectionView.Section
                Dim theJob = CurrentJob
                Dim thkU As String, presU As String, wgtU As String, lenU As String
                If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                    thkU = "in." : presU = "psi" : wgtU = "lbs" : lenU = "in."
                Else
                    thkU = "mm" : presU = "MPa" : wgtU = "kg" : lenU = "mm"
                End If
                Dim analysisName As String = If(Section.AnalysisType IsNot Nothing, Section.AnalysisType.Name, Nothing)
                testName = Libs.HtmlReportGenerator.Generate(
                    FEDFAA1.gDetailedReportData,
                    theJob.Name,
                    Section.Name,
                    analysisName,
                    MainWindowTitle,
                    thkU, presU, wgtU, lenU)
                resultName = justPath & Path.DirectorySeparatorChar & justName & justExtension
                sName = "DetailedRpt.pdf"

            Else
                Return
            End If

            If rptName Is Nothing Then
                Return
            End If

            If rptName = "SummaryRpt" Or rptName = "PCRRpt" Or rptName = "AirportMasterRecordRpt" Or rptName = "DetailedRpt" Then
                pdfFile = resultName
            Else
                pdfFile = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & sName
            End If

            hu.HtmltoPdf(testName, myFile)

            If System.Windows.Forms.DialogResult.OK And pdfcreatorchecker = True Then
                MessageBox.Show("PDF created")
            Else
                MessageBox.Show("PDF was not created")
                pdfcreatorchecker = True
            End If

        End Sub


        ''' <summary>
        ''' Saves the CM Report as an HTML file and opens it in the default browser.
        ''' </summary>
        Private Sub SectionReportOpenHtml(context As Object)
            Dim rptName As String = CStr(context)
            If rptName <> "DetailedRpt" Then Return

            ' Use the new HTML report generator (native SVG + modern CSS)
            Dim Section = CurrentSectionView.Section
            Dim theJob = CurrentJob
            Dim thkU As String, presU As String, wgtU As String, lenU As String
            If TypeOf (theJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                thkU = "in." : presU = "psi" : wgtU = "lbs" : lenU = "in."
            Else
                thkU = "mm" : presU = "MPa" : wgtU = "kg" : lenU = "mm"
            End If
            Dim analysisName As String = If(Section.AnalysisType IsNot Nothing, Section.AnalysisType.Name, Nothing)
            Dim htmlContent As String = Libs.HtmlReportGenerator.Generate(
                FEDFAA1.gDetailedReportData,
                theJob.Name,
                Section.Name,
                analysisName,
                MainWindowTitle,
                thkU, presU, wgtU, lenU)

            ' Write to a temp file and open immediately — no save dialog
            Dim tempName As String = theJob.Name & "-" & Section.Name & "-CM Report"
            ' Sanitize filename
            For Each c As Char In System.IO.Path.GetInvalidFileNameChars()
                tempName = tempName.Replace(c, "_"c)
            Next
            Dim tempPath As String = System.IO.Path.Combine(System.IO.Path.GetTempPath(), tempName & ".html")
            hu.HtmlToFile(htmlContent, tempPath)
        End Sub


        Private Shared Function NewJobEnabled(context As Object) As Boolean
            Return True
        End Function


        Private _StartupJobLoading As Boolean = False
        Public Property StartupJobLoading As Boolean
            Get
                Return _StartupJobLoading
            End Get
            Set
                If Value <> _StartupJobLoading Then
                    _StartupJobLoading = Value
                    OnPropertyChanged(NameOf(StartupJobLoading))
                End If
            End Set
        End Property


        Public Sub OpenStartupJob(path As String)
            LoadJob(path)
            StartupJobLoading = True
        End Sub


        Private Sub OpenJob(context As Object)
            Dim openFile As OpenFileDialog = New OpenFileDialog With {
                .InitialDirectory = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"
            }
            If openFile.ShowDialog() = True Then
                LoadJob(openFile.FileName)
            End If
        End Sub


        Private _ImportedMixName As String = ""
        Public Property ImportedMixName As String
            Get
                Return _ImportedMixName
            End Get
            Set(value As String)
                _ImportedMixName = value
                OnPropertyChanged(NameOf(ImportedMixName))
            End Set
        End Property


        Private Sub LoadJob(FilePath As String)
            Dim MixListName As String = ""
            Dim jobchecker As Boolean = False
            DesignLifeChecker = False
            Dim SlabOptionsJobLoad As Boolean = False
            If Path.GetExtension(FilePath) = ".jobx" Then
                Dim ser = New DataContractSerializer(GetType(FaarFieldJob))
                Dim textReader As TextReader = New StreamReader(FilePath)
                Dim json = textReader.ReadToEnd()
                textReader.Close()

                Dim stream As New MemoryStream
                Dim faarFieldJob As FaarFieldJob
                Dim jobmeasurementsystem = "US Customary"
                Dim FrostDepthReading As Thickness = Nothing

                'Updates name changes to Pavement Types
                json = json.Replace("<Name>HMA on Flexible</Name>", "<Name>HMA Overlay on Flexible</Name>")
                json = json.Replace("<Name>HMA on Rigid</Name>", "<Name>HMA Overlay on Rigid</Name>")
                json = json.Replace("<Name>PCC on Flexible</Name>", "<Name>PCC Overlay on Flexible</Name>")
                json = json.Replace("<Name>Unbonded on Rigid</Name>", "<Name>Unbonded PCC Overlay on Rigid</Name>")

                Dim xmlElem As Object

                Try
                    xmlElem = XElement.Parse(json)
                    corruptFlag = False
                Catch ex As Exception
                    corruptFlag = True
                    MessageBox.Show("Selected file is corrupt.")
                    Debug.WriteLine(ex)
                    Exit Sub
                End Try

                For Each e As XElement In xmlElem.DescendantsAndSelf()

                    If e.Name.LocalName = "MeasurementSystem" Then
                        jobmeasurementsystem = e.Value

                    End If

                    If e.Name.LocalName = "FrostDepth" Then
                        FrostDepthReading = FaarFieldFactory.CreateThickness(e.Value, FaarFieldFactory.CreateUsCustomary)

                    End If

                    If e.Name.LocalName = "ACRThick" Then

                        Dim ns As XNamespace = e.Name.Namespace
                        Dim newelementacrthick = New XElement(ns + "ACRThick")
                        Dim newelementsi = New XElement(ns + "si", "0")
                        Dim newelementus = New XElement(ns + "us", "0")

                        If e.HasAttributes Then
                            e.RemoveAttributes()
                        End If

                        If e.Value.Trim() = "" Then
                            e.Add(newelementsi)
                            e.Add(newelementus)
                        ElseIf Not e.HasElements Then
                            Dim nodeval = e.Value.Trim()
                            e.RemoveNodes()
                            If jobmeasurementsystem = "US Customary" Then
                                CurrentJob.DesignOptions.MeasurementSystem.Name = "US Customary"
                                newelementsi.Value = nodeval * 24.4
                                newelementus.Value = nodeval
                            Else
                                CurrentJob.DesignOptions.MeasurementSystem.Name = "Metric"
                                newelementsi.Value = nodeval
                                newelementus.Value = nodeval / 24.4
                            End If

                            e.Add(newelementsi)
                            e.Add(newelementus)
                        End If
                    End If

                    If e.Name.LocalName = "TrafficMixName" Then
                        If e.HasAttributes Then
                            e.RemoveAttributes()
                        End If
                        If e.Value.Trim() = "" Then
                            'e.Add(vbEmpty)

                        Else
                            ImportedMixName = e.Value
                            MixListName = e.Value
                        End If
                    End If

                Next

                Using ms As MemoryStream = New MemoryStream()
                    Dim xws As XmlWriterSettings = New XmlWriterSettings()
                    xws.OmitXmlDeclaration = True
                    xws.Indent = True

                    Using xw As XmlWriter = XmlWriter.Create(ms, xws)
                        Dim doc As XDocument = New XDocument(xmlElem)
                        doc.Save(stream)
                    End Using
                End Using
                stream.Position = 0

                Try
                    faarFieldJob = DirectCast(ser.ReadObject(stream), FaarFieldJob)
                    If faarFieldJob.DesignOptions.MeasurementSystem Is Nothing Then
                        If jobmeasurementsystem = "US Customary" Then
                            faarFieldJob.DesignOptions.MeasurementSystem = FaarFieldFactory.CreateUsCustomary()
                        Else
                            faarFieldJob.DesignOptions.MeasurementSystem = FaarFieldFactory.CreateMetric()
                        End If
                    End If
                    If faarFieldJob.DesignOptions.CrackPropogation Is Nothing Then
                        faarFieldJob.DesignOptions.CrackPropogation = FaarFieldFactory.CreateThickness(1, FaarFieldFactory.CreateUsCustomary())
                    End If
                    If faarFieldJob.JobInformation.FrostDepth Is Nothing Then
                        faarFieldJob.JobInformation.FrostDepth = FrostDepthReading
                    End If
                    If faarFieldJob.DesignOptions.SlabStress = True Then
                        SlabOptionsJobLoad = True
                    End If
                Catch ex As Exception
                    corruptFlag = True
                    MessageBox.Show("Selected file is not valid.")
                    Debug.WriteLine(ex.Message)
                    Exit Sub
                End Try

                If Jobs.Count = 1 Then
                    For i = 0 To CurrentJob.Sections.Count - 1
                        If CurrentJob.Sections.Item(i).Layers.Count > 0 Then
                            jobchecker = True
                        End If
                    Next
                    If jobchecker = False Then
                        Jobs.Remove(Jobs.Item(0))

                    End If
                End If

                'The following code is to initialize the validation code.
                faarFieldJob.DesignOptions = FaarFieldFactory.CreateDesignOptions(FaarFieldFactory, faarFieldJob.DesignOptions)
                faarFieldJob.JobInformation = FaarFieldFactory.CreateJobInformation(FaarFieldFactory, faarFieldJob.JobInformation)
                If faarFieldJob.DesignOptions.SlabStress <> SlabOptionsJobLoad Then
                    faarFieldJob.DesignOptions.SlabStress = SlabOptionsJobLoad
                End If
                Dim jobView = New JobViewModel(FaarFieldFactory, faarFieldJob, Me)
                jobView.SavePath = FilePath
                Jobs.Add(jobView)

                Try

                    SetCurrentJob(jobView)
                    corruptFlag = False
                Catch ex As Exception
                    corruptFlag = True
                    MessageBox.Show("Job did not complete loading, file appears to be corrupt.")
                    Debug.WriteLine(ex)
                    Jobs.Remove(jobView)
                    If Jobs.Count = 0 Then
                        JobRequiredButtonEnabled = False
                        SectionRequiredButtonEnabled = False
                        JobInformationIsHidden = True
                        DesignOptionsIsHidden = True
                        SummaryReportIsHidden = True
                        SectionIsHidden = True
                        InnerSectionIsHidden = True
                        AirplanesIsHidden = True
                        NotesIsHidden = True
                        SectionReportIsHidden = True
                        CDFGraphIsHidden = True
                        DetailedReportIsHidden = True
                        PCRGraphIsHidden = True
                        PCRReportIsHidden = True
                        AirportMasterRecordIsHidden = True

                        VisibleForCreate = Visibility.Hidden
                        OnPropertyChanged(NameOf(VisibleForCreate))
                        VisibleForEdit = Visibility.Hidden
                        VehicleEditIsHidden = True

                        OnPropertyChanged(NameOf(VisibleForEdit))

                        OnPropertyChanged(NameOf(CurrentJob))


                        OnPropertyChanged(NameOf(Jobs))
                        OnPropertyChanged(NameOf(SectionReportIsHidden))
                        OnPropertyChanged(NameOf(JobInformationIsHidden))
                        OnPropertyChanged(NameOf(SummaryReportIsHidden))
                        OnPropertyChanged(NameOf(InnerSectionIsHidden))
                        OnPropertyChanged(NameOf(AirplanesIsHidden))
                        OnPropertyChanged(NameOf(CDFGraphIsHidden))
                        OnPropertyChanged(NameOf(DetailedReportIsHidden))
                        OnPropertyChanged(NameOf(PCRGraphIsHidden))
                        OnPropertyChanged(NameOf(PCRReportIsHidden))
                        OnPropertyChanged(NameOf(AirportMasterRecordIsHidden))

                        'Removed for Crash Bugs 13458 and 13459
                        'Call OnUpdateselectedMaterial()

                    Else
                        Dim ejob = CType(Jobs(0), JobViewModel)
                        SetCurrentJob(ejob)
                    End If

                    Exit Sub
                End Try
                SetCurrentSection(jobView.SectionFolder.Children.Item(0))

                Dim storeLayers As New ObservableCollection(Of IMaterial)
                Dim numberoflayers2 As Integer = 0
                If Not CurrentSectionView Is Nothing Then
                    For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                        storeLayers.Add(CurrentSectionView.Section.Layers.Item(i))
                        numberoflayers2 += 1
                    Next
                End If

                CurrentSectionView.IsSelected = True
                SectionIsHidden = False
                InnerSectionIsHidden = False
                AirplanesIsHidden = False
                NotesIsHidden = False
                JobRequiredButtonEnabled = True
                SectionRequiredButtonEnabled = True

                OnPropertyChanged(NameOf(CurrentLayers))
                OnPropertyChanged(NameOf(Jobs))

            ElseIf FilePath.Contains(".JOB.xml") Then

                'Dim XMLDoc As New XmlDataDocument
                Dim XMLDoc As New XmlDocument
                Dim XMLNode As XmlNodeList
                Dim job = FaarFieldFactory.CreateFaarFieldJob(FaarFieldFactory, "")
                Dim lastSlashIndex = FilePath.LastIndexOf(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
                Dim jobName = FilePath.Substring(lastSlashIndex + 1)

                job.Name = jobName.Replace(".JOB.xml", "(V 1.42)")

                Jobs.Add(New JobViewModel(FaarFieldFactory, job, Me))
                Dim job1 = CType(Jobs(Jobs.Count - 1), JobViewModel)
                SetCurrentJob(job1)

                Using fs As New FileStream(FilePath, FileMode.Open, FileAccess.Read)
                    XMLDoc.Load(fs)
                End Using

                XMLNode = XMLDoc.GetElementsByTagName("SectionInfo")

                For i = 0 To XMLNode.Count - 1
                    Dim section = FaarFieldFactory.CreateSection(FaarFieldFactory)
                    section.Name = XMLNode(i).ChildNodes.Item(0).InnerText.Trim()
                    CurrentSectionView = CurrentJobView.SectionFolder.AddSection(section)
                    CurrentJob.Sections.Add(section)
                    CurrentSectionView.Section.Life = XMLNode(i).ChildNodes.Item(4).InnerText.Trim()
                    If XMLNode(i).ChildNodes.Item(5).InnerText.Trim() <> 0 Then
                        CurrentSectionView.Section.Sci = XMLNode(i).ChildNodes.Item(5).InnerText.Trim()
                    End If

                    CurrentSectionView.Section.PercentCdfu = XMLNode(i).ChildNodes.Item(6).InnerText.Trim()
                    Dim NumberOfLayers As Integer = XMLNode(i).ChildNodes.Item(7).InnerText.Trim()
                    DesignLife = CurrentSectionView.Section.Life

                    Sci = CurrentSectionView.Section.Sci
                    Cdfu = CurrentSectionView.Section.PercentCdfu
                    NewThickness = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary())
                    Dim LCode As Integer
                    Dim K As Integer = 0
                    Dim L As Integer
                    If XMLNode(i).ChildNodes.Item(9).Name.ToString = "RDEC_Model_Info" Then
                        L = 10
                    Else
                        L = 9
                    End If
                    For J = L To L + NumberOfLayers - 1

                        LCode = XMLNode(i).ChildNodes.Item(J).ChildNodes(3).InnerText.Trim()
                        Call InsertLayer(LCode, K)
                        If K = NumberOfLayers - 1 Then
                            CurrentSectionView.Section.Layers.Item(K).Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
                        Else
                            CurrentSectionView.Section.Layers.Item(K).Thickness = FaarFieldFactory.CreateThickness(XMLNode(i).ChildNodes.Item(J).ChildNodes(0).InnerText.Trim(), FaarFieldFactory.CreateUsCustomary())
                        End If
                        If CurrentSectionView.Section.Layers.Item(K).Category = "P-501 PCC" Then
                            CurrentSectionView.Section.Layers.Item(K).Rupture = FaarFieldFactory.CreateModulus(XMLNode(i).ChildNodes.Item(J).ChildNodes(1).InnerText.Trim(), FaarFieldFactory.CreateUsCustomary())
                        Else
                            CurrentSectionView.Section.Layers.Item(K).Modulus = FaarFieldFactory.CreateModulus(XMLNode(i).ChildNodes.Item(J).ChildNodes(1).InnerText.Trim(), FaarFieldFactory.CreateUsCustomary())
                        End If
                        K = K + 1
                    Next

                    Dim Modul As Double
                    Modul = CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Modulus.UsCustomary
                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Thickness = FaarFieldFactory.CreateThickness(12, FaarFieldFactory.CreateUsCustomary())
                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).SubgradeReaction = FaarFieldFactory.CreateSubgradeReaction((Modul / 20.15) ^ (1 / 1.28405), FaarFieldFactory.CreateUsCustomary())
                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Cbr = Modul / 1500
                    SetDesignComboBox(CurrentSectionView)
                    _analysisType = Nothing
                    _analysisType = CurrentSectionView.Section.AnalysisType
                    OnUpdateAnalysisType()
                    K = 0

                    For J = L + NumberOfLayers To XMLNode(i).ChildNodes.Count - 1

                        For Each aircraft In FullAircraftLibrary
                            If aircraft.Name = XMLNode(i).ChildNodes.Item(J).ChildNodes(0).InnerText.Trim().ToString Then

                                Dim addedAirplane = FaarFieldFactory.CreateAircraft(aircraft, FaarFieldFactory)

                                CurrentSectionView.Section.Airplanes.Add(addedAirplane)
                                CurrentSectionView.Section.Airplanes.Item(K).DefaultGrossWeight = aircraft.GrossWeight
                                CurrentSectionView.Section.Airplanes.Item(K).DefaultCp = aircraft.Cp
                                CurrentSectionView.Section.Airplanes.Item(K).GrossWeight = FaarFieldFactory.CreateWeight(XMLNode(i).ChildNodes.Item(J).ChildNodes(1).InnerText.Trim(), FaarFieldFactory.CreateUsCustomary())
                                CurrentSectionView.Section.Airplanes.Item(K).NumberDepartures = XMLNode(i).ChildNodes.Item(J).ChildNodes(2).InnerText.Trim()
                                CurrentSectionView.Section.Airplanes.Item(K).AnnualGrowth = (XMLNode(i).ChildNodes.Item(J).ChildNodes(3).InnerText.Trim()) * 100
                                CurrentSectionView.Section.Airplanes.Item(K).CDFGraphData = New List(Of Single)
                                For JJ = 1 To 42
                                    CurrentSectionView.Section.Airplanes.Item(K).CDFGraphData.Add(0)
                                Next
                                K = K + 1

                            End If

                        Next


                    Next
                    If K < XMLNode(i).ChildNodes.Count - 1 - (L + NumberOfLayers) Then
                        Dim NOA As Integer
                        NOA = (XMLNode(i).ChildNodes.Count - 1 - (L + NumberOfLayers) - K) + 1
                        MessageBox.Show(NOA.ToString + " aircraft removed from the structure number " + (i + 1).ToString + " of the job file")
                    End If
                    Call SelectDesignedLayer()

                Next

                Dim storeLayers As New ObservableCollection(Of IMaterial)
                Dim numberoflayers2 As Integer = 0
                If Not CurrentSectionView Is Nothing Then
                    For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                        storeLayers.Add(CurrentSectionView.Section.Layers.Item(i))
                        numberoflayers2 += 1
                    Next
                End If

                CurrentLayers.Clear()
                CurrentLayers.Clear()
                CurrentSectionView.Section.Layers.Clear()
                For i = 0 To numberoflayers2 - 1
                    CurrentSectionView.Section.Layers.Add(storeLayers(i))
                Next

                For Each layer In CurrentSectionView.Section.Layers
                    CurrentLayers.Add(layer)
                Next

                CurrentSectionView.IsSelected = True
                SectionIsHidden = False
                InnerSectionIsHidden = False
                AirplanesIsHidden = False
                NotesIsHidden = False
                JobRequiredButtonEnabled = True
                SectionRequiredButtonEnabled = True

                OnPropertyChanged(NameOf(CurrentLayers))
                OnPropertyChanged(NameOf(Jobs))

                CurrentJob.Version_1_4_File = True
                JobViewBorderThickness = 5

            Else
                MessageBox.Show("The selected file is not a job file." + vbNewLine)
            End If

            _selectedtree = 0
            OnPropertyChanged(NameOf(SelectedTree))
            OnUpdateselectedMaterial()
            If CurrentAirplanes.Count = 0 Then
                DeleteTrafficEnabled = False
            Else
                DeleteTrafficEnabled = True
            End If
            DesignLifeChecker = True
            OnPropertyChanged(NameOf(DeleteTrafficEnabled))

            CheckTrafficMixImport()

            RefreshGWPercent()

            For Each Section As Object In CurrentJob.Sections
                CheckTrafficCount(False)
            Next

            'Set up the slab stresses if the saved job has them enabled in the design options
            If SlabOptionsJobLoad Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Then
                    Dim jobSlabEdge = CurrentSectionView.Section.SlabEdgeStressArray(1)
                    Dim jobSlabInterior = CurrentSectionView.Section.SlabInteriorStressArray(1)
                    Dim jobCriticalStressAircraft As String = "None"
                    If CurrentSectionView.Section.LastRun = "PCR" Then
                        CurrentSectionView.Section.SlabEdgeStress = CurrentSectionView.Section.SlabEdgeStressArray(1)
                        CurrentSectionView.Section.SlabInteriorStress = CurrentSectionView.Section.SlabInteriorStressArray(1)
                        CurrentSectionView.Section.CriticalStressAicraft = CurrentAirplanes.Item(0).Name

                    Else
                        For i = 1 To CurrentAirplanes.Count
                            If CurrentSectionView.Section.SlabEdgeStressArray(i) > jobSlabEdge Then
                                jobSlabEdge = CurrentSectionView.Section.SlabEdgeStressArray(i)
                                jobSlabInterior = CurrentSectionView.Section.SlabInteriorStressArray(i)
                                jobCriticalStressAircraft = CurrentAirplanes.Item(i - 1).Name
                            End If
                        Next

                        CurrentSectionView.Section.SlabEdgeStress = jobSlabEdge
                        CurrentSectionView.Section.SlabInteriorStress = jobSlabInterior
                        CurrentSectionView.Section.CriticalStressAicraft = jobCriticalStressAircraft
                        CriticalStressAircraft = CurrentSectionView.Section.CriticalStressAicraft
                        _criticalAircraftReport = CurrentSectionView.Section.CriticalStressAicraft
                    End If

                    OnPropertyChanged(NameOf(SlabEdgeStress))
                    OnPropertyChanged(NameOf(SlabInteriorStress))
                    OnPropertyChanged(NameOf(CriticalStressAircraft))
                    OnPropertyChanged(NameOf(CriticalStressReport))
                    _criticalStressAircraft = Nothing
                End If

            End If

        End Sub


        Private Sub CheckTrafficCount(deleteExcess As Boolean)
            If CurrentAirplanes.Count > 40 Then
                If deleteExcess Then
                    MsgBox("Traffic is limited to 40 aircraft in the mix")
                    For i = CurrentAirplanes.Count - 1 To 40 Step -1
                        If i >= 40 Then
                            CurrentAirplanes.Remove(CurrentAirplanes(i))
                        End If
                    Next
                Else
                    MsgBox("It is not recommended to use more than 40 aircraft in the mix")
                End If
            End If
        End Sub


        Private Sub CheckTrafficMixImport()
            Dim mixmatch(,) As String

            'sectionindex,{ 0 = name, 1 = exists, 2 = matches, 3 = firstinstance }
            ReDim mixmatch(CurrentJob.Sections.Count - 1, 3)

            For i = 0 To CurrentJob.Sections.Count - 1
                If CurrentJob.Sections(i).TrafficMixName IsNot Nothing AndAlso CurrentJob.Sections(i).TrafficMixName <> "" Then
                    mixmatch(i, 0) = CurrentJob.Sections(i).TrafficMixName

                    If CheckTrafficMixExists(CurrentJob.Sections(i).TrafficMixName) Then
                        mixmatch(i, 1) = True
                        If CheckTrafficMixBasicMatch(CurrentJob.Sections(i).TrafficMixName, CurrentJob.Sections(i).Airplanes) Then
                            mixmatch(i, 2) = True
                        Else
                            mixmatch(i, 2) = False
                        End If
                    Else
                        mixmatch(i, 1) = False
                    End If

                    If i > 0 Then
                        Dim first = True
                        For j = i To 0 Step -1
                            If CurrentJob.Sections(i).TrafficMixName = CurrentJob.Sections(j).TrafficMixName Then
                                first = False
                            End If
                        Next
                        mixmatch(i, 3) = first
                    Else
                        mixmatch(i, 3) = True
                    End If

                End If
            Next

            For i = 0 To CurrentJob.Sections.Count - 1

                ' traffic mix used in job file
                If mixmatch(i, 0) IsNot Nothing AndAlso mixmatch(i, 0) <> "" Then

                    ' only prompt first instance
                    If mixmatch(i, 3) = True Then

                        ' traffic mix exists 
                        If mixmatch(i, 1) = True Then

                            ' traffic mix does not match
                            If mixmatch(i, 2) = False Then

                                ' job file uses traffic mix file that does not match version in users directory
                                Dim result = MessageBox.Show("This job file is using a traffic mix file(" + mixmatch(i, 0) + ") that does not match the version found in your traffic library." + vbCrLf + vbCrLf + "Do you want to overwrite your local traffic mix file(" + mixmatch(i, 0) + ") with new traffic?", "Saved File Does Not Match", MessageBoxButton.YesNo)

                                If result = MessageBoxResult.Yes Then

                                    Dim traffic As New ObservableCollection(Of AirplaneInfo)
                                    For Each ap In CurrentJob.Sections(i).Airplanes
                                        traffic.Add(ap)
                                    Next

                                    SetVersions(LibraryVersion, SoftwareVersion)
                                    SaveMix(mixmatch(i, 0), traffic)

                                Else
                                    For Each sec In CurrentJob.Sections
                                        If sec.TrafficMixName = mixmatch(i, 0) Then
                                            'Ignore this traffic mix selection for all instances in this job file
                                            sec.TrafficMixName = Nothing
                                        End If
                                    Next
                                End If

                            End If

                        Else

                            ' job file uses traffic mix file that is not in users traffic library directory
                            Dim result = MessageBox.Show("This job file is using a traffic mix file(" + mixmatch(i, 0) + ") that is not found in your traffic library." + vbCrLf + vbCrLf + "Would you like to save a copy of this traffic mix file to your traffic library?", "Traffic Mix Not Found", MessageBoxButton.YesNo)

                            If result = MessageBoxResult.Yes Then

                                Dim traffic As New ObservableCollection(Of AirplaneInfo)
                                For Each ap In CurrentJob.Sections(i).Airplanes
                                    traffic.Add(ap)
                                Next

                                SetVersions(LibraryVersion, SoftwareVersion)
                                SaveMix(mixmatch(i, 0), traffic)
                                TrafficLibrary.Add(mixmatch(i, 0))
                                OnPropertyChanged(NameOf(CurrentLibrary))

                            Else
                                For Each sec In CurrentJob.Sections
                                    If sec.TrafficMixName = mixmatch(i, 0) Then
                                        'Ignore this traffic mix selection for all instances in this job file
                                        sec.TrafficMixName = Nothing
                                    End If
                                Next
                            End If

                        End If

                    End If

                End If

            Next

        End Sub


        Private Function CheckTrafficMixExists(MixListName As String) As Boolean
            Dim MixPath = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "TrafficLibrary" & Path.DirectorySeparatorChar & MixListName & ".xml"
            Dim MixExists As Boolean = False
            If System.IO.File.Exists(MixPath) Then
                MixExists = True
            End If
            Return MixExists
        End Function


        Private Function CheckTrafficMixBasicMatch(MixListName As String, trafficList As List(Of IAirplaneInfo)) As Boolean
            Dim MixPath = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "TrafficLibrary" & Path.DirectorySeparatorChar & MixListName & ".xml"
            Dim IsMatch As Boolean = True

            Dim traffic As New ObservableCollection(Of AirplaneInfo)
            For Each ap In trafficList
                traffic.Add(ap)
            Next

            If System.IO.File.Exists(MixPath) Then
                Dim ExistingTrafficLibrary = LoadTrafficLibrary(MixListName)

                If ExistingTrafficLibrary.Count = traffic.Count Then
                    For i = 0 To ExistingTrafficLibrary.Count - 1
                        If ExistingTrafficLibrary(i).Name <> traffic(i).Name Then
                            IsMatch = False
                            Exit For
                        End If
                        If ExistingTrafficLibrary(i).NumberDepartures <> traffic(i).NumberDepartures Then
                            IsMatch = False
                            Exit For
                        End If
                        If ExistingTrafficLibrary(i).GrossWeight.UsCustomary <> traffic(i).GrossWeight.UsCustomary Then
                            IsMatch = False
                            Exit For
                        End If
                        If ExistingTrafficLibrary(i).AnnualGrowth <> traffic(i).AnnualGrowth Then
                            IsMatch = False
                            Exit For
                        End If
                        If ExistingTrafficLibrary(i).TotalDepartures <> traffic(i).TotalDepartures Then
                            IsMatch = False
                            Exit For
                        End If
                    Next
                Else
                    IsMatch = False
                End If
            Else
                IsMatch = False
            End If

            Return IsMatch

        End Function


        Private Shared Function OpenJobEnabled(context As Object) As Boolean
            Return True
        End Function


        Sub InsertLayer(ByVal LCode As Integer, ByVal K As Integer)

            Dim UserDefined = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "User Defined"), False)
            Dim Subgrade = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "Subgrade"), False)
            Dim uncrushed = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-154 Uncrushed Aggregate"), True)
            Dim P208 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-208 Crushed Aggregate"), True)
            Dim P209 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-209 Crushed Aggregate"), True)
            Dim P211 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-211 Lime Rock"), True)
            Dim P219 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-219 Recycled Concrete Aggregate"), True)
            Dim flexible = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-401/P-403 HMA Surface"), False)
            Dim overlay = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-401/P-403 HMA Overlay"), False)
            Dim rigid = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Surface"), False)
            Dim P501OverlayUn = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Overlay (unbonded)"), False)
            Dim P501OverlayFl = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Overlay on Flexible"), False)
            Dim P301 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-301 Soil Cement Base"), True)
            Dim P304 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-304 Cement Treated Base"), True)
            Dim P306 = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-306 Lean Concrete"), True)
            Dim stabilized = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-401/P-403 HMA Stabilized"), True)
            Dim VariableFlex = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "Variable (flexible)"), True)
            Dim VariableRigid = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "Variable (rigid)"), True)
            Dim P501OverlayPB = FaarFieldFactory.CreateMaterial(FaarFieldFactory, FindMaterial(Materials, "P-501 PCC Overlay (partially bonded)"), False)

            If LCode = 1 Then
                CurrentSectionView.Section.Layers.Insert(K, flexible)
            ElseIf LCode = 10 Then
                CurrentSectionView.Section.Layers.Insert(K, overlay)
            ElseIf LCode = 5 Then
                CurrentSectionView.Section.Layers.Insert(K, rigid)
            ElseIf LCode = 11 Then
                CurrentSectionView.Section.Layers.Insert(K, P501OverlayUn)
            ElseIf LCode = 12 Then
                CurrentSectionView.Section.Layers.Insert(K, P501OverlayPB)
            ElseIf LCode = 13 Then
                CurrentSectionView.Section.Layers.Insert(K, P501OverlayFl)
            ElseIf LCode = 15 Then
                CurrentSectionView.Section.Layers.Insert(K, P301)
            ElseIf LCode = 16 Then
                CurrentSectionView.Section.Layers.Insert(K, P304)
            ElseIf LCode = 17 Then
                CurrentSectionView.Section.Layers.Insert(K, P306)
            ElseIf LCode = 14 Or LCode = 20 Then
                CurrentSectionView.Section.Layers.Insert(K, stabilized)
            ElseIf LCode = 9 Then
                CurrentSectionView.Section.Layers.Insert(K, VariableFlex)
            ElseIf LCode = 7 Then
                CurrentSectionView.Section.Layers.Insert(K, VariableRigid)
            ElseIf LCode = 8 Then
                CurrentSectionView.Section.Layers.Insert(K, uncrushed)
            ElseIf LCode = 18 Then
                CurrentSectionView.Section.Layers.Insert(K, P208)
            ElseIf LCode = 6 Then
                CurrentSectionView.Section.Layers.Insert(K, P209)
            ElseIf LCode = 21 Then
                CurrentSectionView.Section.Layers.Insert(K, P211)
            ElseIf LCode = 19 Then
                CurrentSectionView.Section.Layers.Insert(K, P219)
            ElseIf LCode = 0 Then
                CurrentSectionView.Section.Layers.Insert(K, UserDefined)
            ElseIf LCode = 4 Then
                CurrentSectionView.Section.Layers.Insert(K, Subgrade)
            End If

        End Sub


        Private Function GetNextSectionName() As String

            Dim i = CurrentJob.Sections.Count
            Dim possibleName As String

            If CurrentJob.Sections.Count = 0 Then
                i = 1
                possibleName = "New Structure" + " " + i.ToString()

                Return possibleName
            End If

            Dim uniqueName = True
            i = CurrentJob.Sections.Count + 1
            possibleName = "New Structure " + i.ToString()

            While uniqueName

                If CurrentJob.Sections.Count > 0 Then
                    For Each Section As Object In CurrentJob.Sections
                        If Section.Name = possibleName Then
                            uniqueName = False
                        End If
                    Next
                    uniqueName = False
                Else
                    uniqueName = False
                End If

            End While

            Return possibleName
        End Function


        Private Sub SectionNew(context As Object)

            SectionIsHidden = False
            InnerSectionIsHidden = False
            AirplanesIsHidden = False

            bAddNewSection = True       'true means we are adding new section
            Dim jobcounter As Integer = 0
            If CurrentJob Is Nothing Then
                jobcounter = 1
                If Jobs.Count = 0 Then
                    NewJob(Nothing)
                End If
                Dim job = CType(Jobs(0), JobViewModel)
                SetCurrentJob(job)
            ElseIf Jobs.Count = 0 Then
                jobcounter = 1
                If Jobs.Count = 0 Then
                    NewJob(Nothing)
                End If
                Dim job = CType(Jobs(0), JobViewModel)
                SetCurrentJob(job)

            End If
            Dim job1 = CType(Jobs(0), JobViewModel)

            If jobcounter <> 1 Then
                Dim section = FaarFieldFactory.CreateSection(FaarFieldFactory)
                section.Name = GetNextSectionName()
                section.Layers.Clear()
                CurrentSectionView = Nothing
                CurrentSectionView = CurrentJobView.SectionFolder.AddSection(section)
                CurrentSectionView.Section.Layers.Clear()
                CurrentJob.Sections.Add(section)
                CurrentLayers.Clear()
                DrawProfile()
                DesignLife = CurrentSectionView.Section.Life
                SlabInteriorStress = CurrentSectionView.Section.SlabInteriorStress
                SlabEdgeStress = CurrentSectionView.Section.SlabEdgeStress
                Sci = CurrentSectionView.Section.Sci

                Cdfu = CurrentSectionView.Section.PercentCdfu
                CurrentSectionView.IsSelected = True

                _sectionIsHidden = False
                _InnerSectionIsHidden = False
                _airplanesIsHidden = False
                _notesIsHidden = False
                OnPropertyChanged(NameOf(SectionIsHidden))
                OnPropertyChanged(NameOf(AirplanesIsHidden))
                OnPropertyChanged(NameOf(NotesIsHidden))
                jobcounter = 0
                _sectionIsHidden = False
                _InnerSectionIsHidden = False
                _airplanesIsHidden = False
                _notesIsHidden = False
                _selectedtree = 0
                OnPropertyChanged(NameOf(SelectedTree))
                JobRequiredButtonEnabled = True
                SectionRequiredButtonEnabled = True
                OnPropertyChanged(NameOf(SectionIsHidden))
                OnPropertyChanged(NameOf(AirplanesIsHidden))
                OnPropertyChanged(NameOf(NotesIsHidden))
                OnPropertyChanged(NameOf(Jobs))
            End If

        End Sub


        Public Sub OnDesignOptions(jobView As JobViewModel)
            DesignOptionsIsHidden = False
            SetCurrentJob(jobView)
        End Sub


        Public Sub OnVehicleEdit()
            VehicleEditIsHidden = False

        End Sub


        Public Sub OnJobInformation(jobView As JobViewModel)
            JobInformationIsHidden = True
            JobInformationIsHidden = False
        End Sub


        Private Shared Function SectionNewEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Sub SaveJob(context As Object)

            If CurrentJob.Sections.Count = 0 Then
                MessageBox.Show("At least one structure should be defined to save the job. ")
                Exit Sub
            End If

            For Each Section As Object In CurrentJob.Sections

                If Section.AnalysisType Is Nothing Then
                    MessageBox.Show("Pavement Type should be defined for each structure to save the job. ")
                    Exit Sub
                End If

            Next

            For Each Section As Object In CurrentJob.Sections

                If Section.Airplanes.Count = 0 Then
                    MessageBox.Show("Each structure on the job needs to have at least one aircraft on traffic list to save the job.")
                    Exit Sub
                End If

            Next

            Dim jCount As Integer = Jobs.Count
            Dim ejob = CType(Jobs(0), JobViewModel)

            Dim jobname As String = Nothing
            Dim fpath As String = Nothing
            Dim check As Thickness
            check = CurrentSectionView.Section.Layers.Item(0).Thickness
            Dim jobnum As Integer = 0
            For i = 0 To jCount - 1
                ejob = CType(Jobs(i), JobViewModel)

                If CurrentJob.Name = ejob.Name Then
                    jobname = ejob.Name
                    fpath = ejob.SavePath
                End If

                'Return values to original library values before save
                For Each sec In ejob.FaarFieldJob.Sections
                    For Each ac In sec.Airplanes
                        If ac.DataStorage IsNot Nothing Then
                            ac.MgPercent = ac.DataStorage(0)
                            ac.MgPercentPCN = ac.DataStorage(1)
                            ac.RunMgPercent = ac.DataStorage(2)
                        End If
                    Next
                Next

            Next

            If jobname Is Nothing Then
                jobname = CurrentJob.Name
            End If

            If jobname.Contains("V 1.42") Then
                MessageBox.Show("The current job was created by FAARFIELD 1.42. " + vbNewLine + "It will be saved in the FAARFIELD v2.0 job file format.")
                jobname = jobname.Replace("(V 1.42)", "")
            End If
            SavePath = fpath

            Dim saveFile As New System.Windows.Forms.SaveFileDialog
            saveFile.DefaultExt = "jobx"
            saveFile.Filter = "FAARFIELD 2.1 Files|*.jobx|All Files|*.*"
            saveFile.FileName = jobname
            If SavePath Is Nothing Then
                saveFile.InitialDirectory = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"
            Else
                saveFile.FileName = SavePath
            End If

            If SavePath IsNot Nothing Then
                SavePath = saveFile.FileName
                Dim saveFileName = GetJobNameFromFile(SavePath)

                CurrentJob.Name = saveFileName

                Dim ser = New DataContractSerializer(GetType(FaarFieldJob))

                Dim writer = New FileStream(SavePath, FileMode.Create)
                ser.WriteObject(writer, CurrentJob)
                writer.Close()
                ValidateJobName(Nothing)
                InsertVersionInfo(SavePath)
                ShowToast("Job saved successfully.")
            Else
                If saveFile.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                    SavePath = saveFile.FileName
                    Dim saveFileName = GetJobNameFromFile(SavePath)
                    CurrentJob.Name = saveFileName

                    ejob.SavePath = SavePath    'xyzzy

                    Dim ser = New DataContractSerializer(GetType(FaarFieldJob))

                    Dim writer = New FileStream(SavePath, FileMode.Create)
                    ser.WriteObject(writer, CurrentJob)
                    writer.Close()
                    ValidateJobName(Nothing)
                    InsertVersionInfo(SavePath)
                    ShowToast("Job saved as " + GetJobNameFromFile(SavePath))

                    If CurrentJob.Version_1_4_File Then
                        CurrentJob.Version_1_4_File = False
                        JobViewBorderThickness = 0
                    End If

                End If
            End If

        End Sub


        Private Function GetJobNameFromFile(path As String) As String
            Dim lastSlashIndex = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal)
            Dim jobName = path.Substring(lastSlashIndex + 1)
            jobName = jobName.Replace(".jobx", "")
            Return jobName
        End Function


        Private Shared Function SaveJobEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Sub SaveAsJob(context As Object)

            If CurrentJob.Sections.Count = 0 Then
                MessageBox.Show("At least one structure should be defined to save the job. ")
                Exit Sub
            End If

            For Each Section As Object In CurrentJob.Sections

                If Section.AnalysisType Is Nothing Then
                    MessageBox.Show("Pavement Type should be defined for each structure to save the job. ")
                    Exit Sub
                End If

            Next

            For Each Section As Object In CurrentJob.Sections

                If Section.Airplanes.Count = 0 Then
                    MessageBox.Show("Each structure on the job needs to have at least one aircraft on traffic list to save the job.")
                    Exit Sub
                End If

            Next
            Dim saveFile As New System.Windows.Forms.SaveFileDialog
            saveFile.DefaultExt = "jobx"
            saveFile.Filter = "FAARFIELD 2.1 Files|*.jobx|All Files|*.*"
            If SavePath Is Nothing Then
                saveFile.InitialDirectory = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD"
            Else
                saveFile.FileName = SavePath
            End If

            If saveFile.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                SavePath = saveFile.FileName
                Dim saveFileName = GetJobNameFromFile(SavePath)
                CurrentJob.Name = saveFileName

                Dim ser = New DataContractSerializer(GetType(FaarFieldJob))

                Dim writer = New FileStream(SavePath, FileMode.Create)
                ser.WriteObject(writer, CurrentJob)
                writer.Close()
                ValidateJobName(Nothing)
                InsertVersionInfo(SavePath)
                ShowToast("Job saved as " + GetJobNameFromFile(SavePath))

                If CurrentJob.Version_1_4_File Then
                    CurrentJob.Version_1_4_File = False
                    JobViewBorderThickness = 0
                End If

            End If

        End Sub


        Private Sub SaveAllJob(context As Object)

            Dim jobCount As Integer = Jobs.Count    'This gets the Job Count.

            Dim ejob = CType(Jobs(0), JobViewModel)

            Dim jobname As String = Nothing
            Dim fpath As String = Nothing
            fpath = ejob.SavePath

            Dim jobnum As Integer = 0

            If CurrentJob.Sections.Count = 0 Then
                MessageBox.Show("At least one structure should be defined to save the job. ")
                Exit Sub
            End If

            For i = 0 To jobCount - 1
                ejob = CType(Jobs(i), JobViewModel)
                SetCurrentJob(ejob)


                jobname = ejob.Name

                If CurrentJob.Sections.Count = 0 Then
                    MessageBox.Show("At least one structure should be defined for job named " + jobname + " to save the job. ")
                    Exit Sub
                End If


                For Each Section As Object In CurrentJob.Sections

                    If Section.AnalysisType Is Nothing Then
                        MessageBox.Show("Pavement Type should be defined for each structure in the job named " + jobname + " to save the job. ")
                        Exit Sub
                    End If

                Next

                For Each Section As Object In CurrentJob.Sections

                    If Section.Airplanes.Count = 0 Then
                        MessageBox.Show("Each structure on the job named  " + jobname + " needs to have at least one aircraft on traffic list to save the job.")
                        Exit Sub
                    End If

                Next

            Next

            For i = 0 To jobCount - 1
                ejob = CType(Jobs(i), JobViewModel)
                SetCurrentJob(ejob)

                jobname = ejob.Name
                fpath = ejob.SavePath

                If fpath Is Nothing Then
                    Dim saveFile As New System.Windows.Forms.SaveFileDialog

                    saveFile.DefaultExt = "jobx"
                    saveFile.Filter = "FAARFIELD 2.1 Files|*.jobx|All Files|*.*"
                    If saveFile.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                        SavePath = saveFile.FileName
                        Dim saveFName = GetJobNameFromFile(SavePath)
                        CurrentJob.Name = saveFName
                        Dim ser1 = New DataContractSerializer(GetType(FaarFieldJob))

                        Dim writer1 = New FileStream(SavePath, FileMode.Create)
                        ser1.WriteObject(writer1, CurrentJob)
                        writer1.Close()
                        ValidateJobName(Nothing)
                        InsertVersionInfo(SavePath)

                        If CurrentJob.Version_1_4_File Then
                            CurrentJob.Version_1_4_File = False
                            JobViewBorderThickness = 0
                        End If

                    End If

                Else

                    Dim saveFileName = GetJobNameFromFile(fpath)
                    CurrentJob.Name = saveFileName

                    Dim ser = New DataContractSerializer(GetType(FaarFieldJob))

                    Dim writer = New FileStream(ejob.SavePath, FileMode.Create)
                    ser.WriteObject(writer, CurrentJob)
                    writer.Close()
                    ValidateJobName(Nothing)
                    InsertVersionInfo(SavePath)
                End If

            Next

            ShowToast("All jobs saved successfully.")

        End Sub


        Public Sub CloseJob(context As Object)
            Dim ret As Integer, M1 As String

            M1 = "This will close the job " + CurrentJob.Name + ". Do you want To save the job?"

            ret = MsgBox(M1, MsgBoxStyle.YesNoCancel)

            If ret = 7 Then
                Jobs.Remove(CurrentJobView)

                SavePath = Nothing      'ep

                OnPropertyChanged(NameOf(Jobs))

                If Jobs.Count = 0 Then
                    JobRequiredButtonEnabled = False
                    SectionRequiredButtonEnabled = False
                    JobInformationIsHidden = True
                    DesignOptionsIsHidden = True
                    SummaryReportIsHidden = True
                    SectionIsHidden = True
                    InnerSectionIsHidden = True
                    AirplanesIsHidden = True
                    NotesIsHidden = True
                    SectionReportIsHidden = True
                    CDFGraphIsHidden = True
                    DetailedReportIsHidden = True
                    PCRGraphIsHidden = True
                    PCRReportIsHidden = True
                    AirportMasterRecordIsHidden = True

                    VisibleForCreate = Visibility.Hidden
                    OnPropertyChanged(NameOf(VisibleForCreate))
                    VisibleForEdit = Visibility.Hidden
                    VehicleEditIsHidden = True

                    OnPropertyChanged(NameOf(VisibleForEdit))
                    OnPropertyChanged(NameOf(CurrentJob))
                    OnPropertyChanged(NameOf(Jobs))
                    OnPropertyChanged(NameOf(SectionReportIsHidden))
                    OnPropertyChanged(NameOf(JobInformationIsHidden))
                    OnPropertyChanged(NameOf(SummaryReportIsHidden))
                    OnPropertyChanged(NameOf(InnerSectionIsHidden))
                    OnPropertyChanged(NameOf(AirplanesIsHidden))
                    OnPropertyChanged(NameOf(CDFGraphIsHidden))
                    OnPropertyChanged(NameOf(DetailedReportIsHidden))
                    OnPropertyChanged(NameOf(PCRGraphIsHidden))
                    OnPropertyChanged(NameOf(PCRReportIsHidden))
                    OnPropertyChanged(NameOf(AirportMasterRecordIsHidden))

                    'Removed for Crash Bugs 13458 and 13459
                    'Call OnUpdateselectedMaterial()

                Else
                    Dim ejob = CType(Jobs(0), JobViewModel)
                    SetCurrentJob(ejob)
                End If
            End If
            If ret = 6 Then
                Call SaveJob(CurrentJob)
                Call SaveJobEnabled(CurrentJob)

                Jobs.Remove(CurrentJobView)

                SavePath = Nothing      'ep

                OnPropertyChanged(NameOf(Jobs))

                If Jobs.Count = 0 Then
                    JobRequiredButtonEnabled = False
                    SectionRequiredButtonEnabled = False
                    _jobInformationIsHidden = True
                    _designOptionsIsHidden = True
                    _summaryReportIsHidden = True
                    _sectionIsHidden = True
                    _InnerSectionIsHidden = True
                    _airplanesIsHidden = True
                    _notesIsHidden = True
                    _sectionReportIsHidden = True
                    _CDFGraphIsHidden = True
                    _DetailedReportIsHidden = True
                    _PCRGraphIsHidden = True
                    _PCRReportIsHidden = True
                    _AirportMasterRecordIsHidden = True
                    OnPropertyChanged(NameOf(Jobs))
                    OnPropertyChanged(NameOf(SectionReportIsHidden))
                    OnPropertyChanged(NameOf(JobInformationIsHidden))
                    OnPropertyChanged(NameOf(SummaryReportIsHidden))
                    OnPropertyChanged(NameOf(SectionIsHidden))
                    OnPropertyChanged(NameOf(AirplanesIsHidden))
                    OnPropertyChanged(NameOf(AirplanesIsHidden))
                    OnPropertyChanged(NameOf(CDFGraphIsHidden))
                    OnPropertyChanged(NameOf(DetailedReportIsHidden))
                    OnPropertyChanged(NameOf(PCRGraphIsHidden))
                    OnPropertyChanged(NameOf(PCRReportIsHidden))
                    OnPropertyChanged(NameOf(AirportMasterRecordIsHidden))
                Else
                    Dim ejob = CType(Jobs(0), JobViewModel)
                    SetCurrentJob(ejob)
                End If
            End If

            OnPropertyChanged(NameOf(Jobs))

        End Sub


        Public Sub DeleteJob(jobView As JobViewModel)
            SetCurrentJob(jobView)
            If jobView.SavePath = "" Then
                If MessageBox.Show("Deleting a Job will remove it from your disk.  Continue?", "Delete Job", System.Windows.Forms.MessageBoxButtons.YesNo) = MessageBoxResult.Yes Then
                    CloseJob(Nothing)
                    If Not jobView.SavePath Is Nothing Then
                        File.Delete(jobView.SavePath)
                    End If
                Else
                    Exit Sub
                End If
            Else
                CloseJob(jobView.SavePath)
                File.Delete(jobView.SavePath)
            End If
        End Sub


        Public Sub PasteSection(sectionFolderView As SectionFolderViewModel)
            If Not _clonedSection Is Nothing Then

                CopiedSectionNumber = 0

                For i = 0 To CurrentJob.Sections.Count - 1
                    If _clonedSection.Name = CurrentJob.Sections.Item(i).Name Then
                        If Right(_clonedSection.Name, 1) = "y" Then
                            _clonedSection.Name = _clonedSection.Name + (CopiedSectionNumber + 1).ToString
                        Else
                            CopiedSectionNumber = Convert.ToInt32(Right(_clonedSection.Name, 1))
                            _clonedSection.Name = _clonedSection.Name.Substring(0, _clonedSection.Name.Length - 1)
                            _clonedSection.Name = _clonedSection.Name + (CopiedSectionNumber + 1).ToString
                        End If

                    End If
                Next

                sectionFolderView.AddSection(_clonedSection)

                Dim jobview = CType(sectionFolderView.Parent, JobViewModel)
                jobview.FaarFieldJob.Sections.Add(_clonedSection)

            End If

        End Sub


        Public Sub CloneSection(sectionView As SectionViewModel)
            CopiedSectionNumber = 0
            Dim copychecker As Boolean = False

            _clonedSection = FaarFieldFactory.CreateSection(FaarFieldFactory, sectionView.Section)

            For i = 0 To sectionView.Section.Layers.Count - 1
                _clonedSection.Layers(i).DesignedLayer = sectionView.Section.Layers(i).DesignedLayer
            Next

            Try
                If _clonedSection.Name.Contains("copy") Then
                    Try
                        CopiedSectionNumber = Convert.ToInt32(Right(_clonedSection.Name, 1))
                        copychecker = True
                    Catch ex As Exception
                        CopiedSectionNumber = 1
                    End Try

                End If
            Catch ex As Exception

            End Try

            If CopiedSectionNumber <= 9 Then
                If CopiedSectionNumber <> 0 Then
                    If copychecker = True Then
                        _clonedSection.Name = _clonedSection.Name.Substring(0, _clonedSection.Name.Length - 1)
                        _clonedSection.Name = _clonedSection.Name + (CopiedSectionNumber + 1).ToString
                    Else
                        _clonedSection.Name = _clonedSection.Name + CopiedSectionNumber.ToString
                    End If
                Else
                    _clonedSection.Name = _clonedSection.Name + " copy"
                End If
            Else
                _clonedSection.Name = _clonedSection.Name + " copy"
            End If

            For i = 0 To CurrentJob.Sections.Count - 1
                If _clonedSection.Name = CurrentJob.Sections.Item(i).Name Then
                    If Right(_clonedSection.Name, 1) = "y" Then
                        _clonedSection.Name = _clonedSection.Name + (CopiedSectionNumber + 1).ToString
                    Else
                        CopiedSectionNumber = Convert.ToInt32(Right(_clonedSection.Name, 1))
                        _clonedSection.Name = _clonedSection.Name.Substring(0, _clonedSection.Name.Length - 1)
                        _clonedSection.Name = _clonedSection.Name + (CopiedSectionNumber + 1).ToString
                    End If
                End If
            Next

            If CurrentLibraryIndex = -1 Then

                _clonedSection.ClonedAirplanes = New List(Of IAirplaneInfo)(_clonedSection.Airplanes)

                'Keep value of CDF, CDF max, P/C ratio CDF and P/C ratio should not to be zero after copy and paste a calculated section for new aircraft list
                For h = 0 To sectionView.Section.Airplanes.Count - 1
                    _clonedSection.ClonedAirplanes.Item(h).Cdf = sectionView.Section.Airplanes.Item(h).Cdf
                    _clonedSection.ClonedAirplanes.Item(h).CdfAircraftMax = sectionView.Section.Airplanes.Item(h).CdfAircraftMax
                    _clonedSection.ClonedAirplanes.Item(h).CtoP = sectionView.Section.Airplanes.Item(h).CtoP
                Next
            Else
                _clonedSection.TrafficMixName = sectionView.Section.TrafficMixName

                'Keep value of CDF, CDF max, P/C ratio CDF and P/C ratio should not to be zero after copy and paste a calculated section for new aircraft list
                For h = 0 To sectionView.Section.Airplanes.Count - 1
                    _clonedSection.Airplanes.Item(h).Cdf = sectionView.Section.Airplanes.Item(h).Cdf
                    _clonedSection.Airplanes.Item(h).CdfAircraftMax = sectionView.Section.Airplanes.Item(h).CdfAircraftMax
                    _clonedSection.Airplanes.Item(h).CtoP = sectionView.Section.Airplanes.Item(h).CtoP
                Next
            End If

            _clonedSection.SectionCDF = New List(Of Single)
            For p = 0 To 42
                _clonedSection.SectionCDF.Add(sectionView.Section.SectionCDF(p))

            Next

            For j = 0 To sectionView.Section.Airplanes.Count - 1
                For h = 0 To 41
                    _clonedSection.Airplanes.Item(j).CDFGraphData(h) = sectionView.Section.Airplanes.Item(j).CDFGraphData(h)
                Next
            Next

            _clonedSection.CriticalStressAircraft = sectionView.Section.CriticalStressAicraft
            _clonedSection.SavedPCRhtml = sectionView.Section.SavedPCRhtml
            _clonedSection.SavedPCRgraph = sectionView.Section.SavedPCRgraph
            _clonedSection.SavedAirportMasterRecordhtml = sectionView.Section.SavedAirportMasterRecordhtml
            _clonedSection.SavedCDFgraph = sectionView.Section.SavedCDFgraph
            _clonedSection.SavedDetailedReportHtml = sectionView.Section.SavedDetailedReportHtml
            _clonedSection.SavedSectionReportHtml = sectionView.Section.SavedSectionReportHtml

        End Sub


        Public Sub CloneJob(jobView As JobViewModel)
            Dim clonedJob = FaarFieldFactory.CreateFaarFieldJob(FaarFieldFactory, jobView.FaarFieldJob)
            clonedJob.Name = clonedJob.Name + " copy"

            Dim jobCount As Integer = Jobs.Count

            Dim ejob = CType(Jobs(0), JobViewModel)
            For i = 0 To jobCount - 1
                ejob = CType(Jobs(i), JobViewModel)
                If ejob.Name = clonedJob.Name Then
                    clonedJob.Name = clonedJob.Name + " copy"

                End If
            Next

            For j = 0 To jobView.SectionFolder.Children.Count - 1
                For i = 0 To jobView.FaarFieldJob.Sections(j).Layers.Count - 1
                    clonedJob.Sections(j).Layers(i).DesignedLayer = jobView.FaarFieldJob.Sections(j).Layers(i).DesignedLayer
                Next

                clonedJob.Sections(j).TrafficMixName = jobView.FaarFieldJob.Sections(j).TrafficMixName

                clonedJob.Sections(j).SectionCDF = New List(Of Single)
                For i = 0 To 42
                    clonedJob.Sections(j).SectionCDF.Add(jobView.FaarFieldJob.Sections(j).SectionCDF(i))
                Next

                For h = 0 To clonedJob.Sections(j).Airplanes.Count - 1
                    For i = 0 To 41
                        clonedJob.Sections(j).Airplanes.Item(h).CDFGraphData(i) = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).CDFGraphData(i)
                    Next
                    clonedJob.Sections(j).Airplanes.Item(h).AnnualGrowth = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).AnnualGrowth
                    clonedJob.Sections(j).Airplanes.Item(h).NumberDepartures = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).NumberDepartures
                    clonedJob.Sections(j).Airplanes.Item(h).TotalDepartures = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).TotalDepartures
                    clonedJob.Sections(j).Airplanes.Item(h).Cdf = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).Cdf
                    clonedJob.Sections(j).Airplanes.Item(h).CdfAircraftMax = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).CdfAircraftMax
                    clonedJob.Sections(j).Airplanes.Item(h).CtoP = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).CtoP
                    clonedJob.Sections(j).Airplanes.Item(h).ACRB = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRB
                    clonedJob.Sections(j).Airplanes.Item(h).ACRB1 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRB1
                    clonedJob.Sections(j).Airplanes.Item(h).ACRB2 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRB2
                    clonedJob.Sections(j).Airplanes.Item(h).ACRB3 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRB3
                    clonedJob.Sections(j).Airplanes.Item(h).ACRB4 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRB4
                    clonedJob.Sections(j).Airplanes.Item(h).ACRThick = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRThick
                    clonedJob.Sections(j).Airplanes.Item(h).ACRThick1 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRThick1
                    clonedJob.Sections(j).Airplanes.Item(h).ACRThick2 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRThick2
                    clonedJob.Sections(j).Airplanes.Item(h).ACRThick3 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRThick3
                    clonedJob.Sections(j).Airplanes.Item(h).ACRThick4 = jobView.FaarFieldJob.Sections(j).Airplanes.Item(h).ACRThick4
                Next
            Next

            Dim clonedJobView = New JobViewModel(FaarFieldFactory, clonedJob, Me)
            Jobs.Add(clonedJobView)
        End Sub


        Private Shared Function CloseJobEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property OnImportTraffic As ICommand
            Get
                Return New DelegateCommand(AddressOf ImportTraffic, AddressOf ImportTrafficEnabled)
            End Get
        End Property


        Private Shared Sub ImportTraffic(context As Object)
            System.Windows.MessageBox.Show("Import Traffic")
        End Sub


        Private Shared Function ImportTrafficEnabled(context As Object) As Boolean
            Return True
        End Function


        'Open User Defined Aircraft folder by user's chosen directory
        Public ReadOnly Property OnOpenUserDefinedAircraftfolder_Command As ICommand
            Get
                Return New DelegateCommand(AddressOf OpenUserDefinedAircraftfolder, AddressOf InitializedEnabled)
            End Get
        End Property


        'Open UDA directory and load UDAs
        Private Sub OpenUserDefinedAircraftfolder()

            If UDAFolderPath IsNot Nothing Then

                Dim userDefinedLibrary As List(Of AirplaneInfo) = GetUserDefinedAircrafts(FaarFieldFactory, UDAFolderPath)
                Dim userDefinedLibraryCount As Integer = userDefinedLibrary.Count
                Dim countAircraftLibrary As Integer = AircraftLibrary.Count

                If userDefinedLibrary IsNot Nothing Then
                    AircraftLibrary.RemoveRange(countAircraftLibrary - userDefinedLibraryCount, userDefinedLibraryCount)
                End If

                OpenUserDefinedAircraftFiles()

                Dim userDefinedLibrary1 As List(Of AirplaneInfo) = GetUserDefinedAircrafts(FaarFieldFactory, UDAFolderPath)
                addUDAtoExternalLibrary(userDefinedLibrary1, UDAFolderPath)

            Else
                OpenUserDefinedAircraftFiles()

                Dim userDefinedLibrary2 As List(Of AirplaneInfo) = GetUserDefinedAircrafts(FaarFieldFactory, UDAFolderPath)
                addUDAtoExternalLibrary(userDefinedLibrary2, UDAFolderPath)

            End If
            OnPropertyChanged(NameOf(AirplaneGroup))
            OnPropertyChanged(NameOf(ListExternalAircraft))

        End Sub


        'Open Folder Browser Window 
        Public Sub OpenUserDefinedAircraftFiles()
            Dim openFolder As New Forms.FolderBrowserDialog
            If openFolder.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                openUDAfolder = True
                UDAFolderPath = openFolder.SelectedPath
            Else
                openUDAfolder = False
            End If
            OnPropertyChanged(NameOf(UDAFolderPath))
        End Sub


        'Add UDA to External Library after changing the new directory
        Public Sub addUDAtoExternalLibrary(userDefinedLibrary As List(Of AirplaneInfo), UDAfolderPath As String)
            ListExternalAircraft = New ObservableCollection(Of AirplaneInfo)
            userDefinedLibrary = GetUserDefinedAircrafts(FaarFieldFactory, UDAfolderPath)
            Dim aircraftManufacturer = userDefinedLibrary.GroupBy(Function(x) x.Manufacturer).Select(Function(x) x.First).ToList

            If userDefinedLibrary IsNot Nothing Then
                If AirplaneGroup.Count = 9 Then
                    For Each airplaneInfo As AirplaneInfo In aircraftManufacturer
                        AirplaneGroup.Add(New AirplaneGroupViewModel(airplaneInfo, Me))
                    Next
                End If

                For Each airplane In userDefinedLibrary
                    ListExternalAircraft.Add(airplane)
                    AircraftLibrary.Add(airplane)
                Next

                For i = 0 To AirplaneGroup.Count - 1
                    Dim tmpName = TryCast(AirplaneGroup.Item(i), AirplaneGroupViewModel).Manufacturer
                    If tmpName = "External Library" Then
                        AirplaneGroup.Item(i).IsSelected = True
                    End If
                Next

            End If
        End Sub


        Public ReadOnly Property OnSaveEdittedUserDefinedAircraft As ICommand
            Get
                Return New DelegateCommand(AddressOf SaveEdittedUserDefinedAircraft, AddressOf SaveEdittedUserDefinedAircraftEnabled)
            End Get
        End Property


        Public Sub SaveEdittedUserDefinedAircraft(context As Object)

            If CurrentUserDefinedAircraft Is Nothing Then
                MessageBox.Show("User Defined Aircraft has not been selected.")
                Exit Sub
            End If

            If _CurrentWheel.Count = 0 Then
                MessageBox.Show("At least one set of tire coordinates should be defined")
                Exit Sub
            End If

            If CurrentEval.Count = 0 Then
                MessageBox.Show("At least one set of evaluation point should be defined")
                Exit Sub
            End If

            Dim coordinateSystem = FaarFieldFactory.CreateUsCustomary()

            If UDName Is Nothing Then
                MessageBox.Show("Fill the New User Defined Aircraft box with the aircraft name")
            End If

            If UDGrossWeight.UsCustomary <= 0 Then
                MessageBox.Show("Aircraft gross weight can not be less than 0 ")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.GrossWeight = _UDGrossWeight
            End If
            If _UDMGPercent <= 0 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            ElseIf _UDMGPercent > 1 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.MgPercent = _UDMGPercent
            End If

            If _UDMGPercentPCN < 0 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            ElseIf _UDMGPercentPCN > 1 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.MgPercentPCN = _UDMGPercentPCN ' / 2 Adjust in save after wheels are mirrored
            End If

            _CurrentUserDefinedAircraft.NumberWheels = _CurrentWheel.Count
            _CurrentUserDefinedAircraft.NumberGear = 13
            _CurrentUserDefinedAircraft.Gear = "X"
            If _UDTirePressure.UsCustomary <= 0 Then
                MessageBox.Show("Tire pressure can not be less than 0 ")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.Cp = _UDTirePressure
            End If

            _CurrentUserDefinedAircraft.GearOrientation = UDGearOrientation
            _CurrentUserDefinedAircraft.Tt = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
            _CurrentUserDefinedAircraft.B = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
            _CurrentUserDefinedAircraft.Tg = 0
            _CurrentUserDefinedAircraft.Tv = 0
            _CurrentUserDefinedAircraft.Ts = 0

            _CurrentUserDefinedAircraft.NumberDepartures = 1200
            _CurrentUserDefinedAircraft.TotalDepartures = 24000

            CurrentUserDefinedAircraft.WheelCoordinates.Clear()

            Dim tmpWheels As New ObservableCollection(Of UserDefinedWheel)

            For i = 0 To CurrentWheel.Count - 1
                Dim udWheel As New UserDefinedWheel With {
                    .TireX = FaarFieldFactory.CreateThickness(CurrentWheel.Item(i).TireX.GetValue(coordinateSystem), coordinateSystem),
                    .TireY = FaarFieldFactory.CreateThickness(CurrentWheel.Item(i).TireY.GetValue(coordinateSystem), coordinateSystem)
                }
                tmpWheels.Add(udWheel)
            Next

            For i = 0 To tmpWheels.Count - 1
                Dim exists As Boolean = False

                For j = i + 1 To tmpWheels.Count - 1
                    If (tmpWheels(i).TireX.UsCustomary = tmpWheels(j).TireX.UsCustomary AndAlso tmpWheels(i).TireY.UsCustomary = tmpWheels(j).TireY.UsCustomary) OrElse (tmpWheels(i).TireX.UsCustomary = tmpWheels(j).TireX.UsCustomary * -1 AndAlso tmpWheels(i).TireY.UsCustomary = tmpWheels(j).TireY.UsCustomary) Then
                        exists = True
                    End If
                Next

                If Not exists Then
                    CurrentUserDefinedAircraft.WheelCoordinates.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpWheels.Item(i).TireX.UsCustomary) * -1, coordinateSystem), FaarFieldFactory.CreateLength(tmpWheels.Item(i).TireY.UsCustomary, coordinateSystem)))
                End If

            Next

            CurrentUserDefinedAircraft.EvaluationPoints.Clear()

            Dim tmpEvals As New ObservableCollection(Of UserDefinedEvalPoints)

            For i = 0 To CurrentEval.Count - 1
                Dim udEval As New UserDefinedEvalPoints With {
                    .EvalX = FaarFieldFactory.CreateThickness(CurrentEval.Item(i).EvalX.GetValue(coordinateSystem), coordinateSystem),
                    .EvalY = FaarFieldFactory.CreateThickness(CurrentEval.Item(i).EvalY.GetValue(coordinateSystem), coordinateSystem)
                }
                tmpEvals.Add(udEval)
            Next

            For i = 0 To tmpEvals.Count - 1
                Dim exists As Boolean = False

                For j = i + 1 To tmpEvals.Count - 1
                    If (tmpEvals(i).EvalX.UsCustomary = tmpEvals(j).EvalX.UsCustomary AndAlso tmpEvals(i).EvalY.UsCustomary = tmpEvals(j).EvalY.UsCustomary) OrElse (tmpEvals(i).EvalX.UsCustomary = tmpEvals(j).EvalX.UsCustomary * -1 AndAlso tmpEvals(i).EvalY.UsCustomary = tmpEvals(j).EvalY.UsCustomary) Then
                        exists = True
                    End If
                Next

                If Not exists Then
                    CurrentUserDefinedAircraft.EvaluationPoints.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpEvals.Item(i).EvalX.UsCustomary) * -1, coordinateSystem), FaarFieldFactory.CreateLength(tmpEvals.Item(i).EvalY.UsCustomary, coordinateSystem)))
                End If

            Next

            MirrorUDAWheels(CurrentUserDefinedAircraft)

            OnPropertyChanged(NameOf(ListExternalAircraft))
            OnPropertyChanged(NameOf(AircraftLibrary))

            For i = 0 To AirplaneGroup.Count - 1
                Dim tmpName = TryCast(AirplaneGroup.Item(i), AirplaneGroupViewModel).Manufacturer
                If tmpName = "External Library" Then
                    AirplaneGroup.Item(i).IsSelected = True
                End If
            Next

            SaveNewUserDefinedfile(_CurrentUserDefinedAircraft)

            'Reload Edit Window with updated UDA
            CurrentUserDefinedAircraft = CurrentUserDefinedAircraft

        End Sub


        Public Sub ClearUserDefinedPage()
            _UDGrossWeight = FaarFieldFactory.CreateWeight(0, FaarFieldFactory.CreateUsCustomary)
            _UDMGPercent = 0
            _UDMGPercentPCN = 0
            _UDNumberOfEvalPoints = 0
            _UDNumberOfWheels = 0
            _UDTs = 0
            _UDName = ""
            _UDTirePressure = FaarFieldFactory.CreatePressure(0, FaarFieldFactory.CreateUsCustomary)
            UDGearOrientation = 0
            _UDGearNumber = 13
            _UDGeartype = "V"
            _CurrentWheel.Clear()
            _CurrentEval.Clear()

            _CurrentUserDefinedAircraft = Nothing

            OnPropertyChanged(NameOf(UDGrossWeight))
            OnPropertyChanged(NameOf(UDName))
            OnPropertyChanged(NameOf(UDTs))
            OnPropertyChanged(NameOf(UDGearType))
            OnPropertyChanged(NameOf(UDGearNumber))
            OnPropertyChanged(NameOf(UDTirePressure))
            OnPropertyChanged(NameOf(UDGearOrientation))
            OnPropertyChanged(NameOf(UDMGPercent))
            OnPropertyChanged(NameOf(UDMGPercentPCN))
            OnPropertyChanged(NameOf(UDNumberOfEvalPoints))
            OnPropertyChanged(NameOf(UDNumberOfWheels))
            OnPropertyChanged(NameOf(CurrentWheel))
            OnPropertyChanged(NameOf(CurrentEval))
            OnPropertyChanged(NameOf(CurrentUserDefinedAircraft))
            UserDefinedGearImage = DrawUserDefinedGear()
        End Sub


        Private Shared Function SaveEdittedUserDefinedAircraftEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property OnDeleteUserDefinedAircraft As ICommand
            Get
                Return New DelegateCommand(AddressOf DeleteUserDefinedAircraftCommand, AddressOf DeleteUserDefinedAircraftEnabled)
            End Get
        End Property


        Public Sub DeleteUserDefinedAircraftCommand(context As Object)

            If CurrentUserDefinedAircraft Is Nothing Then
                MessageBox.Show("No User Defined Aircraft is selected.")
                Exit Sub
            End If

            Dim aircraftName As String = CurrentUserDefinedAircraft.Name

            Dim msgBoxResult As MessageBoxResult = MessageBox.Show(
                "Are you sure you want to delete '" & aircraftName & "'?" & vbCrLf & vbCrLf &
                "The aircraft file will be permanently removed from disk. " &
                "Any saved job files that already contain this aircraft will not be affected.",
                "Delete User Defined Aircraft",
                MessageBoxButton.YesNo)

            If msgBoxResult <> MessageBoxResult.Yes Then
                Exit Sub
            End If

            ' Delete the XML file from disk
            Libs.AircraftLibrary.DeleteUserDefinedAircraft(aircraftName, UDAFolderPath)

            ' Remove from in-memory collections
            Dim toRemove = ListExternalAircraft.FirstOrDefault(Function(a) a.Name = aircraftName)
            If toRemove IsNot Nothing Then
                ListExternalAircraft.Remove(toRemove)
            End If

            Dim toRemoveLib = AircraftLibrary.FirstOrDefault(Function(a) a.Name = aircraftName)
            If toRemoveLib IsNot Nothing Then
                AircraftLibrary.Remove(toRemoveLib)
            End If

            Dim toRemoveFull = FullAircraftLibrary.FirstOrDefault(Function(a) a.Name = aircraftName)
            If toRemoveFull IsNot Nothing Then
                FullAircraftLibrary.Remove(toRemoveFull)
            End If

            ' If no more UDVs remain, remove the External Library group from the tree
            If ListExternalAircraft.Count = 0 Then
                For i = AirplaneGroup.Count - 1 To 0 Step -1
                    Dim tmpName = TryCast(AirplaneGroup.Item(i), AirplaneGroupViewModel)
                    If tmpName IsNot Nothing AndAlso tmpName.Manufacturer = "External Library" Then
                        AirplaneGroup.RemoveAt(i)
                    End If
                Next
            End If

            OnPropertyChanged(NameOf(ListExternalAircraft))
            OnPropertyChanged(NameOf(AircraftLibrary))
            OnPropertyChanged(NameOf(AirplaneGroup))

            ' Clear the edit form
            ClearUserDefinedPage()

        End Sub


        Private Shared Function DeleteUserDefinedAircraftEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property UpdateUDAGearImage_Command As ICommand
            Get
                Return New DelegateCommand(AddressOf UpdateUDAGearImage, AddressOf UpdateUDAGearImageEnabled)
            End Get
        End Property


        Public Sub UpdateUDAGearImage(context As Object)
            UserDefinedGearImage = DrawUserDefinedGear()
        End Sub


        Public Function UpdateUDAGearImageEnabled()
            Return True
        End Function


        Public ReadOnly Property OnSaveNewUserDefinedAircraft As ICommand
            Get
                Return New DelegateCommand(AddressOf SaveNewUserDefinedAircraft, AddressOf SaveNewUserDefinedAircraftEnabled)
            End Get
        End Property


        Public Sub SaveNewUserDefinedAircraft(context As Object)
            Dim coordinateSystem = FaarFieldFactory.CreateUsCustomary()
            Dim Namechecker As Boolean = True
            Dim _CurrentUserDefinedAircraft As New AirplaneInfo

            If _UDName Is Nothing Then
                MessageBox.Show("Fill the New User Defined Aircraft box with the aircraft name")
                Exit Sub
            End If
            For i = 0 To ListExternalAircraft.Count - 1
                If ListExternalAircraft.Item(i).Name = _UDName + "(UDA)" Then
                    Namechecker = False
                End If
            Next
            If Namechecker = False Then
                MessageBox.Show("The same aircraft name can not be used twice")
                Exit Sub
            End If

            _CurrentUserDefinedAircraft.Manufacturer = "External Library"
            _CurrentUserDefinedAircraft.Name = _UDName + "(UDA)"
            If _UDGrossWeight.UsCustomary <= 0 Then
                MessageBox.Show("Aircraft gross weight can not be less than 0 ")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.GrossWeight = _UDGrossWeight
            End If
            If _UDMGPercent <= 0 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            ElseIf _UDMGPercent > 1 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.MgPercent = _UDMGPercent
            End If

            If _UDMGPercentPCN < 0 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            ElseIf _UDMGPercentPCN > 1 Then
                MessageBox.Show("Aircraft percent gross weight should be a positive number between 0 and 1")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.MgPercentPCN = _UDMGPercentPCN ' / 2 Adjust in save after wheels are mirrored
            End If

            _CurrentUserDefinedAircraft.NumberWheels = _CurrentWheel.Count
            _CurrentUserDefinedAircraft.NumberGear = 13
            _CurrentUserDefinedAircraft.Gear = "X"
            If _UDTirePressure.UsCustomary <= 0 Then
                MessageBox.Show("Tire pressure can not be less than 0 ")
                Exit Sub
            Else
                _CurrentUserDefinedAircraft.Cp = _UDTirePressure
            End If

            _CurrentUserDefinedAircraft.GearOrientation = UDGearOrientation
            _CurrentUserDefinedAircraft.Tt = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
            _CurrentUserDefinedAircraft.B = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
            _CurrentUserDefinedAircraft.Tg = 0
            _CurrentUserDefinedAircraft.Tv = 0
            _CurrentUserDefinedAircraft.Ts = 0

            _CurrentUserDefinedAircraft.NumberDepartures = 1200
            _CurrentUserDefinedAircraft.TotalDepartures = 24000

            _CurrentUserDefinedAircraft.WheelCoordinates = New List(Of ICoordinates)
            _CurrentUserDefinedAircraft.EvaluationPoints = New List(Of ICoordinates)

            _CurrentUserDefinedAircraft.WheelCoordinates.Clear()

            Dim tmpWheels As New ObservableCollection(Of UserDefinedWheel)

            For i = 0 To CurrentWheel.Count - 1
                Dim udWheel As New UserDefinedWheel With {
                    .TireX = FaarFieldFactory.CreateThickness(CurrentWheel.Item(i).TireX.GetValue(coordinateSystem), coordinateSystem),
                    .TireY = FaarFieldFactory.CreateThickness(CurrentWheel.Item(i).TireY.GetValue(coordinateSystem), coordinateSystem)
                }
                tmpWheels.Add(udWheel)
            Next

            For i = 0 To tmpWheels.Count - 1
                Dim exists As Boolean = False

                For j = i + 1 To tmpWheels.Count - 1
                    If (tmpWheels(i).TireX.UsCustomary = tmpWheels(j).TireX.UsCustomary AndAlso tmpWheels(i).TireY.UsCustomary = tmpWheels(j).TireY.UsCustomary) OrElse (tmpWheels(i).TireX.UsCustomary = tmpWheels(j).TireX.UsCustomary * -1 AndAlso tmpWheels(i).TireY.UsCustomary = tmpWheels(j).TireY.UsCustomary) Then
                        exists = True
                    End If
                Next

                If Not exists Then
                    _CurrentUserDefinedAircraft.WheelCoordinates.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpWheels.Item(i).TireX.UsCustomary) * -1, coordinateSystem), FaarFieldFactory.CreateLength(tmpWheels.Item(i).TireY.UsCustomary, coordinateSystem)))
                End If

            Next

            _CurrentUserDefinedAircraft.EvaluationPoints.Clear()

            Dim tmpEvals As New ObservableCollection(Of UserDefinedEvalPoints)

            For i = 0 To CurrentEval.Count - 1
                Dim udEval As New UserDefinedEvalPoints With {
                    .EvalX = FaarFieldFactory.CreateThickness(CurrentEval.Item(i).EvalX.GetValue(coordinateSystem), coordinateSystem),
                    .EvalY = FaarFieldFactory.CreateThickness(CurrentEval.Item(i).EvalY.GetValue(coordinateSystem), coordinateSystem)
                }
                tmpEvals.Add(udEval)
            Next

            For i = 0 To tmpEvals.Count - 1
                Dim exists As Boolean = False

                For j = i + 1 To tmpEvals.Count - 1
                    If (tmpEvals(i).EvalX.UsCustomary = tmpEvals(j).EvalX.UsCustomary AndAlso tmpEvals(i).EvalY.UsCustomary = tmpEvals(j).EvalY.UsCustomary) OrElse (tmpEvals(i).EvalX.UsCustomary = tmpEvals(j).EvalX.UsCustomary * -1 AndAlso tmpEvals(i).EvalY.UsCustomary = tmpEvals(j).EvalY.UsCustomary) Then
                        exists = True
                    End If
                Next

                If Not exists Then
                    _CurrentUserDefinedAircraft.EvaluationPoints.Add(FaarFieldFactory.CreateCoordinates(FaarFieldFactory.CreateLength(Math.Abs(tmpEvals.Item(i).EvalX.UsCustomary) * -1, coordinateSystem), FaarFieldFactory.CreateLength(tmpEvals.Item(i).EvalY.UsCustomary, coordinateSystem)))
                End If

            Next

            MirrorUDAWheels(_CurrentUserDefinedAircraft)

            SaveNewUserDefinedfile(_CurrentUserDefinedAircraft)

            Dim userDefinedLibrary As List(Of AirplaneInfo) = GetUserDefinedAircrafts(FaarFieldFactory, UDAFolderPath)
            If AirplaneGroup.Count = 9 Then
                Dim aircraftManufacturer = userDefinedLibrary.GroupBy(Function(x) x.Manufacturer).Select(Function(x) x.First).ToList
                For Each airplaneInfo As AirplaneInfo In aircraftManufacturer
                    AirplaneGroup.Add(New AirplaneGroupViewModel(airplaneInfo, Me))
                Next
            End If

            AircraftLibrary.Add(_CurrentUserDefinedAircraft)

            ListExternalAircraft.Add(_CurrentUserDefinedAircraft)

            OnPropertyChanged(NameOf(ListExternalAircraft))
            OnPropertyChanged(NameOf(AircraftLibrary))

            For i = 0 To AirplaneGroup.Count - 1
                Dim tmpName = TryCast(AirplaneGroup.Item(i), AirplaneGroupViewModel).Manufacturer
                If tmpName = "External Library" Then
                    AirplaneGroup.Item(i).IsSelected = True
                End If
            Next

            Call ClearUserDefinedPage()

            'Change View to Edit Window
            OnEditUserDefined_Click()

            'Load Edit Window with new UDA
            CurrentUserDefinedAircraft = _CurrentUserDefinedAircraft

        End Sub


        Private Shared Function SaveNewUserDefinedAircraftEnabled(context As Object) As Boolean
            Return True
        End Function


        Function SaveNewUserDefinedfile(_CurrentUserDefinedAircraft As IAirplaneInfo)
            If _CurrentUserDefinedAircraft Is Nothing Then
                MessageBox.Show("The New User Defined Aircraft data should be entered To create a New User Defined Aircraft file.")
                Return Nothing
            Else
                _CurrentUserDefinedAircraft.Cdf = 0
                _CurrentUserDefinedAircraft.CdfAircraftMax = 0
                _CurrentUserDefinedAircraft.CtoP = 0
            End If

            Dim saveFilePath As String

            saveFilePath = UDAFolderPath & Path.DirectorySeparatorChar & _CurrentUserDefinedAircraft.Name & ".xml"

            If _CurrentUserDefinedAircraft.WheelCoordinates.Count > 1 Then
                _CurrentUserDefinedAircraft.MgPercentPCN /= 2
            End If

            Try
                SaveNewUserDefinedfile(saveFilePath, _CurrentUserDefinedAircraft)
            Catch ex As Exception

            End Try

            Return UDAFolderPath
        End Function


        Private Sub SaveNewUserDefinedfile(saveFilePath As String, UserDefinedAircraft As AirplaneInfo)
            Dim airplanes = New List(Of AirplaneInfo)()
            airplanes.Add(UserDefinedAircraft)
            Dim ser = New DataContractSerializer(GetType(List(Of AirplaneInfo)))
            Dim writer = New FileStream(saveFilePath, FileMode.Create)
            ser.WriteObject(writer, airplanes)
            writer.Close()

            InsertVersionInfo(saveFilePath)

        End Sub


        Private Sub ValidateDesignOptions(context As Object)

            WarningCdfToleranceHidden = Visibility.Hidden
            WarningLifeToleranceHidden = Visibility.Hidden
            WarningSectionParameterNHidden = Visibility.Hidden
            WarningCrackPropogationHidden = Visibility.Hidden

            Dim options = CType(CurrentJob.DesignOptions, DesignOptions)
            Dim validations = options.Validate(CurrentJob.DesignOptions.MeasurementSystem)
            For Each validation In validations
                Select Case validation.Label
                    Case "MessageCDFTolerance"
                        WarningCdfToleranceHidden = Visibility.Visible
                    Case "MessageLifeTolerance"
                        WarningLifeToleranceHidden = Visibility.Visible
                    Case "MessageNSectionParameter"
                        WarningSectionParameterNHidden = Visibility.Visible
                    Case "MessageCrackRatePropogation"
                        WarningCrackPropogationHidden = Visibility.Visible
                End Select
            Next

            OnPropertyChanged(NameOf(WarningCdfToleranceHidden))
            OnPropertyChanged(NameOf(WarningCrackPropogationHidden))
            OnPropertyChanged(NameOf(WarningLifeToleranceHidden))
            OnPropertyChanged(NameOf(WarningSectionParameterNHidden))

            If CurrentJob.DesignOptions.AllowPartiallyBonded Then
                ' If AllowPartiallyBonded is Yes (in Design Options), then Re-add the 
                ' P-501 PCC Overlay (partially bonded) material to the materials list
                If CategoryP501.Children.Count < 4 Then
                    CategoryP501.Children.Add(PartiallyBondedMaterial)
                    OnPropertyChanged(NameOf(MaterialLibrary))
                End If

            Else

                ' if AllowPartiallyBonded  is No, then remove "P-501 PCC (partially bonded from the materials list
                For Each category In MaterialLibrary
                    'Traverse the materials list
                    For Each material As Object In category.Children
                        ' scan the material names
                        If material.Name.Contains("partially") Then
                            PartiallyBondedMaterial = material
                            CategoryP501 = category
                        End If
                    Next
                Next
                If Not PartiallyBondedMaterial Is Nothing Then
                    ' remove the material
                    CategoryP501.Children.Remove(PartiallyBondedMaterial)

                    OnPropertyChanged(NameOf(MaterialLibrary))
                End If
            End If
        End Sub


        Private Shared Function ValidateDesignOptionsEnabled(context As Object) As Boolean
            Return True
        End Function


#End Region

        Public Property SummaryReportHtml As String
            Get
                Return _summaryReportHtml
            End Get
            Set
                If Value <> _summaryReportHtml Then
                    _summaryReportHtml = Value
                    OnPropertyChanged(NameOf(SummaryReportHtml))
                End If
            End Set
        End Property


        Public Property SectionReportHtml As String
            Get
                Return _sectionReportHtml
            End Get
            Set
                If Value <> _sectionReportHtml Then
                    _sectionReportHtml = Value
                    OnPropertyChanged(NameOf(SectionReportHtml))
                End If
            End Set
        End Property


        Public Property CDFGraphHtml As String
            Get
                Return _CDFGraphHtml
            End Get
            Set
                If Value <> _CDFGraphHtml Then
                    _CDFGraphHtml = Value
                    OnPropertyChanged(NameOf(CDFGraphHtml))
                End If
            End Set
        End Property


        Public Property DetailedReportHtml As String
            Get
                Return _DetailedReportHtml
            End Get
            Set
                If Value <> _DetailedReportHtml Then
                    _DetailedReportHtml = Value
                    OnPropertyChanged(NameOf(DetailedReportHtml))
                End If
            End Set
        End Property


        Public Property PCRGraphHtml As String
            Get
                Return _PCRGraphHtml
            End Get
            Set
                If Value <> _PCRGraphHtml Then
                    _PCRGraphHtml = Value
                    OnPropertyChanged(NameOf(PCRGraphHtml))
                End If
            End Set
        End Property


        Public Property PCRReportHtml As String
            Get
                Return _PCRReportHtml
            End Get
            Set
                If Value <> _PCRReportHtml Then
                    _PCRReportHtml = Value
                    OnPropertyChanged(NameOf(PCRReportHtml))
                End If
            End Set
        End Property


        Public Property AirportMasterRecordHtml As String
            Get
                Return _AirportMasterRecordHtml
            End Get
            Set
                If Value <> _AirportMasterRecordHtml Then
                    _AirportMasterRecordHtml = Value
                    OnPropertyChanged(NameOf(AirportMasterRecordHtml))
                End If
            End Set
        End Property

#Region "RadPane Visiblity"

        Public Property SectionIsHidden As Boolean
            Get
                Return _sectionIsHidden
            End Get
            Set
                If Value <> _sectionIsHidden Then
                    _sectionIsHidden = Value
                    OnPropertyChanged(NameOf(SectionIsHidden))
                End If
            End Set
        End Property


        Private _InnerSectionIsHidden As Boolean = True
        Public Property InnerSectionIsHidden As Boolean
            Get
                Return _InnerSectionIsHidden
            End Get
            Set
                If Value <> _InnerSectionIsHidden Then
                    _InnerSectionIsHidden = Value
                    OnPropertyChanged(NameOf(InnerSectionIsHidden))
                End If
            End Set
        End Property


        Public Property AirplanesIsHidden As Boolean
            Get
                Return _airplanesIsHidden
            End Get
            Set
                If Value <> _airplanesIsHidden Then
                    _airplanesIsHidden = Value
                    OnPropertyChanged(NameOf(AirplanesIsHidden))
                End If
            End Set
        End Property


        Public Property ExplorerIsHidden As Boolean
            Get
                Return _explorerIsHidden
            End Get
            Set
                If Value <> _explorerIsHidden Then
                    _explorerIsHidden = Value
                    OnPropertyChanged(NameOf(ExplorerIsHidden))
                End If
            End Set
        End Property


        Private _CurrentLibraryIndex As Integer = -1
        Public Property CurrentLibraryIndex As Integer
            Get
                Return _CurrentLibraryIndex
            End Get
            Set(value As Integer)
                _CurrentLibraryIndex = value
                OnPropertyChanged(NameOf(CurrentLibraryIndex))
            End Set
        End Property


        Public Property CurrentLibrary As String
            Get
                Return _currentLibrary
            End Get
            Set
                If Not CurrentSectionView Is Nothing Then

                    If Value <> _currentLibrary Then

                        _currentLibrary = Value

                        If Not String.IsNullOrWhiteSpace(Value) Then

                            If _currentLibrary IsNot Nothing Then
                                CurrentAirplanes = LoadTrafficLibrary(Value)
                            End If

                            If CurrentAirplanes Is Nothing Then
                                CurrentAirplanes = New ObservableCollection(Of AirplaneInfo)()
                            End If

                            CurrentSectionView.Section.Airplanes.Clear()
                            For Each airplane In CurrentAirplanes
                                CurrentSectionView.Section.Airplanes.Add(airplane)
                            Next
                            CurrentSectionView.Section.TrafficMixName = Value

                        End If

                        If CurrentAirplanes.Count = 0 Then
                            DeleteTrafficEnabled = False

                            'Hide empty or corrupt library from selection
                            TrafficLibrary.Remove(Value)

                        Else
                            DeleteTrafficEnabled = True
                        End If

                        If Not CurrentLibrary Is Nothing Then
                            DeleteAircraftMixEnabled = True
                        Else
                            DeleteAircraftMixEnabled = False
                        End If

                        CheckTrafficCount(False)

                        OnPropertyChanged(NameOf(CurrentLibrary))

                        RefreshGWPercent()

                    End If

                Else
                    MessageBox.Show("A pavement structure should be defined before adding aircraft")
                End If

            End Set
        End Property


        Public Property DesignOptionsIsHidden As Boolean
            Get
                Return _designOptionsIsHidden
            End Get
            Set
                If Value <> _designOptionsIsHidden Then
                    _designOptionsIsHidden = Value
                    OnPropertyChanged(NameOf(DesignOptionsIsHidden))
                End If
            End Set
        End Property


        Public Property VehicleEditIsHidden As Boolean
            Get
                Return _VehicleEditIsHidden
            End Get
            Set
                If Value <> _VehicleEditIsHidden Then
                    _VehicleEditIsHidden = Value
                    OnPropertyChanged(NameOf(VehicleEditIsHidden))
                End If
            End Set
        End Property


        Public Property LayerThicknessMenuIsHidden As Boolean
            Get
                Return _layerthicknessmenuIsHidden
            End Get
            Set
                If Value <> _layerthicknessmenuIsHidden Then
                    _layerthicknessmenuIsHidden = Value
                    OnPropertyChanged(NameOf(LayerThicknessMenuIsHidden))
                End If
            End Set
        End Property


        Public Property ModulusMenuIsHidden As Boolean
            Get
                Return _modulusmenuIsHidden
            End Get
            Set
                If Value <> _modulusmenuIsHidden Then
                    _modulusmenuIsHidden = Value
                    OnPropertyChanged(NameOf(ModulusMenuIsHidden))
                End If
            End Set
        End Property


        Public Property NotesIsHidden As Boolean
            Get
                Return _notesIsHidden
            End Get
            Set
                If Value <> _notesIsHidden Then
                    '_notesIsHidden = Value     'ep
                    _notesIsHidden = False
                    OnPropertyChanged(NameOf(NotesIsHidden))
                End If
            End Set
        End Property


        Public Property JobInformationIsHidden As Boolean
            Get
                Return _jobInformationIsHidden
            End Get
            Set
                If Value <> _jobInformationIsHidden Then
                    _jobInformationIsHidden = Value
                    OnPropertyChanged(NameOf(JobInformationIsHidden))
                End If
            End Set
        End Property


        Public Property SummaryReportIsHidden As Boolean
            Get
                Return _summaryReportIsHidden
            End Get
            Set
                If Value <> _summaryReportIsHidden Then
                    _summaryReportIsHidden = Value
                    OnPropertyChanged(NameOf(SummaryReportIsHidden))
                End If
            End Set
        End Property


        Public Property JobReportIsHidden As Boolean
            Get
                Return _jobReportIsHidden
            End Get
            Set
                If Value <> _jobReportIsHidden Then
                    _jobReportIsHidden = Value
                    OnPropertyChanged(NameOf(JobReportIsHidden))
                End If
            End Set
        End Property


        Public Property SectionReportIsHidden As Boolean
            Get
                Return _sectionReportIsHidden
            End Get
            Set
                If Value <> _sectionReportIsHidden Then
                    _sectionReportIsHidden = Value
                    OnPropertyChanged(NameOf(SectionReportIsHidden))
                End If
            End Set
        End Property


        Public Property CDFGraphIsHidden As Boolean
            Get
                Return _CDFGraphIsHidden
            End Get
            Set
                If Value <> _CDFGraphIsHidden Then
                    _CDFGraphIsHidden = Value
                    OnPropertyChanged(NameOf(CDFGraphIsHidden))
                End If
            End Set
        End Property


        Public Property DetailedReportIsHidden As Boolean
            Get
                Return _DetailedReportIsHidden
            End Get
            Set
                If Value <> _DetailedReportIsHidden Then
                    _DetailedReportIsHidden = Value
                    OnPropertyChanged(NameOf(DetailedReportIsHidden))
                End If
            End Set
        End Property


        Public Property PCRGraphIsHidden As Boolean
            Get
                Return _PCRGraphIsHidden
            End Get
            Set
                If Value <> _PCRGraphIsHidden Then
                    _PCRGraphIsHidden = Value
                    OnPropertyChanged(NameOf(PCRGraphIsHidden))
                End If
            End Set
        End Property


        Public Property PCRReportIsHidden As Boolean
            Get
                Return _PCRReportIsHidden
            End Get
            Set
                If Value <> _PCRReportIsHidden Then
                    _PCRReportIsHidden = Value
                    OnPropertyChanged(NameOf(PCRReportIsHidden))
                End If
            End Set
        End Property


        Public Property AirportMasterRecordIsHidden As Boolean
            Get
                Return _AirportMasterRecordIsHidden
            End Get
            Set
                If Value <> _AirportMasterRecordIsHidden Then
                    _AirportMasterRecordIsHidden = Value
                    OnPropertyChanged(NameOf(AirportMasterRecordIsHidden))
                End If
            End Set
        End Property


        Public Property DeleteTrafficEnabled As Boolean
            Get
                Return _DeleteTrafficEnabled
            End Get
            Set
                If Value <> _DeleteTrafficEnabled Then
                    _DeleteTrafficEnabled = Value
                    OnPropertyChanged(NameOf(DeleteTrafficEnabled))
                End If

                CheckRunAvailable()

            End Set
        End Property


        Public Property DeleteAircraftMixEnabled As Boolean
            Get
                Return _DeleteAircraftMixEnabled
            End Get
            Set
                If Value <> _DeleteAircraftMixEnabled Then
                    _DeleteAircraftMixEnabled = Value
                    OnPropertyChanged(NameOf(DeleteAircraftMixEnabled))
                End If
            End Set
        End Property


        Public Property SelectDesignLayerEnabled As Boolean
            Get
                Return _SelectDesignLayerEnabled
            End Get
            Set
                If Value <> _SelectDesignLayerEnabled Then
                    _SelectDesignLayerEnabled = Value
                    OnPropertyChanged(NameOf(SelectDesignLayerEnabled))
                End If
            End Set
        End Property


        Public Property StructureErrorMessage As String
            Get
                Return _structureErrorMessage
            End Get
            Set
                If Value <> _structureErrorMessage Then
                    _structureErrorMessage = Value
                    If _structureErrorMessage.Length > 0 Then
                        WarningStructureError = Visibility.Visible
                    Else
                        WarningStructureError = Visibility.Hidden
                    End If
                    OnPropertyChanged(NameOf(StructureErrorMessage))
                End If
            End Set
        End Property


        Public Property DesignLife As Integer
            Get
                Return _designLife
            End Get
            Set(value As Integer)
                If value <> _designLife Then
                    _designLife = value
                    CurrentSectionView.Section.Life = _designLife
                    OnPropertyChanged(NameOf(DesignLife))
                    If DesignLifeChecker = True Then
                        ResetCalculatedAirplaneValues()
                    End If

                    CalculatedLife = Nothing

                End If
            End Set
        End Property


        Public ReadOnly Property NewModulusEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnNewModulusEnter, AddressOf OnNewModulusEnterEnabled)
            End Get
        End Property


        Public Sub OnNewModulusEnter(context As Object)
            If CurrentJob.DesignOptions.MeasurementSystem.GetType Is GetType(UsCustomary) Then

                NewModulus = FaarFieldFactory.CreateModulus(context, FaarFieldFactory.CreateUsCustomary)
            Else
                NewModulus = FaarFieldFactory.CreateThickness(context, FaarFieldFactory.CreateMetric)
            End If

            material.Modulus = NewModulus

            If material.Modulus.UsCustomary <> NewModulus.UsCustomary Then
                NewModulus = material.Modulus
            End If

            If material.CBRActive Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                    If NCHRPFormula Then
                        NewCBR = (NewModulus.UsCustomary / 2555) ^ (1 / 0.64)
                    Else
                        NewCBR = NewModulus.UsCustomary / 1500
                    End If
                End If
            End If

            If material.KValueActive Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                    If PCAConversionFormula Then
                        NewKvalue = FaarFieldFactory.CreateSubgradeReaction((0.8155 * (NewModulus.UsCustomary ^ 0.5719)), New UsCustomary)
                    Else
                        NewKvalue = FaarFieldFactory.CreateSubgradeReaction(Math.Pow(NewModulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                    End If

                End If
            End If

            If material.CBRActive Then
                material.Cbr = NewCBR
            End If

            If material.KValueActive Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                    If PCAConversionFormula Then
                        material.SubgradeReaction = NewKvalue
                    End If
                End If
            End If

            OnPropertyChanged(NameOf(NewCBR))
            OnPropertyChanged(NameOf(NewModulus))
            OnPropertyChanged(NameOf(NewKvalue))

        End Sub


        Public Function OnNewModulusEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property NewRuptureEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnNewRuptureEnter, AddressOf OnNewRuptureEnterEnabled)
            End Get
        End Property


        Public Sub OnNewRuptureEnter(context As Object)

            If CurrentJob.DesignOptions.MeasurementSystem.GetType Is GetType(UsCustomary) Then

                NewRupture = FaarFieldFactory.CreateModulus(context, FaarFieldFactory.CreateUsCustomary)
            Else
                NewRupture = FaarFieldFactory.CreateThickness(context, FaarFieldFactory.CreateMetric)
            End If

            material.Rupture = NewRupture

            If material.Rupture.UsCustomary <> NewRupture.UsCustomary Then
                NewRupture = material.Modulus
            End If

        End Sub


        Public Function OnNewRuptureEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property NewCBREnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnNewCBREnter, AddressOf OnNewCBREnterEnabled)
            End Get
        End Property


        Public Sub OnNewCBREnter(context As Object)

            If CurrentJob.DesignOptions.MeasurementSystem.GetType Is GetType(UsCustomary) Then
                NewCBR = context
            End If

            If NCHRPFormula = True Then
                material.NCHRPActive = True
            End If

            material.Cbr = NewCBR

            If material.Cbr <> NewCBR Then
                NewCBR = material.Cbr
            End If

            If material.CBRActive Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "HMA on Aggregate" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Flexible" Then
                    If NCHRPFormula Then
                        NewModulus = FaarFieldFactory.CreateModulus((2555 * (NewCBR ^ 0.64)), New UsCustomary)
                    Else
                        NewModulus = FaarFieldFactory.CreateModulus((NewCBR * 1500), New UsCustomary)
                    End If
                End If
            End If

            If material.KValueActive Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                    If PCAConversionFormula Then
                        NewKvalue = FaarFieldFactory.CreateSubgradeReaction((0.8155 * (NewModulus.UsCustomary ^ 0.5719)), New UsCustomary)
                    Else
                        NewKvalue = FaarFieldFactory.CreateSubgradeReaction(Math.Pow(NewModulus.UsCustomary / 20.15, 1 / 1.28405), New UsCustomary)
                    End If
                End If
            End If

            If material.KValueActive Then
                If CurrentSectionView.Section.AnalysisType.Name = "New Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "PCC Overlay on Flexible" Or CurrentSectionView.Section.AnalysisType.Name = "Unbonded PCC Overlay on Rigid" Or CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then
                    If PCAConversionFormula Then
                        material.SubgradeReaction = NewKvalue
                    End If
                End If
            End If

            OnPropertyChanged(NameOf(NewCBR))
            OnPropertyChanged(NameOf(NewModulus))
            OnPropertyChanged(NameOf(NewKvalue))

        End Sub


        Public Function OnNewCBREnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property NewKvalueEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnNewKvalueEnter, AddressOf OnNewKvalueEnterEnabled)
            End Get
        End Property


        Public Sub OnNewKvalueEnter(context As Object)

            If CurrentJob.DesignOptions.MeasurementSystem.GetType Is GetType(UsCustomary) Then
                NewKvalue = FaarFieldFactory.CreateSubgradeReaction(context, New UsCustomary)
            Else
                NewKvalue = FaarFieldFactory.CreateSubgradeReaction(context, New Metric)
            End If

            If PCAConversionFormula Then
                material.PCAConversionActive = True
            End If

            material.SubgradeReaction = NewKvalue

            If material.SubgradeReaction.UsCustomary <> NewKvalue.UsCustomary Then
                NewKvalue = material.SubgradeReaction
            End If

            If PCAConversionFormula Then
                NewModulus = FaarFieldFactory.CreateModulus(((NewKvalue.UsCustomary / 0.8155) ^ (1 / 0.5719)), New UsCustomary)
            Else
                NewModulus = FaarFieldFactory.CreateModulus(Math.Pow(NewKvalue.UsCustomary, 1.28405) * 20.15, New UsCustomary)
            End If

            material.Modulus = NewModulus

            If material.CBRActive Then
                material.Cbr = NewCBR
            End If

            material.SubgradeReaction = NewKvalue
            OnPropertyChanged(NameOf(NewModulus))
            OnPropertyChanged(NameOf(NewKvalue))

        End Sub


        Public Function OnNewKvalueEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Property Nsection As Integer
            Get
                If _Nsection > 32 Then
                    MessageBox.Show("NSection parameter can not be greater than 32. " + vbNewLine + "NSection parameter would be set to the default value of 16.")
                    _Nsection = 16
                ElseIf _Nsection < 4 Then
                    MessageBox.Show("NSection parameter can not be smaller than 32. " + vbNewLine + "NSection parameter would be set to the default value of 16.")
                    _Nsection = 16
                End If

                CurrentJob.DesignOptions.SectionParameterN = _Nsection
                Return _Nsection
            End Get
            Set(value As Integer)
                If value <> _Nsection Then
                    CurrentJob.DesignOptions.SectionParameterN = value
                    _Nsection = value
                    OnPropertyChanged(NameOf(Nsection))
                End If
            End Set
        End Property


        Public ReadOnly Property NsectionEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnNSectionEnter, AddressOf OnNSectionEnterEnabled)
            End Get
        End Property


        Public Sub OnNSectionEnter(context As Object)
            _Nsection = context
            OnPropertyChanged(NameOf(Nsection))
            OnUpdateselectedMaterial()
        End Sub


        Public Function OnNSectionEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property MaterialEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnMaterialEnter, AddressOf OnMaterialEnterEnabled)
            End Get
        End Property


        Public Sub OnMaterialEnter(context As Object)
            OnUpdateselectedMaterial()
        End Sub


        Public Function OnMaterialEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Property ModulusMenuText As String
            Get
                Return _modulusmenutext
            End Get
            Set
                If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then

                    If ReplacedLayer.Name = "Subgrade" Then
                        _modulusmenutext = "Subgrade layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "1,000 to 50,000 Psi " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "Enter the new modulus and click OK."
                    ElseIf ReplacedLayer.Name = "User Defined" Then
                        _modulusmenutext = "User Defined layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "1,000 to 400,000 Psi " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "Enter the new modulus and click OK."
                    ElseIf ReplacedLayer.Name = "Variable (flexible)" Then
                        _modulusmenutext = "Variable (flexible) layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "150,000 to 400,000 Psi " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "Enter the new modulus and click OK."
                    ElseIf ReplacedLayer.Name = "Variable (rigid)" Then
                        _modulusmenutext = "Variable (rigid) layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "250,000 to 700,000 Psi " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "Enter the new modulus and click OK."
                    End If
                Else
                    If ReplacedLayer.Name = "Subgrade" Then
                        _modulusmenutext = "Subgrade layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "6.89 to 344.74 MPa " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "Enter the new modulus and click OK."
                    ElseIf ReplacedLayer.Name = "User Defined" Then
                        _modulusmenutext = "User Defined layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + " 6.89 to 27,579.03 MPa " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "Enter the new modulus and click OK."
                    ElseIf ReplacedLayer.Name = "Variable (flexible)" Then
                        _modulusmenutext = "User Defined layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + " 1,034.21 to 2,757.90 MPa " + vbNewLine
                        _modulusmenutext = _modulusmenutext + "Enter the new modulus and click OK."
                    ElseIf ReplacedLayer.Name = "Variable (rigid)" Then
                        _modulusmenutext = "User Defined layer modulus value can be set between " + vbNewLine
                        _modulusmenutext = _modulusmenutext + " 1,723.69 to 4,826.33 MPa " + vbNewLine
                        _modulusmenutext = _modulusmenutext + " Enter the new modulus and click OK."
                    End If

                End If

                OnPropertyChanged(NameOf(ModulusMenuText))
            End Set
        End Property


        Public Property ThicknessMenuText As String
            Get
                Return _thicknessmenutext
            End Get
            Set
                Dim CheckforRigidLayer As Integer = 0
                Dim unit As String
                If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                    unit = "in"
                Else
                    unit = "mm"
                End If
                For I = 0 To CurrentSectionView.Section.Layers.Count - 1
                    If CurrentSectionView.Section.Layers.Item(I).Category = "P-501 PCC" Then
                        CheckforRigidLayer = 1
                    End If
                Next
                If ReplacedLayer.Name = "Subgrade" Then

                ElseIf ReplacedLayer.Name = "User Defined" Then
                    If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Name = ReplacedLayer.Name Then
                        If CheckforRigidLayer = 0 Then
                            _thicknessmenutext = "Subgrade layer CBR value can be set between " + vbNewLine
                            _thicknessmenutext = _thicknessmenutext + "0.7 to 2666.7 Percent " + vbNewLine
                            _thicknessmenutext = _thicknessmenutext + ". Enter the new CBR and click OK."
                        Else
                            _thicknessmenutext = "Subgrade layer k value can be set between " + vbNewLine
                            _thicknessmenutext = _thicknessmenutext + "20.9 to 440.4 pci (5.7 to 119.5 MN/m^3)" + vbNewLine
                            _thicknessmenutext = _thicknessmenutext + " Enter the new K value and click OK."
                        End If
                    End If
                Else
                    _thicknessmenutext = "Enter the new thickness in " + unit + " and click OK."
                End If

                OnPropertyChanged(NameOf(ThicknessMenuText))
            End Set
        End Property


        Public Property NewThickness As Thickness
            Get
                Return _newthickness
            End Get
            Set(value As Thickness)
                _newthickness = value
                OnPropertyChanged(NameOf(NewThickness))
            End Set
        End Property


        Public Property NewModulus As Modulus
            Get
                Return _newmodulus
            End Get
            Set(value As Modulus)
                _newmodulus = value
                OnPropertyChanged(NameOf(NewModulus))
            End Set
        End Property


        Public Property NewRupture As Modulus
            Get
                Return _newRupture
            End Get
            Set(value As Modulus)
                _newRupture = value
                OnPropertyChanged(NameOf(NewRupture))
            End Set
        End Property


        Public Property NewCBR As Double
            Get
                Return _newCBR
            End Get
            Set(value As Double)
                _newCBR = value
                OnPropertyChanged(NameOf(NewCBR))
            End Set
        End Property


        Public Property NewKvalue As SubgradeReaction
            Get
                Return _newKvalue
            End Get
            Set(value As SubgradeReaction)
                _newKvalue = value
                OnPropertyChanged(NameOf(NewKvalue))
            End Set
        End Property


        Public Property Sci As Double
            Get
                If _sci < 67 Or _sci > 100 Then
                    MessageBox.Show("The allowed range for SCI is 67 to 100.")
                    _sci = StoreSci

                ElseIf _sci <> 100 And _cdfu <> 100 Then
                    MessageBox.Show("If SCI for the existing pavement is set to less than 100, the existing pavement is classified as cracked." + vbNewLine + "Percent CDFU is fixed at 100 for SCI < 100.")

                    _cdfu = 100
                    OnPropertyChanged(NameOf(Cdfu))
                    CurrentSectionView.Section.Sci = _sci
                    CurrentSectionView.Section.PercentCdfu = 100

                Else
                    WarningSciCdfu = Visibility.Hidden
                End If
                Return _sci
            End Get
            Set
                If Value <> _sci Then
                    CurrentSectionView.Section.Sci = Value
                    _sci = Value
                    OnPropertyChanged(NameOf(WarningSciCdfu))
                    OnPropertyChanged(NameOf(Sci))
                End If
            End Set
        End Property


        Public Property CriticalStressReport As String
            Get
                Return _criticalAircraftReport
            End Get
            Set(value As String)
                _criticalAircraftReport = value
                OnPropertyChanged(NameOf(CriticalStressReport))
            End Set
        End Property


        Public Property SlabEdgeStress As String
            Get
                If _slabEdgeStress <> 0 Then
                    _slabEdgeStressreport = Format(_slabEdgeStress, "N1").ToString
                Else
                    _slabEdgeStressreport = Nothing
                End If
                If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                    Return _slabEdgeStressreport
                Else
                    _slabEdgeStressreport = Format(_slabEdgeStressreport * 0.00689476, "N1").ToString
                    Return _slabEdgeStressreport
                End If
            End Get
            Set
                _slabEdgeStressreport = Value
                OnPropertyChanged(NameOf(SlabEdgeStress))
            End Set
        End Property


        Public Property SlabInteriorStress As String
            Get
                If _slabInteriorStress <> 0 Then
                    _slabInteriorStressreport = Format(_slabInteriorStress, "N1").ToString
                Else
                    _slabInteriorStressreport = Nothing
                End If
                If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                    Return _slabInteriorStressreport
                Else
                    _slabInteriorStressreport = Format(_slabInteriorStressreport * 0.00689476, "N1").ToString
                    Return _slabInteriorStressreport
                End If
            End Get
            Set
                _slabInteriorStressreport = Value
                OnPropertyChanged(NameOf(SlabInteriorStress))
            End Set
        End Property


        Private Property _criticalStressAircraft As String = "None"
        Public Property CriticalStressAircraft As String
            Get
                Return _criticalStressAircraft
            End Get
            Set
                _criticalStressAircraft = Value
                OnPropertyChanged(NameOf(CriticalStressAircraft))
            End Set
        End Property


        Public ReadOnly Property SciEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnSciEnter, AddressOf OnSciEnterEnabled)
            End Get
        End Property


        Public Sub OnSciEnter(context As Object)
            StoreSci = _sci
            _sci = context
            OnPropertyChanged(NameOf(SCI))
        End Sub


        Public Function OnSciEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Function OnStressEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Property Cdfu As Double
            Get
                If _sci = 100 Then
                    If _cdfu < 10 Or _cdfu > 100 Then
                        MessageBox.Show("Enter a value for the percent CDFU of the existing " + vbNewLine + "pavement in the range of 10% to 100%. Please see the" + vbNewLine + " User's Manual for More information.")
                        _cdfu = StoreCdfu
                    End If
                ElseIf _sci < 100 Then

                    If _cdfu <> 100 Then
                        MessageBox.Show("SCI for the existing pavement is set to less than 100 and the existing pavement is classified as cracked." + vbNewLine + "Percent CDFU is fixed at 100 for SCI < 100.")
                        _cdfu = 100

                    End If
                End If
                CurrentSectionView.Section.PercentCdfu = _cdfu
                CurrentSectionView.Section.Sci = _sci
                Return _cdfu
            End Get
            Set
                If Value <> _cdfu Then
                    CurrentSectionView.Section.PercentCdfu = Value
                    _cdfu = Value
                    OnPropertyChanged(NameOf(Cdfu))
                End If
            End Set
        End Property


        Public ReadOnly Property CdfuEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnCdfuEnter, AddressOf OnCdfuEnterEnabled)
            End Get
        End Property


        Public Sub OnCdfuEnter(context As Object)
            StoreCdfu = _cdfu
            _cdfu = context
            OnPropertyChanged(NameOf(Cdfu))
        End Sub


        Public Function OnCdfuEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Property PtoTC As Double
            Get
                If _PtoTC > 10 Or _PtoTC < 0.1 Then
                    MessageBox.Show("The allowable range for P/TC ratio is 0.1 to 10.")
                    _PtoTC = StorePtoTC
                End If
                Return _PtoTC
            End Get
            Set(value As Double)
                _PtoTC = value
                CurrentSectionView.Section.PtoTC = _PtoTC
                OnPropertyChanged(NameOf(PtoTC))
            End Set
        End Property


        Public ReadOnly Property PtoTCEnter As ICommand
            Get
                Return New DelegateCommand(AddressOf OnPtoTCEnter, AddressOf OnPtoTCEnterEnabled)
            End Get
        End Property


        Public Sub OnPtoTCEnter(context As Object)
            StorePtoTC = _PtoTC
            _PtoTC = context
            OnPropertyChanged(NameOf(PtoTC))
        End Sub


        Public Function OnPtoTCEnterEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Property CalculatedLife As String
            Get
                If _calculatedLife <> 0 Then
                    _calculatedlifereport = Format(_calculatedLife, "N1").ToString
                Else
                    _calculatedlifereport = ""
                End If
                Return _calculatedlifereport
            End Get
            Set
                _calculatedlifereport = Value
                OnPropertyChanged(NameOf(CalculatedLife))
            End Set
        End Property


        Public Property SubgradeDepth As String
            Get
                Return _subgradeDepth
            End Get
            Set
                _subgradeDepth = Value
                OnPropertyChanged(NameOf(SubgradeDepth))
            End Set
        End Property


        Public Property SelectedAirplane As IAirplaneInfo
            Get
                Return _selectedAirplane
            End Get
            Set
                If Value IsNot _selectedAirplane Then
                    _selectedAirplane = Value
                    OnPropertyChanged(NameOf(SelectedAirplane))
                    SlabListUpdate()
                    If RunCompleted Then SelectedTabIndex = 1
                    GearImage = DrawGear()
                End If
            End Set
        End Property


        Public Property SelectedMaterial As IMaterial
            Get
                Return _selectedMaterial
            End Get
            Set
                Dim _factory As New FaarFieldModelFactory
                If Value Is Nothing Then

                Else
                    _selectedMaterial = Value
                    OnPropertyChanged(NameOf(SelectedMaterial))
                End If
            End Set
        End Property


        Public ReadOnly Property OnValidateDesignOptions As ICommand
            Get
                Return New DelegateCommand(AddressOf ValidateDesignOptions, AddressOf ValidateJobInformationEnabled)
            End Get
        End Property


        Public ReadOnly Property OnValidateJobInformation As ICommand
            Get
                Return New DelegateCommand(AddressOf ValidateJobInformation, AddressOf ValidateJobInformationEnabled)
            End Get
        End Property


        Private Sub ValidateJobInformation(context As Object)

            WarningFrostPenetrationHidden = Visibility.Hidden
            Dim jobInformation = CType(CurrentJob.JobInformation, JobInformation)
            Dim validations = jobInformation.Validate(CurrentJob.DesignOptions.MeasurementSystem)

            For Each validation In validations
                Select Case validation.Label
                    Case "MessageFrostPenetration"
                        WarningFrostPenetrationHidden = Visibility.Visible
                End Select
            Next

            OnPropertyChanged(NameOf(ValidateJobInformation))
        End Sub


        Private Shared Function ValidateJobInformationEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property OnValidateJobName As ICommand
            Get
                Return New DelegateCommand(AddressOf ValidateJobName, AddressOf ValidateJobNameEnabled)
            End Get
        End Property


        Private Sub ValidateJobName(context As Object)
            'Need to add check for legal characters            
            CurrentJobView.ChangeName(CurrentJob.Name)
            OnPropertyChanged(NameOf(CurrentJob))

        End Sub


        Private Shared Function ValidateJobNameEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property OnValidateSectionName As ICommand
            Get
                Return New DelegateCommand(AddressOf ValidateSectionName, AddressOf ValidateSectionNameEnabled)
            End Get
        End Property


        Private Sub ValidateSectionName(context As Object)
            'Need to add check for legal characters            

            If CurrentJob.Sections.Count <> 0 Then
                CurrentSectionView.ChangeName(CurrentSectionView.Section.Name)
                OnPropertyChanged(NameOf(CurrentSectionView))
            Else
                MessageBox.Show("First, a structure should be added to the current job. Then, the structure name can be changed.")
            End If

        End Sub


        Public Sub UpdateSectionAirplanes()
            DeleteTrafficEnabled = True
            OnPropertyChanged(NameOf(DeleteTrafficEnabled))
            OnPropertyChanged(NameOf(CurrentAirplanes))
            OnPropertyChanged(NameOf(CurrentSectionView))
        End Sub


        Private Shared Function ValidateSectionNameEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property SaveTrafficLibraryCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnSaveTrafficLibrary, AddressOf OnSaveTrafficLibraryEnabled)
            End Get
        End Property


        Private Sub OnSaveTrafficLibrary(context As Object)

            SetVersions(LibraryVersion, SoftwareVersion)

            CurrentLibrary = SaveTrafficLibrary(CurrentLibrary, CurrentAirplanes, TrafficLibrary, FaarFieldFactory)
            OnPropertyChanged(NameOf(CurrentLibrary))

            RefreshGWPercent()

        End Sub


        Private Function OnSaveTrafficLibraryEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property DeleteselectedaircraftCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDeleteselectedaircraft, AddressOf OnDeleteselectedaircraftEnabled)
            End Get
        End Property


        Public ReadOnly Property DeleteCurrentWheel As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDeleteCurrentWheel, AddressOf OnDeleteCurrentWheelEnabled)
            End Get
        End Property


        Public Sub OnDeleteCurrentWheel(context As Object)
            If _SelectedWheel Is Nothing Then
                System.Windows.MessageBox.Show("No wheel is selected.")
            Else
                CurrentWheel.Remove(_SelectedWheel)
                OnPropertyChanged(NameOf(CurrentWheel))
                UserDefinedGearImage = DrawUserDefinedGear()
            End If
        End Sub


        Public Function OnDeleteCurrentWheelEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property DeleteCurrentEval As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDeleteCurrentEval, AddressOf OnDeleteCurrentEvalEnabled)
            End Get
        End Property


        Public Sub OnDeleteCurrentEval(context As Object)
            If _SelectedEval Is Nothing Then
                System.Windows.MessageBox.Show("No wheel is selected.")
            Else
                CurrentEval.Remove(_SelectedEval)
                OnPropertyChanged(NameOf(CurrentEval))
                UserDefinedGearImage = DrawUserDefinedGear()
            End If
        End Sub


        Public Function OnDeleteCurrentEvalEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Sub OnDeleteselectedaircraft(context As Object)
            Dim tmpAP As ObservableCollection(Of AirplaneInfo) = New ObservableCollection(Of AirplaneInfo)
            Dim SelectedAircraft As IAirplaneInfo
            For Each cap In CurrentAirplanes
                tmpAP.Add(cap)
            Next
            For Each si In context.SelectedItems
                SelectedAircraft = si
                Dim isBelly As Boolean = False

                If (si.Name.Contains("Belly")) Then
                    isBelly = True
                End If
                If si.IsBelly = True Then
                    isBelly = True
                End If
                If si.Name.Contains("(UDA)") Then
                    isBelly = False
                End If

                If isBelly Then
                    For i = 0 To tmpAP.Count - 1
                        If si Is tmpAP(i) Then
                            Dim namelengh As Integer
                            namelengh = tmpAP.Item(i).Name.Length
                            If namelengh >= 5 Then namelengh = 5
                            If tmpAP.Item(i).Name.Substring(tmpAP.Item(i).Name.Length - namelengh) = "Belly" Then
                                SelectedAircraft = tmpAP.Item(i - 1)
                            Else
                                SelectedAircraft = tmpAP.Item(i + 1)
                            End If
                        End If
                    Next
                    tmpAP.Remove(SelectedAircraft)
                End If
                tmpAP.Remove(si)

            Next

            CurrentSectionView.Section.Airplanes.Clear()
            CurrentAirplanes.Clear()
            For Each newAP In tmpAP
                CurrentSectionView.Section.Airplanes.Add(newAP)
                CurrentAirplanes.Add(newAP)
            Next

            UpdateSectionAirplanes()

            OnPropertyChanged(NameOf(GearImage))
            OnPropertyChanged(NameOf(CurrentAirplanes))
            If CurrentAirplanes.Count = 0 Then
                DeleteTrafficEnabled = False
                OnPropertyChanged(NameOf(DeleteTrafficEnabled))
            End If
            OnPropertyChanged(NameOf(CurrentSectionView))

            If RunCompleted Then SelectedTabIndex = 1
            Try
                ResetCalculatedAirplaneValues()
            Catch ex As Exception

            End Try

            'clear Store Aircraft Mix combo box after deleting an aircraft in a saved file
            CurrentLibraryIndex = -1
            CurrentSectionView.Section.TrafficMixName = Nothing
            OnPropertyChanged(NameOf(CurrentLibraryIndex))

            CurrentSectionView.Section.SavedPCRhtml = Nothing
            CurrentSectionView.Section.SavedPCRgraph = Nothing
            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
            CurrentSectionView.Section.SavedCDFgraph = Nothing
            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
            CurrentSectionView.Section.LastRun = Nothing
        End Sub


        Private Function OnDeleteselectedaircraftEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property DeleteSavedFileCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDeleteSavedFile, AddressOf OnDeleteSavedFileEnabled)
            End Get
        End Property


        Private Function OnDeleteSavedFileEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Sub OnDeleteSavedFile(context As Object)

            Dim ask As MessageBoxResult = MsgBox("Are you sure that you want to delete your selected mix file?", MsgBoxStyle.YesNo)

            If ask = MessageBoxResult.Yes Then

                CurrentSectionView.Section.Airplanes.Clear()
                CurrentAirplanes.Clear()
                UpdateSectionAirplanes()

                Dim delPath = SpecialDirectories.MyDocuments & Path.DirectorySeparatorChar & "My FAARFIELD" & Path.DirectorySeparatorChar & "TrafficLibrary" & Path.DirectorySeparatorChar & CurrentLibrary & ".json"

                If File.Exists(delPath) Then
                    File.Delete(delPath)
                End If
                TrafficLibrary.Remove(CurrentLibrary)

                CurrentLibrary = Nothing
                OnPropertyChanged(NameOf(CurrentLibrary))

                OnPropertyChanged(NameOf(TrafficLibrary))

                _DeleteAircraftMixEnabled = False
                OnPropertyChanged(NameOf(DeleteAircraftMixEnabled))
            End If
        End Sub


        Private Sub ResetCalculatedAirplaneValues()
            If CurrentSectionView IsNot Nothing Then
                For I = 0 To CurrentSectionView.Section.Airplanes.Count - 1
                    CurrentSectionView.Section.Airplanes.Item(I).Cdf = 0
                    CurrentSectionView.Section.Airplanes.Item(I).CdfAircraftMax = 0
                    CurrentSectionView.Section.Airplanes.Item(I).CtoP = 0
                    CurrentSectionView.Section.Airplanes.Item(I).ACRB = 0
                    CurrentSectionView.Section.Airplanes.Item(I).ACRB1 = 0
                    CurrentSectionView.Section.Airplanes.Item(I).ACRB2 = 0
                    CurrentSectionView.Section.Airplanes.Item(I).ACRB3 = 0
                    CurrentSectionView.Section.Airplanes.Item(I).ACRB4 = 0
                    CurrentSectionView.Section.Airplanes.Item(I).ACRThick = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                    CurrentSectionView.Section.Airplanes.Item(I).ACRThick1 = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                    CurrentSectionView.Section.Airplanes.Item(I).ACRThick2 = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                    CurrentSectionView.Section.Airplanes.Item(I).ACRThick3 = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                    CurrentSectionView.Section.Airplanes.Item(I).ACRThick4 = FaarFieldFactory.CreateThickness(0, FaarFieldFactory.CreateUsCustomary)
                    ACRHeader = "ACR"
                    ACRHeader1 = "ACR"
                    ACRHeader2 = "ACR"
                    ACRHeader3 = "ACR"
                    ACRHeader4 = "ACR"

                    Dim ag = CurrentSectionView.Section.Airplanes.Item(I).AnnualGrowth / 100
                    Dim nd = CurrentSectionView.Section.Airplanes.Item(I).NumberDepartures

                    Dim TempL, TempL1 As Single
                    TempL1 = DesignLife
                    If 1.0! + ag * DesignLife < 0.0! Then
                        TempL1 = -1.0! / ag
                    End If
                    TempL = CSng(1.0! + TempL1 * ag * 0.5)
                    If CurrentSectionView.Section.Airplanes.Item(I).DefaultCp Is Nothing Then
                        CurrentSectionView.Section.Airplanes.Item(I).DefaultCp = CurrentSectionView.Section.Airplanes.Item(I).Cp
                    End If

                    If CurrentSectionView.Section.Airplanes.Item(I).DefaultGrossWeight Is Nothing Then
                        CurrentSectionView.Section.Airplanes.Item(I).DefaultGrossWeight = CurrentSectionView.Section.Airplanes.Item(I).GrossWeight
                    End If
                    CurrentSectionView.Section.Airplanes.Item(I).TotalDepartures = TempL * nd * TempL1
                    CurrentSectionView.Section.Airplanes.Item(I).Cp = FaarFieldFactory.CreatePressure(CurrentSectionView.Section.Airplanes.Item(I).GrossWeight.UsCustomary * CurrentSectionView.Section.Airplanes.Item(I).MgPercent / CurrentSectionView.Section.Airplanes.Item(I).NumberWheels / (CurrentSectionView.Section.Airplanes.Item(I).DefaultGrossWeight.UsCustomary * CurrentSectionView.Section.Airplanes.Item(I).MgPercent / CurrentSectionView.Section.Airplanes.Item(I).NumberWheels / CurrentSectionView.Section.Airplanes.Item(I).DefaultCp.UsCustomary), New UsCustomary)
                    If CurrentSectionView.Section.Airplanes.Item(I).Name.Contains("Belly") Then
                        If CurrentSectionView.Section.Airplanes.Item(I) Is GridSelectedAirplane Then
                            CurrentSectionView.Section.Airplanes.Item(I - 1).NumberDepartures = CurrentSectionView.Section.Airplanes.Item(I).NumberDepartures
                            CurrentSectionView.Section.Airplanes.Item(I - 1).TotalDepartures = CurrentSectionView.Section.Airplanes.Item(I).TotalDepartures
                            CurrentSectionView.Section.Airplanes.Item(I - 1).AnnualGrowth = CurrentSectionView.Section.Airplanes.Item(I).AnnualGrowth
                            CurrentSectionView.Section.Airplanes.Item(I - 1).GrossWeight = CurrentSectionView.Section.Airplanes.Item(I).GrossWeight
                        ElseIf CurrentSectionView.Section.Airplanes.Item(I - 1) Is GridSelectedAirplane Then
                            CurrentSectionView.Section.Airplanes.Item(I).NumberDepartures = CurrentSectionView.Section.Airplanes.Item(I - 1).NumberDepartures
                            CurrentSectionView.Section.Airplanes.Item(I).TotalDepartures = CurrentSectionView.Section.Airplanes.Item(I - 1).TotalDepartures
                            CurrentSectionView.Section.Airplanes.Item(I).AnnualGrowth = CurrentSectionView.Section.Airplanes.Item(I - 1).AnnualGrowth
                            CurrentSectionView.Section.Airplanes.Item(I).GrossWeight = CurrentSectionView.Section.Airplanes.Item(I - 1).GrossWeight
                        End If
                    End If

                Next
                MessageText = ""
            Else
                SectionIsHidden = True
            End If
        End Sub


        Public Sub ResetSlabStressValues()
            _slabEdgeStress = Nothing
            _slabInteriorStress = Nothing
            SlabEdgeStress = Nothing
            SlabInteriorStress = Nothing
            _criticalStressAircraft = "None"
            OnPropertyChanged(NameOf(CriticalStressAircraft))
            OnPropertyChanged(NameOf(SlabEdgeStress))
            OnPropertyChanged(NameOf(SlabInteriorStress))
        End Sub


        Public ReadOnly Property DeleteTrafficLibraryCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDeleteTrafficLibrary, AddressOf OnDeleteTrafficLibraryEnabled)
            End Get
        End Property


        Public ReadOnly Property OnAircraftCellEndingEditing As ICommand
            Get
                Return New DelegateCommand(AddressOf AircraftCellEndingEditing, AddressOf AircraftCellEndingEditingEnabled)
            End Get
        End Property


        Public Sub AircraftCellEndingEditing(context As Object)
            Dim airplane = _selectedAirplane
            Dim count = CurrentAirplanes
        End Sub


        Private Function AircraftCellEndingEditingEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Sub OnDeleteTrafficLibrary(context As Object)
            Dim ret As Integer, M1 As String
            M1 = "This will delete all aircrafts from the traffix mix. Do you want to proceed?"

            ret = MsgBox(M1, MsgBoxStyle.YesNo)

            If ret = 6 Then
                If Not CurrentSectionView.Section Is Nothing Then
                    CurrentSectionView.Section.Airplanes.Clear()
                End If

                CurrentAirplanes.Clear()
                UpdateSectionAirplanes()
                CurrentSectionView.Section.TrafficMixName = ""

                CurrentLibrary = Nothing
                OnPropertyChanged(NameOf(CurrentLibrary))
                DeleteTrafficEnabled = False
                OnPropertyChanged(NameOf(DeleteTrafficEnabled))

            End If

        End Sub


        Private Function OnDeleteTrafficLibraryEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property DeleteselectedLayerCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDeleteselectedLayer, AddressOf OnDeleteselectedLayerEnabled)
            End Get
        End Property


        Public ReadOnly Property SelectDesignLayerCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnSelectDesignLayer, AddressOf OnSelectDesignLayerEnabled)
            End Get
        End Property


        Public ReadOnly Property GearImageToClipboardCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnGearImageToClipboardCommand, AddressOf OnGearImageToClipboardCommandEnabled)
            End Get
        End Property


        Public ReadOnly Property LayerImageToClipboardCommand As ICommand
            Get
                Return New DelegateCommand(AddressOf OnLayerImageToClipboardCommand, AddressOf OnLayerImageToClipboardCommandEnabled)
            End Get
        End Property


        Public Sub OnGearImageToClipboardCommand(context As Object)
            Clipboard.SetImage(DrawGear())
            Return
        End Sub


        Public Sub OnLayerImageToClipboardCommand(context As Object)
            Clipboard.SetImage(DrawProfile())
            Return
        End Sub


        Public Function OnGearImageToClipboardCommandEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Function OnLayerImageToClipboardCommandEnabled(context As Object) As Boolean
            Return True
        End Function


        Public Sub OnSelectDesignLayer(context As Object)
            If Not AnalysisType Is Nothing Then
                If CurrentSectionView.Section.Layers.Item(0).Name = "P-401/P-403 HMA Surface" Then
                    If _selectedMaterial Is Nothing Then
                        MessageBox.Show("No layer has been selected. The dafult layer would be chosen as the design layer")
                    Else
                        If _selectedMaterial IsNot CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1) Then
                            If _selectedMaterial IsNot CurrentSectionView.Section.Layers.Item(0) Then

                                'If _selectedMaterial IsNot CurrentSectionView.Section.Layers.Item(0) Then
                                For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                                    CurrentSectionView.Section.Layers.Item(i).DesignedLayer = ""
                                Next
                                If _selectedMaterial IsNot Nothing Then
                                    _selectedMaterial.DesignedLayer = "-->"
                                Else
                                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).DesignedLayer = "-->"
                                End If
                                'End If
                                ''System.Windows.Application.Current.Dispatcher.Invoke(Sub() RefreshNewModulus())
                            Else
                                MessageBox.Show("Only base and subbase material could be chosen as the design layer")
                            End If

                        Else
                            MessageBox.Show("Only base and subbase material could be chosen as the design layer")
                        End If
                    End If
                Else
                    MessageBox.Show("The Design Layer can not be changed in " + AnalysisType.Name + " Pavements.")
                End If
            Else

            End If

            ProfileImage = DrawProfile()

        End Sub


        Public Sub OnDeleteselectedLayer(context As Object)

            Dim CheckforRigidLayer As Integer = 0

            For I = 0 To CurrentSectionView.Section.Layers.Count - 1
                If CurrentSectionView.Section.Layers.Item(I).Category = "P-501 PCC" Then
                    CheckforRigidLayer = 1
                End If
            Next

            If _selectedMaterial Is Nothing Then
                MessageBox.Show("No layer is selected.")
            ElseIf CurrentSectionView.Section.Layers.Count <= 3 Then
                MessageBox.Show("minimum of three layers for pavement is required")
                Exit Sub
            ElseIf _selectedMaterial.Name = "Subgrade" Then

                If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).Name = "User Defined" Then
                    CurrentSectionView.Section.Layers.Remove(_selectedMaterial)
                    CurrentLayers.Remove(_selectedMaterial)

                    If CheckforRigidLayer = 1 And CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).SubgradeReaction Is Nothing Then
                        CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).SubgradeReaction = FaarFieldFactory.CreateSubgradeReaction(755.5, New UsCustomary)
                        CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).KValueActive = True

                    End If

                    If CheckforRigidLayer = 0 Then
                        CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Cbr = 66.6666
                        CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).CBRActive = True
                    End If
                    'User Defined disabled if it is the bottom layer
                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).ThicknessActive = False
                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1).Thickness = FaarFieldFactory.CreateThickness(6, FaarFieldFactory.CreateUsCustomary())
                    Call OnUpdateselectedMaterial()

                Else
                    MessageBox.Show("Only Subgrade and User Defined layers are allowed as the bottom layer.")
                End If

            ElseIf _selectedMaterial.Name = "User Defined" AndAlso _selectedMaterial Is CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 1) Then
                If CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).Name = "Subgrade" Or CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).Name = "User Defined" Then
                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).CBRActive = True
                    CurrentSectionView.Section.Layers.Remove(_selectedMaterial)
                    CurrentLayers.Remove(_selectedMaterial)
                    Call OnUpdateselectedMaterial()
                Else
                    MessageBox.Show("Only Subgrade and User Defined layers are allowed as the bottom layer.")
                End If

            ElseIf _selectedMaterial.Name = "P-501 PCC Surface" Then
                If CurrentSectionView.Section.AnalysisType.Name = "HMA Overlay on Rigid" Then 'Not allow user to delete P-501 PCC with HMA Overlay on Rigid design
                    MessageBox.Show("HMA Overlay on Rigid Design requires 501 PCC Surface Layer")
                End If

                If CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay (unbonded)" Or CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay (partially bonded)" Then
                    MessageBox.Show("Rigid Overlay layers can only be placed over PCC Surface layer.")
                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Surface" Then
                    MessageBox.Show("PCC surface layer can not be removed from a New Rigid pavement.")
                End If

            Else
                CurrentSectionView.Section.Layers.Remove(_selectedMaterial)
                CurrentLayers.Remove(_selectedMaterial)
                Call OnUpdateselectedMaterial()
            End If

            SetDesignComboBox(CurrentSectionView)
            _analysisType = Nothing
            _analysisType = CurrentSectionView.Section.AnalysisType
            OnUpdateAnalysisType()
            Call SelectDesignedLayer()
            Call OnUpdateselectedMaterial()

            ResetCalculatedAirplaneValues()
            _selectedMaterial = Nothing

            CurrentSectionView.Section.SavedPCRhtml = Nothing
            CurrentSectionView.Section.SavedPCRgraph = Nothing
            CurrentSectionView.Section.SavedAirportMasterRecordhtml = Nothing
            CurrentSectionView.Section.SavedCDFgraph = Nothing
            CurrentSectionView.Section.SavedDetailedReportHtml = Nothing
            CurrentSectionView.Section.SavedSectionReportHtml = Nothing
            CurrentSectionView.Section.LastRun = Nothing
        End Sub


        Sub SelectDesignedLayer()
            If CurrentSectionView.Section.Layers.Count > 0 Then
                For i = 0 To CurrentSectionView.Section.Layers.Count - 1
                    CurrentSectionView.Section.Layers.Item(i).DesignedLayer = ""
                Next

                If CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Surface" Or CurrentSectionView.Section.Layers.Item(0).Name = "P-401/P-403 HMA Overlay" Or CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay (unbonded)" Or CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay (partially bonded)" Or CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Overlay on Flexible" Then
                    CurrentSectionView.Section.Layers.Item(0).DesignedLayer = "-->"
                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "P-401/P-403 HMA Surface" Then
                    CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).DesignedLayer = "-->"
                ElseIf CurrentSectionView.Section.Layers.Item(0).Name = "User Defined" Then
                    If CurrentSectionView.Section.Layers.Item(1).Name = "P-401/P-403 HMA Surface" Or CurrentSectionView.Section.Layers.Item(0).Name = "P-501 PCC Surface" Then
                        CurrentSectionView.Section.Layers.Item(0).DesignedLayer = "-->"
                    Else
                        CurrentSectionView.Section.Layers.Item(CurrentSectionView.Section.Layers.Count - 2).DesignedLayer = "-->"

                    End If

                End If

            End If
        End Sub


        ' Update ACR Thick Headers in Traffic Grid
        Public Sub UpdateTrafficACRHeaders()
            Dim unit As String
            If TypeOf CurrentJob.DesignOptions.MeasurementSystem Is UsCustomary Then
                unit = "in."
            Else
                unit = "mm"
            End If
            UnitLabel = unit
            EdgeStressLabel = unit
            InteriorStressLabel = unit
            If ACRThickHeader.Contains("(A)") Then
                ACRThickHeader = "ACR Thick (" + unit + ")" + "(A)"
            ElseIf ACRThickHeader.Contains("(B)") Then
                ACRThickHeader = "ACR Thick (" + unit + ")" + "(B)"
            ElseIf ACRThickHeader.Contains("(C)") Then
                ACRThickHeader = "ACR Thick (" + unit + ")" + "(C)"
            ElseIf ACRThickHeader.Contains("(D)") Then
                ACRThickHeader = "ACR Thick (" + unit + ")" + "(D)"
            End If

            ACRThickHeader1 = "ACR Thick (" + unit + ")" + "(A)"
            ACRThickHeader2 = "ACR Thick (" + unit + ")" + "(B)"
            ACRThickHeader3 = "ACR Thick (" + unit + ")" + "(C)"
            ACRThickHeader4 = "ACR Thick (" + unit + ")" + "(D)"

        End Sub


        Public Sub OnUpdateselectedMaterial()
            OnPropertyChanged(NameOf(CurrentLayers))
            OnPropertyChanged(NameOf(CurrentSectionView))
            OnPropertyChanged(NameOf(Layername1))

            ProfileImage = DrawProfile()

            OnPropertyChanged(NameOf(GearImage))
            OnPropertyChanged(NameOf(Section))
            OnPropertyChanged(NameOf(JobViewModel))
            OnPropertyChanged(NameOf(SubgradeReaction))
            OnPropertyChanged(NameOf(TotalThicknessUpdate))
            OnPropertyChanged(NameOf(SlabEdgeStress))
            OnPropertyChanged(NameOf(SlabInteriorStress))
            OnPropertyChanged(NameOf(CriticalStressAircraft))
            OnPropertyChanged(NameOf(AircraftStressLabel))
            OnPropertyChanged(NameOf(Thickness))
            OnPropertyChanged(NameOf(ThicknessUpdateEnabled))

            If CalculatedLifeTracker = False Then
                _calculatedLife = Nothing
                OnPropertyChanged(NameOf(CalculatedLife))
            End If

        End Sub


        Sub OnUpdateAnalysisType()
            OnPropertyChanged(NameOf(AnalysisType))
        End Sub


        Private Function OnDeleteselectedLayerEnabled(context As Object) As Boolean
            Return True
        End Function


        Private Function OnSelectDesignLayerEnabled(context As Object) As Boolean
            Return True
        End Function


        Public ReadOnly Property DropAirplane_Command As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDropAirplane, AddressOf DropEnabled)
            End Get
        End Property


        Public ReadOnly Property DropMaterial_Command As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDropMaterial, AddressOf DropEnabled)
            End Get
        End Property


        Public ReadOnly Property DragMaterial_Command As ICommand
            Get
                Return New DelegateCommand(AddressOf OnDragMaterial, AddressOf DropEnabled)
            End Get
        End Property


        Private Function DropEnabled(context As Object) As Boolean
            Return True
        End Function

#End Region

        Private Property _ProfileImage As BitmapImage
        Public Property ProfileImage As BitmapImage
            Get
                Return _ProfileImage
            End Get
            Set(ByVal value As BitmapImage)
                _ProfileImage = value
                OnPropertyChanged(NameOf(ProfileImage))
            End Set
        End Property


        Private Property _GearImage As BitmapImage
        Public Property GearImage As BitmapImage
            Get
                Return _GearImage
            End Get
            Set(ByVal value As BitmapImage)
                _GearImage = value
                OnPropertyChanged(NameOf(GearImage))
            End Set
        End Property


        Private Property _UserDefinedGearImage As New BitmapImage
        Public Property UserDefinedGearImage As BitmapImage
            Get
                Return _UserDefinedGearImage
            End Get
            Set(ByVal value As BitmapImage)
                _UserDefinedGearImage = value
                OnPropertyChanged(NameOf(UserDefinedGearImage))
            End Set
        End Property


        'CDF Graph's visual same as PCR Graph's
        Public Function DrawCDFGraph() As Bitmap
            Dim cdfchart As New RadCartesianChart()
            Dim tmpAPList = New List(Of AirplaneInfo)(CurrentAirplanes)
            tmpAPList.Sort(Function(x, y) x.CDFGraphData.Max.CompareTo(y.CDFGraphData.Max))
            tmpAPList.Reverse()

            'add margin before drawing the chart
            Dim margin = cdfchart.Margin
            margin.Top = 20
            cdfchart.Margin = margin

            'Get the maximum value for YAxis
            Dim cdfMax As Single = 0
            Dim ymaximum As Double
            Dim cdfScale As Single = 0
            Dim tempcdfscale As Single = 0

            If Not CurrentSectionView.Section.SectionCDF Is Nothing Then
                For i = 0 To 42
                    If cdfMax < CurrentSectionView.Section.SectionCDF(i) Then
                        cdfMax = CurrentSectionView.Section.SectionCDF(i)
                    End If
                Next
            End If

            'YAxis scale

            If cdfMax > 1 Then
                tempcdfscale = CSng(Math.Pow(10, Math.Round(Math.Abs(Math.Log10(cdfMax)))))
                cdfScale = Math.Ceiling(Math.Round(cdfMax, 1, MidpointRounding.ToEven) / 10)
                'ymaximum = Math.Round(cdfMax) + cdfScale
                ymaximum = Math.Round(cdfMax / tempcdfscale, 1, MidpointRounding.ToEven) * tempcdfscale + cdfScale
            Else
                tempcdfscale = CSng(Math.Pow(10, Math.Floor(Math.Log10(cdfMax))))
                cdfScale = tempcdfscale
                ymaximum = Math.Round(cdfMax / tempcdfscale, 2, MidpointRounding.ToEven) * tempcdfscale + cdfScale
            End If

            'draw YAxis
            ', .MajorStep = cdfScale
            cdfchart.VerticalAxis = New LinearAxis() With {
                .LineStroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211)),
                .LineThickness = 3,
                .Maximum = ymaximum,
                .Minimum = 0}

            'add Grid for yAxis
            For i = cdfScale To (cdfMax + 2 * cdfScale) Step cdfScale
                Dim gridLine = New CartesianGridLineAnnotation() With {.Value = i, .Axis = cdfchart.VerticalAxis, .StrokeThickness = 0.5, .Stroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211))}
                cdfchart.Annotations.Add(gridLine)
            Next

            'draw XAxis
            If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                cdfchart.HorizontalAxis = New LinearAxis() With {
                .Title = "Offset - Inches",
                .LineThickness = 2,
                .LineStroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211)),
                .Maximum = 400,
                .Minimum = -400
            }
            Else
                cdfchart.HorizontalAxis = New LinearAxis() With {
                .Title = "Offset - Meters",
                .LineThickness = 2,
                .LineStroke = New SolidColorBrush(System.Windows.Media.Color.FromRgb(90, 164, 211)),
                .Maximum = 10000,
                .Minimum = -10000,
                .MajorStep = 2500
            }
            End If

            'X Axis series
            Dim XAxisSeries, XAxisSeries1, XAxisSeries2 As New List(Of Single)
            Dim x1, x2 As Single

            'add 0 to index(0) of XAxisSeries
            XAxisSeries1.Add(0)
            XAxisSeries2.Add(0)

            'add x coordinate to XSeries
            For i = 1 To 41
                If TypeOf (CurrentJob.DesignOptions.MeasurementSystem) Is UsCustomary Then
                    x1 = -((i - 1) * 10 + 5) '-5 to -415
                    x2 = (i - 1) * 10 + 5 'x = 5 to 415
                Else
                    x1 = -((i - 1) * 10 + 5) * 25.4 'x = -125 to -10000
                    x2 = ((i - 1) * 10 + 5) * 25.4 'x = 125 to 10.000
                End If

                XAxisSeries1.Add(x1)
                XAxisSeries2.Add(x2)
            Next

            'Colors palette for airplane cdf lines
            Dim colorPalette As New Dictionary(Of Int32, SolidColorBrush) From {
                {0, New SolidColorBrush(Colors.SteelBlue)}, {1, New SolidColorBrush(Colors.Red)}, {2, New SolidColorBrush(Colors.DarkGreen)},
                {3, New SolidColorBrush(Colors.Yellow)}, {4, New SolidColorBrush(Colors.Violet)}, {5, New SolidColorBrush(Colors.Orange)},
                {6, New SolidColorBrush(Colors.CadetBlue)}, {7, New SolidColorBrush(Colors.BlanchedAlmond)}, {8, New SolidColorBrush(Colors.Chocolate)},
                {9, New SolidColorBrush(Colors.Aquamarine)}, {10, New SolidColorBrush(Colors.Bisque)}, {11, New SolidColorBrush(Colors.CornflowerBlue)},
                {12, New SolidColorBrush(Colors.DarkGoldenrod)}, {13, New SolidColorBrush(Colors.Firebrick)}, {14, New SolidColorBrush(Colors.Gold)},
                {15, New SolidColorBrush(Colors.HotPink)}, {16, New SolidColorBrush(Colors.Indigo)}, {17, New SolidColorBrush(Colors.DarkKhaki)},
                {18, New SolidColorBrush(Colors.Gray)}, {19, New SolidColorBrush(Colors.MediumPurple)}, {20, New SolidColorBrush(Colors.Navy)},
                {21, New SolidColorBrush(Colors.Aqua)}, {22, New SolidColorBrush(Colors.Brown)}, {23, New SolidColorBrush(Colors.Chartreuse)},
                {24, New SolidColorBrush(Colors.DarkOrchid)}, {25, New SolidColorBrush(Colors.CornflowerBlue)}, {26, New SolidColorBrush(Colors.ForestGreen)},
                {27, New SolidColorBrush(Colors.LightSeaGreen)}, {28, New SolidColorBrush(Colors.Honeydew)}, {29, New SolidColorBrush(Colors.IndianRed)},
                {30, New SolidColorBrush(Colors.LightSalmon)}, {31, New SolidColorBrush(Colors.MediumSeaGreen)}, {32, New SolidColorBrush(Colors.OldLace)},
                {33, New SolidColorBrush(Colors.PaleTurquoise)}, {34, New SolidColorBrush(Colors.RosyBrown)}, {35, New SolidColorBrush(Colors.SkyBlue)},
                {36, New SolidColorBrush(Colors.Tan)}, {37, New SolidColorBrush(Colors.PaleVioletRed)}, {38, New SolidColorBrush(Colors.Plum)},
                {39, New SolidColorBrush(Colors.Lime)}, {40, New SolidColorBrush(Colors.Coral)}}

            'Draw CDF lines of airplanes
            Dim penCount = 0
            For i = 0 To tmpAPList.Count - 1
                'add y coordinate to YSeries
                Dim YAxisSeries As List(Of Single) = tmpAPList(i).CDFGraphData

                'setting color for airplane cdf line
                Dim acCdfLine As New ScatterLineSeries() With {.StrokeThickness = 3, .Stroke = colorPalette(penCount)}
                Dim acCdfLine2 As New ScatterLineSeries() With {.StrokeThickness = 3, .Stroke = colorPalette(penCount)}

                'add data point for airplane cdf line
                acCdfLine.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries1(0), .YValue = YAxisSeries(1)})
                acCdfLine2.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries2(0), .YValue = YAxisSeries(1)})

                For l = 1 To 40
                    acCdfLine.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries1(l), .YValue = YAxisSeries(l)})
                    acCdfLine2.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries2(l), .YValue = YAxisSeries(l)})
                Next

                cdfchart.Series.Add(acCdfLine)
                cdfchart.Series.Add(acCdfLine2)

                If penCount < 39 Then
                    penCount += 1
                Else
                    penCount = 0
                End If
            Next

            'draw cummulative CDF line
            Dim sectionCDF As List(Of Single) = CurrentSectionView.Section.SectionCDF

            Dim cummulativeCDFLine As New ScatterLineSeries() With {.Stroke = New SolidColorBrush(Colors.Blue), .StrokeThickness = 3}
            Dim cummulativeCDFLine2 As New ScatterLineSeries() With {.Stroke = New SolidColorBrush(Colors.Blue), .StrokeThickness = 3}

            cummulativeCDFLine.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries1(0), .YValue = sectionCDF(1)})
            cummulativeCDFLine2.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries2(0), .YValue = sectionCDF(1)})

            For j = 1 To 41
                cummulativeCDFLine.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries1(j), .YValue = sectionCDF(j)})
                cummulativeCDFLine2.DataPoints.Add(New Telerik.Charting.ScatterDataPoint With {.XValue = XAxisSeries2(j), .YValue = sectionCDF(j)})
            Next

            cdfchart.Series.Add(cummulativeCDFLine)
            cdfchart.Series.Add(cummulativeCDFLine2)

            Dim chartSize As New System.Windows.Size(800, 600)

            cdfchart.Measure(chartSize)
            cdfchart.Arrange(New Rect(chartSize))

            Dim chrtAxis = cdfchart.VerticalAxis
            Canvas.SetLeft(chrtAxis, cdfchart.PlotAreaClip.Width / 2)

            cdfchart.UpdateLayout()

            Return CreateBitmapFromCDFChart(cdfchart)

        End Function


        Private Function CreateBitmapFromCDFChart(cdfchart As RadCartesianChart) As Bitmap
            Dim tmpAPList = New List(Of AirplaneInfo)(CurrentAirplanes)
            Dim apCount As Integer = tmpAPList.Count
            tmpAPList.Sort(Function(x, y) x.CDFGraphData.Max.CompareTo(y.CDFGraphData.Max))
            tmpAPList.Reverse()

            'Colors for airplane cdf lines
            Dim colorPalette As New Dictionary(Of Int32, SolidColorBrush) From {
                {0, New SolidColorBrush(Colors.SteelBlue)}, {1, New SolidColorBrush(Colors.Red)}, {2, New SolidColorBrush(Colors.DarkGreen)},
                {3, New SolidColorBrush(Colors.Yellow)}, {4, New SolidColorBrush(Colors.Violet)}, {5, New SolidColorBrush(Colors.Orange)},
                {6, New SolidColorBrush(Colors.CadetBlue)}, {7, New SolidColorBrush(Colors.BlanchedAlmond)}, {8, New SolidColorBrush(Colors.Chocolate)},
                {9, New SolidColorBrush(Colors.Aquamarine)}, {10, New SolidColorBrush(Colors.Bisque)}, {11, New SolidColorBrush(Colors.CornflowerBlue)},
                {12, New SolidColorBrush(Colors.DarkGoldenrod)}, {13, New SolidColorBrush(Colors.Firebrick)}, {14, New SolidColorBrush(Colors.Gold)},
                {15, New SolidColorBrush(Colors.HotPink)}, {16, New SolidColorBrush(Colors.Indigo)}, {17, New SolidColorBrush(Colors.DarkKhaki)},
                {18, New SolidColorBrush(Colors.Gray)}, {19, New SolidColorBrush(Colors.MediumPurple)}, {20, New SolidColorBrush(Colors.Navy)},
                {21, New SolidColorBrush(Colors.Aqua)}, {22, New SolidColorBrush(Colors.Brown)}, {23, New SolidColorBrush(Colors.Chartreuse)},
                {24, New SolidColorBrush(Colors.DarkOrchid)}, {25, New SolidColorBrush(Colors.CornflowerBlue)}, {26, New SolidColorBrush(Colors.ForestGreen)},
                {27, New SolidColorBrush(Colors.LightSeaGreen)}, {28, New SolidColorBrush(Colors.Honeydew)}, {29, New SolidColorBrush(Colors.IndianRed)},
                {30, New SolidColorBrush(Colors.LightSalmon)}, {31, New SolidColorBrush(Colors.MediumSeaGreen)}, {32, New SolidColorBrush(Colors.OldLace)},
                {33, New SolidColorBrush(Colors.PaleTurquoise)}, {34, New SolidColorBrush(Colors.RosyBrown)}, {35, New SolidColorBrush(Colors.SkyBlue)},
                {36, New SolidColorBrush(Colors.Tan)}, {37, New SolidColorBrush(Colors.PaleVioletRed)}, {38, New SolidColorBrush(Colors.Plum)},
                {39, New SolidColorBrush(Colors.Lime)}, {40, New SolidColorBrush(Colors.Coral)}}


            Dim dv As DrawingVisual = New DrawingVisual()
            Dim dc As DrawingContext = dv.RenderOpen()

            Dim chartSize As System.Windows.Size = cdfchart.RenderSize
            Dim bmp As Bitmap
            Dim scale As Double = 2.0
            Dim rtb As New RenderTargetBitmap(CInt(chartSize.Width * scale), CInt((chartSize.Height + 500) * scale), 96D * scale, 96D * scale, PixelFormats.Pbgra32)

            dc.DrawRectangle(Windows.Media.Brushes.White, Nothing, New Rect(New Windows.Size(800, 1300)))

            dc.Close()
            rtb.Render(dv)

            dc = dv.RenderOpen()

            'Draw Chart Title
            dc.DrawRectangle(Windows.Media.Brushes.White, Nothing, New Rect(0, 1, 800, 17))
            dc.DrawText(New FormattedText(CurrentJob.Name & ": " & CurrentSectionView.Section.Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 16, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(250, 1))

            dc.Close()
            rtb.Render(dv)

            rtb.Render(cdfchart)

            dc = dv.RenderOpen()

            'legend at the bottom of the chart
            Dim penCount = 0
            Dim yRect = 0

            'cummulative cdf legend 
            dc.DrawRectangle(New SolidColorBrush(Colors.Blue), Nothing, New Rect(5, 650, 20, 3))
            dc.DrawText(New FormattedText("Cumulative CDF", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(35, 645))

            'aiplane cdf legend 
            If tmpAPList.Count > 40 Then
                yRect = 0
                penCount = 30
                For j = 30 To 39
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(605, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(635, 675 + 30 * yRect))

                    If penCount < 39 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next
                dc.DrawRectangle(Windows.Media.Brushes.White, Nothing, New Rect(0, 1, 800, 17))
                dc.DrawText(New FormattedText("NOTE: All aircrafts are included in the CDF calculations. For clarity only 40 are displayed.", CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 12, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(5, 1000))

            ElseIf tmpAPList.Count <= 40 And tmpAPList.Count > 30 Then
                yRect = 0
                penCount = 30
                For j = 30 To tmpAPList.Count - 1
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(605, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(635, 675 + 30 * yRect))

                    If penCount < 39 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next

            End If

            If tmpAPList.Count > 30 Then
                yRect = 0
                penCount = 20
                For j = 20 To 29
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(405, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(435, 675 + 30 * yRect))

                    If penCount < 29 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next
            ElseIf tmpAPList.Count <= 30 And tmpAPList.Count > 20 Then
                yRect = 0
                penCount = 20
                For j = 20 To tmpAPList.Count - 1
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(405, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(435, 675 + 30 * yRect))

                    If penCount < 29 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next
            End If

            If tmpAPList.Count - 1 > 20 Then
                yRect = 0
                penCount = 10
                For j = 10 To 19
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(205, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(235, 675 + 30 * yRect))

                    If penCount < 19 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next
            ElseIf tmpAPList.Count <= 20 And tmpAPList.Count > 10 Then
                yRect = 0
                penCount = 10
                For j = 10 To tmpAPList.Count - 1
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(205, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(235, 675 + 30 * yRect))

                    If penCount < 19 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next
            End If

            If tmpAPList.Count > 10 Then
                penCount = 0
                yRect = 0
                For j = 0 To 9
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(5, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(35, 675 + 30 * yRect))

                    If penCount < 9 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next
            Else
                penCount = 0
                yRect = 0
                For j = 0 To tmpAPList.Count - 1
                    dc.DrawRectangle(colorPalette(penCount), Nothing, New Rect(5, 680 + 30 * yRect, 20, 3))
                    dc.DrawText(New FormattedText(tmpAPList(j).Name, CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, New Typeface("Verdana"), 11, New SolidColorBrush(Colors.Black), 96D), New Windows.Point(35, 675 + 30 * yRect))

                    If penCount < 9 Then
                        penCount += 1
                        yRect += 1
                    Else
                        penCount = 0
                        yRect = 0
                    End If
                Next
            End If

            dc.Close()
            rtb.Render(dv)

            Dim strm As MemoryStream = New MemoryStream()
            Dim enc As BitmapEncoder = New PngBitmapEncoder()
            enc.Frames.Add(BitmapFrame.Create(rtb))
            enc.Save(strm)
            bmp = New Bitmap(strm)

            Return bmp
        End Function


        'Draw Gear
        Public Function DrawGear() As BitmapImage

            Dim ms As New MemoryStream
            Dim bi As New BitmapImage

            Const logicalW As Integer = 640
            Const logicalH As Integer = 640
            Const ssScale As Integer = 3  ' 3x supersampling for crisp text

            Dim hiRes As New Bitmap(logicalW * ssScale, logicalH * ssScale)

            Using g = Graphics.FromImage(hiRes)
                g.ScaleTransform(ssScale, ssScale)
                Dim tempPictureBox As New System.Windows.Forms.PictureBox With {
                        .Height = logicalH,
                        .Width = logicalW
                    }

                If SelectedAirplane IsNot Nothing Then
                    Using fontProfile As New System.Drawing.Font("Segoe UI", 10, FontStyle.Italic)
                        PaintGear(g, SelectedAirplane, tempPictureBox, fontProfile, CurrentJob.DesignOptions.MeasurementSystem)
                    End Using
                End If
            End Using

            ' Downscale with high-quality bicubic interpolation (SupersampleBitmap disposes hiRes)
            Using finalImage As Bitmap = SupersampleBitmap(hiRes, logicalW, logicalH)
                finalImage.Save(ms, Imaging.ImageFormat.Png)
            End Using

            bi.BeginInit()
            bi.StreamSource = ms
            bi.EndInit()

            Return bi
        End Function


        Public Function DrawUserDefinedGear() As BitmapImage

            Dim bi As New BitmapImage

            Using image As New Bitmap(640, 640),
                  g As Graphics = Graphics.FromImage(image),
                  tempPictureBox As New System.Windows.Forms.PictureBox() With {
                      .Height = 640,
                      .Width = 640
                  },
                  ms As New MemoryStream()

                If CurrentSectionView IsNot Nothing AndAlso CurrentWheel IsNot Nothing Then
                    Using fontProfile As New System.Drawing.Font("Segoe UI", 10, FontStyle.Italic)
                        PaintUserDefinedGear(g, CurrentWheel, CurrentEval, tempPictureBox, fontProfile, CurrentJob.DesignOptions.MeasurementSystem, _UDTs)
                    End Using
                End If

                image.Save(ms, Imaging.ImageFormat.Png)
                ms.Position = 0
                bi.BeginInit()
                bi.CacheOption = BitmapCacheOption.OnLoad
                bi.StreamSource = ms
                bi.EndInit()
                bi.Freeze()
            End Using

            Return bi
        End Function


        Public Function DrawProfile() As BitmapImage

            Dim bi As New BitmapImage

            Using image As New Bitmap(640, 640),
                  g As Graphics = Graphics.FromImage(image),
                  tempPictureBox As New System.Windows.Forms.PictureBox() With {
                      .Height = 640,
                      .Width = 640
                  },
                  ms As New MemoryStream()

                If CurrentSectionView IsNot Nothing Then
                    Using fontProfile As New System.Drawing.Font("Segoe UI", 11, FontStyle.Italic)
                        Paint(g, CurrentSectionView.Section.Layers, tempPictureBox, fontProfile, _brushes, CurrentJob.DesignOptions.MeasurementSystem)
                    End Using
                End If

                Dim totalThickness As Single = CurrentSectionView.Section.Layers.Sum(Function(l) l.Thickness.UsCustomary)

                Dim ratio As Single = (640) / (totalThickness + 10)
                Dim line As Single = ratio * 10

                For i = 0 To CurrentLayers.Count - 1
                    CurrentLayers(i).ButtonHeight = ratio * (CurrentLayers(i).Thickness.UsCustomary)
                Next

                StructureListView_Margin = New System.Windows.Thickness(0, line, 0, 0)

                image.Save(ms, Imaging.ImageFormat.Png)
                ms.Position = 0
                bi.BeginInit()
                bi.CacheOption = BitmapCacheOption.OnLoad
                bi.StreamSource = ms
                bi.EndInit()
                bi.Freeze()
            End Using

            Return bi
        End Function


        Private Sub InsertVersionInfo(path As String)
            Dim libFile As String

            Using reader As New StreamReader(path)
                libFile = reader.ReadToEnd
            End Using

            Dim closeRootPos As Integer
            closeRootPos = InStr(1, libFile, ">", CompareMethod.Text)

            Dim versionAttributes As String = " LibraryVersion=" & ControlChars.Quote & LibraryVersion.ToString() & ControlChars.Quote & " SoftwareVersion=" & ControlChars.Quote & SoftwareVersion.ToString() & ControlChars.Quote

            Dim libFileWithAttribs As String = libFile.Insert(closeRootPos - 1, versionAttributes)

            libFile = libFileWithAttribs

            Using writer As New StreamWriter(path)
                writer.Write(libFile)
            End Using
        End Sub

#End Region

    End Class

End Namespace