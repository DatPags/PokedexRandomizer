Class MainWindow
    Private Const MAX_COLS = 6
    Private Const MOVES_IDX = MAX_COLS
    Private Const MANUAL_IDX = MAX_COLS + 1
    Private Const TOTAL_INFO = MAX_COLS + 1

    Private _ran As Random = New Random
    Private _maxNum As Integer
    Private _urlList As List(Of UrlInfo)
    Private _urlNameMap As Dictionary(Of String, UrlInfo)
    Private _pkmnInfoRetriever As PkmnInfoRetriever

    Private _displays(TOTAL_INFO) As PkmnDisplay
    Private _numPkmnLabel As Label
    Private _skips As SkipsDisplay
    Private _currentTab As Integer

    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        ' Create elements that stay at the top of each tab
        _numPkmnLabel = New Label
        _numPkmnLabel.VerticalContentAlignment = VerticalAlignment.Center
        _numPkmnLabel.FontSize = 14
        RandomizeTop.Children.Add(_numPkmnLabel)
        _skips = New SkipsDisplay
        Grid.SetColumn(_skips, 2)
        RandomizeTop.Children.Add(_skips)
        _currentTab = 0
        TabsBase.AddHandler(TabControl.SelectionChangedEvent, New RoutedEventHandler(AddressOf TabsBase_SelectionChanged))

        ' Add all pokemon displays to the UI
        NumberCombobox.ItemsSource = Enumerable.Range(1, MAX_COLS)
        Dim randomizeGrid = New Grid
        randomizeGrid.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFE5E5E5"))
        For idx = 0 To MAX_COLS - 1
            randomizeGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
            If idx Mod 2 = 1 Then
                Dim rect = New Rectangle
                rect.Fill = New SolidColorBrush(ColorConverter.ConvertFromString("#FFD9D9D9"))
                Grid.SetColumn(rect, idx)
                randomizeGrid.Children.Add(rect)
            End If
            Dim display = New PkmnDisplay
            Grid.SetColumn(display, idx)
            randomizeGrid.Children.Add(display)
            _displays(idx) = display
        Next
        RandomizeBase.Children.Add(randomizeGrid)
        Dim movesDisplay = New PkmnDisplay
        _displays(MOVES_IDX) = movesDisplay
        MovesBase.Children.Add(movesDisplay)
        Dim manualDisplay = New PkmnDisplay
        _displays(MANUAL_IDX) = manualDisplay
        ManualBase.Children.Add(manualDisplay)

        ' Load url list, category images, and pokemon image json
        _numPkmnLabel.Content = "Initializing..."
        _pkmnInfoRetriever = New PkmnInfoRetriever
        _urlList = Await _pkmnInfoRetriever.Url_List()
        _maxNum = _urlList.Count
        Build_Name_Map()
        MoveDisplay.Init_Cat_Images()
        PkmnInfoRetriever.Init_Pkmn_Images()
        _numPkmnLabel.Content = "Total number of Pokemon: " & _maxNum.ToString

        RandomizeButton.IsEnabled = True
        MovesButton.IsEnabled = True
        ManualButton.IsEnabled = True
    End Sub

