Public Interface IPkmnImageFinder
    Sub Init_Pkmn_Images()
    Function Get_All_Images_For_Pkmn(pkmnInfo As Pkmn, settings As Settings) As Task(Of List(Of BitmapImage))
End Interface
