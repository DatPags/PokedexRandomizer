Imports Newtonsoft.Json
Imports SixLabors.ImageSharp
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Public Class PkmnImageFinderLocal
    Implements IPkmnImageFinder, IPkmnImageFinderURI

    Private Shared DATA_FILE As String = IO.Path.Combine(Util.DIRECTORY_BASE, "imageindex.json")
    Private Shared IMAGES_DIRECTORY_BASE As String = IO.Path.Combine(Util.DIRECTORY_BASE, "images")
    Private Shared IMAGES_DIRECTORY_NAME As String = "images"
    Private Shared ICONS_DIRECTORY_NAME As String = "icons"
    Private Const URL_IMG_UNKNOWN = "https://raw.githubusercontent.com/msikma/pokesprite/master/pokemon-gen8/unknown.png"

    Private index As Dictionary(Of Integer, Dictionary(Of String, List(Of String)))

    Public Shared Async Function CreateSelfAsync() As Task(Of PkmnImageFinderLocal)
        Dim obj = New PkmnImageFinderLocal
        If Not IO.File.Exists(DATA_FILE) Then
            Throw New FileNotFoundException("Image data does not exist")
        End If
        Dim bytes = Await System.IO.File.ReadAllBytesAsync(DATA_FILE)
        Dim json As String = Encoding.UTF8.GetString(bytes)
        obj.index = JsonConvert.DeserializeObject(Of Dictionary(Of Integer, Dictionary(Of String, List(Of String))))(json)
        Return obj
    End Function

#Region "Data Archive"
    Friend Shared Async Function CreateJSONFileAndImageArchiveAsync(iconsDumpPath As String, imagesDumpPath As String) As Task
        Debug.WriteLine("Starting image JSON file creation")
        Dim sw As New Stopwatch()
        sw.Start()

        ' initialize the folder structure to build the image archive
        Dim workingDir As String = IO.Path.Combine(Util.DIRECTORY_BASE, "working")
        Dim iconsWorkingDir = IO.Path.Combine(workingDir, "images", ICONS_DIRECTORY_NAME)
        Util.CreateDirectory(iconsWorkingDir)
        Dim imagesWorkingDir = IO.Path.Combine(workingDir, "images", IMAGES_DIRECTORY_NAME)
        Util.CreateDirectory(imagesWorkingDir)
        Dim dict As New Dictionary(Of Integer, Dictionary(Of String, List(Of String)))
        Dim infoFinder = Await PkmnInfoFinderLocal.CreateSelfAsync()

        ' download the default unknown image
        Dim img = Await UtilImage.GetImageFromUrlAsync(URL_IMG_UNKNOWN, Nothing, Nothing)
        Await img.SaveAsPngAsync(IO.Path.Combine(iconsWorkingDir, "unknown.png"))
        Await img.SaveAsPngAsync(IO.Path.Combine(imagesWorkingDir, "unknown.png"))
        dict.Add(0, New Dictionary(Of String, List(Of String)))
        dict(0).Add(ICONS_DIRECTORY_NAME, New List(Of String) From {"unknown.png"})
        dict(0).Add(IMAGES_DIRECTORY_NAME, New List(Of String) From {"unknown.png"})

        ' icons
        Dim tempDict = Await CreateImageArchiveFromHOMEDumpAsync(iconsDumpPath, iconsWorkingDir, infoFinder)
        UpdateMainArchiveIndex(tempDict, dict, ICONS_DIRECTORY_NAME)

        ' images
        tempDict = Await CreateImageArchiveFromHOMEDumpAsync(imagesDumpPath, imagesWorkingDir, infoFinder)
        UpdateMainArchiveIndex(tempDict, dict, IMAGES_DIRECTORY_NAME)

        Debug.WriteLine("Image archive creation completed")
        Dim json = JsonConvert.SerializeObject(dict)
        Await IO.File.WriteAllTextAsync(IO.Path.Combine(Util.DIRECTORY_BASE, "working", "imageindex.json"), json)
        sw.Stop()
        Debug.WriteLine("Completed image JSON file creation: " + sw.ElapsedMilliseconds.ToString + "ms, " + Encoding.UTF8.GetByteCount(json).ToString + " bytes")
    End Function

    Private Shared Async Function CreateImageArchiveFromHOMEDumpAsync(dumpPath As String, workingDir As String, infoFinder As IPkmnInfoFinder) As Task(Of Dictionary(Of Integer, List(Of String)))
        Dim files = Directory.EnumerateFiles(dumpPath).ToList()
        Dim dict As New Dictionary(Of Integer, List(Of String))
        For pkmnNumber As Integer = 1 To infoFinder.GetTotalNumOfPkmn()
            Dim number As Integer = pkmnNumber
            Dim pkmn = Await infoFinder.GetPkmnInfoAsync(pkmnNumber)
            Dim newFilenames As New List(Of String), formIdx As Integer = 0
            For Each formName In pkmn.forms
                If pkmn.number = 774 And formIdx = 1 Then
                    '--manual fix for Minior core form
                    formIdx = 7
                ElseIf pkmn.number = 718 And formIdx = 2 Then
                    '--manual fix for Zygarde complete form
                    formIdx = 4
                ElseIf pkmn.number = 658 And formIdx = 1 Then
                    '--manual fix for Ash Greninja
                    formIdx = 2
                ElseIf pkmn.number = 744 And formIdx = 1 Then
                    '--manual fix for own tempo Rockruff
                    formIdx = 0
                ElseIf pkmn.number = 133 And formIdx = 1 Then
                    '--manual fix for partner Eevee
                    formIdx = 0
                ElseIf pkmn.number = 25 And formIdx = 1 Then
                    '--manual fix for partner Pikachu
                    formIdx = 0
                End If
                Dim matches = files.Where(Function(s) Regex.IsMatch(s, $"poke_(icon|capture)_{number:D4}_{formIdx:D3}_[a-z]*?_n_[0-9]*?_f_n.png")).ToList()
                If matches.Count <= 0 Then
                    Debug.WriteLine("No matches found for " + pkmn.name + ", form: " + formName)
                    Continue For
                End If
                Dim matchPath = matches(0)
                Dim filename = Path.GetFileName(matchPath)
                Dim newPath = IO.Path.Combine(workingDir, filename)
                If Not IO.File.Exists(newPath) Then IO.File.Copy(matchPath, newPath)
                newFilenames.Add(filename)
                formIdx += 1
            Next
            dict(pkmnNumber) = newFilenames
            If pkmn.forms.Count > newFilenames.Count And newFilenames.Count > 0 Then Debug.WriteLine("Missing full image list for " + pkmn.name)
        Next
        Return dict
    End Function

    Private Shared Sub UpdateMainArchiveIndex(tempDict As Dictionary(Of Integer, List(Of String)), ByRef dict As Dictionary(Of Integer, Dictionary(Of String, List(Of String))), dirName As String)
        For Each pkmnNumber In tempDict.Keys
            If Not dict.ContainsKey(pkmnNumber) Then dict.Add(pkmnNumber, New Dictionary(Of String, List(Of String)))
            dict(pkmnNumber).Add(dirName, tempDict(pkmnNumber))
        Next
    End Sub
