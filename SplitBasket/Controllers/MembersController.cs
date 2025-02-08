using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using SplitBasket.Data;
using SplitBasket.Models;

namespace SplitBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Members, each represented by an MemberDto.
        /// </summary>
        /// <returns>
        /// 200 Ok
        /// List of members with it's Id, Name, Amount owed and Amount paid.
        /// </returns>
        /// <example>
        /// GET: api/Members/List -> [{MemberId:1, Name: Dhruv, AmountOwed : 25.3, AmountPaid: 100},{....},{....}]
        /// </example>
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> ListMembers()
        {
            var totalMembers = await _context.Members.CountAsync();
            if (totalMembers == 0)
            {
                return NotFound("No members found.");
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

            return Ok(memberDtos);
        }

        /// <summary>
        /// Returns a member specified by {id}.
        /// </summary>
        /// <returns>
        /// 200 Ok
        /// A member with it's Id, Name, Amount owed and Amount paid.
        /// or
        /// 404 Not Found when there is no Member of that id
        /// </returns>
        /// <example>
        /// GET: api/Members/Find/{id} -> {MemberId:1, Name: Dhruv, AmountOwed : 25.3, AmountPaid: 100}
        /// </example>
        [HttpGet("Find/{id}")]
        public async Task<ActionResult<MemberDto>> FindMember(int id)
        {
            var totalMembers = await _context.Members.CountAsync();
            if (totalMembers == 0)
            {
                return NotFound("No members found.");
            }

            var member = await _context.Members
                .Include(m => m.Purchases)
                    .ThenInclude(p => p.GroupPurchases)
                        .ThenInclude(gp => gp.GroceryItem)
                .FirstOrDefaultAsync(m => m.MemberId == id);

            if (member == null)
            {
                return NotFound($"Member with ID {id} not found.");
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

            return Ok(memberDto);
        }

        /// <summary>
        /// It updates a Member
        /// </summary>
        /// <param name="id">The ID of member to be updated</param>
        /// <returns>
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 200 Ok
        /// </returns>
        /// <example>
        /// api/Members/Update/1 -> Updates a member of id 1
        /// </example>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateMember(int id, UpdMemberDto updatememberDto)
        {
            if (id != updatememberDto.MemberId)
            {
                return BadRequest("Member ID mismatch.");
            }

            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound("Member not found.");
            }

            member.Name = updatememberDto.Name;
            member.EmailId = updatememberDto.EmailId;

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound("Member not found after concurrency check.");
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Member {id} updated successfully.");
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
        /// 201 Created
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// api/Members/Add -> Add the Member in the Members table
        /// </example>
        [HttpPost("Add")]
        public async Task<ActionResult<AddMemberDto>> AddMember(AddMemberDto addmemberDto)
        {
            var existingMember = await _context.Members.FirstOrDefaultAsync(m => m.EmailId == addmemberDto.EmailId);
            if (existingMember != null)
            {
                return BadRequest("Member already exists.");
            }

            Member member = new Member()
            {
                Name = addmemberDto.Name,
                EmailId = addmemberDto.EmailId
            };

            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            
            var responseDto = new AddMemberDto()
            {
                Name = member.Name,
                EmailId = member.EmailId
            };

            return CreatedAtAction(nameof(FindMember), new { id = member.MemberId },
            new { message = $"Member {member.MemberId} added successfully.", memberId = member.MemberId });
        }


        /// <summary>
        /// Delete a Member specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Member we want to delete</param>
        /// <returns>
        /// 201 No Content
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// api/Members/Delete/1 -> Deletes the member associated with id 1
        /// </example>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound("Member not found.");
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return Ok($"Member {id} deleted successfully.");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }
    }
}
