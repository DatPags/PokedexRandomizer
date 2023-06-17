Imports System.IO
Imports System.Runtime.CompilerServices
Imports PokedexRandomizerLib
Imports SixLabors.ImageSharp

Module ExtensionsWPF
    Private TEMP_DIRECTORY As String = IO.Path.Combine(Util.DIRECTORY_BASE, "temp")

    <Extension()>
    Public Function ToMediaColor(fromColor As System.Drawing.Color) As System.Windows.Media.Color
        Return System.Windows.Media.Color.FromArgb(fromColor.A, fromColor.R, fromColor.G, fromColor.B)
    End Function

    <Extension()>
    Public Function ToBrush(fromColor As System.Drawing.Color) As Brush
        Return New SolidColorBrush(fromColor.ToMediaColor)
    End Function

    <Extension()>
    Public Function ToBitmapImage(image As SixLabors.ImageSharp.Image) As System.Windows.Media.Imaging.BitmapImage
        ' no good way to convert image to bitmapimage, so save it to a temp file and re-load it
        ' mutex so deleting the temp directory won't screw up other threads
        SyncLock UtilWPF.bitmapLock
            Dim rand As New Random()
            ' 1/32 chance to clear the temp directory
            If rand.Next() Mod 32 = 0 Then
                For Each path As String In IO.Directory.EnumerateFiles(TEMP_DIRECTORY)
                    Try
                        File.Delete(path)
                    Catch ex As IOException
                        ' ignore files currently in use (currently rendered on the screen)
                    End Try
                Next
            End If
            If Not Util.CreateDirectory(TEMP_DIRECTORY) Then Return Nothing

            Dim tempPath As String = IO.Path.Combine(TEMP_DIRECTORY, $"{rand.Next()}.png")
            image.SaveAsPng(tempPath)

            Dim newImg As New BitmapImage
            newImg.BeginInit()
            newImg.UriSource = New Uri(tempPath)
            newImg.DecodePixelWidth = image.Width
            newImg.EndInit()
            Return newImg
        End SyncLock
    End Function

    <Extension()>
    Public Function GetIcon(pkmn As PkmnInfo, formIndex As Integer) As BitmapImage
        If formIndex < pkmn.iconUris.Count Then
            Dim newImg As New BitmapImage
            newImg.BeginInit()
            newImg.UriSource = New Uri(pkmn.iconUris(formIndex))
            newImg.EndInit()
            Return newImg
        Else
            Return pkmn.icons(formIndex).ToBitmapImage()
        End If
    End Function

    <Extension()>
    Public Function GetImage(pkmn As PkmnInfo, formIndex As Integer) As BitmapImage
        If formIndex < pkmn.imageUris.Count Then
            Dim newImg As New BitmapImage
            newImg.BeginInit()
            newImg.UriSource = New Uri(pkmn.imageUris(formIndex))
            newImg.EndInit()
            Return newImg
        Else
            Return pkmn.images(formIndex).ToBitmapImage()
        End If
    End Function

    <Extension()>
    Public Function GetMoveCategoryImage(infoRetriever As PkmnInfoRetriever, category As String) As BitmapImage
        Dim uri = infoRetriever.FetchMoveCategoryURI(category)
        If uri Is Nothing Then
            Return infoRetriever.FetchMoveCategoryImage(category).ToBitmapImage()
        Else
            Dim newImg As New BitmapImage
            newImg.BeginInit()
            newImg.UriSource = New Uri(uri)
            newImg.EndInit()
            Return newImg
        End If
    End Function
End Module
