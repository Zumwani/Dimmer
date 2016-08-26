Namespace Commands

    Public Class ToggleFavoriteDimAreaEnabledStateCommand
        Implements ICommand

        Public Shared ReadOnly Instance As New ToggleFavoriteDimAreaEnabledStateCommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute
            If App.Current.DimAreaManager.FavoriteDimArea IsNot Nothing Then
                Dim da As DimArea = App.Current.DimAreaManager.FavoriteDimArea
                da.IsEnabled = Not da.IsEnabled
            End If
        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

    End Class

End Namespace