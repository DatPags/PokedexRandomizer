Public Interface IPkmnImageFinder
    Sub Init_Pkmn_Images()
    Function Get_All_Pkmn_Images(pkmnInfo As PkmnInfo) As Task(Of List(Of BitmapImage))
End Interface
