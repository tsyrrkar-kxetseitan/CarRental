using CarRental.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Data
{
    /// <summary>
    /// Entity Framework Core DbContext — the central point for DATA PERSISTENCE.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<RentalBooking> RentalBookings { get; set; }
        public DbSet<UnavailabilityPeriod> UnavailabilityPeriods { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique constraint on Username
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Unique constraint on LicensePlate
            modelBuilder.Entity<Car>()
                .HasIndex(c => c.LicensePlate)
                .IsUnique();

            // User -> RentalBooking (one-to-many)
            modelBuilder.Entity<RentalBooking>()
                .HasOne(rb => rb.User)
                .WithMany(u => u.RentalBookings)
                .HasForeignKey(rb => rb.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Car -> RentalBooking (one-to-many, SetNull on delete for soft-delete support)
            modelBuilder.Entity<RentalBooking>()
                .HasOne(rb => rb.Car)
                .WithMany(c => c.RentalBookings)
                .HasForeignKey(rb => rb.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            // Car -> UnavailabilityPeriod (one-to-many)
            modelBuilder.Entity<UnavailabilityPeriod>()
                .HasOne(up => up.Car)
                .WithMany(c => c.UnavailabilityPeriods)
                .HasForeignKey(up => up.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification -> User (many-to-one)
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Global query filter: exclude soft-deleted cars from default queries
            modelBuilder.Entity<Car>()
                .HasQueryFilter(c => !c.IsDeleted);

            // Seed Admin user (password: "admin123")
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9",
                    Role = "Admin",
                    AccountBalance = 0m,
                    Email = "admin@roadrunners.ro"
                },
                new User
                {
                    Id = 2,
                    Username = "client",
                    PasswordHash = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3",
                    Role = "Client",
                    AccountBalance = 20.00m,
                    Email = "client@roadrunners.ro"
                }
            );

            // Seed 50 cars across Romanian cities
            var cars = new[]
            {
                // București (B)
                new Car { Id = 1,  Brand = "BMW",      Model = "Series 3",   PricePerDay = 75.00m,  Seats = 5, Location = "București",   LicensePlate = "B01RRR",  Description = "Elegant sedan with premium comfort and sporty handling, ideal for business trips.", Chassis = "Sedan", FuelConsumption = "6.2 L/100km", FabricationYear = 2023, EngineType = "2.0L Diesel", Transmission = "Automatic", Horsepower = 190 },
                new Car { Id = 2,  Brand = "BMW",      Model = "Series 3",   PricePerDay = 75.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ01RRR", Description = "Sporty rear-wheel-drive sedan delivering pure driving pleasure on any road.", Chassis = "Sedan", FuelConsumption = "6.8 L/100km", FabricationYear = 2021, EngineType = "2.0L Petrol", Transmission = "Manual", Horsepower = 184 },
                new Car { Id = 3,  Brand = "BMW",      Model = "X5",         PricePerDay = 120.00m, Seats = 7, Location = "București",   LicensePlate = "B02RRR",  Description = "Premium SUV with spacious interior and powerful diesel engine for long journeys.", Chassis = "SUV", FuelConsumption = "9.5 L/100km", FabricationYear = 2024, EngineType = "3.0L Diesel", Transmission = "Automatic", Horsepower = 286 },
                new Car { Id = 4,  Brand = "BMW",      Model = "X5",         PricePerDay = 120.00m, Seats = 7, Location = "Timișoara",   LicensePlate = "TM01RRR", Description = "High-performance luxury SUV with xDrive AWD and dynamic sport suspension.", Chassis = "SUV", FuelConsumption = "10.1 L/100km", FabricationYear = 2022, EngineType = "3.0L Petrol", Transmission = "Automatic", Horsepower = 340 },
                new Car { Id = 5,  Brand = "BMW",      Model = "Series 5",   PricePerDay = 95.00m,  Seats = 5, Location = "Brașov",      LicensePlate = "BV01RRR", Description = "Executive sedan combining luxury, technology and efficient diesel performance.", Chassis = "Sedan", FuelConsumption = "7.0 L/100km", FabricationYear = 2023, EngineType = "2.0L Diesel", Transmission = "Automatic", Horsepower = 197 },
                new Car { Id = 6,  Brand = "Mercedes", Model = "C-Class",    PricePerDay = 85.00m,  Seats = 5, Location = "București",   LicensePlate = "B03RRR",  Description = "Luxury sedan with cutting-edge MBUX technology and refined interior design.", Chassis = "Sedan", FuelConsumption = "7.5 L/100km", FabricationYear = 2024, EngineType = "2.0L Diesel", Transmission = "Automatic", Horsepower = 204 },
                new Car { Id = 7,  Brand = "Mercedes", Model = "C-Class",    PricePerDay = 85.00m,  Seats = 5, Location = "Iași",        LicensePlate = "IS01RRR", Description = "Agile and engaging sedan with turbocharged petrol engine and manual gearbox.", Chassis = "Sedan", FuelConsumption = "7.8 L/100km", FabricationYear = 2022, EngineType = "1.5L Petrol Turbo", Transmission = "Manual", Horsepower = 170 },
                new Car { Id = 8,  Brand = "Mercedes", Model = "E-Class",    PricePerDay = 110.00m, Seats = 5, Location = "București",   LicensePlate = "B04RRR",  Description = "Flagship sedan offering supreme comfort, prestige and whisper-quiet cabin.", Chassis = "Sedan", FuelConsumption = "8.0 L/100km", FabricationYear = 2023, EngineType = "2.0L Diesel", Transmission = "Automatic", Horsepower = 220 },
                new Car { Id = 9,  Brand = "Mercedes", Model = "GLC",        PricePerDay = 105.00m, Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ02RRR", Description = "Versatile luxury SUV with turbocharged power and agile handling.", Chassis = "SUV", FuelConsumption = "8.5 L/100km", FabricationYear = 2024, EngineType = "2.0L Petrol Turbo", Transmission = "Automatic", Horsepower = 258 },
                new Car { Id = 10, Brand = "Mercedes", Model = "GLE",        PricePerDay = 130.00m, Seats = 7, Location = "Timișoara",   LicensePlate = "TM02RRR", Description = "Full-size luxury SUV with 7 seats, air suspension and all-terrain capability.", Chassis = "SUV", FuelConsumption = "9.8 L/100km", FabricationYear = 2023, EngineType = "3.0L Diesel", Transmission = "Automatic", Horsepower = 330 },
                new Car { Id = 11, Brand = "Audi",     Model = "A4",         PricePerDay = 70.00m,  Seats = 5, Location = "București",   LicensePlate = "B05RRR",  Description = "Quattro all-wheel drive with a sophisticated interior and responsive TFSI engine.", Chassis = "Sedan", FuelConsumption = "5.8 L/100km", FabricationYear = 2023, EngineType = "2.0L TFSI Petrol", Transmission = "Automatic", Horsepower = 204 },
                new Car { Id = 12, Brand = "Audi",     Model = "A4",         PricePerDay = 70.00m,  Seats = 5, Location = "Brașov",      LicensePlate = "BV02RRR", Description = "Efficient TDI diesel sedan with precise handling and excellent fuel economy.", Chassis = "Sedan", FuelConsumption = "6.1 L/100km", FabricationYear = 2021, EngineType = "2.0L TDI Diesel", Transmission = "Manual", Horsepower = 163 },
                new Car { Id = 13, Brand = "Audi",     Model = "A6",         PricePerDay = 90.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ03RRR", Description = "Business-class sedan with advanced Virtual Cockpit and smooth TDI power.", Chassis = "Sedan", FuelConsumption = "6.5 L/100km", FabricationYear = 2024, EngineType = "2.0L TDI Diesel", Transmission = "Automatic", Horsepower = 204 },
                new Car { Id = 14, Brand = "Audi",     Model = "Q7",         PricePerDay = 125.00m, Seats = 7, Location = "București",   LicensePlate = "B06RRR",  Description = "Large luxury SUV with three rows, quattro AWD and potent TFSI V6 engine.", Chassis = "SUV", FuelConsumption = "9.2 L/100km", FabricationYear = 2023, EngineType = "3.0L TFSI Petrol", Transmission = "Automatic", Horsepower = 340 },
                new Car { Id = 15, Brand = "Audi",     Model = "Q5",         PricePerDay = 100.00m, Seats = 5, Location = "Iași",        LicensePlate = "IS02RRR", Description = "Compact luxury SUV with sporty dynamics, refined comfort and efficient diesel.", Chassis = "SUV", FuelConsumption = "7.0 L/100km", FabricationYear = 2022, EngineType = "2.0L TDI Diesel", Transmission = "Automatic", Horsepower = 204 },
                new Car { Id = 16, Brand = "Toyota",   Model = "Corolla",    PricePerDay = 45.00m,  Seats = 5, Location = "București",   LicensePlate = "B07RRR",  Description = "Reliable hybrid sedan with outstanding fuel economy, perfect for city commutes.", Chassis = "Sedan", FuelConsumption = "4.5 L/100km", FabricationYear = 2024, EngineType = "1.8L Hybrid", Transmission = "Automatic", Horsepower = 140 },
                new Car { Id = 17, Brand = "Toyota",   Model = "Corolla",    PricePerDay = 45.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ04RRR", Description = "Dependable compact sedan with low running costs and engaging manual gearbox.", Chassis = "Sedan", FuelConsumption = "4.8 L/100km", FabricationYear = 2022, EngineType = "1.6L Petrol", Transmission = "Manual", Horsepower = 132 },
                new Car { Id = 18, Brand = "Toyota",   Model = "RAV4",       PricePerDay = 65.00m,  Seats = 5, Location = "Brașov",      LicensePlate = "BV03RRR", Description = "Popular crossover SUV with hybrid efficiency and spacious cargo area.", Chassis = "SUV", FuelConsumption = "5.5 L/100km", FabricationYear = 2023, EngineType = "2.5L Hybrid", Transmission = "Automatic", Horsepower = 222 },
                new Car { Id = 19, Brand = "Toyota",   Model = "RAV4",       PricePerDay = 65.00m,  Seats = 5, Location = "Timișoara",   LicensePlate = "TM03RRR", Description = "Rugged crossover with confident road manners and practical manual transmission.", Chassis = "SUV", FuelConsumption = "5.8 L/100km", FabricationYear = 2021, EngineType = "2.0L Petrol", Transmission = "Manual", Horsepower = 173 },
                new Car { Id = 20, Brand = "Toyota",   Model = "Land Cruiser", PricePerDay = 140.00m, Seats = 7, Location = "București", LicensePlate = "B08RRR",  Description = "Legendary off-road SUV with luxurious amenities, V6 diesel power and 7-seat capacity.", Chassis = "SUV", FuelConsumption = "10.5 L/100km", FabricationYear = 2024, EngineType = "3.5L V6 Diesel", Transmission = "Automatic", Horsepower = 305 },
                new Car { Id = 21, Brand = "Volkswagen", Model = "Golf",     PricePerDay = 50.00m,  Seats = 5, Location = "București",   LicensePlate = "B09RRR",  Description = "Iconic hatchback with German engineering, nimble handling and turbocharged TSI power.", Chassis = "Hatchback", FuelConsumption = "5.2 L/100km", FabricationYear = 2023, EngineType = "1.5L TSI Petrol", Transmission = "Manual", Horsepower = 150 },
                new Car { Id = 22, Brand = "Volkswagen", Model = "Golf",     PricePerDay = 50.00m,  Seats = 5, Location = "Iași",        LicensePlate = "IS03RRR", Description = "Compact and efficient hatchback with refined ride quality and intuitive infotainment.", Chassis = "Hatchback", FuelConsumption = "5.5 L/100km", FabricationYear = 2021, EngineType = "1.0L TSI Petrol", Transmission = "Manual", Horsepower = 110 },
                new Car { Id = 23, Brand = "Volkswagen", Model = "Passat",   PricePerDay = 65.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ05RRR", Description = "Spacious family sedan with premium features, digital cockpit and smooth TSI engine.", Chassis = "Sedan", FuelConsumption = "5.8 L/100km", FabricationYear = 2024, EngineType = "1.5L TSI Petrol", Transmission = "Automatic", Horsepower = 150 },
                new Car { Id = 24, Brand = "Volkswagen", Model = "Tiguan",   PricePerDay = 70.00m,  Seats = 5, Location = "Brașov",      LicensePlate = "BV04RRR", Description = "Compact SUV with versatile interior, confident road presence and powerful TSI engine.", Chassis = "SUV", FuelConsumption = "6.5 L/100km", FabricationYear = 2023, EngineType = "2.0L TSI Petrol", Transmission = "Automatic", Horsepower = 190 },
                new Car { Id = 25, Brand = "Volkswagen", Model = "Touareg",  PricePerDay = 115.00m, Seats = 5, Location = "Timișoara",   LicensePlate = "TM04RRR", Description = "Flagship SUV combining luxury with off-road prowess and mighty V6 TDI diesel.", Chassis = "SUV", FuelConsumption = "9.0 L/100km", FabricationYear = 2024, EngineType = "3.0L V6 TDI Diesel", Transmission = "Automatic", Horsepower = 286 },
                new Car { Id = 26, Brand = "Dacia",    Model = "Logan",      PricePerDay = 25.00m,  Seats = 5, Location = "București",   LicensePlate = "B10RRR",  Description = "Affordable and practical sedan with unbeatable value and low running costs.", Chassis = "Sedan", FuelConsumption = "5.0 L/100km", FabricationYear = 2024, EngineType = "1.0L SCe Petrol", Transmission = "Manual", Horsepower = 91 },
                new Car { Id = 27, Brand = "Dacia",    Model = "Logan",      PricePerDay = 25.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ06RRR", Description = "Budget-friendly sedan with generous boot space and peppy TCe turbo engine.", Chassis = "Sedan", FuelConsumption = "5.3 L/100km", FabricationYear = 2022, EngineType = "1.0L TCe Petrol", Transmission = "Manual", Horsepower = 100 },
                new Car { Id = 28, Brand = "Dacia",    Model = "Duster",     PricePerDay = 40.00m,  Seats = 5, Location = "București",   LicensePlate = "B11RRR",  Description = "Rugged and affordable SUV with diesel efficiency, perfect for Romanian mountain roads.", Chassis = "SUV", FuelConsumption = "5.8 L/100km", FabricationYear = 2024, EngineType = "1.5L Blue dCi Diesel", Transmission = "Manual", Horsepower = 115 },
                new Car { Id = 29, Brand = "Dacia",    Model = "Duster",     PricePerDay = 40.00m,  Seats = 5, Location = "Brașov",      LicensePlate = "BV05RRR", Description = "Versatile compact SUV with TCe petrol power and confident off-road stance.", Chassis = "SUV", FuelConsumption = "6.2 L/100km", FabricationYear = 2022, EngineType = "1.3L TCe Petrol", Transmission = "Manual", Horsepower = 131 },
                new Car { Id = 30, Brand = "Dacia",    Model = "Jogger",     PricePerDay = 35.00m,  Seats = 7, Location = "Iași",        LicensePlate = "IS04RRR", Description = "7-seat family MPV with incredible value, modern design and nimble city manners.", Chassis = "MPV", FuelConsumption = "5.5 L/100km", FabricationYear = 2024, EngineType = "1.0L TCe Petrol", Transmission = "Manual", Horsepower = 110 },
                new Car { Id = 31, Brand = "Ford",     Model = "Focus",      PricePerDay = 45.00m,  Seats = 5, Location = "București",   LicensePlate = "B12RRR",  Description = "Dynamic hatchback with sharp handling, EcoBoost efficiency and modern connectivity.", Chassis = "Hatchback", FuelConsumption = "5.0 L/100km", FabricationYear = 2023, EngineType = "1.0L EcoBoost Petrol", Transmission = "Manual", Horsepower = 125 },
                new Car { Id = 32, Brand = "Ford",     Model = "Focus",      PricePerDay = 45.00m,  Seats = 5, Location = "Timișoara",   LicensePlate = "TM05RRR", Description = "Refined hatchback with smooth automatic transmission and responsive EcoBoost turbo.", Chassis = "Hatchback", FuelConsumption = "5.3 L/100km", FabricationYear = 2021, EngineType = "1.5L EcoBoost Petrol", Transmission = "Automatic", Horsepower = 150 },
                new Car { Id = 33, Brand = "Ford",     Model = "Kuga",       PricePerDay = 60.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ07RRR", Description = "Stylish crossover with plug-in hybrid drivetrain and spacious boot.", Chassis = "SUV", FuelConsumption = "6.0 L/100km", FabricationYear = 2024, EngineType = "2.5L PHEV Hybrid", Transmission = "Automatic", Horsepower = 225 },
                new Car { Id = 34, Brand = "Ford",     Model = "Mustang",    PricePerDay = 150.00m, Seats = 2, Location = "București",   LicensePlate = "B13RRR",  Description = "Legendary American muscle car with thrilling V8 performance and head-turning presence.", Chassis = "Coupe", FuelConsumption = "12.5 L/100km", FabricationYear = 2023, EngineType = "5.0L V8 Petrol", Transmission = "Manual", Horsepower = 450 },
                new Car { Id = 35, Brand = "Hyundai",  Model = "Tucson",     PricePerDay = 55.00m,  Seats = 5, Location = "București",   LicensePlate = "B14RRR",  Description = "Modern SUV with striking parametric design, hybrid power and advanced safety.", Chassis = "SUV", FuelConsumption = "6.0 L/100km", FabricationYear = 2024, EngineType = "1.6L T-GDI Hybrid", Transmission = "Automatic", Horsepower = 230 },
                new Car { Id = 36, Brand = "Hyundai",  Model = "Tucson",     PricePerDay = 55.00m,  Seats = 5, Location = "Iași",        LicensePlate = "IS05RRR", Description = "Practical SUV with efficient diesel engine and generous cargo capacity.", Chassis = "SUV", FuelConsumption = "6.5 L/100km", FabricationYear = 2022, EngineType = "1.6L CRDi Diesel", Transmission = "Manual", Horsepower = 136 },
                new Car { Id = 37, Brand = "Hyundai",  Model = "i30",        PricePerDay = 40.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ08RRR", Description = "Well-rounded hatchback with European styling, 5-year warranty and peppy T-GDI engine.", Chassis = "Hatchback", FuelConsumption = "5.2 L/100km", FabricationYear = 2023, EngineType = "1.0L T-GDI Petrol", Transmission = "Manual", Horsepower = 120 },
                new Car { Id = 38, Brand = "Kia",      Model = "Sportage",   PricePerDay = 55.00m,  Seats = 5, Location = "Brașov",      LicensePlate = "BV06RRR", Description = "Bold crossover SUV with futuristic design, hybrid efficiency and panoramic display.", Chassis = "SUV", FuelConsumption = "6.2 L/100km", FabricationYear = 2024, EngineType = "1.6L T-GDI Hybrid", Transmission = "Automatic", Horsepower = 230 },
                new Car { Id = 39, Brand = "Kia",      Model = "Sportage",   PricePerDay = 55.00m,  Seats = 5, Location = "Timișoara",   LicensePlate = "TM06RRR", Description = "Economical diesel SUV with robust build quality and 7-year manufacturer warranty.", Chassis = "SUV", FuelConsumption = "6.8 L/100km", FabricationYear = 2022, EngineType = "1.6L CRDi Diesel", Transmission = "Manual", Horsepower = 136 },
                new Car { Id = 40, Brand = "Kia",      Model = "Ceed",       PricePerDay = 42.00m,  Seats = 5, Location = "București",   LicensePlate = "B15RRR",  Description = "Practical hatchback designed in Europe with spirited T-GDI engine and intuitive tech.", Chassis = "Hatchback", FuelConsumption = "5.0 L/100km", FabricationYear = 2023, EngineType = "1.0L T-GDI Petrol", Transmission = "Manual", Horsepower = 120 },
                new Car { Id = 41, Brand = "Skoda",    Model = "Octavia",    PricePerDay = 50.00m,  Seats = 5, Location = "București",   LicensePlate = "B16RRR",  Description = "Spacious sedan with German platform engineering, TSI petrol power and clever details.", Chassis = "Sedan", FuelConsumption = "4.8 L/100km", FabricationYear = 2024, EngineType = "1.5L TSI Petrol", Transmission = "Automatic", Horsepower = 150 },
                new Car { Id = 42, Brand = "Skoda",    Model = "Octavia",    PricePerDay = 50.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ09RRR", Description = "Efficient diesel sedan with class-leading boot space and comfortable long-distance ride.", Chassis = "Sedan", FuelConsumption = "5.2 L/100km", FabricationYear = 2022, EngineType = "2.0L TDI Diesel", Transmission = "Manual", Horsepower = 150 },
                new Car { Id = 43, Brand = "Skoda",    Model = "Superb",     PricePerDay = 65.00m,  Seats = 5, Location = "Brașov",      LicensePlate = "BV07RRR", Description = "Executive-class space and comfort at a competitive price, powered by a refined TSI engine.", Chassis = "Sedan", FuelConsumption = "5.5 L/100km", FabricationYear = 2024, EngineType = "2.0L TSI Petrol", Transmission = "Automatic", Horsepower = 190 },
                new Car { Id = 44, Brand = "Skoda",    Model = "Kodiaq",     PricePerDay = 75.00m,  Seats = 7, Location = "Timișoara",   LicensePlate = "TM07RRR", Description = "Large 7-seat family SUV with clever storage solutions and capable TDI diesel.", Chassis = "SUV", FuelConsumption = "7.5 L/100km", FabricationYear = 2023, EngineType = "2.0L TDI Diesel", Transmission = "Automatic", Horsepower = 200 },
                new Car { Id = 45, Brand = "Renault",  Model = "Clio",       PricePerDay = 30.00m,  Seats = 5, Location = "București",   LicensePlate = "B17RRR",  Description = "Stylish supermini with excellent fuel economy, nimble city handling and modern tech.", Chassis = "Hatchback", FuelConsumption = "4.2 L/100km", FabricationYear = 2024, EngineType = "1.0L SCe Petrol", Transmission = "Manual", Horsepower = 75 },
                new Car { Id = 46, Brand = "Renault",  Model = "Megane",     PricePerDay = 45.00m,  Seats = 5, Location = "Iași",        LicensePlate = "IS06RRR", Description = "French flair meets practicality with a punchy TCe turbo and smooth automatic gearbox.", Chassis = "Hatchback", FuelConsumption = "4.8 L/100km", FabricationYear = 2023, EngineType = "1.3L TCe Petrol", Transmission = "Automatic", Horsepower = 140 },
                new Car { Id = 47, Brand = "Porsche",  Model = "Cayenne",    PricePerDay = 180.00m, Seats = 5, Location = "București",   LicensePlate = "B18RRR",  Description = "High-performance luxury SUV with twin-turbo V8, unmistakable Porsche DNA and sports car agility.", Chassis = "SUV", FuelConsumption = "11.5 L/100km", FabricationYear = 2024, EngineType = "4.0L V8 Twin-Turbo", Transmission = "Automatic", Horsepower = 541 },
                new Car { Id = 48, Brand = "Porsche",  Model = "911",        PricePerDay = 250.00m, Seats = 2, Location = "București",   LicensePlate = "B19RRR",  Description = "Iconic sports car delivering pure driving exhilaration with a legendary flat-six engine.", Chassis = "Coupe", FuelConsumption = "10.0 L/100km", FabricationYear = 2024, EngineType = "3.0L Flat-6 Twin-Turbo", Transmission = "Automatic", Horsepower = 450 },
                new Car { Id = 49, Brand = "Volvo",    Model = "XC60",       PricePerDay = 95.00m,  Seats = 5, Location = "Cluj-Napoca", LicensePlate = "CJ10RRR", Description = "Scandinavian luxury SUV focused on safety, mild hybrid efficiency and understated elegance.", Chassis = "SUV", FuelConsumption = "7.0 L/100km", FabricationYear = 2024, EngineType = "2.0L B5 Mild Hybrid Diesel", Transmission = "Automatic", Horsepower = 235 },
                new Car { Id = 50, Brand = "Volvo",    Model = "XC90",       PricePerDay = 130.00m, Seats = 7, Location = "Brașov",      LicensePlate = "BV08RRR", Description = "Flagship 7-seat SUV with world-leading safety technology and refined mild hybrid petrol power.", Chassis = "SUV", FuelConsumption = "8.5 L/100km", FabricationYear = 2023, EngineType = "2.0L B5 Mild Hybrid Petrol", Transmission = "Automatic", Horsepower = 250 },
            };

            modelBuilder.Entity<Car>().HasData(cars);
        }
    }
}
