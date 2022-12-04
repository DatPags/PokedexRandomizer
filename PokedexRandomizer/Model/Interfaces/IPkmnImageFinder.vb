Public Interface IPkmnImageFinder
    Function Get_All_Images_For_Pkmn(pkmnInfo As PkmnInfo, settings As Settings) As Task(Of List(Of BitmapImage))
End Interface
