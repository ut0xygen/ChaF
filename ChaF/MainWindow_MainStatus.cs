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
    private class MainStatus_
    {
      private static readonly PropertyPath PROPERTY_PATH_OPACITY = new PropertyPath(Viewbox.OpacityProperty);
      private static readonly PropertyPath PROPERTY_PATH_BACKGROUND_OPACITY = new PropertyPath("(0).(1)", new DependencyProperty[] { TextBlock.BackgroundProperty, SolidColorBrush.OpacityProperty });

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
      private Storyboard SB {
        get; set;
      }
      private DoubleAnimation A {
        get; set;
      }
      private Storyboard SBBackground {
        get; set;
      }
      private DoubleAnimation ABackground {
        get; set;
      }
      private object EXLock {
        get; set;
      }
      private SemaphoreSlim EXSemaphore {
        get; set;
      }
      private bool EXProcessing {
        get; set;
      }
      private bool EXShown {
        get; set;
      }


      public MainStatus_(MainWindow window, FrameworkElement parent, FrameworkElement indirect, FrameworkElement target)
      {
        W = window;
        P = parent;
        I = indirect;
        T = target;
        SB = new Storyboard();
        A = new DoubleAnimation(1, 0, DURATION_HIDE);
        SBBackground = new Storyboard();
        ABackground = new DoubleAnimation(OPACITY_BLINK, 0, DURATION_BLINK);
        EXLock = new object();
        EXSemaphore = new SemaphoreSlim(0, 1);
        EXProcessing = false;
        EXShown = false;

        I.Width = P.Width * 0.8;
        I.Height = P.Height * 0.7;
        I.Margin = new Thickness(P.Width * 0.1, 0, 0, 0);
        SB.Children.Add(A);
        Storyboard.SetTargetName(A, I.Name);
        Storyboard.SetTargetProperty(A, PROPERTY_PATH_OPACITY);
        SBBackground.Children.Add(ABackground);
        Storyboard.SetTargetName(ABackground, T.Name);
        Storyboard.SetTargetProperty(ABackground, PROPERTY_PATH_BACKGROUND_OPACITY);
        SB.Completed += OnCompleted;
        SBBackground.Completed += OnCompleted;
      }


      void OnCompleted(object sender, EventArgs e)
      {
        EXSemaphore.Release();
      }

      public async Task Show(string text)
      {
        lock (EXLock) {
          if (EXProcessing || EXShown || text == string.Empty) return;
          EXProcessing = true;
        }


        // Reset.
        DataContextMain_.StatusTextWidth = 0;
        DataContextMain_.StatusText = text;
        T.GetBindingExpression(WidthProperty).UpdateTarget();
        T.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        await Task.Delay(1);
        DataContextMain_.StatusTextWidth = T.ActualWidth;
        DataContextMain_.StatusText = string.Empty;
        T.GetBindingExpression(WidthProperty).UpdateTarget();
        T.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
        await Task.Delay(1);
        // Show.
        DataContextMain_.StatusVisibility = Visibility.Visible;
        I.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(1);

        for (int i = 0; ;) {
          DataContextMain_.StatusText += text[i];
          T.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();

          ++i;
          if (i >= text.Length) {
            break;
          }
          await Task.Delay(TIME_TEXT_FEED);
        }
        if (W.EnableBlink) {
          SBBackground.Begin(W, true);
          await EXSemaphore.WaitAsync();
          SBBackground.Stop(W);
        }


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
        DataContextMain_.StatusVisibility = Visibility.Hidden;
        I.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(1);


        lock (EXLock) {
          EXShown = false;
          EXProcessing = false;
        }
      }
    };
  }
}
