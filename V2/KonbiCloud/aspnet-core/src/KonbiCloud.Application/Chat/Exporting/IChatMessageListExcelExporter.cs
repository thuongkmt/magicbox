using System.Collections.Generic;
using KonbiCloud.Chat.Dto;
using KonbiCloud.Dto;

namespace KonbiCloud.Chat.Exporting
{
    public interface IChatMessageListExcelExporter
    {
        FileDto ExportToFile(List<ChatMessageExportDto> messages);
    }
}
