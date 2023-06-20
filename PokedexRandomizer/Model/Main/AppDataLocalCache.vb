Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters.Binary

Public Class AppDataLocalCache
    Implements IImageCache, IPkmnInfoCache

    Private Shared CACHE_DIRECTORY_BASE As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mpagliaro98", "Pokedex Randomizer")
    Private Shared CACHE_DIRECTORY_IMAGES As String = IO.Path.Combine(CACHE_DIRECTORY_BASE, "cached_images")
    Private Shared CACHE_DIRECTORY_PKMN As String = IO.Path.Combine(CACHE_DIRECTORY_BASE, "cached_data")
    Private Const IMAGE_EXT As String = ".png"
    Private Const PKMN_EXT As String = ".dat"
    Private Const CACHE_LENGTH_IMAGES_SECONDS As ULong = 60 * 60 * 24 * 30 '30 days
    Private Const CACHE_LENGTH_PKMN_SECONDS As ULong = 60 * 60 * 24 * 3 '3 days

    Public Function GetImageIfExists(key As String) As BitmapImage Implements IImageCache.GetImageIfExists
        If Not CreateDirectory(CACHE_DIRECTORY_IMAGES) Then Return Nothing
        Dim path As String = IO.Path.Combine(CACHE_DIRECTORY_IMAGES, GenerateFilename(key, IMAGE_EXT))
        Try
            'check for cache expiration
            Dim lastWriteTime As Date = IO.File.GetLastWriteTimeUtc(path)
            If Date.UtcNow.AddSeconds(-1 * CACHE_LENGTH_IMAGES_SECONDS) > lastWriteTime Then
                Debug.WriteLine("Image cache is expired: " & path)
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
            Debug.WriteLine("Successfully loaded from image cache: " & path)
            Return bmFinal
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException Or TypeOf ex Is IO.FileNotFoundException
            Debug.WriteLine("File not found in image cache: " & path & " - " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Sub StoreImageInCache(image As BitmapImage, key As String) Implements IImageCache.StoreImageInCache
        If Not CreateDirectory(CACHE_DIRECTORY_IMAGES) Then Exit Sub
        Dim path As String = IO.Path.Combine(CACHE_DIRECTORY_IMAGES, GenerateFilename(key, IMAGE_EXT))
        Try
            image.Save(path)
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException Or TypeOf ex Is IO.FileNotFoundException
            Debug.WriteLine("Image file save failed: " & path & " - " & ex.Message)
            IO.File.Delete(path)
        End Try
    End Sub

    Public Function GetPkmnInfoIfExists(key As String) As PkmnInfo? Implements IPkmnInfoCache.GetPkmnInfoIfExists
        If Not CreateDirectory(CACHE_DIRECTORY_PKMN) Then Return Nothing
        Dim path As String = IO.Path.Combine(CACHE_DIRECTORY_PKMN, GenerateFilename(key, PKMN_EXT))
        Try
            'check for cache expiration
            Dim lastWriteTime As Date = IO.File.GetLastWriteTimeUtc(path)
            If Date.UtcNow.AddSeconds(-1 * CACHE_LENGTH_PKMN_SECONDS) > lastWriteTime Then
                Debug.WriteLine("Data cache is expired: " & path)
                Return Nothing
            End If

            Dim pkmnInfo As New PkmnInfo
            Dim bf As New BinaryFormatter
            Using fs As New FileStream(path, FileMode.Open)
                pkmnInfo = bf.Deserialize(fs)
            End Using
            Debug.WriteLine("Successfully loaded from data cache: " & path)
            Return pkmnInfo
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException Or TypeOf ex Is SerializationException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException Or TypeOf ex Is IO.FileNotFoundException
            Debug.WriteLine("File not found in data cache: " & path & " - " & ex.Message)
            Return Nothing
        End Try
    End Function

    Public Sub StorePkmnInfoInCache(info As PkmnInfo, key As String) Implements IPkmnInfoCache.StorePkmnInfoInCache
        If Not CreateDirectory(CACHE_DIRECTORY_PKMN) Then Exit Sub
        Dim path As String = IO.Path.Combine(CACHE_DIRECTORY_PKMN, GenerateFilename(key, PKMN_EXT))
        Try
            Dim bf As New BinaryFormatter
            Using fs As New FileStream(path, FileMode.Create)
                bf.Serialize(fs, info)
            End Using
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException Or TypeOf ex Is SerializationException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException Or TypeOf ex Is IO.FileNotFoundException
            Debug.WriteLine("Data file save failed: " & path & " - " & ex.Message)
            File.Delete(path)
        End Try
    End Sub

    Private Function GenerateFilename(key As String, ext As String) As String
        Dim hash As UInteger = Int32_To_UInt32(key.GetHashCode())
        Return hash.ToString & ext
    End Function

    Private Function CreateDirectory(path As String) As Boolean
        Try
            IO.Directory.CreateDirectory(path)
            Return True
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException
            Debug.WriteLine("Error creating directory " & path & ": " & ex.Message)
            Return False
        End Try
    End Function

    Public Function IImageCache_ClearCache() As Boolean Implements IImageCache.ClearCache
        Return DeleteDirectory(CACHE_DIRECTORY_IMAGES)
    End Function

    Public Function IPkmnInfoCache_ClearCache() As Boolean Implements IPkmnInfoCache.ClearCache
        Return DeleteDirectory(CACHE_DIRECTORY_PKMN)
    End Function

    Private Function DeleteDirectory(path As String) As Boolean
        Try
            IO.Directory.Delete(path, True)
            Return True
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException
            Debug.WriteLine("Error deleting directory " & path & ": " & ex.Message)
            Return False
        End Try
    End Function
End Class
