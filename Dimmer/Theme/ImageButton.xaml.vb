Public Class ImageButton

    Public Shared Shadows ReadOnly ContentProperty As DependencyProperty = DependencyProperty.Register(
        "Content", GetType(ImageSource), GetType(ImageButton))

    Public Shadows Property Content As ImageSource
        Get
            Return Me.GetValue(ContentProperty)
        End Get
        Set(value As ImageSource)
            Me.SetValue(ContentProperty, value)
        End Set
    End Property

    Public Shared Shadows ReadOnly OpacityProperty As DependencyProperty = DependencyProperty.Register(
        "Opacity", GetType(Double), GetType(ImageButton), New PropertyMetadata(1.0))

    Public Shadows Property Opacity As Double
        Get
            Return Me.GetValue(OpacityProperty)
        End Get
        Set(value As Double)
            Me.SetValue(OpacityProperty, value)
        End Set
    End Property

    Public Shared Shadows ReadOnly PaddingProperty As DependencyProperty = DependencyProperty.Register(
        "Padding", GetType(Thickness), GetType(ImageButton), New PropertyMetadata(New Thickness(4)))

    Public Shadows Property Padding As Thickness
        Get
            Return Me.GetValue(PaddingProperty)
        End Get
        Set(value As Thickness)
            Me.SetValue(PaddingProperty, value)
        End Set
    End Property

    Public Event Click(sender As Object, e As RoutedEventArgs)

    Private Sub Button_Click(sender As Object, e As RoutedEventArgs)
        RaiseEvent Click(sender, e)
    End Sub

End Class
