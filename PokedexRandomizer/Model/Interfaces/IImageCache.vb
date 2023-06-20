Public Interface IImageCache
    Function GetImageIfExists(key As String) As BitmapImage
    Sub StoreImageInCache(image As BitmapImage, key As String)
    Function ClearCache() As Boolean
End Interface
