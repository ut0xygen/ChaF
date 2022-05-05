using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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

    private const int TIME_TEXT_FEED = 10;
    private const double OPACITY_BLINK = 0.7;
    private static readonly IEasingFunction _EASING_FUNCTION_EASE_IN_OUT = new CubicEase() { EasingMode = EasingMode.EaseInOut };
    private static readonly Duration DURATION_MAIN_FRAME = new Duration(TimeSpan.FromMilliseconds(500));
    private static readonly Duration DURATION_PROGRESS = new Duration(TimeSpan.FromMilliseconds(200));
    private static readonly Duration DURATION_HIDE = new Duration(TimeSpan.FromMilliseconds(200));
    private static readonly Duration DURATION_BLINK = new Duration(TimeSpan.FromMilliseconds(200));

    private int ID_HOTKEY_CLOSE {
      get; set;
    }
    private int ID_HOTKEY_VISIBILITY {
      get; set;
    }

    private static DataContextMain DataContextMain_ {
      get;
    }
    private IntPtr WindowHandle {
      get; set;
    }
    private bool ValidHotkey {
      get; set;
    }
    private object EXLockHotkey {
      get; set;
    }
    private bool EXProcessingHotkey {
      get; set;
    }
    private string Surface {
      get; set;
    }
    private double HeightRatio {
      get; set;
    }
    private double PositionRatio {
      get; set;
    }
    private bool EnableBlink {
      get; set;
    }
    private double LeftLimit {
      get; set;
    }
    private double TopLimit {
      get; set;
    }
    private MainFrame_ MF {
      get; set;
    }
    private MainProgress_ MP {
      get; set;
    }
    private MainStatus_ MS {
      get; set;
    }
    private MainTime_ MT {
      get; set;
    }


    static MainWindow()
    {
      DataContextMain_ = new DataContextMain();
    }


    private void Setup()
    {
      double screenWidth = SystemParameters.WorkArea.Width;
      double screenHeight = SystemParameters.WorkArea.Height;


      ID_HOTKEY_CLOSE = 0;
      ID_HOTKEY_VISIBILITY = 1;

      Main.DataContext = DataContextMain_;
      ValidHotkey = false;
      EXLockHotkey = new object();
      EXProcessingHotkey = false;
      Surface = "LEFT";
      HeightRatio = 0.05d;
      PositionRatio = 0.0d;
      EnableBlink = false;

      ReadSettings();
      SetupSystem();

      // Set window-size.
      this.Height = screenHeight * HeightRatio;
      this.Width = this.Height * 2.3;
      // Set movable-limit.
      LeftLimit = screenWidth - this.Width;
      TopLimit = screenHeight - this.Height;
      // Set window-position.
      switch (Surface) {
        case "LEFT":
          this.Left = 0.0d;
          this.Top = TopLimit * PositionRatio;
          break;

        case "RIGHT":
          this.Left = LeftLimit;
          this.Top = TopLimit * PositionRatio;
          break;

        case "TOP":
          this.Left = LeftLimit * PositionRatio;
          this.Top = 0.0d;
          break;

        case "BOTTOM":
          this.Left = LeftLimit * PositionRatio;
          this.Top = TopLimit;
          break;
      }
      // Adjust window-position.
      //   Horizontal-axis.
      if (this.Left <= 0) {
        this.Left = 0;
      }
      else if (this.Left >= LeftLimit) {
        this.Left = LeftLimit;
      }
      //   Vertical-axis.
      if (this.Top <= 0) {
        this.Top = 0;
      }
      else if (this.Top >= TopLimit) {
        this.Top = TopLimit;
      }

      // Initialize.
      Main.Height = this.Height;
      Main.Width = this.Width;
      MF = new MainFrame_(this, Main, MainFrame);
      MP = new MainProgress_(this, Main, MainProgress);
      MS = new MainStatus_(this, Main, MainStatusI, MainStatus);
      MT = new MainTime_(this, Main, MainTimeI, MainTime, MainTimeDateI, MainTimeDate, MainTimeClockI, MainTimeClock, MainTimeClockHour, MainTimeClockMinute, MainTimeClockSecond, MainTimeClockColonA, MainTimeClockColonB);
    }

    private void ReadSettings()
    {
      const int MAX_FILE_SIZE = 1048576;  // 2^20


      try {
        FileStream file = File.Open("chaf.ini", FileMode.Open, FileAccess.Read, FileShare.Read);
        long size = file.Length;
        byte[] data;


        if (file.CanRead == false || size == 0 || size > MAX_FILE_SIZE) throw new Exception();
        data = new byte[size];
        file.Read(data, 0, (int)size);
        file.Close();

        _ = Equaller.Equaller.Parse(
          data,
          new EquallerEntryInformation[] {
            new EquallerEntryInformation("SURFACE", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              switch (temp) {
                case "LEFT":
                case "RIGHT":
                case "TOP":
                case "BOTTOM":
                  Surface = temp;
                  break;
              }
            }),
            new EquallerEntryInformation("BLINK", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              switch (temp) {
                case "TRUE":
                  EnableBlink = true;
                  break;
              }
            }),
            new EquallerEntryInformation("HEIGHT", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              try {
                HeightRatio = double.Parse(e.Content);

                if (HeightRatio < 0.05 || PositionRatio > 0.5) {
                  HeightRatio = 0.05d;
                }
              }
              catch {
              }
            }),
            new EquallerEntryInformation("POSITION", false, (EquallerEventArgs e) =>{
              string temp = e.Content.ToUpper();


              try {
                PositionRatio = double.Parse(e.Content);

                if (PositionRatio < 0 || PositionRatio > 1) {
                  PositionRatio = 0.0d;
                }
              }
              catch {
              }
            })
          }, true);
      }
      catch { }
    }

    private void SetupSystem()
    {
      const int GWL_EXSTYLE = -20;
      const long WS_EXNOACTIVATE = 0x08000000L;
      const long WS_EXTRANSPARENT = 0x00000020L;
      const int MAX_HOTKEY_ID = 0xC000;

      int result;
      bool failed = false;


      // Get window handle.
      WindowHandle = new WindowInteropHelper(this).Handle;

      // Apply extend window style.
      SetWindowLongPtr64(
        WindowHandle,
        GWL_EXSTYLE,
        new IntPtr(GetWindowLongPtr64(WindowHandle, GWL_EXSTYLE).ToInt64() | WS_EXNOACTIVATE | WS_EXTRANSPARENT)
      );

      // Register close hotkey.
      failed = true;
      for (; ID_HOTKEY_CLOSE < MAX_HOTKEY_ID; ++ID_HOTKEY_CLOSE) {
        result = RegisterHotKey(WindowHandle, ID_HOTKEY_CLOSE, (int)ModifierKeys.Alt, 0x2E);
        if (result != 0) {
          failed = false;
          break;
        }
      }
      if (failed) Close();
      // Register show/hide hotkey.
      failed = true;
      for (; ID_HOTKEY_VISIBILITY < MAX_HOTKEY_ID; ++ID_HOTKEY_VISIBILITY) {
        result = RegisterHotKey(WindowHandle, ID_HOTKEY_VISIBILITY, (int)ModifierKeys.Alt, 0x78);
        if (result != 0) {
          failed = false;
          break;
        }
      }
      if (failed) Close();
      // Subscribe hotkey event.
      ComponentDispatcher.ThreadPreprocessMessage += OnThreadPreprocessMessage;
    }

    private async void Execute()
    {
      // Show MainFrame.
      await MF.Show();

      // Adjustment statement.
      if (DateTime.Now.Second > 56) {
        DateTime dt;


        // Show waiting-text and play loading animation.
        await MP.Show();
        await MS.Show("SYNCHRONIZING");

        // Adjustment
        dt = DateTime.Now;
        await Task.Delay(((60 - dt.Second) * 1000) + (1000 - dt.Millisecond));

        // Hide waiting-text and stop loading animation.
        await MS.Hide();
        await MP.Hide();
      }

      // Show time(date and clock).
      await MT.Show();

      // Enable hotkey.
      ValidHotkey = true;
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
      ComponentDispatcher.ThreadPreprocessMessage -= OnThreadPreprocessMessage;
      UnregisterHotKey(WindowHandle, ID_HOTKEY_CLOSE);
      UnregisterHotKey(WindowHandle, ID_HOTKEY_VISIBILITY);
    }

    //protected override void OnClosed(EventArgs e)
    //{
    //  base.OnClosed(e);
    //}

    private void OnThreadPreprocessMessage(ref MSG msg, ref bool handled)
    {
      if (msg.message != 0x0312) return;

      lock (EXLockHotkey) {
        if (EXProcessingHotkey || ValidHotkey == false) return;
        EXProcessingHotkey = true;
      }


      if (msg.wParam.ToInt32() == ID_HOTKEY_CLOSE) {
        if (IsVisible) {
          this.Dispatcher.Invoke(new Action(async () => {
            await MT.Hide();
            await MF.Hide();
            this.Hide();
            await MT.HidePost();
            this.Close();
          }));
        }
        else {
          lock (EXLockHotkey) {
            EXProcessingHotkey = false;
          }
        }
      }
      else if (msg.wParam.ToInt32() == ID_HOTKEY_VISIBILITY) {
        this.Dispatcher.Invoke(new Action(async () => {
          if (IsVisible) {
            await MT.Hide();
            await MF.Hide();
            this.Hide();
            await MT.HidePost();
          }
          else {
            this.Show();
            await MF.Show();

            // Synchronization.
            if (DateTime.Now.Second > 56) {
              DateTime dt = DateTime.Now;


              await MP.Show();
              await MS.Show("SYNCHRONIZING");

              dt = DateTime.Now;
              await Task.Delay(((60 - dt.Second) * 1000) + (1000 - dt.Millisecond));


              await MS.Hide();
              await MP.Hide();
            }

            await MT.Show();
          }


          lock (EXLockHotkey) {
            EXProcessingHotkey = false;
          }
        }));
      }
    }
  }
}
