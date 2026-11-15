# Manual QA Test Plan & Bug Report – Stratford Event Booking System

## 1. Test Overview
- **System Under Test:** Stratford Event Booking System (Java, SQL, Web Technologies)
- **Test Types:** Functional Testing, Boundary Value Analysis, Double-Booking Checks, SQL Database Verification, UAT
- **Tools:** Browser DevTools, SQL Database Client, GitHub Issues

## 2. Functional & Edge-Case Test Cases

| Test ID | Module | Test Scenario | Input / Action | Expected Result | Status |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **TC-01** | Venue Reservation | Book available venue with valid future date | Select open venue + future date + valid details | Booking confirmed; confirmation message displayed; SQL status set to `CONFIRMED` | Pass |
| **TC-02** | Seat Availability | Prevent overbooking when 1 seat remains | Submit registration for the final available seat | Seat count updates to `0`; subsequent attempts display *"Event Full"* | Pass |
| **TC-03** | Date Validation | Attempt to book an event in the past | Select yesterday's date on booking form | Form blocks submission with validation error *"Event date must be in the future"* | Fail (Logged BUG-01) |
| **TC-04** | Admin Dashboard | Filter attendees by event name | Select specific event in Admin Dashboard filter | Only attendees registered for that event are displayed | Pass |
| **TC-05** | SQL Integrity | Cancel an active reservation | Click **Cancel Booking** on user dashboard | Booking status updates to `CANCELLED` in SQL; available seat count increments by `+1` | Pass |

## 3. Sample Defect Report

### [BUG-01] Booking form allows users to submit reservations for past dates
- **Severity:** Medium | **Priority:** High | **Browser:** Google Chrome
- **Steps to Reproduce:**
  1. Navigate to the **Venue Reservation** page.
  2. Select an available venue and manually type a past date (e.g., `2025-01-10`) into the date picker.
  3. Fill in valid attendee details and click **Confirm Booking**.
- **Expected Result:** System should reject past dates and display a validation message.
- **Actual Result:** System accepted the past date and generated a booking confirmation record.
- **Fix & Retest:** Added front-end `min` date attribute and Java backend date validation; retested and verified past dates are rejected.
