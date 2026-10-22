import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Header } from '@core/components/header/header';

@Component({
  selector: 'verum-institucional',
  imports: [Header, RouterLink],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <verum-header />
    <main class="mx-auto max-w-3xl space-y-6 px-5 py-10 pb-28 text-slate-800">
      <a routerLink="/" class="text-blue-700 underline">Voltar para a busca</a>
      @switch (pagina) {
        @case ('ranking') {
          <h1 class="text-3xl font-semibold">Como funciona o ranking</h1>
          <p>
            Primeiro verificamos a correspondência com sua consulta. Depois ordenamos as ofertas
            considerando preço, qualidade da fonte e atualização. Frete não participa da comparação.
          </p>
          <p>
            As notas vão de zero a dez. Quando faltam ofertas comparáveis ou histórico, a nota de
            preço é provisória. Os detalhes de cada card mostram a amostra e a confiança do cálculo.
          </p>
          <p>
            Uma fonte conhecida não confirma a reputação de um vendedor de marketplace. Confira
            vendedor, condição, preço e disponibilidade na loja antes de comprar.
          </p>
        }
        @case ('contato') {
          <h1 class="text-3xl font-semibold">Contato</h1>
          <p role="status">
            Página de demonstração. O canal oficial de atendimento ainda não foi definido.
          </p>
          <p>Exemplo fictício, sem atendimento ativo: contato&#64;verum.example.</p>
        }
        @case ('termos') {
          <h1 class="text-3xl font-semibold">Termos de uso</h1>
          <p role="status">
            Página de demonstração. Os termos oficiais ainda serão publicados. Este conteúdo não
            constitui um documento jurídico vigente.
          </p>
          <p>
            O Verum apresenta ofertas de terceiros. A compra ocorre na loja escolhida, onde devem
            ser confirmados preço, disponibilidade e condições.
          </p>
        }
        @default {
          <h1 class="text-3xl font-semibold">Privacidade</h1>
          <p role="status">
            Página de demonstração. A política oficial de privacidade ainda será publicada. Este
            conteúdo não constitui uma política vigente.
          </p>
          <p>
            Os detalhes sobre tratamento de dados e canais para exercício de direitos precisam ser
            definidos antes da publicação comercial.
          </p>
        }
      }
    </main>
  `,
})
export class InstitucionalComponent {
  protected readonly pagina = inject(ActivatedRoute).snapshot.paramMap.get('pagina');
}
