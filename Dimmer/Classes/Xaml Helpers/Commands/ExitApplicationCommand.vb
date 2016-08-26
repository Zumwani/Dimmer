Namespace Commands

    Public Class ExitApplicationCommand
        Implements ICommand

        Public Shared ReadOnly Instance As New ExitApplicationCommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            App.Current.Shutdown()
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

    End Class

End Namespace