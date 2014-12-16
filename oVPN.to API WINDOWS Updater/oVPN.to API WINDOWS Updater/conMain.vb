' oVPN.to Updater by twink0r
' thanks to oVPN.to - MrNice
' appVersion 0002
' dotNET Framework 4


' Copyright (c) <2014> <twink0r & MrNice>
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
' The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES' OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
Imports System.IO, System.Net, System.Text
Module conMain
    Const URL As String = "https://vcp.ovpn.to/xxxapi.php"
    Const appTitle As String = "oVPN.to Win Updater"
    Const appVersion As String = "0002"
    Dim otp As String
    Dim DIR As String = System.AppDomain.CurrentDomain.BaseDirectory
    Dim CVERSION As String
    Dim UID As String
    Dim apiKey As String
    Dim OCFGTYPE As String
    Dim lastUpdateFile As String
    Dim lastUpdateOnline As String

    Sub Main()
        'AppTitle
        Console.Title = appTitle
        Console.ForegroundColor = ConsoleColor.White
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
                Console.Write("Update Certs and Configs now? (Y)es / (N)o :")
                Console.ForegroundColor = ConsoleColor.White
                Dim choice As String
                choice = Console.ReadLine()
                If choice.ToUpper = "Y" Then
                    startUpdate()
                End If

            Else
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
            End If
        Else
            IO.File.Create(DIR & "lastovpntoupdate.txt").Close()
            IO.File.WriteAllText(DIR & "lastovpntoupdate.txt", 0)
        End If


        While True
            Console.ReadLine()
        End While

    End Sub
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
        If UBound(Split(getLastUpdater, ":")) > 0 Then
            If Not Split(getLastUpdater(), ":")(1) = appVersion Then
                Console.ForegroundColor = ConsoleColor.Magenta
                Console.WriteLine("Please update your openVPN-Client!", MsgBoxStyle.Critical, "Newer openVPN Version available!")
                Console.Title = appTitle & " / openVPN-Client not up 2 date"
                Console.ForegroundColor = ConsoleColor.White
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
            Dim bInvalid As Boolean
            Dim res As Integer


            UIDSB = New StringBuilder(350)
            APISB = New StringBuilder(350)
            CTYPSB = New StringBuilder(350)
            res = GetPrivateProfileString("data", "USERID", "", UIDSB, UIDSB.Capacity, DIR & "ovpnapi.ini")
            If UIDSB.ToString = "" Then
                Console.WriteLine("please add your UserID to the configfile")
                bInvalid = True
            Else
                UID = UIDSB.ToString.Replace(" ", "")
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
            status = Console.ReadLine()
            If status.ToUpper = "Y" Then
                IO.File.Create(DIR & "ovpnapi.ini").Close()
                Console.Write("oVPN UserID : ")
                tmpUID = Console.ReadLine()
                Console.Write("oVPN ApiKey : ")
                tmpAPI = Console.ReadLine()
                Console.Write("oVPN CFG Type ? win / lin / and / mac : ")
                tmpOTYPE = Console.ReadLine()
                If tmpOTYPE = "win" Or tmpOTYPE = "lin" Or tmpOTYPE = "and" Or tmpOTYPE = "mac" Then
                    iniContent = "[data]" & vbNewLine & "USERID=" & tmpUID & vbNewLine & "APIKEY=" & tmpAPI & vbNewLine & "OCFGTYPE=" & tmpOTYPE
                Else
                    Console.WriteLine("only win , lin , and or mac")
                    tmpOTYPE = "win"
                    iniContent = "[data]" & vbNewLine & "USERID=" & tmpUID & vbNewLine & "APIKEY=" & tmpAPI & vbNewLine & "OCFGTYPE=" & tmpOTYPE
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
    Function getLastUpdater()
        Try
            Dim Request As HttpWebRequest = CType(WebRequest.Create(URL), HttpWebRequest)
            Request.Method = "POST"
            Request.ContentType = "application/x-www-form-urlencoded"
            Request.UserAgent = "oVPN WIN Update"
            Dim Post As String = "uid=" & UID & "&apikey=" & apiKey & "&action=getlatestwinupdaterversion"
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
        If UBound(Split(getOVPNVersion(), ":")) > 1 Then
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
End Module
