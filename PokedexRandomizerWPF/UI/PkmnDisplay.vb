Imports PokedexRandomizerLib

Public Class PkmnDisplay : Inherits Grid

    Private _pkmnInfo As Pkmn

    Private _image As Image
    Private _name As TextBox
    Private _forms As ComboBox
    Private _class As TextBox
    Private _type1 As TextBox
    Private _type2 As TextBox
    Private _height As TextBox
    Private _weight As TextBox
    Private _abilities As ComboBox
    Private _games As ComboBox
    Private _entry As TextBlock

    Public ReadOnly Property PkmnInfo() As Pkmn
        Get
            Return _pkmnInfo
        End Get
    End Property

    Public Sub New()
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(2.5, GridUnitType.Star)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(26, GridUnitType.Pixel)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(26, GridUnitType.Pixel)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(26, GridUnitType.Pixel)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(34, GridUnitType.Pixel)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(34, GridUnitType.Pixel)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(26, GridUnitType.Pixel)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(26, GridUnitType.Pixel)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(4.0, GridUnitType.Star)})

        _image = New Image With {
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(5, 5, 5, 5)
        }
        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality)

        _name = New TextBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .IsReadOnly = True,
            .FontWeight = FontWeights.Bold,
            .Margin = New Thickness(5, 0, 5, 0.3)
        }

        _forms = New ComboBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(5, 0, 5, 0)
        }
        _forms.AddHandler(ComboBox.SelectionChangedEvent, New RoutedEventHandler(AddressOf FormChange))

        _class = New TextBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .IsReadOnly = True,
            .Margin = New Thickness(5, 0, 5, 0.3)
        }

        Dim typeGrid = New Grid
        typeGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        typeGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        typeGrid.VerticalAlignment = VerticalAlignment.Center

        _type1 = New TextBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .IsReadOnly = True,
            .Margin = New Thickness(5, 0, 5, 0),
            .TextAlignment = TextAlignment.Center,
            .Foreground = Brushes.White
        }

        _type2 = New TextBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .IsReadOnly = True,
            .Margin = New Thickness(5, 0, 5, 0),
            .TextAlignment = TextAlignment.Center,
            .Foreground = Brushes.White
        }

        typeGrid.Children.Add(_type1)
        Grid.SetColumn(_type2, 1)
        typeGrid.Children.Add(_type2)

        Dim statGrid = New Grid
        statGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        statGrid.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        statGrid.VerticalAlignment = VerticalAlignment.Center

        _height = New TextBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .IsReadOnly = True,
            .Margin = New Thickness(5, 0, 5, 0),
            .TextWrapping = TextWrapping.Wrap
        }

        _weight = New TextBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .IsReadOnly = True,
            .Margin = New Thickness(5, 0, 5, 0),
            .TextWrapping = TextWrapping.Wrap
        }

        statGrid.Children.Add(_height)
        Grid.SetColumn(_weight, 1)
        statGrid.Children.Add(_weight)

        _abilities = New ComboBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(5, 0, 5, 0)
        }

        _games = New ComboBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(5, 0, 5, 0)
        }
        _games.AddHandler(ComboBox.SelectionChangedEvent, New RoutedEventHandler(AddressOf GameChange))

        Dim entryBorder = New Border With {
            .BorderThickness = New Thickness(1),
            .Margin = New Thickness(5, 5, 5, 5),
            .BorderBrush = Brushes.LightSlateGray
        }
        Dim scrollViewer = New ScrollViewer With {
            .VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        }
        _entry = New TextBlock With {
            .TextWrapping = TextWrapping.Wrap,
            .Margin = New Thickness(5, 5, 5, 5)
        }
        scrollViewer.Content = _entry
        entryBorder.Child = scrollViewer

        Me.Children.Add(_image)
        Grid.SetRow(_name, 1)
        Me.Children.Add(_name)
        Grid.SetRow(_forms, 2)
        Me.Children.Add(_forms)
        Grid.SetRow(_class, 3)
        Me.Children.Add(_class)
        Grid.SetRow(typeGrid, 4)
        Me.Children.Add(typeGrid)
        Grid.SetRow(statGrid, 5)
        Me.Children.Add(statGrid)
        Grid.SetRow(_abilities, 6)
        Me.Children.Add(_abilities)
        Grid.SetRow(_games, 7)
        Me.Children.Add(_games)
        Grid.SetRow(entryBorder, 8)
        Me.Children.Add(entryBorder)
    End Sub

    Public Sub SetLoading()
        SetText("Loading...")
    End Sub

    Public Sub SetText(text As String)
        ClearDisplay()
        _name.Text = text
    End Sub

    Public Sub PopulateDisplay(pkmnInfo As Pkmn, formIndex As Integer, gameIndex As Integer, abilityIndex As Integer)
        _pkmnInfo = pkmnInfo

        _name.Text = $"#{_pkmnInfo.pkmn.number} - {_pkmnInfo.pkmn.name}"
        If _pkmnInfo.pkmn.forms.Count > 1 Then
            _forms.ItemsSource = _pkmnInfo.pkmn.forms
        Else
            _forms.ItemsSource = New List(Of String) From {{""}}
        End If
        _games.ItemsSource = _pkmnInfo.pkmn.games

        If _pkmnInfo.pkmn.forms.Count > 1 Then
            _forms.SelectedValue = _pkmnInfo.pkmn.forms(formIndex)
        Else
            _forms.SelectedValue = ""
        End If

        If _pkmnInfo.pkmn.games.Count > 0 Then
            _games.SelectedValue = _pkmnInfo.pkmn.games(gameIndex)
        Else
            _entry.Text = "Ecology under research."
        End If

        _abilities.SelectedIndex = abilityIndex
    End Sub

    Public Sub ClearDisplay()
        _image.Source = Nothing
        _name.Text = ""
        _forms.ItemsSource = Nothing
        _class.Text = ""
        _type1.Text = ""
        _type1.Background = Util.GetPkmnTypeColor("").ToBrush
        _type2.Text = ""
        _type2.Background = Util.GetPkmnTypeColor("").ToBrush
        _height.Text = ""
        _weight.Text = ""
        _abilities.ItemsSource = Nothing
        _games.ItemsSource = Nothing
        _entry.Text = ""
        _pkmnInfo = Nothing
    End Sub

    Private Sub FormChange(sender As Object, e As SelectionChangedEventArgs)
        Dim formIndex As Integer = _forms.SelectedIndex
        If formIndex >= 0 Then
            _image.Source = _pkmnInfo.GetImage(formIndex)
            _class.Text = _pkmnInfo.pkmn.species(formIndex)
            _type1.Text = _pkmnInfo.pkmn.types(formIndex)(0)
            _type1.Background = Util.GetPkmnTypeColor(_pkmnInfo.pkmn.types(formIndex)(0)).ToBrush
            _type2.Text = _pkmnInfo.pkmn.types(formIndex)(1)
            _type2.Background = Util.GetPkmnTypeColor(_pkmnInfo.pkmn.types(formIndex)(1)).ToBrush
            _height.Text = _pkmnInfo.pkmn.height(formIndex)
            _weight.Text = _pkmnInfo.pkmn.weight(formIndex)
            _abilities.ItemsSource = _pkmnInfo.pkmn.abilities(formIndex)
            _abilities.SelectedIndex = 0
        End If
    End Sub

    Private Sub GameChange(sender As Object, e As SelectionChangedEventArgs)
        Dim gameIndex As Integer = _games.SelectedIndex
        If gameIndex >= 0 Then
            _entry.Text = _pkmnInfo.pkmn.entries(gameIndex)
        End If
    End Sub
End Class
