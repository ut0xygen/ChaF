using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace ChaF
{
  partial class MainWindow
  {
    private Storyboard _sbMainTime = new Storyboard();
    private Storyboard _sbMainTimeDate = new Storyboard();
    private Storyboard _sbMainTimeDateTextBackground = new Storyboard();
    private Storyboard _sbMainTimeClock = new Storyboard();
    private Storyboard _sbMainTimeClockBackground = new Storyboard();
    private Storyboard _sbMainTimeClockHourTextBackground = new Storyboard();
    private Storyboard _sbMainTimeClockMinuteTextBackground = new Storyboard();
    private Storyboard _sbMainTimeClockSecondTextBackground = new Storyboard();
    private DoubleAnimation _aMainTime = new DoubleAnimation(1.0, 0.0, _DURATION_SEC_FADE_OUT);
    private DoubleAnimation _aMainTimeDate = new DoubleAnimation(1.0, 0.0, _DURATION_SEC_FADE_OUT);
    private DoubleAnimation _aMainTimeDateBackground = new DoubleAnimation(_BACKGROUND_FROM, _BACKGROUND_TO, _DURATION_SEC_BLINK);
    private DoubleAnimation _aMainTimeClock = new DoubleAnimation(1.0, 0.0, _DURATION_SEC_FADE_OUT);
    private DoubleAnimation _aMainTimeClockBackground = new DoubleAnimation(_BACKGROUND_FROM, _BACKGROUND_TO, _DURATION_SEC_BLINK);
    private DoubleAnimation _aMainTimeClockHourTextBackground = new DoubleAnimation(_BACKGROUND_FROM, _BACKGROUND_TO, _DURATION_SEC_BLINK);
    private DoubleAnimation _aMainTimeClockMinuteTextBackground = new DoubleAnimation(_BACKGROUND_FROM, _BACKGROUND_TO, _DURATION_SEC_BLINK);
    private DoubleAnimation _aMainTimeClockSecondTextBackground = new DoubleAnimation(_BACKGROUND_FROM, _BACKGROUND_TO, _DURATION_SEC_BLINK);

    private bool _isRequestedStopMainTimeClock = false;
    private Task _taskMainTimeClock = Task.CompletedTask;


    /// <summary>
    ///   Initializes objects related MainTime.
    /// </summary>
    private void InitializeMainTime()
    {
      MainTime.Width = Main.Width * 0.9;  // If size of Viewbox is auto, then forced to parent size fit?
      MainTime.Height = Main.Height * 0.9;
      MainTimeFrame.Width = MainTime.Width;  // If size of Grid is auto too, then forced to parent size fit?
      MainTimeFrame.Height = MainTime.Height;
      MainTimeDateTextBackground.Color = Colors.Cyan;
      MainTimeClockBackground.Color = Colors.Cyan;
      MainTimeClockHourTextBackground.Color = Colors.Cyan;
      MainTimeClockMinuteTextBackground.Color = Colors.Cyan;
      MainTimeClockSecondTextBackground.Color = Colors.Cyan;

      _sbMainTime.Children.Add(_aMainTime);
      Storyboard.SetTargetName(_sbMainTime, MainTime.Name);
      Storyboard.SetTargetProperty(_sbMainTime, new PropertyPath(Viewbox.OpacityProperty));
      _sbMainTimeDate.Children.Add(_aMainTimeDate);
      Storyboard.SetTargetName(_aMainTimeDate, MainTimeDate.Name);
      Storyboard.SetTargetProperty(_aMainTimeDate, new PropertyPath(Viewbox.OpacityProperty));
      _sbMainTimeDateTextBackground.Children.Add(_aMainTimeDateBackground);
      Storyboard.SetTargetName(_aMainTimeDateBackground, "MainTimeDateTextBackground");
      Storyboard.SetTargetProperty(_aMainTimeDateBackground, new PropertyPath(Brush.OpacityProperty));
      _sbMainTimeClock.Children.Add(_aMainTimeClock);
      Storyboard.SetTargetName(_aMainTimeClock, MainTimeClock.Name);
      Storyboard.SetTargetProperty(_aMainTimeClock, new PropertyPath(Viewbox.OpacityProperty));
      _sbMainTimeClockBackground.Children.Add(_aMainTimeClockBackground);
      Storyboard.SetTargetName(_aMainTimeClockBackground, "MainTimeClockBackground");
      Storyboard.SetTargetProperty(_aMainTimeClockBackground, new PropertyPath(Brush.OpacityProperty));
      _sbMainTimeClockHourTextBackground.Children.Add(_aMainTimeClockHourTextBackground);
      Storyboard.SetTargetName(_aMainTimeClockHourTextBackground, "MainTimeClockHourTextBackground");
      Storyboard.SetTargetProperty(_aMainTimeClockHourTextBackground, new PropertyPath(Brush.OpacityProperty));
      _sbMainTimeClockMinuteTextBackground.Children.Add(_aMainTimeClockMinuteTextBackground);
      Storyboard.SetTargetName(_aMainTimeClockMinuteTextBackground, "MainTimeClockMinuteTextBackground");
      Storyboard.SetTargetProperty(_aMainTimeClockMinuteTextBackground, new PropertyPath(Brush.OpacityProperty));
      _sbMainTimeClockSecondTextBackground.Children.Add(_aMainTimeClockSecondTextBackground);
      Storyboard.SetTargetName(_aMainTimeClockSecondTextBackground, "MainTimeClockSecondTextBackground");
      Storyboard.SetTargetProperty(_aMainTimeClockSecondTextBackground, new PropertyPath(Brush.OpacityProperty));
    }

    /// <summary>
    ///   Show MainClock.
    /// </summary>
    /// <returns></returns>
    private async Task ShowMainClock()
    {
      async Task ShowMainTimeDate()
      {
        SemaphoreSlim ss = new SemaphoreSlim(0, 1);
        string text = DateTime.Now.ToString("yyyy年MM月dd日(ddd)");

        void OnCompleted(object sender, EventArgs e)
        {
          _sbMainTimeDateTextBackground.Completed -= OnCompleted;
          ss.Release();
        }


        // Reset.
        _dcMain.TimeDateTextWidth = 0;
        _dcMain.TimeDateText = text;
        MainTimeDateText.GetBindingExpression(TextBlock.WidthProperty).UpdateTarget();
        MainTimeDateText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        await Task.Delay(1);
        _dcMain.TimeDateTextWidth = MainTimeDateText.ActualWidth;
        _dcMain.TimeDateText = string.Empty;
        MainTimeDateText.GetBindingExpression(TextBlock.WidthProperty).UpdateTarget();
        MainTimeDateText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        await Task.Delay(1);
        _sbMainTimeDate.Stop(this);
        // Show.
        _dcMain.TimeDateVisibility = Visibility.Visible;
        MainTimeDate.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
        await Task.Delay(1);

        for (int i = 0; ;) {
          _dcMain.TimeDateText += text[i];
          MainTimeDateText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();

          ++i;
          if (i >= text.Length) {
            break;
          }
          await Task.Delay(_TEXT_FEED_TIME);
        }

        if (_blink) {
          _sbMainTimeDateTextBackground.Completed += OnCompleted;
          _sbMainTimeDateTextBackground.Begin(this, true);
          await ss.WaitAsync();
        }
      }

      async Task HideMainTimeDate(bool noAnimation = false)
      {
        SemaphoreSlim ss = new SemaphoreSlim(0, 1);

        void OnCompletedHide(object sender, EventArgs e)
        {
          _sbMainTimeDate.Completed -= OnCompletedHide;

          ss.Release();
        }

        if (noAnimation == false) {
          _sbMainTimeDate.Completed += OnCompletedHide;
          _sbMainTimeDate.Begin(this, true);
          await ss.WaitAsync();
        }
        _dcMain.TimeDateVisibility = Visibility.Hidden;
        MainTimeDate.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
        _sbMainTimeDate.Stop(this);
        _sbMainTimeDateTextBackground.Stop(this);
      }

      async Task ShowMainTimeClock()
      {
        string textHour;
        string textMinute;
        string textSecond;
        DateTime dtOld = new DateTime();
        SemaphoreSlim ss = new SemaphoreSlim(0, 1);

        void OnCompleted(object sender, EventArgs e)
        {
          _sbMainTimeClockBackground.Completed -= OnCompleted;
          ss.Release();
        }

        async Task Update()
        {
          DateTime dtCurrent;

          async Task UpdateDate()
          {
            await HideMainTimeDate();
            await ShowMainTimeDate();
          }

          async Task UpdateClockHour()
          {
            SemaphoreSlim ss_ = new SemaphoreSlim(0, 1);

            void OnCompleted_(object sender, EventArgs e)
            {
              _sbMainTimeClockHourTextBackground.Completed -= OnCompleted_;
              ss_.Release();
            }


            _dcMain.TimeClockHourText = dtCurrent.ToString("HH");
            MainTimeClockHourText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            if (_blink) {
              _sbMainTimeClockHourTextBackground.Completed += OnCompleted_;
              _sbMainTimeClockHourTextBackground.Begin(this, true);
              await ss.WaitAsync();
            }
          }

          async Task UpdateClockMinute()
          {
            SemaphoreSlim ss_ = new SemaphoreSlim(0, 1);

            void OnCompleted_(object sender, EventArgs e)
            {
              _sbMainTimeClockMinuteTextBackground.Completed -= OnCompleted_;
              ss_.Release();
            }


            _dcMain.TimeClockMinuteText = dtCurrent.ToString("mm");
            MainTimeClockMinuteText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            if (_blink) {
              _sbMainTimeClockMinuteTextBackground.Completed += OnCompleted_;
              _sbMainTimeClockMinuteTextBackground.Begin(this, true);
              await ss.WaitAsync();
            }
          }

          async Task UpdateClockSecond()
          {
            SemaphoreSlim ss_ = new SemaphoreSlim(0, 1);

            void OnCompleted_(object sender, EventArgs e)
            {
              _sbMainTimeClockSecondTextBackground.Completed -= OnCompleted_;
              ss_.Release();
            }

            _dcMain.TimeClockSecondText = dtCurrent.ToString("ss");
            MainTimeClockSecondText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            if (_blink) {
              _sbMainTimeClockSecondTextBackground.Completed += OnCompleted_;
              _sbMainTimeClockSecondTextBackground.Begin(this, true);
              await ss.WaitAsync();
            }
          }


          while (true) {
            await Task.Delay(1000 - DateTime.Now.Millisecond);

            dtCurrent = DateTime.Now;

            if (dtCurrent.Day != dtOld.Day) {
              _ = UpdateDate();
            }
            if (dtCurrent.Hour != dtOld.Hour) {
              _ = UpdateClockHour();
            }
            if (dtCurrent.Minute != dtOld.Minute) {
              _ = UpdateClockMinute();
            }
            if (dtCurrent.Second != dtOld.Second) {
              _ = UpdateClockSecond();
            }

            if (_isRequestedStopMainTimeClock) {
              break;
            }

            dtOld = dtCurrent;
          }

          await HideMainTimeDate(true);
          _dcMain.TimeClockVisibility = Visibility.Hidden;
          MainTimeClock.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
          _sbMainTimeClockHourTextBackground.Stop(this);
          _sbMainTimeClockMinuteTextBackground.Stop(this);
          _sbMainTimeClockSecondTextBackground.Stop(this);
          _isRequestedStopMainTimeClock = false;
        }

        // Reset.
        _dcMain.TimeClockHourText = string.Empty;
        _dcMain.TimeClockMinuteText = string.Empty;
        _dcMain.TimeClockSecondText = string.Empty;
        _dcMain.TimeClockColonAVisibility = Visibility.Hidden;
        _dcMain.TimeClockColonBVisibility = Visibility.Hidden;
        MainTimeClockHourText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        MainTimeClockMinuteText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        MainTimeClockSecondText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        MainTimeClockColonA.GetBindingExpression(TextBlock.VisibilityProperty).UpdateTarget();
        MainTimeClockColonB.GetBindingExpression(TextBlock.VisibilityProperty).UpdateTarget();
        await Task.Delay(1);
        _sbMainTimeClock.Stop(this);
        // Show.
        _dcMain.TimeClockVisibility = Visibility.Visible;
        MainTimeClock.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
        await Task.Delay(1);

        dtOld = DateTime.Now;
        textHour = dtOld.ToString("HH");
        for (int i = 0; i < textHour.Length; ++i) {
          _dcMain.TimeClockHourText += textHour[i];
          MainTimeClockHourText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
          await Task.Delay(_TEXT_FEED_TIME);
        }

        _dcMain.TimeClockColonAVisibility = Visibility.Visible;
        MainTimeClockColonA.GetBindingExpression(TextBlock.VisibilityProperty).UpdateTarget();
        await Task.Delay(_TEXT_FEED_TIME);

        textMinute = dtOld.ToString("mm");
        for (int i = 0; i < textMinute.Length; ++i) {
          _dcMain.TimeClockMinuteText += textMinute[i];
          MainTimeClockMinuteText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
          await Task.Delay(_TEXT_FEED_TIME);
        }

        textSecond = DateTime.Now.ToString("ss");
        _dcMain.TimeClockColonBVisibility = Visibility.Visible;
        MainTimeClockColonB.GetBindingExpression(TextBlock.VisibilityProperty).UpdateTarget();
        await Task.Delay(_TEXT_FEED_TIME);

        for (int i = 0; ;) {
          _dcMain.TimeClockSecondText += textSecond[i];
          MainTimeClockSecondText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();

          ++i;
          if (i >= textSecond.Length) {
            break;
          }
          await Task.Delay(_TEXT_FEED_TIME);
        }


        if (_blink) {
          _sbMainTimeClockBackground.Completed += OnCompleted;
          _sbMainTimeClockBackground.Begin(this, true);
          await ss.WaitAsync();
        }

        _taskMainTimeClock = Update();
      }


      // Show.
      _dcMain.TimeVisibility = Visibility.Visible;
      MainTime.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
      await Task.Delay(1);
      await ShowMainTimeDate();
      await ShowMainTimeClock();
    }

    /// <summary>
    ///   Hide MainClock.
    /// </summary>
    /// <param name="noAnimation"></param>
    /// <returns></returns>
    private async Task HideMainTime(bool noAnimation = false)
    {
      if (_taskMainTimeClock.IsCompleted) {
        return;
      }


      SemaphoreSlim ss = new SemaphoreSlim(0, 1);

      void OnCompleted(object sender, EventArgs e)
      {
        _sbMainTime.Completed -= OnCompleted;
        ss.Release();
      }

      if (noAnimation == false) {
        _sbMainTime.Completed += OnCompleted;
        _sbMainTime.Begin(this, true);
        await ss.WaitAsync();
      }
      _dcMain.TimeVisibility = Visibility.Hidden;
      MainTime.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
      _sbMainTime.Stop(this);
    }

    /// <summary>
    ///   Please to call after HideMainTime() is finished.
    /// </summary>
    /// <returns></returns>
    private async Task HideMainTime_()
    {
      _isRequestedStopMainTimeClock = true;
      await _taskMainTimeClock;
    }
  }
}