#Region "UI Events"
    Private Async Sub RandomizeButton_Click(sender As Object, e As RoutedEventArgs) Handles RandomizeButton.Click
        RandomizeButton.IsEnabled = False
        Reset_Pkmn_Columns(MAX_COLS - 1)

        Dim randomNumbers As New List(Of Integer)
        Do
            Dim number = _ran.Next(maxValue:=_maxNum)
            If Not randomNumbers.Contains(number) Then
                randomNumbers.Add(number)
            End If
        Loop Until randomNumbers.Count >= MAX_COLS

        Dim tasks As New List(Of Task(Of Pkmn))
        For index = 0 To NumberCombobox.SelectedIndex
            tasks.Add(_pkmnInfoRetriever.Get_Pkmn_Info(randomNumbers(index) + 1, _urlList(randomNumbers(index)).url))
            _displays(index).Set_Loading()
        Next

        Dim listIndex = 0
        For Each pkmnInfo In Await Task.WhenAll(tasks)
            Dim form_game_index = Random_Entry(pkmnInfo)
            _displays(listIndex).Populate_Display(pkmnInfo, form_game_index(0), form_game_index(1), form_game_index(2))
            listIndex += 1
        Next

        RandomizeButton.IsEnabled = True
    End Sub

    Private Async Sub ManualButton_Click(sender As Object, e As RoutedEventArgs) Handles ManualButton.Click
        ManualButton.IsEnabled = False
        Reset_Pkmn_Columns(MANUAL_IDX, MANUAL_IDX)
        EntryListBox.Items.Clear()
        FormListBox.Items.Clear()

        Dim inputResult = Validate_For_Url(ManualTextBox.Text.Trim)
        If Not inputResult.Item1 Then
            _displays(MANUAL_IDX).Set_Text(inputResult.Item3)
            ManualButton.IsEnabled = True
            Exit Sub
        End If

        _displays(MANUAL_IDX).Set_Loading()
        Dim pkmnInfo = Await _pkmnInfoRetriever.Get_Pkmn_Info(inputResult.Item2, inputResult.Item3)
        _displays(MANUAL_IDX).Populate_Display(pkmnInfo, 0, 0, 0)
        Fill_Entry_List(pkmnInfo)
        Fill_Form_List(pkmnInfo)

        ManualButton.IsEnabled = True
    End Sub

    Private Async Sub MovesButton_Click(sender As Object, e As RoutedEventArgs) Handles MovesButton.Click
        MovesButton.IsEnabled = False
        Reset_Pkmn_Columns(MOVES_IDX, MOVES_IDX)
        MoveListBox.Items.Clear()

        Dim inputResult = Validate_For_Url(MovesTextBox.Text.Trim)
        If Not inputResult.Item1 Then
            _displays(MOVES_IDX).Set_Text(inputResult.Item3)
            MovesButton.IsEnabled = True
            Exit Sub
        End If

        _displays(MOVES_IDX).Set_Loading()
        Dim pkmnInfo = Await _pkmnInfoRetriever.Get_Pkmn_Info(inputResult.Item2, inputResult.Item3)
        _displays(MOVES_IDX).Populate_Display(pkmnInfo, 0, 0, 0)
        Fill_Move_List(pkmnInfo)

        MovesButton.IsEnabled = True
    End Sub

    Private Sub ReRandomizeMoves(sender As Object, e As RoutedEventArgs) Handles ReRandomizeMovesButton.Click
        Dim pkmnInfo = _displays(MOVES_IDX).PkmnInfo
        If Not pkmnInfo.pkmn.number = 0 Then
            For formIndex = 0 To pkmnInfo.pkmn.moveForms.Count - 1
                Dim formMoves As FormMoveDisplay = MoveListBox.Items.GetItemAt(formIndex)
                Dim abilityList = pkmnInfo.pkmn.abilities(pkmnInfo.pkmn.forms.IndexOf(pkmnInfo.pkmn.moveForms(formIndex)))
                formMoves.PkmnAbility = abilityList(_ran.Next(abilityList.Count))
                formMoves.PkmnMoveList = Generate_Random_Moves(pkmnInfo.pkmn.moves(formIndex))
            Next
        End If
    End Sub

    Private Sub MovesTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles MovesTextBox.KeyDown
        If MovesButton.IsEnabled AndAlso e.Key = Key.Enter Then
            MovesButton_Click(Me, New RoutedEventArgs)
        End If
    End Sub

    Private Sub ManualTextBox_KeyDown(sender As Object, e As KeyEventArgs) Handles ManualTextBox.KeyDown
        If ManualButton.IsEnabled AndAlso e.Key = Key.Enter Then
            ManualButton_Click(Me, New RoutedEventArgs)
        End If
    End Sub

    Private Sub TabsBase_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        ' Action on the tab that was just left
        Select Case _currentTab
            Case 0
                RandomizeTop.Children.Remove(_numPkmnLabel)
                RandomizeTop.Children.Remove(_skips)
            Case 1
                MovesTop.Children.Remove(_numPkmnLabel)
                MovesTop.Children.Remove(_skips)
            Case 2
                ManualTop.Children.Remove(_numPkmnLabel)
                ManualTop.Children.Remove(_skips)
            Case Else
                Throw New IndexOutOfRangeException("New tab is not yet handled")
        End Select

        ' Action on tab that was entered
        Select Case TabsBase.SelectedIndex
            Case 0
                RandomizeTop.Children.Add(_numPkmnLabel)
                RandomizeTop.Children.Add(_skips)
            Case 1
                MovesTop.Children.Add(_numPkmnLabel)
                MovesTop.Children.Add(_skips)
            Case 2
                ManualTop.Children.Add(_numPkmnLabel)
                ManualTop.Children.Add(_skips)
            Case Else
                Throw New IndexOutOfRangeException("New tab is not yet handled")
        End Select

        ' Update internal tab number
        _currentTab = TabsBase.SelectedIndex
    End Sub

    Private Sub Exit_App(sender As Object, e As RoutedEventArgs)
        System.Windows.Application.Current.Shutdown()
    End Sub
#End Region

#Region "Pkmn Columns"
    Private Sub Reset_Pkmn_Columns()
        Reset_Pkmn_Columns(0, TOTAL_INFO)
    End Sub

    Private Sub Reset_Pkmn_Columns(maxCol As Integer)
        Reset_Pkmn_Columns(0, maxCol)
    End Sub

    Private Sub Reset_Pkmn_Columns(minCol As Integer, maxCol As Integer)
        For index = minCol To maxCol
            _displays(index).Clear_Display()
        Next
    End Sub
