Imports System.Collections.ObjectModel
Imports System.ComponentModel

Namespace ViewModels
    Public Interface ITreeViewItemViewModel
        Inherits INotifyPropertyChanged
        ReadOnly Property Children As ObservableCollection(Of ITreeViewItemViewModel)
        Property IsExpanded As Boolean
        Property IsSelected As Boolean
        ReadOnly Property Parent As ITreeViewItemViewModel
        ReadOnly Property Name As String
    End Interface
End Namespace