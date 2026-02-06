Namespace Interfaces
    Public Interface IJobInformation
        Property LocationIdentifier As String
        Property State  As String
        Property City As String
        Property Country As String
        Property Airport As String
        Property AipProjectNumber As String
        Property Sponsor As String
        Property DesignEngineer As String
        Property Description As String
        Property FrostDepth As IDimensionalProperty
        Property FrostDepthValdiation As IValidationRange
        Property SurfaceDrainage As String
        Property FrostDesign As String
        Property Comments As String
        Property Network As String
        Property Branch As String
        Property Section As String
        Property Factory As IFaarFieldModelFactory




    End Interface
End NameSpace