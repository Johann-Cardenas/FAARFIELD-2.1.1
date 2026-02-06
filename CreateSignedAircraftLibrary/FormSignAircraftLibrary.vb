Imports System.IO
Imports System.Runtime.Serialization
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml
Imports FaarFieldModel


Public Class FormSignAircraftLibrary
    Private Sub ButtonOpenFile_Click(sender As Object, e As EventArgs) Handles ButtonOpenFile.Click
        If OpenFileDialogAircraftLibrary.ShowDialog() = DialogResult.OK Then
            TextBoxAircraftLibrary.Text = OpenFileDialogAircraftLibrary.FileName
        End If
    End Sub

    Private Sub ButtonSign_Click(sender As Object, e As EventArgs) Handles ButtonSign.Click
        If String.IsNullOrWhiteSpace(TextBoxAircraftLibrary.Text) Then
            MessageBox.Show("Enter or chose a valid AircraftLibrary.xml file to sign.")
            Return
        End If

        Dim aircraftLibrary As SignedAircraftLibrary
        Try
            Dim ser = New DataContractSerializer(GetType(SignedAircraftLibrary))
            Dim textReader As TextReader = New StreamReader(TextBoxAircraftLibrary.Text)
            Dim xml = textReader.ReadToEnd()
            textReader.Close()
            Dim stream = New MemoryStream(Encoding.UTF8.GetBytes(xml))
            aircraftLibrary = DirectCast(ser.ReadObject(stream), SignedAircraftLibrary)

        Catch ex As Exception
            MessageBox.Show(TextBoxAircraftLibrary.Text + " is not a valid FAARFIELD aircraft library file.")
            Return
        End Try

        Dim doc As New XmlDocument()
        Dim root As XmlElement
        Dim libVersion As String = ""
        Dim softwareVersion As String = ""

        doc.Load(TextBoxAircraftLibrary.Text)
        root = doc.DocumentElement

        If root.HasAttribute("LibraryVersion") Then
            libVersion = root.GetAttribute("LibraryVersion")
        End If

        If root.HasAttribute("SoftwareVersion") Then
            softwareVersion = root.GetAttribute("SoftwareVersion")
        End If

        If libVersion = "" OrElse softwareVersion = "" Then
            MsgBox("Version attribute is missing in root node")
        End If

        'Create a new instance of RSACryptoServiceProvider.
        Using rsa As New RSACryptoServiceProvider()
            'The hash to sign.

            Dim parameters = rsa.ExportParameters(True)
            aircraftLibrary.Exponent = parameters.Exponent
            aircraftLibrary.Modulus = parameters.Modulus

            Dim hash() As Byte
            Using sha256 As SHA256 = SHA256.Create()
                hash = sha256.ComputeHash(aircraftLibrary.GetHash())
            End Using

            'Create an RSASignatureFormatter object and pass it the 
            'RSACryptoServiceProvider to transfer the key information.
            Dim rsaFormatter As New RSAPKCS1SignatureFormatter(rsa)

            'Set the hash algorithm to SHA256.
            rsaFormatter.SetHashAlgorithm("SHA256")

            'Create a signature for HashValue and return it.
            aircraftLibrary.SignedHash = rsaFormatter.CreateSignature(hash)

            Dim writer As New FileStream(TextBoxAircraftLibrary.Text, FileMode.Create)
            Dim ser As New DataContractSerializer(GetType(SignedAircraftLibrary))
            ser.WriteObject(writer, aircraftLibrary)
            writer.Close()

        End Using

        If Not libVersion = "" OrElse Not softwareVersion = "" Then
            Dim libFile As String

            Using reader As New StreamReader(TextBoxAircraftLibrary.Text)
                libFile = reader.ReadToEnd
            End Using

            Dim closeRootPos As Integer
            closeRootPos = InStr(1, libFile, ">", CompareMethod.Text)

            Dim versionAttributes As String = " LibraryVersion=" & ControlChars.Quote & libVersion.ToString() & ControlChars.Quote & " SoftwareVersion=" & ControlChars.Quote & softwareVersion.ToString() & ControlChars.Quote

            Dim libFileWithAttribs As String = libFile.Insert(closeRootPos - 1, versionAttributes)

            libFile = libFileWithAttribs

            Using writer As New StreamWriter(TextBoxAircraftLibrary.Text)
                writer.Write(libFile)
            End Using
        End If

        MessageBox.Show("The AircraftLibrary XML file has been successfully hashed and signed.")

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        Dim aircraftLibrary As SignedAircraftLibrary
        Try
            Dim ser = New DataContractSerializer(GetType(SignedAircraftLibrary))
            Dim textReader As TextReader = New StreamReader(TextBoxAircraftLibrary.Text)
            Dim xml = textReader.ReadToEnd()
            textReader.Close()
            Dim stream = New MemoryStream(Encoding.UTF8.GetBytes(xml))
            aircraftLibrary = DirectCast(ser.ReadObject(stream), SignedAircraftLibrary)

        Catch ex As Exception
            MessageBox.Show(TextBoxAircraftLibrary.Text + " is not a valid FAARFIELD aircraft library file.")
            Return
        End Try
        Dim rsaKeyInfo = New RSAParameters()
        rsaKeyInfo.Modulus = aircraftLibrary.Modulus
        rsaKeyInfo.Exponent = aircraftLibrary.Exponent
        Dim rsaDecrypt = New RSACryptoServiceProvider()
        rsaDecrypt.ImportParameters(rsaKeyInfo)
        Dim rsaDeformatter = New RSAPKCS1SignatureDeformatter(rsaDecrypt)
        rsaDeformatter.SetHashAlgorithm("SHA256")

        Using sha256 As SHA256 = SHA256.Create()
            Dim hash = sha256.ComputeHash(aircraftLibrary.GetHash())
            If rsaDeformatter.VerifySignature(hash, aircraftLibrary.SignedHash) Then
                MessageBox.Show("The selected AircraftLibrary XML file is properly signed and verified.")
            Else
                MessageBox.Show("The selected aircraftLibrary.xml file checksum does not match the signed checksum/hash.  Please Sign the current AircraftLibrary.xml.")

            End If
        End Using
    End Sub
End Class
