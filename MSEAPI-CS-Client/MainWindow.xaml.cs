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
using MSEAPI_CS;
using MSEAPI_CS.Models;

namespace MSEAPI_CS_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Instance Variables
        MSEMultiSurface mseMultiSurface;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            mseMultiSurface = new MSEMultiSurface();


            mseMultiSurface.Start();
        }

        private void GetDeviceInfoClick(object sender, RoutedEventArgs e)
        {
            MSEDevice mseDevice = new MSEDevice();
            mseDevice.Identifier = "ASE Lab iPad 3";
                
              //  mseMultiSurface.IntAirAct.Devices[0]
            mseMultiSurface.locate(mseDevice, new MSEMultiSurface.MSESingleDeviceHandler(delegate(MSEDevice device) {
                if (device != null)
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OutputTextBlock.Text = device.Identifier + "\n" + device.Location + "\n" + device.Orientation;
                    }));
                    
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OutputTextBlock.Text = "No Device Found";
                    }));

                }

            }), new MSEMultiSurface.MSEErrorHandler(delegate(Exception exception) {
                OutputTextBlock.Text = exception.ToString();
            }));

        }

    }
}
