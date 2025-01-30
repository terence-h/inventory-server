using inventory_server.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Database;

public class AccountDbContext(DbContextOptions<AccountDbContext> options) : IdentityDbContext<Account>(options);