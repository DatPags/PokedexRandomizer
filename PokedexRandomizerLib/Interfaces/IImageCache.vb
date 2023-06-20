Imports SixLabors.ImageSharp

Public Interface IImageCache
    Function GetImageIfExists(key As String) As Image
    Sub StoreImageInCache(image As Image, key As String)
    Function ClearCache() As Boolean
End Interface
