Namespace Commands

    Public Class ToggleDimAreaIsEnabledStateCommand
        Implements ICommand

        Public Shared ReadOnly Instance As New ToggleDimAreaIsEnabledStateCommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If parameter IsNot Nothing Then
                Dim da As DimArea = parameter
                da.IsEnabled = Not da.IsEnabled
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

    End Class

End Namespace