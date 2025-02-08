using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitBasket.Models
{
    public class GroupPurchase
    {
        [Key]
        public int GroupPurchaseId { get; set; }



        [ForeignKey("GroceryItems")]
        public int GroceryItemId { get; set; }
        public virtual GroceryItem GroceryItem { get; set; }



        [ForeignKey("Purchases")]
        public int? PurchaseId { get; set; }  // Nullable int (allows NULL)
        public virtual Purchase? Purchase { get; set; }




        public bool IsBought
        {
            get => PurchaseId.HasValue;  // If PurchaseId is not null, IsBought is true
            set { }  // We do not need a setter here
        }
    }



    public class GroupPurchaseDto
    {
        [Key]
        public int GroupPurchaseId { get; set; }

        public int GroceryItemId { get; set; }

        public int? PurchaseId { get; set; }

        public bool IsBought { get => PurchaseId.HasValue; set { } }
    }


    public class AddGroupPurchaseDto
    {

        public int GroceryItemId { get; set; }

        public int? PurchaseId { get; set; }

        public bool IsBought { get => PurchaseId.HasValue; set { } }
    }
}
