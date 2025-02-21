using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using SplitBasket.Interfaces;
using SplitBasket.Models;
using SplitBasket.Services;

namespace SplitBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseservice;

        public PurchasesController(IPurchaseService context)
        {
            _purchaseservice = context;
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
        [HttpGet("List")]
        public async Task<ActionResult<IEnumerable<PurchaseHistoryDto>>> ListPurchases()
        {
        
            IEnumerable<PurchaseHistoryDto> purchaseHistoryDtos = await _purchaseservice.ListPurchases();

           
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
            var purchaseHistoryDto = await _purchaseservice.FindPurchase(id);

            if (purchaseHistoryDto == null)
            {
                return NotFound($"Purchase with ID {id} doesn't exist.");
            }
            
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
        [Authorize]
        public async Task<IActionResult> PutPurchase(int id, UpdPurchaseDto aupPurchaseDto)
        {
            if (id != aupPurchaseDto.PurchaseID)
            {
                return BadRequest(new { message = "Purchase ID mismatch." });
            }

            ServiceResponse response = await _purchaseservice.UpdatePurchase(id, aupPurchaseDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Purchase not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while updating the purchase." });
            }

            return Ok(new { message = $"Purchase with ID {id} updated successfully." });
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
        public async Task<ActionResult> AddPurchase(AddPurchaseDto aupPurchaseDto)
        {
            
            ServiceResponse response = await _purchaseservice.AddPurchase(aupPurchaseDto);

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while adding the purchase." });
            }

           
            return CreatedAtAction("FindPurchase", new { id = response.CreatedId }, new
            {
                message = $"Purchase added successfully with ID {response.CreatedId}",
                purchaseId = response.CreatedId
            });
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
        [Authorize]
        public async Task<IActionResult> DeletePurchase(int id)
        {
           
            ServiceResponse response = await _purchaseservice.DeletePurchase(id);

            
            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Purchase not found." });
            }
           
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the purchase." });
            }

            return Ok(new { message = $"Purchase with ID {id} deleted successfully." });
        }




        /// <summary>
        /// Retrieves a list of item names purchased by a specific member.
        /// </summary>
        /// <param name="memberId">The ID of the member whose purchased items are to be listed.</param>
        /// <returns>A list of grocery item names purchased by the specified member, or a NotFound status if the member doesn't exist.</returns>
        //[HttpGet("GetPurchasedItemsByMember/{memberId}")]
        //public async Task<ActionResult<IEnumerable<string>>> GetPurchasesByMember(int memberId)
        //{
        //    var memberExists = await _context.Members
        //        .AnyAsync(m => m.MemberId == memberId);

        //    if (!memberExists)
        //    {
        //        return NotFound($"Member with ID {memberId} not found.");
        //    }

        //    var itemNames = await _context.Purchases
        //        .Where(p => p.MemberId == memberId)
        //        .SelectMany(p => p.GroupPurchases)
        //        .Select(gp => gp.GroceryItem.Name)
        //        .ToListAsync();

        //    return Ok(itemNames);
        //}
    }
}
