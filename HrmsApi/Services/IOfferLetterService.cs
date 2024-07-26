using HrmsApi.Models;

namespace HrmsApi.Services
{
    public interface IOfferLetterService
    {
        Task<OfferLetter> GenerateOfferLetterAsync(OfferLetterViewModel model);
    }
}