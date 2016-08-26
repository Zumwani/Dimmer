Imports System.IO
Imports System.Runtime.Remoting
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting.Channels.Ipc
Imports System.Runtime.Serialization.Formatters
Imports System.Threading
Imports System.Windows.Threading
Imports System.Security
Imports System.Runtime.InteropServices
Imports System.ComponentModel

'-----------------------------------------------------------------------
' <copyright file="SingleInstance.cs" company="Microsoft">
'     Copyright (c) Microsoft Corporation.  All rights reserved.
' </copyright>
' <summary>
'     This class checks to make sure that only one instance of 
'     this application is running at a time.
' </summary>
'-----------------------------------------------------------------------

'http://www.codeproject.com/Articles/84270/WPF-Single-Instance-Application

Namespace MS.Shell

    Friend Enum WM
        NULL = &H0
        CREATE = &H1
        DESTROY = &H2
        MOVE = &H3
        SIZE = &H5
        ACTIVATE = &H6
        SETFOCUS = &H7
        KILLFOCUS = &H8
        ENABLE = &HA
        SETREDRAW = &HB
        SETTEXT = &HC
        GETTEXT = &HD
        GETTEXTLENGTH = &HE
        PAINT = &HF
        CLOSE = &H10
        QUERYENDSESSION = &H11
        QUIT = &H12
        QUERYOPEN = &H13
        ERASEBKGND = &H14
        SYSCOLORCHANGE = &H15
        SHOWWINDOW = &H18
        ACTIVATEAPP = &H1C
        SETCURSOR = &H20
        MOUSEACTIVATE = &H21
        CHILDACTIVATE = &H22
        QUEUESYNC = &H23
        GETMINMAXINFO = &H24

        WINDOWPOSCHANGING = &H46
        WINDOWPOSCHANGED = &H47

        CONTEXTMENU = &H7B
        STYLECHANGING = &H7C
        STYLECHANGED = &H7D
        DISPLAYCHANGE = &H7E
        GETICON = &H7F
        SETICON = &H80
        NCCREATE = &H81
        NCDESTROY = &H82
        NCCALCSIZE = &H83
        NCHITTEST = &H84
        NCPAINT = &H85
        NCACTIVATE = &H86
        GETDLGCODE = &H87
        SYNCPAINT = &H88
        NCMOUSEMOVE = &HA0
        NCLBUTTONDOWN = &HA1
        NCLBUTTONUP = &HA2
        NCLBUTTONDBLCLK = &HA3
        NCRBUTTONDOWN = &HA4
        NCRBUTTONUP = &HA5
        NCRBUTTONDBLCLK = &HA6
        NCMBUTTONDOWN = &HA7
        NCMBUTTONUP = &HA8
        NCMBUTTONDBLCLK = &HA9

        SYSKEYDOWN = &H104
        SYSKEYUP = &H105
        SYSCHAR = &H106
        SYSDEADCHAR = &H107
        COMMAND = &H111
        SYSCOMMAND = &H112

        MOUSEMOVE = &H200
        LBUTTONDOWN = &H201
        LBUTTONUP = &H202
        LBUTTONDBLCLK = &H203
        RBUTTONDOWN = &H204
        RBUTTONUP = &H205
        RBUTTONDBLCLK = &H206
        MBUTTONDOWN = &H207
        MBUTTONUP = &H208
        MBUTTONDBLCLK = &H209
        MOUSEWHEEL = &H20A
        XBUTTONDOWN = &H20B
        XBUTTONUP = &H20C
        XBUTTONDBLCLK = &H20D
        MOUSEHWHEEL = &H20E


        CAPTURECHANGED = &H215

        ENTERSIZEMOVE = &H231
        EXITSIZEMOVE = &H232

        IME_SETCONTEXT = &H281
        IME_NOTIFY = &H282
        IME_CONTROL = &H283
        IME_COMPOSITIONFULL = &H284
        IME_SELECT = &H285
        IME_CHAR = &H286
        IME_REQUEST = &H288
        IME_KEYDOWN = &H290
        IME_KEYUP = &H291

        NCMOUSELEAVE = &H2A2

        DWMCOMPOSITIONCHANGED = &H31E
        DWMNCRENDERINGCHANGED = &H31F
        DWMCOLORIZATIONCOLORCHANGED = &H320
        DWMWINDOWMAXIMIZEDCHANGE = &H321

