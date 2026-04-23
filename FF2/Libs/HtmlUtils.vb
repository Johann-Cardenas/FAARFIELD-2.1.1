Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports SelectPdf

Namespace Libs
    ''' <summary>
    ''' HtmlUtils Class
    ''' Created By:  David Ross
    ''' Date:  4/8/2020
    ''' Purpose: HTML Helper functions to simplify html and pdf creation
    ''' </summary>
    Public Class HtmlUtils

        Public Function wrap_p(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<p>" + text + "</p>"
            Else
                tmptxt = "<p class='" + cssclass + "'>" + text + "</p>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_div(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<div>" + text + "</div>"
            Else
                tmptxt = "<div class='" + cssclass + "'>" + text + "</div>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_h2(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<h2>" + text + "</h2>"
            Else
                tmptxt = "<h2 class='" + cssclass + "'>" + text + "</h2>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_h3(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<h3>" + text + "</h3>"
            Else
                tmptxt = "<h3 class='" + cssclass + "'>" + text + "</h3>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_h4(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<h4>" + text + "</h4>"
            Else
                tmptxt = "<h4 class='" + cssclass + "'>" + text + "</h4>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_table(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<table>" + text + "</table>"
            Else
                tmptxt = "<table class='" + cssclass + "'>" + text + "</table>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_thead(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<thead>" + text + "</thead>"
            Else
                tmptxt = "<thead class='" + cssclass + "'>" + text + "</thead>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_tr(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<tr>" + text + "</tr>"
            Else
                tmptxt = "<tr class='" + cssclass + "'>" + text + "</tr>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_th(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<th>" + text + "</th>"
            Else
                tmptxt = "<th class='" + cssclass + "'>" + text + "</th>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_td(text As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<td>" + text + "</td>"
            Else
                tmptxt = "<td class='" + cssclass + "'>" + text + "</td>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_img(src As String, Optional cssclass As String = "") As String
            Dim tmptxt As String
            If cssclass = "" Then
                tmptxt = "<img src='" + src + "'/>"
            Else
                tmptxt = "<img src='" + src + "' class='" + cssclass + "'/>"
            End If
            Return tmptxt
        End Function

        Public Function wrap_bmp_img(src As Bitmap, Optional cssclass As String = "") As String
            Dim tmptxt As String
            'Dim base64String = Convert.ToBase64String(src)
            'Dim base64String = System.Text.Encoding.UTF8.GetString(src)
            Dim bmpStr = encodeTobase64(src)
            If cssclass = "" Then
                tmptxt = "<img src='data:image/png;base64," + bmpStr + "'/>"
            Else
                tmptxt = "<img src='data:image/png;base64," + bmpStr + "' class='" + cssclass + "'/>"
            End If
            Return tmptxt
        End Function

        Public Function encodeTobase64(image As Bitmap) As String
            Using ms As New System.IO.MemoryStream()
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png)
                Return Convert.ToBase64String(ms.ToArray())
            End Using
        End Function



        Public Function CreateHtmlPage(reportcontent As String, reportname As String) As String

            Dim html = "<!DOCTYPE HTML>"

            Try
                Dim assmbly As Assembly = Assembly.GetExecutingAssembly()
                Dim my_namespace As String =
                assmbly.GetName().Name.ToString()

                Dim stylesheet As String

                Using reader As New StreamReader(assmbly.GetManifestResourceStream(my_namespace + ".Reports.css"))
                    stylesheet = reader.ReadToEnd()
                End Using

                html = html & "<html>"
                html = html & "<head>"
                html = html & "<meta http-equiv='X-UA-Compatible' content='IE=edge'>"
                html = html & "<meta charset='UTF-8'>"
                html = html & "<title>"
                html = html & reportname
                html = html & "</title>"
                html = html & "<style>"
                html = html & stylesheet
                html = html & "</style>"
                html = html & "</head>"
                html = html & "<body>"
                html = html & reportcontent
                html = html & "</body>"
                html = html & "</html>"

            Catch ex As Exception
                Debug.WriteLine(ex.Message & ": MainWindowViewModel - CreateHTMLPage")
                Debug.WriteLine(ex)
            End Try

            Return html
        End Function


        ''' <summary>
        ''' Saves the HTML report to a file and opens it in the default browser.
        ''' </summary>
        Public Sub HtmlToFile(html As String, filepath As String)
            Try
                System.IO.File.WriteAllText(filepath, html, System.Text.Encoding.UTF8)
                Process.Start(New ProcessStartInfo(filepath) With {.UseShellExecute = True})
            Catch ex As Exception
                MessageBox.Show("HTML file not created: " & ex.Message, "File Not Saved", MessageBoxButton.OK, MessageBoxImage.Warning)
            End Try
        End Sub


        Public Sub HtmltoPdf(html As String, filepath As String)
            Try
                Dim pageSize As PdfPageSize = DirectCast([Enum].Parse(GetType(PdfPageSize), "Letter", True), PdfPageSize)
                Dim pdfOrientation As PdfPageOrientation = DirectCast([Enum].Parse(GetType(PdfPageOrientation), "Portrait", True), PdfPageOrientation)

                Using converter As New HtmlToPdf()
                    converter.Options.PdfPageSize = pageSize
                    converter.Options.PdfPageOrientation = pdfOrientation
                    converter.Options.WebPageWidth = 1100
                    converter.Options.WebPageHeight = 0
                    converter.Options.CssMediaType = HtmlToPdfCssMediaType.Screen
                    converter.Options.KeepImagesTogether = True
                    converter.Options.MarginTop = 36
                    converter.Options.MarginBottom = 18
                    converter.Options.MarginLeft = 18
                    converter.Options.MarginRight = 18

                    Using doc As PdfDocument = converter.ConvertHtmlString(html, filepath)
                        doc.Save(filepath)
                        doc.Close()
                    End Using
                End Using

            Catch ex As Exception
                MessageBox.Show("PDF not created, file is open or in use.", "File Not Saved", MessageBoxButton.OK, MessageBoxImage.Warning)
            End Try
        End Sub

    End Class

End Namespace