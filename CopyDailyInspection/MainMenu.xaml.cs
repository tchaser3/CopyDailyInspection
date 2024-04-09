/* Title:           Main Menu
 * Date:            6-26-17
 * Author:          Terry Holmes */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using InspectionsDLL;
using NewVehicleDLL;
using NewEventLogDLL;
using VehicleHistoryDLL;

namespace CopyDailyInspection
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Window
    {
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();

        public MainMenu()
        {
            InitializeComponent();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnCopyDailyInspection_Click(object sender, RoutedEventArgs e)
        {
            DailyVehicleInspections DailyVehicleInspections = new DailyVehicleInspections();
            DailyVehicleInspections.Show();
            Close();
        }

        private void btnAbout_Click(object sender, RoutedEventArgs e)
        {
            About About = new About();
            About.ShowDialog();
        }

        private void btnCopyWeeklyInspections_Click(object sender, RoutedEventArgs e)
        {
            WeeklyVehicleInspections WeeklyVehicleInspections = new WeeklyVehicleInspections();
            WeeklyVehicleInspections.Show();
            Close();
        }
    }
}
