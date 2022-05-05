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
    private Storyboard _sbMainStatus = new Storyboard();
    private Storyboard _sbMainStatusTextBackground = new Storyboard();
    private DoubleAnimation _aMainStatus = new DoubleAnimation(1, 0.0, _DURATION_SEC_FADE_OUT);
    private DoubleAnimation _aMainStatusTextBackground = new DoubleAnimation(_BACKGROUND_FROM, _BACKGROUND_TO, _DURATION_SEC_BLINK);


    /// <summary>
    ///   Initializes objects related MainStatus.
    /// </summary>
    private void InitializeMainStatus()
    {
      MainStatus.Width = Main.Width * 0.8;
      MainStatus.Height = Main.Height * 0.7;
      MainStatus.Margin = new Thickness(Main.Width * 0.1, 0, 0, 0);
      MainStatusTextBackground.Color = Colors.Cyan;  // Color.FromArgb()で透明度を指定するとアニメーションが正常に動作しない。

      _sbMainStatus.Children.Add(_aMainStatus);
      Storyboard.SetTargetName(_sbMainStatus, MainStatus.Name);
      Storyboard.SetTargetProperty(_sbMainStatus, new PropertyPath(Viewbox.OpacityProperty));
      _sbMainStatusTextBackground.Children.Add(_aMainStatusTextBackground);
      Storyboard.SetTargetName(_aMainStatusTextBackground, "MainStatusTextBackground");
      Storyboard.SetTargetProperty(_aMainStatusTextBackground, new PropertyPath(SolidColorBrush.OpacityProperty));
    }

    /// <summary>
    ///   Show MainStatus.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private async Task ShowMainStatus(string text)
    {
      if (text == string.Empty) {
        throw new NullReferenceException("\"string text\" is empty.");
      }


      SemaphoreSlim ss = new SemaphoreSlim(0, 1);

      void OnCompleted(object sender, EventArgs e)
      {
        _sbMainStatusTextBackground.Completed -= OnCompleted;
        ss.Release();
      }


      // Reset.
      _dcMain.StatusTextWidth = 0;
      _dcMain.StatusText = text;
      MainStatusText.GetBindingExpression(TextBlock.WidthProperty).UpdateTarget();
      MainStatusText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
      await Task.Delay(1);
      _dcMain.StatusTextWidth = MainStatusText.ActualWidth;
      _dcMain.StatusText = string.Empty;
      MainStatusText.GetBindingExpression(TextBlock.WidthProperty).UpdateTarget();
      MainStatusText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
      await Task.Delay(1);
      _sbMainStatus.Stop(this);
      // Show.
      _dcMain.StatusVisibility = Visibility.Visible;
      MainStatus.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
      await Task.Delay(1);

      for (int i =0; ;) {
        _dcMain.StatusText += text[i];
        MainStatusText.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();

        ++i;
        if (i >= text.Length) {
          break;
        }
        await Task.Delay(_TEXT_FEED_TIME);
      }
      if (_blink) {
        _sbMainStatusTextBackground.Completed += OnCompleted;
        _sbMainStatusTextBackground.Begin(this, true);
        await ss.WaitAsync();
      }
    }

    /// <summary>
    ///   Hide MainStatus.
    /// </summary>
    /// <returns></returns>
    private async Task HideMainStatus(bool noAnimation = false)
    {
      SemaphoreSlim ss = new SemaphoreSlim(0, 1);

      void OnCompleted(object sender, EventArgs e)
      {
        _sbMainStatus.Completed -= OnCompleted;
        ss.Release();
      }


      if (noAnimation == false) {
        _sbMainStatus.Completed += OnCompleted;
        _sbMainStatus.Begin(this, true);
        await ss.WaitAsync();
      }
      _dcMain.StatusVisibility = Visibility.Hidden;
      MainStatus.GetBindingExpression(Viewbox.VisibilityProperty).UpdateTarget();
      _sbMainStatus.Stop(this);
      _sbMainStatusTextBackground.Stop(this);
    }
  }
}
