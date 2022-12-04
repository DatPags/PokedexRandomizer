Module Util
    Private ReadOnly _typeColorMap As IDictionary(Of String, Brush) = New Dictionary(Of String, Brush) From {
        {"Normal", New SolidColorBrush(ColorConverter.ConvertFromString("#FFAAAA99"))},
        {"Bug", New SolidColorBrush(ColorConverter.ConvertFromString("#FFAABB22"))},
        {"Dark", New SolidColorBrush(ColorConverter.ConvertFromString("#FF775544"))},
        {"Dragon", New SolidColorBrush(ColorConverter.ConvertFromString("#FF7766EE"))},
        {"Electric", New SolidColorBrush(ColorConverter.ConvertFromString("#FFFFCC33"))},
        {"Fairy", New SolidColorBrush(ColorConverter.ConvertFromString("#FFEE99EE"))},
        {"Fighting", New SolidColorBrush(ColorConverter.ConvertFromString("#FFBB5544"))},
        {"Fire", New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF4422"))},
        {"Flying", New SolidColorBrush(ColorConverter.ConvertFromString("#FF8899FF"))},
        {"Ghost", New SolidColorBrush(ColorConverter.ConvertFromString("#FF6666BB"))},
        {"Grass", New SolidColorBrush(ColorConverter.ConvertFromString("#FF77CC55"))},
        {"Ground", New SolidColorBrush(ColorConverter.ConvertFromString("#FFDDBB55"))},
        {"Ice", New SolidColorBrush(ColorConverter.ConvertFromString("#FF66CCFF"))},
        {"Poison", New SolidColorBrush(ColorConverter.ConvertFromString("#FFAA5599"))},
        {"Psychic", New SolidColorBrush(ColorConverter.ConvertFromString("#FFFF5599"))},
        {"Rock", New SolidColorBrush(ColorConverter.ConvertFromString("#FFBBAA66"))},
        {"Steel", New SolidColorBrush(ColorConverter.ConvertFromString("#FFAAAABB"))},
        {"Water", New SolidColorBrush(ColorConverter.ConvertFromString("#FF3399FF"))}}

    Private ReadOnly _gameColorMap As IDictionary(Of String, Brush) = New Dictionary(Of String, Brush) From {
        {"Red", New SolidColorBrush(ColorConverter.ConvertFromString("#FFC03028"))},
        {"Blue", New SolidColorBrush(ColorConverter.ConvertFromString("#FF5D81D6"))},
        {"Yellow", New SolidColorBrush(ColorConverter.ConvertFromString("#FFD6B11F"))},
        {"Gold", New SolidColorBrush(ColorConverter.ConvertFromString("#FFAD9551"))},
        {"Silver", New SolidColorBrush(ColorConverter.ConvertFromString("#FF9797AB"))},
        {"Crystal", New SolidColorBrush(ColorConverter.ConvertFromString("#FF87BFBF"))},
        {"Ruby", New SolidColorBrush(ColorConverter.ConvertFromString("#FFC03028"))},
        {"Sapphire", New SolidColorBrush(ColorConverter.ConvertFromString("#FF5D81D6"))},
        {"Emerald", New SolidColorBrush(ColorConverter.ConvertFromString("#FF909E1B"))},
        {"FireRed", New SolidColorBrush(ColorConverter.ConvertFromString("#FFC03028"))},
        {"LeafGreen", New SolidColorBrush(ColorConverter.ConvertFromString("#FF65A843"))},
        {"Diamond", New SolidColorBrush(ColorConverter.ConvertFromString("#FF8471BD"))},
        {"Pearl", New SolidColorBrush(ColorConverter.ConvertFromString("#FFDE4F7A"))},
        {"Platinum", New SolidColorBrush(ColorConverter.ConvertFromString("#FF9797AB"))},
        {"HeartGold", New SolidColorBrush(ColorConverter.ConvertFromString("#FFAD9551"))},
        {"SoulSilver", New SolidColorBrush(ColorConverter.ConvertFromString("#FF9797AB"))},
        {"Black", New SolidColorBrush(ColorConverter.ConvertFromString("#FF574438"))},
        {"White", New SolidColorBrush(ColorConverter.ConvertFromString("#FF9797AB"))},
        {"Black 2", New SolidColorBrush(ColorConverter.ConvertFromString("#FF574438"))},
        {"White 2", New SolidColorBrush(ColorConverter.ConvertFromString("#FF9797AB"))},
        {"X", New SolidColorBrush(ColorConverter.ConvertFromString("#FF5D81D6"))},
        {"Y", New SolidColorBrush(ColorConverter.ConvertFromString("#FFC03028"))},
        {"Omega Ruby", New SolidColorBrush(ColorConverter.ConvertFromString("#FFC03028"))},
        {"Alpha Sapphire", New SolidColorBrush(ColorConverter.ConvertFromString("#FF5D81D6"))},
        {"Sun", New SolidColorBrush(ColorConverter.ConvertFromString("#FFDB8624"))},
        {"Moon", New SolidColorBrush(ColorConverter.ConvertFromString("#FF7038F8"))},
        {"Ultra Sun", New SolidColorBrush(ColorConverter.ConvertFromString("#FFDB8624"))},
        {"Ultra Moon", New SolidColorBrush(ColorConverter.ConvertFromString("#FF7038F8"))},
        {"Let's Go Pikachu", New SolidColorBrush(ColorConverter.ConvertFromString("#FFD6B11F"))},
        {"Let's Go Eevee", New SolidColorBrush(ColorConverter.ConvertFromString("#FFAC8639"))},
        {"Sword", New SolidColorBrush(ColorConverter.ConvertFromString("#FF5D81D6"))},
        {"Shield", New SolidColorBrush(ColorConverter.ConvertFromString("#FFC03028"))},
        {"Brilliant Diamond", New SolidColorBrush(ColorConverter.ConvertFromString("#FF8471BD"))},
        {"Shining Pearl", New SolidColorBrush(ColorConverter.ConvertFromString("#FFDE4F7A"))},
        {"Legends: Arceus", New SolidColorBrush(ColorConverter.ConvertFromString("#FF65A843"))},
        {"Scarlet", New SolidColorBrush(ColorConverter.ConvertFromString("#FFC92127"))},
        {"Violet", New SolidColorBrush(ColorConverter.ConvertFromString("#FF8B2E8B"))}}

    Public Function Get_Type_Color(typeName As String) As Brush
        If _typeColorMap.ContainsKey(typeName) Then
            Return _typeColorMap(typeName)
        Else
            Return Brushes.White
        End If
    End Function

    Public Function Get_Game_Color(gameName As String) As Brush
        Dim firstCommaIndex = gameName.IndexOf(",")
        Dim firstParenIndex = gameName.IndexOf("(")
        Dim firstGame As String
        If firstCommaIndex < 0 And firstParenIndex < 0 Then
            firstGame = gameName
        ElseIf firstCommaIndex >= 0 Then
            firstGame = gameName.Substring(0, firstCommaIndex)
        ElseIf firstParenIndex >= 0 Then
            firstGame = gameName.Substring(0, firstParenIndex - 1)
        Else
            Return Brushes.Black
        End If
        If _gameColorMap.ContainsKey(firstGame) Then
            Return _gameColorMap(firstGame)
        Else
            Return Brushes.Black
        End If
    End Function

    <System.Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Explicit)>
    Structure Int32Union
        <System.Runtime.InteropServices.FieldOffset(0)>
        Public Int32 As Integer
        <System.Runtime.InteropServices.FieldOffset(0)>
        Public UInt32 As UInteger
    End Structure

    Public Function Int32_To_UInt32(i As Integer) As UInteger
        Dim u As New Int32Union With {.Int32 = i}
        Return u.UInt32
    End Function
End Module
