using inventory_server.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Database;

public class AccountDbContext(DbContextOptions<AccountDbContext> options) : IdentityDbContext<Account>(options);