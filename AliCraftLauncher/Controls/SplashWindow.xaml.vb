Imports System.Windows.Media.Animation

Public Class SplashWindow

    Private Sub SplashWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        BeginAnimation(OpacityProperty, New DoubleAnimation(0, 1, New Duration(TimeSpan.FromMilliseconds(260))))
        RingRotate.BeginAnimation(RotateTransform.AngleProperty, New DoubleAnimation(0, 360, New Duration(TimeSpan.FromMilliseconds(1400))) With {.RepeatBehavior = RepeatBehavior.Forever})
        Dim Breath As New DoubleAnimation(0.9, 1, New Duration(TimeSpan.FromMilliseconds(900))) With {.AutoReverse = True, .RepeatBehavior = RepeatBehavior.Forever, .EasingFunction = New SineEase With {.EasingMode = EasingMode.EaseInOut}}
        IconScale.BeginAnimation(ScaleTransform.ScaleXProperty, Breath)
        IconScale.BeginAnimation(ScaleTransform.ScaleYProperty, Breath)
        LabTitle.BeginAnimation(OpacityProperty, New DoubleAnimation(0, 1, New Duration(TimeSpan.FromMilliseconds(400))) With {.BeginTime = TimeSpan.FromMilliseconds(160)})
        LabSub.BeginAnimation(OpacityProperty, New DoubleAnimation(0, 1, New Duration(TimeSpan.FromMilliseconds(400))) With {.BeginTime = TimeSpan.FromMilliseconds(260)})
        Dim Progress As New DoubleAnimation(0, 200, New Duration(TimeSpan.FromMilliseconds(2400))) With {.EasingFunction = New CubicEase With {.EasingMode = EasingMode.EaseOut}}
        BarFill.BeginAnimation(FrameworkElement.WidthProperty, Progress)
    End Sub

    Public Sub CloseWithDelay(Delay As TimeSpan)
        Dim FadeOut As New DoubleAnimation(0, New Duration(TimeSpan.FromMilliseconds(220))) With {.BeginTime = Delay}
        AddHandler FadeOut.Completed, Sub() Close()
        BeginAnimation(OpacityProperty, FadeOut)
    End Sub

End Class