#Region "Windows 7"
        DWMSENDICONICTHUMBNAIL = &H323
        DWMSENDICONICLIVEPREVIEWBITMAP = &H326
#End Region

        USER = &H400

        ' This is the hard-coded message value used by WinForms for Shell_NotifyIcon.
        ' It's relatively safe to reuse.
        TRAYMOUSEMESSAGE = &H800
        'WM_USER + 1024
        APP = &H8000
    End Enum

    <SuppressUnmanagedCodeSecurity>
    Friend NotInheritable Class NativeMethods
        Private Sub New()
        End Sub
        ''' <summary>
        ''' Delegate declaration that matches WndProc signatures.
        ''' </summary>
        Public Delegate Function MessageHandler(uMsg As WM, wParam As IntPtr, lParam As IntPtr, ByRef handled As Boolean) As IntPtr

        <DllImport("shell32.dll", EntryPoint:="CommandLineToArgvW", CharSet:=CharSet.Unicode)>
        Private Shared Function _CommandLineToArgvW(<MarshalAs(UnmanagedType.LPWStr)> cmdLine As String, ByRef numArgs As Integer) As IntPtr
        End Function


        <DllImport("kernel32.dll", EntryPoint:="LocalFree", SetLastError:=True)>
        Private Shared Function _LocalFree(hMem As IntPtr) As IntPtr
        End Function


        Public Shared Function CommandLineToArgvW(cmdLine As String) As String()
            Dim argv As IntPtr = IntPtr.Zero
            Try
                Dim numArgs As Integer = 0

                argv = _CommandLineToArgvW(cmdLine, numArgs)
                If argv = IntPtr.Zero Then
                    Throw New Win32Exception()
                End If
                Dim result = New String(numArgs - 1) {}

                For i As Integer = 0 To numArgs - 1
                    Dim currArg As IntPtr = Marshal.ReadIntPtr(argv, i * Marshal.SizeOf(GetType(IntPtr)))
                    result(i) = Marshal.PtrToStringUni(currArg)
                Next

                Return result
            Finally

                ' Otherwise LocalFree failed.
                ' Assert.AreEqual(IntPtr.Zero, p);
                Dim p As IntPtr = _LocalFree(argv)
            End Try
        End Function

    End Class

    Friend Interface ISingleInstanceApp
        Function SignalExternalCommandLineArgs(args As IList(Of String)) As Boolean
    End Interface

    ''' <summary>
    ''' This class checks to make sure that only one instance of 
    ''' this application is running at a time.
    ''' </summary>
    ''' <remarks>
    ''' Note: this class should be used with some caution, because it does no
    ''' security checking. For example, if one instance of an app that uses this class
    ''' is running as Administrator, any other instance, even if it is not
    ''' running as Administrator, can activate it with command line arguments.
    ''' For most apps, this will not be much of an issue.
    ''' </remarks>
    Friend NotInheritable Class SingleInstance(Of TApplication As {System.Windows.Application, ISingleInstanceApp})
        Private Sub New()
        End Sub

#Region "Private Fields"

        ''' <summary>
        ''' String delimiter used in channel names.
        ''' </summary>
        Private Const Delimiter As String = ":"

        ''' <summary>
        ''' Suffix to the channel name.
        ''' </summary>
        Private Const ChannelNameSuffix As String = "SingeInstanceIPCChannel"

        ''' <summary>
        ''' Remote service name.
        ''' </summary>
        Private Const RemoteServiceName As String = "SingleInstanceApplicationService"

        ''' <summary>
        ''' IPC protocol used (string).
        ''' </summary>
        Private Const IpcProtocol As String = "ipc://"

        ''' <summary>
        ''' Application mutex.
        ''' </summary>
        Private Shared singleInstanceMutex As Mutex

        ''' <summary>
        ''' IPC channel for communications.
        ''' </summary>
        Private Shared channel As IpcServerChannel

        ''' <summary>
        ''' List of command line arguments for the application.
        ''' </summary>
        Private Shared m_commandLineArgs As IList(Of String)

#End Region

#Region "Public Properties"

        ''' <summary>
        ''' Gets list of command line arguments for the application.
        ''' </summary>
        Public Shared ReadOnly Property CommandLineArgs() As IList(Of String)
            Get
                Return m_commandLineArgs
            End Get
        End Property

