using SplitBasket.Interfaces;
using SplitBasket.Models;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using static SplitBasket.Services.MemberService;

namespace SplitBasket.Services
{
    public class MemberService : IMemberService
    {

        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public MemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Members, each represented by an MemberDto.
        /// </summary>
        /// <returns>
        /// List of members with it's Id, Name, Amount owed and Amount paid.
        /// </returns>
        public async Task<IEnumerable<MemberDto>> ListMembers()
        {
            var totalMembers = await _context.Members.CountAsync();
            if (totalMembers == 0)
            {
                return null;
            }

            var members = await _context.Members
                .Include(m => m.Purchases)
                    .ThenInclude(p => p.GroupPurchases)
                        .ThenInclude(gp => gp.GroceryItem)
                .ToListAsync();

            var memberDtos = members.Select(m => new MemberDto
            {
                MemberId = m.MemberId,
                Name = m.Name,
                AmountPaid = m.Purchases
                    .SelectMany(p => p.GroupPurchases)
                    .Sum(gp => gp.GroceryItem.Quantity * gp.GroceryItem.Price),
                AmountOwed = 0
            }).ToList();

            float totalAmountSpent = memberDtos.Sum(m => m.AmountPaid);

            foreach (var member in memberDtos)
            {
                float originalAmountOwed = totalAmountSpent / totalMembers;
                member.AmountOwed = originalAmountOwed - member.AmountPaid;
            }

            return memberDtos;
        }


        /// <summary>
        /// Returns a member specified by {id}.
        /// </summary>
        /// <returns>
        /// A member with it's Id, Name, Amount owed and Amount paid.
        /// </returns>
        /// <example>
        /// GET: api/Members/Find/{id} -> {MemberId:1, Name: Dhruv, AmountOwed : 25.3, AmountPaid: 100}
        /// </example>
        public async Task<MemberDto> FindMember(int id)
        {
            var totalMembers = await _context.Members.CountAsync();
            if (totalMembers == 0)
            {
                return null;
            }

            var member = await _context.Members
                .Include(m => m.Purchases)
                    .ThenInclude(p => p.GroupPurchases)
                        .ThenInclude(gp => gp.GroceryItem)
                .FirstOrDefaultAsync(m => m.MemberId == id);

            if (member == null)
            {
                return null;
            }

            float amountPaid = member.Purchases
                .SelectMany(p => p.GroupPurchases)
                .Sum(gp => gp.GroceryItem.Quantity * gp.GroceryItem.Price);

            float totalAmountSpent = await _context.GroupPurchases
                .Where(gp => gp.PurchaseId.HasValue)
                .SumAsync(gp => gp.GroceryItem.Quantity * gp.GroceryItem.Price);

            float originalAmountOwed = totalAmountSpent / totalMembers;
            float finalAmountOwed = originalAmountOwed - amountPaid;

            var memberDto = new MemberDto
            {
                MemberId = member.MemberId,
                Name = member.Name,
                AmountPaid = amountPaid,
                AmountOwed = finalAmountOwed
            };

            return memberDto;
        }


        /// <summary>
        /// It updates a Member
        /// </summary>
        /// <param name="id">The ID of member to be updated</param>
        /// <returns>
        /// status Updated if update is successful
        /// </returns>
        public async Task<ServiceResponse> UpdateMember(int id, UpdMemberDto updateMemberDto)
        {
            ServiceResponse serviceResponse = new();

           
            if (id != updateMemberDto.MemberId)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Member ID mismatch.");
                return serviceResponse;
            }

            
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Member not found.");
                return serviceResponse;
            }

          
            member.Name = updateMemberDto.Name;
            member.EmailId = updateMemberDto.EmailId;

            
            _context.Entry(member).State = EntityState.Modified;

            try
            {
                
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;

                if (!await _context.Members.AnyAsync(m => m.MemberId == id))
                {
                    serviceResponse.Messages.Add("Member not found after concurrency check.");
                    serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                }
                else
                {
                    serviceResponse.Messages.Add("An error occurred while updating the member.");
                }

                return serviceResponse;
            }

           
            serviceResponse.Status = ServiceResponse.ServiceStatus.Updated;
            return serviceResponse;
        }



        /// <summary>
        /// Adds a Member in the Members table
        /// </summary>
        /// <remarks>
        /// We add a Member as AddMemberDto which is the required information we input to add a member
        /// and MemberDto is the information about the Member displayed in the output
        /// </remarks>
        /// <param name="AddMemberDto">The required information to add the Member</param>
        /// <returns>
        /// status Created if Adding member is successful
        /// </returns>
        public async Task<ServiceResponse> AddMember(AddMemberDto addmemberDto)
        {
            ServiceResponse serviceResponse = new();

            
            Member member = new Member()
            {
                Name = addmemberDto.Name,
                EmailId = addmemberDto.EmailId
            };

            try
            {
                
                _context.Members.Add(member);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the Member.");
                serviceResponse.Messages.Add(ex.Message);
                return serviceResponse;
            }

            
            serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
            serviceResponse.CreatedId = member.MemberId; 
            return serviceResponse;
        }


        /// <summary>
        /// Delete a Member specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Member we want to delete</param>
        /// <returns>
        /// Status Deleted if deletion is successful
        /// </returns>
        public async Task<ServiceResponse> DeleteMember(int id)
        {
            ServiceResponse serviceResponse = new();

          
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
               
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Member cannot be deleted because it does not exist.");
                return serviceResponse;
            }

            try
            {
               
                _context.Members.Remove(member);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Error encountered while deleting the member.");
                serviceResponse.Messages.Add(ex.Message); 
                return serviceResponse;
            }

           
            serviceResponse.Status = ServiceResponse.ServiceStatus.Deleted;
            return serviceResponse;
        }





    }
}
