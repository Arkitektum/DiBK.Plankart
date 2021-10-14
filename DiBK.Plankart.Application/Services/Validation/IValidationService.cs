using DiBK.Plankart.Application.Models.Validation;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DiBK.Plankart.Application.Services
{
    public interface IValidationService
    {
        Task<ValidationResult> ValidateAsync(IFormFile file);
    }
}
