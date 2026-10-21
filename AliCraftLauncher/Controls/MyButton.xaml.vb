Imports System.Windows.Markup

<ContentProperty("Inlines")>
Public Class MyButton

    '声明
    Public Event Click(sender As Object, e As MouseButtonEventArgs) '自定义事件

    '自定义属性
    Public Uuid As Integer = GetUuid()
    Public ReadOnly Property Inlines As InlineCollection
        Get
            Return LabText.Inlines
        End Get
    End Property
    Public Property Text As String
        Get
            Return GetValue(TextProperty)
        End Get
        Set(value As String)
            SetValue(TextProperty, value)
        End Set
    End Property '显示文本
    Public Shared ReadOnly TextProperty As DependencyProperty = DependencyProperty.Register("Text", GetType(String), GetType(MyButton), New PropertyMetadata(New PropertyChangedCallback(
    Sub(sender As DependencyObject, e As DependencyPropertyChangedEventArgs)
        If sender IsNot Nothing Then CType(sender, MyButton).LabText.Text = e.NewValue
    End Sub)))
    Public Property TextPadding As Thickness
        Get
            Return LabText.Padding
        End Get
        Set(value As Thickness)
            LabText.Padding = value
        End Set
    End Property
    Private _ColorType As ColorState = ColorState.Normal  '配色方案
    Public Property ColorType As ColorState
        Get
            Return _ColorType
        End Get
        Set(value As ColorState)
            _ColorType = value
            If PanFore IsNot Nothing Then
                If value = ColorState.Highlight Then
                    PanFore.SetResourceReference(Border.BackgroundProperty, "ColorBrush3")
                Else
                    RestoreBackground()
                End If
            End If
            RefreshColor()
        End Set
    End Property
    Public Enum ColorState
        Normal = 0
        Highlight = 1
        Red = 2
    End Enum
    '属性穿透
    Public Shared Shadows ReadOnly PaddingProperty As DependencyProperty = DependencyProperty.Register("Padding", GetType(Thickness), GetType(MyButton), New PropertyMetadata(New PropertyChangedCallback(
        Sub(sender As MyButton, e As DependencyPropertyChangedEventArgs) If sender IsNot Nothing Then sender.PanFore.Padding = e.NewValue)))
    Public Overloads Property Padding As Thickness
        Get
            Return PanFore.Padding
        End Get
        Set(value As Thickness)
            PanFore.Padding = value
        End Set
    End Property
    Public Property RealRenderTransform As Transform
        Get
            Return PanFore.RenderTransform
        End Get
        Set(value As Transform)
            PanFore.RenderTransform = value
        End Set
    End Property

    '自定义事件
    Private Const AnimationColorIn As Integer = 100
    Private Const AnimationColorOut As Integer = 200
    Private Sub RefreshColor(Optional obj = Nothing, Optional e = Nothing) Handles Me.MouseEnter, Me.MouseLeave, Me.Loaded, Me.IsEnabledChanged
        Try
            If IsLoaded AndAlso AniControlEnabled = 0 Then '防止默认属性变更触发动画

                If IsEnabled Then
                    Select Case ColorType
                        Case ColorState.Normal
                            '悬停时背景由 Button_MouseEnter 动画接管，此处不要抢回常态背景
                            If Not IsMouseOver Then RestoreBackground()
                            If IsMouseOver Then
                                '指向（Main 3）
                                AniStart({AaColor(PanFore, Border.BorderBrushProperty, "ColorBrush3", AnimationColorIn)}, "MyButton Color " & Uuid)
                                SetTextColor("ColorBrush3")
                            Else
                                '普通（Main 1）
                                AniStart({AaColor(PanFore, Border.BorderBrushProperty, "ColorBrush1", AnimationColorOut)}, "MyButton Color " & Uuid)
                                SetTextColor("ColorBrush1")
                            End If
                        Case ColorState.Highlight
                            '填充主按钮：文字改用对比度固定的 OnAccent，而不是跟随边框色
                            If Not IsMouseOver Then RestoreBackground()
                            SetTextColor("ColorBrushOnAccent")
                            If IsMouseOver Then
                                '指向（Main 3）
                                AniStart({AaColor(PanFore, Border.BorderBrushProperty, "ColorBrush3", AnimationColorIn)}, "MyButton Color " & Uuid)
                            Else
                                '高亮（Main 2）
                                AniStart({AaColor(PanFore, Border.BorderBrushProperty, "ColorBrush2", AnimationColorOut)}, "MyButton Color " & Uuid)
                            End If
                        Case ColorState.Red
                            If Not IsMouseOver Then RestoreBackground()
                            If IsMouseOver Then
                                '红色指向
                                AniStart({AaColor(PanFore, Border.BorderBrushProperty, "ColorBrushRedLight", AnimationColorIn)}, "MyButton Color " & Uuid)
                                SetTextColor("ColorBrushRedDark")
                            Else
                                '红色
                                AniStart({AaColor(PanFore, Border.BorderBrushProperty, "ColorBrushRedDark", AnimationColorOut)}, "MyButton Color " & Uuid)
                                SetTextColor("ColorBrushRedDark")
                            End If
                    End Select
                Else
                    '不可用：不再保留强调色填充，避免文字与底色同时变淡而无法辨认
                    AniStart({AaColor(PanFore, Border.BorderBrushProperty, "ColorBrushGray4", AnimationColorOut)}, "MyButton Color " & Uuid)
                    PanFore.SetResourceReference(Border.BackgroundProperty, "ColorBrushDisabledBack")
                    SetTextColor("ColorBrushDisabledFore")
                End If
            Else

                AniStop("MyButton Color " & Uuid)
                If IsEnabled Then
                    Select Case ColorType
                        Case ColorState.Normal
                            RestoreBackground()
                            If IsMouseOver Then
                                PanFore.SetResourceReference(Border.BorderBrushProperty, "ColorBrush3")
                                SetTextColor("ColorBrush3")
                            Else
                                PanFore.SetResourceReference(Border.BorderBrushProperty, "ColorBrush1")
                                SetTextColor("ColorBrush1")
                            End If
                        Case ColorState.Highlight
                            'M3 填充主按钮：背景取强调色，文字取固定的 OnAccent
                            PanFore.SetResourceReference(Border.BorderBrushProperty, "ColorBrush1")
                            PanFore.SetResourceReference(Border.BackgroundProperty, If(IsMouseOver, "ColorBrush2", "ColorBrush3"))
                            SetTextColor("ColorBrushOnAccent")
                        Case ColorState.Red
                            RestoreBackground()
                            If IsMouseOver Then
                                PanFore.SetResourceReference(Border.BorderBrushProperty, "ColorBrushRedLight")
                            Else
                                PanFore.SetResourceReference(Border.BorderBrushProperty, "ColorBrushRedDark")
                            End If
                            SetTextColor("ColorBrushRedDark")
                    End Select
                Else
                    PanFore.SetResourceReference(Border.BorderBrushProperty, "ColorBrushGray4")
                    PanFore.SetResourceReference(Border.BackgroundProperty, "ColorBrushDisabledBack")
                    SetTextColor("ColorBrushDisabledFore")
                End If

            End If
        Catch ex As Exception
            Logger.Warn(ex, "刷新按钮颜色出错")
        End Try
    End Sub

    '鼠标点击判定（务必放在点击事件之后，以使得 Button_MouseUp 先于 Button_MouseLeave 执行）
    Private IsMouseDown As Boolean = False
    Private Sub Button_MouseUp(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonUp
        If Not IsMouseDown Then Return
        Logger.Info($"按下按钮：{Text}")
        RaiseEvent Click(sender, e)
        RaiseCustomEvent() '自定义事件
    End Sub
    Private Sub Button_MouseDown(sender As Object, e As MouseButtonEventArgs) Handles Me.MouseLeftButtonDown
        IsMouseDown = True
        Focus()
        AniStart({
                 AaScaleTransform(PanFore, 0.955 - CType(PanFore.RenderTransform, ScaleTransform).ScaleX, 80,, New AniEaseOutFluent(AniEasePower.ExtraStrong)),
                 AaScaleTransform(PanFore, -0.01, 700,, New AniEaseOutFluent(AniEasePower.Middle))
                 }, "MyButton Scale " & Uuid)
    End Sub
    'M3：加载时应用高亮填充（XAML 声明的 Highlight 按钮此时 PanFore 才就绪）
    Private Sub MyButton_Loaded() Handles Me.Loaded
        If _ColorType = ColorState.Highlight Then PanFore.SetResourceReference(Border.BackgroundProperty, "ColorBrush3")
    End Sub
    ''' <summary>切换文字颜色；使用资源引用以便主题切换时自动跟随。</summary>
    Private Sub SetTextColor(Res As String)
        LabText.SetResourceReference(TextBlock.ForegroundProperty, Res)
    End Sub
    ''' <summary>恢复当前配色方案对应的常态背景（强调色填充或半透明白）。</summary>
    Private Sub RestoreBackground()
        PanFore.SetResourceReference(Border.BackgroundProperty, If(_ColorType = ColorState.Highlight, "ColorBrush3", "ColorBrushHalfWhite"))
    End Sub
    'M3 Expressive：胶囊按钮（圆角 = 高度一半）
    Private Sub MyButton_SizeChanged() Handles Me.SizeChanged
        Try
            If PanFore IsNot Nothing AndAlso ActualHeight > 0 Then PanFore.CornerRadius = New CornerRadius(ActualHeight / 2)
        Catch
        End Try
    End Sub
    Private Sub Button_MouseEnter() Handles Me.MouseEnter
        If Not IsEnabled Then Return '禁用态保持灰底，由 RefreshColor 统一控制
        Dim HoverBack As String = If(_ColorType = ColorState.Red, "ColorBrushRedBack", If(_ColorType = ColorState.Highlight, "ColorBrush2", "ColorBrush7"))
        AniStart(AaColor(PanFore, BackgroundProperty, HoverBack, AnimationColorIn), "MyButton Background " & Uuid)
    End Sub
    Private Sub Button_MouseUp() Handles Me.MouseLeftButtonUp
        If Not IsMouseDown Then Return
        IsMouseDown = False
        AniStart({
               AaScaleTransform(PanFore, 1 - CType(PanFore.RenderTransform, ScaleTransform).ScaleX, 300, 10, New AniEaseOutFluent(AniEasePower.Middle))
           }, "MyButton Scale " & Uuid)
    End Sub
    Private Sub Button_MouseLeave() Handles Me.MouseLeave
        If Not IsEnabled Then Return '禁用态保持灰底，由 RefreshColor 统一控制
        AniStart(AaColor(PanFore, BackgroundProperty, If(_ColorType = ColorState.Highlight, "ColorBrush3", "ColorBrushHalfWhite"), AnimationColorOut), "MyButton Background " & Uuid)
        If Not IsMouseDown Then Return
        IsMouseDown = False
        AniStart(AaScaleTransform(PanFore, 1 - CType(PanFore.RenderTransform, ScaleTransform).ScaleX, 800,, New AniEaseOutFluent(AniEasePower.Strong)), "MyButton Scale " & Uuid)
    End Sub

End Class
