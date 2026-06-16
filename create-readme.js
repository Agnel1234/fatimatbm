// README Document Generator — Our Lady of Fatima, Tambaram
// Run: node create-readme.js
// Output: C:\Apps\fatimatbm\README.docx

const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell,
  Header, Footer, AlignmentType, HeadingLevel, BorderStyle, WidthType,
  ShadingType, VerticalAlign, PageNumber, PageBreak, TableOfContents,
  LevelFormat, ExternalHyperlink
} = require('docx');
const fs = require('fs');
const path = require('path');

// ─── helpers ─────────────────────────────────────────────────────────────────

const BRAND   = '185FA5';  // deep blue (zone 1 colour)
const BRAND2  = '0F6E56';  // teal (zone 4)
const GRAY    = 'F2F4F7';
const DGRAY   = 'D0D3D8';
const WHITE   = 'FFFFFF';
const BLACK   = '000000';

const border1 = { style: BorderStyle.SINGLE, size: 1, color: DGRAY };
const cellBorders = { top: border1, bottom: border1, left: border1, right: border1 };
const noBorder   = { style: BorderStyle.NIL, size: 0, color: WHITE };
const noBorders  = { top: noBorder, bottom: noBorder, left: noBorder, right: noBorder };

function h1(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_1,
    spacing: { before: 320, after: 160 },
    children: [new TextRun({ text, font: 'Arial', size: 32, bold: true, color: BRAND })]
  });
}

function h2(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_2,
    spacing: { before: 240, after: 120 },
    children: [new TextRun({ text, font: 'Arial', size: 26, bold: true, color: '333333' })]
  });
}

function h3(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_3,
    spacing: { before: 180, after: 80 },
    children: [new TextRun({ text, font: 'Arial', size: 22, bold: true, color: '555555' })]
  });
}

function p(text, options = {}) {
  return new Paragraph({
    spacing: { before: 60, after: 100 },
    children: [new TextRun({ text, font: 'Arial', size: 22, ...options })]
  });
}

function pBold(text) {
  return p(text, { bold: true });
}

function bullet(text, level = 0) {
  return new Paragraph({
    numbering: { reference: 'bullets', level },
    spacing: { before: 40, after: 40 },
    children: [new TextRun({ text, font: 'Arial', size: 22 })]
  });
}

function code(text) {
  return new Paragraph({
    spacing: { before: 80, after: 80 },
    indent: { left: 720 },
    children: [new TextRun({ text, font: 'Courier New', size: 18, color: '222222' })]
  });
}

function pageBreak() {
  return new Paragraph({ children: [new PageBreak()] });
}

function sectionRule() {
  return new Paragraph({
    spacing: { before: 120, after: 120 },
    border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: DGRAY } },
    children: []
  });
}

function kv(key, value) {
  const tw = 9360;
  return new Table({
    width: { size: tw, type: WidthType.DXA },
    columnWidths: [2400, 6960],
    rows: [
      new TableRow({ children: [
        new TableCell({
          borders: cellBorders,
          width: { size: 2400, type: WidthType.DXA },
          shading: { fill: GRAY, type: ShadingType.CLEAR },
          margins: { top: 60, bottom: 60, left: 120, right: 120 },
          children: [new Paragraph({ children: [new TextRun({ text: key, font: 'Arial', size: 20, bold: true })] })]
        }),
        new TableCell({
          borders: cellBorders,
          width: { size: 6960, type: WidthType.DXA },
          margins: { top: 60, bottom: 60, left: 120, right: 120 },
          children: [new Paragraph({ children: [new TextRun({ text: value, font: 'Arial', size: 20 })] })]
        })
      ]})
    ]
  });
}

function spacer(before = 80) {
  return new Paragraph({ spacing: { before, after: 0 }, children: [] });
}

// ─── title page ──────────────────────────────────────────────────────────────

function buildTitlePage() {
  return [
    spacer(1440),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 0, after: 120 },
      children: [new TextRun({ text: 'Our Lady of Fatima, Tambaram', font: 'Arial', size: 48, bold: true, color: BRAND })]
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 0, after: 240 },
      children: [new TextRun({ text: 'Church Management System', font: 'Arial', size: 36, color: '444444' })]
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: BRAND } },
      spacing: { before: 0, after: 360 },
      children: []
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 0, after: 120 },
      children: [new TextRun({ text: 'Technical & Functional Reference', font: 'Arial', size: 28, color: '666666' })]
    }),
    new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { before: 0, after: 80 },
      children: [new TextRun({ text: 'Version 1.0  •  June 2026', font: 'Arial', size: 22, color: '888888' })]
    }),
    pageBreak()
  ];
}

// ─── section 1: project overview ─────────────────────────────────────────────

