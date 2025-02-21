using Microsoft.AspNetCore.Mvc;
using SplitBasket.Models;

namespace SplitBasket.Interfaces
{
    public interface IGroceryItemService
    {
        Task<IEnumerable<GroceryItemDto>> ListGroceryItems();

        Task<GroceryItemDto> FindGroceryItem(int id);

        Task<ServiceResponse> UpdateGroceryItem(int id, UpdItemDto groceryItemDto);

        Task<ServiceResponse> AddGroceryItem(AddItemDto groceryItemDto, int memberId);

        Task<ServiceResponse> DeleteGroceryItem(int id);

        Task<ServiceResponse> GetGroceryItemsByMemberId(int memberId);
    }
}
