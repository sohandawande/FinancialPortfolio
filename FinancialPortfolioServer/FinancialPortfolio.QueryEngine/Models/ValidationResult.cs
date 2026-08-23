using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialPortfolio.QueryEngine.Models
{
    public sealed class ValidationResult
    {
        public bool IsValid => !Errors.Any();

        public List<string> Errors { get; set; } = new();
    }
}
