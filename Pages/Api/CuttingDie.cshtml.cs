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
            /// <summary>
            /// Die Identifier
            /// </summary>
            public string StnsRef { get; set; } = string.Empty;
            /// <summary>
            /// Die Description (size)
            /// </summary>
            public string StnsOms { get; set; } = string.Empty;
            /// <summary>
            /// Die Radius
            /// </summary>
            public double Radius__ { get; set; } = 0;
            /// <summary>
            /// Across
            /// </summary>
            public double etiket_b { get; set; } = 0;
            /// <summary>
            /// Around
            /// </summary>
            public double etiket_h { get; set; } = 0;
            /// <summary>
            /// Step / Circumfrence
            /// </summary>
            public double omtrek__ { get; set; } = 0;
            /// <summary>
            /// Web Label Availability
            /// </summary>
            public string weblabel { get; set; } = string.Empty;
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
                // materie_ = 1 for "rotary", materie_ = 2 for "digital"
                int materie = 2; // Default to digital
                string printingLower = printing.ToLowerInvariant();
                
                if (printingLower.Contains("flexo"))
                {
                    materie = 1;
                    _logger.LogInformation("Printing option contains 'flexo' (rotary die type) - using materie_ = 1");
                }
                else if (printingLower.Contains("digital"))
                {
                    materie = 2;
                    _logger.LogInformation("Printing option contains 'digital' - using materie_ = 2");
                }
                else
                {
                    _logger.LogWarning("Printing option '{Printing}' does not contain 'rotary' or 'digital' - defaulting to materie_ = 2 (digital)", printing);
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
                    var sql = "SELECT stns_ref, stns_oms, etiket_b, etiket_h, radius__, omtrek__, weblabel FROM stnspr__ WHERE materie_ = @materie AND weblabel = 'Y' AND stns_oms IS NOT NULL AND stns_oms <> '' AND stns_oms <> 'UNKOWN' ORDER BY stns_oms DESC";
                    
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
                                int etiket_bOrdinal = reader.GetOrdinal("etiket_b");
                                int etiket_hOrdinal = reader.GetOrdinal("etiket_h");
                                int Radius__Ordinal = reader.GetOrdinal("Radius__");
                                int omtrek__Ordinal = reader.GetOrdinal("omtrek__");
                                int weblabelOrdinal = reader.GetOrdinal("weblabel");

                                while (await reader.ReadAsync())
                                {
                                    CuttingDieOption cuttingDieOption = new CuttingDieOption();
                                    cuttingDieOption.StnsRef = reader.IsDBNull(stnsRefOrdinal) ? string.Empty : reader.GetString(stnsRefOrdinal);
                                    cuttingDieOption.StnsOms = reader.IsDBNull(stnsOmsOrdinal) ? string.Empty : reader.GetString(stnsOmsOrdinal);
                                    cuttingDieOption.etiket_b = reader.IsDBNull(etiket_bOrdinal) ? 0 : reader.GetDouble(etiket_bOrdinal);
                                    cuttingDieOption.etiket_h = reader.IsDBNull(etiket_hOrdinal) ? 0 : reader.GetDouble(etiket_hOrdinal);
                                    cuttingDieOption.Radius__ = reader.IsDBNull(Radius__Ordinal) ? 0 : reader.GetDouble(Radius__Ordinal);
                                    cuttingDieOption.omtrek__ = reader.IsDBNull(omtrek__Ordinal) ? 0 : reader.GetDouble(omtrek__Ordinal);
                                    cuttingDieOption.weblabel = reader.IsDBNull(weblabelOrdinal) ? string.Empty : reader.GetString(weblabelOrdinal);

                                    results.Add(cuttingDieOption);

                                    //results.Add(new CuttingDieOption
                                    //{
                                    //    StnsRef = reader.IsDBNull(stnsRefOrdinal) ? string.Empty : reader.GetString(stnsRefOrdinal),
                                    //    StnsOms = reader.IsDBNull(stnsOmsOrdinal) ? string.Empty : reader.GetString(stnsOmsOrdinal),
                                    //    etiket_b = reader.IsDBNull(etiket_bOrdinal) ? string.Empty : reader.GetString(etiket_bOrdinal),
                                    //    etiket_h = reader.IsDBNull(etiket_hOrdinal) ? string.Empty : reader.GetString(etiket_hOrdinal),
                                    //    Radius__ = reader.IsDBNull(Radius__Ordinal) ? string.Empty : reader.GetString(Radius__Ordinal),
                                    //    omtrek__ = reader.IsDBNull(omtrek__Ordinal) ? string.Empty : reader.GetString(omtrek__Ordinal),
                                    //    weblabel = reader.IsDBNull(weblabelOrdinal) ? string.Empty : reader.GetString(weblabelOrdinal)
                                    //});
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

