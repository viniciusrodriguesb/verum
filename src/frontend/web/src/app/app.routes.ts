import { Routes } from '@angular/router';
import { autenticadoGuard } from './core/autenticacao/guards/autenticado.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    title: 'Verum — Encontre o preço justo',
    loadComponent: () =>
      import('./domain/busca/inicio/inicio').then((componente) => componente.InicioComponent),
  },
  {
    path: 'busca/processamento',
    loadComponent: () =>
      import('./domain/busca/processamento/processamento').then(
        (componente) => componente.ProcessamentoComponent,
      ),
  },
  {
    path: 'busca/resultado',
    loadComponent: () =>
      import('./domain/busca/resultados/resultados').then(
        (componente) => componente.ResultadosComponent,
      ),
  },
  {
    path: 'radar',
    canActivate: [autenticadoGuard],
    loadChildren: () => import('@domain/radar/radar.routes').then((rotas) => rotas.ROTAS_RADAR),
  },
  { path: 'busca', pathMatch: 'full', redirectTo: '' },
  { path: '**', redirectTo: '' },
];
