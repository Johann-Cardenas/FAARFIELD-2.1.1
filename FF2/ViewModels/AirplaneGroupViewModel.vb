Imports System.ComponentModel
Imports System.Runtime.CompilerServices
Imports FF2.Utilities
Imports FaarFieldModel.Interfaces

Namespace ViewModels

    Public Class AirplaneGroupViewModel
        Inherits ViewModelBase
        Implements IListBoxItemViewModel

        Dim m_isSelected As Boolean
        Public ReadOnly Property Manufacturer As String
        ReadOnly Property FaarFieldViewModel As MainWindowViewModel

        Public Sub New(info As IAirplaneInfo, viewModel As MainWindowViewModel)
            Manufacturer = info.Manufacturer
            FaarFieldViewModel = viewModel
        End Sub

        Public Property IsSelected As Boolean Implements IListBoxItemViewModel.IsSelected
            Get
                Return IsSelected
            End Get
            Set
                If Value <> IsSelected Then
                    m_isSelected = Value
                    OnPropertyChanged(NameOf(IsSelected))
                    If m_isSelected Then
                        FaarFieldViewModel.UpdateAirplaneByManufacturer(Manufacturer)
                    End If
                End If
            End Set
        End Property

        Public Sub OnDoubleClick() Implements IListBoxItemViewModel.OnDoubleClick
        End Sub

    End Class
End Namespace