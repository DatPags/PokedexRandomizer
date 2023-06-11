Imports PokedexRandomizerLib

Public Class FormMoveDisplay : Inherits Grid

    Private _bgColor As String
    Private _bgRectangle As Rectangle
    Private _pkmnImage As BitmapImage
    Private _image As Image
    Private _pkmnFormName As String
    Private _pkmnFormText As TextBlock
    Private _moveList As List(Of MoveInfo)
    Private _moveDisplayList As List(Of MoveDisplay)
    Private _pkmnAbility As String
    Private _pkmnAbilityText As TextBlock

    Public Property BackgroundColor() As String
        Get
            Return _bgColor
        End Get
        Set(ByVal value As String)
            _bgColor = value
            _bgRectangle.Fill = New SolidColorBrush(ColorConverter.ConvertFromString(value))
        End Set
    End Property

    Public Property PkmnImage() As BitmapImage
        Get
            Return _pkmnImage
        End Get
        Set(ByVal value As BitmapImage)
            _pkmnImage = value
            _image.Source = _pkmnImage
        End Set
    End Property

    Public Property PkmnFormName() As String
        Get
            Return _pkmnFormName
        End Get
        Set(ByVal value As String)
            _pkmnFormName = value
            _pkmnFormText.Text = _pkmnFormName
        End Set
    End Property

    Public Property PkmnAbility() As String
        Get
            Return _pkmnAbility
        End Get
        Set(ByVal value As String)
            _pkmnAbility = value
            _pkmnAbilityText.Text = "Ability: " & _pkmnAbility
        End Set
    End Property

    Public Property PkmnMoveList() As List(Of MoveInfo)
        Get
            Return _moveList
        End Get
        Set(ByVal value As List(Of MoveInfo))
            _moveList = value
            UpdateMoves()
        End Set
    End Property

    Public Sub New(Optional image As SixLabors.ImageSharp.Image = Nothing, Optional formName As String = "",
                   Optional ability As String = "", Optional randomMoves As List(Of MoveInfo) = Nothing)
        ' Initialize the base grid
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(1, GridUnitType.Star)})
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(4, GridUnitType.Star)})
        Me.ColumnDefinitions.Add(New ColumnDefinition With {.Width = New GridLength(4, GridUnitType.Star)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})

        ' Color this grid depending on which row it is on
        _bgRectangle = New Rectangle
        Me.BackgroundColor = "#FFFFFFFF"
        Grid.SetRowSpan(_bgRectangle, 2)
        Grid.SetColumnSpan(_bgRectangle, 3)
        Me.Children.Add(_bgRectangle)

        ' Build a grid for the form on the left side
        Dim formGrid = New Grid
        formGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(4, GridUnitType.Star)})
        formGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        formGrid.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})

        ' Create the image for this form
        _image = New Image With {
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(5, 5, 5, 5)
        }
        RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality)
        Me.PkmnImage = image.ToBitmapImage

        ' Create the text for this form
        _pkmnFormText = New TextBlock With {
            .TextWrapping = TextWrapping.Wrap,
            .TextAlignment = TextAlignment.Center,
            .HorizontalAlignment = HorizontalAlignment.Center,
            .Margin = New Thickness(5, 0, 5, 0)
        }
        Me.PkmnFormName = formName

        ' Create the text for this form's ability
        _pkmnAbilityText = New TextBlock With {
            .TextWrapping = TextWrapping.Wrap,
            .TextAlignment = TextAlignment.Center,
            .HorizontalAlignment = HorizontalAlignment.Center,
            .Margin = New Thickness(5, 0, 5, 0)
        }
        Me.PkmnAbility = ability

        ' Finalize the form grid
        formGrid.Children.Add(_image)
        Grid.SetRow(_pkmnFormText, 1)
        formGrid.Children.Add(_pkmnFormText)
        Grid.SetRow(_pkmnAbilityText, 2)
        formGrid.Children.Add(_pkmnAbilityText)
        Grid.SetRowSpan(formGrid, 2)
        Me.Children.Add(formGrid)

        ' Loop through each of the four random moves
        _moveDisplayList = New List(Of MoveDisplay)
        Me.PkmnMoveList = randomMoves
    End Sub

    Private Sub UpdateMoves()
        For Each moveDisplay In _moveDisplayList
            Me.Children.Remove(moveDisplay)
        Next
        _moveDisplayList = New List(Of MoveDisplay)

        Dim currentRow = 0
        Dim currentCol = 1
        For Each move In _moveList
            Dim moveDisplay = New MoveDisplay(move)
            If currentRow = 0 Then
                moveDisplay.VerticalAlignment = VerticalAlignment.Bottom
            Else
                moveDisplay.VerticalAlignment = VerticalAlignment.Top
            End If

            ' Add the move to the base grid
            Grid.SetRow(moveDisplay, currentRow)
            Grid.SetColumn(moveDisplay, currentCol)
            Me.Children.Add(moveDisplay)
            If currentCol < 2 Then
                currentCol += 1
            Else
                currentRow += 1
                currentCol = 1
            End If

            _moveDisplayList.Add(moveDisplay)
        Next
    End Sub

End Class
