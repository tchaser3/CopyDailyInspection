/* Title:           Copy Weekly Vehicle Inspectioins
 * Date:            6-27-17
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
using WeeklyInspectionsDLL;
using NewEventLogDLL;
using NewEmployeeDLL;
using NewVehicleDLL;
using InspectionsDLL;
using DateSearchDLL;

namespace CopyDailyInspection
{
    /// <summary>
    /// Interaction logic for WeeklyVehicleInspections.xaml
    /// </summary>
    public partial class WeeklyVehicleInspections : Window
    {
        //setting up the classes
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        WeeklyInspectionClass TheWeeklyInspectionClass = new WeeklyInspectionClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();
        VehicleClass TheVehicleClass = new VehicleClass();
        InspectionsClass TheInspectionsClass = new InspectionsClass();
        DateSearchClass TheDataSearchClass = new DateSearchClass();

        FindActiveVehicleByBJCNumberDataSet TheActiveVehicleByBJCNumberDataSet = new FindActiveVehicleByBJCNumberDataSet();
        FindWeeklyVehicleInspectionIDDataSet TheFindWeeklyVehicleInspectionIDDataSet = new FindWeeklyVehicleInspectionIDDataSet();
        FindVehicleInspectionProblemsByInspectionIDDataSet TheFindVehicleInspectionProblemsByInspectionIDDataSet = new FindVehicleInspectionProblemsByInspectionIDDataSet();

        //setting up the data
        OldWeeklyVehicleInspectionsDataSet TheOldWeeklyVehicleInspectionsDataSet;
        OldWeeklyVehicleInspectionsDataSetTableAdapters.WeeklyVehicleInspectionsTableAdapter TheOldWeeklyVehicleInspectionsTableAdapter;
        WeeklyVehicleInspectionDataSet TheWeeklyVehicleInspectionDataSet;

        public WeeklyVehicleInspections()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //setting local variables
            int intCounter;
            int intNumberOfRecords;
            DateTime datInspectionDate;
            DateTime datLimitDate = DateTime.Now;
            int intEmployeeID;
            string strInspectionStatus = "";
            string strNotes;
            int intOdometerReading;
            int intVehicleID;
            int intRecordsReturned;
            bool blnProblemEntry = false;
            bool blnServicability = true;
            bool blnFatalError = false;
            int intInspectionID;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                intNumberOfRecords = TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections.Rows.Count - 1;
                datLimitDate = TheDataSearchClass.RemoveTime(datLimitDate);
                datLimitDate = TheDataSearchClass.SubtractingDays(datLimitDate, 30);

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    datInspectionDate = TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].InspectionDate;

                    if(datInspectionDate > datLimitDate)
                    {
                        if (TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].IsInspectionNotesNull() == true)
                        {
                            strNotes = "NOT PROVIDED";
                        }
                        else
                        {
                            strNotes = TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].InspectionNotes;
                        }
                        intEmployeeID = TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].EmployeeID;
                        intVehicleID = TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].VehicleID;
                        intOdometerReading = TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].CurrentOdometer;

                        if (strNotes == "NOT PROVIDED")
                        {
                            blnProblemEntry = false;
                            strInspectionStatus = "PASSED";
                        }
                        else if (strNotes != "NOT PROVIDED")
                        {
                            if (TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].VehicleServiceability == "YES")
                            {
                                blnProblemEntry = true;
                                strInspectionStatus = "PASSED SERVICE REQUIRED";
                                blnServicability = true;
                            }
                            if (TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections[intCounter].VehicleServiceability == "NO")
                            {
                                blnProblemEntry = true;
                                strInspectionStatus = "FAILED";
                                blnServicability = false;
                            }
                        }

                        TheFindWeeklyVehicleInspectionIDDataSet = TheWeeklyInspectionClass.FindWeelyVehicleInspectionID(intVehicleID, intEmployeeID, intOdometerReading, datInspectionDate);

                        intRecordsReturned = TheFindWeeklyVehicleInspectionIDDataSet.FindWeeklyVehicleInspectionID.Rows.Count;

                        if (intRecordsReturned == 0)
                        {
                            blnFatalError = TheWeeklyInspectionClass.InsertWeeklyVehicleInspection(intVehicleID, datInspectionDate, intEmployeeID, strInspectionStatus, intOdometerReading);

                            if (blnFatalError == true)
                            {
                                TheMessagesClass.ErrorMessage("There Has Been a Massive Problem, Contact IT");
                                return;
                            }

                            TheFindWeeklyVehicleInspectionIDDataSet = TheWeeklyInspectionClass.FindWeelyVehicleInspectionID(intVehicleID, intEmployeeID, intOdometerReading, datInspectionDate);
                        }

                        intInspectionID = TheFindWeeklyVehicleInspectionIDDataSet.FindWeeklyVehicleInspectionID[0].TransactionID;

                        if (blnProblemEntry == true)
                        {
                            TheFindVehicleInspectionProblemsByInspectionIDDataSet = TheInspectionsClass.FindVehicleInspectionProblemsbyInspectionID(intInspectionID);

                            intRecordsReturned = TheFindVehicleInspectionProblemsByInspectionIDDataSet.FindVehicleInspectionProblemsByInspectionID.Rows.Count;

                            if (intRecordsReturned == 0)
                            {
                                blnFatalError = TheInspectionsClass.InsertVehicleInspectionProblem(intVehicleID, intInspectionID, "WEEKLY", strNotes, intOdometerReading, blnServicability, strNotes);

                                if (blnFatalError == true)
                                {
                                    TheMessagesClass.ErrorMessage("There Has Been a Massive Problem, Contact IT");
                                    return;
                                }
                            }
                        }
                    }
                }

                TheWeeklyVehicleInspectionDataSet = TheWeeklyInspectionClass.GetWeeklyVehicleInspectionInfo();

                dgrInspections.ItemsSource = TheWeeklyVehicleInspectionDataSet.weeklyvehicleinspection;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Copy Daily Inspection // Weekly Vehicle Inspection // Process Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
            

            PleaseWait.Close();
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this will load the old data set
            try
            {
                TheOldWeeklyVehicleInspectionsDataSet = new OldWeeklyVehicleInspectionsDataSet();
                TheOldWeeklyVehicleInspectionsTableAdapter = new OldWeeklyVehicleInspectionsDataSetTableAdapters.WeeklyVehicleInspectionsTableAdapter();
                TheOldWeeklyVehicleInspectionsTableAdapter.Fill(TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections);

                dgrInspections.ItemsSource = TheOldWeeklyVehicleInspectionsDataSet.WeeklyVehicleInspections;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Copy Daily Inspection // Weekly Vehicle Inspections // Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
    }
}
