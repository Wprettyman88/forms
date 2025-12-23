using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
using CERM.DataAccess;

namespace WiseLabels.Pages.Api
{
    [IgnoreAntiforgeryToken]
    public class CuttingDieModel : PageModel
    {
        private readonly CermDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CuttingDieModel> _logger;

        public CuttingDieModel(CermDbContext dbContext, IConfiguration configuration, ILogger<CuttingDieModel> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }

        public class CuttingDieOption
        {
            public string StnsRef { get; set; } = string.Empty;
            public string StnsOms { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(string? printing = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(printing))
                {
                    return new JsonResult(new { error = "Printing parameter is required" })
                    {
                        StatusCode = 400
                    };
                }

                // Determine materie_ value based on printing text
                // materie_ = 1 for "rotary" or "flexo", materie_ = 2 for "digital"
                int materie = 2; // Default to digital
                string printingLower = printing.ToLowerInvariant();
                
                if (printingLower.Contains("rotary") || printingLower.Contains("flexo"))
                {
                    materie = 1;
                    string matchedTerm = printingLower.Contains("rotary") ? "rotary" : "flexo";
                    _logger.LogInformation("Printing option contains '{Term}' - using materie_ = 1", matchedTerm);
                }
                else if (printingLower.Contains("digital"))
                {
                    materie = 2;
                    _logger.LogInformation("Printing option contains 'digital' - using materie_ = 2");
                }
                else
                {
                    _logger.LogWarning("Printing option '{Printing}' does not contain 'rotary', 'flexo', or 'digital' - defaulting to materie_ = 2 (digital)", printing);
                }

                // Execute SQL query: SELECT stns_ref, stns_oms FROM stnspr__ WHERE materie_ = @materie
                var results = new List<CuttingDieOption>();

                try
                {
                    // Get connection string
                    var connectionString = _configuration.GetConnectionString("CermDatabase");
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        _logger.LogError("CermDatabase connection string not found in configuration");
                        return new JsonResult(new { error = "Database connection not configured" })
                        {
                            StatusCode = 500
                        };
                    }

                    // Use parameterized SQL query to prevent SQL injection
                    // Filter out rows where stns_oms is empty or NULL
                    var sql = "SELECT stns_ref, stns_oms FROM stnspr__ WHERE materie_ = @materie AND stns_oms IS NOT NULL AND stns_oms != ''";
                    
                    using (var connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        
                        using (var command = new SqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@materie", materie);
                            
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                int stnsRefOrdinal = reader.GetOrdinal("stns_ref");
                                int stnsOmsOrdinal = reader.GetOrdinal("stns_oms");
                                
                                while (await reader.ReadAsync())
                                {
                                    results.Add(new CuttingDieOption
                                    {
                                        StnsRef = reader.IsDBNull(stnsRefOrdinal) ? string.Empty : reader.GetString(stnsRefOrdinal),
                                        StnsOms = reader.IsDBNull(stnsOmsOrdinal) ? string.Empty : reader.GetString(stnsOmsOrdinal)
                                    });
                                }
                            }
                        }
                    }

                    _logger.LogInformation("Retrieved {Count} cutting die options for materie_ = {Materie}", results.Count, materie);
                }
                catch (Exception dbEx)
                {
                    _logger.LogError(dbEx, "Database error while querying cutting die options for materie_ = {Materie}", materie);
                    return new JsonResult(new { error = $"Database error: {dbEx.Message}" })
                    {
                        StatusCode = 500
                    };
                }

                return new JsonResult(new { cuttingDieOptions = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cutting die options");
                return new JsonResult(new { error = $"Error loading cutting die options: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }
    }
}

