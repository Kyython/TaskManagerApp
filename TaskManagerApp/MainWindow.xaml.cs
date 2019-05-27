using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Management;
using System.Windows;

namespace TaskManagerApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var processes = Process.GetProcesses();

            foreach (var process in processes)
            {
                string status = (process.Responding == true ? "Responding" : "Not responding");
                dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);

                processesDataGrid.Items.Add(new { process.ProcessName, process.Id, Status = status, extraProcessInfo.UserName, Memory = BytesToReadableValue(process.PrivateMemorySize64), extraProcessInfo.Description });
            }
        }

        private void KillProcessButton(object sender, RoutedEventArgs e)
        {
            if (processesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Процесс не выбран!");
            }
            else
            {
                string messageBoxText = "Are you sure you want to complete the process?";
                string caption = "Task manager";

                MessageBoxButton buttonMessageBox = MessageBoxButton.OKCancel;
                MessageBoxImage imageMessageBox = MessageBoxImage.Warning;

                MessageBoxResult resultMessageBox = MessageBox.Show(messageBoxText, caption, buttonMessageBox, imageMessageBox);

                switch (resultMessageBox)
                {
                    case MessageBoxResult.OK:

                        dynamic selectedProcessInformation = processesDataGrid.SelectedItem;
                        foreach (var process in Process.GetProcessesByName(selectedProcessInformation.ProcessName))
                        {
                            process.Kill();
                        }
                        break;

                    case MessageBoxResult.Cancel:
                        break;
                }

            }
        }

        public ExpandoObject GetProcessExtraInformation(int processId)
        {
            const int NULL = 0;
            string query = "Select * From Win32_Process Where ProcessID = " + processId;

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            dynamic response = new ExpandoObject();
            response.Description = "Unknown";
            response.UserName = "Unknown";

            foreach (ManagementObject managmentObject in processList)
            {
                string[] argumentList = new string[] { string.Empty, string.Empty };

                int returnValue = Convert.ToInt32(managmentObject.InvokeMethod("GetOwner", argumentList));

                if (returnValue == NULL)
                {
                    response.UserName = argumentList[NULL];
                }

                if (managmentObject["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(managmentObject["ExecutablePath"].ToString());
                        response.Description = info.FileDescription;
                    }
                    catch(Exception)
                    {
                        continue;
                    }
                }
            }

            return response;
        }

        public string BytesToReadableValue(long number)
        {
            const int KILOBYTE_NUMBER = 2;
            const int BYTE_IN_KILOBYTE = 1024;
            const int NULL = 0;

            List<string> suffixes = new List<string> { " B", " KB", " MB", " GB", " TB", " PB" };
            
            for (int i = NULL; i < suffixes.Count; i++)
            {
                long templateNumber = number / (int)Math.Pow(BYTE_IN_KILOBYTE, i + KILOBYTE_NUMBER);

                if (templateNumber == NULL)
                {
                    return (number / (int)Math.Pow(BYTE_IN_KILOBYTE, i)) + suffixes[i];
                }
            }

            return number.ToString();
        }
    }
}
