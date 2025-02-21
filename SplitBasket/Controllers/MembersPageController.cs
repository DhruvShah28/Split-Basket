using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitBasket.Interfaces;
using SplitBasket.Models;
using SplitBasket.Models.ViewModels;

namespace SplitBasket.Controllers
{
    public class MembersPageController : Controller
    {
        private readonly IPurchaseService _purchaseservice;
        private readonly IGroceryItemService _groceryitemservice;
        private readonly IMemberService _memberservice;

        // Dependency injection of service interfaces
        public MembersPageController(IPurchaseService purchaseService, IGroceryItemService groceryItemService, IMemberService memberService)
        {
            _purchaseservice = purchaseService;
            _groceryitemservice = groceryItemService;
            _memberservice = memberService;
        }

        // Show List of Group Purchases on Index page
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: MembersPage/ListMembers
        [HttpGet("ListMembers")]
        public async Task<IActionResult> List()
        {
            IEnumerable<MemberDto?> memberDtos = await _memberservice.ListMembers();
            return View(memberDtos);
        }

        // GET: MembersPage/MemberDetails/{id}
        //[HttpGet("MemberDetails/{id}")]
        //public async Task<IActionResult> Details(int id)
        //{
        //    MemberDto? memberDto = await _memberservice.FindMember(id);

        //    if (memberDto == null)
        //    {
        //        return View("Error", new ErrorViewModel() { Errors = ["Could not find member" ] });
        //    }
        //    else
        //    {
        //        return View(memberDto);
        //    }
        //}
        // GET: MembersPage/MemberDetails/{id}
        [HttpGet("MemberDetails/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            MemberDto? memberDto = await _memberservice.FindMember(id);

            if (memberDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = new List<string> { "Could not find member" } });
            }
            else
            {
                // Fetch grocery items purchased by the member
                var groceryItemsResponse = await _groceryitemservice.GetGroceryItemsByMemberId(id);

                if (groceryItemsResponse.Status == ServiceResponse.ServiceStatus.Error)
                {
                    return View("Error", new ErrorViewModel() { Errors = groceryItemsResponse.Messages });
                }

                // Add the list of grocery item names to the model
                ViewBag.GroceryItems = groceryItemsResponse.Messages;  // These are item names

                return View(memberDto);
            }
        }


        // GET: MembersPage/AddMember
        [HttpGet("AddMember")]
        public IActionResult Add()
        {
            return View();
        }

        // POST: MembersPage/AddMember
        [HttpPost("AddMember")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddMemberDto memberDto)
        {
            if (ModelState.IsValid)
            {
                await _memberservice.AddMember(memberDto);
                return RedirectToAction("List");
            }

            return View(memberDto);
        }

        // GET: MembersPage/EditMember/{id}
        [HttpGet("EditMember/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            MemberDto? memberDto = await _memberservice.FindMember(id);

            if (memberDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Member not found" ]});
            }

            var updateMemberDto = new UpdMemberDto
            {
                MemberId = memberDto.MemberId,
                Name = memberDto.Name
            };

            return View(updateMemberDto);
        }

        // POST: MembersPage/EditMember/{id}
        [HttpPost("EditMember/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdMemberDto updateMemberDto)
        {
            if (id != updateMemberDto.MemberId)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Invalid Member ID"] });
            }

            if (ModelState.IsValid)
            {
                var serviceResponse = await _memberservice.UpdateMember(id, updateMemberDto);

                if (serviceResponse.Status == ServiceResponse.ServiceStatus.Error)
                {
                    return View("Error", new ErrorViewModel() { Errors = serviceResponse.Messages });
                }

                return RedirectToAction("Details", new { id });
            }

            return View(updateMemberDto);
        }


        // GET: MembersPage/DeleteMember/{id}
        [HttpGet("DeleteMember/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            MemberDto? memberDto = await _memberservice.FindMember(id);

            if (memberDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Member not found"] });
            }

            return View(memberDto);
        }

        // POST: MembersPage/DeleteMember/{id}
        [HttpPost("DeleteMember/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            ServiceResponse response = await _memberservice.DeleteMember(id);

            if (response.Status == ServiceResponse.ServiceStatus.Deleted)
            {
                return RedirectToAction("List", "MembersPage");
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages });
            }
        }
    }
}
