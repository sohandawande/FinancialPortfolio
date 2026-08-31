export type MutualFundSchemeType = 1 | 2 | 3 | 4 | 5;
export type DepositInterestType = 1 | 2;
export type DepositStatus = 1 | 2 | 3;
export type RdInstallmentStatus = 1 | 2 | 3 | 4 | 5;
export type RdPaymentMode = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8;

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

export interface RdInstallment {
  id: number;
  recurringDepositId: number;
  installmentNumber: number;
  dueDate: string;
  paidDate?: string | null;
  amount: number;
  fromBankName?: string | null;
  fromAccountNumber?: string | null;
  fromIfsc?: string | null;
  transactionReference?: string | null;
  paymentMode: RdPaymentMode;
  status: RdInstallmentStatus;
  penaltyAmount?: number | null;
  notes?: string | null;
}

export interface RecurringDeposit {
  id: number;
  bankName: string;
  bankIfsc?: string | null;
  accountRef?: string | null;
  linkedAccountNumber?: string | null;
  linkedIfsc?: string | null;
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
  installments?: RdInstallment[];
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
  bankIfsc?: string | null;
  accountRef?: string | null;
  linkedAccountNumber?: string | null;
  linkedIfsc?: string | null;
  monthlyAmount: number;
  interestRate: number;
  tenureMonths: number;
  installmentsPaid: number;
  startDate: string;
  notes?: string | null;
  status: DepositStatus;
}

export interface PayRdInstallmentRequest {
  installmentNumber?: number | null;
  paidDate?: string | null;
  amount?: number | null;
  fromBankName?: string | null;
  fromAccountNumber?: string | null;
  fromIfsc?: string | null;
  transactionReference?: string | null;
  paymentMode?: RdPaymentMode;
  penaltyAmount?: number | null;
  notes?: string | null;
}

export interface BankIfscInfo {
  ifsc: string;
  bank: string;
  bankCode?: string | null;
  branch?: string | null;
  address?: string | null;
  city?: string | null;
  district?: string | null;
  state?: string | null;
  contact?: string | null;
  micr?: string | null;
  rtgs?: boolean | null;
  neft?: boolean | null;
  imps?: boolean | null;
  upi?: boolean | null;
}

export interface BankSuggestion {
  name: string;
  code?: string | null;
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

export const RD_INSTALLMENT_STATUS_LABELS: Record<number, string> = {
  1: 'Pending',
  2: 'Paid',
  3: 'Missed',
  4: 'Partial',
  5: 'Waived',
};

export const RD_PAYMENT_MODE_LABELS: Record<number, string> = {
  1: 'Auto debit',
  2: 'NEFT',
  3: 'RTGS',
  4: 'IMPS',
  5: 'UPI',
  6: 'Cash',
  7: 'Cheque',
  8: 'Other',
};
