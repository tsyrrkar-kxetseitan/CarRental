using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CarRental.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Model = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PricePerDay = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    Seats = table.Column<int>(type: "INTEGER", nullable: false),
                    Location = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LicensePlate = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Chassis = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    FuelConsumption = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    FabricationYear = table.Column<int>(type: "INTEGER", nullable: true),
                    EngineType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Transmission = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Horsepower = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    AccountBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnavailabilityPeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnavailabilityPeriods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UnavailabilityPeriods_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    Type = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentalBookings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    CarId = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RentalBookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalBookings_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalBookings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Cars",
                columns: new[] { "Id", "Brand", "Chassis", "Description", "EngineType", "FabricationYear", "FuelConsumption", "Horsepower", "ImageUrl", "IsDeleted", "LicensePlate", "Location", "Model", "PricePerDay", "Seats", "Transmission" },
                values: new object[,]
                {
                    { 1, "BMW", "Sedan", "Elegant sedan with premium comfort and sporty handling, ideal for business trips.", "2.0L Diesel", 2023, "6.2 L/100km", 190, null, false, "B01RRR", "București", "Series 3", 75.00m, 5, "Automatic" },
                    { 2, "BMW", "Sedan", "Sporty rear-wheel-drive sedan delivering pure driving pleasure on any road.", "2.0L Petrol", 2021, "6.8 L/100km", 184, null, false, "CJ01RRR", "Cluj-Napoca", "Series 3", 75.00m, 5, "Manual" },
                    { 3, "BMW", "SUV", "Premium SUV with spacious interior and powerful diesel engine for long journeys.", "3.0L Diesel", 2024, "9.5 L/100km", 286, null, false, "B02RRR", "București", "X5", 120.00m, 7, "Automatic" },
                    { 4, "BMW", "SUV", "High-performance luxury SUV with xDrive AWD and dynamic sport suspension.", "3.0L Petrol", 2022, "10.1 L/100km", 340, null, false, "TM01RRR", "Timișoara", "X5", 120.00m, 7, "Automatic" },
                    { 5, "BMW", "Sedan", "Executive sedan combining luxury, technology and efficient diesel performance.", "2.0L Diesel", 2023, "7.0 L/100km", 197, null, false, "BV01RRR", "Brașov", "Series 5", 95.00m, 5, "Automatic" },
                    { 6, "Mercedes", "Sedan", "Luxury sedan with cutting-edge MBUX technology and refined interior design.", "2.0L Diesel", 2024, "7.5 L/100km", 204, null, false, "B03RRR", "București", "C-Class", 85.00m, 5, "Automatic" },
                    { 7, "Mercedes", "Sedan", "Agile and engaging sedan with turbocharged petrol engine and manual gearbox.", "1.5L Petrol Turbo", 2022, "7.8 L/100km", 170, null, false, "IS01RRR", "Iași", "C-Class", 85.00m, 5, "Manual" },
                    { 8, "Mercedes", "Sedan", "Flagship sedan offering supreme comfort, prestige and whisper-quiet cabin.", "2.0L Diesel", 2023, "8.0 L/100km", 220, null, false, "B04RRR", "București", "E-Class", 110.00m, 5, "Automatic" },
                    { 9, "Mercedes", "SUV", "Versatile luxury SUV with turbocharged power and agile handling.", "2.0L Petrol Turbo", 2024, "8.5 L/100km", 258, null, false, "CJ02RRR", "Cluj-Napoca", "GLC", 105.00m, 5, "Automatic" },
                    { 10, "Mercedes", "SUV", "Full-size luxury SUV with 7 seats, air suspension and all-terrain capability.", "3.0L Diesel", 2023, "9.8 L/100km", 330, null, false, "TM02RRR", "Timișoara", "GLE", 130.00m, 7, "Automatic" },
                    { 11, "Audi", "Sedan", "Quattro all-wheel drive with a sophisticated interior and responsive TFSI engine.", "2.0L TFSI Petrol", 2023, "5.8 L/100km", 204, null, false, "B05RRR", "București", "A4", 70.00m, 5, "Automatic" },
                    { 12, "Audi", "Sedan", "Efficient TDI diesel sedan with precise handling and excellent fuel economy.", "2.0L TDI Diesel", 2021, "6.1 L/100km", 163, null, false, "BV02RRR", "Brașov", "A4", 70.00m, 5, "Manual" },
                    { 13, "Audi", "Sedan", "Business-class sedan with advanced Virtual Cockpit and smooth TDI power.", "2.0L TDI Diesel", 2024, "6.5 L/100km", 204, null, false, "CJ03RRR", "Cluj-Napoca", "A6", 90.00m, 5, "Automatic" },
                    { 14, "Audi", "SUV", "Large luxury SUV with three rows, quattro AWD and potent TFSI V6 engine.", "3.0L TFSI Petrol", 2023, "9.2 L/100km", 340, null, false, "B06RRR", "București", "Q7", 125.00m, 7, "Automatic" },
                    { 15, "Audi", "SUV", "Compact luxury SUV with sporty dynamics, refined comfort and efficient diesel.", "2.0L TDI Diesel", 2022, "7.0 L/100km", 204, null, false, "IS02RRR", "Iași", "Q5", 100.00m, 5, "Automatic" },
                    { 16, "Toyota", "Sedan", "Reliable hybrid sedan with outstanding fuel economy, perfect for city commutes.", "1.8L Hybrid", 2024, "4.5 L/100km", 140, null, false, "B07RRR", "București", "Corolla", 45.00m, 5, "Automatic" },
                    { 17, "Toyota", "Sedan", "Dependable compact sedan with low running costs and engaging manual gearbox.", "1.6L Petrol", 2022, "4.8 L/100km", 132, null, false, "CJ04RRR", "Cluj-Napoca", "Corolla", 45.00m, 5, "Manual" },
                    { 18, "Toyota", "SUV", "Popular crossover SUV with hybrid efficiency and spacious cargo area.", "2.5L Hybrid", 2023, "5.5 L/100km", 222, null, false, "BV03RRR", "Brașov", "RAV4", 65.00m, 5, "Automatic" },
                    { 19, "Toyota", "SUV", "Rugged crossover with confident road manners and practical manual transmission.", "2.0L Petrol", 2021, "5.8 L/100km", 173, null, false, "TM03RRR", "Timișoara", "RAV4", 65.00m, 5, "Manual" },
                    { 20, "Toyota", "SUV", "Legendary off-road SUV with luxurious amenities, V6 diesel power and 7-seat capacity.", "3.5L V6 Diesel", 2024, "10.5 L/100km", 305, null, false, "B08RRR", "București", "Land Cruiser", 140.00m, 7, "Automatic" },
                    { 21, "Volkswagen", "Hatchback", "Iconic hatchback with German engineering, nimble handling and turbocharged TSI power.", "1.5L TSI Petrol", 2023, "5.2 L/100km", 150, null, false, "B09RRR", "București", "Golf", 50.00m, 5, "Manual" },
                    { 22, "Volkswagen", "Hatchback", "Compact and efficient hatchback with refined ride quality and intuitive infotainment.", "1.0L TSI Petrol", 2021, "5.5 L/100km", 110, null, false, "IS03RRR", "Iași", "Golf", 50.00m, 5, "Manual" },
                    { 23, "Volkswagen", "Sedan", "Spacious family sedan with premium features, digital cockpit and smooth TSI engine.", "1.5L TSI Petrol", 2024, "5.8 L/100km", 150, null, false, "CJ05RRR", "Cluj-Napoca", "Passat", 65.00m, 5, "Automatic" },
                    { 24, "Volkswagen", "SUV", "Compact SUV with versatile interior, confident road presence and powerful TSI engine.", "2.0L TSI Petrol", 2023, "6.5 L/100km", 190, null, false, "BV04RRR", "Brașov", "Tiguan", 70.00m, 5, "Automatic" },
                    { 25, "Volkswagen", "SUV", "Flagship SUV combining luxury with off-road prowess and mighty V6 TDI diesel.", "3.0L V6 TDI Diesel", 2024, "9.0 L/100km", 286, null, false, "TM04RRR", "Timișoara", "Touareg", 115.00m, 5, "Automatic" },
                    { 26, "Dacia", "Sedan", "Affordable and practical sedan with unbeatable value and low running costs.", "1.0L SCe Petrol", 2024, "5.0 L/100km", 91, null, false, "B10RRR", "București", "Logan", 25.00m, 5, "Manual" },
                    { 27, "Dacia", "Sedan", "Budget-friendly sedan with generous boot space and peppy TCe turbo engine.", "1.0L TCe Petrol", 2022, "5.3 L/100km", 100, null, false, "CJ06RRR", "Cluj-Napoca", "Logan", 25.00m, 5, "Manual" },
                    { 28, "Dacia", "SUV", "Rugged and affordable SUV with diesel efficiency, perfect for Romanian mountain roads.", "1.5L Blue dCi Diesel", 2024, "5.8 L/100km", 115, null, false, "B11RRR", "București", "Duster", 40.00m, 5, "Manual" },
                    { 29, "Dacia", "SUV", "Versatile compact SUV with TCe petrol power and confident off-road stance.", "1.3L TCe Petrol", 2022, "6.2 L/100km", 131, null, false, "BV05RRR", "Brașov", "Duster", 40.00m, 5, "Manual" },
                    { 30, "Dacia", "MPV", "7-seat family MPV with incredible value, modern design and nimble city manners.", "1.0L TCe Petrol", 2024, "5.5 L/100km", 110, null, false, "IS04RRR", "Iași", "Jogger", 35.00m, 7, "Manual" },
                    { 31, "Ford", "Hatchback", "Dynamic hatchback with sharp handling, EcoBoost efficiency and modern connectivity.", "1.0L EcoBoost Petrol", 2023, "5.0 L/100km", 125, null, false, "B12RRR", "București", "Focus", 45.00m, 5, "Manual" },
                    { 32, "Ford", "Hatchback", "Refined hatchback with smooth automatic transmission and responsive EcoBoost turbo.", "1.5L EcoBoost Petrol", 2021, "5.3 L/100km", 150, null, false, "TM05RRR", "Timișoara", "Focus", 45.00m, 5, "Automatic" },
                    { 33, "Ford", "SUV", "Stylish crossover with plug-in hybrid drivetrain and spacious boot.", "2.5L PHEV Hybrid", 2024, "6.0 L/100km", 225, null, false, "CJ07RRR", "Cluj-Napoca", "Kuga", 60.00m, 5, "Automatic" },
                    { 34, "Ford", "Coupe", "Legendary American muscle car with thrilling V8 performance and head-turning presence.", "5.0L V8 Petrol", 2023, "12.5 L/100km", 450, null, false, "B13RRR", "București", "Mustang", 150.00m, 2, "Manual" },
                    { 35, "Hyundai", "SUV", "Modern SUV with striking parametric design, hybrid power and advanced safety.", "1.6L T-GDI Hybrid", 2024, "6.0 L/100km", 230, null, false, "B14RRR", "București", "Tucson", 55.00m, 5, "Automatic" },
                    { 36, "Hyundai", "SUV", "Practical SUV with efficient diesel engine and generous cargo capacity.", "1.6L CRDi Diesel", 2022, "6.5 L/100km", 136, null, false, "IS05RRR", "Iași", "Tucson", 55.00m, 5, "Manual" },
                    { 37, "Hyundai", "Hatchback", "Well-rounded hatchback with European styling, 5-year warranty and peppy T-GDI engine.", "1.0L T-GDI Petrol", 2023, "5.2 L/100km", 120, null, false, "CJ08RRR", "Cluj-Napoca", "i30", 40.00m, 5, "Manual" },
                    { 38, "Kia", "SUV", "Bold crossover SUV with futuristic design, hybrid efficiency and panoramic display.", "1.6L T-GDI Hybrid", 2024, "6.2 L/100km", 230, null, false, "BV06RRR", "Brașov", "Sportage", 55.00m, 5, "Automatic" },
                    { 39, "Kia", "SUV", "Economical diesel SUV with robust build quality and 7-year manufacturer warranty.", "1.6L CRDi Diesel", 2022, "6.8 L/100km", 136, null, false, "TM06RRR", "Timișoara", "Sportage", 55.00m, 5, "Manual" },
                    { 40, "Kia", "Hatchback", "Practical hatchback designed in Europe with spirited T-GDI engine and intuitive tech.", "1.0L T-GDI Petrol", 2023, "5.0 L/100km", 120, null, false, "B15RRR", "București", "Ceed", 42.00m, 5, "Manual" },
                    { 41, "Skoda", "Sedan", "Spacious sedan with German platform engineering, TSI petrol power and clever details.", "1.5L TSI Petrol", 2024, "4.8 L/100km", 150, null, false, "B16RRR", "București", "Octavia", 50.00m, 5, "Automatic" },
                    { 42, "Skoda", "Sedan", "Efficient diesel sedan with class-leading boot space and comfortable long-distance ride.", "2.0L TDI Diesel", 2022, "5.2 L/100km", 150, null, false, "CJ09RRR", "Cluj-Napoca", "Octavia", 50.00m, 5, "Manual" },
                    { 43, "Skoda", "Sedan", "Executive-class space and comfort at a competitive price, powered by a refined TSI engine.", "2.0L TSI Petrol", 2024, "5.5 L/100km", 190, null, false, "BV07RRR", "Brașov", "Superb", 65.00m, 5, "Automatic" },
                    { 44, "Skoda", "SUV", "Large 7-seat family SUV with clever storage solutions and capable TDI diesel.", "2.0L TDI Diesel", 2023, "7.5 L/100km", 200, null, false, "TM07RRR", "Timișoara", "Kodiaq", 75.00m, 7, "Automatic" },
                    { 45, "Renault", "Hatchback", "Stylish supermini with excellent fuel economy, nimble city handling and modern tech.", "1.0L SCe Petrol", 2024, "4.2 L/100km", 75, null, false, "B17RRR", "București", "Clio", 30.00m, 5, "Manual" },
                    { 46, "Renault", "Hatchback", "French flair meets practicality with a punchy TCe turbo and smooth automatic gearbox.", "1.3L TCe Petrol", 2023, "4.8 L/100km", 140, null, false, "IS06RRR", "Iași", "Megane", 45.00m, 5, "Automatic" },
                    { 47, "Porsche", "SUV", "High-performance luxury SUV with twin-turbo V8, unmistakable Porsche DNA and sports car agility.", "4.0L V8 Twin-Turbo", 2024, "11.5 L/100km", 541, null, false, "B18RRR", "București", "Cayenne", 180.00m, 5, "Automatic" },
                    { 48, "Porsche", "Coupe", "Iconic sports car delivering pure driving exhilaration with a legendary flat-six engine.", "3.0L Flat-6 Twin-Turbo", 2024, "10.0 L/100km", 450, null, false, "B19RRR", "București", "911", 250.00m, 2, "Automatic" },
                    { 49, "Volvo", "SUV", "Scandinavian luxury SUV focused on safety, mild hybrid efficiency and understated elegance.", "2.0L B5 Mild Hybrid Diesel", 2024, "7.0 L/100km", 235, null, false, "CJ10RRR", "Cluj-Napoca", "XC60", 95.00m, 5, "Automatic" },
                    { 50, "Volvo", "SUV", "Flagship 7-seat SUV with world-leading safety technology and refined mild hybrid petrol power.", "2.0L B5 Mild Hybrid Petrol", 2023, "8.5 L/100km", 250, null, false, "BV08RRR", "Brașov", "XC90", 130.00m, 7, "Automatic" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AccountBalance", "Email", "PasswordHash", "Role", "Username" },
                values: new object[,]
                {
                    { 1, 0m, "admin@roadrunners.ro", "240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9", "Admin", "admin" },
                    { 2, 20.00m, "client@roadrunners.ro", "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3", "Client", "client" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cars_LicensePlate",
                table: "Cars",
                column: "LicensePlate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalBookings_CarId",
                table: "RentalBookings",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalBookings_UserId",
                table: "RentalBookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UnavailabilityPeriods_CarId",
                table: "UnavailabilityPeriods",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RentalBookings");

            migrationBuilder.DropTable(
                name: "UnavailabilityPeriods");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Cars");
        }
    }
}
