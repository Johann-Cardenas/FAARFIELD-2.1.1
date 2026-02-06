Imports System.ComponentModel

Namespace ViewModels
    Public Interface IListBoxItemViewModel
        Inherits INotifyPropertyChanged
        Property IsSelected As Boolean
        Sub OnDoubleClick()
    End Interface
End Namespace