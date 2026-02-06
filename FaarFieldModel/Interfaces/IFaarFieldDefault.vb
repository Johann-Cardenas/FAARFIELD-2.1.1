Imports System.Collections.ObjectModel

Namespace Interfaces
    Public Interface IFaarFieldDefault

        Property Materials As ObservableCollection(Of IMaterial)

        Property Aircraft As List(Of IAircraft)


    End Interface
End Namespace