function buildSection1() {
  return [
    h1('1. Project Overview'),
    p('The Our Lady of Fatima, Tambaram Church Management System is a full-stack web application that digitises the administrative operations of the parish. It manages families, anbiyams (small Christian communities), subscriptions, cemetery registrations, and provides zone-level geographic analytics to parish administrators.'),
    spacer(80),
    h2('1.1 Goals'),
    bullet('Centralise parish family records and membership data'),
    bullet('Track annual subscription payments across all 25 anbiyams and 8 geographic zones'),
    bullet('Manage cemetery plots and associated annual subscriptions'),
    bullet('Provide administrators with a real-time dashboard and geographic zone map'),
    bullet('Secure, role-based access for parish staff'),
    spacer(80),
    h2('1.2 Scope'),
    bullet('25 anbiyams organised under 8 geographic zones'),
    bullet('Approx. 1,200+ registered families'),
    bullet('Monthly subscription tracking (January – December)'),
    bullet('Cemetery subscription management'),
    bullet('User roles: Admin, Staff, Read-only'),
  ];
}

// ─── section 2: architecture ──────────────────────────────────────────────────

function buildSection2() {
  const tw = 9360;
  const cols = [3000, 6360];
  function archRow(layer, detail, shade) {
    return new TableRow({ children: [
      new TableCell({
        borders: cellBorders,
        width: { size: cols[0], type: WidthType.DXA },
        shading: { fill: shade, type: ShadingType.CLEAR },
        margins: { top: 80, bottom: 80, left: 120, right: 120 },
        verticalAlign: VerticalAlign.CENTER,
        children: [new Paragraph({ children: [new TextRun({ text: layer, font: 'Arial', size: 20, bold: true })] })]
      }),
      new TableCell({
        borders: cellBorders,
        width: { size: cols[1], type: WidthType.DXA },
        margins: { top: 80, bottom: 80, left: 120, right: 120 },
        children: [new Paragraph({ children: [new TextRun({ text: detail, font: 'Arial', size: 20 })] })]
      })
    ]});
  }
  const headerRow = new TableRow({ children: [
    new TableCell({
      borders: cellBorders, width: { size: cols[0], type: WidthType.DXA },
      shading: { fill: BRAND, type: ShadingType.CLEAR },
      margins: { top: 80, bottom: 80, left: 120, right: 120 },
      children: [new Paragraph({ children: [new TextRun({ text: 'Layer', font: 'Arial', size: 20, bold: true, color: WHITE })] })]
    }),
    new TableCell({
      borders: cellBorders, width: { size: cols[1], type: WidthType.DXA },
      shading: { fill: BRAND, type: ShadingType.CLEAR },
      margins: { top: 80, bottom: 80, left: 120, right: 120 },
      children: [new Paragraph({ children: [new TextRun({ text: 'Technology / Service', font: 'Arial', size: 20, bold: true, color: WHITE })] })]
    })
  ]});

  return [
    h1('2. System Architecture'),
    p('The system follows a serverless architecture on AWS, with a React single-page application served from CloudFront/S3 calling a REST API backed by AWS Lambda functions and an Amazon RDS SQL Server database.'),
    spacer(120),
    new Table({
      width: { size: tw, type: WidthType.DXA },
      columnWidths: cols,
      rows: [
        headerRow,
        archRow('Frontend',         'React 18 + Vite + TypeScript + Tailwind CSS', GRAY),
        archRow('API Layer',        'AWS API Gateway (REST) + AWS Lambda (Node.js 18)', WHITE),
        archRow('Infrastructure',   'Serverless Framework v3 (serverless.yml), stage: prod, region: ap-south-1', GRAY),
        archRow('Database',         'Amazon RDS SQL Server Express — instance: fatimatbm-web-dev\nHost: fatimatbm-web-dev.c9ycygc8iku9.ap-south-1.rds.amazonaws.com\nDB: fatimachurchtbm', WHITE),
        archRow('Static Hosting',   'S3 bucket: fatima-church-frontend-prod\nCloudFront distribution: E78KFDYPNO5JO\nDomain: davwb2h2rp48q.cloudfront.net', GRAY),
        archRow('CI/CD',            'GitHub Actions (.github/workflows/deploy.yml)\nTriggered on push to main branch', WHITE),
        archRow('Authentication',   'JWT stored in HttpOnly cookie, SameSite=None (cross-origin Lambda/APIGW)', GRAY),
        archRow('Source Control',   'GitHub: https://github.com/Agnel1234/fatimachurchtbmWeb.git', WHITE),
      ]
    }),
    spacer(120),
    h2('2.1 Request Flow'),
    bullet('Browser loads React SPA from CloudFront (S3 origin)'),
    bullet('All API calls go to: https://dn2j2ddqwf.execute-api.ap-south-1.amazonaws.com/prod'),
    bullet('API Gateway routes each path to the corresponding Lambda function'),
    bullet('Lambda connects to RDS SQL Server via mssql driver, runs stored procedures'),
    bullet('JWT in HttpOnly cookie is validated inside each Lambda on every request'),
    spacer(80),
    h2('2.2 Lambda Configuration'),
    bullet('Runtime: Node.js 18.x'),
    bullet('Timeout: 29 seconds (API Gateway max is 29 s)'),
    bullet('Memory: 256 MB'),
    bullet('VPC: Lambda placed inside the same VPC as RDS for private connectivity'),
    bullet('Environment variables: DB_HOST, DB_NAME, DB_USER, DB_PASSWORD, JWT_SECRET (stored as Lambda env vars)'),
    spacer(80),
    h2('2.3 CI/CD Pipeline'),
    bullet('Developer pushes to main branch on GitHub'),
    bullet('GitHub Actions workflow triggers automatically'),
    bullet('Frontend: npm ci → npm run build → aws s3 sync dist/ to S3 → CloudFront invalidation'),
    bullet('Backend: npm ci → npx serverless deploy --stage prod (deploys all Lambda functions)'),
    bullet('Database migrations: POST /health/run-migrations (called post-deploy to apply stored procedure changes)'),
    bullet('Secrets: AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY, and all env vars stored in GitHub Secrets'),
  ];
}

