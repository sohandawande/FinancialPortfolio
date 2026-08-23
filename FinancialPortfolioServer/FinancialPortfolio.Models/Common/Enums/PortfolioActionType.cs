namespace FinancialPortfolio.Models.Common.Enums
{
    public enum StockExchange
    {
        NSE = 1,
        BSE = 2,
        Other = 99
    }

    public enum ProfitLoss
    {
        Profit = 1,
        Loss = 2,
        Equal = 3
    }

    public enum InvestmentAction
    {
        Hold = 1,
        Sell = 2
    }

    public enum LotStatus
    {
        Open = 1,
        PartiallySold = 2,
        FullySold = 3,
        Sold = 4          // For sell transaction rows
    }

    public enum PositionStatus
    {
        Holding = 1,
        FullySold = 2
    }
}
