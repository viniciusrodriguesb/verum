import { AUTENTICACAO_CONFIG } from '@core/autenticacao/configuracoes/autenticacao.config';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Header } from './header';

describe('Header', () => {
  let component: Header;

  let fixture: ComponentFixture<Header>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [
        {
          provide: AUTENTICACAO_CONFIG,
          useValue: { url: 'https://login.example.test', realm: 'teste', clientId: 'teste-web' },
        },
        provideRouter([]),
      ],
      imports: [Header],
    }).compileComponents();

    fixture = TestBed.createComponent(Header);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
