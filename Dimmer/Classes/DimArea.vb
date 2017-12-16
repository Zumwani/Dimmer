Imports System.ComponentModel
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Windows.Threading
Imports System.Xml

''' <summary>Specifies when to undim an dim area.</summary>
<DataContract>
Public Enum UndimOption

    <EnumMember>
    Null = -1
    ''' <summary>Specifies that the area cannot be undimmed, outside of the config window.</summary>
    <EnumMember>
    None = 0
    ''' <summary>Specifies that the area should be undimmed when mouse enters the area.</summary>
    <EnumMember>
    MouseMove = 1
    ''' <summary>Specifies that the area should be undimmed when it is clicked using the mouse.</summary>
    <EnumMember>
    MouseClick = 2

End Enum

''' <summary>An object that stores information about an area that can be dimmed.</summary>
<DataContract>
Public Class DimArea
    Implements INotifyPropertyChanged

    '--Stores information about--
    'Title
    'Bounds
    'Color
    'HideMouse
    'MouseHideDelay
    'RedimWhenMouseStationary
    'RedimDelay
    'UndimOption
    'IsFavorited
    'IsEnabled

#Region "Constructors"

    ''' <summary>Creates a new dim area with set defaults, without a UID.</summary>
    Public Shared Function CreateDummy() As DimArea
        Return New DimArea With {.UndimOption = UndimOption.MouseClick, .HideMouseDelay = TimeSpan.FromSeconds(3), .HideMouse = True, .Color = Color.FromArgb(200, 0, 0, 0)}
    End Function

    ''' <summary>Creates a new dim area with set defaults.</summary>
    Public Shared Function CreateNew() As DimArea
        Return CreateNew("My dimarea", 0, 0, Nothing, Nothing, Color.FromArgb((255 * 0.75), 0, 0, 0), True, TimeSpan.FromSeconds(3), True, TimeSpan.FromSeconds(20), UndimOption.MouseClick)
    End Function

    ''' <summary>Creates a new dim area with the specified properties.</summary>
    Public Shared Function CreateNew(Title As String, Bounds As Rect, Color As Color, HideMouse As Boolean, HideMouseDelay As TimeSpan?, AutoRedim As Boolean, AutoRedimDelay As TimeSpan?, UndimOption As UndimOption) As DimArea
        Return CreateNew(Title, Bounds.Left, Bounds.Top, Bounds.Width, Bounds.Height, Color, HideMouse, HideMouseDelay, AutoRedim, AutoRedimDelay, UndimOption)
    End Function

    ''' <summary>Creates a new dim area with the specified properties.</summary>
    Public Shared Function CreateNew(Title As String, Left As Double, Top As Double, Width As Double?, Height As Double?, Color As Color, HideMouse As Boolean, HideMouseDelay As TimeSpan?, AutoRedim As Boolean, AutoRedimDelay As TimeSpan?, UndimOption As UndimOption) As DimArea
        Dim da As New DimArea
        With da
            .UID = Guid.NewGuid
            .Title = Title
            .Left = Left
            .Top = Top
            .Width = Width
            .Height = Height
            .Color = Color
            .HideMouse = HideMouse
            .HideMouseDelay = HideMouseDelay
            .RedimWhenMouseStationary = AutoRedim
            .RedimDelay = AutoRedimDelay
            .UndimOption = UndimOption
        End With
        App.Current.DimAreaManager.Add(da)
        Return da
    End Function

#End Region
#Region "Properties"

    ''' <summary>Occurs when a property has changed.</summary>
    Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

    Private _Title As String
    Private _Left As Double
    Private _Top As Double
    Private _Width As Double?
    Private _Height As Double?
    Private _Color As Color
    Private _HideMouse As Boolean
    Private _HideMouseDelay As TimeSpan?
    Private _RedimWhenMouseStationary As Boolean
    Private _RedimDelay As TimeSpan?
    Private _UndimOption As UndimOption
    Private _IsEnabled As Boolean

    ''' <summary>The UID of this dim area that is used to identify this dim area among a collection of dim areas.</summary>
    <DataMember>
    Public Property UID As Guid

    ''' <summary>The title of this dim area.</summary>
    <DataMember>
    Public Property Title As String
        Get
            Return _Title
        End Get
        Set(value As String)
            _Title = value
            _PropertyChanged("Title")
        End Set
    End Property

    ''' <summary>The x position on the screen of this dim area.</summary>
    <DataMember>
    Public Property Left As Double
        Get
            Return _Left
        End Get
        Set(value As Double)
            _Left = value
            _PropertyChanged("Left")
        End Set
    End Property

    ''' <summary>The y position on the screen of this dim area.</summary>
    <DataMember>
    Public Property Top As Double
        Get
            Return _Top
        End Get
        Set(value As Double)
            _Top = value
            _PropertyChanged("Top")
        End Set
    End Property

    ''' <summary>The width of this dim area.</summary>
    <DataMember>
    Public Property Width As Double?
        Get
            Return _Width
        End Get
        Set(value As Double?)
            _Width = value
            _PropertyChanged("Width")
        End Set
    End Property

    ''' <summary>The height of this dim area.</summary>
    <DataMember>
    Public Property Height As Double?
        Get
            Return _Height
        End Get
        Set(value As Double?)
            _Height = value
            _PropertyChanged("Height")
        End Set
    End Property

    ''' <summary>The color of this dim area.</summary>
    <DataMember>
    Public Property Color As Color
        Get
            Return _Color
        End Get
        Set(value As Color)
            _Color = value
            _PropertyChanged("Color")
        End Set
    End Property

    ''' <summary>Gets or sets whatever this dim area should automatically hide the mouse.</summary>
    <DataMember>
    Public Property HideMouse As Boolean
        Get
            Return _HideMouse
        End Get
        Set(value As Boolean)
            _HideMouse = value
            _PropertyChanged("HideMouse")
        End Set
    End Property

    ''' <summary>Gets or sets delay before hiding the mouse if HideMouse is true.</summary>
    <DataMember>
    Public Property HideMouseDelay As TimeSpan?
        Get
            Return _HideMouseDelay
        End Get
        Set(value As TimeSpan?)
            _HideMouseDelay = value
            _PropertyChanged("HideMouseDelay")
        End Set
    End Property

    ''' <summary>Gets or sets whatever the dim area should re-dim itself after mouse has been stationary.</summary>
    <DataMember>
    Public Property RedimWhenMouseStationary As Boolean
        Get
            Return _RedimWhenMouseStationary
        End Get
        Set(value As Boolean)
            _RedimWhenMouseStationary = value
            _PropertyChanged("RedimWhenMouseStationary")
        End Set
    End Property

    ''' <summary>Gets or sets delay before re-dimming after mouse has been stationary, inside dim area.</summary>
    <DataMember>
    Public Property RedimDelay As TimeSpan?
        Get
            Return _RedimDelay
        End Get
        Set(value As TimeSpan?)
            _RedimDelay = value
            _PropertyChanged("RedimDelay")
        End Set
    End Property

    ''' <summary>Gets or sets which action will undim the area.</summary>
    <DataMember>
    Public Property UndimOption As UndimOption
        Get
            Return _UndimOption
        End Get
        Set(value As UndimOption)
            _UndimOption = value
            _PropertyChanged("UndimOption")
        End Set
    End Property

    ''' <summary>Gets or sets whatever this dim area is favorited.</summary>
    <DataMember>
    Public Property IsFavorited As Boolean
        Get
            Return (App.Current.DimAreaManager.FavoriteDimArea Is Me)
        End Get
        Set(value As Boolean)
            If value = True Then
                App.Current.DimAreaManager.FavoriteDimArea = Me
            Else
                If App.Current.DimAreaManager.FavoriteDimArea Is Me Then
                    App.Current.DimAreaManager.FavoriteDimArea = Nothing
                End If
            End If
            _PropertyChanged("IsFavorited")
        End Set
    End Property

    ''' <summary>Gets or sets whatever this area is enabled or not. This also specifies whatever the area is visible or not.</summary>
    <DataMember>
    Public Property IsEnabled As Boolean
        Get
            Return _IsEnabled
        End Get
        Set(value As Boolean)
            _IsEnabled = value
            _PropertyChanged("IsEnabled")
            If value = True Then
                Show()
            Else
                Hide()
            End If
        End Set
    End Property

    Private Sub _PropertyChanged(PropertyName As String)
        'Workaround for the issue where dependency objects cannot be serialized by DataContractSerializer
        _IsSaved = False
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(PropertyName))
        Save()
    End Sub

