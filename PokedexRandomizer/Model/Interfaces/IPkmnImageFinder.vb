Public Interface IPkmnImageFinder
    Function GetPkmnImageListAsync(pkmnInfo As PkmnInfo, settings As Settings, cache As IImageCache) As Task(Of List(Of BitmapImage))
End Interface
