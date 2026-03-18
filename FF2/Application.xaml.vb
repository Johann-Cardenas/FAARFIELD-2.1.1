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

        ' Show About dialog on startup — user must acknowledge before continuing
        Dim aboutWindow As New Views.AboutWindow()
        aboutWindow.Owner = window
        aboutWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner
        aboutWindow.ShowDialog()

    End Sub

End Class
