Imports Telerik.Windows.DragDrop.Behaviors

Namespace Utilities

    Public Class ListBoxCopyDragDropBehavior
        Inherits ListBoxDragDropBehavior
        Sub New()

        End Sub

        Protected Overrides Function IsMovingItems(state As DragDropState) As Boolean
            Return False
        End Function
    End Class

End Namespace
