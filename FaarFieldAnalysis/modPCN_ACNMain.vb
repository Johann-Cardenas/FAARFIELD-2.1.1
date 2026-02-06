Module modPCN_ACNMain

    'Izydor 11/11/2014 (ICAOversion variable)
    Public ICAOversion As Boolean = True

    Public ACN_mode_true As Boolean 'ikawa 01/29/03
    Public PCN_mode_true As Boolean 'GFH 06/12/08
    Public Stress_mode As Boolean 'ikawa 11/25/08
    Public Index3StressCalc As Boolean 'ikawa seattle
    Public SamePcntAndPress As Boolean

    Public MaxGrossWeightTrue As Boolean
    Public mode_changed As Boolean 'ikawa 02/14/03
    Public ResizingfrmGear As Boolean ' GFH 04/17/06.

    Public rowsNumber As Integer 'ik02
    Public ACNrowsNumber As Integer
    Public ThickrowsNumber As Integer
    Public frmGearStarted As Boolean
    Public VGAMode As Boolean
    Public ScaleModeConst As Double

    Public frmGearStartHeight As Double
    Public frmGearStartWidth As Double
    Public lstLibFileStartHeight As Double
    Public lblCriticalAircraftStartTop As Double
    Public lblCriticalAircraftTextStartTop As Double
    Public lblEvaluationThicknessStartTop As Double
    Public txtEvaluationThicknessStartTop As Double
    Public lblMessageStartTop As Double
    Public grdParmsStartTop As Double
    Public grdParmsStartLeft As Double
    Public grdOutputStartTop As Double
    Public grdOutputStartLeft As Double
    Public cmdFlexibleComputeStartTop As Double
    Public cmdFlexibleComputeStartLeft As Double
    Public cmdRigidComputeStartTop As Double
    Public cmdRigidComputeStartLeft As Double
    Public fraEditWheelsStartLeft As Double
    Public fraLibraryFunctionsStartLeft As Double
    Public fraMiscellaneousFunctionsStartLeft As Double
    Public fraOptionsStartLeft As Double
    Public fraCompModeStartTop As Double
    Public fraCompModeStartLeft As Double
    Public picGearStartHeight As Double
    Public picGearStartWidth As Double

    Public frmACNtxtOutputHeight As Double
    Public frmACNtxtOutputStartHeight As Double
    Public frmACNgphAlphaHeight As Double
    Public frmACNgphAlphaStartHeight As Double
    Public frmACNHeight As Double
    Public frmACNStartHeight As Double

    Public frmACNtxtOutputWidth As Double
    Public frmACNtxtOutputStartWidth As Double
    Public frmACNgphAlphaWidth As Double
    Public frmACNgphAlphaStartWidth As Double
    Public frmACNWidth As Double
    Public frmACNStartWidth As Double

    Public Const SGCol As Integer = 0
    Public Const SGText As String = "SG"
    Public Const CBRCol As Integer = 1
    Public Const CBRText As String = "CBR"
    Public Const CBRtCol As Integer = 2
    Public Const CBRtText As String = "Flex t, "
    Public Const ACNFlexCol As Integer = 3
    Public Const ACNFlexText As String = "ACN Flex"

    Public Const kCol As Integer = 4
    Public Const kText As String = "k, "
    Public Const RigtCol As Integer = 5
    Public Const RigtText As String = "Rig t, "
    Public Const ACNRigCol As Integer = 6
    Public Const ACNRigText As String = "ACN Rig"



End Module
