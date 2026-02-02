using Microsoft.EntityFrameworkCore;

namespace AP.Common.Services.Contracts;

public interface IDbFunctions
{
    void CreateDbFunctions(DbContext db);
}