Imports PokedexRandomizerLib

Public Class MoveDisplay : Inherits Grid
    Private _nameText As Label
    Private _typeText As TextBox
    Private _category As Image
    Private _catText As String
    Private _powerVal As String
    Private _powerText As Label
    Private _accVal As String
    Private _accText As Label
    Private _infoRetriever As PkmnInfoRetriever

    Public Property MoveName() As String
        Get
            Return _nameText.Content
        End Get
        Set(value As String)
            _nameText.Content = value
        End Set
    End Property

    Public Property MoveType() As String
        Get
            Return _typeText.Text
        End Get
        Set(value As String)
            _typeText.Text = value
        End Set
    End Property

    Public Property MoveCategory() As String
        Get
            Return _catText
        End Get
        Set(value As String)
            _catText = value
            _category.Source = _infoRetriever.GetMoveCategoryImage(value.ToLower)
        End Set
    End Property

    Public Property MovePower() As String
        Get
            Return _powerVal
        End Get
        Set(value As String)
            _powerVal = value
            _powerText.Content = "Pow: " & value
        End Set
    End Property

    Public Property MoveAccuracy() As String
        Get
            Return _accVal
        End Get
        Set(value As String)
            Dim acc As Integer
            If Integer.TryParse(value, acc) AndAlso acc > 100 Then
                value = "∞"
            End If
            _accVal = value
            _accText.Content = "Acc: " & value
        End Set
    End Property

    Public ReadOnly Property PkmnMove() As MoveInfo
        Get
            Return New MoveInfo With {.name = MoveName, .type = MoveType, .category = MoveCategory,
                .power = MovePower, .accuracy = MoveAccuracy}
        End Get
    End Property

    Public Sub New(pkmnInfo As MoveInfo, infoRetriever As PkmnInfoRetriever)
        Me.New(pkmnInfo.name, pkmnInfo.type, pkmnInfo.category, pkmnInfo.power, pkmnInfo.accuracy, infoRetriever)
    End Sub

    Public Sub New(Optional name As String = "", Optional type As String = "", Optional category As String = "Physical",
                   Optional power As String = "", Optional acc As String = "", Optional infoRetriever As PkmnInfoRetriever = Nothing)
        _infoRetriever = infoRetriever
        Me.Background = New SolidColorBrush(ColorConverter.ConvertFromString("#FFF0F0F0"))
        Me.Margin = New Thickness(5, 5, 5, 5)
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(3, GridUnitType.Star)})
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1.5, GridUnitType.Star)})
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1.5, GridUnitType.Star)})
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1.5, GridUnitType.Star)})

        _nameText = New Label With {
            .VerticalAlignment = VerticalAlignment.Center
        }
        Me.MoveName = name

        _typeText = New TextBox With {
            .Background = Util.GetPkmnTypeColor(type).ToBrush,
            .VerticalAlignment = VerticalAlignment.Center,
            .TextAlignment = TextAlignment.Center,
            .IsReadOnly = True,
            .Foreground = Brushes.White
        }
        Me.MoveType = type

        _category = New Image With {
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center
        }
        Me.MoveCategory = category

        _powerText = New Label With {
            .VerticalAlignment = VerticalAlignment.Center
        }
        Me.MovePower = power

        _accText = New Label With {
            .VerticalAlignment = VerticalAlignment.Center
        }
        Me.MoveAccuracy = acc

        Me.Children.Add(_nameText)
        Grid.SetColumn(_typeText, 1)
        Me.Children.Add(_typeText)
        Grid.SetColumn(_category, 2)
        Me.Children.Add(_category)
        Grid.SetColumn(_powerText, 3)
        Me.Children.Add(_powerText)
        Grid.SetColumn(_accText, 4)
        Me.Children.Add(_accText)
    End Sub
End Class
