namespace Konbini.RfidFridge.Domain
{
    public class SaleResponse
    {
        public SaleResponse()
        {
            
        }
        public int Amout { get; set; }
        public string ResponseCode { get; set; }
        public string Message { get; set; }
        public override string ToString()
        {
            return $"[{ResponseCode}] {Message}";
        }
      
    }
}