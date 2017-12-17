Imports System.IO
Imports System.IO.Compression

Public Class Setup
    Inherits DependencyObject

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property Current As New Setup

    Public Shared ReadOnly Property DefaultPath As String = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles) + "\Dimmer"

    Public Shared ReadOnly PathProperty As DependencyProperty = DependencyProperty.Register("Path", GetType(String), GetType(Setup), New PropertyMetadata(DefaultPath))
    Public Shared ReadOnly ProgressProperty As DependencyProperty = DependencyProperty.Register("Progress", GetType(Double), GetType(Setup), New PropertyMetadata(Nothing))
    Public Shared ReadOnly IsInstallingProperty As DependencyProperty = DependencyProperty.Register("IsInstalling", GetType(Boolean), GetType(Setup), New PropertyMetadata(Nothing))

    Public Property Path As String
        Get
            Return GetValue(PathProperty)
        End Get
        Set(ByVal value As String)
            SetValue(PathProperty, value)
        End Set
    End Property

    Public Property Progress As Double
        Get
            Return GetValue(ProgressProperty)
        End Get
        Set(ByVal value As Double)
            SetValue(ProgressProperty, value)
        End Set
    End Property

    Public Property IsInstalling As Boolean
        Get
            Return GetValue(IsInstallingProperty)
        End Get
        Set(ByVal value As Boolean)
            SetValue(IsInstallingProperty, value)
        End Set
    End Property

    Public ReadOnly Property MainExecutable As String
        Get
            Return Path + "\" + ExecutableName
        End Get
    End Property

    Private Const ExecutableName = "Dimmer.exe"
    Private Const chunkSize = 65536

    Public Async Sub Install()

        IsInstalling = True

        Dim path = Me.Path
        Await Task.Run(Sub()

                           CloseInstance()

                           SetProgress(0.1)

                           Dim tempDir = GetTempFolder()

                           WriteZipFile(tempDir)
                           SetProgress(0.25)

                           ExtractZipFile(tempDir)
                           SetProgress(0.5)

                           CopyFiles(tempDir, path)
                           Clean(tempDir)

                       End Sub)

        SetProgress(1)

        While Progress < 1
            Await Task.Delay(TimeSpan.FromSeconds(0.001))
        End While

        IsInstalling = False

    End Sub

    Private Sub CloseInstance()

        Dim procs = Process.GetProcessesByName("Dimmer")
        For Each proc In procs
            proc.Kill()
        Next

    End Sub

    Private Function GetTempFolder() As String

        Dim tempDir = My.Computer.FileSystem.SpecialDirectories.Temp + "\Dimmer"

        If Directory.Exists(tempDir) Then
            For Each file In Directory.GetFiles(tempDir)
                IO.File.Delete(file)
            Next
            For Each folder In Directory.GetDirectories(tempDir)
                Directory.Delete(folder, True)
            Next
        Else
            Directory.CreateDirectory(tempDir)
        End If

        Return tempDir

    End Function

    Private Sub WriteZipFile(tempDir As String)

        Using writer As New BinaryWriter(File.Open(tempDir + "\Contents.zip", FileMode.Create))
            writer.Write(My.Resources.Contents)
        End Using

    End Sub

    Private Sub ExtractZipFile(tempDir As String)
        ZipFile.ExtractToDirectory(tempDir + "\Contents.zip", tempDir + "\Contents")
    End Sub

    Private Sub CopyFiles(tempDir As String, path As String)

        If Directory.Exists(path) Then
            Directory.Delete(path, True)
        End If

        Directory.CreateDirectory(path)

        Dim files = Directory.GetFiles(tempDir + "\Contents", "*", SearchOption.AllDirectories)
        Dim i As Integer = 0
        For Each file In files

            Dim relname = file.Substring((tempDir + "\Contents").Length)
            Directory.CreateDirectory(Directory.GetParent(path + relname).FullName)
            IO.File.Copy(file, path + relname)

            i += 1
            SetProgress(0.5 + ((i / files.Count) * 0.45))

        Next

    End Sub

    Private Sub Clean(tempDir As String)
        If Directory.Exists(tempDir) Then
            Directory.Delete(tempDir, True)
        End If
    End Sub

    Private Async Sub SetProgress(progress As Double)
        Await Windows.Application.Current.Dispatcher.BeginInvoke(Sub() _SetProgress(progress))
    End Sub

    Private Async Sub _SetProgress(progress As Double)
        While Me.Progress < progress
            Me.Progress += 0.005
            Await Task.Delay(TimeSpan.FromSeconds(0.001))
        End While

        Me.Progress = progress
    End Sub

End Class

Public Class SetupExtension
    Inherits Markup.MarkupExtension

    Public Overrides Function ProvideValue(serviceProvider As IServiceProvider) As Object
        Return Setup.Current
    End Function

End Class