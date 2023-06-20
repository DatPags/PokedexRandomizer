Imports SixLabors.ImageSharp

Public Interface IPkmnImageFinder
    Function GetPkmnIconListAsync(pkmnInfo As PkmnInfo) As Task(Of List(Of Image))
    Function GetPkmnImageListAsync(pkmnInfo As PkmnInfo) As Task(Of List(Of Image))
End Interface
