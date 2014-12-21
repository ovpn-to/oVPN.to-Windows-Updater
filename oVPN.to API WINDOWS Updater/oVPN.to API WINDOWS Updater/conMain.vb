' oVPN.to Updater by twink0r
' thanks to oVPN.to - MrNice
' appVersion 0002
' dotNET Framework 4


' Copyright (c) <2014> <twink0r & MrNice>
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
' The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES' OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
'
' Changelog : 21.12.14
' - Add 
'       SSL Fingerprint Check
' Changelog : 21.12.14
' - Add 
'       Config File changed - exe_autoupdate
'       Autoupdate function - hashcheck and autostart 

Imports System.IO, System.Net, System.Text
Imports System.Net.Security
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Cryptography

Module conMain
    Const URL As String = "https://vcp.ovpn.to/xxxapi.php"
    Const appTitle As String = "oVPN.to Win Updater"
    Const appVersion As String = "0004"
    Const oVPNSSLFP As String = "D4A54FC76F692CA048927F6179303B95ACD5DA2F"
    Dim otp As String
    Dim DIR As String = System.AppDomain.CurrentDomain.BaseDirectory
    Dim lastUpdaterV As String
    Dim CVERSION As String
    Dim UID As String
    Dim apiKey As String
    Dim OCFGTYPE As String
    Dim lastUpdateFile As String
    Dim lastUpdateOnline As String
    Dim bAutoUpdate As Boolean

    Sub Main()
        Console.Title = appTitle
        Console.ForegroundColor = ConsoleColor.White
        ServicePointManager.ServerCertificateValidationCallback = New RemoteCertificateValidationCallback(AddressOf AcceptAllCertifications)
        checkSSL()
        'AppTitle

        If Not IO.File.Exists(DIR & "lastovpntoupdate.txt") Then
            IO.File.Create(DIR & "lastovpntoupdate.txt").Close()
            IO.File.WriteAllText(DIR & "lastovpntoupdate.txt", 0)
        End If

        readINI()
        updateClient()
        ' start update checks and go!
        Console.WriteLine("please wait...")
        If UBound(Split(lastupdate(), ":")) > 0 Then
            lastUpdateOnline = Split(lastupdate(), ":")(1)
        End If
        If IO.File.Exists(DIR & "lastovpntoupdate.txt") Then
            lastUpdateFile = IO.File.ReadAllText(DIR & "lastovpntoupdate.txt")
            If lastUpdateFile < lastUpdateOnline Then
                Console.Clear()
                Console.ForegroundColor = ConsoleColor.Magenta
                Console.WriteLine("Update available!")
                Console.Write("Update Certs and Configs now? (Y)es / (N)o : ")
                Console.ForegroundColor = ConsoleColor.White

                Dim choice As String
                choice = Console.ReadLine()
                If choice.ToUpper = "Y" Then
                    startUpdate()
                End If

            ElseIf lastUpdateFile = lastUpdateOnline Then
                Console.Clear()
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("Configs up2date")
                Console.ForegroundColor = ConsoleColor.White
                Console.Write("force update ? (Y)es / (N)o : ")
                Dim strForce As String
                strForce = Console.ReadLine()
                If strForce.ToUpper = "Y" Then
                    startUpdate()
                Else
                    Console.WriteLine("Enter Y or we do nothing!")
                End If
            Else
                Console.Clear()
                Console.WriteLine(lastupdate())
                Console.WriteLine(UID)

            End If
        Else
            IO.File.Create(DIR & "lastovpntoupdate.txt").Close()
            IO.File.WriteAllText(DIR & "lastovpntoupdate.txt", 0)
        End If


        While True
            Console.ReadLine()
        End While

    End Sub
    Sub checkHash()
        Dim onlineHash As String
        Dim localHash As String
        localHash = GetSHA512(System.AppDomain.CurrentDomain.FriendlyName)
        onlineHash = getHash()
        If Not onlineHash = localHash Then
            Console.WriteLine("ERROR! INVALID FILE-HASH! EXIT")
            Threading.Thread.Sleep(5000)
            End
        End If
    End Sub
    Function GetSHA512(ByVal filePath As String)
        Dim sha512 As SHA512CryptoServiceProvider = New SHA512CryptoServiceProvider
        Dim f As FileStream = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)

        f = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192)
        sha512.ComputeHash(f)
        f.Close()

        Dim hash As Byte() = sha512.Hash
        Dim buff As StringBuilder = New StringBuilder
        Dim hashByte As Byte

        For Each hashByte In hash
            buff.Append(String.Format("{0:X2}", hashByte))
        Next

        Dim sha512string As String
        sha512string = buff.ToString()

        Return sha512string

    End Function
    Function checkSSL()
        Dim wc As New Net.WebClient
        Return wc.DownloadString(URL)
    End Function
    Public Function AcceptAllCertifications(ByVal sender As Object, ByVal certification As System.Security.Cryptography.X509Certificates.X509Certificate, ByVal chain As System.Security.Cryptography.X509Certificates.X509Chain, ByVal sslPolicyErrors As System.Net.Security.SslPolicyErrors) As Boolean
        Dim x509 As New X509Certificate2
        Dim rawData() As Byte = certification.GetRawCertData
        x509.Import(rawData)

        If x509.Thumbprint = oVPNSSLFP Then
            Return True
        Else
            Console.WriteLine("vcp.ovpn.to SSL Certificate ERROR! EXIT")
            Threading.Thread.Sleep(5000)
            End
        End If

    End Function
    Sub startUpdate()
        getOPENVPNVersion()
        Dim Request As String
        Dim SplitRes As Array

        ' request Certs !
        Console.WriteLine("Checking for oVPN Certs/Config-Update:")
        Request = requestcerts()
        Console.WriteLine(Request)
        While Request = "wait" Or Request = "submitted"
            Request = requestcerts()
            Console.WriteLine(Request)
            Threading.Thread.Sleep(5000)
        End While

        If InStr(Request, "ready") Then
            SplitRes = Split(Request, ":")
            otp = SplitRes(1)
            getconfigs()
            getCerts()
        End If

    End Sub
    Sub updateClient()
        lastUpdaterV = getLastUpdater()
        If Not lastUpdaterV = appVersion Then
            If bAutoUpdate = False Then
                Console.ForegroundColor = ConsoleColor.Magenta
                Console.WriteLine("PLEASE UPDATE YOUR WINDOWS-UPDATER TOOL!!!")
                Console.Title = appTitle & " / WIN-Updater not up 2 date"
                Console.ForegroundColor = ConsoleColor.White
                Threading.Thread.Sleep(10000)
            ElseIf bAutoUpdate = True Then
                getLastUpdaterBin()
            End If
        End If

    End Sub
    Private Declare Auto Function GetPrivateProfileString Lib "kernel32" (ByVal lpAppName As String, _
                ByVal lpKeyName As String, _
                ByVal lpDefault As String, _
                ByVal lpReturnedString As StringBuilder, _
                ByVal nSize As Integer, _
                ByVal lpFileName As String) As Integer
    Sub readINI()
        If IO.File.Exists(DIR & "ovpnapi.ini") Then
            Dim UIDSB As StringBuilder
            Dim APISB As StringBuilder
            Dim CTYPSB As StringBuilder
            Dim UPSB As StringBuilder
            Dim bInvalid As Boolean
            Dim res As Integer


            UIDSB = New StringBuilder(350)
            APISB = New StringBuilder(350)
            CTYPSB = New StringBuilder(350)
            UPSB = New StringBuilder(350)
            res = GetPrivateProfileString("data", "USERID", "", UIDSB, UIDSB.Capacity, DIR & "ovpnapi.ini")
            If UIDSB.ToString = "" Then
                Console.WriteLine("please add your UserID to the configfile")
                bInvalid = True
            Else
                If IsNumeric(UIDSB.ToString.Replace(" ", "")) Then
                    UID = UIDSB.ToString.Replace(" ", "")
                Else
                    Console.WriteLine("UserID only Numeric")
                    Threading.Thread.Sleep(2500)
                    End
                End If
            End If
            res = GetPrivateProfileString("data", "APIKEY", "", APISB, APISB.Capacity, DIR & "ovpnapi.ini")
            If APISB.ToString = "" Then
                Console.WriteLine("please add your ApiKey to the configfile")
                bInvalid = True
            Else
                apiKey = APISB.ToString
            End If
            res = GetPrivateProfileString("data", "OCFGTYPE", "", CTYPSB, CTYPSB.Capacity, DIR & "ovpnapi.ini")
            If CTYPSB.ToString = "" Then
                Console.WriteLine("please add your configfile type to the configfile")
                bInvalid = True
            Else
                OCFGTYPE = CTYPSB.ToString
            End If

            res = GetPrivateProfileString("data", "exe_autoupdate", "", UPSB, UPSB.Capacity, DIR & "ovpnapi.ini")
            If UPSB.ToString = "" Then
                Console.WriteLine("please add your autoupdate setting to the configfile")
                bInvalid = True
            Else
                bAutoUpdate = UPSB.ToString
            End If

            If bInvalid = True Then
                Console.WriteLine("update your config first.")
                Threading.Thread.Sleep(3000)
                End
            End If
        Else
            Console.WriteLine("configfile doesn't exist")
            Console.Write("create configfile? (Y)es / (N)o : ")
            Dim status As String
            Dim iniContent As String
            Dim tmpUID As String
            Dim tmpAPI As String
            Dim tmpOTYPE As String
            Dim tmpAuto As String
            status = Console.ReadLine()
            If status.ToUpper = "Y" Then
                IO.File.Create(DIR & "ovpnapi.ini").Close()
                Console.Write("oVPN UserID : ")
                tmpUID = Console.ReadLine()
                Console.Write("oVPN ApiKey : ")
                tmpAPI = Console.ReadLine()
                Console.Write("oVPN CFG Type ? win / lin / and / mac : ")
                tmpOTYPE = Console.ReadLine()
                Console.Write("exe autoupdate? (Y)es / (N)o : ")
                tmpAuto = Console.ReadLine()
                If tmpAuto.ToUpper = "Y" Then
                    tmpAuto = True
                Else
                    tmpAuto = False
                End If

                If tmpOTYPE = "win" Or tmpOTYPE = "lin" Or tmpOTYPE = "and" Or tmpOTYPE = "mac" Then
                    iniContent = "[data]" & vbNewLine & "USERID=" & tmpUID & vbNewLine & "APIKEY=" & tmpAPI & vbNewLine & "OCFGTYPE=" & tmpOTYPE & vbNewLine & "exe_autoupdate=" & tmpAuto.ToLower
                Else
                    Console.WriteLine("only win , lin , and or mac")
                    tmpOTYPE = "win"
                    iniContent = "[data]" & vbNewLine & "USERID=" & tmpUID & vbNewLine & "APIKEY=" & tmpAPI & vbNewLine & "OCFGTYPE=" & tmpOTYPE & vbNewLine & "exe_autoupdate=" & tmpAuto.ToLower
                End If

                IO.File.WriteAllText(DIR & "ovpnapi.ini", iniContent)
                Console.WriteLine("please restart updater.")
                Threading.Thread.Sleep(5000)
                End

            End If

        End If


    End Sub
    Function requestcerts()
        Try
            Dim Request As HttpWebRequest = CType(WebRequest.Create(URL), HttpWebRequest)
            Request.Method = "POST"
            Request.ContentType = "application/x-www-form-urlencoded"
            Request.UserAgent = "oVPN WIN Update"
            Dim Post As String = "uid=" & UID & "&apikey=" & apiKey & "&action=requestcerts"
            Dim byteArray() As Byte = Encoding.UTF8.GetBytes(Post)
            Request.ContentLength = byteArray.Length
            Dim DataStream As Stream = Request.GetRequestStream()
            DataStream.Write(byteArray, 0, byteArray.Length)
            DataStream.Close()
            Dim Response As HttpWebResponse = Request.GetResponse()
            DataStream = Response.GetResponseStream()
            Dim reader As New StreamReader(DataStream)
            Dim ServerResponse As String = reader.ReadToEnd()
            reader.Close()
            DataStream.Close()
            Response.Close()
            Return ServerResponse
        Catch ex As Exception
            Return "Cert request doesn't work at this moment please try again later"
        End Try
    End Function
    Function getHash()
        Try
            Dim wc As New Net.WebClient
            Dim strResponse As String
            strResponse = wc.DownloadString(URL & "?action=getlatestwinupdaterhash")
            If Len(strResponse) = 128 Then
                Return strResponse
            Else
                Return "No SHA512 hash found!"
            End If
        Catch ex As Exception
            Return "hash request doesn't work at this moment please try again later"
        End Try
    End Function
    Function getLastUpdater()
        Try
            Dim wc As New Net.WebClient
            Return wc.DownloadString("https://vcp.ovpn.to/xxxapi.php?action=getlatestwinupdaterversion")
        Catch ex As Exception
            Return "Cant get last Win Updater Version. try again later"
        End Try
    End Function
    Function getOVPNVersion()
        Try
            Dim Request As HttpWebRequest = CType(WebRequest.Create(URL), HttpWebRequest)
            Request.Method = "POST"
            Request.ContentType = "application/x-www-form-urlencoded"
            Request.UserAgent = "oVPN WIN Update"
            Dim Post As String = "uid=" & UID & "&apikey=" & apiKey & "&action=getovpnversion"
            Dim byteArray() As Byte = Encoding.UTF8.GetBytes(Post)
            Request.ContentLength = byteArray.Length
            Dim DataStream As Stream = Request.GetRequestStream()
            DataStream.Write(byteArray, 0, byteArray.Length)
            DataStream.Close()
            Dim Response As HttpWebResponse = Request.GetResponse()
            DataStream = Response.GetResponseStream()
            Dim reader As New StreamReader(DataStream)
            Dim ServerResponse As String = reader.ReadToEnd()
            reader.Close()
            DataStream.Close()
            Response.Close()
            Return ServerResponse
        Catch ex As Exception
            Return "Cant get oVPN Version. try again later"
        End Try
    End Function
    Function lastupdate()
        Try
            Dim Request As HttpWebRequest = CType(WebRequest.Create(URL), HttpWebRequest)
            Request.Method = "POST"
            Request.ContentType = "application/x-www-form-urlencoded"
            Request.UserAgent = "oVPN WIN Update"
            Dim Post As String = "uid=" & UID & "&apikey=" & apiKey & "&action=lastupdate"
            Dim byteArray() As Byte = Encoding.UTF8.GetBytes(Post)
            Request.ContentLength = byteArray.Length
            Dim DataStream As Stream = Request.GetRequestStream()
            DataStream.Write(byteArray, 0, byteArray.Length)
            DataStream.Close()
            Dim Response As HttpWebResponse = Request.GetResponse()
            DataStream = Response.GetResponseStream()
            Dim reader As New StreamReader(DataStream)
            Dim ServerResponse As String = reader.ReadToEnd()
            reader.Close()
            DataStream.Close()
            Response.Close()
            Return ServerResponse
        Catch ex As Exception
            Return ("Error")
        End Try
    End Function
    Sub getconfigs()
        Try
            Console.WriteLine("Requesting oVPN ConfigUpdate:")
            Dim client As New System.Net.WebClient
            client.DownloadFile(URL & "?uid=" & UID & "&otp=" & otp & "&action=getconfigs&version=" & CVERSION & "&type=" & OCFGTYPE, DIR & "config.zip")
            Console.WriteLine("done. configs.zip saved to " & DIR & "config.zip")
        Catch ex As Exception
            Console.WriteLine("Cert request doesn't work at this moment please try again later")
        End Try
    End Sub
    Sub getCerts()
        Try
            Console.WriteLine("Requesting oVPN Certificates: ")
            Dim client As New System.Net.WebClient
            client.DownloadFile(URL & "?uid=" & UID & "&otp=" & otp & "&action=getcerts", DIR & "certs.zip")
            Console.WriteLine("done. certs.zip saved to " & DIR & "certs.zip")
            IO.File.WriteAllText(DIR & "lastovpntoupdate.txt", lastUpdateOnline)
        Catch ex As Exception
            Console.WriteLine("Cert request doesn't work at this moment please try again later")
        End Try
    End Sub

    Sub getOPENVPNVersion()
        Dim oProcess As New Process()
        Dim oStartInfo As New ProcessStartInfo("cmd.exe")
        oStartInfo.Arguments = "/c openvpn --version"
        oStartInfo.UseShellExecute = False
        oStartInfo.RedirectStandardOutput = True
        oProcess.StartInfo = oStartInfo
        oProcess.Start()

        Dim sOutput As String
        Using oStreamReader As System.IO.StreamReader = oProcess.StandardOutput
            sOutput = oStreamReader.ReadToEnd()
        End Using
        sOutput = Left$(sOutput, 13)

        sOutput = Split(sOutput, " ")(1).Replace(".", "")
        If UBound(Split(getOVPNVersion(), ":")) > 0 Then
            If Not Split(getOVPNVersion(), ":")(1) = sOutput Then
                Console.ForegroundColor = ConsoleColor.Magenta
                Console.WriteLine("Warning! Update your openVPN " & Split(getOVPNVersion(), ":")(1) & " Client manually")
                Console.ForegroundColor = ConsoleColor.White
            Else
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("openVPN-Client is up2date. " & Split(getOVPNVersion(), ":")(1))
                Console.ForegroundColor = ConsoleColor.White
            End If
        End If

        If sOutput < 234 Then
            CVERSION = "22x"
        Else
            CVERSION = "23x"
        End If

    End Sub
    Sub getLastUpdaterBin()
        Dim fileHash As String = ""
        Dim wc As New Net.WebClient
        Try
            Console.WriteLine("downloading update...")

            wc.DownloadFile("https://vcp.ovpn.to/files/winupdater/ovpnapi.exe", DIR & "ovpnapi" + lastUpdaterV + ".exe")
            Console.WriteLine("update finshed!")
            fileHash = GetSHA512(DIR & "ovpnapi" + lastUpdaterV + ".exe")

            If fileHash.ToLower = getHash().ToString.ToLower Then
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine("Update ok ! hash valid")
                Console.ForegroundColor = ConsoleColor.White
                Threading.Thread.Sleep(1000)
                If IO.File.Exists(DIR & "ovpnapi" + lastUpdaterV + ".exe") Then
                    Process.Start(DIR & "ovpnapi" + lastUpdaterV + ".exe")
                End If
                End
            Else
                Console.ForegroundColor = ConsoleColor.Magenta
                Console.WriteLine("Update not valid")
                Console.ForegroundColor = ConsoleColor.White
            End If
            Threading.Thread.Sleep(10000)
        Catch ex As Exception
            MsgBox("can not download new updater")
        End Try
    End Sub
End Module