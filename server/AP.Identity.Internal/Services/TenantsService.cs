using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using AP.Common.Data;
using AP.Common.Data.Identity.Entities;
using AP.Common.Models;
using AP.Common.Utilities.Extensions;
using AP.Common.Utilities.Helpers;
using AP.Identity.Internal.Models.Tenants;
using AP.Identity.Internal.Services.Contracts;
using static AP.Identity.Internal.Constants.ApiErrorMessages;

namespace AP.Identity.Internal.Services;

public class TenantsService(DataContext dbContext, IMapper mapper) : ITenantsService
{
    public async Task<ApiResult<List<TenantOutput>>> GetAllTenants()
    {
        var tenants = await dbContext.Tenants.Select(t => mapper.Map<TenantOutput>(t)).ToListAsync();

        return ApiResult<List<TenantOutput>>.SuccessWith(tenants);
    }

    public async Task<ApiResult<List<TenantOutput>>> GetTenants(int currentUserId)
    {
        // return the tenants for the currentUserId
        var tenants = await dbContext.UserTenants
            .Include(ut => ut.Tenant).ThenInclude(x => x!.TenantContacts)!.ThenInclude(x => x.Contact)
            .Where(ut => ut.UserId == currentUserId)
            .Select(ut => new TenantOutput
            {
                TenantId = ut.TenantId ?? default,
                TenantName = ut.Tenant!.TenantName,
                TenantBIC = ut.Tenant.TenantBIC,
                TenantType = ut.Tenant.TenantType,
                Ownership = ut.Tenant.Ownership,
                Domain = ut.Tenant.Domain,
                Summary = ut.Tenant.Summary,
                LogoUrl = ut.Tenant.LogoUrl,
                Active = ut.Tenant.Active,
                Enabled = ut.Tenant.Enabled,
                CreatedOn = ut.Tenant.CreatedOn,
                CreatedBy = ut.Tenant.CreatedBy,
                Contacts = ut.Tenant.TenantContacts != null
                    ? ut.Tenant.TenantContacts
                        .Where(tc => tc.Contact != null)
                        .Select(tc => new ContactOutput()
                        {
                            ContactId = tc.ContactId ?? default!,
                            ContactName = tc.Contact != null ? tc.Contact.ContactName : default!,
                            Email = tc.Contact != null ? tc.Contact.Email : null,
                            Phone = tc.Contact != null ? tc.Contact.Phone : null,
                            Title = tc.Contact != null ? tc.Contact.Title : null,
                            Address = tc.Contact != null ? tc.Contact.Address : null,
                            Active = tc.Active,
                            Primary = tc.Primary,
                            CreatedOn = tc.CreatedOn,
                        }).ToList()
                    : new List<ContactOutput>(),
            })
            .ToListAsync();

        return ApiResult<List<TenantOutput>>.SuccessWith(tenants);
    }

    public async Task<ApiResult<TenantsResponse>> GetTenants(int? page, int? size, string? name, string? sort)
    {
        var searchFilter = name != null ? name.Replace(" ", "") : string.Empty;

        var count = await dbContext.Tenants
            .CountAsync(tenant => name == null || tenant.TenantName.Contains(searchFilter));
        Pager.Calculate(count, page, size,/* out int? pageNum, out int? pageSize,*/ out int skipRows, out int takeRows);

        var tenants = dbContext.Tenants
            .Include(x => x.TenantContacts)!.ThenInclude(x => x.Contact)
            .Where(tenant => (name == null || (tenant.TenantName).Contains(searchFilter)))
            .OrderByDescending(x => x.TenantId)
            .Skip(skipRows)
            .Take(takeRows);

        var tenantsResponse = new TenantsResponse()
        {
            Count = count,
            Page = page,
            Size = size,
            Name = name,
            Sort = sort,
            Tenants = await tenants.Select(tenant => MapToDomainModel(tenant)).ToListAsync()
        };

        return ApiResult<TenantsResponse>.SuccessWith(tenantsResponse);
    }

    public async Task<ApiResult<TenantOutput>> GetTenant(int tenantId)
    {
        var tenant = await dbContext.Tenants
            .Include(x => x.TenantContacts)!.ThenInclude(x => x.Contact)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);

