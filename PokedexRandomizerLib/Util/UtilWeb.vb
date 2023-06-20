Imports System.Net.Http
Imports System.Net.Http.Handlers

Public Class UtilWeb
    Public Shared Function DecodeHtmlStringList(strList As List(Of String)) As List(Of String)
        Dim newList = New List(Of String)
        For Each item In strList
            newList.Add(DecodeHtmlString(item))
        Next
        Return newList
    End Function

    Public Shared Function DecodeHtmlString(str As String) As String
        Dim writer = New System.IO.StringWriter
        System.Web.HttpUtility.HtmlDecode(str, writer)
        Return writer.ToString
    End Function

    Public Shared Async Function GetHtmlAsync(url As String) As Task(Of HtmlAgilityPack.HtmlDocument)
        Debug.WriteLine("Getting HTML from URL: " + url)
        Dim html = New HtmlAgilityPack.HtmlDocument
        Dim bytes = Await Util.HttpClient.GetByteArrayAsync(url)
        Dim utf8 = New System.Text.UTF8Encoding
        html.LoadHtml(utf8.GetString(bytes))
        Return html
    End Function

    Public Shared Async Function GetTextFromUrlAsync(url As String) As Task(Of String)
        Debug.WriteLine("Getting text from URL: " + url)
        Return Await Util.HttpClient.GetStringAsync(url)
    End Function

    Public Shared Async Function GetBytesFromUrlAsync(url As String, Optional pi As IProgress(Of String) = Nothing) As Task(Of Byte())
        Debug.WriteLine("Getting bytes from URL: " + url)
        If pi IsNot Nothing Then
            Dim handler As New HttpClientHandler() With {.AllowAutoRedirect = True}
            Dim ph As New ProgressMessageHandler(handler)
            AddHandler ph.HttpReceiveProgress, Sub(sender, args)
                                                   pi.Report("Downloading new data: " + String.Format("{0:P1}", args.BytesTransferred / Convert.ToDouble(args.TotalBytes)))
                                               End Sub
            Dim reportClient = New HttpClient(ph)
            Return Await reportClient.GetByteArrayAsync(url)
        Else
            Return Await Util.HttpClient.GetByteArrayAsync(url)
        End If
    End Function
End Class
