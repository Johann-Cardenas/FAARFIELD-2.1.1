Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization.Json
Imports System.Security.Cryptography
Imports System.Text
Imports FaarFieldModel.Interfaces
<DataContract>
<KnownType(GetType(AirplaneInfo))>
Public Class SavedTrafficLibrary

    <DataMember>
    Public Property Airplanes As List(Of IAirplaneInfo)

    Public Sub New()

    End Sub
End Class

