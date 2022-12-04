Class PkmnInfoRetriever
    Private Const URL_BASE = "https://pokemondb.net"
    Private Const URL_NATDEX = "/pokedex/national"

    Private Shared ImageEngine As IPkmnImageFinder = New PkmnImageFinderPokesprite

    Public Shared Sub Init_Pkmn_Images()
        ImageEngine.Init_Pkmn_Images()
    End Sub

    Public Async Function Url_List() As Task(Of List(Of UrlInfo))
        Dim html = Await Load_Html_Async(URL_BASE & URL_NATDEX)
        Dim data = New_UrlMapData()
        Parse_Html_Url_List(html, data)
        Dim urlInfoList = New List(Of UrlInfo)
        For urlIndex = 0 To data.count - 1
            urlInfoList.Add(New UrlInfo With {.number = urlIndex + 1, .name = data.names(urlIndex), .url = data.urls(urlIndex)})
        Next
        Return urlInfoList
    End Function

    Public Async Function Get_Pkmn_Info(pkmnNumber As Integer, url As String, settings As Settings) As Task(Of Pkmn)
        Dim pkmn As Pkmn, pkmnInfo As PkmnInfo?
        Dim cache As IPkmnInfoCache = New AppDataLocalCache()
        If settings.UseCache Then pkmnInfo = cache.GetPkmnInfoIfExists(url) Else pkmnInfo = Nothing

        If pkmnInfo Is Nothing Then
            Dim html = Await Load_Html_Async(URL_BASE & url)
            Dim data = New_GatherInfoData(pkmnNumber)
            Parse_Html_Pkmn_Info(html, data)
            data.heights = Decode_Html_String_List(data.heights)
            pkmn = Load_Info_Into_PkmnInfo(data)

            Dim moveData = New_GatherMovesData(pkmnNumber)
            Parse_Html_Moves(html, moveData)
            Remove_Duplicates(moveData.moves)
            Condense_Move_Lists(moveData)
            pkmn.pkmn.moveForms = moveData.forms
            pkmn.pkmn.moves = moveData.moves

            If settings.UseCache Then cache.StorePkmnInfoInCache(pkmn.pkmn, url)
        Else
            pkmn = New Pkmn With {.pkmn = pkmnInfo.Value}
        End If

        pkmn.images = Await ImageEngine.Get_All_Images_For_Pkmn(pkmn, settings)
        Return pkmn
    End Function

#Region "Html Utilities"
    Private Function Decode_Html_String_List(strList As List(Of String)) As List(Of String)
        Dim newList = New List(Of String)
        For Each item In strList
            newList.Add(Decode_Html_String(item))
        Next
        Return newList
    End Function

    Private Function Decode_Html_String(str As String) As String
        Dim writer = New System.IO.StringWriter
        System.Web.HttpUtility.HtmlDecode(str, writer)
        Return writer.ToString
    End Function

    Private Async Function Load_Html_Async(url As String) As Task(Of HtmlAgilityPack.HtmlDocument)
        Dim t As Task(Of HtmlAgilityPack.HtmlDocument)
        t = Task.Run(Function() As HtmlAgilityPack.HtmlDocument
                         Dim client As New System.Net.WebClient
                         Dim html = New HtmlAgilityPack.HtmlDocument
                         Dim bytes = client.DownloadData(url)
                         Dim utf8 = New System.Text.UTF8Encoding
                         html.LoadHtml(utf8.GetString(bytes))
                         Return html
                     End Function)
        Return Await t
    End Function
#End Region

