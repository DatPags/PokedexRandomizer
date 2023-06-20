Module UtilImage
    Public Async Function Get_Image_Async(url As String) As Task(Of BitmapImage)
        Dim client As New System.Net.WebClient
        Dim uri As New Uri(url)
        Dim stream = Await client.OpenReadTaskAsync(uri)
        Dim image = System.Drawing.Image.FromStream(stream)
        Return Image_To_Image_Source(image)
    End Function

    Public Function Image_To_Image_Source(bm As System.Drawing.Bitmap) As BitmapImage
        Dim bmFinal As BitmapImage
        Using memory As New System.IO.MemoryStream
            bm.Save(memory, System.Drawing.Imaging.ImageFormat.Png)
            memory.Position = 0
            bmFinal = New BitmapImage
            bmFinal.BeginInit()
            bmFinal.StreamSource = memory
            bmFinal.CacheOption = BitmapCacheOption.OnLoad
            bmFinal.EndInit()
        End Using
        Return bmFinal
    End Function

    Public Function Image_Source_To_Image(bm As BitmapImage) As System.Drawing.Bitmap
        Using memory As New System.IO.MemoryStream
            Dim enc = New BmpBitmapEncoder
            enc.Frames.Add(BitmapFrame.Create(bm))
            enc.Save(memory)
            Dim bitmap = New System.Drawing.Bitmap(memory)
            Return New System.Drawing.Bitmap(bitmap)
        End Using
    End Function

    Public Function Images_Equal(bmi1 As BitmapImage, bmi2 As BitmapImage) As Boolean
        Dim bm1 As System.Drawing.Bitmap = Image_Source_To_Image(bmi1)
        Dim bm2 As System.Drawing.Bitmap = Image_Source_To_Image(bmi2)
        Return Images_Equal(bm1, bm2)
    End Function

    Public Function Images_Equal(bm1 As System.Drawing.Bitmap, bmi2 As BitmapImage) As Boolean
        Dim bm2 As System.Drawing.Bitmap = Image_Source_To_Image(bmi2)
        Return Images_Equal(bm1, bm2)
    End Function

    Public Function Images_Equal(bmi1 As BitmapImage, bm2 As System.Drawing.Bitmap) As Boolean
        Dim bm1 As System.Drawing.Bitmap = Image_Source_To_Image(bmi1)
        Return Images_Equal(bm1, bm2)
    End Function

    Public Function Images_Equal(bm1 As System.Drawing.Bitmap, bm2 As System.Drawing.Bitmap) As Boolean
        Dim threshold As Integer = 10

        If bm1.Width <> bm2.Width OrElse bm1.Height <> bm2.Height Then
            Return False
        End If

        Dim wid As Integer = Math.Min(bm1.Width, bm2.Width)
        Dim hgt As Integer = Math.Min(bm1.Height, bm2.Height)
        Dim bm3 As New System.Drawing.Bitmap(wid, hgt)

        Dim color1, color2 As System.Drawing.Color
        Dim eq_color As System.Drawing.Color = System.Drawing.Color.White
        Dim ne_color As System.Drawing.Color = System.Drawing.Color.Red
        Dim dr, dg, db, diff As Integer
        For x As Integer = 0 To wid - 1
            For y As Integer = 0 To hgt - 1
                color1 = bm1.GetPixel(x, y)
                color2 = bm2.GetPixel(x, y)
                dr = CInt(color1.R) - color2.R
                dg = CInt(color1.G) - color2.G
                db = CInt(color1.B) - color2.B
                diff = dr * dr + dg * dg + db * db
                If diff <= threshold Then
                    bm3.SetPixel(x, y, eq_color)
                Else
                    bm3.SetPixel(x, y, ne_color)
                    Return False
                End If
            Next y
        Next x

        If (bm1.Width <> bm2.Width) OrElse (bm1.Height <> bm2.Height) Then
            Return False
        Else
            Return True
        End If
    End Function
End Module
