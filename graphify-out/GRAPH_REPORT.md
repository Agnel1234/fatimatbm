# Graph Report - .  (2026-05-20)

## Corpus Check
- 64 files · ~109,974 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 455 nodes · 676 edges · 46 communities (29 shown, 17 thin omitted)
- Extraction: 94% EXTRACTED · 6% INFERRED · 0% AMBIGUOUS · INFERRED: 39 edges (avg confidence: 0.88)
- Token cost: 108,708 input · 19,182 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Database Schema and PDF Stored Procs|Database Schema and PDF Stored Procs]]
- [[_COMMUNITY_Form Designer Base Types|Form Designer Base Types]]
- [[_COMMUNITY_Modern PDF Generator|Modern PDF Generator]]
- [[_COMMUNITY_Form1 Grid Event Handlers|Form1 Grid Event Handlers]]
- [[_COMMUNITY_Application Architecture Overview|Application Architecture Overview]]
- [[_COMMUNITY_AppTheme Styling|AppTheme Styling]]
- [[_COMMUNITY_Enhanced PDF Generator|Enhanced PDF Generator]]
- [[_COMMUNITY_FamilyPopup Logic|FamilyPopup Logic]]
- [[_COMMUNITY_PDF Modernization Docs|PDF Modernization Docs]]
- [[_COMMUNITY_Form Layout Controls|Form Layout Controls]]
- [[_COMMUNITY_Subscription Popup|Subscription Popup]]
- [[_COMMUNITY_Deployment Documentation|Deployment Documentation]]
- [[_COMMUNITY_Non-Parish Cemetery Subscription|Non-Parish Cemetery Subscription]]
- [[_COMMUNITY_Form1 PDF Data Loading|Form1 PDF Data Loading]]
- [[_COMMUNITY_Form1 Dashboard Designer|Form1 Dashboard Designer]]
- [[_COMMUNITY_Cemetery Subscription Popup|Cemetery Subscription Popup]]
- [[_COMMUNITY_ThemedDialog Message Boxes|ThemedDialog Message Boxes]]
- [[_COMMUNITY_LoginForm Authentication|LoginForm Authentication]]
- [[_COMMUNITY_Quick Start PDF Integration|Quick Start PDF Integration]]
- [[_COMMUNITY_Program Entry Point and Maps|Program Entry Point and Maps]]
- [[_COMMUNITY_Cemetery Form Designer|Cemetery Form Designer]]
- [[_COMMUNITY_LoginForm Designer|LoginForm Designer]]
- [[_COMMUNITY_Database Setup PowerShell|Database Setup PowerShell]]
- [[_COMMUNITY_AnbiyamPopup Designer|AnbiyamPopup Designer]]
- [[_COMMUNITY_CemeterySubscription Designer|CemeterySubscription Designer]]
- [[_COMMUNITY_ProgressForm Designer|ProgressForm Designer]]
- [[_COMMUNITY_Program.cs Bootstrap|Program.cs Bootstrap]]
- [[_COMMUNITY_Performance Analysis|Performance Analysis]]
- [[_COMMUNITY_DatabaseHelper Access|DatabaseHelper Access]]
- [[_COMMUNITY_SubscriptionPopup Designer|SubscriptionPopup Designer]]
- [[_COMMUNITY_Claude Settings Permissions|Claude Settings Permissions]]
- [[_COMMUNITY_Claude Local Settings|Claude Local Settings]]
- [[_COMMUNITY_Form1 Stub|Form1 Stub]]
- [[_COMMUNITY_PdfExportExample Stub|PdfExportExample Stub]]
- [[_COMMUNITY_Claude Settings Files|Claude Settings Files]]
- [[_COMMUNITY_Delete Icon|Delete Icon]]
- [[_COMMUNITY_Edit Icon|Edit Icon]]
- [[_COMMUNITY_Add Icon|Add Icon]]

## God Nodes (most connected - your core abstractions)
1. `Form1` - 76 edges
2. `AppTheme` - 22 edges
3. `SubscriptionPopup` - 22 edges
4. `ModernPdfGenerator` - 21 edges
5. `EnhancedPdfGenerator` - 18 edges
6. `FamilyPopup` - 17 edges
7. `Form1` - 16 edges
8. `NonParishCemeterySubscriptionPopup` - 16 edges
9. `CemeterySubscriptionPopup` - 14 edges
10. `Cemetery` - 13 edges

## Surprising Connections (you probably didn't know these)
- `SubscriptionPopup Form` --semantically_similar_to--> `CemeterySubscriptionPopup Form`  [INFERRED] [semantically similar]
  SubscriptionPopup.cs → CemeterySubscriptionPopup.cs