#Region "Parse Url List"
    Private Sub Parse_Html_Url_List(html As HtmlAgilityPack.HtmlDocument, ByRef data As UrlMapData)
        For Each node As HtmlAgilityPack.HtmlNode In html.DocumentNode.ChildNodes
            Check_Url_List_Node(node, data, 0)
        Next
    End Sub

    Private Sub Check_Url_List_Node(node As HtmlAgilityPack.HtmlNode, ByRef data As UrlMapData, depth As Integer)
        For Each childNode In node.ChildNodes
            Dim name = childNode.Name.Trim

            If Not name.StartsWith("#") Then
                ' If it has a name, then it's an HTML tag
                For Each attribute In childNode.Attributes
                    Dim attrName = attribute.Name.Trim
                    Dim attrValue = attribute.Value.Trim

                    If data.inInfocard And depth <= data.infocardDepth Then
                        ' Turn off the infocard flag if we're no longer in one
                        data.inInfocard = False
                    End If

                    If name = "div" And attrName = "class" Then
                        ' Look for <div class="infocard">
                        If attrValue = "infocard" Then
                            data.count += 1
                            data.inInfocard = True
                            data.infocardDepth = depth
                            data.nextName = False
                        End If
                    ElseIf data.inInfocard And name = "a" And attrName = "class" Then
                        ' If we're in the infocard, look for <a class="ent-name">, which means next href is a url
                        If attrValue = "ent-name" Then
                            data.nextHref = True
                        End If
                    ElseIf data.inInfocard And data.nextHref And name = "a" And attrName = "href" Then
                        ' If we're in the infocard and the href flag is on, get the data from the next href
                        data.urls.Add(attrValue)
                        data.nextHref = False
                        data.nextName = True
                    End If
                Next
            Else
                ' No name, so it's some sort of data
                Dim text = childNode.InnerText.Trim

                If data.nextName Then
                    data.names.Add(text)
                    data.nextName = False
                End If
            End If

            If childNode.HasChildNodes Then
                Check_Url_List_Node(childNode, data, depth + 1)
            End If
        Next
    End Sub
#End Region

