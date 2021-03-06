﻿Namespace Commands

    Public Class CloseSetupAndOpenAppCommand
        Inherits Markup.MarkupExtension
        Implements ICommand

        Public Event CanExecuteChanged As EventHandler Implements ICommand.CanExecuteChanged

        Public Sub Execute(parameter As Object) Implements ICommand.Execute

            Dim path = Setup.Current.Path + "\Dimmer.exe"
            If IO.File.Exists(path) Then
                Process.Start(path, "-config")
            End If

            Windows.Application.Current.Shutdown()

        End Sub

        Public Function CanExecute(parameter As Object) As Boolean Implements ICommand.CanExecute

            'Dim isSelected = TryCast(TryCast(parameter, RoutedEventArgs)?.OriginalSource, TabItem)?.IsSelected
            'Dim isInstalled = (IO.File.Exists(Setup.Current.MainExecutable))

            'Return (isSelected And isInstalled)
            Return True
        End Function

        Public Overrides Function ProvideValue(serviceProvider As IServiceProvider) As Object
            Return Me
        End Function

    End Class

End Namespace