#End Region
#Region "Methods"

    ''' <summary>Shows this dim area.</summary>
    Public Sub Show()
        App.Current.DimAreaManager.Show(Me)
    End Sub

    ''' <summary>Hides this dim area.</summary>
    Public Sub Hide()
        App.Current.DimAreaManager.Hide(Me)
    End Sub

    Public Sub ToggleEnabled()
        IsEnabled = Not IsEnabled
    End Sub

#End Region
#Region "Removing"

    ''' <summary>Specifies whatever this dim area is pending to be removed.</summary>
    Public ReadOnly Property IsRemovePending As Boolean

    ''' <summary>Starts the removal process for this dim area. The area will not be removed from disk until FinializeRemove() has been called in order to allow for undo functionality.</summary>
    Public Sub Remove()
        _IsRemovePending = True
        IsEnabled = False
        _IsSaved = True
        CancelSave()
        App.Current.DimAreaManager.Remove(Me)
    End Sub

    Public Shadows Property Parent As DimAreaElement

    ''' <summary>Finalize the remove process and remove file from disk.</summary>
    Public Sub FinalizeRemove()

        _IsSaved = False
        CancelSave()

        If IsFavorited Then
            App.Current.DimAreaManager.FavoriteDimArea = Nothing
        End If

        App.Current.DimAreaManager.Remove(Me)
        App.Current.DimAreaManager.Clean(Me)

        If Parent IsNot Nothing Then
            Dim w = Window.GetWindow(Parent)
            If w IsNot Nothing Then
                w.Close()
            End If
        End If

    End Sub

    ''' <summary>Cancel the pending remove process and add it back to the collection.</summary>
    Public Sub UndoRemove()
        _IsRemovePending = False
        App.Current.DimAreaManager.Add(Me)
        ForcedSave()
    End Sub

