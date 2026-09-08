import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NavegacaoMobile } from './navegacao-mobile';

describe('NavegacaoMobile', () => {
  let component: NavegacaoMobile;
  let fixture: ComponentFixture<NavegacaoMobile>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NavegacaoMobile],
    }).compileComponents();

    fixture = TestBed.createComponent(NavegacaoMobile);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
