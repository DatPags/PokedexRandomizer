Public Interface IPkmnInfoFinder
    Function GetTotalNumOfPkmn() As Integer
    Function DoesPkmnExist(pkmnName As String) As Boolean
    Function DoesPkmnExist(pkmnNumber As Integer) As Boolean
    Function PkmnNameToNumber(pkmnName As String) As Integer
    Function GetPkmnInfoAsync(pkmnNumber As Integer) As Task(Of PkmnInfo)
End Interface
