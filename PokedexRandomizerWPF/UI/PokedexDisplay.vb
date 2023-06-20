Imports PokedexRandomizerLib

Public Class PokedexDisplay
    Inherits Grid

    Private _pkmnNumber As Integer

    Public ReadOnly Property PkmnNumber As Integer
        Get
            Return _pkmnNumber
        End Get
    End Property

    Public Sub New(pkmn As Pkmn, useImages As Boolean)
        _pkmnNumber = pkmn.pkmn.number

        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})
        Me.Height = 80
        Me.Width = 80
        Me.ToolTip = "#" + pkmn.pkmn.number.ToString("D4") + ": " + pkmn.pkmn.name

        Dim border As New Border With {
            .BorderThickness = New Thickness(2),
            .BorderBrush = New SolidColorBrush(Color.FromRgb(225, 225, 225))
        }

        Dim element As FrameworkElement
        If useImages Then
            element = New Image With {
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center,
                .Source = pkmn.GetIcon(0)
            }
            RenderOptions.SetBitmapScalingMode(element, BitmapScalingMode.HighQuality)
        Else
            element = New TextBlock With {
                .TextWrapping = TextWrapping.Wrap,
                .TextAlignment = TextAlignment.Center,
                .HorizontalAlignment = HorizontalAlignment.Center,
                .VerticalAlignment = VerticalAlignment.Center,
                .Text = "#" + pkmn.pkmn.number.ToString("D4") + vbCrLf + pkmn.pkmn.name
            }
        End If

        border.Child = element
        Me.Children.Add(border)
    End Sub
End Class
