
Namespace Views

    Partial Public Class MainWindow
        Inherits Window

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        Private Sub Login_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            Dim gVM = New ViewModels.GlobalDOMViewModel()
            Dim password As String = gVM.Encrypt(pswdBox.Password)
            ViewModels.GlobalDOMViewModel.Password = password
        End Sub

    End Class

End Namespace
