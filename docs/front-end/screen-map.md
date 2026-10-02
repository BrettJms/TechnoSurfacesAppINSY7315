# Screen map

The twenty screens from the Task 1 journey maps, who can use each one, and where each screen gets its data. This is the checklist for connecting the front end to the back end.

"Ready" means the data source is merged into `develop` today. The status column records the state of the repository on 2 October 2026 and should be updated as branches merge.

## Who can use each screen

Six screens are for the Managing Director only. They do not appear in an estimator's side menu, and an estimator who opens one by its address sees a read only page that says who it is for, not an error page. The wording of that note is agreed with Amaan, who owns the authorisation rules.

| Group | Screen | Managing Director | Estimator |
|---|---|---|---|
| Getting in | Sign in | Yes | Yes |
| | Forgot password | Yes | Yes |
| | Activate account | Yes | Yes |
| | Dashboard | Shows the approval queue | Shows their own quotes |
| Quoting | Quote list | Yes | Yes |
| | New quote | Yes | Yes |
| | Costing sheet | Edits any quote | Edits their own drafts only |
| | Customer quotation | Yes | Yes |
| | Approval queue | Yes | Managing Director only |
| | Review quote | Approves | Read only |
| | Version history | Yes | Yes |
| | Invoice record | Yes | Yes |
| Data | Material catalogue | Yes | Yes, including all prices |
| | Price editor | Edits | Managing Director only |
| | Customers | Yes | Yes |
| | Customer detail | Yes | Yes |
| Running the app | Rate card | Edits | Managing Director only |
| | Users | Yes | Managing Director only |
| | Quotation terms | Edits | Managing Director only |
| | Audit trail | Yes | Managing Director only |

Estimators can see every price, including cost prices. That is a decision the client confirmed, not an oversight.

## Where each screen gets its data

| Screen | What it needs | Comes from | Owner | Ready |
|---|---|---|---|---|
| Sign in | Credentials, lockout, deactivated accounts refused | ASP.NET Core Identity and `ISignInService` | Amaan | Yes |
| Forgot password | Single use, time limited reset token | User administration | Amaan | No |
| Activate account | First password for an account the Managing Director created | User administration | Amaan | No |
| Dashboard | Approval queue for the Managing Director, own quotes for an estimator | Quote workflow | Morgan | No |
| Quote list | Quotes filtered by status | Quote workflow | Morgan | No |
| New quote | Customer, contact, site, project, markup | Quote workflow | Morgan | No |
| Costing sheet | Supplier, product line, colour and sheet size lists; add, change and remove lines; totals | `/api/catalogue/*` and `/api/quotes/{id}/lines` endpoints | Morgan | No |
| Customer quotation | Quotation lines with no cost fields, standing terms, warranty by brand | Quotation generation | Morgan | No |
| Approval queue | Quotes waiting for approval | Approval workflow | Morgan | No |
| Review quote | Correct and approve in one step, the changes made to the quote | Approval workflow and the audit trail | Morgan, Amaan | No |
| Version history | Every version with its date, author and total | Quote versioning | Morgan | No |
| Invoice record | Pastel invoice number, date and amount | Invoice record | Morgan | No |
| Material catalogue | Suppliers, product lines, colours, sheet sizes and prices | The seeded catalogue | Kallan | Data yes, no read service yet |
| Price editor | Change a price from a date without overwriting the old one | `IPriceHistory.SetMaterialPriceAsync` | Kallan | No, on `feature/price-resolution` |
| Customers | Customer list | Customers and contacts | Morgan | No |
| Customer detail | A customer with its contacts | Customers and contacts | Morgan | No |
| Rate card | Rates, including the ones awaiting the client's figures | `IPriceHistory.SetRateAsync` and the seeded rate card | Kallan | Data yes, editing on `feature/price-resolution` |
| Users | List, create, deactivate, reactivate, reset password | User administration | Amaan | No |
| Quotation terms | Standing terms held in one place | Quotation generation | Morgan | No |
| Audit trail | Changes filtered by user, entity and date | Audit interceptor and user administration | Amaan | No, interceptor in pull request 9 |

Kallan's price resolution and calculation engine are merged and sit underneath the costing sheet. The screen reaches them through Morgan's endpoints.

## Stories the screens deliver

Only stories whose wording appears in the team's Task 2 files are listed with a meaning. The full text of every story is in the final Task 1 document.

| Story | What it means | Screen |
|---|---|---|
| US-01 | Choose supplier, product line, colour and sheet size in turn, and the price fills in without typing | Costing sheet |
| US-02 | See the sheet dimensions next to the price | Costing sheet |
| US-03 | A price that cannot be found blocks the line and is never shown as zero | Costing sheet |
| US-09 | Totals recalculate in full | Costing sheet |
| US-11 | No cost price, supplier discount or markup on the customer quotation | Customer quotation |
| US-12 | Standing terms come from one maintained place | Quotation terms, Customer quotation |
| US-15 | A quotation is addressed to a contact and billed to a customer | Customers, Customer quotation |
| US-16 | Submit a quote for approval | Costing sheet |
| US-17 | See the quotes waiting for approval | Approval queue |
| US-18 | Correct and approve in one step | Review quote |
| US-19 | An estimator can see that their figures were changed | Review quote, Audit trail |
| US-20 | Reopen a quote after a counter offer | Version history |
| US-21 | Each revision is a new version | Version history |
| US-23 | Only the Managing Director changes prices | Price editor, Rate card |
| US-25 | Record the Pastel invoice against an accepted quote | Invoice record |
| US-26 | No self registration, and a deactivated account cannot sign in | Sign in, Users |
| US-27 | No page can be reached without signing in | Every screen |

US-05 and US-28 are assigned to the front end, but their wording is not in the Task 2 files.
