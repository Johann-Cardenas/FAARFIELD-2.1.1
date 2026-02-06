' change "Single" to "Double", "CSng" to "CDbl" for FAASR3D by YC 102418-041519

Option Strict On
Option Explicit On

Module frmStructure
    Public IFParam As Double
    Public OverlayType As String
    Public InfiniteElement As Boolean
    Public Overlay, SubBaseExists As Boolean
    Public NumberOfLayers As Short
    Public ThkOverlay, ThickTL, ThkBase, ThkSubBase As Double
    'Public Cutoff, EqJStfX As Double
    Public Cutoff, EqJStfX, EqJStfY, EqEdgStfX, EqEdgStfY As Double  'modified to be in consistence with FEAFAA
    Public EqJStf12X, EqJStf12Y As Double  'added for spring BC to simulate infinite slab by YGC 092613
    Public KeyEqEdgStf As Short  'added to suppress corner spring when using horizontal spring to simulate infinite slab by YGC 092613
    Public KeyEqJStf, KeyEqJStf12 As Short  ' Modified to combine the spring BC for finite slab and infinte slab model by YGC 112213	
    Public XDimension, YDimension As Double
    Public XScaleFactor, YScaleFactor As Double  'added by YGC 020911


    Public Sub frmStructure_Load(ByRef LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms)
        Dim I As Integer

        NumberOfLayers = CShort(LEAStructure.NLayers)
        SubBaseExists = False
        If ((Not Overlay) And (NumberOfLayers > 3)) Or (Overlay And (NumberOfLayers > 4)) Then
            SubBaseExists = True
        End If

        If Overlay Then
            For I = 0 To NumberOfLayers - 1
                LayerThickness(I) = CDbl(LEAStructure.Thick(I + 1))
                EMod(I) = CDbl(LEAStructure.Modulus(I + 1))
                PoissonsRatio(I) = CDbl(LEAStructure.Poisson(I + 1))
            Next

            For I = NumberOfLayers To 6
                LayerThickness(I) = CDbl(LEAStructure.Thick(NumberOfLayers))
                EMod(I) = CDbl(LEAStructure.Modulus(NumberOfLayers))
                PoissonsRatio(I) = CDbl(LEAStructure.Poisson(NumberOfLayers))
            Next
        Else
            For I = 1 To NumberOfLayers
                LayerThickness(I) = CDbl(LEAStructure.Thick(I))
                EMod(I) = CDbl(LEAStructure.Modulus(I))
                PoissonsRatio(I) = CDbl(LEAStructure.Poisson(I))
            Next

            For I = NumberOfLayers + 1 To 6
                LayerThickness(I) = CDbl(LEAStructure.Thick(NumberOfLayers))
                EMod(I) = CDbl(LEAStructure.Modulus(NumberOfLayers))
                PoissonsRatio(I) = CDbl(LEAStructure.Poisson(NumberOfLayers))
            Next
        End If

        'modified for 20 ft by YGC 092613
        'Modified to 25 ft by YGC 021110
        'frmStructure.XDimension = 30
        'frmStructure.YDimension = 30
        'frmStructure.XDimension = 25
        'frmStructure.YDimension = 25
        frmStructure.XDimension = 20
        frmStructure.YDimension = 20
        'Modify end by YGC 021110
        'Modify end by YGC 092613

        If MeshCase = "5DSYM" Then
            frmStructure.XDimension = 100
            frmStructure.YDimension = 30
        End If


        Cutoff = 240
        InfiniteElement = True

        'modified for spring BC to simulate infinite slab by YGC 092613
        'EqJStfX = 100000
        'EqJStfY = 100000

        ' Modified to combine the spring BC for finite slab and infinte slab model by YGC 112213
        'EqJStfX = 0
        'EqJStfY = 0
        EqJStfX = 100000
        EqJStfY = 100000
        ' Modify ended to combine the spring BC for finite slab and infinte slab model by YGC 112213

        EqJStf12X = 10000000000.0
        EqJStf12Y = 10000000000.0
        'modify ended for spring BC to simulate infinite slab by YGC 092613

        ' Modified to combine the spring BC for finite slab and infinte slab model by YGC 112213
        KeyEqJStf = 0   'no vertical spring
        KeyEqJStf12 = 1
        ' Modify ended to combine the spring BC for finite slab and infinte slab model by YGC 112213


        EqEdgStfX = 10000
        EqEdgStfY = 10000

        KeyEqEdgStf = 0 'added to suppress corner spring when using horizontal spring to simulate infinite slab by YGC 092613


        'ik001
        XScaleFactor = CDbl(frmStructure.XDimension / 25.0#)
        YScaleFactor = CDbl(frmStructure.YDimension / 25.0#)

        'XScaleFactor = CDbl(frmStructure.XDimension / 30.0#)
        'YScaleFactor = CDbl(frmStructure.YDimension / 30.0#)

        If LEAStructure.InterfaceParm(1) = 1 Then
            OverlayType = "FullyBonded"
        ElseIf LEAStructure.InterfaceParm(1) = 0 Then
            OverlayType = "UnBonded"
        ElseIf LEAStructure.InterfaceParm(1) < 1 Then
            OverlayType = "PartiallyBonded"
        End If

        If OverlayType = "FullyBonded" Then
            IFParam = 1.0
        ElseIf OverlayType = "PartiallyBonded" Then
            IFParam = 0.5
        ElseIf OverlayType = "UnBonded" Then
            IFParam = 0.0
        End If
    End Sub
End Module