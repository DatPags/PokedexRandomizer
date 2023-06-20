Public Class PkmnInfoRetriever
    Private InfoEngine As IPkmnInfoFinder
    Private ImageEngine As IPkmnImageFinder
    Private InfoCache As IPkmnInfoCache
    Private ImageCache As IImageCache
    Private _ran As New Random()

    Public Sub New(infoEngine As IPkmnInfoFinder, imageEngine As IPkmnImageFinder, Optional infoCache As IPkmnInfoCache = Nothing,
                   Optional imageCache As IImageCache = Nothing)
        Me.InfoEngine = infoEngine
        Me.ImageEngine = imageEngine
        Me.InfoCache = infoCache
        Me.ImageCache = imageCache
    End Sub

    Public Async Function Get_Pkmn_Info(pkmnNumber As Integer, settings As Settings) As Task(Of Pkmn)
        Dim pkmnInfo As PkmnInfo?
        Dim cache As IPkmnInfoCache = New AppDataLocalCache()
        If InfoCache IsNot Nothing AndAlso settings.UseCache Then pkmnInfo = cache.GetPkmnInfoIfExists(pkmnNumber.ToString) Else pkmnInfo = Nothing

        If pkmnInfo Is Nothing Then
            pkmnInfo = Await InfoEngine.Get_Pkmn_Info(pkmnNumber)
            If InfoCache IsNot Nothing AndAlso settings.UseCache Then cache.StorePkmnInfoInCache(pkmnInfo.Value, pkmnNumber.ToString)
        End If

        Dim pkmn As New Pkmn With {.pkmn = pkmnInfo.Value}
        pkmn.images = Await ImageEngine.Get_All_Images_For_Pkmn(pkmn.pkmn, settings, ImageCache)
        Return pkmn
    End Function

    Public Function Total_Number_Of_Pokemon() As Integer
        Return InfoEngine.Total_Number_Of_Pokemon()
    End Function

    Public Function Validate_Search(search As String, ByRef pkmnNum As Integer, ByRef result As String) As Boolean
        pkmnNum = 0
        result = ""
        If Not Integer.TryParse(search, pkmnNum) Then
            ' Test this as a name
            If Not InfoEngine.Pkmn_Exists_By_Name(search) Then
                result = $"{search} is not a valid name."
                Return False
            End If
            pkmnNum = InfoEngine.Pkmn_Name_To_Number(search)
            Return True
        Else
            ' Test this as a number
            If Not InfoEngine.Pkmn_Exists_By_Num(pkmnNum) Then
                result = $"{search} is not a valid dex number."
                Return False
            End If
            Return True
        End If
    End Function

    Public Function ClearCache() As Boolean
        If InfoCache IsNot Nothing Then
            If Not InfoCache.ClearCache() Then Return False
        End If
        If ImageCache IsNot Nothing Then
            If Not ImageCache.ClearCache() Then Return False
        End If
        Return True
    End Function

    Public Function Random_Entry(pkmnInfo As Pkmn, settings As Settings) As List(Of Integer)
        Dim entryToUse = _ran.Next(maxValue:=pkmnInfo.pkmn.games.Count)
        Dim formIndex As Integer
        If pkmnInfo.pkmn.forms.Count > 1 Then
            If settings.PrioritizeForms Then
                ' Prioritize forms, select an entry that matches the chosen form
                formIndex = _ran.Next(maxValue:=pkmnInfo.pkmn.forms.Count)
                Dim entriesChoose = New List(Of Integer)
                For entryIdx = 0 To pkmnInfo.pkmn.games.Count - 1
                    If pkmnInfo.pkmn.forms(formIndex) = pkmnInfo.pkmn.name Then
                        If Not pkmnInfo.pkmn.games(entryIdx).Contains("(") And Not pkmnInfo.pkmn.games(entryIdx).EndsWith(")") Then
                            entriesChoose.Add(entryIdx)
                        End If
                    Else
                        If pkmnInfo.pkmn.games(entryIdx).Contains("(") And pkmnInfo.pkmn.games(entryIdx).EndsWith(")") Then
                            Dim gameName = pkmnInfo.pkmn.games(entryIdx)
                            Dim i = gameName.IndexOf("(")
                            Dim gameNameSubstr = gameName.Substring(i + 1, gameName.IndexOf(")", i + 1) - i - 1)
                            If gameNameSubstr = pkmnInfo.pkmn.forms(formIndex) Then
                                entriesChoose.Add(entryIdx)
                            End If
                        End If
                    End If
                Next
                If entriesChoose.Count > 0 Then
                    Dim entryIdx = _ran.Next(maxValue:=entriesChoose.Count)
                    entryToUse = entriesChoose(entryIdx)
                End If
            Else
                ' Prioritize entries, attempt to match chosen entry to a form
                If pkmnInfo.pkmn.games(entryToUse).Contains("(") And pkmnInfo.pkmn.games(entryToUse).EndsWith(")") Then
                    ' If game name has (), choose the form that matches the text in the ()
                    Dim gameName = pkmnInfo.pkmn.games(entryToUse)
                    Dim i = gameName.IndexOf("(")
                    Dim gameNameSubstr = gameName.Substring(i + 1, gameName.IndexOf(")", i + 1) - i - 1)
                    Dim foundForm = False
                    For fi = 0 To pkmnInfo.pkmn.forms.Count - 1
                        Dim formName = pkmnInfo.pkmn.forms(fi)
                        If formName = gameNameSubstr Then
                            formIndex = fi
                            foundForm = True
                            Exit For
                        End If
                    Next
                    If Not foundForm Then
                        ' If no form matches what's in the (), choose a form at random
                        formIndex = _ran.Next(maxValue:=pkmnInfo.pkmn.forms.Count)
                    End If
                Else
                    ' If the game doesn't have (), choose a form that isn't in the () of another game
                    Dim possibleForms = New List(Of Integer)
                    For fi = 0 To pkmnInfo.pkmn.forms.Count - 1
                        Dim formName = pkmnInfo.pkmn.forms(fi)
                        Dim formNamePossible = True
                        For Each gameName In pkmnInfo.pkmn.games
                            If gameName.Contains("(") And gameName.EndsWith(")") Then
                                Dim i = gameName.IndexOf("(")
                                Dim gameNameSubstr = gameName.Substring(i + 1, gameName.IndexOf(")", i + 1) - i - 1)
                                If formName = gameNameSubstr Then
                                    formNamePossible = False
                                    Exit For
                                End If
                            End If
                        Next
                        If formNamePossible Then
                            possibleForms.Add(fi)
                        End If
                    Next
                    If possibleForms.Count > 0 Then
                        formIndex = possibleForms(_ran.Next(maxValue:=possibleForms.Count))
                    Else
                        ' If every form name is in another game name's (), choose a form at random
                        formIndex = _ran.Next(maxValue:=pkmnInfo.pkmn.forms.Count)
                    End If
                End If
            End If
        Else
            ' Only 1 form, so choose any entry
            formIndex = 0
        End If
        Return New List(Of Integer) From {{formIndex}, {entryToUse}, {_ran.Next(maxValue:=pkmnInfo.pkmn.abilities(formIndex).Count)}}
    End Function

    Public Function Generate_Random_Moves(moveList As List(Of MoveInfo)) As List(Of MoveInfo)
        Dim randomMoves = New List(Of MoveInfo)
        If moveList.Count <= 4 Then
            Return moveList
        Else
            Dim randomNumbers = New List(Of Integer)
            Do While randomNumbers.Count < 4
                Dim rand = _ran.Next(maxValue:=moveList.Count)
                Dim inList = False
                For Each x In randomNumbers
                    If rand = x Then
                        inList = True
                        Exit For
                    End If
                Next
                If Not inList Then
                    randomNumbers.Add(rand)
                    randomMoves.Add(moveList(rand))
                End If
            Loop
            Return randomMoves
        End If
    End Function
End Class
