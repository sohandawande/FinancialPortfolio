import {
  Component,
  input,
  output,
  signal,
  effect,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime } from 'rxjs';

@Component({
  selector: 'app-data-search-box',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './data-search-box.html',
  styleUrl: './data-search-box.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataSearchBox {
  readonly placeholder = input('Search...');
  readonly value = input('');

  readonly searchChange = output<string>();

  readonly localValue = signal('');

  private readonly subject = new Subject<string>();

  constructor() {
    effect(() => {
      this.localValue.set(this.value());
    });

    this.subject.pipe(debounceTime(400)).subscribe((v) => {
      this.searchChange.emit(v.trim());
    });
  }

  onInput(value: string): void {
    this.localValue.set(value);
    this.subject.next(value);
  }

  clear(): void {
    this.localValue.set('');
    this.searchChange.emit('');
  }
}