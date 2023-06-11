Imports SixLabors.ImageSharp

Public Interface IPkmnImageFinder
    Function GetPkmnIconListAsync(pkmnInfo As PkmnInfo, settings As Settings, cache As IImageCache) As Task(Of List(Of Image))
    Function GetPkmnImageListAsync(pkmnInfo As PkmnInfo, settings As Settings, cache As IImageCache) As Task(Of List(Of Image))
End Interface
