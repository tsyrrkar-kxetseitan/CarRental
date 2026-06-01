$word = New-Object -ComObject Word.Application
$word.Visible = $false
$doc = $word.Documents.Add()
$selection = $word.Selection

# Helper function to add a title
function Add-Title($text) {
    $selection.Font.Size = 24
    $selection.Font.Bold = 1
    $selection.TypeText($text)
    $selection.TypeParagraph()
    $selection.Font.Size = 12
    $selection.Font.Bold = 0
}

# Helper function to add a heading
function Add-Heading($text) {
    $selection.Font.Size = 16
    $selection.Font.Bold = 1
    $selection.TypeText($text)
    $selection.TypeParagraph()
    $selection.Font.Size = 12
    $selection.Font.Bold = 0
}

# Helper function to add a table
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

Add-Title("Activity Report: Weeks 9-10")
$selection.Font.Italic = 1
$selection.TypeText("Project: RoadRunners Car Rental Service | Team: Road Runners")
$selection.TypeParagraph()
$selection.Font.Italic = 0
$selection.TypeParagraph()

Add-Heading("1. Implementation Status")
$selection.TypeText("Total functionalities implemented (100%):")
$selection.TypeParagraph()
$selection.TypeText("- Visual Identity Refresh (Brand palette, Logo, Background)")
$selection.TypeParagraph()
$selection.TypeText("- Tiered Pricing Algorithm (5% cumulative daily discount)")
$selection.TypeParagraph()
$selection.TypeText("- User Authentication & Sign-up")
$selection.TypeParagraph()
$selection.TypeText("- Interactive Rental Calendar with Unavailability Merging")
$selection.TypeParagraph()
$selection.TypeText("- Administrative Rental Log (Week/Month/Year filters)")
$selection.TypeParagraph()
$selection.TypeText("- Soft-Delete Architecture for Log Persistence")
$selection.TypeParagraph()
$selection.TypeParagraph()

Add-Heading("2. Tester Activity (Bug Report)")
$selection.TypeText("Number of bugs identified: 4 | Resolved: 4 | Partially: 0")
$selection.TypeParagraph()

$bugData = @(
    @("Bug Description", "Solution Description", "LOC", "Time", "Review"),
    @("FOREIGN KEY failure on Car delete", "Implemented Soft-Delete & safety checks", "60", "1.5h", "Resolved"),
    @("Logs showing 01 Jan 0001 dates", "Developed DatabaseSeeder for timestamps", "40", "45m", "Resolved"),
    @("Razor Syntax Error in MyRentals", "Corrected variable scope in view", "2", "10m", "Resolved"),
    @("Build Error: CarRental.Web.exe locked", "Terminated background process", "0", "5m", "Resolved")
)
Add-Table 5 5 $bugData

Add-Heading("3. Team Leader Activity")
$tlData = @(
    @("Activity", "Lines of Code", "Time Spent", "Review"),
    @("Project planning, Pricing algorithm design, Code reviews, Integration", "~100", "8h", "Verified")
)
Add-Table 2 4 $tlData

Add-Heading("4. Developer Activities")

$selection.Font.Bold = 1
$selection.TypeText("Frontend Developer:")
$selection.TypeParagraph()
$selection.Font.Bold = 0
$feData = @(
    @("Activity", "Lines of Code", "Time Spent", "Review"),
    @("Visual redesign, Welcome page, Calendar rendering logic", "~750", "5h", "Verified")
)
Add-Table 2 4 $feData

$selection.Font.Bold = 1
$selection.TypeText("Backend Developer:")
$selection.TypeParagraph()
$selection.Font.Bold = 0
$beData = @(
    @("Activity", "Lines of Code", "Time Spent", "Review"),
    @("Pricing logic, Sign-up, Log filters, Unit Test project", "~800", "6h", "Verified")
)
Add-Table 2 4 $beData

$selection.Font.Bold = 1
$selection.TypeText("Data Developer:")
$selection.TypeParagraph()
$selection.Font.Bold = 0
$daData = @(
    @("Activity", "Lines of Code", "Time Spent", "Review"),
    @("Schema updates, Soft-Delete filters, DatabaseSeeder", "~350", "4h", "Verified")
)
Add-Table 2 4 $daData

$outputPath = "c:\Personal\scoala\An3\semestrul2\Industrial Informatics\project\CarRental\docs\EN_Weeks_9_10_Activity_Report_RoadRunners.docx"
$doc.SaveAs([ref]$outputPath)
$doc.Close()
$word.Quit()

Write-Host "Word document created at: $outputPath"
