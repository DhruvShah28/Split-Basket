using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using SplitBasket.Models;

namespace SplitBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary> 
        /// Returns a list of purchases as a PurchaseHistoryDto.
        /// </summary>
        /// <returns>
        /// 200 OK - List of purchases including Id, Date Purchased, Name of Member who purchased, Name of Items, Total Amount of the purchase.
        /// </returns>
        /// <example>
        /// GET: api/Purchase/List -> [{ItemId:2, DatePurchased:"2025-02-02",MemberName:"Dhruv",ItemNames:["Milk","Protein Bar"],TotalAmount:55 },{....},{....}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<PurchaseHistoryDto>>> GetPurchases()
        {
            List<Purchase> purchases = await _context.Purchases
                .Include(p => p.GroupPurchases)
                    .ThenInclude(gp => gp.GroceryItem)
                .Include(p => p.Member)
                .ToListAsync();

            List<PurchaseHistoryDto> purchaseHistoryDtos = new List<PurchaseHistoryDto>();

            foreach (var purchase in purchases)
            {
                purchaseHistoryDtos.Add(new PurchaseHistoryDto()
                {
                    PurchaseID = purchase.PurchaseID,
                    DatePurchased = purchase.DatePurchased,
                    MemberName = purchase.Member.Name,
                    ItemNames = purchase.GroupPurchases.Select(gp => gp.GroceryItem.Name).ToList(),
                    TotalAmount = purchase.GroupPurchases.Sum(gp => gp.GroceryItem.Quantity * gp.GroceryItem.Price)
                });
            }

            // return 200 OK with PurchaseHistoryDtos
            return Ok(purchaseHistoryDtos);
        }


        /// <summary> 
        /// Retrieves a specific purchase history by ID, including the related member and items, along with the total amount spent.
        /// </summary>
        /// <param name="id">The ID of the purchase to retrieve.</param>
        /// <returns>A PurchaseHistoryDto containing the purchase details, or a 404 Not Found if the purchase does not exist.</returns>
        /// <example>
        /// api/Purchases/Find/1 -> {ItemId:2, DatePurchased:"2025-02-02", MemberName:"Dhruv", ItemNames:["Milk", "Protein Bar"], TotalAmount:55}
        /// </example>
        [HttpGet("Find/{id}")]
        public async Task<ActionResult<PurchaseHistoryDto>> FindPurchase(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.GroupPurchases)
                    .ThenInclude(gp => gp.GroceryItem)
                .Include(p => p.Member)
                .FirstOrDefaultAsync(p => p.PurchaseID == id);

            if (purchase == null)
            {
                return NotFound();
            }

            var purchaseHistoryDto = new PurchaseHistoryDto()
            {
                PurchaseID = purchase.PurchaseID,
                DatePurchased = purchase.DatePurchased,
                MemberName = purchase.Member.Name,
                ItemNames = purchase.GroupPurchases.Select(gp => gp.GroceryItem.Name).ToList(),
                TotalAmount = purchase.GroupPurchases.Sum(gp => gp.GroceryItem.Quantity * gp.GroceryItem.Price)
            };

            return Ok(purchaseHistoryDto);
        }



        /// <summary>
        /// Updates the details of an existing purchase specified by its ID.
        /// </summary>
        /// <param name="id">The ID of the purchase to update.</param>
        /// <param name="aupPurchaseDto">The DTO containing the updated purchase details.</param>
        /// <returns>200 Ok if the update is successful, 400 Bad Request if validation fails, 404 Not Found if the purchase is not found.</returns>
        /// <example>
        /// api/Purchases/Update/1 -> Updates a purchase of id 1
        /// </example>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutPurchase(int id, UpdPurchaseDto aupPurchaseDto)
        {
            if (id != aupPurchaseDto.PurchaseID)
            {
                return BadRequest("The ID in the URL does not match the ID in the body.");
            }

            var memberExists = await _context.Members
                .AnyAsync(m => m.MemberId == aupPurchaseDto.MemberId);
            if (!memberExists)
            {
                return BadRequest("Invalid MemberId.");
            }

            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound("Purchase not found.");
            }

            purchase.DatePurchased = aupPurchaseDto.DatePurchased;
            purchase.MemberId = aupPurchaseDto.MemberId;

            _context.Entry(purchase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseExists(id))
                {
                    return NotFound("Purchase not found after concurrency check.");
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Purchase {id} updated successfully.");
        }

        /// <summary>
        /// Creates a new purchase record based on the provided details in the AUPurchaseDto.
        /// </summary>
        /// <param name="aupPurchaseDto">The DTO containing the details of the new purchase.</param>
        /// <returns>201 Created if the purchase is successfully created, 400 Bad Request if validation fails.</returns>
        /// <example>
        /// api/Purchases/Add -> Add a purchase
        /// </example>
        [HttpPost("Add")]
        public async Task<ActionResult> PostPurchase(AddPurchaseDto aupPurchaseDto)
        {
            var memberExists = await _context.Members
                .AnyAsync(m => m.MemberId == aupPurchaseDto.MemberId);

            if (!memberExists)
            {
                return BadRequest("Invalid MemberId.");
            }

            var purchase = new Purchase
            {
                DatePurchased = aupPurchaseDto.DatePurchased,
                MemberId = aupPurchaseDto.MemberId,
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            var responseDto = new
            {
                message = $"Purchase added successfully for member with ID {aupPurchaseDto.MemberId}.",
                purchaseId = purchase.PurchaseID,
                memberId = purchase.MemberId,
                datePurchased = purchase.DatePurchased
            };

            return CreatedAtAction(nameof(FindPurchase), new { id = purchase.PurchaseID },
            new { message = $"Purchase {purchase.PurchaseID} added successfully.", purchaseId = purchase.PurchaseID });
        }

        /// <summary>
        /// Deletes an existing purchase record specified by its ID.
        /// </summary>
        /// <param name="id">The ID of the purchase to delete.</param>
        /// <returns>200 Ok if the deletion is successful, 404 Not Found if the purchase is not found.</returns>
        /// <example>
        /// api/Purchases/Delete/1 -> Delete purchase of id 1
        /// </example>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                return NotFound("Purchase not found.");
            }

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            return Ok($"Purchase {id} deleted successfully.");
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchases.Any(e => e.PurchaseID == id);
        }

        /// <summary>
        /// Retrieves a list of item names purchased by a specific member.
        /// </summary>
        /// <param name="memberId">The ID of the member whose purchased items are to be listed.</param>
        /// <returns>A list of grocery item names purchased by the specified member, or a NotFound status if the member doesn't exist.</returns>
        [HttpGet("GetPurchasedItemsByMember/{memberId}")]
        public async Task<ActionResult<IEnumerable<string>>> GetPurchasesByMember(int memberId)
        {
            var memberExists = await _context.Members
                .AnyAsync(m => m.MemberId == memberId);

            if (!memberExists)
            {
                return NotFound($"Member with ID {memberId} not found.");
            }

            var itemNames = await _context.Purchases
                .Where(p => p.MemberId == memberId)
                .SelectMany(p => p.GroupPurchases)
                .Select(gp => gp.GroceryItem.Name)
                .ToListAsync();

            return Ok(itemNames);
        }
    }
}
