# Fatima Church – AWS Cost Estimate & Migration Challenges

**Date:** June 2026  
**Scope:** React (S3 + CloudFront) → API Gateway → Lambda → RDS SQL Server  
**Region:** ap-south-1 (Mumbai) — closest to Chennai, lowest latency  
**SSL Note:** No purchased certificates needed — covered below.

---

## 1. SSL / TLS Certificates — Zero Cost

This is good news. **No certificate purchase is required at all.**

| Entry Point | Default HTTPS URL | Certificate |
|---|---|---|
| Frontend (CloudFront) | `https://d1xxxxx.cloudfront.net` | Built-in — free |
| API Gateway | `https://xxxxxxxx.execute-api.ap-south-1.amazonaws.com` | Built-in — free |

If a custom domain is ever needed (e.g., `app.fatimaschurch.com`), **AWS Certificate Manager (ACM)** issues certificates completely free of charge for use with CloudFront and API Gateway. ACM auto-renews them — no manual renewal, no annual fee. The only requirement is owning a domain name.

**Bottom line:** No certificate cost, now or later.

---

## 2. Monthly AWS Cost Estimate

Usage assumptions for a small parish (~200–300 families, ~5–10 staff users):
- ~50,000 API calls/month
- ~2–5 GB CloudFront data transfer/month
- Database size well under 10 GB

### Service-by-Service Breakdown

| # | AWS Service | Configuration | Est. Cost/month |
|---|---|---|---|
| 1 | **Amazon S3** | Static React app hosting (~50 MB) | < ₹5 / $0.05 |
| 2 | **CloudFront** | 2 GB transfer, HTTPS, no custom cert | ~$2.00 |
| 3 | **API Gateway** | REST, ~50K requests/month | ~$0.20 |
| 4 | **Lambda** | Node.js 20.x, ~50K invocations, 256 MB | **$0** (within free tier) |
| 5 | **RDS SQL Server Express** | db.t3.micro, 20 GB GP2 storage | ~$22.00 |
| 6 | **AWS Secrets Manager** | 3 secrets (DB creds, JWT keys) | ~$1.20 |
| 7 | **VPC / Networking** | Private subnets, no NAT Gateway needed | $0 |
| 8 | **CloudWatch Logs** | Basic Lambda + API Gateway logs | ~$0.50 |
| | **Total (Minimum)** | | **~$26/month** |

### RDS SQL Server — Edition Comparison

| Edition | Limit | db.t3.micro/month | Recommended For |
|---|---|---|---|
| **Express** (free license) | 10 GB DB, 1 GB RAM | ~$22 | ✅ Small parish — fits easily |
| Web | No DB size limit, 1 GB RAM | ~$70 | Medium load |
| Standard | Full features, HA | ~$250+ | Enterprise |

**Recommendation:** Start with **Express**. The entire parish dataset (families, members, subscriptions, cemeteries) will comfortably fit within 10 GB. Upgrading editions later is a single RDS parameter change.

### Free Tier Safety Net

AWS Free Tier (first 12 months) covers:
- Lambda: 1M requests + 400K GB-seconds/month — **fully free**
- RDS: 750 hours db.t3.micro/month — **covers Express fully for year 1**
- S3: 5 GB storage, 20K GET, 2K PUT — **sufficient for static hosting**

**If on free tier:** Effective cost is closer to **~$4–5/month** (just CloudFront + Secrets Manager).

---

## 3. Challenges

### 3.1 SSL / Domain (Low Risk)

| Challenge | Detail |
|---|---|
| No custom domain | Default CloudFront and API Gateway URLs work fine with HTTPS. URLs will look like `d1xxxxx.cloudfront.net` — functional but not branded. |
| Custom domain later | If a domain is obtained (even a free subdomain), ACM certificate is issued for free. No code changes needed — just a CloudFront alias + DNS CNAME. |

### 3.2 Database Migration (Medium Risk)

