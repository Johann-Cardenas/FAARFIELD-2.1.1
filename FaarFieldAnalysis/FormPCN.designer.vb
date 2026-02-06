<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormPCN
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblPCN = New System.Windows.Forms.Label()
        Me.tbPCN = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'lblPCN
        '
        Me.lblPCN.BackColor = System.Drawing.Color.White
        Me.lblPCN.Font = New System.Drawing.Font("Consolas", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblPCN.Location = New System.Drawing.Point(234, 107)
        Me.lblPCN.Name = "lblPCN"
        Me.lblPCN.Size = New System.Drawing.Size(294, 176)
        Me.lblPCN.TabIndex = 0
        Me.lblPCN.Text = "Label1"
        '
        'tbPCN
        '
        Me.tbPCN.Font = New System.Drawing.Font("Consolas", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tbPCN.Location = New System.Drawing.Point(12, 12)
        Me.tbPCN.Multiline = True
        Me.tbPCN.Name = "tbPCN"
        Me.tbPCN.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.tbPCN.Size = New System.Drawing.Size(568, 569)
        Me.tbPCN.TabIndex = 1
        '
        'FormPCN
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(593, 593)
        Me.Controls.Add(Me.tbPCN)
        Me.Controls.Add(Me.lblPCN)
        Me.Name = "FormPCN"
        Me.Text = "PCN Results"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblPCN As System.Windows.Forms.Label
    Friend WithEvents tbPCN As System.Windows.Forms.TextBox
End Class
