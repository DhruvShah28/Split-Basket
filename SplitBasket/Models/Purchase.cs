using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;



namespace SplitBasket.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseID { get; set; }

        public DateOnly DatePurchased { get; set; }
        

        // One purchase is done by one member
        [ForeignKey("Members")]
        public int MemberId { get; set; }
        public virtual Member Member { get; set; }

        public virtual ICollection<GroupPurchase> GroupPurchases { get; set; } = new List<GroupPurchase>();
    }


    public class PurchaseHistoryDto
    {
        [Key]
        public int PurchaseID { get; set; }

        public DateOnly DatePurchased { get; set; }

        public string MemberName { get; set; }

        public List<string> ItemNames { get; set; }
        public float TotalAmount { get; set; }
    }


    public class UpdPurchaseDto
    {
        [Key]
        public int PurchaseID { get; set; }

        public DateOnly DatePurchased { get; set; }

        public int MemberId { get; set; }

    }


    public class AddPurchaseDto
    {
        public DateOnly DatePurchased { get; set; }

        public int MemberId { get; set; }

    }



}
