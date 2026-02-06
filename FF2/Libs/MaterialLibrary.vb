Imports System.Collections.ObjectModel
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Namespace Libs

    Public Module MaterialLibrary
        Private _usCustomary As IMeasurmentSystem
        Public Function GetMaterials(factory As IFaarFieldModelFactory) As ObservableCollection(Of MaterialDefault)
            _usCustomary = factory.CreateUsCustomary()
            Dim materialDefaults As New ObservableCollection(Of MaterialDefault)
            'materialDefaults.AddRange(GetGeneral(factory))
            'materialDefaults.AddRange(GetAggregate(factory))
            'materialDefaults.AddRange(GetHma(factory))
            'materialDefaults.AddRange(GetPcc(factory))
            'materialDefaults.AddRange(GetStabilized(factory))

            For Each md In GetGeneral(factory)
                materialDefaults.Add(md)
            Next
            For Each md In GetAggregate(factory)
                materialDefaults.Add(md)
            Next
            For Each md In GetHma(factory)
                materialDefaults.Add(md)
            Next
            For Each md In GetPcc(factory)
                materialDefaults.Add(md)
            Next
            For Each md In GetStabilized(factory)
                materialDefaults.Add(md)
            Next

            Return materialDefaults
        End Function


        Private Function GetGeneral(factory As IFaarFieldModelFactory) As ObservableCollection(Of MaterialDefault)
            Dim materialDefaults As New ObservableCollection(Of MaterialDefault)
            Dim material = New MaterialDefault() With {
                .Category = "General",
                .Name = "User Defined",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(6, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Cbr = 66.6666,
                .CbrValidation = factory.CreateValidationRange(0.7, 33, True),
                .SubgradeReaction = factory.CreateSubgradeReaction(755.5, _usCustomary),
                .SubgradeReactionValidation = factory.CreateSubgradeReactionValidationRange(20.9, 13364.9, True, _usCustomary),
                .Modulus = factory.CreateModulus(100000, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(1000, 4000000, True, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = True,
                .IsUserDefined = True,
                .AllowedModulusEdit = True,
                .LayerCode = 0,
                .CBRActive = True,
                .KValueActive = True,
                .ModulusActive = True,
                .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)


            ' .Name = "Subgrade",

            material = New MaterialDefault() With {
                .Category = "General",
                .Name = "Subgrade",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(12, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Cbr = 10.0,
                .CbrValidation = factory.CreateValidationRange(0.7, 33, True),
                .SubgradeReaction = factory.CreateSubgradeReaction(172.4, _usCustomary),
                .SubgradeReactionValidation = factory.CreateSubgradeReactionValidationRange(20.9, 440.4, True, _usCustomary),
                .Modulus = factory.CreateModulus(15000, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(1000, 50000, True, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = True,
                .IsUserDefined = True,
                .AllowedModulusEdit = True,
                .LayerCode = 4,
                .CBRActive = True,
                .KValueActive = True,
                .ModulusActive = True,
                .ThicknessActive = False,
                .RuptureActive = False
            }
            materialDefaults.Add(material)
            Return materialDefaults
        End Function


        Private Function GetAggregate(factory As IFaarFieldModelFactory) As IEnumerable(Of MaterialDefault)
            Dim materialDefaults As New ObservableCollection(Of MaterialDefault)

            Dim material = New MaterialDefault() With {
                .Category = "Aggregate",
                .Name = "P-154 Uncrushed Aggregate",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(6, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(40000, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = False,
                .LayerCode = 8,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Aggregate",
                .Name = "P-208 Crushed Aggregate",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(6, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(75000, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = False,
                .LayerCode = 18,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Aggregate",
                .Name = "P-209 Crushed Aggregate",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(6, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(75000, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = False,
                .LayerCode = 6,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Aggregate",
                .Name = "P-211 Lime Rock",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(6, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(60000, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = False,
                .LayerCode = 21,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Aggregate",
                .Name = "P-219 Recycled Concrete Aggregate",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(6, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(75000, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = False,
                .LayerCode = 19,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)
            Return materialDefaults
        End Function


        Private Function GetHma(factory As IFaarFieldModelFactory) As IEnumerable(Of MaterialDefault)
            Dim materialDefaults As New ObservableCollection(Of MaterialDefault)
            Dim material = New MaterialDefault() With {
                    .Category = "P-401/P-403 HMA",
                    .Name = "P-401/P-403 HMA Surface",
                    .ValidFor = Analysis.Both,
                    .Thickness = factory.CreateThickness(4, _usCustomary),
                    .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                    .Modulus = factory.CreateModulus(200000, _usCustomary),
                    .AllowedTop = True,
                    .AllowedBottom = False,
                    .IsUserDefined = False,
                    .AllowedModulusEdit = False,
                    .LayerCode = 1,
                    .CBRActive = False,
                    .KValueActive = False,
                    .ModulusActive = False,
                    .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                    .Category = "P-401/P-403 HMA",
                    .Name = "P-401/P-403 HMA Overlay",
                    .ValidFor = Analysis.Both,
                    .Thickness = factory.CreateThickness(6, _usCustomary),
                    .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                    .Modulus = factory.CreateModulus(200000, _usCustomary),
                    .AllowedTop = True,
                    .AllowedBottom = False,
                    .IsUserDefined = False,
                    .AllowedModulusEdit = False,
                    .LayerCode = 10,
                    .CBRActive = False,
                    .KValueActive = False,
                    .ModulusActive = False,
                    .ThicknessActive = True,
                .RuptureActive = False
            }
            materialDefaults.Add(material)
            Return materialDefaults
        End Function


        Private Function GetPcc(factory As IFaarFieldModelFactory) As IEnumerable(Of MaterialDefault)
            Dim materialDefaults As New ObservableCollection(Of MaterialDefault)
            Dim material = New MaterialDefault() With {
                    .Category = "P-501 PCC",
                    .Name = "P-501 PCC Surface",
                    .ValidFor = Analysis.Both,
                    .Thickness = factory.CreateThickness(12, _usCustomary),
                    .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                    .Modulus = factory.CreateModulus(4000000, _usCustomary),
                    .Rupture = factory.CreateModulus(650, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(300000, 5000000, True, _usCustomary),
                    .AllowedTop = True,
                    .AllowedBottom = False,
                    .IsUserDefined = False,
                    .AllowedModulusEdit = True,
                    .LayerCode = 5,
                    .CBRActive = False,
                    .KValueActive = False,
                    .ModulusActive = False,
                    .ThicknessActive = True,
                .RuptureActive = True
          }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "P-501 PCC",
                .Name = "P-501 PCC Overlay (unbonded)",
                .ValidFor = Analysis.Rigid,
                .Thickness = factory.CreateThickness(12, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(4000000, _usCustomary),
                .Rupture = factory.CreateModulus(650, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(300000, 5000000, True, _usCustomary),
                .AllowedTop = True,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = True,
                .LayerCode = 11,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = True
                }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "P-501 PCC",
                .Name = "P-501 PCC Overlay (partially bonded)",
                .ValidFor = Analysis.Rigid,
                .Thickness = factory.CreateThickness(12, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(4000000, _usCustomary),
                .Rupture = factory.CreateModulus(650, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(300000, 5000000, True, _usCustomary),
                .AllowedTop = True,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = True,
                .LayerCode = 12,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = True
                }
            'If TreeNodeJob.FaarFieldJob.DesignOptions.AllowPartiallyBonded = False Then
            materialDefaults.Add(material)
            ' End If

            material = New MaterialDefault() With {
                .Category = "P-501 PCC",
                .Name = "P-501 PCC Overlay on Flexible",
                .ValidFor = Analysis.Rigid,
                .Thickness = factory.CreateThickness(15, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(4000000, _usCustomary),
                .Rupture = factory.CreateModulus(650, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(300000, 5000000, True, _usCustomary),
                .AllowedTop = True,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = True,
                .LayerCode = 13,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = True
                }
            materialDefaults.Add(material)

            Return materialDefaults
        End Function

        Private Function GetStabilized(factory As IFaarFieldModelFactory) As IEnumerable(Of MaterialDefault)
            Dim materialDefaults As New ObservableCollection(Of MaterialDefault)
            Dim material = New MaterialDefault() With {
                    .Category = "Stabilized",
                    .Name = "P-301 Soil Cement Base",
                    .ValidFor = Analysis.Rigid,
                    .Thickness = factory.CreateThickness(6, _usCustomary),
                    .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                    .Modulus = factory.CreateModulus(250000, _usCustomary),
                    .AllowedTop = False,
                    .AllowedBottom = False,
                    .IsUserDefined = False,
                    .AllowedModulusEdit = True,
                    .LayerCode = 15,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
                    }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Stabilized",
                .Name = "P-304 Cement Treated Base",
                .ValidFor = Analysis.Rigid,
                .Thickness = factory.CreateThickness(5, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(500000, _usCustomary),
                 .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = True,
                .LayerCode = 16,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
                }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Stabilized",
                .Name = "P-306 Lean Concrete",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(5, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(700000, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = True,
                .LayerCode = 17,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
                }
            materialDefaults.Add(material)

            'material = New MaterialDefault() With {
            '    .Category = "Stabilized",
            '    .Name = "P-306 Lean Concrete",
            '    .ValidFor = Analysis.Flexible,
            '    .Thickness = factory.CreateThickness(6, _usCustomary),
            '    .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
            '    .Modulus = factory.CreateModulus(700000, _usCustomary),
            '    .AllowedTop = False,
            '    .AllowedBottom = False,
            '    .IsUserDefined = False,
            '    .AllowedModulusEdit = True,
            '    .LayerCode = 17
            '    }
            'materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Stabilized",
                .Name = "P-401/P-403 HMA Stabilized",
                .ValidFor = Analysis.Both,
                .Thickness = factory.CreateThickness(5, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(400000, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = False,
                .LayerCode = 14,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = False,
                .ThicknessActive = True,
                .RuptureActive = False
                }
            materialDefaults.Add(material)


            material = New MaterialDefault() With {
                .Category = "Stabilized",
                .Name = "Variable (flexible)",
                .ValidFor = Analysis.Flexible,
                .Thickness = factory.CreateThickness(5, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(150000, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(150000, 400000, True, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = True,
                .LayerCode = 9,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = True,
                .ThicknessActive = True,
                .RuptureActive = False
                }
            materialDefaults.Add(material)

            material = New MaterialDefault() With {
                .Category = "Stabilized",
                .Name = "Variable (rigid)",
                .ValidFor = Analysis.Rigid,
                .Thickness = factory.CreateThickness(5, _usCustomary),
                .ThicknessValidation = factory.CreateThicknessValidationRange(1, 500, True, _usCustomary),
                .Modulus = factory.CreateModulus(250000, _usCustomary),
                .ModulusValidation = factory.CreateModulusValidationRange(250000, 700000, True, _usCustomary),
                .AllowedTop = False,
                .AllowedBottom = False,
                .IsUserDefined = False,
                .AllowedModulusEdit = True,
                .LayerCode = 7,
                .CBRActive = False,
                .KValueActive = False,
                .ModulusActive = True,
                .ThicknessActive = True,
                .RuptureActive = False
                }
            materialDefaults.Add(material)
            Return materialDefaults
        End Function
    End Module
End Namespace
