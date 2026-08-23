import { FilterOperator } from '../../models/query/filter-operator.enum';
import { FilterRequest, QueryRequest } from '../../models/query/query-request.model';
import { SortDirection } from '../../models/query/sort-direction.enum';

export function applyClientQuery<T extends object>(
  rows: T[],
  query: QueryRequest,
  searchKeys: string[] = [],
): { page: T[]; total: number } {
  let list = [...rows];
  const term = (query.globalSearch || '').trim().toLowerCase();

  if (term && searchKeys.length) {
    list = list.filter((row) =>
      searchKeys.some((key) =>
        String((row as Record<string, unknown>)[key] ?? '')
          .toLowerCase()
          .includes(term),
      ),
    );
  }

  if (query.filters?.length) {
    list = list.filter((row) => query.filters!.every((filter) => matchesFilter(row, filter)));
  }

  const sorts = query.sorts;
  if (sorts?.length) {
    const { field, direction } = sorts[0];
    const dir = direction === SortDirection.Desc ? -1 : 1;
    list.sort((a, b) => compareValues((a as Record<string, unknown>)[field], (b as Record<string, unknown>)[field]) * dir);
  }

  const total = list.length;
  const pageSize = query.pageSize || 10;
  const maxPage = Math.max(1, Math.ceil(total / pageSize) || 1);
  const pageNumber = Math.min(query.pageNumber || 1, maxPage);
  const start = (pageNumber - 1) * pageSize;
  return { page: list.slice(start, start + pageSize), total };
}

function matchesFilter<T extends object>(row: T, filter: FilterRequest): boolean {
  const raw = (row as Record<string, unknown>)[filter.field];
  const expected = filter.value ?? '';
  const left = String(raw ?? '');
  const right = String(expected);
  const leftNum = Number(raw);
  const rightNum = Number(expected);
  const numeric = Number.isFinite(leftNum) && expected !== '' && Number.isFinite(rightNum);

  switch (filter.operator) {
    case FilterOperator.Equals:
      return numeric ? leftNum === rightNum : left.toLowerCase() === right.toLowerCase();
    case FilterOperator.NotEquals:
      return numeric ? leftNum !== rightNum : left.toLowerCase() !== right.toLowerCase();
    case FilterOperator.Contains:
      return left.toLowerCase().includes(right.toLowerCase());
    case FilterOperator.StartsWith:
      return left.toLowerCase().startsWith(right.toLowerCase());
    case FilterOperator.EndsWith:
      return left.toLowerCase().endsWith(right.toLowerCase());
    case FilterOperator.GreaterThan:
      return numeric && leftNum > rightNum;
    case FilterOperator.GreaterThanOrEqual:
      return numeric && leftNum >= rightNum;
    case FilterOperator.LessThan:
      return numeric && leftNum < rightNum;
    case FilterOperator.LessThanOrEqual:
      return numeric && leftNum <= rightNum;
    default:
      return true;
  }
}

function compareValues(av: unknown, bv: unknown): number {
  if (typeof av === 'number' && typeof bv === 'number') return av - bv;
  if (av instanceof Date && bv instanceof Date) return av.getTime() - bv.getTime();
  return String(av ?? '').localeCompare(String(bv ?? ''), undefined, { numeric: true });
}
