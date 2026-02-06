Option Strict On
Option Explicit On

Module modPCN_runACNClass

    'Public RunACNLib1 As New ACNClassLib.clsACN
    'Public ACNdataFF1 As ACNClassLib.clsACN.ACNdata

    'Public RunACNLib_Rigid1 As New ACNClassLib.clsACN
    'Public ACNdataFF_Rigid1 As ACNClassLib.clsACN.ACNdata

    'Public RunACNLib_Rigid2 As New ACNClassLib.clsACN
    'Public ACNdataFF_Rigid2 As ACNClassLib.clsACN.ACNdata


    'Public RunACNLib As New ACNClassLib.clsACN()

    Public RunACNLib As New ACRClassLib.clsACR
    'Public RunACNLib1 As New ACNClassLib.clsACN


    'Public RunACNLib As New ACNClassLib.clsACN()
    'Public ACNdataFF As ACNClassLib.clsACN.ACNdata

    Sub Calculate_ACN()

        'Dim RunACN As ACNClassLib.clsACN
        Dim ACNdataFF As ACRClassLib.clsACR.ACRdata

        '    'Dim abc As accl

        ACNdataFF = RunACNLib.CalculateACR(ACRClassLib.clsACR.PavementType.Rigid, GL(1), 30, 4, 200, AC(1).libTX, AC(1).libTY)

    End Sub

End Module
