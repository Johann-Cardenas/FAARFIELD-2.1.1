Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports FF2.Utilities
Imports FaarFieldModel.Interfaces

Namespace ViewModels
    Public Class SectionFolderViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel


        Dim m_isExpanded As Boolean
        Dim m_isSelected As Boolean

        Private ReadOnly Property FaarFieldViewModel As MainWindowViewModel
        Public ReadOnly Property Name As String Implements ITreeViewItemViewModel.Name
        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent
        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children
        ReadOnly _job As IFaarFieldJob
        Private m_summaryReport As SummaryReportViewModel

        Public Sub New(job As IFaarFieldJob, parentJob As ITreeViewItemViewModel, faarFieldViewModel As MainWindowViewModel, summaryReport As SummaryReportViewModel)
            _job = job
            Parent = parentJob
            Name = "Structures"
            Children = New ObservableCollection(Of ITreeViewItemViewModel)()
            m_summaryReport = summaryReport

            Me.FaarFieldViewModel = faarFieldViewModel

            For Each section In job.Sections
                AddSection(section)
            Next

            IsExpanded = True
            OnPropertyChanged(NameOf(IsExpanded))
        End Sub

        Public Function AddSection(section As ISection) As SectionViewModel
            Dim sectionView = New SectionViewModel(section, Me, FaarFieldViewModel, m_summaryReport)
            Children.Add(sectionView)
            Return sectionView
        End Function

        Public Property IsExpanded As Boolean Implements ITreeViewItemViewModel.IsExpanded
            Get
                Return m_isExpanded
            End Get
            Set
                If Value <> m_isExpanded Then
                    m_isExpanded = Value
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
                Return m_isSelected
            End Get
            Set
                If Value <> IsSelected Then
                    m_isSelected = Value
                    OnPropertyChanged(NameOf(IsSelected))
                End If
            End Set
        End Property
    End Class
End Namespace