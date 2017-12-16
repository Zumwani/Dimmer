Imports System.ComponentModel
Imports System.Windows.Threading

Public Class DimAreaElement

#Region "Properties"

    Public Shared ReadOnly ColorProperty As DependencyProperty = DependencyProperty.Register("Color", GetType(Color), GetType(DimAreaElement), New PropertyMetadata(Colors.Black))
    Public Shared ReadOnly DimAreaProperty As DependencyProperty = DependencyProperty.Register("DimArea", GetType(DimArea), GetType(DimAreaElement), New PropertyMetadata(Nothing, Sub(d, p)

                                                                                                                                                                                       Dim handler As PropertyChangedEventHandler = AddressOf CType(d, DimAreaElement).Dimarea_PropertyChanged

                                                                                                                                                                                       If p.OldValue IsNot Nothing Then
                                                                                                                                                                                           RemoveHandler CType(p.OldValue, DimArea).PropertyChanged, handler
                                                                                                                                                                                       End If
                                                                                                                                                                                       If p.NewValue IsNot Nothing Then
                                                                                                                                                                                           AddHandler CType(p.NewValue, DimArea).PropertyChanged, handler
                                                                                                                                                                                           CType(p.NewValue, DimArea).Parent = d
                                                                                                                                                                                       End If

                                                                                                                                                                                       CType(d, DimAreaElement).UpdateColor(p.NewValue)


                                                                                                                                                                                   End Sub))

    Public Property DimArea As DimArea
        Get
            Return Me.GetValue(DimAreaProperty)
        End Get
        Set(value As DimArea)
            Me.SetValue(DimAreaProperty, value)
        End Set
    End Property

    Private _IsVisible As Boolean
    Public Shadows ReadOnly Property IsVisible As Boolean
        Get
            Return _IsVisible
        End Get
    End Property

    Public Property Color As Color
        Get
            Return Me.GetValue(ColorProperty)
        End Get
        Set(value As Color)
            Me.SetValue(ColorProperty, value)
            Me.RaiseEvent(New RoutedEventArgs(ColorChangedEvent))
        End Set
    End Property

    Public ReadOnly Property Bounds As Rect
        Get
            Return Me.ScreenBounds
        End Get
    End Property

    Public Shadows ReadOnly Property IsMouseOver As Boolean
        Get
            Dim pos As Point = Win32.Managed.CursorPosition
            Return If(Not Bounds.IsEmpty, Bounds.Contains(pos), False)
        End Get
    End Property

    Property _IsCursorVisible As Boolean = True
    Public Property IsCursorVisible As Boolean
        Get
            Return _IsCursorVisible
        End Get
        Set(value As Boolean)
            If value = True Then
                ShowCursor()
            Else
                HideCursor()
            End If
        End Set
    End Property

    Private Sub Dimarea_PropertyChanged(sender As DimArea, e As PropertyChangedEventArgs)
        If e.PropertyName = "Color" Then
            UpdateColor(sender)
        End If
    End Sub

    Private Sub UpdateColor(Optional DimArea As DimArea = Nothing)
        If DimArea Is Nothing Then
            DimArea = Me.DimArea
        End If
        If DimArea IsNot Nothing AndAlso Not DimArea.IsRemovePending Then
            Color = DimArea.Color
        Else
            Color = Colors.Transparent
        End If
    End Sub

    Public Shared ReadOnly ColorChangedEvent As RoutedEvent = EventManager.RegisterRoutedEvent("ColorChanged", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(DimAreaElement))
    Public Shared ReadOnly ShownEvent As RoutedEvent = EventManager.RegisterRoutedEvent("Shown", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(DimAreaElement))
    Public Shared ReadOnly HidEvent As RoutedEvent = EventManager.RegisterRoutedEvent("Hid", RoutingStrategy.Direct, GetType(RoutedEventHandler), GetType(DimAreaElement))

    Public Custom Event ColorChanged As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            Me.AddHandler(ColorChangedEvent, value)
        End AddHandler
        RemoveHandler(value As RoutedEventHandler)
            Me.RemoveHandler(ColorChangedEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)
            Me.RaiseEvent(New RoutedEventArgs(ColorChangedEvent))
        End RaiseEvent
    End Event

    Public Shadows Custom Event Shown As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            Me.AddHandler(ShownEvent, value)
        End AddHandler
        RemoveHandler(value As RoutedEventHandler)
            Me.RemoveHandler(ShownEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)
            Me.RaiseEvent(New RoutedEventArgs(ShownEvent))
        End RaiseEvent
    End Event

    Public Shadows Custom Event Hid As RoutedEventHandler
        AddHandler(value As RoutedEventHandler)
            Me.AddHandler(HidEvent, value)
        End AddHandler
        RemoveHandler(value As RoutedEventHandler)
            Me.RemoveHandler(HidEvent, value)
        End RemoveHandler
        RaiseEvent(sender As Object, e As RoutedEventArgs)
            Me.RaiseEvent(New RoutedEventArgs(HidEvent))
        End RaiseEvent
    End Event

