Imports System.Runtime.InteropServices

''' <summary>
''' 手柄（XInput）输入轮询。只在需要时启动计时器，没有手柄时不产生额外开销。
''' </summary>
Friend Module ModGamepad

#Region "XInput 声明"

    <StructLayout(LayoutKind.Sequential)>
    Private Structure XInputState
        Public PacketNumber As UInteger
        Public Gamepad As XInputGamepad
    End Structure

    <StructLayout(LayoutKind.Sequential)>
    Private Structure XInputGamepad
        Public Buttons As UShort
        Public LeftTrigger As Byte
        Public RightTrigger As Byte
        Public ThumbLX As Short
        Public ThumbLY As Short
        Public ThumbRX As Short
        Public ThumbRY As Short
    End Structure

    Private Const ButtonDPadUp As UShort = &H1
    Private Const ButtonDPadDown As UShort = &H2
    Private Const ButtonDPadLeft As UShort = &H4
    Private Const ButtonDPadRight As UShort = &H8
    Private Const ButtonStart As UShort = &H10
    Private Const ButtonBack As UShort = &H20
    Private Const ButtonLeftThumb As UShort = &H40
    Private Const ButtonRightThumb As UShort = &H80
    Private Const ButtonLeftShoulder As UShort = &H100
    Private Const ButtonRightShoulder As UShort = &H200
    Private Const ButtonA As UShort = &H1000
    Private Const ButtonB As UShort = &H2000
    Private Const ButtonX As UShort = &H4000
    Private Const ButtonY As UShort = &H8000

    Private Declare Function XInputGetState Lib "xinput1_4.dll" (UserIndex As Integer, ByRef State As XInputState) As Integer
    Private Declare Function XInputGetState9 Lib "xinput9_1_0.dll" (UserIndex As Integer, ByRef State As XInputState) As Integer

    ''' <summary>手柄输入的逻辑动作。</summary>
    Public Enum PadAction
        None = 0
        Up
        Down
        Left
        Right
        Accept
        Back
        Menu
        PageLeft
        PageRight
    End Enum

#End Region

