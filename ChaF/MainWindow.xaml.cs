using System;
using System.Runtime;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;


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
