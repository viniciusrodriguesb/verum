import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AvisoApiComponent } from './shared/components/aviso-api/aviso-api';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [RouterOutlet, AvisoApiComponent],
  selector: 'verum-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {}
