Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports FF2.Utilities
Imports FaarFieldModel.Interfaces

Namespace ViewModels
    Public Class JobInformationViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        ReadOnly _job As IFaarFieldJob
        Dim _isExpanded As Boolean
        Dim _isSelected As Boolean

        Public ReadOnly Property Name As String Implements ITreeViewItemViewModel.Name
        Private ReadOnly Property vm As MainWindowViewModel

        'Private ReadOnly Property Report5100 As JobInformationReportViewModel
        'Private ReadOnly Property ReportSummary As SummaryReportViewModel

        Public Sub New(job As IFaarFieldJob, parentJob As ITreeViewItemViewModel, viewModel As MainWindowViewModel)
            _job = job
            vm = viewModel
            Parent = parentJob
            Name = "Job Information"
            'Children = New ObservableCollection(Of ITreeViewItemViewModel)
            Children = Nothing

        End Sub




        'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        '' This method is called by the Set accessor of each property.
        '' The CallerMemberName attribute that is applied to the optional propertyName
        '' parameter causes the property name of the caller to be substituted as an argument.
        'Private Sub OnPropertyChanged(<CallerMemberName()> Optional ByVal info As String = Nothing)
        '    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
        'End Sub

        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent

        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children
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
                        vm.OnJobInformation(CType(Parent, JobViewModel))
                        'vm.SetCurrentJob(CType(Parent, JobViewModel))
                    End If
                End If
            End Set
        End Property
    End Class

End Namespace