- `CemeterySubscriptionPopup Form` --semantically_similar_to--> `NonParishCemeterySubscriptionPopup Form`  [INFERRED] [semantically similar]
  CemeterySubscriptionPopup.cs → NonParishCemeterySubscriptionPopup.cs
- `FamilyPopup Form` --semantically_similar_to--> `NonParishFamily Form`  [INFERRED] [semantically similar]
  FamilyPopup.cs → NonParishFamily.cs
- `EnhancedPdfGenerator` --references--> `PdfColor`  [EXTRACTED]
  EnhancedPdfGenerator.cs → ModernPdfGenerator.cs
- `EnhancedPdfGenerator` --references--> `PdfStandardFont`  [EXTRACTED]
  EnhancedPdfGenerator.cs → ModernPdfGenerator.cs

## Hyperedges (group relationships)
- **Subscription Popup Forms** — SubscriptionPopup_SubscriptionPopup, CemeterySubscriptionPopup_CemeterySubscriptionPopup, NonParishCemeterySubscriptionPopup_NonParishCemeterySubscriptionPopup [INFERRED 0.90]
- **Forms using AppTheme/ThemedDialog** — AppTheme_AppTheme, ThemedDialog_ThemedDialog, Form1_Form1, FamilyPopup_FamilyPopup, AnbiyamPopup_AnbiyamPopup [INFERRED 0.85]
- **DatabaseHelper Consumers** — DatabaseHelper_DatabaseHelper, Form1_Form1, FamilyPopup_FamilyPopup, SubscriptionPopup_SubscriptionPopup, CemeterySubscriptionPopup_CemeterySubscriptionPopup, AnbiyamPopup_AnbiyamPopup, Cemetery_Cemetery, NonParishFamily_NonParishFamily, LoginForm_LoginForm [EXTRACTED 1.00]
- **PDF Generation Pipeline (SP -> DataTable -> DTO -> PDF)** —  [INFERRED 0.95]
- **Schema migration sequence: Initial -> Master -> V4** —  [INFERRED 0.95]
- **Family core schema cluster** —  [INFERRED 0.95]
- **PDF Modernization Documents** —  [INFERRED]
- **Deployment Documents** —  [INFERRED]
- **Summary / Reference Docs** —  [INFERRED]
- **CRUD UI Action Icons** — icon_delete, icon_pencil, icon_plus [INFERRED 0.85]

## Communities (46 total, 17 thin omitted)

### Community 0 - "Database Schema and PDF Stored Procs"
Cohesion: 0.07
Nodes (40): CemeteryDetailData DTO, DatabaseHelper (referenced), EnhancedPdfGenerator (static class), FamilyReportData DTO, Initial_ddl_Scripts.sql (schema DDL), Initial_dml_Scripts.sql (seed DML), Initial_dummy_data_Scripts.sql (test data), Master_Installation_Script.sql (idempotent installer) (+32 more)

### Community 1 - "Form Designer Base Types"
Cohesion: 0.07
Nodes (10): AnbiyamPopup, TestFat, Cemetery, TestFat, NonParishFamily, TestFat, ProgressForm, TestFat (+2 more)

### Community 2 - "Modern PDF Generator"
Cohesion: 0.13
Nodes (9): CemeteryDetailData, FamilyReportData, MemberData, ModernPdfGenerator, SubscriptionData, TestFat, TimelineEventData, PdfColor (+1 more)

### Community 3 - "Form1 Grid Event Handlers"
Cohesion: 0.10
Nodes (6): bool, DateTime, Form1, object, Point, string

### Community 4 - "Application Architecture Overview"
Cohesion: 0.16
Nodes (24): AnbiyamPopup Form, AnbiyamPopup Designer, AppTheme (Styling), CemeterySubscriptionPopup Form, CemeterySubscriptionPopup Designer, Cemetery Form, Cemetery Designer, DatabaseHelper (DB Access) (+16 more)

### Community 5 - "AppTheme Styling"
Cohesion: 0.09
Nodes (4): Color, AppTheme, TestFat, Font

### Community 7 - "FamilyPopup Logic"
Cohesion: 0.13
Nodes (3): FamilyMemberDto, FamilyPopup, TestFat

### Community 8 - "PDF Modernization Docs"
Cohesion: 0.16
Nodes (15): Cemetery Records, PDF Cover Page, EnhancedPdfGenerator, Family Records, Our Lady of Fatima Church, Tambaram, ModernPdfGenerator, PDF Export Stored Procedures, Subscription (+7 more)

