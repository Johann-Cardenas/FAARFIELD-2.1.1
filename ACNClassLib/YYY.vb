Option Strict On
Option Explicit On

Imports System.IO
Imports Microsoft.VisualBasic.FileIO

Public Module YYY
    Friend iSelectEval_ACN As Integer = 0

    Friend start_cat As Integer = 1
    Friend end_cat As Integer = 4

    Friend ACRdata1 As ACRClassLib.clsACR.ACRdata

    Public Function CalculateACN1_Output(ByVal PavementType As clsACR.PavementType,
         ByVal gross_weight As Single,
         ByVal percent_gw As Single,
         ByVal wheels_number As Integer,
         ByVal tire_pressure As Single,
         ByVal CoordX() As Single,
         ByVal CoordY() As Single,
         ByVal modul1 As Single,
         ByVal iSelectEval As Integer,
         ByVal SW() As Integer) As ACRClassLib.clsACR.ACRdata


        bSelectWheels = True
        ReDim gSW(wheels_number)

        For i1 As Integer = 1 To wheels_number
            gSW(i1) = SW(i1)
        Next

        iSelectEval_ACN = iSelectEval

        gICAOCodeIndex = ICAOCodeIndexF(modul1)
        start_cat = gICAOCodeIndex
        end_cat = start_cat
        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN1_Output = RunACN.CalculateACR(PavementType, gross_weight,
                                           percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
        RunACN = Nothing

    End Function




    Public Function CalculateACN1_Output(ByVal PavementType As clsACR.PavementType,
            ByVal gross_weight As Single,
            ByVal percent_gw As Single,
            ByVal wheels_number As Integer,
            ByVal tire_pressure As Single,
            ByVal CoordX() As Single,
            ByVal CoordY() As Single,
            ByVal modul1 As Single,
            ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        gICAOCodeIndex = ICAOCodeIndexF(modul1)
        start_cat = gICAOCodeIndex
        end_cat = start_cat
        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN1_Output = RunACN.CalculateACR(PavementType, gross_weight,
                                           percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
        RunACN = Nothing

    End Function



    Public Function CalculateACN4_Output(ByVal PavementType As clsACR.PavementType,
        ByVal gross_weight As Single,
        ByVal percent_gw As Single,
        ByVal wheels_number As Integer,
        ByVal tire_pressure As Single,
        ByVal CoordX() As Single,
        ByVal CoordY() As Single,
        ByVal iSelectEval As Integer,
        ByVal SW() As Integer) As ACRClassLib.clsACR.ACRdata

        Try

            bSelectWheels = True
            ReDim gSW(wheels_number)

            For i1 As Integer = 1 To wheels_number
                gSW(i1) = SW(i1)
            Next

            iSelectEval_ACN = iSelectEval

            Call Print1_Output()

            Dim RunACN As ACRClassLib.clsACR
            RunACN = New ACRClassLib.clsACR()
            CalculateACN4_Output = RunACN.CalculateACR(PavementType, gross_weight,
                            percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
            RunACN = Nothing

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Function


    Public Function CalculateACN4_Output(ByVal PavementType As clsACR.PavementType,
        ByVal gross_weight As Single,
        ByVal percent_gw As Single,
        ByVal wheels_number As Integer,
        ByVal tire_pressure As Single,
        ByVal CoordX() As Single,
        ByVal CoordY() As Single,
        ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        Try

            iSelectEval_ACN = iSelectEval

            Call Print1_Output()

            Dim RunACN As ACRClassLib.clsACR
            RunACN = New ACRClassLib.clsACR()
            CalculateACN4_Output = RunACN.CalculateACR(PavementType, gross_weight,
                            percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
            RunACN = Nothing

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Function




    Public Function CalculateACN_OutputMesh(ByVal PavementType As clsACR.PavementType,
            ByVal gross_weight As Single,
            ByVal percent_gw As Single,
            ByVal wheels_number As Integer,
            ByVal tire_pressure As Single,
            ByVal CoordX() As Single,
            ByVal CoordY() As Single,
            ByVal modul1 As Single,
            ByVal MeshSize1 As Single,
            ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        gMeshSize = MeshSize1
        bMeshSize = True
        Call Print1_Output()
        gICAOCodeIndex = ICAOCodeIndexF(modul1)
        start_cat = gICAOCodeIndex
        end_cat = start_cat

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN_OutputMesh = RunACN.CalculateACR(PavementType, gross_weight,
                                percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
        RunACN = Nothing

    End Function


    Public Function CalculateACN_OutputMesh(ByVal PavementType As clsACR.PavementType,
      ByVal gross_weight As Single,
      ByVal percent_gw As Single,
      ByVal wheels_number As Integer,
      ByVal tire_pressure As Single,
      ByVal CoordX() As Single,
      ByVal CoordY() As Single,
      ByVal MeshSize As Single,
      ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        gMeshSize = MeshSize
        bMeshSize = True
        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN_OutputMesh = RunACN.CalculateACR(PavementType, gross_weight,
                                percent_gw, wheels_number, tire_pressure, CoordX, CoordY)
        RunACN = Nothing

    End Function



    Public Function OneACN_Output(ByVal PavementType As clsACR.PavementType,
      ByVal gross_weight As Single,
      ByVal percent_gw As Single,
      ByVal wheels_number As Integer,
      ByVal tire_pressure As Single,
      ByVal X1() As Single,
      ByVal Y1() As Single,
      ByVal CBRinput As Single,
      ByVal MeshSize1 As Single,
      ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        OneACN_Output = ACRClassLib.YYY.CalculateACN_OutputMesh(PavementType, gross_weight,
                      percent_gw, wheels_number, tire_pressure, X1, Y1, CBRinput, MeshSize1, iSelectEval)

    End Function


    Public Function FourACN_Output(ByVal PavementType As clsACR.PavementType,
        ByVal gross_weight As Single,
        ByVal percent_gw As Single,
        ByVal wheels_number As Integer,
        ByVal tire_pressure As Single,
        ByVal X1() As Single,
        ByVal Y1() As Single,
        ByVal CBRinput As Single,
        ByVal MeshSize1 As Single,
        ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        FourACN_Output = ACRClassLib.YYY.CalculateACN_OutputMesh(PavementType, gross_weight,
                      percent_gw, wheels_number, tire_pressure, X1, Y1, CBRinput, MeshSize1, iSelectEval)
        RunACN = Nothing

    End Function




    Public Function Div1_OneACN(ByVal PavementType As clsACR.PavementType,
      ByVal gross_weight As Single,
      ByVal percent_gw As Single,
      ByVal wheels_number As Integer,
      ByVal tire_pressure As Single,
      ByVal X1() As Single,
      ByVal Y1() As Single,
      ByVal modul1 As Single,
      ByVal newDiv1 As Single,
      ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        div1 = newDiv1
        bNewDiv1 = True
        bMeshSize = False 'new123
        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        Div1_OneACN = RunACN.CalculateACR(PavementType, gross_weight,
                             percent_gw, wheels_number, tire_pressure, X1, Y1, modul1)
        RunACN = Nothing

    End Function


    Public Function Div1_FourACN(ByVal PavementType As clsACR.PavementType,
          ByVal gross_weight As Single,
          ByVal percent_gw As Single,
          ByVal wheels_number As Integer,
          ByVal tire_pressure As Single,
          ByVal X1() As Single,
          ByVal Y1() As Single,
          ByVal newDiv1 As Single,
          ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        Try

            div1 = newDiv1
            bNewDiv1 = True
            bMeshSize = False 'new123
            Call Print1_Output()

            Div1_FourACN = ACRClassLib.YYY.CalculateACN4_Output(PavementType, gross_weight,
                                         percent_gw, wheels_number, tire_pressure, X1, Y1, iSelectEval)

        Catch ex As Exception

            Dim txt As String
            txt = ex.Message
            txt = txt + Environment.NewLine + Environment.NewLine
            txt = txt + ex.StackTrace
            txt = txt + Environment.NewLine + Environment.NewLine
            MsgBox(txt)

        End Try



    End Function


    Public Function Mesh_OneACN(ByVal PavementType As clsACR.PavementType,
        ByVal gross_weight As Single,
        ByVal percent_gw As Single,
        ByVal wheels_number As Integer,
        ByVal tire_pressure As Single,
        ByVal X1() As Single,
        ByVal Y1() As Single,
        ByVal CBRinput As Single,
        ByVal MeshSize1 As Single,
        ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()

        Mesh_OneACN = ACRClassLib.YYY.CalculateACN_OutputMesh(PavementType, gross_weight,
                      percent_gw, wheels_number, tire_pressure, X1, Y1, CBRinput, MeshSize1, iSelectEval)

        RunACN = Nothing
    End Function


    Public Function Mesh_FourACN(ByVal PavementType As clsACR.PavementType,
          ByVal gross_weight As Single,
          ByVal percent_gw As Single,
          ByVal wheels_number As Integer,
          ByVal tire_pressure As Single,
          ByVal X1() As Single,
          ByVal Y1() As Single,
          ByVal MeshSize1 As Single,
          ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        Mesh_FourACN = ACRClassLib.YYY.CalculateACN_OutputMesh(PavementType, gross_weight,
                              percent_gw, wheels_number, tire_pressure, X1, Y1, MeshSize1, iSelectEval)
        RunACN = Nothing

    End Function


    Friend Sub Check_WDir1_Directory()

        'WDir1 = System.Windows.Forms.Application.StartupPath & "\ACR_Results_" & gPavementType
        WDir1 = SpecialDirectories.MyDocuments + "\My FAARFIELD\ACR_Results_" & PaveTypeF(gPavementType)

        If Directory.Exists(WDir1) Then
        Else
            System.IO.Directory.CreateDirectory(WDir1)
        End If

    End Sub

    Friend Sub Print1_Output()

        Dim FF1 As String
        FF1 = getTodaysDateFormatted()
        gFileName = FF1

        gPrintOutput = True
        gPrintOutput_ACN = False
        gPrintOutput_Responses = True
        gPrint1800 = False

    End Sub


End Module
