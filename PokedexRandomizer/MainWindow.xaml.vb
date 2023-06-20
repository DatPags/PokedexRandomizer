Class MainWindow
    Private Const MAX_COLS = 6
    Private Const MOVES_IDX = MAX_COLS
    Private Const MANUAL_IDX = MAX_COLS + 1
    Private Const TOTAL_INFO = MAX_COLS + 1

    Private _pkmnInfoRetriever As PkmnInfoRetriever
    Private _settings As Settings

    Private _ran As Random = New Random
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

        ' Initialize settings
        _settings = New Settings()
        MenuForms.IsChecked = _settings.PrioritizeForms
        MenuCache.IsChecked = _settings.UseCache
        MenuCache.Visibility = Visibility.Collapsed
#If DEBUG Then
        MenuCache.Visibility = Visibility.Visible
#End If

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

        ' initialize the model
        _numPkmnLabel.Content = "Initializing..."
        Dim infoEngine As IPkmnInfoFinder = Await PkmnInfoFinderPokemonDB.Create_Self()
        Dim imageEngine As IPkmnImageFinder = Await PkmnImageFinderPokesprite.Create_Self()
        Dim cache As New AppDataLocalCache()
        _pkmnInfoRetriever = New PkmnInfoRetriever(infoEngine, imageEngine, infoCache:=cache, imageCache:=cache)
        _numPkmnLabel.Content = "Total number of Pokémon: " & _pkmnInfoRetriever.Total_Number_Of_Pokemon().ToString
        MoveDisplay.Init_Cat_Images(_settings, cache)

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
            Dim number = _ran.Next(maxValue:=_pkmnInfoRetriever.Total_Number_Of_Pokemon())
            If Not randomNumbers.Contains(number) Then
                randomNumbers.Add(number)
            End If
        Loop Until randomNumbers.Count >= MAX_COLS

        Dim tasks As New List(Of Task(Of Pkmn))
        For index = 0 To NumberCombobox.SelectedIndex
            tasks.Add(_pkmnInfoRetriever.Get_Pkmn_Info(randomNumbers(index) + 1, _settings))
            _displays(index).Set_Loading()
        Next

        Dim listIndex = 0
        For Each pkmnInfo In Await Task.WhenAll(tasks)
            Dim form_game_index = _pkmnInfoRetriever.Random_Entry(pkmnInfo, _settings)
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

        Dim pkmnNum As Integer, result As String = ""
        If Not _pkmnInfoRetriever.Validate_Search(ManualTextBox.Text.Trim, pkmnNum, result) Then
            _displays(MANUAL_IDX).Set_Text(result)
            ManualButton.IsEnabled = True
            Exit Sub
        End If

        _displays(MANUAL_IDX).Set_Loading()
        Dim pkmnInfo = Await _pkmnInfoRetriever.Get_Pkmn_Info(pkmnNum, _settings)
        _displays(MANUAL_IDX).Populate_Display(pkmnInfo, 0, 0, 0)
        Fill_Entry_List(pkmnInfo)
        Fill_Form_List(pkmnInfo)

        ManualButton.IsEnabled = True
    End Sub

    Private Async Sub MovesButton_Click(sender As Object, e As RoutedEventArgs) Handles MovesButton.Click
        MovesButton.IsEnabled = False
        Reset_Pkmn_Columns(MOVES_IDX, MOVES_IDX)
        MoveListBox.Items.Clear()

        Dim pkmnNum As Integer, result As String = ""
        If Not _pkmnInfoRetriever.Validate_Search(MovesTextBox.Text.Trim, pkmnNum, result) Then
            _displays(MOVES_IDX).Set_Text(result)
            MovesButton.IsEnabled = True
            Exit Sub
        End If

        _displays(MOVES_IDX).Set_Loading()
        Dim pkmnInfo = Await _pkmnInfoRetriever.Get_Pkmn_Info(pkmnNum, _settings)
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
                formMoves.PkmnMoveList = _pkmnInfoRetriever.Generate_Random_Moves(pkmnInfo.pkmn.moves(formIndex))
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

    Private Sub MenuCache_Click(sender As Object, e As RoutedEventArgs) Handles MenuCache.Click
        _settings.UseCache = MenuCache.IsChecked
        Debug.WriteLine("DEBUG: Cache is " + If(_settings.UseCache, "enabled", "disabled"))
    End Sub

    Private Sub MenuForms_Click(sender As Object, e As RoutedEventArgs) Handles MenuForms.Click
        _settings.PrioritizeForms = MenuForms.IsChecked
    End Sub

    Private Sub MenuCacheClear_Click(sender As Object, e As RoutedEventArgs) Handles MenuCacheClear.Click

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
            Dim randomMoves = _pkmnInfoRetriever.Generate_Random_Moves(pkmnInfo.pkmn.moves(formIndex))
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
