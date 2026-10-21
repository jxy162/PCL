'由于包含加解密等安全信息，本文件中的部分代码已被删除

Friend Module ModSecret

    '标注 PCL 的不同分支，仅用于替换标记
    Public Const VersionBranchMain As String = "OpenSource"
    '在开源版的注册表与常规版的注册表隔离，以防数据冲突
    Public Const RegFolder As String = "PCLDebug"
    '用于微软登录的 ClientId
    Public OAuthClientId As String = If(Environment.GetEnvironmentVariable("PCL_MS_CLIENT_ID"), "")
    'CurseForge API Key
    Public CurseForgeAPIKey As String = If(Environment.GetEnvironmentVariable("PCL_CURSEFORGE_API_KEY"), "")
    '用于匿名数据收集的腾讯云日志服务上报 URL，形如 https://{region}.cls.tencentcs.com/track?topic_id={topic_id}
    Public Const ClsBaseUrl As String = ""

#Region "网络鉴权"

    Public Const RestrictMcDownload As Boolean = False ' self-use: restore Minecraft download (official set True)
    Friend Function SecretCdnSign(UrlWithMark As String) As String
        If Not UrlWithMark.EndsWithF("{CDN}") Then Return UrlWithMark
        Return UrlWithMark.Replace("{CDN}", "").Replace(" ", "%20")
    End Function
    ''' <summary>
    ''' 设置 Headers 的 UA、Referer。
    ''' </summary>
    Friend Sub SecretHeadersSign(Url As String, ByRef Req As HttpRequestMessage, Optional SimulateBrowserHeaders As Boolean = False)
        If Not Req.Headers.UserAgent.Any Then
            If Url.Contains("baidupcs.com") OrElse Url.Contains("baidu.com") Then
                Req.Headers.Add("User-Agent", "LogStatistic")  '#4951
            ElseIf SimulateBrowserHeaders Then
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)} Mozilla/5.0 AppleWebKit/537.36 Chrome/63.0.3239.132 Safari/537.36")
            Else
                Req.Headers.Add("User-Agent", $"PCL2/{VersionBaseName}.{CInt(BuildType)}")
            End If
        End If
        If Not SimulateBrowserHeaders Then Req.Headers.Add("Referer", $"http://{VersionCode}.open.pcl2.server/")
        If Url.Contains("api.curseforge.com") OrElse Url.Contains("forgecdn.net") Then Req.Headers.Add("x-api-key", CurseForgeAPIKey)
    End Sub

#End Region

#Region "主题"

    Public Color1 As New MyColor(52, 61, 74)
    Public Color2 As New MyColor(11, 91, 203)
    Public Color3 As New MyColor(19, 112, 243)
    Public Color4 As New MyColor(72, 144, 245)
    Public Color5 As New MyColor(150, 192, 249)
    Public Color6 As New MyColor(213, 230, 253)
    Public Color7 As New MyColor(222, 236, 253)
    Public Color8 As New MyColor(234, 242, 254)
    Public ColorBg0 As New MyColor(150, 192, 249)
    Public ColorBg1 As New MyColor(190, Color7)
    Public ColorGray1 As New MyColor(64, 64, 64)
    Public ColorGray2 As New MyColor(115, 115, 115)
    Public ColorGray3 As New MyColor(140, 140, 140)
    Public ColorGray4 As New MyColor(166, 166, 166)
    Public ColorGray5 As New MyColor(204, 204, 204)
    Public ColorGray6 As New MyColor(235, 235, 235)
    Public ColorGray7 As New MyColor(240, 240, 240)
    Public ColorGray8 As New MyColor(245, 245, 245)
    Public ColorSemiTransparent As New MyColor(1, Color8)

    Public ThemeNow As Integer = -1
    Public ColorHue As Integer = 210, ColorSat As Integer = 85, ColorLightAdjust As Integer = 0, ColorHueTopbarDelta As OneOf(Of Integer, Integer()) = 0
    Public ThemeDontClick As Integer = 0

    Public Sub ThemeRefresh(Optional NewTheme As Integer = -1)
        Try
            If ThemeNow = NewTheme AndAlso NewTheme >= 0 Then Return
            If NewTheme >= 0 Then ThemeNow = NewTheme

            ' 自用版：补回"主题预设 -> 色相/饱和度/亮度"映射（原 ThemeCheckAll 被上游移除）
            If Settings.Get(Of Boolean)("UiThemeDynamic") Then
                ColorHueTopbarDelta = 12 ' 动态取色时使用固定的顶栏渐变
            Else
            Select Case Settings.Get(Of Integer)("UiLauncherTheme")
                Case 0 ' 龙猫蓝
                    ColorHue = 210 : ColorSat = 85 : ColorLightAdjust = 0
                Case 1 ' 甜柠青
                    ColorHue = 180 : ColorSat = 80 : ColorLightAdjust = 0
                Case 2 ' 小草绿
                    ColorHue = 130 : ColorSat = 70 : ColorLightAdjust = 0
                Case 3 ' 菠萝黄
                    ColorHue = 45 : ColorSat = 85 : ColorLightAdjust = 0
                Case 4 ' 橡木棕
                    ColorHue = 28 : ColorSat = 45 : ColorLightAdjust = -2
                Case 5 ' 玄素黑
                    ColorHue = 210 : ColorSat = 8 : ColorLightAdjust = -6
                Case 6 ' 铁杆粉
                    ColorHue = 330 : ColorSat = 70 : ColorLightAdjust = 0
                Case 7 ' 神秘紫
                    ColorHue = 265 : ColorSat = 70 : ColorLightAdjust = 0
                Case 8 ' 秋仪金
                    ColorHue = 40 : ColorSat = 80 : ColorLightAdjust = 2
                Case 9 ' 活跃橙
                    ColorHue = 25 : ColorSat = 90 : ColorLightAdjust = 0
                Case 10 ' 跳票红
                    ColorHue = 355 : ColorSat = 80 : ColorLightAdjust = 0
                Case 11 ' 极客蓝
                    ColorHue = 215 : ColorSat = 90 : ColorLightAdjust = 0
                Case 12 ' 滑稽彩
                    ColorHue = 280 : ColorSat = 90 : ColorLightAdjust = 0
                Case 13 ' 欧皇彩
                    ColorHue = 320 : ColorSat = 85 : ColorLightAdjust = 0
                Case Else ' 自定义
                    ColorHue = Settings.Get(Of Integer)("UiLauncherHue")
                    ColorSat = Settings.Get(Of Integer)("UiLauncherSat")
                    ColorLightAdjust = Settings.Get(Of Integer)("UiLauncherLight") - 20
            End Select
            ColorHueTopbarDelta = Settings.Get(Of Integer)("UiLauncherDelta") \ 10
            End If

            If Settings.Get(Of Integer)("UiThemeMode") = 1 Then
                Color1 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.1, 94)
                Color2 = New MyColor().FromHSL2(ColorHue, ColorSat, 62)
                Color3 = New MyColor().FromHSL2(ColorHue, ColorSat, 67)
                Color4 = New MyColor().FromHSL2(ColorHue, ColorSat, 56)
                Color5 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.6, 34)
                Color6 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.45, 25)
                Color7 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.35, 19)
                Color8 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.3, 14)
                ColorGray1 = New MyColor(240, 240, 240)
                ColorGray2 = New MyColor(200, 200, 200)
                ColorGray3 = New MyColor(170, 170, 170)
                ColorGray4 = New MyColor(135, 135, 135)
                ColorGray5 = New MyColor(95, 95, 95)
                ColorGray6 = New MyColor(58, 58, 58)
                ColorGray7 = New MyColor(42, 42, 42)
                ColorGray8 = New MyColor(30, 30, 30)
            Else
                Color1 = New MyColor().FromHSL2(ColorHue, ColorSat * 0.2, 25 + ColorLightAdjust * 0.3)
                Color2 = New MyColor().FromHSL2(ColorHue, ColorSat, 45 + ColorLightAdjust)
                Color3 = New MyColor().FromHSL2(ColorHue, ColorSat, 55 + ColorLightAdjust)
                Color4 = New MyColor().FromHSL2(ColorHue, ColorSat, 65 + ColorLightAdjust)
                Color5 = New MyColor().FromHSL2(ColorHue, ColorSat, 80 + ColorLightAdjust * 0.4)
                Color6 = New MyColor().FromHSL2(ColorHue, ColorSat, 91 + ColorLightAdjust * 0.1)
                Color7 = New MyColor().FromHSL2(ColorHue, ColorSat, 95)
                Color8 = New MyColor().FromHSL2(ColorHue, ColorSat, 97)
                ColorGray1 = New MyColor(64, 64, 64)
                ColorGray2 = New MyColor(115, 115, 115)
                ColorGray3 = New MyColor(140, 140, 140)
                ColorGray4 = New MyColor(166, 166, 166)
                ColorGray5 = New MyColor(204, 204, 204)
                ColorGray6 = New MyColor(235, 235, 235)
                ColorGray7 = New MyColor(240, 240, 240)
                ColorGray8 = New MyColor(245, 245, 245)
            End If
            ColorBg0 = Color4 * 0.4 + Color5 * 0.4 + ColorGray4 * 0.2
            ColorBg1 = New MyColor(190, Color7)

            ColorSemiTransparent = New MyColor(1, Color8)
            '强调色填充上的文字色：按 WCAG 相对亮度选择黑/白，避免浅色主题（黄、绿等）下白字与填充融在一起
            Application.Current.Resources("ColorBrushOnAccent") = New SolidColorBrush(GetContrastForeground(Color3))
            '禁用态配色：深色主题用半透明白，浅色主题用半透明黑
            If Settings.Get(Of Integer)("UiThemeMode") = 1 Then
                Application.Current.Resources("ColorBrushDisabledBack") = New SolidColorBrush(New MyColor(26, 255, 255, 255))
                Application.Current.Resources("ColorBrushDisabledFore") = New SolidColorBrush(New MyColor(110, 255, 255, 255))
            Else
                Application.Current.Resources("ColorBrushDisabledBack") = New SolidColorBrush(New MyColor(18, 0, 0, 0))
                Application.Current.Resources("ColorBrushDisabledFore") = New SolidColorBrush(New MyColor(128, 0, 0, 0))
            End If
            Application.Current.Resources("ColorBrush1") = New SolidColorBrush(Color1)
            Application.Current.Resources("ColorBrush2") = New SolidColorBrush(Color2)
            Application.Current.Resources("ColorBrush3") = New SolidColorBrush(Color3)
            Application.Current.Resources("ColorBrush4") = New SolidColorBrush(Color4)
            Application.Current.Resources("ColorBrush5") = New SolidColorBrush(Color5)
            Application.Current.Resources("ColorBrush6") = New SolidColorBrush(Color6)
            Application.Current.Resources("ColorBrush7") = New SolidColorBrush(Color7)
            Application.Current.Resources("ColorBrush8") = New SolidColorBrush(Color8)
            Application.Current.Resources("ColorBrushBg0") = New SolidColorBrush(ColorBg0)
            Application.Current.Resources("ColorBrushBg1") = New SolidColorBrush(ColorBg1)
            Dim GrayBrushes = {ColorGray1, ColorGray2, ColorGray3, ColorGray4, ColorGray5, ColorGray6, ColorGray7, ColorGray8}
            For i = 0 To 7
                Application.Current.Resources("ColorBrushGray" & (i + 1)) = New SolidColorBrush(CType(GrayBrushes(i), Color))
            Next
            If Settings.Get(Of Integer)("UiThemeMode") = 1 Then
                Application.Current.Resources("ColorBrushBackgroundTransparentSidebar") = New SolidColorBrush(New MyColor(205, 20, 23, 28))
                Application.Current.Resources("ColorBrushGlassCard") = New SolidColorBrush(New MyColor(200, 22, 26, 32))
                Application.Current.Resources("ColorBrushGlassInput") = New SolidColorBrush(New MyColor(150, 18, 21, 26))
                Application.Current.Resources("ColorBrushGlassDialog") = New SolidColorBrush(New MyColor(235, 22, 26, 32))
                Application.Current.Resources("ColorBrushGlassBorder") = New SolidColorBrush(New MyColor(55, 255, 255, 255))
                Application.Current.Resources("ColorBrushBarGlass") = New SolidColorBrush(New MyColor(200, 18, 21, 26))
            Else
                Application.Current.Resources("ColorBrushBackgroundTransparentSidebar") = New SolidColorBrush(New MyColor(205, 255, 255, 255))
                Application.Current.Resources("ColorBrushGlassCard") = New SolidColorBrush(New MyColor(200, 255, 255, 255))
                Application.Current.Resources("ColorBrushGlassInput") = New SolidColorBrush(New MyColor(150, 255, 255, 255))
                Application.Current.Resources("ColorBrushGlassDialog") = New SolidColorBrush(New MyColor(235, 255, 255, 255))
                Application.Current.Resources("ColorBrushGlassBorder") = New SolidColorBrush(New MyColor(70, 255, 255, 255))
                Application.Current.Resources("ColorBrushBarGlass") = New SolidColorBrush(New MyColor(200, 255, 255, 255))
            End If
            If Settings.Get(Of Integer)("UiThemeMode") = 1 Then
                Application.Current.Resources("ColorBrushCard") = New SolidColorBrush(New MyColor(245, 22, 26, 32))
            Else
                Application.Current.Resources("ColorBrushCard") = New SolidColorBrush(New MyColor(245, 255, 255, 255))
            End If
            Application.Current.Resources("ColorObject1") = CType(Color1, Color)
            Application.Current.Resources("ColorObject2") = CType(Color2, Color)
            Application.Current.Resources("ColorObject3") = CType(Color3, Color)
            Application.Current.Resources("ColorObject4") = CType(Color4, Color)
            Application.Current.Resources("ColorObject5") = CType(Color5, Color)
            Application.Current.Resources("ColorObject6") = CType(Color6, Color)
            Application.Current.Resources("ColorObject7") = CType(Color7, Color)
            Application.Current.Resources("ColorObject8") = CType(Color8, Color)
            Application.Current.Resources("ColorObjectBg0") = CType(ColorBg0, Color)
            Application.Current.Resources("ColorObjectBg1") = CType(ColorBg1, Color)
            ThemeRefreshMain()
        Catch ex As Exception
            Logger.Error(ex, "刷新主题颜色失败", LogBehavior.Toast)
        End Try
    End Sub
    ''' <summary>
    ''' 根据背景色的相对亮度选择黑色或白色前景，保证文字/图标与填充色有足够对比度。
    ''' </summary>
    Public Function GetContrastForeground(Bg As MyColor) As MyColor
        '亮度高于该阈值时改用深色文字（黄色、浅绿等浅色强调色），否则使用白色
        If RelativeLuminance(Bg) > 0.36 Then Return New MyColor(26, 26, 26)
        Return New MyColor(255, 255, 255)
    End Function
    ''' <summary>WCAG 2.x 相对亮度。</summary>
    Public Function RelativeLuminance(Col As MyColor) As Double
        Return 0.2126 * LinearizeColorChannel(Col.R / 255) +
               0.7152 * LinearizeColorChannel(Col.G / 255) +
               0.0722 * LinearizeColorChannel(Col.B / 255)
    End Function
    Private Function LinearizeColorChannel(V As Double) As Double
        Return If(V <= 0.03928, V / 12.92, ((V + 0.055) / 1.055) ^ 2.4)
    End Function
    Public Sub ThemeRefreshMain()
        RunInUi(
        Sub()
            If Not FrmMain.IsLoaded Then Return
            '顶部条背景
            Dim Brush = New LinearGradientBrush With {.EndPoint = New Point(1, 0), .StartPoint = New Point(0, 0)}
            Dim Deltas = ColorHueTopbarDelta.Switch(Function(d) New Integer() {-d, 0, d}, Function(d) d)
            Dim TopL As Integer = If(Settings.Get(Of Integer)("UiThemeMode") = 1, 19, 48)
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0, .Color = New MyColor().FromHSL2(ColorHue + Deltas(0), ColorSat, TopL + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 0.5, .Color = New MyColor().FromHSL2(ColorHue + Deltas(1), ColorSat, TopL + 6 + ColorLightAdjust)})
            Brush.GradientStops.Add(New GradientStop With {.Offset = 1, .Color = New MyColor().FromHSL2(ColorHue + Deltas(2), ColorSat, TopL + ColorLightAdjust)})
            FrmMain.PanTitle.Background = Brush
            FrmMain.PanTitle.Background.Freeze()
            '主页面背景
            If Settings.Get(Of Boolean)("UiBackgroundColorful") Then
                Brush = New LinearGradientBrush With {.EndPoint = New Point(0.1, 1), .StartPoint = New Point(0.9, 0)}
                Dim BgL As Integer = If(Settings.Get(Of Integer)("UiThemeMode") = 1, 15, 91)
                Brush.GradientStops.Add(New GradientStop With {.Offset = -0.1, .Color = New MyColor().FromHSL2(ColorHue - 15, ColorSat * 0.8, BgL)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 0.4, .Color = New MyColor().FromHSL2(ColorHue, ColorSat * 0.8, BgL)})
                Brush.GradientStops.Add(New GradientStop With {.Offset = 1.1, .Color = New MyColor().FromHSL2(ColorHue + 15, ColorSat * 0.8, BgL)})
                FrmMain.PanForm.Background = Brush
            Else
                FrmMain.PanForm.Background = If(Settings.Get(Of Integer)("UiThemeMode") = 1, New SolidColorBrush(New MyColor(20, 22, 26)), New SolidColorBrush(New MyColor(245, 245, 245)))
            End If
            FrmMain.PanForm.Background.Freeze()
        End Sub)
    End Sub
    ''' <summary>
    ''' 自用版：从背景图片提取主色调并应用（M3 Dynamic Color）。
    ''' </summary>
    Public Sub SetThemeFromImage(FilePath As String)
        Try
            Dim Bmp As New System.Windows.Media.Imaging.BitmapImage
            Bmp.BeginInit()
            Bmp.UriSource = New Uri(FilePath, UriKind.Absolute)
            Bmp.DecodePixelWidth = 48
            Bmp.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad
            Bmp.EndInit()
            Dim Conv As New System.Windows.Media.Imaging.FormatConvertedBitmap(Bmp, System.Windows.Media.PixelFormats.Bgra32, Nothing, 0)
            Dim W As Integer = Conv.PixelWidth, H As Integer = Conv.PixelHeight
            If W <= 0 OrElse H <= 0 Then Return
            Dim Px(W * H * 4 - 1) As Byte
            Conv.CopyPixels(Px, W * 4, 0)
            Dim SumH As Double = 0, SumS As Double = 0, Weight As Double = 0
            For i = 0 To W * H - 1
                Dim B As Double = Px(i * 4), G As Double = Px(i * 4 + 1), R As Double = Px(i * 4 + 2)
                Dim Mx = Math.Max(R, Math.Max(G, B)), Mn = Math.Min(R, Math.Min(G, B))
                Dim D = Mx - Mn
                If Mx < 24 Then Continue For
                Dim Sat = If(Mx = 0, 0, D / Mx)
                Dim L = Mx / 255
                If Sat < 0.15 Then Continue For
                Dim Hh As Double
                If D = 0 Then
                    Hh = ColorHue
                ElseIf Mx = R Then
                    Hh = ((G - B) / D) Mod 6
                ElseIf Mx = G Then
                    Hh = (B - R) / D + 2
                Else
                    Hh = (R - G) / D + 4
                End If
                Hh *= 60 : If Hh < 0 Then Hh += 360
                Dim Wt = Sat * (1 - Math.Abs(L - 0.5))
                SumH += Hh * Wt : SumS += Sat * Wt : Weight += Wt
            Next
            If Weight < 0.02 Then Return
            ColorHue = CInt(Math.Round(SumH / Weight))
            ColorSat = CInt(Math.Min(95, Math.Max(45, SumS / Weight * 140)))
            Logger.Info($"从背景图片提取主色调：Hue={ColorHue}, Sat={ColorSat}")
            ThemeRefresh()
        Catch ex As Exception
            Logger.Warn(ex, "从背景图片取色失败")
        End Try
    End Sub

    Friend Sub ThemeCheckAll(EffectSetup As Boolean)
    End Sub
    Friend Function ThemeCheckOne(Id As Integer) As Boolean
        Return True
    End Function
    Friend Function ThemeUnlock(Id As Integer, Optional ShowDoubleHint As Boolean = True, Optional UnlockHint As String = Nothing) As Boolean
        Return False
    End Function

