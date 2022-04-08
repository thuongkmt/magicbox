using KonbiCloud.MachineLoadout;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Machines.Dtos
{
    public class LoadoutDto
    {
        public Guid MachineId { get; set; }
        public string MachineName { get; set; }
        public bool IsOnline { get; set; }
        public List<LoadoutList> LoadoutList { get; set; }
    }

    public class LoadoutList
    {
        public int Index { get; set; }
        public List<LoadoutItemDto> Loadouts { get; set; }
    }

    public class UpdateLoadoutDto
    {
        public Guid MachineId { get; set; }
        public List<LoadoutItemDto> Loadouts { get; set; }
    }
}
