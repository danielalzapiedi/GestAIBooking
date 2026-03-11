using GestAI.Domain.Entities;
using GestAI.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GestAI.Infrastructure.Persistence;

public static class DbInitializer
{
    public sealed record SeedOptions(string AdminEmail, string AdminPassword, string PropertyName, string[] UnitNames);

    public static async Task MigrateAndSeedAsync(AppDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ILogger logger, SeedOptions options, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);
        if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
        var admin = await userManager.FindByEmailAsync(options.AdminEmail);
        if (admin is null)
        {
            admin = new User { UserName = options.AdminEmail, Email = options.AdminEmail, EmailConfirmed = true, Nombre = "Admin", Apellido = "GestAI" };
            var created = await userManager.CreateAsync(admin, options.AdminPassword);
            if (!created.Succeeded) throw new InvalidOperationException(string.Join("; ", created.Errors.Select(e => e.Description)));
            await userManager.AddToRoleAsync(admin, "Admin");
        }
        var account = await db.Accounts.FirstOrDefaultAsync(a => a.OwnerUserId == admin.Id, ct);
        if (account is null)
        {
            account = new Account { OwnerUserId = admin.Id, Name = "Mi cuenta", IsActive = true, CreatedAtUtc = DateTime.UtcNow };
            db.Accounts.Add(account); await db.SaveChangesAsync(ct);
        }
        var prop = await db.Properties.FirstOrDefaultAsync(p => p.AccountId == account.Id, ct);
        if (prop is null)
        {
            prop = new Property
            {
                AccountId = account.Id,
                Name = options.PropertyName,
                CommercialName = options.PropertyName,
                City = "Villa Rumipal",
                Province = "Córdoba",
                Country = "Argentina",
                Address = "Costanera s/n",
                Phone = "+54 9 3546 000000",
                Email = "reservas@demo.local",
                Currency = "ARS",
                DepositPolicy = "Se solicita seña para confirmar.",
                DefaultDepositPercentage = 30,
                DefaultCheckInTime = new TimeOnly(14, 0),
                DefaultCheckOutTime = new TimeOnly(10, 0),
                CancellationPolicy = "Cancelación flexible hasta 7 días antes.",
                TermsAndConditions = "No se permite fumar dentro de las unidades.",
                CheckInInstructions = "Presentarse con DNI en recepción.",
                PropertyRules = "Respetar horarios de descanso.",
                CommercialContactName = "Equipo GestAI Demo",
                CommercialContactPhone = "+54 9 3546 000000",
                CommercialContactEmail = "reservas@demo.local",
                PublicSlug = "demo-alma-de-lago",
                PublicDescription = "Complejo demo listo para reserva directa.",
                IsActive = true
            };
            db.Properties.Add(prop); await db.SaveChangesAsync(ct);
        }
        var existingUnits = await db.Units.Where(u => u.PropertyId == prop.Id).OrderBy(u => u.Id).ToListAsync(ct);
        if (existingUnits.Count == 0)
        {
            var i = 1;
            foreach (var name in options.UnitNames.Distinct())
            {
                db.Units.Add(new Unit { PropertyId = prop.Id, Name = name, CapacityAdults = 2, CapacityChildren = 2, TotalCapacity = 4, BaseRate = 85000 + (i * 5000), ShortDescription = $"Unidad demo {i}", DisplayOrder = i, IsActive = true, OperationalStatus = UnitOperationalStatus.Clean });
                i++;
            }
            await db.SaveChangesAsync(ct);
            existingUnits = await db.Units.Where(u => u.PropertyId == prop.Id).OrderBy(u => u.Id).ToListAsync(ct);
        }
        foreach (var unit in existingUnits)
        {
            if (!await db.RatePlans.AnyAsync(x => x.UnitId == unit.Id, ct))
            {
                var plan = new RatePlan { PropertyId = prop.Id, UnitId = unit.Id, Name = "Tarifa general", BaseNightlyRate = unit.BaseRate, WeekendAdjustmentEnabled = true, WeekendAdjustmentType = RateAdjustmentType.Percentage, WeekendAdjustmentValue = 12, IsActive = true };
                plan.SeasonalRates.Add(new SeasonalRate { Name = "Verano", StartMonth = 12, StartDay = 15, EndMonth = 2, EndDay = 28, AdjustmentType = RateAdjustmentType.Percentage, AdjustmentValue = 20, IsActive = true });
                plan.DateRangeRates.Add(new DateRangeRate { Name = "Feriado demo", DateFrom = DateOnly.FromDateTime(DateTime.Today.AddDays(15)), DateTo = DateOnly.FromDateTime(DateTime.Today.AddDays(18)), AdjustmentType = RateAdjustmentType.Fixed, AdjustmentValue = 10000, IsActive = true });
                db.RatePlans.Add(plan);
            }
        }
        if (!await db.Guests.AnyAsync(x => x.PropertyId == prop.Id, ct))
        {
            db.Guests.AddRange(
                new Guest { PropertyId = prop.Id, FullName = "Juan Pérez", Phone = "+54 9 351 111111", Email = "juan@example.com", IsActive = true },
                new Guest { PropertyId = prop.Id, FullName = "María Gómez", Phone = "+54 9 351 222222", Email = "maria@example.com", IsActive = true });
            await db.SaveChangesAsync(ct);
        }
        var firstGuest = await db.Guests.Where(x => x.PropertyId == prop.Id).OrderBy(x => x.Id).FirstAsync(ct);
        var secondGuest = await db.Guests.Where(x => x.PropertyId == prop.Id).OrderBy(x => x.Id).Skip(1).FirstAsync(ct);
        if (!await db.Bookings.AnyAsync(x => x.PropertyId == prop.Id, ct))
        {
            var unit1 = existingUnits[0];
            var unit2 = existingUnits[Math.Min(1, existingUnits.Count - 1)];
            db.Bookings.AddRange(
                new Booking { PropertyId = prop.Id, UnitId = unit1.Id, GuestId = firstGuest.Id, BookingCode = "RSV-DEMO-001", CheckInDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)), CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(5)), Adults = 2, Children = 1, Status = BookingStatus.Confirmed, Source = BookingSource.WhatsApp, TotalAmount = 285000, SuggestedNightlyRate = 95000, Notes = "Reserva demo", InternalNotes = "Prefiere vista al lago", GuestVisibleNotes = "Traer DNI", CreatedAt = DateTime.UtcNow.AddDays(-2), UpdatedAt = DateTime.UtcNow.AddDays(-1) },
                new Booking { PropertyId = prop.Id, UnitId = unit2.Id, GuestId = secondGuest.Id, BookingCode = "RSV-DEMO-002", CheckInDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)), CheckOutDate = DateOnly.FromDateTime(DateTime.Today.AddDays(10)), Adults = 2, Children = 0, Status = BookingStatus.Tentative, Source = BookingSource.Direct, TotalAmount = 270000, SuggestedNightlyRate = 90000, Notes = "Cotización avanzada", CreatedAt = DateTime.UtcNow.AddDays(-1), UpdatedAt = DateTime.UtcNow.AddDays(-1) });
            await db.SaveChangesAsync(ct);
        }
        var demoBooking = await db.Bookings.Where(x => x.PropertyId == prop.Id).OrderBy(x => x.Id).FirstAsync(ct);
        if (!await db.Payments.AnyAsync(x => x.BookingId == demoBooking.Id, ct))
        {
            db.Payments.Add(new Payment { PropertyId = prop.Id, BookingId = demoBooking.Id, Amount = 120000, Method = PaymentMethod.Transfer, Date = DateOnly.FromDateTime(DateTime.Today), Status = PaymentStatus.Paid, Notes = "Seña demo" });
        }
        if (!await db.MessageTemplates.AnyAsync(x => x.PropertyId == prop.Id, ct))
        {
            db.MessageTemplates.AddRange(
                new MessageTemplate { PropertyId = prop.Id, Type = TemplateType.Inquiry, Name = "Consulta base", Body = "Hola {GuestName}, gracias por consultar por {PropertyName}. Tenemos disponible {UnitName} del {CheckInDate} al {CheckOutDate}.", IsActive = true },
                new MessageTemplate { PropertyId = prop.Id, Type = TemplateType.Confirmation, Name = "Confirmación base", Body = "Hola {GuestName}, tu reserva en {PropertyName} quedó confirmada. Saldo pendiente: {BalanceDue}.", IsActive = true },
                new MessageTemplate { PropertyId = prop.Id, Type = TemplateType.PreCheckIn, Name = "Pre check-in", Body = "Te esperamos en {PropertyName} el {CheckInDate}. Unidad: {UnitName}.", IsActive = true },
                new MessageTemplate { PropertyId = prop.Id, Type = TemplateType.PostCheckOut, Name = "Post check-out", Body = "Gracias por alojarte en {PropertyName}, {GuestName}. ¡Te esperamos nuevamente!", IsActive = true });
        }
        await db.SaveChangesAsync(ct);
        if (!await db.SaasPlanDefinitions.AnyAsync(ct))
        {
            db.SaasPlanDefinitions.AddRange(
                new SaasPlanDefinition { Code = GestAI.Domain.Enums.SaasPlanCode.Starter, Name = "Starter", MaxProperties = 1, MaxUnits = 5, MaxUsers = 2, IncludesOperations = false, IncludesPublicPortal = false, IncludesReports = false },
                new SaasPlanDefinition { Code = GestAI.Domain.Enums.SaasPlanCode.Pro, Name = "Pro", MaxProperties = 3, MaxUnits = 20, MaxUsers = 5, IncludesOperations = true, IncludesPublicPortal = true, IncludesReports = true },
                new SaasPlanDefinition { Code = GestAI.Domain.Enums.SaasPlanCode.Manager, Name = "Manager", MaxProperties = 10, MaxUnits = 100, MaxUsers = 20, IncludesOperations = true, IncludesPublicPortal = true, IncludesReports = true });
            await db.SaveChangesAsync(ct);
        }
        if (!await db.AccountSubscriptionPlans.AnyAsync(x => x.AccountId == account.Id && x.IsActive, ct))
        {
            var proPlan = await db.SaasPlanDefinitions.FirstAsync(x => x.Code == GestAI.Domain.Enums.SaasPlanCode.Pro, ct);
            db.AccountSubscriptionPlans.Add(new AccountSubscriptionPlan { AccountId = account.Id, PlanDefinitionId = proPlan.Id, IsActive = true });
            await db.SaveChangesAsync(ct);
        }

        if (!await db.AccountUsers.AnyAsync(x => x.AccountId == account.Id && x.UserId == admin.Id, ct))
        {
            db.AccountUsers.Add(new AccountUser
            {
                AccountId = account.Id,
                UserId = admin.Id,
                Role = InternalUserRole.Owner,
                IsActive = true,
                CanManageBookings = true,
                CanManageGuests = true,
                CanManagePayments = true,
                CanViewReports = true,
                CanManageConfiguration = true,
                InvitedAtUtc = DateTime.UtcNow
            });
            await db.SaveChangesAsync(ct);
        }

        var adminTracked = await db.Users.FirstAsync(u => u.Id == admin.Id, ct);
        if (adminTracked.DefaultPropertyId is null)
        {
            adminTracked.DefaultPropertyId = prop.Id; adminTracked.DefaultAccountId = account.Id; await db.SaveChangesAsync(ct);
        }
        logger.LogInformation("DB migrate + seed OK. Admin: {Email}, PropertyId: {PropertyId}", options.AdminEmail, prop.Id);
    }
}
