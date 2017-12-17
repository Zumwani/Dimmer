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

    Private Const Name = "Dimmer"
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

                           AddToStartmenu(path + "\" + ExecutableName)
                           AddToWindowsInstaller(path + "\" + ExecutableName)

                       End Sub)

        SetProgress(1)

        While Progress < 1
            Await Task.Delay(TimeSpan.FromSeconds(0.001))
        End While

        IsInstalling = False

    End Sub

    Public Async Sub Uninstall()


        IsInstalling = True

        Dim path = GetInstalledPath(Application.InstalledGUID)
        Await Task.Run(Async Function()

                           CloseInstance()

                           SetProgress(0.1)

                           Await Task.Delay(TimeSpan.FromSeconds(0.1))

                           DeleteFolder(path)
                           SetProgress(0.8)

                           RemoveFromStartMenu()
                           RemoveFromWindowsInstaller(Application.InstalledGUID)

                       End Function)

        SetProgress(1)
        ScheduleRemoveSelf()

        While Progress < 1
            Await Task.Delay(TimeSpan.FromSeconds(0.001))
        End While

        IsInstalling = False
        Progress = 1

    End Sub

    Private Sub CloseInstance()

        Dim procs = Process.GetProcessesByName(Name)
        For Each proc In procs
            proc.Kill()
        Next

    End Sub

#Region "Install"

    Private Function GetTempFolder() As String

        Dim tempDir = My.Computer.FileSystem.SpecialDirectories.Temp + "\" + Name

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

    Private Sub AddToStartmenu(executable As String)

        Dim folder = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs"
        Dim shortcut = """" + folder + "\" + Name + """"
        Dim target = """" + executable + """"

        Dim command = "/c mklink " + shortcut + " " + target

        Dim info As New ProcessStartInfo("cmd", command) With {.CreateNoWindow = True, .UseShellExecute = False}
        Process.Start(info)

    End Sub

    Private Sub AddToWindowsInstaller(executable As String)

        Dim path = "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"

        Dim guid = "{" + GetGUIDFromAssembly(executable).ToUpper() + "}"

        Dim fvi = FileVersionInfo.GetVersionInfo(executable)
        Dim displayName As String = fvi.ProductName
        Dim version = fvi.ProductVersion
        Dim publisher = fvi.CompanyName
        Dim installLocation = Directory.GetParent(executable)
        Dim icon = executable + ",0"
        Dim bytesLength = Directory.GetParent(executable).EnumerateFiles("*", SearchOption.AllDirectories).Sum(Function(f) f.Length)
        Dim size = bytesLength / 1024
        Dim uninstallString = GetUninstaller(guid)
        Dim installDate = Now.Date.ToShortDateString()

        Using key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path, True)
            Using subkey = key.CreateSubKey(guid, True)

                subkey.SetValue("DisplayIcon", icon)
                subkey.SetValue("DisplayName", displayName)
                subkey.SetValue("DisplayVersion", version)
                subkey.SetValue("Publisher", publisher)
                subkey.SetValue("InstallLocation", installLocation)
                subkey.SetValue("UninstallString", uninstallString)
                subkey.SetValue("EstimatedSize", size, Microsoft.Win32.RegistryValueKind.DWord)
                subkey.SetValue("NoModify", 1, Microsoft.Win32.RegistryValueKind.DWord)
                subkey.SetValue("NoRepair", 1, Microsoft.Win32.RegistryValueKind.DWord)
                subkey.SetValue("InstallDate", installDate)

            End Using
        End Using

    End Sub

    Private Function GetGUIDFromAssembly(path As String) As String
        Dim bytes = File.ReadAllBytes(path)
        Dim assembly = Reflection.Assembly.ReflectionOnlyLoad(bytes)
        Dim guid = Runtime.InteropServices.Marshal.GetTypeLibGuidForAssembly(assembly)
        Return guid.ToString()
    End Function

    Private Function GetUninstaller(guid As String) As String

        Dim path = "C:\ProgramData\Setup " + Name + ".exe"

        Dim currentPath = [GetType]().Assembly.Location
        File.Copy(currentPath, path, True)

        Return path + " -uninstall:" + guid

    End Function

#End Region
#Region "Uninstall"

    Private Function GetInstalledPath(guid As String) As String

        If guid = "" Then
            Return ""
        End If

        Dim path = "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"

        Try

            Using key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path)
                Using subkey = key.OpenSubKey(guid)

                    Dim installPath = subkey.GetValue("InstallLocation", "")
                    Return installPath

                End Using
            End Using

        Catch ex As Security.SecurityException
        Catch ex2 As NullReferenceException
        End Try

        Return ""

    End Function

    Private Sub DeleteFolder(path As String)
        If Directory.Exists(path) Then
            Directory.Delete(path, True)
        End If
    End Sub

    Private Sub RemoveFromStartMenu()
        Dim folder = "C:\ProgramData\Microsoft\Windows\Start Menu\Programs"
        Dim shortcut = folder + "\" + Name
        If File.Exists(shortcut) Then
            File.Delete(shortcut)
        End If
    End Sub

    Private Sub RemoveFromWindowsInstaller(guid As String)

        Dim path = "SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"

        Using key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(path, True)
            key.DeleteSubKey(guid, False)
        End Using

    End Sub

    Private Sub ScheduleRemoveSelf()
        Dim Info As New ProcessStartInfo With {
            .Arguments = "/C choice /C Y /N /D Y /T 3 & Del """ + [GetType]().Assembly.Location + """",
            .WindowStyle = ProcessWindowStyle.Hidden,
            .CreateNoWindow = True,
            .FileName = "cmd"
        }
        Process.Start(Info)
    End Sub

#End Region

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