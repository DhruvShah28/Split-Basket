using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using SplitBasket.Data.Migrations;
using SplitBasket.Models;

namespace SplitBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupPurchasesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupPurchasesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of all group purchases, including related grocery items and purchase details.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a collection of group purchases with details such as the grocery item and associated purchase.
        /// </remarks>
        /// <returns>A list of <see cref="GroupPurchaseDto"/> objects representing the group purchases.</returns>
        /// <example>
        /// GET /api/GroupPurchases/List -> [{"GroupPurchaseId": 1,"GroceryItemId": 2,"PurchaseId": 3,"IsBought": true},{"GroupPurchaseId": 2,"GroceryItemId": 1,"PurchaseId": null,"IsBought": false} ]
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<GroupPurchaseDto>>> GetGroupPurchases()
        {
            var groupPurchases = await _context.GroupPurchases
                .Where(gp => gp.PurchaseId.HasValue)
                .Include(gp => gp.GroceryItem)
                .Include(gp => gp.Purchase)
                .Select(gp => new GroupPurchaseDto
                {
                    GroupPurchaseId = gp.GroupPurchaseId,
                    GroceryItemId = gp.GroceryItemId,
                    PurchaseId = gp.PurchaseId,
                    IsBought = gp.PurchaseId.HasValue
                })
                .ToListAsync();

            return Ok(groupPurchases);
        }

        /// <summary>
        /// Retrieves details of a single group purchase by its ID.
        /// </summary>
        /// <remarks>
        /// This endpoint returns a single group purchase, including its associated grocery item and purchase details.
        /// </remarks>
        /// <param name="id">The ID of the group purchase to retrieve.</param>
        /// <returns>A <see cref="GroupPurchaseDto"/> object representing the group purchase.</returns>
        /// <example>
        /// GET /api/GroupPurchases/Find/1 -> {"GroupPurchaseId": 1,"GroceryItemId": 2,"PurchaseId": 3,"IsBought": true}
        /// </example>
        [HttpGet("Find/{id}")]
        public async Task<ActionResult<GroupPurchaseDto>> GetGroupPurchase(int id)
        {
            var groupPurchase = await _context.GroupPurchases
                .Where(gp => gp.PurchaseId.HasValue)
                .Include(gp => gp.GroceryItem)
                .Include(gp => gp.Purchase)
                .Where(gp => gp.GroupPurchaseId == id)
                .Select(gp => new GroupPurchaseDto
                {
                    GroupPurchaseId = gp.GroupPurchaseId,
                    GroceryItemId = gp.GroceryItemId,
                    PurchaseId = gp.PurchaseId,
                    IsBought = gp.PurchaseId.HasValue
                })
                .FirstOrDefaultAsync();

            if (groupPurchase == null)
            {
                return NotFound(new { message = $"Group purchase with ID {id} not found or Item is not Purchased." });
            }

            return Ok(groupPurchase);
        }

        /// <summary>
        /// Updates an existing group purchase.
        /// </summary>
        /// <remarks>
        /// This endpoint allows you to update the details of an existing group purchase, including the grocery item and associated purchase.
        /// </remarks>
        /// <param name="id">The ID of the group purchase to update.</param>
        /// <param name="groupPurchaseDto">The updated details for the group purchase.</param>
        /// <returns>No content if the update is successful, or an error message if the group purchase is not found or the ID doesn't match.</returns>
        /// <example>
        /// PUT /api/GroupPurchases/Update/1 -> Updates the group purchase with group purchase id 1
        /// </example>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> PutGroupPurchase(int id, GroupPurchaseDto groupPurchaseDto)
        {
            if (id != groupPurchaseDto.GroupPurchaseId)
            {
                return BadRequest(new { message = "Group purchase ID mismatch." });
            }

            var groupPurchase = await _context.GroupPurchases.FindAsync(id);

            if (groupPurchase == null)
            {
                return NotFound(new { message = $"Group purchase with ID {id} not found." });
            }

            // Check if GroceryItemId exists
            var groceryItemExists = await _context.GroceryItems.AnyAsync(g => g.ItemId == groupPurchaseDto.GroceryItemId);
            if (!groceryItemExists)
            {
                return BadRequest(new { message = $"Grocery item with ID {groupPurchaseDto.GroceryItemId} not found." });
            }

            groupPurchase.GroceryItemId = groupPurchaseDto.GroceryItemId;
            groupPurchase.PurchaseId = groupPurchaseDto.PurchaseId;

            _context.Entry(groupPurchase).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupPurchaseExists(id))
                {
                    return NotFound(new { message = $"Group purchase with ID {id} not found." });
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Group purchase with ID {id} updated successfully.");
        }



        /// <summary>
        /// Adds a new group purchase.
        /// </summary>
        /// <remarks>
        /// This endpoint allows you to add a new group purchase with the provided details, including the associated grocery item and purchase.
        /// </remarks>
        /// <param name="groupPurchaseDto">The details of the group purchase to create.</param>
        /// <returns>The created <see cref="GroupPurchaseDto"/> object, including the generated ID.</returns>
        /// <example>
        /// POST /api/GroupPurchases/Add -> Adds group purchase
        /// </example>
        [HttpPost("Add")]
        public async Task<ActionResult<GroupPurchaseDto>> PostGroupPurchase(AddGroupPurchaseDto groupPurchaseDto)
        {
            if (groupPurchaseDto == null)
            {
                return BadRequest(new { message = "Invalid group purchase data." });
            }

            // Check if GroceryItemId exists
            var groceryItemExists = await _context.GroceryItems.AnyAsync(g => g.ItemId == groupPurchaseDto.GroceryItemId);
            if (!groceryItemExists)
            {
                return BadRequest(new { message = $"Grocery item with ID {groupPurchaseDto.GroceryItemId} not found." });
            }

            var groupPurchase = new GroupPurchase
            {
                GroceryItemId = groupPurchaseDto.GroceryItemId,
                PurchaseId = groupPurchaseDto.PurchaseId
            };

            _context.GroupPurchases.Add(groupPurchase);
            await _context.SaveChangesAsync();

            var responseDto = new AddGroupPurchaseDto
            {
                GroceryItemId = groupPurchase.GroceryItemId,
                PurchaseId = groupPurchase.PurchaseId,
                IsBought = groupPurchase.PurchaseId.HasValue
            };

            return CreatedAtAction(nameof(GetGroupPurchase), new { id = groupPurchase.GroupPurchaseId }, 
                new { message = $"Group Purchase {groupPurchase.GroupPurchaseId} added successfully."});
        }


        /// <summary>
        /// Deletes a group purchase by ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows you to delete a specific group purchase by its ID.
        /// </remarks>
        /// <param name="id">The ID of the group purchase to delete.</param>
        /// <returns>No content if the deletion is successful, or an error message if the group purchase is not found.</returns>
        /// <example>
        /// DELETE /api/GroupPurchases/Delete/1 -> Deletes the group purchase of id 1
        /// </example>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteGroupPurchase(int id)
        {
            var groupPurchase = await _context.GroupPurchases.FindAsync(id);
            if (groupPurchase == null)
            {
                return NotFound(new { message = $"Group purchase with ID {id} not found." });
            }

            _context.GroupPurchases.Remove(groupPurchase);
            await _context.SaveChangesAsync();

            return Ok($"Group purchase of id {id} deleted successfully.");
        }



        /// <summary>
        /// Links an existing GroceryItem to a Purchase.
        /// </summary>
        /// <param name="groceryItemId">The ID of the Grocery Item.</param>
        /// <param name="purchaseId">The ID of the Purchase.</param>
        /// <returns>
        /// Returns a success message if the GroceryItem is linked to the Purchase.
        /// Returns a 404 if the Grocery Item or Purchase is not found.
        /// Returns a 400 if the Grocery Item is already linked to the Purchase.
        /// </returns>
        /// <example>
        /// api/GroupPurchases/LinkGroceryItem?groceryItemId=1&purchaseId=2 -> Links Grocery Item 1 to Purchase 2.
        /// </example>
        [HttpPost("LinkGroceryItem")]
        public async Task<IActionResult> LinkGroceryItemToPurchase(int groceryItemId, int purchaseId)
        {
            var groceryItem = await _context.GroceryItems.FindAsync(groceryItemId);
            var purchase = await _context.Purchases.FindAsync(purchaseId);

            if (groceryItem == null || purchase == null)
            {
                return NotFound("Grocery Item or Purchase not found.");
            }

            var existingLink = await _context.GroupPurchases
                .FirstOrDefaultAsync(gp => gp.GroceryItemId == groceryItemId && gp.PurchaseId == purchaseId);

            if (existingLink != null)
            {
                return BadRequest("Grocery Item is already linked to this Purchase.");
            }

            var groupPurchase = new GroupPurchase
            {
                GroceryItemId = groceryItemId,
                PurchaseId = purchaseId
            };

            _context.GroupPurchases.Add(groupPurchase);
            await _context.SaveChangesAsync();

            return Ok($"Grocery Item {groceryItemId} linked to Purchase {purchaseId}.");
        }






        /// <summary>
        /// Unlinks an existing GroceryItem from a Purchase.
        /// </summary>
        /// <param name="groceryItemId">The ID of the Grocery Item.</param>
        /// <param name="purchaseId">The ID of the Purchase.</param>
        /// <returns>
        /// Returns a success message if the GroceryItem is unlinked from the Purchase.
        /// Returns a 404 if the Grocery Item or Purchase is not found.
        /// Returns a 404 if the Grocery Item is not linked to the Purchase.
        /// </returns>
        /// <example>
        /// api/GroupPurchases/UnlinkGroceryItem?groceryItemId=1&purchaseId=2 -> Unlinks Grocery Item 1 from Purchase 2.
        /// </example>
        [HttpDelete("UnlinkGroceryItem")]
        public async Task<IActionResult> UnlinkGroceryItemFromPurchase(int groceryItemId, int purchaseId)
        {
            var groupPurchase = await _context.GroupPurchases
                .FirstOrDefaultAsync(gp => gp.GroceryItemId == groceryItemId && gp.PurchaseId == purchaseId);

            if (groupPurchase == null)
            {
                return NotFound("Grocery Item is not linked to this Purchase.");
            }

            _context.GroupPurchases.Remove(groupPurchase);
            await _context.SaveChangesAsync();

            return Ok($"Grocery Item {groceryItemId} unlinked from Purchase {purchaseId}.");
        }



        private bool GroupPurchaseExists(int id)
        {
            return _context.GroupPurchases.Any(e => e.GroupPurchaseId == id);
        }
    }
}
