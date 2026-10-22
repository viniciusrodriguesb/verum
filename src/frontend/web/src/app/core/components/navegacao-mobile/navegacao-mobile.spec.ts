import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { NavegacaoMobileComponent } from './navegacao-mobile';

describe('NavegacaoMobileComponent', () => {
  let component: NavegacaoMobileComponent;

  let fixture: ComponentFixture<NavegacaoMobileComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      providers: [provideRouter([])],
      imports: [NavegacaoMobileComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(NavegacaoMobileComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('oferece destinos distintos sem histórico desabilitado ou redirecionamento redundante', () => {
    fixture.detectChanges();

    const links = Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('a'));

    expect(links.map((link) => link.getAttribute('href'))).toEqual(['/', '/radar', '/conta']);

    expect(links.some((link) => link.hasAttribute('aria-disabled'))).toBe(false);
  });
});