#End Region

    Public Async Function GetPkmnIconListAsync(pkmnInfo As PkmnInfo, settings As Settings, cache As IImageCache) As Task(Of List(Of SixLabors.ImageSharp.Image)) Implements IPkmnImageFinder.GetPkmnIconListAsync
        Return Await GetPkmnImageListInternalAsync(pkmnInfo, settings, cache, ICONS_DIRECTORY_NAME)
    End Function

    Public Async Function GetPkmnImageListAsync(pkmnInfo As PkmnInfo, settings As Settings, cache As IImageCache) As Task(Of List(Of SixLabors.ImageSharp.Image)) Implements IPkmnImageFinder.GetPkmnImageListAsync
        Return Await GetPkmnImageListInternalAsync(pkmnInfo, settings, cache, IMAGES_DIRECTORY_NAME)
    End Function

    Public Function GetPkmnIconURIList(pkmnInfo As PkmnInfo) As List(Of String) Implements IPkmnImageFinderURI.GetPkmnIconURIList
        Return GetPkmnImageUrisInternal(pkmnInfo, ICONS_DIRECTORY_NAME)
    End Function

    Public Function GetPkmnImageURIList(pkmnInfo As PkmnInfo) As List(Of String) Implements IPkmnImageFinderURI.GetPkmnImageURIList
        Return GetPkmnImageUrisInternal(pkmnInfo, IMAGES_DIRECTORY_NAME)
    End Function

    Private Function GetPkmnImageUrisInternal(pkmnInfo As PkmnInfo, directoryName As String) As List(Of String)
        Dim paths As New List(Of String)
        If index.ContainsKey(pkmnInfo.number) Then
            For idx As Integer = 0 To pkmnInfo.forms.Count - 1
                If idx < index(pkmnInfo.number)(directoryName).Count Then
                    paths.Add(index(pkmnInfo.number)(directoryName)(idx))
                Else
                    paths.Add(index(0)(directoryName)(0))
                End If
            Next
        Else
            For Each form In pkmnInfo.forms
                paths.Add(index(0)(directoryName)(0))
            Next
        End If
        For idx As Integer = 0 To paths.Count - 1
            paths(idx) = IO.Path.Combine(IMAGES_DIRECTORY_BASE, directoryName, paths(idx))
        Next
        Return paths
    End Function

    Private Async Function GetPkmnImageListInternalAsync(pkmnInfo As PkmnInfo, settings As Settings, cache As IImageCache, directoryName As String) As Task(Of List(Of SixLabors.ImageSharp.Image))
        Dim paths = GetPkmnImageUrisInternal(pkmnInfo, directoryName)
        Dim images As New List(Of Image)
        For Each path In paths
            images.Add(Await Image.LoadAsync(path))
        Next
        Return images
    End Function
End Class
