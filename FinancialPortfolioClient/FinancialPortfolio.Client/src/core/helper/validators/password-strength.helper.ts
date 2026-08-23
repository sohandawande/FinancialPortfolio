export interface PasswordStrengthResult {
  score: number;
  label: 'Weak' | 'Fair' | 'Good' | 'Strong' | 'Empty';
  percent: number;
  hasMinLength: boolean;
  hasLetter: boolean;
  hasNumber: boolean;
  hasSymbol: boolean;
}

export function getPasswordStrength(password: string): PasswordStrengthResult {
  const value = password ?? '';

  const hasMinLength = value.length >= 8;
  const hasLetter = /[A-Za-z]/.test(value);
  const hasNumber = /[0-9]/.test(value);
  const hasSymbol = /[^A-Za-z0-9]/.test(value);

  if (!value) {
    return {
      score: 0,
      label: 'Empty',
      percent: 0,
      hasMinLength,
      hasLetter,
      hasNumber,
      hasSymbol,
    };
  }

  let score = 0;
  if (hasMinLength) score++;
  if (hasLetter) score++;
  if (hasNumber) score++;
  if (hasSymbol) score++;
  if (value.length >= 12) score = Math.min(4, score + 1);

  const labels: PasswordStrengthResult['label'][] = [
    'Weak',
    'Weak',
    'Fair',
    'Good',
    'Strong',
  ];

  return {
    score,
    label: labels[score],
    percent: (score / 4) * 100,
    hasMinLength,
    hasLetter,
    hasNumber,
    hasSymbol,
  };
}