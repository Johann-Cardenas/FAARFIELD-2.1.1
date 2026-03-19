Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Runtime.CompilerServices
Imports FaarFieldAnalysis
Imports FaarFieldModel.Interfaces
Imports FF2.Utilities

Namespace ViewModels
    Public Class DetailedReportViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        Dim _isExpanded As Boolean
        Dim _isSelected As Boolean
        Private Property FaarFieldViewModel As MainWindowViewModel

        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent
        Public Property Name As String Implements ITreeViewItemViewModel.Name
        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children

        Sub New(section As ISection, sectionView As SectionViewModel, viewModel As MainWindowViewModel)
            Parent = sectionView
            Name = "CM Report"
            IsExpanded = True
            FaarFieldViewModel = viewModel
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
                        Try
                            Dim sectionview = CType(Parent, SectionViewModel)
                            If FaarFieldViewModel.CurrentSectionView IsNot sectionview Then
                                FaarFieldViewModel.SetCurrentSection(sectionview)
                            End If
                            Dim html = FaarFieldViewModel.refreshDetailedReport()
                            If html <> FaarFieldViewModel.DetailedReportHtml Then
                                FaarFieldViewModel.DetailedReportIsHidden = True
                                FaarFieldViewModel.DetailedReportHtml = html
                            End If
                            FaarFieldViewModel.DetailedReportIsHidden = False
                        Catch ex As Exception
                            Debug.WriteLine("DetailedReportViewModel.IsSelected error: " & ex.Message)
                            FaarFieldViewModel.DetailedReportIsHidden = False
                            FaarFieldViewModel.DetailedReportHtml = FaarFieldViewModel.CreateDetailedReportErrorPage(ex.Message)
                        End Try
                    End If
                End If
            End Set
        End Property
    End Class
End Namespace
