using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Xml.Linq;
using System.Management;
using System.Net;
using System.Windows.Controls;


namespace ipset
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string xmlFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ipset", "networkConfig.xml");

        public MainWindow()
        {
            InitializeComponent();

            // Ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(xmlFilePath));

            // Load existing configurations if any
            LoadConfigurations();

            // Populate Network Adapters ComboBox
            PopulateNetworkAdaptersComboBox();

            listBoxItems.SelectionChanged += listBoxItems_SelectionChanged;
        }

        private void LoadConfigurations()
        {
            if (File.Exists(xmlFilePath))
            {
                XDocument doc = XDocument.Load(xmlFilePath);
                var configurations = from config in doc.Descendants("Configuration")
                                     select new
                                     {
                                         Description = config.Element("Description").Value,
                                         IPAddress = config.Element("IPAddress").Value
                                     };

                foreach (var config in configurations)
                {
                    listBoxItems.Items.Add(new { Description = config.Description, IPAddress = config.IPAddress });
                }
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string description = txtDescription.Text;
            string ipAddress = txtIPAddress.Text;

            // Display in ListBox with separate items for description and IP address
            listBoxItems.Items.Add(new { Description = description, IPAddress = ipAddress });

            // Save to XML
            XDocument doc;
            if (File.Exists(xmlFilePath))
            {
                doc = XDocument.Load(xmlFilePath);
            }
            else
            {
                doc = new XDocument(new XElement("Configurations"));
            }

            XElement newConfig = new XElement("Configuration",
                                    new XElement("Description", description),
                                    new XElement("IPAddress", ipAddress));
            doc.Root.Add(newConfig);
            doc.Save(xmlFilePath);

            // Refresh ListBox
            listBoxItems.Items.Clear(); // Clear existing items
            LoadConfigurations(); // Reload configurations from XML
        }

        private void PopulateNetworkAdaptersComboBox()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                cmbNetworkAdapters.Items.Add(ni.Name);
            }
        }


        public static class NetworkHelper
        {
            public static void SetIPAddress(string interfaceName, string ipAddress)
            {
                var scope = new ManagementScope(@"\\.\ROOT\cimv2");
                var query = new SelectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Description='" + interfaceName + "'");
                var searcher = new ManagementObjectSearcher(scope, query);

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    if ((bool)queryObj["IPEnabled"])
                    {
                        ManagementBaseObject newIP = queryObj.GetMethodParameters("EnableStatic");
                        newIP["IPAddress"] = new string[] { ipAddress };
                        ManagementBaseObject setIP = queryObj.InvokeMethod("EnableStatic", newIP, null);
                    }
                }
            }
        }

        private void SetIPButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedAdapter = cmbNetworkAdapters.SelectedItem?.ToString();
                if (selectedAdapter != null)
                {
                    var selectedItem = (dynamic)listBoxItems.SelectedItem;
                    if (selectedItem != null)
                    {
                        string description = selectedItem.Description;
                        string ipAddress = selectedItem.IPAddress;

                        // Call the method to set the IP address
                        NetworkHelper.SetIPAddress(selectedAdapter, ipAddress);

                        // Show popup with confirmation message
                        string message = $"IP address for adapter {selectedAdapter} set to {ipAddress}";
                        MessageBox.Show(message, "IP Address Set", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Please select an item from the ListBox.");
                    }
                }
                else
                {
                    MessageBox.Show("Please select a network adapter.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void listBoxItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBoxItems.SelectedItem != null)
            {
                // Extract Description and IP Address from selected ListBox item
                var selectedItem = (dynamic)listBoxItems.SelectedItem;
                string description = selectedItem.Description;
                string ipAddress = selectedItem.IPAddress;

                // Display IP Address in the TextBlock
                txtSelectedIPAddress.Text = ipAddress;
            }
        }
    }
}

