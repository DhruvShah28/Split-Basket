using Microsoft.AspNetCore.Mvc;
using SplitBasket.Models;

namespace SplitBasket.Interfaces
{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseHistoryDto>> ListPurchases();

        Task<PurchaseHistoryDto> FindPurchase(int id);

        Task<ServiceResponse> UpdatePurchase(int id, UpdPurchaseDto aupPurchaseDto);

        Task<ServiceResponse> AddPurchase(AddPurchaseDto aupPurchaseDto);

        Task<ServiceResponse> DeletePurchase(int id);

    }
}
