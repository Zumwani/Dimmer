Imports System.Collections.ObjectModel
Imports System.IO

''' <summary>An object that manages dim areas.</summary>
Public Class DimAreaManager

#Region "Load / save"

    ''' <summary>Load all files in the directory.</summary>
    Public Sub LoadAll(Directory As DirectoryInfo)
        For Each file In Directory.GetFiles("*.xml")
            Load(file)
        Next
    End Sub

    ''' <summary>Save all dim areas that require saving.</summary>
    Public Sub SaveAll()
        For Each dimarea In DimAreas
            If Not dimarea.IsSaved Then
                dimarea.DoImmediateSave()
            End If
        Next
    End Sub

    ''' <summary>Attempt to load the file as a dim area and if successful add it the collection.</summary>
    Public Sub Load(File As FileInfo)
        Try
            Add(DimArea.FromDisk(File))
        Catch ex As Exception
            MessageBox.Show("File '" + File.FullName + "'" + vbNewLine + vbNewLine + ex.Message, "Could not load dim area.")
        End Try
    End Sub

    ''' <summary>Cleans up after the dim area, removes the serialized file if one exists.</summary>
    Public Sub Clean(DimArea As DimArea)
        DimAreaStore.Delete(DimArea.UID.ToString + ".xml", False)
    End Sub

#End Region
#Region "Collection"

    Private _FavoriteDimArea As DimArea
    Public Property FavoriteDimArea As DimArea
        Get
            Return _FavoriteDimArea
        End Get
        Set(value As DimArea)
            _FavoriteDimArea = value
            App.Current.UpdateNotifyIcon()
        End Set
    End Property

    ''' <summary>Occurs when a dim area is added.</summary>
    Public Event DimAreaAdded()

    ''' <summary>Occurs when a dim area is removed.</summary>
    Public Event DimAreaRemoved()

    Private _DimAreas As New ObservableCollection(Of DimArea)
    ''' <summary>Gets the collection of dim areas.</summary>
    Public ReadOnly Property DimAreas As ReadOnlyObservableCollection(Of DimArea)
        Get
            Static __DimAreas As New ReadOnlyObservableCollection(Of DimArea)(_DimAreas)
            Return __DimAreas
        End Get
    End Property

    ''' <summary>Adds the dim area to the collection.</summary>
    Public Sub Add(DimArea As DimArea)
        If DimArea IsNot Nothing Then

            _DimAreas.Add(DimArea)

            RaiseEvent DimAreaAdded()

        End If
    End Sub

    ''' <summary>Removes the dim area from the collection.</summary>
    Friend Sub Remove(DimArea As DimArea)

        If DimArea IsNot Nothing Then

            If DimAreas.Contains(DimArea) Then
                _DimAreas.Remove(DimArea)
            End If

            If Windows.ContainsKey(DimArea) AndAlso Not DimArea.IsRemovePending Then
                Windows(DimArea).Close()
            End If

            RaiseEvent DimAreaRemoved()

        End If

    End Sub

    ''' <summary>Gets the max index of the collection.</summary>
    Public ReadOnly Property MaxIndex As Integer
        Get
            Return IIf(DimAreas.Count > 0, DimAreas.Count - 1, 0)
        End Get
    End Property

    ''' <summary>Move an item to the specified index.</summary>
    Public Sub MoveItem(DimArea As DimArea, Index As Integer)
        _DimAreas.Move(_DimAreas.IndexOf(DimArea), Index)
    End Sub

#End Region
#Region "Windows"

    ''' <summary>Gets the list of created windows.</summary>
    Private Windows As New Dictionary(Of DimArea, DimAreaElementHost)

    ''' <summary>Gets an existing window or creates and returns a new one.</summary>
    Public ReadOnly Property Window(DimArea As DimArea) As DimAreaElementHost
        Get
            Return GetWindow(DimArea)
        End Get
    End Property

    ''' <summary>Shows the dim area.</summary>
    Public Sub Show(DimArea As DimArea)
        GetWindow(DimArea).Show()
        App.Current.UpdateNotifyIcon()
    End Sub

    ''' <summary>Hides the dim areas.</summary>
    Public Sub Hide(DimArea As DimArea)
        GetWindow(DimArea).Hide()
        App.Current.UpdateNotifyIcon()
    End Sub

    ''' <summary>Hides all dim areas.</summary>
    Public Sub HideAll()
        For Each area In DimAreas
            area.IsEnabled = False
        Next
    End Sub

    ''' <summary>Closes all dim areas.</summary>
    Public Sub CloseAll()
        For Each win In Windows.Values
            win.Close()
        Next
    End Sub

    ''' <summary>Gets an existing window or creates and returns a new one.</summary>
    Private Function GetWindow(DimArea As DimArea) As DimAreaElementHost
        If Windows.ContainsKey(DimArea) Then
            If Windows(DimArea) Is Nothing OrElse Not Windows(DimArea).IsLoaded Then
                Windows(DimArea) = New DimAreaElementHost(DimArea)
            End If
        Else

            Windows.Add(DimArea, New DimAreaElementHost(DimArea))
        End If
        Return Windows(DimArea)
    End Function

#End Region

End Class