#End Region

#Region "Utilities"
    Private Function Random_Entry(pkmnInfo As Pkmn) As List(Of Integer)
        Dim entryToUse = _ran.Next(maxValue:=pkmnInfo.pkmn.games.Count)
        Dim formIndex As Integer
        If pkmnInfo.pkmn.forms.Count > 1 Then
            If MenuForms.IsChecked Then
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
        Return New List(Of Integer) From {{formIndex}, {entryToUse},
            {_ran.Next(maxValue:=pkmnInfo.pkmn.abilities(formIndex).Count)}}
    End Function

    Private Function Generate_Random_Moves(moveList As List(Of MoveInfo)) As List(Of MoveInfo)
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

    Private Sub Build_Name_Map()
        _urlNameMap = New Dictionary(Of String, UrlInfo)
        For Each urlInfo In _urlList
            _urlNameMap.Add(urlInfo.name.ToLower, urlInfo)
            If urlInfo.name.ToLower.Contains("é") Then
                _urlNameMap.Add(urlInfo.name.ToLower.Replace("é", "e"), urlInfo)
            End If
            If urlInfo.name.ToLower.Contains("♂") Then
                _urlNameMap.Add(urlInfo.name.ToLower.Replace("♂", "-m"), urlInfo)
            End If
            If urlInfo.name.ToLower.Contains("♀") Then
                _urlNameMap.Add(urlInfo.name.ToLower.Replace("♀", "-f"), urlInfo)
            End If
        Next
    End Sub

    Private Function Validate_For_Url(textboxContent As String) As (Boolean, Integer, String)
        Dim pkmnNum As Integer
        If Not Integer.TryParse(textboxContent, pkmnNum) Then
            ' Test this as a name
            If Not _urlNameMap.ContainsKey(textboxContent.ToLower) Then
                Return (False, 0, $"{textboxContent} is not a valid name.")
            End If
            Return (True, _urlNameMap(textboxContent.ToLower).number, _urlNameMap(textboxContent.ToLower).url)
        Else
            ' Test this as a number
            pkmnNum = Integer.Parse(textboxContent)
            If pkmnNum <= 0 Or pkmnNum > _maxNum Then
                Return (False, 0, $"{textboxContent} is not a valid dex number.")
            End If
            Return (True, pkmnNum, _urlList(pkmnNum - 1).url)
        End If
    End Function
#End Region

#Region "Fill Listboxes"
    Private Sub Fill_Entry_List(pkmnInfo As Pkmn)
        For entryIndex = 0 To pkmnInfo.pkmn.games.Count - 1
            Dim entryDisplay = New EntryDisplay(pkmnInfo.pkmn.games(entryIndex), pkmnInfo.pkmn.entries(entryIndex))
            If entryIndex Mod 2 = 0 Then
                entryDisplay.BackgroundColor = "#FFFFFFFF"
            Else
                entryDisplay.BackgroundColor = "#FFF8F8F8"
            End If
            EntryListBox.Items.Add(entryDisplay)
        Next
    End Sub

    Private Sub Fill_Form_List(pkmnInfo As Pkmn)
        For formIndex = 0 To pkmnInfo.pkmn.forms.Count - 1
            FormListBox.Items.Add(New FormDisplay(pkmnInfo.images(formIndex), pkmnInfo.pkmn.forms(formIndex)))
        Next
    End Sub

    Private Sub Fill_Move_List(pkmnInfo As Pkmn)
        For formIndex = 0 To pkmnInfo.pkmn.moveForms.Count - 1
            Dim im As BitmapImage
            Dim abilityList As List(Of String)
            If pkmnInfo.pkmn.forms.Contains(pkmnInfo.pkmn.moveForms(formIndex)) Then
                im = pkmnInfo.images(pkmnInfo.pkmn.forms.IndexOf(pkmnInfo.pkmn.moveForms(formIndex)))
                abilityList = pkmnInfo.pkmn.abilities(pkmnInfo.pkmn.forms.IndexOf(pkmnInfo.pkmn.moveForms(formIndex)))
            Else
                im = pkmnInfo.images(0)
                abilityList = pkmnInfo.pkmn.abilities(0)
            End If
            Dim formName = pkmnInfo.pkmn.moveForms(formIndex)
            Dim randomMoves = Generate_Random_Moves(pkmnInfo.pkmn.moves(formIndex))
            Dim ability = abilityList(_ran.Next(abilityList.Count))
            Dim formMoves = New FormMoveDisplay(im, formName, ability, randomMoves)

            ' Color this grid depending on which row it is on
            If formIndex Mod 2 = 0 Then
                formMoves.BackgroundColor = "#FFFFFFFF"
            Else
                formMoves.BackgroundColor = "#FFF8F8F8"
            End If

            MoveListBox.Items.Add(formMoves)
        Next
    End Sub
#End Region
End Class
