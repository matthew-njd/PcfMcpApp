using System.ComponentModel;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using PcfMcpApp.Api.Data;


namespace PcfMcpApp.Api.Tools
{
    [McpServerToolType]
    public class SalesTools(ApplicationDbContext db)
    {
        [McpServerTool]
        [Description(
            "Searches for customers by name. Use this first whenever the user refers to a customer by name " +
            "so you can resolve their ID before calling any other tool. Returns matching customer IDs, names, and emails.")]
        public async Task<string> SearchCustomers(
            [Description("Partial or full customer name to search for (case-insensitive)")] string nameQuery)
        {
            var matches = await db.Customers
                .AsNoTracking()
                .Where(c => c.Name.ToLower().Contains(nameQuery.ToLower()))
                .ToListAsync();

            if (matches.Count == 0)
                return $"No customers found matching '{nameQuery}'.";

            var sb = new StringBuilder();
            sb.AppendLine($"Found {matches.Count} customer(s) matching '{nameQuery}':");
            foreach (var c in matches)
                sb.AppendLine($"  - Id: {c.Id} | Name: {c.Name} | Email: {c.Email}");

            return sb.ToString();
        }

        [McpServerTool]
        [Description(
            "Returns sales transactions for a specific customer, optionally filtered by date range. " +
            "Includes each sale's amount and date, plus a total summary. " +
            "If no date range is provided, returns all sales for the customer.")]
        public async Task<string> GetSalesForCustomer(
            [Description("The unique ID of the customer (use SearchCustomers first to resolve a name to an ID)")] int customerId,
            [Description("Optional start date (ISO format, e.g. 2024-01-01). Omit to include all historical sales.")] DateTime? dateFrom,
            [Description("Optional end date (ISO format, e.g. 2024-12-31). Omit to include up to today.")] DateTime? dateTo)
        {
            var customer = await db.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == customerId);
            if (customer is null)
                return $"No customer found with ID {customerId}.";

            var from = dateFrom ?? DateTime.MinValue;
            var to = dateTo ?? DateTime.Now;

            var sales = await db.Sales
                .AsNoTracking()
                .Where(s => s.CustomerId == customerId && s.SaleDate >= from && s.SaleDate <= to)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();

            if (sales.Count == 0)
                return $"No sales found for {customer.Name}" +
                       (dateFrom.HasValue || dateTo.HasValue ? $" in the specified date range ({from:d} to {to:d})." : ".");

            var total = sales.Sum(s => s.Amount);
            var sb = new StringBuilder();
            sb.AppendLine($"Sales for {customer.Name}" +
                          (dateFrom.HasValue || dateTo.HasValue ? $" from {from:d} to {to:d}" : " (all time)") + ":");
            foreach (var s in sales)
                sb.AppendLine($"  - {s.SaleDate:d}: {s.Amount:C}");
            sb.AppendLine($"  Total: {total:C} across {sales.Count} sale(s).");

            return sb.ToString();
        }

        [McpServerTool]
        [Description(
            "Returns the top customers ranked by total sales revenue, optionally filtered by date range. " +
            "Useful for questions like 'who are our best customers this year?' or 'who spent the most last quarter?'")]
        public async Task<string> GetTopCustomers(
            [Description("Optional start date (ISO format). Omit to include all historical sales.")] DateTime? dateFrom,
            [Description("Optional end date (ISO format). Omit to include up to today.")] DateTime? dateTo,
            [Description("Number of top customers to return (default is 5)")] int topN = 5)
        {
            var from = dateFrom ?? DateTime.MinValue;
            var to = dateTo ?? DateTime.Now;

            var results = await db.Sales
                .AsNoTracking()
                .Where(s => s.SaleDate >= from && s.SaleDate <= to)
                .GroupBy(s => s.CustomerId)
                .Select(g => new { CustomerId = g.Key, Total = g.Sum(s => s.Amount), Count = g.Count() })
                .OrderByDescending(g => g.Total)
                .Take(topN)
                .ToListAsync();

            if (results.Count == 0)
                return "No sales data found for the specified period.";

            var customerIds = results.Select(r => r.CustomerId).ToList();
            var customers = await db.Customers
                .AsNoTracking()
                .Where(c => customerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);

            var sb = new StringBuilder();
            sb.AppendLine($"Top {results.Count} customer(s) by revenue" +
                          (dateFrom.HasValue || dateTo.HasValue ? $" from {from:d} to {to:d}" : " (all time)") + ":");

            for (int i = 0; i < results.Count; i++)
            {
                var r = results[i];
                var name = customers.TryGetValue(r.CustomerId, out var c) ? c.Name : $"Customer {r.CustomerId}";
                sb.AppendLine($"  {i + 1}. {name}: {r.Total:C} across {r.Count} sale(s)");
            }

            return sb.ToString();
        }

