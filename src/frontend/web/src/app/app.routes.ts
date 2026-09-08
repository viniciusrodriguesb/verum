import { Routes } from '@angular/router';

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
];
