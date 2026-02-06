Imports System.Collections.ObjectModel

Namespace Interfaces

    Public Class FaarFieldDefault
        Implements IFaarFieldDefault

        Public Property Materials As ObservableCollection(Of IMaterial) Implements IFaarFieldDefault.Materials
        Public Property Aircraft As List(Of IAircraft) Implements IFaarFieldDefault.Aircraft

        ''' <summary>
        ''' This is the constructor for the FaarFieldDefault class.
        ''' </summary>
        ''' <param name="materialPath">This is the path where the materials are located.</param>
        ''' <param name="aircraftPath">This is the path where the aircrafts are located.</param>
        Public Sub New(materialPath As String, aircraftPath As String)

        End Sub

        Private Sub LoadMaterials(materialPath As String)
            If (String.IsNullOrWhiteSpace(materialPath)) Then
                ''If no material path is given load defaults internally
                MaterialsFromCode()
            Else
                ''Otherwise deserialize JSON default file
                Throw New NotImplementedException()
            End If
        End Sub

        Private Sub MaterialsFromCode()
            Materials = New ObservableCollection(Of IMaterial)()
        End Sub
    End Class
End Namespace