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

Add-Title("Design and Architecture Specification")
$selection.Font.Italic = 1
$selection.TypeText("Project: RoadRunners Car Rental System")
$selection.TypeParagraph()
$selection.Font.Italic = 0

# 1. UML Class Diagram
Add-Heading("1. UML Class Diagram")
$selection.TypeText("The following PlantUML code describes the system entities, their relationships, and the controller layer. This code can be pasted into PlantText.com for visualization.")
$selection.TypeParagraph()
$selection.Font.Name = "Courier New"
$selection.Font.Size = 10
$uml = @"
@startuml
skinparam classAttributeIconSize 0

package "Core.Entities" {
    class User {
        +int Id
        +string Username
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
    }
    class UnavailabilityPeriod {
        +int Id
        +int CarId
        +DateTime StartDate
        +DateTime EndDate
        +string Reason
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
        +AddCar()
        +DeleteCar()
    }
}

User "1" -- "0..*" RentalBooking
Car "1" -- "0..*" RentalBooking
Car "1" -- "0..*" UnavailabilityPeriod
RentalBooking -- User : UserId
RentalBooking -- Car : CarId
UnavailabilityPeriod -- Car : CarId
CarController ..> ApplicationDbContext : uses
AdminController ..> ApplicationDbContext : uses
@enduml
"@
$selection.TypeText($uml)
$selection.TypeParagraph()
$selection.Font.Name = "Arial"
$selection.Font.Size = 12

# 2. Application Architecture Description
Add-Heading("2. Application Architecture Description")
$selection.TypeText("The application follows a 3-Tier Layered Architecture combined with the Model-View-Controller (MVC) design pattern.")
$selection.TypeParagraph()
$selection.TypeText("- Presentation Layer (UI): Built with ASP.NET Core MVC and Razor Pages. It handles user interaction and displays data using the custom RoadRunners visual identity.")
$selection.TypeParagraph()
$selection.TypeText("- Business Logic Layer: Contained within Controllers and Services. This layer implements the Tiered Pricing Algorithm and booking validation rules.")
$selection.TypeParagraph()
$selection.TypeText("- Data Access Layer (DAL): Uses Entity Framework Core as an ORM with a SQLite database. Implements soft-delete logic and audit trails for rental history.")
$selection.TypeParagraph()

# 3. User Interface Design
Add-Heading("3. User Interface Design")
$selection.TypeText("The UI is designed to be premium and professional, using a dark blue (#1F5082) and orange (#E46A28) color palette. Key features include:")
$selection.TypeParagraph()
$selection.TypeText("- Hero Section: A welcoming landing page with a clear Call to Action (CTA).")
$selection.TypeParagraph()
$selection.TypeText("- Interactive Calendar: A custom JavaScript-based horizontal calendar for selecting booking dates with real-time availability updates.")
$selection.TypeParagraph()

# 4. Description of Main Classes
Add-Heading("4. Description of Main Classes")
$classData = @(
    @("Class Name", "Role / Responsibility"),
    @("User", "Manages user identity, credentials, and role-based access control (Admin vs. Client)."),
    @("Car", "Represents the fleet assets. Tracks availability and maintains soft-delete status."),
    @("RentalBooking", "Records the transactional relationship between a user and a car for a specific duration."),
    @("UnavailabilityPeriod", "Manages fleet constraints like maintenance or admin-requested blocking."),
    @("CarController", "Handles the rental lifecycle: browsing, pricing calculations, and confirming bookings."),
    @("AdminController", "Provides management tools: fleet CRUD operations and the global rental log dashboard.")
)
Add-Table 7 2 $classData

# 5. Testing Plan
Add-Heading("5. Testing Plan")
$testData = @(
    @("Test Case", "Input Values", "Expected Result"),
    @("Tiered Pricing (4 days)", "Price: 100/day, Duration: 4 days", "Total: 395 (300 + 95)"),
    @("Tiered Pricing (11 days)", "Price: 100/day, Duration: 11 days", "Total: 920 (Cap at 40% discount hit)"),
    @("Booking Overlap", "Existing: May 1-5, New: May 4-6", "System rejects the new booking as 'Unavailable'"),
    @("Soft-Delete Visibility", "Car ID 1 marked 'IsDeleted = true'", "Car hidden from Index but visible in Admin Rental Log")
)
Add-Table 5 3 $testData

$outputPath = "c:\Personal\scoala\An3\semestrul2\Industrial Informatics\project\CarRental\docs\Design_and_Architecture_Specification.docx"
$doc.SaveAs([ref]$outputPath)
$doc.Close()
$word.Quit()

Write-Host "Design document created at: $outputPath"
