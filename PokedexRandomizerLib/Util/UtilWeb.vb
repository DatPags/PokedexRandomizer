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
        Dim html = New HtmlAgilityPack.HtmlDocument
        Dim bytes = Await Util.HttpClient.GetByteArrayAsync(url)
        Dim utf8 = New System.Text.UTF8Encoding
        html.LoadHtml(utf8.GetString(bytes))
        Return html
    End Function

    Public Shared Async Function GetTextFromUrlAsync(url As String) As Task(Of String)
        Return Await Util.HttpClient.GetStringAsync(url)
    End Function
End Class
