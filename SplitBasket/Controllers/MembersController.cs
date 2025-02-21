using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using SplitBasket.Data;
using SplitBasket.Interfaces;
using SplitBasket.Models;
using SplitBasket.Services;
using Microsoft.AspNetCore.Authorization;

namespace SplitBasket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly IMemberService _memberservice;

        public MembersController(IMemberService context)
        {
            _memberservice = context;
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
            IEnumerable<MemberDto> MemberDtos = await _memberservice.ListMembers();
            return Ok(MemberDtos);
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
            var member = await _memberservice.FindMember(id);

            if (member == null)
            {
                return NotFound($"Member with ID {id} doesn't exist");
            }
            else
            {
                return Ok(member);
            }
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
        [Authorize]
        public async Task<IActionResult> UpdateMember(int id, UpdMemberDto updateMemberDto)
        {
            if (id != updateMemberDto.MemberId)
            {
                return BadRequest(new { message = "Member ID mismatch." });
            }
            ServiceResponse response = await _memberservice.UpdateMember(id, updateMemberDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Member not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while updating the member." });
            }

            return Ok(new { message = $"Member with ID {id} updated successfully." });
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
        public async Task<ActionResult> AddMember(AddMemberDto addmemberDto)
        {
          
            ServiceResponse response = await _memberservice.AddMember(addmemberDto);

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while adding the member." });
            }

            return CreatedAtAction("FindMember", new { id = response.CreatedId }, new
            {
                message = $"Member added successfully with ID {response.CreatedId}",
                memberId = response.CreatedId
            });
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
        [Authorize]
        public async Task<ActionResult> DeleteMember(int id)
        {
            
            ServiceResponse response = await _memberservice.DeleteMember(id);

            
            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Member not found." });
            }
        
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the member." });
            }

            
            return Ok(new { message = $"Member with ID {id} deleted successfully." });
        }

    }
}
