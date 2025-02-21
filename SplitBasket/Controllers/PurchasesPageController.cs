using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Interfaces;
using SplitBasket.Models;
using SplitBasket.Models.ViewModels;
using SplitBasket.Services;

namespace SplitBasket.Controllers
{
    public class PurchasesPageController : Controller
    {
        private readonly IMemberService _memberservice;
        private readonly IGroceryItemService _groceryitemservice;
        private readonly IGroupPurchaseService _grouppurchaseservice;
        private readonly IPurchaseService _purchaseservice;

        // Dependency injection of service interfaces
        public PurchasesPageController(IPurchaseService purchaseService, IMemberService memberService, IGroceryItemService groceryItemService, IGroupPurchaseService groupPurchaseService)
        {
            _memberservice = memberService;
            _groceryitemservice = groceryItemService;
            _grouppurchaseservice = groupPurchaseService;
            _purchaseservice = purchaseService;
        }

        // Show List of Purchases
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }




        // GET: PurchasesPage/ListPurchases
        [HttpGet("ListPurchases")]
        public async Task<IActionResult> List()
        {
            IEnumerable<PurchaseHistoryDto?> purchaseDtos = await _purchaseservice.ListPurchases();
            return View(purchaseDtos);
        }




        // GET: PurchasesPage/PurchaseDetails/{id}
        [HttpGet("PurchaseDetails/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            PurchaseHistoryDto? purchaseDto = await _purchaseservice.FindPurchase(id);

            if (purchaseDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Could not find purchase"] });
            }
            else
            {
                return View(purchaseDto);
            }
        }



        // GET: PurchasesPage/AddPurchase
        [HttpGet("AddPurchase")]
        public async Task<IActionResult> Add()
        {
            var members = await _memberservice.ListMembers();

            var items = await _grouppurchaseservice.GetItemsWithIsBoughtFalse();

            ViewBag.Members = members;
            ViewBag.Items = items;

            return View();
        }



        // POST: PurchasesPage/AddPurchase
        [HttpPost("AddPurchase")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddPurchaseDto purchaseDto, List<int> itemIds)
        {
            if (ModelState.IsValid)
            {
                // Call the modified AddPurchaseWithItems method to handle the purchase and associated items
                await _grouppurchaseservice.AddPurchaseWithItems(purchaseDto, itemIds);
                return RedirectToAction("List");
            }

            // In case of validation failure, return the purchaseDto with errors to the view
            return View(purchaseDto);
        }




        // GET: PurchasesPage/EditPurchase/{id}
        [HttpGet("EditPurchase/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            PurchaseHistoryDto? purchaseDto = await _purchaseservice.FindPurchase(id);

            if (purchaseDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Purchase not found"] });
            }

            var updatePurchaseDto = new UpdPurchaseDto
            {
                PurchaseID = purchaseDto.PurchaseID,
                DatePurchased = purchaseDto.DatePurchased
            };

            var members = await _memberservice.ListMembers();

            ViewBag.Members = members;

            return View(updatePurchaseDto);
        }

        // POST: PurchasesPage/EditPurchase/{id}
        [HttpPost("EditPurchase/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdPurchaseDto updatePurchaseDto)
        {
            if (id != updatePurchaseDto.PurchaseID)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Invalid Purchase ID"] });
            }

            if (ModelState.IsValid)
            {
                var serviceResponse = await _purchaseservice.UpdatePurchase(id, updatePurchaseDto);

                if (serviceResponse.Status == ServiceResponse.ServiceStatus.Error)
                {
                    return View("Error", new ErrorViewModel() { Errors = serviceResponse.Messages });
                }

                return RedirectToAction("Details", new { id });
            }

            return View(updatePurchaseDto);
        }



        // GET: PurchasesPage/DeletePurchase/{id}
        [HttpGet("DeletePurchase/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            PurchaseHistoryDto? purchaseDto = await _purchaseservice.FindPurchase(id);

            if (purchaseDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Purchase not found"] });
            }

            return View(purchaseDto);
        }



        // POST: PurchasesPage/DeletePurchase/{id}
        [HttpPost("DeletePurchase/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            ServiceResponse response = await _purchaseservice.DeletePurchase(id);

            if (response.Status == ServiceResponse.ServiceStatus.Deleted)
            {
                return RedirectToAction("List", "PurchasesPage");
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages });
            }
        }
    }
}
