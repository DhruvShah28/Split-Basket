using SplitBasket.Interfaces;
using SplitBasket.Models;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using static SplitBasket.Services.PurchaseService;

namespace SplitBasket.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public PurchaseService(ApplicationDbContext context)
        {
            _context = context;
        }



        /// <summary> 
        /// Returns a list of purchases as a PurchaseHistoryDto.
        /// </summary>
        /// <returns>
        /// List of purchases including Id, Date Purchased, Name of Member who purchased, Name of Items, Total Amount of the purchase.
        /// </returns>
        /// <example>
        /// GET: api/Purchase/List -> [{ItemId:2, DatePurchased:"2025-02-02",MemberName:"Dhruv",ItemNames:["Milk","Protein Bar"],TotalAmount:55 },{....},{....}]
        /// </example>
        public async Task<IEnumerable<PurchaseHistoryDto>> ListPurchases()
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

            return purchaseHistoryDtos;
        }


        /// <summary> 
        /// Retrieves a specific purchase history by ID, including the related member and items, along with the total amount spent.
        /// </summary>
        /// <param name="id">The ID of the purchase to retrieve.</param>
        /// <returns>A PurchaseHistoryDto containing the purchase details, or a 404 Not Found if the purchase does not exist.</returns>
        /// <example>
        /// api/Purchases/Find/1 -> {ItemId:2, DatePurchased:"2025-02-02", MemberName:"Dhruv", ItemNames:["Milk", "Protein Bar"], TotalAmount:55}
        /// </example>
        public async Task<PurchaseHistoryDto> FindPurchase(int id)
        {
            var purchase = await _context.Purchases
                .Include(p => p.GroupPurchases)
                    .ThenInclude(gp => gp.GroceryItem)
                .Include(p => p.Member)
                .FirstOrDefaultAsync(p => p.PurchaseID == id);

            if (purchase == null)
            {
                return null;
            }

            var purchaseHistoryDto = new PurchaseHistoryDto()
            {
                PurchaseID = purchase.PurchaseID,
                DatePurchased = purchase.DatePurchased,
                MemberName = purchase.Member.Name,
                ItemNames = purchase.GroupPurchases.Select(gp => gp.GroceryItem.Name).ToList(),
                TotalAmount = purchase.GroupPurchases.Sum(gp => gp.GroceryItem.Quantity * gp.GroceryItem.Price)
            };

            return purchaseHistoryDto;
        }



        /// <summary>
        /// Updates the details of an existing purchase specified by its ID.
        /// </summary>
        /// <param name="id">The ID of the purchase to update.</param>
        /// <param name="aupPurchaseDto">The DTO containing the updated purchase details.</param>
        /// <returns>
        /// Status Updated if update is successful
        /// </returns>
        public async Task<ServiceResponse> UpdatePurchase(int id, UpdPurchaseDto aupPurchaseDto)
        {
            ServiceResponse serviceResponse = new();

           
            if (id != aupPurchaseDto.PurchaseID)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Purchase ID mismatch.");
                return serviceResponse;
            }

           
            var purchase = await _context.Purchases.FindAsync(id);
            if (purchase == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Purchase not found.");
                return serviceResponse;
            }

            
            purchase.DatePurchased = aupPurchaseDto.DatePurchased;
            purchase.MemberId = aupPurchaseDto.MemberId;

            
            var memberExists = await _context.Members.AnyAsync(m => m.MemberId == aupPurchaseDto.MemberId);
            if (!memberExists)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Invalid MemberId.");
                return serviceResponse;
            }

            
            _context.Entry(purchase).State = EntityState.Modified;

            try
            {
               
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;

                
                if (!await _context.Purchases.AnyAsync(p => p.PurchaseID == id))
                {
                    serviceResponse.Messages.Add("Purchase not found after concurrency check.");
                    serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                }
                else
                {
                    serviceResponse.Messages.Add("An error occurred while updating the purchase.");
                }

                return serviceResponse;
            }

            
            serviceResponse.Status = ServiceResponse.ServiceStatus.Updated;
            serviceResponse.Messages.Add($"Purchase {id} updated successfully.");
            return serviceResponse;
        }


        /// <summary>
        /// Creates a new purchase record based on the provided details in the AUPurchaseDto.
        /// </summary>
        /// <param name="aupPurchaseDto">The DTO containing the details of the new purchase.</param>
        /// <returns>status Created if the purchase is successfully created</returns>
        public async Task<ServiceResponse> AddPurchase(AddPurchaseDto aupPurchaseDto)
        {
            ServiceResponse serviceResponse = new();

          
            var memberExists = await _context.Members
                .AnyAsync(m => m.MemberId == aupPurchaseDto.MemberId);

            if (!memberExists)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Invalid MemberId.");
                return serviceResponse;
            }

            try
            {
                var purchase = new Purchase
                {
                    DatePurchased = aupPurchaseDto.DatePurchased,
                    MemberId = aupPurchaseDto.MemberId
                };

                
                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                
                serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
                serviceResponse.CreatedId = purchase.PurchaseID; 
            }
            catch (Exception ex)
            {
               
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the Purchase.");
                serviceResponse.Messages.Add(ex.Message);
            }

            return serviceResponse;
        }


        /// <summary>
        /// Deletes an existing purchase record specified by its ID.
        /// </summary>
        /// <param name="id">The ID of the purchase to delete.</param>
        /// <returns>status Deleted if the deletion is successful</returns>
        //public async Task<ServiceResponse> DeletePurchase(int id)
        //{
        //    ServiceResponse serviceResponse = new();


        //    var purchase = await _context.Purchases.FindAsync(id);
        //    if (purchase == null)
        //    {

        //        serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
        //        serviceResponse.Messages.Add("Purchase cannot be deleted because it does not exist.");
        //        return serviceResponse;
        //    }

        //    try
        //    {

        //        _context.Purchases.Remove(purchase);
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {

        //        serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
        //        serviceResponse.Messages.Add("Error encountered while deleting the purchase.");
        //        serviceResponse.Messages.Add(ex.Message); 
        //        return serviceResponse;
        //    }


        //    serviceResponse.Status = ServiceResponse.ServiceStatus.Deleted;
        //    return serviceResponse;
        //}


        public async Task<ServiceResponse> DeletePurchase(int id)
        {
            var response = new ServiceResponse();

            // Fetch the purchase record and related GroupPurchases
            var purchase = await _context.Purchases
                .FirstOrDefaultAsync(p => p.PurchaseID == id);

            if (purchase == null)
            {
                response.Status = ServiceResponse.ServiceStatus.Error;
                response.Messages.Add("Purchase not found.");
                return response;
            }

            // Fetch the related group purchases and set IsBought = false
            var groupPurchases = await _context.GroupPurchases
                .Where(gp => gp.PurchaseId == id)
                .ToListAsync();

            foreach (var groupPurchase in groupPurchases)
            {
                groupPurchase.IsBought = false;
            }

            // Save changes to the GroupPurchases table
            await _context.SaveChangesAsync();

            // Delete the purchase record
            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            response.Status = ServiceResponse.ServiceStatus.Deleted;
            response.Messages.Add("Purchase deleted successfully.");

            return response;
        }




    }
}
