using Microsoft.AspNetCore.Mvc;
using SplitBasket.Models;

namespace SplitBasket.Interfaces
{
    public interface IGroupPurchaseService
    {
        Task<IEnumerable<GroupPurchaseDto>> ListGroupPurchases();

        Task<GroupPurchaseDto> FindGroupPurchase(int id);

        Task<ServiceResponse> UpdateGroupPurchase(int id, GroupPurchaseDto groupPurchaseDto);

        Task<ServiceResponse> AddGroupPurchase(AddGroupPurchaseDto groupPurchaseDto);

        Task<ServiceResponse> DeleteGroupPurchase(int id);

        Task<IEnumerable<GroceryItemDto>> GetItemsWithIsBoughtFalse();

        Task<GroupPurchase> GetGroupPurchaseByPurchaseId(int purchaseId);

        Task AddPurchaseWithItems(AddPurchaseDto purchaseDto, List<int> itemIds);
    }
}
