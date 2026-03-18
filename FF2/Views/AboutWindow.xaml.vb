Imports System.IO

Namespace Views
    Partial Public Class AboutWindow
        Inherits Window

        Public Sub New()
            InitializeComponent()

            ' Load build date from embedded resource
            Try
                Dim buildDatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "BuildDate.txt")
                If File.Exists(buildDatePath) Then
                    Dim raw = File.ReadAllText(buildDatePath).Trim()
                    BuildDateText.Text = "Built " & raw
                Else
                    BuildDateText.Text = "Built " & DateTime.Now.ToString("yyyy-MM-dd")
                End If
            Catch
                BuildDateText.Text = ""
            End Try
        End Sub

        Private Sub CloseButton_Click(sender As Object, e As RoutedEventArgs)
            Close()
        End Sub

        ' Allow dragging the borderless window
        Protected Overrides Sub OnMouseLeftButtonDown(e As Input.MouseButtonEventArgs)
            MyBase.OnMouseLeftButtonDown(e)
            If e.ButtonState = Input.MouseButtonState.Pressed Then
                DragMove()
            End If
        End Sub
    End Class
End Namespace