#Region "轮询与事件"

    ''' <summary>虚拟光标是否处于激活状态（由用户操作手柄后置位，鼠标操作后复位）。</summary>
    Public PadActive As Boolean = False
    ''' <summary>收到手柄动作时触发，在 UI 线程上执行。</summary>
    Public Event ActionReceived(Act As PadAction)

    Private PollThread As Threading.Thread = Nothing
    Private PollStop As Boolean = False
    Private LastButtons As UShort = 0
    Private LastRepeatTick As Integer
    Private RepeatStartTick As Integer
    Private RepeatAction As PadAction = PadAction.None

    '方向键长按重复：首次 380ms，之后每 130ms 触发一次
    Private Const RepeatDelay As Integer = 380
    Private Const RepeatInterval As Integer = 130
    '摇杆死区，比例阈值
    Private Const StickDeadZone As Double = 0.55

    '单击回调，避免每个按钮单独写一长串判断
    Private ReadOnly ButtonMap As New Dictionary(Of UShort, PadAction) From {
        {ButtonA, PadAction.Accept},
        {ButtonB, PadAction.Back},
        {ButtonStart, PadAction.Menu},
        {ButtonBack, PadAction.Back},
        {ButtonDPadUp, PadAction.Up},
        {ButtonDPadDown, PadAction.Down},
        {ButtonDPadLeft, PadAction.Left},
        {ButtonDPadRight, PadAction.Right},
        {ButtonLeftShoulder, PadAction.PageLeft},
        {ButtonRightShoulder, PadAction.PageRight}
    }

    ''' <summary>开始监听手柄。重复调用安全。</summary>
    Public Sub Start()
        If PollThread IsNot Nothing Then Return
        PollStop = False
        PollThread = New Threading.Thread(AddressOf PollLoop) With {.IsBackground = True, .Name = "Gamepad"}
        PollThread.Start()
    End Sub

    ''' <summary>停止监听手柄。</summary>
    Public Sub [Stop]()
        PollStop = True
        PollThread = Nothing
    End Sub

    ''' <summary>重新开始一轮按键判定，用于鼠标介入后避免残留状态。</summary>
    Public Sub ResetState()
        RepeatAction = PadAction.None
        PadActive = False
    End Sub

    Private Sub PollLoop()
        Dim State As XInputState
        While Not PollStop
            Dim Connected As Boolean = False
            For Index = 0 To 3
                If Not TryReadState(Index, State) Then Continue For
                Connected = True
                ProcessState(State)
                Exit For
            Next
            '没有手柄时降低轮询频率，避免空耗
            Threading.Thread.Sleep(If(Connected, 12, 500))
        End While
    End Sub

    ''' <summary>读取指定序号手柄的状态；库缺失或未连接时返回 False。</summary>
    Private Function TryReadState(Index As Integer, ByRef State As XInputState) As Boolean
        Try
            Return XInputGetState(Index, State) = 0
        Catch ex As DllNotFoundException
        Catch ex As EntryPointNotFoundException
        End Try
        Try
            Return XInputGetState9(Index, State) = 0
        Catch
            Return False
        End Try
    End Function
    Private Sub ProcessState(State As XInputState)
        Dim Buttons = State.Gamepad.Buttons
        Dim Pressed As UShort = CUShort(Buttons And Not LastButtons)
        LastButtons = CUShort(Buttons)

        If Pressed <> 0 Then
            For Each Item In ButtonMap
                If (Pressed And Item.Key) <> 0 Then RaisePad(Item.Value)
            Next
            LastRepeatTick = GetTimeMs()
            RepeatStartTick = LastRepeatTick
            RepeatAction = DirectionFromButtons(Buttons)
            Return
        End If

        '摇杆：在死区之外时作为方向输入，与方向键共用重复节奏
        Dim StickAction = DirectionFromStick(State.Gamepad)
        Dim Held As PadAction = If(DirectionFromButtons(Buttons) <> PadAction.None, DirectionFromButtons(Buttons), StickAction)
        If Held = PadAction.None Then
            RepeatAction = PadAction.None
            Return
        End If
        If Held <> RepeatAction Then
            RepeatAction = Held
            LastRepeatTick = GetTimeMs()
            RepeatStartTick = LastRepeatTick
            RaisePad(Held)
            Return
        End If
        Dim Now As Integer = GetTimeMs()
        '长按重复：第一次在 RepeatDelay 之后，之后按 RepeatInterval 连发
        Dim Wait As Integer = If(Now - RepeatStartTick < RepeatDelay, RepeatDelay, RepeatInterval)
        If Now - LastRepeatTick >= Wait Then
            LastRepeatTick = Now
            RaisePad(Held)
        End If
    End Sub

    Private Function DirectionFromButtons(Buttons As UShort) As PadAction
        If (Buttons And ButtonDPadUp) <> 0 Then Return PadAction.Up
        If (Buttons And ButtonDPadDown) <> 0 Then Return PadAction.Down
        If (Buttons And ButtonDPadLeft) <> 0 Then Return PadAction.Left
        If (Buttons And ButtonDPadRight) <> 0 Then Return PadAction.Right
        Return PadAction.None
    End Function

    Private Function DirectionFromStick(Pad As XInputGamepad) As PadAction
        Dim X As Double = Pad.ThumbLX / 32767.0
        Dim Y As Double = Pad.ThumbLY / 32767.0
        If Math.Abs(X) < StickDeadZone AndAlso Math.Abs(Y) < StickDeadZone Then Return PadAction.None
        '取偏移更大的那个轴，避免斜向输入时同时触发两个方向
        If Math.Abs(X) > Math.Abs(Y) Then Return If(X > 0, PadAction.Right, PadAction.Left)
        Return If(Y > 0, PadAction.Up, PadAction.Down)
    End Function

    Private Sub RaisePad(Act As PadAction)
        If Act = PadAction.None Then Return
        RunInUi(
        Sub()
            If FrmMain Is Nothing OrElse Not FrmMain.IsActive Then Return
            PadActive = True
            RaiseEvent ActionReceived(Act)
        End Sub)
    End Sub

#End Region

End Module