Imports System.Text
Imports Newtonsoft.Json

Public Class PkmnInfoFinderLocal
    Implements IPkmnInfoFinder

    Private Shared DIRECTORY_BASE As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mpagliaro98", "Pokedex Randomizer")
    Private Shared DATA_FILE As String = IO.Path.Combine(DIRECTORY_BASE, "pkmn.json")

    Private data As Dictionary(Of Integer, PkmnInfo)

    Public Shared Async Function CreateSelf() As Task(Of IPkmnInfoFinder)
        Dim obj As New PkmnInfoFinderLocal
        If Not IO.File.Exists(DATA_FILE) Then
            '--TODO download from the web
            PkmnInfoFinderPokemonDB.CreateJSONFile()
        End If
        Dim bytes = Await System.IO.File.ReadAllBytesAsync(DATA_FILE)
        Dim json As String = Encoding.UTF8.GetString(bytes)
        obj.data = JsonConvert.DeserializeObject(Of Dictionary(Of Integer, PkmnInfo))(json)
        Return obj
    End Function

    Public Function GetTotalNumOfPkmn() As Integer Implements IPkmnInfoFinder.GetTotalNumOfPkmn
        Return data.Keys.Count
    End Function

    Public Function DoesPkmnExist(pkmnName As String) As Boolean Implements IPkmnInfoFinder.DoesPkmnExist
        Return data.ToList().Exists(Function(p) p.Value.name = pkmnName)
    End Function

    Public Function DoesPkmnExist(pkmnNumber As Integer) As Boolean Implements IPkmnInfoFinder.DoesPkmnExist
        Return data.ContainsKey(pkmnNumber)
    End Function

    Public Function PkmnNameToNumber(pkmnName As String) As Integer Implements IPkmnInfoFinder.PkmnNameToNumber
        Dim kv = data.ToList().Find(Function(p) p.Value.name = pkmnName)
        Return kv.Key
    End Function

    Public Async Function GetPkmnInfoAsync(pkmnNumber As Integer) As Task(Of PkmnInfo) Implements IPkmnInfoFinder.GetPkmnInfoAsync
        Return Await Task.Run(Function() As PkmnInfo
                                  Return data(pkmnNumber)
                              End Function)
    End Function
End Class
