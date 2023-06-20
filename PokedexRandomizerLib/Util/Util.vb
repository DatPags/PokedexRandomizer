﻿Imports System.Drawing
Imports System.IO
Imports System.Security.Cryptography
Imports System.Net.Http
Imports Newtonsoft.Json

Public Class Util
    Public Shared DIRECTORY_BASE As String = IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "mpagliaro98", "Pokedex Randomizer")

    Public Const URL_HASH As String = "https://raw.githubusercontent.com/DatPags/PokedexRandomizer/master/hash.txt"
    Public Const URL_DATA As String = "https://github.com/DatPags/PokedexRandomizer/raw/master/data.zip"

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

    Public Shared Async Function CreateDataArchive(updateData As Boolean) As Task
        '--create the working directory
        Dim workingDir As String = IO.Path.Combine(DIRECTORY_BASE, "working")
        If IO.Directory.Exists(workingDir) Then IO.Directory.Delete(workingDir, True)
        CreateDirectory(workingDir)

        '--generate all data
        If updateData Then
            Await PkmnInfoFinderPokemonDB.CreateJSONFile()
        Else
            Debug.WriteLine("Skipping data file generation, copying from existing")
            IO.File.Copy(IO.Path.Combine(DIRECTORY_BASE, "pkmn.json"), IO.Path.Combine(workingDir, "pkmn.json"), True)
        End If
        Await PkmnImageFinderLocal.CreateJSONFileAndImageArchive(Environment.GetEnvironmentVariable("iconsDumpPath"), Environment.GetEnvironmentVariable("imagesDumpPath"))
        Await CreateTypeColorMap()
        Await CreateGameColorMap()

        '--zip up the data and delete the working directory
        Debug.WriteLine("Starting zip file creation...")
        Dim zipPath As String = IO.Path.Combine(DIRECTORY_BASE, "data.zip")
        If IO.File.Exists(zipPath) Then IO.File.Delete(zipPath)
        IO.Compression.ZipFile.CreateFromDirectory(workingDir, zipPath, IO.Compression.CompressionLevel.Optimal, False)
        IO.Directory.Delete(workingDir, True)
        Debug.WriteLine("Zip file created")

        '--produce the hash of the new data archive
        Dim hash As String = ProduceHashOfFile(zipPath)
        Await IO.File.WriteAllTextAsync(IO.Path.Combine(DIRECTORY_BASE, "newhash.txt"), hash)
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
        Dim hashPath As String = IO.Path.Combine(DIRECTORY_BASE, "hash.txt")
        If IO.File.Exists(hashPath) Then
            Dim hash As String = Await IO.File.ReadAllTextAsync(hashPath)
            Dim hashWeb As String = Await UtilWeb.GetTextFromUrlAsync(URL_HASH)
            If hash = hashWeb Then
                Debug.WriteLine("Hash matches - data is up to date")
                Return
            End If
        End If

        '--download zip file
        Debug.WriteLine("Downloading data archive...")
        Dim bytes() = Await UtilWeb.GetBytesFromUrlAsync(URL_DATA)
        Debug.WriteLine("Saving temporary data archive file...")
        Dim zipPath As String = IO.Path.Combine(DIRECTORY_BASE, "tempdata.zip")
        Await IO.File.WriteAllBytesAsync(zipPath, bytes)
        Debug.WriteLine("Data archive downloaded successfully")

        '--extract it to the base directory
        Debug.WriteLine("Extracting data...")
        IO.Compression.ZipFile.ExtractToDirectory(zipPath, DIRECTORY_BASE, True)
        Debug.WriteLine("Extraction successful")

        '--delete zip file
        IO.File.Delete(zipPath)

        '--download and save the hash of the zip file
        Debug.WriteLine("Updating hash value...")
        Dim newHash As String = Await UtilWeb.GetTextFromUrlAsync(URL_HASH)
        Await IO.File.WriteAllTextAsync(hashPath, newHash)
        Debug.WriteLine("Hash value updated: " + newHash)
    End Function

    Public Shared Async Function LoadExtraData() As Task
        Dim json = Await IO.File.ReadAllTextAsync(IO.Path.Combine(DIRECTORY_BASE, "typecolormap.json"))
        _typeColorMap = JsonConvert.DeserializeObject(Of Dictionary(Of String, Color))(json)

        json = Await IO.File.ReadAllTextAsync(IO.Path.Combine(DIRECTORY_BASE, "gamecolormap.json"))
        _gameColorMap = JsonConvert.DeserializeObject(Of Dictionary(Of String, Color))(json)
    End Function

    Public Shared Function ProduceHashOfFile(file_name As String) As String
        Dim hash = System.Security.Cryptography.MD5.Create()
        Dim hashValue() As Byte
        Dim fileStream As FileStream = File.OpenRead(file_name)
        fileStream.Position = 0
        hashValue = hash.ComputeHash(fileStream)
        Dim hash_hex = PrintByteArray(hashValue)
        fileStream.Close()
        Return hash_hex
    End Function

    Public Shared Function PrintByteArray(array() As Byte) As String
        Dim hex_value As String = ""
        Dim i As Integer
        For i = 0 To array.Length - 1
            hex_value += array(i).ToString("X2")
        Next i
        Return hex_value.ToLower
    End Function
End Class
