using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitBasket.Data.Migrations;
using SplitBasket.Interfaces;
using SplitBasket.Models;
using SplitBasket.Models.ViewModels;

namespace SplitBasket.Controllers
{
    public class GroceryItemsPageController : Controller
    {
        private readonly IPurchaseService _purchaseservice;
        private readonly IGroceryItemService _groceryitemservice;
        private readonly IGroupPurchaseService _grouppurchaseservice;
        private readonly IMemberService _memberservice;

        // Dependency injection of service interfaces
        public GroceryItemsPageController(IMemberService memberService, IPurchaseService purchaseService, IGroceryItemService groceryItemService, IGroupPurchaseService groupPurchaseService)
        {
            _purchaseservice = purchaseService;
            _groceryitemservice = groceryItemService;
            _grouppurchaseservice = groupPurchaseService;
            _memberservice = memberService;
        }

        // Show List of Grocery Items
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: GroceryItemsPage/ListGroceryItems
        [HttpGet("ListGroceryItems")]
        public async Task<IActionResult> List()
        {
            IEnumerable<GroceryItemDto?> groceryItemDtos = await _groceryitemservice.ListGroceryItems();
            return View(groceryItemDtos);
        }

        // GET: GroceryItemsPage/GroceryItemDetails/{id}
        [HttpGet("GroceryItemDetails/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            GroceryItemDto? groceryItemDto = await _groceryitemservice.FindGroceryItem(id);

            if (groceryItemDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Could not find grocery item"] });
            }
            else
            {
                return View(groceryItemDto);
            }
        }

        // GET: GroceryItemsPage/AddGroceryItem
        [HttpGet("AddGroceryItem")]
        public async Task<IActionResult> Add()
        {
            // Assuming you have a service to get the list of members
            var members = await _memberservice.ListMembers();

            // Pass the list of members to the view
            ViewBag.Members = members;

            return View();
        }


        //// POST: GroceryItemsPage/AddGroceryItem
        //[HttpPost("AddGroceryItem")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Add(AddItem viewModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        // You can access AddItemDto and MemberId here
        //        var groceryItemDto = viewModel.AddItemDto;
        //        var memberId = viewModel.MemberId;

        //        // Pass AddItemDto and MemberId to the service method
        //        await _groceryitemservice.AddGroceryItem(groceryItemDto);
        //        return RedirectToAction("List");
        //    }

        //    return View(viewModel);
        //}


        [HttpPost("AddGroceryItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddItem viewModel)
        {
            if (ModelState.IsValid)
            {
                var groceryItemDto = viewModel.AddItemDto;
                var memberId = viewModel.MemberId;

                // Call the service method to add the grocery item and associate it with the member
                await _groceryitemservice.AddGroceryItem(groceryItemDto, memberId);

                return RedirectToAction("List");
            }

            return View(viewModel);
        }




        // GET: GroceryItemsPage/EditGroceryItem/{id}
        [HttpGet("EditGroceryItem/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            GroceryItemDto? groceryItemDto = await _groceryitemservice.FindGroceryItem(id);

            if (groceryItemDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Grocery item not found"] });
            }

            var updateGroceryItemDto = new UpdItemDto
            {
                ItemId = groceryItemDto.ItemId,
                Name = groceryItemDto.Name,
                Quantity = groceryItemDto.Quantity,
                UnitPrice = groceryItemDto.UnitPrice
            };

            return View(updateGroceryItemDto);
        }

        // POST: GroceryItemsPage/EditGroceryItem/{id}
        [HttpPost("EditGroceryItem/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdItemDto updateGroceryItemDto)
        {
            if (id != updateGroceryItemDto.ItemId)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Invalid Grocery Item ID"] });
            }

            if (ModelState.IsValid)
            {
                var serviceResponse = await _groceryitemservice.UpdateGroceryItem(id, updateGroceryItemDto);

                if (serviceResponse.Status == ServiceResponse.ServiceStatus.Error)
                {
                    return View("Error", new ErrorViewModel() { Errors = serviceResponse.Messages });
                }

                return RedirectToAction("Details", new { id });
            }

            return View(updateGroceryItemDto);
        }

        // GET: GroceryItemsPage/DeleteGroceryItem/{id}
        [HttpGet("DeleteGroceryItem/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            GroceryItemDto? groceryItemDto = await _groceryitemservice.FindGroceryItem(id);

            if (groceryItemDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Grocery item not found"] });
            }

            return View(groceryItemDto);
        }

        // POST: GroceryItemsPage/DeleteGroceryItem/{id}
        [HttpPost("DeleteGroceryItem/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            ServiceResponse response = await _groceryitemservice.DeleteGroceryItem(id);

            if (response.Status == ServiceResponse.ServiceStatus.Deleted)
            {
                return RedirectToAction("List", "GroceryItemsPage");
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages });
            }
        }
    }
}
