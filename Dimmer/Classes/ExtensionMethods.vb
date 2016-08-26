Imports System.IO
Imports System.Runtime.CompilerServices

Module ExtensionMethods

    Public DataPathString As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\" + App.Name

    ''' <summary>Gets the path to the data directory in AppData.</summary>
    Public ReadOnly Property DataPath As DirectoryInfo
        Get
            Return Directory.CreateDirectory(DataPathString)
        End Get
    End Property

    ''' <summary>Gets the path to the dim area store in AppData.</summary>
    Public ReadOnly Property DimAreaStore As DirectoryInfo
        Get
            Return Directory.CreateDirectory(DataPath.FullName + "\DimAreaStore")
        End Get
    End Property

    ''' <summary>Checks if the specified file name exists in the directory.</summary>
    <Extension>
    Public Function Contains(DirectoryInfo As DirectoryInfo, Name As String, Optional CaseSensetive As Boolean = True) As Boolean
        For Each File In DirectoryInfo.GetFiles
            If CaseSensetive Then
                If File.Name = Name Then
                    Return True
                End If
            Else
                If File.Name.ToLower = Name.ToLower Then
                    Return True
                End If
            End If
        Next
        Return False
    End Function

    ''' <summary>Gets the file with the specified file name.</summary>
    <Extension>
    Public Function GetFile(DirectoryInfo As DirectoryInfo, Name As String, Optional ThrowOnNotExists As Boolean = True) As FileInfo
        If ThrowOnNotExists AndAlso Not DirectoryInfo.Contains(Name) Then
            Throw New FileNotFoundException
        End If
        Return New FileInfo(DirectoryInfo.FullName + "\" + Name)
    End Function

    ''' <summary>Gets an uri instance that points to this directory.</summary>
    <Extension>
    Public Function GetUri(DirectoryInfo As DirectoryInfo) As Uri
        Return New Uri(DirectoryInfo.FullName)
    End Function

    ''' <summary>Gets an uri instance that points to this file.</summary>
    <Extension>
    Public Function GetUri(FileInfo As FileInfo) As Uri
        Return New Uri(FileInfo.FullName)
    End Function

    ''' <summary>Deletes the specified file in the directory.</summary>
    <Extension>
    Public Sub Delete(DirectoryInfo As DirectoryInfo, Name As String, Optional ThrowOnNotExists As Boolean = True)
        Dim file = DirectoryInfo.GetFile(Name, False)
        If file.Exists Then
            file.Delete()
        Else
            If ThrowOnNotExists Then
                Throw New FileNotFoundException
            End If
        End If
    End Sub

    ''' <summary>Deletes the file.</summary>
    <Extension>
    Public Sub Delete(FileInfo As FileInfo, Optional ThrowOnNotExists As Boolean = True)
        If FileInfo.Exists Then
            FileInfo.Delete()
        Else
            If ThrowOnNotExists Then
                Throw New FileNotFoundException
            End If
        End If
    End Sub

    ''' <summary>Creates an directory in this directory.</summary>
    <Extension>
    Public Function CreateDirectory(Directory As DirectoryInfo, Name As String) As DirectoryInfo
        Return IO.Directory.CreateDirectory(Directory.FullName + "\" + Name)
    End Function

    ''' <summary>Gets the screen that contains the specified point.</summary>
    Public Function GetScreen(Point As Point) As Forms.Screen
        Return Forms.Screen.FromPoint(New System.Drawing.Point(Point.X, Point.Y))
    End Function

    ''' <summary>Converts an alpha value, 0-255, to an percentage, 0.0-100.0, value.</summary>
    Public Function AlphaToPercentage(Alpha As Double) As Double
        Return Math.Round((Alpha / 255) * 100)
    End Function

    ''' <summary>Converts a percentage value, 0.0-100.0, to an alpha, 0-255, value.</summary>
    Public Function PercentageToAlpha(Percentage As Double) As Double

        Dim a As Double = Percentage

        If a < 0 Then
            a = 0
        ElseIf a > 255
            a = 255
        End If

        a = Percentage * 255 / 100

        a = IIf(a < 0, 0, a)
        a = IIf(a > 255, 255, a)

        Return a

    End Function

    ''' <summary>Gets the bounds of this control on the screen.</summary>
    <Extension>
    Public Function ScreenBounds(Element As Control) As Rect

        Dim window As Window = Element.FindWindow

        Dim bounds As Rect = Nothing

        If window IsNot Nothing Then
            Dim boundsOnWindow As Rect = New Rect(Element.TransformToAncestor(window).Transform(New Point(0, 0)), New Size(Element.ActualWidth, Element.ActualHeight))
            bounds = New Rect(window.PointToScreen(boundsOnWindow.Location), New Size(Element.ActualWidth, Element.ActualHeight))
        End If

        Return bounds

    End Function

    ''' <summary>Finds the window that this control is hosted on.</summary>
    <Extension>
    Public Function FindWindow(Element As Control) As Window

        Dim parent = VisualTreeHelper.GetParent(Element)

        While TypeOf parent IsNot Window
            If parent IsNot Nothing Then
                parent = VisualTreeHelper.GetParent(parent)
            Else
                Exit While
            End If
        End While

        Return parent

    End Function

    ''' <summary>Checks whatever the specified screen has an application that is in fullscreen mode.</summary>
    Public Function ScreenHasFullscreenApp(Rect As Rect) As Boolean

        With Forms.Screen.FromRectangle(New System.Drawing.Rectangle(Rect.X, Rect.Y, Rect.Width, Rect.Height))

            Dim points As Point() =
                {
                New Point(.Bounds.Left + 10, .Bounds.Top + 10),
                New Point(.Bounds.Right - 10, .Bounds.Top + 10),
                New Point(.Bounds.Left + 10, .Bounds.Bottom - 10),
                New Point(.Bounds.Right - 10, .Bounds.Bottom - 10)
                }

            Dim _handles As IntPtr() = {0, 0, 0, 0}

            For i As Integer = 0 To 3
                _handles(i) = Win32.Unmanaged.WindowFromPoint(points(i).X, points(i).Y)
            Next

            Dim testHandle As IntPtr = _handles(0)

            If _handles(1) = testHandle AndAlso _handles(2) = testHandle AndAlso _handles(3) = testHandle Then
                Return True
            Else
                Return False
            End If

        End With

    End Function

End Module
