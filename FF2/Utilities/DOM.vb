Imports System.Data
Imports System.IO
Imports System.Xml
Imports System.Math
Imports System.Collections.ObjectModel
Imports iTextSharp.text
Imports System.Net
Imports System.Text
Imports Microsoft.VisualBasic.FileIO
Imports System.Net.Http
Imports FF2.ViewModels

Public Class DOM

    Private Shared ReadOnly readr As XmlReader
    Public XMLDom As DataSet = New DataSet
    Private OutputPath As String = SpecialDirectories.MyDocuments + "\My FAARFIELD\UploadedJob.xml"

    Sub New()

    End Sub


    Public Sub WriteXmlToFile()
        If XMLDom Is Nothing Then
            Return
        End If

        ' Create a file name to write to.
        Dim filename As String = OutputPath
        'Dim filename As String = "XmlDoc.xml"

        ' Create the FileStream to write with.
        Dim stream As New System.IO.FileStream _
           (filename, System.IO.FileMode.Create)

        ' Create an XmlTextWriter with the fileStream.
        Dim xmlWriter As New System.Xml.XmlTextWriter _
           (stream, System.Text.Encoding.Unicode)

        ' Write to the file with the WriteXml method.
        XMLDom.WriteXml(xmlWriter)

        xmlWriter.Close()
    End Sub


    ''' <summary>
    ''' Writes an XML element in a specific XPath location to 
    ''' the output of the WriteTemplateXML sub.
    ''' This sub can only be called after WriteTemplateXML is called.
    ''' </summary>
    ''' <param name="xml">The XML node that will be added to the DOM xml</param>
    ''' <param name="srcXPath">The xpath for input element</param>
    ''' <param name="destXPath">The xpath of the destination element</param>
    ''' 
    Public Sub AddNodeToXPath(xml As String, srcXPath As String, destXPath As String)
        Dim dstdoc As New XmlDocument()
        dstdoc.Load(OutputPath)

        Dim srcdoc As New XmlDocument()
        srcdoc.LoadXml(xml)

        Dim nsmgr As New XmlNamespaceManager(New NameTable)
        nsmgr.AddNamespace("ns", "http://schemas.datacontract.org/2004/07/FaarFieldModel")
        nsmgr.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance")
        nsmgr.AddNamespace("a", "http://schemas.microsoft.com/2003/10/Serialization/Arrays")

        Dim copiedNode As XmlNode
        copiedNode = dstdoc.ImportNode(srcdoc.SelectSingleNode(srcXPath, nsmgr), True)

        Dim destElement As XmlNode
        destElement = dstdoc.SelectSingleNode(destXPath, nsmgr)

        For Each sourceNode In srcdoc.SelectNodes(srcXPath, nsmgr)
            Dim imported As XmlNode = dstdoc.ImportNode(sourceNode, True)
            destElement.AppendChild(imported)
        Next

        dstdoc.Save(OutputPath)
    End Sub


    ''' <summary>
    ''' Encodes the file referenced by the OutPutPath variable and sends it in a PUT request to the PAVEAIR DOM service.
    ''' </summary>
    Public Sub SendPutRequest(username As String, password As String, database As String, endpoint As String)
        Try
            Dim xmlDoc As XDocument = XDocument.Load(OutputPath)
            Dim doc As XmlDocument = New XmlDocument()
            Dim action As String = ""

            Select Case endpoint
                Case "UploadFaarfieldJob"
                    action = "Upload"
                Case "UpdateFaarfieldJob"
                    action = "Update"
            End Select

            doc.Load(OutputPath)

            'PUT REQUEST URLs
            Dim url = $"{GlobalDOMViewModel.WebServiceUrl}/{endpoint}/{username}/{password}/{database}"
            Dim webReq As WebRequest = HttpWebRequest.Create(url)
            Dim request As HttpWebRequest = TryCast(webReq, HttpWebRequest)
            request.Method = "PUT"

            Dim encoding As Encoding = Encoding.UTF8
            Dim docAsBytes As Byte() = encoding.GetBytes(doc.OuterXml)
            request.ContentType = "application/plain"
            request.Headers.Add("Cookie:;")
            'Set the ContentLength property of the WebRequest.  
            request.ContentLength = docAsBytes.Length
            'Get the request stream.  
            Dim dataStream As Stream = request.GetRequestStream()
            'Write the data to the request stream.
            dataStream.Write(docAsBytes, 0, docAsBytes.Length)
            dataStream.Close()

            Dim responseA As WebResponse = request.GetResponse()
            Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
            'Get the stream containing content returned by the server.  
            'The using block ensures the stream is automatically closed.
            Using dataStream1 As Stream = response.GetResponseStream()
                'Open the stream using a StreamReader for easy access.  
                Dim reader2 As New StreamReader(dataStream1)
                Dim responseFromServer As String = reader2.ReadToEnd()
                responseFromServer = Replace(responseFromServer, "<string xmlns=""http://schemas.microsoft.com/2003/10/Serialization/"">", "")
                responseFromServer = Replace(responseFromServer, "</string>", "")

                If responseFromServer <> "Data has been sent." Then
                    MessageBox.Show("Error: " & responseFromServer, "PAVEAIR Error", MessageBoxButton.OK, MessageBoxImage.Error)
                Else
                    MessageBox.Show(action & " Complete", "Complete", MessageBoxButton.OK)
                End If
            End Using
            'Clean up the response.  
            response.Close()

        Catch ex As Exception
            MessageBox.Show(ex.Message, "FAARFIELD Error", MessageBoxButton.OK, MessageBoxImage.Error)
        End Try
    End Sub

End Class



