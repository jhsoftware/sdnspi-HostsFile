Imports JHSoftware.SimpleDNS.Plugin

Public Class HostsFilePlugIn
  Implements IGetHostPlugIn
  Implements IListsIPAddress
  Implements IListsDomainName

  Private MyData As HostsData
  Private MyCfg As New HFConfig

  Private LastReload As DateTime

  Private WithEvents fMon As System.IO.FileSystemWatcher

#Region "events"
  Public Event AsyncError(ByVal ex As System.Exception) Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.AsyncError
  Public Event SaveConfig(ByVal config As String) Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.SaveConfig
  Public Event LogLine(ByVal text As String) Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.LogLine
#End Region

#Region "not implemented"
  Public Sub LoadState(ByVal stateXML As String) Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.LoadState
  End Sub

  Public Function SaveState() As String Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.SaveState
    Return ""
  End Function

  Public Sub LookupTXT(ByVal req As IDNSRequest, ByRef resultText As String, ByRef resultTTL As Integer) Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.LookupTXT
    Throw New NotSupportedException
  End Sub

#End Region

  Public Function GetPlugInTypeInfo() As JHSoftware.SimpleDNS.Plugin.IPlugInBase.PlugInTypeInfo Implements JHSoftware.SimpleDNS.Plugin.IPlugInBase.GetPlugInTypeInfo
    With GetPlugInTypeInfo
      .Name = "Hosts File"
      .Description = "Retrieve host and reverse records from a standard ""hosts"" file"
      .InfoURL = "http://www.simpledns.com/plugin-hostsfile"
      .ConfigFile = False
    End With
  End Function

  Public Sub LoadConfig(ByVal config As String, ByVal instanceID As Guid, ByVal dataPath As String, ByRef maxThreads As Integer) Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.LoadConfig
    MyCfg = HFConfig.FromString(config)
  End Sub

  Public Sub StartService() Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.StartService
    Load()
    If MyCfg.AutoReload Then
      fMon = New System.IO.FileSystemWatcher
      fMon.Path = System.IO.Path.GetDirectoryName(MyCfg.FileName)
      fMon.Filter = System.IO.Path.GetFileName(MyCfg.FileName)
      fMon.IncludeSubdirectories = False
      fMon.NotifyFilter = IO.NotifyFilters.LastWrite
      fMon.EnableRaisingEvents = True
    End If
  End Sub

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
    Dim dom As DomainName = Nothing
    Dim ip4 As IPAddressV4, ip6 As IPAddressV6
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
        If Not DomainName.TryParse(x.Substring(0, i), dom) Then Exit While
        x = x.Substring(i).TrimStart
        If ip.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
          REM ipv4
          ip4 = New IPAddressV4(ip.GetAddressBytes)
          If Not tmpData.Fwd4.ContainsKey(dom) Then tmpData.Fwd4.Add(dom, ip4)
          If Not tmpData.Rev4.ContainsKey(ip4) Then tmpData.Rev4.Add(ip4, dom)
        Else
          REM ipv6
          ip6 = New IPAddressV6(ip.GetAddressBytes)
          If Not tmpData.Fwd6.ContainsKey(dom) Then tmpData.Fwd6.Add(dom, ip6)
          If Not tmpData.Rev6.ContainsKey(ip6) Then tmpData.Rev6.Add(ip6, dom)
        End If
      End While
    End While
    f.Close()
    MyData = tmpData
  End Sub

  Public Sub StopService() Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.StopService
    MyData = Nothing
    If fMon IsNot Nothing Then
      fMon.EnableRaisingEvents = False
      fMon.Dispose()
      fMon = Nothing
    End If
  End Sub

  Public Sub Lookup(ByVal req As IDNSRequest, ByRef resultIP As IPAddress, ByRef resultTTL As Integer) Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.Lookup
    If CType(req.QType, UShort) = 1US Then
      Dim rIP As IPAddressV4 = Nothing
      If MyData.Fwd4.TryGetValue(req.QName, rIP) Then resultIP = rIP Else resultIP = Nothing
    Else
      Dim rIP As IPAddressV6 = Nothing
      If MyData.Fwd6.TryGetValue(req.QName, rIP) Then resultIP = rIP Else resultIP = Nothing
    End If
    resultTTL = MyCfg.TTL
  End Sub

  Public Sub LookupReverse(ByVal req As IDNSRequest, ByRef resultName As JHSoftware.SimpleDNS.Plugin.DomainName, ByRef resultTTL As Integer) Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.LookupReverse
    Dim qip = req.QNameIP
    If qip.IPVersion = 4 Then
      If Not MyData.Rev4.TryGetValue(DirectCast(qip, IPAddressV4), resultName) Then resultName = Nothing
    Else
      If Not MyData.Rev6.TryGetValue(DirectCast(qip, IPAddressV6), resultName) Then resultName = Nothing
    End If
    resultTTL = MyCfg.TTL
  End Sub

  Public Function GetOptionsUI(ByVal instanceID As Guid, ByVal dataPath As String) As JHSoftware.SimpleDNS.Plugin.OptionsUI Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.GetOptionsUI
    Return New OptionsCtrl
  End Function

  Public Function InstanceConflict(ByVal config1 As String, ByVal config2 As String, ByRef errorMsg As String) As Boolean Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.InstanceConflict
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
      RaiseEvent LogLine("Hosts file update detected - reloading")
      Try
        Load()
      Catch ex As Exception
        MyData = New HostsData
        RaiseEvent LogLine("Error reloading data file: " & ex.Message)
      End Try

    Catch ex As Exception
      RaiseEvent AsyncError(ex)
    End Try
  End Sub

  Public Function GetDNSAskAbout() As JHSoftware.SimpleDNS.Plugin.DNSAskAboutGH Implements JHSoftware.SimpleDNS.Plugin.IGetHostPlugIn.GetDNSAskAbout
    GetDNSAskAbout = New JHSoftware.SimpleDNS.Plugin.DNSAskAboutGH
    With GetDNSAskAbout
      .ForwardIPv4 = True
      .ForwardIPv6 = True
      .RevIPv4Addr = IPAddressV4.Any
      .RevIPv4MaskSize = 0
      .RevIPv6Addr = IPAddressV6.Any
      .RevIPv6MaskSize = 0
    End With
  End Function

  Public Function ListsIPAddress(ByVal ip As IPAddress) As Boolean Implements JHSoftware.SimpleDNS.Plugin.IListsIPAddress.ListsIPAddress
    If ip.IPVersion = 4 Then
      Return MyData.Rev4.ContainsKey(DirectCast(ip, IPAddressV4))
    Else
      Return MyData.Rev6.ContainsKey(DirectCast(ip, IPAddressV6))
    End If
  End Function

  Public Function ListsDomainName(ByVal domain As JHSoftware.SimpleDNS.Plugin.DomainName) As Boolean Implements JHSoftware.SimpleDNS.Plugin.IListsDomainName.ListsDomainName
    If MyData.Fwd4.ContainsKey(domain) Then Return True
    Return MyData.Fwd6.ContainsKey(domain)
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
  Friend Fwd4 As New Dictionary(Of DomainName, IPAddressV4)
  Friend Fwd6 As New Dictionary(Of DomainName, IPAddressV6)
  Friend Rev4 As New Dictionary(Of IPAddressV4, DomainName)
  Friend Rev6 As New Dictionary(Of IPAddressV6, DomainName)
End Class