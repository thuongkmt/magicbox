namespace KonbiCloud.Transactions.Dtos
{
    public class ProductTransactionDto
    {
        public Products.Product Product { get; set; }
        public decimal Amount { get; set; }
        public decimal Price { get; set; }
        public string TagId { get; set; }
    }
}