using Acr.UserDialogs;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using KonbiCloud.ApiClient;
using KonbiCloud.Commands;
using KonbiCloud.Extensions;
using KonbiCloud.Models.TagsManagement;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using KonbiCloud.ViewModels.Base;
using Konbini.Messages.Services;
using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KonbiCloud.ViewModels
{
    public class InventoryViewModel : XamarinViewModel
    {
        private readonly IProductCategoriesAppService _productCategoriesAppService;
        private readonly IProductsAppService _productAppService;
        private readonly IProductTagsAppService _productTagsAppService;
        private readonly IApplicationContext _applicationContext;
        //private readonly ISendMessageToCloudService _sendMessageToCloudService;
        public ISendMessageToCloudService SendMessageToCloudService { get; set; }
        public ICommand PageAppearingCommand => HttpRequestCommand.Create(PageAppearingAsync);
        public ICommand ConnectBluetoothCommand => HttpRequestCommand.Create(ConnectBluetoothAsync);
        //public ICommand ReadCommand => HttpRequestCommand.Create(ReadTags);
        public ICommand WriteCommand => HttpRequestCommand.Create(WriteTags);
        public ICommand ClearCommand => HttpRequestCommand.Create(ClearTags);
        //public ICommand ScanCommand => HttpRequestCommand.Create(ScanTags);

        private const byte COMMAND_BOOTCODE = 0x40;
        private const byte RESPONSE_BOOTCODE = 0xF0;
        private const int SCANNING_DELAY = 1000;
        private Dictionary<string, DateTime> _dictTags = new Dictionary<string, DateTime>();
        public Dictionary<string, DateTime> ScanningTag = new Dictionary<string, DateTime>();
        private ObservableCollection<Items> _items = new ObservableCollection<Items>();
        public ObservableCollection<Items> Items
        {
            get => _items;
            set
            {
                _items = value;
                RaisePropertyChanged(() => Items);
            }
        }
        private int _itemsCount = 0;
        public int ItemsCount { 
            get => _itemsCount;
            set
            {
                _itemsCount = value;
                RaisePropertyChanged(() => ItemsCount);
            }
        }
        private List<string> tags;
        private ProductCategoryListModel _selectedCategory;
        public ProductCategoryListModel SelectedCategory
        {
            get { return _selectedCategory; }
            set
            {
                if (value != null)
                {
                    HandleSelectedCategory(value);
                    _selectedCategory = value;
                }
                RaisePropertyChanged(() => SelectedCategory);
            }
        }
        private ProductListModel _selectedProduct;
        public ProductListModel SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                RaisePropertyChanged(() => SelectedProduct);
            }
        }
        private bool _enableBtn;
        public bool EnableBtn
        {
            get => _enableBtn;
            set
            {
                _enableBtn = value;
                RaisePropertyChanged(() => EnableBtn);
            }
        }
        private string _connectText;
        public string ConnectText
        {
            get => _connectText;
            set
            {
                _connectText = value;
                RaisePropertyChanged(() => ConnectText);
            }
        }
        private bool _isInitialized;
        private GetAllProductCategoriesInput _filterCategory;
        private GetAllProductsInput _filterProduct;
        private GetAllProductTagsInput _filterProductTag;
        private ObservableRangeCollection<ProductCategoryListModel> _categories = new ObservableRangeCollection<ProductCategoryListModel>();
        public ObservableRangeCollection<ProductCategoryListModel> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                RaisePropertyChanged(() => Categories);
            }
        }
        private ObservableRangeCollection<ProductListModel> _products = new ObservableRangeCollection<ProductListModel>();
        public bool _inventoryScanning = false;
        //private string _inventoryText = "Scan";
        //public string InventoryText { get => _inventoryText;
        //    set {
        //        _inventoryText = value;
        //        RaisePropertyChanged(() => InventoryText);
        //    }
        //}

        public ObservableRangeCollection<ProductListModel> Products
        {
            get => _products;
            set
            {
                _products = value;
                RaisePropertyChanged(() => Products);
            }
        }
        private ObservableRangeCollection<ProductTagListModel> _productTags = new ObservableRangeCollection<ProductTagListModel>();
        public ObservableRangeCollection<ProductTagListModel> ProductTags
        {
            get => _productTags;
            set
            {
                _productTags = value;
                RaisePropertyChanged(() => ProductTags);
            }
        }
        private string _message;
        public string Message { get=>_message;
            set {
                _message = value;
                RaisePropertyChanged(() => Message);
            }
        }

        
        public InventoryViewModel(IProductCategoriesAppService productCategoriesAppService, IProductsAppService productsAppService, 
            IProductTagsAppService productTagsAppService, IApplicationContext applicationContext)
        {
            _productCategoriesAppService = productCategoriesAppService;
            _productAppService = productsAppService;
            _productTagsAppService = productTagsAppService;
            _applicationContext = applicationContext;
            if (!BleApplication._client.Connected) ConnectBluetoothDevice();
        }
        private async Task DisconnectBluetoothDevice()
        {
            try
            {
                BleApplication._client.Dispose();

                //if (BleApplication._stream is object)
                //{
                //    BleApplication._stream.Dispose();
                //    BleApplication._stream = null;
                //}
                BleApplication._client = new BluetoothClient();

                EnableBtn = false;
                ConnectText = "Connect";
                //UserDialogs.Instance.ShowLoading($"Disconnecting {BleApplication._device.DeviceName}...");
                Message = "Disconnected to " + BleApplication._device.DeviceName + ".";
            }
            catch(Exception ex)
            {
                //await UserDialogs.Instance.AlertAsync(ex.Message, "Disconnect error");
                Message = "Disconnect error";
            }           
        }
        private async Task ConnectBluetoothDevice()
        {
            try
            {
                BluetoothDevicePicker picker = new BluetoothDevicePicker();
                picker.RequireAuthentication = false;
                BleApplication._device = await picker.PickSingleDeviceAsync();

                if (!BleApplication._device.Authenticated)
                {
                    BluetoothSecurity.PairRequest(BleApplication._device.DeviceAddress, "0000");
                }

                //connect to device
                BleApplication._client.Connect(BleApplication._device.DeviceAddress, BluetoothService.SerialPort);
                if (BleApplication._client.Connected)
                {
                    BleApplication._stream = BleApplication._client.GetStream();
                    //set reader mode
                    byte[] data = new byte[] { 0x40, 0x03, 0x0B, 0x01, 0xB1 };
                    BleApplication._stream.Write(data, 0, data.Length);
                    StartInventory();

                    EnableBtn = true;
                    ConnectText = "Disconnect";
                    Message = "Connected to " + BleApplication._device.DeviceName + ".";
                    //UserDialogs.Instance.Toast($"Connected to {BleApplication._device.DeviceName}.", TimeSpan.FromMilliseconds(3000));
                    
                }
                else
                {
                    Message = "Cannot connect to " + BleApplication._device.DeviceName + ".";
                    //UserDialogs.Instance.Toast($"Cannot connect to {BleApplication._device.DeviceName}.", TimeSpan.FromMilliseconds(3000));
                    
                }
            }
            catch(Exception ex)
            {
                Message = "Connection error";
                //UserDialogs.Instance.Toast("Connection error", TimeSpan.FromMilliseconds(3000));
                
            }
                        
        }

        private async Task ScanTags()
        {
            //if (!_inventoryScanning)
            //    StartInventory();
            //else
            //    StopInventory();
        }
        private async Task ReadTags()
        {
            if (SelectedProduct != null)
            {                               
                InitItems();
                await SetBusyAsync(async () =>
                {
                    //get all product tags
                    _filterProductTag = new GetAllProductTagsInput
                    {
                        ProductFilter = SelectedProduct.Name,
                        MaxResultCount = 10000,
                        SkipCount = 0
                    };
                    GetProductTagsAsync(true);
                });                
            }
            else
            {
                await UserDialogs.Instance.AlertAsync("No product was selected.");
            }

        }

        private void StopInventory()
        {
            isStop = false;
            _inventoryScanning = false;
            //InventoryText = "Scan";
        }
        private async void StartInventory()
        {
            //InitItems();

            await Task.Run(() =>
             {
                 _inventoryScanning = true;
                 //InventoryText = "Stop";
                 //isStop = true;
                 if (BleApplication._stream.CanRead)
                 {
                     byte[] myReadBuffer = new byte[1024];
                     using (var memoryStream = new MemoryStream())
                     {
                         do
                         {
                             //if (_clear)
                             //{
                             //    InitItems();
                             //    _clear = false;
                             //}
                             int numberOfBytesRead = BleApplication._stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                             memoryStream.Write(myReadBuffer, 0, numberOfBytesRead);
                             ParseTagData(myReadBuffer);
                             if (!BleApplication._stream.DataAvailable)
                                 Thread.Sleep(1);
                            //DataReceived(myReadBuffer);
                            Thread.Sleep(SCANNING_DELAY);
                             //System.Diagnostics.Debug.WriteLine("tags: " + tags);
                        }
                        //while (BleApplication._stream.DataAvailable);
                        while (true);
                     }
                     //if (!BleApplication._stream.DataAvailable) StopInventory();
                 }
                 else
                 {
                     System.Diagnostics.Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                 }
             });

        }

        void DataReceived(byte[] myReadBuffer)
        {
            BleApplication._stream.Read(myReadBuffer, 0, myReadBuffer.Length);

            //System.Diagnostics.Debug.WriteLine("<---- " + myReadBuffer.ToArray().ToHexString());
            ParseTagData(myReadBuffer);
        }
        private StringBuilder _bufferBuilder = new StringBuilder();
        private bool isStop;
        private bool _clear = false;

        private void ParseTagData(byte[] buf)
        {
            _bufferBuilder.Append(buf.ToArray().ToHexStringNoSpace());

        ParseData:
            var sbuffer = _bufferBuilder.ToString();
            if (sbuffer.Contains("000055AA"))
            {
                try
                {
                    if (sbuffer.Length >= 500)
                    {
                        var startIndex = sbuffer.IndexOf("000055AA");
                        var tagMessage = sbuffer.Substring(startIndex, 500);
                        var tagBytes = tagMessage.StringToByteArray();
                        var tagLength = (new byte[] { tagBytes[11] }).ToHexStringNoSpace();
                        var tagId = tagMessage.Substring(24, int.Parse(tagLength) * 4);
                        //System.Diagnostics.Debug.WriteLine("tagId: " + tagId);
                        ProcessTags(tagId);
                        var existingBuffer = sbuffer.Replace(tagMessage, string.Empty);
                        if (existingBuffer.Length == 0)
                        {
                            _bufferBuilder.Clear();
                        }
                        else
                        {
                            _bufferBuilder.Clear();
                            _bufferBuilder.Append(existingBuffer);
                        }
                        //System.Diagnostics.Debug.WriteLine("_bufferBuilder: " + _bufferBuilder);
                        //System.Diagnostics.Debug.WriteLine("_bufferBuilder L: " + _bufferBuilder.Length);
                        if (_bufferBuilder.Length >= 500)
                        {
                            goto ParseData;
                        }
                    }
                    else
                    {
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
        }
        
        private void ProcessTags(string tag)
        {
            if (_clear) InitItems();
            var list = Items;
            var currentTag = _dictTags.Keys.FirstOrDefault(x => x == tag);
            if (currentTag == null)
            {
                _dictTags.Add(tag, DateTime.Now);
               
            }
            else
            {
                _dictTags[tag] = DateTime.Now;
            }
            
            //ScanningTag = _dictTags.Where(x => (DateTime.Now - x.Value).TotalMilliseconds <= SCANNING_DELAY).ToDictionary(x => x.Key, x => x.Value);
            ////ScanningTag = _dictTags.Except(removedItems);
            //System.Diagnostics.Debug.WriteLine("Scanning Tags: " + string.Join(",", ScanningTag.Select(x => x.Key).ToList()));
            var check = tags==null? 0: tags.Where(x => x == tag).Count();
            if (check == 0)
            {
                list.Add(new Items
                {
                    TagId = tag
                });
                Items = list;
                ItemsCount = Items.Count;
                tags = Items.Select(x => x.TagId).ToList();
                //RaisePropertyChanged(() => Items);
                //Message = "Found...";
            }
        }
        private async Task WriteTags()
        {
            if (SelectedProduct != null)
            {
                if(!await UserDialogs.Instance.ConfirmAsync($"Are you sure want to add {ItemsCount} tags of {SelectedProduct.Name} to cloud?."))
                {
                    Message = "Cancelled";
                    return;
                }
                try
                {
                    var input = new ListProductTagDto();
                    input.TenantId = _applicationContext.CurrentTenant.TenantId;
                    var list = new List<ProductTagInputDto>();
                    foreach (var i in Items)
                    {
                        list.Add(new ProductTagInputDto
                        {
                            Name = i.TagId,
                            ProductId = SelectedProduct.Id.ToString()
                        });
                    }
                    input.ListTags = list;
                    InitItems();
                    await SetBusyAsync(async () =>
                    {
                        await _productTagsAppService.InsertTags(input);
                        await NavigationService.GoBackAsync();
                        //get product tags
                        //_filterProductTag = new GetAllProductTagsInput
                        //{
                        //    ProductFilter = SelectedProduct.Name,
                        //    MaxResultCount = 10000,
                        //    SkipCount = 0
                        //};
                        //GetProductTagsAsync(true);
                        
                        //Message = "Added";
                        //Thread.Sleep(1000);
                        //Message = "Ready";
                        //CanAdd = true;
                    });                    
                }
                catch(Exception ex)
                {
                    Message = ex.Message;
                    await UserDialogs.Instance.AlertAsync(ex.Message, "Add tags error");
                    //Trace.Message(ex.Message);
                }
            }
            else
            {
                await UserDialogs.Instance.AlertAsync("No product was selected.");

            }
        }

        private void InitItems()
        {
            Items = new ObservableCollection<Items>();
            ItemsCount = 0;
            tags = null;
        }

        public async Task PageAppearingAsync()
        {
            if (!BleApplication._client.Connected)
            {
                ConnectText = "Connect";
            }
            else ConnectText = "Disconnect";

                //Message = "";
            InitItems();
            //if (ItemsCount > 0)
            //{
            //    EnableBtn = true;
            //}
            //SelectedCategory = null;
            if (_isInitialized)
            {
                return;
            }

            await SetBusyAsync(async () =>
            {
                await FetchDataAsync(overwrite: true);
            });

            _isInitialized = true;
        }
        private async Task ClearTags()
        {
            InitItems();
            Message = "";
            _clear = true;
        }
        public async Task FetchDataAsync(string filterText = null, bool overwrite = false, int skipCount = 0)
        {
            _filterCategory = new GetAllProductCategoriesInput
            {
                Filter = filterText,
                MaxResultCount = 10000,
                SkipCount = 0
            };

            await FetchCategoriesAsync(overwrite);
        }
        private async Task FetchCategoriesAsync(bool overwrite = false)
        {
            var result = await _productCategoriesAppService.GetAll(_filterCategory);
            if (overwrite)
            {
                Categories.ReplaceRange(ObjectMapper.Map<List<ProductCategoryListModel>>(result.Items));
            }
            else
            {
                Categories.AddRange(ObjectMapper.Map<List<ProductCategoryListModel>>(result.Items));
            }
        }
        private async void HandleSelectedCategory(ProductCategoryListModel category)
        {
            Console.WriteLine(category.Id + " " + category.Name);
            _filterProduct = new GetAllProductsInput
            {
                Filter = null,
                CategoryFilter = category.Id,
                MaxResultCount = 10000,
                SkipCount = 0
            };
            await FetchProductsAsync(true);
        }
        private async Task FetchProductsAsync(bool overwrite = false)
        {
            var result = await _productAppService.GetAll(_filterProduct);
            if (overwrite)
            {
                Products.ReplaceRange(ObjectMapper.Map<List<ProductListModel>>(result.Items));
            }
            else
            {
                Products.AddRange(ObjectMapper.Map<List<ProductListModel>>(result.Items));
            }
        }

        private async void GetProductTagsAsync(bool overwrite = false)
        {
            var resultTags = await _productTagsAppService.GetAll(_filterProductTag);
            if (overwrite)
            {
                ProductTags.ReplaceRange(ObjectMapper.Map<List<ProductTagListModel>>(resultTags.Items));
            }
            else
            {                
                ProductTags.AddRange(ObjectMapper.Map<List<ProductTagListModel>>(resultTags.Items));
            }
            //add to Items
            var list = new ObservableCollection<Items>();
            foreach (var item in ProductTags)
            {
                list.Add(new ViewModels.Items
                {
                    ProductName = item.ProductTag.ProductName,
                    TagId = item.ProductTag.Name,
                    //Price = item.ProductTag.
                });
            }
            Items = list;
            ItemsCount = Items.Count;
            tags = Items.Select(x => x.TagId).ToList();
            if (ItemsCount == 0) Message = "No products";
            else Message = "Done";
        }

        public async Task ConnectBluetoothAsync()
        {
            if (BleApplication._client.Connected)
            {
                DisconnectBluetoothDevice();                
            }
            else
            {
                ConnectBluetoothDevice();
            }
        }
    }
    public class Items
    {
        public string ProductName { get; set; }
        public string TagId { get; set; }
        public decimal Price { get; set; }
    }
}
