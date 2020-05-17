using EO.ViewModels.ControllerModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string LAN_Address { get; private set; }
        public string User { get; private set; }
        public string Pwd { get; private set; }

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                GetNetworkConfig();

                MainContent.Content = new Frame() { Content = new LoginPage() };
            }
            catch(Exception ex)
            {
                int debug = 1;
            }
        }

        private void GetNetworkConfig()
        {

            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");

            ManagementObjectCollection nics = mc.GetInstances();

            List<string> enabledIPs = new List<string>();

            foreach (ManagementObject nic in nics)
            {
                if (Convert.ToBoolean(nic["ipEnabled"]) == true)
                {
                    string IpAddress = (nic["IPAddress"] as String[])[0];

                    enabledIPs.Add(IpAddress);

                    string IPSubnet = (nic["IPSubnet"] as String[])[0];

                    string DefaultGateWay = (nic["DefaultIPGateway"] as String[])[0];
                }
            }

            if (enabledIPs.Count > 0)
            {
                LAN_Address = enabledIPs.First();
            }
        }

        public void OnLogInClick(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
               
                client.DefaultRequestHeaders.Accept.Add(
                   new MediaTypeWithQualityHeaderValue("application/json"));

                Frame f = MainContent.Content as Frame;
                User = (f.Content as LoginPage).UserName.Text;
                Pwd = (f.Content as LoginPage).Password.Password;

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                LoginRequest request = new LoginRequest(User, Pwd);

                string jsonData = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = client.PostAsync("api/Login/Login", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    IEnumerable<string> values;
                    httpResponse.Headers.TryGetValues("EO-Header", out values);
                    if (values != null && values.ToList().Count == 1)
                    {                        
                        this.MainContent.Content = new Frame() { Content = new DashboardPage() };
                    }
                    else
                    {
                        MessageBox.Show("Unrecognized username / password");
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        public GetByteArrayResponse GetImage(long imageId)
        {
            GetByteArrayResponse response = new GetByteArrayResponse();

            byte[] img = new byte[0];

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse = client.GetAsync("api/Login/GetImage?imageId=" + ((long)imageId).ToString()).Result;
                

                if (httpResponse.IsSuccessStatusCode)
                {
                    //string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    //response = JsonConvert.DeserializeObject<GetByteArrayResponse>(strData);
                    img = httpResponse.Content.ReadAsByteArrayAsync().Result;
                    response.ImageData = img;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving an image");
                }
            }
            catch(Exception ex)
            {

            }

            return response;
        }
        public void OnBackClick(object currentPage)
        {
            //change page based on who's calling

            if(currentPage is InventoryPage || currentPage is ArrangementPage || currentPage is WorkOrderPage 
                || currentPage is VendorPage || currentPage is ShipmentPage || currentPage is ReportsPage || currentPage is CustomerPage )
            {
                this.ButtonContent.Content = null;
                this.MainContent.Content = new Frame() { Content = new DashboardPage() };
            }
            else if(currentPage is PlantPage || currentPage is ContainerPage || currentPage is ArrangementPage || currentPage is FoliagePage 
                || currentPage is MaterialsPage || currentPage is ImportPage)
            {
                this.MainContent.Content = new Frame() { Content = new InventoryPage() };
            }
            else if(currentPage is WorkOrderReportPage || currentPage is ShipmentReportPage)
            {
                this.MainContent.Content = new Frame() { Content = new ReportsPage() };
            }
        }

        public void OnSaveClick(object currentPage)
        {
            if(currentPage is PlantPage)
            {
                ((PlantPage)currentPage).AddPlant();
            }
            else if(currentPage is ContainerPage)
            {
                ((ContainerPage)currentPage).AddContainer();
            }
            else if(currentPage is ArrangementPage)
            {
                ((ArrangementPage)currentPage).AddArrangement();
            }
            else if(currentPage is VendorPage)
            {
                ((VendorPage)currentPage).AddVendor();
            }
            else if(currentPage is ShipmentPage)
            {
                ((ShipmentPage)currentPage).AddShipment();
            }
            else if(currentPage is WorkOrderPage)
            {
                ((WorkOrderPage)currentPage).AddWorkOrder();
            }
        }

        public List<VendorDTO> GetVendors()
        {
            List<VendorDTO> vDTO = new List<VendorDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                string jsonData = JsonConvert.SerializeObject(new GetPersonRequest());
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse =
                    client.PostAsync("api/Login/GetVendors",content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetVendorResponse resp = JsonConvert.DeserializeObject<GetVendorResponse>(strData);
                    vDTO = resp.VendorList;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving vendors");
                }
            }
            catch(Exception ex)
            {

            }

            return vDTO;
        }

        public List<InventoryTypeDTO> GetInventoryTypes()
        {
            GetInventoryTypeResponse iDTO = new GetInventoryTypeResponse(new List<InventoryTypeDTO>());

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetInventoryTypes").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    iDTO = JsonConvert.DeserializeObject<GetInventoryTypeResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving inventory types");
                }
            }
            catch (Exception ex)
            {

            }

            return iDTO.InventoryType;
        }

        public List<InventoryDTO> GetInventoryByType(long inventorytype)
        {
            GetInventoryResponse response = new GetInventoryResponse(new List<InventoryDTO>());

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetInventory?inventoryType=" + inventorytype.ToString()).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    response = JsonConvert.DeserializeObject<GetInventoryResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving inventory");
                }
            }
            catch(Exception ex)
            {

            }

            return response.InventoryList;
        }

        public List<PlantNameDTO> GetPlantNamesByType(long plantTypeId)
        {
            List<PlantNameDTO> plantNameList = new List<PlantNameDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetPlantNamesByType?plantTypeId=" + plantTypeId.ToString()).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetPlantNameResponse response = JsonConvert.DeserializeObject<GetPlantNameResponse>(strData);

                    plantNameList = response.PlantNames;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving plant names");
                }
            }
            catch (Exception ex)
            {

            }

            return plantNameList;
        }

        public GetPlantResponse GetPlantsByType(long plantTypeId)
        {
            GetPlantResponse plants = new GetPlantResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetPlantsByType?plantTypeId=" + plantTypeId).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    plants = JsonConvert.DeserializeObject<GetPlantResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving plants");
                }
            }
            catch (Exception ex)
            {

            }

            return plants;
        }

        public List<PlantTypeDTO> GetPlantTypes()
        {
            List<PlantTypeDTO> plantTypes = new List<PlantTypeDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetPlantTypes").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetPlantTypeResponse response = JsonConvert.DeserializeObject<GetPlantTypeResponse>(strData);
                    plantTypes = response.PlantTypes;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving plant types");
                }
            }
            catch(Exception ex)
            {

            }
            return plantTypes;
        }

        public List<PlantNameDTO> GetPlantNames()
        {
            List<PlantNameDTO> plantNames = new List<PlantNameDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetPlantNames").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    plantNames = JsonConvert.DeserializeObject<List<PlantNameDTO>>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving plant names");
                }
            }
            catch (Exception ex)
            {

            }
            return plantNames;
        }
        public List<ContainerNameDTO> GetContainerNamesByType(long containerTypeId)
        {
            List<ContainerNameDTO> containerNameList = new List<ContainerNameDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetContainerNamesByType?containerTypeId=" + containerTypeId.ToString()).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    containerNameList = JsonConvert.DeserializeObject<List<ContainerNameDTO>>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving container names");
                }
            }
            catch (Exception ex)
            {

            }

            return containerNameList;
        }

        public List<ContainerTypeDTO> GetContainerTypes()
        {
            List<ContainerTypeDTO> containerTypes = new List<ContainerTypeDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetContainerTypes").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetContainerTypeResponse response = JsonConvert.DeserializeObject<GetContainerTypeResponse>(strData);
                    containerTypes = response.ContainerTypeList;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving container types");
                }
            }
            catch (Exception ex)
            {

            }
            return containerTypes;
        }

        public GetContainerResponse GetContainersByType(long typeId)
        {
            GetContainerResponse containers = new GetContainerResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetContainersByType?containerTypeId=" + typeId).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetContainerResponse response = JsonConvert.DeserializeObject<GetContainerResponse>(strData);
                    containers = response;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving containers");
                }
            }
            catch (Exception ex)
            {

            }

            return containers;
        }

        public ServiceCodeDTO GetServiceCodeById(long serviceCodeId)
        {
            ServiceCodeDTO serviceCode = new ServiceCodeDTO();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetServiceCodeById?serviceCodeId=" + ((int)serviceCodeId).ToString()).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    serviceCode = JsonConvert.DeserializeObject<ServiceCodeDTO>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving service code with id = " + serviceCodeId.ToString());
                }
            }
            catch(Exception ex)
            {

            }

            return serviceCode;
        }

        public List<string> GetSizeByInventoryType(long inventoryTypeId)
        {
            List<string> sizes = new List<string>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://192.168.1.3:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetSizeByInventoryType?inventoryTypeid=" + inventoryTypeId.ToString()).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetSizeResponse response = JsonConvert.DeserializeObject<GetSizeResponse>(strData);
                    sizes = response.Sizes;
                }
                else
                {
                    //MessageBox.Show("There was an error retreiving plant types");
                }
            }
            catch (Exception ex)
            {

            }

            return sizes;
        }
        public List<ServiceCodeDTO> GetServiceCodes(SharedData.Enums.ServiceCodeType svcCodeType)
        {
            List<ServiceCodeDTO> serviceCodes = new List<ServiceCodeDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetAllServiceCodesByType?serviceCodeType=" + ((int)svcCodeType).ToString()).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetServiceCodeResponse response  = JsonConvert.DeserializeObject<GetServiceCodeResponse>(strData);
                    serviceCodes = response.ServiceCodeList;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving service codes");
                }
            }
            catch(Exception ex)
            {

            }
            return serviceCodes;
        }

        public GetPersonResponse GetCustomers(GetPersonRequest request)
        {
            GetPersonResponse persons = new GetPersonResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                string jsonData = JsonConvert.SerializeObject(request);
                StringContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage httpResponse =
                    client.PostAsync("api/Login/GetPerson", content).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    persons = JsonConvert.DeserializeObject<GetPersonResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving customers");
                }
            }
            catch (Exception ex)
            {

            }

            return persons;
        }

        public List<MaterialNameDTO> GetMaterialNamesByType(long materialTypeId)
        {
            List<MaterialNameDTO> materialNameList = new List<MaterialNameDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetMaterialNamesByType?materialTypeId=" + materialTypeId.ToString()).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetMaterialNameResponse response = JsonConvert.DeserializeObject<GetMaterialNameResponse>(strData);

                    materialNameList = response.MaterialNames;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving plant names");
                }
            }
            catch (Exception ex)
            {

            }

            return materialNameList;
        }

        public GetMaterialResponse GetMaterialsByType(long materialTypeId)
        {
            GetMaterialResponse materials = new GetMaterialResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetMaterialsByType?materialTypeId=" + materialTypeId).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    materials = JsonConvert.DeserializeObject<GetMaterialResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving materials");
                }
            }
            catch (Exception ex)
            {

            }

            return materials;
        }

        public List<MaterialTypeDTO> GetMaterialTypes()
        {
            List<MaterialTypeDTO> materialTypes = new List<MaterialTypeDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetMaterialTypes").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetMaterialTypeResponse response = JsonConvert.DeserializeObject<GetMaterialTypeResponse>(strData);
                    materialTypes = response.MaterialTypes;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving material types");
                }
            }
            catch (Exception ex)
            {

            }
            return materialTypes;
        }

        public List<MaterialNameDTO> GetMaterialNames()
        {
            List<MaterialNameDTO> materialNames = new List<MaterialNameDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetMaterialNames").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    materialNames = JsonConvert.DeserializeObject<List<MaterialNameDTO>>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving material names");
                }
            }
            catch (Exception ex)
            {

            }
            return materialNames;
        }

        public List<FoliageTypeDTO> GetFoliageTypes()
        {
            List<FoliageTypeDTO> foliageTypes = new List<FoliageTypeDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetFoliageTypes").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetFoliageTypeResponse response = JsonConvert.DeserializeObject<GetFoliageTypeResponse>(strData);
                    foliageTypes = response.FoliageTypes;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving foliage types");
                }
            }
            catch (Exception ex)
            {

            }
            return foliageTypes;
        }

        public List<FoliageNameDTO> GetFoliageNames()
        {
            List<FoliageNameDTO> foliageNames = new List<FoliageNameDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetFoliageNames").Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    foliageNames = JsonConvert.DeserializeObject<List<FoliageNameDTO>>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving foliage names");
                }
            }
            catch (Exception ex)
            {

            }
            return foliageNames;
        }

        public List<FoliageNameDTO> GetFoliageNamesByType(long foliageTypeId)
        {
            List<FoliageNameDTO> foliageNameList = new List<FoliageNameDTO>();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("plain/text"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetFoliageNamesByType?foliageTypeId=" + foliageTypeId.ToString()).Result;

                if (httpResponse.IsSuccessStatusCode)
                {
                    string strData = httpResponse.Content.ReadAsStringAsync().Result;
                    GetFoliageNameResponse response = JsonConvert.DeserializeObject<GetFoliageNameResponse>(strData);

                    foliageNameList = response.FoliageNames;
                }
                else
                {
                    MessageBox.Show("There was an error retreiving foliage names");
                }
            }
            catch (Exception ex)
            {

            }

            return foliageNameList;
        }
        public GetFoliageResponse GetFoliageByType(long foliageTypeId)
        {
            GetFoliageResponse foliage = new GetFoliageResponse();

            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:9000/");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Add("EO-Header", User + " : " + Pwd);

                HttpResponseMessage httpResponse =
                    client.GetAsync("api/Login/GetFoliageByType?foliageTypeId=" + foliageTypeId).Result;
                if (httpResponse.IsSuccessStatusCode)
                {
                    Stream streamData = httpResponse.Content.ReadAsStreamAsync().Result;
                    StreamReader strReader = new StreamReader(streamData);
                    string strData = strReader.ReadToEnd();
                    strReader.Close();
                    foliage = JsonConvert.DeserializeObject<GetFoliageResponse>(strData);
                }
                else
                {
                    MessageBox.Show("There was an error retreiving foliage");
                }
            }
            catch (Exception ex)
            {

            }

            return foliage;
        }
    }
}
