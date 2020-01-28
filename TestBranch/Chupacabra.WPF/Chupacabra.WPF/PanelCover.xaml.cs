using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KinectoPhone.Desktop
{
    /// <summary>
    /// Interaction logic for PanelCover.xaml
    /// </summary>
    public partial class PanelCover : UserControl
    {
        public PanelCover()
        {
            InitializeComponent();
        }

        public Uri ImageFile
        {
            set { Main.Source = new BitmapImage(value); }
        }

    }
}
