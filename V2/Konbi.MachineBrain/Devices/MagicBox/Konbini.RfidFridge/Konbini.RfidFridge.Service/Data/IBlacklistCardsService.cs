using Konbini.RfidFridge.Domain.DTO;
using Konbini.RfidFridge.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbini.RfidFridge.Service.Data
{
    public interface IBlacklistCardsService
    {

        void Add(BlackListCardsDto card);
        void ReloadData();
        void Init(bool isEnable);
        List<BlackListCardsDto> BlackListCards { get; set; }
        bool CheckCPASBlaskList(string cardNumer);
    }
}
