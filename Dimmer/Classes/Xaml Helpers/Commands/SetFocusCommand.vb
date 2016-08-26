Namespace Commands

    Friend Class SetFocusCommand
        Implements ICommand

        Public Shared ReadOnly Instance As New SetFocusCommand

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

        Public Event CanExecuteChanged(sender As Object, e As EventArgs) Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute

            If parameter IsNot Nothing Then
                FocusManager.SetFocusedElement(parameter, parameter)
            End If

        End Sub

    End Class

End Namespace