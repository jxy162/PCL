Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Animation

''' <summary>
''' 全屏与手柄共用的焦点导航：聚焦项放大并上移，其他项缩小变淡。
''' 全屏时始终生效；窗口模式下仅在手柄操作后生效，鼠标移动会交还给鼠标。
''' </summary>
Friend Module ModFocusNav

    Private Const FocusScale As Double = 1.12
    Private Const UnfocusScale As Double = 0.9
    Private Const FocusLift As Double = -8
    Private Const UnfocusOpacity As Double = 0.55
    Private Const AnimationTime As Integer = 220
    Public Const CardTag As String = "FsCard"

    Private Focused As FrameworkElement = Nothing
    Private ReadOnly StyledElements As New HashSet(Of FrameworkElement)

    Private Function IsActive() As Boolean
        Return FrmMain.IsFullscreenMode OrElse ModGamepad.PadActive
    End Function

    Private Function GetRoots() As List(Of FrameworkElement)
        Dim Roots As New List(Of FrameworkElement)
        If FrmMain.PanMsg.Children.Count > 0 Then
            Roots.Add(FrmMain.PanMsg)
        ElseIf FrmMain.IsFullscreenMode Then
            Roots.Add(FrmMain.PanFsNav)
            If FrmMain.FsOnHome Then
                Roots.Add(FrmMain.PanFsHome)
            Else
                Roots.Add(FrmMain.PanMain)
            End If
        Else
            Roots.Add(FrmMain.PanMain)
            Roots.Add(FrmMain.PanBottom)
        End If
        Return Roots
    End Function

    Private Function IsFocusTarget(Element As FrameworkElement) As Boolean
        If Not Element.IsEnabled OrElse Element.ActualWidth <= 0 OrElse Element.ActualHeight <= 0 Then Return False
        If CardTag.Equals(Element.Tag) Then Return True
        Return TypeOf Element Is MyButton OrElse TypeOf Element Is MyRadioButton OrElse TypeOf Element Is MyIconButton OrElse
               TypeOf Element Is MyListItem OrElse TypeOf Element Is MyCheckBox
    End Function

    Private Sub CollectTargets(Parent As DependencyObject, Result As List(Of FrameworkElement))
        For Index = 0 To VisualTreeHelper.GetChildrenCount(Parent) - 1
            Dim Child As DependencyObject = VisualTreeHelper.GetChild(Parent, Index)
            Dim Element = TryCast(Child, FrameworkElement)
            If Element IsNot Nothing AndAlso Element.Visibility <> Visibility.Visible Then Continue For
            If Element IsNot Nothing AndAlso IsFocusTarget(Element) Then
                Result.Add(Element)
            Else
                CollectTargets(Child, Result)
            End If
        Next
    End Sub

    Private Function CollectAll() As List(Of FrameworkElement)
        Dim Result As New List(Of FrameworkElement)
        If Not IsActive() Then Return Result
        For Each Root In GetRoots()
            If Root.Visibility = Visibility.Visible Then CollectTargets(Root, Result)
        Next
        Return Result
    End Function

    Private Function CenterOf(Element As FrameworkElement) As Point
        Try
            Return Element.TransformToVisual(FrmMain.PanForm).Transform(New Point(Element.ActualWidth / 2, Element.ActualHeight / 2))
        Catch
            Return New Point(0, 0)
        End Try
    End Function

    Private Function FindDefault(Targets As List(Of FrameworkElement)) As FrameworkElement
        Dim Preferred = Targets.FirstOrDefault(Function(Item) Item.Name = "TileFsLaunch")
        If Preferred IsNot Nothing Then Return Preferred
        Return Targets.OrderBy(Function(Item) CenterOf(Item).Y).ThenBy(Function(Item) CenterOf(Item).X).First()
    End Function

    Private Function FindNeighbor(Origin As FrameworkElement, Direction As ModGamepad.PadAction, Targets As List(Of FrameworkElement)) As FrameworkElement
        Dim From As Point = CenterOf(Origin)
        Dim Best As FrameworkElement = Nothing
        Dim BestScore As Double = Double.MaxValue
        For Each Candidate In Targets
            If Candidate Is Origin Then Continue For
            Dim Offset As Vector = CenterOf(Candidate) - From
            Dim Along As Double
            Dim Across As Double
            Select Case Direction
                Case ModGamepad.PadAction.Up
                    Along = -Offset.Y
                    Across = Math.Abs(Offset.X)
                Case ModGamepad.PadAction.Down
                    Along = Offset.Y
                    Across = Math.Abs(Offset.X)
                Case ModGamepad.PadAction.Left
                    Along = -Offset.X
                    Across = Math.Abs(Offset.Y)
                Case Else
                    Along = Offset.X
                    Across = Math.Abs(Offset.Y)
            End Select
            If Along <= 1 Then Continue For
            Dim Score As Double = Along + Across * 2
            If Score < BestScore Then
                BestScore = Score
                Best = Candidate
            End If
        Next
        Return Best
    End Function

    ''' <summary>取得元素可用于动效的缩放与位移变换；必要时为没有变换的元素创建变换组。</summary>
    Private Sub ResolveTransforms(Element As FrameworkElement, ByRef Scale As ScaleTransform, ByRef Lift As TranslateTransform, Create As Boolean)
        Dim Transform As Transform = Element.RenderTransform
        If TypeOf Transform Is TransformGroup Then
            For Each Item In CType(Transform, TransformGroup).Children
                If TypeOf Item Is ScaleTransform Then Scale = CType(Item, ScaleTransform)
                If TypeOf Item Is TranslateTransform Then Lift = CType(Item, TranslateTransform)
            Next
        ElseIf Transform Is Nothing AndAlso Create Then
            Scale = New ScaleTransform(1, 1)
            Lift = New TranslateTransform(0, 0)
            Dim Group As New TransformGroup
            Group.Children.Add(Scale)
            Group.Children.Add(Lift)
            Element.RenderTransformOrigin = New Point(0.5, 0.5)
            Element.RenderTransform = Group
        End If
    End Sub

    ''' <summary>
    ''' 以动画过渡到目标值。动画结束后取消动画并把基础值设为目标值，
    ''' 避免动画保持住属性而影响控件自身的缩放动画。
    ''' </summary>
    Private Sub Animate(Target As DependencyObject, Prop As DependencyProperty, Value As Double)
        Dim Anim As New DoubleAnimation(Value, New Duration(TimeSpan.FromMilliseconds(AnimationTime))) With {
            .EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseOut},
            .FillBehavior = FillBehavior.Stop}
        If TypeOf Target Is UIElement Then
            CType(Target, UIElement).BeginAnimation(Prop, Anim, HandoffBehavior.SnapshotAndReplace)
        Else
            CType(Target, Animatable).BeginAnimation(Prop, Anim, HandoffBehavior.SnapshotAndReplace)
        End If
        Target.SetValue(Prop, Value)
    End Sub

    Private Sub ApplyStyle(Element As FrameworkElement, IsFocused As Boolean, IsReset As Boolean)
        Dim Emphasis As Boolean = IsFocused AndAlso Not IsReset
        Dim Scale As ScaleTransform = Nothing
        Dim Lift As TranslateTransform = Nothing
        ResolveTransforms(Element, Scale, Lift, Not IsReset)
        If Scale IsNot Nothing Then
            Dim Value As Double = If(IsReset, 1.0, If(IsFocused, FocusScale, UnfocusScale))
            Animate(Scale, ScaleTransform.ScaleXProperty, Value)
            Animate(Scale, ScaleTransform.ScaleYProperty, Value)
        End If
        If Lift IsNot Nothing Then Animate(Lift, TranslateTransform.YProperty, If(Emphasis, FocusLift, 0.0))
        If CardTag.Equals(Element.Tag) OrElse FrmMain.IsFullscreenMode Then
            Animate(Element, UIElement.OpacityProperty, If(IsFocused OrElse IsReset, 1.0, UnfocusOpacity))
        End If
        If CardTag.Equals(Element.Tag) AndAlso TypeOf Element Is Border Then
            Dim Card As Border = CType(Element, Border)
            Card.Background = New SolidColorBrush(If(Emphasis, Color.FromArgb(&H3C, 255, 255, 255), Color.FromArgb(&H26, 255, 255, 255)))
            Card.BorderBrush = New SolidColorBrush(If(Emphasis, Color.FromArgb(&HC8, &H9B, &HE8, &HA0), Color.FromArgb(&H3C, 255, 255, 255)))
        End If
    End Sub

    ''' <summary>按当前可见范围重新计算焦点与各元素的样式；不可用时恢复全部元素。</summary>
    Public Sub RefreshFocus()
        Dim Targets = CollectAll()
        For Each Old In StyledElements.ToList()
            If Not Targets.Contains(Old) Then
                ApplyStyle(Old, False, True)
                StyledElements.Remove(Old)
            End If
        Next
        If Focused IsNot Nothing AndAlso Not Targets.Contains(Focused) Then Focused = Nothing
        If Focused Is Nothing AndAlso Targets.Count > 0 Then Focused = FindDefault(Targets)
        For Each Target In Targets
            ApplyStyle(Target, Target Is Focused, False)
            StyledElements.Add(Target)
        Next
    End Sub

    ''' <summary>清除焦点，把所有元素恢复常态（用于鼠标接管操作）。</summary>
    Public Sub ClearFocus()
        Focused = Nothing
        For Each Old In StyledElements.ToList()
            ApplyStyle(Old, False, True)
        Next
        StyledElements.Clear()
    End Sub

    ''' <summary>手柄方向键移动焦点；Direction 为 None 时只确保存在默认焦点。</summary>
    Public Sub MoveFocus(Direction As ModGamepad.PadAction)
        Dim Targets = CollectAll()
        If Targets.Count = 0 Then Return
        If Focused Is Nothing OrElse Not Targets.Contains(Focused) Then
            Focused = FindDefault(Targets)
        ElseIf Direction <> ModGamepad.PadAction.None Then
            Dim Neighbor = FindNeighbor(Focused, Direction, Targets)
            If Neighbor IsNot Nothing Then Focused = Neighbor
        End If
        RefreshFocus()
        Focused?.BringIntoView()
    End Sub

    ''' <summary>鼠标悬停到可聚焦元素上时，焦点跟随鼠标。</summary>
    Public Sub FocusUnderMouse(Source As Object)
        If Not IsActive() Then Return
        Dim Roots = GetRoots()
        Dim Current = TryCast(Source, DependencyObject)
        Do While Current IsNot Nothing
            Dim Element = TryCast(Current, FrameworkElement)
            If Element IsNot Nothing AndAlso IsFocusTarget(Element) AndAlso Roots.Any(Function(Root) Element.IsDescendantOf(Root)) Then
                If Element IsNot Focused Then
                    Focused = Element
                    RefreshFocus()
                End If
                Return
            End If
            Current = If(TypeOf Current Is Visual, VisualTreeHelper.GetParent(Current), Nothing)
        Loop
    End Sub

    ''' <summary>构造用于手柄触发点击的鼠标事件参数。</summary>
    Public Function CreateClickArgs(Source As Object) As MouseButtonEventArgs
        Return New MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left) With {
            .RoutedEvent = UIElement.MouseLeftButtonUpEvent,
            .Source = Source}
    End Function

    ''' <summary>对当前焦点执行确认操作；尚未有焦点时先选出默认项。</summary>
    Public Sub ActivateFocused()
        If Focused Is Nothing Then
            MoveFocus(ModGamepad.PadAction.None)
            Return
        End If
        If TypeOf Focused Is MyButton Then
            CType(Focused, MyButton).FocusActivate()
        ElseIf TypeOf Focused Is MyRadioButton Then
            CType(Focused, MyRadioButton).FocusActivate()
        ElseIf TypeOf Focused Is MyIconButton Then
            CType(Focused, MyIconButton).FocusActivate()
        ElseIf TypeOf Focused Is MyListItem Then
            CType(Focused, MyListItem).FocusActivate()
        ElseIf TypeOf Focused Is MyCheckBox Then
            CType(Focused, MyCheckBox).FocusActivate()
        ElseIf CardTag.Equals(Focused.Tag) Then
            FrmMain.ActivateFsCard(Focused)
        End If
    End Sub

    ''' <summary>手柄肩键切换主导航页面：全屏时切换顶部导航，窗口模式切换底部导航。</summary>
    Public Sub SwitchPage(Forward As Boolean)
        If FrmMain.PanMsg.Children.Count > 0 Then Return
        Dim Radios As New List(Of MyRadioButton)
        If FrmMain.IsFullscreenMode Then
            Radios.AddRange({FrmMain.NavFsHome, FrmMain.NavFsInstance, FrmMain.NavFsDownload, FrmMain.NavFsSetup})
        Else
            For Each Child In FrmMain.PanTitleSelect.Children
                If TypeOf Child Is MyRadioButton Then Radios.Add(CType(Child, MyRadioButton))
            Next
        End If
        Dim Index As Integer = Radios.FindIndex(Function(Radio) Radio.Checked)
        Dim Target As Integer = Index + If(Forward, 1, -1)
        If Index < 0 OrElse Target < 0 OrElse Target >= Radios.Count Then Return
        Radios(Target).SetChecked(True, True, True)
    End Sub

End Module