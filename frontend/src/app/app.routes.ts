import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { DocumentListComponent } from './features/documents/pages/document-list/document-list.component';
import { DocumentEditorComponent } from './features/documents/pages/document-editor/document-editor.component';

export const routes: Routes = [
  // { path: '', redirectTo: 'documents', pathMatch: 'full' },
  // {
  //   path: 'documents',
  //   loadComponent: () =>
  //     import('./features/documents/pages/document-list/document-list.component').then(
  //       (m) => m.DocumentListComponent
  //     )
  // },
  // {
  //   path: 'documents/new',
  //   loadComponent: () =>
  //     import('./features/documents/pages/document-editor/document-editor.component').then(
  //       (m) => m.DocumentEditorComponent
  //     )
  // },
  // {
  //   path: 'documents/:id',
  //   loadComponent: () =>
  //     import('./features/documents/pages/document-editor/document-editor.component').then(
  //       (m) => m.DocumentEditorComponent
  //     )
  // }
  { path: 'login', component: LoginComponent },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'documents', pathMatch: 'full' },
      { path: 'documents', component: DocumentListComponent },
      { path: 'documents/new', component: DocumentEditorComponent },
      { path: 'documents/:id', component: DocumentEditorComponent }
    ]
  },
  { path: '**', redirectTo: 'documents' }
];