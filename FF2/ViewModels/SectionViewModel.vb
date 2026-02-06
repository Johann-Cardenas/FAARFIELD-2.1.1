Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports FaarFieldAnalysis
Imports FaarFieldModel.Interfaces
Imports FF2.Utilities

Namespace ViewModels
    Public Class SectionViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        Dim _isExpanded As Boolean
        Dim _isSelected As Boolean
        Private Property FaarFieldViewModel As MainWindowViewModel
        Public ReadOnly Property Section As ISection
        'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        '' This method is called by the Set accessor of each property.
        '' The CallerMemberName attribute that is applied to the optional propertyName
        '' parameter causes the property name of the caller to be substituted as an argument.
        'Private Sub OnPropertyChanged(<CallerMemberName()> Optional ByVal info As String = Nothing)
        '    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
        'End Sub

        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent
        Public Property Name As String Implements ITreeViewItemViewModel.Name

        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children

        Public ReadOnly Property Report As ReportViewModel
        Public ReadOnly Property ReportCDFGraph As CDFGraphViewModel
        Public ReadOnly Property ReportPCR As ReportPCRViewModel
        Public ReadOnly Property PCRGraph As GraphPCN
        Public ReadOnly Property AirportMasterRecord As AirportMasterRecord

        Public ReadOnly Property SummaryReport As SummaryReportViewModel

        Sub New(section As ISection, sectionFolder As SectionFolderViewModel, viewModel As MainWindowViewModel, summaryReport As SummaryReportViewModel)
            Me.Section = section
            Parent = sectionFolder
            Name = section.Name
            FaarFieldViewModel = viewModel
            Children = New ObservableCollection(Of ITreeViewItemViewModel)

            Report = New ReportViewModel(section, Me, FaarFieldViewModel)
            Children.Add(Report)

            ReportCDFGraph = New CDFGraphViewModel(section, Me, FaarFieldViewModel)
            Children.Add(ReportCDFGraph)
            Me.SummaryReport = summaryReport

            ReportPCR = New ReportPCRViewModel(section, Me, FaarFieldViewModel)
            Children.Add(ReportPCR)

            PCRGraph = New GraphPCN(section, Me, FaarFieldViewModel)
            Children.Add(PCRGraph)

            AirportMasterRecord = New AirportMasterRecord(section, Me, FaarFieldViewModel)
            Children.Add(AirportMasterRecord)

            Me.FaarFieldViewModel = FaarFieldViewModel

            'OnPropertyChanged(NameOf(Jobs))

        End Sub

        Public Sub ChangeName(changedName As String)
            Name = changedName
            OnPropertyChanged(NameOf(Name))
        End Sub

        Public Property IsExpanded As Boolean Implements ITreeViewItemViewModel.IsExpanded
            Get
                Return _isExpanded
            End Get
            Set

                If Value <> _isExpanded Then

                    _isExpanded = Value
                    OnPropertyChanged(NameOf(IsExpanded))
                End If

                ' Expand all the way up to the root.
                If IsExpanded And Parent IsNot Nothing Then
                    Parent.IsExpanded = True
                End If
            End Set

        End Property

        Public Property IsSelected As Boolean Implements ITreeViewItemViewModel.IsSelected
            Get
                Return _isSelected
            End Get
            Set
                If Value <> IsSelected Then
                    _isSelected = Value
                    OnPropertyChanged(NameOf(IsSelected))
                    If Value Then
                        FaarFieldViewModel.SetCurrentSection(Me)
                    End If
                End If
            End Set
        End Property
    End Class

End Namespace
