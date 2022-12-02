Imports System.Runtime.CompilerServices

Public Module Extensions

    <Extension()>
    Public Sub Save(image As BitmapImage, filePath As String)
        Dim encoder As BitmapEncoder = New PngBitmapEncoder()
        encoder.Frames.Add(BitmapFrame.Create(image))

        Using fileStream = New System.IO.FileStream(filePath, System.IO.FileMode.Create)
            encoder.Save(fileStream)
        End Using
    End Sub
End Module
