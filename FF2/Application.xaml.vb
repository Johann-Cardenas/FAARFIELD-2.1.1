Partial Public Class Application
    Inherits Windows.Application

    ' Application-level events, such as Startup, Exit, and DispatcherUnhandledException
    ' can be handled in this file.

    Protected Overrides Sub OnStartup(e As StartupEventArgs)

        If e.Args.Length > 0 Then
            Current.Properties("StartFilePath") = e.Args(0)
        End If

        Dim window As New Views.MainWindow()
        window.Show()

    End Sub

End Class