#End Region
#Region "Serialization"

    ''' <summary>Specifies whatever the dim area is saved to disk or not.</summary>
    Public ReadOnly Property IsSaved As Boolean = True

    'Use of timer to wait a second before saving will help ensure that there is only one write to the same file at a time. (within the application at any rate)
    Private WithEvents SaveTimer As New DispatcherTimer With {.Interval = TimeSpan.FromSeconds(1)}

    ''' <summary>Saves the dim area to disk.</summary>
    Public Sub Save()
        If Not IsSaved AndAlso SaveTimer IsNot Nothing Then
            'Reset timer
            SaveTimer.Stop()
            SaveTimer.Start()
        End If
    End Sub

    Private Sub SaveTimer_Tick() Handles SaveTimer.Tick
        'No other save call right now, _Save() can finally be called.
        _Save()
        SaveTimer.Stop()
    End Sub

    ''' <summary>Force a save, used when a removal process has been canceled.</summary>
    Public Sub ForcedSave()
        If UID = Guid.Empty Then
            UID = Guid.NewGuid
        End If
        _IsSaved = False
        _Save()
    End Sub

    ''' <summary>Saves the dim area immediately, used when closing app to make sure that all dim areas has been saved.</summary>
    Public Sub DoImmediateSave()
        _Save()
    End Sub

    ''' <summary>Cancels a pending save.</summary>
    Public Sub CancelSave()
        If SaveTimer IsNot Nothing Then
            SaveTimer.Stop()
        End If
    End Sub

    ''' <summary>Saves the dim area to disk, for real this time.</summary>
    Private Sub _Save()
        If Not UID = Guid.Empty AndAlso Not IsSaved Then

            Using fs As Stream = New FileStream(DimAreaStore.FullName + "\" + UID.ToString + ".xml", FileMode.Create)
                Using xmls = XmlWriter.Create(fs, New XmlWriterSettings() With {.Indent = True})

                    Dim ser As New DataContractSerializer(GetType(DimArea))
                    ser.WriteObject(xmls, Me)

                End Using
            End Using

        End If
    End Sub

    ''' <summary>Attempts to deserialize the file as a dim area. Returns nothing if it couldn´t.</summary>
    Public Shared Function FromDisk(File As FileInfo) As DimArea

        Dim dimarea As DimArea = Nothing

        If File.Exists AndAlso File.Extension = ".xml" Then

            Try

                Using stream = New FileStream(File.FullName, FileMode.Open)
                    Using xmls = XmlReader.Create(stream)

                        Dim ser As New DataContractSerializer(GetType(DimArea))
                        dimarea = ser.ReadObject(xmls, True)

                    End Using
                End Using

            Catch ex As Exception
            End Try

        End If

        Return dimarea

    End Function

#End Region

End Class
