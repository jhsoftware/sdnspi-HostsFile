Imports System.Windows.Forms

Public Class OptionsCtrl

  Public Overrides Sub LoadData(ByVal config As String)
    If config Is Nothing Then
      'new instance
      If Not RemoteGUI Then
        If Environment.OSVersion.Platform = PlatformID.Win32Windows Then
          Dim windir As String = Environment.GetEnvironmentVariable("windir")
          If windir IsNot Nothing Then txtFile.Text = windir & "\hosts"
        Else
          txtFile.Text = Environment.SystemDirectory & "\drivers\etc\hosts"
        End If
      End If
    Else
      Dim tmp = HFConfig.FromString(config)
      txtFile.Text = tmp.FileName
      chkMonitor.Checked = tmp.AutoReload
      CtlTTL1.Value = tmp.TTL
    End If
  End Sub

  Public Overrides Function SaveData() As String
    Dim tmp As New HFConfig
    tmp.FileName = txtFile.Text.Trim
    tmp.AutoReload = chkMonitor.Checked
    tmp.TTL = CtlTTL1.Value
    Return tmp.ToString
  End Function

  Public Overrides Function ValidateData() As Boolean
    If Not RemoteGUI Then
      If Not My.Computer.FileSystem.FileExists(txtFile.Text.Trim) Then
        MessageBox.Show("Hosts file does not exist!", "Hosts file", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Return False
      End If
    End If
    Return True
  End Function

  Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
    If RemoteGUI Then MessageBox.Show("This function is not available during remote management", _
                                      "Browse for file/folder", MessageBoxButtons.OK, _
                                      MessageBoxIcon.Warning) : Exit Sub

    OpenFileDialog1.FileName = txtFile.Text.Trim
    Try
      If OpenFileDialog1.ShowDialog <> Windows.Forms.DialogResult.OK Then Exit Sub
    Catch ex As System.InvalidOperationException
      REM reported by Kento for file name "C:\"
      OpenFileDialog1.FileName = ""
      If OpenFileDialog1.ShowDialog <> Windows.Forms.DialogResult.OK Then Exit Sub
    End Try
    txtFile.Text = OpenFileDialog1.FileName
  End Sub

End Class
