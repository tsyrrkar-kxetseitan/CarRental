# Activity Report: Weeks 11-12
**Project:** RoadRunners Car Rental Service  
**Team Name:** Road Runners  
**Period:** 2026-05-07 to 2026-05-21

---

## 1. Project Overview
During this period, the RoadRunners Car Rental platform received major feature enhancements across the entire stack. Key focuses were the implementation of a multi-car shopping cart with payment validation, an Account page with rental history, a complete email notification system (SMTP), an in-site notification infrastructure, an admin revenue dashboard, and expanded car specifications. A professional Privacy Policy page was also created.

---

## 2. Functionality Implementation Status

### Total Functionalities Implemented (100%):
*   **Account Page & Rental History:** Dedicated Account page with user data, balance display, and full rental history (repurposed from the former My Rentals page). Multi-car orders display each car entry individually.
*   **Shopping Cart / Payment Page:** Multi-car cart with booking summary table, account balance integration (use-balance checkbox), and a "Confirm Payment" flow with site-based popup confirmation.
*   **Payment Data Validation:** Full client-side and server-side validation — 16-digit card number, letters-only cardholder name, MM/YY expiry with past-date rejection, 3-digit CVV, with inline helper hints.
*   **Admin Revenue Dashboard:** Four revenue cards — Gross Revenue, Expenses (Refunds + €20 Welcome Bonuses), Net Revenue, and Active/Canceled booking statistics. Refund cap logic prevents total refunds from exceeding gross revenue.
*   **Admin Cancel Restrictions:** Admin can only cancel upcoming bookings (not active ones). Client can cancel active bookings with pro-rated refund for remaining days (capped at total paid).
*   **120% Admin Refund Policy:** All admin-initiated cancellations refund 120% of the booking value to the client's balance.
*   **Case-Insensitive Search:** All search features (Browse Cars, Welcome page) now use `.ToLower()` for case-insensitive matching across brand, model, location, and free-text fields.
*   **Car Specifications:** Six new data fields per car — Chassis type, Fabrication Year, Engine Type, Horsepower, Transmission, and Fuel Consumption — visible on Details page and editable in Admin Create/Edit forms.
*   **€20 Welcome Bonus:** New users receive €20 credited to their account balance on registration.
*   **Privacy Policy:** Professional, comprehensive Privacy Policy page with 10 sections covering data collection, GDPR rights, cookies, data retention, and contact information.
*   **Cancel Popup Modal:** Replaced browser-native confirm dialogs with styled site-based pop-up modals for all cancel actions.
*   **Car Details Link in Cart:** Car names in the cart/payment table are now clickable, linking to the respective car details page.
*   **Wikipedia Image Fix:** Fallback logic for BMW Series naming conventions (e.g., "Series 3" → "3_Series") to correctly load Wikipedia images.

### Partially Implemented Functionalities (Work in Progress):
*   **Email Notification System (90%):** Four professionally designed HTML email templates created (Welcome, Booking Confirmation, 3-Day Rental Reminder, Admin Cancellation). Gmail SMTP integration configured. Currently undergoing final testing and validation.
*   **In-Site Notification System (90%):** Notification bell with dropdown in navbar, unread badge counter, mark-as-read functionality. Backend complete with NotificationController and NotificationService. Frontend AJAX integration in place. Currently undergoing final testing.
*   **Background Reminder Service (90%):** Hosted background service running every 6 hours to automatically send email + in-site reminders for bookings starting in 3 days. Currently undergoing final testing.
*   **Seed Data Expansion (100%):** Database expanded from 10 to 50 unique cars across 5 Romanian cities, each with unique specifications, descriptions, license plates, and pricing.

---

## 3. Tester Activity (Bug Report)

**Statistics:**
*   **Bugs Identified:** 5
*   **Bugs Resolved:** 5
*   **Bugs Partially Resolved:** 0

| Bug Description | Solution Description | LOC Added/Rewritten | Time Spent | Review |
| :--- | :--- | :--- | :--- | :--- |
| "Confirm Payment" button non-functional; no booking created. | Fixed POST form submission handler and payment processing pipeline in `CarController.ConfirmPayment`. | 45 | 1.5h | Resolved |
| Cancel buttons non-functional in Rental History and Admin Rental Log. | Replaced browser-native `confirm()` dialogs with Bootstrap modal pop-ups; fixed POST form targets. | 30 | 1h | Resolved |
| BMW Series 3 and Series 5 Wikipedia images not loading. | Added fallback term logic in `GetWikiImage` to convert "Series X" to "X_Series" for Wikipedia API. | 15 | 30m | Resolved |
| Search results were case-sensitive (e.g. "audi" ≠ "Audi"). | Added `.ToLower()` to all search filter expressions in `CarController.Index`. | 12 | 20m | Resolved |
| File lock error (`CarRental.Web.exe` / PID conflict) preventing build. | Terminated orphaned `dotnet` process via `taskkill /F /PID`. | 0 | 5m | Resolved |

---

## 4. Activity Tables (Per Developer)

### **Team Leader — Mastan Andrei-Mircea**
| Activity | Result | Time Spent |
| :--- | :--- | :--- |
| Feature planning and requirement analysis (Account page, Cart, Email, Notifications, Revenue dashboard). | Defined complete feature specifications and acceptance criteria for 6 major enhancements across the full stack. | 5h |
| Defined the refund policy (120% admin, pro-rated client). | Established business rules: 120% admin refund, 100%/85% client tiers, pro-rated active cancellation with discount-aware cap. | |
| Coordinated multi-sprint implementation. Code review and integration testing. | Verified end-to-end flows (registration → booking → payment → cancel → refund). Approved all merge changes. | |

### **Frontend Developer**
| Activity | Number of Lines of Code Written | Time Spent | Review |
| :--- | :--- | :--- | :--- |
| Account page layout (MyAccount.cshtml), Cart/Payment form with validation hints, Admin Revenue Dashboard cards, Car Details specification table, Notification bell dropdown with AJAX, Register form with Email field, Admin Create/Edit forms with specification fields, Privacy Policy page (10 sections), Cancel modal pop-ups. | ~900 | 7h | Verified |

### **Backend Developer**
| Activity | Number of Lines of Code Written | Time Spent | Review |
| :--- | :--- | :--- | :--- |
| EmailService with 4 HTML templates (Gmail SMTP), NotificationService/Controller, BackgroundReminderService, payment validation (server-side regex), case-insensitive search, refund cap logic, booking confirmation emails, admin cancel email/notification pipeline. | ~1100 | 8h | Verified |

### **Data Developer**
| Activity | Number of Lines of Code Written | Time Spent | Review |
| :--- | :--- | :--- | :--- |
| Entity updates (Car: 6 spec fields, User: Email, RentalBooking: RefundAmount, Notification: new entity). ApplicationDbContext with 50-car seed data. Fresh EF Core migration. ViewModels updates (RegisterViewModel, CarFormViewModel, CartViewModel). DI registration in Program.cs. | ~600 | 5h | Verified |

---

## 5. Conclusion
The project has advanced significantly during this period, with the implementation of the full shopping cart and payment workflow, account management, administrative revenue tracking, and the foundation for a complete email/notification infrastructure. The platform now features 50 unique cars with detailed specifications, professional payment validation, and a comprehensive Privacy Policy. The email notification system and in-site notification bell are in final testing and represent the primary work in progress for the next sprint.
