Imports FaarFieldModel.Interfaces

Public Enum Analysis
    Rigid = -1
    Both = 0
    Flexible = 1
End Enum

Public Class MaterialDefault
    Implements IMaterial
    Implements IMaterialValidation

    Public Property ValidFor As Integer Implements IMaterialValidation.ValidFor
    Public Property Category As String Implements IMaterial.Category
    Public Property Name As String Implements IMaterial.Name
    Public Property Thickness As Thickness Implements IMaterial.Thickness
    Public Property ThicknessValidation As IValidationRange Implements IMaterialValidation.ThicknessValidation
    Public Property SubgradeReaction As SubgradeReaction Implements IMaterial.SubgradeReaction
    Public Property SubgradeReactionValidation As IValidationRange Implements IMaterialValidation.SubgradeReactionValidation
    Public Property Modulus As Modulus Implements IMaterial.Modulus
    Public Property ModulusValidation As IValidationRange Implements IMaterialValidation.ModulusValidation
    Public Property Cbr As Double Implements IMaterial.Cbr
    Public Property CbrValidation As IValidationRange Implements IMaterialValidation.CbrValidation
    Public Property AllowedTop As Boolean Implements IMaterialValidation.AllowedTop
    Public Property AllowedBottom As Boolean Implements IMaterialValidation.AllowedBottom
    Public Property AllowedModulusEdit As Boolean Implements IMaterialValidation.AllowedModulusEdit
    Public Property IsUserDefined As Boolean Implements IMaterial.IsUserDefined
    Public Property Note As String Implements IMaterial.Note
    Public Property CanDelete As Boolean Implements IMaterial.CanDelete
    Public Property LayerCode As Integer Implements IMaterial.LayerCode
    Public Property CBRActive As Boolean Implements IMaterial.CBRActive
    Public Property KValueActive As Boolean Implements IMaterial.KValueActive
    Public Property PCAConversionActive As Boolean Implements IMaterial.PCAConversionActive
    Public Property NCHRPActive As Boolean Implements IMaterial.NCHRPActive
    Public Property ModulusActive As Boolean Implements IMaterial.ModulusActive
    Public Property ThicknessActive As Boolean Implements IMaterial.ThicknessActive
    Public Property RuptureActive As Boolean Implements IMaterial.RuptureActive
    Public Property Rupture As Modulus Implements IMaterial.Rupture
    Public Property DesignedLayer As String Implements IMaterial.DesignedLayer
    Public Property OnCheckedDesignLayer As Boolean Implements IMaterial.OnCheckedDesignLayer

    Public Overrides Function ToString() As String
        Return Name
    End Function

    Public Property ButtonHeight As Integer Implements IMaterial.ButtonHeight

End Class
