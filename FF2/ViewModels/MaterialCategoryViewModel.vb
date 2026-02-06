Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports FF2.Utilities
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Namespace ViewModels

    Public Class MaterialCategoryViewModel
        Inherits ViewModelBase
        Implements ITreeViewItemViewModel

        Public ReadOnly FaarFieldFactory As IFaarFieldModelFactory


        Public ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel) Implements ITreeViewItemViewModel.Children

        Public ReadOnly Property Parent As ITreeViewItemViewModel Implements ITreeViewItemViewModel.Parent
        Public ReadOnly Property Name As String Implements ITreeViewItemViewModel.Name

        Dim _isExpanded As Boolean = True
        Dim _isSelected As Boolean


        Sub New(factory As IFaarFieldModelFactory, materials As ObservableCollection(Of MaterialDefault))
            FaarFieldFactory = factory
            Parent = Nothing
            Name = materials(0).Category
            Children = New ObservableCollection(Of ITreeViewItemViewModel)()

            For Each material In materials
                Children.Add(New MaterialViewModel(factory, material, Me))
            Next


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