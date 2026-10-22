import { Directive, ElementRef, forwardRef, inject } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { valorMonetario } from '../utils/valor-monetario';

@Directive({
  selector: 'input[verumValorBrl]',
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => ValorBrlDirective), multi: true },
  ],
  host: { '(input)': 'alterar($event)', '(blur)': 'tocar()', inputmode: 'decimal' },
})
export class ValorBrlDirective implements ControlValueAccessor {
  private readonly elemento = inject<ElementRef<HTMLInputElement>>(ElementRef);

  private aoAlterar: (valor: number | null) => void = () => undefined;

  protected tocar: () => void = () => undefined;

  writeValue(valor: number | null): void {
    this.elemento.nativeElement.value = valor === null ? '' : valor.toFixed(2).replace('.', ',');
  }

  registerOnChange(funcao: (valor: number | null) => void): void {
    this.aoAlterar = funcao;
  }

  registerOnTouched(funcao: () => void): void {
    this.tocar = funcao;
  }

  setDisabledState(desabilitado: boolean): void {
    this.elemento.nativeElement.disabled = desabilitado;
  }

  protected alterar(evento: Event): void {
    this.aoAlterar(valorMonetario((evento.target as HTMLInputElement).value));
  }
}
