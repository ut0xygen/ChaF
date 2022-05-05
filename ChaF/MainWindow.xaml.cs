using System;
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
    /// <summary>
    ///   Constructor.
    /// </summary>
    public MainWindow()
    {
      InitializeComponent();

      Loaded += (sender, e) =>
      {
        Setup();
        Execute();
      };
    }
  }
}
