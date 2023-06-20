Imports System.IO
Imports SixLabors.ImageSharp

Public Class UtilImage
    Public Shared Async Function GetImageFromUrlAsync(url As String, settings As Settings, cache As IImageCache) As Task(Of Image)
        Dim cachedImage As Image = If(cache IsNot Nothing AndAlso settings.UseCache, cache.GetImageIfExists(url), Nothing)
        If cachedImage Is Nothing Then
            Debug.WriteLine("Getting image from URL: " + url)
            Dim imageContent = Await Util.HttpClient.GetByteArrayAsync(url)
            Using imageBuffer = New MemoryStream(imageContent)
                cachedImage = Image.Load(imageBuffer)
                If cache IsNot Nothing AndAlso settings.UseCache Then cache.StoreImageInCache(cachedImage, url)
            End Using
        End If
        Return cachedImage
    End Function
End Class