// ─── section 3: functional features ──────────────────────────────────────────

function buildSection3() {
  return [
    h1('3. Functional Features'),

    h2('3.1 Family Management'),
    p('Families are the core entity of the system. Each family belongs to one anbiyam.'),
    bullet('Create, view, edit, and delete family records'),
    bullet('Fields: family code, head of family, address, phone, email, anbiyam'),
    bullet('Family members can be added under each family'),
    bullet('Search families by name or family code'),
    bullet('Pagination with configurable page size'),
    spacer(60),

    h2('3.2 Anbiyam Management'),
    p('Anbiyams are small Christian communities (25 in total) grouped into 8 geographic zones.'),
    bullet('Create, edit, and delete anbiyam records'),
    bullet('Fields: anbiyam name, anbiyam code, zone number (1–8), coordinator name, coordinator phone, assistant coordinator, email'),
    bullet('Each anbiyam is assigned to one of 8 zones via the zone integer column'),
    bullet('Filter anbiyams by zone'),
    spacer(60),

    h2('3.3 Subscription Management (Monthly Matrix)'),
    p('Tracks annual subscription payments for each family, showing a 12-month matrix (Jan–Dec).'),
    bullet('Grid view: one row per family, 12 columns for payment months'),
    bullet('Each cell shows payment status: Paid / Unpaid / N/A'),
    bullet('Filter by year and anbiyam'),
    bullet('Search families by name or code'),
    bullet('Shows count of paid months per family'),
    bullet('Page size capped at 50 families for performance'),
    bullet('Powered by stored procedure sp_GetSubscriptionMatrix'),
    spacer(60),

    h2('3.4 Cemetery Subscription Management'),
    p('Tracks annual cemetery plot subscriptions separately from regular parish subscriptions.'),
    bullet('Same 12-month matrix layout as regular subscriptions'),
    bullet('Filter by year, anbiyam, and cemetery type'),
    bullet('Page size capped at 50 families for performance'),
    bullet('Powered by stored procedure sp_GetCemeterySubscriptionList'),
    bullet('Also manages Non-Parish Cemetery records separately'),
    spacer(60),

    h2('3.5 Dashboard'),
    p('The main landing page for authenticated users showing parish-wide statistics.'),
    bullet('Total families, paid families, unpaid families'),
    bullet('8 Zone Cards: one card per geographic zone, showing zone name, colour-coded header, list of anbiyams in that zone, total/paid family counts, progress bar, and paid percentage'),
    bullet('Each zone card links to the Anbiyam Map page pre-filtered to that zone'),
    bullet('Fixed colour identity per zone (see Zone Colours section)'),
    bullet('Charts/analytics section with bar and pie charts'),
    spacer(60),

    h2('3.6 Zone Map (Anbiyam Map Page)'),
    p('An interactive geographic map showing the 8 zones as SVG polygons, with drill-down to anbiyam and family level.'),
    bullet('8 SVG polygon regions, one per zone, colour-coded with fixed zone identity colours'),
    bullet('Clicking a zone polygon opens a detail panel on the right'),
    bullet('Detail panel shows: zone summary (total families, paid families, anbiyam count), list of anbiyams in the zone'),
    bullet('Clicking an anbiyam within the zone shows families of that anbiyam with dues/paid filter tabs'),
    bullet('URL parameter ?zone=<number> pre-selects a zone (used by dashboard zone card links)'),
    bullet('Powered by stored procedure sp_GetAnbiyamMapSummary (returns zone column)'),
    spacer(60),

    h2('3.7 User Management'),
    p('Administrators can create and manage system user accounts.'),
    bullet('Create, edit, deactivate user accounts'),
    bullet('Role assignment: Admin, Staff, Read-only'),
    bullet('JWT-based authentication with HttpOnly cookie session'),
    bullet('Login / logout flow'),
    spacer(60),

    h2('3.8 Health & Migrations'),
    p('Internal admin endpoint for database maintenance.'),
    bullet('GET /health – returns Lambda uptime and DB connectivity status'),
    bullet('POST /health/run-migrations – applies stored procedure migrations in sequence (idempotent, uses CREATE OR ALTER PROCEDURE)'),
  ];
}

