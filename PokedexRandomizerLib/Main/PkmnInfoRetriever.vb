Public Class PkmnInfoRetriever
    Private InfoEngine As IPkmnInfoFinder
    Private ImageEngine As IPkmnImageFinder
    Private _ran As New Random()

    Public Sub New(infoEngine As IPkmnInfoFinder, imageEngine As IPkmnImageFinder)
        Me.InfoEngine = infoEngine
        Me.ImageEngine = imageEngine
    End Sub

    Public Async Function GetPkmnAsync(pkmnNumber As Integer, settings As Settings) As Task(Of Pkmn)
        Dim pkmnInfo As PkmnInfo = Await InfoEngine.GetPkmnInfoAsync(pkmnNumber)
        Dim pkmn As New Pkmn With {.pkmn = pkmnInfo}
        pkmn.icons = Await ImageEngine.GetPkmnIconListAsync(pkmn.pkmn)
        pkmn.iconUris = New List(Of String)
        If TypeOf ImageEngine Is IPkmnImageFinderURI Then pkmn.iconUris = DirectCast(ImageEngine, IPkmnImageFinderURI).GetPkmnIconURIList(pkmn.pkmn)
        pkmn.images = Await ImageEngine.GetPkmnImageListAsync(pkmn.pkmn)
        pkmn.imageUris = New List(Of String)
        If TypeOf ImageEngine Is IPkmnImageFinderURI Then pkmn.imageUris = DirectCast(ImageEngine, IPkmnImageFinderURI).GetPkmnImageURIList(pkmn.pkmn)
        Return pkmn
    End Function

    Public Function GetTotalNumOfPkmn() As Integer
        Return InfoEngine.GetTotalNumOfPkmn()
    End Function

    Public Function ValidateSearch(search As String, ByRef pkmnNum As Integer, ByRef result As String) As Boolean
        pkmnNum = 0
        result = ""
        If Not Integer.TryParse(search, pkmnNum) Then
            ' Test this as a name
            If Not InfoEngine.DoesPkmnExist(search) Then
                result = $"{search} is not a valid name."
                Return False
            End If
            pkmnNum = InfoEngine.PkmnNameToNumber(search)
            Return True
        Else
            ' Test this as a number
            If Not InfoEngine.DoesPkmnExist(pkmnNum) Then
                result = $"{search} is not a valid dex number."
                Return False
            End If
            Return True
        End If
    End Function

    Public Function SelectRandomEntry(pkmn As Pkmn, settings As Settings) As List(Of Integer)
        Dim entryToUse = _ran.Next(maxValue:=pkmn.pkmn.games.Count)
        Dim formIndex As Integer
        If pkmn.pkmn.forms.Count > 1 Then
            If settings.PrioritizeForms Then
                ' Prioritize forms, select an entry that matches the chosen form
                formIndex = _ran.Next(maxValue:=pkmn.pkmn.forms.Count)
                Dim entriesChoose = New List(Of Integer)
                For entryIdx = 0 To pkmn.pkmn.games.Count - 1
                    If pkmn.pkmn.forms(formIndex) = pkmn.pkmn.name Then
                        If Not pkmn.pkmn.games(entryIdx).Contains("(") And Not pkmn.pkmn.games(entryIdx).EndsWith(")") Then
                            entriesChoose.Add(entryIdx)
                        End If
                    Else
                        If pkmn.pkmn.games(entryIdx).Contains("(") And pkmn.pkmn.games(entryIdx).EndsWith(")") Then
                            Dim gameName = pkmn.pkmn.games(entryIdx)
                            Dim i = gameName.IndexOf("(")
                            Dim gameNameSubstr = gameName.Substring(i + 1, gameName.IndexOf(")", i + 1) - i - 1)
                            If gameNameSubstr = pkmn.pkmn.forms(formIndex) Then
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
                If pkmn.pkmn.games(entryToUse).Contains("(") And pkmn.pkmn.games(entryToUse).EndsWith(")") Then
                    ' If game name has (), choose the form that matches the text in the ()
                    Dim gameName = pkmn.pkmn.games(entryToUse)
                    Dim i = gameName.IndexOf("(")
                    Dim gameNameSubstr = gameName.Substring(i + 1, gameName.IndexOf(")", i + 1) - i - 1)
                    Dim foundForm = False
                    For fi = 0 To pkmn.pkmn.forms.Count - 1
                        Dim formName = pkmn.pkmn.forms(fi)
                        If formName = gameNameSubstr Then
                            formIndex = fi
                            foundForm = True
                            Exit For
                        End If
                    Next
                    If Not foundForm Then
                        ' If no form matches what's in the (), choose a form at random
                        formIndex = _ran.Next(maxValue:=pkmn.pkmn.forms.Count)
                    End If
                Else
                    ' If the game doesn't have (), choose a form that isn't in the () of another game
                    Dim possibleForms = New List(Of Integer)
                    For fi = 0 To pkmn.pkmn.forms.Count - 1
                        Dim formName = pkmn.pkmn.forms(fi)
                        Dim formNamePossible = True
                        For Each gameName In pkmn.pkmn.games
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
                        formIndex = _ran.Next(maxValue:=pkmn.pkmn.forms.Count)
                    End If
                End If
            End If
        Else
            ' Only 1 form, so choose any entry
            formIndex = 0
        End If
        Return New List(Of Integer) From {{formIndex}, {entryToUse}, {_ran.Next(maxValue:=pkmn.pkmn.abilities(formIndex).Count)}}
    End Function

    Public Function PickRandomMoves(moveList As List(Of MoveInfo)) As List(Of MoveInfo)
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
