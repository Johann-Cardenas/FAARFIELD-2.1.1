
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
    Public Class ReportPCRViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        Dim _isExpanded As Boolean
        Dim _isSelected As Boolean
        Dim PCRMAXFinder As Integer

        Public Property MeasurmentSystem As IMeasurmentSystem
        Private Property FaarFieldViewModel As MainWindowViewModel
        Property runanalysis As RunAnalysis
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

        Private ReadOnly _job As IFaarFieldJob
        Sub New(section As ISection, sectionView As SectionViewModel, viewModel As MainWindowViewModel)

            Parent = sectionView

            Name = "PCR Report"
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
                If Value <> IsSelected Then
                    _isSelected = Value
                    OnPropertyChanged(NameOf(IsSelected))
                    If Value Then
                        Dim sectionview = CType(Parent, SectionViewModel)
                        FaarFieldViewModel.SetCurrentSection(sectionview)
                        FaarFieldViewModel.PCRReportIsHidden = True
                        FaarFieldViewModel.PCRReportHtml = FaarFieldViewModel.refreshPCRReport()
                        FaarFieldViewModel.PCRReportIsHidden = False
                    End If
                    End If
            End Set
        End Property


    End Class
End Namespace