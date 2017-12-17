Imports System.ComponentModel
Imports System.Windows.Controls.Primitives

Public Class TabItemExtensions

    Public Shared ReadOnly SelectedCommand As DependencyProperty = CreateCommandExecutionEventBehaviour(Selector.SelectedEvent, "SelectedCommand", GetType(TabItem))

    Public Shared Sub SetSelectedCommand(ByVal o As TabItem, ByVal value As ICommand)
        o.SetValue(SelectedCommand, value)
    End Sub

    Public Shared Function GetSelectedCommand(ByVal o As TabItem) As ICommand
        Return TryCast(o.GetValue(SelectedCommand), ICommand)
    End Function

End Class

'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
''''
''''
'''' EventBehaviourFactory created by Samuel Jack
'''' http://blog.functionalfun.net/2008/09/hooking-up-commands-to-events-in-wpf.html
''''
''''
'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
'''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

Module EventBehaviourFactory

    Function CreateCommandExecutionEventBehaviour(ByVal routedEvent As RoutedEvent, ByVal propertyName As String, ByVal ownerType As Type) As DependencyProperty
        Dim [property] As DependencyProperty = DependencyProperty.RegisterAttached(propertyName, GetType(ICommand), ownerType, New PropertyMetadata(Nothing, AddressOf New ExecuteCommandOnRoutedEventBehaviour(routedEvent).PropertyChangedHandler))
        Return [property]
    End Function

    Private Class ExecuteCommandOnRoutedEventBehaviour
        Inherits ExecuteCommandBehaviour

        Private ReadOnly _routedEvent As RoutedEvent

        Public Sub New(ByVal routedEvent As RoutedEvent)
            _routedEvent = routedEvent
        End Sub

        Protected Overrides Sub AdjustEventHandlers(ByVal sender As DependencyObject, ByVal oldValue As Object, ByVal newValue As Object)
            Dim element As UIElement = TryCast(sender, UIElement)
            If element Is Nothing Then
                Return
            End If

            If oldValue IsNot Nothing Then
                element.[RemoveHandler](_routedEvent, New RoutedEventHandler(AddressOf EventHandler))
            End If

            If newValue IsNot Nothing Then
                element.[AddHandler](_routedEvent, New RoutedEventHandler(AddressOf EventHandler))
            End If
        End Sub

        Protected Sub EventHandler(ByVal sender As Object, ByVal e As RoutedEventArgs)
            HandleEvent(sender, e)
        End Sub

    End Class

    Friend MustInherit Class ExecuteCommandBehaviour

        Protected _property As DependencyProperty

        Protected MustOverride Sub AdjustEventHandlers(ByVal sender As DependencyObject, ByVal oldValue As Object, ByVal newValue As Object)

        Protected Sub HandleEvent(ByVal sender As Object, ByVal e As EventArgs)
            Dim dp As DependencyObject = TryCast(sender, DependencyObject)
            If dp Is Nothing Then
                Return
            End If

            Dim command As ICommand = TryCast(dp.GetValue(_property), ICommand)
            If command Is Nothing Then
                Return
            End If

            If DesignerProperties.GetIsInDesignMode(New FrameworkElement) Then
                Return
            End If

            If command.CanExecute(e) Then
                command.Execute(e)
            End If

        End Sub

        Public Sub PropertyChangedHandler(ByVal sender As DependencyObject, ByVal e As DependencyPropertyChangedEventArgs)
            If _property Is Nothing Then
                _property = e.[Property]
            End If

            Dim oldValue As Object = e.OldValue
            Dim newValue As Object = e.NewValue
            AdjustEventHandlers(sender, oldValue, newValue)
        End Sub

    End Class

End Module
