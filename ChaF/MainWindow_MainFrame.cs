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
    private Storyboard _storyboardMainFrame = new Storyboard();
    private DoubleAnimation _animationMainFrame = new DoubleAnimation(0, 0, _DURATION_SEC_MAIN_FRAME);


    /// <summary>
    ///   Initializes objects related MainFrame.
    /// </summary>
    private void InitializeMainFrame()
    {
      MainFrame.Fill = Brushes.Black;
      MainFrame.Opacity = 0.6;
      _animationMainFrame.EasingFunction = _EASING_FUNCTION_EASE_IN_OUT;
      _storyboardMainFrame.Children.Add(_animationMainFrame);
      Storyboard.SetTargetName(_animationMainFrame, MainFrame.Name);
    }

    /// <summary>
    ///   Show MainFrame.
    /// </summary>
    /// <returns></returns>
    private async Task ShowMainFrame()
    {
      SemaphoreSlim ss = new SemaphoreSlim(0, 1);

      void OnCompleted(object sender, EventArgs e)
      {
        _storyboardMainFrame.Completed -= OnCompleted;
        ss.Release();
      }


      _animationMainFrame.From = 0;

      // If top or bottom.
      if (this.Left == 0 || this.Left.ToString() == _positionLeftUpper.ToString()) {
        Storyboard.SetTargetProperty(_animationMainFrame, new PropertyPath(Rectangle.WidthProperty));
        _animationMainFrame.To = this.Width;

        if (this.Left == 0) {
          MainFrame.HorizontalAlignment = HorizontalAlignment.Left;
        }
        else {
          MainFrame.HorizontalAlignment = HorizontalAlignment.Right;
        }
      }
      else {
        // If up-side or bottom-side.
        if (this.Top == 0 || this.Top.ToString() == _positionTopUpper.ToString()) {
          Storyboard.SetTargetProperty(_animationMainFrame, new PropertyPath(Rectangle.HeightProperty));
          _animationMainFrame.To = this.Height;

          if (this.Top == 0) {
            MainFrame.VerticalAlignment = VerticalAlignment.Top;
          }
          else {
            MainFrame.VerticalAlignment = VerticalAlignment.Bottom;
          }
        }
      }

      _storyboardMainFrame.Completed += OnCompleted;
      _storyboardMainFrame.Begin(this, true);
      await ss.WaitAsync();
    }

    private async Task HideMainFrame()
    {
      SemaphoreSlim ss = new SemaphoreSlim(0, 1);

      void OnCompleted(object sender, EventArgs e)
      {
        _storyboardMainFrame.Completed -= OnCompleted;
        ss.Release();
      }


      _animationMainFrame.To = 0;

      // 画面左端・右端に位置する場合
      if (this.Left <= 0 || this.Left >= _positionLeftUpper) {
        Storyboard.SetTargetProperty(_animationMainFrame, new PropertyPath(Rectangle.WidthProperty));
        _animationMainFrame.From = this.Width;

        if (this.Left <= 0) {
          MainFrame.HorizontalAlignment = HorizontalAlignment.Left;
        }
        else {
          MainFrame.HorizontalAlignment = HorizontalAlignment.Right;
        }
      }
      else {
        // 画面上端・下端に位置する場合
        if (this.Top <= 0 || this.Top >= _positionTopUpper) {
          Storyboard.SetTargetProperty(_animationMainFrame, new PropertyPath(Rectangle.HeightProperty));
          _animationMainFrame.From = this.Height;

          if (this.Left <= 0) {
            MainFrame.VerticalAlignment = VerticalAlignment.Bottom;
          }
          else {
            MainFrame.VerticalAlignment = VerticalAlignment.Top;
          }
        }
      }

      _storyboardMainFrame.Completed += OnCompleted;
      _storyboardMainFrame.Begin(this, true);
      await ss.WaitAsync();
      _storyboardMainFrame.Stop(this);
    }
  }
}
