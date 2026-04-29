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

            ' Figure counter for sequential numbering
            Dim figNum As Integer = 0

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
            sb.AppendLine("<meta http-equiv='X-UA-Compatible' content='IE=edge'>")
            sb.AppendLine("<meta charset='UTF-8'>")
            sb.AppendLine("<meta name='viewport' content='width=device-width, initial-scale=1.0'>")
            sb.AppendLine("<title>FAARFIELD CM Report — " & WebEncode(jobName) & "</title>")
            sb.AppendLine("<style>")
            sb.AppendLine(GetCss())
            sb.AppendLine("</style>")
            sb.AppendLine("</head>")
            sb.AppendLine("<body>")

            ' ===== Header =====
            sb.AppendLine("<header class='report-header'>")
            sb.AppendLine("<h1>FAA FAARFIELD CM Report — Computational Mechanics <button id='btn-print' class='btn-action'>Print Report</button></h1>")
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
                Dim cdfClass = If(Math.Abs(rpt.CDFSweep.MaxCDF - 1.0) < 0.05, "success", If(Math.Abs(rpt.CDFSweep.MaxCDF - 1.0) > 0.2, "danger", ""))
                AppendCard(sb, "Max Total CDF", Format(rpt.CDFSweep.MaxCDF, "0.000000"), "", cdfClass)
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
                Dim converged = lastIt.CDFErr < CDF.CDFExitErr
                Dim convStr = If(converged, "Yes", "No")
                AppendCard(sb, "Converged / Iterations", convStr & " / " & rpt.Iterations.Count.ToString(), "", If(converged, "success", "danger"))
            End If
            sb.AppendLine("</section>")

            ' ===== Pavement Response Summary (per-aircraft critical responses) =====
            ' Quick-reference table at the top of the report listing the LEAF responses that
            ' drive every downstream section: epsilon_22 (vertical strain at top of subgrade,
            ' from the subgrade VerticalStrain LEAF call), sigma_22 (vertical stress at top of
            ' subgrade, from an additional AllResponses LEAF call at the same EvalDepth), and
            ' epsilon_11 (tensile strain at the bottom of the AC layer, from the asphalt
            ' AllResponses LEAF call at the AC-bottom depth). For PCR runs prefer the
            ' Step-1 evaluation snapshot so the original mix is shown.
            Dim acDetailsForResponse() As clsAircraftDetail = rpt.AircraftDetails
            If rpt.EvaluationAircraftDetails IsNot Nothing AndAlso rpt.EvaluationAircraftDetails.Length > 1 Then
                acDetailsForResponse = rpt.EvaluationAircraftDetails
            End If
            If acDetailsForResponse IsNot Nothing Then
                sb.AppendLine("<section class='response-summary'>")
                sb.AppendLine("<h2>Pavement Response Summary</h2>")
                sb.AppendLine("<div class='callout info'><p>Per-aircraft critical responses computed by LEAF on the converged structure. " &
                    "These are the inputs used by every downstream section: " &
                    "&epsilon;<sub>22</sub> drives the Subgrade Damage Model (Section D); " &
                    "&epsilon;<sub>11</sub> drives the asphalt fatigue check (Section D.2); " &
                    "&sigma;<sub>22</sub> is included for full transparency of the subgrade stress state. " &
                    "Strain values reported in microstrain (1 &mu;&epsilon; = 10<sup>&minus;6</sup> in./in.); magnitudes (absolute values).</p></div>")
                sb.AppendLine("<table class='data-table centered'>")
                sb.AppendLine("<thead><tr>")
                sb.AppendLine("<th>Aircraft</th>")
                sb.AppendLine("<th>&epsilon;<sub>22</sub><br/><span style='font-weight:normal;font-size:11px'>Vertical Strain @ Top of Subgrade<br/>(&mu;&epsilon;)</span></th>")
                sb.AppendLine("<th>&sigma;<sub>22</sub><br/><span style='font-weight:normal;font-size:11px'>Vertical Stress @ Top of Subgrade<br/>(kPa)</span></th>")
                sb.AppendLine("<th>&epsilon;<sub>11</sub><br/><span style='font-weight:normal;font-size:11px'>Tensile Strain @ Bottom of AC<br/>(&mu;&epsilon;)</span></th>")
                sb.AppendLine("</tr></thead><tbody>")
                ' For PCR runs the engine iterates GL(IA) to find a round-MGW, so the standard
                ' det fields reflect the converged MGW load. UserInput* fields hold the responses
                ' captured at the user's typed gear load by a dedicated pre-PCR LEAF pass — those
                ' are what the user wants to see here.
                Dim usingUserInput As Boolean = False
                For ia As Integer = 1 To UBound(acDetailsForResponse)
                    If acDetailsForResponse(ia) IsNot Nothing AndAlso acDetailsForResponse(ia).HasUserInputResponses Then
                        usingUserInput = True : Exit For
                    End If
                Next

                For ia As Integer = 1 To UBound(acDetailsForResponse)
                    If acDetailsForResponse(ia) Is Nothing Then Continue For
                    Dim det = acDetailsForResponse(ia)
                    Dim e22Val As Double = If(det.HasUserInputResponses, det.UserInputVerticalStrain, det.VerticalStrain)
                    Dim s22Psi As Double = If(det.HasUserInputResponses, det.UserInputSubgradeStress, det.SubgradeVertStress)
                    Dim e11Val As Double = If(det.HasUserInputResponses, det.UserInputAsphaltStrain, det.AsphaltStrain)
                    Dim e22Str As String = If(e22Val > 0, Format(e22Val * 1000000, "0.00"), "&mdash;")
                    ' FAARFIELD internal stress unit is psi (US Customary). Convert to kPa for display
                    ' regardless of the UI's measurement system (1 psi = 6.89475729 kPa).
                    Dim s22Kpa As Double = s22Psi * 6.89475729
                    Dim s22Str As String = If(s22Psi > 0, Format(s22Kpa, "0.00"), "&mdash;")
                    Dim e11Str As String = If(e11Val > 0, Format(e11Val * 1000000, "0.00"), "&mdash;")
                    sb.AppendLine("<tr>")
                    sb.Append("<td>" & WebEncode(det.ACName) & "</td>")
                    sb.Append("<td>" & e22Str & "</td>")
                    sb.Append("<td>" & s22Str & "</td>")
                    sb.Append("<td>" & e11Str & "</td>")
                    sb.AppendLine("</tr>")
                Next
                sb.AppendLine("</tbody></table>")
                If usingUserInput Then
                    sb.AppendLine("<p class='fig-caption'><strong>PCR run:</strong> &epsilon;<sub>22</sub>, &sigma;<sub>22</sub>, &epsilon;<sub>11</sub> shown above are computed at the <strong>user-input gear load</strong> on the evaluation pavement (a dedicated LEAF pass before the PCR rounds run). " &
                        "&sigma;<sub>22</sub> converted from psi to kPa (&times; 6.89475729). The PCR rounds' converged Maximum Gross Weight (MGW) and per-round PCR are reported separately in Section K.</p>")
                Else
                    sb.AppendLine("<p class='fig-caption'>&epsilon;<sub>22</sub> is taken at the top of the subgrade (LEAF VerticalStrain). " &
                        "&sigma;<sub>22</sub> is the LEAF AllResponses StressZ at the same evaluation depth, converted from psi to kPa (&times; 6.89475729). " &
                        "&epsilon;<sub>11</sub> is the principal horizontal tensile strain at the bottom of the asphalt concrete layer " &
                        "(from the LEAF AllResponses call used for asphalt fatigue). A &mdash; means the response was not computed for this aircraft (e.g., asphalt CDF disabled).</p>")
                End If
                sb.AppendLine("</section>")
            End If

            ' ===== Table of Contents =====
            sb.AppendLine("<nav class='toc' id='toc'>")
            sb.AppendLine("<h2>Table of Contents</h2>")
            sb.AppendLine("<ol>")
            sb.AppendLine("<li><a href='#section-a'>Pavement Structure Summary</a></li>")
            sb.AppendLine("<li><a href='#section-b'>Design Equations</a></li>")
            sb.AppendLine("<li><a href='#section-c'>Understanding Coverage-to-Pass (C/P)</a></li>")
            sb.AppendLine("<li><a href='#section-d'>Subgrade Damage Model</a></li>")
            sb.AppendLine("<li><a href='#section-e'>Per-Aircraft Detailed Breakdown</a></li>")
            sb.AppendLine("<li><a href='#section-f'>Coverage-to-Pass (C/P) Distribution</a></li>")
            sb.AppendLine("<li><a href='#section-g'>CDF Sweep Table (" & (2 * (CDF.NOFF - 1) + 1).ToString() & " offsets, bilateral)</a></li>")
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

            ' A.3 Aggregate sublayer modulus explanation
            If rpt.SublayerData.HasAggregateSublayers Then
                AppendSublayerModulusSection(sb, rpt.SublayerData, thicknessUnit, pressureUnit)
            End If

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
                "wanders as a rigid body. FAARFIELD evaluates " & CDF.NOFF.ToString() & " strips on each side of the nominal " &
                "wheel path centerline (offsets 0 to " & Format((CDF.NOFF - 1) * CDF.OFFSETINC, "0") & " in.) for a total of " &
                (2 * (CDF.NOFF - 1) + 1).ToString() & " bilateral offsets.</p>")
            sb.AppendLine("</div>")

            ' C/P concept diagram (2-panel: Gaussian + single vs dual C/P curves)
            figNum += 1
            sb.AppendLine("<figure>")
            AppendCoverageConceptSVG(sb)
            sb.AppendLine("<figcaption>Figure " & figNum & ": Gaussian lateral wander and single vs. dual wheel C/P comparison</figcaption>")
            sb.AppendLine("</figure>")

            sb.AppendLine("</section>")

            ' ===== Section D: Subgrade Damage Model =====
            ' For PCR runs, AircraftDetails gets overwritten by each round's shrinking mix.
            ' EvaluationAircraftDetails snapshots the original-mix Step-1 results so this
            ' section reflects every aircraft and its evaluation-pavement subgrade strain.
            Dim acDetailsForFatigue() As clsAircraftDetail = rpt.AircraftDetails
            If rpt.EvaluationAircraftDetails IsNot Nothing AndAlso rpt.EvaluationAircraftDetails.Length > 1 Then
                acDetailsForFatigue = rpt.EvaluationAircraftDetails
            End If
            If acDetailsForFatigue IsNot Nothing Then
                sb.AppendLine("<section id='section-d'>")
                sb.AppendLine("<h2><span class='sec-num'>D</span> Subgrade Damage Model</h2>")
                sb.AppendLine("<div class='callout info'><p>The chart below shows the subgrade damage model curve (allowable repetitions vs. vertical compressive strain at the top of the subgrade) " &
                    "with each aircraft's computed strain and N<sub>fail</sub> plotted as scatter points.</p></div>")

                ' SVG Damage Curve
                figNum += 1
                sb.AppendLine("<figure>")
                AppendFatigueCurveSVG(sb, rpt, subgradeMod, acDetailsForFatigue)
                sb.AppendLine("<figcaption>Figure " & figNum & ": Subgrade damage model with aircraft scatter points</figcaption>")
                sb.AppendLine("</figure>")

                ' Aircraft loading-response parameters table
                sb.AppendLine("<h3>Aircraft Loading Response Parameters</h3>")
                sb.AppendLine("<table class='data-table centered'><thead><tr>")
                sb.AppendLine("<th>Aircraft</th><th>Vert. Strain &epsilon;<sub>22</sub> (&mu;&epsilon;)</th><th>AA</th><th>BB</th><th>N<sub>fail</sub></th><th>Repetitions</th><th>N<sub>fail</sub>/Reps</th><th>Model</th>")
                sb.AppendLine("</tr></thead><tbody>")
                For ia As Integer = 1 To UBound(acDetailsForFatigue)
                    If acDetailsForFatigue(ia) Is Nothing Then Continue For
                    Dim det = acDetailsForFatigue(ia)
                    Dim displayedN As Double = NtoFailForDisplay(det)
                    Dim ratio As Double = If(det.TotalRepetitions > 0, displayedN / det.TotalRepetitions, 0)
                    sb.AppendLine("<tr>")
                    sb.Append("<td>" & WebEncode(det.ACName) & "</td>")
                    sb.Append("<td>" & Format(det.VerticalStrain * 1000000, "0.00") & "</td>")
                    sb.Append("<td>" & Format(det.NtoFailAA, "0.000000") & "</td>")
                    sb.Append("<td>" & Format(det.NtoFailBB, "0.000") & "</td>")
                    sb.Append("<td>" & Format(displayedN, "0.000E+00") & "</td>")
                    sb.Append("<td>" & Format(det.TotalRepetitions, "#,##0") & "</td>")
                    sb.Append("<td>" & Format(ratio, "0.00E+00") & "</td>")
                    sb.Append("<td>" & WebEncode(det.SubgradeModelUsed) & "</td>")
                    sb.AppendLine("</tr>")
                Next
                sb.AppendLine("</tbody></table>")

                ' SVG Life Ratio Chart
                figNum += 1
                sb.AppendLine("<figure>")
                AppendLifeRatioSVG(sb, rpt, acDetailsForFatigue)
                sb.AppendLine("<figcaption>Figure " & figNum & ": Fatigue life reserve ratio per aircraft</figcaption>")
                sb.AppendLine("</figure>")

                sb.AppendLine("<div class='callout info'>")
                sb.AppendLine("<p><strong>How to read this chart:</strong> The ratio N<sub>fail</sub>/Repetitions compares the " &
                    "pavement's fatigue capacity at the computed strain level to the total number of aircraft departures. " &
                    "A value less than 1.0 (red bar, extending left) means the pavement cannot survive all repetitions " &
                    "<em>if every pass loaded the exact same point</em>. This does <strong>not</strong> indicate failure.</p>")
                sb.AppendLine("<p>In reality, aircraft wander laterally (&sigma; = 30.435 in.), so only a fraction of passes " &
                    "load any given strip. This fraction is the <strong>Coverage-to-Pass (C/P)</strong> ratio. " &
                    "The actual damage is:</p>")
                sb.AppendLine("<p style='text-align:center;font-family:Cambria Math,serif;font-size:15px;margin:8px 0'>" &
                    "CDF = Repetitions &times; C/P / N<sub>fail</sub></p>")
                sb.AppendLine("<p>For a converged design (CDF &asymp; 1.0), rearranging gives C/P &asymp; N<sub>fail</sub>/Repetitions. " &
                    "This is why the two quantities appear numerically similar &mdash; but they measure different things:</p>")
                sb.AppendLine("<ul style='margin:6px 0 0 20px;font-size:13px'>")
                sb.AppendLine("<li><strong>N<sub>fail</sub>/Reps</strong> &mdash; fatigue capacity vs. demand (depends on strain, " &
                    "subgrade modulus, traffic volume)</li>")
                sb.AppendLine("<li><strong>C/P</strong> &mdash; geometric load probability (depends on tire width, " &
                    "wander &sigma;, lateral offset)</li>")
                sb.AppendLine("</ul>")
                sb.AppendLine("<p style='margin-top:8px'>A green bar (ratio &gt; 1.0) means the aircraft has fatigue life in reserve " &
                    "even without wander &mdash; it is not the critical aircraft driving the design.</p>")
                sb.AppendLine("</div>")

                ' D.2 Asphalt (HMA) Fatigue Characterization
                If rpt.AsphaltCDFComputed Then
                    sb.AppendLine("<div class='asphalt-fatigue-section'>")
                    sb.AppendLine("<h3>D.2 Asphalt (HMA) Layer Fatigue</h3>")

                    sb.AppendLine("<div class='callout info'><p>In addition to subgrade rutting, FAARFIELD evaluates fatigue cracking of the " &
                        "hot-mix asphalt (HMA) surface layer. The governing failure mode for design is typically subgrade rutting " &
                        "(CDF<sub>subgrade</sub> &rarr; 1.0), but the asphalt CDF is computed in parallel using " &
                        "the horizontal tensile strain at the bottom of the HMA layer.</p></div>")

                    If rpt.AsphaltModel = "RDEC" Then
                        ' RDEC equation card
                        sb.AppendLine("<div class='equation-card rdec-model'>")
                        sb.AppendLine("<h4>RDEC Asphalt Fatigue Model</h4>")
                        sb.AppendLine("<div class='eq'>PV = 44.422 &times; &epsilon;<sup>5.14</sup> &times; (E &times; 0.0068948)<sup>2.993</sup> &times; VP<sup>1.85</sup> &times; GP<sup>&minus;0.4063</sup></div>")
                        sb.AppendLine("<div class='eq'>N<sub>fail</sub> = 0.4801 &times; PV<sup>&minus;0.90074</sup></div>")
                        sb.AppendLine("<div class='eq-note'>")
                        sb.AppendLine("<strong>&epsilon;</strong> = horizontal tensile strain at bottom of HMA<br/>")
                        sb.AppendLine("<strong>E</strong> = flexural modulus (psi &times; 0.0068948 &rarr; MPa)<br/>")
                        sb.AppendLine("<strong>VP</strong> = V<sub>a</sub> / (V<sub>a</sub> + V<sub>b</sub>) &nbsp;(void parameter)<br/>")
                        sb.AppendLine("<strong>GP</strong> = (PNMS &minus; PPCS) / P200 &nbsp;(gradation parameter)")
                        sb.AppendLine("</div></div>")

                        ' RDEC mix parameters table
                        sb.AppendLine("<h4>RDEC Mix Parameters</h4>")
                        sb.AppendLine("<table class='data-table param-table'><thead><tr>")
                        sb.AppendLine("<th>Parameter</th><th>Symbol</th><th>Value</th><th>Description</th>")
                        sb.AppendLine("</tr></thead><tbody>")
                        AppendParamRow4(sb, "Flexural Modulus", "E", Format(rpt.RdecFlexuralMod, "#,##0") & " psi", "HMA flexural stiffness at design conditions")
                        AppendParamRow4(sb, "Air Voids", "V<sub>a</sub>", Format(rpt.RdecAirVoids, "0.0") & " %", "Percent air voids in HMA mix")
                        AppendParamRow4(sb, "Asphalt Content", "V<sub>b</sub>", Format(rpt.RdecAsphaltContent, "0.0") & " %", "Asphalt content by volume")
                        AppendParamRow4(sb, "Void Parameter", "VP", Format(rpt.RdecVoidParameter, "0.0000"), "V<sub>a</sub> / (V<sub>a</sub> + V<sub>b</sub>)")
                        AppendParamRow4(sb, "Nom. Max Sieve Passing", "PNMS", Format(rpt.RdecPNMS, "0.0") & " %", "Percent passing nominal maximum sieve")
                        AppendParamRow4(sb, "Primary Control Sieve", "PPCS", Format(rpt.RdecPPCS, "0.0") & " %", "Percent passing primary control sieve")
                        AppendParamRow4(sb, "P-200 Fraction", "P200", Format(rpt.RdecP200, "0.0") & " %", "Percent passing #200 sieve")
                        AppendParamRow4(sb, "Gradation Parameter", "GP", Format(rpt.RdecGradationParameter, "0.000"), "(PNMS &minus; PPCS) / P200")
                        sb.AppendLine("</tbody></table>")
                    Else
                        ' AI equation card
                        sb.AppendLine("<div class='equation-card ai-model'>")
                        sb.AppendLine("<h4>Asphalt Institute (AI) Fatigue Model</h4>")
                        sb.AppendLine("<div class='eq'>AA = 2.68 &minus; 5.0 &times; log<sub>10</sub>(&epsilon;)</div>")
                        sb.AppendLine("<div class='eq'>BB = 2.665 &times; log<sub>10</sub>(E<sub>asp</sub>)</div>")
                        sb.AppendLine("<div class='eq'>N<sub>fail</sub> = 10<sup>(AA &minus; BB)</sup></div>")
                        sb.AppendLine("<div class='eq-note'>&epsilon; = horizontal tensile strain at bottom of HMA<br/>E<sub>asp</sub> = asphalt surface modulus (psi)</div>")
                        sb.AppendLine("</div>")
                    End If

                    ' Per-aircraft asphalt CDF table
                    sb.AppendLine("<h4>Asphalt CDF Per Aircraft</h4>")
                    sb.AppendLine("<table class='data-table'><thead><tr>")
                    sb.AppendLine("<th>Aircraft</th><th>HMA Strain (&mu;&epsilon;)</th><th>N<sub>fail,HMA</sub></th><th>Repetitions</th><th>CDF<sub>HMA</sub></th><th>CDF<sub>Subgrade</sub></th><th>Governing</th>")
                    sb.AppendLine("</tr></thead><tbody>")
                    For ia As Integer = 1 To UBound(rpt.AircraftDetails)
                        If rpt.AircraftDetails(ia) Is Nothing Then Continue For
                        Dim det = rpt.AircraftDetails(ia)
                        Dim governing As String = "&mdash;"
                        If det.AsphaltCDF > 0 AndAlso det.MaxCDF > 0 Then
                            governing = If(det.MaxCDF >= det.AsphaltCDF, "<span class='badge-subgrade'>Subgrade</span>", "<span class='badge-asphalt'>Asphalt</span>")
                        End If
                        sb.AppendLine("<tr>")
                        sb.Append("<td>" & WebEncode(det.ACName) & "</td>")
                        sb.Append("<td>" & If(det.AsphaltStrain > 0, Format(det.AsphaltStrain * 1000000, "0.00"), "&mdash;") & "</td>")
                        sb.Append("<td>" & If(det.AsphaltNtoFail > 0, Format(det.AsphaltNtoFail, "0.000E+00"), "&mdash;") & "</td>")
                        sb.Append("<td>" & Format(det.TotalRepetitions, "#,##0") & "</td>")
                        sb.Append("<td>" & If(det.AsphaltCDF > 0, Format(det.AsphaltCDF, "0.000E+00"), "&mdash;") & "</td>")
                        sb.Append("<td>" & Format(det.MaxCDF, "0.000000") & "</td>")
                        sb.Append("<td>" & governing & "</td>")
                        sb.AppendLine("</tr>")
                    Next
                    sb.AppendLine("</tbody></table>")

                    ' Asphalt CDF summary comparison
                    Dim govLabel As String = If(rpt.CDFSweep.MaxCDF >= rpt.AsphaltCDFTotal, "Subgrade Rutting", "Asphalt Fatigue")
                    sb.AppendLine("<div class='cdf-comparison'>")
                    sb.AppendLine("<div class='cdf-compare-card subgrade-card'>")
                    sb.AppendLine("<div class='cdf-compare-label'>CDF<sub>Subgrade</sub></div>")
                    sb.AppendLine("<div class='cdf-compare-value'>" & Format(rpt.CDFSweep.MaxCDF, "0.000000") & "</div>")
                    sb.AppendLine("</div>")
                    sb.AppendLine("<div class='cdf-compare-vs'>vs</div>")
                    sb.AppendLine("<div class='cdf-compare-card asphalt-card'>")
                    sb.AppendLine("<div class='cdf-compare-label'>CDF<sub>Asphalt</sub></div>")
                    sb.AppendLine("<div class='cdf-compare-value'>" & Format(rpt.AsphaltCDFTotal, "0.000000") & "</div>")
                    sb.AppendLine("</div>")
                    sb.AppendLine("<div class='cdf-compare-governing'>Governing: <strong>" & govLabel & "</strong></div>")
                    sb.AppendLine("</div>")

                    sb.AppendLine("<div class='callout note'><p>FAARFIELD uses the governing failure mode (typically subgrade rutting) " &
                        "for thickness design convergence (CDF &rarr; 1.0). The asphalt CDF is computed in parallel " &
                        "but does not directly control the design thickness unless it exceeds the subgrade CDF. " &
                        "Monitoring the asphalt CDF is valuable for evaluating HMA layer fatigue life under different traffic mixes.</p></div>")

                    sb.AppendLine("</div>") ' close asphalt-fatigue-section
                End If

                sb.AppendLine("</section>")
            End If

            ' ===== Section E: Per-Aircraft Breakdown =====
            ' Prefer EvaluationAircraftDetails (Step-1 PCNLifeCalc snapshot of the original mix)
            ' so PCR runs show every field at the USER-INPUT gear load. AircraftDetails is
            ' overwritten by each PCR round's MGW iteration — reading from there would label
            ' the round's converged MGW as "Gear Load", which is misleading.
            Dim acDetailsForE() As clsAircraftDetail = rpt.AircraftDetails
            Dim isEvalSource As Boolean = False
            If rpt.EvaluationAircraftDetails IsNot Nothing AndAlso rpt.EvaluationAircraftDetails.Length > 1 Then
                acDetailsForE = rpt.EvaluationAircraftDetails
                isEvalSource = True
            End If
            If acDetailsForE IsNot Nothing Then
                sb.AppendLine("<section id='section-e'>")
                sb.AppendLine("<h2><span class='sec-num'>E</span> Per-Aircraft Detailed Breakdown</h2>")
                ' Detect whether ANY aircraft in this view has a user-input snapshot. If yes,
                ' the table will read GearLoad / ε22 / σ22 / ε11 from the UserInput* fields.
                Dim eUsesUserInput As Boolean = False
                For ia As Integer = 1 To UBound(acDetailsForE)
                    If acDetailsForE(ia) IsNot Nothing AndAlso acDetailsForE(ia).HasUserInputResponses Then
                        eUsesUserInput = True : Exit For
                    End If
                Next
                If eUsesUserInput AndAlso rpt.PCRRounds IsNot Nothing AndAlso rpt.PCRRounds.Count > 0 Then
                    sb.AppendLine("<div class='callout info'><p><strong>Data source (PCR run):</strong> Gear Load and the &epsilon;<sub>22</sub>, &sigma;<sub>22</sub>, &epsilon;<sub>11</sub> responses below are computed at the <strong>user-input gear load</strong> (Gross Taxi Weight &times; main-gear distribution). A dedicated LEAF pass before the PCR rounds run captures these values; the PCR engine itself is not modified. The PCR rounds' converged Maximum Gross Weight (MGW) and per-round PCR appear separately in Section K.</p></div>")
                ElseIf isEvalSource AndAlso rpt.PCRRounds IsNot Nothing AndAlso rpt.PCRRounds.Count > 0 Then
                    sb.AppendLine("<div class='callout info'><p><strong>Data source:</strong> values shown are computed at the <strong>user-input gear load</strong> on the evaluation pavement (PCR Step-1 pass). The PCR rounds' converged Maximum Gross Weight (MGW) and per-round PCR are reported separately in Section K.</p></div>")
                End If

                For ia As Integer = 1 To UBound(acDetailsForE)
                    If acDetailsForE(ia) Is Nothing Then Continue For
                    Dim det = acDetailsForE(ia)
                    ' Per-aircraft user-input switch: the helpers below pull from UserInput* fields
                    ' when populated; otherwise they fall through to the standard det.* fields used
                    ' for non-PCR runs.
                    Dim displayedGearLoad As Single = If(det.HasUserInputResponses AndAlso det.UserInputGrossLoad > 0, det.UserInputGrossLoad, det.GrossLoad)
                    Dim displayedE22 As Double = If(det.HasUserInputResponses AndAlso det.UserInputVerticalStrain > 0, det.UserInputVerticalStrain, det.VerticalStrain)

                    sb.AppendLine("<div class='aircraft-block'>")
                    sb.AppendLine("<h3>Aircraft " & ia.ToString() & ": " & WebEncode(det.ACName) & "</h3>")

                    ' Gear parameters table
                    sb.AppendLine("<table class='data-table param-table'><thead><tr><th>Parameter</th><th>Value</th><th>Description</th></tr></thead><tbody>")
                    AppendParamRow(sb, "Gear Type", det.GearType, "Landing gear configuration")
                    AppendParamRow(sb, "Gear Load", Format(displayedGearLoad, "#,##0") & " " & weightUnit, If(det.HasUserInputResponses, "User-input gear load (GTW &times; main-gear distribution)", "Maximum gear load applied to pavement"))
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
                    AppendParamRow(sb, "Max Vertical Strain", Format(displayedE22 * 1000000, "0.00") & " &mu;&epsilon;", If(det.HasUserInputResponses, "&epsilon;<sub>22</sub> at user-input gear load", "LEAF-computed subgrade strain"))
                    If det.HorizontalStrain <> 0 Then
                        AppendParamRow(sb, "Horizontal Strain", Format(det.HorizontalStrain * 1000000, "0.00") & " &mu;&epsilon;", "LEAF-computed horizontal strain")
                    End If
                    AppendParamRow(sb, "N<sub>fail</sub>", Format(NtoFailForDisplay(det), "0.000E+00"), "Allowable repetitions (" & WebEncode(det.SubgradeModelUsed) & ")")
                    AppendParamRow(sb, "Max C/P Ratio", Format(det.MaxCtoP, "0.00000"), "Peak coverage-to-pass ratio")
                    If det.GearAdjusted Then
                        AppendParamRow(sb, "C/P Before Gear Adj.", Format(det.CtoPBeforeGearAdj, "0.00000"), "Before multi-gear adjustment")
                        AppendParamRow(sb, "C/P After Gear Adj.", Format(det.CtoPAfterGearAdj, "0.00000"), "After multi-gear adjustment")
                    End If
                    AppendParamRow(sb, "Max CDF (this aircraft)", Format(det.MaxCDF, "0.000000"), "Peak damage contribution")
                    AppendParamRow(sb, "CDF at Critical Offset", Format(det.CDFAtCriticalOffset, "0.000000"), "Damage at critical strip")
                    sb.AppendLine("</tbody></table>")

                    ' Gear configuration SVG
                    If det.NWheels > 0 AndAlso det.WheelX IsNot Nothing Then
                        figNum += 1
                        sb.AppendLine("<figure>")
                        AppendGearConfigSVG(sb, det, rpt.CDFSweep.MaxCDFOffset, lengthUnit)
                        sb.AppendLine("<figcaption>Figure " & figNum & ": Gear configuration for " & WebEncode(det.ACName) & "</figcaption>")
                        sb.AppendLine("</figure>")
                    End If

                    ' Pavement cross-section SVG
                    figNum += 1
                    sb.AppendLine("<figure>")
                    AppendPavementCrossSectionSVG(sb, rpt, det, thicknessUnit, lengthUnit)
                    sb.AppendLine("<figcaption>Figure " & figNum & ": Pavement cross-section for " & WebEncode(det.ACName) & "</figcaption>")
                    sb.AppendLine("</figure>")

                    ' Per-aircraft CDF by offset SVG
                    If rpt.CDFSweep.NAircraftCaptured > 0 Then
                        Dim acColor = ChartColors((ia - 1) Mod ChartColors.Length)
                        figNum += 1
                        sb.AppendLine("<figure>")
                        AppendSingleAircraftCDFSvg(sb, det, rpt.CDFSweep.MaxCDFOffset, acColor, lengthUnit)
                        sb.AppendLine("<figcaption>Figure " & figNum & ": CDF by offset for " & WebEncode(det.ACName) & "</figcaption>")
                        sb.AppendLine("</figure>")
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
                    sb.AppendLine("<li>Apply gear load of " & Format(displayedGearLoad, "#,##0") & " " & weightUnit &
                        " with tire pressure " & Format(det.TirePressure, "0.0") & " " & pressureUnit &
                        " and tire contact width " & Format(det.TireWidth, "0.00") & " " & lengthUnit &
                        " (" & WebEncode(det.GearType) & " gear).</li>")
                    sb.AppendLine("<li>Run LEAF to compute vertical subgrade strain. Result: &epsilon;<sub>v</sub> = " &
                        Format(det.VerticalStrain * 1000000, "0.00") & " &mu;&epsilon; at evaluation depth " &
                        Format(rpt.SublayerData.EvalDepthSubgrade, "0.00") & " " & thicknessUnit & ".</li>")
                    sb.AppendLine("<li>Compute N<sub>fail</sub> using " & WebEncode(det.SubgradeModelUsed) &
                        " model: AA=" & Format(det.NtoFailAA, "0.000000") & ", BB=" & Format(det.NtoFailBB, "0.000") &
                        ". N<sub>fail</sub> = " & Format(NtoFailForDisplay(det), "0.000E+00") & ".</li>")
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
                figNum += 1
                sb.AppendLine("<figure>")
                AppendCoveragePlotSVG(sb, rpt, lengthUnit)
                sb.AppendLine("<figcaption>Figure " & figNum & ": C/P ratio distribution across pavement width</figcaption>")
                sb.AppendLine("</figure>")
                sb.AppendLine("</section>")
            End If

            ' ===== Section G: CDF Sweep Table (bilateral, 81 offsets) =====
            If rpt.CDFSweep.NAircraftCaptured > 0 Then
                Dim nBilateral As Integer = 2 * (CDF.NOFF - 1) + 1
                Dim xMinVal As Double = -CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)

                sb.AppendLine("<section id='section-g'>")
                sb.AppendLine("<h2><span class='sec-num'>G</span> CDF Sweep Table (" & nBilateral.ToString() & " offsets, bilateral)</h2>")

                sb.AppendLine("<div class='table-scroll'><table class='data-table compact'><thead><tr>")
                sb.Append("<th>Offset (" & lengthUnit & ")</th>")
                For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                    Dim acName = "AC" & ia.ToString()
                    If rpt.AircraftDetails IsNot Nothing AndAlso ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                        acName = rpt.AircraftDetails(ia).ACName
                    End If
                    Dim shortName = If(acName.Length > 20, acName.Substring(0, 18) & "...", acName)
                    sb.Append("<th title='" & WebEncode(acName) & "'>" & WebEncode(shortName) & " C/P</th><th title='" & WebEncode(acName) & "'>" & WebEncode(shortName) & " CDF</th>")
                Next
                sb.AppendLine("<th>Total CDF</th></tr></thead><tbody>")

                For ib As Integer = 0 To nBilateral - 1
                    Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                    Dim absIdx As Integer = Math.Abs(ib - (CDF.NOFF - 1)) + 1
                    Dim cls = If(absIdx = rpt.CDFSweep.MaxCDFOffset, " class='highlight'", "")
                    sb.Append("<tr" & cls & "><td>" & Format(offVal, "0") & "</td>")
                    For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                        sb.Append("<td>" & Format(rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, absIdx), "0.00000") & "</td>")
                        sb.Append("<td>" & Format(rpt.CDFSweep.CDFPerAircraftPerOffset(ia, absIdx), "0.000000") & "</td>")
                    Next
                    sb.AppendLine("<td><strong>" & Format(rpt.CDFSweep.CDFTotalPerOffset(absIdx), "0.000000") & "</strong></td></tr>")
                Next
                sb.AppendLine("</tbody></table></div>")

                Dim criticalOffset As Double = (rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC
                sb.AppendLine("<div class='callout info'>Critical offset at &plusmn;" & Format(criticalOffset, "0") & " " & lengthUnit &
                    ", Max CDF = " & Format(rpt.CDFSweep.MaxCDF, "0.000000") & "</div>")
                sb.AppendLine("</section>")
            End If

            ' ===== Section H: CDF Distribution =====
            If rpt.CDFSweep.NAircraftCaptured > 0 AndAlso rpt.AircraftDetails IsNot Nothing Then
                sb.AppendLine("<section id='section-h'>")
                sb.AppendLine("<h2><span class='sec-num'>H</span> CDF Distribution Across Pavement Width</h2>")

                figNum += 1
                sb.AppendLine("<figure>")
                AppendCompositeCDFSvg(sb, rpt, lengthUnit)
                sb.AppendLine("<figcaption>Figure " & figNum & ": Composite CDF distribution across pavement width</figcaption>")
                sb.AppendLine("</figure>")

                ' CDF contribution bar chart
                figNum += 1
                sb.AppendLine("<figure>")
                AppendCDFContributionSVG(sb, rpt)
                sb.AppendLine("<figcaption>Figure " & figNum & ": CDF contribution per aircraft at critical offset</figcaption>")
                sb.AppendLine("</figure>")

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
                    figNum += 1
                    sb.AppendLine("<figure>")
                    AppendConvergenceSVG(sb, rpt, thicknessUnit)
                    sb.AppendLine("<figcaption>Figure " & figNum & ": Newton-Raphson convergence history</figcaption>")
                    sb.AppendLine("</figure>")
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

                figNum += 1
                sb.AppendLine("<figure>")
                AppendACRDamageSVG(sb, rpt)
                sb.AppendLine("<figcaption>Figure " & figNum & ": ACR vs. CDF per departure bubble chart</figcaption>")
                sb.AppendLine("</figure>")

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
            sb.AppendLine("<p>Generated by " & WebEncode(appTitle) & " &mdash; Federal Aviation Administration</p>")
            sb.AppendLine("<p>" & DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "</p>")
            sb.AppendLine("<p><a href='#toc'>&uarr; Back to Table of Contents</a></p>")
            sb.AppendLine("</footer>")

            ' Back-to-top button
            sb.AppendLine("<button id='btn-top' class='btn-top'>&uarr;</button>")

            ' JavaScript
            sb.AppendLine("<script>")
            sb.AppendLine(GetScript())
            sb.AppendLine("</script>")

            sb.AppendLine("</body></html>")
            Return sb.ToString()
        End Function

#Region "Helper Methods"

        Private Shared Sub AppendCard(sb As StringBuilder, label As String, value As String, unit As String, Optional extraClass As String = "")
            Dim cls = "card" & If(extraClass <> "", " " & extraClass, "")
            sb.Append("<div class='" & cls & "'><div class='card-label'>" & label & "</div>")
            sb.Append("<div class='card-value'>" & value)
            If unit <> "" Then sb.Append(" <span class='card-unit'>" & unit & "</span>")
            sb.AppendLine("</div></div>")
        End Sub

        Private Shared Sub AppendParamRow(sb As StringBuilder, param As String, value As String, desc As String)
            sb.AppendLine("<tr><td>" & param & "</td><td>" & value & "</td><td>" & desc & "</td></tr>")
        End Sub

        Private Shared Sub AppendParamRow4(sb As StringBuilder, param As String, symbol As String, value As String, desc As String)
            sb.AppendLine("<tr><td>" & param & "</td><td>" & symbol & "</td><td>" & value & "</td><td>" & desc & "</td></tr>")
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

        ''' <summary>
        ''' Computes N_fail (allowable repetitions) from the captured vertical strain and model
        ''' name. Mirrors the formulas in modCDF.LeafCDFFlex so the Aircraft Loading Response
        ''' Parameters table and the Bleasdale scatter overlay can show a value even when the
        ''' analysis-side gNtoFail() global was not populated (the non-tandem subgrade branch in
        ''' modCDF.vb does not assign gNtoFail, so capturing det.NtoFail = gNtoFail(IA) yields 0
        ''' for those aircraft). When det.NtoFail is already a positive value (tandem path) it
        ''' should be used directly; this helper is the report-side fallback.
        ''' </summary>
        Private Shared Function ComputeNtoFailForReport(verticalStrain As Double, model As String, aa As Double, bb As Double) As Double
            If verticalStrain <= 0 Then Return 0
            Dim eps As Double = verticalStrain
            If String.Equals(model, "Bleasdale", StringComparison.OrdinalIgnoreCase) Then
                ' Match modCDF.vb: floor strain at 0.001 (1,000 με) before applying the formula.
                If eps < 0.001 Then eps = 0.001
                Const a11 As Double = -0.163768916705
                Const b11 As Double = 185.192806802
                Const c11 As Double = 1.65054449461
                If eps <= 0.001765093 Then
                    Dim inner As Double = a11 + b11 * eps
                    If inner <= 0 Then Return 1.0E+15
                    Dim expn As Double = inner ^ (-1.0 / c11)
                    If expn > 15 Then Return 1.0E+15
                    Return 10.0 ^ expn
                Else
                    Return (0.00414131183 / eps) ^ 8.1
                End If
            ElseIf String.Equals(model, "Straight-Line", StringComparison.OrdinalIgnoreCase) Then
                ' Captured AA/BB are the active branch's parameters; the original-criterion AAorig
                ' is pre-multiplied by 10000^(1/BBorig) at capture time so a single (AA/ε)^BB
                ' formula reproduces both branches.
                If aa > 0 AndAlso bb > 0 Then Return (aa / eps) ^ bb
                Return 0
            Else
                ' Standard subgrade criterion: N = 10000 * (AA/ε)^BB
                If aa > 0 AndAlso bb > 0 Then Return 10000.0 * (aa / eps) ^ bb
                Return 0
            End If
        End Function

        ''' <summary>
        ''' Returns det.NtoFail when it is a positive value (analysis populated it via the tandem
        ''' branch). Otherwise computes the value from captured strain and model.
        ''' </summary>
        Private Shared Function NtoFailForDisplay(det As clsAircraftDetail) As Double
            If det Is Nothing Then Return 0
            If det.NtoFail > 0 Then Return det.NtoFail
            Return ComputeNtoFailForReport(det.VerticalStrain, det.SubgradeModelUsed, det.NtoFailAA, det.NtoFailBB)
        End Function

        Private Shared Function WebEncode(s As String) As String
            If s Is Nothing Then Return ""
            Return s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("""", "&quot;")
        End Function

#End Region

#Region "Sublayer Modulus Explanation"

        Private Shared Sub AppendSublayerModulusSection(sb As StringBuilder, sld As clsSublayerData, thkUnit As String, presUnit As String)
            sb.AppendLine("<div class='sublayer-modulus-section'>")
            sb.AppendLine("<h3>Unbound Aggregate Modulus &mdash; Sublayering Procedure</h3>")

            ' Explanation
            sb.AppendLine("<div class='callout note'>")
            sb.AppendLine("<p>Unbound aggregate layers (crushed base and uncrushed subbase) do not have a single " &
                "fixed modulus. FAARFIELD subdivides each aggregate layer into sublayers and computes a " &
                "depth-dependent modulus for each using an empirical formula. The modulus of each sublayer " &
                "depends on the modulus of the material below it &mdash; sublayers near the bottom (close to the subgrade) " &
                "have lower moduli, while sublayers near the top have higher moduli. The computation proceeds from " &
                "the bottom of the aggregate layer upward.</p>")
            sb.AppendLine("</div>")

            ' Formula card
            sb.AppendLine("<div class='equation-card'>")
            sb.AppendLine("<h4>Sublayer Modulus Reduction Formula</h4>")
            sb.AppendLine("<div class='eq'>f<sub>1</sub> = 1 + C &times; ln(t) / ln(10)</div>")
            sb.AppendLine("<div class='eq'>f<sub>2</sub> = D &times; ln(E<sub>i&minus;1</sub>) &times; ln(t) / ln&sup2;(10)</div>")
            sb.AppendLine("<div class='eq sublayer-main-eq'>E<sub>i</sub> = E<sub>i&minus;1</sub> &times; (f<sub>1</sub> &minus; f<sub>2</sub>)</div>")
            sb.AppendLine("<div class='eq-note'>where <var>t</var> = sublayer thickness (" & thkUnit & "), " &
                "<var>E<sub>i&minus;1</sub></var> = modulus of the layer below (" & presUnit & "). " &
                "Applied iteratively from the bottom sublayer upward.</div>")
            sb.AppendLine("</div>")

            ' Base sublayer detail table — full input trace per row
            If sld.BaseSublayers.Count > 0 Then
                Dim baseTotalThk As Single = 0
                For Each x In sld.BaseSublayers : baseTotalThk += x.Thickness : Next
                sb.AppendLine("<h4>P-209 Crushed Aggregate Base &mdash; Sublayer Moduli " &
                    "(" & sld.BaseSublayerCount.ToString() & " sublayers, total " &
                    Format(baseTotalThk, "0.00") & " " & thkUnit & ")</h4>")
                sb.AppendLine("<table class='data-table sublayer-detail'><thead><tr>" &
                    "<th>Sublayer</th>" &
                    "<th>Thickness (" & thkUnit & ")</th>" &
                    "<th>C</th><th>D</th>" &
                    "<th>f<sub>1</sub></th><th>f<sub>2</sub></th>" &
                    "<th>E<sub>i&minus;1</sub> (" & presUnit & ")</th>" &
                    "<th>Modulus (" & presUnit & ")</th>" &
                    "</tr></thead><tbody>")
                Dim anyBaseInterp As Boolean = False
                For si As Integer = 0 To sld.BaseSublayers.Count - 1
                    Dim bsl = sld.BaseSublayers(si)
                    Dim posLabel As String = If(si = 0, " (top)", If(si = sld.BaseSublayers.Count - 1, " (bottom)", ""))
                    Dim flag As String = If(bsl.IsBoundaryInterpolated, " *", "")
                    If bsl.IsBoundaryInterpolated Then anyBaseInterp = True
                    sb.AppendLine("<tr><td>" & (si + 1).ToString() & posLabel & flag & "</td>" &
                        "<td>" & Format(bsl.Thickness, "0.00") & "</td>" &
                        "<td>" & Format(sld.BaseCoeffC, "0.00") & "</td>" &
                        "<td>" & Format(sld.BaseCoeffD, "0.00") & "</td>" &
                        "<td>" & Format(bsl.F1, "0.0000") & "</td>" &
                        "<td>" & Format(bsl.F2, "0.0000") & "</td>" &
                        "<td>" & Format(bsl.ModBelow, "#,##0") & "</td>" &
                        "<td>" & Format(bsl.Modulus, "#,##0") & "</td></tr>")
                Next
                sb.AppendLine("<tr class='ref-row'><td>&darr; Layer below</td><td>&mdash;</td>" &
                    "<td>&mdash;</td><td>&mdash;</td><td>&mdash;</td><td>&mdash;</td><td>&mdash;</td>" &
                    "<td>" & Format(sld.BaseModUnder, "#,##0") & "</td></tr>")
                sb.AppendLine("</tbody></table>")
                If anyBaseInterp Then
                    sb.AppendLine("<p class='fig-caption'>* Top sublayer modulus is produced by linear " &
                        "interpolation between E<sub>below</sub> (the modulus of the underlying P-209 layer) " &
                        "and the formula value E<sub>i&minus;1</sub> &times; (f<sub>1</sub> &minus; f<sub>2</sub>) " &
                        "evaluated at the reference thickness (10 in. for P-209). The displayed f<sub>1</sub>, " &
                        "f<sub>2</sub>, and E<sub>i&minus;1</sub> are the values used in that reference-thickness " &
                        "evaluation (against the modulus of sublayer 2 prior to its own boundary correction). " &
                        "Modulus = E<sub>below</sub> + ((TS<sub>1</sub> &minus; 5)/5) &times; " &
                        "(E<sub>i&minus;1</sub>&times;(f<sub>1</sub>&minus;f<sub>2</sub>) &minus; E<sub>below</sub>). " &
                        "This refinement applies automatically when the top sublayer is thinner than the reference thickness.</p>")
                End If
            End If

            ' Subbase sublayer detail table — full input trace per row
            If sld.SubbaseSublayers.Count > 0 Then
                Dim sbTotalThk As Single = 0
                For Each x In sld.SubbaseSublayers : sbTotalThk += x.Thickness : Next
                sb.AppendLine("<h4>P-154 Uncrushed Aggregate Subbase &mdash; Sublayer Moduli " &
                    "(" & sld.SubbaseSublayerCount.ToString() & " sublayers, total " &
                    Format(sbTotalThk, "0.00") & " " & thkUnit & ")</h4>")
                sb.AppendLine("<table class='data-table sublayer-detail'><thead><tr>" &
                    "<th>Sublayer</th>" &
                    "<th>Thickness (" & thkUnit & ")</th>" &
                    "<th>C</th><th>D</th>" &
                    "<th>f<sub>1</sub></th><th>f<sub>2</sub></th>" &
                    "<th>E<sub>i&minus;1</sub> (" & presUnit & ")</th>" &
                    "<th>Modulus (" & presUnit & ")</th>" &
                    "</tr></thead><tbody>")
                Dim anySbInterp As Boolean = False
                For si As Integer = 0 To sld.SubbaseSublayers.Count - 1
                    Dim ssl = sld.SubbaseSublayers(si)
                    Dim posLabel As String = If(si = 0, " (top)", If(si = sld.SubbaseSublayers.Count - 1, " (bottom)", ""))
                    Dim flag As String = If(ssl.IsBoundaryInterpolated, " *", "")
                    If ssl.IsBoundaryInterpolated Then anySbInterp = True
                    sb.AppendLine("<tr><td>" & (si + 1).ToString() & posLabel & flag & "</td>" &
                        "<td>" & Format(ssl.Thickness, "0.00") & "</td>" &
                        "<td>" & Format(sld.SubbaseCoeffC, "0.00") & "</td>" &
                        "<td>" & Format(sld.SubbaseCoeffD, "0.00") & "</td>" &
                        "<td>" & Format(ssl.F1, "0.0000") & "</td>" &
                        "<td>" & Format(ssl.F2, "0.0000") & "</td>" &
                        "<td>" & Format(ssl.ModBelow, "#,##0") & "</td>" &
                        "<td>" & Format(ssl.Modulus, "#,##0") & "</td></tr>")
                Next
                sb.AppendLine("<tr class='ref-row'><td>&darr; Layer below</td><td>&mdash;</td>" &
                    "<td>&mdash;</td><td>&mdash;</td><td>&mdash;</td><td>&mdash;</td><td>&mdash;</td>" &
                    "<td>" & Format(sld.SubbaseModUnder, "#,##0") & "</td></tr>")
                sb.AppendLine("</tbody></table>")
                If anySbInterp Then
                    sb.AppendLine("<p class='fig-caption'>* Top sublayer modulus is produced by linear " &
                        "interpolation between E<sub>below</sub> (the modulus of the underlying P-154 layer) " &
                        "and the formula value E<sub>i&minus;1</sub> &times; (f<sub>1</sub> &minus; f<sub>2</sub>) " &
                        "evaluated at the reference thickness (8 in. for P-154). The displayed f<sub>1</sub>, " &
                        "f<sub>2</sub>, and E<sub>i&minus;1</sub> are the values used in that reference-thickness " &
                        "evaluation (against the modulus of sublayer 2 prior to its own boundary correction). " &
                        "Modulus = E<sub>below</sub> + ((TS<sub>1</sub> &minus; 4)/4) &times; " &
                        "(E<sub>i&minus;1</sub>&times;(f<sub>1</sub>&minus;f<sub>2</sub>) &minus; E<sub>below</sub>). " &
                        "This refinement applies automatically when the top sublayer is thinner than the reference thickness.</p>")
                End If
            End If

            ' SVG modulus-depth profile
            AppendModulusDepthSVG(sb, sld, thkUnit, presUnit)

            sb.AppendLine("<p class='fig-caption'>Modulus vs. depth profile for the expanded sublayer structure. " &
                "Aggregate layers are subdivided and their moduli computed bottom&rarr;up using the empirical reduction formula. " &
                "The teal step line traces the modulus at each sublayer depth.</p>")
            sb.AppendLine("</div>")
        End Sub


        Private Shared Sub AppendModulusDepthSVG(sb As StringBuilder, sld As clsSublayerData, thkUnit As String, presUnit As String)
            Dim allLayers = sld.ExpandedSublayers
            If allLayers.Count < 2 Then Exit Sub

            Dim svgW As Integer = 800
            Dim svgH As Integer = 480

            ' Compute depth and modulus ranges
            Dim depths As New List(Of Single)
            Dim cumD As Single = 0
            For i As Integer = 0 To allLayers.Count - 2
                depths.Add(cumD)
                cumD += allLayers(i).Thickness
            Next
            depths.Add(cumD)

            Dim maxDepth As Single = cumD * 1.15F
            If maxDepth < 10 Then maxDepth = 10

            Dim minMod As Single = Single.MaxValue
            Dim maxMod As Single = Single.MinValue
            For Each sl In allLayers
                If sl.Modulus > maxMod Then maxMod = sl.Modulus
                If sl.Modulus < minMod Then minMod = sl.Modulus
            Next
            Dim modPadMin As Single = minMod * 0.85F
            Dim modPadMax As Single = maxMod * 1.1F

            ' Layout
            Dim layerX As Single = 70
            Dim layerW As Single = 100
            Dim plotL As Single = 220
            Dim plotR As Single = 750
            Dim plotT As Single = 30
            Dim plotB As Single = 420
            Dim plotH As Single = plotB - plotT
            Dim plotW As Single = plotR - plotL

            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='modulus-depth-svg' xmlns='http://www.w3.org/2000/svg'>")

            ' Defs for gradient
            sb.AppendLine("<defs>")
            sb.AppendLine("<linearGradient id='agg-grad' x1='0' y1='0' x2='1' y2='0'>")
            sb.AppendLine("<stop offset='0%' stop-color='#00796B' stop-opacity='0.15'/>")
            sb.AppendLine("<stop offset='100%' stop-color='#00796B' stop-opacity='0.05'/>")
            sb.AppendLine("</linearGradient>")
            sb.AppendLine("</defs>")

            ' Grid lines for modulus axis
            Dim modStep As Single = CSng(Math.Pow(10, Math.Floor(Math.Log10(modPadMax - modPadMin))))
            If (modPadMax - modPadMin) / modStep < 3 Then modStep /= 2
            If (modPadMax - modPadMin) / modStep > 8 Then modStep *= 2
            Dim mt As Single = CSng(Math.Ceiling(modPadMin / modStep) * modStep)
            While mt <= modPadMax
                Dim x As Single = SvgModToX(mt, plotL, modPadMin, modPadMax, plotW)
                sb.AppendLine("<line x1='" & Fmt(x) & "' y1='" & Fmt(plotT) & "' x2='" & Fmt(x) & "' y2='" & Fmt(plotB) & "' stroke='#d0d4da' stroke-width='0.9'/>")
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & Fmt(plotB + 16) & "' text-anchor='middle' class='tick-label'>" & Format(mt, "#,##0") & "</text>")
                mt += modStep
            End While

            ' Depth grid lines at layer interfaces
            For Each d As Single In depths
                Dim y As Single = SvgDepthToY(d, plotT, maxDepth, plotH)
                sb.AppendLine("<line x1='" & Fmt(plotL) & "' y1='" & Fmt(y) & "' x2='" & Fmt(plotR) & "' y2='" & Fmt(y) & "' stroke='#d0d4da' stroke-width='0.9'/>")
                ' Connector line from layer column to chart
                sb.AppendLine("<line x1='" & Fmt(layerX + layerW) & "' y1='" & Fmt(y) & "' x2='" & Fmt(plotL) & "' y2='" & Fmt(y) & "' stroke='#ccc' stroke-width='0.4' stroke-dasharray='3,3'/>")
                ' Depth label
                sb.AppendLine("<text x='" & Fmt(layerX - 6) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick-label'>" & Format(d, "0.0") & "</text>")
            Next

            ' Axes
            sb.AppendLine("<line x1='" & Fmt(plotL) & "' y1='" & Fmt(plotT) & "' x2='" & Fmt(plotL) & "' y2='" & Fmt(plotB) & "' stroke='#3a3f4a' stroke-width='1.4'/>")
            sb.AppendLine("<line x1='" & Fmt(plotL) & "' y1='" & Fmt(plotB) & "' x2='" & Fmt(plotR) & "' y2='" & Fmt(plotB) & "' stroke='#3a3f4a' stroke-width='1.4'/>")
            sb.AppendLine("<text x='" & Fmt(plotL + plotW / 2) & "' y='" & Fmt(plotB + 36) & "' text-anchor='middle' class='axis-label'>Modulus (" & presUnit & ")</text>")
            sb.AppendLine("<text x='14' y='" & Fmt(plotT + plotH / 2) & "' text-anchor='middle' transform='rotate(-90,14," & Fmt(plotT + plotH / 2) & ")' class='axis-label'>Depth (" & thkUnit & ")</text>")

            ' Left panel: layer bars
            Dim layerColorList() As String = {"#37474F", "#00796B", "#795548", "#A1887F", "#BDBDBD", "#607D8B"}
            For i As Integer = 0 To allLayers.Count - 2
                Dim y1 As Single = SvgDepthToY(depths(i), plotT, maxDepth, plotH)
                Dim y2 As Single = SvgDepthToY(depths(i + 1), plotT, maxDepth, plotH)
                Dim h As Single = Math.Max(y2 - y1, 2)

                ' Determine if aggregate
                Dim isAgg As Boolean = IsAggregateSublayer(allLayers(i), sld)

                Dim barCol As String
                If isAgg Then
                    ' Brown gradient shade
                    Dim modRange As Single = maxMod - minMod
                    If modRange < 100 Then modRange = 100
                    Dim t As Single = (allLayers(i).Modulus - minMod) / modRange
                    t = Math.Max(0, Math.Min(1, t))
                    Dim r As Integer = CInt(161 + (90 - 161) * t * 0.7)
                    Dim gv As Integer = CInt(136 + (100 - 136) * t * 0.7)
                    Dim bv As Integer = CInt(127 + (80 - 127) * t * 0.7)
                    barCol = "#" & r.ToString("X2") & gv.ToString("X2") & bv.ToString("X2")
                ElseIf i = 0 Then
                    barCol = layerColorList(0)
                Else
                    barCol = layerColorList(Math.Min(i, layerColorList.Length - 1))
                End If

                sb.AppendLine("<rect x='" & Fmt(layerX) & "' y='" & Fmt(y1) & "' width='" & Fmt(layerW) & "' height='" & Fmt(h) & "' fill='" & barCol & "' fill-opacity='0.8' stroke='#666' stroke-width='0.5'/>")

                ' Thickness label inside bar
                If h > 14 Then
                    sb.AppendLine("<text x='" & Fmt(layerX + layerW / 2) & "' y='" & Fmt(y1 + h / 2 + 4) & "' text-anchor='middle' fill='white' class='small-label'>" &
                        Format(allLayers(i).Thickness, "0.0") & " " & thkUnit & "</text>")
                End If

                ' Highlight aggregate sublayers in chart area
                If isAgg Then
                    Dim xm As Single = SvgModToX(allLayers(i).Modulus, plotL, modPadMin, modPadMax, plotW)
                    sb.AppendLine("<rect x='" & Fmt(plotL) & "' y='" & Fmt(y1) & "' width='" & Fmt(xm - plotL) & "' height='" & Fmt(h) & "' fill='url(#agg-grad)'/>")
                End If
            Next

            ' Step profile line
            Dim pathD As String = ""
            For i As Integer = 0 To allLayers.Count - 2
                Dim y1 As Single = SvgDepthToY(depths(i), plotT, maxDepth, plotH)
                Dim y2 As Single = SvgDepthToY(depths(i + 1), plotT, maxDepth, plotH)
                Dim x As Single = SvgModToX(allLayers(i).Modulus, plotL, modPadMin, modPadMax, plotW)

                If i = 0 Then
                    pathD &= "M" & Fmt(x) & "," & Fmt(y1)
                Else
                    pathD &= " L" & Fmt(x) & "," & Fmt(y1)
                End If
                pathD &= " L" & Fmt(x) & "," & Fmt(y2)

                ' Modulus label
                Dim labelX As Single = x + 5
                Dim anchor As String = "start"
                If labelX + 50 > plotR Then
                    labelX = x - 5
                    anchor = "end"
                End If
                sb.AppendLine("<text x='" & Fmt(labelX) & "' y='" & Fmt((y1 + y2) / 2 + 4) & "' text-anchor='" & anchor & "' class='mod-label'>" & Format(allLayers(i).Modulus, "#,##0") & "</text>")

                ' Step connector to next layer
                If i < allLayers.Count - 2 Then
                    Dim xNext As Single = SvgModToX(allLayers(i + 1).Modulus, plotL, modPadMin, modPadMax, plotW)
                    pathD &= " L" & Fmt(xNext) & "," & Fmt(y2)
                End If
            Next
            sb.AppendLine("<path d='" & pathD & "' fill='none' stroke='#00796B' stroke-width='2.5' stroke-linejoin='round'/>")

            ' Dots at layer transitions
            For i As Integer = 0 To allLayers.Count - 2
                Dim y1 As Single = SvgDepthToY(depths(i), plotT, maxDepth, plotH)
                Dim x As Single = SvgModToX(allLayers(i).Modulus, plotL, modPadMin, modPadMax, plotW)
                sb.AppendLine("<circle cx='" & Fmt(x) & "' cy='" & Fmt(y1) & "' r='3' fill='#00796B' stroke='white' stroke-width='1'/>")
            Next

            sb.AppendLine("</svg>")
        End Sub


        Private Shared Function IsAggregateSublayer(layer As clsLayerInfo, sld As clsSublayerData) As Boolean
            For Each bsl In sld.BaseSublayers
                If Math.Abs(layer.Modulus - bsl.Modulus) < 1 AndAlso layer.LCode = bsl.LCode Then Return True
            Next
            For Each ssl In sld.SubbaseSublayers
                If Math.Abs(layer.Modulus - ssl.Modulus) < 1 AndAlso layer.LCode = ssl.LCode Then Return True
            Next
            Return False
        End Function


        Private Shared Function SvgDepthToY(d As Single, plotT As Single, maxDepth As Single, plotH As Single) As Single
            Return plotT + (d / maxDepth) * plotH
        End Function

        Private Shared Function SvgModToX(m As Single, plotL As Single, modPadMin As Single, modPadMax As Single, plotW As Single) As Single
            Return plotL + ((m - modPadMin) / (modPadMax - modPadMin)) * plotW
        End Function

        Private Shared Function Fmt(v As Single) As String
            Return Format(v, "0.#")
        End Function

#End Region

#Region "SVG Chart: Fatigue Curve"

        Private Shared Sub AppendFatigueCurveSVG(sb As StringBuilder, rpt As clsDetailedReportData, subgradeMod As Double, Optional acDetails() As clsAircraftDetail = Nothing)
            ' acDetails: aircraft array used for scatter overlay and Bleasdale detection. Falls back
            ' to rpt.AircraftDetails when omitted (thickness-design path).
            If acDetails Is Nothing Then acDetails = rpt.AircraftDetails

            Dim svgW As Integer = 900, svgH As Integer = 550
            Dim ml As Integer = 80, mr As Integer = 40, mt As Integer = 40, mb As Integer = 60
            Dim pw As Integer = svgW - ml - mr, ph As Integer = svgH - mt - mb

            ' Data range
            Dim computedAA As Double = 0.000247 + 0.000245 * Math.Log10(subgradeMod)
            Dim computedBB As Double = 0.0658 * subgradeMod ^ 0.559

            ' Detect Bleasdale model
            Dim isBleasdale As Boolean = False
            If acDetails IsNot Nothing Then
                For ia As Integer = 1 To UBound(acDetails)
                    If acDetails(ia) IsNot Nothing AndAlso acDetails(ia).SubgradeModelUsed = "Bleasdale" Then
                        isBleasdale = True : Exit For
                    End If
                Next
            End If

            ' Bleasdale parameters (mirror modCDF.FAAModulusThick / Bleasdale block in modCDF.vb).
            Dim a11 As Double = -0.163768916705
            Dim b11 As Double = 185.192806802
            Dim c11 As Double = 1.65054449461
            Dim microAsymptote As Double = 884.3  ' strain at which (a + b·ε) = 0 (formula divergence)
            Dim microClamp As Double = 1000.0     ' StrainMax floor enforced in modCDF.vb (FAA endurance limit)
            Dim microTransition As Double = 1765.1 ' Bleasdale → power-law crossover (N ≈ 1,000 cov)

            ' Axis ranges. For Bleasdale, the Y axis must fit N_fail at the strain clamp:
            '   N(ε=1,000με) = 10^((a + b·0.001)^(-1/c)) ≈ 10^10.27 ≈ 1.86×10^10
            ' so logNMax = 11 keeps the curve visible from the clamp downward without clipping.
            ' X axis starts just to the left of the clamp so the clamp marker is visible while
            ' avoiding the formula-divergence region (asymptote at 884 με) entirely.
            Dim logStrainMin As Double, logStrainMax As Double
            Dim logNMin As Double, logNMax As Double
            If isBleasdale Then
                logStrainMin = Math.Log10(microClamp) - 0.04
                logStrainMax = Math.Log10(microTransition) + 0.5
                logNMin = 0 : logNMax = 11
            Else
                logStrainMin = 2 : logStrainMax = 4
                logNMin = 0 : logNMax = 10
            End If

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='Subgrade damage model chart'>")
            sb.AppendLine("<title>Subgrade damage model chart</title>")

            ' Plot background
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#bcc3cc'/>")

            ' Bleasdale zone backgrounds
            If isBleasdale Then
                Dim xClamp = ml + ((Math.Log10(microClamp) - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                Dim xTransition = ml + ((Math.Log10(microTransition) - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                xClamp = Math.Max(ml, Math.Min(ml + pw, xClamp))
                xTransition = Math.Max(ml, Math.Min(ml + pw, xTransition))

                ' Zone A: Below clamp — strains floored to 1,000 με so N is constant (endurance limit)
                sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & Fmt(xClamp - ml) & "' height='" & ph & "' fill='#9E9E9E' opacity='0.10'/>")
                ' Zone B: Bleasdale curve (blue) — between clamp and transition
                sb.AppendLine("<rect x='" & Fmt(xClamp) & "' y='" & mt & "' width='" & Fmt(xTransition - xClamp) & "' height='" & ph & "' fill='#2E5EA8' opacity='0.07'/>")
                ' Zone C: Power law (amber)
                sb.AppendLine("<rect x='" & Fmt(xTransition) & "' y='" & mt & "' width='" & Fmt(ml + pw - xTransition) & "' height='" & ph & "' fill='#D68228' opacity='0.07'/>")

                ' Zone labels
                sb.AppendLine("<text x='" & Fmt((ml + xClamp) / 2) & "' y='" & (mt + 14) & "' text-anchor='middle' style='font-size:11px;fill:#5a6270;font-style:italic'>Below clamp</text>")
                sb.AppendLine("<text x='" & Fmt((xClamp + xTransition) / 2) & "' y='" & (mt + 14) & "' text-anchor='middle' style='font-size:13px;fill:#2E5EA8;font-style:italic'>Bleasdale Curve (Cov &ge; 1,000)</text>")
                sb.AppendLine("<text x='" & Fmt((xTransition + ml + pw) / 2) & "' y='" & (mt + 14) & "' text-anchor='middle' style='font-size:13px;fill:#B4641E;font-style:italic'>Power Law (Cov &lt; 1,000)</text>")

                ' Strain clamp vertical line at 1,000 με (FAARFIELD subgrade endurance limit; modCDF.vb floors StrainMax at 0.001)
                sb.AppendLine("<line x1='" & Fmt(xClamp) & "' y1='" & mt & "' x2='" & Fmt(xClamp) & "' y2='" & (mt + ph) & "' stroke='#388E3C' stroke-width='1.8' stroke-dasharray='6,3' opacity='0.85'/>")
                sb.AppendLine("<text x='" & Fmt(xClamp + 3) & "' y='" & (mt + ph - 5) & "' style='font-size:13px;fill:#388E3C;font-weight:600'>Strain Clamp " & Format(microClamp, "#,##0") & " &mu;&epsilon;</text>")

                ' Transition vertical line (Bleasdale → power law at 1,765.1 με)
                sb.AppendLine("<line x1='" & Fmt(xTransition) & "' y1='" & mt & "' x2='" & Fmt(xTransition) & "' y2='" & (mt + ph) & "' stroke='#B4641E' stroke-width='1.5' stroke-dasharray='6,3' opacity='0.6'/>")
                sb.AppendLine("<text x='" & Fmt(xTransition + 3) & "' y='" & (mt + ph - 5) & "' style='font-size:13px;fill:#B4641E'>Transition " & Format(microTransition, "0.0") & " &mu;&epsilon;</text>")

                ' N=1000 horizontal reference line
                Dim logN1000 As Double = 3
                Dim y1000 = mt + ph - ((logN1000 - logNMin) / (logNMax - logNMin)) * ph
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y1000) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y1000) & "' stroke='#B4641E' stroke-width='1' stroke-dasharray='4,4' opacity='0.5'/>")
                sb.AppendLine("<text x='" & (ml + pw - 5) & "' y='" & Fmt(y1000 - 4) & "' text-anchor='end' style='font-size:13px;fill:#B4641E'>N = 1,000</text>")
            End If

            ' Grid lines
            Dim nYDecades = CInt(logNMax - logNMin)
            For i As Integer = 0 To nYDecades
                Dim logVal = logNMin + i
                Dim y = mt + ph - ((logVal - logNMin) / (logNMax - logNMin)) * ph
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#d0d4da' stroke-width='0.9'/>")
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>10<tspan dy='-5' font-size='7'>" & CInt(logVal).ToString() & "</tspan></text>")
            Next
            ' X ticks
            Dim logXStart = Math.Ceiling(logStrainMin)
            Dim logXEnd = Math.Floor(logStrainMax)
            For i As Integer = CInt(logXStart) To CInt(logXEnd)
                Dim x = ml + ((i - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                sb.AppendLine("<line x1='" & Fmt(x) & "' y1='" & mt & "' x2='" & Fmt(x) & "' y2='" & (mt + ph) & "' stroke='#d0d4da' stroke-width='0.9'/>")
                Dim lbl = CInt(10 ^ i)
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & Format(lbl, "#,##0") & "</text>")
            Next

            ' Fatigue model curve
            Dim nPts As Integer = 400
            If isBleasdale Then
                ' Bleasdale: two-segment curve. Apply the same strain clamp the analysis applies
                ' (modCDF.vb floors StrainMax at 0.001) so the curve is horizontal at the
                ' endurance-limit value of N for ε < 1,000 με instead of running off to the
                ' formula's asymptote at 884 με. This makes the curve descend exactly from the
                ' clamp at 1,000 με as the user expects.
                Dim bleaPath As New StringBuilder("M")
                Dim powPath As New StringBuilder()
                Dim bleaStarted As Boolean = False, powStarted As Boolean = False
                Dim strainClampAbs As Double = microClamp / 1000000.0
                For i As Integer = 0 To nPts
                    Dim logS = logStrainMin + (logStrainMax - logStrainMin) * i / nPts
                    Dim microS = 10 ^ logS
                    Dim strainAbs As Double = microS / 1000000.0
                    ' Mirror the analysis-side clamp: any strain below 1,000 με is treated as 1,000 με.
                    Dim strainEff As Double = If(strainAbs < strainClampAbs, strainClampAbs, strainAbs)
                    Dim nFail As Double
                    Dim inner As Double = a11 + b11 * strainEff
                    If inner <= 0.0001 Then
                        nFail = 1.0E+15
                    ElseIf strainEff <= 0.001765093 Then
                        Dim expn As Double = inner ^ (-1.0 / c11)
                        If expn > 15 Then nFail = 1.0E+15 Else nFail = 10.0 ^ expn
                    Else
                        nFail = (0.00414131183 / strainEff) ^ 8.1
                    End If
                    Dim logN As Double = Math.Log10(Math.Max(nFail, 1))
                    logN = Math.Max(logNMin, Math.Min(logNMax, logN))
                    Dim x = ml + ((logS - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                    Dim y = mt + ph - ((logN - logNMin) / (logNMax - logNMin)) * ph

                    ' Branch by the *effective* (clamped) strain so the curve segments line up
                    ' with the rendered zones.
                    If strainEff <= 0.001765093 Then
                        If Not bleaStarted Then bleaPath.Append(Fmt(x) & " " & Fmt(y)) : bleaStarted = True _
                        Else bleaPath.Append(" L" & Fmt(x) & " " & Fmt(y))
                    Else
                        If Not powStarted Then
                            ' Add transition point to both
                            bleaPath.Append(" L" & Fmt(x) & " " & Fmt(y))
                            powPath.Append("M" & Fmt(x) & " " & Fmt(y))
                            powStarted = True
                        Else
                            powPath.Append(" L" & Fmt(x) & " " & Fmt(y))
                        End If
                    End If
                Next
                sb.AppendLine("<path d='" & bleaPath.ToString() & "' fill='none' stroke='#1F77B4' stroke-width='2.5'/>")
                If powPath.Length > 0 Then
                    sb.AppendLine("<path d='" & powPath.ToString() & "' fill='none' stroke='#B4641E' stroke-width='2.5' stroke-dasharray='8,4'/>")
                End If
            Else
                ' Standard model: single curve
                Dim pathD As New StringBuilder("M")
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
            End If

            ' Aircraft scatter points with label collision avoidance.
            ' Each point's N_fail comes from clsAircraftDetail.NtoFail, which was computed in
            ' modCDF.vb from the per-aircraft critical vertical compressive strain (ε_22) at the
            ' top of the subgrade using the Bleasdale piecewise formula (with strain floored to
            ' 0.001 = 1,000 με). For PCR the scatter set is the original-mix snapshot.
            Dim placedLabels As New List(Of Tuple(Of Double, Double))
            If acDetails IsNot Nothing Then
                For ia As Integer = 1 To UBound(acDetails)
                    If acDetails(ia) Is Nothing Then Continue For
                    Dim det = acDetails(ia)
                    Dim micro = det.VerticalStrain * 1000000
                    If micro <= 0 Then Continue For
                    ' Honor the same strain clamp the analysis applies (1,000 με) so the marker
                    ' lands on the rendered curve at low-strain aircraft.
                    If isBleasdale AndAlso micro < microClamp Then micro = microClamp
                    Dim logS = Math.Log10(micro)
                    ' Fall back to a recomputed N_fail when the analysis-side gNtoFail global was
                    ' not populated for this aircraft (non-tandem subgrade path in modCDF.vb).
                    Dim displayedN As Double = NtoFailForDisplay(det)
                    Dim logN = Math.Log10(Math.Max(displayedN, 1))
                    Dim x = ml + ((logS - logStrainMin) / (logStrainMax - logStrainMin)) * pw
                    Dim y = mt + ph - ((logN - logNMin) / (logNMax - logNMin)) * ph
                    x = Math.Max(ml, Math.Min(ml + pw, x))
                    y = Math.Max(mt, Math.Min(mt + ph, y))
                    Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                    sb.AppendLine("<circle cx='" & Fmt(x) & "' cy='" & Fmt(y) & "' r='5' fill='" & clr & "' stroke='white' stroke-width='1.5'>")
                    sb.AppendLine("<title>" & WebEncode(det.ACName) & ": strain=" & Format(micro, "0.0") & " microstrain, Nfail=" & Format(displayedN, "0.00E+00") & "</title>")
                    sb.AppendLine("</circle>")
                    ' Label collision avoidance
                    Dim labelY = y + 4
                    For Each placed In placedLabels
                        If Math.Abs(x - placed.Item1) < 60 AndAlso Math.Abs(labelY - placed.Item2) < 12 Then
                            labelY = placed.Item2 + 14
                        End If
                    Next
                    placedLabels.Add(Tuple.Create(x, labelY))
                    sb.AppendLine("<text x='" & Fmt(x + 8) & "' y='" & Fmt(labelY) & "' class='label'>" & WebEncode(det.ACName) & "</text>")

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
            Dim titleText = If(isBleasdale, "Subgrade Damage Model (Bleasdale Piecewise)", "Subgrade Damage Model")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>" & titleText & "</text>")

            ' Bleasdale equation box
            If isBleasdale Then
                sb.AppendLine("<rect x='" & (ml + pw - 270) & "' y='" & (mt + ph - 96) & "' width='265' height='91' fill='white' stroke='#ccc' rx='4' opacity='0.92'/>")
                sb.AppendLine("<text x='" & (ml + pw - 262) & "' y='" & (mt + ph - 80) & "' style='font-size:13px;fill:#333;font-weight:600'>Bleasdale (Cov &ge; 1,000):</text>")
                sb.AppendLine("<text x='" & (ml + pw - 262) & "' y='" & (mt + ph - 68) & "' style='font-size:13px;fill:#555;font-family:Cambria Math,serif'>N = 10^((a + b&middot;&epsilon;)^(-1/c))</text>")
                sb.AppendLine("<text x='" & (ml + pw - 262) & "' y='" & (mt + ph - 54) & "' style='font-size:13px;fill:#333;font-weight:600'>Power Law (Cov &lt; 1,000):</text>")
                sb.AppendLine("<text x='" & (ml + pw - 262) & "' y='" & (mt + ph - 42) & "' style='font-size:13px;fill:#555;font-family:Cambria Math,serif'>N = (0.00414 / &epsilon;)^8.1</text>")
                sb.AppendLine("<text x='" & (ml + pw - 262) & "' y='" & (mt + ph - 28) & "' style='font-size:13px;fill:#388E3C;font-weight:600'>Strain clamp: &epsilon; &ge; " & Format(microClamp, "#,##0") & " &mu;&epsilon;</text>")
                sb.AppendLine("<text x='" & (ml + pw - 262) & "' y='" & (mt + ph - 14) & "' style='font-size:12px;fill:#5a6270'>Asymptote: " & Format(microAsymptote, "0.0") & " &mu;&epsilon;  |  Transition: " & Format(microTransition, "0.0") & " &mu;&epsilon;</text>")
            End If

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Life Ratio"

        Private Shared Sub AppendLifeRatioSVG(sb As StringBuilder, rpt As clsDetailedReportData, Optional acDetails() As clsAircraftDetail = Nothing)
            If acDetails Is Nothing Then acDetails = rpt.AircraftDetails
            If acDetails Is Nothing Then Return
            Dim items As New List(Of Tuple(Of String, Double, String))
            For ia As Integer = 1 To UBound(acDetails)
                If acDetails(ia) Is Nothing Then Continue For
                Dim det = acDetails(ia)
                Dim ratio As Double = If(det.TotalRepetitions > 0, NtoFailForDisplay(det) / det.TotalRepetitions, 0)
                items.Add(Tuple.Create(det.ACName, ratio, ChartColors((ia - 1) Mod ChartColors.Length)))
            Next
            If items.Count = 0 Then Return

            Dim barH As Integer = 28, gap As Integer = 8
            Dim svgH = 60 + items.Count * (barH + gap)
            Dim svgW As Integer = 800, ml As Integer = 180, mr As Integer = 30
            Dim pw = svgW - ml - mr

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='Fatigue life reserve diverging bar chart'>")
            sb.AppendLine("<title>Fatigue life reserve diverging bar chart</title>")
            sb.AppendLine("<rect x='" & ml & "' y='30' width='" & pw & "' height='" & (svgH - 50) & "' fill='#FAFBFC' stroke='#bcc3cc'/>")
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

        ''' <summary>
        ''' Per-aircraft CDF vs Offset chart. Plots det.CDFByOffset(IOFF) — the gear-level
        ''' (multi-tire) CDF that the analysis stores at each of the NOFF=41 evaluation
        ''' offsets — mirrored across the centerline so the chart spans the full pavement
        ''' strip width [-400, +400] in. The analysis already sums tire contributions through
        ''' the C/P calculation; the report just plots what the backend produced (which is
        ''' the same data the FAARFIELD traffic-table max CDF is computed from). No synthetic
        ''' per-tire curves, no wheel-mirror reconstruction, no scaling heuristics.
        ''' </summary>
        Private Shared Sub AppendSingleAircraftCDFSvg(sb As StringBuilder, det As clsAircraftDetail, critOffset As Integer, acColor As String, lengthUnit As String)
            Dim svgW As Integer = 950, svgH As Integer = 450
            Dim ml As Integer = 85, mr As Integer = 25, mt As Integer = 40, mb As Integer = 55
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            ' Y-axis range. Auto-scale to peak with 20% headroom; widen to include CDF=1.0 when
            ' the curve is in the design-failure ballpark so the user can see how close the
            ' aircraft is to the design target.
            Dim maxCDF As Double = 0
            For ioff As Integer = 1 To CDF.NOFF
                If det.CDFByOffset(ioff) > maxCDF Then maxCDF = det.CDFByOffset(ioff)
            Next
            If maxCDF <= 0 Then maxCDF = 0.001
            Dim yMax As Double = maxCDF * 1.2
            If maxCDF >= 0.5 AndAlso yMax < 1.1 Then yMax = 1.1

            ' Bilateral X range: mirror the unilateral NOFF=41 offsets (0..400 in.) across the
            ' centerline so the chart spans -400..+400 in.
            Dim xMinVal As Double = -CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
            Dim xMaxVal As Double = CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
            Dim xRng As Double = xMaxVal - xMinVal
            Dim toX = Function(v As Double) ml + ((v - xMinVal) / xRng) * pw

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='CDF vs offset chart for " & WebEncode(det.ACName) & "'>")
            sb.AppendLine("<title>CDF vs offset chart for " & WebEncode(det.ACName) & "</title>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#bcc3cc'/>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='22' text-anchor='middle' class='chart-title'>" & WebEncode(det.ACName) & " &mdash; Per-Aircraft CDF vs Offset</text>")

            ' Y-axis tick step picked from a {1, 2, 2.5, 5} × 10^k ladder so labels are clean
            ' integers / one-decimal values rather than auto-divided fractions of yMax.
            Dim rawStep As Double = yMax / 6.0
            Dim mag As Double = Math.Pow(10, Math.Floor(Math.Log10(rawStep)))
            Dim normStep As Double = rawStep / mag
            Dim niceN As Double = If(normStep < 1.5, 1.0, If(normStep < 2.25, 2.0, If(normStep < 3.5, 2.5, If(normStep < 7.5, 5.0, 10.0))))
            Dim yStep As Double = niceN * mag
            ' Round yMax up to the next yStep so the top tick lands on a clean number.
            Dim yMaxNice As Double = Math.Ceiling(yMax / yStep) * yStep
            yMax = yMaxNice
            ' Decimals: enough to express yStep without trailing junk (e.g., 0.2 -> 1 decimal).
            Dim yDecimals As Integer = Math.Max(0, CInt(-Math.Floor(Math.Log10(yStep))))
            Dim yFmt As String = If(yDecimals = 0, "0", "0." & New String("0"c, yDecimals))

            ' Y-axis grid + tick labels (drawn first so the curve renders on top)
            Dim yTickVal As Double = 0
            Do While yTickVal <= yMax + yStep * 0.0001
                Dim y As Double = mt + ph - (yTickVal / yMax) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>" & Format(yTickVal, yFmt) & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#d0d4da' stroke-width='0.9'/>")
                yTickVal += yStep
            Loop
            For xTick As Integer = CInt(xMinVal) To CInt(xMaxVal) Step 100
                Dim xp As Double = toX(xTick)
                sb.AppendLine("<text x='" & Fmt(xp) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & xTick.ToString() & "</text>")
            Next

            ' CDF = 1.0 horizontal reference line (design-failure threshold) when within range.
            ' Drawn with an inline caption on the line itself; intentionally NOT in the legend.
            Dim showDesignLine As Boolean = (1.0 <= yMax)
            If showDesignLine Then
                Dim yDesign As Double = mt + ph - (1.0 / yMax) * ph
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(yDesign) & "' x2='" & (ml + pw) & "' y2='" & Fmt(yDesign) & "' stroke='#7a8595' stroke-width='1.1' stroke-dasharray='6,4' opacity='0.85'/>")
                sb.AppendLine("<text x='" & (ml + 8) & "' y='" & Fmt(yDesign - 4) & "' style='font-size:11px;fill:#5a6270;font-weight:600'>CDF = 1.0 (design failure)</text>")
            End If

            ' Bilateral mirror: absIdx = |ib - (NOFF-1)| + 1 maps bilateral index ib in
            ' [0, 2·NOFF-2] back to unilateral [1, NOFF]. Render strategy:
            '   1) If per-tire CDF capture is available, draw a stacked area where each colored
            '      layer is one tire's CDF contribution (det.CDFContribByTireByOffset(iw, IOFF))
            '      and the stack top equals det.CDFByOffset(IOFF). This makes the strip-by-strip
            '      tire summation visually obvious.
            '   2) Otherwise (older saved jobs), fall back to the previous single-curve fill+stroke.
            Dim nBilPts As Integer = 2 * (CDF.NOFF - 1) + 1
            ' Tableau-10 inspired palette — softer, more harmonious than D3 categorical.
            ' Holds up well under the more transparent fill (opacity 0.32) used for the stack.
            Dim tireColors() As String = {"#4E79A7", "#F28E2B", "#59A14F", "#E15759", "#76B7B2", "#EDC948", "#B07AA1", "#9C755F"}

            Dim haveStack As Boolean = det.HasTireCDFContrib AndAlso det.NWheels >= 1 _
                                       AndAlso det.CDFContribByTireByOffset IsNot Nothing
            If haveStack Then
                Dim nTires As Integer = det.NWheels
                ' Build per-tire bilateral series and cumulative stack heights.
                Dim cumLower(nBilPts - 1) As Double  ' running stack baseline
                Dim cumUpper(nBilPts - 1) As Double  ' running stack top
                For ib As Integer = 0 To nBilPts - 1
                    cumLower(ib) = 0
                    cumUpper(ib) = 0
                Next
                For iw As Integer = 1 To nTires
                    Dim cIdx As Integer = (iw - 1) Mod tireColors.Length
                    Dim layerPath As New StringBuilder()
                    ' Upper boundary forward
                    For ib As Integer = 0 To nBilPts - 1
                        Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                        Dim absIdx As Integer = Math.Abs(ib - (CDF.NOFF - 1)) + 1
                        Dim contrib As Double = 0
                        If absIdx >= 1 AndAlso absIdx <= CDF.NOFF AndAlso iw <= det.CDFContribByTireByOffset.GetUpperBound(0) Then
                            contrib = det.CDFContribByTireByOffset(iw, absIdx)
                        End If
                        cumUpper(ib) = cumLower(ib) + contrib
                        Dim xp As Double = toX(offVal)
                        Dim yp As Double = mt + ph - (Math.Min(cumUpper(ib), yMax) / yMax) * ph
                        If ib = 0 Then layerPath.Append("M" & Fmt(xp) & " " & Fmt(yp)) Else layerPath.Append(" L" & Fmt(xp) & " " & Fmt(yp))
                    Next
                    ' Lower boundary reverse (close the polygon)
                    For ib As Integer = nBilPts - 1 To 0 Step -1
                        Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                        Dim xp As Double = toX(offVal)
                        Dim yp As Double = mt + ph - (Math.Min(cumLower(ib), yMax) / yMax) * ph
                        layerPath.Append(" L" & Fmt(xp) & " " & Fmt(yp))
                    Next
                    layerPath.Append(" Z")
                    ' More transparent fill (0.32) so the cumulative top line reads cleanly through
                    ' the stack; dashed contour at 0.85 opacity to delineate each tire's band.
                    sb.AppendLine("<path d='" & layerPath.ToString() & "' fill='" & tireColors(cIdx) & "' fill-opacity='0.32' stroke='" & tireColors(cIdx) & "' stroke-width='1.0' stroke-dasharray='4,2' stroke-opacity='0.85'/>")
                    ' Promote upper to next layer's lower
                    For ib As Integer = 0 To nBilPts - 1
                        cumLower(ib) = cumUpper(ib)
                    Next
                Next

                ' Top stroke: trace det.CDFByOffset directly so the visible top equals the
                ' analysis-side cumulative regardless of any rounding in the per-tire stack.
                ' Slightly thicker solid line so the per-aircraft cumulative reads as the
                ' primary curve over the dashed tire-band contours.
                Dim topPath As New StringBuilder()
                For ib As Integer = 0 To nBilPts - 1
                    Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                    Dim absIdx As Integer = Math.Abs(ib - (CDF.NOFF - 1)) + 1
                    Dim cdfVal As Double = If(absIdx >= 1 AndAlso absIdx <= CDF.NOFF, det.CDFByOffset(absIdx), 0)
                    Dim xp As Double = toX(offVal)
                    Dim yp As Double = mt + ph - (Math.Min(cdfVal, yMax) / yMax) * ph
                    If ib = 0 Then topPath.Append("M" & Fmt(xp) & " " & Fmt(yp)) Else topPath.Append(" L" & Fmt(xp) & " " & Fmt(yp))
                Next
                sb.AppendLine("<path d='" & topPath.ToString() & "' fill='none' stroke='" & acColor & "' stroke-width='2.6' stroke-linejoin='round' stroke-linecap='round'/>")
            Else
                ' Fallback: single-curve fill+stroke (older saved jobs without per-tire capture).
                Dim cdfPath As New StringBuilder()
                Dim cdfFill As New StringBuilder()
                Dim cdfLastX As Double = 0
                For ib As Integer = 0 To nBilPts - 1
                    Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                    Dim absIdx As Integer = Math.Abs(ib - (CDF.NOFF - 1)) + 1
                    Dim cdfVal As Double = If(absIdx >= 1 AndAlso absIdx <= CDF.NOFF, det.CDFByOffset(absIdx), 0)
                    Dim xp As Double = toX(offVal)
                    Dim yp As Double = mt + ph - (cdfVal / yMax) * ph
                    If ib = 0 Then
                        cdfPath.Append("M" & Fmt(xp) & " " & Fmt(yp))
                        cdfFill.Append("M" & Fmt(xp) & " " & Fmt(mt + ph) & " L" & Fmt(xp) & " " & Fmt(yp))
                    Else
                        cdfPath.Append(" L" & Fmt(xp) & " " & Fmt(yp))
                        cdfFill.Append(" L" & Fmt(xp) & " " & Fmt(yp))
                    End If
                    cdfLastX = xp
                Next
                cdfFill.Append(" L" & Fmt(cdfLastX) & " " & Fmt(mt + ph) & " Z")
                sb.AppendLine("<path d='" & cdfFill.ToString() & "' fill='" & acColor & "' opacity='0.10'/>")
                sb.AppendLine("<path d='" & cdfPath.ToString() & "' fill='none' stroke='" & acColor & "' stroke-width='2.6' stroke-linejoin='round' stroke-linecap='round'/>")
            End If

            ' Centerline (offset = 0)
            Dim zeroX As Double = toX(0)
            sb.AppendLine("<line x1='" & Fmt(zeroX) & "' y1='" & mt & "' x2='" & Fmt(zeroX) & "' y2='" & (mt + ph) & "' stroke='black' stroke-width='1.5'/>")

            ' Critical offset markers (both sides of centerline)
            If critOffset >= 1 AndAlso critOffset <= CDF.NOFF Then
                Dim critVal As Double = (critOffset - 1) * CDF.OFFSETINC
                Dim critXR As Double = toX(critVal)
                Dim critXL As Double = toX(-critVal)
                sb.AppendLine("<line x1='" & Fmt(critXR) & "' y1='" & mt & "' x2='" & Fmt(critXR) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
                If critVal > 0 Then sb.AppendLine("<line x1='" & Fmt(critXL) & "' y1='" & mt & "' x2='" & Fmt(critXL) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
            End If

            ' Axis labels
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>Offset (" & lengthUnit & ")</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>CDF</text>")

            ' Legend (CDF=1.0 reference is intentionally NOT included — it is captioned in-place on the line)
            Dim lgX As Integer = svgW - mr - 230
            Dim lgY As Integer = mt + 8
            ' Per-aircraft cumulative CDF — primary thick solid line, matches the top stroke
            sb.AppendLine("<line x1='" & lgX & "' y1='" & (lgY + 5) & "' x2='" & (lgX + 16) & "' y2='" & (lgY + 5) & "' stroke='" & acColor & "' stroke-width='2.6' stroke-linecap='round'/>")
            sb.AppendLine("<text x='" & (lgX + 20) & "' y='" & (lgY + 9) & "' style='font-size:13px'>Per-aircraft CDF (&Sigma; tires per strip)</text>")
            ' Tire-contributions stacked swatch with dashed contour (matches the layer style)
            If haveStack Then
                lgY += 16
                Dim swatchY As Integer = lgY + 1
                Dim swatchH As Integer = 9
                Dim swatchBandH As Integer = swatchH \ 4
                Dim sw0 As Integer = lgX
                Dim sw1 As Integer = lgX + 16
                For sb_i As Integer = 0 To 3
                    Dim cIdx As Integer = sb_i Mod tireColors.Length
                    sb.AppendLine("<rect x='" & sw0 & "' y='" & (swatchY + sb_i * swatchBandH) & "' width='" & (sw1 - sw0) & "' height='" & swatchBandH & "' fill='" & tireColors(cIdx) & "' fill-opacity='0.32' stroke='" & tireColors(cIdx) & "' stroke-width='0.6' stroke-dasharray='2,1' stroke-opacity='0.85'/>")
                Next
                sb.AppendLine("<text x='" & (lgX + 20) & "' y='" & (lgY + 9) & "' style='font-size:13px'>Tire contributions (stacked)</text>")
            End If
            lgY += 16
            sb.AppendLine("<line x1='" & lgX & "' y1='" & (lgY + 5) & "' x2='" & (lgX + 16) & "' y2='" & (lgY + 5) & "' stroke='black' stroke-width='1.5'/>")
            sb.AppendLine("<text x='" & (lgX + 20) & "' y='" & (lgY + 9) & "' style='font-size:13px'>Centerline (0)</text>")
            lgY += 16
            sb.AppendLine("<line x1='" & lgX & "' y1='" & (lgY + 5) & "' x2='" & (lgX + 16) & "' y2='" & (lgY + 5) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
            sb.AppendLine("<text x='" & (lgX + 20) & "' y='" & (lgY + 9) & "' fill='#D62728' style='font-size:13px'>Critical offset</text>")

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Coverage Plot (C/P Distribution)"

        Private Shared Sub AppendCoveragePlotSVG(sb As StringBuilder, rpt As clsDetailedReportData, lengthUnit As String)
            Dim svgW As Integer = 950, svgH As Integer = 450
            Dim ml As Integer = 80, mr As Integer = 150, mt As Integer = 40, mb As Integer = 50
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            ' Bilateral range
            Dim nBilateral As Integer = 2 * (CDF.NOFF - 1) + 1
            Dim xMinVal As Double = -CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
            Dim xMaxVal As Double = CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
            Dim xRng As Double = xMaxVal - xMinVal
            Dim toX = Function(v As Double) ml + ((v - xMinVal) / xRng) * pw

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
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='Coverage-to-pass distribution chart'>")
            sb.AppendLine("<title>Coverage-to-pass distribution chart</title>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#bcc3cc'/>")
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='20' text-anchor='middle' class='chart-title'>Coverage-to-Pass (C/P) Distribution</text>")

            ' Centerline
            Dim cx = toX(0)
            sb.AppendLine("<line x1='" & Fmt(cx) & "' y1='" & mt & "' x2='" & Fmt(cx) & "' y2='" & (mt + ph) & "' stroke='#000' stroke-width='0.8' stroke-dasharray='4,3'/>")

            ' Draw curves per aircraft (bilateral)
            For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                Dim pathD As New StringBuilder("M")
                For ib As Integer = 0 To nBilateral - 1
                    Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                    Dim absIdx As Integer = Math.Abs(ib - (CDF.NOFF - 1)) + 1
                    Dim x = toX(offVal)
                    Dim y = mt + ph - (rpt.CDFSweep.CtoPPerAircraftPerOffset(ia, absIdx) / yMax) * ph
                    If ib = 0 Then pathD.Append(Fmt(x) & " " & Fmt(y)) Else pathD.Append(" L" & Fmt(x) & " " & Fmt(y))
                Next
                sb.AppendLine("<path d='" & pathD.ToString() & "' fill='none' stroke='" & clr & "' stroke-width='2'/>")
            Next

            ' Per-wheel C/P decomposition for critical aircraft only
            Dim critAcIdx As Integer = -1
            Dim critAcCDF As Double = 0
            If rpt.AircraftDetails IsNot Nothing Then
                For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                    If ia <= UBound(rpt.AircraftDetails) AndAlso rpt.AircraftDetails(ia) IsNot Nothing Then
                        If rpt.AircraftDetails(ia).MaxCDF > critAcCDF Then
                            critAcCDF = rpt.AircraftDetails(ia).MaxCDF
                            critAcIdx = ia
                        End If
                    End If
                Next
            End If
            Dim tireColorsHex() As String = {"#1F77B4", "#FF7F0E", "#2CA02C", "#D6272B", "#9467BD", "#8C564B", "#E377C2", "#7F7F7F"}
            If critAcIdx > 0 Then
                Dim critDet = rpt.AircraftDetails(critAcIdx)
                Dim hasTireData As Boolean = (critDet.NWheels > 1 AndAlso critDet.WheelX IsNot Nothing AndAlso critDet.XCenter > 0 AndAlso critDet.TireWidth > 0)
                If hasTireData Then
                    Dim sigma As Double = 30.435
                    Dim halfTW As Double = critDet.TireWidth / 2.0

                    ' Compute per-tire C/P at each bilateral offset
                    Dim perTireCtoP(critDet.NWheels, nBilateral - 1) As Double
                    Dim totalCtoP(nBilateral - 1) As Double
                    For ib As Integer = 0 To nBilateral - 1
                        Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                        totalCtoP(ib) = 0
                        For iw As Integer = 1 To critDet.NWheels
                            Dim wheelOff As Double = critDet.XCenter + critDet.WheelX(iw)
                            Dim yoffR As Double = Math.Abs(offVal - wheelOff)
                            Dim cpR As Double = GaussArea(yoffR - halfTW, yoffR + halfTW, sigma)
                            Dim yoffL As Double = Math.Abs(offVal + wheelOff)
                            Dim cpL As Double = GaussArea(yoffL - halfTW, yoffL + halfTW, sigma)
                            perTireCtoP(iw, ib) = cpR + cpL
                            totalCtoP(ib) += cpR + cpL
                        Next
                    Next

                    ' Scale factor: match recomputed total to stored C/P peak
                    Dim scaleFactor As Double = 1.0
                    Dim storedPeak As Double = 0
                    For ioff As Integer = 1 To CDF.NOFF
                        If rpt.CDFSweep.CtoPPerAircraftPerOffset(critAcIdx, ioff) > storedPeak Then storedPeak = rpt.CDFSweep.CtoPPerAircraftPerOffset(critAcIdx, ioff)
                    Next
                    Dim recompPeak As Double = 0
                    For ib As Integer = 0 To nBilateral - 1
                        If totalCtoP(ib) > recompPeak Then recompPeak = totalCtoP(ib)
                    Next
                    If recompPeak > 0.000001 Then scaleFactor = storedPeak / recompPeak

                    ' Draw per-tire C/P curves (dashed)
                    For iw As Integer = 1 To critDet.NWheels
                        Dim cIdx As Integer = (iw - 1) Mod tireColorsHex.Length
                        Dim tirePath As New StringBuilder()
                        For ib As Integer = 0 To nBilateral - 1
                            Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                            Dim scaledCP As Double = perTireCtoP(iw, ib) * scaleFactor
                            Dim xp As Double = toX(offVal)
                            Dim yp As Double = mt + ph - (scaledCP / yMax) * ph
                            If ib = 0 Then
                                tirePath.Append("M" & Fmt(xp) & " " & Fmt(yp))
                            Else
                                tirePath.Append(" L" & Fmt(xp) & " " & Fmt(yp))
                            End If
                        Next
                        sb.AppendLine("<path d='" & tirePath.ToString() & "' fill='none' stroke='" & tireColorsHex(cIdx) & "' stroke-width='1.2' stroke-dasharray='4,3' opacity='0.6'/>")
                    Next
                End If
            End If

            ' Bilateral critical offset lines
            If rpt.CDFSweep.MaxCDFOffset >= 1 Then
                Dim critVal As Double = (rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC
                Dim critXP = toX(critVal)
                Dim critXN = toX(-critVal)
                sb.AppendLine("<line x1='" & Fmt(critXP) & "' y1='" & mt & "' x2='" & Fmt(critXP) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
                sb.AppendLine("<line x1='" & Fmt(critXN) & "' y1='" & mt & "' x2='" & Fmt(critXN) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
            End If

            ' Y ticks
            Dim nYT As Integer = 5
            For i As Integer = 0 To nYT
                Dim val = yMax * i / nYT
                Dim y = mt + ph - (i / CDbl(nYT)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>" & Format(val, "0.0000") & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#d0d4da' stroke-width='0.9'/>")
            Next
            ' X ticks (bilateral: -400 to +400, step 100)
            For i As Integer = -4 To 4
                Dim val = i * 100
                Dim x = toX(val)
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
            Dim extraLegY = legY + rpt.CDFSweep.NAircraftCaptured * 18
            sb.AppendLine("<line x1='" & legX & "' y1='" & Fmt(extraLegY + 6) & "' x2='" & (legX + 12) & "' y2='" & Fmt(extraLegY + 6) & "' stroke='#000' stroke-width='0.8' stroke-dasharray='4,3'/>")
            sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(extraLegY + 10) & "' class='legend-text'>Centerline</text>")
            extraLegY += 18
            sb.AppendLine("<line x1='" & legX & "' y1='" & Fmt(extraLegY + 6) & "' x2='" & (legX + 12) & "' y2='" & Fmt(extraLegY + 6) & "' stroke='#D62728' stroke-width='1' stroke-dasharray='5,3'/>")
            sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(extraLegY + 10) & "' class='legend-text'>Critical offset</text>")

            ' Per-wheel legend entries (critical aircraft only)
            If critAcIdx > 0 Then
                Dim critDet2 = rpt.AircraftDetails(critAcIdx)
                Dim hasTD As Boolean = (critDet2.NWheels > 1 AndAlso critDet2.WheelX IsNot Nothing AndAlso critDet2.XCenter > 0 AndAlso critDet2.TireWidth > 0)
                If hasTD Then
                    For iw As Integer = 1 To Math.Min(critDet2.NWheels, tireColorsHex.Length)
                        extraLegY += 18
                        Dim cIdx As Integer = (iw - 1) Mod tireColorsHex.Length
                        Dim wheelPos As Double = critDet2.XCenter + critDet2.WheelX(iw)
                        sb.AppendLine("<line x1='" & legX & "' y1='" & Fmt(extraLegY + 6) & "' x2='" & (legX + 12) & "' y2='" & Fmt(extraLegY + 6) & "' stroke='" & tireColorsHex(cIdx) & "' stroke-width='1.2' stroke-dasharray='4,3' opacity='0.6'/>")
                        sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(extraLegY + 10) & "' class='legend-text'>Tire " & iw & " (x=" & Format(wheelPos, "0.0") & ")</text>")
                    Next
                End If
            End If

            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>Offset (" & lengthUnit & ")</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>C/P Ratio</text>")
            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Chart: Composite CDF"

        Private Shared Sub AppendCompositeCDFSvg(sb As StringBuilder, rpt As clsDetailedReportData, lengthUnit As String)
            Dim svgW As Integer = 950, svgH As Integer = 500
            Dim ml As Integer = 90, mr As Integer = 150, mt As Integer = 40, mb As Integer = 50
            Dim pw = svgW - ml - mr, ph = svgH - mt - mb

            ' Bilateral range
            Dim nBilateral As Integer = 2 * (CDF.NOFF - 1) + 1
            Dim xMinVal As Double = -CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
            Dim xMaxVal As Double = CDbl((CDF.NOFF - 1) * CDF.OFFSETINC)
            Dim xRng As Double = xMaxVal - xMinVal
            Dim toX = Function(v As Double) ml + ((v - xMinVal) / xRng) * pw

            Dim maxCDF As Double = 0
            For ioff As Integer = 1 To CDF.NOFF
                If rpt.CDFSweep.CDFTotalPerOffset(ioff) > maxCDF Then maxCDF = rpt.CDFSweep.CDFTotalPerOffset(ioff)
            Next
            If maxCDF <= 0 Then maxCDF = 0.001
            Dim yMax = maxCDF * 1.2

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='Composite CDF distribution across pavement width'>")
            sb.AppendLine("<title>Composite CDF distribution across pavement width</title>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#bcc3cc'/>")
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='20' text-anchor='middle' class='chart-title'>Composite CDF Across Pavement Width</text>")

            ' Centerline
            Dim cx = toX(0)
            sb.AppendLine("<line x1='" & Fmt(cx) & "' y1='" & mt & "' x2='" & Fmt(cx) & "' y2='" & (mt + ph) & "' stroke='#000' stroke-width='0.8' stroke-dasharray='4,3'/>")

            ' Per-aircraft CDF curves (bilateral)
            For ia As Integer = 1 To rpt.CDFSweep.NAircraftCaptured
                Dim clr = ChartColors((ia - 1) Mod ChartColors.Length)
                Dim pathD As New StringBuilder("M")
                For ib As Integer = 0 To nBilateral - 1
                    Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                    Dim absIdx As Integer = Math.Abs(ib - (CDF.NOFF - 1)) + 1
                    Dim x = toX(offVal)
                    Dim y = mt + ph - (rpt.CDFSweep.CDFPerAircraftPerOffset(ia, absIdx) / yMax) * ph
                    If ib = 0 Then pathD.Append(Fmt(x) & " " & Fmt(y)) Else pathD.Append(" L" & Fmt(x) & " " & Fmt(y))
                Next
                sb.AppendLine("<path d='" & pathD.ToString() & "' fill='none' stroke='" & clr & "' stroke-width='2' opacity='0.7'/>")
            Next

            ' Total CDF (thick black) with filled area (bilateral)
            Dim totalPath As New StringBuilder("M")
            Dim totalFill As New StringBuilder("M")
            Dim totalFirstX As Double = toX(xMinVal)
            Dim totalLastX As Double = toX(xMinVal)
            For ib As Integer = 0 To nBilateral - 1
                Dim offVal As Double = xMinVal + ib * CDF.OFFSETINC
                Dim absIdx As Integer = Math.Abs(ib - (CDF.NOFF - 1)) + 1
                Dim x = toX(offVal)
                Dim y = mt + ph - (rpt.CDFSweep.CDFTotalPerOffset(absIdx) / yMax) * ph
                If ib = 0 Then
                    totalPath.Append(Fmt(x) & " " & Fmt(y))
                    totalFill.Append(Fmt(x) & " " & Fmt(mt + ph) & " L" & Fmt(x) & " " & Fmt(y))
                    totalFirstX = x
                Else
                    totalPath.Append(" L" & Fmt(x) & " " & Fmt(y))
                    totalFill.Append(" L" & Fmt(x) & " " & Fmt(y))
                End If
                totalLastX = x
            Next
            totalFill.Append(" L" & Fmt(totalLastX) & " " & Fmt(mt + ph) & " Z")
            sb.AppendLine("<path d='" & totalFill.ToString() & "' fill='#222' opacity='0.06'/>")
            sb.AppendLine("<path d='" & totalPath.ToString() & "' fill='none' stroke='#222' stroke-width='2.5'/>")

            ' Bilateral critical offset
            If rpt.CDFSweep.MaxCDFOffset >= 1 Then
                Dim critVal As Double = (rpt.CDFSweep.MaxCDFOffset - 1) * CDF.OFFSETINC
                Dim critXP = toX(critVal)
                Dim critXN = toX(-critVal)
                sb.AppendLine("<line x1='" & Fmt(critXP) & "' y1='" & mt & "' x2='" & Fmt(critXP) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1.5' stroke-dasharray='5,3'/>")
                sb.AppendLine("<line x1='" & Fmt(critXN) & "' y1='" & mt & "' x2='" & Fmt(critXN) & "' y2='" & (mt + ph) & "' stroke='#D62728' stroke-width='1.5' stroke-dasharray='5,3'/>")
            End If

            ' Y ticks
            Dim nYT As Integer = 5
            For i As Integer = 0 To nYT
                Dim val = yMax * i / nYT
                Dim y = mt + ph - (i / CDbl(nYT)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>" & FmtCDFSvg(val) & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#d0d4da' stroke-width='0.9'/>")
            Next
            ' X ticks (bilateral: -400 to +400, step 100)
            For i As Integer = -4 To 4
                Dim val = i * 100
                Dim x = toX(val)
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
            totLegY += 18
            sb.AppendLine("<line x1='" & legX & "' y1='" & Fmt(totLegY + 6) & "' x2='" & (legX + 12) & "' y2='" & Fmt(totLegY + 6) & "' stroke='#000' stroke-width='0.8' stroke-dasharray='4,3'/>")
            sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(totLegY + 10) & "' class='legend-text'>Centerline</text>")
            totLegY += 18
            sb.AppendLine("<line x1='" & legX & "' y1='" & Fmt(totLegY + 6) & "' x2='" & (legX + 12) & "' y2='" & Fmt(totLegY + 6) & "' stroke='#D62728' stroke-width='1.5' stroke-dasharray='5,3'/>")
            sb.AppendLine("<text x='" & (legX + 16) & "' y='" & Fmt(totLegY + 10) & "' class='legend-text'>Critical offset</text>")

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
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='CDF contribution percentage bar chart'>")
            sb.AppendLine("<title>CDF contribution percentage bar chart</title>")
            sb.AppendLine("<rect x='" & ml & "' y='30' width='" & pw & "' height='" & (svgH - 50) & "' fill='#FAFBFC' stroke='#bcc3cc'/>")
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
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='Newton-Raphson convergence chart'>")
            sb.AppendLine("<title>Newton-Raphson convergence chart</title>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#bcc3cc'/>")
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
                sb.AppendLine("<text x='" & (ml + pw - 5) & "' y='" & Fmt(yThresh - 4) & "' text-anchor='end' style='font-size:13px;fill:#2CA02C'>Threshold = " & Format(CDF.CDFExitErr, "0.000") & "</text>")
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
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='ACR vs CDF per departure bubble chart'>")
            sb.AppendLine("<title>ACR vs CDF per departure bubble chart</title>")
            sb.AppendLine("<rect x='" & ml & "' y='" & mt & "' width='" & pw & "' height='" & ph & "' fill='#FAFBFC' stroke='#bcc3cc'/>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='20' text-anchor='middle' class='chart-title'>ACR vs. CDF per Departure</text>")

            For Each pt In pts
                Dim x = ml + ((pt.Item1 - minACR) / (maxACR - minACR)) * pw
                Dim logV = Math.Log10(pt.Item2)
                Dim y = mt + ph - ((logV - minLogCPD) / (maxLogCPD - minLogCPD)) * ph
                Dim r = 8 + 20 * (pt.Item3 / maxDep)
                sb.AppendLine("<circle cx='" & Fmt(x) & "' cy='" & Fmt(y) & "' r='" & Fmt(r) & "' fill='" & pt.Item5 & "' opacity='0.6' stroke='" & pt.Item5 & "' stroke-width='1.5'>")
                sb.AppendLine("<title>" & WebEncode(pt.Item4) & ": ACR=" & Format(pt.Item1, "0.0") & ", CDF/dep=" & Format(pt.Item2, "0.00E+00") & ", Dep=" & Format(pt.Item3, "#,##0") & "</title>")
                sb.AppendLine("</circle>")
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & Fmt(y - r - 4) & "' text-anchor='middle' class='label'>" & WebEncode(pt.Item4) & "</text>")
            Next

            ' Axes
            sb.AppendLine("<text x='" & Fmt(ml + pw / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' class='axis-label'>ACR</text>")
            sb.AppendLine("<text x='12' y='" & Fmt(mt + ph / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90,12," & Fmt(mt + ph / 2) & ")'>CDF per Departure (log scale)</text>")

            ' Y ticks
            For i As Integer = CInt(minLogCPD) To CInt(maxLogCPD)
                Dim y = mt + ph - ((i - minLogCPD) / (maxLogCPD - minLogCPD)) * ph
                sb.AppendLine("<text x='" & (ml - 5) & "' y='" & Fmt(y + 4) & "' text-anchor='end' class='tick'>1E" & i.ToString() & "</text>")
                sb.AppendLine("<line x1='" & ml & "' y1='" & Fmt(y) & "' x2='" & (ml + pw) & "' y2='" & Fmt(y) & "' stroke='#d0d4da' stroke-width='0.9'/>")
            Next

            ' X ticks for ACR
            Dim nXT As Integer = 5
            For i As Integer = 0 To nXT
                Dim val = minACR + (maxACR - minACR) * i / nXT
                Dim x = ml + (i / CDbl(nXT)) * pw
                sb.AppendLine("<text x='" & Fmt(x) & "' y='" & (mt + ph + 18) & "' text-anchor='middle' class='tick'>" & Format(val, "0.0") & "</text>")
                sb.AppendLine("<line x1='" & Fmt(x) & "' y1='" & mt & "' x2='" & Fmt(x) & "' y2='" & (mt + ph) & "' stroke='#d0d4da' stroke-width='0.9'/>")
            Next

            ' Bubble size legend
            sb.AppendLine("<rect x='" & (ml + pw - 155) & "' y='" & (mt + 5) & "' width='150' height='40' fill='white' stroke='#ccc' rx='4' opacity='0.9'/>")
            sb.AppendLine("<circle cx='" & (ml + pw - 140) & "' cy='" & (mt + 20) & "' r='6' fill='#999' opacity='0.5'/>")
            sb.AppendLine("<circle cx='" & (ml + pw - 120) & "' cy='" & (mt + 20) & "' r='12' fill='#999' opacity='0.5'/>")
            sb.AppendLine("<text x='" & (ml + pw - 105) & "' y='" & (mt + 24) & "' class='tick'>Bubble size = Ann. Departures</text>")

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Diagram: Pavement Cross-Section"

        Private Shared Sub AppendPavementCrossSectionSVG(sb As StringBuilder, rpt As clsDetailedReportData, det As clsAircraftDetail, thkUnit As String, lenUnit As String)
            Dim svgW As Integer = 850, svgH As Integer = 450
            Dim layers = rpt.SublayerData.DesignLayers
            If layers.Count < 2 Then Return

            ' Calculate total depth (excluding semi-infinite subgrade)
            Dim totalDepth As Double = 0
            For i As Integer = 0 To layers.Count - 2
                totalDepth += layers(i).Thickness
            Next
            If totalDepth <= 0 Then Return
            Dim displayDepth = totalDepth * 1.35

            ' Layout
            Dim leftX As Integer = 50, leftW As Integer = 300
            Dim rightX As Integer = 430, rightW As Integer = 360
            Dim topY As Integer = 50, availH As Integer = svgH - 100
            Dim pxPerIn As Double = availH / displayDepth

            ' Layer colors
            Dim layerColors() As String = {"#505050", "#C2B280", "#D2B48C", "#8B7765", "#789A5A"}

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='Pavement cross-section diagram for " & WebEncode(det.ACName) & "'>")
            sb.AppendLine("<title>Pavement cross-section diagram for " & WebEncode(det.ACName) & "</title>")
            sb.AppendLine("<text x='" & Fmt(svgW / 2) & "' y='25' text-anchor='middle' class='chart-title'>Pavement Cross-Section &mdash; " & WebEncode(det.ACName) & "</text>")

            ' === Left panel: Layer stack ===
            Dim yOff As Double = topY
            For i As Integer = 0 To layers.Count - 1
                Dim lyr = layers(i)
                Dim h As Double
                If i < layers.Count - 1 Then
                    h = lyr.Thickness * pxPerIn
                Else
                    h = availH - (yOff - topY) ' subgrade fills rest
                End If
                Dim clrIdx = Math.Min(i, layerColors.Length - 1)
                sb.AppendLine("<rect x='" & leftX & "' y='" & Fmt(yOff) & "' width='" & leftW & "' height='" & Fmt(h) & "' fill='" & layerColors(clrIdx) & "' opacity='0.7' stroke='#444' stroke-width='0.5'/>")

                ' Label inside layer
                Dim label As String
                If i < layers.Count - 1 Then
                    label = "Layer " & (i + 1).ToString() & ": " & Format(lyr.Thickness, "0.0") & " " & thkUnit & ", E=" & Format(lyr.Modulus, "#,##0")
                Else
                    label = "Subgrade: E=" & Format(lyr.Modulus, "#,##0") & " " & "psi"
                End If
                If h > 16 Then
                    sb.AppendLine("<text x='" & Fmt(leftX + leftW / 2) & "' y='" & Fmt(yOff + h / 2 + 4) & "' text-anchor='middle' fill='white' style='font-size:13px;font-weight:600'>" & label & "</text>")
                End If

                ' Dimension annotation (right of layer stack)
                If i < layers.Count - 1 Then
                    sb.AppendLine("<text x='" & (leftX + leftW + 8) & "' y='" & Fmt(yOff + h / 2 + 4) & "' class='tick'>" & Format(lyr.Thickness, "0.00") & """</text>")
                End If
                yOff += h
            Next

            ' === Right panel: Stress projection (multi-tire with dual partner inference) ===
            Dim subgradeY = topY + totalDepth * pxPerIn
            ' Background
            sb.AppendLine("<rect x='" & rightX & "' y='" & topY & "' width='" & rightW & "' height='" & Fmt(subgradeY - topY) & "' fill='#96969620' rx='2'/>")

            ' Surface and subgrade lines
            sb.AppendLine("<line x1='" & rightX & "' y1='" & topY & "' x2='" & (rightX + rightW) & "' y2='" & topY & "' stroke='#333' stroke-width='2'/>")
            sb.AppendLine("<text x='" & (rightX + 5) & "' y='" & (topY - 5) & "' class='tick'>Surface</text>")
            sb.AppendLine("<line x1='" & rightX & "' y1='" & Fmt(subgradeY) & "' x2='" & (rightX + rightW) & "' y2='" & Fmt(subgradeY) & "' stroke='#789A5A' stroke-width='2'/>")
            sb.AppendLine("<text x='" & (rightX + 5) & "' y='" & Fmt(subgradeY + 14) & "' class='tick' fill='#789A5A'>Subgrade (Eval Depth = " & Format(rpt.SublayerData.EvalDepthSubgrade, "0.0") & " " & thkUnit & ")</text>")

            ' Build list of all physical tires (with dual partner inference)
            Dim allTiresX As New List(Of Single)
            For i As Integer = 1 To det.NWheels
                allTiresX.Add(det.WheelX(i))
            Next
            If det.DualSpacing > 0 Then
                Dim partnersX As New List(Of Single)
                For i As Integer = 0 To allTiresX.Count - 1
                    Dim partnerX As Single = allTiresX(i) + det.DualSpacing
                    Dim hasPartner As Boolean = False
                    For j As Integer = 0 To allTiresX.Count - 1
                        If j <> i AndAlso Math.Abs(allTiresX(j) - partnerX) < 1 Then
                            hasPartner = True : Exit For
                        End If
                    Next
                    If Not hasPartner Then partnersX.Add(partnerX)
                Next
                allTiresX.AddRange(partnersX)
            End If

            ' Symmetric mirror inference: for any wheel at x where |x| > 1,
            ' if no wheel exists near -x, add the mirror partner.
            ' Reconstructs D-type dual gears where only one wheel is captured.
            Dim mirrorsXcs As New List(Of Single)
            For i As Integer = 0 To allTiresX.Count - 1
                If Math.Abs(allTiresX(i)) > 1 Then
                    Dim mirrorX As Single = -allTiresX(i)
                    Dim hasMirror As Boolean = False
                    For j As Integer = 0 To allTiresX.Count - 1
                        If Math.Abs(allTiresX(j) - mirrorX) < 1 Then hasMirror = True : Exit For
                    Next
                    If Not hasMirror Then mirrorsXcs.Add(mirrorX)
                End If
            Next
            allTiresX.AddRange(mirrorsXcs)

            ' Tire colors
            Dim tireColors() As String = {"#1F77B4", "#FF7F0E", "#2CA02C", "#D62728", "#9467BD", "#8C564B"}

            Dim midX = rightX + rightW / 2
            Dim nTires As Integer = allTiresX.Count

            If nTires <= 1 Then
                ' Single tire — use original layout
                Dim tireWidthPx = Math.Max(20, Math.Min(rightW * 0.3, det.TireWidth * pxPerIn * 0.5))
                sb.AppendLine("<rect x='" & Fmt(midX - tireWidthPx / 2) & "' y='" & Fmt(topY - 12) & "' width='" & Fmt(tireWidthPx) & "' height='12' fill='#333' rx='2'/>")
                sb.AppendLine("<text x='" & Fmt(midX) & "' y='" & Fmt(topY - 16) & "' text-anchor='middle' class='tick' fill='#FF7F0E'>TW=" & Format(det.TireWidth, "0.0") & """</text>")

                Dim projWidthPx = det.ProjectedTireWidthAtSubgrade * pxPerIn * 0.5
                projWidthPx = Math.Max(tireWidthPx + 20, Math.Min(rightW * 0.85, projWidthPx))
                sb.AppendLine("<line x1='" & Fmt(midX - tireWidthPx / 2) & "' y1='" & topY & "' x2='" & Fmt(midX - projWidthPx / 2) & "' y2='" & Fmt(subgradeY) & "' stroke='#D62728' stroke-width='1.5' stroke-dasharray='6,3' opacity='0.7'/>")
                sb.AppendLine("<line x1='" & Fmt(midX + tireWidthPx / 2) & "' y1='" & topY & "' x2='" & Fmt(midX + projWidthPx / 2) & "' y2='" & Fmt(subgradeY) & "' stroke='#D62728' stroke-width='1.5' stroke-dasharray='6,3' opacity='0.7'/>")
                sb.AppendLine("<line x1='" & Fmt(midX - projWidthPx / 2) & "' y1='" & Fmt(subgradeY) & "' x2='" & Fmt(midX + projWidthPx / 2) & "' y2='" & Fmt(subgradeY) & "' stroke='#D62728' stroke-width='2'/>")
                sb.AppendLine("<text x='" & Fmt(midX) & "' y='" & Fmt(subgradeY + 28) & "' text-anchor='middle' class='tick' fill='#D62728'>Proj. Width = " & Format(det.ProjectedTireWidthAtSubgrade, "0.0") & " " & lenUnit & "</text>")

                ' Gaussian wander bell curve (above surface)
                Dim sigma As Double = 30.435
                Dim gaussH As Double = 35
                Dim gaussPath As New StringBuilder("M")
                Dim nG As Integer = 100
                For i As Integer = 0 To nG
                    Dim xPx = rightX + i * rightW / nG
                    Dim xInches = (xPx - midX) / (pxPerIn * 0.5)
                    Dim gVal = Math.Exp(-0.5 * (xInches / sigma) ^ 2)
                    Dim yPx = topY - 15 - gVal * gaussH
                    If i = 0 Then gaussPath.Append(Fmt(xPx) & " " & Fmt(yPx)) Else gaussPath.Append(" L" & Fmt(xPx) & " " & Fmt(yPx))
                Next
                sb.AppendLine("<path d='" & gaussPath.ToString() & "' fill='none' stroke='#1F77B4' stroke-width='1.5' opacity='0.6'/>")
                sb.AppendLine("<text x='" & Fmt(midX + 30) & "' y='" & Fmt(topY - 15 - gaussH + 5) & "' class='tick' fill='#1F77B4'>&sigma;=30.4""</text>")
            Else
                ' Multi-tire — compute positions relative to center of all tires
                Dim centerX As Single = 0
                For i As Integer = 0 To allTiresX.Count - 1
                    centerX += allTiresX(i)
                Next
                centerX /= allTiresX.Count

                ' Determine scale: fit all tires + projection cones within rightW
                Dim maxSpread As Single = 0
                For i As Integer = 0 To allTiresX.Count - 1
                    Dim dist = Math.Abs(allTiresX(i) - centerX) + det.TireWidth / 2 + CSng(totalDepth)
                    If dist > maxSpread Then maxSpread = CSng(dist)
                Next
                maxSpread = Math.Max(maxSpread, 50)
                Dim projScale As Double = (rightW * 0.45) / maxSpread

                ' Draw each tire and its projection cone
                For t As Integer = 0 To allTiresX.Count - 1
                    Dim relX As Double = (allTiresX(t) - centerX) * projScale
                    Dim tireCenterPx As Double = midX + relX
                    Dim tireWidthPx As Double = Math.Max(12, det.TireWidth * projScale)
                    Dim clr As String = tireColors(t Mod tireColors.Length)

                    ' Tire rectangle at surface
                    sb.AppendLine("<rect x='" & Fmt(tireCenterPx - tireWidthPx / 2) & "' y='" & Fmt(topY - 12) & "' width='" & Fmt(tireWidthPx) & "' height='12' fill='" & clr & "' rx='2' opacity='0.85'/>")

                    ' Tire width label
                    sb.AppendLine("<text x='" & Fmt(tireCenterPx) & "' y='" & Fmt(topY - 16) & "' text-anchor='middle' style='font-size:13px' fill='" & clr & "'>TW=" & Format(det.TireWidth, "0.0") & """</text>")

                    ' Projection cone (stress spread lines from tire edges to subgrade)
                    Dim projHalfW As Double = tireWidthPx / 2 + (subgradeY - topY) ' 1:1 spread in px
                    ' Clamp projection width
                    projHalfW = Math.Min(projHalfW, rightW * 0.48)

                    sb.AppendLine("<line x1='" & Fmt(tireCenterPx - tireWidthPx / 2) & "' y1='" & topY & "' x2='" & Fmt(tireCenterPx - projHalfW) & "' y2='" & Fmt(subgradeY) & "' stroke='" & clr & "' stroke-width='1.2' stroke-dasharray='6,3' opacity='0.6'/>")
                    sb.AppendLine("<line x1='" & Fmt(tireCenterPx + tireWidthPx / 2) & "' y1='" & topY & "' x2='" & Fmt(tireCenterPx + projHalfW) & "' y2='" & Fmt(subgradeY) & "' stroke='" & clr & "' stroke-width='1.2' stroke-dasharray='6,3' opacity='0.6'/>")

                    ' Projected width bar at subgrade
                    sb.AppendLine("<line x1='" & Fmt(tireCenterPx - projHalfW) & "' y1='" & Fmt(subgradeY) & "' x2='" & Fmt(tireCenterPx + projHalfW) & "' y2='" & Fmt(subgradeY) & "' stroke='" & clr & "' stroke-width='1.5' opacity='0.7'/>")
                Next

                ' Overlap/gap annotation at subgrade
                If allTiresX.Count = 2 Then
                    Dim spacing As Double = Math.Abs(allTiresX(1) - allTiresX(0))
                    Dim projectedWidth As Double = det.TireWidth + 2 * totalDepth ' Each tire projects TW + 2*depth at subgrade
                    Dim gapOrOverlap As Double = spacing - projectedWidth
                    Dim annotationY As Double = subgradeY + 28
                    If gapOrOverlap < 0 Then
                        sb.AppendLine("<text x='" & Fmt(midX) & "' y='" & Fmt(annotationY) & "' text-anchor='middle' class='tick' fill='#D62728'>Overlap = " & Format(Math.Abs(gapOrOverlap), "0.0") & " " & lenUnit & " at subgrade</text>")
                    ElseIf gapOrOverlap > 0 Then
                        sb.AppendLine("<text x='" & Fmt(midX) & "' y='" & Fmt(annotationY) & "' text-anchor='middle' class='tick' fill='#2CA02C'>Gap = " & Format(gapOrOverlap, "0.0") & " " & lenUnit & " at subgrade</text>")
                    Else
                        sb.AppendLine("<text x='" & Fmt(midX) & "' y='" & Fmt(annotationY) & "' text-anchor='middle' class='tick' fill='#333'>Projections just touching at subgrade</text>")
                    End If
                End If

                ' Per-tire Gaussian wander curves (above surface)
                Dim sigma As Double = 30.435
                Dim gaussH As Double = 30
                For t As Integer = 0 To allTiresX.Count - 1
                    Dim relX As Double = (allTiresX(t) - centerX) * projScale
                    Dim tireCenterPx As Double = midX + relX
                    Dim clr As String = tireColors(t Mod tireColors.Length)
                    Dim gaussPath As New StringBuilder("M")
                    Dim nG As Integer = 100
                    For i As Integer = 0 To nG
                        Dim xPx = rightX + i * rightW / nG
                        Dim xInches = (xPx - tireCenterPx) / (projScale)
                        Dim gVal = Math.Exp(-0.5 * (xInches / sigma) ^ 2)
                        Dim yPx = topY - 15 - gVal * gaussH
                        If i = 0 Then gaussPath.Append(Fmt(xPx) & " " & Fmt(yPx)) Else gaussPath.Append(" L" & Fmt(xPx) & " " & Fmt(yPx))
                    Next
                    sb.AppendLine("<path d='" & gaussPath.ToString() & "' fill='none' stroke='" & clr & "' stroke-width='1.2' opacity='0.5'/>")
                Next
                sb.AppendLine("<text x='" & Fmt(midX) & "' y='" & Fmt(topY - 15 - gaussH - 2) & "' text-anchor='middle' style='font-size:13px' fill='#666'>&sigma;=30.4"" per tire</text>")
            End If

            ' Depth annotation
            sb.AppendLine("<text x='" & (rightX + rightW + 5) & "' y='" & Fmt(topY + (subgradeY - topY) / 2 + 4) & "' class='tick'>D=" & Format(totalDepth, "0.0") & """</text>")

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Diagram: Gear Configuration"

        ''' <summary>
        ''' Appends an inline SVG plan-view gear configuration diagram showing wheel positions,
        ''' CDF offset strips, dimension annotations, and Gaussian wander overlay.
        ''' </summary>
        Private Shared Sub AppendGearConfigSVG(sb As StringBuilder, det As clsAircraftDetail, criticalOffset As Integer, lengthUnit As String)
            If det.NWheels = 0 OrElse det.WheelX Is Nothing Then
                sb.AppendLine("<p class='note-box'>Gear geometry data not available for this aircraft.</p>")
                Return
            End If

            Dim svgW As Integer = 1000, svgH As Integer = 550

            ' Build list of all physical tire positions (stored + inferred dual partners)
            Dim allTiresX As New List(Of Single)
            Dim allTiresY As New List(Of Single)
            For i As Integer = 1 To det.NWheels
                allTiresX.Add(det.WheelX(i))
                allTiresY.Add(det.WheelY(i))
            Next
            If det.DualSpacing > 0 Then
                Dim partnersX As New List(Of Single)
                Dim partnersY As New List(Of Single)
                For i As Integer = 0 To allTiresX.Count - 1
                    Dim partnerX As Single = allTiresX(i) + det.DualSpacing
                    Dim exists As Boolean = False
                    For j As Integer = 0 To allTiresX.Count - 1
                        If j <> i AndAlso Math.Abs(allTiresX(j) - partnerX) < 1 AndAlso Math.Abs(allTiresY(j) - allTiresY(i)) < 1 Then exists = True : Exit For
                    Next
                    If Not exists Then partnersX.Add(partnerX) : partnersY.Add(allTiresY(i))
                Next
                allTiresX.AddRange(partnersX)
                allTiresY.AddRange(partnersY)
            End If

            ' Symmetric mirror inference: for any wheel at (x, y) where |x| > 1,
            ' if no wheel exists near (-x, y), add the mirror partner.
            ' Reconstructs D-type dual gears where only one wheel is captured.
            Dim mirrorsX As New List(Of Single)
            Dim mirrorsY As New List(Of Single)
            For i As Integer = 0 To allTiresX.Count - 1
                If Math.Abs(allTiresX(i)) > 1 Then
                    Dim mirrorX As Single = -allTiresX(i)
                    Dim hasMirror As Boolean = False
                    For j As Integer = 0 To allTiresX.Count - 1
                        If Math.Abs(allTiresX(j) - mirrorX) < 1 AndAlso Math.Abs(allTiresY(j) - allTiresY(i)) < 1 Then
                            hasMirror = True : Exit For
                        End If
                    Next
                    If Not hasMirror Then mirrorsX.Add(mirrorX) : mirrorsY.Add(allTiresY(i))
                End If
            Next
            allTiresX.AddRange(mirrorsX)
            allTiresY.AddRange(mirrorsY)

            ' Find coordinate ranges (using all physical tires)
            Dim minX As Single = Single.MaxValue, maxX As Single = Single.MinValue
            Dim minY As Single = Single.MaxValue, maxY As Single = Single.MinValue
            For i As Integer = 0 To allTiresX.Count - 1
                If allTiresX(i) < minX Then minX = allTiresX(i)
                If allTiresX(i) > maxX Then maxX = allTiresX(i)
                If allTiresY(i) < minY Then minY = allTiresY(i)
                If allTiresY(i) > maxY Then maxY = allTiresY(i)
            Next
            Dim pad As Single = CSng(Math.Max(det.TireWidth * 2, 30))
            minX -= pad : maxX += pad
            minY -= pad : maxY += pad

            ' Extend X range for bilateral CDF strips
            Dim centerX As Single = (minX + pad + maxX - pad) / 2
            Dim bilateralExtent As Single = CSng((CDF.NOFF - 1) * CDF.OFFSETINC) + pad
            minX = Math.Min(minX, centerX - bilateralExtent)
            maxX = Math.Max(maxX, centerX + bilateralExtent)

            ' Plot area
            Dim mL As Integer = 80, mR As Integer = 30, mT As Integer = 30, mB As Integer = 50
            Dim pW As Integer = svgW - mL - mR
            Dim pH As Integer = svgH - mT - mB

            ' Uniform scale
            Dim rangeX As Single = Math.Max(maxX - minX, 1)
            Dim rangeY As Single = Math.Max(maxY - minY, 1)
            Dim scX As Single = CSng(pW / rangeX)
            Dim scY As Single = CSng(pH / rangeY)
            Dim sc As Single = Math.Min(scX, scY)
            Dim usedW As Single = rangeX * sc
            Dim usedH As Single = rangeY * sc
            Dim offX As Single = mL + (pW - usedW) / 2
            Dim offY As Single = mT + (pH - usedH) / 2

            Dim toPxX = Function(wx As Single) offX + (wx - minX) * sc
            Dim toPxY = Function(wy As Single) offY + (maxY - wy) * sc

            sb.AppendLine("<div class='chart-container-wide'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' style='max-width:" & svgW & "px;'>")
            sb.AppendLine("<title>Gear configuration diagram</title>")

            ' Arrowhead marker
            sb.AppendLine("<defs><marker id='arrowGear' markerWidth='6' markerHeight='4' refX='3' refY='2' orient='auto'>")
            sb.AppendLine("<polygon points='0 0, 6 2, 0 4' fill='#666'/></marker></defs>")

            ' Bilateral CDF offset strips (-40 to +40 = 81 strips)
            For ioff As Integer = -(CDF.NOFF - 1) To (CDF.NOFF - 1)
                Dim offsetInches As Single = CSng(ioff * CDF.OFFSETINC)
                Dim stripXpos As Single = centerX + offsetInches
                If stripXpos >= minX AndAlso stripXpos <= maxX Then
                    Dim px As Single = toPxX(stripXpos)
                    If Math.Abs(ioff) = criticalOffset - 1 AndAlso ioff <> 0 Then
                        sb.AppendLine("<line x1='" & Fmt(px) & "' y1='" & mT & "' x2='" & Fmt(px) & "' y2='" & (mT + pH) & "' stroke='#DC3232' stroke-width='1' stroke-dasharray='4,3' opacity='0.7'/>")
                        If ioff > 0 Then
                            Dim critInches As Integer = CInt((criticalOffset - 1) * CDF.OFFSETINC)
                            sb.AppendLine("<text x='" & Fmt(px) & "' y='" & (mT - 3) & "' text-anchor='middle' fill='red' style='font-size:13px'>Critical: &plusmn;" & critInches & """ </text>")
                        End If
                    ElseIf ioff <> 0 Then
                        sb.AppendLine("<line x1='" & Fmt(px) & "' y1='" & mT & "' x2='" & Fmt(px) & "' y2='" & (mT + pH) & "' stroke='#A0A0A0' stroke-width='0.5' stroke-dasharray='3,3' opacity='0.4'/>")
                    End If
                    If ioff Mod 10 = 0 AndAlso ioff <> 0 Then
                        Dim stripLabel = If(ioff > 0, "+" & Format(offsetInches, "0"), Format(offsetInches, "0"))
                        sb.AppendLine("<text x='" & Fmt(px) & "' y='" & (mT + pH + 14) & "' text-anchor='middle' fill='#5a6270' style='font-size:13px'>" & stripLabel & "</text>")
                    End If
                End If
            Next

            ' Zero-line (centerline)
            Dim zeroPx As Single = toPxX(centerX)
            sb.AppendLine("<line x1='" & Fmt(zeroPx) & "' y1='" & mT & "' x2='" & Fmt(zeroPx) & "' y2='" & (mT + pH) & "' stroke='black' stroke-width='1.5'/>")
            sb.AppendLine("<text x='" & Fmt(zeroPx) & "' y='" & (mT + pH + 14) & "' text-anchor='middle' fill='black' style='font-size:13px;font-weight:bold'>0</text>")

            ' Per-tire Gaussian wander overlays (one per physical tire)
            Dim sigma As Double = 30.435
            Dim gaussMax As Double = 1.0 / (sigma * Math.Sqrt(2 * Math.PI))
            Dim gaussH As Single = 35
            Dim tireGaussColors() As String = {"rgba(46,94,168,", "rgba(214,39,40,", "rgba(44,160,44,", "rgba(255,127,14,"}
            For iTire As Integer = 0 To allTiresX.Count - 1
                Dim tireCenter As Single = allTiresX(iTire)
                Dim gColorBase As String = tireGaussColors(iTire Mod tireGaussColors.Length)
                Dim gaussPath As New System.Text.StringBuilder()
                Dim firstGauss As Boolean = True
                For px As Integer = mL To mL + pW
                    Dim wx As Single = minX + CSng((px - offX) / sc)
                    Dim dist As Double = wx - tireCenter
                    Dim gVal As Double = Math.Exp(-dist * dist / (2 * sigma * sigma)) / (sigma * Math.Sqrt(2 * Math.PI))
                    Dim py As Single = CSng(mT + pH - (gVal / gaussMax) * gaussH)
                    If firstGauss Then
                        gaussPath.Append("M" & px & "," & Fmt(CSng(mT + pH)))
                        gaussPath.Append(" L" & px & "," & Fmt(py))
                        firstGauss = False
                    Else
                        gaussPath.Append(" L" & px & "," & Fmt(py))
                    End If
                Next
                gaussPath.Append(" L" & (mL + pW) & "," & Fmt(CSng(mT + pH)) & " Z")
                sb.AppendLine("<path d='" & gaussPath.ToString() & "' fill='" & gColorBase & "0.06)' stroke='" & gColorBase & "0.4)' stroke-width='1'/>")
            Next

            ' All physical tires
            Dim tireR As Single = det.TireWidth / 2
            For i As Integer = 0 To allTiresX.Count - 1
                Dim cx As Single = toPxX(allTiresX(i))
                Dim cy As Single = toPxY(allTiresY(i))
                Dim rPx As Single = tireR * sc
                rPx = CSng(Math.Max(rPx, 6))
                rPx = CSng(Math.Min(rPx, 35))
                Dim circleColorBase As String = tireGaussColors(i Mod tireGaussColors.Length)
                sb.AppendLine("<circle cx='" & Fmt(cx) & "' cy='" & Fmt(cy) & "' r='" & Fmt(rPx) & "' fill='" & circleColorBase & "0.45)' stroke='" & circleColorBase & "1)' stroke-width='1'/>")
                sb.AppendLine("<text x='" & Fmt(cx + rPx + 3) & "' y='" & Fmt(cy + 3) & "' style='font-size:13px'>(" & Format(allTiresX(i), "0.0") & ", " & Format(allTiresY(i), "0.0") & ")</text>")
            Next

            ' Dual spacing dimension
            If det.DualSpacing > 0 AndAlso allTiresX.Count >= 2 Then
                Dim w1 As Integer = 0, w2 As Integer = 1
                For i As Integer = 0 To allTiresX.Count - 2
                    For j As Integer = i + 1 To allTiresX.Count - 1
                        If Math.Abs(allTiresY(i) - allTiresY(j)) < 1 AndAlso Math.Abs(allTiresX(i) - allTiresX(j)) > 1 Then
                            w1 = i : w2 = j
                            GoTo FoundDualSVG
                        End If
                    Next
                Next
FoundDualSVG:
                Dim px1 As Single = toPxX(allTiresX(w1))
                Dim px2 As Single = toPxX(allTiresX(w2))
                Dim py As Single = toPxY(allTiresY(w1)) - tireR * sc - 12
                py = CSng(Math.Max(py, mT + 5))
                sb.AppendLine("<line x1='" & Fmt(px1) & "' y1='" & Fmt(py) & "' x2='" & Fmt(px2) & "' y2='" & Fmt(py) & "' stroke='#666' stroke-width='0.8'/>")
                sb.AppendLine("<line x1='" & Fmt(px1) & "' y1='" & Fmt(py - 3) & "' x2='" & Fmt(px1) & "' y2='" & Fmt(py + 3) & "' stroke='#666' stroke-width='0.8'/>")
                sb.AppendLine("<line x1='" & Fmt(px2) & "' y1='" & Fmt(py - 3) & "' x2='" & Fmt(px2) & "' y2='" & Fmt(py + 3) & "' stroke='#666' stroke-width='0.8'/>")
                sb.AppendLine("<text x='" & Fmt((px1 + px2) / 2) & "' y='" & Fmt(py - 4) & "' text-anchor='middle' fill='#666' style='font-size:13px'>" & Format(det.DualSpacing, "0.0") & """ dual</text>")
            End If

            ' Inferred dual spacing annotation (for mirror-inferred pairs where DualSpacing=0)
            If det.DualSpacing = 0 AndAlso allTiresX.Count >= 2 Then
                ' Find two tires at same Y with maximum X separation
                Dim bestI As Integer = -1, bestJ As Integer = -1
                Dim bestDist As Single = 0
                For i As Integer = 0 To allTiresX.Count - 2
                    For j As Integer = i + 1 To allTiresX.Count - 1
                        If Math.Abs(allTiresY(i) - allTiresY(j)) < 1 Then
                            Dim dist As Single = Math.Abs(allTiresX(i) - allTiresX(j))
                            If dist > bestDist Then bestDist = dist : bestI = i : bestJ = j
                        End If
                    Next
                Next
                If bestI >= 0 AndAlso bestDist > 1 Then
                    Dim px1 As Single = toPxX(allTiresX(bestI))
                    Dim px2 As Single = toPxX(allTiresX(bestJ))
                    Dim py As Single = toPxY(allTiresY(bestI)) - tireR * sc - 12
                    py = CSng(Math.Max(py, mT + 5))
                    sb.AppendLine("<line x1='" & Fmt(px1) & "' y1='" & Fmt(py) & "' x2='" & Fmt(px2) & "' y2='" & Fmt(py) & "' stroke='#666' stroke-width='0.8'/>")
                    sb.AppendLine("<line x1='" & Fmt(px1) & "' y1='" & Fmt(py - 3) & "' x2='" & Fmt(px1) & "' y2='" & Fmt(py + 3) & "' stroke='#666' stroke-width='0.8'/>")
                    sb.AppendLine("<line x1='" & Fmt(px2) & "' y1='" & Fmt(py - 3) & "' x2='" & Fmt(px2) & "' y2='" & Fmt(py + 3) & "' stroke='#666' stroke-width='0.8'/>")
                    sb.AppendLine("<text x='" & Fmt((px1 + px2) / 2) & "' y='" & Fmt(py - 4) & "' text-anchor='middle' fill='#666' style='font-size:13px'>" & Format(bestDist, "0.0") & """ spacing</text>")
                End If
            End If

            ' Tandem spacing dimension
            If det.TandemSpacing > 0 AndAlso allTiresX.Count >= 2 Then
                Dim w1 As Integer = 0, w2 As Integer = 1
                For i As Integer = 0 To allTiresX.Count - 2
                    For j As Integer = i + 1 To allTiresX.Count - 1
                        If Math.Abs(allTiresX(i) - allTiresX(j)) < 1 AndAlso Math.Abs(allTiresY(i) - allTiresY(j)) > 1 Then
                            w1 = i : w2 = j
                            GoTo FoundTandemSVG
                        End If
                    Next
                Next
FoundTandemSVG:
                Dim py1 As Single = toPxY(allTiresY(w1))
                Dim py2 As Single = toPxY(allTiresY(w2))
                Dim px As Single = toPxX(allTiresX(w1)) + tireR * sc + 15
                px = CSng(Math.Min(px, mL + pW - 5))
                sb.AppendLine("<line x1='" & Fmt(px) & "' y1='" & Fmt(py1) & "' x2='" & Fmt(px) & "' y2='" & Fmt(py2) & "' stroke='#666' stroke-width='0.8'/>")
                sb.AppendLine("<line x1='" & Fmt(px - 3) & "' y1='" & Fmt(py1) & "' x2='" & Fmt(px + 3) & "' y2='" & Fmt(py1) & "' stroke='#666' stroke-width='0.8'/>")
                sb.AppendLine("<line x1='" & Fmt(px - 3) & "' y1='" & Fmt(py2) & "' x2='" & Fmt(px + 3) & "' y2='" & Fmt(py2) & "' stroke='#666' stroke-width='0.8'/>")
                sb.AppendLine("<text x='" & Fmt(px + 5) & "' y='" & Fmt((py1 + py2) / 2 + 3) & "' fill='#666' style='font-size:13px'>" & Format(det.TandemSpacing, "0.0") & """ tandem</text>")
            End If

            ' Contact area annotation
            If det.ContactArea > 0 Then
                sb.AppendLine("<text x='" & mL & "' y='" & (mT + pH + 30) & "' fill='#666' style='font-size:13px'>Contact area: " & Format(det.ContactArea, "0.0") & " " & WebEncode(lengthUnit) & "&sup2;</text>")
            End If

            ' Sigma annotation
            sb.AppendLine("<text x='" & mL & "' y='" & (mT + pH + 42) & "' fill='#2E5EA8' style='font-size:13px'>&sigma; = 30.435 in. (Gaussian lateral wander)</text>")

            ' Axes
            sb.AppendLine("<line x1='" & mL & "' y1='" & mT & "' x2='" & mL & "' y2='" & (mT + pH) & "' stroke='#3a3f4a' stroke-width='1.3'/>")
            sb.AppendLine("<line x1='" & mL & "' y1='" & (mT + pH) & "' x2='" & (mL + pW) & "' y2='" & (mT + pH) & "' stroke='#3a3f4a' stroke-width='1.3'/>")
            sb.AppendLine("<text x='" & (mL + pW / 2) & "' y='" & (svgH - 5) & "' text-anchor='middle' style='font-size:14px'>Lateral position (" & WebEncode(lengthUnit) & ")</text>")

            ' Y-axis label
            sb.AppendLine("<text x='15' y='" & (mT + pH / 2) & "' text-anchor='middle' transform='rotate(-90,15," & (mT + pH / 2) & ")' style='font-size:14px'>Longitudinal (" & WebEncode(lengthUnit) & ")</text>")

            ' Title
            sb.AppendLine("<text x='" & (svgW / 2) & "' y='18' text-anchor='middle' style='font-size:17px;font-weight:bold'>Gear Configuration: " & WebEncode(det.ACName) & " (" & WebEncode(det.GearType) & ")</text>")

            ' Legend — per-wheel entries, collision detection, background
            Dim lgSwSz As Integer = 14
            Dim lgRowHeight As Integer = 20
            Dim lgPadding As Integer = 10
            Dim lgFontStyle As String = "font-family:'Segoe UI',system-ui,sans-serif;font-size:12px"

            ' Determine entry count (cap per-wheel at 4 tires for readability)
            Dim lgPerWheel As Boolean = allTiresX.Count > 1 AndAlso allTiresX.Count <= 4
            Dim nTireEnt As Integer = If(lgPerWheel, allTiresX.Count, 1)
            Dim nWanderEnt As Integer = If(lgPerWheel, allTiresX.Count, 1)
            Dim nLgEntries As Integer = nTireEnt + nWanderEnt + 3
            Dim lgWidth As Integer = 240
            Dim lgHeight As Integer = nLgEntries * lgRowHeight + lgPadding * 2

            ' Default position: upper-right
            Dim lgPosX As Integer = svgW - mR - lgWidth - 5
            Dim lgPosY As Integer = mT + 8

            ' Collision detection: check if any tire circle overlaps legend bounds
            Dim lgCollision As Boolean = False
            For i As Integer = 0 To allTiresX.Count - 1
                Dim tcx As Single = toPxX(allTiresX(i))
                Dim tcy As Single = toPxY(allTiresY(i))
                Dim trPx As Single = CSng(Math.Min(Math.Max(tireR * sc, 6), 35))
                If tcx + trPx > lgPosX AndAlso tcx - trPx < lgPosX + lgWidth AndAlso
                   tcy + trPx > lgPosY AndAlso tcy - trPx < lgPosY + lgHeight Then
                    lgCollision = True
                    Exit For
                End If
            Next
            If lgCollision Then lgPosX = mL + 5

            ' Legend background
            sb.AppendLine("<rect x='" & lgPosX & "' y='" & lgPosY & "' width='" & lgWidth & "' height='" & lgHeight & "' rx='3' fill='rgba(255,255,255,0.9)' stroke='#d5dce6'/>")

            Dim lgCY As Integer = lgPosY + lgPadding
            Dim lgSX As Integer = lgPosX + lgPadding
            Dim lgTX As Integer = lgSX + lgSwSz + 8

            ' Tire entries
            If lgPerWheel Then
                For t As Integer = 0 To allTiresX.Count - 1
                    Dim cBase As String = tireGaussColors(t Mod tireGaussColors.Length)
                    sb.AppendLine("<circle cx='" & (lgSX + lgSwSz \ 2) & "' cy='" & (lgCY + lgSwSz \ 2) & "' r='" & (lgSwSz \ 2) & "' fill='" & cBase & "0.45)' stroke='" & cBase & "1)'/>")
                    sb.AppendLine("<text x='" & lgTX & "' y='" & (lgCY + lgSwSz \ 2 + 4) & "' style='" & lgFontStyle & "'>Wheel " & (t + 1) & "</text>")
                    lgCY += lgRowHeight
                Next
            Else
                Dim cBase0 As String = tireGaussColors(0)
                sb.AppendLine("<circle cx='" & (lgSX + lgSwSz \ 2) & "' cy='" & (lgCY + lgSwSz \ 2) & "' r='" & (lgSwSz \ 2) & "' fill='" & cBase0 & "0.45)' stroke='" & cBase0 & "1)'/>")
                Dim tireLbl As String = If(allTiresX.Count > 4, "Tire patches (" & allTiresX.Count & " wheels)", "Tire contact patch")
                sb.AppendLine("<text x='" & lgTX & "' y='" & (lgCY + lgSwSz \ 2 + 4) & "' style='" & lgFontStyle & "'>" & tireLbl & "</text>")
                lgCY += lgRowHeight
            End If

            ' Wander entries
            If lgPerWheel Then
                For t As Integer = 0 To allTiresX.Count - 1
                    Dim cBase As String = tireGaussColors(t Mod tireGaussColors.Length)
                    sb.AppendLine("<rect x='" & lgSX & "' y='" & lgCY & "' width='" & lgSwSz & "' height='" & lgSwSz & "' fill='" & cBase & "0.08)' stroke='" & cBase & "0.4)'/>")
                    sb.AppendLine("<text x='" & lgTX & "' y='" & (lgCY + lgSwSz \ 2 + 4) & "' fill='" & cBase & "1)' style='" & lgFontStyle & "'>Wander — Wh." & (t + 1) & " (&sigma;=30.4"")</text>")
                    lgCY += lgRowHeight
                Next
            Else
                sb.AppendLine("<rect x='" & lgSX & "' y='" & lgCY & "' width='" & lgSwSz & "' height='" & lgSwSz & "' fill='rgba(46,94,168,0.08)' stroke='rgba(46,94,168,0.4)'/>")
                sb.AppendLine("<text x='" & lgTX & "' y='" & (lgCY + lgSwSz \ 2 + 4) & "' fill='#2E5EA8' style='" & lgFontStyle & "'>Lateral wander (&sigma;=30.4"")</text>")
                lgCY += lgRowHeight
            End If

            ' Evaluation strips
            sb.AppendLine("<line x1='" & lgSX & "' y1='" & (lgCY + lgSwSz \ 2) & "' x2='" & (lgSX + lgSwSz) & "' y2='" & (lgCY + lgSwSz \ 2) & "' stroke='#A0A0A0' stroke-width='1' stroke-dasharray='3,3'/>")
            sb.AppendLine("<text x='" & lgTX & "' y='" & (lgCY + lgSwSz \ 2 + 4) & "' fill='#5a6270' style='" & lgFontStyle & "'>Eval. strips (" & Format(CDF.OFFSETINC, "0") & """ spacing)</text>")
            lgCY += lgRowHeight

            ' Critical strip
            sb.AppendLine("<line x1='" & lgSX & "' y1='" & (lgCY + lgSwSz \ 2) & "' x2='" & (lgSX + lgSwSz) & "' y2='" & (lgCY + lgSwSz \ 2) & "' stroke='#DC3232' stroke-width='1.5' stroke-dasharray='4,3'/>")
            sb.AppendLine("<text x='" & lgTX & "' y='" & (lgCY + lgSwSz \ 2 + 4) & "' fill='red' style='" & lgFontStyle & "'>Critical strip (max CDF)</text>")
            lgCY += lgRowHeight

            ' Centerline
            sb.AppendLine("<line x1='" & lgSX & "' y1='" & (lgCY + lgSwSz \ 2) & "' x2='" & (lgSX + lgSwSz) & "' y2='" & (lgCY + lgSwSz \ 2) & "' stroke='black' stroke-width='1.5'/>")
            sb.AppendLine("<text x='" & lgTX & "' y='" & (lgCY + lgSwSz \ 2 + 4) & "' style='" & lgFontStyle & "'>Wheel path centerline (0)</text>")

            sb.AppendLine("</svg></div>")
        End Sub

#End Region

#Region "SVG Diagram: Coverage Concept"

        Private Shared Function NormalCDFApprox(x As Double) As Double
            ' Polynomial approximation of the standard normal CDF
            If x < -8 Then Return 0
            If x > 8 Then Return 1
            Dim t As Double = 1.0 / (1.0 + 0.2316419 * Math.Abs(x))
            Dim d As Double = 0.3989422804014327
            Dim p As Double = d * Math.Exp(-x * x / 2.0) * t * ((((1.330274429 * t - 1.821255978) * t + 1.781477937) * t - 0.356563782) * t + 0.31938153)
            If x >= 0 Then Return 1 - p Else Return p
        End Function

        Private Shared Function GaussArea(a As Double, b As Double, sigma As Double) As Double
            Return NormalCDFApprox(b / sigma) - NormalCDFApprox(a / sigma)
        End Function

        Private Shared Sub AppendCoverageConceptSVG(sb As StringBuilder)
            Dim svgW As Integer = 850, svgH As Integer = 650
            Dim sigma As Double = 30.435
            Dim exTW As Double = 16 ' example tire width
            Dim dualSpacing As Double = 40 ' example dual spacing

            sb.AppendLine("<div class='chart-wrap'>")
            sb.AppendLine("<svg viewBox='0 0 " & svgW & " " & svgH & "' class='chart-svg' xmlns='http://www.w3.org/2000/svg' role='img' aria-label='Coverage-to-pass concept diagram showing Gaussian wander and single vs dual wheel comparison'>")
            sb.AppendLine("<title>Coverage-to-pass concept diagram</title>")

            ' Arrow marker defs for dimension lines
            sb.AppendLine("<defs>")
            sb.AppendLine("<marker id='arr-l' markerWidth='8' markerHeight='6' refX='0' refY='3' orient='auto'><path d='M0,3 L8,0 L8,6 Z' fill='#666'/></marker>")
            sb.AppendLine("<marker id='arr-r' markerWidth='8' markerHeight='6' refX='8' refY='3' orient='auto'><path d='M8,3 L0,0 L0,6 Z' fill='#666'/></marker>")
            sb.AppendLine("</defs>")

            ' === Panel A: Gaussian wander + dual tires ===
            Dim panelAY As Integer = 50
            sb.AppendLine("<text x='425' y='" & panelAY & "' text-anchor='middle' class='chart-title'>Panel A: Gaussian Lateral Wander</text>")

            Dim axisY = panelAY + 130
            Dim midX As Double = 425
            Dim pxPerIn As Double = 2.5

            ' Gaussian bell curve
            Dim gPath As New StringBuilder("M")
            Dim gFill As New StringBuilder("M")
            Dim gaussAmp As Double = 90
            For i As Integer = 0 To 200
                Dim xPx = midX - 250 + i * 2.5
                Dim xIn = (xPx - midX) / pxPerIn
                Dim gVal = Math.Exp(-0.5 * (xIn / sigma) ^ 2)
                Dim yPx = axisY - gVal * gaussAmp
                If i = 0 Then
                    gPath.Append(Fmt(xPx) & " " & Fmt(yPx))
                    gFill.Append(Fmt(xPx) & " " & axisY & " L" & Fmt(xPx) & " " & Fmt(yPx))
                Else
                    gPath.Append(" L" & Fmt(xPx) & " " & Fmt(yPx))
                    gFill.Append(" L" & Fmt(xPx) & " " & Fmt(yPx))
                End If
            Next
            gFill.Append(" L" & Fmt(midX + 250) & " " & axisY & " Z")
            sb.AppendLine("<path d='" & gFill.ToString() & "' fill='#1F77B4' opacity='0.08'/>")
            sb.AppendLine("<path d='" & gPath.ToString() & "' fill='none' stroke='#1F77B4' stroke-width='2'/>")

            ' Axis line
            sb.AppendLine("<line x1='" & Fmt(midX - 260) & "' y1='" & axisY & "' x2='" & Fmt(midX + 260) & "' y2='" & axisY & "' stroke='#666' stroke-width='1'/>")

            ' Sigma annotations
            For s As Integer = 1 To 3
                Dim xOff = s * sigma * pxPerIn
                sb.AppendLine("<line x1='" & Fmt(midX + xOff) & "' y1='" & axisY & "' x2='" & Fmt(midX + xOff) & "' y2='" & (axisY + 6) & "' stroke='#666'/>")
                sb.AppendLine("<line x1='" & Fmt(midX - xOff) & "' y1='" & axisY & "' x2='" & Fmt(midX - xOff) & "' y2='" & (axisY + 6) & "' stroke='#666'/>")
                sb.AppendLine("<text x='" & Fmt(midX + xOff) & "' y='" & (axisY + 16) & "' text-anchor='middle' class='tick'>" & s & "&sigma;</text>")
                sb.AppendLine("<text x='" & Fmt(midX - xOff) & "' y='" & (axisY + 16) & "' text-anchor='middle' class='tick'>-" & s & "&sigma;</text>")
            Next

            ' Two tire rectangles at ±(dualSpacing/2)
            Dim tirePx = exTW * pxPerIn
            Dim halfDualPx = (dualSpacing / 2) * pxPerIn
            ' Wheel 1 (left, blue)
            sb.AppendLine("<rect x='" & Fmt(midX - halfDualPx - tirePx / 2) & "' y='" & Fmt(axisY - 8) & "' width='" & Fmt(tirePx) & "' height='16' fill='#1F77B4' opacity='0.35' rx='2'/>")
            ' Wheel 2 (right, orange)
            sb.AppendLine("<rect x='" & Fmt(midX + halfDualPx - tirePx / 2) & "' y='" & Fmt(axisY - 8) & "' width='" & Fmt(tirePx) & "' height='16' fill='#FF7F0E' opacity='0.35' rx='2'/>")
            ' Wheel labels
            sb.AppendLine("<text x='" & Fmt(midX - halfDualPx) & "' y='" & Fmt(axisY + 30) & "' text-anchor='middle' class='tick' fill='#1F77B4'>Wheel 1</text>")
            sb.AppendLine("<text x='" & Fmt(midX + halfDualPx) & "' y='" & Fmt(axisY + 30) & "' text-anchor='middle' class='tick' fill='#FF7F0E'>Wheel 2</text>")

            ' Dual spacing dimension line
            Dim dimY = axisY + 42
            sb.AppendLine("<line x1='" & Fmt(midX - halfDualPx) & "' y1='" & dimY & "' x2='" & Fmt(midX + halfDualPx) & "' y2='" & dimY & "' stroke='#666' stroke-width='1' marker-start='url(#arr-l)' marker-end='url(#arr-r)'/>")
            sb.AppendLine("<text x='" & Fmt(midX) & "' y='" & Fmt(dimY - 4) & "' text-anchor='middle' class='tick' fill='#666'>Dual spacing = " & Format(dualSpacing, "0") & """</text>")

            ' Evaluation strip shading at offset 30"
            Dim evalOff As Double = 30
            Dim evalXL = midX + (evalOff - exTW / 2) * pxPerIn
            Dim evalXR = midX + (evalOff + exTW / 2) * pxPerIn
            sb.AppendLine("<rect x='" & Fmt(evalXL) & "' y='" & Fmt(axisY - gaussAmp) & "' width='" & Fmt(evalXR - evalXL) & "' height='" & Fmt(gaussAmp) & "' fill='#FF8C00' opacity='0.2'/>")
            sb.AppendLine("<line x1='" & Fmt(midX + evalOff * pxPerIn) & "' y1='" & Fmt(axisY - gaussAmp - 5) & "' x2='" & Fmt(midX + evalOff * pxPerIn) & "' y2='" & axisY & "' stroke='#D62728' stroke-dasharray='4,2' stroke-width='1'/>")
            sb.AppendLine("<text x='" & Fmt(midX + evalOff * pxPerIn) & "' y='" & Fmt(axisY - gaussAmp - 8) & "' text-anchor='middle' class='tick' fill='#D62728'>Eval strip @ 30""</text>")

            ' === Panel B: Single vs Dual C/P curves (bilateral) ===
            Dim panelBY = axisY + 125
            sb.AppendLine("<text x='425' y='" & Fmt(panelBY) & "' text-anchor='middle' class='chart-title'>Panel B: C/P Curves &mdash; Single vs Dual Wheel</text>")

            Dim cpPlotL As Integer = 80, cpPlotW As Integer = 690, cpPlotH As Integer = 200
            Dim cpPlotT = panelBY + 15
            sb.AppendLine("<rect x='" & cpPlotL & "' y='" & Fmt(cpPlotT) & "' width='" & cpPlotW & "' height='" & cpPlotH & "' fill='#FAFBFC' stroke='#bcc3cc'/>")

            ' Bilateral X range for Panel B
            Dim cpXMin As Double = -200, cpXMax As Double = 200, cpXRng As Double = 400

            ' Compute max C/P for scaling (bilateral range)
            Dim maxCPAny As Double = 0
            For offs As Integer = -200 To 200
                Dim cpSingle = GaussArea(offs - exTW / 2, offs + exTW / 2, sigma)
                Dim cpDual = GaussArea(offs - dualSpacing / 2 - exTW / 2, offs - dualSpacing / 2 + exTW / 2, sigma) +
                             GaussArea(offs + dualSpacing / 2 - exTW / 2, offs + dualSpacing / 2 + exTW / 2, sigma)
                maxCPAny = Math.Max(maxCPAny, Math.Max(cpSingle, cpDual))
            Next
            Dim cpScale = cpPlotH * 0.85 / maxCPAny

            ' Single wheel curve (bilateral)
            Dim singlePath As New StringBuilder("M")
            For offs As Integer = -200 To 200
                Dim cp = GaussArea(offs - exTW / 2, offs + exTW / 2, sigma)
                Dim xP = cpPlotL + ((offs - cpXMin) / cpXRng) * cpPlotW
                Dim yP = cpPlotT + cpPlotH - cp * cpScale
                If offs = -200 Then singlePath.Append(Fmt(xP) & " " & Fmt(yP)) Else singlePath.Append(" L" & Fmt(xP) & " " & Fmt(yP))
            Next
            sb.AppendLine("<path d='" & singlePath.ToString() & "' fill='none' stroke='#1F77B4' stroke-width='2'/>")

            ' Dual wheel curve (bilateral)
            Dim dualPath As New StringBuilder("M")
            For offs As Integer = -200 To 200
                Dim cp = GaussArea(offs - dualSpacing / 2 - exTW / 2, offs - dualSpacing / 2 + exTW / 2, sigma) +
                         GaussArea(offs + dualSpacing / 2 - exTW / 2, offs + dualSpacing / 2 + exTW / 2, sigma)
                Dim xP = cpPlotL + ((offs - cpXMin) / cpXRng) * cpPlotW
                Dim yP = cpPlotT + cpPlotH - cp * cpScale
                If offs = -200 Then dualPath.Append(Fmt(xP) & " " & Fmt(yP)) Else dualPath.Append(" L" & Fmt(xP) & " " & Fmt(yP))
            Next
            sb.AppendLine("<path d='" & dualPath.ToString() & "' fill='none' stroke='#FF7F0E' stroke-width='2'/>")

            ' Individual dual wheel contributions (dashed, bilateral)
            For wSign As Integer = -1 To 1 Step 2
                Dim wPath As New StringBuilder("M")
                For offs As Integer = -200 To 200
                    Dim cp = GaussArea(offs + wSign * dualSpacing / 2 - exTW / 2, offs + wSign * dualSpacing / 2 + exTW / 2, sigma)
                    Dim xP = cpPlotL + ((offs - cpXMin) / cpXRng) * cpPlotW
                    Dim yP = cpPlotT + cpPlotH - cp * cpScale
                    If offs = -200 Then wPath.Append(Fmt(xP) & " " & Fmt(yP)) Else wPath.Append(" L" & Fmt(xP) & " " & Fmt(yP))
                Next
                sb.AppendLine("<path d='" & wPath.ToString() & "' fill='none' stroke='#FF7F0E' stroke-width='1' stroke-dasharray='4,3' opacity='0.5'/>")
            Next

            ' Centerline in Panel B
            Dim clX = cpPlotL + (0 - cpXMin) / cpXRng * cpPlotW
            sb.AppendLine("<line x1='" & Fmt(clX) & "' y1='" & Fmt(cpPlotT) & "' x2='" & Fmt(clX) & "' y2='" & Fmt(cpPlotT + cpPlotH) & "' stroke='#000' stroke-width='0.8' stroke-dasharray='4,3'/>")

            ' X axis ticks for Panel B (bilateral: -200 to +200, step 50)
            For i As Integer = -4 To 4
                Dim val = i * 50
                Dim xT = cpPlotL + ((val - cpXMin) / cpXRng) * cpPlotW
                sb.AppendLine("<text x='" & Fmt(xT) & "' y='" & Fmt(cpPlotT + cpPlotH + 14) & "' text-anchor='middle' class='tick'>" & val & "</text>")
            Next
            sb.AppendLine("<text x='" & Fmt(cpPlotL + cpPlotW / 2) & "' y='" & Fmt(cpPlotT + cpPlotH + 28) & "' text-anchor='middle' class='axis-label'>Offset from centerline (in.)</text>")
            sb.AppendLine("<text x='" & (cpPlotL - 10) & "' y='" & Fmt(cpPlotT + cpPlotH / 2) & "' text-anchor='middle' class='axis-label' transform='rotate(-90," & (cpPlotL - 10) & "," & Fmt(cpPlotT + cpPlotH / 2) & ")'>C/P</text>")

            ' Legend
            Dim legY = cpPlotT + 10
            sb.AppendLine("<line x1='" & (cpPlotL + cpPlotW - 160) & "' y1='" & Fmt(legY + 6) & "' x2='" & (cpPlotL + cpPlotW - 140) & "' y2='" & Fmt(legY + 6) & "' stroke='#1F77B4' stroke-width='2'/>")
            sb.AppendLine("<text x='" & (cpPlotL + cpPlotW - 135) & "' y='" & Fmt(legY + 10) & "' class='legend-text'>Single wheel</text>")
            sb.AppendLine("<line x1='" & (cpPlotL + cpPlotW - 160) & "' y1='" & Fmt(legY + 22) & "' x2='" & (cpPlotL + cpPlotW - 140) & "' y2='" & Fmt(legY + 22) & "' stroke='#FF7F0E' stroke-width='2'/>")
            sb.AppendLine("<text x='" & (cpPlotL + cpPlotW - 135) & "' y='" & Fmt(legY + 26) & "' class='legend-text'>Dual wheel (sum)</text>")
            sb.AppendLine("<line x1='" & (cpPlotL + cpPlotW - 160) & "' y1='" & Fmt(legY + 38) & "' x2='" & (cpPlotL + cpPlotW - 140) & "' y2='" & Fmt(legY + 38) & "' stroke='#FF7F0E' stroke-width='1' stroke-dasharray='4,3' opacity='0.5'/>")
            sb.AppendLine("<text x='" & (cpPlotL + cpPlotW - 135) & "' y='" & Fmt(legY + 42) & "' class='legend-text'>Individual wheels</text>")

            ' Border
            sb.AppendLine("<rect x='1' y='1' width='" & (svgW - 2) & "' height='" & (svgH - 2) & "' fill='none' stroke='#d1d5db' rx='4'/>")

            sb.AppendLine("</svg></div>")
            ' Caption now handled by <figcaption> wrapping in Generate()
        End Sub

#End Region

#Region "Number Formatting Helper"

        Private Shared Function Fmt(v As Double) As String
            Return Format(v, "0.#")
        End Function

        ''' <summary>
        ''' Formats CDF values for SVG Y-axis tick labels.
        ''' Uses scientific notation with SVG tspan superscript for very small values.
        ''' </summary>
        Private Shared Function FmtCDFSvg(val As Double) As String
            If val = 0 Then Return "0"
            If Math.Abs(val) >= 0.001 Then Return Format(val, "0.000000")
            Dim exp As Integer = CInt(Math.Floor(Math.Log10(Math.Abs(val))))
            Dim mantissa As Double = val / Math.Pow(10, exp)
            Return Format(mantissa, "0.00") & Chr(215) & "10<tspan dy='-4' font-size='7'>" & exp.ToString() & "</tspan><tspan dy='4'> </tspan>"
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
  --text-light: #556270;
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
  color: #2c3e50;
  background: #ffffff;
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
  border-bottom: 3px solid #1a3c6e;
  margin-bottom: 30px;
}
.report-header h1 {
  color: #1a3c6e;
  font-size: 22px;
  font-weight: 700;
  margin-bottom: 4px;
}
.subtitle { color: #556270; font-size: 12px; margin-bottom: 12px; }
.header-meta {
  display: flex; justify-content: center; flex-wrap: wrap;
  font-size: 13px; color: #556270;
}
.header-meta > div { margin: 0 12px; }
.header-meta > div + div {
  padding-left: 20px;
  border-left: 1px solid #ccc;
}

/* Dashboard cards */
.dashboard {
  display: flex;
  flex-wrap: wrap;
  margin-bottom: 30px;
}
.dashboard .card { margin: 0 6px 12px 6px; }
.card {
  flex: 1 1 155px;
  max-width: 200px;
  background: #f8f9fb;
  border: 1px solid #d5dce6;
  border-radius: 6px;
  padding: 14px 16px;
  text-align: center;
  transition: box-shadow 0.15s ease;
}
.card:hover { box-shadow: 0 2px 8px rgba(0,0,0,0.08); }
.card.success { border-top: 3px solid #2CA02C; }
.card.danger { border-top: 3px solid #D62728; }
.card-label { font-size: 12px; text-transform: uppercase; letter-spacing: 0.6px; color: #4B5563; margin-bottom: 6px; }
.card-value { font-size: 20px; font-weight: 700; color: #1a3c6e; }
.card-unit { font-size: 12px; font-weight: 400; color: #556270; }

/* TOC */
.toc {
  background: #f8f9fb;
  border: 1px solid #d5dce6;
  border-radius: 6px;
  padding: 20px 28px;
  margin-bottom: 30px;
}
.toc h2 { font-size: 16px; margin-bottom: 10px; color: #1a3c6e; }
.toc ol { padding-left: 20px; -webkit-column-count: 2; column-count: 2; -webkit-column-gap: 30px; column-gap: 30px; }
.toc li { margin-bottom: 4px; font-size: 13px; break-inside: avoid; }
.toc a { color: #1a3c6e; text-decoration: none; }
.toc a:hover { text-decoration: underline; }

/* Section headings */
section { margin-bottom: 36px; page-break-inside: avoid; }
section + section { border-top: 1px solid #d5dce6; padding-top: 30px; }
section > h2 {
  font-size: 18px;
  color: #1a3c6e;
  border-bottom: 2px solid #e8eef6;
  padding-bottom: 10px;
  margin-bottom: 18px;
}
.sec-num {
  display: inline-block;
  background: #1a3c6e;
  color: white;
  width: 26px; height: 26px;
  text-align: center;
  line-height: 26px;
  border-radius: 50%;
  font-size: 13px;
  margin-right: 8px;
  vertical-align: middle;
}
h3 { font-size: 15px; color: #2c3e50; margin: 18px 0 10px; }
h4 { font-size: 14px; color: #556270; margin: 14px 0 8px; }

/* Tables */
.data-table {
  width: 100%;
  border-collapse: collapse;
  margin-bottom: 16px;
  font-size: 13px;
}
.data-table th {
  background: #1a3c6e !important;
  color: white !important;
  padding: 8px 10px;
  text-align: left;
  font-weight: 600;
  font-size: 12px;
  white-space: nowrap;
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
}
thead { display: table-header-group; }
.data-table td {
  padding: 8px 12px;
  border-bottom: 1px solid #d5dce6;
}
.data-table tbody tr { transition: background 0.15s ease; }
.data-table tbody tr:hover { background: #e8eef6; }
.data-table tbody tr:nth-child(even) { background: #f8f9fb; }
.data-table tbody tr:nth-child(even):hover { background: #e8eef6; }
.data-table.compact td, .data-table.compact th { padding: 6px 10px; font-size: 12px; }
.data-table.centered th, .data-table.centered td { text-align: center; }
.highlight { background: #fff8e1 !important; }
.highlight td { font-weight: 600; }
.table-scroll { overflow-x: auto; margin-bottom: 16px; }
.table-scroll th:first-child, .table-scroll td:first-child {
  position: relative; left: 0; background: white; z-index: 1;
}
.table-scroll tr:nth-child(even) td:first-child { background: #f8f9fb; }
.table-scroll .highlight td:first-child { background: #fff8e1; }
.param-table td:first-child { font-weight: 600; white-space: nowrap; }

/* Equation cards */
.equation-card {
  background: linear-gradient(135deg, #f0f4fa 0%, #e8eef6 100%);
  border-left: 4px solid #1a3c6e;
  border-radius: 0 6px 6px 0;
  padding: 16px 20px;
  margin-bottom: 14px;
}
.equation-card h4 { color: #1a3c6e; margin: 0 0 8px; font-size: 13px; }
.eq {
  font-family: 'Cambria Math', 'Latin Modern Math', 'STIX Two Math', serif;
  font-size: 15px;
  margin: 4px 0;
  padding: 2px 0;
}
.eq-note { font-size: 12px; color: #556270; margin-top: 8px; border-top: 1px dashed #d5dce6; padding-top: 6px; }

/* Callouts */
.callout {
  border-radius: 6px;
  padding: 14px 18px;
  margin-bottom: 16px;
  font-size: 13px;
  line-height: 1.5;
}
.callout.info {
  background: #e8eef6;
  border-left: 4px solid #1a3c6e;
}
.callout.warn {
  background: #fff8e1;
  border-left: 4px solid #FF7F0E;
}
.alert {
  background: #fdecea;
  border: 1px solid #D62728;
  border-radius: 6px;
  padding: 20px;
  text-align: center;
  color: #D62728;
  font-weight: 600;
}

/* Figures */
figure { margin: 16px 0; text-align: center; }
figcaption { font-size: 13px; color: #4B5563; margin-top: 8px; font-style: italic; }

/* Charts */
.chart-wrap {
  margin: 16px 0;
  text-align: center;
}
.chart-container-wide { margin: 16px 0; text-align: center; }
.chart-wrap svg { width: 100%; }
.chart-svg {
  width: 100%;
  max-width: 900px;
  height: auto;
  border: 1px solid #d5dce6;
  border-radius: 6px;
  background: white;
}
.chart-svg .chart-title { font: bold 17px 'Segoe UI', sans-serif; fill: #2c3e50; }
.chart-svg .axis-label { font: 600 15px 'Segoe UI', sans-serif; fill: #2c3e50; }
.chart-svg .tick { font: 13px 'Segoe UI', sans-serif; fill: #556270; }
.chart-svg .label { font: 14px 'Segoe UI', sans-serif; fill: #2c3e50; }
.chart-svg .legend-text { font: 13px 'Segoe UI', sans-serif; fill: #2c3e50; }
.chart-svg circle:hover { opacity: 0.9; stroke-width: 2.5; cursor: pointer; }
.chart-svg rect.bar:hover { opacity: 0.9; cursor: pointer; }

/* Steps */
.steps {
  background: #f8f9fb;
  border: 1px solid #d5dce6;
  border-radius: 6px;
  padding: 16px 20px;
  margin: 12px 0;
}
.steps h4 { margin: 0 0 10px; }
.steps ol { padding-left: 24px; }
.steps li { margin-bottom: 6px; font-size: 13px; }

/* Aircraft block */
.aircraft-block {
  border: 1px solid #d5dce6;
  border-radius: 6px;
  padding: 20px;
  margin-bottom: 24px;
}
.aircraft-block h3 {
  color: #1a3c6e;
  border-bottom: 1px solid #d5dce6;
  padding-bottom: 6px;
  margin: 0 0 14px;
}

/* Collapsible details */
details {
  border: 1px solid #d5dce6;
  border-radius: 6px;
  margin-bottom: 12px;
}
details summary {
  padding: 10px 14px;
  cursor: pointer;
  font-weight: 600;
  font-size: 13px;
  background: #f8f9fb;
  border-radius: 6px;
}
details[open] summary { border-bottom: 1px solid #d5dce6; border-radius: 6px 6px 0 0; }
details > table, details > div { margin: 0; }

/* Buttons */
.btn-action {
  background: #1a3c6e; color: white; border: none;
  padding: 8px 16px; border-radius: 6px; cursor: pointer;
  font-size: 12px; margin-left: 12px;
}
.btn-action:hover { opacity: 0.85; }
.btn-top {
  position: fixed; bottom: 30px; right: 30px;
  background: #1a3c6e; color: white; border: none;
  width: 40px; height: 40px; border-radius: 50%;
  font-size: 18px; cursor: pointer; display: none;
  box-shadow: 0 2px 8px rgba(0,0,0,0.2);
}

/* Footer */
footer {
  text-align: center;
  padding: 20px 0;
  border-top: 2px solid #d5dce6;
  margin-top: 40px;
  color: #556270;
  font-size: 12px;
}
footer a { color: #1a3c6e; text-decoration: none; }

/* Sublayer modulus section */
.sublayer-modulus-section { margin-top: 24px; }
.sublayer-modulus-section h3 { color: #1a3c6e; border-bottom: 2px solid #1a3c6e; padding-bottom: 6px; }
.sublayer-main-eq { font-size: 16px !important; font-weight: 600; color: #1a3c6e; }
.callout.note {
  background: #e8f5e9;
  border-left: 4px solid #1a3c6e;
  border-radius: 6px;
  padding: 14px 18px;
  margin-bottom: 16px;
  font-size: 13px;
  line-height: 1.5;
}
.sublayer-detail { max-width: 100%; width: 100%; }
.sublayer-detail th, .sublayer-detail td { padding: 6px 8px; font-size: 12px; }
.sublayer-detail th { white-space: nowrap; }
.sublayer-detail td { font-variant-numeric: tabular-nums; text-align: right; }
.sublayer-detail td:first-child { text-align: left; white-space: nowrap; }
.sublayer-detail .ref-row { background: #fff8e1; font-style: italic; }
.modulus-depth-svg {
  width: 100%;
  max-width: 800px;
  margin: 16px auto;
  display: block;
  border: 1px solid #d5dce6;
  border-radius: 6px;
  background: white;
}
.modulus-depth-svg .tick-label { font-size: 13px; fill: #666; font-family: 'Segoe UI', sans-serif; }
.modulus-depth-svg .axis-label { font-size: 15px; fill: #444; font-family: 'Segoe UI', sans-serif; font-weight: 600; }
.modulus-depth-svg .small-label { font-size: 12px; font-family: 'Segoe UI', sans-serif; }
.modulus-depth-svg .mod-label { font-size: 13px; fill: #00796B; font-family: 'Segoe UI', sans-serif; font-weight: 600; }
.fig-caption { font-size: 13px; color: #4B5563; text-align: center; margin: 8px 0 20px; font-style: italic; }

/* Print / PDF */
@media print {
  body { max-width: 100%; padding: 10px; font-size: 12px; zoom: 1; }
  .toc { break-after: page; }
  section { break-inside: avoid; }
  .dashboard { justify-content: flex-start; }
  .card { flex: 0 1 180px; max-width: 200px; }
  svg { page-break-inside: avoid; }
  figure { page-break-inside: avoid; }
  details { border: none; }
  details > summary { display: none; }
  details > table, details > div { display: block !important; }
  .chart-svg { max-width: 100%; border: none; }
  svg { max-width: 100%; height: auto; shape-rendering: geometricPrecision; }
  figure { break-inside: avoid; page-break-inside: avoid; margin: 12px 0; }
  table { break-inside: avoid; page-break-inside: avoid; }
  h2, h3, h4 { break-after: avoid; page-break-after: avoid; }
  -webkit-print-color-adjust: exact;
  print-color-adjust: exact;
  color-adjust: exact;
  .callout, .equation-card, .steps { break-inside: avoid; page-break-inside: avoid; }
  .aircraft-block { break-inside: auto; page-break-inside: auto; }
  .aircraft-block + .aircraft-block { page-break-before: always; }
  .aircraft-block figure,
  .aircraft-block .steps,
  .aircraft-block table,
  .chart-wrap { break-inside: avoid; page-break-inside: avoid; }
  .cdf-comparison { break-inside: avoid; page-break-inside: avoid; }
  thead { display: table-header-group; }
  footer a[href]::after { content: none; }
  .btn-action, .btn-top { display: none !important; }
}

/* Asphalt CDF comparison */
.cdf-comparison {
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 24px 0;
  flex-wrap: wrap;
}
.cdf-comparison > * { margin: 0 10px; }
.cdf-compare-card {
  text-align: center;
  padding: 16px 28px;
  border-radius: 6px;
  min-width: 180px;
}
.subgrade-card { background: #e8f5e9; border: 2px solid #4caf50; }
.asphalt-card { background: #fff3e0; border: 2px solid #ff9800; }
.cdf-compare-label { font-size: 12px; text-transform: uppercase; letter-spacing: 0.5px; color: #556270; margin-bottom: 4px; }
.cdf-compare-value { font-size: 22px; font-weight: 700; font-family: 'Consolas', monospace; }
.cdf-compare-vs { font-size: 16px; font-weight: 600; color: #556270; }
.cdf-compare-governing { width: 100%; text-align: center; font-size: 15px; margin-top: 4px; }
.badge-subgrade { background: #4caf50; color: white; padding: 2px 8px; border-radius: 3px; font-size: 11px; font-weight: 600; }
.badge-asphalt { background: #ff9800; color: white; padding: 2px 8px; border-radius: 3px; font-size: 11px; font-weight: 600; }
.rdec-model, .ai-model { border-left-color: #ff9800; }

/* Responsive */
@media (max-width: 768px) {
  body { padding: 12px 16px; }
  .dashboard { justify-content: flex-start; }
  .card { flex: 1 1 140px; max-width: 48%; }
  .header-meta { flex-direction: column; }
  .header-meta > div { margin: 3px 0; }
  .toc ol { -webkit-column-count: 1; column-count: 1; }
  .chart-svg { max-width: 100%; }
}
"
        End Function

#End Region


#Region "JavaScript"

        Private Shared Function GetScript() As String
            Return "
(function() {
  var DESIGN_W = 1100;
  function scaleReport() {
    document.body.style.zoom = '1';
    var vw = document.documentElement.clientWidth || DESIGN_W;
    var z = vw / DESIGN_W;
    if (z < 0.5) z = 0.5;
    document.body.style.zoom = z.toString();
  }
  scaleReport();
  if (window.addEventListener) window.addEventListener('resize', scaleReport);
  else if (window.attachEvent) window.attachEvent('onresize', scaleReport);
})();
var tocLinks = document.querySelectorAll('.toc a[href^=""#""]');
for (var i = 0; i < tocLinks.length; i++) {
  (function(a) {
    a.addEventListener('click', function(e) {
      e.preventDefault();
      var target = document.querySelector(a.getAttribute('href'));
      if (target) target.scrollIntoView();
    });
  })(tocLinks[i]);
}
var btnPrint = document.getElementById('btn-print');
if (btnPrint) btnPrint.addEventListener('click', function() { window.print(); });
var topBtn = document.getElementById('btn-top');
if (topBtn) {
  window.addEventListener('scroll', function() {
    topBtn.style.display = (window.pageYOffset || 0) > 400 ? 'block' : 'none';
  });
  topBtn.addEventListener('click', function() { window.scrollTo(0, 0); });
}
"
        End Function

#End Region

    End Class
End Namespace
