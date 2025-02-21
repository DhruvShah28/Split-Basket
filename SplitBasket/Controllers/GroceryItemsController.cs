using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using SplitBasket.Models;
using SplitBasket.Interfaces;
using SplitBasket.Services;
using Microsoft.AspNetCore.Authorization;

namespace SplitBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroceryItemsController : ControllerBase
    {
        private readonly IGroceryItemService _groceryitemservice;

        public GroceryItemsController(IGroceryItemService context)
        {
            _groceryitemservice = context;
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
        public async Task<ActionResult<IEnumerable<GroceryItemDto>>> ListGroceryItems()
        {
            IEnumerable<GroceryItemDto> groceryItemDtos = await _groceryitemservice.ListGroceryItems();
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
        public async Task<ActionResult<GroceryItemDto>> FindGroceryItem(int id)
        {
            var groceryItem = await _groceryitemservice.FindGroceryItem(id);

            if (groceryItem == null)
            {
                return NotFound($"Grocery item with ID {id} doesn't exist.");
            }
            else
            {
                return Ok(groceryItem);
            }
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
        [Authorize]
        public async Task<IActionResult> UpdateGroceryItem(int id, UpdItemDto groceryItemDto)
        {
            if (id != groceryItemDto.ItemId)
            {
                return BadRequest(new { message = "Item ID mismatch." });
            }

            ServiceResponse response = await _groceryitemservice.UpdateGroceryItem(id, groceryItemDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Grocery item not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while updating the grocery item." });
            }

            return Ok(new { message = $"Grocery item with ID {id} updated successfully." });
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
        public async Task<ActionResult> AddGroceryItem([FromBody]AddItemDto groceryItemDto, [FromQuery] int memberId)
        {
            if (groceryItemDto == null || memberId <= 0)
            {
                return BadRequest("Invalid input.");
            }

            var response = await _groceryitemservice.AddGroceryItem(groceryItemDto, memberId);

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while adding the grocery item." });
            }

            // Return success with the ID of the newly created grocery item
            return CreatedAtAction(nameof(FindGroceryItem), new { id = response.CreatedId }, new
            {
                message = $"Grocery item added successfully with ID {response.CreatedId}",
                itemId = response.CreatedId
            });
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
        [Authorize]
        public async Task<ActionResult> DeleteGroceryItem(int id)
        {
            
            ServiceResponse response = await _groceryitemservice.DeleteGroceryItem(id);

            
            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Grocery item not found." });
            }
           
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the grocery item." });
            }

            
            return Ok(new { message = $"Grocery item with ID {id} deleted successfully." });
        }



        /// <summary>
        /// List of GroceryItem Names purchased by a member.
        /// </summary>
        /// <param name="id">The id of the member who purchased</param>
        /// <returns>
        /// 200 OK if it runs successfully
        /// </returns>
        /// <example>
        /// GET: api/GroceryItems/ByMember/{memberId} -> gives the list of grocery items purchased by a specific member
        /// </example>
        [HttpGet("ByMember/{memberId}")]
        public async Task<ActionResult<ServiceResponse>> GetGroceryItemsByMemberId(int memberId)
        {
            if (memberId <= 0)
            {
                return BadRequest("Invalid member ID.");
            }

            var response = await _groceryitemservice.GetGroceryItemsByMemberId(memberId);

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while fetching the grocery items." });
            }

            return Ok(response);
        }
    }
}