// ─── section 4: database schema ───────────────────────────────────────────────

function buildSection4() {
  return [
    h1('4. Database Schema'),
    p('Database: fatimachurchtbm on Amazon RDS SQL Server Express (fatimatbm-web-dev instance, ap-south-1).'),
    spacer(80),

    h2('4.1 Core Tables'),

    h3('dbo.family'),
    bullet('family_id (PK, int identity)'),
    bullet('family_code (varchar) – unique short code'),
    bullet('head_of_family (varchar) – name of head of household'),
    bullet('anbiyam_id (FK → dbo.anbiyam)'),
    bullet('address, phone, email, and other contact fields'),
    spacer(60),

    h3('dbo.anbiyam'),
    bullet('anbiyam_id (PK, int identity)'),
    bullet('anbiyam_name (varchar)'),
    bullet('anbiyam_code (varchar)'),
    bullet('zone (int, 1–8) – geographic zone assignment'),
    bullet('coordinator_name, coordinator_phone, ass_coordinator_name, coordinator_email'),
    spacer(60),

    h3('dbo.family_subscription'),
    bullet('id (PK)'),
    bullet('family_id (FK → dbo.family)'),
    bullet('subscription_year (int)'),
    bullet('subscription_month (int, 1–12)'),
    bullet('payment_status (varchar: Paid / Unpaid)'),
    bullet('payment_date, amount'),
    spacer(60),

    h3('dbo.family_member'),
    bullet('id (PK)'),
    bullet('family_id (FK → dbo.family)'),
    bullet('member_name, relationship, date_of_birth, gender, etc.'),
    spacer(60),

    h3('dbo.users'),
    bullet('user_id (PK)'),
    bullet('username, password_hash, role, is_active'),
    bullet('created_at, updated_at'),
    spacer(80),

    h2('4.2 Zone Column on dbo.anbiyam'),
    p('The zone integer column (1–8) on dbo.anbiyam is the foundation of all zone-based navigation. All 25 anbiyams are pre-assigned to one of 8 zones. Zone 0 is used for anbiyams with no zone assigned (fallback).'),
    spacer(80),

    h2('4.3 Zone Colours'),
    p('Fixed colour identity per zone number, used consistently across Dashboard and Anbiyam Map:'),
    bullet('Zone 1: #185FA5 (deep blue)'),
    bullet('Zone 2: #534AB7 (indigo)'),
    bullet('Zone 3: #A32D2D (crimson)'),
    bullet('Zone 4: #0F6E56 (teal)'),
    bullet('Zone 5: #854F0B (amber brown)'),
    bullet('Zone 6: #3B6D11 (forest green)'),
    bullet('Zone 7: #993556 (magenta)'),
    bullet('Zone 8: #993C1D (rust orange)'),
  ];
}

// ─── section 5: stored procedures ────────────────────────────────────────────

