Imports System.Threading.Tasks
Imports JHSoftware.SimpleDNS.Plugin

Public Class HostsFilePlugIn
  Implements ILookupHost
  Implements ILookupReverse
  Implements IOptionsUI
  Implements IListsIPAddress
  Implements IListsDomainName

  Private MyData As HostsData
  Private MyCfg As New HFConfig

  Private LastReload As DateTime

  Private WithEvents fMon As System.IO.FileSystemWatcher

  Public Property Host As IHost Implements IPlugInBase.Host

#Region "not implemented"
  Public Sub LoadState(ByVal stateXML As String) Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.LoadState
  End Sub

  Public Function SaveState() As String Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.SaveState
    Return ""
  End Function

#End Region

  Public Function GetPlugInTypeInfo() As JHSoftware.SimpleDNS.Plugin.IPlugInBase.PlugInTypeInfo Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.GetTypeInfo
    With GetPlugInTypeInfo
      .Name = "Hosts File"
      .Description = "Retrieve host and reverse records from a standard ""hosts"" file"
      .InfoURL = "https://simpledns.plus/plugin-hostsfile"
    End With
  End Function

  Public Sub LoadConfig(ByVal config As String, ByVal instanceID As Guid, ByVal dataPath As String) Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.LoadConfig
    MyCfg = HFConfig.FromString(config)
  End Sub

  Private Function StartService() As Task Implements IPlugInBase.StartService
    Load()
    If MyCfg.AutoReload Then
      fMon = New System.IO.FileSystemWatcher
      fMon.Path = System.IO.Path.GetDirectoryName(MyCfg.FileName)
      fMon.Filter = System.IO.Path.GetFileName(MyCfg.FileName)
      fMon.IncludeSubdirectories = False
      fMon.NotifyFilter = IO.NotifyFilters.LastWrite
      fMon.EnableRaisingEvents = True
    End If
    Return Task.CompletedTask
  End Function

  Private Sub Load()
    LastReload = DateTime.UtcNow

    Dim f As System.IO.StreamReader
    Dim failCt As Integer
    Do
      Try
        f = My.Computer.FileSystem.OpenTextFileReader(MyCfg.FileName)
        Exit Do
      Catch ex As System.IO.FileNotFoundException
        Throw ex
      Catch ex As System.IO.IOException
        failCt += 1
        REM continue trying for 5 seconds (20 x 1/4 second)
        If failCt >= 20 Then Throw ex
        Threading.Thread.Sleep(250)
      End Try
    Loop

    Dim tmpData As New HostsData
    Dim x As String, i As Integer
    Dim ws As Char() = New Char() {" "c, ChrW(9)}
    Dim ip As System.Net.IPAddress = Nothing
    Dim dom As DomName = Nothing
    Dim ip4 As SdnsIPv4, ip6 As SdnsIPv6
    While Not f.EndOfStream
      x = f.ReadLine
      i = x.IndexOf("#"c)
      If i >= 0 Then x = x.Substring(0, i)
      i = x.IndexOfAny(ws)
      If i < 1 Then Continue While
      If Not System.Net.IPAddress.TryParse(x.Substring(0, i), ip) Then Continue While
      x = x.Substring(i).TrimStart
      While x.Length > 0
        i = x.IndexOfAny(ws)
        If i < 0 Then i = x.Length
        If Not DomName.TryParse(x.Substring(0, i), dom) Then Exit While
        x = x.Substring(i).TrimStart
        If ip.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
          REM ipv4
          ip4 = New SdnsIPv4(ip.GetAddressBytes)
          If Not tmpData.Fwd4.ContainsKey(dom) Then tmpData.Fwd4.Add(dom, ip4)
          If Not tmpData.Rev4.ContainsKey(ip4) Then tmpData.Rev4.Add(ip4, dom)
        Else
          REM ipv6
          ip6 = New SdnsIPv6(ip.GetAddressBytes)
          If Not tmpData.Fwd6.ContainsKey(dom) Then tmpData.Fwd6.Add(dom, ip6)
          If Not tmpData.Rev6.ContainsKey(ip6) Then tmpData.Rev6.Add(ip6, dom)
        End If
      End While
    End While
    f.Close()
    MyData = tmpData
  End Sub

  Public Sub StopService() Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.StopService
    MyData = Nothing
    If fMon IsNot Nothing Then
      fMon.EnableRaisingEvents = False
      fMon.Dispose()
      fMon = Nothing
    End If
  End Sub

  Public Function LookupHost(name As DomName, ipv6 As Boolean, req As IRequestContext) As Task(Of LookupResult(Of SdnsIP)) Implements ILookupHost.LookupHost
    Return Task.FromResult(LookupHost2(name, ipv6, req))
  End Function
  Public Function LookupHost2(name As DomName, ipv6 As Boolean, req As IRequestContext) As LookupResult(Of SdnsIP)
    If ipv6 Then
      Dim rIP As SdnsIPv6 = Nothing
      If MyData.Fwd6.TryGetValue(name, rIP) Then Return New LookupResult(Of SdnsIP) With {.Value = rIP, .TTL = MyCfg.TTL}
    Else
      Dim rIP As SdnsIPv4 = Nothing
      If MyData.Fwd4.TryGetValue(name, rIP) Then Return New LookupResult(Of SdnsIP) With {.Value = rIP, .TTL = MyCfg.TTL}
    End If
    Return Nothing
  End Function

  Public Function LookupReverse(qip As SdnsIP, req As IRequestContext) As Task(Of LookupResult(Of DomName)) Implements ILookupReverse.LookupReverse
    Return Task.FromResult(LookupReverse2(qip, req))
  End Function
  Public Function LookupReverse2(qip As SdnsIP, req As IRequestContext) As LookupResult(Of DomName)
    Dim rName As DomName
    If qip.IPVersion = 4 Then
      If Not MyData.Rev4.TryGetValue(DirectCast(qip, SdnsIPv4), rName) Then Return Nothing
    Else
      If Not MyData.Rev6.TryGetValue(DirectCast(qip, SdnsIPv6), rName) Then Return Nothing
    End If
    Return New LookupResult(Of DomName) With {.Value = rName, .TTL = MyCfg.TTL}
  End Function

  Public Function GetOptionsUI(ByVal instanceID As Guid, ByVal dataPath As String) As JHSoftware.SimpleDNS.Plugin.OptionsUI Implements JHSoftware.SimpleDNS.Plugin.IOptionsUI.GetOptionsUI
    Return New OptionsCtrl
  End Function

  Public Function InstanceConflict(ByVal config1 As String, ByVal config2 As String, ByRef errorMsg As String) As Boolean Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.InstanceConflict
    Dim cfg1 = HFConfig.FromString(config1)
    Dim cfg2 = HFConfig.FromString(config2)
    If cfg1.FileName.ToLower = cfg2.FileName.ToLower Then
      errorMsg = "Another plug-in is using the same hosts file"
      Return True
    End If
    Return False
  End Function

  Private Sub fMon_Changed(ByVal sender As Object, ByVal e As System.IO.FileSystemEventArgs) Handles fMon.Changed
    Try

      If DateTime.UtcNow.Subtract(LastReload).TotalSeconds < 5 Then Exit Sub
      Host.LogLine("Hosts file update detected - reloading")
      Try
        Load()
      Catch ex As Exception
        MyData = New HostsData
        Host.LogLine("Error reloading data file: " & ex.Message)
      End Try

    Catch ex As Exception
      Host.AsyncError(ex)
    End Try
  End Sub

  Public Function ListsIPAddress(ByVal ip As SdnsIP) As Task(Of Boolean) Implements JHSoftware.SimpleDNS.Plugin.IListsIPAddress.ListsIPAddress
    If ip.IPVersion = 4 Then
      Return Task.FromResult(MyData.Rev4.ContainsKey(DirectCast(ip, SdnsIPv4)))
    Else
      Return Task.FromResult(MyData.Rev6.ContainsKey(DirectCast(ip, SdnsIPv6)))
    End If
  End Function

  Public Function ListsDomainName(ByVal domain As DomName) As Task(Of Boolean) Implements JHSoftware.SimpleDNS.Plugin.IListsDomainName.ListsDomainName
    If MyData.Fwd4.ContainsKey(domain) Then Return Task.FromResult(True)
    Return Task.FromResult(MyData.Fwd6.ContainsKey(domain))
  End Function

