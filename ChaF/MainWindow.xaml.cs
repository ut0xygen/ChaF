using System;
using System.Reflection;
using System.Threading;
using System.Windows;


namespace ChaF
{
  public partial class MainWindow : Window
  {
    private Mutex MutexHandle {
      get;
    }
    private bool MutexOwned {
      get;
    }


    public MainWindow()
    {
      try {
        MutexHandle = new Mutex(false, Assembly.GetExecutingAssembly().GetName().Name);
        MutexOwned = MutexHandle.WaitOne(0, false);
      }
      catch (AbandonedMutexException) {
        MutexOwned = true;
      }
      if (MutexOwned == false) {
        MessageBox.Show("Already running.", "ChaF", MessageBoxButton.OK, MessageBoxImage.Error);
        this.Close();
      }

      InitializeComponent();

      Loaded += (sender, e) =>
      {
        Setup();
        Execute();
      };
    }

    protected override void OnClosed(EventArgs e)
    {
      if (MutexOwned) MutexHandle.ReleaseMutex();
      base.OnClosed(e);
    }
  }
}
