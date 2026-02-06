Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports FaarFieldAnalysis
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports System.Drawing.Font
Imports FF2.Utilities

Namespace ViewModels
    Public Class ReportViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        Dim m_isExpanded As Boolean
        Dim m_isSelected As Boolean

        Private Property FaarFieldViewModel As MainWindowViewModel
        Property RunAnalysis As RunAnalysis
        Public ReadOnly Property Section As ISection
        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent
        Public Property Name As String Implements ITreeViewItemViewModel.Name
        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children

        Sub New(section As ISection, sectionView As SectionViewModel, viewModel As MainWindowViewModel)
            Parent = sectionView
            Name = "Structure Report"
            IsExpanded = True
            FaarFieldViewModel = viewModel
            'OnPropertyChanged(NameOf(Jobs))
        End Sub

        Public Sub ChangeName(changedName As String)
            Name = changedName
            OnPropertyChanged(NameOf(Name))
        End Sub

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
                    If Value Then
                        Dim sectionView = CType(Parent, SectionViewModel)
                        FaarFieldViewModel.SetCurrentSection(sectionView)
                        FaarFieldViewModel.SectionReportIsHidden = True
                        FaarFieldViewModel.SectionReportIsHidden = False
                        FaarFieldViewModel.SectionReportHtml = FaarFieldViewModel.refreshsectionreport()
                    End If
                End If
            End Set
        End Property
    End Class
End Namespace