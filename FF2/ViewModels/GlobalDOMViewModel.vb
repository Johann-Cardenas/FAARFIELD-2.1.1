Imports System.IO
Imports System.Net
Imports System.Security.Cryptography
Imports System.Text

Namespace ViewModels
    Public Class GlobalDOMViewModel
        Public Shared AuthCheck As Boolean = False
        Public Shared Username As String = Nothing
        Public Shared Password As String = Nothing
        Public Shared FormType As String
        'Public Shared WebServiceUrl As String = "http://aratransweb.com:8080/DOMService.svc"
        'Public Shared WebServiceUrl As String = "http://localhost:39749/DOMService.svc"
        'Public Shared WebServiceUrl As String = "http://trans-ehtdev/DOMService.svc"
        Public Shared WebServiceUrl As String = "http://faapaveair.faa.gov/DOMService.svc"

        Public Function Encrypt(clearText As String) As String
            Dim EncryptionKey As String = "MAKV2SPBNI99212"
            Dim clearBytes As Byte() = Encoding.Unicode.GetBytes(clearText)
            Using encryptor As Aes = Aes.Create()
                Dim pdb As New Rfc2898DeriveBytes(EncryptionKey, New Byte() {&H49, &H76, &H61, &H6E, &H20, &H4D,
             &H65, &H64, &H76, &H65, &H64, &H65, &H76})
                encryptor.Key = pdb.GetBytes(32)
                encryptor.IV = pdb.GetBytes(16)
                Using ms As New MemoryStream()
                    Using cs As New CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write)
                        cs.Write(clearBytes, 0, clearBytes.Length)
                        cs.Close()
                    End Using
                    clearText = Convert.ToBase64String(ms.ToArray())
                End Using
            End Using

            clearText = Replace(clearText, "/", "_")
            Return clearText
        End Function
    End Class

End Namespace