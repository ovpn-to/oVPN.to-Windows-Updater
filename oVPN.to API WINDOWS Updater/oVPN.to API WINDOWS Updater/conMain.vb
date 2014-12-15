Imports System.IO, System.Net, System.Text
Module conMain
    Const URL As String = "https://vcp.ovpn.to/xxxapi.php"
    Const appTitle As String = "oVPN.to Win Updater"
    Const appVersion As String = "0001"
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
                Console.WriteLine("Configs out2date")
                Console.Write("Update now? (Y)es / (N)o :")
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
                    Console.WriteLine("all good ! all things up to date")
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
                MsgBox("Client not up 2 date.", MsgBoxStyle.Critical, "Warning! out 2 date")
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
            Dim res As Integer


            UIDSB = New StringBuilder(350)
            APISB = New StringBuilder(350)
            CTYPSB = New StringBuilder(350)
            res = GetPrivateProfileString("data", "USERID", "", UIDSB, UIDSB.Capacity, DIR & "ovpnapi.ini")
            UID = UIDSB.ToString
            res = GetPrivateProfileString("data", "APIKEY", "", APISB, APISB.Capacity, DIR & "ovpnapi.ini")
            apiKey = APISB.ToString
            res = GetPrivateProfileString("data", "OCFGTYPE", "", CTYPSB, CTYPSB.Capacity, DIR & "ovpnapi.ini")
            OCFGTYPE = CTYPSB.ToString

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
                Console.WriteLine("please restart updater")
                Exit Sub
           
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
            Console.WriteLine("done.")
        Catch ex As Exception
            Console.WriteLine("Cert request doesn't work at this moment please try again later")
        End Try
    End Sub
    Sub getCerts()
        Try
            Console.WriteLine("Requesting oVPN Certificates: ")
            Dim client As New System.Net.WebClient
            client.DownloadFile(URL & "?uid=" & UID & "&otp=" & otp & "&action=getcerts", DIR & "certs.zip")
            Console.WriteLine("done.")
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
