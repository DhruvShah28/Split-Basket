using Microsoft.AspNetCore.Mvc;
using SplitBasket.Models;

namespace SplitBasket.Interfaces
{
    public interface IMemberService
    {
        Task<IEnumerable<MemberDto>> ListMembers();

        Task<MemberDto> FindMember(int id);

        Task<ServiceResponse> UpdateMember(int id, UpdMemberDto updatememberDto);

        Task<ServiceResponse> AddMember(AddMemberDto addmemberDto);

        Task<ServiceResponse> DeleteMember(int id);
    }
}



