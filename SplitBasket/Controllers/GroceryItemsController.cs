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
    public class GroceryItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroceryItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of GroceryItems, each represented by a GroceryItemDto.
        /// </summary>
        /// <param name="GroceryItemDto">Includes ItemId, Name, Quantity, UnitPrice, TotalAmount, MemberName, and DatePurchased</param>
        /// <returns>
        /// 200 OK with a list of grocery items.
        /// </returns>
        /// <example>
        /// GET: api/GroceryItems -> [{ItemId:1, Name: "Milk", Quantity: 2, UnitPrice: 15, TotalAmount: 30, MemberName: "John", DatePurchased: "2025-02-01"}]
        /// </example>
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<GroceryItemDto>>> GetGroceryItems()
        {
            var groceryItems = await _context.GroceryItems
                .Include(g => g.GroupPurchases)
                .ThenInclude(gp => gp.Purchase)
                .ThenInclude(p => p.Member)
                .ToListAsync();

            var groceryItemDtos = groceryItems.Select(g => new GroceryItemDto
            {
                ItemId = g.ItemId,
                Name = g.Name,
                Quantity = g.Quantity,
                UnitPrice = g.Price,
                TotalAmount =(int)( g.Quantity * g.Price),
                MemberName = g.GroupPurchases.FirstOrDefault()?.Purchase?.Member?.Name ?? "Unknown",
                DatePurchased = g.GroupPurchases.FirstOrDefault()?.Purchase?.DatePurchased ?? DateOnly.FromDateTime(DateTime.Now)
            }).ToList();

            return Ok(groceryItemDtos);
        }

        /// <summary>
        /// Returns a GroceryItem specified by its {id}, represented by a GroceryItemDto.
        /// </summary>
        /// <param name="id">GroceryItem id</param>
        /// <param name="GroceryItemDto">Includes ItemId, Name, Quantity, UnitPrice, TotalAmount, MemberName, and DatePurchased</param>
        /// <returns>
        /// 200 OK if found with GroceryItemDto, 404 Not Found if not found.
        /// </returns>
        /// <example>
        /// GET: api/GroceryItems/Find/{id} -> {ItemId:1, Name: "Milk", Quantity: 2, UnitPrice: 15, TotalAmount: 30, MemberName: "John", DatePurchased: "2025-02-01"}
        /// </example>
        [HttpGet("Find/{id}")]
        public async Task<ActionResult<GroceryItemDto>> GetGroceryItem(int id)
        {
            var groceryItem = await _context.GroceryItems
                .Include(g => g.GroupPurchases)
                .ThenInclude(gp => gp.Purchase)
                .ThenInclude(p => p.Member)
                .FirstOrDefaultAsync(g => g.ItemId == id);

            if (groceryItem == null)
            {
                return NotFound();
            }

            var groceryItemDto = new GroceryItemDto
            {
                ItemId = groceryItem.ItemId,
                Name = groceryItem.Name,
                Quantity = groceryItem.Quantity,
                UnitPrice = groceryItem.Price,
                TotalAmount = (int)(groceryItem.Quantity * groceryItem.Price),
                MemberName = groceryItem.GroupPurchases.FirstOrDefault()?.Purchase?.Member?.Name ?? "Unknown",
                DatePurchased = groceryItem.GroupPurchases.FirstOrDefault()?.Purchase?.DatePurchased ?? DateOnly.FromDateTime(DateTime.Now)
            };

            return Ok(groceryItemDto);
        }

        /// <summary>
        /// Updates a GroceryItem specified by its {id}.
        /// </summary>
        /// <param name="id">The id of the grocery item to update</param>
        /// <param name="groceryItem">The updated grocery item information</param>
        /// <returns>
        /// 400 Bad Request if the ID in the route does not match, 404 Not Found if the item doesn't exist, 204 No Content if successful.
        /// </returns>
        /// <example>
        /// api/GroceryItems/Update/1 -> Updates grocery item with id 1
        /// </example>
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateGroceryItem(int id, UpdItemDto groceryItemDto)
        {
            if (id != groceryItemDto.ItemId)
            {
                return BadRequest("Item ID mismatch.");
            }

            var groceryItem = await _context.GroceryItems.FindAsync(id);
            if (groceryItem == null)
            {
                return NotFound("Grocery item not found.");
            }

            groceryItem.Name = groceryItemDto.Name;
            groceryItem.Quantity = groceryItemDto.Quantity;
            groceryItem.Price = groceryItemDto.UnitPrice;

            _context.Entry(groceryItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroceryItemExists(id))
                {
                    return NotFound("Grocery item not found after concurrency check." );
                }
                else
                {
                    throw;
                }
            }

            return Ok($"Grocery item {id} updated successfully.");
        }

        /// <summary>
        /// Adds a new GroceryItem.
        /// </summary>
        /// <param name="groceryItemDto">The information needed to add a grocery item</param>
        /// <returns>
        /// 201 Created if successful, 400 Bad Request for invalid data.
        /// </returns>
        /// <example>
        /// api/GroceryItems/Add -> Add grocery item
        /// </example>
        [HttpPost("Add")]
        public async Task<ActionResult> AddGroceryItem(AddItemDto groceryItemDto)
        {
            // Check if the grocery item is valid
            if (string.IsNullOrEmpty(groceryItemDto.Name) || groceryItemDto.Quantity <= 0 || groceryItemDto.UnitPrice <= 0)
            {
                return BadRequest("Invalid input data for grocery item.");
            }

            var groceryItem = new GroceryItem
            {
                Name = groceryItemDto.Name,
                Quantity = groceryItemDto.Quantity,
                Price = groceryItemDto.UnitPrice
            };

            _context.GroceryItems.Add(groceryItem);
            await _context.SaveChangesAsync();

            var responseDto = new
            {
                message = $"Grocery item '{groceryItem.Name}' added successfully.",
                itemId = groceryItem.ItemId,
                name = groceryItem.Name,
                quantity = groceryItem.Quantity,
                price = groceryItem.Price
            };

            return CreatedAtAction(nameof(GetGroceryItem), new { id = groceryItem.ItemId },
                new { message = $"Member {groceryItem.ItemId} added successfully.", ItemId = groceryItem.ItemId });
        }


        /// <summary>
        /// Deletes a GroceryItem specified by its {id}.
        /// </summary>
        /// <param name="id">The id of the grocery item to delete</param>
        /// <returns>
        /// 200 OK if deletion is successful, 404 Not Found if the item does not exist.
        /// </returns>
        /// <example>
        /// api/GroceryItems/Delete/1 -> Delete grocery item of id 1
        /// </example>
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteGroceryItem(int id)
        {
            var groceryItem = await _context.GroceryItems.FindAsync(id);
            if (groceryItem == null)
            {
                return NotFound("Grocery item not found.");
            }

            _context.GroceryItems.Remove(groceryItem);
            await _context.SaveChangesAsync();

            return Ok($"Grocery item {id} deleted successfully.");
        }

        private bool GroceryItemExists(int id)
        {
            return _context.GroceryItems.Any(e => e.ItemId == id);
        }
    }
}
