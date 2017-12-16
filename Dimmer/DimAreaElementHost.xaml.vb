Imports System.ComponentModel
Imports System.Windows.Threading

Public Class DimAreaElementHost

#Region "Properties"

    Private WithEvents _DimArea As DimArea
    Public Property DimArea As DimArea
        Get
            Return _DimArea
        End Get
        Set(value As DimArea)
            _DimArea = value
        End Set
    End Property

    Public Shared ReadOnly _WidthProperty As DependencyProperty = DependencyProperty.Register("_Width", GetType(Double), GetType(DimAreaElementHost), Nothing)
    Public Shared ReadOnly _HeightProperty As DependencyProperty = DependencyProperty.Register("_Height", GetType(Double), GetType(DimAreaElementHost), Nothing)

    Public Property _Width As Double
        Get
            Return Me.GetValue(_WidthProperty)
        End Get
        Set(value As Double)
            Me.SetValue(_WidthProperty, value)
        End Set
    End Property

    Public Property _Height As Double
        Get
            Return Me.GetValue(_HeightProperty)
        End Get
        Set(value As Double)
            Me.SetValue(_HeightProperty, value)
        End Set
    End Property

    Private Sub DimArea_PropertyChanged(sender As Object, e As PropertyChangedEventArgs) Handles _DimArea.PropertyChanged
        If e.PropertyName = "Width" Or e.PropertyName = "Height" Then
            UpdateSize()
        End If
    End Sub

    Private Sub UpdateSize()

        If DimArea.Width IsNot Nothing Then
            _Width = DimArea.Width
        Else
            _Width = GetScreen(New Point(DimArea.Left, DimArea.Top)).Bounds.Width
        End If

        If DimArea.Height IsNot Nothing Then
            _Height = DimArea.Height
        Else
            _Height = GetScreen(New Point(DimArea.Left, DimArea.Top)).Bounds.Height
        End If

    End Sub

#End Region
#Region "Window"

    Sub New(DimArea As DimArea)
        Me.DimArea = DimArea
        InitializeComponent()
    End Sub

    Public Overloads Sub Show()
        If Not IsLoaded Then
            MyBase.Show()
        End If
        UpdateSize()
        DimAreaElement.Show()
        BringToFront()
        timer.Start()
    End Sub

    Public Overloads Sub Hide()
        DimAreaElement.Hide()
        timer.Stop()
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        DimAreaElement.DimArea = DimArea
        Win32.Managed.HideWindowFromAltTab(Me)
    End Sub

    Private Sub Window_DragEnter(sender As Object, e As DragEventArgs)
        Hide()
    End Sub

    Private timer As New DispatcherTimer(TimeSpan.FromSeconds(5), DispatcherPriority.Normal, Sub()
                                                                                                 BringToFront()
                                                                                             End Sub, App.Current.Dispatcher)

    Private Sub BringToFront()
        Topmost = False
        Topmost = True
    End Sub

#End Region

End Class
