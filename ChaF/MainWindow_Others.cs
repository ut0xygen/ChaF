using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using Equaller;


namespace ChaF
{
  partial class MainWindow
  {
    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hwnd, int index);  // 64bit only.
    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hwnd, int index, IntPtr newValue);  // 64bit only.
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int RegisterHotKey(IntPtr hWnd, int id, int modKey, int vKey);
    [DllImport("user32.dll", SetLastError = true)]
    private static extern int UnregisterHotKey(IntPtr hWnd, int id);

    private class DataContextMain
    {
      public Visibility ProgressVisibility {
        get; set;
      }
      public Visibility StatusVisibility {
        get; set;
      }
      public string StatusText {
        get; set;
      }
      public double StatusTextWidth {
        get; set;
      }
      public Visibility TimeVisibility {
        get; set;
      }
      public Visibility TimeDateVisibility {
        get; set;
      }
      public string TimeDateText {
        get; set;
      }
      public double TimeDateTextWidth {
        get; set;
      }
      public Visibility TimeClockVisibility {
        get; set;
      }
      public Visibility TimeClockColonAVisibility {
        get; set;
      }
      public Visibility TimeClockColonBVisibility {
        get; set;
      }
      public string TimeClockHourText {
        get; set;
      }
      public string TimeClockMinuteText {
        get; set;
      }
      public string TimeClockSecondText {
        get; set;
      }


      public DataContextMain()
      {
        ProgressVisibility = Visibility.Hidden;
        StatusVisibility = Visibility.Hidden;
        TimeVisibility = Visibility.Hidden;
        TimeDateVisibility = Visibility.Hidden;
        TimeClockVisibility = Visibility.Hidden;
        TimeClockColonAVisibility = Visibility.Hidden;
        TimeClockColonBVisibility = Visibility.Hidden;
      }
    }


    // Constant value.
    private const int MAX_SIZE_FILE = 100000;
    private const int _MAX_HOTKEY_ID = 0xC000;
    private const int _TEXT_FEED_TIME = 10;
    private const double _BACKGROUND_FROM = 0.7;
    private const double _BACKGROUND_TO = 0.0;
    private static readonly IEasingFunction _EASING_FUNCTION_EASE_IN_OUT = new CubicEase() { EasingMode = EasingMode.EaseInOut };
    private static readonly Duration _DURATION_SEC_MAIN_FRAME = new Duration(TimeSpan.FromMilliseconds(500));
    private static readonly Duration _DURATION_SEC_PROGRESS = new Duration(TimeSpan.FromMilliseconds(200));
    private static readonly Duration _DURATION_SEC_FADE_OUT = new Duration(TimeSpan.FromMilliseconds(200));
    private static readonly Duration _DURATION_SEC_BLINK = new Duration(TimeSpan.FromMilliseconds(200));

    // System objects.
    IntPtr _hWnd;
    private int _hotkeyClose = 0;
    private int _hotkeyVisibility = 1;
    private object _lockerHotkey = new object();
    private bool _isInitialized = false;
    private bool _isProcessingHotkey = false;

    // Window movable limit.
    private double _positionLeftUpper = 0.0d;
    private double _positionTopUpper = 0.0d;

    // DataContext.
    private DataContextMain _dcMain = new DataContextMain();

    // Settings.
    private string _surface = "LEFT";
    private bool _blink = false;
    private double _height = 0.05d;
    private double _position = 0.0d;


    /// <summary>
    ///   Calculate size and position.
    /// </summary>
    private void Setup()
    {
      double screenWidth;
      double screenHeight;


      ReadSettings();
      SetupSystem();

      // Get screen-size(Work-area).
      screenWidth = SystemParameters.WorkArea.Width;
      screenHeight = SystemParameters.WorkArea.Height;
      // Set window-size.
      this.Height = screenHeight * _height;
      this.Width = this.Height * 2.3;
      // Set movable-limit.
      _positionLeftUpper = screenWidth - this.Width;
      _positionTopUpper = screenHeight - this.Height;
      // Set window-position.
      switch (_surface) {
        case "LEFT":
          this.Left = 0.0d;
          this.Top = _positionTopUpper * _position;
          break;

        case "RIGHT":
          this.Left = _positionLeftUpper;
          this.Top = _positionTopUpper * _position;
          break;

        case "TOP":
          this.Left = _positionLeftUpper * _position;
          this.Top = 0.0d;
          break;

        case "BOTTOM":
          this.Left = _positionLeftUpper * _position;
          this.Top = _positionTopUpper;
          break;
      }
      // Adjust window-position.
      //   Horizontal-axis.
      if (this.Left <= 0) {
        this.Left = 0;
      }
      else if (this.Left >= _positionLeftUpper) {
        this.Left = _positionLeftUpper;
      }
      //   Vertical-axis.
      if (this.Top <= 0) {
        this.Top = 0;
      }
      else if (this.Top >= _positionTopUpper) {
        this.Top = _positionTopUpper;
      }

      // Initialize.
      Main.Height = this.Height;
      Main.Width = this.Width;
      Main.DataContext = _dcMain;
      InitializeMainFrame();
      InitializeMainProgress();
      InitializeMainStatus();
      InitializeMainTime();
    }

    /// <summary>
    ///   Read settings file.
    /// </summary>
    private void ReadSettings()
    {
        byte[] data;


      try {
        FileStream file;
        long size;


        file = File.Open("chaf.ini", FileMode.Open, FileAccess.Read, FileShare.Read);

        size = file.Length;
        if (file.CanRead == false || size == 0 || size > MAX_SIZE_FILE) {
          throw new Exception();
        }

        data = new byte[size];
        file.Read(data, 0, (int)size);

        file.Close();

        EquallerEntryInformation[] entries = new EquallerEntryInformation[] {
            new EquallerEntryInformation("SURFACE", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              switch (temp) {
                case "LEFT":
                case "RIGHT":
                case "TOP":
                case "BOTTOM":
                  _surface = temp;
                  break;
              }
            }),
            new EquallerEntryInformation("BLINK", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              switch (temp) {
                case "TRUE":
                  _blink = true;
                  break;
              }
            }),
            new EquallerEntryInformation("HEIGHT", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              try {
                _height = double.Parse(e.Content);

                if (_height < 0.05 || _position > 0.5) {
                  _height = 0.05d;
                }
              }
              catch {
              }
            }),
            new EquallerEntryInformation("POSITION", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              try {
                _position = double.Parse(e.Content);

                if (_position < 0 || _position > 1) {
                  _position = 0.0d;
                }
              }
              catch {
              }
            })
          };
        _ = Equaller.Equaller.Parse(data, entries, true);
      }
      catch {
      }
    }

    /// <summary>
    ///   Apply window-ex-style and set hot-key.
    /// </summary>
    private void SetupSystem()
    {
      const int GWL_EXSTYLE = -20;
      const long WS_EXNOACTIVATE = 0x08000000L;
      const long WS_EXTRANSPARENT = 0x00000020L;
      int result;


      _hWnd = new WindowInteropHelper(this).Handle;

      SetWindowLongPtr64(
        _hWnd,
        GWL_EXSTYLE,
        new IntPtr(GetWindowLongPtr64(_hWnd, GWL_EXSTYLE).ToInt64() | WS_EXNOACTIVATE | WS_EXTRANSPARENT)
      );

      for (; _hotkeyClose < _MAX_HOTKEY_ID; ++_hotkeyClose) {
        result = RegisterHotKey(_hWnd, _hotkeyClose, (int)ModifierKeys.Alt, 0x2E);
        if (result != 0) {
          break;
        }
      }
      for (; _hotkeyVisibility < _MAX_HOTKEY_ID; ++_hotkeyVisibility) {
        result = RegisterHotKey(_hWnd, _hotkeyVisibility, (int)ModifierKeys.Alt, 0x78);
        if (result != 0) {
          break;
        }
      }

      ComponentDispatcher.ThreadPreprocessMessage += OnThreadPreprocessMessage;
    }

    private async void Execute()
    {
      await ShowMainFrame();

      // Synchronization.
      if (DateTime.Now.Second > 56) {
        DateTime dt = DateTime.Now;


        await ShowMainProgress();
        await ShowMainStatus("SYNCHRONIZING");

        dt = DateTime.Now;
        await Task.Delay(((60 - dt.Second) * 1000) + (1000 - dt.Millisecond));

        await HideMainStatus();
        await HideMainProgress();
      }
      await ShowMainClock();
      
      _isInitialized = true;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      ComponentDispatcher.ThreadPreprocessMessage -= OnThreadPreprocessMessage;
      UnregisterHotKey(_hWnd, _hotkeyClose);
      UnregisterHotKey(_hWnd, _hotkeyVisibility);
    }

    protected override void OnClosed(EventArgs e)
    {
      base.OnClosed(e);
    }

    private void OnThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
      // Check hotkey.
      if (msg.message != 0x0312) {
        return;
      }


      lock (_lockerHotkey) {
        if (_isInitialized == false || _isProcessingHotkey) {
          return;
        }
        else {
          _isProcessingHotkey = true;
        }
      }


      if (msg.wParam.ToInt32() == _hotkeyClose) {
        if (IsVisible) {
          Task.Run(() => {
            this.Dispatcher.Invoke(new Action(async () => {
              await HideMainTime();
              await HideMainFrame();
              this.Hide();
              await HideMainTime_();
              this.Close();
            }));
          });
        }
        else {
          lock (_lockerHotkey) {
            _isProcessingHotkey = false;
          }
        }
      }
      else if (msg.wParam.ToInt32() == _hotkeyVisibility) {
        Task.Run(() => {
          this.Dispatcher.Invoke(new Action(async () => {
            if (IsVisible) {
              await HideMainTime();
              await HideMainFrame();
              this.Hide();
              await HideMainTime_();
            }
            else {
              this.Show();
              await ShowMainFrame();

              // Synchronization.
              if (DateTime.Now.Second > 56) {
                DateTime dt = DateTime.Now;


                await ShowMainProgress();
                await ShowMainStatus("SYNCHRONIZING");

                dt = DateTime.Now;
                await Task.Delay(((60 - dt.Second) * 1000) + (1000 - dt.Millisecond));


                await HideMainStatus();
                await HideMainProgress();
              }

              await ShowMainClock();
            }


            lock (_lockerHotkey) {
              _isProcessingHotkey = false;
            }
          }));
        });
      }
    }
  }
}
