import { ComponentFixture, TestBed } from '@angular/core';
import { DocumentEditorComponent } from './document-editor.component';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

describe('DocumentEditorComponent', () => {
  let component: DocumentEditorComponent;
  let fixture: ComponentFixture<DocumentEditorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DocumentEditorComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(DocumentEditorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
