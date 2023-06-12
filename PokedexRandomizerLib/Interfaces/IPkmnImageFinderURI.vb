Public Interface IPkmnImageFinderURI
    Function GetPkmnIconURIList(pkmnInfo As PkmnInfo) As List(Of String)
    Function GetPkmnImageURIList(pkmnInfo As PkmnInfo) As List(Of String)
End Interface
