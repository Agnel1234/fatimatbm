# Our Lady of Fatima Church — Parish Management System
## Web Redesign Plan: React + Serverless API + AWS RDS

**Document Version:** 1.0  
**Prepared for:** Fatima Church Parish, Tambaram, Chennai  
**Target Stack:** React (S3 + CloudFront) · AWS Lambda + API Gateway · AWS RDS (SQL Server) · AWS Cognito / JWT Session Management

---

## 1. Executive Summary

The existing system is a Windows Forms desktop application (C# / .NET 4.8) backed by a local SQL Server instance. It manages the complete parish lifecycle — anbiyam zones, families, family members, cemetery records, and subscription collections. The redesign moves this to a fully browser-based, cloud-hosted platform accessible from any device, without losing a single byte of existing business logic or data.

The approach:
- **Keep all existing SQL scripts as-is.** All 9 tables, ~50 stored procedures, and all indexes are reused directly on AWS RDS (SQL Server edition). Nothing in the database layer is rewritten.
- **Replace the WinForms UI with React.** Every form, grid, filter panel, and chart becomes a React component. Layout, feature parity, and role-based access are all preserved.
- **Replace the WinForms ↔ SQL connection with Lambda functions.** Each stored procedure call becomes one AWS Lambda function invoked over API Gateway. Lambda connects to RDS privately via VPC.
- **Replace WinForms dialog sessions with JWT-based web sessions.** Login authenticates against the existing `users` table (SHA-256 hash). On success, API Gateway issues a JWT stored in an HttpOnly cookie.

---

## 2. Current System — Complete Architecture Audit

### 2.1 Technology Stack

| Layer | Current | Target |
|---|---|---|
| UI | C# Windows Forms (.NET 4.8) | React 18 (TypeScript) |
| Charting | MS Chart Controls | Recharts / Chart.js |
| Map | IE WebBrowser + Leaflet.js | Leaflet.js React wrapper |
| Data Access | ADO.NET + Stored Procedures | AWS Lambda + mssql (node) |
| Database | SQL Server (local) | AWS RDS SQL Server |
| Authentication | SHA-256 hash against users table | Same hash check via Lambda → JWT |
| Deployment | Local machine install | S3 + CloudFront + API Gateway |
| Licensing | Syncfusion WinForms controls | Open-source React equivalents |

### 2.2 Database Schema — Complete Table Inventory

#### Table: `anbiyam`
Represents a church sub-group (zone unit). All families must belong to one.

| Column | Type | Notes |
|---|---|---|
| anbiyam_id | INT PK IDENTITY | |
| anbiyam_name | NVARCHAR(50) | Unique within zone |
| anbiyam_code | NVARCHAR(10) | 2-letter code, unique, auto-derived from name |
| anbiyam_zone | INT | 1–8 |
| anbiyam_coordinator_name | NVARCHAR(50) | |
| anbiyam_ass_coordinator_name | NVARCHAR(50) | Nullable |
| coordinator_email | NVARCHAR(100) | Nullable |
| coordinator_phone | NVARCHAR(15) | |
| created_at | DATETIME | |
| modified | DATE | |

**Business rule:** Cannot delete an anbiyam if any family is linked to it. Code must be unique — enforced in `sp_InsertOrUpdateAnbiyam`.

---

#### Table: `family`
Core entity. One row per registered parish family.

| Column | Type | Notes |
|---|---|---|
| family_id | INT PK IDENTITY | |
| anbiyam_id | INT FK | |
| family_code | NVARCHAR(10) | = anbiyam_code + sequential count, e.g. SJN5 |
| head_of_family | NVARCHAR(100) | Husband name if active, else wife name |
| gender | NVARCHAR(10) | Gender of head |
| family_permanant_address | NVARCHAR(200) | |
| family_temp_address | NVARCHAR(200) | |
| family_city / family_state / zip_code | various | Permanent address parts |
| family_temp_city / family_temp_state / family_temp_zipcode | various | Temp address parts |
| phone / email | various | Head's contact |
| monthly_subscription | INT | Monthly contribution amount in ₹ |
| parish_member_since | INT | Year of joining, auto-set to YEAR(GETDATE()) on insert |
| isactive | BIT | 1=active, 0=soft-deleted |
| ishusbandactive | BIT | Controls whether husband section is filled |
| iswifeactive | BIT | Controls whether wife section is filled |
| multiple_familycards | BIT | Family has more than one card |
| family_notes | NVARCHAR(200) | Free-text admin notes |
| last_subscription_date | DATETIME | Last time subscription was updated |
| disabled_date | DATETIME | Set when family is soft-deleted |
| created_at / modified | various | Audit timestamps |

**Business rules:**
- Family code = anbiyam_code + count-of-families-in-that-anbiyam + 1.
- Soft delete only: `isactive=0`, `disabled_date=GETDATE()`, `family_code='DELETED'`.
- At least one of husband or wife must be active as head.

---

#### Table: `family_member`
Individual persons within a family. Up to ~9 per family (1 head + 1 spouse + 5 children + 2 others).

| Column | Type | Notes |
|---|---|---|
| member_id | INT PK IDENTITY | |
| family_id | INT FK | |
| first_name | NVARCHAR(50) | Full name (last_name was dropped) |
| relationship | NVARCHAR(30) | Head / Spouse / Son / Daughter / Brother / Sister / Father / Mother / Father-In-Law / Mother-In-Law / Brother-In-Law / Sister-In-Law |
| gender | NVARCHAR(10) | Male / Female |
| dob | DATE | |
| member_status | NVARCHAR(20) | Active / Inactive / Deceased |
| occupation | NVARCHAR(50) | |
| qualification | NVARCHAR(50) | |
| blood_group | NVARCHAR(5) | |
| email / phone | various | |
| member_group | NVARCHAR(50) | Family / Child1..Child5 / Other1 / Other2 |
| baptized_date | DATETIME | |
| marriage_date | DATETIME | |
| first_communion_date | DATETIME | |
| first_confirmation_date | DATETIME | |
| priesthood_date | DATETIME | |
| child_class | NVARCHAR(50) | School class/standard (children) |
| child_institution | NVARCHAR(100) | School name |
| is_admin_council | BIT | Parish admin council member |
| is_legion_of_mary | BIT | |
| is_youth_group | BIT | |
| is_alter_services | BIT | Altar server |
| is_vencent_de_paul_soc | BIT | St. Vincent de Paul Society |
| is_choir | BIT | |
| is_catechism_student | BIT | |
| is_catechism_teacher | BIT | |
| is_women_assoc | BIT | Women's Association |
| is_litergy_council | BIT | Liturgy Council |

**Business rules:**
- Age computed at query time: `DATEDIFF(YEAR, dob, GETDATE()) - correction`.
- Deceased members show `-` for age; excluded from all counts and charts.
- Gender for "other relations" inferred from relationship type (Brother/Father/Brother-In-Law = Male; rest = Female).

---

#### Table: `family_subscription_yearly`
Monthly subscription tracking per family per year. One row per (family, year) pair.

| Column | Type | Notes |
|---|---|---|
| family_subscription_year_id | INT PK IDENTITY | |
| family_id | INT FK | Unique per year |
| subscription_year | INT | e.g. 2025 |
| jan_amount ... dec_amount | DECIMAL(10,2) | 12 month amount columns |
| jan_paid_date ... dec_paid_date | DATE | 12 payment date columns |
| jan_status ... dec_status | NVARCHAR(20) | 12 status columns: Paid / Pending / Overdue |
| total_amount | Computed | Sum of all month amounts (persisted) |
| created_at / modified | DATETIME | Audit timestamps |

**Business rules:**
- Upserted per month via `sp_SetFamilySubscriptionMonth` using dynamic SQL.
- Row is created on first month payment; subsequent months update the same row.

---

#### Table: `cemetery_details`
Deceased parish members buried in the church cemetery.

| Column | Type | Notes |
|---|---|---|
| cemetery_id | INT PK IDENTITY | |
| family_id | INT FK | |
| member_id | INT FK | Optional — links to specific family_member |
| deceased_name | NVARCHAR(100) | Copied from family_member.first_name on creation |
| date_of_birth | DATE | Nullable |
| date_of_death | DATE | Required |
| burial_date | DATE | |
| burial_place | NVARCHAR(200) | Defaults to 'Tambaram' |
| grave_number | NVARCHAR(50) | Cemetery code/plot number |
| remarks | NVARCHAR(255) | |

**Business rule:** When a cemetery record is created for a member, that member's `member_status` is automatically set to `'Deceased'`.

---

#### Table: `nonparishcemetery`
Outsiders (non-parishioners) buried in the church cemetery.

| Column | Type | Notes |
|---|---|---|
| CemeteryId | INT PK IDENTITY | |
| Name | NVARCHAR(100) | Deceased name |
| DOB | DATE | |
| Address / City / State / ZipCode | various | |
| ContactPerson | NVARCHAR(100) | Next of kin |
| ContactPhone | NVARCHAR(20) | |
| DeceasedDate | DATE | |
| BuriedDate | DATE | |
| gender | NVARCHAR(6) | |
| cemeterycode | NVARCHAR(6) | Plot/grave code |
| Remarks | NVARCHAR(500) | |

---

#### Table: `cemetery_subscription_yearly`
Annual cemetery maintenance fee per parish family.

| Column | Type | Notes |
|---|---|---|
| cemetery_subscription_year_id | INT PK IDENTITY | |
| family_id | INT FK | Unique per year |
| subscription_year | INT | |
| amount | DECIMAL(10,2) | |
| payment_date | DATE | |
| payment_status | NVARCHAR(20) | Paid / Pending / Overdue |
| remarks | NVARCHAR(255) | |

---

#### Table: `nonparish_cemetery_subscription`
Annual maintenance fee for non-parish cemetery entries (added in V4).

| Column | Type | Notes |
|---|---|---|
| subscription_id | INT PK IDENTITY | |
| nonparish_cemetery_id | INT FK → nonparishcemetery | |
| subscription_year | INT | Unique per cemetery entry per year |
| amount | DECIMAL(10,2) | |
| payment_date | DATE | |
| payment_status | NVARCHAR(20) | Paid / Pending / Overdue |
| remarks | NVARCHAR(500) | |

---

#### Table: `users`
Application login accounts.

| Column | Type | Notes |
|---|---|---|
| userid | INT PK IDENTITY | |
| username | NVARCHAR(100) | Unique |
| password | NVARCHAR(256) | SHA-256 hex string |
| userrole | NVARCHAR(50) | Admin / Guest |
| created_at | DATETIME | |

**Default users:** Admin (Admin@123) and Guest (Guest@123), both SHA-256 hashed at creation.

---

### 2.3 Complete Stored Procedure Inventory

Below are all ~50 stored procedures, grouped by domain, with their exact parameter signatures. Every one of these maps directly to a Lambda function endpoint.

#### Anbiyam Procedures

| Procedure | Parameters | Purpose |
|---|---|---|
| sp_GetAllAnbiyam | — | Dropdown list (code-name, id) |
| sp_GetAllAnbiyams | — | Full grid with member/family counts |
| sp_GetAnbiyamById | @anbiyam_id INT | Single record |
| sp_GetSelectedAnbiyam | @anbiyam_id INT | Edit form load (returns all fields) |
| sp_GetAnbiyamCodeById | @anbiyam_id INT | Returns just the code string |
| sp_InsertOrUpdateAnbiyam | @anbiyam_id, @anbiyam_name, @anbiyam_zone, @anbiyam_coordinator_name, @anbiyam_ass_coordinator_name, @coordinator_email, @coordinator_phone, @anbiyam_code | Create (id=0) or update |
| sp_DeleteAnbiyam | @anbiyamID | Hard delete, blocked if families exist |
| sp_GetTotalAnbiyamsCount | — | Count for code generation |
| sp_SearchFamilyWithAnbiyam | @anbiyam_id, @coordinator_name | Filtered anbiyam grid |
| sp_GetAnbiyamGridPaged | @pageNumber, @pageSize, @anbiyam_id, @coordinator_name | Paginated anbiyam grid (V4) |
| sp_GetAnbiyamTotalCount | @anbiyam_id, @coordinator_name | Count for pagination (V4) |

#### Family Procedures

| Procedure | Parameters | Purpose |
|---|---|---|
| sp_SaveFamily | @family_code, @anbiyam_id, @head_of_family, @gender, @family_permanant_address, @family_temp_address, @family_perm_city, @family_perm_state, @family_perm_zipcode, @family_temp_city, @family_temp_state, @family_temp_zipcode, @phone, @email, @monthly_subscription, @is_multiple_cards, @family_notes, @last_subscriptin_date, @family_id OUTPUT, @ishusbandactive, @iswifeactive | Upsert family (insert if id=0, update otherwise) |
| sp_GetFamilyById | @family_id | Single family record |
| sp_GetFamilyDetailsById | @family_id | Family details for edit form (includes head_of_family, isactive) |
| sp_GetFamiliesByAnbiyam | @anbiyam_id | All families in an anbiyam |
| sp_GetFamilyWithAnbiyam | — | Anbiyam grid with family/member counts |
| sp_GetFamilyBasicDetails | @anbiyam_id, @family_head, @occupation, @cemetery_available | TOP 30 family list (legacy) |
| sp_GetFamilyBasicDetailsPaged | @pageNumber, @pageSize, @anbiyam_id, @family_head, @occupation, @cemetery_available | Paginated family list (V4) |
| sp_GetFamilyTotalCount | @anbiyam_id, @family_head, @occupation, @cemetery_available | Count for family pagination (V4) |
| sp_GetFamilyBasicDetailsForExport | @anbiyam_id, @family_head, @occupation, @cemetery_available | All records for PDF export |
| sp_GetFamilyCountByZone | — | Dashboard chart: families by zone |
| sp_GetFamiliesByZone | — | Dashboard chart: families by zone (V4 version) |
| sp_GetTotalFamilyCount | @anbiyam_id | Count of families in anbiyam |
| sp_DeleteFamily | @familyID | Soft delete (sets isactive=0, code='DELETED') |
| sp_DisableFamily | @familyID | Alias soft-delete (sets isactive=0) |
| sp_GetFamilyByCode | @family_code | Look up family by code (V4) |
| sp_AggregateOccupations | — | Distinct occupation list for filter dropdown |

#### Family Member Procedures

| Procedure | Parameters | Purpose |
|---|---|---|
| sp_SaveFamilyMember | @member_id OUTPUT, @family_id, @first_name, @relationship, @gender, @dob, @member_status, @occupation, @qualification, @blood_group, @email, @phone, @baptized_date, @marriage_date, @first_communion_date, @confirmation_date, @priesthood_date, @isadmin, @legionofmary, @isyouth, @isalterservices, @isvencentdepaul, @iswomenassoc, @islitergycouncil, @ischoir, @iscatechismteacher, @iscatechismstudent, @childclass, @child_institution, @member_group | Upsert member |
| sp_GetFamilyMemberById | @member_id | Single member record |
| sp_GetFamilyMembersByFamilyId | @family_id | Members grid for a family (summary view with age computation) |
| sp_GetFamilyMembersDetailsByFamilyId | @family_id | Full member details for edit form |
| sp_GetFamilyMembersForDropdown | @family_id | Living members only (for cemetery assignment) |

#### Dashboard / Chart Procedures

| Procedure | Parameters | Purpose |
|---|---|---|
| sp_GetAgeGroupChartData | — | Age bands: Little Sprouts / Rising Teens / Young Achievers / Prime Movers / Wisdom Circle |
| sp_GenderData | — | Male/Female count (active members only) |
| sp_GetFamilyCountByZone | — | Zone-wise family count |

#### Cemetery Procedures

| Procedure | Parameters | Purpose |
|---|---|---|
| sp_InsertOrUpdateCemetery | @cemeteryID, @memberID, @burialDate, @deathDate, @cemeteryCode, @remarks | Upsert parish cemetery record; marks member as Deceased |
| sp_GetFamilyCemetery | @familyID | Cemetery records for a specific family |
| sp_GetAllCemeteries (legacy) | @burial_date_from, @burial_date_to, @deceased_date_from, @deceased_date_to, @IsOurparish | All cemeteries with optional filters |
| sp_GetAllCemeteriesPaged | @pageNumber, @pageSize, @burial_date_from, @burial_date_to, @deceased_date_from, @deceased_date_to, @IsOurparish | Paginated cemetery grid (V4) |
| sp_GetAllCemeteriesTotalCount | same filters | Count for cemetery pagination (V4) |
| sp_SaveNonParishCemetery | @Name, @DOB, @Address, @City, @State, @ZipCode, @Remarks, @ContactPerson, @ContactPhone, @DeceasedDate, @BuriedDate, @gender, @cemeterycode | Insert non-parish cemetery record |
| sp_GetNonParishCemeteryList | @name | List non-parish records (for subscription lookup) |

#### Subscription Procedures

| Procedure | Parameters | Purpose |
|---|---|---|
| sp_SetFamilySubscriptionMonth | @family_id, @subscription_year, @month, @amount, @paid_date, @status | Upsert one month's family subscription |
| sp_GetFamilySubscriptionYear | @family_id, @subscription_year | All 12 months for a family/year |
| sp_GetLastPaidSubscriptionDate | @family_id | Last paid month details |
| sp_SetCemeterySubscription | @family_id, @subscription_year, @amount, @payment_date, @payment_status | Upsert cemetery yearly subscription |
| sp_GetCemeterySubscription | @family_id, @subscription_year | Cemetery subscription record |
| sp_GetAllSubscriptions | @pageNumber, @pageSize | Paginated unified view (Family + Cemetery + Outside Parish) |
| sp_GetSubscriptionsFiltered | @pageNumber, @pageSize, @familyName, @anbiyamId, @subscriptionType, @status, @yearFrom, @yearTo | Filtered+paginated subscriptions |
| sp_GetSubscriptionTotalCount | same filters | Count for subscription pagination |
| sp_SetNonParishCemeterySubscription | @nonparish_cemetery_id, @subscription_year, @amount, @payment_date, @payment_status, @remarks | Upsert non-parish cemetery subscription |
| sp_GetNonParishCemeterySubscriptions | @nonparish_cemetery_id | All subscription years for one non-parish cemetery entry |
| sp_GetNonParishSubById | @subscription_id | Single non-parish subscription row |

#### Auth Procedures (inline SQL in WinForms, will become Lambda)

Authentication uses an inline SQL query (not a stored procedure):
```sql
SELECT COUNT(*) FROM dbo.[users] 
WHERE username = @username 
AND password = CONVERT(NVARCHAR(256), HASHBYTES('SHA2_256', @password), 2)
```
This will be moved into a dedicated Lambda `POST /auth/login`.

---

### 2.4 Complete Form / Screen Inventory

| WinForms Screen | Purpose | Triggered By |
|---|---|---|
| **LoginForm** | SHA-256 auth against users table. Two roles: Admin (full access) / Guest (read-only). | App startup |
| **Form1 — Families Tab** | Paginated family list (30/page), filterable by anbiyam, name, occupation, cemetery availability. Sub-grid shows selected family's members. Buttons: Create, Edit, Cemetery, Disable, Export PDF. | Nav bar |
| **Form1 — Anbiyam Tab** | Paginated grid of all anbiyams with coordinator, family/member/gender counts. Buttons: Create, Edit, Export PDF. | Nav bar |
| **Form1 — Cemetery Tab** | Paginated grid of cemetery records. Toggle between parish and non-parish. Date filters for burial date and deceased date. Button: Add Outside Parish. | Nav bar |
| **Form1 — Subscription Tab** | Paginated subscription list filterable by anbiyam, family name, type (Family/Cemetery/Outside Parish), status (Paid/Pending), year range. | Nav bar |
| **Form1 — Dashboard Tab** | Three charts: Age group bar chart, gender pie/donut chart, zone family count bar chart. Stat cards. | Nav bar |
| **FamilyPopup** | Create/Edit family. Full form with: general info (code, anbiyam, addresses, subscription amount, notes, last subscription date), husband section (name, DOB, phone, email, blood group, occupation, qualification, ministries, sacraments), wife section (same), up to 5 child sections, up to 2 other-relation sections. "Same address" checkbox auto-copies temp → permanent. Multiple cards checkbox. Disable button. | Create/Edit button on family tab |
| **AnbiyamPopup** | Create/Edit anbiyam. Fields: name, zone (1-8), code (auto-derived from first 2 chars of name), coordinator, assistant coordinator, email, phone. Duplicate code prevention. | Create/Edit on anbiyam tab |
| **Cemetery** | Per-family cemetery dialog. Shows existing cemetery records for the family. Allows registering a new record (select member from dropdown, enter death date, burial date, cemetery code, remarks) or updating an existing one (selecting from grid). | Cemetery button on family tab (family must be selected) |
| **NonParishFamily** | Register an outsider in the cemetery. Fields: name, gender, DOB, address, contact person/phone, deceased date, burial date, cemetery code, remarks. | "Outside Parish" button on cemetery tab |
| **SubscriptionPopup** | Per-family monthly subscription management. Year picker. 12 toggle buttons (Paid/Not Paid). Clicking a month button shows amount + paid-on date editor. Save writes one month at a time via sp_SetFamilySubscriptionMonth. | From family list or subscription tab |
| **CemeterySubscriptionPopup** | Per-family annual cemetery subscription. Fields: year, amount, paid-on date, status (Paid/Pending/Overdue). Upserts via sp_SetCemeterySubscription. | From cemetery or family grid |
| **NonParishCemeterySubscriptionPopup** | Annual subscription for a non-parish cemetery record. Same fields as CemeterySubscriptionPopup + remarks. Shows subscription history grid. | From non-parish cemetery grid |

---

## 3. Target Architecture

### 3.1 High-Level Architecture Diagram

```
Browser (React SPA)
       │
       │ HTTPS
       ▼
  CloudFront CDN
       │
  S3 Bucket (static assets)
       │
       │ API calls (HTTPS)
       ▼
  API Gateway (REST)
       │
  Lambda Functions (Node.js / Python)
       │
  VPC (private subnet)
       │
  RDS SQL Server (existing schema, migrated)
       │
  Secrets Manager (DB credentials)
```

**Auth flow:**
```
POST /auth/login
  → Lambda verifies SHA-256 hash against users table
  → Returns signed JWT (RS256, 8-hour expiry)
  → Frontend stores JWT in HttpOnly cookie (or memory + refresh token)
  → All subsequent API calls carry Authorization: Bearer <token>
  → API Gateway Lambda Authorizer validates token before routing
```

### 3.2 AWS Services

| Service | Role |
|---|---|
| S3 | Static React bundle hosting |
| CloudFront | CDN + HTTPS + cache |
| API Gateway | REST API endpoint, route all HTTP to Lambda |
| Lambda | Business logic, SP invocation |
| RDS (SQL Server Standard) | Existing database migrated as-is |
| VPC | Lambda + RDS in private subnet |
| Secrets Manager | RDS credentials |
| IAM | Lambda execution roles |
| ACM | SSL certificate for custom domain |
| Route 53 | DNS (optional) |

---

## 4. Frontend Design — React Application

### 4.1 Project Structure

```
src/
  api/           # All API call functions (axios wrappers)
  components/
    common/      # DataTable, Pagination, Modal, StatusBadge, FilterBar
    charts/      # AgeGroupChart, GenderChart, ZoneChart
    layout/      # AppShell, TopNav, Sidebar
  pages/
    Login/
    Dashboard/
    Anbiyam/
      AnbiyamList.tsx
      AnbiyamForm.tsx     # Create/Edit modal
    Families/
      FamilyList.tsx
      FamilyForm.tsx      # Full create/edit (replaces FamilyPopup)
      FamilyMembers.tsx   # Sub-grid
    Cemetery/
      CemeteryList.tsx
      CemeteryForm.tsx    # Parish cemetery record
      NonParishForm.tsx   # Outside-parish record
    Subscriptions/
      SubscriptionList.tsx
      FamilySubscriptionForm.tsx     # Monthly toggle form
      CemeterySubscriptionForm.tsx   # Annual form
      NonParishSubForm.tsx
  auth/          # useAuth hook, AuthContext, ProtectedRoute
  hooks/         # usePagination, useFilters, useDebounce
  types/         # TypeScript interfaces for all domain models
  utils/         # date formatting, validation helpers
```

### 4.2 Session Management

**Strategy:** JWT in `HttpOnly` `Secure` cookie (prevents XSS theft).

- On login success, Lambda sets `Set-Cookie: token=<JWT>; HttpOnly; Secure; SameSite=Strict; Max-Age=28800` (8 hours).
- React does NOT access the token directly. It just holds a `user` object in React Context (username + role) fetched via a `GET /auth/me` call on app load.
- On 401 response from any API, `AuthContext` clears state and redirects to `/login`.
- Role stored in JWT claims: `{ sub: "Admin", role: "Admin" }`. API Gateway Lambda Authorizer extracts role and sets it in request context. Role-restricted Lambda routes return 403 for Guests.
- Logout: `POST /auth/logout` clears the cookie.

**React Auth Context:**
```typescript
interface AuthContextType {
  user: { username: string; role: 'Admin' | 'Guest' } | null;
  login: (username: string, password: string) => Promise<void>;
  logout: () => void;
  isAdmin: () => boolean;
}
```

All mutating actions (Create, Edit, Disable, Save subscription) are wrapped in `isAdmin()` guards — identical to the Guest restriction in the existing WinForms app.

### 4.3 Navigation Structure

The existing tab-based navigation (`Families | Anbiyam | Cemetery | Subscriptions | Dashboard`) becomes a fixed left sidebar or top navigation bar with the same five sections. The church branding (Navy + Gold + Teal palette from AppTheme) is replicated in CSS variables / Tailwind config.

**Color palette (CSS variables):**
```css
--color-navy:    #1A2D42;
--color-teal:    #2C6E7A;
--color-gold:    #C9A84C;
--color-off-white: #F0F4F7;
--color-row-alt: #F0F7FA;
--color-paid-bg: #D4EDDA;  --color-paid-fg: #155724;
--color-pending-bg: #FFF3CD; --color-pending-fg: #856404;
--color-overdue-bg: #F8D7DA; --color-overdue-fg: #842029;
```

### 4.4 Screen-by-Screen React Implementation

---

#### 4.4.1 Login Page (`/login`)

**Component:** `LoginPage.tsx`

UI elements: Church cross icon, "Our Lady of Fatima Church — Tambaram, Chennai" heading, username + password inputs, Sign In button.

On submit:
```typescript
await api.post('/auth/login', { username, password });
// Sets HttpOnly cookie server-side
// Frontend fetches /auth/me to get user object
navigate('/dashboard');
```

Error: Show "Invalid username or password" inline (no page reload).

---

#### 4.4.2 Dashboard Page (`/dashboard`)

**Component:** `DashboardPage.tsx`  
**Data:** Three API calls on mount (parallel).

```typescript
const [ageData, genderData, zoneData] = await Promise.all([
  api.get('/dashboard/age-groups'),     // → sp_GetAgeGroupChartData
  api.get('/dashboard/gender'),          // → sp_GenderData
  api.get('/dashboard/zone-families'),   // → sp_GetFamiliesByZone
]);
```

**Charts (Recharts):**
- `AgeGroupChart`: Vertical bar chart. Bands: Little Sprouts (0-12), Rising Teens (13-18), Young Achievers (19-35), Prime Movers (36-60), Wisdom Circle (60+). Color palette from `ChartPalette`.
- `GenderChart`: Pie/donut chart. Male (Teal) vs Female (Gold).
- `ZoneFamilyChart`: Horizontal bar chart. Zone 1–8 vs family count.

**Stat cards** (4 tiles across top): Total Families, Total Members, Total Anbiyams, Active Subscriptions. These call `/dashboard/stats` which runs quick COUNT queries.

---

#### 4.4.3 Anbiyam List Page (`/anbiyam`)

**Component:** `AnbiyamList.tsx`

**State:** pageNumber (default 1), pageSize (50), filters (anbiyam_id, coordinator_name).

**API calls:**
```typescript
// On load and filter change:
GET /anbiyam?page=1&pageSize=50&coordinatorName=John
// → sp_GetAnbiyamGridPaged

GET /anbiyam/count?coordinatorName=John
// → sp_GetAnbiyamTotalCount
```

**Table columns:** Zone | Name | Coordinator | Mobile | Families | Members | Male | Female | Actions

**Actions (Admin only):** Edit button per row, Delete button per row.

**Header buttons:** "Create Anbiyam" (Admin only), "Export PDF".

**Filter bar:** Coordinator name text input + Search/Reset buttons.

---

#### 4.4.4 Anbiyam Create/Edit Modal (`AnbiyamForm.tsx`)

**Trigger:** Create button (id=0) or Edit button (id=selectedId).

**On open with id > 0:** `GET /anbiyam/:id` → `sp_GetSelectedAnbiyam`

**Fields:**
- Anbiyam Name (text, readonly on edit)
- Anbiyam Code (text, auto-populated as first 2 chars of name on blur, editable)
- Zone (select 1–8)
- Coordinator Name (required)
- Assistant Coordinator (optional)
- Email (optional)
- Phone (required, numeric only, 10 digits)

**On submit:** `POST /anbiyam` or `PUT /anbiyam/:id` → `sp_InsertOrUpdateAnbiyam`

**Validation:** Duplicate anbiyam code → SP raises error → display as form-level error.

---

#### 4.4.5 Family List Page (`/families`)

**Component:** `FamilyList.tsx`

**State:** pageNumber, pageSize (30, matching existing behavior), filters (anbiyam_id, familyHead, occupation, cemeteryAvailable).

**API calls:**
```typescript
GET /families?page=1&pageSize=30&anbiyamId=5&head=Jose&occupation=Teacher&cemetery=true
// → sp_GetFamilyBasicDetailsPaged

GET /families/count?same filters
// → sp_GetFamilyTotalCount
```

**Family list table columns:** Anbiyam | Code | Family Head | Mobile | Subscription (₹) | Member Since | Members | Cemeteries | Multiple Cards

**Member sub-table (appears below when a family row is clicked):**
```typescript
GET /families/:id/members
// → sp_GetFamilyMembersByFamilyId
```
Columns: Name | Relation | Gender | Age | Status | Occupation | Married? | AdminCou.. | AlterServ.. | Legion of Mary | Youth | VincentDe.. | Choir | Catechism Student | Catechism Teacher | Women's Assoc

**Action buttons:**
- Create Family (Admin only)
- Edit Family (Admin only, requires selection)
- Cemetery Info (requires selection)
- Disable Family (Admin only, requires selection, styled orange)
- Export PDF

**Filter bar:** Family Head Name (text) | Anbiyam Zone (dropdown) | Occupation (dropdown from sp_AggregateOccupations) | Cemetery (Yes/No/All dropdown) | Search | Reset

---

#### 4.4.6 Family Create/Edit Form (`/families/new`, `/families/:id/edit`)

This is the most complex form — it replaces `FamilyPopup.cs`. Implemented as a full page (not a modal) due to complexity.

**Sections (collapsible panels):**

**General Info:**
- Family Code (readonly, auto-generated: anbiyam_code + count+1)
- Anbiyam (dropdown → sp_GetAllAnbiyam)
- Zone (auto-filled from anbiyam selection, readonly)
- Permanent Address, City, State, ZIP
- Temporary Address, City, State, ZIP
- "Same as Temporary" checkbox (copies temp → perm)
- Monthly Subscription (₹, numeric)
- Multiple Cards checkbox
- Last Subscription Date (date picker)
- Family Notes (textarea)

**Husband (Head) Section** — toggle enabled/disabled with `isHusbandActive` checkbox:
- Name, DOB, Phone (10 digits), Email, Blood Group, Occupation, Qualification
- Baptism date, Marriage date
- Ministry checkboxes: Admin Council, Choir, Women's Assoc, Catechism Teacher, Liturgy Council, Legion of Mary, Vincent de Paul

**Wife Section** — toggle with `isWifeActive` checkbox (same fields as husband):
- Same fields as husband section

**Validation:** At least one of husband/wife must be enabled. If both enabled, spouse name must be filled.

**Children (up to 5):** Each child panel added with "+ Add Child" button. Per child:
- Name, Relationship (Son/Daughter dropdown), DOB, Phone, Blood Group, Occupation, Qualification
- School Standard, School Institution (for children still studying)
- Baptism, Marriage, First Communion, Confirmation, Priesthood dates
- Ministry checkboxes: Admin Council, Choir, LOM, Vincent de Paul, Altar Services, Youth, Catechism Teacher, Catechism Student
- Remove button

**Other Relations (up to 2):** Added with "+ Add Other Relation". Per relation:
- Name, Relationship (dropdown: Brother/Sister/Father/Mother/Father-In-Law/Mother-In-Law/Brother-In-Law/Sister-In-Law)
- DOB, Phone, Blood Group, Occupation, Qualification
- Baptism, Marriage, First Communion, Confirmation dates
- Ministry checkboxes: Admin Council, Choir, LOM, Vincent de Paul, Altar Services, Youth, Catechism Teacher

**On submit:**
```typescript
// Single API call — Lambda wraps in a DB transaction:
POST /families          // create
PUT  /families/:id      // update

// Lambda internally calls:
// 1. sp_SaveFamily (upsert) → gets family_id
// 2. sp_SaveFamilyMember for each member (upsert loop)
// If any step fails → transaction rollback
```

---

#### 4.4.7 Cemetery List Page (`/cemetery`)

**Component:** `CemeteryList.tsx`

**Toggle:** Parish Cemetery (default) / Outside Parish

**Filters:** Burial date from/to, Deceased date from/to, Reset.

**API calls:**
```typescript
GET /cemetery?page=1&pageSize=50&isParish=true&burialFrom=2024-01-01&burialTo=2024-12-31
// → sp_GetAllCemeteriesPaged

GET /cemetery/count?same filters
// → sp_GetAllCemeteriesTotalCount
```

**Table columns (Parish):** Cemetery Code | Deceased Name | Deceased Date | Burial Date | Remarks | Contact Person | Contact Mobile  
**Table columns (Outside Parish):** Cemetery Code | Deceased Name | Deceased Date | Burial Date | Remarks | Contact Person | Contact Mobile

**Action buttons:** "Add Outside Parish Member" (Admin only) opens NonParishForm.

**Row click (Parish):** Opens CemeterySubscriptionForm for that family.  
**Row click (Outside Parish):** Opens NonParishSubForm.

---

#### 4.4.8 Parish Cemetery Form (`CemeteryForm.tsx`)

Opened from the Family tab "Cemetery" button. Shows existing cemetery records for that family in a table. Allows adding a new record or editing an existing one.

**Add new record:**
- Member dropdown (sp_GetFamilyMembersForDropdown — living members only)
- Deceased Date (date picker)
- Burial Date (date picker)
- Cemetery Code / Grave Number
- Remarks

**On submit:** `POST /cemetery/parish` → `sp_InsertOrUpdateCemetery`

---

#### 4.4.9 Outside Parish Cemetery Form (`NonParishForm.tsx`)

Opened from the Cemetery tab "Add Outside Parish" button.

**Fields:** Name, Gender, DOB, Address, City, State, ZIP, Contact Person, Contact Phone, Deceased Date, Buried Date, Cemetery Code, Remarks.

**On submit:** `POST /cemetery/nonparish` → `sp_SaveNonParishCemetery`

---

#### 4.4.10 Subscription List Page (`/subscriptions`)

**Component:** `SubscriptionList.tsx`

**Filters:** Family Name (text) | Anbiyam (dropdown) | Type (Family/Cemetery/Outside Parish Cemetery/All) | Status (Paid/Pending/All) | Year From | Year To | Search | Reset

**API calls:**
```typescript
GET /subscriptions?page=1&pageSize=50&familyName=John&anbiyamId=3&type=Family&status=Paid&yearFrom=2024&yearTo=2025
// → sp_GetSubscriptionsFiltered

GET /subscriptions/count?same filters
// → sp_GetSubscriptionTotalCount
```

**Table columns:** Family Code | Head Name | Anbiyam | Amount (₹) | Status (colored badge) | Type | Year | Date Paid

**Row click:** Opens the appropriate subscription form based on Type.

---

#### 4.4.11 Family Monthly Subscription Form (`FamilySubscriptionForm.tsx`)

Replaces `SubscriptionPopup.cs`. Opened per family for a selected year.

**Layout:**
- Family name/code header
- Year picker (NumericInput, 2023–2100)
- 12 month toggle buttons in a 4×3 grid. Each button shows month name + status (PAID / NOT PAID / DUE).
- Clicking a month button expands an inline editor below the grid: Amount (₹), Paid On (date), Save button.

**On load:** `GET /subscriptions/family/:familyId/year/:year` → `sp_GetFamilySubscriptionYear`  
**On save:** `POST /subscriptions/family/:familyId/month` body: `{ year, month, amount, paidDate, status }` → `sp_SetFamilySubscriptionMonth`

Month button colors:
- Active/Paid: Teal (matches AppTheme PaidBg)
- Due (amount set, not marked paid): Khaki (PendingBg)
- Empty: Light gray

---

#### 4.4.12 Cemetery Subscription Form (`CemeterySubscriptionForm.tsx`)

Replaces `CemeterySubscriptionPopup.cs`.

**Fields:** Family info (header), Year (spinner), Amount (₹), Paid On (date), Status (Paid/Pending/Overdue dropdown).

**On load:** `GET /subscriptions/cemetery/family/:familyId/year/:year` → `sp_GetCemeterySubscription`  
**On save:** `POST /subscriptions/cemetery/family/:familyId` → `sp_SetCemeterySubscription`

---

#### 4.4.13 Non-Parish Cemetery Subscription Form (`NonParishSubForm.tsx`)

Replaces `NonParishCemeterySubscriptionPopup.cs`.

**Same fields as CemeterySubscriptionForm + Remarks textarea + Subscription History table below.**

**On load:** `GET /subscriptions/nonparish/:cemeteryId` → `sp_GetNonParishCemeterySubscriptions`  
**On save:** `POST /subscriptions/nonparish/:cemeteryId` → `sp_SetNonParishCemeterySubscription`

---

## 5. Backend Design — Serverless API Layer

### 5.1 Lambda Architecture Pattern

Each Lambda function follows the same structure:

```javascript
// Node.js 20.x runtime
import sql from 'mssql';
import { getConnection } from './db'; // pool from Secrets Manager

export const handler = async (event) => {
  const { role } = event.requestContext.authorizer; // from JWT authorizer
  
  // Role check (for Admin-only endpoints):
  if (role !== 'Admin') return { statusCode: 403, body: 'Forbidden' };
  
  const pool = await getConnection();
  const request = pool.request();
  
  // Bind parameters from event.body or event.pathParameters
  request.input('param1', sql.NVarChar, value1);
  
  const result = await request.execute('sp_StoredProcedureName');
  
  return {
    statusCode: 200,
    body: JSON.stringify(result.recordset),
    headers: { 'Content-Type': 'application/json' }
  };
};
```

**DB Connection:** Single connection pool per Lambda warm instance, initialized once outside the handler (cold start optimization). Credentials fetched from Secrets Manager at cold start and cached.

**Error handling:** All Lambda functions return structured errors:
```json
{ "error": "SP error message from SQL Server", "code": "DB_ERROR" }
```

### 5.2 Complete API Endpoint Catalog

#### Auth

| Method | Path | Lambda | SP / Query |
|---|---|---|---|
| POST | /auth/login | authLogin | Inline: SHA-256 check on users table |
| GET | /auth/me | authMe | Reads JWT claims only (no DB) |
| POST | /auth/logout | authLogout | Clears cookie only |

---

#### Dashboard

| Method | Path | Lambda | SP |
|---|---|---|---|
| GET | /dashboard/age-groups | dashboardAgeGroups | sp_GetAgeGroupChartData |
| GET | /dashboard/gender | dashboardGender | sp_GenderData |
| GET | /dashboard/zone-families | dashboardZone | sp_GetFamiliesByZone |
| GET | /dashboard/stats | dashboardStats | Inline COUNT queries |

---

#### Anbiyam

| Method | Path | Lambda | SP | Role |
|---|---|---|---|---|
| GET | /anbiyam | getAnbiyamList | sp_GetAnbiyamGridPaged | All |
| GET | /anbiyam/count | getAnbiyamCount | sp_GetAnbiyamTotalCount | All |
| GET | /anbiyam/dropdown | getAnbiyamDropdown | sp_GetAllAnbiyam | All |
| GET | /anbiyam/:id | getAnbiyamById | sp_GetSelectedAnbiyam | All |
| POST | /anbiyam | createAnbiyam | sp_InsertOrUpdateAnbiyam (id=0) | Admin |
| PUT | /anbiyam/:id | updateAnbiyam | sp_InsertOrUpdateAnbiyam (id=:id) | Admin |
| DELETE | /anbiyam/:id | deleteAnbiyam | sp_DeleteAnbiyam | Admin |

---

#### Families

| Method | Path | Lambda | SP | Role |
|---|---|---|---|---|
| GET | /families | getFamilyList | sp_GetFamilyBasicDetailsPaged | All |
| GET | /families/count | getFamilyCount | sp_GetFamilyTotalCount | All |
| GET | /families/occupations | getOccupations | sp_AggregateOccupations | All |
| GET | /families/:id | getFamilyById | sp_GetFamilyDetailsById | All |
| GET | /families/:id/members | getFamilyMembers | sp_GetFamilyMembersByFamilyId | All |
| GET | /families/:id/members/detail | getFamilyMembersDetail | sp_GetFamilyMembersDetailsByFamilyId | All |
| GET | /families/:id/members/dropdown | getMembersDropdown | sp_GetFamilyMembersForDropdown | All |
| GET | /families/:id/cemetery | getFamilyCemetery | sp_GetFamilyCemetery | All |
| POST | /families | createFamily | sp_SaveFamily + sp_SaveFamilyMember (transaction) | Admin |
| PUT | /families/:id | updateFamily | sp_SaveFamily + sp_SaveFamilyMember (transaction) | Admin |
| PUT | /families/:id/disable | disableFamily | sp_DisableFamily | Admin |
| DELETE | /families/:id | deleteFamily | sp_DeleteFamily | Admin |
| GET | /families/export | exportFamilies | sp_GetFamilyBasicDetailsForExport | All |

---

#### Cemetery

| Method | Path | Lambda | SP | Role |
|---|---|---|---|---|
| GET | /cemetery | getCemeteryList | sp_GetAllCemeteriesPaged | All |
| GET | /cemetery/count | getCemeteryCount | sp_GetAllCemeteriesTotalCount | All |
| POST | /cemetery/parish | createParishCemetery | sp_InsertOrUpdateCemetery | Admin |
| PUT | /cemetery/parish/:id | updateParishCemetery | sp_InsertOrUpdateCemetery | Admin |
| GET | /cemetery/nonparish | getNonParishList | sp_GetNonParishCemeteryList | All |
| POST | /cemetery/nonparish | createNonParish | sp_SaveNonParishCemetery | Admin |

---

#### Subscriptions

| Method | Path | Lambda | SP | Role |
|---|---|---|---|---|
| GET | /subscriptions | getSubscriptions | sp_GetSubscriptionsFiltered | All |
| GET | /subscriptions/count | getSubCount | sp_GetSubscriptionTotalCount | All |
| GET | /subscriptions/family/:id/year/:year | getFamilySubYear | sp_GetFamilySubscriptionYear | All |
| POST | /subscriptions/family/:id/month | saveMonth | sp_SetFamilySubscriptionMonth | Admin |
| GET | /subscriptions/family/:id/last-paid | getLastPaid | sp_GetLastPaidSubscriptionDate | All |
| GET | /subscriptions/cemetery/family/:id/year/:year | getCemeterySubYear | sp_GetCemeterySubscription | All |
| POST | /subscriptions/cemetery/family/:id | saveCemeteryYear | sp_SetCemeterySubscription | Admin |
| GET | /subscriptions/nonparish/:cemeteryId | getNonParishSubs | sp_GetNonParishCemeterySubscriptions | All |
| POST | /subscriptions/nonparish/:cemeteryId | saveNonParishSub | sp_SetNonParishCemeterySubscription | Admin |

---

### 5.3 API Gateway Setup

- **Type:** REST API (not HTTP API) — supports Lambda Authorizer and usage plans.
- **Lambda Authorizer:** Validates JWT on every request. Extracts `role` from JWT payload and injects into `event.requestContext.authorizer`.
- **CORS:** Configured to allow the CloudFront domain only. Preflight OPTIONS handled at API Gateway level.
- **Stage:** `prod` / `dev`. Stage variables used for environment-specific config.
- **Throttling:** 1000 req/s burst, 500 req/s steady (adjust based on parish size — typically < 10 concurrent users).

---

### 5.4 Database Migration — RDS Setup

**Steps:**
1. Create an AWS RDS SQL Server Standard instance (or Express for small load) in a private VPC subnet.
2. Run `Initial_ddl_Scripts.sql` to create all tables.
3. Run `Initial_dml_Scripts.sql` to create all base stored procedures.
4. Run `V4_ddl_Scripts.sql` to create the `nonparish_cemetery_subscription` table and all new indexes.
5. Run `V4_dml_Scripts.sql` to create all V4 stored procedures.
6. Migrate existing data using SQL Server Management Studio → Tasks → Export Data → target RDS instance.
7. Run `Initial_dummy_data_Scripts.sql` in dev environment for test data.

**Connection from Lambda:** Lambda runs in the same VPC as RDS. Uses a connection pool (mssql package). Credentials stored in Secrets Manager and referenced by ARN in Lambda environment variables.

**No changes to any SQL script.** All tables, indexes, and stored procedures are deployed exactly as written.

---

## 6. Implementation Phases

### Phase 1 — Infrastructure Setup (Week 1)

- Create AWS VPC, subnets, security groups.
- Provision RDS SQL Server, run all DDL + DML scripts.
- Configure Secrets Manager with DB credentials.
- Set up S3 bucket + CloudFront distribution.
- Set up API Gateway + Lambda IAM roles.
- Deploy a "health check" Lambda (`GET /health`) to verify VPC → RDS connectivity.

### Phase 2 — Auth + Lambda Foundation (Week 2)

- Build shared DB connection pool module (Lambda layer).
- Implement JWT signing/verification (RS256, key pair in Secrets Manager).
- Build Lambda Authorizer for API Gateway.
- Implement `POST /auth/login`, `GET /auth/me`, `POST /auth/logout`.
- Build React app shell: Login page, AuthContext, ProtectedRoute, AppShell with navigation.
- Wire login page to `POST /auth/login`. Test session flow end-to-end.

### Phase 3 — Dashboard + Anbiyam (Week 3)

- Build all Dashboard Lambda endpoints (age groups, gender, zone families, stats).
- Build React Dashboard page with Recharts charts and stat cards.
- Build Anbiyam Lambda endpoints (list, count, dropdown, CRUD).
- Build React Anbiyam list page + create/edit modal.
- Full end-to-end test: create anbiyam, edit, view in grid, attempt delete with family linked.

### Phase 4 — Family Management (Weeks 4–5)

This is the most complex phase due to `FamilyForm.tsx`.

- Build all Family Lambda endpoints (list, count, create, update, disable, delete, members).
- Build `FamilyList.tsx` with filter bar, paginated grid, member sub-grid.
- Build `FamilyForm.tsx` — all sections: general, husband, wife, up to 5 children, up to 2 other relations.
- Implement transaction Lambda: `POST/PUT /families` wraps `sp_SaveFamily` + multiple `sp_SaveFamilyMember` calls in a single BEGIN TRANSACTION / COMMIT / ROLLBACK.
- Test: create family with 3 children + 1 other relation, verify all members saved, edit and update, disable.

### Phase 5 — Cemetery (Week 6)

- Build Cemetery Lambda endpoints (parish + non-parish lists, CRUD).
- Build `CemeteryList.tsx` with parish/non-parish toggle and date filters.
- Build `CemeteryForm.tsx` and `NonParishForm.tsx`.
- Test: register a parish member death, verify `member_status` flips to Deceased, verify member disappears from living member dropdown.

### Phase 6 — Subscriptions (Week 7)

- Build all Subscription Lambda endpoints.
- Build `SubscriptionList.tsx` with all filters.
- Build `FamilySubscriptionForm.tsx` (12 month toggles).
- Build `CemeterySubscriptionForm.tsx` and `NonParishSubForm.tsx`.
- Test: mark 6 months paid for a family, verify amounts and dates saved, change year and verify fresh state.

### Phase 7 — PDF Export + Hardening (Week 8)

- Implement PDF export Lambda using `pdfkit` or `puppeteer` (Lambda layer). Mirrors the existing Syncfusion PDF export logic — family list table, anbiyam roster.
- Add input validation on all Lambda endpoints (Joi / zod schemas).
- Add rate limiting on auth endpoint (5 attempts / 15 minutes per IP, via API Gateway Usage Plan).
- Security review: verify CORS headers, JWT expiry, role enforcement on every Admin endpoint.
- Performance test: simulate 10 concurrent users, verify pagination performance on RDS.

### Phase 8 — UAT + Go-Live (Week 9–10)

- Deploy full stack to production AWS environment.
- Run data migration from existing SQL Server → RDS.
- Parallel run period: both WinForms and web app active for 2 weeks.
- User acceptance testing with parish admin staff.
- DNS cutover via Route 53 to custom domain (e.g. `parish.fatimachurch.in`).
- Decommission local SQL Server after verification.

---

## 7. Security Checklist

| Item | Implementation |
|---|---|
| HTTPS everywhere | CloudFront + ACM certificate, HTTP → HTTPS redirect |
| JWT HttpOnly cookie | Prevents XSS access to token |
| JWT RS256 signing | Asymmetric — Lambda Authorizer holds public key only |
| JWT expiry | 8 hours; no refresh token needed (internal admin tool) |
| Role enforcement | Every Admin-only Lambda checks role before executing SP |
| SQL injection | 100% parameterized — all DB access via stored procedures |
| CORS | CloudFront domain only; no wildcard origins |
| Secrets Manager | DB credentials never hardcoded in Lambda code |
| VPC isolation | RDS not publicly accessible; Lambda in same private subnet |
| Input validation | Joi schemas on all Lambda request bodies |
| Auth rate limiting | API Gateway Usage Plan: 5 failed logins/15min per IP |

---

## 8. Key Design Decisions and Rationale

**Why reuse stored procedures instead of ORMs?**  
The existing SPs encode all business logic (duplicate code prevention, cascade status updates, computed family codes, soft-delete patterns). Rewriting them in an ORM would risk regressions. SPs also provide a natural API contract boundary.

**Why Lambda (serverless) instead of EC2/ECS?**  
Parish management is low-concurrency (< 20 users). Lambda eliminates server management, scales to zero when the office is closed, and costs nearly nothing at this scale.

**Why SQL Server on RDS instead of migrating to PostgreSQL/MySQL?**  
All stored procedures use T-SQL features (dynamic column names via sp_executesql, CROSS APPLY VALUES for date pivots, TRY/CATCH patterns). A database engine migration would require rewriting every SP. RDS SQL Server preserves the entire investment.

**Why React + S3 instead of Next.js / server-rendered?**  
This is an internal admin tool. SEO is irrelevant. A pure SPA on S3+CloudFront is simpler to deploy, cache, and maintain. React Query handles data fetching and caching efficiently.

**Why not keep the WinForms app?**  
It requires Windows installation on every machine, cannot be used on phones or tablets at the church office, and relies on a local SQL Server that is a single point of failure. The web app is accessible from any browser on any device.

---

## 9. Risks and Mitigations

| Risk | Mitigation |
|---|---|
| RDS connectivity from Lambda cold starts | Use connection pooling; keep Lambda warm with scheduled EventBridge pings |
| Large family form complexity (up to 9 members) | Implement as multi-step wizard with auto-save draft to localStorage |
| Data migration errors | Run migration in staging first; validate row counts per table before cutover |
| User adoption | Keep WinForms running in parallel for 2 weeks during UAT |
| Slow PDF generation in Lambda | Use async Lambda with S3 pre-signed URL pattern: Lambda generates PDF → saves to S3 → returns download URL |
| SQL Server Express limit (10GB) | Use SQL Server Standard edition on RDS for production |

---

## 10. Summary of Reused vs New Components

| Component | Reused As-Is | New / Equivalent |
|---|---|---|
| All 9 DB tables | ✅ Deployed directly to RDS | — |
| All ~50 stored procedures | ✅ Called from Lambda | — |
| All 7 performance indexes (V4) | ✅ Created on RDS | — |
| SHA-256 auth logic | ✅ Same SQL in Lambda | — |
| Business rules (family code gen, cascade Deceased status, soft delete, anbiyam duplicate prevention) | ✅ Enforced in SPs unchanged | — |
| WinForms UI | — | React components |
| ADO.NET DatabaseHelper | — | mssql connection pool Lambda layer |
| AppTheme (colors/fonts) | — | CSS variables + Tailwind config |
| Syncfusion controls (ToggleButton, PDF) | — | React toggles + pdfkit |
| IE WebBrowser Leaflet map | — | react-leaflet |
| Local SQL Server | — | AWS RDS SQL Server |
| Windows-only deployment | — | S3 + CloudFront (any browser) |

---

*End of Redesign Plan*
