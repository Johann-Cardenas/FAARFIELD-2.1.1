Imports System.Collections.ObjectModel
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces

Public Module ProgramDefaults

    Dim _materialLibrary As ObservableCollection(Of MaterialDefault)
    Dim _factory As IFaarFieldModelFactory
    Dim _usCustomary As IMeasurmentSystem

    Public Function GetAnalysisType(factory As IFaarFieldModelFactory, materialLibrary As ObservableCollection(Of MaterialDefault)) As List(Of IAnalysisType)
        _materialLibrary = materialLibrary
        _factory = factory
        _usCustomary = factory.CreateUsCustomary()

        Dim analyses = New List(Of IAnalysisType) From {
            factory.CreateAnalysisType(0, "New Flexible", False, False, GetNewFlexibleLayers()),
            factory.CreateAnalysisType(1, "HMA on Aggregate", False, False, GetAcOnAggregateLayers()),
            factory.CreateAnalysisType(2, "HMA Overlay on Flexible", False, True, GetAcOnFlexibleLayers),
            factory.CreateAnalysisType(3, "HMA Overlay on Rigid", True, True, GetAcOnRigidLayers()),
            factory.CreateAnalysisType(4, "New Rigid", True, True, GetNewRigidLayers()),
            factory.CreateAnalysisType(5, "PCC Overlay on Flexible", True, True, GetPccOnFlexibleLayers()),
            factory.CreateAnalysisType(6, "Unbonded PCC Overlay on Rigid", True, True, GetPccOnRigidLayers())
        }

        'factory.CreateAnalysisType(7, "Partially Bonded on Rigid", True, True, GetPccOnPartiallyBondedLayers())

        'factory.CreateAnalysisType(7, "Fully Bonded on Rigid", True, True, GetPccOnFullyBonded()),

        Return analyses
    End Function


    Private Function FindMaterial(materialLibrary As ObservableCollection(Of MaterialDefault), name As String) As MaterialDefault
        Dim libMaterial = New MaterialDefault
        For Each mat In materialLibrary
            If mat.Name = name Then
                libMaterial = mat
            End If
        Next
        Return libMaterial
    End Function


    Private Function GetNewFlexibleLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()
        Dim flexible = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Surface"), False)
        For i = 0 To 18
            If flexible.Name = _materialLibrary.Item(i).Name Then
                flexible.Category = New String(_materialLibrary.Item(i).Category)
            End If
        Next
        flexible.Thickness = _factory.CreateThickness(4, _usCustomary)
        layers.Add(flexible)

        ' Dim flexible = FindMaterial(_materialLibrary, "P-401/P-403 HMA Surface")
        'layers.Add(_factory.CreateMaterial(_factory, flexible, False))
        Dim stabilized = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Stabilized"), False)
        For i = 0 To 18
            If stabilized.Name = _materialLibrary.Item(i).Name Then
                stabilized.Category = New String(_materialLibrary.Item(i).Category)
            End If
        Next
        stabilized.Thickness = _factory.CreateThickness(5, _usCustomary)
        layers.Add(stabilized)

        'Dim stabilized = FindMaterial(_materialLibrary, "P-401/P-403 HMA Stabilized")
        'layers.Add(_factory.CreateMaterial(_factory, stabilized, True))

        Dim aggregate = FindMaterial(_materialLibrary, "P-209 Crushed Aggregate")
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = New String(_materialLibrary.Item(i).Category)
            End If
        Next
        aggregate.Thickness = _factory.CreateThickness(10, _usCustomary)

        layers.Add(_factory.CreateMaterial(_factory, aggregate, True))

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = New String(_materialLibrary.Item(i).Category)
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, True))


        Return layers
    End Function

    Private Function GetNewRigidLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()
        'Dim rigid = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Surface"), False)
        Dim rigid As IMaterial
        rigid = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Surface"), False)
        For i = 0 To 18
            If rigid.Name = _materialLibrary.Item(i).Name Then
                rigid.Category = _materialLibrary.Item(i).Category
            End If
        Next

        rigid.Thickness = _factory.CreateThickness(14, _usCustomary)
        rigid.Rupture = _factory.CreateModulus(650, _usCustomary)
        'rigid.Modulus = _factory.CreateModulus(650, _usCustomary)
        layers.Add(rigid)

        Dim stabilized = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Stabilized"), True)
        For i = 0 To 18
            If stabilized.Name = _materialLibrary.Item(i).Name Then
                stabilized.Category = _materialLibrary.Item(i).Category
            End If
        Next
        stabilized.Thickness = _factory.CreateThickness(5, _usCustomary)
        layers.Add(stabilized)

        Dim aggregate = FindMaterial(_materialLibrary, "P-209 Crushed Aggregate")
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = _materialLibrary.Item(i).Category
            End If
        Next
        aggregate.Thickness = _factory.CreateThickness(6, _usCustomary)

        layers.Add(_factory.CreateMaterial(_factory, aggregate, True))

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, True))

        Return layers
    End Function

    Private Function GetAcOnFlexibleLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()

        Dim overlay = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Overlay"), False)
        For i = 0 To 18
            If overlay.Name = _materialLibrary.Item(i).Name Then
                overlay.Category = _materialLibrary.Item(i).Category
            End If
        Next
        overlay.Thickness = _factory.CreateThickness(4, _usCustomary)
        layers.Add(overlay)

        Dim flexible = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Surface"), False)
        For i = 0 To 18
            If flexible.Name = _materialLibrary.Item(i).Name Then
                flexible.Category = _materialLibrary.Item(i).Category
            End If
        Next
        flexible.Thickness = _factory.CreateThickness(4, _usCustomary)
        layers.Add(flexible)

        Dim stabilized = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "Variable (flexible)"), True)
        For i = 0 To 18
            If stabilized.Name = _materialLibrary.Item(i).Name Then
                stabilized.Category = _materialLibrary.Item(i).Category
            End If
        Next
        stabilized.Thickness = _factory.CreateThickness(5, _usCustomary)
        layers.Add(stabilized)

        Dim aggregate = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-209 Crushed Aggregate"), True)
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = _materialLibrary.Item(i).Category
            End If
        Next
        aggregate.Thickness = _factory.CreateThickness(6, _usCustomary)   '10
        layers.Add(aggregate)

        'Dim aggregate = FindMaterial(_materialLibrary, "P-209 Crushed Aggregate")
        'layers.Add(_factory.CreateMaterial(_factory, aggregate, False))

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, False))

        Return layers
    End Function

    Private Function GetAcOnRigidLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()

        Dim overlay = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Overlay"), False)
        For i = 0 To 18
            If overlay.Name = _materialLibrary.Item(i).Name Then
                overlay.Category = _materialLibrary.Item(i).Category
            End If
        Next
        overlay.Thickness = _factory.CreateThickness(12, _usCustomary)
        layers.Add(overlay)

        Dim rigid = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Surface"), False)
        For i = 0 To 18
            If rigid.Name = _materialLibrary.Item(i).Name Then
                rigid.Category = _materialLibrary.Item(i).Category
            End If
        Next
        rigid.Thickness = _factory.CreateThickness(14, _usCustomary) '14
        layers.Add(rigid)

        Dim stabilized = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Stabilized"), False)
        For i = 0 To 18
            If stabilized.Name = _materialLibrary.Item(i).Name Then
                stabilized.Category = _materialLibrary.Item(i).Category
            End If
        Next
        stabilized.Thickness = _factory.CreateThickness(5, _usCustomary)
        stabilized.Modulus = _factory.CreateModulus(400000, _usCustomary)         'new    ed
        layers.Add(stabilized)

        'Dim stabilized = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary,  "Variable (rigid)"), True)
        'stabilized.Thickness = _factory.CreateThickness(5, _usCustomary)    '
        'stabilized.Modulus = _factory.CreateModulus(400000, _usCustomary)        'ed
        'layers.Add(stabilized)

        Dim aggregate = FindMaterial(_materialLibrary, "P-209 Crushed Aggregate")
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = _materialLibrary.Item(i).Category
            End If
        Next
        aggregate.Thickness = _factory.CreateThickness(6, _usCustomary)
        layers.Add(_factory.CreateMaterial(_factory, aggregate, True))

        'Dim aggregate = FindMaterial(_materialLibrary,  "P-209 Crushed Aggregate")
        'layers.Add(_factory.CreateMaterial(_factory, aggregate, True))

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, True))

        Return layers
    End Function

    Private Function GetAcOnAggregateLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()


        Dim flexible = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Surface"), False)
        For i = 0 To 18
            If flexible.Name = _materialLibrary.Item(i).Name Then
                flexible.Category = _materialLibrary.Item(i).Category
            End If
        Next
        flexible.Thickness = _factory.CreateThickness(4, _usCustomary)
        layers.Add(flexible)

        Dim aggregate = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-209 Crushed Aggregate"), True)
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = _materialLibrary.Item(i).Category
            End If
        Next
        'aggregate.Thickness = _factory.CreateThickness(6, _usCustomary)   '10
        aggregate.Thickness = _factory.CreateThickness(10, _usCustomary)
        layers.Add(aggregate)

        Dim uncrushed = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-154 Uncrushed Aggregate"), True)
        For i = 0 To 18
            If uncrushed.Name = _materialLibrary.Item(i).Name Then
                uncrushed.Category = _materialLibrary.Item(i).Category
            End If
        Next
        uncrushed.Thickness = _factory.CreateThickness(6, _usCustomary)    '16
        layers.Add(uncrushed)

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, True))

        Return layers
    End Function

    Private Function GetPccOnRigidLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()

        Dim overlay = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Overlay (unbonded)"), False)
        For i = 0 To 18
            If overlay.Name = _materialLibrary.Item(i).Name Then
                overlay.Category = _materialLibrary.Item(i).Category
            End If
        Next
        overlay.Thickness = _factory.CreateThickness(12, _usCustomary)
        layers.Add(overlay)

        Dim rigid = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Surface"), False)
        For i = 0 To 18
            If rigid.Name = _materialLibrary.Item(i).Name Then
                rigid.Category = _materialLibrary.Item(i).Category
            End If
        Next
        rigid.Thickness = _factory.CreateThickness(14, _usCustomary)
        layers.Add(rigid)

        Dim stabilized = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "Variable (rigid)"), True)
        For i = 0 To 18
            If stabilized.Name = _materialLibrary.Item(i).Name Then
                stabilized.Category = _materialLibrary.Item(i).Category
            End If
        Next
        stabilized.Thickness = _factory.CreateThickness(5, _usCustomary)
        layers.Add(stabilized)

        Dim aggregate = FindMaterial(_materialLibrary, "P-209 Crushed Aggregate")
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, aggregate, True))

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, True))

        Return layers
    End Function


    Private Function GetPccOnFlexibleLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()

        Dim overlay = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Overlay on Flexible"), False)
        For i = 0 To 18
            If overlay.Name = _materialLibrary.Item(i).Name Then
                overlay.Category = _materialLibrary.Item(i).Category
            End If
        Next
        overlay.Thickness = _factory.CreateThickness(15, _usCustomary)
        overlay.Rupture = _factory.CreateModulus(650, _usCustomary)
        layers.Add(overlay)

        Dim flexible = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-401/P-403 HMA Surface"), False)
        For i = 0 To 18
            If flexible.Name = _materialLibrary.Item(i).Name Then
                flexible.Category = _materialLibrary.Item(i).Category
            End If
        Next
        flexible.Thickness = _factory.CreateThickness(4, _usCustomary)
        layers.Add(flexible)

        Dim aggregate = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-209 Crushed Aggregate"), True)
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = _materialLibrary.Item(i).Category
            End If
        Next
        aggregate.Thickness = _factory.CreateThickness(12, _usCustomary)
        layers.Add(aggregate)

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, True))

        Return layers
    End Function

    Private Function GetPccOnPartiallyBondedLayers() As ObservableCollection(Of IMaterial)
        Dim layers = New ObservableCollection(Of IMaterial)()

        Dim overlay = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Overlay (partially bonded)"), False)
        For i = 0 To 18
            If overlay.Name = _materialLibrary.Item(i).Name Then
                overlay.Category = _materialLibrary.Item(i).Category
            End If
        Next
        overlay.Thickness = _factory.CreateThickness(12, _usCustomary)
        layers.Add(overlay)

        Dim rigid = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "P-501 PCC Surface"), False)
        For i = 0 To 18
            If rigid.Name = _materialLibrary.Item(i).Name Then
                rigid.Category = _materialLibrary.Item(i).Category
            End If
        Next
        rigid.Thickness = _factory.CreateThickness(14, _usCustomary)
        layers.Add(rigid)

        Dim stabilized = _factory.CreateMaterial(_factory, FindMaterial(_materialLibrary, "Variable (rigid)"), True)
        For i = 0 To 18
            If stabilized.Name = _materialLibrary.Item(i).Name Then
                stabilized.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(stabilized)

        Dim aggregate = FindMaterial(_materialLibrary, "P-209 Crushed Aggregate")
        For i = 0 To 18
            If aggregate.Name = _materialLibrary.Item(i).Name Then
                aggregate.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, aggregate, True))

        Dim subgrade = FindMaterial(_materialLibrary, "Subgrade")
        For i = 0 To 18
            If subgrade.Name = _materialLibrary.Item(i).Name Then
                subgrade.Category = _materialLibrary.Item(i).Category
            End If
        Next
        layers.Add(_factory.CreateMaterial(_factory, subgrade, True))

        Return layers
    End Function
End Module