#End Region

#Region "Public Methods"

        ''' <summary>
        ''' Checks if the instance of the application attempting to start is the first instance. 
        ''' If not, activates the first instance.
        ''' </summary>
        ''' <returns>True if this is the first instance of the application.</returns>
        Public Shared Function InitializeAsFirstInstance(uniqueName As String) As Boolean
            m_commandLineArgs = GetCommandLineArgs(uniqueName)

            ' Build unique application Id and the IPC channel name.
            Dim applicationIdentifier As String = uniqueName + Environment.UserName

            Dim channelName As String = [String].Concat(applicationIdentifier, Delimiter, ChannelNameSuffix)

            ' Create mutex based on unique application Id to check if this is the first instance of the application. 
            Dim firstInstance As Boolean
            singleInstanceMutex = New Mutex(True, applicationIdentifier, firstInstance)
            If firstInstance Then
                CreateRemoteService(channelName)
            Else
                SignalFirstInstance(channelName, m_commandLineArgs)
            End If

            Return firstInstance
        End Function

        ''' <summary>
        ''' Cleans up single-instance code, clearing shared resources, mutexes, etc.
        ''' </summary>
        Public Shared Sub Cleanup()
            If singleInstanceMutex IsNot Nothing Then
                singleInstanceMutex.Close()
                singleInstanceMutex = Nothing
            End If

            If channel IsNot Nothing Then
                ChannelServices.UnregisterChannel(channel)
                channel = Nothing
            End If
        End Sub

#End Region

