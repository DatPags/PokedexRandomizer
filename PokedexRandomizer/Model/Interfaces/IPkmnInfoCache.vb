Public Interface IPkmnInfoCache
    Function GetPkmnInfoIfExists(key As String) As PkmnInfo?
    Sub StorePkmnInfoInCache(info As PkmnInfo, key As String)
End Interface
