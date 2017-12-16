Public Class UI

    Private Sub Image_MouseUp(sender As Object, e As RoutedEventArgs)
        Dim dimarea = TryCast(TryCast(sender, FrameworkElement)?.DataContext, DimArea)
        dimarea?.ToggleEnabled()
    End Sub

End Class
