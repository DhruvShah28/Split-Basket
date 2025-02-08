using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.NetworkInformation;

namespace SplitBasket.Models
{
    public class GroceryItem
    {
        [Key]
        public int ItemId { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public float Price { get; set; }

        public virtual ICollection<GroupPurchase> GroupPurchases { get; set; } = new List<GroupPurchase>();
    }


    public class GroceryItemDto
    {
        [Key]
        public int ItemId { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public float UnitPrice { get; set; }

        public int TotalAmount { get; set; }

        public string MemberName { get; set; }

        public DateOnly DatePurchased { get; set; }

    }


    public class UpdItemDto
    {
        [Key]
        public int ItemId { get; set; }

        public string Name { get; set; }

        public int Quantity { get; set; }

        public float UnitPrice { get; set; }

    }

    public class AddItemDto
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public float UnitPrice { get; set; }

    }
}
