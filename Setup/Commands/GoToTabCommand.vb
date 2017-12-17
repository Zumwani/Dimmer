Namespace Commands

    Public Class GoToTabCommand
        Inherits Markup.MarkupExtension
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Async Sub Execute(parameter As Object) Implements ICommand.Execute

            Await Task.Delay(TimeSpan.FromSeconds(0.1))

            If Setup.Current.IsInstalling Then
                Return
            End If

            Dim tab As TabItem = parameter
            If tab IsNot Nothing Then
                tab.IsSelected = True
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