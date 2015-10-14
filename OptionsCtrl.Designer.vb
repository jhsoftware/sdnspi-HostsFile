<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class OptionsCtrl
    Inherits JHSoftware.SimpleDNS.Plugin.OptionsUI

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
    Me.components = New System.ComponentModel.Container
    Me.txtFile = New System.Windows.Forms.TextBox
    Me.btnBrowse = New System.Windows.Forms.Button
    Me.Label1 = New System.Windows.Forms.Label
    Me.Label2 = New System.Windows.Forms.Label
    Me.ToolTip1 = New System.Windows.Forms.ToolTip(Me.components)
    Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog
    Me.CtlTTL1 = New ctlTTL
    Me.chkMonitor = New System.Windows.Forms.CheckBox
    Me.SuspendLayout()
    '
    'txtFile
    '
    Me.txtFile.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.txtFile.Location = New System.Drawing.Point(0, 16)
    Me.txtFile.Name = "txtFile"
    Me.txtFile.Size = New System.Drawing.Size(339, 20)
    Me.txtFile.TabIndex = 1
    '
    'btnBrowse
    '
    Me.btnBrowse.Anchor = CType((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
    Me.btnBrowse.Location = New System.Drawing.Point(345, 14)
    Me.btnBrowse.Name = "btnBrowse"
    Me.btnBrowse.Size = New System.Drawing.Size(23, 23)
    Me.btnBrowse.TabIndex = 2
    Me.btnBrowse.Text = ".."
    Me.ToolTip1.SetToolTip(Me.btnBrowse, "Browse")
    Me.btnBrowse.UseVisualStyleBackColor = True
    '
    'Label1
    '
    Me.Label1.AutoSize = True
    Me.Label1.Location = New System.Drawing.Point(-3, 0)
    Me.Label1.Name = "Label1"
    Me.Label1.Size = New System.Drawing.Size(53, 13)
    Me.Label1.TabIndex = 0
    Me.Label1.Text = "Hosts file:"
    '
    'Label2
    '
    Me.Label2.AutoSize = True
    Me.Label2.Location = New System.Drawing.Point(-3, 89)
    Me.Label2.Margin = New System.Windows.Forms.Padding(3, 15, 3, 0)
    Me.Label2.Name = "Label2"
    Me.Label2.Size = New System.Drawing.Size(165, 13)
    Me.Label2.TabIndex = 4
    Me.Label2.Text = "DNS Record TTL (Time To Live):"
    '
    'OpenFileDialog1
    '
    Me.OpenFileDialog1.AddExtension = False
    Me.OpenFileDialog1.FileName = "OpenFileDialog1"
    Me.OpenFileDialog1.Filter = "Hosts file|hosts|Text files (*.txt)|*.txt|All files (*.*)|*.*"
    Me.OpenFileDialog1.Title = "Select hosts file"
    '
    'CtlTTL1
    '
    Me.CtlTTL1.AutoSize = True
    Me.CtlTTL1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink
    Me.CtlTTL1.BackColor = System.Drawing.Color.Transparent
    Me.CtlTTL1.Location = New System.Drawing.Point(0, 105)
    Me.CtlTTL1.Name = "CtlTTL1"
    Me.CtlTTL1.ReadOnly = False
    Me.CtlTTL1.Size = New System.Drawing.Size(156, 21)
    Me.CtlTTL1.TabIndex = 5
    Me.CtlTTL1.Value = 300
    '
    'chkMonitor
    '
    Me.chkMonitor.AutoSize = True
    Me.chkMonitor.Checked = True
    Me.chkMonitor.CheckState = System.Windows.Forms.CheckState.Checked
    Me.chkMonitor.Location = New System.Drawing.Point(0, 54)
    Me.chkMonitor.Margin = New System.Windows.Forms.Padding(3, 15, 3, 3)
    Me.chkMonitor.Name = "chkMonitor"
    Me.chkMonitor.Size = New System.Drawing.Size(256, 17)
    Me.chkMonitor.TabIndex = 3
    Me.chkMonitor.Text = "Automatically re-load hosts file when it is updated"
    Me.chkMonitor.UseVisualStyleBackColor = True
    '
    'OptionsCtrl
    '
    Me.Controls.Add(Me.chkMonitor)
    Me.Controls.Add(Me.CtlTTL1)
    Me.Controls.Add(Me.Label2)
    Me.Controls.Add(Me.Label1)
    Me.Controls.Add(Me.btnBrowse)
    Me.Controls.Add(Me.txtFile)
    Me.Name = "OptionsCtrl"
    Me.Size = New System.Drawing.Size(368, 134)
    Me.ResumeLayout(False)
    Me.PerformLayout()

  End Sub
  Friend WithEvents txtFile As System.Windows.Forms.TextBox
  Friend WithEvents btnBrowse As System.Windows.Forms.Button
  Friend WithEvents Label1 As System.Windows.Forms.Label
  Friend WithEvents Label2 As System.Windows.Forms.Label
  Friend WithEvents CtlTTL1 As ctlTTL
  Friend WithEvents ToolTip1 As System.Windows.Forms.ToolTip
  Friend WithEvents OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
  Friend WithEvents chkMonitor As System.Windows.Forms.CheckBox

End Class
