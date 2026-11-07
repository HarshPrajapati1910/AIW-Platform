import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AnalysisDrawerComponent } from './analysis-drawer.component';

describe('AnalysisDrawerComponent', () => {
  let component: AnalysisDrawerComponent;
  let fixture: ComponentFixture<AnalysisDrawerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AnalysisDrawerComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(AnalysisDrawerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
