using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace ChaF
{
  partial class MainWindow
  {
    private class MainProgress_
    {
      private static readonly PropertyPath PROPERTY_PATH_WIDTH = new PropertyPath(Rectangle.WidthProperty);

      private MainWindow W {
        get;
      }
      private FrameworkElement P {
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

      private bool IsRequestedStop {
        get; set;
      }


      public MainProgress_(MainWindow window, FrameworkElement parent, FrameworkElement target)
      {
        W = window;
        P = parent;
        T = target;
        SB = new Storyboard();
        A = new DoubleAnimation(0, 0, DURATION_PROGRESS);
        EXLock = new object();
        EXSemaphore = new SemaphoreSlim(0, 1);
        EXProcessing = false;
        EXShown = false;

        IsRequestedStop = false;

        T.Height = P.Height * 0.05;
        T.Margin = new Thickness(0, 0, 0, T.Height);
        SB.Children.Add(A);
        Storyboard.SetTargetName(A, T.Name);
        Storyboard.SetTargetProperty(A, PROPERTY_PATH_WIDTH);
      }


      async void OnCompletedStart(object sender, EventArgs e)
      {
        SB.Stop(W);

        T.HorizontalAlignment = HorizontalAlignment.Left;
        A.From = 0;
        A.To = P.Width;

        SB.Completed -= OnCompletedStart;

        if (IsRequestedStop == false) {
          await Task.Delay(200);

          SB.Completed += OnCompletedEnd;
          SB.Begin(W, true);
        }
        else {
          IsRequestedStop = false;
          EXSemaphore.Release();
        }
      }

      void OnCompletedEnd(object sender, EventArgs e)
      {
        SB.Stop(W);

        T.HorizontalAlignment = HorizontalAlignment.Right;
        A.From = P.Width;
        A.To = 0;

        SB.Completed -= OnCompletedEnd;
        SB.Completed += OnCompletedStart;
        SB.Begin(W, true);
      }

      public async Task Show()
      {
        lock (EXLock) {
          if (EXProcessing || EXShown) return;
          EXProcessing = true;
        }


        DataContextMain_.ProgressVisibility = Visibility.Visible;
        T.GetBindingExpression(VisibilityProperty).UpdateTarget();
        await Task.Delay(1);

        T.HorizontalAlignment = HorizontalAlignment.Left;
        A.From = 0;
        A.To = P.Width;
        SB.Completed += OnCompletedEnd;
        SB.Begin(W, true);


        lock (EXLock) {
          EXShown = true;
          EXProcessing = false;
        }
      }

      public async Task Hide()
      {
        lock (EXLock) {
          if (EXProcessing || EXShown == false) return;
          EXProcessing = true;
        }


        IsRequestedStop = true;
        await EXSemaphore.WaitAsync();
        DataContextMain_.ProgressVisibility = Visibility.Hidden;
        T.GetBindingExpression(VisibilityProperty).UpdateTarget();


        lock (EXLock) {
          EXShown = false;
          EXProcessing = false;
        }
      }
    };
  }
}
