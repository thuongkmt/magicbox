using System.Collections.Generic;
using KonbiCloud.Plate.Dtos;
using KonbiCloud.Dto;

namespace KonbiCloud.Plate.Exporting
{
    public interface IDiscsExcelExporter
    {
        FileDto ExportToFile(List<GetDiscForView> discs);
    }
}