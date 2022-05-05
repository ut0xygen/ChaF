using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;


namespace ChaF
{
  partial class MainWindow
  {
    private Storyboard _sbMainProgress = new Storyboard();
    private DoubleAnimation _aMainProgress = new DoubleAnimation(0, 0, _DURATION_SEC_PROGRESS);

    private bool _isStoppedMainProgress = true;
    private bool _isRequestedStopMainProgress = false;
    private SemaphoreSlim _ssMainProgress = new SemaphoreSlim(0, 1);


    /// <summary>
    ///   Initializes objects related MainProgress.
    /// </summary>
    private void InitializeMainProgress()
    {
      MainProgress.Fill = Brushes.Cyan;
      MainProgress.Height = Main.Height * 0.05;
      MainProgress.Margin = new Thickness(0, 0, 0, MainProgress.Height);
      MainProgress.Opacity = 0.7;
      _sbMainProgress.Children.Add(_aMainProgress);
      Storyboard.SetTargetName(_aMainProgress, MainProgress.Name);
      Storyboard.SetTargetProperty(_aMainProgress, new PropertyPath(Rectangle.WidthProperty));
    }

    /// <summary>
    ///   Show MainProgress.
    /// </summary>
    /// <returns></returns>
    private async Task ShowMainProgress()
    {
      async void OnCompleted(object sender, EventArgs e)
      {
        MainProgress.HorizontalAlignment = HorizontalAlignment.Left;
        _aMainProgress.From = 0;
        _aMainProgress.To = Main.Width;

        _sbMainProgress.Completed -= OnCompleted;

        if (_isRequestedStopMainProgress == false) {
          await Task.Delay(200);

          _sbMainProgress.Completed += OnCompletedMainProgressEnd;
          _sbMainProgress.Begin(this, true);
        }
        else {
          _isStoppedMainProgress = true;
          _isRequestedStopMainProgress = false;
          _ssMainProgress.Release();
        }
      }

      void OnCompletedMainProgressEnd(object sender, EventArgs e)
      {
        MainProgress.HorizontalAlignment = HorizontalAlignment.Right;
        _aMainProgress.From = Main.Width;
        _aMainProgress.To = 0;

        _sbMainProgress.Completed -= OnCompletedMainProgressEnd;
        _sbMainProgress.Completed += OnCompleted;
        _sbMainProgress.Begin(this, true);
      }


      if (_isStoppedMainProgress == false) {
        throw new NullReferenceException("\"MainProgress\" is executing.");
      }

      _isStoppedMainProgress = false;
      _dcMain.ProgressVisibility = Visibility.Visible;
      MainProgress.GetBindingExpression(Rectangle.VisibilityProperty).UpdateTarget();
      await Task.Delay(1);
      OnCompleted(null, null);
    }

    /// <summary>
    ///   Hide MainProgress.
    /// </summary>
    /// <returns></returns>
    private async Task HideMainProgress()
    {
      if (_isStoppedMainProgress) {
        return;
      }


      _isRequestedStopMainProgress = true;
      await _ssMainProgress.WaitAsync();
      _dcMain.ProgressVisibility = Visibility.Hidden;
      MainProgress.GetBindingExpression(Rectangle.VisibilityProperty).UpdateTarget();
      _sbMainProgress.Stop(this);
    }
  }
}