function buildSection5() {
  return [
    h1('5. Stored Procedures'),
    p('All stored procedures follow the CREATE OR ALTER PROCEDURE pattern so that running migrations is idempotent. Migrations are applied by calling POST /health/run-migrations after each deployment.'),
    spacer(80),

    h2('5.1 sp_GetSubscriptionMatrix'),
    h3('Purpose'),
    p('Returns the 12-month subscription payment matrix for all families in a given year, with optional anbiyam filter and text search. Supports server-side pagination.'),
    h3('Parameters'),
    bullet('@year int'),
    bullet('@anbiyam_id int (nullable)'),
    bullet('@search nvarchar(200) (nullable)'),
    bullet('@pageNumber int'),
    bullet('@pageSize int (capped at 50 in the Lambda handler)'),
    h3('Result Sets'),
    bullet('Recordset 0: family rows with 12-month status columns (jan_status … dec_status) and paidCount'),
    bullet('Recordset 1: total row count for pagination'),
    h3('Key Change: Removed family_member Subquery'),
    p('Previous versions joined to dbo.family_member to retrieve the lady_head_name for display. This subquery caused a full table scan across all family members for every paginated request, leading to Lambda timeouts (> 29 s) even for page sizes of 50.'),
    p('The subquery was removed entirely. The display name now uses ISNULL(NULLIF(f.head_of_family, \'\'), f.family_code) from the family row, which is fully indexed and instantaneous.'),
    spacer(80),

    h2('5.2 sp_GetCemeterySubscriptionList'),
    h3('Purpose'),
    p('Returns the 12-month cemetery subscription matrix. Same layout and pagination as sp_GetSubscriptionMatrix but filtered to cemetery subscriptions.'),
    h3('Key Change: Removed family_member Subquery'),
    p('Identical fix as sp_GetSubscriptionMatrix – removed the family_member join from both the data recordset and the count recordset. Display name now uses ISNULL(NULLIF(f.head_of_family, \'\'), f.family_code).'),
    spacer(80),

    h2('5.3 sp_GetAnbiyamMapSummary'),
    h3('Purpose'),
    p('Returns anbiyam-level payment summary data used by both the Dashboard zone cards and the Anbiyam Map Page.'),
    h3('Key Change: Added zone Column'),
    p('Both Recordset 0 (anbiyam summary) and Recordset 1 (family dues detail) now include ISNULL(a.zone, 0) AS zone. The ORDER BY was updated to a.zone, a.anbiyam_name. The frontend groups 25 anbiyam rows into 8 zone aggregates using this column.'),
    h3('Result Sets'),
    bullet('Recordset 0: one row per anbiyam – anbiyamId, anbiyamName, zone, totalFamilies, paidFamilies'),
    bullet('Recordset 1: one row per family with dues – familyId, familyCode, headOfFamily, anbiyamId, anbiyamName, zone, monthsPaid, monthsUnpaid'),
    spacer(80),

    h2('5.4 Migration Runner (run-migrations.js)'),
    p('Located at backend/health/run-migrations.js. Applies migrations 1–13 in sequence on POST /health/run-migrations. Each migration checks a version table before applying, making the process fully idempotent. Uses the CREATE OR ALTER PROCEDURE SQL syntax for all SP migrations.'),
    bullet('Migration 9: sp_GetSubscriptionMatrix (removed family_member join from Recordset 0)'),
    bullet('Migration 11: sp_GetCemeterySubscriptionList (removed family_member join from both recordsets)'),
    bullet('Migration 13: sp_GetAnbiyamMapSummary (added zone column to both recordsets)'),
  ];
}

// ─── section 6: frontend architecture ────────────────────────────────────────

function buildSection6() {
  return [
    h1('6. Frontend Architecture'),

    h2('6.1 Technology Stack'),
    bullet('React 18 with functional components and hooks'),
    bullet('TypeScript for type safety across all files'),
    bullet('Vite as the build tool (fast HMR in dev, optimised production build)'),
    bullet('Tailwind CSS for utility-first styling'),
    bullet('React Router v6 for client-side navigation with useSearchParams for URL state'),
    bullet('Axios (via api/client.ts) for all API calls with withCredentials: true for cookie auth'),
    spacer(80),

    h2('6.2 Page Structure'),
    bullet('/ – Login'),
    bullet('/dashboard – Dashboard (zone cards, stats, charts)'),
    bullet('/families – Family list and management'),
    bullet('/family/:id – Family detail with members'),
    bullet('/anbiyam – Anbiyam list and management'),
    bullet('/anbiyam-map – Zone map (?zone=<1-8> pre-selects a zone)'),
    bullet('/subscriptions – Monthly subscription matrix'),
    bullet('/cemetery – Cemetery subscription matrix'),
    bullet('/cemetery/non-parish – Non-parish cemetery records'),
    bullet('/users – User management (admin only)'),
    spacer(80),

    h2('6.3 Authentication'),
    p('JWT is issued on login and stored in an HttpOnly, SameSite=None, Secure cookie. The cookie is automatically sent by the browser on every API request (withCredentials: true on the Axios client). On 401 responses, the app redirects to /login.'),
    spacer(80),

    h2('6.4 Zone Navigation (Dashboard → Map)'),
    p('The Dashboard groups the 25 anbiyam rows returned by sp_GetAnbiyamMapSummary into 8 zone aggregates using useMemo and a zoneMap accumulator. Each zone card renders with the fixed zone colour. Clicking a zone card navigates to /anbiyam-map?zone=<zoneNumber>. The AnbiyamMapPage reads the ?zone search param on mount and pre-selects the corresponding zone polygon.'),
    spacer(80),

    h2('6.5 Zone Map (AnbiyamMapPage.tsx)'),
    p('The page renders an SVG canvas (viewBox 0 0 500 400) with 8 polygon paths, one per zone. Polygons are pre-defined in the ZONE_POLYGONS array (indexed 0–7, matching zones 1–8). Clicking a polygon highlights it and loads the detail panel. The detail panel uses tabs to switch between the anbiyam list and the dues families list within the selected zone.'),
  ];
}

