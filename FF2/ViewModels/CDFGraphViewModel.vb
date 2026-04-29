Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports FF2.Utilities
Imports FaarFieldAnalysis
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Namespace ViewModels
    Public Class CDFGraphViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        Dim _isExpanded As Boolean
        Dim _isSelected As Boolean
        Private Property FaarFieldViewModel As MainWindowViewModel
        Property RunAnalysis As RunAnalysis
        Public ReadOnly Property Section As ISection

        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent
        Public Property Name As String Implements ITreeViewItemViewModel.Name

        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children

        Private ReadOnly _job As IFaarFieldJob

        Sub New(section As ISection, sectionView As SectionViewModel, viewModel As MainWindowViewModel)
            Parent = sectionView
            Name = "CDF Graph"
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
                ' Idempotent on True so re-clicking the tree node after the user closed the
                ' pane via its × button reopens the tab. The False→True transition still fires
                ' OnPropertyChanged for the tree-view binding.
                Dim transitioning As Boolean = (Value <> _isSelected)
                _isSelected = Value
                If transitioning Then OnPropertyChanged(NameOf(IsSelected))
                If Value Then
                    Dim sectionview = CType(Parent, SectionViewModel)
                    If FaarFieldViewModel.CurrentSectionView IsNot sectionview Then
                        FaarFieldViewModel.SetCurrentSection(sectionview)
                    End If
                    Dim html = FaarFieldViewModel.refreshcdfgraph()
                    If html <> FaarFieldViewModel.CDFGraphHtml Then
                        FaarFieldViewModel.CDFGraphIsHidden = True
                        FaarFieldViewModel.CDFGraphHtml = html
                    End If
                    FaarFieldViewModel.CDFGraphIsHidden = False
                End If
            End Set
        End Property
    End Class
End Namespace