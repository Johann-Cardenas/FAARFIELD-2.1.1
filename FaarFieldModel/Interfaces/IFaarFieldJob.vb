Namespace Interfaces
    Public Interface IFaarFieldJob
        Property Name As String
        Property JobInformation As IJobInformation
        Property Sections As List(Of ISection)
        Property DesignOptions As IDesignOptions

        Property Version_1_4_File As Boolean


    End Interface
End Namespace