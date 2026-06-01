$word = New-Object -ComObject Word.Application
$word.Visible = $false
$doc = $word.Documents.Add()
$selection = $word.Selection

# Helper functions
function Add-Title($text) {
    $selection.Font.Size = 24
    $selection.Font.Bold = 1
    $selection.TypeText($text)
    $selection.TypeParagraph()
    $selection.Font.Size = 12
    $selection.Font.Bold = 0
}

function Add-Heading($text) {
    $selection.Font.Size = 16
    $selection.Font.Bold = 1
    $selection.TypeText($text)
    $selection.TypeParagraph()
    $selection.Font.Size = 12
    $selection.Font.Bold = 0
}

function Add-Table($rows, $cols, $data) {
    $table = $doc.Tables.Add($selection.Range, $rows, $cols)
    $table.Borders.Enable = $true
    for ($r = 1; $r -le $rows; $r++) {
        for ($c = 1; $c -le $cols; $c++) {
            $table.Cell($r, $c).Range.Text = $data[$r-1][$c-1]
            if ($r -eq 1) { $table.Cell($r, $c).Range.Font.Bold = 1 }
        }
    }
    $selection.Start = $table.Range.End
    $selection.TypeParagraph()
}

Add-Title("Extended Design and Architecture Specification")
$selection.Font.Italic = 1
$selection.TypeText("Project: RoadRunners Car Rental System | Version 1.1")
$selection.TypeParagraph()
$selection.Font.Italic = 0

# 1. UML Class Diagram with Precise Relationships
Add-Heading("1. UML Class Diagram")
$selection.TypeText("The updated PlantUML code below uses precise relationship notation (Composition, Aggregation, and Association).")
$selection.TypeParagraph()
$selection.Font.Name = "Courier New"
$selection.Font.Size = 9
$uml = @"
@startuml
skinparam classAttributeIconSize 0

package "Core.Entities" {
    class User {
        +int Id
        +string Username
        +string PasswordHash
        +string Role
        +List<RentalBooking> RentalBookings
    }
    class Car {
        +int Id
        +string Brand
        +string Model
        +decimal PricePerDay
        +bool IsDeleted
        +List<RentalBooking> RentalBookings
        +List<UnavailabilityPeriod> UnavailabilityPeriods
    }
    class RentalBooking {
        +int Id
        +int UserId
        +int CarId
        +DateTime StartDate
        +DateTime EndDate
        +decimal TotalPrice
        +string Status
        +DateTime CreatedAt
    }
    class UnavailabilityPeriod {
        +int Id
        +int CarId
        +DateTime StartDate
        +DateTime EndDate
        +string Reason
        +DateTime CreatedAt
    }
}

package "Data" {
    class ApplicationDbContext {
        +DbSet<User> Users
        +DbSet<Car> Cars
        +DbSet<RentalBooking> RentalBookings
        +DbSet<UnavailabilityPeriod> UnavailabilityPeriods
        #OnModelCreating()
    }
    
    class DatabaseSeeder {
        +SeedSampleData()
    }
}

package "Web.Controllers" {
    class CarController {
        +Index()
        +Details()
        +Rent()
        +CalculateTotalPrice()
    }
    class AdminController {
        +Index()
        +RentalLog()
        +DeleteCar()
    }
    class AccountController {
        +Login()
        +Register()
        +Logout()
    }
}

' Relationships
' User and Car are aggregated by the Context
ApplicationDbContext o-- User
ApplicationDbContext o-- Car
ApplicationDbContext o-- RentalBooking
ApplicationDbContext o-- UnavailabilityPeriod

' Car has a COMPOSITION relationship with UnavailabilityPeriod (Lifecycle dependent)
Car *-- UnavailabilityPeriod

' RentalBooking has an ASSOCIATION with User and Car
RentalBooking --> User : Associated User
RentalBooking --> Car : Associated Car

' Controllers depend on the Context
CarController ..> ApplicationDbContext : Dependency
AdminController ..> ApplicationDbContext : Dependency
AccountController ..> ApplicationDbContext : Dependency

@enduml
"@
$selection.TypeText($uml)
$selection.TypeParagraph()
$selection.Font.Name = "Arial"
$selection.Font.Size = 12

# 2. Application Architecture Description
Add-Heading("2. Application Architecture Description")
$selection.TypeText("Layered Structure Details:")
$selection.TypeParagraph()
$selection.TypeText("- UI (Presentation): Uses the MVC pattern. Views are strictly responsible for layout, while Controllers handle interaction logic.")
$selection.TypeParagraph()
$selection.TypeText("- Logic (Business): Encapsulated in the CarController (pricing engine) and AdminController (fleet constraints logic).")
$selection.TypeParagraph()
$selection.TypeText("- Data Access: Entity Framework Core with SQLite. Implements global query filters for soft-delete logic.")
$selection.TypeParagraph()

# 3. Comprehensive Testing Plan
Add-Heading("3. Extended Testing Plan")
$testData = @(
    @("ID", "Test Case Description", "Input / Scenario", "Expected Result"),
    @("TC1", "Client Registration", "Unique Username, Valid Password", "Account created, redirected to Login"),
    @("TC2", "Auth Failure", "Correct User, Wrong Password", "Error message: 'Invalid credentials'"),
    @("TC3", "Tiered Pricing (Standard)", "3 days @ €75/day", "Total: €225 (No discount)"),
    @("TC4", "Tiered Pricing (Tier 1)", "4 days @ €75/day", "Total: €296.25 (€225 + €71.25 [5% off])"),
    @("TC5", "Tiered Pricing (Cap)", "15 days @ €100/day", "Total: €1160 (Cap at 40% discount for days 11-15)"),
    @("TC6", "Overlap Check (Internal)", "Ex: May 5-10. New: May 6-8", "Rejection: Dates overlap with existing booking"),
    @("TC7", "Overlap Check (Edge)", "Ex: May 5-10. New: May 10-12", "Rejection: Last day of existing is first of new"),
    @("TC8", "Admin Maintenance Lock", "Car 1 locked May 20-22", "Car 1 disappears from search for those dates"),
    @("TC9", "Unavailability Merging", "Admin adds May 1-5 and May 4-10", "Calendar displays a single block: May 1-10"),
    @("TC10", "Soft-Delete Persistence", "Delete Car 1 (with past rentals)", "Car 1 hidden from fleet; History preserved in Rental Log"),
    @("TC11", "Future Booking Deletion", "Delete Car 2 (has booking tomorrow)", "Car 2 deleted; Client booking status set to 'CanceledByAdmin'"),
    @("TC12", "Log Filtering", "Admin selects 'Last Month'", "Table shows only rentals within the previous 30 days")
)
Add-Table 13 4 $testData

Add-Heading("4. Modeling & Design Considerations")
$selection.TypeText("- Performance: SQLite indexing on Username and Foreign Keys ensures fast lookup even as the rental log grows.")
$selection.TypeParagraph()
$selection.TypeText("- Scalability: The use of EF Core Migrations allows the schema to evolve easily as new car features are added.")
$selection.TypeParagraph()

$outputPath = "c:\Personal\scoala\An3\semestrul2\Industrial Informatics\project\CarRental\docs\Extended_Design_Specification_v1.1.docx"
$doc.SaveAs([ref]$outputPath)
$doc.Close()
$word.Quit()

Write-Host "Extended design document created at: $outputPath"
