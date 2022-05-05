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
    private class MainFrame_
    {
      private static readonly PropertyPath PROPERTY_PATH_WIDTH = new PropertyPath(Rectangle.WidthProperty);
      private static readonly PropertyPath PROPERTY_PATH_HEIGHT = new PropertyPath(Rectangle.HeightProperty);

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


      public MainFrame_(MainWindow window, FrameworkElement parent, FrameworkElement target)
      {
        W = window;
        P = parent;
        T = target;
        SB = new Storyboard();
        A = new DoubleAnimation(0, 0, DURATION_MAIN_FRAME);
        EXLock = new object();
        EXSemaphore = new SemaphoreSlim(0, 1);
        EXProcessing = false;
        EXShown = false;

        A.EasingFunction = _EASING_FUNCTION_EASE_IN_OUT;
        SB.Children.Add(A);
        Storyboard.SetTargetName(A, T.Name);
        SB.Completed += OnCompleted;
      }


      private void OnCompleted(object sender, EventArgs e)
      {
        EXSemaphore.Release();
      }

      public async Task Show()
      {
        lock (EXLock) {
          if (EXProcessing || EXShown) return;
          EXProcessing = true;
        }


        A.From = 0;
        if (W.Top == 0 || W.Top.ToString() == W.TopLimit.ToString()) {
          T.Width = P.Width;
          T.VerticalAlignment = (W.Top == 0) ? VerticalAlignment.Top : VerticalAlignment.Bottom;
          A.To = P.Height;
          Storyboard.SetTargetProperty(A, PROPERTY_PATH_HEIGHT);
        }
        else {
          T.Height = P.Height;
          T.HorizontalAlignment = (W.Left == 0) ? HorizontalAlignment.Left : HorizontalAlignment.Right;
          A.To = P.Width;
          Storyboard.SetTargetProperty(A, PROPERTY_PATH_WIDTH);
        }

        SB.Begin(W, true);
        await EXSemaphore.WaitAsync();


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


        A.To = 0;
        if (W.Top <= 0 || W.Top >= W.TopLimit) {
          A.From = P.Height;
          Storyboard.SetTargetProperty(A, PROPERTY_PATH_HEIGHT);
        }
        else {
          A.From = P.Width;
          Storyboard.SetTargetProperty(A, PROPERTY_PATH_WIDTH);
        }

        SB.Begin(W, true);
        await EXSemaphore.WaitAsync();
        SB.Stop(W);


        lock (EXLock) {
          EXShown = false;
          EXProcessing = false;
        }
      }
    };
  }
}
