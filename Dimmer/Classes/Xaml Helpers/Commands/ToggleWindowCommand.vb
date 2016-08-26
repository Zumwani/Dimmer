Namespace Commands

    Public Class ToggleWindowCommand
        Implements ICommand

        Public Shared ReadOnly Property Instance As ToggleWindowCommand
            Get
                Return New ToggleWindowCommand
            End Get
        End Property

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute

            Dim win As Window = parameter

            If win.IsVisible Then
                win.Hide()
            Else
                win.Show()
                win.Activate()
            End If

        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

    End Class

End Namespace