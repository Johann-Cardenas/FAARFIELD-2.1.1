Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports FaarFieldModel.Interfaces
Imports Microsoft.VisualBasic.FileIO
Imports FaarFieldModel
Imports FaarFieldAnalysis
Imports FF2.Utilities

Namespace ViewModels
    Public Class SummaryReportViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        Private ReadOnly _job As IFaarFieldJob
        Dim _isExpanded As Boolean
        Dim _isSelected As Boolean

        Public ReadOnly Property Name As String Implements ITreeViewItemViewModel.Name
        Private ReadOnly Property ViewModel As MainWindowViewModel
        Property RunAnalysis As RunAnalysis

        Public Sub New(job As IFaarFieldJob, parentJob As ITreeViewItemViewModel, vm As MainWindowViewModel)
            _job = job
            Parent = parentJob
            Name = "Summary Report"
            Children = Nothing
            ViewModel = vm
        End Sub

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
                        Dim jobview = CType(Parent, JobViewModel)
                        If ViewModel.CurrentJobView IsNot jobview Then
                            ViewModel.SetCurrentJob(jobview)
                        End If
                        Dim html = ViewModel.RefreshHtml()
                        If html <> ViewModel.SummaryReportHtml Then
                            ViewModel.SummaryReportIsHidden = True
                            ViewModel.SummaryReportHtml = html
                        End If
                        ViewModel.SummaryReportIsHidden = False
                    End If
                End If
            End Set
        End Property

    End Class
End Namespace

