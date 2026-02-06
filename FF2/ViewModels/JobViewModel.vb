Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports FF2.Utilities
Imports FaarFieldModel.Interfaces

Namespace ViewModels
    Public Class JobViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel
        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children
        'Public Property IsExpanded As Boolean Implements ITreeViewItemViewModel.IsExpanded
        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent

        Public Property Name As String Implements ITreeViewItemViewModel.Name
        Public Property SavePath As String
        Public ReadOnly Property FaarFieldViewModel As MainWindowViewModel

        Public ReadOnly Property SectionFolder As SectionFolderViewModel


        Public ReadOnly Property FaarFieldJob As IFaarFieldJob
        Public ReadOnly Property FaarFieldFactory As IFaarFieldModelFactory
        Dim _isExpanded As Boolean
        Dim _isSelected As Boolean

        Sub New(factory As IFaarFieldModelFactory, job As IFaarFieldJob, faarFieldViewModel As MainWindowViewModel)
            FaarFieldFactory = factory
            FaarFieldJob = job
            Parent = Nothing
            Children = New ObservableCollection(Of ITreeViewItemViewModel)()
            Name = job.Name
            Children.Add(New JobInformationViewModel(job, Me, faarFieldViewModel))
            Children.Add(New DesignOptionsViewModel(job, Me, faarFieldViewModel))
            Dim summaryReport = New SummaryReportViewModel(job, Me, faarFieldViewModel)
            Children.Add(summaryReport)
            SectionFolder = New SectionFolderViewModel(job, Me, faarFieldViewModel, summaryReport)
            Children.Add(SectionFolder)

            Me.FaarFieldViewModel = faarFieldViewModel

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

        Public Sub ChangeName(changedName As String)
            Name = changedName
            OnPropertyChanged(NameOf(Name))
        End Sub


        Public Property IsSelected As Boolean Implements ITreeViewItemViewModel.IsSelected
            Get
                Return _isSelected
            End Get
            Set
                If Value <> IsSelected Then
                    _isSelected = Value
                    OnPropertyChanged(NameOf(IsSelected))
                    If Value Then
                        FaarFieldViewModel.SetCurrentJob(Me)
                        OnPropertyChanged(NameOf(Name))
                    End If
                End If
            End Set
        End Property

        'Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

        '' This method is called by the Set accessor of each property.
        '' The CallerMemberName attribute that is applied to the optional propertyName
        '' parameter causes the property name of the caller to be substituted as an argument.
        'Private Sub OnPropertyChanged(<CallerMemberName()> Optional ByVal info As String = Nothing)
        '    RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(info))
        'End Sub

    End Class
End Namespace