Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports FF2.Utilities
Imports FaarFieldModel.Interfaces

Namespace ViewModels
    Public Class AirplaneByManufacturerViewModel
        Inherits ViewModelBase
        Implements IListBoxItemViewModel

        Dim _isSelected As Boolean
        Public ReadOnly Property Name As String
        Public ReadOnly Property Airplane As IAirplaneInfo
        ReadOnly Property FaarFieldViewModel As MainWindowViewModel

        Public Sub New(info As IAirplaneInfo, viewModel As MainWindowViewModel)
            Name = info.Name
            Airplane = info
            FaarFieldViewModel = viewModel
        End Sub

        Public Property IsSelected As Boolean Implements IListBoxItemViewModel.IsSelected
            Get
                Return _isSelected
            End Get
            Set
                If Value <> IsSelected Then
                    _isSelected = Value
                    OnPropertyChanged(NameOf(IsSelected))
                    If _isSelected Then
                    End If
                End If
            End Set
        End Property

        Public Sub OnDoubleClick() Implements IListBoxItemViewModel.OnDoubleClick

        End Sub
    End Class
End Namespace