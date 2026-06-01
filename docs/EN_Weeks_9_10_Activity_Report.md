# Activity Report: Weeks 9-10
**Project:** RoadRunners Car Rental Service  
**Team Name:** Road Runners  
**Period:** 2026-04-20 to 2026-05-07

---

## 1. Project Overview
The RoadRunners Car Rental platform has undergone a complete visual and functional transformation during this period. Key focuses were the implementation of a professional visual identity, a complex tiered pricing model, and a robust administrative rental log.

---

## 2. Functionality Implementation Status

### Total Functionalities Implemented (100%):
*   **Visual Identity Refresh:** Integration of brand palette (#1F5082, #1E4D7B, #3A6086, #C2D1DF, #E46A28), custom logo, and semi-transparent background.
*   **Tiered Pricing Algorithm:** Automatic price calculation: 100% for first 3 days, followed by a cumulative 5% daily discount (capped at 40%).
*   **User Authentication:** Secure Sign-up and Login system for Clients.
*   **Interactive Rental Calendar:** Role-based views (Admin: Color-coded users; Client: Simple availability) with overlapping unavailability merging logic.
*   **Administrative Rental Log:** Centralized dashboard for monitoring rentals across Week, Month, and Year periods.
*   **Soft-Delete Architecture:** Ability to remove cars from the active fleet while preserving all historical rental data for auditing.
*   **Unavailability Management:** Admin feature to block car availability for maintenance or other reasons.

### Partially Implemented Functionalities:
*   **Unit Testing Suite (85%):** 18 automated tests completed for core pricing and database logic. Future work includes UI automation tests.

---

## 3. Tester Activity (Bug Report)

**Statistics:**
*   **Bugs Identified:** 4
*   **Bugs Resolved:** 4
*   **Bugs Partially Resolved:** 0

| Bug Description | Solution Description | LOC Added/Rewritten | Time Spent | Review |
| :--- | :--- | :--- | :--- | :--- |
| `FOREIGN KEY` constraint failure on Car deletion. | Implemented "Soft-Delete" and forced unavailability checks. | 60 | 1.5h | Resolved |
| Logs showing default `01 Jan 0001` date for rentals. | Developed a `DatabaseSeeder` to refresh timestamps. | 40 | 45m | Resolved |
| Razor Syntax Error in `MyRentals.cshtml`. | Corrected variable declaration scope in the view. | 2 | 10m | Resolved |
| Build Error: `CarRental.Web.exe` locked. | Terminated orphaned background process via terminal. | 0 | 5m | Resolved |

---

## 4. Activity Tables (Per Developer)

### **Team Leader**
| Activity | Number of Lines of Code Written | Time Spent | Review |
| :--- | :--- | :--- | :--- |
| Project planning, definition of the Tiered Pricing algorithm, code reviews, and final integration of Frontend/Backend modules. | ~100 | 8h | Verified |

### **Frontend Developer**
| Activity | Number of Lines of Code Written | Time Spent | Review |
| :--- | :--- | :--- | :--- |
| Visual redesign (Palette/Logo/Background), Welcome page creation, and Interactive Calendar rendering logic. | ~750 | 5h | Verified |

### **Backend Developer**
| Activity | Number of Lines of Code Written | Time Spent | Review |
| :--- | :--- | :--- | :--- |
| Tiered Pricing logic, Sign-up feature, Log filtering system, and Unit Test implementation (18 tests). | ~800 | 6h | Verified |

### **Data Developer**
| Activity | Number of Lines of Code Written | Time Spent | Review |
| :--- | :--- | :--- | :--- |
| Database schema updates (Unavailability/Status), Soft-Delete global filters, and the runtime `DatabaseSeeder`. | ~350 | 4h | Verified |

---

## 5. Conclusion
The project is currently in a stable state. The transition to the RoadRunners visual identity is complete, and all core business logic—specifically the tiered pricing and rental log persistence—is fully functional and verified by automated tests.
