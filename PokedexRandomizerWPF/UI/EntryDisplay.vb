Imports PokedexRandomizerLib

Public Class EntryDisplay : Inherits Grid

    Private _gameText As TextBlock
    Private _entryText As TextBlock
    Private _bgRectangle As Rectangle
    Private _bgColor As String

    Public Property PkmnGame() As String
        Get
            Return _gameText.Text
        End Get
        Set(ByVal value As String)
            Dim gameParts = value.Split(",").ToList
            Dim lastComma = gameParts.Count - 2
            If gameParts(gameParts.Count - 1).Contains("(") Then
                Dim lastPart = gameParts(gameParts.Count - 1).Split("(")
                gameParts.RemoveAt(gameParts.Count - 1)
                gameParts.Add(lastPart.First)
                gameParts.Add("(" & lastPart.Last)
            End If
            _gameText.Inlines.Clear()
            For partIdx = 0 To gameParts.Count - 1
                Dim part = gameParts(partIdx)
                If part.StartsWith("(") Then
                    _gameText.Inlines.Add(New Run(part))
                Else
                    _gameText.Inlines.Add(New Run(part) With {.Foreground = Util.GetPkmnGameColor(part.Trim).ToBrush})
                End If
                If partIdx <= lastComma Then
                    _gameText.Inlines.Add(New Run(","))
                End If
            Next
        End Set
    End Property

    Public Property PkmnEntry() As String
        Get
            Return _entryText.Text
        End Get
        Set(ByVal value As String)
            _entryText.Text = value
        End Set
    End Property

    Public Property BackgroundColor() As String
        Get
            Return _bgColor
        End Get
        Set(ByVal value As String)
            _bgColor = value
            _bgRectangle.Fill = New SolidColorBrush(ColorConverter.ConvertFromString(value))
        End Set
    End Property

    Public Sub New(Optional gameName As String = "", Optional entry As String = "")
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(3, GridUnitType.Star)})

        _gameText = New TextBlock With {
            .TextWrapping = TextWrapping.Wrap,
            .Margin = New Thickness(5, 0, 5, 0)
        }
        Me.PkmnGame = gameName

        _entryText = New TextBlock With {
            .TextWrapping = TextWrapping.Wrap,
            .Margin = New Thickness(0, 0, 5, 0)
        }
        Me.PkmnEntry = entry

        _bgRectangle = New Rectangle
        Grid.SetColumnSpan(_bgRectangle, 2)
        Me.Children.Add(_bgRectangle)

        Me.Children.Add(_gameText)
        Grid.SetColumn(_entryText, 1)
        Me.Children.Add(_entryText)
    End Sub

End Class
