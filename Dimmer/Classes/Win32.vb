Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Namespace Win32

    ''' <summary>Contains wrapper methods for the unmanaged PInvoke methods.</summary>
    Friend NotInheritable Class Managed

        Private Sub New()
        End Sub

        ''' <summary>Gets the cursor position on the screen.</summary>
        Public Shared ReadOnly Property CursorPosition As Point
            Get
                Dim p As Unmanaged.Win32Point = Nothing
                Unmanaged.GetCursorPos(p)
                Return New Point(p.X, p.Y)
            End Get
        End Property

        ''' <summary>Hide a window from alt-tab.</summary>
        Public Shared Sub HideWindowFromAltTab(Window As Window)

            Dim wndHelper As New WindowInteropHelper(Window)

            Dim exStyle As Integer = Unmanaged.GetWindowLong(wndHelper.Handle, Unmanaged.GetWindowLongFields.GWL_EXSTYLE)
            exStyle = exStyle Or Unmanaged.ExtendedWindowStyles.WS_EX_TOOLWINDOW
            Unmanaged.SetWindowLong(wndHelper.Handle, Unmanaged.GetWindowLongFields.GWL_EXSTYLE, exStyle)

        End Sub

    End Class

    ''' <summary>Contains unmanaged PInvoke methods.</summary>
    Friend NotInheritable Class Unmanaged

        Private Sub New()
        End Sub

        <DllImport("user32.dll")>
        Friend Shared Function GetCursorPos(ByRef pt As Win32Point) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <StructLayout(LayoutKind.Sequential)>
        Friend Structure Win32Point
            Public X As Int32
            Public Y As Int32
        End Structure

        <Flags>
        Public Enum ExtendedWindowStyles
            WS_EX_TOOLWINDOW = &H80
        End Enum
        Public Enum GetWindowLongFields
            GWL_EXSTYLE = (-20)
        End Enum

        <DllImport("user32.dll")>
        Public Shared Function GetWindowLong(hWnd As IntPtr, nIndex As Integer) As IntPtr
        End Function

        Public Shared Function SetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
            Dim [error] As Integer = 0
            Dim result As IntPtr = IntPtr.Zero
            ' Win32 SetWindowLong doesn't clear error on success
            SetLastError(0)

            If IntPtr.Size = 4 Then
                ' use SetWindowLong
                Dim tempResult As Int32 = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong))
                [error] = Marshal.GetLastWin32Error()
                result = New IntPtr(tempResult)
            Else
                ' use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong)
                [error] = Marshal.GetLastWin32Error()
            End If

            If (result = IntPtr.Zero) AndAlso ([error] <> 0) Then
                Throw New System.ComponentModel.Win32Exception([error])
            End If

            Return result
        End Function

        <DllImport("user32.dll", EntryPoint:="SetWindowLongPtr", SetLastError:=True)>
        Public Shared Function IntSetWindowLongPtr(hWnd As IntPtr, nIndex As Integer, dwNewLong As IntPtr) As IntPtr
        End Function

        <DllImport("user32.dll", EntryPoint:="SetWindowLong", SetLastError:=True)>
        Public Shared Function IntSetWindowLong(hWnd As IntPtr, nIndex As Integer, dwNewLong As Int32) As Int32
        End Function

        Public Shared Function IntPtrToInt32(intPtr As IntPtr) As Integer
            Return CInt(intPtr.ToInt64())
        End Function

        <DllImport("kernel32.dll", EntryPoint:="SetLastError")>
        Public Shared Sub SetLastError(dwErrorCode As Integer)
        End Sub

        <DllImport("user32.dll")>
        Public Shared Function WindowFromPoint(ByVal xPoint As Integer, ByVal yPoint As Integer) As IntPtr
        End Function

    End Class
End Namespace
