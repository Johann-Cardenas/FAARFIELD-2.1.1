Imports System.Collections.ObjectModel

Namespace Interfaces
    Public Interface IModelSectionAction
        Function Validate(defaults As ObservableCollection(Of MaterialDefault), measurementSystem As IMeasurmentSystem) As List(Of IValidation)
    End Interface
End Namespace