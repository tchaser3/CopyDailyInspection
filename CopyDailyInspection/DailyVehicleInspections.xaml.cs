/* Title:           Daily Vehicle Inspections
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
using NewEventLogDLL;
using DataValidationDLL;
using NewVehicleDLL;
using VehicleHistoryDLL;
using DateSearchDLL;
using NewEmployeeDLL;

namespace CopyDailyInspection
{
    /// <summary>
    /// Interaction logic for DailyVehicleInspections.xaml
    /// </summary>
    public partial class DailyVehicleInspections : Window
    {
        //setting up the class
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        InspectionsClass TheInspectionsClass = new InspectionsClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        DataValidationClass TheDataValidationClass = new DataValidationClass();
        VehicleClass TheVehicleClass = new VehicleClass();
        VehicleHistoryClass TheVehicleHistoryClass = new VehicleHistoryClass();
        DateSearchClass TheDateSearchClass = new DateSearchClass();
        EmployeeClass TheEmployeeClass = new EmployeeClass();

        FindActiveVehicleByBJCNumberDataSet TheFindActiveVehicleByBJCNumber = new FindActiveVehicleByBJCNumberDataSet();
        FindVehicleHistoryByVehicleIDAndDateRangeDataSet TheFindVehicleHistoryByVehicleIDAndDateRangeDataSet = new FindVehicleHistoryByVehicleIDAndDateRangeDataSet();
        FindDailyVehicleInspectionDateMatchDataSet TheFindDailyVehicleInspectionDateMatch = new FindDailyVehicleInspectionDateMatchDataSet();
        DailyVehicleInspectionsDataSet TheDailyVehicleInspectionsDataSet;
        FindInspectionIDForDailyInspectionDataSet TheFindInspectionIDForDailyInspectionDataSet = new FindInspectionIDForDailyInspectionDataSet();
        FindVehicleInspectionProblemsByInspectionIDDataSet TheFindVehicleInspectionProblemsByInspectionIDDataSet = new FindVehicleInspectionProblemsByInspectionIDDataSet();

        //setting access data
        VehicleInventorySheetDataSet TheVehicleInventorySheetDataSet;
        VehicleInventorySheetDataSetTableAdapters.vehicleinventorysheetTableAdapter TheVehicleInventorySheetTableAdapter;
        VerifyEmployeeDataSet TheVerifyEmployeeDataSet = new VerifyEmployeeDataSet();
        

        public DailyVehicleInspections()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
            
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this will load up the data set;
            try
            {
                TheVehicleInventorySheetDataSet = new VehicleInventorySheetDataSet();
                TheVehicleInventorySheetTableAdapter = new VehicleInventorySheetDataSetTableAdapters.vehicleinventorysheetTableAdapter();
                TheVehicleInventorySheetTableAdapter.Fill(TheVehicleInventorySheetDataSet.vehicleinventorysheet);

                TheDailyVehicleInspectionsDataSet = TheInspectionsClass.GetDailyVehicleInspectionsInfo();

                dgrInspections.ItemsSource = TheDailyVehicleInspectionsDataSet.dailyvehicleinspection;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Copy Daily Inspection // Daily Vehicle Inspection // Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //this will process the information
            //setting local variables
            int intCounter;
            int intNumberOfRecords;
            int intTransactionID;
            int intVehicleID;
            DateTime datInspectionDate;
            DateTime datTempDate;
            DateTime datLimitingDate;
            int intBJCNumber;
            int intEmployeeID;
            string strInspectionStatus = "";
            string strNotes;
            string strProblemReported;
            string strProblemCritical;
            int intOdometerReading;
            int intRecordsReturned;
            int intEmployeeCounter;
            string strFirstName;
            string strLastName;
            bool blnProblemEntry = false;
            bool blnServicability = true;
            DateTime datSearchDate = DateTime.Now;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            //try catch for exceptions
            try
            {
                //getting the record count
                intNumberOfRecords = TheVehicleInventorySheetDataSet.vehicleinventorysheet.Rows.Count - 1;
                datSearchDate = TheDateSearchClass.RemoveTime(datSearchDate);
                datSearchDate = TheDateSearchClass.SubtractingDays(datSearchDate, 30);

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    intBJCNumber = TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].BJCNumber;
                    datInspectionDate = TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].Date;
                    datTempDate = TheDateSearchClass.RemoveTime(datInspectionDate);
                    datLimitingDate = TheDateSearchClass.AddingDays(datTempDate, 1);
                    intTransactionID = TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].TransactionID;
                    blnProblemEntry = false;

                    if(datInspectionDate > datSearchDate)
                    {
                        if (TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].IsNotesNull() == true)
                        {
                            strNotes = "NONE ENTERED";
                        }
                        else
                        {
                            strNotes = TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].Notes;
                        }
                        intOdometerReading = TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].OdometerReading;
                        strProblemReported = TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].ProblemReported;
                        strProblemCritical = TheVehicleInventorySheetDataSet.vehicleinventorysheet[intCounter].ProblemCritical;

                        if (strProblemReported == "NO")
                        {
                            strInspectionStatus = "PASSED";
                        }
                        else if ((strProblemReported == "YES") && (strProblemCritical == "NO"))
                        {
                            strInspectionStatus = "PASSED SERVICE REQUIRED";
                            blnProblemEntry = true;
                            blnServicability = true;
                        }
                        else if ((strProblemReported == "YES") && (strProblemCritical == "YES"))
                        {
                            strInspectionStatus = "FAILED";
                            blnProblemEntry = true;
                            blnServicability = false;
                        }

                        //getting the active vehicle
                        TheFindActiveVehicleByBJCNumber = TheVehicleClass.FindActiveVehicleByBJCNumber(intBJCNumber);

                        intRecordsReturned = TheFindActiveVehicleByBJCNumber.FindActiveVehicleByBJCNumber.Rows.Count;

                        if (intRecordsReturned == 1)
                        {
                            intVehicleID = TheFindActiveVehicleByBJCNumber.FindActiveVehicleByBJCNumber[0].VehicleID;

                            //getting vehicle history
                            TheFindVehicleHistoryByVehicleIDAndDateRangeDataSet = TheVehicleHistoryClass.FindVehicleHistoryByVehicleIDAndDateRange(intVehicleID, datTempDate, datLimitingDate);

                            intRecordsReturned = TheFindVehicleHistoryByVehicleIDAndDateRangeDataSet.FindVehicleHistoryByVehicleIDAndDateRange.Rows.Count - 1;

                            if (intRecordsReturned > -1)
                            {
                                for (intEmployeeCounter = 0; intEmployeeCounter <= intRecordsReturned; intEmployeeCounter++)
                                {
                                    strLastName = TheFindVehicleHistoryByVehicleIDAndDateRangeDataSet.FindVehicleHistoryByVehicleIDAndDateRange[intEmployeeCounter].LastName;
                                    strFirstName = TheFindVehicleHistoryByVehicleIDAndDateRangeDataSet.FindVehicleHistoryByVehicleIDAndDateRange[intEmployeeCounter].FirstName;

                                    TheVerifyEmployeeDataSet = TheEmployeeClass.VerifyEmployee(strFirstName, strLastName);

                                    intEmployeeID = TheVerifyEmployeeDataSet.VerifyEmployee[0].EmployeeID;

                                    EnterDailyInspection(intVehicleID, datInspectionDate, intEmployeeID, strInspectionStatus, intOdometerReading);

                                    if (blnProblemEntry == true)
                                    {
                                        EnterVehicleProblem(intVehicleID, strNotes, intEmployeeID, strInspectionStatus, intOdometerReading, blnServicability, strNotes, datInspectionDate);
                                    }
                                }
                            }
                            else
                            {
                                intEmployeeID = 0;

                                EnterDailyInspection(intVehicleID, datInspectionDate, intEmployeeID, strInspectionStatus, intOdometerReading);

                                if (blnProblemEntry == true)
                                {
                                    EnterVehicleProblem(intVehicleID, strNotes, intEmployeeID, strInspectionStatus, intOdometerReading, blnServicability, strNotes, datInspectionDate);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "Copy Daily Inspection // Daily Vehicle Inspection // Process Button " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            TheDailyVehicleInspectionsDataSet = TheInspectionsClass.GetDailyVehicleInspectionsInfo();

            dgrInspections.ItemsSource = TheDailyVehicleInspectionsDataSet.dailyvehicleinspection;

            PleaseWait.Close();
        }
        private void EnterVehicleProblem(int intVehicleID, string strVehicleProblem, int intEmployeeID, string strInspectionStatus, int intOdometerReading, bool blnServicability, string strNotes, DateTime datTransactionDate)
        {
            bool blnFatalError = false;
            int intInspectionID = 0;
            int intRecordsReturned;

            TheFindInspectionIDForDailyInspectionDataSet = TheInspectionsClass.FindInspectionIDForDailyInspections(intVehicleID, datTransactionDate, intEmployeeID, strInspectionStatus, intOdometerReading);

            intInspectionID = TheFindInspectionIDForDailyInspectionDataSet.FindInspectionIDForDailyInspection[0].TransactionID;

            TheFindVehicleInspectionProblemsByInspectionIDDataSet = TheInspectionsClass.FindVehicleInspectionProblemsbyInspectionID(intInspectionID);

            intRecordsReturned = TheFindVehicleInspectionProblemsByInspectionIDDataSet.FindVehicleInspectionProblemsByInspectionID.Rows.Count;

            if (intRecordsReturned == 0)
            {
                blnFatalError = TheInspectionsClass.InsertVehicleInspectionProblem(intVehicleID, intInspectionID, "DAILY", strVehicleProblem, intOdometerReading, blnServicability, strNotes);

                if (blnFatalError == true)
                {
                    TheMessagesClass.ErrorMessage("There Has Been A Massive Problem, Contact ID");
                }
            }
        }
        private void EnterDailyInspection(int intVehicleID, DateTime datTransactionDate, int intEmployeeID, string strInspectionStatus, int intOdometerReading )
        {
            //setting local variable
            bool blnFatalError = false;
            int intRecordsReturned;

            TheFindInspectionIDForDailyInspectionDataSet = TheInspectionsClass.FindInspectionIDForDailyInspections(intVehicleID, datTransactionDate, intEmployeeID, strInspectionStatus, intOdometerReading);

            intRecordsReturned = TheFindInspectionIDForDailyInspectionDataSet.FindInspectionIDForDailyInspection.Rows.Count;
           
            if(intRecordsReturned == 0)
            {
                blnFatalError = TheInspectionsClass.InsertDailyVehicleInspection(intVehicleID, datTransactionDate, intEmployeeID, strInspectionStatus, intOdometerReading);

                if (blnFatalError == true)
                {
                    TheMessagesClass.ErrorMessage("There Has Been A Massive Problem, Contact ID");
                }
            }
        }
    }
}