#Region "Private Methods"

        ''' <summary>
        ''' Gets command line args - for ClickOnce deployed applications, command line args may not be passed directly, they have to be retrieved.
        ''' </summary>
        ''' <returns>List of command line arg strings.</returns>
        Private Shared Function GetCommandLineArgs(uniqueApplicationName As String) As IList(Of String)
            Dim args As String() = Nothing
            If AppDomain.CurrentDomain.ActivationContext Is Nothing Then
                ' The application was not clickonce deployed, get args from standard API's
                args = Environment.GetCommandLineArgs()
            Else
                ' The application was clickonce deployed
                ' Clickonce deployed apps cannot recieve traditional commandline arguments
                ' As a workaround commandline arguments can be written to a shared location before 
                ' the app is launched and the app can obtain its commandline arguments from the 
                ' shared location               
                Dim appFolderPath As String = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), uniqueApplicationName)

                Dim cmdLinePath As String = Path.Combine(appFolderPath, "cmdline.txt")
                If File.Exists(cmdLinePath) Then
                    Try
                        Using reader As TextReader = New StreamReader(cmdLinePath, System.Text.Encoding.Unicode)
                            args = NativeMethods.CommandLineToArgvW(reader.ReadToEnd())
                        End Using

                        File.Delete(cmdLinePath)
                    Catch generatedExceptionName As IOException
                    End Try
                End If
            End If

            If args Is Nothing Then
                args = New String() {}
            End If

            Return New List(Of String)(args)
        End Function

        ''' <summary>
        ''' Creates a remote service for communication.
        ''' </summary>
        ''' <param name="channelName">Application's IPC channel name.</param>
        Private Shared Sub CreateRemoteService(channelName As String)
            Dim serverProvider As New BinaryServerFormatterSinkProvider()
            serverProvider.TypeFilterLevel = TypeFilterLevel.Full
            Dim props As IDictionary = New Dictionary(Of String, String)()

            props("name") = channelName
            props("portName") = channelName
            props("exclusiveAddressUse") = "false"

            ' Create the IPC Server channel with the channel properties
            channel = New IpcServerChannel(props, serverProvider)

            ' Register the channel with the channel services
            ChannelServices.RegisterChannel(channel, True)

            ' Expose the remote service with the REMOTE_SERVICE_NAME
            Dim remoteService As New IPCRemoteService()
            RemotingServices.Marshal(remoteService, RemoteServiceName)
        End Sub

        ''' <summary>
        ''' Creates a client channel and obtains a reference to the remoting service exposed by the server - 
        ''' in this case, the remoting service exposed by the first instance. Calls a function of the remoting service 
        ''' class to pass on command line arguments from the second instance to the first and cause it to activate itself.
        ''' </summary>
        ''' <param name="channelName">Application's IPC channel name.</param>
        ''' <param name="args">
        ''' Command line arguments for the second instance, passed to the first instance to take appropriate action.
        ''' </param>
        Private Shared Sub SignalFirstInstance(channelName As String, args As IList(Of String))
            Dim secondInstanceChannel As New IpcClientChannel()
            ChannelServices.RegisterChannel(secondInstanceChannel, True)

            Dim remotingServiceUrl As String = Convert.ToString((IpcProtocol & channelName) + "/") & RemoteServiceName

            ' Obtain a reference to the remoting service exposed by the server i.e the first instance of the application
            Dim firstInstanceRemoteServiceReference As IPCRemoteService = DirectCast(RemotingServices.Connect(GetType(IPCRemoteService), remotingServiceUrl), IPCRemoteService)

            ' Check that the remote service exists, in some cases the first instance may not yet have created one, in which case
            ' the second instance should just exit
            If firstInstanceRemoteServiceReference IsNot Nothing Then
                ' Invoke a method of the remote service exposed by the first instance passing on the command line
                ' arguments and causing the first instance to activate itself
                firstInstanceRemoteServiceReference.InvokeFirstInstance(args)
            End If
        End Sub

        ''' <summary>
        ''' Callback for activating first instance of the application.
        ''' </summary>
        ''' <param name="arg">Callback argument.</param>
        ''' <returns>Always null.</returns>
        Private Shared Function ActivateFirstInstanceCallback(arg As Object) As Object
            ' Get command line args to be passed to first instance
            Dim args As IList(Of String) = TryCast(arg, IList(Of String))
            ActivateFirstInstance(args)
            Return Nothing
        End Function

        ''' <summary>
        ''' Activates the first instance of the application with arguments from a second instance.
        ''' </summary>
        ''' <param name="args">List of arguments to supply the first instance of the application.</param>
        Private Shared Sub ActivateFirstInstance(args As IList(Of String))
            ' Set main window state and process command line args
            If Application.Current Is Nothing Then
                Return
            End If

            DirectCast(Application.Current, TApplication).SignalExternalCommandLineArgs(args)
        End Sub

#End Region

#Region "Private Classes"

        ''' <summary>
        ''' Remoting service class which is exposed by the server i.e the first instance and called by the second instance
        ''' to pass on the command line arguments to the first instance and cause it to activate itself.
        ''' </summary>
        Private Class IPCRemoteService
            Inherits MarshalByRefObject
            ''' <summary>
            ''' Activates the first instance of the application.
            ''' </summary>
            ''' <param name="args">List of arguments to pass to the first instance.</param>
            Public Sub InvokeFirstInstance(args As IList(Of String))
                If Application.Current IsNot Nothing Then
                    ' Do an asynchronous call to ActivateFirstInstance function
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, New DispatcherOperationCallback(AddressOf SingleInstance(Of TApplication).ActivateFirstInstanceCallback), args)
                End If
            End Sub

            ''' <summary>
            ''' Remoting Object's ease expires after every 5 minutes by default. We need to override the InitializeLifetimeService class
            ''' to ensure that lease never expires.
            ''' </summary>
            ''' <returns>Always null.</returns>
            Public Overrides Function InitializeLifetimeService() As Object
                Return Nothing
            End Function
        End Class

#End Region

    End Class
End Namespace