        if (tenant is null)
        {
            return ApiResult<TenantOutput>.Failure(TenantNotFound, tenantId);
        }

        return ApiResult<TenantOutput>.SuccessWith(MapToDomainModel(tenant));
    }

    public async Task<ApiResult<TenantOutput>> CreateTenant(TenantRequest model, int currentUserId)
    {
        var validation = await ValidateTenant(null, model, currentUserId);
        if (!validation.Succeeded)
        {
            validation.Errors?.Insert(0, CreateTenantError);
            return ApiResult<TenantOutput>.Failure(validation.Errors ?? default!);
        }

        var tenant = MapToEntityModel(model, currentUserId);

        dbContext.Tenants.Add(tenant);

        var contacts = new List<Contact>();
        var update = UpdateContactsInDbContext(tenant, model.Contacts, contacts);
        if (!update.Succeeded)
        {
            return ApiResult<TenantOutput>.Failure(update.Errors ?? default!);
        }

        int result = await dbContext.SaveChangesAsync();

        return result >= 1
            ? ApiResult<TenantOutput>.SuccessWith(MapToDomainModel(tenant), StatusCodes.Status201Created)
            : ApiResult<TenantOutput>.Failure(CreateTenantError);
    }

    public async Task<ApiResult<TenantOutput>> EditTenant(int tenantId, TenantRequest model, int currentUserId)
    {
        var tenant = await dbContext.Tenants
            .Include(x => x.TenantContacts)!.ThenInclude(x => x.Contact)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        if (tenant == null)
        {
            return ApiResult<TenantOutput>.Failure(TenantNotFound.WithMessageArgs(tenantId));
        }

        var validation = await ValidateTenant(tenantId, model, currentUserId);
        if (!validation.Succeeded)
        {
            validation.Errors?.Insert(0, EditTenantError.WithMessageArgs(tenantId));
            return ApiResult<TenantOutput>.Failure(validation.Errors ?? default!);
        }

        tenant.TenantName = model.TenantName;
        tenant.TenantBIC = model.TenantBIC;
        tenant.TenantType = model.TenantType;
        tenant.Ownership = model.Ownership;
        tenant.Domain = model.OptionalFields?.Domain;
        tenant.Summary = model.OptionalFields?.Summary;
        tenant.LogoUrl = model.OptionalFields?.LogoUrl;
        tenant.UpdatedOn = DateTime.UtcNow;
        tenant.UpdatedBy = currentUserId;

        var contacts = new List<Contact>();
        var update = UpdateContactsInDbContext(tenant, model.Contacts, contacts);
        if (!update.Succeeded)
        {
            return ApiResult<TenantOutput>.Failure(update.Errors ?? default!);
        }

        await dbContext.SaveChangesAsync();

        return ApiResult<TenantOutput>.SuccessWith(MapToDomainModel(tenant));
    }

    public async Task<ApiResult<TenantContactsResponse>> EditTenantContacts(int tenantId, TenantContactsRequest model)
    {
        var tenant = await dbContext.Tenants
            .Include(x => x.TenantContacts)!.ThenInclude(x => x.Contact)
            .FirstOrDefaultAsync(t => t.TenantId == tenantId);
        if (tenant == null)
        {
            return ApiResult<TenantContactsResponse>.Failure(TenantNotFound.WithMessageArgs(tenantId));
        }

        var contacts = new List<Contact>();
        var update = UpdateContactsInDbContext(tenant, model.Contacts, contacts);
        if (!update.Succeeded)
        {
            return ApiResult<TenantContactsResponse>.Failure(update.Errors ?? default!);
        }

        // save to db
        await dbContext.SaveChangesAsync();

        var response = new TenantContactsResponse()
        {
            TenantId = tenantId,
            Contacts = tenant.TenantContacts?
                .Where(tc => tc.Contact != null)
                .Select(tc => new ContactOutput()
                {
                    ContactId = tc.ContactId ?? default!,
                    ContactName = tc.Contact != null ? tc.Contact.ContactName : default!,
                    Email = tc.Contact?.Email,
                    Phone = tc.Contact?.Phone,
                    Title = tc.Contact?.Title,
                    Address = tc.Contact?.Address,
                    Active = tc.Active,
                    Primary = tc.Primary,
                    CreatedOn = tc.CreatedOn,
                }).ToList(),
        };

        return ApiResult<TenantContactsResponse>.SuccessWith(response);
    }

    public async Task<ApiResult<TenantOutput>> ActivateOrDeactivateTenant(int tenantId, bool active, int currentUserId)
    {
        var tenant = await dbContext.Tenants.FindAsync(tenantId);
        if (tenant == null)
        {
            return ApiResult<TenantOutput>.Failure(TenantNotFound.WithMessageArgs(tenantId));
        }

        tenant.Active = active;
        tenant.UpdatedOn = DateTime.UtcNow;
        tenant.UpdatedBy = currentUserId;
        await dbContext.SaveChangesAsync();

        return ApiResult<TenantOutput>.SuccessWith(MapToDomainModel(tenant));
    }

    public async Task<ApiResult<bool>> DeleteTenant(int tenantId, int currentUserId)
    {
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.TenantId == tenantId);
        if (tenant == null)
        {
            return ApiResult<bool>.Failure(TenantNotFound.WithMessageArgs(tenantId));
        }

        var tenantUsers = await dbContext.UserTenants
            .Where(ut => ut.TenantId == tenantId)
            .Include(ut => ut.Tenant)
            .ToListAsync();
        var errors = new List<ApiError>();
        if (tenantUsers != null && tenantUsers.Count != 0)
        {
            errors.Add(CannotDeleteTenantWithUsers.WithMessageArgs(tenant.TenantName, tenant.TenantId));
        }

        if (errors.Count != 0)
        {
            errors.Insert(0, CannotDeleteTenant.WithMessageArgs(tenant.TenantName, tenant.TenantId));
            return ApiResult<bool>.Failure(errors);
        }

        dbContext.Tenants.Remove(tenant);
        await dbContext.SaveChangesAsync();

        return ApiResult<bool>.SuccessWith(true, StatusCodes.Status204NoContent);
    }

    #region private methods

    private async Task<ApiResult> ValidateTenant(int? tenantId, TenantRequest model, int currentUserId)
    {
        model.TrimStringProperties<TenantRequest>();

        // validations
        var errors = new List<ApiError>();
        if (!await dbContext.Users.AnyAsync(u => u.UserId == currentUserId))
        {
            errors.Add(InvalidUser.WithMessageArgs(currentUserId));
        }
        if (await dbContext.Tenants.AnyAsync(x => x.TenantId != tenantId && x.TenantName == model.TenantName))
        {
            errors.Add(TenantNameAlreadyUsed.WithMessageArgs(model.TenantName));
        }
        if (await dbContext.Tenants.AnyAsync(x => x.TenantId != tenantId && x.TenantBIC == model.TenantBIC))
        {
            errors.Add(TenantBICAlreadyUsed.WithMessageArgs(model.TenantBIC));
        }
        if (await dbContext.TenantTypes.FindAsync(model.TenantType) == null)
        {
            errors.Add(InvalidTenantType.WithMessageArgs(model.TenantType));
        }
        if (await dbContext.TenantOwnerships.FindAsync(model.Ownership) == null)
        {
            errors.Add(InvalidTenantOwnership.WithMessageArgs(model.Ownership));
        }

        // return with errors
        if (errors.Count != 0)
        {
            return ApiResult.Failure(errors);
        }

        return ApiResult.Success;
    }

    private ApiResult UpdateContactsInDbContext(Tenant tenant, List<ContactInput>? modelContacts, List<Contact> contacts)
    {
        var errors = new List<ApiError>();
        modelContacts?.TrimStringProperties<List<ContactInput>>();

        modelContacts?.ForEach(el =>
        {
            // add new contact
            if ((el.ContactId ?? 0) == 0)
            {
                // validations
                if (contacts.Any(c => c.ContactName == el.ContactName))
                {
                    errors.Add(ContactNameDuplicated.WithMessageArgs(el.ContactName));
                }
                else if (!string.IsNullOrWhiteSpace(el.Email) && contacts.Any(c => c.Email == el.Email))
                {
                    errors.Add(ContactEmailDuplicated.WithMessageArgs(el.Email));
                }
                else
                {
                    // new contact
                    var contact = MapToEntityModel(el);
                    if (!string.IsNullOrWhiteSpace(el.Email) && dbContext.Contacts.Any(x => x.Email == el.Email))
                    {
                        // use existing in db table
                        contact = dbContext.Contacts.SingleOrDefault(x => x.Email == el.Email);
                        MapToEntityModel(el, contact!);
                    }
                    else if (!string.IsNullOrWhiteSpace(el.Phone) && dbContext.Contacts.Any(x => x.Phone == el.Phone))
                    {
                        // use existing in db table
                        contact = dbContext.Contacts.FirstOrDefault(x => x.Phone == el.Phone);
                        MapToEntityModel(el, contact!);
                    }
                    else
                    {
                        // add new contact
                        dbContext.Contacts.Add(contact!);
                    }

                    contacts.Add(contact ?? default!);

                    // add or update tenant-contact relation
                    var tenantContact = tenant.TenantContacts?
                        .FirstOrDefault(x => x.TenantId == tenant.TenantId && contact != null && x.ContactId == contact.ContactId);
                    if (tenantContact == null)
                    {
                        tenantContact = new TenantContact()
                        {
                            Tenant = tenant,
                            Contact = contact!,
                            Active = el.Active,
                            Primary = el.Primary,
                            CreatedOn = DateTime.UtcNow
                        };
                        // to avoid duplication
                        if (tenant.TenantContacts?.Any(e => e.TenantId == tenant.TenantId && e.ContactId == contact?.ContactId) is false)
                        {
                            tenant.TenantContacts?.Add(tenantContact);
                        }
                    }
                    else
                    {
                        tenantContact.Tenant = tenant;
                        tenantContact.Contact = contact!;
                        tenantContact.Active = el.Active;
                        tenantContact.Primary = el.Primary;
                    }
                }
            }
            // edit contact
            else
            {
                var contact = dbContext.Contacts.Find(el.ContactId);
                var tenantContact = tenant.TenantContacts?
                    .FirstOrDefault(x => x.TenantId == tenant.TenantId && contact != null && x.ContactId == el.ContactId);

                // validations
                if (contact == null)
                {
                    errors.Add(ContactNotFound.WithMessageArgs(el.ContactId ?? default));
                }
                else if (contacts.Any(c => c.ContactName == el.ContactName))
                {
                    errors.Add(ContactNameDuplicated.WithMessageArgs(el.ContactName));
                }
                else if (!string.IsNullOrWhiteSpace(el.Email) && contacts.Any(c => c.Email == el.Email))
                {
                    errors.Add(ContactEmailDuplicated.WithMessageArgs(el.Email));
                }
                else
                {
                    // update contact
                    if (!string.IsNullOrWhiteSpace(el.Email) && dbContext.Contacts.Any(x => x.ContactId != el.ContactId && x.Email == el.Email))
                    {
                        // get existing in db table
                        var existingContact = dbContext.Contacts.SingleOrDefault(x => x.ContactId != el.ContactId && x.Email == el.Email);

                        // remove this contact
                        if (!ContactUsedByAnotherTenant(tenant.TenantId, contact.ContactId))
                        {
                            dbContext.Contacts.Remove(contact);
                        }
                        if (tenantContact != null) tenant.TenantContacts?.Remove(tenantContact);

                        // use existing in db table
                        contact = existingContact ?? default!;
                        MapToEntityModel(el, contact!);
                    }
                    else if (!string.IsNullOrWhiteSpace(el.Phone) && dbContext.Contacts.Any(x => x.ContactId != el.ContactId && x.Phone == el.Phone))
                    {
                        // get existing in db table
                        var existingContact = dbContext.Contacts.SingleOrDefault(x => x.ContactId != el.ContactId && x.Phone == el.Phone);

                        // remove this contact
                        if (!ContactUsedByAnotherTenant(tenant.TenantId, contact.ContactId))
                        {
                            dbContext.Contacts.Remove(contact);
                        }
                        if (tenantContact != null) tenant.TenantContacts?.Remove(tenantContact);

                        // use existing in db table
                        contact = existingContact ?? default!;
                        MapToEntityModel(el, contact!);
                    }
                    else
                    {
                        MapToEntityModel(el, contact);
                    }

                    contacts.Add(contact ?? default!);

                    // add or update tenant-contact relation
                    tenantContact = tenant.TenantContacts?
                        .FirstOrDefault(x => x.TenantId == tenant.TenantId && contact != null && x.ContactId == contact.ContactId);
                    if (tenantContact == null)
                    {
                        tenantContact = new TenantContact()
                        {
                            Tenant = tenant,
                            Contact = contact!,
                            Active = el.Active,
                            Primary = el.Primary,
                            CreatedOn = DateTime.UtcNow
                        };
                        // to avoid duplication
                        if (tenant.TenantContacts?.Any(e => e.TenantId == tenant.TenantId && e.ContactId == contact?.ContactId) is false)
                        {
                            tenant.TenantContacts?.Add(tenantContact);
                        }
                    }
                    else
                    {
                        tenantContact.Tenant = tenant;
                        tenantContact.Contact = contact!;
                        tenantContact.Active = el.Active;
                        tenantContact.Primary = el.Primary;
                    }
                }
            }
        });

        // return with errors
        if (errors.Count != 0)
        {
            errors.Insert(0, TenantContactsError);
            return ApiResult.Failure(errors);
        }

        return ApiResult.Success;
    }


    private bool ContactUsedByAnotherTenant(int tenantId, int contactId)
    {
        // checked against the Tenants table to see if the contact is used by another tenant
        if (dbContext.Tenants.Any(t => t.TenantContacts!.Any(tc => tc.ContactId == contactId && tc.TenantId != tenantId)))
            return true;

        return false;
    }

    private static TenantOutput MapToDomainModel(Tenant tenant)
    {
        return new TenantOutput
        {
            TenantId = tenant.TenantId,
            TenantBIC = tenant.TenantBIC,
            TenantName = tenant.TenantName,
            TenantType = tenant.TenantType,
            Ownership = tenant.Ownership,
            Domain = tenant.Domain,
            Summary = tenant.Summary,
            LogoUrl = tenant.LogoUrl,
            Active = tenant.Active,
            Enabled = tenant.Enabled,
            CreatedOn = tenant.CreatedOn,
            CreatedBy = tenant.CreatedBy,
            UpdatedOn = tenant.UpdatedOn,
            UpdatedBy = tenant.UpdatedBy,
            Contacts = tenant.TenantContacts != null
                ? [.. tenant.TenantContacts
                    .Where(tc => tc.Contact != null)
                    .Select(tc => new ContactOutput()
                    {
                        ContactId = tc.ContactId ?? default!,
                        ContactName = tc.Contact != null ? tc.Contact.ContactName : default!,
                        Email = tc.Contact?.Email,
                        Phone = tc.Contact?.Phone,
                        Title = tc.Contact?.Title,
                        Address = tc.Contact?.Address,
                        Active = tc.Active,
                        Primary = tc.Primary,
                        CreatedOn = tc.CreatedOn,
                    })]
                : [],
        };
    }

    private static Tenant MapToEntityModel(TenantRequest model, int currentUserId)
    {
        return new Tenant
        {
            TenantName = model.TenantName,
            TenantBIC = model.TenantBIC,
            TenantType = model.TenantType,
            Ownership = model.Ownership,
            Domain = model.OptionalFields?.Domain,
            Summary = model.OptionalFields?.Summary,
            LogoUrl = model.OptionalFields?.LogoUrl,

            Active = true,
            Enabled = true,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = currentUserId,
        };
    }

    private static Contact MapToEntityModel(ContactInput model)
    {
        model.TrimStringProperties<ContactInput>();

        return new Contact
        {
            ContactName = model.ContactName,
            Email = model.Email,
            Phone = model.Phone,
            Title = model.Title,
            Address = model.Address,
            CreatedOn = DateTime.UtcNow,
        };
    }

    private static void MapToEntityModel(ContactInput model, Contact contact)
    {
        model.TrimStringProperties<ContactInput>();

        contact.ContactName = model.ContactName;
        contact.Email = model.Email;
        contact.Phone = model.Phone;
        contact.Title = model.Title;
        contact.Address = model.Address;
    }

    #endregion
}