        [McpServerTool]
        [Description(
            "Returns an overall sales summary across all customers for a given period. " +
            "Useful for high-level business questions like 'how did we do this month?' or 'what is our total revenue this year?'")]
        public async Task<string> GetSalesSummary(
            [Description("Optional start date (ISO format). Omit to include all historical sales.")] DateTime? dateFrom,
            [Description("Optional end date (ISO format). Omit to include up to today.")] DateTime? dateTo)
        {
            var from = dateFrom ?? DateTime.MinValue;
            var to = dateTo ?? DateTime.Now;

            var sales = await db.Sales
                .AsNoTracking()
                .Where(s => s.SaleDate >= from && s.SaleDate <= to)
                .ToListAsync();

            if (sales.Count == 0)
                return "No sales found for the specified period.";

            var totalRevenue = sales.Sum(s => s.Amount);
            var totalTransactions = sales.Count;
            var avgSaleValue = totalRevenue / totalTransactions;
            var uniqueCustomers = sales.Select(s => s.CustomerId).Distinct().Count();
            var largestSale = sales.Max(s => s.Amount);
            var smallestSale = sales.Min(s => s.Amount);

            var sb = new StringBuilder();
            sb.AppendLine($"Sales summary" +
                          (dateFrom.HasValue || dateTo.HasValue ? $" from {from:d} to {to:d}" : " (all time)") + ":");
            sb.AppendLine($"  Total revenue:       {totalRevenue:C}");
            sb.AppendLine($"  Total transactions:  {totalTransactions}");
            sb.AppendLine($"  Unique customers:    {uniqueCustomers}");
            sb.AppendLine($"  Average sale value:  {avgSaleValue:C}");
            sb.AppendLine($"  Largest sale:        {largestSale:C}");
            sb.AppendLine($"  Smallest sale:       {smallestSale:C}");

            return sb.ToString();
        }

        [McpServerTool]
        [Description(
            "Returns the most recent sales transactions, optionally filtered to a specific customer. " +
            "Useful for questions like 'show me the latest sales', 'what has Acme bought recently?', or 'when did Wayne Technologies last make a purchase?'")]
        public async Task<string> GetRecentSales(
            [Description("Optional customer ID to filter by. Omit to get recent sales across all customers. Use SearchCustomers first to resolve a name to an ID.")] int? customerId,
            [Description("Number of recent sales to return (default is 10)")] int limit = 10)
        {
            var query = db.Sales.AsNoTracking().AsQueryable();

            if (customerId.HasValue)
                query = query.Where(s => s.CustomerId == customerId.Value);

            var sales = await query
                .OrderByDescending(s => s.SaleDate)
                .Take(limit)
                .ToListAsync();

            if (sales.Count == 0)
                return customerId.HasValue
                    ? $"No recent sales found for customer ID {customerId}."
                    : "No sales found.";

            var customerIds = sales.Select(s => s.CustomerId).Distinct().ToList();
            var customers = await db.Customers
                .AsNoTracking()
                .Where(c => customerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id);

            var sb = new StringBuilder();
            sb.AppendLine($"Most recent {sales.Count} sale(s)" +
                          (customerId.HasValue && customers.TryGetValue(customerId.Value, out var fc)
                              ? $" for {fc.Name}"
                              : " across all customers") + ":");

            foreach (var s in sales)
            {
                var name = customers.TryGetValue(s.CustomerId, out var c) ? c.Name : $"Customer {s.CustomerId}";
                sb.AppendLine($"  - {s.SaleDate:d}: {name} — {s.Amount:C}");
            }

            return sb.ToString();
        }
    }
}