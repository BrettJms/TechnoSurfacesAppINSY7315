# Quoting System for Techno Surfaces
A prototype for INSY7315: Information Systems 3E was created for Techno Surfaces (Pty) Ltd, a solid surface fabricator based in Cape Town with clients ranging from individual houses to 
national businesses such as Bootleggers, Vida e Caffè, and multiple airports in South Africa.

Techno Surfaces generates all of its quotes from a single Excel sheet and printed supplier pricing lists that are manually searched across twelve different worksheets. 
A price increase must be applied twelve times, and a mistyped search gives a wrong value rather than an error, which results in actual pricing errors. 
The purpose of this method is to make such errors obvious rather than invisible. 
The price of a material only shows up once its supplier, product line, colour, size, and thickness have all been selected.

This is the Task 1 prototype, which consists just of the front end and is constructed using ASP.NET Core MVC with Bootstrap. 
It needs a database and the back end logic. 
As planned for this part of the assignment, all data is seeded in memory when the application launches and nothing is stored in between searches.

Clone the repository, launch dotnet restore and then dotnet run from the project folder, and then open the local URL that appears. 
Three demo accounts, Paul Schluter as Managing Director and two estimators, Lerato Mokoena and Devan Naidoo, have simple access buttons on the login screen that don't require a password.

Signing in, a specific role dashboard, browsing and filtering quotes, creating a new quote using the colour to supplier material flow, the internal costing sheet, the client side quotation, the approval queue, 
version history for counteroffers and logging the resulting Pastel invoice reference are all covered by the app. 
The material catalogue, customer data, and admin screens for rates, users, quotation terms, and the audit trail are all included.

The client's actual supplier pricing lists are the source of the material costs, supplier codes, and sheet sizes. 
Since the client stated that the prices in their example workbook are not actual amounts and the actual rate card has not yet been verified, the labour and fabrication rates on the rate card are placeholders.

Built by Brett James (ST10440287), Kallan Jones (ST10445389), Morgan Gibbon
(ST10439398), Amaan Tesfaye (ST10287107) and Matteo Nusca (ST10440432)
