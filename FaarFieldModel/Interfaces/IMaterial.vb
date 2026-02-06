Namespace Interfaces
    Public Interface IMaterial
        Property Category As String
        Property Name As String
        Property Thickness As Thickness
        Property Cbr As Double
        Property Modulus As Modulus
        Property SubgradeReaction As SubgradeReaction
        Property Note As String
        Property IsUserDefined As Boolean
        Property CanDelete As Boolean
        Property LayerCode As Integer
        Property CBRActive As Boolean
        Property KValueActive As Boolean
        Property PCAConversionActive As Boolean
        Property NCHRPActive As Boolean
        Property ModulusActive As Boolean
        Property ThicknessActive As Boolean
        Property RuptureActive As Boolean
        Property Rupture As Modulus
        Property DesignedLayer As String
        Property OnCheckedDesignLayer As Boolean
        Property ButtonHeight As Integer
    End Interface
End NameSpace