End Class

Friend Class HFConfig
  Friend FileName As String
  Friend TTL As Integer = 300
  Friend AutoReload As Boolean = True

  Shared Function FromString(ByVal config As String) As HFConfig
    Dim rv As New HFConfig
    If config.Length = 0 Then Return rv
    If config.StartsWith("<File>") Then
      rv.AutoReload = False 'old version didn't have this option - so turn if off
      Dim doc As New Xml.XmlDocument
      Dim root As Xml.XmlElement = doc.CreateElement("root")
      doc.AppendChild(root)
      root.InnerXml = config
      For Each node As Xml.XmlNode In root.ChildNodes
        If Not TypeOf node Is Xml.XmlElement Then Continue For
        Select Case DirectCast(node, Xml.XmlElement).Name
          Case "File"
            rv.FileName = node.InnerText
          Case "TTL"
            Integer.TryParse(node.InnerText, rv.TTL)
        End Select
      Next
    Else
      Dim sa = PipeDecode(config)
      If sa.Length > 0 Then rv.FileName = sa(0)
      If sa.Length > 1 Then rv.AutoReload = (sa(1) = "Y")
      If sa.Length > 2 Then rv.TTL = Integer.Parse(sa(2))
    End If
    Return rv
  End Function

  Overrides Function ToString() As String
    Return PipeEncode(FileName) & "|" & _
           If(AutoReload, "Y", "N") & "|" & _
           TTL
  End Function
End Class

Friend Class HostsData
  Friend Fwd4 As New Dictionary(Of DomName, SdnsIPv4)
  Friend Fwd6 As New Dictionary(Of DomName, SdnsIPv6)
  Friend Rev4 As New Dictionary(Of SdnsIPv4, DomName)
  Friend Rev6 As New Dictionary(Of SdnsIPv6, DomName)
End Class