Public Class SkipsDisplay : Inherits StackPanel
    Private Const NUM_SKIPS = 3

    Private _skipLabel As Label
    Private _skipChecks(NUM_SKIPS - 1) As CheckBox
    Private _resetButton As Button

    Public Sub New()
        Me.Orientation = Orientation.Horizontal
        Me.HorizontalAlignment = HorizontalAlignment.Right

        _skipLabel = New Label With {
            .FontSize = 14,
            .Content = "Skips:",
            .VerticalContentAlignment = VerticalAlignment.Center,
            .HorizontalContentAlignment = HorizontalAlignment.Right,
            .VerticalAlignment = VerticalAlignment.Center
        }

        For skipIdx = 0 To NUM_SKIPS - 1
            _skipChecks(skipIdx) = New CheckBox With {
                .VerticalAlignment = VerticalAlignment.Center,
                .Margin = New Thickness(2, 0, 2, 0),
                .IsChecked = True
            }
        Next

        _resetButton = New Button With {
            .Content = "Reset",
            .VerticalAlignment = VerticalAlignment.Center,
            .Padding = New Thickness(5, 1, 5, 1),
            .Margin = New Thickness(10, 0, 5, 0)
        }
        _resetButton.AddHandler(Button.ClickEvent, New RoutedEventHandler(AddressOf Skips_Reset))

        Me.Children.Add(_skipLabel)
        For skipIdx = 0 To NUM_SKIPS - 1
            Me.Children.Add(_skipChecks(skipIdx))
        Next
        Me.Children.Add(_resetButton)
    End Sub

    Private Sub Skips_Reset(sender As Object, e As RoutedEventArgs)
        For skipIdx = 0 To NUM_SKIPS - 1
            _skipChecks(skipIdx).IsChecked = True
        Next
    End Sub
End Class
