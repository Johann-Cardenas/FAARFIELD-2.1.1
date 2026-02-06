Namespace Converters

    Public Class BrowserBehavior

        Public Shared ReadOnly HtmlProperty As DependencyProperty = DependencyProperty.RegisterAttached("Html", GetType(String), GetType(BrowserBehavior), New FrameworkPropertyMetadata(AddressOf OnHtmlChanged))

        <AttachedPropertyBrowsableForType(GetType(WebBrowser))>
        Public Shared Function GetHtml(ByVal d As WebBrowser) As String
            Return CStr(d.GetValue(HtmlProperty))
        End Function

        Public Shared Sub SetHtml(ByVal d As WebBrowser, ByVal value As String)
            d.SetValue(HtmlProperty, value)
        End Sub

        Private Shared Sub OnHtmlChanged(ByVal dependencyObject As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
            Dim webBrowser As WebBrowser = TryCast(dependencyObject, WebBrowser)
            If webBrowser IsNot Nothing Then webBrowser.NavigateToString(If(TryCast(e.NewValue, String), "&nbsp;"))
        End Sub
    End Class
End Namespace


