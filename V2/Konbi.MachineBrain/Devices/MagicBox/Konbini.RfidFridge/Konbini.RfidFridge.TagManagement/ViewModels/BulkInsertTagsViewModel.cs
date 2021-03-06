using Caliburn.Micro;
using Konbini.Messages.Services;
using Konbini.RfidFridge.TagManagement.Data;
using Konbini.RfidFridge.TagManagement.DTO;
using Konbini.RfidFridge.TagManagement.Enums;
using Konbini.RfidFridge.TagManagement.Interface;
using Konbini.RfidFridge.TagManagement.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Screen = Konbini.RfidFridge.TagManagement.Enums.Screen;
using Timer = System.Timers.Timer;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public class BulkInsertTagsViewModel : StateViewModel
    {
        public BulkInsertTagsViewModel(IEventAggregator events, ShellViewModel shellView) : base(events, shellView)
        {
            // events.Subscribe(this);
        }

        #region Private fields
        private List<TagDTO> tags;
        private int tagCount;
        private string hardwareMessage;
        private int TENANT_ID;
        private string idStart;
        private string idStop;
        private string tagPrefix;

        #endregion


        #region Services
        public IMbCloudService MbCloudService { get; set; }
        public IRfidReaderInterface RfidReaderInterface { get; set; }
        public ISendMessageToCloudService SendMessageToCloudService { get; set; }
        #endregion

        #region Properties

        public List<MachineDTO.Machine> Machines { get; set; }
        public List<ProductDTO.Product> Products { get; set; }
        public List<TagDTO> Tags
        {
            get => tags; set
            {
                tags = value;
                NotifyOfPropertyChange(() => Tags);
                TagCount = value.Count;

                CheckIfCanAdd();
            }
        }

        public int TagCount
        {
            get => tagCount; set
            {
                tagCount = value;
                NotifyOfPropertyChange(() => TagCount);
            }
        }

        private ProductDTO.Product selectedProduct;
        public ProductDTO.Product SelectedProduct
        {
            get => selectedProduct;
            set
            {
                selectedProduct = value;
                if (selectedProduct != null)
                {
                    Price = int.Parse(Convert.ToString(selectedProduct.Price * 100));
                    CanChangePrice = true;
                }
                NotifyOfPropertyChange(() => SelectedProduct);
                CheckIfCanAdd();
            }
        }
        public MachineDTO.Machine SelectedMachine
        {
            get => selectedMachine;
            set
            {
                selectedMachine = value;
                NotifyOfPropertyChange(() => SelectedProduct);
                CheckIfCanAdd();
            }
        }
        public string CurrentPriceInString { get; set; }
        public bool CanChangePrice { get; set; }
        private int _price;
        private bool canTopup;
        private MachineDTO.Machine selectedMachine;
        private string message;

        public int Price
        {
            get => _price;
            set
            {
                _price = value;
                NotifyOfPropertyChange(() => Price);
                CheckIfCanAdd();
            }
        }

        public bool CanAdd
        {
            get => canTopup;
            set
            {
                canTopup = value;
                NotifyOfPropertyChange(() => CanAdd);
            }
        }

        public string Message
        {
            get => message;
            set
            {
                message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }
        public string HardwareMessage
        {
            get => hardwareMessage; set
            {
                hardwareMessage = value;
                NotifyOfPropertyChange(() => HardwareMessage);
            }
        }

        public string IdStart
        {
            get => idStart; set
            {
                idStart = value;
                NotifyOfPropertyChange(() => IdStart);
            }
        }

        public string IdStop
        {
            get => idStop; set
            {
                idStop = value;
                NotifyOfPropertyChange(() => IdStop);
            }
        }

        public string TagPrefix
        {
            get => tagPrefix; set
            {
                tagPrefix = value;
                NotifyOfPropertyChange(() => TagPrefix);
            }
        }


        #endregion

        public void InitData()
        {


            using (var context = new KDbContext())
            {
                var server = context.Settings.SingleOrDefault(x => x.Key == SettingKey.RbmqServer)?.Value.Trim();
                var user = context.Settings.SingleOrDefault(x => x.Key == SettingKey.RbmqUser)?.Value.Trim();
                var pass = context.Settings.SingleOrDefault(x => x.Key == SettingKey.RbmqPass)?.Value.Trim();
                var tenantId = context.Settings.SingleOrDefault(x => x.Key == SettingKey.TenantId)?.Value.Trim();
                if (string.IsNullOrEmpty(tenantId))
                {
                    TENANT_ID = 0;
                }
                else
                {
                    int.TryParse(tenantId, out int outTenantId);
                    TENANT_ID = outTenantId;
                }

                // SendMessageToCloudService.InitConfigAndConnect(server, user, pass, "magicbox-tagapp");
            }

        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            var debug = System.Diagnostics.Debugger.IsAttached;
            debug = false;
            Task.Run(() =>
            {
                InitData();
                //if (!debug)
                //{
                //    if (!RfidReaderInterface.Connect())
                //    {
                //        Message = "Failed to connect to hardware!";
                //        return;
                //    }
                //    else
                //    {
                //        RfidReaderInterface.StartRecord();
                //        RfidReaderInterface.OnTagsRecord = OnReaderTagsRecord;
                //    }
                //}


                try
                {
                    Message = "Loading products from cloud...";
                    Products = MbCloudService.GetAllProducts();
                    NotifyOfPropertyChange(() => Products);

                    Message = "Ready";
                }
                catch (Exception ex)
                {
                    SeriLogService.LogError(ex.ToString());
                    Message = ex.Message;
                }
            });
        }

        public void OnReaderTagsRecord(List<string> tags)
        {
            Tags = tags.Select(x => new TagDTO(x)).ToList();
            HardwareMessage = $"Total: {Tags.Count}";
        }

        public void AddTags()
        {

            CanAdd = false;
            var confirmMessage = $"Are you sure want to add {TagCount} of {SelectedProduct.Name} to cloud?.";
            var confirm = System.Windows.MessageBox.Show(confirmMessage, "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Information);

            Task.Run(() =>
            {
                if (!(confirm == MessageBoxResult.OK))
                {
                    Message = "Cancelled";
                    CanAdd = true;
                    return;
                }
                try
                {
                    var productId = SelectedProduct.Id;
                    var listTags = Tags.Select(x => new ListTag { name = x.TagId, productId = productId.ToString() }).ToList();

                    var uploadDto = new BulkTagsDto
                    {
                        tenantId = TENANT_ID,
                        listTags = listTags
                    };
                    Message = "Adding...";

                    var result = MbCloudService.BulkInsertTags(uploadDto).Result;

                    if (result)
                    {
                        Message = "Added";
                        Thread.Sleep(1000);
                        Message = "Ready";
                        CanAdd = true;
                    }
                    else
                    {
                        Message = "Failed!!";
                        CanAdd = true;
                    }

                }
                catch (Exception ex)
                {
                    Message = ex.Message;
                    SeriLogService.LogInfo(ex.ToString());
                }
            });
        }

        public void GenerateTags()
        {
            try
            {
                Tags = new List<TagDTO>();

                //AA06BBxxxxxxxCC00002E
                var intFrom = int.Parse(IdStart);
                var intTo = int.Parse(IdStop);


                List<string> tags = new List<string>();

                for (int i = intFrom; i <= intTo; i++)
                {
                    var tag = i.ToString("X");
                    tag = tag.PadLeft(6, '0');
                    tags.Add(TagPrefix + tag);
                }
                if (tags.Count <= 0)
                {
                    return;
                }
                var okMessage = $"Generated tags {tags.First()} - {tags.Last()}. Ready to ADD";
                //NotifyOfPropertyChange(CanAdd);

                Tags = tags.Select(x => new TagDTO(x)).ToList();
                Message = okMessage;

                CanAdd = true;
                NotifyOfPropertyChange(() => CanAdd);

            }
            catch (Exception ex)
            {
                Message = ex.Message;
            }

        }
        public int CalulateQuantum(int quantum)
        {
            var quantumInString = AppendQuantumString(quantum);
            int.TryParse(quantumInString + "0", out int outValue);
            NotifyOfPropertyChange(CurrentPriceInString);
            return outValue;
        }
        public string AppendQuantumString(int quantum)
        {
            var sb = new StringBuilder(CurrentPriceInString);
            if (sb.Length <= 2)
            {
                sb.Append(quantum);
            }
            CurrentPriceInString = sb.ToString();
            return CurrentPriceInString;
        }

        public void ResetValue()
        {
            CurrentPriceInString = string.Empty;
            Price = 0;
        }

        public bool CheckIfCanAdd()
        {
            //CanAdd = SelectedProduct != null && TagCount > 0 && Price > 0;
            CanAdd = SelectedProduct != null && TagCount > 0;
            return CanAdd;
        }
        public override void Handle(AppMessage message)
        {
            if (ShellView.ToggleMenu || ShellView.CurrentScreen != Screen.Main) return;
            if (message is KeyPressedMessage key)
            {
                switch (key.Key)
                {
                    case Key.Subtract:
                    case Key.OemMinus:
                        if (CanChangePrice)
                        {
                            if (Price > 10)
                            {
                                Price -= 10;
                            }
                        }
                        break;
                    case Key.Enter:
                        if (CheckIfCanAdd())
                        {
                            AddTags();
                        }
                        break;
                    case Key.Add:
                    case Key.OemPlus:
                        if (CanChangePrice)
                        {
                            Price += 10;
                        }
                        break;
                    case Key.Multiply:
                        if (CanChangePrice)
                        {
                            ResetValue();
                        }
                        break;
                    case Key.Divide:
                        if (CanChangePrice)
                        {

                        }
                        break;
                    case Key.D1:
                    case Key.NumPad1:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(1);
                        }
                        break;
                    case Key.D2:
                    case Key.NumPad2:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(2);
                        }
                        break;
                    case Key.D3:
                    case Key.NumPad3:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(3);
                        }
                        break;
                    case Key.D4:
                    case Key.NumPad4:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(4);
                        }
                        break;
                    case Key.D5:
                    case Key.NumPad5:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(5);
                        }
                        break;
                    case Key.D6:
                    case Key.NumPad6:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(6);
                        }
                        break;
                    case Key.D7:
                    case Key.NumPad7:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(7);
                        }
                        break;
                    case Key.D8:
                    case Key.NumPad8:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(8);
                        }
                        break;
                    case Key.D9:
                    case Key.NumPad9:
                        if (CanChangePrice)
                        {
                            Price = CalulateQuantum(9);
                        }
                        break;
                    case Key.D0:
                    case Key.NumPad0:
                        if (CanChangePrice)
                        {
                            if (!string.IsNullOrEmpty(CurrentPriceInString))
                            {
                                Price = CalulateQuantum(0);
                            }
                        }
                        break;

                }
            }
        }


    }


}