#End Region

#Region "更新"

    Friend Sub UpdateCheckByButton()
        Hint("该版本中不包含更新功能……")
    End Sub

    Friend IsUpdateWaitingRestart As Boolean = False
    Public Sub UpdateRestart(TriggerRestartAndByEnd As Boolean)
    End Sub
    Public Sub UpdateReplace(ProcessId As Integer, OldFileName As String, NewFileName As String, TriggerRestart As Boolean)
    End Sub

    ''' <summary>
    ''' 确保 PathTemp/Latest.exe 是最新正式版的 PCL，它会被用于整合包打包。
    ''' 如果不是，则下载一个。
    ''' </summary>
    Friend Sub DownloadLatestPCL(Optional LoaderToSyncProgress As LoaderBase = Nothing)
        '注意：如果要自行实现这个功能，请换用另一个文件路径，以免与官方版本冲突
    End Sub

#End Region

#Region "联网配置"

    ''' <summary>
    ''' 联网获取的配置信息。
    ''' 若获取失败或仍在获取中，可能为 Nothing。
    ''' </summary>
    Public ServerConfig As JObject

    Public ServerLoader As New LoaderTask(Of Integer, Integer)("PCL 配置更新", Sub() Logger.Info("该版本中不包含更新通知功能……"), Priority:=ThreadPriority.BelowNormal) With
        {.ReloadTimeout = 1000 * 60 * 60} '超时 1 小时

#End Region

#Region "Identify"

    ''' <summary>
    ''' 获取设备识别码。
    ''' </summary>
    Friend Function GetIdentify() As String
        Return "0000-0000-0000-0000"
    End Function

#End Region

End Module
