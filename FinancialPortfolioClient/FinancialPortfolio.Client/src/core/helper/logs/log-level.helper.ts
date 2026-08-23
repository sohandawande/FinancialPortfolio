import { LogLevel } from '../../enums/logs/log-level.enum';

export class LogLevelHelper {
  static toString(level: LogLevel): string {
    return LogLevel[level];
  }

  static fromString(value: string): LogLevel {
    const key = value as keyof typeof LogLevel;
    return LogLevel[key] ?? LogLevel.Error;
  }

  static getOptions() {
    return [
      { value: LogLevel.Information, label: 'Information' },
      { value: LogLevel.Warning,     label: 'Warning' },
      { value: LogLevel.Error,       label: 'Error' },
      { value: LogLevel.Critical,    label: 'Critical' },
      { value: LogLevel.Audit,       label: 'Audit' },
      { value: LogLevel.Security,    label: 'Security' },
    ];
  }
}