// ─── section 7: deployment ────────────────────────────────────────────────────

function buildSection7() {
  return [
    h1('7. Deployment'),

    h2('7.1 One-Command Developer Push'),
    p('All changes are deployed using push_workflow.bat located at C:\\Apps\\fatimatbmweb\\push_workflow.bat. The batch file:'),
    bullet('Stages all modified files (git add)'),
    bullet('Commits with a descriptive message'),
    bullet('Pushes to origin main'),
    p('GitHub Actions then handles the rest automatically.'),
    spacer(80),

    h2('7.2 GitHub Actions Workflow (deploy.yml)'),
    p('The .github/workflows/deploy.yml workflow runs on every push to main and performs:'),
    h3('Backend Deploy'),
    bullet('npm ci in /backend'),
    bullet('npx serverless deploy --stage prod (deploys all Lambda functions via Serverless Framework)'),
    bullet('POST /health/run-migrations to apply any new stored procedure migrations'),
    h3('Frontend Deploy'),
    bullet('npm ci in /frontend'),
    bullet('npm run build (Vite produces optimised dist/ bundle)'),
    bullet('aws s3 sync dist/ s3://fatima-church-frontend-prod --delete'),
    bullet('aws cloudfront create-invalidation --distribution-id E78KFDYPNO5JO --paths "/*"'),
    spacer(80),

    h2('7.3 Environment Variables & Secrets'),
    p('All secrets are stored in GitHub Secrets and injected into GitHub Actions at deploy time. They are never committed to source code.'),
    bullet('AWS_ACCESS_KEY_ID / AWS_SECRET_ACCESS_KEY – IAM deploy user'),
    bullet('DB_HOST, DB_NAME, DB_USER, DB_PASSWORD – RDS connection'),
    bullet('JWT_SECRET – token signing key'),
    spacer(80),

    h2('7.4 AWS Resources Summary'),
    spacer(40),
    kv('API Gateway', 'https://dn2j2ddqwf.execute-api.ap-south-1.amazonaws.com/prod'),
    spacer(20),
    kv('RDS Instance', 'fatimatbm-web-dev (ap-south-1)'),
    spacer(20),
    kv('RDS Host', 'fatimatbm-web-dev.c9ycygc8iku9.ap-south-1.rds.amazonaws.com'),
    spacer(20),
    kv('Database', 'fatimachurchtbm'),
    spacer(20),
    kv('S3 Bucket', 'fatima-church-frontend-prod'),
    spacer(20),
    kv('CloudFront', 'E78KFDYPNO5JO — davwb2h2rp48q.cloudfront.net'),
    spacer(20),
    kv('GitHub Repo', 'https://github.com/Agnel1234/fatimachurchtbmWeb.git'),
  ];
}

// ─── section 8: technical changes log ────────────────────────────────────────