#Region "Parse Pkmn Info"
    Private Sub Parse_Html_Pkmn_Info(html As HtmlAgilityPack.HtmlDocument, ByRef data As GatherInfoData)
        For Each node As HtmlAgilityPack.HtmlNode In html.DocumentNode.ChildNodes
            Check_Pkmn_Info_Node(node, data, 0)
        Next
    End Sub

    Private Sub Check_Pkmn_Info_Node(node As HtmlAgilityPack.HtmlNode, ByRef data As GatherInfoData, depth As Integer)
        For Each childNode In node.ChildNodes
            Dim name = childNode.Name.Trim

            If Not name.StartsWith("#") Then
                ' If it has a name, then it's an HTML tag
                For Each attribute In childNode.Attributes
                    Dim attrName = attribute.Name.Trim
                    Dim attrValue = attribute.Value.Trim

                    If data.inEntryTable And data.inGameList And depth <= data.gameListDepth Then
                        ' Break out of the games list for this row, finalize this game list string
                        data.inGameList = False
                        data.gamesTempStr = data.gamesTempStr.Substring(0, data.gamesTempStr.Length - 2)
                        If Not data.currentEntryCategory = "" And Not data.currentEntryCategory = data.name Then
                            data.gamesTempStr &= " (" & data.currentEntryCategory & ")"
                        End If
                        data.games.Add(data.gamesTempStr)
                        data.gamesTempStr = ""
                        data.nextEntry = True
                    ElseIf name = "div" And attrName = "class" Then
                        If attrValue = "sv-tabs-tab-list" And data.firstTabList Then
                            ' tabs-tab-list indicates entering the tab list
                            data.inTabList = True
                            data.tabListDepth = depth
                        ElseIf attrValue = "sv-tabs-panel-list" And Not data.inTabTable And data.firstTabTable Then
                            ' tabs-panel-list indicates entering the tab table
                            data.inTabTable = True
                            data.tabTableDepth = depth
                        End If
                    ElseIf data.inTabTable And depth <= data.tabTableDepth Then
                        ' Turn off inTabTable if it's left the tab tables
                        data.inTabTable = False
                        data.firstTabTable = False
                    ElseIf data.dataMode = 5 And name = "a" And attrName = "href" And
                            attrValue.StartsWith("/ability/") Then
                        data.nextAbility = True
                    End If

                    If data.inTabList And depth <= data.tabListDepth And data.formDataFound Then
                        ' Turn off inTabList after leaving the tab list, happens when entering the tab panel
                        data.inTabList = False
                        data.firstTabList = False
                    End If
                Next

                If name = "h1" Then
                    ' The only <h1> tag on the page is for the Pokemon's name
                    data.nextName = True
                ElseIf name = "h2" Then
                    ' Check if the <h2> is for entries, if it is then the next <h2> will be the end of entries
                    If data.inEntryTable Then
                        data.inEntryTable = False
                    Else
                        data.checkNextEntryTable = True
                    End If
                ElseIf data.inEntryTable And name = "h3" Then
                    ' When in the entry table, <h3> will be new categories
                    data.nextNewCategory = True
                ElseIf data.inEntryTable And name = "th" Then
                    ' When in entry table, <th> will be the start of the game list for a given row
                    data.inGameList = True
                    data.gameListDepth = depth
                ElseIf data.inTabTable And name = "th" Then
                    ' When in tabs table, <th> appears before text indicating new piece of data (species, type)
                    data.nextDataMode = True
                    data.dataModeDepth = depth
                ElseIf data.dataMode <> 0 And depth < data.dataModeDepth Then
                    ' Deactivate the data mode if it's out of scope
                    If data.dataMode = 2 Then
                        ' Finalize types if data mode was for types
                        If data.tempTypes.Count = 2 Then
                            data.types.Add(New List(Of String) From {data.tempTypes(0), data.tempTypes(1)})
                        Else
                            data.types.Add(New List(Of String) From {data.tempTypes(0), ""})
                        End If
                        data.tempTypes = New List(Of String)
                    End If
                    data.dataMode = 0
                End If
            Else
                ' Name starts with #, so it's some sort of data
                Dim text = childNode.InnerText.Trim

                If data.nextName Then
                    ' If the nextName flag is on, get this text and save it as the name
                    data.name = text
                    data.nextName = False
                ElseIf data.checkNextEntryTable Then
                    ' If the checkNextEntryTable flag is on, see if this is the entry table
                    If text.StartsWith("Pok") And text.EndsWith("dex entries") Then
                        data.inEntryTable = True
                    End If
                    data.checkNextEntryTable = False
                ElseIf data.nextNewCategory Then
                    ' If the nextNewCategory flag is on, set the current text as the entry category text
                    data.currentEntryCategory = text
                    data.nextNewCategory = False
                ElseIf data.inGameList Then
                    ' If we're in a game list, append the name of this game to the temp string
                    If Not text = "" Then
                        data.gamesTempStr &= text & ", "
                    End If
                ElseIf data.inEntryTable And data.nextEntry Then
                    ' If nextEntry is on, this data is a dex entry
                    data.entries.Add(text)
                    data.nextEntry = False
                ElseIf data.inTabList Then
                    ' All data in the tab list will be form names, so add them to the list
                    If Not text = "" Then
                        data.forms.Add(text)
                    End If
                    data.formDataFound = True
                ElseIf data.inTabTable And data.nextDataMode Then
                    ' Activate a data mode if one is detected
                    If text = "Species" Then
                        data.dataMode = 1
                    ElseIf text = "Type" Then
                        data.dataMode = 2
                    ElseIf text = "Height" Then
                        data.dataMode = 3
                    ElseIf text = "Weight" Then
                        data.dataMode = 4
                    ElseIf text = "Abilities" Then
                        data.dataMode = 5
                        data.abilities.Add(New List(Of String))
                    End If
                    data.nextDataMode = False
                ElseIf data.inTabTable And data.dataMode <> 0 Then
                    ' Process a data mode
                    If data.dataMode = 1 And Not text = "" Then
                        data.classifications.Add(text)
                    ElseIf data.dataMode = 2 And Not text = "" Then
                        data.tempTypes.Add(text)
                    ElseIf data.dataMode = 3 And Not text = "" Then
                        data.heights.Add(text.Replace("&nbsp;", "").Trim)
                    ElseIf data.dataMode = 4 And Not text = "" Then
                        data.weights.Add(text.Replace("&nbsp;", "").Trim)
                    ElseIf data.dataMode = 5 And Not text = "" And data.nextAbility Then
                        data.abilities(data.abilities.Count - 1).Add(text)
                        data.nextAbility = False
                    End If
                End If
            End If

            If childNode.HasChildNodes Then
                Check_Pkmn_Info_Node(childNode, data, depth + 1)
            End If
        Next
    End Sub
#End Region

