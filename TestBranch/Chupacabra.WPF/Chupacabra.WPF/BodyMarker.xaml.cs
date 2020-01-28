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
    /// Interaction logic for BodyMarker.xaml
    /// </summary>
    public partial class BodyMarker : UserControl
    {

        public double x
        {
            get { return (double)this.GetValue(Canvas.LeftProperty); }
            set { this.SetValue(Canvas.LeftProperty, value); }
        }

        public double y
        {
            get { return (double)this.GetValue(Canvas.TopProperty); }
            set { this.SetValue(Canvas.TopProperty, value); }
        }

        public BodyMarker()
        {
            InitializeComponent();
        }
    }
}
