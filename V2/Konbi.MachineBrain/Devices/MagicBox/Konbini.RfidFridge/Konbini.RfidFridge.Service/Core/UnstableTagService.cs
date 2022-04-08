using Emgu.CV;
using Konbini.RfidFridge.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Core
{
    public class UnstableTagService
    {
        #region Properties

        #endregion

        #region Services
        public LogService LogService;

        #endregion

        public List<UnstableInventoryDto> UnstableInventories { get; set; }

        public UnstableTagService(LogService logService)
        {
            LogService = logService;
            UnstableInventories = new List<UnstableInventoryDto>();
        }

        public void Record(List<string> tags)
        {
            foreach (var tag in tags)
            {
                var isExist = UnstableInventories.Any(x => x.Inventory.TagId == tag);

                if (!isExist)
                {
                    var inventory = new InventoryDto();
                    inventory.TagId = tag;
                    UnstableInventories.Add(new UnstableInventoryDto()
                    {
                        Inventory = inventory,
                        LastChange = DateTime.Now,
                        NumberOfChanges = 1
                    });
                }
                else
                {
                    var item = UnstableInventories.FirstOrDefault(x => x.Inventory.TagId == tag);
                    if (item != null)
                    {
                        item.LastChange = DateTime.Now;
                        item.NumberOfChanges += 1;
                    }
                }
            }

            UnstableInventories.OrderByDescending(x => x.LastChange).OrderByDescending(x => x.NumberOfChanges);
        }

        public void ClearRecord()
        {
            UnstableInventories.Clear();
        }
    }
}
