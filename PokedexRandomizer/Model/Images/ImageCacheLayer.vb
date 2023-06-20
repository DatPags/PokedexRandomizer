Public Class ImageCacheLayer
    Implements IImageCache

    Private Shared CACHE_DIRECTORY As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mpagliaro98", "Pokedex Randomizer", "images")
    Private Const IMAGE_EXT As String = ".png"
    Private Const CACHE_LENGTH_SECONDS As ULong = 60 * 60 * 24 * 30 '30 days

    Public Function GetImageIfExists(key As String) As BitmapImage Implements IImageCache.GetImageIfExists
        Dim hash As Integer = key.GetHashCode()
        If Not CreateCacheDirectory() Then Return Nothing
        Dim path As String = IO.Path.Combine(CACHE_DIRECTORY, hash.ToString & IMAGE_EXT)
        Try
            'check for cache expiration
            Dim lastWriteTime As Date = IO.File.GetLastWriteTimeUtc(path)
            If Date.UtcNow.AddSeconds(-1 * CACHE_LENGTH_SECONDS) > lastWriteTime Then
                Debug.WriteLine("Cache is expired: " & path)
                Return Nothing
            End If

            Dim bmFinal As BitmapImage
            Using fs As IO.FileStream = IO.File.Open(path, IO.FileMode.Open)
                fs.Position = 0
                bmFinal = New BitmapImage
                bmFinal.BeginInit()
                bmFinal.StreamSource = fs
                bmFinal.CacheOption = BitmapCacheOption.OnLoad
                bmFinal.EndInit()
            End Using
            Debug.WriteLine("Successfully loaded from cache: " & path)
            Return bmFinal
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException Or TypeOf ex Is IO.FileNotFoundException
            Debug.WriteLine("File not found in cache: " & path & " - " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Sub StoreInCache(image As BitmapImage, key As String) Implements IImageCache.StoreInCache
        Dim hash As Integer = key.GetHashCode()
        If Not CreateCacheDirectory() Then Exit Sub
        Dim path As String = IO.Path.Combine(CACHE_DIRECTORY, hash.ToString & IMAGE_EXT)
        Try
            image.Save(path)
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException Or TypeOf ex Is IO.FileNotFoundException
            Debug.WriteLine("File save failed: " & path & " - " & ex.Message)
        End Try
    End Sub

    Private Function CreateCacheDirectory() As Boolean
        Try
            IO.Directory.CreateDirectory(CACHE_DIRECTORY)
            Return True
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException
            Debug.WriteLine("Error creating directory: " & ex.Message)
            Return False
        End Try
    End Function
End Class
