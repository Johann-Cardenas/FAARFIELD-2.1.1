Imports System.Text
Imports FaarFieldAnalysis

Namespace Libs

    ''' <summary>
    ''' Generates a standalone HTML report with inline SVG charts, modern CSS,
    ''' and clickable table of contents. This is a parallel pipeline to the
    ''' existing GDI+ bitmap-based PDF report.
    ''' </summary>
    Public Class HtmlReportGenerator

        ' Chart color palette (hex equivalents of the GDI+ palette)
        Private Shared ReadOnly ChartColors() As String = {
            "#1F77B4", "#FF7F0E", "#2CA02C", "#D62728", "#9467BD",
            "#8C564B", "#E377C2", "#7F7F7F", "#BCBD22", "#17BECF"
        }

        Public Shared Function Generate(
            rpt As clsDetailedReportData,
            jobName As String,
            sectionName As String,
            analysisTypeName As String,
            appTitle As String,
            thicknessUnit As String,
            pressureUnit As String,
            weightUnit As String,
            lengthUnit As String
        ) As String

            Dim sb As New StringBuilder(65536)

            ' Subgrade modulus for equations
            Dim subgradeMod As Double = 15000
            If rpt.SublayerData.DesignLayers.Count > 0 Then
                subgradeMod = rpt.SublayerData.DesignLayers(rpt.SublayerData.DesignLayers.Count - 1).Modulus
            End If
            Dim computedAA As Double = 0.000247 + 0.000245 * Math.Log10(subgradeMod)
            Dim computedBB As Double = 0.0658 * subgradeMod ^ 0.559

            ' Count aircraft
            Dim nAcCount As Integer = 0
            If rpt.AircraftDetails IsNot Nothing Then
                For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                    If rpt.AircraftDetails(ia) IsNot Nothing Then nAcCount += 1
                Next
            End If

            ' ===== Document start =====
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html lang='en'>")
            sb.AppendLine("<head>")
            sb.AppendLine("<meta charset='UTF-8'>")
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>")
            sb.AppendLine("<title>FAARFIELD Detailed Computation Report — " & WebEncode(jobName) & "</title>")
            sb.AppendLine("<style>")
            sb.AppendLine(GetCss())
            sb.AppendLine("</style>")
            sb.AppendLine("</head>")
            sb.AppendLine("<body>")

            ' ===== Header =====
            sb.AppendLine("<header class='report-header'>")
            sb.AppendLine("<h1>FAA FAARFIELD Detailed Computation Report</h1>")
            sb.AppendLine("<p class='subtitle'>" & WebEncode(appTitle) & "</p>")
            sb.AppendLine("<div class='header-meta'>")
            sb.AppendLine("<div><strong>Job:</strong> " & WebEncode(jobName) & "</div>")
            sb.AppendLine("<div><strong>Structure:</strong> " & WebEncode(sectionName) & "</div>")
            If analysisTypeName IsNot Nothing Then
                sb.AppendLine("<div><strong>Analysis:</strong> " & WebEncode(analysisTypeName) & "</div>")
            End If
            sb.AppendLine("<div><strong>Generated:</strong> " & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "</div>")
            sb.AppendLine("</div>")
            sb.AppendLine("</header>")

            If Not rpt.IsPopulated Then
                sb.AppendLine("<div class='alert'>No detailed computation data available. Run a thickness design or life computation first.</div>")
                sb.AppendLine("</body></html>")
                Return sb.ToString()
            End If

            ' ===== Dashboard =====
            sb.AppendLine("<section class='dashboard'>")
            If rpt.CDFSweep.MaxCDF > 0 Then
                AppendCard(sb, "Max Total CDF", Format(rpt.CDFSweep.MaxCDF, "0.000000"), "")
            End If
            If rpt.Iterations.Count > 0 Then
                Dim lastThk = rpt.Iterations(rpt.Iterations.Count - 1).Thickness
                AppendCard(sb, "Design Thickness", Format(lastThk, "0.00"), thicknessUnit)
            End If
            AppendCard(sb, "Aircraft in Mix", nAcCount.ToString(), "")
            If rpt.CDFSweep.MaxCDFOffset > 0 Then
                AppendCard(sb, "Critical Offset", Format((rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC, "0"), lengthUnit)
            End If
            AppendCard(sb, "Subgrade Modulus", Format(subgradeMod, "#,##0"), pressureUnit)
            If rpt.Iterations.Count > 0 Then
                Dim lastIt = rpt.Iterations(rpt.Iterations.Count - 1)
                Dim convStr = If(lastIt.CDFErr < CDF.CDFExitErr, "Yes", "No")
                AppendCard(sb, "Converged / Iterations", convStr & " / " & rpt.Iterations.Count.ToString(), "")
            End If
            sb.AppendLine("</section>")

            ' ===== Table of Contents =====
            sb.AppendLine("<nav class='toc' id='toc'>")
            sb.AppendLine("<h2>Table of Contents</h2>")
            sb.AppendLine("<ol>")
            sb.AppendLine("<li><a href='#section-a'>Pavement Structure Summary</a></li>")
            sb.AppendLine("<li><a href='#section-b'>Design Equations</a></li>")
            sb.AppendLine("<li><a href='#section-c'>Understanding Coverage-to-Pass (C/P)</a></li>")
            sb.AppendLine("<li><a href='#section-d'>Fatigue Characterization</a></li>")
            sb.AppendLine("<li><a href='#section-e'>Per-Aircraft Detailed Breakdown</a></li>")
            sb.AppendLine("<li><a href='#section-f'>Coverage-to-Pass (C/P) Distribution</a></li>")
            sb.AppendLine("<li><a href='#section-g'>CDF Sweep Table (" & CDF.NOFF.ToString() & " offsets)</a></li>")
            sb.AppendLine("<li><a href='#section-h'>CDF Distribution Across Pavement Width</a></li>")
            sb.AppendLine("<li><a href='#section-i'>Newton-Raphson Convergence</a></li>")
            If rpt.ACRDetails.Count > 0 Then
                sb.AppendLine("<li><a href='#section-j'>ACR Details</a></li>")
            End If
            If rpt.PCRRounds.Count > 0 Then
                sb.AppendLine("<li><a href='#section-k'>PCR Elimination Rounds</a></li>")
            End If
            If rpt.ACRDetails.Count > 0 AndAlso rpt.AircraftDetails IsNot Nothing Then
                sb.AppendLine("<li><a href='#section-l'>ACR vs. Damage Per Departure</a></li>")
            End If
            sb.AppendLine("</ol>")
            sb.AppendLine("</nav>")

            ' ===== Section A: Pavement Structure =====
            sb.AppendLine("<section id='section-a'>")
            sb.AppendLine("<h2><span class='sec-num'>A</span> Pavement Structure Summary</h2>")
            AppendLayerTable(sb, "Design Layers", rpt.SublayerData.DesignLayers, thicknessUnit, pressureUnit)

            If rpt.SublayerData.ExpandedSublayers.Count > rpt.SublayerData.DesignLayers.Count Then
                sb.AppendLine("<h3>Expanded Sublayer Structure (after modulus adjustment)</h3>")
                AppendLayerTable(sb, "", rpt.SublayerData.ExpandedSublayers, thicknessUnit, pressureUnit)
            End If

            sb.AppendLine("<div class='callout info'>Evaluation Depth at Subgrade = " &
                Format(rpt.SublayerData.EvalDepthSubgrade, "0.00") & " " & thicknessUnit & "</div>")
            sb.AppendLine("</section>")

            ' ===== Section B: Design Equations =====
            sb.AppendLine("<section id='section-b'>")
            sb.AppendLine("<h2><span class='sec-num'>B</span> Design Equations</h2>")

            ' B.1 Subgrade failure model
            sb.AppendLine("<div class='equation-card'>")
            sb.AppendLine("<h4>Subgrade Strain Failure Model (FAA Standard)</h4>")
            sb.AppendLine("<div class='eq'>AA = 0.000247 + 0.000245 &times; log<sub>10</sub>(E<sub>subgrade</sub>)</div>")
            sb.AppendLine("<div class='eq'>BB = 0.0658 &times; E<sub>subgrade</sub><sup>0.559</sup></div>")
            sb.AppendLine("<div class='eq'>N<sub>fail</sub> = 10,000 &times; (AA / &epsilon;<sub>v</sub>)<sup>BB</sup></div>")
            sb.AppendLine("<div class='eq-note'>For this structure (E<sub>subgrade</sub> = " &
                Format(subgradeMod, "#,##0") & " " & pressureUnit & "): AA = " &
                Format(computedAA, "0.000000") & ", BB = " & Format(computedBB, "0.000") & "</div>")
            sb.AppendLine("</div>")

            ' B.2 CDF formula
            sb.AppendLine("<div class='equation-card'>")
            sb.AppendLine("<h4>Cumulative Damage Factor (CDF)</h4>")
            sb.AppendLine("<div class='eq'>CDF<sub>aircraft</sub> = Repetitions &times; (C/P) / N<sub>fail</sub></div>")
            sb.AppendLine("<div class='eq'>CDF<sub>total</sub> = &Sigma; CDF<sub>aircraft</sub> &nbsp;&nbsp;(summed over all aircraft)</div>")
            sb.AppendLine("<div class='eq-note'>where C/P = Coverage-to-Pass ratio from Gaussian lateral wander model (&sigma; = 30.435 in.)</div>")
            sb.AppendLine("</div>")

            ' B.3 C/P equation
            sb.AppendLine("<div class='equation-card'>")
            sb.AppendLine("<h4>Coverage-to-Pass (Gaussian Wander Model)</h4>")
            sb.AppendLine("<div class='eq'>C/P(offset) = &int; G(x; &sigma;) dx &nbsp;&nbsp;over tire contact width</div>")
            sb.AppendLine("<div class='eq'>G(x; &sigma;) = [1 / (&sigma;&radic;(2&pi;))] &times; exp(&minus;x&sup2; / 2&sigma;&sup2;)</div>")
            sb.AppendLine("<div class='eq-note'>&sigma; = 30.435 in. (std. dev. of lateral wander)<br/>Projected tire width at depth d: TW<sub>proj</sub> = TW + 2d</div>")
            sb.AppendLine("</div>")

            ' B.4 Convergence criterion
            sb.AppendLine("<div class='equation-card'>")
            sb.AppendLine("<h4>Convergence Criterion</h4>")
            sb.AppendLine("<div class='eq'>|ln(CDF<sub>total</sub>)| &lt; " & Format(CDF.CDFExitErr, "0.000") & " &nbsp;&nbsp;(exit criterion)</div>")
            sb.AppendLine("<div class='eq-note'>Design converges when 0.995 &lt; CDF<sub>total</sub> &lt; 1.005<br/>Sublayers activated when 0.5 &lt; CDF &lt; 2.0 (|ln(CDF)| &lt; " & Format(CDF.CDFErrCntrl, "0.000") & ")</div>")
            sb.AppendLine("</div>")
            sb.AppendLine("</section>")

            ' ===== Section C: C/P Concept =====
            sb.AppendLine("<section id='section-c'>")
            sb.AppendLine("<h2><span class='sec-num'>C</span> Understanding Coverage-to-Pass (C/P)</h2>")
            sb.AppendLine("<div class='callout info'>")
            sb.AppendLine("<p>The Coverage-to-Pass (C/P) ratio is the probability that a given evaluation " &
                "strip (10 in. wide) on the pavement surface will be loaded by a passing wheel, accounting for " &
                "Gaussian lateral wander of the aircraft about the nominal wheel path centerline. " &
                "The standard deviation of wander is &sigma; = 30.435 in. (corresponding to a wander width of 70 in.).</p>")
            sb.AppendLine("<p>For multi-wheel gear configurations, the total C/P at each strip is the superposition (sum) of the " &
                "individual GaussArea contributions from every wheel in the gear assembly, because the entire gear " &
                "wanders as a rigid body. FAARFIELD evaluates " & CDF.NOFF.ToString() & " strips on one side of the nominal " &
                "wheel path centerline (offsets 0 to " & Format((CDF.NOFF - 1) * CDF.OFFSETINC, "0") & " in.).</p>")
            sb.AppendLine("</div>")
            sb.AppendLine("</section>")

            ' ===== Section D: Fatigue Characterization =====
            If rpt.AircraftDetails IsNot Nothing Then
                sb.AppendLine("<section id='section-d'>")
                sb.AppendLine("<h2><span class='sec-num'>D</span> Fatigue Characterization</h2>")
                sb.AppendLine("<div class='callout info'><p>The following chart shows the subgrade fatigue model curve (allowable repetitions vs. vertical strain) " &
                    "with each aircraft's computed strain and N<sub>fail</sub> plotted as scatter points.</p></div>")

                ' SVG Fatigue Curve
                AppendFatigueCurveSVG(sb, rpt, subgradeMod)

                ' Fatigue parameters table
                sb.AppendLine("<h3>Aircraft Fatigue Parameters</h3>")
                sb.AppendLine("<table class='data-table'><thead><tr>")
                sb.AppendLine("<th>Aircraft</th><th>Vert. Strain (&mu;&epsilon;)</th><th>AA</th><th>BB</th><th>N<sub>fail</sub></th><th>Repetitions</th><th>N<sub>fail</sub>/Reps</th><th>Model</th>")
                sb.AppendLine("</tr></thead><tbody>")
                For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                    If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                    Dim det = rpt.AircraftDetails(ia)
                    Dim ratio As Double = If(det.TotalRepetitions > 0, det.NtoFail / det.TotalRepetitions, 0)
                    sb.AppendLine("<tr>")
                    sb.Append("<td>" & WebEncode(det.ACName) & "</td>")
                    sb.Append("<td>" & Format(det.VerticalStrain * 1000000, "0.00") & "</td>")
                    sb.Append("<td>" & Format(det.NtoFailAA, "0.000000") & "</td>")
                    sb.Append("<td>" & Format(det.NtoFailBB, "0.000") & "</td>")
                    sb.Append("<td>" & Format(det.NtoFail, "0.000E+00") & "</td>")
                    sb.Append("<td>" & Format(det.TotalRepetitions, "#,##0") & "</td>")
                    sb.Append("<td>" & Format(ratio, "0.00E+00") & "</td>")
                    sb.Append("<td>" & WebEncode(det.SubgradeModelUsed) & "</td>")
                    sb.AppendLine("</tr>")
                Next
                sb.AppendLine("</tbody></table>")

                ' SVG Life Ratio Chart
                AppendLifeRatioSVG(sb, rpt)

                sb.AppendLine("</section>")
            End If

            ' ===== Section E: Per-Aircraft Breakdown =====
            If rpt.AircraftDetails IsNot Nothing Then
                sb.AppendLine("<section id='section-e'>")
                sb.AppendLine("<h2><span class='sec-num'>E</span> Per-Aircraft Detailed Breakdown</h2>")

                For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                    If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                    Dim det = rpt.AircraftDetails(ia)

                    sb.AppendLine("<div class='aircraft-block'>")
                    sb.AppendLine("<h3>Aircraft " & ia.ToString() & ": " & WebEncode(det.ACName) & "</h3>")

                    ' Gear parameters table
                    sb.AppendLine("<table class='data-table param-table'><thead><tr><th>Parameter</th><th>Value</th><th>Description</th></tr></thead><tbody>")
                    AppendParamRow(sb, "Gear Type", det.GearType, "Landing gear configuration")
                    AppendParamRow(sb, "Gear Load", Format(det.GrossLoad, "#,##0") & " " & weightUnit, "Maximum gear load applied to pavement")
                    AppendParamRow(sb, "Tire Pressure", Format(det.TirePressure, "0.0") & " " & pressureUnit, "Inflation pressure")
                    AppendParamRow(sb, "Tire Width (TW)", Format(det.TireWidth, "0.00") & " " & lengthUnit, "Contact width at surface")
                    If det.TandemSpacing > 0 Then
                        AppendParamRow(sb, "Tandem Spacing", Format(det.TandemSpacing, "0.00") & " " & lengthUnit, "Distance between tandem wheels")
                    End If
                    AppendParamRow(sb, "Contact Area", Format(det.ContactArea, "0.00") & " " & lengthUnit & "&sup2;", "Static tire contact area")
                    AppendParamRow(sb, "Annual Departures", Format(det.AnnualDepartures, "#,##0"), "Annual departure level (ADL)")
                    AppendParamRow(sb, "Total Repetitions", Format(det.TotalRepetitions, "#,##0"), "Computed from ADL &times; 20-year design life")
                    AppendParamRow(sb, "# Gear Loads", det.NGearLoads.ToString(), "Main + belly gear load groups")
                    AppendParamRow(sb, "Projected TW at Subgrade", Format(det.ProjectedTireWidthAtSubgrade, "0.00") & " " & lengthUnit, "TW + 2&times;depth (45&deg; spread)")
                    AppendParamRow(sb, "Max Vertical Strain", Format(det.VerticalStrain * 1000000, "0.00") & " &mu;&epsilon;", "LEAF-computed subgrade strain")
                    If det.HorizontalStrain <> 0 Then
                        AppendParamRow(sb, "Horizontal Strain", Format(det.HorizontalStrain * 1000000, "0.00") & " &mu;&epsilon;", "LEAF-computed horizontal strain")
                    End If
                    AppendParamRow(sb, "N<sub>fail</sub>", Format(det.NtoFail, "0.000E+00"), "Allowable repetitions (" & WebEncode(det.SubgradeModelUsed) & ")")
                    AppendParamRow(sb, "Max C/P Ratio", Format(det.MaxCtoP, "0.00000"), "Peak coverage-to-pass ratio")
                    If det.GearAdjusted Then
                        AppendParamRow(sb, "C/P Before Gear Adj.", Format(det.CtoPBeforeGearAdj, "0.00000"), "Before multi-gear adjustment")
                        AppendParamRow(sb, "C/P After Gear Adj.", Format(det.CtoPAfterGearAdj, "0.00000"), "After multi-gear adjustment")
                    End If
                    AppendParamRow(sb, "Max CDF (this aircraft)", Format(det.MaxCDF, "0.000000"), "Peak damage contribution")
                    AppendParamRow(sb, "CDF at Critical Offset", Format(det.CDFAtCriticalOffset, "0.000000"), "Damage at critical strip")
                    sb.AppendLine("</tbody></table>")

                    ' Per-aircraft CDF by offset SVG
                    If rpt.CDFSweep.NAircraftCaptured > 0 Then
                        Dim acColor = ChartColors((ia - 1) Mod ChartColors.Length)
                        AppendSingleAircraftCDFSvg(sb, det, rpt.CDFSweep.MaxCDFOffset, acColor, lengthUnit)
                    End If

                    ' CDF by offset table (collapsible)
                    If rpt.CDFSweep.NAircraftCaptured > 0 Then
                        sb.AppendLine("<details><summary>CDF by Offset &mdash; " & WebEncode(det.ACName) & " (click to expand)</summary>")
                        sb.AppendLine("<table class='data-table compact'><thead><tr><th>Offset (" & lengthUnit & ")</th><th>C/P</th><th>CDF</th></tr></thead><tbody>")
                        For ioff As Integer = 1 To CDF.NOFF
                            Dim offsetVal = (ioff - 1) * CDF.OFFSETINC
                            Dim cls = If(ioff = rpt.CDFSweep.MaxCDFOffset, " class='highlight'", "")
                            sb.AppendLine("<tr" & cls & "><td>" & Format(offsetVal, "0") & "</td><td>" &
                                Format(det.CtoPByOffset(ioff), "0.00000") & "</td><td>" &
                                Format(det.CDFByOffset(ioff), "0.000000") & "</td></tr>")
                        Next
                        sb.AppendLine("</tbody></table></details>")
                    End If

                    ' Step-by-step walkthrough
                    sb.AppendLine("<div class='steps'>")
                    sb.AppendLine("<h4>Step-by-Step Computation</h4>")
                    sb.AppendLine("<ol>")
                    sb.AppendLine("<li>Apply gear load of " & Format(det.GrossLoad, "#,##0") & " " & weightUnit &
                        " with tire pressure " & Format(det.TirePressure, "0.0") & " " & pressureUnit &
                        " and tire contact width " & Format(det.TireWidth, "0.00") & " " & lengthUnit &
                        " (" & WebEncode(det.GearType) & " gear).</li>")
                    sb.AppendLine("<li>Run LEAF to compute vertical subgrade strain. Result: &epsilon;<sub>v</sub> = " &
                        Format(det.VerticalStrain * 1000000, "0.00") & " &mu;&epsilon; at evaluation depth " &
                        Format(rpt.SublayerData.EvalDepthSubgrade, "0.00") & " " & thicknessUnit & ".</li>")
                    sb.AppendLine("<li>Compute N<sub>fail</sub> using " & WebEncode(det.SubgradeModelUsed) &
                        " model: AA=" & Format(det.NtoFailAA, "0.000000") & ", BB=" & Format(det.NtoFailBB, "0.000") &
                        ". N<sub>fail</sub> = " & Format(det.NtoFail, "0.000E+00") & ".</li>")
                    sb.AppendLine("<li>Compute C/P at " & CDF.NOFF.ToString() & " strips. Peak C/P = " &
                        Format(det.MaxCtoP, "0.00000") & ".</li>")
                    sb.AppendLine("<li>Project tire width to subgrade (45&deg; spread): TW<sub>proj</sub> = " &
                        Format(det.ProjectedTireWidthAtSubgrade, "0.00") & " " & lengthUnit & ".</li>")
                    sb.AppendLine("<li>Total repetitions: " & Format(det.AnnualDepartures, "#,##0") &
                        " &times; 20 = " & Format(det.TotalRepetitions, "#,##0") & ".</li>")
                    sb.AppendLine("<li>CDF = Reps &times; C/P / N<sub>fail</sub>. Max CDF = " &
                        Format(det.MaxCDF, "0.000000") & ". CDF at critical strip = " &
                        Format(det.CDFAtCriticalOffset, "0.000000") & ".</li>")
                    If det.GearAdjusted Then
                        sb.AppendLine("<li>Multi-gear adjustment: C/P before = " &
                            Format(det.CtoPBeforeGearAdj, "0.00000") & ", after = " &
                            Format(det.CtoPAfterGearAdj, "0.00000") & ".</li>")
                    End If
                    sb.AppendLine("</ol></div>")

                    sb.AppendLine("</div>") ' aircraft-block
                Next ia
                sb.AppendLine("</section>")
            End If

            ' ===== Section F: C/P Distribution =====
            If rpt.CDFSweep.NAircraftCaptured > 0 AndAlso rpt.AircraftDetails IsNot Nothing Then
                sb.AppendLine("<section id='section-f'>")
                sb.AppendLine("<h2><span class='sec-num'>F</span> Coverage-to-Pass (C/P) Distribution</h2>")
                AppendCoveragePlotSVG(sb, rpt, lengthUnit)
                sb.AppendLine("</section>")
            End If

            ' ===== Section G: CDF Sweep Table =====
            If rpt.CDFSweep.NAircraftCaptured > 0 Then
                sb.AppendLine("<section id='section-g'>")
                sb.AppendLine("<h2><span class='sec-num'>G</span> CDF Sweep Table (" & CDF.NOFF.ToString() & " offsets)</h2>")

                sb.AppendLine("<div class='table-scroll'><table class='data-table compact'><thead><tr>")
                sb.Append("<th>Offset (" & lengthUnit & ")</th>")
                For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                    Dim acName = "AC" & ia.ToString()
                    If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                        acName = rpt.AircraftDetails(ia).ACName
                    End If
                    sb.Append("<th>" & WebEncode(acName) & " C/P</th><th>" & WebEncode(acName) & " CDF</th>")
                Next
                sb.AppendLine("<th>Total CDF</th></tr></thead><tbody>")

                For ioff As Integer = 1 To CDF.NOFF
                    Dim offsetVal = (ioff - 1) * CDF.OFFSETINC
                    Dim cls = If(ioff = rpt.CDFSweep.MaxCDFOffset, " class='highlight'", "")
                    sb.Append("<tr" & cls & "><td>" & Format(offsetVal, "0") & "</td>")
                    For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                        sb.Append("<td>" & Format(rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, ioff), "0.00000") & "</td>")
                        sb.Append("<td>" & Format(rpt.CDFSweep.CDFPerAircraftPerOffset(ia, ioff), "0.000000") & "</td>")
                    Next
                    sb.AppendLine("<td><strong>" & Format(rpt.CDFSweep.CDFTotalPerOffset(ioff), "0.000000") & "</strong></td></tr>")
                Next
                sb.AppendLine("</tbody></table></div>")

                sb.AppendLine("<div class='callout info'>Critical offset at #" & rpt.CDFSweep.MaxCDFOffset.ToString() &
                    " = " & Format((rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC, "0") & " " & lengthUnit &
                    ", Max CDF = " & Format(rpt.CDFSweep.MaxCDF, "0.000000") & "</div>")
                sb.AppendLine("</section>")
            End If

            ' ===== Section H: CDF Distribution =====
            If rpt.CDFSweep.NAircraftCaptured > 0 AndAlso rpt.AircraftDetails IsNot Nothing Then
                sb.AppendLine("<section id='section-h'>")
                sb.AppendLine("<h2><span class='sec-num'>H</span> CDF Distribution Across Pavement Width</h2>")

                AppendCompositeCDFSvg(sb, rpt, lengthUnit)

                ' CDF contribution bar chart
                AppendCDFContributionSVG(sb, rpt)

                ' CDF contribution summary table
                sb.AppendLine("<h3>CDF Contribution Summary at Critical Offset</h3>")
                Dim totalCritCDF As Double = rpt.CDFSweep.CDFTotalPerOffset(rpt.CDFSweep.MaxCDFOffset)
                sb.AppendLine("<table class='data-table'><thead><tr><th>Aircraft</th><th>CDF at Critical Offset</th><th>% of Total</th></tr></thead><tbody>")
                For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                    Dim acName = "AC" & ia.ToString()
                    If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                        acName = rpt.AircraftDetails(ia).ACName
                    End If
                    Dim acCDF As Double = rpt.CDFSweep.CDFPerAircraftPerOffset(ia, rpt.CDFSweep.MaxCDFOffset)
                    Dim pct As Double = If(totalCritCDF > 0, acCDF / totalCritCDF * 100, 0)
                    sb.AppendLine("<tr><td>" & WebEncode(acName) & "</td><td>" & Format(acCDF, "0.000000") & "</td><td>" & Format(pct, "0.0") & "%</td></tr>")
                Next
                sb.AppendLine("<tr class='highlight'><td><strong>Total</strong></td><td><strong>" &
                    Format(totalCritCDF, "0.000000") & "</strong></td><td><strong>100.0%</strong></td></tr>")
                sb.AppendLine("</tbody></table>")
                sb.AppendLine("</section>")
            End If

            ' ===== Section I: Newton-Raphson Convergence =====
            If rpt.Iterations.Count > 0 Then
                sb.AppendLine("<section id='section-i'>")
                sb.AppendLine("<h2><span class='sec-num'>I</span> Newton-Raphson Convergence</h2>")

                If rpt.Iterations.Count >= 2 Then
                    AppendConvergenceSVG(sb, rpt, thicknessUnit)
                End If

                ' Iteration log table
                sb.AppendLine("<h3>Iteration Log</h3>")
                sb.AppendLine("<table class='data-table compact'><thead><tr>")
                sb.AppendLine("<th>#</th><th>Thickness (" & thicknessUnit & ")</th><th>CDF<sub>sub</sub></th><th>|ln(CDF)|</th><th>&Delta;t (" & thicknessUnit & ")</th><th>Factor</th><th>Sublayered</th>")
                sb.AppendLine("</tr></thead><tbody>")
                For Each iter In rpt.Iterations
                    sb.AppendLine("<tr><td>" & iter.IterationNumber.ToString() & "</td>" &
                        "<td>" & Format(iter.Thickness, "0.00") & "</td>" &
                        "<td>" & Format(iter.CDFMAX, "0.000000") & "</td>" &
                        "<td>" & Format(iter.CDFErr, "0.00000") & "</td>" &
                        "<td>" & Format(iter.DELT, "0.00") & "</td>" &
                        "<td>" & Format(iter.Factor, "0.000") & "</td>" &
                        "<td>" & If(iter.SubLayered, "Yes", "No") & "</td></tr>")
                Next
                sb.AppendLine("</tbody></table>")

                ' Convergence Summary
                Dim lastIter = rpt.Iterations(rpt.Iterations.Count - 1)
                sb.AppendLine("<h3>Convergence Summary</h3>")
                sb.AppendLine("<table class='data-table'><thead><tr><th>Parameter</th><th>Value</th></tr></thead><tbody>")
                sb.AppendLine("<tr><td>Final Subgrade CDF</td><td>" & Format(lastIter.CDFMAX, "0.000000") & "</td></tr>")
                sb.AppendLine("<tr><td>Final |ln(CDF)|</td><td>" & Format(lastIter.CDFErr, "0.00000") & "</td></tr>")
                sb.AppendLine("<tr><td>Design Layer Final Thickness</td><td>" & Format(lastIter.Thickness, "0.00") & " " & thicknessUnit & "</td></tr>")
                sb.AppendLine("<tr><td>Converged</td><td>" & If(lastIter.CDFErr < CDF.CDFExitErr, "Yes", "No") & "</td></tr>")
                sb.AppendLine("<tr><td>Total Iterations</td><td>" & rpt.Iterations.Count.ToString() & "</td></tr>")
                If rpt.CDFSweep.NAircraftCaptured > 0 Then
                    sb.AppendLine("<tr><td>Critical Offset Position</td><td>" & Format((rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC, "0") & " " & lengthUnit & "</td></tr>")
                End If
                sb.AppendLine("</tbody></table>")
                sb.AppendLine("</section>")
            End If

            ' ===== Section J: ACR Details =====
            If rpt.ACRDetails.Count > 0 Then
                sb.AppendLine("<section id='section-j'>")
                sb.AppendLine("<h2><span class='sec-num'>J</span> ACR Details</h2>")
                For Each acrDet In rpt.ACRDetails
                    sb.AppendLine("<h3>" & WebEncode(acrDet.ACName) & " &mdash; " & WebEncode(acrDet.SubgradeCategory) & "</h3>")
                    sb.AppendLine("<table class='data-table'><thead><tr><th>Parameter</th><th>Value</th></tr></thead><tbody>")
                    sb.AppendLine("<tr><td>Designed Base Thickness</td><td>" & Format(acrDet.DesignedBaseThickness, "0.00") & " " & thicknessUnit & "</td></tr>")
                    sb.AppendLine("<tr><td>Final DSWL</td><td>" & Format(acrDet.FinalDSWL, "#,##0") & " lb</td></tr>")
                    sb.AppendLine("<tr><td>Final ACR</td><td>" & Format(acrDet.FinalACR, "0.0") & "</td></tr>")
                    sb.AppendLine("</tbody></table>")

                    If acrDet.DSWLIterations.Count > 0 Then
                        sb.AppendLine("<details><summary>DSWL Iteration Log (click to expand)</summary>")
                        sb.AppendLine("<table class='data-table compact'><thead><tr><th>#</th><th>Gear Load (lb)</th><th>NtoFail</th><th>CovACN</th><th>Delta</th></tr></thead><tbody>")
                        For Each dswlIter In acrDet.DSWLIterations
                            sb.AppendLine("<tr><td>" & dswlIter.IterationNumber.ToString() & "</td>" &
                                "<td>" & Format(dswlIter.Load, "#,##0") & "</td>" &
                                "<td>" & Format(dswlIter.NtoFail, "0.00E+00") & "</td>" &
                                "<td>" & Format(dswlIter.CovACN, "0.00E+00") & "</td>" &
                                "<td>" & Format(dswlIter.Delta, "0.00E+00") & "</td></tr>")
                        Next
                        sb.AppendLine("</tbody></table></details>")
                    End If
                Next
                sb.AppendLine("</section>")
            End If

            ' ===== Section K: PCR Elimination Rounds =====
            If rpt.PCRRounds.Count > 0 Then
                sb.AppendLine("<section id='section-k'>")
                sb.AppendLine("<h2><span class='sec-num'>K</span> PCR Elimination Rounds</h2>")
                sb.AppendLine("<table class='data-table'><thead><tr><th>Round</th><th>Critical Aircraft</th><th>Critical CDF</th><th>Final MGW (lb)</th><th>Round PCR</th><th>Early Exit</th></tr></thead><tbody>")
                Dim maxPCR As Single = 0
                For Each pcrRound In rpt.PCRRounds
                    If pcrRound.RoundPCR > maxPCR Then maxPCR = pcrRound.RoundPCR
                    sb.AppendLine("<tr><td>" & pcrRound.RoundNumber.ToString() & "</td>" &
                        "<td>" & WebEncode(If(pcrRound.CriticalAircraftName, "")) & "</td>" &
                        "<td>" & Format(pcrRound.CriticalAircraftCDF, "0.000000") & "</td>" &
                        "<td>" & Format(pcrRound.FinalMGW, "#,##0") & "</td>" &
                        "<td>" & Format(pcrRound.RoundPCR, "0.0") & "</td>" &
                        "<td>" & If(pcrRound.EarlyExit, "Yes", "No") & "</td></tr>")
                Next
                sb.AppendLine("</tbody></table>")
                sb.AppendLine("<div class='callout info'><strong>Final PCR = " & Format(maxPCR, "0.0") & "</strong></div>")
                sb.AppendLine("</section>")
            End If

            ' ===== Section L: ACR vs Damage =====
            If rpt.ACRDetails.Count > 0 AndAlso rpt.AircraftDetails IsNot Nothing Then
                sb.AppendLine("<section id='section-l'>")
                sb.AppendLine("<h2><span class='sec-num'>L</span> ACR vs. Damage Per Departure</h2>")

                AppendACRDamageSVG(sb, rpt)

                ' Summary table
                sb.AppendLine("<h3>ACR and PCR Summary</h3>")
                sb.AppendLine("<table class='data-table'><thead><tr><th>Aircraft</th><th>ACR</th><th>Ann. Departures</th><th>CDF Contribution</th><th>CDF per Departure</th></tr></thead><tbody>")
                For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                    If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                    Dim det = rpt.AircraftDetails(ia)
                    Dim acr As Double = 0
                    For Each acrDet In rpt.ACRDetails
                        If acrDet.ACName = det.ACName Then acr = acrDet.FinalACR : Exit For
                    Next
                    Dim cdfPerDep As Double = If(det.AnnualDepartures > 0, det.CDFAtCriticalOffset / (det.AnnualDepartures * 20), 0)
                    sb.AppendLine("<tr><td>" & WebEncode(det.ACName) & "</td>" &
                        "<td>" & If(acr > 0, Format(acr, "0.0"), "N/A") & "</td>" &
                        "<td>" & Format(det.AnnualDepartures, "#,##0") & "</td>" &
                        "<td>" & Format(det.CDFAtCriticalOffset, "0.000000") & "</td>" &
                        "<td>" & Format(cdfPerDep, "0.000E+00") & "</td></tr>")
                Next
                If rpt.PCRRounds.Count > 0 Then
                    Dim finalPCR As Single = 0
                    For Each pr In rpt.PCRRounds
                        If pr.RoundPCR > finalPCR Then finalPCR = pr.RoundPCR
                    Next
                    sb.AppendLine("<tr class='highlight'><td><strong>Pavement PCR</strong></td><td><strong>" &
                        Format(finalPCR, "0.0") & "</strong></td><td>&mdash;</td><td>&mdash;</td><td>&mdash;</td></tr>")
                End If
                sb.AppendLine("</tbody></table>")
                sb.AppendLine("</section>")
            End If

            ' ===== Footer =====
            sb.AppendLine("<footer>")
            sb.AppendLine("<p>Generated by FAARFIELD &mdash; Federal Aviation Administration</p>")
            sb.AppendLine("<p><a href='#toc'>&uarr; Back to Table of Contents</a></p>")
            sb.AppendLine("</footer>")

            sb.AppendLine("</body></html>")
            Return sb.ToString()
        End Function

#Region "Helper Methods"

        Private Shared Sub AppendCard(sb As StringBuilder, label As String, value As String, unit As String)
            sb.Append("<div class='card'><div class='card-label'>" & label & "</div>")
            sb.Append("<div class='card-value'>" & value)
            If unit <> "" Then sb.Append(" <span class='card-unit'>" & unit & "</span>")
            sb.AppendLine("</div></div>")
        End Sub

        Private Shared Sub AppendParamRow(sb As StringBuilder, param As String, value As String, desc As String)
            sb.AppendLine("<tr><td>" & param & "</td><td>" & value & "</td><td>" & desc & "</td></tr>")
        End Sub

        Private Shared Sub AppendLayerTable(sb As StringBuilder, title As String, layers As List(Of clsLayerInfo), thkUnit As String, presUnit As String)
            If title <> "" Then sb.AppendLine("<h3>" & title & "</h3>")
            sb.AppendLine("<table class='data-table'><thead><tr><th>Layer #</th><th>Thickness (" & thkUnit & ")</th><th>Modulus (" & presUnit & ")</th><th>LCode</th></tr></thead><tbody>")
            Dim num As Integer = 1
            For Each layer In layers
                Dim thkStr = If(num < layers.Count, Format(layer.Thickness, "0.00"), "Semi-infinite")
                sb.AppendLine("<tr><td>" & num.ToString() & "</td><td>" & thkStr & "</td><td>" & Format(layer.Modulus, "#,##0") & "</td><td>" & layer.LCode.ToString() & "</td></tr>")
                num += 1
            Next
            sb.AppendLine("</tbody></table>")
        End Sub

        Private Shared Function WebEncode(s As String) As String
            If s Is Nothing Then Return ""
            Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
        End Function

#End Region

#Region "SVG Chart: Fatigue Curve"

        Private Shared Sub AppendFatigueCurveSVG(sb As StringBuilder, rpt As clsDetailedReportData, subgradeMod As Double)
            Dim svgW As Integer = 850, svgH As Integer = 500
            Dim ml As Integer = 80, mr As Integer = 30, mt As Integer = 40, mb As Integer = 60
            Dim pw As Integer = svgW - ml - mr, ph As Integer = svgH - mt - mb

            ' Data range
            Dim computedAA As Double = 0.000247 + 0.000245 * Math.Log10(subgradeMod)
            Dim computedBB As Double = 0.0658 * subgradeMod ^ 0.559

            ' Strain range in microstrain: 100 to 10000
            Dim logStrainMin As Double = 2, logStrainMax As Double = 4 ' log10(microstrain)
            ' N range: 10^0 to 10^10
            Dim logNMin As Double = 0, logNMax As Double = 10

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg'>")

            ' Plot background
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#ccc'/>")

            ' Grid lines
            For i As Integer = 0 To 10
                Dim y = mt + ph - (i / 10.0) * ph
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#e8e8e8' stroke-width='0.5'/>")
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>10<tspan dy='-5' font-size='7'>" & i.ToString() & "</tspan></text>")
            Next
            For i As Integer = 2 To 4
                Dim x = ml + ((i - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                sb.AppendLine("<line x1='" & Fmt(x) & "' y1='" & mt & "' x2='" & Fmt(x) & "' y2='" & (mt + ph) & "' stroke='#e8e8e8' stroke-width='0.5'/>")
                Dim lbl = CInt(10 ^ i)
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & Format(lbl, "#,##0") & "</text>")
            Next

            ' Fatigue model curve
            Dim pathD As New StringBuilder("M")
            Dim nPts As Integer = 200
            For i As Integer = 0 To nPts
                Dim logS = logStrainMin + (logStrainMax - logStrainMin) * i / nPts
                Dim strainAbs As Double = (10 ^ logS) / 1000000.0
                Dim nFail As Double = 10000 * (computedAA / strainAbs) ^ computedBB
                Dim logN As Double = Math.Log10(Math.Max(nFail, 1))
                Dim x = ml + ((logS - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                Dim y = mt + ph - ((logN - logNMin) / (logNMax - logNMin)) * ph
                y = Math.Max(mt, Math.Min(mt + ph, y))
                If i = 0 Then pathD.Append(Fmt(x) & " " & Fmt(y)) Else pathD.Append(" L" & Fmt(x) & " " & Fmt(y))
            Next
            sb.AppendLine("<path d='" & pathD.ToString() & "' fill='none' stroke='#1F77B4' stroke-width='2.5'/>")

            ' Aircraft scatter points
            If rpt.AircraftDetails IsNot Nothing Then
                For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                    If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                    Dim det = rpt.AircraftDetails(ia)
                    Dim micro = det.VerticalStrain * 1000000
                    If micro <= 0 Then Continue For
                    Dim logS = Math.Log10(micro)
                    Dim logN = Math.Log10(Math.Max(det.NtoFail, 1))
                    Dim x = ml + ((logS - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                    Dim y = mt + ph - ((logN - logNMin) / (logNMax - logNMin)) * ph
                    Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                    sb.AppendLine("<circle cx='" & Fmt(x) & "' cy='" & Fmt(y) & "' r='5' fill='" & clr & "' stroke='white' stroke-width='1.5'/>")
                    sb.AppendLine("<text x='" & Fmt(x + 8) & "' y='" & Fmt(y + 4) & "' class='label'>" & WebEncode(det.ACName) & "</text>")

                    ' Repetitions horizontal dashed line
                    If det.TotalRepetitions > 0 Then
                        Dim logR = Math.Log10(det.TotalRepetitions)
                        Dim yR = mt + ph - ((logR - logNMin) / (logNMax - logNMin)) * ph
                        If yR >= mt AndAlso yR <= mt + ph Then
                            sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(yR) & "' x2='" & (ml + pw) & "' y2='" & Fmt(yR) & "' stroke='" & clr & "' stroke-width='1' stroke-dasharray='6,3' opacity='0.5'/>")
                        End If
                    End If
                Next
            End If

            ' Axis labels
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>Vertical Strain (&mu;&epsilon;)</text>")
            sb.AppendLine("<text x='15' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,15," & Fmt(mt + ph / 2) & ")'>Allowable Repetitions (N<tspan dy='-4' font-size='8'>fail</tspan><tspan dy='4'>)</tspan></text>")

            ' Title
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>Subgrade Fatigue Model</text>")

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Life Ratio"

        Private Shared Sub AppendLifeRatioSVG(sb As StringBuilder, rpt As clsDetailedReportData)
            If rpt.AircraftDetails Is Nothing Then Return
            Dim items As New List(Of Tuple(Of String, Double, String))
            For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                Dim det = rpt.AircraftDetails(ia)
                Dim ratio As Double = If(det.TotalRepetitions > 0, det.NtoFail / det.TotalRepetitions, 0)
                items.Add(Tuple.Create(det.ACName, ratio, ChartColors((ia - 1) Mod ChartColors.Length)))
            Next
            If items.Count = 0 Then Return

            Dim barH As Integer = 28, gap As Integer = 8
            Dim svgH = 60 + items.Count * (barH + gap)
            Dim svgW As Integer = 800, ml As Integer = 180, mr As Integer = 30
            Dim pw = svgW - ml - mr

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg'>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>Fatigue Life Reserve (N<tspan dy='-4' font-size='8'>fail</tspan><tspan dy='4'> / Repetitions)</tspan></text>")

            Dim maxLogRatio As Double = 0
            For Each item In items
                Dim lr = If(item.Item2 > 0, Math.Log10(item.Item2), 0)
                If Math.Abs(lr) > maxLogRatio Then maxLogRatio = Math.Abs(lr)
            Next
            If maxLogRatio < 1 Then maxLogRatio = 1
            maxLogRatio = Math.Ceiling(maxLogRatio)

            Dim centerX = ml + pw / 2
            Dim yStart = 40

            ' Reference line at ratio=1 (log=0)
            sb.AppendLine("<line x1='" & Fmt(centerX) & "' y1='" & yStart & "' x2='" & Fmt(centerX) & "' y2='" & (yStart + items.Count * (barH + gap)) & "' stroke='#333' stroke-width='1.5'/>")
            sb.AppendLine("<text x='" & Fmt(centerX) & "' y='" & (yStart - 5) & "' text-anchor='middle' class='tick'>Ratio = 1.0</text>")

            For i As Integer = 0 To items.Count - 1
                Dim item = items(i)
                Dim yy = yStart + i * (barH + gap)
                Dim logR = If(item.Item2 > 0, Math.Log10(item.Item2), -maxLogRatio)
                logR = Math.Max(-maxLogRatio, Math.Min(maxLogRatio, logR))
                Dim barWidth = (logR / maxLogRatio) * (pw / 2)
                Dim clr = If(logR >= 0, "#2CA02C", "#D62728")

                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(yy + barH / 2 + 4) & "' text-anchor='end' class='label'>" & WebEncode(item.Item1) & "</text>")
                If barWidth >= 0 Then
                    sb.AppendLine("<rect x='" & Fmt(centerX) & "' y='" & Fmt(yy) & "' width='" & Fmt(barWidth) & "' height='" & barH & "' fill='" & clr & "' opacity='0.75' rx='3'/>")
                Else
                    sb.AppendLine("<rect x='" & Fmt(centerX + barWidth) & "' y='" & Fmt(yy) & "' width='" & Fmt(-barWidth) & "' height='" & barH & "' fill='" & clr & "' opacity='0.75' rx='3'/>")
                End If
                sb.AppendLine("<text x='" & Fmt(centerX + barWidth + If(barWidth >= 0, 5, -5)) & "' y='" & Fmt(yy + barH / 2 + 4) & "' text-anchor='" & If(barWidth >= 0, "start", "end") & "' class='tick'>" & Format(item.Item2, "0.00E+00") & "</text>")
            Next

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Single Aircraft CDF"

        Private Shared Sub AppendSingleAircraftCDFSvg(sb As StringBuilder, det As clsAircraftDetail, critOffset As Integer, acColor As String, lengthUnit As String)
            Dim svgW As Integer = 750, svgH As Integer = 400
            Dim ml As Integer = 70, mr As Integer = 25, mt As Integer = 40, mb As Integer = 50
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            Dim maxCDF As Double = 0
            For ioff As Integer = 1 To CDF.NOFF
                If det.CDFByOffset(ioff) > maxCDF Then maxCDF = det.CDFByOffset(ioff)
            Next
            If maxCDF <= 0 Then maxCDF = 0.001
            Dim yMax = maxCDF * 1.2

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg'>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#ccc'/>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>" & WebEncode(det.ACName) & " &mdash; CDF vs Offset</text>")

            ' Data path
            Dim pathD As New StringBuilder("M")
            For ioff As Integer = 1 To CDF.NOFF
                Dim offsetVal = (ioff - 1) * CDF.OFFSETINC
                Dim x = ml + (offsetVal / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                Dim y = mt + ph - (det.CDFByOffset(ioff) / yMax) * ph
                If ioff = 1 Then pathD.Append(Fmt(x) & " " & Fmt(y)) Else pathD.Append(" L" & Fmt(x) & " " & Fmt(y))
            Next
            sb.AppendLine("<path d='" & pathD.ToString() & "' fill='none' stroke='" & acColor & "' stroke-width='2'/>")

            ' Critical offset marker
            If critOffset >= 1 AndAlso critOffset <= CDF.NOFF Then
                Dim critX = ml + ((critOffset - 1) * CDF.OFFSETINC / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                sb.AppendLine("<line x1='" & Fmt(critX) & "' y1='" & mt & "' x2='" & Fmt(critX) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
            End If

            ' Y axis ticks
            Dim nYTicks As Integer = 5
            For i As Integer = 0 To nYTicks
                Dim val = yMax * i / nYTicks
                Dim y = mt + ph - (i / CDbl(nYTicks)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>" & Format(val, "0.000000") & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#eee' stroke-width='0.5'/>")
            Next
            ' X axis ticks
            For i As Integer = 0 To 4
                Dim val = i * 100
                Dim x = ml + (val / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & val.ToString() & "</text>")
            Next

            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>Offset (" & lengthUnit & ")</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>CDF</text>")
            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Coverage Plot (C/P Distribution)"

        Private Shared Sub AppendCoveragePlotSVG(sb As StringBuilder, rpt As clsDetailedReportData, lengthUnit As String)
            Dim svgW As Integer = 850, svgH As Integer = 450
            Dim ml As Integer = 80, mr As Integer = 150, mt As Integer = 40, mb As Integer = 50
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            Dim maxCP As Double = 0
            For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                For ioff As Integer = 1 To CDF.NOFF
                    If rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, ioff) > maxCP Then
                        maxCP = rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, ioff)
                    End If
                Next
            Next
            If maxCP <= 0 Then maxCP = 0.001
            Dim yMax = maxCP * 1.15

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg'>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#ccc'/>")
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='20' text-anchor='middle' class='chart-title'>Coverage-to-Pass (C/P) Distribution</text>")

            ' Draw curves per aircraft
            For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                Dim pathD As New StringBuilder("M")
                For ioff As Integer = 1 To CDF.NOFF
                    Dim offsetVal = (ioff - 1) * CDF.OFFSETINC
                    Dim x = ml + (offsetVal / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                    Dim y = mt + ph - (rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, ioff) / yMax) * ph
                    If ioff = 1 Then pathD.Append(Fmt(x) & " " & Fmt(y)) Else pathD.Append(" L" & Fmt(x) & " " & Fmt(y))
                Next
                sb.AppendLine("<path d='" & pathD.ToString() & "' fill='none' stroke='" & clr & "' stroke-width='2'/>")
            Next

            ' Critical offset line
            If rpt.CDFSweep.MaxCDFOffset >= 1 Then
                Dim critX = ml + ((rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                sb.AppendLine("<line x1='" & Fmt(critX) & "' y1='" & mt & "' x2='" & Fmt(critX) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
            End If

            ' Y ticks
            Dim nYT As Integer = 5
            For i As Integer = 0 To nYT
                Dim val = yMax * i / nYT
                Dim y = mt + ph - (i / CDbl(nYT)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>" & Format(val, "0.0000") & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#eee' stroke-width='0.5'/>")
            Next
            ' X ticks
            For i As Integer = 0 To 4
                Dim val = i * 100
                Dim x = ml + (val / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & val.ToString() & "</text>")
            Next

            ' Legend
            Dim legX = ml + pw + 10, legY = mt + 10
            For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                Dim acName = "AC" & ia.ToString()
                If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                    acName = rpt.AircraftDetails(ia).ACName
                End If
                Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                sb.AppendLine("<rect x='" & legX & "' y='" & Fmt(legY + (ia - 1) * 18) & "' width='12' height='12' fill='" & clr & "'/>")
                sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(legY + (ia - 1) * 18 + 10) & "' class='legend-text'>" & WebEncode(acName) & "</text>")
            Next

            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>Offset (" & lengthUnit & ")</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>C/P Ratio</text>")
            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Composite CDF"

        Private Shared Sub AppendCompositeCDFSvg(sb As StringBuilder, rpt As clsDetailedReportData, lengthUnit As String)
            Dim svgW As Integer = 900, svgH As Integer = 500
            Dim ml As Integer = 80, mr As Integer = 150, mt As Integer = 40, mb As Integer = 50
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            Dim maxCDF As Double = 0
            For ioff As Integer = 1 To CDF.NOFF
                If rpt.CDFSweep.CDFTotalPerOffset(ioff) > maxCDF Then maxCDF = rpt.CDFSweep.CDFTotalPerOffset(ioff)
            Next
            If maxCDF <= 0 Then maxCDF = 0.001
            Dim yMax = maxCDF * 1.2

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg'>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#ccc'/>")
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='20' text-anchor='middle' class='chart-title'>Composite CDF Across Pavement Width</text>")

            ' Per-aircraft CDF curves
            For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                Dim pathD As New StringBuilder("M")
                For ioff As Integer = 1 To CDF.NOFF
                    Dim offsetVal = (ioff - 1) * CDF.OFFSETINC
                    Dim x = ml + (offsetVal / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                    Dim y = mt + ph - (rpt.CDFSweep.CDFPerAircraftPerOffset(ia, ioff) / yMax) * ph
                    If ioff = 1 Then pathD.Append(Fmt(x) & " " & Fmt(y)) Else pathD.Append(" L" & Fmt(x) & " " & Fmt(y))
                Next
                sb.AppendLine("<path d='" & pathD.ToString() & "' fill='none' stroke='" & clr & "' stroke-width='1.5' opacity='0.7'/>")
            Next

            ' Total CDF (thick black)
            Dim totalPath As New StringBuilder("M")
            For ioff As Integer = 1 To CDF.NOFF
                Dim offsetVal = (ioff - 1) * CDF.OFFSETINC
                Dim x = ml + (offsetVal / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                Dim y = mt + ph - (rpt.CDFSweep.CDFTotalPerOffset(ioff) / yMax) * ph
                If ioff = 1 Then totalPath.Append(Fmt(x) & " " & Fmt(y)) Else totalPath.Append(" L" & Fmt(x) & " " & Fmt(y))
            Next
            sb.AppendLine("<path d='" & totalPath.ToString() & "' fill='none' stroke='#222' stroke-width='2.5'/>")

            ' Critical offset
            If rpt.CDFSweep.MaxCDFOffset >= 1 Then
                Dim critX = ml + ((rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                sb.AppendLine("<line x1='" & Fmt(critX) & "' y1='" & mt & "' x2='" & Fmt(critX) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1.5' stroke-dasharray='5,3'/>")
            End If

            ' Y ticks
            Dim nYT As Integer = 5
            For i As Integer = 0 To nYT
                Dim val = yMax * i / nYT
                Dim y = mt + ph - (i / CDbl(nYT)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>" & Format(val, "0.000000") & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#eee' stroke-width='0.5'/>")
            Next
            ' X ticks
            For i As Integer = 0 To 4
                Dim val = i * 100
                Dim x = ml + (val / ((CDF.NOFF - 1) * CDF.OFFSETINC)) * pw
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & val.ToString() & "</text>")
            Next

            ' Legend
            Dim legX = ml + pw + 10, legY = mt + 10
            For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                Dim acName = "AC" & ia.ToString()
                If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                    acName = rpt.AircraftDetails(ia).ACName
                End If
                Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                sb.AppendLine("<rect x='" & legX & "' y='" & Fmt(legY + (ia - 1) * 18) & "' width='12' height='12' fill='" & clr & "'/>")
                sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(legY + (ia - 1) * 18 + 10) & "' class='legend-text'>" & WebEncode(acName) & "</text>")
            Next
            Dim totLegY = legY + rpt.CDFSweep.NAircraftCaptured * 18
            sb.AppendLine("<line x1='" & legX & "' y1='" & Fmt(totLegY + 6) & "' x2='" & (legX + 12) & "' y2='" & Fmt(totLegY + 6) & "' stroke='#222' stroke-width='2.5'/>")
            sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(totLegY + 10) & "' class='legend-text'>Total CDF</text>")

            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>Offset (" & lengthUnit & ")</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>CDF</text>")
            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: CDF Contribution Bar"

        Private Shared Sub AppendCDFContributionSVG(sb As StringBuilder, rpt As clsDetailedReportData)
            If rpt.AircraftDetails Is Nothing Then Return
            Dim nAc As Integer = rpt.CDFSweep.NAircraftCaptured
            If nAc = 0 Then Return

            Dim totalCritCDF As Double = rpt.CDFSweep.CDFTotalPerOffset(rpt.CDFSweep.MaxCDFOffset)
            If totalCritCDF <= 0 Then Return

            Dim barH As Integer = 28, gap As Integer = 8
            Dim svgH = 50 + nAc * (barH + gap)
            Dim svgW As Integer = 800, ml As Integer = 180, mr As Integer = 60
            Dim pw = svgW - ml - mr

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg'>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>CDF Contribution at Critical Offset (%)</text>")

            Dim yStart = 40
            For ia As Integer = 1 To nAc
                Dim acName = "AC" & ia.ToString()
                If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                    acName = rpt.AircraftDetails(ia).ACName
                End If
                Dim acCDF = rpt.CDFSweep.CDFPerAircraftPerOffset(ia, rpt.CDFSweep.MaxCDFOffset)
                Dim pct = acCDF / totalCritCDF * 100
                Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                Dim yy = yStart + (ia - 1) * (barH + gap)
                Dim bw = (pct / 100) * pw

                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(yy + barH / 2 + 4) & "' text-anchor='end' class='label'>" & WebEncode(acName) & "</text>")
                sb.AppendLine("<rect x='" & ml & "' y='" & Fmt(yy) & "' width='" & Fmt(bw) & "' height='" & barH & "' fill='" & clr & "' opacity='0.8' rx='3'/>")
                sb.AppendLine("<text x='" & Fmt(ml + bw + 5) & "' y='" & Fmt(yy + barH / 2 + 4) & "' class='tick'>" & Format(pct, "0.0") & "%</text>")
            Next
            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Convergence"

        Private Shared Sub AppendConvergenceSVG(sb As StringBuilder, rpt As clsDetailedReportData, thkUnit As String)
            Dim svgW As Integer = 850, svgH As Integer = 450
            Dim ml As Integer = 70, mr As Integer = 70, mt As Integer = 40, mb As Integer = 50
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            Dim nIter = rpt.Iterations.Count
            If nIter < 2 Then Return

            ' Ranges
            Dim maxErr As Double = 0, minErr As Double = 1.0E+10
            Dim maxThk As Double = 0, minThk As Double = 1.0E+10
            For Each iter In rpt.Iterations
                If iter.CDFErr > 0 Then
                    Dim logE = Math.Log10(iter.CDFErr)
                    If logE > maxErr Then maxErr = logE
                    If logE < minErr Then minErr = logE
                End If
                If iter.Thickness > maxThk Then maxThk = iter.Thickness
                If iter.Thickness < minThk Then minThk = iter.Thickness
            Next
            maxErr = Math.Ceiling(maxErr)
            minErr = Math.Floor(Math.Max(minErr, -4))
            If maxThk = minThk Then maxThk = minThk + 1

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg'>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#ccc'/>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>Newton-Raphson Convergence</text>")

            ' Error line (left axis, blue)
            Dim errPath As New StringBuilder("M")
            For i As Integer = 0 To nIter - 1
                Dim iter = rpt.Iterations(i)
                Dim x = ml + (i / CDbl(nIter - 1)) * pw
                Dim logE = If(iter.CDFErr > 0, Math.Log10(iter.CDFErr), minErr)
                Dim y = mt + ph - ((logE - minErr) / (maxErr - minErr)) * ph
                If i = 0 Then errPath.Append(Fmt(x) & " " & Fmt(y)) Else errPath.Append(" L" & Fmt(x) & " " & Fmt(y))
                sb.AppendLine("<circle cx='" & Fmt(x) & "' cy='" & Fmt(y) & "' r='3' fill='#1F77B4'/>")
            Next
            sb.AppendLine("<path d='" & errPath.ToString() & "' fill='none' stroke='#1F77B4' stroke-width='2'/>")

            ' Thickness line (right axis, red)
            Dim thkPath As New StringBuilder("M")
            For i As Integer = 0 To nIter - 1
                Dim iter = rpt.Iterations(i)
                Dim x = ml + (i / CDbl(nIter - 1)) * pw
                Dim y = mt + ph - ((iter.Thickness - minThk) / (maxThk - minThk)) * ph
                If i = 0 Then thkPath.Append(Fmt(x) & " " & Fmt(y)) Else thkPath.Append(" L" & Fmt(x) & " " & Fmt(y))
                sb.AppendLine("<circle cx='" & Fmt(x) & "' cy='" & Fmt(y) & "' r='3' fill='#D62728'/>")
            Next
            sb.AppendLine("<path d='" & thkPath.ToString() & "' fill='none' stroke='#D62728' stroke-width='2'/>")

            ' Convergence threshold
            Dim logThresh = Math.Log10(CDF.CDFExitErr)
            If logThresh >= minErr AndAlso logThresh <= maxErr Then
                Dim yThresh = mt + ph - ((logThresh - minErr) / (maxErr - minErr)) * ph
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(yThresh) & "' x2='" & (ml + pw) & "' y2='" & Fmt(yThresh) & "' stroke='#2CA02C' stroke-width='1' stroke-dasharray='6,3'/>")
            End If

            ' Left Y axis ticks
            For i As Integer = CInt(minErr) To CInt(maxErr)
                Dim y = mt + ph - ((i - minErr) / (maxErr - minErr)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick' fill='#1F77B4'>10<tspan dy='-5' font-size='7'>" & i.ToString() & "</tspan></text>")
            Next
            ' Right Y axis ticks
            Dim nRT As Integer = 4
            For i As Integer = 0 To nRT
                Dim val = minThk + (maxThk - minThk) * i / nRT
                Dim y = mt + ph - (i / CDbl(nRT)) * ph
                sb.AppendLine("<text x='" & (ml + pw + 5) & "' y='" & Fmt(y + 4) & "' text-anchor='start' class='tick' fill='#D62728'>" & Format(val, "0.0") & "</text>")
            Next
            ' X axis ticks
            For i As Integer = 0 To nIter - 1
                Dim x = ml + (i / CDbl(nIter - 1)) * pw
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & (i + 1).ToString() & "</text>")
            Next

            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>Iteration</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' fill='#1F77B4' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>|ln(CDF)| (log scale)</text>")
            sb.AppendLine("<text x='" & (svgW - 8) & "' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' fill='#D62728' transform='rotate(90," & (svgW - 8) & "," & Fmt(mt + ph / 2) & ")'>Thickness (" & thkUnit & ")</text>")

            ' Legend
            sb.AppendLine("<rect x='" & (ml + 10) & "' y='" & (mt + 5) & "' width='12' height='12' fill='#1F77B4'/>")
            sb.AppendLine("<text x='" & (ml + 26) & "' y='" & (mt + 15) & "' class='legend-text'>|ln(CDF)| error</text>")
            sb.AppendLine("<rect x='" & (ml + 10) & "' y='" & (mt + 22) & "' width='12' height='12' fill='#D62728'/>")
            sb.AppendLine("<text x='" & (ml + 26) & "' y='" & (mt + 32) & "' class='legend-text'>Thickness</text>")

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: ACR Damage"

        Private Shared Sub AppendACRDamageSVG(sb As StringBuilder, rpt As clsDetailedReportData)
            If rpt.AircraftDetails Is Nothing OrElse rpt.ACRDetails.Count = 0 Then Return

            Dim svgW As Integer = 850, svgH As Integer = 500
            Dim ml As Integer = 80, mr As Integer = 30, mt As Integer = 40, mb As Integer = 60
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            ' Collect data
            Dim pts As New List(Of Tuple(Of Double, Double, Double, String, String)) ' acr, cdfPerDep, annDep, name, color
            For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                Dim det = rpt.AircraftDetails(ia)
                Dim acr As Double = 0
                For Each acrDet In rpt.ACRDetails
                    If acrDet.ACName = det.ACName Then acr = acrDet.FinalACR : Exit For
                Next
                If acr <= 0 Then Continue For
                Dim cdfPerDep = If(det.AnnualDepartures > 0, det.CDFAtCriticalOffset / (det.AnnualDepartures * 20), 0)
                If cdfPerDep <= 0 Then Continue For
                pts.Add(Tuple.Create(acr, cdfPerDep, CDbl(det.AnnualDepartures), det.ACName, ChartColors((ia - 1) Mod ChartColors.Length)))
            Next
            If pts.Count = 0 Then Return

            Dim minACR = pts.Min(Function(p) p.Item1) * 0.8
            Dim maxACR = pts.Max(Function(p) p.Item1) * 1.2
            Dim minLogCPD = Math.Floor(Math.Log10(pts.Min(Function(p) p.Item2)))
            Dim maxLogCPD = Math.Ceiling(Math.Log10(pts.Max(Function(p) p.Item2)))
            Dim maxDep = pts.Max(Function(p) p.Item3)

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg'>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#ccc'/>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>ACR vs. CDF per Departure</text>")

            For Each pt In pts
                Dim x = ml + ((pt.Item1 - minACR) / (maxACR - minACR)) * pw
                Dim logV = Math.Log10(pt.Item2)
                Dim y = mt + ph - ((logV - minLogCPD) / (maxLogCPD - minLogCPD)) * ph
                Dim r = 8 + 20 * (pt.Item3 / maxDep)
                sb.AppendLine("<circle cx='" & Fmt(x) & "' cy='" & Fmt(y) & "' r='" & Fmt(r) & "' fill='" & pt.Item5 & "' opacity='0.6' stroke='" & pt.Item5 & "' stroke-width='1.5'/>")
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & Fmt(y - r - 4) & "' text-anchor='middle' class='label'>" & WebEncode(pt.Item4) & "</text>")
            Next

            ' Axes
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>ACR</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>CDF per Departure (log scale)</text>")

            ' Y ticks
            For i As Integer = CInt(minLogCPD) To CInt(maxLogCPD)
                Dim y = mt + ph - ((i - minLogCPD) / (maxLogCPD - minLogCPD)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>1E" & i.ToString() & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#eee' stroke-width='0.5'/>")
            Next

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "Number Formatting Helper"

        Private Shared Function Fmt(v As Double) As String
            Return Format(v, "0.#")
        End Function

#End Region

#Region "CSS Stylesheet"

        Private Shared Function GetCss() As String
            Return "
:root {
  --primary: #1a3c6e;
  --primary-light: #e8eef6;
  --accent: #D62728;
  --text: #2c3e50;
  --text-light: #6c7a89;
  --border: #d5dce6;
  --bg: #ffffff;
  --bg-alt: #f8f9fb;
  --success: #2CA02C;
  --warning: #FF7F0E;
  --radius: 6px;
}

* { margin: 0; padding: 0; box-sizing: border-box; }

body {
  font-family: 'Segoe UI', system-ui, -apple-system, sans-serif;
  color: var(--text);
  background: var(--bg);
  line-height: 1.6;
  max-width: 1100px;
  margin: 0 auto;
  padding: 20px 40px;
  font-size: 14px;
}

/* Header */
.report-header {
  text-align: center;
  padding: 30px 0 20px;
  border-bottom: 3px solid var(--primary);
  margin-bottom: 30px;
}
.report-header h1 {
  color: var(--primary);
  font-size: 22px;
  font-weight: 700;
  margin-bottom: 4px;
}
.subtitle { color: var(--text-light); font-size: 12px; margin-bottom: 12px; }
.header-meta {
  display: flex; justify-content: center; gap: 24px; flex-wrap: wrap;
  font-size: 13px; color: var(--text-light);
}

/* Dashboard cards */
.dashboard {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(155px, 1fr));
  gap: 12px;
  margin-bottom: 30px;
}
.card {
  background: var(--bg-alt);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 14px 16px;
  text-align: center;
}
.card-label { font-size: 11px; text-transform: uppercase; letter-spacing: 0.5px; color: var(--text-light); margin-bottom: 4px; }
.card-value { font-size: 20px; font-weight: 700; color: var(--primary); }
.card-unit { font-size: 12px; font-weight: 400; color: var(--text-light); }

/* TOC */
.toc {
  background: var(--bg-alt);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 20px 28px;
  margin-bottom: 30px;
}
.toc h2 { font-size: 16px; margin-bottom: 10px; color: var(--primary); }
.toc ol { padding-left: 20px; columns: 2; column-gap: 30px; }
.toc li { margin-bottom: 4px; font-size: 13px; break-inside: avoid; }
.toc a { color: var(--primary); text-decoration: none; }
.toc a:hover { text-decoration: underline; }

/* Section headings */
section { margin-bottom: 36px; page-break-inside: avoid; }
section > h2 {
  font-size: 18px;
  color: var(--primary);
  border-bottom: 2px solid var(--primary-light);
  padding-bottom: 6px;
  margin-bottom: 16px;
}
.sec-num {
  display: inline-block;
  background: var(--primary);
  color: white;
  width: 26px; height: 26px;
  text-align: center;
  line-height: 26px;
  border-radius: 50%;
  font-size: 13px;
  margin-right: 8px;
  vertical-align: middle;
}
h3 { font-size: 15px; color: var(--text); margin: 18px 0 10px; }
h4 { font-size: 14px; color: var(--text-light); margin: 14px 0 8px; }

/* Tables */
.data-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 16px;
  font-size: 13px;
}
.data-table th {
  background: var(--primary);
  color: white;
  padding: 8px 10px;
  text-align: left;
  font-weight: 600;
  font-size: 12px;
  white-space: nowrap;
}
.data-table td {
  padding: 6px 10px;
  border-bottom: 1px solid var(--border);
}
.data-table tbody tr:hover { background: var(--primary-light); }
.data-table.compact td, .data-table.compact th { padding: 4px 8px; font-size: 12px; }
.highlight { background: #fff8e1 !important; }
.highlight td { font-weight: 600; }
.table-scroll { overflow-x: auto; margin-bottom: 16px; }
.param-table td:first-child { font-weight: 600; white-space: nowrap; }

/* Equation cards */
.equation-card {
  background: linear-gradient(135deg, #f0f4fa 0%, #e8eef6 100%);
  border-left: 4px solid var(--primary);
  border-radius: 0 var(--radius) var(--radius) 0;
  padding: 16px 20px;
  margin-bottom: 14px;
}
.equation-card h4 { color: var(--primary); margin: 0 0 8px; font-size: 13px; }
.eq {
  font-family: 'Cambria Math', 'Latin Modern Math', 'STIX Two Math', serif;
  font-size: 15px;
  margin: 4px 0;
  padding: 2px 0;
}
.eq-note { font-size: 12px; color: var(--text-light); margin-top: 8px; border-top: 1px dashed var(--border); padding-top: 6px; }

/* Callouts */
.callout {
  border-radius: var(--radius);
  padding: 14px 18px;
  margin-bottom: 16px;
  font-size: 13px;
  line-height: 1.5;
}
.callout.info {
  background: var(--primary-light);
  border-left: 4px solid var(--primary);
}
.callout.warn {
  background: #fff8e1;
  border-left: 4px solid var(--warning);
}
.alert {
  background: #fdecea;
  border: 1px solid var(--accent);
  border-radius: var(--radius);
  padding: 20px;
  text-align: center;
  color: var(--accent);
  font-weight: 600;
}

/* Charts */
.chart-wrap {
  margin: 16px 0;
  text-align: center;
}
.chart-svg {
  width: 100%;
  max-width: 900px;
  height: auto;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: white;
}
.chart-svg .chart-title { font: bold 12px 'Segoe UI', sans-serif; fill: var(--text); }
.chart-svg .axis-label { font: 11px 'Segoe UI', sans-serif; fill: var(--text); }
.chart-svg .tick { font: 9px 'Segoe UI', sans-serif; fill: var(--text-light); }
.chart-svg .label { font: 10px 'Segoe UI', sans-serif; fill: var(--text); }
.chart-svg .legend-text { font: 10px 'Segoe UI', sans-serif; fill: var(--text); }

/* Steps */
.steps {
  background: var(--bg-alt);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 16px 20px;
  margin: 12px 0;
}
.steps h4 { margin: 0 0 10px; }
.steps ol { padding-left: 24px; }
.steps li { margin-bottom: 6px; font-size: 13px; }

/* Aircraft block */
.aircraft-block {
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 20px;
  margin-bottom: 24px;
  page-break-inside: avoid;
}
.aircraft-block h3 {
  color: var(--primary);
  border-bottom: 1px solid var(--border);
  padding-bottom: 6px;
  margin: 0 0 14px;
}

/* Collapsible details */
details {
  border: 1px solid var(--border);
  border-radius: var(--radius);
  margin-bottom: 12px;
}
details summary {
  padding: 10px 14px;
  cursor: pointer;
  font-weight: 600;
  font-size: 13px;
  background: var(--bg-alt);
  border-radius: var(--radius);
}
details[open] summary { border-bottom: 1px solid var(--border); border-radius: var(--radius) var(--radius) 0 0; }
details > table, details > div { margin: 0; }

/* Footer */
footer {
  text-align: center;
  padding: 20px 0;
  border-top: 2px solid var(--border);
  margin-top: 40px;
  color: var(--text-light);
  font-size: 12px;
}
footer a { color: var(--primary); text-decoration: none; }

/* Print */
@media print {
  body { max-width: 100%; padding: 10px; font-size: 11px; }
  .toc { break-after: page; }
  section { break-inside: avoid; }
  .dashboard { grid-template-columns: repeat(3, 1fr); }
  details[open] summary ~ * { display: block !important; }
}
"
        End Function

#End Region

    End Class
End Namespace
