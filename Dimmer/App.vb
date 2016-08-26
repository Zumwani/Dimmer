Imports Zumwani.Dimmer.MS.Shell
Imports Hardcodet.Wpf.TaskbarNotification
Imports System.IO

Public Class App
    Inherits Windows.Application
    Implements ISingleInstanceApp
    'TODO: Settings not saving (only occurs when closing app?)
#Region "Properties"

    ''' <summary>The name of this app.</summary>
    Public Shared ReadOnly Property Name As String = "Dimmer"

    ''' <summary>The executable path of this app.</summary>
    Public ReadOnly Property ExecutablePath As String = System.Reflection.Assembly.GetExecutingAssembly().Location

    ''' <summary>Gets the instance of the current app.</summary>
    Public Overloads Shared ReadOnly Property Current As App

    ''' <summary>Gets the notify icon.</summary>
    Public ReadOnly Property NotifyIcon As TaskbarIcon

    ''' <summary>Gets the dim area manager.</summary>
    Public ReadOnly Property DimAreaManager As New DimAreaManager

#End Region
#Region "App"

    ''' <summary>The token that is used to keep track of multiple application instances and make sure there is only one.</summary>
    Private Const SingleInstanceToken As String = "a8b47fd0-c54b-48e8-a11e-13413e93b11a"

    ''' <summary>The application entry point.</summary>
    Public Shared Sub Main(Args As String())

        'Custom single instance system is needed since application structure is: Notify icon > Main Window
        If SingleInstance(Of App).InitializeAsFirstInstance(SingleInstanceToken) Then

            With New App
                .Initialize(Args)
                .Run()
                .Deinitialize()
            End With

            'Allow single instance code to perform cleanup operations
            SingleInstance(Of App).Cleanup()

        End If

    End Sub

    ''' <summary>This function is called when a second application instance is found.</summary>
    ''' <param name="args">The command line args that was used on when launching the second instance.</param>
    Private Function SignalExternalCommandLineArgs(args As IList(Of String)) As Boolean Implements ISingleInstanceApp.SignalExternalCommandLineArgs
        ProcessCommandlineArguments(args.ToArray)
        MainWindow.Show()
        MainWindow.Activate()
        Return True
    End Function

#End Region
#Region "Initialize"

    ''' <summary>Gets if this is the first time the application is launched.</summary>
    Public ReadOnly Property IsFirstStartup As Boolean

    ''' <summary>Initializes the application.</summary>
    Private Sub Initialize(Args As String())

        _Current = Me

        MainWindow = New ConfigWindow
        Me.ShutdownMode = ShutdownMode.OnExplicitShutdown

        App.Current._IsFirstStartup = Not Directory.Exists(DataPathString) 'Must be before loading dim areas

        DimAreaManager.LoadAll(DimAreaStore)
        InitializeNotifyIcon()
        ProcessCommandlineArguments(Args)

        If IsFirstStartup Then

            'Set app to auto start by default
            Commands.ToggleAutoStartCommand.Instance.IsEnabled = True

            'Show config window this time since user might not know that the app is notify icon based. A notification will tell this to the user when window is closed.
            MainWindow.Show()
            MainWindow.Activate()

        End If

    End Sub

    ''' <summary>Initializes the notify icon.</summary>
    Private Sub InitializeNotifyIcon()

        _NotifyIcon = New TaskbarIcon

        With _NotifyIcon

            .ToolTipText = App.Name

            .DoubleClickCommand = Commands.ToggleWindowCommand.Instance
            .DoubleClickCommandParameter = MainWindow

            .LeftClickCommand = Commands.ToggleFavoriteDimAreaEnabledStateCommand.Instance

            .ContextMenu = New ContextMenu
            .ContextMenu.FontSize = 14

            .ContextMenu.Items.Add(New MenuItem() _
                                   With {
                                       .Header = "Configure dimareas",
                                       .Command = Commands.ToggleWindowCommand.Instance,
                                       .CommandParameter = MainWindow
                                        })

            .ContextMenu.Items.Add(New Separator)

            With CType(.ContextMenu.Items(.ContextMenu.Items.Add(New MenuItem())), MenuItem)
                .Header = "Start automatically at boot"
                .Command = Commands.ToggleAutoStartCommand.Instance
                .SetBinding(MenuItem.IsCheckedProperty, New Binding() With {.Source = Commands.ToggleAutoStartCommand.Instance, .Path = New PropertyPath("IsEnabled"), .Mode = BindingMode.TwoWay})
                Commands.ToggleAutoStartCommand.Instance.Initialize()
            End With

            .ContextMenu.Items.Add(New Separator)

            .ContextMenu.Items.Add(New MenuItem() _
                                   With {
                                       .Header = "Exit Dimmer",
                                       .Command = Commands.ExitApplicationCommand.Instance
                                   })


            .Visibility = Visibility.Visible


        End With

        UpdateNotifyIcon()

    End Sub

    ''' <summary>Updates the icon on the notify icon.</summary>
    Public Sub UpdateNotifyIcon()
        If NotifyIcon IsNot Nothing Then
            If DimAreaManager.FavoriteDimArea IsNot Nothing AndAlso DimAreaManager.FavoriteDimArea.IsEnabled Then
                NotifyIcon.Icon = My.Resources.icon_favorite_dimmed
            Else
                NotifyIcon.Icon = My.Resources.icon
            End If
        End If
    End Sub

    ''' <summary>The command line argument used to open the config window.</summary>
    Private Const ConfigArgument As String = "-config"
    ''' <summary>The command line argument to disable all dim areas.</summary>
    Private Const DisableAllArgument As String = "-disableall"

    ''' <summary>Processes the command line arguments.</summary>
    Private Sub ProcessCommandlineArguments(Args As String())

        For Each arg In Args
            Select Case arg.ToLower

                Case Is = ConfigArgument
                    MainWindow.Show()

                Case Is = DisableAllArgument
                    DimAreaManager.HideAll()

            End Select
        Next

    End Sub

    ''' <summary>De initializes the application.</summary>
    Private Sub Deinitialize()
        NotifyIcon.Dispose()
        DimAreaManager.SaveAll()
    End Sub

#End Region

End Class
