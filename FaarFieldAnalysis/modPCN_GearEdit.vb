
Imports System.Drawing

Module modPCN_GearEdit

    Public gPtoC(MaxSectAC) As Double
    'Public gPtoC_Rigid(MaxSectAC) As Double
    Public gPtoC_PCN(MaxSectAC) As Double


    'New stuff for canvas / picturebox conversion
    Public topLeft As Point
    Public bottomRIght As Point

    Dim CtoPmaxFlex, CtoPmaxRigid As Double ' Needed in ChangeAnnualDapartures and WriteParmGrid.
    Public PtoCFlex, PtoCRigid As Double ' Needed in ChangeAnnualDapartures and WriteParmGrid.
    'Public PtoTC As Double ' GFH 9/16/09. Int to Double 12/14/09.

    Public Const ACN_mode As Integer = 1 'ikawa 01/24/03
    Public Const Thick_mode As Integer = 2 'ikawa 01/24/03

    Public IWheelSelected As Integer
    Public Const CoordResolution As Integer = 10

    Public Operation, LastOperation As Integer
    Public LastXP, LastYP As Double
    Public LastIWheel As Integer

    Public Const NoOperation As Integer = 0
    Public Const MoveWheel As Integer = 1
    Public Const AddWheel As Integer = 2
    Public Const RemoveWheel As Integer = 3
    Public Const SelectAWheel As Integer = 4
    Public Const ChangeXCoordinate As Integer = 5
    Public Const ChangeYCoordinate As Integer = 6

    Public Const kPaTopsi As Double = 0.1450377438
    Public Const cmToin As Double = 0.3937008
    Public Const kgTolb As Double = 2.2046225
    Public Const tonTolb As Double = 2204.6226218

    Public EditWheels As Boolean 'Izydor Kawa added code

    'Public ChangeDataRet As MessageBoxResult

    'Public Const MaxSectAC As Integer = 20
    'Public Const MaxLibGroups As Integer = 10
    'Public Const MaxLibAC As Integer = 512 ' Total number of aircraft in library.
    'Public Const MaxNEval As Integer = 8 ' Maximum number of evaluation points
    'Public Const MaxNTires As Integer = 24 ' Maximum number of tires on eval. gear.
    'Public Const MaxNTTrack As Integer = 10 ' Maximum number of gear tracks (for CDF).

    'Public ViewingAircraft As Integer
    'Public lstLibFileIndex As Integer ' See frmParameters.lstAircraft
    'Public lstACGroupIndex As Integer ' See frmParameters.lstAircraft
    'Public lstAircraftIndex As Integer ' See frmParameters.lstAircraft

    'Public ILibACGroup As Integer
    'Public NLibACGroups As Integer
    'Public LibACGroup(MaxLibGroups) As Integer
    'Public LibACGroupName(MaxLibGroups) As String

    'Public libNAC As Integer ' Number of aircraft in library list.
    'Public NBelly As Integer
    'Public Const BellyExt As String = " Belly"
    ' Public libACName(MaxLibAC) As String
    ' Public libGL(MaxLibAC) As Single
    ' Public libNMainGears(MaxLibAC) As Single
    'Public libPcntOnMainGears(MaxLibAC) As Single    'ikawa 01/24/03
    'Public libPcntOnMainGears(MaxLibAC, 2) As Single 'ikawa 01/24/03
    'Global libMGpcnt(MaxLibAC)          As Single    'ikawa 01/24/03
    'Public libMGpcnt(MaxLibAC, 2) As Single 'ikawa 01/24/03

    'Public libNTires(MaxLibAC) As Integer
    'Public libTX(MaxLibAC, MaxNTires) As Single
    'Public libTY(MaxLibAC, MaxNTires) As Single
    'Public libCP(MaxLibAC) As Single
    'Public libNEVPTS(MaxLibAC) As Integer
    'Public libEVPTX(MaxLibAC, MaxNEval) As Single
    'Public libEVPTY(MaxLibAC, MaxNEval) As Single
    'Public libGear(MaxLibAC) As String
    'Public libNTTrack(MaxLibAC) As Integer
    'Public libIGear(MaxLibAC) As Integer
    'Public libTT(MaxLibAC) As Single
    'Public libTS(MaxLibAC) As Single
    'Public libTG(MaxLibAC) As Single
    'Public libB(MaxLibAC) As Single
    'Public libBF(MaxLibAC) As Single ' Front, for large B-777 models. GFH 12-13-05.
    'Public libBR(MaxLibAC) As Single ' Rear, for large B-777 models. GFH 12-13-05.
    'Public libXAC(MaxLibAC, MaxNTTrack) As Single
    'Public libAlpha(MaxLibAC) As Double
    'Public libCoverages(MaxLibAC) As Double
    'Public libAnnualDepartures(MaxLibAC) As Double
    'Public libXGridOrigin(MaxLibAC) As Double
    'Public libYGridOrigin(MaxLibAC) As Double
    'Public libXGridMax(MaxLibAC) As Double
    'Public libYGridMax(MaxLibAC) As Double
    Public libXGridNPoints(MaxLibAC) As Double
    Public libYGridNPoints(MaxLibAC) As Double



    Public libIndex1 As Integer ' Library index for load index (link). 'PPPP
    'Public LI As Integer ' Temporary alias for LibIndex(I)

    'Public NAC As Integer ' Number of aircraft in current section.
    'Public ACName(MaxSectAC) As String
    'Public GL(MaxSectAC) As Single ' Gross aircraft load for design.
    'Public WT(MaxSectAC) As Single
    'Public TW(MaxSectAC) As Single

    'Public Const MinGLFraction As Single = 0.1
    'Public Const MaxGLFraction As Single = 10
    Public Const MaxEvalThickness As Double = 260 ' Set by limitation in the CBR module.
    Public Const MinEvalThickness As Double = 0.1


End Module
