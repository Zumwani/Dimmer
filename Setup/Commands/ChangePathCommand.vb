Imports System.Windows.Forms

Namespace Commands

    Public Class ChangePathCommand
        Inherits Markup.MarkupExtension
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute

            Dim dialog As New FolderBrowserDialog With
                {
                .SelectedPath = Setup.Current.Path,
                .ShowNewFolderButton = True
                }

            If dialog.ShowDialog() = DialogResult.OK Then
                If dialog.SelectedPath.EndsWith("Dimmer") Then
                    Setup.Current.Path = dialog.SelectedPath
                Else
                    Setup.Current.Path = dialog.SelectedPath + "\Dimmer"
                End If
            End If

        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute
            Return True
        End Function

        Public Overrides Function ProvideValue(serviceProvider As IServiceProvider) As Object
            Return Me
        End Function

    End Class

End Namespace