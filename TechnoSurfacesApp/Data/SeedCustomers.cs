using TechnoSurfacesApp.Models;

namespace TechnoSurfacesApp.Data;

/// <summary>
/// Seeded from a one-off Pastel Customer Masterfile export at cut-over.
/// RA Woodcraft and its account code RAW001 come from the client's own invoice
/// IN114317; the rest are representative of their stated customer mix, from a
/// single homeowner to a national franchise.
/// </summary>
public static class SeedCustomers
{
    public static void Load()
    {
        Db.Customers.AddRange(new[]
        {
            new Customer
            {
                Id = 1, CompanyName = "Bootleggers Coffee Company", AccountCode = "BOO001",
                VatNumber = "4120287731",
                BillingAddress = "12 Kloof Street, Gardens, Cape Town, 8001",
                Contacts =
                {
                    new Contact { Id = 1, CustomerId = 1, Name = "Nadine Roberts", JobTitle = "Operations Manager", Tel = "021 422 8810", Email = "nadine@bootleggers.co.za" },
                    new Contact { Id = 2, CustomerId = 1, Name = "Sipho Dlamini",  JobTitle = "Store Development", Tel = "021 422 8814", Email = "sipho@bootleggers.co.za" }
                }
            },
            new Customer
            {
                Id = 2, CompanyName = "Vida e Caff\u00E8", AccountCode = "VID001",
                VatNumber = "4550193882",
                BillingAddress = "Block C, Sunclare Building, 21 Dreyer Street, Claremont, 7708",
                Contacts =
                {
                    new Contact { Id = 3, CustomerId = 2, Name = "Chantel Fourie", JobTitle = "Projects Coordinator", Tel = "021 673 1000", Email = "chantel@vidaecaffe.com" },
                    new Contact { Id = 4, CustomerId = 2, Name = "Thabo Mahlangu", JobTitle = "Regional Build Manager", Tel = "021 673 1024", Email = "thabo@vidaecaffe.com" },
                    new Contact { Id = 5, CustomerId = 2, Name = "Ilse van Wyk",   JobTitle = "Procurement", Tel = "021 673 1031", Email = "ilse@vidaecaffe.com" }
                }
            },
            new Customer
            {
                Id = 3, CompanyName = "RA Woodcraft", AccountCode = "RAW001",
                VatNumber = "4690293412",
                BillingAddress = "Unit 5 Haryn Park, 13 Mocke Road, Diep River, 7800",
                Contacts =
                {
                    new Contact { Id = 6, CustomerId = 3, Name = "Ryan Abrahams", JobTitle = "Owner", Tel = "021 705 4432", Email = "ryan@rawoodcraft.co.za" }
                }
            },
            new Customer
            {
                Id = 4, CompanyName = "Airports Company South Africa", AccountCode = "ACS001",
                VatNumber = "4020159372",
                BillingAddress = "Cape Town International Airport, Matroosfontein, 7490",
                Contacts =
                {
                    new Contact { Id = 7, CustomerId = 4, Name = "Lindiwe Khumalo", JobTitle = "Facilities Projects", Tel = "021 937 1200", Email = "lindiwe.khumalo@airports.co.za" },
                    new Contact { Id = 8, CustomerId = 4, Name = "Gerrit Steyn",    JobTitle = "Retail Fit-out Lead", Tel = "021 937 1246", Email = "gerrit.steyn@airports.co.za" }
                }
            },
            new Customer
            {
                Id = 5, CompanyName = "Kitchen Studio Constantia", AccountCode = "KIT001",
                VatNumber = "4310288104",
                BillingAddress = "Shop 4, Constantia Village, Constantia, 7806",
                Contacts =
                {
                    new Contact { Id = 9,  CustomerId = 5, Name = "Amanda Pretorius", JobTitle = "Design Consultant", Tel = "021 794 6621", Email = "amanda@kitchenstudio.co.za" },
                    new Contact { Id = 10, CustomerId = 5, Name = "Riaan Botha",      JobTitle = "Installations", Tel = "021 794 6628", Email = "riaan@kitchenstudio.co.za" }
                }
            },
            new Customer
            {
                Id = 6, CompanyName = "M. Petersen (private)", AccountCode = "PET001",
                VatNumber = null,
                BillingAddress = "18 Buitengracht Close, Pinelands, 7405",
                Contacts =
                {
                    new Contact { Id = 11, CustomerId = 6, Name = "Michelle Petersen", JobTitle = null, Tel = "082 447 1902", Email = "m.petersen@gmail.com" }
                }
            }
        });
    }
}