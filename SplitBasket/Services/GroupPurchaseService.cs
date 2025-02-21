using SplitBasket.Interfaces;
using SplitBasket.Models;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using static SplitBasket.Services.GroupPurchaseService;

namespace SplitBasket.Services
{
    public class GroupPurchaseService : IGroupPurchaseService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public GroupPurchaseService(ApplicationDbContext context)
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
        public async Task<IEnumerable<GroupPurchaseDto>> ListGroupPurchases()
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

            return groupPurchases;
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
        public async Task<GroupPurchaseDto> FindGroupPurchase(int id)
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
                return null;
            }

            return groupPurchase;
        }

        /// <summary>
        /// Updates an existing group purchase.
        /// </summary>
        /// <remarks>
        /// This endpoint allows you to update the details of an existing group purchase, including the grocery item and associated purchase.
        /// </remarks>
        /// <param name="id">The ID of the group purchase to update.</param>
        /// <param name="groupPurchaseDto">The updated details for the group purchase.</param>
        /// <returns>
        /// status Updated if update is successful
        /// </returns>
        public async Task<ServiceResponse> UpdateGroupPurchase(int id, GroupPurchaseDto groupPurchaseDto)
        {
            ServiceResponse serviceResponse = new();

            if (id != groupPurchaseDto.GroupPurchaseId)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Group purchase ID mismatch.");
                return serviceResponse;
            }

            var groupPurchase = await _context.GroupPurchases.FindAsync(id);
            if (groupPurchase == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Group purchase with ID {id} not found.");
                return serviceResponse;
            }

            var groceryItemExists = await _context.GroceryItems.AnyAsync(g => g.ItemId == groupPurchaseDto.GroceryItemId);
            if (!groceryItemExists)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Grocery item with ID {groupPurchaseDto.GroceryItemId} not found.");
                return serviceResponse;
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
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;

                if (!await _context.GroupPurchases.AnyAsync(gp => gp.GroupPurchaseId == id))
                {
                    serviceResponse.Messages.Add("Group purchase not found after concurrency check.");
                    serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                }
                else
                {
                    serviceResponse.Messages.Add("An error occurred while updating the group purchase.");
                }

                return serviceResponse;
            }

            serviceResponse.Status = ServiceResponse.ServiceStatus.Updated;
            return serviceResponse;
        }




        /// <summary>
        /// Adds a new group purchase.
        /// </summary>
        /// <remarks>
        /// This endpoint allows you to add a new group purchase with the provided details, including the associated grocery item and purchase.
        /// </remarks>
        /// <param name="groupPurchaseDto">The details of the group purchase to create.</param>
        /// <returns>status Created if addition is successful</returns>
        public async Task<ServiceResponse> AddGroupPurchase(AddGroupPurchaseDto groupPurchaseDto)
        {
            ServiceResponse serviceResponse = new();

            if (groupPurchaseDto == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Invalid group purchase data.");
                return serviceResponse;
            }

            var groceryItemExists = await _context.GroceryItems.AnyAsync(g => g.ItemId == groupPurchaseDto.GroceryItemId);
            if (!groceryItemExists)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Grocery item with ID {groupPurchaseDto.GroceryItemId} not found.");
                return serviceResponse;
            }

            var groupPurchase = new GroupPurchase
            {
                GroceryItemId = groupPurchaseDto.GroceryItemId,
                PurchaseId = groupPurchaseDto.PurchaseId
            };

            try
            {
                _context.GroupPurchases.Add(groupPurchase);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the Group Purchase.");
                serviceResponse.Messages.Add(ex.Message);
                return serviceResponse;
            }

            serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
            serviceResponse.CreatedId = groupPurchase.GroupPurchaseId;
            return serviceResponse;
        }



        /// <summary>
        /// Deletes a group purchase by ID.
        /// </summary>
        /// <remarks>
        /// This endpoint allows you to delete a specific group purchase by its ID.
        /// </remarks>
        /// <param name="id">The ID of the group purchase to delete.</param>
        /// <returns>status Deleted if the deletion is successful</returns>
        public async Task<ServiceResponse> DeleteGroupPurchase(int id)
        {
            ServiceResponse serviceResponse = new();

            var groupPurchase = await _context.GroupPurchases.FindAsync(id);
            if (groupPurchase == null)
            {
                
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add($"Group purchase with ID {id} not found.");
                return serviceResponse;
            }

            try
            {
               
                _context.GroupPurchases.Remove(groupPurchase);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Error encountered while deleting the group purchase.");
                serviceResponse.Messages.Add(ex.Message); 
                return serviceResponse;
            }

            
            serviceResponse.Status = ServiceResponse.ServiceStatus.Deleted;
            return serviceResponse;
        }
        public async Task<IEnumerable<GroceryItemDto>> GetItemsWithIsBoughtFalse()
        {
            var items = await _context.GroupPurchases
                .Where(gp => gp.IsBought == false)
                .Select(gp => new GroceryItemDto  
                {
                    ItemId = gp.GroceryItem.ItemId,
                    Name = gp.GroceryItem.Name,
                    UnitPrice = gp.GroceryItem.Price,
                    Quantity = gp.GroceryItem.Quantity
                })
                .ToListAsync();

            return items;
        }


        public async Task<GroupPurchase> GetGroupPurchaseByPurchaseId(int purchaseId)
        {
            return await _context.GroupPurchases
                .FirstOrDefaultAsync(gp => gp.PurchaseId == purchaseId);
        }

        //public async Task AddPurchaseWithItems(AddPurchaseDto purchaseDto, List<int> itemIds)
        //{
        //    var purchase = new Purchase
        //    {
        //        DatePurchased = purchaseDto.DatePurchased,
        //        MemberId = purchaseDto.MemberId,
        //    };

        //    _context.Purchases.Add(purchase);
        //    await _context.SaveChangesAsync();

        //    var items = await _context.GroceryItems
        //                               .Where(item => itemIds.Contains(item.ItemId))
        //                               .ToListAsync();

        //    foreach (var item in items)
        //    {
        //        var groupPurchase = new GroupPurchase
        //        {
        //            PurchaseId = purchase.PurchaseID,
        //            GroceryItemId = item.ItemId,
        //            IsBought= true
        //        };

        //        _context.GroupPurchases.Add(groupPurchase);

                
        //    }

        //    await _context.SaveChangesAsync();
        //}
        public async Task AddPurchaseWithItems(AddPurchaseDto purchaseDto, List<int> itemIds)
        {
            // 1. Create a new purchase entity
            var purchase = new Purchase
            {
                DatePurchased = purchaseDto.DatePurchased,
                MemberId = purchaseDto.MemberId,
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            // 2. Loop through the selected items
            foreach (var itemId in itemIds)
            {
                // 3. Check if the item is already in the GroupPurchases with IsBought = false
                var existingGroupPurchase = await _context.GroupPurchases
                    .FirstOrDefaultAsync(gp => gp.GroceryItemId == itemId && gp.IsBought == false);

                if (existingGroupPurchase != null)
                {
                    // 4. Update the existing GroupPurchase (mark as bought and assign PurchaseId)
                    existingGroupPurchase.IsBought = true;
                    existingGroupPurchase.PurchaseId = purchase.PurchaseID;
                }
                else
                {
                    // 5. If no existing GroupPurchase, create a new one
                    var groupPurchase = new GroupPurchase
                    {
                        PurchaseId = purchase.PurchaseID,
                        GroceryItemId = itemId,
                        IsBought = true
                    };

                    _context.GroupPurchases.Add(groupPurchase);
                }
            }

            // 6. Save changes to the database
            await _context.SaveChangesAsync();
        }


    }
}
