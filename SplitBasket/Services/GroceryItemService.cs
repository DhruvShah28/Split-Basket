using SplitBasket.Interfaces;
using SplitBasket.Models;
using Microsoft.EntityFrameworkCore;
using SplitBasket.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;
using static SplitBasket.Services.GroceryItemService;
using SplitBasket.Data.Migrations;

namespace SplitBasket.Services
{
    public class GroceryItemService : IGroceryItemService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public GroceryItemService(ApplicationDbContext context)
        {
            _context = context;
        }
        /// <summary>
        /// Returns a list of GroceryItems, each represented by a GroceryItemDto.
        /// </summary>
        /// <param name="GroceryItemDto">Includes ItemId, Name, Quantity, UnitPrice, TotalAmount, MemberName, and DatePurchased</param>
        /// <returns>
        /// list of grocery items.
        /// </returns>
        /// <example>
        /// GET: api/GroceryItems -> [{ItemId:1, Name: "Milk", Quantity: 2, UnitPrice: 15, TotalAmount: 30, MemberName: "John", DatePurchased: "2025-02-01"}]
        /// </example>
        public async Task<IEnumerable<GroceryItemDto>> ListGroceryItems()
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
                TotalAmount = (int)(g.Quantity * g.Price),
                MemberName = g.GroupPurchases.FirstOrDefault()?.Purchase?.Member?.Name ?? "Unknown",
                DatePurchased = g.GroupPurchases.FirstOrDefault()?.Purchase?.DatePurchased ?? DateOnly.FromDateTime(DateTime.Now)
            }).ToList();

            return groceryItemDtos;
        }

        /// <summary>
        /// Returns a GroceryItem specified by its {id}, represented by a GroceryItemDto.
        /// </summary>
        /// <param name="id">GroceryItem id</param>
        /// <param name="GroceryItemDto">Includes ItemId, Name, Quantity, UnitPrice, TotalAmount, MemberName, and DatePurchased</param>
        /// <returns>
        /// GroceryItemDto for a specific grocery item
        /// </returns>
        /// <example>
        /// GET: api/GroceryItems/Find/{id} -> {ItemId:1, Name: "Milk", Quantity: 2, UnitPrice: 15, TotalAmount: 30, MemberName: "John", DatePurchased: "2025-02-01"}
        /// </example>
        public async Task<GroceryItemDto> FindGroceryItem(int id)
        {
            var groceryItem = await _context.GroceryItems
                .Include(g => g.GroupPurchases)
                .ThenInclude(gp => gp.Purchase)
                .ThenInclude(p => p.Member)
                .FirstOrDefaultAsync(g => g.ItemId == id);

            if (groceryItem == null)
            {
                return null;
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

            return groceryItemDto;
        }

        /// <summary>
        /// Updates a GroceryItem specified by its {id}.
        /// </summary>
        /// <param name="id">The id of the grocery item to update</param>
        /// <param name="groceryItem">The updated grocery item information</param>
        /// <returns>
        /// status Updated if update is successful
        /// </returns>
        public async Task<ServiceResponse> UpdateGroceryItem(int id, UpdItemDto groceryItemDto)
        {
            ServiceResponse serviceResponse = new();

          
            if (id != groceryItemDto.ItemId)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Item ID mismatch.");
                return serviceResponse;
            }

            
            var groceryItem = await _context.GroceryItems.FindAsync(id);
            if (groceryItem == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Grocery item not found.");
                return serviceResponse;
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
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;

                if (!await _context.GroceryItems.AnyAsync(g => g.ItemId == id))
                {
                    serviceResponse.Messages.Add("Grocery item not found after concurrency check.");
                    serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                }
                else
                {
                    serviceResponse.Messages.Add("An error occurred while updating the grocery item.");
                }

                return serviceResponse;
            }

           
            serviceResponse.Status = ServiceResponse.ServiceStatus.Updated;
            serviceResponse.Messages.Add($"Grocery item {id} updated successfully.");
            return serviceResponse;
        }


        /// <summary>
        /// Adds a new GroceryItem.
        /// </summary>
        /// <param name="groceryItemDto">The information needed to add a grocery item</param>
        /// <returns>
        /// status Created if item is added successfully
        /// </returns>
        //public async Task<ServiceResponse> AddGroceryItem(AddItemDto groceryItemDto)
        //{
        //    ServiceResponse serviceResponse = new();


        //    GroceryItem groceryItem = new GroceryItem()
        //    {
        //        Name = groceryItemDto.Name,
        //        Quantity = groceryItemDto.Quantity,
        //        Price = groceryItemDto.UnitPrice
        //    };

        //    try
        //    {

        //        _context.GroceryItems.Add(groceryItem);
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {

        //        serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
        //        serviceResponse.Messages.Add("There was an error adding the grocery item.");
        //        serviceResponse.Messages.Add(ex.Message);
        //        return serviceResponse;
        //    }


        //    serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
        //    serviceResponse.CreatedId = groceryItem.ItemId; 
        //    return serviceResponse;
        //}
        //public async Task AddGroceryItem(AddItemDto groceryItemDto, int memberId)
        //{
        //    // Create the new grocery item
        //    var groceryItem = new GroceryItem
        //    {
        //        Name = groceryItemDto.Name,
        //        Quantity = groceryItemDto.Quantity,
        //        Price = groceryItemDto.UnitPrice
        //    };

        //    // Add the grocery item to the context
        //    _context.GroceryItems.Add(groceryItem);
        //    await _context.SaveChangesAsync(); // Save to get the ItemId

        //    // Create a new GroupPurchase to link the grocery item with the selected member
        //    var groupPurchase = new GroupPurchase
        //    {
        //        GroceryItemId = groceryItem.ItemId, // Link the group purchase to the grocery item
        //        Purchase = new Purchase
        //        {
        //            MemberId = memberId, // Link the group purchase to the selected member
        //            DatePurchased = DateOnly.FromDateTime(DateTime.Now)
        //        }
        //    };

        //    // Add the GroupPurchase to the context
        //    _context.GroupPurchases.Add(groupPurchase);
        //    await _context.SaveChangesAsync();
        //}

        public async Task<ServiceResponse> AddGroceryItem(AddItemDto groceryItemDto, int memberId)
        {
            var response = new ServiceResponse();

            try
            {
                // Create the new grocery item
                var groceryItem = new GroceryItem
                {
                    Name = groceryItemDto.Name,
                    Quantity = groceryItemDto.Quantity,
                    Price = groceryItemDto.UnitPrice
                };

                // Add the grocery item to the context
                _context.GroceryItems.Add(groceryItem);
                await _context.SaveChangesAsync(); // Save to get the ItemId

                // Create a new GroupPurchase to link the grocery item with the selected member
                var groupPurchase = new GroupPurchase
                {
                    GroceryItemId = groceryItem.ItemId, // Link the group purchase to the grocery item
                    Purchase = new Purchase
                    {
                        MemberId = memberId, // Link the group purchase to the selected member
                        DatePurchased = DateOnly.FromDateTime(DateTime.Now)
                    }
                };

                // Add the GroupPurchase to the context
                _context.GroupPurchases.Add(groupPurchase);
                await _context.SaveChangesAsync();

                response.Status = ServiceResponse.ServiceStatus.Created;
                response.CreatedId = groceryItem.ItemId; // Set the ID of the created item
            }
            catch (Exception ex)
            {
                response.Status = ServiceResponse.ServiceStatus.Error;
                response.Messages.Add($"Error: {ex.Message}");
            }

            return response;
        }


        /// <summary>
        /// Deletes a GroceryItem specified by its {id}.
        /// </summary>
        /// <param name="id">The id of the grocery item to delete</param>
        /// <returns>
        /// status deleted if deletion is successful
        /// </returns>
        public async Task<ServiceResponse> DeleteGroceryItem(int id)
        {
            ServiceResponse serviceResponse = new();

       
            var groceryItem = await _context.GroceryItems.FindAsync(id);
            if (groceryItem == null)
            {
                
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Grocery item cannot be deleted because it does not exist.");
                return serviceResponse;
            }

            try
            {
                
                _context.GroceryItems.Remove(groceryItem);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Error encountered while deleting the grocery item.");
                serviceResponse.Messages.Add(ex.Message); 
                return serviceResponse;
            }

          
            serviceResponse.Status = ServiceResponse.ServiceStatus.Deleted;
            return serviceResponse;
        }


        /// <summary>
        /// List of GroceryItem Names purchased by a member.
        /// </summary>
        /// <param name="id">The id of the member who purchased</param>
        /// <returns>
        /// set status to Success
        /// </returns>
        public async Task<ServiceResponse> GetGroceryItemsByMemberId(int memberId)
        {
            var response = new ServiceResponse();

            try
            {
                // Get the grocery items where GroupPurchases link to the memberId
                var groceryItems = await _context.GroceryItems
                    .Include(g => g.GroupPurchases)
                    .ThenInclude(gp => gp.Purchase)
                    .Where(g => g.GroupPurchases.Any(gp => gp.Purchase.MemberId == memberId))
                    .ToListAsync();

                // Extract just the item names
                var itemNames = groceryItems.Select(g => g.Name).ToList();

                // Set the response status and data
                response.Status = ServiceResponse.ServiceStatus.Success;
                response.Messages.AddRange(itemNames);
            }
            catch (Exception ex)
            {
                response.Status = ServiceResponse.ServiceStatus.Error;
                response.Messages.Add($"Error: {ex.Message}");
            }

            return response;
        }

    }
}
