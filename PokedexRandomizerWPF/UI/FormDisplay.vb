Public Class FormDisplay : Inherits Grid

    Private _im As Image
    Private _formText As TextBlock
    Private _image As BitmapImage
    Private _formName As String

    Public Property PkmnImage() As BitmapImage
        Get
            Return _image
        End Get
        Set(ByVal value As BitmapImage)
            _image = value
            _im.Source = _image
        End Set
    End Property

    Public Property PkmnFormName() As String
        Get
            Return _formName
        End Get
        Set(ByVal value As String)
            _formName = value
            _formText.Text = _formName
        End Set
    End Property

    Public Sub New(Optional image As BitmapImage = Nothing, Optional formName As String = "")
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(3, GridUnitType.Star)})
        Me.RowDefinitions.Add(New RowDefinition With {.Height = New GridLength(1, GridUnitType.Star)})

        _im = New Image With {
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(5, 5, 5, 5)
        }
        RenderOptions.SetBitmapScalingMode(_im, BitmapScalingMode.HighQuality)
        Me.PkmnImage = image

        _formText = New TextBlock With {
            .TextWrapping = TextWrapping.Wrap,
            .TextAlignment = TextAlignment.Center,
            .HorizontalAlignment = HorizontalAlignment.Center
        }
        Me.PkmnFormName = formName

        Me.Children.Add(_im)
        Grid.SetRow(_formText, 1)
        Me.Children.Add(_formText)
    End Sub

End Class
