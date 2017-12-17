Class Application

    Public Shared ReadOnly Property IsInstalling As Boolean = True
    Public Shared ReadOnly Property IsUninstalling As Boolean
    Public Shared ReadOnly Property InstalledGUID As String

    Protected Overrides Sub OnStartup(e As StartupEventArgs)

        For Each arg In e.Args
            If arg.StartsWith("-uninstall:") Then

                _IsInstalling = False
                _IsUninstalling = True

                Dim guid = arg.Substring("-uninstall:".Length)

                If Not System.Guid.TryParse(guid, New Guid) Then
                    MessageBox.Show("-uninstall was passed as an argument, but the specified guid was invalid. The setup will now close.")
                    Shutdown()
                End If

                _InstalledGUID = guid

            End If
        Next

    End Sub

End Class