Option Strict On
Option Explicit On

Imports System.IO

Public Module YYY2


    Public Function CalculateACN4_Div1_2gears(ByVal PavementType As clsACR.PavementType,
         ByVal gross_weight As Single,
         ByVal percent_gw As Single,
         ByVal wheels_number As Integer,
         ByVal tire_pressure As Single,
         ByVal CoordX() As Single,
         ByVal CoordY() As Single,
         ByVal percent_gw2 As Single,
          ByVal wheels_number2 As Integer,
          ByVal tire_pressure2 As Single,
          ByVal CoordX2() As Single,
          ByVal CoordY2() As Single,
          ByVal newDiv1 As Single,
          ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        div1 = newDiv1
        bNewDiv1 = True
        bMeshSize = False 'new123

        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN4_Div1_2gears = RunACN.CalculateACR(PavementType, gross_weight,
                                percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)
        RunACN = Nothing

    End Function




    Public Function CalculateACN1_Div1_2gears(ByVal PavementType As clsACR.PavementType,
         ByVal gross_weight As Single,
         ByVal percent_gw As Single,
         ByVal wheels_number As Integer,
         ByVal tire_pressure As Single,
         ByVal CoordX() As Single,
         ByVal CoordY() As Single,
         ByVal percent_gw2 As Single,
          ByVal wheels_number2 As Integer,
          ByVal tire_pressure2 As Single,
          ByVal CoordX2() As Single,
          ByVal CoordY2() As Single,
         ByVal modul1 As Single,
         ByVal newDiv1 As Single,
         ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        div1 = newDiv1
        bNewDiv1 = True
        bMeshSize = False 'new123

        Call Print1_Output()

        gICAOCodeIndex = ICAOCodeIndexF(modul1)
        start_cat = gICAOCodeIndex
        end_cat = start_cat

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN1_Div1_2gears = RunACN.CalculateACR(PavementType, gross_weight,
                                percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)
        RunACN = Nothing

    End Function



    Public Function CalculateACN4_Mesh_2gears(ByVal PavementType As clsACR.PavementType,
         ByVal gross_weight As Single,
         ByVal percent_gw As Single,
         ByVal wheels_number As Integer,
         ByVal tire_pressure As Single,
         ByVal CoordX() As Single,
         ByVal CoordY() As Single,
         ByVal percent_gw2 As Single,
          ByVal wheels_number2 As Integer,
          ByVal tire_pressure2 As Single,
          ByVal CoordX2() As Single,
          ByVal CoordY2() As Single,
         ByVal MeshSize1 As Single,
         ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        gMeshSize = MeshSize1
        bMeshSize = True

        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN4_Mesh_2gears = RunACN.CalculateACR(PavementType, gross_weight,
                                percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)
        RunACN = Nothing

    End Function



    Public Function CalculateACN1_Mesh_2gears(ByVal PavementType As clsACR.PavementType,
         ByVal gross_weight As Single,
         ByVal percent_gw As Single,
         ByVal wheels_number As Integer,
         ByVal tire_pressure As Single,
         ByVal CoordX() As Single,
         ByVal CoordY() As Single,
         ByVal percent_gw2 As Single,
          ByVal wheels_number2 As Integer,
          ByVal tire_pressure2 As Single,
          ByVal CoordX2() As Single,
          ByVal CoordY2() As Single,
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
        CalculateACN1_Mesh_2gears = RunACN.CalculateACR(PavementType, gross_weight,
                                percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)
        RunACN = Nothing

    End Function





    Public Function CalculateACN4_Output_2gears(ByVal PavementType As clsACR.PavementType,
      ByVal gross_weight As Single,
      ByVal percent_gw As Single,
      ByVal wheels_number As Integer,
      ByVal tire_pressure As Single,
      ByVal CoordX() As Single,
      ByVal CoordY() As Single,
      ByVal percent_gw2 As Single,
      ByVal wheels_number2 As Integer,
      ByVal tire_pressure2 As Single,
      ByVal CoordX2() As Single,
      ByVal CoordY2() As Single,
      ByVal iSelectEval As Integer,
      ByVal SW1() As Integer,
      ByVal SW2() As Integer) As ACRClassLib.clsACR.ACRdata

        bSelectWheels = True

        ReDim gSW(wheels_number + wheels_number2)

        For i1 As Integer = 1 To wheels_number
            gSW(i1) = SW1(i1)
        Next

        For i1 As Integer = 1 + wheels_number To wheels_number + wheels_number2
            gSW(i1) = SW2(i1 - wheels_number)
        Next


        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN4_Output_2gears = RunACN.CalculateACR(PavementType, gross_weight,
                                           percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                           percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)
        RunACN = Nothing

    End Function













    Public Function CalculateACN4_Output_2gears(ByVal PavementType As clsACR.PavementType,
          ByVal gross_weight As Single,
          ByVal percent_gw As Single,
          ByVal wheels_number As Integer,
          ByVal tire_pressure As Single,
          ByVal CoordX() As Single,
          ByVal CoordY() As Single,
          ByVal percent_gw2 As Single,
          ByVal wheels_number2 As Integer,
          ByVal tire_pressure2 As Single,
          ByVal CoordX2() As Single,
          ByVal CoordY2() As Single,
          ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN4_Output_2gears = RunACN.CalculateACR(PavementType, gross_weight,
                                           percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                           percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2)
        RunACN = Nothing

    End Function



    Public Function CalculateACN1_Output_2gears(ByVal PavementType As clsACR.PavementType,
          ByVal gross_weight As Single,
          ByVal percent_gw As Single,
          ByVal wheels_number As Integer,
          ByVal tire_pressure As Single,
          ByVal CoordX() As Single,
          ByVal CoordY() As Single,
          ByVal percent_gw2 As Single,
          ByVal wheels_number2 As Integer,
          ByVal tire_pressure2 As Single,
          ByVal CoordX2() As Single,
          ByVal CoordY2() As Single,
          ByVal modul1 As Single,
          ByVal iSelectEval As Integer) As ACRClassLib.clsACR.ACRdata

        iSelectEval_ACN = iSelectEval

        gICAOCodeIndex = ICAOCodeIndexF(modul1)
        start_cat = gICAOCodeIndex
        end_cat = start_cat

        Call Print1_Output()

        Dim RunACN As ACRClassLib.clsACR
        RunACN = New ACRClassLib.clsACR()
        CalculateACN1_Output_2gears = RunACN.CalculateACR(PavementType, gross_weight,
                                           percent_gw, wheels_number, tire_pressure, CoordX, CoordY,
                                           percent_gw2, wheels_number2, tire_pressure2, CoordX2, CoordY2, modul1)
        RunACN = Nothing

    End Function









End Module
