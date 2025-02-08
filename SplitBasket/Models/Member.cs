using System.ComponentModel.DataAnnotations;

namespace SplitBasket.Models
{
    public class Member
    {
        [Key]
        public int MemberId { get; set; }

        public string Name { get; set; }

        public string EmailId { get; set; }

        // one member can have many purchases
        public ICollection<Purchase> Purchases { get; set; }

    }

    public class MemberDto
    {
        [Key]
        public int MemberId { get; set; }

        public string Name { get; set; }

        public float AmountOwed { get; set; }

        public float AmountPaid { get; set; }
    }


    public class UpdMemberDto
    {
        [Key]
        public int MemberId { get; set; }

        public string Name { get; set; }

        public string EmailId { get; set; }
    }


    public class AddMemberDto
    {
        public string Name { get; set; }

        public string EmailId { get; set; }
    }
}
