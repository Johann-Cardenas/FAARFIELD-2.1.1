Option Explicit On

Imports System.IO
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports ACClassLib
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports Microsoft.VisualBasic.FileIO




Public Module FEDFAA1

    Public Function getTodaysDateFormatted2() As String
        Return DateTime.Now.ToString("yyyy-MM-dd HH;mm;ss") 'ik2020.03
    End Function

    Public gDetailedReportData As New clsDetailedReportData()
    Public ReducedCrossSectionRunChecker As Boolean = False
    Public gOutputDirName As String 'ik2020.03

    Public gCaseCDFlow As Boolean
    Public Const gTolerance As Single = 0.005
    Public indexPCNtable2 As Integer
    Public OldCDFPic As Double
    Public NewCDFPic As Double
    Public gACRmaxIndex As Integer
    Public SubgradeCategoryPCRReport As String
    Public SubgradeCategory As String
    Public HMAonRigidCase As Integer
    Public TFN As String
    Public gNewACName(81) As String
    Public gNewAnnualDepart(81) As Single
    Public gNewGL(81) As Single
    Public FlexonRigidChecker As Boolean = False
    Public FlexonRigidCounter As Integer = 0

    Public FF_true_ACN As Boolean = False
    'Public gRun234etc As Boolean = False 'ik2021.05
    Public gAdjustAnnOrGL As Boolean = False 'ik2021.05
    'Public gInteriorStressBigger As Boolean = False 'ik2021.05

    Public gPCR_Life As Boolean = False 'ik2021.05
    Public gLEAFstressGreater() As Boolean 'ik2021.05
    Public gLEAFstressGreaterOverlay() As Boolean 'ik2021.05

    Public gSelectedACindex As Integer 'ik2021.05


    Public Property sThick1 As Single
    Public Property sThick2 As Single
    Public Property sRcon2 As Single
    Public Property sModulus2 As Single
    Public Property sLCode2 As Short

    Public gNewPCN(81) As Single
    Public gNewPCNthick(81) As Single
    Public gNewPCNtoReport As Single

    Public H_index As Integer
    Public cdf_max1M(1) As Single
    Public ind2maxACxM(1) As Integer
    Public ind2maxACxMSave(1) As Integer

    Public MinimumStrainChecker As Boolean = False
    'Public gGearInd(MaxSectAC) As Short

    Public OverflowExit As Boolean = False
    Public LowLifeExit As Boolean = False

    Public gCDF_target As Single 'ACN/PCN Method
    Public gCDF_target_copy As Single
    Public gCDF_reached As Single
    Public gPCN_report As Single
    Public gPCN_report_index As Integer

    Public gOverlayLife_reached As Single

    Public gOverlayLife_target As Single
    Public gOverlayLife_target_copy As Single


    Public gCriticalACEquivCovs(NAC, NAC) As Double
    Public gTARGET As Double
    Public gPcnt1(MaxSectAC) As Single

    Public ICAOCodeIndex As Integer
    Public ICAOCode(3) As String

    Public gETimemsecs As Single

    Public gCoverage_NtoFail(MaxSectAC) As Double
    Public gFlex_NtoFail_Copy(MaxSectAC) As Double

    Public gMGW(MaxSectAC) As Single
    Public gLifeStr(MaxSectAC) As Single

    'Public gACN_Thick(MaxSectAC) As Single

    Public gACN_GL(MaxSectAC) As Single
    Public gACN_GL_thick(MaxSectAC) As Single
    Public gkk As Integer
    Public lPavementType As ACRClassLib.clsACR.PavementType


    'Public FF_true_ACN As Boolean = False
    Public ConversionACCovs(MaxSectAC) As Double

    Public SaveM_NAC(1) As Short
    Public SaveM_LibIndex(MaxSectAC, 1) As Short
    Public SaveM_ACName(MaxSectAC, 1) As String
    Public SaveM_GL(MaxSectAC, 1) As Single
    Public SaveM_RepsAnnual(MaxSectAC, 1) As Single
    Public SaveM_RepsInc(MaxSectAC, 1) As Single
    Public SaveM_WT(MaxSectAC, 1) As Single
    Public SaveM_TW(MaxSectAC, 1) As Single
    Public SaveM_MGpcnt(MaxSectAC, 1) As Single
    Public SaveM_NAC_order(MaxSectAC, 1) As Integer




    Public Save_NAC As Short
    Public Save_LibIndex(MaxSectAC) As Short
    Public Save_ACName(MaxSectAC) As String
    Public Save_GL(MaxSectAC) As Single
    Public Save_RepsAnnual(MaxSectAC) As Single
    Public Save_RepsInc(MaxSectAC) As Single
    Public Save_WT(MaxSectAC) As Single
    Public Save_TW(MaxSectAC) As Single
    Public Save_MGpcnt(MaxSectAC) As Single
    Public Save_NAC_order(MaxSectAC) As Integer


    Public NAC_order(MaxSectAC) As Integer

    Public Save2_NAC As Short
    Public Save2_LibIndex(MaxSectAC) As Short
    Public Save2_ACName(MaxSectAC) As String
    Public Save2_GL(MaxSectAC) As Single
    Public Save2_RepsAnnual(MaxSectAC) As Single
    Public Save2_RepsInc(MaxSectAC) As Single
    Public Save2_WT(MaxSectAC) As Single
    Public Save2_TW(MaxSectAC) As Single
    Public Save2_MGpcnt(MaxSectAC) As Single
    Public Save2_NAC_order(MaxSectAC) As Integer



    Public Save3_NAC As Short
    Public Save3_LibIndex(MaxSectAC) As Short
    Public Save3_ACName(MaxSectAC) As String
    Public Save3_GL(MaxSectAC) As Single
    Public Save3_RepsAnnual(MaxSectAC) As Single
    Public Save3_RepsInc(MaxSectAC) As Single
    Public Save3_WT(MaxSectAC) As Single
    Public Save3_TW(MaxSectAC) As Single
    Public Save3_MGpcnt(MaxSectAC) As Single
    Public Save3_NAC_order(MaxSectAC) As Integer

    Public numberoferrors As Boolean


    Dim PercentGross(NAC * 2) As Single

    Public jobACName(MaxSects, MaxSectAC) As String
    Public AC_layer_min(MaxNPLayers) As Single 'PPPP
    Public gUserInterrupted As Boolean = False 'September 13, 2006
    Public DefaultNSection As Short = 16 'TODO: THIS PROBABLY SHOULD BE REPLACED WITH INPUT VALUE 

    Public LEDFAA13Stress As Single 'ikawa
    Public LEDFAA13StressAM, LEDFAA13StressAM2 As Single 'ikawa


    Public PCCStress(40), OverlayStress(40) As Double
    Public gCDF() As Single, gCDF2max As Single
    Public CalcStress() As Integer
    Public PrintResults13, PrintResultsCDF As Boolean 'ikawa 

    Public t1Thick As Date

    Public Const Log10 As Single = 2.302585!
    ' General use variables.

    Public SS, S, FileName As String
    Public Ret As Long
    Public Temp, Temp1, Temp2 As Single
    Public ITemp As Short
    Public NL As String  ' New Line. See frmStartup.form_load for setting statement.
    Public NL2 As String ' 2 new lines.


    Public NotesString As String

    ' The following are set in Sub StandardMsgs
    Public NonStandardStr As String
    Public Note320, Note328 As String

    'Global NoB777$ ' GFH 04/07/03.
    Public BadACList As String
    Public NotDefaultLife As String

    ' File related variables.
    Public ACDATPath As String

    Public WorkingDir As String  ' GFH 04-02-03.
    Public MyDocumentDir As String


    ' System states.
    Public DesigningStr As Boolean
    Public LifeComputation As Boolean
    Public PCRRun As Boolean
    Public ThicknessDesignRun As Boolean = False
    Public LifeCompactionRun As Boolean = False
    Public AnalyzedasFlexible As Boolean = False 'added for Life computation, Kairat 


    Public UndefinedLayer As Boolean
    Public AC_Note320, AC_Note328 As Boolean
    'Global B777inMix As Integer ' GFH 04/07/03.
    Public GoodACList As Boolean

    ' Aircraft related declarations.
    Public Const MaxJobs As Short = 100
    Public Const MaxSects As Short = 100 ' Maximum number of sections in a job.
    Public Const MaxSectAC As Short = 80 'was 10  ' GFH 02-24-03. Increased again from 30 to 40, 08/13/03.
    Public Const MaxLibAC As Short = 105
    Public Const MaxLibGroups As Short = 10

    Public Const MaxNTires As Short = 24 'was 12' Maximum number of tires on eval. gear. GFH 02-17-03
    Public Const MaxNTTrack As Short = 12 'was 10 ' Maximum number of gear tracks (for CDF).


    Public IJob As Integer
    Public JobName As String
    Public NSects, ISect As Integer
    Public ILayer As Short ' Current layer.

    Public ILibACGroup As Short
    Public NLibACGroups As Short
    Public LibACGroup(MaxLibGroups) As Short
    Public LibACGroupName(MaxLibGroups) As String
    Public ExternalLibraryActive As Boolean


    Public libNAC As Short ' Number of aircraft in library list.
    Public NBelly As Short
    Public Const BellyExt As String = " Belly"


    Public LibIndex(MaxSectAC) As Short ' Library index for load index (link).
    Public LI As Short ' Temporary alias for LibIndex(I)

    Public NextraAC As Integer 'ikawa Jan11
    Public NAC As Short ' Number of aircraft in current section.
    Public jobNAC(MaxSects) As Short ' Number in each section in job file.
    Public ACName(MaxSectAC) As String 'Module FEDFAA1


    Public bSections(MaxSects) As Boolean


    Public GL(MaxSectAC) As Single ' Gross aircraft load for design.
    Public jobGL(MaxSects, MaxSectAC) As Single
    Public MGpcnt(MaxSectAC) As Single ' Added for A340-500/600. GFH 04/22/03.
    Public WT(MaxSectAC) As Single
    Public TW(MaxSectAC) As Single
    Public Contactarea(MaxSectAC) As Double
    Public TirePressureF(MaxSectAC) As Double

    Public ErrorMessages As List(Of String)

    'AirportMasterRecord Property
    'Public Property gPCN_Field39 As String


    'Public gPCNmax As Single

    'Public A200Flex_S(9), B120Flex_S(9), C80Flex_S(9), D50Flex_S(9) As Single
    'Public A200Flex_D(5), B120Flex_D(5), C80Flex_D(5), D50Flex_D(5) As Single
    'Public A200Flex_DD(5), B120Flex_DD(5), C80Flex_DD(5), D50Flex_DD(5) As Single
    'Public A200Flex_DD_D(4), B120Flex_DD_D(4), C80Flex_DD_D(4), D50Flex_DD_D(4) As Single
    'Public A200Flex_DDD(4), B120Flex_DDD(4), C80Flex_DDD(4), D50Flex_DDD(4) As Single

    'Public A200Rigid_S(9), B120Rigid_S(9), C80Rigid_S(9), D50Rigid_S(9) As Single
    'Public A200Rigid_D(5), B120Rigid_D(5), C80Rigid_D(5), D50Rigid_D(5) As Single
    'Public A200Rigid_DD(5), B120Rigid_DD(5), C80Rigid_DD(5), D50Rigid_DD(5) As Single
    'Public A200Rigid_DD_D(4), B120Rigid_DD_D(4), C80Rigid_DD_D(4), D50Rigid_DD_D(4) As Single
    'Public A200Rigid_DDD(4), B120Rigid_DDD(4), C80Rigid_DDD(4), D50Rigid_DDD(4) As Single


    'Public VV(9), D(5), DD(5), DD_D(4), DDD(4) As Single

    'Public s_MGW, d_MGW, dd_MGW, dd_d_MGW, ddd_MGW As Single
    'Public sPaveType, sSubgrType, sTireType As String




    Public gPCNmax As Single
    Public gPCN_Field39 As String

    'Public SubgradeCategory As String

    Public A200Flex_S(9), B120Flex_S(9), C80Flex_S(9), D50Flex_S(9) As Single
    Public A200Flex_D(10), B120Flex_D(10), C80Flex_D(10), D50Flex_D(10) As Single
    Public A200Flex_DD(10), B120Flex_DD(10), C80Flex_DD(10), D50Flex_DD(10) As Single
    Public A200Flex_DD_2D2(4), B120Flex_DD_2D2(4), C80Flex_DD_2D2(4), D50Flex_DD_2D2(4) As Single

    Public A200Rigid_S(9), B120Rigid_S(9), C80Rigid_S(9), D50Rigid_S(9) As Single
    Public A200Rigid_D(10), B120Rigid_D(10), C80Rigid_D(10), D50Rigid_D(10) As Single
    Public A200Rigid_DD(10), B120Rigid_DD(10), C80Rigid_DD(10), D50Rigid_DD(10) As Single
    Public A200Rigid_DD_2D2(4), B120Rigid_DD_2D2(4), C80Rigid_DD_2D2(4), D50Rigid_DD_2D2(4) As Single

    Public SSS123(9), D(10), DD(10), DD_2D2(4) As Single

    Public s_MGW, d_MGW, dd_MGW, dd_2d2_MGW As Single
    Public sPaveType, sSubgrType, sTireType As String








    Public ACName1(MaxSectAC) As String
    Public NACgraph As Integer = 0
    Public designlayernumber As Integer
    Public ACRThickDataFed(100, 5) As Single
    Public ACRThickSubgradeData(100, 5) As Single
    Public SubgradeCategoryCheck As List(Of Single)
    Public LastLayerModulus As Single
    Public ACNdataFF1 As ACRClassLib.clsACR.ACRdata
    Public ACROption As Boolean = True
    Public ACRSubCat1 As String
    Public ACRSubCat2 As String
    Public ACRSubCat3 As String
    Public ACRSubCat4 As String


    Public TotalDepart1(NAC * 2) As Single
    Public gCDF1, gCDF2 As Double
    Public gCDFmax1, gCDFmax2 As Double
    Public gCtoP1, gCtoP2 As Double

    Public CDFPCRScalingIssue As Double
    Public LifeCounterForPCRRuns As Integer


    Public Sub SaveACdata1()

        ReDim TotalDepart1(NAC * 2)
        ReDim PercentGross(NAC * 2)


        gCDF1 = jobCDFtable(ISect, 1)
        gCDFmax1 = jobCDFacrftMaxtable(ISect, 1)
        gCtoP1 = jobCtoPtable(ISect, 1)

        gCDF2 = jobCDFtable(ISect, 2)
        gCDFmax2 = jobCDFacrftMaxtable(ISect, 2)
        gCtoP2 = jobCtoPtable(ISect, 2)
        If DesignType = 11 Or DesignType = 12 Then
            'For K = 1 To 80
            '    CDFtableTemp3(K) = jobCDFtable(ISect, K)
            '    CDFacrftMaxtableTemp3(K) = jobCDFacrftMaxtable(ISect, K)
            'Next
            'For k1 = 1 To NAC + 1
            '    For k2 = 1 To 41
            '        CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
            '    Next
            'Next
        End If

        For ik1 As Integer = 1 To NAC
            TotalDepart1(ik1) = Reps(ik1)
            PercentGross(ik1) = MGpcnt(ik1)
        Next

    End Sub


    Public Sub RestoreACdata1()

        jobCDFtable(ISect, 1) = gCDF1
        jobCDFacrftMaxtable(ISect, 1) = gCDFmax1
        jobCtoPtable(ISect, 1) = gCtoP1

        jobCDFtable(ISect, 2) = gCDF2
        jobCDFacrftMaxtable(ISect, 2) = gCDFmax2
        jobCtoPtable(ISect, 2) = gCtoP2

        For ik1 As Integer = 1 To NAC
            Reps(ik1) = TotalDepart1(ik1)
            MGpcnt(ik1) = PercentGross(ik1)
        Next

    End Sub



    ' Type for passing aircraft data to LEA routine.
    Structure ACParms
        Dim GearLoad As Double
        Dim NTires As Integer
        Dim TirePress() As Double
        Dim TireX() As Double
        Dim TireY() As Double
        Dim NEvalPoints As Integer
        Dim EvalX() As Double
        Dim EvalY() As Double
        Dim Gear As String
        Dim GearOrientation As Integer
    End Structure

    'Zfindlife Porpeties and type
    Public iC1, iC2 As Integer
    Public globalLeft, globalTop As Integer


    Public cmdLifeOriginalX, cmdOriginalY As Integer
    Public cmdModifyStrOriginalX As Integer
    Public cmdAddDeleteOriginalX As Integer
    Public cmdSaveStrOriginalX As Integer
    Public DTemp As Double ' GFH 08/14/03.
    Public ThicktoSubgrade As Single
    Public LenLayerType As Single

    Public htmlText As String
    'Public gPictureStructure As New PictureBox

    Structure structureinfo
        Public Shared no(100) As String
        Public Shared type(100) As String
        Public Shared thickness(100) As String
        Public Shared modulus(100) As String
        Public Shared poissonRatio(100) As String
        Public Shared streagth(100) As String
        Public Shared length As Int16
        Private dummy As Int16
    End Structure

    Dim PaveStruct As String
    Dim TotalThickness As String
    Private FAArfieldHTMLPath As String

    Public LEAAircraft() As ACParms

    ' Pavement structure declarations.


    Public Const MaxNPLayers As Short = 24 'ikawa was 16
    Public Const MaxModulusNPLayers As Short = 32 ' Modification on 2/11/96
    Public Const NMO As Short = 1
    Public Const NTH As Short = 1
    Public Const NPVMNTS As Short = 1
    Public SectName As String
    Public jobSectName(MaxSects) As String
    Public NPLayers As Short
    Public julNPLayers As Short
    Public jobNPLayers(MaxSects) As Short
    Public Modulus(MaxNPLayers) As Single
    Public julModulus(MaxModulusNPLayers) As Single ' Modification on 2/11/96
    Public jobModulus(MaxSects, MaxNPLayers) As Single
    Public RCon(MaxNPLayers) As Single
    Public jobRCon(MaxSects, MaxNPLayers) As Single

    Public Thick(MaxNPLayers) As Single
    Public jobThick(MaxSects, MaxNPLayers) As Single
    Public julThick(MaxModulusNPLayers) As Single ' Modification on 2/11/96
    Public ttresh As Single 'kairat 7
    Public MinThickReached As Boolean = False 'izydor 2015
    Public LEDFAA13Thick As Single 'ikawa


    Public PoissonsRatio(MaxNPLayers) As Single ' Modification on 2012.12.21
    Public jobPoissonsRatio(MaxSects, MaxNPLayers) As Single ' Modification on 2012.12.21
    Public leafPoissonsRatio(MaxModulusNPLayers) As Single ' Modification on 2012.12.21


    Public LCode(MaxNPLayers) As Short
    Public julLCode(MaxModulusNPLayers) As Short ' Modification on 2/11/96
    Public jobLCode(MaxSects, MaxNPLayers) As Short
    Public SCIB As Single
    Public jobSCIB(MaxSects) As Single
    Public LifeExistPCC As Single
    Public jobLifeExistPCC(MaxSects) As Single
    Public CDFAsp As Single
    Public jobCDFAsp(MaxSects) As Single
    Public Designed As Date
    Public CompactionDesigned(MaxSects, MaxJobs) As Date  'added to record time for compaction design by YGC 102213
    Public jobDesigned(MaxSects) As Date
    Public Const NullDate As Date = #11/1/1971#


    Public SMin As String 'ikawa 2012.12.31
    Public jobSMin(MaxSects) As String 'ikawa 2012.12.31


    Structure StrParms
        Dim RCon As Double
        Dim NLayers As Integer
        Dim Thick() As Double
        Dim Modulus() As Double
        Dim Poisson() As Double
        Dim InterfaceCode() As Double
        Dim LayerCode() As Double
        Dim NEvalDepths As Integer
        Dim EvalDepth() As Double
    End Structure
    Public LEAStructure As StrParms
    Public julRCon As Single

    Public NEvalDepths As Short
    Public EvalDepth(10) As Double
    Public SubLayers As Boolean ' Julea sublayering on/off.


    Public Const NLayerTypes As Short = 22 ' See FEDFAA.GBL Sub InitLayers()
    Public LayerType(NLayerTypes) As String ' Text for LCode ID number.
    Public LayerTypeEx(NLayerTypes) As String ' Expanded type description.
    Public LayerTypeList(NLayerTypes) As String ' For dialog box list.
    Public LayerTypePic(NLayerTypes) As String ' For thestructure picture.


    ' Defaults are set in frmStructure.general.init().
    Public DefaultPoisson(NLayerTypes) As Single ' Poisson's Ratio.
    Public DefaultPoissonSGAC As Single
    Public DefaultPoissonSGPCC As Single
    Public DefaultModulus(NLayerTypes) As Single
    Public DefaultInterface(NLayerTypes) As Single
    Public ThickMin(NLayerTypes) As Single
    Public RCONMin, RCONMax As Single
    Public DefaultRCON As Single

    'Edward SCI = 100 
    Public Const SCI As Single = 80.0! 'to uncomment
    'Public Const SCI As Single = 100.0! 'to comment

    Public DefaultSCIB(NLayerTypes) As Single
    Public SCIBMin(NLayerTypes) As Single
    Public SCIBMax(NLayerTypes) As Single
    Public DefaultLifeExistPCC(NLayerTypes) As Single
    Public LifeExistPCCMin(NLayerTypes) As Single
    Public LifeExistPCCMax(NLayerTypes) As Single

    Public STRNH(MaxSectAC) As Single ' Maximum strain in asphalt layer.
    Public STRNV(MaxSectAC) As Single ' Maximum strain in subgrade layer.
    Public STRSH(MaxSectAC) As Single ' Maximum stress in concrete layer.

    Public RepsAnnual(MaxSectAC) As Single
    Public jobRepsAnnual(MaxSects, MaxSectAC) As Single
    Public RepsInc(MaxSectAC) As Single
    Public jobRepsInc(MaxSects, MaxSectAC) As Single
    Public Reps(MaxSectAC) As Single ' Number of departures.

    ' Declarations for overlays on rigid.
    Public Overflow1 As Boolean
    Public CrackGrowthRate As Single = 1.0
    Public PtoTC As Double = 1
    Public NSection As Short 'Number of increments in calculating delta SCI
    Public T2min, T1min As Single
    Public CurTT, CurDT As Single
    Public LogCA(MaxSectAC) As Double
    Public LogCE(MaxSectAC) As Double
    Public LastTT As Single
    Public Ijk As Short ' Times of calling Juleads.exe in the analysis
    Public Const SCIError As Single = 0.01! ' For controlling the precision of the calculated SCI
    Public LifeError As Single = 0.4 'was 0.04 ' For controlling the precision of pavement life
    Public OverlayLife As Single ' Computed rigid life for display.
    Public Const Ncontrol As Short = 9 'was 200 'ikawa  For controlling the maximum iteration number
    Public Const Ncontrol2 As Short = 200 'ikawa  For controlling the maximum iteration number


    ' GFH 4/30/95 - design termination on ThickError removed in frmStructure
    ' Sub DesignRigidOverlay because designed life could be very different than design life.
    Public Const ThickError As Single = 0.1 ' For controlling the precision of calculated thickness
    Public DThick As Single 'Step length for searching required thickness
    Public IZ As Short '  III%
    Public SCIE As Single ', SCIB!
    Public CurSCI, LastSCI, LastDT As Single
    Public CallowA(MaxSectAC) As Single
    Public CallowE(MaxSectAC) As Single
    Public CaEqual(MaxSectAC) As Single
    Public CEEqual(MaxSectAC) As Single
    Public Const DTInit As Single = 5.0!
    ' End rigid overlays.    ---------------------------------------------

    Public Life As Short ' Design life.
    Public jobLife(MaxSects) As Short
    Public Const DefaultLife As Short = 20
    Public Const MaxLife As Short = 50
    Public Const MinLife As Short = 1
    Public LifeStr As Single ' Structure life (for printing).

    'Public Const SIGMAW As Single = 30.475 ' Standard deviation of lateral wander.
    Public Const SIGMAW As Single = 30.435 ' Standard deviation of lateral wander.
    'Global Const SIGMAW! = 60      ' Standard deviation of lateral wander.

    'Public Const NND As String = "Undefined"
    Public Const NND As String = "User Defined"
    Public Const NAsp As String = "Asphalt"
    Public Const NBase As String = "Base"
    Public Const NSBase As String = "Subbase"
    Public Const NSG As String = "Subgrade"
    Public Const NPCC As String = "PCC"
    Public Const NAgBase As String = "Ag Crushed"
    Public Const NStBase As String = "St (Rigid)"
    Public Const NAgSBase As String = "Ag Uncrushed"
    Public Const NStSBase As String = "St (Flexible)"
    Public Const NACO As String = "AC Overlay"
    Public Const NPCCOU As String = "PCC Overlay Unbond"
    Public Const NPCCOB As String = "PCC Overlay Part Bond"
    Public Const NPCCOF As String = "PCC Overlay on Flex."
    Public Const NRBase As String = "Rubblized PCC Base"


    Public DesignType As Short
    Public Const InvalidDesignType As Short = 0
    Public Const NewFlex As Short = 1
    Public Const FlexOnFlex As Short = 2
    Public Const PCCOnFlex As Short = 3
    Public Const NewRigid As Short = 10
    Public Const UnbondOnRigid As Short = 11
    Public Const PartBondOnRigid As Short = 12
    Public Const FlexOnRigid As Short = 13
    Public OverlayRigOnRig As Boolean

    Public Property TrafficList As List(Of IAirplaneInfo)
    Public Property InitTraffic As Boolean

    ' Units set in frmStartup!chkUnitsConversion_Click
    Public Structure UnitsConversion
        Dim Metric As Boolean
        Dim inch As Double
        Dim inchName As String
        Dim inchFormat As String
        Dim pounds As Double
        Dim poundsName As String
        Dim poundsFormat As String
        Dim psi As Double
        Dim psiName As String
        Dim psiFormat As String
        Dim psiMPa As Double
        Dim psiMPaName As String
        Dim psiMPaFormat As String
        Dim pci As Double
        Dim pciName As String
        Dim pciFormat As String
        'ikawa March 25, 2008
        Dim inch2 As Double
        Dim inch2Name As String
        Dim inch2Format As String
    End Structure

    Public UnitsOut As UnitsConversion

    Public Sub ThreadTest()
        For i = 0 To 1000
            If gUserInterrupted Then
                Exit Sub
            End If
            Thread.Sleep(1000)
        Next
    End Sub



    Public Sub InitializeJob(job As IFaarFieldJob)

        DesigningStr = True
        ComputeAircraftCDF = True
        'gUseCompaction = job.DesignOptions.ComputeCompaction
        gPtoC1 = False 'frmStartup
        gConstTirePressure = False 'frmStartup
        gHMAonRigid_Mod = True
        PrintResultsCDF = True

        gNewModulus_P154 = True
        gP154_C688D156 = True
        gP209_C1052D20 = True

        gNewModulus_P209 = True
        gBleasdaleModel = True
        gTandemFnew = True

        gRDEC = False

        gSolverType = 0 'EBE solver YGC 083012
        gSlabMeshSize = 5 'YGC 061113

        g2Dgear = 90
        g3inchPCC = False
        g2inchHMA = False

        gRigidNewParam = False
        gFSlopeNewValue = False
        gRigidCalibrationNewValue = False

        DesigningP209 = job.DesignOptions.AutomaticFlexibleBaseDesign
        OutPutFile = job.DesignOptions.Outfile
        CDFExitErr = job.DesignOptions.CdfTolerance
        LifeError = job.DesignOptions.LifeTolerance
        If job.DesignOptions.CalculateHmaCdf Then
            NoACCDF = False
        Else
            NoACCDF = True
        End If
        If job.DesignOptions.AutomaticFlexibleBaseDesign Then
            DesigningP209 = True
        End If
        If job.DesignOptions.Outfile Then
            OutPutFile = True
        End If

        AlternateSG = job.DesignOptions.AlternateSubgrade
        NSection = job.DesignOptions.SectionParameterN
        CrackGrowthRate = job.DesignOptions.CrackPropogation.UsCustomary
        If job.DesignOptions.Outfile = True Then
            NoOutFiles = False
        Else
            NoOutFiles = True
        End If
        ACROption = job.DesignOptions.ACROptions

        bPartBondonRigid = job.DesignOptions.AllowPartiallyBonded
        '<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        Dim path As String
        'If (Not NoOutFiles) Then 'ik2020.03
        '    WorkingDir = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\"
        '    path = WorkingDir & "PrintOut-" & gOutputDirName & "\"
        '    If Not Directory.Exists(path) Then
        '        Directory.CreateDirectory(path)
        '    End If
        '    WorkingDir = path
        'End If


        'WorkingDir = SpecialDirectories.MyDocuments + "\My FAARFIELD\Temporary" ik2020.03
        'If NoOutFiles Then
        WorkingDir = SpecialDirectories.MyDocuments + "\My FAARFIELD" 'ik2020.03
        gOutputDirName = JobName 'ik2020.03
        ' End If

        'Directory.CreateDirectory(WorkingDir) ik2020.03
        WorkingDir = WorkingDir & "\"

        With UnitsOut
            If TypeOf (job.DesignOptions.MeasurementSystem) Is Metric Then
                '     Multiply by these to output from English to Metric.
                '     Divide by these to input from Metric to English.
                .Metric = True
                .inch = 25.4 ' to mm
                .inchName = "mm"
                .inchFormat = "#,##0.0" ' "#,##0.0" ' GFH 3/4/03.

                .inch2 = 25.4 * 25.4 ' to mm
                .inch2Name = "mm^2"
                .inch2Format = "#,##0.0" ' "#,##0.0"

                .pounds = 1 / 2204.6225 ' to tonnes
                .poundsName = "tonnes"
                .poundsFormat = "#,##0.000"
                .psi = 6.894757 ' to kPa
                .psiName = "kPa"
                .psiFormat = "#,##0"
                .psiMPa = 0.006894757 ' to MPa
                .psiMPaName = "MPa"
                .psiMPaFormat = "#,##0.00"
                '.pci = 0.27144703 ' to kg/cm^3
                .pci = 0.27144716 ' to MN/m^3
                '.pciName = "kg/cm^3"
                .pciName = "MN/m^3"
                .pciFormat = "#,##0.0"
            Else
                '     Use these to input/output from English to English.
                .Metric = False
                .inch = 1
                .inchName = "in"
                .inchFormat = "#,##0.00" ' "#,##0.00" ' GFH 3/4/03. Change for more precision in output.

                .inch2 = 1
                .inch2Name = "in.^2"
                .inch2Format = "#,##0.00" ' "#,##0.00" 

                .pounds = 1
                .poundsName = "lbs"
                .poundsFormat = "#,###,##0"
                .psi = 1
                .psiName = "psi"
                .psiFormat = "#,##0"
                .psiMPa = 1
                .psiMPaName = "psi"
                .psiMPaFormat = "#,##0"
                .pci = 1
                .pciName = "pci"
                .pciFormat = "#,##0.0"
            End If
        End With




    End Sub




    Function LPad(ByRef N As Integer, ByRef SS As String) As String
        ' Adds leading spaces to variant string SS to make it N characters long.
        ' Used to format output to a file. #### characters in a Format function
        ' do not force spaces like QuickBasic.
        ' Typically, SS = Format(XX, "0.00")

        ITemp = CShort(Len(SS))
        If N - ITemp < 1 Then N = ITemp + 1
        LPad = Space(N - ITemp) & SS
    End Function


    Function MsgBoxDQ(ByRef Caption As String, ByVal Idq As Short, ByRef Title As String) As Long
        'If DemoState Then
        '    If Idq = 0 Then
        '        Caption = Caption & NL2 & "Click OK to continue."
        '    Else
        '        Caption = Caption & NL2 & "Click the appropriate button to continue."
        '    End If
        'End If
        'MsgBoxDQ = MsgBox(Caption, CType(Idq, Microsoft.VisualBasic.MsgBoxStyle), Title)
        ''ikawa MsgBoxDQ = MsgBox(Caption, MsgBoxStyle.OKOnly, Title)
        Return 0
    End Function

    Sub ErrorMessageForUser(message As String, caption As String)

    End Sub

    ''' <summary>
    ''' Set values for current section
    ''' </summary>
    ''' <param name="job">Input FAARFIELD Job</param>
    ''' <param name="section">FAARFIELD Section</param>
    Public Sub SetCurrentSectData(job As IFaarFieldJob, section As ISection) ', layerIndex As Integer
        ReDim bSections(MaxSects)

        Dim II, J As Short, Flag As Boolean
        Static I As Short

        ' Transfer RDEC properties from section model to engine globals
        gFlexuralMod(ISect) = section.RdecFlexuralMod
        gAirVoids(ISect) = section.RdecAirVoids
        gAsphaltContentByVol(ISect) = section.RdecAsphaltContentByVol
        gPNMS(ISect) = section.RdecPNMS
        gPPCS(ISect) = section.RdecPPCS
        gP200(ISect) = section.RdecP200

        gFlexuralMod1 = gFlexuralMod(ISect)
        gAirVoids1 = gAirVoids(ISect)
        gAsphaltContentByVol1 = gAsphaltContentByVol(ISect)

        gPNMS1 = gPNMS(ISect)
        gPPCS1 = gPPCS(ISect)
        gP2001 = gP200(ISect)


        SectName = jobSectName(ISect)
        Designed = jobDesigned(ISect)

        SMin = jobSMin(ISect) '2013.01.03


        CDFAsp = jobCDFAsp(ISect)
        SCIB = jobSCIB(ISect)
        LifeExistPCC = jobLifeExistPCC(ISect)

        'Make sure layer defaults are set before setting current materials
        InitLayers()

        'Load AC Library
        InitAcLib()

        'Set all job defaults for current job
        InitializeJob(job)

        ErrorMessages = New List(Of String)

        'All Outputs are stored in ISect = 0
        'Previous there was a global array that stored all values by sectionID
        'We are now using just a single value of this array to store output returns (mostly CDF)
        ISect = 1

        'Calculations in FAARFIELD are actually performed in USCustomary (why?)
        'This flag is set so that reports and the like can properly generate
        If (TypeOf (job.DesignOptions.MeasurementSystem) Is UsCustomary) Then
            EnglishUnits = True
        Else
            EnglishUnits = False
        End If

        LifeComputation = Not section.ThicknessOptimization
        'IterLayerChosen = layerIndex
        SectName = section.Name
        Designed = DateTime.Now
        SMin = DateTime.Now

        'CDFAsp = section.PercentCdfu
        SCIB = section.Sci
        LifeExistPCC = section.PercentCdfu
        PtoTC = section.PtoTC
        NPLayers = section.Layers.Count

        For I = 0 To section.Layers.Count - 1
            If Not section.Layers.Item(I).DesignedLayer = "" Then
                designlayernumber = I + 1
            End If
        Next

        UndefinedLayer = False
        For I = 1 To NPLayers
            Thick(I) = section.Layers(I - 1).Thickness.UsCustomary
            Modulus(I) = section.Layers(I - 1).Modulus.UsCustomary
            'add RCon array to data stream. change code to put RCon(i) into the RCon array (not Modulus). Retain modulus
            If section.Layers(I - 1).Rupture Is Nothing Then
                RCon(I) = 0
            Else
                RCon(I) = section.Layers(I - 1).Rupture.UsCustomary 'user input
            End If


            LCode(I) = section.Layers(I - 1).LayerCode
            PoissonsRatio(I) = DefaultPoisson(section.Layers(I - 1).LayerCode)
            If LayerType(LCode(I)) = NND Then UndefinedLayer = True
        Next I

        'ACR Data



        SubgradeCategoryCheck = New List(Of Single)


        For I = 0 To 5
            SubgradeCategoryCheck.Add(0)
        Next

        SubgradeCategoryCheck(1) = 2000
        SubgradeCategoryCheck(2) = 10000
        SubgradeCategoryCheck(3) = 20000
        SubgradeCategoryCheck(4) = 35000

        LastLayerModulus = Modulus(NPLayers)

        ' Aircraft data.
        NAC = section.Airplanes.Count
        ReDim CalcStress(NAC * 2)

        Flag = False
        Life = section.Life
        II = 0  ' Number of missing a/c, see below.


        For I = 1 To NAC
            ACName(I) = section.Airplanes(I - 1).Name
            Call CheckAirplaneName(ACName(I))
            jobACName(ISect, I) = ACName(I) 'ikawa

            LibIndex(I) = I
            GL(I) = section.Airplanes(I - 1).GrossWeight.UsCustomary
            RepsAnnual(I) = section.Airplanes(I - 1).NumberDepartures
            RepsInc(I) = section.Airplanes(I - 1).AnnualGrowth / 100.0!
        Next I


        '        For I = 1 To NAC
        'RepeatLink:
        '            ACName(I) = section.Airplanes(I - 1).Name
        '            Call CheckAirplaneName(ACName(I))
        '            jobACName(ISect, I) = ACName(I) 'ikawa

        '            Dim MatchFound As Boolean

        '            ' Set up indirect link from a load aircraft to the library list.

        '            For J = 1 To libNAC ' All aircraft in the library.
        '                MatchFound = (ACName(I) = AC(J).libACName)

        '                If (Mid(ACName(I), 1, 2) = "B-") And Not MatchFound Then
        '                    MatchFound = ("B" + Mid(ACName(I), 3) = AC(J).libACName)
        '                    If MatchFound Then
        '                        ACName(I) = "B" + Mid(ACName(I), 3)
        '                        jobACName(ISect, I) = ACName(I)
        '                    End If
        '                ElseIf (Mid(ACName(I), 1, 2) = "A-") And Not MatchFound Then
        '                    MatchFound = ("A" + Mid(ACName(I), 3) = AC(J).libACName)
        '                    If MatchFound Then
        '                        ACName(I) = "A" + Mid(ACName(I), 3)
        '                        jobACName(ISect, I) = ACName(I)
        '                    End If
        '                End If

        '                If MatchFound Then
        '                    'ACName(I) = AC(J).libACName
        '                    LI = J ' Set link.
        '                    Flag = True
        '                    Exit For ' Stop looking for match.
        '                End If
        '            Next J


        '            If Flag = False Then
        '                II = II + 1S
        '                S = "Airplane " & ACName(I) & " cannot be found in the Library List." & NL
        '                S = S & "It has not been included in the airplane list" & NL
        '                S = S & "for job " & JobName & " and section " & SectName & "."

        '                'AirplaneNotThere = True
        '                bSections(ISect) = True

        '                'Ret = MsgBoxDQ(S, 0, "Airplane Not There")
        '                Ret = MsgBoxDQ(S, 0, "Airplane Not Found")

        '                'AirplaneNotThere = False

        '                If I < NAC - II + 1 Then
        '                    For J = I To NAC - 1S
        '                        jobACName(ISect, J) = jobACName(ISect, J + 1)
        '                        jobGL(ISect, J) = jobGL(ISect, J + 1)
        '                        jobRepsAnnual(ISect, J) = jobRepsAnnual(ISect, J + 1)
        '                        jobRepsInc(ISect, J) = jobRepsInc(ISect, J + 1)
        '                    Next J
        '                    GoTo RepeatLink
        '                End If
        '            Else

        '                LibIndex(I) = LI
        '                GL(I) = section.Airplanes(I - 1).GrossWeight.UsCustomary
        '                RepsAnnual(I) = section.Airplanes(I - 1).NumberDepartures
        '                RepsInc(I) = section.Airplanes(I - 1).AnnualGrowth / 100.0!
        '            End If

        '        Next I

        ' NAC = NAC - II
        jobNAC(ISect) = NAC

        Dim bFF13 As Boolean
        bFF13 = False
        ' I = 0 'Check for A380 and B747 ============================================

        Do
StartAgain:
            I = I + 1S
            If (Mid(ACName(I), 1, 4) = "A380") Or (Mid(ACName(I), 1, 4) = "B747") Then

                If I = NAC Then
                    NAC = NAC + 1S
                    jobNAC(ISect) = NAC

                    ACName(NAC) = ACName(I) & " Belly"
                    GL(NAC) = GL(I)
                    RepsAnnual(NAC) = RepsAnnual(I)
                    RepsInc(NAC) = RepsInc(I)

                    jobACName(ISect, NAC) = jobACName(ISect, I) & " Belly"
                    jobGL(ISect, NAC) = jobGL(ISect, I)
                    jobRepsAnnual(ISect, NAC) = jobRepsAnnual(ISect, I)
                    jobRepsInc(ISect, NAC) = jobRepsInc(ISect, I)

                    Dim MatchFound As Boolean
                    For J = 1 To libNAC ' All aircraft in the library.
                        MatchFound = (ACName(NAC) = AC(J).libACName)
                        If MatchFound Then
                            LI = J ' Set link.
                            LibIndex(NAC) = LI
                            Exit For
                        End If
                    Next J
                    I = I + 1S
                    Exit Do
                Else '==============================================================
                    If ACName(I + 1) = ACName(I) & " Belly" Then
                        Exit Do
                    ElseIf InStr(ACName(I), "Belly") > 0 Then
                        GoTo StartAgain
                    Else
                    End If

                    NAC = NAC + 1S
                    jobNAC(ISect) = NAC

                    For k1 As Short = NAC To I + 2S Step -1
                        ACName(k1) = ACName(k1 - 1)
                        GL(k1) = GL(k1 - 1)
                        RepsAnnual(k1) = RepsAnnual(k1 - 1)
                        RepsInc(k1) = RepsInc(k1 - 1)

                        jobACName(ISect, k1) = jobACName(ISect, k1 - 1)
                        jobGL(ISect, k1) = jobGL(ISect, k1 - 1)
                        jobRepsAnnual(ISect, k1) = jobRepsAnnual(ISect, k1 - 1)
                        jobRepsInc(ISect, k1) = jobRepsInc(ISect, k1 - 1)
                        LibIndex(k1) = LibIndex(k1 - 1)
                    Next

                    ACName(I + 1) = ACName(I) & " Belly"
                    GL(I + 1) = GL(I)
                    RepsAnnual(I + 1) = RepsAnnual(I)
                    RepsInc(I + 1) = RepsInc(I)

                    jobACName(ISect, I + 1) = jobACName(ISect, I) & " Belly"
                    jobGL(ISect, I + 1) = jobGL(ISect, I)
                    jobRepsAnnual(ISect, I + 1) = jobRepsAnnual(ISect, I)
                    jobRepsInc(ISect, I + 1) = jobRepsInc(ISect, I)
                    'LibIndex(I + 1) = LibIndex(I)

                    Dim MatchFound As Boolean
                    For J = 1 To libNAC ' All aircraft in the library.
                        MatchFound = (ACName(I + 1) = AC(J).libACName)
                        If MatchFound Then
                            LI = J ' Set link.
                            LibIndex(I + 1) = LI
                            Exit For
                        End If
                    Next J
                End If
            End If

        Loop While I <= NAC

        Call CheckAdvisoryRequirements()
        Call SetDesignType()
        Call UpdateCurrentSectData()

    End Sub


    Sub SetDesignType()

        OverlayRigOnRig = False
        S = LayerType(LCode(1))
        SS = LayerType(LCode(2))
        If S = NAsp Or (S = NND And Not (SS = NPCC Or SS = NAsp)) Then
            DesignType = NewFlex 'SetDesignType
        ElseIf (S = NACO And SS = NAsp) Or (S = NND And SS = NAsp) Or (S = NACO And SS = NND) Then
            DesignType = FlexOnFlex
        ElseIf S = NPCC Then
            DesignType = NewRigid
        ElseIf (S = NPCCOF And SS = NAsp) Or (S = NPCCOF And SS = NND) Then
            DesignType = PCCOnFlex
        ElseIf SS = NPCC And S = NPCCOU Then
            DesignType = UnbondOnRigid
            OverlayRigOnRig = True
        ElseIf SS = NPCC And S = NPCCOB Then
            DesignType = PartBondOnRigid
            OverlayRigOnRig = True
        ElseIf SS = NPCC And (S = NACO Or S = NND) Then
            DesignType = FlexOnRigid
            OverlayRigOnRig = True
        ElseIf (S = NACO) And (SS = "Rubblized PCC Base") Then
            DesignType = FlexOnFlex
        Else
            DesignType = InvalidDesignType
        End If

    End Sub

    Sub UpdateCurrentSectData()

        Dim I As Short
        Dim wingGearLoad, bellyGearLoad As Double
        Dim bA340 As Boolean

        GoodACList = True
        If NAC = 2 Then
            If AC(LibIndex(NAC - 1)).libGear = "H" Then ' Belly gear aircraft.
                GoodACList = False
            End If
        ElseIf NAC = 1 Then
            GoodACList = False
        End If

        '  B777inMix = False ' GFH 04/07/03.
        For I = 1 To NAC
            '    If Left$(ACName$(I), 5) = "B-777" Then B777inMix = True ' GFH 04/07/03.
            LI = LibIndex(I)
            'If ACName(I) = "A340-500/600" Or (ACName(I) = "A340-500/600" & BellyExt) Then ' GFH 04/22/03.
            'If (Mid(ACName(I), 1, 8) = "A340-500" Or Mid(ACName(I), 1, 8) = "A340-600") Then ' GFH 04/22/03.
            '    If AC(LI).libMGpcnt * GL(I) <= 634930 Then
            '        wingGearLoad = 148494 + AC(LI).libMGpcnt * GL(I) * 0.422 ' For two wing gears.
            '    Else
            '        wingGearLoad = -132127 + AC(LI).libMGpcnt * GL(I) * 0.864 ' For two wing gears.
            '    End If
            '    'If ACName(I) = "A340-500/600" Then
            '    If (Mid(ACName(I), 1, 8) = "A340-500" Or Mid(ACName(I), 1, 8) = "A340-600") And (InStr(4, ACName(I), "Belly", CompareMethod.Text) = 0) Then
            '        MGpcnt(I) = CSng(wingGearLoad / 2 / GL(I)) ' Convert to one wing gear for load response.
            '    Else
            '        bellyGearLoad = AC(LI).libMGpcnt * GL(I) - wingGearLoad
            '        MGpcnt(I) = CSng(bellyGearLoad / GL(I)) ' One belly gear for load response.
            '    End If
            'Else
            'bA340 = CheckForA340(I)
            'If bA340 Then
            '    MGpcnt(I) = AC(LI).libMGpcnt / 2
            'ElseIf AC(LI).libACName.Contains("MD-11") And AC(LI).libACName.Contains("Belly") Then
            '    MGpcnt(I) = AC(LI).libMGpcnt
            'ElseIf AC(LI).libACName.Contains("30/30F/40") And AC(LI).libACName.Contains("Belly") Then
            '    MGpcnt(I) = AC(LI).libMGpcnt
            'ElseIf AC(LI).libACName.Contains("IL-86") And AC(LI).libACName.Contains("Belly") Then
            '    MGpcnt(I) = AC(LI).libMGpcnt
            'ElseIf AC(LI).libACName.Contains("KC-10") And AC(LI).libACName.Contains("Belly") Then
            '    MGpcnt(I) = AC(LI).libMGpcnt
            'Else

            MGpcnt(I) = AC(LI).libMGpcnt / 2
            'End If


            Temp = GL(I) * MGpcnt(I) / AC(LI).libNTires / AC(LI).libCP ' End GFH 04/22/03.
            'GL(1) = 400000
            'Temp = GL(I) * MGpcnt(I) / AC(LI).libNTires / AC(LI).libCP ' End GFH 04/22/03.

            'ikawa95 March 11, 2008 constant tire area
            If gConstTirePressure Then
                'constant tire pressure
            Else
                Temp = Temp * AC(LI).libGL / GL(I) ' constant tire area
            End If


            ''Temp - constant tire area
            'If (Mid(ACName(I), 1, 8) = "A340-500" Or Mid(ACName(I), 1, 8) = "A340-600") Then
            '    If AC(LI).libMGpcnt * GL(I) <= 634930 Then
            '        wingGearLoad = 148494 + AC(LI).libMGpcnt * AC(LI).libGL * 0.422 ' For two wing gears.
            '    Else
            '        wingGearLoad = -132127 + AC(LI).libMGpcnt * AC(LI).libGL * 0.864 ' For two wing gears.
            '    End If

            '    If (Mid(ACName(I), 1, 8) = "A340-500" Or Mid(ACName(I), 1, 8) = "A340-600") _
            '        And (InStr(4, ACName(I), "Belly", CompareMethod.Text) = 0) Then
            '        Temp = CSng(wingGearLoad / 2 / AC(LI).libNTires / AC(LI).libCP)
            '    Else
            '        bellyGearLoad = AC(LI).libMGpcnt * AC(LI).libGL - wingGearLoad
            '        Temp = CSng(bellyGearLoad / 2 / AC(LI).libNTires / AC(LI).libCP)
            '    End If
            'End If
            'Temp = Temp * AC(LI).libGL / GL(I)
            '    Temp = GL(I) * libMGpcnt(LI) / libNTires(LI) / libCP(LI)
            '   Contact Area = (Tire Load) / (Tire pressure)
            '   Area of ellipse = a * b * pi
            WT(I) = CSng(2 * Math.Sqrt(Temp / (1.6 * 3.14159))) ' Minor axis.
            TW(I) = CSng(WT(I) * 1.6) ' Major axis.
            Contactarea(I) = Temp
            'Dim isbelly As Boolean
            'isbelly = False



            'If Mid(ACName(I), 15, 19) = "Belly" Then
            '    isbelly = True
            'End If
            ''            
            'If ACName(I) = "A380 Belly" Then
            '    isbelly = True
            'End If

            'If ACName(I) = "A380e Belly" Then
            '    isbelly = True
            'End If

            'If ACName(I) = "B747-100 SF Belly" Then
            '    isbelly = True
            'End If

            'If ACName(I) = "B747-200B Combi Mixed Belly" Then
            '    isbelly = True
            'End If

            'If ACName(I) = "B747-300 Combi Mixed Belly" Then
            '    isbelly = True
            'End If
            'If ACName(I) = "B747-400 Belly" Then
            '    isbelly = True
            'End If

            'If ACName(I) = "B747-400ER Belly" Then
            '    isbelly = True
            'End If

            'If ACName(I) = "B747-8 Belly" Then
            '    isbelly = True
            'End If
            'If ACName(I) = "B747-8F Belly" Then
            '    isbelly = True
            'End If
            'If ACName(I) = "B747-SP Belly" Then
            '    isbelly = True
            'End If
            'If ACName(I).Contains("Belly") Then
            '    isbelly = True
            'End If
            'If isbelly = False Then
            TirePressureF(I) = (GL(I) * AC(LI).libMGpcnt / (Contactarea(I) * AC(LI).libNTires))
            'Else
            '    TirePressureF(I) = (1 / 4) * (GL(I) * AC(LI).libMGpcnt / (Contactarea(I) * AC(LI).libNTires))
            'End If

            Temp1 = Life
            If 1.0! + RepsInc(I) * Life < 0.0! Then
                Temp1 = -1.0! / RepsInc(I)
            End If
            Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
            Reps(I) = Temp * RepsAnnual(I) * Temp1

        Next I

    End Sub

    Public Sub CheckAdvisoryRequirements()
        Dim I As Integer, bCond1, bCond2 As Boolean
        For I = 1 To MaxNPLayers : AC_layer_min(MaxNPLayers) = 0 : Next

        Call InitLayers() 'CheckAdvisoryRequirements()
        Call SetDesignType() 'CheckAdvisoryRequirements()
        lightAircraft_12_5kLess = True
        gP208_60kMoreCase = False     'for new flexible
        lightAircraft60kMore = False 'for flexible
        Aircraft_100t_lbs = False

        Dim isTruck As Boolean
        For I = 1 To NAC
            isTruck = False
            If ACName(I).IndexOf("Truck") >= 0 Or ACName(I).IndexOf("Vehicle") >= 0 Then
                isTruck = True
            End If

            If GL(I) > 60000 And (Not isTruck) Then
                lightAircraft_12_5kLess = False
            End If

            If GL(I) >= 100000 And (Not isTruck) Then
                Aircraft_100t_lbs = True
            End If

            If GL(I) > 60000 And (Not isTruck) Then 'new flexible with P-208
                lightAircraft60kMore = True
            End If
        Next I

        lightAC_12_5_to_less_100k = Not (lightAircraft_12_5kLess Or Aircraft_100t_lbs)

        '*****************************************************
        '*****        Rigid pavement requirements        *****
        '*****************************************************
        If DesignType = NewRigid Then  'LayerTypeEx$(5) = "P-501 PCC Surface"
            If lightAircraft_12_5kLess Then
                ThickMin(5) = ChooseNumber(5)
            Else
                ThickMin(5) = ChooseNumber(6) ' default ThickMin(5) = 5.0!
            End If
            If Thick(1) < ThickMin(5) And (Not g3inchPCC) Then
                Thick(1) = ThickMin(5)
            End If

            ' layer under PCC surface
            bCond1 = (LCode(2) = 6) Or (LCode(2) = 18) Or (LCode(2) = 21) Or (LCode(2) = 15)
            bCond2 = (LCode(2) = 14) Or (LCode(2) = 16) Or (LCode(2) = 17)

            'If Aircraft_100t_lbs Then
            '    If bCond1 Then
            '        If Thick(2) < ChooseNumber(6) Then
            '            Thick(2) = ChooseNumber(6)
            '        End If
            '    ElseIf bCond2 Then
            '        If Thick(2) < ChooseNumber(5) Then
            '            Thick(2) = ChooseNumber(5)
            '        End If
            '    End If
            'Else
            '    If Thick(2) < ChooseNumber(4) And (LCode(2) <> 0) Then
            '        Thick(2) = ChooseNumber(4)
            '    End If
            'End If

            If Not lightAircraft_12_5kLess Then
                ThickMin(6) = ChooseNumber(6)
                ThickMin(18) = ChooseNumber(6)
                ThickMin(15) = ChooseNumber(6)
                ThickMin(21) = ChooseNumber(6)

                ThickMin(14) = ChooseNumber(5)
                ThickMin(16) = ChooseNumber(5)
                ThickMin(17) = ChooseNumber(5)
            End If


        End If 'NewRigid ----------------------------------------------




        '*****************************************************
        '*****      Flexible pavement requirements      ******
        '*****************************************************
        If DesignType = NewFlex Then 'Table 3-3 of 6F page 3-17
            If lightAircraft_12_5kLess Then
                ThickMin(1) = ChooseNumber(3) '"P-401 Asphalt Surface"
            Else
                ThickMin(1) = ChooseNumber(4)
            End If

            bCond1 = (LCode(2) = 14) Or (LCode(2) = 16) Or (LCode(2) = 17)
            If Aircraft_100t_lbs Then
                If bCond1 Then
                    If Thick(2) < ChooseNumber(5) Then
                        Thick(2) = ChooseNumber(5)
                    End If
                End If
            End If

            If lightAircraft_12_5kLess Then
                If LCode(2) = 6 Then 'P-209
                    If ThickMin(6) < ChooseNumber(6) Then
                        ThickMin(6) = ChooseNumber(6) 'Minimum thickness for P-209 with Thickness Design run = 6
                    End If
                Else
                    ThickMin(6) = ChooseNumber(3)
                    ThickMin(18) = ChooseNumber(3)
                End If
            Else
                ThickMin(6) = ChooseNumber(6)
                ThickMin(18) = ChooseNumber(6)
            End If

            'If LCode(2) = 6 Then 'P-209
            '    If lightAircraft_12_5kLess Then
            '        If Thick(2) < ChooseNumber(3) Then
            '            Thick(2) = ChooseNumber(3)
            '        End If
            '    Else
            '        If Thick(2) < ChooseNumber(6) Then
            '            Thick(2) = ChooseNumber(6)
            '        End If
            '    End If
            'End If

            If LCode(2) = 18 Then 'P-208
                If lightAircraft60kMore Then
                    gP208_60kMoreCase = True
                End If
            End If

            'If LCode(2) = 18 Then 'P-208
            '    If lightAircraft_12_5kLess Then
            '        If Thick(2) < ChooseNumber(3) Then
            '            Thick(2) = ChooseNumber(3)
            '        End If
            '    Else
            '        If lightAircraft60kMore Then
            '            gP208_60kMoreCase = True
            '        End If
            '    End If
            'End If
        End If 'NewFlex ----------------------------------------------


        If g2inchHMA Then
            ThickMin(1) = ChooseNumber(2)
        End If

        If g3inchPCC Then
            ThickMin(5) = ChooseNumber(3)
            ThickMin(11) = ChooseNumber(3)
        End If

        If DesignType = FlexOnFlex Or DesignType = PCCOnFlex Then
            ThickMin(1) = ChooseNumber(1) 'default ThickMin(1) = 4.0!
        End If

        'Enforce thickness requirements
        For I = 1 To NPLayers - 1
            If Thick(I) < ThickMin(LCode(I)) Then
                Thick(I) = ThickMin(LCode(I))
            End If
        Next



    End Sub

    Public Function ChooseNumber(ByRef val1 As Single) As Single

        If val1 = 1 Then
            If EnglishUnits Then
                ChooseNumber = 1.0!
            Else
                ChooseNumber = 0.984252!
            End If
        ElseIf val1 = 2 Then
            If EnglishUnits Then
                ChooseNumber = 2.0!
            Else
                ChooseNumber = 1.968504!
            End If
        ElseIf val1 = 3 Then
            If EnglishUnits Then
                ChooseNumber = 3.0!
            Else
                ChooseNumber = 2.95276!
            End If
        ElseIf val1 = 4 Then
            If EnglishUnits Then
                ChooseNumber = 4.0!
            Else
                ChooseNumber = 3.937008!
            End If
        ElseIf val1 = 5 Then
            If EnglishUnits Then
                ChooseNumber = 5.0!
            Else
                ChooseNumber = 4.921226!
            End If
        ElseIf val1 = 6 Then
            If EnglishUnits Then
                ChooseNumber = 6.0!
            Else
                ChooseNumber = 5.905512!
            End If
        End If

        'Hard conversion 
        Dim A2to050cm, A4to100cm, A5to125cm As Single
        A2to050cm = 1.968504! 'gives 50 cm
        A4to100cm = 3.937008! 'gives 100 cm
        A5to125cm = 4.92126! 'gives 125 cm


    End Function

    Public Function AutomaticDesign() As Boolean
        numberoferrors = False
        'Design for P-209 only for base layer P-209 and subbase P-154 then
        'Design for LayerTypeEx$(14) = "P-401 Stabilized Base (flexible)" and P-209
        'LayerTypeEx$(6)  = "P-209 Crushed Aggregate Base Course"
        'LayerTypeEx$(8)  = "P-154 Subbase Course (uncrushed aggregate)"
        'LayerTypeEx$(14) = "P-401 Stabilized Base (flexible)"
        'Automatic design for flex stabilzed base and P-154 (not P-209)

        If Not DesigningP209 Then
            AutomaticDesign = False
            Exit Function
        End If

        Dim NP4, NP5 As Boolean

        If NPLayers = 4 Then NP4 = True
        If NPLayers = 5 Then NP5 = True

        If Not (NP4 Or NP5) Then
            AutomaticDesign = False
            Exit Function
        End If

        Dim case01, case02, case20, case03, case04, case05 As Boolean
        Dim case06, case07, case08, case09, case10 As Boolean
        Dim caseP209_P219_Layer2, caseP209_P219_Layer3 As Boolean 'IK 2016.05.31

        caseP209_P219_Layer2 = (LCode(2) = 6) Or (LCode(2) = 19)
        caseP209_P219_Layer3 = (LCode(3) = 6) Or (LCode(3) = 19)


        case01 = NP4 And (caseP209_P219_Layer2) And (LCode(3) = 8)
        case02 = NP4 And (LCode(2) = 14) And (caseP209_P219_Layer3)
        case20 = NP4 And (LCode(2) = 14) And (LCode(3) = 19)
        case03 = NP4 And (LCode(2) = 14) And (LCode(3) = 8)

        case04 = NP5 And (LCode(2) = 14) And (caseP209_P219_Layer3) _
                                                   And (LCode(4) = 8)

        case05 = NP4 And ((LCode(2) = 18) Or (LCode(2) = 21)) _
                                                   And (LCode(3) = 8)
        case06 = NP5 And (LCode(2) = 14) And ((LCode(3) = 18) _
                    Or (LCode(3) = 21)) And (LCode(4) = 8)

        case07 = NP4 And ((LCode(2) = 16) Or (LCode(2) = 17)) _
                                                    And (caseP209_P219_Layer3)
        case08 = NP4 And ((LCode(2) = 16) Or (LCode(2) = 17)) _
                                                    And (LCode(3) = 8)

        case09 = NP5 And ((LCode(2) = 16) Or (LCode(2) = 17)) _
                     And ((LCode(3) = 18) Or (caseP209_P219_Layer3) _
                     Or (LCode(3) = 21)) And (LCode(4) = 8)

        case10 = NP4 And (LCode(2) = 9) And (LCode(3) = 8)


        Dim DesignP209onP154 As Boolean
        Dim DesignP401StabilonP154 As Boolean
        DesignP209onP154 = DesigningP209 And (Not LifeComputation) And LCode(2) = 6 And LCode(3) = 8
        DesignP401StabilonP154 = DesigningP209 And (Not LifeComputation) And LCode(2) = 14 And LCode(3) = 8

        If case01 Or case03 Or case05 Or case08 Or case10 Then
            DesigningP209DrawStructure = False
            Call SetData_DesignBase_SubgadeCBR20_4Layers()
            IterLayerChosen = 2
            ILayer = 2
            StraightLineModel = -1
            Call LeafDesignFlex()

            DesigningP209DrawStructure = True
            Call DesignRestoreStructure_4Layers()
            IterLayerChosen = 3
            ILayer = 3
            StraightLineModel = -1
            Call LeafDesignFlex()

        ElseIf case02 Or case07 Or case20 Then
            If Not LifeComputation Then
                Thick(2) = 5
            End If
            StraightLineModel = -1
            DesigningP209DrawStructure = True
            Call LeafDesignFlex()

        ElseIf case04 Or case06 Or case09 Then
            '5 layer structures
            DesigningP209DrawStructure = False
            Call SetData_DesignBase_SubgadeCBR20_5Layers()
            IterLayerChosen = 3
            ILayer = 3
            'This checking to see if miniumum 209 thickness is set.
            Call LeafDesignFlex()
            DesigningP209DrawStructure = True

            Call DesignRestoreStructure_5Layers()
            IterLayerChosen = 4
            ILayer = 4
            'This makes sure intitial structure is correct
            Call LeafDesignFlex()

        Else
            AutomaticDesign = False
            Exit Function
        End If

        AutomaticDesign = True
    End Function

    Sub btnPCR_Click(ByRef ct As CancellationToken) 'Handles btnPCR.Click

        Try           'PtoTC,  EvalThick, RepsAnnual(1), GL(1), NAC
            Dim WDir1 As String, myDoc1 As String, NAC_noGear As Integer
            myDoc1 = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            WDir1 = myDoc1 & "\FAARFIELD\PCN_Results\"
            gCDF_target_copy = 0 : gOverlayLife_target_copy = 0  'Me.Enabled = False

            If Not Directory.Exists(WDir1) Then System.IO.Directory.CreateDirectory(WDir1)
            If JobName = "Samples" Then Exit Sub
            ReDim NAC_order(NAC), Save_NAC_order(NAC), Save2_NAC_order(NAC), cdf_max1M(NAC)

            gDontPrint_PCN = False ': RepsAnnual(1) = 25000000000
            TFN = WDir1 & "Results_" & JobName & "_" & SectName & "_"
            TFN = TFN & getTodaysDateFormatted() & ".txt"

            Call Redim0NewACN(1) 'ReDim gNewACName(size1)
            gGraphSection = SectName : gGraphJob = JobName

            Dim TimeSave1, TimeSave2 As Integer, ETimemsecs As Single
            'Dim Path33 As String = Environment.CurrentDirectory
            ChDir(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD")

            Call SetDesignType()
            Call Determine_PavementType_Thick()
            Call SetPCN_for_AC() ' "A380 Belly"      AC(i).libMGpcntPCN = 57.08 / 100 / 2

            '+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            TimeSave1 = timeGetTime
            'btnF5010.Enabled = False : btnGraphPCN.Enabled = False : btnPCR.Enabled = False
            FEDFAA1.FF_true_ACN = True : gConstTirePressure = True : PCRRun = True : LifeCounterForPCRRuns = 0 : FlexonRigidChecker = False : FlexonRigidCounter = 0 'TRUE for PCN calculations

            Dim gNAC As Integer = UBound(AC, 1)
            For i As Integer = 1 To gNAC
                If AC(i).libMGpcntPCN = 0 Then
                    AC(i).libMGpcntPCN = AC(i).libMGpcnt
                    'ElseIf AC(i).libACName.Contains("MD-11") And AC(i).libACName.Contains("Belly") Then
                    '    AC(i).libMGpcnt = AC(i).libMGpcntPCN
                    'ElseIf AC(i).libACName.Contains("30/30F/40") And AC(i).libACName.Contains("Belly") Then
                    '    AC(i).libMGpcnt = AC(i).libMGpcntPCN
                    'ElseIf AC(i).libACName.Contains("IL-86") And AC(i).libACName.Contains("Belly") Then
                    '    AC(i).libMGpcnt = AC(i).libMGpcntPCN
                    'ElseIf AC(i).libACName.Contains("KC-10") And AC(i).libACName.Contains("Belly") Then
                    '    AC(i).libMGpcnt = AC(i).libMGpcntPCN
                Else
                    AC(i).libMGpcnt = AC(i).libMGpcntPCN * 2
                End If
            Next

            For i As Integer = 1 To NAC
                NAC_order(i) = i
            Next

            Call RedimSaveM(NAC)
            Dim IC As Integer, max1 As Single
            Call UpdateCurrentSectData()
            Call Save_Aircraft_data()
            Call SaveM_Aircraft_data(1)
            'GoTo start_of_table3 'to comment 22222444444444
            '++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

            'Table 3 Calculations *******************************
            ReDim gACN_GL(NAC) : ReDim gACN_GL_thick(NAC)

            'Dim gACRmaxIndex As Integer
            Dim gACRmaxVal As Single

            'GoTo skipACR 'xxxxx to comment
start_of_table3:
            ' Call PrintResultsPCN04_Table3_Header(TFN)
            'Call Restore_Aircraft_data()
            '********************************************************   <<<<<<<<<<<
            'Table 3. Flexible ACN at Indicated Gross Weight and Strength
            NAC_noGear = 0
            For ik3 As Integer = 1 To NAC
                ' btnGraphPCN.Text = "T3 " & CStr(ik3)
                Dim RunACNLib3 As New ACRClassLib.clsACR
                Dim ACNdataFF3 As ACRClassLib.clsACR.ACRdata

                If Check_H_Aircraft_Function(ik3) Then
                    Continue For
                End If
                If ACROption = True Then

                    For I = 1 To 4

                        Modulus(NPLayers) = SubgradeCategoryCheck(I)
                        Call MakeACNcall(ik3, 99)
                        ACRThickDataFed(ik3, I) = ACNdataFF1.libACR(1)
                        ACRThickSubgradeData(ik3, I) = ACNdataFF1.libACRthick(1)
                        ACNdataFF1 = Nothing
                    Next
                End If

                'SubgradeCategory = "B"
                Modulus(NPLayers) = LastLayerModulus
                Call MakeACNcall(ik3, 99)
                NAC_noGear = NAC_noGear + 1

                If (gACRmaxVal < gACN_GL(ik3)) Then 'And RepsAnnual(ik3) >= 10 Then
                    gACRmaxVal = gACN_GL(ik3)
                    gACRmaxIndex = ik3
                End If

            Next
            'End for Table 333333333333333333333333333333333333


skipACR:
            'gACRmaxIndex = 23 : NAC_noGear = NAC
            ' 'to comment 'mod900
            'gACRmaxIndex = 2 : NAC_noGear = NAC 'xxxxx to comment
            'gACRmaxIndex = 0 '9999999999
            ' Call ButtonsEnabled(False)

            For i As Integer = 1 To NAC
                RepsAnnual(i) = CSng(RepsAnnual(i) * PtoTC)  'ssssssssssss
            Next




            ReDim gNtoFail_copy(NAC) : ReDim gNtoFail(NAC)
            ReDim gStrain2(NAC) : ReDim gStrain2C(NAC)
            ' >>>>> STEP 1 run life for the evaluation section and traffic mix <<<<<
            ' btnGraphPCN.Text = "Step 1"
            'Call cmdLife_Click(sender, e) 'Calculating Life 1 (NAC)
            ReDim gLEAFstressGreater(NAC)
            ReDim gLEAFstressGreaterOverlay(NAC)

            gPCR_Life = True
            Call PCNLifeCalc()
            gPCR_Life = False


            Call SaveACdata1()
            'Call After_cmdLife_Click_2(Me)

            'make copy of coverages
            For i As Integer = 1 To NAC
                gNtoFail_copy(i) = gNtoFail(i)
                gStrain2C(i) = gStrain2(i)
                '  ggCoverage_copy(i) = gCoverage_NtoFail(i)
            Next
            '=================================================

            'Modulus(NPLayers) = 9200
            'Call After_cmdLife_Click(Me) '9999999999
            OldCDFPic = CDFPic

            'GoTo Table1
            'Determine max offset place
            Dim offset1 As Integer, max9 As Double
            offset1 = 0 : max9 = 0

            For ik As Integer = 1 To NOFF
                If max9 < CDFdata2(1, NAC + 1, ik) Then
                    max9 = CDFdata2(1, NAC + 1, ik)
                    offset1 = ik
                End If
            Next

            'calculate updated CDFPic
            Dim sum1 As Double
            NewCDFPic = 0

            For ik As Integer = 1 To NAC
                If Check_H_Aircraft_Function(ik) Then
                    Continue For
                Else
                    If H_index > 0 Then
                        sum1 = CDFdata2(1, H_index, offset1) + CDFdata2(1, H_index + 1, offset1)

                        If sum1 > 0.0001 Then
                            NewCDFPic = NewCDFPic + sum1
                        End If
                    Else
                        If CDFdata2(1, ik, offset1) > 0.0001 Then
                            Dim k As Double
                            NewCDFPic = NewCDFPic + CDFdata2(1, ik, offset1)
                            k = NewCDFPic
                        End If
                    End If
                End If
            Next

            NewCDFPic = max9   'AliA

            gCaseCDFlow = False
            If NewCDFPic = 0 Then
                'For ik As Integer = 1 To NAC

                '    If jobCDFacrftMaxtable(ISect, ik) < 0.0001 Then
                '        Continue For
                '    End If
                '    newCDFPic = newCDFPic + jobCDFacrftMaxtable(ISect, ik)
                'Next
                NewCDFPic = CDFPic
                gCaseCDFlow = True
            End If

            CDFPic = CSng(NewCDFPic)













            '=================================================
Table1:

            'Table 1. Input Traffic Data
            'Dim ConversionACCovs(NAC) As Double
            ReDim ConversionACCovs(NAC)

            For pp1 As Short = 1 To NAC
                max1 = -1000
                For IC = 1 To 41
                    If CtoP(pp1, IC) > max1 Then
                        max1 = CtoP(pp1, IC)
                    End If
                Next IC
                gPtoC(pp1) = 1 / max1
            Next

            For I1 As Integer = 1 To NAC
                Dim TempL, TempL1 As Single
                TempL1 = Life
                If 1.0! + RepsInc(I1) * Life < 0.0! Then
                    TempL1 = -1.0! / RepsInc(I1)
                End If
                TempL = CSng(1.0! + TempL1 * RepsInc(I1) * 0.5)
                Reps(I1) = TempL * RepsAnnual(I1) * TempL1
                'ConversionACCovs(I1) = Reps(I1) * PtoTC / gPtoC(I1)

                ConversionACCovs(I1) = Reps(I1) / gPtoC(I1) 'sssssssssssssssss
            Next

            '================================== CDFMAX
            'Dim CDFPicMin As Double = 0.0001
            'If CDFPic < CDFPicMin Then
            '    GoTo CDFPicMin_Case
            'End If
            '================================== CDFMAX

            '*******************************************************
            Dim ind2maxACxMxxxx1(NAC) As Integer, ind2maxACxMSave1xxxx(NAC) As Integer
            ReDim ind2maxACxM(NAC), ind2maxACxMSave(NAC)

            gCDF_target_copy = CDFPic
            gOverlayLife_target_copy = OverlayLife
            gPCN_report = 0

            For ik As Integer = 1 To NAC_noGear
                Call RedimNewACN(ik)
                If ik > 1 Then
                    'btnGraphPCN.Text = "Les" & CStr(ik - 1)  '>>>>> Eliminating one aircraft --------
                    Call Set_NAC_lessOne_Aircraft_dataM(ind2maxACxM(ik - 1), ik - 1)
                    If NAC = 0 Then GoTo finish1
                    'Call Save2_Aircraft_data() 'Copy of aircraft data NAC-1 ect.
                    Call SaveM_Aircraft_data(ik)
                    'Call cmdLife_Click(sender, e)
                    Call PCNLifeCalc(ct)
                End If

                If ct.IsCancellationRequested Then
                    Debug.WriteLine("Task cancelled")
                    ct.ThrowIfCancellationRequested()
                End If

                Call StepsToDoM(ind2maxACxM(ik), ik, ind2maxACxMSave(ik))
                ' btnGraphPCN.Text = "PCR" & CStr(ik)

                If ik = 10 Then
                    ik = 10
                End If

                'Call PrintResults002(Me) '9999999999

                ' Capture PCR round data for detailed report
                Try
                    Dim pcrRound As New clsPCRRound()
                    pcrRound.RoundNumber = ik
                    If ind2maxACxM(ik) >= 1 AndAlso ind2maxACxM(ik) <= NAC_noGear Then
                        pcrRound.CriticalAircraftName = gNewACName(ik)
                    End If
                    pcrRound.CriticalAircraftCDF = CDFPic
                    pcrRound.FinalMGW = gMGW(ind2maxACxM(ik))
                    pcrRound.RoundPCR = gNewPCN(ik)
                    pcrRound.EarlyExit = (ind2maxACxMSave(ik) = gACRmaxIndex)
                    gDetailedReportData.PCRRounds.Add(pcrRound)
                Catch ex As Exception
                End Try

                If gPCN_report < gNewPCN(ik) Then
                    gPCN_report = gNewPCN(ik)
                    gPCN_report_index = ind2maxACxMSave(ik)
                    indexPCNtable2 = ik
                End If
                'If gPCN_report_index = gACRmaxIndex Then
                If ind2maxACxMSave(ik) = gACRmaxIndex Then
                    GoTo finish1
                End If
            Next

            GoTo finish1

CDFPicMin_Case:

            'gPCN_report_index = gACRmaxIndex



finish1:
            'Call PrintResultsPCN05Summary(TFN)

            ' btnPCR.Text = "PCR"
            Call Restore_Aircraft_data() '********************
            TimeSave2 = timeGetTime
            gETimemsecs = CSng((TimeSave2 - TimeSave1) / 1000)
            'cmdHelp.Text = LPad(4, CStr(Format(ETimemsecs / 60, "#0")))
            Call Print_PCN_results2(lPavementType, ConversionACCovs) '7777777777

            ChDir(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD")
            Call InitAcLib() 'Re-initiate Aircraft Library
            gConstTirePressure = False : FEDFAA1.FF_true_ACN = False : PCRRun = False
            ' btnPCR.Enabled = True : btnF5010.Enabled = True : btnGraphPCN.Enabled = True
            LifeComputation = False : DesigningStr = False
            ' btnGraphPCN.Text = "Graph"
            'Call ButtonsEnabled(True) : Me.Enabled = True

            'Call RestoreACdata1()
        Catch ex As Exception
            If TypeOf ex Is TaskCanceledException Then
                Debug.WriteLine("Task Cancelled: {0}",
                                 CType(ex, TaskCanceledException).Message)
            ElseIf TypeOf ex Is OperationCanceledException Then
                Debug.WriteLine("Operation Cancelled: {0}",
                                 CType(ex, OperationCanceledException).Message)
            Else
                Debug.WriteLine("Exception: " + ex.GetType().Name)

                Dim txt As String
                txt = ex.Message
                txt = txt + Environment.NewLine + Environment.NewLine
                txt = txt + ex.StackTrace
                txt = txt + Environment.NewLine + Environment.NewLine
                MsgBox(txt)
            End If

            ' Call ButtonsEnabled(True)
        End Try

    End Sub

    Friend Sub StepsToDoM(ByRef ind0maxAC As Integer, ByVal ind0 As Integer, ByRef ind0maxAC0Save As Integer)

        Dim cdf_max1 As Single, n2, n3 As Integer

        gCDF_target = gCDF_target_copy 'critical CDF for the second run
        gOverlayLife_target = gOverlayLife_target_copy

        Call ABC_Reset_CDFdata()
        ' Call PrintResultsPCN01_CDF(TFN, ind0)

        ' >>>>> STEP 2 identify the critical CDF offset and CDF at the critical offset <<<<<
        cdf_max1 = -1.0E+35 'find index for max cdf contribution
        For n3 = 1 To 41
            If cdf_max1 < CDFdata2(1, NAC + 1, n3) Then
                cdf_max1 = CDFdata2(1, NAC + 1, n3)
                ind1max = n3 'critical CDF offset
            End If
        Next

        cdf_max1 = -1.0E+35 'find aircraft with max cdf contribution
        For n2 = 1 To NAC
            If cdf_max1 < CDFdata2(1, n2, ind1max) Then
                cdf_max1 = CDFdata2(1, n2, ind1max)
                ind0maxAC = n2 'aircraft with max cdf contribution
            End If
        Next

        'added part
        Dim cdf_max_for_AC As Single 'ik2021 AnnDepart
        cdf_max_for_AC = -1.0E+35

        For ik2 = 1 To NOFF
            If cdf_max_for_AC < CDFdata2(1, ind0maxAC, ik2) Then
                cdf_max_for_AC = CDFdata2(1, ind0maxAC, ik2)
            End If
        Next
        'end of added part

        '================================== CDFMAX
        If gCDF_target_copy < 0.0001 Or gCaseCDFlow Then
            ind0maxAC = gACRmaxIndex
        End If
        '================================== CDFMAX

        'ind0maxAC = 2 ' to comment xxxxx
        '   Call PrintResultsPCN01B_cdfmax(TFN, ind1max, ACName(ind0maxAC)) 'Prints max cdf
        Call Check_H_Aircraft(ind0maxAC)

        'STEP 4 remove all aircraft from the mix except for the critical aircraft 
        Call Set_OneAircraft_dataM(ind0maxAC, ind0)
        'gStrain2(NAC_order(1)) = gStrain2_copy


        'STEP 4.1 adjust annual departures until the CDF is equal to the critical CDF in STEP 1.
        'This is the total equivalent departures of the critical aircraft
        ' btnGraphPCN.Text = "Dep" & CStr(ind0)


        If gCDF_target_copy < 0.0001 Or gCaseCDFlow Then

        Else
            If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then
                Call NewAdjustAnnDepartOverlay()
            Else
                RepsAnnual(1) = CSng(RepsAnnual(1) * gCDF_target / cdf_max_for_AC)
                RepsAnnual(2) = RepsAnnual(1)
                Call NewAdjustAnnDepart2017()
            End If

        End If

        'If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then
        '    Call NewAdjustAnnDepartOverlay()
        'Else
        '    Call NewAdjustAnnDepart2017()
        'End If



        'Call gSaveCDFdata()
        gCDF_reached = CDFPic
        gOverlayLife_reached = OverlayLife
        gNewAnnualDepart(ind0) = RepsAnnual(1)

        'STEP 5 adjust the GW until CDF = 1.  This is MGW for PCN
        gCDF_target = 1
        gOverlayLife_target = Life
        ' btnGraphPCN.Text = "GL" & CStr(ind0)
        Call AdjustGrossWeight2017() 'for GL (StepsToDo)
        ind0maxAC0Save = NAC_order(1)
        cdf_max1M(ind0maxAC0Save) = cdf_max1
        gMGW(ind0maxAC) = GL(1)
        gMGW(ind0) = GL(1)

        'If ind0maxAC = 2 Then
        '    Call PrintResults001(Me)
        'End If

        '   btnGraphPCN.Text = "PCR" & CStr(ind0)
        Call MakeACNcall(1, ind0)
        '  Call PrintResultsPCN04_CDFtarget(TFN, ind0)


    End Sub

    Public Sub Determine_PavementType_Thick()

        If DesignType = NewFlex Then
            lPavementType = ACRClassLib.clsACR.PavementType.Flexible : EvalThick = 0
            For I1 As Integer = 1 To NPLayers - 1
                EvalThick = EvalThick + Thick(I1)
            Next I1
            EvalThick = Math.Round(EvalThick, 5)

        ElseIf (DesignType = FlexOnRigid) And (gHMAonRigid_Mod = False) Then
            lPavementType = ACRClassLib.clsACR.PavementType.Rigid : EvalThick = Thick(1)

        ElseIf (DesignType = FlexOnRigid) And (Thick(2) > Thick(1)) Then
            lPavementType = ACRClassLib.clsACR.PavementType.Rigid : EvalThick = Thick(1)

        ElseIf (DesignType = FlexOnRigid) And (Thick(2) <= Thick(1)) Then
            lPavementType = ACRClassLib.clsACR.PavementType.Flexible : EvalThick = Thick(1)

        ElseIf DesignType = FlexOnFlex Or DesignType = FlexOnRigid Then
            lPavementType = ACRClassLib.clsACR.PavementType.Flexible : EvalThick = Thick(1)

        ElseIf DesignType = PCCOnFlex Or DesignType = NewRigid Then
            lPavementType = ACRClassLib.clsACR.PavementType.Rigid : EvalThick = Thick(1)
            ConcreteFlexuralStrength = RCon(1)
        ElseIf OverlayRigOnRig Then
            lPavementType = ACRClassLib.clsACR.PavementType.Rigid : EvalThick = Thick(1)
            ConcreteFlexuralStrength = RCon(1)
        End If

    End Sub
    Friend Sub StepsToDo(ByRef ind0maxAC As Integer, ByVal ind0 As Integer, ByRef ind0maxAC0Save As Integer)

        'Call StepsToDo_Strain(ind0maxAC, ind0, ind0maxAC0Save)
        'Exit Sub

        Dim cdf_max1 As Single, n2, n3 As Integer

        gCDF_target = gCDF_target_copy 'critical CDF for the second run
        gOverlayLife_target = gOverlayLife_target_copy

        Call ABC_Reset_CDFdata()
        'Call ButtonsEnabled(False)
        ' Call PrintResultsPCN01_CDF(TFN, ind0)

        ' >>>>> STEP 2 identify the critical CDF offset and CDF at the critical offset <<<<<
        cdf_max1 = -1.0E+35 'find index for max cdf contribution
        For n3 = 1 To 41
            If cdf_max1 < CDFdata2(1, NAC + 1, n3) Then
                cdf_max1 = CDFdata2(1, NAC + 1, n3)
                ind1max = n3 'critical CDF offset
            End If
        Next

        gCriticalOffset = ind1max
        'Call PrintResults_CDF("CDF_Results01.txt")
        'Call Set_targets()
        'Call MakeACNcall(1, 1) 'temporary 'to comment

        cdf_max1 = -1.0E+35 'find aircraft with max cdf contribution
        For n2 = 1 To NAC
            If cdf_max1 < CDFdata2(1, n2, ind1max) Then
                cdf_max1 = CDFdata2(1, n2, ind1max)
                ind0maxAC = n2 'aircraft with max cdf contribution
            End If
        Next

        'ind0maxAC = 8 'comment123

        'If ind0 = 1 Then
        '    ind0maxAC = 8
        'End If

        '  Call PrintResultsPCN01B_cdfmax(TFN, ind1max, ACName(ind0maxAC)) 'Prints max cdf


        Call Check_H_Aircraft(ind0maxAC)
        'If H_index > 0 Then ind0maxAC = H_index '????

        'STEP 4 remove all aircraft from the mix except for the critical aircraft 
        If ind0 = 1 Then
            Call Set_OneAircraft_data(ind0maxAC)  'Set data for 1 aircraft 
        ElseIf ind0 = 2 Then
            Call Set_OneAircraft_data2(ind0maxAC)  'Set data for 1 aircraft 
        ElseIf ind0 = 3 Then
            Call Set_OneAircraft_data3(ind0maxAC)  'Set data for 1 aircraft 
        End If

        'STEP 4.1 adjust annual departures until the CDF is equal to the critical CDF in STEP 1.
        'This is the total equivalent departures of the critical aircraft
        '  btnGraphPCN.Text = "Dep" & CStr(ind0)

        If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then
            Call NewAdjustAnnDepartOverlay()
        Else
            Call NewAdjustAnnDepart2017()
        End If

        'Call gSaveCDFdata()
        gCDF_reached = CDFPic
        gOverlayLife_reached = OverlayLife
        gNewAnnualDepart(ind0) = RepsAnnual(1)

        'STEP 5 adjust the GW until CDF = 1.  This is MGW for PCN
        gCDF_target = 1
        gOverlayLife_target = Life
        '   btnGraphPCN.Text = "GL" & CStr(ind0)
        Call AdjustGrossWeight2017() 'for GL (StepsToDo)
        ind0maxAC0Save = NAC_order(1)
        gMGW(ind0maxAC) = GL(1)
        gMGW(ind0) = GL(1)

        '  btnGraphPCN.Text = "PCR" & CStr(ind0)
        Call MakeACNcall(1, ind0)
        ' Call PrintResultsPCN04_CDFtarget(TFN, ind0)
        'gPCN_report = gNewPCN(ind0)
        'gPCN_report_index = ind0maxAC


    End Sub
    Public Sub MakeACNcall(ByVal ind1 As Integer, ByVal run1 As Integer)

        Dim msg1 As String, bA340 As Boolean
        Dim NewX(), NewY() As Single, pcnt1 As Single, num1 As Integer
        Dim NewX2(), NewY2() As Single, pcnt2 As Single, num2 As Integer
        Dim sb1 As Single = Modulus(NPLayers)

        Dim RunACNLib1 As New ACRClassLib.clsACR


        Dim ACNdataFF2 As ACRClassLib.clsACR.ACRdata

        Dim RunACNLib_Rigid1 As New ACRClassLib.clsACR
        Dim ACNdataFF_Rigid1 As ACRClassLib.clsACR.ACRdata

        Dim RunACNLib_Rigid2 As New ACRClassLib.clsACR
        Dim ACNdataFF_Rigid2 As ACRClassLib.clsACR.ACRdata

        Dim bFlex, bRig, bGearH, bEqualPress As Boolean
        Dim bGearX, bGroups1, bGroups2, bX13, bX14 As Boolean

        Dim isBelly As Boolean = False

        'lPavementType = "Rigid" 'to comment
        bFlex = (lPavementType = ACRClassLib.clsACR.PavementType.Flexible)
        bRig = (lPavementType = ACRClassLib.clsACR.PavementType.Rigid)

        bGearX = (AC(LibIndex(ind1)).libGear = "X")
        bGroups1 = (AC(LibIndex(ind1)).libNGroups = 1)
        bGroups2 = (AC(LibIndex(ind1)).libNGroups = 2)
        bX13 = (AC(LibIndex(ind1)).libIGear = 13)
        bX14 = (AC(LibIndex(ind1)).libIGear = 14)

        If LibIndex(ind1) < AC.Count - 1 Then
            If bRig Then
                If AC(LibIndex(ind1 + 1)).libACName.Contains("Belly") Then
                    bGearH = True
                End If
            End If
            If bFlex And Not bGearX Then
                If AC(LibIndex(ind1 + 1)).libACName.Contains("Belly") Then
                    bGearH = True
                End If
            End If
            If AC(LibIndex(ind1 + 1)).libACName.Contains("Belly") Then
                isBelly = True
            End If
        End If


        Dim OneGear = False
        If AC(LibIndex(ind1)).libACName.Contains("SWL") Or AC(LibIndex(ind1)).libTX.Count = 2 Then
            OneGear = True
        End If

        Dim UDACraft = False
        If AC(LibIndex(ind1)).libACName.Contains("UDA") Then
            UDACraft = True
        End If

        bEqualPress = False
        If bGearH Or isBelly Then
            bEqualPress = ((AC(LibIndex(ind1)).libCP = AC(LibIndex(ind1 + 1)).libCP))
        End If



        '========= Flexible =========
        If (bFlex And isBelly And Not bGearH And bEqualPress) Then 'Equal Tire Pressure - gear combined

            pcnt1 = (MGpcnt(ind1) + MGpcnt(ind1 + 1)) * 2
            Dim wheelCX((AC(LibIndex(ind1)).libTX.Count - 1 + AC(LibIndex(ind1 + 1)).libTX.Count - 1)) As Single
            Dim wheelCY((AC(LibIndex(ind1)).libTY.Count - 1 + AC(LibIndex(ind1 + 1)).libTY.Count - 1)) As Single

            Dim j = 0
            For i = 1 To (AC(LibIndex(ind1)).libTX.Count - 1) / 2
                wheelCX(i) = AC(LibIndex(ind1)).libTX(i)
                wheelCY(i) = AC(LibIndex(ind1)).libTY(i)
                j = i + 1
            Next
            For i = 1 To (AC(LibIndex(ind1 + 1)).libTX.Count - 1) / 2
                wheelCX(j) = AC(LibIndex(ind1 + 1)).libTX(i)
                wheelCY(j) = AC(LibIndex(ind1 + 1)).libTY(i)
                j += 1
            Next

            For i = AC(LibIndex(ind1 + 1)).libTX.Count - 1 To ((AC(LibIndex(ind1 + 1)).libTX.Count - 1) / 2) + 1 Step -1
                wheelCX(j) = AC(LibIndex(ind1 + 1)).libTX(i)
                wheelCY(j) = AC(LibIndex(ind1 + 1)).libTY(i)
                j += 1
            Next

            For i = AC(LibIndex(ind1)).libTX.Count - 1 To ((AC(LibIndex(ind1)).libTX.Count - 1) / 2) + 1 Step -1
                wheelCX(j) = AC(LibIndex(ind1)).libTX(i)
                wheelCY(j) = AC(LibIndex(ind1)).libTY(i)
                j += 1
            Next

            num1 = UBound(wheelCX, 1)

            Dim SW(num1) As Integer
            For i = 1 To num1
                If (wheelCX(i)) >= 0 Then
                    SW(i) = 1
                Else
                    SW(i) = 0
                End If
            Next

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
               pcnt1, num1, AC(LibIndex(ind1)).libCP,
               wheelCX,
               wheelCY,
               Modulus(NPLayers), SW)

            'ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
            '   pcnt1, num1, AC(LibIndex(ind1)).libCP,
            '   wheelCX,
            '   wheelCY,
            '   Modulus(NPLayers))

            msg1 = "Flex; Equal Pressure; main and belly combined"

        ElseIf (bFlex And isBelly And Not bGearH And Not bEqualPress) Then 'And False

            'Global coordinates
            pcnt1 = MGpcnt(ind1) * 2 '2017.10.12 modification 101

            If AC(LibIndex(ind1 + 1)).libIGear = 3 Or AC(LibIndex(ind1 + 1)).libIGear = 6 Then
                pcnt2 = MGpcnt(ind1 + 1) * 2
            Else
                pcnt2 = MGpcnt(ind1 + 1)
            End If

            num1 = UBound(AC(LibIndex(ind1)).libTX, 1)
            num2 = UBound(AC(LibIndex(ind1 + 1)).libTX, 1)

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
               pcnt1, num1, AC(LibIndex(ind1)).libCP,
                AC(LibIndex(ind1)).libTX, AC(LibIndex(ind1)).libTY,
               pcnt2, num2, AC(LibIndex(ind1 + 1)).libCP,
               AC(LibIndex(ind1 + 1)).libTX, AC(LibIndex(ind1 + 1)).libTY,
               Modulus(NPLayers))

            msg1 = "Flex; Not Equal Tire Pressure; main and belly separate"

        ElseIf (bFlex And bGearH And bEqualPress) Then 'Equal Tire Pressure

            'Global coordinates
            Call GetCoordinates_AllWheels(NewX, NewY, pcnt1, num1, ind1)
            gPcnt1(1) = pcnt1

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
                    pcnt1, num1, AC(LibIndex(ind1)).libCP, NewX, NewY,
                    Modulus(NPLayers))
            msg1 = "Flex; H gear; Equal Tire Pressure"
            '  Call PrintResultsPCN_Cond11(msg1, TFN, GL(ind1),
            'pcnt1, num1, AC(LibIndex(ind1)).libCP, NewX, NewY, sb1, ind1, run1)

        ElseIf (bFlex And bGearH And (Not bEqualPress)) Then 'Not Equal Tire Pressure

            'Global coordinates
            Call GetCoordinates_Body(NewX, NewY, ind1)
            pcnt1 = MGpcnt(ind1) * 2 '2017.10.12 modification 101
            pcnt2 = MGpcnt(ind1 + 1)
            num1 = UBound(NewX, 1)
            num2 = UBound(AC(LibIndex(ind1 + 1)).libTX, 1)

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
               pcnt1, num1, AC(LibIndex(ind1)).libCP,
               NewX, NewY,
               pcnt2, num2, AC(LibIndex(ind1 + 1)).libCP,
               AC(LibIndex(ind1 + 1)).libTX, AC(LibIndex(ind1 + 1)).libTY,
               Modulus(NPLayers))

            msg1 = "Flex; H gear; Not Equal Tire Pressure"
            '  Call PrintResultsPCN_Cond12(msg1, TFN, GL(ind1),
            ' pcnt1, num1, AC(LibIndex(ind1)).libCP,
            'NewX, NewY,
            'pcnt2, num2, AC(LibIndex(ind1 + 1)).libCP,
            'AC(LibIndex(ind1 + 1)).libTX, AC(LibIndex(ind1 + 1)).libTY,
            'sb1, ind1)

        ElseIf (bFlex And bGearX And bGroups1) Then 'Flex + X + 1gear

            If OneGear Then ' Or UDACraft
                pcnt1 = MGpcnt(ind1)
            Else
                pcnt1 = MGpcnt(ind1) * 2
            End If
            num1 = UBound(AC(LibIndex(ind1)).libTX, 1)

            Dim SW(num1) As Integer
            For i = 1 To (AC(LibIndex(ind1)).libTX).Count - 1
                If (AC(LibIndex(ind1)).libTX(i)) >= 0 Then
                    SW(i) = 1
                Else
                    SW(i) = 0
                End If
            Next

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
               pcnt1, num1, AC(LibIndex(ind1)).libCP,
               AC(LibIndex(ind1)).libTX,
               AC(LibIndex(ind1)).libTY,
               Modulus(NPLayers), SW)

            'ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
            '   pcnt1, num1, AC(LibIndex(ind1)).libCP,
            '   AC(LibIndex(ind1)).libTX,
            '   AC(LibIndex(ind1)).libTY,
            '   Modulus(NPLayers))

            msg1 = "Flex; X gear; 1 Group"
            '  Call PrintResultsPCN_Cond11(msg1, TFN, GL(ind1),
            ' pcnt1, num1, AC(LibIndex(ind1)).libCP,
            'AC(LibIndex(ind1)).libTX,
            'AC(LibIndex(ind1)).libTY, sb1, ind1, run1)

        ElseIf (bFlex And bGearX And bGroups2) Then 'Flex + X + 2gears
            'Global coordinates
            'Dim NumOfGears As Short
            'NumOfGears = AC(LibIndex(ind1)).libNGroups
            'Call GetCoordinatesForX(NewX, NewY, pcnt1, num1, ind1)
            pcnt1 = MGpcnt(ind1)
            num1 = UBound(AC(LibIndex(ind1)).libTX, 1)

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
               pcnt1, num1, AC(LibIndex(ind1)).libCP,
               AC(LibIndex(ind1)).libTX,
               AC(LibIndex(ind1)).libTY,
               Modulus(NPLayers))

            msg1 = "Flex; X gear; 2 Groups"
            ' Call PrintResultsPCN_Cond11(msg1, TFN, GL(ind1),
            ' pcnt1, num1, AC(LibIndex(ind1)).libCP,
            'AC(LibIndex(ind1)).libTX,
            'AC(LibIndex(ind1)).libTY, sb1, ind1, run1)


        ElseIf bFlex And (AC(LibIndex(ind1)).libACName = "C-5") Then

            Call GetCoordinates_C5Type(NewX, NewY, pcnt1, num1, ind1)
            gPcnt1(1) = pcnt1

            Dim SW(num1) As Integer
            For i1 As Integer = 1 To AC(LibIndex(ind1)).libNTires
                SW(i1) = 1
            Next

            '       Call PrintResultsPCN_Cond11(msg1, TFN, GL(ind1), _
            'pcnt1, num1, AC(LibIndex(ind1)).libCP, NewX, NewY, sb1, ind1, run1)


            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
                    pcnt1, num1, AC(LibIndex(ind1)).libCP, NewX, NewY,
                    Modulus(NPLayers), SW)
            msg1 = "Flex; C-5 gear; Flexible Pavement"
            ' Call PrintResultsPCN_Cond11(msg1, TFN, GL(ind1),
            'pcnt1, num1, AC(LibIndex(ind1)).libCP, NewX, NewY, sb1, ind1, run1)

            'ElseIf bFlex And (AC(ind1).libGear = "A") Then
        ElseIf bFlex And (AC(LibIndex(ind1)).libGear = "A") Then

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
                    AC(LibIndex(ind1)).libMGpcntPCN,
                    AC(LibIndex(ind1)).libNTires,
                    AC(LibIndex(ind1)).libCP,
                    AC(LibIndex(ind1)).libTX, AC(LibIndex(ind1)).libTY,
                    Modulus(NPLayers))

        ElseIf bFlex Then
            pcnt1 = MGpcnt(ind1) * 2
            num1 = AC(LibIndex(ind1)).libNTires * 2
            ReDim NewX(num1)
            ReDim NewY(num1)

            Dim SW(num1) As Integer
            For i1 As Integer = 1 To AC(LibIndex(ind1)).libNTires
                SW(i1) = 1
            Next

            Dim RightMostX As Single, xShift As Single
            Dim n11 As Integer = AC(LibIndex(ind1)).libNTires
            RightMostX = -1.0E+35
            For J = 1 To AC(LibIndex(ind1)).libNTires
                If AC(LibIndex(ind1)).libTX(J) > RightMostX Then RightMostX = AC(LibIndex(ind1)).libTX(J)
            Next J

            If (AC(LibIndex(ind1)).libGear = "B") Then
                xShift = RightMostX + AC(LibIndex(ind1)).libTT / 2
            Else
                xShift = RightMostX + AC(LibIndex(ind1)).libTS / 2
            End If

            For i As Integer = 1 To n11 'Body
                NewX(i) = (-xShift + AC(LibIndex(ind1)).libTX(i))
                NewX(n11 + i) = -NewX(i)
                NewY(i) = AC(LibIndex(ind1)).libTY(i)
                NewY(n11 + i) = NewY(i)
            Next

            ' SubgradeCategory = "A"
            'ACNdataFF1.libSubCat(1) = "B"
            'currentjob.sections.item(0).layers.item(3).

            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
                        pcnt1, num1, AC(LibIndex(ind1)).libCP,
                        NewX, NewY, Modulus(NPLayers), SW)
            'ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
            '            pcnt1, num1, AC(LibIndex(ind1)).libCP,
            '            NewX, NewY, Modulus(NPLayers), SW)
            'ACNdataFF2 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
            '            pcnt1, num1, AC(LibIndex(ind1)).libCP,
            '            NewX, NewY, Modulus(NPLayers), SW)

            Dim a As Single = 2


            msg1 = "Flex; 2 gears"
            ' PrintResultsPCN_Cond11(msg1, TFN, GL(ind1), pcnt1, num1, AC(LibIndex(ind1)).libCP,
            '  NewX, NewY, sb1, ind1, run1)

            '========= RIGID =========        Local coordinates

        ElseIf (bRig And isBelly) Then 'And Not bGearH

            Dim cx1() As Single = Array.FindAll(AC(LibIndex(ind1)).libTX, Function(x) x <= 0)
            Dim wheelCX1(cx1.Count - 1) As Single
            Dim wheelCY1(cx1.Count - 1) As Single

            Dim j = 1
            For i = 1 To (AC(LibIndex(ind1)).libTX.Count - 1)
                If AC(LibIndex(ind1)).libTX(i) <= 0 Then
                    wheelCX1(j) = AC(LibIndex(ind1)).libTX(i)
                    wheelCY1(j) = AC(LibIndex(ind1)).libTY(i)
                    j += 1
                End If
            Next

            Dim wheelCount As Single = 0
            Dim cx2() As Single = Array.FindAll(AC(LibIndex(ind1 + 1)).libTX, Function(x) x <= 0)

            If AC(LibIndex(ind1 + 1)).libIGear = 3 Or AC(LibIndex(ind1 + 1)).libIGear = 6 Then
                'Only Negative
                wheelCount = cx2.Count - 1
            Else
                'All Wheels
                wheelCount = AC(LibIndex(ind1 + 1)).libTX.Count - 1
            End If

            Dim wheelCX2(wheelCount) As Single
            Dim wheelCY2(wheelCount) As Single

            If AC(LibIndex(ind1 + 1)).libIGear = 3 Or AC(LibIndex(ind1 + 1)).libIGear = 6 Then
                'Only Negative
                j = 1
                For i = 1 To (AC(LibIndex(ind1 + 1)).libTX.Count - 1)
                    If AC(LibIndex(ind1 + 1)).libTX(i) <= 0 Then
                        wheelCX2(j) = AC(LibIndex(ind1 + 1)).libTX(i)
                        wheelCY2(j) = AC(LibIndex(ind1 + 1)).libTY(i)
                        j += 1
                    End If
                Next
            Else
                'All Wheels
                For i = 1 To (AC(LibIndex(ind1 + 1)).libTX.Count - 1)
                    wheelCX2(i) = AC(LibIndex(ind1 + 1)).libTX(i)
                    wheelCY2(i) = AC(LibIndex(ind1 + 1)).libTY(i)
                Next
            End If

            'bA340 = CheckForA340(CShort(ind1))

            'If bA340 Then
            '    MGpcnt(ind1) /= 2
            'End If

            pcnt1 = MGpcnt(ind1)

            ACNdataFF_Rigid1 = RunACNLib_Rigid1.CalculateACR(
               lPavementType, GL(ind1),
               pcnt1,
               AC(LibIndex(ind1)).libNTires,
               AC(LibIndex(ind1)).libCP,
               wheelCX1,
               wheelCY1,
               Modulus(NPLayers))

            'bA340 = CheckForA340(CShort(ind1 + 1))

            'If bA340 Then
            '    MGpcnt(ind1 + 1) /= 2
            'End If

            pcnt2 = MGpcnt(ind1 + 1)

            ACNdataFF_Rigid2 = RunACNLib_Rigid2.CalculateACR(
                lPavementType, GL(ind1 + 1),
                pcnt2,
                AC(LibIndex(ind1 + 1)).libNTires,
                AC(LibIndex(ind1 + 1)).libCP,
                wheelCX2,
                wheelCY2,
                Modulus(NPLayers))

            ReDim ACNdataFF1.libACR(1) : ReDim ACNdataFF1.libACRthick(1)

            Debug.Print("ACNdataFF_Rigid1.libACR(1)= " & ACNdataFF_Rigid1.libACR(1))
            Debug.Print("ACNdataFF_Rigid2.libACR(1)= " & ACNdataFF_Rigid2.libACR(1))
            Debug.Print("Modulus(NPLayers)= " & Modulus(NPLayers))

            If ACNdataFF_Rigid1.libACR(1) >= ACNdataFF_Rigid2.libACR(1) Then
                ACNdataFF1.libACR(1) = ACNdataFF_Rigid1.libACR(1)
                ACNdataFF1.libACRthick(1) = ACNdataFF_Rigid1.libACRthick(1)
            Else
                ACNdataFF1.libACR(1) = ACNdataFF_Rigid2.libACR(1)
                ACNdataFF1.libACRthick(1) = ACNdataFF_Rigid2.libACRthick(1)
            End If
            Debug.Print("ACNdataFF1.libACR(1) = " & ACNdataFF1.libACR(1))

            msg1 = "Rigid; H gear"

        ElseIf (bRig And bGearH) Then 'Rigid

            bA340 = CheckForA340(CShort(ind1))
            pcnt1 = MGpcnt(ind1)
            ACNdataFF_Rigid1 = RunACNLib_Rigid1.CalculateACR(
               lPavementType, GL(ind1),
               pcnt1,
               AC(LibIndex(ind1)).libNTires,
               AC(LibIndex(ind1)).libCP,
               AC(LibIndex(ind1)).libTX,
               AC(LibIndex(ind1)).libTY,
               Modulus(NPLayers))

            bA340 = CheckForA340(CShort(ind1 + 1))
            pcnt2 = MGpcnt(ind1 + 1)
            ACNdataFF_Rigid2 = RunACNLib_Rigid2.CalculateACR(
                lPavementType, GL(ind1 + 1),
                pcnt2,
                AC(LibIndex(ind1 + 1)).libNTires,
                AC(LibIndex(ind1 + 1)).libCP,
                AC(LibIndex(ind1 + 1)).libTX,
                AC(LibIndex(ind1 + 1)).libTY,
                Modulus(NPLayers))

            ReDim ACNdataFF1.libACR(1) : ReDim ACNdataFF1.libACRthick(1)

            Debug.Print("ACNdataFF_Rigid1.libACR(1)= " & ACNdataFF_Rigid1.libACR(1))
            Debug.Print("ACNdataFF_Rigid2.libACR(1)= " & ACNdataFF_Rigid2.libACR(1))
            Debug.Print("Modulus(NPLayers)= " & Modulus(NPLayers))

            If ACNdataFF_Rigid1.libACR(1) >= ACNdataFF_Rigid2.libACR(1) Then
                ACNdataFF1.libACR(1) = ACNdataFF_Rigid1.libACR(1)
                ACNdataFF1.libACRthick(1) = ACNdataFF_Rigid1.libACRthick(1)
            Else
                ACNdataFF1.libACR(1) = ACNdataFF_Rigid2.libACR(1)
                ACNdataFF1.libACRthick(1) = ACNdataFF_Rigid2.libACRthick(1)
            End If
            Debug.Print("ACNdataFF1.libACR(1) = " & ACNdataFF1.libACR(1))


            msg1 = "Rigid; H gear"
            'Call PrintResultsMakeACNcall_01(msg1, TFN, ACNdataFF1, ACNdataFF_Rigid1, ACNdataFF_Rigid2)
            '  Call PrintResultsPCN_Cond12(msg1, TFN, GL(ind1),
            '  pcnt1, AC(LibIndex(ind1)).libNTires, AC(LibIndex(ind1)).libCP,
            ' AC(LibIndex(ind1)).libTX, AC(LibIndex(ind1)).libTY,
            ' pcnt2, AC(LibIndex(ind1 + 1)).libNTires, AC(LibIndex(ind1 + 1)).libCP,
            ' AC(LibIndex(ind1 + 1)).libTX, AC(LibIndex(ind1 + 1)).libTY, sb1, ind1)

        ElseIf (bRig And bGearX And bGroups1) Then
            ''If AC(LibIndex(ind1)).libACName.Contains("(UDA)") Then
            ''    AC(LibIndex(ind1)).libMGpcntPCN = AC(LibIndex(ind1)).libMGpcntPCN / 2
            ''End If
            'ACNdataFF1 = RunACNLib_Rigid1.CalculateACR(
            '   lPavementType, GL(ind1),
            '   AC(LibIndex(ind1)).libMGpcntPCN,
            '   AC(LibIndex(ind1)).libNTires,
            '   AC(LibIndex(ind1)).libCP,
            '   AC(LibIndex(ind1)).libTX,
            '   AC(LibIndex(ind1)).libTY,
            '   Modulus(NPLayers))


            Dim cx() As Single = Array.FindAll(AC(LibIndex(ind1)).libTX, Function(x) x <= 0)
            Dim wheelCX(cx.Count - 1) As Single
            Dim wheelCY(cx.Count - 1) As Single

            Dim j = 1
            For i = 1 To (AC(LibIndex(ind1)).libTX.Count - 1)
                If AC(LibIndex(ind1)).libTX(i) <= 0 Then
                    wheelCX(j) = AC(LibIndex(ind1)).libTX(i)
                    wheelCY(j) = AC(LibIndex(ind1)).libTY(i)
                    j += 1
                End If
            Next

            ACNdataFF1 = RunACNLib_Rigid1.CalculateACR(
               lPavementType, GL(ind1),
               AC(LibIndex(ind1)).libMGpcntPCN,
               AC(LibIndex(ind1)).libNTires,
               AC(LibIndex(ind1)).libCP,
               wheelCX,
               wheelCY,
               Modulus(NPLayers))

            msg1 = "Rigid; X gear; 1 Group"
            ' Call PrintResultsMakeACNcall_01(msg1, TFN, ACNdataFF1, ACNdataFF_Rigid1, ACNdataFF_Rigid2)

        ElseIf (bRig And bGearX And bGroups2) Then 'B747 A380 & A340-xxx

            Call GetCoordinatesForX_Gear1(NewX, NewY, pcnt1, num1, ind1)

            ACNdataFF_Rigid1 = RunACNLib_Rigid1.CalculateACR(
               lPavementType, GL(ind1), pcnt1, num1,
               AC(LibIndex(ind1)).libCP, NewX, NewY, Modulus(NPLayers))

            Call GetCoordinatesForX_Gear2(NewX2, NewY2, pcnt2, num2, ind1)

            ACNdataFF_Rigid2 = RunACNLib_Rigid2.CalculateACR(
                lPavementType, GL(ind1), pcnt2, num2,
                AC(LibIndex(ind1)).libCP, NewX2, NewY2, Modulus(NPLayers))

            ReDim ACNdataFF1.libACR(1) : ReDim ACNdataFF1.libACRthick(1)

            If ACNdataFF_Rigid1.libACR(1) >= ACNdataFF_Rigid2.libACR(1) Then
                ACNdataFF1.libACR(1) = ACNdataFF_Rigid1.libACR(1)
                ACNdataFF1.libACRthick(1) = ACNdataFF_Rigid1.libACRthick(1)
            Else
                ACNdataFF1.libACR(1) = ACNdataFF_Rigid2.libACR(1)
                ACNdataFF1.libACRthick(1) = ACNdataFF_Rigid2.libACRthick(1)
            End If

            msg1 = "Rigid; X gear; 2 Groups"
            'Call PrintResultsMakeACNcall_01(msg1, TFN, ACNdataFF1, ACNdataFF_Rigid1, ACNdataFF_Rigid2)
            ' Call PrintResultsPCN_Cond12(msg1, TFN, GL(ind1),
            'pcnt1, num1, AC(LibIndex(ind1)).libCP, NewX, NewY,
            'pcnt2, num2, AC(LibIndex(ind1)).libCP, NewX2, NewY2, sb1, ind1)


        Else
            'Flexible and rigid local coordinates
            ACNdataFF1 = RunACNLib1.CalculateACR(lPavementType, GL(ind1),
               AC(LibIndex(ind1)).libMGpcntPCN,
               AC(LibIndex(ind1)).libNTires,
               AC(LibIndex(ind1)).libCP,
               AC(LibIndex(ind1)).libTX, AC(LibIndex(ind1)).libTY,
               Modulus(NPLayers))

            'new123
            'msg1 = ">>>> One Gear <<<<"
            'Call PrintResultsPCN_Cond11(msg1, TFN, GL(ind1), _
            '        AC(LibIndex(ind1)).libMGpcntPCN, _
            '         AC(LibIndex(ind1)).libNTires, _
            '        AC(LibIndex(ind1)).libCP, _
            '        AC(LibIndex(ind1)).libTX, AC(LibIndex(ind1)).libTY, _
            '        sb1, ind1, run1)

        End If
        'Dim check As Double
        'check = ACNdataFF2.libACR(1)

        If run1 < 99 Then 'Table 2
            gNewACName(run1) = ACName(ind1)
            gNewGL(run1) = GL(ind1)
            gNewPCNthick(run1) = ACNdataFF1.libACRthick(1)
            gNewPCN(run1) = ACNdataFF1.libACR(1)
        ElseIf run1 = 99 Then 'Table 3
            gACN_GL(ind1) = ACNdataFF1.libACR(1)
            gACN_GL_thick(ind1) = ACNdataFF1.libACRthick(1)



        End If

        ' Capture ACR details for detailed report
        Try
            Dim acrDet As New clsACRDetail()
            acrDet.ACName = ACName(ind1)
            If bFlex Then
                acrDet.SubgradeCategory = "Flexible"
            Else
                acrDet.SubgradeCategory = "Rigid"
            End If
            acrDet.DesignedBaseThickness = ACNdataFF1.libACRthick(1)
            acrDet.FinalACR = ACNdataFF1.libACR(1)
            If RunACNLib1 IsNot Nothing Then
                acrDet.FinalDSWL = RunACNLib1.DSWLFinalGearLoad
                For Each entry In RunACNLib1.DSWLLog
                    Dim dswlIter As New clsDSWLIteration()
                    dswlIter.IterationNumber = entry.IterationNumber
                    dswlIter.Load = entry.GearLoad
                    dswlIter.NtoFail = entry.NtoFail
                    dswlIter.CovACN = entry.CovACN
                    dswlIter.Delta = entry.Delta
                    acrDet.DSWLIterations.Add(dswlIter)
                Next
            End If
            gDetailedReportData.ACRDetails.Add(acrDet)
        Catch ex As Exception
        End Try

        'gACR(ind2maxAC1) = ACNdataFF1.libACR(1)
        'gACN_Thick(ind2maxAC1) = ACNdataFF1.libACRthick(1)
        RunACNLib1 = Nothing
        RunACNLib_Rigid1 = Nothing : ACNdataFF_Rigid1 = Nothing
        RunACNLib_Rigid2 = Nothing : ACNdataFF_Rigid2 = Nothing

        'gross_weight, percent_gw, wheels_number, tire_pressure
        'CoordX(), CoordY(), CBRinput

    End Sub
    Private Sub GetCoordinates_AllWheels(ByRef NewX() As Single, ByRef NewY() As Single,
                              ByRef pcnt1 As Single, ByRef num1 As Integer, ByVal iii As Integer)

        gkk = H_index
        gkk = iii
        Dim n100, n11, n22 As Integer
        Dim locTG As Single = AC(LibIndex(gkk)).libTV


        If (Mid(ACName(gkk), 1, 4) = "A380") Or (Mid(ACName(gkk), 1, 4) = "B747") Then

            n11 = AC(LibIndex(gkk + 1S)).libNTires
            n22 = AC(LibIndex(gkk)).libNTires
            n100 = n11 + n22
            n100 = 2 * n100

            ReDim NewX(n100)
            ReDim NewY(n100)

            Dim min1 As Single

            '====================   BODY   ====================
            min1 = -10000
            For i As Integer = 1 To n22 'Body
                If AC(LibIndex(gkk)).libTX(i) > min1 Then
                    min1 = AC(LibIndex(gkk)).libTX(i)
                End If
            Next
            For i As Integer = 1 To n22 'Body
                NewX(i) = -(-AC(LibIndex(gkk)).libTX(i) + AC(LibIndex(gkk)).libTS / 2 + min1)
                NewX(n11 * 2 + n22 * 2 - i + 1) = -NewX(i)
                NewY(i) = AC(LibIndex(gkk)).libTY(i) + locTG
                NewY(n11 * 2 + n22 * 2 - i + 1) = NewY(i)
            Next

            '====================   BELLY   ====================

            If (Mid(ACName(gkk), 1, 4) = "A380") Then

                Dim tg1 As Single
                tg1 = 5264 / 25.4 / 2

                For i As Integer = 1 To n11  'Belly
                    NewX(i + n22) = -(-AC(LibIndex(gkk + 1S)).libTX(i) + tg1)
                    NewX(n22 + n11 * 2 - i + 1) = -NewX(i + n22)
                    NewY(i + n22) = AC(LibIndex(gkk + 1S)).libTY(n11 + 1 - i)
                    NewY(n22 + n11 * 2 - i + 1) = NewY(i + n22)
                Next

            Else

                min1 = -10000
                For i As Integer = 1 To n11 'Belly
                    If AC(LibIndex(gkk + 1S)).libTX(i) > min1 Then
                        min1 = AC(LibIndex(gkk + 1S)).libTX(i)
                    End If
                Next

                For i As Integer = 1 To n11  'Belly
                    NewX(i + n22) = -(-AC(LibIndex(gkk + 1S)).libTX(i) + AC(LibIndex(gkk + 1S)).libTS / 2 + min1)
                    NewX(n22 + n11 * 2 - i + 1) = -NewX(i + n22)
                    NewY(i + n22) = AC(LibIndex(gkk + 1S)).libTY(n11 + 1 - i)
                    NewY(n22 + n11 * 2 - i + 1) = NewY(i + n22)
                Next

            End If

            pcnt1 = (AC(Save_LibIndex(gkk)).libMGpcntPCN + AC(Save_LibIndex(gkk + 1)).libMGpcntPCN) * 2
            pcnt1 = MGpcnt(gkk) * 2 + MGpcnt(gkk + 1) * 2

        Else 'Airbus other with one belly

            n11 = AC(LibIndex(gkk + 1S)).libNTires 'Belly
            n22 = AC(LibIndex(gkk)).libNTires 'Body
            n100 = n11 + n22 * 2

            ReDim NewX(n100)
            ReDim NewY(n100)

            Dim min1 As Single

            '====================   BODY   ====================
            min1 = -10000
            For i As Integer = 1 To n22 'Body
                If AC(LibIndex(gkk)).libTX(i) > min1 Then
                    min1 = AC(LibIndex(gkk)).libTX(i)
                End If
            Next
            For i As Integer = 1 To n22 'Body
                NewX(i) = -(-AC(LibIndex(gkk)).libTX(i) + AC(LibIndex(gkk)).libTS / 2 + min1)
                NewX(n11 + n22 * 2 - i + 1) = -NewX(i)
                NewY(i) = AC(LibIndex(gkk)).libTY(i) + locTG
                NewY(n11 + n22 * 2 - i + 1) = NewY(i)
            Next

            '====================   BELLY   ====================
            min1 = -10000
            For i As Integer = 1 To n11 'Belly
                If AC(LibIndex(gkk + 1S)).libTX(i) > min1 Then
                    min1 = AC(LibIndex(gkk + 1S)).libTX(i)
                End If
            Next

            For i As Integer = 1 To n11  'Belly
                NewX(i + n22) = AC(LibIndex(gkk + 1S)).libTX(i)
                NewY(i + n22) = AC(LibIndex(gkk + 1S)).libTY(i)
            Next

            'pcnt1 = AC(Save_LibIndex(gkk)).libMGpcntPCN * 2 + AC(Save_LibIndex(gkk + 1)).libMGpcntPCN
            'pcnt1 = AC(Save_LibIndex(gkk)).libMGpcntPCN * 2 + AC(Save_LibIndex(gkk + 1)).libMGpcntPCN
            pcnt1 = MGpcnt(gkk) * 2 + MGpcnt(gkk + 1)

        End If

        num1 = UBound(NewX, 1)

    End Sub






    Private Sub GetCoordinates_C5Type(ByRef NewX() As Single, ByRef NewY() As Single,
                            ByRef pcnt1 As Single, ByRef num1 As Integer, ByVal iii As Integer)

        gkk = iii
        Dim n11, n22 As Integer
        n11 = AC(LibIndex(gkk)).libNTires
        n22 = n11 * 2
        ReDim NewX(n22) : ReDim NewY(n22)

        Dim Wheels As Integer, xCenter, yCenter As Single
        Dim xShift As Single


        Wheels = AC(LI).libNTires
        For J = 1 To Wheels
            xCenter = xCenter + AC(LI).libTX(J)
            yCenter = yCenter + AC(LI).libTY(J)
        Next J

        xCenter = xCenter / Wheels : yCenter = yCenter / Wheels

        Dim RightMostX As Single
        RightMostX = -1.0E+35
        For J = 1 To AC(LI).libNTires
            If AC(LI).libTX(J) > RightMostX Then RightMostX = AC(LI).libTX(J)
        Next J
        xShift = RightMostX + AC(LibIndex(gkk)).libTS / 2

        For i As Integer = 1 To n11 'Body
            NewX(i) = (-xShift + AC(LibIndex(gkk)).libTX(i))
            NewX(n11 + i) = -NewX(i)
            NewY(i) = AC(LibIndex(gkk)).libTY(i)
            NewY(n11 + i) = NewY(i)
        Next

        pcnt1 = MGpcnt(gkk) * 2
        num1 = UBound(NewX, 1)

    End Sub
    Private Sub GetCoordinatesForX_Gear1(ByRef NewX() As Single, ByRef NewY() As Single,
                           ByRef pcnt1 As Single, ByRef num1 As Integer, ByVal iii As Integer)

        num1 = AC(LibIndex(iii)).libNTires1
        ReDim NewX(num1)
        ReDim NewY(num1)

        For i As Integer = 1 To num1 'Body
            NewX(i) = AC(LibIndex(iii)).libTX(i)
            NewY(i) = AC(LibIndex(iii)).libTY(i)
        Next

        Dim Xcg, Ycg As Single
        Xcg = 0 : Ycg = 0

        For i As Integer = 1 To num1
            Xcg = Xcg + NewX(i)
            Ycg = Ycg + NewY(i)
        Next

        Xcg = Xcg / num1 : Ycg = Ycg / num1
        For i As Integer = 1 To num1 'Body
            NewX(i) = NewX(i) - Xcg
            NewY(i) = NewY(i) - Ycg
        Next


        pcnt1 = MGpcnt(iii) * num1 / AC(LibIndex(iii)).libNTires


    End Sub




    Private Sub GetCoordinatesForX_Gear2(ByRef NewX() As Single, ByRef NewY() As Single,
                        ByRef pcnt1 As Single, ByRef num2 As Integer, ByVal iii As Integer)

        num2 = AC(LibIndex(iii)).libNTires2
        If AC(LibIndex(iii)).libIGear = 14 Then
            num2 = num2 * 2
        End If


        ReDim NewX(num2)
        ReDim NewY(num2)

        Dim jj1 As Integer
        For i As Integer = 1 To num2 'Body
            jj1 = i + AC(LibIndex(iii)).libNTires1 * 2
            NewX(i) = AC(LibIndex(iii)).libTX(jj1)
            NewY(i) = AC(LibIndex(iii)).libTY(jj1)
        Next

        Dim Xcg, Ycg As Single
        Xcg = 0 : Ycg = 0

        For i As Integer = 1 To num2
            Xcg = Xcg + NewX(i)
            Ycg = Ycg + NewY(i)
        Next

        Xcg = Xcg / num2 : Ycg = Ycg / num2
        For i As Integer = 1 To num2 'Body
            NewX(i) = NewX(i) - Xcg
            NewY(i) = NewY(i) - Ycg
        Next


        pcnt1 = MGpcnt(iii) * num2 / AC(LibIndex(iii)).libNTires


    End Sub
    Public Function CheckForA340(ByRef lNAC As Short) As Boolean
        'Public Const tonTolb As Double = 2204.6226218
        Dim lW1, lA1, lB1, lC1, lA2, lB2, lC2 As Single
        Dim ACName1 As String, bBelly1 As Boolean
        ACName1 = ACName(lNAC)

        LI = LibIndex(lNAC)
        CheckForA340 = True
        bBelly1 = False

        If (InStr(4, ACName(lNAC), "Belly", CompareMethod.Text) > 0) Then
            ACName1 = ACName(lNAC).Substring(0, ACName(lNAC).Length - 6)
            bBelly1 = True
        Else
            ACName1 = ACName(lNAC)
        End If



        'If (Mid(ACName(lNAC), 1, 4) = "A340-200 std") Then
        If ACName1 = "A340-200 std" Or ACName1 = "A340-200 opt" Then

            lW1 = 255 * tonTolb
            If GL(lNAC) <= lW1 Then
                lA1 = 0.000267229
                lB1 = -0.0384015
                lC1 = 79.0654
            Else
                lA2 = -0.0000279533
                lB2 = 0.0494635
                lC2 = 68.2699
            End If

        ElseIf ACName1 = "A340-300 std" Or ACName1 = "A340-300 opt" Then

            lW1 = 255 * tonTolb
            If GL(lNAC) <= lW1 Then
                lA1 = 0.000305899
                lB1 = -0.0348257
                lC1 = 78.9784
            Else
                lA2 = -0.0000695729
                lB2 = 0.0740839
                lC2 = 64.611
            End If

        ElseIf ACName1 = "A340-500 WV000" Or ACName1 = "A340-500 WV001" Or ACName1 = "A340-500 WV003" Or ACName1 = "A340-500 WV102" Then

            lW1 = 300 * tonTolb
            If GL(lNAC) <= lW1 Then
                lA1 = 0.000176578
                lB1 = -0.0789252
                lC1 = 62.9139
            Else
                lA2 = -0.0000417996
                lB2 = 0.0694457
                lC2 = 45.8422
            End If

        ElseIf ACName1 = "A340-500 WV101" Then

            lW1 = 290 * tonTolb
            If GL(lNAC) <= lW1 Then
                lA1 = 0.000681069
                lB1 = -0.0375472
                lC1 = 63.1467
            Else
                lA2 = -0.0000394864
                lB2 = 0.0499539
                lC2 = 51.9809
            End If

        ElseIf ACName1 = "A340-600 WV000" Or ACName1 = "A340-600 WV001" Or ACName1 = "A340-600 WV103" Then

            lW1 = 300 * tonTolb
            If GL(lNAC) <= lW1 Then
                lA1 = 0.000209874
                lB1 = -0.076488
                lC1 = 62.7883
            Else
                lA2 = -0.0000474603
                lB2 = 0.0743476
                lC2 = 44.7554
            End If

        ElseIf ACName1 = "A340-600 WV101" Then

            lW1 = 290 * tonTolb
            If GL(lNAC) <= lW1 Then
                lA1 = 0.000608954
                lB1 = -0.0422914
                lC1 = 63.2269
            Else
                lA2 = -0.0000443847
                lB2 = 0.0540171
                lC2 = 51.2947
            End If

        Else
            CheckForA340 = False
            Exit Function
        End If


        If GL(lNAC) <= lW1 Then
            MGpcnt(lNAC) = CSng(lA1 * (GL(lNAC) / tonTolb - lW1 / tonTolb) ^ 2 +
                           lB1 * (GL(lNAC) / tonTolb - lW1 / tonTolb) + lC1)
            MGpcnt(lNAC) = MGpcnt(lNAC) / 100 / 2
        Else
            MGpcnt(lNAC) = CSng(lA2 * GL(lNAC) / tonTolb + lB2 * GL(lNAC) / tonTolb + lC2)
            MGpcnt(lNAC) = MGpcnt(lNAC) / 100 / 2
        End If

        If bBelly1 Then
            MGpcnt(lNAC) = CSng(0.95 - MGpcnt(lNAC) * 2)
        End If


        If FF_true_ACN Then
            If ACName1 = "A340-200 std" Then
                MGpcnt(lNAC) = 78.11 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 15.61 / 100
                End If
            ElseIf ACName1 = "A340-200 opt" Then
                MGpcnt(lNAC) = 78.15 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 15.54 / 100
                End If

            ElseIf ACName1 = "A340-300 std" Then
                MGpcnt(lNAC) = 79.35 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 15.22 / 100
                End If

            ElseIf ACName1 = "A340-300 opt" Then
                MGpcnt(lNAC) = 78.72 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 15.11 / 100
                End If

            ElseIf ACName1 = "A340-500 WV000" Or ACName1 = "A340-500 WV001" Or ACName1 = "A340-500 WV003" Or ACName1 = "A340-500 WV102" Then
                MGpcnt(lNAC) = 63.92 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 28.78 / 100
                End If

            ElseIf ACName1 = "A340-500 WV101" Then
                MGpcnt(lNAC) = 63.27 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 28.85 / 100
                End If
            ElseIf ACName1 = "A340-600 WV000" Or ACName1 = "A340-600 WV001" Or ACName1 = "A340-600 WV103" Then
                MGpcnt(lNAC) = 64.42 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 29.1 / 100
                End If
            ElseIf ACName1 = "A340-600 WV101" Then
                MGpcnt(lNAC) = 63.49 / 100 / 2
                If bBelly1 Then
                    MGpcnt(lNAC) = 28.78 / 100
                End If
            End If
        End If


    End Function
    Private Sub GetCoordinates_Body(ByRef NewX1() As Single, ByRef NewY1() As Single, ByVal iii As Integer)
        Dim n11, n22 As Integer
        n11 = AC(LibIndex(iii)).libNTires
        n22 = n11 * 2

        ReDim NewX1(n22), NewY1(n22)

        Dim min1 As Single
        '====================   BODY   ====================
        min1 = -10000
        For i As Integer = 1 To n11 'Body
            If AC(LibIndex(iii)).libTX(i) > min1 Then
                min1 = AC(LibIndex(iii)).libTX(i)
            End If
        Next

        If AC(LibIndex(iii)).libGear = "X" Then
            For i As Integer = 1 To n11 'Body
                NewX1(i) = AC(LibIndex(iii)).libTX(i)
                NewX1(n11 + i) = -NewX1(i)
                NewY1(i) = AC(LibIndex(iii)).libTY(i)
                NewY1(n11 + i) = NewY1(i)
            Next
        Else
            For i As Integer = 1 To n11 'Body
                NewX1(i) = -(-AC(LibIndex(iii)).libTX(i) + AC(LibIndex(iii)).libTS / 2 + min1)
                NewX1(n11 + i) = -NewX1(i)
                NewY1(i) = AC(LibIndex(iii)).libTY(i) + AC(LibIndex(iii)).libTV
                NewY1(n11 + i) = NewY1(i)
            Next

        End If

        'For i As Integer = 1 To n11 'Body
        '    NewX1(i) = -(-AC(LibIndex(iii)).libTX(i) + AC(LibIndex(iii)).libTS / 2 + min1)
        '    NewX1(n11 + i) = -NewX1(i)
        '    NewY1(i) = AC(LibIndex(iii)).libTY(i) + AC(LibIndex(iii)).libTV
        '    NewY1(n11 + i) = NewY1(i)
        'Next


    End Sub
    Public Function ICAOCodeIndexF(ByVal modul1 As Single) As Integer
        'FAARFIELD

        'paragraph 1.1.3.2 a) Subgrade category
        Dim divis1, divis2, divis3 As Single
        'divis1 = 25381.6   '175 MPa
        divis1 = 21755.66 '150 MPa 
        divis2 = 14503.7738  '100 Mpa
        divis3 = 8702.264    '60 Mpa

        'divis1 = 25381.6   '175 MPa
        'divis2 = 14503.77  '100 Mpa
        'divis3 = 8702.26    '60 Mpa

        'If InputCBR >= 13 Then ICAOCodeIndex = 4
        If modul1 >= divis1 Then ICAOCodeIndexF = 4 '                    Category A
        If divis2 < modul1 And modul1 < divis1 Then ICAOCodeIndexF = 3 ' Category B
        If divis3 < modul1 And modul1 <= divis2 Then ICAOCodeIndexF = 2 'Category C
        If modul1 <= divis3 Then ICAOCodeIndexF = 1 '                    Category D

    End Function



    Public Sub SetPCN_for_AC()

        Dim gNAC As Integer
        gNAC = UBound(AC, 1)

        For i As Integer = 1 To gNAC

            'If AC(i).libACName = "A380 Belly" Then
            '    'AC(i).libMGpcntPCN = 57.08 / 100 / 2
            '    AC(i).libMGpcnt = 57.08 / 100 / 2

            'ElseIf AC(i).libACName = "A380e Belly" Then
            '    AC(i).libMGpcnt = 56.59 / 100 / 2

            'ElseIf AC(i).libACName = "B747-100 SF Belly" Then
            '    AC(i).libMGpcnt = 92.48 / 100 / 4

            'ElseIf AC(i).libACName = "B747-200B Combi Mixed Belly" Then
            '    AC(i).libMGpcnt = 90.96 / 100 / 4

            'ElseIf AC(i).libACName = "B747-300 Combi Mixed Belly" Then
            '    AC(i).libMGpcnt = 90.96 / 100 / 4

            'ElseIf AC(i).libACName = "B747-400 Belly" Then
            '    AC(i).libMGpcnt = 93.32 / 100 / 4

            'ElseIf AC(i).libACName = "B747-400ER Belly" Then
            '    AC(i).libMGpcnt = 93.6 / 100 / 4

            'ElseIf AC(i).libACName = "B747-8 Belly" Then
            '    AC(i).libMGpcnt = 94.7 / 100 / 4

            'ElseIf AC(i).libACName = "B747-8F Belly" Then
            '    AC(i).libMGpcnt = 94.4 / 100 / 4

            'ElseIf AC(i).libACName = "B747-SP Belly" Then
            '    AC(i).libMGpcnt = 87.68 / 100 / 4
            'End If

        Next

    End Sub





    Public Sub Print_PCN_results(ByVal lsPavementType As String, ByVal ConversionACCovs() As Double,
                                 ByVal ConversionACCovsToFailure() As Double,
                                 ByVal CriticalACTotalEquivCovs() As Double)

        Dim ThickCriticalAC(NAC) As Double
        Dim ACNThicknessRtn(,) As Double
        Dim S1, SS1 As String

        Dim DTemp As Double ' For printing to see if EvalThick reached.
        Dim UnitsOutName As String
        If EnglishUnits Then
            UnitsOutName = "English"
        Else
            UnitsOutName = "Metric"
        End If

        Dim MaxNWheels As Integer = 0 : Dim MaxNMainGears As Integer = 0

        For I As Integer = 1 To NAC
            If AC(LibIndex(I)).libNTires > MaxNWheels Then
                MaxNWheels = AC(LibIndex(I)).libNTires
            End If
            'If AC(LibIndex(I)).libNTires > MaxNWheels Then
            '    NMainGears = AC(LibIndex(I)).lib
            'End If
        Next

        Dim FF As String

        Dim FullOutput As Boolean = True

        If EnglishUnits Then
            ICAOCode(0) = "D(7k)"
            ICAOCode(1) = "C(11k)"
            ICAOCode(2) = "B(17k)"
            ICAOCode(3) = "A(29k)"
        Else
            ICAOCode(0) = "D(50 MPa)"
            ICAOCode(1) = "C(80 Mpa)"
            ICAOCode(2) = "B(120 MPa)"
            ICAOCode(3) = "A(200 Mpa)"
        End If

        Dim InputGrossWeight(NAC) As Double, InputAnnualDepartures(NAC) As Double
        Dim ThickFlexible(NAC) As Double, ThickRigid(NAC) As Double
        Dim ThickLife(NAC) As Double




        FF = getTodaysDateFormatted()

        If lsPavementType = "Flexible" Then
            FileName = "PCN Results Flexible " & FF & ".txt"
        ElseIf lsPavementType = "Rigid" Then
            FileName = "PCN Results Rigid " & FF & ".txt"
        End If


        S1 = "This file name = " & FileName & NL
        'S = S & "Library file name = " & ExtFilePath & ExternalAircraftFileName & ".Ext" & NL
        'S1 = S1 & "Section name: " & SectName & " in job file: " & WorkingDir & "\" & JobName & ".JOB.xml" & NL
        S1 = S1 & "Section name: " & SectName & " in job file: " & NL & WorkingDir & JobName & ".JOB.xml" & NL

        S1 = S1 & "Units = " & UnitsOutName & NL2


        If lsPavementType = "Flexible" Then
            S1 = S1 & "Evaluation pavement type is flexible and design program is FAARFIELD." & NL
        ElseIf lsPavementType = "Rigid" Then
            S1 = S1 & "Evaluation pavement type is rigid and design program is FAARFIELD." & NL
        End If

        SS1 = ""
        S1 = S1 & SS1.ToString() & NL

        Dim modul1 As Single
        modul1 = Modulus(NPLayers)
        ICAOCodeIndex = ICAOCodeIndexF(modul1)

        SS1 = CStr((ICAOCode(ICAOCodeIndex - 1)))

        S1 = S1 & StrDup(26, " ") & "Subgrade Modulus = " & Format(Modulus(NPLayers), "#,##0") & " psi" & " (Subgrade Category is " & SS1.ToString() & ")" & NL

        If lsPavementType = "Flexible" Then
        ElseIf lsPavementType = "Rigid" Then
            S1 = S1 & StrDup(25, " ") & "flexural strength = "
            S1 = S1 & Format(ConcreteFlexuralStrength * UnitsOut.psi, UnitsOut.psiFormat) & " " & UnitsOut.psiName & NL1
        End If


        S1 = S1 & "             Evaluation pavement thickness = "
        S1 = S1 & Format(EvalThick * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName & NL
        S1 = S1 & "       Pass to Traffic Cycle (PtoTC) Ratio = " & Format(PtoTC, "0.00")
        If PtoTC < 1.0# Or 3.0# < PtoTC Or (PtoTC - Math.Floor(PtoTC)) <> 0 Then S = S & " (non-standard)"
        S1 = S1 & NL

        S1 = S1 & "         Maximum number of wheels per gear = " & Format(MaxNWheels, "0") & NL

        S1 = S1 & NL
        'S1 = S1 & "        Maximum number of gears per aircraft = " & Format(MaxNMainGears, "0") & NL2

        If lsPavementType = "Flexible" Then

            If MaxNWheels >= 4 Then
                S1 = S1 & "At least one aircraft has 4 or more wheels per gear." & NL2  'The FAA recommends a reference section assuming" & NL
                'If UnitsOut.Metric Then
                '    S1 = S1 & "127 mm of HMA and 203 mm of crushed aggregate for equivalent thickness calculations." & NL2
                'Else
                '    S1 = S1 & "5 inches of HMA and 8 inches of crushed aggregate for equivalent thickness calculations." & NL2
                'End If
            Else
                S1 = S1 & "No aircraft have 4 or more wheels per gear." & NL2 'The FAA recommends a reference section assuming" & NL
                'If UnitsOut.Metric Then
                '    S1 = S1 & "76 mm of HMA and 152 mm of crushed aggregate for equivalent thickness calculations." & NL2
                'Else
                '    S1 = S1 & "3 inches of HMA and 6 inches of crushed aggregate for equivalent thickness calculations." & NL2
                'End If
            End If


        End If

        'ACName(1) = "1234567890-1234567890-123"

        S1 = S1 & "Results Table 1. Input Traffic Data" & NL


        S1 = S1 & "                             Gross   Percent    Tire     Annual      20-yr" & NL
        S1 = S1 & " No.  Aircraft Name          Weight  Gross Wt   Press     Deps     Coverages" & NL
        S1 = S1 & StrDup(76, "-") & NL

        For I As Integer = 1 To NAC
            S1 = S1 & LPad(3, Format(I, "0")) & "  " & RPad2(20, ACName(I))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "
            'S = S & LPad(7, String.Format("{0:N2}", PercentGross(I)))
            'S = S & LPad(7, String.Format("{0:N2}", 2 * AC(LibIndex(I)).libMGpcnt * 100))

            If AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            S1 = S1 & LPad(10, String.Format("{0:N1}", AC(LibIndex(I)).libCP * UnitsOut.psi, UnitsOut.psiFormat))
            S1 = S1 & LPad(10, String.Format("{0:N0}", RepsAnnual(I), "#,###,##0")) ' GFH 9/28/09.

            'If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            '    S1 = S1 & LPad(12, String.Format("{0:N0}", RepsAnnual(I) * Life / gPtoC(I), "#,###,##0"))
            'Else
            '    S1 = S1 & LPad(12, String.Format("{0:N0}", RepsAnnual(I) * Life / gPtoC_Rigid(I), "#,###,##0"))
            'End If

            'S1 = S1 & LPad(12, String.Format("{0:N0}", RepsAnnual(I) * Life / gPtoC(I), "#,###,##0"))
            S1 = S1 & LPad(12, String.Format("{0:N0}", ConversionACCovs(I), "#,###,##0"))


            'S1 = S1 & LPad(8, String.Format("{0:N2}", ThickFlexible(I) * UnitsOut.inch)) & NL '6D Thickness
            S1 = S1 & NL
        Next I

        '============================================================================================
        S1 = S1 & NL
        'ListBoxLibraryAircraft.SelectedIndex = lstLibFileListIndexSave

        'S = ""

        S1 = S1 & "Results Table 2. PCN Values" & NL

        FullOutput = False '!!!!!
        If FullOutput Then
            S1 = S1 & "                          Critical        Thickness      Maximum    " & NL
            S1 = S1 & "                       Aircraft Total     for Total     Allowable       PCN at Indicated Code" & NL
            If lsPavementType = "Flexible" Then
                S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Equiv. Covs.  Gross Weight    A(15)  B(10)   C(6)   D(3)    CDF" & NL
            Else
                S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Equiv. Covs.  Gross Weight   " & ICAOCode(3) & ICAOCode(2) & ICAOCode(1) & ICAOCode(0) & "    CDF" & NL
            End If
            S1 = S1 & StrDup(99, "-") & NL
        Else
            'S1 = S1 & "                          Critical        Thickness      Maximum      ACN Thick at" & NL
            'S1 = S1 & "                          Critical        Thickness      Maximum        Life for" & NL
            'S1 = S1 & "                       Aircraft Total     for Total     Allowable    Max. Allowable           PCN on" & NL
            'S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Equiv. Covs.  Gross Weight   Gross Weight    CDF      " & ICAOCode(ICAOCodeIndex - 1) & NL
            'S1 = S1 & StrDup(99, "-") & NL

            'S1 = S1 & "                          Critical          Maximum        Life for" & NL
            'S1 = S1 & "                       Aircraft Total      Allowable         MGW                PCN on" & NL
            'S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Gross Weight                   CDF      " & ICAOCode(ICAOCodeIndex - 1) & NL

            S1 = S1 & "                          Critical          Maximum      ACN Thick" & NL
            S1 = S1 & "                       Aircraft Total      Allowable      at max.              PCN on" & NL
            S1 = S1 & " No. Aircraft Name       Equiv. Covs.    Gross Weight       MGW        CDF     " & ICAOCode(ICAOCodeIndex - 1) & NL
            S1 = S1 & StrDup(86, "-") & NL
        End If


        ReDim ACNrtn(NAC, 4)
        ReDim ACNThicknessRtn(NAC, 4)


        Dim cdf1, cdf_max As Double, cdf_string As String, cdf_length As Integer
        Dim Spaces1, Spaces2 As Integer




        For I As Integer = 1 To NAC
            cdf1 = ConversionACCovs(I) / ConversionACCovsToFailure(I)
            If cdf_max < cdf1 Then cdf_max = cdf1
        Next

        cdf_string = CStr(Format(Math.Round(cdf_max, 4), "#,##0.0000"))
        cdf_length = cdf_string.Length

        If cdf_length > 11 Then
            Spaces1 = cdf_length
            Spaces2 = 9 - (cdf_length - 11)
            If Spaces2 < 6 Then Spaces2 = 6
        Else
            Spaces1 = 11
            Spaces2 = 9
        End If


        Dim seq1 As Integer = 0

        Dim CDFTotal As Double = 0.0#
        For I As Integer = 1 To NAC

            If Not (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
                seq1 = seq1 + 1
                'S1 = S1 & LPad(3, Format(I, "0")) & "  " & RPad2(20, ACName(I))
                S1 = S1 & LPad(3, Format(seq1, "0")) & "  " & RPad2(20, ACName(I))
            End If

            If CriticalACTotalEquivCovs(I) = 0.0 Then
                Continue For
            End If

            If CriticalACTotalEquivCovs(I) <= 5000000 Then ' About one departure per minute for 20 years.
                S1 = S1 & LPad(11, String.Format("{0:N0}", CriticalACTotalEquivCovs(I)))
            Else
                S1 = S1 & LPad(11, ">5,000,000")
            End If
            'S1 = S1 & LPad(13, String.Format("{0:N2}", ThickCriticalAC(I) * UnitsOut.inch)) 'commented


            'S = S & LPad(16, String.Format("{0:N0}", MaxGrossWeight(I) * UnitsOut.pounds))
            S1 = S1 & LPad(16, String.Format("{0:N0}", gMGW(I) * UnitsOut.pounds))
            If FullOutput Then
                S1 = S1 & LPad(10, String.Format("{0:N2}", ACNrtn(I, 4))) & LPad(7, String.Format("{0:N2}", ACNrtn(I, 3)))
                S1 = S1 & LPad(7, String.Format("{0:N2}", ACNrtn(I, 2))) & LPad(7, String.Format("{0:N2}", ACNrtn(I, 1)))
            Else
                'S = S & LPad(14, String.Format("{0:N2}", ACNThicknessRtn(I, ICAOCodeIndex) * UnitsOut.inch))
                'S1 = S1 & LPad(14, String.Format("{0:N3}", gLifeStr(I)))
                S1 = S1 & LPad(12, String.Format("{0:N2}", gACN_GL_thick(I)))

            End If


            DTemp = ConversionACCovs(I) / ConversionACCovsToFailure(I)
            CDFTotal += DTemp
            'S1 = S1 & LPad(11, String.Format("{0:N4}", DTemp))
            S1 = S1 & LPad(Spaces1, String.Format("{0:N4}", DTemp))

            'S1 = S1 & LPad(9, String.Format("{0:N1}", ACNrtn(I, ICAOCodeIndex))) & NL
            'S1 = S1 & LPad(9, String.Format("{0:N1}", gACR(I))) & NL
            S1 = S1 & LPad(Spaces2, String.Format("{0:N1}", gACN_GL(I))) & NL
        Next I

        'J = IIf(FullOutput, 85, 70)
        J = 70

        'S1 = S1 & New String(" "c, J) & "Total CDF =" & LPad(9, String.Format("{0:N4}", CDFTotal)) & NL
        S1 = S1 & New String(" "c, 53) & "Total CDF =" & LPad(11, String.Format("{0:N4}", CDFTotal)) & NL2

        Dim SS7, sT1 As String
        SS7 = ICAOCode(ICAOCodeIndex - 1)
        sT1 = PaveTypeF(lPavementType)

        S1 = S1 & "Results Table 3. " & sT1 & " ACN at Indicated Gross Weight and Strength" & NL1
        S1 = S1 & " No. Aircraft Name          Gross    % GW on     Tire       ACN      ACN on" & NL1
        S1 = S1 & "                            Weight  Main Gear  Pressure    Thick      " & SS7 & NL1
        S1 = S1 & "--------------------------------------------------------------------------" & NL1

        seq1 = 0
        For I As Integer = 1 To NAC

            If Not (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
                seq1 = seq1 + 1
                'S1 = S1 & LPad(3, Format(I, "0")) & "  "
                S1 = S1 & LPad(3, Format(seq1, "0")) & "  "
                S1 = S1 & (RPad2(20, ACName(I)))

            End If

            If CriticalACTotalEquivCovs(I) = 0.0 Then
                Continue For
            End If

            'S1 = S1 & (LPad(10, Format(GL(I), "#,###,###")))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "

            '7777777
            If AC(LibIndex(I)).libGear = "H" Or (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * gPcnt1(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            'S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libMGpcntPCN, "#0.00")))

            S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libCP, "##.0")))
            S1 = S1 & (LPad(11, Format(gACN_GL_thick(I), "##.00")))
            S1 = S1 & (LPad(10, Format(gACN_GL(I), "##.0"))) & NL1
        Next

        S1 = S1 & NL1
        S1 = S1 & LPad(5, CStr(Format(gETimemsecs, "#0"))) & " sec." & NL1
        S1 = S1 & LPad(4, CStr(Format(gETimemsecs / 60, "#0.00"))) & " min."

        'extra output start XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        S1 = S1 & NL2
        S1 = S1 & "===============================================" & NL1

        S1 = S1 & LPad(3, "LP") &
            LPad(10, "PtoC") & "  " &
            LPad(15, "NtoFail_life") & "  " &
            LPad(15, "CriticTot_targ") & "  " &
            LPad(15, "Cov_Life") & "  " _
            & LPad(15, "Difference") &
            LPad(12, "%") & NL


        For i As Integer = 1 To NAC
            S1 = S1 & (LPad(3, Format(i, "#")))
            S1 = S1 & (LPad(10, Format(gPtoC(i), "#0.00000"))) & "  "
            S1 = S1 & (LPad(15, Format(gFlex_NtoFail_Copy(i), "#,##0.00"))) & "  "
            S1 = S1 & (LPad(15, Format(CriticalACTotalEquivCovs(i), "#,##0.00"))) & "  "
            '  S1 = S1 & (LPad(15, Format(gNtoFail(i), "#,##0.00"))) & "  "
            Dim abc As Double
            ' abc = (gNtoFail(i) - CriticalACTotalEquivCovs(i)) / CriticalACTotalEquivCovs(i)
            abc = 1
            'S1 = S1 & (LPad(15, Format(gNtoFail(i) - CriticalACTotalEquivCovs(i), "#,##0.00")))
            S1 = S1 & (LPad(12, Format(abc * 100, "#,##0.00000"))) & NL1
        Next

        S1 = S1 & NL2

        For j1 As Integer = 1 To NAC
            S1 = S1 & (LPad(3, Format(j1, "#")))
            For i1 As Integer = 1 To NAC
                S1 = S1 & (LPad(21, Format(gCriticalACEquivCovs(i1, j1), "#,##0.00")))
            Next
            S1 = S1 & NL1
        Next

        S1 = S1 & NL1

        For j1 As Integer = 1 To NAC
            S1 = S1 & (LPad(3, Format(j1, "#")))
            S1 = S1 & (LPad(21, Format(gSTRAIN(j1), "#,##0.00000000")))
            S1 = S1 & NL1
        Next

        'extra output   end XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        PCNOutputText = S1

        'Dim frmPCN As New frmPCN
        Dim frmPCN As New FormPCN
        frmPCN.Show()

    End Sub



    Public Sub Print_PCN_results2(ByVal lsPavementType As ACRClassLib.clsACR.PavementType, ByVal ConversionACCovs() As Double)

        Dim ThickCriticalAC(NAC) As Double
        Dim ACNThicknessRtn(,) As Double
        Dim S1, SS1 As String

        Dim DTemp As Double ' For printing to see if EvalThick reached.
        Dim UnitsOutName As String
        If EnglishUnits Then
            UnitsOutName = "English"
        Else
            UnitsOutName = "Metric"
        End If

        Dim MaxNWheels As Integer = 0 : Dim MaxNMainGears As Integer = 0

        For I As Integer = 1 To NAC
            If AC(LibIndex(I)).libNTires > MaxNWheels Then
                MaxNWheels = AC(LibIndex(I)).libNTires
            End If
        Next

        Dim FF As String

        Dim FullOutput As Boolean = True

        If EnglishUnits Then
            ICAOCode(0) = "D"
            ICAOCode(1) = "C"
            ICAOCode(2) = "B"
            ICAOCode(3) = "A"
        Else
            ICAOCode(0) = "D"
            ICAOCode(1) = "C"
            ICAOCode(2) = "B"
            ICAOCode(3) = "A"
        End If

        Dim InputGrossWeight(NAC) As Double, InputAnnualDepartures(NAC) As Double
        Dim ThickFlexible(NAC) As Double, ThickRigid(NAC) As Double
        Dim ThickLife(NAC) As Double




        FF = getTodaysDateFormatted()

        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
            FileName = "PCR Results Flexible " & FF & ".txt"
        ElseIf lsPavementType = ACRClassLib.clsACR.PavementType.Rigid Then
            FileName = "PCR Results Rigid " & FF & ".txt"
        End If

        S1 = "This file name = " & FileName & NL
        S1 = S1 & "Section name: " & SectName & " in job file: " & NL & WorkingDir & JobName & ".JOB.xml" & NL
        S1 = S1 & "Units = " & UnitsOutName & NL2

        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
            S1 = S1 & "Evaluation pavement type is flexible and design program is FAARFIELD." & NL
        ElseIf lsPavementType = ACRClassLib.clsACR.PavementType.Rigid Then
            S1 = S1 & "Evaluation pavement type is rigid and design program is FAARFIELD." & NL
        End If

        SS1 = ""
        S1 = S1 & SS1.ToString() & NL

        Dim modul1 As Single
        modul1 = Modulus(NPLayers)
        ICAOCodeIndex = ICAOCodeIndexF(modul1) 'FAARFIELD


        SS1 = CStr((ICAOCode(ICAOCodeIndex - 1)))
        SubgradeCategoryPCRReport = SS1.ToString
        S1 = S1 & StrDup(24, " ") & "Subgrade Modulus = " & Format(Modulus(NPLayers), "#,##0") & " psi" & " (Subgrade Category is " & SS1.ToString() & ")" & NL

        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
        ElseIf lsPavementType = ACRClassLib.clsACR.PavementType.Rigid Then
            S1 = S1 & StrDup(23, " ") & "Flexural strength = "
            S1 = S1 & Format(ConcreteFlexuralStrength * UnitsOut.psi, UnitsOut.psiFormat) & " " & UnitsOut.psiName & NL1
        End If

        S1 = S1 & StrDup(11, " ") & "Evaluation pavement thickness = "
        S1 = S1 & Format(EvalThick * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName & NL
        S1 = S1 & "     Pass to Traffic Cycle (PtoTC) Ratio = " & Format(PtoTC, "0.00")
        If PtoTC < 1.0# Or 3.0# < PtoTC Or (PtoTC - Math.Floor(PtoTC)) <> 0 Then S = S & " (non-standard)"
        S1 = S1 & NL
        S1 = S1 & "       Maximum number of wheels per gear = " & Format(MaxNWheels, "0") & NL

        If OverlayRigOnRig Or DesignType = FlexOnRigid Then
            S1 = S1 & StrDup(28, " ") & "Overlay Life = " & Format(gOverlayLife_target_copy, "#,##0.000") & NL
            'ElseIf DesignType = FlexOnRigid Then
            '    If AnalyzedasFlexible Then

            '    Else

            '    End If
        Else
            S1 = S1 & StrDup(37, " ") & "CDF = " & Format(gCDF_target_copy, "#,##0.000") & NL
        End If


        S1 = S1 & NL
        If lsPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
            'The FAA recommends a reference section assuming" & NL
            If MaxNWheels >= 4 Then
                S1 = S1 & "At least one aircraft has 4 or more wheels per gear." & NL2
            Else
                S1 = S1 & "No aircraft have 4 or more wheels per gear." & NL2
            End If
        End If

        'ACName(1) = "1234567890-1234567890-123"
        S1 = S1 & "Results Table 1. Input Traffic Data" & NL
        S1 = S1 & "                             Gross   Percent    Tire     Annual      20-yr" & NL
        S1 = S1 & " No.  Aircraft Name          Weight  Gross Wt   Press     Deps     Coverages" & NL
        S1 = S1 & StrDup(76, "-") & NL

        For I As Integer = 1 To NAC
            S1 = S1 & LPad(3, Format(I, "0")) & "  " & RPad2(20, ACName(I))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "

            If AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))

            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))

            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            S1 = S1 & LPad(10, String.Format("{0:N1}", AC(LibIndex(I)).libCP * UnitsOut.psi, UnitsOut.psiFormat))
            S1 = S1 & LPad(10, String.Format("{0:N0}", RepsAnnual(I), "#,###,##0")) ' GFH 9/28/09.
            S1 = S1 & LPad(12, String.Format("{0:N0}", ConversionACCovs(I), "#,###,##0"))
            S1 = S1 & NL
        Next I

        '============================================================================================
        S1 = S1 & NL & NL
        'ListBoxLibraryAircraft.SelectedIndex = lstLibFileListIndexSave

        S1 = S1 & "Results Table 2. PCR Value" & NL
        S1 = S1 & "                     Critical        Max. Allowable      ACR Thick" & NL
        S1 = S1 & "                  Aircraft Total    Gross Wt of the        at max.    PCR on" & NL
        S1 = S1 & "Aircraft Name    Equiv. Departures Critical Aircraft         MGW       " & ICAOCode(ICAOCodeIndex - 1) & NL
        S1 = S1 & StrDup(76, "-") & NL

        FullOutput = False '!!!!!
        ReDim ACNrtn(NAC, 4)
        ReDim ACNThicknessRtn(NAC, 4)

        Dim seq1 As Integer = 0
        Dim CDFTotal As Double = 0.0#

        S1 = S1 & RPad2(20, gNewACName(indexPCNtable2))         'S1 = S1 & LPad(11, "")
        S1 = S1 & LPad(12, String.Format("{0:N0}", gNewAnnualDepart(indexPCNtable2)))
        S1 = S1 & LPad(16, String.Format("{0:N0}", gNewGL(indexPCNtable2) * UnitsOut.pounds))
        S1 = S1 & LPad(16, String.Format("{0:N2}", gNewPCNthick(indexPCNtable2) * UnitsOut.inch))
        S1 = S1 & LPad(12, String.Format("{0:N1}", gNewPCN(indexPCNtable2))) & NL

        'J = IIf(FullOutput, 85, 70)
        J = 70
        'S1 = S1 & New String(" "c, J) & "Total CDF =" & LPad(9, String.Format("{0:N4}", CDFTotal)) & NL
        'S1 = S1 & New String(" "c, 53) & "Total CDF =" & LPad(11, String.Format("{0:N4}", CDFTotal)) & NL2
        S1 = S1 & NL & NL
        '==========================================================================================

        Dim SS7, sT1 As String
        SS7 = LPad(8, ICAOCode(ICAOCodeIndex - 1))
        sT1 = PaveTypeF(lPavementType)

        S1 = S1 & "Results Table 3. " & sT1 & " ACR at Indicated Gross Weight and Strength" & NL1
        S1 = S1 & " No. Aircraft Name          Gross    % GW on     Tire       ACR       ACR on" & NL1
        S1 = S1 & "                            Weight  Main Gear  Pressure    Thick    " & SS7 & NL1
        S1 = S1 & StrDup(76, "-") & NL

        seq1 = 0
        For I As Integer = 1 To NAC

            Call Check_H_Aircraft(I)

            'If H_index = I - 1 Then
            '    Continue For
            'End If

            If H_index = 0 Then
            Else
                If H_index = I - 1 Then
                    Continue For
                End If
            End If

            seq1 = seq1 + 1
            S1 = S1 & LPad(3, Format(seq1, "0")) & "  "
            S1 = S1 & (RPad2(20, ACName(I)))
            S1 = S1 & LPad(10, String.Format("{0:N0}", GL(I) * UnitsOut.pounds)) & " "

            If AC(LibIndex(I)).libGear = "A" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "X" Then
                S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            ElseIf AC(LibIndex(I)).libGear = "H" Then
                If (Mid(ACName(I), 1, 4) = "A380") Or (Mid(ACName(I), 1, 4) = "B747") Then
                    S1 = S1 & LPad(7, String.Format("{0:N2}", MGpcnt(I) * 200 + MGpcnt(I + 1) * 200))
                Else
                    S1 = S1 & LPad(7, String.Format("{0:N2}", MGpcnt(I) * 200 + MGpcnt(I + 1) * 100))
                End If
            Else
                S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            End If

            'If AC(LibIndex(I)).libGear = "H" Or (InStr(4, ACName(I), "Belly", CompareMethod.Text) > 0) Then
            '    'S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * gPcnt1(I) * 100))
            '    'S1 = S1 & LPad(7, String.Format("{0:N2}", MGpcnt(I) * 100 + MGpcnt(I + 1) * 100))
            '    S1 = S1 & LPad(7, String.Format("{0:N2}", AC(LibIndex(I)).libMGpcnt * 200 + AC(LibIndex(I + 1)).libMGpcnt * 200))
            'ElseIf AC(LibIndex(I)).libGear = "HB" Or AC(LibIndex(I)).libGear = "A" Then
            '    S1 = S1 & LPad(7, String.Format("{0:N2}", 1 * MGpcnt(I) * 100))
            'Else
            '    S1 = S1 & LPad(7, String.Format("{0:N2}", 2 * MGpcnt(I) * 100))
            'End If
            ''S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libMGpcntPCN, "#0.00")))

            S1 = S1 & (LPad(10, Format(AC(LibIndex(I)).libCP, "##.0")))
            S1 = S1 & (LPad(11, Format(gACN_GL_thick(I), "##.00")))
            S1 = S1 & (LPad(12, Format(gACN_GL(I), "##.0"))) & NL1
        Next

        S1 = S1 & NL1
        S1 = S1 & LPad(5, CStr(Format(gETimemsecs, "#0"))) & " sec." & NL1
        S1 = S1 & LPad(4, CStr(Format(gETimemsecs / 60, "#0.00"))) & " min."

        PCNOutputText = S1

        'Dim frmPCN As New frmPCN
        Dim frmPCN As New FormPCN
        'frmPCN.Show()

    End Sub




    Public Sub Set_NAC_lessOne_Aircraft_dataM(ByVal in1 As Integer, ByRef call1 As Integer)
        Dim ind3 As Integer

        NAC = SaveM_NAC(call1)
        ind3 = 0

        If H_index = 0 Then
            For i11 As Integer = 1 To NAC
                If i11 = in1 Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = SaveM_LibIndex(i11, call1)
                ACName(ind3) = SaveM_ACName(i11, call1)
                GL(ind3) = SaveM_GL(i11, call1)
                RepsAnnual(ind3) = SaveM_RepsAnnual(i11, call1)
                RepsInc(ind3) = SaveM_RepsInc(i11, call1)
                MGpcnt(ind3) = SaveM_MGpcnt(i11, call1)
                NAC_order(ind3) = SaveM_NAC_order(i11, call1)
            Next
            NAC = SaveM_NAC(call1) - 1S
        Else
            For i11 As Integer = 1 To NAC
                If (i11 = H_index) Or (i11 = H_index + 1) Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = SaveM_LibIndex(i11, call1)
                ACName(ind3) = SaveM_ACName(i11, call1)
                GL(ind3) = SaveM_GL(i11, call1)
                RepsAnnual(ind3) = SaveM_RepsAnnual(i11, call1)
                RepsInc(ind3) = SaveM_RepsInc(i11, call1)
                MGpcnt(ind3) = SaveM_MGpcnt(i11, call1)
                NAC_order(ind3) = SaveM_NAC_order(i11, call1)
            Next
            NAC = SaveM_NAC(call1) - 2S
        End If

    End Sub



    Public Sub Set_NAC_lessOne_Aircraft_data(ByVal in1 As Integer)
        Dim ind3 As Integer

        NAC = Save_NAC
        ind3 = 0

        If H_index = 0 Then
            For i11 As Integer = 1 To NAC
                If i11 = in1 Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save_LibIndex(i11)
                ACName(ind3) = Save_ACName(i11)
                GL(ind3) = Save_GL(i11)
                RepsAnnual(ind3) = Save_RepsAnnual(i11)
                RepsInc(ind3) = Save_RepsInc(i11)
                MGpcnt(ind3) = Save_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save_NAC - 1S
        Else
            For i11 As Integer = 1 To NAC
                If (i11 = H_index) Or (i11 = H_index + 1) Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save_LibIndex(i11)
                ACName(ind3) = Save_ACName(i11)
                GL(ind3) = Save_GL(i11)
                RepsAnnual(ind3) = Save_RepsAnnual(i11)
                RepsInc(ind3) = Save_RepsInc(i11)
                MGpcnt(ind3) = Save_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save_NAC - 2S
        End If

    End Sub



    Public Sub Set_NAC_lessOne_Aircraft_data2(ByVal in1 As Integer)
        Dim ind3 As Integer

        NAC = Save2_NAC
        ind3 = 0

        If H_index = 0 Then
            For i11 As Integer = 1 To NAC
                If i11 = in1 Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save2_LibIndex(i11)
                ACName(ind3) = Save2_ACName(i11)
                GL(ind3) = Save2_GL(i11)
                RepsAnnual(ind3) = Save2_RepsAnnual(i11)
                RepsInc(ind3) = Save2_RepsInc(i11)
                MGpcnt(ind3) = Save2_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save2_NAC - 1S
        Else
            For i11 As Integer = 1 To NAC
                If (i11 = H_index) Or (i11 = H_index + 1) Then
                    Continue For
                End If
                ind3 = ind3 + 1
                LibIndex(ind3) = Save2_LibIndex(i11)
                ACName(ind3) = Save2_ACName(i11)
                GL(ind3) = Save2_GL(i11)
                RepsAnnual(ind3) = Save2_RepsAnnual(i11)
                RepsInc(ind3) = Save2_RepsInc(i11)
                MGpcnt(ind3) = Save2_MGpcnt(i11)
                NAC_order(ind3) = Save_NAC_order(i11)
            Next
            NAC = Save2_NAC - 2S
        End If

    End Sub






    Public Sub Check_H_Aircraft(ByVal index1 As Integer)

        H_index = 0

        Try

            If index1 >= 1 Then
                If AC(LibIndex(index1)).libACName.Contains("Belly") Then
                    H_index = index1 - 1
                End If
            ElseIf AC(LibIndex(index1 + 1)).libACName.Contains("Belly") Then
                H_index = index1
            End If
        Catch ex As Exception

        End Try

    End Sub


    Public Function Check_H_Aircraft_Function(ByVal index1 As Integer) As Boolean

        H_index = 0

        Try

            If index1 > 1 Then
                If AC(LibIndex(index1)).libACName.Contains("Belly") Then
                    H_index = index1 - 1

                ElseIf index1 + 1 <= AC.Count - 1 AndAlso AC(LibIndex(index1 + 1)).libACName.Contains("Belly") Then
                    H_index = index1
                End If

            End If
        Catch ex As Exception

        End Try

        If H_index = 0 Then
            Check_H_Aircraft_Function = False
        Else
            If H_index = index1 - 1 Then
                Check_H_Aircraft_Function = True
            End If
        End If


    End Function



    Public Sub Set_OneAircraft_data3(ByVal in1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = Save3_LibIndex(in1)
            ACName(1) = Save3_ACName(in1)
            GL(1) = Save3_GL(in1)
            RepsAnnual(1) = Save3_RepsAnnual(in1)
            RepsInc(1) = Save3_RepsInc(in1)
            MGpcnt(1) = Save3_MGpcnt(in1)
            NAC_order(1) = Save3_NAC_order(in1)

        Else

            NAC = 2
            LibIndex(1) = Save3_LibIndex(H_index)
            ACName(1) = Save3_ACName(H_index)
            GL(1) = Save3_GL(H_index)
            RepsAnnual(1) = Save3_RepsAnnual(H_index)
            RepsInc(1) = Save3_RepsInc(H_index)
            MGpcnt(1) = Save3_MGpcnt(H_index)
            NAC_order(1) = Save3_NAC_order(H_index)

            LibIndex(2) = Save3_LibIndex(H_index + 1)
            ACName(2) = Save3_ACName(H_index + 1)
            GL(2) = Save3_GL(H_index + 1)
            RepsAnnual(2) = Save3_RepsAnnual(H_index + 1)
            RepsInc(2) = Save3_RepsInc(H_index + 1)
            MGpcnt(2) = Save3_MGpcnt(H_index + 1)
            NAC_order(1) = Save3_NAC_order(H_index + 1)

        End If

    End Sub




    Public Sub Set_OneAircraft_data2(ByVal in1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = Save2_LibIndex(in1)
            ACName(1) = Save2_ACName(in1)
            GL(1) = Save2_GL(in1)
            RepsAnnual(1) = Save2_RepsAnnual(in1)
            RepsInc(1) = Save2_RepsInc(in1)
            MGpcnt(1) = Save2_MGpcnt(in1)
            NAC_order(1) = Save2_NAC_order(in1)

        Else

            NAC = 2
            LibIndex(1) = Save2_LibIndex(H_index)
            ACName(1) = Save2_ACName(H_index)
            GL(1) = Save2_GL(H_index)
            RepsAnnual(1) = Save2_RepsAnnual(H_index)
            RepsInc(1) = Save2_RepsInc(H_index)
            MGpcnt(1) = Save2_MGpcnt(H_index)
            NAC_order(1) = Save2_NAC_order(H_index)

            LibIndex(2) = Save2_LibIndex(H_index + 1)
            ACName(2) = Save2_ACName(H_index + 1)
            GL(2) = Save2_GL(H_index + 1)
            RepsAnnual(2) = Save2_RepsAnnual(H_index + 1)
            RepsInc(2) = Save2_RepsInc(H_index + 1)
            MGpcnt(2) = Save2_MGpcnt(H_index + 1)
            NAC_order(2) = Save2_NAC_order(H_index + 1)

        End If

    End Sub


    Public Sub Set_OneAircraft_dataM(ByVal in1 As Integer, ByVal call1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = SaveM_LibIndex(in1, call1)
            ACName(1) = SaveM_ACName(in1, call1)
            GL(1) = SaveM_GL(in1, call1)
            RepsAnnual(1) = SaveM_RepsAnnual(in1, call1)
            RepsInc(1) = SaveM_RepsInc(in1, call1)
            MGpcnt(1) = SaveM_MGpcnt(in1, call1)
            NAC_order(1) = SaveM_NAC_order(in1, call1)

        Else

            NAC = 2
            LibIndex(1) = SaveM_LibIndex(H_index, call1)
            ACName(1) = SaveM_ACName(H_index, call1)
            GL(1) = SaveM_GL(H_index, call1)
            RepsAnnual(1) = SaveM_RepsAnnual(H_index, call1)
            RepsInc(1) = SaveM_RepsInc(H_index, call1)
            MGpcnt(1) = SaveM_MGpcnt(H_index, call1)
            NAC_order(1) = SaveM_NAC_order(H_index, call1)

            LibIndex(2) = SaveM_LibIndex(H_index + 1, call1)
            ACName(2) = SaveM_ACName(H_index + 1, call1)
            GL(2) = SaveM_GL(H_index + 1, call1)
            RepsAnnual(2) = SaveM_RepsAnnual(H_index + 1, call1)
            RepsInc(2) = SaveM_RepsInc(H_index + 1, call1)
            MGpcnt(2) = SaveM_MGpcnt(H_index + 1, call1)
            NAC_order(2) = SaveM_NAC_order(H_index + 1, call1)

        End If



    End Sub


    Public Sub Set_OneAircraft_data(ByVal in1 As Integer)

        If H_index = 0 Then

            NAC = 1
            LibIndex(1) = Save_LibIndex(in1)
            ACName(1) = Save_ACName(in1)
            GL(1) = Save_GL(in1)
            RepsAnnual(1) = Save_RepsAnnual(in1)
            RepsInc(1) = Save_RepsInc(in1)
            MGpcnt(1) = Save_MGpcnt(in1)
            NAC_order(1) = Save_NAC_order(in1)

        Else

            NAC = 2
            LibIndex(1) = Save_LibIndex(H_index)
            ACName(1) = Save_ACName(H_index)
            GL(1) = Save_GL(H_index)
            RepsAnnual(1) = Save_RepsAnnual(H_index)
            RepsInc(1) = Save_RepsInc(H_index)
            MGpcnt(1) = Save_MGpcnt(H_index)
            NAC_order(1) = Save_NAC_order(H_index)

            LibIndex(2) = Save_LibIndex(H_index + 1)
            ACName(2) = Save_ACName(H_index + 1)
            GL(2) = Save_GL(H_index + 1)
            RepsAnnual(2) = Save_RepsAnnual(H_index + 1)
            RepsInc(2) = Save_RepsInc(H_index + 1)
            MGpcnt(2) = Save_MGpcnt(H_index + 1)
            NAC_order(2) = Save_NAC_order(H_index + 1)

        End If


    End Sub

    Public Sub RedimSaveM(ByVal iNAC As Integer)

        ReDim SaveM_NAC(iNAC)
        ReDim SaveM_LibIndex(iNAC, iNAC)
        ReDim SaveM_ACName(iNAC, iNAC)
        ReDim SaveM_GL(iNAC, iNAC)
        ReDim SaveM_RepsAnnual(iNAC, iNAC)
        ReDim SaveM_RepsInc(iNAC, iNAC)
        ReDim SaveM_WT(iNAC, iNAC)
        ReDim SaveM_TW(iNAC, iNAC)
        ReDim SaveM_MGpcnt(iNAC, iNAC)
        ReDim SaveM_NAC_order(iNAC, iNAC)

    End Sub



    Public Sub SaveM_Aircraft_data(ByVal ind1 As Integer)

        SaveM_NAC(ind1) = NAC
        For I As Short = 1 To NAC
            SaveM_LibIndex(I, ind1) = LibIndex(I)
            SaveM_ACName(I, ind1) = ACName(I)
            SaveM_GL(I, ind1) = GL(I)
            SaveM_RepsAnnual(I, ind1) = RepsAnnual(I)
            SaveM_RepsInc(I, ind1) = RepsInc(I)
            SaveM_MGpcnt(I, ind1) = MGpcnt(I)
            SaveM_NAC_order(I, ind1) = NAC_order(I)
        Next

    End Sub

    Public Sub Save_Aircraft_data()

        Save_NAC = NAC
        If Save_NAC = 1 Then CDFChecker = True
        For I As Short = 1 To NAC
            Save_LibIndex(I) = LibIndex(I)
            Save_ACName(I) = ACName(I)
            Save_GL(I) = GL(I)
            Save_RepsAnnual(I) = RepsAnnual(I)
            Save_RepsInc(I) = RepsInc(I)
            Save_MGpcnt(I) = MGpcnt(I)
            Save_NAC_order(I) = NAC_order(I)
        Next

    End Sub

    Public Sub Save2_Aircraft_data()

        Save2_NAC = NAC
        For I As Short = 1 To NAC
            Save2_LibIndex(I) = LibIndex(I)
            Save2_ACName(I) = ACName(I)
            Save2_GL(I) = GL(I)
            Save2_RepsAnnual(I) = RepsAnnual(I)
            Save2_RepsInc(I) = RepsInc(I)
            Save2_MGpcnt(I) = MGpcnt(I)
            Save2_NAC_order(I) = NAC_order(I)
        Next

    End Sub


    Public Sub Save3_Aircraft_data()

        Save3_NAC = NAC
        For I As Short = 1 To NAC
            Save3_LibIndex(I) = LibIndex(I)
            Save3_ACName(I) = ACName(I)
            Save3_GL(I) = GL(I)
            Save3_RepsAnnual(I) = RepsAnnual(I)
            Save3_RepsInc(I) = RepsInc(I)
            Save3_MGpcnt(I) = MGpcnt(I)
            Save3_NAC_order(I) = NAC_order(I)
        Next

    End Sub






    Public Sub Restore_Aircraft_data()

        NAC = Save_NAC
        For I As Short = 1 To NAC
            LibIndex(I) = Save_LibIndex(I)
            ACName(I) = Save_ACName(I)
            GL(I) = Save_GL(I)
            RepsAnnual(I) = Save_RepsAnnual(I)
            RepsInc(I) = Save_RepsInc(I)
            MGpcnt(I) = Save_MGpcnt(I)
            NAC_order(I) = Save_NAC_order(I)
        Next

    End Sub


    Public Sub Set_Aircraft_data(ByVal ind1 As Short)

        LibIndex(1) = Save_LibIndex(ind1)
        ACName(1) = Save_ACName(ind1)
        GL(1) = Save_GL(ind1)
        RepsAnnual(1) = Save_RepsAnnual(ind1)
        RepsInc(1) = Save_RepsInc(ind1)
        MGpcnt(1) = Save_MGpcnt(ind1)
        NAC_order(1) = Save_NAC_order(ind1)

    End Sub

    Public Sub NewIterationMethod(ByVal ind As Integer)

        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If

        GL(1) = CSng(GrossWeight)

        LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        'Call frmStructure.cmdAddDelete_Click(sender, e)

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If

        If publicNtoFail > gTARGET Then
            Do While publicNtoFail > gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight + 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While publicNtoFail < gTARGET And Not Math.Abs(publicNtoFail - gTARGET) < LDiff
                GrossWeight = GrossWeight - 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


    End Sub



    Public Sub NewIterationFlexible(ByVal ind As Integer)

        Dim LDiff As Single = 0.095 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.01
        End If



        GL(1) = CSng(GrossWeight)

        LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        ' Call frmStructure.cmdAddDelete_Click(sender, e)

        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - GrossWeight * 0.1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 1000
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 100
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If



        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 10
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If


        If LifeStr > Life Then
            Do While LifeStr > Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight + 1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        Else
            Do While LifeStr < Life And Not Math.Abs(LifeStr - Life) < LDiff
                GrossWeight = GrossWeight - 1
                Call ACNFlexComp_FF_Life(ind)
            Loop
        End If



    End Sub

    Public Sub ACNFlexComp_FF_Life(ByVal ind As Integer)

        GL(1) = CSng(GrossWeight)

        LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        '  Call frmStructure.cmdAddDelete_Click(sender, e)

        'GrossWeight = GL(1)

        CBRThickness(1) = 0

        If DesignType = NewFlex Then 'ACNFlexComp_FF_Life
            For i As Short = 1 To NPLayers
                CBRThickness(1) = CBRThickness(1) + Thick(i)
            Next
        Else
            CBRThickness(1) = Thick(1)
        End If




    End Sub

    Public Sub ACNFlexComp_FF(ByVal ind As Integer)

        GL(1) = CSng(GrossWeight)

        LifeComputation = False : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        ' Call frmStructure.cmdAddDelete_Click(sender, e)
        ' Call PCNLifeCalc()

        'GrossWeight = GL(1)

        CBRThickness(1) = 0

        If DesignType = NewFlex Then
            For i As Short = 1 To NPLayers
                CBRThickness(1) = CBRThickness(1) + Thick(i)
            Next
        Else
            CBRThickness(1) = Thick(1)
        End If

    End Sub

    Public Sub GetMGW_FF(ByRef CallSource As String, ByRef MGW As Double,
               ByRef ReturnCoverages As Double, ByRef CriticalACThick As Double,
               ByVal Ind As Integer)

        Dim I, NAircraft As Integer
        '  Dim CriticalACIndex As Long ' Made a public variable so it can be set from a mouse down event.
        '  Dim EvalThick As Double ' Made a public variable so it can be set from a click event.
        Dim CriticalACCovsToFailure As Double
        Dim CriticalACEquivCovs() As Double, ConversionACCovs() As Double
        'Dim CriticalACTotalEquivCovs As Double
        'Dim ConversionACCovsToFailure() As Double 2015.09.16
        Dim Thickness, Covs, DelCovs, LastThick, DelThick, ThickM3, ThickM1, ThickM2, ThickM4, Func, FuncM1, GrossWeightM1, GrossWt, GrossWeightM2 As Double
        Dim FlexiblePCN As Boolean
        Dim LogCovs, LogCoverages, DelLogCovs As Double
        Dim SS, S, FullFileName As String
        Dim chkPCAThicknessDesignSave As Boolean
        Const Exp As Double = 2.7182818285
        Const MaxFlexLogCovs As Double = 18.42 ' = Log(100,000,000)

        'If Not (MinEvalThickness <= EvalThick And EvalThick <= MaxEvalThickness) Then
        '   windows.forms.messagebox.show("Please enter a valid evaluation thickness.", "Enter Evaluation Thickness")
        '    Exit Sub
        'End If
        '  lstLibFile.ListIndex = CriticalACIndex



        GrossWeight = GL(Ind)
        AnnualDepartures = RepsAnnual(Ind)
        'WheelRadius1 = 8.6947454228772738
        'PtoCFlex = 1 : PtoCRigid = 1
        'FlexibleCoverages = AnnualDepartures * Life / PtoCFlex
        'RigidCoverages = AnnualDepartures * Life / PtoCRigid

        'CriticalACIndex = ListBoxLibraryAircraft.SelectedIndex 'PPPP
        Dim GrossWeightSave As Double = GrossWeight
        Dim AnnualDeparturesSave As Double = AnnualDepartures
        Dim WheelRadiusSave As Double = WheelRadius1
        Dim FlexibleCoveragesSave As Double = FlexibleCoverages
        Dim RigidCoveragesSave As Double = RigidCoverages
        '  Coverages = CriticalACTotalEquivCovs
        Dim DelGrossWt As Double = GrossWeight / 100 ' Do not set to be very small or successive thickness calculations may be equal.
        Dim NewtonCoeff As Double = 1.0#
        Dim J As Integer = 0

        If MaxGWFlexible Then

            '-----------------------------------------------------
            'Call NewIterationFlexible(Ind)
            Call NewIterationMethod(Ind)


            CriticalACThick = CBRThickness(1) ' Used only for output.
            FuncM1 = CBRThickness(1) - EvalThick
            ReturnCoverages = FlexibleCoverages
            '-----------------------------------------------------


            GoTo Skip_To_100





            ACNFlexComp_FF(Ind) '1
            CriticalACThick = CBRThickness(1) ' Used only for output.
            FuncM1 = CBRThickness(1) - EvalThick

            Do
                J += 1
                GrossWt = GrossWeight
                GrossWeight += DelGrossWt
                LastThick = CBRThickness(1)
                ACNFlexComp_FF(Ind) '2
                If J >= 50 Then
                    SS = "The gross weight iteration cannot converge. " & NL2
                    SS = SS & "The last two thickness values were:" & NL2
                    SS = SS & Format(LastThick * UnitsOut.inch, UnitsOut.inchFormat) & " and "
                    SS = SS & Format(CBRThickness(1) * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName
                    Windows.Forms.MessageBox.Show(SS, "Cannot Converge")
                    Exit Do
                End If
                DelThick = CBRThickness(1) - LastThick

                If DelThick = 0 Then
                    SS = "Two successive thickness evaluations gave the same thickness. " & NL
                    SS = SS & "The calculation of allowable gross weight cannot converge. " & NL
                    SS = SS & "The evaluation thickness may be too large."
                    Windows.Forms.MessageBox.Show(SS, "Thickness May Be Too Large")
                    Exit Do
                End If

                NewtonCoeff = 1
                Do
                    'If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * MinGLFraction) Then
                    If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * 0.1) Then
                        NewtonCoeff /= 2
                    Else
                        Exit Do
                    End If
                Loop
                GrossWeight = GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick
                '      If CallSource = "MGWOnly" Then
                '      If chkMGWcovs.Value = vbUnchecked Then
                '       Computes MGW based on annual departures because converts to coverages in CoverageToPass().
                '        WheelRadius = GrossWeight * PcntOnMainGears / (100 * NMainGears * NWheels)
                '        WheelRadius = Sqr(WheelRadius / TirePressure / PI)
                '        Call CoverageToPass ' Sets FlexibleCoverages and RigidCoverages, used in ACNFlexComp().
                '        ReturnCoverages = FlexibleCoverages
                '      ElseIf CallSource = "MGWforPCN" Then
                '      ElseIf chkMGWcovs.Value = vbChecked Then ' Do nothing.
                '       Computes MGW based on coverages not annual departures.
                ReturnCoverages = FlexibleCoverages
                '      End If
                ACNFlexComp_FF(Ind) '3
                FuncM1 = CBRThickness(1) - EvalThick

                'Application.DoEvents()
                '      Debug.Print J; "GrossWeight = "; GrossWeight; " "; NewtonCoeff
                NewtonCoeff = 1.0#
            Loop Until Math.Abs(FuncM1) < 0.0001


Skip_To_100:

        Else

            'PPPP
            'chkPCAThicknessDesignSave = CheckBoxPCAThick.IsChecked
            'If CheckBoxPCAMGW.IsChecked = True Then
            '    CheckBoxPCAThick.IsChecked = True
            'Else
            '    CheckBoxPCAThick.IsChecked = False
            'End If

            'Call NewIterationFlexible(Ind) 'for Rigid


            Call NewIterationMethod(Ind) 'for Rigid



            CriticalACThick = CBRThickness(1) ' Used only for output.
            FuncM1 = CBRThickness(1) - EvalThick
            ReturnCoverages = FlexibleCoverages
            '-----------------------------------------------------


            GoTo Skip_To_222



            RigidACN(I)
            CriticalACThick = RigidThickness(1) ' Used only for output.
            FuncM1 = RigidThickness(1) - EvalThick

            Do
                J += 1
                GrossWt = GrossWeight
                GrossWeight += DelGrossWt
                LastThick = RigidThickness(1)
                RigidACN(I)
                If RigidThickness(1) = 0 Then Exit Do ' Occurs in ACNRigComp when very light load for PCA.
                DelThick = RigidThickness(1) - LastThick
                If J >= 50 Then
                    SS = "The gross weight iteration cannot converge." & NL2
                    SS = SS & "The last two thickness values were:" & NL2
                    SS = SS & Format(LastThick * UnitsOut.inch, UnitsOut.inchFormat) & " and "
                    SS = SS & Format(RigidThickness(1) * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName
                    Windows.Forms.MessageBox.Show(SS, "Cannot Converge")
                    Exit Do
                End If
                If DelThick = 0 Then
                    SS = "Two successive thickness evaluations gave the same thickness." & NL
                    SS = SS & "The calculation of allowable gross weight cannot converge." & NL
                    SS = SS & "The evaluation thickness may be too large."
                    Windows.Forms.MessageBox.Show(SS, "Thickness May Be Too Large")
                    Exit Do
                End If
                Do
                    'If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * MinGLFraction) Then
                    If (GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick) < (GrossWt * 0.1) Then
                        NewtonCoeff /= 2
                    Else
                        Exit Do
                    End If
                Loop
                GrossWeight = GrossWt - NewtonCoeff * FuncM1 * DelGrossWt / DelThick

                '      If CallSource = "MGWOnly" Then
                '      If chkMGWcovs.Value = vbUnchecked Then
                '       Computes MGW based on annual departures because converts to coverages in CoverageToPass().
                '        WheelRadius = GrossWeight * PcntOnMainGears / (100 * NMainGears * NWheels)
                '        WheelRadius = Sqr(WheelRadius / TirePressure / PI)
                '        Call CoverageToPass ' Sets FlexibleCoverages and RigidCoverages, used in RigidACN().
                '        ReturnCoverages = RigidCoverages
                '      ElseIf CallSource = "MGWforPCN" Then
                '      ElseIf chkMGWcovs.Value = vbChecked Then
                '       Computes MGW based on coverages not annual departures.
                '        ReturnCoverages = FlexibleCoverages ' Output error found by Jeff Rapol. Fixed by GFH 03/06/13.
                ReturnCoverages = RigidCoverages
                '      End If

                RigidACN(I)
                If RigidThickness(1) = 0 Then Exit Do
                '     The following is required for the H51 program gross weight determination. H51 appears to have fairly
                '     large granularity in this mode. Maybe gross weight is converted to an integer, or something similar.
                '     The linear interpolation should work ok with the PCA method for the last step.
                GrossWeightM2 = GrossWeightM1
                GrossWeightM1 = GrossWeight
                ThickM4 = ThickM3
                ThickM3 = ThickM2
                ThickM2 = ThickM1
                ThickM1 = RigidThickness(1)
                '      Debug.Print J; ThickM4; ThickM3; ThickM2; ThickM1; RigidThickness(1); GrossWeightM2; GrossWeightM1
                If (ThickM3 = ThickM1 And ThickM4 = ThickM2) Or (Math.Abs(EvalThick - ThickM1) < 0.1 And J > 1) Then
                    DelThick = ThickM1 - ThickM2
                    If Math.Abs(DelThick) > 1.0E-20 Then ' Let the J >= 20 test above terminate if necessary.
                        GrossWeight = ((GrossWeightM1 - GrossWeightM2) / DelThick) * (EvalThick - ThickM2) + GrossWeightM2
                        RigidThickness(1) = EvalThick
                        '          Debug.Print J; ThickM2; ThickM1; GrossWeightM2; GrossWeightM1; EvalThick; GrossWeight
                        Exit Do
                    End If
                End If
                FuncM1 = RigidThickness(1) - EvalThick
                '      Debug.Print J; "GrossWeight = "; GrossWeight; "Thickness = "; RigidThickness(1); "Coverages = "; Coverages; " "; NewtonCoeff
                NewtonCoeff = 1.0#
            Loop Until Math.Abs(FuncM1) < 0.0001
            System.Windows.Forms.Application.DoEvents()
            '    Debug.Print J; "GrossWeight = "; GrossWeight; "Thickness = "; RigidThickness(1); "Coverages = "; Coverages
            Debug.WriteLine("Pcnt = " & PcntOnMainGears & "Area = " & TireContactArea)
            'CheckBoxPCAThick.IsChecked = chkPCAThicknessDesignSave 'PPPP

        End If


Skip_To_222:

        'PPPP
        'ButtonFlexible.IsEnabled = True
        'ButtonRigid.IsEnabled = True

        'PPPP
        'SetStatusMessage("MGW = " & Format(GrossWeight * UnitsOut.pounds, UnitsOut.poundsFormat) & " " & UnitsOut.poundsName)
        '  S = S & "    Critical Airplane Allowable Gross Weight = "
        '  S = S & Format(GrossWeight * UnitsOut.pounds, UnitsOut.poundsFormat) & " " & UnitsOut.poundsName & vbCrLf & vbCrLf

        '  txtMGW(lstLibFile.ListIndex) = GrossWeight
        MGW = GrossWeight

        '  libGL(libIndex) = GrossWeightSave
        '  libCoverages(libIndex) = CoveragesSave
        GrossWeight = GrossWeightSave
        WheelRadius1 = WheelRadiusSave
        FlexibleCoverages = FlexibleCoveragesSave
        RigidCoverages = RigidCoveragesSave
        AnnualDepartures = AnnualDeparturesSave
        'WriteParmGrid() 'PPPP

    End Sub




    Public Sub NewAdjustAnnDepart_Old()

        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If

        LDiff = gCDF_target * CSng(gTolerance)


        'GL(1) = CSng(GrossWeight)

        'LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        'Call frmStructure.cmdAddDelete_Click(sender, e)
        Call PCNLifeCalc()

        'If Math.Abs(LifeStr - 20) < LDiff Then
        If Math.Abs(CDFPic - gCDF_target) < LDiff Then
            Exit Sub
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                Call PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                Call PCNLifeCalc()
            Loop
        End If

        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + 100
                Call PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - 100
                If RepsAnnual(1) <= 0 Then
                    RepsAnnual(1) = 100
                End If
                Call PCNLifeCalc()
            Loop
        End If




        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + 10
                Call PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - 10
                If RepsAnnual(1) <= 0 Then
                    RepsAnnual(1) = 10
                End If
                Call PCNLifeCalc()
            Loop
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) + 1
                Call PCNLifeCalc()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                RepsAnnual(1) = RepsAnnual(1) - 1
                If RepsAnnual(1) <= 0 Then
                    RepsAnnual(1) = 1
                End If
                Call PCNLifeCalc()
            Loop
        End If



    End Sub



    Public Sub NewAdjustGrossWeight_Old()

        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025

        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If

        LDiff = gCDF_target * CSng(gTolerance)

        'GL(1) = CSng(GrossWeight)

        'LifeComputation = True : DesigningStr = Not LifeComputation
        Dim sender As Object
        Dim e As System.EventArgs
        'Call frmStructure.cmdAddDelete_Click(sender, e)
        Call PCNLifeCalc()

        'If Math.Abs(LifeStr - 20) < LDiff Then
        If Math.Abs(CDFPic - gCDF_target) < LDiff Then
            Exit Sub
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + GL(1) * CSng(0.1)
                Call PCNLifeCalc()
                'frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
                'frmStructure.Refresh()
                System.Windows.Forms.Application.DoEvents()
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - GL(1) * CSng(0.1)
                If GL(1) <= 0 Then
                    GL(1) = 100
                End If
                Call PCNLifeCalc()
                'frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
                ' frmStructure.Refresh()
                System.Windows.Forms.Application.DoEvents()

            Loop
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + 2000
                Call PCNLifeCalc()
                'frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - 2000
                If GL(1) <= 0 Then
                    GL(1) = 2000
                    Exit Do
                End If
                Call PCNLifeCalc()
                ' frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
            Loop
        End If



        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + 100
                Call PCNLifeCalc()
                '  frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - 100
                If GL(1) <= 0 Then
                    GL(1) = 100
                    Exit Do
                End If
                Call PCNLifeCalc()
                '' frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
            Loop
        End If


        If CDFPic < gCDF_target Then
            Do While CDFPic < gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) + 10
                Call PCNLifeCalc()
                ' frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
            Loop
        Else
            Do While CDFPic > gCDF_target And Not Math.Abs(CDFPic - gCDF_target) < LDiff
                GL(1) = GL(1) - 10
                If GL(1) <= 0 Then
                    GL(1) = 10
                    Exit Do
                End If
                Call PCNLifeCalc()
                ' frmStructure.btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4))
            Loop
        End If



    End Sub



    Public Sub NewAdjustGrossWeight2017()

        Dim target1, result1 As Single
        Dim LDiff As Single
        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        target1 = GetTarget1()
        LDiff = target1 * CSng(gTolerance)
        result1 = GetResult1()


        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            If CDFPic > target1 Then
                Do
                    tt2 = GL(1) : SS2 = CDFPic

                    If CDFPic / gCDF_target > 10 Then
                        GL(1) = CSng(GL(1) * 0.85)
                    ElseIf CDFPic / gCDF_target > 4 Then
                        GL(1) = CSng(GL(1) * 0.9)
                    ElseIf CDFPic / gCDF_target > 2 Then
                        GL(1) = CSng(GL(1) * 0.93)
                    ElseIf CDFPic / gCDF_target > 1.2 Then
                        GL(1) = CSng(GL(1) * 0.95)
                    Else
                        GL(1) = CSng(GL(1) * 0.97)
                    End If

                    If GL(1) <= 100 Then
                        GL(1) = 100
                        GL(2) = GL(1)
                        If SS1 = SS2 Then Exit Sub
                    End If
                    GL(2) = GL(1)

                    result1 = GetResult1()
                    tt1 = GL(1) : SS1 = CDFPic
                Loop Until (Math.Abs(CDFPic - target1) < LDiff) Or (CDFPic < target1)
            Else
                Do
                    tt1 = GL(1) : SS1 = CDFPic
                    If CDFPic / gCDF_target < 0.01 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.25)
                    ElseIf CDFPic / gCDF_target < 0.1 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.17)
                    ElseIf CDFPic / gCDF_target < 0.3 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.075)
                    ElseIf CDFPic / gCDF_target < 0.75 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.045)
                    Else
                        GL(1) = GL(1) + GL(1) * CSng(0.02)
                    End If

                    GL(2) = GL(1)
                    result1 = GetResult1()
                    tt2 = GL(1) : SS2 = CDFPic
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic > gCDF_target)
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic - gCDF_target) < LDiff) Then
            Exit Sub
        End If

        If GL(1) <= 2 Then
            If gCDF_target > CDFPic Then
                Exit Sub
            End If
        End If

        GL(1) = CSng((tt1 + tt2) / 2)
        GL(2) = GL(1)
        result1 = GetResult1()

        Dim zz1, zz2, aa, bb As Double
rep1:

        If SS1 < 0.001 Then
            SS1 = 0.001
        End If

        If SS2 < 0.001 Then
            SS2 = 0.001
        End If

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1

        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        GL(1) = CSng(ttACN)
        GL(2) = GL(1)
        result1 = GetResult1()

        If CDFPic < gCDF_target Then
            tt2 = GL(1)
            SS2 = CDFPic
        Else
            tt1 = GL(1)
            SS1 = CDFPic
        End If
        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            GoTo rep1
        End If

    End Sub




    Public Sub NewAdjustGrossWeight2()

        gIterLevel1A = 0 : gIterLevel1B = 0 : gIterLevel2 = 0
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        LDiff = gCDF_target * CSng(gTolerance)
        Dim SS1, SS2, tt1, tt2 As Double
        '====================================================================
        'Call SubLoopCalculations()
        '====================================================================

        '  Call PCNLifeCalc()
        Call PCNLifeCalc()

        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            If CDFPic > gCDF_target Then
                Do
                    tt2 = GL(1) : SS2 = CDFPic

                    If CDFPic / gCDF_target > 100000 Then
                        GL(1) = CSng(GL(1) * 0.65)
                    ElseIf CDFPic / gCDF_target > 10000 Then
                        GL(1) = CSng(GL(1) * 0.7)
                    ElseIf CDFPic / gCDF_target > 1000 Then
                        GL(1) = CSng(GL(1) * 0.75)
                    ElseIf CDFPic / gCDF_target > 100 Then
                        GL(1) = CSng(GL(1) * 0.8)
                    ElseIf CDFPic / gCDF_target > 10 Then
                        GL(1) = CSng(GL(1) * 0.85)
                    ElseIf CDFPic / gCDF_target > 4 Then
                        GL(1) = CSng(GL(1) * 0.9)
                    ElseIf CDFPic / gCDF_target > 2 Then
                        GL(1) = CSng(GL(1) * 0.93)
                    ElseIf CDFPic / gCDF_target > 1.2 Then
                        GL(1) = CSng(GL(1) * 0.95)
                    Else
                        GL(1) = CSng(GL(1) * 0.97)
                    End If

                    If GL(1) <= 10 Then
                        GL(1) = 10 : GL(2) = 10
                        If SS1 = SS2 Then Exit Sub
                    End If
                    GL(2) = GL(1)

                    Call PCNLifeCalc()
                    tt1 = GL(1) : SS1 = CDFPic
                    gIterLevel1A = gIterLevel1A + 1

                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic < gCDF_target)
            Else
                Do
                    tt1 = GL(1) : SS1 = CDFPic
                    If CDFPic / gCDF_target < 0.01 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.25)
                    ElseIf CDFPic / gCDF_target < 0.1 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.17)
                    ElseIf CDFPic / gCDF_target < 0.3 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.075)
                    ElseIf CDFPic / gCDF_target < 0.75 Then
                        GL(1) = GL(1) + GL(1) * CSng(0.045)
                    Else
                        GL(1) = GL(1) + GL(1) * CSng(0.02)
                    End If

                    GL(2) = GL(1)
                    Call PCNLifeCalc()
                    tt2 = GL(1) : SS2 = CDFPic
                    gIterLevel1B = gIterLevel1B + 1
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic > gCDF_target)
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic - gCDF_target) < LDiff) Then
            Exit Sub
        End If

        If GL(1) <= 2 Then
            If gCDF_target > CDFPic Then
                Exit Sub
            End If
        End If

        GL(1) = CSng((tt1 + tt2) / 2)
        GL(2) = GL(1)
        Call PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
        Dim iCountSteps As Integer = 0
rep1:
        iCountSteps = iCountSteps + 1

        Dim min99 As Double = 0.0000000001 'mod900
        min99 = 0.001
        If SS1 < min99 Then
            SS1 = min99
        End If

        If SS2 < min99 Then
            SS2 = min99
        End If

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1

        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        GL(1) = CSng(ttACN)
        GL(2) = GL(1)
        Call PCNLifeCalc()

        If CDFPic < gCDF_target Then
            tt2 = GL(1)
            SS2 = CDFPic
        Else
            tt1 = GL(1)
            SS1 = CDFPic
        End If
        If (Math.Abs(CDFPic - gCDF_target) > LDiff) And (iCountSteps < 25) Then
            gIterLevel2 = gIterLevel2 + 1
            GoTo rep1
        End If

    End Sub

    Public Sub NewAdjustAnnDepart2017() 'mod900

        Dim LDiff As Double = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gCDF_target * gTolerance
        Dim SS1, SS2 As Double, tt1, tt2 As Double
        Dim i11, i22, i33 As Integer
        i11 = 0 : i22 = 0 : i33 = 0

        Dim x11, x22, y11, y22, aaa, bbb As Double

        'Dim keep As Single
        'keep = RepsAnnual(1)
        'Call TEST_NewAdjustAnnDepart2017()
        'RepsAnnual(1) = keep


        If RepsAnnual(1) <= 0 And gCDF_target > 0 Then
            RepsAnnual(1) = 10
        End If

        Call PCNLifeCalc()

        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            If CDFPic > gCDF_target Then
                Do
                    i11 = i11 + 1
                    tt2 = RepsAnnual(1) : SS2 = CDFPic

                    If i11 = 1 Then
                        x11 = CDFPic
                        y11 = RepsAnnual(1)
                    End If

                    If i11 = 2 Then
                        x22 = CDFPic
                        y22 = RepsAnnual(1)
                    End If


                    If CDFPic / gCDF_target > 10 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf CDFPic / gCDF_target > 4 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf CDFPic / gCDF_target > 2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf CDFPic / gCDF_target > 1.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If
                    If RepsAnnual(1) <= 2 Then
                        RepsAnnual(1) = 2
                    End If

                    If i11 = 2 Then
                        bbb = (y22 - y11) / (x22 - x11)
                        aaa = y11 - bbb * x11
                        RepsAnnual(1) = CSng(aaa + bbb * gCDF_target)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt1 = RepsAnnual(1) : SS1 = CDFPic
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic < gCDF_target)
            Else
                Do
                    i22 = i22 + 1
                    tt1 = RepsAnnual(1) : SS1 = CDFPic

                    If i22 = 1 Then
                        x11 = CDFPic
                        y11 = RepsAnnual(1)
                    End If

                    If i22 = 2 Then
                        x22 = CDFPic
                        y22 = RepsAnnual(1)
                    End If

                    If CDFPic / gCDF_target < 0.001 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(36)
                    ElseIf CDFPic / gCDF_target < 0.01 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(18)
                    ElseIf CDFPic / gCDF_target < 0.1 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(9)
                    ElseIf CDFPic / gCDF_target < 0.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf CDFPic / gCDF_target < 0.75 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf CDFPic / gCDF_target < 0.85 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    If i22 = 2 Then
                        bbb = (y22 - y11) / (x22 - x11)
                        aaa = y11 - bbb * x11
                        RepsAnnual(1) = CSng(aaa + bbb * gCDF_target)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt2 = RepsAnnual(1) : SS2 = CDFPic
                Loop Until (Math.Abs(CDFPic - gCDF_target) < LDiff) Or (CDFPic > gCDF_target)
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic - gCDF_target) < LDiff) Then
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gCDF_target > CDFPic Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        If SS1 <= 0 Then
            SS1 = 1.1
        End If

        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()
        i33 = i33 + 1

        If CDFPic < gCDF_target Then
            tt2 = RepsAnnual(1)
            SS2 = CDFPic
        Else
            tt1 = RepsAnnual(1)
            SS1 = CDFPic
        End If
        If Math.Abs(CDFPic - gCDF_target) > LDiff Then
            GoTo rep1
        End If

    End Sub




    Public Function CDFPic8() As Single
        'CDFPic8 = CDFdata2(1, 1, gCriticalOffset)
        CDFPic8 = CDFPic
    End Function



    Public Sub NewAdjustAnnDepart2()

        'Call NewAdjustAnnDepart2new()
        'Exit Sub

        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gCDF_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Dim sender As Object, e As System.EventArgs
        Call PCNLifeCalc()

        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            If gCDF_target < CDFPic8() Then
                Do
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()

                    If CDFPic8() / gCDF_target > 10 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf CDFPic8() / gCDF_target > 4 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf CDFPic8() / gCDF_target > 2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf CDFPic8() / gCDF_target > 1.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If

                    'RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                    If RepsAnnual(1) <= 100 Then RepsAnnual(1) = 100
                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()
                    'Loop While (gCDF_target < CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target > CDFPic8())
            Else
                Do
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()

                    If CDFPic8() / gCDF_target < 0.01 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(15)
                    ElseIf CDFPic8() / gCDF_target < 0.1 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(7)
                    ElseIf CDFPic8() / gCDF_target < 0.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf CDFPic8() / gCDF_target < 0.75 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf CDFPic8() / gCDF_target < 0.85 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()
                    'Loop While (gCDF_target > CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target < CDFPic8())
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Then
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gCDF_target > CDFPic8() Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()

        If CDFPic8() < gCDF_target Then
            tt2 = RepsAnnual(1)
            SS2 = CDFPic8()
        Else
            tt1 = RepsAnnual(1)
            SS1 = CDFPic8()
        End If
        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            GoTo rep1
        End If

    End Sub


    Public Sub NewAdjustAnnDepart2new()

        'Dim CDFPic8 As Single
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gCDF_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Dim sender As Object, e As System.EventArgs
        Call PCNLifeCalc()

        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            If gCDF_target < CDFPic8() Then
                Do
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()

                    If CDFPic8() / gCDF_target > 10 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf CDFPic8() / gCDF_target > 4 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf CDFPic8() / gCDF_target > 2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf CDFPic8() / gCDF_target > 1.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If

                    'RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                    If RepsAnnual(1) <= 100 Then RepsAnnual(1) = 100
                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()
                    'Loop While (gCDF_target < CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target > CDFPic8())
            Else
                Do
                    tt1 = RepsAnnual(1) : SS1 = CDFPic8()
                    If CDFPic8() / gCDF_target < 0.01 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(15)
                    ElseIf CDFPic8() / gCDF_target < 0.1 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(7)
                    ElseIf CDFPic8() / gCDF_target < 0.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf CDFPic8() / gCDF_target < 0.75 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf CDFPic8() / gCDF_target < 0.85 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt2 = RepsAnnual(1) : SS2 = CDFPic8()
                    'Loop While (gCDF_target > CDFPic8) 'Or (Math.Abs(CDFPic8 - gCDF_target) < LDiff)
                Loop Until (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Or (gCDF_target < CDFPic8())
            End If
        Else
            Exit Sub
        End If


        If (Math.Abs(CDFPic8() - gCDF_target) < LDiff) Then
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gCDF_target > CDFPic8() Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gCDF_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()

        If CDFPic8() < gCDF_target Then
            tt2 = RepsAnnual(1)
            SS2 = CDFPic8()
        Else
            tt1 = RepsAnnual(1)
            SS1 = CDFPic8()
        End If
        If Math.Abs(CDFPic8() - gCDF_target) > LDiff Then
            GoTo rep1
        End If

    End Sub



    Public Sub NewAdjustAnnDepartOverlay()

        Dim gLife_target As Single = gOverlayLife_target
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            LDiff = 0.1
        End If
        LDiff = gLife_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Dim sender As Object, e As System.EventArgs
        Call PCNLifeCalc()


        'Dim t77(100) As Single
        'For i As Integer = 100 To 1000 Step 1000
        '   Call PCNLifeCalc()
        '    t77(i) = OverlayLife
        'Next


        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            If gLife_target > OverlayLife Then
                Do
                    tt2 = RepsAnnual(1) : SS2 = OverlayLife

                    If OverlayLife / gLife_target < 0.01 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.65)
                    ElseIf OverlayLife / gLife_target < 0.2 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.7)
                    ElseIf OverlayLife / gLife_target < 0.5 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.75)
                    ElseIf OverlayLife / gLife_target < 0.75 Then
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.85)
                    Else
                        RepsAnnual(1) = CSng(RepsAnnual(1) * 0.9)
                    End If

                    'RepsAnnual(1) = RepsAnnual(1) - RepsAnnual(1) * CSng(0.1)
                    If RepsAnnual(1) <= 100 Then RepsAnnual(1) = 100
                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt1 = RepsAnnual(1) : SS1 = OverlayLife
                    'Loop While (gLife_target > OverlayLife) ' Or (Math.Abs(OverlayLife - gLife_target) < LDiff)
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (gLife_target < OverlayLife)
            Else
                Do
                    tt1 = RepsAnnual(1) : SS1 = OverlayLife

                    If OverlayLife / gLife_target > 100 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(15)
                    ElseIf OverlayLife / gLife_target > 10 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(7)
                    ElseIf OverlayLife / gLife_target > 3.3 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(2)
                    ElseIf OverlayLife / gLife_target > 1.33 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.35)
                    ElseIf OverlayLife / gLife_target > 1.2 Then
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.2)
                    Else
                        RepsAnnual(1) = RepsAnnual(1) + RepsAnnual(1) * CSng(0.1)
                    End If

                    RepsAnnual(2) = RepsAnnual(1)
                    Call PCNLifeCalc()
                    tt2 = RepsAnnual(1) : SS2 = OverlayLife
                    'Loop While (gLife_target < OverlayLife) 'Or (Math.Abs(OverlayLife - gLife_target) < LDiff)
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (gLife_target > OverlayLife)
            End If
        Else
            Exit Sub
        End If


        If RepsAnnual(1) <= 2 Then
            If gLife_target > OverlayLife Then
                Exit Sub
            End If
        End If

        RepsAnnual(1) = CSng((tt1 + tt2) / 2)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gLife_target) + bb
        RepsAnnual(1) = CSng(ttACN)
        RepsAnnual(2) = RepsAnnual(1)
        Call PCNLifeCalc()

        If OverlayLife < gLife_target Then
            tt2 = RepsAnnual(1)
            SS2 = OverlayLife
        Else
            tt1 = RepsAnnual(1)
            SS1 = OverlayLife
        End If
        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            GoTo rep1
        End If

    End Sub





    Public Sub NewAdjustGrossWeightOverlay() 'ik2020.04

        Dim gLife_target As Single = gOverlayLife_target
        Dim LDiff As Single = 4.95 '0.055 '0.045 '0.025
        LDiff = gLife_target * CSng(gTolerance)

        Dim SS1, SS2 As Double
        Dim tt1, tt2 As Double

        Call PCNLifeCalc()

        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            If (OverlayLife > gLife_target) Then
                Do
                    tt1 = GL(1) : SS1 = OverlayLife
                    If OverlayLife / gLife_target > 1000 Then
                        GL(1) = CSng(GL(1) * 2)
                    ElseIf OverlayLife / gLife_target > 100 Then
                        GL(1) = CSng(GL(1) * 1.75)
                    ElseIf OverlayLife / gLife_target > 10 Then
                        GL(1) = CSng(GL(1) * 1.5)
                    ElseIf OverlayLife / gLife_target > 5 Then
                        GL(1) = CSng(GL(1) * 1.25)
                    ElseIf OverlayLife / gLife_target < 2 Then
                        GL(1) = CSng(GL(1) * 1.1)
                    ElseIf OverlayLife / gLife_target < 1.5 Then
                        GL(1) = CSng(GL(1) * 1.05)
                    Else
                        GL(1) = CSng(GL(1) * 1.03)
                    End If

                    If GL(1) <= 100 Then
                        GL(1) = 100
                        GL(2) = GL(1)
                        If SS1 = SS2 Then Exit Sub
                    End If

                    GL(2) = GL(1)
                    Call PCNLifeCalc()
                    tt2 = GL(1) : SS2 = OverlayLife
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (OverlayLife < gLife_target)
            Else
                Do
                    tt2 = GL(1) : SS2 = OverlayLife
                    If OverlayLife / gLife_target < 0.01 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.25)
                    ElseIf OverlayLife / gLife_target < 0.1 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.1)
                    ElseIf OverlayLife / gLife_target < 0.3 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.075)
                    ElseIf OverlayLife / gLife_target < 0.5 Then
                        GL(1) = GL(1) - GL(1) * CSng(0.045)
                    Else
                        GL(1) = GL(1) - GL(1) * CSng(0.02)
                    End If

                    GL(2) = GL(1)
                    Call PCNLifeCalc()
                    tt1 = GL(1) : SS1 = OverlayLife
                Loop Until (Math.Abs(OverlayLife - gLife_target) < LDiff) Or (OverlayLife > gLife_target)
            End If
        Else
            Exit Sub
        End If

        If (Math.Abs(OverlayLife - gLife_target) < LDiff) Then
            Exit Sub
        End If

        If GL(1) <= 2 Then
            If gLife_target > OverlayLife Then
                Exit Sub
            End If
        End If

        GL(1) = CSng((tt1 + tt2) / 2)
        GL(2) = GL(1)
        Call PCNLifeCalc()

        Dim zz1, zz2, aa, bb As Double
rep1:
        zz1 = Math.Log(SS1)
        zz2 = Math.Log(SS2)
        aa = (tt1 - tt2) / (zz1 - zz2)
        bb = tt1 - aa * zz1
        Dim ttACN As Double
        ttACN = aa * Math.Log(gLife_target) + bb
        GL(1) = CSng(ttACN)
        GL(2) = GL(1)
        Call PCNLifeCalc()

        If OverlayLife < gLife_target Then
            tt2 = GL(1)
            SS2 = OverlayLife
        Else
            tt1 = GL(1)
            SS1 = OverlayLife
        End If
        If Math.Abs(OverlayLife - gLife_target) > LDiff Then
            GoTo rep1
        End If

    End Sub




    Public Sub ABC_Reset_CDFdata()

        If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then

        Else
            Exit Sub
        End If



        Dim IA, IOFF As Integer

        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf
        For IA = 1 To NAC
            For IOFF = 1 To NOFF
                CDFdata(1, IA, IOFF) = 0
                CDFdata(1, NAC + 1, IOFF) = 0
            Next IOFF
        Next IA


        For IA = 1 To NAC
            For IOFF = 1 To NOFF
                'jobCDFtable(ISect, IA) = lclCDF(IA, IOFF) 'LeafCDFFlex
                CDFdata(1, IA, IOFF) = CSng(jobCDFtable(ISect, IA))
                CDFdata(1, NAC + 1, IOFF) = CDFdata(1, NAC + 1, IOFF) + CSng(jobCDFtable(ISect, IA))
            Next IOFF
        Next IA
        'cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf cdf

        Dim k1, k2, k3 As Integer

        For k1 = 1 To NAC + 1
            For k2 = 1 To 41
                CDFdata2(1, k1, k2) = CDFdata(1, k1, k2)
            Next
        Next
        'If LifeComputation Then
        '    LifeCounterForPCRRuns = LifeCounterForPCRRuns + 1
        '    If LifeCounterForPCRRuns = 1 Then
        '        For k1 = 1 To NAC + 1
        '            For k2 = 1 To 41
        '                CDFdata3(1, k1, k2) = CDFdata(1, k1, k2)
        '            Next
        '        Next
        '    End If
        'End If



    End Sub

    Public SavedgPtoC As Double()

    Public Sub AdjustGrossWeight2017()

        SavedgPtoC = gPtoC

        For I = 1 To 80 ' GFH 08/13/03.
            '       Save from last call to LeafCDFFlex for reporting in Aircraft Table.
            '       Could multiply final CDFs by (Design Life) / Life but for RepsAnnualInc.

            CDFtableTemp2(I) = jobCDFtable(ISect, I)
            CDFacrftMaxtableTemp2(I) = jobCDFacrftMaxtable(ISect, I)

        Next I

        'For k1 = 0 To 79
        '    For k2 = 1 To 41
        '        CDFdata3(1, k1, k2) = CDFdata2(1, k1, k2)
        '    Next
        'Next
        Try 'ik2021.05 AdjustGrossWeight2017

            If (DesignType = FlexOnRigid) Or OverlayRigOnRig Then
                gAdjustAnnOrGL = True  'ik2021.05
                Call NewAdjustGrossWeightOverlay() 'Review777 GL1
                gAdjustAnnOrGL = False  'ik2021.05

                If DesignType = FlexOnRigid Then
                    If AnalyzedasFlexible Then
                        lPavementType = ACRClassLib.clsACR.PavementType.Flexible
                    Else
                        lPavementType = ACRClassLib.clsACR.PavementType.Rigid
                    End If
                End If
            Else
                gAdjustAnnOrGL = True  'ik2021.05
                Call NewAdjustGrossWeight2() 'Review777 GL1
                gAdjustAnnOrGL = False  'ik2021.05
            End If

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try


    End Sub


    Public Sub Set_targets()

        If SectName = "RunW_A_F-1" Then '1
            CDFPic = 0.0000191146482
        ElseIf SectName = "RunW_B_F-2" Then '2
            CDFPic = 0.0008227169
        ElseIf SectName = "RunW_C_F-3" Then '3
            CDFPic = 0.000004627695
        ElseIf SectName = "RunW_D_F-4" Then '4
            CDFPic = 0.269525
        ElseIf SectName = "RunW_E_F-5" Then '5
            CDFPic = 78.88951
        ElseIf SectName = "RunW_F_F-6" Then '6
            CDFPic = 22.34727
        ElseIf SectName = "RunW_G_F-7" Then '7
            CDFPic = 0.000208526573
        ElseIf SectName = "RunW_H_F-8" Then '8
            CDFPic = 0.027195327
        ElseIf SectName = "RunW_I_R-1" Then '9
            CDFPic = 4195080.0
        ElseIf SectName = "RunW_J_R-2" Then '10
            CDFPic = 3.01121736
        End If



    End Sub

    Sub PCNLifeCalc(Optional ByRef ct As CancellationToken = Nothing)
        Dim J, I, IErr As Short
        Dim XC, YC As Single
        Dim TimeSave1, TimeSave2 As Integer



        'Call cmdLifePosition() 'frmSTRUC cmdAddDelete_Click frmStructure
        PCRRun = True
        FEDFAA1.FF_true_ACN = True
        StraightLineModel = -1
        LifeComputation = True


        Call CheckAdvisoryRequirements() 'frmSTRUC cmdAddDelete Click


        '    AddingDeletingLayer = True
        '    LayerSelected = False

        '    S = "  Select the layer to be" & NL
        '    S = S & "  added or deleted by" & NL
        '    S = S & "  clicking the mouse on" & NL
        '    S = S & "  the layer. The bottom" & NL
        '    S = S & "  layer cannot be" & NL
        '    S = S & "  selected."

        '    cmdModifyStr.Enabled = False
        '    cmdAddDelete.Enabled = False
        '    txtSelectLayer.Text = S
        '    txtSelectLayer.Visible = True
        '    cmdSelectLayerCancel.Visible = True
        '    I = NPLayers

        '    If DemoState = True Then
        '        Refresh()
        '        Call Delay()
        '        '     Select the layer above the bottom layer for demonstration.
        '        J = NPLayers - 1S
        '        YC = CSng(yBoxTop(J) + (yBoxBottom(J) - yBoxTop(J)) * 0.5)
        '        '     Just to put the coordinate in the picture.
        '        XC = CSng(xLeftMod + (xRightMod - xLeftMod) * 0.5)
        '        'Call picStructure_MouseDown(picStructure, New System.Windows.Forms.MouseEventArgs(CType((1 * &H100000), MouseButtons), 0, CInt(VB6.TwipsToPixelsX(XC)), CInt(VB6.TwipsToPixelsY(YC)), 0))
        '        Call picStructure_MouseDown(picStructure, New System.Windows.Forms.MouseEventArgs(CType((1 * &H100000), MouseButtons), 0, CInt(XC), CInt(YC), 0))
        '    Else
        '        Do
        '            System.Windows.Forms.Application.DoEvents() ' dlgAddDelete is called from picStructure.
        '        Loop Until LayerSelected = True ' Set in picStructure and cmdSelectLayerCancel.
        '    End If

        'LayerSelected = False
        'txtSelectLayer.Visible = False ' Set in picStructure and
        'cmdSelectLayerCancel.Visible = False ' cmdSelectLayerCancel so that the
        ''     Select layer message disappears before painting the form
        'cmdModifyStr.Enabled = True
        'cmdAddDelete.Enabled = True


        'If I <> NPLayers Then
        '        If DesignType = NewFlex Then
        '            IterLayerChosen = NPLayers - 1S
        '        ElseIf DesignType = NewRigid Or DesignType = PCCOnFlex Then
        '            IterLayerChosen = 1
        '        Else
        '            IterLayerChosen = 1
        '        End If

        '        'If LCode(IterLayerChosen) = 19 Then
        '        '    If NPLayers > 3 Then
        '        '        IterLayerChosen = IterLayerChosen + 1S
        '        '        If IterLayerChosen = NPLayers Then
        '        '            IterLayerChosen = 1
        '        '        End If
        '        '    Else
        '        '        IterLayerChosen = 1
        '        '    End If

        '        'End If

        '    End If
        '  Call DrawStructure()

        'Else




        ReDim CalcStress(NAC * 2) 'ikawa 001
        ReDim gCDF(NAC * 2)


        If NAC = 0 Then
            S = "There are no airplane in" & NL
            S = S & "job " & JobName & " section " & SectName & "."
            Ret = MsgBoxDQ(S, 0, "Designing Structure")
            Exit Sub
        End If

        DesigningStr = True




        If Not LifeComputation Then Designed = NullDate
        If Not LifeComputation Then SMin = CStr(NullDate) 'frmSTRUC cmdAddDelete
        If Not LifeComputation Then CompactionDesigned(ISect, IJob) = NullDate



        'If Not BatchSect Then ' GFH 04/23/03.
        '    StartingtmrElapsed = True
        '    Call tmrElapsedSet(True)
        '    ETimeMessage$ = "ETimeStart"
        'End If

        'System.Windows.Forms.Application.DoEvents()

        CDFAsp = 0.0!
        CDFPic = 0.0!
        ILayer = 0
        NStrIterations = 0

        For I = 1 To NAC ' Reset CDF and P/C for all aircraft. GFH 08/14/03.
            jobCDFtable(ISect, I) = 0
            jobCDFacrftMaxtable(ISect, I) = 0
            jobCtoPtable(ISect, I) = 0
        Next I


        Call SetDesignType()

        'DesignType = NewFlex

        If ct.IsCancellationRequested Then
            Debug.WriteLine("Task cancelled in PCNLifeCalc")
            ct.ThrowIfCancellationRequested()
        End If

        If DesignType = NewFlex Then 'cmdAddDelete Life Design

            If Not NoACCDF Then : HMA_CDF_Calc = True : End If
            '     Highlight design layer in picture only for design.
            If Not LifeComputation Then : ILayer = IterLayerChosen : End If

            Dim AutoDesign As Boolean
            If Not LifeComputation Then
                AutoDesign = AutomaticDesign()
            End If

            If Not AutoDesign Then
                'old design like in LEDFAA 1.3

                IterLayerChosen = 3
                DesigningP209DrawStructure = True
                Call LeafDesignFlex()
            End If

            'Call gSaveCDFdata() 'DesignType = NewFlex
            Call WriteToFileCtoPsub("NewFlex")
            'Call PrintResultsPCN01("222222_After_Life.txt") 'review999

        ElseIf DesignType = FlexOnFlex Then

            If Not NoACCDF Then
                HMA_CDF_Calc = True
            End If


            If Not LifeComputation Then ILayer = 1
            If gUserInterrupted Then GoTo UserInterrupted
            Call LeafDesignFlexOFlex()


            Call WriteToFileCtoPsub("FlexOnFlex")

        ElseIf DesignType = NewRigid Then

            Modulus(1) = DefaultModulus(5) 'ikawa Changing E modulus for PCC layer
            '************************************
            'Call Edward_Input() 'to comment
            '************************************
            gHorStressNIKE3D = Nothing 'ikawa 2017.08.15
            gHorStressLEAF = Nothing 'ikawa 2017.08.15


            If Not LifeComputation Then ILayer = 1
            ' Call DrawStructure() : Refresh()

            If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                Call pre_DesignRigid_NP() 'LEAF
                Call SelectACwithCDF005()
                DesigningStr = True : LEDFAA13Thick = Thick(1)
            End If

            If gUserInterrupted Then GoTo UserInterrupted

            If gOnly_LEAF Then
                Call pre_DesignRigid_NP() 'LEAF
            Else
                'Call pre_DesignRigid_NP() 'LEAF
                'Call DesignRigid_NP() '3D-FEM stress   
                Call DesignRigid_NP(ct)

            End If

            'btnGraphPCN.Text = CStr(Math.Round(CDFPic, 4)) 'review999

            '************************************
            'Call Edward_Output() 'to comment
            '************************************
            Call WriteToFileCtoPsub("NewRigid")

        ElseIf DesignType = PCCOnFlex Then

            gHorStressNIKE3D = Nothing 'ikawa 2017.08.15
            gHorStressLEAF = Nothing 'ikawa 2017.08.15

            If Not LifeComputation Then ILayer = 1
            'Call DrawStructure() : Refresh()
            'System.Diagnostics.Debug.WriteLine("PCC Overlay on Flexible" & DesignType)

            If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                Call pre_DesignRigid_NP() 'LEAF
                Call SelectACwithCDF005()
                DesigningStr = True : LEDFAA13Thick = Thick(1)
            End If

            If gUserInterrupted Then GoTo UserInterrupted

            If gOnly_LEAF Then
                Call pre_DesignRigid_NP() 'LEAF
            Else
                Call DesignRigid_NP(ct) '3D-FEM stress   
            End If

            Call WriteToFileCtoPsub("PCCOnFlex")


        ElseIf DesignType = UnbondOnRigid Or DesignType = PartBondOnRigid Then 'cmdAddDelete Life Design

            gHorStressNIKE3D = Nothing 'ikawa 2017.08.15
            gHorStressLEAF = Nothing 'ikawa 2017.08.15

            If Not LifeComputation Then ILayer = 1
            ' Call DrawStructure() : Refresh()

            If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                Call SelectACwithCDF005()
                DesigningStr = True : LEDFAA13Thick = Thick(1)
            End If

            If gUserInterrupted Then GoTo UserInterrupted

            'gOnly_LEAF = True 'DesignType = UnbondedOnRigid

            If gOnly_LEAF Then
                Call pre_DesignRigidOverlay_NP()  'LEAF stress
            Else
                Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
            End If


            Call WriteToFileCtoPsub("UnbondOnRig")


            'ElseIf OverlayRigOnRig Then 'cmdAddDelete Life Design
        ElseIf DesignType = FlexOnRigid Then 'cmdAddDelete Life Design

            gHorStressNIKE3D = Nothing 'ikawa 2017.08.15
            gHorStressLEAF = Nothing 'ikawa 2017.08.15

            '>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            HMAonRigidCase = 0 : AnalyzedasFlexible = False
            OverlayLife = 0 : LifeStr = 0

            If Not LifeComputation Then ILayer = 1
            ' Call DrawStructure() : Refresh()

            'GoTo newCalulations

            '+++++++++++++++++++++++++++++++++++++++++++
            If gHMAonRigid_Mod Then
                If LifeComputation Then

                    Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
                    'save cdf for AConRigid
                    Call copyArray1(jobCDFtable, jobCDFtableRigid)
                    Call copyArray1(jobCDFacrftMaxtable, jobCDFacrftMaxtableRigid)
                    Call copyArray1(jobCtoPtable, jobCtoPtableRigid)



                    If Thick(1) < Thick(2) Then
                        'nothing
                    Else 'calculate for NewFlexible

                        DesignType = FlexOnFlex
                        sLCode2 = LCode(2)
                        LCode(2) = 1
                        sModulus2 = Modulus(2)
                        Modulus(2) = CSng(Math.Round(4000000 * (0.02 + 0.0064 * SCIB + (0.00584 * SCIB) ^ 2)))
                        sRcon2 = RCon(2)
                        RCon(2) = 0.0
                        sThick1 = Thick(1)
                        sThick2 = Thick(2)
                        Call LeafDesignFlexOFlex()
                        'Call RestoreAConRigid()
                        RCon(2) = sRcon2
                        Modulus(2) = sModulus2
                        LCode(2) = sLCode2
                        DesignType = FlexOnRigid

                        If OverlayLife > LifeStr Then
                            'restore cdf for AConRigid
                            Call copyArray1(jobCDFtableRigid, jobCDFtable)
                            Call copyArray1(jobCDFacrftMaxtableRigid, jobCDFacrftMaxtable)
                            Call copyArray1(jobCtoPtableRigid, jobCtoPtable)

                        Else
                            OverlayLife = LifeStr
                            AnalyzedasFlexible = True
                        End If


                    End If


                Else 'Design =================================================
                    'Thick(1) = Thick(2) - CSng(0.01)
                    Thick(1) = Thick(2)

                    '  Call DrawStructure() : Refresh()
                    System.Windows.Forms.Application.DoEvents()

                    LifeComputation = True
                    Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
                    LifeComputation = False

                    If Math.Abs(OverlayLife - Life) < LifeError Then

                    ElseIf OverlayLife > Life Then

                        'pre-design
                        If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                            Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                            Call SelectACwithCDF005()
                            DesigningStr = True : LEDFAA13Thick = Thick(1)
                        End If

                        Call DesignRigidOverlay_NP(ct)  '3D-FEM stress

                    Else

                        'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                        'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                        'Flex life
                        'Thick(1) = Thick(2)
                        LifeComputation = True
                        DesignType = FlexOnFlex
                        Call LeafDesignFlexOFlex()
                        LifeComputation = False
                        ' Call RestoreAConRigid()
                        RCon(2) = sRcon2
                        Modulus(2) = sModulus2
                        LCode(2) = sLCode2
                        DesignType = FlexOnRigid

                        If OverlayLife > LifeStr Then
                            'only AConRigid
                            If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                                Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                                Call SelectACwithCDF005()
                                DesigningStr = True : LEDFAA13Thick = Thick(1)
                            End If

                            Call DesignRigidOverlay_NP(ct)  '3D-FEM stress

                        Else 'only Flex

                            If LifeStr >= Life Then
                                'Design done.
                                OverlayLife = LifeStr
                                HMAonRigidCase = 4
                            Else
                                DesignType = FlexOnFlex
                                Call LeafDesignFlexOFlex()
                                LifeStr = Life / CDFPic
                                HMAonRigidCase = 3
                                OverlayLife = LifeStr
                            End If

                            RCon(2) = sRcon2
                            Modulus(2) = sModulus2
                            LCode(2) = sLCode2
                            DesignType = FlexOnRigid
                            AnalyzedasFlexible = True

                        End If
                    End If
                End If

            Else ' ======= Old LifeComputation And Design =======

                'gOnly_LEAF = True 'temporary
                If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                    Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                    Call SelectACwithCDF005()
                    DesigningStr = True : LEDFAA13Thick = Thick(1)
                End If

                If gUserInterrupted Then GoTo UserInterrupted

                If gOnly_LEAF Then
                    Call pre_DesignRigidOverlay_NP()  'LEAF stress
                Else
                    Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
                End If
            End If

            '-----------------------------------------------

        Else ' *** end of DesignType
            Ret = MsgBoxDQ("The structure is not valid for design.", 0, "Designing a Structure")
            DesigningStr = False
            ' Call tmrElapsedSet(False)
        End If


        buttonCDFgraph = True





UserInterrupted:  'September 13, 2006

        ILayer = 0 ' Turn off highlight.

        'System.Windows.Forms.Application.DoEvents()

        'CleanupAfterDesign:
        'If (Not gUserInterrupted) And (Not LifeComputation) And IErr = 0 Then
        '    If DesigningStr = True Then Designed = Now
        'End If

        'added 2016.10.17
        Exit Sub
        PCRRun = True
        FEDFAA1.FF_true_ACN = True
        StraightLineModel = -1
        LifeComputation = True
        If DesignType = NewFlex Then
            Call LeafDesignFlex()
        End If

        If DesignType = NewRigid Then
            gOnly_LEAF = False
            If gOnly_LEAF Then
                Call pre_DesignRigid_NP() 'LEAF
            Else
                'Call pre_DesignRigid_NP() 'LEAF
                Call DesignRigid_NP(ct) '3D-FEM stress   
            End If
        End If

        If DesignType = FlexOnFlex Then

            IterLayerChosen = 1
            If Not NoACCDF Then
                HMA_CDF_Calc = True
            End If
            StraightLineModel = -1
            If Not LifeComputation Then ILayer = 1
            If gUserInterrupted Then Exit Sub
            Call LeafDesignFlexOFlex()
        End If

        If DesignType = FlexOnRigid Then
            Dim sLCode2 As Short
            Dim sThick1, sThick2, sRcon2, sModulus2 As Single
            StraightLineModel = -1
            IterLayerChosen = 1
            HMAonRigidCase = 0
            If Not LifeComputation Then ILayer = 1
            gHMAonRigid_Mod = True

            If gHMAonRigid_Mod Then 'FlexOnRigid module
                'begin Life ac-on-flex
                'LCode(1) = 10 is an asphalt overlay
                'If AC is thicker than PCC and Life
                If LCode(1) = 10 And Thick(1) >= Thick(2) And LifeComputation Then 'AC Overlay
                    DesignType = FlexOnFlex
                    sLCode2 = LCode(2)
                    LCode(2) = 1
                    sModulus2 = FEDFAA1.Modulus(2)

                    FEDFAA1.Modulus(2) = CSng(Math.Round(FEDFAA1.Modulus(2) * (0.02 + 0.0064 * SCIB + (0.00584 * SCIB) ^ 2)))

                    sRcon2 = RCon(2)
                    RCon(2) = 0.0
                    sThick1 = Thick(1)
                    sThick2 = Thick(2)

                    Call LeafDesignFlexOFlex()
                    Thick(1) = sThick1
                    Thick(2) = sThick2
                    RCon(2) = sRcon2
                    FEDFAA1.Modulus(2) = sModulus2
                    LCode(2) = sLCode2
                    DesignType = FlexOnRigid
                End If
                'end Life ac-on-flex

                'begin kairat 6
                If LCode(1) = 10 And Not LifeComputation Then 'AC Overlay
                    DesignType = FlexOnFlex
                    'Dim ttresh As Single

                    sLCode2 = LCode(2)
                    LCode(2) = 1
                    sModulus2 = FEDFAA1.Modulus(2)
                    FEDFAA1.Modulus(2) = CSng(Math.Round(FEDFAA1.Modulus(2) * (0.02 + 0.0064 * SCIB + (0.00584 * SCIB) ^ 2)))

                    sRcon2 = RCon(2)
                    RCon(2) = 0.0
                    sThick1 = Thick(1)
                    sThick2 = Thick(2)
                    Call LeafDesignFlexOFlex_light()
                    ttresh = Thick(1)
                    Thick(1) = sThick1
                    Thick(2) = sThick2
                    RCon(2) = sRcon2
                    FEDFAA1.Modulus(2) = sModulus2
                    LCode(2) = sLCode2
                    'ttresh = System.Math.Max(Thick(1), Thick(2))
                    'MsgBox(ttresh, MsgBoxStyle.OkOnly, "abc")


                    DesignType = FlexOnRigid
                End If
                Thick(1) = Thick(2) - CSng(0.01)

                ' Call DrawStructure() : Refresh()
                'System.Windows.Forms.Application.DoEvents()

                LifeComputation = True
                Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
                LifeComputation = False

                If Math.Abs(OverlayLife - Life) < LifeError Then

                ElseIf OverlayLife > Life Then

                    'pre-design
                    If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                        Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                        Call SelectACwithCDF005()
                        DesigningStr = True : LEDFAA13Thick = Thick(1)
                    End If

                    Call DesignRigidOverlay_NP(ct)  '3D-FEM stress

                Else

                    'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
                    'XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

                    'Flex life
                    'Thick(1) = Thick(2)
                    LifeComputation = True
                    ' Call SetAsFlexOnFlex()
                    Call LeafDesignFlexOFlex()
                    LifeComputation = False
                    'Call RestoreAConRigid()

                    If OverlayLife > LifeStr Then
                        'only AConRigid
                        If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                            Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                            Call SelectACwithCDF005()
                            DesigningStr = True : LEDFAA13Thick = Thick(1)
                        End If

                        Call DesignRigidOverlay_NP(ct)  '3D-FEM stress

                    Else 'only Flex

                        If LifeStr >= Life Then
                            'Design done.
                            OverlayLife = LifeStr
                            HMAonRigidCase = 4
                        Else
                            ' Call SetAsFlexOnFlex()
                            Call LeafDesignFlexOFlex()
                            LifeStr = Life / CDFPic
                            HMAonRigidCase = 3
                            OverlayLife = LifeStr
                        End If

                        ' Call RestoreAConRigid()
                        AnalyzedasFlexible = True

                    End If
                End If

                'end kairat

            Else
                'gOnly_LEAF = True 'temporary

                If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                    Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                    Call SelectACwithCDF005()
                    DesigningStr = True : LEDFAA13Thick = Thick(1)

                    If Thick(1) > Thick(2) Then
                        LEDFAA13Thick = Thick(2)
                    End If
                End If
            End If

            'gOnly_LEAF = True  'for FAARFIELD 1.41, this var was set to False
            'If gOnly_LEAF Then
            '    Call pre_DesignRigidOverlay_NP()  'LEAF stress
            'Else
            '    Call DesignRigidOverlay_NP()  '3D-FEM stress
            'End If

            'kairat 10/15/15
            If gHMAonRigid_Mod Then 'FlexOnRigid module
                'begin Life ac-on-flex 1
                'If LCode(1) = 10 And Thick(1) = Thick(2) Then 'AC Overlay
                If LCode(1) = 10 And Thick(1) = Thick(2) And Not LifeComputation Then 'AC Overlay
                    'LifeComputation = True
                    DesignType = FlexOnFlex
                    Dim sjobCDFtable(MaxSects, MaxSectAC) As Double ' to display in a/c table in frmLoadFile, JIA 08/04/03.
                    Dim sjobCDFacrftMaxtable(MaxSects, MaxSectAC) As Double ' Same, except max for one aircraft, GFH.

                    LifeComputation = True
                    sLCode2 = LCode(2)
                    LCode(2) = 1
                    sModulus2 = FEDFAA1.Modulus(2)
                    FEDFAA1.Modulus(2) = CSng(Math.Round(FEDFAA1.Modulus(2) * (0.02 + 0.0064 * SCIB + (0.00584 * SCIB) ^ 2)))
                    sRcon2 = RCon(2)
                    RCon(2) = 0.0
                    sThick1 = Thick(1)
                    sThick2 = Thick(2)
                    For I = 1 To MaxSects
                        For J = 1 To MaxSectAC
                            sjobCDFtable(I, J) = jobCDFtable(I, J)
                            sjobCDFacrftMaxtable(I, J) = jobCDFacrftMaxtable(I, J)
                            'jobCtoPtableSub(I, J) = jobCtoPtable(I, J)
                        Next J
                    Next I
                    Call LeafDesignFlexOFlex()
                    'MsgBox(LifeStr, MsgBoxStyle.OkOnly, "abc")

                    For I = 1 To MaxSects
                        For J = 1 To MaxSectAC
                            jobCDFtable(I, J) = sjobCDFtable(I, J)
                            jobCDFacrftMaxtable(I, J) = sjobCDFacrftMaxtable(I, J)
                            'jobCtoPtableSub(I, J) = jobCtoPtable(I, J)
                        Next J
                    Next I

                    RCon(2) = sRcon2
                    FEDFAA1.Modulus(2) = sModulus2
                    LCode(2) = sLCode2
                    DesignType = FlexOnRigid

                    'LifeComputation = False

                    Dim pickOL As Single 'kairat 7
                    If LifeStr >= 20 And OverlayLife >= 20 Then pickOL = System.Math.Min(LifeStr, OverlayLife)
                    If LifeStr >= 20 And OverlayLife < 20 Then pickOL = LifeStr
                    If LifeStr < 20 And OverlayLife >= 20 Then pickOL = OverlayLife
                    If LifeStr < 20 And OverlayLife < 20 Then pickOL = System.Math.Max(LifeStr, OverlayLife)
                    OverlayLife = pickOL

                End If
                'end Life ac-on-flex 1
            End If
            'end kairat 10/15/15

            'begin Life ac-on-flex 2
            'If LifeComputation And Thick(1) >= Thick(2) And LCode(1) = 10 Then
            If gHMAonRigid_Mod And LifeComputation And Thick(1) >= Thick(2) And LCode(1) = 10 Then
                Dim pickOL As Single 'kairat 7
                'If LifeStr >= 20 And OverlayLife >= 20 Then pickOL = System.Math.Min(LifeStr, OverlayLife)
                If LifeStr >= 20 And OverlayLife >= 20 Then pickOL = System.Math.Min(LifeStr, OverlayLife)
                If LifeStr >= 20 And OverlayLife < 20 Then
                    If Thick(1) = Thick(2) Then
                        pickOL = 20
                    Else
                        pickOL = LifeStr
                    End If
                    'pickOL = LifeStr
                End If

                If LifeStr < 20 And OverlayLife >= 20 Then pickOL = OverlayLife
                If LifeStr < 20 And OverlayLife < 20 Then pickOL = System.Math.Max(LifeStr, OverlayLife)
                OverlayLife = pickOL
                LifeStr = pickOL
            Else
                LifeStr = OverlayLife
            End If
            'end Life ac-on-flex 2

            If LifeComputation And LCode(1) = 10 Then  ' Kairat
                If OverlayLife = LifeStr Then  ' Kairat
                    AnalyzedasFlexible = True
                Else
                    AnalyzedasFlexible = False
                End If
            End If

        End If

        If DesignType = PCCOnFlex Then
            IterLayerChosen = 1
            If Not LifeComputation Then ILayer = 1

            Debug.WriteLine("PCC Overlay on Flexible" & DesignType)

            If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                Call pre_DesignRigid_NP() 'LEAF
                Call SelectACwithCDF005()
                DesigningStr = True : LEDFAA13Thick = Thick(1)
            End If
            gOnly_LEAF = False
            If gOnly_LEAF Then
                Call pre_DesignRigid_NP() 'LEAF
            Else
                Call DesignRigid_NP(ct) '3D-FEM stress   
            End If
        End If

        If DesignType = UnbondOnRigid Then
            gOnly_LEAF = False
            If Not LifeComputation Then ILayer = 1

            If Not LifeComputation And gUseLEAF_preDesign_forRigid Then
                Call pre_DesignRigidOverlay_NP()   'LEAF only stress
                Call SelectACwithCDF005()
                DesigningStr = True : LEDFAA13Thick = Thick(1)
            End If

            'gOnly_LEAF 'DesignType = UnbondedOnRigid

            If gOnly_LEAF Then
                Call pre_DesignRigidOverlay_NP()  'LEAF stress
            Else
                Call DesignRigidOverlay_NP(ct)  '3D-FEM stress
            End If
            If LifeComputation Then
                LifeStr = OverlayLife
            End If
        End If

    End Sub
    Public Sub copyArray1(ByRef CopyFrom1(,) As Double, ByRef CopyTo1(,) As Double) 'ikawa
        Dim ub1 As Integer

        ub1 = UBound(CopyFrom1, 2)
        For i As Integer = 1 To ub1
            CopyTo1(ISect, i) = CopyFrom1(ISect, i)
        Next

    End Sub
    Sub AirportMasterRecordData()
        Try

            'Me.Text = "Airport Master Record for Section " & gGraphSection & " in Job " & gGraphJob

            gPCNmax = gPCN_report

            If lPavementType = ACRClassLib.clsACR.PavementType.Flexible Then
                sPaveType = "F"
                sTireType = "X"
            Else
                sPaveType = "R"
                sTireType = "W"
            End If

            Dim modul1 As Single
            modul1 = Modulus(NPLayers)
            ICAOCodeIndex = ICAOCodeIndexF(modul1)
            ICAOCode(0) = "D"
            ICAOCode(1) = "C"
            ICAOCode(2) = "B"
            ICAOCode(3) = "A"

            SubgradeCategory = CStr(ICAOCode(ICAOCodeIndex - 1)).Substring(0, 1)

            'gPCN_Field39 = Format(gPCNmax, "N1") & "/" & sPaveType & "/" & SubgradeCategory & "/" & sTireType & "/T"

            'Round to nearest 10 for Airport Master Record
            gPCN_Field39 = Math.Round(gPCNmax / 10) * 10 & "/" & sPaveType & "/" & SubgradeCategory & "/" & sTireType & "/T"

            Call InitArrays()


            If lPavementType = ACRClassLib.clsACR.PavementType.Flexible Then

                If SubgradeCategory = "A" Then

                    s_MGW = MGW_result(A200Flex_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(A200Flex_D, gPCNmax, D)
                    dd_MGW = MGW_result(A200Flex_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(A200Flex_DD_2D2, gPCNmax, DD_2D2)

                ElseIf SubgradeCategory = "B" Then

                    s_MGW = MGW_result(B120Flex_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(B120Flex_D, gPCNmax, D)
                    dd_MGW = MGW_result(B120Flex_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(B120Flex_DD_2D2, gPCNmax, DD_2D2)


                ElseIf SubgradeCategory = "C" Then

                    s_MGW = MGW_result(C80Flex_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(C80Flex_D, gPCNmax, D)
                    dd_MGW = MGW_result(C80Flex_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(C80Flex_DD_2D2, gPCNmax, DD_2D2)


                ElseIf SubgradeCategory = "D" Then

                    s_MGW = MGW_result(D50Flex_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(D50Flex_D, gPCNmax, D)
                    dd_MGW = MGW_result(D50Flex_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(D50Flex_DD_2D2, gPCNmax, DD_2D2)

                End If

            Else 'Rigid

                If SubgradeCategory = "A" Then

                    s_MGW = MGW_result(A200Rigid_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(A200Rigid_D, gPCNmax, D)
                    dd_MGW = MGW_result(A200Rigid_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(A200Rigid_DD_2D2, gPCNmax, DD_2D2)

                ElseIf SubgradeCategory = "B" Then

                    s_MGW = MGW_result(B120Rigid_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(B120Rigid_D, gPCNmax, D)
                    dd_MGW = MGW_result(B120Rigid_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(B120Rigid_DD_2D2, gPCNmax, DD_2D2)

                ElseIf SubgradeCategory = "C" Then

                    s_MGW = MGW_result(C80Rigid_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(C80Rigid_D, gPCNmax, D)
                    dd_MGW = MGW_result(C80Rigid_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(C80Rigid_DD_2D2, gPCNmax, DD_2D2)


                ElseIf SubgradeCategory = "D" Then

                    s_MGW = MGW_result(D50Rigid_S, gPCNmax, SSS123)
                    d_MGW = MGW_result(D50Rigid_D, gPCNmax, D)
                    dd_MGW = MGW_result(D50Rigid_DD, gPCNmax, DD)
                    dd_2d2_MGW = MGW_result(D50Rigid_DD_2D2, gPCNmax, DD_2D2)

                End If

            End If

            'MGW_S.Text = Format(s_MGW / 1000, "##,##0")
            'MGW_D.Text = Format(d_MGW / 1000, "##,##0")
            'MGW_2D.Text = Format(dd_MGW / 1000, "##,##0")
            'MGW_2D2.Text = Format(dd_2d2_MGW / 1000, "##,##0")
            ''MGW_DDD.Text = Format(ddd_MGW / 1000, "##,##0")

            ''MGW_.Text = s_MGW
            'PCN.Text = gPCN_Field39


            If lPavementType = ACRClassLib.clsACR.PavementType.Flexible Then

                If SubgradeCategory = "A" Then

                    If gPCNmax < 30 Then s_MGW = 0
                    If gPCNmax < 40 Then d_MGW = 0
                    If gPCNmax < 90 Then dd_MGW = 0
                    If gPCNmax < 400 Then dd_2d2_MGW = 0

                ElseIf SubgradeCategory = "B" Then

                    If gPCNmax < 30 Then s_MGW = 0
                    If gPCNmax < 60 Then d_MGW = 0
                    If gPCNmax < 110 Then dd_MGW = 0
                    If gPCNmax < 400 Then dd_2d2_MGW = 0

                    'If gPCNmax < 30 Then MGW_S.Text = ""
                    'If gPCNmax < 60 Then MGW_D.Text = ""
                    'If gPCNmax < 110 Then MGW_2D.Text = ""
                    'If gPCNmax < 400 Then MGW_2D2.Text = ""

                ElseIf SubgradeCategory = "C" Then

                    If gPCNmax < 30 Then s_MGW = 0
                    If gPCNmax < 80 Then d_MGW = 0
                    If gPCNmax < 130 Then dd_MGW = 0
                    If gPCNmax < 400 Then dd_2d2_MGW = 0



                ElseIf SubgradeCategory = "D" Then

                    If gPCNmax < 30 Then s_MGW = 0
                    If gPCNmax < 90 Then d_MGW = 0
                    If gPCNmax < 160 Then dd_MGW = 0
                    If gPCNmax < 400 Then dd_2d2_MGW = 0

                End If

            Else 'Rigid


                If SubgradeCategory = "A" Then

                    If gPCNmax < 60 Then d_MGW = 0
                    If gPCNmax < 100 Then dd_MGW = 0
                    If gPCNmax < 400 Then dd_2d2_MGW = 0


                ElseIf SubgradeCategory = "B" Then

                    If gPCNmax < 70 Then d_MGW = 0
                    If gPCNmax < 110 Then dd_MGW = 0
                    If gPCNmax < 450 Then dd_2d2_MGW = 0

                ElseIf SubgradeCategory = "C" Then

                    If gPCNmax < 80 Then d_MGW = 0
                    If gPCNmax < 130 Then dd_MGW = 0
                    If gPCNmax < 490 Then dd_2d2_MGW = 0

                ElseIf SubgradeCategory = "D" Then

                    If gPCNmax < 90 Then d_MGW = 0
                    If gPCNmax < 150 Then dd_MGW = 0
                    If gPCNmax < 550 Then dd_2d2_MGW = 0

                End If

            End If

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub

    Function MGW_result(ByRef as1() As Single, ByVal PCNval As Single, ByRef weigh1() As Single) As Single

        Dim PCN1, PCN2 As Single
        Dim W1, W2 As Single

        Dim iCount As Integer
        Dim U1 As Integer

        U1 = UBound(as1, 1)

        For iCount = 1 To U1
            If PCNval <= as1(iCount) Then
                Exit For
            End If
        Next


        If iCount = 1 Then
            PCN1 = as1(iCount - 1)
            PCN2 = as1(iCount)

            W1 = weigh1(iCount - 1)
            W2 = weigh1(iCount)

        ElseIf iCount = U1 + 1 Then
            'PCN1 = as1(iCount + 1)
            'PCN2 = as1(iCount)

            'W1 = weigh1(iCount - 1)
            'W2 = weigh1(iCount)

            MGW_result = weigh1(U1)
            Exit Function
        Else
            PCN1 = as1(iCount - 1)
            PCN2 = as1(iCount)

            W1 = weigh1(iCount - 1)
            W2 = weigh1(iCount)

        End If

        MGW_result = (PCNval - PCN1) / (PCN2 - PCN1) * (W2 - W1) + W1

    End Function

    Sub InitArrays()


        Try

            'Flexible Pavements
            A200Flex_S(1) = 18.9 : B120Flex_S(1) = 20.4 : C80Flex_S(1) = 22.8 : D50Flex_S(1) = 26.6
            A200Flex_S(2) = 29.9 : B120Flex_S(2) = 41.9 : C80Flex_S(2) = 49.6 : D50Flex_S(2) = 54.9
            A200Flex_S(3) = 70.2 : B120Flex_S(3) = 95 : C80Flex_S(3) = 105.9 : D50Flex_S(3) = 113.8
            A200Flex_S(4) = 125.9 : B120Flex_S(4) = 153.9 : C80Flex_S(4) = 166.4 : D50Flex_S(4) = 175.2
            A200Flex_S(5) = 188.2 : B120Flex_S(5) = 216.8 : C80Flex_S(5) = 229.6 : D50Flex_S(5) = 238.5
            A200Flex_S(6) = 255.2 : B120Flex_S(6) = 282.6 : C80Flex_S(6) = 294.7 : D50Flex_S(6) = 303
            A200Flex_S(7) = 325.4 : B120Flex_S(7) = 350.3 : C80Flex_S(7) = 361.1 : D50Flex_S(7) = 368.5
            A200Flex_S(8) = 398 : B120Flex_S(8) = 419.4 : C80Flex_S(8) = 428.3 : D50Flex_S(8) = 434.9
            A200Flex_S(9) = 472.3 : B120Flex_S(9) = 489.5 : C80Flex_S(9) = 496.4 : D50Flex_S(9) = 502.2

            A200Flex_D(1) = 34.7 : B120Flex_D(1) = 57.6 : C80Flex_D(1) = 71.5 : D50Flex_D(1) = 88.2
            A200Flex_D(2) = 63 : B120Flex_D(2) = 89.2 : C80Flex_D(2) = 108 : D50Flex_D(2) = 131.9
            A200Flex_D(3) = 128.4 : B120Flex_D(3) = 160.5 : C80Flex_D(3) = 189.4 : D50Flex_D(3) = 230.9
            A200Flex_D(4) = 197.4 : B120Flex_D(4) = 231.4 : C80Flex_D(4) = 272.1 : D50Flex_D(4) = 320.9
            A200Flex_D(5) = 252.9 : B120Flex_D(5) = 294.9 : C80Flex_D(5) = 340.7 : D50Flex_D(5) = 395.7
            A200Flex_D(6) = 307 : B120Flex_D(6) = 346.9 : C80Flex_D(6) = 396.8 : D50Flex_D(6) = 455.6
            A200Flex_D(7) = 375.2 : B120Flex_D(7) = 419 : C80Flex_D(7) = 471.3 : D50Flex_D(7) = 539.6
            A200Flex_D(8) = 442.9 : B120Flex_D(8) = 491.7 : C80Flex_D(8) = 544.3 : D50Flex_D(8) = 622.3
            A200Flex_D(9) = 511.7 : B120Flex_D(9) = 562 : C80Flex_D(9) = 616.3 : D50Flex_D(9) = 701.5
            A200Flex_D(10) = 580.9 : B120Flex_D(10) = 630 : C80Flex_D(10) = 690.9 : D50Flex_D(10) = 778.6

            A200Flex_DD(1) = 89.2 : B120Flex_DD(1) = 106.7 : C80Flex_DD(1) = 124.4 : D50Flex_DD(1) = 158
            A200Flex_DD(2) = 153.6 : B120Flex_DD(2) = 187.6 : C80Flex_DD(2) = 232.1 : D50Flex_DD(2) = 301
            A200Flex_DD(3) = 223.3 : B120Flex_DD(3) = 277.9 : C80Flex_DD(3) = 355.7 : D50Flex_DD(3) = 447.6
            A200Flex_DD(4) = 284.7 : B120Flex_DD(4) = 353.5 : C80Flex_DD(4) = 454.8 : D50Flex_DD(4) = 577.5
            A200Flex_DD(5) = 346.6 : B120Flex_DD(5) = 425.9 : C80Flex_DD(5) = 549.7 : D50Flex_DD(5) = 708.6
            A200Flex_DD(6) = 412.1 : B120Flex_DD(6) = 509.5 : C80Flex_DD(6) = 655.7 : D50Flex_DD(6) = 843.6
            A200Flex_DD(7) = 478 : B120Flex_DD(7) = 588.9 : C80Flex_DD(7) = 759.1 : D50Flex_DD(7) = 975.3
            A200Flex_DD(8) = 533.3 : B120Flex_DD(8) = 635.7 : C80Flex_DD(8) = 802.4 : D50Flex_DD(8) = 1061.7
            A200Flex_DD(9) = 588 : B120Flex_DD(9) = 678.2 : C80Flex_DD(9) = 833 : D50Flex_DD(9) = 1118.4
            A200Flex_DD(10) = 641 : B120Flex_DD(10) = 706.5 : C80Flex_DD(10) = 844.3 : D50Flex_DD(10) = 1118.2

            A200Flex_DD_2D2(1) = 351.5 : B120Flex_DD_2D2(1) = 368.6 : C80Flex_DD_2D2(1) = 401.6 : D50Flex_DD_2D2(1) = 490.5
            A200Flex_DD_2D2(2) = 449.3 : B120Flex_DD_2D2(2) = 480.8 : C80Flex_DD_2D2(2) = 550.4 : D50Flex_DD_2D2(2) = 727.7
            A200Flex_DD_2D2(3) = 553.4 : B120Flex_DD_2D2(3) = 610.1 : C80Flex_DD_2D2(3) = 721.9 : D50Flex_DD_2D2(3) = 1028.9
            A200Flex_DD_2D2(4) = 663.9 : B120Flex_DD_2D2(4) = 758.3 : C80Flex_DD_2D2(4) = 935.2 : D50Flex_DD_2D2(4) = 1395.2



            'Rigid Pavements
            A200Rigid_S(1) = 12.7 : B120Rigid_S(1) = 13.6 : C80Rigid_S(1) = 14.4 : D50Rigid_S(1) = 16.6
            A200Rigid_S(2) = 28.4 : B120Rigid_S(2) = 33.6 : C80Rigid_S(2) = 37.1 : D50Rigid_S(2) = 40.4
            A200Rigid_S(3) = 74.6 : B120Rigid_S(3) = 82.6 : C80Rigid_S(3) = 87.7 : D50Rigid_S(3) = 92.6
            A200Rigid_S(4) = 128.7 : B120Rigid_S(4) = 138.3 : C80Rigid_S(4) = 144.4 : D50Rigid_S(4) = 150.3
            A200Rigid_S(5) = 189 : B120Rigid_S(5) = 199.3 : C80Rigid_S(5) = 205.9 : D50Rigid_S(5) = 212
            A200Rigid_S(6) = 254 : B120Rigid_S(6) = 264.3 : C80Rigid_S(6) = 270.8 : D50Rigid_S(6) = 277.3
            A200Rigid_S(7) = 323 : B120Rigid_S(7) = 332.6 : C80Rigid_S(7) = 338.8 : D50Rigid_S(7) = 344.8
            A200Rigid_S(8) = 394.8 : B120Rigid_S(8) = 403.4 : C80Rigid_S(8) = 409 : D50Rigid_S(8) = 414.7
            A200Rigid_S(9) = 469.3 : B120Rigid_S(9) = 476.5 : C80Rigid_S(9) = 481.2 : D50Rigid_S(9) = 485.7

            A200Rigid_D(1) = 57 : B120Rigid_D(1) = 69.5 : C80Rigid_D(1) = 78.2 : D50Rigid_D(1) = 86.8
            A200Rigid_D(2) = 96.4 : B120Rigid_D(2) = 110.9 : C80Rigid_D(2) = 120.6 : D50Rigid_D(2) = 130.2
            A200Rigid_D(3) = 185.5 : B120Rigid_D(3) = 201.9 : C80Rigid_D(3) = 212.7 : D50Rigid_D(3) = 223.3
            A200Rigid_D(4) = 276.6 : B120Rigid_D(4) = 294.1 : C80Rigid_D(4) = 305.5 : D50Rigid_D(4) = 317
            A200Rigid_D(5) = 351.6 : B120Rigid_D(5) = 372 : C80Rigid_D(5) = 385.8 : D50Rigid_D(5) = 399.8
            A200Rigid_D(6) = 420.3 : B120Rigid_D(6) = 444 : C80Rigid_D(6) = 460 : D50Rigid_D(6) = 476.3
            A200Rigid_D(7) = 509 : B120Rigid_D(7) = 533.7 : C80Rigid_D(7) = 550.5 : D50Rigid_D(7) = 567.8
            A200Rigid_D(8) = 598.4 : B120Rigid_D(8) = 623.9 : C80Rigid_D(8) = 640.9 : D50Rigid_D(8) = 659.4
            A200Rigid_D(9) = 688.3 : B120Rigid_D(9) = 713.8 : C80Rigid_D(9) = 731.6 : D50Rigid_D(9) = 750.7
            A200Rigid_D(10) = 785.8 : B120Rigid_D(10) = 811.4 : C80Rigid_D(10) = 829.2 : D50Rigid_D(10) = 848.4

            A200Rigid_DD(1) = 98.4 : B120Rigid_DD(1) = 110.7 : C80Rigid_DD(1) = 126 : D50Rigid_DD(1) = 147.6
            A200Rigid_DD(2) = 177.6 : B120Rigid_DD(2) = 210.8 : C80Rigid_DD(2) = 240.9 : D50Rigid_DD(2) = 274.3
            A200Rigid_DD(3) = 274.9 : B120Rigid_DD(3) = 325.9 : C80Rigid_DD(3) = 365.7 : D50Rigid_DD(3) = 407.8
            A200Rigid_DD(4) = 361.6 : B120Rigid_DD(4) = 426.7 : C80Rigid_DD(4) = 475.6 : D50Rigid_DD(4) = 526.7
            A200Rigid_DD(5) = 449.7 : B120Rigid_DD(5) = 527.5 : C80Rigid_DD(5) = 585.4 : D50Rigid_DD(5) = 645.7
            A200Rigid_DD(6) = 547 : B120Rigid_DD(6) = 637.3 : C80Rigid_DD(6) = 703.1 : D50Rigid_DD(6) = 771.3
            A200Rigid_DD(7) = 641.6 : B120Rigid_DD(7) = 744 : C80Rigid_DD(7) = 817.9 : D50Rigid_DD(7) = 894.7
            A200Rigid_DD(8) = 711 : B120Rigid_DD(8) = 823.9 : C80Rigid_DD(8) = 906.6 : D50Rigid_DD(8) = 993.1
            A200Rigid_DD(9) = 767.6 : B120Rigid_DD(9) = 889.5 : C80Rigid_DD(9) = 981.2 : D50Rigid_DD(9) = 1077.5
            A200Rigid_DD(10) = 803.9 : B120Rigid_DD(10) = 930.8 : C80Rigid_DD(10) = 1030.3 : D50Rigid_DD(10) = 1137.4

            A200Rigid_DD_2D2(1) = 379 : B120Rigid_DD_2D2(1) = 437.3 : C80Rigid_DD_2D2(1) = 490.7 : D50Rigid_DD_2D2(1) = 553.8
            A200Rigid_DD_2D2(2) = 524.6 : B120Rigid_DD_2D2(2) = 610.9 : C80Rigid_DD_2D2(2) = 681.8 : D50Rigid_DD_2D2(2) = 760.9
            A200Rigid_DD_2D2(3) = 692.8 : B120Rigid_DD_2D2(3) = 804.3 : C80Rigid_DD_2D2(3) = 890 : D50Rigid_DD_2D2(3) = 982.4
            A200Rigid_DD_2D2(4) = 880.3 : B120Rigid_DD_2D2(4) = 1013.5 : C80Rigid_DD_2D2(4) = 1112.3 : D50Rigid_DD_2D2(4) = 1215.4


            SSS123(1) = 7500
            SSS123(2) = 15000
            SSS123(3) = 30000
            SSS123(4) = 45000
            SSS123(5) = 60000
            SSS123(6) = 75000
            SSS123(7) = 90000
            SSS123(8) = 105000
            SSS123(9) = 120000

            D(1) = 37500
            D(2) = 50000
            D(3) = 75000
            D(4) = 100000
            D(5) = 125000
            D(6) = 150000
            D(7) = 175000
            D(8) = 200000
            D(9) = 225000
            D(10) = 250000

            DD(1) = 100000
            DD(2) = 150000
            DD(3) = 200000
            DD(4) = 250000
            DD(5) = 300000
            DD(6) = 350000
            DD(7) = 400000
            DD(8) = 450000
            DD(9) = 500000
            DD(10) = 550000


            DD_2D2(1) = 640000
            DD_2D2(2) = 800000
            DD_2D2(3) = 960000
            DD_2D2(4) = 1120000

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try

    End Sub




    Public Sub CreateHTMLinfo(ByRef Info As String, ByRef PInfo As String, ByVal I As Integer, ByVal IncludeImage As Boolean)




        '======================================================================================================
        ' For display in the text box.
        'Info = "<html>" +
        '       "<head><title>FAARFIELD</title></head>" +
        '       "<body style=""font-size: 8pt; font-family: Arial"">" +
        '       "<b>" + frmStartup.Text + "</b><br><br>" +
        '       "Section " & SectName & " in Job " & JobName$ & ".<br>" +
        '       "Working directory is " & WorkingDir + "<br><br>"
        'Debug.WriteLine(frmStartup.Text)


        If Life <> DefaultLife Then
            Info = Info & NotDefaultLife & "<br>"
            'PInfo = PInfo & NotDefaultLife & NL2
        End If

        'If Not StandardStr Then 'CreateHTMLinfo
        '    Info = Info & NonStandardStr & "<br>"
        '    'PInfo = PInfo & NonStandardStr & NL2
        'End If

        If Not GoodACList Then
            Info = Info & BadACList & "<br>"
            'PInfo = PInfo & BadACList & NL2
        End If

        S = ""
        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
            If Designed <> NullDate Then
                If CDFAsp = -1 Then
                    S = " Asphalt CDF was not computed."
                Else
                    S = " Asphalt CDF = " & Format(CDFAsp, "0.0000") & "."
                End If
            End If
        End If
        SS = ""
        If DesignType = NewFlex Then 'CreateHTMLInfo
            SS = "New Flexible"
        ElseIf DesignType = FlexOnFlex Then
            SS = "AC Overlay on Flexible"
        ElseIf DesignType = NewRigid Then
            SS = "New Rigid"
        ElseIf DesignType = UnbondOnRigid Then
            SS = "Unbonded PCC Overlay on Rigid"
        ElseIf DesignType = PartBondOnRigid Then
            SS = "Part Bonded PCC Overlay on Rigid"
        ElseIf DesignType = PCCOnFlex Then
            SS = "PCC Overlay on Flexible"
        ElseIf DesignType = FlexOnRigid Then
            SS = "AC Overlay on Rigid"
        End If
        PaveStruct = SS

        Info = Info & "The structure is " & SS & "." & S & "<br>"

        S = ""
        If OverlayRigOnRig Then
            S = "SCI of the existing pavement = " & Format(SCIB, "0") & "." & "<br>"
            If SCIB = 100.0! Then
                S = S & "Percent CDF used of the existing pavement = " & Format(LifeExistPCC, "0") & "." & "<br>"
            End If
        End If

        'S = S & NL
        S = S & "Design Life = " & Format(Life, "0") & " years." & "<br>" ' GFH 04/23/03.

        'bool1 = False : bool2 = False : bool3 = False
        If SMin = "Min" Then 'jobSMin(ISect)
            S = S & "A design for this section was completed on " & Format(Designed, "mm/dd/yy") & " at " & Format(Designed, "hh:mm:ss") & "." & "<br>"
            S = S & "Minimum layer thicknesses were reached." & "<br>"
        ElseIf Designed <> NullDate Then
            S = S & "A design for this section was completed on " & Format(Designed, "mm/dd/yy") & " at " & Format(Designed, "hh:mm:ss") & "." & "<br>"
        Else
            S = S & "A design has not been completed for this section." & "<br>"
        End If

        If AnalyzedasFlexible = True Then S = S & "The structure is being analyzed as a flexible section" 'Kairat 

        If DesignType = FlexOnRigid And Designed <> NullDate Then
            If HMAonRigidCase = 1 Then

            ElseIf HMAonRigidCase = 2 Then
                S = S & "<br>Minimum HMA thickness was reached. Therefore, CDF totals may be less than 1.0. (b)<br><br>"
            ElseIf HMAonRigidCase = 3 Then
                S = S & "<br>Overlay thickness was determined using a flexible pavement design procedure," & "<br>"
                S = S & "considering the existing PCC as a high-stiffness base layer." & "<br>"
                S = S & "CDF is determined for the top of the subgrade. Refer to AC 150/5320-6," & "<br>"
                S = S & "paragraph 405, for additional information. (c)" & "<br><br>"
            ElseIf HMAonRigidCase = 4 Then
                S = S & "<br>Overlay thickness was determined using a flexible pavement design procedure," & "<br>"
                S = S & "considering the existing PCC as a high-stiffness base layer." & "<br>"
                S = S & "Minimum HMA thickness was reached. Therefore, CDF totals may be" & "<br>"
                S = S & "less than 1.0. CDF is determined for the top of the subgrade." & "<br>"
                S = S & "Refer to AC 150/5320-6, paragraph 405, for additional information. (d)" & "<br><br>"
            End If
        End If

        If DesignType = NewRigid Or DesignType = NewFlex Then 'CreateHTMLInfo
            If Designed <> NullDate And CompactionDesigned(ISect, IJob) <> NullDate Then 'CreateHTMLinfo
                S = S & "Compaction requirements for this section were computed on " &
                Format(CompactionDesigned(ISect, IJob), "mm/dd/yy") & " at " &
                Format(CompactionDesigned(ISect, IJob), "hh:mm:ss") & "." & "<br>"
            End If
        End If

        S = S & "<br>"

        S = S & "<b>" & "Pavement Structure Information by Layer, Top First</b>" & "<br><br>"

        Info = Info & S


        Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
        "  <tr>" +
            "<td  align=""center"">No. </td>" +
            "<td  align=""center"">Type </td>" +
            "<td  align=""center"">Thickness <br>" & UnitsOut.inchName & "</td>" +
            "<td  align=""center"">Modulus <br>" & UnitsOut.psiMPaName & "</td>" +
            "<td  align=""center"">Poisson's <br>Ratio</td>" +
            "<td  align=""center"">Strength<br>R," & UnitsOut.psiMPaName & "</td>" +
            "</tr>"


        For I = 0 To NLayerTypes - 1
            If Len(LayerTypePic(I)) > LenLayerType Then
                LenLayerType = Len(LayerTypePic(I))
            End If
        Next I

        ThicktoSubgrade = 0.0!

        structureinfo.length = NPLayers
        'PaveStr.length = NPLayers

        For I = 1 To NPLayers
            structureinfo.no(I - 1) = I.ToString
            If I < NPLayers Then ThicktoSubgrade = ThicktoSubgrade + Thick(I)
            LI = LCode(I)
            S = LayerType(LI)
            If S = NPCC Or S = NPCCOU Or S = NPCCOB Or S = NPCCOF Then
                S = LayerTypePic(LI) '"PCC"
            Else
                S = LayerTypePic(LI)
            End If

            structureinfo.type(I - 1) = S

            'struct_layertype = S
            'Info = Info & LPad(3, Format(I, "0")) & New String(" ", 2) & TabStr
            Info = Info & "<tr>" &
            "<td  align=""center"">" & I.ToString & " </td>" &
            "<td  align=""center"">" & S & "</td>" &
            "<td  align=""center"">" & Format(Thick(I) * UnitsOut.inch, UnitsOut.inchFormat) & "</td>" &
            "<td  align=""center"">" & Format(Modulus(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat) & "</td>"

            structureinfo.thickness(I - 1) = Format(Thick(I) * UnitsOut.inch, UnitsOut.inchFormat).ToString
            structureinfo.modulus(I - 1) = Format(Modulus(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat).ToString

            Temp = DefaultPoisson(LI)
            If I = NPLayers Then
                If RCon(1) > 0.0! Or RCon(2) > 0.0! Then
                    Temp = DefaultPoissonSGPCC
                Else
                    Temp = DefaultPoissonSGAC
                End If
                If LayerType(LI) = NND Then Temp = DefaultPoisson(LI)
            End If


            If LayerType(LI) = NND Then
                Temp = jobPoissonsRatio(ISect, I)
            End If
            structureinfo.poissonRatio(I - 1) = Temp.ToString("0.00")

            structureinfo.streagth(I - 1) = Format(RCon(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat).ToString

            Info = Info &
            "<td  align=""center"">" & Format(Temp, "0.00") & "</td>" &
            "<td  align=""center"">" & Format(RCon(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat) & "</td>" &
            "</tr>"
        Next I

        Info = Info & "</table>"

        TotalThickness = Format(ThicktoSubgrade * UnitsOut.inch, UnitsOut.inchFormat).ToString


        S = "Total thickness to the top of the subgrade = "
        S = S & Format(ThicktoSubgrade * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName
        Info = Info & "<br><b>" & S & "</b><br><br><br>"
        PInfo = PInfo & S

        Info = Info & "<b>Airplane Information </b><br><br>"
        Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
        "<tr>" +
            "<td  align=""center"">No. </td>" +
            "<td  align=""center"">Name</td>" +
            "<td  align=""center"">Gross Wt.<br>" & UnitsOut.poundsName & "</td>" +
            "<td  align=""center"">Annual<br>Departures</td>" +
            "<td  align=""center"">% Annual<br>Growth</td>" +
        "</tr>"



        For I = 1 To NAC
            'Info = Info & LPad(3, Format(I, "0")) & New String(" ", 3) & TabStr
            Info = Info & "<tr>" &
            "<td  align=""center"">" & Format(I, "0") & " </td>" +
            "<td  align=""center"">" & ACName(I) & "</td>" +
            "<td  align=""center"">" & Format(GL(I) * UnitsOut.pounds, UnitsOut.poundsFormat) & "</td>" &
            "<td  align=""center"">" & Format(RepsAnnual(I), "#,##0") & "</td>" &
            "<td  align=""center"">" & Format(RepsInc(I) * 100.0!, "0.00") & "</td>" &
            "</tr>"
        Next I


        Info = Info & "</table>"




        If ComputeAircraftCDF Then ' GFH 08/14/03.

            Info = Info & "<br><br><b>Additional Airplane Information</b><br><br>"

            If (DesignType = NewFlex) Or (DesignType = FlexOnFlex) Then
                'Info = Info & "<p class=""small"">Subgrade CDF</p>"
                Info = Info & "Subgrade CDF"
                'Info = Info & "<font size=""3""><br><br></font>"
            Else

            End If

            Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
            "<tr>" +
                "<td  align=""center"">No. </td>" +
                "<td  align=""center"">Name</td>" +
                "<td  align=""center"">CDF<br>Contribution</td>" +
                "<td  align=""center"">CDF Max<br>for Airplane</td>" +
                "<td  align=""center"">P/C<br>Ratio</td>" +
            "</tr>"


            For I = 1 To NAC
                Info = Info & "<tr>" &
                "<td  align=""center"">" & Format(I, "0") & " </td>" +
                "<td  align=""center"">" & ACName(I) & "</td>" +
                "<td  align=""center"">" & Format(jobCDFtable(ISect, I), "#,###,##0.00") & "</td>" &
                "<td  align=""center"">" & Format(jobCDFacrftMaxtable(ISect, I), "#,###,##0.00") & "</td>"


                DTemp = Math.Abs(jobCtoPtable(ISect, I))
                If DTemp <> 0 Then DTemp = 1 / DTemp
                If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
                Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
                "</tr>"

            Next I


            Info = Info & "</table>"


            'New HMA - CDF
            If (Not NoACCDF) And (DesignType = NewFlex) Then 'for HMA CDF
                Info = Info & "<br>HMA CDF"
                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
                "<tr>" +
                    "<td  align=""center"">No. </td>" +
                    "<td  align=""center"">Name</td>" +
                    "<td  align=""center"">CDF<br>Contribution</td>" +
                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
                    "<td  align=""center"">P/C<br>Ratio</td>" +
                "</tr>"


                For I = 1 To NAC
                    Info = Info & "<tr>" &
                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
                    "<td  align=""center"">" & ACName(I) & "</td>" +
                    "<td  align=""center"">" & Format(jobCDFtableHMA(ISect, I), "#,###,##0.00") & "</td>" &
                    "<td  align=""center"">" & Format(jobCDFacrftMaxtableHMA(ISect, I), "#,###,##0.00") & "</td>"


                    DTemp = Math.Abs(jobCtoPtableHMA(ISect, I))
                    If DTemp <> 0 Then DTemp = 1 / DTemp
                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
                    "</tr>"

                Next I

                Info = Info & "</table>"

            End If


            If (Not NoACCDF) And (DesignType = NewFlex And LCode(2) = 14) Then
                Info = Info & "<br>P-401/P-403 St (flex) CDF"
                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
                "<tr>" +
                    "<td  align=""center"">No. </td>" +
                    "<td  align=""center"">Name</td>" +
                    "<td  align=""center"">CDF<br>Contribution</td>" +
                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
                    "<td  align=""center"">P/C<br>Ratio</td>" +
                "</tr>"


                For I = 1 To NAC
                    Info = Info & "<tr>" &
                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
                    "<td  align=""center"">" & ACName(I) & "</td>" +
                    "<td  align=""center"">" & Format(jobCDFtable401(ISect, I), "#,###,##0.00") & "</td>" &
                    "<td  align=""center"">" & Format(jobCDFacrftMaxtable401(ISect, I), "#,###,##0.00") & "</td>"


                    DTemp = Math.Abs(jobCtoPtable401(ISect, I))
                    If DTemp <> 0 Then DTemp = 1 / DTemp
                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
                    "</tr>"

                Next I

                Info = Info & "</table>"

            End If


            'HMA Overlay over HMA - CDF
            If (Not NoACCDF) And (DesignType = FlexOnFlex) Then
                Info = Info & "<br>Overlay HMA CDF"
                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
                "<tr>" +
                    "<td  align=""center"">No. </td>" +
                    "<td  align=""center"">Name</td>" +
                    "<td  align=""center"">CDF<br>Contribution</td>" +
                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
                    "<td  align=""center"">P/C<br>Ratio</td>" +
                "</tr>"


                For I = 1 To NAC
                    Info = Info & "<tr>" &
                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
                    "<td  align=""center"">" & ACName(I) & "</td>" +
                    "<td  align=""center"">" & Format(jobCDFtableAC(ISect, I), "#,###,##0.00") & "</td>" &
                    "<td  align=""center"">" & Format(jobCDFacrftMaxtableAC(ISect, I), "#,###,##0.00") & "</td>"


                    DTemp = Math.Abs(jobCtoPtableAC(ISect, I))
                    If DTemp <> 0 Then DTemp = 1 / DTemp
                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
                    "</tr>"

                Next I

                Info = Info & "</table>"

                'HMA under HMA overlay
                Info = Info & "<br>HMA CDF"
                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
                "<tr>" +
                    "<td  align=""center"">No. </td>" +
                    "<td  align=""center"">Name</td>" +
                    "<td  align=""center"">CDF<br>Contribution</td>" +
                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
                    "<td  align=""center"">P/C<br>Ratio</td>" +
                "</tr>"

                For I = 1 To NAC
                    Info = Info & "<tr>" &
                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
                    "<td  align=""center"">" & ACName(I) & "</td>" +
                    "<td  align=""center"">" & Format(jobCDFtableHMA(ISect, I), "#,###,##0.00") & "</td>" &
                    "<td  align=""center"">" & Format(jobCDFacrftMaxtableHMA(ISect, I), "#,###,##0.00") & "</td>"


                    DTemp = Math.Abs(jobCtoPtableHMA(ISect, I))
                    If DTemp <> 0 Then DTemp = 1 / DTemp
                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
                    "</tr>"

                Next I

                Info = Info & "</table>"

                If LCode(3) = 14 Then
                    Info = Info & "<br>P-401/P-403 St (flex) CDF"
                    Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
                    "<tr>" +
                        "<td  align=""center"">No. </td>" +
                        "<td  align=""center"">Name</td>" +
                        "<td  align=""center"">CDF<br>Contribution</td>" +
                        "<td  align=""center"">CDF Max<br>for Airplane</td>" +
                        "<td  align=""center"">P/C<br>Ratio</td>" +
                    "</tr>"

                    For I = 1 To NAC
                        Info = Info & "<tr>" &
                        "<td  align=""center"">" & Format(I, "0") & " </td>" +
                        "<td  align=""center"">" & ACName(I) & "</td>" +
                        "<td  align=""center"">" & Format(jobCDFtable401(ISect, I), "#,###,##0.00") & "</td>" &
                        "<td  align=""center"">" & Format(jobCDFacrftMaxtable401(ISect, I), "#,###,##0.00") & "</td>"


                        DTemp = Math.Abs(jobCtoPtable401(ISect, I))
                        If DTemp <> 0 Then DTemp = 1 / DTemp
                        If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
                        Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
                        "</tr>"

                    Next I

                    Info = Info & "</table>"
                End If

            End If

            Info = Info & "</body></html>"
            'Info = Info & "</table></body></html>"

        End If




        If DesignType = NewRigid Or DesignType = NewFlex Then 'CreateHTMLinfo
        Else
            GoTo PassCompaction
        End If


        'Added compaction capability to FF1.4 based on FF1.313 082012 by YGC 021913
        'Added to output COmpaction Criteria in Notes by YGC 042312
        If Designed <> NullDate And CompactionDesigned(ISect, IJob) <> NullDate Then 'CreateHTMLinfo
            Info = Info & "<br><br><b>Subgrade Compaction Requirements</b><br><br>"

            Info = Info & "NonCohesive Soil"

            'implementing metric
            Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
                            "<tr>" +
                            "<td  align=""center"">Percent Maximum Dry Density(%) </td>" +
                            "<td  align=""center"">Depth of compaction <br> from pavement surface (" & UnitsOut.inchName & ") </td>" +
                            "<td  align=""center"">Depth of compaction <br> from top of subgrade (" & UnitsOut.inchName & ") </td>" +
                            "<td  align=""center"">Critical Airplane for Compaction </td>" +
                            "</tr>"



            'modify ended by YGC 102213 

            For J = 1 To NDenLevel
                If jobCompactionIntDenNCtable(ISect, IJob, J) >= DensityNCMin Then


                    'metrication
                    Info = Info & "<tr>" &
                        "<td  align=""center"">" & Format(jobCompactionIntDenNCtable(ISect, IJob, J), "0") & " </td>" +
                        "<td  align=""center"">" & Format(jobCompactionIntDenDepthNCtable(ISect, IJob, J - 1) * UnitsOut.inch, "0") & " - " & Format(jobCompactionIntDenDepthNCtable(ISect, IJob, J) * UnitsOut.inch, "0") & "</td>"
                    If jobCompactionIntDenDepthNCtable(ISect, IJob, J) <= ThicktoSubgrade Then
                        Info = Info & "<td  align=""center"">" & "--" & " </td>"
                    Else
                        Info = Info & "<td  align=""center"">" & Format(Math.Max(jobCompactionIntDenDepthNCtable(ISect, IJob, J - 1) - ThicktoSubgrade, 0) * UnitsOut.inch, "0") & " - " & Format((jobCompactionIntDenDepthNCtable(ISect, IJob, J) - ThicktoSubgrade) * UnitsOut.inch, "0") & "</td>"
                    End If
                    'Info = Info & "<td  align=""center"">" & CallAC(jobCompactionIntDenCriticalACNCtable(ISect, J)).ACname & "</td>"
                    Info = Info & "<td  align=""center"">" & ACName(jobCompactionIntDenCriticalACNCtable(ISect, IJob, J)) & "</td>"



                End If
            Next J

            Info = Info & "</table>"

            Info = Info & "<br>"

            Info = Info & "Cohesive Soil"


            'implementing metric
            Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
                        "<tr>" +
                        "<td  align=""center"">Percent Maximum Dry Density(%) </td>" +
                        "<td  align=""center"">Depth of compaction <br> from pavement surface (" & UnitsOut.inchName & ") </td>" +
                        "<td  align=""center"">Depth of compaction <br> from top of subgrade (" & UnitsOut.inchName & ") </td>" +
                        "<td  align=""center"">Critical Airplane for Compaction </td>" +
                        "</tr>"
            'modify ended by YGC 102213 

            For J = 1 To NDenLevel
                If jobCompactionIntDenCtable(ISect, IJob, J) >= DensityCMin Then

                    'metrication    * UnitsOut.inch
                    Info = Info & "<tr>" &
                        "<td  align=""center"">" & Format(jobCompactionIntDenCtable(ISect, IJob, J), "0") & " </td>" +
                        "<td  align=""center"">" & Format(jobCompactionIntDenDepthCtable(ISect, IJob, J - 1) * UnitsOut.inch, "0") & " - " & Format(jobCompactionIntDenDepthCtable(ISect, IJob, J) * UnitsOut.inch, "0") & "</td>"
                    If jobCompactionIntDenDepthCtable(ISect, IJob, J) <= ThicktoSubgrade Then
                        Info = Info & "<td  align=""center"">" & "--" & " </td>"
                    Else
                        Info = Info & "<td  align=""center"">" & Format(Math.Max(jobCompactionIntDenDepthCtable(ISect, IJob, J - 1) - ThicktoSubgrade, 0) * UnitsOut.inch, "0") & " - " & Format((jobCompactionIntDenDepthCtable(ISect, IJob, J) - ThicktoSubgrade) * UnitsOut.inch, "0") & "</td>"
                    End If
                    'Info = Info & "<td  align=""center"">" & CallAC(jobCompactionIntDenCriticalACCtable(ISect, J)).ACname & "</td>"
                    Info = Info & "<td  align=""center"">" & ACName(jobCompactionIntDenCriticalACCtable(ISect, IJob, J)) & "</td>"



                End If
            Next J

            Info = Info & "</table>"

            'added for NOTES by YGC 102213 
            Info = Info & "<br><b>Subgrade Compaction Notes:</b><br>"
            Info = Info & "1.	Noncohesive  soils, for the purpose of determining compaction control, are those with a plasticity index (PI) less than 3.<br>"
            Info = Info & "2.	Tabulated values indicate depth ranges within which densities should equal or exceed the indicated percentage of the maximum dry density as specified in item P-152.<br>"
            If CallAC(IDHeaviestAC).GearLoad >= 60000 Then
                Info = Info & "3.	Maximum dry density is determined using ASTM Method D 1557.<br>"
            Else
                Info = Info & "3.	Maximum dry density is determined using ASTM Method D 698.<br>"
            End If
            Info = Info & "4.	The subgrade in cut areas should have natural densities shown or should (a) be compacted from the surface to achieve the required densities, (b) be removed and replaced at the densities shown, or (c) when economics and grades permit, be covered with sufficient select or subbase material so that the uncompacted subgrade is at a depth where the in-place densities are satisfactory.<br>"
            'Info = Info & "5.	For swelling soils refer to AC 150/5320-6E paragraph 313.<br>"
            Info = Info & "5.	For swelling soils refer to AC 150/5320-6F paragraph 3.10.<br>"
            'add ended for NOTES by YGC 102213 

        End If


PassCompaction:


        Info = Info & "<br><b>User is responsible for checking frost protection requirements.</b><br>"

        If IncludeImage Then
            'Dim s111 As String
            's111 = MyDocumentDir & "\Structure.jpg"
            'Directory.SetCurrentDirectory(MyDocumentDir)
            'System.Windows.Forms.Application.DoEvents()
            Info = Info & "<br><br><br>"
            'Info = Info & "<img src=Structure.jpg><br>"
            Info = Info & "<p><img src=Structure.jpg align=""middle"" width=""357"" height=""273""></p><br>"


            'Info = Info & "<p><img src=s111 align=""middle"" width=""357"" height=""273""></p><br>"

            'Info = Info & "<p><img src=Structure.jpg align=""middle"" width=""286"" height=""218""></p><br>"
            'Info = Info & "<p><img src=Structure.jpg align=""middle"" width=""238"" height=""182""></p><br>"
            'http://www.w3schools.com/tags/att_img_align.asp
        End If

        '======================================================================================================



    End Sub


    Public Sub CreateHTML2(ByRef Info As String)
        'http://stackoverflow.com/questions/24983498/save-image-from-picturebox-vb
        'http://www.w3schools.com/html/html_images.asp
        'http://www.w3schools.com/tags/tag_img.asp

        htmlText = "<html>" +
           "<body>" +
           "<h2>Spectacular Mountains</h2><br>" +
           "<img src=Structure.jpg width=""238"" height=""182"">" +
           "</body>" +
           "</html>"

        '"<img src=MyStructure111.jpg alt=""Mountain View"" style=""width:238px;height:182px"">" + _
        'good
        '"<img src=MyStructure111.jpg alt=""Mountain View"" style=""width:476px;height:364px"">" + _

        '"<img src=""pic_mountain.jpg"" alt=""Mountain View"" style=""width:304px;height:228px"">" + _
        '"<img src=gPictureStructure.image alt=""Mountain View"" style=""width:304px;height:228px"">" + _

        Info = htmlText

    End Sub


    Public Sub CreatePDFAuto()


        'Try 'added kawa 2015

        '    Dim pdfFile As String
        '    Dim sav As New SaveFileDialog()
        '    'sav.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString
        '    'sav.InitialDirectory = WorkingDir
        '    sav.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\"


        '    sav.Filter = "PDF Files (*.pdf)|*.pdf"
        '    sav.RestoreDirectory = True

        '    sav.FileName = JobName & "_" & SectName
        '    pdfFile = sav.FileName & ".pdf"


        '    'If sav.ShowDialog = Windows.Forms.DialogResult.OK Then
        '    '    pdfFile = sav.FileName
        '    '    If InStr(pdfFile, ".pdf") = 0 Then
        '    '        pdfFile = pdfFile.Substring(0, pdfFile.Length - 4) & ".pdf"

        '    '        If File.Exists(pdfFile) Then
        '    '            Dim ret As Integer, M1 As String
        '    '            M1 = "File " & pdfFile & " already exists. " & NL & "Do you want to replace it?"

        '    '            ret = MsgBox(M1, MsgBoxStyle.YesNo, "Save As")
        '    '            '7 for No, 6 for Yes
        '    '            If ret = 7 Then
        '    '                Exit Sub
        '    '            End If
        '    '        End If

        '    '    End If
        '    'Else
        '    '    Exit Sub
        '    'End If

        '    'http://www.w3schools.com/html/html_paragraphs.asp

        '    Dim document As New Document
        '    'PdfWriter.GetInstance(document, New FileStream("C:\ZZZZ\InfoPDF.pdf", FileMode.Create))
        '    PdfWriter.GetInstance(document, New FileStream(WorkingDir & "\" & pdfFile, FileMode.Create))
        '    document.Open()
        '    Dim hw As iTextSharp.text.html.simpleparser.HTMLWorker = New iTextSharp.text.html.simpleparser.HTMLWorker(document)

        '    hw.Parse(New StringReader(htmlText))
        '    document.Close()


        'Catch ex As Exception

        '    Dim abc As Boolean
        '    abc = InStr(1, ex.Message, "The process cannot access the file", CompareMethod.Text) > 0

        '    If abc Then

        '        Dim txt As String
        '        'txt = ex.Message
        '        txt = "A PDF design report is open and is preventing" + NL
        '        txt = txt + "completion of the job." + NL2
        '        txt = txt + "Please close the report to continue."

        '        MsgBox(txt)

        '    Else

        '        Dim txt As String
        '        txt = ex.Message
        '        txt = txt + Environment.NewLine + Environment.NewLine
        '        txt = txt + ex.StackTrace
        '        txt = txt + Environment.NewLine + Environment.NewLine
        '        MsgBox(txt)

        '    End If



        'End Try



    End Sub

    Public Sub CreatePDF()


        'Dim pdfFile As String
        'Dim sav As New SaveFileDialog()
        ''sav.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString
        'sav.InitialDirectory = WorkingDir
        'sav.Filter = "PDF Files (*.pdf)|*.pdf"
        'sav.RestoreDirectory = True

        'sav.FileName = JobName & "_" & SectName

        'If sav.ShowDialog = Windows.Forms.DialogResult.OK Then
        '    pdfFile = sav.FileName
        '    If InStr(pdfFile, ".pdf") = 0 Then
        '        pdfFile = pdfFile.Substring(0, pdfFile.Length - 4) & ".pdf"

        '        If File.Exists(pdfFile) Then
        '            Dim ret As Integer, M1 As String
        '            M1 = "File " & pdfFile & " already exists. " & NL & "Do you want to replace it?"

        '            ret = MsgBox(M1, MsgBoxStyle.YesNo, "Save As")
        '            '7 for No, 6 for Yes
        '            If ret = 7 Then
        '                Exit Sub
        '            End If
        '        End If

        '    End If
        'Else
        '    Exit Sub
        'End If




        ''http://www.w3schools.com/html/html_paragraphs.asp

        'Dim document As New Document
        ''PdfWriter.GetInstance(document, New FileStream("C:\ZZZZ\InfoPDF.pdf", FileMode.Create))
        'PdfWriter.GetInstance(document, New FileStream(pdfFile, FileMode.Create))
        'document.Open()
        'Dim hw As iTextSharp.text.html.simpleparser.HTMLWorker = New iTextSharp.text.html.simpleparser.HTMLWorker(document)

        'hw.Parse(New StringReader(htmlText))
        'document.Close()


    End Sub




    Public Function FindLife(ByVal LifePTraff As Single,
                         ByVal LifeEstimated As Single,
                         ByVal CDFMAX As Single,
                         ByRef Overflow As Boolean,
                         ByRef StressResponse(,) As Double,
                         ByVal RCONval As Single,
                         ByVal SCIval As Single) As Single

        FindLife = FindLife2018(LifePTraff, LifeEstimated, CDFMAX, Overflow, StressResponse, RCONval, SCIval)
        Exit Function

        Dim I As Short, CDFM1, DELT, TempMax As Single
        Dim LifeM1, PcntCDFUTemp, OldReps(NAC) As Single
        Dim CDFtableTemp(MaxSectAC), CDFacrftMaxtableTemp(MaxSectAC) As Double
        Dim InfiLife As Boolean = False
        Dim bBisection As Boolean = False '2018.06.25
        Dim LL11, LL22 As Single
        Dim CD11, CD22 As Single


        LifeM1 = Life
        CDFPic = CDFMAX : CDFM1 = CDFMAX
        PcntCDFUTemp = PcntCDFU
        For I = 1 To NAC : OldReps(I) = Reps(I) : Next I

        ''===========================================
        'FileOpen(9, "UUUUUUUUUUU.txt", OpenMode.Append)
        'Print(9, LPad(34, "LifeStr"))
        'Print(9, LPad(34, "CDF"))
        'PrintLine(9, "")
        'FileClose(9)
        ''===========================================


        LifeStr = LifeEstimated

        '        '===========================================
        '        LifeStr = 0.0001
        'start2:

        '        If LifeStr < 10000000 Then
        '            LifeStr = LifeStr * 10
        '        Else
        '            LifeStr = LifeStr * 1.2!
        '        End If
        '        '===========================================

        Do
            For I = 1 To NAC
                Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
                If Temp1 > TempMax And RepsInc(I) < 0 Then
                    InfiLife = True : Temp1 = TempMax
                Else
                    InfiLife = False
                End If
                Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
                Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
            Next I

            Overflow = True
            'Call LeafCDFRigid_NP(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)
            Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)

            ''===========================================
            'FileOpen(9, "UUUUUUUUUUU.txt", OpenMode.Append)

            'If LifeStr < 1 Then
            '    Print(9, LPad(35, Format(LifeStr, "#,##0.00000000000000000")))
            'Else
            '    Print(9, LPad(35, Format(LifeStr, "#,##0.000")))
            'End If

            'If CDFMAX < 1 Then
            '    Print(9, LPad(35, Format(CDFSUBMAX, "#,##0.0000000000000000")))
            'Else
            '    Print(9, LPad(35, Format(CDFSUBMAX, "#,##0.00000")))
            'End If

            'PrintLine(9, "")
            'FileClose(9)
            'GoTo start2
            ''===========================================


            If Overflow And False Then
                S = "PCC stresses (or traffic) are too low" & vbCrLf
                S = S & "to accurately compute life."
                Ret = MsgBoxDQ(S, 0, "Computing Life (FindLife)")
                GoTo fin1
            ElseIf bBisection Then

                If CDFSUBMAX < 1 Then
                    LL11 = LifeStr
                    CD11 = CDFSUBMAX
                Else
                    LL22 = LifeStr
                    CD22 = CDFSUBMAX
                End If

                LifeStr = (LL11 + LL22) / 2
                CDFM1 = CDFSUBMAX

            ElseIf InfiLife And CDFSUBMAX < 1.0 Then
                FindLife = 10000000 : LifeStr = FindLife : GoTo fin1
            ElseIf CDFSUBMAX > 1 And LifeStr = 0.01! Then
                FindLife = 0.01 : LifeStr = FindLife : GoTo fin1
            ElseIf Math.Abs(1 - CDFSUBMAX) < 0.001 Then
                FindLife = LifeStr : LifeStr = FindLife : GoTo fin1
            ElseIf LifeM1 < 0.0001 Then
                LifeM1 = 0.0! : LifeStr = 0.0! : FindLife = LifeStr
                GoTo fin1
            ElseIf ((CDFSUBMAX < 0.0001) And (CDFM1 > 10000000)) Then
                bBisection = True

                CD22 = CDFM1
                CD11 = CDFSUBMAX

                LL22 = LL11
                LL11 = LifeStr
                LifeStr = (LL11 + LL22) / 2

            End If

            If bBisection Then


            Else
                DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFSUBMAX - CDFM1)

                LL11 = LifeStr
                Temp = LifeStr
                LifeStr = LifeM1 + DELT : If LifeStr <= 0 Then LifeStr = 0.01
                LifeM1 = Temp
                CDFM1 = CDFSUBMAX
            End If

        Loop Until System.Math.Abs(CDFM1 - 1.0!) < 0.001

fin1:
        For I = 1 To NAC : Reps(I) = OldReps(I) : Next I
        FindLife = LifeStr

    End Function


    Public Function FindLife2018(ByVal LifePTraff As Single,
                      ByVal LifeEstimated As Single,
                      ByVal CDFMAX As Single,
                      ByRef Overflow As Boolean,
                      ByRef StressResponse(,) As Double,
                      ByVal RCONval As Single,
                      ByVal SCIval As Single) As Single

        Dim I As Short, CDFM1, DELT, TempMax As Single
        Dim LifeM1, PcntCDFUTemp, OldReps(NAC) As Single
        Dim CDFtableTemp(MaxSectAC), CDFacrftMaxtableTemp(MaxSectAC) As Double
        Dim InfiLife As Boolean = False
        Dim LL11, LL22 As Single, CD11, CD22 As Single

        iC1 = 0 : iC2 = 0
        LifeM1 = Life
        CDFPic = CDFMAX : CDFM1 = CDFMAX
        PcntCDFUTemp = PcntCDFU
        For I = 1 To NAC : OldReps(I) = Reps(I) : Next I
        LifeStr = LifeEstimated

        Do
            For I = 1 To NAC
                Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
                If Temp1 > TempMax And RepsInc(I) < 0 Then
                    InfiLife = True : Temp1 = TempMax
                Else
                    InfiLife = False
                End If
                Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
                Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
            Next I

            Overflow = True
            Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)

            If Overflow And False Then
                S = "PCC stresses (or traffic) are too low" & vbCrLf
                S = S & "to accurately compute life."
                Ret = MsgBoxDQ(S, 0, "Computing Life (FindLife)")
                GoTo fin1
            ElseIf Math.Abs(1 - CDFSUBMAX) < 0.001 Then
                FindLife2018 = LifeStr : LifeStr = FindLife2018 : GoTo fin1
            ElseIf InfiLife And CDFSUBMAX < 1.0 Then
                FindLife2018 = 1.0E+38 : LifeStr = FindLife2018 : GoTo fin1
            ElseIf (LifeStr < 0.001!) Or (LifeStr > 1000.0) Then
                GoTo doBisection
            End If


            DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFSUBMAX - CDFM1)

            LL11 = LifeStr
            Temp = LifeStr
            LifeStr = LifeM1 + DELT : If LifeStr <= 0 Then LifeStr = 0.01
            LifeM1 = Temp
            CDFM1 = CDFSUBMAX
            iC1 = iC1 + 1

        Loop Until (System.Math.Abs(CDFM1 - 1.0!) < 0.001) Or iC1 > 30

fin1:
        For I = 1 To NAC : Reps(I) = OldReps(I) : Next I
        FindLife2018 = LifeStr
        Exit Function


doBisection:

        If CDFSUBMAX > 1 Then
            Do
                LL22 = LifeStr : CD22 = CDFSUBMAX
                LifeStr = LifeStr * 0.5!

                For I = 1 To NAC
                    Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
                    If Temp1 > TempMax And RepsInc(I) < 0 Then
                        InfiLife = True : Temp1 = TempMax
                    Else
                        InfiLife = False
                    End If
                    Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                    Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
                    Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
                Next I

                Overflow = True
                Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)
                LL11 = LifeStr : CD11 = CDFSUBMAX
            Loop Until (System.Math.Abs(CDFSUBMAX - 1.0!) < 0.001) Or (CDFSUBMAX < 1)
        Else
            Do
                LL11 = LifeStr : CD11 = CDFSUBMAX
                LifeStr = LifeStr * 2.0!
                For I = 1 To NAC
                    Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
                    If Temp1 > TempMax And RepsInc(I) < 0 Then
                        InfiLife = True : Temp1 = TempMax
                    Else
                        InfiLife = False
                    End If
                    Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
                    Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
                    Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
                Next I

                Overflow = True
                Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)
                LL22 = LifeStr : CD22 = CDFSUBMAX
            Loop Until (System.Math.Abs(CDFSUBMAX - 1.0!) < 0.001) Or (CDFSUBMAX > 1)

        End If


repeat1:


        LifeStr = (LL11 + LL22) / 2

        For I = 1 To NAC
            Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
            If Temp1 > TempMax And RepsInc(I) < 0 Then
                InfiLife = True : Temp1 = TempMax
            Else
                InfiLife = False
            End If
            Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
            Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
            Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
        Next I
        Overflow = True
        Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)

        If CDFSUBMAX < 1 Then
            LL11 = LifeStr
            CD11 = CDFSUBMAX
        Else
            LL22 = LifeStr
            CD22 = CDFSUBMAX
        End If

        If (System.Math.Abs(CDFSUBMAX - 1.0!) < 0.001) Or (iC2 > 30) Then
            GoTo fin1
        Else
            iC2 = iC2 + 1
            GoTo repeat1
        End If
        GoTo fin1

    End Function




End Module