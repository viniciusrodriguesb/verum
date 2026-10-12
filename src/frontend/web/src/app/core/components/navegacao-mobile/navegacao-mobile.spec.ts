import { provideRouter } from '@angular/router';
import { ComponentFixture, TestBed } from '@angular/core/testing';
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
});