function buildSection8() {
  return [
    h1('8. Technical Changes Log'),
    p('This section records all significant technical changes made during the development sessions.'),
    spacer(80),

    h2('8.1 Zone-Based Navigation System'),
    h3('Motivation'),
    p('Parish administrators wanted to view subscription health at the zone level (8 zones) rather than per-anbiyam (25 anbiyams). Zones map to geographic areas of Tambaram.'),
    h3('Backend Changes'),
    bullet('Migration 13: sp_GetAnbiyamMapSummary updated to include ISNULL(a.zone, 0) AS zone in both recordsets'),
    bullet('ORDER BY changed to a.zone, a.anbiyam_name so zones are returned in order'),
    h3('Frontend Changes — DashboardPage.tsx'),
    bullet('Zone grouping: 25 anbiyam rows from the API are aggregated into 8 zone objects using a useMemo reducer'),
    bullet('Zone cards: each of the 8 zone cards shows a coloured header (zone number + zone colour), list of anbiyams, total/paid family counts, and a progress bar'),
    bullet('Clicking a zone card navigates to /anbiyam-map?zone=<zoneNumber>'),
    bullet('Replaced keyword-based and hash-based colour functions with a fixed ZONE_COLORS array indexed by zone number'),
    h3('Frontend Changes — AnbiyamMapPage.tsx'),
    bullet('Fully rewritten to use zone-level grouping instead of per-anbiyam polygons'),
    bullet('8 SVG polygon paths (ZONE_POLYGONS array) representing geographic zone boundaries'),
    bullet('useMemo groups API anbiyam data into ZoneGroup objects'),
    bullet('useSearchParams reads ?zone= on mount to pre-select a zone from the dashboard link'),
    bullet('Detail panel: zone header, anbiyam list within zone, drill-down to family dues list'),
    spacer(80),

    h2('8.2 Subscription Performance Fix (Timeout Removal)'),
    h3('Problem'),
    p('The Subscription page (/subscriptions) and Cemetery Subscription page (/cemetery) were showing "Failed to load subscription data" and "Failed to load cemetery subscription data" errors. The root cause was Lambda timeouts: the stored procedures were exceeding the 29-second API Gateway limit.'),
    h3('Root Cause Analysis'),
    p('Both sp_GetSubscriptionMatrix and sp_GetCemeterySubscriptionList contained a correlated subquery joining to dbo.family_member to retrieve the lady head name. Even though the page only returns 50 rows (OFFSET/FETCH NEXT pagination), SQL Server must evaluate the ORDER BY expression across ALL rows before pagination. The family_member subquery inside the ORDER BY caused a full table scan of family_member for every paginated request.'),
    h3('Fix 1: Removed family_member Subquery from sp_GetSubscriptionMatrix'),
    p('Recordset 0 no longer joins to dbo.family_member. The display name field familyHead now uses ISNULL(NULLIF(f.head_of_family, \'\'), f.family_code) – a simple column reference with no subquery. The search WHERE clause was also simplified to check only head_of_family and family_code columns.'),
    h3('Fix 2: Removed family_member Subquery from sp_GetCemeterySubscriptionList'),
    p('Same fix applied to both Recordset 0 and Recordset 1 of the cemetery SP.'),
    h3('Fix 3: Capped Page Size at 50'),
    bullet('backend/subscriptions/matrix.js: Math.min(50, parseInt(q.pageSize || \'50\', 10))'),
    bullet('backend/subscriptions/cemetery-list.js: same cap applied'),
    spacer(80),

    h2('8.3 RDS Database Migration (Instance Change)'),
    h3('Old Instance'),
    p('fatimachurchinstanceind – original RDS instance. Pending decommission after confirming all data is on the new instance.'),
    h3('New Instance'),
    p('fatimatbm-web-dev – current production instance. All Lambda functions connect to this instance. Daily automated backups enabled (7-day retention).'),
    spacer(80),

    h2('8.4 Database Migration Runner'),
    p('backend/health/run-migrations.js runs numbered migrations in sequence. Each migration is idempotent (checks a _migrations version table before applying). Uses CREATE OR ALTER PROCEDURE for all SP changes. Called automatically by the GitHub Actions deploy workflow after each backend deployment.'),
    spacer(80),

    h2('8.5 JWT HttpOnly Cookie Authentication'),
    p('Authentication uses JWT tokens stored in HttpOnly cookies with SameSite=None and Secure flags to allow cross-origin requests from the CloudFront domain to the API Gateway domain. The Axios client is configured with withCredentials: true. All Lambda handlers validate the JWT on every request.'),
    spacer(80),

    h2('8.6 Sidebar & Layout Updates'),
    p('frontend/src/components/Sidebar.tsx and Layout.tsx were updated to include the Anbiyam Map navigation link and to support the zone-based navigation structure.'),
    spacer(80),

    h2('8.7 App Router Updates (App.tsx)'),
    p('The AnbiyamMapPage route (/anbiyam-map) was added to the React Router configuration in App.tsx.'),
  ];
}

// ─── section 9: troubleshooting ───────────────────────────────────────────────

function buildSection9() {
  return [
    h1('9. Troubleshooting'),

    h2('9.1 "Failed to load subscription data" Error'),
    bullet('Symptom: The subscription matrix page shows an error toast on load'),
    bullet('Cause: Lambda timeout (> 29 s) due to family_member subquery in SP'),
    bullet('Fix: Run POST /health/run-migrations to apply migration 9 (removes the subquery)'),
    bullet('Verify: Check CloudWatch logs for the subscription Lambda — query time should drop to < 1 s'),
    spacer(60),

    h2('9.2 Zone Cards All Show Same Colour'),
    bullet('Symptom: All 8 zone cards on the dashboard appear in gray'),
    bullet('Cause: The ZONE_COLORS array was not being indexed by zone number, or the zone field was missing from the API response'),
    bullet('Fix: Ensure migration 13 has been applied (sp_GetAnbiyamMapSummary returns zone column). Verify the frontend ZONE_COLORS lookup uses the zone integer field.'),
    spacer(60),

    h2('9.3 Lambda Cold Start Delays'),
    bullet('Symptom: First request after idle period takes 5–10 s'),
    bullet('Cause: Lambda cold start — Node.js runtime and mssql connection pool initialisation'),
    bullet('Mitigation: Consider enabling Lambda Provisioned Concurrency for the most-used functions'),
    spacer(60),

    h2('9.4 Deployment Failures'),
    bullet('Check GitHub Actions logs for the failed step'),
    bullet('Backend failures: usually a serverless.yml syntax error or missing IAM permission'),
    bullet('Frontend failures: check S3 sync or CloudFront invalidation step'),
    bullet('DB migration failures: check /health/run-migrations response body for which migration failed and why'),
    spacer(60),

    h2('9.5 RDS Connection Refused'),
    bullet('Ensure Lambda and RDS are in the same VPC and subnet'),
    bullet('Check the RDS security group allows inbound on port 1433 from the Lambda security group'),
    bullet('Verify DB_HOST environment variable in Lambda matches the current RDS instance endpoint'),
  ];
}