### Community 9 - "Form Layout Controls"
Cohesion: 0.16
Nodes (9): CheckBox, DateTimePicker, FamilyPopup, TestFat, NonParishFamily, TestFat, FlowLayoutPanel, GroupBox (+1 more)

### Community 10 - "Subscription Popup"
Cohesion: 0.21
Nodes (3): SubscriptionPopup, TestFat, ToggleButton

### Community 11 - "Deployment Documentation"
Cohesion: 0.18
Nodes (11): Idempotent Database Deployment, Inno Setup Installer, Master_Installation_Script.sql, Complete Deployment Ready Summary, Deployment Checklist, Deployment Summary, Idempotent Deployment Summary, Installation Guide (+3 more)

### Community 13 - "Non-Parish Cemetery Subscription"
Cohesion: 0.24
Nodes (4): ComboBox, DataGridView, NonParishCemeterySubscriptionPopup, TestFat

### Community 15 - "Form1 Dashboard Designer"
Cohesion: 0.20
Nodes (7): Chart, Form1, TestFat, TabControl, TableLayoutPanel, TabPage, WebBrowser

### Community 18 - "Cemetery Subscription Popup"
Cohesion: 0.28
Nodes (3): CemeterySubscriptionPopup, TestFat, NumericUpDown

### Community 23 - "Program Entry Point and Maps"
Cohesion: 0.29
Nodes (6): AssemblyInfo, Form1 (main form, referenced), LoginForm (referenced from Program.Main), Program (entry point), anbiyam_map.html (Leaflet map), IE11 Browser Emulation registry hack for Leaflet WebBrowser

### Community 24 - "Cemetery Form Designer"
Cohesion: 0.29
Nodes (4): BackgroundWorker, Button, Cemetery, TestFat

### Community 25 - "LoginForm Designer"
Cohesion: 0.29
Nodes (4): LoginForm, TestFat, PictureBox, TextBox

### Community 26 - "Database Setup PowerShell"
Cohesion: 0.57
Nodes (5): Create-Database(), Execute-SqlScript(), Write-Error-Custom(), Write-Info(), Write-Success()

### Community 27 - "AnbiyamPopup Designer"
Cohesion: 0.33
Nodes (3): AnbiyamPopup, TestFat, Label

### Community 29 - "CemeterySubscription Designer"
Cohesion: 0.40
Nodes (3): CemeterySubscriptionPopup, TestFat, IContainer

### Community 32 - "Performance Analysis"
Cohesion: 0.67
Nodes (4): Family Grid Performance (60s issue), Index Optimization, N+1 SQL Query Pattern, Performance Analysis

## Knowledge Gaps
- **95 isolated node(s):** `TestFat`, `TestFat`, `TestFat`, `Color`, `Font` (+90 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **17 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `Form1` connect `Form1 Grid Event Handlers` to `Form Designer Base Types`, `Form1 Filter Panel Builders`, `Form1 Stub`, `Form1 Family Search and CRUD`, `Form1 Dashboard Designer`, `Form1 Theming and Layout`, `Form1 Charts and Maps`, `Form1 Subscription Tab`, `Cemetery Form Designer`, `AnbiyamPopup Designer`, `AnbiyamPopup Logic`?**
  _High betweenness centrality (0.140) - this node is a cross-community bridge._
- **Why does `SubscriptionPopup` connect `Subscription Popup` to `Form Designer Base Types`, `Form Layout Controls`, `Cemetery Subscription Popup`, `Cemetery Form Designer`, `LoginForm Designer`, `AnbiyamPopup Designer`?**
  _High betweenness centrality (0.044) - this node is a cross-community bridge._
- **Why does `Label` connect `AnbiyamPopup Designer` to `Form1 Grid Event Handlers`, `Form Layout Controls`, `Subscription Popup`, `Non-Parish Cemetery Subscription`, `Form1 Dashboard Designer`, `Cemetery Subscription Popup`, `Cemetery Form Designer`, `LoginForm Designer`?**
  _High betweenness centrality (0.038) - this node is a cross-community bridge._
- **What connects `TestFat`, `TestFat`, `TestFat` to the rest of the system?**
  _96 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `Database Schema and PDF Stored Procs` be split into smaller, more focused modules?**
  _Cohesion score 0.06767676767676768 - nodes in this community are weakly interconnected._
- **Should `Form Designer Base Types` be split into smaller, more focused modules?**
  _Cohesion score 0.07407407407407407 - nodes in this community are weakly interconnected._
- **Should `Modern PDF Generator` be split into smaller, more focused modules?**
  _Cohesion score 0.12698412698412698 - nodes in this community are weakly interconnected._