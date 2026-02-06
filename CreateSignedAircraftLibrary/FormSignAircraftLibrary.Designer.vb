<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FormSignAircraftLibrary
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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TextBoxAircraftLibrary = New System.Windows.Forms.TextBox()
        Me.ButtonOpenFile = New System.Windows.Forms.Button()
        Me.OpenFileDialogAircraftLibrary = New System.Windows.Forms.OpenFileDialog()
        Me.ButtonSign = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout
        '
        'Label1
        '
        Me.Label1.AutoSize = true
        Me.Label1.Location = New System.Drawing.Point(13, 13)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(107, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Aircraft library to sign:"
        '
        'TextBoxAircraftLibrary
        '
        Me.TextBoxAircraftLibrary.Location = New System.Drawing.Point(126, 10)
        Me.TextBoxAircraftLibrary.Name = "TextBoxAircraftLibrary"
        Me.TextBoxAircraftLibrary.Size = New System.Drawing.Size(511, 20)
        Me.TextBoxAircraftLibrary.TabIndex = 1
        '
        'ButtonOpenFile
        '
        Me.ButtonOpenFile.Location = New System.Drawing.Point(646, 8)
        Me.ButtonOpenFile.Name = "ButtonOpenFile"
        Me.ButtonOpenFile.Size = New System.Drawing.Size(75, 23)
        Me.ButtonOpenFile.TabIndex = 2
        Me.ButtonOpenFile.Text = "..."
        Me.ButtonOpenFile.UseVisualStyleBackColor = true
        '
        'OpenFileDialogAircraftLibrary
        '
        Me.OpenFileDialogAircraftLibrary.FileName = "aircraftlibrary.xml"
        Me.OpenFileDialogAircraftLibrary.Filter = "XML files|*.xml"
        '
        'ButtonSign
        '
        Me.ButtonSign.Location = New System.Drawing.Point(195, 50)
        Me.ButtonSign.Name = "ButtonSign"
        Me.ButtonSign.Size = New System.Drawing.Size(75, 23)
        Me.ButtonSign.TabIndex = 3
        Me.ButtonSign.Text = "Sign"
        Me.ButtonSign.UseVisualStyleBackColor = true
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(450, 50)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(75, 23)
        Me.Button1.TabIndex = 4
        Me.Button1.Text = "Check"
        Me.Button1.UseVisualStyleBackColor = true
        '
        'FormSignAircraftLibrary
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6!, 13!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(733, 92)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.ButtonSign)
        Me.Controls.Add(Me.ButtonOpenFile)
        Me.Controls.Add(Me.TextBoxAircraftLibrary)
        Me.Controls.Add(Me.Label1)
        Me.Name = "FormSignAircraftLibrary"
        Me.Text = "Sign Aircraft Library"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents TextBoxAircraftLibrary As TextBox
    Friend WithEvents ButtonOpenFile As Button
    Friend WithEvents OpenFileDialogAircraftLibrary As OpenFileDialog
    Friend WithEvents ButtonSign As Button
    Friend WithEvents Button1 As Button
End Class
