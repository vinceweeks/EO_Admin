﻿using EO.ViewModels.ControllerModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModels.ControllerModels;
using ViewModels.DataModels;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for VendorPage.xaml
    /// </summary>
    public partial class VendorPage : Page
    {
        MainWindow wnd = Application.Current.MainWindow as MainWindow;
        ObservableCollection<VendorDTO> list3 = new ObservableCollection<VendorDTO>();

        public VendorPage()
        {
            InitializeComponent();

            GetVendorResponse response = GetVendors();

            foreach(VendorDTO dto in response.VendorList)
            {
                list3.Add(dto);
            }

            this.VendorListView.ItemsSource = list3;
        }

        public GetVendorResponse GetVendors()
        {
            GetVendorResponse response = new GetVendorResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                string jsonData = JsonConvert.SerializeObject(new GetPersonRequest());
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse =
                    client.PostAsync("api/Login/GetVendors",content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    response = JsonConvert.DeserializeObject<GetVendorResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving vendors");
                }
            }
            catch (Exception ex)
            {

            }
            return response;
        }

        public void AddVendor()
        {
            try
            {
                AddVendorRequest addVendorRequest = new AddVendorRequest();

                if(String.IsNullOrEmpty(this.VendorName.Text))
                {
                    MessageBox.Show("The least you can do is enter a name...");
                    return;
                }

                addVendorRequest.Vendor.VendorName = this.VendorName.Text;
                addVendorRequest.Vendor.VendorEmail = this.VendorEmail.Text;
                addVendorRequest.Vendor.VendorPhone = this.VendorPhone.Text;
                addVendorRequest.Vendor.StreetAddress = this.Address1.Text;
                addVendorRequest.Vendor.UnitAptSuite = this.Address2.Text;
                addVendorRequest.Vendor.City = this.City.Text;
                addVendorRequest.Vendor.State = this.State.Text;
                addVendorRequest.Vendor.ZipCode = this.Zip.Text;

                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                //client.DefaultRequestHeaders.Add("appkey", "myapp_key");
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", wnd.User + " : " + wnd.Pwd);

                string jsonData = JsonConvert.SerializeObject(addVendorRequest);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/AddVendor", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    GetVendorResponse apiResponse = JsonConvert.DeserializeObject<GetVendorResponse>(strData);

                    //if (apiResponse.Messages.Count > 0)
                    //{
                    //    StringBuilder sb = new StringBuilder();
                    //    foreach (KeyValuePair<string, List<string>> messages in apiResponse.Messages)
                    //    {
                    //        foreach (string msg in messages.Value)
                    //        {
                    //            sb.AppendLine(msg);
                    //        }
                    //    }

                    //    MessageBox.Show(sb.ToString());
                    //}
                    //else
                    {
                        if (apiResponse.VendorList.Count == 1)
                        {
                            ClearEditFields();

                            list3.Add(apiResponse.VendorList[0]);
                            this.VendorListView.ItemsSource = null;
                            this.VendorListView.ItemsSource = list3;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Error adding Vendor");
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void AddPersonSelection(PersonDTO person)
        {
            list3.Clear();

            VendorDTO v = new VendorDTO();

            v.City = person.city;
            v.State = person.state;
            v.StreetAddress = person.street_address;
            v.UnitAptSuite = person.unit_apt_suite;
            v.VendorEmail = person.email;
            v.VendorName = person.CustomerName;
            v.VendorPhone = person.phone_primary;
            v.ZipCode = person.zipcode;

            list3.Add(v);

            VendorListView.ItemsSource = list3;
        }

        private void ClearEditFields()
        {
            this.VendorEmail.Text = "";
            this.VendorName.Text = "";
            this.VendorPhone.Text = "";
            this.Address1.Text = "";
            this.Address2.Text = "";
            this.City.Text = "";
            this.State.Text = "";
            this.Zip.Text = "";
        }
        private void SaveVendorButton_Click(object sender, RoutedEventArgs e)
        {
            AddVendor();
        }

        private void VendorListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VendorDTO item = (sender as ListView).SelectedValue as VendorDTO;

            //populate the edit fields
            this.VendorEmail.Text = item.VendorEmail;
            this.VendorName.Text = item.VendorName;
            this.VendorPhone.Text = item.VendorPhone;
            this.Address1.Text = item.StreetAddress;
            this.Address2.Text = item.UnitAptSuite;
            this.City.Text = item.City;
            this.State.Text = item.State;
            this.Zip.Text = item.ZipCode;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //search
            PersonFilter filter = new PersonFilter();
            MainWindow wnd = Application.Current.MainWindow as MainWindow;
            filter.Owner = wnd;

            // inventoryTypes = wnd.GetInventoryTypes();

            filter.mainWnd = wnd;
            filter.vendorParentWnd = this;

            ObservableCollection<KeyValuePair<long, string>> list1 = new ObservableCollection<KeyValuePair<long, string>>();
            //foreach (InventoryTypeDTO inventoryType in inventoryTypes)
            //{
            //    if (inventoryType.InventoryTypeName != "Arrangements")
            //    {
            //        list1.Add(new KeyValuePair<long, string>(inventoryType.InventoryTypeId, inventoryType.InventoryTypeName));
            //    }
            //}

            //filter.InventoryTypeCombo.ItemsSource = list1;

            filter.ShowDialog();
        }
    }
}
