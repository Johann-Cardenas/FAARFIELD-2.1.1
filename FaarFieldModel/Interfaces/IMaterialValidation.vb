Namespace Interfaces
    Public Interface IMaterialValidation
        Property ValidFor As Integer
        Property AllowedTop As Boolean
        Property AllowedBottom As Boolean
        Property AllowedModulusEdit As Boolean
        Property ThicknessValidation As IValidationRange
        Property CbrValidation As IValidationRange
        Property ModulusValidation As IValidationRange
        Property SubgradeReactionValidation As IValidationRange
    End Interface
End NameSpace