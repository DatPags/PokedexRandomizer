Public Interface IImageCache
    Function GetImageIfExists(key As String) As BitmapImage
    Sub StoreInCache(image As BitmapImage, key As String)
End Interface
