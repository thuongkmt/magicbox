
namespace KonbiCloud.BlackListCards.Dto
{
    public class BlackListCardDto
    {
        public long? Id { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public decimal UnpaidAmount { get; set; }
    }
}
