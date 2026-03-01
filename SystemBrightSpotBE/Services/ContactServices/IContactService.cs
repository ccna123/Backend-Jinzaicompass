using SystemBrightSpotBE.Dtos.Contact;

namespace SystemBrightSpotBE.Services.ContactServices
{
    public interface IContactService
    {
        Task Create(CreateContactDto request);
    }
}
