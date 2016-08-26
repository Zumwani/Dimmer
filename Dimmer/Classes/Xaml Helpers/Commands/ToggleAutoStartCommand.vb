Namespace Commands

    Public Class ToggleAutoStartCommand
        Inherits DependencyObject
        Implements ICommand

        Public Shared ReadOnly Instance As New ToggleAutoStartCommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            GetIsEnabled()
            IsEnabled = Not IsEnabled
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

        Sub New()
            Initialize()
        End Sub

        Public Sub Initialize()
            GetIsEnabled()
        End Sub

        Public Shared ReadOnly IsEnabledProperty As DependencyProperty = DependencyProperty.Register("IsEnabled", GetType(Boolean), GetType(ToggleAutoStartCommand), Nothing)

        Public Property IsEnabled As Boolean
            Get
                GetIsEnabled()
                Return Me.GetValue(IsEnabledProperty)
            End Get
            Set(value As Boolean)
                If Not value = Me.GetValue(IsEnabledProperty) Then
                    If value = True Then
                        SetEnabled()
                    Else
                        SetDisabled()
                    End If
                End If
            End Set
        End Property

        Private Sub GetIsEnabled()
            If IsEnabledProperty IsNot Nothing Then
                Dim val As String = My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True).GetValue(App.Name, "")
                Me.SetValue(IsEnabledProperty, val = ControlChars.Quote + App.Current.ExecutablePath + ControlChars.Quote)
            End If
        End Sub

        Private Sub SetEnabled()
            My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True).SetValue(App.Name, ControlChars.Quote + App.Current.ExecutablePath + ControlChars.Quote)
            GetIsEnabled()
        End Sub

        Private Sub SetDisabled()
            My.Computer.Registry.CurrentUser.OpenSubKey("SOFTWARE\Microsoft\Windows\CurrentVersion\Run", True).DeleteValue(App.Name)
            GetIsEnabled()
        End Sub

    End Class

End Namespace