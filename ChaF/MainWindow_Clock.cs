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
    private class MainTime_
    {
      private static readonly PropertyPath PROPERTY_PATH_OPACITY = new PropertyPath(Viewbox.OpacityProperty);
      private static readonly PropertyPath PROPERTY_PATH_TEXTBLOCK_BACKGROUND_OPACITY = new PropertyPath("(0).(1)", new DependencyProperty[] { TextBlock.BackgroundProperty, SolidColorBrush.OpacityProperty });

      private MainWindow W {
        get;
      }
      private FrameworkElement P {
        get;
      }
      private FrameworkElement I {
        get;
      }
      private FrameworkElement T {
        get;
      }
      private FrameworkElement TCAI {
        get;
      }
      private FrameworkElement TCA {
        get;
      }
      private FrameworkElement TCBI {
        get;
      }
      private FrameworkElement TCB {
        get;
      }
      private FrameworkElement TCBCA {
        get;
      }
      private FrameworkElement TCBCB {
        get;
      }
      private FrameworkElement TCBCC {
        get;
      }
      private FrameworkElement TCBCD {
        get;
      }
      private FrameworkElement TCBCE {
        get;
      }
      private Storyboard SB {
        get; set;
      }
      private DoubleAnimation A {
        get; set;
      }
      private Storyboard SBDate {
        get; set;
      }
      private DoubleAnimation ADate {
        get; set;
      }
      private Storyboard SBDateBackground {
        get; set;
      }
      private DoubleAnimation ADateBackgorund {
        get; set;
      }
      private Storyboard SBClock {
        get; set;
      }
      private DoubleAnimation AClock {
        get; set;
      }
      private Storyboard SBClockBackground {
        get; set;
      }
      private DoubleAnimation AClockBackground {
        get; set;
      }
      private Storyboard SBClockHourBackground {
        get; set;
      }
      private DoubleAnimation AClockHourBackground {
        get; set;
      }
      private Storyboard SBClockMinuteBackground {
        get; set;
      }
      private DoubleAnimation AClockMinuteBackground {
        get; set;
      }
      private Storyboard SBClockSecondBackground {
        get; set;
      }
      private DoubleAnimation AClockSecondBackground {
        get; set;
      }
      private object EXLock {
        get; set;
      }
      private SemaphoreSlim EXSemaphore {
        get; set;
      }
      private SemaphoreSlim EXSemaphoreHour {
        get; set;
      }
      private SemaphoreSlim EXSemaphoreMinute {
        get; set;
      }
      private SemaphoreSlim EXSemaphoreSecond {
        get; set;
      }
      private bool EXProcessing {
        get; set;
      }
      private bool EXShown {
        get; set;
      }

      private bool IsRequestedStop {
        get; set;
      }
      private Task TaskUpdate {
        get; set;
      }


      public MainTime_(MainWindow window, FrameworkElement parent, FrameworkElement indirect, FrameworkElement target, FrameworkElement targetChildAIndirect, FrameworkElement targetChildA, FrameworkElement targetChildBIndirect, FrameworkElement targetChildB, FrameworkElement targetChildBChildA, FrameworkElement targetChildBChildB, FrameworkElement targetChildBChildC, FrameworkElement targetChildBChildD, FrameworkElement targetChildBChildE)
      {
        W = window;
        P = parent;
        I = indirect;
        T = target;
        TCAI = targetChildAIndirect;
        TCA = targetChildA;
        TCBI = targetChildBIndirect;
        TCB = targetChildB;
        TCBCA = targetChildBChildA;
        TCBCB = targetChildBChildB;
        TCBCC = targetChildBChildC;
        TCBCD = targetChildBChildD;
        TCBCE = targetChildBChildE;
        SB = new Storyboard();
        A = new DoubleAnimation(1, 0, DURATION_HIDE);
        SBDate = new Storyboard();
        ADate = new DoubleAnimation(1, 0, DURATION_HIDE);
        SBDateBackground = new Storyboard();
        ADateBackgorund = new DoubleAnimation(OPACITY_BLINK, 0, DURATION_BLINK);
        SBClock = new Storyboard();
        AClock = new DoubleAnimation(1, 0, DURATION_HIDE);
        SBClockBackground = new Storyboard();
        AClockBackground = new DoubleAnimation(OPACITY_BLINK, 0, DURATION_BLINK);
        SBClockHourBackground = new Storyboard();
        AClockHourBackground = new DoubleAnimation(OPACITY_BLINK, 0, DURATION_BLINK);
        SBClockMinuteBackground = new Storyboard();
        AClockMinuteBackground = new DoubleAnimation(OPACITY_BLINK, 0, DURATION_BLINK);
        SBClockSecondBackground = new Storyboard();
        AClockSecondBackground = new DoubleAnimation(OPACITY_BLINK, 0, DURATION_BLINK);
        EXLock = new object();
        EXSemaphore = new SemaphoreSlim(0, 1);
        EXSemaphoreHour = new SemaphoreSlim(0, 1);
        EXSemaphoreMinute = new SemaphoreSlim(0, 1);
        EXSemaphoreSecond = new SemaphoreSlim(0, 1);
        EXProcessing = false;
        EXShown = false;

        IsRequestedStop = false;
        TaskUpdate = Task.CompletedTask;

        I.Width = P.Width * 0.9;  // If size of Viewbox is auto, then forced to parent size fit?
        I.Height = P.Height * 0.9;
        T.Width = I.Width;  // If size of Grid is auto too, then forced to parent size fit?
        T.Height = I.Height;
        SB.Children.Add(A);
        Storyboard.SetTargetName(A, I.Name);
        Storyboard.SetTargetProperty(A, PROPERTY_PATH_OPACITY);
        SBDate.Children.Add(ADate);
        Storyboard.SetTargetName(ADate, TCAI.Name);
        Storyboard.SetTargetProperty(ADate, PROPERTY_PATH_OPACITY);
        SBDateBackground.Children.Add(ADateBackgorund);
        Storyboard.SetTargetName(ADateBackgorund, TCA.Name);
        Storyboard.SetTargetProperty(ADateBackgorund, PROPERTY_PATH_TEXTBLOCK_BACKGROUND_OPACITY);
        SBClock.Children.Add(AClock);
        Storyboard.SetTargetName(AClock, TCBI.Name);
        Storyboard.SetTargetProperty(AClock, PROPERTY_PATH_OPACITY);
        SBClockBackground.Children.Add(AClockBackground);
        Storyboard.SetTargetName(AClockBackground, TCB.Name);
        Storyboard.SetTargetProperty(AClockBackground, new PropertyPath("(0).(1)", new DependencyProperty[] { StackPanel.BackgroundProperty, SolidColorBrush.OpacityProperty }));
        SBClockHourBackground.Children.Add(AClockHourBackground);
        Storyboard.SetTargetName(AClockHourBackground, TCBCA.Name);
        Storyboard.SetTargetProperty(AClockHourBackground, PROPERTY_PATH_TEXTBLOCK_BACKGROUND_OPACITY);
        SBClockMinuteBackground.Children.Add(AClockMinuteBackground);
        Storyboard.SetTargetName(AClockMinuteBackground, TCBCB.Name);
        Storyboard.SetTargetProperty(AClockMinuteBackground, PROPERTY_PATH_TEXTBLOCK_BACKGROUND_OPACITY);
        SBClockSecondBackground.Children.Add(AClockSecondBackground);
        Storyboard.SetTargetName(AClockSecondBackground, TCBCC.Name);
        Storyboard.SetTargetProperty(AClockSecondBackground, PROPERTY_PATH_TEXTBLOCK_BACKGROUND_OPACITY);
        SB.Completed += OnCompleted;
        SBDate.Completed += OnCompleted;
        SBDateBackground.Completed += OnCompleted;
        SBClock.Completed += OnCompleted;
        SBClockBackground.Completed += OnCompleted;
        SBClockHourBackground.Completed += OnCompletedHour;
        SBClockMinuteBackground.Completed += OnCompletedMinute;
        SBClockSecondBackground.Completed += OnCompletedSecond;
      }


      void OnCompleted(object sender, EventArgs e)
      {
        EXSemaphore.Release();
      }

      void OnCompletedHour(object sender, EventArgs e)
      {
        EXSemaphoreHour.Release();
      }

      void OnCompletedMinute(object sender, EventArgs e)
      {
        EXSemaphoreMinute.Release();
      }

      void OnCompletedSecond(object sender, EventArgs e)
      {
        EXSemaphoreSecond.Release();
      }


      public async Task Show()
      {
        lock (EXLock) {
          if (EXProcessing || EXShown) return;
          EXProcessing = true;
        }


        DataContextMain_.TimeVisibility = Visibility.Visible;
        I.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(1);

        await ShowDate();
        await ShowClock();


        lock (EXLock) {
          EXShown = true;
          EXProcessing = false;
        }
      }

      public async Task Hide(bool animation = true)
      {
        lock (EXLock) {
          if (EXProcessing || EXShown == false) return;
          EXProcessing = true;
        }


        if (animation) {
          SB.Begin(W, true);
          await EXSemaphore.WaitAsync();
          SB.Stop(W);
        }
        DataContextMain_.TimeVisibility = Visibility.Hidden;
        I.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
        await Task.Delay(1);


        lock (EXLock) {
          EXShown = false;
          EXProcessing = false;
        }
      }

      public async Task HidePost()
      {
        IsRequestedStop = true;
        await TaskUpdate;
      }

      async Task ShowDate()
      {
        string text = DateTime.Now.ToString("yyyy年MM月dd日(ddd)");

        // Reset.
        DataContextMain_.TimeDateTextWidth = 0;
        DataContextMain_.TimeDateText = text;
        TCA.GetBindingExpression(WidthProperty).UpdateTarget();
        TCA.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        await Task.Delay(1);
        DataContextMain_.TimeDateTextWidth = TCA.ActualWidth;
        DataContextMain_.TimeDateText = string.Empty;
        TCA.GetBindingExpression(WidthProperty).UpdateTarget();
        TCA.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        await Task.Delay(1);
        // Show.
        DataContextMain_.TimeDateVisibility = Visibility.Visible;
        TCAI.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(1);

        for (int i = 0; ;) {
          DataContextMain_.TimeDateText += text[i];
          TCA.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();

          ++i;
          if (i >= text.Length) {
            break;
          }
          await Task.Delay(TIME_TEXT_FEED);
        }

        if (W.EnableBlink) {
          SBDateBackground.Begin(W, true);
          await EXSemaphore.WaitAsync();
          SBDateBackground.Stop(W);
        }
      }

      async Task HideDate(bool animation = true)
      {
        if (animation) {
          SBDate.Begin(W, true);
          await EXSemaphore.WaitAsync();
          SBDate.Stop(W);
        }
        DataContextMain_.TimeDateVisibility = Visibility.Hidden;
        TCAI.GetBindingExpression(VisibilityProperty).UpdateTarget();
      }

      async Task ShowClock()
      {
        string textHour;
        string textMinute;
        string textSecond;
        DateTime dtOld;

        async Task Update()
        {
          DateTime dtCurrent;

          async Task UpdateDate()
          {
            await HideDate();
            await ShowDate();
          }

          async Task UpdateClockHour()
          {
            DataContextMain_.TimeClockHourText = dtCurrent.ToString("HH");
            TCBCA.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            if (W.EnableBlink) {
              SBClockHourBackground.Begin(W, true);
              await EXSemaphoreHour.WaitAsync();
              SBClockHourBackground.Stop(W);
            }
          }

          async Task UpdateClockMinute()
          {
            DataContextMain_.TimeClockMinuteText = dtCurrent.ToString("mm");
            TCBCB.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            if (W.EnableBlink) {
              SBClockMinuteBackground.Begin(W, true);
              await EXSemaphoreMinute.WaitAsync();
              SBClockMinuteBackground.Stop(W);
            }
          }

          async Task UpdateClockSecond()
          {
            DataContextMain_.TimeClockSecondText = dtCurrent.ToString("ss");
            TCBCC.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
            if (W.EnableBlink) {
              SBClockSecondBackground.Begin(W, true);
              await EXSemaphoreSecond.WaitAsync();
              SBClockSecondBackground.Stop(W);
            }
          }


          while (true) {
            await Task.Delay(1000 - DateTime.Now.Millisecond);

            dtCurrent = DateTime.Now;
            if (dtCurrent.Date != dtOld.Date) {
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
            dtOld = dtCurrent;

            if (IsRequestedStop) {
              break;
            }
          }

          await HideDate(false);
          DataContextMain_.TimeClockVisibility = Visibility.Hidden;
          DataContextMain_.TimeClockHourText = string.Empty;
          DataContextMain_.TimeClockMinuteText = string.Empty;
          DataContextMain_.TimeClockSecondText = string.Empty;
          DataContextMain_.TimeClockColonAVisibility = Visibility.Hidden;
          DataContextMain_.TimeClockColonBVisibility = Visibility.Hidden;
          TCBI.GetBindingExpression(VisibilityProperty).UpdateTarget();
          TCBCA.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
          TCBCB.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
          TCBCC.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
          TCBCD.GetBindingExpression(VisibilityProperty).UpdateTarget();
          TCBCE.GetBindingExpression(VisibilityProperty).UpdateTarget();
          await Task.Delay(1);
          IsRequestedStop = false;
        }


        DataContextMain_.TimeClockVisibility = Visibility.Visible;
        TCBI.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(1);

        dtOld = DateTime.Now;
        // Show hour.
        textHour = dtOld.ToString("HH");
        for (int i = 0; i < textHour.Length; ++i) {
          DataContextMain_.TimeClockHourText += textHour[i];
          TCBCA.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
          await Task.Delay(TIME_TEXT_FEED);
        }
        // Show first colon.
        DataContextMain_.TimeClockColonAVisibility = Visibility.Visible;
        TCBCD.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(TIME_TEXT_FEED);
        // Show minute.
        textMinute = dtOld.ToString("mm");
        for (int i = 0; i < textMinute.Length; ++i) {
          DataContextMain_.TimeClockMinuteText += textMinute[i];
          TCBCB.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
          await Task.Delay(TIME_TEXT_FEED);
        }
        // Show second colon.
        DataContextMain_.TimeClockColonBVisibility = Visibility.Visible;
        TCBCE.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(TIME_TEXT_FEED);
        // Show second.
        textSecond = DateTime.Now.ToString("ss");
        for (int i = 0; ;) {
          DataContextMain_.TimeClockSecondText += textSecond[i];
          TCBCC.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();

          ++i;
          if (i >= textSecond.Length) {
            break;
          }
          await Task.Delay(TIME_TEXT_FEED);
        }

        if (W.EnableBlink) {
          SBClockBackground.Begin(W, true);
          await EXSemaphore.WaitAsync();
        }

        TaskUpdate = Update();
      }
    };
  }
}