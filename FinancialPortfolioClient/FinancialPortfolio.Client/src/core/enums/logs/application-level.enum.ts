/** Display / filter values — match server ApplicationLevelNames */
export const ApplicationLevel = {
  Api: 'FinancialPortfolio.Api',
  Business: 'FinancialPortfolio.Business',
  Data: 'FinancialPortfolio.Data',
  Models: 'FinancialPortfolio.Models',
  QueryEngine: 'FinancialPortfolio.QueryEngine',
  FrontendClient: 'FinancialPortfolio.Client',
} as const;

export type ApplicationLevel =
  (typeof ApplicationLevel)[keyof typeof ApplicationLevel];