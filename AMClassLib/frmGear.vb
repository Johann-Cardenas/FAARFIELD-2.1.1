' change "Single" to "Double", "CSng" to "CDbl" for FAASR3D by YC 102418-041519

Option Strict On
Option Explicit On

Module frmGear
    Public NAC, iNAC As Integer
    Public Xcg, Ycg As Double
    Public WhlWidth, WhlLength As Double
    Public GearParallel As Boolean, SelfWeight As Boolean
    Public EdgeLoad, Interior As Boolean
    Public GearAngle As Double

    Public NWheels As Short
    Public Const NMaxWheels As Short = 56
    Public XWheels(NMaxWheels) As Double
    Public YWheels(NMaxWheels) As Double
    Public XGridMax, XGridOrigin, XGridNPoints As Double
    Public YGridMax, YGridOrigin, YGridNPoints As Double
    Public GrossWeight, TirePressure As Double
    Public libGear As String
    Public Structure ScalingInfo
        Dim index2 As Integer
        Dim Scaled As Boolean
        Dim ScaleFactorA As Double
    End Structure



    Public Sub fromGear_Load(ByRef ACIndex As Integer, ByRef LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms)     'ik 06.08
        Dim I As Short


        If LEAAircraft(ACIndex).ACname = "C-17A" Then
            GearAngle = 90
        ElseIf LEAAircraft(ACIndex).ACname = "C-5" Then
            GearAngle = 0
        ElseIf LEAAircraft(ACIndex).ACname = "IL76T" Then
            GearAngle = 90
        End If

        'libGear = LEAAircraft(ACIndex).strDummy(1)
        GrossWeight = LEAAircraft(ACIndex).GearLoad
        NWheels = CShort(LEAAircraft(ACIndex).NTires)

        For I = 1 To NWheels
            XWheels(I) = LEAAircraft(ACIndex).TireY(I)
            YWheels(I) = LEAAircraft(ACIndex).TireX(I)
        Next I
        TirePressure = LEAAircraft(ACIndex).TirePress(1)

        If EdgeLoad Then
            Interior = False
            GearParallel = True
        Else
            Interior = True
            GearParallel = False
        End If

        SelfWeight = False
        Call PlotGear()
        If GearAngle = 0.0# Then frmGear.GearParallel = True
        If GearAngle <> 0.0# Then frmGear.GearParallel = False
    End Sub

    Public Sub PlotGear()
        Dim EqWheelArea, TireLoad, ContactLength As Double

        ' Gear axes are X +ve forward, Y +ve to the right.
        ' Gear axes on screen are X +ve upward, Y +ve to the right.
        ' Screen axes are X +ve to the right, Y +ve upward.
        ' Therefore, screen coordinates are written (y, x) in gear axes.
        ' Remember when drawing on screen and returning screen coordinates from mouse.

        Call GearCG(XWheels, YWheels, NWheels, Xcg, Ycg)

        TireLoad = ((GrossWeight)) * (CDbl(1.0# / NWheels))
        EqWheelArea = TireLoad / TirePressure
        ContactLength = System.Math.Sqrt(EqWheelArea / 0.5227)
        WhlWidth = CDbl(0.6 * ContactLength)
        WhlLength = CDbl(0.8712 * ContactLength)

    End Sub
End Module