#End Region
#Region "Usercontrol"

    Private Sub UserControl_Loaded(sender As Object, e As RoutedEventArgs)
        mouseTrackerTimer.Start()
        If DimArea IsNot Nothing Then
            Color = DimArea.Color
        End If
        Show()
    End Sub

    Private Sub UserControl_Unloaded(sender As Object, e As RoutedEventArgs)
        Hide()
    End Sub

#End Region
#Region "Show / Hide"

    ''' <summary>Indicates whatever this instance is a preview, used by config window preview.</summary>
    Public Property IsPreview As Boolean

    Private timeSinceLastUndim As Date
    Private ignoreCursorMoveOnce As Boolean
    Private lastMousePosition As Point

    Private mouseTrackerTimer As New DispatcherTimer(TimeSpan.FromSeconds(0.5), DispatcherPriority.Normal, Sub()

                                                                                                               If DimArea IsNot Nothing AndAlso (DimArea.IsEnabled Or IsPreview) AndAlso Not IsVisible Then
                                                                                                                   If IsMouseOver Then

                                                                                                                       If DimArea.RedimWhenMouseStationary Then
                                                                                                                           QueueRedim()
                                                                                                                       End If

                                                                                                                   Else
                                                                                                                       Show()
                                                                                                                   End If
                                                                                                               End If

                                                                                                           End Sub, Me.Dispatcher)

    Private WithEvents RedimTimer As New DispatcherTimer

    Private Sub QueueRedim()

        If DimArea.RedimDelay.HasValue AndAlso Not DimArea.RedimDelay.Value.TotalSeconds = 0 And IsEnabled Then

            If lastMousePosition = Win32.Managed.CursorPosition AndAlso Not ScreenHasFullscreenApp(Bounds) Then
                If Not RedimTimer.IsEnabled Then
                    RedimTimer.Interval = DimArea.RedimDelay
                    RedimTimer.Start()
                End If
            Else
                RedimTimer.Stop()
                RedimTimer.Interval = DimArea.RedimDelay
                RedimTimer.Start()
            End If

        Else
            redimTimer.Stop()
        End If

        lastMousePosition = Win32.Managed.CursorPosition

    End Sub

    Private Sub _Redim() Handles RedimTimer.Tick
        RedimTimer.Stop()
        If IsEnabled And CanRedim AndAlso Not ScreenHasFullscreenApp(Bounds) Then
            Show()
            ignoreCursorMoveOnce = True
            HideCursor()
        End If
    End Sub

    Public Sub Show()
        If IsEnabled Then
            _IsVisible = True
            redimTimer.Stop()
            RaiseEvent Shown(Me, New RoutedEventArgs(ShownEvent))
        End If
    End Sub

    Public Sub Hide()
        If CanRedim Then
            CanRedim = False
            _IsVisible = False
            timeSinceLastUndim = Now
            lastMousePosition = Win32.Managed.CursorPosition
            RaiseEvent Hid(Me, New RoutedEventArgs(HidEvent))
        End If
    End Sub

    ''' <summary>Contains a value that specifies if the hide animation is currently running.</summary>
    Private CanRedim As Boolean = True
    Private Sub HideAnimation_Completed(sender As Object, e As EventArgs)
        CanRedim = True
    End Sub

    Private Sub UserControl_MouseMove(sender As Object, e As MouseEventArgs)

        If ignoreCursorMoveOnce Then
            ignoreCursorMoveOnce = False
            Exit Sub
        End If

        If DimArea IsNot Nothing AndAlso Not DimArea.IsRemovePending Then
            If DimArea.HideMouse Then
                If Not DimArea.HideMouseDelay = TimeSpan.Zero Then
                    If DimArea.UndimOption = UndimOption.MouseMove Then
                        Hide()
                    End If
                    ShowCursor()
                    QueueCursorHide()
                Else
                    HideCursor()
                End If
            Else
                ShowCursor()
            End If
        Else
            ShowCursor()
        End If

    End Sub

    Private Sub UserControl_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If DimArea IsNot Nothing Then
            If DimArea.UndimOption = UndimOption.MouseClick Then
                Hide()
            End If
        End If
    End Sub

#End Region
#Region "Show / hide Cursor"

    Public Sub QueueCursorHide()
        hidecursorTimer.Stop()

        If DimArea IsNot Nothing AndAlso Not DesignerProperties.GetIsInDesignMode(Me) Then
            If Not DimArea.HideMouseDelay = TimeSpan.Zero Then
                hidecursorTimer.Interval = DimArea.HideMouseDelay
                hidecursorTimer.Start()
            Else
                HideCursor()
            End If
        End If

    End Sub

    Private hidecursorTimer As New DispatcherTimer(TimeSpan.FromSeconds(4), DispatcherPriority.Normal, Sub()
                                                                                                           HideCursor()
                                                                                                           hidecursorTimer.Stop()
                                                                                                       End Sub, Me.Dispatcher)

    Public Sub HideCursor()
        If DimArea IsNot Nothing AndAlso DimArea.HideMouse Then
            Me.Cursor = Cursors.None
            _IsCursorVisible = False
        End If
    End Sub

    Public Sub ShowCursor()
        Me.Cursor = Cursors.Arrow
        _IsCursorVisible = True
    End Sub

#End Region

End Class