#Region "Parse Pkmn Moves"
    Private Sub Parse_Html_Moves(html As HtmlAgilityPack.HtmlDocument, ByRef data As GatherMovesData)
        For Each node As HtmlAgilityPack.HtmlNode In html.DocumentNode.ChildNodes
            Check_Pkmn_Moves_Node(node, data, 0)
        Next
    End Sub

    Private Sub Check_Pkmn_Moves_Node(node As HtmlAgilityPack.HtmlNode, ByRef data As GatherMovesData, depth As Integer)
        For Each childNode In node.ChildNodes
            Dim name = childNode.Name.Trim

            If Not name.StartsWith("#") Then
                ' If it has a name, then it's an HTML tag
                For Each attribute In childNode.Attributes
                    Dim attrName = attribute.Name.Trim
                    Dim attrValue = attribute.Value.Trim

                    If name = "div" And attrName = "class" Then
                        If attrValue = "sv-tabs-tab-list" And data.firstTabList Then
                            ' tabs-tab-list indicates entering the tab list for form names
                            data.inTabList = True
                            data.tabListDepth = depth
                        ElseIf attrValue = "sv-tabs-tab-list" And data.inMovesTable And data.inTabsPanelActive Then
                            ' Entering the tab list here is for form lists above each small move table
                            data.formMovesExist = True
                            data.inMoveTabList = True
                            data.moveTabListDepth = depth
                            data.skipMoveTabCheck = True
                        ElseIf attrValue = "resp-scroll" And data.inMovesTable And data.inTabsPanelActive Then
                            ' resp-scroll indicates entering a move table
                            data.inSmallMoveTable = True
                            data.smallMoveTableDepth = depth
                            data.skipLeaveCheck = True
                        ElseIf attrValue = "sv-tabs-panel active" And data.inMovesTable And
                                Not data.inTabsPanelActive And data.firstTabsPanelActive Then
                            ' tabs-panel active for the first time is moves for most recent game
                            data.inTabsPanelActive = True
                            data.tabsPanelActiveDepth = depth
                            Exit For
                        ElseIf attrValue = "tabset-moves-game-form tabs-wrapper" And data.inMovesTable And
                                data.inTabsPanelActive Then
                            ' Disable adding to every move list if a tab wrapper is found
                            data.addMoveToAll = False
                        End If
                    ElseIf name = "a" And attrName = "href" Then
                        If data.inMovesTable And data.inTabsPanelActive And data.inSmallMoveTable And
                                attrValue.StartsWith("/move/") Then
                            ' ahref tag starting with /move/ indicates next data is a move name
                            data.nextMove = True
                        End If
                    ElseIf name = "span" And attrName = "title" Then
                        If data.moveCountdown = 3 Then
                            ' <img title="-"> will be a move category during a move countdown
                            data.tempMove.category = attrValue
                            data.moveCountdown -= 1
                        End If
                    End If

                    If data.inMovesTable And data.inTabsPanelActive And data.firstTabsPanelActive And
                            depth <= data.tabsPanelActiveDepth Then
                        ' Leaving the tabs panel active table, separate because it happens in a div class
                        data.inTabsPanelActive = False
                        data.firstTabsPanelActive = False
                    ElseIf data.inTabList And depth <= data.tabListDepth And data.formDataFound Then
                        ' Turn off inTabList after leaving the tab list
                        data.inTabList = False
                        data.firstTabList = False
                    ElseIf data.inMoveTabList And depth <= data.moveTabListDepth And Not data.skipMoveTabCheck Then
                        ' Turn off the tab list after leaving a move tab list
                        data.inMoveTabList = False
                    End If

                    If data.inMovesTable And data.inTabsPanelActive And data.inSmallMoveTable And
                            depth <= data.smallMoveTableDepth And Not data.skipLeaveCheck Then
                        ' Leave small move table and increment current form
                        data.inSmallMoveTable = False
                        data.currentForm += 1
                    End If
                Next

                ' No attributes
                If data.inMovesTable And data.inTabsPanelActive And data.inSmallMoveTable And
                        depth <= data.smallMoveTableDepth And Not data.skipLeaveCheck Then
                    ' Leave small move table and increment current form
                    data.inSmallMoveTable = False
                    data.currentForm += 1
                End If

                If name = "h1" Then
                    ' The only <h1> tag is the pokemon's name
                    data.nextName = True
                ElseIf name = "h2" Then
                    ' Check if the <h2> is for moves, if it is then the next <h2> will be the end of moves
                    If data.inMovesTable Then
                        data.inMovesTable = False
                    Else
                        data.checkNextMovesTable = True
                    End If
                ElseIf name = "h3" Then
                    ' <h3> tags in the move list will reset the current form
                    If data.inMovesTable And data.inTabsPanelActive Then
                        data.currentForm = 0
                        data.addMoveToAll = True
                    End If
                End If
                data.skipLeaveCheck = False
                data.skipMoveTabCheck = False
            Else
                ' Name starts with #, so it's some sort of data
                Dim text = childNode.InnerText.Trim

                If data.inTabList Then
                    ' All data in the tab list will be form names, so add them to the list
                    If Not text = "" Then
                        data.forms.Add(text)
                        data.formDataFound = True
                        data.moves.Add(New List(Of MoveInfo))
                    End If
                ElseIf data.nextName Then
                    ' If the nextName flag is on, get this text and save it as the name
                    data.name = text
                    data.nextName = False
                ElseIf data.checkNextMovesTable Then
                    ' If the checkNextMovesTable flag is on, see if this is the moves table
                    If text.StartsWith("Moves learned by ") Then
                        data.inMovesTable = True
                    End If
                    data.checkNextMovesTable = False
                ElseIf data.inMovesTable And data.inTabsPanelActive And data.inSmallMoveTable And
                        data.nextMove Then
                    ' If the nextMove flag is on, this text is a move name
                    data.nextMove = False
                    data.moveCountdown = 4
                    data.tempMove = New MoveInfo With {.name = text}
                ElseIf data.inMoveTabList Then
                    ' Add this form to the valid forms list if it's not already in the list
                    If Not text = "" Then
                        Dim newValidForm = True
                        For Each validForm In data.validForms
                            If validForm = text Then
                                newValidForm = False
                                Exit For
                            End If
                        Next
                        If newValidForm Then
                            data.validForms.Add(text)
                        End If
                    End If
                ElseIf data.inMovesTable And data.inTabsPanelActive And data.inSmallMoveTable And
                        data.moveCountdown > 0 And Not text = "" Then
                    ' During a move countdown, each subsequent text will be part of the move data
                    Select Case data.moveCountdown
                        Case 4
                            data.tempMove.type = text
                        Case 3
                            data.tempMove.power = text
                        Case 2
                            data.tempMove.accuracy = Decode_Html_String(text)
                            If data.addMoveToAll Then
                                For formIndex = 0 To data.forms.Count - 1
                                    data.moves(formIndex).Add(data.tempMove)
                                Next
                            Else
                                data.moves(data.currentForm).Add(data.tempMove)
                            End If
                    End Select
                    data.moveCountdown -= 1
                End If
            End If

            If childNode.HasChildNodes Then
                Check_Pkmn_Moves_Node(childNode, data, depth + 1)
            End If
        Next
    End Sub
