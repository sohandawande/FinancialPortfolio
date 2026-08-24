export type MutualFundSchemeType = 1 | 2 | 3 | 4 | 5;
export type DepositInterestType = 1 | 2;
export type DepositStatus = 1 | 2 | 3;

export interface MutualFund {
  id: number;
  schemeName: string;
  amc: string;
  folioNumber?: string | null;
  schemeCode?: number | null;
  navAsOf?: string | null;
  navSource?: string | null;
  schemeType: MutualFundSchemeType;
  units: number;
  averageNav: number;
  currentNav: number;
  investedAmount: number;
  currentValue: number;
  gainLoss: number;
  gainLossPercent: number;
  purchaseDate: string;
  notes?: string | null;
  isActive: boolean;
}

export interface FixedDeposit {
  id: number;
  bankName: string;
  accountRef?: string | null;
  principal: number;
  interestRate: number;
  tenureMonths: number;
  interestType: DepositInterestType;
  startDate: string;
  maturityDate: string;
  maturityAmount: number;
  currentValue: number;
  accruedInterest: number;
  status: DepositStatus;
  notes?: string | null;
}

export interface RecurringDeposit {
  id: number;
  bankName: string;
  accountRef?: string | null;
  monthlyAmount: number;
  interestRate: number;
  tenureMonths: number;
  installmentsPaid: number;
  startDate: string;
  maturityDate: string;
  investedAmount: number;
  currentValue: number;
  maturityAmount: number;
  status: DepositStatus;
  notes?: string | null;
}

export interface WealthBucket {
  key: string;
  label: string;
  invested: number;
  currentValue: number;
  gainLoss: number;
  allocationPercent: number;
  count: number;
}

export interface WealthSummary {
  totalInvested: number;
  totalCurrentValue: number;
  totalGainLoss: number;
  totalGainLossPercent: number;
  buckets: WealthBucket[];
  mutualFunds: MutualFund[];
  fixedDeposits: FixedDeposit[];
  recurringDeposits: RecurringDeposit[];
}

export interface MutualFundSchemeLookup {
  schemeCode: number;
  schemeName: string;
  amc?: string | null;
  source: string;
}

export interface MutualFundNavSync {
  updated: number;
  failed: number;
  skipped: number;
  primarySource: string;
  fallbackSource?: string | null;
  errors: string[];
}

export interface UpsertMutualFundRequest {
  schemeName: string;
  amc: string;
  folioNumber?: string | null;
  schemeCode?: number | null;
  schemeType: MutualFundSchemeType;
  units: number;
  averageNav: number;
  currentNav: number;
  purchaseDate: string;
  notes?: string | null;
  isActive: boolean;
}

export interface UpsertFixedDepositRequest {
  bankName: string;
  accountRef?: string | null;
  principal: number;
  interestRate: number;
  tenureMonths: number;
  interestType: DepositInterestType;
  startDate: string;
  notes?: string | null;
  status: DepositStatus;
}

export interface UpsertRecurringDepositRequest {
  bankName: string;
  accountRef?: string | null;
  monthlyAmount: number;
  interestRate: number;
  tenureMonths: number;
  installmentsPaid: number;
  startDate: string;
  notes?: string | null;
  status: DepositStatus;
}

export const SCHEME_LABELS: Record<number, string> = {
  1: 'Equity',
  2: 'Debt',
  3: 'Hybrid',
  4: 'Index',
  5: 'Other',
};

export const STATUS_LABELS: Record<number, string> = {
  1: 'Active',
  2: 'Matured',
  3: 'Closed',
};
