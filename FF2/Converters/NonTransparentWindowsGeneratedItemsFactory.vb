Imports Telerik.Windows.Controls.Docking
Imports Telerik.Windows.Controls.Navigation

Namespace Converters

    Public Class NonTransparentWindowsGeneratedItemsFactory
        Inherits DefaultGeneratedItemsFactory

        Public Overrides Function CreateToolWindow() As ToolWindow
            Dim window = MyBase.CreateToolWindow()
            RadWindowInteropHelper.SetAllowTransparency(window, False)
            RadWindowInteropHelper.SetClipMaskCornerRadius(window, New CornerRadius(3))
            RadWindowInteropHelper.SetOpaqueWindowBackground(window, Brushes.LightGray)
            Return window
        End Function
    End Class
End Namespace