#End Region

#Region "Moves Utilities"
    Private Sub Remove_Duplicates(ByRef moves As List(Of List(Of MoveInfo)))
        For formIndex = 0 To moves.Count - 1
            Dim newList = New List(Of MoveInfo)
            For Each move In moves(formIndex)
                Dim dup = False
                For i = 0 To newList.Count - 1
                    If newList(i).name = move.name Then
                        dup = True
                        Exit For
                    End If
                Next
                If dup Then
                    Continue For
                End If
                newList.Add(move)
            Next
            moves(formIndex) = newList
        Next
    End Sub

    Private Sub Condense_Move_Lists(ByRef data As GatherMovesData)
        If Not data.formMovesExist Then
            data.forms = New List(Of String) From {{data.forms(0)}}
            data.moves = New List(Of List(Of MoveInfo)) From {{data.moves(0)}}
        Else
            Dim newFormList = New List(Of String)
            Dim newMoveList = New List(Of List(Of MoveInfo))
            Dim j As Integer = 0
            For i = 0 To data.forms.Count - 1
                Dim validForm = False
                For Each formName In data.validForms
                    If data.forms(i) = formName Then
                        validForm = True
                        Exit For
                    End If
                Next
                If validForm Then
                    newFormList.Add(data.forms(i))
                    newMoveList.Add(data.moves(j))
                    j += 1
                End If
            Next
            data.forms = newFormList
            data.moves = newMoveList
        End If
    End Sub
#End Region

End Class
