Imports System.ComponentModel

Class ConfigWindow

#Region "Window"

    Private Sub ConfigWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles ConfigWindow.Loaded

        'Listen to when dim areas are added or removed
        AddHandler App.Current.DimAreaManager.DimAreaAdded, AddressOf DimAreaManager_DimAreaAdded
        AddHandler App.Current.DimAreaManager.DimAreaRemoved, AddressOf DimAreaManager_DimAreaRemoved

        'Update UI elements that needs to be updated at this stage
        UpdateUndimOptions()
        UpdateUpDownButtons()
        UpdatePreview()

    End Sub

    Private HasShownNotification As Boolean

    Private Sub ConfigWindow_Closing(sender As Object, e As CancelEventArgs) Handles ConfigWindow.Closing

        e.Cancel = True
        Hide()
        FinishRemove()

        'Make sure that all dimareas are saved
        App.Current.DimAreaManager.SaveAll()

        If Not HasShownNotification And App.Current.IsFirstStartup Then
            HasShownNotification = True
            App.Current.NotifyIcon.ShowBalloonTip("Dimmer", "Configuration window has been closed. The application can now be accessed from the notification bar.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.None)
        End If

    End Sub

    Private Sub ConfigWindow_MouseDown(sender As Object, e As MouseButtonEventArgs)
        'Unselect element when clicking on a blank space
        FocusManager.SetFocusedElement(Me, Me)
    End Sub

#End Region
#Region "UI"

    'Used to track whatever UI elements are updating so that they do not make redundant save calls
    Private IsLoading As Boolean = False

#Region "Undim options"

    'Manages the undim options

    Private Sub DimOptionMoveButton_Click(sender As Object, e As RoutedEventArgs)
        CType(List.SelectedItem, DimArea).UndimOption = UndimOption.MouseMove
    End Sub

    Private Sub DimOptionClickButton_Click(sender As Object, e As RoutedEventArgs)
        CType(List.SelectedItem, DimArea).UndimOption = UndimOption.MouseClick
    End Sub

    Private Sub UpdateUndimOptions()
        If Not IsLoading Then
            IsLoading = True
            If List.SelectedItem IsNot Nothing Then
                Dim item As DimArea = List.SelectedItem
                DimOptionMoveButton.IsChecked = (item.UndimOption = UndimOption.MouseMove)
                DimOptionClickButton.IsChecked = (item.UndimOption = UndimOption.MouseClick)
            Else
                DimOptionMoveButton.IsChecked = False
                DimOptionClickButton.IsChecked = False
            End If
            IsLoading = False
        End If
    End Sub

#End Region
#Region "Color boxes"

    'Manages the color box and alpha box

    Private Sub UpdateColorBoxes()
        If List.SelectedItem IsNot Nothing Then
            IsLoading = True
            ColorBox.SelectedColor = CType(List.SelectedItem, DimArea).Color
            AlphaBox.Text = AlphaToPercentage(CType(List.SelectedItem, DimArea).Color.A)
            IsLoading = False
        End If
    End Sub

    Private Sub SetColor()
        If Not IsLoading AndAlso List.SelectedItem IsNot Nothing Then

            Dim color As Color = ColorBox.SelectedColor

            Dim perc As Integer
            If Integer.TryParse(AlphaBox.Text, perc) Then
                color.A = PercentageToAlpha(perc)
                CType(List.SelectedItem, DimArea).Color = color
            End If

        End If
    End Sub

#End Region
#Region "Mouse hide delay"

    'Manages the mouse hide delay box

    Private Sub UpdateMouseHideDelay()
        If List.SelectedItem IsNot Nothing Then
            IsLoading = True

            With CType(List.SelectedItem, DimArea).HideMouseDelay
                If .HasValue Then
                    MouseHideDelayBox.Text = .Value.TotalSeconds
                Else
                    MouseHideDelayBox.Text = String.Empty
                End If
            End With

            IsLoading = False
        End If
    End Sub

    Private Sub SetMouseHideDelay()

        If Not IsLoading AndAlso List.SelectedItem IsNot Nothing Then

            Dim sec As Double
            If Double.TryParse(MouseHideDelayBox.Text, sec) AndAlso sec >= 0 Then
                CType(List.SelectedItem, DimArea).HideMouseDelay = TimeSpan.FromSeconds(sec)
            End If

        End If

    End Sub

#End Region
#Region "Redim delay"

    'Manages the redim delay box

    Private Sub UpdateRedimDelay()
        If List.SelectedItem IsNot Nothing Then
            IsLoading = True

            With CType(List.SelectedItem, DimArea).RedimDelay
                If .HasValue Then
                    RedimDelayBox.Text = .Value.TotalSeconds
                Else
                    RedimDelayBox.Text = String.Empty
                End If
            End With

            IsLoading = False
        End If
    End Sub

    Private Sub SetRedimDelay()

        If Not IsLoading AndAlso List.SelectedItem IsNot Nothing Then

            Dim sec As Double
            If Double.TryParse(RedimDelayBox.Text, sec) AndAlso sec >= 0 Then
                CType(List.SelectedItem, DimArea).RedimDelay = TimeSpan.FromSeconds(sec)
            End If

        End If

    End Sub

#End Region
#Region "Add / remove / undo"

    'Manages the add, remove and undo 'buttons'

    Private Sub AddButton_Click(sender As Object, e As RoutedEventArgs)
        DimArea.CreateNew()
    End Sub

    Private Sub RemoveButton_Click(sender As Object, e As RoutedEventArgs)
        FinishRemove() 'Finish remove if one is in progress
        BeginRemove() 'Begin remove process of new item
    End Sub

    Private removeditem As DimArea
    Private Sub BeginRemove()
        If List.SelectedItem IsNot Nothing Then
            removeditem = List.SelectedItem
            removeditem.Remove()
            UndoNotification.Visibility = Visibility.Visible
            UndoNotification.BeginStoryboard(UndoNotification.FindResource("ShowHideAnimation"))
        End If
    End Sub

    Private Sub FinishRemove()
        If removeditem IsNot Nothing Then
            removeditem.FinalizeRemove()
            removeditem = Nothing
        End If
    End Sub

    Private Sub UndoLink_Click(sender As Object, e As RoutedEventArgs)
        UndoRemove()
    End Sub

    Private Sub UndoRemove()
        If removeditem IsNot Nothing Then
            removeditem.UndoRemove()
            removeditem = Nothing
            UndoNotification.BeginStoryboard(UndoNotification.FindResource("HideAnimation"))
        End If
    End Sub

#End Region
#Region "Make sure item is selected"

    Private Sub DimAreaManager_DimAreaAdded()
        If List.Items.Count > 0 Then
            List.SelectedItem = List.Items(List.Items.Count - 1)
            FocusManager.SetFocusedElement(Me, List)
        End If
        contentarea.IsEnabled = Not (List.Items.Count = 0)
    End Sub

    Private Sub DimAreaManager_DimAreaRemoved()
        If List.Items.Count > 0 AndAlso List.SelectedItem Is Nothing Then
            List.SelectedItem = List.Items(List.Items.Count - 1)
        End If
        contentarea.IsEnabled = Not (List.Items.Count = 0)
    End Sub

#End Region
#Region "Move up / down"

    'Manages move up / down buttons

    Private Sub UpButton_Click(sender As Object, e As RoutedEventArgs)
        MoveUp()
    End Sub

    Private Sub DownButton_Click(sender As Object, e As RoutedEventArgs)
        MoveDown()
    End Sub

    Private Sub MoveUp()
        Dim item As DimArea = List.SelectedItem
        App.Current.DimAreaManager.MoveItem(item, App.Current.DimAreaManager.DimAreas.IndexOf(item) - 1)
        List.SelectedItem = item
        UpdateUpDownButtons()
    End Sub

    Private Sub MoveDown()
        Dim item As DimArea = List.SelectedItem
        App.Current.DimAreaManager.MoveItem(item, App.Current.DimAreaManager.DimAreas.IndexOf(item) + 1)
        List.SelectedItem = item
        UpdateUpDownButtons()
    End Sub

    Private Sub UpdateUpDownButtons()
        If IsLoaded Then
            DownButton.IsEnabled = (List.SelectedItem IsNot Nothing AndAlso Not List.SelectedIndex = App.Current.DimAreaManager.MaxIndex)
            UpButton.IsEnabled = (List.SelectedItem IsNot Nothing AndAlso Not List.SelectedIndex = 0)
        End If
    End Sub

#End Region
#Region "List context menu"

    'Manages the list context menu

    Private Sub UpdateContextMenu()

        Dim area As DimArea = List.SelectedItem

        If area IsNot Nothing Then

            If area.IsFavorited Then
                FavoriteMenuItem.Header = "Remove as favorite"
            Else
                FavoriteMenuItem.Header = "Set as favorite"
            End If

            MoveDownMenuItem.IsEnabled = (List.SelectedItem IsNot Nothing AndAlso Not List.SelectedIndex = App.Current.DimAreaManager.MaxIndex)
            MoveUpMenuItem.IsEnabled = (List.SelectedItem IsNot Nothing AndAlso Not List.SelectedIndex = 0)

        End If

    End Sub

    Private Sub FavoriteMenuItem_Click(sender As Object, e As RoutedEventArgs)
        If App.Current.DimAreaManager.FavoriteDimArea IsNot List.SelectedItem Then
            App.Current.DimAreaManager.FavoriteDimArea = List.SelectedItem
        Else
            App.Current.DimAreaManager.FavoriteDimArea = Nothing
        End If
        ListContextMenu.IsOpen = False
    End Sub

    Private Sub MoveUpMenuItem_Click(sender As Object, e As RoutedEventArgs)
        MoveUp()
        ListContextMenu.IsOpen = False
    End Sub

    Private Sub MoveDownMenuItem_Click(sender As Object, e As RoutedEventArgs)
        MoveDown()
        ListContextMenu.IsOpen = False
    End Sub

    Private Sub RemoveMenuItem_Click(sender As Object, e As RoutedEventArgs)
        BeginRemove()
    End Sub

    Private Sub List_ContextMenuOpening(sender As Object, e As ContextMenuEventArgs)

        'Make sure that the context menu does not open when a blank space is clicked. (also check for whatever the eye button was clicked since it will trigger on right click as well)
        If TypeOf e.OriginalSource.DataContext IsNot DimArea Or TypeOf e.OriginalSource Is Image Then
            e.Handled = True
            ListContextMenu.IsOpen = False
        Else
            UpdateContextMenu()
        End If

    End Sub

#End Region
#Region "Credits"

    'Manages the credits panel

    Private Sub CreditsPanel_MouseEnter(sender As Object, e As MouseEventArgs)
        CreditsPanel.BeginStoryboard(CreditsPanel.FindResource("ShowAnimation"))
    End Sub

    Private Sub CreditsPanel_MouseLeave(sender As Object, e As MouseEventArgs)
        CreditsPanel.BeginStoryboard(CreditsPanel.FindResource("HideAnimation"))
    End Sub

    Private Sub CreditsShowAnimation_Completed(sender As Object, e As EventArgs)
        CreditsActiveLabel.Opacity = 0
        CreditsActiveLabel.Visibility = Visibility.Visible
    End Sub

    Private Sub CreditsHideAnimation_Completed(sender As Object, e As EventArgs)
        CreditsActiveLabel.Visibility = Visibility.Collapsed
    End Sub

    Private Sub CreditsLink_MouseUp(sender As Run, e As MouseButtonEventArgs)
        Process.Start(sender.Tag)
    End Sub

#End Region

    Private Sub UpdatePreview()
        Preview.DimArea = List.SelectedItem
    End Sub

    Private Sub List_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        UpdateUpDownButtons()
        UpdateUndimOptions()
        UpdateColorBoxes()
        UpdateMouseHideDelay()
        UpdateRedimDelay()
        UpdatePreview()
    End Sub

#End Region

End Class