﻿Imports System.Drawing
Imports System.Net.Http
Imports Newtonsoft.Json

Public Class Util
    Public Shared DIRECTORY_BASE As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mpagliaro98", "Pokedex Randomizer")

    Public Shared ReadOnly Property HttpClient As HttpClient = New HttpClient

    Private Shared _typeColorMap As IDictionary(Of String, Color)
    Private Shared _gameColorMap As IDictionary(Of String, Color)

    Public Shared Function GetPkmnTypeColor(typeName As String) As Color
        If _typeColorMap.ContainsKey(typeName) Then
            Return _typeColorMap(typeName)
        Else
            Return Color.White
        End If
    End Function

    Public Shared Function GetPkmnGameColor(gameName As String) As Color
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
            Return Color.Black
        End If
        If _gameColorMap.ContainsKey(firstGame) Then
            Return _gameColorMap(firstGame)
        Else
            Return Color.Black
        End If
    End Function

    Public Shared Function CreateDirectory(path As String) As Boolean
        Try
            IO.Directory.CreateDirectory(path)
            Return True
        Catch ex As Exception When TypeOf ex Is IO.IOException Or TypeOf ex Is UnauthorizedAccessException _
            Or TypeOf ex Is IO.PathTooLongException Or TypeOf ex Is IO.DirectoryNotFoundException
            Debug.WriteLine("Error creating directory " & path & ": " & ex.Message)
            Return False
        End Try
    End Function

    <System.Runtime.InteropServices.StructLayout(Runtime.InteropServices.LayoutKind.Explicit)>
    Structure Int32Union
        <System.Runtime.InteropServices.FieldOffset(0)>
        Public Int32 As Integer
        <System.Runtime.InteropServices.FieldOffset(0)>
        Public UInt32 As UInteger
    End Structure

    Public Shared Function IntegerToUInteger(i As Integer) As UInteger
        Dim u As New Int32Union With {.Int32 = i}
        Return u.UInt32
    End Function

    Public Shared Async Function CreateDataArchive() As Task
        '--create the working directory
        Dim workingDir As String = IO.Path.Combine(DIRECTORY_BASE, "working")
        If IO.Directory.Exists(workingDir) Then IO.Directory.Delete(workingDir, True)
        CreateDirectory(workingDir)

        '--generate all data
        Await PkmnInfoFinderPokemonDB.CreateJSONFile()
        Await PkmnImageFinderLocal.CreateJSONFileAndImageArchive(Environment.GetEnvironmentVariable("iconsDumpPath"), Environment.GetEnvironmentVariable("imagesDumpPath"))
        Await CreateTypeColorMap()
        Await CreateGameColorMap()

        '--zip up the data and delete the working directory
        Debug.WriteLine("Starting zip file creation...")
        IO.Compression.ZipFile.CreateFromDirectory(workingDir, IO.Path.Combine(DIRECTORY_BASE, "data.zip"), IO.Compression.CompressionLevel.Optimal, False)
        IO.Directory.Delete(workingDir, True)
        Debug.WriteLine("Zip file created")
    End Function

    Private Shared Async Function CreateTypeColorMap() As Task
        Dim typeColorMap As New Dictionary(Of String, Color) From {
            {"Normal", Color.FromArgb(&HFFAAAA99)},
            {"Bug", Color.FromArgb(&HFFAABB22)},
            {"Dark", Color.FromArgb(&HFF775544)},
            {"Dragon", Color.FromArgb(&HFF7766EE)},
            {"Electric", Color.FromArgb(&HFFFFCC33)},
            {"Fairy", Color.FromArgb(&HFFEE99EE)},
            {"Fighting", Color.FromArgb(&HFFBB5544)},
            {"Fire", Color.FromArgb(&HFFFF4422)},
            {"Flying", Color.FromArgb(&HFF8899FF)},
            {"Ghost", Color.FromArgb(&HFF6666BB)},
            {"Grass", Color.FromArgb(&HFF77CC55)},
            {"Ground", Color.FromArgb(&HFFDDBB55)},
            {"Ice", Color.FromArgb(&HFF66CCFF)},
            {"Poison", Color.FromArgb(&HFFAA5599)},
            {"Psychic", Color.FromArgb(&HFFFF5599)},
            {"Rock", Color.FromArgb(&HFFBBAA66)},
            {"Steel", Color.FromArgb(&HFFAAAABB)},
            {"Water", Color.FromArgb(&HFF3399FF)}
        }
        Dim json = JsonConvert.SerializeObject(typeColorMap)
        Await IO.File.WriteAllTextAsync(IO.Path.Combine(DIRECTORY_BASE, "working", "typecolormap.json"), json)
    End Function

    Private Shared Async Function CreateGameColorMap() As Task
        Dim gameColorMap As New Dictionary(Of String, Color) From {
            {"Red", Color.FromArgb(&HFFC03028)},
            {"Blue", Color.FromArgb(&HFF5D81D6)},
            {"Yellow", Color.FromArgb(&HFFD6B11F)},
            {"Gold", Color.FromArgb(&HFFAD9551)},
            {"Silver", Color.FromArgb(&HFF9797AB)},
            {"Crystal", Color.FromArgb(&HFF87BFBF)},
            {"Ruby", Color.FromArgb(&HFFC03028)},
            {"Sapphire", Color.FromArgb(&HFF5D81D6)},
            {"Emerald", Color.FromArgb(&HFF909E1B)},
            {"FireRed", Color.FromArgb(&HFFC03028)},
            {"LeafGreen", Color.FromArgb(&HFF65A843)},
            {"Diamond", Color.FromArgb(&HFF8471BD)},
            {"Pearl", Color.FromArgb(&HFFDE4F7A)},
            {"Platinum", Color.FromArgb(&HFF9797AB)},
            {"HeartGold", Color.FromArgb(&HFFAD9551)},
            {"SoulSilver", Color.FromArgb(&HFF9797AB)},
            {"Black", Color.FromArgb(&HFF574438)},
            {"White", Color.FromArgb(&HFF9797AB)},
            {"Black 2", Color.FromArgb(&HFF574438)},
            {"White 2", Color.FromArgb(&HFF9797AB)},
            {"X", Color.FromArgb(&HFF5D81D6)},
            {"Y", Color.FromArgb(&HFFC03028)},
            {"Omega Ruby", Color.FromArgb(&HFFC03028)},
            {"Alpha Sapphire", Color.FromArgb(&HFF5D81D6)},
            {"Sun", Color.FromArgb(&HFFDB8624)},
            {"Moon", Color.FromArgb(&HFF7038F8)},
            {"Ultra Sun", Color.FromArgb(&HFFDB8624)},
            {"Ultra Moon", Color.FromArgb(&HFF7038F8)},
            {"Let's Go Pikachu", Color.FromArgb(&HFFD6B11F)},
            {"Let's Go Eevee", Color.FromArgb(&HFFAC8639)},
            {"Sword", Color.FromArgb(&HFF5D81D6)},
            {"Shield", Color.FromArgb(&HFFC03028)},
            {"Brilliant Diamond", Color.FromArgb(&HFF8471BD)},
            {"Shining Pearl", Color.FromArgb(&HFFDE4F7A)},
            {"Legends: Arceus", Color.FromArgb(&HFF65A843)},
            {"Scarlet", Color.FromArgb(&HFFC92127)},
            {"Violet", Color.FromArgb(&HFF8B2E8B)}
        }
        Dim json = JsonConvert.SerializeObject(gameColorMap)
        Await IO.File.WriteAllTextAsync(IO.Path.Combine(DIRECTORY_BASE, "working", "gamecolormap.json"), json)
    End Function

    Public Shared Async Function DownloadDataIfNotExists() As Task
        '--exit if data already exists (saved hash exists and matches hash of remote data)
        '--download zip file
        '--extract it to the base directory
        '--delete zip file
        '--download and save the hash of the zip file
    End Function

    Public Shared Async Function LoadExtraData() As Task
        Dim json = Await IO.File.ReadAllTextAsync(IO.Path.Combine(DIRECTORY_BASE, "typecolormap.json"))
        _typeColorMap = JsonConvert.DeserializeObject(Of Dictionary(Of String, Color))(json)

        json = Await IO.File.ReadAllTextAsync(IO.Path.Combine(DIRECTORY_BASE, "gamecolormap.json"))
        _gameColorMap = JsonConvert.DeserializeObject(Of Dictionary(Of String, Color))(json)
    End Function
End Class
