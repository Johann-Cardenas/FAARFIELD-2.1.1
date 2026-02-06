Option Strict On
Option Explicit On
Imports VB = Microsoft.VisualBasic
'Imports System.Xml.Serialization


Imports System.IO
Imports System.Runtime.ExceptionServices
Imports System.Threading
Imports ACClassLib
Imports FaarFieldModel
Imports FaarFieldModel.Interfaces
Imports Microsoft.VisualBasic.FileIO
Imports System

Imports System.Xml
Imports System.Globalization

Imports iTextSharp.text
Imports iTextSharp.text.html.simpleparser
Imports iTextSharp.text.pdf



Module ZFindLife

    '    Public iC1, iC2 As Integer
    '    Public globalLeft, globalTop As Integer


    '    Public cmdLifeOriginalX, cmdOriginalY As Integer
    '    Public cmdModifyStrOriginalX As Integer
    '    Public cmdAddDeleteOriginalX As Integer
    '    Public cmdSaveStrOriginalX As Integer

    '    Public htmlText As String
    '    'Public gPictureStructure As New PictureBox

    '    Structure structureinfo
    '        Public Shared no(100) As String
    '        Public Shared type(100) As String
    '        Public Shared thickness(100) As String
    '        Public Shared modulus(100) As String
    '        Public Shared poissonRatio(100) As String
    '        Public Shared streagth(100) As String
    '        Public Shared length As Int16
    '        Private dummy As Int16
    '    End Structure

    '    Dim PaveStruct As String
    '    Dim TotalThickness As String
    '    Private FAArfieldHTMLPath As String

    '    Public Sub CreateHTMLinfo(ByRef Info As String, ByRef PInfo As String, ByVal I As Integer, ByVal IncludeImage As Boolean)

    '        Dim DTemp As Double ' GFH 08/14/03.
    '        Dim ThicktoSubgrade As Single
    '        Dim LenLayerType As Single


    '        '======================================================================================================
    '        ' For display in the text box.
    '        'Info = "<html>" +
    '        '       "<head><title>FAARFIELD</title></head>" +
    '        '       "<body style=""font-size: 8pt; font-family: Arial"">" +
    '        '       "<b>" + frmStartup.Text + "</b><br><br>" +
    '        '       "Section " & SectName & " in Job " & JobName$ & ".<br>" +
    '        '       "Working directory is " & WorkingDir + "<br><br>"
    '        'Debug.WriteLine(frmStartup.Text)


    '        If Life <> DefaultLife Then
    '            Info = Info & NotDefaultLife & "<br>"
    '            'PInfo = PInfo & NotDefaultLife & NL2
    '        End If

    '        'If Not StandardStr Then 'CreateHTMLinfo
    '        '    Info = Info & NonStandardStr & "<br>"
    '        '    'PInfo = PInfo & NonStandardStr & NL2
    '        'End If

    '        If Not GoodACList Then
    '            Info = Info & BadACList & "<br>"
    '            'PInfo = PInfo & BadACList & NL2
    '        End If

    '        S = ""
    '        If DesignType = NewFlex Or DesignType = FlexOnFlex Then
    '            If Designed <> NullDate Then
    '                If CDFAsp = -1 Then
    '                    S = " Asphalt CDF was not computed."
    '                Else
    '                    S = " Asphalt CDF = " & Format(CDFAsp, "0.0000") & "."
    '                End If
    '            End If
    '        End If
    '        SS = ""
    '        If DesignType = NewFlex Then 'CreateHTMLInfo
    '            SS = "New Flexible"
    '        ElseIf DesignType = FlexOnFlex Then
    '            SS = "AC Overlay on Flexible"
    '        ElseIf DesignType = NewRigid Then
    '            SS = "New Rigid"
    '        ElseIf DesignType = UnbondOnRigid Then
    '            SS = "Unbonded PCC Overlay on Rigid"
    '        ElseIf DesignType = PartBondOnRigid Then
    '            SS = "Part Bonded PCC Overlay on Rigid"
    '        ElseIf DesignType = PCCOnFlex Then
    '            SS = "PCC Overlay on Flexible"
    '        ElseIf DesignType = FlexOnRigid Then
    '            SS = "AC Overlay on Rigid"
    '        End If
    '        PaveStruct = SS

    '        Info = Info & "The structure is " & SS & "." & S & "<br>"

    '        S = ""
    '        If OverlayRigOnRig Then
    '            S = "SCI of the existing pavement = " & Format(SCIB, "0") & "." & "<br>"
    '            If SCIB = 100.0! Then
    '                S = S & "Percent CDF used of the existing pavement = " & Format(LifeExistPCC, "0") & "." & "<br>"
    '            End If
    '        End If

    '        'S = S & NL
    '        S = S & "Design Life = " & Format(Life, "0") & " years." & "<br>" ' GFH 04/23/03.

    '        'bool1 = False : bool2 = False : bool3 = False
    '        If SMin = "Min" Then 'jobSMin(ISect)
    '            S = S & "A design for this section was completed on " & Format(Designed, "mm/dd/yy") & " at " & Format(Designed, "hh:mm:ss") & "." & "<br>"
    '            S = S & "Minimum layer thicknesses were reached." & "<br>"
    '        ElseIf Designed <> NullDate Then
    '            S = S & "A design for this section was completed on " & Format(Designed, "mm/dd/yy") & " at " & Format(Designed, "hh:mm:ss") & "." & "<br>"
    '        Else
    '            S = S & "A design has not been completed for this section." & "<br>"
    '        End If

    '        If AnalyzedasFlexible = True Then S = S & "The structure is being analyzed as a flexible section" 'Kairat 

    '        If DesignType = FlexOnRigid And Designed <> NullDate Then
    '            If HMAonRigidCase = 1 Then

    '            ElseIf HMAonRigidCase = 2 Then
    '                S = S & "<br>Minimum HMA thickness was reached. Therefore, CDF totals may be less than 1.0. (b)<br><br>"
    '            ElseIf HMAonRigidCase = 3 Then
    '                S = S & "<br>Overlay thickness was determined using a flexible pavement design procedure," & "<br>"
    '                S = S & "considering the existing PCC as a high-stiffness base layer." & "<br>"
    '                S = S & "CDF is determined for the top of the subgrade. Refer to AC 150/5320-6," & "<br>"
    '                S = S & "paragraph 405, for additional information. (c)" & "<br><br>"
    '            ElseIf HMAonRigidCase = 4 Then
    '                S = S & "<br>Overlay thickness was determined using a flexible pavement design procedure," & "<br>"
    '                S = S & "considering the existing PCC as a high-stiffness base layer." & "<br>"
    '                S = S & "Minimum HMA thickness was reached. Therefore, CDF totals may be" & "<br>"
    '                S = S & "less than 1.0. CDF is determined for the top of the subgrade." & "<br>"
    '                S = S & "Refer to AC 150/5320-6, paragraph 405, for additional information. (d)" & "<br><br>"
    '            End If
    '        End If

    '        If DesignType = NewRigid Or DesignType = NewFlex Then 'CreateHTMLInfo
    '            If Designed <> NullDate And CompactionDesigned(ISect, IJob) <> NullDate Then 'CreateHTMLinfo
    '                S = S & "Compaction requirements for this section were computed on " &
    '                Format(CompactionDesigned(ISect, IJob), "mm/dd/yy") & " at " &
    '                Format(CompactionDesigned(ISect, IJob), "hh:mm:ss") & "." & "<br>"
    '            End If
    '        End If

    '        S = S & "<br>"

    '        S = S & "<b>" & "Pavement Structure Information by Layer, Top First</b>" & "<br><br>"

    '        Info = Info & S


    '        Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '        "  <tr>" +
    '            "<td  align=""center"">No. </td>" +
    '            "<td  align=""center"">Type </td>" +
    '            "<td  align=""center"">Thickness <br>" & UnitsOut.inchName & "</td>" +
    '            "<td  align=""center"">Modulus <br>" & UnitsOut.psiMPaName & "</td>" +
    '            "<td  align=""center"">Poisson's <br>Ratio</td>" +
    '            "<td  align=""center"">Strength<br>R," & UnitsOut.psiMPaName & "</td>" +
    '            "</tr>"


    '        For I = 0 To NLayerTypes - 1
    '            If Len(LayerTypePic(I)) > LenLayerType Then
    '                LenLayerType = Len(LayerTypePic(I))
    '            End If
    '        Next I

    '        ThicktoSubgrade = 0.0!

    '        structureinfo.length = NPLayers
    '        'PaveStr.length = NPLayers

    '        For I = 1 To NPLayers
    '            structureinfo.no(I - 1) = I.ToString
    '            If I < NPLayers Then ThicktoSubgrade = ThicktoSubgrade + Thick(I)
    '            LI = LCode(I)
    '            S = LayerType(LI)
    '            If S = NPCC Or S = NPCCOU Or S = NPCCOB Or S = NPCCOF Then
    '                S = LayerTypePic(LI) '"PCC"
    '            Else
    '                S = LayerTypePic(LI)
    '            End If

    '            structureinfo.type(I - 1) = S

    '            'struct_layertype = S
    '            'Info = Info & LPad(3, Format(I, "0")) & New String(" ", 2) & TabStr
    '            Info = Info & "<tr>" &
    '            "<td  align=""center"">" & I.ToString & " </td>" &
    '            "<td  align=""center"">" & S & "</td>" &
    '            "<td  align=""center"">" & Format(Thick(I) * UnitsOut.inch, UnitsOut.inchFormat) & "</td>" &
    '            "<td  align=""center"">" & Format(Modulus(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat) & "</td>"

    '            structureinfo.thickness(I - 1) = Format(Thick(I) * UnitsOut.inch, UnitsOut.inchFormat).ToString
    '            structureinfo.modulus(I - 1) = Format(Modulus(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat).ToString

    '            Temp = DefaultPoisson(LI)
    '            If I = NPLayers Then
    '                If RCon(1) > 0.0! Or RCon(2) > 0.0! Then
    '                    Temp = DefaultPoissonSGPCC
    '                Else
    '                    Temp = DefaultPoissonSGAC
    '                End If
    '                If LayerType(LI) = NND Then Temp = DefaultPoisson(LI)
    '            End If


    '            If LayerType(LI) = NND Then
    '                Temp = jobPoissonsRatio(ISect, I)
    '            End If
    '            structureinfo.poissonRatio(I - 1) = Temp.ToString("0.00")

    '            structureinfo.streagth(I - 1) = Format(RCon(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat).ToString

    '            Info = Info &
    '            "<td  align=""center"">" & Format(Temp, "0.00") & "</td>" &
    '            "<td  align=""center"">" & Format(RCon(I) * UnitsOut.psiMPa, UnitsOut.psiMPaFormat) & "</td>" &
    '            "</tr>"
    '        Next I

    '        Info = Info & "</table>"

    '        TotalThickness = Format(ThicktoSubgrade * UnitsOut.inch, UnitsOut.inchFormat).ToString


    '        S = "Total thickness to the top of the subgrade = "
    '        S = S & Format(ThicktoSubgrade * UnitsOut.inch, UnitsOut.inchFormat) & " " & UnitsOut.inchName
    '        Info = Info & "<br><b>" & S & "</b><br><br><br>"
    '        PInfo = PInfo & S

    '        Info = Info & "<b>Airplane Information </b><br><br>"
    '        Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '        "<tr>" +
    '            "<td  align=""center"">No. </td>" +
    '            "<td  align=""center"">Name</td>" +
    '            "<td  align=""center"">Gross Wt.<br>" & UnitsOut.poundsName & "</td>" +
    '            "<td  align=""center"">Annual<br>Departures</td>" +
    '            "<td  align=""center"">% Annual<br>Growth</td>" +
    '        "</tr>"



    '        For I = 1 To NAC
    '            'Info = Info & LPad(3, Format(I, "0")) & New String(" ", 3) & TabStr
    '            Info = Info & "<tr>" &
    '            "<td  align=""center"">" & Format(I, "0") & " </td>" +
    '            "<td  align=""center"">" & ACName(I) & "</td>" +
    '            "<td  align=""center"">" & Format(GL(I) * UnitsOut.pounds, UnitsOut.poundsFormat) & "</td>" &
    '            "<td  align=""center"">" & Format(RepsAnnual(I), "#,##0") & "</td>" &
    '            "<td  align=""center"">" & Format(RepsInc(I) * 100.0!, "0.00") & "</td>" &
    '            "</tr>"
    '        Next I


    '        Info = Info & "</table>"




    '        If ComputeAircraftCDF Then ' GFH 08/14/03.

    '            Info = Info & "<br><br><b>Additional Airplane Information</b><br><br>"

    '            If (DesignType = NewFlex) Or (DesignType = FlexOnFlex) Then
    '                'Info = Info & "<p class=""small"">Subgrade CDF</p>"
    '                Info = Info & "Subgrade CDF"
    '                'Info = Info & "<font size=""3""><br><br></font>"
    '            Else

    '            End If

    '            Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '            "<tr>" +
    '                "<td  align=""center"">No. </td>" +
    '                "<td  align=""center"">Name</td>" +
    '                "<td  align=""center"">CDF<br>Contribution</td>" +
    '                "<td  align=""center"">CDF Max<br>for Airplane</td>" +
    '                "<td  align=""center"">P/C<br>Ratio</td>" +
    '            "</tr>"


    '            For I = 1 To NAC
    '                Info = Info & "<tr>" &
    '                "<td  align=""center"">" & Format(I, "0") & " </td>" +
    '                "<td  align=""center"">" & ACName(I) & "</td>" +
    '                "<td  align=""center"">" & Format(jobCDFtable(ISect, I), "#,###,##0.00") & "</td>" &
    '                "<td  align=""center"">" & Format(jobCDFacrftMaxtable(ISect, I), "#,###,##0.00") & "</td>"


    '                DTemp = Math.Abs(jobCtoPtable(ISect, I))
    '                If DTemp <> 0 Then DTemp = 1 / DTemp
    '                If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
    '                Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
    '                "</tr>"

    '            Next I


    '            Info = Info & "</table>"


    '            'New HMA - CDF
    '            If (Not NoACCDF) And (DesignType = NewFlex) Then 'for HMA CDF
    '                Info = Info & "<br>HMA CDF"
    '                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '                "<tr>" +
    '                    "<td  align=""center"">No. </td>" +
    '                    "<td  align=""center"">Name</td>" +
    '                    "<td  align=""center"">CDF<br>Contribution</td>" +
    '                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
    '                    "<td  align=""center"">P/C<br>Ratio</td>" +
    '                "</tr>"


    '                For I = 1 To NAC
    '                    Info = Info & "<tr>" &
    '                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
    '                    "<td  align=""center"">" & ACName(I) & "</td>" +
    '                    "<td  align=""center"">" & Format(jobCDFtableHMA(ISect, I), "#,###,##0.00") & "</td>" &
    '                    "<td  align=""center"">" & Format(jobCDFacrftMaxtableHMA(ISect, I), "#,###,##0.00") & "</td>"


    '                    DTemp = Math.Abs(jobCtoPtableHMA(ISect, I))
    '                    If DTemp <> 0 Then DTemp = 1 / DTemp
    '                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
    '                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
    '                    "</tr>"

    '                Next I

    '                Info = Info & "</table>"

    '            End If


    '            If (Not NoACCDF) And (DesignType = NewFlex And LCode(2) = 14) Then
    '                Info = Info & "<br>P-401/P-403 St (flex) CDF"
    '                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '                "<tr>" +
    '                    "<td  align=""center"">No. </td>" +
    '                    "<td  align=""center"">Name</td>" +
    '                    "<td  align=""center"">CDF<br>Contribution</td>" +
    '                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
    '                    "<td  align=""center"">P/C<br>Ratio</td>" +
    '                "</tr>"


    '                For I = 1 To NAC
    '                    Info = Info & "<tr>" &
    '                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
    '                    "<td  align=""center"">" & ACName(I) & "</td>" +
    '                    "<td  align=""center"">" & Format(jobCDFtable401(ISect, I), "#,###,##0.00") & "</td>" &
    '                    "<td  align=""center"">" & Format(jobCDFacrftMaxtable401(ISect, I), "#,###,##0.00") & "</td>"


    '                    DTemp = Math.Abs(jobCtoPtable401(ISect, I))
    '                    If DTemp <> 0 Then DTemp = 1 / DTemp
    '                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
    '                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
    '                    "</tr>"

    '                Next I

    '                Info = Info & "</table>"

    '            End If


    '            'HMA Overlay over HMA - CDF
    '            If (Not NoACCDF) And (DesignType = FlexOnFlex) Then
    '                Info = Info & "<br>Overlay HMA CDF"
    '                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '                "<tr>" +
    '                    "<td  align=""center"">No. </td>" +
    '                    "<td  align=""center"">Name</td>" +
    '                    "<td  align=""center"">CDF<br>Contribution</td>" +
    '                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
    '                    "<td  align=""center"">P/C<br>Ratio</td>" +
    '                "</tr>"


    '                For I = 1 To NAC
    '                    Info = Info & "<tr>" &
    '                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
    '                    "<td  align=""center"">" & ACName(I) & "</td>" +
    '                    "<td  align=""center"">" & Format(jobCDFtableAC(ISect, I), "#,###,##0.00") & "</td>" &
    '                    "<td  align=""center"">" & Format(jobCDFacrftMaxtableAC(ISect, I), "#,###,##0.00") & "</td>"


    '                    DTemp = Math.Abs(jobCtoPtableAC(ISect, I))
    '                    If DTemp <> 0 Then DTemp = 1 / DTemp
    '                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
    '                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
    '                    "</tr>"

    '                Next I

    '                Info = Info & "</table>"

    '                'HMA under HMA overlay
    '                Info = Info & "<br>HMA CDF"
    '                Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '                "<tr>" +
    '                    "<td  align=""center"">No. </td>" +
    '                    "<td  align=""center"">Name</td>" +
    '                    "<td  align=""center"">CDF<br>Contribution</td>" +
    '                    "<td  align=""center"">CDF Max<br>for Airplane</td>" +
    '                    "<td  align=""center"">P/C<br>Ratio</td>" +
    '                "</tr>"

    '                For I = 1 To NAC
    '                    Info = Info & "<tr>" &
    '                    "<td  align=""center"">" & Format(I, "0") & " </td>" +
    '                    "<td  align=""center"">" & ACName(I) & "</td>" +
    '                    "<td  align=""center"">" & Format(jobCDFtableHMA(ISect, I), "#,###,##0.00") & "</td>" &
    '                    "<td  align=""center"">" & Format(jobCDFacrftMaxtableHMA(ISect, I), "#,###,##0.00") & "</td>"


    '                    DTemp = Math.Abs(jobCtoPtableHMA(ISect, I))
    '                    If DTemp <> 0 Then DTemp = 1 / DTemp
    '                    If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
    '                    Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
    '                    "</tr>"

    '                Next I

    '                Info = Info & "</table>"

    '                If LCode(3) = 14 Then
    '                    Info = Info & "<br>P-401/P-403 St (flex) CDF"
    '                    Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '                    "<tr>" +
    '                        "<td  align=""center"">No. </td>" +
    '                        "<td  align=""center"">Name</td>" +
    '                        "<td  align=""center"">CDF<br>Contribution</td>" +
    '                        "<td  align=""center"">CDF Max<br>for Airplane</td>" +
    '                        "<td  align=""center"">P/C<br>Ratio</td>" +
    '                    "</tr>"

    '                    For I = 1 To NAC
    '                        Info = Info & "<tr>" &
    '                        "<td  align=""center"">" & Format(I, "0") & " </td>" +
    '                        "<td  align=""center"">" & ACName(I) & "</td>" +
    '                        "<td  align=""center"">" & Format(jobCDFtable401(ISect, I), "#,###,##0.00") & "</td>" &
    '                        "<td  align=""center"">" & Format(jobCDFacrftMaxtable401(ISect, I), "#,###,##0.00") & "</td>"


    '                        DTemp = Math.Abs(jobCtoPtable401(ISect, I))
    '                        If DTemp <> 0 Then DTemp = 1 / DTemp
    '                        If DTemp > 100 Then S$ = ">100 " Else S$ = Format(DTemp, "0.00")
    '                        Info = Info & "<td  align=""center"">" & LPad$(9, S$) & "</td>" &
    '                        "</tr>"

    '                    Next I

    '                    Info = Info & "</table>"
    '                End If

    '            End If

    '            Info = Info & "</body></html>"
    '            'Info = Info & "</table></body></html>"

    '        End If




    '        If DesignType = NewRigid Or DesignType = NewFlex Then 'CreateHTMLinfo
    '        Else
    '            GoTo PassCompaction
    '        End If


    '        'Added compaction capability to FF1.4 based on FF1.313 082012 by YGC 021913
    '        'Added to output COmpaction Criteria in Notes by YGC 042312
    '        If Designed <> NullDate And CompactionDesigned(ISect, IJob) <> NullDate Then 'CreateHTMLinfo
    '            Info = Info & "<br><br><b>Subgrade Compaction Requirements</b><br><br>"

    '            Info = Info & "NonCohesive Soil"

    '            'implementing metric
    '            Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '                            "<tr>" +
    '                            "<td  align=""center"">Percent Maximum Dry Density(%) </td>" +
    '                            "<td  align=""center"">Depth of compaction <br> from pavement surface (" & UnitsOut.inchName & ") </td>" +
    '                            "<td  align=""center"">Depth of compaction <br> from top of subgrade (" & UnitsOut.inchName & ") </td>" +
    '                            "<td  align=""center"">Critical Airplane for Compaction </td>" +
    '                            "</tr>"



    '            'modify ended by YGC 102213 

    '            For J = 1 To NDenLevel
    '                If jobCompactionIntDenNCtable(ISect, IJob, J) >= DensityNCMin Then


    '                    'metrication
    '                    Info = Info & "<tr>" &
    '                        "<td  align=""center"">" & Format(jobCompactionIntDenNCtable(ISect, IJob, J), "0") & " </td>" +
    '                        "<td  align=""center"">" & Format(jobCompactionIntDenDepthNCtable(ISect, IJob, J - 1) * UnitsOut.inch, "0") & " - " & Format(jobCompactionIntDenDepthNCtable(ISect, IJob, J) * UnitsOut.inch, "0") & "</td>"
    '                    If jobCompactionIntDenDepthNCtable(ISect, IJob, J) <= ThicktoSubgrade Then
    '                        Info = Info & "<td  align=""center"">" & "--" & " </td>"
    '                    Else
    '                        Info = Info & "<td  align=""center"">" & Format(Math.Max(jobCompactionIntDenDepthNCtable(ISect, IJob, J - 1) - ThicktoSubgrade, 0) * UnitsOut.inch, "0") & " - " & Format((jobCompactionIntDenDepthNCtable(ISect, IJob, J) - ThicktoSubgrade) * UnitsOut.inch, "0") & "</td>"
    '                    End If
    '                    'Info = Info & "<td  align=""center"">" & CallAC(jobCompactionIntDenCriticalACNCtable(ISect, J)).ACname & "</td>"
    '                    Info = Info & "<td  align=""center"">" & ACName(jobCompactionIntDenCriticalACNCtable(ISect, IJob, J)) & "</td>"



    '                End If
    '            Next J

    '            Info = Info & "</table>"

    '            Info = Info & "<br>"

    '            Info = Info & "Cohesive Soil"


    '            'implementing metric
    '            Info = Info & "<table border=""1"" width=""100%"" bordercolorlight=""#C0C0C0"" bordercolordark=""#FFFFFF"" cellspacing=""0"" cellpadding=""0"" style=""font-size: 8pt; font-family: Arial"">" &
    '                        "<tr>" +
    '                        "<td  align=""center"">Percent Maximum Dry Density(%) </td>" +
    '                        "<td  align=""center"">Depth of compaction <br> from pavement surface (" & UnitsOut.inchName & ") </td>" +
    '                        "<td  align=""center"">Depth of compaction <br> from top of subgrade (" & UnitsOut.inchName & ") </td>" +
    '                        "<td  align=""center"">Critical Airplane for Compaction </td>" +
    '                        "</tr>"
    '            'modify ended by YGC 102213 

    '            For J = 1 To NDenLevel
    '                If jobCompactionIntDenCtable(ISect, IJob, J) >= DensityCMin Then

    '                    'metrication    * UnitsOut.inch
    '                    Info = Info & "<tr>" &
    '                        "<td  align=""center"">" & Format(jobCompactionIntDenCtable(ISect, IJob, J), "0") & " </td>" +
    '                        "<td  align=""center"">" & Format(jobCompactionIntDenDepthCtable(ISect, IJob, J - 1) * UnitsOut.inch, "0") & " - " & Format(jobCompactionIntDenDepthCtable(ISect, IJob, J) * UnitsOut.inch, "0") & "</td>"
    '                    If jobCompactionIntDenDepthCtable(ISect, IJob, J) <= ThicktoSubgrade Then
    '                        Info = Info & "<td  align=""center"">" & "--" & " </td>"
    '                    Else
    '                        Info = Info & "<td  align=""center"">" & Format(Math.Max(jobCompactionIntDenDepthCtable(ISect, IJob, J - 1) - ThicktoSubgrade, 0) * UnitsOut.inch, "0") & " - " & Format((jobCompactionIntDenDepthCtable(ISect, IJob, J) - ThicktoSubgrade) * UnitsOut.inch, "0") & "</td>"
    '                    End If
    '                    'Info = Info & "<td  align=""center"">" & CallAC(jobCompactionIntDenCriticalACCtable(ISect, J)).ACname & "</td>"
    '                    Info = Info & "<td  align=""center"">" & ACName(jobCompactionIntDenCriticalACCtable(ISect, IJob, J)) & "</td>"



    '                End If
    '            Next J

    '            Info = Info & "</table>"

    '            'added for NOTES by YGC 102213 
    '            Info = Info & "<br><b>Subgrade Compaction Notes:</b><br>"
    '            Info = Info & "1.	Noncohesive  soils, for the purpose of determining compaction control, are those with a plasticity index (PI) less than 3.<br>"
    '            Info = Info & "2.	Tabulated values indicate depth ranges within which densities should equal or exceed the indicated percentage of the maximum dry density as specified in item P-152.<br>"
    '            If CallAC(IDHeaviestAC).GearLoad >= 60000 Then
    '                Info = Info & "3.	Maximum dry density is determined using ASTM Method D 1557.<br>"
    '            Else
    '                Info = Info & "3.	Maximum dry density is determined using ASTM Method D 698.<br>"
    '            End If
    '            Info = Info & "4.	The subgrade in cut areas should have natural densities shown or should (a) be compacted from the surface to achieve the required densities, (b) be removed and replaced at the densities shown, or (c) when economics and grades permit, be covered with sufficient select or subbase material so that the uncompacted subgrade is at a depth where the in-place densities are satisfactory.<br>"
    '            'Info = Info & "5.	For swelling soils refer to AC 150/5320-6E paragraph 313.<br>"
    '            Info = Info & "5.	For swelling soils refer to AC 150/5320-6F paragraph 3.10.<br>"
    '            'add ended for NOTES by YGC 102213 

    '        End If


    'PassCompaction:


    '        Info = Info & "<br><b>User is responsible for checking frost protection requirements.</b><br>"

    '        If IncludeImage Then
    '            Dim s111 As String
    '            s111 = MyDocumentDir & "\Structure.jpg"
    '            Directory.SetCurrentDirectory(MyDocumentDir)
    '            System.Windows.Forms.Application.DoEvents()
    '            Info = Info & "<br><br><br>"
    '            'Info = Info & "<img src=Structure.jpg><br>"
    '            Info = Info & "<p><img src=Structure.jpg align=""middle"" width=""357"" height=""273""></p><br>"


    '            'Info = Info & "<p><img src=s111 align=""middle"" width=""357"" height=""273""></p><br>"

    '            'Info = Info & "<p><img src=Structure.jpg align=""middle"" width=""286"" height=""218""></p><br>"
    '            'Info = Info & "<p><img src=Structure.jpg align=""middle"" width=""238"" height=""182""></p><br>"
    '            'http://www.w3schools.com/tags/att_img_align.asp
    '        End If

    '        '======================================================================================================



    '    End Sub


    '    Public Sub CreateHTML2(ByRef Info As String)
    '        'http://stackoverflow.com/questions/24983498/save-image-from-picturebox-vb
    '        'http://www.w3schools.com/html/html_images.asp
    '        'http://www.w3schools.com/tags/tag_img.asp

    '        htmlText = "<html>" +
    '           "<body>" +
    '           "<h2>Spectacular Mountains</h2><br>" +
    '           "<img src=Structure.jpg width=""238"" height=""182"">" +
    '           "</body>" +
    '           "</html>"

    '        '"<img src=MyStructure111.jpg alt=""Mountain View"" style=""width:238px;height:182px"">" + _
    '        'good
    '        '"<img src=MyStructure111.jpg alt=""Mountain View"" style=""width:476px;height:364px"">" + _

    '        '"<img src=""pic_mountain.jpg"" alt=""Mountain View"" style=""width:304px;height:228px"">" + _
    '        '"<img src=gPictureStructure.image alt=""Mountain View"" style=""width:304px;height:228px"">" + _

    '        Info = htmlText

    '    End Sub


    '    Public Sub CreatePDFAuto()


    '        'Try 'added kawa 2015

    '        '    Dim pdfFile As String
    '        '    Dim sav As New SaveFileDialog()
    '        '    'sav.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString
    '        '    'sav.InitialDirectory = WorkingDir
    '        '    sav.InitialDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) & "\FAARFIELD\"


    '        '    sav.Filter = "PDF Files (*.pdf)|*.pdf"
    '        '    sav.RestoreDirectory = True

    '        '    sav.FileName = JobName & "_" & SectName
    '        '    pdfFile = sav.FileName & ".pdf"


    '        '    'If sav.ShowDialog = Windows.Forms.DialogResult.OK Then
    '        '    '    pdfFile = sav.FileName
    '        '    '    If InStr(pdfFile, ".pdf") = 0 Then
    '        '    '        pdfFile = pdfFile.Substring(0, pdfFile.Length - 4) & ".pdf"

    '        '    '        If File.Exists(pdfFile) Then
    '        '    '            Dim ret As Integer, M1 As String
    '        '    '            M1 = "File " & pdfFile & " already exists. " & NL & "Do you want to replace it?"

    '        '    '            ret = MsgBox(M1, MsgBoxStyle.YesNo, "Save As")
    '        '    '            '7 for No, 6 for Yes
    '        '    '            If ret = 7 Then
    '        '    '                Exit Sub
    '        '    '            End If
    '        '    '        End If

    '        '    '    End If
    '        '    'Else
    '        '    '    Exit Sub
    '        '    'End If

    '        '    'http://www.w3schools.com/html/html_paragraphs.asp

    '        '    Dim document As New Document
    '        '    'PdfWriter.GetInstance(document, New FileStream("C:\ZZZZ\InfoPDF.pdf", FileMode.Create))
    '        '    PdfWriter.GetInstance(document, New FileStream(WorkingDir & "\" & pdfFile, FileMode.Create))
    '        '    document.Open()
    '        '    Dim hw As iTextSharp.text.html.simpleparser.HTMLWorker = New iTextSharp.text.html.simpleparser.HTMLWorker(document)

    '        '    hw.Parse(New StringReader(htmlText))
    '        '    document.Close()


    '        'Catch ex As Exception

    '        '    Dim abc As Boolean
    '        '    abc = InStr(1, ex.Message, "The process cannot access the file", CompareMethod.Text) > 0

    '        '    If abc Then

    '        '        Dim txt As String
    '        '        'txt = ex.Message
    '        '        txt = "A PDF design report is open and is preventing" + NL
    '        '        txt = txt + "completion of the job." + NL2
    '        '        txt = txt + "Please close the report to continue."

    '        '        MsgBox(txt)

    '        '    Else

    '        '        Dim txt As String
    '        '        txt = ex.Message
    '        '        txt = txt + Environment.NewLine + Environment.NewLine
    '        '        txt = txt + ex.StackTrace
    '        '        txt = txt + Environment.NewLine + Environment.NewLine
    '        '        MsgBox(txt)

    '        '    End If



    '        'End Try



    '    End Sub

    '    Public Sub CreatePDF()


    '        'Dim pdfFile As String
    '        'Dim sav As New SaveFileDialog()
    '        ''sav.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString
    '        'sav.InitialDirectory = WorkingDir
    '        'sav.Filter = "PDF Files (*.pdf)|*.pdf"
    '        'sav.RestoreDirectory = True

    '        'sav.FileName = JobName & "_" & SectName

    '        'If sav.ShowDialog = Windows.Forms.DialogResult.OK Then
    '        '    pdfFile = sav.FileName
    '        '    If InStr(pdfFile, ".pdf") = 0 Then
    '        '        pdfFile = pdfFile.Substring(0, pdfFile.Length - 4) & ".pdf"

    '        '        If File.Exists(pdfFile) Then
    '        '            Dim ret As Integer, M1 As String
    '        '            M1 = "File " & pdfFile & " already exists. " & NL & "Do you want to replace it?"

    '        '            ret = MsgBox(M1, MsgBoxStyle.YesNo, "Save As")
    '        '            '7 for No, 6 for Yes
    '        '            If ret = 7 Then
    '        '                Exit Sub
    '        '            End If
    '        '        End If

    '        '    End If
    '        'Else
    '        '    Exit Sub
    '        'End If




    '        ''http://www.w3schools.com/html/html_paragraphs.asp

    '        'Dim document As New Document
    '        ''PdfWriter.GetInstance(document, New FileStream("C:\ZZZZ\InfoPDF.pdf", FileMode.Create))
    '        'PdfWriter.GetInstance(document, New FileStream(pdfFile, FileMode.Create))
    '        'document.Open()
    '        'Dim hw As iTextSharp.text.html.simpleparser.HTMLWorker = New iTextSharp.text.html.simpleparser.HTMLWorker(document)

    '        'hw.Parse(New StringReader(htmlText))
    '        'document.Close()


    '    End Sub




    '    Public Function FindLife(ByVal LifePTraff As Single,
    '                         ByVal LifeEstimated As Single,
    '                         ByVal CDFMAX As Single,
    '                         ByRef Overflow As Boolean,
    '                         ByRef StressResponse(,) As Double,
    '                         ByVal RCONval As Single,
    '                         ByVal SCIval As Single) As Single

    '        FindLife = FindLife2018(LifePTraff, LifeEstimated, CDFMAX, Overflow, StressResponse, RCONval, SCIval)
    '        Exit Function

    '        Dim I As Short, CDFM1, DELT, TempMax As Single
    '        Dim LifeM1, PcntCDFUTemp, OldReps(NAC) As Single
    '        Dim CDFtableTemp(MaxSectAC), CDFacrftMaxtableTemp(MaxSectAC) As Double
    '        Dim InfiLife As Boolean = False
    '        Dim bBisection As Boolean = False '2018.06.25
    '        Dim LL11, LL22 As Single
    '        Dim CD11, CD22 As Single


    '        LifeM1 = Life
    '        CDFPic = CDFMAX : CDFM1 = CDFMAX
    '        PcntCDFUTemp = PcntCDFU
    '        For I = 1 To NAC : OldReps(I) = Reps(I) : Next I

    '        ''===========================================
    '        'FileOpen(9, "UUUUUUUUUUU.txt", OpenMode.Append)
    '        'Print(9, LPad(34, "LifeStr"))
    '        'Print(9, LPad(34, "CDF"))
    '        'PrintLine(9, "")
    '        'FileClose(9)
    '        ''===========================================


    '        LifeStr = LifeEstimated

    '        '        '===========================================
    '        '        LifeStr = 0.0001
    '        'start2:

    '        '        If LifeStr < 10000000 Then
    '        '            LifeStr = LifeStr * 10
    '        '        Else
    '        '            LifeStr = LifeStr * 1.2!
    '        '        End If
    '        '        '===========================================

    '        Do
    '            For I = 1 To NAC
    '                Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
    '                If Temp1 > TempMax And RepsInc(I) < 0 Then
    '                    InfiLife = True : Temp1 = TempMax
    '                Else
    '                    InfiLife = False
    '                End If
    '                Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
    '                Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
    '                Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
    '            Next I

    '            Overflow = True
    '            'Call LeafCDFRigid_NP(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)
    '            Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)

    '            ''===========================================
    '            'FileOpen(9, "UUUUUUUUUUU.txt", OpenMode.Append)

    '            'If LifeStr < 1 Then
    '            '    Print(9, LPad(35, Format(LifeStr, "#,##0.00000000000000000")))
    '            'Else
    '            '    Print(9, LPad(35, Format(LifeStr, "#,##0.000")))
    '            'End If

    '            'If CDFMAX < 1 Then
    '            '    Print(9, LPad(35, Format(CDFSUBMAX, "#,##0.0000000000000000")))
    '            'Else
    '            '    Print(9, LPad(35, Format(CDFSUBMAX, "#,##0.00000")))
    '            'End If

    '            'PrintLine(9, "")
    '            'FileClose(9)
    '            'GoTo start2
    '            ''===========================================


    '            If Overflow And False Then
    '                S = "PCC stresses (or traffic) are too low" & vbCrLf
    '                S = S & "to accurately compute life."
    '                Ret = MsgBoxDQ(S, 0, "Computing Life (FindLife)")
    '                GoTo fin1
    '            ElseIf bBisection Then

    '                If CDFSUBMAX < 1 Then
    '                    LL11 = LifeStr
    '                    CD11 = CDFSUBMAX
    '                Else
    '                    LL22 = LifeStr
    '                    CD22 = CDFSUBMAX
    '                End If

    '                LifeStr = (LL11 + LL22) / 2
    '                CDFM1 = CDFSUBMAX

    '            ElseIf InfiLife And CDFSUBMAX < 1.0 Then
    '                FindLife = 10000000 : LifeStr = FindLife : GoTo fin1
    '            ElseIf CDFSUBMAX > 1 And LifeStr = 0.01! Then
    '                FindLife = 0.01 : LifeStr = FindLife : GoTo fin1
    '            ElseIf Math.Abs(1 - CDFSUBMAX) < 0.001 Then
    '                FindLife = LifeStr : LifeStr = FindLife : GoTo fin1
    '            ElseIf LifeM1 < 0.0001 Then
    '                LifeM1 = 0.0! : LifeStr = 0.0! : FindLife = LifeStr
    '                GoTo fin1
    '            ElseIf ((CDFSUBMAX < 0.0001) And (CDFM1 > 10000000)) Then
    '                bBisection = True

    '                CD22 = CDFM1
    '                CD11 = CDFSUBMAX

    '                LL22 = LL11
    '                LL11 = LifeStr
    '                LifeStr = (LL11 + LL22) / 2

    '            End If

    '            If bBisection Then


    '            Else
    '                DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFSUBMAX - CDFM1)

    '                LL11 = LifeStr
    '                Temp = LifeStr
    '                LifeStr = LifeM1 + DELT : If LifeStr <= 0 Then LifeStr = 0.01
    '                LifeM1 = Temp
    '                CDFM1 = CDFSUBMAX
    '            End If

    '        Loop Until System.Math.Abs(CDFM1 - 1.0!) < 0.001

    'fin1:
    '        For I = 1 To NAC : Reps(I) = OldReps(I) : Next I
    '        FindLife = LifeStr

    '    End Function


    '    Public Function FindLife2018(ByVal LifePTraff As Single,
    '                      ByVal LifeEstimated As Single,
    '                      ByVal CDFMAX As Single,
    '                      ByRef Overflow As Boolean,
    '                      ByRef StressResponse(,) As Double,
    '                      ByVal RCONval As Single,
    '                      ByVal SCIval As Single) As Single

    '        Dim I As Short, CDFM1, DELT, TempMax As Single
    '        Dim LifeM1, PcntCDFUTemp, OldReps(NAC) As Single
    '        Dim CDFtableTemp(MaxSectAC), CDFacrftMaxtableTemp(MaxSectAC) As Double
    '        Dim InfiLife As Boolean = False
    '        Dim LL11, LL22 As Single, CD11, CD22 As Single

    '        iC1 = 0 : iC2 = 0
    '        LifeM1 = Life
    '        CDFPic = CDFMAX : CDFM1 = CDFMAX
    '        PcntCDFUTemp = PcntCDFU
    '        For I = 1 To NAC : OldReps(I) = Reps(I) : Next I
    '        LifeStr = LifeEstimated

    '        Do
    '            For I = 1 To NAC
    '                Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
    '                If Temp1 > TempMax And RepsInc(I) < 0 Then
    '                    InfiLife = True : Temp1 = TempMax
    '                Else
    '                    InfiLife = False
    '                End If
    '                Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
    '                Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
    '                Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
    '            Next I

    '            Overflow = True
    '            Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)

    '            If Overflow And False Then
    '                S = "PCC stresses (or traffic) are too low" & vbCrLf
    '                S = S & "to accurately compute life."
    '                Ret = MsgBoxDQ(S, 0, "Computing Life (FindLife)")
    '                GoTo fin1
    '            ElseIf Math.Abs(1 - CDFSUBMAX) < 0.001 Then
    '                FindLife2018 = LifeStr : LifeStr = FindLife2018 : GoTo fin1
    '            ElseIf InfiLife And CDFSUBMAX < 1.0 Then
    '                FindLife2018 = 1.0E+38 : LifeStr = FindLife2018 : GoTo fin1
    '            ElseIf (LifeStr < 0.001!) Or (LifeStr > 1000.0) Then
    '                GoTo doBisection
    '            End If


    '            DELT = (1.0! - CDFM1) * (LifeStr - LifeM1) / (CDFSUBMAX - CDFM1)

    '            LL11 = LifeStr
    '            Temp = LifeStr
    '            LifeStr = LifeM1 + DELT : If LifeStr <= 0 Then LifeStr = 0.01
    '            LifeM1 = Temp
    '            CDFM1 = CDFSUBMAX
    '            iC1 = iC1 + 1

    '        Loop Until (System.Math.Abs(CDFM1 - 1.0!) < 0.001) Or iC1 > 30

    'fin1:
    '        For I = 1 To NAC : Reps(I) = OldReps(I) : Next I
    '        FindLife2018 = LifeStr
    '        Exit Function


    'doBisection:

    '        If CDFSUBMAX > 1 Then
    '            Do
    '                LL22 = LifeStr : CD22 = CDFSUBMAX
    '                LifeStr = LifeStr * 0.5!

    '                For I = 1 To NAC
    '                    Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
    '                    If Temp1 > TempMax And RepsInc(I) < 0 Then
    '                        InfiLife = True : Temp1 = TempMax
    '                    Else
    '                        InfiLife = False
    '                    End If
    '                    Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
    '                    Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
    '                    Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
    '                Next I

    '                Overflow = True
    '                Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)
    '                LL11 = LifeStr : CD11 = CDFSUBMAX
    '            Loop Until (System.Math.Abs(CDFSUBMAX - 1.0!) < 0.001) Or (CDFSUBMAX < 1)
    '        Else
    '            Do
    '                LL11 = LifeStr : CD11 = CDFSUBMAX
    '                LifeStr = LifeStr * 2.0!
    '                For I = 1 To NAC
    '                    Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
    '                    If Temp1 > TempMax And RepsInc(I) < 0 Then
    '                        InfiLife = True : Temp1 = TempMax
    '                    Else
    '                        InfiLife = False
    '                    End If
    '                    Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
    '                    Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
    '                    Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
    '                Next I

    '                Overflow = True
    '                Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)
    '                LL22 = LifeStr : CD22 = CDFSUBMAX
    '            Loop Until (System.Math.Abs(CDFSUBMAX - 1.0!) < 0.001) Or (CDFSUBMAX > 1)

    '        End If


    'repeat1:


    '        LifeStr = (LL11 + LL22) / 2

    '        For I = 1 To NAC
    '            Temp1 = LifeStr + LifePTraff : TempMax = -1.0! / RepsInc(I) / 2
    '            If Temp1 > TempMax And RepsInc(I) < 0 Then
    '                InfiLife = True : Temp1 = TempMax
    '            Else
    '                InfiLife = False
    '            End If
    '            Temp = CSng(1.0! + Temp1 * RepsInc(I) * 0.5)
    '            Temp2 = CSng(1.0! + LifePTraff * RepsInc(I) * 0.5)
    '            Reps(I) = Temp * RepsAnnual(I) * Temp1 - Temp2 * RepsAnnual(I) * LifePTraff
    '        Next I
    '        Overflow = True
    '        Call LeafCDFRigid_2014(CDFSUBMAX, Overflow, StressResponse, RCONval, SCIval)

    '        If CDFSUBMAX < 1 Then
    '            LL11 = LifeStr
    '            CD11 = CDFSUBMAX
    '        Else
    '            LL22 = LifeStr
    '            CD22 = CDFSUBMAX
    '        End If

    '        If (System.Math.Abs(CDFSUBMAX - 1.0!) < 0.001) Or (iC2 > 30) Then
    '            GoTo fin1
    '        Else
    '            iC2 = iC2 + 1
    '            GoTo repeat1
    '        End If
    '        GoTo fin1

    '    End Function


End Module
