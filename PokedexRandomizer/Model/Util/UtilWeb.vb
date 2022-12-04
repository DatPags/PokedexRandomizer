Module UtilWeb
    Public Function DecodeHtmlStringList(strList As List(Of String)) As List(Of String)
        Dim newList = New List(Of String)
        For Each item In strList
            newList.Add(DecodeHtmlString(item))
        Next
        Return newList
    End Function

    Public Function DecodeHtmlString(str As String) As String
        Dim writer = New System.IO.StringWriter
        System.Web.HttpUtility.HtmlDecode(str, writer)
        Return writer.ToString
    End Function

    Public Async Function GetHtmlAsync(url As String) As Task(Of HtmlAgilityPack.HtmlDocument)
        Dim t As Task(Of HtmlAgilityPack.HtmlDocument)
        t = Task.Run(Function() As HtmlAgilityPack.HtmlDocument
                         Dim client As New System.Net.WebClient
                         Dim html = New HtmlAgilityPack.HtmlDocument
                         Dim bytes = client.DownloadData(url)
                         Dim utf8 = New System.Text.UTF8Encoding
                         html.LoadHtml(utf8.GetString(bytes))
                         Return html
                     End Function)
        Return Await t
    End Function

    Public Async Function GetTextFromUrlAsync(url As String) As Task(Of String)
        Dim html As String
        Dim request = System.Net.WebRequest.Create(url)
        Using response = Await request.GetResponseAsync
            Using reader = New System.IO.StreamReader(response.GetResponseStream)
                html = reader.ReadToEnd
            End Using
        End Using
        Return html
    End Function
End Module
