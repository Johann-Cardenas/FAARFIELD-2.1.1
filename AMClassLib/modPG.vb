' change "Single" to "Double", "CSng" to "CDbl" for FAASR3D by YC 102418-041519

Option Strict On
Option Explicit On

Imports System.Text.RegularExpressions ' YC 101215 092016

Module modAutoMesh
    Public rgdFile As String
    Public ingFile As String


    '_ikawa Dim xdim, ydim As Double
    Public EMod(6) As Double, PoissonsRatio(6) As Double, LayerThickness(6) As Double
    'Public XScaleFactor, YScaleFactor As Double  ' Moved to frmStructure.vb by YGC 020911

    Dim fff2, fff5, fff7 As Double
    Dim xlen1, ylen1, xlen2 As Double
    Public Const gFineMeshSize As Double = 6
    Public Const gFileName As String = "nikein"
    Public Const gSlabSize As Double = 300

    ' modifed for slab mesh selection by YGC 061113
    'added by YGC 021111
    'Public Const NoPts1 As Short = 101
    'Public Const NoPts2 As Short = 51
    Public NoPts1 As Integer
    Public NoPts2 As Integer
    'add end by YGC 021111
    ' modifed for slab mesh selection by YGC 061113 END

    'modified to decrease element number for fine area by YGC 072214 
    Public SlabFineMeshNumX As Integer = 40
    Public SlabFineMeshNumY As Integer = 12
    'modified to decrease element number for fine area by YGC 072214 END

    Dim NumBlcksCrseMeshX, NumBlcksCrseMeshY As Short
    Dim NumBlcksFineMeshX, NumBlcksFineMeshY As Short
    Dim e, c As Double

    Dim SymmSngl, SymmDbl As Boolean
    Dim MeshErr As Integer, SymmErr As Double
    Dim NXSymmetric, NYSymmetric As Short


    'YC 101215 092016
    'Dim Xfp, Yfp, Xtr(20), Ytr(20), Xgrid, Ygrid As Double 
    'Dim NSlabs As Short, ThkBase As Double
    'Dim LParam, Factor1, Factor2, Factor3, Friction As Double
    Public Xfp, Yfp, Xtr(20), Ytr(20), Xgrid, Ygrid As Double
    Public NSlabs As Short, ThkBase As Double
    Public LParam, Factor1, Factor2, Factor3, Friction As Double

    Public NX1, NX2, NY1, NY2 As Integer    'YC 101215 092016
    Public tkpcc, tkol As Double    'YC 101215 092016

    Public IndxModes, IndxSymm, IndxFndn, IndxOvrl As Short

    Public INGString As String
    Public NLcd, NSld, NMat, NPart As Integer

    ' QW 08-23-2017
    Public NNd, NBrckEle As Integer
    Public Nd() As FAAMeshClassLib.clsMesh.NodeCharacteristics
    Public BrickElement() As FAAMeshClassLib.clsMesh.BrickElementCharacteristics
    Public SpringType() As FAAMeshClassLib.clsMesh.SpringTypeCharacteristics
    Public SpringElement() As FAAMeshClassLib.clsMesh.SpringElementCharacteristics
    Public SlidingElement() As FAAMeshClassLib.clsMesh.SlidingElementCharacteristics
    Public NodalLoad() As FAAMeshClassLib.clsMesh.NodalLoadCharacteristics
    Public IPC As FEMClassLib.clsFEM.InputCards
    ' End QW 08-23-2017

    Public NPartSld, NPartBC, NPartMat As Integer

    Public itpro As Integer

    Public LCD() As FAAMeshClassLib.clsMesh.LoadCurveCharacteristics
    Public SLD() As FAAMeshClassLib.clsMesh.SlidingCharacteristics
    Public MAT() As FAAMeshClassLib.clsMesh.MaterialCharacteristics

    Public PART() As FAAMeshClassLib.clsMesh.PartCharacteristics
    Public PARTSLD() As FAAMeshClassLib.clsMesh.PartSldCharacteristics
    Public PARTBC() As FAAMeshClassLib.clsMesh.PartBCCharacteristics
    Public PARTMAT() As FAAMeshClassLib.clsMesh.PartMatCharacteristics

    Public ACLoad() As FAAMeshClassLib.clsMesh.ACLoadCharacteristics
    ' YC 101215 092016 END 

    Public NX1F, NX2F, NX3F, NX4F, NX5F As Integer 'YC 040517
    Public NY1F, NY2F, NY3F As Integer
    Public TS, tk, t3, t4, f13 As Double






    Sub AMMain(ByRef Filnam As String, ByRef Interior As Boolean,
          ByVal LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms, ByVal ACIndex As Short,
          ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms)

        SymmDbl = False : SymmSngl = False : MeshErr = 0 : SymmErr = 0
        'Izydor Kawa 10/16/2012
        rgdFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.rgd"
        ingFile = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\My FAARFIELD\nikein.ing"

        'This program creates the input file for ingrid.rgd and ingrid.ing
        Call GearLoads(ACIndex, LEAAircraft)
        Call Layer_Interface()
        'Call WriteTextFile(OpenMode.Output, Filnam & ".rgd")
        Call WriteTextFile(OpenMode.Output, rgdFile)

        'FileOpen(7, Filnam & ".ing", OpenMode.Output)
        'FileOpen(7, ingFile, OpenMode.Output)


        Call Material()

        Call Layer_SlabHexEdge()

        Call InfSlbDummyNodes() 'added for spring BC to simulate infinite slab by YGC 092613

        Call Layer_BaseEdgNew(LEAStructure)

        'Call DummyNodes()         'added to replace fixed corner node BC by spring BC by YGC 060311,suppresed for spring BC to simulate infinite slab by YGC 092613

        ' - PrintLine(7, INGString) 'YC 101215 092016

        ' - PrintLine(7, "")
        ' - PrintLine(7, "tp 0.01")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "end")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "rz 40")
        ' - PrintLine(7, "rx -45")
        ' - PrintLine(7, "set tv display")
        'FileClose(7)

        'Call ReadIngParms(INGString) 'YC 101215 092016 'YC 040517
        Call PassIngParms()

    End Sub

    Sub Layer_Interface()
        'This subroutine calculates the stiffness multiplier for weak bond interface model.
        'Tangential interface stiffness is given by:
        'KTang = Factor1 * Factor2 * KInterface
        'where KInterface = interface stiffness computed by NIKE3D contact surface routine (Type3)
        'Factor1 = NIKE3D Penalty Stiffness Scale Factor (=10.0 by default)
        'Factor2 = LParam/(1 - LParam) for 0.001 <= LParam <= 0.99
        '   = 100 for LParam > 99
        '   = 0.001 for LParam < 0.001
        'LParam is the interface input variable that ranges from 0 to 1
        'with LParam = 0 for fully unbonded and LParam = 1 for fully bonded.

        LParam = CDbl(frmStructure.IFParam / 10.0#)
        If LParam < 0.001 Then
            Factor2 = 0.001
        ElseIf LParam > 0.99 Then
            Factor2 = 100.0#
        Else
            Factor2 = CDbl(LParam / (1.0# - LParam))
        End If
        Factor1 = 10.0#
    End Sub


    Sub Material()
        'to remove parameter definiton for MeshClassLib by YC 101215 092016
        'subroutine for multiple aircraft in 1 step, full contact of AC on Rigid overlay by YGC 042312

        Dim IFOption, IFOption1, IFOption2, IFOption3 As String

        ' - PrintLine(7, "Rigid Airport Pavement")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "nk3d")
        ' - PrintLine(7, "anal stat")
        ' - PrintLine(7, "prcd 1 2 0 1.0 1.0 1.0")

        Dim ii, jj, kk As Integer
        Dim SS1, SS2 As String


        kk = 0
        INGString = Nothing
        For ii = 1 To gNACarg
            If NCat(iCat) > 1 Then
                If GroupIndex(ii) = iCat Then                 'suppressed to compute all aircraft in 1 step by YGC 102213, retrieved to seperate sym/unsym aircraft by YC 082216 092016
                    kk += 1
                    SS1 = "lcd "
                    SS1 = SS1 & CStr(kk) & " " & CStr(NCat(iCat) + 1)
                    SS2 = " "

                    'modified to reduce the length of a line in .ING by YGC 092512

                    'For jj = 0 To Math.Min(8, NCat(iCat))
                    '    If jj = kk Then
                    '        SS2 = SS2 + CDbl(jj).ToString("0").Replace(",", ".") & " 1.0 "
                    '    Else
                    '        SS2 = SS2 + CDbl(jj).ToString("0").Replace(",", ".") & " 0.0 "
                    '    End If
                    'Next

                    'SS1 = SS1 + SS2
                    '' - PrintLine(7, SS1)

                    INGString = INGString & SS1 & vbCrLf

                    For jj = 0 To NCat(iCat)
                        If jj Mod 8 = 0 And jj > 0 Then

                            INGString = INGString & vbCrLf

                        End If

                        If jj = kk Then
                            SS2 = CDbl(jj).ToString("0.0").Replace(",", ".") & " 1.0 "
                        Else
                            SS2 = CDbl(jj).ToString("0.0").Replace(",", ".") & " 0.0 "
                        End If

                        INGString = INGString & SS2

                    Next

                    INGString = INGString & vbCrLf

                    'modify ended to reduce the length of a line in .ING by YGC 092512


                End If     'suppressed to compute all aircraft in 1 step by YGC 102213,retrieved to seperate sym/unsym aircraft by YC 082216 092016

                'modified to compute all aircraft in 1 step by YGC 102213
                'Else
                '    If GroupIndex(ii) = iCat Then
                '        ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
                '    End If
            ElseIf NCat(iCat) = 1 Then

                INGString = "lcd 2 2" & vbCrLf & "0 0 1.0 1.0" & vbCrLf

                'modify ended to compute all aircraft in 1 step by YGC 102213

            End If
        Next ii
        INGString = INGString & vbCrLf

        '' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")

        '-----------------------------------------------
        IFOption = "sv"
        IFOption1 = "sl" 'sliding only - interface type 'LIA
        IFOption2 = "tied" 'tied sliding - interface type 'LIA
        IFOption3 = "sv" 'sliding with voids - interface type 'LIA


        ' added for partially bonded overlay by YC 100115
        If Factor2 > 0.001! Then
            Friction = 0.005#
        Else
            Friction = 0.0#
        End If
        ' added for partially bonded overlay by YC 100115 END

        Factor1 = 10

        INGString = INGString & "si 1 " & IFOption3 & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & "0.0" & " nomerge;" & vbCrLf

        ' modified for partially bonded overlay by YC 100115
        'If Overlay And Factor2 <= 0.1 Then 'Unbound Rigid Overlay
        '    ' - PrintLine(7, "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & "0.0" & " nomerge;")
        'End If

        'If Overlay Then  ' corrected for AConRigid overlay by YC 100115-1
        If Overlay And Factor2 <= 0.1 Then
            INGString = INGString & "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;" & vbCrLf
        End If
        ' modified for partially bonded overlay by YC 100115 END

        INGString = INGString & vbCrLf

        INGString = INGString & "mat  1  1" & vbCrLf &
                    "e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) & vbCrLf &
                    "pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & vbCrLf &
                    "ro 8.3912e-2" & vbCrLf & vbCrLf

        INGString = INGString & "mat  2  1" & vbCrLf &
            "e " & LPad(11, Format(EMod(2), "####000.00").Replace(",", ".")) & vbCrLf &
            "pr " & Format(PoissonsRatio(2), "#0.00").Replace(",", ".") & vbCrLf &
            "ro 1.872e-4" & vbCrLf & vbCrLf

        INGString = INGString & "mat  3  1" & vbCrLf &
            "e " & LPad(11, Format(EMod(3), "####000.00").Replace(",", ".")) & vbCrLf &
            "pr " & Format(PoissonsRatio(3), "#0.00").Replace(",", ".") & vbCrLf &
            "ro 1.872e-4" & vbCrLf & vbCrLf

        INGString = INGString & "mat  4  1" & vbCrLf &
          "e " & LPad(11, Format(EMod(4), "####000.00").Replace(",", ".")) & vbCrLf &
          "pr " & Format(PoissonsRatio(4), "#0.00").Replace(",", ".") & vbCrLf &
          "ro 1.872e-4" & vbCrLf & vbCrLf

        INGString = INGString & "mat  5  1" & vbCrLf &
             "e " & LPad(11, Format(EMod(5), "####000.00").Replace(",", ".")) & vbCrLf &
             "pr " & Format(PoissonsRatio(5), "#0.00").Replace(",", ".") & vbCrLf &
             "ro 1.872e-4" & vbCrLf & vbCrLf

        INGString = INGString & "mat  6  1" & vbCrLf &
            "e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) & vbCrLf &
            "pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & vbCrLf &
            "ro 1.872e-4" & vbCrLf & vbCrLf

        If InfiniteElement Then

            INGString = INGString & "mat  7 56" & vbCrLf &
                "e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) & vbCrLf &
                "pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & vbCrLf &
                "ro 6" & vbCrLf & vbCrLf

            INGString = INGString & "mat  8 56" & vbCrLf &
                    "e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) & vbCrLf &
                    "pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & vbCrLf &
                    "ro 1" & vbCrLf & vbCrLf

            INGString = INGString & "mat  9 56" & vbCrLf &
                "e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) & vbCrLf &
                "pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & vbCrLf &
                "ro 2" & vbCrLf & vbCrLf

            INGString = INGString & "mat 10 56" & vbCrLf &
                "e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) & vbCrLf &
                "pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & vbCrLf &
                "ro 4" & vbCrLf & vbCrLf

            INGString = INGString & "mat 11 56" & vbCrLf &
                "e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) & vbCrLf &
                "pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & vbCrLf &
                "ro 5" & vbCrLf & vbCrLf
        End If

        If Overlay Then
            INGString = INGString & "mat 12  1" & vbCrLf &
                "e " & LPad(11, Format(EMod(0), "####000.00").Replace(",", ".")) & vbCrLf &
                "pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & vbCrLf &
                "ro 8.3912e-2" & vbCrLf & vbCrLf
        End If

        INGString = INGString & "endmat" & vbCrLf & vbCrLf

    End Sub


    Sub MaterialAllAC()
        'used in version up to FF 2016.09.20 v1.41.0011, renamed by YC 101215 092016
        'subroutine for multiple aircraft in 1 step, full contact of AC on Rigid overlay by YGC 042312

        Dim IFOption, IFOption1, IFOption2, IFOption3 As String

        ' - PrintLine(7, "Rigid Airport Pavement")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "nk3d")
        ' - PrintLine(7, "anal stat")
        ' - PrintLine(7, "prcd 1 2 0 1.0 1.0 1.0")


        Dim ii, jj, kk As Integer
        Dim SS1, SS2 As String


        kk = 0
        For ii = 1 To gNACarg
            If NCat(iCat) > 1 Then
                'If GroupIndex(ii) = iCat Then                 'suppressed to compute all aircraft in 1 step by YGC 102213
                kk += 1
                SS1 = "lcd "
                SS1 = SS1 & CStr(kk) & " " & CStr(NCat(iCat) + 1)
                SS2 = " "

                'modified to reduce the length of a line in .ING by YGC 092512

                'For jj = 0 To Math.Min(8, NCat(iCat))
                '    If jj = kk Then
                '        SS2 = SS2 + CDbl(jj).ToString("0").Replace(",", ".") & " 1.0 "
                '    Else
                '        SS2 = SS2 + CDbl(jj).ToString("0").Replace(",", ".") & " 0.0 "
                '    End If
                'Next

                'SS1 = SS1 + SS2
                '' - PrintLine(7, SS1)

                ' - PrintLine(7, SS1)

                For jj = 0 To NCat(iCat)
                    If jj Mod 8 = 0 And jj > 0 Then
                        PrintLine(7)
                        If jj = kk Then
                            SS2 = CDbl(jj).ToString("0.0").Replace(",", ".") & " 1.0 "
                        Else
                            SS2 = CDbl(jj).ToString("0.0").Replace(",", ".") & " 0.0 "
                        End If
                        Print(7, SS2)
                    Else
                        If jj = kk Then
                            SS2 = CDbl(jj).ToString("0.0").Replace(",", ".") & " 1.0 "
                        Else
                            SS2 = CDbl(jj).ToString("0.0").Replace(",", ".") & " 0.0 "
                        End If
                        Print(7, SS2)
                    End If
                Next
                PrintLine(7)

                'modify ended to reduce the length of a line in .ING by YGC 092512


                'End If     'suppressed to compute all aircraft in 1 step by YGC 102213

                'modified to compute all aircraft in 1 step by YGC 102213
                'Else
                '    If GroupIndex(ii) = iCat Then
                '        ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
                '    End If
            ElseIf NCat(iCat) = 1 Then
                ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
                'modify ended to compute all aircraft in 1 step by YGC 102213

            End If
        Next ii


        '' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")


        '-----------------------------------------------
        IFOption = "sv"
        IFOption1 = "sl" 'sliding only - interface type 'LIA
        IFOption2 = "tied" 'tied sliding - interface type 'LIA
        IFOption3 = "sv" 'sliding with voids - interface type 'LIA


        ' added for partially bonded overlay by YC 100115
        If Factor2 > 0.001! Then
            Friction = 0.005#
        Else
            Friction = 0.0#
        End If
        ' added for partially bonded overlay by YC 100115 END

        Factor1 = 10
        ' - PrintLine(7, "si 1 " & IFOption3 & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & "0.0" & " nomerge;")


        ' modified for partially bonded overlay by YC 100115
        'If Overlay And Factor2 <= 0.1 Then 'Unbound Rigid Overlay
        '    ' - PrintLine(7, "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & "0.0" & " nomerge;")
        'End If

        If Overlay Then
            ' - PrintLine(7, "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;")
        End If
        ' modified for partially bonded overlay by YC 100115 END


        ' - PrintLine(7, "")

        'concrete slab material
        'PCC density is changed to 8.3912e-2 by YGC 101011
        '' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        ' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 8.3912e-2")
        'PCC density change end by YGC 101011

        'upper subbase material
        ' - PrintLine(7, "mat  2  1" & "  e " & LPad(11, Format(EMod(2), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(2), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '2nd subbase material
        ' - PrintLine(7, "mat  3  1" & "  e " & LPad(11, Format(EMod(3), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(3), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '3rd subbase material
        ' - PrintLine(7, "mat  4  1" & "  e " & LPad(11, Format(EMod(4), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(4), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '4th subbase material
        ' - PrintLine(7, "mat  5  1" & "  e " & LPad(11, Format(EMod(5), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(5), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        'subgrade material
        ' - PrintLine(7, "mat  6  1" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        If InfiniteElement Then
            'infinite direction downward (-z)
            ' - PrintLine(7, "mat  7 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 6")

            'infinite direction +x
            ' - PrintLine(7, "mat  8 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1")

            'infinite direction +y
            ' - PrintLine(7, "mat  9 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 2")

            'infinite direction -x
            ' - PrintLine(7, "mat 10 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 4")

            'ikawa If Not SymmSngl Then
            'infinite direction -y
            ' - PrintLine(7, "mat 11 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 5")
            'ikawa End If
        End If
        If Overlay Then
            ' - PrintLine(7, "mat 12 1" & "  e " & LPad(11, Format(EMod(0), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 8.3912e-2")   'Modified to read overlay density from .ING file by YGC 020612
            '& " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        End If

        ' - PrintLine(7, "endmat")
        ' - PrintLine(7, "")
    End Sub

    Sub Material_beforeOct1_2012()
        'subroutine for multiple aircraft in 1 step, full contact of AC on Rigid overlay by YGC 042312

        Dim IFOption, IFOption1, IFOption2, IFOption3 As String

        ' - PrintLine(7, "Rigid Airport Pavement")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "nk3d")
        ' - PrintLine(7, "anal stat")
        ' - PrintLine(7, "prcd 1 2 0 1.0 1.0 1.0")


        Dim ii, jj, kk As Integer
        Dim SS1, SS2 As String

        kk = 0
        For ii = 1 To gNACarg
            If NCat(iCat) > 1 Then
                If GroupIndex(ii) = iCat Then
                    kk += 1
                    SS1 = "lcd "
                    SS1 = SS1 & CStr(kk) & " " & CStr(NCat(iCat) + 1)
                    SS2 = " "

                    For jj = 0 To Math.Min(8, NCat(iCat))
                        If jj = kk Then
                            SS2 = SS2 + CDbl(jj).ToString("0.0").Replace(",", ".") & " 1.0 "
                        Else
                            SS2 = SS2 + CDbl(jj).ToString("0.0").Replace(",", ".") & " 0.0 "
                        End If
                    Next
                    SS1 = SS1 + SS2
                    ' - PrintLine(7, SS1)
                End If
            Else
                If GroupIndex(ii) = iCat Then
                    ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
                End If
            End If
        Next ii


        '' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")


        '-----------------------------------------------
        IFOption = "sv"
        IFOption1 = "sl" 'sliding only - interface type 'LIA
        IFOption2 = "tied" 'tied sliding - interface type 'LIA
        IFOption3 = "sv" 'sliding with voids - interface type 'LIA

        Factor1 = 10
        ' - PrintLine(7, "si 1 " & IFOption3 & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & "0.0" & " nomerge;")


        If Overlay And Factor2 <= 0.1 Then 'Unbound Rigid Overlay
            ' - PrintLine(7, "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & "0.0" & " nomerge;")
        End If
        ' - PrintLine(7, "")



        'concrete slab material
        'PCC density is changed to 8.3912e-2 by YGC 101011
        '' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        ''       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        ' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 8.3912e-2")
        'PCC density change end by YGC 101011

        'upper subbase material
        ' - PrintLine(7, "mat  2  1" & "  e " & LPad(11, Format(EMod(2), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(2), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '2nd subbase material
        ' - PrintLine(7, "mat  3  1" & "  e " & LPad(11, Format(EMod(3), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(3), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '3rd subbase material
        ' - PrintLine(7, "mat  4  1" & "  e " & LPad(11, Format(EMod(4), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(4), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '4th subbase material
        ' - PrintLine(7, "mat  5  1" & "  e " & LPad(11, Format(EMod(5), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(5), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        'subgrade material
        ' - PrintLine(7, "mat  6  1" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        If InfiniteElement Then
            'infinite direction downward (-z)
            ' - PrintLine(7, "mat  7 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 6")

            'infinite direction +x
            ' - PrintLine(7, "mat  8 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1")

            'infinite direction +y
            ' - PrintLine(7, "mat  9 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 2")

            'infinite direction -x
            ' - PrintLine(7, "mat 10 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 4")

            'ikawa If Not SymmSngl Then
            'infinite direction -y
            ' - PrintLine(7, "mat 11 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 5")
            'ikawa End If
        End If
        If Overlay Then
            ' - PrintLine(7, "mat 12 1" & "  e " & LPad(11, Format(EMod(0), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 8.3912e-2")   'Modified to read overlay density from .ING file by YGC 020612
            '& " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        End If

        ' - PrintLine(7, "endmat")
        ' - PrintLine(7, "")
    End Sub

    Sub MaterialTied()
        'subroutine for multiple aircraft in 1 step, automated by YGC 031312
        'tied contact of AC on Rigid overlay by YGC 042312

        Dim IFOption, IFOption1, IFOption2, IFOption3 As String

        ' - PrintLine(7, "Rigid Airport Pavement")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "nk3d")
        ' - PrintLine(7, "anal stat")
        ' - PrintLine(7, "prcd 1 2 0 1.0 1.0 1.0")


        Dim ii, jj, kk As Integer
        Dim SS1, SS2 As String

        kk = 0
        For ii = 1 To gNACarg
            If NCat(iCat) > 1 Then
                If GroupIndex(ii) = iCat Then
                    kk += 1
                    SS1 = "lcd "
                    SS1 = SS1 & CStr(kk) & " " & CStr(NCat(iCat) + 1)
                    SS2 = " "

                    For jj = 0 To Math.Min(8, NCat(iCat))
                        If jj = kk Then
                            SS2 = SS2 + CDbl(jj).ToString("0.0").Replace(",", ".") & " 1.0 "
                        Else
                            SS2 = SS2 + CDbl(jj).ToString("0.0").Replace(",", ".") & " 0.0 "
                        End If
                    Next
                    SS1 = SS1 + SS2
                    ' - PrintLine(7, SS1)
                End If
            Else
                If GroupIndex(ii) = iCat Then
                    ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
                End If
            End If
        Next ii


        '' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")


        '-----------------------------------------------
        IFOption = "sv"
        IFOption1 = "sl" 'sliding only - interface type 'LIA
        IFOption2 = "tied" 'tied sliding - interface type 'LIA
        IFOption3 = "sv" 'sliding with voids - interface type 'LIA

        If Factor2 > 0.001! Then
            Friction = 0.005
        Else
            Friction = 0.0#
        End If

        'ikawa If Factor2 > 99.0# Then IFOption = "tied"

        'Modified for "tied" contact with 0 friction by YGC 021312 
        'If Factor2 > 0.1 Then IFOption = "tied"  
        If Factor2 > 0.1 Then
            IFOption = "tied"
            Friction = 0.0#
        End If
        'Modify end by YGC 021312

        Factor1 = 10
        Factor3 = 10
        ' - PrintLine(7, "si 1 " & IFOption3 & " pnlt " & CStr(Factor3).Replace(",", ".") & " fric " & "0.0" & " nomerge;")

        If Overlay Then
            ' Change overlay index by YGC 060311
            '' - PrintLine(7, "si 2 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;")
            ' - PrintLine(7, "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;")
        End If
        ' - PrintLine(7, "")

        'concrete slab material
        'PCC density is changed to 8.3912e-2 by YGC 101011
        '' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        ''       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        ' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 8.3912e-2")
        'PCC density change end by YGC 101011

        'upper subbase material
        ' - PrintLine(7, "mat  2  1" & "  e " & LPad(11, Format(EMod(2), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(2), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '2nd subbase material
        ' - PrintLine(7, "mat  3  1" & "  e " & LPad(11, Format(EMod(3), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(3), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '3rd subbase material
        ' - PrintLine(7, "mat  4  1" & "  e " & LPad(11, Format(EMod(4), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(4), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '4th subbase material
        ' - PrintLine(7, "mat  5  1" & "  e " & LPad(11, Format(EMod(5), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(5), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        'subgrade material
        ' - PrintLine(7, "mat  6  1" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        If InfiniteElement Then
            'infinite direction downward (-z)
            ' - PrintLine(7, "mat  7 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 6")

            'infinite direction +x
            ' - PrintLine(7, "mat  8 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1")

            'infinite direction +y
            ' - PrintLine(7, "mat  9 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 2")

            'infinite direction -x
            ' - PrintLine(7, "mat 10 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 4")

            'ikawa If Not SymmSngl Then
            'infinite direction -y
            ' - PrintLine(7, "mat 11 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 5")
            'ikawa End If
        End If
        If Overlay Then
            ' - PrintLine(7, "mat 12 1" & "  e " & LPad(11, Format(EMod(0), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 8.3912e-2")   'Modified to read overlay density from .ING file by YGC 020612
            '& " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        End If

        ' - PrintLine(7, "endmat")
        ' - PrintLine(7, "")
    End Sub

    Sub Material1AC()
        'subroutine for 1 aircraft in 1 step, automated by YGC 031312


        Dim IFOption, IFOption1, IFOption2, IFOption3 As String

        ' - PrintLine(7, "Rigid Airport Pavement")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "nk3d")
        ' - PrintLine(7, "anal stat")
        ' - PrintLine(7, "prcd 1 2 0 1.0 1.0 1.0")

        'Modify for 1 AC by YGC 020612

        'Dim ii, jj, kk As Integer
        'Dim SS1, SS2 As String

        'kk = 0
        'For ii = 1 To gNACarg
        '    If NCat(iCat) > 1 Then
        '        If GroupIndex(ii) = iCat Then
        '            kk += 1
        '            SS1 = "lcd "
        '            SS1 = SS1 & CStr(kk) & " " & CStr(NCat(iCat) + 1)
        '            SS2 = " "

        '            For jj = 0 To Math.Min(8, NCat(iCat))
        '                If jj = kk Then
        '                    SS2 = SS2 + CDbl(jj).ToString("0.0").Replace(",", ".") & " 1.0 "
        '                Else
        '                    SS2 = SS2 + CDbl(jj).ToString("0.0").Replace(",", ".") & " 0.0 "
        '                End If
        '            Next
        '            SS1 = SS1 + SS2
        '            ' - PrintLine(7, SS1)
        '        End If
        '    Else
        '        If GroupIndex(ii) = iCat Then
        '            ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
        '        End If
        '    End If
        'Next ii


        ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
        'Modify end for 1 AC by YGC 020612

        '-----------------------------------------------
        IFOption = "sv"
        IFOption1 = "sl" 'sliding only - interface type 'LIA
        IFOption2 = "tied" 'tied sliding - interface type 'LIA
        IFOption3 = "sv" 'sliding with voids - interface type 'LIA

        If Factor2 > 0.001! Then
            Friction = 0.005
        Else
            Friction = 0.0#
        End If

        'ikawa If Factor2 > 99.0# Then IFOption = "tied"

        'Modified for "tied" contact with 0 friction by YGC 021312 
        'If Factor2 > 0.1 Then IFOption = "tied"  
        If Factor2 > 0.1 Then
            IFOption = "tied"
            Friction = 0.0#
        End If
        'Modify end by YGC 021312

        Factor1 = 10
        Factor3 = 10
        ' - PrintLine(7, "si 1 " & IFOption3 & " pnlt " & CStr(Factor3).Replace(",", ".") & " fric " & "0.0" & " nomerge;")

        If Overlay Then
            ' Change overlay index by YGC 060311
            '' - PrintLine(7, "si 2 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;")
            ' - PrintLine(7, "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;")
        End If
        ' - PrintLine(7, "")

        'concrete slab material
        'PCC density is changed to 8.3912e-2 by YGC 101011
        '' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        ''       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        ' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(1), "#0.00").Replace(",", ".") & " ro 8.3912e-2")
        'PCC density change end by YGC 101011

        'upper subbase material
        ' - PrintLine(7, "mat  2  1" & "  e " & LPad(11, Format(EMod(2), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(2), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '2nd subbase material
        ' - PrintLine(7, "mat  3  1" & "  e " & LPad(11, Format(EMod(3), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(3), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '3rd subbase material
        ' - PrintLine(7, "mat  4  1" & "  e " & LPad(11, Format(EMod(4), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(4), "#0.00").Replace(",", ".") & " ro 1.872e-4")
        '4th subbase material
        ' - PrintLine(7, "mat  5  1" & "  e " & LPad(11, Format(EMod(5), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(5), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        'subgrade material
        ' - PrintLine(7, "mat  6  1" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
        '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1.872e-4")

        If InfiniteElement Then
            'infinite direction downward (-z)
            ' - PrintLine(7, "mat  7 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 6")

            'infinite direction +x
            ' - PrintLine(7, "mat  8 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 1")

            'infinite direction +y
            ' - PrintLine(7, "mat  9 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 2")

            'infinite direction -x
            ' - PrintLine(7, "mat 10 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 4")

            'ikawa If Not SymmSngl Then
            'infinite direction -y
            ' - PrintLine(7, "mat 11 56" & "  e " & LPad(11, Format(EMod(6), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00").Replace(",", ".") & " ro 5")
            'ikawa End If
        End If
        If Overlay Then
            ' - PrintLine(7, "mat 12 1" & "  e " & LPad(11, Format(EMod(0), "####000.00").Replace(",", ".")) _
            '       & " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 8.3912e-2")   'Modified to read overlay density from .ING file by YGC 020612
            '& " pr " & Format(PoissonsRatio(0), "#0.00").Replace(",", ".") & " ro 2.247e-4")
        End If

        ' - PrintLine(7, "endmat")
        ' - PrintLine(7, "")
    End Sub

    Sub GearLoads(ByVal ACIndex As Short, ByVal LEAAircraft() As LEAFClassLib.clsLEAF.LEAFACParms)

        Dim xdim, ydim As Double

        Dim I As Integer, YTrMin, YTrMax, XTrMax, XTrMin As Double
        Dim NoXElsEven, NoYElsEven As Boolean

        NoXElsEven = False : NoYElsEven = False

        If frmGear.GearParallel = True Then 'if gear oriented parallel to joint

            Xfp = frmGear.WhlWidth : Yfp = frmGear.WhlLength

            '''''If LEAAircraft(ACIndex).ACname = "C-130" Then
            '''''    Dim tempX, tempY As Double
            '''''    tempX = Xfp
            '''''    tempY = Yfp

            '''''    Xfp = tempY
            '''''    Yfp = tempX
            '''''End If

            XTrMin = 0.0# : XTrMax = 0.0# : YTrMax = 0.0# : YTrMin = 0.0#
            For I = 1 To NWheels
                Xtr(I) = CDbl(YWheels(I) - frmGear.Ycg)
                Ytr(I) = CDbl(XWheels(I) - frmGear.Xcg)
                If Xtr(I) > XTrMax Then XTrMax = Xtr(I)
                If Xtr(I) < XTrMin Then XTrMin = Xtr(I)
                If Ytr(I) > YTrMax Then YTrMax = Ytr(I)
                If Ytr(I) < YTrMin Then YTrMin = Ytr(I)
            Next I
            xdim = XTrMax - XTrMin

            If Interior = True Then 'for interior case only
                xdim = XTrMax
                If NYSymmetric = NWheels And NXSymmetric = NWheels Then SymmDbl = True
                If SymmDbl = False Then GoTo 999
            End If
            ydim = YTrMax - YTrMin

            If Interior = False Then 'for edge case only
                If NYSymmetric = NWheels Then SymmSngl = True
                For I = 1 To NWheels
                    Xtr(I) = CDbl(Xtr(I) - XTrMin + Xfp / 2.0#)
                Next I
            End If

        ElseIf frmGear.GearAngle = 90.0# Then 'If gear oriented perpendicular to joint

            Yfp = frmGear.WhlWidth : Xfp = frmGear.WhlLength
            XTrMin = 0.0# : XTrMax = 0.0# : YTrMax = 0.0# : YTrMin = 0.0#
            For I = 1 To NWheels
                Xtr(I) = CDbl(XWheels(I) - frmGear.Xcg)
                Ytr(I) = CDbl(YWheels(I) - frmGear.Ycg)
                If Xtr(I) > XTrMax Then XTrMax = Xtr(I)
                If Xtr(I) < XTrMin Then XTrMin = Xtr(I)
                If Ytr(I) > YTrMax Then YTrMax = Ytr(I)
                If Ytr(I) < YTrMin Then YTrMin = Ytr(I)
            Next I
            xdim = XTrMax - XTrMin


            If Interior = True Then 'for interior case only
                xdim = XTrMax
                If NYSymmetric = NWheels And NXSymmetric = NWheels Then SymmDbl = True
                If SymmDbl = False Then GoTo 999
            End If
            ydim = YTrMax - YTrMin

            If Not Interior Then  'for edge case only
                If NXSymmetric = NWheels Then SymmSngl = True
                For I = 1 To NWheels
                    Xtr(I) = CDbl(Xtr(I) - XTrMin + Xfp / 2.0#)
                Next I
            End If

        Else
            SymmSngl = False
            SymmDbl = False
        End If



999:
    End Sub

    Sub Layer_SlabHexEdge()
        'to seperate sym and unsym gear by YC 082216 092016
        'to remove parameter definiton for MeshClassLib by YC 101215  092016

        NX1 = SlabFineMeshNumX
        NX2 = 6
        NY1 = SlabFineMeshNumY
        NY2 = 6

        tkpcc = -LayerThickness(1)
        tkol = -LayerThickness(0)


        GoTo newslablabel   'YC 040517

        If Overlay And Factor2 > 0.1 Then  'AC Overlay 

            If MeshCase = "SYM" Then
                INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "1 " & (1 + NY1) & " " & (1 + NY1 + NY2) & ";" & vbCrLf &
                "1 2 3;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & " " & -tkpcc - tkol & vbCrLf &
                "si- 1 1 1 3 3 1 1 s 0 0 1" & vbCrLf &
                "mt 1 1 1 3 3 2 1" & vbCrLf &
                "mt 1 1 2 3 3 3 12" & vbCrLf &
                "end" & vbCrLf & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "1 " & (1 + NY2) & " " & (1 + NY2 + NY1) & " " & (1 + NY2 + 2 * NY1) & " " & (1 + 2 * NY2 + 2 * NY1) & ";" & vbCrLf &
                "1 2 3;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "-150 -60 0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & " " & -tkpcc - tkol & vbCrLf &
                "si- 1 1 1 3 5 1 1 s 0 0 1" & vbCrLf &
                "mt 1 1 1 3 5 2 1" & vbCrLf &
                "mt 1 1 2 3 5 3 12" & vbCrLf &
                "end" & vbCrLf & vbCrLf
            End If

        Else 'PCC overlay or New PCC
            If Overlay = True Then

                If MeshCase = "SYM" Then
                    INGString = INGString & "start" & vbCrLf &
                    "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                    "1 " & (1 + NY1) & " " & (1 + NY1 + NY2) & ";" & vbCrLf &
                    "1 2;" & vbCrLf &
                    "0.0 210 300.0" & vbCrLf &
                    "0.0 60 150.0" & vbCrLf &
                    -tkpcc & " " & -tkpcc - tkol & vbCrLf &
                    "si- 1 1 1 3 3 1 10 s 0 0 100" & vbCrLf &
                    "mate 12" & vbCrLf &
                    "end" & vbCrLf & vbCrLf
                ElseIf MeshCase = "UNSYM" Then
                    INGString = INGString & "start" & vbCrLf &
                    "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                    "1 " & (1 + NY2) & " " & (1 + NY2 + NY1) & " " & (1 + NY2 + 2 * NY1) & " " & (1 + 2 * NY2 + 2 * NY1) & ";" & vbCrLf &
                    "1 2;" & vbCrLf &
                    "0.0 210 300.0" & vbCrLf &
                    "-150 -60 0.0 60 150.0" & vbCrLf &
                    -tkpcc & " " & -tkpcc - tkol & vbCrLf &
                    "si- 1 1 1 3 5 1 10 s 0 0 100" & vbCrLf &
                    "mate 12" & vbCrLf &
                    "end" & vbCrLf & vbCrLf
                End If

            End If

            If MeshCase = "SYM" Then
                INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "1 " & (1 + NY1) & " " & (1 + NY1 + NY2) & ";" & vbCrLf &
                "1 2;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & vbCrLf &
                "si- 1 1 1 3 3 1 1 s 0 0 1" & vbCrLf

                If Overlay = True Then INGString = INGString & "si+ 1 1 2 3 3 2 10 m 0 0 100" & vbCrLf

            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "1 " & (1 + NY2) & " " & (1 + NY2 + NY1) & " " & (1 + NY2 + 2 * NY1) & " " & (1 + 2 * NY2 + 2 * NY1) & ";" & vbCrLf &
                "1 2;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "-150 -60 0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & vbCrLf &
                "si- 1 1 1 3 5 1 1 s 0 0 1" & vbCrLf

                If Overlay = True Then INGString = INGString & "si+ 1 1 2 3 5 2 10 m 0 0 100" & vbCrLf

            End If

            INGString = INGString & "mate 1" & vbCrLf &
                  "end" & vbCrLf & vbCrLf

        End If


newslablabel:  'YC 040517
        'underlay slab
        If MeshCase = "SYM" Then
            INGString = INGString & "start" & vbCrLf &
            "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
            "1 " & (1 + NY1) & " " & (1 + NY1 + NY2) & ";" & vbCrLf &
            "1 2;" & vbCrLf &
            "0.0 210 300.0" & vbCrLf &
            "0.0 60 150.0" & vbCrLf &
            "0 " & -tkpcc & vbCrLf &
            "si- 1 1 1 3 3 1 1 s 0 0 1" & vbCrLf

            If Overlay = True And Factor2 <= 0.1 Then INGString = INGString & "si+ 1 1 2 3 3 2 10 m 0 0 100" & vbCrLf 'PCC overlay

        ElseIf MeshCase = "UNSYM" Then
            INGString = INGString & "start" & vbCrLf &
            "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
            "1 " & (1 + NY2) & " " & (1 + NY2 + NY1) & " " & (1 + NY2 + 2 * NY1) & " " & (1 + 2 * NY2 + 2 * NY1) & ";" & vbCrLf &
            "1 2;" & vbCrLf &
            "0.0 210 300.0" & vbCrLf &
            "-150 -60 0.0 60 150.0" & vbCrLf &
            "0 " & -tkpcc & vbCrLf &
            "si- 1 1 1 3 5 1 1 s 0 0 1" & vbCrLf

            If Overlay = True And Factor2 <= 0.1 Then INGString = INGString & "si+ 1 1 2 3 5 2 10 m 0 0 100" & vbCrLf 'PCC overlay

        End If

        INGString = INGString & "mate 1" & vbCrLf &
              "end" & vbCrLf & vbCrLf


        If Overlay Then 'Overlay

            If MeshCase = "SYM" Then
                INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "1 " & (1 + NY1) & " " & (1 + NY1 + NY2) & ";" & vbCrLf &
                "1 2;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "0.0 60 150.0" & vbCrLf &
                -tkpcc & " " & -tkpcc - tkol & vbCrLf

                If Overlay = True And Factor2 <= 0.1 Then INGString = INGString & "si- 1 1 1 3 3 1 10 s 0 0 100" & vbCrLf 'PCC overlay

            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "1 " & (1 + NY2) & " " & (1 + NY2 + NY1) & " " & (1 + NY2 + 2 * NY1) & " " & (1 + 2 * NY2 + 2 * NY1) & ";" & vbCrLf &
                "1 2;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "-150 -60 0.0 60 150.0" & vbCrLf &
                -tkpcc & " " & -tkpcc - tkol & vbCrLf

                If Overlay = True And Factor2 <= 0.1 Then INGString = INGString & "si- 1 1 1 3 5 1 10 s 0 0 100" & vbCrLf 'PCC overlay

            End If


            INGString = INGString & "mate 12" & vbCrLf &
                "end" & vbCrLf & vbCrLf

        End If


    End Sub

    Sub Layer_SlabHexEdge1p41()
        'used in version up to FF 2016.09.20 v1.41.0011, renamed by YC 101215 092016
        ' sub Layer_SlabHexEdge for FF1.4 non-uniform slab mesh by YGC 092613 

        ' Parameters
        ' - PrintLine(7, "Parameter")
        ' - PrintLine(7, "tkpcc " & CStr(-LayerThickness(1)).Replace(",", ".") _
        '       & " tkol " & CStr(-LayerThickness(0)).Replace(",", "."))

        'modified to decrease element number for fine area by YGC 072214 
        '' - PrintLine(7, "NX1 56 NX2 6 ")
        '' - PrintLine(7, "NY1 16 NY2 6 ")
        ' - PrintLine(7, "NX1 " & CStr(SlabFineMeshNumX) & " NX2 6 ")
        ' - PrintLine(7, "NY1 " & CStr(SlabFineMeshNumY) & " NY2 6 ")
        'modified to decrease element number for fine area by YGC 072214 END

        ' - PrintLine(7, ";")
        ' - PrintLine(7, " ")

        If Overlay And Factor2 > 0.1 Then  'AC Overlay 
            ' - PrintLine(7, "start")

            ' - PrintLine(7, "1 [1+%NX1] [1+%NX1+%NX2];1 [1+%NY1] [1+%NY1+%NY2];1 2 3;")
            ' - PrintLine(7, "0.0 210 300.0")
            ' - PrintLine(7, "0.0 60 150.0")
            ' - PrintLine(7, "0 [-%tkpcc][-%tkpcc - %tkol]")

            ' - PrintLine(7, "si- 1 1 1 3 3 1 1 s 0 0 1")

            ' - PrintLine(7, "mt 1 1 1 3 3 2 1")
            ' - PrintLine(7, "mt 1 1 2 3 3 3 12")
            ' - PrintLine(7, "end")
            ' - PrintLine(7, "")

        Else 'PCC overlay or New PCC
            If Overlay = True Then
                ' - PrintLine(7, "start")

                ' - PrintLine(7, "1 [1+%NX1] [1+%NX1+%NX2];1 [1+%NY1] [1+%NY1+%NY2];1 2;")
                ' - PrintLine(7, "0.0 210 300.0")
                ' - PrintLine(7, "0.0 60 150.0")
                ' - PrintLine(7, "[-%tkpcc][-%tkpcc - %tkol]")

                ' - PrintLine(7, "si- 1 1 1 3 3 1 10 s 0 0 100") ' Change overlay index by YGC 060311

                ' - PrintLine(7, "mate 12")
                ' - PrintLine(7, "end")
                ' - PrintLine(7, "")
            End If


            ' - PrintLine(7, "start")
            'create the base slab, or slab if no overlay

            ' - PrintLine(7, "1 [1+%NX1] [1+%NX1+%NX2];1 [1+%NY1] [1+%NY1+%NY2];1 2;")
            ' - PrintLine(7, "0.0 210 300.0")
            ' - PrintLine(7, "0.0 60 150.0")
            ' - PrintLine(7, "0 [-%tkpcc]")

            ' - PrintLine(7, "si- 1 1 1 3 3 1 1 s 0 0 1")

            If Overlay = True Then 'define a contact surface on top of the base slab
                ' - PrintLine(7, "si+ 1 1 2 3 3 2 10 m 0 0 100") ' Change overlay index by YGC 060311
            End If

            ' - PrintLine(7, "mate 1")
            ' - PrintLine(7, "end")
            ' - PrintLine(7, "")

        End If


    End Sub

    Sub Layer_SlabHexEdgeFF1p4()
        ' sub Layer_SlabHexEdge for FF1.4, full contact of AC on Rigid by YGC 042312
        'Renamed by YGC 092613


        ' Parameters
        ' - PrintLine(7, "Parameter tkpcc " & CStr(-LayerThickness(1)).Replace(",", ".") _
        '       & " tkol " & CStr(-LayerThickness(0)).Replace(",", ".") & ";")

        If Overlay And Factor2 > 0.1 Then  'AC Overlay 
            ' - PrintLine(7, "start")
            If SymmSngl = True Then
                ' - PrintLine(7, "1 " & NoPts1 & "; 1 " & NoPts2 & "; 1 2 3;")
            Else
                ' - PrintLine(7, "1 " & NoPts1 & "; 1 " & NoPts1 & "; 1 2 3;")
            End If

            ' - PrintLine(7, "0.0 " & CStr(frmStructure.XDimension * 12).Replace(",", "."))


            If SymmSngl = True Then
                ' - PrintLine(7, "0.0 " & CStr(frmStructure.YDimension * 12 / 2).Replace(",", "."))
            Else
                ' - PrintLine(7, "" & CStr(-frmStructure.YDimension * 12 / 2).Replace(",", ".") & " " & CStr(frmStructure.YDimension * 12 / 2).Replace(",", "."))
            End If
            ' - PrintLine(7, "0 [-%tkpcc][-%tkpcc - %tkol]")
            ' - PrintLine(7, "si- 1 1 1 2 2 1 1 s 0 0 1")

            ' - PrintLine(7, "mt 1 1 1 2 2 2 1")
            ' - PrintLine(7, "mt 1 1 2 2 2 3 12")
            ' - PrintLine(7, "end")
            ' - PrintLine(7, "")

        Else 'PCC overlay or New PCC
            If Overlay = True Then
                ' - PrintLine(7, "start")
                If SymmSngl = True Then
                    ' - PrintLine(7, "1 " & NoPts1 & "; 1 " & NoPts2 & "; 1 2;")
                Else
                    ' - PrintLine(7, "1 " & NoPts1 & "; 1 " & NoPts1 & "; 1 2;")
                End If


                ' - PrintLine(7, "0.0 " & CStr(frmStructure.XDimension * 12).Replace(",", "."))


                If SymmSngl = True Then
                    ' - PrintLine(7, "0.0 " & CStr(frmStructure.YDimension * 12 / 2).Replace(",", "."))
                Else
                    ' - PrintLine(7, "" & CStr(-frmStructure.YDimension * 12 / 2).Replace(",", ".") & " " & CStr(frmStructure.YDimension * 12 / 2).Replace(",", "."))
                End If
                ' - PrintLine(7, "[-%tkpcc][-%tkpcc - %tkol]")

                '' - PrintLine(7, "si- 1 1 1 2 2 1 2 s 0 0 100")
                ' - PrintLine(7, "si- 1 1 1 2 2 1 10 s 0 0 100") ' Change overlay index by YGC 060311

                'If SymmSngl = True Then ' - PrintLine(7, "b 1 1 1 2 1 2 010000") 'added for symmetrical BC by YGC 060611

                ' - PrintLine(7, "mate 12")
                ' - PrintLine(7, "end")
                ' - PrintLine(7, "")
            End If


            ' - PrintLine(7, "start")
            'create the base slab, or slab if no overlay
            If SymmSngl = True Then
                ' - PrintLine(7, "1 " & NoPts1 & "; 1 " & NoPts2 & "; 1 2;")
            Else
                ' - PrintLine(7, "1 " & NoPts1 & "; 1 " & NoPts1 & "; 1 2;")
            End If

            ' - PrintLine(7, "0.0 " & CStr(frmStructure.XDimension * 12).Replace(",", "."))

            If SymmSngl = True Then
                ' - PrintLine(7, "0.0 " & CStr(frmStructure.YDimension * 12 / 2).Replace(",", "."))
            Else
                ' - PrintLine(7, "" & CStr(-frmStructure.YDimension * 12 / 2).Replace(",", ".") & " " & CStr(frmStructure.YDimension * 12 / 2).Replace(",", "."))
            End If

            ' - PrintLine(7, "0 [-%tkpcc]")

            ' - PrintLine(7, "si- 1 1 1 2 2 1 1 s 0 0 1")

            ' If SymmSngl = True Then ' - PrintLine(7, "b 1 1 1 2 1 2 010000") 'added for symmetrical BC by YGC 060611

            'Suppressed to replace fixed corner node BC by spring BC by YGC 060311
            'If SymmSngl = True Then
            '    ' - PrintLine(7, "b 2 2 2 2 2 2 110000")
            'Else
            '    ' - PrintLine(7, "b 2 1 2 2 1 2 110000")
            '    ' - PrintLine(7, "b 2 2 2 2 2 2 110000")
            'End If
            'Suppress end by YGC 060311

            If Overlay = True Then 'define a contact surface on top of the base slab
                '' - PrintLine(7, "si+ 1 1 2 2 2 2 2 m 0 0 100")
                ' - PrintLine(7, "si+ 1 1 2 2 2 2 10 m 0 0 100") ' Change overlay index by YGC 060311
            End If

            ' - PrintLine(7, "mate 1")
            ' - PrintLine(7, "end")
            ' - PrintLine(7, "")



        End If


    End Sub

    'Sub Layer_SlabHexEdge()
    Sub Layer_SlabHexEdgeFF1p3()
        'sub Layer_SlabHexEdge for FF1.3 comment by YGC 010611

        If MeshCase = "1DSYM" Then
            NumBlcksFineMeshX = 12 : NumBlcksFineMeshY = 8
            NumBlcksCrseMeshX = 8 : NumBlcksCrseMeshY = 4
            c = 60
            e = 40
            'c = gFineMeshSize * NumBlcksFineMeshX '60
            'e = gFineMeshSize * NumBlcksFineMeshY '40
        ElseIf MeshCase = "2DSYM" Then
            NumBlcksFineMeshX = 20 : NumBlcksFineMeshY = 16
            NumBlcksCrseMeshX = 7 : NumBlcksCrseMeshY = 2
            'c = 100 : e = 80
            c = 120 : e = 96  'Modified by YGC 030311
        ElseIf MeshCase = "3DSYM" Then
            NumBlcksFineMeshX = 36 : NumBlcksFineMeshY = 12
            NumBlcksCrseMeshX = 4 : NumBlcksCrseMeshY = 3
            c = 180 : e = 60
        ElseIf MeshCase = "4DNSY" Then
            NumBlcksFineMeshX = 28 : NumBlcksFineMeshY = 20
            NumBlcksCrseMeshX = 5 : NumBlcksCrseMeshY = 2
            c = 140 : e = 100
        ElseIf MeshCase = "5DSYM" Then
            NumBlcksFineMeshX = 36 : NumBlcksFineMeshY = 12
            NumBlcksCrseMeshX = 4 : NumBlcksCrseMeshY = 3

            'NumBlcksFineMeshX = 48 : NumBlcksFineMeshY = 12
            'NumBlcksCrseMeshX = 3 : NumBlcksCrseMeshY = 3
            c = 240 : e = 60
            'c = 240 * 3 / 4 : e = 60 * 3 / 4
        End If


        If MeshCase = "1DSYM" Then
            fff2 = 2 : fff5 = 1 : fff7 = 5
        ElseIf MeshCase = "2DSYM" Then
            fff2 = 1 : fff5 = 1 : fff7 = 5
        ElseIf MeshCase = "3DSYM" Then
            NumBlcksFineMeshX = 30 : NumBlcksFineMeshY = 10
            NumBlcksCrseMeshX = 5 : NumBlcksCrseMeshY = 4

            'NumBlcksFineMeshX = 50 : NumBlcksFineMeshY = 25  'Modified by YGC 030311

            'c = CDbl(gFineMeshSize * NumBlcksFineMeshX / 1.2)
            'e = CDbl(gFineMeshSize * NumBlcksFineMeshY / 1.2)

            c = CDbl(gFineMeshSize * NumBlcksFineMeshX / XScaleFactor)
            e = CDbl(gFineMeshSize * NumBlcksFineMeshY / YScaleFactor)


            If gFineMeshSize = 12 Then : NumBlcksCrseMeshX = 4 : NumBlcksCrseMeshY = 2 : End If
            If gFineMeshSize = 2 Then : NumBlcksCrseMeshX = CShort(NumBlcksCrseMeshX / 2) : NumBlcksCrseMeshY = CShort(NumBlcksCrseMeshY / 2) : End If

        ElseIf MeshCase = "4DNSY" Then
            fff2 = 1 : fff5 = 1 : fff7 = 3
        ElseIf MeshCase = "5DSYM" Then

            NumBlcksFineMeshX = 30 : NumBlcksFineMeshY = 10
            NumBlcksCrseMeshX = 40 : NumBlcksCrseMeshY = 4 ' 50x50

            NumBlcksCrseMeshX = CShort((frmStructure.XDimension * 12 - NumBlcksFineMeshX * gFineMeshSize) / (6 * 2))
            c = CDbl(gFineMeshSize * NumBlcksFineMeshX / XScaleFactor)
            e = CDbl(gFineMeshSize * NumBlcksFineMeshY / YScaleFactor)

            If gFineMeshSize = 12 Then : NumBlcksCrseMeshX = 4 : NumBlcksCrseMeshY = 2 : End If
            If gFineMeshSize = 2 Then : NumBlcksCrseMeshX = CShort(NumBlcksCrseMeshX / 2) : NumBlcksCrseMeshY = CShort(NumBlcksCrseMeshY / 2) : End If

        End If



        xlen1 = frmStructure.XDimension * 12 + 0.1! 'in inches
        ylen1 = frmStructure.YDimension * 12 / 2 + 0.1! 'in inches
        xlen2 = 24 * XScaleFactor + 0.1!  ' needs to = f24


        ' Parameters
        ' - PrintLine(7, "parameter tkpcc " & CStr(-LayerThickness(1)).Replace(",", ".") _
        '       & " tkol " & CStr(-LayerThickness(0)).Replace(",", ".") & " cc " & CStr(c).Replace(",", ".") & " ee " & CStr(e).Replace(",", "."))
        ' - PrintLine(7, " NFX " & NumBlcksFineMeshX & " NFY " & NumBlcksFineMeshY _
        '       & " NCX " & NumBlcksCrseMeshX & " NCY " & NumBlcksCrseMeshY)
        ' - PrintLine(7, " fff2 " & CStr(fff2).Replace(",", ".") & " fff5 " & CStr(fff5).Replace(",", ".") & " fff7 " & CStr(fff7).Replace(",", ".")) ' & ";")
        ' - PrintLine(7, " xlen1 " & CStr(xlen1).Replace(",", ".") & " ylen1 " & CStr(ylen1).Replace(",", ".") & " xlen2 " & CStr(xlen2).Replace(",", ".") & ";")
        ' - PrintLine(7, "")






        Dim cc2 As Double, NFX2, NFX3 As Integer

        NFX2 = 50
        cc2 = CDbl(gFineMeshSize * (50 + NumBlcksFineMeshX) / XScaleFactor)
        NFX3 = CShort((frmStructure.XDimension * 12 - (NumBlcksFineMeshX + NFX2) * gFineMeshSize) / (6 * 2))

        If MeshCase = "5DSYM" Then
            '' - PrintLine(7, "parameter NFX2 46 NFX3 2 cc2 285 ;")
            '' - PrintLine(7, "parameter NFX2 46 NFX3 6 cc2 228 ;")
            '' - PrintLine(7, "parameter NFX2 54 NFX3 4 cc2 252 ;")
            '' - PrintLine(7, "parameter NFX2 50 NFX3 5 cc2 218.18 ;")
            ' - PrintLine(7, "parameter NFX2 50 NFX3 " & NFX3 & " cc2 " & CStr(cc2).Replace(",", ".") & " ;")
        End If







        'slab 1 - loaded slab
        If Overlay = True Then 'create the overlay layer
            ' - PrintLine(7, "start")
            If SymmSngl = True Then
                If Not MeshCase = "5DSYM" Then
                    ' - PrintLine(7, "1 [1 + %NFX] [1 + %NFX + %NCX]; 1 [1 + %NFY] [1 + %NFY + %NCY]; 1 2;")
                Else
                    ' - PrintLine(7, "1 [1 + %NFX] [1 + %NFX + %NFX2] [1 + %NFX + %NFX2 + %NFX3];")
                    ' - PrintLine(7, "1 [1 + %NFY] [1 + %NFY + %NCY]; 1 2;")
                End If
            Else
                ' - PrintLine(7, "1 [1 + %NFX] [1 + %NFX + %NCX]; ")
                ' - PrintLine(7, "1 [1 + %NCY] [1 + %NCY + 2 * %NFY] [1 + 2 * %NCY + 2 * %NFY] ; 1 2;")
            End If

            '' - PrintLine(7, "0.0 %cc 300.")
            If Not MeshCase = "5DSYM" Then
                ' - PrintLine(7, "0.0 %cc " & CStr(gSlabSize).Replace(",", "."))
            Else
                ' - PrintLine(7, "0.0 %cc %cc2 " & CStr(gSlabSize).Replace(",", "."))
            End If


            If SymmSngl = True Then
                '' - PrintLine(7, " 0.0 %ee 150.")
                ' - PrintLine(7, " 0.0 %ee " & CStr(gSlabSize / 2).Replace(",", "."))
            Else
                '' - PrintLine(7, " -150. [-%ee] %ee 150.")
                ' - PrintLine(7, "" & CStr(-gSlabSize / 2).Replace(",", ".") & " [-%ee] %ee " & CStr(gSlabSize / 2).Replace(",", "."))
            End If
            ' - PrintLine(7, "[-%tkpcc][-%tkpcc - %tkol]")

            If SymmSngl = True Then
                If Not MeshCase = "5DSYM" Then
                    ' - PrintLine(7, "si- 1 1 1 3 3 1 2 s 0 0 100")
                Else
                    ' - PrintLine(7, "si- 1 1 1 4 3 1 2 s 0 0 100")
                End If

            Else
                ' - PrintLine(7, "si- 1 1 1 3 4 1 2 s 0 0 100")
            End If
            ' - PrintLine(7, "mate 12")
            ' - PrintLine(7, "end")
            ' - PrintLine(7, "")
        End If


        ' - PrintLine(7, "start")
        'create the base slab, or slab if no overlay
        If SymmSngl = True Then

            If Not MeshCase = "5DSYM" Then
                ' - PrintLine(7, "1 [1 + %NFX] [1 + %NFX + %NCX]; 1 [1 + %NFY] [1 + %NFY + %NCY]; 1 2;")
            Else
                ' - PrintLine(7, "1 [1 + %NFX] [1 + %NFX + %NFX2] [1 + %NFX + %NFX2 + %NFX3];")
                ' - PrintLine(7, "1 [1 + %NFY] [1 + %NFY + %NCY]; 1 2;")
            End If

        Else
            ' - PrintLine(7, "1 [1 + %NFX] [1 + %NFX + %NCX]; ")
            ' - PrintLine(7, "1 [1 + %NCY] [1 + %NCY + 2 * %NFY] [1 + 2 * %NCY + 2 * %NFY] ; 1 2;")
        End If

        'Print(7, "0.0 %cc 300.")
        If Not MeshCase = "5DSYM" Then
            ' - PrintLine(7, "0.0 %cc " & CStr(gSlabSize).Replace(",", "."))
        Else
            ' - PrintLine(7, "0.0 %cc %cc2 " & CStr(gSlabSize).Replace(",", "."))
        End If

        If SymmSngl = True Then
            'Print(7, " 0.0 %ee 150.")
            Print(7, " 0.0 %ee " & CStr(gSlabSize / 2).Replace(",", "."))
        Else
            'Print(7, " -150. [-%ee] %ee 150.")
            ' - PrintLine(7, "" & CStr(-gSlabSize / 2).Replace(",", ".") & " [-%ee] %ee " & CStr(gSlabSize / 2).Replace(",", "."))
        End If
        ' - PrintLine(7, " 0 [-%tkpcc]")


        If SymmSngl = True Then

            If Not MeshCase = "5DSYM" Then
                ' - PrintLine(7, "si- 1 1 1 3 3 1 1 s 0 0 1")
                '' - PrintLine(7, "b 1 3 1 1 3 1 110000")
                ' - PrintLine(7, "b 3 3 2 3 3 2 110000")
            Else
                ' - PrintLine(7, "si- 1 1 1 4 3 1 1 s 0 0 1")
                ' - PrintLine(7, "b 4 3 2 4 3 2 110000")
            End If


        Else
            ' - PrintLine(7, "si- 1 1 1 3 4 1 1 s 0 0 1")
            ' - PrintLine(7, "b 3 1 2 3 1 2 110000")
            ' - PrintLine(7, "b 3 4 2 3 4 2 110000")
        End If

        If Overlay = True Then 'define a contact surface on top of the base slab
            If SymmSngl = True Then
                If Not MeshCase = "5DSYM" Then
                    ' - PrintLine(7, "si+ 1 1 2 3 3 2 2 m 0 0 100")
                Else
                    ' - PrintLine(7, "si+ 1 1 2 4 3 2 2 m 0 0 100")
                End If


            Else
                ' - PrintLine(7, "si+ 1 1 2 3 4 2 2 m 0 0 100")
            End If
        End If
        ' - PrintLine(7, "mate 1")
        ' - PrintLine(7, "end")
        ' - PrintLine(7, "")

    End Sub

    Sub WriteTextFile(ByVal OpenModeV As OpenMode, ByVal TextFileName As String)

        'modified to public parametr by YC 101215 092016
        'Dim I As Short, IndxModes As Short
        'Dim IndxSymm, IndxFndn As Short
        'Dim IndxOvrl As Short 'added by YGC 060311
        Dim I As Short
        'modified to public parametr by YC 101215 092016 END


        'source code for full contact of AC on Rigid overlay by YGC 042312

        NSlabs = 1

        ' added for slab mesh selection by YGC 061113
        NoPts1 = Convert.ToInt32(frmStructure.XDimension * 12 / SlabMeshSize) + 1
        NoPts2 = Convert.ToInt32((NoPts1 + 1) / 2)
        ' added for slab mesh selection by YGC 061113 END


        ' modfied for FF1.4 non-uniform mesh by YGC 092613
        'Modified for FF1.4 uniform mesh by YGC 020911
        'Xgrid = gFineMeshSize / XScaleFactor
        'Ygrid = gFineMeshSize / YScaleFactor
        'Xgrid = frmStructure.XDimension * 12 / (NoPts1 - 1)
        'Ygrid = frmStructure.YDimension * 12 / (NoPts1 - 1)

        'modified for decreased element number for fine area by YGC 072214 
        'reactivated scalae factor in mesh desnity by YGC 112213 
        'Xgrid = 3
        'Ygrid = 3
        'Xgrid = 3.75
        'Ygrid = 3.75
        Xgrid = CDbl(210 / SlabFineMeshNumX)
        Ygrid = CDbl(60 / SlabFineMeshNumY)
        'reactivate end scalae factor in mesh desnity by YGC 112213 
        'modified for decreased element number for fine area by YGC 072214 END

        'Modify end by YGC 020911
        'Modify end by YGC 092613

        'FileOpen(8, TextFileName, OpenModeV)           ' QW 09-15-2019

        'Suppressed to be in consistence with FEAFAA by YGC060311
        If OpenModeV = OpenMode.Output Then
            ' - PrintLine(8 SolverType) 'added for solver choice by YGC 083012
            ' - PrintLine(8 NCat(iCat))
        End If
        'Suppress end by YGC060311

        ' - PrintLine(8 NWheels)
        ' - PrintLine(8 CStr(Xgrid).Replace(",", "."))
        ' - PrintLine(8 CStr(Ygrid).Replace(",", "."))
        ' - PrintLine(8 CStr(TirePressure).Replace(",", "."))

        For I = 1 To NWheels
            ' - Print(8 VB6.Format(Xtr(I), "##0.000").Replace(",", ".") & " ")
            ' - PrintLine(8 VB6.Format(Ytr(I), "##0.000").Replace(",", "."))
        Next I

        ' - PrintLine(8 VB6.Format(Xfp, "##0.000").Replace(",", "."))
        ' - PrintLine(8 VB6.Format(Yfp, "##0.000").Replace(",", "."))

        IndxSymm = 0
        ' to seperate sym/unsym aircraft by YC 082216 092016
        'If SymmDbl = True Then
        '    IndxSymm = 2
        'ElseIf SymmSngl = True Then
        '    IndxSymm = 1
        'End If
        If MeshCase = "SYM" Then IndxSymm = 1
        ' to seperate sym/unsym aircraft by YC 082216 092016 END

        ' - PrintLine(8 VB6.Format(IndxSymm, "0"))

        'Modified by YGC 021111
        '' - PrintLine(8 VB6.Format(XScaleFactor, "0.000").Replace(",", "."))
        '' - PrintLine(8 VB6.Format(YScaleFactor, "0.000").Replace(",", "."))
        ' - PrintLine(8 VB6.Format(frmStructure.XScaleFactor, "0.000").Replace(",", "."))
        ' - PrintLine(8 VB6.Format(frmStructure.YScaleFactor, "0.000").Replace(",", "."))
        'Modify end by YGC 021111

        ' - PrintLine(8 VB6.Format(NSlabs, "0"))
        'Interface stiffness scale factors
        ' - PrintLine(8 CStr(Factor1).Replace(",", ".") & " " & CStr(Factor2).Replace(",", "."))
        'Angle that CL of Gear makes wrt Joint
        ' - PrintLine(8 VB6.Format(frmGear.GearAngle, "#0.00").Replace(",", "."))
        'Equivalent Joint Stiffness

        ' Modified to combine the spring BC for finite slab and infinte slab model by YGC 112213	
        'modified for spring BC to simulate infinite slab by YGC 092613
        'Modified to be in consistence with FEAFAA by YGC060311
        '' - PrintLine(8 VB6.Format(frmStructure.EqJStfX, "######0.0").Replace(",", "."))
        '' - PrintLine(8 VB6.Format(frmStructure.EqJStfX, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqJStfY, "######0.0").Replace(",", "."))
        '' - PrintLine(8 VB6.Format(frmStructure.EqJStfX, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqJStfY, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqJStf12X, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqJStf12Y, "######0.0").Replace(",", "."))
        ' - PrintLine(8 VB6.Format(frmStructure.EqJStfX, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqJStfY, "######0.0").Replace(",", ".") & " " & VB6.Format(KeyEqJStf, "0"))
        ' - PrintLine(8 VB6.Format(frmStructure.EqJStf12X, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqJStf12Y, "######0.0").Replace(",", ".") & " " & VB6.Format(KeyEqJStf12, "0"))
        'modify ended for spring BC to simulate infinite slab by YGC 092613
        ' Modify ended to combine the spring BC for finite slab and infinte slab model by YGC 112213	


        'modified to suppress corner spring when using horizontal spring to simulate infinite slab by YGC 092613
        '' - PrintLine(8 VB6.Format(frmStructure.EqEdgStfX, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqEdgStfY, "######0.0").Replace(",", ".")) 
        ' - PrintLine(8 VB6.Format(frmStructure.EqEdgStfX, "######0.0").Replace(",", ".") & " " & VB6.Format(frmStructure.EqEdgStfY, "######0.0").Replace(",", ".") & " " & VB6.Format(KeyEqEdgStf, "0"))
        'modify ended to suppress corner spring when using horizontal spring to simulate infinite slab by YGC 092613


        'Incompatible Modes?
        IndxModes = 0
        IndxModes = 1
        ' - PrintLine(8 VB6.Format(IndxModes, "0"))

        IndxFndn = 2 'Infinite Element Foundation
        ' - PrintLine(8 VB6.Format(IndxFndn, "0"))

        'Modified to be in consistence with FEAFAA by YGC060311 
        '' - PrintLine(8 VB6.Format(0, "0"))

        'Modified to print thickness consistently by YGC 032312
        '' - PrintLine(8 VB6.Format(LayerThickness(1), "######0.0").Replace(",", "."))
        '' - PrintLine(8 LayerThickness(1)) 'corrected by YC 040517
        ' - PrintLine(8 VB6.Format(LayerThickness(1), "0.000").Replace(",", "."))

        If Overlay And Factor2 <= 0.1 Then  'Interface only for UnBound PCC Overlay by YGC 042312
            IndxOvrl = 1
        Else
            IndxOvrl = 0
        End If

        ' - PrintLine(8 Format(IndxOvrl, "0"))

        'Modified to print thickness consistently by YGC 032312
        'If IndxOvrl <> 0 Then ' - PrintLine(8 Format(LayerThickness(0), "##0.0"))
        'If IndxOvrl <> 0 Then ' - PrintLine(8 LayerThickness(0))   'corrected by YC 040517
        'If IndxOvrl <> 0 Then ' - PrintLine(8 VB6.Format(LayerThickness(0), "0.000").Replace(",", "."))


        'If frmGear.chkTempLoad.CheckState Then
        '    SurfTemp = CDbl(frmGear.txtSurfTemp.Text)
        '    BotmTemp = CDbl(frmGear.txtBotmTemp.Text)

        '    'TempDiff = (BotmTemp - SurfTemp) / 2

        '    ThermCoef = CDbl(frmGear.txtThermCoef.Text)

        '    If (frmGear.cmbCurlShape.Text = "Circular") Then
        '        CurlShapePara = 0
        '    Else
        '        If (frmGear.cmbCurlShape.Text = "Catenary") Then
        '            CurlShapePara = 1
        '        End If
        '    End If

        '    ' - PrintLine(8 SurfTemp & " " & BotmTemp & " " & CurlShapePara & " " & ThermCoef)

        'End If

        'Modify end by YGC 060311
        'FileClose(8)               ' QW 09-15-2019

        Call PassRgdParms(OpenModeV) 'YC 101215 092016


    End Sub

    Sub PassIngParms()
        'newly created to pass .ing parameters by YC 040517

        Try


            NLcd = NCat(iCat)
            ReDim LCD(NLcd)

            Dim iLcd, jLcd As Integer
            For iLcd = 1 To NLcd
                With LCD(iLcd)
                    ReDim .LdPntTime(NLcd + 1), .LdPntMag(NLcd + 1)
                    For jLcd = 1 To NLcd + 1
                        .LdPntTime(jLcd) = jLcd - 1
                        If jLcd <> iLcd + 1 Then
                            .LdPntMag(jLcd) = 0
                        Else
                            .LdPntMag(jLcd) = 1
                        End If
                    Next jLcd
                End With
            Next iLcd


            Dim NSld As Integer = NSlabs
            If Overlay And Factor2 <= 0.1 Then NSld = 2 * NSlabs 'rigid overlay

            ReDim SLD(NSld)

            Dim iSld As Integer
            For iSld = 1 To NSld
                With SLD(iSld)
                    .IdxSld = iSld
                    .TypSld = "sv"
                    .PenaltySld = Factor1
                    .FrictionSld = 0.0
                    If Overlay And Factor2 <= 0.1 And iSld >= NSlabs + 1 Then .FrictionSld = Friction 'rigid overlay
                    .TypSldMerge = "nomerge"
                End With
            Next iSld


            Dim NMat As Integer = 11
            If Overlay Then NMat = 12
            ReDim MAT(NMat)

            Dim iMat As Integer
            For iMat = 1 To NMat
                MAT(iMat).IdxMat = iMat
                ReDim MAT(iMat).MatPara(6, 8)
            Next

            For iMat = 1 To 6
                MAT(iMat).KeyMatPrpty = 1
                MAT(iMat).MatPara(1, 1) = EMod(iMat)
                MAT(iMat).MatPara(2, 1) = PoissonsRatio(iMat)
                MAT(iMat).DensityMat = 0.0001872
            Next iMat
            MAT(1).DensityMat = 0.083912

            For iMat = 7 To 11
                MAT(iMat).KeyMatPrpty = 56
                MAT(iMat).MatPara(1, 1) = EMod(6)
                MAT(iMat).MatPara(2, 1) = PoissonsRatio(6)
            Next iMat
            MAT(7).DensityMat = 6
            MAT(8).DensityMat = 1
            MAT(9).DensityMat = 2
            MAT(10).DensityMat = 4
            MAT(11).DensityMat = 5

            If Overlay Then
                MAT(12).KeyMatPrpty = 1
                MAT(12).MatPara(1, 1) = EMod(0)
                MAT(12).MatPara(2, 1) = PoissonsRatio(0)
                MAT(12).DensityMat = 0.083912
            End If


            Dim NPartSLD As Integer = 2 * NSld

            Dim NPartMat As Integer = NSlabs + NumberOfLayers - 1 + 1 ' NSlabs slabs + slab/foundation layers-1 slab layer+1 extra SG ? YC
            If Overlay Then NPartMat = 2 * NSlabs + NumberOfLayers - 1 + 1

            Dim NPartBC As Integer
            If MeshCase = "SYM" Then NPartBC = 2
            If MeshCase = "UNSYM" Then NPartBC = 3

            Dim NPart As Integer = NSlabs + NPartBC + 1 'NSlabs slab parts + NPartBC dummy node parts+1 foundation part
            If Overlay Then NPart = 2 * NSlabs + NPartBC + 1

            ReDim PART(NPart)
            ReDim PARTSLD(NPartSLD)
            ReDim PARTBC(NPartBC)
            ReDim PARTMAT(NPartMat)

            Dim iSlab As Integer
            Dim iPart As Integer = 0
            Dim iPartSld As Integer = 0
            Dim iPartMat As Integer = 0


            For iSlab = 1 To NSlabs
                iPart = iPart + 1

                With PART(iPart)    'PCC LAYER

                    .NXCtr = 3
                    ReDim .NdXCtr(.NXCtr), .CoordXCtr(.NXCtr)
                    .NdXCtr = {0, 1, 1 + NX1, 1 + NX1 + NX2}
                    .CoordXCtr = {0, 0, 210, 300}

                    If MeshCase = "SYM" Then .NYCtr = 3
                    If MeshCase = "UNSYM" Then .NYCtr = 5
                    ReDim .NdYCtr(.NYCtr), .CoordYCtr(.NYCtr)
                    If MeshCase = "SYM" Then
                        .NdYCtr = {0, 1, 1 + NY1, 1 + NY1 + NY2}
                        .CoordYCtr = {0, 0, 60, 150}
                    ElseIf MeshCase = "UNSYM" Then
                        .NdYCtr = {0, 1, 1 + NY2, 1 + NY1 + NY2, 1 + NY2 + 2 * NY1, 1 + 2 * NY2 + 2 * NY1}
                        .CoordYCtr = {0, -150, -60, 0, 60, 150}
                    End If

                    .NZCtr = 2
                    ReDim .NdZCtr(.NZCtr), .CoordZCtr(.NZCtr)

                    .NdZCtr = {0, 1, 2}
                    .CoordZCtr = {0, 0, -tkpcc}

                    iPartMat = iPartMat + 1
                    PARTMAT(iPartMat).IdxPart = iPart
                    PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, 1}
                    PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, .NZCtr}
                    PARTMAT(iPartMat).MatIdx = 1 'PCC slab

                    iPartSld = iPartSld + 1
                    PARTSLD(iPartSld).IdxPart = iPart
                    PARTSLD(iPartSld).SldNdIdxCtrIni = {0, 1, 1, 1}
                    PARTSLD(iPartSld).SldNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 1}
                    PARTSLD(iPartSld).SldIdx = SLD(iSlab).IdxSld
                    PARTSLD(iPartSld).SldSmTyp = "s"
                    PARTSLD(iPartSld).SldDir = {0, 0, 0, 1}

                    If Overlay And Factor2 <= 0.1 Then 'PCC Overlay
                        iPartSld = iPartSld + 1
                        PARTSLD(iPartSld).IdxPart = iPart
                        PARTSLD(iPartSld).SldNdIdxCtrIni = {0, 1, 1, 2}
                        PARTSLD(iPartSld).SldNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 2}
                        PARTSLD(iPartSld).SldIdx = SLD(NSlabs + iSlab).IdxSld
                        PARTSLD(iPartSld).SldSmTyp = "m"
                        PARTSLD(iPartSld).SldDir = {0, 0, 0, 100}
                    End If

                End With

            Next

            If Overlay Then
                For iSlab = 1 To NSlabs
                    iPart = iPart + 1
                    PART(iPart) = PART(iPart - NSlabs)
                    With PART(iPart)
                        .CoordZCtr = {0, -tkpcc, -tkpcc - tkol}

                        iPartMat = iPartMat + 1
                        PARTMAT(iPartMat).IdxPart = iPart
                        PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, 1}
                        PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, .NZCtr}
                        PARTMAT(iPartMat).MatIdx = 12 'overlay

                        If Factor2 <= 0.1 Then 'PCC Overlay
                            iPartSld = iPartSld + 1
                            PARTSLD(iPartSld).IdxPart = iPart
                            PARTSLD(iPartSld).SldNdIdxCtrIni = {0, 1, 1, 1}
                            PARTSLD(iPartSld).SldNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 1}
                            PARTSLD(iPartSld).SldIdx = SLD(NSlabs + iSlab).IdxSld
                            PARTSLD(iPartSld).SldSmTyp = "s"
                            PARTSLD(iPartSld).SldDir = {0, 0, 0, 100}
                        End If
                    End With

                Next
            End If


            Dim iPartBc As Integer  'DUMMY NODES
            For iPartBc = 1 To NPartBC
                iPart = iPart + 1

                PART(iPart) = PART(1)

                With PART(iPart)

                    If Overlay Then ' overlay
                        .NZCtr = 3
                        ReDim .NdZCtr(.NZCtr), .CoordZCtr(.NZCtr)
                        .NdZCtr = {0, 1, 2, 3}
                        .CoordZCtr = {0, 0, -tkpcc, -tkpcc - tkol}
                    End If

                    If iPartBc = 1 Then
                        .NYCtr = 1
                        .NdYCtr = {0, -1}
                        .CoordYCtr = {0, 150.02}
                    ElseIf iPartBc = 2 Then
                        .NXCtr = 1
                        .NdXCtr = {0, -1}
                        .CoordXCtr = {0, 300.02}
                    ElseIf iPartBc = 3 Then
                        .NYCtr = 1
                        .NdYCtr = {0, -1}
                        .CoordYCtr = {0, -150.02}
                    End If

                    PARTBC(iPartBc).IdxPart = iPart
                    PARTBC(iPartBc).StrBCValue = "111111"
                    PARTBC(iPartBc).BCNdIdxCtrIni = {0, 1, 1, 1}
                    PARTBC(iPartBc).BCNdIdxCtrEnd = {0, .NXCtr, .NYCtr, .NZCtr}
                    If .NYCtr = 1 Then PARTBC(iPartBc).BCNdIdxCtrIni(2) = 0 : PARTBC(iPartBc).BCNdIdxCtrEnd(2) = 0
                    If .NXCtr = 1 Then PARTBC(iPartBc).BCNdIdxCtrIni(1) = 0 : PARTBC(iPartBc).BCNdIdxCtrEnd(1) = 0

                End With
            Next


            iPart = iPart + 1 'FOUNDATION
            With PART(iPart)

                .NXCtr = 6
                ReDim .NdXCtr(.NXCtr), .CoordXCtr(.NXCtr)
                .NdXCtr = {0, 1, 1 + NX1F, 1 + NX1F + NX2F, 1 + NX1F + NX2F + NX3F, 1 + NX1F + NX2F + NX3F + NX4F, 1 + NX1F + NX2F + NX3F + NX4F + NX5F}
                .CoordXCtr = {0, -150.0, -30.0, 0.0, 210.0, 300, 450.0}

                If MeshCase = "SYM" Then .NYCtr = 4
                If MeshCase = "UNSYM" Then .NYCtr = 7
                ReDim .NdYCtr(.NYCtr), .CoordYCtr(.NYCtr)
                If MeshCase = "SYM" Then
                    .NdYCtr = {0, 1, 1 + NY1F, 1 + NY1F + NY2F, 1 + NY1F + NY2F + NY3F}
                    .CoordYCtr = {0, 0.0, 60.0, 150.0, 300.0}
                ElseIf MeshCase = "UNSYM" Then
                    .NdYCtr = {0, 1, 1 + NY3F, 1 + NY3F + NY2F, 1 + NY3F + NY2F + NY1F, 1 + NY3F + NY2F + 2 * NY1F, 1 + NY3F + 2 * NY2F + 2 * NY1F, 1 + 2 * NY3F + 2 * NY2F + 2 * NY1F}
                    .CoordYCtr = {0, -300.0, -150.0, -60.0, 0.0, 60.0, 150.0, 300.0}
                End If

                .NZCtr = NumberOfLayers - 1 + 1 + 1 '- 1 slab layer + 1 additional SG + 1   NumberOfLayers exclude overlay
                ReDim .NdZCtr(.NZCtr), .CoordZCtr(.NZCtr)
                If NumberOfLayers = 6 Then
                    .NdZCtr = {0, 1, 2, 3, 4, 5, 7, 8}
                    .CoordZCtr = {0, 0, tk, t3 + t4, t3 + t4 + tk, TS + tk, -15 + TS + tk, -f13}
                ElseIf NumberOfLayers = 5 Then
                    .NdZCtr = {0, 1, 2, 3, 4, 6, 7}
                    .CoordZCtr = {0, 0, tk, t3 + tk, TS + tk, -15 + TS + tk, -f13}
                ElseIf NumberOfLayers = 4 Then
                    .NdZCtr = {0, 1, 2, 3, 5, 6}
                    .CoordZCtr = {0, 0, tk, TS + tk, -15 + TS + tk, -f13}
                ElseIf NumberOfLayers = 3 Then  'ts=0?
                    .NdZCtr = {0, 1, 2, 4, 5}
                    .CoordZCtr = {0, 0, tk, -15 + TS + tk, -f13}
                End If

                If NumberOfLayers > 2 Then
                    iPartMat = iPartMat + 1
                    PARTMAT(iPartMat).IdxPart = iPart
                    PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, 1}
                    PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 2}
                    PARTMAT(iPartMat).MatIdx = 2
                End If

                If NumberOfLayers > 3 Then
                    iPartMat = iPartMat + 1
                    PARTMAT(iPartMat).IdxPart = iPart
                    PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, 2}
                    PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 3}
                    PARTMAT(iPartMat).MatIdx = 3
                End If

                If NumberOfLayers > 4 Then
                    iPartMat = iPartMat + 1
                    PARTMAT(iPartMat).IdxPart = iPart
                    PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, 3}
                    PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 4}
                    PARTMAT(iPartMat).MatIdx = 4
                End If

                If NumberOfLayers > 5 Then
                    iPartMat = iPartMat + 1
                    PARTMAT(iPartMat).IdxPart = iPart
                    PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, 4}
                    PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 5}
                    PARTMAT(iPartMat).MatIdx = 5
                End If

                iPartMat = iPartMat + 1
                PARTMAT(iPartMat).IdxPart = iPart
                PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, .NZCtr - 2}
                PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, .NZCtr - 1}
                PARTMAT(iPartMat).MatIdx = 6    'finite SG

                iPartMat = iPartMat + 1
                PARTMAT(iPartMat).IdxPart = iPart
                PARTMAT(iPartMat).MatNdIdxCtrIni = {0, 1, 1, .NZCtr - 1}
                PARTMAT(iPartMat).MatNdIdxCtrEnd = {0, .NXCtr, .NYCtr, .NZCtr}
                PARTMAT(iPartMat).MatIdx = 7 'infinite SG


                For iSlab = 1 To NSlabs
                    iPartSld = iPartSld + 1
                    PARTSLD(iPartSld).IdxPart = iPart
                    PARTSLD(iPartSld).SldNdIdxCtrIni = {0, 1, 1, 1}
                    PARTSLD(iPartSld).SldNdIdxCtrEnd = {0, .NXCtr, .NYCtr, 1}
                    PARTSLD(iPartSld).SldIdx = SLD(iSlab).IdxSld    'YC ?
                    PARTSLD(iPartSld).SldSmTyp = "m"
                    PARTSLD(iPartSld).SldDir = {0, 0, 0, 1}
                Next

            End With


        Catch ex As Exception
            MsgBox("Error " & ex.Message & vbCr & "Occured in Sub PassIngParms.", MsgBoxStyle.OkOnly, "File Error")
            Exit Sub
        End Try

    End Sub

    Sub PassRgdParms(ByVal OpenModeV As OpenMode)
        ' to pass .rgd parameters by YC 101215 092016

        Static iAC As Integer

        If OpenModeV = OpenMode.Output Then
            ReDim ACLoad(NCat(iCat))
            iAC = 1
        ElseIf OpenModeV = OpenMode.Append Then
            iAC = iAC + 1
        End If

        Dim j, iSlab As Integer

        With ACLoad(iAC)
            .NWhls = NWheels
            .PcntCt = CDbl(TirePressure)

            ReDim .XTr(NWheels), .YTr(NWheels)
            For j = 1 To NWheels
                .XTr(j) = Xtr(j)
                .YTr(j) = Ytr(j)
            Next

            .XDim = Xfp
            .YDim = Yfp
            .IndxSy = IndxSymm
            .ScaleX = XScaleFactor
            .ScaleY = YScaleFactor
            .NSlabs = NSlabs

            ReDim .XGrid(NSlabs), .YGrid(NSlabs)
            For iSlab = 1 To NSlabs
                .XGrid(iSlab) = Xgrid
                .YGrid(iSlab) = Ygrid
            Next iSlab

            .f1 = Factor1
            .f2 = Factor2

            .Alpha = GearAngle

            .EqStifX = EqJStfX
            .EqStifY = EqJStfY
            .KeyEqStif = KeyEqJStf

            .EqStif12X = EqJStf12X
            .EqStif12Y = EqJStf12Y
            .KeyEqStif12 = KeyEqJStf12

            .EqEdgStifX = EqEdgStfX
            .EqEdgStifY = EqEdgStfY
            .KeyEqEdgStif = KeyEqEdgStf

            .IndxIm = IndxModes
            .IndxFn = IndxFndn
            .ThkSlb = LayerThickness(1)
            .IndxOvrl = IndxOvrl

            If IndxOvrl = 1 Then .ThkOvr = LayerThickness(0)


        End With

    End Sub

    Sub ReadIngParms(ByVal INGString As String)
        ' to read .ing parameters by YC 101215 092016

        NLcd = 0
        NSld = 0
        NMat = 0
        NPart = 0
        NPartSld = 0
        NPartBC = 0
        NPartMat = 0

        Const StrInt As String = "((-?)(\d+))"    ' integer string
        Const StrFloat As String = "((-?)(\d+\.?\d*))"     'float string
        Const StrScientific As String = StrFloat & "((e|E)([-+])\d+){0,1}"  'scientific (or float) string

        Dim reLcdHeaderPattern As String = "(\s*)(lcd)(\s+)" & "(" & "?<ldidx>" & StrInt & ")" & "(\s+)" & "(" & "?<ldpntnum>" & StrInt & ")"
        Dim reSldPattern As String = "(\s*)(si)(\s+)" & "(" & "?<sldidx>" & StrInt & ")" & "(\s*)(?<sldtyp>\w+)" & "(\s+)(pnlt)(\s+)" & "(" & "?<sldpnlt>" & StrFloat & ")" & "(\s+)(fric)(\s+)" & "(" & "?<sldfric>" & StrFloat & ")" & "(\s+)(?<sldmergetyp>\w+)" & "(\s*)"
        Dim reMatHeaderPattern As String = "(\s*)(mat)(\s+)" & "(" & "?<matidx>" & StrInt & ")" & "(\s+)" & "(" & "?<matproperty>" & StrInt & ")"
        Dim rePartSldPattern As String = "(\s*)(si)([+-])(\s+)" & "(" & "?<sldndxi>" & StrInt & ")" & "(\s+)" & "(" & "?<sldndyi>" & StrInt & ")" & "(\s+)" & "(" & "?<sldndzi>" & StrInt & ")" & "(\s+)" _
                                       & "(" & "?<sldndxe>" & StrInt & ")" & "(\s+)" & "(" & "?<sldndye>" & StrInt & ")" & "(\s+)" & "(" & "?<sldndze>" & StrInt & ")" & "(\s+)" _
                                       & "(" & "?<sldidx>" & StrInt & ")" & "(\s+)(?<sldsmtype>\w+)(\s+)" _
                                       & "(" & "?<slddirx>" & StrInt & ")" & "(\s+)" & "(" & "?<slddiry>" & StrInt & ")" & "(\s+)" & "(" & "?<slddirz>" & StrInt & ")" & "(\s*)"
        Dim rePartBCPattern As String = "(\s*)(b)(\s+)" & "(" & "?<bndryndxi>" & StrInt & ")" & "(\s+)" & "(" & "?<bndryndyi>" & StrInt & ")" & "(\s+)" & "(" & "?<bndryndzi>" & StrInt & ")" & "(\s+)" _
                                       & "(" & "?<bndryndxe>" & StrInt & ")" & "(\s+)" & "(" & "?<bndryndye>" & StrInt & ")" & "(\s+)" & "(" & "?<bndryndze>" & StrInt & ")" & "(\s+)" _
                                       & "(?<bndryval>\d{6})" & "(\s*)"
        Dim rePartMatePattern As String = "(\s*)(mate)(\s+)" & "(" & "?<matidx>" & StrInt & ")" & "(\s*)"
        Dim rePartMtPattern As String = "(\s*)(mt)(\s+)" & "(" & "?<matndxi>" & StrInt & ")" & "(\s+)" & "(" & "?<matndyi>" & StrInt & ")" & "(\s+)" & "(" & "?<matndzi>" & StrInt & ")" & "(\s+)" _
                                       & "(" & "?<matndxe>" & StrInt & ")" & "(\s+)" & "(" & "?<matndye>" & StrInt & ")" & "(\s+)" & "(" & "?<matndze>" & StrInt & ")" & "(\s+)" _
                                      & "(" & "?<matidx>" & StrInt & ")" & "(\s*)"

        Dim STRFileLines() As String, NLen As Integer
        Dim ReadLineString As String

        Try

            STRFileLines = Split(INGString, vbCrLf)
            NLen = STRFileLines.Length

            Dim ilen As Integer
            Dim NPartSldNeg As Integer = 0, NPartSldPos As Integer = 0
            '******************************************************************
            '******************************************************************
            ' to obtain the number of load, sliding interface, material and part
            For ilen = 0 To NLen - 1
                ReadLineString = STRFileLines(ilen)

                If Regex.IsMatch(ReadLineString, "(lcd)(\s+)") Then
                    NLcd = NLcd + 1
                End If

                If Regex.IsMatch(ReadLineString, "(si)(\s+)") Then
                    NSld = NSld + 1
                End If

                If Regex.IsMatch(ReadLineString, "(mat)(\s+)") Then
                    NMat = NMat + 1
                End If

                If Regex.IsMatch(ReadLineString, "(start)") Then
                    NPart = NPart + 1
                End If

                If Regex.IsMatch(ReadLineString, "(si\-)(\s+)") Then
                    NPartSldNeg = NPartSldNeg + 1
                End If

                If Regex.IsMatch(ReadLineString, "(si\+)(\s+)") Then
                    NPartSldPos = NPartSldPos + 1
                End If

                If Regex.IsMatch(ReadLineString, rePartBCPattern) Then
                    NPartBC = NPartBC + 1
                End If

                If Regex.IsMatch(ReadLineString, "(mate|mt)(\s+)") Then
                    NPartMat = NPartMat + 1
                End If

            Next
            '******************************************************************
            '******************************************************************

            If NPartSldNeg <> NPartSldPos Then
                MsgBox("Error: The number of si+ is not equal to the number of si- in .Ing file", MsgBoxStyle.OkOnly, "File Error")
                Exit Sub
            End If

            NPartSld = NPartSldNeg + NPartSldPos

            ReDim LCD(NLcd)
            ReDim SLD(NSld)
            ReDim MAT(NMat)
            ReDim PART(NPart)
            ReDim PARTSLD(NPartSld)
            'ReDim PARTSLDNEG(NPartSldNeg), PARTSLDPOS(NPartSldPos)
            ReDim PARTBC(NPartBC)
            ReDim PARTMAT(NPartMat)

            Dim reLcdHeader As New Regex(reLcdHeaderPattern)
            Dim IdxLd, NumLdPnt As Integer
            Dim reLdPntPattern As String = "(\s*)" & "(" & "?<ldtimpnt>" & StrFloat & ")" & "(\s+)" & "(" & "?<ldmagpnt>" & StrFloat & ")" & "(\s*)"
            Dim reLdPnt As New Regex(reLdPntPattern)
            Dim isLdPntReading As Boolean = False

            Dim reSld As New Regex(reSldPattern)

            Dim reTpro As New Regex("(\s*)(tpro)(\s+)" & "(" & "?<itpro>" & StrInt & ")")

            Dim reMatHeader As New Regex(reMatHeaderPattern)
            Dim isMatReading As Boolean = False

            Dim reInt As New Regex("(\s*)" & StrInt & "(\s*)")   'Regular Expression of integer
            Dim reFloat As New Regex("(\s*)" & StrFloat & "(\s*)")   'Regular Expression of float
            Dim reScientific As New Regex("(\s*)" & StrScientific & "(\s*)")   'Regular Expression of scientific and float

            Dim isNdXCtrReading As Boolean = False, isNdYCtrReading As Boolean = False, isNdZCtrReading As Boolean = False
            Dim isXCtrReading As Boolean = False, isYCtrReading As Boolean = False, isZCtrReading As Boolean = False
            Dim isPartReading As Boolean = False

            Dim rePartSld As New Regex(rePartSldPattern)
            Dim rePartBC As New Regex(rePartBCPattern)
            Dim rePartMate As New Regex(rePartMatePattern)
            Dim rePartMt As New Regex(rePartMtPattern)

            Dim m As Match

            Dim iLcd As Integer = 0
            Dim iSld As Integer = 0
            Dim iMat As Integer = 0

            Dim iPart As Integer = 0
            Dim iPartSld As Integer
            Dim iPartSldNeg As Integer = 0, iPartSldPos As Integer = 0
            Dim iPartBc As Integer = 0
            Dim iPartMat As Integer = 0

            Dim i, j As Integer

            '******************************************************************
            '******************************************************************
            For ilen = 0 To NLen - 1
                ReadLineString = STRFileLines(ilen)

                '********************** to read in Load definition****************************
                If Regex.IsMatch(ReadLineString, "(lcd)(\s+)") Then

                    iLcd = iLcd + 1

                    For Each m In reLcdHeader.Matches(ReadLineString)
                        IdxLd = CInt(m.Groups("ldidx").Value)
                        NumLdPnt = CInt(m.Groups("ldpntnum").Value)
                    Next

                    If NLcd <> NumLdPnt - 1 Then
                        MsgBox("The number of load and the number of load point defintion is not consistent!" & vbCrLf, MsgBoxStyle.OkOnly, "File Error")
                        Exit Sub
                    End If

                    ReDim LCD(iLcd).LdPntTime(NumLdPnt), LCD(iLcd).LdPntMag(NumLdPnt)

                    isLdPntReading = True
                    j = 1

                    GoTo nxtln
                End If

                If isLdPntReading Then

                    For Each m In reLdPnt.Matches(ReadLineString)
                        LCD(iLcd).LdPntTime(j) = CDbl(m.Groups("ldtimpnt").Value)
                        LCD(iLcd).LdPntMag(j) = CDbl(m.Groups("ldmagpnt").Value)
                        j = j + 1
                    Next

                    If j > NumLdPnt Then isLdPntReading = False

                    GoTo nxtln
                End If
                '********************** read in Load definition END****************************


                '**********************to read in temperature indicator ****************************
                If Regex.IsMatch(ReadLineString, "(tpro)(\s+)") Then
                    For Each m In reTpro.Matches(ReadLineString)
                        itpro = CInt(m.Groups("itpro").Value)
                    Next
                End If
                '**********************read in temperature indicator END****************************


                '**********************to read in sliding interface definition ****************************
                If Regex.IsMatch(ReadLineString, "(si)(\s+)") Then
                    iSld = iSld + 1

                    For Each m In reSld.Matches(ReadLineString)
                        SLD(iSld).IdxSld = CInt(m.Groups("sldidx").Value)
                        SLD(iSld).TypSld = m.Groups("sldtyp").Value
                        SLD(iSld).PenaltySld = CDbl(m.Groups("sldpnlt").Value)
                        SLD(iSld).FrictionSld = CDbl(m.Groups("sldfric").Value)
                        SLD(iSld).TypSldMerge = m.Groups("sldmergetyp").Value
                    Next
                End If
                '**********************read in sliding interface definition ****************************


                '**********************to read in material definition ****************************
                If Regex.IsMatch(ReadLineString, "(mat)(\s+)") Then
                    iMat = iMat + 1

                    For Each m In reMatHeader.Matches(ReadLineString)
                        MAT(iMat).IdxMat = CInt(m.Groups("matidx").Value)
                        MAT(iMat).KeyMatPrpty = CInt(m.Groups("matproperty").Value)
                    Next

                    isMatReading = True
                    i = 0

                    ReDim MAT(iMat).MatPara(6, 8)

                    GoTo nxtln
                End If

                If isMatReading Then

                    If Regex.IsMatch(ReadLineString, "(temp)(\s+)") Then
                        i = i + 1
                        j = 1
                        For Each m In reFloat.Matches(ReadLineString)
                            If j <= 8 Then
                                MAT(iMat).MatPara(i, j) = CDbl(m.Value)
                                j = j + 1
                            End If
                        Next
                        GoTo nxtln
                    End If

                    If Regex.IsMatch(ReadLineString, "(e)(\s+)") Then
                        i = i + 1
                        j = 1
                        For Each m In reFloat.Matches(ReadLineString)
                            If j <= 8 Then
                                MAT(iMat).MatPara(i, j) = CDbl(m.Value)
                                j = j + 1
                            End If
                        Next
                        GoTo nxtln
                    End If

                    If Regex.IsMatch(ReadLineString, "(pr)(\s+)") Then
                        i = i + 1
                        j = 1
                        For Each m In reFloat.Matches(ReadLineString)
                            If j <= 8 Then
                                MAT(iMat).MatPara(i, j) = CDbl(m.Value)
                                j = j + 1
                            End If
                        Next
                        GoTo nxtln
                    End If

                    If Regex.IsMatch(ReadLineString, "(alpha)(\s+)") Then
                        i = i + 1
                        j = 1
                        For Each m In reScientific.Matches(ReadLineString)
                            If j <= 8 Then
                                MAT(iMat).MatPara(i, j) = CDbl(m.Value)
                                j = j + 1
                            End If
                        Next
                        GoTo nxtln
                    End If

                    If Regex.IsMatch(ReadLineString, "(ro)(\s+)") Then
                        For Each m In reScientific.Matches(ReadLineString)
                            MAT(iMat).DensityMat = CDbl(m.Value)
                        Next
                        GoTo nxtln
                    End If

                    If Regex.IsMatch(ReadLineString, "(endmat)") Then isMatReading = False

                    GoTo nxtln
                End If
                '********************** read in material definition END ****************************


                '********************** to read in Part definition****************************
                If Regex.IsMatch(ReadLineString, "(start)") Then
                    iPart = iPart + 1
                    isPartReading = True
                    isNdXCtrReading = True

                    GoTo nxtln
                End If

                If isNdXCtrReading Then

                    PART(iPart).NXCtr = reInt.Matches(ReadLineString).Count
                    ReDim PART(iPart).NdXCtr(PART(iPart).NXCtr)

                    j = 1
                    For Each m In reInt.Matches(ReadLineString)
                        PART(iPart).NdXCtr(j) = CInt(m.Value)
                        j = j + 1
                    Next

                    isNdXCtrReading = False
                    isNdYCtrReading = True

                    GoTo nxtln
                End If


                If isNdYCtrReading Then

                    PART(iPart).NYCtr = reInt.Matches(ReadLineString).Count
                    ReDim PART(iPart).NdYCtr(PART(iPart).NYCtr)

                    j = 1
                    For Each m In reInt.Matches(ReadLineString)
                        PART(iPart).NdYCtr(j) = CInt(m.Value)
                        j = j + 1
                    Next

                    isNdYCtrReading = False
                    isNdZCtrReading = True

                    GoTo nxtln
                End If


                If isNdZCtrReading Then

                    PART(iPart).NZCtr = reInt.Matches(ReadLineString).Count
                    ReDim PART(iPart).NdZCtr(PART(iPart).NZCtr)

                    j = 1
                    For Each m In reInt.Matches(ReadLineString)
                        PART(iPart).NdZCtr(j) = CInt(m.Value)
                        j = j + 1
                    Next

                    isNdZCtrReading = False
                    isXCtrReading = True

                    GoTo nxtln

                End If


                If isXCtrReading Then

                    ReDim PART(iPart).CoordXCtr(PART(iPart).NXCtr)

                    j = 1
                    For Each m In reFloat.Matches(ReadLineString)
                        PART(iPart).CoordXCtr(j) = CDbl(m.Value)
                        j = j + 1
                    Next

                    isXCtrReading = False
                    isYCtrReading = True

                    GoTo nxtln
                End If

                If isYCtrReading Then

                    ReDim PART(iPart).CoordYCtr(PART(iPart).NYCtr)

                    j = 1
                    For Each m In reFloat.Matches(ReadLineString)
                        PART(iPart).CoordYCtr(j) = CDbl(m.Value)
                        j = j + 1
                    Next

                    isYCtrReading = False
                    isZCtrReading = True

                    GoTo nxtln
                End If

                If isZCtrReading Then

                    ReDim PART(iPart).CoordZCtr(PART(iPart).NZCtr)

                    j = 1
                    For Each m In reFloat.Matches(ReadLineString)
                        PART(iPart).CoordZCtr(j) = CDbl(m.Value)
                        j = j + 1
                    Next

                    isZCtrReading = False

                    GoTo nxtln
                End If

                If isPartReading And Regex.IsMatch(ReadLineString, "(si)([+-])(\s+)") Then
                    iPartSld = iPartSld + 1

                    ReDim PARTSLD(iPartSld).SldNdIdxCtrIni(3), PARTSLD(iPartSld).SldNdIdxCtrEnd(3), PARTSLD(iPartSld).SldDir(3)

                    PARTSLD(iPartSld).IdxPart = iPart
                    For Each m In rePartSld.Matches(ReadLineString)
                        PARTSLD(iPartSld).SldNdIdxCtrIni(1) = CInt(m.Groups("sldndxi").Value)
                        PARTSLD(iPartSld).SldNdIdxCtrIni(2) = CInt(m.Groups("sldndyi").Value)
                        PARTSLD(iPartSld).SldNdIdxCtrIni(3) = CInt(m.Groups("sldndzi").Value)
                        PARTSLD(iPartSld).SldNdIdxCtrEnd(1) = CInt(m.Groups("sldndxe").Value)
                        PARTSLD(iPartSld).SldNdIdxCtrEnd(2) = CInt(m.Groups("sldndye").Value)
                        PARTSLD(iPartSld).SldNdIdxCtrEnd(3) = CInt(m.Groups("sldndze").Value)
                        PARTSLD(iPartSld).SldIdx = CInt(m.Groups("sldidx").Value)
                        PARTSLD(iPartSld).SldSmTyp = m.Groups("sldsmtype").Value
                        PARTSLD(iPartSld).SldDir(1) = CInt(m.Groups("slddirx").Value)
                        PARTSLD(iPartSld).SldDir(2) = CInt(m.Groups("slddiry").Value)
                        PARTSLD(iPartSld).SldDir(3) = CInt(m.Groups("slddirz").Value)
                    Next

                    GoTo nxtln
                End If


                If isPartReading And Regex.IsMatch(ReadLineString, "(b)(\s+)") Then
                    iPartBc = iPartBc + 1

                    ReDim PARTBC(iPartBc).BCNdIdxCtrIni(3), PARTBC(iPartBc).BCNdIdxCtrEnd(3)

                    PARTBC(iPartBc).IdxPart = iPart
                    For Each m In rePartBC.Matches(ReadLineString)
                        PARTBC(iPartBc).BCNdIdxCtrIni(1) = CInt(m.Groups("bndryndxi").Value)
                        PARTBC(iPartBc).BCNdIdxCtrIni(2) = CInt(m.Groups("bndryndyi").Value)
                        PARTBC(iPartBc).BCNdIdxCtrIni(3) = CInt(m.Groups("bndryndzi").Value)
                        PARTBC(iPartBc).BCNdIdxCtrEnd(1) = CInt(m.Groups("bndryndxe").Value)
                        PARTBC(iPartBc).BCNdIdxCtrEnd(2) = CInt(m.Groups("bndryndye").Value)
                        PARTBC(iPartBc).BCNdIdxCtrEnd(3) = CInt(m.Groups("bndryndze").Value)
                        If Len(m.Groups("bndryval").Value) <> 6 Then
                            MsgBox("Error: The digit number of boundary value is not equal to 6", MsgBoxStyle.OkOnly, "File Error")
                            Exit Sub
                        End If
                        PARTBC(iPartBc).StrBCValue = m.Groups("bndryval").Value
                    Next

                    GoTo nxtln
                End If

                If isPartReading And Regex.IsMatch(ReadLineString, "(mate)(\s+)") Then
                    iPartMat = iPartMat + 1

                    ReDim PARTMAT(iPartMat).MatNdIdxCtrIni(3), PARTMAT(iPartMat).MatNdIdxCtrEnd(3)

                    PARTMAT(iPartMat).IdxPart = iPart
                    PARTMAT(iPartMat).MatNdIdxCtrIni(1) = 1
                    PARTMAT(iPartMat).MatNdIdxCtrIni(2) = 1
                    PARTMAT(iPartMat).MatNdIdxCtrIni(3) = 1
                    PARTMAT(iPartMat).MatNdIdxCtrEnd(1) = PART(iPart).NXCtr
                    PARTMAT(iPartMat).MatNdIdxCtrEnd(2) = PART(iPart).NYCtr
                    PARTMAT(iPartMat).MatNdIdxCtrEnd(3) = PART(iPart).NZCtr

                    For Each m In rePartMate.Matches(ReadLineString)
                        PARTMAT(iPartMat).MatIdx = CInt(m.Groups("matidx").Value)
                    Next

                    GoTo nxtln
                End If

                If isPartReading And Regex.IsMatch(ReadLineString, "(mt)(\s+)") Then
                    iPartMat = iPartMat + 1

                    ReDim PARTMAT(iPartMat).MatNdIdxCtrIni(3), PARTMAT(iPartMat).MatNdIdxCtrEnd(3)

                    PARTMAT(iPartMat).IdxPart = iPart
                    For Each m In rePartMt.Matches(ReadLineString)
                        PARTMAT(iPartMat).MatNdIdxCtrIni(1) = CInt(m.Groups("matndxi").Value)
                        PARTMAT(iPartMat).MatNdIdxCtrIni(2) = CInt(m.Groups("matndyi").Value)
                        PARTMAT(iPartMat).MatNdIdxCtrIni(3) = CInt(m.Groups("matndzi").Value)
                        PARTMAT(iPartMat).MatNdIdxCtrEnd(1) = CInt(m.Groups("matndxe").Value)
                        PARTMAT(iPartMat).MatNdIdxCtrEnd(2) = CInt(m.Groups("matndye").Value)
                        PARTMAT(iPartMat).MatNdIdxCtrEnd(3) = CInt(m.Groups("matndze").Value)
                        PARTMAT(iPartMat).MatIdx = CInt(m.Groups("matidx").Value)
                    Next

                    GoTo nxtln
                End If

                If isPartReading And Regex.IsMatch(ReadLineString, "(end)") Then
                    isPartReading = False
                    GoTo nxtln
                End If
                '********************** read in Part definition END****************************

nxtln:      Next
            '******************************************************************
            '******************************************************************

        Catch ex As Exception
            MsgBox("Error " & ex.Message & vbCr & "Occured in Sub ReadIngParms.", MsgBoxStyle.OkOnly, "File Error")
            Exit Sub
        End Try

    End Sub

    Public Sub GearCG(ByRef XW() As Double, ByRef YW() As Double, ByRef N As Short, ByRef Xcg As Double, ByRef Ycg As Double)
        Dim LibIndexAM As Integer
        Dim WheelRadius As Double
        Dim libXGridNPoints(1), libYGridNPoints(1) As Double

        Dim I, J As Short
        Dim XRcg(), YRcg(), IXRcg(), IYRcg() As Double
        Dim PcntOnMainGears As Double
        Dim NMainGears As Integer

        ReDim XRcg(N) : ReDim YRcg(N) : ReDim IXRcg(N) : ReDim IYRcg(N)

        PcntOnMainGears = 100 : NMainGears = 1
        WheelRadius = CDbl(GrossWeight * PcntOnMainGears / (100 * NMainGears * NWheels))
        WheelRadius = CDbl(System.Math.Sqrt(WheelRadius / TirePressure / PI))

        Xcg = 0 : Ycg = 0
        For I = 1 To N
            Xcg = Xcg + XW(I) : Ycg = Ycg + YW(I)
        Next I
        Xcg = Xcg / N : Ycg = Ycg / N

        If libXGridNPoints(LibIndexAM) <> 0 And libYGridNPoints(LibIndexAM) <> 0 Then
            Exit Sub
        End If

        For I = 1 To N
            XRcg(I) = XW(I) - Xcg : YRcg(I) = YW(I) - Ycg
            IXRcg(I) = Fix(XRcg(I)) ' Test symmetry to the nearest inch.
            IYRcg(I) = Fix(YRcg(I))
        Next I

        NXSymmetric = 0 : NYSymmetric = 0
        For I = 1 To N
            For J = 1 To N
                ' Test symmetry about the X axis.
                If IXRcg(I) = IXRcg(J) And IYRcg(I) = -IYRcg(J) And I <> J Then
                    NXSymmetric = CShort(NXSymmetric + 1)
                End If
                ' Test symmetry about the Y axis.
                If IYRcg(I) = IYRcg(J) And IXRcg(I) = -IXRcg(J) And I <> J Then
                    NYSymmetric = CShort(NYSymmetric + 1)
                End If
            Next J
            ' Wheels on an axis are symmetric but not found above.
            If IYRcg(I) = 0 Then NXSymmetric = CShort(NXSymmetric + 1)
            If IXRcg(I) = 0 Then NYSymmetric = CShort(NYSymmetric + 1)
        Next I

        XGridOrigin = 1.0E+35 : XGridMax = -1.0E+35
        YGridOrigin = 1.0E+35 : YGridMax = -1.0E+35


        'sss
        'NXSymmetric = 0 'check symmetry
        'NYSymmetric = 0

        If NXSymmetric = N And NYSymmetric = N Then ' Symmetric about both axes.
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
            Next I
            XGridOrigin = XGridOrigin + Xcg
            XGridMax = Xcg
            YGridOrigin = YGridOrigin + Ycg
            YGridMax = Ycg
        ElseIf NXSymmetric = N And NYSymmetric < N Then  ' Symmetric about X axis.
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
                If XRcg(I) > XGridMax Then XGridMax = XRcg(I)
            Next I
            XGridOrigin = XGridOrigin + Xcg
            XGridMax = XGridMax + Xcg
            YGridOrigin = YGridOrigin + Ycg
            YGridMax = Ycg
        ElseIf NXSymmetric < N And NYSymmetric = N Then  ' Symmetric about Y axis.
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
                If YRcg(I) > YGridMax Then YGridMax = YRcg(I)
            Next I
            XGridOrigin = XGridOrigin + Xcg
            XGridMax = Xcg
            YGridOrigin = YGridOrigin + Ycg
            YGridMax = YGridMax + Ycg
        ElseIf NXSymmetric < N And NYSymmetric < N Then  ' No symmetry.
            For I = 1 To N
                If XRcg(I) < XGridOrigin Then XGridOrigin = XRcg(I)
                If XRcg(I) > XGridMax Then XGridMax = XRcg(I)
                If YRcg(I) < YGridOrigin Then YGridOrigin = YRcg(I)
                If YRcg(I) > YGridMax Then YGridMax = YRcg(I)
            Next I
            XGridOrigin = XGridOrigin + Xcg
            XGridMax = XGridMax + Xcg
            YGridOrigin = YGridOrigin + Ycg
            YGridMax = YGridMax + Ycg
        Else ' Two tires in the same place.
            '    Error recovery here
        End If


        If MeshCase = "4DNSY" Then
            'no symmetry
            NXSymmetric = 0 : NYSymmetric = 0
        End If


    End Sub



    Sub Layer_BaseEdgNew(ByVal LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms)
        'to seperate sym and unsym gear by YC 082216 092016
        'to remove parameter definiton for MeshClassLib by YC 101215 092016

        Dim NumSB As Integer
        'Dim TS As Double   'YC 040517
        Dim I As Integer
        Dim InfiniteElementHeight As Double


        Dim bthick As Double, iStart As Integer
        bthick = 0

        If Overlay = True Then
            iStart = 3
        Else
            iStart = 2
        End If

        For I = iStart To LEAStructure.NLayers - 1
            bthick = bthick + LEAStructure.Thick(I)
        Next I

        TS = 0.0
        If Overlay = True Then
            NumberOfLayers = NumberOfLayers - 1S
        End If

        If SubBaseExists = True Then
            NumSB = NumberOfLayers - 3 ' number of subbase layers below the upper subbase
            For I = 3 To 2 + NumSB
                TS = TS - LayerThickness(I) : Next I
        End If

        Dim p6 As Double

        tk = -LayerThickness(2)
        t3 = -LayerThickness(3)
        t4 = -LayerThickness(4)
        p6 = 0


        Dim f1, f2, f9, f10, f12 As Integer
        Dim f3, f4, f5, f6 As Double

        'f1 number of node along horizontal direction if not symmetric
        'f2 number of node along horizontal direction if symmetric
        f1 = 51

        If SymmSngl = True Then
            f2 = CInt((f1 + 1) / 2)
        Else
            f2 = f1
        End If

        'f3 dimension along -x
        'f4 dimension along x
        f3 = -150 'Modified by YGC 032911
        f4 = 450 'Modified by YGC 032911

        'f5 dimension along -y
        'f6 dimension along y
        If SymmSngl = True Then
            f5 = 0
        Else
            f5 = -300 'Modified by YGC 032911
        End If
        f6 = 300 'Modified by YGC 032911

        'f9 number of node layers in infinite part of SG
        'f10 number of node layers in finite part of SG
        'f12 number of elements in BS/SB
        f9 = 2
        f10 = 3
        f12 = 1

        'f13 dimension along -z
        'Define the proportions of the infinite element using the rule: Xq - Xc = Xp - Xq
        If Overlay = True Then
            InfiniteElementHeight = LayerThickness(0) + LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1) - LayerThickness(0)) 'this parameter controls the depth to apparent base of the inf. subgrade
        Else
            InfiniteElementHeight = LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1)) 'this parameter controls the depth to apparent base of the inf. subgrade
        End If

        'Dim NX1F, NX2F, NX3F, NX4F, NX5F As Integer 'YC 040517
        'Dim NY1F, NY2F, NY3F As Integer

        NX1F = 8
        NX2F = 4
        NX3F = CInt(SlabFineMeshNumX / 2)
        NX4F = 6
        NX5F = 5

        NY1F = CInt(SlabFineMeshNumY / 2)
        NY2F = 6
        NY3F = 5

        INGString = INGString & "exch 1 2 3" & vbCrLf &
                "gct 4; ryz; rz 180; mz " & TS + tk & ";" & " rzx;" & vbCrLf &
                "lev 1 grep 0 1 ; ;" & vbCrLf &
                "lev 2 grep 2 ; ;" & vbCrLf &
                "lev 3 grep 3; ;" & vbCrLf &
                "lev 4 grep 0 4; ;" & vbCrLf & vbCrLf

        ' for BS and SB
        If MeshCase = "SYM" Then
            INGString = INGString & "start" & vbCrLf &
            "1 " & (1 + NX1F) & " " & (1 + NX1F + NX2F) & " " & (1 + NX1F + NX2F + NX3F) &
            " " & (1 + NX1F + NX2F + NX3F + NX4F) & " " & (1 + NX1F + NX2F + NX3F + NX4F + NX5F) & ";" & vbCrLf &
            "1 " & (1 + NY1F) & " " & (1 + NY1F + NY2F) & " " & (1 + NY1F + NY2F + NY3F) & ";" & vbCrLf
        ElseIf MeshCase = "UNSYM" Then
            INGString = INGString & "start" & vbCrLf &
            "1 " & (1 + NX1F) & " " & (1 + NX1F + NX2F) & " " & (1 + NX1F + NX2F + NX3F) &
            " " & (1 + NX1F + NX2F + NX3F + NX4F) & " " & (1 + NX1F + NX2F + NX3F + NX4F + NX5F) & ";" & vbCrLf &
            "1 " & (1 + NY3F) & " " & (1 + NY3F + NY2F) & " " & (1 + NY3F + NY2F + NY1F) &
            " " & (1 + NY3F + NY2F + 2 * NY1F) & " " & (1 + NY3F + 2 * NY2F + 2 * NY1F) & " " & (1 + 2 * NY3F + 2 * NY2F + 2 * NY1F) & ";" & vbCrLf
        End If


        GoTo newfoundationlabel     'YC 040517

        If SubBaseExists = True Then

            INGString = INGString & "1 " & (1 + f12) & " " & (1 + 2 * f12)
            'If NumberOfLayers > 4 Then INGString = INGString & (1 + 3 * f12)
            'If NumberOfLayers > 5 Then INGString = INGString & (1 + 4 * f12)

            'If NumberOfLayers > 4 Then INGString = INGString & (1 + 3 * f12) ' corrected by YC 121216
            'If NumberOfLayers > 5 Then INGString = INGString & (1 + 4 * f12)
            If NumberOfLayers > 4 Then INGString = INGString & " " & (1 + 3 * f12)
            If NumberOfLayers > 5 Then INGString = INGString & " " & (1 + 4 * f12)



            INGString = INGString & ";" & vbCrLf

            If MeshCase = "SYM" Then
                INGString = INGString & "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
                 "0.0 60.0 150.0 300.0" & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
                 "-300.0 -150.0 -60.0 0.0 60.0 150.0 300.0" & vbCrLf
            End If

            If NumberOfLayers > 5 Then
                If MeshCase = "SYM" Then
                    INGString = INGString & (TS + tk) & " " & (t3 + t4 + tk) & " " & (t3 + tk) & " " & tk & " " & p6 & vbCrLf &
                    "si+ 1 1 5 6 4 5 1 m 0 0 1" & vbCrLf &
                    "mt 1 1 1 6 4 2 5" & vbCrLf &
                    "mt 1 1 2 6 4 3 4" & vbCrLf &
                    "mt 1 1 3 6 4 4 3" & vbCrLf &
                    "mt 1 1 4 6 4 5 2" & vbCrLf
                ElseIf MeshCase = "UNSYM" Then
                    INGString = INGString & (TS + tk) & " " & (t3 + t4 + tk) & " " & (t3 + tk) & " " & tk & " " & p6 & vbCrLf &
                    "si+ 1 1 5 6 7 5 1 m 0 0 1" & vbCrLf &
                    "mt 1 1 1 6 7 2 5" & vbCrLf &
                    "mt 1 1 2 6 7 3 4" & vbCrLf &
                    "mt 1 1 3 6 7 4 3" & vbCrLf &
                    "mt 1 1 4 6 7 5 2" & vbCrLf
                End If
            ElseIf NumberOfLayers > 4 Then
                If MeshCase = "SYM" Then
                    INGString = INGString & (TS + tk) & " " & (t3 + tk) & " " & tk & " " & p6 & vbCrLf &
                        "si+ 1 1 4 6 4 4 1 m 0 0 1" & vbCrLf &
                        "mt 1 1 1 6 4 2 4" & vbCrLf &
                        "mt 1 1 2 6 4 3 3" & vbCrLf &
                        "mt 1 1 3 6 4 4 2" & vbCrLf
                ElseIf MeshCase = "UNSYM" Then
                    INGString = INGString & (TS + tk) & " " & (t3 + tk) & " " & tk & " " & p6 & vbCrLf &
                        "si+ 1 1 4 6 7 4 1 m 0 0 1" & vbCrLf &
                        "mt 1 1 1 6 7 2 4" & vbCrLf &
                        "mt 1 1 2 6 7 3 3" & vbCrLf &
                        "mt 1 1 3 6 7 4 2" & vbCrLf
                End If
            Else
                If MeshCase = "SYM" Then
                    INGString = INGString & (TS + tk) & " " & tk & " " & p6 & vbCrLf &
                       "si+ 1 1 3 6 4 3 1 m 0 0 1" & vbCrLf &
                       "mt 1 1 1 6 4 2 3" & vbCrLf &
                       "mt 1 1 2 6 4 3 2" & vbCrLf
                ElseIf MeshCase = "UNSYM" Then
                    INGString = INGString & (TS + tk) & " " & tk & " " & p6 & vbCrLf &
                       "si+ 1 1 3 6 7 3 1 m 0 0 1" & vbCrLf &
                       "mt 1 1 1 6 7 2 3" & vbCrLf &
                       "mt 1 1 2 6 7 3 2" & vbCrLf
                End If
            End If
        Else
            If MeshCase = "SYM" Then
                INGString = INGString & "1 " & (1 + f12) & ";" & vbCrLf &
                    "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
                    "0.0 60.0 150.0 300.0" & vbCrLf &
                    tk & " " & p6 & vbCrLf &
                    "si+ 1 1 2 6 4 2 1 m 0 0 1" & vbCrLf &
                     "mate 2" & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & "1 " & (1 + f12) & ";" & vbCrLf &
                    "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
                    "-300.0 -150.0 -60.0 0.0 60.0 150.0 300.0" & vbCrLf &
                    tk & " " & p6 & vbCrLf &
                    "si+ 1 1 2 6 7 2 1 m 0 0 1" & vbCrLf &
                     "mate 2" & vbCrLf
            End If
        End If

        INGString = INGString & "end" & vbCrLf & vbCrLf

        ' for SG
        If MeshCase = "SYM" Then
            INGString = INGString & "start" & vbCrLf &
            "1 " & (1 + NX1F) & " " & (1 + NX1F + NX2F) & " " & (1 + NX1F + NX2F + NX3F) &
            " " & (1 + NX1F + NX2F + NX3F + NX4F) & " " & (1 + NX1F + NX2F + NX3F + NX4F + NX5F) & ";" & vbCrLf &
            "1 " & (1 + NY1F) & " " & (1 + NY1F + NY2F) & " " & (1 + NY1F + NY2F + NY3F) & ";" & vbCrLf &
            "1 3 4; " & vbCrLf &
            "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
            "0.0 60.0 150.0 300.0" & vbCrLf &
            (TS + tk) & " " & (-15 + TS + tk) & " " & (-f13) & vbCrLf &
            "mt 1 1 1 6 4 2 6" & vbCrLf &
            "mt 1 1 2 6 4 3 7" & vbCrLf &
            "end" & vbCrLf
        ElseIf MeshCase = "UNSYM" Then
            INGString = INGString & "start" & vbCrLf &
            "1 " & (1 + NX1F) & " " & (1 + NX1F + NX2F) & " " & (1 + NX1F + NX2F + NX3F) &
            " " & (1 + NX1F + NX2F + NX3F + NX4F) & " " & (1 + NX1F + NX2F + NX3F + NX4F + NX5F) & ";" & vbCrLf &
            "1 " & (1 + NY3F) & " " & (1 + NY3F + NY2F) & " " & (1 + NY3F + NY2F + NY1F) &
            " " & (1 + NY3F + NY2F + 2 * NY1F) & " " & (1 + NY3F + 2 * NY2F + 2 * NY1F) & " " & (1 + 2 * NY3F + 2 * NY2F + 2 * NY1F) & ";" & vbCrLf &
            "1 3 4; " & vbCrLf &
            "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
            "-300.0 -150.0 -60 0.0 60.0 150.0 300.0" & vbCrLf &
            (TS + tk) & " " & (-15 + TS + tk) & " " & (-f13) & vbCrLf &
            "mt 1 1 1 6 7 2 6" & vbCrLf &
            "mt 1 1 2 6 7 3 7" & vbCrLf &
            "end" & vbCrLf
        End If


newfoundationlabel:  'YC 040517
        If NumberOfLayers = 6 Then
            INGString = INGString & "1 " & (1 + f12) & " " & (1 + 2 * f12) & " " & (1 + 3 * f12) & " " & (1 + 4 * f12) & " " & (1 + 4 * f12 + 2) & " " & (1 + 4 * f12 + 3)
        ElseIf NumberOfLayers = 5 Then
            INGString = INGString & "1 " & (1 + f12) & " " & (1 + 2 * f12) & " " & (1 + 3 * f12) & " " & (1 + 3 * f12 + 2) & " " & (1 + 3 * f12 + 3)
        ElseIf NumberOfLayers = 4 Then
            INGString = INGString & "1 " & (1 + f12) & " " & (1 + 2 * f12) & " " & (1 + 2 * f12 + 2) & " " & (1 + 2 * f12 + 3)
        ElseIf NumberOfLayers = 3 Then
            INGString = INGString & "1 " & (1 + f12) & " " & (1 + f12 + 2) & " " & (1 + f12 + 3)
        End If

        INGString = INGString & ";" & vbCrLf

        If MeshCase = "SYM" Then
            INGString = INGString & "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
             "0.0 60.0 150.0 300.0" & vbCrLf
        ElseIf MeshCase = "UNSYM" Then
            INGString = INGString & "-150.0 -30.0 0.0 210.0 300 450.0" & vbCrLf &
             "-300.0 -150.0 -60.0 0.0 60.0 150.0 300.0" & vbCrLf
        End If

        If NumberOfLayers = 6 Then
            INGString = INGString & p6 & " " & tk & " " & (t3 + tk) & " " & (t3 + t4 + tk) & " " & (TS + tk) & " " & (-15 + TS + tk) & " " & (-f13) & vbCrLf
            If MeshCase = "SYM" Then
                INGString = INGString &
               "si+ 1 1 1 6 4 1 1 m 0 0 1" & vbCrLf &
               "mt 1 1 1 6 4 2 2" & vbCrLf &
               "mt 1 1 2 6 4 3 3" & vbCrLf &
               "mt 1 1 3 6 4 4 4" & vbCrLf &
               "mt 1 1 4 6 4 5 5" & vbCrLf &
               "mt 1 1 5 6 4 6 6" & vbCrLf &
               "mt 1 1 6 6 4 7 7" & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString &
               "si+ 1 1 1 6 7 1 1 m 0 0 1" & vbCrLf &
               "mt 1 1 1 6 7 2 2" & vbCrLf &
               "mt 1 1 2 6 7 3 3" & vbCrLf &
               "mt 1 1 3 6 7 4 4" & vbCrLf &
               "mt 1 1 4 6 7 5 5" & vbCrLf &
               "mt 1 1 5 6 7 6 6" & vbCrLf &
               "mt 1 1 6 6 7 7 7" & vbCrLf
            End If
        ElseIf NumberOfLayers = 5 Then
            INGString = INGString & p6 & " " & tk & " " & (t3 + tk) & " " & (TS + tk) & " " & (-15 + TS + tk) & " " & (-f13) & vbCrLf
            If MeshCase = "SYM" Then
                INGString = INGString &
                    "si+ 1 1 1 6 4 1 1 m 0 0 1" & vbCrLf &
                    "mt 1 1 1 6 4 2 2" & vbCrLf &
                    "mt 1 1 2 6 4 3 3" & vbCrLf &
                    "mt 1 1 3 6 4 4 4" & vbCrLf &
                    "mt 1 1 4 6 4 5 6" & vbCrLf &
                    "mt 1 1 5 6 4 6 7" & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString &
                    "si+ 1 1 1 6 7 1 1 m 0 0 1" & vbCrLf &
                    "mt 1 1 1 6 7 2 2" & vbCrLf &
                    "mt 1 1 2 6 7 3 3" & vbCrLf &
                    "mt 1 1 3 6 7 4 4" & vbCrLf &
                    "mt 1 1 4 6 7 5 6" & vbCrLf &
                    "mt 1 1 5 6 7 6 7" & vbCrLf
            End If
        ElseIf NumberOfLayers = 4 Then
            INGString = INGString & p6 & " " & tk & " " & (TS + tk) & " " & (-15 + TS + tk) & " " & (-f13) & vbCrLf
            If MeshCase = "SYM" Then
                INGString = INGString &
                   "si+ 1 1 1 6 4 1 1 m 0 0 1" & vbCrLf &
                   "mt 1 1 1 6 4 2 2" & vbCrLf &
                   "mt 1 1 2 6 4 3 3" & vbCrLf &
                   "mt 1 1 3 6 4 4 6" & vbCrLf &
                   "mt 1 1 4 6 4 5 7" & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & (TS + tk) & " " & tk & " " & p6 & vbCrLf &
                   "si+ 1 1 1 6 7 1 1 m 0 0 1" & vbCrLf &
                   "mt 1 1 1 6 7 2 2" & vbCrLf &
                   "mt 1 1 2 6 7 3 3" & vbCrLf &
                   "mt 1 1 3 6 7 4 6" & vbCrLf &
                   "mt 1 1 4 6 7 5 7" & vbCrLf
            End If
        ElseIf NumberOfLayers = 3 Then
            INGString = INGString & p6 & " " & tk & " " & (-15 + TS + tk) & " " & (-f13) & vbCrLf
            If MeshCase = "SYM" Then
                INGString = INGString &
                   "si+ 1 1 1 6 4 1 1 m 0 0 1" & vbCrLf &
                   "mt 1 1 1 6 4 2 2" & vbCrLf &
                   "mt 1 1 2 6 4 3 6" & vbCrLf &
                   "mt 1 1 3 6 4 4 7" & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & (TS + tk) & " " & tk & " " & p6 & vbCrLf &
                   "si+ 1 1 1 6 7 1 1 m 0 0 1" & vbCrLf &
                   "mt 1 1 1 6 7 2 2" & vbCrLf &
                   "mt 1 1 2 6 7 3 6" & vbCrLf &
                   "mt 1 1 3 6 7 4 7" & vbCrLf
            End If
        End If


    End Sub


    Sub Layer_BaseEdgNew1p41(ByVal LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms)
        'used in version upto FF 2016.09.20 v1.41.0011, renamed by YC 101215 092016
        'Sub Layer_BaseEdgNew for FF1.4 non-uniform by YGC 092613

        Dim DepthOfSubgrade As Double
        Dim NumSB As Integer
        Dim TS As Double
        Dim I As Integer
        Dim InfiniteElementHeight As Double

        Dim f1, f2, f3, f4, f5, f6, f9, f10, f12, f13 As Double

        Dim bthick As Double, iStart As Integer
        bthick = 0

        If Overlay = True Then
            iStart = 3
        Else
            iStart = 2
        End If

        For I = iStart To LEAStructure.NLayers - 1
            bthick = bthick + LEAStructure.Thick(I)
        Next I
        'Print(7, " bthick " & CStr(Math.Round(bthick, 4)).Replace(",", "."))
        TS = 0.0
        If Overlay = True Then
            NumberOfLayers = NumberOfLayers - 1S
        End If

        If SubBaseExists = True Then
            NumSB = NumberOfLayers - 3 ' number of subbase layers below the upper subbase
            For I = 3 To 2 + NumSB
                TS = TS - LayerThickness(I) : Next I
        End If

        ' - PrintLine(7, "exch 1 2 3")
        ' - PrintLine(7, "Parameter")
        Print(7, "tk " & CStr(-LayerThickness(2)).Replace(",", "."))
        Print(7, " ts " & CStr(Math.Round(TS, 2)).Replace(",", "."))
        Print(7, " t3 " & CStr(Math.Round(-LayerThickness(3), 2)).Replace(",", "."))
        ' - PrintLine(7, " t4 " & CStr(-LayerThickness(4)).Replace(",", "."))

        'f1 number of node along horizontal direction if not symmetric
        'f2 number of node along horizontal direction if symmetric
        f1 = 51


        If SymmSngl = True Then
            f2 = (f1 + 1) / 2
        Else
            f2 = f1
        End If

        'f3 dimension along -x
        'f4 dimension along x
        f3 = -150 'Modified by YGC 032911
        f4 = 450 'Modified by YGC 032911

        'f5 dimension along -y
        'f6 dimension along y
        If SymmSngl = True Then
            f5 = 0
        Else
            f5 = -300 'Modified by YGC 032911
        End If
        f6 = 300 'Modified by YGC 032911

        'f9 number of node layers in infinite part of SG
        'f10 number of node layers in finite part of SG
        'f12 number of elements in BS/SB
        f9 = 2
        f10 = 3
        f12 = 1

        'f13 dimension along -z
        'Define the proportions of the infinite element using the rule: Xq - Xc = Xp - Xq
        If Overlay = True Then
            InfiniteElementHeight = LayerThickness(0) + LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1) - LayerThickness(0)) 'this parameter controls the depth to apparent base of the inf. subgrade
        Else
            InfiniteElementHeight = LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1)) 'this parameter controls the depth to apparent base of the inf. subgrade
        End If

        Print(7, " f1 " & CStr(f1).Replace(",", ".")) ' - PrintLine(7, " f2 " & CStr(f2).Replace(",", "."))
        Print(7, " f3 " & CStr(f3).Replace(",", ".")) ' - PrintLine(7, " f4 " & CStr(f4).Replace(",", "."))
        Print(7, " f5 " & CStr(f5).Replace(",", ".")) ' - PrintLine(7, " f6 " & CStr(f6).Replace(",", "."))
        Print(7, " f9 " & CStr(f9).Replace(",", ".")) : Print(7, " f10 " & CStr(f10).Replace(",", ".")) ' - PrintLine(7, " f12 " & CStr(f12).Replace(",", "."))
        ' - PrintLine(7, " f13 " & CStr(f13).Replace(",", "."))



        'modified for decreased element number for fine area by YGC 072214 
        '' - PrintLine(7, "NX1F 8 NX2F 32 NX3F 6 NX4F 5")
        '' - PrintLine(7, "NY1F 8 NY2F 6 NY3F 5")
        ' - PrintLine(7, "NX1F 8 NX2F 4 NX3F " & CStr(SlabFineMeshNumX / 2) & " NX4F 6 NX5F 5")
        ' - PrintLine(7, "NY1F " & CStr(SlabFineMeshNumY / 2) & " NY2F 6 NY3F 5")
        'modified for decreased element number for fine area by YGC 072214 END

        ' - PrintLine(7, ";")
        ' - PrintLine(7, "")

        ' - PrintLine(7, "gct 4; ryz; rz 180; mz [%ts + %tk]; rzx;")
        ' - PrintLine(7, "lev 1 grep 0 1 ; ;")
        ' - PrintLine(7, "lev 2 grep 2 ; ;")
        ' - PrintLine(7, "lev 3 grep 3; ;")
        ' - PrintLine(7, "lev 4 grep 0 4; ;")
        ' - PrintLine(7, "")

        ' for BS and SB
        ' - PrintLine(7, "start")

        'modified for decreased element number for fine area by YGC 072214 
        '' - PrintLine(7, "1 [1+%NX1F] [1+%NX1F+%NX2F] [1+%NX1F+%NX2F+%NX3F] [1+%NX1F+%NX2F+%NX3F+%NX4F];")
        ' - PrintLine(7, "1 [1+%NX1F] [1+%NX1F+%NX2F] [1+%NX1F+%NX2F+%NX3F]")
        ' - PrintLine(7, "[1+%NX1F+%NX2F+%NX3F+%NX4F][1+%NX1F+%NX2F+%NX3F+%NX4F+%NX5F];")
        'modified for decreased element number for fine area by YGC 072214 END 

        ' - PrintLine(7, "1 [1+%NY1F] [1+%NY1F+%NY2F] [1+%NY1F+%NY2F+%NY3F];")

        If SubBaseExists = True Then
            Print(7, "1 [1+%f12] [1+2*%f12]")
            If NumberOfLayers > 4 Then Print(7, " [1+3*%f12]")
            If NumberOfLayers > 5 Then Print(7, " [1+4*%f12]")
            ' - PrintLine(7, ";")

            '' - PrintLine(7, "-150.0 -30.0 210.0 300 450.0")   'modified for decreased element number for fine area by YGC 072214
            ' - PrintLine(7, "-150.0 -30.0 0.0 210.0 300 450.0")

            ' - PrintLine(7, "0.0 60.0 150.0 300.0")

            If NumberOfLayers > 5 Then
                ' - PrintLine(7, "[%ts+%tk] [%t3+%t4+%tk] [%t3+%tk] %tk %p6")

                'modified for decreased element number for fine area by YGC 072214 
                '' - PrintLine(7, "si+ 1 1 5 5 4 5 1 m 0 0 1")
                '' - PrintLine(7, "mt 1 1 1 5 4 2 5")
                '' - PrintLine(7, "mt 1 1 2 5 4 3 4")
                '' - PrintLine(7, "mt 1 1 3 5 4 4 3")
                '' - PrintLine(7, "mt 1 1 4 5 4 5 2")
                ' - PrintLine(7, "si+ 1 1 5 6 4 5 1 m 0 0 1")
                ' - PrintLine(7, "mt 1 1 1 6 4 2 5")
                ' - PrintLine(7, "mt 1 1 2 6 4 3 4")
                ' - PrintLine(7, "mt 1 1 3 6 4 4 3")
                ' - PrintLine(7, "mt 1 1 4 6 4 5 2")
                'modified for decreased element number for fine area by YGC 072214 END

            ElseIf NumberOfLayers > 4 Then
                ' - PrintLine(7, "[%ts+%tk] [%t3+%tk] %tk %p6")

                'modified for decreased element number for fine area by YGC 072214 
                '' - PrintLine(7, "si+ 1 1 4 5 4 4 1 m 0 0 1")
                '' - PrintLine(7, "mt 1 1 1 5 4 2 4")
                '' - PrintLine(7, "mt 1 1 2 5 4 3 3")
                '' - PrintLine(7, "mt 1 1 3 5 4 4 2")
                ' - PrintLine(7, "si+ 1 1 4 6 4 4 1 m 0 0 1")
                ' - PrintLine(7, "mt 1 1 1 6 4 2 4")
                ' - PrintLine(7, "mt 1 1 2 6 4 3 3")
                ' - PrintLine(7, "mt 1 1 3 6 4 4 2")
                'modified for decreased element number for fine area by YGC 072214 END

            Else
                ' - PrintLine(7, "[%ts+%tk] %tk %p6")

                'modified for decreased element number for fine area by YGC 072214 
                '' - PrintLine(7, "si+ 1 1 3 5 4 3 1 m 0 0 1")
                '' - PrintLine(7, "mt 1 1 1 5 4 2 3")
                '' - PrintLine(7, "mt 1 1 2 5 4 3 2")
                ' - PrintLine(7, "si+ 1 1 3 6 4 3 1 m 0 0 1")
                ' - PrintLine(7, "mt 1 1 1 6 4 2 3")
                ' - PrintLine(7, "mt 1 1 2 6 4 3 2")
                'modified for decreased element number for fine area by YGC 072214 END

            End If
        Else
            ' - PrintLine(7, "1 [1+%f12];")

            '' - PrintLine(7, "-150.0 -30.0 210.0 300 450.0")   'modified for decreased element number for fine area by YGC 072214 
            ' - PrintLine(7, "-150.0 -30.0 0.0 210.0 300 450.0")

            ' - PrintLine(7, "0.0 60.0 150.0 300.0")
            ' - PrintLine(7, "%tk %p6")

            '' - PrintLine(7, "si+ 1 1 2 5 4 2 1 m 0 0 1")  'modified for decreased element number for fine area by YGC 072214 
            ' - PrintLine(7, "si+ 1 1 2 6 4 2 1 m 0 0 1")

            ' - PrintLine(7, "mate 2")
        End If

        ' - PrintLine(7, "end")
        ' - PrintLine(7, "")

        ' for SG
        ' - PrintLine(7, "start")

        'modified for decreased element number for fine area by YGC 072214 
        '' - PrintLine(7, "1 [1+%NX1F] [1+%NX1F+%NX2F] [1+%NX1F+%NX2F+%NX3F] [1+%NX1F+%NX2F+%NX3F+%NX4F];")
        ' - PrintLine(7, "1 [1+%NX1F] [1+%NX1F+%NX2F] [1+%NX1F+%NX2F+%NX3F]")
        ' - PrintLine(7, "[1+%NX1F+%NX2F+%NX3F+%NX4F][1+%NX1F+%NX2F+%NX3F+%NX4F+%NX5F];")
        'modified for decreased element number for fine area by YGC 072214 END

        ' - PrintLine(7, "1 [1+%NY1F] [1+%NY1F+%NY2F] [1+%NY1F+%NY2F+%NY3F];")
        ' - PrintLine(7, "1 3 4; ")

        '' - PrintLine(7, "-150.0 -30.0 210.0 300 450.0")   'modified for decreased element number for fine area by YGC 072214 
        ' - PrintLine(7, "-150.0 -30.0 0.0 210.0 300 450.0")

        ' - PrintLine(7, "0.0 60.0 150.0 300.0")
        ' - PrintLine(7, "[%ts+%tk] [-15+%ts+%tk] [-%f13]")

        'modified for decreased element number for fine area by YGC 072214 
        '' - PrintLine(7, "mt 1 1 1 5 4 2 6")
        '' - PrintLine(7, "mt 1 1 2 5 4 3 7")
        ' - PrintLine(7, "mt 1 1 1 6 4 2 6")
        ' - PrintLine(7, "mt 1 1 2 6 4 3 7")
        'modified for decreased element number for fine area by YGC 072214 END

        ' - PrintLine(7, "end")
        ' - PrintLine(7, "")
    End Sub

    Sub Layer_BaseEdgNewFF1p4(ByVal LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms)
        'Sub Layer_BaseEdgNew for FF1.4 by YGC 011311
        'Renamed by YGC 092613

        Dim DepthOfSubgrade As Double
        Dim NumSB As Integer
        Dim TS As Double
        Dim I As Integer
        Dim InfiniteElementHeight As Double

        Dim f1, f2, f3, f4, f5, f6, f9, f10, f12, f13 As Double

        ' - PrintLine(7, "exch 1 2 3")
        Print(7, " parameter tk " & CStr(-LayerThickness(2)).Replace(",", "."))

        Dim bthick As Double, iStart As Integer
        bthick = 0

        If Overlay = True Then
            iStart = 3
        Else
            iStart = 2
        End If

        For I = iStart To LEAStructure.NLayers - 1
            bthick = bthick + LEAStructure.Thick(I)
        Next I
        'Print(7, " bthick " & CStr(Math.Round(bthick, 4)).Replace(",", "."))
        TS = 0.0
        If Overlay = True Then
            NumberOfLayers = NumberOfLayers - 1S
        End If

        If SubBaseExists = True Then
            NumSB = NumberOfLayers - 3 ' number of subbase layers below the upper subbase
            For I = 3 To 2 + NumSB
                TS = TS - LayerThickness(I) : Next I
        End If
        Print(7, " ts " & CStr(Math.Round(TS, 2)).Replace(",", "."))
        Print(7, " t3 " & CStr(Math.Round(-LayerThickness(3), 2)).Replace(",", "."))
        Print(7, " t4 " & CStr(-LayerThickness(4)).Replace(",", "."))
        Print(7, " p6 0.0")

        ' - PrintLine(7, " p7 " & CStr(Math.Round(-DepthOfSubgrade, 2)).Replace(",", "."))

        'f1 number of node along horizontal direction if not symmetric
        'f2 number of node along horizontal direction if symmetric
        f1 = 51


        If SymmSngl = True Then
            f2 = (f1 + 1) / 2
        Else
            f2 = f1
        End If

        'f3 dimension along -x
        'f4 dimension along x
        f3 = -150 'Modified by YGC 032911
        f4 = 450 'Modified by YGC 032911

        'f5 dimension along -y
        'f6 dimension along y
        If SymmSngl = True Then
            f5 = 0
        Else
            f5 = -300 'Modified by YGC 032911
        End If
        f6 = 300 'Modified by YGC 032911

        'f9 number of node layers in infinite part of SG
        'f10 number of node layers in finite part of SG
        'f12 number of elements in BS/SB
        f9 = 2
        f10 = 3
        f12 = 1

        'f13 dimension along -z
        'Define the proportions of the infinite element using the rule: Xq - Xc = Xp - Xq
        If Overlay = True Then
            InfiniteElementHeight = LayerThickness(0) + LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1) - LayerThickness(0)) 'this parameter controls the depth to apparent base of the inf. subgrade
        Else
            InfiniteElementHeight = LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1)) 'this parameter controls the depth to apparent base of the inf. subgrade
        End If

        Print(7, " f1 " & CStr(f1).Replace(",", ".")) ' - PrintLine(7, " f2 " & CStr(f2).Replace(",", "."))
        Print(7, " f3 " & CStr(f3).Replace(",", ".")) ' - PrintLine(7, " f4 " & CStr(f4).Replace(",", "."))
        Print(7, " f5 " & CStr(f5).Replace(",", ".")) ' - PrintLine(7, " f6 " & CStr(f6).Replace(",", "."))
        Print(7, " f9 " & CStr(f9).Replace(",", ".")) : Print(7, " f10 " & CStr(f10).Replace(",", ".")) ' - PrintLine(7, " f12 " & CStr(f12).Replace(",", "."))
        ' - PrintLine(7, " f13 " & CStr(f13).Replace(",", "."))
        ' - PrintLine(7, " rs 1.00;")
        ' - PrintLine(7, "")

        ' - PrintLine(7, "gct 4; ryz; rz 180; mz [%ts + %tk]; rzx;")
        ' - PrintLine(7, "lev 1 grep 0 1 ; ;")
        ' - PrintLine(7, "lev 2 grep 2 ; ;")
        ' - PrintLine(7, "lev 3 grep 3; ;")
        ' - PrintLine(7, "lev 4 grep 0 4; ;")
        ' - PrintLine(7, "")

        ' for BS and SB
        ' - PrintLine(7, "start")
        Print(7, "1 [%f1]; 1 [%f2];")
        If SubBaseExists = True Then
            Print(7, "1 [1+%f12] [1+2*%f12]")
            If NumberOfLayers > 4 Then Print(7, " [1+3*%f12]")
            If NumberOfLayers > 5 Then Print(7, " [1+4*%f12]")
            ' - PrintLine(7, ";")
            ' - PrintLine(7, "[%f3] [%f4]")
            ' - PrintLine(7, "[%f5] [%f6]")

            If NumberOfLayers > 5 Then
                ' - PrintLine(7, "[%ts+%tk] [%t3+%t4+%tk] [%t3+%tk] %tk %p6")
                ' - PrintLine(7, "si+ 1 1 5 2 2 5 1 m 0 0 1")
                ' - PrintLine(7, "mt 1 1 1 2 2 2 5")
                ' - PrintLine(7, "mt 1 1 2 2 2 3 4")
                ' - PrintLine(7, "mt 1 1 3 2 2 4 3")
                ' - PrintLine(7, "mt 1 1 4 2 2 5 2")
            ElseIf NumberOfLayers > 4 Then
                ' - PrintLine(7, "[%ts+%tk] [%t3+%tk] %tk %p6")
                ' - PrintLine(7, "si+ 1 1 4 2 2 4 1 m 0 0 1")
                ' - PrintLine(7, "mt 1 1 1 2 2 2 4")
                ' - PrintLine(7, "mt 1 1 2 2 2 3 3")
                ' - PrintLine(7, "mt 1 1 3 2 2 4 2")
            Else
                ' - PrintLine(7, "[%ts+%tk] %tk %p6")
                ' - PrintLine(7, "si+ 1 1 3 2 2 3 1 m 0 0 1")
                ' - PrintLine(7, "mt 1 1 1 2 2 2 3")
                ' - PrintLine(7, "mt 1 1 2 2 2 3 2")
            End If
        Else
            ' - PrintLine(7, "1 [1+%f12];")
            ' - PrintLine(7, "[%f3] [%f4]")
            ' - PrintLine(7, "[%f5] [%f6]")
            ' - PrintLine(7, "%tk %p6")
            ' - PrintLine(7, "si+ 1 1 2 2 2 2 1 m 0 0 1")
            ' - PrintLine(7, "mate 2")
        End If




        ' B.C. added by YGC032911, suppress by YGC 060311
        '' - PrintLine(7, "b 1 1 1 1 2 3 100000")
        '' - PrintLine(7, "b 1 2 1 2 2 3 010000")
        '' - PrintLine(7, "b 2 1 1 2 2 3 100000")
        ' B.C. add end by YGC032911, suppress by YGC 060311

        ' - PrintLine(7, "end")
        ' - PrintLine(7, "")

        ' for finite SG
        ' - PrintLine(7, "start")
        If SymmSngl = True Then
            ' - PrintLine(7, "1 [%f1]; 1 [%f2]; 1 %f10; ")
        Else
            ' - PrintLine(7, "1 [%f1]; 1 [%f1]; 1 %f10; ")
        End If
        ' - PrintLine(7, "[%f3] [%f4]")
        ' - PrintLine(7, "[%f5] [%f6]")
        ' - PrintLine(7, "[-15+%ts+%tk] [%ts+%tk]")

        ' B.C. added by YGC032911, suppress by YGC 060311
        '' - PrintLine(7, "b 1 1 1 1 2 2 100000")
        '' - PrintLine(7, "b 1 2 1 2 2 2 010000")
        '' - PrintLine(7, "b 2 1 1 2 2 2 100000")
        ' B.C. add end by YGC032911, suppress by YGC 060311

        'If SymmSngl = True Then ' - PrintLine(7, "b 1 1 1 2 1 2 010000") 'added for symmetrical BC by YGC 060611

        ' - PrintLine(7, "mate 6")
        ' - PrintLine(7, "end")
        ' - PrintLine(7, "")

        ' for infinite SG
        ' - PrintLine(7, "start")
        If SymmSngl = True Then
            ' - PrintLine(7, "1 [%f1]; 1 [%f2]; 1 %f9; ")
        Else
            ' - PrintLine(7, "1 [%f1]; 1 [%f1]; 1 %f9; ")
        End If
        ' - PrintLine(7, "[%f3] [%f4]")
        ' - PrintLine(7, "[%f5] [%f6]")
        ' - PrintLine(7, "[-%f13] [-15+%ts+%tk]")

        ' B.C. added by YGC032911, suppress by YGC 060311
        '' - PrintLine(7, "b 1 1 1 1 2 2 100000")
        '' - PrintLine(7, "b 1 2 1 2 2 2 010000")
        '' - PrintLine(7, "b 2 1 1 2 2 2 100000")
        ' B.C. add end by YGC032911, suppress by YGC 060311

        ' If SymmSngl = True Then ' - PrintLine(7, "b 1 1 1 2 1 2 010000") 'added for symmetrical BC by YGC 060611

        ' - PrintLine(7, "mate 7")
        ' - PrintLine(7, "end")
        ' - PrintLine(7, "")

    End Sub

    'Sub Layer_BaseEdgNew(ByVal LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms)
    Sub Layer_BaseEdgNewFF1p3(ByVal LEAStructure As LEAFClassLib.clsLEAF.LEAFStrParms)
        'sub Layer_BaseEdgNew for FF1.305, comemnt by YGC 011311

        'subroutine to set up the 3d mesh for the subbase and subgrade layers, edge case
        Dim DepthOfSubgrade As Double
        Dim NumSB As Integer
        Dim TS As Double
        Dim InfiniteElementHeight As Double
        'Dim DepthOfSubgrade As Double
        Dim I As Integer
        Dim f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f12, f13, f14, f24 As Double
        Dim f1y, f2y, f3y, f4y, f5y, f6y, f7y As Double


        DepthOfSubgrade = CDbl(frmStructure.Cutoff)
        ' - PrintLine(7, "exch 1 2 3")
        Print(7, " parameter tk " & CStr(-LayerThickness(2)).Replace(",", "."))

        Dim bthick As Double, iStart As Integer
        bthick = 0

        If Overlay = True Then
            iStart = 3
        Else
            iStart = 2
        End If

        For I = iStart To LEAStructure.NLayers - 1
            bthick = bthick + LEAStructure.Thick(I)
        Next I
        Print(7, " bthick " & CStr(Math.Round(bthick, 4)).Replace(",", "."))
        TS = -0.0001
        If Overlay = True Then
            NumberOfLayers = NumberOfLayers - 1S
        End If

        If SubBaseExists = True Then
            NumSB = NumberOfLayers - 3 ' number of subbase layers below the upper subbase
            For I = 3 To 2 + NumSB
                TS = TS - LayerThickness(I) : Next I
        End If
        Print(7, " ts " & CStr(Math.Round(TS, 2)).Replace(",", "."))
        Print(7, " t3 " & CStr(Math.Round(-LayerThickness(3), 2)).Replace(",", ".")) : Print(7, " t4 " & CStr(-LayerThickness(4)).Replace(",", "."))
        Print(7, " p6 -0.0001")

        ' - PrintLine(7, " p7 " & CStr(Math.Round(-DepthOfSubgrade, 2)).Replace(",", "."))


        If c < 70 Then
            f2 = 2
        Else
            f2 = 1
        End If

        f1 = c : f5 = 1
        f3 = CShort(NumBlcksFineMeshX / 2)
        f4 = f1 + (gSlabSize - f1) / NumBlcksCrseMeshX * f2
        f6 = f1 + (gSlabSize - f1) / NumBlcksCrseMeshX * (f2 + f5)
        f7 = Math.Max(NumBlcksCrseMeshX - f2 - f5, 1S)

        f1y = e
        f3y = CShort(NumBlcksFineMeshY / 2)
        f2y = 2 'never used
        f4y = f1y + (gSlabSize - f1y) / NumBlcksCrseMeshY

        f5y = NumBlcksCrseMeshY
        f6y = gSlabSize / 2
        f7y = 5

        '==================================================================================================
        f14 = 3
        f24 = -24
        f8 = f3 + 1
        f9 = 2
        f10 = 3
        f12 = 1

        'Define the proportions of the infinite element using the rule: Xq - Xc = Xp - Xq
        If Overlay = True Then
            InfiniteElementHeight = LayerThickness(0) + LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1) - LayerThickness(0)) 'this parameter controls the depth to apparent base of the inf. subgrade
        Else
            InfiniteElementHeight = LayerThickness(1) + LayerThickness(2) - TS + 15.0#
            'If InfiniteElementHeight > 43 Then
            '    InfiniteElementHeight = 43
            'End If

            f13 = CInt(2.0# * InfiniteElementHeight - LayerThickness(1)) 'this parameter controls the depth to apparent base of the inf. subgrade
        End If


        'f9 number of node layers in top part of subgrade layer
        'f12 number of elements through the thickness of subbase layers
        'f13 this parameter controls the depth to apparent base of the inf. subgrade
        'f14 this parameter controls the number of elements in the extended base region
        'f24 thickness of the extension


        Print(7, "f1 " & CStr(f1).Replace(",", ".")) : Print(7, " f2 " & CStr(f2).Replace(",", ".")) : Print(7, " f3 " & CStr(f3).Replace(",", "."))
        Print(7, " f4 " & CStr(f4).Replace(",", ".")) : Print(7, " f5 " & CStr(f5).Replace(",", ".")) : Print(7, " f6 " & CStr(f6).Replace(",", "."))
        ' - PrintLine(7, " f7 " & CStr(f7).Replace(",", "."))
        Print(7, "f1Y " & CStr(f1y).Replace(",", ".")) : Print(7, " f2Y " & CStr(f2y).Replace(",", ".")) : Print(7, " f3Y " & CStr(f3y).Replace(",", "."))
        Print(7, " f4Y " & CStr(f4y).Replace(",", ".")) : Print(7, " f5Y " & CStr(f5y).Replace(",", ".")) : Print(7, " f6Y " & CStr(f6y).Replace(",", "."))
        ' - PrintLine(7, " f7Y " & CStr(f7y).Replace(",", "."))
        ' - PrintLine(7, "f8 " & CStr(f8).Replace(",", "."))
        ' - PrintLine(7, "f9 " & CStr(f9).Replace(",", "."))
        If InfiniteElement = True Then
            ' - PrintLine(7, "f10 " & CStr(f10).Replace(",", "."))
        End If
        Print(7, " f12 " & CStr(f12).Replace(",", ".")) : Print(7, " f13 " & CStr(f13).Replace(",", ".")) : Print(7, " f14 " & CStr(f14).Replace(",", "."))
        Print(7, " f24 " & CStr(f24).Replace(",", ".")) ' - PrintLine(7, " rs 1.00;")

        ' - PrintLine(7, "gct 4; ryz; rz 180; mz [%ts + %tk]; rzx;")
        ' - PrintLine(7, "lev 1 grep 0 1 ; ;")
        ' - PrintLine(7, "lev 2 grep 2 ; ;")
        ' - PrintLine(7, "lev 3 grep 3; ;")
        ' - PrintLine(7, "lev 4 grep 0 4; ;")



        ' - PrintLine(7, " ") : ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Subgrade Regular Elements ==")
        ' - PrintLine(7, "start")
        If SymmSngl = True Then
            ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
            ' - PrintLine(7, "1 [1 + %f3Y] [1 + %f3Y + %f5Y];")
            ' - PrintLine(7, "1 %f10;")
            '' - PrintLine(7, "0.0 %f1 %f4 %f6 300.0")
            ' - PrintLine(7, "0.0 %f1 %f4 %f6 " & gSlabSize)
            ' - PrintLine(7, "0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
        Else
            ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
            ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
            ' - PrintLine(7, "1 %f10;")
            '' - PrintLine(7, "0.0 %f1 %f4 %f6 300.0")
            ' - PrintLine(7, "0.0 %f1 %f4 %f6 " & CStr(gSlabSize).Replace(",", "."))
            ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
        End If
        ' - PrintLine(7, "mate 6")
        ' - PrintLine(7, "end")



        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Infinite Elements +x direction ==")
        ' - PrintLine(7, "start")
        If SymmSngl = True Then
            ' - PrintLine(7, "1 2; 1 [1 + %f3Y][1 + %f3Y + %f5Y]; 1 %f10;")
            '' - PrintLine(7, "300.0 [300. + (300./60.)*(%f13 -15. + (%tk + %ts))]")
            ' - PrintLine(7, "" & CStr(gSlabSize).Replace(",", ".") & " [" & CStr(gSlabSize).Replace(",", ".") &
            ' " + (" & CStr(gSlabSize).Replace(",", ".") & "/60.)*(%f13 -15. + (%tk + %ts))]")
            ' - PrintLine(7, "0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
        Else
            ' - PrintLine(7, "1 2;")
            ' - PrintLine(7, "1 [1 + %f5Y][1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
            ' - PrintLine(7, "1 %f10;")
            ' - PrintLine(7, "300.0 [300. + (300./60.)*(%f13 -15. + (%tk + %ts))]")
            ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
        End If
        ' - PrintLine(7, "mate 8")
        ' - PrintLine(7, "edit")
        '' - PrintLine(7, "phr 302. -1500. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "phr " & (CStr(gSlabSize + 2)).Replace(",", ".") & " -1500. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "z = z-(z-%tk-%ts)*(%f13-15. +(%tk+%ts))/(-15)")
        ' - PrintLine(7, "y = y - (y /60.) * ((-1*%f13)-(-15. +(%tk+%ts)))")
        ' - PrintLine(7, "end")


        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Infinite Elements +y direction ==")
        ' - PrintLine(7, "start") 'Infinite elements facing in + y-direction
        ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
        ' - PrintLine(7, "1 2; 1 %f10;")
        '' - PrintLine(7, "0.0 %f1 %f4 %f6 300.")
        ' - PrintLine(7, "0.0 %f1 %f4 %f6 " & CStr(gSlabSize).Replace(",", "."))
        ' - PrintLine(7, "%f6Y [%f6Y + (%f6Y/60.)*(%f13 - 15. + (%tk + %ts))]")
        ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
        ' - PrintLine(7, "mate 9")
        ' - PrintLine(7, "edit")
        '' - PrintLine(7, "phr -1500. 152. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "phr -1500. " & CStr((gSlabSize / 2 + 2)).Replace(",", ".") & " -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "z = z-(z-%tk-%ts)*(%f13- 15. +(%tk+%ts))/(-15)")
        '' - PrintLine(7, "phr 0. 152. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "phr 0. " & CStr((gSlabSize / 2 + 2)).Replace(",", ".") & " -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "x = x -(x /60.) * ((-1*%f13)-(-15.+(%tk+%ts)))")
        '' - PrintLine(7, "phr -1500. 0. -1500. -152. 1500. 0.")
        ' - PrintLine(7, "phr -1500. 0. -1500. " & CStr((-gSlabSize / 2 - 2)).Replace(",", ".") & " 1500. 0.")
        ' - PrintLine(7, "x = x - 2. * (x /60.) * ((-1*%f13)-(- 15. +(%tk+%ts)))")
        ' - PrintLine(7, "end")



        If SymmSngl = False Then
            ' - PrintLine(7, " ")
            ' - PrintLine(7, "c == Infinite Elements -y direction ==")
            ' - PrintLine(7, "pslv 2")
            ' - PrintLine(7, "start") 'Infinite elements facing in - y-direction
            '' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
            ' - PrintLine(7, "1 [1 + %f7] [1 + %f7 + %f5] [1 + %f7 + %f5 + %f2] [1 + %f3 + %f2 + %f5 + %f7];")
            ' - PrintLine(7, "1 2; 1 %f10;")
            '' - PrintLine(7, "0.0 %f1 %f4 %f6 300.")
            ' - PrintLine(7, "-300.0 [-%f6] [-%f4] [-%f1] 0.")
            ' - PrintLine(7, "[%f6Y] [%f6Y + (%f6Y/60.)*(%f13 - 15. + (%tk + %ts))]")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
            ' - PrintLine(7, "mate 9")
            ' - PrintLine(7, "edit")
            ' - PrintLine(7, "phr -1500. 152. -1500. 1500. 1500. 0.")
            ' - PrintLine(7, "z = z-(z-%tk-%ts)*(%f13- 15. +(%tk+%ts))/(-15)")
            ' - PrintLine(7, "phr 0. 152. -1500. 1500. 1500. 0.")
            ' - PrintLine(7, "x = x -(x /60.) * ((-1*%f13)-(-15.+(%tk+%ts)))")
            ' - PrintLine(7, "phr -1500. 152. -1500. 0. 1500. 0.")
            ' - PrintLine(7, "x = x -(x /60.) * ((-1*%f13)-(-15.+(%tk+%ts)))")
            ' - PrintLine(7, "end")
            ' - PrintLine(7, "pplv")
        End If


        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Infinite Elements -z direction ==")
        ' - PrintLine(7, "start") 'Infinite elements facing downward along x-axis (loaded side)
        If SymmSngl = True Then
            ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
            ' - PrintLine(7, "1 [1 + %f3Y] [1 + %f3Y + %f5Y];")
            ' - PrintLine(7, "1 %f9;")
            '' - PrintLine(7, "0.0 %f1 %f4 %f6 300.0")
            ' - PrintLine(7, "0.0 %f1 %f4 %f6 " & CStr(gSlabSize).Replace(",", "."))
            ' - PrintLine(7, "0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-%f13 ] [-15. + (%tk + %ts)]")
        Else
            ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
            ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
            ' - PrintLine(7, "1 %f9;")
            '' - PrintLine(7, "0.0 %f1 %f4 %f6 300.0")
            ' - PrintLine(7, "0.0 %f1 %f4 %f6 " & CStr(gSlabSize).Replace(",", "."))
            ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-%f13 ] [-15. + (%tk + %ts)]")
        End If
        ' - PrintLine(7, "mate 7")
        ' - PrintLine(7, "edit")
        ' - PrintLine(7, "phr -1500. -1500. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "y = y - (y /60.) * (z - (-15. + (%tk + %ts)))")
        ' - PrintLine(7, "phr 0. -1500. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "x = x - (x /60.) * (z - (-15. + (%tk + %ts)))")
        ' - PrintLine(7, "end")



        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Base & Subbase Layers ==")
        If SymmSngl = True Then ' for case of symmetry on x-z plane
            ' - PrintLine(7, "start") 'BASE and SUBBASE layers along x-axis (Loaded side)
            If SubBaseExists = True Then
                ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
                ' - PrintLine(7, "1 [1 + %f3Y] [1 + %f3Y + %f5Y];")
                Print(7, "1 [1 + %f12] [1 + 2 * (%f12)]")
                If NumberOfLayers > 4 Then Print(7, " [1 + 3 * %f12]")
                If NumberOfLayers > 5 Then Print(7, " [1 + 4 * %f12]")
                ' - PrintLine(7, ";")
                '' - PrintLine(7, "0.0 %f1 %f4 %f6 300.0")
                ' - PrintLine(7, "0.0 %f1 %f4 %f6 " & CStr(gSlabSize).Replace(",", "."))
                ' - PrintLine(7, "0.0 %f1Y %f6Y")
                If NumberOfLayers > 5 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %t4 + %tk] [%t3 + %tk] %tk %p6")
                    ' - PrintLine(7, "si+ 1 1 5 5 3 5 1 m 0 0 1")
                    ' - PrintLine(7, "mt 1 1 1 5 3 2 5")
                    ' - PrintLine(7, "mt 1 1 2 5 3 3 4")
                    ' - PrintLine(7, "mt 1 1 3 5 3 4 3")
                    ' - PrintLine(7, "mt 1 1 4 5 3 5 2")
                ElseIf NumberOfLayers > 4 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %tk] %tk %p6")
                    ' - PrintLine(7, "si+ 1 1 4 5 3 4 1 m 0 0 1")
                    ' - PrintLine(7, "mt 1 1 1 5 3 2 4")
                    ' - PrintLine(7, "mt 1 1 2 5 3 3 3")
                    ' - PrintLine(7, "mt 1 1 3 5 3 4 2")
                Else
                    ' - PrintLine(7, "[%ts + %tk] %tk %p6")
                    ' - PrintLine(7, "si+ 1 1 3 5 3 3 1 m 0 0 1")
                    ' - PrintLine(7, "mt 1 1 1 5 3 2 3")
                    ' - PrintLine(7, "mt 1 1 2 5 3 3 2")
                End If
            Else
                ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
                Print(7, "1 [1 + %f3Y] [1 + %f3Y + %f5Y]; 1 [1 + %f12];")
                '' - PrintLine(7, " 0.0 %f1 %f4 %f6 300.0")
                ' - PrintLine(7, " 0.0 %f1 %f4 %f6 " & CStr(gSlabSize).Replace(",", "."))
                ' - PrintLine(7, " 0.0 %f1Y %f6Y")
                ' - PrintLine(7, "%tk %p6")
                ' - PrintLine(7, "si+ 1 1 2 5 3 2 1 m 0 0 1")
                ' - PrintLine(7, "mate 2")
            End If
            ' - PrintLine(7, "end")
        Else 'for case of no symmetery
            ' - PrintLine(7, " ")
            ' - PrintLine(7, "start") 'BASE and SUBBASE layers along x-axis (Loaded side)
            If SubBaseExists = True Then
                ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
                ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
                Print(7, "1 [1 + %f12] [1 + 2 * (%f12)]")
                If NumberOfLayers > 4 Then Print(7, " [1 + 3 * %f12]")
                If NumberOfLayers > 5 Then Print(7, " [1 + 4 * %f12]")
                ' - PrintLine(7, ";")
                ' - PrintLine(7, "0.0 %f1 %f4 %f6 300.0")
                ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
                If NumberOfLayers > 5 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %t4 + %tk] [%t3 + %tk] %tk %p6")
                    ' - PrintLine(7, "si+ 1 1 5 5 5 5 1 m 0 0 1")
                    ' - PrintLine(7, "mt 1 1 1 5 5 2 5")
                    ' - PrintLine(7, "mt 1 1 2 5 5 3 4")
                    ' - PrintLine(7, "mt 1 1 3 5 5 4 3")
                    ' - PrintLine(7, "mt 1 1 4 5 5 5 2")
                ElseIf NumberOfLayers > 4 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %tk] %tk %p6")
                    ' - PrintLine(7, "si+ 1 1 4 5 5 4 1 m 0 0 1")
                    ' - PrintLine(7, "mt 1 1 1 5 5 2 4")
                    ' - PrintLine(7, "mt 1 1 2 5 5 3 3")
                    ' - PrintLine(7, "mt 1 1 3 5 5 4 2")
                Else
                    ' - PrintLine(7, "[%ts + %tk] %tk %p6")
                    ' - PrintLine(7, "si+ 1 1 3 5 5 3 1 m 0 0 1")
                    ' - PrintLine(7, "mt 1 1 1 5 5 2 3")
                    ' - PrintLine(7, "mt 1 1 2 5 5 3 2")
                End If
            Else
                ' - PrintLine(7, "1 [1 + %f3] [1 + %f3 + %f2] [1 + %f3 + %f2 + %f5] [1 + %f3 + %f2 + %f5 + %f7];")
                ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
                ' - PrintLine(7, "1 [1 + %f12];")
                '' - PrintLine(7, " 0.0 %f1 %f4 %f6 300.0")
                ' - PrintLine(7, " 0.0 %f1 %f4 %f6 " & CStr(gSlabSize).Replace(",", "."))
                ' - PrintLine(7, " [-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
                ' - PrintLine(7, "%tk %p6")
                ' - PrintLine(7, "si+ 1 1 2 5 5 2 1 m 0 0 1")
                ' - PrintLine(7, "mate 2")
            End If
            ' - PrintLine(7, "end")
        End If


        'Extended base support beyond the slab edge - modified drb 12/02
        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Subbase Layers - Regular Elements, Extended ==")
        If SymmSngl = True Then
            ' - PrintLine(7, "start") 'Subbase Layers - Regular Elements Extended
            If SubBaseExists = True Then
                ' - PrintLine(7, "1 %f14; 1 [1 + %f3Y] [1 + %f3Y + %f5Y];")
                ' - PrintLine(7, "1 [1 + %f12] [1 + 2 * (%f12)]")
                If NumberOfLayers > 4 Then Print(7, " [1 + 3 * %f12]")
                If NumberOfLayers > 5 Then Print(7, " [1 + 4 * %f12]")
                ' - PrintLine(7, ";")
                ' - PrintLine(7, "%f24 0.0")
                ' - PrintLine(7, "0.0 %f1Y %f6Y")
                If NumberOfLayers > 5 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %t4 + %tk] [%t3 + %tk] %tk %p6")
                    '        Print #7, "si+ 1 1 5 2 4 5 1 m 0 0 1"
                    ' - PrintLine(7, "mt 1 1 1 2 3 2 5")
                    ' - PrintLine(7, "mt 1 1 2 2 3 3 4")
                    ' - PrintLine(7, "mt 1 1 3 2 3 4 3")
                    ' - PrintLine(7, "mt 1 1 4 2 3 5 2")
                ElseIf NumberOfLayers > 4 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %tk] %tk %p6")
                    '        Print #7, "si+ 1 1 4 2 4 4 1 m 0 0 1"
                    ' - PrintLine(7, "mt 1 1 1 2 3 2 4")
                    ' - PrintLine(7, "mt 1 1 2 2 3 3 3")
                    ' - PrintLine(7, "mt 1 1 3 2 3 4 2")
                Else
                    ' - PrintLine(7, "[%ts + %tk] %tk %p6")
                    '        Print #7, "si+ 1 1 3 2 4 3 1 m 0 0 1"
                    ' - PrintLine(7, "mt 1 1 1 2 3 2 3")
                    ' - PrintLine(7, "mt 1 1 2 2 3 3 2")
                End If
            Else
                ' - PrintLine(7, "1 %f14; 1 [1 + %f3Y] [1 + %f3Y + %f5Y]; 1 [1 + %f12];")
                ' - PrintLine(7, "%f24 0.0")
                ' - PrintLine(7, "0.0 %f1Y %f6Y")
                ' - PrintLine(7, "%tk %p6")
                ' - PrintLine(7, "mate 2")
            End If
            ' - PrintLine(7, "end")

            '' - PrintLine(7, "start") 'SUBGRADE layer - regular elements extended
            'Print #7, "1 %f14; 1 [1 + %f3Y] [1 + %f3Y + %f2Y] [1 + %f3Y + %f2Y + %f5Y]; 1 %f10;"
            '' - PrintLine(7, "1 %f14; 1 [1 + %f3Y] [1 + %f3Y + %f5Y]; 1 %f10;")
            'ikawa ' - PrintLine(7, "-24.0 0.0")
            '' - PrintLine(7, "%f24 0.0")

            'Print #7, "0.0 %f1Y %f4Y %f6Y"
            '' - PrintLine(7, "0.0 %f1Y %f6Y")
            '' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
            '' - PrintLine(7, "mate 6")
            '' - PrintLine(7, "end")
        Else ' case of no symmetry
            ' - PrintLine(7, " ")
            ' - PrintLine(7, "start") 'Subbase Layers - Regular Elements Extended
            If SubBaseExists = True Then
                ' - PrintLine(7, "1 %f14;")
                ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
                ' - PrintLine(7, "1 [1 + %f12] [1 + 2 * (%f12)]")
                If NumberOfLayers > 4 Then Print(7, " [1 + 3 * %f12]")
                If NumberOfLayers > 5 Then Print(7, " [1 + 4 * %f12]")
                ' - PrintLine(7, ";")
                ' - PrintLine(7, "%f24 0.0")
                ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
                If NumberOfLayers > 5 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %t4 + %tk] [%t3 + %tk] %tk %p6")
                    '        Print #7, "si+ 1 1 5 2 4 5 1 m 0 0 1"
                    ' - PrintLine(7, "mt 1 1 1 2 5 2 5")
                    ' - PrintLine(7, "mt 1 1 2 2 5 3 4")
                    ' - PrintLine(7, "mt 1 1 3 2 5 4 3")
                    ' - PrintLine(7, "mt 1 1 4 2 5 5 2")
                ElseIf NumberOfLayers > 4 Then
                    ' - PrintLine(7, "[%ts + %tk] [%t3 + %tk] %tk %p6")
                    '        Print #7, "si+ 1 1 4 2 4 4 1 m 0 0 1"
                    ' - PrintLine(7, "mt 1 1 1 2 5 2 4")
                    ' - PrintLine(7, "mt 1 1 2 2 5 3 3")
                    ' - PrintLine(7, "mt 1 1 3 2 5 4 2")
                Else
                    ' - PrintLine(7, "[%ts + %tk] %tk %p6")
                    '        Print #7, "si+ 1 1 3 2 4 3 1 m 0 0 1"
                    ' - PrintLine(7, "mt 1 1 1 2 5 2 3")
                    ' - PrintLine(7, "mt 1 1 2 2 5 3 2")
                End If
            Else
                ' - PrintLine(7, "1 %f14;")
                ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
                ' - PrintLine(7, "1 [1 + %f12];")
                ' - PrintLine(7, "%f24 0.0")
                ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
                ' - PrintLine(7, "%tk %p6")
                ' - PrintLine(7, "mate 2")
            End If
            ' - PrintLine(7, "end")
        End If


        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Subgrade Layer - Regular Elements, Extended ==")
        ' - PrintLine(7, "start") 'SUBGRADE layer - regular elements extended
        If SymmSngl = True Then
            ' - PrintLine(7, "1 %f14; 1 [1 + %f3Y] [1 + %f3Y + %f5Y]; 1 %f10;")
            ' - PrintLine(7, "%f24 0.0")
            ' - PrintLine(7, "0.0 %f1Y %f6Y")
        Else ' case of no symmetry
            ' - PrintLine(7, "1 %f14;")
            ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
            ' - PrintLine(7, "1 %f10;")
            ' - PrintLine(7, "%f24 0.0")
            ' - PrintLine(7, "[-%f6Y] [-%f1Y]0.0 %f1Y %f6Y")
        End If
        ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
        ' - PrintLine(7, "mate 6")
        ' - PrintLine(7, "end")



        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Extended Subgrade Layer - Infinite Elements in -z direction ==")
        ' - PrintLine(7, "start") 'SUBGRADE layer - infinite elements -z direction, extended region
        If SymmSngl = True Then
            ' - PrintLine(7, "1 %f14; 1 [1 + %f3Y] [1 + %f3Y + %f5Y]; 1 %f9;")
            ' - PrintLine(7, "%f24 0.0")
            ' - PrintLine(7, "0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-%f13 ] [-15. + (%tk + %ts)]")
        Else
            ' - PrintLine(7, "1 %f14;")
            ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
            ' - PrintLine(7, "1 %f9;")
            ' - PrintLine(7, "%f24 0.0")
            ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-%f13 ] [-15. + (%tk + %ts)]")
        End If
        ' - PrintLine(7, "mate 7")
        ' - PrintLine(7, "edit")
        ' - PrintLine(7, "phr -1500. -1500. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "y = y - (y /60.) * (z - (-15. + (%tk + %ts)))")
        ' - PrintLine(7, "phr -1500. -1500. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "x = x - (x /60.) * (z - (-15. + (%tk + %ts)))")
        ' - PrintLine(7, "end")




        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Extended Subgrade Layer - Infinite Elements in +y direction ==")
        ' - PrintLine(7, "start") 'SUBGRADE layer - infinite elements facing in + y-direction, extended region
        ' - PrintLine(7, "1 %f14; 1 2; 1 %f10;")
        ' - PrintLine(7, "%f24 0.0")
        ' - PrintLine(7, "%f6Y [%f6Y + (%f6Y/60.)*(%f13 - 15. + (%tk + %ts))]")
        ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
        ' - PrintLine(7, "mate 9")
        ' - PrintLine(7, "edit")
        '' - PrintLine(7, "phr -1500. 152. -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "phr -1500. " & CStr((gSlabSize / 2 + 2)).Replace(",", ".") & " -1500. 1500. 1500. 0.")
        ' - PrintLine(7, "z = z-(z-%tk-%ts)*(%f13- 15. +(%tk+%ts))/(-15)")
        '' - PrintLine(7, "phr -1500. 152. -1500. 0. 1500. 0.")
        ' - PrintLine(7, "phr -1500. " & CStr((gSlabSize / 2 + 2)).Replace(",", ".") & " -1500. 0. 1500. 0.")
        ' - PrintLine(7, "x = x -(x /60.) * ((-1*%f13)-(-15.+(%tk+%ts)))")
        '' - PrintLine(7, "phr -1500. 0. -1500. -152. 1500. 0.")
        ' - PrintLine(7, "phr -1500. 0. -1500. " & CStr((-gSlabSize / 2 - 2)).Replace(",", ".") & " 1500. 0.")
        ' - PrintLine(7, "x = x - 2. * (x /60.) * ((-1*%f13)-(- 15. +(%tk+%ts)))")
        ' - PrintLine(7, "end")


        If SymmSngl = False Then
            ' - PrintLine(7, " ")
            ' - PrintLine(7, "c == Extended Subgrade Layer - Infinite Elements in -y direction ==")
            ' - PrintLine(7, "pslv 2")
            ' - PrintLine(7, "start") 'SUBGRADE layer - infinite elements facing in + y-direction, extended region
            ' - PrintLine(7, "1 %f14; 1 2; 1 %f10;")
            ' - PrintLine(7, "0.0 [-%f24]")
            ' - PrintLine(7, "[%f6Y] [%f6Y + (%f6Y/60.)*(%f13 - 15. + (%tk + %ts))]")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
            ' - PrintLine(7, "mate 9")
            ' - PrintLine(7, "edit")
            '' - PrintLine(7, "phr -1500. 152. -1500. 1500. 1500. 0.")
            ' - PrintLine(7, "phr -1500. 152. -1500. 1500. 1500. 0.")
            ' - PrintLine(7, "z = z-(z-%tk-%ts)*(%f13- 15. +(%tk+%ts))/(-15)")
            ' - PrintLine(7, "phr -1500. 152. -1500. 0. 1500. 0.")
            ' - PrintLine(7, "x = x - 2.* (x /60.) * ((-1*%f13)-(-15.+(%tk+%ts)))")
            ' - PrintLine(7, "phr 0. 152. -1500. 1500. 1500. 0.")
            ' - PrintLine(7, "x = x - (x /60.) * ((-1*%f13)-(-15.+(%tk+%ts)))")
            ' - PrintLine(7, "end")
            ' - PrintLine(7, "pplv")
        End If




        ' - PrintLine(7, " ")
        ' - PrintLine(7, "c == Extended Subgrade Layer - Infinite Elements in -x direction ==")
        ' - PrintLine(7, "start") 'infinite elements facing in -x direction
        If SymmSngl = True Then
            ' - PrintLine(7, "1 2;")
            ' - PrintLine(7, "1 [1 + %f3Y] [1 + %f3Y + %f5Y];")
            ' - PrintLine(7, "1 %f10;")
            ' - PrintLine(7, "%f24 [%f24 - (-%f24/60.)*(%f13 - 15. +(%tk + %ts))]")
            ' - PrintLine(7, "0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
            ' - PrintLine(7, "mate 10")
            ' - PrintLine(7, "b 2 1 1 2 3 2 111111")
        Else
            ' - PrintLine(7, "1 2;")
            ' - PrintLine(7, "1 [1 + %f5Y] [1 + %f3Y + %f5Y] [1 + 2.* %f3Y + %f5Y] [1 + 2.* %f3Y + 2.* %f5Y];")
            ' - PrintLine(7, "1 %f10;")
            ' - PrintLine(7, "%f24 [%f24 - (-%f24/60.)*(%f13 - 15. +(%tk + %ts))]")
            ' - PrintLine(7, "[-%f6Y] [-%f1Y] 0.0 %f1Y %f6Y")
            ' - PrintLine(7, "[-15. + %tk + %ts] [%tk + %ts]")
            ' - PrintLine(7, "mate 10")
            ' - PrintLine(7, "b 2 1 1 2 5 2 111111")
        End If
        ' - PrintLine(7, "edit")
        ' - PrintLine(7, "phr -1500. -1500. -1500. -25. 1500. 0")
        ' - PrintLine(7, "z = z - (z-%tk-%ts)*(%f13-15. +(%tk+%ts))/(-15)")
        ' - PrintLine(7, "y = y - (y /60.) * ((-1*%f13)-(-15. +(%tk+%ts)))")
        ' - PrintLine(7, "end")


999:

    End Sub


    Sub Material_No_Inf()
        Dim IFOption, IFOption1, IFOption2, IFOption3 As String

        ' - PrintLine(7, "Rigid Airport Pavement")
        ' - PrintLine(7, "")
        ' - PrintLine(7, "nk3d")
        ' - PrintLine(7, "anal stat")
        ' - PrintLine(7, "prcd 1 2 0 1.0 1.0 1.0")

        Dim ii, jj, kk As Integer
        Dim SS1, SS2 As String

        kk = 0
        For ii = 1 To gNACarg
            If NCat(iCat) > 1 Then
                If GroupIndex(ii) = iCat Then
                    kk += 1
                    SS1 = "lcd "
                    SS1 = SS1 & CStr(kk) & " " & CStr(NCat(iCat) + 1)
                    SS2 = " "

                    For jj = 0 To Math.Min(8, NCat(iCat))
                        If jj = kk Then
                            SS2 = SS2 + CDbl(jj).ToString("0.0") & " 1.0 "
                        Else
                            SS2 = SS2 + CDbl(jj).ToString("0.0") & " 0.0 "
                        End If
                    Next
                    SS1 = SS1 + SS2
                    ' - PrintLine(7, SS1)
                End If
            Else
                If GroupIndex(ii) = iCat Then
                    ' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
                End If
            End If
        Next ii


        '' - PrintLine(7, "lcd 2 2 0 0 1.0 1.0")
        '-----------------------------------------------
        IFOption = "sv"
        IFOption1 = "sl" 'sliding only - interface type 'LIA
        IFOption2 = "tied" 'tied sliding - interface type 'LIA
        IFOption3 = "sv" 'sliding with voids - interface type 'LIA

        If Factor2 > 0.001! Then
            Friction = 0.005
        Else
            Friction = 0.0#
        End If

        'ikawa If Factor2 > 99.0# Then IFOption = "tied"
        If Factor2 > 0.1 Then IFOption = "tied"

        Factor1 = 10
        Factor3 = 10
        ' - PrintLine(7, "si 1 " & IFOption3 & " pnlt " & CStr(Factor3).Replace(",", ".") & " fric " & "0.0" & " nomerge;")

        If Overlay Then
            ' Change overlay index by YGC 060311
            '' - PrintLine(7, "si 2 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;")
            ' - PrintLine(7, "si 10 " & IFOption & " pnlt " & CStr(Factor1).Replace(",", ".") & " fric " & CStr(Friction).Replace(",", ".") & " nomerge;")
        End If
        ' - PrintLine(7, "")

        'concrete slab material
        ' - PrintLine(7, "mat  1  1" & "  e " & LPad(11, Format(EMod(1), "####000.00")) _
        '       & " pr " & Format(PoissonsRatio(1), "#0.00") & " ro 2.247e-4")
        'upper subbase material
        ' - PrintLine(7, "mat  2  1" & "  e " & LPad(11, Format(EMod(2), "####000.00")) _
        '       & " pr " & Format(PoissonsRatio(2), "#0.00") & " ro 1.872e-4")
        '2nd subbase material
        ' - PrintLine(7, "mat  3  1" & "  e " & LPad(11, Format(EMod(3), "####000.00")) _
        '       & " pr " & Format(PoissonsRatio(3), "#0.00") & " ro 1.872e-4")
        '3rd subbase material
        ' - PrintLine(7, "mat  4  1" & "  e " & LPad(11, Format(EMod(4), "####000.00")) _
        '       & " pr " & Format(PoissonsRatio(4), "#0.00") & " ro 1.872e-4")
        '4th subbase material
        ' - PrintLine(7, "mat  5  1" & "  e " & LPad(11, Format(EMod(5), "####000.00")) _
        '       & " pr " & Format(PoissonsRatio(5), "#0.00") & " ro 1.872e-4")

        'subgrade material
        ' - PrintLine(7, "mat  6  1" & "  e " & LPad(11, Format(EMod(6), "####000.00")) _
        '       & " pr " & Format(PoissonsRatio(6), "#0.00") & " ro 1.872e-4")

        If InfiniteElement Then
            'infinite direction downward (-z)
            ' - PrintLine(7, "mat  7  1" & "  e " & LPad(11, Format(EMod(6), "####000.00")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00") & " ro 1.872e-4")

            'infinite direction +x
            ' - PrintLine(7, "mat  8  1" & "  e " & LPad(11, Format(EMod(6), "####000.00")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00") & " ro 1.872e-4")

            'infinite direction +y
            ' - PrintLine(7, "mat  9  1" & "  e " & LPad(11, Format(EMod(6), "####000.00")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00") & " ro 1.872e-4")

            'infinite direction -x
            ' - PrintLine(7, "mat 10  1" & "  e " & LPad(11, Format(EMod(6), "####000.00")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00") & " ro 1.872e-4")

            'ikawa If Not SymmSngl Then
            'infinite direction -y
            ' - PrintLine(7, "mat 11  1" & "  e " & LPad(11, Format(EMod(6), "####000.00")) _
            '       & " pr " & Format(PoissonsRatio(6), "#0.00") & " ro 1.872e-4")
            'ikawa End If
        End If
        If Overlay Then
            ' - PrintLine(7, "mat 12 1" & "  e " & LPad(11, Format(EMod(0), "####000.00")) _
            '       & " pr " & Format(PoissonsRatio(0), "#0.00") & " ro 2.247e-4")
        End If

        ' - PrintLine(7, "endmat")
        ' - PrintLine(7, "")
    End Sub

    Sub DummyNodes()
        'added to replace fixed corner node BC by spring BC by YGC 060311

        'sets up a 2D mesh containing the fixed "dummy" nodes that are used to constrain the corners of slabs
        ' - PrintLine(7, "c dummy nodes")
        Print(7, "parameter tk " & -LayerThickness(1)) 'slab thickness
        ' - PrintLine(7, " tkol " & -LayerThickness(0) & ";") 'overlay thickness
        If SymmSngl = True Then
            ' - PrintLine(7, "start")
            ' - PrintLine(7, "1 2 3 4; 1 2 3; -1;")
            ' - PrintLine(7, "-300.1 -0.1 300.1 600.1")
            ' - PrintLine(7, "0.0 150.0 450.0")
            ' - PrintLine(7, "[-%tk]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
            ' cancelled for an running error when N>1 by YGC 091310
            ' - PrintLine(7, "b 1 1 0 4 3 0 111111")
            ' - PrintLine(7, "end")

            ' - PrintLine(7, "start")
            ' - PrintLine(7, "1 2 3 4; 1 2 3; -1;")
            ' - PrintLine(7, "-300.0 0.0 300.0 600.0")
            ' - PrintLine(7, "-0.1 150.1 450.1")
            ' - PrintLine(7, "[-%tk]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
            ' cancelled for an running error when N>1 by YGC 091310
            ' - PrintLine(7, "b 1 1 0 4 3 0 111111")
            ' - PrintLine(7, "end")
        Else
            ' - PrintLine(7, "start")
            ' - PrintLine(7, "1 2 3 4; 1 2 3 4; -1;")
            ' - PrintLine(7, "-300.1 -0.1 300.1 600.1")
            ' - PrintLine(7, "-450.0 -150.0 150.0 450.0")
            ' - PrintLine(7, "[-%tk]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
            ' cancelled for an running error when N>1 by YGC 091310
            ' - PrintLine(7, "b 1 1 0 4 4 0 111111")
            ' - PrintLine(7, "end")

            ' - PrintLine(7, "start")
            ' - PrintLine(7, "1 2 3 4; 1 2 3 4; -1;")
            ' - PrintLine(7, "-300.0 0.0 300.0 600.0")
            ' - PrintLine(7, "-450.1 -150.1 150.1 450.1")
            ' - PrintLine(7, "[-%tk]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
            ' cancelled for an running error when N>1 by YGC 091310
            ' - PrintLine(7, "b 1 1 0 4 4 0 111111")
            ' - PrintLine(7, "end")
        End If

        If Overlay = True And Factor2 <= 0.1 Then 'for UnBound PCC Overlay by YGC 042312 
            If SymmSngl = True Then
                ' - PrintLine(7, "start")
                ' - PrintLine(7, "1 2 3 4; 1 2 3; -1;")
                ' - PrintLine(7, "-300.1 -0.1 300.1 600.1")
                ' - PrintLine(7, "0.0 150.0 450.0")
                ' - PrintLine(7, "[-%tk - %tkol]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
                ' cancelled for an running error when N>1 by YGC 091310
                ' - PrintLine(7, "b 1 1 0 4 3 0 111111")
                ' - PrintLine(7, "end")

                ' - PrintLine(7, "start")
                ' - PrintLine(7, "1 2 3 4; 1 2 3; -1;")
                ' - PrintLine(7, "-300.0 0.0 300.0 600.0")
                ' - PrintLine(7, "-0.1 150.1 450.1")
                ' - PrintLine(7, "[-%tk - %tkol]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
                ' cancelled for an running error when N>1 by YGC 091310
                ' - PrintLine(7, "b 1 1 0 4 3 0 111111")
                ' - PrintLine(7, "end")
            Else
                ' - PrintLine(7, "start")
                ' - PrintLine(7, "1 2 3 4; 1 2 3 4; -1;")
                ' - PrintLine(7, "-300.1 -0.1 300.1 600.1")
                ' - PrintLine(7, "-450.0 -150.0 150.0 450.0")
                ' - PrintLine(7, "[-%tk - %tkol]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
                ' cancelled for an running error when N>1 by YGC 091310
                ' - PrintLine(7, "b 1 1 0 4 4 0 111111")
                ' - PrintLine(7, "end")

                ' - PrintLine(7, "start")
                ' - PrintLine(7, "1 2 3 4; 1 2 3 4; -1;")
                ' - PrintLine(7, "-300.0 0.0 300.0 600.0")
                ' - PrintLine(7, "-450.1 -150.1 150.1 450.1")
                ' - PrintLine(7, "[-%tk - %tkol]") 'modified to lower than loading surface to eliminate dummy loads by YGC 073010
                ' cancelled for an running error when N>1 by YGC 091310
                ' - PrintLine(7, "b 1 1 0 4 4 0 111111")
                ' - PrintLine(7, "end")
            End If
        End If


    End Sub

    Sub InfSlbDummyNodes()
        'to seperate sym and unsym gear by YC 082216 092016
        'rewritten by YC 101215 092016
        'added for spring BC to simulate infinite slab by YGC 092613
        'modified for overlay by YGC 112213

        INGString = INGString & "c dummy nodes for slab outside joint" & vbCrLf

        If Overlay = True Then

            INGString = INGString & "start" & vbCrLf &
            "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
            "-1;" & vbCrLf &
            "1 2 3;" & vbCrLf &
            "0.0 210 300.0" & vbCrLf &
            "150.02" & vbCrLf &
            "0 " & -tkpcc & " " & -tkpcc - tkol & vbCrLf &
            "b 1 0 1 3 0 3 111111" & vbCrLf &
            "end" & vbCrLf & vbCrLf

            If MeshCase = "SYM" Then
                INGString = INGString & "start" & vbCrLf &
                "-1;" & vbCrLf &
                "1 " & (1 + NY1) & " " & (1 + NY1 + NY2) & ";" & vbCrLf &
                "1 2 3;" & vbCrLf &
                "300.02" & vbCrLf &
                "0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & " " & -tkpcc - tkol & vbCrLf &
                "b 0 1 1 0 3 3 111111" & vbCrLf &
                "end" & vbCrLf & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & "start" & vbCrLf &
                "-1;" & vbCrLf &
                "1 " & (1 + NY2) & " " & (1 + NY2 + NY1) & " " & (1 + NY2 + 2 * NY1) & " " & (1 + 2 * NY2 + 2 * NY1) & ";" & vbCrLf &
                "1 2 3;" & vbCrLf &
                "300.02" & vbCrLf &
                "-150 -60 0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & " " & -tkpcc - tkol & vbCrLf &
                "b 0 1 1 0 5 3 111111" & vbCrLf &
                "end" & vbCrLf & vbCrLf

                INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "-1;" & vbCrLf &
                "1 2 3;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "-150.02" & vbCrLf &
                "0 " & -tkpcc & " " & -tkpcc - tkol & vbCrLf &
                "b 1 0 1 3 0 3 111111" & vbCrLf &
                "end" & vbCrLf & vbCrLf
            End If

        Else

            INGString = INGString & "start" & vbCrLf &
                "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                "-1;" & vbCrLf &
                "1 2;" & vbCrLf &
                "0.0 210 300.0" & vbCrLf &
                "150.02" & vbCrLf &
                "0 " & -tkpcc & vbCrLf &
                "b 1 0 1 3 0 2 111111" & vbCrLf &
                "end" & vbCrLf & vbCrLf

            If MeshCase = "SYM" Then
                INGString = INGString & "start" & vbCrLf &
                "-1;" & vbCrLf &
                 "1 " & (1 + NY1) & " " & (1 + NY1 + NY2) & ";" & vbCrLf &
                "1 2;" & vbCrLf &
                "300.02" & vbCrLf &
                "0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & vbCrLf &
                "b 0 1 1 0 3 2 111111" & vbCrLf &
                "end" & vbCrLf & vbCrLf
            ElseIf MeshCase = "UNSYM" Then
                INGString = INGString & "start" & vbCrLf &
                "-1;" & vbCrLf &
                "1 " & (1 + NY2) & " " & (1 + NY2 + NY1) & " " & (1 + NY2 + 2 * NY1) & " " & (1 + 2 * NY2 + 2 * NY1) & ";" & vbCrLf &
                "1 2;" & vbCrLf &
                "300.02" & vbCrLf &
                "-150 -60 0.0 60 150.0" & vbCrLf &
                "0 " & -tkpcc & vbCrLf &
                "b 0 1 1 0 5 2 111111" & vbCrLf &
                 "end" & vbCrLf & vbCrLf

                INGString = INGString & "start" & vbCrLf &
                    "1 " & (1 + NX1) & " " & (1 + NX1 + NX2) & ";" & vbCrLf &
                    "-1;" & vbCrLf &
                    "1 2;" & vbCrLf &
                    "0.0 210 300.0" & vbCrLf &
                    "-150.02" & vbCrLf &
                    "0 " & -tkpcc & vbCrLf &
                    "b 1 0 1 3 0 2 111111" & vbCrLf &
                    "end" & vbCrLf & vbCrLf
            End If
        End If

    End Sub

    Sub Conversion()
        ' QW 08-23-2017
        Dim i, j, k, n As Integer
        With IPC
            NMat = MAT.Length - 1
            .nmmat = NMat
            .inpsd = 1
            .ntime = ACLoad.Length - 1
            .nlcur = LCD.Length - 1
            .nptm = LCD.Length
            .nload = 0
            n = .ntime
            For i = 1 To n
                .nload = .nload + NodalLoad(i).NNodalLoad
            Next i
            .numpc = 0
            .numdc = 0
            .nrcc = 0
            n = Nd.Length - 1
            For i = 0 To n - 2
                If Nd(i + 1).X = 0 And Nd(i + 1).Y = 0 And Nd(i + 1).Z = 0 And Nd(i + 2).X = 0 And Nd(i + 2).Y = 0 And Nd(i + 2).Z = 0 Then
                    GoTo out
                End If
            Next i
out:        .numnp = i + 1
            .numelh = BrickElement.Length - 1
            .nmmtde = SpringType.Length - 1
            .nmelde = SpringElement.Length - 1
            .nmmass = 0

            ' Material
            ReDim .matype(NMat), .den(NMat), .prop(48, NMat)
            For i = 1 To NMat
                .matype(i) = MAT(i).KeyMatPrpty
                .den(i) = MAT(i).DensityMat
                For j = 1 To 6
                    For k = 1 To 8
                        .prop(6 * (k - 1) + j, i) = MAT(i).MatPara(j, k)
                    Next k
                Next j
            Next i
            ' Node and element
            ReDim .idp(6, .numnp), .x(3, .numnp), .ia(9 * .numelh - 1), .nob(.numnp)
            For i = 1 To .numnp
                .nob(i) = Nd(i).BXYZ
                For j = 1 To 6
                    .idp(j, i) = 0
                Next j
                For j = 1 To 3
                    If Nd(i).BXYZ = j Then .idp(j, i) = 1
                Next j
                If Nd(i).BXYZ = 4 Then
                    .idp(1, i) = 1 : .idp(2, i) = 1
                End If
                If Nd(i).BXYZ = 5 Then
                    .idp(2, i) = 1 : .idp(3, i) = 1
                End If
                If Nd(i).BXYZ = 6 Then
                    .idp(1, i) = 1 : .idp(3, i) = 1
                End If
                If Nd(i).BXYZ = 7 Then
                    .idp(1, i) = 1 : .idp(2, i) = 1 : .idp(3, i) = 1
                End If
                For j = 1 To 3
                    If Nd(i).BRXYZ = j Then .idp(j + 3, i) = 1
                Next j
                If Nd(i).BRXYZ = 4 Then
                    .idp(4, i) = 1 : .idp(5, i) = 1
                End If
                If Nd(i).BRXYZ = 5 Then
                    .idp(5, i) = 1 : .idp(6, i) = 1
                End If
                If Nd(i).BRXYZ = 6 Then
                    .idp(4, i) = 1 : .idp(6, i) = 1
                End If
                If Nd(i).BRXYZ = 7 Then
                    .idp(4, i) = 1 : .idp(5, i) = 1 : .idp(6, i) = 1
                End If
                .x(1, i) = Nd(i).X
                .x(2, i) = Nd(i).Y
                .x(3, i) = Nd(i).Z
            Next i
            For i = 0 To .numelh - 1
                .ia(9 * i) = BrickElement(i + 1).IdxMat
                For j = 1 To 8
                    .ia(9 * i + j) = BrickElement(i + 1).Node(j)
                Next j
            Next i
            ' Discrete element
            ReDim .cmde(2, .nmmtde), .mtypde(.nmmtde), .ixde(3, .nmelde), .sclf(.nmelde)
            For i = 1 To .nmmtde
                .cmde(1, i) = SpringType(i).Stiffness
                .cmde(2, i) = SpringType(i).Direction
                .mtypde(i) = 1
            Next i
            For i = 1 To .nmelde
                .ixde(1, i) = SpringElement(i).Node1
                .ixde(2, i) = SpringElement(i).Node2
                .ixde(3, i) = SpringElement(i).IdxSprType
                .sclf(i) = 1
            Next i
            ' Sliding surface element

            .numsv = CInt((PARTSLD.Length - 1) / 2)
            ReDim .iparm(7, .numsv), .fric(3, .numsv), .pend(.numsv), .stfsf(.numsv), .ngap(.numsv)
            ReDim .iaug(.numsv), .sfact(.numsv), .altol(2, .numsv), .tdeath(.numsv), .tbury(.numsv), .ifd(.numsv)
            .nrttls = 0 : .nrttlm = 0
            For i = 1 To .numsv
                .nrttls = .nrttls + SlidingElement(2 * i - 1).NSldElement
                .nrttlm = .nrttlm + SlidingElement(2 * i).NSldElement
                .iparm(1, i) = SlidingElement(2 * i - 1).NSldElement
                .iparm(2, i) = SlidingElement(2 * i).NSldElement
                .iparm(5, i) = SlidingElement(2 * i).IdxDirection
                .stfsf(i) = SlidingElement(2 * i).PenaltySld
                If Math.Abs(SlidingElement(2 * i).FrictionSld - 0.005) < 0.0001 Then
                    .fric(1, i) = 0.005
                    .sfact(i) = ACLoad(1).f2

                    '.sfact(i) = CDbl(LPad(10, Format(ACLoad(1).f2, "0.000E+00"))) ' YC? 102418-041519-
                    .iacc = 1   ' YC 102418-041519
                Else                            ' FrictionSld only set equal to 0.005 or 0 in Sub Material_No_Inf()
                    .fric(1, i) = 0.0
                    .sfact(i) = 0.0

                    .iacc = 0   ' YC 102418-041519

                End If
                .iaug(i) = 0 : .ifd(i) = 0
                .altol(1, i) = 0.0 : .altol(2, i) = 0.0
                .tdeath(i) = 0.0 : .tbury(i) = 0.0
                .fric(2, i) = 0.0 : .fric(3, i) = 0.0
                If .stfsf(i) = 0.0 Then .stfsf(i) = 1.0
                If .ngap(i) = 0 Then .ngap(i) = 999999
                If .ngap(i) < 0 Then .ngap(i) = 0
            Next i
            ReDim .irects(4, .nrttls), .irectm(4, .nrttlm)
            n = 0
            For i = 1 To 2 * .numsv Step 2
                For j = 1 To SlidingElement(i).NSldElement
                    For k = 1 To 4
                        .irects(k, n + j) = SlidingElement(i).Element(j, k)
                    Next k
                Next j
                n = n + SlidingElement(i).NSldElement   ' QW 04-15-2019
            Next i
            n = 0
            For i = 2 To 2 * .numsv Step 2
                For j = 1 To SlidingElement(i).NSldElement
                    For k = 1 To 4
                        .irectm(k, n + j) = SlidingElement(i).Element(j, k)
                    Next k
                Next j
                n = n + SlidingElement(i).NSldElement   ' QW 04-15-2019
            Next i

            ' Load
            NAC = ACLoad.Length - 1

            ReDim .nod(.nload), .idirn(.nload), .ncur(.nload), .fac(.nload), .npc(.nlcur + 1), .pld(.nlcur * .nptm * 2)
            ReDim .tmode(.numnp), .cnwmk(2), .tbase(.numnp)

            'Dim timv(.nptm), rv(.nptm) As Single   ' YC 102418-041519
            Dim timv(.nptm), rv(.nptm) As Double

            .npc(1) = 1
            For i = 1 To .nlcur
                For j = 1 To .nptm
                    timv(j) = LCD(i).LdPntTime(j)
                    rv(j) = LCD(i).LdPntMag(j)
                Next j
                .npc(i + 1) = .npc(i) + 2 * .nptm
                j = .npc(i)
                For k = 1 To .nptm
                    .pld(j) = timv(k)
                    j = j + 1
                    .pld(j) = rv(k)
                    j = j + 1
                Next k
            Next i
            .nptst = .npc(.nlcur + 1)
            k = 0
            For i = 1 To NAC
                For j = 1 To NodalLoad(i).NNodalLoad
                    .nod(k + j) = NodalLoad(i).Node(j)
                    .idirn(k + j) = 3
                    .ncur(k + j) = i
                    .fac(k + j) = -NodalLoad(i).Load(j)
                Next j
                k = k + NodalLoad(i).NNodalLoad
            Next i

            If ACLoad(1).IsThermal Then
                .itemp = 1 : .itread = 1
                .cnwmk(1) = 0.5 : .cnwmk(2) = 0.25
                For i = 1 To .numnp
                    .tmode(i) = Nd(i).Temp
                    .tbase(i) = 0
                Next i
            Else
                .itemp = 0 : .itread = 0
                .cnwmk(1) = 0 : .cnwmk(2) = 0
                For i = 1 To .numnp
                    .tmode(i) = 0
                    .tbase(i) = 0
                Next i
            End If

        End With

        ' PathSet



    End Sub
End Module