// ─── build document ───────────────────────────────────────────────────────────

async function build() {
  const doc = new Document({
    numbering: {
      config: [
        {
          reference: 'bullets',
          levels: [
            {
              level: 0,
              format: LevelFormat.BULLET,
              text: '•',
              alignment: AlignmentType.LEFT,
              style: { paragraph: { indent: { left: 720, hanging: 360 } } }
            },
            {
              level: 1,
              format: LevelFormat.BULLET,
              text: '○',
              alignment: AlignmentType.LEFT,
              style: { paragraph: { indent: { left: 1080, hanging: 360 } } }
            }
          ]
        }
      ]
    },
    styles: {
      default: {
        document: { run: { font: 'Arial', size: 22 } }
      },
      paragraphStyles: [
        {
          id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Normal', quickFormat: true,
          run: { size: 32, bold: true, font: 'Arial', color: BRAND },
          paragraph: { spacing: { before: 320, after: 160 }, outlineLevel: 0 }
        },
        {
          id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Normal', quickFormat: true,
          run: { size: 26, bold: true, font: 'Arial', color: '333333' },
          paragraph: { spacing: { before: 240, after: 120 }, outlineLevel: 1 }
        },
        {
          id: 'Heading3', name: 'Heading 3', basedOn: 'Normal', next: 'Normal', quickFormat: true,
          run: { size: 22, bold: true, font: 'Arial', color: '555555' },
          paragraph: { spacing: { before: 180, after: 80 }, outlineLevel: 2 }
        }
      ]
    },
    sections: [
      {
        properties: {
          page: {
            size: { width: 12240, height: 15840 },
            margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 }
          }
        },
        headers: {
          default: new Header({
            children: [
              new Paragraph({
                alignment: AlignmentType.RIGHT,
                border: { bottom: { style: BorderStyle.SINGLE, size: 4, color: DGRAY } },
                children: [
                  new TextRun({ text: 'Our Lady of Fatima, Tambaram — Church Management System', font: 'Arial', size: 18, color: '888888' })
                ]
              })
            ]
          })
        },
        footers: {
          default: new Footer({
            children: [
              new Paragraph({
                alignment: AlignmentType.CENTER,
                border: { top: { style: BorderStyle.SINGLE, size: 4, color: DGRAY } },
                children: [
                  new TextRun({ text: 'Page ', font: 'Arial', size: 18, color: '888888' }),
                  new TextRun({ children: [PageNumber.CURRENT], font: 'Arial', size: 18, color: '888888' }),
                  new TextRun({ text: ' of ', font: 'Arial', size: 18, color: '888888' }),
                  new TextRun({ children: [PageNumber.TOTAL_PAGES], font: 'Arial', size: 18, color: '888888' }),
                ]
              })
            ]
          })
        },
        children: [
          ...buildTitlePage(),
          // Table of Contents
          new Paragraph({
            heading: HeadingLevel.HEADING_1,
            children: [new TextRun({ text: 'Table of Contents', font: 'Arial', size: 32, bold: true, color: BRAND })]
          }),
          new TableOfContents('Table of Contents', { hyperlink: true, headingStyleRange: '1-3' }),
          pageBreak(),
          ...buildSection1(),
          pageBreak(),
          ...buildSection2(),
          pageBreak(),
          ...buildSection3(),
          pageBreak(),
          ...buildSection4(),
          pageBreak(),
          ...buildSection5(),
          pageBreak(),
          ...buildSection6(),
          pageBreak(),
          ...buildSection7(),
          pageBreak(),
          ...buildSection8(),
          pageBreak(),
          ...buildSection9(),
        ]
      }
    ]
  });

  const buffer = await Packer.toBuffer(doc);
  const outPath = path.join(__dirname, 'README.docx');
  fs.writeFileSync(outPath, buffer);
  console.log('Created: ' + outPath);
}

build().catch(err => { console.error(err); process.exit(1); });
