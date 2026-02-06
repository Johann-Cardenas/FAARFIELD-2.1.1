Option Strict On
Option Explicit On
Public Module modWindows


    Public gPtoC1 As Boolean
    Public gConstTirePressure As Boolean


    'RDEC Model
    Public gPNMS1, gPPCS1, gP2001 As Single

    Public gFlexuralMod1, gAirVoids1, gAsphaltContentByVol1 As Single
    Public gFlexuralMod(MaxSects) As Single ' = 600000
    Public gAirVoids(MaxSects) As Single '= 3.5
    Public gAsphaltContentByVol(MaxSects) As Single '= 12
    Public gVoidPar(MaxSects) As Single '= 3.5 / (3.5 + 12)

    Public gPNMS(MaxSects) As Single '= 95
    Public gPPCS(MaxSects) As Single '= 58
    Public gP200(MaxSects) As Single '= 4.5
    Public gGradationPar(MaxSects) As Single '= (95 - 58) / 4.5

    Public gRDEC As Boolean = False

    Public lightAircraft30kLess, lightAircraft60kMore, P208_60kMoreCase As Boolean
    Public lightAircraft12kLess As Boolean
    Public HMA_CDF_Calc As Boolean = False

    Public lightAC_12_5_to_less_100k, gP208_60kMoreCase As Boolean
    Public lightAircraft_12_5kLess As Boolean
    Public buttonCDFgraph As Boolean = False

    Public Const TimeFile As Short = 9 'ikawa
    Declare Function timeGetTime Lib "winmm" () As Integer

End Module