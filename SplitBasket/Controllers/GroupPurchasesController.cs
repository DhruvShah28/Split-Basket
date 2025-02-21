using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using SplitBasket.Data.Migrations;
using SplitBasket.Interfaces;
using SplitBasket.Models;
using SplitBasket.Services;

namespace SplitBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupPurchasesController : ControllerBase
    {
        private readonly IGroupPurchaseService _grouppurchaseservice;

        public GroupPurchasesController(IGroupPurchaseService context)
        {
            _grouppurchaseservice = context;
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
        public async Task<ActionResult<IEnumerable<GroupPurchaseDto>>> ListGroupPurchases()
        {
            IEnumerable<GroupPurchaseDto> groupPurchaseDtos = await _grouppurchaseservice.ListGroupPurchases();
            return Ok(groupPurchaseDtos);
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
        public async Task<ActionResult<GroupPurchaseDto>> FindGroupPurchase(int id)
        {
            var groupPurchase = await _grouppurchaseservice.FindGroupPurchase(id);

            if (groupPurchase == null)
            {
                return NotFound($"Group purchase with ID {id} doesn't exist");
            }
            else
            {
                return Ok(groupPurchase);
            }
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
        public async Task<IActionResult> UpdateGroupPurchase(int id, GroupPurchaseDto groupPurchaseDto)
        {
            if (id != groupPurchaseDto.GroupPurchaseId)
            {
                return BadRequest(new { message = "Group purchase ID mismatch." });
            }

            ServiceResponse response = await _grouppurchaseservice.UpdateGroupPurchase(id, groupPurchaseDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Group purchase not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while updating the group purchase." });
            }

            return Ok(new { message = $"Group purchase with ID {id} updated successfully." });
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
        public async Task<ActionResult> AddGroupPurchase(AddGroupPurchaseDto groupPurchaseDto)
        {
            ServiceResponse response = await _grouppurchaseservice.AddGroupPurchase(groupPurchaseDto);

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while adding the group purchase." });
            }

            return CreatedAtAction("FindGroupPurchase", new { id = response.CreatedId }, new
            {
                message = $"Group purchase added successfully with ID {response.CreatedId}",
                groupPurchaseId = response.CreatedId
            });
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
            ServiceResponse response = await _grouppurchaseservice.DeleteGroupPurchase(id);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Group purchase not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the group purchase." });
            }

            return Ok(new { message = $"Group purchase with ID {id} deleted successfully." });
        }

    }
}
