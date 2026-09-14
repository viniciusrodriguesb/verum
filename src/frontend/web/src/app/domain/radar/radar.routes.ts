import { Routes } from '@angular/router';

export const ROTAS_RADAR: Routes = [
  {
    path: '',
    title: 'Meus radares | Verum Radar',
    loadComponent: () =>
      import('./meus-radares/meus-radares').then((componente) => componente.MeusRadaresComponent),
  },
];