| Challenge | Detail |
|---|---|
| Local SQL Server → RDS | Need to take a `.bak` backup of the production `fatimachurchtbm` database and restore it to RDS SQL Server Express. This is a one-time operation. |
| SQL Server version matching | Must pick the same major version on RDS (e.g., SQL Server 2019) that the local instance uses. Check via `SELECT @@VERSION` on the local machine. |
| No schema changes to prod | V4 DDL scripts (new indexes, new table `nonparish_cemetery_subscription`) must be tested on a staging copy before running on the restored RDS. |
| Dynamic SQL in SPs | `sp_SetFamilySubscriptionMonth` uses `sp_executesql`. Works identically on RDS — no change needed. |
| Inline auth query (LoginForm) | SHA-256 auth uses inline SQL (`HASHBYTES`). This will be converted to a stored procedure `sp_AuthenticateUser` — same logic, just wrapped in an SP to keep Lambda code clean. |

### 3.3 Lambda in VPC — Cold Starts (Medium Risk)

| Challenge | Detail |
|---|---|
| VPC required for RDS | Lambda must be placed inside the same VPC as RDS (private subnet). This adds ~300–800 ms cold start on first invocation. |
| Mitigation | Enable **Provisioned Concurrency** for the auth Lambda only (most latency-sensitive). All others can tolerate cold starts for a church-scale admin app. |
| NAT Gateway not needed | Lambda only calls RDS (internal VPC) and Secrets Manager (VPC endpoint available). No public internet access required from Lambda — saves ~$33/month on NAT Gateway. |

### 3.4 CORS Configuration (Low Risk)

API Gateway needs CORS enabled for requests from the CloudFront domain. This is a one-time configuration per endpoint. Must be set correctly for `Authorization` headers and `credentials: 'include'` (for HttpOnly JWT cookie).

### 3.5 SPA Routing on S3/CloudFront (Low Risk)

React uses client-side routing (React Router). S3 returns 403/404 on direct URL access (e.g., `app.com/families`). Fix: configure CloudFront custom error response — redirect all 403/404 to `index.html` with HTTP 200. One-time setup.

### 3.6 mssql Package in Lambda (Low Risk)

The Node.js `mssql` package (~15 MB with dependencies) must be bundled into each Lambda deployment package or published as a **Lambda Layer** shared across all functions. Using a Layer keeps individual function sizes small and allows updating the DB driver independently.

### 3.7 Concurrency & Connection Pooling (Medium Risk)

| Challenge | Detail |
|---|---|
| Lambda is stateless | Each Lambda invocation opens a new DB connection by default. At burst traffic, this can exhaust SQL Server Express connection limits (~100 connections). |
| Mitigation | Use **RDS Proxy** (adds ~$0.015/vCPU-hour = ~$8-10/month) to pool connections. For a small church app with 5–10 concurrent users, this is optional initially. |

### 3.8 No Authentication Service / Cognito (Low-Medium Risk)

The plan uses custom JWT (RS256) in HttpOnly cookies — no Cognito, no third-party auth. This keeps cost at $0 for auth but requires careful implementation:
- Token signing key stored in Secrets Manager
- Token expiry + refresh logic must be built manually
- No built-in MFA (can be added later)

### 3.9 Production Data Safety During Migration (High Importance)

| Step | Safeguard |
|---|---|
| Backup first | Full `.bak` before any RDS activity |
| Parallel run | Keep WinForms app pointing to local SQL Server; web app points to RDS until fully validated |
| Read-only Lambda first | Deploy APIs with SELECT-only DB user, validate all data reads before enabling write endpoints |
| No DDL in web app | Zero `ALTER TABLE` / `DROP` in any Lambda function — all DB interaction via SPs only |

---

## 4. Cost Summary

| Scenario | Monthly Cost |
|---|---|
| Free tier (first 12 months) | ~$4–5/month |
| After free tier (Express edition) | ~$26/month |
| After free tier (Web edition, optional upgrade) | ~$70/month |
| With RDS Proxy added | ~$36/month |

**No SSL/certificate cost — ever** (unless a custom domain is purchased, in which case ACM is still free; only the domain registrar charges apply, typically $10–15/year).

---

*Prepared June 2026 | Based on AWS ap-south-1 pricing | Subject to AWS pricing changes*
