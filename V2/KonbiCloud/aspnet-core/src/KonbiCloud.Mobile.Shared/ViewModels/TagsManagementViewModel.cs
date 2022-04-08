using Acr.UserDialogs;
using KonbiCloud.ApiClient;
using KonbiCloud.Commands;
using KonbiCloud.Extensions;
using KonbiCloud.Models.TagsManagement;
using KonbiCloud.Products;
using KonbiCloud.Products.Dtos;
using KonbiCloud.ViewModels.Base;
using MvvmHelpers;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KonbiCloud.ViewModels
{
    public class TagsManagementViewModel : XamarinViewModel
    {


        ICharacteristic _characteristicWrite;
        private readonly IUserDialogs _userDialogs;

        private readonly IProductCategoriesAppService _productCategoriesAppService;
        private readonly IProductsAppService _productAppService;
        private readonly IProductTagsAppService _productTagsAppService;
        private readonly IApplicationContext _applicationContext;
        public string CharacteristicValue => ToHexString(BleApplication._characteristicUpdate?.Value).Replace("-", "");
        public ICommand WriteCommand => HttpRequestCommand.Create(WriteTags);
        public ICommand ClearCommand => HttpRequestCommand.Create(ClearTags);
        private string ToHexString(byte[] bytes)
        {
            return bytes != null ? BitConverter.ToString(bytes) : string.Empty;
        }
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
        public int ItemsCount
        {
            get => _itemsCount;
            set
            {
                _itemsCount = value;
                RaisePropertyChanged(() => ItemsCount);
            }
        }
        private List<string> tags;
        private bool _clear = false;
        public string bufferString = "";
        private string _message;
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                RaisePropertyChanged(() => Message);
            }
        }
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
        private bool _enableBtn = false;
        public bool EnableBtn
        {
            get => _enableBtn;
            set
            {
                _enableBtn = value;
                RaisePropertyChanged(() => EnableBtn);
            }
        }
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
        private bool _updatesStarted;

        public ObservableRangeCollection<ProductTagListModel> ProductTags
        {
            get => _productTags;
            set
            {
                _productTags = value;
                RaisePropertyChanged(() => ProductTags);
            }
        }
        public TagsManagementViewModel(IProductCategoriesAppService productCategoriesAppService, IProductsAppService productsAppService,
            IProductTagsAppService productTagsAppService, IApplicationContext applicationContext)
        {
            _productCategoriesAppService = productCategoriesAppService;
            _productAppService = productsAppService;
            _productTagsAppService = productTagsAppService;
            _applicationContext = applicationContext;
        }
        public override async Task InitializeAsync(object navigationData)
        {
            var input = (DeviceListItemViewModel)navigationData;
            BleApplication.deviceConnected = input;
            //get data for categories and product
            await FetchDataAsync(overwrite: true);

            try
            {
                if (BleApplication.deviceConnected.IsConnected)
                {
                    var deviceId = BleApplication.deviceConnected.Id;
                    // old id 0000fff0-0000-1000-8000-00805f9b34fb

                    var services = await BleApplication.deviceConnected.Device.GetServicesAsync();

                    //var test = string.Empty;
                    //foreach (var s in services)
                    //{
                    //    test += $"{s.Name}, {s.Id}";
                    //    //await UserDialogs.Instance.AlertAsync("S: " + test);
                    //    //var ll = await s.GetCharacteristicsAsync();
                    //    //foreach (var cs in ll)
                    //    //{
                    //    //    test = $"{cs.Name}, {cs.Id}";
                    //      await UserDialogs.Instance.AlertAsync(test);
                    //    //}
                    //}



                    BleApplication._service = await BleApplication.deviceConnected.Device.GetServiceAsync(Guid.Parse("0000fff0-0000-1000-8000-00805f9b34fb"));
                    if (BleApplication._service == null)
                    {
                       // await UserDialogs.Instance.AlertAsync("New device");
                        BleApplication._service = await BleApplication.deviceConnected.Device.GetServiceAsync(Guid.Parse("49535343-fe7d-4ae5-8fa9-9fafd205e455"));
                        BleApplication._characteristicUpdate =  BleApplication._service.GetCharacteristicsAsync().Result[2];
                        await UserDialogs.Instance.AlertAsync("_characteristicUpdate: " + BleApplication._characteristicUpdate.Id);
                        StartUpdates();

                    }
                    else
                    {

                        //await UserDialogs.Instance.AlertAsync("Old device");

                        //_characteristicWrite = await BleApplication._service.GetCharacteristicAsync(Guid.Parse("0000fff2-0000-1000-8000-00805f9b34fb"));
                        BleApplication._characteristicUpdate = await BleApplication._service.GetCharacteristicAsync(Guid.Parse("0000fff1-0000-1000-8000-00805f9b34fb"));

                        StartUpdates();
                    }
                }
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync("C:"+ex.Message);
            }
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
            //Message = category.Name + " category was seleted.";
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

        private async Task WriteTags()
        {
            if (SelectedProduct != null)
            {
                if (!await UserDialogs.Instance.ConfirmAsync($"Are you sure want to add {ItemsCount} tags of {SelectedProduct.Name} to cloud?."))
                {
                    UserDialogs.Instance.Toast("Cancelled", TimeSpan.FromMilliseconds(3000));
                    //_userDialogs.ErrorToast("", "Cancelled", TimeSpan.FromMilliseconds(3000));
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

                    await SetBusyAsync(async () =>
                    {
                        await _productTagsAppService.InsertTags(input);
                        UserDialogs.Instance.Toast("Successful", TimeSpan.FromMilliseconds(3000));
                        Message = "Successful";
                        InitItems();
                    });
                }
                catch (Exception ex)
                {
                    Message = ex.Message;
                    await UserDialogs.Instance.AlertAsync(ex.Message, "Add tags error");
                    //Trace.Message(ex.Message);
                }
            }
            else
            {
                await UserDialogs.Instance.AlertAsync("No product was selected.");
                Message = "No product was selected.";
            }
        }
        private async void StartUpdates()
        {
            try
            {
                _updatesStarted = true;
                BleApplication._characteristicUpdate.ValueUpdated -= CharacteristicOnValueUpdated;
                BleApplication._characteristicUpdate.ValueUpdated += CharacteristicOnValueUpdated;
                await BleApplication._characteristicUpdate.StartUpdatesAsync();
                Debug.WriteLine("Start updates");
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync("B:" + ex.Message);
            }
        }
        private async void StopUpdates()
        {
            try
            {
                _updatesStarted = false;
                await BleApplication._characteristicUpdate.StopUpdatesAsync();
                BleApplication._characteristicUpdate.ValueUpdated -= CharacteristicOnValueUpdated;
                Debug.WriteLine("Stop update");

                StartUpdates();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync(ex.Message);
            }
        }
        private async Task ClearTags()
        {
            InitItems();
            Message = "";
            //_clear = true;            
            StopUpdates();
        }
        private void InitItems()
        {
            Items = new ObservableCollection<Items>();
            ItemsCount = 0;
            tags = null;
            //_clear = true;
        }
        private void CharacteristicOnValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            try
            {
                if (BleApplication.deviceConnected.IsConnected)
                {
                    bufferString += CharacteristicValue;
                    if (bufferString.Length % 500 == 0)
                    {
                        //Messages.Insert(0, $"{DateTime.Now.TimeOfDay} - Updated: {bufferString}");
                        ParseTagData(bufferString);
                        Debug.WriteLine("Data receiver: " + bufferString);
                        bufferString = "";
                    }
                    //int position = bufferString.IndexOf("000055AA");
                    //if (bufferString.Length >500 + position)
                    //{
                    //    ParseTagData(bufferString.Substring(position, 500));
                    //    bufferString = bufferString.Substring(position + 500);
                    //}
                    //Debug.WriteLine("Data receiver: " + bufferString.Length);
                    RaisePropertyChanged(() => CharacteristicValue);
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert("A:" + ex.Message);
                Message = ex.Message;
            }
        }

        private void ParseTagData(string bufferString)
        {
            var startIndex = bufferString.IndexOf("000055AA");
            var tagMessage = bufferString.Substring(startIndex);
            var tagId = tagMessage.Substring(24, 24);
            //Debug.WriteLine("===>" + tagId);
            //Messages.Insert(0, $"{DateTime.Now.TimeOfDay} - Updated: {tagId}");
            ProcessTags(tagId);
        }
        private void ProcessTags(string tagId)
        {
            var currentTag = Items.Where(x => x.TagId == tagId);
            if (currentTag.Count() == 0)
            //if (tags == null || !tags.Contains(tagId))
            {
                Items.Add(new Items
                {
                    TagId = tagId
                });

                ItemsCount = Items.Count;
                EnableBtn = ItemsCount > 0 ? true : false;
                tags = Items.Select(x => x.TagId).ToList();
                //tags.Add(tagId);
                //Debug.WriteLine("tags" + tags);
                //UserDialogs.Instance.Toast("Found tag", TimeSpan.FromMilliseconds(3000));
                //Message = "Found tag...";
                RaisePropertyChanged(() => Items);
            }
        }
    }
}
